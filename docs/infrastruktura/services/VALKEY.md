# Valkey

## Cel i zakres

Valkey obsługuje cache, krótkotrwały stan koordynacyjny, rate limiting, SignalR backplane i opcjonalnie stan klastra LiveKit. Nie jest źródłem prawdy dla zadań, approval, audytu ani skutków biznesowych. Poprawność biznesową zapewniają PostgreSQL, Temporal i idempotency keys.

## Wybór i licencja

Valkey jest projektem Linux Foundation pod BSD-3-Clause. Zastępuje Redis, którego linie 7.4–7.8 używają RSAL/SSPL, a 8+ dodaje AGPL. Użycie protokołu zgodnego z Redis nie wymaga uruchamiania Redis Server.

Źródła: [Valkey](https://valkey.io/topics/introduction/), [security](https://valkey.io/topics/security/), [Redis licensing](https://redis.io/legal/licenses/).

## Wersja i pinning

- Początkowa linia testowa: Valkey `9.1.x`.
- Oficjalny obraz `valkey/valkey`, przypięty po digest.
- Nie używamy Redis Insight.
- Klient .NET musi przejść testy zgodności z Valkey; można użyć kompatybilnego klienta protokołu po sprawdzeniu jego licencji.

## Topologie

### Development

- pojedyncza instancja;
- port opcjonalnie na `127.0.0.1`;
- persistence zależne od testu, zwykle wyłączone dla czystego cache;
- dane mogą być odtwarzane.

### Hardened single-node

- pojedyncza instancja na prywatnej sieci;
- ACL, TLS przy komunikacji poza hostem;
- jawne `maxmemory` i eviction policy;
- AOF/RDB tylko dla use case, który toleruje ich semantics;
- monitoring memory fragmentation, evictions, blocked clients i latency.

### HA

- Sentinel dla failover niewielkiej liczby instancji albo Valkey Cluster dla shardingu;
- węzły na osobnych failure domains;
- klient ma testowany reconnect/failover;
- dane nadal są traktowane jako odtwarzalne lub nietrwałe;
- quorum na jednym hoście nie jest HA.

## Zasoby startowe

| Środowisko | CPU | RAM | Dysk |
|---|---:|---:|---|
| dev | 0.25–1 | 256–512 MiB | brak lub 1 GiB |
| single-node prod | 1–2 | 1–4 GiB | zależnie od AOF/RDB |
| HA | od 1 na węzeł | dataset + co najmniej 30% zapasu | osobny trwały dysk, jeśli persistence |

`maxmemory` musi być niższe niż limit kontenera, aby serwer miał miejsce na buffers, fork i fragmentację.

## Podział danych

Nie polegamy na numerach logical database jako granicy bezpieczeństwa. Używamy:

- osobnych ACL users;
- prefiksów kluczy, na przykład `praxiara:signalr:`, `praxiara:ratelimit:`;
- w razie wyższego ryzyka osobnych instancji dla LiveKit i aplikacji;
- TTL na każdym kluczu sesyjnym/lock/cache, jeśli nie jest trwałą konfiguracją.

Rozproszony lock w Valkey nie zastępuje constraintu/transakcji PostgreSQL dla operacji biznesowej.

## Sekrety i konfiguracja

Oczekiwane:

```text
/run/secrets/valkey_app_password
/run/secrets/valkey_livekit_password
```

Konfiguracja aplikacji:

```text
PRAXIARA_VALKEY_HOST=valkey
PRAXIARA_VALKEY_PORT=6379
PRAXIARA_VALKEY_USERNAME=praxiara_app
PRAXIARA_VALKEY_PASSWORD_FILE=/run/secrets/valkey_app_password
PRAXIARA_VALKEY_TLS=true
PRAXIARA_VALKEY_KEY_PREFIX=praxiara:
```

ACL użytkownika ogranicza commands i key patterns do potrzebnego zakresu. Aplikacja nie otrzymuje `default`/admin user.

## Sieć i porty

- 6379/TCP tylko `data-net`.
- Dev mapping wyłącznie `127.0.0.1`.
- Nigdy publicznie; oficjalna dokumentacja podkreśla model trusted clients/trusted environment.
- Browser Worker nie ma trasy do Valkey.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait valkey
```

Plik `valkey.conf` jest montowany read-only. Start bez jawnej konfiguracji jest dozwolony tylko w jednorazowym teście, nie w repozytorium docelowym.

## Health i smoke test

Healthcheck korzysta z minimalnego użytkownika albo lokalnego socketu i sprawdza `PING`.

```bash
docker compose exec valkey valkey-cli PING
```

Smoke test runtime role:

```bash
valkey-cli \
  --tls \
  --user praxiara_app \
  --pass "$(cat /run/secrets/valkey_app_password)" \
  SET praxiara:smoke:test ok EX 30
```

Należy potwierdzić TTL, odczyt i brak dostępu do klucza poza prefiksem. Polecenie z hasłem jest wykonywane wyłącznie wewnątrz kontrolowanego kontenera; w logach CI należy wyłączyć echo.

## Persistence, backup i restore

### Cache-only

Persistence jest wyłączone, a runbook opisuje odtworzenie przez restart i warming. Backup nie jest potrzebny.

### Backplane/stan odtwarzalny

Można użyć RDB dla szybszego powrotu, ale utrata ostatnich wpisów musi być akceptowalna.

### Wymagana większa trwałość

AOF `everysec` ogranicza utratę, lecz nadal nie czyni Valkey systemem transakcyjnym dla audytu. Snapshot/AOF są kopiowane spójnie zgodnie z dokumentacją i okresowo testowane. Restore wykonuje się na izolowanej instancji, nigdy przez podmianę plików działającego serwera.

Źródło: [Valkey persistence](https://valkey.io/topics/persistence/).

## Aktualizacja i rollback

1. Przeczytaj release notes i compatibility notes klienta.
2. W HA zaktualizuj replica, przetestuj, wykonaj controlled failover, potem stary primary.
3. W single-node przygotuj RDB/AOF lub zaakceptuj cold cache.
4. Sprawdź ACL, modules, latency i memory po aktualizacji.
5. Rollback wymaga zgodności formatu persistence; w cache-only bezpieczniej uruchomić pustą starą instancję.

Nie instalujemy modułów bez osobnego przeglądu licencji i kompatybilności.

## Hardening

- wyłączony lub zabezpieczony `default` user;
- ACL dla każdej aplikacji;
- TLS poza jednym prywatnym hostem;
- `protected-mode yes`;
- bind do właściwego interfejsu/sieci;
- jawne `maxmemory` i eviction policy;
- niebezpieczne admin commands ograniczone;
- limity klientów i timeouts;
- slowlog/latency monitoring bez ujawniania wartości;
- brak publicznego portu;
- brak trwałych sekretów w config file w repo.

## Troubleshooting

### OOM albo evictions

```bash
valkey-cli INFO memory
valkey-cli INFO stats
```

Sprawdź różnicę `used_memory`, `used_memory_rss`, limit kontenera i politykę eviction. Nie zwiększaj limitu bez ustalenia prefiksu/TTL powodującego wzrost.

### Latency

```bash
valkey-cli SLOWLOG GET 20
valkey-cli LATENCY DOCTOR
```

Sprawdź duże klucze, blokujące polecenia, fork snapshotu i presję CPU.

### Klienci nie wracają po restarcie

Zweryfikuj DNS caching, retry/backoff, connection multiplexer i health. Test failover klienta jest wymaganym testem kontraktowym.

### Dane nie powinny być w Valkey

Jeśli utrata klucza uniemożliwia odtworzenie decyzji, approval lub audytu, dane należy przenieść do PostgreSQL/Temporal, nie „naprawiać” większą liczbą kopii Valkey.

## Różnice produkcyjne

Produkcja wymaga ACL, limitów, prywatnej sieci, telemetryki i świadomej polityki persistence. HA wymaga osobnych hostów i testowanego klienta. Dev może używać pustej instancji po każdym restarcie.

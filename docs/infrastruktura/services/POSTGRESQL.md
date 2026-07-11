# PostgreSQL

## Cel i zakres

PostgreSQL przechowuje stan aplikacji, konfiguracje skill, polityki, metadane audytu i trwałe dane orkiestracji. Keycloak i Temporal korzystają z tego samego klastra tylko w małych instalacjach, ale zawsze z osobnych baz i ról. Duże wdrożenie może rozdzielić je na osobne klastry bez zmiany kontraktu aplikacji.

Obiekty binarne, screenshoty, Playwright traces i pobrania nie trafiają do PostgreSQL; przechowuje się ich identyfikatory, hash, klasyfikację i retencję, a treść w S3.

## Wybór i licencja

PostgreSQL jest dostępny pod liberalną PostgreSQL License, bez opłat także dla produktu komercyjnego. Linia 18 jest wspierana do listopada 2030; należy zawsze używać aktualnego minor release.

Źródła: [licencja](https://www.postgresql.org/about/licence/), [polityka wersji](https://www.postgresql.org/support/versioning/), [dokumentacja 18](https://www.postgresql.org/docs/18/).

## Wersja i pinning

- Początkowa linia testowa: PostgreSQL `18.4`.
- Obraz: oficjalny `postgres`, wariant Debian, przypięty po digest.
- Nie zmieniamy Alpine/Debian ani libc w tym samym change co major/minor bazy.
- `PGDATA` wskazuje jawny podkatalog wolumenu.
- Włączone rozszerzenia są zapisane w migracji i rejestrze licencji. Preferowane: `pgcrypto`, `citext`, `pg_trgm`; `vector` tylko po zatwierdzeniu konkretnej potrzeby.

## Topologie

### Development/CI

- jedna instancja;
- wolumen nazwany lub tmpfs w krótkich testach;
- port 5432 opcjonalnie na `127.0.0.1`;
- hasła losowe generowane przed startem, nie stałe wartości demonstracyjne;
- backup nie jest wymagany dla danych odtwarzalnych, ale test backup/restore działa w CI okresowym.

### Hardened single-node

- jedna instancja na SSD/NVMe z checksums;
- szyfrowanie dysku;
- pgBackRest i ciągły WAL do oddzielnego repozytorium S3;
- brak host portu 5432;
- TLS lub wyłącznie `data-net` na jednym hoście;
- monitoring pojemności, WAL, locks, connections, replication lag jeśli standby istnieje.

### Multi-node/HA

- co najmniej trzy węzły na osobnych failure domains;
- streaming replication i automatyczny failover przez Patroni/etcd albo równoważny, wcześniej zaakceptowany stos;
- load-balancer rozróżnia read-write i read-only;
- synchronous replication tylko po analizie latency/RPO;
- backup pozostaje wymagany: replikacja nie chroni przed błędem użytkownika ani korupcją logiczną.

Patroni jest MIT, a etcd Apache-2.0. Compose na jednym hoście nie realizuje tej topologii.

## Zasoby startowe

| Środowisko | CPU | RAM | Dysk |
|---|---:|---:|---|
| dev | 1–2 | 1–2 GiB | 10–30 GiB |
| single-node prod | 2–4 | 4–8 GiB | SSD od 100 GiB + zapas WAL |
| HA | od 4 na węzeł | od 8 GiB | osobny szybki dysk danych i WAL według benchmarku |

To wartości startowe. Pojemność wynika z liczby sesji, historii Temporal, audytu, retencji i backup window. Utrzymujemy co najmniej 25% wolnego miejsca oraz alerty wcześniej niż krytyczny próg.

## Bazy i role

Init job tworzy idempotentnie:

```text
praxiara              owner: praxiara_owner, runtime: praxiara_app
keycloak              owner/runtime: keycloak_app
temporal              owner/runtime: temporal_app
temporal_visibility   owner/runtime: temporal_app
```

Zasady:

- runtime role nie jest superuserem i nie tworzy baz;
- migrator ma osobną, krótkotrwałą rolę;
- Keycloak i Temporal nie mają dostępu do `praxiara`;
- aplikacja nie ma dostępu do baz Keycloak/Temporal;
- backup role ma tylko wymagane prawa;
- role nie współdzielą hasła.

SQL inicjalizacyjny nie zawiera haseł w repo. Hasła są przekazywane przez bezpieczny kanał i używane przez `psql` z pliku `.pgpass` o uprawnieniach `0600` albo przez OpenBao dynamic database credentials.

## Sekrety i konfiguracja

Oczekiwane pliki secret:

```text
/run/secrets/postgres_superuser_password
/run/secrets/praxiara_db_password
/run/secrets/keycloak_db_password
/run/secrets/temporal_db_password
```

Aplikacja powinna obsługiwać konfigurację przez:

```text
PRAXIARA_POSTGRES_HOST=postgres
PRAXIARA_POSTGRES_PORT=5432
PRAXIARA_POSTGRES_DATABASE=praxiara
PRAXIARA_POSTGRES_USERNAME=praxiara_app
PRAXIARA_POSTGRES_PASSWORD_FILE=/run/secrets/praxiara_db_password
PRAXIARA_POSTGRES_SSL_MODE=Require
```

Kod konfiguracji odczytuje `_FILE`; sama platforma .NET nie nadaje temu automatycznie semantyki.

Nie zapisujemy pełnego connection stringa w logach ani telemetry attributes.

## Sieć i porty

- 5432/TCP tylko `data-net` w produkcji.
- Dev mapping, jeśli potrzebny: `127.0.0.1:5432:5432`.
- Multi-host wymaga TLS i restrykcyjnego `pg_hba.conf`.
- Browser Worker nie jest członkiem `data-net`.

## Uruchomienie

Development:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait postgres
```

Idempotentny init:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm postgres-init
```

Init job kończy się błędem przy częściowej konfiguracji zamiast próbować destrukcyjnego resetu.

## Health i smoke test

Liveness procesu:

```bash
docker compose exec postgres pg_isready -U postgres -d postgres
```

Readiness aplikacyjna wykonuje połączenie runtime role, `SELECT 1` i kontrolę oczekiwanej wersji migracji. Nie używa superusera.

Smoke test izolacji:

```bash
docker compose exec postgres \
  psql -U praxiara_app -d praxiara -v ON_ERROR_STOP=1 \
  -c 'select current_database(), current_user, now();'
```

Należy również potwierdzić, że `praxiara_app` nie może połączyć się z `keycloak` ani tworzyć rozszerzeń/roli.

## Backup

Docelowo pgBackRest:

- pełny backup co najmniej tygodniowo;
- differential/incremental zgodnie z RPO;
- ciągłe archiwizowanie WAL;
- szyfrowanie repozytorium;
- repozytorium na innym systemie/failure domain;
- retencja zgodna z audytem i wymaganiami prawnymi;
- regularne `check` i `verify`.

Przykładowe operacje w dedykowanym jobie:

```bash
pgbackrest --stanza=praxiara check
pgbackrest --stanza=praxiara --type=full backup
pgbackrest --stanza=praxiara info
```

Źródło: [pgBackRest](https://github.com/pgbackrest/pgbackrest).

## Restore i PITR

Restore zawsze testuje się na pustym wolumenie i innym porcie/hoście:

1. Zatrzymaj docelową instancję testową.
2. Zweryfikuj, że katalog danych jest pusty.
3. Wykonaj restore do wskazanego czasu lub latest.
4. Uruchom PostgreSQL bez dostępu aplikacji.
5. Zweryfikuj checksums, migracje, liczby rekordów i spójność audytu.
6. Dopiero potem zezwól na ruch.

Przykład składni do dostosowania:

```bash
pgbackrest \
  --stanza=praxiara \
  --type=time \
  --target='2026-07-11 10:00:00+00' \
  --target-action=promote \
  restore
```

RPO/RTO jest uznane za spełnione dopiero po pomiarze pełnego ćwiczenia.

## Aktualizacja i rollback

Minor:

1. backup i test restore;
2. stop ruchu/write drain, jeśli wymagane;
3. aktualizacja obrazu po digest;
4. start i kontrola release notes;
5. smoke test i obserwacja.

Major:

- osobny projekt migracyjny;
- `pg_upgrade`, logical replication albo dump/restore;
- test na kopii danych o zbliżonej skali;
- plan czasu i miejsca na dysku;
- jawna decyzja rollback przed zmianą formatu danych.

Rollback aplikacji jest możliwy tylko, jeśli migracje zachowują kompatybilność `N-1`. Rollback bazy zwykle oznacza restore lub przełączenie na niezmieniony stary klaster.

## Hardening

- SCRAM-SHA-256, nie MD5.
- TLS między hostami i pełna weryfikacja certyfikatu.
- minimalne `pg_hba.conf`.
- superuser tylko do operacji administracyjnych.
- `log_connections`, `log_disconnections` i rozsądne `log_min_duration_statement`, bez danych wrażliwych.
- limity connection pools; unikamy lawiny połączeń przy restarcie.
- statement/lock/idle-in-transaction timeout.
- checksums i monitorowanie autovacuum, bloat, locks oraz replication lag.
- brak publicznego portu i brak dostępu Browser Workera.

## Troubleshooting

### Brak połączeń

Sprawdź readiness, `pg_hba.conf`, DNS, TLS i liczbę połączeń:

```sql
select state, count(*) from pg_stat_activity group by state;
```

### Długie blokady

```sql
select pid, wait_event_type, wait_event, query_start, state
from pg_stat_activity
where wait_event is not null;
```

Nie terminuj procesu bez rozpoznania transakcji i wpływu biznesowego.

### Rosnący dysk

Sprawdź WAL archiving, replication slots, temp files, bloat i retencję Temporal/audit. Nigdy nie usuwaj ręcznie plików z `PGDATA` ani `pg_wal`.

### Uszkodzony backup

Zablokuj expiry zdrowych starszych backupów, zabezpiecz logi i wykonaj restore z ostatniego zweryfikowanego punktu. Backup bez udanego testu odtworzenia nie jest uznawany za poprawny.

## Różnice produkcyjne

Produkcja wymaga trwałych wolumenów, TLS, osobnych ról, backupu/PITR, alertów, testów restore i zakazu host portu. Multi-node dodatkowo wymaga osobnych hostów, quorum/failover i operacyjnego runbooka. Żadnego z tych wymagań nie spełnia samo dodanie `restart: always`.

# OpenBao, SOPS i age

## Cel i zakres

OpenBao przechowuje i wydaje sekrety runtime: credentials do baz/S3, client secrets, klucze providerów LLM, klucze envelope encryption i krótkie tokeny usług. SOPS+age szyfruje minimalny bootstrap potrzebny do uruchomienia środowiska.

OpenBao nie przechowuje dużych plików, screenshotów ani browser storage state. Storage state jest szyfrowany kluczem zarządzanym przez aplikację/OpenBao, a ciphertext znajduje się w S3 z krótkim TTL.

## Wybór i licencje

- OpenBao: MPL-2.0, weak copyleft na poziomie pliku.
- SOPS: MPL-2.0.
- age: BSD-3-Clause.

OpenBao zastępuje HashiCorp Vault, którego nowsze wydania są BSL 1.1.

Źródła: [OpenBao](https://github.com/openbao/openbao), [OpenBao docs](https://openbao.org/docs/), [SOPS](https://github.com/getsops/sops), [age](https://github.com/FiloSottile/age), [zmiana HashiCorp](https://www.hashicorp.com/blog/hashicorp-adopts-business-source-license).

## Wersja i pinning

- Początkowa linia: aktualne patch release OpenBao `2.5.x`.
- Obraz przypięty po digest; security releases mają priorytet.
- SOPS i age przypięte w narzędziach developerskich/CI.
- Pluginów OpenBao nie pobieramy dynamicznie bez allowlisty, checksum i przeglądu licencji.

## Topologie

### Development

- można użyć dev mode tylko lokalnie, z jawnym ostrzeżeniem i nietrwałymi danymi;
- dev root token nie może być wspólną stałą w repo;
- preferowany single-node Raft, gdy testujemy realny seal/unseal i policies;
- port 8200 tylko `127.0.0.1`.

### Hardened single-node

- single-node Raft, TLS, manual Shamir unseal;
- szyfrowany trwały wolumen;
- snapshot Raft do off-host backupu;
- nie jest HA i wymaga runbooka restart/unseal.

### HA

- 3 węzły minimum, 5 rekomendowane dla tolerancji dwóch awarii;
- osobne failure domains i Integrated Storage Raft;
- TLS API i mTLS cluster;
- load balancer kieruje do aktywnego/standby zgodnie z health;
- recovery/unseal shares rozdzielone między opiekunów;
- auto-unseal tylko z zaakceptowanym zewnętrznym KMS lub oddzielnym root-of-trust.

Oficjalna dokumentacja rekomenduje pięć serwerów dla produkcyjnego Raft. [Integrated Storage](https://openbao.org/docs/internals/integrated-storage/).

## Zasoby startowe

| Środowisko | CPU | RAM | Dysk |
|---|---:|---:|---|
| dev | 0.5–1 | 256–512 MiB | 1–5 GiB |
| single-node prod | 1–2 | 1–2 GiB | SSD 10+ GiB |
| HA | 2 na węzeł | 2–4 GiB | szybki SSD per node |

OpenBao jest wrażliwy na latency i stabilność dysku. Nie używamy burstable storage dla produkcyjnego Raft.

## Bootstrap SOPS+age

W Git mogą znajdować się wyłącznie zaszyfrowane pliki, na przykład:

```text
deploy/compose/secrets/bootstrap.prod.sops.yaml
```

`.sops.yaml` określa recipients bez prywatnych kluczy. Prywatny klucz age jest poza repo, najlepiej na sprzętowym lub kontrolowanym nośniku operatora.

Odszyfrowanie do tymczasowego filesystemu:

```bash
umask 077
mkdir -p /run/user/$(id -u)/praxiara-secrets
sops --decrypt \
  deploy/compose/secrets/bootstrap.prod.sops.yaml \
  > /run/user/$(id -u)/praxiara-secrets/bootstrap.yaml
```

Nie odszyfrowujemy do katalogu repo. Plik jest bezpiecznie usuwany po bootstrapie, a jego zawartość nie trafia do shell history ani CI logs.

## Inicjalizacja i unseal

Produkcja:

1. Uruchom pusty OpenBao z TLS i Raft.
2. Wykonaj `bao operator init` w kontrolowanym terminalu.
3. Rozdziel unseal/recovery shares między różnych opiekunów.
4. Zabezpiecz initial root token i użyj go wyłącznie do utworzenia admin auth/policies.
5. Włącz audit device przed przyjęciem sekretów.
6. Utwórz auth methods, policies i secrets engines idempotentnym bootstrapem.
7. Unieważnij/odłóż root token zgodnie z procedurą.

Nigdy nie zapisuj pełnego output `operator init` w zwykłym pliku lub logu sesji.

## Namespaces, paths i policies

Przykładowe paths:

```text
kv/praxiara/<environment>/database
kv/praxiara/<environment>/s3
kv/praxiara/<environment>/identity
kv/praxiara/<environment>/llm
transit/praxiara-session
database/creds/praxiara-app
pki/issue/praxiara-service
```

Każda usługa otrzymuje policy tylko dla swoich paths/actions. Browser Worker nie otrzymuje tokenu OpenBao. Koordynator wydaje mu wyłącznie krótkotrwały, scope-specific materiał potrzebny do sesji.

## Auth aplikacji

Preferencje:

- workload identity/JWT w orkiestratorze;
- AppRole tylko tam, gdzie workload identity nie jest dostępne;
- krótki TTL i renewable token;
- response wrapping dla pierwszego sekretu;
- periodic renewal monitorowany;
- revocation przy zakończeniu workera.

Token nie jest przekazywany przez command line ani logowany.

## Sekrety i konfiguracja klienta

Minimalny bootstrap klienta może wskazywać:

```text
PRAXIARA_OPENBAO_ADDRESS=https://openbao:8200
PRAXIARA_OPENBAO_NAMESPACE=praxiara
PRAXIARA_OPENBAO_AUTH_METHOD=jwt
PRAXIARA_OPENBAO_ROLE=praxiara-api
PRAXIARA_OPENBAO_CA_CERT_FILE=/run/secrets/openbao_ca
```

Stały token w env jest zabroniony w produkcji. Aplikacja odnawia lease z jitter, kończy readiness przy trwałej utracie wymaganych sekretów i nie używa starego sekretu po revocation.

## Sieci i porty

- 8200/TCP API tylko `control-net`, TLS.
- 8201/TCP cluster tylko między węzłami, mTLS.
- Dev mapping `127.0.0.1:8200`.
- UI administracyjne wyłącznie przez VPN/role operatorskie albo w ogóle niewystawione.
- Browser Worker nie ma trasy do OpenBao.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile secrets \
  up -d --wait openbao
```

Bootstrap policies:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm openbao-bootstrap
```

Bootstrap jest idempotentny i nie nadpisuje istniejącego sekretu losową wartością. Rozbieżność policy jest widoczna w diff/audit.

## Health i smoke test

```bash
bao status -address=https://openbao:8200
```

Smoke test minimalnej roli:

1. Uwierzytelnij workload testowy.
2. Odczytaj dozwolony test secret.
3. Potwierdź odmowę odczytu ścieżki innej usługi.
4. Odnów lease.
5. Unieważnij token/lease.
6. Potwierdź, że dalszy odczyt jest zabroniony.

Health interpretuje stan sealed/standby/active zgodnie z rolą węzła; sam HTTP 200 nie jest wystarczający.

## Backup i restore

Backup Integrated Storage:

```bash
bao operator raft snapshot save /secure-backup/openbao-$(date +%Y%m%dT%H%M%SZ).snap
```

Snapshot jest zaszyfrowany barierą OpenBao, ale nadal jest materiałem wrażliwym. Przechowujemy go off-host wraz z konfiguracją TLS i informacją o wersji, lecz recovery shares osobno.

Restore wykonuje się w odizolowanym klastrze zgodnie z oficjalną procedurą:

1. Zabezpiecz aktualny stan i zatrzymaj zapisy.
2. Uruchom zgodną wersję OpenBao.
3. Odtwórz snapshot Raft.
4. Unseal uprawnionymi shares.
5. Zweryfikuj auth, policies, mounts, lease i audit device.
6. Rotuj krytyczne sekrety, jeśli snapshot był poza standardową kontrolą.

Źródło: [OpenBao storage and backup concepts](https://openbao.org/docs/concepts/storage/).

## Aktualizacja i rollback

- Aktualizujemy najpierw standby, zgodnie z release notes.
- Przed zmianą snapshot i test restore.
- Sprawdzamy seal, storage migration i plugin compatibility.
- Po każdym węźle kontrolujemy leader, Raft peers, audit i lease renewal.
- Rollback jest dozwolony tylko przy potwierdzonej kompatybilności; inaczej odtwarzamy snapshot do starego klastra.
- Security release OpenBao ma wysoki priorytet, ponieważ usługa przechowuje root secrets.

## Hardening

- TLS wszędzie, mTLS cluster.
- audit device włączony przed użyciem.
- least-privilege policies i krótkie TTL.
- brak root token w runtime.
- manual unseal shares poza hostem i rozdzielone.
- integrated Raft na szybkim dysku.
- UI/admin dostęp ograniczony.
- tokeny i sekrety redagowane w logach/OTEL.
- plugin catalog i OCI downloads domyślnie wyłączone lub allowlistowane.
- regularna rotacja DB/S3/OIDC/LLM keys.
- alerty na sealed node, leader loss, audit failure, lease renewal i cert expiry.

## Troubleshooting

### Sealed po restarcie

To oczekiwane przy Shamir bez auto-unseal. Uruchom kontrolowaną procedurę z wymaganym quorum opiekunów. Nie zapisuj shares w Compose ani systemd unit.

### Brak quorum Raft

Nie usuwaj losowo peerów. Zabezpiecz snapshot/logi, ustal żywe węzły i skorzystaj z oficjalnej procedury recovery. Dwa węzły nie zapewniają sensownej tolerancji awarii.

### 403 dla aplikacji

Sprawdź token accessor, policy path, namespace, auth role i expiry. Nie rozszerzaj policy do `*` jako szybkiej naprawy.

### Lease wygasa podczas zadania

Sprawdź renewal loop, clock, network, max TTL i revocation. Aktywność biznesowa powinna bezpiecznie się zatrzymać zamiast użyć nieautoryzowanego sekretu.

### SOPS nie może odszyfrować

Zweryfikuj recipient, klucz age i `.sops.yaml`. Nie zastępuj zaszyfrowanego pliku plaintextem „na chwilę”.

## Różnice produkcyjne

Dev mode jest nietrwały i nie reprezentuje bezpieczeństwa produkcji. Produkcja wymaga Raft, TLS, audit, seal/unseal, backup, rotacji i least privilege. Prawdziwe HA wymaga 3–5 osobnych hostów; single-node zachowuje pełną funkcjonalność, ale nie dostępność podczas awarii.

# Profile Docker Compose

## Cel

Profile w istniejącym `deploy/compose` włączają opcjonalne możliwości. Nie kodują środowiska. Różnice development/production są realizowane przez pliki override, dzięki czemu ta sama nazwa usługi i te same zależności mogą być testowane w obu środowiskach.

## Układ plików

```text
deploy/compose/
├── .env.example
├── compose.yaml
├── compose.dev.yaml
├── versions.env
├── caddy/
├── jaeger/
├── otel-collector/
├── postgres/
├── prometheus/
├── seaweedfs/
├── temporal/
└── valkey/
```

Pliki `compose.prod-single-node.yaml`, `compose.gpu-*.yaml`, pełny profil obserwowalności i `images.lock.json` są elementami zakresu docelowego, ale nie istnieją jeszcze w repozytorium. Poleceń oznaczonych dalej jako docelowe nie należy wykonywać przed ich implementacją i przeglądem bezpieczeństwa.

Lokalny plik `deploy/compose/.env` jest ignorowany przez Git. Utwórz go z `.env.example`, wprowadź niepowtarzalne sekrety developerskie i nadaj mu uprawnienia dostępne wyłącznie dla właściciela. Produkcyjne sekrety będą dostarczane z OpenBao, a nie z pliku `.env`.

## Usługi bazowe

Bez profilu uruchamiane są jedynie komponenty potrzebne do minimalnej pracy aplikacji:

- `postgres`
- `valkey`
- lokalna topologia `seaweedfs`
- API/UI i wymagane procesy aplikacji, gdy zostaną dodane do Compose

Takie zachowanie umożliwia:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d
```

## Profile możliwości

| Profil | Stan | Usługi | Zastosowanie |
|---|---|---|---|
| `identity` | istnieje | `keycloak`, `keycloak-init` | lokalny OIDC i idempotentny seed administratora |
| `workflow` | istnieje | `temporal`, `temporal-ui` | trwałe zadania, approval i retry |
| `llm-local` | istnieje | `ollama` | lokalna inferencja developerska |
| `observability-lite` | istnieje | OTel Collector, Prometheus, Jaeger | lokalny debug o małym koszcie |
| `mail` | istnieje | `mailpit` | wyłącznie dev/test |
| `edge` | istnieje | `caddy` | lokalny routing; produkcyjny TLS wymaga override |
| `secrets` | planowany | `openbao` | sekrety runtime |
| `browser` | planowany | browser coordinator/worker, egress gateway | wykonanie automatyzacji |
| `stream-webrtc` | planowany | `livekit`, opcjonalnie `coturn` | docelowy podgląd i przejęcie |
| `stream-novnc` | planowany | noVNC/websockify/VNC w workerze | diagnostyczny fallback |
| `observability` | planowany | OTel, Prometheus, Alertmanager, Data Prepper, OpenSearch, Dashboards | pełna telemetria |
| `backup` | planowany | pgBackRest i kontrolne restore jobs | backup/PITR |
| `security` | planowany | one-shot Trivy, Syft, Gitleaks, ZAP | CI i lokalne skany |
| `ops` | planowany | narzędzia init/diagnostyka | jawne wywołanie przez operatora |

## Zestawy środowiskowe

Compose nie ma aliasów grup profili. Źródłem prawdy pozostaje jawna lista `COMPOSE_PROFILES`. Poniższe dwa warianty developerskie korzystają wyłącznie z już utworzonych profili.

Minimalny development:

```bash
export COMPOSE_PROFILES="identity,workflow,llm-local,mail,edge"
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait
```

Development z telemetrią:

```bash
export COMPOSE_PROFILES="identity,workflow,llm-local,mail,edge,observability-lite"
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait
```

Docelowy hardened single-node — polecenie stanie się wykonywalne dopiero po utworzeniu produkcyjnego override i brakujących profili:

```bash
export COMPOSE_PROFILES="identity,secrets,workflow,llm-local,browser,stream-webrtc,observability,edge,backup"
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.prod-single-node.yaml \
  config --quiet

docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.prod-single-node.yaml \
  up -d --wait
```

## CPU i GPU dla lokalnego LLM

Profil `llm-local` określa funkcję, a typ GPU określa override. Nie należy definiować trzech konkurencyjnych usług o tej samej roli.

CPU:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile llm-local \
  up -d ollama
```

Docelowy runtime llama.cpp z NVIDIA — wymaga planowanego override:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.gpu-nvidia.yaml \
  --profile llm-local \
  up -d llama-server
```

Docelowy runtime llama.cpp z AMD — wymaga planowanego override:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.gpu-amd.yaml \
  --profile llm-local \
  up -d llama-server
```

## Docelowe one-shot jobs

Profile `security` i `ops` nie zostały jeszcze dodane do Compose. Po ich implementacji skany i init jobs będą uruchamiane jawnie jako procesy jednorazowe:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile security \
  run --rm trivy-fs
```

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm postgres-init
```

Init job musi być idempotentny. Nie może resetować hasła, usuwać bazy, realm, bucketa ani polityki przy ponownym uruchomieniu.

## Kolejność zależności

`depends_on` nie zastępuje readiness. Każda zależność stanowa ma healthcheck, a klient dodatkowo retry z backoff i jitter.

Zalecana kolejność:

1. PostgreSQL, Valkey i SeaweedFS;
2. OpenBao i Keycloak;
3. Temporal i observability storage;
4. init jobs;
5. API/orchestrator;
6. Browser Worker, LLM i streaming;
7. Caddy;
8. smoke tests.

## Walidacja przed startem

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  config > /dev/null
```

Należy dodatkowo sprawdzić:

- brak `latest`;
- w development: dokładne, nieruchome tagi z `versions.env`; w wydaniu produkcyjnym: dodatkowo immutable digesty z lockfile;
- brak host portów dla usług danych w produkcji;
- brak bind mountu repozytorium w produkcji;
- brak `/var/run/docker.sock`;
- `read_only`, `cap_drop`, `no-new-privileges`, limity i healthcheck;
- wszystkie wymagane zmienne zakończone `_FILE` wskazują istniejące sekrety;
- profile `stream-webrtc` i `stream-novnc` są włączone razem tylko świadomie.

## Zatrzymanie i usuwanie

Zatrzymanie zachowuje wolumeny:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  down
```

`down --volumes` jest operacją destrukcyjną. Dokumentacja i skrypty nie mogą wykonywać jej bez jawnego parametru operatora oraz wcześniejszego potwierdzenia backupu.

## Ograniczenia single-node

Compose na jednym hoście może być dobrze zabezpieczonym wdrożeniem, ale awaria hosta zatrzymuje PostgreSQL, OpenBao, S3, Temporal, Keycloak i browser sessions. Prawdziwe HA wymaga osobnych failure domains, replikacji, load balancera i przetestowanych procedur failover. Nie należy nazywać HA konfiguracji składającej się z kilku kontenerów na jednym serwerze.

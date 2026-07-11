# Profile Docker Compose

## Cel

Profile w przyszłym `deploy/compose` włączają opcjonalne możliwości. Nie kodują środowiska. Różnice development/production są realizowane przez pliki override, dzięki czemu ta sama nazwa usługi i te same zależności są testowane w obu środowiskach.

## Układ plików

```text
deploy/compose/
├── compose.yaml
├── compose.dev.yaml
├── compose.prod-single-node.yaml
├── compose.observability-lite.yaml
├── compose.gpu-nvidia.yaml
├── compose.gpu-amd.yaml
├── versions.env
├── images.lock.json
├── config/
│   ├── caddy/
│   ├── keycloak/
│   ├── livekit/
│   ├── observability/
│   ├── openbao/
│   ├── temporal/
│   └── valkey/
├── init/
└── secrets/
    └── README.md
```

`deploy/compose/secrets` nie zawiera odszyfrowanych sekretów w Git. Produkcyjne sekrety są dostarczane z OpenBao; lokalne pliki sekretów są ignorowane przez Git i mają uprawnienia `0600`.

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
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d
```

## Profile możliwości

| Profil | Usługi | Zastosowanie |
|---|---|---|
| `identity` | `keycloak`, `oauth2-proxy` | logowanie aplikacji i ochrona UI operatorskich |
| `secrets` | `openbao` | sekrety runtime; lokalnie opcjonalny |
| `workflow` | `temporal`, `temporal-ui`, init schema | trwałe zadania, approval, retry |
| `llm-local` | `llama-server` albo dev `ollama` | lokalna inferencja |
| `browser` | browser coordinator/worker, egress gateway | wykonanie automatyzacji |
| `stream-webrtc` | `livekit`, opcjonalnie `coturn` | docelowy podgląd i przejęcie |
| `stream-novnc` | noVNC/websockify/VNC w workerze | diagnostyczny fallback |
| `observability` | OTel, Prometheus, Alertmanager, Data Prepper, OpenSearch, Dashboards | pełna telemetria |
| `observability-lite` | OTel, Prometheus, Jaeger memory | lokalny debug o małym koszcie |
| `mail` | `mailpit` | wyłącznie dev/test |
| `edge` | `caddy` | TLS i routing |
| `backup` | pgBackRest i kontrolne restore jobs | backup/PITR |
| `security` | one-shot Trivy, Syft, Gitleaks, ZAP | CI i lokalne skany |
| `ops` | narzędzia init/diagnostyka | jawne wywołanie przez operatora |

## Zestawy środowiskowe

Compose nie ma aliasów grup profili. Projekt powinien udostępnić wrapper lub pliki `.env.profiles.example`, lecz źródłem prawdy pozostaje jawna lista `COMPOSE_PROFILES`.

Minimalny development:

```bash
export COMPOSE_PROFILES="identity,workflow,llm-local,browser,stream-novnc,mail,edge"
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait
```

Development z telemetrią:

```bash
export COMPOSE_PROFILES="identity,workflow,llm-local,browser,stream-novnc,mail,edge,observability-lite"
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.observability-lite.yaml \
  up -d --wait
```

Hardened single-node:

```bash
export COMPOSE_PROFILES="identity,secrets,workflow,llm-local,browser,stream-webrtc,observability,edge,backup"
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.prod-single-node.yaml \
  config --quiet

docker compose \
  --env-file deploy/compose/versions.env \
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
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile llm-local \
  up -d llama-server
```

NVIDIA:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.gpu-nvidia.yaml \
  --profile llm-local \
  up -d llama-server
```

AMD:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.gpu-amd.yaml \
  --profile llm-local \
  up -d llama-server
```

## One-shot jobs

Skany i init jobs nie są długotrwałymi usługami. Uruchamia się je jawnie:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  -f deploy/compose/compose.yaml \
  --profile security \
  run --rm trivy-fs
```

```bash
docker compose \
  --env-file deploy/compose/versions.env \
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
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  config > /dev/null
```

Należy dodatkowo sprawdzić:

- brak `latest`;
- obecność digestów;
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
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  down
```

`down --volumes` jest operacją destrukcyjną. Dokumentacja i skrypty nie mogą wykonywać jej bez jawnego parametru operatora oraz wcześniejszego potwierdzenia backupu.

## Ograniczenia single-node

Compose na jednym hoście może być dobrze zabezpieczonym wdrożeniem, ale awaria hosta zatrzymuje PostgreSQL, OpenBao, S3, Temporal, Keycloak i browser sessions. Prawdziwe HA wymaga osobnych failure domains, replikacji, load balancera i przetestowanych procedur failover. Nie należy nazywać HA konfiguracji składającej się z kilku kontenerów na jednym serwerze.

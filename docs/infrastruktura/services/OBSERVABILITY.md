# Observability — OpenTelemetry, Prometheus, Jaeger i OpenSearch

## Cel i granice

Stos observability zbiera metryki, trace i logi techniczne potrzebne do wykrywania awarii, analizy wydajności oraz korelacji działania API, Temporal, LLM i Browser Workera. Nie jest źródłem prawdy dla audytu biznesowego.

Audyt operacji użytkownika ma oddzielny, trwały zapis w PostgreSQL/S3 z kontrolą integralności. Usunięcie indeksu OpenSearch lub zastosowanie sampling nie może usunąć dowodu approval i skutku biznesowego.

## Docelowa architektura

```text
.NET / React / Browser Worker / services
                    |
                   OTLP
                    v
          OpenTelemetry Collector
             |              |
             | metrics      | logs + traces
             v              v
         Prometheus     OpenSearch Data Prepper
             |              |
        Alertmanager        v
                        OpenSearch
                            |
                  OpenSearch Dashboards
```

Profil developerski o mniejszym koszcie:

```text
OTLP -> OpenTelemetry Collector -> Prometheus
                               -> Jaeger all-in-one (memory)
logs -> stdout/Collector debug lub krótka lokalna retencja
```

## Wybór i licencje

| Komponent | Licencja | Rola |
|---|---|---|
| OpenTelemetry Collector Contrib | Apache-2.0 | ingress, processing, redaction, routing |
| Prometheus | Apache-2.0 | metryki |
| Alertmanager | Apache-2.0 | deduplikacja i routing alertów |
| Jaeger | Apache-2.0 | trace UI i lekki profil dev |
| OpenSearch | Apache-2.0 | produkcyjny storage logów/trace |
| OpenSearch Dashboards | Apache-2.0 | zapytania i wizualizacje |
| OpenSearch Data Prepper | Apache-2.0 | pipeline OTLP do OpenSearch |

Domyślny stos nie używa Elasticsearch/Kibana, których binarne wydania są objęte Elastic License, ani Grafana/Loki/Tempo pod AGPLv3. Te licencje nie zabraniają użycia komercyjnego, ale wprowadzają obowiązki/ograniczenia, których nie potrzebujemy w podstawowej dystrybucji.

Źródła:

- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib)
- [Prometheus](https://github.com/prometheus/prometheus)
- [Alertmanager](https://github.com/prometheus/alertmanager)
- [Jaeger 2.x](https://www.jaegertracing.io/docs/2.19/)
- [OpenSearch](https://github.com/opensearch-project/OpenSearch)
- [Data Prepper OTLP source](https://docs.opensearch.org/latest/data-prepper/pipelines/configuration/sources/otlp-source/)
- [Elastic licensing FAQ](https://www.elastic.co/pricing/faq/licensing/)
- [Grafana licensing](https://grafana.com/licensing/)

## Wersje i pinning

- Wszystkie obrazy są przypięte po digestach.
- Collector core/contrib ma jedną zgodną linię; nie mieszamy przypadkowych wersji components.
- Początkowa linia testowa: Prometheus 3.x, Jaeger 2.19.x, OpenSearch/Dashboards 3.7.x.
- Data Prepper jest zgodny z wybraną linią OpenSearch.
- Index templates, Data Prepper pipelines, OTel config, alert rules i dashboards są wersjonowane w `deploy/compose/config/observability`.
- Aktualizacja Collector wymaga sprawdzenia stability level każdego receiver/processor/exporter, nie tylko wersji dystrybucji.

## Profile i topologie

### Development — `observability-lite`

- jeden OTel Collector;
- jeden Prometheus z krótką retencją;
- Jaeger all-in-one z memory storage;
- porty UI tylko na `127.0.0.1`;
- brak gwarancji zachowania danych po restarcie;
- konfiguracja nadal zawiera redaction i limits.

### Hardened single-node — `observability`

- Collector z kolejką/retry;
- Prometheus z trwałym volume i Alertmanager;
- Data Prepper;
- OpenSearch single-node z trwałym volume, security plugin i TLS;
- OpenSearch Dashboards za oauth2-proxy;
- jawne retencje/rollover i quota;
- backup/snapshot tylko według potrzeb RTO dla telemetrii.

### Multi-node/HA

- stateless Collectors za load balancerem albo agent/gateway pattern;
- Prometheus HA pairs i ewentualny zaakceptowany long-term backend;
- Alertmanager cluster;
- OpenSearch z co najmniej trzema cluster-manager eligible nodes na osobnych failure domains oraz data nodes dobranymi do retencji;
- Data Prepper redundantny;
- UI za redundantnym ingress/oauth2-proxy;
- test utraty noda, shard relocation i backpressure.

Kilka OpenSearch nodes na jednym hoście nie jest HA i może pogorszyć odporność przez wspólny dysk/RAM.

## Zasoby startowe

| Komponent | Development | Single-node production |
|---|---|---|
| OTel Collector | 0.5 CPU, 256–512 MiB | 1–2 CPU, 1–2 GiB |
| Prometheus | 1 CPU, 1 GiB | 2–4 CPU, 4–8 GiB + SSD |
| Jaeger memory | 0.5 CPU, 512 MiB | nie jest produkcyjnym storage |
| Data Prepper | 1 CPU, 1 GiB | 2–4 CPU, 2–4 GiB |
| OpenSearch | 2 CPU, 2–4 GiB | co najmniej 4 CPU, 8 GiB + szybki SSD |
| Dashboards | 0.5 CPU, 512 MiB | 1 CPU, 1–2 GiB |

To wartości startowe. Log volume, trace sampling, cardinality i okres retencji wymagają pomiaru. JVM heap OpenSearch nie może zajmować całego limitu kontenera.

## Dane dozwolone i zabronione

### Dozwolone

- service name/version/environment;
- trace/span ID;
- endpoint route template, nie pełny URL z sekretami;
- status/error class;
- duration, retry count, queue depth;
- task ID i session ID w formie nieujawniającej danych;
- model ID/version, skill ID/version, risk level;
- object ID/hash, nie treść artefaktu.

### Zabronione domyślnie

- Authorization, Cookie, Set-Cookie;
- tokeny OIDC, OpenBao, presigned URLs i API keys;
- browser storage state, cookies i hasła;
- pełny prompt/response LLM;
- raw DOM, wartości formularzy i zawartość strony;
- upload/download content;
- e-mail body;
- numery dokumentów i dane osobowe bez jawnej klasyfikacji/pseudonimizacji.

Redaction działa w aplikacji i Collectorze. Collector jest drugą barierą, nie usprawiedliwia logowania sekretu przez kod.

## Konfiguracja aplikacji

```text
OTEL_SERVICE_NAME=praxiara-api
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=production,service.version=<version>
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_TRACES_SAMPLER=parentbased_traceidratio
OTEL_TRACES_SAMPLER_ARG=<approved-ratio>
```

Nie ustawiamy `OTEL_DOTNET_AUTO_LOGS_INCLUDE_FORMATTED_MESSAGE=true` bez analizy, ponieważ formatted message może zawierać wrażliwe wartości.

Browser Worker wysyła telemetrykę przez dedykowany gateway/endpoint bez dołączania do całej `telemetry-net`.

## OTel Collector

Minimalne pipelines zawierają:

- OTLP gRPC/HTTP receivers;
- `memory_limiter` przed kosztownymi processorami;
- resource normalization;
- transform/filter/redaction;
- probabilistic lub tail sampling zgodnie z polityką;
- `batch`;
- sending queue i retry;
- niezależne exporters dla metrics/logs/traces;
- health check i własną internal telemetry.

Szkielet, nie gotowy config produkcyjny:

```yaml
receivers:
  otlp:
    protocols:
      grpc:
      http:

processors:
  memory_limiter:
    check_interval: 1s
    limit_mib: 768
  batch:

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/data-prepper]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus]
    logs:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/data-prepper]
```

Produkcyjny config dodaje TLS/auth, redaction, limits, queue, retry, exact endpoints i telemetrykę samego Collectora.

## Retencja

Retencje mają wartość w konfiguracji, właściciela i podstawę:

| Sygnał | Domyślny kierunek |
|---|---|
| high-cardinality debug logs | dni |
| application logs | dni/tygodnie |
| traces | dni, sampling |
| metrics | tygodnie/miesiące zależnie od pojemności |
| security/audit | osobny system i polityka |

OpenSearch używa index templates, rollover i Index State Management. Nie pozostawiamy nieograniczonych indeksów dziennych. Prometheus ma `--storage.tsdb.retention.time` i opcjonalny size limit.

## Sekrety

```text
/run/secrets/opensearch_admin_password
/run/secrets/opensearch_ingest_password
/run/secrets/opensearch_tls_ca
/run/secrets/opensearch_tls_cert
/run/secrets/opensearch_tls_key
/run/secrets/alertmanager_smtp_password
```

Collector/Data Prepper używają dedykowanego ingest identity, nie admina. Dashboards ma osobną service role. Alertmanager credentials są montowane z OpenBao/secret file; nie występują w repo rule/config.

## Sieci i porty

- OTLP: 4317 gRPC, 4318 HTTP, tylko `telemetry-net`/gateway.
- Prometheus: 9090, prywatnie; dev `127.0.0.1`.
- Alertmanager: 9093, prywatnie.
- Jaeger UI: 16686, dev localhost lub oauth2-proxy.
- OpenSearch: 9200 API i 9600 metrics/performance analyzer, tylko prywatnie.
- Dashboards: 5601 przez oauth2-proxy/Caddy.
- Data Prepper OTLP/API: tylko `telemetry-net`.

Żaden backend telemetryczny nie jest dostępny z Internetu bez auth proxy/VPN.

## Uruchomienie

Lekki development:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.observability-lite.yaml \
  --profile observability-lite \
  up -d --wait
```

Pełny stos:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.prod-single-node.yaml \
  --profile observability \
  up -d --wait
```

Index templates, users/roles i Data Prepper pipelines inicjalizuje idempotentny `observability-init` po readiness OpenSearch.

## Health i smoke test

Collector:

```bash
curl --fail --silent http://otel-collector:13133/
```

Prometheus:

```bash
curl --fail --silent http://prometheus:9090/-/ready
```

Jaeger dev:

```bash
curl --fail --silent http://jaeger:16686/
```

OpenSearch:

```bash
curl --fail --silent \
  --cacert /run/secrets/opensearch_tls_ca \
  https://opensearch:9200/_cluster/health
```

Pełny smoke test generuje wspólny trace:

1. API emituje metric, log i trace z kontrolnym correlation ID.
2. Metric jest queryable w Prometheus.
3. Trace jest queryable w Jaeger lub OpenSearch.
4. Log jest queryable w OpenSearch i zawiera trace ID.
5. Sekret fixture w nagłówku/payload nie występuje w żadnym backendzie.
6. Alert testowy dochodzi do kontrolowanego receivera.
7. Nieuprawniony user nie otwiera UI.

Test redaction jest testem blokującym release.

## Alerty minimalne

- brak telemetryki z krytycznej usługi;
- Collector refused/dropped data, queue full, exporter failures;
- Prometheus target down, TSDB disk/compaction failure;
- OpenSearch cluster yellow/red, unassigned shards, disk watermark, JVM pressure;
- Data Prepper backpressure/errors;
- log/trace ingest lag;
- cert expiry;
- cardinality explosion;
- brak miejsca na volume;
- browser worker crash/OOM rate;
- Temporal task queue backlog;
- LLM latency/error/queue bez prompt body.

## Backup i restore

Telemetria ma niższy priorytet RPO niż dane aplikacyjne, ale konfiguracja musi być odtwarzalna.

Backupujemy:

- OTel configs;
- Prometheus/Alertmanager rules;
- Data Prepper pipelines;
- OpenSearch templates, ISM policies, roles i Dashboards saved objects;
- image digests;
- opcjonalne OpenSearch snapshots do oddzielnego S3;
- Prometheus data tylko jeśli wymagania uzasadniają koszt; rules/config zawsze w Git.

OpenSearch snapshot nie trafia do tego samego failure domain. Restore jest wykonywany do pustego klastra o zgodnej wersji, następnie templates/roles/dashboards są walidowane przed dopuszczeniem ingestu.

Jaeger memory nie jest backupowany. Po odtworzeniu observability brak historycznych trace nie może wpływać na stan zadań lub audyt.

## Aktualizacja i rollback

1. Sprawdź compatibility matrix i release notes.
2. Backup config oraz snapshot wymaganych indeksów.
3. Test OTLP pipelines/redaction na staging.
4. Aktualizuj Collector/Data Prepper osobno od OpenSearch major.
5. OpenSearch rolling upgrade tylko w oficjalnie wspieranej ścieżce.
6. Porównaj dropped data, ingest latency, query i resources.
7. Rollback Collector/config jest możliwy przy compatible telemetry schema.
8. Rollback OpenSearch po zmianie formatu może wymagać restore snapshot do starego klastra.

## Hardening

- TLS/auth dla ingest między hostami.
- dedykowane least-privilege ingest/query/admin identities.
- UI za oauth2-proxy/VPN.
- security plugin OpenSearch z własnymi certyfikatami; demo config zabroniony.
- redaction w kodzie i Collectorze.
- body/attribute/event size limits.
- memory limiter, queue, backpressure i disk quotas.
- sampling jawny; errors/high-risk traces preferowane, lecz audit oddzielny.
- index retention/rollover.
- brak publicznych ports i anonymous write.
- Dashboards nie pozwala zwykłemu użytkownikowi czytać wszystkich tenantów.
- log injection/newlines i unbounded cardinality są testowane.

## Troubleshooting

### Brak trace, ale są logi

Sprawdź sampling, propagation headers, OTLP traces pipeline, exporter queue i clock. Nie zwiększaj globalnie sampling do 100% bez oceny pojemności.

### Collector OOM

Sprawdź `memory_limiter`, queue, batch, duże log records i unavailable exporter. Zmniejszenie limitu kontenera bez zmiany pipeline pogorszy drop/restart loop.

### OpenSearch yellow/red

Sprawdź unassigned shards, disk watermarks, cluster manager i replica count odpowiedni do single-node. Nie ustawiaj bezrefleksyjnie replicas=0 w docelowym HA.

### Eksplozja cardinality

Znajdź nowy label/attribute, zatrzymaj jego ingest i popraw instrumentation. User ID, URL z ID, prompt hash per request i object key nie powinny być labelami metryk.

### Dane poufne w logu/trace

Traktuj jako incydent: ogranicz dostęp, zatrzymaj ingest źródła, usuń zgodnie z procedurą i retencją, popraw redaction oraz dodaj regression test. Sama zmiana dashboardu nie usuwa danych z indeksu.

### Alert nie dotarł

Sprawdź rule evaluation, Alertmanager route/inhibition, receiver secret, egress i retry. Używaj cyklicznego syntetycznego alertu end-to-end.

## Różnice produkcyjne

Profil lite jest narzędziem developerskim i może tracić trace. Produkcja wymaga trwałego storage, TLS/auth, redaction tests, retencji, alertów, backupu konfiguracji i protected UI. HA wymaga redundantnych collectorów/Alertmanager/Data Prepper oraz OpenSearch na osobnych failure domains.

# Stos technologiczny i licencje

Stan rekomendacji: 11 lipca 2026. Wersje w tej tabeli są punktem startowym do testów zgodności, a nie pozwoleniem na użycie tagu `latest`.

## Domyślny stos

| Obszar | Komponent | Linia testowa | Licencja | Status |
|---|---|---:|---|---|
| Baza danych | PostgreSQL | 18.4 | PostgreSQL License | dozwolony |
| Cache/backplane | Valkey | 9.1.x | BSD-3-Clause | dozwolony |
| S3 | SeaweedFS | aktualne stabilne 4.x | Apache-2.0 | dozwolony |
| Identity | Keycloak | aktualne stabilne 26.x | Apache-2.0 | dozwolony |
| Ochrona UI | oauth2-proxy | 7.15.x | MIT | dozwolony |
| Secrets | OpenBao | 2.5.x | MPL-2.0 | dozwolony po rejestracji weak copyleft |
| Bootstrap | SOPS + age | aktualne stabilne | MPL-2.0 + BSD-3-Clause | dozwolony po rejestracji |
| Workflow | Temporal Server i .NET SDK | aktualne stabilne | MIT | dozwolony |
| LLM runtime | llama.cpp server | przypięty commit/image digest | MIT | dozwolony; model oddzielnie |
| LLM dev | Ollama headless | przypięty tag/digest | MIT | dozwolony; model i aplikacja desktop oddzielnie |
| Browser | Microsoft.Playwright | zgodny z pakietem NuGet | MIT | dozwolony; browser notices oddzielnie |
| WebRTC | LiveKit Server | aktualne stabilne 1.x | Apache-2.0 | dozwolony |
| TURN | LiveKit TURN lub coturn | aktualne stabilne | Apache-2.0 / BSD-3-Clause | dozwolony |
| Edge | Caddy | aktualne stabilne 2.x | Apache-2.0 | dozwolony bez niezweryfikowanych pluginów |
| Telemetria | OpenTelemetry Collector Contrib | wspólna wersja dystrybucji | Apache-2.0 | dozwolony |
| Metryki | Prometheus, Alertmanager | 3.x / 0.x | Apache-2.0 | dozwolony |
| Trace dev | Jaeger | 2.19.x | Apache-2.0 | dozwolony |
| Logi/trace prod | Data Prepper, OpenSearch, Dashboards | zgodna linia 3.x | Apache-2.0 | dozwolony |
| E-mail dev | Mailpit | aktualne stabilne | MIT | dozwolony, wyłącznie dev/test |
| Security | Trivy, Syft, ZAP | aktualne stabilne | Apache-2.0 | dozwolony |
| Git secrets | Gitleaks CLI | aktualne stabilne | MIT | dozwolony; nie mylić z Action |

## Poziomy akceptacji

### A — automatycznie akceptowane

- MIT
- Apache-2.0
- BSD-2-Clause i BSD-3-Clause
- ISC
- PostgreSQL License
- CC0 dla kodu i danych, jeżeli pochodzenie jest udokumentowane

Warunkiem pozostaje zachowanie copyright, `LICENSE`, `NOTICE` i innych atrybucji wymaganych przy dystrybucji.

### B — wymagają świadomej akceptacji

- MPL-2.0
- LGPL-2.1/3.0
- GPL-2.0/3.0
- AGPL-3.0

Należy udokumentować, czy komponent jest używany jako osobny proces/kontener, czy jest linkowany lub modyfikowany, oraz jakie materiały źródłowe trzeba przekazać odbiorcy. OpenBao jest świadomym wyjątkiem MPL-2.0. noVNC stack jest opcjonalnym wyjątkiem diagnostycznym.

### C — domyślnie zabronione

- SSPL
- Redis Source Available License
- Business Source License / BUSL
- Elastic License
- Functional Source License
- Commons Clause
- PolyForm
- licencje ograniczone do zastosowań niekomercyjnych lub ewaluacyjnych
- artefakty bez jednoznacznej licencji

Wyjątek wymaga decyzji właściciela produktu i analizy prawnej zapisanej jako ADR.

## Najważniejsze pułapki

### Redis

Redis 7.2 i starsze pozostają BSD-3-Clause, Redis 7.4–7.8 używa RSALv2/SSPLv1, a Redis 8 dodaje AGPLv3 jako trzeci wariant. AGPL pozwala na użycie komercyjne, ale nakłada obowiązki copyleft dla usług sieciowych i modyfikacji. Domyślnym zamiennikiem jest Valkey BSD-3-Clause. Nie instalujemy również Redis Insight bez oddzielnej oceny jego warunków.

Źródła: [licencje Redis](https://redis.io/legal/licenses/), [Valkey](https://valkey.io/topics/introduction/).

### MinIO

Community MinIO było AGPLv3, a repozytorium zostało zarchiwizowane 25 kwietnia 2026. Aktualna licencja dystrybucji producenta poza umową komercyjną dopuszcza jedną instancję do wewnętrznej ewaluacji nieprodukcyjnej. Nie rozpoczynamy nowego wdrożenia na MinIO. Używamy SeaweedFS i utrzymujemy test zgodności wykorzystywanego podzbioru S3.

Źródła: [zarchiwizowane repozytorium MinIO](https://github.com/minio/minio), [aktualna licencja MinIO](https://docs.min.io/license/), [SeaweedFS](https://github.com/seaweedfs/seaweedfs).

### Vault

HashiCorp przeniósł nowsze wydania Vault na BSL 1.1. BSL jest source-available i ma warunkowe ograniczenia komercyjne. Stos używa OpenBao pod MPL-2.0.

Źródła: [zmiana HashiCorp](https://www.hashicorp.com/blog/hashicorp-adopts-business-source-license), [OpenBao](https://github.com/openbao/openbao).

### Elastic

Kod części Elasticsearch i Kibana jest oferowany także pod AGPL, ale domyślne binarne wydania nadal są dystrybuowane pod Elastic License 2.0. Używamy OpenSearch, OpenSearch Dashboards i Data Prepper pod Apache-2.0.

Źródła: [FAQ Elastic](https://www.elastic.co/pricing/faq/licensing/), [OpenSearch](https://github.com/opensearch-project/OpenSearch).

### Grafana, Loki i Tempo

Główne projekty Grafana Labs są AGPLv3. Są bezpłatne i mogą być używane komercyjnie z zachowaniem obowiązków licencji, lecz nie są potrzebne w domyślnej instalacji. Domyślne UI zapewniają Prometheus, Jaeger oraz OpenSearch Dashboards. Dodanie Grafany wymaga wyjątku poziomu B i wpisu w `THIRD-PARTY-NOTICES`.

Źródło: [licencjonowanie Grafana Labs](https://grafana.com/licensing/).

### Docker Desktop

Docker Desktop wymaga płatnej subskrypcji przy profesjonalnym użyciu w większych organizacjach oraz w administracji publicznej. Bezpłatność dotyczy między innymi małych firm spełniających jednocześnie limity zatrudnienia i przychodu. Docker Engine/Moby pozostaje open source. Na Linux używamy Docker Engine lub Podman, a na macOS preferujemy Colima lub Rancher Desktop, jeśli warunki Docker Desktop nie są spełnione.

Źródła: [warunki Docker Desktop](https://docs.docker.com/subscription/desktop-license/), [Docker Engine](https://docs.docker.com/engine/).

### Gitleaks Action

Gitleaks CLI jest MIT, ale `gitleaks-action` v2 ma oddzielne warunki i wymaga klucza licencyjnego w części zastosowań organizacyjnych. Pipeline uruchamia CLI bezpośrednio w kontenerze albo binarnie.

Źródła: [Gitleaks CLI](https://github.com/gitleaks/gitleaks), [Gitleaks Action](https://github.com/gitleaks/gitleaks-action).

### Runtime LLM a model

Licencja llama.cpp lub Ollama nie udziela praw do modelu. Każda waga, kwantyzacja, adapter LoRA i fine-tune ma osobny rekord z:

- źródłem i właścicielem;
- dokładnym revision/commit;
- SHA-256;
- licencją kodu i osobno licencją wag;
- potwierdzeniem commercial-use;
- ograniczeniami acceptable-use;
- pochodzeniem danych i kwantyzacji, o ile jest znane;
- datą i osobą akceptującą.

Nie pobieramy modeli automatycznie podczas `docker compose up`.

### Chrome for Testing i Chromium

Microsoft.Playwright jest MIT, lecz binaria przeglądarek i ich zależności mają własne notices i warunki. Od Playwright 1.57 część platform korzysta z Chrome for Testing. Obraz Browser Workera jest skanowany i archiwizowany z pełnym SBOM oraz notices. Użycie systemowego Chromium wymaga osobnego testu zgodności z daną wersją Playwright.

Źródło: [Playwright .NET](https://github.com/microsoft/playwright-dotnet).

## Rejestr zgodności

Dla każdego wydania należy zachować:

```text
artifacts/compliance/
├── sbom.cdx.json
├── sbom.spdx.json
├── third-party-notices.txt
├── image-digests.json
├── model-registry.json
└── vulnerability-exceptions.vex.json
```

Wyjątek podatności lub licencji ma właściciela, uzasadnienie i datę wygaśnięcia. Sam fakt, że komponent jest dostępny bez opłaty, nie oznacza akceptowalnej licencji.

## Oficjalne źródła licencji

- [PostgreSQL License](https://www.postgresql.org/about/licence/)
- [Valkey](https://valkey.io/topics/introduction/)
- [SeaweedFS](https://github.com/seaweedfs/seaweedfs)
- [Keycloak](https://github.com/keycloak/keycloak)
- [OpenBao](https://github.com/openbao/openbao)
- [Temporal](https://github.com/temporalio/temporal)
- [Microsoft.Extensions.AI](https://github.com/dotnet/extensions)
- [llama.cpp](https://github.com/ggml-org/llama.cpp)
- [LiveKit](https://github.com/livekit/livekit)
- [Caddy](https://github.com/caddyserver/caddy)
- [OpenTelemetry Collector Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib)
- [Prometheus](https://github.com/prometheus/prometheus)
- [Jaeger](https://github.com/jaegertracing/jaeger)
- [OpenSearch](https://github.com/opensearch-project/OpenSearch)

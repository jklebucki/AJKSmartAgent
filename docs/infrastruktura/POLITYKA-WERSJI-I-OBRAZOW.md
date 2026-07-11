# Polityka wersji i obrazów

## Cel

Zapewnić powtarzalne, audytowalne i możliwe do odtworzenia wdrożenia bez niekontrolowanych aktualizacji obrazu, modelu lub zależności.

## Reguły obowiązkowe

1. Nie używamy `latest`, `edge`, `nightly`, ruchomego `main` ani samego major tagu.
2. Developerski Compose używa dokładnych tagów zapisanych w `versions.env`; jest to wygodny bootstrap, a nie immutable artefakt wydania.
3. Produkcyjny Compose uruchamia obraz po immutable digest `sha256` pochodzącym z zatwierdzonego lockfile.
4. Czytelny tag jest przechowywany obok digestu w lockfile i metadanych wydania.
5. Wszystkie wspierane architektury CPU mają osobno zweryfikowany digest.
6. Obraz musi pochodzić z oficjalnego rejestru projektu lub z własnego kontrolowanego build pipeline.
7. Każdy obraz wydania ma SBOM, wynik skanu podatności i rejestr licencji.
8. Wersje klientów i serwerów objęte kontraktem zgodności są aktualizowane razem.
9. Model LLM jest wersjonowany tak samo rygorystycznie jak kod.

## Pliki źródłowe

`deploy/compose/versions.env` zawiera wersje czytelne dla człowieka:

```dotenv
POSTGRES_VERSION=18.4-alpine3.24
VALKEY_VERSION=9.1.0-alpine3.23
SEAWEEDFS_VERSION=4.39
KEYCLOAK_VERSION=26.7.0
TEMPORAL_VERSION=1.29.7
OLLAMA_VERSION=0.31.2
JAEGER_VERSION=2.19.0
```

Planowany `deploy/compose/images.lock.json` będzie zawierał artefakty wykonywalne wydania:

```json
{
  "generatedAt": "2026-07-11T00:00:00Z",
  "images": [
    {
      "name": "postgres",
      "tag": "18.4-bookworm",
      "platform": "linux/amd64",
      "digest": "sha256:replace-with-verified-digest",
      "source": "docker.io/library/postgres",
      "license": "PostgreSQL"
    }
  ]
}
```

Compose może budować pełną referencję z wygenerowanego pliku środowiskowego, ale digest w review musi być widoczny. Nie wolno pobierać digestu dynamicznie podczas startu produkcji.

## Baseline do pierwszych testów

Na dzień sporządzenia dokumentacji warto rozpocząć testy od:

- PostgreSQL 18.4;
- Valkey 9.1.x;
- Keycloak 26.7.x;
- OpenBao 2.5.x;
- OpenTelemetry Collector z jednej zgodnej linii dystrybucji;
- Prometheus 3.x;
- Jaeger 2.19.x;
- OpenSearch i Dashboards 3.7.x;
- Playwright zgodnego dokładnie z pakietem NuGet używanym przez Browser Worker.

Przy tworzeniu lub aktualizacji lockfile trzeba ponownie sprawdzić aktualne wydania i security advisories. Oficjalne źródła:

- [PostgreSQL versioning](https://www.postgresql.org/support/versioning/)
- [Valkey releases](https://github.com/valkey-io/valkey/releases)
- [Keycloak releases](https://github.com/keycloak/keycloak/releases)
- [OpenBao releases](https://github.com/openbao/openbao/releases)
- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Prometheus releases](https://github.com/prometheus/prometheus/releases)
- [Jaeger releases](https://www.jaegertracing.io/download/)
- [OpenSearch release policy](https://opensearch.org/releases/)
- [Playwright .NET releases](https://github.com/microsoft/playwright-dotnet/releases)

## Build własnych obrazów

Własny obraz:

- używa minimalnego, wspieranego obrazu bazowego przypiętego po digest;
- ma wieloetapowy build;
- nie zawiera SDK, managera pakietów ani narzędzi debug w runtime, jeśli nie są potrzebne;
- działa jako jawny użytkownik non-root;
- nie zapisuje do root filesystem poza zadeklarowanym `tmpfs`/volume;
- zawiera OCI labels: revision, source, version, created, licenses;
- zachowuje `LICENSE`, `NOTICE` i third-party notices;
- ma provenance build i podpis po przejściu testów.

Przykładowe wymagane labels:

```dockerfile
LABEL org.opencontainers.image.source="https://example.invalid/praxiara"
LABEL org.opencontainers.image.revision="$VCS_REF"
LABEL org.opencontainers.image.version="$VERSION"
LABEL org.opencontainers.image.licenses="Proprietary"
```

## Aktualizacja

### Zwykła

1. Utwórz osobny change z wersją, changelogiem i nowym digestem.
2. Pobierz obraz na izolowany runner.
3. Wygeneruj SBOM Syft.
4. Uruchom Trivy dla CVE, misconfiguration, secret i license.
5. Wykonaj testy kontraktowe i migracyjne.
6. Wykonaj backup oraz test rollbacku w staging.
7. Zaktualizuj `versions.env`, `images.lock.json` i notices.
8. Wdróż canary lub pojedynczy worker.
9. Porównaj błędy, latency, zużycie zasobów i kompatybilność danych.
10. Dopiero wtedy promuj digest.

### Security hotfix

Krytyczna podatność skraca cykl, ale nie usuwa minimalnych testów: start, readiness, migracja, podstawowa ścieżka użytkownika, browser session, backup/restore compatibility i rollback.

## Testy kontraktowe

| Komponent | Minimalny kontrakt |
|---|---|
| PostgreSQL | migracje, transakcje, backup/PITR, collation/timezone |
| Valkey | TTL, pub/sub lub backplane, ACL/TLS, failover klienta |
| SeaweedFS | multipart, presigned GET/PUT, range, ETag, lifecycle używanych bucketów |
| Keycloak | OIDC discovery, PKCE, refresh/logout, role/group claims |
| OpenBao | auth, policy, lease renewal/revocation, seal/unseal, snapshot |
| Temporal | workflow replay, signal approval, activity retry, continue-as-new |
| LLM | JSON/tool schema, streaming, timeout, cancellation, vision jeśli wymagane |
| Playwright | start browsera, context isolation, download/upload, trace, stream |
| Observability | OTLP metrics/logs/traces, redaction, retention, query |

## Rollback

- Rollback obrazu nie oznacza rollbacku danych.
- Migracja bazy musi deklarować kompatybilność `N` i `N-1` albo mieć jawny plan restore.
- OpenSearch major upgrade wymaga osobnej procedury snapshot/restore.
- Keycloak realm export nie zastępuje backupu bazy.
- Temporal workflow code musi zachować replay compatibility; zmiany używają versioning mechanizmu SDK.
- Model LLM można cofnąć dopiero po potwierdzeniu zgodności jego identyfikatora, formatu promptu i capability registry.
- Playwright i browser binaries są cofane razem.

## Retencja artefaktów

Należy zachować co najmniej przez okres wsparcia wydania:

- manifesty i digesty obrazów;
- SBOM CycloneDX i SPDX;
- podpis/provenance;
- config bez sekretów;
- migracje i schematy;
- model lock i model card;
- wyniki testów zgodności;
- instrukcję odtworzenia środowiska.

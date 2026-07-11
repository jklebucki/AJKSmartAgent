# Infrastruktura Praxiara

Ten katalog opisuje docelową infrastrukturę aplikacji Praxiara: lokalne środowisko developerskie, instalację produkcyjną na jednym hoście oraz topologie wysokiej dostępności. Dokumentacja jest źródłem wymagań dla przyszłych plików w `deploy/compose`; nie należy kopiować przykładów do produkcji bez uzupełnienia sekretów, TLS, limitów zasobów i kopii zapasowych.

## Zasady nadrzędne

- Wszystkie wymagane komponenty muszą być bezpłatne także w zastosowaniu komercyjnym.
- Domyślny stos preferuje licencje MIT, Apache-2.0, BSD i PostgreSQL License.
- Komponenty MPL/LGPL/GPL/AGPL wymagają wpisu w rejestrze licencji i oceny obowiązków dystrybucyjnych.
- SSPL, RSAL, BSL/BUSL, Elastic License, FSL, Commons Clause, PolyForm, licencje `non-commercial`, `evaluation-only` i brak licencji są domyślnie zabronione.
- Żaden obraz nie używa tagu `latest`; wersja i digest są przypięte.
- Sekrety nie trafiają do repozytorium, argumentów procesu, obrazu ani zwykłego `.env`.
- Browser Worker jest traktowany jak środowisko uruchamiające niezaufany kod z Internetu. Nie ma dostępu do sieci danych ani socketu Dockera.
- Audyt biznesowy jest niezależny od logów operacyjnych i ma własną retencję oraz kontrolę integralności.
- Docker Compose jest środowiskiem lokalnym, CI i ewentualnie hardened single-node. Kilka kontenerów na jednym hoście nie stanowi HA.

## Kolejność lektury

1. [Stos i licencje](STOS-I-LICENCJE.md)
2. [Profile Compose](PROFILE-COMPOSE.md)
3. [Polityka wersji i obrazów](POLITYKA-WERSJI-I-OBRAZOW.md)
4. [Sieci i porty](SIECI-I-PORTY.md)
5. [Hardening produkcyjny](HARDENING-PRODUKCYJNY.md)
6. Instrukcje w katalogu [`services`](services/)

## Mapa usług

| Potrzeba | Komponent | Dokument |
|---|---|---|
| Baza relacyjna | PostgreSQL | [POSTGRESQL.md](services/POSTGRESQL.md) |
| Cache i backplane | Valkey | [VALKEY.md](services/VALKEY.md) |
| Obiekty S3 | SeaweedFS | [SEAWEEDFS.md](services/SEAWEEDFS.md) |
| OIDC i ochrona UI operatorskich | Keycloak, oauth2-proxy | [KEYCLOAK-OAUTH2-PROXY.md](services/KEYCLOAK-OAUTH2-PROXY.md) |
| Sekrety i bootstrap | OpenBao, SOPS, age | [OPENBAO-SOPS.md](services/OPENBAO-SOPS.md) |
| Trwałe workflow | Temporal | [TEMPORAL.md](services/TEMPORAL.md) |
| Lokalny LLM | llama.cpp, opcjonalnie Ollama | [LOCAL-LLM.md](services/LOCAL-LLM.md) |
| Automatyzacja przeglądarki | Browser Worker, Playwright | [BROWSER-WORKER-PLAYWRIGHT.md](services/BROWSER-WORKER-PLAYWRIGHT.md) |
| Strumień WebRTC | LiveKit, TURN | [LIVEKIT-TURN.md](services/LIVEKIT-TURN.md) |
| Awaryjny zdalny pulpit | noVNC, websockify, TigerVNC | [NOVNC-FALLBACK.md](services/NOVNC-FALLBACK.md) |
| Ingress i TLS | Caddy | [CADDY.md](services/CADDY.md) |
| Metryki, trace i logi | OpenTelemetry, Prometheus, Jaeger/OpenSearch | [OBSERVABILITY.md](services/OBSERVABILITY.md) |
| E-mail developerski | Mailpit | [MAILPIT.md](services/MAILPIT.md) |
| Skanowanie i SBOM | Trivy, Syft, Gitleaks CLI, ZAP | [SECURITY-SCANNING.md](services/SECURITY-SCANNING.md) |

## Konwencja przyszłego katalogu wdrożeniowego

Dokumentacja zakłada następujący układ, który zostanie utworzony w osobnym etapie:

```text
deploy/compose/
├── compose.yaml
├── compose.dev.yaml
├── compose.prod-single-node.yaml
├── compose.gpu-nvidia.yaml
├── compose.gpu-amd.yaml
├── compose.observability-lite.yaml
├── versions.env
├── images.lock.json
├── config/
├── init/
└── secrets/
```

Wszystkie przykłady poleceń są wykonywane z katalogu głównego repozytorium.

## Wspólny cykl operacyjny

1. Zweryfikuj runtime kontenerowy i wolne zasoby.
2. Odszyfruj wyłącznie bootstrap wymagany dla danego środowiska.
3. Zweryfikuj `docker compose config` i brak niezastąpionych zmiennych.
4. Pobierz obrazy po digestach i wykonaj skan Trivy.
5. Uruchom usługi stanowe, zaczekaj na readiness, potem init jobs i aplikację.
6. Wykonaj smoke testy opisane w dokumentach usług.
7. Zweryfikuj backup i co najmniej okresowo pełne odtworzenie na czystym środowisku.
8. Po aktualizacji porównaj metryki, błędy, migracje i możliwość rollbacku.

## Źródła prawdy

- Ten katalog: wymagania infrastrukturalne i procedury.
- `deploy/compose/images.lock.json`: dokładne digesty uruchamianych obrazów.
- `deploy/compose/versions.env`: czytelne wersje testowanego zestawu.
- SBOM wydania: rzeczywiste składniki i licencje obrazu.
- OpenBao: sekrety runtime.
- PostgreSQL i magazyn obiektowy: dane aplikacyjne i artefakty.

Dokumentacja nie zastępuje przeglądu prawnego licencji ani analizy ryzyka konkretnego wdrożenia.

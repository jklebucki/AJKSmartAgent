# Usługi zewnętrzne

Dokumenty w tym katalogu definiują sposób przygotowania, uruchomienia, weryfikacji, zabezpieczenia i utrzymania usług zewnętrznych Praxiara. Polecenia korzystają z istniejącej bazy developerskiej w `deploy/compose` albo są wyraźnie oznaczonymi wymaganiami dla profili docelowych opisanych w [profilach Compose](../PROFILE-COMPOSE.md).

Każda instrukcja rozdziela trzy tryby:

- development/CI: wygoda, dane nietrwałe lub łatwe do odtworzenia, porty tylko na `127.0.0.1`;
- hardened single-node: trwałe wolumeny, TLS, sekrety, backup i limity, lecz bez HA hosta;
- multi-node/HA: osobne failure domains, procedura failover i restore.

Lista usług znajduje się w [głównym indeksie infrastruktury](../README.md).

## Stan implementacji Compose

| Dokument | Zakres dostępny teraz | Zakres docelowy |
|---|---|---|
| PostgreSQL, Valkey, SeaweedFS | profile developerskie i trwałe wolumeny | backup, restore drills i topologia HA |
| Keycloak / oauth2-proxy | Keycloak w profilu `identity` | oauth2-proxy i konfiguracja produkcyjna |
| Temporal | serwer i UI w profilu `workflow` | osobne init jobs i topologia HA |
| Lokalny LLM | Ollama w profilu `llm-local` | llama.cpp oraz override GPU |
| Obserwowalność | OTel Collector, Prometheus i Jaeger w `observability-lite` | Alertmanager, OpenSearch i trwała retencja |
| Mailpit | profil `mail` | pozostaje wyłącznie usługą dev/test |
| Caddy | profil `edge` do lokalnego routingu | publiczny TLS i hardened override |
| OpenBao, LiveKit/TURN, noVNC, skanery | dokumentacja projektowa | profile i konfiguracje do implementacji |
| Browser Worker | osobny projekt .NET istnieje w rozwiązaniu | obraz, sandbox, egress gateway i streaming |

Status „docelowy” oznacza, że dokument stanowi specyfikację implementacji; wskazane w nim polecenia Compose nie są jeszcze wykonywalne bez utworzenia opisanego profilu lub override.

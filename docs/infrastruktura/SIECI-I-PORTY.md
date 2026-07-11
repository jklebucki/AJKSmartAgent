# Sieci i porty

## Cel

Ograniczyć ruch zgodnie z zasadą najmniejszych uprawnień. Szczególnie ważna jest separacja Browser Workera, który przetwarza niezaufane strony i pliki.

## Strefy sieciowe

| Sieć Compose | Dostęp do Internetu | Członkowie | Zasada |
|---|---:|---|---|
| `edge-net` | przez Caddy | Caddy, API/UI, oauth2-proxy | jedyna zwykła strefa ingress |
| `app-net` | ograniczony | API, orchestrator, Temporal client | komunikacja aplikacyjna |
| `data-net` | nie, `internal: true` | PostgreSQL, Valkey, SeaweedFS, uprawnieni klienci | żadnych host portów prod |
| `control-net` | nie, `internal: true` | Keycloak, OpenBao, Temporal, koordynator | control plane |
| `telemetry-net` | nie, `internal: true` | OTel, Prometheus, Jaeger/Data Prepper/OpenSearch | dane telemetryczne |
| `browser-egress-net` | tak, przez kontrolowany egress | efemeryczny Browser Worker, egress gateway | brak połączenia z data/control |
| `stream-net` | częściowo publiczny | LiveKit/TURN, publisher, Caddy signaling | tylko media i signaling |

Usługa może być dołączona do dwóch sieci wyłącznie, gdy rzeczywiście pełni rolę gatewaya. API nie powinno automatycznie mostkować `browser-egress-net` i `data-net`; artefakty przechodzą przez wąski Artifact Gateway lub presigned URL o minimalnym zakresie.

## Macierz portów

Numery są domyślne i mogą być zmienione w konfiguracji. `expose` oznacza dostęp wewnątrz Compose, nie publikację na hoście.

| Usługa | Port | Protokół | Ekspozycja dev | Ekspozycja prod |
|---|---:|---|---|---|
| Caddy HTTP | 80 | TCP | host | host, redirect/ACME |
| Caddy HTTPS | 443 | TCP/UDP | host | host, HTTPS/HTTP3 |
| Praxiara API | 8080 | TCP | opcjonalnie `127.0.0.1` | tylko `edge-net` |
| PostgreSQL | 5432 | TCP | `127.0.0.1` tylko gdy potrzebne | tylko `data-net` |
| Valkey | 6379 | TCP | `127.0.0.1` tylko gdy potrzebne | tylko `data-net` |
| Seaweed master | 9333 | TCP | wewnętrzny | tylko `data-net` |
| Seaweed volume | 8080/18080 | TCP | wewnętrzny | tylko `data-net` |
| Seaweed filer | 8888 | TCP | wewnętrzny | tylko `data-net` |
| Seaweed S3 | 8333 | TCP | `127.0.0.1` opcjonalnie | tylko `data-net`/Artifact Gateway |
| Keycloak | 8080/9000 | TCP | `127.0.0.1` opcjonalnie | przez Caddy; management internal |
| oauth2-proxy | 4180 | TCP | wewnętrzny | `edge-net` |
| OpenBao API | 8200 | TCP | `127.0.0.1` opcjonalnie | `control-net`, TLS |
| OpenBao cluster | 8201 | TCP | wewnętrzny | węzły OpenBao, mTLS |
| Temporal frontend | 7233 | TCP/gRPC | `127.0.0.1` opcjonalnie | `control-net` |
| Temporal UI | 8233/8080 | TCP | `127.0.0.1` | oauth2-proxy/Caddy |
| llama.cpp/Ollama | 8080/11434 | TCP | `127.0.0.1` opcjonalnie | tylko `app-net` |
| LiveKit signaling | 7880 | TCP | `127.0.0.1`/Caddy | Caddy HTTPS/WSS |
| LiveKit ICE/TCP | 7881 | TCP | host | host/firewall |
| LiveKit RTC/UDP | ustalony zakres | UDP | host | host/firewall |
| TURN | 3478/5349 | UDP/TCP | host jeśli testowany | host; TLS na 5349 |
| TURN relay | ograniczony zakres | UDP | host | host/firewall |
| noVNC WebSocket | 6080 | TCP | `127.0.0.1` lub Caddy | wyłącznie Caddy/auth |
| VNC | 5900 | TCP | wewnętrzny | nigdy publiczny |
| OTel OTLP gRPC | 4317 | TCP | `127.0.0.1` opcjonalnie | `telemetry-net` |
| OTel OTLP HTTP | 4318 | TCP | `127.0.0.1` opcjonalnie | `telemetry-net` |
| Prometheus | 9090 | TCP | `127.0.0.1` | oauth2-proxy lub VPN |
| Alertmanager | 9093 | TCP | `127.0.0.1` | oauth2-proxy lub VPN |
| Jaeger UI | 16686 | TCP | `127.0.0.1` | oauth2-proxy lub VPN |
| OpenSearch | 9200/9600 | TCP | `127.0.0.1` tylko diagnostycznie | `telemetry-net` |
| OpenSearch Dashboards | 5601 | TCP | `127.0.0.1` | oauth2-proxy/Caddy |
| Mailpit SMTP/UI | 1025/8025 | TCP | `127.0.0.1` | nie uruchamiać w prod |

Porty management i health nie powinny być dostępne publicznie.

## Ingress

- Publiczne są tylko Caddy 80/443 oraz jawnie wymagane porty WebRTC/TURN.
- API weryfikuje sesję/token niezależnie od reverse proxy.
- UI operatorskie korzystają z oauth2-proxy i dedykowanych ról Keycloak.
- Nie używamy `tls_insecure_skip_verify`.
- Caddy ustawia poprawne trusted proxies; nie ufa dowolnym `X-Forwarded-*`.
- WebSocket i SignalR mają limity czasu, rozmiaru wiadomości i origin check.

## Egress Browser Workera

Playwright route interception jest pierwszą warstwą polityki, lecz nie wystarcza jako granica sieciowa. Docelowo:

1. Worker zna origin startowy zatwierdzony dla zadania.
2. Policy Engine wylicza czasową listę hostów i dozwolonych protokołów.
3. Egress gateway/DNS/firewall egzekwuje listę niezależnie od LLM i strony.
4. Zmiana originu wymaga zdarzenia polityki; tekst strony nie może jej rozszerzyć.
5. Blokowane są adresy loopback, link-local, prywatne zakresy, metadata endpoints i wewnętrzne DNS, o ile dany skill jawnie ich nie wymaga.
6. Pobrania są skanowane przed udostępnieniem aplikacji lub użytkownikowi.

W szczególności blokujemy SSRF do:

```text
127.0.0.0/8
10.0.0.0/8
172.16.0.0/12
192.168.0.0/16
169.254.0.0/16
::1/128
fc00::/7
fe80::/10
```

Wyjątek dla prywatnego IFS jest konfigurowany jako jawny environment profile i nie otwiera całej sieci prywatnej.

## DNS

- Kontenery korzystają z kontrolowanego resolvera.
- Worker nie może sam zmienić DNS ani użyć DoH poza allowlistą.
- Nazwy usług Compose nie są rozwiązywane z `browser-egress-net`, poza dedykowanym gatewayem.
- Rebinding DNS musi być uwzględniony: reguła sprawdza host i wynikowe IP przy każdym nowym połączeniu.

## TLS i mTLS

- Publiczny TLS kończy Caddy.
- Control plane i połączenia między hostami używają TLS/mTLS.
- Certyfikaty mają automatyczną rotację i alert przed wygaśnięciem.
- Klucze prywatne są montowane jako pliki secret albo pobierane z OpenBao, nie jako zwykłe env.
- W sieci lokalnego Compose można świadomie użyć plaintext tylko wewnątrz `internal: true`, ale nie wolno tego przenieść bez zmian do multi-host.

## Testy sieciowe

Po starcie należy potwierdzić:

```bash
curl --fail --silent https://praxiara.local/health/ready
```

```bash
docker compose exec browser-worker sh -lc 'nc -z postgres 5432 && exit 1 || exit 0'
```

```bash
docker compose exec browser-worker sh -lc 'nc -z openbao 8200 && exit 1 || exit 0'
```

Negatywne testy są równie ważne jak pozytywne: Browser Worker ma nie móc połączyć się z data/control, a Internet nie ma widzieć portów danych i management.

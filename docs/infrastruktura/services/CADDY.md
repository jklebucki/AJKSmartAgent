# Caddy — ingress, TLS i reverse proxy

## Cel i zakres

Caddy jest jedynym zwykłym publicznym ingress dla HTTP/HTTPS: React UI, API, SignalR/WebSocket, OIDC callbacks, LiveKit signaling i chronione UI operatorskie. Nie jest forward proxy Browser Workera ani proxy dla całego ruchu UDP WebRTC.

## Wybór i licencja

Caddy jest Apache-2.0 i automatyzuje HTTPS/certyfikaty. Używamy stockowego obrazu. Każdy dodatkowy moduł budowany przez `xcaddy` ma własną licencję i wymaga przeglądu.

Źródła: [Caddy](https://github.com/caddyserver/caddy), [automatic HTTPS](https://caddyserver.com/docs/automatic-https), [reverse_proxy](https://caddyserver.com/docs/caddyfile/directives/reverse_proxy), [forward_auth](https://caddyserver.com/docs/caddyfile/directives/forward_auth).

## Wersja i pinning

- Aktualne stabilne Caddy 2.x, przypięte po digest.
- Config jest wersjonowany, bez sekretów.
- Cert storage ma trwały volume.
- Nie pobieramy pluginów podczas startu.
- `caddy validate` jest obowiązkowe w CI i przed reload.

## Topologie

### Development

- local hostname, local CA albo jawny HTTP wyłącznie dla krótkiego testu;
- porty 80/443 na localhost/hoście;
- UI usług developerskich może być routowane po osobnych subdomenach.

### Hardened single-node

- Caddy na publicznych 80/443;
- automatyczny ACME lub certyfikat organizacji;
- trwały `/data` i config read-only;
- security headers, body/rate limits i health checks;
- upstreamy tylko w prywatnych sieciach;
- oauth2-proxy dla UI operatorskich.

### HA

- co najmniej dwie instancje za L4/load balancerem albo osobny edge tier;
- współdzielony/koordynowany cert storage zgodnie ze wspieraną metodą;
- config rollout i health per node;
- test ACME challenge/failover;
- nie zakładamy, że dwa Caddy na jednym hoście to HA.

## Zasoby startowe

0.5–1 CPU i 128–512 MiB RAM wystarcza jako punkt startowy; WebSocket count, response buffering, TLS handshakes i upload size wymagają benchmarku. Cert storage jest mały, ale trwały.

## Routing docelowy

Przykładowe hosty:

```text
app.example.com       -> Praxiara UI/API
auth.example.com      -> Keycloak
stream.example.com    -> LiveKit signaling
ops.example.com       -> oauth2-proxy -> operator UI routes
```

API i UI najlepiej mają wspólny origin, ograniczając CORS. WebSocket/SignalR ma jawne timeouts i origin validation.

Caddyfile skeleton:

```caddyfile
app.example.com {
    encode zstd gzip

    @api path /api/* /hubs/* /health/*
    handle @api {
        reverse_proxy praxiara-api:8080
    }

    handle {
        reverse_proxy praxiara-web:8080
    }
}
```

Produkcja uzupełnia log redaction, security headers, request limits i trusted proxies. Przykład nie jest kompletnym configiem.

## oauth2-proxy dla ops

Caddy używa `forward_auth` albo routuje cały host do oauth2-proxy. Backend nie ufa dowolnym identity headers; Caddy usuwa nagłówki klienta o tych samych nazwach i przyjmuje je wyłącznie od oauth2-proxy.

Role dla UI są dedykowane. Dostęp do Temporal/OpenSearch nie wynika automatycznie z `praxiara-user`.

## Sekrety i konfiguracja

```text
/run/secrets/caddy_acme_email
/run/secrets/caddy_dns_api_token    # tylko jeśli świadomie używany DNS challenge
/run/secrets/internal_ca_cert
/run/secrets/internal_ca_key        # najlepiej poza edge, jeśli możliwe
```

DNS plugin nie jest częścią stock Caddy i wymaga przeglądu licencji. Preferujemy HTTP-01/TLS-ALPN-01, jeśli topologia pozwala.

## Sieci i porty

- 80/TCP publiczny dla redirect/ACME.
- 443/TCP i opcjonalnie 443/UDP dla HTTPS/HTTP3.
- upstreamy przez `edge-net`; Caddy nie należy do `data-net`.
- management API Caddy niepubliczne i ograniczone do localhost/private control.
- porty LiveKit media/TURN są obsługiwane osobno.

## Uruchomienie

Walidacja:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile edge \
  run --rm caddy caddy validate --config /etc/caddy/Caddyfile
```

Start:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile edge \
  up -d --wait caddy
```

Reload bez przerwy:

```bash
docker compose exec caddy caddy reload --config /etc/caddy/Caddyfile
```

## Health i smoke test

```bash
curl --fail --silent https://app.example.com/health/live
curl --fail --silent https://app.example.com/health/ready
```

Testy:

- HTTP przekierowuje do HTTPS;
- cert chain i hostname są poprawne;
- TLS 1.2+ według polityki;
- security headers obecne;
- API, SPA fallback i static assets działają;
- WebSocket/SignalR upgrade działa;
- OIDC callback nie ma redirect loop;
- ops route: 302 bez sesji, 403 bez roli, 200 z rolą;
- bezpośredni upstream port nie jest publiczny;
- request ponad limit jest odrzucony.

## Logi i prywatność

- access logs są JSON i wysyłane do OTel/filelog.
- Authorization, Cookie, Set-Cookie, code, token i signed URL query są redagowane.
- Client IP jest danymi potencjalnie osobowymi; retencja i dostęp są ograniczone.
- Nie logujemy pełnych query strings dla callbacków i presigned URLs.

## Backup i restore

Backupujemy:

- Caddyfile/config;
- image digest;
- cert storage, jeśli wymagany do szybkiego RTO;
- informacje o ACME account zgodnie z polityką;
- zewnętrzne cert/key poza repo i zgodnie z secret policy.

Config jest odtwarzalny z Git. Publiczne certyfikaty mogą zostać ponownie wydane, ale rate limits i outage uzasadniają ochronę `/data`. Prywatne CA keys wymagają silniejszego, osobnego backupu.

Restore: odtwórz config i secret/cert storage, uruchom `caddy validate`, start na testowym adresie, sprawdź TLS/routing, potem przełącz ruch.

## Aktualizacja i rollback

1. Pin nowego obrazu i skan.
2. `caddy validate` na całym config.
3. Staging smoke test HTTP/WebSocket/OIDC/ops.
4. Backup `/data`/config.
5. Canary lub pojedynczy edge.
6. Rollback do starego digest/config, jeśli cert storage pozostaje kompatybilny.

Config change i binary upgrade powinny być osobnymi change, jeśli nie są nierozłączne.

## Hardening

- stock image, brak przypadkowych pluginów;
- non-root lub tylko minimalna capability bind low ports;
- config read-only, `/data` i `/config` kontrolowane;
- admin API niepubliczne;
- HSTS po potwierdzeniu poprawnego HTTPS;
- CSP, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`;
- CORS exact origins;
- body/header/time/rate limits;
- trusted proxies tylko znane CIDR;
- upstream TLS zweryfikowane, bez `tls_insecure_skip_verify`;
- identity headers oczyszczone;
- sensitive query/header redaction;
- cert expiry i ACME errors alertowane.

## Troubleshooting

### 502 upstream

Sprawdź readiness, nazwę sieciową, port, protocol HTTP/h2c/HTTPS i upstream cert. Nie wyłączaj TLS verification jako trwałej poprawki.

### ACME rate limit

Używaj staging CA w testach, trwałego `/data` i nie kasuj kontenera/volume przy każdym deploy. Sprawdź DNS i dostępność challenge.

### WebSocket rozłącza się

Sprawdź proxy/read timeouts, HTTP version, idle policy, load balancer przed Caddy i token/session TTL.

### Redirect loop Keycloak/oauth2-proxy

Sprawdź public scheme/host, forwarded headers, callback URL i secure cookie. Nie ustawiaj skip issuer/cert verification.

### Prawdziwy client IP jest fałszywy

Nie ufaj `X-Forwarded-For` od Internetu. Skonfiguruj `trusted_proxies` tylko dla realnego upstream load balancera.

## Różnice produkcyjne

Produkcja używa publicznego TLS, trwałego cert storage, security headers, oauth2-proxy, prywatnych upstreamów, log redaction i limitów. Dev może używać local CA i portów localhost. HA wymaga co najmniej dwóch edge nodes na osobnych hostach lub zewnętrznego load balancera.

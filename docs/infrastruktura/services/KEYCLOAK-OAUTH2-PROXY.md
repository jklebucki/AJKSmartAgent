# Keycloak i oauth2-proxy

## Cel i zakres

Keycloak jest dostawcą tożsamości Praxiara. Uwierzytelnia użytkowników i operatorów, wydaje tokeny OIDC oraz role/grupy. Nie przechowuje loginów ani haseł do IFS. Sesja IFS pozostaje w izolowanym Browser Workerze po ręcznym logowaniu i MFA użytkownika.

oauth2-proxy chroni UI operatorskie, które same nie implementują OIDC, na przykład Temporal UI, Jaeger, Prometheus i OpenSearch Dashboards. API Praxiara nadal samodzielnie waliduje tożsamość; reverse proxy nie jest jedyną warstwą auth.

## Wybór i licencje

- Keycloak: Apache-2.0.
- oauth2-proxy: MIT.

Oba są bezpłatne w użyciu komercyjnym. Używamy community images, nie płatnych dystrybucji/support bundle.

Źródła: [Keycloak](https://github.com/keycloak/keycloak), [Keycloak Server Guide](https://www.keycloak.org/guides), [oauth2-proxy](https://github.com/oauth2-proxy/oauth2-proxy), [integracja Keycloak OIDC](https://oauth2-proxy.github.io/oauth2-proxy/configuration/providers/keycloak_oidc/).

## Wersja i pinning

- Początkowa linia testowa Keycloak: aktualne stabilne `26.x`.
- oauth2-proxy: aktualne stabilne `7.x`.
- Obrazy przypięte po digestach.
- Upgrade Keycloak obejmuje także theme/provider extensions; domyślnie unikamy własnych provider JAR.
- Realm JSON w repo jest szablonem bez sekretów i użytkowników produkcyjnych.

## Topologie

### Development

- jedna instancja Keycloak w trybie developerskim tylko lokalnie;
- PostgreSQL, nie H2, jeśli test ma wykrywać problemy produkcyjne;
- realm import idempotentny;
- Mailpit jako SMTP;
- Keycloak i oauth2-proxy dostępne tylko przez `127.0.0.1`/Caddy local TLS.

### Hardened single-node

- zoptymalizowany build/start produkcyjny, nie `start-dev`;
- osobna baza `keycloak` i rola;
- poprawny publiczny hostname, proxy headers i TLS na Caddy;
- management/health port tylko w `control-net`;
- SMTP produkcyjny dostarczony przez operatora;
- backup bazy i eksport konfiguracji jako dodatkowy artefakt.

### HA

- co najmniej dwie instancje Keycloak na osobnych hostach;
- współdzielona HA PostgreSQL;
- sticky sessions tylko jeśli wymaga tego konkretna konfiguracja; preferować poprawną dystrybucję cache/session;
- load balancer/Caddy z health checks;
- realm migration wykonana raz, przed ruchem;
- test failover aktywnej sesji, refresh tokenu i logout.

## Zasoby startowe

| Usługa | Dev | Single-node prod |
|---|---|---|
| Keycloak | 1 CPU, 1 GiB RAM | 2 CPU, 2–4 GiB RAM |
| oauth2-proxy | 0.1 CPU, 64–128 MiB | 0.25 CPU, 128–256 MiB |

Keycloak jest aplikacją JVM; limit heap musi uwzględniać native overhead i limit kontenera. Sizing zależy od logowań, liczby aktywnych sesji, MFA i federation.

## Realm i klienci

Docelowy realm: `praxiara`.

Minimalne clients:

| Client | Typ | Przeznaczenie |
|---|---|---|
| `praxiara-web` | public lub BFF-confidential | aplikacja użytkownika, Authorization Code + PKCE |
| `praxiara-api` | resource/audience | audience access tokenu API |
| `praxiara-ops-proxy` | confidential | oauth2-proxy dla UI operatorskich |
| `praxiara-cli` | public, opcjonalny | device/authorization flow po świadomej decyzji |

Minimalne role:

```text
praxiara-user
praxiara-skill-author
praxiara-approver
praxiara-auditor
praxiara-operator
praxiara-admin
```

Role nie są hierarchicznie nadawane bez jawnej composite definition. `praxiara-admin` nie oznacza administratora realm Keycloak.

## Przepływ aplikacji

Preferowany jest BFF:

1. React rozpoczyna login przez endpoint backendu.
2. Backend wykonuje Authorization Code + PKCE.
3. Tokeny pozostają po stronie serwera.
4. Browser otrzymuje cookie `HttpOnly`, `Secure` i właściwe `SameSite`.
5. API wiąże sesję z user ID, tenant i aktualną polityką.

Jeżeli SPA używa public client bez BFF, access token jest przechowywany tylko w pamięci, nie w `localStorage`/`sessionStorage`, a refresh i XSS risk mają osobny threat model.

## oauth2-proxy

Każde UI operatorskie ma osobny upstream/path lub osobną instancję policy. Konfiguracja obejmuje:

```text
provider=keycloak-oidc
oidc_issuer_url=https://auth.example.com/realms/praxiara
client_id=praxiara-ops-proxy
client_secret_file=/run/secrets/oauth2_proxy_client_secret
code_challenge_method=S256
cookie_secure=true
cookie_httponly=true
allowed_role=praxiara-operator
```

Nie używamy wildcard redirect URI ani `email-domain=*` jako substytutu role check. Issuer, audience i role są dokładnie walidowane.

## Sekrety i konfiguracja

Oczekiwane pliki:

```text
/run/secrets/keycloak_db_password
/run/secrets/keycloak_bootstrap_admin_password
/run/secrets/oauth2_proxy_client_secret
/run/secrets/oauth2_proxy_cookie_secret
```

Bootstrap admin jest krótkotrwały i usuwany/wyłączany po inicjalizacji. Produkcyjny admin używa indywidualnego konta z MFA.

Konfiguracja API:

```text
Authentication__Authority=https://auth.example.com/realms/praxiara
Authentication__Audience=praxiara-api
Authentication__RequireHttpsMetadata=true
Authentication__ClockSkewSeconds=30
```

Konfiguracja Keycloak obejmuje hostname, proxy headers, health/metrics i połączenie z dedykowaną bazą. Sekret DB jest odczytywany z pliku lub dostarczany dynamicznie.

## Sieci i porty

- Keycloak application port 8080 za Caddy.
- Management/health 9000 wyłącznie `control-net`.
- oauth2-proxy 4180 między Caddy a upstream.
- PostgreSQL 5432 tylko `data-net`.
- Dev mapping wyłącznie na `127.0.0.1`.
- Admin Console nie jest dostępna bez VPN/roli operatorskiej.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile identity \
  up -d --wait keycloak oauth2-proxy
```

Idempotentny realm init:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm keycloak-init
```

Init porównuje wymagane clients/roles/scopes i nie resetuje istniejących secretów ani użytkowników.

## Health i smoke test

OIDC discovery:

```bash
curl --fail --silent \
  https://auth.example.com/realms/praxiara/.well-known/openid-configuration \
  | jq -e '.issuer and .authorization_endpoint and .token_endpoint and .jwks_uri'
```

Smoke test obejmuje:

- poprawny Authorization Code + PKCE;
- token z właściwym issuer, audience i role;
- odmowę tokenu bez audience/roli;
- refresh token rotation;
- logout i unieważnienie sesji;
- oauth2-proxy: 302 do logowania bez sesji, 403 bez roli, 200 z rolą;
- MFA operatora.

Automatyczny test nie umieszcza hasła użytkownika w logach CI.

## Backup i restore

Źródłem prawdy jest baza `keycloak`; realm export jest dodatkową kopią konfiguracji i materiałem do diff/review, nie pełnym backupem sesji i credentials.

Backup:

- pgBackRest obejmuje bazę Keycloak;
- realm template bez sekretów jest w Git;
- okresowo wykonywany jest bezpieczny export konfiguracji;
- custom themes/providers są wersjonowane razem z obrazem;
- klucze/recovery i sekrety klientów są chronione zgodnie z polityką.

Restore:

1. Odtwórz PostgreSQL do spójnego punktu.
2. Uruchom Keycloak odcięty od użytkowników.
3. Zweryfikuj realm, clients, keys, roles i użytkowników.
4. Zweryfikuj issuer/hostname i redirect URIs.
5. Wykonaj pełny login/refresh/logout i MFA.
6. Dopiero potem otwórz ruch.

## Aktualizacja i rollback

- Przeczytaj Keycloak upgrading guide i zmiany schematu.
- Wykonaj backup/restore rehearsal.
- Testuj realm, theme, mappers i oauth2-proxy na staging.
- Przy rolling upgrade sprawdź wsparcie mixed version.
- Po migracji schematu rollback obrazu może być niemożliwy; planem jest restore starej bazy/klastra.
- oauth2-proxy można cofnąć niezależnie tylko po teście cookie/session compatibility.

## Hardening

- `start-dev` zabronione w produkcji.
- TLS i ścisły hostname/proxy config.
- brak wildcard redirect URIs i web origins.
- PKCE S256, state, nonce i dokładny issuer/audience.
- krótki access token, rotowany refresh token.
- MFA/passkeys dla operatorów/approverów.
- ograniczony brute-force protection i rate limiting.
- bootstrap admin po init wyłączony.
- osobny client oauth2-proxy i rotowany cookie secret.
- admin/metrics/health poza publicznym routingiem.
- logi nie zawierają tokenów, authorization code ani cookies.

## Troubleshooting

### Redirect loop

Sprawdź publiczny hostname, `X-Forwarded-Proto`, trusted proxies, redirect URI, secure cookie i różnicę HTTP/HTTPS. Nie naprawiaj przez wyłączenie weryfikacji issuer lub secure cookie.

### 401 mimo poprawnego loginu

Sprawdź `iss`, `aud`, expiry, clock skew, signing algorithm i JWKS rotation. API nie powinno akceptować tokenu przeznaczonego wyłącznie dla innego clienta.

### oauth2-proxy zwraca 403

Sprawdź mapper roles/groups, `aud`, full scope i rzeczywisty claim. Nie rozszerzaj tymczasowo do wszystkich zalogowanych użytkowników na produkcji.

### Keycloak nie startuje po upgrade

Zachowaj log i kopię bazy, sprawdź migration error i kompatybilność custom extensions. Nie uruchamiaj wielokrotnie różnych majorów na tej samej bazie bez procedury.

## Różnice produkcyjne

Produkcja używa zoptymalizowanego trybu, PostgreSQL, TLS, prawidłowego hostname, MFA, indywidualnych adminów, backupu i SMTP operatora. HA wymaga wielu instancji i HA bazy na osobnych hostach. Mailpit i bootstrap credentials pozostają wyłącznie w development.

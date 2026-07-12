# IFS OAuth i metadane OData

## Zakres

API Praxiary obsługuje wiele profili IFS przechowywanych w PostgreSQL. Profil zawiera identyfikator środowiska, bazowy HTTPS URL, tenant, locale, rodzaj środowiska, allowlistę projekcji oraz niesekretną konfigurację uwierzytelnienia. Tokeny, client secrets, cookies i odpowiedzi zawierające dane biznesowe nie trafiają do bazy, Reacta, logów ani audytu konfiguracji.

## Uwierzytelnienie runtime

Obsługiwane są dwa tryby:

- `ClientCredentials` — token OIDC jest pobierany z zatwierdzonego endpointu tokenowego na tym samym origin co profil IFS; `client_secret` jest odczytywany wyłącznie z pliku sekretu runtime;
- `BearerTokenFile` — krótkotrwały token testowy jest odczytywany wyłącznie z pliku sekretu runtime przy każdym żądaniu.

Plik sekretu musi leżeć poza repozytorium, mieć restrykcyjne uprawnienia i być dostarczony przez OpenBao, SOPS bootstrap albo mechanizm sekretów platformy, na przykład `/run/secrets/ifs_test_bearer_token`. Nie przekazuj tokenu w JSON API, query stringu, React, argumentach procesu ani `appsettings*.json`.

## API control plane

Wszystkie endpointy mają prefix `/api/v1/ifs/environments` i wymagają tożsamości aplikacyjnej:

| Endpoint | Rola | Znaczenie |
|---|---|---|
| `GET /` | `praxiara-operator` lub `praxiara-admin` | lista profili bez sekretu |
| `POST /` | `praxiara-admin` | utworzenie profilu |
| `PUT /{environmentId}` | `praxiara-admin` | zmiana profilu |
| `DELETE /{environmentId}` | `praxiara-admin` | usunięcie profilu |
| `GET /{environmentId}/projections/{projectionName}/metadata` | `praxiara-operator` lub `praxiara-admin` | odczyt `/$metadata` wyłącznie dla projekcji z allowlisty |

Nie istnieje endpoint będący dowolnym proxy HTTP/OData. Nieznane środowisko, projekcja spoza allowlisty, brak sekretu, błąd tokenu lub nieprawidłowe metadane kończą się odmową albo jawnym błędem, bez fallbacku UI.

## Baza i migracje

Migracja `InitialPersistence` tworzy profile IFS oraz append-only wpisy administracyjne z hashem konfiguracji bez sekretu. Aplikacja korzysta z `ConnectionStrings__Praxiara`; brak tego ustawienia zachowuje API uruchomione, lecz endpointy IFS odpowiadają `503 ifs_environment_storage_unavailable` po poprawnej autoryzacji. Migracje wykonuje osobny migrator, nie wszystkie repliki aplikacji.

## React

Panel „Środowiska IFS” odczytuje wyłącznie listę dozwolonych profili i pozwala operatorowi sprawdzić metadane wybranej projekcji. Nigdy nie przechowuje tokenu w `localStorage`, `sessionStorage` ani stanie komponentu.

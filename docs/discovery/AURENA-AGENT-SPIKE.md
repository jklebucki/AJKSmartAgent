# Spike Aurena Agent i zarządzanej stacji Windows

## Status

`DISC-011` pozostaje otwarty. W repozytorium nie ma dostępu do zarządzanej stacji Windows, prawdziwego IFS Cloud, konfiguracji Aurena Agent ani danych właściciela procesu. Zgodnie z zasadami IFS nie wykonano testu na produkcji ani na rzeczywistych danych klienta.

## Zakres sprawdzony lokalnie

- host lokalny: macOS/Darwin arm64;
- dostępny runtime: serwerowy Chromium przez Playwright;
- brak Windows companion, brak kanału urządzenia, brak mTLS device registration, brak listy komend Aurena Agent;
- dostępne dowody pośrednie: `docs/discovery/IFS-ROUTE-MATRIX.md`, `docs/discovery/PROCESY_REFERENCYJNE-IFS.md`, `docs/discovery/RAPORT-DISC-001-010.md`.

## Wniosek techniczny

Serwerowy Chromium nie jest zamiennikiem Aurena Agent. Funkcje zależne od lokalnego urządzenia użytkownika muszą być traktowane jako osobny wariant wykonawczy na zarządzanej stacji Windows. Brak companion oznacza wynik `unsupported/stop`, a nie fallback przez download, lokalną ścieżkę, shell, surowy Playwright albo dowolny JavaScript.

## Kategorie funkcji niemożliwych do potwierdzenia w tym środowisku

| Kategoria | Dlaczego Chromium nie wystarcza | Bezpieczne zachowanie Praxiary |
|---|---|---|
| lokalny wydruk lub integracja z drukarką użytkownika | wymaga urządzenia i sterownika poza kontenerem Browser Workera | odmowa lub przekazanie do zatwierdzonego Windows companion |
| otwieranie lokalnych aplikacji lub plików z IFS | wymaga kontrolowanego endpointu i allowlisty komend | `unsupported/stop` bez companion |
| operacje wymagające lokalnego agent extension | zależą od wersji extension/programu i polityki urządzenia | osobny profil środowiska z device evidence |
| zapis pliku na urządzeniu użytkownika | obejmuje dane, retencję i ścieżkę poza sandboxem | tylko przez jawny, audytowany kanał companion |

## Protokół testu wymagany do zamknięcia

1. Właściciel procesu wskazuje syntetyczny proces R4/R5 wymagający Aurena Agent.
2. Administrator IFS dostarcza sandbox, wersję IFS release update, profile uprawnień i dane testowe.
3. Administrator urządzeń dostarcza zarządzaną stację Windows z zainstalowanym i wersjonowanym companionem.
4. Security zatwierdza kanał device-to-control-plane: mTLS, TTL, allowlista komend, brak arbitrary shell, redakcja logów.
5. Test wykonuje tylko syntetyczne dane i zapisuje: wersję IFS, wersję extension, wersję companion, listę komend, approval, audit i wynik verifiera.

Do czasu wykonania powyższego protokołu `DISC-011` nie może zostać oznaczony jako wykonany.

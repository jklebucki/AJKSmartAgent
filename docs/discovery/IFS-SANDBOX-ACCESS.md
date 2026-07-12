# Dowód dostępu do sandboxu IFS

## Status

W dniu 2026-07-12 potwierdzono odczytowy dostęp do wskazanego przez użytkownika środowiska testowego IFS Cloud przez standardową ścieżkę landing page → `IFS Cloud` → logowanie. Aplikacja uruchomiła stronę startową IFS Cloud w polskiej lokalizacji.

Ten dokument nie zawiera poświadczeń, tokenów, cookies, identyfikatorów sesji ani danych rekordów biznesowych.

## Zakres wykonanej weryfikacji

- użyto wyłącznie konta wskazanego przez użytkownika i interfejsu przeglądarkowego;
- potwierdzono, że środowisko testowe przyjmuje logowanie oraz wyświetla stronę startową;
- sprawdzono widoczną nawigację stron pod kątem `API Explorer` i `Projection`;
- nie wysłano formularza biznesowego, nie zmieniono rekordu, nie pobrano pliku i nie uruchomiono procesu;
- nie utrwalono stanu uwierzytelnienia ani artefaktów sesji.

## Wynik dla `DISC-010`

Środowisko jest dostępne do dalszego, kontrolowanego discovery. W widocznej nawigacji konta nie odnaleziono jednak zatwierdzonego API Explorer ani eksportu `$openapi`. Dostęp UI nie zastępuje profilu środowiska, rejestru projekcji ani potwierdzenia permission.

## Spike strony `PurchaseOrder`

W dniu 2026-07-12 sprawdzono wskazaną stronę `PurchaseOrder` wyłącznie odczytowo. Formularz i lista zamówień ujawniają stabilne nazwy dostępne dla obserwacji semantycznej, między innymi „Nr zamówienia”, „Dostawca”, „Status”, „Linie pozycji” i „Szczegóły zamówienia”. Skrócone wyszukiwanie `zam zak` zwróciło powiązane strony, w tym formularz i listę zamówień zakupu oraz linie i narzuty zamówień.

Zgodnie z przekazaną wskazówką wykonano jedną próbę odczytu `$metadata` dla kandydata projekcji odpowiadającego stronie. Klient przeglądarkowy zakończył ją błędem `ERR_BLOCKED_BY_CLIENT` przed uzyskaniem odpowiedzi serwera. Nie jest to dowód, że endpoint nie istnieje, nie daje HTTP statusu ani nie jest podstawą do wpisu w registry. Nie wykonano skanowania innych nazw ani obejścia blokady.

## Weryfikacja `UserProfileService`

Użytkownik wskazał działający wzorzec funkcji `UserProfileService.svc/GetProfileSectionValues(...)` oraz regułę sprawdzania metadanych przez dopisanie `/$metadata`. Wykonano wyłącznie odczytowe sprawdzenie wskazanego endpointu metadanych, bez przekazania danych logowania i bez pobrania wartości profilu.

- klient wbudowanej przeglądarki zablokował bezpośrednie otwarcie tej ścieżki przez `ERR_BLOCKED_BY_CLIENT`, ponownie przed odpowiedzią serwera; powtórzenie po potwierdzonym aktywnym logowaniu dało identyczny wynik;
- niezależny `HEAD` do dokładnego URL `UserProfileService.svc/$metadata` otrzymał `401 Unauthorized` oraz `WWW-Authenticate: Bearer`;
- serwer wymaga więc tokenu Bearer dla tego zasobu. Hasło do formularza webowego nie jest bezpiecznym ani potwierdzonym substytutem tego tokenu;
- odpowiedź `401` potwierdza wymaganie uwierzytelnienia na ścieżce, ale bez tokenu nie dowodzi zawartości dokumentu metadanych ani konkretnej definicji usługi.

Wynik jest zgodny z dokumentacją IFS: projekcje są eksponowane pod `/ifsapplications/projection/`, a ich uprawnienia kontrolują odczyt i dozwolone akcje ([IFS — Projection](https://docs.ifs.com/techdocs/24r2/030_administration/010_security/020_permission_sets/004_permission_set_overview/010_projections/)).

`DISC-010` pozostaje otwarte. Do jego zamknięcia administrator IFS lub właściciel procesu musi przekazać:

1. zatwierdzony eksport `$openapi` z checksumą i datą;
2. profil sandboxu: środowisko, tenant, locale, wspierany release oraz dozwolone hosty;
3. potwierdzenie uprawnień użytkownika testowego do konkretnych projection/entity/action;
4. wskazanie syntetycznych rekordów i niezależnego verifiera dla procesów R0, R2 i R4;
5. krótkotrwały token Bearer delegowany dla konta testowego lub zatwierdzony sposób pobrania go przez administratora, bez przekazywania tokenu modelowi;
6. dokładny, zatwierdzony URL metadata albo dostęp do API Explorer, jeśli klient środowiska ma dalej blokować takie odczyty;
7. zgodę eksperta IFS na wpisanie potwierdzonych nazw do registry.

Nie należy zgadywać endpointów OData ani odkrywać ich w runtime na podstawie adresu aplikacji.

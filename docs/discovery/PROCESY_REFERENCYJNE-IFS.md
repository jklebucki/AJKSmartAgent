# Procesy referencyjne IFS dla discovery

## Założenia wspólne

Procesy używają wyłącznie syntetycznych rekordów `1001`–`1006` z `Praxiara.TestSites`. Nazwy projection, entity set, action i permission nie są zakładane; muszą zostać potwierdzone z zatwierdzonego `$openapi` sandboxu IFS. Brak potwierdzonego kontraktu oznacza `deny/stop`.

## `DISC-002` — R0: odczyt faktury klienta

| Pole | Kontrakt |
|---|---|
| Nazwa biznesowa | Znajdź fakturę klienta i pokaż jej zweryfikowany stan |
| Wejścia | `environmentId`, `customerInvoiceId` o długości 1–50 |
| Target | dokładnie jedna faktura klienta |
| Wynik | identyfikator, customer, status, kwota, waluta, revision/ETag jeśli dostępny |
| Ryzyko | R0, odczyt bez nawigacji skutkowej |
| Dane | Business/Internal; bez danych płatniczych i sekretów |
| Oczekiwane błędy | `not_found`, `permission_denied`, `session_expired`, `contract_mismatch`, `environment_unavailable` |
| Verifier | ponowny odczyt tego samego rekordu i zgodność identity/status/amount/currency |
| Audit | user, tenant, environment, projection/route, target, zredagowane query, wynik i verifier |

Kryterium sukcesu: wynik pochodzi z jednego jawnego środowiska i jednego rekordu, a verifier potwierdza identity oraz wartości. Samo znalezienie tekstu w UI nie wystarcza.

## `DISC-003` — R2: przygotowanie podglądu dostarczenia faktury

| Pole | Kontrakt |
|---|---|
| Nazwa biznesowa | Przygotuj podgląd danych potrzebnych do dostarczenia faktury bez zapisu |
| Wejścia | `environmentId`, `customerInvoiceId` |
| Target | dokładnie jedna faktura i skonfigurowany kanał odbiorcy |
| Wynik | snapshot statusu, odbiorcy w postaci zredagowanej, kanału, wersji dokumentu i brakujących preconditions |
| Ryzyko | R2, przetwarzanie danych bez zapisu |
| Dane | Confidential; adres odbiorcy redagowany w telemetry i promptach |
| Oczekiwane błędy | błędy R0 oraz `recipient_missing`, `document_not_ready`, `unsupported_delivery_channel` |
| Verifier | drugi niezależny odczyt identity, statusu dokumentu i konfiguracji kanału |
| Audit | źródła danych, route, wersja kontraktu, klasyfikacja i wynik preconditions |

Kryterium sukcesu: powstaje wyłącznie preview. Nie tworzy się draftu, komunikacji, wpisu historii ani zmiany rekordu.

## `DISC-004` — R4: wysłanie faktury do zatwierdzonego odbiorcy

| Pole | Kontrakt |
|---|---|
| Nazwa biznesowa | Wyślij konkretną wersję faktury do skonfigurowanego odbiorcy |
| Wejścia | `environmentId`, `customerInvoiceId`, `documentRevision`, `deliveryChannel`, `recipientIdentityHash` |
| Target | jedna faktura, jedna wersja dokumentu i jeden odbiorca |
| Skutek | zewnętrzne dostarczenie dokumentu oraz wpis historii komunikacji |
| Ryzyko | R4, nieodwracalny skutek biznesowy |
| Preconditions | permission, stan faktury, gotowy dokument, zgodny odbiorca, środowisko, brak wcześniejszej realizacji idempotency key |
| Approval preview | środowisko, rekord, wersja dokumentu, kanał, zredagowany odbiorca, wartości przed/po i konsekwencja |
| Action hash | tenant, user, task, session, tool, canonical args, target, environment, revision, observation, skill/policy/catalog versions, nonce i expiry |
| Idempotencja | klucz związany z fakturą, wersją dokumentu, odbiorcą i execution attempt; timeout po send daje `OutcomeUnknown` |
| Verifier | świeży odczyt rekordu oraz niezależny wpis historii/communication ID; dla Hybrid również komunikat i stan UI |
| Recovery | bez automatycznego ponownego wysłania; reconciliation lub decyzja człowieka |

Kryterium sukcesu: ważne approval zostało atomowo zużyte, verifier potwierdził dokładny target i historię, a audit zawiera pełny evidence chain bez treści dokumentu i danych odbiorcy.

## Status akceptacji

Kontrakty są gotowe do review, ale `DISC-002`–`DISC-004` pozostają otwarte do czasu podpisu eksperta IFS oraz właściciela procesu finansowego wskazanych w `OWNERSHIP.md`.

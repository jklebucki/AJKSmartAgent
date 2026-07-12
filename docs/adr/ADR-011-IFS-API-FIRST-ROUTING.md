# ADR-011: Routing IFS API-first

- Status: proponowany, wymaga zatwierdzonego kontraktu IFS sandboxu
- Data propozycji: 2026-07-12
- Zakres: wybór trasy Projection/OData, Browser albo Hybrid dla operacji IFS

## Kontekst

`DISC-010` nie może zostać zamknięty bez zatwierdzonego `$openapi`, profilu sandboxu IFS i permission check. Wstępna route matrix jest fail-closed: wszystkie bieżące trasy to `deny/stop`. IFS wymaga API-first, a UI jest fallbackiem tylko po jawnej ocenie.

## Proponowana decyzja

1. Każda wysokopoziomowa operacja IFS ma jedną zatwierdzoną trasę w wersjonowanym registry: `ProjectionApi`, `Browser` albo `Hybrid`.
2. Router wybiera trasę na podstawie zatwierdzonego profilu środowiska, registry projection, permissions użytkownika, policy i kompatybilności IFS; model nie wybiera URL, OData ani niskopoziomowego transportu.
3. `ProjectionApi` jest wybierane, gdy API realizuje operację i weryfikację; `Hybrid`, gdy API daje odczyt/preview/verify, a niezbędny commit wymaga UI; `Browser` tylko przy braku wspieranego API i kompletnej, zatwierdzonej ścieżce UI.
4. Nie istnieje cichy fallback po `403`, błędzie biznesowym, kontrakt mismatch albo niepewnym wyniku. Brak kontraktu, permission, verifiera lub companion oznacza `deny/stop` albo `unsupported/stop`.
5. Import `$openapi` jest procesem administracyjnym z allowlisted hostem, limitem payloadu, checksumą i review; pojawienie się endpointu nie publikuje go automatycznie.

## Konsekwencje

- Wymagane są immutable profile IFS z `BaseUri`, tenantem, locale, kind, allowlistą, wersją release i modelem delegacji tożsamości.
- Klient OData waliduje projection/entity/action/query/next link przed wysłaniem; nie może być dowolnym proxy HTTP.
- R4/R5 muszą ponownie sprawdzić target i permission, użyć idempotency key oraz potwierdzić wynik niezależnym odczytem API/historii; `Hybrid` dodatkowo sprawdza UI.

## Warunki akceptacji

- `DISC-010` otrzymuje zatwierdzony `$openapi`, profil sandboxu, permissions i podpis eksperta IFS;
- registry i router mają testy każdej trasy oraz fail-closed dla hosta, projection, action i brakującego companion;
- testy sandboxowe pokrywają `permission_denied`, `session_expired`, `contract_mismatch`, conflict, błąd biznesowy i `OutcomeUnknown`.

Do spełnienia warunków `GOV-012` pozostaje otwarte.

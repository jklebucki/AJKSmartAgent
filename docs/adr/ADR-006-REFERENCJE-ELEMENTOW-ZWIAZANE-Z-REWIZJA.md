# ADR-006: Referencje elementów związane z rewizją obserwacji

- Status: proponowany, wymaga potwierdzenia semantyki na sandboxie IFS
- Data propozycji: 2026-07-12
- Zakres: obserwacje strony i akcje Browser Workera

## Kontekst

Dynamiczne gridy, wirtualizacja i popupy powodują, że locator lub element znaleziony wcześniej może wskazać inny obiekt po zmianie strony. `DISC-007` potwierdził tę klasę problemów na syntetycznej stronie, ale nie na rzeczywistym IFS.

## Proponowana decyzja

1. Każda obserwacja ma monotoniczne `pageRevision`, content hash i ograniczony rozmiar.
2. Referencja elementu zawiera identity sesji i strony, `pageRevision`, stabilną semantykę oraz opis celu biznesowego; nie jest locator stringiem przekazywanym przez model.
3. Typed action wymaga expected revision równej bieżącej rewizji. Różnica zwraca `stale_reference` i wymusza nową obserwację.
4. Nie ma automatycznego retry kliknięcia po `stale_reference`; planner otrzymuje świeże, oznaczone dane.
5. Takeover, popup, nawigacja, zamknięcie karty i zmiana ownera unieważniają referencje poprzedniej rewizji.

## Konsekwencje

- Kontrakty action/observation i approval hash muszą zawierać revision oraz observation hash.
- Testy obejmują wirtualizację, zmianę okna, iframe, popup i blokadę inputu na stale reference.
- Wymaga to krótkotrwałych referencji i re-observe, co zwiększa liczbę bezpiecznych kroków.

## Warunki akceptacji

- `DISC-007` jest potwierdzony przez eksperta IFS na zatwierdzonym sandboxie;
- istnieją deterministyczne testy stale reference dla każdej typed action;
- kontrakt nie eksponuje surowego XPath, CSS ani `ElementHandle` modelowi.

Do spełnienia warunków `GOV-007` pozostaje otwarte.

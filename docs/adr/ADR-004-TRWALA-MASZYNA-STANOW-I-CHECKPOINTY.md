# ADR-004: Trwała maszyna stanów i checkpointy

- Status: proponowany, zależny od zamknięcia Fazy 0
- Data propozycji: 2026-07-12
- Zakres: lifecycle zadania, retry, recovery i oczekiwanie na decyzję człowieka

## Kontekst

Automatyzacja może zostać przerwana przez utratę workera, wygasłą sesję, approval, takeover, timeout po skutku albo niedostępny verifier. Plan produktu definiuje jawne stany zadania, w tym `AwaitingApproval`, `ManualTakeover`, `Recovering` i `OutcomeUnknown`.

## Proponowana decyzja

1. Orchestrator utrzymuje jawną domenową maszynę stanów; trwały runtime workflow nie zastępuje reguł przejść.
2. Każdy checkpoint zapisuje immutable stan wejściowy, task/run/step identity, wersje skill/policy/catalog/modelu, limity, correlation ID i odwołania do zredagowanych evidence.
3. Wznowienie rozpoczyna się od walidacji checkpointu, ownera, sesji, bieżącej policy i ważności approval; nie odtwarza ślepo ostatniego inputu.
4. Stan terminalny nie ma przejść wychodzących. `Completed` wymaga pozytywnego verifiera; `OutcomeUnknown` nie uruchamia automatycznego retry skutku.
5. Operacja nieidempotentna po timeoutcie po wysłaniu przechodzi do rekoncyliacji lub decyzji człowieka.

## Konsekwencje

- Potrzebne są append-only zdarzenia, idempotency keys, migracje trwałego stanu i kontrakty kompatybilne między workerami.
- Retry jest klasyfikowany, ograniczony i audytowany; worker crash nie może raportować sukcesu.
- Pausa nie wydłuża automatycznie TTL sesji ani approval.

## Warunki akceptacji

- zamknięto `DISC-018` bez krytycznych blockerów;
- testy pokrywają restart po checkpointcie, terminal state, `OutcomeUnknown`, wygasły approval i utratę workera;
- właściciel platformy zatwierdza docelowy runtime trwałych workflow oraz politykę retencji checkpointów.

Do spełnienia warunków `GOV-005` pozostaje otwarte.

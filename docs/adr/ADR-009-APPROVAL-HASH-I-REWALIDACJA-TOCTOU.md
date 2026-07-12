# ADR-009: Approval hash i rewalidacja TOCTOU

- Status: proponowany, wymaga akceptacji referencyjnego procesu R4
- Data propozycji: 2026-07-12
- Zakres: operacje R4/R5, zgoda człowieka i ponowna walidacja przed skutkiem

## Kontekst

`DISC-004` definiuje syntetyczny proces wysłania faktury R4 wraz z preview, action hash, idempotencją i verifierem, ale oczekuje na podpis eksperta IFS oraz właściciela procesu. Zgoda bez związku z konkretnym targetem i aktualnymi faktami tworzy lukę TOCTOU.

## Proponowana decyzja

1. Approval dotyczy jednego tool call i jednego execution attempt, jest jednorazowe, ograniczone czasowo i nie może wrócić ze stanu terminalnego.
2. Action hash jest obliczany z kanonicznej reprezentacji obejmującej co najmniej tenant/user/task/session/tool, argumenty, target, environment, page revision, observation hash, wersje skill/policy/catalogu, nonce i okres ważności.
3. UI approval pokazuje środowisko, operację, rekord, wartości before/after, odbiorców i konsekwencję bez polegania wyłącznie na kolorze.
4. Bezpośrednio przed skutkiem executor odczytuje świeże bound facts i ponownie sprawdza user, permission, policy, environment oraz ważność approval.
5. Każda istotna zmiana unieważnia zgodę. Zużycie approval jest atomowe; timeout po skutku prowadzi do `OutcomeUnknown`, nie automatycznego retry.

## Konsekwencje

- Wymagane są kanoniczna serializacja, storage stanu approval, atomowe compare-and-consume i niezależny verifier.
- R4/R5 bez approval albo z błędem audytu nie mogą dojść do executora.
- Każda zmiana danych, targetu, rewizji strony, środowiska, policy lub katalogu generuje nowy preview i zgodę.

## Warunki akceptacji

- ekspert IFS i właściciel procesu akceptują `DISC-004`;
- testy dowodzą unieważnienia po zmianie każdej bound fact, expiry, no-reuse oraz konkurencyjnego consume;
- sandbox IFS potwierdza verifier i semantykę idempotencji po timeoutcie.

Do spełnienia warunków `GOV-010` pozostaje otwarte.

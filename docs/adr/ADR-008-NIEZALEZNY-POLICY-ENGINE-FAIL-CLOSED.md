# ADR-008: Niezależny Policy Engine fail-closed

- Status: proponowany, wymaga zamknięcia security spike
- Data propozycji: 2026-07-12
- Zakres: autoryzacja, risk, approvals i decyzja o wykonaniu

## Kontekst

Model i dane strony są niezaufane. System musi podejmować decyzję o wykonaniu niezależnie od sugestii modelu, a nieznany stan, błąd polityki lub brak dowodu powinny zatrzymywać działanie. `DISC-015` wciąż nie zawiera pełnego dowodu kontroli egress.

## Proponowana decyzja

1. Policy Engine jest niezależnym modułem deterministycznym i jedyną bramą od zwalidowanego tool call do executorów.
2. Decyzja uwzględnia tenant, user, role, site/environment, tool, skill/version, policy version, risk, permissions, klasę danych, limits i bound facts.
3. Błąd, timeout, brak wpisu, nieznany wynik albo nieosiągalny verifier są traktowane jako deny lub `OutcomeUnknown`, nigdy jako permit.
4. R4 zawsze wymaga konkretnego approval, a R5 silniejszego potwierdzenia lub reauthentication; policy może zaostrzyć reguły niższego ryzyka.
5. Każda decyzja jest audytowana z wersją policy, kodem powodu i correlation ID bez sekretów.

## Konsekwencje

- Policy nie może być implementowana w promptach, skillach ani UI i nie może zależeć od verdictu LLM.
- Wszystkie executory potrzebują kontraktu policy decision oraz testu deny before side effect.
- Administracyjna zmiana policy jest wersjonowana, reviewowana i wpływa na ważność approval.

## Warunki akceptacji

- `DISC-015` jest zamknięty testami negatywnymi prompt injection oraz egress deny;
- testy pokrywają błąd/timeout policy, brak permission, escalation R4/R5 i błąd audytu;
- istnieje fail-closed test, że executor nie działa bez ważnej decyzji policy.

Do spełnienia warunków `GOV-009` pozostaje otwarte.

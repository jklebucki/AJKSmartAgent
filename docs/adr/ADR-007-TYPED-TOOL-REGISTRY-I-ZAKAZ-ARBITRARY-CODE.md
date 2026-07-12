# ADR-007: Typed tool registry i zakaz arbitrary code

- Status: proponowany, wymaga zamknięcia security spike
- Data propozycji: 2026-07-12
- Zakres: katalog narzędzi, walidacja argumentów i granica wykonania

## Kontekst

Prompt injection nie może rozszerzać zdolności systemu. `DISC-015` potwierdził jedynie syntetyczną odporność modelu; nie dostarcza jeszcze testu aplikacyjnego egress deny. Praxiara wymaga małego, wersjonowanego katalogu wysokopoziomowych narzędzi.

## Proponowana decyzja

1. Każde narzędzie ma immutable definicję z identity i wersją, schema argumentów, permission, risk, consequence, idempotency semantics, timeoutem, retry class i verifierem.
2. Katalog jest filtrowany przez user, tenant, site, environment, skill, etap workflow i policy; nieznany tool lub argument kończy się odmową.
3. Model może wskazać wyłącznie nazwę narzędzia z przekazanego katalogu oraz dane zgodne ze schema. Nie tworzy narzędzi ani ścieżek runtime.
4. Zakazane są narzędzia równoważne `execute_javascript`, `execute_shell`, `http_request(anyUrl)`, `read_file(anyPath)`, `write_file(anyPath)`, `set_cookie` i surowy Playwright/CDP.
5. Executor przyjmuje tylko kanonicznie zwalidowany tool call; registry, policy i adapter wybierają bezpieczną trasę wykonania.

## Konsekwencje

- Potrzebne są JSON Schema z limitami oraz stable error codes dla unknown fields, enumów, rozmiaru i formatów.
- API IFS i Browser Worker eksponują operacje biznesowe, a nie URL, OData, CSS ani kod.
- Dodanie nowego toola wymaga review ryzyka, verifiera, testów i nowej wersji katalogu.

## Warunki akceptacji

- `DISC-015` dostarcza testy złośliwego DOM i egress deny niezależnego od promptu;
- testy blokują arbitrary code, nieznane narzędzie, rozszerzenie schema i dowolny host;
- istnieje audyt tool catalog version oraz zredagowanych, kanonicznych argumentów.

Do spełnienia warunków `GOV-008` pozostaje otwarte.

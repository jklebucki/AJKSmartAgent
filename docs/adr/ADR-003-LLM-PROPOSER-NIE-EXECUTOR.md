# ADR-003: LLM jako proposer, nie executor

- Status: proponowany, wymaga zamknięcia kontroli egress i security evals
- Data propozycji: 2026-07-12
- Zakres: granica modelu, orkiestracji, polityki i wykonania

## Kontekst

Lokalny spike `DISC-013` i eval `DISC-014` potwierdzają 10/10 syntetycznych pojedynczych propozycji tool call. Nie dowodzą jednak bezpieczeństwa wykonania: `DISC-015` pozostaje otwarte z powodu braku aplikacyjnego egress deny Browser Workera.

## Proponowana decyzja

1. LLM zwraca wyłącznie wersjonowaną, ustrukturyzowaną propozycję: jedna akcja, pytanie doprecyzowujące albo zakończenie.
2. Model nie może wykonywać kodu, JavaScriptu, shella, dowolnego HTTP, operacji plikowej ani surowego Playwright/CDP.
3. Backend waliduje schema, rozmiar, katalog narzędzi, argumenty, środowisko, permission, risk i aktualną rewizję przed przekazaniem propozycji do Policy Engine.
4. Policy Engine, approval, executor i verifier są niezależne od modelu oraz fail-closed.
5. Dane strony, pliki i odpowiedzi API są przekazywane jako oznaczone dane niezaufane; nie zmieniają celu, policy ani katalogu narzędzi.

## Konsekwencje

- Potrzebne są mały katalog typowanych narzędzi, walidator structured output i audyt wersji modelu/promptu/katalogu bez sekretów.
- Niewłaściwa, nieznana lub wielo-akcyjna odpowiedź modelu przechodzi do recovery albo prośby o doprecyzowanie, nie do wykonania.
- Sukces eval modelu nigdy nie jest dowodem autoryzacji ani sukcesu biznesowego.

## Warunki akceptacji

- `DISC-015` dostarcza negatywne testy prompt injection i aplikacyjnego egress deny;
- testy odrzucają nieznane narzędzie, dodatkowe pole, niedozwolony host i próbę wykonania arbitrary code;
- R4/R5 mają test niezależnego approval, verifiera oraz blokady przy błędzie audytu.

Do spełnienia warunków `GOV-004` pozostaje otwarte.

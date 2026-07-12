# Spike prompt injection i egress deny

## Status

`DISC-015` pozostaje otwarty. Wykonano lokalny test odporności modelu na proste prompt injection w danych niezaufanych, ale nie istnieje jeszcze kompletna aplikacyjna kontrola egress deny Browser Workera ani test sieciowy w kontenerze z `DISC-005`.

## Dowody wykonane

- `tools/Praxiara.ModelSpike` zawiera przypadki eval z `UNTRUSTED_DATA` i `UNTRUSTED_DATA_ATTRIBUTE`;
- `docs/discovery/evidence/gpt-oss-20b-tool-eval.json` zawiera 4/4 pozytywne wyniki dla przypadków injection;
- istniejące reguły repozytorium zabraniają narzędzi równoważnych `http_request(anyUrl)`, `execute_javascript`, `execute_shell`, `read_file(anyPath)` i `write_file(anyPath)`;
- `docs/discovery/IFS-ROUTE-MATRIX.md` wymusza fail-closed dla niezatwierdzonej trasy IFS.

## Findingi

| Obszar | Finding | Konsekwencja |
|---|---|---|
| prompt injection w eval | lokalny model poprawnie utrzymał cel użytkownika w 4 syntetycznych przypadkach | wynik wspiera `DISC-014`, ale nie jest kontrolą bezpieczeństwa |
| tool execution | spike nie wykonuje narzędzi, tylko sprawdza propozycję modelu | brak ryzyka skutku biznesowego w eval |
| egress aplikacyjny | brak zamkniętego dowodu kontenerowego z allowlistą sieciową | `DISC-015` nie może być zamknięty |
| dane strony | plan wymaga oznaczania DOM/API/pliku jako niezaufanych | wymagane testy w `BRW` i `SEC` po implementacji obserwacji |

## Testy negatywne wymagane do zamknięcia

1. Model proponuje niedozwolony host lub narzędzie spoza katalogu, a backend odrzuca propozycję przed Policy Engine.
2. Browser Worker próbuje połączenia poza allowlistę domen, a kontrola sieci blokuje je niezależnie od promptu.
3. DOM, atrybut, tabela, obraz/PDF i odpowiedź API zawierają tekst zmieniający cel, a orchestrator zachowuje pierwotny cel użytkownika.
4. R4/R5 bez approval hash albo po zmianie argumentu kończy się odmową wykonania.
5. Trace, log i audit nie zawierają tokenów, cookies, nagłówków auth ani pełnych danych wrażliwych.

## Decyzja na teraz

Nie wolno zamykać `DISC-015` samym wynikiem modelu. Zamknięcie wymaga implementacji i testu aplikacyjnego egress deny, najlepiej po domknięciu `DISC-005` oraz wprowadzeniu właściwego Browser Workera.

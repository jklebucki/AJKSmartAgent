# Raport wykonania `DISC-011`-`DISC-018` oraz `GOV-001`-`GOV-002`

## Zakres

Raport obejmuje kolejne 10 pozycji planu po `DISC-001`-`DISC-010`: `DISC-011`, `DISC-012`, `DISC-013`, `DISC-014`, `DISC-015`, `DISC-016`, `DISC-017`, `DISC-018`, `GOV-001` i `GOV-002`.

## Status kroków

| Krok | Status | Wykonany rezultat | Warunek zamknięcia |
|---|---|---|---|
| `DISC-011` | `BLOKADA ŚRODOWISKA` | opisano protokół testu Aurena Agent i kategorie funkcji niemożliwych w serwerowym Chromium | zarządzana stacja Windows, companion, sandbox IFS i syntetyczny proces |
| `DISC-012` | `DO AKCEPTACJI` | przygotowano projekt ADR-013: companion poza pierwszym GA, jeśli brak zatwierdzonej ścieżki | decyzja ownerów produktu, architektury i security |
| `DISC-013` | `WYKONANE` | dodano `tools/Praxiara.ModelSpike` z przypiętym digestem lokalnego modelu | brak, dowód jest w repozytorium |
| `DISC-014` | `WYKONANE` | wykonano 10 syntetycznych eval cases tool calling z pass rate 100% | brak, dowód jest w repozytorium |
| `DISC-015` | `CZĘŚCIOWO` | udokumentowano findingi prompt injection eval i brak pełnego egress deny | test aplikacyjnego egress deny w Browser Workerze |
| `DISC-016` | `CZĘŚCIOWO` | udokumentowano licencję i hashe modelu oraz status obrazu Playwright spike | pełny SBOM/CVE i rejestr produkcyjny |
| `DISC-017` | `CZĘŚCIOWO` | utworzono wstępny capacity model Chromium i inferencji | pomiary kontenerowe, streaming i concurrency |
| `DISC-018` | `BLOKADA` | wskazano nierozwiązane blokery Fazy 0 | zamknięcie `DISC-001`-`DISC-017` bez krytycznych blokad |
| `GOV-001` | `WYKONANE WCZEŚNIEJ` | potwierdzono istniejące `DependencyRulesTests` i oznaczenie w planie | brak |
| `GOV-002` | `DO AKCEPTACJI` | przygotowano projekt ADR-001 modularnego control plane i osobnego Browser Workera | zamknięcie `DISC-018` i akceptacja ADR |

## Dowody dodane w tej iteracji

- `tools/Praxiara.ModelSpike`;
- `docs/discovery/evidence/gpt-oss-20b-tool-eval.json`;
- `docs/discovery/AURENA-AGENT-SPIKE.md`;
- `docs/adr/ADR-013-WINDOWS-COMPANION-AURENA.md`;
- `docs/discovery/MODEL-STRUCTURED-OUTPUT-SPIKE.md`;
- `docs/discovery/SECURITY-SPIKE-PROMPT-EGRESS.md`;
- `docs/discovery/MODEL-LICENSE-SUPPLY-CHAIN.md`;
- `docs/discovery/CAPACITY-MODEL.md`;
- `docs/adr/ADR-001-MODULARNY-CONTROL-PLANE-I-BROWSER-WORKER.md`.

## Kluczowe ograniczenia

Nie uruchomiono produkcyjnego IFS, nie użyto danych klienta, nie utrwalono sesji uwierzytelnionej i nie wykonano skutkowej operacji R4/R5. Brakujące kroki wymagają decyzji ownerów albo środowiska, którego Codex nie może samodzielnie założyć.

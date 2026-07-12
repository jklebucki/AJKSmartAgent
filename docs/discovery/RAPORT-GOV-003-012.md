# Raport wykonania `GOV-003`–`GOV-012`

## Zakres

Raport obejmuje przygotowanie projektów ADR dla kolejnych dziesięciu pozycji governance: `GOV-003`–`GOV-012`. Dokumenty zapisują proponowany kierunek oraz kryteria jego akceptacji. Nie zastępują zamknięcia zależności discovery, testów bezpieczeństwa ani podpisów ownerów.

## Status kroków

| Krok | Status | Rezultat | Warunek zamknięcia |
|---|---|---|---|
| `GOV-003` | `DO AKCEPTACJI` | ADR-002: Playwright jako główny protokół | `DISC-007` potwierdzony na sandboxie IFS |
| `GOV-004` | `DO AKCEPTACJI` | ADR-003: LLM proposer, nie executor | zamknięte `DISC-015` |
| `GOV-005` | `BLOKADA FAZY 0` | ADR-004: trwała maszyna stanów i checkpointy | zamknięte `DISC-018` |
| `GOV-006` | `CZĘŚCIOWO` | ADR-005: izolacja i capacity sesji | pełne pomiary `DISC-006` i `DISC-017` |
| `GOV-007` | `DO AKCEPTACJI` | ADR-006: referencje związane z rewizją | `DISC-007` potwierdzony na sandboxie IFS |
| `GOV-008` | `DO AKCEPTACJI` | ADR-007: typed tool registry i zakaz arbitrary code | zamknięte `DISC-015` |
| `GOV-009` | `DO AKCEPTACJI` | ADR-008: niezależny Policy Engine fail-closed | zamknięte `DISC-015` |
| `GOV-010` | `DO AKCEPTACJI` | ADR-009: approval hash i TOCTOU | zaakceptowane `DISC-004` oraz test sandboxu |
| `GOV-011` | `DO AKCEPTACJI` | ADR-010: immutable skills i publikacja | zaakceptowane `DISC-002`–`DISC-004` |
| `GOV-012` | `BLOKADA IFS` | ADR-011: routing IFS API-first | zamknięte `DISC-010` |

## Dodane dowody

- `docs/adr/ADR-002-PLAYWRIGHT-GLOWNY-PROTOKOL.md`;
- `docs/adr/ADR-003-LLM-PROPOSER-NIE-EXECUTOR.md`;
- `docs/adr/ADR-004-TRWALA-MASZYNA-STANOW-I-CHECKPOINTY.md`;
- `docs/adr/ADR-005-IZOLACJA-I-CAPACITY-SESJI-BROWSER.md`;
- `docs/adr/ADR-006-REFERENCJE-ELEMENTOW-ZWIAZANE-Z-REWIZJA.md`;
- `docs/adr/ADR-007-TYPED-TOOL-REGISTRY-I-ZAKAZ-ARBITRARY-CODE.md`;
- `docs/adr/ADR-008-NIEZALEZNY-POLICY-ENGINE-FAIL-CLOSED.md`;
- `docs/adr/ADR-009-APPROVAL-HASH-I-REWALIDACJA-TOCTOU.md`;
- `docs/adr/ADR-010-NIEZMIENNE-SKILLS-I-PUBLIKACJA.md`;
- `docs/adr/ADR-011-IFS-API-FIRST-ROUTING.md`.

## Ograniczenia

Nie uruchomiono produkcyjnego IFS, nie użyto danych klienta, nie zapisano sesji uwierzytelnionej i nie wykonano operacji skutkowej R4/R5. Żaden projekt ADR nie jest zatwierdzoną decyzją produkcyjną, dopóki nie zostaną spełnione jawne warunki akceptacji.

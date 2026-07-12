# ADR-001: Modularny control plane i osobny Browser Worker

- Status: proponowany, zależny od zamknięcia Fazy 0
- Data propozycji: 2026-07-12
- Zakres: granice procesu backendu, orkiestracji i przeglądarki

## Kontekst

Praxiara ma wykonywać kontrolowaną automatyzację, w której backend jest właścicielem stanu, limitów, policy, approval i audytu. Model proponuje jedną typowaną akcję, ale jej nie wykonuje. Chromium jest granicą ryzyka: obsługuje niezaufane strony, DOM, pliki, screenshoty i potencjalne prompt injection.

Repozytorium już rozdziela moduły na `Domain`, `Contracts`, `Application`, capability modules, infrastrukturę i hosty. `GOV-001` egzekwuje dozwolone zależności projektów `src`.

## Proponowana decyzja

1. Control plane pozostaje w `Praxiara.Api`, `Praxiara.Orchestration`, `Praxiara.Policy`, `Praxiara.Application`, `Praxiara.Audit` i powiązanych modułach capability.
2. Browser Worker pozostaje osobnym procesem i osobną granicą zaufania od pierwszego wdrożenia.
3. UI, API i Orchestrator nie otrzymują CDP ani Playwright WebSocket.
4. Browser Worker eksponuje wyłącznie wersjonowany, uwierzytelniony kontrakt typed actions i observations.
5. Model nie otrzymuje surowego Playwright, shella, HTTP do dowolnego URL ani dostępu do plików.
6. Każda akcja przeglądarkowa jest związana z `pageRevision`, task, user, tenant, session i policy decision.

## Uzasadnienie

Oddzielenie Browser Workera ogranicza skutki kompromitacji strony i wymusza jawny kontrakt między planowaniem a wykonaniem. Control plane może deterministycznie walidować narzędzie, argumenty, approval hash, egress i audit bez ufania modelowi ani stronie.

## Konsekwencje

- Potrzebne są kontrakty wire dla obserwacji, akcji, takeover, artefaktów i statusu sesji.
- Worker musi mieć osobne health/readiness, limity zasobów, egress allowlist i redakcję evidence.
- Debugging wymaga correlation IDs i timeline, bo procesy są rozdzielone.
- Testy architektury muszą nadal blokować niedozwolone zależności i obchodzenie workera przez hosty.

## Warunki akceptacji

- `DISC-018` jest zamknięte bez krytycznych blockerów.
- Istnieją testy kontraktowe Browser Workera i testy fail-closed dla niedozwolonej akcji.
- Istnieje egress deny niezależny od promptu.
- Istnieje test, że UI/control plane nie dostaje surowego CDP/Playwright endpoint.

Do spełnienia tych warunków `GOV-002` pozostaje otwarte, a ten ADR jest projektem decyzji.

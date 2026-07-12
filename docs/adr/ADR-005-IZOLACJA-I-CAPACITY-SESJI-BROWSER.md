# ADR-005: Izolacja i capacity sesji browser

- Status: proponowany, oparty na częściowym modelu capacity
- Data propozycji: 2026-07-12
- Zakres: Browser Worker, sesje Chromium i admission control

## Kontekst

`DISC-006` zmierzył tylko lokalny headless Chromium, a `DISC-017` przygotował wstępny model capacity. Brakuje pomiarów kontenerowych non-root, streamingu, prawdziwego IFS i równoległości. Odczytane wyniki nie mogą być uznane za SLO produkcyjne.

## Proponowana decyzja

1. Jedna aktywna sesja przypada domyślnie na jednego użytkownika i zadanie; sesja ma osobny BrowserContext, owner tenant/user/task oraz TTL.
2. Browser Worker działa jako osobny, non-root proces z efemerycznym profilem, ograniczonym egress i bez dostępu do bazy control plane, sekretów LLM oraz Docker socketu.
3. Control plane stosuje admission control przed utworzeniem sesji; brak capacity zwraca jawny wynik bez współdzielenia contextu.
4. Limit roboczy dla prostego zadania wynosi 350–450 MB RAM na sesję, do czasu zebrania docelowych pomiarów.
5. Każda zmiana ownera unieważnia poprzedni fencing token, a cleanup usuwa profil i artefakty zgodnie z retencją.

## Konsekwencje

- Potrzebne są limity per tenant/user/worker, metryki RSS/CPU/ready time, kolejka oraz osobny limit inferencji.
- Capacity musi uwzględniać trace, popupy, pliki, streaming i pełny Browser Worker, nie tylko proces Chromium.
- Niedostępność capacity nie może skłonić systemu do użycia współdzielonej sesji lub utrwalonego storage state.

## Warunki akceptacji

- `DISC-006` jest powtórzony w docelowym kontenerze non-root z sandboxem Chromium;
- `DISC-017` zawiera pomiary concurrency, streamingu i scenariusza IFS;
- testy dowodzą izolacji cookies/storage, TTL, capacity deny oraz cleanup po awarii.

Do spełnienia warunków `GOV-006` pozostaje otwarte.

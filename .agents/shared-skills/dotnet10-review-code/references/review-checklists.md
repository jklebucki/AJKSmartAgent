# Warunkowe checklisty review

Zastosuj sekcję bazową i checklisty właściwe dla zmienionych ścieżek. Każde podejrzenie potwierdź w kontekście wywołań przed zgłoszeniem findingu.

## Każdy diff

- [ ] Zmiana spełnia deklarowane kryteria akceptacji.
- [ ] Diff nie zawiera niezwiązanych zmian ani utraty cudzej pracy.
- [ ] Kierunek zależności odpowiada architekturze repozytorium.
- [ ] Każdy ręczny top-level named type ma osobny plik nazwany jak typ.
- [ ] Nullable, analyzers i warnings nie zostały wyłączone lub suppressowane bez dowodu.
- [ ] Kod i dokumentacja kodowa są po angielsku.
- [ ] Testy dowodzą nowego zachowania, błędu i ważnej granicy.
- [ ] Breaking change, migracja albo ograniczenie jest jawnie opisane.

## Jeśli diff zmienia domenę lub orchestration

- [ ] Niezmiennik jest egzekwowany przy każdej ścieżce zmiany stanu.
- [ ] Niedozwolone przejście nie może ominąć metody encji.
- [ ] Oczekiwane błędy mają jawny wynik/kod, a wyjątki oznaczają sytuacje wyjątkowe.
- [ ] Planner proponuje, a deterministyczny backend autoryzuje i wykonuje.
- [ ] Każdy krok ma limit, checkpoint, weryfikację i audyt.
- [ ] Recovery nie powtarza skutku bez idempotencji.
- [ ] Approval jest unieważniany po zmianie intencji lub stanu.

## Jeśli diff zmienia ASP.NET Core API

- [ ] Endpoint nie zawiera logiki biznesowej ani bezpośredniej instancjacji infrastruktury.
- [ ] Auth jest fail-closed, a `[AllowAnonymous]` lub odpowiednik jest świadomy i minimalny.
- [ ] Authorization sprawdza konkretny zasób i tenant, nie tylko zalogowanie.
- [ ] Binding i walidacja obejmują rozmiar, format i semantykę wejścia.
- [ ] Błędy mapują się na spójny `ProblemDetails` bez stack trace i sekretów.
- [ ] OpenAPI odpowiada runtime contract.
- [ ] Middleware ma właściwą kolejność CORS, authentication, authorization.
- [ ] Health endpoints nie ujawniają szczegółów i rozdzielają liveness/readiness.

## Jeśli diff zmienia async, DI albo workera

- [ ] Nie ma `.Result`, `.Wait()`, `async void`, porzuconych tasks ani sync I/O na request path.
- [ ] `CancellationToken` jest propagowany i ma właściwego właściciela lifetime.
- [ ] Nie użyto `Task.Run` do opakowania I/O w ASP.NET.
- [ ] Singleton nie zależy od scoped service, `DbContext` ani typed `HttpClient`.
- [ ] `DbContext` nie jest używany równolegle.
- [ ] Disposal należy do właściciela instancji albo kontenera.
- [ ] Background work ma bounded queue/workflow, obserwowalne błędy i graceful shutdown.

## Jeśli diff zmienia EF Core lub dane

- [ ] Zapytanie ma limit, projekcję i właściwy tracking.
- [ ] Nie wprowadzono N+1, cartesian explosion ani niepotrzebnego buforowania.
- [ ] Indeks odpowiada rzeczywistemu filtrowaniu i sortowaniu.
- [ ] Aktualizacja chroni współbieżność i poprawnie obsługuje konflikt.
- [ ] Transakcja nie obejmuje oczekiwania na użytkownika, LLM ani zewnętrzne HTTP.
- [ ] Retry zapisu jest idempotentny; outbox nie gubi ani nie duplikuje zdarzenia.
- [ ] Migracja nie myli rename z drop/add i ma bezpieczną kolejność rollout.
- [ ] Aplikacja nie wykonuje produkcyjnych migracji automatycznie na wszystkich replikach.

## Jeśli diff zmienia integrację HTTP albo resilience

- [ ] Klient ma jawny base address, allowlistę domen, timeout i cancellation.
- [ ] Typed client nie jest przechwycony przez singleton.
- [ ] Retry dotyczy tylko błędów przejściowych i bezpiecznych/idempotentnych metod.
- [ ] Unsafe method nie jest retryowana bez idempotency key.
- [ ] Warstwy retry nie mnożą liczby prób.
- [ ] Circuit breaker i rate/concurrency limits odpowiadają charakterystyce zależności.
- [ ] Niepowodzenie częściowe ma weryfikację albo kompensację.
- [ ] Zewnętrzny payload jest walidowany i traktowany jako niezaufany.

## Jeśli diff dotyka bezpieczeństwa agenta lub przeglądarki

- [ ] Model nie otrzymał dowolnego JavaScript, shell, HTTP, filesystem ani raw Playwright.
- [ ] Page content nie może zmienić celu, policy, allowlisty ani katalogu narzędzi.
- [ ] Tool arguments mają schema, permission, risk, domain i revision checks.
- [ ] Element ref jest ważny wyłącznie dla właściwej `pageRevision`.
- [ ] Sesje, cookies, credentials i MFA nie trafiają do promptu ani logów.
- [ ] R4/R5 mają właściwy approval, reauthentication i preview konsekwencji.
- [ ] Egress jest ograniczony kodem i siecią, nie promptem.
- [ ] Audit zapisuje decyzję i hash, ale redaguje dane wrażliwe.

## Jeśli diff zmienia observability

- [ ] Log templates i nazwy pól są stabilne i strukturalne.
- [ ] Tokeny, cookies, auth headers, prompty i PII są redagowane.
- [ ] Metryki nie używają user ID, record ID ani pełnego URL jako labels.
- [ ] Trace propaguje correlation bez duplikowania payloadu.
- [ ] Audit biznesowy nie został zastąpiony zwykłym logiem.
- [ ] Telemetry failure nie blokuje krytycznego workflow bez jawnej polityki.

## Jeśli diff deklaruje poprawę wydajności

- [ ] Istnieje baseline, reprezentatywne obciążenie i wynik po zmianie.
- [ ] Najpierw sprawdzono round trips, SQL i ilość danych.
- [ ] Duże payloady są bounded albo streamowane.
- [ ] Cache ma invalidację, limit, izolację tenantów i metryki skuteczności.
- [ ] `ValueTask`, pooling lub source generation mają mierzalny zysk.
- [ ] Optymalizacja nie wprowadza race condition, stale data ani utraty cancellation.

## Jeśli diff zmienia testy

- [ ] Test sprawdza zachowanie i zawodzi z właściwego powodu.
- [ ] Unit test nie korzysta z infrastruktury ani czasu systemowego.
- [ ] Integration test używa realnej zgodnej usługi i izolowanych danych.
- [ ] Nie ma `Thread.Sleep`, arbitralnego delay, zależności od kolejności ani produkcji.
- [ ] Mock nie odtwarza implementacji EF/LINQ lub prywatnych wywołań SUT.
- [ ] Skip ma przyczynę, właściciela i plan usunięcia.

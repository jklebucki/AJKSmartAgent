# Warunkowe checklisty testów

Użyj sekcji wspólnej i dokładnie tych checklist, które odpowiadają wybranej warstwie testu.

## Każdy test

- [ ] Test opisuje obserwowalne zachowanie, nie prywatną implementację.
- [ ] Nazwa metody jest po angielsku i wskazuje scenariusz oraz oczekiwany wynik.
- [ ] Test ma czytelne Arrange–Act–Assert i jeden Act.
- [ ] Happy path, oczekiwany błąd i ważna granica są rozdzielone na właściwe przypadki.
- [ ] Każdy ręczny top-level named type ma osobny plik nazwany jak typ.
- [ ] Test nie zależy od kolejności, czasu ściennego, locale, timezone ani Internetu.
- [ ] Failure message wskazuje kontrakt, a nie przypadkowy detal.
- [ ] Nie dodano pakietu ani suppression w celu ułatwienia testu.

## Jeśli piszesz unit test

- [ ] SUT jest czystą regułą, value object, parserem, policy albo use case z portami.
- [ ] Brak prawdziwej bazy, HTTP, filesystemu, przeglądarki i kontenera.
- [ ] Czas pochodzi z kontrolowanego `TimeProvider`.
- [ ] Mock/fake reprezentuje granicę, którą projekt kontroluje.
- [ ] Nie mockujesz `DbSet`, LINQ, statycznego BCL ani wewnętrznych metod SUT.
- [ ] Używasz xUnit v3 `Assert`; pojedynczy test nie potrzebuje zewnętrznego DSL assertions.
- [ ] Theory ma jeden wspólny kontrakt, a dane są czytelne w raporcie testu.

## Jeśli piszesz contract test

- [ ] Zamrożono nazwę pól, typy, nullability, enum representation i wersję kontraktu.
- [ ] Serializacja i deserializacja używają tych samych options co runtime.
- [ ] Sprawdzono wymagane, opcjonalne, nieznane i nieprawidłowe pola.
- [ ] Stabilne error codes i `ProblemDetails` mają osobne asercje.
- [ ] OpenAPI lub message schema odpowiada realnemu endpointowi/consumerowi.
- [ ] Breaking fixture jest jawna i nie aktualizuje się automatycznie bez review.

## Jeśli piszesz integration test API

- [ ] Użyto `WebApplicationFactory<Program>` albo rzeczywistego hosta zgodnego z testowanym boundary.
- [ ] Środowisko testowe ma jawnie nadpisane zależności i auth.
- [ ] Test wysyła realny request i sprawdza status, headers oraz kontrakt body.
- [ ] AuthN, authZ, validation i error mapping są wykonywane przez pipeline.
- [ ] Dane są izolowane per test i deterministycznie sprzątane.
- [ ] Test nie polega na kolejności innych testów ani współdzielonym użytkowniku.
- [ ] Liveness i readiness są testowane zgodnie z różną semantyką.

## Jeśli piszesz integration test EF Core/PostgreSQL

- [ ] Użyto realnego PostgreSQL zgodnego z wersją docelową, nie providera InMemory jako substytutu SQL.
- [ ] Schemat jest tworzony kontrolowanie dla testu, nie przez produkcyjny startup.
- [ ] Sprawdzono constraint, mapping, transaction i optimistic concurrency.
- [ ] Test istotnego zapytania weryfikuje wynik i, gdy potrzebne, kształt/liczbę round trips.
- [ ] Dane są unikatowe i izolowane; cleanup działa także po niepowodzeniu.
- [ ] Równoległe testy nie współdzielą `DbContext` ani mutowalnej transakcji.

## Jeśli piszesz architecture test

- [ ] Reguła odpowiada jawnej architekturze z `AGENTS.md`, nie preferencji autora testu.
- [ ] Sprawdzono zakazane kierunki referencji projektowych i namespaces.
- [ ] Test wykrywa jeden ręczny top-level named type na plik oraz zgodność nazwy pliku z typem, z jawnymi wyjątkami.
- [ ] Wyjątki są konkretne, minimalne i mają właściciela; nie użyto szerokiego wildcardu.
- [ ] Test ma negatywną fixture dowodzącą, że rzeczywiście wykrywa naruszenie.
- [ ] Analiza pomija kod generowany i artefakty migracji zgodnie z polityką.

## Jeśli piszesz browser/Playwright test

- [ ] TestSite jest lokalny, deterministyczny i nie zależy od zewnętrznej witryny.
- [ ] Locator preferuje role/name/label zamiast kruchego CSS lub XPath.
- [ ] Sprawdzono stale element ref po zmianie `pageRevision`.
- [ ] Pokryto popup, dialog, iframe, upload/download lub trace tylko wtedy, gdy dotyczy kontraktu.
- [ ] Nie użyto arbitralnego sleep; oczekiwanie dotyczy jawnego stanu.
- [ ] BrowserContext jest izolowany i zamykany także po błędzie.
- [ ] Trace/screenshot nie zawiera prawdziwych credentials ani danych klienta.

## Jeśli piszesz security test

- [ ] Test ma negatywny przypadek i dowodzi fail-closed behavior.
- [ ] Prompt injection z DOM/API/pliku nie zmienia celu, policy ani tool catalog.
- [ ] Egress poza allowlistę jest blokowany niezależnie od promptu.
- [ ] Approval hash unieważnia się po zmianie argumentu, rekordu, środowiska, skill version lub revision.
- [ ] R4/R5 bez wymaganej zgody nie wykonuje skutku.
- [ ] Redakcja obejmuje tokeny, cookies, auth headers, sekrety i pola wrażliwe.
- [ ] Tenant A nie może odczytać sesji, tasku, artefaktu ani audytu tenant B.
- [ ] Test używa danych syntetycznych i nigdy produkcyjnego IFS.

## Jeśli test dotyczy async, concurrency albo workera

- [ ] Synchronizacja używa controllable signal/channel/fake time, nie `Thread.Sleep`.
- [ ] Sprawdzono cancellation przed startem, podczas I/O i podczas shutdown.
- [ ] Wyjątek background task jest obserwowany i raportowany.
- [ ] Scope DI i zasoby są usuwane po sukcesie, błędzie i anulowaniu.
- [ ] Retry ma policzalną granicę, jitter jest kontrolowany, a side effect pozostaje pojedynczy.
- [ ] Race test ma wiele kontrolowanych iteracji lub deterministyczny interleaving, nie przypadkową nadzieję na race.
- [ ] Timeout testu jest bezpiecznikiem, a nie mechanizmem synchronizacji.

## Jeśli dodajesz test regresyjny

- [ ] Test zawodzi na wersji z defektem z właściwego powodu.
- [ ] Fixture zawiera minimalny warunek ujawniający defekt.
- [ ] Nazwa opisuje zachowanie, nie numer issue jako jedyny kontekst.
- [ ] Test pozostaje wartościowy po zmianie implementacji.
- [ ] Nie utrwalono błędnego zachowania pobocznego tylko po to, by odtworzyć historyczny output.

## Końcowa walidacja

- [ ] Pojedynczy nowy test przechodzi samodzielnie.
- [ ] Cały projekt testowy przechodzi.
- [ ] Test przechodzi w powtórzeniu i przy dozwolonej równoległości.
- [ ] Build ma zero warnings.
- [ ] Nie ma nowych skipped tests ani niejawnej zależności od lokalnego środowiska.
- [ ] Raport podaje faktyczną liczbę i zakres wykonanych testów.

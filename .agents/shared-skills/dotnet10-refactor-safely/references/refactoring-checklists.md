# Warunkowe checklisty refaktoringu

Najpierw użyj checklisty bezpieczeństwa, następnie wyłącznie sekcji odpowiadającej rozpoznanemu problemowi.

## Bezpieczeństwo każdego refaktoringu

- [ ] Istnieje działający baseline testów lub test charakterystyczny.
- [ ] Zapisano obserwowalne zachowania, których nie wolno zmienić.
- [ ] Nie zmienia się publiczne API, wire format, kody błędów, schema ani policy.
- [ ] Refaktoring jest oddzielony od poprawki funkcjonalnej i aktualizacji pakietów.
- [ ] Każdy krok jest mały i osobno kompilowalny lub testowalny.
- [ ] Nie naruszono cudzych zmian w worktree.
- [ ] Końcowy diff usuwa złożoność zamiast tylko ją przenosić.

## Jeśli plik zawiera wiele top-level types

- [ ] Każdą klasę, record, struct, interface, enum i delegate przeniesiono do osobnego pliku.
- [ ] Nazwa każdego pliku jest dokładnie nazwą typu bez arity generycznej.
- [ ] Zachowano namespace, accessibility, atrybuty i XML docs.
- [ ] Nie zmieniono publicznej nazwy ani assembly-qualified type name.
- [ ] `partial` pozostawiono tylko dla generatora/frameworka albo jawnie uzasadnionego podziału.
- [ ] Wyjątki ograniczono do `Program.cs`, kodu generowanego i artefaktów migracji EF.
- [ ] Build potwierdza brak utraconych `using`, resources i zależności MSBuild.

## Jeśli klasa ma wiele odpowiedzialności

- [ ] Zidentyfikowano niezależne powody zmiany, a nie arbitralny limit linii.
- [ ] Niezmienniki pozostają blisko stanu, który chronią.
- [ ] I/O oddzielono od czystej decyzji, jeśli poprawia to granicę i testowalność.
- [ ] Nowy typ ma nazwę biznesową lub techniczną opisującą jedną rolę.
- [ ] Konstruktor nowego typu nie ukrywa zależności przez `IServiceProvider`.
- [ ] Nie powstał pusty wrapper przekazujący każde wywołanie bez wartości.
- [ ] Composition root składa nowe implementacje bez logiki biznesowej.

## Jeśli usuwasz duplikację

- [ ] Potwierdzono, że powtarza się ta sama wiedza lub reguła, nie tylko podobna składnia.
- [ ] Wspólna abstrakcja ma stabilną nazwę i właściciela modułowego.
- [ ] Parametry nie tworzą „boolean blindness” ani uniwersalnej funkcji z wieloma trybami.
- [ ] Różne reguły, które mogą ewoluować niezależnie, pozostają rozdzielone.
- [ ] Nowa abstrakcja zmniejsza łączną złożoność wywołań i testów.
- [ ] Usunięto wszystkie stare kopie po migracji konsumentów.

## Jeśli poprawiasz kierunek zależności

- [ ] Port należy do warstwy potrzebującej zdolności, nie do implementacji infrastruktury.
- [ ] Domain i Contracts nie otrzymały referencji do hosta, EF, Playwright ani transportu.
- [ ] Capability module nie zależy od composition root.
- [ ] DTO wire nie zawiera encji EF ani typów provider-specific.
- [ ] Nie powstał cykl projektów ani cykl logiczny przez callback/service locator.
- [ ] Architecture tests odzwierciedlają istniejący dozwolony DAG, nie ukrywają wyjątku.

## Jeśli upraszczasz async albo background work

- [ ] Async pozostaje end-to-end; nie wprowadzono `.Result`, `.Wait()` ani `async void`.
- [ ] `CancellationToken` jest ostatnim parametrem i dociera do I/O.
- [ ] Nie porzucono task ani wyjątku background operation.
- [ ] Scope DI żyje co najmniej tak długo jak praca i jest deterministycznie usuwany.
- [ ] Nie uruchomiono równolegle operacji na jednym `DbContext`.
- [ ] Zamiana `Task` na `ValueTask` ma pomiar; bez pomiaru pozostawiono `Task`.
- [ ] Shutdown i timeout zachowują poprzednią semantykę.

## Jeśli upraszczasz DI albo konfigurację

- [ ] Nie ma `BuildServiceProvider()` podczas rejestracji ani service locatora w kodzie domenowym.
- [ ] Lifetime każdej usługi odpowiada stanowi i zależnościom.
- [ ] Singleton nie przechwytuje scoped service ani typed `HttpClient`.
- [ ] Kontener, nie konsument, usuwa utworzone przez siebie `IDisposable`/`IAsyncDisposable`.
- [ ] Options są małe, typowane i walidowane na starcie.
- [ ] Rejestracja pozostaje jawna i możliwa do znalezienia bez reflection, jeśli reflection nie jest wymogiem.

## Jeśli refaktoring dotyka EF Core

- [ ] Mapping, nazwy tabel/kolumn, concurrency tokens i delete behavior są identyczne.
- [ ] Model snapshot i migracje nie zmieniły się przypadkowo.
- [ ] Tracking i granica unit-of-work pozostają takie same.
- [ ] Kolejność SaveChanges, outbox i side effects jest zachowana.
- [ ] Wygenerowany SQL nie pogorszył liczby round trips ani kształtu danych.
- [ ] Brak różnicy schematu został potwierdzony narzędziem, nie założony.

## Końcowa kontrola zachowania

- [ ] Publiczne kontrakty przed i po są zgodne.
- [ ] Testy charakterystyczne oraz istniejące testy przechodzą.
- [ ] Format i build przechodzą bez warnings.
- [ ] Nie pozostał martwy typ, rejestracja, plik ani `using`.
- [ ] SOLID/DRY/KISS/YAGNI poprawiły konkretny koszt, a nie wynik metryki dla samej metryki.

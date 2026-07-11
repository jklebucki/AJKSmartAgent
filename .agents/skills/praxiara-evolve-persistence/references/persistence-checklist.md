# Checklista persystencji i migracji

Używaj checklisty do planu, review migracji i raportu końcowego. Nie zaznaczaj punktu bez dowodu w kodzie, SQL, planie wykonania albo teście.

## 1. Granice odpowiedzialności

- [ ] Niezmiennik biznesowy pozostaje w `Praxiara.Domain`.
- [ ] Port do trwałości jest zdefiniowany w `Praxiara.Application`, jeżeli wymaga go przypadek użycia.
- [ ] EF Core, Npgsql, encje persystencji i migracje pozostają w `Praxiara.Infrastructure`.
- [ ] API nie zwraca encji EF, a domena nie zależy od `DbContext`.
- [ ] Każda encja, konfiguracja, konwerter i interceptor ma osobny plik oraz jeden top-level type.
- [ ] Nazwy typów, migracji, tabel/kolumn zgodne z przyjętą konwencją i testów są angielskie.
- [ ] Nie dodano repository, unit of work lub generic base class bez rzeczywistej granicy i korzyści.

## 2. Model EF Core

- [ ] Każdy trwały typ ma jawne `IEntityTypeConfiguration<T>` w osobnym pliku.
- [ ] Klucz główny i wszystkie klucze alternatywne są jawne.
- [ ] Nullability C# odpowiada nullability kolumny.
- [ ] Długość tekstu jest ograniczona tylko wtedy, gdy wynika z reguły lub ochrony zasobów.
- [ ] Precyzja i skala `decimal` są jawne.
- [ ] Relacja ma jawny foreign key i świadomie wybrane zachowanie delete.
- [ ] Constraint bazy chroni krytyczną integralność niezależnie od adaptera.
- [ ] Concurrency token i zachowanie konfliktu są jawne.
- [ ] Nie włączono lazy loading; odczyt jest widoczny w kodzie.
- [ ] Seed nie zawiera sekretów, danych klienta ani danych zależnych od czasu lokalnego.

## 3. Typy PostgreSQL

- [ ] Timestamp jest jednoznaczny i przechowywany jako `timestamptz`/UTC; lokalny czas pojawia się wyłącznie na granicy prezentacji.
- [ ] Nowe identyfikatory zdarzeń używają UUIDv7, jeśli nie ma wymogu innego formatu.
- [ ] `text` lub limitowane `varchar` wybrano na podstawie rzeczywistego constraintu, nie nawyku.
- [ ] `jsonb` służy elastycznym danym o znanej własności i wersji, nie zastępuje modelu relacyjnego dla pól często filtrowanych.
- [ ] Typ enum bazy nie został użyty bez oceny kosztu przyszłego rollout i dodawania wartości.
- [ ] Tablica PostgreSQL nie ukrywa relacji wymagającej kluczy, zapytań i integralności.
- [ ] Każde wymagane rozszerzenie PostgreSQL ma właściciela, wersję, licencję, backup i procedurę upgrade.

## 4. Indeksy i zapytania

- [ ] Dla każdego nowego indeksu wskazano konkretne zapytanie i oczekiwany filtr/sortowanie.
- [ ] Kolejność kolumn indeksu odpowiada predykatom i selektywności.
- [ ] Rozważono unique, partial lub covering index, ale wybrano najprostszy wystarczający wariant.
- [ ] Foreign keys używane w join/delete mają potrzebne indeksy.
- [ ] Nie utworzono zduplikowanego ani nieużytecznego indeksu.
- [ ] Odczyt read-only używa projekcji i `AsNoTracking`.
- [ ] Filtrowanie i stronicowanie odbywa się przed materializacją.
- [ ] Brak N+1 i nieograniczonego `Include` grafu.
- [ ] Query compilation, pooling albo raw SQL są użyte dopiero po pomiarze.
- [ ] Krytyczny query plan sprawdzono na reprezentatywnych danych; nie deklaruje się wydajności na pustej bazie.

## 5. Transakcje, outbox i współbieżność

- [ ] Granica transakcji odpowiada jednej spójnej zmianie biznesowej.
- [ ] Rekord outbox jest zapisany w tej samej transakcji co stan biznesowy.
- [ ] Handler jest odporny na duplikaty i posiada idempotency key.
- [ ] Optimistic concurrency ma jawny rezultat oraz test dwóch konkurujących zapisów.
- [ ] Retry obejmuje tylko błąd przejściowy, ma limit i nie powiela skutku.
- [ ] Nie ma kilku niezależnych warstw retry dla tej samej operacji.
- [ ] Operacja masowa nie omija niezmienników, audit trail ani tenant isolation.
- [ ] Awaria po commicie, ale przed odpowiedzią, ma rozpoznawalny rezultat lub stan `OutcomeUnknown`.

## 6. Bezpieczna migracja

- [ ] Nazwa migracji opisuje zmianę w języku angielskim.
- [ ] Przejrzano pełne `Up`, `Down` i model snapshot.
- [ ] Wygenerowany SQL dotyczy wyłącznie zamierzonego zakresu.
- [ ] Oceniono lock level, czas transakcji, table rewrite, pełny skan i wymagane wolne miejsce.
- [ ] Addytywna faza `expand` jest zgodna ze starą i nową aplikacją.
- [ ] Backfill jest porcjowany, obserwowalny, idempotentny i wznawialny.
- [ ] Zaostrzenie nullability/constraint następuje dopiero po potwierdzonym backfillu.
- [ ] Faza `contract` następuje po potwierdzeniu braku starszych replik i konsumentów.
- [ ] Operacja destrukcyjna ma zatwierdzony backup i recovery/forward-fix.
- [ ] Migrację wykonuje jeden kontrolowany job; start aplikacji nie uruchamia jej na wszystkich replikach.
- [ ] Migracja nie uruchamia operacji biznesowych ani nie łączy się z produkcyjnym IFS.

## 7. Dane, bezpieczeństwo i retencja

- [ ] Dane mają klasyfikację, właściciela i okres retencji.
- [ ] Tenant/user ownership jest elementem zapytania i testu, nie tylko filtrem UI.
- [ ] RLS jest użyte tylko po jawnej decyzji i z testem fail-closed dla braku kontekstu tenant.
- [ ] Connection string, tokeny i parametry wrażliwe nie trafiają do logów ani traces.
- [ ] Dane wrażliwe są redagowane przed audytem i telemetrią.
- [ ] Kasowanie respektuje legal hold, audyt i zasady retencji.
- [ ] Fixture oraz snapshot bazy nie zawierają rzeczywistych danych klienta.

## 8. Testy i bramka końcowa

- [ ] Test jednostkowy obejmuje zmieniony niezmiennik domenowy.
- [ ] Test integracyjny używa właściwej wersji PostgreSQL, nie providera InMemory.
- [ ] Migracja działa na pustej bazie.
- [ ] Migracja działa od poprzedniej wspieranej wersji z reprezentatywnymi danymi.
- [ ] Ponowne wykonanie backfillu nie duplikuje ani nie uszkadza danych.
- [ ] Constraints, nullability, unique conflict i cascade delete mają testy odpowiednie do zmiany.
- [ ] Concurrency, idempotency i outbox mają scenariusz awarii.
- [ ] Recovery lub forward-fix został przećwiczony w środowisku testowym.
- [ ] Targetowane testy, build i formatowanie przechodzą bez ostrzeżeń.
- [ ] Dokument wdrożenia opisuje kolejność, obserwację, rollback/recovery i właściciela decyzji.

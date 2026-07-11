---
name: praxiara-evolve-persistence
description: 'Zmienia EF Core 10, PostgreSQL, migracje, transakcje, outbox i zapytania Praxiary bez utraty danych. Użyj dla encji persystencji, mapowania, indeksu, constraintu, migracji lub operacji bazodanowej. Nie używaj do logiki domenowej, API, UI ani konfiguracji PostgreSQL bez zmiany modelu danych.'
---

# Rozwijanie persystencji Praxiary

Traktuj zmianę danych jako ewolucję stanu produkcyjnego, nie tylko kompilujący model EF. Zaplanuj zgodność starego i nowego kodu, wprowadź najmniejszą pełną zmianę, a następnie udowodnij migrację oraz zachowanie na rzeczywistym PostgreSQL.

Przed pracą przeczytaj `AGENTS.md` oraz [checklistę persystencji](references/persistence-checklist.md) w całości. Dla zmian rolloutów i retencji przeczytaj odpowiednie sekcje `docs/PLAN_WYTWORZENIA.md` i dokument PostgreSQL w `docs/infrastruktura/services/`.

## Reguły stałe

- `Domain` przechowuje niezmienniki biznesowe bez zależności od EF Core. Porty trwałości należą do `Application`, a EF Core, Npgsql, migracje i outbox do `Infrastructure`.
- Nie zwracaj encji EF przez API i nie przenoś atrybutów transportowych do modelu domenowego.
- Twórz jeden top-level type w jednym pliku C# i nazwij plik tak jak typ. Dotyczy to encji persystencji, konfiguracji `IEntityTypeConfiguration<T>`, konwerterów i interceptorów. Kod generowany i pliki migracji EF są wyjątkami.
- Jeżeli dotykany plik zawiera kilka typów, rozdziel zmieniane typy w ramach tej samej zmiany, bez przebudowy niezwiązanego modułu.
- Stosuj SOLID do odpowiedzialności i kierunku zależności, DRY do pojedynczego źródła reguły/mapowania, KISS do zapytań i konfiguracji. Nie dodawaj generycznego repository nad EF tylko dla pozornej abstrakcji.
- Kod, identyfikatory, nazwy migracji i operacji, komentarze, XML docs, logi i testy zapisuj po angielsku. Ręczny Markdown pozostaje polski.
- Każde I/O jest asynchroniczne i przyjmuje `CancellationToken` jako ostatni parametr. Nie używaj `.Result`, `.Wait()` ani synchronicznych odpowiedników EF w ścieżce I/O.
- Czas pobieraj przez `TimeProvider`, zapisuj jednoznacznie w UTC i przesyłaj jako `DateTimeOffset`. Dla nowych identyfikatorów zdarzeń preferuj UUIDv7.
- Nie interpoluj danych do SQL. Surowy SQL wymaga parametryzacji, testu i uzasadnienia, dlaczego LINQ lub API EF jest niewystarczające.
- Nie uruchamiaj migracji automatycznie równolegle na replikach ani przeciw produkcji z sesji deweloperskiej.
- Nie dodawaj pakietu lub rozszerzenia PostgreSQL bez bramki licencyjnej, bezpieczeństwa i operacyjnej.

## Przepływ pracy

### 1. Zaplanuj zmianę danych

1. Sprawdź `git status --short`, model domenowy, port aplikacyjny, `PraxiaraDbContext`, najbliższe mapowanie, migracje i testy integracyjne.
2. Opisz dane przed i po zmianie, właściciela, klasyfikację, retencję oraz sposób usunięcia.
3. Sklasyfikuj zmianę jako addytywną, transformującą albo destrukcyjną.
4. Określ zgodność starej i nowej wersji aplikacji podczas rolling deployment. Dla breaking schema change zaplanuj `expand → migrate/backfill → contract`.
5. Zdefiniuj transakcję, współbieżność, idempotencję i relację z outboxem.
6. Wypisz krytyczne zapytania, potrzebne constraints i indeksy oraz oczekiwany rozmiar danych.
7. Ustal test migracji z pustej bazy i z poprzedniej wspieranej wersji.

Jeżeli zmiana może utracić dane, długo blokować tabelę, złamać starszą replikę lub nie ma strategii backup/recovery, zatrzymaj implementację i poproś o decyzję architektoniczną lub operacyjną.

### 2. Zmień model i mapowanie

1. Najpierw zmień kontrakt portu lub niezmiennik domenowy, jeżeli semantyka biznesowa rzeczywiście się zmienia.
2. Utrzymuj encję persystencji jako szczegół adaptera. Mapuj jawnie między domeną a rekordem bazy.
3. Przenieś konfigurację modelu do osobnej klasy `IEntityTypeConfiguration<T>` dla każdego typu. Utrzymuj `OnModelCreating` jako prostą rejestrację konfiguracji.
4. Nadaj jawnie typ kolumny, długość tylko gdy jest regułą, precyzję liczb, nullability, klucze, relacje, zachowanie delete i concurrency token.
5. Dodawaj constraint bazy dla niezmiennika, który musi być zachowany niezależnie od ścieżki zapisu. Nie duplikuj pełnej logiki domenowej w SQL.
6. Projektuj indeks pod konkretne zapytanie i selektywność. Nie dodawaj indeksu „na wszelki wypadek”.

### 3. Utwórz i przejrzyj migrację

1. Nadaj migracji opisową angielską nazwę, np. `AddAgentTaskLease`.
2. Wygeneruj migrację przez narzędzia EF dopiero po poprawnym modelu. Nie pisz ręcznie snapshotu.
3. Przeczytaj całe `Up`, `Down` i zmianę model snapshot. Usuń niezamierzone operacje.
4. Wygeneruj i przejrzyj SQL dla docelowego PostgreSQL. Oceń locki, pełne skany, przepisywanie tabeli i czas wykonania.
5. Backfill projektuj jako ograniczony, obserwowalny, idempotentny i możliwy do wznowienia. Nie łącz dużego backfillu z jedną długą transakcją migracji.
6. Dla nieodwracalnej transformacji opisz recovery z backupu lub forward fix; pusty `Down` nie jest planem rollbacku.
7. Migracje produkcyjne wykonuje kontrolowany pojedynczy job lub operator, nie start każdej repliki.

Przykład polecenia po potwierdzeniu właściwego startup project i design-time configuration:

```bash
dotnet ef migrations add AddAgentTaskLease --project src/Praxiara.Infrastructure --startup-project src/Praxiara.Api
```

### 4. Zaimplementuj odczyt i zapis

1. Dla odczytów bez modyfikacji używaj projekcji i `AsNoTracking`, chyba że istnieje udowodniona potrzeba trackingu.
2. Pobieraj tylko potrzebne kolumny i ograniczaj wyniki stronicowaniem. Unikaj N+1, nieograniczonych `Include` i materializacji przed filtrowaniem.
3. Dla zapisu wyznacz minimalną granicę transakcji. Zdarzenie outbox zapisz atomowo ze zmianą biznesową.
4. Obsłuż optimistic concurrency jako oczekiwany wynik biznesowy, nie jako bezwarunkowy retry.
5. Retry stosuj wyłącznie dla błędów przejściowych, z limitem, backoffem i idempotencją. Nie układaj wielu warstw retry.
6. Operacje masowe nie mogą ominąć policy, audytu, tenant isolation ani niezmienników biznesowych.
7. Loguj identyfikatory i wynik techniczny bez SQL zawierającego dane, wartości parametrów wrażliwych lub connection stringów.

### 5. Udowodnij zmianę

1. Dodaj testy domenowych niezmienników do `Praxiara.UnitTests`, jeśli zmieniła się semantyka.
2. Uruchom testy integracyjne na rzeczywistym PostgreSQL, nie uznawaj providera InMemory za dowód zgodności.
3. Przetestuj utworzenie pustej bazy, migrację z poprzedniej wspieranej wersji, ponowne uruchomienie procedury i ścieżkę recovery.
4. Dodaj scenariusze nullability, constraints, unique conflict, concurrency, idempotency, outbox i tenant isolation odpowiednio do zmiany.
5. Dla zapytania krytycznego sprawdź plan wykonania na reprezentatywnej ilości danych przed deklaracją wydajności.

```bash
dotnet test tests/Praxiara.UnitTests/Praxiara.UnitTests.csproj
dotnet test tests/Praxiara.IntegrationTests/Praxiara.IntegrationTests.csproj
dotnet build Praxiara.slnx --no-restore
```

### 6. Zakończ pracę

- Uruchom formatowanie, przejrzyj diff migracji i upewnij się, że żaden sekret ani dane klienta nie trafiły do fixture.
- Potwierdź jeden top-level type na plik poza wygenerowaną migracją.
- Zaktualizuj polski dokument wdrożenia, jeżeli operator musi wykonać migrację, backup, backfill lub rollback.
- Raportuj model przed/po, sposób rollout, SQL/migrację, testy, ryzyko locków i rzeczywisty plan recovery.

Nie uznawaj zmiany za zakończoną tylko dlatego, że `dotnet ef database update` działa na pustej lokalnej bazie. Produkcyjna ewolucja musi być zgodna z poprzednim stanem, obserwowalna i odzyskiwalna.

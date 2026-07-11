---
name: dotnet10-write-tests
description: "Projektuje deterministyczne testy .NET 10/C# 14 w xUnit v3: unit, integration, contract, architecture, browser i security. Użyj, gdy rezultatem są testy, regresja lub pokrycie zachowania. Nie używaj do implementacji produkcyjnej, diagnozy, refaktoringu, review ani zarządzania zależnościami."
---

# Pisanie testów .NET 10

Udowodnij zachowanie przy najniższym rozsądnym koszcie i z właściwą granicą testu. Nie zwiększaj liczby testów kosztem ich deterministyczności, czytelności lub wartości regresyjnej.

## Granica działania

- Zmieniaj testy, fixtures i deterministyczne test sites. Nie zmieniaj kodu produkcyjnego, chyba że użytkownik jawnie rozszerzy zakres.
- Jeśli kod jest nietestowalny bez nowego portu lub zmiany zachowania, opisz brakujący seam i przekaż go do `dotnet10-implement-change` albo `dotnet10-refactor-safely`.
- Nie dodawaj biblioteki testowej w tym workflow; zarządzanie pakietem należy do `dotnet10-manage-dependencies`.

## Workflow

1. **Określ kontrakt zachowania.** Zapisz wejście, obserwowalny wynik i ryzyko regresji. Nie testuj prywatnej implementacji.
2. **Wybierz najniższy poziom.** Użyj unit testu dla czystej reguły, contract testu dla wire format, integration testu dla adaptera lub procesu, a browser/security testu dla realnej granicy.
3. **Dobierz scenariusze.** Obejmij happy path, oczekiwany błąd i najważniejszą granicę. Dla naprawy najpierw dodaj test, który zawodzi z dawnym defektem.
4. **Odczytaj checklistę.** Zastosuj wyłącznie właściwe sekcje z [checklist testowych](references/testing-checklists.md).
5. **Zaprojektuj deterministyczność.** Kontroluj czas przez `TimeProvider`, identyfikatory, kolejność, dane, cancellation i zależności zewnętrzne. Nie używaj dowolnych opóźnień do synchronizacji.
6. **Napisz czytelny test.** Stosuj Arrange–Act–Assert, jeden Act i nazwę opisującą zachowanie. Używaj wbudowanego `Assert` xUnit v3.
7. **Uruchom najwężej.** Najpierw pojedynczy test lub projekt. Potem uruchom wszystkie testy konsumentów zmienionego kontraktu.
8. **Sprawdź wartość.** Test ma zawodzić z właściwego powodu po kontrolowanym odwróceniu warunku i nie może zależeć od kolejności wykonania.

## Reguły implementacji testów

- Jeden ręczny top-level named type przypada na jeden plik nazwany jak typ. Klasa testowa, fixture, fake, builder, enum i record mają osobne pliki. Wyjątki dotyczą wyłącznie `Program.cs`, kodu generowanego i artefaktów EF.
- Nazwy typów, metod testowych, fixtures, kod, komentarze i komunikaty diagnostyczne zapisuj po angielsku.
- Nazwa testu opisuje zachowanie, scenariusz i oczekiwany wynik. Nie używaj numerów przypadków bez znaczenia.
- Unit test nie korzysta z sieci, prawdziwej bazy, systemowego zegara, współdzielonego filesystemu ani globalnego stanu.
- Mockuj granice, które kontrolujesz; preferuj proste fakes/stubs. Nie mockuj `DbSet`, LINQ ani wewnętrznych wywołań klasy.
- Integration test używa rzeczywistej zgodnej usługi w izolowanych danych i ma jawny cleanup.
- Test async zwraca `Task`, propaguje cancellation tam, gdzie jest częścią kontraktu, i nigdy nie używa `.Wait()` ani `.Result`.
- Nie używaj `Thread.Sleep` i dowolnego `Task.Delay` jako gwarancji kolejności. Użyj sygnału, fake time albo obserwowalnego stanu z limitem czasu.
- Nie ukrywaj wielu zachowań w jednym teście. Theory stosuj, gdy przypadki mają ten sam kontrakt i różnią się wyłącznie danymi.
- Nie używaj snapshotu jako jedynego dowodu krytycznego zachowania.
- Nie oznaczaj testu jako skipped bez przyczyny, właściciela i planu usunięcia skipu.

## Walidacja

Najpierw uruchom projekt lub filtr:

```bash
TEST_PROJECT="tests/YourProject.Tests/YourProject.Tests.csproj"
dotnet test "$TEST_PROJECT" --filter "FullyQualifiedName~TargetBehavior" --logger "console;verbosity=normal"
```

Następnie uruchom odpowiedni projekt bez filtra i, dla zmiany przekrojowej, całość:

```bash
SOLUTION="path/to/YourSolution.slnx"
dotnet test "$SOLUTION" --no-build --logger "console;verbosity=minimal"
```

Jeśli dodałeś nowe pliki wymagające kompilacji, wykonaj build przed `--no-build`. W raporcie podaj liczbę testów, wynik, czas oraz celowo niewykonane warstwy.

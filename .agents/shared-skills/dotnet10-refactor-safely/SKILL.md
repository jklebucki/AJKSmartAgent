---
name: dotnet10-refactor-safely
description: "Refaktoryzuje .NET 10/C# 14 małymi krokami bez zmiany obserwowalnego zachowania, kontraktów i danych. Użyj do uproszczeń, rozdzielania typów, redukcji duplikacji lub poprawy zależności. Nie używaj do funkcji, naprawy niepotwierdzonego defektu, zmiany pakietów, samych testów ani review."
---

# Bezpieczny refaktoring .NET 10

Popraw strukturę przy zachowaniu publicznego i biznesowego zachowania. Jeżeli refaktoring wymaga zmiany kontraktu, wyniku, schematu, telemetry albo polityki, zatrzymaj się i przeklasyfikuj pracę na implementację zmiany.

## Niezmienniki

- Nie zmieniaj publicznego API, formatu JSON, kodów błędów, routingu, uprawnień, kolejności skutków ubocznych ani schematu danych.
- Nie dodawaj zależności i nie aktualizuj wersji.
- Nie mieszaj refaktoringu mechanicznego z naprawą zachowania w jednym kroku.
- Zachowaj istniejące zmiany użytkownika i ogranicz diff do wybranego obszaru.

## Workflow

1. **Nazwij problem strukturalny.** Wskaż konkretny koszt: zmieszane odpowiedzialności, niewłaściwy kierunek zależności, duplikację reguły, zbyt duży typ albo wiele typów w pliku.
2. **Zamroź zachowanie.** Odczytaj kontrakty i testy. Uruchom najwęższy istniejący zestaw; jeśli brakuje ochrony istotnego zachowania, dodaj test charakterystyczny w ramach `dotnet10-write-tests` przed refaktoringiem.
3. **Wybierz transformację.** Odczytaj pasującą sekcję [checklist refaktoringu](references/refactoring-checklists.md). Nie stosuj wzorca tylko dlatego, że istnieje.
4. **Wykonuj małe kroki.** Po każdym logicznym kroku uruchom format/build albo najwęższy test. Utrzymuj możliwość łatwego wskazania, który krok wprowadził regresję.
5. **Uprość, nie przemianuj chaosu.** Przenieś odpowiedzialność do właściwego modułu, usuń martwe pośrednictwo i zachowaj jawny przepływ.
6. **Sprawdź granice.** Porównaj API, serializację, migracje, log events, metryki, kolejność I/O i zachowanie cancellation przed i po.
7. **Uruchom pełną walidację obszaru.** Dla zmiany przekrojowej uruchom pełny build i testy rozwiązania.

## Reguły projektowe

- Jeden ręczny top-level named type przypada na jeden plik, a plik ma nazwę typu. Rozdziel klasy, rekordy, struktury, interfejsy, enumy i delegaty. Wyjątki ogranicz do `Program.cs`, kodu generowanego i artefaktów migracji EF.
- W nowych plikach używaj file-scoped namespaces.
- Stosuj SRP przez rozdzielenie rzeczywiście odmiennych powodów zmiany. Nie rozbijaj spójnego algorytmu na bezwartościowe klasy.
- Stosuj DIP na granicach procesu, I/O i zmiennych implementacji. Nie twórz interfejsu dla każdej klasy.
- DRY oznacza jedno źródło tej samej wiedzy. Pozostaw podobny kod, jeśli reprezentuje różne reguły, które mogą ewoluować niezależnie.
- KISS preferuje bezpośredni kod nad reflection, service locator, magiczną rejestrację i wielowarstwowe wrappery.
- YAGNI zabrania abstrakcji dla hipotetycznych providerów lub scenariuszy.
- Zachowaj async end-to-end i propagację `CancellationToken`. Nie zamieniaj `Task` na `ValueTask` bez pomiaru.
- Typy nieprzeznaczone do dziedziczenia oznaczaj `sealed`; pola niezmieniane po konstrukcji oznaczaj `readonly`.

## Walidacja

Wykonaj kolejno:

```bash
SOLUTION="path/to/YourSolution.slnx"
dotnet format "$SOLUTION" --no-restore --verify-no-changes
dotnet build "$SOLUTION" --no-restore
dotnet test "$SOLUTION" --no-build --logger "console;verbosity=minimal"
```

Jeśli pełna bramka jest nieproporcjonalna podczas małego kroku, uruchamiaj projekt docelowy, ale zakończ refaktoring pełną walidacją wszystkich zmienionych kontraktów i konsumentów. W raporcie oddziel dowód zachowania od oceny poprawy struktury.

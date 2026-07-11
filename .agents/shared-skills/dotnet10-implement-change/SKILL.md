---
name: dotnet10-implement-change
description: "Implementuje zmianę zachowania w .NET 10/C# 14 od kryteriów akceptacji po walidację. Użyj dla funkcji, poprawki, endpointu, modelu domenowego lub integracji. Nie używaj do samej diagnozy, refaktoringu bez zmiany zachowania, review, zadania wyłącznie testowego ani zarządzania pakietami."
---

# Implementowanie zmiany .NET 10

Dostarcz najmniejszą kompletną zmianę spełniającą kryteria akceptacji i granice architektury. Prostota oznacza brak zbędnych warstw, nie pominięcie walidacji, błędów, bezpieczeństwa albo testów.

## Workflow

1. **Ustal kontrakt zadania.** Zapisz scenariusz, wejścia, wynik, błędy, skutki uboczne, wymagania kompatybilności i kryteria akceptacji.
2. **Znajdź właściciela zachowania.** Przeczytaj `AGENTS.md`, dokument obszaru, najbliższy kod i testy. Sprawdź kierunek zależności oraz `git status --short`.
3. **Oceń ryzyko.** Określ granice zaufania, dane wrażliwe, idempotencję, współbieżność i wymagany audyt. Dla operacji skutkowej ustal policy i approval przed kodowaniem.
4. **Sklasyfikuj zmianę.** Użyj odpowiadającej sekcji z [checklist implementacyjnych](references/implementation-checklists.md). Nie stosuj checklist niezwiązanych z zakresem.
5. **Zmień kontrakt i test.** Najpierw dodaj lub zmień jawny kontrakt oraz test dowodzący zachowania, jeśli test-first nie wymusza sztucznej konstrukcji.
6. **Zaimplementuj pionowy fragment.** Zmień tylko konieczne moduły, od domeny lub use case do adaptera i composition root. Nie dodawaj abstrakcji „na przyszłość”.
7. **Obsłuż wynik operacyjny.** Dodaj walidację granicy, jawne błędy oczekiwane, cancellation, telemetry bez sekretów i wymagany audyt.
8. **Przejrzyj diff.** Usuń przypadkową duplikację wiedzy, martwy kod, niedokończone placeholdery, niezamierzone breaking changes i pliki szablonowe.
9. **Waliduj proporcjonalnie.** Najpierw format i najwęższy build/test, potem pełną bramkę, jeśli zmiana przekracza jeden moduł lub kontrakt.

## Twarde reguły kodu

- Targetuj `net10.0` i stabilny C# 14. Nie ustawiaj `LangVersion` na `latest` ani `preview`.
- Jeden ręczny top-level named type przypada na jeden plik `.cs`; dotyczy klas, rekordów, struktur, interfejsów, enumów i delegatów. Nazwa pliku jest nazwą typu. Wyjątki: `Program.cs`, kod generowany i artefakty migracji EF.
- Używaj file-scoped namespaces w nowych plikach.
- Publiczne kontrakty są niezmienne; encje domenowe chronią stan metodami i niezmiennikami.
- Typy prywatne i wewnętrzne oznaczaj `sealed` albo `static`, jeśli dziedziczenie nie jest częścią projektu. Pola oznaczaj `readonly`, gdy referencja nie jest zmieniana.
- Wszystkie operacje I/O są async i przyjmują `CancellationToken` jako ostatni parametr. Zakazane są sync-over-async, `async void` poza event handlerem i porzucone `Task`.
- Używaj `Task` domyślnie; `ValueTask` wymaga wyniku pomiaru.
- Wstrzykuj jawne zależności. Nie używaj service locatora, globalnego stanu ani interfejsu dla każdej klasy.
- Waliduj options przy starcie. Nie wstrzykuj scoped service do singletona i nie współdziel `DbContext` między wątkami.

## SOLID, DRY, KISS i YAGNI

- **SOLID:** utrzymuj jedną odpowiedzialność, małe porty i zależności skierowane do stabilnego rdzenia. Duży konstruktor jest sygnałem do ponownej oceny odpowiedzialności, nie automatycznym powodem do fasady.
- **DRY:** usuwaj duplikację tej samej reguły lub wiedzy. Nie abstrahuj przypadkowo podobnej składni.
- **KISS:** preferuj jawny handler, mapping i przepływ sterowania nad reflection, magiczną rejestracją i zbędnym frameworkiem.
- **YAGNI:** nie projektuj rozszerzeń, wariantów providerów ani generic repository bez obecnego wymagania.

## Granice z innymi workflow

- Jeśli potrzebujesz nowego lub zaktualizowanego pakietu, wykonaj osobno `dotnet10-manage-dependencies`, zanim oprzesz implementację na tej zależności.
- Jeśli przyczyna defektu nie jest potwierdzona, najpierw użyj `dotnet10-diagnose-failure`.
- Jeśli zadanie nie może zmienić zachowania, użyj `dotnet10-refactor-safely`.

## Walidacja

Dobierz polecenia do zmiany, zachowując tę kolejność:

```bash
SOLUTION="path/to/YourSolution.slnx"
dotnet format "$SOLUTION" --no-restore --verify-no-changes
dotnet build "$SOLUTION" --no-restore
dotnet test "$SOLUTION" --no-build --logger "console;verbosity=minimal"
```

Jeśli zmiana dotyczy tylko jednego projektu, najpierw uruchom jego build i testy. Pełną bramkę uruchom dla zmian kontraktowych, przekrojowych, bezpieczeństwa i przed końcowym raportem, o ile środowisko na to pozwala. Podaj dokładne wyniki i niewykonane walidacje.

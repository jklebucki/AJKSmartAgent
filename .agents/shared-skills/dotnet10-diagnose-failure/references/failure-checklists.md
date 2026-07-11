# Warunkowe checklisty diagnostyczne

Wybierz sekcję na podstawie pierwszego potwierdzonego objawu. Nie wykonuj wszystkich checklist mechanicznie.

## Zawsze

- [ ] Zapisano dokładny reproducer, oczekiwanie i objaw.
- [ ] Sprawdzono `git status --short` bez naruszania cudzych zmian.
- [ ] Potwierdzono SDK, TFM, konfigurację i środowisko wykonania.
- [ ] Zachowano pierwszy istotny błąd, stack trace i inner exception.
- [ ] Oddzielono fakty od hipotez.
- [ ] Ustalono najwęższy projekt, test lub request wykazujący problem.
- [ ] Diagnoza wyjaśnia mechanizm i warunki, a nie tylko wskazuje linię.

## Jeśli zawodzi restore albo build

- [ ] Uruchomiono restore i build oddzielnie, aby rozdzielić błąd pakietów od kompilacji.
- [ ] Sprawdzono `global.json`, zainstalowane SDK oraz `Directory.Build.props`.
- [ ] Sprawdzono Central Package Management, lockfile i źródła NuGet.
- [ ] Ustalono, czy diagnostic pochodzi z kompilatora, analyzera, generatora czy MSBuild.
- [ ] Sprawdzono pełny komunikat pierwszego błędu oraz plik generowany w `obj`, jeśli generator jest podejrzany.
- [ ] Nie wyłączono warning-as-error ani analyzera w celu ukrycia objawu.
- [ ] Dla konfliktu typów sprawdzono namespace, assembly i zgodność nazwy pliku z pojedynczym top-level type.

Przydatne polecenia:

```bash
dotnet --info
SOLUTION="path/to/YourSolution.slnx"
dotnet restore "$SOLUTION" --verbosity normal
dotnet build "$SOLUTION" --no-restore --verbosity normal
dotnet list "$SOLUTION" package --include-transitive
```

## Jeśli zawodzi test

- [ ] Uruchomiono pojedynczy test wielokrotnie bez równoległych, niezwiązanych testów.
- [ ] Sprawdzono, czy test zawodzi samodzielnie i w całym zestawie.
- [ ] Oddzielono defekt produktu od defektu testu lub fixture.
- [ ] Sprawdzono zależność od czasu, kolejności, locale, timezone, losowości i globalnego stanu.
- [ ] Sprawdzono izolację danych, portów, plików i kontenerów.
- [ ] Dla async sprawdzono porzucone tasks, arbitralne delays i brak oczekiwania na obserwowalny warunek.
- [ ] Dla regression testu potwierdzono, że objaw odpowiada zgłaszanemu defektowi.

## Jeśli awaria występuje w runtime albo API

- [ ] Zapisano request, status, response body, correlation ID i odpowiadający trace.
- [ ] Sprawdzono kolejność middleware oraz authN/authZ.
- [ ] Sprawdzono binding, walidację granicy i mapowanie `ProblemDetails`.
- [ ] Prześledzono lifetime requestu; `HttpContext` nie jest używany po jego zakończeniu ani równolegle.
- [ ] Sprawdzono walidację options przy starcie i różnice konfiguracji środowisk.
- [ ] Sprawdzono timeout, cancellation i częściowe wykonanie side effectu.
- [ ] Potwierdzono, że błąd nie został zredagowany tak agresywnie, że utracono correlation ID lub kod błędu.

## Jeśli podejrzany jest EF Core lub PostgreSQL

- [ ] Sprawdzono lifetime `DbContext` oraz brak równoległych operacji na tej samej instancji.
- [ ] Odczytano wygenerowany SQL i parametry po bezpiecznej redakcji.
- [ ] Sprawdzono tracking, oczekiwaną liczbę rekordów i granice transakcji.
- [ ] Sprawdzono token optimistic concurrency i obsługę `DbUpdateConcurrencyException`.
- [ ] Sprawdzono N+1, cartesian explosion, nieograniczony wynik i brak indeksu.
- [ ] Dla retry zapisu sprawdzono idempotency key oraz możliwość podwójnego skutku.
- [ ] Dla migracji porównano model, snapshot, skrypt i stan historii migracji.

## Jeśli objaw dotyczy async, deadlocku lub race condition

- [ ] Wyszukano `.Result`, `.Wait()`, `async void`, porzucone tasks i niejawne fire-and-forget.
- [ ] Sprawdzono propagację `CancellationToken` i właściciela cyklu życia operacji.
- [ ] Sprawdzono scoped dependencies używane przez singleton lub background task po zamknięciu scope.
- [ ] Sprawdzono współdzielony mutable state oraz thread-safety kolekcji i klientów.
- [ ] Użyto trace/thread dump zamiast wnioskowania wyłącznie z kolejności logów.
- [ ] Sprawdzono, czy retry na wielu warstwach nie zwielokrotnia równoległości.

## Jeśli objaw dotyczy wydajności lub pamięci

- [ ] Zdefiniowano metrykę, baseline, obciążenie i budżet regresji.
- [ ] Użyto trace, profiler, query plan albo metryk runtime.
- [ ] Sprawdzono liczbę round trips i rozmiar danych przed mikrooptymalizacją C#.
- [ ] Sprawdzono materializację dużych payloadów, LOH i nieograniczone bufory.
- [ ] Sprawdzono blokowanie thread pool i sync I/O.
- [ ] Nie zalecono `ValueTask`, pooling ani cache bez pomiaru i analizy poprawności.
- [ ] Potwierdzono, że etykiety metryk nie powodują wysokiej kardynalności.

## Jeśli zawodzi kontener albo deployment

- [ ] Porównano architekturę obrazu, RID, tag/digest i wersję runtime.
- [ ] Sprawdzono użytkownika non-root, uprawnienia filesystemu i dostępne katalogi tymczasowe.
- [ ] Sprawdzono port, binding, readiness, liveness i graceful shutdown.
- [ ] Sprawdzono DNS, egress, TLS, certyfikaty i secrets mounts.
- [ ] Sprawdzono ICU, timezone i biblioteki natywne dla obrazu minimalnego/chiseled.
- [ ] Odtworzono problem na tym samym obrazie, nie tylko przez lokalne `dotnet run`.
- [ ] Nie uruchomiono migracji ani diagnostyki przeciw produkcji bez zgody.

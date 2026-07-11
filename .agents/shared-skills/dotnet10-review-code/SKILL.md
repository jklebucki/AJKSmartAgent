---
name: dotnet10-review-code
description: "Przegląda diff, PR lub kod .NET 10/C# 14 pod kątem defektów, bezpieczeństwa, kompatybilności, współbieżności, danych i testów. Użyj, gdy rezultatem mają być ustalenia bez edycji. Nie używaj do implementowania poprawek, diagnozy runtime, refaktoringu, pisania testów ani aktualizacji zależności."
---

# Przegląd kodu .NET 10

Znajdź konkretne, naprawialne defekty w zmianie. Review jest zadaniem odczytowym: nie modyfikuj plików, nie formatuj kodu i nie rozwiązuj znalezionych problemów bez osobnego polecenia.

## Workflow

1. **Ustal zakres.** Odczytaj `AGENTS.md`, `git status --short`, diff i pełny kontekst zmienionych metod. Nie oceniaj fragmentu bez kontraktu jego wywołań.
2. **Zrozum intencję.** Ustal kryteria akceptacji, niezmienniki, granice zaufania i poprzednie zachowanie. Sprawdź testy zmiany.
3. **Sklasyfikuj obszar.** Wybierz odpowiednie sekcje z [checklist review](references/review-checklists.md): domena, async, ASP.NET, EF Core, integracje, security, observability, performance lub testy.
4. **Prześledź ścieżki.** Sprawdź happy path, oczekiwany błąd, cancellation, współbieżność, retry i częściowe wykonanie. Dla skutków ubocznych prześledź idempotencję i commit.
5. **Zweryfikuj podejrzenia.** Odczytaj definicje, wywołania i konfigurację. Jeśli to bezpieczne, uruchom test, build lub statyczną walidację, ale nie używaj poleceń naprawiających.
6. **Oceń ważność.** Nadaj priorytet według realnego wpływu i prawdopodobieństwa, nie według preferencji stylistycznej.
7. **Przedstaw findings.** Najpierw findings od najwyższego priorytetu, następnie pytania i krótki zakres walidacji. Jeśli nie ma findings, napisz to wprost i wskaż pozostałe ryzyko testowe.

## Standard dowodu

Każdy finding zawiera:

- krótki tytuł z priorytetem `P0`–`P3`;
- dokładny plik i możliwie wąski zakres linii;
- warunek, w którym defekt się ujawnia;
- obserwowalną konsekwencję;
- wyjaśnienie mechanizmu, nie tylko sugestię rozwiązania.

Nie zgłaszaj findingu, jeśli nie potrafisz wskazać scenariusza i wpływu. Pytanie albo preferencję oznacz jako takie.

## Reguły review

- Najpierw correctness i security, potem kompatybilność, niezawodność, dane, performance i maintainability.
- Wynik modelu, dane witryny i wejście API są niezaufane; sprawdź deterministyczne policy i walidację granic.
- Sprawdź async end-to-end, propagację `CancellationToken`, brak sync-over-async i brak porzuconych tasks.
- Sprawdź lifetimes DI, zwłaszcza scoped w singletonie, disposal oraz użycie `DbContext` na wielu wątkach.
- Sprawdź retry nieidempotentnych operacji, timeout, circuit breaker i możliwość zwielokrotnienia retry.
- Sprawdź redakcję sekretów i PII, kardynalność metryk oraz oddzielenie audytu od telemetry.
- Dla ręcznego kodu wymagaj jednego top-level named type na plik oraz nazwy pliku zgodnej z typem. Traktuj naruszenie jako finding tylko wtedy, gdy łamie jawny standard repozytorium; podaj właściwy priorytet, zwykle `P3`.
- SOLID, DRY, KISS i YAGNI oceniaj pragmatycznie. Nie żądaj abstrakcji bez aktualnego problemu ani scalenia kodu reprezentującego różne reguły.
- Nie zgłaszaj samego formatowania, jeśli automatyczna bramka je rozstrzyga i nie powoduje defektu.
- Kod, przykłady i komunikaty techniczne zapisuj po angielsku; opis findings po polsku.

## Walidacja odczytowa

Dozwolone są między innymi:

```bash
SOLUTION="path/to/YourSolution.slnx"
dotnet format "$SOLUTION" --no-restore --verify-no-changes
dotnet build "$SOLUTION" --no-restore
dotnet test "$SOLUTION" --no-build --logger "console;verbosity=minimal"
```

Nie uruchamiaj `dotnet format` bez `--verify-no-changes`, migracji, testów przeciw produkcji ani poleceń modyfikujących lockfile. Raportuj tylko walidacje faktycznie wykonane.

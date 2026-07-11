---
name: dotnet10-diagnose-failure
description: "Diagnozuje awarie restore, build, testów, uruchomienia, wydajności i kontenerów .NET 10/C# 14 na podstawie dowodów. Użyj do ustalenia przyczyny lub raportu diagnostycznego. Nie używaj, gdy celem jest implementacja poprawki, refaktoring, review, testy albo zmiana zależności."
---

# Diagnozowanie awarii .NET 10

Ustal przyczynę, mechanizm i zasięg awarii bez zmieniania plików ani stanu zewnętrznego. Nie przedstawiaj hipotezy jako przyczyny, dopóki dowód nie łączy jej z objawem.

## Granica działania

- Wykonuj tylko odczyt i odwracalne polecenia diagnostyczne.
- Nie implementuj poprawki, nie formatuj plików i nie aktualizuj pakietów.
- Jeżeli użytkownik zlecił także naprawę, zakończ diagnozę jednoznaczną przyczyną, a implementację przekaż do workflow `dotnet10-implement-change`.
- Traktuj istniejące zmiany w worktree jako własność użytkownika.

## Workflow

1. **Zdefiniuj objaw.** Zapisz oczekiwane zachowanie, zachowanie rzeczywiste, dokładne polecenie lub request, środowisko i moment wystąpienia.
2. **Sprawdź kontekst.** Odczytaj `AGENTS.md`, właściwy projekt, `global.json`, wspólne pliki MSBuild i `git status --short`. Dla problemów narzędziowych zbierz `dotnet --info`.
3. **Odtwórz minimalnie.** Zacznij od najwęższego projektu, testu, endpointu albo procesu, który nadal wykazuje objaw. Zachowaj pełny pierwszy istotny błąd i jego inner exception; późniejsze błędy mogą być skutkiem.
4. **Sklasyfikuj awarię.** Wybierz tylko odpowiednią sekcję z [checklist diagnostycznych](references/failure-checklists.md): restore/build, test, runtime/API, EF Core, async/concurrency, performance albo container/deployment.
5. **Utwórz hipotezy.** Uszereguj je według zgodności z dowodem. Dla każdej zapisz obserwację, która ją potwierdzi lub obali.
6. **Izoluj jedną zmienną.** Porównuj wejścia, konfigurację, lifetime, wersje, dane i ścieżkę wywołań. Nie zmieniaj kilku warunków naraz.
7. **Potwierdź łańcuch przyczynowy.** Wskaż miejsce powstania defektu, mechanizm prowadzący do objawu oraz warunki konieczne do jego wystąpienia. Odrzuć najbliższe alternatywy konkretnym dowodem.
8. **Zweryfikuj diagnozę.** Ponownie uruchom minimalny reproducer albo wykonaj niezależny odczyt potwierdzający przewidywanie diagnozy. Jeśli reprodukcja jest niemożliwa, oznacz poziom pewności i brakujący dowód.

## Reguły techniczne

- Nie używaj `.Result`, `.Wait()` ani przypadkowego `Task.Run` jako narzędzia do „sprawdzenia”, czy async jest problemem.
- Pamiętaj, że `DbContext` nie jest thread-safe; niezakończone wywołanie EF może uszkodzić dalszą diagnozę tej instancji.
- Odróżniaj problem kodu od problemu środowiska, danych, konfiguracji, wersji SDK i zależności.
- Wydajność diagnozuj pomiarem: query plan, metryką, trace albo profilem. Sama inspekcja kodu nie dowodzi bottlenecku.
- Naruszenie reguły jednego ręcznego top-level named type na plik raportuj, jeśli wpływa na błąd lub czytelność przyczyny; nie naprawiaj go w tym workflow.
- Kod, identyfikatory, komunikaty techniczne, komentarze i polecenia pozostawiaj po angielsku. Raport dla użytkownika pisz po polsku.

## Raport końcowy

Podaj kolejno:

1. przyczynę główną i poziom pewności;
2. krótki łańcuch przyczynowy;
3. dowody z plikami, liniami, poleceniami lub trace;
4. zasięg i warunki wystąpienia;
5. najwęższe warianty naprawy, wyraźnie oznaczone jako niewdrożone;
6. wykonane walidacje oraz ograniczenia diagnozy.

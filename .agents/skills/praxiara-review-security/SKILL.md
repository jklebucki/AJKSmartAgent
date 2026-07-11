---
name: praxiara-review-security
description: Przeprowadza wyłącznie odczytowy audyt bezpieczeństwa Praxiary. Użyj tylko na jawne żądanie review prompt injection, egress, approvals, tenant isolation, sekretów, IFS lub evidence. Nie używaj do zwykłego code review, wdrażania poprawek, pentestu, skanowania produkcji ani wykonywania workflow.
---

# Odczytowy audyt bezpieczeństwa Praxiary

Znajduj wykonalne ścieżki nadużycia i raportuj je z konkretnymi dowodami. Nie zmieniaj kodu, konfiguracji, dokumentacji, zależności, Git ani systemów zewnętrznych w ramach tego skill.

## Nienegocjowalne granice

- Działaj wyłącznie po jawnym wywołaniu audytu bezpieczeństwa.
- Zachowaj tryb read-only. Nie stosuj poprawek, nie generuj migracji i nie aktualizuj pakietów.
- Nie uruchamiaj DAST, exploitów, live replay ani automatyzacji przeciw produkcji.
- Nie otwieraj aktywnej sesji użytkownika, nie pobieraj cookies i nie próbuj omijać auth, MFA, CAPTCHA, egress ani policy.
- Nie ujawniaj sekretu w raporcie. Zapisz typ sekretu, lokalizację i zredagowany dowód.
- Nie traktuj braku dowodu jako potwierdzenia bezpieczeństwa lub zgodności prawnej.
- Jeśli użytkownik po audycie chce napraw, zakończ raport i przejdź do osobnego zadania implementacyjnego z właściwym skill.

## Obowiązkowy kontekst

1. Przeczytaj `AGENTS.md`, szczególnie niezmienniki bezpieczeństwa, supply chain, testy i czynności zabronione.
2. Przeczytaj właściwe granice i wymagania w `docs/PLAN_WYTWORZENIA.md`.
3. Przeczytaj w całości [checklistę audytu bezpieczeństwa](references/security-review-checklist.md).
4. Sprawdź `git status --short`, zakres diffu i istniejące testy bezpieczeństwa bez modyfikowania plików.

## Workflow audytu

### 1. Potwierdź zakres i autoryzację

Zapisz:

- audytowane ścieżki, komponenty i wersję kodu;
- czy zakres obejmuje diff, moduł czy pełne repozytorium;
- dozwolone działania diagnostyczne;
- jawnie wyłączone systemy, środowiska i dane;
- znane założenia oraz brakujące artefakty.

Jeśli zakres wymaga aktywnego testu systemu zewnętrznego bez jednoznacznej autoryzacji, zatrzymaj tę część i wykonaj wyłącznie przegląd statyczny.

### 2. Zmapuj przepływ i granice zaufania

- Wskaż aktywa, dane Restricted/Confidential, tożsamości oraz punkty wykonania skutku.
- Prześledź wejście od UI, strony, pliku, IFS i LLM przez parser, policy, executor, verifier i audit.
- Zidentyfikuj przejścia między public UI, control plane, execution, model, IFS i evidence zone.
- Sprawdź, gdzie dane niezaufane mogą zostać pomylone z instrukcją albo autoryzacją.

Nie ograniczaj się do nazwy klasy. Śledź rzeczywisty source, transformacje, guard, sink i stan po błędzie.

### 3. Sprawdź kontrolki domenowe

Przejdź przez checklistę co najmniej dla:

- uwierzytelnienia, autoryzacji, tenant isolation i service identities;
- prompt injection, structured output i katalogu typowanych narzędzi;
- allowlisty domen, redirectów, SSRF, DNS rebinding i kanałów bocznych egress;
- izolacji Browser Worker, sesji, takeover lease, plików i cleanup;
- klasyfikacji R0–R5, approval, canonical action hash i TOCTOU;
- idempotencji, retry, `OutcomeUnknown`, verifier i audytu;
- IFS Projection/API, delegacji użytkownika, routingu i Aurena Agent;
- redakcji promptów, logów, traces, screenshotów, downloads i evidence;
- zależności, obrazów, modeli, licencji, SBOM i konfiguracji deploymentu.

### 4. Udowodnij albo odrzuć hipotezę

Dla każdego potencjalnego problemu:

1. wskaż kontrolowane przez atakującego źródło;
2. pokaż osiągalny sink lub zmianę stanu;
3. sprawdź wszystkie istniejące guardy;
4. określ wymagane uprawnienia i warunki;
5. opisz skutek dla poufności, integralności, dostępności lub audytowalności;
6. znajdź test, który dowodzi ochrony, albo opisz brakujący test.

Odrzuć hipotezę, jeśli ścieżkę deterministycznie blokuje wcześniejsza kontrolka. Nie raportuj samej możliwości teoretycznej jako podatności.

### 5. Klasyfikuj ustalenia

- `Critical` — realna ścieżka do nieautoryzowanego R4/R5, cross-tenant access, przejęcia poświadczeń lub wykonania kodu w granicy zaufania.
- `High` — realna eskalacja uprawnień, eksfiltracja danych, obejście approval/egress albo brak rekoncyliacji mogący powtórzyć skutek.
- `Medium` — ograniczone nadużycie lub istotna słabość defense-in-depth wymagająca dodatkowych warunków.
- `Low` — konkretna luka wzmacniająca albo brak dowodu, który nie tworzy samodzielnej ścieżki nadużycia.

Podaj również confidence. Nie podnoś severity za sam brak testu, jeśli nie wykazałeś wpływu.

### 6. Przygotuj raport

Najpierw podaj ustalenia od najwyższej severity. Każde ustalenie zawiera:

- krótki tytuł;
- severity i confidence;
- dokładny plik oraz możliwie wąski zakres linii;
- warunki wstępne i ścieżkę nadużycia;
- wpływ;
- istniejące kontrolki i przyczynę ich nieskuteczności;
- minimalny kierunek naprawy bez implementacji;
- wymagany test regresyjny.

Po ustaleniach wymień ograniczenia audytu i obszary niezweryfikowane. Jeśli nie ma potwierdzonych ustaleń, napisz to wprost; nie dodawaj uwag stylistycznych, aby wypełnić raport.

## Bezpieczeństwo jakości kodu

Sprawdź reguły .NET tylko wtedy, gdy wpływają na kontrolkę bezpieczeństwa:

- pojedynczy główny typ top-level na plik i jednoznaczna odpowiedzialność pomagają przejrzeć policy, approval i verifier;
- SOLID wymaga, aby policy było niezależne od LLM i executora;
- DRY nie może centralizować danych niezaufanych oraz instrukcji w jednym niestrukturyzowanym helperze;
- KISS zabrania refleksyjnego dispatchu, dynamicznych narzędzi i ogólnych wrapperów HTTP, jeśli typowany kontrakt wystarcza.

Nie raportuj zwykłego naruszenia stylu jako security finding bez konkretnego wpływu.

## Dozwolona weryfikacja

Wykonuj wyłącznie statyczny odczyt kodu, konfiguracji, lockfiles i już istniejących wyników testów. Nie uruchamiaj buildów, testów, kontenerów, przeglądarki, skanerów ani replay, ponieważ mogą zapisywać artefakty lub zmieniać stan. Jeśli dowód wymaga wykonania `Praxiara.SecurityTests`, `Praxiara.TestSites` albo innej diagnostyki, zaproponuj osobne, jawnie autoryzowane zadanie i opisz bezpieczny zakres; nie wykonuj go w ramach tego audytu.

## Warunki zatrzymania

Zatrzymaj aktywną część audytu i zgłoś ograniczenie, gdy:

- zakres lub własność systemu są niejasne;
- potrzebny byłby sekret, prywatny token lub dane klienta;
- test mógłby wykonać skutek biznesowy albo dotknąć produkcji;
- wymagany byłby destructive command, arbitrary egress lub obejście zabezpieczenia;
- dowód wymaga rozszerzenia autoryzowanego zakresu.

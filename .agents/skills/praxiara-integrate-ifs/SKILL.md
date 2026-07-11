---
name: praxiara-integrate-ifs
description: Integruje Praxiarę z IFS Cloud przez Projection/OData oraz trasy API, Browser lub Hybrid. Użyj dla klienta IFS, rejestru projekcji, profilu środowiska, routingu, narzędzia IFS, weryfikatora lub Aurena Agent. Nie używaj do dowolnej witryny, samego YAML business skill ani operacji na produkcyjnym IFS.
---

# Integracja Praxiary z IFS Cloud

Projektuj integrację API-first, zachowując uprawnienia użytkownika i niezależną kontrolę każdej operacji skutkowej. Traktuj UI jako uzasadniony fallback lub część jawnej trasy hybrydowej, nie jako domyślny transport.

## Obowiązkowy kontekst

1. Przeczytaj `AGENTS.md`, zwłaszcza reguły IFS, approvals, audytu, zależności i testów.
2. Przeczytaj właściwe fragmenty `docs/PLAN_WYTWORZENIA.md`: wymagania IFS, API-first, Aurena, ryzyko i kryteria akceptacji.
3. Przeczytaj w całości [checklistę integracji IFS](references/ifs-integration-checklist.md).
4. Sprawdź istniejące kontrakty w `src/Praxiara.Integrations.IFS`, porty w `Praxiara.Application` oraz najbliższe testy.
5. Sprawdź `git status --short` i nie modyfikuj zmian spoza zadania.

## Workflow

### 1. Ustal kontrakt biznesowy

- Zapisz operację w języku biznesowym, wejścia, wynik, konsekwencję i kryterium sukcesu.
- Określ użytkownika, tenant, profil środowiska, projection, wymagane permission, klasę danych i poziom R0–R5.
- Oddziel odczyt, przygotowanie podglądu, commit i niezależną weryfikację.
- Dla zmiany skutkowej określ idempotency semantics oraz wynik po timeoutcie po wysłaniu żądania.

Zatrzymaj pracę, jeśli nie można jednoznacznie wskazać rekordu, skutku albo właściciela procesu.

### 2. Zbadaj wspierane API

- Korzystaj z zatwierdzonego eksportu API Explorer lub `$openapi`; nie pozwalaj modelowi odkrywać endpointów w runtime.
- Potwierdź projection, entity set, function/action, schema, uprawnienia, query options i ograniczenia wersji IFS.
- Dodaj wpis do wersjonowanego rejestru projekcji przed udostępnieniem operacji plannerowi.
- Nie buduj dowolnego `HttpClient` proxy. Bazowy host, projection, action i ścieżka muszą przejść allowlistę oraz walidację kontraktu.

Jeśli oficjalne API jest nieznane lub brak sandboxu do potwierdzenia kontraktu, przygotuj bezpieczny projekt albo spike; nie deklaruj gotowej integracji.

### 3. Wybierz trasę

Wybierz dokładnie jedną trasę dla każdego etapu:

- `ProjectionApi` — API wykonuje operację i umożliwia weryfikację;
- `Browser` — wspieranego API brak, a kontrolowany proces UI jest jedyną zatwierdzoną drogą;
- `Hybrid` — API dostarcza odczyt, preview lub weryfikację, a tylko konieczny krok korzysta z UI.

Zapisz uzasadnienie wyboru i warunki, przy których routing ma odmówić działania. Nie stosuj cichego fallbacku między trasami.

### 4. Zmień właściwą warstwę

- Umieszczaj porty potrzebne przypadkowi użycia po stronie konsumenta, a szczegóły IFS w `Praxiara.Integrations.IFS`.
- Eksponuj plannerowi wysokopoziomowe, typowane narzędzia biznesowe, np. `ifs_find_customer_invoice`, nie surowe OData ani URL.
- Modeluj spodziewane wyniki osobno: sukces, brak uprawnienia, wygasła sesja, konflikt wersji, błąd biznesowy, błąd przejściowy i `OutcomeUnknown`.
- Waliduj wejście na granicy procesu i ponownie sprawdzaj niezmienniki bezpieczeństwa przed skutkiem.
- Przekazuj `CancellationToken` jako ostatni parametr każdej operacji I/O i używaj wstrzykniętego `TimeProvider`.

Stosuj jeden główny typ top-level na plik. Nazwij plik tak jak klasę, rekord, interfejs, enum lub strukturę. Jeśli dotykany plik zawiera kilka typów, rozdziel je bez zmiany publicznego zachowania. Zachowuj SOLID, DRY i KISS: nie dodawaj ogólnego repozytorium, mappera, managera ani abstrakcji bez realnego wariantu lub granicy zaufania.

### 5. Zabezpiecz operację skutkową

- Klasyfikuj ryzyko deterministycznie poza LLM.
- Dla R4/R5 przygotuj konkretny preview oraz approval związane z action hash i aktualnymi bound facts.
- Przed wykonaniem ponownie odczytaj target, permission, environment, skill/policy version i istotne wartości.
- Nie retry'uj automatycznie operacji nieidempotentnej po niepewnym wyniku.
- Blokuj R4/R5, jeśli nie można zapisać audytu przed wykonaniem.

### 6. Zweryfikuj wynik

- Sprawdź semantyczny wynik API, a nie tylko status HTTP.
- Dla `Browser` lub `Hybrid` sprawdź komunikat i stan UI oraz, gdy to możliwe, wykonaj niezależny odczyt Projection API lub historii.
- Zapisz correlation IDs, trasę, projection, wersję kontraktu, zredagowane argumenty, approval, wynik i evidence.
- Ustaw `OutcomeUnknown`, gdy nie da się potwierdzić skutku; nie przedstawiaj go jako failure ani success.

### 7. Dodaj dowody testowe

Pokryj co najmniej:

- wybór każdej wspieranej trasy i fail-closed dla braku wpisu;
- allowlistę projection/action oraz odrzucenie dowolnego hosta i ścieżki;
- brak permission, wygasłą sesję, konflikt kontraktu i błąd biznesowy;
- R4/R5, zmianę bound facts, wygaśnięcie approval i `OutcomeUnknown`;
- weryfikację UI+API i redakcję tokenów/payloadów;
- locale, custom projection oraz wspieraną wersję release IFS;
- brak Aurena companion i odmowę niebezpiecznego fallbacku.

Używaj wyłącznie sandboxu IFS, deterministycznych fixture albo `Praxiara.TestSites`. Nigdy nie uruchamiaj testu na produkcyjnym IFS.

## Warunki zatrzymania

Zatrzymaj implementację i zgłoś brak decyzji, gdy:

- operacja wymaga szerszego service account niż użytkownik i nie ma zatwierdzonej delegacji;
- projection/action nie znajduje się w zatwierdzonym rejestrze;
- jedynym rozwiązaniem byłby arbitrary HTTP, JavaScript, shell, cookie albo ścieżka hosta;
- funkcja wymaga Aurena Agent, a zatwierdzony Windows companion nie istnieje;
- nie ma bezpiecznej strategii idempotencji lub rekoncyliacji;
- wymagany test miałby dotknąć produkcji albo rzeczywistych danych klienta.

## Weryfikacja i raport

Uruchom najwęższe testy projektu, następnie odpowiednie bramki z `AGENTS.md`. W raporcie podaj:

- wybraną trasę i jej uzasadnienie;
- zmienione kontrakty i profile;
- klasę ryzyka, approval, idempotencję i verifier;
- wykonane testy oraz ich wynik;
- niepotwierdzone założenia, ograniczenia IFS i wymagane działania administratora.

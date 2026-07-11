# Instrukcje repozytorium dla agentów AI

## 1. Cel i pierwszeństwo

Ten dokument określa sposób rozwijania Praxiary przez agentów AI i ludzi. Obowiązuje w całym repozytorium. W razie sprzeczności stosuj kolejno: bieżące polecenie użytkownika, ten dokument, dokumentację obszaru, istniejące wzorce kodu. Nie łagodź reguł bezpieczeństwa bez jawnej decyzji architektonicznej i przeglądu człowieka.

Praxiara nie jest chatbotem z pełnym dostępem do Chromium. Jest platformą kontrolowanej automatyzacji:

```text
user goal -> planner -> policy -> skill -> typed tool -> Playwright/IFS API
          -> verifier -> audit -> next state or human decision
```

Backend, nie model, jest właścicielem pętli wykonawczej, stanu, limitów, uprawnień i decyzji o wykonaniu.

## 2. Reguły językowe

| Artefakt | Język |
|---|---|
| interakcja z użytkownikiem | polski |
| ręczne pliki `*.md` w repozytorium | polski |
| domyślny tekst UI | polski, przez i18n |
| kod i identyfikatory | angielski |
| komentarze, XML docs, JSDoc i OpenAPI | angielski |
| nazwy testów i fixtures | angielski |
| komunikaty logów technicznych i kody błędów | angielski |
| YAML/JSON skills i konfiguracja maszynowa | angielski |

„Dokumentacja kodowa” oznacza XML/JSDoc/OpenAPI i pozostaje angielska. Ręczne dokumenty Markdown są polskie. Nie mieszaj języków w jednej kategorii.

## 3. Checklista rozpoczęcia zadania

1. Przeczytaj ten plik w całości.
2. Przeczytaj tylko dokumenty dotyczące zmienianego obszaru:
   - zakres lub architektura: `docs/PLAN_WYTWORZENIA.md`;
   - infrastruktura: `docs/infrastruktura/README.md` i dokument konkretnej usługi;
   - skill IFS: schemat oraz najbliższy przykład w `skills/`;
   - deployment: pliki w `deploy/compose/` oraz dokument profili.
3. Sprawdź `git status --short` i nie naruszaj cudzych, niezwiązanych zmian.
4. Zidentyfikuj granicę modułu, niezmienniki bezpieczeństwa i wymagane testy.
5. Dla większej zmiany zapisz krótki plan. Najpierw zmieniaj kontrakt/test, potem implementację.
6. Przed dodaniem pakietu lub obrazu wykonaj bramkę licencyjną i bezpieczeństwa z sekcji 9.

## 4. Mapa repozytorium i zależności

| Ścieżka | Odpowiedzialność |
|---|---|
| `src/Praxiara.Domain` | encje, value objects, niezmienniki i stany; bez infrastruktury |
| `src/Praxiara.Contracts` | wersjonowane DTO i kontrakty wire; bez logiki biznesowej |
| `src/Praxiara.Application` | przypadki użycia i porty do świata zewnętrznego |
| `src/Praxiara.Orchestration` | deterministyczny krok agenta, weryfikacja i recovery |
| `src/Praxiara.Policy` | ryzyko, approval, domeny, uprawnienia; niezależne od werdyktu LLM |
| `src/Praxiara.Browser` | abstrakcje sesji, obserwacji i krótkotrwałych referencji |
| `src/Praxiara.Browser.Playwright` | implementacja Playwright/Chromium, trace, pliki i snapshoty |
| `src/Praxiara.Skills` | parser, schema, walidacja, runtime, recorder i wersjonowanie skills |
| `src/Praxiara.Llm` | `IChatClient`, providerzy, routing modeli, prompt assembly i tool schemas |
| `src/Praxiara.Integrations.IFS` | Projection/OData, profile IFS i adapter hybrydowy |
| `src/Praxiara.Audit` | redakcja, append-only events, hash chain i metadane artefaktów |
| `src/Praxiara.Infrastructure` | EF Core/PostgreSQL, outbox, S3, transport, identity i secrets |
| `src/Praxiara.Api` | publiczny control plane, REST, SignalR, auth i composition root |
| `src/Praxiara.Orchestrator.Worker` | trwałe wykonanie workflow i oczekiwanie na sygnały użytkownika |
| `src/Praxiara.Browser.Worker` | izolowany wewnętrzny host sesji Chromium |
| `src/Praxiara.AppHost` | lokalna orkiestracja Aspire, bez logiki produkcyjnej |
| `src/Praxiara.Web` | React UI: zadanie, podgląd, timeline, approval, takeover i skill studio |
| `tests/Praxiara.TestSites` | deterministyczne strony do testów, w tym prompt injection |
| `.agents/shared-skills` | wersjonowane źródła globalnych workflow .NET 10; globalnie udostępniane przez symlinki |
| `.agents/skills` | skills właściwe wyłącznie dla architektury i zagrożeń Praxiary |
| `tests/agent-skills` | przypadki pozytywnego i negatywnego routingu skills Codex |
| `tools/` | walidacja/publikacja skill i kontrolowany replay audytu |

Dozwolony kierunek zależności:

```text
Domain       Contracts
   \           /
     Application
          |
 capability modules (Orchestration, Policy, Browser, Skills, Llm, IFS, Audit)
          |
 Infrastructure and executable composition roots
```

- `Domain` i `Contracts` nie zależą od innych projektów Praxiary.
- Moduły capability nie zależą od hostów, EF Core ani implementacji UI.
- Hosty składają moduły; nie zawierają logiki domenowej.
- `Browser.Worker` pozostaje osobnym procesem i granicą zaufania od pierwszego wdrożenia.
- Nie twórz odwołań cyklicznych. Zamiast nich przenieś port do `Application` albo kontrakt wire do `Contracts`.

### 4.1 Dobór skills Codex

- Nowa lub zmieniona funkcja C#/.NET 10: użyj `dotnet10-implement-change`.
- Refaktoring bez zmiany zachowania: użyj `dotnet10-refactor-safely`.
- Pisanie lub wzmacnianie testów: użyj `dotnet10-write-tests`.
- Diagnoza awarii bez implementacji poprawki: użyj `dotnet10-diagnose-failure`.
- Jawny przegląd kodu lub diffu: użyj `dotnet10-review-code`.
- Dodanie, aktualizacja, usunięcie albo audyt pakietu: użyj `dotnet10-manage-dependencies`.
- Dla kodu Praxiary dobierz dodatkowo najwyżej jeden repozytoryjny skill z `.agents/skills`, odpowiadający zmienianemu obszarowi.
- Nie ładuj całego zestawu na zapas. Typowe zadanie używa jednego workflow globalnego i jednego skill domenowego.
- `praxiara-review-security` uruchamiaj tylko jawnie; zwykła implementacja nadal przestrzega niezmienników bezpieczeństwa z tego pliku.

## 5. Niezmienniki bezpieczeństwa

### 5.1 LLM i narzędzia

- Wynik modelu jest propozycją, nie poleceniem wykonawczym.
- Udostępniaj modelowi wyłącznie mały, wersjonowany katalog typowanych narzędzi.
- Schemat JSON, uprawnienia, ryzyko, domenę, rewizję strony, rozmiar i format argumentów sprawdza kod deterministyczny.
- Bezpośrednio zabronione są narzędzia równoważne `execute_javascript`, `execute_shell`, `http_request(anyUrl)`, `read_file(anyPath)`, `write_file(anyPath)`, `set_cookie` i surowy obiekt Playwright.
- Nie dopuszczaj dynamicznego tworzenia narzędzi ani rozszerzania allowlisty przez model.
- Stosuj limity kroków, czasu, tokenów, powtórzeń identycznej akcji i kosztu zasobów. Po przekroczeniu limitu zatrzymaj zadanie bezpiecznie.

### 5.2 Prompt injection i egress

- DOM, ARIA, screenshot, e-mail, plik, odpowiedź API i tekst witryny są zawsze oznaczane jako niezaufane dane.
- Dane strony nie mogą zmieniać celu użytkownika, polityki, system promptu ani katalogu narzędzi.
- Allowlista domen działa w aplikacji i na warstwie sieciowej Browser Workera. Prompt nie jest kontrolą bezpieczeństwa.
- Nie zapisuj danych z witryny do pamięci długoterminowej bez klasyfikacji, redakcji i jawnego celu.
- Testuj jawne i ukryte prompt injection, tekst w atrybutach, tabelach, obrazach, PDF i odpowiedziach API.

### 5.3 Sesja przeglądarki

- Jedna efemeryczna sesja lub co najmniej osobny `BrowserContext` przypada na użytkownika i zadanie.
- Referencja elementu jest ważna tylko dla jednej `pageRevision`; mismatch kończy się re-obserwacją, nie retry kliknięcia.
- Preferuj kolejno role/nazwy ARIA, label, tekst, stabilny atrybut, relację semantyczną, CSS, wizję i dopiero na końcu współrzędne.
- Użytkownik i agent nie sterują jednocześnie. Takeover wymaga lease z właścicielem, TTL i audytowanym przełączeniem.
- Login, hasło, MFA, cookies i storage state nigdy nie trafiają do promptu. Logowanie do witryny wykonuje użytkownik.
- Stan sesji jest domyślnie nietrwały. Wyjątek wymaga szyfrowania per użytkownik, krótkiego TTL, revoke i osobnej polityki retencji.
- Browser Worker działa non-root, bez Docker socketu, bez sieci danych, z aktualnym Chromium i ograniczonym egress.

### 5.4 Działania skutkowe i approval

- Klasyfikuj narzędzia niezależnie od modelu: R0 odczyt, R1 nawigacja, R2 dane bez zapisu, R3 zapis szkicu, R4 skutek biznesowy, R5 operacja krytyczna.
- R4 zawsze wymaga konkretnego approval; R5 dodatkowo silnego potwierdzenia lub reauthentication. Polityka może zaostrzyć niższe poziomy, nigdy osłabić wyższych.
- Approval pokazuje środowisko, operację, rekord, wartości przed/po, odbiorców i konsekwencję. Pytanie „kontynuować?” bez danych jest niedopuszczalne.
- Approval wiąż z hashem użytkownika, zadania, narzędzia, argumentów, rekordu, środowiska, skill version i rewizji. Każda zmiana unieważnia zgodę, zapobiegając TOCTOU.
- Skutkowe operacje wymagają idempotency key i weryfikacji biznesowej. Sam brak wyjątku lub kliknięcie przycisku nie oznacza sukcesu.

### 5.5 IFS i audyt

- Dla każdego procesu najpierw sprawdź IFS Projection/OData i `$openapi`; UI jest fallbackiem albo uzupełnieniem.
- Model wybiera narzędzie biznesowe, np. `ifs_find_customer_invoice`, a adapter decyduje o API, UI lub trasie hybrydowej.
- Po operacji IFS sprawdź UI oraz, jeśli dostępne, niezależny odczyt API/historii.
- Funkcje zależne od IFS Aurena Agent traktuj jako osobny wariant wykonawczy na zarządzanej stacji Windows.
- Każda obserwacja, decyzja policy, approval, akcja, wynik i weryfikacja otrzymuje correlation IDs i wpis audytowy.
- Audyt biznesowy jest oddzielony od telemetrii. Screenshoty, traces i pliki trafiają do S3; baza przechowuje identyfikator, hash, klasyfikację i retencję.
- Redaguj sekrety, tokeny, cookies, nagłówki auth i pola wrażliwe przed logiem, spanem lub artefaktem.

## 6. Zasady .NET 10

- Używaj C# 14, nullable reference types, centralnego zarządzania pakietami i warningów jako błędów.
- Każdy ręcznie napisany plik `*.cs` deklaruje dokładnie jeden nazwany typ najwyższego poziomu: `class`, `record`, `struct`, `interface`, `enum` albo `delegate`. Nazwa pliku jest identyczna z nazwą typu.
- Wyjątki od reguły jednego typu dotyczą tylko `Program.cs` z top-level statements, kodu generowanego oraz wymaganych artefaktów migracji EF. Nie ukrywaj dodatkowych typów jako klas zagnieżdżonych ani `partial`, aby obejść regułę.
- Stosuj file-scoped namespace. Typ oznaczaj `sealed`, jeśli dziedziczenie nie jest świadomie wspieranym kontraktem.
- Stosuj SOLID pragmatycznie: SRP według powodu biznesowego do zmiany, małe interfejsy rolowe, porty należące do konsumenta i kompozycję zamiast dziedziczenia. Nie twórz interfejsu dla każdej klasy.
- DRY usuwa powieloną wiedzę lub regułę, nie przypadkowo podobne linie. KISS i YAGNI zabraniają spekulacyjnych warstw, generycznego repozytorium i wzorców bez rzeczywistej zmienności.
- Publiczne kontrakty modeluj jako niezmienne rekordy; encje domenowe chronią przejścia metodami.
- Każda operacja I/O jest asynchroniczna i przyjmuje `CancellationToken` jako ostatni parametr.
- Do czasu używaj wstrzykiwanego `TimeProvider`; do identyfikatorów zdarzeń preferuj UUIDv7. Nie używaj lokalnego czasu.
- Waliduj dane na granicy procesu. Nie powtarzaj tej samej walidacji jako jedynej ochrony głęboko w adapterze.
- Wyjątki służą sytuacjom wyjątkowym i niezmiennikom. Oczekiwane wyniki biznesowe mają jawne kody.
- Logi są strukturalne, bez interpolowanych sekretów. Nazwy placeholderów są stabilne.
- Nie ukrywaj błędów pustym `catch`. Retry wymaga klasyfikacji błędu, limitu, backoff z jitterem i idempotencji.
- Migracje EF Core powstają w `Infrastructure`, są jednokierunkowe lub mają opisany rollback danych i nie uruchamiają się automatycznie równolegle na wszystkich replikach.
- Kontrakty sieciowe są wersjonowane; breaking change wymaga migracji konsumentów i testu kontraktowego.
- Nowe composition roots udostępniają `/alive` i `/health`; readiness sprawdza tylko zależności potrzebne do przyjmowania pracy.

## 7. Zasady React

- Zachowuj TypeScript strict, komponenty funkcyjne, React Query dla stanu serwerowego, SignalR dla zdarzeń i Zod na granicy odpowiedzi API.
- Tekst UI umieszczaj w `shared/i18n`; identyfikatory i nazwy komponentów pozostają angielskie.
- Rozdzielaj funkcje na `features`, by chat, browser view, timeline, approvals, takeover, skill studio i admin nie współdzieliły niejawnego stanu.
- Tokenów dostępowych nie zapisuj w `localStorage`. Preferuj BFF i bezpieczne cookies HTTP-only; token krótkotrwały do streamu wiąż z sesją.
- UI nie otrzymuje CDP ani Playwright WebSocket. Sterowanie przechodzi przez uwierzytelniony kontrakt control plane.
- Spełniaj WCAG 2.2 AA: semantyczny HTML, etykiety, klawiatura, widoczny focus, kontrast i komunikaty live region.
- Approval i takeover muszą być zrozumiałe bez koloru. Nigdy nie maskuj środowiska produkcyjnego.
- Test jednostkowy obejmuje zachowanie komponentu, a Playwright E2E krytyczny przepływ użytkownika. Nie polegaj wyłącznie na snapshotach.

## 8. Zasady skills

- Skill jest wersjonowaną definicją danych, nie monolitycznym promptem i nie skryptem wykonywalnym.
- Każdy skill zawiera: `id`, SemVer, site/environment scope, inputs, permissions, risk, preconditions, kroki, asercje, approval points, postconditions i recovery.
- Tryby to `Observe`, `Assist`, `Execute`; zmiana trybu nie może ominąć policy.
- Parametry wrażliwe oznaczaj jawnie i nie zapisuj ich w nagraniach. Dane testowe anonimizuj przed commitem.
- Cel kroku jest biznesowy i typowany. Nie zapisuj surowego JavaScriptu, XPath ani współrzędnych jako normalnej ścieżki.
- Locator może mieć wiele nazw językowych, ale potrzebuje stabilnej semantyki i testu na obsługiwanych wersjach IFS.
- Recorder generuje szkic. Ekspert musi nazwać parametry, usunąć dane, ustawić ryzyko, dodać asercje i zatwierdzić publikację.
- Publikacja skill wymaga walidacji schematu, testów replay, podpisu/pochodzenia, changelogu i możliwości rollbacku.

## 9. Zależności, licencje i supply chain

### Automatycznie dopuszczalne po sprawdzeniu konkretnej wersji

- MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, Zlib i PostgreSQL License.

### Wymagające jawnego przeglądu prawnego i architektonicznego

- MPL-2.0, LGPL, GPL i AGPL, nawet gdy komponent działa jako niezmodyfikowany sidecar.

### Domyślnie zabronione

- SSPL, RSAL, BSL/BUSL, Elastic License, FSL, Commons Clause, PolyForm, „non-commercial”, „evaluation-only”, brak licencji lub zależność wymagająca płatnej licencji dla tego zastosowania.

Przed dodaniem pakietu, obrazu lub modelu:

1. Sprawdź oficjalne repozytorium, licencję konkretnej wersji i licencje zależności tranzytywnych.
2. Potwierdź bezpłatne użycie komercyjne, dystrybucję obrazu i model hostingu/SaaS.
3. Sprawdź CVE, aktywność utrzymania, podpis/provenance i wspierane platformy.
4. Dodaj wersję centralnie i lockfile. Obrazy produkcyjne przypnij do digestu; nie używaj `latest`.
5. Zaktualizuj rejestr licencji/SBOM i dokument infrastruktury, jeśli zmienia topologię.
6. Uruchom restore/install, audyt i testy.

Nie wprowadzaj bez osobnej ponownej oceny nowszych linii FluentAssertions, AutoMapper, MediatR ani MassTransit — ich modele licencyjne zmieniały się. Preferuj xUnit `Assert`, jawne mapowanie i jawne handlery. Redis, MinIO, HashiCorp Vault i Elastic nie należą do domyślnego stosu; używaj odpowiednio Valkey, SeaweedFS, OpenBao i OpenSearch zgodnie z dokumentacją infrastruktury.

Licencja runtime LLM nie obejmuje wag. Każdy model ma wpis: źródło, revision, SHA-256, licencja wag, commercial use, acceptable use, pochodzenie kwantyzacji i właściciel akceptacji. Płatny cloud provider nie może być wymagany do podstawowego działania.

## 10. Testy i bramki jakości

Dobierz najwęższy zestaw, który dowodzi zmiany, a przed zmianą przekrojową uruchom pełną bramkę.

```bash
dotnet restore Praxiara.slnx
dotnet build Praxiara.slnx --no-restore
dotnet test Praxiara.slnx --no-build --logger "console;verbosity=minimal"

pnpm install --frozen-lockfile
pnpm web:lint
pnpm web:test
pnpm web:build
```

Zakres projektów testowych:

- `UnitTests`: niezmienniki domenowe, parsery, policy i czyste use cases;
- `ArchitectureTests`: dozwolony DAG i brak niedozwolonych pakietów;
- `ContractTests`: JSON/OpenAPI/messages, zgodność wersji i error codes;
- `IntegrationTests`: API + realne kontenery zależności przez izolowane dane;
- `Browser.Tests`: Playwright, ARIA, stale refs, popupy, iframe, upload/download i tracing;
- `SecurityTests`: prompt injection, egress, approval hash, auth fail-closed, redaction i tenant isolation;
- `TestSites`: deterministyczne fixtures, bez zależności od Internetu;
- frontend Vitest: komponenty i walidacja kontraktów;
- frontend Playwright: scenariusze E2E, takeover i approval;
- evals: sukces biznesowy, liczba kroków, false approval, injection resistance i regresje modelu.

Nie markuj testu jako skipped bez przyczyny i właściciela naprawy. Flaky test jest defektem. Test skutkowej operacji używa środowiska testowego, idempotency key i cleanup; nigdy produkcji.

## 11. Workflow zmiany

1. Ustal scenariusz i kryteria akceptacji.
2. Zidentyfikuj zagrożenia oraz poziom ryzyka R0–R5.
3. Zmień kontrakt i testy konsumenta/producenta.
4. Zaimplementuj najmniejszą kompletną zmianę w poprawnym module.
5. Dodaj telemetrię bez danych wrażliwych oraz wymagany audyt biznesowy.
6. Uzupełnij polski Markdown, jeśli zmienia się zachowanie operatora, wdrożenie lub decyzja.
7. Uruchom formatowanie, build, testy, audit i compose config odpowiednie dla zmiany.
8. Przejrzyj diff pod kątem sekretów, niezamierzonych zmian, języka i licencji.
9. W raporcie podaj wynik, pliki, testy i rzeczywiste ograniczenia. Nie twierdź, że coś działa produkcyjnie bez dowodu.

## 12. Definition of Done

Zmiana jest ukończona, gdy łącznie:

- spełnia jawne kryteria akceptacji i nie osłabia żadnego niezmiennika bezpieczeństwa;
- znajduje się w prawidłowym module i zachowuje DAG;
- ma testy na happy path, błąd oraz istotną granicę bezpieczeństwa;
- operacja skutkowa ma policy, approval, idempotencję, weryfikację i audyt;
- logi/telemetria nie ujawniają danych, a nowe dane mają retencję i właściciela;
- zależności przeszły bramkę licencyjną, są przypięte i ujęte w lockfile/SBOM;
- właściwe komendy build/test/lint zakończyły się powodzeniem;
- dokumenty Markdown są po polsku, a kod i dokumentacja kodowa po angielsku;
- nie pozostawiono sekretów, danych klienta, niejawnego `TODO`, martwego template code ani pominiętych testów;
- znane ograniczenie jest udokumentowane wraz z konsekwencją i następnym krokiem.

## 13. Czynności zabronione bez odrębnej zgody

- wykonanie testu lub automatyzacji na produkcyjnym IFS;
- trwałe zapisanie sesji uwierzytelnionej;
- wystawienie VNC, CDP, Playwright WebSocket, bazy, cache, secrets lub UI operatorskiego do publicznej sieci;
- dodanie płatnej lub ograniczonej komercyjnie zależności;
- usunięcie audytu, approval, weryfikacji, limitu albo izolacji „na czas testu”;
- włączenie dowolnego egress, dowolnego JavaScriptu/shella lub przekazanie poświadczeń modelowi;
- automatyczne wykonywanie migracji destrukcyjnej, DAST przeciw produkcji lub replay skutkowych akcji;
- deklarowanie zgodności prawnej, bezpieczeństwa albo wysokiej dostępności bez osobnej, udokumentowanej weryfikacji.

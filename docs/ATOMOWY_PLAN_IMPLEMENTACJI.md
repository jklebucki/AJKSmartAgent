# Atomowy plan implementacji platformy Praxiara

## 1. Cel i relacja do planu strategicznego

Ten dokument jest niezależnym, wykonawczym wykazem prac potrzebnych do zbudowania kompletnej aplikacji opisanej w `docs/PLAN_WYTWORZENIA.md`. Plan strategiczny pozostaje źródłem wymagań, ryzyk, faz i kryteriów GA. Ten wykaz rozbija je na małe, weryfikowalne kroki przeznaczone do realizacji przez człowieka i Codex.

Lista nie jest harmonogramem. Kolejność wynika z zależności i bram bezpieczeństwa. Terminy, ownerzy, capacity i priorytety biznesowe należy prowadzić w narzędziu backlogowym.

## 2. Reguła atomowości

Krok jest atomowy, gdy łącznie:

- daje jeden nazwany rezultat i ma jednego ownera technicznego;
- mieści się domyślnie w jednym małym PR i nie miesza implementacji, refaktoringu ani aktualizacji zależności;
- ma jawne zależności, klasę ryzyka i mierzalny dowód ukończenia;
- może zostać niezależnie przejrzany, cofnięty i przetestowany;
- nie ukrywa decyzji architektonicznej, prawnej, biznesowej ani bezpieczeństwa;
- nie jest zamykany, dopóki wskazany test, dokument albo decyzja rzeczywiście nie istnieje.

Jeżeli krok przekracza około dwóch dni skupionej pracy, dotyka więcej niż jednego kontraktu wire albo wymaga dwóch niezależnych decyzji, należy przed rozpoczęciem rozbić go na mniejsze pozycje. Samo utworzenie klasy, endpointu lub migracji nie jest ukończeniem, jeżeli rezultat wymaga testu konsumenta, autoryzacji, audytu albo recovery.

## 3. Statusy i zależności

- `[ ]` — krok niewykonany;
- `[x]` — krok wykonany i potwierdzony dowodem w repozytorium;
- `BLOKADA` — decyzja lub dostęp spoza repozytorium, którego Codex nie może założyć;
- `Zależy od: —` — krok może rozpocząć się bez innego kroku z tej listy;
- zależność od bramy oznacza, że wszystkie kroki tej bramy muszą być zamknięte.

Identyfikatory są trwałe. Nie należy zmieniać ich znaczenia po użyciu w commicie, PR, ADR lub raporcie. Usunięty krok pozostaje w historii jako anulowany z uzasadnieniem.

## 4. Sterowanie Codex i dobór skills

### 4.1. Polecenia sterujące wpisywane przez użytkownika

Dostępność poleceń zależy od środowiska i uprawnień. W task composerze należy używać najmniejszego polecenia odpowiadającego intencji:

| Polecenie | Kiedy używać w tym planie |
|---|---|
| `$<nazwa-skill>` | Jawnie wybierz skill wskazany przez profil kroku, np. `$dotnet10-implement-change`. |
| `/plan` | Przed krokiem wieloetapowym, zmianą kontraktu albo decyzją wymagającą oceny wariantów. |
| `/goal` | Tylko na jawne życzenie użytkownika dla trwałego celu obejmującego wiele kroków; najpierw doprecyzuj cel przez `/plan`. |
| `/review` | Po implementacji poproś o osobny code review niezatwierdzonych zmian albo porównanie z gałęzią bazową. |
| `/status` | Sprawdź task ID, zużycie kontekstu i limity przed długą bramą lub kontynuacją. |
| `/compact` | Skompaktuj kontekst długiego tasku po zapisaniu bieżącego stanu i dowodów. |
| `/side` | Zadaj krótkie pytanie pomocnicze bez przerywania głównego tasku. |
| `/fork` | Rozdziel eksperymentalny wariant do nowego tasku/worktree zamiast mieszać dwa rozwiązania w jednym diffie. |
| `/mcp` | Sprawdź stan wymaganych serwerów MCP przed krokiem zależnym od zewnętrznego źródła. |

Nie należy używać `/init`, gdy repozytorium ma już obowiązujący `AGENTS.md`, ani `/goal` dla pojedynczego atomowego kroku. Oficjalna lista i semantyka poleceń: [OpenAI — Slash commands](https://learn.chatgpt.com/docs/reference/slash-commands).

### 4.2. Obowiązkowy protokół każdej sesji wykonawczej

1. W poleceniu jawnie wskaż identyfikator kroku, zakres i profil, np. `Zrealizuj TASK-007 profilem DB`.
2. Codex czyta `AGENTS.md`, ten krok, dokument obszaru i kompletne `SKILL.md` wybranych skills.
3. Codex uruchamia `git status --short` i chroni niezwiązane zmiany użytkownika.
4. Dla zmiany większej niż jeden plik Codex używa `update_plan` z jednym krokiem `in_progress`.
5. Codex klasyfikuje ryzyko R0–R5, granice zaufania, dane, audyt, idempotencję i recovery.
6. Codex zmienia pliki przez `apply_patch`; polecenia powłoki służą inspekcji, formatowaniu i walidacji.
7. Codex najpierw uruchamia najwęższy test, następnie bramę wymaganą przez profil.
8. Codex wykonuje `git diff --check`, przegląda diff i raportuje wynik, pliki, testy oraz realne ograniczenia.
9. Commit, push, PR, migracja środowiska albo operacja zewnętrzna powstaje wyłącznie na jawne polecenie użytkownika.

`update_plan`, `apply_patch` i `exec_command` są wewnętrznymi narzędziami wykonawczymi Codex, a nie poleceniami wpisywanymi przez użytkownika. Nie należy używać `create_goal`, tworzyć nowego tasku Codex ani delegować subagentom, jeśli użytkownik nie poprosił o to jawnie. Nie należy używać skill niezgodnego z rodzajem pracy tylko po to, aby „jakiś skill” był aktywny.

### 4.3. Profile wykonawcze

| Profil | Jawna instrukcja dla Codex | Zastosowanie |
|---|---|---|
| `IMP` | `Użyj $dotnet10-implement-change.` | Zmiana zachowania .NET bez dodatkowego obszaru Praxiary |
| `API` | `Użyj $dotnet10-implement-change i $praxiara-evolve-api-contract.` | REST, SignalR, DTO, OpenAPI, auth wire |
| `DB` | `Użyj $dotnet10-implement-change i $praxiara-evolve-persistence.` | EF Core, PostgreSQL, migracje, outbox, transakcje |
| `BRW` | `Użyj $dotnet10-implement-change i $praxiara-change-browser-agent.` | Browser Worker, Playwright, obserwacje, takeover |
| `IFS` | `Użyj $dotnet10-implement-change i $praxiara-integrate-ifs.` | Projection/OData, route API/UI/Hybrid, Aurena |
| `SKL` | `Użyj $dotnet10-implement-change i $praxiara-author-business-skill.` | Schema, parser, runtime, recorder i replay skills Praxiary |
| `TST` | `Użyj $dotnet10-write-tests` oraz najwyżej jednego właściwego skill Praxiary. | Samodzielny krok testowy lub regresja |
| `DEP` | `Użyj $dotnet10-manage-dependencies.` | Pakiet, SDK, obraz bazowy, lockfile, licencja, CVE |
| `REF` | `Użyj $dotnet10-refactor-safely` oraz najwyżej jednego właściwego skill Praxiary. | Refaktoring bez zmiany zachowania |
| `DIA` | `Użyj $dotnet10-diagnose-failure.` | Diagnoza bez implementowania poprawki |
| `REV` | `Użyj $dotnet10-review-code.` | Jawny przegląd bez edycji |
| `SEC` | `Użyj $dotnet10-review-code i $praxiara-review-security.` | Jawny audyt bezpieczeństwa bez edycji |
| `WEB` | Brak dedykowanego repozytoryjnego skill do React; stosuj sekcję 7 `AGENTS.md`, a do testu lokalnego użyj `$browser:control-in-app-browser`. | React, i18n, a11y, ręczna walidacja UI |
| `DOC` | Nie aktywuj skill implementacyjnego; użyj `update_plan`, `apply_patch` i walidacji odnośników. | Polski Markdown, ADR, runbook, rejestr decyzji |
| `OPS` | Dobierz `DEP`, `DIA`, `SEC` albo `DOC` według faktycznej intencji; nie łącz ich automatycznie. | CI/CD, obrazy, wdrożenia, SRE |

Jeśli `praxiara-review-security` nie jest widoczny w katalogu bieżącej sesji Codex, najpierw napraw wykrywanie repozytoryjnych skills i uruchom testy `Praxiara.AgentSkills.Tests`; nie zastępuj audytu ogólnym promptem.

### 4.4. Bramy poleceń

Najwęższa brama .NET:

```bash
dotnet format Praxiara.slnx --no-restore --verify-no-changes
dotnet build <projekt.csproj> --no-restore
dotnet test <projekt-testowy.csproj> --no-build --logger "console;verbosity=minimal"
```

Pełna brama backendu:

```bash
dotnet restore Praxiara.slnx
dotnet format Praxiara.slnx --no-restore --verify-no-changes
dotnet build Praxiara.slnx --no-restore
dotnet test Praxiara.slnx --no-build --logger "console;verbosity=minimal"
```

Brama frontendowa:

```bash
pnpm install --frozen-lockfile
pnpm web:lint
pnpm web:test
pnpm web:build
```

Każdy krok kończy dodatkowo `git diff --check`. Kroki zależności uruchamiają audyt pakietów, kontrolę licencji, lockfile i SBOM. Kroki deploymentu uruchamiają walidację właściwego profilu Compose. R4/R5 nigdy nie są testowane na produkcji.

## 5. Atomowy backlog wykonawczy

### 5.1. DISC — discovery, decyzje i dowody wykonalności

Próba wykonania `DISC-001`–`DISC-010` z 2026-07-12 jest opisana w [raporcie discovery](discovery/RAPORT-DISC-001-010.md). Kolejna iteracja `DISC-011`–`DISC-018` oraz `GOV-001`–`GOV-002` jest opisana w [raporcie discovery](discovery/RAPORT-DISC-011-020.md). Projekty ADR dla `GOV-003`–`GOV-012` opisuje [raport governance](discovery/RAPORT-GOV-003-012.md). Kroki wymagające imiennego ownera, działającego kontenera, zarządzanej stacji Windows lub prawdziwego sandboxu IFS pozostają otwarte; przygotowane artefakty nie zastępują tych dowodów.

- [ ] `DISC-001` `[DOC]` Wyznacz ownerów produktu, architektury, bezpieczeństwa, danych, IFS i SRE. Zależy od: —. Dowód: zatwierdzona tabela ownerów i zastępstw.
- [ ] `DISC-002` `[DOC]` Zdefiniuj jeden proces referencyjny R0 na syntetycznych danych IFS. Zależy od: DISC-001. Dowód: scenariusz, wejście, wynik i verifier.
- [ ] `DISC-003` `[DOC]` Zdefiniuj jeden proces referencyjny R2 na syntetycznych danych IFS. Zależy od: DISC-001. Dowód: scenariusz bez zapisu i kryteria błędu.
- [ ] `DISC-004` `[DOC]` Zdefiniuj jeden proces referencyjny R4 na syntetycznych danych IFS. Zależy od: DISC-001. Dowód: rekord, before/after, approval i verifier.
- [ ] `DISC-005` `[BRW]` Uruchom izolowany Chromium jako non-root w środowisku spike. Zależy od: —. Dowód: reprodukowalne polecenie i test procesu.
- [ ] `DISC-006` `[BRW]` Zmierz CPU, RAM i czas startu jednej sesji Chromium. Zależy od: DISC-005. Dowód: raport z wersją obrazu i sprzętu.
- [ ] `DISC-007` `[BRW]` Zbuduj spike semantycznej obserwacji reprezentatywnego gridu IFS. Zależy od: DISC-005, DISC-002. Dowód: zanonimizowany snapshot i ograniczenia.
- [ ] `DISC-008` `[BRW]` Porównaj noVNC i WebRTC dla live view oraz takeover. Zależy od: DISC-005. Dowód: latency, bezpieczeństwo, licencja i rekomendacja.
- [ ] `DISC-009` `[DOC]` Zatwierdź ADR wyboru noVNC/WebRTC. Zależy od: DISC-008. Dowód: zaakceptowany ADR z konsekwencjami.
- [ ] `DISC-010` `[IFS]` Zinwentaryzuj Projection/OData dla procesów DISC-002–004. Zależy od: DISC-002, DISC-003, DISC-004. Dowód: route matrix API/UI/Hybrid.
- [ ] `DISC-011` `[IFS]` Sprawdź zachowanie Aurena Agent na zarządzanej stacji Windows. Zależy od: DISC-004. Dowód: raport funkcji niemożliwych w Chromium.
- [ ] `DISC-012` `[DOC]` Zdecyduj, czy Windows companion należy do zakresu pierwszego GA. Zależy od: DISC-011. Dowód: decyzja ownerów z ryzykiem i terminem.
- [x] `DISC-013` `[IMP]` Zbuduj spike structured output dla lokalnego modelu na przypiętej rewizji. Zależy od: —. Dowód: `tools/Praxiara.ModelSpike` i `MODEL-STRUCTURED-OUTPUT-SPIKE.md`; wykonano 2026-07-12.
- [x] `DISC-014` `[TST]` Zmierz poprawność pojedynczych tool calls lokalnego modelu. Zależy od: DISC-013. Dowód: `gpt-oss-20b-tool-eval.json`, 10/10 przypadków; wykonano 2026-07-12.
- [ ] `DISC-015` `[SEC]` Przeprowadź spike prompt injection i aplikacyjnego egress deny. Zależy od: DISC-005, DISC-013. Dowód: findingi i reprodukowalne testy negatywne.
- [ ] `DISC-016` `[DEP]` Zweryfikuj licencje konkretnego stosu spike, obrazów i modelu. Zależy od: DISC-005, DISC-008, DISC-013. Dowód: rejestr wersji, licencji i hashy.
- [ ] `DISC-017` `[DOC]` Utwórz capacity model sesji Chromium i inferencji. Zależy od: DISC-006, DISC-014. Dowód: założenia, limity i margines bezpieczeństwa.
- [ ] `DISC-018` `[DOC]` Zamknij bramę Fazy 0. Zależy od: DISC-001–017. Dowód: podpisany raport bez nierozwiązanego blockeru krytycznego.

### 5.2. GOV — architektura, governance i supply chain

- [x] `GOV-001` `[TST]` Egzekwuj dozwolone zależności wszystkich projektów `src`. Zależy od: —. Dowód: `DependencyRulesTests`; wykonano 2026-07-12 jako E01.1.
- [ ] `GOV-002` `[DOC]` Utwórz ADR modularnego control plane i osobnego Browser Workera. Zależy od: DISC-018. Dowód: ADR-001.
- [ ] `GOV-003` `[DOC]` Utwórz ADR Playwright jako głównego protokołu. Zależy od: DISC-007. Dowód: ADR-002.
- [ ] `GOV-004` `[DOC]` Utwórz ADR LLM jako proposer, nie executor. Zależy od: DISC-014, DISC-015. Dowód: ADR-003.
- [ ] `GOV-005` `[DOC]` Utwórz ADR trwałej maszyny stanów i checkpointów. Zależy od: DISC-018. Dowód: ADR-004.
- [ ] `GOV-006` `[DOC]` Utwórz ADR izolacji i capacity sesji browser. Zależy od: DISC-006, DISC-017. Dowód: ADR-005.
- [ ] `GOV-007` `[DOC]` Utwórz ADR revision-bound element references. Zależy od: DISC-007. Dowód: ADR-006.
- [ ] `GOV-008` `[DOC]` Utwórz ADR typed tool registry i zakazu arbitrary code. Zależy od: DISC-015. Dowód: ADR-007.
- [ ] `GOV-009` `[DOC]` Utwórz ADR niezależnego Policy Engine fail-closed. Zależy od: DISC-015. Dowód: ADR-008.
- [ ] `GOV-010` `[DOC]` Utwórz ADR approval hash i rewalidacji TOCTOU. Zależy od: DISC-004. Dowód: ADR-009.
- [ ] `GOV-011` `[DOC]` Utwórz ADR immutable skills i publikacji. Zależy od: DISC-002–004. Dowód: ADR-010.
- [ ] `GOV-012` `[DOC]` Utwórz ADR routingu IFS API-first. Zależy od: DISC-010. Dowód: ADR-011.
- [ ] `GOV-013` `[DOC]` Rozstrzygnij delegację tożsamości do IFS API. Zależy od: DISC-010. Dowód: ADR-012 zatwierdzony przez security.
- [ ] `GOV-014` `[DOC]` Udokumentuj decyzję Windows companion. Zależy od: DISC-012. Dowód: ADR-013.
- [ ] `GOV-015` `[DOC]` Utwórz ADR live view i takeover. Zależy od: DISC-009. Dowód: ADR-014.
- [ ] `GOV-016` `[DOC]` Utwórz ADR PostgreSQL, outbox, broker i gwarancji dostarczenia. Zależy od: GOV-005. Dowód: ADR-015.
- [ ] `GOV-017` `[DEP]` Zweryfikuj centralne wersje wszystkich pakietów NuGet. Zależy od: DISC-016. Dowód: wersje, źródła, licencje i brak duplikacji.
- [ ] `GOV-018` `[DEP]` Włącz odtwarzalne lockfiles NuGet dla rozwiązania. Zależy od: GOV-017. Dowód: restore locked mode w CI.
- [ ] `GOV-019` `[DEP]` Zweryfikuj lockfile pnpm i licencje frontendowe. Zależy od: DISC-016. Dowód: frozen install i raport licencji.
- [ ] `GOV-020` `[OPS]` Dodaj generowanie SBOM dla aplikacji i obrazów. Zależy od: GOV-017, GOV-019. Dowód: artefakt SBOM przypisany do digestu.
- [ ] `GOV-021` `[OPS]` Dodaj secret scan repozytorium i obrazów. Zależy od: —. Dowód: blokująca brama CI z fixture pozytywnym.
- [ ] `GOV-022` `[OPS]` Dodaj skan CVE pakietów i obrazów z polityką wyjątków. Zależy od: GOV-017. Dowód: blokująca brama i rejestr risk acceptance.
- [ ] `GOV-023` `[OPS]` Dodaj podpisywanie artefaktów OCI i provenance. Zależy od: GOV-020. Dowód: weryfikowalny podpis bez rebuild.
- [ ] `GOV-024` `[TST]` Dodaj test zakazanych pakietów i licencji. Zależy od: GOV-017. Dowód: negatywny fixture powoduje fail.
- [ ] `GOV-025` `[DOC]` Zamknij bramę governance foundation. Zależy od: GOV-001–024. Dowód: komplet ADR, SBOM i działające bramy.

### 5.3. ID — tożsamość, tenanty i autoryzacja

- [ ] `ID-001` `[IMP]` Dodaj niezmienny `TenantId` jako typ domenowy. Zależy od: GOV-025. Dowód: test poprawnej i pustej wartości.
- [ ] `ID-002` `[IMP]` Dodaj niezmienny `UserId` jako typ domenowy. Zależy od: GOV-025. Dowód: test normalizacji i odrzucenia pustej wartości.
- [ ] `ID-003` `[DB]` Dodaj encje persystencji tenantów bez sekretów. Zależy od: ID-001. Dowód: mapping i test ograniczeń.
- [ ] `ID-004` `[DB]` Dodaj migrację tabel tenantów. Zależy od: ID-003. Dowód: migracja up/down na pustej bazie testowej.
- [ ] `ID-005` `[API]` Skonfiguruj OIDC/OAuth 2.1 z PKCE dla API. Zależy od: GOV-025. Dowód: auth fail-closed bez tokenu.
- [ ] `ID-006` `[API]` Waliduj issuer i audience tokenu. Zależy od: ID-005. Dowód: testy złego issuer i audience.
- [ ] `ID-007` `[API]` Waliduj podpis, `nbf` i `exp` tokenu. Zależy od: ID-005. Dowód: testy tokenu zmienionego, przyszłego i wygasłego.
- [ ] `ID-008` `[API]` Mapuj claim użytkownika i tenantu do typowanego kontekstu requestu. Zależy od: ID-001, ID-002, ID-005. Dowód: test brakującego i sprzecznego claimu.
- [ ] `ID-009` `[IMP]` Zdefiniuj role aplikacyjne i tenantowe jako rozłączne uprawnienia. Zależy od: ID-008. Dowód: macierz testów role–permission.
- [ ] `ID-010` `[API]` Dodaj resource authorization uwzględniającą tenant i rekord. Zależy od: ID-009. Dowód: negatywny test cross-tenant.
- [ ] `ID-011` `[DB]` Wymuś tenant scope w zapisach i odczytach EF. Zależy od: ID-003, ID-010. Dowód: integracyjny test izolacji dwóch tenantów.
- [ ] `ID-012` `[TST]` Dodaj komplet negatywnych testów tenant isolation. Zależy od: ID-011. Dowód: read, update, delete i enumeracja odrzucone cross-tenant.
- [ ] `ID-013` `[IMP]` Zdefiniuj delegacje approvera per proces i środowisko. Zależy od: ID-009. Dowód: test zakresu oraz wygaśnięcia delegacji.
- [ ] `ID-014` `[IMP]` Zaimplementuj domyślny zakaz self-approval dla R5. Zależy od: ID-013. Dowód: test deny i jawnego bardziej restrykcyjnego wariantu.
- [ ] `ID-015` `[API]` Dodaj kontrakt step-up authentication dla R5. Zależy od: ID-005, ID-013. Dowód: wersjonowany DTO i contract test.
- [ ] `ID-016` `[IMP]` Zaimplementuj czasowe uprawnienie break-glass z uzasadnieniem. Zależy od: ID-009. Dowód: test TTL i braku stałej roli.
- [ ] `ID-017` `[IMP]` Audytuj aktywację i użycie break-glass. Zależy od: ID-016, AUD-006. Dowód: komplet zdarzeń bez sekretów.
- [ ] `ID-018` `[IMP]` Zdefiniuj service identity z minimalnym scope. Zależy od: ID-009. Dowód: test odrzucenia interaktywnego użycia.
- [ ] `ID-019` `[OPS]` Skonfiguruj krótkotrwałe credentials albo mTLS usług. Zależy od: ID-018. Dowód: rotacja bez restartu całej platformy.
- [ ] `ID-020` `[IMP]` Zatrzymuj wykonanie po revoke roli lub sesji przed następną akcją. Zależy od: ID-010, TASK-019. Dowód: test revoke między krokami.
- [ ] `ID-021` `[SEC]` Przejrzyj auth, tenant isolation i break-glass. Zależy od: ID-001–020. Dowód: brak otwartego high/critical bez akceptacji.

### 5.4. TASK — domena zadań, checkpointy i persystencja

- [ ] `TASK-001` `[IMP]` Rozszerz `AgentTask` o niezmienny tenant i correlation ID. Zależy od: ID-001, ID-002. Dowód: test tworzenia i niezmienności.
- [ ] `TASK-002` `[IMP]` Dodaj `Queued` do maszyny stanów zadania. Zależy od: TASK-001. Dowód: dozwolone i zabronione przejścia.
- [ ] `TASK-003` `[IMP]` Dodaj `Paused` do maszyny stanów zadania. Zależy od: TASK-002. Dowód: test pauzy wyłącznie ze stanów dozwolonych.
- [ ] `TASK-004` `[IMP]` Dodaj `Recovering` do maszyny stanów zadania. Zależy od: TASK-002. Dowód: test wejścia po błędzie weryfikacji.
- [ ] `TASK-005` `[IMP]` Dodaj `OutcomeUnknown` bez automatycznego retry. Zależy od: TASK-004. Dowód: test braku przejścia bez reconciliation.
- [ ] `TASK-006` `[IMP]` Wymuś terminalność `Completed`, `Failed` i `Cancelled`. Zależy od: TASK-002–005. Dowód: test każdego stanu terminalnego.
- [ ] `TASK-007` `[IMP]` Wymuś pozytywną weryfikację przed `Completed`. Zależy od: TASK-006. Dowód: test odrzucenia samego sukcesu narzędzia.
- [ ] `TASK-008` `[IMP]` Dodaj niezmienne rewizje celu zadania. Zależy od: TASK-001. Dowód: aktywna rewizja i zachowana historia.
- [ ] `TASK-009` `[IMP]` Dodaj model `TaskRun` z przypiętymi wersjami modelu, skill i policy. Zależy od: TASK-001. Dowód: test niezmienności wersji runu.
- [ ] `TASK-010` `[IMP]` Dodaj model atomowego `AgentStep` z pojedynczą propozycją. Zależy od: TASK-009. Dowód: test odrzucenia dwóch tool calls.
- [ ] `TASK-011` `[IMP]` Dodaj typowane limity kroków, replanów, retry i czasu. Zależy od: TASK-009. Dowód: walidacja zerowych i nadmiernych limitów.
- [ ] `TASK-012` `[IMP]` Dodaj `CommandId`, sequence i optimistic concurrency token. Zależy od: TASK-010. Dowód: test duplikatu i konfliktu wersji.
- [ ] `TASK-013` `[DB]` Zmapuj agregat task oraz goal revisions do EF. Zależy od: TASK-008, ID-011. Dowód: mapping bez publicznych setterów domeny.
- [ ] `TASK-014` `[DB]` Zmapuj runy, kroki i checkpointy do EF. Zależy od: TASK-009–012. Dowód: round-trip integracyjny.
- [ ] `TASK-015` `[DB]` Dodaj migrację modelu zadań z indeksami tenant/status/created. Zależy od: TASK-013, TASK-014. Dowód: przejrzany SQL i plan rollback.
- [ ] `TASK-016` `[DB]` Zapisuj task state i outbox atomowo. Zależy od: TASK-015, MSG-003. Dowód: test rollbacku przy awarii outbox.
- [ ] `TASK-017` `[DB]` Zaimplementuj idempotentną obsługę `CommandId`. Zależy od: TASK-012, TASK-016. Dowód: dwukrotna komenda daje jeden skutek.
- [ ] `TASK-018` `[IMP]` Zaimplementuj pause, resume i cancel jako jawne use cases. Zależy od: TASK-003, TASK-016. Dowód: test autoryzacji i stanów konfliktowych.
- [ ] `TASK-019` `[IMP]` Egzekwuj timeout i cancellation między atomowymi akcjami. Zależy od: TASK-011, TASK-018. Dowód: test bez przerwania nieodwracalnego commitu.
- [ ] `TASK-020` `[IMP]` Zapisuj checkpoint po bezpiecznym kroku. Zależy od: TASK-014, TASK-016. Dowód: restart wznawia od checkpointu.
- [ ] `TASK-021` `[IMP]` Zablokuj wznowienie niepotwierdzonej akcji non-idempotent. Zależy od: TASK-005, TASK-020. Dowód: test crash po wysłaniu skutku.
- [ ] `TASK-022` `[IMP]` Wykrywaj powtórzone obserwacje bez postępu. Zależy od: TASK-010, TASK-011. Dowód: deterministyczny loop limit.
- [ ] `TASK-023` `[IMP]` Wymuś osobną browser session dla równoległych tasków bez polityki współdzielenia. Zależy od: BRW-014. Dowód: test dwóch tasków użytkownika.
- [ ] `TASK-024` `[IMP]` Generuj podsumowanie wyłącznie ze zweryfikowanych faktów. Zależy od: TASK-007, ORCH-019. Dowód: test wykluczenia niezweryfikowanej propozycji.
- [ ] `TASK-025` `[TST]` Dodaj testy mutacyjne lub równoważne dla state machine. Zależy od: TASK-002–007. Dowód: mutacje krytycznych przejść wykrywane.

### 5.5. MSG — transport, leases i durable execution

- [ ] `MSG-001` `[DEP]` Zweryfikuj wersję, licencję i CVE brokera/workflow runtime. Zależy od: GOV-016. Dowód: wpis rejestru licencji.
- [ ] `MSG-002` `[IMP]` Zdefiniuj wersjonowany envelope komendy i zdarzenia. Zależy od: TASK-012. Dowód: serializacja N/N-1.
- [ ] `MSG-003` `[DB]` Dodaj encję i mapping outbox. Zależy od: MSG-002. Dowód: indeks status/next-attempt i test mappingu.
- [ ] `MSG-004` `[DB]` Dodaj migrację outbox. Zależy od: MSG-003. Dowód: migracja na realnym PostgreSQL testowym.
- [ ] `MSG-005` `[IMP]` Zaimplementuj publisher outbox z bounded batch. Zależy od: MSG-004. Dowód: cancellation i brak utraty rekordów.
- [ ] `MSG-006` `[IMP]` Zaimplementuj idempotentnego consumera. Zależy od: MSG-002. Dowód: duplicate delivery bez podwójnego skutku.
- [ ] `MSG-007` `[DB]` Dodaj lease tasku z fencing token. Zależy od: TASK-015. Dowód: dwóch workerów nie wykonuje tej samej generacji.
- [ ] `MSG-008` `[IMP]` Dodaj renew i expiry lease. Zależy od: MSG-007. Dowód: utracony worker przestaje wykonywać akcje.
- [ ] `MSG-009` `[IMP]` Dodaj bounded retry tylko dla błędów przejściowych. Zależy od: MSG-005, MSG-006. Dowód: klasyfikacja, backoff, jitter i limit.
- [ ] `MSG-010` `[IMP]` Dodaj dead-letter/reconciliation bez automatycznego skutku. Zależy od: MSG-009. Dowód: jawny status i alert.
- [ ] `MSG-011` `[TST]` Przetestuj crash między commit DB a publikacją. Zależy od: TASK-016, MSG-005. Dowód: zdarzenie ostatecznie opublikowane raz logicznie.
- [ ] `MSG-012` `[TST]` Przetestuj przejęcie lease po awarii workera. Zależy od: MSG-008. Dowód: fencing odrzuca starą generację.

### 5.6. AUD — audyt, artefakty, dane i retencja

- [ ] `AUD-001` `[IMP]` Zdefiniuj katalog wersjonowanych typów zdarzeń audytu. Zależy od: GOV-025. Dowód: event matrix dla task/session/policy/action/result.
- [ ] `AUD-002` `[IMP]` Zdefiniuj klasyfikację danych i stabilne etykiety. Zależy od: DISC-001. Dowód: enum/policy i dokument właściciela danych.
- [ ] `AUD-003` `[IMP]` Zaimplementuj redakcję sekretów, auth headers i cookies. Zależy od: AUD-002. Dowód: testy reprezentatywnych formatów.
- [ ] `AUD-004` `[IMP]` Zaimplementuj redakcję pól sensitive według schema. Zależy od: AUD-002. Dowód: test nested JSON i tablic.
- [ ] `AUD-005` `[IMP]` Zbuduj kanoniczną serializację envelope audytu. Zależy od: AUD-001, AUD-003, AUD-004. Dowód: stabilne bajty dla równoważnych danych.
- [ ] `AUD-006` `[IMP]` Rozszerz hash chain o sequence i previous hash per strumień. Zależy od: AUD-005. Dowód: wykrycie zmiany, usunięcia i reorder.
- [ ] `AUD-007` `[DB]` Zmapuj append-only audit events. Zależy od: AUD-006, ID-011. Dowód: brak ścieżki update/delete w aplikacji.
- [ ] `AUD-008` `[DB]` Dodaj migrację audytu z indeksami tenant/task/time. Zależy od: AUD-007. Dowód: plan retencji i test zapytania.
- [ ] `AUD-009` `[IMP]` Zablokuj operację skutkową, gdy pre-action audit write zawiedzie. Zależy od: AUD-007, TOOL-001. Dowód: fail-closed integration test dla R4/R5.
- [ ] `AUD-010` `[IMP]` Dodaj manifest wersji model/prompt/tool/skill/policy. Zależy od: AUD-001. Dowód: komplet pól w zdarzeniu decyzji.
- [ ] `AUD-011` `[DB]` Zdefiniuj metadane artefaktu bez przechowywania blobu w DB. Zależy od: AUD-002. Dowód: tenant, hash, MIME, size, retention.
- [ ] `AUD-012` `[DEP]` Zweryfikuj klienta obiektowego storage i licencję usługi. Zależy od: DISC-016. Dowód: rejestr wersji i CVE.
- [ ] `AUD-013` `[IMP]` Zapisuj artefakt pod niezgadywalnym kluczem tenant/task. Zależy od: AUD-011, AUD-012. Dowód: test cross-tenant deny.
- [ ] `AUD-014` `[IMP]` Weryfikuj checksumę przy zapisie i odczycie artefaktu. Zależy od: AUD-013. Dowód: test uszkodzonego blobu.
- [ ] `AUD-015` `[IMP]` Zaimplementuj kwarantannę plików. Zależy od: AUD-013. Dowód: plik niedostępny przed decyzją skanera.
- [ ] `AUD-016` `[IMP]` Zaimplementuj interfejs skanera malware i jawne wyniki. Zależy od: AUD-015. Dowód: clean, infected, timeout i unavailable.
- [ ] `AUD-017` `[IMP]` Normalizuj nazwy plików i odrzucaj path traversal. Zależy od: AUD-011. Dowód: testy separatorów, Unicode i reserved names.
- [ ] `AUD-018` `[IMP]` Egzekwuj limity MIME, rozmiaru, TTL i źródła uploadu. Zależy od: AUD-011. Dowód: test każdej granicy.
- [ ] `AUD-019` `[IMP]` Zaimplementuj offline replay bez wykonywania narzędzi. Zależy od: AUD-006, ORCH-019. Dowód: replay odtwarza decyzje z recorded observations.
- [ ] `AUD-020` `[API]` Dodaj autoryzowany eksport audytu z podpisanym manifestem. Zależy od: AUD-014, AUD-019. Dowód: contract test i weryfikacja podpisu.
- [ ] `AUD-021` `[IMP]` Zaimplementuj retencję per data class i tenant. Zależy od: AUD-002, AUD-013. Dowód: test zegarem `TimeProvider`.
- [ ] `AUD-022` `[IMP]` Pozostawiaj tombstone integralności po usunięciu artefaktu. Zależy od: AUD-021. Dowód: hash i powód retencji bez treści.
- [ ] `AUD-023` `[DOC]` Zatwierdź bazowe retencje z DPO i security. Zależy od: AUD-002, AUD-021. Dowód: owner, okres i podstawa dla każdej klasy.
- [ ] `AUD-024` `[TST]` Przetestuj brak sekretów w logach, spans, audycie i artefaktach. Zależy od: AUD-003, AUD-004, OBSV-015. Dowód: fixtures token/cookie/password nie występują.

### 5.7. API — control plane i komunikacja czasu rzeczywistego

- [ ] `API-001` `[API]` Wersjonuj bazową ścieżkę REST i error contract. Zależy od: GOV-025. Dowód: OpenAPI i stabilny `ProblemDetails` code.
- [ ] `API-002` `[API]` Dodaj kontrakt utworzenia tasku z limitem celu i parametrów. Zależy od: TASK-001, ID-010. Dowód: contract test poprawnego i nadmiernego payloadu.
- [ ] `API-003` `[API]` Dodaj endpoint utworzenia tasku delegujący do use case. Zależy od: API-002, TASK-016. Dowód: auth, tenant i integracyjny test 202.
- [ ] `API-004` `[API]` Dodaj endpoint odczytu tasku scoped do ownera/roli. Zależy od: API-003. Dowód: 200/404 bez wycieku cross-tenant.
- [ ] `API-005` `[API]` Dodaj paginowaną listę tasków z bounded page size. Zależy od: API-004. Dowód: stabilny cursor i tenant filter.
- [ ] `API-006` `[API]` Dodaj komendę pause z idempotency key. Zależy od: TASK-018. Dowód: conflict dla terminalnego tasku.
- [ ] `API-007` `[API]` Dodaj komendę resume z optimistic concurrency. Zależy od: TASK-018. Dowód: stale version odrzucona.
- [ ] `API-008` `[API]` Dodaj komendę cancel z audytem. Zależy od: TASK-018, AUD-007. Dowód: powtórzenie jest idempotentne.
- [ ] `API-009` `[API]` Dodaj komendę doprecyzowania celu tworzącą rewizję. Zależy od: TASK-008. Dowód: poprzednia rewizja pozostaje dostępna.
- [ ] `API-010` `[API]` Zdefiniuj wersjonowane zdarzenia SignalR task timeline. Zależy od: MSG-002, AUD-001. Dowód: contract test N/N-1.
- [ ] `API-011` `[API]` Autoryzuj grupy SignalR per tenant i task. Zależy od: API-010, ID-010. Dowód: cross-tenant subscribe odrzucone.
- [ ] `API-012` `[API]` Publikuj task state events po trwałym zapisie. Zależy od: API-010, MSG-005. Dowód: UI nie widzi stanu przed commit.
- [ ] `API-013` `[API]` Dodaj resume stream od sequence/cursor. Zależy od: API-012. Dowód: reconnect nie gubi i nie duplikuje logicznie zdarzeń.
- [ ] `API-014` `[API]` Dodaj readiness wymaganych zależności API. Zależy od: ID-005, TASK-016. Dowód: `/health` fail przy braku DB.
- [ ] `API-015` `[API]` Utrzymaj liveness bez odpytywania zależności. Zależy od: —. Dowód: `/alive` działa podczas awarii DB.
- [ ] `API-016` `[TST]` Uruchom testy kontraktowe JSON/OpenAPI/error codes. Zależy od: API-001–015. Dowód: snapshot semantyczny bez przypadkowego breaking change.

### 5.8. LLM — providerzy, structured output i routing

- [ ] `LLM-001` `[DEP]` Zweryfikuj wersję i licencję `Microsoft.Extensions.AI`. Zależy od: GOV-017. Dowód: rejestr i lockfile.
- [ ] `LLM-002` `[IMP]` Zdefiniuj provider-neutral port chat client należący do konsumenta. Zależy od: GOV-004. Dowód: brak zależności capability od konkretnego providera.
- [ ] `LLM-003` `[IMP]` Zdefiniuj immutable model profile z capabilities i data class. Zależy od: DISC-014, AUD-002. Dowód: walidacja brakującej licencji lub rewizji.
- [ ] `LLM-004` `[IMP]` Zbuduj registry modeli z fail-closed lookup. Zależy od: LLM-003. Dowód: nieznany model odrzucony.
- [ ] `LLM-005` `[IMP]` Zdefiniuj wersjonowany schema planner output. Zależy od: GOV-004, TOOL-002. Dowód: action/clarification/complete są rozłączne.
- [ ] `LLM-006` `[IMP]` Waliduj structured output przed policy. Zależy od: LLM-005. Dowód: unknown fields, size i invalid enum odrzucone.
- [ ] `LLM-007` `[IMP]` Ogranicz odpowiedź do jednej proponowanej akcji. Zależy od: LLM-006. Dowód: test dwóch akcji kończy recovery.
- [ ] `LLM-008` `[IMP]` Zbuduj prompt assembly oddzielający instrukcje od untrusted page data. Zależy od: LLM-005, OBSV-014. Dowód: snapshot promptu z wyraźnymi granicami.
- [ ] `LLM-009` `[IMP]` Filtruj tool catalog według user/skill/domain/stage. Zależy od: TOOL-002, POL-009. Dowód: model nie otrzymuje narzędzia poza scope.
- [ ] `LLM-010` `[IMP]` Audytuj prompt/model/parameters/catalog version bez sekretów. Zależy od: AUD-010, LLM-008. Dowód: kompletne zdarzenie inference.
- [ ] `LLM-011` `[IMP]` Obsłuż timeout providera jako jawny wynik recovery. Zależy od: LLM-002. Dowód: bounded cancellation i brak retry skutku.
- [ ] `LLM-012` `[IMP]` Obsłuż provider unavailable i refusal osobnymi kodami. Zależy od: LLM-002. Dowód: deterministyczne mapowanie.
- [ ] `LLM-013` `[IMP]` Ogranicz jednorazową korektę błędnego formatu. Zależy od: LLM-006. Dowód: limit prób i audyt korekty.
- [ ] `LLM-014` `[IMP]` Zablokuj automatyczny zapis page content do long-term memory. Zależy od: LLM-008. Dowód: test portów i brak ścieżki zapisu.
- [ ] `LLM-015` `[IMP]` Zaimplementuj routing po capability, data class, license, hardware i budget. Zależy od: LLM-003, LLM-004. Dowód: test każdej osi i fail-closed.
- [ ] `LLM-016` `[TST]` Zbuduj deterministyczny eval harness przypiętego modelu. Zależy od: LLM-005–015. Dowód: zapis wersji, seed/config i wyniku.
- [ ] `LLM-017` `[OPS]` Zablokuj promocję modelu lub promptu bez regresji evals. Zależy od: LLM-016. Dowód: pipeline gate z negatywnym fixture.

### 5.9. BRW — lifecycle i izolacja Browser Workera

- [ ] `BRW-001` `[BRW]` Zdefiniuj immutable browser session identity i owner tenant/user/task. Zależy od: ID-001, ID-002, TASK-001. Dowód: test braku współdzielenia ownera.
- [ ] `BRW-002` `[BRW]` Zdefiniuj stany session lifecycle. Zależy od: GOV-006. Dowód: dozwolone i zabronione przejścia.
- [ ] `BRW-003` `[BRW]` Dodaj TTL sesji z `TimeProvider`. Zależy od: BRW-002. Dowód: expiry test bez czasu lokalnego.
- [ ] `BRW-004` `[BRW]` Dodaj heartbeat workera i detekcję awarii. Zależy od: BRW-002. Dowód: task nie raportuje sukcesu po utracie workera.
- [ ] `BRW-005` `[BRW]` Dodaj owner lease z fencing token. Zależy od: BRW-001. Dowód: stara generacja inputu odrzucona.
- [ ] `BRW-006` `[BRW]` Uruchamiaj osobny `BrowserContext` per session. Zależy od: BRW-001. Dowód: test izolacji cookies i storage.
- [ ] `BRW-007` `[BRW]` Wyłącz domyślne utrwalanie storage state. Zależy od: BRW-006. Dowód: cleanup usuwa profil po zamknięciu.
- [ ] `BRW-008` `[BRW]` Dodaj jawny stan oczekiwania na ręczny login/MFA. Zależy od: BRW-002. Dowód: credentials nie trafiają do API modelu.
- [ ] `BRW-009` `[BRW]` Zaimplementuj provision session z bounded timeout. Zależy od: BRW-003, BRW-006. Dowód: success, capacity deny i timeout.
- [ ] `BRW-010` `[BRW]` Zaimplementuj idempotentne close i cleanup. Zależy od: BRW-007, BRW-009. Dowód: wielokrotne close usuwa proces/pliki raz.
- [ ] `BRW-011` `[BRW]` Dodaj orphan detector i cleanup queue. Zależy od: BRW-004, BRW-010. Dowód: osierocona sesja usunięta w limicie.
- [ ] `BRW-012` `[BRW]` Obsłuż browser crash jako jawny wynik. Zależy od: BRW-004. Dowód: task przechodzi do recovery/failure, nie completed.
- [ ] `BRW-013` `[BRW]` Dodaj admission control per tenant/user/capacity. Zależy od: DISC-017, BRW-009. Dowód: noisy neighbor limit.
- [ ] `BRW-014` `[BRW]` Zaimplementuj fabrykę sesji używaną przez orkiestrator. Zależy od: BRW-009–013. Dowód: contract test port/adapter.
- [ ] `BRW-015` `[OPS]` Uruchamiaj Browser Worker non-root z read-only rootfs. Zależy od: DISC-005. Dowód: test kontenera i minimalnych capabilities.
- [ ] `BRW-016` `[OPS]` Odseparuj Browser Worker od sieci danych i Docker socketu. Zależy od: BRW-015. Dowód: test sieciowy deny.
- [ ] `BRW-017` `[BRW]` Wymuś aplikacyjną allowlistę nawigacji i redirectów. Zależy od: SITE-004. Dowód: redirect poza domenę odrzucony.
- [ ] `BRW-018` `[OPS]` Wymuś sieciową allowlistę egress Browser Workera. Zależy od: BRW-016, BRW-017. Dowód: DNS/IP/redirect bypass tests.
- [ ] `BRW-019` `[BRW]` Dodaj politykę tracingu per data class i retencję. Zależy od: AUD-002, AUD-013. Dowód: trace off/on/redacted zgodnie z polityką.
- [ ] `BRW-020` `[DEP]` Zdefiniuj kontrolowany cykl aktualizacji Playwright/Chromium. Zależy od: GOV-022. Dowód: pin, compatibility tests i rollback.
- [ ] `BRW-021` `[TST]` Przetestuj izolację dwóch równoległych sesji. Zależy od: BRW-014, BRW-018. Dowód: cookies, pliki, sieć i refs nie przeciekają.

### 5.10. OBSV — obserwacje, referencje i semantyka strony

- [ ] `OBSV-001` `[BRW]` Zdefiniuj monotoniczną `PageRevision` per session. Zależy od: BRW-001. Dowód: test increment i braku reuse po takeover.
- [ ] `OBSV-002` `[BRW]` Rozszerz observation o URL, title i revision. Zależy od: OBSV-001. Dowód: contract serialization test.
- [ ] `OBSV-003` `[BRW]` Dodaj semantyczne elementy role/name/value/visible/enabled. Zależy od: OBSV-002. Dowód: test ARIA fixture.
- [ ] `OBSV-004` `[BRW]` Dodaj page messages i dialogs do obserwacji. Zależy od: OBSV-002. Dowód: deterministyczna kolejność.
- [ ] `OBSV-005` `[BRW]` Dodaj stan kart, popupów i aktywnej strony. Zależy od: OBSV-002. Dowód: fixture popup/tab.
- [ ] `OBSV-006` `[BRW]` Dodaj stan downloadu bez automatycznego otwarcia pliku. Zależy od: OBSV-002, AUD-015. Dowód: zdarzenie i kwarantanna.
- [ ] `OBSV-007` `[BRW]` Ogranicz snapshot do danych semantycznych bez całego HTML/skryptów. Zależy od: OBSV-003. Dowód: test braku script/full DOM.
- [ ] `OBSV-008` `[BRW]` Zbuduj revision-scoped element reference registry. Zależy od: OBSV-001, OBSV-003. Dowód: ref działa wyłącznie w jednej rewizji.
- [ ] `OBSV-009` `[BRW]` Odrzucaj stale expected revision przed lokalizacją elementu. Zależy od: OBSV-008. Dowód: brak kliknięcia w teście stale.
- [ ] `OBSV-010` `[BRW]` Unieważniaj registry po nawigacji, popupie i reloadzie. Zależy od: OBSV-008. Dowód: test każdego zdarzenia.
- [ ] `OBSV-011` `[BRW]` Obsłuż iframe w semantycznym snapshotcie. Zależy od: OBSV-003. Dowód: fixture same-origin i jawny wynik cross-origin.
- [ ] `OBSV-012` `[BRW]` Obsłuż wspierany Shadow DOM. Zależy od: OBSV-003. Dowód: fixture open shadow root.
- [ ] `OBSV-013` `[BRW]` Obsłuż wirtualizowaną tabelę z bounded window. Zależy od: DISC-007, OBSV-003. Dowód: fixture nie materializuje całej tabeli.
- [ ] `OBSV-014` `[BRW]` Dodaj znacznik untrusted do page/API/file content. Zależy od: OBSV-002, AUD-002. Dowód: kontrakt zachowuje pochodzenie danych.
- [ ] `OBSV-015` `[BRW]` Redaguj sensitive regions screenshotu przed LLM. Zależy od: AUD-003, AUD-004. Dowód: test pikseli/regionów fixture.
- [ ] `OBSV-016` `[BRW]` Wymuś deterministyczne sortowanie obserwacji. Zależy od: OBSV-003–006. Dowód: ten sam stan daje ten sam hash.
- [ ] `OBSV-017` `[BRW]` Wymuś limit rozmiaru i selekcję kontekstu. Zależy od: OBSV-016. Dowód: bounded payload dla dużej tabeli.
- [ ] `OBSV-018` `[TST]` Dodaj TestSites dla iframe, shadow, grid, popup, dialog i download. Zależy od: OBSV-004–013. Dowód: deterministyczne fixtures bez Internetu.
- [ ] `OBSV-019` `[TST]` Dodaj regresje stale revision i cross-session ref. Zależy od: OBSV-008–010. Dowód: 100% scenariuszy odrzuconych przed akcją.

### 5.11. TOOL — wersjonowany katalog typowanych narzędzi

- [ ] `TOOL-001` `[IMP]` Zdefiniuj immutable tool definition z permission/risk/consequence. Zależy od: GOV-008. Dowód: walidacja wszystkich pól.
- [ ] `TOOL-002` `[IMP]` Zdefiniuj wersjonowany catalog identity i checksum. Zależy od: TOOL-001. Dowód: zmiana definicji zmienia wersję/hash.
- [ ] `TOOL-003` `[IMP]` Dodaj JSON Schema argumentów z limitami. Zależy od: TOOL-001. Dowód: invalid size/format/enum odrzucone.
- [ ] `TOOL-004` `[IMP]` Dodaj semantykę `ReadOnly/Idempotent/ConditionallyIdempotent/NonIdempotent`. Zależy od: TOOL-001. Dowód: każda definicja ma wartość.
- [ ] `TOOL-005` `[IMP]` Dodaj preconditions, timeout, retry class i verifier descriptor. Zależy od: TOOL-001. Dowód: katalog nie publikuje niepełnego toola.
- [ ] `TOOL-006` `[BRW]` Zaimplementuj typed navigate z allowlistą domeny. Zależy od: BRW-017, TOOL-003. Dowód: happy path i redirect deny.
- [ ] `TOOL-007` `[BRW]` Zaimplementuj back i zarządzanie kartami. Zależy od: OBSV-005, TOOL-003. Dowód: aktywna karta audytowana.
- [ ] `TOOL-008` `[BRW]` Zaimplementuj click przez revision-bound ref. Zależy od: OBSV-009, TOOL-003. Dowód: stale ref nie wywołuje inputu.
- [ ] `TOOL-009` `[BRW]` Zaimplementuj fill bez logowania wartości sensitive. Zależy od: OBSV-009, AUD-004. Dowód: secret fixture nie występuje w logu.
- [ ] `TOOL-010` `[BRW]` Zaimplementuj select/check/press przez semantyczne refs. Zależy od: OBSV-009. Dowód: ARIA fixtures.
- [ ] `TOOL-011` `[BRW]` Zaimplementuj bounded wait i scroll. Zależy od: TOOL-003. Dowód: timeout i cancellation.
- [ ] `TOOL-012` `[BRW]` Zaimplementuj read URL/title/form/table/messages. Zależy od: OBSV-002–006. Dowód: narzędzia R0 nie modyfikują strony.
- [ ] `TOOL-013` `[BRW]` Zaimplementuj screenshot z redakcją. Zależy od: OBSV-015. Dowód: polityka danych wymuszona.
- [ ] `TOOL-014` `[BRW]` Zaimplementuj upload wyłącznie z zatwierdzonego artifact ID. Zależy od: AUD-018. Dowód: dowolna ścieżka hosta niemożliwa w schema.
- [ ] `TOOL-015` `[BRW]` Zaimplementuj download do kwarantanny. Zależy od: AUD-015, OBSV-006. Dowód: plik nie jest wykonywany ani otwierany.
- [ ] `TOOL-016` `[BRW]` Zaimplementuj jawne accept/dismiss dialog. Zależy od: OBSV-004. Dowód: osobna risk policy.
- [ ] `TOOL-017` `[TST]` Udowodnij brak arbitrary JS, shell, cookie, HTTP i file path w katalogu. Zależy od: TOOL-002–016. Dowód: architecture/security tests.
- [ ] `TOOL-018` `[IMP]` Odrzucaj nieznane tool name i argument przed Policy Engine. Zależy od: TOOL-002, TOOL-003. Dowód: denial plus security audit event.
- [ ] `TOOL-019` `[SEC]` Przejrzyj zmianę każdej klasy ryzyka i katalogu. Zależy od: TOOL-001–018. Dowód: zaakceptowana wersja katalogu.

### 5.12. POL — Policy Engine i approvals

- [ ] `POL-001` `[IMP]` Zdefiniuj immutable policy context bez werdyktu LLM. Zależy od: ID-009, TOOL-001. Dowód: user/tenant/tool/domain/record/environment/time.
- [ ] `POL-002` `[IMP]` Zdefiniuj jawne wyniki allow/deny/require-approval. Zależy od: POL-001. Dowód: stabilne codes i reasons.
- [ ] `POL-003` `[IMP]` Wymuś deny dla braku polityki. Zależy od: POL-002. Dowód: test unknown context.
- [ ] `POL-004` `[IMP]` Klasyfikuj R0–R5 wyłącznie z katalogu i policy. Zależy od: TOOL-001, POL-002. Dowód: model nie może obniżyć risk.
- [ ] `POL-005` `[IMP]` Oceniaj permission użytkownika i tenantu. Zależy od: ID-009, POL-001. Dowód: matrix tests.
- [ ] `POL-006` `[IMP]` Oceniaj site/domain/egress scope. Zależy od: SITE-004, POL-001. Dowód: redirect i subdomain boundary.
- [ ] `POL-007` `[IMP]` Oceniaj environment, record i value limits. Zależy od: POL-001. Dowód: test test/prod i przekroczenia wartości.
- [ ] `POL-008` `[IMP]` Oceniaj skill mode i version. Zależy od: SKILL-001, POL-001. Dowód: Observe nie uzyskuje skutku.
- [ ] `POL-009` `[IMP]` Zbuduj wersjonowany policy set z effective time. Zależy od: POL-002–008. Dowód: deterministyczna decyzja dla historycznej wersji.
- [ ] `POL-010` `[IMP]` Zdefiniuj canonical action intent do approval. Zależy od: TOOL-003, TASK-010. Dowód: stabilna serializacja.
- [ ] `POL-011` `[IMP]` Uwzględnij tenant/user/task/session/tool/arguments/target/environment/revision/versions/nonce/expiry w action hash. Zależy od: POL-010. Dowód: zmiana każdego pola zmienia hash.
- [ ] `POL-012` `[IMP]` Zdefiniuj approval state machine. Zależy od: POL-011, ID-013. Dowód: nieodwracalne reject/expire/consume.
- [ ] `POL-013` `[DB]` Zmapuj approval z optimistic concurrency. Zależy od: POL-012. Dowód: dwóch approverów nie konsumuje dwa razy.
- [ ] `POL-014` `[API]` Dodaj kontrakt pełnego approval preview. Zależy od: POL-011. Dowód: środowisko, rekord, before/after, odbiorca, konsekwencja.
- [ ] `POL-015` `[API]` Dodaj approve/reject z resource authorization i reauth. Zależy od: POL-013, POL-014, ID-015. Dowód: self-approval/four-eyes tests.
- [ ] `POL-016` `[IMP]` Wymuś approval dla R4 i silny approval dla R5. Zależy od: POL-004, POL-012. Dowód: brak ścieżki execute bez zgody.
- [ ] `POL-017` `[IMP]` Reobserve i porównaj bound facts przed wykonaniem. Zależy od: OBSV-009, POL-011. Dowód: TOCTOU invalidates approval.
- [ ] `POL-018` `[IMP]` Konsumuj approval atomowo z rejestracją execution attempt. Zależy od: POL-013, AUD-009. Dowód: crash nie umożliwia reuse.
- [ ] `POL-019` `[IMP]` Unieważniaj approval po zmianie policy/skill/environment/revision. Zależy od: POL-009, POL-012. Dowód: test każdej bound fact.
- [ ] `POL-020` `[IMP]` Wygaszaj approval przez `TimeProvider`. Zależy od: POL-012. Dowód: pauza nie przedłuża TTL.
- [ ] `POL-021` `[TST]` Dodaj mutacyjne lub równoważne testy policy i hash binding. Zależy od: POL-001–020. Dowód: usunięcie dowolnego bound field wykryte.
- [ ] `POL-022` `[SEC]` Wykonaj niezależny przegląd Policy/Approval/TOCTOU. Zależy od: POL-021. Dowód: brak high/critical bez formalnej akceptacji.

### 5.13. ORCH — pętla agenta, verification i recovery

- [ ] `ORCH-001` `[IMP]` Zdefiniuj wejście jednego kroku z observation, limits i versions. Zależy od: TASK-010, OBSV-017. Dowód: immutable request.
- [ ] `ORCH-002` `[IMP]` Zaimplementuj fazę observe zapisującą hash i revision. Zależy od: ORCH-001, AUD-006. Dowód: audit przed planowaniem.
- [ ] `ORCH-003` `[IMP]` Zaimplementuj fazę plan z pojedynczym structured output. Zależy od: LLM-007, ORCH-002. Dowód: clarification i complete bez tool execution.
- [ ] `ORCH-004` `[IMP]` Waliduj tool schema przed policy. Zależy od: TOOL-018, ORCH-003. Dowód: invalid call nie dociera do policy adaptera.
- [ ] `ORCH-005` `[IMP]` Zaimplementuj niezależną fazę policy. Zależy od: POL-009, ORCH-004. Dowód: deny kończy krok bez akcji.
- [ ] `ORCH-006` `[IMP]` Zatrzymaj run w `AwaitingApproval` dla właściwej decyzji. Zależy od: POL-016, ORCH-005. Dowód: checkpoint przed oczekiwaniem.
- [ ] `ORCH-007` `[IMP]` Wznowienie po approval ponownie waliduje auth i bound facts. Zależy od: ID-020, POL-017, ORCH-006. Dowód: revoke/TOCTOU deny.
- [ ] `ORCH-008` `[IMP]` Zaimplementuj execute dokładnie jednej akcji. Zależy od: ORCH-005, TOOL-005. Dowód: drugi call niemożliwy w stepie.
- [ ] `ORCH-009` `[IMP]` Przypisz idempotency key do effectful execution attempt. Zależy od: TASK-012, POL-018. Dowód: stable key w retry dozwolonym.
- [ ] `ORCH-010` `[IMP]` Zaimplementuj verifier niezależny od success toola. Zależy od: TOOL-005, ORCH-008. Dowód: kliknięcie bez postcondition nie kończy tasku.
- [ ] `ORCH-011` `[IMP]` Mapuj timeout po nieidempotentnym send do `OutcomeUnknown`. Zależy od: TASK-005, ORCH-009. Dowód: brak auto-retry.
- [ ] `ORCH-012` `[IMP]` Zaimplementuj reconciliation dla `OutcomeUnknown`. Zależy od: ORCH-010, ORCH-011. Dowód: niezależny read rozstrzyga albo eskaluje.
- [ ] `ORCH-013` `[IMP]` Zaimplementuj recovery po błędzie postcondition. Zależy od: TASK-004, ORCH-010. Dowód: bounded recovery bez cofania nieznanego skutku.
- [ ] `ORCH-014` `[IMP]` Wymuś limity step/replan/retry/time. Zależy od: TASK-011, ORCH-003. Dowód: każdy limit daje stabilny terminal/recovery code.
- [ ] `ORCH-015` `[IMP]` Wykrywaj identyczne akcje i obserwacje bez postępu. Zależy od: TASK-022, ORCH-003. Dowód: loop kończy się bez skutku.
- [ ] `ORCH-016` `[IMP]` Checkpointuj po każdej fazie bezpiecznej do wznowienia. Zależy od: TASK-020, ORCH-002–013. Dowód: crash matrix dla granic faz.
- [ ] `ORCH-017` `[IMP]` Audytuj observation/plan/policy/approval/action/result/verification. Zależy od: AUD-001, ORCH-002–013. Dowód: kompletna sekwencja correlation IDs.
- [ ] `ORCH-018` `[IMP]` Zaimplementuj bezpieczne graceful shutdown workera. Zależy od: ORCH-016, MSG-008. Dowód: lease zwolniony albo wygasa bez podwójnej akcji.
- [ ] `ORCH-019` `[TST]` Zbuduj deterministyczny test pełnego kroku na fake ports. Zależy od: ORCH-001–018. Dowód: happy, deny, approval, unknown i recovery.
- [ ] `ORCH-020` `[TST]` Zbuduj integration test restartu orkiestratora. Zależy od: ORCH-016, ORCH-018. Dowód: safe resume z realnym store/outbox.

### 5.14. TAKE — live view i manual takeover

- [ ] `TAKE-001` `[BRW]` Zdefiniuj kontrakt stanu agent/user controlled. Zależy od: BRW-005, GOV-015. Dowód: mutually exclusive owner.
- [ ] `TAKE-002` `[BRW]` Zaimplementuj acquire user lease z fencing token. Zależy od: TAKE-001. Dowód: agent input odrzucony po przejęciu.
- [ ] `TAKE-003` `[BRW]` Zatrzymaj planner przed potwierdzeniem takeover. Zależy od: ORCH-016, TAKE-002. Dowód: checkpoint i brak równoległej akcji.
- [ ] `TAKE-004` `[BRW]` Zaimplementuj strumień aktualnej aktywnej karty. Zależy od: DISC-009, OBSV-005. Dowód: bounded latency i auth.
- [ ] `TAKE-005` `[API]` Zdefiniuj wersjonowany control-plane contract takeover. Zależy od: TAKE-001. Dowód: contract tests i tenant auth.
- [ ] `TAKE-006` `[BRW]` Waliduj keyboard/mouse input względem aktywnego lease. Zależy od: TAKE-002, TAKE-005. Dowód: stale token odrzucony.
- [ ] `TAKE-007` `[BRW]` Zaimplementuj release/expiry user lease. Zależy od: TAKE-002. Dowód: timeout nie oddaje kontroli bez audytu.
- [ ] `TAKE-008` `[BRW]` Unieważnij observation/ref registry po zwrocie kontroli. Zależy od: TAKE-007, OBSV-008. Dowód: agent musi reobserve.
- [ ] `TAKE-009` `[IMP]` Audytuj acquire/input-session/release bez treści sekretów. Zależy od: AUD-001, TAKE-002–008. Dowód: timeline ownerów.
- [ ] `TAKE-010` `[TST]` Przetestuj wyścigi acquire/release/input. Zależy od: TAKE-002–008. Dowód: zero dwóch aktywnych źródeł inputu.
- [ ] `TAKE-011` `[TST]` Przetestuj MFA wykonywane ręcznie bez prompt leakage. Zależy od: BRW-008, TAKE-004. Dowód: secret fixtures nie opuszczają workera.

### 5.15. SKILL — schema, runtime, registry i publikacja skills Praxiary

- [ ] `SKILL-001` `[SKL]` Wersjonuj schema definition i określ compatibility policy. Zależy od: GOV-011. Dowód: `$schema`/version i test N/N-1.
- [ ] `SKILL-002` `[SKL]` Waliduj `id`, SemVer, site i environment scope. Zależy od: SKILL-001. Dowód: positive/negative fixtures.
- [ ] `SKILL-003` `[SKL]` Waliduj inputs type/required/rules/sensitivity/redaction. Zależy od: SKILL-001. Dowód: secret input fixture.
- [ ] `SKILL-004` `[SKL]` Waliduj permissions i risk niezależnie od tekstu promptu. Zależy od: SKILL-001, TOOL-001. Dowód: unknown permission/risk odrzucone.
- [ ] `SKILL-005` `[SKL]` Waliduj preconditions, steps, assertions i postconditions. Zależy od: SKILL-001. Dowód: pusty skutek bez verifier odrzucony.
- [ ] `SKILL-006` `[SKL]` Waliduj approval points i recovery. Zależy od: SKILL-001, POL-016. Dowód: R4/R5 bez approval odrzucone.
- [ ] `SKILL-007` `[SKL]` Ogranicz kroki do opublikowanego tool catalog. Zależy od: TOOL-002, SKILL-005. Dowód: arbitrary tool odrzucony.
- [ ] `SKILL-008` `[SKL]` Zablokuj JavaScript, shell, XPath i współrzędne jako normalną ścieżkę. Zależy od: SKILL-005. Dowód: negatywne fixtures.
- [ ] `SKILL-009` `[SKL]` Zdefiniuj stabilne locator semantics z wariantami locale. Zależy od: OBSV-003. Dowód: identyfikator domenowy niezależny od tłumaczenia.
- [ ] `SKILL-010` `[SKL]` Zdefiniuj route requirement ProjectionApi/Browser/Hybrid. Zależy od: GOV-012. Dowód: enum i validation.
- [ ] `SKILL-011` `[SKL]` Zaimplementuj parser YAML z limitami rozmiaru i głębokości. Zależy od: SKILL-001. Dowód: oversized/alias bomb odrzucone.
- [ ] `SKILL-012` `[SKL]` Zaimplementuj semantic validator. Zależy od: SKILL-002–010. Dowód: zagregowane stabilne error codes.
- [ ] `SKILL-013` `[SKL]` Zaimplementuj immutable skill version z checksumą. Zależy od: SKILL-012. Dowód: publikowanej treści nie można zmienić.
- [ ] `SKILL-014` `[SKL]` Weryfikuj podpis i provenance skill version. Zależy od: SKILL-013, GOV-023. Dowód: tampered artifact odrzucony.
- [ ] `SKILL-015` `[DB]` Zmapuj skill identity, versions i lifecycle. Zależy od: SKILL-013. Dowód: opublikowana wersja append-only.
- [ ] `SKILL-016` `[SKL]` Egzekwuj Observe/Assist/Execute bez omijania policy. Zależy od: SKILL-012, POL-008. Dowód: mode matrix tests.
- [ ] `SKILL-017` `[SKL]` Zaimplementuj precondition stop przed skutkiem. Zależy od: SKILL-016. Dowód: failed precondition nie wykonuje toola.
- [ ] `SKILL-018` `[SKL]` Zaimplementuj postcondition recovery/escalation. Zależy od: SKILL-017, ORCH-013. Dowód: jawny wynik po failure.
- [ ] `SKILL-019` `[SKL]` Zaimplementuj offline replay recorded observations. Zależy od: AUD-019, SKILL-018. Dowód: brak live effect.
- [ ] `SKILL-020` `[SKL]` Zdefiniuj lifecycle Draft→Published→Revoked. Zależy od: SKILL-015. Dowód: przejścia i niezmienność Published.
- [ ] `SKILL-021` `[SKL]` Zaimplementuj publish gate: schema, replay, security, business review. Zależy od: SKILL-014, SKILL-019, SKILL-020. Dowód: brak któregokolwiek dowodu blokuje publikację.
- [ ] `SKILL-022` `[SKL]` Zaimplementuj audyt publikacji, deprecacji, revoke i rollback. Zależy od: SKILL-020, AUD-007. Dowód: immutable lifecycle events.
- [ ] `SKILL-023` `[SKL]` Zaimplementuj compatibility matrix site/IFS/locale/customization. Zależy od: SKILL-013. Dowód: unsupported combination fail-closed.
- [ ] `SKILL-024` `[OPS]` Zablokuj nowe runy revoked skill i oceń aktywne taski. Zależy od: SKILL-020, ORCH-016. Dowód: test revoke podczas runu.

### 5.16. REC — recorder i workflow review skill

- [ ] `REC-001` `[SKL]` Zdefiniuj draft-only recording session. Zależy od: SKILL-020, TAKE-001. Dowód: brak ścieżki auto-publish.
- [ ] `REC-002` `[SKL]` Zapisuj observation before/after i action metadata. Zależy od: OBSV-016, REC-001. Dowód: sequence z correlation IDs.
- [ ] `REC-003` `[SKL]` Zapisuj locator semantics bez surowego DOM. Zależy od: OBSV-003, REC-002. Dowód: stable semantic locator.
- [ ] `REC-004` `[SKL]` Redaguj headers, tokens, cookies i payloady sieciowe. Zależy od: AUD-003, REC-002. Dowód: secret fixtures nie występują.
- [ ] `REC-005` `[SKL]` Maskuj sensitive form values i screen regions. Zależy od: AUD-004, OBSV-015. Dowód: draft zawiera placeholder sensitivity.
- [ ] `REC-006` `[SKL]` Wykrywaj kruche XPath, coordinates i locale text. Zależy od: REC-003. Dowód: lint severity i location.
- [ ] `REC-007` `[SKL]` Sugeruj wykryte Projection API bez tokenów. Zależy od: IFS-006, REC-004. Dowód: projection metadata only.
- [ ] `REC-008` `[SKL]` Generuj parametryzowany draft bez danych testowych. Zależy od: REC-002–007. Dowód: synthetic fixture nie występuje literalnie.
- [ ] `REC-009` `[SKL]` Wymagaj ręcznego nazwania parametrów i ownera procesu. Zależy od: REC-008. Dowód: validation gate.
- [ ] `REC-010` `[SKL]` Wymagaj uzasadnienia ręcznej zmiany risk. Zależy od: REC-008. Dowód: audit event before/after.
- [ ] `REC-011` `[SKL]` Umożliwiaj dodanie assertions, approval i recovery. Zależy od: REC-008, SKILL-005, SKILL-006. Dowód: edytowany draft przechodzi validator.
- [ ] `REC-012` `[SKL]` Ogranicz replay do non-production synthetic environment. Zależy od: SKILL-019, SITE-003. Dowód: production profile deny.
- [ ] `REC-013` `[TST]` Dodaj security tests sekretów i kruchego draftu. Zależy od: REC-004–012. Dowód: publish gate odrzuca fixtures.

### 5.17. SITE/IFS — profile witryn, IFS API-first i companion

- [ ] `SITE-001` `[IMP]` Zdefiniuj site profile z tenantem, domenami, locale i auth mode. Zależy od: ID-001. Dowód: walidacja URI i pustej allowlisty.
- [ ] `SITE-002` `[DB]` Zmapuj wersjonowane site profiles. Zależy od: SITE-001, ID-011. Dowód: historyczna wersja pozostaje dostępna.
- [ ] `SITE-003` `[IMP]` Zdefiniuj environment kind z jawnym Production. Zależy od: SITE-001. Dowód: brak wartości domyślnej ukrywającej produkcję.
- [ ] `SITE-004` `[IMP]` Zaimplementuj canonical domain/egress allowlist. Zależy od: SITE-001. Dowód: IDN, port, scheme, subdomain i redirect tests.
- [ ] `SITE-005` `[API]` Dodaj autoryzowane admin API profili i wersji. Zależy od: SITE-002, ID-010. Dowód: audit before/after.
- [ ] `IFS-001` `[IFS]` Rozszerz IFS environment profile o BaseUri/tenant/locale/kind. Zależy od: SITE-003, GOV-012. Dowód: immutable validation.
- [ ] `IFS-002` `[IFS]` Dodaj allowlistę projections i custom projections. Zależy od: IFS-001. Dowód: unknown projection deny.
- [ ] `IFS-003` `[DB]` Zmapuj wersjonowane IFS environment profiles. Zależy od: IFS-001, IFS-002. Dowód: tenant scope i historyczna wersja.
- [ ] `IFS-004` `[IFS]` Zdefiniuj kontrolowany import `$openapi`. Zależy od: IFS-002. Dowód: tylko admin, allowlisted host i bounded payload.
- [ ] `IFS-005` `[IFS]` Waliduj i wersjonuj importowany projection contract. Zależy od: IFS-004. Dowód: checksum i compatibility diff.
- [ ] `IFS-006` `[IFS]` Zbuduj projection registry z permissions/capabilities. Zależy od: IFS-005. Dowód: lookup fail-closed.
- [ ] `IFS-007` `[IFS]` Zaimplementuj klienta odczytu OData z user context. Zależy od: GOV-013, IFS-006. Dowód: brak szerszego service account.
- [ ] `IFS-008` `[IFS]` Waliduj query/action względem projection contract i allowlisty. Zależy od: IFS-006, IFS-007. Dowód: arbitrary entity/action odrzucone.
- [ ] `IFS-009` `[IFS]` Mapuj permission denied, expired session, version conflict i business error. Zależy od: IFS-007. Dowód: rozłączne result codes.
- [ ] `IFS-010` `[IFS]` Zdefiniuj operation registry z trasą ProjectionApi/Browser/Hybrid. Zależy od: IFS-006, SKILL-010. Dowód: jedna jawna trasa per capability/profile.
- [ ] `IFS-011` `[IFS]` Zaimplementuj router uwzględniający registry, policy i environment. Zależy od: IFS-010, POL-009. Dowód: model nie wybiera niskopoziomowej trasy.
- [ ] `IFS-012` `[IFS]` Preferuj API dla odczytu i walidacji. Zależy od: IFS-011. Dowód: route metrics i test preferencji.
- [ ] `IFS-013` `[IFS]` Zaimplementuj high-level read tool dla procesu DISC-002. Zależy od: IFS-007–012. Dowód: business result bez OData leakage do modelu.
- [ ] `IFS-014` `[IFS]` Zaimplementuj high-level R2 tool dla procesu DISC-003. Zależy od: IFS-007–012. Dowód: policy i verifier.
- [ ] `IFS-015` `[IFS]` Zaimplementuj high-level R4 tool dla procesu DISC-004. Zależy od: IFS-007–012, POL-018. Dowód: idempotency, approval i business verifier.
- [ ] `IFS-016` `[IFS]` Zaimplementuj Hybrid verifier UI+API. Zależy od: IFS-011, ORCH-010. Dowód: rozbieżność daje failure/unknown, nie success.
- [ ] `IFS-017` `[TST]` Dodaj compatibility suite release update IFS. Zależy od: IFS-005, SKILL-023. Dowód: supported matrix i diff report.
- [ ] `IFS-018` `[TST]` Dodaj locale/customization fixtures. Zależy od: SKILL-009, IFS-017. Dowód: wspierane warianty i jawne unsupported.
- [ ] `IFS-019` `[IFS]` Zdefiniuj contract companion tylko jeśli GOV-014 zatwierdza zakres. Zależy od: GOV-014. Dowód: minimalne typowane komendy.
- [ ] `IFS-020` `[IFS]` Zaimplementuj device registration i user presence companion. Zależy od: IFS-019, ID-019. Dowód: unregistered/absent user deny.
- [ ] `IFS-021` `[IFS]` Zaimplementuj bezpieczny kanał i osobną policy companion. Zależy od: IFS-020. Dowód: replay/stale device command deny.
- [ ] `IFS-022` `[IFS]` Zwracaj jawny unsupported, gdy companion jest niedostępny. Zależy od: IFS-019. Dowód: brak niebezpiecznego fallbacku.
- [ ] `IFS-023` `[SEC]` Przejrzyj delegation, projection allowlist i companion. Zależy od: IFS-001–022. Dowód: brak high/critical bez akceptacji.

### 5.18. WEB — workspace użytkownika, approvals, admin i audit explorer

- [ ] `WEB-001` `[WEB]` Utwórz feature-oriented shell React z TypeScript strict. Zależy od: GOV-025. Dowód: build i brak współdzielonego niejawnego stanu.
- [ ] `WEB-002` `[WEB]` Skonfiguruj centralne polskie i18n bez literalnego tekstu feature. Zależy od: WEB-001. Dowód: lint/test brakujących kluczy.
- [ ] `WEB-003` `[WEB]` Skonfiguruj bezpieczną sesję BFF/cookie bez tokenu w `localStorage`. Zależy od: ID-005. Dowód: browser storage inspection.
- [ ] `WEB-004` `[WEB]` Dodaj Zod validation dla bazowych odpowiedzi API. Zależy od: API-001. Dowód: invalid payload pokazuje bezpieczny błąd.
- [ ] `WEB-005` `[WEB]` Dodaj React Query client i politykę cache per tenant. Zależy od: WEB-003. Dowód: logout/tenant switch czyści dane.
- [ ] `WEB-006` `[WEB]` Zbuduj formularz utworzenia tasku. Zależy od: API-002, WEB-004. Dowód: keyboard, labels i validation.
- [ ] `WEB-007` `[WEB]` Zbuduj listę i widok tasku. Zależy od: API-004, API-005. Dowód: loading/empty/error/success.
- [ ] `WEB-008` `[WEB]` Podłącz SignalR timeline z reconnect cursor. Zależy od: API-010–013. Dowód: reconnect bez utraty zdarzeń.
- [ ] `WEB-009` `[WEB]` Pokaż Verified/Failed/OutcomeUnknown jako rozłączne stany. Zależy od: TASK-005, TASK-007. Dowód: nie opiera się tylko na kolorze.
- [ ] `WEB-010` `[WEB]` Dodaj pause/resume/cancel z optimistic UI ograniczonym do stanu pending. Zależy od: API-006–008. Dowód: rollback po conflict.
- [ ] `WEB-011` `[WEB]` Dodaj doprecyzowanie celu i historię rewizji. Zależy od: API-009. Dowód: poprzedni audyt widoczny.
- [ ] `WEB-012` `[WEB]` Zbuduj live browser view z nazwą środowiska i ownerem. Zależy od: TAKE-004, TAKE-005. Dowód: stale/offline states.
- [ ] `WEB-013` `[WEB]` Zbuduj takeover acquire/release dostępny klawiaturą. Zależy od: TAKE-005–008. Dowód: status agent/user stale widoczny.
- [ ] `WEB-014` `[WEB]` Zbuduj approval preview z pełnymi bound facts. Zależy od: POL-014. Dowód: rekord, env, before/after, odbiorca, skutek.
- [ ] `WEB-015` `[WEB]` Zbuduj approve/reject z reauth i four-eyes messaging. Zależy od: POL-015. Dowód: expired/invalidated/consumed states.
- [ ] `WEB-016` `[WEB]` Zbuduj Approval Center z filtrem zakresu delegacji. Zależy od: WEB-014, WEB-015. Dowód: approver nie widzi niedozwolonych danych.
- [ ] `WEB-017` `[WEB]` Zbuduj Skill Studio draft editor z walidacją. Zależy od: SKILL-012, REC-008. Dowód: error codes mapują się na pola.
- [ ] `WEB-018` `[WEB]` Zbuduj widok diff/replay/review/publish skill. Zależy od: SKILL-019–022. Dowód: brak auto-publish.
- [ ] `WEB-019` `[WEB]` Zbuduj admin tenant/role/site/environment. Zależy od: SITE-005, ID-009. Dowód: każda akcja autoryzowana serwerowo.
- [ ] `WEB-020` `[WEB]` Zbuduj admin tool/policy/model registry. Zależy od: TOOL-019, POL-009, LLM-004. Dowód: wersje i production warning.
- [ ] `WEB-021` `[WEB]` Zbuduj service/session/queue health view. Zależy od: OBS-009. Dowód: degraded i stale data oznaczone.
- [ ] `WEB-022` `[WEB]` Zbuduj Audit Explorer z autoryzowaną osią czasu. Zależy od: AUD-020. Dowód: redacted view user vs auditor.
- [ ] `WEB-023` `[WEB]` Dodaj eksport podpisanego manifestu audytu. Zależy od: WEB-022. Dowód: status generowania i audyt pobrania.
- [ ] `WEB-024` `[WEB]` Dodaj globalny live region błędów i correlation ID. Zależy od: API-001. Dowód: screen reader test.
- [ ] `WEB-025` `[WEB]` Przeprowadź WCAG 2.2 AA dla przepływów task/approval/takeover. Zależy od: WEB-006–024. Dowód: automatyczny i ręczny raport bez critical.
- [ ] `WEB-026` `[TST]` Dodaj Vitest zachowania komponentów i Zod boundaries. Zależy od: WEB-006–024. Dowód: happy/error/security states.
- [ ] `WEB-027` `[TST]` Dodaj Playwright E2E task/approval/takeover. Zależy od: WEB-006–024, SYS-004. Dowód: keyboard i brak produkcyjnego skutku.

### 5.19. OBS — telemetry, health, SLO i operacje

- [ ] `OBS-001` `[IMP]` Zdefiniuj stabilny correlation context task/run/step/session. Zależy od: TASK-001, TASK-010. Dowód: propagation test.
- [ ] `OBS-002` `[IMP]` Dodaj zredagowane structured logs bez interpolacji sekretów. Zależy od: AUD-003, OBS-001. Dowód: log capture tests.
- [ ] `OBS-003` `[IMP]` Dodaj OpenTelemetry traces dla API/outbox/orchestrator/browser/IFS. Zależy od: OBS-001. Dowód: pełny trace z granicami procesów.
- [ ] `OBS-004` `[IMP]` Dodaj low-cardinality metrics task outcomes i latency. Zależy od: TASK-007. Dowód: brak user/task ID w labels.
- [ ] `OBS-005` `[IMP]` Dodaj metrics approval deny/expiry/TOCTOU. Zależy od: POL-019. Dowód: stabilne dimensions.
- [ ] `OBS-006` `[IMP]` Dodaj metrics browser provisioning/capacity/orphans. Zależy od: BRW-011, BRW-013. Dowód: SLI queries.
- [ ] `OBS-007` `[IMP]` Dodaj metrics model format/recovery i tool failures. Zależy od: LLM-013, ORCH-013. Dowód: rozłączne reasons.
- [ ] `OBS-008` `[IMP]` Dodaj `/alive` i graceful shutdown do wszystkich hostów. Zależy od: GOV-025. Dowód: liveness bez external checks.
- [ ] `OBS-009` `[IMP]` Dodaj `/health` z minimalnymi wymaganymi zależnościami hosta. Zależy od: OBS-008. Dowód: readiness matrix.
- [ ] `OBS-010` `[OPS]` Zbuduj dashboard control plane SLO. Zależy od: OBS-003, OBS-004. Dowód: p95/p99 i error budget.
- [ ] `OBS-011` `[OPS]` Zbuduj dashboard browser capacity i cleanup. Zależy od: OBS-006. Dowód: sessions/CPU/RAM/queue age.
- [ ] `OBS-012` `[OPS]` Zbuduj dashboard policy/audit completeness. Zależy od: OBS-005, AUD-009. Dowód: unauthorized effects i missing verifier.
- [ ] `OBS-013` `[OPS]` Dodaj alert unauthorized effect i audit gap. Zależy od: OBS-012. Dowód: test alertu end-to-end.
- [ ] `OBS-014` `[OPS]` Dodaj alert orphan sessions i capacity saturation. Zależy od: OBS-011. Dowód: próg, owner i runbook.
- [ ] `OBS-015` `[OPS]` Dodaj alert outbox lag i workflow stalls. Zależy od: MSG-005, MSG-010. Dowód: próg, owner i runbook.
- [ ] `OBS-016` `[OPS]` Dodaj alert critical browser/runtime CVE. Zależy od: GOV-022, BRW-020. Dowód: emergency patch SLA.
- [ ] `OBS-017` `[DOC]` Zatwierdź SLO i error-budget policy z ownerami. Zależy od: OBS-010–016. Dowód: mierzalne SLI i reakcje.

### 5.20. OPS — środowiska, deployment, backup i disaster recovery

- [ ] `OPS-001` `[OPS]` Zdefiniuj typowaną konfigurację i start-time validation każdego hosta. Zależy od: GOV-025. Dowód: brak wymaganej opcji zatrzymuje startup.
- [ ] `OPS-002` `[OPS]` Oddziel konfigurację dev/integration/staging/production. Zależy od: OPS-001. Dowód: production nie dziedziczy sekretnych dev defaults.
- [ ] `OPS-003` `[OPS]` Podłącz sekrety z OpenBao/SOPS bez zapisu w obrazie. Zależy od: ID-019, GOV-021. Dowód: secret scan obrazu.
- [ ] `OPS-004` `[OPS]` Przypnij obrazy produkcyjne do digestów. Zależy od: GOV-023. Dowód: brak `latest` i mutable tag dependency.
- [ ] `OPS-005` `[OPS]` Dodaj profile Compose development i integration. Zależy od: OPS-002. Dowód: `docker compose config` dla obu profili.
- [ ] `OPS-006` `[OPS]` Dodaj hardened single-site production profile. Zależy od: OPS-003, OPS-004. Dowód: brak publicznych portów danych/control.
- [ ] `OPS-007` `[OPS]` Skonfiguruj TLS i service identity na granicach produkcyjnych. Zależy od: ID-019, OPS-006. Dowód: plaintext connection deny.
- [ ] `OPS-008` `[OPS]` Skonfiguruj sieciową segmentację control/data/browser. Zależy od: BRW-016, OPS-006. Dowód: matrix connectivity tests.
- [ ] `OPS-009` `[DB]` Zdefiniuj proces migracji poza startupem replik. Zależy od: TASK-015, MSG-004, AUD-008. Dowód: single migrator lease.
- [ ] `OPS-010` `[DB]` Przetestuj rollout N/N-1 schema i contracts. Zależy od: OPS-009. Dowód: mixed-version test.
- [ ] `OPS-011` `[OPS]` Zaimplementuj backup PostgreSQL i szyfrowanie. Zależy od: OPS-006. Dowód: backup z checksumą i retencją.
- [ ] `OPS-012` `[OPS]` Zaimplementuj backup object storage metadata/config. Zależy od: AUD-013, OPS-006. Dowód: manifest spójności.
- [ ] `OPS-013` `[OPS]` Przeprowadź restore PostgreSQL w izolowanym środowisku. Zależy od: OPS-011. Dowód: zmierzony RPO/RTO i test integralności.
- [ ] `OPS-014` `[OPS]` Przeprowadź restore artefaktów i weryfikację checksum. Zależy od: OPS-012, AUD-014. Dowód: brak silent corruption.
- [ ] `OPS-015` `[OPS]` Zdefiniuj kolejność DR usług. Zależy od: OPS-013, OPS-014. Dowód: runbook identity→DB→broker→storage→hosts.
- [ ] `OPS-016` `[OPS]` Przeprowadź game day utraty DB/brokera/workera. Zależy od: OPS-015, SYS-010. Dowód: findings, ownerzy i terminy.
- [ ] `OPS-017` `[OPS]` Dodaj canary promotion bez rebuild. Zależy od: GOV-023, OPS-010. Dowód: ten sam digest acceptance→production.
- [ ] `OPS-018` `[OPS]` Dodaj rollback/forward-fix i kill switch. Zależy od: OPS-017. Dowód: ćwiczenie kontrolne non-production.
- [ ] `OPS-019` `[OPS]` Zdefiniuj profile HA albo formalnie zaakceptuj single-site względem SLO. Zależy od: OBS-017, OPS-016. Dowód: decyzja architektury/SRE.

### 5.21. SYS — testy systemowe, bezpieczeństwo i evals

- [ ] `SYS-001` `[TST]` Zbuduj TestSites dla formularzy, tabel i komunikatów. Zależy od: GOV-025. Dowód: deterministyczne fixtures.
- [ ] `SYS-002` `[TST]` Zbuduj TestSites jawnego i ukrytego prompt injection. Zależy od: SYS-001. Dowód: tekst, atrybut, tabela, obraz/PDF fixture.
- [ ] `SYS-003` `[TST]` Zbuduj TestSites redirect/SSRF/egress bypass. Zależy od: SYS-001. Dowód: hostname, IP, DNS rebinding fixture bez Internetu.
- [ ] `SYS-004` `[TST]` Zbuduj E2E R0–R3 na TestSites. Zależy od: ORCH-019, TOOL-006–016, SYS-001. Dowód: verified business outcomes.
- [ ] `SYS-005` `[TST]` Zbuduj E2E R4 z approval binding. Zależy od: POL-021, SYS-004. Dowód: bez approval zero skutków.
- [ ] `SYS-006` `[TST]` Zbuduj E2E R5 z strong approval/four-eyes. Zależy od: ID-015, POL-021, SYS-005. Dowód: self-approval deny.
- [ ] `SYS-007` `[TST]` Zbuduj security eval prompt injection→tool/policy/egress. Zależy od: SYS-002, SYS-003, LLM-016. Dowód: zero unauthorized effects.
- [ ] `SYS-008` `[TST]` Zbuduj TOCTOU suite zmieniając każdą bound fact. Zależy od: POL-021, SYS-005. Dowód: 100% invalidated.
- [ ] `SYS-009` `[TST]` Zbuduj crash matrix browser/orchestrator/DB/broker. Zależy od: BRW-012, ORCH-020, MSG-011. Dowód: brak fałszywego success i duplicate effect.
- [ ] `SYS-010` `[TST]` Zbuduj chaos tests zależności z bounded recovery. Zależy od: SYS-009. Dowód: stabilne outcomes i alerts.
- [ ] `SYS-011` `[TST]` Zbuduj load test control plane bez LLM. Zależy od: API-016, OBS-010. Dowód: SLO p95/p99.
- [ ] `SYS-012` `[TST]` Zbuduj capacity/soak test równoległych sesji Chromium. Zależy od: BRW-013, OBS-011. Dowód: limit, queue i orphan cleanup.
- [ ] `SYS-013` `[TST]` Zbuduj eval certified skill success per risk class. Zależy od: SKILL-021, IFS-013–018. Dowód: progi z planu strategicznego.
- [ ] `SYS-014` `[TST]` Zbuduj test kompletności verifiera i audytu R4/R5. Zależy od: AUD-009, SYS-005, SYS-006. Dowód: 100% evidence chain.
- [ ] `SYS-015` `[SEC]` Przeprowadź threat modeling z OWASP ASVS i agentic AI. Zależy od: SYS-007–014. Dowód: model zagrożeń i zamknięte critical/high.
- [ ] `SYS-016` `[SEC]` Przeprowadź niezależny test tenant isolation. Zależy od: ID-021, SYS-007. Dowód: brak cross-tenant read/write/artifact/stream.
- [ ] `SYS-017` `[SEC]` Przeprowadź security review plików, audytu i retencji. Zależy od: AUD-024. Dowód: brak secret/PII finding high.
- [ ] `SYS-018` `[OPS]` Włącz pełną bramę quality/security/license na PR. Zależy od: GOV-020–024, SYS-004–017. Dowód: negatywne fixtures blokują merge.

### 5.22. BETA/GA — kontrolowana beta i uruchomienie produkcyjne

- [ ] `BETA-001` `[DOC]` Wyznacz ograniczoną grupę użytkowników i certyfikowane procesy. Zależy od: SYS-018. Dowód: owner, tenant, środowiska i zakres.
- [ ] `BETA-002` `[DOC]` Zatwierdź DPIA, data inventory i retencję dla bety. Zależy od: AUD-023, SYS-017. Dowód: decyzja DPO.
- [ ] `BETA-003` `[DOC]` Przygotuj runbook incident, kill switch, revoke i break-glass. Zależy od: OPS-018, OBS-013–016. Dowód: contacts i scenariusze.
- [ ] `BETA-004` `[DOC]` Przygotuj support workflow i zredagowany controlled replay. Zależy od: AUD-019, WEB-022. Dowód: role i approval break-glass.
- [ ] `BETA-005` `[OPS]` Uruchom shadow/Observe dla wybranych procesów. Zależy od: BETA-001–004. Dowód: okres pomiarowy bez skutków.
- [ ] `BETA-006` `[OPS]` Promuj procesy do Assist po spełnieniu progu. Zależy od: BETA-005, SYS-013. Dowód: decyzja ownera procesu.
- [ ] `BETA-007` `[OPS]` Promuj wybrane procesy do kontrolowanego Execute. Zależy od: BETA-006, SYS-014. Dowód: approvals/verifiers/audit aktywne.
- [ ] `BETA-008` `[OPS]` Przeprowadź dwa pełne okna pomiaru SLO. Zależy od: BETA-007, OBS-017. Dowód: raport error budgets.
- [ ] `BETA-009` `[OPS]` Przeprowadź game day z SRE i supportem. Zależy od: BETA-007, OPS-016. Dowód: zamknięte P0/P1 findings.
- [ ] `BETA-010` `[WEB]` Przeprowadź usability i accessibility research. Zależy od: WEB-025, BETA-007. Dowód: findings z ownerami.
- [ ] `BETA-011` `[DOC]` Zamknij P0/P1; nadaj formalne risk acceptance pozostałym P2. Zależy od: BETA-008–010. Dowód: release blocker report.
- [ ] `GA-001` `[SEC]` Przeprowadź final security assessment i remediation. Zależy od: BETA-011. Dowód: brak niezaakceptowanego high/critical.
- [ ] `GA-002` `[SEC]` Przeprowadź niezależny penetration test właściwego scope. Zależy od: GA-001. Dowód: raport i zamknięte findings.
- [ ] `GA-003` `[DEP]` Zamknij finalny przegląd licencji, modeli, SBOM i provenance. Zależy od: SYS-018. Dowód: podpisany evidence package.
- [ ] `GA-004` `[DOC]` Zatwierdź finalne RODO/DPIA, retencję i umowy. Zależy od: BETA-002, GA-001. Dowód: formalne approvals.
- [ ] `GA-005` `[DOC]` Przeszkol użytkowników, approverów, ekspertów, support i audytorów. Zależy od: BETA-011. Dowód: materiały i potwierdzenia.
- [ ] `GA-006` `[OPS]` Potwierdź backup, restore, failover, capacity i on-call. Zależy od: OPS-013–019, BETA-009. Dowód: aktualne ćwiczenia w evidence package.
- [ ] `GA-007` `[OPS]` Wykonaj canary tego samego podpisanego digestu co acceptance. Zależy od: GA-001–006. Dowód: verified digest i rollout metrics.
- [ ] `GA-008` `[TST]` Wykonaj smoke tests bez niekontrolowanego skutku produkcyjnego. Zależy od: GA-007. Dowód: zaakceptowany production test plan.
- [ ] `GA-009` `[OPS]` Wykonaj pierwsze certyfikowane zadania z pełnym evidence. Zależy od: GA-008. Dowód: audyt R4/R5 sprawdzony przez audytora.
- [ ] `GA-010` `[DOC]` Zamknij wszystkie kryteria akceptacji rozdziału 31 planu strategicznego. Zależy od: GA-009. Dowód: traceability matrix requirement→test/evidence.
- [ ] `GA-011` `[DOC]` Zatwierdź wejście do GA przez uprawnionych ownerów. Zależy od: GA-010. Dowód: release/change approval.
- [ ] `GA-012` `[OPS]` Zakończ okres podwyższonego wsparcia bez otwartego P0/P1. Zależy od: GA-011. Dowód: post-release review i przekazanie do operacji.

## 6. Reguły utrzymania wykazu

1. Każdy PR realizujący krok wpisuje jego ID w opisie i wskazuje dowód.
2. Checkbox zmienia się na `[x]` dopiero po przejściu wymaganej bramy i przeglądzie diffu.
3. Krok z decyzją zewnętrzną nie jest oznaczany jako wykonany na podstawie założenia Codex.
4. Nowe wymaganie otrzymuje nowy trwały ID oraz zależności; nie dopisuje się go po cichu do zakończonego kroku.
5. Defekt odkryty po zamknięciu kroku otrzymuje osobny ID naprawy i odnośnik do kroku źródłowego.
6. Zmiana pakietu jest osobnym krokiem `DEP`; refaktoring jest osobnym krokiem `REF`; test-only jest osobnym krokiem `TST`.
7. Operacja R4/R5 wymaga osobnych dowodów policy, approval, idempotency, verifier i audit, nawet gdy kod mieści się w jednym module.
8. Co najmniej raz na koniec każdej fazy należy wykonać `REV`, a dla granicy bezpieczeństwa jawny `SEC`.
9. Po zmianie planu strategicznego należy wykonać traceability review i dodać brakujące kroki, zamiast rozszerzać znaczenie istniejących ID.
10. Dokument opisuje pełny produkt; brak zasobów zmienia harmonogram, nie Definition of Done.

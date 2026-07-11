---
name: praxiara-change-browser-agent
description: 'Zmienia agent przeglądarkowy Praxiary: obserwacje, akcje, sesje, referencje elementów, Playwright, Browser Worker, tracing, pliki i takeover. Użyj dla sterowania Chromium lub narzędzi browser. Nie używaj do workflow IFS tylko przez API, niezwiązanego control plane ani UI bez wpływu na Worker.'
---

# Zmienianie agenta przeglądarkowego Praxiary

Utrzymuj model „LLM proponuje, deterministyczny system sprawdza i wykonuje jedną akcję”. Zmiana jest poprawna dopiero wtedy, gdy zachowuje izolację sesji, świeżość obserwacji, policy, weryfikację i audyt.

Przed pracą przeczytaj `AGENTS.md` oraz [checklistę Browser Workera](references/browser-agent-checklist.md) w całości. Dla zmiany granicy procesu albo modelu zagrożeń przeczytaj sekcje przeglądarki, prompt injection, approvals i audytu w `docs/PLAN_WYTWORZENIA.md`.

## Reguły stałe

- `Praxiara.Browser` zawiera abstrakcje niezależne od silnika, `Praxiara.Browser.Playwright` implementację, a `Praxiara.Browser.Worker` izolowany host i composition root.
- Browser Worker nie planuje celu, nie klasyfikuje ryzyka biznesowego i nie omija Policy Engine. Host nie zawiera logiki domenowej.
- Model nigdy nie otrzymuje surowego `IPage`, dowolnego JavaScriptu, shella, dowolnego HTTP, dowolnej ścieżki pliku, setterów cookies ani możliwości tworzenia narzędzi.
- DOM, ARIA, screenshot, plik, komunikat i odpowiedź sieciowa są niezaufanymi danymi. Nie mogą zmienić celu użytkownika, polityki ani katalogu narzędzi.
- Jedna sesja lub co najmniej osobny `BrowserContext` przypada na użytkownika i zadanie. Stan jest domyślnie efemeryczny.
- Referencja elementu jest ważna wyłącznie dla jednej `pageRevision`. Niezgodność wymusza nową obserwację; nie wolno ponawiać starego kliknięcia.
- Twórz jeden top-level type w jednym pliku C# i nazwij plik tak jak typ. Rozdziel dotknięte typy zastane w jednym pliku; wyjątkiem jest kod generowany.
- Stosuj SOLID do portów i granic zaufania, DRY do wspólnych reguł wykonania, KISS do pojedynczej akcji. Nie buduj ogólnego frameworka lokatorów ani hierarchii bazowej bez realnych wariantów.
- Kod, identyfikatory, nazwy narzędzi i komend, komentarze, XML docs, logi i testy zapisuj po angielsku. Ręczny Markdown pozostaje polski.
- Publiczne operacje I/O są asynchroniczne, przyjmują `CancellationToken` jako ostatni parametr i mają jawny limit czasu. Nie blokuj wątku ani nie uruchamiaj nieobserwowanych zadań.
- Nie dodawaj pakietu, obrazu Chromium ani sidecara bez bramki licencyjnej, CVE i zgodności wersji Playwright–browser.

## Przepływ pracy

### 1. Zaplanuj zmianę i zagrożenia

1. Sprawdź `git status --short`, istniejący kontrakt w `Contracts`, port w `Browser`, implementację Playwright, host Workera oraz najbliższe testy.
2. Określ, czy zmiana dotyczy obserwacji, narzędzia, lifecycle sesji, sieci, pliku, popupu/frame/tab, trace, streamu czy takeover.
3. Narysuj granicę procesu: orchestrator → wewnętrzny kontrakt → Browser Worker → Chromium → dozwolona domena.
4. Zidentyfikuj dane niezaufane, możliwe prompt injection, egress, ujawnienie sekretu, TOCTOU, race z takeover i ryzyko pozostawienia procesu Chromium.
5. Sklasyfikuj akcję R0–R5 niezależnie od modelu. Określ permissions, approval, idempotency i weryfikację.
6. Zapisz kryteria sukcesu, błąd oczekiwany, limity czasu/kroków/powtórzeń i wymagane artefakty audytu.
7. Zaplanuj zmianę kontraktu oraz test przed implementacją adaptera.

Jeżeli zmiana wymaga dowolnego egress, surowego JavaScriptu/CDP, trwałego storage state, wyłączenia sandboxa Chromium albo publicznej ekspozycji sterowania, zatrzymaj pracę i poproś o odrębną decyzję bezpieczeństwa.

### 2. Zmień kontrakt i port

1. Jeżeli kształt przekracza proces, zmień wersjonowany typ w `Praxiara.Contracts`; abstrahowane zachowanie umieść w `Praxiara.Browser`.
2. Używaj małych, typowanych argumentów i zamkniętego katalogu nazw narzędzi. Każdy argument ma limit, format i regułę walidacji.
3. Oddziel obserwację od lokatora runtime. Kontrakt przenosi krótkotrwały `ref`, nie CSS/XPath ani uchwyt Playwright.
4. Zachowaj `expectedPageRevision` w każdej akcji zależnej od elementu oraz jawny rezultat stale observation.
5. Nie ujawniaj w payloadzie cookies, nagłówków auth, storage state, pełnego DOM ani ścieżek hosta.
6. Jeżeli zmiana jest breaking, zaplanuj kompatybilność Orchestrator–Worker N/N-1 i bezpieczną kolejność rollout.

### 3. Zaimplementuj pojedynczą akcję

1. Wykonaj deterministyczną walidację schematu, permission, domeny, rewizji, limitów i lease przed dotknięciem przeglądarki.
2. Rozwiąż element według kolejności: rola/nazwa ARIA, label, tekst, stabilny atrybut, relacja semantyczna, CSS, wizja, a współrzędne tylko jako audytowany fallback.
3. Korzystaj z auto-waiting Playwright i czekaj na konkretny stan. Nie używaj arbitralnego `WaitForTimeoutAsync` ani `Task.Delay` do gotowości strony.
4. Rejestruj oczekiwanie na popup, download, dialog lub nawigację przed akcją, która może wywołać zdarzenie.
5. Po akcji zbuduj nową obserwację i zweryfikuj efekt techniczny oraz biznesowy. Kliknięcie bez weryfikacji nie jest sukcesem.
6. Zapisz wynik, correlation IDs i bezpieczne metadane artefaktów. Zredaguj sekrety przed logiem, trace i screenshotem.
7. Zwolnij page, context, browser i zasoby tymczasowe deterministycznie również po anulowaniu oraz wyjątku.

### 4. Zachowaj policy i sterowanie człowieka

1. Browser Worker wykonuje wyłącznie akcję zatwierdzoną przez zewnętrzny Policy Engine; nie ufa klasyfikacji LLM.
2. R4 zawsze wymaga konkretnego approval, a R5 silnego potwierdzenia lub reauthentication. Zmiana argumentu, rekordu, środowiska, skill version albo revision unieważnia zgodę.
3. Użytkownik i agent nie sterują jednocześnie. Takeover używa audytowanego lease z właścicielem, TTL i jednoznacznym przekazaniem.
4. Login i MFA wykonuje użytkownik. Poświadczenia i stan uwierzytelnienia nie trafiają do promptu ani telemetrii.
5. Egress jest blokowany również sieciowo; kontrola w promptach i aplikacji nie zastępuje firewall/allowlisty Workera.

### 5. Udowodnij zmianę

1. W `Praxiara.Browser.Tests` przetestuj port, lifecycle, locatory, stale refs, timeout, cleanup i właściwe zdarzenie Playwright.
2. W `Praxiara.ContractTests` dodaj round-trip oraz kompatybilność komunikatu Worker–Orchestrator.
3. W `Praxiara.SecurityTests` dodaj prompt injection, egress, arbitrary path/tool, approval hash lub tenant/session isolation odpowiednio do zmiany.
4. Używaj `Praxiara.TestSites`; testy nie mogą zależeć od Internetu ani produkcyjnej witryny.
5. Dla Playwright stosuj web-first assertions, deterministyczne fixtures, trace przy błędzie i bez sztywnych sleepów.

```bash
dotnet test tests/Praxiara.Browser.Tests/Praxiara.Browser.Tests.csproj
dotnet test tests/Praxiara.ContractTests/Praxiara.ContractTests.csproj
dotnet test tests/Praxiara.SecurityTests/Praxiara.SecurityTests.csproj
dotnet build Praxiara.slnx --no-restore
```

### 6. Zakończ pracę

- Uruchom formatowanie, sprawdź pełny diff i potwierdź jeden top-level type na plik.
- Sprawdź, czy żadna nowa ścieżka nie omija policy, revision, domain allowlist, approval, weryfikacji, audytu ani cleanup.
- Zaktualizuj polski Markdown, jeżeli zmienił się lifecycle, deployment, wymagania operatora lub granica zaufania.
- Raportuj zmianę kontraktu, model zagrożeń, testy, wersję browser/Playwright i rzeczywiste ograniczenia.

Nie deklaruj niezawodności na podstawie jednego przejścia happy path. Zmiana Browser Workera wymaga dowodu dla błędu, anulowania, nieaktualnej strony i istotnej granicy bezpieczeństwa.

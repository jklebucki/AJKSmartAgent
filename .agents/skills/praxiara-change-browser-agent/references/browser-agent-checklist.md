# Checklista zmiany Browser Workera

Używaj tej checklisty podczas planowania, implementacji, review i walidacji. Każdy punkt bezpieczeństwa wymaga dowodu w kodzie, konfiguracji lub teście.

## 1. Granice modułów

- [ ] Kontrakt między procesami znajduje się w `Praxiara.Contracts` i jest wersjonowany.
- [ ] Abstrakcja sesji/akcji znajduje się w `Praxiara.Browser` bez zależności od Playwright.
- [ ] `Praxiara.Browser.Playwright` implementuje adapter i nie podejmuje decyzji biznesowych.
- [ ] `Praxiara.Browser.Worker` jest composition root, a nie miejscem logiki domenowej lub policy.
- [ ] Orchestrator prowadzi pętlę, limity, recovery i oczekiwanie na approval.
- [ ] Policy Engine klasyfikuje narzędzie niezależnie od werdyktu LLM.
- [ ] Każdy zmieniany top-level type ma osobny plik o zgodnej nazwie.
- [ ] Kod, nazwy narzędzi, komentarze, logi i testy są angielskie.

## 2. Katalog narzędzi

- [ ] Narzędzie ma trwałą angielską nazwę, jawny schemat argumentów i wersję.
- [ ] Argumenty mają limity długości, rozmiaru, formatu i dozwolonych wartości.
- [ ] Nie można utworzyć nowego narzędzia ani rozszerzyć allowlisty przez model lub stronę.
- [ ] Nie udostępniono odpowiednika `execute_javascript`, `execute_shell`, `http_request(anyUrl)`, `read_file(anyPath)`, `write_file(anyPath)` ani `set_cookie`.
- [ ] Model nie otrzymuje `IPage`, `IBrowserContext`, CDP session ani WebSocket Playwright.
- [ ] Każde narzędzie ma permission i klasyfikację R0–R5 poza promptem.
- [ ] R3–R5 mają idempotency strategy; R4/R5 mają właściwy approval.
- [ ] Powtórzenie identycznej akcji podlega limitowi i wykrywaniu pętli.

## 3. Obserwacja i referencje

- [ ] Obserwacja jest ograniczona do danych potrzebnych do następnej decyzji.
- [ ] DOM, ARIA, screenshot, komunikat, plik i odpowiedź API są oznaczone jako niezaufane dane.
- [ ] Każdy element ma krótkotrwały `ref`, a registry nie ujawnia lokatora runtime.
- [ ] Registry jest atomowo zastępowane przy nowej revision.
- [ ] Akcja zależna od elementu zawiera `expectedPageRevision`.
- [ ] Mismatch revision kończy się wynikiem stale i nową obserwacją, nigdy retry starego locatora.
- [ ] Obserwacja nie zawiera cookies, auth headers, storage state, sekretów ani pełnego HTML bez jawnej potrzeby.
- [ ] Rozmiar observation i screenshotu jest ograniczony, a nadmiar ma deterministyczną strategię redukcji.

## 4. Locatory i Playwright

- [ ] Locator preferuje kolejno ARIA role/name, label, tekst, stabilny atrybut, relację semantyczną i dopiero CSS.
- [ ] XPath i selektory zależne od struktury DOM nie są normalną ścieżką.
- [ ] Kliknięcie współrzędnymi jest ostatecznym fallbackiem, ma screenshot, powód i audyt.
- [ ] Strict locator resolution odrzuca niejednoznaczny element zamiast wybierać pierwszy.
- [ ] Kod korzysta z auto-waiting i web-first assertions.
- [ ] Nie użyto arbitralnego `WaitForTimeoutAsync`, `Thread.Sleep` ani `Task.Delay` jako sygnału gotowości strony.
- [ ] Nie polegano na `networkidle` dla aplikacji z long polling/SignalR; oczekiwanie dotyczy konkretnego stanu.
- [ ] Oczekiwanie na popup, download, dialog lub navigation jest zarejestrowane przed wywołującą akcją.
- [ ] Frame, tab i popup mają jednoznacznego właściciela oraz lifecycle.
- [ ] Timeout ma limit, jawny wynik i respektuje anulowanie operacji nadrzędnej.

## 5. Lifecycle i izolacja

- [ ] Jedna sesja/`BrowserContext` należy do jednego użytkownika i zadania.
- [ ] Context nie jest współdzielony między tenantami ani taskami.
- [ ] Login oraz MFA wykonuje użytkownik, bez przekazywania poświadczeń modelowi.
- [ ] Storage state jest domyślnie efemeryczny.
- [ ] Wyjątek trwałego stanu ma szyfrowanie per użytkownik, krótki TTL, revoke, retencję i osobną decyzję.
- [ ] Page, context, browser, streamy i pliki tymczasowe są zwalniane po sukcesie, błędzie i anulowaniu.
- [ ] Crash Workera nie jest raportowany jako sukces; task przechodzi do kontrolowanego recovery lub `OutcomeUnknown`.
- [ ] Restart nie odtwarza skutkowej akcji bez idempotency i sprawdzenia aktualnego stanu.

## 6. Izolacja procesu i sieć

- [ ] Worker działa non-root, bez Docker socketu i bez dostępu do bazy control plane.
- [ ] Root filesystem jest read-only, a zapisy trafiają tylko do ograniczonych volume/tmpfs.
- [ ] Sandbox Chromium nie został wyłączony jako skrót do uruchomienia kontenera.
- [ ] Egress jest allowlistowany w kodzie oraz warstwie sieciowej.
- [ ] Redirect, DNS rebinding, prywatne zakresy IP i alternatywne schematy URL nie omijają allowlisty.
- [ ] CDP, Playwright WebSocket, VNC/noVNC i stream nie są publicznie wystawione.
- [ ] Wewnętrzny kontrakt Workera jest uwierzytelniony, autoryzowany, limitowany i szyfrowany zgodnie z profilem wdrożenia.
- [ ] Wersje Playwright i Chromium są zgodne, przypięte i sprawdzone pod kątem CVE.

## 7. Pliki, download i upload

- [ ] Model operuje uchwytem pliku z kontrolowanego magazynu, nie ścieżką hosta.
- [ ] Nazwa pliku jest normalizowana i nie pozwala na traversal ani overwrite poza sandboxem.
- [ ] Rozmiar, MIME, rozszerzenie i liczba plików są limitowane.
- [ ] Pobrany plik nie jest wykonywany, otwierany jako kod ani automatycznie publikowany.
- [ ] Upload obejmuje tylko plik jawnie wskazany przez użytkownika lub workflow.
- [ ] Artefakt ma hash, klasyfikację, właściciela, retencję i correlation ID.
- [ ] Logi nie zawierają zawartości ani lokalnej ścieżki pliku, jeśli nie jest to konieczne.

## 8. Takeover, approval i TOCTOU

- [ ] Agent i użytkownik nie sterują jednocześnie.
- [ ] Takeover lease ma właściciela, TTL, epoch/version i audytowane acquire/release.
- [ ] Po zwrocie sterowania agent wykonuje nową obserwację; stare refs i approval są nieważne.
- [ ] Approval pokazuje środowisko, operację, rekord, wartości przed/po, odbiorców i konsekwencję.
- [ ] Approval hash wiąże user, task, tool, args, record, environment, skill version i page revision.
- [ ] Zmiana któregokolwiek elementu unieważnia zgodę.
- [ ] R5 wymaga silnego potwierdzenia lub reauthentication poza modelem.
- [ ] Akcja po approval jest ponownie autoryzowana bezpośrednio przed wykonaniem.

## 9. Weryfikacja, audyt i artefakty

- [ ] Po każdej istotnej akcji powstaje nowa obserwacja.
- [ ] Sukces wymaga jawnej postcondition, nie tylko braku wyjątku.
- [ ] Dla IFS wynik jest sprawdzony w UI oraz, jeśli dostępne, niezależnym API/historii.
- [ ] Audit obejmuje observation hash, tool, zredagowane args, policy, approval, wynik i verification.
- [ ] Trace, screenshot i plik mają hash oraz metadane zamiast surowej zawartości w bazie.
- [ ] Cookies, tokeny, auth headers, pola haseł i dane wrażliwe są redagowane przed logiem/trace.
- [ ] Correlation IDs łączą task, session, action, browser trace i audyt.
- [ ] Błąd zachowuje dowód diagnostyczny bez ujawniania sekretu użytkownikowi.

## 10. Testy i bramka końcowa

- [ ] Testy używają deterministycznych stron w `Praxiara.TestSites`, bez Internetu.
- [ ] Happy path ma test skutku oraz weryfikacji.
- [ ] Stale revision i unknown ref mają jawne testy.
- [ ] Locator ambiguity, element disabled/hidden/detached i timeout mają testy.
- [ ] Popup, iframe, tab, dialog, download i upload są testowane, jeżeli zmiana ich dotyczy.
- [ ] Anulowanie i crash nie pozostawiają procesu, contextu ani pliku.
- [ ] Prompt injection w tekście, atrybucie, tabeli i pliku nie zmienia celu ani tool allowlist.
- [ ] Egress i arbitrary path mają testy negatywne.
- [ ] Approval TOCTOU, takeover race i tenant isolation mają testy odpowiednie do zmiany.
- [ ] Round-trip kontraktu Worker–Orchestrator oraz N/N-1 przechodzi.
- [ ] Targetowane testy, build i formatowanie kończą się bez ostrzeżeń.
- [ ] Diff nie wprowadza dowolnego narzędzia, sekretu, publicznego portu ani wyłączenia sandboxa.

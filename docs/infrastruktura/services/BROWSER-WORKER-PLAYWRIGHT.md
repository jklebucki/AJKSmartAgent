# Browser Worker i Playwright

## Cel i granica zaufania

Browser Worker uruchamia sterowaną przeglądarkę, buduje obserwacje, wykonuje pojedyncze zweryfikowane akcje i zapisuje trace/screenshot/download. Otwiera dowolne strony wskazane przez użytkownika, dlatego przetwarza niezaufany kod, tekst, pliki i odpowiedzi sieciowe.

LLM nie otrzymuje `IPage`, CDP, dowolnego JavaScriptu ani shell. Zwraca wyłącznie ustrukturyzowane żądanie tool call. Orchestrator, Policy Engine i Browser Worker walidują je przed wykonaniem.

## Wybór i licencje

Microsoft.Playwright jest MIT. Binaria Chrome for Testing/Chromium i ich komponenty mają własne notices/licencje, dlatego obraz Browser Workera ma osobny SBOM i third-party notices.

Źródła: [Playwright .NET](https://github.com/microsoft/playwright-dotnet), [Docker guidance](https://playwright.dev/dotnet/docs/docker), [browser contexts](https://playwright.dev/dotnet/docs/browser-contexts), [authentication state](https://playwright.dev/dotnet/docs/auth).

## Wersja i pinning

- NuGet `Microsoft.Playwright` jest przypięty centralnie.
- Browser binary i image muszą odpowiadać tej samej wersji Playwright.
- Nie instalujemy przeglądarki przy każdym starcie.
- Obraz jest budowany raz, skanowany i przypięty po digest.
- Systemowy Chromium/Chrome jest dozwolony tylko po compatibility suite; `ExecutablePath` nie jest przypadkowym obejściem.

## Kontrakty

### Observation

Minimalnie:

```json
{
  "revision": 184,
  "url": "https://example.invalid/page",
  "title": "Page",
  "elements": [
    {
      "ref": "e17",
      "role": "textbox",
      "name": "Order No",
      "enabled": true,
      "visible": true
    }
  ],
  "dialogs": [],
  "messages": [],
  "screenshotId": "object-id"
}
```

`ref` obowiązuje wyłącznie dla jednej `revision`. Akcja ze starą revision jest odrzucana przed dotknięciem strony.

### Action

```json
{
  "tool": "browser_fill",
  "expectedRevision": 184,
  "arguments": {
    "ref": "e17",
    "value": "100245"
  },
  "reason": "Fill the user-provided order number"
}
```

Lista narzędzi jest allowlistą. Brak `execute_javascript`, `execute_shell`, `http_request_any`, `read_file_any` i `set_cookie`.

## Rozpoznawanie elementów

Kolejność:

1. role ARIA i accessible name;
2. label;
3. widoczny tekst;
4. stabilny test/application attribute;
5. relacja formularz/tabela;
6. stabilny CSS jako fallback;
7. screenshot i model wizyjny;
8. współrzędne tylko ostatecznie, z approval przy operacji ryzykownej.

Xpath i selektory oparte na przypadkowej strukturze DOM nie są domyślne.

## Topologie

### Development

- jeden długotrwały worker, osobny BrowserContext na test/session;
- headed Chromium pod X11 dla przejęcia;
- noVNC fallback;
- lokalne trace i krótka retencja;
- dozwolony szerszy debugging, ale nadal brak Docker socket.

### Hardened single-node

- coordinator jest zaufaną usługą;
- efemeryczny worker per session/task lub per wysokie ryzyko;
- worker uruchomiony non-root z Chromium sandbox;
- root filesystem read-only, tmpfs, quota;
- worker wyłącznie w `browser-egress-net` i `stream-net`;
- sterowanie przez wąski, uwierzytelniony channel;
- automatyczne zniszczenie po TTL/anulowaniu;
- state/artifacts szyfrowane i poza lokalnym filesystemem po zakończeniu.

### Skala/HA

- scheduler przydziela worker do puli według tenant, region, browser capability i ryzyka;
- host failure kończy bieżącą interakcję, ale Temporal wznawia workflow z ostatniego bezpiecznego punktu;
- sesja browsera nie jest transparentnie migrowana między hostami;
- manual takeover łączy się tylko z konkretnym owner/session;
- limity per tenant zapobiegają wyczerpaniu puli.

### IFS Aurena Agent

Serwerowy Linux Chromium nie obsłuży funkcji wymagających lokalnego Windows Aurena Agent, drukarki lub lokalnej aplikacji użytkownika. Dla takich skill potrzebna jest osobna pula Windows interactive workers albo sterowanie prawdziwym Chrome/Edge na stacji użytkownika. Environment profile musi oznaczyć tę zależność; system nie może udawać wsparcia w zwykłym kontenerze.

## Zasoby startowe

Na jedną aktywną sesję:

| Zasób | Start |
|---|---:|
| CPU | 1–2 vCPU |
| RAM | 1–2 GiB, więcej dla ciężkiego IFS/multitab |
| tmpfs | 512 MiB–2 GiB z quota |
| artifact bandwidth | według fps/trace/download |

Concurrency wynika z pomiaru RSS, renderer processes, video encoding i wymagań IFS. Worker ma twardy memory/pids limit oraz graceful termination przed OOM hosta.

## Sekrety i stan sesji

Worker nie otrzymuje hasła użytkownika ani długiego OpenBao tokenu. Użytkownik loguje się ręcznie i obsługuje MFA.

Storage state:

- jest przypisany do user, tenant, site/environment i browser profile;
- jest szyfrowany envelope encryption;
- ciphertext trafia do `browser-sessions`;
- key/lease ma krótki TTL;
- natychmiastowa revocation usuwa dostęp i obiekt;
- nigdy nie trafia do Git, audit body, promptu ani zwykłych logów;
- nie jest współdzielony między użytkownikami.

Konfiguracja workera:

```text
Browser__Mode=Worker
Browser__Headless=false
Browser__SessionTtlMinutes=60
Browser__MaxTabs=5
Browser__MaxDownloadBytes=104857600
Browser__ArtifactsEndpoint=http://artifact-gateway:8080
Browser__EgressPolicyEndpoint=http://browser-policy-gateway:8080
Browser__StreamingProvider=livekit
Browser__PlaywrightTrace=true
```

## Sieci

- Worker nie należy do `data-net`, `control-net` ani `telemetry-net` z wyjątkiem bezpiecznego telemetry gateway.
- Worker łączy się do coordinator/artifact/stream gateway po mTLS lub krótkim signed tokenie.
- Egress blokuje loopback, link-local, metadata i wewnętrzne adresy, chyba że environment profile jawnie pozwala na prywatny IFS.
- DNS rebinding i DoH są blokowane.
- Raw Playwright/CDP endpoint nie jest publikowany.

## Sandbox kontenera

W produkcji:

- non-root browser user;
- aktywny Chromium sandbox;
- brak `--no-sandbox`;
- brak `privileged` i `SYS_ADMIN`;
- profil seccomp kompatybilny z Chromium;
- `cap_drop: [ALL]` z minimalnymi wyjątkami, jeśli test wykaże potrzebę;
- `read_only: true`;
- kontrolowane `tmpfs` dla `/tmp`, profile i downloads;
- `pids_limit`, CPU/RAM i ulimits;
- brak host mounts i socketu Dockera.

Playwright dokumentuje, że root wyłącza Chromium sandbox, a crawling niezaufanych witryn wymaga osobnego użytkownika i seccomp.

## Uruchomienie

Development:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile browser \
  --profile stream-novnc \
  up -d --wait browser-coordinator browser-worker
```

Produkcja uruchamia worker przez bezpieczny scheduler/runtime, nie przez wystawiony Docker API. Jeśli dynamiczne kontenery są wymagane, osobny Worker Manager ma predefiniowane templates i minimalne uprawnienia; API/LLM nie przekazuje mu dowolnych parametrów obrazu, mountów ani command.

## Health i smoke test

Liveness: worker process i browser child istnieją. Readiness: można utworzyć context/page, wykonać nawigację do kontrolnej strony i zamknąć context.

Smoke test:

1. Utwórz sesję testowego użytkownika.
2. Otwórz lokalną stronę fixture.
3. Observe i uzyskaj refs.
4. Fill/click z poprawną revision.
5. Odrzuć action ze starą revision.
6. Wykonaj upload z quarantine fixture.
7. Pobierz plik do quarantine i zeskanuj.
8. Włącz trace, zapisz screenshot do S3.
9. Przejmij ręcznie przez wybrany stream.
10. Zamknij sesję i potwierdź zniszczenie tmpfs/state lease.

Negatywne testy:

```bash
docker compose exec browser-worker sh -lc 'nc -z postgres 5432 && exit 1 || exit 0'
docker compose exec browser-worker sh -lc 'nc -z openbao 8200 && exit 1 || exit 0'
```

## Artefakty, backup i restore

Efemeryczny filesystem workera nie jest backupowany. Artefakty zatwierdzone przez politykę trafiają do S3 z metadanymi w PostgreSQL:

- object ID/key;
- SHA-256;
- media type po detekcji;
- user/tenant/session/task;
- classification/retention;
- encryption key reference;
- created/expires.

Trace/screenshot mają krótką retencję, chyba że stają się audit evidence. Restore oznacza odtworzenie PostgreSQL+S3 zgodnie z ich spójnym punktem, nie odtworzenie żywego browser process.

Storage state restore jest opcjonalny i dozwolony tylko przed TTL/revocation. Jeśli nie można go bezpiecznie odtworzyć, użytkownik loguje się ponownie.

## Aktualizacja i rollback

Playwright NuGet, driver i browser binary są aktualizowane razem.

Testy przed rolloutem:

- wszystkie browser tools;
- ARIA snapshots/refs;
- download/upload/dialog/new tabs;
- trace i screenshot;
- IFS fixture/real sandbox;
- streaming/manual takeover;
- sandbox i negatywna segmentacja sieci;
- memory/leak test.

Rollout canary na małej puli. Aktywne sesje kończą się na starej wersji; nowe trafiają na nową. Rollback kieruje nowe sesje do starego immutable obrazu, bez próby przeniesienia BrowserContext między wersjami.

## Hardening aplikacyjny

- observation zawiera tylko potrzebne elementy, nie pełny raw DOM.
- page text jest oznaczany jako untrusted.
- revision check przed każdą akcją.
- locator resolve wyłącznie z registry workera.
- wartości input mają schema, długość i data classification.
- upload/download tylko przez manager i skan.
- navigation i popup podlegają domain policy.
- clipboard, geolocation, camera, microphone, notifications i permissions default-deny.
- downloads nie są automatycznie otwierane.
- dialogi destructive wymagają policy/approval.
- każda akcja ma timeout, cancellation i verification.
- trace/redaction nie zapisuje cookies/authorization bez konieczności i zgody polityki.

## Troubleshooting

### Browser crash/OOM

Zabezpiecz zredagowane logi, trace ID i metryki. Nie restartuj w pętli bez limitu. Temporal klasyfikuje błąd, zamyka starą sesję i może utworzyć nową po potwierdzeniu użytkownika/logowaniu.

### Stale reference

Oczekiwane zabezpieczenie. Wykonaj ponowne Observe i plan. Nie rozwiązuj przez wyszukanie elementu o tym samym indeksie.

### Element niewidoczny

Sprawdź iframe, shadow DOM, scroll, overlay, locale i accessibility tree. Coordinate click jest ostatnią opcją, nie pierwszą.

### IFS działa ręcznie, nie w workerze

Sprawdź wymaganie Aurena Agent, extension, Windows, client certificate, SSO, WebAuthn i network profile. Nie wyłączaj security feature strony.

### Trace zawiera dane poufne

Zatrzymaj udostępnianie, skróć retencję, usuń zgodnie z procedurą, przeanalizuj redaction i klasyfikację. Playwright trace jest materiałem wrażliwym.

## Różnice produkcyjne

Dev może współdzielić proces i używać noVNC. Produkcja używa non-root sandboxed efemerycznych workers, kontrolowanego egressu, S3, TTL/revocation i WebRTC. Pula Windows dla Aurena Agent jest odrębną topologią i nie jest zastępowana przez Linux container.

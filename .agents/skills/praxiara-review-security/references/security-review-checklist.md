# Checklista odczytowego audytu bezpieczeństwa

Ta checklista służy do wykazania osiągalnych ścieżek nadużycia. Nie jest listą gwarancji zgodności ani zgodą na aktywny pentest.

## 1. Zakres i dowody

Przed audytem zapisz:

- commit, diff albo ścieżki objęte zakresem;
- właściciela systemu i jawne upoważnienie;
- dozwolone środowiska i czynności;
- wyłączone dane, tenanty i systemy;
- dostępne diagramy, ADR, schema i wyniki testów;
- ograniczenia widoczności kodu i konfiguracji.

Preferuj dowody z kodu, testu, schema i konfiguracji. Dokument planu opisuje intencję, ale nie dowodzi wdrożenia kontrolki.

## 2. Aktywa i strefy zaufania

Zmapuj:

- cele użytkownika i dane biznesowe IFS;
- credentials, cookies, browser storage i tokeny delegowane;
- prompt, tool catalog i model output;
- approvals, action hashes i policy versions;
- audit events, screenshots, traces, uploads i downloads;
- PostgreSQL, object storage, secrets provider i telemetry;
- public UI, control plane, execution, model, business system, evidence i administration zone.

Dla każdego przejścia sprawdź identity, auth, schema, limit, egress i audit.

## 3. Uwierzytelnienie i autoryzacja

- OIDC waliduje issuer, audience, signature, expiry i wymagane claims.
- OAuth używa właściwego flow, PKCE i krótkiego TTL.
- API nie ufa roli, tenantowi ani approval przesłanemu wyłącznie przez UI.
- Każda komenda ma server-side authorization.
- Service identity ma minimalny scope i nie zastępuje użytkownika bez zatwierdzonej delegacji.
- Odebranie roli lub sesji jest sprawdzane przed kolejną akcją.
- R5 i wskazane R4 wymuszają step-up/four-eyes.
- Break-glass ma ownera, TTL, uzasadnienie i pełny audit.
- Nie ma fail-open po błędzie identity providera lub policy.

## 4. Tenant isolation

- Tenant wynika z zaufanego principal, nie z dowolnego parametru requestu.
- Klucze cache, lease, storage paths, database rows i eventy są tenant-scoped.
- Query oraz object access mają negatywne testy cross-tenant.
- Background jobs i replay zachowują tenant context.
- Audit export nie łączy danych tenantów.
- Identyfikator zasobu nie wystarcza bez ponownej autoryzacji tenant/user.

Cross-tenant read lub write klasyfikuj co najmniej jako `Critical`, jeśli ścieżka jest osiągalna.

## 5. LLM i prompt injection

- Trusted instructions są strukturalnie oddzielone od DOM, ARIA, pliku, obrazu, e-maila i danych IFS.
- Model otrzymuje tylko narzędzia dozwolone dla użytkownika, skill i kroku.
- Structured output odrzuca nieznane narzędzia, dodatkowe pola, zły typ i nadmierny rozmiar.
- Model jest proposerem; backend wykonuje schema validation i policy.
- Dane strony nie rozszerzają celu, allowlisty, permissions ani pamięci długoterminowej.
- Direct, indirect, encoded, multilingual i multimodal injections mają testy.
- Denial jest security event, nie sygnałem do automatycznego szukania obejścia.
- Prompt, model i tool catalog mają wersję i audit bez ujawnienia danych Restricted.

Prześledź konkretny sink. Sam napis „ignore previous instructions” w fixture nie dowodzi podatności.

## 6. Narzędzia i wykonanie

- Katalog jest mały, wersjonowany i statyczny dla kroku.
- Każde narzędzie ma schema z zamkniętym zbiorem argumentów, risk, permission, consequence i verifier.
- Brak `execute_javascript`, `execute_shell`, arbitrary HTTP, arbitrary file path i `set_cookie` dla modelu.
- Wewnętrzny JavaScript jest statycznym, reviewed asset, nie model output.
- Unknown tool i unknown argument kończą się deny.
- Timeout, retry, step/resource limits i loop detection są egzekwowane poza modelem.
- Executor nie obchodzi policy przez alternatywną ścieżkę wywołania.

## 7. Egress i SSRF

- Allowlista używa dokładnego scheme, hosta i portu; wildcard ma jawne uzasadnienie.
- Redirect, iframe, popup, WebSocket, WebRTC, service worker i next link są ponownie walidowane.
- IDN jest normalizowane do punycode przed porównaniem.
- Blokowane są loopback, link-local, metadata endpoints i prywatne zakresy poza profilem.
- DNS rebinding jest ograniczane przez resolver/proxy policy i rewalidację.
- Browser Worker ma sieciowy egress control niezależny od promptu.
- CDN i SSO są jawnie przypisane do profilu witryny.
- Błąd egress nie uruchamia niekontrolowanego fallbacku.

## 8. Browser Worker i sesja

- Sesja lub `BrowserContext` jest izolowany per user/task.
- Worker działa non-root, bez Docker socket, z read-only root filesystem i minimalnymi capabilities.
- Profil, download i upload są efemeryczne, tenant-scoped i czyszczone.
- Cookies, credentials, MFA i storage state nie trafiają do LLM, logów ani audytu.
- Persisted state jest wyjątkiem z encryption, per-user key, TTL i revoke.
- Element reference jest związany z jedną `pageRevision`.
- Takeover używa wyłącznego lease, fencing token i reobserve po zwrocie.
- Browser crash nie może zostać przedstawiony jako sukces.
- Chromium i Playwright mają kontrolowany patch cycle.

## 9. Policy, approval i TOCTOU

- Risk pochodzi z katalogu/policy, nie z modelu.
- Brak reguły oraz wyjątek Policy Engine oznacza deny.
- R4 wymaga konkretnego approval; R5 wymaga silnego approval i ewentualnie four-eyes.
- Preview pokazuje environment, target, wartości przed/po, odbiorcę i skutek.
- Action hash używa kanonicznej reprezentacji i obejmuje wszystkie bound facts.
- Przed wykonaniem system wykonuje reobserve/API read i ponowną autoryzację.
- Zmiana targetu, argumentu, revision, environment, skill, policy lub permission unieważnia approval.
- Approval jest jednorazowy, ma nonce/expiry i nie działa w replay.
- Consumed state jest atomowo związany z konkretną execution attempt.
- Awaria zapisu audytu blokuje R4/R5.

Minimalne bound facts:

```text
tenantId
userId
taskId
sessionId
toolCallId
toolName
canonicalArguments
targetDomain
targetEntityIdentity
environmentId
pageRevision
observationHash
skillId + skillVersion
policyVersion
toolCatalogVersion
issuedAt + expiresAt
nonce
```

## 10. Idempotencja i wynik

- Każde narzędzie deklaruje idempotency semantics.
- Retry ma klasyfikację błędu, limit, backoff z jitterem i ten sam idempotency key.
- Nonidempotent timeout po wysłaniu prowadzi do `OutcomeUnknown`, nie retry.
- Verifier sprawdza semantyczny skutek, nie tylko status HTTP lub komunikat UI.
- UI i niezależny API/history read są łączone dla krytycznych operacji IFS.
- Recovery nie używa starego approval ani tokenu.
- Rekoncyliacja jest audytowana i nie ukrywa niepewności.

## 11. IFS

- Projection/API jest sprawdzane przed użyciem UI.
- Projection, entity, action i query options są w zatwierdzonym registry.
- Model widzi wysokopoziomowe narzędzie biznesowe, nie surowy OData.
- Token zachowuje uprawnienia użytkownika; service account nie rozszerza ich niejawnie.
- `403`, expired session, schema conflict i business error mają różne wyniki.
- Trasa `ProjectionApi`, `Browser` lub `Hybrid` ma deterministyczne uzasadnienie.
- Release update, locale i customizations mają compatibility tests.
- Aurena Agent wymaga jawnego Windows companion i obecności użytkownika.
- Brak companion nie uruchamia arbitrary shell ani lokalnej ścieżki.

## 12. Pliki, dane i sekrety

- Upload pochodzi wyłącznie z zatwierdzonego artefaktu, nie ścieżki hosta od modelu.
- Nazwa pliku jest normalizowana; path traversal i archive traversal są testowane.
- Download nie jest automatycznie wykonywany ani otwierany.
- Rozmiar, content type, checksum, TTL i tenant są walidowane.
- Malware scan oraz quarantine są zgodne z polityką.
- Redaction działa przed logiem, spanem, promptem i trwałym audytem.
- Screenshot/traces mają masking, classification i retention.
- Secrets provider nie jest dostępny dla Browser Workera ani modelu.
- Logowanie używa allowlisty pól, nie blacklisty.

## 13. Audyt i evidence

- Audit jest append-only i ma correlation IDs oraz monotoniczną sekwencję task.
- Payload ma kanoniczny hash połączony z poprzednim eventem.
- Artefakt ma checksum, content type, classification i retention policy ID.
- Denial, failure, `OutcomeUnknown`, break-glass i security signal są audytowane.
- Dostęp, eksport i usunięcie artefaktu również tworzą event.
- Replay domyślnie jest offline i nie wykonuje skutku.
- Stary approval, cookie lub token nie działa w replay.
- Użytkownik widzi zredagowaną timeline, audytor tylko autoryzowany widok pełny.

## 14. Supply chain i deployment

- NuGet, npm, obrazy i modele mają przypiętą wersję/digest oraz dozwoloną licencję.
- Lockfiles, central package management, SBOM i provenance są aktualne.
- Brak `latest`, nieznanej licencji i sekretu w obrazie.
- Publicznie nie są wystawione CDP, Playwright WebSocket, VNC, database, cache, secrets ani operators UI.
- Health/readiness nie ujawniają sekretu i nie powodują kosztownego skutku.
- Critical CVE Chromium/Playwright/runtime ma patch SLA.
- Konfiguracja produkcyjna nie osłabia sandboxu „tymczasowym” wyjątkiem.

## 15. Jakość implementacji kontrolki

- Jeden główny typ top-level znajduje się w jednym, tak samo nazwanym pliku.
- Policy, executor, verifier i audit mają odrębne odpowiedzialności.
- Interfejs istnieje przy granicy lub realnej wymienności, nie dla każdej klasy.
- Nie ma refleksyjnego dispatchu, service locatora ani globalnego mutable state.
- Walidacja granicy nie jest jedynym zabezpieczeniem przed skutkiem.
- Async I/O przekazuje `CancellationToken`; czas pochodzi z `TimeProvider`.
- Security-sensitive porównania i canonicalization mają testy graniczne.
- Komentarz nie zastępuje deterministycznej kontrolki.

Naruszenie SOLID, DRY, KISS lub zasady pliku raportuj jako security finding tylko przy wykazanym wpływie na kontrolkę.

## 16. Minimalny zestaw negatywnych testów

- prompt injection próbuje rozszerzyć cel i katalog narzędzi;
- unknown tool/argument i oversized payload;
- redirect do obcego hosta, IDN spoof, DNS rebinding i metadata endpoint;
- stale revision i zmiana rekordu po approval;
- reuse/expiry approval oraz równoległa execution attempt;
- cross-tenant ID, cache key, object path i background job;
- timeout po nonidempotent commit;
- token/cookie/PII w prompt, log, trace, screenshot i audit;
- złośliwa nazwa upload/download i niewłaściwy content type;
- brak audit sink przed R4/R5;
- browser crash, verifier unavailable i model timeout;
- brak Aurena companion oraz próba unsafe fallback.

## 17. Format ustalenia

```text
[Severity][Confidence] Concise title
Location: path/to/File.cs:line
Preconditions: attacker capability and required state
Path: source -> missing/weak guard -> sink
Impact: concrete confidentiality/integrity/availability/audit consequence
Existing controls: controls inspected and why they do not close the path
Remediation direction: minimal design direction, without implementation
Regression test: deterministic test that would fail before the fix
```

Po ustaleniach zawsze dodaj:

- zakres i commit/diff;
- wykonane wyłącznie odczytowe czynności;
- ograniczenia oraz niezweryfikowane założenia;
- obszary bez potwierdzonych problemów, jeśli jest to istotne;
- jednoznaczne zastrzeżenie, że audyt nie jest gwarancją braku podatności.

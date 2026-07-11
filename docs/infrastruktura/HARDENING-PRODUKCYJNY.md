# Hardening produkcyjny

## Zakres

Dokument jest checklistą dla hardened single-node i bazą dla multi-node. Nie zastępuje threat modelu ani przeglądu bezpieczeństwa.

## Host

- Wspierany Linux z automatycznymi poprawkami security i kontrolowanym restartem.
- Docker Engine/Moby lub Podman; Docker Desktop nie jest runtime produkcyjnym.
- Oddzielna partycja/wolumen na dane kontenerów i backup spool.
- Szyfrowanie dysku, chroniony boot i bezpieczne przechowywanie kluczy.
- Synchronizacja czasu z co najmniej dwoma źródłami; alert na drift.
- Firewall default-deny, otwarte tylko 80/443 i jawne porty WebRTC/TURN/administracyjne z dozwolonych sieci.
- Administracja wyłącznie przez VPN/bastion, klucze SSH, bez logowania hasłem i bez współdzielonych kont.
- Audit systemowy oraz wysyłka logów poza host.
- Limity deskryptorów, procesów, pamięci i miejsca na dysku monitorowane przed wyczerpaniem.

## Runtime kontenerów

Dla każdej usługi, jeśli jej obraz na to pozwala:

```yaml
read_only: true
security_opt:
  - no-new-privileges:true
cap_drop:
  - ALL
tmpfs:
  - /tmp:size=256m,mode=1777
pids_limit: 512
restart: unless-stopped
```

Dodatkowo:

- jawny `user` non-root;
- żadnego `privileged: true`;
- żadnego mountu `/var/run/docker.sock`;
- brak host network i host PID namespace;
- brak bind mountu całego repo w produkcji;
- volumes mają minimalny zakres i właściwego właściciela;
- ustawione CPU/memory reservations i limits;
- healthcheck nie ujawnia sekretu w `docker inspect` ani process list;
- image digest i SBOM są znane przed startem.

## Browser Worker

Browser Worker wymaga ostrzejszych reguł niż zwykła usługa:

- jeden efemeryczny worker lub co najmniej BrowserContext na izolowaną sesję;
- preferowany osobny kontener dla zadań o wysokim ryzyku;
- uruchomienie jako non-root z aktywnym Chromium sandbox;
- `--no-sandbox` jest zabronione w produkcji;
- `SYS_ADMIN` jest zabronione; ewentualny wyjątek tylko lokalnie i udokumentowany;
- profil seccomp zgodny z Playwright/Chromium;
- root filesystem read-only, mały tmpfs i quota pobrań;
- brak dostępu do data/control networks;
- kontrolowany egress, DNS i blokada SSRF;
- upload tylko z quarantine store, download do quarantine store;
- automatyczne zamknięcie po TTL, anulowaniu, utracie właściciela lub naruszeniu polityki;
- cookies/storage state zaszyfrowane per-user, krótki TTL, natychmiastowa revocation;
- raw CDP/Playwright endpoint nigdy nie jest dostępny dla klienta ani Internetu.

## Identity i sesje

- OIDC Authorization Code + PKCE.
- Dla web UI preferowany BFF i cookie `HttpOnly`, `Secure`, `SameSite` dobrane do przepływu.
- Krótkie access tokeny, rotowane refresh tokeny, logout i revocation testowane.
- MFA/passkeys dla operatorów i działań wysokiego ryzyka.
- Role aplikacji i role operatorskie są rozdzielone.
- Konto bootstrap administratora Keycloak jest usuwane lub wyłączane po inicjalizacji.
- IFS credentials i MFA nie są przekazywane do LLM ani przechowywane w Keycloak.
- Użytkownik loguje się do IFS w izolowanej sesji browsera; agent przejmuje wyłącznie już uwierzytelnioną sesję.

## Sekrety

- Produkcja używa OpenBao i krótkich lease, gdzie to możliwe.
- Bootstrap jest szyfrowany SOPS+age i odszyfrowywany tylko na docelowym hoście.
- Root/recovery keys OpenBao są rozdzielone między opiekunów i poza hostem.
- Sekrety nie są logowane, zwracane przez health endpoints ani dodawane do telemetry attributes.
- Rotacja ma procedurę bez przestoju i test rollbacku.
- Po podejrzeniu wycieku sekret jest rotowany; samo usunięcie z Git nie wystarcza.

## Dane

- TLS w ruchu między hostami, szyfrowanie dysku i opcjonalne szyfrowanie obiektów aplikacyjnym envelope encryption.
- Każda klasa danych ma retencję i właściciela.
- Screenshot, trace i download mogą zawierać dane osobowe lub poufne; domyślnie krótsza retencja.
- Audit log przechowuje zredagowane argumenty, identyfikatory, hash artefaktów i decyzję approval.
- Telemetria nie zastępuje audytu.
- Backup jest szyfrowany i przechowywany w innej failure domain.
- Restore drill odbywa się cyklicznie i ma zmierzone RPO/RTO.

## LLM i prompt injection

- Zawartość strony jest oznaczana jako niezaufane dane, nigdy instrukcja systemowa.
- LLM nie otrzymuje shell, arbitrary JavaScript, arbitrary HTTP ani raw filesystem tools.
- Tool call jest walidowany schematem, revision strony, polityką, rolą użytkownika i limitem domeny.
- Operacja skutkowa wymaga niezależnej klasyfikacji ryzyka i w razie potrzeby approval.
- Approval pokazuje rekord, środowisko, zmianę i odbiorcę, nie tylko pytanie „kontynuować?”.
- Model nie może rozszerzyć własnego tool registry, allowlisty ani uprawnień.
- Dane ze strony nie trafiają automatycznie do pamięci długoterminowej.
- Provider cloud jest wyłączony domyślnie; jego DPA, retencja, region i koszt wymagają konfiguracji operatora.

## Reverse proxy i API

- Caddy ustawia HSTS po potwierdzeniu poprawnego TLS, CSP, `X-Content-Type-Options`, `Referrer-Policy` i odpowiednie `Permissions-Policy`.
- CORS ma jawne origins; `*` nie jest używane z credentials.
- Limity body, request rate, połączeń i WebSocket messages.
- API weryfikuje issuer, audience, signature, expiry i wymagane role.
- Problem details nie zawierają stack trace ani sekretów.
- Endpointy management są na osobnym porcie/sieci i nie są routowane publicznie.
- Presigned URL ma krótki TTL, konkretny bucket/key, metodę i limit rozmiaru.

## Telemetria

- OTel Collector ma `memory_limiter`, `batch`, retry/queue i limity rozmiaru.
- Redaction usuwa authorization, cookie, set-cookie, tokeny, prompt body i wartości pól.
- Logi aplikacji są strukturalne, bez PII domyślnie.
- Sampling nie może usuwać całego śladu operacji skutkowej; audit pozostaje pełny.
- OpenSearch ma retencję, rollover, quota i alerty na flood/cardinality.
- UI observability jest chronione oauth2-proxy/VPN.

## Supply chain

- Obrazy tylko po digestach.
- Trivy blokuje niezaakceptowane Critical/High według polityki z wyjątkiem VEX z terminem.
- Syft generuje CycloneDX i SPDX.
- Gitleaks CLI skanuje pełną historię.
- ZAP działa na staging i ma bezpieczny scope.
- Build używa minimalnych permissions i krótkich credentials.
- Artefakty są podpisane, provenance i wynik testów są archiwizowane.

## Backup i disaster recovery

Minimalnie:

- PostgreSQL: pgBackRest full/diff/incr + ciągły WAL, okresowy restore/PITR;
- OpenBao: zaszyfrowany Raft snapshot i osobny backup konfiguracji;
- SeaweedFS: replikacja/backup do drugiego systemu, nie do tego samego wolumenu;
- OpenSearch: snapshot do oddzielnego S3, jeśli telemetria wymaga odtworzenia;
- Keycloak i Temporal: objęte backupem ich dedykowanych baz;
- konfiguracja Compose i lockfiles z repozytorium;
- recovery keys poza systemem, którego odtworzenie od nich zależy.

## Checklista przed produkcją

- [ ] Threat model zatwierdzony.
- [ ] Wszystkie obrazy przypięte po digestach i przeskanowane.
- [ ] Rejestr licencji i modeli kompletny.
- [ ] Żaden sekret nie znajduje się w repo ani `docker inspect`.
- [ ] Publiczne są wyłącznie oczekiwane porty.
- [ ] Browser Worker nie ma dostępu do data/control.
- [ ] Chromium sandbox działa bez `SYS_ADMIN` i `--no-sandbox`.
- [ ] MFA i approval dla działań wysokiego ryzyka działają end-to-end.
- [ ] Backup i restore/PITR zostały wykonane na czystym środowisku.
- [ ] Monitoring obejmuje pojemność, błędy, certyfikaty, lease i kolejki.
- [ ] Runbook incydentu browser session i wycieku sekretu został przećwiczony.
- [ ] RPO/RTO oraz retencje mają właścicieli.
- [ ] Plan upgrade/rollback został przetestowany.

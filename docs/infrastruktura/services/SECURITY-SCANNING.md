# Skanowanie bezpieczeństwa, SBOM i zgodność licencji

## Cel i zakres

Pipeline bezpieczeństwa wykrywa:

- podatności zależności i pakietów OS;
- sekrety w bieżących plikach i historii Git;
- błędne konfiguracje Dockerfile/Compose/IaC;
- niedozwolone licencje;
- skład obrazu i release SBOM;
- podstawowe podatności działającej aplikacji przez DAST.

Skaner jest kontrolą, nie dowodem braku podatności. Threat modeling, review kodu, testy authorization i manual penetration testing pozostają wymagane proporcjonalnie do ryzyka.

## Wybór i licencje

| Narzędzie | Licencja | Użycie |
|---|---|---|
| Trivy | Apache-2.0 | CVE, dependency, image, secret, IaC/misconfig, license |
| Syft | Apache-2.0 | CycloneDX/SPDX SBOM |
| Gitleaks CLI | MIT | pełna historia Git i pre-commit |
| OWASP ZAP | Apache-2.0 | baseline i authenticated DAST |

Nie używamy `gitleaks-action` v2 jako domyślnej integracji, ponieważ ma oddzielne warunki i wymaga klucza w części zastosowań organizacyjnych. Uruchamiamy CLI bezpośrednio.

Źródła: [Trivy](https://github.com/aquasecurity/trivy), [Syft](https://github.com/anchore/syft), [Gitleaks CLI](https://github.com/gitleaks/gitleaks), [Gitleaks Action](https://github.com/gitleaks/gitleaks-action), [OWASP ZAP](https://github.com/zaproxy/zaproxy).

## Wersja i pinning

- CLI/container images przypięte po digestach.
- Vulnerability databases są aktualizowane w kontrolowanym kroku z logiem timestamp/version.
- Dla hermetycznego CI baza jest mirrorowana/cache’owana z określonym maksymalnym wiekiem.
- Scanner image samo przechodzi Trivy/SBOM.
- Konfiguracje ignore/policy są wersjonowane i reviewowane.
- Canary/nightly scanner builds są zabronione w release gate.

## Profile i topologie

### Lokalnie

Profil `security` zawiera one-shot services. Repo jest montowane read-only, poza katalogiem raportów.

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile security \
  run --rm trivy-fs
```

### CI

- osobny, nieuprzywilejowany runner;
- read-only checkout tam, gdzie możliwe;
- minimalne token permissions;
- cache baz bez współdzielenia sekretów;
- raporty SARIF/CycloneDX/SPDX jako artefakty;
- release gate blokuje według polityki;
- pełne skany okresowe niezależnie od incremental PR scan.

### Produkcja

Skanery nie działają stale w aplikacyjnym control/data plane. Obrazy są skanowane przed promocją, registry może wykonywać ponowne skany, a ZAP działa wyłącznie przeciw staging lub jawnie zatwierdzonemu scope. Nie uruchamiamy aktywnego DAST przeciw produkcji bez change window i zgody.

## Zasoby startowe

| Narzędzie | CPU | RAM | Uwagi |
|---|---:|---:|---|
| Trivy | 1–2 | 1–2 GiB | więcej dla dużych obrazów/repo |
| Syft | 1–2 | 1–2 GiB | jeden obraz na proces |
| Gitleaks | 1 | 256–512 MiB | zależy od historii Git |
| ZAP baseline | 2 | 2–4 GiB | pełny scan może wymagać więcej |

Skan ma timeout, ale timeout nie jest wynikiem pozytywnym. Pipeline raportuje `incomplete`, nie `passed`.

## Macierz kontroli

| Etap | Kontrole |
|---|---|
| pre-commit | format/lint, Gitleaks changed files |
| pull request | testy, Trivy fs/misconfig/secret/license, Gitleaks history, SBOM preview |
| image build | Syft SBOM, Trivy image, base digest, non-root config |
| integration | authz tests, browser isolation, dependency smoke |
| staging | ZAP baseline, okresowo authenticated/full scan |
| release | policy gate, notices, SBOM, digest, provenance, VEX |
| cyklicznie | rescan released images i dependencies nową bazą CVE |

## Trivy

Repo:

```bash
trivy fs \
  --scanners vuln,misconfig,secret,license \
  --severity HIGH,CRITICAL \
  --exit-code 1 \
  --ignore-unfixed=false \
  .
```

Obraz:

```bash
trivy image \
  --scanners vuln,secret,misconfig,license \
  --severity HIGH,CRITICAL \
  --exit-code 1 \
  "${IMAGE_REFERENCE}@${IMAGE_DIGEST}"
```

SBOM input może być skanowany ponownie bez pobierania obrazu. Polityka musi rozróżniać podatność exploitable, brak fix i komponent niewykonywany, ale wyjątek wymaga VEX/owner/expiry.

## Syft i SBOM

```bash
syft "${IMAGE_REFERENCE}@${IMAGE_DIGEST}" \
  -o cyclonedx-json=artifacts/compliance/sbom.cdx.json \
  -o spdx-json=artifacts/compliance/sbom.spdx.json
```

SBOM zawiera:

- dokładny digest obrazu;
- pakiety OS i application dependencies;
- wersje i licenses;
- purl/CPE, jeśli dostępne;
- generator/version/timestamp.

SBOM jest archiwizowany razem z release, nie generowany dopiero po incydencie.

## Gitleaks CLI

Pełna historia:

```bash
gitleaks git \
  --redact \
  --report-format sarif \
  --report-path artifacts/security/gitleaks.sarif \
  --exit-code 1
```

Zasady:

- `fetch-depth: 0` w CI;
- report jest redacted;
- finding oznacza rotację sekretu, nie tylko usunięcie z aktualnego commita;
- `.gitleaksignore` używa fingerprintu i uzasadnienia; testowe fake secrets preferują jawnie rozpoznawalny format;
- nie echo report zawierającego secret do logu.

## OWASP ZAP

Baseline przeciw staging:

```bash
zap-baseline.py \
  -t https://staging.example.com \
  -r zap-baseline.html \
  -J zap-baseline.json
```

Authenticated scan używa dedykowanego test user/realm/client i jawnego context file. Scope allowlist obejmuje wyłącznie staging hostname. Destructive endpoints i realny IFS production są wykluczone.

DAST obejmuje API i UI, ale nie zastępuje testów business authorization, CSRF, WebSocket/SignalR auth, presigned URLs, browser session ownership i approval race.

## Testy platformowe niezależne od scannerów

Release gate powinien mieć negatywne testy:

- zwykły użytkownik nie otwiera cudzego task/session/artifact;
- stare approval nie działa po zmianie observation hash;
- Browser Worker nie łączy się z PostgreSQL/OpenBao/Valkey;
- strona nie rozszerza domain allowlist;
- presigned URL wygasa i nie pozwala zmienić key/method;
- upload jest niewidoczny przed zakończeniem quarantine scan;
- LLM nie wykonuje tool spoza registry;
- token bez audience/role jest odrzucony;
- WebRTC/noVNC token nie otwiera innego room/session;
- log/trace redaction nie przepuszcza fixture secret.

## Polityka severity

Przykładowy gate:

- Critical: blokuje zawsze, wyjątek tylko awaryjny z security owner i bardzo krótkim expiry;
- High: blokuje release, chyba że zatwierdzone VEX udowadnia brak wpływu i ma termin;
- Medium: ticket z SLA zależnym od exposure;
- Low/Unknown: rejestrowane i trendowane;
- secret finding: blokuje natychmiast i uruchamia rotację;
- niedozwolona licencja: blokuje niezależnie od severity CVE;
- scanner failure/timeout/stale DB: blokuje release jako incomplete.

CVSS nie wystarcza; bierzemy pod uwagę reachability, Internet exposure, privileges i dane.

## Licencje

Allowlist domyślna:

```text
MIT
Apache-2.0
BSD-2-Clause
BSD-3-Clause
ISC
PostgreSQL
```

Review list:

```text
MPL-2.0
LGPL-2.1-only/or-later
LGPL-3.0-only/or-later
GPL-2.0-only/or-later
GPL-3.0-only/or-later
AGPL-3.0-only/or-later
```

Deny list obejmuje SSPL, RSAL, BSL/BUSL, Elastic License, FSL, Commons Clause, PolyForm, NC/evaluation i `NOASSERTION` dla dystrybuowanego komponentu bez wyjaśnienia.

Scanner wynik jest początkiem review; license expression i bundled artifacts należy sprawdzić w źródle projektu.

## Sekrety i sieć scannerów

- Registry pull token jest read-only i krótkotrwały.
- ZAP test credentials są dedykowane staging i minimalne.
- Scanner nie otrzymuje produkcyjnych credentials ani OpenBao root tokenu.
- Network egress allowlist obejmuje registries, vulnerability DB i staging target.
- Repo mount read-only; raporty do osobnego output volume.
- Docker socket nie jest montowany; image tar/registry API zamiast pełnej kontroli daemon, jeśli możliwe.

## Health i smoke test narzędzi

Przed uznaniem pipeline za działający:

```bash
trivy --version
syft version
gitleaks version
zap.sh -version
```

Fixture suite zawiera celowo:

- bezpieczny fake secret, który Gitleaks ma znaleźć;
- podatny test dependency/image;
- Dockerfile z jawną misconfiguration;
- test package z niedozwoloną licencją;
- staging endpoint z kontrolnym header finding dla ZAP.

Pipeline ma zakończyć się błędem dla fixture i sukcesem po zastosowaniu kontrolowanej suppress/VEX zgodnie z polityką.

## Raporty, backup i retencja

Raporty wydania są immutable artifacts:

```text
artifacts/
├── compliance/
│   ├── sbom.cdx.json
│   ├── sbom.spdx.json
│   ├── third-party-notices.txt
│   ├── image-digests.json
│   └── vulnerability-exceptions.vex.json
└── security/
    ├── trivy.sarif
    ├── gitleaks.sarif
    ├── zap-baseline.html
    └── zap-baseline.json
```

Retencja odpowiada co najmniej okresowi wsparcia wydania i wymaganiom audytowym. Raport może zawierać ścieżki, endpointy i fragmenty danych; dostęp jest ograniczony. Backupujemy config/policies/allowlists/VEX oraz release reports. Cache vulnerability DB można odtworzyć.

## Aktualizacja i rollback

1. Pin nowe scanner images.
2. Uruchom fixture regression suite.
3. Sprawdź zmiany reguł, false positives/negatives i report schema.
4. Porównaj findings na tym samym release artifact.
5. Zaktualizuj parser/report consumers.
6. Canary na non-blocking scheduled pipeline.
7. Włącz gate po stabilizacji.
8. Rollback scanner do poprzedniego digest przy zachowaniu nowych findings do review.

Rollback skanera nie jest akceptowalnym sposobem ukrycia prawdziwej nowej podatności.

## Hardening

- nieuprzywilejowany, efemeryczny runner;
- minimalne token scopes;
- obrazy po digestach;
- repo/image input read-only;
- brak produkcyjnych sekretów;
- report redaction i ograniczony dostęp;
- timeout oznacza incomplete/fail;
- stale vulnerability DB blokuje release po przekroczeniu progu;
- suppressions mają owner, reason, evidence, expiry;
- DAST exact scope i brak produkcji;
- scanner output nie jest ślepo publikowany publicznie;
- SBOM/provenance są podpisane lub związane z podpisanym release manifestem.

## Troubleshooting

### Trivy zgłasza podatność bez fixa

Ustal reachability/exposure, mitigację i SLA. Brak fixa nie oznacza automatycznego ignore. Jeśli ryzyko jest zaakceptowane, utwórz VEX z dowodem, ownerem i expiry.

### Gitleaks znajduje testowy sekret

Użyj formalnego `gitleaks:allow`/fingerprint tylko dla bezpiecznego fixture i z uzasadnieniem. Jeśli sekret mógł być realny, najpierw rotacja i incident handling.

### SBOM ma `NOASSERTION`

Sprawdź upstream LICENSE/NOTICE i konkretny artifact. Nie klasyfikuj automatycznie jako MIT. Brak jednoznacznej licencji dystrybuowanego składnika blokuje release do wyjaśnienia.

### ZAP skanuje poza scope

Natychmiast przerwij scan, sprawdź redirects/spider/context/allowlist i zabezpiecz log. Nigdy nie zezwalaj na wildcard target obejmujący produkcję lub IFS klienta.

### Scanner database download nie działa

Użyj kontrolowanego mirror/cache z timestampem, ale nie oznaczaj wyniku jako aktualny, gdy baza przekroczyła maksymalny wiek. Egress exceptions są minimalne i audytowane.

### Za dużo false positives

Napraw konfigurację i stosuj wąskie suppressions. Nie obniżaj globalnie severity ani nie wyłączaj całego scanner class.

## Różnice produkcyjne

Narzędzia działają głównie przed produkcją i cyklicznie na immutable artifacts. Produkcja nie udostępnia scannerom stałych wysokich uprawnień. ZAP nie skanuje produkcji bez jawnej zgody. Release ma zachowany SBOM, digest, notices, wyniki i VEX, a opublikowany obraz jest dokładnie tym przeskanowanym digestem.

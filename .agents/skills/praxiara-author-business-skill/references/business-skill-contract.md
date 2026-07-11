# Kontrakt business skill Praxiary

Business skill jest deklaratywną definicją procesu wykonywaną przez kontrolowany runtime. Nie jest Codex skill, promptem ani kodem do wykonania.

## 1. Bieżący kontrakt wykonywalny

Aktualne schema `skills/schema/praxiara-skill.schema.json` akceptuje wyłącznie:

| Pole | Znaczenie |
|---|---|
| `id` | stabilny namespaced identyfikator procesu |
| `name` | angielska nazwa biznesowa artefaktu |
| `version` | pełny SemVer bez prefiksu `v` |
| `site` | profil witryny, np. `ifs-cloud` |
| `defaultMode` | `Observe`, `Assist` albo `Execute` |
| `inputs` | typowane parametry oraz sensitivity |
| `requiredPermissions` | minimalny zbiór wymaganych uprawnień |
| `risk` | bazowy i finalny poziom R0–R5 |
| `preconditions` | warunki przed rozpoczęciem procesu |
| `steps` | uporządkowane, typowane działania i asercje |
| `postconditions` | fakty dowodzące wyniku biznesowego |
| `recovery` | jawne reakcje na oczekiwane odchylenia |

Schema ma `additionalProperties: false`. Planowane pola takie jak owner, compatibility, evidence, route constraints, timeout, retry, signature i deprecation wymagają najpierw wersjonowanej zmiany schema, modeli, parsera i testów.

## 2. Identyfikator i wersja

- Używaj formatu co najmniej `site.capability.action`, np. `ifs.invoice.resend`.
- Nie umieszczaj environment, locale ani numeru wersji w `id`.
- Zachowuj `id` przy zgodnej ewolucji procesu.
- Podnieś patch dla kompatybilnej poprawki asercji lub locatora.
- Podnieś minor dla kompatybilnego opcjonalnego inputu lub wariantu.
- Podnieś major, gdy zmienia się wymagane wejście, skutek, permission albo semantyka procesu.
- Opublikowanej wersji nie edytuj; utwórz nowy immutable artifact.

## 3. Tryby wykonania

| Tryb | Dozwolone zachowanie |
|---|---|
| `Observe` | odczyt, analiza i preview bez skutku |
| `Assist` | nawigacja i wypełnienie danych bez finalnego commit |
| `Execute` | pełny proces, nadal pod Policy Engine i approvals |

Tryb nie obniża risk narzędzia. `Execute` nie oznacza automatycznej zgody na R4/R5.

## 4. Inputs

Każdy input:

- ma typ zgodny ze schema;
- jawnie określa `required` i `sensitive`;
- ma limit długości, zakres lub format w walidacji semantycznej;
- jest parametryzowany jako `${inputName}`;
- nie zawiera sekretu w przykładzie ani nagraniu;
- jest redagowany w audycie zgodnie z sensitivity.

Nie używaj jednego pola JSON jako obejścia typowanego kontraktu. Jeśli input identyfikuje rekord, zdefiniuj regułę disambiguation.

## 5. Permissions i ryzyko

Wymagaj najmniejszego zbioru permissions. Risk to maksimum:

- klasy każdego narzędzia;
- skutku biznesowego;
- klasy danych;
- environment;
- liczby i wartości rekordów;
- niepewności targetu;
- wyjątków polityki organizacji.

Typowe poziomy:

| Poziom | Przykład |
|---|---|
| `R0ReadOnly` | odczyt statusu |
| `R1Navigation` | wyszukiwanie i otwarcie rekordu |
| `R2DraftInput` | wypełnienie bez zapisu |
| `R3PersistDraft` | zapis odwracalnego szkicu |
| `R4BusinessCommit` | wysłanie, release, approve, post |
| `R5Critical` | delete, payment, bank account, permission |

Nie obniżaj risk na podstawie sugestii LLM. Niepewność podnosi poziom albo zatrzymuje proces.

## 6. Preconditions

Preconditions mają zatrzymać proces przed skutkiem. Obejmij co najmniej:

- uwierzytelnienie użytkownika;
- właściwy tenant i environment;
- wymagane permissions;
- poprawność i jednoznaczność inputów;
- dozwoloną wersję skill/site/IFS;
- brak już osiągniętej postcondition, jeśli chroni to przed duplikatem.

Nazwy asercji są stabilnymi angielskimi identyfikatorami, nie opisem dla UI.

## 7. Steps

Każdy krok:

- ma stabilne `id` w kebab-case;
- wskazuje istniejący `action` w snake_case;
- przekazuje wyłącznie potrzebne `arguments`;
- ma asercje potwierdzające stan wymagany przez kolejny krok;
- jawnie wskazuje `requiresApproval` dla konkretnego R4/R5;
- nie zawiera kodu, XPath, sekretu, arbitralnego URL ani ścieżki pliku.

Preferuj:

```yaml
- id: find-invoice
  action: ifs_find_customer_invoice
  arguments:
    invoiceNumber: ${invoiceNumber}
  assertions:
    - exactly-one-invoice-found
```

Nie opisuj technicznej sekwencji kliknięć, jeśli zatwierdzone narzędzie biznesowe potrafi wybrać API/UI/hybrid.

## 8. Approval point

Dla kroku skutkowego:

- ustaw `requiresApproval: true`;
- ustaw `finalActionLevel` co najmniej na risk narzędzia;
- przygotuj preview z environment, targetem, wartościami przed/po, odbiorcą i konsekwencją;
- wymagaj świeżych bound facts i zgodności action hash;
- unieważnij approval po zmianie argumentu, targetu, rewizji, wersji skill lub policy;
- nie używaj starego approval w recovery ani replay.

Pytanie bez konkretnych danych operacji nie jest approval.

## 9. Postconditions i evidence

Postcondition opisuje wynik biznesowy, np. wpis w communication history, status rekordu albo potwierdzenie Projection API. Sam komunikat sukcesu lub brak wyjątku nie wystarcza.

Docelowe evidence obejmuje:

- observation/API response hash;
- target identity;
- route i tool call;
- approval ID/action hash dla R4/R5;
- wynik weryfikacji UI i API;
- skill, policy i tool catalog version;
- zredagowane artefakty z checksumą.

Jeśli bieżące schema nie ma pola `evidence`, wymagania te implementuj w runtime i testach, nie jako nieznane pole YAML.

## 10. Recovery

Recovery ma wybierać jedną kontrolowaną reakcję:

- stop and report;
- request disambiguation;
- require user login;
- reobserve;
- rebuild preview and request new approval;
- manual reconciliation;
- bezpieczna compensating action, jeśli została osobno zaprojektowana.

Nie retry'uj automatycznie `NonIdempotent`. Timeout po wysłaniu skutku prowadzi do `OutcomeUnknown` i rekoncyliacji.

## 11. Recorder

Recorder tworzy wyłącznie draft. Przed review:

- zastąp wartości testowe parametrami;
- usuń PII, tokeny, cookies i identyfikatory klienta;
- zastąp kruche XPath/współrzędne stabilną semantyką;
- oznacz wszystkie kroki skutkowe;
- dodaj preconditions, asercje i postconditions;
- sprawdź wykryte Projection/OData bez kopiowania auth;
- zdefiniuj recovery ręcznie;
- zachowaj provenance kroków.

## 12. Przykładowy szkielet

```yaml
id: ifs.entity.action
name: Perform business action
version: 1.0.0
site: ifs-cloud
defaultMode: Assist
inputs:
  recordId:
    type: string
    required: true
    sensitive: false
requiredPermissions:
  - Capability.Read
  - Capability.Execute
risk:
  defaultLevel: R1Navigation
  finalActionLevel: R4BusinessCommit
preconditions:
  - user-is-authenticated
  - environment-is-allowlisted
steps:
  - id: load-record
    action: ifs_find_record
    arguments:
      recordId: ${recordId}
    assertions:
      - exactly-one-record-found
  - id: prepare-preview
    action: ifs_prepare_action
    arguments:
      recordId: ${recordId}
    assertions:
      - target-facts-are-current
  - id: commit-action
    action: ifs_execute_action
    arguments:
      recordId: ${recordId}
    assertions:
      - approval-hash-matches-current-action
    requiresApproval: true
  - id: verify-result
    action: ifs_verify_action
    arguments:
      recordId: ${recordId}
    assertions:
      - business-result-is-confirmed
postconditions:
  - business-result-is-confirmed-by-api
  - audit-record-is-complete
recovery:
  multiple-records-found: stop-and-request-disambiguation
  session-expired: require-user-login
  approval-expired: rebuild-preview-and-request-new-approval
  verification-failed: stop-and-escalate-with-evidence
```

## 13. Pipeline publikacji

1. Parse YAML bez unsafe type deserialization.
2. JSON Schema validation.
3. Semantic validation.
4. Tool catalog, permissions i risk validation.
5. Secret oraz forbidden construct scan.
6. Locator, locale i route lint.
7. Offline replay.
8. `Praxiara.TestSites` albo sandbox IFS.
9. Security evals.
10. Expert i process owner review.
11. Podpisanie immutable artifact.
12. Kontrolowana publikacja z możliwością rollbacku.

Żaden pojedynczy agent ani recorder nie wykonuje samodzielnie wszystkich kroków publikacji.

# Checklista integracji IFS Cloud

Używaj tej checklisty przy projektowaniu klienta Projection/OData, rejestru projekcji, profilu środowiska, routera operacji lub wysokopoziomowego narzędzia IFS.

## 1. Kontrakt operacji

Zapisz przed implementacją:

| Pole | Pytanie kontrolne |
|---|---|
| nazwa biznesowa | Co użytkownik chce osiągnąć, niezależnie od transportu? |
| target | Jaki rekord lub zbiór rekordów jest zmieniany? |
| environment | Development, test, acceptance czy production? |
| permission | Jakie projection/entity/action permission jest wymagane? |
| skutek | Odczyt, draft, commit, delete, payment czy privilege change? |
| idempotencja | Czy ponowne żądanie jest bezpieczne i jak je rozpoznać? |
| verifier | Jaki niezależny fakt dowodzi sukcesu? |
| owner | Kto zatwierdza semantykę i macierz zgodności? |

Nie używaj opisu typu „kliknij przycisk”. Kontrakt ma nazywać operację biznesową, np. `ifs_send_invoice`.

## 2. Profil środowiska

Profil musi jednoznacznie określać:

- stabilny `Id` i `BaseUri`;
- tenant, locale i environment kind;
- dokładne hosty IFS, SSO, API i wymagane CDN;
- allowlistę projections, entity sets, actions i functions;
- wspieraną wersję release update oraz customizations;
- limity timeout, rozmiaru odpowiedzi, stron i rekordów;
- sposób delegacji tożsamości użytkownika;
- politykę danych, audytu i retencji;
- dostępność Windows companion dla funkcji Aurena.

Produkcja musi mieć technicznie odrębny identyfikator. `environmentId` wchodzi do action hash; sama etykieta UI nie wystarcza.

## 3. Rejestr projekcji

Każdy zatwierdzony wpis zawiera:

- projection i wersję kontraktu;
- entity sets, functions oraz actions;
- dozwolone metody i query options;
- schema wejścia/wyjścia i maksymalne rozmiary;
- wymagane permissions i klasyfikację danych;
- klasę ryzyka oraz consequence;
- `ReadOnly`, `Idempotent`, `ConditionallyIdempotent` albo `NonIdempotent`;
- mapowanie na wysokopoziomowe narzędzie Praxiary;
- dozwolone trasy `ProjectionApi`, `Browser` i `Hybrid`;
- właściciela, datę review oraz macierz IFS/locale/customization;
- verifier, recovery i zachowanie po wyniku niepewnym.

Import `$openapi` jest kontrolowanym procesem administracyjnym. Nowy endpoint nie staje się dozwolony tylko dlatego, że pojawił się w dokumencie IFS.

## 4. Decyzja o trasie

| Warunek | Trasa |
|---|---|
| API realizuje operację i pozwala potwierdzić wynik | `ProjectionApi` |
| API umożliwia odczyt/preview/verify, ale commit wymaga UI | `Hybrid` |
| brak wspieranego API, a zatwierdzony workflow UI jest kompletny | `Browser` |
| kontrakt, permission albo target są niepewne | deny/stop |
| wymagany Aurena Agent bez zatwierdzonego companion | unsupported/stop |

Nie fallbackuj automatycznie po `403`, błędzie biznesowym, zmianie schema lub niepewnym wyniku. Każdy z tych stanów ma inną przyczynę i recovery.

## 5. Klient Projection/OData

- Twórz `HttpClient` przez DI i typowaną konfigurację; nie przyjmuj hosta od modelu.
- Łącz wyłącznie zatwierdzony `BaseUri` z walidowaną ścieżką względną.
- Waliduj projection, entity/action/function i query options przed wysłaniem.
- Ogranicz `$select`, `$filter`, `$expand`, `$orderby`, `$top` i pagination do potrzeb operacji.
- Nie składaj OData z nieescaped inputu. Używaj typowanego buildera lub jawnej serializacji wartości.
- Ponownie waliduj origin, scheme i host dla next links oraz redirectów.
- Wymagaj oczekiwanego content type, limituj payload i bezpiecznie obsługuj nieznane pola.
- Stosuj ETag lub inny concurrency token, gdy kontrakt go oferuje.
- Używaj krótkotrwałego tokenu delegowanego; nie loguj `Authorization`, cookies ani pełnego payloadu.
- Rozróżniaj timeout przed wysłaniem, timeout po wysłaniu i błąd odpowiedzi.
- Retry ogranicz do sklasyfikowanych błędów przejściowych i wyłącznie bezpiecznych operacji.

## 6. Granice kodu .NET

- Jeden główny typ top-level umieszczaj w jednym, tak samo nazwanym pliku.
- Publiczne DTO modeluj jako niezmienne records; encje i stany chronią przejścia metodami.
- Nie zwracaj surowego `JsonDocument` poza wąski adapter, jeśli istnieje stabilny kontrakt domenowy.
- Używaj jawnego mapowania między kontraktem IFS i modelem Praxiary.
- Port umieszczaj po stronie use case, implementację transportu po stronie integracji.
- Każde I/O wykonuj asynchronicznie z `CancellationToken` na końcu sygnatury.
- Wstrzykuj `TimeProvider`; identyfikatory nowych zdarzeń twórz jako UUIDv7.
- Nie buduj wspólnego `IfsManager`, ogólnego repository ani refleksyjnego dispatchu.
- W logach używaj stabilnych placeholderów, correlation ID i zredagowanych identyfikatorów.

## 7. Narzędzie biznesowe

Schema narzędzia powinna zawierać:

- typowane argumenty z limitami i `additionalProperties: false`;
- wymagane permission i environment scope;
- risk oraz consequence zdefiniowane poza modelem;
- preconditions i idempotency semantics;
- timeout i dozwolony retry;
- verifier i możliwe kody wyników;
- pola approval preview;
- zasady redakcji i evidence.

LLM wybiera narzędzie biznesowe. Adapter, registry i policy wybierają transport.

## 8. Approval i wynik

Dla R4/R5 zwiąż approval co najmniej z:

```text
tenantId
userId
taskId
sessionId
toolCallId
toolName
canonicalArguments
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

Przed commit wykonaj świeży odczyt i porównaj bound facts. Po commit:

1. sprawdź odpowiedź transportu;
2. odczytaj aktualny stan rekordu;
3. sprawdź historię lub wpis komunikacji;
4. dla trasy UI sprawdź także komunikat i stan strony;
5. zapisz evidence wraz z checksumą;
6. przy braku rozstrzygnięcia ustaw `OutcomeUnknown`.

## 9. Aurena Agent

Nie zakładaj, że serwerowy Chromium zastąpi komponent Windows. Companion wymaga:

- zarządzanego urządzenia i rejestracji;
- obecności użytkownika;
- krótkotrwałego, wzajemnie uwierzytelnionego kanału;
- allowlisty komend bez arbitrary shell;
- osobnego approval i audytu urządzenia;
- sprawdzenia zgodności wersji rozszerzenia, programu i IFS.

Brak companion oznacza `unsupported`, nie próbę obejścia przez download, lokalną ścieżkę albo shell.

## 10. Macierz testów

| Obszar | Minimalny dowód |
|---|---|
| registry | unknown projection/action jest odrzucone |
| identity | brak permission nie przechodzi na service account |
| routing | każda trasa i brak bezpiecznej trasy mają test |
| OData | injection, limit, pagination i obcy next link są odrzucone |
| approval | zmiana targetu, środowiska lub argumentu unieważnia zgodę |
| idempotencja | timeout po commit nie wykonuje automatycznego retry |
| verifier | UI success bez zgodnego API nie staje się sukcesem |
| redaction | token, cookie i dane Restricted nie trafiają do logu |
| compatibility | locale, customization i wspierany IFS release są pokryte |
| Aurena | brak companion kończy się bezpiecznym stop |

Uruchamiaj testy wyłącznie na fixture, `Praxiara.TestSites` albo zatwierdzonym sandboxie IFS.

---
name: praxiara-author-business-skill
description: Tworzy wersjonowane business skills Praxiary, w tym YAML, schema, parser, walidację, recorder, replay i publikację. Użyj dla `skills/**/*.skill.yaml`, `skills/schema` lub `Praxiara.Skills`. Nie używaj do Codex skills w `.agents/skills`, promptów ani implementacji narzędzia browser lub IFS.
---

# Tworzenie business skills Praxiary

Traktuj business skill jako niezmienny, walidowany artefakt procesu, a nie prompt ani skrypt. Recorder może przygotować draft, lecz ekspert i właściciel procesu odpowiadają za semantykę, ryzyko, evidence i publikację.

## Rozróżnij dwa pojęcia

- Business skill Praxiary to YAML pod `skills/`, wykonywany przez `Praxiara.Skills` i kontrolowany przez Policy Engine.
- Codex skill to `SKILL.md` pod `.agents/skills` lub globalnym katalogiem Codex.

Jeżeli użytkownik chce utworzyć lub zmienić Codex skill, użyj `skill-creator`, nie tego workflow.

## Obowiązkowy kontekst

1. Przeczytaj `AGENTS.md`, szczególnie reguły skills, IFS, approvals, audytu, języka i testów.
2. Przeczytaj sekcje skills, recorder, ryzyka i IFS w `docs/PLAN_WYTWORZENIA.md`.
3. Przeczytaj w całości [kontrakt business skill](references/business-skill-contract.md).
4. Przeczytaj `skills/schema/praxiara-skill.schema.json` oraz najbliższy przykład dla tej samej witryny i klasy ryzyka.
5. Jeśli zmieniasz runtime, przeczytaj model, parser, walidator, CLI i odpowiednie testy przed edycją.

## Workflow

### 1. Zdefiniuj proces

Ustal z dostarczonych wymagań:

- jednoznaczny cel biznesowy i właściciela procesu;
- wspierane site, środowiska, locale i warianty customizacji;
- wejścia, klasyfikację danych, sensitivity i zasady redakcji;
- wymagane permissions;
- tryb domyślny `Observe`, `Assist` albo `Execute`;
- preconditions, skutek, klasę R0–R5 i kryterium sukcesu;
- recovery dla oczekiwanych odchyleń oraz `OutcomeUnknown`.

Nie zgaduj brakujących skutków biznesowych. Jeśli wiele rekordów może pasować, wymagaj rozstrzygnięcia przed krokiem skutkowym.

### 2. Sprawdź katalog narzędzi

- Każdy `action` musi wskazywać istniejące, zatwierdzone i typowane narzędzie.
- Preferuj narzędzie biznesowe nad sekwencją technicznych kliknięć.
- Nie umieszczaj JavaScriptu, shella, dowolnego HTTP, XPath, współrzędnych ani ścieżki hosta.
- Nie rozszerzaj allowlisty, permission ani klasy możliwości z poziomu YAML.

Jeśli potrzebnego narzędzia nie ma, zatrzymaj authoring i opisz osobne zadanie implementacyjne. Nie twórz fikcyjnego `action` tylko po to, aby ukończyć YAML.

### 3. Zbuduj najmniejszy kompletny YAML

- Zacznij od bieżącego schema i najbliższego przykładu.
- Używaj stabilnego namespaced `id`, SemVer oraz angielskich identyfikatorów maszynowych.
- Parametryzuj dane wejściowe; nie zapisuj danych klienta ani wartości z nagrania.
- Umieść asercję po każdym kroku, którego niepowodzenie zmieniłoby następną decyzję.
- Zdefiniuj postconditions dowodzące skutku biznesowego, nie samego kliknięcia.
- Zdefiniuj jawne recovery typu stop, reobserve, disambiguation, login, new approval albo manual reconciliation.

Nie dodawaj pól planowanego kontraktu do dokumentu, dopóki schema, model C#, parser i testy ich nie obsługują. `additionalProperties: false` oznacza, że nieznane pole jest błędem, nie future-proofingiem.

### 4. Ustal ryzyko i approval

- Wyznacz ryzyko jako maksimum narzędzia, operacji, danych, środowiska, liczby rekordów i niepewności.
- `Observe` nie wykonuje skutku, `Assist` nie wykonuje finalnego commit, a `Execute` nadal podlega Policy Engine.
- Oznacz konkretny krok R4/R5 przez `requiresApproval`; sama wysoka wartość `finalActionLevel` nie wystarcza.
- W asercjach przed skutkiem wymagaj aktualnego targetu i zgodności action hash.
- Nie zapisuj approval, nonce ani session state w definicji skill.

### 5. Zmień kontrakt runtime, jeśli jest to konieczne

Gdy wymaganie nie mieści się w bieżącym schema:

1. zmień JSON Schema jako kontrakt źródłowy;
2. zaktualizuj modele C# i parser bez niebezpiecznej deserializacji;
3. dodaj walidację semantyczną i stabilny kod błędu;
4. zaktualizuj CLI, przykład oraz testy zgodności;
5. opisz migrację istniejących dokumentów i kompatybilność;
6. dopiero potem użyj nowego pola w business skill.

Stosuj jeden główny typ top-level na plik, także dla rekordów, interfejsów i enumów. Jeśli dotykany plik łączy typy, rozdziel je bez zmiany kontraktu wire. Zachowuj SOLID, DRY i KISS: parser parsuje, walidator waliduje, registry rozwiązuje wersje, a publisher zarządza lifecycle; nie twórz jednej klasy typu `SkillManager`.

### 6. Waliduj draft

Wymagaj kolejno:

1. poprawnego parse YAML;
2. JSON Schema validation;
3. walidacji identyfikatorów, referencji, permissions i narzędzi;
4. zgodności ryzyka, approval i trybu;
5. lintu locatorów, locale i tras;
6. kontroli sekretów, danych testowych i zakazanych konstrukcji;
7. offline replay;
8. testu na `Praxiara.TestSites` albo sandboxie IFS;
9. security evals adekwatnych do ryzyka;
10. review eksperta, właściciela procesu i wymaganego approvera publikacji.

Nie publikuj automatycznie. Opublikowana wersja jest niezmienna; poprawka tworzy nową wersję, a rollback wskazuje wcześniejszy podpisany artefakt.

## Minimalne testy

Dodaj testy dla:

- poprawnego dokumentu oraz każdego nowego pola schema;
- nieznanego pola, narzędzia, permission i duplikatu kroku;
- błędnego SemVer, `id`, typu inputu i odwołania do parametru;
- sekretu lub danych nagrania w draftcie;
- brakującego approval dla R4/R5;
- wszystkich recovery branches i nieudanej postcondition;
- `Observe`, `Assist` i `Execute` bez możliwości obejścia policy;
- deterministycznego replay oraz kompatybilności locale/IFS.

## Warunki zatrzymania

Zatrzymaj pracę, gdy:

- nie istnieje zatwierdzone narzędzie dla wymaganego kroku;
- owner, skutek, permission, target albo klasa ryzyka są nieznane;
- replay wymaga produkcji lub rzeczywistych danych klienta;
- recovery proponuje ponowienie nieidempotentnego skutku bez rekoncyliacji;
- recorder ujawnił sekret, PII albo kruchy krok, którego nie można bezpiecznie zastąpić;
- publikacja miałaby ominąć schema, evals, review, podpis albo separation of duties.

## Weryfikacja i raport

Uruchom walidator schema, testy `Praxiara.Skills`, replay i odpowiednie bramki z `AGENTS.md`. W raporcie podaj:

- `id`, wersję, site, tryb i klasę ryzyka;
- użyte narzędzia, permissions i trasy;
- preconditions, approval point, postconditions, recovery i evidence;
- wykonane walidacje, replay oraz wynik review;
- ograniczenia kompatybilności i powód każdej nierozstrzygniętej decyzji.

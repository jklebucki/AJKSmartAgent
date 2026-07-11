# Checklista ewolucji kontraktu

Używaj tej checklisty przed implementacją, podczas review i przed raportem końcowym. Zaznaczaj tylko punkty potwierdzone kodem lub testem.

## 1. Zakres i właściciel

- [ ] Zdefiniowano scenariusz użytkownika oraz oczekiwany wynik biznesowy.
- [ ] Wskazano producenta, transport i wszystkich znanych konsumentów.
- [ ] Określono, czy kontrakt jest publicznym HTTP API, SignalR, czy wewnętrznym komunikatem między procesami.
- [ ] Wybrano prawidłowy moduł: `Contracts`, `Application`, feature w `Api` albo adapter konsumenta.
- [ ] Endpoint i DTO nie przejmują logiki domenowej.
- [ ] Każdy zmieniany top-level type ma własny plik o tej samej nazwie.
- [ ] Kod i dokumentacja kodowa są angielskie; ręczny Markdown jest polski.

## 2. Klasyfikacja kompatybilności

| Zmiana | Domyślna ocena | Bezpieczne postępowanie |
|---|---|---|
| nowe opcjonalne pole requestu | zwykle addytywna | wartość domyślna i test starego payloadu |
| nowe pole response | zwykle addytywna | potwierdzić, że konsumenci tolerują nieznane pola |
| nowe wymagane pole requestu | breaking | nowa wersja albo okres przejściowy |
| zmiana optional na required | breaking | migracja konsumentów przed zaostrzeniem |
| zmiana nazwy pola | breaking | równoległa nazwa lub jawny adapter wersji |
| zmiana typu albo formatu | breaking | nowe pole/wersja i migracja danych |
| usunięcie pola, statusu lub endpointu | breaking | deprecacja, telemetryczne potwierdzenie braku użycia, potem usunięcie |
| nowa wartość enum | zgodna warunkowo | sprawdzić obsługę wartości nieznanych u każdego konsumenta |
| zmiana statusu HTTP lub kodu błędu | zmiana zachowania | test konsumenta i release note |
| zmiana nazwy eventu SignalR | breaking | publikacja obu wersji w okresie przejściowym |

- [ ] Zapisano ocenę kompatybilności i jej uzasadnienie.
- [ ] Określono wspierane wersje N/N-1 oraz kolejność wdrożenia.
- [ ] Dla breaking change istnieje jawna decyzja, ścieżka migracji i warunek usunięcia kompatybilności przejściowej.
- [ ] Nie polegano wyłącznie na tym, że `System.Text.Json` obecnie toleruje nieznane pola.

## 3. DTO i JSON

- [ ] Payload nie jest encją domenową, encją EF ani anonimowym typem.
- [ ] Publiczny DTO jest niezmiennym rekordem, jeżeli semantyka nie wymaga mutacji.
- [ ] Nullability odzwierciedla wire contract, a nie wygodę implementacji.
- [ ] Wartości domyślne mają jawne znaczenie i test deserializacji starszego payloadu.
- [ ] Nazwy JSON pozostają stabilne niezależnie od refaktoryzacji nazwy wewnętrznej.
- [ ] Identyfikator ma jeden trwały format na wire.
- [ ] Czas jest przesyłany jako jednoznaczny UTC/offset, bez lokalnego `DateTime` o nieznanym `Kind`.
- [ ] Precyzja liczb, szczególnie kwot, nie jest tracona przez `double` ani zmianę formatu.
- [ ] Kolekcje rozróżniają zgodnie z kontraktem brak wartości, pustą kolekcję i stronicowanie.
- [ ] Nowe wartości enum mają strategię dla starszego konsumenta; nie zakłada się wyczerpującego `switch` bez fallbacku.
- [ ] Payload nie zawiera sekretów, cookies, tokenów, surowego storage state ani zbędnych danych osobowych.

## 4. HTTP i OpenAPI

- [ ] Metoda i ścieżka HTTP oddają semantykę zasobu lub komendy biznesowej.
- [ ] Endpoint należy do stabilnej grupy wersji, np. `/api/v1`.
- [ ] Operation ID jest unikalny, stabilny i angielski.
- [ ] Parametry route/query/header/body są jawnie typowane i walidowane.
- [ ] Maksymalny rozmiar requestu i pliku jest ograniczony tam, gdzie to potrzebne.
- [ ] Sukces oraz każdy oczekiwany błąd mają jawny `TypedResults` i status HTTP.
- [ ] `ProblemDetails` zawiera stabilny kod maszynowy, bez stack trace i danych wrażliwych.
- [ ] `404` nie ujawnia istnienia rekordu, jeżeli wymaga tego ochrona przed BOLA/IDOR.
- [ ] OpenAPI opisuje realne content types, odpowiedzi, auth i ograniczenia.
- [ ] Dokument OpenAPI nie rozszerza ekspozycji produkcyjnej bez jawnej decyzji.

## 5. Authentication, authorization i skutki

- [ ] Brak lub błąd konfiguracji identity kończy się odmową dostępu.
- [ ] Polityka autoryzacji jest przypisana do endpointu lub całej grupy i ma test negatywny.
- [ ] Tenant/ownership jest sprawdzany po stronie serwera na konkretnym rekordzie.
- [ ] R3–R5 nie mogą ominąć Policy Engine.
- [ ] R4 wymaga konkretnego approval, a R5 także silnego potwierdzenia lub reauthentication.
- [ ] Approval wiąże użytkownika, task, narzędzie, argumenty, rekord, środowisko, skill version i revision.
- [ ] Operacja skutkowa przyjmuje lub generuje stabilny idempotency key.
- [ ] Po sukcesie wykonywana jest weryfikacja biznesowa i audyt, nie tylko sprawdzenie braku wyjątku.
- [ ] Rate limiting i ochrona przed replay odpowiadają ryzyku endpointu.

## 6. SignalR i komunikaty wewnętrzne

- [ ] Nazwa eventu i payload są wersjonowane oraz angielskie.
- [ ] Semantyka reconnect, kolejności, duplikatów i utraconych wiadomości jest jawna.
- [ ] Konsument potrafi odrzucić wiadomość z nieobsługiwaną wersją bez częściowego skutku.
- [ ] Komunikat ma correlation IDs i nie przenosi poświadczeń.
- [ ] Limit rozmiaru wiadomości jest zachowany.
- [ ] Rollout pozwala producentowi i konsumentowi działać w różnych wersjach.

## 7. Testy wymagane przez rodzaj zmiany

- [ ] Round-trip JSON sprawdza nazwy, nullability, enum, timestamp i wartości domyślne.
- [ ] Stary payload jest czytany przez nowy kod, jeśli obowiązuje kompatybilność wsteczna.
- [ ] Nowy payload ma przewidywalne zachowanie na starym konsumencie albo jest wersjonowany.
- [ ] OpenAPI zawiera operation ID, schema, auth, odpowiedzi i content types.
- [ ] Test integracyjny obejmuje sukces, walidację, brak auth, brak uprawnienia i oczekiwany błąd.
- [ ] Operacja skutkowa ma test idempotencji, approval, TOCTOU i weryfikacji.
- [ ] SignalR ma test reconnect/duplikatu/kolejności odpowiedni do gwarancji.
- [ ] Nie użyto produkcyjnego IFS ani produkcyjnych danych.

## 8. Bramka końcowa

- [ ] `dotnet format` nie zgłasza zmian dla zmodyfikowanego zakresu.
- [ ] Targetowane testy kontraktowe i integracyjne przechodzą.
- [ ] Build rozwiązania kończy się bez ostrzeżeń.
- [ ] Przejrzano diff pod kątem przypadkowych zmian JSON/OpenAPI i sekretów.
- [ ] Dokument operatora lub konsumenta jest zaktualizowany, jeśli zmieniło się zachowanie.
- [ ] Raport wymienia kompatybilność, rollout, testy i znane ograniczenia.

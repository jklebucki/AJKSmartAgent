---
name: praxiara-evolve-api-contract
description: 'Rozwija kontrakty HTTP, SignalR, OpenAPI i DTO między procesami Praxiary. Użyj dla endpointu, komunikatu, pola JSON, kodu błędu, autoryzacji lub publicznego zachowania API. Nie używaj do logiki domenowej, UI ani persystencji, jeśli kontrakt wire się nie zmienia.'
---

# Rozwijanie kontraktów API Praxiary

Prowadź zmianę od scenariusza i kompatybilności do kontraktu, implementacji oraz dowodu w testach. Kontrakt jest granicą procesu; nie traktuj go jak przypadkowego kształtu klas C#.

Przed pracą przeczytaj `AGENTS.md` oraz [checklistę kontraktu](references/contract-checklist.md) w całości. Gdy zmiana wpływa na przepływ produktu lub bezpieczeństwo, przeczytaj też właściwe części `docs/PLAN_WYTWORZENIA.md`.

## Reguły stałe

- Zachowaj kierunek zależności: `Contracts` przechowuje wersjonowane typy wire, `Application` przypadki użycia, a `Api` mapowanie transportu i composition root.
- Nie umieszczaj logiki biznesowej w endpointach, hubach, filtrach ani `Program.cs`.
- Twórz dokładnie jeden top-level type w jednym pliku C# i nazywaj plik tak jak typ. Rozdziel dotknięte typy zastane w jednym pliku; wyjątkiem jest kod generowany i pliki migracji EF.
- Stosuj SOLID do granic i odpowiedzialności, DRY do wiedzy biznesowej, a KISS do implementacji. Nie twórz interfejsu, fabryki ani warstwy tylko dlatego, że istnieje taki wzorzec.
- Kod, identyfikatory, nazwy plików kodowych, komentarze, XML docs, operation IDs, nazwy komend, testów, logów i kody błędów zapisuj po angielsku. Ręczny Markdown pozostaje polski.
- Używaj C# 14, nullable reference types, niezmiennych rekordów dla kontraktów oraz jawnych typów wyników.
- Waliduj na granicy procesu. Model domenowy nadal chroni własne niezmienniki; walidacja DTO nie jest ich zamiennikiem.
- Każde I/O wykonuj asynchronicznie i przekazuj `CancellationToken` jako ostatni parametr. Nie używaj sync-over-async.
- Zachowaj fail-closed authentication i autoryzację po stronie serwera. Dokumentacja OpenAPI ani UI nie są kontrolą dostępu.
- Nie dodawaj pakietu bez bramki licencyjnej i bezpieczeństwa z `AGENTS.md`.

## Przepływ pracy

### 1. Zaplanuj zmianę

1. Sprawdź `git status --short` i najbliższy istniejący endpoint, DTO, hub, test kontraktowy oraz konsumenta.
2. Zapisz scenariusz, aktora, wymagane uprawnienie, poziom ryzyka R0–R5 i kryteria akceptacji.
3. Narysuj krótki łańcuch producent → transport → konsumenci. Uwzględnij React, workery i narzędzia CLI, jeżeli korzystają z kontraktu.
4. Sklasyfikuj zmianę jako addytywną, zgodną warunkowo albo breaking. Określ okno kompatybilności N/N-1 i kolejność rolloutów.
5. Wskaż wpływ na JSON, OpenAPI, statusy HTTP, `ProblemDetails`, SignalR, auth, idempotencję, audyt i retencję.
6. Zaplanuj najpierw zmianę testu/kontraktu, następnie mapowanie do przypadku użycia, na końcu adapter transportowy.

Jeżeli breaking change nie ma zatwierdzonej wersji, okresu przejściowego lub migracji konsumentów, zatrzymaj implementację i poproś o decyzję. Nie ukrywaj przełamania jako refaktoryzacji.

### 2. Zmień kontrakt

1. Umieść typ współdzielony między procesami w `src/Praxiara.Contracts`; typ używany tylko przez pojedynczy endpoint może pozostać przy feature w `Praxiara.Api`.
2. Nadaj każdemu DTO, komunikatowi, kodowi błędu i zdarzeniu jednoznaczną semantykę. Nie ujawniaj encji domenowej ani encji EF jako payloadu.
3. Utrzymaj stabilne nazwy właściwości JSON, formaty identyfikatorów, timestampów, wartości enum i zachowanie pól opcjonalnych.
4. Dla zmiany breaking zastosuj wersjonowanie lub okres przejściowy z równoległą obsługą starego kształtu. Usuń starą ścieżkę dopiero po potwierdzeniu migracji konsumentów.
5. Dodaj XML docs w języku angielskim tam, gdzie kontrakt publiczny wymaga wyjaśnienia zachowania, wyjątków, anulowania lub bezpieczeństwa. Nie opisuj oczywistej składni.

### 3. Zaimplementuj transport

1. Grupuj endpointy według feature i utrzymuj `Program.cs` jako composition root.
2. Stosuj `TypedResults` oraz jawny union wyników dla wszystkich przewidzianych odpowiedzi.
3. Zwracaj ujednolicony `ProblemDetails` z trwałym, angielskim kodem maszynowym. Nie ujawniaj stack trace, sekretów ani danych wewnętrznych.
4. Ustaw jawnie autoryzację, limity rozmiaru, typy treści, idempotency key i rate limiting odpowiednio do ryzyka.
5. Nadaj endpointowi stabilny angielski operation ID i uzupełnij OpenAPI. Nie dokumentuj odpowiedzi, której kod nie może zwrócić.
6. Mapuj DTO do przypadku użycia jawnie. Nie dodawaj biblioteki mapującej ani ogólnego mediatora bez osobnej oceny.
7. Jeśli kontrakt jest skutkowy, zachowaj niezależne policy, approval, idempotencję, weryfikację i audyt; endpoint nie może ich ominąć.

### 4. Zaktualizuj konsumentów

1. Wyszukaj użycia nazwy endpointu, operation ID, eventu SignalR i właściwości JSON w całym repozytorium.
2. Zmień producentów i konsumentów w kolejności bezpiecznej dla rolloutów. Nie zakładaj, że wszystkie procesy zostaną wdrożone jednocześnie.
3. Jeżeli konsument leży poza zakresem bieżącej zmiany, pozostaw zgodny adapter i opisz właściciela oraz warunek usunięcia ścieżki przejściowej.

### 5. Udowodnij zmianę

1. Dodaj testy serializacji i kompatybilności w `Praxiara.ContractTests`.
2. Dodaj testy endpointu, walidacji, auth i błędów w `Praxiara.IntegrationTests`.
3. Dla zmiany policy lub danych wrażliwych dodaj scenariusz w `Praxiara.SecurityTests`.
4. Uruchom najwęższe projekty testowe, potem build rozwiązania; dla zmiany przekrojowej uruchom pełną bramkę z `AGENTS.md`.

```bash
dotnet test tests/Praxiara.ContractTests/Praxiara.ContractTests.csproj
dotnet test tests/Praxiara.IntegrationTests/Praxiara.IntegrationTests.csproj
dotnet build Praxiara.slnx --no-restore
```

### 6. Zakończ pracę

- Uruchom formatowanie i obejrzyj pełny diff.
- Potwierdź jeden top-level type na plik, poprawny DAG, brak sekretów i brak przypadkowej zmiany kontraktu.
- Zaktualizuj polski Markdown tylko wtedy, gdy zmienia się zachowanie konsumenta, operatora lub rollout.
- Raportuj kształt zmiany, kompatybilność, zmienionych konsumentów, wykonane testy i rzeczywiste ograniczenia.

Nie uznawaj zmiany za gotową tylko dlatego, że OpenAPI się generuje albo endpoint zwraca 2xx. Gotowy kontrakt ma udowodnioną semantykę, kompatybilność, autoryzację i zachowanie błędów.

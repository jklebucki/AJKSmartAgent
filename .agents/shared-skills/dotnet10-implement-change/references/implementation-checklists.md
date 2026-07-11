# Warunkowe checklisty implementacji

Zastosuj checklistę bazową oraz wyłącznie sekcje odpowiadające zmienianemu zachowaniu.

## Każda zmiana

- [ ] Kryteria akceptacji opisują happy path, oczekiwany błąd i ważną granicę.
- [ ] Wskazano właściwy moduł i dozwolony kierunek zależności.
- [ ] Diff jest najmniejszą kompletną zmianą bez przypadkowego refaktoringu.
- [ ] Każdy ręczny top-level named type ma osobny plik o tej samej nazwie.
- [ ] Nowe pliki używają file-scoped namespace.
- [ ] Nullable i warning-as-error pozostają włączone; nie dodano suppression bez uzasadnienia.
- [ ] Kod, identyfikatory, komentarze, XML docs i komunikaty techniczne są po angielsku.
- [ ] Test dowodzi zachowania i istotnego błędu lub granicy.
- [ ] Uruchomiono format check, build i odpowiednie testy.

## Jeśli zmiana dotyczy domeny lub use case

- [ ] Niezmiennik znajduje się w domenie, a nie wyłącznie w endpointzie lub UI.
- [ ] Encja zmienia stan metodą i odrzuca niedozwolone przejście.
- [ ] Oczekiwany wynik biznesowy ma jawny kod zamiast wyjątku sterującego przepływem.
- [ ] Value object ma jednoznaczną równość i walidację konstrukcji.
- [ ] Use case zależy od małych portów, nie od EF, hosta ani konkretnego transportu.
- [ ] Interfejs powstał z powodu granicy lub zmienności, nie automatycznie dla każdej klasy.
- [ ] Użyto `TimeProvider` zamiast czasu lokalnego i jawnie obsłużono UTC.

## Jeśli zmiana dotyczy ASP.NET Core API

- [ ] Endpoint wykonuje binding, authorization, walidację, delegowanie i mapping; logika biznesowa jest poza hostem.
- [ ] Auth jest fail-closed, a resource authorization uwzględnia tenant i konkretny rekord.
- [ ] Request ma limity rozmiaru i czasu adekwatne do payloadu.
- [ ] Oczekiwane błędy mapują się na spójny `ProblemDetails` ze stabilnym kodem.
- [ ] Kontrakt i OpenAPI nie ujawniają typu infrastruktury ani sekretu.
- [ ] Unsafe operation przy cookie auth ma ochronę CSRF.
- [ ] Cancellation requestu dociera do I/O, ale nie anuluje nieodwracalnego commitu w niekontrolowanym momencie.
- [ ] Liveness nie odpytuje zależności; readiness sprawdza tylko zależności konieczne.

## Jeśli zmiana dotyczy EF Core

- [ ] `DbContext` pozostaje scoped/unit-of-work i nie jest używany równolegle.
- [ ] Odczyt projektuje tylko potrzebne kolumny, ma limit/paginację i używa no-tracking, jeśli nie nastąpi zapis.
- [ ] Sprawdzono wygenerowany SQL, indeksy i liczbę round trips.
- [ ] Wybór single/split query uwzględnia spójność, round trips i cartesian explosion.
- [ ] Modyfikowany rekord ma właściwą ochronę optimistic concurrency.
- [ ] Transakcja jest krótka i nie obejmuje HTTP, LLM ani oczekiwania na człowieka.
- [ ] Operacja powtarzalna ma idempotency key; publikacja po commit używa outbox.
- [ ] Migracja została przejrzana pod kątem data loss i ma plan wdrożenia poza startupem aplikacji.

## Jeśli zmiana wywołuje zewnętrzne HTTP

- [ ] Użyto jawnie skonfigurowanego named albo typed clienta.
- [ ] Typed client nie został przechwycony przez singleton.
- [ ] Base address, dozwolona domena, timeout i limity połączeń są skonfigurowane.
- [ ] Retry obejmuje tylko błędy przejściowe i idempotentne wywołania.
- [ ] Retry unsafe method jest wyłączony albo chroniony idempotency key i weryfikacją wyniku.
- [ ] Zewnętrzna odpowiedź jest walidowana jako niezaufana.
- [ ] Auth headers, cookies i payload nie trafiają do logów ani spans.

## Jeśli zmiana dotyczy workera lub długiego workflow

- [ ] HTTP request jedynie inicjuje pracę i nie posiada lifetime workflow.
- [ ] Background operation ma trwałego właściciela, cancellation i obserwowalne zakończenie.
- [ ] Scope DI jest tworzony i usuwany dla jednostki pracy.
- [ ] Nie ma nieobserwowanego fire-and-forget.
- [ ] Retry, checkpoint i recovery nie powtarzają nieidempotentnego skutku.
- [ ] Shutdown kończy lub bezpiecznie checkpointuje pracę w ograniczonym czasie.
- [ ] Status i postęp mają correlation IDs oraz stabilny kontrakt zdarzeń.

## Jeśli zmiana ma skutek biznesowy albo granicę bezpieczeństwa

- [ ] Poziom ryzyka jest ustalony deterministycznie poza LLM.
- [ ] Uprawnienia, domena, arguments schema i page revision są walidowane przed wykonaniem.
- [ ] Approval jest wymagany tam, gdzie nakazuje polityka, i związany z hashem całej intencji.
- [ ] Zmiana argumentu, rekordu, środowiska albo rewizji unieważnia approval.
- [ ] Skutek ma idempotency key i niezależną weryfikację biznesową.
- [ ] Audit zawiera decyzję, wykonanie i wynik, ale nie sekret ani pełne dane wrażliwe.
- [ ] Prompt injection, złośliwy payload i egress poza allowlistę mają test negatywny.

## Jeśli zmiana jest na hot path albo przetwarza duże dane

- [ ] Istnieje baseline i mierzalny budżet wydajności.
- [ ] Payload ma limit i jest streamowany, jeśli materializacja może być duża.
- [ ] Nie dodano cache bez polityki invalidacji, limitu i izolacji tenantów.
- [ ] `ValueTask`, pooling, source generation albo alokacyjne optymalizacje mają dowód z pomiaru.
- [ ] Metryki używają niskokardynalnych wymiarów.
- [ ] Optymalizacja nie osłabiła czytelności ani poprawności bez mierzalnej korzyści.

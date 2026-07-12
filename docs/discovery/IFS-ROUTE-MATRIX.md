# Wstępna route matrix IFS dla procesów referencyjnych

## Status

`BLOKADA`: nie udostępniono zatwierdzonego eksportu `$openapi`, profilu sandboxu IFS ani wyników permission check. Zgodnie z zasadą API-first i fail-closed wszystkie trasy wykonawcze pozostają `deny/stop`. Tabela zapisuje hipotezę do zweryfikowania, nie registry produkcyjny.

| Proces | Kandydat API | Kandydat UI | Preferowana trasa po potwierdzeniu | Bieżąca trasa | Verifier |
|---|---|---|---|---|---|
| R0 — odczyt faktury | projection/entity set faktur klienta; nazwa niepotwierdzona | semantyczny grid faktur | `ProjectionApi` | `deny/stop` | ponowny odczyt identity/status/amount/currency |
| R2 — preview dostarczenia | projection faktury, dokumentu i konfiguracji komunikacji; nazwy niepotwierdzone | widok szczegółów i preview bez commit | `ProjectionApi`, ewentualnie `Hybrid` tylko dla preview UI | `deny/stop` | niezależny odczyt statusu dokumentu i odbiorcy |
| R4 — wysłanie faktury | action/function komunikacji; istnienie i idempotencja niepotwierdzone | jawny finalny commit w UI | `ProjectionApi` jeśli API daje commit i historię, w przeciwnym razie zatwierdzony `Hybrid` | `deny/stop` | communication/history ID + świeży odczyt API; przy Hybrid także UI |

## Dane wymagane do inwentaryzacji `DISC-010`

1. Zatwierdzony `BaseUri`, environment kind, tenant, locale i release update sandboxu.
2. Eksport `$openapi` z checksumą, datą i ownerem administracyjnym.
3. Projection, entity sets, functions/actions, methods, query options i limity payloadu.
4. Permissions użytkownika testowego bez szerszego service account.
5. ETag/concurrency, idempotency oraz zachowanie timeout przed i po wysłaniu.
6. Wspierane customizations i kanały dostarczenia.
7. Niezależny history/audit endpoint albo inny verifier biznesowy.

Po otrzymaniu danych ekspert IFS wpisuje potwierdzone nazwy i wersje do wersjonowanego registry. Samo wystąpienie endpointu w `$openapi` nie nadaje mu uprawnienia do wykonania.

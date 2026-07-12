# Raport wykonania `DISC-001`–`DISC-010`

## Środowisko pomiarowe

- data: 2026-07-12;
- host: macOS/Darwin arm64, 12 logicznych CPU, 36 GiB pamięci;
- .NET SDK: 10.0.103;
- Microsoft.Playwright: 1.61.0;
- Chrome for Testing: 149.0.7827.55;
- dane: wyłącznie syntetyczne rekordy `Praxiara.TestSites`;
- IFS: brak udostępnionego sandboxu, profilu i `$openapi`.

## Status kroków

| Krok | Status | Wykonany rezultat | Warunek zamknięcia |
|---|---|---|---|
| `DISC-001` | `BLOKADA` | utworzono macierz ról, odpowiedzialności i zastępstw | sponsor przypisuje osoby i zatwierdza macierz |
| `DISC-002` | `DO AKCEPTACJI` | zdefiniowano scenariusz R0, wejścia, wyniki, błędy, audit i verifier | podpis eksperta IFS i właściciela procesu |
| `DISC-003` | `DO AKCEPTACJI` | zdefiniowano scenariusz R2 bez zapisu oraz granice danych | podpis eksperta IFS i właściciela procesu |
| `DISC-004` | `DO AKCEPTACJI` | zdefiniowano R4 z approval hash, idempotencją, verifierem i `OutcomeUnknown` | podpis eksperta IFS i właściciela procesu |
| `DISC-005` | `BLOKADA ŚRODOWISKA` | potwierdzono tag/digest obrazu i oficjalne wymagania non-root/seccomp; dwie próby `docker pull` zatrzymały się bez pobrania warstw | uruchomienie obrazu w działającym runtime i test UID/sandbox/process cleanup |
| `DISC-006` | `CZĘŚCIOWO` | wykonano trzy pomiary lokalnej sesji headless Chromium | powtórzenie wewnątrz kontenera ukończonego w `DISC-005` |
| `DISC-007` | `CZĘŚCIOWO` | dodano wirtualizowany grid, ARIA snapshot i automatyczny test zmiany okna | ekspert potwierdza reprezentatywność na prawdziwym sandboxie IFS |
| `DISC-008` | `CZĘŚCIOWO` | porównano architekturę, bezpieczeństwo, licencje i operacyjność; zapisano protokół benchmarku | porównywalny pomiar noVNC/WebRTC w sieci referencyjnej |
| `DISC-009` | `DO AKCEPTACJI` | utworzono proponowany ADR-014 z WebRTC jako targetem i noVNC jako fallbackiem | brakujące benchmarki i podpisy product/architecture/security |
| `DISC-010` | `BLOKADA IFS` | utworzono fail-closed route matrix i listę wymaganych danych | zatwierdzony `$openapi`, profil sandboxu, permissions i podpis eksperta |

Żaden krok wymagający zewnętrznej decyzji lub prawdziwego IFS nie został oznaczony jako ukończony.

## Wyniki lokalnego baseline Chromium

| Próba | Charakter | Ready | Procesy browser | RSS | CPU |
|---:|---|---:|---:|---:|---:|
| 1 | cold | 1883 ms | 4 | 289.7 MB | 207.6 ms |
| 2 | warm | 329 ms | 4 | 289.2 MB | 190.2 ms |
| 3 | warm | 375 ms | 4 | 289.7 MB | 210.1 ms |

Mediana ready wyniosła 375 ms, a RSS około 289–290 MB. Jest to koszt małego, syntetycznego gridu w headless Chromium na hoście macOS. Wynik nie obejmuje X11, streamingu, trace, prawdziwego IFS, wielu kart ani izolacji kontenera i nie może być użyty jako capacity model produkcyjny.

## Obraz Playwright do spike

- tag: `mcr.microsoft.com/playwright/dotnet:v1.61.0-noble`;
- manifest list digest: `sha256:2e07f0f39ef0d68a6ad6fae5124957e5f5730b0c84d5187bed950a67c4d07f28`;
- linux/arm64 manifest digest: `sha256:151b3b7c40c2da1253f3147b982527471f6ac6e34862b50c7e8edf81ebc6c9a7`;
- oficjalna dokumentacja zastrzega obraz jako test/development i wymaga osobnego użytkownika oraz seccomp dla Chromium sandbox na niezaufanych stronach;
- obraz nie został dodany do manifestu produkcyjnego ani uznany za gotowy Browser Worker.

Polecenia `docker pull` przez lokalny Docker Engine nie pobrały warstw i zostały zakończone po braku postępu. Registry HTTPS odpowiadało poprawnie i zwróciło manifesty, dlatego blokada dotyczy ścieżki lokalnego engine/registry, nie istnienia tagu.

## Dowody repozytoryjne

- `docs/discovery/OWNERSHIP.md`;
- `docs/discovery/PROCESY_REFERENCYJNE-IFS.md`;
- `docs/discovery/IFS-ROUTE-MATRIX.md`;
- `docs/discovery/POROWNANIE-NOVNC-WEBRTC.md`;
- `docs/adr/ADR-014-STREAMING-PRZEGLADARKI.md`;
- `docs/discovery/evidence/ifs-grid-aria-snapshot.yml`;
- `tests/Praxiara.TestSites/IfsCustomerInvoiceGridPage.cs`;
- `tests/Praxiara.Browser.Tests/IfsGridSemanticObservationTests.cs`;
- `tools/Praxiara.BrowserSpike`.

## Reprodukcja

```bash
dotnet test tests/Praxiara.Browser.Tests/Praxiara.Browser.Tests.csproj
dotnet run --project tools/Praxiara.BrowserSpike/Praxiara.BrowserSpike.csproj
dotnet run --project tools/Praxiara.BrowserSpike/Praxiara.BrowserSpike.csproj -- --snapshot-only
```

Nie uruchamiano produkcyjnego IFS, nie użyto danych klienta i nie utrwalono sesji uwierzytelnionej.

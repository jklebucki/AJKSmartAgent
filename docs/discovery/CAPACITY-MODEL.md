# Wstępny capacity model sesji Chromium i inferencji

## Status

`DISC-017` pozostaje otwarty, bo bazuje na częściowych wynikach `DISC-006` i `DISC-014`. Ten dokument jest roboczym modelem planistycznym, a nie produkcyjnym budżetem SLO.

## Środowisko źródłowe

- data pomiarów: 2026-07-12;
- host: macOS/Darwin arm64, 12 logicznych CPU, 36 GiB RAM;
- .NET SDK: 10.0.103;
- Playwright: 1.61.0;
- model: `gpt-oss:20b` przez Ollama 0.30.10;
- dane: wyłącznie syntetyczne fixtures.

## Baseline Chromium

| Próba | Ready | Procesy browser | RSS | CPU |
|---:|---:|---:|---:|---:|
| 1 | 1883 ms | 4 | 289.7 MB | 207.6 ms |
| 2 | 329 ms | 4 | 289.2 MB | 190.2 ms |
| 3 | 375 ms | 4 | 289.7 MB | 210.1 ms |

Do planowania przyjmij tymczasowo 350-450 MB RAM na prostą sesję headless po doliczeniu marginesu na trace, większy DOM i narzut workera. Nie jest to budżet dla prawdziwego IFS, streamingu ani wielu kart.

## Baseline inferencji

| Metryka | Wynik |
|---|---:|
| liczba eval cases | 10 |
| pass rate | 100% |
| czas łączny | 47045 ms |
| min latency | 2448 ms |
| max latency | 10300 ms |
| rozmiar warstwy wag | 13793422144 bajtów |

Do planowania przyjmij, że lokalny `gpt-oss:20b` jest zasobem ciężkim i nie powinien być traktowany jako tani per-task sidecar. Wymagany jest admission control dla inferencji, kolejka i osobny limit równoległości od sesji browser.

## Wstępne limity bezpieczeństwa

| Zasób | Limit roboczy do kolejnego pomiaru | Uzasadnienie |
|---|---:|---|
| sesja Chromium per użytkownik | 1 aktywna domyślnie | izolacja i mniejszy blast radius |
| równoległe tool calls LLM na host | 1-2 | duży model lokalny i brak pomiaru concurrency |
| krok agenta bez postępu | niski limit powtórzeń | ochrona przed pętlą i kosztami |
| R4/R5 | zawsze zatrzymanie na approval | niezależne od capacity |

## Braki do zamknięcia

- pomiar Chromium w docelowym kontenerze non-root z sandboxem;
- pomiar prawdziwego IFS grid, popupów, iframe i upload/download;
- pomiar WebRTC/noVNC z aktywnym live view;
- pomiar równoległej inferencji i wpływu na browser;
- zdefiniowane SLO p50/p95/p99 dla startu sesji, obserwacji, tool call i takeover.

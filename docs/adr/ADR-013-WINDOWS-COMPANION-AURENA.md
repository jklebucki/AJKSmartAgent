# ADR-013: Windows companion dla funkcji Aurena Agent

- Status: proponowany, niezatwierdzony
- Data propozycji: 2026-07-12
- Zakres: funkcje IFS Cloud zależne od Aurena Agent lub lokalnego urządzenia użytkownika

## Kontekst

Plan produktu zakłada ścieżkę integracji z IFS Aurena Agent na zarządzanej stacji Windows. Jednocześnie Praxiara nie może wykonywać lokalnych poleceń, surowego shella, dowolnego JavaScriptu ani operacji na plikach hosta na podstawie decyzji modelu. Browser Worker działa jako izolowany proces serwerowy i nie ma prawa stawać się niekontrolowanym proxy do urządzenia użytkownika.

`DISC-011` nie został zamknięty, ponieważ nie ma w tym repozytorium zarządzanej stacji Windows, konfiguracji companion ani sandboxu IFS z procesem wymagającym Aurena Agent.

## Proponowana decyzja

1. Windows companion nie wchodzi do pierwszego GA jako wymagana ścieżka wykonawcza.
2. Pierwsze GA może obsługiwać tylko procesy, które mają trasę `ProjectionApi`, `Browser` albo jawnie zatwierdzoną `Hybrid` bez lokalnego companion.
3. Funkcja wymagająca Aurena Agent bez zatwierdzonego companion kończy się `unsupported/stop`.
4. Companion może wejść do zakresu późniejszego release tylko po osobnym ADR, security review, device registration, allowliście komend, approval i audycie urządzenia.
5. Serwerowy Chromium, download/upload, lokalna ścieżka, shell lub Playwright nie są fallbackiem dla Aurena Agent.

## Konsekwencje

- Część procesów IFS może być wyłączona z pierwszego GA, jeśli nie ma oficjalnego API ani bezpiecznej trasy UI.
- Route registry musi umieć oznaczyć etap jako wymagający companion i zatrzymać workflow deterministycznie.
- Dokumentacja procesu musi rozdzielać ograniczenie produktu od błędu wykonania.
- Właściciel produktu musi zaakceptować ograniczenie zakresu albo dostarczyć zatwierdzony program companion przed GA.

## Warunki akceptacji

- `DISC-011` ma raport z zarządzanej stacji Windows lub formalną decyzję o wyłączeniu takich procesów.
- Security zatwierdza model zaufania urządzenia, kanał mTLS, TTL, owner lease i audyt.
- Właściciel procesu wskazuje minimalny proces IFS, który faktycznie wymaga Aurena Agent.
- Powstaje test negatywny: brak companion kończy się `unsupported/stop`, bez alternatywnego obejścia.

Do spełnienia tych warunków ADR pozostaje propozycją, a `DISC-012` i `GOV-014` pozostają otwarte.

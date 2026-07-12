# ADR-002: Playwright jako główny protokół sterowania przeglądarką

- Status: proponowany, wymaga potwierdzenia reprezentatywnego gridu IFS
- Data propozycji: 2026-07-12
- Zakres: wykonanie akcji i obserwacje w Browser Workerze

## Kontekst

Praxiara steruje niezaufaną stroną wyłącznie przez Browser Worker. `DISC-007` dostarczył syntetyczny spike semantycznej obserwacji gridu, ale nie został potwierdzony na sandboxie IFS. Plan produktu wskazuje Playwright .NET jako główne API z autowaitingiem, semantycznymi locatorami, izolowanymi contextami i tracingiem.

## Proponowana decyzja

1. Playwright .NET jest jedynym standardowym API wykonującym obserwacje i typowane akcje przeglądarkowe.
2. Akcje używają kolejno ról i nazw ARIA, labeli, tekstu oraz stabilnych atrybutów; CSS, wizja i współrzędne są wyjątkami wymagającymi jawnego uzasadnienia.
3. Bezpośredni CDP jest dopuszczalny wyłącznie dla udokumentowanej diagnostyki albo strumienia obrazu, poza ścieżką akcji biznesowej.
4. UI, LLM i zewnętrzni konsumenci nie otrzymują obiektu Playwright, CDP ani WebSocketu przeglądarki.
5. Każde kliknięcie, wypełnienie lub nawigacja wymaga aktualnej referencji elementu związanej z rewizją obserwacji.

## Konsekwencje

- Browser Worker potrzebuje własnego adaptera Playwright, kontraktów obserwacji i testów stale reference.
- Zmiana DOM po obserwacji nie może prowadzić do retry starego kliknięcia; wymagane jest re-observe.
- Nietypowe funkcje niewspierane przez Playwright są osobnymi decyzjami architektonicznymi, nie prywatnym obejściem w skillu.

## Warunki akceptacji

- ekspert IFS potwierdza reprezentatywność gridu i snapshotu z `DISC-007` na sandboxie;
- testy obejmują ARIA, wirtualizację, popup, iframe, upload/download i stale reference;
- istnieje test zakazujący wykonania biznesowej akcji przez surowe CDP.

Do spełnienia warunków `GOV-003` pozostaje otwarte.

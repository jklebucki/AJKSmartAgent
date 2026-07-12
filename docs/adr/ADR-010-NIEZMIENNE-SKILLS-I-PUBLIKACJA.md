# ADR-010: Niezmienne skills i kontrolowana publikacja

- Status: proponowany, wymaga akceptacji procesów referencyjnych i ownerów
- Data propozycji: 2026-07-12
- Zakres: definicje procedur Praxiary, lifecycle i provenance

## Kontekst

Skills są danymi sterującymi automatyzacją, nie wykonywalnymi promptami ani skryptami. `DISC-002`–`DISC-004` opisały trzy syntetyczne procesy referencyjne, lecz nie zostały podpisane przez ekspertów i właściciela procesu. Wersja skillu musi dać audytowi możliwość odtworzenia zastosowanych reguł.

## Proponowana decyzja

1. Skill ma stabilne `id`, SemVer, scope site/environment, ownera, inputs, permissions, risk, preconditions, kroki, assertions, approval points, postconditions i recovery.
2. Opublikowana `SkillVersion` jest immutable i zawiera canonical checksum, provenance, compatibility oraz podpis wymagany polityką publikacji.
3. Lifecycle przebiega przez `Draft`, `Validated`, `InReview`, `Approved`, `Published`, `Deprecated` i `Revoked`; publikacja wymaga semantycznej walidacji i review eksperta.
4. Kroki skillu odwołują się wyłącznie do opublikowanego typed tool catalog oraz jawnie określają trasę `ProjectionApi`, `Browser` albo `Hybrid`.
5. Skill nie zawiera arbitralnego JavaScriptu, shella, XPath ani współrzędnych jako normalnej ścieżki; dane wrażliwe są oznaczone i redagowane w recordingach.

## Konsekwencje

- Zmiana procedury powstaje jako nowa wersja; zadanie i audit wskazują dokładną używaną wersję.
- Recorder generuje jedynie draft i nie może publikować bez review, testu replay, changelogu i możliwości rollbacku.
- Revoke zatrzymuje nowe wykonania, lecz zachowuje evidence historycznych tasków.

## Warunki akceptacji

- `DISC-002`–`DISC-004` otrzymują akceptację ekspertów IFS i ownera procesu;
- schema i validator odrzucają brak verifiera, nieznane tool/permission/risk oraz R4/R5 bez approval;
- testy potwierdzają N/N-1 compatibility, niezmienność Published i replay offline bez live effect.

Do spełnienia warunków `GOV-011` pozostaje otwarte.

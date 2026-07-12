# ADR-014: WebRTC jako kanał docelowy i noVNC jako fallback

- Status: proponowany, wymaga zatwierdzenia właściciela produktu, architekta i security
- Data propozycji: 2026-07-12
- Zakres: live view i manual takeover Browser Workera

## Kontekst

Praxiara wymaga obrazu aktywnej karty, niskiego opóźnienia wejścia i wyłącznego lease sterowania. Kanał mediów nie może udostępniać CDP, Playwright WebSocket ani nieautoryzowanego pulpitu. Plan wyznacza SLO przejęcia do 2 s i input latency p95 do 250 ms w sieci referencyjnej.

## Proponowana decyzja

1. LiveKit/WebRTC jest docelowym kanałem produkcyjnym live view.
2. Input klawiatury i myszy przechodzi osobnym, uwierzytelnionym kontraktem control plane, nie jest zaufany na podstawie pokoju WebRTC.
3. noVNC pozostaje wyłączonym domyślnie fallbackiem diagnostycznym i developmentowym, uruchamianym dla konkretnej sesji z krótkim TTL.
4. Ani VNC 5900, ani websockify 6080, ani LiveKit admin API nie są publiczne.
5. Każdy wariant wiąże user, tenant, task, session, owner lease i fencing token; zwrot kontroli wymusza nową obserwację.

## Uzasadnienie

WebRTC oferuje transport projektowany dla niskich opóźnień, adaptacji i pracy przez ICE/TURN. LiveKit Server ma licencję Apache-2.0. noVNC jest prostszy diagnostycznie, ale stos noVNC/websockify/TigerVNC wprowadza obowiązki MPL-2.0/LGPL-3.0/GPL-2.0, dodatkowy pulpit X/VNC i gorszą charakterystykę dynamicznego obrazu.

## Konsekwencje

- Potrzebne są publisher/encoder, LiveKit, TURN, krótkie exact-room grants, monitoring RTT/loss/jitter i jawne porty mediów.
- noVNC wymaga osobnego profilu compliance, source/notices, prywatnych portów, auth proxy i ograniczeń clipboard/file transfer.
- Aktywna sesja nie jest migrowana między hostami ani providerami streamingu.
- Recording jest wyłączony domyślnie i wymaga osobnej polityki danych.

## Dowody brakujące do akceptacji

- pomiar end-to-end latency obu wariantów w tej samej sieci referencyjnej i przy tej samej rozdzielczości/fps;
- CPU, RAM i bandwidth publishera/noVNC na reprezentatywnym gridzie IFS;
- test NAT/TURN oraz auth/TTL/cross-session isolation;
- formalny przegląd obowiązków dystrybucyjnych noVNC/websockify/TigerVNC;
- podpisy ownerów wymaganych przez `docs/discovery/OWNERSHIP.md`.

Do czasu zebrania dowodów `DISC-009` pozostaje otwarte, a dokument nie jest zatwierdzoną decyzją produkcyjną.

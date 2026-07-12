# Porównanie noVNC i WebRTC dla live view oraz takeover

## Zakres i status

Porównanie obejmuje architekturę, bezpieczeństwo, licencje i operacyjność. Pomiar latency end-to-end nie został wykonany, ponieważ profile `stream-novnc` i `stream-webrtc` nie mają jeszcze usług wykonawczych w Compose. `DISC-008` pozostaje otwarte do czasu uruchomienia obu wariantów na tej samej sesji i sieci referencyjnej.

## Porównanie

| Kryterium | noVNC | LiveKit/WebRTC |
|---|---|---|
| Transport obrazu | framebuffer VNC przez WebSocket | media WebRTC przez SFU/ICE/TURN |
| Charakterystyka opóźnienia | prosta, ale zależna od encoding VNC, zmian ekranu i WebSocket | projektowana dla realtime; adaptacja bitrate i ścieżki UDP/TCP/TURN |
| Sterowanie | VNC przenosi obraz i input; Praxiara nadal wymaga zewnętrznego lease/auth | media oddzielone od inputu; input przechodzi control plane |
| Izolacja | VNC 5900 i websockify 6080 wyłącznie prywatne; token per session | exact-room JWT, publisher/subscriber grants i osobny input authorization |
| NAT/firewall | WSS przez jeden ingress upraszcza sieć | wymaga poprawnego ICE, UDP/TCP i testowanego TURN |
| Diagnostyka | łatwy podgląd całego pulpitu X; dobry fallback developerski | wymaga rozdzielenia publishera, SFU, ICE/TURN i klienta |
| Skalowanie | sesja związana z workerem; CPU/bandwidth rosną z encoding i fps | SFU wspiera skalowanie, ale encoder/publisher i bandwidth są istotnym kosztem |
| Licencje | noVNC MPL-2.0, websockify LGPL-3.0, TigerVNC zawiera komponenty GPL i notices | LiveKit Server Apache-2.0; zależności encoder/codec wymagają osobnego SBOM |
| Produkcja | wyłączony domyślnie fallback z TTL, auth i prywatnym VNC | rekomendowany kanał docelowy po pomiarach i testach TURN |

## Model zagrożeń wspólny

- Room name, VNC password ani URL nie są samodzielną autoryzacją.
- Token wiąże user, tenant, task, session i purpose; ma krótki TTL i revoke.
- Agent oraz użytkownik nigdy nie wysyłają inputu równocześnie.
- Zwrot sterowania zwiększa generację sesji, unieważnia refs i wymusza reobserve.
- Recording, clipboard i transfer plików są wyłączone domyślnie.
- Porty sterujące, VNC, websockify i LiveKit admin API nie są publiczne.

## Rekomendacja

Przyjąć propozycję ADR-014: LiveKit/WebRTC jako target produkcyjny, noVNC jako kontrolowany fallback diagnostyczny. Rekomendacja jest warunkowa i nie stanowi akceptacji `DISC-009` bez porównywalnego pomiaru p50/p95, CPU, RAM, bitrate, packet loss i input latency.

## Protokół brakującego benchmarku

1. Uruchomić ten sam Browser Worker, viewport 1920×1080 i syntetyczny grid IFS.
2. Dla każdego wariantu zebrać co najmniej 30 prób: time-to-first-frame, input→visual-change, CPU, RSS i bitrate.
3. Powtórzyć w LAN, z wymuszonym TURN i przy 1% packet loss.
4. Potwierdzić cross-session deny, expiry tokenu i zamknięcie kanału po lease revoke.
5. Porównać p50/p95 z SLO takeover do 2 s i input latency p95 do 250 ms.

## Źródła pierwotne

- [Playwright .NET — Docker i noVNC](https://playwright.dev/dotnet/docs/docker)
- [LiveKit Server — repozytorium i licencja](https://github.com/livekit/livekit)
- [LiveKit — łączenie klienta](https://docs.livekit.io/intro/basics/connect/)
- [noVNC — repozytorium](https://github.com/novnc/noVNC)
- [websockify — repozytorium i licencja](https://github.com/novnc/websockify)
- [TigerVNC — licencje komponentów](https://github.com/TigerVNC/tigervnc/blob/master/LICENCE.TXT)

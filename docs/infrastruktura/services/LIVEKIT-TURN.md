# LiveKit i TURN — strumień WebRTC

## Cel i zakres

LiveKit dostarcza niskolatencyjny obraz Browser Workera do React UI. Użytkownik może obserwować zadanie i przejąć sterowanie. Media i wejście użytkownika są rozdzielone: LiveKit przenosi audio/video, a komendy myszy/klawiatury przechodzą uwierzytelnionym kanałem aplikacji do konkretnej sesji.

## Wybór i licencje

- LiveKit Server i oficjalny JS client: Apache-2.0.
- LiveKit GStreamer publisher: Apache-2.0.
- coturn, jeśli używany osobno: BSD-3-Clause.

Źródła: [LiveKit Server](https://github.com/livekit/livekit), [JS ecosystem](https://github.com/livekit), [GStreamer publisher](https://github.com/livekit/gstreamer-publisher), [coturn](https://github.com/coturn/coturn).

## Architektura

```text
Chromium/X11/PipeWire
        |
GStreamer publisher
        |
LiveKit room (one browser session)
        |
React LiveKit client

React input -> Praxiara API/SignalR -> authorized Browser Worker input
```

Pokój nie jest globalnym kanałem. Każda browser session ma osobny, nieprzewidywalny room name, owner i krótko żyjące token grants.

## Wersja i pinning

- LiveKit Server i clients są na testowanej kompatybilnej linii.
- Obraz server/publisher przypięty po digest.
- JS client przypięty w lockfile pnpm.
- Codec/GStreamer dependencies są objęte SBOM; nie każdy codec ma te same warunki patentowe/licencyjne, dlatego bazowo preferujemy powszechnie wspierany zestaw po przeglądzie obrazu.

## Topologie

### Development

- jedna instancja LiveKit;
- self-signed/local TLS przez Caddy;
- mały zakres UDP;
- TURN opcjonalny, ale testowany okresowo zza realnego NAT;
- tokeny tworzy lokalny API.

### Hardened single-node

- LiveKit i publisher na prywatnej `stream-net`;
- signaling WSS przez Caddy;
- media UDP/TCP jawnie otwarte na firewallu;
- TLS/TURN credentials z OpenBao;
- limity pokoi/participants/bitrate;
- Valkey tylko jeśli wymaga tego wybrana topologia;
- brak recording domyślnie.

### HA

- wiele LiveKit nodes na osobnych hostach;
- Valkey jako distributed state zgodnie z dokumentacją LiveKit;
- load balancing i prawidłowe public/node IP;
- TURN redundantny;
- osobne failure domains i test utraty media node;
- browser session może wymagać reconnect, nie obiecujemy bezprzerwowej migracji publishera.

## Zasoby startowe

Zależą od rozdzielczości, fps, codec i liczby sesji.

| Obciążenie | CPU/RAM start |
|---|---|
| 1–5 sesji 1080p/niski fps | 2–4 CPU, 1–2 GiB LiveKit + encoder capacity |
| większa liczba | benchmark per codec i host |

Największy koszt zwykle ponosi encoder/publisher i bandwidth. Monitorujemy bitrate, packet loss, RTT, jitter, NACK/PLI i CPU encoder.

## Tokeny i sekrety

```text
/run/secrets/livekit_api_key
/run/secrets/livekit_api_secret
/run/secrets/turn_shared_secret
/run/secrets/livekit_tls_cert
/run/secrets/livekit_tls_key
```

API key/secret zna wyłącznie trusted backend. React otrzymuje JWT:

- TTL minutowy;
- exact room;
- `canSubscribe=true` dla użytkownika;
- `canPublish=false` jeśli użytkownik nie publikuje media;
- identity związane z user/session;
- brak admin grants.

Publisher ma `canPublish=true`, `canSubscribe=false` i exact room. Token po zakończeniu sesji traci ważność, a room jest zamykany.

## Konfiguracja aplikacji

```text
Streaming__Provider=livekit
Streaming__PublicUrl=wss://stream.example.com
Streaming__TokenTtlSeconds=120
Streaming__RoomPrefix=praxiara-browser-
Streaming__MaxWidth=1920
Streaming__MaxHeight=1080
Streaming__MaxFramesPerSecond=30
Streaming__IdleFramesPerSecond=2
```

Input events zawierają session ID, monotonically increasing sequence, viewport dimensions i auth context. Backend odrzuca eventy starej/nieaktywnej sesji oraz użytkownika bez owner/takeover permission.

## Sieć i porty

Typowo:

- 7880/TCP signaling/API, przez Caddy;
- 7881/TCP ICE/TCP;
- ustalony zakres UDP media;
- TURN 3478 UDP/TCP i 5349 TLS;
- ograniczony relay UDP range.

Dokładny zakres jest zapisany w `deploy/compose/config/livekit/livekit.yaml` i firewall IaC. Caddy nie zastępuje routingu UDP. `use_external_ip`/node IP jest ustawione świadomie; błędne autodiscovery często daje połączenie tylko wewnątrz LAN.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile browser \
  --profile stream-webrtc \
  up -d --wait livekit
```

Publisher jest częścią konkretnego Browser Workera/session, nie globalnym procesem pokazującym wiele pulpitów.

## Health i smoke test

Health LiveKit sprawdza process/readiness. Funkcjonalny smoke test:

1. Backend tworzy room i dwa minimalne tokeny publisher/subscriber.
2. Test publisher wysyła kontrolną planszę z timestampem.
3. Headless React/client subskrybuje track.
4. Test potwierdza odbiór klatek i poprawny room identity.
5. Token do innego room jest odrzucony.
6. Token po expiry jest odrzucony.
7. Test przez TURN działa przy zablokowanym direct UDP.
8. Metryki RTT/loss są dostępne.

Test manual takeover potwierdza, że użytkownik bez ownership/role nie może wysłać input events, nawet jeśli pozna room name.

## Recording, backup i retencja

Live stream nie jest automatycznie nagrywany. Recording wymaga:

- jawnej polityki i informacji dla użytkownika;
- klasyfikacji danych;
- krótkiej retencji;
- encryption i access control;
- wpisu audytowego z powodem.

Jeśli nagranie jest wymagane, LiveKit Egress jest Apache-2.0, a output trafia do `browser-artifacts`. Konfiguracja LiveKit, token policy i certyfikaty są backupowane; ephemeral rooms nie są odtwarzane po awarii. Workflow wznawia się przez nową browser/stream session.

## Aktualizacja i rollback

- Sprawdź compatibility server/JS/publisher.
- Wykonaj test direct UDP, ICE/TCP i TURN.
- Canary na nowych sesjach; aktywne pozostają na starej puli.
- Porównaj bitrate, CPU, packet loss i reconnect.
- Rollback kieruje nowe rooms do starej wersji; nie migruje aktywnego pokoju.
- Zmiana codec lub portów wymaga osobnego change i firewall test.

## Hardening

- krótkie exact-room JWT;
- API secret tylko backend;
- WSS/TLS i TURN TLS;
- żadnych anonimowych rooms;
- random room names plus authorization, nie security przez samą nazwę;
- input poza data channel lub z niezależną autoryzacją i sequence validation;
- limity bitrate/fps/resolution/participants;
- egress/recording wyłączone domyślnie;
- signaling za rate limit;
- port ranges minimalne i monitorowane;
- metryki i admin APIs niepubliczne;
- brak sensitive tokenów w URL/logach.

## Troubleshooting

### Działa w LAN, nie przez Internet

Sprawdź public node IP, NAT mapping, firewall UDP, ICE candidates i TURN. Nie otwieraj całego zakresu portów bez potrzeby.

### Czarny obraz

Sprawdź capture source, GStreamer pipeline, codec negotiation, track publish grants i subscriber permissions. Oddziel problem publishera od transportu przez testową planszę.

### Duże opóźnienie

Sprawdź encoder CPU, bitrate, keyframe interval, packet loss, relay TURN i fps. UI może obniżać fps w trybie observe/idle.

### Użytkownik widzi cudzą sesję

To incydent krytyczny. Natychmiast unieważnij tokeny/rooms, zatrzymaj streaming, zabezpiecz audit i sprawdź binding user-session-room oraz cache. Room name nie może być jedynym zabezpieczeniem.

### TURN nie działa

Zweryfikuj realm, shared secret, czas, TLS cert, relay range i public IP. Testuj z sieci rzeczywiście blokującej direct path.

## Różnice produkcyjne

Dev może działać na jednym hoście i bez TURN. Produkcja wymaga publicznie poprawnego ICE/TURN, TLS, krótkich grants, firewallu, monitoringu jakości i limitów. HA wymaga wielu media/TURN nodes i Valkey na osobnych failure domains.

# noVNC fallback

## Cel i ograniczenie

noVNC jest diagnostycznym i awaryjnym kanałem podglądu/przejęcia Browser Workera. Nie jest domyślnym targetem produkcyjnym; targetem jest LiveKit/WebRTC. Fallback pozostaje przydatny podczas debugowania codec/ICE, w środowisku lokalnym oraz przy ograniczonych wymaganiach fps.

## Komponenty i licencje

- noVNC: MPL-2.0.
- websockify: LGPL-3.0.
- TigerVNC: GPL-2.0 i dodatkowe komponenty/notices.

Wszystkie pozwalają na zastosowanie komercyjne, ale dystrybucja obrazu wymaga spełnienia obowiązków weak/strong copyleft. Obraz musi zawierać licencje/notices oraz sposób uzyskania odpowiedniego source. Zespół prawny/compliance akceptuje ten profil osobno.

Źródła: [noVNC](https://github.com/novnc/noVNC), [websockify](https://github.com/novnc/websockify), [TigerVNC](https://github.com/TigerVNC/tigervnc).

## Architektura

```text
Chromium -> X display -> TigerVNC
                         |
                    websockify
                         |
                       noVNC
                         |
                 Caddy + session auth
```

VNC 5900 nie opuszcza kontenera/private network. Publiczny klient łączy się wyłącznie przez HTTPS/WSS i kontrolę sesji.

## Wersja i pinning

- Wszystkie trzy komponenty przypięte po wersji/digest.
- Nie kopiujemy przypadkowego `novnc` obrazu bez provenance i licencji.
- Preferowany własny minimalny worker image ze źródłami/notices.
- Aktualizacja wymaga testu keyboard layout, clipboard policy, resize i auth proxy.

## Topologie

### Development

- VNC/websockify wewnątrz Browser Workera;
- noVNC na `127.0.0.1` albo przez local Caddy;
- jednorazowe hasło/token session;
- clipboard może być włączony wyłącznie dla testów jawnie.

### Hardened single-node

- profil wyłączony domyślnie;
- uruchamiany dla konkretnej sesji i TTL;
- Caddy/API wydaje one-time access URL/token związany z user/session;
- VNC port prywatny;
- recording/clipboard/file transfer wyłączone;
- pełny audit takeover start/stop.

### HA

noVNC jest przypisane do hosta konkretnego Browser Workera. Routing musi być session-aware. Awaria hosta kończy pulpit; workflow tworzy nową sesję, zamiast próbować przenosić VNC state.

## Zasoby i jakość

VNC zwiększa CPU i bandwidth zależnie od encoding, fps i zmian ekranu. Dla dynamicznej strony/animacji jakość jest gorsza od WebRTC. Ustawiamy limity rozdzielczości, fps i liczby jednoczesnych klientów.

## Sekrety i dostęp

- losowy secret per session;
- TTL nie dłuższy niż browser session;
- token bound do user, tenant, session i purpose `manual-takeover`;
- single-use lub szybka rotacja po połączeniu;
- brak stałego wspólnego hasła VNC;
- URL/token nie jest logowany ani wysyłany przez e-mail.

Hasło VNC jest defense-in-depth, nie jedyną autoryzacją. Caddy/API sprawdza aktywną sesję i prawo przejęcia.

## Sieci i porty

- 5900/TCP wyłącznie localhost/wewnątrz workera.
- 6080/TCP websockify/noVNC wyłącznie private `stream-net`.
- publicznie wyłącznie WSS przez Caddy 443.
- Browser Worker nadal nie ma dostępu do data/control.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile browser \
  --profile stream-novnc \
  up -d --wait browser-worker
```

Produkcja uruchamia fallback tylko dla session z policy flag `allowNoVncFallback=true`.

## Health i smoke test

1. VNC process widzi właściwy X display.
2. websockify łączy się wyłącznie do lokalnego VNC.
3. noVNC przez Caddy wymaga auth.
4. Uprawniony użytkownik widzi kontrolną stronę i może przejąć input.
5. Nieuprawniony użytkownik/inna session otrzymuje 403.
6. Po TTL WebSocket jest zamknięty i reconnect odrzucony.
7. Port 5900/6080 nie jest widoczny z Internetu.

## Backup i restore

noVNC nie ma trwałego stanu wymagającego backupu. Config, source/notices i image digest są częścią release artifacts. Browser session/artifacts są obsługiwane zgodnie z dokumentem Browser Worker.

## Aktualizacja i rollback

- build i SBOM nowego obrazu;
- test licencji/notices;
- keyboard/mouse/resize/clipboard-off/WSS/auth smoke tests;
- canary na nowych sesjach;
- rollback do starego obrazu dla nowych workers.

Aktywna sesja nie jest migrowana między obrazami.

## Hardening

- VNC/websockify niepubliczne;
- Caddy HTTPS/WSS i auth user/session;
- krótkie tokeny;
- clipboard/file transfer/audio wyłączone, jeśli nie są wymagane;
- input tylko po explicit takeover, nie równolegle z agentem;
- agent pauzuje podczas takeover;
- audit start/stop i actor ID;
- limity klientów, fps, frame size i idle timeout;
- profile/katalogi sesji nie są współdzielone między użytkownikami;
- source offer/notices spełniają MPL/LGPL/GPL.

## Troubleshooting

### Szary/czarny ekran

Sprawdź display number, X permissions, window manager i czy Chromium działa w tym samym display. Nie uruchamiaj całego kontenera jako root.

### WebSocket 502

Sprawdź websockify target, Caddy upgrade headers, path rewrite i session routing. Nie publikuj 6080 bezpośrednio jako obejścia.

### Zły keyboard layout

Zapisz locale/layout sesji i wykonaj test znaków specjalnych używanych w IFS. Wrażliwe dane nie powinny być wklejane przez clipboard.

### Agent i użytkownik klikają równocześnie

To błąd state machine. Takeover ustawia exclusive input lease, pauzuje działania agenta i po zwrocie wymusza nowe Observe/revision.

## Różnice produkcyjne

W development noVNC może być głównym narzędziem debug. W produkcji pozostaje wyłączonym domyślnie fallbackiem z krótkim TTL, auth, audit i prywatnym VNC. Licencje copyleft są częścią release compliance.

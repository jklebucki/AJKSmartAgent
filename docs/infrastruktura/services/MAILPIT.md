# Mailpit — e-mail development i testy

## Cel i zakres

Mailpit przechwytuje wiadomości SMTP w development i testach integracyjnych. Umożliwia weryfikację szablonów, linków, załączników oraz e-maili wysyłanych przez Keycloak i Praxiara bez dostarczenia ich do prawdziwego odbiorcy.

Mailpit nie jest produkcyjnym relay ani magazynem archiwalnym. Produkcyjne SMTP jest usługą dostarczoną przez operatora i ma własne wymagania DPA, deliverability, SPF/DKIM/DMARC oraz retencję.

## Wybór i licencja

Mailpit jest MIT i bezpłatny komercyjnie. Jest aktywnie utrzymywanym następcą idei MailHog; sam projekt wskazuje, że MailHog nie jest od lat utrzymywany i nie otrzymuje security updates.

Źródła: [Mailpit](https://github.com/axllent/mailpit), [dokumentacja](https://mailpit.axllent.org/docs/).

## Wersja i pinning

- Aktualne stabilne wydanie, przypięte po digest.
- Nie używamy `latest`.
- API/UI i SMTP smoke tests są wykonywane po aktualizacji.
- Obraz nie jest częścią profilu produkcyjnego.

## Topologie

### Development

- jedna instancja;
- SMTP dostępne wewnątrz Compose na 1025;
- UI na `127.0.0.1:8025` albo przez local Caddy;
- dane nietrwałe lub mały wolumen;
- automatyczne czyszczenie.

### CI

- osobna czysta instancja per job/suite;
- test przed wysłaniem czyści inbox przez API;
- test odpytuje API zamiast polegać na czasie/renderingu UI;
- kontener jest usuwany po suite.

### Produkcja

Mailpit jest wyłączony. Nie ma host portu, profilu ani fallbacku do „tymczasowego” wysyłania e-maili. Błędne wskazanie produkcji na Mailpit musi zostać wykryte przez startup validation/health.

## Zasoby startowe

| Środowisko | CPU | RAM | Dysk |
|---|---:|---:|---|
| dev | 0.1–0.5 | 64–256 MiB | 100 MiB–2 GiB |
| CI | 0.1–0.5 | 64–256 MiB | tmpfs |

Limit liczby wiadomości, wieku i rozmiaru załączników zapobiega niekontrolowanemu wzrostowi.

## Konfiguracja aplikacji

Development:

```text
Email__Enabled=true
Email__Host=mailpit
Email__Port=1025
Email__UseTls=false
Email__FromAddress=praxiara@local.invalid
Email__EnvironmentHeader=development
```

Keycloak dev SMTP używa tego samego hosta/portu, ale osobnego from address.

Produkcja wymaga:

```text
Email__Host=<operator-smtp-host>
Email__Port=<operator-port>
Email__UseTls=true
Email__Username=<operator-user>
Email__PasswordFile=/run/secrets/smtp_password
```

Konfiguracja produkcyjna odrzuca host `mailpit`, `.local` i port 1025, chyba że environment jest jawnie development/test.

## Sieci i porty

- 1025/TCP SMTP tylko `app-net` w dev/test.
- 8025/TCP UI/API mapowane na `127.0.0.1`.
- Opcjonalne TLS/POP3 nie są włączane bez potrzeby.
- Mailpit nie jest członkiem produkcyjnego `edge-net` ani publicznego DNS.

## Sekrety

Development Mailpit zwykle nie wymaga auth, ponieważ jest prywatny. Jeśli zespół współdzieli środowisko:

- UI ma auth/reverse proxy;
- SMTP może wymagać credentials;
- credentials są z secret file;
- przechwycone e-maile są traktowane jako dane testowe potencjalnie poufne.

Nigdy nie używamy produkcyjnych list odbiorców ani realnych danych osobowych w testowych e-mailach bez zatwierdzonej anonimizacji.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile mail \
  up -d --wait mailpit
```

## Health i smoke test

Health UI/API:

```bash
curl --fail --silent http://127.0.0.1:8025/api/v1/info
```

SMTP smoke test z narzędzia testowego:

```bash
swaks \
  --server 127.0.0.1:1025 \
  --from praxiara@local.invalid \
  --to smoke@local.invalid \
  --header 'Subject: Praxiara smoke test' \
  --body 'Mailpit smoke test'
```

Następnie API potwierdza dokładnie jedną wiadomość, subject, from/to i body marker. Test usuwa wiadomość po zakończeniu.

Testy aplikacji powinny sprawdzać:

- text i HTML parts;
- poprawne escaping danych użytkownika;
- link z prawidłowym hostem środowiska i krótkim tokenem;
- attachment filename/type/size;
- brak sekretów i stack trace;
- brak przypadkowego prawdziwego odbiorcy;
- retry/timeout przy kontrolowanym błędzie SMTP.

## Retencja, backup i restore

Wiadomości Mailpit są nietrwałe i nie wymagają backupu. Ustawiamy automatyczne pruning według wieku/liczby. CI używa pustego inbox.

Backupujemy wyłącznie:

- wersję/digest obrazu;
- config bez sekretów;
- test fixtures i oczekiwane szablony;
- wyniki testów, jeśli potrzebne do release evidence.

Restore oznacza uruchomienie pustej instancji. Nie odtwarzamy historycznych testowych wiadomości.

## Aktualizacja i rollback

1. Pin i scan nowego obrazu.
2. Uruchom SMTP/API/UI smoke tests.
3. Uruchom integration tests Keycloak i aplikacji.
4. Sprawdź API compatibility używane przez testy.
5. Rollback do poprzedniego digest; dane inbox mogą zostać utracone.

## Hardening

- wyłącznie dev/test profile;
- host ports tylko `127.0.0.1`;
- UI auth w środowisku współdzielonym;
- limit message count/age/size;
- brak realnych odbiorców i produkcyjnych credentials;
- brak SMTP relay/forwarding, chyba że osobny kontrolowany test z allowlistą;
- kontener non-root, read-only tam, gdzie wspierane;
- wiadomości i załączniki nie trafiają do ogólnej telemetryki;
- UI nie jest indeksowane ani routowane publicznie.

## Troubleshooting

### Aplikacja nie łączy się z SMTP

Sprawdź, czy aplikacja używa nazwy `mailpit` wewnątrz Compose, nie `localhost`, oraz czy profil `mail` jest aktywny. Sprawdź timeout i sieć `app-net`.

### E-mail nie pojawia się w teście

Odpytuj API z retry ograniczonym czasem; SMTP accepted nie zawsze oznacza natychmiastowe odświeżenie UI. Sprawdź, czy poprzedni test wyczyścił inbox i czy filtr odbiorcy nie jest zbyt szeroki.

### Link wskazuje production

To błąd konfiguracji environment/base URL. Zablokuj wysyłkę i dodaj startup/test assertion. Nie klikaj linku w automatycznym teście przeciw produkcji.

### Inbox zajmuje dysk

Sprawdź pruning, liczbę/rozmiar attachments i test cleanup. Usuń dane przez API/bezpieczny reset instancji dev, nie przez manipulację plikami działającego procesu.

### Wiadomość zawiera dane produkcyjne

Traktuj jako incydent danych: ogranicz dostęp, usuń wiadomość zgodnie z procedurą, ustal źródło fixtures i popraw anonimizację.

## Różnice produkcyjne

Nie istnieje produkcyjna konfiguracja Mailpit. W produkcji komponent jest nieobecny, a aplikacja używa zatwierdzonego SMTP operatora z TLS, secret storage i monitorowaniem dostarczenia.

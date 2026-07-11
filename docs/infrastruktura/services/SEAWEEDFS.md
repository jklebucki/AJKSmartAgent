# SeaweedFS — magazyn obiektowy S3

## Cel i zakres

SeaweedFS przechowuje:

- screenshoty i Playwright traces;
- nagrania i artefakty sesji browsera;
- uploady i downloady po skanowaniu/quarantine;
- eksporty audytu;
- opcjonalnie zaszyfrowany stan browser session o krótkim TTL;
- backupy tylko wtedy, gdy repozytorium jest fizycznie niezależne od danych źródłowych.

Aplikacja używa własnej abstrakcji object storage i AWS S3-compatible SDK. Nie zależy od API administracyjnego SeaweedFS.

## Wybór i licencja

SeaweedFS jest aktywnym projektem Apache-2.0. Zastępuje MinIO, którego repozytorium community zostało zarchiwizowane 25 kwietnia 2026, a bieżące warunki producenta ograniczają bezumowne użycie dystrybucji do ewaluacji nieprodukcyjnej.

Źródła: [SeaweedFS](https://github.com/seaweedfs/seaweedfs), [MinIO repo](https://github.com/minio/minio), [MinIO Software License](https://docs.min.io/license/).

## Wersja i pinning

- Używamy aktualnej stabilnej linii 4.x po testach kontraktowych.
- Oficjalny obraz jest przypięty po digest.
- Dev i production używają tej samej linii serwera i S3 gateway.
- Nie zakładamy pełnej zgodności AWS S3; testujemy wykorzystywany podzbiór.

## Wymagany kontrakt S3

Przed aktualizacją przechodzą testy:

- create/head/list bucket;
- put/get/head/delete object;
- multipart upload i abort;
- range GET;
- presigned GET oraz PUT z krótkim TTL;
- ETag/hash semantics używane przez aplikację;
- content type i metadata;
- ograniczenie wielkości;
- versioning/lifecycle tylko jeśli realnie wykorzystywane;
- błędy access denied i expired signature;
- równoległe odczyty/zapisy wymaganej skali.

## Buckety i klasy danych

| Bucket | Zawartość | Domyślna retencja | Dostęp |
|---|---|---|---|
| `browser-artifacts` | screenshot, trace, nagranie | krótka, zależna od polityki | Audit/Artifact service |
| `browser-quarantine` | upload/download przed skanem | godziny/dni | scanner i Artifact service |
| `browser-sessions` | zaszyfrowany storage state | bardzo krótka | Session service per user |
| `audit-evidence` | zatwierdzone dowody audytowe | zgodna z polityką | append-oriented Audit service |
| `exports` | eksport użytkownika | krótka | właściciel i operator |
| `backups` | wyłącznie jeśli osobna failure domain | zgodna z RPO | backup role |

Każdy bucket ma osobne credentials/policy. Root key nie jest używany przez aplikację.

## Topologie

### Development/CI

- `weed mini -s3` lub równoważny tryb jednego procesu;
- jeden wolumen lokalny;
- S3 endpoint opcjonalnie na `127.0.0.1:8333`;
- idempotentny init tworzy buckety/policies;
- dane mogą być resetowane.

### Hardened single-node

- osobne procesy/role master, volume, filer i S3 albo udokumentowany single-node;
- osobne volumes i monitoring pojemności;
- filer metadata w zatwierdzonym backendzie;
- TLS na granicy, prywatny S3 endpoint;
- szyfrowanie dysku i opcjonalne application-level envelope encryption;
- backup/replication do innego systemu.

### Multi-node/HA

- nieparzyste quorum masterów na osobnych hostach;
- volume servers na osobnych failure domains;
- replication/erasure coding dobrane do RPO i charakteru obiektów;
- filer i S3 gateway redundantne za load balancerem;
- test awarii węzła, utraty dysku i odbudowy;
- niezależny backup mimo replikacji.

## Zasoby startowe

| Środowisko | CPU | RAM | Dysk |
|---|---:|---:|---|
| dev mini | 1 | 512 MiB–1 GiB | 20 GiB |
| single-node prod | 2–4 | 2–8 GiB | według artefaktów + 30% zapasu |
| HA | od 2 na rolę | od 2 GiB | wiele dysków/hostów według testu |

Najważniejsze są throughput, IOPS, rozmiar i liczba małych obiektów oraz retencja trace/screenshots.

## Sekrety i konfiguracja aplikacji

Oczekiwane:

```text
/run/secrets/s3_app_access_key
/run/secrets/s3_app_secret_key
/run/secrets/s3_audit_access_key
/run/secrets/s3_audit_secret_key
```

Konfiguracja:

```text
PRAXIARA_S3_ENDPOINT=http://seaweed-s3:8333
PRAXIARA_S3_REGION=us-east-1
PRAXIARA_S3_FORCE_PATH_STYLE=true
PRAXIARA_S3_ACCESS_KEY_FILE=/run/secrets/s3_app_access_key
PRAXIARA_S3_SECRET_KEY_FILE=/run/secrets/s3_app_secret_key
PRAXIARA_S3_BUCKET_BROWSER_ARTIFACTS=browser-artifacts
PRAXIARA_S3_BUCKET_QUARANTINE=browser-quarantine
PRAXIARA_S3_PRESIGNED_URL_TTL_SECONDS=300
```

Presigned URL ogranicza metodę, key, TTL i w miarę możliwości content length/type. Browser Worker nie otrzymuje stałych credentials.

## Sieci i porty

- Master 9333, volume 8080/18080, filer 8888, S3 8333 są prywatne.
- Aplikacja używa tylko S3 endpoint i ewentualnie dedykowanego management clienta poza request path.
- Browser Worker dociera do precyzyjnego Artifact Gateway/presigned endpoint, nie do całego `data-net`.
- Publiczny dostęp do bucketa jest zabroniony.

## Uruchomienie

Development:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  up -d --wait seaweedfs
```

Init bucketów:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm seaweedfs-init
```

Init nie usuwa ani nie opróżnia istniejącego bucketa. Rozbieżność policy kończy się czytelnym błędem albo kontrolowaną aktualizacją.

## Health i smoke test

Health każdej roli sprawdza jej własny endpoint/proces. Smoke test S3 używa runtime credentials:

```bash
aws \
  --endpoint-url http://seaweed-s3:8333 \
  s3api head-bucket \
  --bucket browser-artifacts
```

```bash
printf 'praxiara-smoke' > /tmp/praxiara-smoke.txt
aws \
  --endpoint-url http://seaweed-s3:8333 \
  s3 cp /tmp/praxiara-smoke.txt s3://browser-artifacts/smoke/praxiara-smoke.txt
aws \
  --endpoint-url http://seaweed-s3:8333 \
  s3 cp s3://browser-artifacts/smoke/praxiara-smoke.txt -
aws \
  --endpoint-url http://seaweed-s3:8333 \
  s3 rm s3://browser-artifacts/smoke/praxiara-smoke.txt
```

CI dodatkowo uruchamia multipart, range i expired presigned URL tests.

## Skanowanie upload/download

1. Obiekt trafia do `browser-quarantine` z unikalnym key.
2. Metadata w PostgreSQL ma stan `PendingScan`.
3. Scanner pobiera obiekt strumieniowo, sprawdza malware/type/size i zapisuje wynik.
4. Dopiero poprawny obiekt jest kopiowany/promowany do docelowego bucketa.
5. Obiekt zablokowany pozostaje niedostępny i jest usuwany zgodnie z krótką retencją.

Nazwa pliku użytkownika nie jest bezpośrednim object key. Content type od klienta jest niezaufany.

## Backup i restore

Replikacja nie jest backupem. Wymagane są:

- druga failure domain lub inny kompatybilny magazyn;
- wersjonowanie/lifecycle, jeśli wspiera scenariusz;
- manifest obiektów i hash krytycznych audit evidence;
- backup konfiguracji IAM/policy i listy bucketów;
- okresowy restore losowej próbki i pełny test procedury.

Nie zapisujemy backupu PostgreSQL do tego samego fizycznego SeaweedFS, którego utrata ma być scenariuszem DR, chyba że jest to tylko pierwsza kopia przed replikacją off-site.

Restore:

1. Utwórz pusty, odizolowany bucket/cluster.
2. Odtwórz IAM i policies z bezpiecznej konfiguracji.
3. Odtwórz obiekty i zweryfikuj hash/manifest.
4. Zweryfikuj referencje z kopią PostgreSQL o zgodnym punkcie czasu.
5. Nie nadpisuj produkcji przed zakończeniem walidacji.

## Aktualizacja i rollback

- test kontraktowy S3 przed każdą zmianą;
- snapshot/backup metadanych i konfiguracji;
- rolling update gatewayów/filerów/volume/master zgodnie z release notes;
- kontrola mixed-version support;
- obserwacja błędów, replication lag i disk usage;
- rollback tylko, jeśli format danych/metadanych jest kompatybilny; w przeciwnym razie restore do nowego klastra.

## Hardening

- żadnych root credentials w aplikacji;
- prywatny endpoint i TLS między hostami;
- osobne policy per bucket/use case;
- krótki TTL presigned URLs;
- blokada public access;
- szyfrowanie dysku i wrażliwego storage state także na poziomie aplikacji;
- quota/retencja i alerty na pojemność;
- nieprzewidywalne object keys;
- logowanie operacji administracyjnych;
- brak zaufania do nazwy, MIME i rozszerzenia uploadu;
- regularne testy usunięcia zgodnego z retencją.

## Troubleshooting

### S3 signature mismatch

Sprawdź zegar, region, path-style, endpoint host użyty do podpisu i proxy headers. Nie wyłączaj walidacji podpisu.

### Presigned URL działa tylko wewnątrz Compose

Endpoint użyty do podpisu musi odpowiadać hostowi dostępnemu klientowi. Rozwiązaniem jest dedykowany publiczny Artifact Gateway albo poprawnie skonfigurowany zewnętrzny S3 hostname, nie publikacja całego management plane.

### Rosnący dysk

Sprawdź lifecycle, niedokończone multipart uploads, quarantine, retencję trace i liczbę wersji. Nie usuwaj plików bezpośrednio z volume directory.

### Brak obiektu wskazanego przez PostgreSQL

Zdarzenie jest błędem integralności. Oznacz rekord, zabezpiecz audit, sprawdź race w finalize/upload i backup. Nie maskuj problemu pustym plikiem.

### Niespójność z AWS S3

Zapisz minimalny reproducer w integration tests. Jeśli operacja nie jest gwarantowanym kontraktem, zmień adapter lub workflow. Nie wprowadzaj zależności od nieudokumentowanego zachowania.

## Różnice produkcyjne

Dev używa prostego `mini`, krótkiej retencji i danych odtwarzalnych. Produkcja wymaga IAM, TLS, trwałości, backupu off-site, quota i monitoringu. HA wymaga master quorum i volume servers na osobnych hostach; kilka procesów na jednym hoście nadal jest single-node.

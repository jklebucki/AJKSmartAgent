# Temporal

## Cel i zakres

Temporal zapewnia trwałe wykonanie długich zadań agenta: retry, timeout, cancellation, approval, manual takeover, checkpoint logic i wznowienie po restarcie. Jest infrastrukturą orkiestracji, nie plannerem AI ani Policy Engine.

Najważniejsza reguła: workflow jest deterministyczny. Wywołanie LLM, Playwright, IFS API, S3, e-mail i zewnętrznego HTTP odbywa się w Activity. Workflow zarządza stanem, timers, signals i decyzjami o kolejności.

## Wybór i licencja

Temporal Server i oficjalny .NET SDK są MIT i bezpłatne komercyjnie. Temporal Cloud jest usługą płatną i nie jest częścią domyślnego stosu.

Źródła: [Temporal Server](https://github.com/temporalio/temporal), [.NET SDK](https://github.com/temporalio/sdk-dotnet), [dokumentacja](https://docs.temporal.io/).

## Wersja i pinning

- Serwer, UI, CLI i schema tools pochodzą ze zgodnej, testowanej linii i są przypięte po digestach.
- NuGet `Temporalio` jest przypięty centralnie.
- Zmiana SDK wymaga replay tests wszystkich trwałych workflow histories.
- `temporal server start-dev` jest dozwolony tylko lokalnie.

## Model workflow Praxiara

Przykładowy podział:

```text
AgentTaskWorkflow
├── ResolveSkillActivity
├── ObserveBrowserActivity
├── PlanNextActionActivity
├── AuthorizeActionActivity
├── WaitForApprovalSignal
├── ExecuteToolActivity
├── VerifyOutcomeActivity
├── AppendAuditActivity
└── ContinueAsNew
```

Signals/updates:

```text
ApproveAction
RejectAction
CancelTask
PauseForManualTakeover
ResumeAfterManualTakeover
ProvideUserInput
```

Workflow history nie przechowuje screenshotów, cookies, pełnych promptów ani plików. Przechowuje zredagowane identyfikatory i object IDs. Sensitive payloads używają codec/encryption przed trafieniem do Temporal persistence.

## Idempotency

- `taskId`, `stepId` i `toolCallId` są stabilne.
- Activity skutkowa otrzymuje idempotency key i sprawdza audyt/stan biznesowy przed powtórzeniem.
- Retry nie może ponownie wysłać, zaksięgować ani usunąć rekordu bez deduplikacji.
- Non-retryable business errors są odróżnione od transient failures.
- Approval jest związane z exact action hash i wygasa po zmianie observation/revision.

## Topologie

### Development

- `temporal server start-dev` lub gotowy dev server;
- ephemeral persistence dopuszczalne dla szybkiego testu;
- preferowana konfiguracja PostgreSQL w testach integracyjnych;
- UI na `127.0.0.1`.

### Hardened single-node

- Temporal Server z PostgreSQL persistence i visibility;
- osobne bazy `temporal` i `temporal_visibility` lub oficjalnie wspierany schemat;
- UI za oauth2-proxy;
- frontend 7233 tylko `control-net`;
- TLS i auth między hostami/klientami;
- backup PostgreSQL i kontrolowana retencja histories.

### HA

- role frontend/history/matching/worker rozłożone na osobnych hostach;
- HA PostgreSQL;
- wiele workerów per task queue;
- mTLS/JWT authorizer;
- test failover podczas Activity, approval wait i Continue-As-New;
- capacity planning task queues i persistence IOPS.

## Zasoby startowe

| Element | Dev | Single-node prod |
|---|---|---|
| Temporal Server | 1 CPU, 512 MiB–1 GiB | 2–4 CPU, 2–4 GiB |
| Temporal UI | 0.25 CPU, 128 MiB | 0.5 CPU, 256 MiB |
| Praxiara worker | 1 CPU, 512 MiB | według concurrency i Activities |

PostgreSQL history growth i liczbę workers dobiera się według zadań, długości histories i retry, nie wyłącznie liczby użytkowników.

## Sekrety i konfiguracja

```text
/run/secrets/temporal_db_password
/run/secrets/temporal_client_cert
/run/secrets/temporal_client_key
/run/secrets/temporal_ca
```

Konfiguracja aplikacji:

```text
Temporal__Address=temporal:7233
Temporal__Namespace=praxiara
Temporal__TaskQueue=praxiara-agent
Temporal__Tls__CaFile=/run/secrets/temporal_ca
Temporal__Tls__CertificateFile=/run/secrets/temporal_client_cert
Temporal__Tls__PrivateKeyFile=/run/secrets/temporal_client_key
```

Search Attributes nie zawierają promptów ani PII. Można użyć `TaskId`, `TenantIdHash`, `RiskLevel`, `SkillId`, `State`.

## Sieci i porty

- 7233/TCP gRPC tylko `control-net`.
- UI 8233/8080 wyłącznie `127.0.0.1` w dev albo Caddy+oauth2-proxy.
- PostgreSQL tylko `data-net`.
- Browser Worker nie łączy się bezpośrednio z Temporal; dedykowany trusted worker wykonuje Activities i steruje efemerycznym browser workerem.

## Uruchomienie

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile workflow \
  up -d --wait temporal temporal-ui
```

Schema/init:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  --profile ops \
  run --rm temporal-init
```

W produkcji nie polegamy na niejawnej automatycznej migracji wykonywanej równolegle przez wszystkie serwery.

## Health i smoke test

```bash
temporal operator namespace describe --namespace praxiara
```

Smoke workflow:

1. Uruchom `InfrastructureSmokeWorkflow` z unikalnym ID.
2. Activity zapisuje marker i zwraca.
3. Workflow czeka na signal.
4. Wyślij signal approval.
5. Workflow kończy się i wynik jest odczytywalny.
6. Powtórz Activity z kontrolowanym transient failure i potwierdź retry.
7. Zrestartuj worker podczas wait i potwierdź wznowienie.

```bash
temporal workflow list --namespace praxiara
```

CI musi również uruchamiać replay histories po zmianie kodu workflow.

## Backup i restore

Źródłem prawdy self-hosted Temporal jest PostgreSQL. pgBackRest obejmuje obie bazy. Dodatkowo zachowujemy:

- wersję serwera i schema;
- namespace configuration;
- dynamic config;
- encryption codec keys poza bazą;
- search attributes definitions;
- workflow code/revision zgodny z historią.

Restore:

1. Odtwórz PostgreSQL do spójnego punktu.
2. Uruchom zgodną wersję Temporal bez workers.
3. Zweryfikuj namespace i visibility.
4. Uruchom worker w wersji potrafiącej replay istniejących histories.
5. Wykonaj replay/describe kontrolnej próbki.
6. Dopiero potem dopuść nowe workflow.

Odtworzenie bazy bez zgodnego workflow code może pozostawić niezdatne do replay zadania.

## Aktualizacja i rollback

- Przestrzegaj oficjalnej kolejności schema migration i server version.
- Aktualizuj serwer w zgodnej linii, potem workers.
- Stosuj Worker Versioning lub jawne version markers dla breaking workflow changes.
- Uruchamiaj replay tests przed merge.
- Nie usuwaj Activity/workflow type dopóki istnieją aktywne histories.
- Rollback serwera zależy od schema compatibility; w przeciwnym razie restore.
- Rollback worker wymaga, aby stary kod rozumiał nowe histories/version markers.

## Hardening

- mTLS/JWT authorization w multi-host.
- UI za oauth2-proxy i rolą operatora.
- frontend/DB niepubliczne.
- payload encryption codec dla danych poufnych.
- brak sekretów i dużych payloadów w history/search attributes.
- Activity timeouts, heartbeat, retry policy i cancellation jawne.
- ograniczone concurrency per task queue/tenant.
- Continue-As-New przed nadmiernym wzrostem history.
- audit decyzji biznesowych poza Temporal.
- alerty na task queue backlog, schedule-to-start, failed workflows, DB latency i history size.

## Troubleshooting

### Non-deterministic workflow

Zatrzymaj rollout, zachowaj history i uruchom replay test na revision. Napraw przez version marker/kompatybilną ścieżkę, nie przez usunięcie workflow history.

### Activity wykonuje się ponownie

To oczekiwane w modelu at-least-once. Napraw idempotency/deduplication. Nie ustawiaj `MaximumAttempts=1` dla operacji skutkowej jako substytutu poprawności.

### Workflow history za duże

Usuń duże payloady, użyj object IDs i Continue-As-New. Nie zapisuj screenshotów/DOM/promptów w history.

### Approval dotarło po zmianie strony

Signal zawiera action/observation hash. Workflow odrzuca stare approval i wymaga nowego preview.

### UI pokazuje dane, których operator nie powinien widzieć

Usuń sensitive search attributes/payloads, włącz codec i ogranicz UI role. oauth2-proxy chroni dostęp, ale nie redaguje danych w history.

## Różnice produkcyjne

Dev server jest wygodnym narzędziem, nie produkcyjną topologią. Produkcja wymaga PostgreSQL, auth/TLS, backup, UI auth, replay/versioning i observability. HA wymaga redundantnych ról serwera i workers na osobnych hostach oraz HA persistence.

# Spike structured output lokalnego modelu

## Status

`DISC-013` i `DISC-014` zostały wykonane lokalnie na syntetycznych przypadkach eval. Wynik nie jest zatwierdzeniem produkcyjnym modelu, bo nie obejmuje pełnej baterii evals, sandboxu IFS, testów długiego kontekstu ani audytu acceptable use.

## Konfiguracja

| Pole | Wartość |
|---|---|
| data przebiegu | 2026-07-12 |
| runtime | Ollama 0.30.10 |
| model | `gpt-oss:20b` |
| digest modelu | `17052f91a42e97930aa6e28a6c6c06a983e6a58dbb00434885a0cf5313e376f7` |
| manifest lokalny | `$HOME/.ollama/models/manifests/registry.ollama.ai/library/gpt-oss/20b` |
| warstwa wag | `sha256:e7b273f9636059a689e3ddcab3716e4f65abe0143ac978e46673ad0e52d09efb` |
| warstwa licencji | `sha256:f60356777647e927149cbd4c0ec1314a90caba9400ad205ddc4ce47ed001c2d6` |
| dane eval | wyłącznie syntetyczne identyfikatory faktur i środowisko `ifs-sandbox` |

## Zakres narzędzi eval

Model otrzymał mały katalog trzech wysokopoziomowych narzędzi IFS:

- `ifs_find_customer_invoice`;
- `ifs_prepare_invoice_delivery_preview`;
- `ifs_send_customer_invoice`.

Eval sprawdzał wybór dokładnie jednego tool call, zgodność nazwy narzędzia i kanonicznych argumentów. Narzędzie nie było wykonywane. Nie utrwalono chain-of-thought ani danych klienta.

## Wynik

| Metryka | Wynik |
|---|---:|
| liczba przypadków | 10 |
| zaliczone | 10 |
| pass rate | 100% |
| czas łączny | 47045 ms |
| najkrótszy przypadek | 2448 ms |
| najdłuższy przypadek | 10300 ms |

Surowe zredagowane wyniki zapisano w `docs/discovery/evidence/gpt-oss-20b-tool-eval.json`.

## Przypadki bezpieczeństwa w eval

Cztery przypadki zawierały niezaufane dane z próbą zmiany celu:

- bezpośrednie polecenie `UNTRUSTED_DATA`;
- polecenie po polsku;
- tekst wyglądający jak zakodowana instrukcja;
- atrybut wyglądający jak komenda narzędzia.

W tych przypadkach model wybrał oczekiwane narzędzie zgodne z celem użytkownika. To jest pozytywny sygnał eval, ale nie zastępuje aplikacyjnych kontroli: tool registry, policy, egress deny, approval hash i walidacji argumentów.

## Reprodukcja

```bash
ollama serve
dotnet run --project tools/Praxiara.ModelSpike/Praxiara.ModelSpike.csproj --no-build
```

Program weryfikuje digest modelu przed uruchomieniem eval. Jeśli digest jest inny, przebieg kończy się błędem.

## Ograniczenia

- brak pełnej kompatybilności z produkcyjnym `Praxiara.Llm`;
- brak testu tool catalog większego niż trzy narzędzia;
- brak pomiaru wielu równoległych tasków;
- brak testu na prawdziwych payloadach IFS;
- brak formalnej decyzji o dopuszczeniu modelu w rejestrze produkcyjnym.

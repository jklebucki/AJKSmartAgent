# Lokalny LLM — llama.cpp, Ollama i rejestr modeli

## Cel i zakres

Domyślna instalacja Praxiara musi działać bez płatnego providera chmurowego. Warstwa aplikacji korzysta z `Microsoft.Extensions.AI` i `IChatClient`, a lokalną inferencję dostarcza `llama-server` z projektu llama.cpp. Ollama jest opcjonalnym ułatwieniem developerskim.

Provider cloud jest dodatkowym adapterem, wyłączonym do czasu skonfigurowania kosztu, klucza, DPA, regionu, retencji i polityki danych.

## Wybór i licencje

- Microsoft.Extensions.AI: MIT.
- llama.cpp: MIT.
- Ollama server repository: MIT.

Licencja runtime nie obejmuje modelu, tokenizer assets, fine-tune, LoRA ani kwantyzacji. Każdy artefakt modelu przechodzi oddzielny przegląd.

Źródła: [Microsoft.Extensions.AI](https://github.com/dotnet/extensions), [IChatClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient), [llama.cpp](https://github.com/ggml-org/llama.cpp), [Ollama license](https://github.com/ollama/ollama/blob/main/LICENSE).

## Runtime docelowy

`llama-server` jest preferowany produkcyjnie, ponieważ:

- ma mały surface;
- udostępnia API zgodne z OpenAI;
- pozwala jawnie zamontować zatwierdzony model;
- nie wymaga automatycznego model registry pull;
- może działać na CPU i GPU.

Ollama jest używane lokalnie, gdy poprawia ergonomię. Desktop GUI/installer i registry artifacts nie są automatycznie objęte licencją repozytorium serwera; domyślnie uruchamiamy headless container i zatwierdzone modele.

## Abstrakcja aplikacji

Aplikacja rejestruje provider według konfiguracji:

```text
Llm__Provider=llamacpp
Llm__Endpoint=http://llama-server:8080/v1
Llm__ModelId=<approved-model-id>
Llm__RequestTimeoutSeconds=120
Llm__MaxOutputTokens=2048
```

Model Router opisuje capabilities, nie tylko nazwę:

```text
toolCalling
strictJsonSchema
vision
streaming
contextWindow
maxOutputTokens
commercialApprovalId
dataResidency
```

Planner nie otrzymuje tool, którego wybrany model/provider nie potrafi wiarygodnie obsłużyć. Parser zawsze waliduje JSON/schema po stronie aplikacji.

## Rejestr modeli

Docelowy plik: `deploy/compose/models.lock.json`. Przykładowa struktura:

```json
{
  "models": [
    {
      "id": "approved-model-id",
      "source": "https://huggingface.co/owner/repository",
      "revision": "immutable-commit",
      "file": "model-q4_k_m.gguf",
      "sha256": "replace-with-real-sha256",
      "runtime": "llama.cpp",
      "weightsLicense": "SPDX-or-reviewed-license",
      "commercialUseApproved": true,
      "acceptableUseReviewed": true,
      "quantizationSource": "documented-source",
      "approvedBy": "owner",
      "approvedAt": "2026-07-11"
    }
  ]
}
```

Nie wpisujemy niezweryfikowanego `license: apache-2.0` tylko dlatego, że upstream family miała taką licencję. Fine-tune może mieć inne warunki.

## Dostarczanie modelu

- Model nie jest pobierany podczas startu produkcji.
- Download job używa exact revision, weryfikuje SHA-256 i licencję.
- Model trafia do read-only volume/cache.
- Runtime nie ma write access do modelu.
- Model nie jest umieszczany w repo Git ani głównym obrazie aplikacji.
- Mirror wewnętrzny przechowuje dokładnie zatwierdzony artefakt i jego notices/model card.

## Topologie

### Development CPU

- jedna instancja llama.cpp lub Ollama;
- quantized GGUF dobrany do RAM;
- port tylko `127.0.0.1` lub `app-net`;
- mała concurrency.

### Development GPU

- override NVIDIA albo AMD;
- sterownik/runtime hosta ma osobną politykę licencyjną i kompatybilność;
- test fallback/odmowy startu przy braku GPU.

### Hardened single-node

- dedykowany limit CPU/RAM/GPU;
- model volume read-only;
- endpoint tylko `app-net`;
- brak outbound Internet;
- request queue, concurrency i token limits;
- monitoring latency, tokens, queue i OOM;
- zatwierdzony model oraz fallback.

### HA/skala

- wiele stateless inference replicas z tym samym model digest;
- router z health/capability/capacity;
- sticky nie jest wymagane dla pojedynczego requestu bez server-side session;
- rollout canary nowego modelu i eval gate;
- brak automatycznego mieszania odpowiedzi różnych modeli w jednym tasku bez zapisu model ID.

## Zasoby

Sizing wynika z rozmiaru GGUF, context window, batch i liczby równoległych requestów.

Orientacyjnie:

| Model | RAM/VRAM | Zastosowanie |
|---|---:|---|
| mały 3–8B Q4 | około 3–7 GiB + KV cache | routing, klasyfikacja, prosty planner |
| średni 12–32B Q4 | około 8–24 GiB + KV cache | główny planner/tool calling |
| duży | według model card i benchmarku | tylko jeśli jakość uzasadnia koszt |

Nie gwarantujemy konkretnego modelu w tej dokumentacji. Wybór wynika z evals na rzeczywistych zadaniach IFS, tool schema, prompt injection i języku użytkownika.

## Sekrety i konfiguracja

Lokalny llama.cpp nie wymaga cloud API key. Endpoint jest prywatny i może mieć service auth na gatewayu.

Provider cloud, jeśli włączony:

```text
Llm__Provider=openai-compatible-cloud
Llm__Endpoint=https://provider.example/v1
Llm__ApiKeyFile=/run/secrets/llm_provider_api_key
Llm__ModelId=<approved-cloud-model>
Llm__DataPolicyId=<approved-policy>
Llm__MonthlyBudget=<configured-budget>
```

Klucz pochodzi z OpenBao. Prompt/response body nie trafia do zwykłych logów ani OTEL.

## Sieci i porty

- llama.cpp zwykle 8080/TCP, Ollama 11434/TCP.
- Dev mapping wyłącznie `127.0.0.1`.
- Produkcja tylko `app-net`, opcjonalnie przez wewnętrzny LLM Gateway.
- Runtime lokalny nie potrzebuje Internetu po dostarczeniu modelu.
- Browser Worker nie łączy się bezpośrednio z LLM; orchestrator buduje i waliduje kontekst.

## Uruchomienie

CPU:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  --profile llm-local \
  up -d --wait llama-server
```

NVIDIA:

```bash
docker compose \
  --env-file deploy/compose/versions.env \
  --env-file deploy/compose/.env \
  -f deploy/compose/compose.yaml \
  -f deploy/compose/compose.dev.yaml \
  -f deploy/compose/compose.gpu-nvidia.yaml \
  --profile llm-local \
  up -d --wait llama-server
```

Przed startem init/verify job kontroluje model ID, file size i SHA-256.

## Health i smoke test

```bash
curl --fail --silent http://llama-server:8080/health
```

```bash
curl --fail --silent http://llama-server:8080/v1/models | jq .
```

Smoke test nie ogranicza się do odpowiedzi tekstowej. Wysyła mały schema-constrained tool request i weryfikuje:

- poprawny model ID;
- JSON zgodny ze schematem;
- brak dodatkowego toola;
- streaming/cancellation;
- timeout;
- token usage;
- zachowanie przy prompt injection w niezaufanej sekcji danych.

Pełne evals zapisują model revision, prompt version i wynik, bez wrażliwych danych produkcyjnych.

## Backup i odtworzenie

Model można odtworzyć z kontrolowanego mirroru według `models.lock.json`; nie jest konieczne backupowanie każdej lokalnej cache. Należy jednak backupować:

- lockfile;
- model card i licencję;
- eval results i approval;
- prompt/tool schema versions;
- własne adaptery/fine-tunes i ich provenance;
- configuration bez secretów.

Jeżeli model source może zniknąć, zatwierdzony artefakt jest przechowywany w wewnętrznym immutable mirror zgodnie z licencją.

Restore polega na pobraniu exact artifact, sprawdzeniu SHA-256, uruchomieniu runtime i przejściu smoke/eval gate.

## Aktualizacja i rollback

Runtime update:

1. pin nowego obrazu;
2. SBOM/CVE/licencje;
3. test formatu modelu i API;
4. benchmark latency/memory;
5. eval suite;
6. canary;
7. rollout.

Model update jest zmianą zachowania produktu, nawet przy tym samym API:

1. nowy rekord modelu;
2. legal/acceptable-use review;
3. IFS/tool/prompt-injection evals;
4. porównanie quality, safety, latency i kosztu;
5. canary z zapisem model ID per task;
6. rollback przez router do starego immutable modelu.

Nie nadpisujemy pliku modelu pod tym samym ID.

## Hardening

- endpoint prywatny i uwierzytelniony między usługami;
- brak shell/filesystem/network tools w runtime;
- model volume read-only;
- brak outbound Internet;
- input/output/token/context limits;
- concurrency i queue limits;
- timeout/cancellation i circuit breaker;
- prompt, page data i system instructions rozdzielone strukturalnie;
- tool call walidowany poza modelem;
- brak prompt/response body w logach;
- cloud keys tylko z OpenBao;
- model license i hash sprawdzane przy starcie.

## Troubleshooting

### OOM przy dłuższym kontekście

Zmniejsz context/batch/concurrency albo wybierz właściwą kwantyzację. Nie pozwalaj pojedynczemu użytkownikowi przekroczyć globalnego limitu. Monitoruj KV cache oddzielnie od rozmiaru wag.

### Model zwraca niepoprawny tool JSON

Odrzuć wynik, zapisz zredagowany błąd i użyj recovery/retry z ograniczeniem. Nie wykonuj heurystycznie naprawionych argumentów operacji skutkowej. Jeśli problem jest systemowy, model nie spełnia capability `strictJsonSchema`.

### Ollama pobiera inny model niż oczekiwany

Nie używaj ruchomej nazwy/tagu. Importuj zatwierdzony blob lub manifest, zweryfikuj digest i registry metadata. Produkcja preferuje jawny GGUF w llama.cpp.

### GPU nie jest widoczny

Sprawdź host driver/runtime, Compose device reservation i zgodność obrazu. Fallback CPU musi być jawny; ciche przełączenie może złamać timeout/SLO.

### Jakość spadła bez zmiany modelu

Porównaj prompt version, tool schema, context truncation, quantization hash, runtime version i sampling parameters. Wszystkie muszą być częścią telemetryki technicznej/audytu bez zapisu treści wrażliwej.

## Różnice produkcyjne

Development może używać Ollama i małego modelu. Produkcja używa immutable llama.cpp image/model, prywatnej sieci, limitów, eval gate, monitoringu i zatwierdzonego rejestru modeli. Provider cloud nigdy nie jest „bezpłatnym fallbackiem”; jest osobnym, kosztowym i prawnym wyborem operatora.

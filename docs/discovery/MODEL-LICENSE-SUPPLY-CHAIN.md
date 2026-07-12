# Rejestr licencji i supply chain dla stosu spike

## Status

`DISC-016` pozostaje otwarty, bo zależy od `DISC-005`, `DISC-008` i `DISC-013`. Zweryfikowano lokalny model i wybrane artefakty spike, ale nie ma pełnego rejestru produkcyjnego, SBOM obrazów ani zatwierdzenia ownerów.

## Model lokalny

| Pole | Wartość |
|---|---|
| model | `gpt-oss:20b` |
| runtime lokalny | Ollama 0.30.10 |
| digest Ollama API z przebiegu eval | `17052f91a42e97930aa6e28a6c6c06a983e6a58dbb00434885a0cf5313e376f7` |
| lokalna warstwa wag | `sha256:e7b273f9636059a689e3ddcab3716e4f65abe0143ac978e46673ad0e52d09efb` |
| lokalna warstwa licencji | `sha256:f60356777647e927149cbd4c0ec1314a90caba9400ad205ddc4ce47ed001c2d6` |
| licencja lokalnej warstwy | Apache-2.0 |
| źródło weryfikacji | lokalny blob licencji Ollama oraz oficjalna strona OpenAI Open Models |
| status produkcyjny | kandydat discovery, nie dodany do produkcyjnego model registry |

Oficjalna strona OpenAI Open Models opisuje `gpt-oss` jako modele open-weight 120B/20B do uruchamiania lokalnego i wskazuje Apache 2.0 oraz dopuszczenie eksperymentowania, dostosowania i wdrożenia komercyjnego. Lokalny blob licencji odpowiada Apache License 2.0.

## Obraz Playwright spike

| Pole | Wartość |
|---|---|
| tag | `mcr.microsoft.com/playwright/dotnet:v1.61.0-noble` |
| manifest list digest | `sha256:2e07f0f39ef0d68a6ad6fae5124957e5f5730b0c84d5187bed950a67c4d07f28` |
| linux/arm64 manifest digest | `sha256:151b3b7c40c2da1253f3147b982527471f6ac6e34862b50c7e8edf81ebc6c9a7` |
| status | nie dodano jako produkcyjnego obrazu Browser Workera |

Obraz wymaga osobnego uruchomienia w działającym Docker Engine oraz potwierdzenia non-root, sandboxu Chromium, CVE i SBOM. Te warunki należą do zamknięcia `DISC-005` i dalszego `DISC-016`.

## NuGet użyty przez spike

| Pakiet | Wersja | Użycie | Status |
|---|---:|---|---|
| `Microsoft.Playwright` | 1.61.0 | browser spike i testy browser | centralnie przypięty |
| `Microsoft.NET.Test.Sdk` | 18.7.0 | projekty testowe | centralnie przypięty |
| `xunit.v3` | 3.2.2 | testy | centralnie przypięty |
| `xunit.runner.visualstudio` | 3.1.5 | uruchamianie testów | centralnie przypięty |
| `Scalar.AspNetCore` | 2.16.11 | dokumentacja API | centralnie przypięty |

Pełny audyt NuGet wykonuje końcowa walidacja tej zmiany. Ten dokument nie zastępuje docelowego SBOM ani rejestru licencji dla wszystkich pakietów.

## Braki do zamknięcia

- produkcyjny model registry z ownerem akceptacji;
- pełny SBOM i CVE dla obrazu Browser Workera;
- pełny raport licencji transitive dla NuGet i frontend;
- decyzja prawna/architektoniczna dla komponentów WebRTC/noVNC po domknięciu `DISC-008`;
- lockfile i procedura aktualizacji modelu.

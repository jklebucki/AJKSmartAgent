# Rejestr licencji zależności kodu

Stan weryfikacji: 12 lipca 2026. Ten rejestr obejmuje zależności obecnego szkieletu, nie przyszłe usługi kontenerowe ani wagi modeli. Nie zastępuje analizy prawnej przed dystrybucją produktu.

## Zasada

W repozytorium wolno używać wyłącznie komponentów bezpłatnych w zastosowaniu komercyjnym. Wersje są przypięte w `Directory.Packages.props`, `package.json` i `pnpm-lock.yaml`. Przy każdej aktualizacji należy ponownie sprawdzić metadane licencji i zależności tranzytywne; nazwa pakietu nie gwarantuje niezmienności warunków.

## Bezpośrednie zależności .NET

| Grupa | Licencja |
|---|---|
| .NET, ASP.NET Core, Aspire, Microsoft.Extensions.AI, EF Core | MIT |
| Microsoft.Extensions.Http 10.0.9 | MIT |
| Microsoft.Playwright | MIT |
| OpenTelemetry | Apache-2.0 |
| Npgsql EF Core provider | PostgreSQL License |
| Scalar.AspNetCore | MIT |
| YamlDotNet | MIT |
| xUnit v3 | Apache-2.0 |
| coverlet | MIT |

Źródła: [dotnet](https://github.com/dotnet), [Aspire](https://github.com/dotnet/aspire), [Playwright .NET](https://github.com/microsoft/playwright-dotnet), [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet), [Npgsql](https://github.com/npgsql/npgsql), [Scalar](https://github.com/scalar/scalar), [YamlDotNet](https://github.com/aaubry/YamlDotNet), [xUnit](https://github.com/xunit/xunit), [coverlet](https://github.com/coverlet-coverage/coverlet).

## Bezpośrednie zależności frontendowe

| Pakiet lub grupa | Licencja |
|---|---|
| React, React DOM, React Router | MIT |
| TanStack Query | MIT |
| Microsoft SignalR client | MIT |
| i18next, react-i18next | MIT |
| Zod | MIT |
| Vite, Vitest, Oxlint, Testing Library | MIT |
| TypeScript, Playwright Test | Apache-2.0 |

Źródła: [React](https://github.com/facebook/react), [React Router](https://github.com/remix-run/react-router), [TanStack Query](https://github.com/TanStack/query), [SignalR](https://github.com/dotnet/aspnetcore), [i18next](https://github.com/i18next/i18next), [Zod](https://github.com/colinhacks/zod), [Vite](https://github.com/vitejs/vite), [Vitest](https://github.com/vitest-dev/vitest), [Oxlint](https://github.com/oxc-project/oxc), [Playwright](https://github.com/microsoft/playwright).

## Zależności tranzytywne

Aktualny lockfile zawiera wyłącznie licencje umożliwiające bezpłatne użycie komercyjne: MIT, MIT-0, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, BlueOak-1.0.0, CC0-1.0, Unlicense oraz MPL-2.0.

Wyjątek MPL-2.0 stanowi `lightningcss` wraz z natywnym bindingiem. Jest narzędziem build-time, nie jest modyfikowane ani linkowane z kodem serwera. Przy dystrybucji środowiska developerskiego należy zachować jego informację licencyjną. Zmiana sposobu użycia wymaga ponownej oceny file-level copyleft.

## Polecenia kontrolne

```bash
dotnet list Praxiara.slnx package --vulnerable --include-transitive
dotnet list Praxiara.slnx package --deprecated
pnpm audit --audit-level moderate
pnpm licenses list --json
```

Przed wydaniem należy dodatkowo wygenerować SBOM CycloneDX lub SPDX dla aplikacji i każdego obrazu, dołączyć wymagane `LICENSE`/`NOTICE` oraz skorelować komponenty z wynikami skanu CVE.

## Poza zakresem tego rejestru

- licencje obrazów i usług opisuje `docs/infrastruktura/STOS-I-LICENCJE.md`;
- Chromium/Chrome for Testing oraz kodeki mają własne notices w obrazie Browser Workera;
- licencja Ollama lub llama.cpp nie obejmuje wag modelu;
- każda waga, kwantyzacja i fine-tune wymaga osobnego rekordu z revision, SHA-256, prawem do użycia komercyjnego i acceptable-use policy;
- warunki API chmurowego są usługą płatną/kontraktową, nawet gdy adapter SDK ma licencję MIT.

# Skills Codex dla rozwoju .NET 10

## Decyzja projektowa

Praxiara używa małego, rozłącznego zestawu workflow zamiast instalowania całych katalogów społecznościowych. Codex przeznacza na początkową listę skills ograniczony budżet kontekstu, dlatego dziesiątki nakładających się opisów pogarszają wybór właściwego workflow.

Typowe zadanie powinno aktywować:

1. jeden globalny workflow określający rodzaj pracy;
2. najwyżej jeden repozytoryjny skill określający obszar Praxiary.

## Skills globalne

Źródła są wersjonowane w `.agents/shared-skills`, a katalogi są udostępnione globalnie przez symlinki w `~/.agents/skills`.

Instalację lub odtworzenie dowiązań wykonuje idempotentny skrypt:

```bash
./tools/install-codex-skills.sh
```

| Skill | Działanie |
|---|---|
| `dotnet10-implement-change` | implementacja nowego albo zmienionego zachowania produkcyjnego |
| `dotnet10-refactor-safely` | zmiana struktury bez zmiany publicznego zachowania |
| `dotnet10-write-tests` | tworzenie i wzmacnianie testów xUnit v3 |
| `dotnet10-diagnose-failure` | ustalenie przyczyny awarii bez automatycznej implementacji poprawki |
| `dotnet10-review-code` | jawny, tylko odczytowy przegląd kodu lub diffu |
| `dotnet10-manage-dependencies` | dodawanie, aktualizacja, usuwanie i audyt zależności |

## Skills repozytoryjne

Codex wykrywa je bezpośrednio z `.agents/skills`.

| Skill | Działanie |
|---|---|
| `praxiara-evolve-api-contract` | REST, SignalR, DTO, OpenAPI, auth i testy kontraktowe |
| `praxiara-evolve-persistence` | EF Core, Npgsql, migracje, transakcje, outbox i izolacja tenantów |
| `praxiara-change-browser-agent` | Browser Worker, Playwright, pętla agenta, takeover i weryfikacja |
| `praxiara-integrate-ifs` | IFS Projection/OData, UI, ścieżki hybrydowe i Aurena Agent |
| `praxiara-author-business-skill` | biznesowe YAML skills Praxiary, schema, recorder, runtime i replay |
| `praxiara-review-security` | jawny, tylko odczytowy audyt prompt injection, egress, approval i audytu |

`praxiara-author-business-skill` nie służy do tworzenia skills Codex w `.agents`. `praxiara-review-security` ma wyłączone wywołanie niejawne, aby pełny audyt nie uruchamiał się przypadkowo podczas zwykłej implementacji.

## Użycie

Skill można wskazać jawnie w poleceniu:

```text
Użyj $dotnet10-refactor-safely i $praxiara-evolve-persistence, aby rozdzielić model persystencji bez zmiany zachowania.
```

Opis w frontmatter umożliwia także wybór niejawny. Jeżeli dwa workflow wydają się pasować, wybierz ten odpowiadający intencji użytkownika: implementacja, refaktoring, testy, diagnoza, review albo zależności.

## Niezmienniki wspólne

- `.NET 10`, stabilny C# 14, nullable i warningi jako błędy;
- jeden ręcznie napisany nazwany typ najwyższego poziomu na plik, z nazwą pliku zgodną z nazwą typu;
- SOLID, DRY, KISS i YAGNI stosowane pragmatycznie;
- brak abstrakcji, interfejsów i wzorców projektowych bez rzeczywistej zmienności;
- asynchroniczne I/O z `CancellationToken`, poprawne lifetime DI i jawne wyniki błędów;
- bezpieczeństwo fail-closed, strukturalne logi bez sekretów i testy właściwe dla ryzyka;
- kod, komentarze i dokumentacja kodowa po angielsku; interakcje i ręczny Markdown po polsku.

## Walidacja

Każdy katalog przechodzi oficjalny walidator `skill-creator`. Walidator wymaga PyYAML; poniższe polecenia instalują jego wersję `6.0.2` na licencji MIT w tymczasowym środowisku poza repozytorium. Projekt `Praxiara.AgentSkills.Tests` dodatkowo sprawdza metadane, nazwy, metadane UI, lokalne odnośniki, brak placeholderów i pokrycie pozytywnymi oraz negatywnymi przypadkami routingu.

```bash
VALIDATOR_ENV="${TMPDIR:-/tmp}/praxiara-skill-validator"
python3 -m venv "$VALIDATOR_ENV"
"$VALIDATOR_ENV/bin/pip" install "PyYAML==6.0.2"
find .agents/shared-skills .agents/skills -mindepth 1 -maxdepth 1 -type d -print0 |
  xargs -0 -n1 "$VALIDATOR_ENV/bin/python" ~/.codex/skills/.system/skill-creator/scripts/quick_validate.py

dotnet test tests/Praxiara.AgentSkills.Tests/Praxiara.AgentSkills.Tests.csproj
```

## Źródła

Zestaw został napisany od nowa dla Praxiary na podstawie:

- [oficjalnych zasad tworzenia skills Codex](https://learn.chatgpt.com/docs/build-skills);
- [katalogu .NET Agent Skills zespołu .NET](https://github.com/dotnet/skills), licencja MIT;
- [społecznościowego katalogu Awesome GitHub Copilot](https://github.com/github/awesome-copilot), licencja MIT;
- [konwencji kodowania C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions);
- [najlepszych praktyk ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-10.0);
- [wytycznych dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines);
- [praktyk testów jednostkowych .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices).

Nie skopiowano upstreamowych skills wprost. Część z nich używa starszych wersji .NET, płatnych lub niepożądanych bibliotek albo wymusza wzorce sprzeczne z KISS i architekturą Praxiary.

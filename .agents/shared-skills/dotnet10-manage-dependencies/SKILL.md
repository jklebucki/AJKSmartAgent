---
name: dotnet10-manage-dependencies
description: "Bezpiecznie zmienia zależności NuGet, SDK, narzędzia dotnet i obrazy bazowe projektów .NET 10. Użyj dla wersji, licencji, CVE, Central Package Management, lockfile lub migracji po aktualizacji. Nie używaj do zwykłej implementacji, diagnozy, refaktoringu, review ani samych testów."
---

# Zarządzanie zależnościami .NET 10

Wprowadź najmniejszą uzasadnioną zmianę zależności po sprawdzeniu licencji, bezpieczeństwa, kompatybilności i kosztu operacyjnego. Bezpłatne użycie komercyjne jest warunkiem koniecznym, nie domniemaniem.

## Workflow

1. **Ustal potrzebę.** Określ brakującą zdolność, konsumentów i powód, dla którego BCL, ASP.NET Core shared framework albo obecny pakiet nie wystarcza.
2. **Zbadaj stan repo.** Przeczytaj `AGENTS.md`, `Directory.Packages.props`, projekty konsumentów, lockfiles, rejestr licencji i `git status --short`.
3. **Porównaj alternatywy.** Preferuj rozwiązanie wbudowane i aktywnie utrzymywane. Nie dodawaj pakietu wyłącznie dla kilku linii jawnego kodu.
4. **Zweryfikuj konkretną wersję.** Użyj oficjalnego repozytorium, NuGet i informacji o wydaniu. Sprawdź TFM, stabilność, aktywność, zależności tranzytywne, CVE, provenance oraz licencję tej wersji.
5. **Wykonaj właściwą bramkę.** Zastosuj pasującą sekcję z [bramek zależności](references/dependency-gates.md). Brak jednoznacznej licencji lub prawa do użycia komercyjnego kończy workflow bez dodania zależności.
6. **Zmień deklarację centralnie.** Wersję NuGet umieść w `Directory.Packages.props`; projekt otrzymuje `PackageReference` bez lokalnej wersji. Nie używaj wildcardów ani `latest`.
7. **Dostosuj tylko kompatybilność.** Wprowadź minimalne zmiany kodu wymagane przez nową wersję. Nie dodawaj przy okazji funkcji ani refaktoringu. Każdy ręczny top-level named type pozostaje w osobnym pliku nazwanym jak typ.
8. **Zaktualizuj provenance.** Odśwież lockfile, rejestr licencji, SBOM lub dokument obrazu, zależnie od rodzaju artefaktu.
9. **Waliduj.** Wykonaj restore, audyt, build i testy wszystkich konsumentów. Dla aktualizacji sprawdź także breaking changes i zachowanie runtime.

## Reguły decyzji

- Dopuszczaj automatycznie tylko licencje zaakceptowane w `AGENTS.md`, po sprawdzeniu konkretnej wersji i zależności tranzytywnych.
- Licencje copyleft i niestandardowe wymagają jawnego przeglądu określonego w repozytorium. Nie interpretuj samodzielnie niejasnych warunków prawnych.
- Nie dodawaj prerelease, jeśli zadanie nie wymaga funkcji dostępnej wyłącznie w prerelease i użytkownik nie zaakceptował ryzyka.
- Nie wyłączaj `NuGetAudit`, nie suppressuj advisory bez udokumentowanej analizy i daty ponownej oceny.
- Zachowaj zgodność wersji pakietów Microsoft z `net10.0`; provider EF musi wspierać używaną główną wersję EF Core.
- Nie aktualizuj wielu niezależnych rodzin pakietów w jednym kroku. Ułatw bisekcję i rollback.
- Dla obrazu używaj jawnego tagu podczas developmentu i digestu w produkcyjnym manifeście. Nie używaj `latest`.
- Kod, identyfikatory, komentarze, XML docs i polecenia zapisuj po angielsku; dokumenty Markdown i raport dla użytkownika po polsku.

## Walidacja

Minimalny zestaw dla NuGet:

```bash
SOLUTION="path/to/YourSolution.slnx"
dotnet restore "$SOLUTION"
dotnet list "$SOLUTION" package --vulnerable --include-transitive
dotnet list "$SOLUTION" package --deprecated --include-transitive
dotnet build "$SOLUTION" --no-restore
dotnet test "$SOLUTION" --no-build --logger "console;verbosity=minimal"
```

Jeżeli repozytorium wersjonuje `packages.lock.json`, po świadomym odświeżeniu i review potwierdź dodatkowo restore z `--locked-mode`. W raporcie podaj starą i nową wersję, licencję, źródła weryfikacji, wyniki advisory oraz rzeczywisty zakres testów.

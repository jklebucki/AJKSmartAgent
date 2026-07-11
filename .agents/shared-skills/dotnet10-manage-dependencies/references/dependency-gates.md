# Warunkowe bramki zależności

Wykonaj bramkę wspólną i sekcję właściwą dla rodzaju artefaktu. Wszystkie ustalenia dotyczą konkretnej wersji, nie samej nazwy projektu.

## Każda zależność

- [ ] Istnieje konkretna potrzeba, której nie pokrywa BCL, shared framework ani obecny kod.
- [ ] Sprawdzono oficjalne źródło, maintainerów, datę wydania i aktywność projektu.
- [ ] Potwierdzono stabilną wersję wspierającą `net10.0` i używane platformy.
- [ ] Sprawdzono bezpośrednią licencję oraz licencje tranzytywne.
- [ ] Potwierdzono bezpłatne użycie komercyjne i właściwy model dystrybucji/hostingu.
- [ ] Sprawdzono CVE, deprecated status i znane problemy konkretnej wersji.
- [ ] Sprawdzono podpis, checksum, provenance lub wiarygodne źródło publikacji.
- [ ] Porównano co najmniej rozwiązanie wbudowane i brak zależności.
- [ ] Zapisano właściciela aktualizacji i wpływ operacyjny.

## Jeśli dodajesz pakiet NuGet

- [ ] Wersja znajduje się w `Directory.Packages.props`, a `PackageReference` nie ma lokalnego `Version`.
- [ ] Nie dodano pakietu do projektu, który nie konsumuje jego API.
- [ ] Sprawdzono zależności tranzytywne przez `dotnet list package --include-transitive`.
- [ ] API pakietu nie wymusza service locatora, globalnego mutable state ani osłabienia granic modułów.
- [ ] Rejestracja DI ma właściwy lifetime i disposal.
- [ ] Każdy ręczny adapter lub options type ma osobny plik nazwany jak typ.
- [ ] Rejestr licencji i lockfile zostały zaktualizowane.

## Jeśli aktualizujesz pakiet NuGet

- [ ] Przeczytano release notes i breaking changes dla wszystkich pomijanych wersji.
- [ ] Aktualizacja jednej rodziny pakietów jest oddzielona od niezależnych aktualizacji.
- [ ] Provider i framework mają kompatybilne major versions, szczególnie EF Core.
- [ ] Sprawdzono zmiany defaultów, serializacji, retry, auth i telemetry.
- [ ] Minimalne adaptacje kodu nie zmieniają zachowania poza wymogiem nowej wersji.
- [ ] Istnieje prosty rollback wersji i lockfile.
- [ ] Uruchomiono testy wszystkich bezpośrednich i pośrednich konsumentów.

## Jeśli usuwasz pakiet

- [ ] Wyszukano użycia namespace, API, MSBuild targets, analyzers i konfiguracji runtime.
- [ ] Sprawdzono, czy pakiet nie był potrzebny wyłącznie w publish albo design time.
- [ ] Usunięto osieroconą konfigurację, rejestracje DI i wpis licencji.
- [ ] Restore nie przywraca pakietu tranzytywnie w nieoczekiwany sposób.
- [ ] Build i testy przechodzą w konfiguracji produkcyjnej.

## Jeśli aktualizujesz SDK albo narzędzie dotnet

- [ ] Zaktualizowano jawny pin, a nie lokalne środowisko bez śladu w repo.
- [ ] Sprawdzono support policy, workload compatibility i zmiany MSBuild/analyzers.
- [ ] Nie ustawiono `LangVersion=latest` ani `preview`; `net10.0` pozostaje na C# 14.
- [ ] Sprawdzono format lockfile, restore resolver i zachowanie `dotnet test`.
- [ ] CI używa tej samej feature band albo udokumentowanego roll-forward.
- [ ] Wykonano clean-room restore z pustym cache, jeśli zmiana dotyczy resolvera.

## Jeśli aktualizujesz bazowy obraz .NET

- [ ] Użyto oficjalnego repozytorium Microsoft i zgodnego wariantu `aspnet`, `runtime` albo `sdk`.
- [ ] Finalny obraz nie zawiera SDK, jeśli aplikacja go nie potrzebuje w runtime.
- [ ] Kontener działa jako non-root `app` i używa portu zgodnego z konfiguracją.
- [ ] Sprawdzono architektury docelowe, RID i biblioteki natywne.
- [ ] Dla Alpine/chiseled sprawdzono ICU, timezone, Kerberos, LDAP i potrzebę shell.
- [ ] Produkcyjny manifest ma digest, a SBOM i skan obrazu są odświeżone.
- [ ] Health probes i graceful shutdown działają na zbudowanym obrazie.

## Jeśli licencja jest niejasna lub ograniczona

- [ ] Nie dodano artefaktu do repo ani lockfile.
- [ ] Zapisano dokładny tekst/identyfikator licencji i źródło niejasności.
- [ ] Sprawdzono alternatywę na licencji automatycznie dopuszczalnej.
- [ ] Przekazano decyzję do jawnego przeglądu prawnego i architektonicznego.
- [ ] Nie przedstawiono własnej interpretacji jako porady prawnej.

## Końcowa walidacja

- [ ] Restore w trybie locked kończy się powodzeniem po świadomym odświeżeniu lockfile.
- [ ] Audyt vulnerable i deprecated obejmuje zależności tranzytywne.
- [ ] Build ma zero warnings i errors.
- [ ] Testy konsumentów przechodzą.
- [ ] Diff nie zawiera przypadkowych aktualizacji innych rodzin pakietów.
- [ ] Dokument licencji/SBOM odpowiada faktycznemu grafowi.

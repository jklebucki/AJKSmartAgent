# Praxiara Web

Powłoka interfejsu użytkownika. Instrukcje uruchomienia i zasady pracy znajdują się w głównych plikach `README.md` oraz `AGENTS.md` repozytorium.

## Uruchomienie lokalne

Uruchom API na porcie `5176`, a następnie interfejs:

```bash
dotnet run --project src/Praxiara.Api
pnpm web:dev
```

W lokalnym środowisku Compose wygodniej użyć skryptów repozytorium. API odczyta wówczas dane PostgreSQL z ignorowanego pliku `deploy/compose/.env`, a frontend będzie komunikował się z API na porcie `5176`:

```bash
./scripts/run-api-development.sh
./scripts/run-web-development.sh
```

Skrypty nie zapisują sekretów do `appsettings`. Migrację bazy wykonaj raz po uruchomieniu PostgreSQL:

```bash
./scripts/migrate-development-database.sh
```

Vite przekazuje `/api` oraz `/hubs` do `http://localhost:5176`. Dzięki temu frontend używa ścieżek względnych, a przeglądarka nie wymaga konfiguracji CORS dla środowiska lokalnego.

Jeżeli API działa pod innym adresem, ustaw tylko dla lokalnego procesu Vite zmienną `PRAXIARA_API_UPSTREAM`, na przykład:

```bash
PRAXIARA_API_UPSTREAM=http://localhost:8080 pnpm web:dev
```

Nie umieszczaj w tej zmiennej sekretów ani tokenów. Nie używaj prefiksu `VITE_`: ten prefiks udostępnia wartość kodowi działającemu w przeglądarce.

#!/bin/sh
set -eu

repository_root="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
compose_environment="$repository_root/deploy/compose/.env"

if [ ! -r "$compose_environment" ]; then
  echo "Missing readable Compose environment file: $compose_environment" >&2
  exit 1
fi

set -a
. "$compose_environment"
set +a

quote_connection_value() {
  printf '%s' "$1" | sed 's/"/""/g'
}

database_name="$(quote_connection_value "$PRAXIARA_DB_NAME")"
database_user="$(quote_connection_value "$PRAXIARA_DB_USER")"
database_password="$(quote_connection_value "$PRAXIARA_DB_PASSWORD")"

connection_string="Host=127.0.0.1;Port=5432;Database=\"$database_name\";Username=\"$database_user\";Password=\"$database_password\""
export ConnectionStrings__Praxiara="$connection_string"
export PRAXIARA_DESIGNTIME_CONNECTION_STRING="$connection_string"

exec dotnet run --project "$repository_root/src/Praxiara.Api/Praxiara.Api.csproj"

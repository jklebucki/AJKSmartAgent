#!/bin/sh
set -eu

validate_identifier() {
  value="$1"
  label="$2"

  if [ -z "$value" ]; then
    echo "$label must not be empty." >&2
    exit 1
  fi

  if printf '%s' "$value" | LC_ALL=C grep -q '[[:cntrl:]]'; then
    echo "$label must not contain control characters." >&2
    exit 1
  fi
}

create_role_and_database() {
  database_name="$1"
  role_name="$2"
  role_password="$3"

  validate_identifier "$database_name" "Database name"
  validate_identifier "$role_name" "Role name"

  psql \
    --username "$POSTGRES_USER" \
    --dbname "$POSTGRES_DB" \
    --set=ON_ERROR_STOP=1 \
    --set=database_name="$database_name" \
    --set=role_name="$role_name" \
    --set=role_password="$role_password" <<'SQL'
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L', :'role_name', :'role_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'role_name')
\gexec

SELECT format('CREATE DATABASE %I OWNER %I', :'database_name', :'role_name')
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = :'database_name')
\gexec

SELECT format('REVOKE ALL ON DATABASE %I FROM PUBLIC', :'database_name')
\gexec

SELECT format('GRANT ALL PRIVILEGES ON DATABASE %I TO %I', :'database_name', :'role_name')
\gexec
SQL

  psql \
    --username "$POSTGRES_USER" \
    --dbname "$database_name" \
    --set=ON_ERROR_STOP=1 \
    --command='REVOKE CREATE ON SCHEMA public FROM PUBLIC;'
}

create_role_and_database "$PRAXIARA_DB_NAME" "$PRAXIARA_DB_USER" "$PRAXIARA_DB_PASSWORD"
create_role_and_database "$KEYCLOAK_DB_NAME" "$KEYCLOAK_DB_USER" "$KEYCLOAK_DB_PASSWORD"
create_role_and_database "$TEMPORAL_DB_NAME" "$TEMPORAL_DB_USER" "$TEMPORAL_DB_PASSWORD"
create_role_and_database "$TEMPORAL_VISIBILITY_DB_NAME" "$TEMPORAL_DB_USER" "$TEMPORAL_DB_PASSWORD"

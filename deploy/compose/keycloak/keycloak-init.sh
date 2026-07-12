#!/bin/sh
set -eu

require_value() {
  name="$1"
  value="$2"
  if [ -z "$value" ]; then
    echo "$name is required." >&2
    exit 1
  fi
}

require_value "PRAXIARA_SEED_ADMIN_USERNAME" "${PRAXIARA_SEED_ADMIN_USERNAME:-}"
require_value "PRAXIARA_SEED_ADMIN_PASSWORD_FILE" "${PRAXIARA_SEED_ADMIN_PASSWORD_FILE:-}"

if [ ! -r "$PRAXIARA_SEED_ADMIN_PASSWORD_FILE" ]; then
  echo "PRAXIARA_SEED_ADMIN_PASSWORD_FILE is not readable." >&2
  exit 1
fi

admin_password="$(cat "$PRAXIARA_SEED_ADMIN_PASSWORD_FILE")"
require_value "Seed administrator password" "$admin_password"

kcadm="/opt/keycloak/bin/kcadm.sh"
"$kcadm" config credentials \
  --server "$KEYCLOAK_URL" \
  --realm master \
  --user "$KEYCLOAK_BOOTSTRAP_ADMIN_USERNAME" \
  --password "$KEYCLOAK_BOOTSTRAP_ADMIN_PASSWORD"

user_id="$("$kcadm" get users --realm praxiara --query "username=$PRAXIARA_SEED_ADMIN_USERNAME" --fields id --format csv --noquotes | tr -d '\r\n')"
if [ -z "$user_id" ]; then
  "$kcadm" create users --realm praxiara \
    --set "username=$PRAXIARA_SEED_ADMIN_USERNAME" \
    --set enabled=true
  user_id="$("$kcadm" get users --realm praxiara --query "username=$PRAXIARA_SEED_ADMIN_USERNAME" --fields id --format csv --noquotes | tr -d '\r\n')"
  require_value "Seed administrator id" "$user_id"
  "$kcadm" set-password --realm praxiara --userid "$user_id" --new-password "$admin_password"
fi

"$kcadm" add-roles --realm praxiara --userid "$user_id" --rolename praxiara-admin

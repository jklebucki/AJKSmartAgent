#!/bin/sh
set -eu

if [ "$(id -u)" = "0" ]; then
  mkdir -p /run/valkey
  chown valkey:valkey /data /run/valkey
  chmod 0700 /run/valkey
  exec setpriv --reuid=valkey --regid=valkey --clear-groups -- /bin/sh "$0" "$@"
fi

if ! printf '%s' "$VALKEY_PASSWORD" | grep -Eq '^[A-Za-z0-9._~+/-]{24,}$'; then
  echo 'VALKEY_PASSWORD must contain at least 24 URL-safe characters.' >&2
  exit 1
fi

umask 077
{
  printf 'bind 0.0.0.0\n'
  printf 'port 6379\n'
  printf 'protected-mode yes\n'
  printf 'dir /data\n'
  printf 'dbfilename dump.rdb\n'
  printf 'appendonly yes\n'
  printf 'appendfsync everysec\n'
  printf 'save 60 1\n'
  printf 'requirepass %s\n' "$VALKEY_PASSWORD"
} > /run/valkey/valkey.conf

exec valkey-server /run/valkey/valkey.conf

#!/bin/sh
set -eu

repository_root="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
cd "$repository_root"

exec env PRAXIARA_API_UPSTREAM=http://localhost:5176 pnpm web:dev

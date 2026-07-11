#!/usr/bin/env bash

set -euo pipefail

repository_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
source_root="$repository_root/.agents/shared-skills"
target_root="${HOME}/.agents/skills"

mkdir -p "$target_root"

for source in "$source_root"/*; do
    skill_name=$(basename "$source")
    target="$target_root/$skill_name"

    if [[ -L "$target" ]]; then
        if [[ "$(readlink "$target")" != "$source" ]]; then
            printf 'Conflicting symlink: %s\n' "$target" >&2
            exit 1
        fi

        continue
    fi

    if [[ -e "$target" ]]; then
        printf 'Conflicting global skill: %s\n' "$target" >&2
        exit 1
    fi

    ln -s "$source" "$target"
    printf 'Installed %s\n' "$skill_name"
done

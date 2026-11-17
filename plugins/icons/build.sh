#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

# --- [COMPOSITION] ----------------------------------------------------------------------

uv run -q --script generate.py "$@"
for dir in */; do
    name=${dir%/}
    [[ -f "${name}/icon.svg" ]] || continue
    svgo -q --multipass "${name}/icon.svg" "${name}/icon-dark.svg"
    plugin=$(fd -t d -d 2 -x echo {} \; "^${name}$" .. --exclude icons | head -n 1)
    out=${plugin:+${plugin}/assets}
    out=${out:-${name}}
    mkdir -p "${out}"
    resvg -w 1024 -h 1024 "${name}/icon.svg" "${out}/icon.png"
    resvg -w 1024 -h 1024 "${name}/icon-dark.svg" "${out}/icon-dark.png"
    oxipng -q -o 4 "${out}/icon.png" "${out}/icon-dark.png"
done

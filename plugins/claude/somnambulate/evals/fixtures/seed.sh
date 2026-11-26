#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ---------------------------------------------------------------------

readonly FIXTURE="${BASH_SOURCE[0]%/*}/$1"
readonly CONFIG="${HOME%/*}/config"

# --- [COMPOSITION] -------------------------------------------------------------------

[[ -d ${CONFIG} ]] || {
    echo "seed: no Claude Code config directory beside HOME at ${CONFIG}" >&2
    exit 1
}
cp -R "${FIXTURE}" .memory
jq -n --arg dir "${PWD}/.memory" '{autoMemoryDirectory: $dir, env: {CLAUDE_CODE_DISABLE_CLAUDE_MDS: "0"}}' >"${CONFIG}/settings.json"

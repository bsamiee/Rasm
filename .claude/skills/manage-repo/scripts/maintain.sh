#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------
root="$(git -C "${0%/*}" rev-parse --show-toplevel)"
readonly root

# --- [COMPOSITION] ----------------------------------------------------------------------
main() {
    cd "$root"
    mise daemons stop
    mise upgrade --local --no-prune
    mise exec -- nx reset --onlyDaemon --onlyWorkspaceData
    mise exec -- nx run rasm:upgrade
    mise exec -- nx reset --onlyDaemon --onlyWorkspaceData
    mise exec -- pnpm prune
    mise exec -- playwright install --only-shell chromium
    launchctl kickstart -k "gui/${EUID}/dev.mise.pitchfork"
    osascript -e 'quit app "Visual Studio Code"'
    for _ in {1..30}; do pgrep -qx Code || break; sleep 1; done
    mise exec -- pixi run --locked -e hdf5 pixi run --locked -e default code .
    mise prune --yes
}

main "$@"

#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------
root="$(git -C "${0%/*}" rev-parse --show-toplevel)"
readonly root

# --- [COMPOSITION] ----------------------------------------------------------------------
main() {
    cd "$root"
    mise daemons stop
    mise exec -- nx reset --onlyWorkspaceData
    mise exec -- nx run rasm:upgrade
    mise exec -- nx reset --onlyWorkspaceData
    mise exec -- nx run-many --targets=install --projects=tag:language:rust,tag:build:swiftpm,tag:output:command
    mise exec -- pnpm prune
    mise exec -- playwright install --only-shell chromium
    launchctl kickstart -k "gui/${EUID}/dev.mise.pitchfork"
    mise exec -- watch-apps quit 'Visual Studio Code'
    mise exec -- pixi run --locked -e hdf5 pixi run --locked -e default code .
    mise prune --yes
}

main "$@"

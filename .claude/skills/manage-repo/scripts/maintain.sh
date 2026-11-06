#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------
root="$(git -C "${0%/*}" rev-parse --show-toplevel)"
readonly root

# --- [OPERATIONS] -----------------------------------------------------------------------
recycle() {
    trap - EXIT
    launchctl kickstart -k "gui/${EUID}/dev.mise.pitchfork"
}

# --- [COMPOSITION] ----------------------------------------------------------------------
main() {
    cd "$root"
    trap recycle EXIT
    mise daemons stop
    mise exec -- nx run rasm:upgrade
    mise exec -- nx run-many --targets=install --projects=tag:language:rust,tag:build:swiftpm,tag:output:command
    mise exec -- playwright install --only-shell chromium
    mise daemons stop
    recycle
    mise exec -- watch-apps quit 'Visual Studio Code'
    mise exec -- code .
    mise prune --yes
}

main "$@"

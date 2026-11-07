#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------
root="$(git -C "${0%/*}" rev-parse --show-toplevel)"
readonly root

# --- [OPERATIONS] -----------------------------------------------------------------------
recycle() {
    local result=$? target="gui/${EUID}/dev.mise.pitchfork"
    trap - EXIT
    if ((result == 0)); then
        mise daemons stop || result=$?
    fi
    if ((result != 0)); then
        launchctl kill SIGTERM "$target" || return "$result"
        return "$result"
    fi
    launchctl kickstart -k "$target"
}

# --- [COMPOSITION] ----------------------------------------------------------------------
main() {
    cd "$root"
    trap recycle EXIT
    mise daemons stop
    mise upgrade --no-global --no-prune
    mise install --force pypi:ast-grep/ast-grep-mcp
    hk_version=$(mise current hk)
    mise exec -- sd 'download/v[^/]+/hk@[^#]+#' "download/v$hk_version/hk@$hk_version#" .config/hk.pkl
    mise exec -- nx run rasm:upgrade
    mise exec -- nx run-many --targets=install --projects=tag:language:rust,tag:build:swiftpm,tag:output:command
    mise exec -- playwright install --only-shell chromium
    recycle
    mise exec -- watch-apps quit 'Visual Studio Code'
    mise exec -- code .
    mise prune --yes
}

main "$@"

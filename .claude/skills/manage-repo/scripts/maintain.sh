#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------
root="$(git -C "${0%/*}" rev-parse --show-toplevel)"
domain="gui/$(id -u)"
readonly root domain

# --- [OPERATIONS] -----------------------------------------------------------------------
unload() {
    mise bootstrap macos launchd-agents status --json |
        jq -r --argjson names "$(yq -p toml -oj '.bootstrap.macos.launchd.agents | keys' mise.toml)" \
            '.launchd.agents[] | select(.loaded and (.name | IN($names[]))) | .label' |
        while IFS= read -r label; do
            launchctl bootout --wait "$domain/$label"
        done
}

# --- [COMPOSITION] ----------------------------------------------------------------------
main() {
    cd "$root"
    unload
    mise exec -- colima stop
    mise upgrade --local --no-prune
    mise exec -- pnpm exec nx reset --onlyDaemon --onlyWorkspaceData
    mise exec -- pnpm exec nx run rasm:upgrade
    mise exec -- pnpm exec nx reset --onlyDaemon --onlyWorkspaceData
    mise exec -- env -C "$(mise exec -- printenv COLIMA_HOME)" colima start --activate=false --ssh-config=false --mount none
    mise exec -- colima update
    mise exec -- pixi run --locked -e hdf5 pixi run --locked -e default code .
    mise prune --yes
}

main "$@"

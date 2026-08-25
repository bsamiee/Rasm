#!/usr/bin/env bash
# Title    : osascript-runner
# Contract : runner <bundle-id> <script-path> [data-arg ...]
set -Eeuo pipefail
shopt -s inherit_errexit

readonly OSASCRIPT=/usr/bin/osascript
readonly PERL=/usr/bin/perl
readonly SEND_TIMEOUT="${AUTOMATION_SEND_TIMEOUT:-45}"

main() {
    [[ $# -ge 2 ]] || {
        printf 'usage: runner <bundle-id> <script-path> [data-arg ...]\n' >&2
        return 2
    }
    [[ "${SEND_TIMEOUT}" =~ ^[1-9][0-9]*$ ]] || {
        printf 'AUTOMATION_SEND_TIMEOUT must be a positive integer\n' >&2
        return 2
    }
    local -r bundle_id=$1 script_path=$2
    shift 2
    [[ -f "${script_path}" ]] || {
        printf 'script path is not a file: %s\n' "${script_path}" >&2
        return 2
    }
    exec "${PERL}" -e 'alarm shift; exec @ARGV' "${SEND_TIMEOUT}" \
        "${OSASCRIPT}" "${script_path}" "${bundle_id}" "$@"
}

main "$@"

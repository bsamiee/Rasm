#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit nullglob
IFS=$'\n\t'
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'test: Bash 5.3+ required\n' >&2; exit 1; }
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly TEST_TARGET="${TEST_TARGET:-${ROOT_DIR}/tests/csharp/libs/Rasm/Rasm.Tests.csproj}" CONFIGURATION="${CONFIGURATION:-Release}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks"
ACTIVE_LOCK=""
_die() { printf 'test: %s\n' "$1" >&2; exit "${2:-1}"; }
_err() { printf 'test: failed with %s at line %s: %s\n' "$1" "$3" "$2" >&2; }
trap '_err "$?" "${BASH_COMMAND}" "${LINENO}"' ERR
_release_lock() { local -r lock="${ACTIVE_LOCK:-}"; ACTIVE_LOCK=""; [[ -n "${lock}" ]] && rmdir -- "${lock}" 2>/dev/null || true; }
trap '_release_lock; exit $?' EXIT
_with_lock() {
    local -r lock_dir="${LOCK_ROOT}/${1}.lock" deadline=$((BASH_MONOSECONDS + 120))
    shift
    mkdir -p -- "${LOCK_ROOT}"
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        ((BASH_MONOSECONDS < deadline)) || _die "Lock timeout: ${lock_dir}"
        sleep 0.2
    done
    ACTIVE_LOCK="${lock_dir}"
    "$@"
}
_run() {
    local -r filter="${1:-}"
    local -a args=(--configuration "${CONFIGURATION}")
    [[ -z "${filter}" ]] || args+=(--filter "${filter}")
    dotnet test "${TEST_TARGET}" "${args[@]}"
}
_self_test() {
    command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    [[ -f "${TEST_TARGET}" ]] || _die "Missing test target: ${TEST_TARGET}"
}
_main() {
    [[ "${1:-}" == "--self-test" ]] && { _self_test; printf 'test: self-test passed\n'; return 0; }
    command -v dotnet >/dev/null || _die "Missing required command: dotnet"
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    _with_lock test-cs _run "${1:-}"
}
_main "$@"

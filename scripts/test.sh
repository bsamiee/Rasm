#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit nullglob
IFS=$'\n\t'
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'test: Bash 5.3+ required\n' >&2; exit 1; }
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
_run_id() { printf '%(%Y-%m-%dT%H-%M-%S)T' -1; }
readonly TEST_TARGET="${TEST_TARGET:-${ROOT_DIR}/tests/csharp/libs/Rasm/Rasm.Tests.csproj}" CONFIGURATION="${CONFIGURATION:-Release}"
readonly TEST_TARGET_NAME="${TEST_TARGET##*/}"
readonly TEST_SLICE="${TEST_TARGET_NAME%.csproj}" TEST_RUN_ID="${TEST_RUN_ID:-$(_run_id)}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks"
readonly TEST_RESULTS_DIR="${TEST_RESULTS_DIR:-${ROOT_DIR}/.artifacts/test/${TEST_SLICE}/${TEST_RUN_ID}}"
ACTIVE_LOCK=""
_die() { printf 'test: %s\n' "$1" >&2; exit "${2:-1}"; }
_err() { printf 'test: failed with %s at line %s: %s\n' "$1" "$3" "$2" >&2; }
trap '_err "$?" "${BASH_COMMAND}" "${LINENO}"' ERR
_release_lock() { local -r lock="${ACTIVE_LOCK:-}"; ACTIVE_LOCK=""; [[ -n "${lock}" ]] && rmdir -- "${lock}" 2>/dev/null || true; }
_cleanup_exit() { local -r exit_code="$?"; _release_lock; exit "${exit_code}"; }
trap _cleanup_exit EXIT
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
    local -r mode="${1:-run}"
    local -r filter="${2:-}"
    local -a args=(--configuration "${CONFIGURATION}" --results-directory "${TEST_RESULTS_DIR}")
    [[ "${mode}" == "list" ]] && args+=(--list-tests)
    [[ "${mode}" == "coverage" ]] && args+=(/p:CollectCoverage=true)
    [[ "${mode}" == "coverage" && -n "${COVERAGE_THRESHOLD:-}" ]] && args+=(/p:Threshold="${COVERAGE_THRESHOLD}")
    [[ "${mode}" == "coverage" && -n "${COVERAGE_THRESHOLD_TYPE:-}" ]] && args+=(/p:ThresholdType="${COVERAGE_THRESHOLD_TYPE}")
    [[ "${mode}" == "coverage" && -n "${COVERAGE_THRESHOLD_STAT:-}" ]] && args+=(/p:ThresholdStat="${COVERAGE_THRESHOLD_STAT}")
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
    [[ "${1:-}" == "--list-tests" ]] && { shift; (($# <= 1)) || _die "Unexpected arguments: $*" 2; _with_lock test-cs _run list "${1:-}"; return 0; }
    [[ "${1:-}" == "--coverage" ]] && { shift; (($# <= 1)) || _die "Unexpected arguments: $*" 2; _with_lock test-cs _run coverage "${1:-}"; return 0; }
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    _with_lock test-cs _run run "${1:-}"
}
_main "$@"

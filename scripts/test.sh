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
readonly TEST_LOCK_TIMEOUT_SECONDS="${TEST_LOCK_TIMEOUT_SECONDS:-120}"
readonly TEST_LOCK_STALE_SECONDS="${TEST_LOCK_STALE_SECONDS:-30}"
readonly TEST_RESULTS_DIR="${TEST_RESULTS_DIR:-${ROOT_DIR}/.artifacts/test/${TEST_SLICE}/${TEST_RUN_ID}}"
ACTIVE_LOCK="" ACTIVE_TOKEN=""
_die() { printf 'test: %s\n' "$1" >&2; exit "${2:-1}"; }
_err() { printf 'test: failed with %s at line %s: %s\n' "$1" "$3" "$2" >&2; }
trap '_err "$?" "${BASH_COMMAND}" "${LINENO}"' ERR
_release_lock() {
    local -r lock_dir="${ACTIVE_LOCK:-}"
    local -r token="${ACTIVE_TOKEN:-}"
    ACTIVE_LOCK="" ACTIVE_TOKEN=""
    [[ -z "${lock_dir}" ]] || {
        [[ -f "${lock_dir}/owner" && "$(<"${lock_dir}/owner")" == *"|${token}|"* ]] && rm -f -- "${lock_dir}/owner" 2>/dev/null || true
        rmdir -- "${lock_dir}" 2>/dev/null || true
    }
}
_lock_stale() {
    local -r lock_dir="$1"
    local -r meta="${lock_dir}/owner"
    local created
    [[ -f "${meta}" ]] || {
        created="$(stat -f %m "${lock_dir}" 2>/dev/null || stat -c %Y "${lock_dir}" 2>/dev/null || printf '%s' "${EPOCHSECONDS}")"
        ((EPOCHSECONDS - created > TEST_LOCK_STALE_SECONDS)) && return 0
        return 1
    }
    local owner_pid owner_started owner_token owner_name owner_start_command
    IFS='|' read -r owner_pid owner_started owner_token owner_name < "${meta}" || return 0
    [[ -n "${owner_token}" && -n "${owner_name}" ]] || return 0
    [[ "${owner_pid}" =~ ^[0-9]+$ ]] || return 0
    kill -0 "${owner_pid}" 2>/dev/null || return 0
    owner_start_command="$(ps -o lstart= -p "${owner_pid}" 2>/dev/null || true)"
    [[ -n "${owner_started}" && "${owner_start_command}" == "${owner_started}" ]] && return 1
    return 0
}
_clear_stale_lock() {
    local -r lock_dir="$1"
    rm -f -- "${lock_dir}/owner" 2>/dev/null || true
    rmdir -- "${lock_dir}" 2>/dev/null || true
}
_lock_owner_label() {
    local -r meta="$1"
    [[ -f "${meta}" ]] && cat "${meta}" || printf 'unknown'
}
_cleanup_exit() { local -r exit_code="$?"; _release_lock; exit "${exit_code}"; }
trap _cleanup_exit EXIT
_with_lock() {
    local -r name="$1"
    shift
    local -r lock_key="${name//[^[:alnum:]_.:-]/_}"
    local -r lock_dir="${LOCK_ROOT}/${lock_key}.lock"
    local -r lock_meta="${lock_dir}/owner"
    local -r deadline=$((BASH_MONOSECONDS + TEST_LOCK_TIMEOUT_SECONDS))
    mkdir -p -- "${LOCK_ROOT}"
    while ! mkdir -- "${lock_dir}" 2>/dev/null; do
        _lock_stale "${lock_dir}" && _clear_stale_lock "${lock_dir}" && continue
        ((BASH_MONOSECONDS < deadline)) || _die "Lock timeout: ${lock_dir} (owner: $(_lock_owner_label "${lock_meta}"))"
        sleep 0.2
    done
    ACTIVE_LOCK="${lock_dir}"
    ACTIVE_TOKEN="$$:${BASH_MONOSECONDS}:${SRANDOM}"
    local lock_tmp
    lock_tmp="$(mktemp "${lock_dir}/owner.XXXXXXXX")"
    printf '%s|%s|%s|%s\n' "$$" "$(ps -o lstart= -p "$$" 2>/dev/null || true)" "${ACTIVE_TOKEN}" "${name}" > "${lock_tmp}"
    mv -- "${lock_tmp}" "${lock_meta}"
    "$@"
    _release_lock
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

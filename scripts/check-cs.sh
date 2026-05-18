#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 2))) || { printf 'check-cs: Bash 5.2+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx" FORMAT_SEVERITY="${FORMAT_SEVERITY:-warn}" CONFIGURATIONS_RAW="${CONFIGURATIONS:-Debug Release}" TEST_CONFIGURATION="${TEST_CONFIGURATION:-Release}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks"
readonly UNUSED_CODE_DIAGNOSTICS="IDE0005 IDE0051 IDE0052 CS0169 CS0649"
readonly -a PROJECT_EXCLUDE_ARGS=(--exclude .artifacts --exclude .cache --exclude .git --exclude .nx --exclude bin --exclude coverage --exclude node_modules --exclude obj --exclude test-results --exclude tmp)
readonly -a DOTNET_SERIAL_BUILD_ARGS=(-maxcpucount:1 -p:BuildInParallel=false)
ACTIVE_LOCK=""
_die() { local -r message="$1" code="${2:-1}"; printf 'check-cs: %s\n' "${message//$'\n'/$'\n'check-cs: }" >&2; exit "${code}"; }
_err() { local -r exit_code="$1" command="$2" line="$3"; printf 'check-cs: failed with %s at line %s: %s\n' "${exit_code}" "${line}" "${command}" >&2; }
trap '_err "$?" "${BASH_COMMAND}" "${LINENO}"' ERR
_release_lock() {
    local -r lock_dir="${ACTIVE_LOCK:-}"
    ACTIVE_LOCK=""
    [[ -z "${lock_dir}" ]] || {
        rm -f -- "${lock_dir}/owner" 2>/dev/null || true
        rmdir -- "${lock_dir}" 2>/dev/null || true
    }
}
_trap_exit() {
    local -r exit_code="$?"
    _release_lock
    exit "${exit_code}"
}
trap _trap_exit EXIT
_lock_stale() {
    local -r lock_dir="$1"
    local -r meta="${lock_dir}/owner"
    local owner_pid owner_started owner_start_command
    [[ -f "${meta}" ]] || return 0
    IFS='|' read -r owner_pid owner_started < "${meta}" || return 0
    [[ "${owner_pid}" =~ ^[0-9]+$ ]] || return 0
    kill -0 "${owner_pid}" 2>/dev/null || return 0
    owner_start_command="$(ps -o lstart= -p "${owner_pid}" 2>/dev/null || true)"
    [[ -n "${owner_started}" && "${owner_start_command}" == "${owner_started}" ]] && return 1
    return 0
}
_with_lock() {
    local -r name="$1"
    local -r handler="$2"
    shift 2
    local -r lock_key="${name//[^[:alnum:]_.:-]/_}"
    local -r lock_dir="${LOCK_ROOT}/${lock_key}.lock"
    local -r lock_meta="${lock_dir}/owner"
    mkdir -p -- "${LOCK_ROOT}"
    local attempts=0
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        _lock_stale "${lock_dir}" && rm -rf -- "${lock_dir}"
        ((attempts++ < 600)) || _die "Timed out waiting for quality gate lock: ${lock_dir}"
        sleep 0.2
    done
    ACTIVE_LOCK="${lock_dir}"
    printf '%s|%s|%s\n' "$$" "$(ps -o lstart= -p "$$" 2>/dev/null || true)" "${name}" > "${lock_meta}"
    "${handler}" "$@"
    _release_lock
}
_run_check() {
    local -r mode="$1"
    shift
    local -a solution_projects=("$@")
    dotnet restore "${SOLUTION_PATH}" --locked-mode --disable-parallel
    local -a configurations=()
    IFS=' ' read -r -a configurations <<< "${CONFIGURATIONS_RAW}"
    [[ "${mode}" == "test" && " ${CONFIGURATIONS_RAW} " != *" ${TEST_CONFIGURATION} "* ]] && configurations+=("${TEST_CONFIGURATION}")
    readonly -a configurations
    local configuration
    for configuration in "${configurations[@]}"; do
        dotnet build "${SOLUTION_PATH}" --configuration "${configuration}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}"
    done
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity "${FORMAT_SEVERITY}" --no-restore
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity info --diagnostics "${UNUSED_CODE_DIAGNOSTICS}" --no-restore
    [[ "${mode}" == "test" ]] || return 0
    local -a test_projects=()
    mapfile -t test_projects < <(printf '%s\n' "${solution_projects[@]}" | rg --no-line-number '(^|/)[^/]*Tests\.csproj$' || true)
    readonly -a test_projects
    ((${#test_projects[@]} > 0)) || _die "No C# test projects found in ${SOLUTION_PATH}"
    local test_project
    for test_project in "${test_projects[@]}"; do dotnet test "${ROOT_DIR}/${test_project}" --configuration "${TEST_CONFIGURATION}" --no-build -- RunConfiguration.MaxCpuCount=1; done
}
_main() {
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    local -r mode="${1:-check}"
    [[ "${mode}" == "check" || "${mode}" == "test" || "${mode}" == "--self-test" ]] || _die "Unexpected argument: ${mode}" 2
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd rg; do
        command -v "${command}" >/dev/null || _die "Missing required command: ${command}"
    done
    local -a workspace_projects=() solution_projects=()
    mapfile -t workspace_projects < <(cd -- "${ROOT_DIR}" && fd -H -e csproj . --strip-cwd-prefix=always "${PROJECT_EXCLUDE_ARGS[@]}" | LC_ALL=C sort)
    readonly -a workspace_projects
    ((${#workspace_projects[@]} > 0)) || _die "No C# projects discovered."
    mapfile -t solution_projects < <(cd -- "${ROOT_DIR}" && dotnet sln "${SOLUTION_PATH}" list | rg --no-line-number '\.csproj$' | LC_ALL=C sort || true)
    readonly -a solution_projects
    ((${#solution_projects[@]} > 0)) || _die "No C# projects listed in ${SOLUTION_PATH}"
    local missing extra
    missing="$(comm -23 <(printf '%s\n' "${workspace_projects[@]}") <(printf '%s\n' "${solution_projects[@]}"))"
    extra="$(comm -13 <(printf '%s\n' "${workspace_projects[@]}") <(printf '%s\n' "${solution_projects[@]}"))"
    readonly missing extra
    [[ -z "${missing}" ]] || _die "C# projects missing from Workspace.slnx:
- ${missing//$'\n'/$'\n'- }"
    [[ -z "${extra}" ]] || _die "Workspace.slnx contains non-discovered C# projects:
- ${extra//$'\n'/$'\n'- }"
    [[ "${mode}" != "--self-test" ]] || {
        command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"; bash -n "${BASH_SOURCE[0]}"; shellcheck "${BASH_SOURCE[0]}"; printf 'check-cs: self-test passed\n'; return 0
    }
    _with_lock check-cs _run_check "${mode}" "${solution_projects[@]}"
}
_main "$@"

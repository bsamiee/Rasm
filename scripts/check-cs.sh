#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'check-cs: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob
shopt -s array_expand_once 2>/dev/null || true
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx" FORMAT_SEVERITY="${FORMAT_SEVERITY:-warn}" CONFIGURATIONS_RAW="${CONFIGURATIONS:-Debug Release}" TEST_CONFIGURATION="${TEST_CONFIGURATION:-Release}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks"
readonly UNUSED_CODE_DIAGNOSTICS="IDE0005 IDE0051 IDE0052 CS0169 CS0649"
readonly -a PROJECT_EXCLUDE_ARGS=(--exclude .artifacts --exclude .cache --exclude .git --exclude .nx --exclude bin --exclude coverage --exclude node_modules --exclude obj --exclude test-results --exclude tmp)
readonly -a DOTNET_SERIAL_BUILD_ARGS=(-maxcpucount:1 -p:BuildInParallel=false)
readonly -a FULL_TRIGGER_FILES=(Directory.Build.props Directory.Build.targets Directory.Packages.props Workspace.slnx .editorconfig global.json)
readonly -a IGNORED_TEST_ROOTS=(tests/ast-grep tests/py_analyzer tests/python)
declare -Ar MODE_HANDLER=([check]=_run_changed [full]=_run_full [test]=_run_changed_test [test-full]=_run_full_test)
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
    local created
    [[ -f "${meta}" ]] || {
        created="$(stat -f %m "${lock_dir}" 2>/dev/null || stat -c %Y "${lock_dir}" 2>/dev/null || printf '%s' "${EPOCHSECONDS}")"
        ((EPOCHSECONDS - created > 5)) && return 0
        return 1
    }
    local owner_pid owner_started owner_start_command
    IFS='|' read -r owner_pid owner_started < "${meta}" || return 0
    [[ "${owner_pid}" =~ ^[0-9]+$ ]] || return 0
    kill -0 "${owner_pid}" 2>/dev/null || return 0
    owner_start_command="$(ps -o lstart= -p "${owner_pid}" 2>/dev/null || true)"
    [[ -n "${owner_started}" && "${owner_start_command}" == "${owner_started}" ]] && return 1
    return 0
}
_with_lock() {
    local -r name="$1" handler="$2"
    shift 2
    local -r lock_key="${name//[^[:alnum:]_.:-]/_}"
    local -r lock_dir="${LOCK_ROOT}/${lock_key}.lock"
    local -r lock_meta="${lock_dir}/owner"
    mkdir -p -- "${LOCK_ROOT}"
    local -r deadline=$((BASH_MONOSECONDS + 120))
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        _lock_stale "${lock_dir}" && rm -rf -- "${lock_dir}"
        ((BASH_MONOSECONDS < deadline)) || _die "Timed out waiting for quality gate lock: ${lock_dir}"
        sleep 0.2
    done
    ACTIVE_LOCK="${lock_dir}"
    printf '%s|%s|%s\n' "$$" "$(ps -o lstart= -p "$$" 2>/dev/null || true)" "${name}" > "${lock_meta}"
    "${handler}" "$@"
    _release_lock
}
_array_has() {
    local -r needle="$1"
    shift
    local item
    for item in "$@"; do [[ "${item}" == "${needle}" ]] && return 0; done
    return 1
}
_append_unique() {
    local -n __target="$1"
    shift
    local item
    for item in "$@"; do _array_has "${item}" "${__target[@]}" || __target+=("${item}"); done
}
_configs() {
    local -n __configs="$1"
    local -r with_tests="$2"
    __configs=()
    IFS=' ' read -r -a __configs <<< "${CONFIGURATIONS_RAW}"
    [[ "${with_tests}" != test ]] || _array_has "${TEST_CONFIGURATION}" "${__configs[@]}" || __configs+=("${TEST_CONFIGURATION}")
}
_test_projects() {
    local -n __tests="$1"
    shift
    mapfile -t __tests < <(printf '%s\n' "$@" | rg --no-line-number '(^|/)[^/]*Tests\.csproj$' || true)
}
_changed_files() {
    local -n __changed="$1"
    mapfile -t __changed < <(
        cd -- "${ROOT_DIR}" && {
            git diff --name-only --diff-filter=ACDMRTUXB
            git diff --cached --name-only --diff-filter=ACDMRTUXB
            git ls-files --others --exclude-standard
        } | LC_ALL=C sort -u
    )
}
_ignored_test_fixture() {
    local -r file="$1"
    [[ "${file}" == *.csproj ]] && return 1
    local root
    for root in "${IGNORED_TEST_ROOTS[@]}"; do [[ "${file}" == "${root}/"* ]] && return 0; done
    return 1
}
_full_trigger() {
    local -r file="$1"
    local trigger
    for trigger in "${FULL_TRIGGER_FILES[@]}"; do [[ "${file}" == "${trigger}" ]] && return 0; done
    [[ "${file}" == *.csproj || "${file}" == */packages.lock.json || "${file}" == packages.lock.json ]]
}
_routable_file() {
    local -r file="$1"
    _ignored_test_fixture "${file}" && return 1
    _full_trigger "${file}" && return 0
    [[ "${file}" == *.cs || "${file}" == *.props || "${file}" == *.targets || "${file}" == *.json || "${file}" == *.resx || "${file}" == *.ico || "${file}" == *.ghicon || "${file}" == *.yml || "${file}" == *.yaml ]]
}
_owning_project() {
    local -r file="$1"
    local dir="${file%/*}"
    [[ "${dir}" == "${file}" ]] && dir="."
    while [[ "${dir}" != "." && "${dir}" != "/" ]]; do
        local -a matches=()
        mapfile -t matches < <(cd -- "${ROOT_DIR}" && fd -H -e csproj . "${dir}" --max-depth 1)
        ((${#matches[@]} == 1)) && { printf '%s\n' "${matches[0]}"; return 0; }
        dir="${dir%/*}"
    done
    return 1
}
_solution_projects() {
    local -n __workspace="$1" __solution="$2"
    mapfile -t __workspace < <(cd -- "${ROOT_DIR}" && fd -H -e csproj . --strip-cwd-prefix=always "${PROJECT_EXCLUDE_ARGS[@]}" | LC_ALL=C sort)
    ((${#__workspace[@]} > 0)) || _die "No C# projects discovered."
    mapfile -t __solution < <(cd -- "${ROOT_DIR}" && dotnet sln "${SOLUTION_PATH}" list | rg --no-line-number '\.csproj$' | LC_ALL=C sort || true)
    ((${#__solution[@]} > 0)) || _die "No C# projects listed in ${SOLUTION_PATH}"
    local missing extra
    missing="$(comm -23 <(printf '%s\n' "${__workspace[@]}") <(printf '%s\n' "${__solution[@]}"))"
    extra="$(comm -13 <(printf '%s\n' "${__workspace[@]}") <(printf '%s\n' "${__solution[@]}"))"
    [[ -z "${missing}" ]] || _die "C# projects missing from Workspace.slnx:
- ${missing//$'\n'/$'\n'- }"
    [[ -z "${extra}" ]] || _die "Workspace.slnx contains non-discovered C# projects:
- ${extra//$'\n'/$'\n'- }"
}
_project_references() {
    local -r project="$1"
    local -n __refs="$2"
    local -a raw_refs=()
    mapfile -t raw_refs < <(cd -- "${ROOT_DIR}" && dotnet list "${project}" reference | rg --no-line-number '\.csproj$' || true)
    __refs=()
    local ref ref_dir resolved
    for ref in "${raw_refs[@]}"; do
        [[ "${ref}" == /* ]] && { __refs+=("${ref#"${ROOT_DIR}/"}"); continue; }
        ref_dir="$(cd -- "${ROOT_DIR}/${project%/*}/${ref%/*}" && pwd -P)"
        resolved="${ref_dir}/${ref##*/}"
        __refs+=("${resolved#"${ROOT_DIR}/"}")
    done
}
_expand_projects() {
    local -n __projects="$1"
    shift
    local -a solution_projects=("$@")
    local changed=1 project candidate
    while ((changed)); do
        changed=0
        for candidate in "${solution_projects[@]}"; do
            _array_has "${candidate}" "${__projects[@]}" && continue
            local -a refs=()
            _project_references "${candidate}" refs
            for project in "${__projects[@]}"; do
                _array_has "${project}" "${refs[@]}" || continue
                __projects+=("${candidate}")
                changed=1
                break
            done
        done
    done
    mapfile -t __projects < <(printf '%s\n' "${__projects[@]}" | LC_ALL=C sort -u)
}
_route_changed() {
    local -r projects_ref="$1" full_ref="$2" routed_ref="$3"
    # shellcheck disable=SC2178  # nameref target names are scalar inputs for array/int outputs.
    local -n __projects="${projects_ref}"
    local -n __full="${full_ref}"
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array output.
    local -n __routed="${routed_ref}"
    shift 3
    local -a solution_projects=("$@") changed_files=()
    __projects=()
    __full=0
    __routed=()
    _changed_files changed_files
    local file project
    for file in "${changed_files[@]}"; do
        _routable_file "${file}" || continue
        __routed+=("${file}")
        _full_trigger "${file}" && { __full=1; continue; }
        [[ "${file}" == tools/cs-analyzer/* ]] && { __full=1; continue; }
        project="$(_owning_project "${file}" || true)"
        [[ -n "${project}" ]] && _append_unique __projects "${project}"
    done
    ((__full == 0)) || { __projects=("${solution_projects[@]}"); return 0; }
    ((${#__projects[@]} == 0)) || _expand_projects "${projects_ref}" "${solution_projects[@]}"
}
_restore() {
    dotnet restore "${SOLUTION_PATH}" --locked-mode --disable-parallel
}
_build_projects() {
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array.
    local -n __projects="$1"
    local -r with_tests="$2"
    local -a configurations=()
    _configs configurations "${with_tests}"
    local configuration project
    for configuration in "${configurations[@]}"; do
        for project in "${__projects[@]}"; do
            dotnet build "${ROOT_DIR}/${project}" --configuration "${configuration}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}"
        done
    done
}
_format_projects() {
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array.
    local -n __projects="$1"
    local project
    for project in "${__projects[@]}"; do
        dotnet format "${ROOT_DIR}/${project}" --verify-no-changes --severity "${FORMAT_SEVERITY}" --no-restore
        dotnet format "${ROOT_DIR}/${project}" --verify-no-changes --severity info --diagnostics "${UNUSED_CODE_DIAGNOSTICS}" --no-restore
    done
}
_build_solution() {
    local -r with_tests="$1"
    local -a configurations=()
    _configs configurations "${with_tests}"
    local configuration
    for configuration in "${configurations[@]}"; do
        dotnet build "${SOLUTION_PATH}" --configuration "${configuration}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}"
    done
}
_format_solution() {
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity "${FORMAT_SEVERITY}" --no-restore
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity info --diagnostics "${UNUSED_CODE_DIAGNOSTICS}" --no-restore
}
_run_tests() {
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array.
    local -n __tests="$1"
    ((${#__tests[@]} > 0)) || { printf 'check-cs: no relevant C# tests\n'; return 0; }
    local test_project
    for test_project in "${__tests[@]}"; do
        dotnet test "${ROOT_DIR}/${test_project}" --configuration "${TEST_CONFIGURATION}" --no-build -- RunConfiguration.MaxCpuCount=1
    done
}
_run_targeted() {
    local -r with_tests="$1"
    shift
    local -a projects=("$@") tests=()
    printf 'check-cs: targeted projects (%d): %s\n' "${#projects[@]}" "${projects[*]}"
    _restore
    _build_projects projects "${with_tests}"
    _format_projects projects
    [[ "${with_tests}" == test ]] || return 0
    _test_projects tests "${projects[@]}"
    _run_tests tests
}
_run_solution() {
    local -r with_tests="$1"
    shift
    # shellcheck disable=SC2034  # populated through nameref by _test_projects.
    local -a solution_projects=("$@") tests=()
    printf 'check-cs: full solution gate\n'
    _restore
    _build_solution "${with_tests}"
    _format_solution
    [[ "${with_tests}" == test ]] || return 0
    _test_projects tests "${solution_projects[@]}"
    _run_tests tests
}
_run_changed() {
    local -a solution_projects=("$@") projects=() routed=()
    local full=0
    _route_changed projects full routed "${solution_projects[@]}"
    ((${#routed[@]} > 0)) || { printf 'check-cs: no C#-relevant changes; skipping dotnet gate\n'; return 0; }
    ((full == 0)) && { _run_targeted check "${projects[@]}"; return; }
    _run_solution check "${solution_projects[@]}"
}
_run_changed_test() {
    local -a solution_projects=("$@") projects=() routed=()
    local full=0
    _route_changed projects full routed "${solution_projects[@]}"
    ((${#routed[@]} > 0)) || { printf 'check-cs: no C#-relevant changes; skipping dotnet gate\n'; return 0; }
    ((full == 0)) && { _run_targeted test "${projects[@]}"; return; }
    _run_solution test "${solution_projects[@]}"
}
_run_full() { _run_solution check "$@"; }
_run_full_test() { _run_solution test "$@"; }
_assert_eq() { [[ "$1" == "$2" ]] || _die "ASSERT ${FUNCNAME[1]}:${BASH_LINENO[0]}: '${1}' != '${2}'"; }
_self_test() {
    command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local owner
    owner="$(_owning_project libs/csharp/Rasm.Rhino/Commands/Command.cs)"
    _assert_eq libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj "${owner}"
    _full_trigger Directory.Build.props && _full_trigger libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj
    _ignored_test_fixture tests/ast-grep/pass/src/domain/expression_flow.ts || _die "Ignored test fixture route failed"
}
_main() {
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    local -r mode="${1:-check}"
    [[ "${mode}" == "--self-test" ]] && { _self_test; printf 'check-cs: self-test passed\n'; return 0; }
    [[ -v MODE_HANDLER["${mode}"] ]] || _die "Unexpected argument: ${mode}" 2
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd git rg; do command -v "${command}" >/dev/null || _die "Missing required command: ${command}"; done
    # shellcheck disable=SC2034  # populated for bidirectional coverage by _solution_projects.
    local -a workspace_projects=() solution_projects=()
    _solution_projects workspace_projects solution_projects
    : "${workspace_projects[*]}"
    _with_lock check-cs "${MODE_HANDLER[${mode}]}" "${solution_projects[@]}"
}
_main "$@"

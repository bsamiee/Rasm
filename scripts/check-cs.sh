#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'check-cs: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob array_expand_once
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
declare -Ar MODE_SPEC=([check]='changed|check' [full]='full|check' [test]='changed|test' [test-full]='full|test')
ACTIVE_LOCK="" ACTIVE_TOKEN=""
_die() { local -r message="$1" code="${2:-1}"; printf 'check-cs: %s\n' "${message//$'\n'/$'\n'check-cs: }" >&2; exit "${code}"; }
_err() { local -r exit_code="$1" command="$2" line="$3"; printf 'check-cs: failed with %s at line %s: %s\n' "${exit_code}" "${line}" "${command}" >&2; }
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
        ((EPOCHSECONDS - created > 30)) && return 0
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
    ACTIVE_TOKEN="$$:${BASH_MONOSECONDS}:${SRANDOM}"
    local lock_tmp
    lock_tmp="$(mktemp "${lock_dir}/owner.XXXXXXXX")"
    printf '%s|%s|%s|%s\n' "$$" "$(ps -o lstart= -p "$$" 2>/dev/null || true)" "${ACTIVE_TOKEN}" "${name}" > "${lock_tmp}"
    mv -- "${lock_tmp}" "${lock_meta}"
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
_test_projects() {
    local -n __tests="$1"
    shift
    __tests=()
    local project
    for project in "$@"; do [[ "${project}" == */*Tests.csproj || "${project}" == *Tests.csproj ]] && __tests+=("${project}"); done
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
    case "${file}" in tests/ast-grep/*|tests/py_analyzer/*|tests/python/*) return 0 ;; *) return 1 ;; esac
}
_full_trigger() {
    local -r file="$1"
    local trigger
    for trigger in "${FULL_TRIGGER_FILES[@]}"; do [[ "${file}" == "${trigger}" ]] && return 0; done
    [[ "${file}" == *.csproj || "${file}" == */packages.lock.json || "${file}" == packages.lock.json ]]
}
_route_file() {
    local -r file="$1"
    local project
    _ignored_test_fixture "${file}" && { printf 'ignore\n'; return; }
    { _full_trigger "${file}" || [[ "${file}" == tools/cs-analyzer/* ]]; } && { printf 'full\n'; return; }
    case "${file}" in
        *.cs|*.props|*.targets|*.json|*.resx|*.ico|*.ghicon|*.yml|*.yaml) ;;
        *) printf 'ignore\n'; return ;;
    esac
    project="$(_owning_project "${file}" || true)"
    [[ -n "${project}" ]] && { printf 'project|%s\n' "${project}"; return; }
    case "${file}" in *.cs|*.props|*.targets) printf 'full\n' ;; *) printf 'ignore\n' ;; esac
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
    local -n __solution="$1"
    local -a workspace_projects=()
    mapfile -t workspace_projects < <(cd -- "${ROOT_DIR}" && fd -H -e csproj . --strip-cwd-prefix=always "${PROJECT_EXCLUDE_ARGS[@]}" | LC_ALL=C sort)
    ((${#workspace_projects[@]} > 0)) || _die "No C# projects discovered."
    mapfile -t __solution < <(cd -- "${ROOT_DIR}" && dotnet sln "${SOLUTION_PATH}" list | rg --no-line-number '\.csproj$' | LC_ALL=C sort || true)
    ((${#__solution[@]} > 0)) || _die "No C# projects listed in ${SOLUTION_PATH}"
    local missing extra
    missing="$(comm -23 <(printf '%s\n' "${workspace_projects[@]}") <(printf '%s\n' "${__solution[@]}"))"
    extra="$(comm -13 <(printf '%s\n' "${workspace_projects[@]}") <(printf '%s\n' "${__solution[@]}"))"
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
    local file route project
    for file in "${changed_files[@]}"; do
        route="$(_route_file "${file}")"
        [[ "${route}" != ignore ]] || continue
        __routed+=("${file}")
        [[ "${route}" != full ]] || { __full=1; continue; }
        project="${route#project|}"
        _array_has "${project}" "${__projects[@]}" || __projects+=("${project}")
    done
    ((__full == 0)) || { __projects=("${solution_projects[@]}"); return 0; }
    ((${#__projects[@]} == 0)) || _expand_projects "${projects_ref}" "${solution_projects[@]}"
}
_build_targets() {
    local -r with_tests="$1"
    shift
    local -a configurations=()
    IFS=' ' read -r -a configurations <<< "${CONFIGURATIONS_RAW}"
    [[ "${with_tests}" != test ]] || _array_has "${TEST_CONFIGURATION}" "${configurations[@]}" || configurations+=("${TEST_CONFIGURATION}")
    local configuration target
    for configuration in "${configurations[@]}"; do
        for target in "$@"; do
            dotnet build "${target}" --configuration "${configuration}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}"
        done
    done
}
_format_targets() {
    local target
    for target in "$@"; do
        dotnet format "${target}" --verify-no-changes --severity "${FORMAT_SEVERITY}" --no-restore
        dotnet format "${target}" --verify-no-changes --severity info --diagnostics "${UNUSED_CODE_DIAGNOSTICS}" --no-restore
    done
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
_run_gate() {
    local -r scope="$1" with_tests="$2"
    shift 2
    # shellcheck disable=SC2034  # populated through nameref by _test_projects.
    local -a projects=("$@") targets=() format_targets=() tests=()
    ((${#projects[@]} > 0)) || _die "No C# projects selected"
    [[ "${scope}" == full ]] && targets=("${SOLUTION_PATH}") || targets=("${projects[@]/#/${ROOT_DIR}/}")
    [[ "${scope}" == targeted && ${#targets[@]} -gt 1 ]] && format_targets=("${SOLUTION_PATH}") || format_targets=("${targets[@]}")
    printf 'check-cs: %s gate (%d): %s\n' "${scope}" "${#projects[@]}" "${projects[*]}"
    dotnet restore "${SOLUTION_PATH}" --locked-mode --disable-parallel
    _build_targets "${with_tests}" "${targets[@]}"
    _format_targets "${format_targets[@]}"
    [[ "${with_tests}" == test ]] || return 0
    _test_projects tests "${projects[@]}"
    _run_tests tests
}
_run_mode() {
    local -r mode="$1"
    shift
    local scope with_tests
    IFS='|' read -r scope with_tests <<< "${MODE_SPEC[${mode}]}"
    local -a solution_projects=("$@") projects=() routed=()
    local full=0
    [[ "${scope}" == full ]] || {
        _route_changed projects full routed "${solution_projects[@]}"
        ((${#routed[@]} > 0)) || { printf 'check-cs: no C#-relevant changes; skipping dotnet gate\n'; return 0; }
        ((full == 0)) && { _run_gate targeted "${with_tests}" "${projects[@]}"; return; }
    }
    _run_gate full "${with_tests}" "${solution_projects[@]}"
}
_assert_eq() { [[ "$1" == "$2" ]] || _die "ASSERT ${FUNCNAME[1]}:${BASH_LINENO[0]}: '${1}' != '${2}'"; }
_self_test() {
    command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local owner
    owner="$(_owning_project libs/csharp/Rasm.Rhino/Commands/Command.cs)"
    _assert_eq libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj "${owner}"
    [[ -v MODE_SPEC[check] && -v MODE_SPEC[full] && -v MODE_SPEC[test] && -v MODE_SPEC[test-full] ]] || _die "Mode table incomplete"
    _full_trigger Directory.Build.props && _full_trigger libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj
    _ignored_test_fixture tests/ast-grep/pass/src/domain/expression_flow.ts || _die "Ignored test fixture route failed"
    _assert_eq ignore "$(_route_file package.json)"
    _assert_eq full "$(_route_file tools/cs-analyzer/config.json)"
}
_main() {
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    local -r mode="${1:-check}"
    [[ "${mode}" == "--self-test" ]] && { _self_test; printf 'check-cs: self-test passed\n'; return 0; }
    [[ -v MODE_SPEC["${mode}"] ]] || _die "Unexpected argument: ${mode}" 2
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd git rg; do command -v "${command}" >/dev/null || _die "Missing required command: ${command}"; done
    local -a solution_projects=()
    _solution_projects solution_projects
    _with_lock check-cs _run_mode "${mode}" "${solution_projects[@]}"
}
_main "$@"

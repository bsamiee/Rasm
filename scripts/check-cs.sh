#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'check-cs: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob array_expand_once
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx" CHECK_CS_MAX_CPU="${CHECK_CS_MAX_CPU:-4}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks"
readonly -a PROJECT_EXCLUDE_ARGS=(--exclude .artifacts --exclude .cache --exclude .git --exclude .nx --exclude bin --exclude coverage --exclude node_modules --exclude obj --exclude test-results --exclude tmp)
readonly -a DOTNET_BUILD_ARGS=("-maxcpucount:${CHECK_CS_MAX_CPU}")
readonly -a FULL_TRIGGER_FILES=(Directory.Build.props Directory.Build.targets Directory.Packages.props Workspace.slnx .editorconfig global.json)
declare -Ar MODE_SPEC=([check]=changed [full]=full)
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
    local waited=0
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        _lock_stale "${lock_dir}" && { rm -rf -- "${lock_dir}"; continue; }
        ((waited)) || { printf 'check-cs: waiting for quality gate lock: %s\n' "${lock_dir}" >&2; waited=1; }
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
    case "${file}" in tests/tools/ast-grep/*|tests/tools/py_analyzer/*) return 0 ;; *) return 1 ;; esac
}
_full_trigger() {
    local -r file="$1"
    local trigger
    for trigger in "${FULL_TRIGGER_FILES[@]}"; do [[ "${file}" == "${trigger}" ]] && return 0; done
    return 1
}
_route_file() {
    local -r file="$1"
    local project
    _ignored_test_fixture "${file}" && { printf 'ignore\n'; return; }
    { _full_trigger "${file}" || [[ "${file}" == tools/cs-analyzer/* ]]; } && { printf 'full\n'; return; }
    case "${file}" in
        *.cs|*.csproj|*.props|*.targets|*.json|*.resx|*.ico|*.ghicon|*.yml|*.yaml) ;;
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
    __refs=()
    local -a ref_lines=()
    mapfile -t ref_lines < <(rg --no-filename '<ProjectReference\b' "${ROOT_DIR}/${project}" || true)
    local line ref ref_dir resolved
    for line in "${ref_lines[@]}"; do
        [[ "${line}" == *'OutputItemType="Analyzer"'* || "${line}" == *"OutputItemType='Analyzer'"* ]] && continue
        [[ "${line}" == *'ReferenceOutputAssembly="false"'* || "${line}" == *"ReferenceOutputAssembly='false'"* ]] && continue
        ref="${line#*Include=\"}"
        [[ "${ref}" == "${line}" ]] && ref="${line#*Include=\'}"
        [[ "${ref}" == "${line}" ]] && continue
        ref="${ref%%\"*}"
        ref="${ref%%\'*}"
        [[ "${ref}" == *.csproj ]] || continue
        [[ "${ref}" == /* ]] && { __refs+=("${ref#"${ROOT_DIR}/"}"); continue; }
        [[ "${ref}" == */* ]] && ref_dir="$(cd -- "${ROOT_DIR}/${project%/*}/${ref%/*}" && pwd -P)" || ref_dir="$(cd -- "${ROOT_DIR}/${project%/*}" && pwd -P)"
        resolved="${ref_dir}/${ref##*/}"
        __refs+=("${resolved#"${ROOT_DIR}/"}")
    done
}
_project_graph() {
    local -n __graph="$1"
    shift
    __graph=()
    local project
    for project in "$@"; do
        local -a refs=()
        _project_references "${project}" refs
        __graph["${project}"]="$(printf '%s\n' "${refs[@]}")"
    done
}
_expand_projects() {
    local -n __projects="$1"
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an associative-array output.
    local -n __graph="$2"
    shift
    shift
    local -a solution_projects=("$@")
    local changed=1 project candidate raw_refs
    while ((changed)); do
        changed=0
        for candidate in "${solution_projects[@]}"; do
            _array_has "${candidate}" "${__projects[@]}" && continue
            local -a refs=()
            raw_refs="${__graph[${candidate}]:-}"
            [[ -n "${raw_refs}" ]] && mapfile -t refs <<< "${raw_refs}"
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
    local -r projects_ref="$1" full_ref="$2" routed_ref="$3" format_ref="$4"
    # shellcheck disable=SC2178  # nameref target names are scalar inputs for array/int outputs.
    local -n __projects="${projects_ref}"
    local -n __full="${full_ref}"
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array output.
    local -n __routed="${routed_ref}"
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array output.
    local -n __format="${format_ref}"
    local -a changed_files=()
    __projects=()
    __full=0
    __routed=()
    __format=()
    _changed_files changed_files
    local file route project
    for file in "${changed_files[@]}"; do
        route="$(_route_file "${file}")"
        [[ "${route}" != ignore ]] || continue
        __routed+=("${file}")
        [[ "${route}" != full ]] || { __full=1; continue; }
        project="${route#project|}"
        _array_has "${project}" "${__projects[@]}" || __projects+=("${project}")
        [[ "${file}" == *.cs ]] && __format+=("${project}|${file}")
    done
}
_configurations() {
    local -r scope="$1"
    local -n __configurations="$2"
    local raw="${CONFIGURATIONS:-}"
    [[ -n "${raw}" ]] || { [[ "${scope}" == full ]] && raw="Debug Release" || raw="Debug"; }
    IFS=' ' read -r -a __configurations <<< "${raw}"
}
_restore_targets() {
    local target
    for target in "$@"; do
        dotnet restore "${target}" --locked-mode
    done
}
_build_targets() {
    local -r scope="$1"
    shift
    local -a configurations=()
    _configurations "${scope}" configurations
    local configuration target
    for configuration in "${configurations[@]}"; do
        for target in "$@"; do
            dotnet build "${target}" --configuration "${configuration}" --no-restore "${DOTNET_BUILD_ARGS[@]}"
        done
    done
}
_format_full() {
    dotnet format whitespace "${SOLUTION_PATH}" --verify-no-changes --no-restore
}
_format_targeted() {
    (("$#" > 0)) || return 0
    local -a projects=()
    local route project file
    for route in "$@"; do
        project="${route%%|*}"
        _array_has "${project}" "${projects[@]}" || projects+=("${project}")
    done
    for project in "${projects[@]}"; do
        local -a files=()
        for route in "$@"; do
            [[ "${route%%|*}" == "${project}" ]] || continue
            file="${route#*|}"
            files+=("${file}")
        done
        dotnet format whitespace "${ROOT_DIR}/${project}" --include "${files[@]}" --verify-no-changes --no-restore
    done
}
_run_gate() {
    local -r scope="$1" projects_ref="$2" format_ref="$3"
    # shellcheck disable=SC2178  # nameref target name is a scalar input for an array output.
    local -n __projects="${projects_ref}"
    local -n __format_routes="${format_ref}"
    local -a targets=()
    ((${#__projects[@]} > 0)) || _die "No C# projects selected"
    [[ "${scope}" == full ]] && targets=("${SOLUTION_PATH}") || targets=("${__projects[@]/#/${ROOT_DIR}/}")
    printf 'check-cs: %s gate (%d): %s\n' "${scope}" "${#__projects[@]}" "${__projects[*]}"
    _restore_targets "${targets[@]}"
    _build_targets "${scope}" "${targets[@]}"
    case "${scope}" in
        full) _format_full ;;
        *) _format_targeted "${__format_routes[@]}" ;;
    esac
}
_run_mode() {
    local -r mode="$1"
    local -r scope="${MODE_SPEC[${mode}]}"
    local -a solution_projects=() projects=() routed=()
    # shellcheck disable=SC2034  # populated/read through namerefs in _route_changed/_run_gate.
    local -a format_routes=()
    # shellcheck disable=SC2034  # populated through nameref in _project_graph/_expand_projects.
    local -A project_graph=()
    local full=0
    [[ "${scope}" == full ]] || {
        _route_changed projects full routed format_routes
        ((${#routed[@]} > 0)) || { printf 'check-cs: no C#-relevant changes; skipping dotnet gate\n'; return 0; }
        _solution_projects solution_projects
        ((full == 0)) && {
            _project_graph project_graph "${solution_projects[@]}"
            ((${#projects[@]} == 0)) || _expand_projects projects project_graph "${solution_projects[@]}"
            _run_gate targeted projects format_routes
            return
        }
    }
    _solution_projects solution_projects
    projects=("${solution_projects[@]}")
    _run_gate full projects format_routes
}
_assert_eq() { [[ "$1" == "$2" ]] || _die "ASSERT ${FUNCNAME[1]}:${BASH_LINENO[0]}: '${1}' != '${2}'"; }
_self_test() {
    command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local owner
    owner="$(_owning_project libs/csharp/Rasm.Rhino/Commands/Command.cs)"
    _assert_eq libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj "${owner}"
    [[ -v MODE_SPEC[check] && -v MODE_SPEC[full] ]] || _die "Mode table incomplete"
    [[ ! -v MODE_SPEC[test] && ! -v MODE_SPEC[test-full] ]] || _die "Mode table must not advertise test routes; run scripts/test.sh"
    _full_trigger Directory.Build.props && ! _full_trigger libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj
    _ignored_test_fixture tests/tools/ast-grep/pass/src/domain/expression_flow.ts || _die "Ignored test fixture route failed"
    _assert_eq ignore "$(_route_file package.json)"
    _assert_eq full "$(_route_file tools/cs-analyzer/config.json)"
    _assert_eq "project|libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj" "$(_route_file libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj)"
    _assert_eq "project|libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj" "$(_route_file libs/csharp/Rasm.Rhino/packages.lock.json)"
}
_main() {
    (($# <= 1)) || _die "Unexpected arguments: $*" 2
    local -r mode="${1:-check}"
    [[ "${mode}" == "--self-test" ]] && { _self_test; printf 'check-cs: self-test passed\n'; return 0; }
    [[ -v MODE_SPEC["${mode}"] ]] || _die "Unexpected argument: ${mode}" 2
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd git rg; do command -v "${command}" >/dev/null || _die "Missing required command: ${command}"; done
    _with_lock check-cs _run_mode "${mode}"
}
_main "$@"

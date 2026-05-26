#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'rhino: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob array_expand_once
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly CONFIGURATION="${CONFIGURATION:-Release}"
_msbuild_property() {
    local -r project="$1" property="$2"
    dotnet msbuild "${project}" -p:Configuration="${CONFIGURATION}" "-getProperty:${property}" -nologo
}
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly BRIDGE_PROTOCOL_PROJECT="${ROOT_DIR}/tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
readonly BRIDGE_CLIENT_PROJECT="${ROOT_DIR}/tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
readonly BRIDGE_PLUGIN_PROJECT="${ROOT_DIR}/tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
PACKAGE_STAGE_ROOT="$(_msbuild_property "${BRIDGE_CLIENT_PROJECT}" YakStageRoot)"
PACKAGE_STAGE_ROOT="${PACKAGE_STAGE_ROOT%/}"
readonly PACKAGE_STAGE_ROOT
readonly API_RHINO_WIP_APP_PATH="${RHINO_WIP_APP_PATH:-/Applications/RhinoWIP.app}"
readonly API_RHINO_WIP_RESOURCES="${API_RHINO_WIP_APP_PATH}/Contents/Frameworks/RhCore.framework/Versions/A/Resources"
readonly API_RHINO_CODE="${API_RHINO_WIP_APP_PATH}/Contents/Resources/bin/rhinocode"
readonly -a DOTNET_SERIAL_BUILD_ARGS=(-maxcpucount:1 -p:BuildInParallel=false)
readonly -a RHINO_HOST_PACKAGE_EXCLUDES=(
    'Eto.*' 'Eto.macOS.*' 'Grasshopper.*' 'Grasshopper2.*' 'GrasshopperIO.*'
    'Microsoft.macOS.*' 'Rhino.Runtime.Code.*' 'Rhino.UI.*'
    'RhinoCodePlatform.Rhino3D.*' 'RhinoCommon.*' 'System.Drawing.Common.*'
)
readonly -a PACKAGE_PROJECT_ROOTS=("${ROOT_DIR}/apps" "${ROOT_DIR}/tools")
declare -Ar API_REFERENCES=(
    [rhino-common]="${API_RHINO_WIP_RESOURCES}/RhinoCommon.dll|${API_RHINO_WIP_RESOURCES}/RhinoCommon.xml|${ROOT_DIR}/.cache/nuget/packages/rhinocommon/*/lib/net8.0/RhinoCommon.xml"
    [rhino-ui]="${API_RHINO_WIP_RESOURCES}/Rhino.UI.dll|${API_RHINO_WIP_RESOURCES}/Rhino.UI.xml|"
    [rhino-code-remote]="${API_RHINO_WIP_RESOURCES}/Rhino.Runtime.Code.Remote.dll||"
    [rhino-code]="${API_RHINO_WIP_RESOURCES}/Rhino.Runtime.Code.dll||"
    [eto]="${API_RHINO_WIP_RESOURCES}/Eto.dll|${API_RHINO_WIP_RESOURCES}/Eto.xml|${ROOT_DIR}/.cache/nuget/packages/rhinocommon/*/lib/net8.0/Eto.xml"
    [gh2]="${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll|${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml|${ROOT_DIR}/.cache/nuget/packages/grasshopper2/*/ref/net7.0/Grasshopper2.xml"
    [gh2-io]="${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll|${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml|${ROOT_DIR}/.cache/nuget/packages/grasshopper2/*/ref/net7.0/GrasshopperIO.xml"
)
readonly -a API_REFERENCE_ORDER=(rhino-common rhino-ui rhino-code rhino-code-remote eto gh2 gh2-io)
declare -a CLEANUP_PATHS=()
declare -a CLEANUP_LOCKS=()
declare -a CLEANUP_ROLLBACKS=()
CLEANING=0
declare -Ar ROUTES=(
    [--self-test]='_self_test|0|0|'
    [build]='_build|0|1|'
    [verify]='_verify|1|1|'
    [package]='_cmd_package|2|2|'
    [deploy]='_package_action|2|2|deploy'
    [install]='_package_action|2|2|install'
    [push]='_package_action|2|2|push'
    [bridge:build]='_bridge_build|0|0|'
    [bridge:launch]='_bridge_client|0|0|launch'
    [bridge:restart]='_bridge_client|0|0|restart'
    [bridge:doctor]='_bridge_client|0|999|doctor'
    [bridge:load]='_bridge_client|1|999|load'
    [bridge:load-smoke]='_bridge_client|1|999|load-smoke'
    [bridge:check]='_bridge_client|1|999|check'
    [bridge:clean]='_bridge_client|1|1|clean'
    [bridge:unload]='_bridge_client|1|1|unload'
    [bridge:quit]='_bridge_client|0|0|quit'
    [api:doctor]='_api|0|0|doctor'
    [api:path]='_api|1|2|path'
    [api:xml]='_api|2|2|xml'
    [api:types]='_api|1|2|types'
    [api:decompile]='_api|2|2|decompile'
)
readonly -a BRIDGE_PROJECTS=("${BRIDGE_PROTOCOL_PROJECT}" "${BRIDGE_PLUGIN_PROJECT}" "${BRIDGE_CLIENT_PROJECT}")
_trap_err() {
    local -r exit_code="$?"
    local index
    printf 'rhino: error at %s:%s: %s\n' "${BASH_SOURCE[1]:-${BASH_SOURCE[0]}}" "${BASH_LINENO[0]:-0}" "${BASH_COMMAND}" >&2
    for ((index = 1; index < ${#FUNCNAME[@]}; index++)); do
        printf 'rhino: stack[%d] %s at %s:%s\n' "${index}" "${FUNCNAME[index]}" "${BASH_SOURCE[index]}" "${BASH_LINENO[index - 1]:-0}" >&2
    done
    exit "${exit_code}"
}
trap _trap_err ERR
_cleanup_exit() {
    local -r exit_code="$?"
    local entry index lock_dir path previous_dir stage_dir
    ((CLEANING == 0)) || exit "${exit_code}"
    CLEANING=1
    for ((index = ${#CLEANUP_ROLLBACKS[@]} - 1; index >= 0; index--)); do
        entry="${CLEANUP_ROLLBACKS[index]}"
        stage_dir="${entry%%|*}"
        previous_dir="${entry#*|}"
        [[ -e "${stage_dir}" || ! -e "${previous_dir}" ]] || mv -- "${previous_dir}" "${stage_dir}" 2>/dev/null || true
    done
    for ((index = ${#CLEANUP_PATHS[@]} - 1; index >= 0; index--)); do
        path="${CLEANUP_PATHS[index]}"
        rm -rf -- "${path}" 2>/dev/null || true
    done
    for ((index = ${#CLEANUP_LOCKS[@]} - 1; index >= 0; index--)); do
        lock_dir="${CLEANUP_LOCKS[index]}"
        rmdir -- "${lock_dir}" 2>/dev/null || true
    done
    exit "${exit_code}"
}
trap _cleanup_exit EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
_die() {
    printf 'rhino: %s\n' "$1" >&2
    exit "${2:-1}"
}
_dispatch() {
    local -r route="$1"
    shift
    [[ -v ROUTES["${route}"] ]] || _die "Unknown route: ${route}" 2
    local handler min max preset
    IFS='|' read -r handler min max preset <<< "${ROUTES[${route}]}"
    (($# >= min && $# <= max)) || _die "Wrong arg count for ${route}: got $#, expected ${min}..${max}" 2
    [[ -n "${preset}" ]] && set -- "${preset}" "$@"
    "${handler}" "$@"
}
_self_test() {
    local command
    for command in dotnet fd ilspycmd jq plutil rg shellcheck; do
        command -v "${command}" >/dev/null 2>&1 || _die "${command} is required"
    done
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local route handler
    for route in "${!ROUTES[@]}"; do
        IFS='|' read -r handler _ _ _ <<< "${ROUTES[${route}]}"
        declare -F "${handler}" >/dev/null || _die "Route handler missing: ${route} -> ${handler}"
    done
    local path
    for path in "${SOLUTION_PATH}" "${BRIDGE_PROJECTS[@]}"; do
        [[ -e "${path}" ]] || _die "Missing required path: ${path}"
    done
}
_package_projects() {
    local -n __projects="$1"
    __projects=()
    local -a roots=()
    local root
    for root in "${PACKAGE_PROJECT_ROOTS[@]}"; do
        [[ -d "${root}" ]] && roots+=("${root}")
    done
    ((${#roots[@]} > 0)) || return 0
    mapfile -t __projects < <(fd -H -e csproj . "${roots[@]}" | LC_ALL=C sort)
}
_package_slug() {
    local -r project="$1"
    local payload
    payload="$(dotnet msbuild "${project}" -p:Configuration="${CONFIGURATION}" -getProperty:YakPackageSlug -nologo)"
    [[ -n "${payload}" ]] || return 1
    printf '%s\n' "${payload}"
}
_package_project() {
    local -r package_slug="$1"
    local -n __selected_project="$2"
    local -a projects=()
    _package_projects projects
    local candidate candidate_slug
    local -a matches=()
    for candidate in "${projects[@]}"; do
        candidate_slug="$(_package_slug "${candidate}" || true)"
        [[ "${candidate_slug}" == "${package_slug}" ]] && matches+=("${candidate}")
    done
    ((${#matches[@]} == 1)) || _die "Expected one package project for ${package_slug}, found ${#matches[@]}"
    __selected_project="${matches[0]}"
}
_build() {
    local -r version="${1:-}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore)
    [[ -n "${version}" ]] && build_args+=("/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${SOLUTION_PATH}" --locked-mode --disable-parallel
    dotnet build "${SOLUTION_PATH}" "${build_args[@]}" "${DOTNET_SERIAL_BUILD_ARGS[@]}"
}
_bridge_build() {
    dotnet restore "${SOLUTION_PATH}" --locked-mode --disable-parallel
    local project
    for project in "${BRIDGE_PROJECTS[@]}"; do
        dotnet build "${project}" --configuration "${CONFIGURATION}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}"
    done
}
_lock_stale() {
    local -r lock_dir="$1"
    local -r meta="${lock_dir}/owner"
    local created owner_pid owner_started
    if [[ ! -f "${meta}" ]]; then
        created="$(stat -f %m "${lock_dir}" 2>/dev/null || stat -c %Y "${lock_dir}" 2>/dev/null || printf '%s' "${EPOCHSECONDS}")"
        ((EPOCHSECONDS - created > 5)) && return 0
        return 1
    fi
    IFS='|' read -r owner_pid owner_started < "${meta}" || return 0
    [[ "${owner_pid}" =~ ^[0-9]+$ ]] || return 0
    kill -0 "${owner_pid}" 2>/dev/null || return 0
    local owner_start_command
    owner_start_command="$(ps -o lstart= -p "${owner_pid}" 2>/dev/null || true)"
    [[ -n "${owner_started}" && "${owner_start_command}" == "${owner_started}" ]] && return 1
    return 0
}
_with_lock() {
    local -r name="$1"
    local -r handler="$2"
    shift 2
    local -r lock_root="${PACKAGE_STAGE_ROOT}/locks"
    local -r lock_key="${name//[^[:alnum:]_.:-]/_}"
    local -r lock_dir="${lock_root}/${lock_key}.lock"
    mkdir -p -- "${lock_root}"
    local -r deadline=$((BASH_MONOSECONDS + 30))
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        _lock_stale "${lock_dir}" && rm -rf -- "${lock_dir}"
        ((BASH_MONOSECONDS < deadline)) || _die "Timed out waiting for ${name} lock: ${lock_dir}"
        sleep 0.1
    done
    CLEANUP_LOCKS+=("${lock_dir}")
    local lock_tmp
    lock_tmp="$(mktemp "${lock_dir}/owner.XXXXXXXXXX")"
    printf '%s|%s|%s\n' "$$" "$(ps -o lstart= -p "$$" 2>/dev/null || true)" "${name}" >"${lock_tmp}"
    mv -- "${lock_tmp}" "${lock_dir}/owner"
    "${handler}" "$@"
    rm -f -- "${lock_dir}/owner" 2>/dev/null || true
    rmdir -- "${lock_dir}" 2>/dev/null || true
}
_bridge_client() {
    _with_lock bridge-client-build _bridge_client_run "$@"
}
_bridge_client_run() {
    local -r lock_root="${PACKAGE_STAGE_ROOT}/locks"
    local build_log
    build_log="$(mktemp "${lock_root}/bridge-client-build.XXXXXXXXXX")"
    CLEANUP_PATHS+=("${build_log}")
    dotnet restore "${BRIDGE_CLIENT_PROJECT}" --locked-mode --disable-parallel >"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client restore failed"; }
    dotnet build "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}" >>"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client build failed"; }
    rm -f -- "${build_log}"
    dotnet run --no-build --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" -- "$@"
}
_bridge_invoke() {
    dotnet run --no-build --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" -- "$@"
}
_verify() {
    local -r pattern="$1"
    _with_lock bridge-client-build _verify_run "${pattern}"
}
_verify_run() {
    local -r pattern="$1"
    local -a scenarios=()
    _verify_discover "${pattern}" scenarios
    ((${#scenarios[@]} > 0)) || _die "No *.verify.csx scenarios matched: ${pattern}"
    local -r lock_root="${PACKAGE_STAGE_ROOT}/locks"
    local build_log
    build_log="$(mktemp "${lock_root}/bridge-client-build.XXXXXXXXXX")"
    CLEANUP_PATHS+=("${build_log}")
    dotnet restore "${BRIDGE_CLIENT_PROJECT}" --locked-mode --disable-parallel >"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client restore failed"; }
    dotnet build "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}" >>"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client build failed"; }
    rm -f -- "${build_log}"
    local -r report_dir="${PACKAGE_STAGE_ROOT}/verify"
    mkdir -p -- "${report_dir}/.tmp"
    CLEANUP_PATHS+=("${report_dir}/.tmp")
    local -a result_files=()
    local scenario name project result rc status ok=0 failed=0
    for scenario in "${scenarios[@]}"; do
        name="${scenario##*/}"; name="${name%.verify.csx}"
        result="${report_dir}/${name}.json"
        _verify_project "${scenario}" project
        rc=0
        _bridge_invoke check "${project}" "${scenario}" --result "${result}" >/dev/null 2>&1 || rc=$?
        status="$(jq -r '.status // "failed"' "${result}" 2>/dev/null || printf 'failed')"
        result_files+=("${result}")
        case "${status}" in
            ok) ((ok += 1)); printf '[OK]     %s report=%s\n' "${scenario#"${ROOT_DIR}/"}" "${result}" >&2 ;;
            *) ((failed += 1)); printf '[FAILED] %s rc=%s status=%s result=%s\n' "${scenario#"${ROOT_DIR}/"}" "${rc}" "${status}" "${result}" >&2 ;;
        esac
    done
    local -r result_stream="${report_dir}/.tmp/results.jsonl"
    : > "${result_stream}"
    local result_file
    for result_file in "${result_files[@]}"; do
        jq -c . "${result_file}" >> "${result_stream}" 2>/dev/null || true
    done
    local -r summary="${report_dir}/summary.json"
    jq -n --argjson ok "${ok}" --argjson failed "${failed}" --slurpfile s "${result_stream}" '{summary:{ok:$ok,failed:$failed,total:($ok+$failed)},scenarios:$s}' > "${summary}"
    cat "${summary}"
    ((failed == 0)) || exit 1
}
_verify_discover() {
    local -r pattern="$1"
    local -n __scenarios="$2"
    __scenarios=()
    if [[ -f "${pattern}" && "${pattern}" == *.verify.csx ]]; then
        __scenarios=("${pattern}")
        return 0
    fi
    if [[ -d "${pattern}" ]]; then
        mapfile -t __scenarios < <(fd -H -e csx '\.verify\.csx$' "${pattern}" | LC_ALL=C sort)
        return 0
    fi
    mapfile -t __scenarios < <(fd -H -e csx '\.verify\.csx$' "${ROOT_DIR}" | rg -- "${pattern}" | LC_ALL=C sort)
}
_verify_project() {
    local -r scenario="$1"
    local -n __project="$2"
    local abs_scenario rel_scenario test_project candidate dir
    abs_scenario="$(cd -- "$(dirname -- "${scenario}")" && pwd)/${scenario##*/}"
    rel_scenario="${abs_scenario#"${ROOT_DIR}/"}"
    case "${rel_scenario}" in
        tests/csharp/libs/*/scenarios/*.verify.csx)
            test_project="${rel_scenario#tests/csharp/libs/}"
            test_project="${test_project%%/*}"
            candidate="${ROOT_DIR}/libs/csharp/${test_project}/${test_project}.csproj"
            [[ -f "${candidate}" ]] || _die "No source project found for test scenario: ${scenario} -> ${candidate}"
            __project="${candidate}"
            return 0
            ;;
    esac
    dir="$(cd -- "$(dirname -- "${scenario}")" && pwd)"
    while [[ "${dir}" == "${ROOT_DIR}" || "${dir}" == "${ROOT_DIR}/"* ]]; do
        local -a projects=()
        mapfile -t projects < <(fd -H -e csproj . "${dir}" --max-depth 1 | LC_ALL=C sort)
        ((${#projects[@]} <= 1)) || _die "Expected at most one scenario project in ${dir}, found ${#projects[@]}"
        if ((${#projects[@]} == 1)); then
            __project="${projects[0]}"
            return 0
        fi
        [[ "${dir}" != "${ROOT_DIR}" ]] || break
        dir="${dir%/*}"
    done
    _die "No owning project found for scenario: ${scenario}"
}
_dotnet_root_valid() {
    local -r root="$1"
    [[ -n "${root}" && -d "${root}/host/fxr" && -d "${root}/shared/Microsoft.NETCore.App" ]]
}
_dotnet_root_resolve() {
    local -a candidates=()
    [[ -n "${DOTNET_ROOT:-}" ]] && candidates=("${DOTNET_ROOT}" "${DOTNET_ROOT}/share/dotnet")
    local dotnet_path dotnet_real runtime_line runtime_path runtime_root
    if dotnet_path="$(command -v dotnet 2>/dev/null)"; then
        dotnet_real="$(realpath "${dotnet_path}" 2>/dev/null || printf '%s\n' "${dotnet_path}")"
        candidates+=("${dotnet_real%/*}")
        local -a runtime_lines=()
        mapfile -t runtime_lines < <(dotnet --list-runtimes 2>/dev/null || true)
        for runtime_line in "${runtime_lines[@]}"; do
            [[ "${runtime_line}" == Microsoft.NETCore.App\ * ]] || continue
            runtime_path="${runtime_line##* [}"
            runtime_path="${runtime_path%]}"
            runtime_root="${runtime_path%/shared/Microsoft.NETCore.App}"
            [[ "${runtime_root}" != "${runtime_path}" ]] && candidates+=("${runtime_root}")
        done
    fi
    candidates+=("/usr/local/share/dotnet")
    local root
    declare -A seen=()
    for root in "${candidates[@]}"; do
        [[ -n "${root}" && ! -v seen["${root}"] ]] || continue
        seen["${root}"]=1
        _dotnet_root_valid "${root}" && { printf '%s\n' "${root}"; return 0; }
    done
    return 1
}
_with_dotnet_apphost() {
    local dotnet_root
    if dotnet_root="$(_dotnet_root_resolve)"; then
        DOTNET_ROOT="${dotnet_root}" DOTNET_MULTILEVEL_LOOKUP=0 "$@"
        return
    fi
    env -u DOTNET_ROOT -u DOTNET_MULTILEVEL_LOOKUP "$@"
}
_api() {
    local -r action="$1"
    shift
    local key="${1:-}"
    case "${action}" in
        doctor) _api_doctor ;;
        path) [[ -n "${key}" ]] || _die "api path requires <key>"; _api_path "${key}" "${2:-assembly}" ;;
        xml) [[ -n "${key}" && -n "${2:-}" ]] || _die "api xml requires <key> <pattern>"; _api_search "${key}" "$2" ;;
        types) [[ -n "${key}" ]] || _die "api types requires <key>"; _api_types "${key}" "${2:-}" ;;
        decompile) [[ -n "${key}" && -n "${2:-}" ]] || _die "api decompile requires <key> <type>"; _api_decompile "${key}" "$2" ;;
        *) _die "Unknown api action: ${action}" ;;
    esac
}
_api_meta() {
    local -r key="$1"
    local -n __assembly="$2" __xml="$3" __fallback="$4"
    [[ -v API_REFERENCES["${key}"] ]] || _die "Unknown API key: ${key}"
    IFS='|' read -r __assembly __xml __fallback <<< "${API_REFERENCES[${key}]}"
}
_api_xml_path() {
    local -r key="$1"
    local -n __path="$2" __status="$3"
    local meta_assembly meta_xml meta_fallback
    _api_meta "${key}" meta_assembly meta_xml meta_fallback
    : "${meta_assembly}"
    if [[ -n "${meta_xml}" && -f "${meta_xml}" ]]; then
        __path="${meta_xml}"; __status="primary"; return 0
    fi
    if [[ -n "${meta_fallback}" ]]; then
        local -a matches=()
        mapfile -t matches < <({ compgen -G "${meta_fallback}" || true; } | LC_ALL=C sort)
        if ((${#matches[@]} > 0)); then
            __path="${matches[${#matches[@]} - 1]}"; __status="fallback"; return 0
        fi
    fi
    __path=""; __status="missing"
    return 1
}
_api_path() {
    local -r key="$1" kind="$2"
    local assembly xml fallback resolved status
    _api_meta "${key}" assembly xml fallback
    case "${kind}" in
        assembly) [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"; printf '%s\n' "${assembly}" ;;
        xml) _api_xml_path "${key}" resolved status || _die "Missing API XML for ${key}: ${xml}"; printf '%s\n' "${resolved}" ;;
        *) _die "Unknown api path kind: ${kind}" ;;
    esac
}
_api_search() {
    local -r key="$1" pattern="$2"
    local xml status
    _api_xml_path "${key}" xml status || _die "Missing API XML for ${key}"
    rg -n -C 2 -- "${pattern}" "${xml}"
}
_api_types() {
    local -r key="$1" pattern="${2:-}"
    local assembly xml fallback types
    _api_meta "${key}" assembly xml fallback
    [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"
    types="$(_with_dotnet_apphost ilspycmd -l cisde "${assembly}")"
    [[ -n "${pattern}" ]] || { printf '%s\n' "${types}"; return; }
    rg -n -- "${pattern}" <<< "${types}" || _die "No API types matched ${key}: ${pattern}"
}
_api_decompile() {
    local -r key="$1" type_name="$2"
    local assembly xml fallback
    _api_meta "${key}" assembly xml fallback
    [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"
    _with_dotnet_apphost ilspycmd -t "${type_name}" "${assembly}"
}
_api_doctor() {
    local plist="${API_RHINO_WIP_APP_PATH}/Contents/Info.plist"
    local rhino_version="missing"
    [[ -f "${plist}" ]] && rhino_version="$(plutil -extract CFBundleVersion raw -o - "${plist}" 2>/dev/null || printf 'unknown')"
    local dotnet_root ilspy_status="failed" ilspy_version=""
    dotnet_root="$(_dotnet_root_resolve || true)"
    local output
    mkdir -p -- "${PACKAGE_STAGE_ROOT}"
    output="$(mktemp "${PACKAGE_STAGE_ROOT}/ilspy.XXXXXXXXXX")"
    CLEANUP_PATHS+=("${output}")
    _with_dotnet_apphost ilspycmd --version >"${output}" 2>&1 && ilspy_status="ok"
    IFS= read -r ilspy_version < "${output}" || true
    local rhinocode_direct=0 rhinocode_roll=0 rhinocode_status="ok"
    if [[ ! -x "${API_RHINO_CODE}" ]]; then
        rhinocode_status="missing"
    else
        "${API_RHINO_CODE}" list --json >/dev/null 2>&1 || rhinocode_direct="$?"
        DOTNET_ROLL_FORWARD=Major "${API_RHINO_CODE}" list --json >/dev/null 2>&1 || rhinocode_roll="$?"
    fi
    local key assembly xml fallback resolved status assembly_status
    local refs="["
    local refs_sep=""
    for key in "${API_REFERENCE_ORDER[@]}"; do
        _api_meta "${key}" assembly xml fallback
        : "${fallback}"
        assembly_status="missing"
        [[ -f "${assembly}" ]] && assembly_status="present"
        _api_xml_path "${key}" resolved status || true
        refs+="${refs_sep}{\"key\":\"${key}\",\"assembly\":{\"status\":\"${assembly_status}\",\"path\":\"${assembly}\"},\"xml\":{\"status\":\"${status}\",\"path\":\"${resolved:-${xml}}\"}}"
        refs_sep=","
    done
    refs+="]"
    jq -n --arg rhino_app "${API_RHINO_WIP_APP_PATH}" --arg rhino_version "${rhino_version}" \
          --arg ilspy_status "${ilspy_status}" --arg dotnet_root "${dotnet_root:-hostfxr-probe}" --arg ilspy_version "${ilspy_version:-unavailable}" \
          --arg rhinocode_status "${rhinocode_status}" --arg rhinocode_path "${API_RHINO_CODE}" \
          --argjson rhinocode_direct "${rhinocode_direct}" --argjson rhinocode_roll "${rhinocode_roll}" \
          --argjson refs "${refs}" \
          '{rhino:{app:$rhino_app,version:$rhino_version},ilspy:{status:$ilspy_status,dotnet_root:$dotnet_root,version:$ilspy_version},rhinocode:{status:$rhinocode_status,path:$rhinocode_path,direct:$rhinocode_direct,roll_forward:$rhinocode_roll},references:$refs}'
}
_package_meta() {
    local -r package_slug="$1" package_version="$2"
    shift 2
    local -n __project="$1" __manifest_dir="$2" __target_dir="$3" __target_framework="$4" __assembly_name="$5" __target_ext="$6" __project_dir="$7" __yak_path="$8" __yak_platform="$9"
    shift 9
    local -n __yak_push_source="$1" __package_dir="$2" __package_pattern="$3"
    _package_project "${package_slug}" __project
    local -a version_args=()
    [[ -n "${package_version}" ]] && version_args=("/p:Version=${package_version}" "/p:InformationalVersion=${package_version}")
    local payload
    payload="$(
        dotnet msbuild "${__project}" \
            -p:Configuration="${CONFIGURATION}" \
            "${version_args[@]}" \
            -getProperty:YakPackageSlug \
            -getProperty:YakManifestDirectory \
            -getProperty:TargetDir \
            -getProperty:TargetFramework \
            -getProperty:AssemblyName \
            -getProperty:TargetExt \
            -getProperty:MSBuildProjectDirectory \
            -getProperty:YakPath \
            -getProperty:YakPlatform \
            -getProperty:YakPushSource \
            -getProperty:YakPackageDirectory \
            -getProperty:YakPackagePattern \
            -nologo
    )"
    readonly payload
    local -a fields=()
    mapfile -t fields < <(jq -er '.Properties | .YakPackageSlug, .YakManifestDirectory, .TargetDir, .TargetFramework, .AssemblyName, .TargetExt, .MSBuildProjectDirectory, .YakPath, .YakPlatform, (.YakPushSource // ""), .YakPackageDirectory, .YakPackagePattern' <<< "${payload}")
    ((${#fields[@]} == 12)) || _die "Could not evaluate package metadata for ${package_slug}"
    [[ "${fields[0]}" == "${package_slug}" ]] || _die "Package slug mismatch for ${__project}: expected ${package_slug}, evaluated ${fields[0]}"
    __manifest_dir="${fields[1]}"
    __target_dir="${fields[2]}"
    __target_framework="${fields[3]}"
    __assembly_name="${fields[4]}"
    __target_ext="${fields[5]}"
    __project_dir="${fields[6]}"
    __yak_path="${fields[7]}"
    __yak_platform="${fields[8]}"
    __yak_push_source="${fields[9]}"
    __package_dir="${fields[10]}"
    __package_pattern="${fields[11]}"
    [[ -n "${__target_framework}" && -n "${__assembly_name}" ]] || _die "Package project metadata is incomplete for ${package_slug}: ${__project}"
    [[ "${__target_ext}" == ".rhp" ]] || _die "Package project must emit .rhp for ${package_slug}: ${__project}"
    [[ "${__yak_platform}" == "mac" && "${__package_pattern}" == *-rh9_*-mac.yak ]] || _die "Package distribution must target Rhino 9 macOS for ${package_slug}: ${__package_pattern}"
}
_package_find() {
    local -r package_slug="$1" version="$2" stage_dir="$3" package_pattern="$4"
    local -n __package_file="$5"
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "${package_pattern}" . "${stage_dir}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one Yak package for ${package_slug} ${version}, found ${#package_files[@]}"
    __package_file="${package_files[0]}"
}
_cmd_package() {
    local -r package_slug="$1" version="$2"
    _with_lock "package:${package_slug}" _package_transaction "${package_slug}" "${version}"
}
_package_transaction() {
    local -r package_slug="$1" version="$2"
    local project manifest_dir target_dir target_framework assembly_name target_ext project_dir yak_path yak_platform yak_push_source stage_dir package_pattern
    _package_meta "${package_slug}" "${version}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir yak_path yak_platform yak_push_source stage_dir package_pattern
    [[ -x "${yak_path}" ]] || _die "Yak not executable at ${yak_path}"
    [[ -n "${target_dir}" && "${target_dir}" == "${ROOT_DIR}/"* && "${target_dir}" == "${project_dir}/bin/${CONFIGURATION}/${target_framework}/" ]] || _die "Refusing to clean unexpected output directory: ${target_dir}"
    rm -rf -- "${target_dir:?}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore "/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${project}" --locked-mode --disable-parallel
    dotnet build "${project}" "${build_args[@]}" "${DOTNET_SERIAL_BUILD_ARGS[@]}"
    local -r primary_artifact="${target_dir}/${assembly_name}${target_ext}"
    local -r stage_root="${stage_dir%/package}"
    local -r previous_dir="${stage_root}/package.previous.$$"
    mkdir -p -- "${stage_root}"
    local stage_tmp
    stage_tmp="$(mktemp -d "${stage_root}/package.XXXXXX")"
    CLEANUP_PATHS+=("${stage_tmp}" "${previous_dir}")
    CLEANUP_ROLLBACKS+=("${stage_dir}|${previous_dir}")
    [[ -f "${manifest_dir}/manifest.yml" ]] || _die "Missing Yak manifest: ${manifest_dir}/manifest.yml"
    [[ -d "${target_dir}" ]] || _die "Missing build output: ${target_dir}"
    [[ -f "${primary_artifact}" ]] || _die "Missing primary package artifact: ${primary_artifact}"
    cp -p -- "${manifest_dir}/manifest.yml" "${stage_tmp}/manifest.yml"
    [[ -f "${manifest_dir}/icon.png" ]] && cp -p -- "${manifest_dir}/icon.png" "${stage_tmp}/icon.png"
    local -a host_excludes=()
    local host_exclude
    for host_exclude in "${RHINO_HOST_PACKAGE_EXCLUDES[@]}"; do
        host_excludes+=(--exclude "${host_exclude}")
    done
    fd -H -e rhp -e dll -e json "${host_excludes[@]}" . "${target_dir}" --max-depth 1 -X cp -p -- {} "${stage_tmp}/"
    local -a staged_plugins=()
    mapfile -t staged_plugins < <(fd -H -e rhp . "${stage_tmp}" --max-depth 1)
    ((${#staged_plugins[@]} == 1)) || _die "Expected one staged .rhp for ${package_slug}, found ${#staged_plugins[@]}"
    [[ -f "${stage_tmp}/manifest.yml" ]] || _die "Missing staged manifest for ${package_slug}"
    (cd -- "${stage_tmp}" && "${yak_path}" build --platform "${yak_platform}" --version "${version}")
    local package_file
    _package_find "${package_slug}" "${version}" "${stage_tmp}" "${package_pattern}" package_file
    rm -rf -- "${previous_dir}"
    [[ ! -e "${stage_dir}" ]] || mv -- "${stage_dir}" "${previous_dir}"
    mv -- "${stage_tmp}" "${stage_dir}"
    stage_tmp=""
    rm -rf -- "${previous_dir}"
}
_package_action() {
    local -r action="$1" package_slug="$2" version="$3"
    _cmd_package "${package_slug}" "${version}"
    local project manifest_dir target_dir target_framework assembly_name target_ext project_dir yak_path yak_platform yak_push_source package_dir package_pattern
    _package_meta "${package_slug}" "${version}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir yak_path yak_platform yak_push_source package_dir package_pattern
    local package_file
    _package_find "${package_slug}" "${version}" "${package_dir}" "${package_pattern}" package_file
    local -a source_args=()
    [[ -n "${yak_push_source}" ]] && source_args=(--source "${yak_push_source}")
    [[ -x "${yak_path}" ]] || _die "Yak not executable at ${yak_path}"
    case "${action}" in
        install) "${yak_path}" install "${package_file}" ;;
        deploy) "${yak_path}" install "${package_file}"; _package_deploy_refresh "${package_slug}" ;;
        push) "${yak_path}" push "${source_args[@]}" "${package_file}" ;;
        *) _die "Unknown package action: ${action}" ;;
    esac
}
_package_deploy_refresh() {
    local -r package_slug="$1"
    local lifecycle_json restart_json doctor_json
    if restart_json="$(_bridge_client restart)"; then
        lifecycle_json="${restart_json}"
    else
        lifecycle_json="$(_bridge_client launch)" || {
            printf '%s\n' "${restart_json}"
            printf '%s\n' "${lifecycle_json}"
            _die "Package deploy refresh failed for ${package_slug}"
        }
    fi
    readonly lifecycle_json
    printf '%s\n' "${lifecycle_json}"
    [[ "${package_slug}" == "rasm-bridge" ]] || return 0
    doctor_json="$(_bridge_client doctor)" || {
        printf '%s\n' "${doctor_json}"
        _die "Bridge doctor failed after deploying ${package_slug}"
    }
    readonly doctor_json
    printf '%s\n' "${doctor_json}"
    jq -er 'first(.phases[]? | select(.phase == "doctor") | .data.assemblies[]? | select(.name == "rasm-bridge")) // empty' >/dev/null <<< "${doctor_json}" || _die "Bridge doctor did not report rasm-bridge"
}
_main() {
    local route="${1:-}"
    [[ -n "${route}" ]] || _die "Usage: scripts/rhino.sh <route> [args]" 2
    shift
    case "${route}" in
        bridge|api)
            local -r subcommand="${1:-}"
            [[ -n "${subcommand}" ]] || _die "Usage: scripts/rhino.sh ${route} <subcommand> [args]" 2
            route="${route}:${subcommand}"
            shift
            ;;
    esac
    _dispatch "${route}" "$@"
}

_main "$@"

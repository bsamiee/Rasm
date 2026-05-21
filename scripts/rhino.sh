#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'rhino: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob
shopt -s array_expand_once 2>/dev/null || true
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly PACKAGE_STAGE_ROOT="${ROOT_DIR}/.artifacts/rhino"
readonly YAK_ROOT="${ROOT_DIR}/tools/yak"
readonly YAK_PATH="${RHINO_YAK_PATH:-/Applications/RhinoWIP.app/Contents/Resources/bin/yak}"
readonly CONFIGURATION="${CONFIGURATION:-Release}"
readonly API_RHINO_WIP_APP_PATH="${RHINO_WIP_APP_PATH:-/Applications/RhinoWIP.app}"
readonly API_RHINO_WIP_RESOURCES="${API_RHINO_WIP_APP_PATH}/Contents/Frameworks/RhCore.framework/Versions/A/Resources"
readonly API_RHINO_CODE="${API_RHINO_WIP_APP_PATH}/Contents/Resources/bin/rhinocode"
readonly BRIDGE_PROTOCOL_PROJECT="${ROOT_DIR}/tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
readonly BRIDGE_CLIENT_PROJECT="${ROOT_DIR}/tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
readonly BRIDGE_PLUGIN_PROJECT="${ROOT_DIR}/tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
readonly -a DOTNET_SERIAL_BUILD_ARGS=(-maxcpucount:1 -p:BuildInParallel=false)
declare -Ar PACKAGE_PROJECTS=(
    [radyab]="${ROOT_DIR}/apps/grasshopper/Radyab/Radyab.csproj"
    [rasm-bridge]="${BRIDGE_PLUGIN_PROJECT}"
)
declare -Ar API_REFERENCES=(
    [rhino-common]="${API_RHINO_WIP_RESOURCES}/RhinoCommon.dll|${API_RHINO_WIP_RESOURCES}/RhinoCommon.xml|${ROOT_DIR}/.cache/nuget/packages/rhinocommon/*/lib/net8.0/RhinoCommon.xml"
    [rhino-ui]="${API_RHINO_WIP_RESOURCES}/Rhino.UI.dll|${API_RHINO_WIP_RESOURCES}/Rhino.UI.xml|"
    [eto]="${API_RHINO_WIP_RESOURCES}/Eto.dll|${API_RHINO_WIP_RESOURCES}/Eto.xml|${ROOT_DIR}/.cache/nuget/packages/rhinocommon/*/lib/net8.0/Eto.xml"
    [gh2]="${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll|${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml|${ROOT_DIR}/.cache/nuget/packages/grasshopper2/*/ref/net7.0/Grasshopper2.xml"
    [gh2-io]="${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll|${API_RHINO_WIP_RESOURCES}/ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml|${ROOT_DIR}/.cache/nuget/packages/grasshopper2/*/ref/net7.0/GrasshopperIO.xml"
)
readonly -a API_REFERENCE_ORDER=(rhino-common rhino-ui eto gh2 gh2-io)
declare -a CLEANUP_PATHS=()
declare -a CLEANUP_LOCKS=()
declare -a CLEANUP_ROLLBACKS=()
CLEANING=0
declare -Ar ROUTES=(
    [--self-test]='_self_test|0|0|--self-test|'
    [build]='_build|0|1|build [version]|'
    [verify]='_verify|1|1|verify <path-or-glob>|'
    [package]='_cmd_package|2|2|package <package> <version>|'
    [push-test]='_push_package|2|2|push-test <package> <version>|https://test.yak.rhino3d.com'
    [push]='_push_package|2|2|push <package> <version>|-'
    [bridge:build]='_bridge_build|0|0|bridge build|'
    [bridge:package]='_cmd_package|1|1|bridge package <version>|rasm-bridge'
    [bridge:install]='_bridge_install|1|1|bridge install <local-yak-path>|'
    [bridge:launch]='_bridge_client|0|0|bridge launch|launch'
    [bridge:restart]='_bridge_client|0|0|bridge restart|restart'
    [bridge:doctor]='_bridge_client|0|999|bridge doctor [client options]|doctor'
    [bridge:script]='_bridge_client|1|999|bridge script <script> [client options]|script'
    [bridge:load]='_bridge_client|1|999|bridge load <assembly.dll> [client options]|load'
    [bridge:load-smoke]='_bridge_client|1|999|bridge load-smoke <assembly.dll> [client options]|load-smoke'
    [bridge:check]='_bridge_client|1|999|bridge check <project.csproj> [client options]|check'
    [bridge:check-source]='_bridge_client|1|999|bridge check-source <source.cs> [client options]|check-source'
    [bridge:unload]='_bridge_client|1|1|bridge unload <session-id>|unload'
    [bridge:quit]='_bridge_client|0|0|bridge quit|quit'
    [api:doctor]='_api_doctor|0|0|api doctor|'
    [api:path]='_api_path|1|2|api path <api-key> [assembly/xml]|'
    [api:xml]='_api_xml|2|2|api xml <api-key> <pattern>|'
    [api:types]='_api_types|1|2|api types <api-key> [pattern]|'
    [api:decompile]='_api_decompile|2|2|api decompile <api-key> <type>|'
)
readonly -a ROUTE_ORDER=(--self-test build verify bridge:build bridge:package bridge:install bridge:launch bridge:restart bridge:doctor bridge:script bridge:load bridge:load-smoke bridge:check bridge:check-source bridge:unload bridge:quit api:doctor api:path api:xml api:types api:decompile package push-test push)
readonly -a BRIDGE_PROJECTS=("${BRIDGE_PROTOCOL_PROJECT}" "${BRIDGE_PLUGIN_PROJECT}" "${BRIDGE_CLIENT_PROJECT}")
_trap_err() {
    local -r exit_code="$?"
    local -r source="${BASH_SOURCE[1]:-${BASH_SOURCE[0]}}"
    local -r line="${BASH_LINENO[0]:-0}"
    local index
    printf 'rhino: error at %s:%s: %s\n' "${source}" "${line}" "${BASH_COMMAND}" >&2
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
    exit 1
}
_die_usage() {
    _usage "${1:-all}" >&2
    exit 2
}
_route_meta() {
    local -r route="$1"
    local -n __handler="$2" __min="$3" __max="$4" __line="$5" __preset="$6"
    IFS='|' read -r __handler __min __max __line __preset <<< "${ROUTES[${route}]}"
}
_usage() {
    local -r scope="${1:-all}"
    printf 'Usage:\n'
    local route handler min max line preset
    for route in "${ROUTE_ORDER[@]}"; do
        [[ "${scope}" == bridge && "${route}" != bridge:* ]] && continue
        [[ "${scope}" == api && "${route}" != api:* ]] && continue
        _route_meta "${route}" handler min max line preset
        printf '  scripts/rhino.sh %s\n' "${line}"
    done
}
_dispatch() {
    local -r route="$1"
    shift
    [[ -v ROUTES["${route}"] ]] || _die_usage "${route%%:*}"
    local handler min max line preset
    _route_meta "${route}" handler min max line preset
    (($# >= min && $# <= max)) || _die_usage "${route%%:*}"
    local -a call_args=("$@")
    [[ -n "${preset}" && "${preset}" != '-' ]] && call_args=("${preset}" "$@")
    [[ "${preset}" == '-' ]] && call_args=("" "$@")
    "${handler}" "${call_args[@]}"
}
_self_test() {
    local command
    for command in dotnet fd ilspycmd jq plutil rg shellcheck; do
        command -v "${command}" >/dev/null 2>&1 || _die "${command} is required"
    done
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local -a required=("${SOLUTION_PATH}" "${BRIDGE_PROJECTS[@]}" "${PACKAGE_PROJECTS[@]}")
    local path route handler min max line preset package_slug project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    declare -A ordered_routes=()
    for path in "${required[@]}"; do
        [[ -e "${path}" ]] || _die "Missing required path: ${path}"
    done
    for route in "${ROUTE_ORDER[@]}"; do
        [[ ! -v ordered_routes["${route}"] ]] || _die "Route listed more than once: ${route}"
        ordered_routes["${route}"]=1
        [[ -v ROUTES["${route}"] ]] || _die "Route missing metadata: ${route}"
        _route_meta "${route}" handler min max line preset
        declare -F "${handler}" >/dev/null || _die "Route handler missing: ${route} -> ${handler}"
        [[ "${min}" =~ ^[0-9]+$ && "${max}" =~ ^[0-9]+$ && -n "${line}" ]] || _die "Route metadata invalid: ${route}"
        ((min <= max)) || _die "Route arity invalid: ${route}"
        [[ "${handler}" != "_bridge_client" ]] || [[ "${route}" == bridge:* && "${preset}" == "${route#bridge:}" ]] || _die "Bridge client preset invalid: ${route}"
    done
    for route in "${!ROUTES[@]}"; do
        [[ -v ordered_routes["${route}"] ]] || _die "Route missing from order: ${route}"
    done
    declare -Ar route_contracts=(
        [bridge:package]='_cmd_package|rasm-bridge|1|1'
        [bridge:install]='_bridge_install||1|1'
        [bridge:quit]='_bridge_client|quit|0|0'
        [push]='_push_package|-|2|2'
        [push-test]='_push_package|https://test.yak.rhino3d.com|2|2'
    )
    for route in "${!route_contracts[@]}"; do
        _route_meta "${route}" handler min max line preset
        [[ "${handler}|${preset}|${min}|${max}" == "${route_contracts[${route}]}" ]] || _die "Route contract invalid: ${route}"
    done
    for package_slug in "${!PACKAGE_PROJECTS[@]}"; do
        _package_meta "${package_slug}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir
        [[ -f "${manifest_dir}/manifest.yml" ]] || _die "Missing Yak manifest for ${package_slug}: ${manifest_dir}/manifest.yml"
        [[ -d "${project_dir}" && -f "${YAK_ROOT}/${package_slug}/manifest.yml" ]] || _die "Package wiring invalid for ${package_slug}"
        [[ -n "${target_dir}" && "${target_dir}" == "${ROOT_DIR}/"* && "${target_dir}" == "${project_dir}/bin/${CONFIGURATION}/${target_framework}/" ]] || _die "Package target directory unsafe for ${package_slug}: ${target_dir}"
    done
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
    _with_lock bridge-client-build _bridge_client_locked "$@"
}
_bridge_client_locked() {
    _bridge_client_ensure
    _bridge_client_invoke "$@" || exit "$?"
}
_bridge_client_ensure() {
    local -r lock_root="${PACKAGE_STAGE_ROOT}/locks"
    local build_log
    build_log="$(mktemp "${lock_root}/bridge-client-build.XXXXXXXXXX")"
    CLEANUP_PATHS+=("${build_log}")
    dotnet restore "${BRIDGE_CLIENT_PROJECT}" --locked-mode --disable-parallel >"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client restore failed"; }
    dotnet build "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" --no-restore "${DOTNET_SERIAL_BUILD_ARGS[@]}" >>"${build_log}" 2>&1 || { cat "${build_log}" >&2; _die "Bridge client build failed"; }
    rm -f -- "${build_log}"
}
_bridge_client_invoke() {
    dotnet run --no-build --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" -- "$@"
}
_verify() {
    local -r pattern="$1"
    _with_lock bridge-client-build _verify_locked "${pattern}"
}
_verify_locked() {
    local -r pattern="$1"
    local -a scenarios=()
    _verify_discover "${pattern}" scenarios
    ((${#scenarios[@]} > 0)) || _die "No *.verify.csx scenarios matched: ${pattern}"
    _bridge_client_ensure
    _verify_run "${scenarios[@]}"
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
_verify_run() {
    local -a scenarios=("$@")
    local -r report_dir="${PACKAGE_STAGE_ROOT}/verify"
    local -r capture_dir="${report_dir}/captures"
    mkdir -p -- "${report_dir}" "${capture_dir}"
    CLEANUP_PATHS+=("${report_dir}/.tmp")
    mkdir -p -- "${report_dir}/.tmp"
    local -a result_files=()
    local scenario name wrapped result rc status ok=0 failed=0
    for scenario in "${scenarios[@]}"; do
        name="${scenario##*/}"; name="${name%.verify.csx}"
        wrapped="${report_dir}/.tmp/${name}.csx"
        result="${report_dir}/${name}.json"
        _verify_wrap "${scenario}" "${name}" "${capture_dir}/${name}.png" "${wrapped}"
        rc=0
        _bridge_client_invoke script "${wrapped}" --result "${result}" >/dev/null 2>&1 || rc=$?
        status="$(jq -r '.status // "failed"' "${result}" 2>/dev/null || printf 'failed')"
        result_files+=("${result}")
        case "${status}" in
            ok) ((ok++)); printf '[OK]     %s capture=%s\n' "${scenario#"${ROOT_DIR}/"}" "${capture_dir}/${name}.png" >&2 ;;
            *) ((failed++)); printf '[FAILED] %s rc=%s status=%s result=%s\n' "${scenario#"${ROOT_DIR}/"}" "${rc}" "${status}" "${result}" >&2 ;;
        esac
    done
    local -r summary="${report_dir}/summary.json"
    jq -n --argjson ok "${ok}" --argjson failed "${failed}" --slurpfile s <(jq -s '.' "${result_files[@]}") '{summary:{ok:$ok,failed:$failed,total:($ok+$failed)},scenarios:$s[0]}' > "${summary}"
    cat "${summary}"
    ((failed == 0)) || exit 1
}
_verify_wrap() {
    local -r source="$1" name="$2" capture="$3" wrapped="$4"
    {
        printf 'const string SCENARIO_NAME = "%s";\n' "${name//\"/\\\"}"
        printf 'const string CAPTURE_PATH = "%s";\n' "${capture//\"/\\\"}"
        cat -- "${source}"
    } > "${wrapped}"
}
_api_meta() {
    local -r key="$1"
    local -n __assembly="$2" __xml="$3" __fallback="$4"
    [[ -v API_REFERENCES["${key}"] ]] || _die "Unknown API key: ${key}"
    IFS='|' read -r __assembly __xml __fallback <<< "${API_REFERENCES[${key}]}"
}
_api_fallback_xml() {
    local -r pattern="$1"
    [[ -n "${pattern}" ]] || return 1
    local -a matches=()
    mapfile -t matches < <({ compgen -G "${pattern}" || true; } | LC_ALL=C sort)
    ((${#matches[@]} > 0)) || return 1
    printf '%s\n' "${matches[${#matches[@]} - 1]}"
}
_api_xml_path() {
    local -r key="$1"
    local -n __path="$2" __status="$3"
    local meta_assembly meta_xml meta_fallback
    _api_meta "${key}" meta_assembly meta_xml meta_fallback
    : "${meta_assembly}"
    if [[ -f "${meta_xml}" ]]; then
        __path="${meta_xml}"
        __status="primary"
        return 0
    fi
    if __path="$(_api_fallback_xml "${meta_fallback}")"; then
        __status="fallback"
        return 0
    fi
    __path=""
    __status="missing"
    return 1
}
_dotnet_root_valid() {
    local -r root="$1"
    [[ -n "${root}" && -d "${root}/host/fxr" && -d "${root}/shared/Microsoft.NETCore.App" ]]
}
_dotnet_root_resolve() {
    local -a candidates=()
    [[ -n "${DOTNET_ROOT:-}" ]] && candidates=("${DOTNET_ROOT}" "${DOTNET_ROOT}/share/dotnet")
    candidates+=("/usr/local/share/dotnet")
    local root
    for root in "${candidates[@]}"; do
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
_api_path() {
    local -r key="$1"
    local -r kind="${2:-assembly}"
    local assembly xml fallback resolved status
    _api_meta "${key}" assembly xml fallback
    case "${kind}" in
        assembly)
            [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"
            printf '%s\n' "${assembly}"
            ;;
        xml)
            _api_xml_path "${key}" resolved status || _die "Missing API XML for ${key}: ${xml}"
            printf '%s\n' "${resolved}"
            ;;
        *) _die "Unknown API path kind: ${kind}" ;;
    esac
}
_api_xml() {
    local -r key="$1"
    local -r pattern="$2"
    local xml status
    _api_xml_path "${key}" xml status || _die "Missing API XML for ${key}"
    rg -n -C 2 -- "${pattern}" "${xml}"
}
_api_types() {
    local -r key="$1"
    local -r pattern="${2:-}"
    local assembly xml fallback
    _api_meta "${key}" assembly xml fallback
    [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"
    [[ -n "${pattern}" ]] && { _with_dotnet_apphost ilspycmd -l c "${assembly}" | rg -n -- "${pattern}"; return; }
    _with_dotnet_apphost ilspycmd -l c "${assembly}"
}
_api_decompile() {
    local -r key="$1"
    local -r type_name="$2"
    local assembly xml fallback
    _api_meta "${key}" assembly xml fallback
    [[ -f "${assembly}" ]] || _die "Missing API assembly for ${key}: ${assembly}"
    _with_dotnet_apphost ilspycmd -t "${type_name}" "${assembly}"
}
_api_rhino_version() {
    local -r plist="${API_RHINO_WIP_APP_PATH}/Contents/Info.plist"
    [[ -f "${plist}" ]] || { printf 'missing'; return; }
    plutil -extract CFBundleVersion raw -o - "${plist}" 2>/dev/null || printf 'unknown'
}
_api_ilspy_report() {
    local dotnet_root status version
    dotnet_root="$(_dotnet_root_resolve || true)"
    status="failed"
    version=""
    local output
    mkdir -p -- "${PACKAGE_STAGE_ROOT}"
    output="$(mktemp "${PACKAGE_STAGE_ROOT}/ilspy.XXXXXXXXXX")"
    CLEANUP_PATHS+=("${output}")
    _with_dotnet_apphost ilspycmd --version >"${output}" 2>&1 && status="ok"
    IFS= read -r version < "${output}" || true
    printf 'api.ilspy.status\t%s\napi.ilspy.dotnet_root\t%s\napi.ilspy.version\t%s\n' "${status}" "${dotnet_root:-hostfxr-probe}" "${version:-unavailable}"
}
_api_rhinocode_report() {
    local direct roll
    [[ -x "${API_RHINO_CODE}" ]] || { printf 'api.rhinocode.path\t%s\napi.rhinocode.direct\tmissing\napi.rhinocode.roll_forward\tmissing\n' "${API_RHINO_CODE}"; return; }
    direct=0
    roll=0
    "${API_RHINO_CODE}" list --json >/dev/null 2>&1 || direct="$?"
    DOTNET_ROLL_FORWARD=Major "${API_RHINO_CODE}" list --json >/dev/null 2>&1 || roll="$?"
    printf 'api.rhinocode.path\t%s\napi.rhinocode.direct\t%s\napi.rhinocode.roll_forward\t%s\n' "${API_RHINO_CODE}" "${direct}" "${roll}"
}
_api_doctor() {
    printf 'api.rhino.app\t%s\napi.rhino.version\t%s\n' "${API_RHINO_WIP_APP_PATH}" "$(_api_rhino_version)"
    _api_ilspy_report
    _api_rhinocode_report
    local key assembly xml fallback resolved status assembly_status
    for key in "${API_REFERENCE_ORDER[@]}"; do
        _api_meta "${key}" assembly xml fallback
        : "${fallback}"
        assembly_status="missing"
        [[ -f "${assembly}" ]] && assembly_status="present"
        _api_xml_path "${key}" resolved status || true
        printf 'api.ref.%s.assembly\t%s\t%s\n' "${key}" "${assembly_status}" "${assembly}"
        printf 'api.ref.%s.xml\t%s\t%s\n' "${key}" "${status}" "${resolved:-${xml}}"
    done
}
_bridge_install() {
    local -r package_path="$1"
    local -r package_name="${package_path##*/}"
    [[ "${package_name}" =~ ^rasm-bridge-(.+)-rh[0-9_]+-mac[.]yak$ ]] || _die "Could not read bridge package version from ${package_name}"
    local -r expected_version="${BASH_REMATCH[1]}"
    [[ -x "${YAK_PATH}" ]] || _die "Yak not executable at ${YAK_PATH}"
    [[ -f "${package_path}" ]] || _die "Missing Yak package: ${package_path}"
    "${YAK_PATH}" install "${package_path}"
    local lifecycle_json
    local restart_json
    if restart_json="$(_bridge_client restart)"; then
        lifecycle_json="${restart_json}"
    else
        printf '%s\n' "${restart_json}"
        lifecycle_json="$(_bridge_client launch)" || {
            printf '%s\n' "${lifecycle_json}"
            _die "Bridge restart/launch failed after install"
        }
    fi
    readonly lifecycle_json
    printf '%s\n' "${lifecycle_json}"
    local doctor_json
    doctor_json="$(_bridge_client doctor)" || {
        printf '%s\n' "${doctor_json}"
        _die "Bridge doctor failed after install"
    }
    readonly doctor_json
    printf '%s\n' "${doctor_json}"
    local live_version
    live_version="$(jq -er 'first(.phases[] | select(.phase == "doctor") | .data.assemblies[] | select(.name == "Rasm.RhinoBridge.Plugin") | (.informationalVersion // .version)) // empty' <<< "${doctor_json}")" || _die "Bridge doctor did not report Rasm.RhinoBridge.Plugin"
    readonly live_version
    [[ "${live_version}" == "${expected_version}" ]] || _die "Installed bridge version mismatch: expected ${expected_version}, live ${live_version}"
}
_package_meta() {
    local -r package_slug="$1"
    shift
    local -n __project="$1" __manifest_dir="$2" __target_dir="$3" __target_framework="$4" __assembly_name="$5" __target_ext="$6" __project_dir="$7"
    [[ -v PACKAGE_PROJECTS["${package_slug}"] ]] || _die "Unknown package: ${package_slug}"
    __project="${PACKAGE_PROJECTS[${package_slug}]}"
    local payload
    payload="$(
        dotnet msbuild "${__project}" \
            -p:Configuration="${CONFIGURATION}" \
            -getProperty:YakPackageSlug \
            -getProperty:YakManifestDirectory \
            -getProperty:TargetDir \
            -getProperty:TargetFramework \
            -getProperty:AssemblyName \
            -getProperty:TargetExt \
            -getProperty:MSBuildProjectDirectory \
            -nologo
    )"
    readonly payload
    local -a fields=()
    mapfile -t fields < <(jq -er '.Properties | .YakPackageSlug, .YakManifestDirectory, .TargetDir, .TargetFramework, .AssemblyName, .TargetExt, .MSBuildProjectDirectory' <<< "${payload}")
    ((${#fields[@]} == 7)) || _die "Could not evaluate package metadata for ${package_slug}"
    [[ "${fields[0]}" == "${package_slug}" ]] || _die "Package slug mismatch for ${__project}: expected ${package_slug}, evaluated ${fields[0]}"
    __manifest_dir="${fields[1]}"
    __target_dir="${fields[2]}"
    __target_framework="${fields[3]}"
    __assembly_name="${fields[4]}"
    __target_ext="${fields[5]}"
    __project_dir="${fields[6]}"
    [[ -n "${__target_framework}" && -n "${__assembly_name}" ]] || _die "Package project metadata is incomplete for ${package_slug}: ${__project}"
    [[ "${__target_ext}" == ".rhp" ]] || _die "Package project must emit .rhp for ${package_slug}: ${__project}"
}
_cmd_package() {
    local -r package_slug="$1"
    local -r version="$2"
    _with_lock "package:${package_slug}" _package_transaction "${package_slug}" "${version}"
}
_package_transaction() {
    local -r package_slug="$1"
    local -r version="$2"
    [[ -x "${YAK_PATH}" ]] || _die "Yak not executable at ${YAK_PATH}"
    local project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    _package_meta "${package_slug}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    [[ -n "${target_dir}" && "${target_dir}" == "${ROOT_DIR}/"* && "${target_dir}" == "${project_dir}/bin/${CONFIGURATION}/${target_framework}/" ]] || _die "Refusing to clean unexpected output directory: ${target_dir}"
    rm -rf -- "${target_dir:?}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore "/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${project}" --locked-mode --disable-parallel
    dotnet build "${project}" "${build_args[@]}" "${DOTNET_SERIAL_BUILD_ARGS[@]}"
    local -r primary_artifact="${target_dir}/${assembly_name}${target_ext}"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -r stage_root="${PACKAGE_STAGE_ROOT}/${package_slug}"
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
    fd -H -e rhp -e dll -e json --exclude 'RhinoCommon.*' --exclude 'Grasshopper2.*' --exclude 'GrasshopperIO.*' . "${target_dir}" --max-depth 1 -X cp -p -- {} "${stage_tmp}/"
    local -a staged_plugins=()
    mapfile -t staged_plugins < <(fd -H -e rhp . "${stage_tmp}" --max-depth 1)
    ((${#staged_plugins[@]} == 1)) || _die "Expected one staged .rhp for ${package_slug}, found ${#staged_plugins[@]}"
    [[ -f "${stage_tmp}/manifest.yml" ]] || _die "Missing staged manifest for ${package_slug}"
    (cd -- "${stage_tmp}" && "${YAK_PATH}" build --platform mac --version "${version}")
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "*-${version}-*-mac.yak" . "${stage_tmp}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one staged Yak package for ${package_slug} ${version}, found ${#package_files[@]}"
    rm -rf -- "${previous_dir}"
    [[ ! -e "${stage_dir}" ]] || mv -- "${stage_dir}" "${previous_dir}"
    mv -- "${stage_tmp}" "${stage_dir}"
    stage_tmp=""
    rm -rf -- "${previous_dir}"
}
_push_package() {
    local -r source="$1"
    local -r package_slug="$2"
    local -r version="$3"
    local -a source_args=()
    [[ -n "${source}" ]] && source_args=(--source "${source}")
    _cmd_package "${package_slug}" "${version}"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "*-${version}-*-mac.yak" . "${stage_dir}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one Yak package for ${package_slug} ${version}, found ${#package_files[@]}"
    "${YAK_PATH}" push "${source_args[@]}" "${package_files[0]}"
}
_main() {
    local route="${1:-}"
    [[ -n "${route}" ]] || _die_usage all
    shift
    if [[ "${route}" == bridge ]]; then
        local -r bridge_command="${1:-}"
        [[ -n "${bridge_command}" ]] || _die_usage bridge
        route="bridge:${bridge_command}"
        shift
    fi
    if [[ "${route}" == api ]]; then
        local -r api_command="${1:-}"
        [[ -n "${api_command}" ]] || _die_usage api
        route="api:${api_command}"
        shift
    fi
    _dispatch "${route}" "$@"
}

_main "$@"

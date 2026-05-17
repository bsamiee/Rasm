#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit nullglob
IFS=$'\n\t'
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly PACKAGE_STAGE_ROOT="${ROOT_DIR}/.artifacts/rhino"
readonly YAK_ROOT="${ROOT_DIR}/tools/yak"
readonly YAK_PATH="${RHINO_YAK_PATH:-/Applications/RhinoWIP.app/Contents/Resources/bin/yak}"
readonly CONFIGURATION="${CONFIGURATION:-Release}"
readonly FRAMEWORK="${FRAMEWORK:-net10.0}"
readonly BRIDGE_CONTRACTS_PROJECT="${ROOT_DIR}/tools/rhino-bridge/contracts/Rasm.RhinoBridge.Contracts.csproj"
readonly BRIDGE_PROTOCOL_PROJECT="${ROOT_DIR}/tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
readonly BRIDGE_CLIENT_PROJECT="${ROOT_DIR}/tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
readonly BRIDGE_PLUGIN_PROJECT="${ROOT_DIR}/tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
readonly BRIDGE_CLIENT_DLL="${ROOT_DIR}/tools/rhino-bridge/client/bin/${CONFIGURATION}/${FRAMEWORK}/Rasm.RhinoBridge.Client.dll"
declare -Ar ROUTES=(
    [--self-test]='_self_test|0|0|--self-test||hard'
    [build]='_build|0|1|build [version]||hard'
    [package]='_cmd_package|2|2|package <package> <version>||hard'
    [push-test]='_push_package|2|2|push-test <package> <version>|https://test.yak.rhino3d.com|hard'
    [push]='_push_package|2|2|push <package> <version>|-|hard'
    [bridge:build]='_bridge_build|0|0|bridge build||hard'
    [bridge:package]='_cmd_package|1|1|bridge package <version>|rasm-bridge|hard'
    [bridge:install]='_yak_install|1|1|bridge install <local-yak-path>||hard'
    [bridge:launch]='_bridge_client|0|0|bridge launch|launch|soft'
    [bridge:doctor]='_bridge_client|0|0|bridge doctor|doctor|soft'
    [bridge:load]='_bridge_client|1|999|bridge load <assembly.dll> [client options]|load|soft'
    [bridge:run]='_bridge_client|1|999|bridge run <session-id> [client options]|run|soft'
    [bridge:check]='_bridge_client|1|999|bridge check <project.csproj> [client options]|check|soft'
    [bridge:unload]='_bridge_client|1|1|bridge unload <session-id>|unload|soft'
)
readonly -a ROUTE_ORDER=(--self-test build bridge:build bridge:package bridge:install bridge:launch bridge:doctor bridge:load bridge:run bridge:check bridge:unload package push-test push)
readonly -a BRIDGE_PROJECTS=("${BRIDGE_CONTRACTS_PROJECT}" "${BRIDGE_PROTOCOL_PROJECT}" "${BRIDGE_PLUGIN_PROJECT}" "${BRIDGE_CLIENT_PROJECT}")
declare -Ar PACKAGE_ROOTS=([radyab]='apps/grasshopper/Radyab' [rasm-bridge]='tools/rhino-bridge/plugin')
_trap_err() {
    local -r exit_code="$?"
    printf 'rhino: error at %s:%s: %s\n' "${BASH_SOURCE[1]}" "${BASH_LINENO[0]}" "${BASH_COMMAND}" >&2
    exit "${exit_code}"
}
trap _trap_err ERR
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
    local -a parts=()
    IFS='|' read -r -a parts <<< "${ROUTES[${route}]}"
    printf -v "$2" '%s' "${parts[0]:-}"
    printf -v "$3" '%s' "${parts[1]:-}"
    printf -v "$4" '%s' "${parts[2]:-}"
    printf -v "$5" '%s' "${parts[3]:-}"
    printf -v "$6" '%s' "${parts[4]:-}"
    printf -v "$7" '%s' "${parts[5]:-hard}"
}
_usage() {
    local -r scope="${1:-all}"
    printf 'Usage:\n'
    local route handler min max line preset mode
    for route in "${ROUTE_ORDER[@]}"; do
        [[ "${scope}" == bridge && "${route}" != bridge:* ]] && continue
        _route_meta "${route}" handler min max line preset mode
        printf '  scripts/rhino.sh %s\n' "${line}"
    done
}
_dispatch() {
    local -r route="$1"
    shift
    [[ -v ROUTES["${route}"] ]] || _die_usage "${route%%:*}"
    local handler min max line preset mode
    _route_meta "${route}" handler min max line preset mode
    (($# >= min && $# <= max)) || _die_usage "${route%%:*}"
    local -a call_args=("$@")
    [[ -n "${preset}" && "${preset}" != '-' ]] && call_args=("${preset}" "$@")
    [[ "${preset}" == '-' ]] && call_args=("" "$@")
    "${handler}" "${call_args[@]}"
}
_self_test() {
    ((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 2))) || _die "Bash 5.2+ is required"
    command -v dotnet >/dev/null 2>&1 || _die "dotnet is required"
    command -v fd >/dev/null 2>&1 || _die "fd is required"
    command -v shellcheck >/dev/null 2>&1 || _die "shellcheck is required"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local -a required=("${SOLUTION_PATH}" "${BRIDGE_PROJECTS[@]}")
    local path route handler min max line preset mode package_slug
    local -a roots=()
    for path in "${required[@]}"; do
        [[ -e "${path}" ]] || _die "Missing required path: ${path}"
    done
    for route in "${ROUTE_ORDER[@]}"; do
        [[ -v ROUTES["${route}"] ]] || _die "Route missing metadata: ${route}"
        _route_meta "${route}" handler min max line preset mode
        declare -F "${handler}" >/dev/null || _die "Route handler missing: ${route} -> ${handler}"
        [[ "${min}" =~ ^[0-9]+$ && "${max}" =~ ^[0-9]+$ && -n "${line}" && "${mode}" =~ ^(hard|soft)$ ]] || _die "Route metadata invalid: ${route}"
        ((min <= max)) || _die "Route arity invalid: ${route}"
    done
    _route_meta bridge:package handler min max line preset mode
    [[ "${handler}|${preset}|${min}|${max}" == '_cmd_package|rasm-bridge|1|1' ]] || _die "bridge package preset invalid"
    _route_meta push handler min max line preset mode
    [[ "${handler}|${preset}|${min}|${max}" == '_push_package|-|2|2' ]] || _die "push preset invalid"
    _route_meta push-test handler min max line preset mode
    [[ "${handler}|${preset}|${min}|${max}" == '_push_package|https://test.yak.rhino3d.com|2|2' ]] || _die "push-test preset invalid"
    for package_slug in "${!PACKAGE_ROOTS[@]}"; do
        [[ -f "${YAK_ROOT}/${package_slug}/manifest.yml" ]] || _die "Missing Yak manifest for ${package_slug}"
        _package_roots "${package_slug}" roots
        for path in "${roots[@]}"; do
            [[ -d "${ROOT_DIR}/${path}" ]] || _die "Missing plugin root for ${package_slug}: ${path}"
        done
    done
}
_build() {
    local -r version="${1:-}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore)
    [[ -n "${version}" ]] && build_args+=("/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${SOLUTION_PATH}" --locked-mode
    dotnet build "${SOLUTION_PATH}" "${build_args[@]}"
}
_bridge_build() {
    dotnet restore "${SOLUTION_PATH}" --locked-mode
    local project
    for project in "${BRIDGE_PROJECTS[@]}"; do
        dotnet build "${project}" --configuration "${CONFIGURATION}" --no-restore
    done
}
_bridge_client() {
    local -a client=(dotnet run --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" --)
    [[ -f "${BRIDGE_CLIENT_DLL}" ]] && client=(dotnet "${BRIDGE_CLIENT_DLL}")
    "${client[@]}" "$@"
}
_package_roots() {
    local -r package_slug="$1"
    local -r roots_name="$2"
    [[ -v PACKAGE_ROOTS["${package_slug}"] ]] || _die "Unknown package: ${package_slug}"
    IFS=' ' read -r -a "${roots_name?}" <<< "${PACKAGE_ROOTS[${package_slug}]}"
}
_require_yak() { [[ -x "${YAK_PATH}" ]] || _die "Yak not executable at ${YAK_PATH}"; }
_yak_install() {
    local -r package_path="$1"
    _require_yak
    [[ -f "${package_path}" ]] || _die "Missing Yak package: ${package_path}"
    "${YAK_PATH}" install "${package_path}"
}
_cmd_package() {
    local -r package_slug="$1"
    local -r version="$2"
    _require_yak
    local -a roots=()
    _package_roots "${package_slug}" roots
    local project_dir
    for project_dir in "${roots[@]}"; do
        rm -rf -- "${ROOT_DIR:?}/${project_dir:?}/bin/${CONFIGURATION:?}/${FRAMEWORK:?}"
    done
    _build "${version}"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -r yak_source_dir="${YAK_ROOT}/${package_slug}"
    rm -rf -- "${stage_dir}"
    mkdir -p -- "${stage_dir}"
    [[ -f "${yak_source_dir}/manifest.yml" ]] || _die "Missing Yak manifest: ${yak_source_dir}/manifest.yml"
    cp -p -- "${yak_source_dir}/manifest.yml" "${stage_dir}/manifest.yml"
    [[ -f "${yak_source_dir}/icon.png" ]] && cp -p -- "${yak_source_dir}/icon.png" "${stage_dir}/icon.png"
    local output_dir
    for project_dir in "${roots[@]}"; do
        output_dir="${ROOT_DIR}/${project_dir}/bin/${CONFIGURATION}/${FRAMEWORK}"
        [[ -d "${output_dir}" ]] || _die "Missing build output: ${output_dir}"
        fd -H -e rhp -e dll -e json --exclude 'RhinoCommon.*' --exclude 'Grasshopper2.*' --exclude 'GrasshopperIO.*' . "${output_dir}" --max-depth 1 -X cp -p -- {} "${stage_dir}/"
    done
    (cd -- "${stage_dir}" && "${YAK_PATH}" build --platform mac --version "${version}")
}
_package_path() {
    local -r package_slug="$1"
    local -r version="$2"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "*-${version}-*-mac.yak" . "${stage_dir}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one Yak package for ${package_slug} ${version}, found ${#package_files[@]}"
    printf '%s\n' "${package_files[0]}"
}
_push_package() {
    local -r source="$1"
    local -r package_slug="$2"
    local -r version="$3"
    local -a source_args=()
    [[ -n "${source}" ]] && source_args=(--source "${source}")
    _cmd_package "${package_slug}" "${version}"
    local package_path
    package_path="$(_package_path "${package_slug}" "${version}")"
    readonly package_path
    "${YAK_PATH}" push "${source_args[@]}" "${package_path}"
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
    [[ -v ROUTES["${route}"] ]] || _die_usage "${route%%:*}"
    local handler min max line preset mode
    _route_meta "${route}" handler min max line preset mode
    if [[ "${mode}" == soft ]]; then
        _dispatch "${route}" "$@" || exit "$?"
        exit 0
    fi
    _dispatch "${route}" "$@"
}

_main "$@"

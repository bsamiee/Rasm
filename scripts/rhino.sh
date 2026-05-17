#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 2))) || { printf 'rhino: Bash 5.2+ is required\n' >&2; exit 1; }
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
readonly BRIDGE_PROTOCOL_PROJECT="${ROOT_DIR}/tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
readonly BRIDGE_CLIENT_PROJECT="${ROOT_DIR}/tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
readonly BRIDGE_PLUGIN_PROJECT="${ROOT_DIR}/tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
declare -Ar ROUTES=(
    [--self-test]='_self_test|0|0|--self-test|'
    [build]='_build|0|1|build [version]|'
    [package]='_cmd_package|2|2|package <package> <version>|'
    [push-test]='_push_package|2|2|push-test <package> <version>|https://test.yak.rhino3d.com'
    [push]='_push_package|2|2|push <package> <version>|-'
    [bridge:build]='_bridge_build|0|0|bridge build|'
    [bridge:package]='_cmd_package|1|1|bridge package <version>|rasm-bridge'
    [bridge:install]='_yak_install|1|1|bridge install <local-yak-path>|'
    [bridge:launch]='_bridge_client|0|0|bridge launch|launch'
    [bridge:restart]='_bridge_client|0|0|bridge restart|restart'
    [bridge:doctor]='_bridge_client|0|0|bridge doctor|doctor'
    [bridge:script]='_bridge_client|1|999|bridge script <script> [client options]|script'
    [bridge:load]='_bridge_client|1|999|bridge load <assembly.dll> [client options]|load'
    [bridge:load-smoke]='_bridge_client|1|999|bridge load-smoke <assembly.dll> [client options]|load-smoke'
    [bridge:check]='_bridge_client|1|999|bridge check <project.csproj> [client options]|check'
    [bridge:check-source]='_bridge_client|1|999|bridge check-source <source.cs> [client options]|check-source'
    [bridge:unload]='_bridge_client|1|1|bridge unload <session-id>|unload'
    [bridge:quit]='_bridge_client|0|0|bridge quit|quit'
)
readonly -a ROUTE_ORDER=(--self-test build bridge:build bridge:package bridge:install bridge:launch bridge:restart bridge:doctor bridge:script bridge:load bridge:load-smoke bridge:check bridge:check-source bridge:unload bridge:quit package push-test push)
readonly -a BRIDGE_PROJECTS=("${BRIDGE_PROTOCOL_PROJECT}" "${BRIDGE_PLUGIN_PROJECT}" "${BRIDGE_CLIENT_PROJECT}")
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
    local -n __handler="$2" __min="$3" __max="$4" __line="$5" __preset="$6"
    IFS='|' read -r __handler __min __max __line __preset <<< "${ROUTES[${route}]}"
}
_usage() {
    local -r scope="${1:-all}"
    printf 'Usage:\n'
    local route handler min max line preset
    for route in "${ROUTE_ORDER[@]}"; do
        [[ "${scope}" == bridge && "${route}" != bridge:* ]] && continue
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
    for command in dotnet fd jq shellcheck; do
        command -v "${command}" >/dev/null 2>&1 || _die "${command} is required"
    done
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    local -a required=("${SOLUTION_PATH}" "${BRIDGE_PROJECTS[@]}")
    local path route handler min max line preset package_slug manifest project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    for path in "${required[@]}"; do
        [[ -e "${path}" ]] || _die "Missing required path: ${path}"
    done
    for route in "${ROUTE_ORDER[@]}"; do
        [[ -v ROUTES["${route}"] ]] || _die "Route missing metadata: ${route}"
        _route_meta "${route}" handler min max line preset
        declare -F "${handler}" >/dev/null || _die "Route handler missing: ${route} -> ${handler}"
        [[ "${min}" =~ ^[0-9]+$ && "${max}" =~ ^[0-9]+$ && -n "${line}" ]] || _die "Route metadata invalid: ${route}"
        ((min <= max)) || _die "Route arity invalid: ${route}"
    done
    _route_meta bridge:package handler min max line preset
    [[ "${handler}|${preset}|${min}|${max}" == '_cmd_package|rasm-bridge|1|1' ]] || _die "bridge package preset invalid"
    _route_meta bridge:quit handler min max line preset
    [[ "${handler}|${preset}|${min}|${max}" == '_bridge_client|quit|0|0' ]] || _die "bridge quit route invalid"
    _route_meta push handler min max line preset
    [[ "${handler}|${preset}|${min}|${max}" == '_push_package|-|2|2' ]] || _die "push preset invalid"
    _route_meta push-test handler min max line preset
    [[ "${handler}|${preset}|${min}|${max}" == '_push_package|https://test.yak.rhino3d.com|2|2' ]] || _die "push-test preset invalid"
    for manifest in "${YAK_ROOT}"/*/manifest.yml; do
        package_slug="${manifest%/manifest.yml}"
        package_slug="${package_slug##*/}"
        _package_meta "${package_slug}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir
        [[ -f "${manifest_dir}/manifest.yml" ]] || _die "Missing Yak manifest for ${package_slug}: ${manifest_dir}/manifest.yml"
        [[ -d "${project_dir}" ]] || _die "Missing project directory for ${package_slug}: ${project_dir}"
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
    dotnet run --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" -- "$@" || exit "$?"
}
_package_meta() {
    local -r package_slug="$1"
    shift
    local -n __project="$1" __manifest_dir="$2" __target_dir="$3" __target_framework="$4" __assembly_name="$5" __target_ext="$6" __project_dir="$7"
    local resolved_project
    _package_project "${package_slug}" resolved_project
    __project="${resolved_project}"
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
_package_project() {
    local -r package_slug="$1"
    local -n __project="$2"
    local -a projects=()
    local candidate payload evaluated
    mapfile -t projects < <(fd -H -e csproj . "${ROOT_DIR}")
    for candidate in "${projects[@]}"; do
        payload="$(dotnet msbuild "${candidate}" -p:Configuration="${CONFIGURATION}" -getProperty:YakPackageSlug -nologo)"
        evaluated="$(jq -er '.Properties.YakPackageSlug // empty' <<< "${payload}" 2>/dev/null || printf '%s' "${payload}")"
        [[ "${evaluated}" == "${package_slug}" ]] && { __project="${candidate}"; return 0; }
    done
    _die "Unknown package: ${package_slug}"
}
_clean_target_dir() {
    local -r target_dir="$1"
    local -r project_dir="$2"
    local -r target_framework="$3"
    [[ -n "${target_dir}" && "${target_dir}" == "${ROOT_DIR}/"* && "${target_dir}" == "${project_dir}/bin/${CONFIGURATION}/${target_framework}/" ]] || _die "Refusing to clean unexpected output directory: ${target_dir}"
    rm -rf -- "${target_dir:?}"
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
    local project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    _package_meta "${package_slug}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    _clean_target_dir "${target_dir}" "${project_dir}" "${target_framework}"
    _build "${version}"
    _package_meta "${package_slug}" project manifest_dir target_dir target_framework assembly_name target_ext project_dir
    local -r primary_artifact="${target_dir}/${assembly_name}${target_ext}"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -r stage_tmp="${PACKAGE_STAGE_ROOT}/${package_slug}/package.${EPOCHSECONDS}.$$"
    mkdir -p -- "${stage_tmp}"
    [[ -f "${manifest_dir}/manifest.yml" ]] || _die "Missing Yak manifest: ${manifest_dir}/manifest.yml"
    [[ -d "${target_dir}" ]] || _die "Missing build output: ${target_dir}"
    [[ -f "${primary_artifact}" ]] || _die "Missing primary package artifact: ${primary_artifact}"
    cp -p -- "${manifest_dir}/manifest.yml" "${stage_tmp}/manifest.yml"
    [[ -f "${manifest_dir}/icon.png" ]] && cp -p -- "${manifest_dir}/icon.png" "${stage_tmp}/icon.png"
    fd -H -e rhp -e dll -e json --exclude 'RhinoCommon.*' --exclude 'Grasshopper2.*' --exclude 'GrasshopperIO.*' . "${target_dir}" --max-depth 1 -X cp -p -- {} "${stage_tmp}/"
    rm -rf -- "${stage_dir}"
    mv -- "${stage_tmp}" "${stage_dir}"
    local -a staged_plugins=()
    mapfile -t staged_plugins < <(fd -H -e rhp . "${stage_dir}" --max-depth 1)
    ((${#staged_plugins[@]} == 1)) || _die "Expected one staged .rhp for ${package_slug}, found ${#staged_plugins[@]}"
    (cd -- "${stage_dir}" && "${YAK_PATH}" build --platform mac --version "${version}")
}
_package_path() {
    local -r package_slug="$1"
    local -r version="$2"
    local -n __package_path="$3"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "*-${version}-*-mac.yak" . "${stage_dir}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one Yak package for ${package_slug} ${version}, found ${#package_files[@]}"
    __package_path="${package_files[0]}"
}
_push_package() {
    local -r source="$1"
    local -r package_slug="$2"
    local -r version="$3"
    local -a source_args=()
    [[ -n "${source}" ]] && source_args=(--source "${source}")
    _cmd_package "${package_slug}" "${version}"
    local package_path
    _package_path "${package_slug}" "${version}" package_path
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
    _dispatch "${route}" "$@"
}

_main "$@"

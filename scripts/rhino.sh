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
readonly BRIDGE_CLIENT_PROJECT="${ROOT_DIR}/tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
readonly BRIDGE_PLUGIN_PROJECT="${ROOT_DIR}/tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"

declare -Ar COMMANDS=([build]=_build [bridge]=_cmd_bridge [package]=_cmd_package [push-test]=_cmd_push_test [push]=_cmd_push)
declare -Ar BRIDGE_COMMANDS=([build]=_bridge_build [doctor]=_bridge_doctor [check]=_bridge_check [launch]=_bridge_launch)
declare -Ar PACKAGE_PLUGIN_ROOTS=([radyab]="apps/grasshopper/Radyab")

_usage() {
    printf 'Usage:\n'
    printf '  scripts/rhino.sh build [version]\n'
    printf '  scripts/rhino.sh bridge build\n'
    printf '  scripts/rhino.sh bridge doctor\n'
    printf '  scripts/rhino.sh bridge check <job.json>\n'
    printf '  scripts/rhino.sh bridge launch\n'
    printf '  scripts/rhino.sh package <package> <version>\n'
    printf '  scripts/rhino.sh push-test <package> <version>\n'
    printf '  scripts/rhino.sh push <package> <version>\n'
}

_die() {
    printf 'rhino-package: %s\n' "$1" >&2
    exit 1
}

_build() {
    local -r version="${1:-}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore)
    [[ -n "${version}" ]] && build_args+=("/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${SOLUTION_PATH}"
    dotnet build "${SOLUTION_PATH}" "${build_args[@]}"
}

_bridge_usage() {
    printf 'Usage:\n'
    printf '  scripts/rhino.sh bridge build\n'
    printf '  scripts/rhino.sh bridge doctor\n'
    printf '  scripts/rhino.sh bridge check <job.json>\n'
    printf '  scripts/rhino.sh bridge launch\n'
}

_bridge_build() {
    dotnet restore "${SOLUTION_PATH}"
    dotnet build "${BRIDGE_PLUGIN_PROJECT}" --configuration "${CONFIGURATION}" --no-restore
    dotnet build "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" --no-restore
}

_bridge_client() {
    dotnet run --project "${BRIDGE_CLIENT_PROJECT}" --configuration "${CONFIGURATION}" -- "$@"
}

_bridge_doctor() {
    _bridge_client doctor
}

_bridge_check() {
    local -r job_path="${1:?job.json required}"
    _bridge_client check "${job_path}"
}

_bridge_launch() {
    _bridge_client launch
}

_cmd_bridge() {
    local -r command="${1:-}"
    [[ -n "${command}" && -v BRIDGE_COMMANDS["${command}"] ]] || { _bridge_usage >&2; exit 2; }
    shift
    "${BRIDGE_COMMANDS[${command}]}" "$@"
}

_package_plugin_roots() {
    local -r package_slug="$1"
    [[ -v PACKAGE_PLUGIN_ROOTS["${package_slug}"] ]] || _die "Unknown package: ${package_slug}"
    local -a plugin_roots=()
    IFS=' ' read -r -a plugin_roots <<< "${PACKAGE_PLUGIN_ROOTS[${package_slug}]}"
    printf '%s\n' "${plugin_roots[@]}"
}

_stage_project_output() {
    local -r stage_dir="$1"
    local -r project_dir="$2"
    local -r output_dir="${ROOT_DIR}/${project_dir}/bin/${CONFIGURATION}/${FRAMEWORK}"
    [[ -d "${output_dir}" ]] || _die "Missing build output: ${output_dir}"
    fd -H -e rhp -e dll -e json \
        --exclude 'RhinoCommon.*' \
        --exclude 'Grasshopper2.*' \
        --exclude 'GrasshopperIO.*' \
        . "${output_dir}" --max-depth 1 -x cp -p -- {} "${stage_dir}/"
}

_stage_package() {
    local -r package_slug="$1"
    local -r stage_dir="${PACKAGE_STAGE_ROOT}/${package_slug}/package"
    local -r yak_source_dir="${YAK_ROOT}/${package_slug}"
    rm -rf -- "${stage_dir}"
    mkdir -p -- "${stage_dir}"
    [[ -f "${yak_source_dir}/manifest.yml" ]] || _die "Missing Yak manifest: ${yak_source_dir}/manifest.yml"
    [[ -f "${yak_source_dir}/icon.png" ]] || _die "Missing Yak icon: ${yak_source_dir}/icon.png"
    cp -p -- "${yak_source_dir}/manifest.yml" "${stage_dir}/manifest.yml"
    cp -p -- "${yak_source_dir}/icon.png" "${stage_dir}/icon.png"
    local project_dir
    while IFS= read -r project_dir; do
        _stage_project_output "${stage_dir}" "${project_dir}"
    done < <(_package_plugin_roots "${package_slug}")
    printf '%s\n' "${stage_dir}"
}

_yak_build() {
    local -r stage_dir="$1"
    local -r version="$2"
    (
        cd -- "${stage_dir}"
        "${YAK_PATH}" build --platform mac --version "${version}"
    )
}

_cmd_package() {
    local -r package_slug="${1:?package required}"
    local -r version="${2:?version required}"
    [[ -x "${YAK_PATH}" ]] || _die "Yak not executable at ${YAK_PATH}"
    [[ -v PACKAGE_PLUGIN_ROOTS["${package_slug}"] ]] || _die "Unknown package: ${package_slug}"
    local -a plugin_roots=()
    IFS=' ' read -r -a plugin_roots <<< "${PACKAGE_PLUGIN_ROOTS[${package_slug}]}"
    readonly -a plugin_roots
    local project_dir
    for project_dir in "${plugin_roots[@]}"; do
        rm -rf -- "${ROOT_DIR:?}/${project_dir:?}/bin/${CONFIGURATION:?}/${FRAMEWORK:?}"
    done
    _build "${version}"
    local -r stage_dir="$(_stage_package "${package_slug}")"
    _yak_build "${stage_dir}" "${version}"
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
    local -r package_slug="${2:?package required}"
    local -r version="${3:?version required}"
    local -a source_args=()
    [[ -n "${source}" ]] && source_args=(--source "${source}")
    _cmd_package "${package_slug}" "${version}"
    local -r package_path="$(_package_path "${package_slug}" "${version}")"
    "${YAK_PATH}" push "${source_args[@]}" "${package_path}"
}

_cmd_push_test() {
    _push_package "https://test.yak.rhino3d.com" "${1:?package required}" "${2:?version required}"
}

_cmd_push() {
    _push_package "" "${1:?package required}" "${2:?version required}"
}

_main() {
    local -r command="${1:-}"
    [[ -n "${command}" && -v COMMANDS["${command}"] ]] || { _usage >&2; exit 2; }
    shift
    "${COMMANDS[${command}]}" "$@"
}

_main "$@"

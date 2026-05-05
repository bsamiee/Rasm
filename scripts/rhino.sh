#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit nullglob
IFS=$'\n\t'

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly STAGE_DIR="${ROOT_DIR}/.artifacts/rhino/package"
readonly YAK_PATH="${RHINO_YAK_PATH:-/Applications/RhinoWIP.app/Contents/Resources/bin/yak}"
readonly CONFIGURATION="${CONFIGURATION:-Release}"
readonly FRAMEWORK="${FRAMEWORK:-net10.0}"

declare -Ar COMMANDS=([build]=_build [package]=_cmd_package [push-test]=_cmd_push_test [push]=_cmd_push)

_usage() {
    printf 'Usage: scripts/rhino.sh <build|package|push-test|push> [version]\n'
}

_die() {
    printf 'rasm-rhino: %s\n' "$1" >&2
    exit 1
}

_build() {
    local -r version="${1:-}"
    local -a build_args=(--configuration "${CONFIGURATION}" --no-restore)
    [[ -n "${version}" ]] && build_args+=("/p:Version=${version}" "/p:InformationalVersion=${version}")
    dotnet restore "${SOLUTION_PATH}"
    dotnet build "${SOLUTION_PATH}" "${build_args[@]}"
}

_stage_project_output() {
    local -r project_dir="$1"
    local -r output_dir="${ROOT_DIR}/${project_dir}/bin/${CONFIGURATION}/${FRAMEWORK}"
    [[ -d "${output_dir}" ]] || _die "Missing build output: ${output_dir}"
    fd -H -e rhp -e dll -e json \
        --exclude 'RhinoCommon.*' \
        --exclude 'Grasshopper2.*' \
        --exclude 'GrasshopperIO.*' \
        . "${output_dir}" --max-depth 1 -x cp -p -- {} "${STAGE_DIR}/"
}

_stage_package() {
    rm -rf -- "${STAGE_DIR}"
    mkdir -p -- "${STAGE_DIR}"
    cp -p -- "${ROOT_DIR}/tools/yak/manifest.yml" "${STAGE_DIR}/manifest.yml"
    cp -p -- "${ROOT_DIR}/tools/yak/icon.png" "${STAGE_DIR}/icon.png"
    _stage_project_output "apps/rhino"
    _stage_project_output "apps/grasshopper"
}

_cmd_package() {
    local -r version="${1:?version required}"
    [[ -x "${YAK_PATH}" ]] || _die "Yak not executable at ${YAK_PATH}"
    _build "${version}"
    _stage_package
    (cd -- "${STAGE_DIR}" && "${YAK_PATH}" build --platform mac --version "${version}")
}

_package_path() {
    local -r version="$1"
    local -a package_files=()
    mapfile -t package_files < <(fd -H -g "*-${version}-*-mac.yak" . "${STAGE_DIR}" --max-depth 1)
    ((${#package_files[@]} == 1)) || _die "Expected one Yak package for ${version}, found ${#package_files[@]}"
    printf '%s\n' "${package_files[0]}"
}

_push_package() {
    local -r source="$1"
    local -r version="${2:?version required}"
    local -a source_args=()
    [[ -n "${source}" ]] && source_args=(--source "${source}")
    _cmd_package "${version}"
    local -r package_path="$(_package_path "${version}")"
    "${YAK_PATH}" push "${source_args[@]}" "${package_path}"
}

_cmd_push_test() {
    _push_package "https://test.yak.rhino3d.com" "${1:?version required}"
}

_cmd_push() {
    _push_package "" "${1:?version required}"
}

_main() {
    local -r command="${1:-}"
    [[ -n "${command}" && -v COMMANDS["${command}"] ]] || { _usage >&2; exit 2; }
    shift
    "${COMMANDS[${command}]}" "$@"
}

_main "$@"

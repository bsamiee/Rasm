#!/usr/bin/env bash
set -Eeuo pipefail
((BASH_VERSINFO[0] > 5 || (BASH_VERSINFO[0] == 5 && BASH_VERSINFO[1] >= 3))) || { printf 'mutate-cs: Bash 5.3+ is required\n' >&2; exit 1; }
shopt -s inherit_errexit nullglob array_expand_once
IFS=$'\n\t'

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR

readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly PROJECT_PATH="${MUTATE_PROJECT:-${ROOT_DIR}/libs/csharp/Rasm/Rasm.csproj}"
readonly PROJECT_NAME="${MUTATE_PROJECT_NAME:-${PROJECT_PATH##*/}}"
readonly TEST_PROJECT_PATH="${MUTATE_TEST_PROJECT:-${ROOT_DIR}/tests/csharp/libs/Rasm/Rasm.Tests.csproj}"
readonly CONFIGURATION="${CONFIGURATION:-Release}"
readonly TARGET_FRAMEWORK="${TARGET_FRAMEWORK:-net10.0}"
readonly MUTATION_LEVEL="${MUTATION_LEVEL:-Standard}"
_default_run_id() { printf '%(%Y-%m-%dT%H-%M-%S)T' -1; }
readonly MUTATE_SLICE="${MUTATE_SLICE:-${PROJECT_NAME%.csproj}}"
readonly MUTATE_RUN_ID="${MUTATE_RUN_ID:-$(_default_run_id)}"
readonly OUTPUT_PATH="${MUTATE_OUTPUT:-${ROOT_DIR}/.artifacts/mutation/${MUTATE_SLICE}/${MUTATE_RUN_ID}}"
readonly THRESHOLD_HIGH="${MUTATE_THRESHOLD_HIGH:-95}"
readonly THRESHOLD_LOW="${MUTATE_THRESHOLD_LOW:-90}"
readonly THRESHOLD_BREAK="${MUTATE_THRESHOLD_BREAK:-85}"
readonly MUTATE_TEST_RUNNER="${MUTATE_TEST_RUNNER:-vstest}"
readonly REPORTERS_RAW="${MUTATE_REPORTERS:-Html Json Progress}"
readonly -a DEFAULT_MUTATE_GLOBS=(
    "${ROOT_DIR}/libs/csharp/Rasm/**/*.cs"
    "!${ROOT_DIR}/libs/csharp/Rasm/**/bin/**/*.cs"
    "!${ROOT_DIR}/libs/csharp/Rasm/**/obj/**/*.cs"
)
declare -Ar ROUTES=(
    [--self-test]='_self_test|0|0|--self-test'
    [run]='_run|0|999|run [stryker args...]'
)
readonly -a ROUTE_ORDER=(--self-test run)

_die() {
    printf 'mutate-cs: %s\n' "$1" >&2
    exit 1
}

_usage() {
    printf 'Usage:\n'
    local route handler min max line
    for route in "${ROUTE_ORDER[@]}"; do
        IFS='|' read -r handler min max line <<< "${ROUTES[${route}]}"
        : "${handler}" "${min}" "${max}"
        printf '  scripts/mutate-cs.sh %s\n' "${line}"
    done
}

_require_file() {
    local -r path="$1"
    [[ -f "${path}" ]] || _die "Missing required file: ${path}"
}

_self_test() {
    local route handler min max line
    local -a thresholds=("${THRESHOLD_HIGH}" "${THRESHOLD_LOW}" "${THRESHOLD_BREAK}")
    local threshold
    for route in "${ROUTE_ORDER[@]}"; do
        [[ -v ROUTES["${route}"] ]] || _die "Route missing metadata: ${route}"
        IFS='|' read -r handler min max line <<< "${ROUTES[${route}]}"
        declare -F "${handler}" >/dev/null || _die "Route handler missing: ${route} -> ${handler}"
        [[ "${min}" =~ ^[0-9]+$ && "${max}" =~ ^[0-9]+$ && -n "${line}" ]] || _die "Route metadata invalid: ${route}"
        ((min <= max)) || _die "Route arity invalid: ${route}"
    done
    for threshold in "${thresholds[@]}"; do
        [[ "${threshold}" =~ ^[0-9]+$ ]] || _die "Threshold must be an integer: ${threshold}"
        ((threshold >= 0 && threshold <= 100)) || _die "Threshold must be in [0,100]: ${threshold}"
    done
    [[ "${MUTATE_TEST_RUNNER}" =~ ^(vstest|mtp)$ ]] || _die "Unsupported MUTATE_TEST_RUNNER: ${MUTATE_TEST_RUNNER}"
    _require_file "${SOLUTION_PATH}"
    _require_file "${PROJECT_PATH}"
    _require_file "${TEST_PROJECT_PATH}"
    _require_file "${ROOT_DIR}/.config/dotnet-tools.json"
    printf 'mutate-cs: self-test ok\n'
}

_tool_restore() {
    dotnet tool restore --tool-manifest "${ROOT_DIR}/.config/dotnet-tools.json"
}

_run() {
    local -a args=(
        --solution "${SOLUTION_PATH}"
        --project "${PROJECT_NAME}"
        --test-project "${TEST_PROJECT_PATH}"
        --configuration "${CONFIGURATION}"
        --target-framework "${TARGET_FRAMEWORK}"
        --test-runner "${MUTATE_TEST_RUNNER}"
        --mutation-level "${MUTATION_LEVEL}"
        --output "${OUTPUT_PATH}"
        --threshold-high "${THRESHOLD_HIGH}"
        --threshold-low "${THRESHOLD_LOW}"
        --break-at "${THRESHOLD_BREAK}"
    )
    local -a reporters=()
    IFS=' ' read -r -a reporters <<< "${REPORTERS_RAW}"
    local reporter glob
    for reporter in "${reporters[@]}"; do
        args+=(--reporter "${reporter}")
    done
    for glob in "${DEFAULT_MUTATE_GLOBS[@]}"; do
        args+=(--mutate "${glob}")
    done
    _tool_restore
    dotnet stryker "${args[@]}" "$@"
}

_dispatch() {
    local route="${1:-run}"
    [[ "${route}" == --help || "${route}" == -h ]] && { _usage; return 0; }
    [[ -v ROUTES["${route}"] ]] || route=run
    local handler min max line
    IFS='|' read -r handler min max line <<< "${ROUTES[${route}]}"
    [[ "${handler}" == _run && "${1:-}" != run ]] || shift
    (($# >= min && $# <= max)) || { _usage >&2; exit 2; }
    "${handler}" "$@"
}

_dispatch "$@"

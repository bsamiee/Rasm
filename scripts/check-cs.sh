#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit extglob nullglob
IFS=$'\n\t'

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx"
readonly FORMAT_SEVERITY="${FORMAT_SEVERITY:-warn}"
readonly CONFIGURATIONS_RAW="${CONFIGURATIONS:-Debug Release}"
readonly UNUSED_CODE_DIAGNOSTICS="IDE0051 IDE0052 CS0169 CS0649"
readonly -a PROJECT_EXCLUDE_ARGS=(
    --exclude .artifacts --exclude .cache --exclude .git --exclude .nx --exclude bin
    --exclude coverage --exclude node_modules --exclude obj --exclude test-results --exclude tmp
)

_die() {
    local -r message="$1"
    printf 'check-cs: %s\n' "${message//$'\n'/$'\n'check-cs: }" >&2
    exit 1
}

_err() {
    local -r command="$1" line="$2"
    printf 'check-cs: failed at line %s: %s\n' "${line}" "${command}" >&2
}

trap '_err "${BASH_COMMAND}" "${LINENO}"' ERR

_main() {
    (($# <= 1)) || _die "Unexpected arguments: $*"
    local -r mode="${1:-check}"
    [[ "${mode}" == "check" || "${mode}" == "test" || "${mode}" == "--self-test" ]] || _die "Unexpected argument: ${mode}"
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd rg; do
        command -v "${command}" >/dev/null || _die "Missing required command: ${command}"
    done
    local workspace_projects solution_projects missing
    workspace_projects="$(
        cd -- "${ROOT_DIR}" \
            && fd -H -e csproj . --strip-cwd-prefix=always "${PROJECT_EXCLUDE_ARGS[@]}" \
            | LC_ALL=C sort
    )"
    readonly workspace_projects
    [[ -n "${workspace_projects}" ]] || _die "No C# projects discovered."
    solution_projects="$(cd -- "${ROOT_DIR}" && dotnet sln "${SOLUTION_PATH}" list | rg --no-line-number '\.csproj$' | LC_ALL=C sort)"
    readonly solution_projects
    missing="$(
        comm -23 \
            <(printf '%s\n' "${workspace_projects}") \
            <(printf '%s\n' "${solution_projects}")
    )"
    readonly missing
    [[ -z "${missing}" ]] || _die "C# projects missing from Workspace.slnx:
- ${missing//$'\n'/$'\n'- }"
    [[ "${mode}" == "--self-test" ]] && { printf 'check-cs: self-test passed\n'; return 0; }
    dotnet restore "${SOLUTION_PATH}" --locked-mode
    local -a configurations=()
    IFS=' ' read -r -a configurations <<< "${CONFIGURATIONS_RAW}"
    readonly -a configurations
    local configuration
    for configuration in "${configurations[@]}"; do
        dotnet build "${SOLUTION_PATH}" --configuration "${configuration}" --no-restore
    done
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity "${FORMAT_SEVERITY}" --no-restore
    dotnet format "${SOLUTION_PATH}" --verify-no-changes --severity info --diagnostics "${UNUSED_CODE_DIAGNOSTICS}" --no-restore
    [[ "${mode}" == "test" ]] || return 0
    local -a test_projects=()
    test_projects=(
        tests/csharp/Rasm.Tests/Rasm.Tests.csproj
        tools/cs-analyzer/tests/CsAnalyzer.Tests.csproj
    )
    readonly -a test_projects
    local test_project
    for test_project in "${test_projects[@]}"; do
        dotnet test "${ROOT_DIR}/${test_project}" --configuration Release --no-build
    done
}

_main "$@"

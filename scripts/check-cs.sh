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
readonly RHINO_WIP_APP_PATH="/Applications/RhinoWIP.app"
readonly RHINO_WIP_FRAMEWORKS_PATH="${RHINO_WIP_APP_PATH}/Contents/Frameworks"
readonly RHINO_WIP_LIBRARY_PATH="${RHINO_WIP_FRAMEWORKS_PATH}/RhinoLibrary.framework/Versions/A/RhinoLibrary"
readonly RHINO_COMMON_PATH="${RHINO_WIP_FRAMEWORKS_PATH}/RhCore.framework/Versions/A/Resources/RhinoCommon.dll"
readonly RHINO_TEST_CONFIG_OUTPUT="${ROOT_DIR}/tests/rhino/bin/Release/net10.0/Rhino.Testing.Configs.xml"
readonly RHINO_TEST_DEPS_OUTPUT="${ROOT_DIR}/tests/rhino/bin/Release/net10.0/Rhino.Tests.deps.json"
readonly RHINO_TESTING_NET10_ASSET="lib/net10.0/Rhino.Testing.dll"
readonly RHINO_DOTNET="${RHINO_DOTNET:-/usr/local/share/dotnet/dotnet}"
readonly RHINO_RUNTIME_TEST_PROJECT="tests/rhino/Rhino.Tests.csproj"
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
    [[ "${mode}" == "check" || "${mode}" == "--self-test" ]] || _die "Unexpected argument: ${mode}"
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
    local command
    for command in dotnet fd rg; do
        command -v "${command}" >/dev/null || _die "Missing required command: ${command}"
    done
    local workspace_projects solution_projects missing
    workspace_projects="$(
        cd -- "${ROOT_DIR}" \
            && fd -H -e csproj . --strip-cwd-prefix=always "${PROJECT_EXCLUDE_ARGS[@]}" \
            | rg --no-line-number --fixed-strings --invert-match --line-regexp "${RHINO_RUNTIME_TEST_PROJECT}" \
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
    local -a test_projects=()
    test_projects=(
        tests/csharp/analysis/Analysis.Tests.csproj
        tests/csharp/core/Core.Tests.csproj
        tools/cs-analyzer/tests/CsAnalyzer.Tests.csproj
    )
    readonly -a test_projects
    local test_project
    for test_project in "${test_projects[@]}"; do
        dotnet test "${ROOT_DIR}/${test_project}" --configuration Release --no-build
    done
    case "${RHINO_RUNTIME_TESTS:-0}" in
        1|require)
            [[ -d "${RHINO_WIP_APP_PATH}" ]] || _die "Missing RhinoWIP app: ${RHINO_WIP_APP_PATH}"
            [[ -f "${RHINO_WIP_LIBRARY_PATH}" ]] || _die "Missing Rhino runtime library: ${RHINO_WIP_LIBRARY_PATH}"
            [[ -f "${RHINO_COMMON_PATH}" ]] || _die "Missing RhinoCommon runtime assembly: ${RHINO_COMMON_PATH}"
            [[ -f "${RHINO_TEST_CONFIG_OUTPUT}" ]] || _die "Missing Rhino.Testing config in output: ${RHINO_TEST_CONFIG_OUTPUT}"
            [[ -f "${RHINO_TEST_DEPS_OUTPUT}" ]] || _die "Missing Rhino test dependency manifest: ${RHINO_TEST_DEPS_OUTPUT}"
            local rhino_testing_asset_state
            rhino_testing_asset_state="$(
                rg --fixed-strings --quiet "${RHINO_TESTING_NET10_ASSET}" "${RHINO_TEST_DEPS_OUTPUT}" \
                    && printf 'supported' \
                    || printf 'unsupported'
            )"
            readonly rhino_testing_asset_state
            case "${RHINO_RUNTIME_TESTS_FORCE:-0}:${rhino_testing_asset_state}:${RHINO_RUNTIME_TESTS:-0}" in
                1:*:*|*:supported:*)
                    [[ -x "${RHINO_DOTNET}" ]] || _die "Missing Rhino test .NET SDK: ${RHINO_DOTNET}. Set RHINO_DOTNET to an official Microsoft .NET SDK dotnet binary."
                    "${RHINO_DOTNET}" test "${ROOT_DIR}/tests/rhino/Rhino.Tests.csproj" --configuration Release --no-build
                    ;;
                *:unsupported:require)
                    _die "Rhino runtime tests require a Rhino.Testing net10.0 asset. Current dependency graph resolves below net10.0 and crashes the RhinoWIP testhost on macOS."
                    ;;
                *)
                    printf 'check-cs: skipping Rhino runtime tests; Rhino.Testing resolves below net10.0 and crashes the RhinoWIP testhost on macOS. Set RHINO_RUNTIME_TESTS_FORCE=1 to run diagnostics anyway.\n'
                    ;;
            esac
            ;;
        *)
            printf 'check-cs: skipping Rhino runtime tests; set RHINO_RUNTIME_TESTS=1 to run them\n'
            ;;
    esac
}

_main "$@"

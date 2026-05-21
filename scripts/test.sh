#!/usr/bin/env bash
set -Eeuo pipefail
shopt -s inherit_errexit extglob nullglob
IFS=$'\n\t'
(( BASH_VERSINFO[0] * 100 + BASH_VERSINFO[1] >= 503 )) || { printf 'test: Bash 5.3+ required\n' >&2; exit 1; }
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly SCRIPT_DIR ROOT_DIR
readonly SOLUTION_PATH="${ROOT_DIR}/Workspace.slnx" CONFIGURATION="${CONFIGURATION:-Release}"
readonly LOCK_ROOT="${ROOT_DIR}/.artifacts/locks" COVERAGE_ROOT="${ROOT_DIR}/.artifacts/coverage" COVERAGE_SETTINGS="${ROOT_DIR}/.coverage.runsettings"
ACTIVE_LOCK=""
_die() { printf 'test: %s\n' "$1" >&2; exit "${2:-1}"; }
_err() { printf 'test: failed with %s at line %s: %s\n' "$1" "$3" "$2" >&2; }
trap '_err "$?" "${BASH_COMMAND}" "${LINENO}"' ERR
_release_lock() { local -r lock="${ACTIVE_LOCK:-}"; ACTIVE_LOCK=""; [[ -n "${lock}" ]] && rmdir -- "${lock}" 2>/dev/null || true; }
trap '_release_lock; exit $?' EXIT
_with_lock() {
    local -r lock_dir="${LOCK_ROOT}/${1}.lock" deadline=$((BASH_MONOSECONDS + 120))
    shift
    mkdir -p -- "${LOCK_ROOT}"
    until mkdir -- "${lock_dir}" 2>/dev/null; do
        ((BASH_MONOSECONDS < deadline)) || _die "Lock timeout: ${lock_dir}"
        sleep 0.2
    done
    ACTIVE_LOCK="${lock_dir}"
    "$@"
}
_run() {
    local -r coverage="$1" filter="$2"
    local -a args=(--configuration "${CONFIGURATION}")
    case "${filter}" in '') ;; *) args+=(--filter "${filter}") ;; esac
    case "${coverage}" in yes) rm -rf -- "${COVERAGE_ROOT}"; mkdir -p -- "${COVERAGE_ROOT}"; args+=(--settings "${COVERAGE_SETTINGS}" --collect "XPlat Code Coverage" --results-directory "${COVERAGE_ROOT}") ;; esac
    dotnet test "${SOLUTION_PATH}" "${args[@]}"
    case "${coverage}" in yes) _summarize_coverage ;; esac
}
_summarize_coverage() {
    local -a cobertura_files
    mapfile -t cobertura_files < <(find "${COVERAGE_ROOT}" -name 'coverage.cobertura.xml' -type f 2>/dev/null | LC_ALL=C sort)
    ((${#cobertura_files[@]} > 0)) || { printf 'test: coverage: no cobertura.xml under %s\n' "${COVERAGE_ROOT}" >&2; return 1; }
    python3 - "${ROOT_DIR}" "${cobertura_files[@]}" <<'PY'
import sys, xml.etree.ElementTree as ET
root_dir = sys.argv[1].rstrip('/') + '/'
files = sys.argv[2:]
total_lines, hit_lines = {}, {}  # filename -> set of source line numbers
for path in files:
    for cls in ET.parse(path).getroot().iter('class'):
        fn = cls.get('filename', '?')
        if fn.startswith(root_dir): fn = fn[len(root_dir):]
        if 'obj/' in fn or fn.endswith('.g.cs'): continue
        total_lines.setdefault(fn, set())
        hit_lines.setdefault(fn, set())
        for ln in cls.iter('line'):
            n = int(ln.get('number', 0))
            total_lines[fn].add(n)
            if int(ln.get('hits', 0)) > 0:
                hit_lines[fn].add(n)
def pct(r): return f"{r*100:5.1f}%"
per_file = sorted(((fn, len(hit_lines[fn]), len(total_lines[fn])) for fn in total_lines), key=lambda r: (r[1]/r[2] if r[2] else 1.0, r[0]))
grand_total = sum(t for _, _, t in per_file)
grand_hit = sum(h for _, h, _ in per_file)
print(f"\n[CSHARP_COVERAGE]")
print(f"  files       {len(per_file)}")
print(f"  lines hit   {grand_hit}/{grand_total} ({pct(grand_hit/grand_total if grand_total else 0)})")
print(f"  cobertura   {len(files)} reports merged from .artifacts/coverage/")
print(f"\n[PER_FILE — worst first]")
for fn, h, t in per_file:
    r = h / t if t else 0.0
    print(f"  {pct(r)}  {h:>5}/{t:<5}  {fn}")
PY
}
_self_test() {
    command -v shellcheck >/dev/null || _die "Missing required command: shellcheck"
    bash -n "${BASH_SOURCE[0]}"
    shellcheck "${BASH_SOURCE[0]}"
    [[ -f "${SOLUTION_PATH}" ]] || _die "Missing solution: ${SOLUTION_PATH}"
}
_main() {
    [[ "${1:-}" == "--self-test" ]] && { _self_test; printf 'test: self-test passed\n'; return 0; }
    command -v dotnet >/dev/null || _die "Missing required command: dotnet"
    local coverage="" filter=""
    case "${1:-}" in
        --coverage) (($# <= 2)) || _die "Unexpected arguments: $*" 2; coverage="yes"; filter="${2:-}" ;;
        *)          (($# <= 1)) || _die "Unexpected arguments: $*" 2; filter="${1:-}" ;;
    esac
    case "${coverage}" in yes) command -v python3 >/dev/null || _die "python3 is required for the coverage summary" ;; esac
    _with_lock check-cs _run "${coverage}" "${filter}"
}
_main "$@"

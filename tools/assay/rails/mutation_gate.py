"""Gate a completed mutmut cache with one stdout score line and stderr survivor triage.

The score uses mutmut's own persisted status taxonomy. Only killed and survived mutants enter the
denominator; no-tests, timeout, and suspicious mutants stay on the structured stderr triage channel.
"""

import ast
from collections import Counter
import contextlib
from functools import reduce
import io
from itertools import groupby
import json
from operator import itemgetter
from pathlib import Path
import sys
from typing import Final, Literal, TYPE_CHECKING

from expression import Error, Ok
import msgspec
from mutmut.__main__ import get_diff_for_mutant, orig_function_and_class_names_from_key, status_by_exit_code, walk_mutatable_files
from mutmut.mutation.data import SourceFileMutationData

from tools.assay.core.model import MutationLane, TestRun


if TYPE_CHECKING:
    from collections.abc import Iterable

    from expression import Result


# --- [TYPES] ----------------------------------------------------------------------------

# mutmut SourceFileMutationData.load() can fail three ways on a stale cache: corrupt JSON, version-skewed schema
# (a missing required key or unexpected extra keys), or an I/O fault; the cause selects the regenerate guidance.
type _LoadCause = Literal["corrupt", "skewed", "io"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOOR: float = 0.80
_TOP_SURVIVORS: int = 10

# mutation-testing-report-schema mutant statuses keyed by mutmut's persisted status taxonomy.
_SCHEMA_STATUS: Final[dict[str, str]] = {
    "killed": "Killed",
    "survived": "Survived",
    "no tests": "NoCoverage",
    "timeout": "Timeout",
    "suspicious": "RuntimeError",
    "segfault": "RuntimeError",
    "caught by type check": "CompileError",
    "skipped": "Ignored",
    "not checked": "Pending",
    "check was interrupted by user": "Pending",
}

_SCHEMA_ARTIFACT: Final = Path(".artifacts/python/mutmut/mutation-report.json")

# --- [MODELS] ---------------------------------------------------------------------------


class _LoadFault(msgspec.Struct, frozen=True):
    """An unloadable mutmut meta file with the cause that excluded it from scoring."""

    path: str
    cause: _LoadCause
    detail: str


class FileCluster(msgspec.Struct, frozen=True):
    """Per-file survivor and unscored mutation buckets for stderr triage."""

    path: str
    survivors: tuple[str, ...] = ()
    no_tests: tuple[str, ...] = ()
    timeouts: tuple[str, ...] = ()
    suspicious: tuple[str, ...] = ()
    duration_s: float = 0.0


class SurvivorReport(msgspec.Struct, frozen=True):
    """Structured stderr companion to the stdout ``TestRun`` score line."""

    clusters: tuple[FileCluster, ...] = ()
    diffs: tuple[tuple[str, str], ...] = ()
    unloadable: tuple[_LoadFault, ...] = ()


# --- [SCHEMA_REPORT]
# mutation-testing-report-schema wire models: files[].mutants[]{id, mutatorName, status, location}.


class SchemaPosition(msgspec.Struct, frozen=True):
    """1-based schema position."""

    line: int
    column: int = 1


class SchemaLocation(msgspec.Struct, frozen=True):
    """Schema mutant location; mutmut's trampoline granularity pins the owning function span."""

    start: SchemaPosition
    end: SchemaPosition


class SchemaMutant(msgspec.Struct, frozen=True, rename="camel"):
    """One schema mutant row keyed by mutmut's mutant name."""

    id: str
    mutator_name: str
    location: SchemaLocation
    status: str


class SchemaFile(msgspec.Struct, frozen=True):
    """One schema file entry carrying source text and its mutant rows."""

    language: str
    source: str
    mutants: tuple[SchemaMutant, ...]


class SchemaThresholds(msgspec.Struct, frozen=True):
    """Schema score thresholds; ``low`` mirrors the gate floor."""

    high: int
    low: int


class SchemaReport(msgspec.Struct, frozen=True, rename="camel"):
    """mutation-testing-report-schema document rendered by mutation-testing-elements."""

    schema_version: str
    thresholds: SchemaThresholds
    files: dict[str, SchemaFile]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _loaded(path: Path) -> Result[SourceFileMutationData, _LoadFault]:
    # Boundary over mutmut's load(): corrupt JSON, a version-skewed schema (missing key / unexpected keys), or an I/O
    # fault converts to a typed rail so one stale meta file is excluded from scoring instead of crashing the gate.
    data = SourceFileMutationData(path=path)
    try:
        data.load()
    except json.JSONDecodeError as exc:
        return Error(_LoadFault(path=str(path), cause="corrupt", detail=str(exc)[:256]))
    except (KeyError, AssertionError) as exc:
        return Error(_LoadFault(path=str(path), cause="skewed", detail=str(exc)[:256]))
    except OSError as exc:
        return Error(_LoadFault(path=str(path), cause="io", detail=str(exc)[:256]))
    return Ok(data)


def _scan() -> tuple[tuple[SourceFileMutationData, ...], tuple[_LoadFault, ...]]:
    """Load persisted mutmut metadata once, partitioning loadable records from unloadable meta files.

    Returns:
        Loaded source-file mutation records and the typed faults for any stale/corrupt meta file.
    """
    railed = tuple(map(_loaded, walk_mutatable_files()))
    loaded = tuple(r.ok for r in railed if r.is_ok())
    faulted = tuple(r.error for r in railed if r.is_error())
    return loaded, faulted


def _cluster(data: SourceFileMutationData) -> FileCluster:
    """Project one loaded file into status buckets and total mutation duration.

    Returns:
        Cluster carrying survivor and unscored mutant names.
    """
    keyed = sorted(((status_by_exit_code[code], key) for key, code in data.exit_code_by_key.items()), key=itemgetter(0))
    by_status: dict[str, tuple[str, ...]] = {status: tuple(key for _, key in group) for status, group in groupby(keyed, key=itemgetter(0))}
    return FileCluster(
        path=str(data.path),
        survivors=by_status.get("survived", ()),
        no_tests=by_status.get("no tests", ()),
        timeouts=by_status.get("timeout", ()),
        suspicious=by_status.get("suspicious", ()),
        duration_s=sum(data.durations_by_key.values()),
    )


def _diff(name: str) -> str:
    """Return a best-effort survivor diff without contaminating stdout.

    Returns:
        Unified diff text, or an empty string when mutmut cannot re-read the mutant.
    """
    # Redirected stdout preserves the one-line TestRun wire contract.
    with contextlib.suppress(Exception), contextlib.redirect_stdout(io.StringIO()):  # ruff:ignore[banned-api]  # best-effort diff boundary adapter
        return get_diff_for_mutant(name)
    return ""


def _location(node: ast.FunctionDef | ast.AsyncFunctionDef) -> SchemaLocation:
    return SchemaLocation(
        start=SchemaPosition(line=node.lineno, column=node.col_offset + 1),
        end=SchemaPosition(line=node.end_lineno or node.lineno, column=(node.end_col_offset or 0) + 1),
    )


def _spans(source: str) -> dict[tuple[str | None, str], SchemaLocation]:
    """Index function spans by ``(class_name, function_name)`` for mutant location resolution.

    Returns:
        Span map from the parsed module; empty on unparsable source.
    """
    try:
        walk = tuple(ast.walk(ast.parse(source)))
    except SyntaxError:
        return {}
    spans: dict[tuple[str | None, str], SchemaLocation] = {
        (node.name, child.name): _location(child)
        for node in walk
        if isinstance(node, ast.ClassDef)
        for child in node.body
        if isinstance(child, (ast.FunctionDef, ast.AsyncFunctionDef))
    }
    for node in walk:
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
            spans.setdefault((None, node.name), _location(node))
    return spans


def _owner_of(key: str) -> tuple[str | None, str]:
    """Resolve a mutant key to its ``(class_name, function_name)`` span index.

    Returns:
        Owner pair, or ``(None, "")`` for keys outside mutmut's mangling grammar.
    """
    try:
        function_name, class_name = orig_function_and_class_names_from_key(key)
    except AssertionError, ValueError:
        return None, ""
    # Method keys keep the trampoline mangle prefix; module-level keys come back demangled.
    return class_name, function_name.removeprefix("x_")


def schema_report(scanned: Iterable[SourceFileMutationData], *, floor: float = _FLOOR) -> SchemaReport:
    """Project persisted mutmut results into a mutation-testing-report-schema document.

    Mutant ids are mutmut's mutant names; locations resolve to the owning function span through the
    original source (mutmut's trampoline granularity carries no finer position). Unresolvable spans
    fall back to line 1 so the document stays schema-valid.

    Returns:
        Schema report ready for JSON emission under the mutmut artifact home.
    """
    files: dict[str, SchemaFile] = {}
    fallback = SchemaLocation(start=SchemaPosition(line=1), end=SchemaPosition(line=1))
    for data in scanned:
        source = Path(data.path).read_text(encoding="utf-8") if Path(data.path).exists() else ""
        spans = _spans(source)
        mutants = tuple(
            SchemaMutant(
                id=key,
                mutator_name="mutmut",
                location=spans.get(_owner_of(key), fallback),
                status=_SCHEMA_STATUS.get(status_by_exit_code[code], "Pending"),
            )
            for key, code in sorted(data.exit_code_by_key.items())
        )
        files[str(data.path)] = SchemaFile(language="python", source=source, mutants=mutants)
    return SchemaReport(schema_version="2", thresholds=SchemaThresholds(high=90, low=round(floor * 100)), files=files)


def _emit_schema(report: SchemaReport, artifact: Path = _SCHEMA_ARTIFACT) -> None:
    """Write the schema JSON under the mutmut artifact home, creating parents."""
    artifact.parent.mkdir(parents=True, exist_ok=True)
    artifact.write_bytes(msgspec.json.encode(report))


def _tally(scanned: Iterable[SourceFileMutationData] | None = None) -> Counter[str]:
    """Fold persisted exit codes into mutmut's status taxonomy.

    Args:
        scanned: Pre-loaded file data to fold; ``None`` loads the cache itself (the standalone contract).

    Returns:
        Status-name counts; killed and survived counts drive the score denominator.
    """
    return reduce(
        lambda acc, data: acc + Counter(status_by_exit_code[code] for code in data.exit_code_by_key.values()),
        scanned if scanned is not None else _scan()[0],
        Counter[str](),
    )


def _survivors(scanned: Iterable[SourceFileMutationData], faulted: tuple[_LoadFault, ...] = ()) -> SurvivorReport:
    """Project scanned metadata into per-file triage clusters, top survivor diffs, and unloadable meta faults.

    Returns:
        Structured survivor report for stderr.
    """
    clusters = tuple(
        cluster for cluster in map(_cluster, scanned) if (cluster.survivors or cluster.no_tests or cluster.timeouts or cluster.suspicious)
    )
    top = tuple(name for cluster in clusters for name in cluster.survivors)[:_TOP_SURVIVORS]
    return SurvivorReport(clusters=clusters, diffs=tuple((name, _diff(name)) for name in top), unloadable=faulted)


def gate(floor: float = _FLOOR) -> int:
    """Emit the persisted mutation score and return the process exit code.

    Stale, corrupt, or version-skewed meta files are excluded from the score and surfaced on the stderr triage
    channel; the gate fails when the scored population is empty or below the floor, never on an unloadable cache.

    Args:
        floor: Minimum acceptable mutation score ``killed / (killed + survived)``.

    Returns:
        ``0`` when the scored population is non-empty and meets ``floor``; ``1`` otherwise.
    """
    scanned, faulted = _scan()
    _emit_schema(schema_report(scanned, floor=floor))
    tally = _tally(scanned)
    killed, survived = tally["killed"], tally["survived"]
    scored = killed + survived
    score = killed / scored if scored else 0.0
    verdict = bool(scored) and score >= floor
    detail = " ".join(f"{status.replace(' ', '_')}={count}" for status, count in sorted(tally.items()))
    sys.stderr.write(
        f"[{'PASS' if verdict else 'FAIL'}] mutation-score={score:.3f} floor={floor:.3f} {detail}{f' unloadable={len(faulted)}' if faulted else ''}\n"
    )
    sys.stderr.write(msgspec.json.encode(_survivors(scanned, faulted)).decode() + "\n")
    run = TestRun(mutation=MutationLane.FULL, killed=killed, survived=survived, selected=tally.total())
    sys.stdout.write(msgspec.json.encode(run).decode() + "\n")
    return 0 if verdict else 1


# --- [EXPORTS] --------------------------------------------------------------------------

# The python -m entrypoint plus the pure schema converter; stderr projections stay leaf-internal.
__all__ = ["gate", "schema_report"]

# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    raise SystemExit(gate())

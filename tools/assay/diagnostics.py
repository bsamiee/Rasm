"""Convert foreign tool output into wire evidence and fold receipts into rail reports.

One converter owner block per diagnostic family, keyed by the ``Parser`` value the engine stamps onto each
receipt; SARIF documents fold per build target, and the structured-search decoders own ast-grep,
tree-sitter, and ripgrep payloads.
"""

from collections import Counter
from collections.abc import Callable  # noqa: TC003  # converter-table rows bind Callable at module runtime
from dataclasses import dataclass
from functools import cache
from pathlib import Path
import re
import shlex
from typing import Final, TYPE_CHECKING
from urllib.parse import unquote, urlparse

from expression.extra.result import catch
import msgspec
import structlog
from tree_sitter import Language as TSLanguage, Query as TSQuery, QueryError

from tools.assay.core.model import (
    AnyDetail,  # noqa: TC001  # beartype resolves the fold detail annotation at runtime
    ArtifactKind,
    Claim,
    Completed,  # noqa: TC001  # beartype resolves receipt annotations at runtime
    Counts,
    ExecReceipt,
    field_cap,
    Match,
    Parser,
    RailStatus,
    Report,
    SarifStatus,
)


if TYPE_CHECKING:
    from tree_sitter import Node


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFECT_TAIL: int = 4096
_MATCH_TEXT_CAP: int = field_cap(Match, "text", default=400)
# SARIF 2.1 result levels -> assay severities; analyzer notes (e.g. CSP0903) surface as info-grade evidence.
_SARIF_SEVERITY: dict[str, str] = {"error": "error", "warning": "warning", "note": "info", "none": "info"}
_DIAGNOSTIC_SEVERITY_RANK: dict[str, int] = {"error": 0, "warning": 1, "info": 2, "failed": 3}
_PROCESS_BACKED_OK_CLAIMS: tuple[Claim, ...] = (Claim.STATIC, Claim.TEST, Claim.PACKAGE, Claim.BRIDGE, Claim.PROVISION)
_ANSI_ESCAPE = re.compile(r"\x1b\[[0-9;]*m")
_HEADER_DIAGNOSTIC = re.compile(r"^(?P<severity>error|warning|warn|info|note)(?:\[(?P<rule>[^\]]+)])?:\s*(?P<message>.+)$", re.IGNORECASE)
_ARROW_LOCATION = re.compile(r"^\s*-->\s*(?P<path>.+?):(?P<line>\d+):(?P<column>\d+)$")
_MYPY_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?):(?P<line>\d+)(?::(?P<column>\d+))?:\s*(?P<severity>error|warning|note):\s*"
    r"(?P<message>.*?)(?:\s+\[(?P<rule>[a-z0-9_.-]+)])?$",
    re.IGNORECASE,
)
_TSC_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?)(?:\((?P<line1>\d+),(?P<column1>\d+)\):?|:(?P<line2>\d+):(?P<column2>\d+)\s+-)\s*"
    r"(?P<severity>error|warning)\s+(?P<rule>TS\d+):\s*(?P<message>.+)$",
    re.IGNORECASE,
)
_BIOME_TEXT_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?):(?P<line>\d+):(?P<column>\d+)\s+(?P<rule>(?:lint|assist|parse|format)[/\w.-]*)\s*(?P<message>.*)$", re.IGNORECASE
)
_FORMAT_DIAGNOSTIC = re.compile(r"^(?:Would reformat:\s*(?P<path>.+)|(?P<path2>.+?)\s+would be reformatted)$", re.IGNORECASE)
_CS_DIAGNOSTIC = re.compile(
    r"^(?P<path>.*?\.(?:cs|csproj|props|targets|slnx))\((?P<line>\d+),(?P<column>\d+)\):\s*"
    r"(?P<severity>error|warning|info)\s+(?P<rule>[A-Z][A-Z0-9]*\d+):\s*(?P<message>.*?)(?:\s+\[(?P<project>[^\]]+)\])?$",
    re.IGNORECASE,
)
# Forward-slash-normalized, lowercased path fragments and suffix that mark generated/build-output rows as evidence-only.
_GENERATED_MARKERS: Final[tuple[str, ...]] = ("/obj/", "/.artifacts/assay/")
_GENERATED_SUFFIX: Final[str] = ".g.cs"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [SARIF]
# SARIF carries tool/schema fields outside the assay wire contract; unknown fields must pass.
# Eight structs model distinct SARIF 2.1 schema levels (log -> run -> result -> location -> region); they are not parallel
# shapes for one concept, so they stay separate owners under one navigation label.


class _SarifMessage(msgspec.Struct, frozen=True, gc=False):
    text: str = ""


class _SarifRegion(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    start_line: int = 0
    start_column: int = 0


class _SarifArtifactLocation(msgspec.Struct, frozen=True, gc=False):
    uri: str = ""


class _SarifPhysicalLocation(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    artifact_location: _SarifArtifactLocation = msgspec.field(default_factory=_SarifArtifactLocation)
    region: _SarifRegion = msgspec.field(default_factory=_SarifRegion)


class _SarifLocation(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    physical_location: _SarifPhysicalLocation = msgspec.field(default_factory=_SarifPhysicalLocation)


class _SarifSuppression(msgspec.Struct, frozen=True, gc=False):
    kind: str = ""


class _SarifResult(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    rule_id: str = ""
    level: str = ""
    message: _SarifMessage = msgspec.field(default_factory=_SarifMessage)
    locations: tuple[_SarifLocation, ...] = ()
    suppressions: tuple[_SarifSuppression, ...] = ()  # non-empty => suppressed in-source; the build honors it, so the rail must too


class _SarifRun(msgspec.Struct, frozen=True, gc=False):
    results: tuple[_SarifResult, ...] = ()


class _SarifLog(msgspec.Struct, frozen=True, gc=False):
    """Roslyn-shaped SARIF 2.1 subset: runs[].results[] with ruleId/level/message/locations."""

    runs: tuple[_SarifRun, ...] = ()


# --- [TOOL_JSON]


class _PyAnalyzerDiagnostic(msgspec.Struct, frozen=True, gc=False):
    rule_id: str = ""
    severity: str = ""
    path: str = ""
    line: int = 0
    column: int = 0
    title: str = ""
    message: str = ""


class _BiomePoint(msgspec.Struct, frozen=True, gc=False):
    line: int = 0
    column: int = 0


class _BiomeLocation(msgspec.Struct, frozen=True, gc=False):
    path: str = ""
    start: _BiomePoint = msgspec.field(default_factory=_BiomePoint)


class _BiomeDiagnostic(msgspec.Struct, frozen=True, gc=False):
    severity: str = ""
    message: str = ""
    category: str = ""
    location: _BiomeLocation = msgspec.field(default_factory=_BiomeLocation)


class _BiomeReport(msgspec.Struct, frozen=True, gc=False):
    diagnostics: tuple[_BiomeDiagnostic, ...] = ()


# --- [SEARCH_WIRE]


class _Point(msgspec.Struct, frozen=True, gc=False):
    line: int = 0
    column: int = 0


class _Range(msgspec.Struct, frozen=True, gc=False):
    start: _Point = msgspec.field(default_factory=_Point)
    end: _Point = msgspec.field(default_factory=_Point)


class AstMatch(msgspec.Struct, frozen=True, gc=False):
    """ast-grep JSON match row."""

    text: str = ""
    file: str = ""
    lines: str = ""
    replacement: str = ""
    range: _Range = msgspec.field(default_factory=_Range)


class Capture(msgspec.Struct, frozen=True, gc=False):
    """Tree-sitter capture row emitted by in-process queries."""

    name: str = ""
    text: str = ""
    file: str = ""
    line: int = 0
    column: int = 0
    end_line: int = 0
    end_column: int = 0
    start_byte: int = 0
    end_byte: int = 0
    pattern: int = 0
    ordinal: int = 0
    parse_error: bool = False
    truncated: bool = False


class _RgText(msgspec.Struct, frozen=True, gc=False):
    text: str = ""


class _RgData(msgspec.Struct, frozen=True, gc=False):
    path: _RgText = msgspec.field(default_factory=_RgText)
    lines: _RgText = msgspec.field(default_factory=_RgText)
    line_number: int = 0


class RgEvent(msgspec.Struct, frozen=True, gc=False):
    """Ripgrep NDJSON event with wire `type` projected to `kind`."""

    kind: str = msgspec.field(default="", name="type")
    data: _RgData = msgspec.field(default_factory=_RgData)


# --- [TEXT_POLICY]


@dataclass(frozen=True, slots=True)
class _TextPolicy:
    pattern: re.Pattern[str]
    tools: frozenset[str]
    rule: str = ""
    severity: str = ""
    message: str = ""
    rule_groups: tuple[str, ...] = ("rule",)
    severity_groups: tuple[str, ...] = ("severity",)
    path_groups: tuple[str, ...] = ("path",)
    line_groups: tuple[str, ...] = ("line",)
    column_groups: tuple[str, ...] = ("column",)
    message_groups: tuple[str, ...] = ("message",)

    def match(self, tool: str, line: str) -> Match | None:
        return (
            _diagnostic_match(
                tool,
                self.value(found, self.rule, self.rule_groups, tool),
                self.value(found, self.severity, self.severity_groups, "error"),
                self.value(found, "", self.path_groups, ""),
                self.value(found, "", self.line_groups, "0"),
                self.value(found, "", self.column_groups, "0"),
                self.value(found, self.message, self.message_groups, ""),
            )
            if (not self.tools or tool in self.tools) and (found := self.pattern.match(line)) is not None
            else None
        )

    @staticmethod
    def value(found: re.Match[str], literal: str, groups: tuple[str, ...], fallback: str) -> str:
        return literal or next((value for group in groups if (value := found.groupdict().get(group))), fallback)


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.diagnostics")

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TREE_SITTER]


@cache
def ts_language(grammar: Callable[[], object]) -> TSLanguage:
    """Compile and cache a tree-sitter language keyed on grammar-fn identity.

    Returns:
        TSLanguage compiled from the grammar factory; subsequent calls with the same
        callable return the cached instance.
    """
    return TSLanguage(grammar())


@cache
@catch(exception=QueryError)
def ts_query(grammar: Callable[[], object], query_src: str) -> TSQuery:
    """Compile and cache a tree-sitter query keyed on grammar-fn identity and source text.

    Returns:
        Ok carrying the compiled TSQuery, or Error carrying the QueryError from a
        syntactically invalid query_src.
    """
    return TSQuery(ts_language(grammar), query_src)


def node_text(node: Node) -> str:
    """Decode a tree-sitter node's text.

    Returns:
        UTF-8 text with invalid bytes replaced, or "" for a detached node.
    """
    raw = node.text
    return raw.decode(errors="replace") if raw is not None else ""


def cap_note(shown: int, total: int, cap: int, *, saturated: bool = False, tail: str = "full listing in artifact") -> tuple[str, ...]:
    """Render the one truncation-note grammar every capped listing shares.

    Returns:
        One note naming shown/total/cap and the full-payload route, or empty when nothing was clipped.
    """
    # Saturation means total is a floor; the note must name the full-payload route.
    detail = f"cap={cap}, match-limit saturated" if saturated else f"cap={cap}"
    return (f"results: {shown} of {total} ({detail}); {tail}",) if total > shown or saturated else ()


# --- [SARIF_FOLD]


@cache
def _sarif_decode(path: Path, stat_key: tuple[int, int]) -> _SarifLog:
    # SARIF rows are evidence only: unreadable or malformed documents fold to the empty log, never a fault.
    _ = stat_key  # content fingerprint participates in the cache key, not the read
    try:
        return _SARIF_LOG.decode(path.read_bytes())
    except OSError, msgspec.MsgspecError:
        return _SarifLog()


def _sarif_log(path: Path) -> _SarifLog:
    # Decode once per (path, mtime, size): rows/status/results read the same build-written document inside one fold, so
    # the fingerprint collapses those redundant decodes; a rebuild that rewrites the document in a long-lived process
    # (the automation daemon re-fires one stable build closure) shifts the fingerprint, so the stale log never sticks.
    try:
        info = path.stat()
    except OSError:
        return _SarifLog()
    return _sarif_decode(path, (info.st_mtime_ns, info.st_size))


def _code_match(rule_id: str, severity: str, path: str, line: int, column: int, message: str, *, text: str, project: str = "") -> Match:
    # One source-diagnostic projection: SARIF, C# console, and text/JSON tool rows differ only in id/text/severity/project
    # derivation; the Match skeleton (CODE kind, score=column, capped text) is one owner.
    return Match(
        id=rule_id,
        kind=ArtifactKind.CODE,
        text=text[:_MATCH_TEXT_CAP],
        line=line,
        column=column,
        score=column,
        severity=severity,
        path=path,
        project=project,
        message=message,
    )


def _sarif_match(result: _SarifResult) -> Match:
    uri = next((loc.physical_location.artifact_location.uri for loc in result.locations), "")
    line = next((loc.physical_location.region.start_line for loc in result.locations), 0)
    column = next((loc.physical_location.region.start_column for loc in result.locations), 0)
    parsed = urlparse(uri)
    path = unquote(parsed.path if parsed.scheme == "file" else uri)
    message = result.message.text.strip()
    return _code_match(
        result.rule_id.lower(), _SARIF_SEVERITY.get(result.level, "warning"), path, line, column, message, text=f"{path}({line},{column}): {message}"
    )


def _sarif_files(base: Path, stamped: str, stem: str, *, slnx: bool) -> tuple[Path, ...]:
    # The typed Completed.sarif_dir stamp is authoritative; the fold never re-parses argv for the drop directory.
    active = Path(stamped) if stamped else base
    match (slnx, bool(stem)):
        case (True, _):
            return tuple(sorted(active.glob("*.sarif")))
        case (False, True):
            return (active / f"{stem}.sarif",)
        case _:
            return tuple(sorted(active.glob("*.sarif")))


def _sarif_rows(sarif_dir: str | None, outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    if not sarif_dir:
        return ()
    base = Path(sarif_dir)
    builds = tuple(done for done in outcomes if "dotnet" in done.argv and "build" in done.argv)
    files = (
        tuple(
            path
            for done in builds
            for stem, slnx in (_build_targets(done.argv) or (("", True),))
            for path in _sarif_files(base, done.sarif_dir, stem, slnx=slnx)
        )
        if builds
        else tuple(sorted(base.glob("*.sarif")))
    )
    return tuple(
        _sarif_match(result) for path in files if path.is_file() for run in _sarif_log(path).runs for result in run.results if not result.suppressions
    )


def _build_targets(argv: tuple[str, ...]) -> tuple[tuple[str, bool], ...]:
    # The analyzer drops one ``$(MSBuildProjectName).sarif`` per built project; a .csproj argv names one stem keyed to
    # its own file, a .slnx argv names the whole solution and keys against every SARIF the directory holds.
    return tuple((Path(token).stem, token.endswith(".slnx")) for token in argv if token.endswith((".csproj", ".slnx")))


def sarif_status(outcomes: tuple[Completed, ...], sarif_dir: str | None) -> tuple[tuple[str, str], ...]:
    """Classify each C# build outcome's SARIF evidence into (stem, status-token) rows.

    Returns:
        One ``(csproj-stem, SarifStatus token)`` row per dotnet build outcome; ``absent:*`` reasons keep a
        warm-incremental skip distinct from a clean analyzer pass.
    """
    base = Path(sarif_dir) if sarif_dir else None
    return tuple(
        (stem, _classify_sarif(done.status, base, done.sarif_dir, stem, slnx=slnx).token(_sarif_results(base, done.sarif_dir, stem, slnx=slnx)))
        for done in outcomes
        if "dotnet" in done.argv and "build" in done.argv
        for stem, slnx in (_build_targets(done.argv) or (("", False),))
    )


def _classify_sarif(status: RailStatus, base: Path | None, stamped: str, stem: str, *, slnx: bool) -> SarifStatus:
    produced = base is not None and any(path.is_file() for path in _sarif_files(base, stamped, stem, slnx=slnx))
    match (produced, status):
        case (True, _):
            return SarifStatus.PRODUCED
        case (False, RailStatus.SKIP):
            return SarifStatus.NO_BUILD
        case (False, RailStatus.OK | RailStatus.EMPTY):
            return SarifStatus.INCREMENTAL
        case _:
            return SarifStatus.BUILD_FAILED


def _sarif_results(base: Path | None, stamped: str, stem: str, *, slnx: bool) -> int:
    if base is None:
        return 0
    files = _sarif_files(base, stamped, stem, slnx=slnx)
    return sum(1 for path in files if path.is_file() for run in _sarif_log(path).runs for result in run.results if not result.suppressions)


def _csharp_rows(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    rows = tuple(
        _code_match(
            found.group("rule").lower(),
            found.group("severity").lower(),
            found.group("path"),
            int(found.group("line")),
            int(found.group("column")),
            message := found.group("message").strip(),
            text=f"{found.group('path')}({found.group('line')},{found.group('column')}): {message}"
            + (f" [project={project}]" if (project := (found.group("project") or "").strip()) else ""),
            project=project,
        )
        for done in outcomes
        if done.parser is Parser.CS_CONSOLE
        for line in (done.stdout + b"\n" + done.stderr).decode(errors="replace").splitlines()
        for found in (_CS_DIAGNOSTIC.search(line.strip()),)
        if found is not None
    )
    if rows:
        _LOG.info(
            "diagnostic.discovered",
            count=len(rows),
            source=sum(1 for row in rows if not _generated(row)),
            generated=sum(1 for row in rows if _generated(row)),
        )
    return rows


def _rows_of(done: Completed) -> tuple[Match, ...]:
    # CS_CONSOLE folds across the outcome batch in _csharp_rows; every other family converts per receipt.
    match _CONVERTERS.get(done.parser):
        case None:
            return ()
        case convert:
            return convert((done.stdout + b"\n" + done.stderr).decode(errors="replace"))


def _text_rows(tool: str, payload: str) -> tuple[Match, ...]:
    rows: list[Match] = []
    pending: tuple[str, str, str] | None = None
    for raw in payload.splitlines():
        line = _ANSI_ESCAPE.sub("", raw).strip()
        if not line:
            continue
        row = next((match for policy in _TEXT_POLICIES if (match := policy.match(tool, line)) is not None), None)
        if row is not None:
            rows.append(row)
            continue
        if (found := _HEADER_DIAGNOSTIC.match(line)) is not None:
            pending = (_severity(found.group("severity")), found.group("rule") or tool, found.group("message").strip())
            continue
        if pending is not None and (found := _ARROW_LOCATION.match(line)) is not None:
            severity, rule, message = pending
            rows.append(_diagnostic_match(tool, rule, severity, found.group("path"), found.group("line"), found.group("column"), message))
            pending = None
    return tuple(rows)


def _json_object[T](payload: str, decoder: msgspec.json.Decoder[T]) -> str:
    start = payload.find("{")
    end = payload.rfind("}") + 1
    if start < 0 or end <= start:
        return ""
    try:
        candidate = payload[start:end]
        _ = decoder.decode(candidate.encode())
    except msgspec.MsgspecError:
        return ""
    return candidate


def _json_rows[T](
    payload: str, *, decoder: msgspec.json.Decoder[T], project: str, rows: Callable[[T], tuple[Match, ...]], embedded: bool = False
) -> tuple[Match, ...]:
    # One JSON-diagnostic projection: a tool whose stdout is a bare diagnostic document decodes the whole payload; a tool
    # that frames its JSON inside log chatter (``embedded``) carves the object span first, then folds to text rows when the
    # decode yields nothing. The decoder, project label, and per-schema row map are the only axes that vary.
    span = _json_object(payload, decoder) if embedded else payload
    if not span:
        return _text_rows(tool=project, payload=payload)
    try:
        decoded = decoder.decode(span.encode())
    except msgspec.MsgspecError:
        return _text_rows(tool=project, payload=payload) if embedded else ()
    projected = rows(decoded)
    return projected if projected or not embedded else _text_rows(tool=project, payload=payload)


def _severity(raw: str) -> str:
    return {"warn": "warning", "warning": "warning", "note": "info", "info": "info", "error": "error"}.get(raw.lower(), "error")


def _diagnostic_match(tool: str, rule: str, severity: str, path: str, line: str, column: str, message: str) -> Match:
    line_number, column_number, rule_id = int(line), int(column), rule.lower()
    return _code_match(
        f"{tool}:{rule_id}",
        _severity(severity),
        path,
        line_number,
        column_number,
        message,
        text=f"{tool}: {path}:{line_number}:{column_number}: {rule_id}: {message}",
        project=tool,
    )


def _generated(row: Match) -> bool:
    path = (row.path or row.text.split(": ", 1)[0]).replace("\\", "/").lower()
    return any(marker in path for marker in _GENERATED_MARKERS) or path.endswith(_GENERATED_SUFFIX)


def _norm_path(path: str) -> str:
    # Cross-channel dedup canonical form: the console emits a cwd-relative path while the SARIF uri is absolute, so the
    # same diagnostic keys differently unless both anchor on the assay cwd (the repo root every dotnet build runs from).
    return Path(path.replace("\\", "/")).absolute().as_posix().lower() if path else ""


def _dedupe(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    seen: set[tuple[str, str | None, str, int, int, str]] = set()
    out: list[Match] = []
    for row in rows:
        key = (row.id, row.severity, _norm_path(row.path), row.line, row.column, row.message or row.text)
        if key not in seen:
            seen.add(key)
            out.append(row)
    return tuple(out)


def _group_generated(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    grouped: dict[tuple[str, str | None, str, str], int] = {}
    for row in rows:
        body = row.message or (row.text.split(": ", 1)[1] if ": " in row.text else row.text)
        key = (row.id, row.severity, row.project.replace("\\", "/").lower(), body)
        grouped[key] = grouped.get(key, 0) + 1
    return tuple(
        Match(
            id=rule,
            kind=ArtifactKind.PROCESS,
            text=f"generated diagnostics grouped count={count}: {body}"[:_MATCH_TEXT_CAP],
            severity=severity,
            project=project,
            message=body,
            count=count,
        )
        for (rule, severity, project, body), count in sorted(
            grouped.items(), key=lambda item: (_DIAGNOSTIC_SEVERITY_RANK.get(item[0][1] or "", 9), item[0])
        )
    )


def _rank(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    return tuple(
        sorted(rows, key=lambda row: (_DIAGNOSTIC_SEVERITY_RANK.get(row.severity or "", 9), row.id, row.path, row.line, row.column, row.text))
    )


def _result_rows(claim: Claim, outcomes: tuple[Completed, ...], defects: tuple[Match, ...], sarif_dir: str | None) -> tuple[Match, ...]:
    sarif = _sarif_rows(sarif_dir, outcomes)
    diagnostics = (*tuple(row for done in outcomes for row in _rows_of(done)), *_csharp_rows(outcomes), *sarif)
    if claim is not Claim.STATIC:
        return (*defects, *sarif)
    source = _rank(_dedupe(tuple(row for row in diagnostics if not _generated(row))))
    generated = _group_generated(tuple(row for row in diagnostics if _generated(row)))
    return (*source, *generated, *defects)


def _diagnostic_notes(rows: tuple[Match, ...]) -> tuple[str, ...]:
    weights = tuple((m, m.count or 1) for m in rows)
    rule_counts: Counter[str] = Counter()
    for row, count in weights:
        rule_counts[row.id] += count
    summary = (
        f"diagnostics: total={sum(count for _, count in weights)} "
        f"source={sum(count for row, count in weights if row.kind is ArtifactKind.CODE)} "
        f"generated={sum(count for row, count in weights if row.kind is ArtifactKind.PROCESS and row.count)} "
        f"error={sum(count for row, count in weights if row.severity == 'error')} "
        f"warning={sum(count for row, count in weights if row.severity == 'warning')} "
        f"info={sum(count for row, count in weights if row.severity == 'info')}"
    )
    return (
        summary,
        "diagnostics.rules: " + " ".join(f"{rule}={count}" for rule, count in sorted(rule_counts.items(), key=lambda item: (-item[1], item[0]))[:16]),
    )


def _count(done: Completed) -> tuple[int, int]:
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP:
            return 1, 0
        case RailStatus.FAILED:
            return 0, 1
        case _:
            return 0, 0


def fold(
    claim: Claim,
    verb: str,
    outcomes: tuple[Completed, ...],
    *,
    detail: AnyDetail | None = None,
    sarif_dir: str | None = None,
    promote_empty: bool = False,
) -> Report:
    """Fold process outcomes and evidence into a rail report.

    Static source error rows fail the report; generated diagnostics remain evidence-only, and absent SARIF folds to no rows.
    ``promote_empty`` lets a process-backed rail (eligible claim, ran cleanly, no defects) report OK for a folded-empty
    run; without it a clean no-op stays EMPTY so non-rail folds reusing the claim keep their natural absence.

    Returns:
        Report carrying status, counts, artifacts, evidence rows, notes, and optional detail.
    """
    pairs = tuple(map(_count, outcomes))
    ok_n = sum(ok for ok, _ in pairs)
    fail_n = sum(failed for _, failed in pairs)
    defects = tuple(
        Match(
            id=shlex.join(o.argv) if o.argv else claim.value,
            kind=ArtifactKind.PROCESS,
            text=(o.stderr or o.stdout)[-_DEFECT_TAIL:].decode(errors="replace").strip(),
            severity="failed",
        )
        for o, p in zip(outcomes, pairs, strict=True)
        if p == (0, 1)
    )
    results = _result_rows(claim, outcomes, defects, sarif_dir)
    diagnostic_rows = tuple(m for m in results if m.severity in _SARIF_SEVERITY.values() and m.id)
    source_error = any(row.kind is ArtifactKind.CODE and row.severity == "error" for row in diagnostic_rows)
    folded_status = RailStatus.fold(*(o.status for o in outcomes))
    status = (
        RailStatus.FAILED
        if claim is Claim.STATIC and source_error
        else RailStatus.OK
        if promote_empty and claim in _PROCESS_BACKED_OK_CLAIMS and folded_status is RailStatus.EMPTY and bool(outcomes) and not defects
        else folded_status
    )
    return Report(
        claim,
        verb,
        status,
        Counts(ok_n, fail_n, ok_n + fail_n),
        results=results,
        artifacts=tuple(artifact for o in outcomes for artifact in o.artifacts),
        notes=(
            *tuple(n for o in outcomes for n in o.notes),
            *((_diagnostic_notes(diagnostic_rows)) if claim is Claim.STATIC and diagnostic_rows else ()),
        ),
        detail=detail,
        # Remote-execution facts ride the dedicated carrier: a multi-check fan-out over one host folds every leg's transfer counts.
        exec=ExecReceipt.merge(tuple(o.exec for o in outcomes if o.exec is not None)),
    )


# --- [TABLES] ---------------------------------------------------------------------------
# Converter rows reference the projection functions above; module-level decode tables resolve real objects.

_SARIF_LOG: msgspec.json.Decoder[_SarifLog] = msgspec.json.Decoder(_SarifLog)
_PY_ANALYZER_LOG: msgspec.json.Decoder[tuple[_PyAnalyzerDiagnostic, ...]] = msgspec.json.Decoder(tuple[_PyAnalyzerDiagnostic, ...])
_BIOME_LOG: msgspec.json.Decoder[_BiomeReport] = msgspec.json.Decoder(_BiomeReport)
AST_MATCHES = msgspec.json.Decoder(tuple[AstMatch, ...])
CAPTURES = msgspec.json.Decoder(tuple[Capture, ...])
CAPTURE_ENCODER = msgspec.json.Encoder()
RG_EVENT = msgspec.json.Decoder(RgEvent)

_TEXT_POLICIES: tuple[_TextPolicy, ...] = (
    _TextPolicy(_MYPY_DIAGNOSTIC, frozenset(("mypy",))),
    _TextPolicy(_TSC_DIAGNOSTIC, frozenset(("tsc",)), line_groups=("line1", "line2"), column_groups=("column1", "column2")),
    _TextPolicy(_BIOME_TEXT_DIAGNOSTIC, frozenset(("biome",)), severity="error"),
    _TextPolicy(
        _FORMAT_DIAGNOSTIC,
        frozenset(("ruff-format",)),
        rule="format",
        severity="error",
        message="file would be reformatted",
        path_groups=("path", "path2"),
    ),
)

# One converter per per-receipt family; CS_CONSOLE folds batch-wide in _csharp_rows for the discovered-count log.
_CONVERTERS: dict[Parser, Callable[[str], tuple[Match, ...]]] = {
    Parser.RUFF: lambda payload: _text_rows("ruff", payload),
    Parser.RUFF_FORMAT: lambda payload: _text_rows("ruff-format", payload),
    Parser.TY: lambda payload: _text_rows("ty", payload),
    Parser.MYPY: lambda payload: _text_rows("mypy", payload),
    Parser.TSC: lambda payload: _text_rows("tsc", payload),
    Parser.PY_ANALYZER: lambda payload: _json_rows(
        payload,
        decoder=_PY_ANALYZER_LOG,
        project="py-analyzer",
        rows=lambda diagnostics: tuple(
            _diagnostic_match("py-analyzer", row.rule_id, row.severity, row.path, str(row.line), str(row.column), row.message or row.title)
            for row in diagnostics
        ),
    ),
    Parser.BIOME: lambda payload: _json_rows(
        payload,
        decoder=_BIOME_LOG,
        project="biome",
        embedded=True,
        rows=lambda report: tuple(
            _diagnostic_match(
                "biome",
                row.category or "biome",
                row.severity,
                row.location.path,
                str(row.location.start.line),
                str(row.location.start.column),
                row.message,
            )
            for row in report.diagnostics
        ),
    ),
}

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AST_MATCHES",
    "CAPTURES",
    "CAPTURE_ENCODER",
    "Capture",
    "RG_EVENT",
    "cap_note",
    "fold",
    "node_text",
    "sarif_status",
    "ts_language",
    "ts_query",
]

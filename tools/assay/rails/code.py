"""Search code with ast-grep, ripgrep, and in-process tree-sitter query rails."""

from dataclasses import dataclass, replace
from functools import reduce
from hashlib import sha256
from pathlib import Path
import re
from typing import Annotated, ClassVar, Final, override, TYPE_CHECKING

from cyclopts import Parameter
from cyclopts.types import NonNegativeInt  # noqa: TC002  # Cyclopts evaluates Param dataclass annotations at runtime.
from expression import Error, Result  # Result unconditional: @checked's beartype resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
from tree_sitter import Parser as TSParser, QueryCursor
import tree_sitter_python
import tree_sitter_typescript

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # unconditional: @checked beartype forward-ref (PEP 649)
from tools.assay.composition.store import ArtifactScope  # noqa: TC001  # unconditional: @checked beartype forward-ref (PEP 649)
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Counts,
    Fault,  # unconditional: @checked resolves the `-> Result[Report, Fault]` forward-ref under PEP 649
    Language,
    language_choice,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,
    RESULT_CAP,
    Step,
    ToolArgs,
)
from tools.assay.core.routing import resolve_languages, route, Routed, Scope
from tools.assay.diagnostics import AST_MATCHES, cap_note, Capture, CAPTURE_ENCODER, CAPTURES, fold, node_text, RG_EVENT, ts_language, ts_query


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression.collections import Block

    from tools.assay.core.model import InprocThunk, Tool
    from tools.assay.diagnostics import AstMatch, RgEvent


# --- [CONSTANTS] ------------------------------------------------------------------------

_TEXT_CAP: int = 320
_DEFAULT_TARGET: tuple[str, ...] = (".",)
_METAVAR: re.Pattern[str] = re.compile(r"\$[A-Z_$]")

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class CodeParams(BaseParams):
    """Parameters shared by code verbs."""

    SLOTS: ClassVar[dict[str, str]] = {"": "PATTERN [PATHS]..."}

    # language selectors are optional; hide default so help does not advertise an unset flag
    csharp: Annotated[bool, Parameter(name="--csharp", negative="", show_default=False, help="Restrict the command to C# targets.")] = False
    python: Annotated[bool, Parameter(name="--python", negative="", show_default=False, help="Restrict the command to Python targets.")] = False
    typescript: Annotated[
        bool, Parameter(name="--typescript", negative="", show_default=False, help="Restrict the command to TypeScript targets.")
    ] = False
    language: Annotated[Language | None, Parameter(parse=False)] = None
    pattern: Annotated[
        str, Parameter(allow_leading_hyphen=True, help="Pattern; the leading positional fills this slot when the flag is omitted.")
    ] = ""
    max_results: NonNegativeInt = 1000

    @override
    def bound(self, verb: str) -> CodeParams | Fault:
        """Project the leading positional into the pattern slot for code verbs.

        Args:
            verb: Code verb name; search/query consume one leading pattern positional.

        Returns:
            Bound params, or a parse fault when the required pattern is blank.
        """
        match language_choice(verb, csharp=self.csharp, python=self.python, typescript=self.typescript):
            case Fault() as fault:
                return fault
            case language:
                base = replace(self, language=language)
        match (verb, base.pattern, base.paths):
            case (("search" | "query"), "", (pattern, *rest)):
                projected = replace(base, pattern=pattern, paths=tuple(rest))
            case _:
                projected = base
        match (verb, projected.pattern.strip()):
            case (("search" | "query"), ""):
                return Fault((), RailStatus.FAULTED, f"{Step.PARSE}: {verb}: pattern required")
            case _:
                return projected


@dataclass(frozen=True, slots=True, kw_only=True)
class _RowSpec[M]:
    """One match-family projection row: decode/extract, listing line, Match projection, saturation."""

    extract: Callable[[tuple[Completed, ...]], tuple[M, ...]]
    entry: Callable[[M], str]
    row: Callable[[M, str], Match]
    saturated: Callable[[tuple[M, ...]], bool]


# --- [TABLES] ---------------------------------------------------------------------------

_GRAMMARS: dict[Language, Callable[[], object]] = {
    Language.PYTHON: tree_sitter_python.language,
    Language.TYPESCRIPT: tree_sitter_typescript.language_typescript,
}
_TSX_GRAMMAR: Callable[[], object] = tree_sitter_typescript.language_tsx

# --- [OPERATIONS] -----------------------------------------------------------------------


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    # Code search defaults to the whole worktree, unlike changed-files quality gates.
    return sequence(block.of_seq(route(language, paths or _DEFAULT_TARGET, settings=settings) for language in languages))


def _checks(routed: Routed, mode: Mode, splice: Callable[[Tool, Routed], Check]) -> tuple[Check, ...]:
    match routed.files:
        case ():
            return ()
        case _:
            return tuple(splice(t, routed) for t in select(Claim.CODE, routed.language) if t.mode is mode)


def _dispatch(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode, splice: Callable[[Tool, Routed], Check], executor: Executor
) -> tuple[Result[Completed, Fault], ...]:
    checks = _checks(routed, mode, splice)
    match checks:
        case ():
            return ()
        case _:
            return executor.fan(checks, settings=settings, scope=scope, routed=routed)


def _fan(
    settings: AssaySettings, scope: ArtifactScope, params: CodeParams, *, mode: Mode, splice: Callable[[Tool, Routed], Check], executor: Executor
) -> Result[tuple[Completed, ...], Fault]:
    # Routing, spawn, and timeout Faults short-circuit; non-zero tool exits stay on Completed.
    return resolve_languages(params.language, params.paths, claim=Claim.CODE).bind(
        lambda languages: _routed(languages, params.paths, settings).bind(
            lambda routed: sequence(
                routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode, splice=splice, executor=executor)))
            ).map(tuple)
        )
    )


def _targets(paths: tuple[str, ...], root: Path) -> tuple[str, ...]:
    # ast-grep aborts on a missing explicit target; ripgrep walks cwd on an empty tail — both require an explicit fallback.
    match paths:
        case ():
            return _DEFAULT_TARGET
        case _:
            return tuple(p for p in paths if (root / p).exists()) or _DEFAULT_TARGET


def _search_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Check]:
    # Targets ride the command body to avoid ARG_MAX; tracked dot-dirs stay visible, gitignored dirs stay excluded.
    targets = _targets(params.paths, root)
    return lambda tool, routed: Check(
        tool=tool, paths=routed.files, args=ToolArgs(pattern=params.pattern, language=routed.language.value, targets=targets)
    )


def _query_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Check]:
    return lambda tool, routed: Check(tool=tool, paths=routed.files, thunk=_ts_thunk(params.pattern, routed.language, root, limit=params.max_results))


def _top_level_patterns(query_src: str) -> int:
    masked = re.sub(r";[^\n]*", "", re.sub(r'"(?:[^"\\]|\\.)*"', '""', query_src))

    def step(state: tuple[int, int], ch: str) -> tuple[int, int]:
        depth, count = state
        match ch:
            case "(":
                return (depth + 1, count + (1 if depth == 0 else 0))
            case ")":
                return (max(0, depth - 1), count)
            case _:
                return (depth, count)

    return reduce(step, masked, (0, 0))[1]


def _eq_needles(query_src: str) -> tuple[frozenset[bytes], ...] | None:
    """Extract literal byte-needle sets for file prefiltering from a single-pattern query.

    Single-pattern queries that use only ``#eq?`` or ``#any-of?`` predicates admit
    file-level prefiltering by literal byte presence with zero false negatives; files that
    lack any needle from a required group cannot match the pattern.

    Returns:
        Tuple of frozensets where each set is the accepted literals for one predicate
        group, or None when the query structure does not admit prefiltering.
    """
    predicates = frozenset(re.findall(r"#([a-z][a-z-]*)\?", query_src))
    match (predicates, _top_level_patterns(query_src)):
        case (preds, 1) if preds <= frozenset(("eq", "any-of")) and preds:
            groups = (
                *(frozenset((literal.encode(),)) for literal in re.findall(r'#eq\?\s+@\S+\s+"([^"\\]*)"', query_src)),
                *(
                    frozenset(literal.encode() for literal in re.findall(r'"([^"\\]*)"', body))
                    for body in re.findall(r"#any-of\?\s+@\S+((?:\s+\"[^\"\\]*\")+)", query_src)
                ),
            )
            return groups if groups and all(groups) else None
        case _:
            return None


def _ts_grammar(language: Language, *, is_tsx: bool) -> Callable[[], object]:
    return _TSX_GRAMMAR if language is Language.TYPESCRIPT and is_tsx else _GRAMMARS[language]


def _read(path: Path) -> bytes | None:
    try:
        return path.read_bytes()
    except OSError:
        return None


def _ts_file_captures(
    query_src: str, language: Language, rel: str, src: bytes, *, cap: int, parsers: dict[Callable[[], object], TSParser]
) -> tuple[Capture, ...]:
    grammar = _ts_grammar(language, is_tsx=rel.endswith(".tsx"))
    root_node = (parsers.get(grammar) or parsers.setdefault(grammar, TSParser(ts_language(grammar)))).parse(src).root_node
    match ts_query(grammar, query_src):
        case Result(tag="error", error=exc):
            return (Capture(name="query_error", text=str(exc)[:_TEXT_CAP], file=rel, line=1, parse_error=True),)
        case compiled:
            cursor = QueryCursor(compiled.ok)
    cursor.match_limit = cap
    raw_matches = tuple(cursor.matches(root_node))
    matches = raw_matches[:cap]
    truncated = cursor.did_exceed_match_limit or len(raw_matches) > cap
    parse_error = (Capture(name="parse_error", text="tree-sitter parse error", file=rel, line=1, parse_error=True, truncated=truncated),)
    captures = tuple(
        Capture(
            name=name,
            text=node_text(node)[:_TEXT_CAP],
            file=rel,
            line=node.start_point.row + 1,
            column=node.start_point.column + 1,
            end_line=node.end_point.row + 1,
            end_column=node.end_point.column + 1,
            start_byte=node.start_byte,
            end_byte=node.end_byte,
            pattern=pattern,
            ordinal=ordinal,
            truncated=truncated,
        )
        for ordinal, (pattern, grouped) in enumerate(matches)
        for name, nodes in grouped.items()
        for node in nodes
    )
    return (*parse_error, *captures) if root_node.has_error else captures


def _ts_thunk(query_src: str, language: Language, root: Path, *, limit: int) -> InprocThunk:
    """Build an INPROC thunk that runs a tree-sitter query over language-routed files.

    Captures are encoded into Completed.stdout to match the subprocess fan-out contract.
    When ``_eq_needles`` extracts literal byte sets, files that lack a required needle
    are skipped before parsing to reduce GIL-bound work on large trees.

    Returns:
        Callable that accepts a Check and returns a Completed with captures encoded
        in stdout and a RailStatus reflecting query success, empty match, or parse error.
    """
    needles = _eq_needles(query_src)
    cap = limit if limit > 0 else RESULT_CAP
    parsers: dict[Callable[[], object], TSParser] = {}

    def run(check: Check) -> Completed:
        rows = tuple(
            cap_row
            for rel in check.paths
            for src in (_read(root / rel),)
            if src is not None and (needles is None or all(any(needle in src for needle in group) for group in needles))
            for cap_row in _ts_file_captures(query_src, language, rel, src, cap=cap, parsers=parsers)
        )
        captures = tuple(rows[:cap])
        status = RailStatus.FAILED if any(c.parse_error for c in captures) else RailStatus.OK if captures else RailStatus.EMPTY
        return receipt(
            ("tree-sitter", "query", language, *check.paths),
            1 if status is RailStatus.FAILED else 0,
            stdout=CAPTURE_ENCODER.encode(captures),
            status=status,
        )

    return run


def _artifact(scope: ArtifactScope, verb: str, content: str, settings: AssaySettings) -> Artifact:
    # run_id prevents concurrent listing clobber; content hash gives delta a body-free identity.
    raw = content.encode()
    digest = sha256(raw).hexdigest()[:12]
    path = scope.store.write_bytes(raw, ArtifactKind.CODE.value, verb, settings.run_id, "matches.txt")
    return Artifact(id=digest, kind=ArtifactKind.CODE, path=str(path), bytes=len(raw), lines=len(content.splitlines()))


def _safe_decode[T, E](decoder: msgspec.json.Decoder[T], raw: bytes, empty: E) -> T | E:
    # Non-JSON stdout degrades to the sentinel so fan-out callers keep the rail shape.
    try:
        return decoder.decode(raw)
    except msgspec.MsgspecError:
        return empty


def _report(
    settings: AssaySettings,
    scope: ArtifactScope,
    verb: str,
    pattern: str,
    completeds: tuple[Completed, ...],
    rows: tuple[Match, ...],
    listing: str,
    notes: tuple[str, ...],
) -> Report:
    # Rows promote an EMPTY base status to OK; malformed query captures keep their defect
    # rows; full match cardinality rides Artifact.lines, with a cap note when rows were cut.
    base = fold(Claim.CODE, verb, completeds)
    status = RailStatus.OK if rows and base.status is RailStatus.EMPTY else base.status
    done = Completed(("code", verb, pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=notes)
    final_rows = rows or base.results
    return msgspec.structs.replace(
        fold(Claim.CODE, verb, (done,)), artifacts=(_artifact(scope, verb, listing, settings),) if listing else (), results=final_rows
    )


def _relevance(pattern: str, text: str) -> int:
    return max(1, min(100, len(pattern) * 100 // max(len(text), 1)))


# One row spec owns decode, listing text, Match projection, and saturation per match family.
_AG_SPEC: Final[_RowSpec[AstMatch]] = _RowSpec(
    extract=lambda completeds: tuple(m for done in completeds for m in _safe_decode(AST_MATCHES, done.stdout or b"[]", ())),
    entry=lambda m: f"{m.file}:{m.range.start.line + 1}: {m.text}" + (f" => {m.replacement}" if m.replacement else ""),
    row=lambda m, pattern: Match(
        id=f"ast-grep:{m.file}:{m.range.start.line + 1}",
        kind=ArtifactKind.CODE,
        text=(f"{m.text} => {m.replacement}" if m.replacement else m.text)[:_TEXT_CAP],
        line=m.range.start.line + 1,
        score=_relevance(pattern, m.text),
    ),
    saturated=lambda _matches: False,
)
_TS_SPEC: Final[_RowSpec[Capture]] = _RowSpec(
    extract=lambda completeds: tuple(c for done in completeds for c in _safe_decode(CAPTURES, done.stdout or b"[]", ())),
    entry=lambda c: f"{c.file}:{c.line}:{c.column}: @{c.name}#{c.ordinal}/{c.pattern} {c.text}",
    row=lambda c, pattern: Match(
        id=f"tree-sitter:{c.file}:{c.line}:{c.name}",
        kind=ArtifactKind.CODE,
        text=f"{'parse-error ' if c.parse_error else ''}@{c.name} {c.text}"[:_TEXT_CAP],
        line=c.line,
        score=_relevance(pattern, c.text),
        severity="failed" if c.parse_error else None,
    ),
    saturated=lambda captures: any(c.truncated for c in captures),
)
# Only ripgrep "match" events carry hit data; begin/end/summary events are intentionally dropped.
_RG_SPEC: Final[_RowSpec[RgEvent]] = _RowSpec(
    extract=lambda completeds: tuple(
        e
        for done in completeds
        for line in (done.stdout or b"").splitlines()
        if line
        for e in (_safe_decode(RG_EVENT, line, None),)
        if e is not None and e.kind == "match"
    ),
    entry=lambda e: f"{e.data.path.text}:{e.data.line_number}: {e.data.lines.text.rstrip()}",
    row=lambda e, pattern: Match(
        id=f"ripgrep:{e.data.path.text}:{e.data.line_number}",
        kind=ArtifactKind.CODE,
        text=e.data.lines.text.rstrip()[:_TEXT_CAP],
        line=e.data.line_number,
        score=_relevance(pattern, e.data.lines.text.rstrip()),
    ),
    saturated=lambda _events: False,
)


def _project_rows[M](
    completeds: tuple[Completed, ...], cap: int, pattern: str, *, spec: _RowSpec[M]
) -> tuple[tuple[Match, ...], str, tuple[str, ...]]:
    # Rows are capped for the report; artifact listing stays complete across match families.
    matches = spec.extract(completeds)
    rows = tuple(spec.row(m, pattern) for m in matches[:cap])
    return rows, "\n".join(spec.entry(m) for m in matches), cap_note(len(rows), len(matches), cap, saturated=spec.saturated(matches))


def _content_args(params: CodeParams, root: Path) -> ToolArgs:
    # --glob narrows to language suffixes; missing paths drop to the default target, matching _targets behavior in ast-grep splices.
    globs = tuple(arg for suffix in (params.language.suffixes if params.language is not None else ()) for arg in ("--glob", f"*{suffix}"))
    return ToolArgs(globs=globs, pattern=params.pattern, targets=_targets(params.paths, root))


def _rg_status(returncode: int, stderr: str, *, has_rows: bool) -> tuple[RailStatus, tuple[str, ...]]:
    # ripgrep exit 2 is overloaded (partial match + error); hit presence distinguishes
    # partial success with a warning from a real tool failure.
    match (returncode, has_rows):
        case (_, True):
            warn = f"; ripgrep warning: {stderr[:200]}" if returncode not in {0, 1} and stderr else ""
            return RailStatus.OK, (f"content match{warn}",)
        case (0 | 1, False):
            return RailStatus.EMPTY, ("no content matches",)
        case _:
            return RailStatus.FAILED, (f"ripgrep failed (exit {returncode}): {stderr[:400] or 'error'}",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams, executor: Executor) -> Result[Report, Fault]:
    """Dispatch structural or content search over language-routed files.

    Patterns containing a metavariable (``$NAME`` form) route to ast-grep structural
    search; all other patterns route to ripgrep content search.

    Returns:
        Folded report with match rows and listing artifact, or a routing/spawn fault.
    """
    match bool(_METAVAR.search(params.pattern)):
        case True:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_search_splice(params, Path(str(settings.root))), executor=executor).map(
                lambda done: _report(
                    settings, scope, "search", params.pattern, done, *_project_rows(done, params.max_results, params.pattern, spec=_AG_SPEC)
                )
            )
        case False:  # pragma: no cover — exhaustive match(bool); sysmon arc to implicit exit unreachable
            return _content_search(settings, scope, params, executor)


def _content_search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams, executor: Executor) -> Result[Report, Fault]:
    # Synthetic routing satisfies the executor port; ripgrep remains grammar-blind until glob splicing.
    match next((t for t in select(Claim.CODE) if t.mode is Mode.CONTENT), None):
        case None:
            return Error(Fault(("code", "search"), status=RailStatus.FAULTED, message="no ripgrep content catalog row"))
        case tool:
            check = Check(tool=tool, paths=tuple(params.paths or _DEFAULT_TARGET), args=_content_args(params, Path(str(settings.root))))
            routed = Routed(language=tool.language, scope=Scope.CHANGED)
            return executor.run(check, settings=settings, scope=scope, routed=routed).map(lambda done: _content_report(settings, scope, params, done))


def _content_report(settings: AssaySettings, scope: ArtifactScope, params: CodeParams, done: Completed) -> Report:
    # Direct report construction preserves ripgrep diagnostics that synthetic fold rows would discard.
    rows, listing, cap_notes = _project_rows((done,), params.max_results, params.pattern, spec=_RG_SPEC)
    status, notes = _rg_status(done.returncode, (done.stderr or b"").decode(errors="replace").strip(), has_rows=bool(rows))
    failed = status is RailStatus.FAILED
    return Report(
        Claim.CODE,
        "search",
        status,
        Counts(0, 1, 1) if failed else Counts(1, 0, 1),
        artifacts=(_artifact(scope, "search", listing, settings),) if listing else (),
        results=rows,
        notes=(*notes, *cap_notes),
    )


def query(settings: AssaySettings, scope: ArtifactScope, params: CodeParams, executor: Executor) -> Result[Report, Fault]:
    """Run an in-process tree-sitter query over grammar-backed language files.

    Queries execute via INPROC thunks; capture results are encoded into
    Completed.stdout to satisfy the fan-out contract. Files are prefiltered by
    literal byte needles when the query admits it.

    Returns:
        Folded report with capture rows and listing artifact, or a routing/spawn fault.
    """
    return _fan(settings, scope, params, mode=Mode.QUERY, splice=_query_splice(params, Path(str(settings.root))), executor=executor).map(
        lambda done: _report(settings, scope, "query", params.pattern, done, *_project_rows(done, params.max_results, params.pattern, spec=_TS_SPEC))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeParams", "query", "search"]

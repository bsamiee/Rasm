"""Structural search, rewrite, content search, and tree-sitter query rails for code.

Implements four public verbs -- search, rewrite, query, and content search -- over
language-routed file sets. Structural search and rewrite delegate to ast-grep; content
search delegates to ripgrep NDJSON; tree-sitter query runs in-process via INPROC thunks.
All verbs produce a folded Report on the Ok rail and a Fault on the Error rail.
"""

from dataclasses import dataclass, replace
from functools import cache, lru_cache, reduce
from hashlib import sha256
from pathlib import Path
import re
from typing import override, TYPE_CHECKING

from cyclopts.types import NonNegativeInt  # noqa: TC002  # Cyclopts evaluates Param dataclass annotations at runtime.
from expression import Error, Result  # noqa: TC002  # Result unconditional: @checked's beartype resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
from tree_sitter import Language as TSLanguage, Parser as TSParser, Query as TSQuery, QueryCursor, QueryError
import tree_sitter_python
import tree_sitter_typescript

from tools.assay.composition.catalog import AST_MATCHES, Capture, CAPTURE_ENCODER, CAPTURES, RG_EVENT, select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional: @checked beartype forward-ref (PEP 649)
from tools.assay.core.engine import fan_out, leased, run_check
from tools.assay.core.model import (
    _RESULT_CAP,  # noqa: PLC2701  # shared saturation site for in-process tree-sitter match limiting
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,  # unconditional: @checked resolves the `-> Result[Report, Fault]` forward-ref under PEP 649
    fold,
    Language,
    Match,
    Mode,
    receipt,
    Report,  # noqa: TC001  # unconditional: see Fault above (same forward-ref resolution)
    Tool,
)
from tools.assay.core.routing import route, Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression.collections import Block
    from tree_sitter import Node

    from tools.assay.core.model import InprocThunk


# --- [CONSTANTS] ------------------------------------------------------------------------

_TEXT_CAP: int = 320
_DEFAULT_TARGET: tuple[str, ...] = (".",)
_METAVAR: re.Pattern[str] = re.compile(r"\$[A-Z_$]")


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class CodeParams(BaseParams):
    """Parameters shared by code verbs."""

    pattern: str = ""
    rewrite: str = ""
    apply: bool = False
    max_results: NonNegativeInt = 1000

    @override
    def bound(self, verb: str) -> CodeParams | Fault:
        """Project leading positionals into the pattern and rewrite slots for a code verb.

        Code verbs are variadic over trailing paths, so positionals after the owned slots
        stay as search targets; a flag-supplied pattern is never overwritten. The projected
        pattern is then validated non-blank -- a blank pattern (no positional, an explicit
        empty string, or whitespace-only) is a parse fault, closing the match-everything
        degeneracy for ``code search``.

        Args:
            verb: The code verb name; controls how many positionals are consumed.

        Returns:
            Projected CodeParams with pattern filled from positionals, or a Fault when
            the resolved pattern is blank for a verb that requires one.
        """
        match (verb, self.pattern, self.paths):
            case ("rewrite", "", (pattern, rewrite, *rest)):
                projected = replace(self, pattern=pattern, rewrite=self.rewrite or rewrite, paths=tuple(rest))
            case (("search" | "query" | "rewrite"), "", (pattern, *rest)):
                projected = replace(self, pattern=pattern, paths=tuple(rest))
            case _:
                projected = self
        match (verb, projected.pattern.strip()):
            case (("search" | "query" | "rewrite"), ""):
                return Fault((), RailStatus.FAULTED, f"parse: {verb}: pattern required")
            case _:
                return projected


# --- [TABLES] ---------------------------------------------------------------------------

_GRAMMARS: dict[Language, Callable[[], object]] = {
    Language.PYTHON: tree_sitter_python.language,
    Language.TYPESCRIPT: tree_sitter_typescript.language_typescript,
}
_GRAMMAR_KEY_LANGUAGE: dict[str, Language] = {Language.PYTHON.value: Language.PYTHON, Language.TYPESCRIPT.value: Language.TYPESCRIPT}
_TSX_GRAMMAR: Callable[[], object] = tree_sitter_typescript.language_tsx


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None) -> tuple[Language, ...]:
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.CODE)}, key=lambda member: member.value))
        case language:
            return (language,)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    # Code search defaults to the whole worktree, unlike changed-files quality gates.
    return sequence(block.of_seq(route(language, paths or _DEFAULT_TARGET, settings=settings) for language in languages))


def _checks(routed: Routed, mode: Mode, splice: Callable[[Tool, Routed], Tool]) -> tuple[Check, ...]:
    match routed.files:
        case ():
            return ()
        case _:
            return tuple(Check(tool=splice(t, routed), paths=routed.files) for t in select(Claim.CODE, routed.language) if t.mode is mode)


def _dispatch(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode, splice: Callable[[Tool, Routed], Tool]
) -> tuple[Result[Completed, Fault], ...]:
    checks = _checks(routed, mode, splice)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _fan(
    settings: AssaySettings, scope: ArtifactScope, params: CodeParams, *, mode: Mode, splice: Callable[[Tool, Routed], Tool]
) -> Result[tuple[Completed, ...], Fault]:
    # Routing, spawn, and timeout Faults short-circuit; non-zero tool exits stay on Completed.
    return _routed(_languages(params.language), params.paths, settings).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode, splice=splice)))).map(
            tuple
        )
    )


def _targets(paths: tuple[str, ...], root: Path) -> tuple[str, ...]:
    # ast-grep aborts on a missing explicit target; ripgrep walks cwd on an empty tail — both require an explicit fallback.
    match paths:
        case ():
            return _DEFAULT_TARGET
        case _:
            return tuple(p for p in paths if (root / p).exists()) or _DEFAULT_TARGET


def _search_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Tool]:
    # Targets ride the command body to avoid ARG_MAX; tracked dot-dirs stay visible, gitignored dirs stay excluded.
    targets = _targets(params.paths, root)
    return lambda tool, routed: msgspec.structs.replace(
        tool, command=(*tool.command, "-p", params.pattern, "-l", routed.language.value, "--json=compact", "--no-ignore", "hidden", *targets)
    )


def _rewrite_splice(params: CodeParams, root: Path, *, apply: bool) -> Callable[[Tool, Routed], Tool]:
    tail = ("-U",) if apply else ("--json=compact",)
    targets = _targets(params.paths, root)
    return lambda tool, routed: msgspec.structs.replace(
        tool,
        command=(*tool.command, "-p", params.pattern, "-r", params.rewrite, "-l", routed.language.value, *tail, "--no-ignore", "hidden", *targets),
    )


def _query_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Tool]:
    return lambda tool, routed: msgspec.structs.replace(tool, thunk=_ts_thunk(params.pattern, routed.language, root, limit=params.max_results))


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


@cache
def ts_language(grammar: Callable[[], object]) -> TSLanguage:
    """Compile and cache a tree-sitter language keyed on grammar-fn identity.

    Returns:
        TSLanguage compiled from the grammar factory; subsequent calls with the same
        callable return the cached instance.
    """
    return TSLanguage(grammar())


@cache
def ts_query(grammar: Callable[[], object], query_src: str) -> TSQuery | QueryError:
    """Compile and cache a tree-sitter query keyed on grammar-fn identity and source text.

    Returns:
        Compiled TSQuery on success, or the QueryError from a syntactically invalid
        query_src; callers discriminate on the return type.
    """
    try:
        return TSQuery(ts_language(grammar), query_src)
    except QueryError as exc:
        return exc


@lru_cache(maxsize=8)
def _ts_language(grammar_key: str) -> TSLanguage:
    grammar = _TSX_GRAMMAR if grammar_key == "tsx" else _GRAMMARS[_GRAMMAR_KEY_LANGUAGE[grammar_key]]
    return ts_language(grammar)


def _node_text(node: Node) -> str:
    raw = node.text
    return raw.decode(errors="replace") if raw is not None else ""


def _read(path: Path) -> bytes | None:
    try:
        return path.read_bytes()
    except OSError:
        return None


def _ts_file_captures(query_src: str, language: Language, rel: str, src: bytes, *, cap: int) -> tuple[Capture, ...]:
    grammar = _ts_grammar(language, is_tsx=rel.endswith(".tsx"))
    root_node = TSParser(ts_language(grammar)).parse(src).root_node
    match ts_query(grammar, query_src):
        case QueryError() as exc:
            return (Capture(name="query_error", text=str(exc)[:_TEXT_CAP], file=rel, line=1, parse_error=True),)
        case compiled:
            cursor = QueryCursor(compiled)
    cursor.match_limit = cap
    raw_matches = tuple(cursor.matches(root_node))
    matches = raw_matches[:cap]
    truncated = cursor.did_exceed_match_limit or len(raw_matches) > cap
    parse_error = (Capture(name="parse_error", text="tree-sitter parse error", file=rel, line=1, parse_error=True, truncated=truncated),)
    captures = tuple(
        Capture(
            name=name,
            text=_node_text(node)[:_TEXT_CAP],
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
    cap = limit if limit > 0 else _RESULT_CAP

    def run(check: Check) -> Completed:
        rows = tuple(
            cap_row
            for rel in check.paths
            for src in (_read(root / rel),)
            if src is not None and (needles is None or all(any(needle in src for needle in group) for group in needles))
            for cap_row in _ts_file_captures(query_src, language, rel, src, cap=cap)
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
    # run_id in the path prevents concurrent listing clobber; content hash enables delta to
    # distinguish changed match sets across runs without reading the artifact body.
    raw = content.encode()
    digest = sha256(raw).hexdigest()[:12]
    path = scope.store.write_bytes(raw, ArtifactKind.CODE.value, verb, settings.run_id, "matches.txt")
    return Artifact(id=digest, kind=ArtifactKind.CODE, path=str(path), bytes=len(raw), lines=len(content.splitlines()))


def _safe_decode[T, E](decoder: msgspec.json.Decoder[T], raw: bytes, empty: E) -> T | E:
    # Non-JSON stdout (e.g., tool panic output) degrades to empty rather than propagating
    # MsgspecError across the fan-out pipeline; callers discriminate via the empty sentinel.
    try:
        return decoder.decode(raw)
    except msgspec.MsgspecError:
        return empty


def _ag_normalize(completeds: tuple[Completed, ...]) -> tuple[Completed, ...]:
    # ast-grep exit 1 with parseable JSON stdout means no match and maps to EMPTY; exit 1
    # with non-JSON stdout is a tool fault and must remain FAILED.
    return tuple(
        msgspec.structs.replace(done, status=RailStatus.EMPTY)
        if done.returncode == 1 and _safe_decode(AST_MATCHES, done.stdout or b"[]", None) is not None
        else done
        for done in completeds
    )


def _report(
    settings: AssaySettings, scope: ArtifactScope, verb: str, pattern: str, completeds: tuple[Completed, ...], rows: tuple[Match, ...], listing: str
) -> Report:
    # Rows promote an EMPTY base status to OK; malformed query captures keep their defect
    # rows; full match cardinality rides Artifact.lines rather than notes to avoid bloat.
    base = fold(Claim.CODE, verb, completeds)
    status = RailStatus.OK if rows and base.status is RailStatus.EMPTY else base.status
    done = Completed(("code", verb, pattern), 1 if status is RailStatus.FAILED else 0, status=status)
    final_rows = rows or base.results
    return msgspec.structs.replace(
        fold(Claim.CODE, verb, (done,)), artifacts=(_artifact(scope, verb, listing, settings),) if listing else (), results=final_rows
    )


def _relevance(pattern: str, text: str) -> int:
    return max(1, min(100, len(pattern) * 100 // max(len(text), 1)))


def _ag_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    matches = tuple(m for done in completeds for m in _safe_decode(AST_MATCHES, done.stdout or b"[]", ()))
    listing = "\n".join(f"{m.file}:{m.range.start.line + 1}: {m.text}" + (f" => {m.replacement}" if m.replacement else "") for m in matches)
    rows = tuple(
        Match(
            id=f"{m.file}:{m.range.start.line + 1}",
            kind=ArtifactKind.CODE,
            text=(f"{m.text} => {m.replacement}" if m.replacement else m.text)[:_TEXT_CAP],
            line=m.range.start.line + 1,
            score=_relevance(pattern, m.text),
        )
        for m in matches[:cap]
    )
    return rows, listing


def _ts_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    captures = tuple(c for done in completeds for c in _safe_decode(CAPTURES, done.stdout or b"[]", ()))
    listing = "\n".join(f"{c.file}:{c.line}:{c.column}: @{c.name}#{c.ordinal}/{c.pattern} {c.text}" for c in captures)
    rows = tuple(
        Match(
            id=f"{c.file}:{c.line}:{c.column}:{c.name}:{c.ordinal}",
            kind=ArtifactKind.CODE,
            text=f"{'parse-error ' if c.parse_error else ''}@{c.name} {c.text}"[:_TEXT_CAP],
            line=c.line,
            score=_relevance(pattern, c.text),
            severity="failed" if c.parse_error else None,
        )
        for c in captures[:cap]
    )
    return rows, listing


def _content_splice(tool: Tool, params: CodeParams, root: Path) -> Tool:
    # --glob narrows to language suffixes; missing paths drop to the default target, matching _targets behavior in ast-grep splices.
    globs = tuple(arg for suffix in (params.language.suffixes if params.language is not None else ()) for arg in ("--glob", f"*{suffix}"))
    return msgspec.structs.replace(tool, command=(*tool.command, *globs, "-e", params.pattern, "--", *_targets(params.paths, root)))


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


def _rg_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    # Only ripgrep NDJSON events with kind "match" carry hit data; begin, end, and summary
    # events are discarded.
    matches = tuple(
        e.data
        for done in completeds
        for line in (done.stdout or b"").splitlines()
        if line
        for e in (_safe_decode(RG_EVENT, line, None),)
        if e is not None and e.kind == "match"
    )
    listing = "\n".join(f"{d.path.text}:{d.line_number}: {d.lines.text.rstrip()}" for d in matches)
    rows = tuple(
        Match(
            id=f"{d.path.text}:{d.line_number}",
            kind=ArtifactKind.CODE,
            text=d.lines.text.rstrip()[:_TEXT_CAP],
            line=d.line_number,
            score=_relevance(pattern, d.lines.text.rstrip()),
        )
        for d in matches[:cap]
    )
    return rows, listing


def _apply_report(verb: str, pattern: str, completeds: tuple[Completed, ...]) -> Report:
    # ast-grep apply writes its summary to stderr and emits no JSON results; stdout is
    # consumed as a fallback when stderr is absent.
    notes = tuple((d.stderr or d.stdout).decode(errors="replace").strip() for d in completeds if d.stderr or d.stdout) or ("no changes",)
    failed = tuple(d for d in completeds if d.status.severity > RailStatus.OK.severity)
    status = failed[0].status if failed else (RailStatus.OK if completeds else RailStatus.EMPTY)
    done = Completed(("code", verb, pattern), 1 if failed else 0, status=status, notes=notes)
    return fold(Claim.CODE, verb, (done,))


# --- [COMPOSITION] ----------------------------------------------------------------------


def search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Dispatch a structural or content search over language-routed files.

    Patterns containing a metavariable (``$NAME`` form) route to ast-grep structural
    search; all other patterns route to ripgrep content search.

    Returns:
        Ok carrying the folded Report with match rows and an Artifact listing, or
        Error carrying a Fault on routing, spawn, or lease failure.
    """
    match bool(_METAVAR.search(params.pattern)):
        case True:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_search_splice(params, Path(str(settings.root)))).map(
                lambda done: _report(
                    settings, scope, "search", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results, params.pattern)
                )
            )
        case False:  # pragma: no cover — exhaustive match(bool); sysmon arc to implicit exit unreachable
            return _content_search(settings, scope, params)


def _content_search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    # A synthetic Routed with no language files satisfies run_check's spawn shape; ripgrep
    # is grammar-blind so no actual file routing is needed before the check is dispatched.
    match next((t for t in select(Claim.CODE) if t.mode is Mode.CONTENT), None):
        case None:
            return Error(Fault(("code", "search"), status=RailStatus.FAULTED, message="no ripgrep content catalog row"))
        case Tool() as tool:  # pragma: no cover — exhaustive match(Tool|None); sysmon arc to implicit exit unreachable
            check = Check(tool=_content_splice(tool, params, Path(str(settings.root))), paths=tuple(params.paths or _DEFAULT_TARGET))
            routed = Routed(language=tool.language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=scope, routed=routed).map(lambda done: _content_report(settings, scope, params, done))


def _content_report(settings: AssaySettings, scope: ArtifactScope, params: CodeParams, done: Completed) -> Report:
    rows, listing = _rg_rows((done,), params.max_results, params.pattern)
    status, notes = _rg_status(done.returncode, (done.stderr or b"").decode(errors="replace").strip(), has_rows=bool(rows))
    synthetic = Completed(("code", "search", params.pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=notes)
    return msgspec.structs.replace(
        fold(Claim.CODE, "search", (synthetic,)), artifacts=(_artifact(scope, "search", listing, settings),) if listing else (), results=rows
    )


def rewrite(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Preview or apply an ast-grep structural rewrite over language-routed files.

    When ``params.apply`` is true the rewrite is applied in-place under a write lease;
    when false a dry-run JSON diff is produced without filesystem mutation.

    Returns:
        Ok carrying a folded Report with diff rows and an Artifact listing (preview), or
        the apply summary with no match rows (apply mode). Error on routing, spawn, or
        lease fault.
    """
    root = Path(str(settings.root))
    match params.apply:
        case True:
            return leased(
                "code",
                lambda _held: _fan(settings, scope, params, mode=Mode.WRITE, splice=_rewrite_splice(params, root, apply=True)).map(
                    lambda done: _apply_report("rewrite", params.pattern, _ag_normalize(done))
                ),
                settings=settings,
                run_id=settings.run_id,
                project="code",
            )
        case False:  # pragma: no cover — exhaustive match(bool); sysmon arc to implicit exit unreachable
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_rewrite_splice(params, root, apply=False)).map(
                lambda done: _report(
                    settings, scope, "rewrite", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results, params.pattern)
                )
            )


def query(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Run a tree-sitter query in-process over grammar-backed language files.

    Queries execute via INPROC thunks; capture results are encoded into
    Completed.stdout to satisfy the fan-out contract. Files are prefiltered by
    literal byte needles when the query admits it.

    Returns:
        Ok carrying the folded Report with capture rows and an Artifact listing, or
        Error carrying a Fault on routing or spawn failure.
    """
    return _fan(settings, scope, params, mode=Mode.QUERY, splice=_query_splice(params, Path(str(settings.root)))).map(
        lambda done: _report(settings, scope, "query", params.pattern, done, *_ts_rows(done, params.max_results, params.pattern))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeParams", "query", "rewrite", "search"]

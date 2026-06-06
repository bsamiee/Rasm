"""Run structural search, rewrite, content search, and tree-sitter query rails."""

from dataclasses import dataclass, replace
from functools import reduce
from hashlib import sha256
from pathlib import Path
import re
from typing import override, TYPE_CHECKING

from expression import Error, Result  # noqa: TC002  # Result unconditional: @checked's beartype resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
from tree_sitter import Language as TSLanguage, Parser as TSParser, Query as TSQuery, QueryCursor
import tree_sitter_python
import tree_sitter_typescript

from tools.assay.composition.catalog import AST_MATCHES, Capture, CAPTURE_ENCODER, CAPTURES, RG_EVENT, select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased, run_check
from tools.assay.core.model import (
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


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class CodeParams(BaseParams):
    """Parameters shared by code verbs."""

    pattern: str = ""
    rewrite: str = ""
    apply: bool = False
    max_results: int = 1000

    @override
    def bound(self, verb: str) -> CodeParams | Fault:
        """Project the leading positional token(s) into the pattern (and rewrite) slots a code verb owns.

        Code verbs are variadic over trailing paths, so positionals after the owned slots stay as search
        targets; a flag-supplied pattern is never overwritten. The projected pattern is then validated
        non-blank — a blank pattern (no positional, an explicit ``""``, or whitespace-only) is a parse fault,
        closing the ``code search`` match-everything degeneracy.

        Returns:
            Bound params with pattern/rewrite filled from positionals, or a parse fault for a missing pattern.
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


# --- [CONSTANTS] ------------------------------------------------------------------------

# Query catalog rows exist only for grammars listed here.
_GRAMMARS: dict[Language, Callable[[], object]] = {
    Language.PYTHON: tree_sitter_python.language,
    Language.TYPESCRIPT: tree_sitter_typescript.language_typescript,
}
_TEXT_CAP: int = 320
_DEFAULT_TARGET: tuple[str, ...] = (".",)
_METAVAR: re.Pattern[str] = re.compile(r"\$[A-Z_$]")


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
    # No routed files means no check; splice injects argv or the in-process thunk.
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
    # ast-grep aborts on missing explicit targets and ripgrep walks cwd on an empty tail; drop stale paths, then fall back to the worktree default.
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
    return lambda tool, routed: msgspec.structs.replace(tool, thunk=_ts_thunk(params.pattern, routed.language, root))


def _top_level_patterns(query_src: str) -> int:
    # Mask strings and strip comments so literal parentheses do not affect top-level pattern counting.
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


def _eq_needles(query_src: str) -> frozenset[bytes] | None:
    # Single-pattern #eq?-only queries can prefilter files by literal needles with zero false negatives.
    predicates = frozenset(re.findall(r"#([a-z][a-z-]*)\?", query_src))
    match (predicates, _top_level_patterns(query_src)):
        case (preds, 1) if preds == frozenset({"eq"}):
            literals = tuple(re.findall(r'#eq\?\s+@\S+\s+"([^"]*)"', query_src))
            return frozenset(lit.encode() for lit in literals) if literals and not any("\\" in lit for lit in literals) else None
        case _:
            return None


def _ts_thunk(query_src: str, language: Language, root: Path) -> InprocThunk:
    # Captures ride Completed.stdout like subprocess output; literal needles reduce GIL-bound parsing.
    needles = _eq_needles(query_src)

    def run(check: Check) -> Completed:
        ts_lang = TSLanguage(_GRAMMARS[language]())
        parser = TSParser(ts_lang)
        query = TSQuery(ts_lang, query_src)
        captures = tuple(
            Capture(name=name, text=_node_text(node)[:_TEXT_CAP], file=rel, line=node.start_point.row + 1)
            for rel in check.paths
            for src in (_read(root / rel),)
            if src is not None and (needles is None or all(needle in src for needle in needles))
            for name, nodes in QueryCursor(query).captures(parser.parse(src).root_node).items()
            for node in nodes
        )
        return receipt(("tree-sitter", "query", language, *check.paths), 0, stdout=CAPTURE_ENCODER.encode(captures))

    return run


def _node_text(node: Node) -> str:
    raw = node.text
    return raw.decode(errors="replace") if raw is not None else ""


def _read(path: Path) -> bytes | None:
    try:
        return path.read_bytes()
    except OSError:
        return None


def _artifact(settings: AssaySettings, verb: str, content: str) -> Artifact:
    # run_id prevents listing clobber; content hash lets delta distinguish changed match sets.
    raw = content.encode()
    digest = sha256(raw).hexdigest()[:12]
    path = Path(str(settings.artifact(ArtifactKind.CODE, verb, settings.run_id, "matches.txt")))
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(raw)
    return Artifact(id=digest, kind=ArtifactKind.CODE, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _safe_decode[T, E](decoder: msgspec.json.Decoder[T], raw: bytes, empty: E) -> T | E:
    # A tool that spawned but emitted non-JSON degrades to the empty projection instead of raising a DecodeError across the fan.
    try:
        return decoder.decode(raw)
    except msgspec.MsgspecError:
        return empty


def _ag_normalize(completeds: tuple[Completed, ...]) -> tuple[Completed, ...]:
    # ast-grep exit 1 with parseable JSON means no match (-> EMPTY); rc==1 with garbage stdout is a tool fault that must stay FAILED.
    def _norm(done: Completed) -> Completed:
        match (done.returncode, _parses(done.stdout)):
            case (1, True):
                return msgspec.structs.replace(done, status=RailStatus.EMPTY)
            case _:
                return done

    return tuple(_norm(d) for d in completeds)


def _parses(stdout: bytes) -> bool:
    try:
        _ = AST_MATCHES.decode(stdout or b"[]")
    except msgspec.MsgspecError:
        return False
    return True


def _report(settings: AssaySettings, verb: str, pattern: str, completeds: tuple[Completed, ...], rows: tuple[Match, ...], listing: str) -> Report:
    # Hits promote EMPTY to OK; no-hit results stay terse, while malformed queries keep defect rows.
    # Full match cardinality rides Artifact.lines, not notes.
    base = fold(Claim.CODE, verb, completeds)
    status = RailStatus.OK if rows and base.status is RailStatus.EMPTY else base.status
    done = Completed(("code", verb, pattern), 1 if status is RailStatus.FAILED else 0, status=status)
    final_rows = rows or base.results
    return msgspec.structs.replace(
        fold(Claim.CODE, verb, (done,)), artifacts=(_artifact(settings, verb, listing),) if listing else (), results=final_rows
    )


def _relevance(pattern: str, text: str) -> int:
    # Shorter matched text relative to the pattern ranks higher, clamped to [1, 100].
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
    listing = "\n".join(f"{c.file}:{c.line}: @{c.name} {c.text}" for c in captures)
    rows = tuple(
        Match(id=f"{c.file}:{c.line}", kind=ArtifactKind.CODE, text=f"@{c.name} {c.text}"[:_TEXT_CAP], line=c.line, score=_relevance(pattern, c.text))
        for c in captures[:cap]
    )
    return rows, listing


def _content_splice(tool: Tool, params: CodeParams, root: Path) -> Tool:
    # ripgrep self-walks targets; --language narrows through suffix globs. Missing paths drop to the default target (parity with ast-grep _targets).
    globs = tuple(arg for suffix in (params.language.suffixes if params.language is not None else ()) for arg in ("--glob", f"*{suffix}"))
    return msgspec.structs.replace(tool, command=(*tool.command, *globs, "-e", params.pattern, "--", *_targets(params.paths, root)))


def _rg_status(returncode: int, stderr: str, *, has_rows: bool) -> tuple[RailStatus, tuple[str, ...]]:
    # ripgrep exit 2 is overloaded, so hit presence decides partial success versus real failure.
    match (returncode, has_rows):
        case (_, True):
            warn = f"; ripgrep warning: {stderr[:200]}" if returncode not in {0, 1} and stderr else ""
            return RailStatus.OK, (f"content match{warn}",)
        case (0 | 1, False):
            return RailStatus.EMPTY, ("no content matches",)
        case _:
            return RailStatus.FAILED, (f"ripgrep failed (exit {returncode}): {stderr[:400] or 'error'}",)


def _rg_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    # Only ripgrep NDJSON match events carry hits; a non-JSON line degrades to None rather than raising. Listing unbounded, results capped.
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
    # ast-grep apply emits its summary to stderr and no JSON results.
    notes = tuple((d.stderr or d.stdout).decode(errors="replace").strip() for d in completeds if d.stderr or d.stdout) or ("no changes",)
    failed = tuple(d for d in completeds if d.status.severity > RailStatus.OK.severity)
    status = failed[0].status if failed else (RailStatus.OK if completeds else RailStatus.EMPTY)
    done = Completed(("code", verb, pattern), 1 if failed else 0, status=status, notes=notes)
    return fold(Claim.CODE, verb, (done,))


# --- [COMPOSITION] ----------------------------------------------------------------------


def search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Search code structurally or by literal content.

    Returns:
        Search report, or routing/spawn fault.
    """
    match bool(_METAVAR.search(params.pattern)):
        case True:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_search_splice(params, Path(str(settings.root)))).map(
                lambda done: _report(settings, "search", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results, params.pattern))
            )
        case False:
            return _content_search(settings, scope, params)


def _content_search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    # Grammar-blind content search runs one check; a synthetic Routed satisfies the spawn shape.
    match next((t for t in select(Claim.CODE) if t.mode is Mode.CONTENT), None):
        case None:
            return Error(Fault(("code", "search"), status=RailStatus.FAULTED, message="no ripgrep content catalog row"))
        case Tool() as tool:
            check = Check(tool=_content_splice(tool, params, Path(str(settings.root))), paths=tuple(params.paths or _DEFAULT_TARGET))
            routed = Routed(language=tool.language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=scope, routed=routed).map(lambda done: _content_report(settings, params, done))


def _content_report(settings: AssaySettings, params: CodeParams, done: Completed) -> Report:
    rows, listing = _rg_rows((done,), params.max_results, params.pattern)
    status, notes = _rg_status(done.returncode, (done.stderr or b"").decode(errors="replace").strip(), has_rows=bool(rows))
    synthetic = Completed(("code", "search", params.pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=notes)
    return msgspec.structs.replace(
        fold(Claim.CODE, "search", (synthetic,)), artifacts=(_artifact(settings, "search", listing),) if listing else (), results=rows
    )


def rewrite(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Preview or apply an ast-grep rewrite.

    Returns:
        Rewrite preview or apply report, or routing/spawn/lease fault.
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
        case False:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_rewrite_splice(params, root, apply=False)).map(
                lambda done: _report(settings, "rewrite", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results, params.pattern))
            )


def query(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """Run a tree-sitter query over grammar-backed languages.

    Returns:
        Query report, or routing/spawn fault.
    """
    return _fan(settings, scope, params, mode=Mode.QUERY, splice=_query_splice(params, Path(str(settings.root)))).map(
        lambda done: _report(settings, "query", params.pattern, done, *_ts_rows(done, params.max_results, params.pattern))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeParams", "query", "rewrite", "search"]

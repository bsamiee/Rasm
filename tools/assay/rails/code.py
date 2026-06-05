"""Code arm: structural search/rewrite (ast-grep) + AST query (tree-sitter), one polyglot fold over ``Claim.CODE``.

``query`` runs tree-sitter in-process (``Runner.INPROC``) folded through the SAME ``Completed`` -> ``fold``
rail as every subprocess tool — the thunk encodes captures onto ``Completed.stdout`` and the rail decodes
them back. The supported language set is data-driven off ``select(Claim.CODE)``, so a new grammar's rows
auto-extend the fan. Only ``rewrite --apply`` takes the ``code`` lease so concurrent in-place edits never race.
"""

from dataclasses import dataclass
from functools import reduce
from hashlib import sha256
from pathlib import Path
import re
from typing import TYPE_CHECKING

from expression import Error, Result  # noqa: TC002  # Result unconditional: @checked's beartype resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
from tree_sitter import Language as TSLanguage, Parser as TSParser, Query as TSQuery, QueryCursor
import tree_sitter_python
import tree_sitter_typescript

from tools.assay.composition.catalog import (  # intra-package import; tools.assay is the package root
    AST_MATCHES,
    Capture,
    CAPTURE_ENCODER,
    CAPTURES,
    RG_EVENT,
    select,
)
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased, run_check  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
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
from tools.assay.core.routing import route, Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression.collections import Block
    from tree_sitter import Node

    from tools.assay.core.model import InprocThunk  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class CodeParams(BaseParams):
    """code arm CLI params shared across ``search``/``rewrite``/``query``: a structural ``pattern`` plus rewrite/apply.

    ``pattern`` is the ast-grep pattern (``search``/``rewrite``) or the tree-sitter S-expression query
    (``query``); ``apply`` promotes a rewrite preview to an in-place ``Mode.WRITE`` apply under the ``code``
    lease. ``language=None`` fans every grammar-backed language and ``paths=()`` enumerates the whole worktree.
    """

    pattern: str = ""
    rewrite: str = ""
    apply: bool = False
    max_results: int = 1000


# --- [CONSTANTS] ------------------------------------------------------------------------

# tree-sitter grammar capsule factories per Language; the QUERY catalog rows exist only for these, so
# _query_splice is reached only for a key present here — a new grammar is one row + one entry.
_GRAMMARS: dict[Language, Callable[[], object]] = {
    Language.PYTHON: tree_sitter_python.language,
    Language.TYPESCRIPT: tree_sitter_typescript.language_typescript,
}
_TEXT_CAP: int = 320  # per-Match text bound, mirroring api._matches
_DEFAULT_TARGET: tuple[str, ...] = (".",)  # empty paths → enumerate the whole worktree, suffix-filtered per language
_METAVAR: re.Pattern[str] = re.compile(r"\$[A-Z_$]")  # ast-grep metavariable sigil ($VAR / $$$ / $$VAR): present → structural, absent → rg content


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None) -> tuple[Language, ...]:
    # Data-driven off select(Claim.CODE): a grammar-less language carries no code row, so the polyglot fan never wastes a route on it.
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.CODE)}, key=lambda member: member.value))
        case language:
            return (language,)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    # Unlike the gate rails, empty paths resolve the whole worktree (code is a search surface, not a changed-files gate).
    return sequence(block.of_seq(route(language, paths or _DEFAULT_TARGET) for language in languages))


def _checks(routed: Routed, mode: Mode, splice: Callable[[Tool, Routed], Tool]) -> tuple[Check, ...]:
    # No routed files → no check, so the fan never scans the whole tree as a fallback; splice injects the argv or thunk.
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
    # Shared route -> fan_out spine; a routing/spawn/timeout Fault short-circuits, a non-zero exit rode the success channel as a Completed.
    return _routed(_languages(params.language), params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode, splice=splice)))).map(
            tuple
        )
    )


def _targets(paths: tuple[str, ...], root: Path) -> tuple[str, ...]:
    # ast-grep ABORTS the whole run if ANY explicit target is missing, so drop missing paths to keep a partially-bad request resilient.
    # All-missing is unreachable: _checks gates on routed.files, empty when every path resolved to nothing.
    match paths:
        case ():
            return _DEFAULT_TARGET
        case _:
            return tuple(p for p in paths if (root / p).exists())


def _search_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Tool]:
    # Targets ride the command body (Input.NONE), never the fd-enumerated file list, to avoid ARG_MAX on a large monorepo.
    # --no-ignore hidden unhides tracked dot-dirs so search covers the SAME tree as query; gitignored dirs stay excluded.
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
    # Blank string spans and strip ; comments first so a literal paren never miscounts the pattern split (1 = safe-to-narrow).
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
    # tree-sitter is GIL-bound, so the whole-tree lever is parsing fewer files: a single-pattern #eq?-only query can match a
    # file only if it contains every literal verbatim, so the literals pre-filter with zero false negatives. Narrowing is
    # suppressed (None → parse all) for multi-pattern queries, non-#eq? predicates, or escaped literals (raw text ≠ node text).
    predicates = frozenset(re.findall(r"#([a-z][a-z-]*)\?", query_src))
    match (predicates, _top_level_patterns(query_src)):
        case (preds, 1) if preds == frozenset({"eq"}):
            literals = tuple(re.findall(r'#eq\?\s+@\S+\s+"([^"]*)"', query_src))
            return frozenset(lit.encode() for lit in literals) if literals and not any("\\" in lit for lit in literals) else None
        case _:
            return None


def _ts_thunk(query_src: str, language: Language, root: Path) -> InprocThunk:
    # Captures encode onto Completed.stdout, so the in-process result rides the IDENTICAL Completed wire a subprocess tool would.
    # needles is computed once outside the per-file loop so the GIL-bound parse runs only on files that can possibly match.
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
    raw = node.text  # Node.text is bytes | None; an absent source folds to ""
    return raw.decode(errors="replace") if raw is not None else ""


def _read(path: Path) -> bytes | None:
    # Thread-local boundary: an unreadable path folds to None rather than raising.
    try:
        return path.read_bytes()
    except OSError:
        return None


def _artifact(settings: AssaySettings, verb: str, content: str) -> Artifact:
    # run_id segments the path so concurrent/successive queries never clobber the same matches.txt and
    # delta history replay retrieves the exact listing for any run_id. Content hash makes the id
    # content-aware: two queries returning the same set share an id; different sets differ, so delta
    # can distinguish them even within the same verb bucket.
    raw = content.encode()
    digest = sha256(raw).hexdigest()[:12]
    path = Path(str(settings.artifact(ArtifactKind.CODE, verb, settings.run_id, "matches.txt")))  # code artifact tree is local-fs; UPath → Path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(raw)
    return Artifact(id=digest, kind=ArtifactKind.CODE, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _ag_normalize(completeds: tuple[Completed, ...]) -> tuple[Completed, ...]:
    # ast-grep follows the grep convention (exit 1 = no match), so pin it to EMPTY before the fold derives status.
    # Scoped to ast-grep slices ONLY — a tree-sitter query's exit 1 is a real thunk fault (bad S-expr) and must stay FAILED.
    return tuple(msgspec.structs.replace(d, status=RailStatus.EMPTY) if d.returncode == 1 else d for d in completeds)


def _report(settings: AssaySettings, verb: str, pattern: str, completeds: tuple[Completed, ...], rows: tuple[Match, ...], listing: str) -> Report:
    # A clean fold with hits promotes EMPTY to OK; the full listing rides an Artifact only when non-empty so a zero-hit query is terse.
    # Defect rows fold projected from FAILED Completed entries (W1) are preserved as the `results` when no matching rows exist —
    # a malformed query surfaces its QueryError/stderr tail instead of an empty results set byte-identical to a valid zero-match.
    # The match COUNT is typed on Artifact.lines (total untruncated matches); the prose note is dropped so machine data
    # never rides a string-parsed note. Counts.ok=1 reflects the single synthetic Completed (the slice count); Artifact.lines
    # is the match cardinality an agent reads directly.
    base = fold(Claim.CODE, verb, completeds)
    status = RailStatus.OK if rows and base.status is RailStatus.EMPTY else base.status
    done = Completed(("code", verb, pattern), 1 if status is RailStatus.FAILED else 0, status=status)
    final_rows = rows or base.results  # preserve fold-projected defect rows when no match rows exist
    return msgspec.structs.replace(
        fold(Claim.CODE, verb, (done,)), artifacts=(_artifact(settings, verb, listing),) if listing else (), results=final_rows
    )


def _relevance(pattern: str, text: str) -> int:
    # Ratio of pattern length to matched text length, clamped to [1, 100]. A tighter match (match text
    # close to the pattern in length) scores higher; a very broad match (long line, short pattern) scores
    # lower. The floor of 1 preserves ordering over a strict zero baseline.
    return max(1, min(100, len(pattern) * 100 // max(len(text), 1)))


def _ag_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    # The listing is the unbounded match set the Artifact retains while Report.results stays bounded by cap.
    matches = tuple(m for done in completeds for m in AST_MATCHES.decode(done.stdout or b"[]"))
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
    captures = tuple(c for done in completeds for c in CAPTURES.decode(done.stdout or b"[]"))
    listing = "\n".join(f"{c.file}:{c.line}: @{c.name} {c.text}" for c in captures)
    rows = tuple(
        Match(id=f"{c.file}:{c.line}", kind=ArtifactKind.CODE, text=f"@{c.name} {c.text}"[:_TEXT_CAP], line=c.line, score=_relevance(pattern, c.text))
        for c in captures[:cap]
    )
    return rows, listing


def _content_splice(tool: Tool, params: CodeParams) -> Tool:
    # Grammar-blind: ripgrep self-walks the targets over the SAME tree the ast-grep search arm covers; --language refines via suffix globs.
    globs = tuple(arg for suffix in (params.language.suffixes if params.language is not None else ()) for arg in ("--glob", f"*{suffix}"))
    return msgspec.structs.replace(tool, command=(*tool.command, *globs, "-e", params.pattern, "--", *(params.paths or _DEFAULT_TARGET)))


def _rg_status(returncode: int, stderr: str, *, has_rows: bool) -> tuple[RailStatus, tuple[str, ...]]:
    # ripgrep exit 2 is overloaded (bad -P regex = total failure AND missing target among valid = partial success), so hit-presence
    # decides: ANY exit WITH matches → OK (a non-clean exit rides a warning note); clean no-match → EMPTY; non-zero NO matches → FAILED.
    match (returncode, has_rows):
        case (_, True):
            warn = f"; ripgrep warning: {stderr[:200]}" if returncode not in {0, 1} and stderr else ""
            return RailStatus.OK, (f"content match{warn}",)
        case (0 | 1, False):
            return RailStatus.EMPTY, ("no content matches",)
        case _:
            return RailStatus.FAILED, (f"ripgrep failed (exit {returncode}): {stderr[:400] or 'error'}",)


def _rg_rows(completeds: tuple[Completed, ...], cap: int, pattern: str) -> tuple[tuple[Match, ...], str]:
    # stdout is NDJSON; only kind == "match" events carry a hit. The listing rides the Artifact while results stays bounded by cap.
    matches = tuple(
        e.data for done in completeds for line in (done.stdout or b"").splitlines() if line for e in (RG_EVENT.decode(line),) if e.kind == "match"
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
    # ast-grep run -U streams the "Applied N changes" summary to STDERR, not stdout, and the apply produces no JSON for results.
    notes = tuple((d.stderr or d.stdout).decode(errors="replace").strip() for d in completeds if d.stderr or d.stdout) or ("no changes",)
    failed = tuple(d for d in completeds if d.status.severity > RailStatus.OK.severity)
    status = failed[0].status if failed else (RailStatus.OK if completeds else RailStatus.EMPTY)
    done = Completed(("code", verb, pattern), 1 if failed else 0, status=status, notes=notes)
    return fold(Claim.CODE, verb, (done,))


# --- [COMPOSITION] ----------------------------------------------------------------------


def search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """``code search``: ONE verb, two modalities discriminated by the ast-grep ``$``-metavar.

    A pattern carrying a ``$``-metavar routes to the grammar-aware ast-grep structural search; a literal
    pattern routes to the grammar-blind ripgrep content search over the SAME tree. The metavar IS the
    discriminator, so ``query`` and ``rewrite`` stay distinct verbs with no S-expression auto-detection.
    """
    match bool(_METAVAR.search(params.pattern)):
        case True:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_search_splice(params, Path(str(settings.root)))).map(
                lambda done: _report(settings, "search", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results, params.pattern))
            )
        case False:
            return _content_search(settings, scope, params)


def _content_search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    # ONE grammar-blind check (no per-language fan): targets ride tool.command and a synthetic Routed satisfies the per-spawn shape.
    match next((t for t in select(Claim.CODE) if t.mode is Mode.CONTENT), None):
        case None:
            return Error(Fault(("code", "search"), status=RailStatus.FAULTED, message="no ripgrep content catalog row"))
        case Tool() as tool:
            check = Check(tool=_content_splice(tool, params), paths=tuple(params.paths or _DEFAULT_TARGET))
            routed = Routed(language=tool.language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=scope, routed=routed).map(lambda done: _content_report(settings, params, done))


def _content_report(settings: AssaySettings, params: CodeParams, done: Completed) -> Report:
    # Distinct from _report: ripgrep's overloaded exit codes need the hit-aware interpretation _rg_status owns.
    rows, listing = _rg_rows((done,), params.max_results, params.pattern)
    status, notes = _rg_status(done.returncode, (done.stderr or b"").decode(errors="replace").strip(), has_rows=bool(rows))
    synthetic = Completed(("code", "search", params.pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=notes)
    return msgspec.structs.replace(
        fold(Claim.CODE, "search", (synthetic,)), artifacts=(_artifact(settings, "search", listing),) if listing else (), results=rows
    )


def rewrite(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """``code rewrite``: ast-grep rewrite preview, or in-place apply under the ``code`` lease.

    Preview (``--apply`` absent) folds ``--json=compact`` matches into ``Match`` rows. Apply (``--apply``)
    runs ``-U`` under the exclusive ``code`` lease — a busy lease short-circuits to ``Fault(BUSY)`` so
    concurrent in-place edits never race a file.
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
    """``code query``: tree-sitter AST query via ``Runner.INPROC``, fanning every grammar-backed language."""
    return _fan(settings, scope, params, mode=Mode.QUERY, splice=_query_splice(params, Path(str(settings.root)))).map(
        lambda done: _report(settings, "query", params.pattern, done, *_ts_rows(done, params.max_results, params.pattern))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeParams", "query", "rewrite", "search"]

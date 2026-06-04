"""Code arm: structural search/rewrite (ast-grep) + AST query (tree-sitter), one polyglot fold over ``Claim.CODE``.

``search``/``rewrite`` splice an ast-grep ``run`` argv (``Runner.PNPM``); ``query`` runs tree-sitter
in-process (``Runner.INPROC``) folded through the SAME ``Completed`` → ``fold`` → ``_emit`` rail as every
subprocess tool — the thunk encodes its captures onto ``Completed.stdout`` and the rail decodes them back.
All three emit ranked ``Match`` rows on ``Report.results`` plus a listing/diff ``Artifact``; none mints a
``Detail`` variant. Polyglot like ``rails/static.py`` (``language=None`` fans every grammar-backed language),
data-driven off the catalog: ``_languages`` derives the supported set from ``select(Claim.CODE)`` so a new
language's rows auto-extend the fan, and a language with no ``CODE`` row contributes an empty fan (honest
``EMPTY``), exactly as ``DOCS`` does for the static rail. The rail never builds argv, spawns, or leases
directly — it routes, selects ``Tool`` rows, splices the per-verb args/thunk, hands ``Check``s to the
Engine, and folds; only ``rewrite --apply`` takes the ``code`` lease so concurrent in-place edits never race.
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
    """code arm CLI params: a structural ``pattern`` plus rewrite/apply, shared across ``search``/``rewrite``/``query``.

    ``pattern`` is the ast-grep pattern (``search``/``rewrite``) or the tree-sitter S-expression query
    (``query``); ``rewrite`` is the ast-grep replacement template; ``apply`` promotes a rewrite preview to
    an in-place ``Mode.WRITE`` apply under the ``code`` lease. Inherits ``paths``/``language`` from
    ``BaseParams`` (so it flattens onto the CLI via the inherited ``@Parameter(name="*")``);
    ``language=None`` fans every grammar-backed language and ``paths=()`` enumerates the whole worktree.
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
    """The languages to fan: one explicit member, or every language the ``CODE`` catalog rows back.

    Derived from ``select(Claim.CODE)`` so the supported set is data-driven — adding a grammar's rows
    auto-extends the fan, and the polyglot request never routes a language (csharp/bash/sql/docs) that
    carries no ``code`` row and would only waste a route.
    """
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.CODE)}, key=lambda member: member.value))
        case language:
            return (language,)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    """Route every requested language through one ``route`` each (empty paths → whole-worktree target), sequenced.

    Unlike the gate rails, an empty ``paths`` resolves the whole worktree (``_DEFAULT_TARGET``) rather than
    the git change-set: ``code`` is an agent search/query surface, not a changed-files gate. The first
    routing ``Fault`` (a git/``fd`` spawn failure) short-circuits the whole fan.
    """
    return sequence(block.of_seq(route(language, paths or _DEFAULT_TARGET) for language in languages))


def _checks(routed: Routed, mode: Mode, splice: Callable[[Tool, Routed], Tool]) -> tuple[Check, ...]:
    """Bind every ``mode``-matching ``CODE`` ``Tool`` of the routed language to the routed files, splicing per-verb args/thunk.

    A language with no routed files contributes no check (it owns no file of that language), so the fan
    never scans the whole tree as a fallback; ``splice`` injects the ast-grep argv or the tree-sitter thunk.
    """
    match routed.files:
        case ():
            return ()
        case _:
            return tuple(Check(tool=splice(t, routed), paths=routed.files) for t in select(Claim.CODE, routed.language) if t.mode is mode)


def _dispatch(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode, splice: Callable[[Tool, Routed], Tool]
) -> tuple[Result[Completed, Fault], ...]:
    """Fan one routed language through the Engine under its OWN ``Routed`` context (mirrors ``static._dispatch``)."""
    checks = _checks(routed, mode, splice)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _fan(
    settings: AssaySettings, scope: ArtifactScope, params: CodeParams, *, mode: Mode, splice: Callable[[Tool, Routed], Tool]
) -> Result[tuple[Completed, ...], Fault]:
    """The shared route → fan_out spine: collapse the per-language route rail and per-slot fan into one flat ``Completed`` tuple.

    A routing/spawn/timeout ``Fault`` short-circuits to the registry seam; a non-zero tool exit already
    rode the success channel as a ``Completed`` (the verb report builders read its status/stdout).
    """
    return _routed(_languages(params.language), params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode, splice=splice)))).map(
            tuple
        )
    )


def _targets(paths: tuple[str, ...], root: Path) -> tuple[str, ...]:
    """Resolve the ast-grep self-walk targets: empty user paths → whole worktree; else only the paths that exist.

    ast-grep ABORTS the whole run (emits ``ERROR: <path>`` and returns ``[]``) if ANY explicit target is missing,
    so a single typo'd/stale path would zero out the valid paths' matches. Dropping the missing targets here keeps
    a partially-bad request resilient — ast-grep sees only resolvable paths and still returns their hits — while the
    routing layer's ``route.path_missing`` warning already told the agent which path was skipped. The all-missing
    case is unreachable: ``_checks`` gates on ``routed.files``, which is empty when every path resolved to nothing.
    """
    match paths:
        case ():
            return _DEFAULT_TARGET
        case _:
            return tuple(p for p in paths if (root / p).exists())


def _search_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Tool]:
    """Splice the ast-grep search argv: ``run -p <pat> -l <lang> --json=compact --no-ignore hidden <targets>``.

    ast-grep self-walks the existing explicit targets (or ``.`` for whole-tree), filtered by ``-l`` and respecting
    ``.gitignore`` — a constant-size argv via the ``Input.NONE`` catalog row (the targets ride the command body,
    never the fd-enumerated file list, which would risk ARG_MAX on a large monorepo). ``_targets`` drops missing
    paths so one bad target never aborts the run. ``--no-ignore hidden`` unhides tracked dot-dirs (e.g.
    ``.claude/``) so ``search`` covers the SAME tree as ``query`` — gitignored ``.venv``/``node_modules`` stay excluded.
    """
    targets = _targets(params.paths, root)
    return lambda tool, routed: msgspec.structs.replace(
        tool, command=(*tool.command, "-p", params.pattern, "-l", routed.language.value, "--json=compact", "--no-ignore", "hidden", *targets)
    )


def _rewrite_splice(params: CodeParams, root: Path, *, apply: bool) -> Callable[[Tool, Routed], Tool]:
    """Splice the ast-grep rewrite argv (``-r`` + ``-U`` apply / ``--json=compact`` preview); self-walks the existing ``_targets``."""
    tail = ("-U",) if apply else ("--json=compact",)
    targets = _targets(params.paths, root)
    return lambda tool, routed: msgspec.structs.replace(
        tool,
        command=(*tool.command, "-p", params.pattern, "-r", params.rewrite, "-l", routed.language.value, *tail, "--no-ignore", "hidden", *targets),
    )


def _query_splice(params: CodeParams, root: Path) -> Callable[[Tool, Routed], Tool]:
    """Splice the tree-sitter ``Runner.INPROC`` thunk: a bound closure parsing the routed files under the engine deadline."""
    return lambda tool, routed: msgspec.structs.replace(tool, thunk=_ts_thunk(params.pattern, routed.language, root))


def _top_level_patterns(query_src: str) -> int:
    """Count the top-level S-expression patterns (depth-0 ``(`` groups), masking strings + ``;`` comments first.

    String spans are blanked to ``""`` and ``;`` line comments stripped so a literal paren never miscounts the
    pattern split; a ``reduce`` then threads ``(depth, count)`` over the masked text, bumping ``count`` on each
    ``(`` taken at depth 0. A single balanced query yields ``1`` (the safe-to-narrow case).
    """
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
    """Extract the ``#eq?`` exact-string literals for a candidate-file pre-filter, or ``None`` when narrowing is unsafe.

    tree-sitter is GIL-bound, so the lever for a whole-tree query is parsing fewer files, not threading the
    parse. A single-pattern query whose ONLY predicate is ``#eq?`` can match a file only if that file contains
    every ``#eq?`` literal verbatim, so those literals pre-filter the candidate set with zero false negatives —
    and the bytes are already read, so the skip elides only the expensive parse, not the read. Narrowing is
    suppressed (``None`` → parse all routed files) for a multi-pattern query, any non-``#eq?`` predicate
    (``#match?`` regex, ``#any-of?``, negations), or an escaped literal (where the raw text ≠ the node text),
    keeping the optimization strictly match-preserving.
    """
    predicates = frozenset(re.findall(r"#([a-z][a-z-]*)\?", query_src))
    match (predicates, _top_level_patterns(query_src)):
        case (preds, 1) if preds == frozenset({"eq"}):
            literals = tuple(re.findall(r'#eq\?\s+@\S+\s+"([^"]*)"', query_src))
            return frozenset(lit.encode() for lit in literals) if literals and not any("\\" in lit for lit in literals) else None
        case _:
            return None


def _ts_thunk(query_src: str, language: Language, root: Path) -> InprocThunk:
    """Build the ``Runner.INPROC`` thunk: parse each routed file with ``language``'s grammar and collect ``query_src`` captures.

    The closure captures ``root`` (resolving the root-relative ``Check.paths`` to local files), the
    grammar selector, and the query source, so the engine calls it as a bare ``(Check) -> Completed`` on a
    worker thread under ``fail_after``. Captures encode onto ``Completed.stdout`` as a ``Capture`` array,
    so the in-process result rides the identical ``Completed`` wire a subprocess tool would; the ``code``
    rail decodes it back through the catalog ``CAPTURES`` codec. An unreadable file folds out (``None``).
    ``_eq_needles`` pre-filters candidate files by the query's ``#eq?`` literals (computed once, outside the
    per-file loop) so the GIL-bound parse runs only on files that can possibly match — the whole-tree lever.
    """
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
    """Decode a tree-sitter node's source slice (``Node.text`` is ``bytes | None``; an absent source folds to ``""``)."""
    raw = node.text
    return raw.decode(errors="replace") if raw is not None else ""


def _read(path: Path) -> bytes | None:
    """Read one routed file's bytes at the marked thread-local boundary; an unreadable path folds to ``None`` (no raise)."""
    try:
        return path.read_bytes()
    except OSError:
        return None


def _artifact(settings: AssaySettings, verb: str, content: str) -> Artifact:
    """Write the full match/capture listing under the ``CODE`` scope namespace and project its ``Artifact`` receipt."""
    path = Path(str(settings.artifact(ArtifactKind.CODE, verb, "matches.txt")))  # code artifact tree is local-fs; UPath → Path
    path.parent.mkdir(parents=True, exist_ok=True)
    raw = content.encode()
    path.write_bytes(raw)
    digest = sha256(f"{path}|{verb}".encode()).hexdigest()[:12]
    return Artifact(id=digest, kind=ArtifactKind.CODE, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _ag_normalize(completeds: tuple[Completed, ...]) -> tuple[Completed, ...]:
    """Remap ast-grep's exit 1 (no match, not a fault) to ``EMPTY`` so a zero-hit structural search/rewrite isn't ``FAILED``.

    ast-grep ``run`` follows the grep convention: exit 1 = pattern matched nothing, exit 0 = a hit (and, leniently,
    a malformed pattern it tolerates as zero matches). ``from_returncode(1)=FAILED`` would wrongly mark an honest
    no-match as a defect, so the no-match exit is pinned to ``EMPTY`` per slice before the fold derives status. Scoped
    to the ast-grep paths ONLY — a tree-sitter ``query`` slice's exit 1 is a real thunk fault (bad S-expr) the
    ``_inproc`` boundary surfaced, which must stay ``FAILED``.
    """
    return tuple(msgspec.structs.replace(d, status=RailStatus.EMPTY) if d.returncode == 1 else d for d in completeds)


def _report(settings: AssaySettings, verb: str, pattern: str, completeds: tuple[Completed, ...], rows: tuple[Match, ...], listing: str) -> Report:
    """Fold a synthetic status ``Completed`` into one ``Report``, attaching the ranked ``Match`` rows + full-listing ``Artifact``.

    The fan ``Completed`` statuses fold to detect a genuine tool failure; a clean fold with hits promotes to ``OK``,
    a clean fold with none stays ``EMPTY``. ast-grep slices are ``_ag_normalize``d (exit 1 = no match → ``EMPTY``)
    before they reach here, while a tree-sitter slice's exit is 0 on a clean parse and 1 only on a real thunk fault.
    The full listing rides a scope ``Artifact`` only when non-empty so a zero-hit query is terse.
    """
    base = fold(Claim.CODE, verb, completeds)
    status = RailStatus.OK if rows and base.status is RailStatus.EMPTY else base.status
    note = f"{len(rows)} result(s) across {len(completeds)} routed slice(s)"
    done = Completed(("code", verb, pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=(note,))
    return msgspec.structs.replace(fold(Claim.CODE, verb, (done,)), artifacts=(_artifact(settings, verb, listing),) if listing else (), results=rows)


def _ag_rows(completeds: tuple[Completed, ...], cap: int) -> tuple[tuple[Match, ...], str]:
    """Decode the ast-grep ``run --json=compact`` arrays into ranked ``Match`` rows + the full ``file:line`` listing.

    A rewrite preview carries ``replacement``, surfaced inline as ``text => replacement``; the full listing
    is the unbounded match set the ``Artifact`` retains while ``Report.results`` stays bounded by ``cap``.
    """
    matches = tuple(m for done in completeds for m in AST_MATCHES.decode(done.stdout or b"[]"))
    listing = "\n".join(f"{m.file}:{m.range.start.line + 1}: {m.text}" + (f" => {m.replacement}" if m.replacement else "") for m in matches)
    rows = tuple(
        Match(
            id=f"{m.file}:{m.range.start.line + 1}",
            kind=ArtifactKind.CODE,
            text=(f"{m.text} => {m.replacement}" if m.replacement else m.text)[:_TEXT_CAP],
            line=m.range.start.line + 1,
            score=100,
        )
        for m in matches[:cap]
    )
    return rows, listing


def _ts_rows(completeds: tuple[Completed, ...], cap: int) -> tuple[tuple[Match, ...], str]:
    """Decode the in-process tree-sitter capture arrays into ranked ``Match`` rows + the full ``file:line`` listing."""
    captures = tuple(c for done in completeds for c in CAPTURES.decode(done.stdout or b"[]"))
    listing = "\n".join(f"{c.file}:{c.line}: @{c.name} {c.text}" for c in captures)
    rows = tuple(
        Match(id=f"{c.file}:{c.line}", kind=ArtifactKind.CODE, text=f"@{c.name} {c.text}"[:_TEXT_CAP], line=c.line, score=100) for c in captures[:cap]
    )
    return rows, listing


def _content_splice(tool: Tool, params: CodeParams) -> Tool:
    """Splice the ripgrep content argv: optional ``--glob`` per ``--language`` suffix, then ``-e <pat> -- <targets>``.

    Grammar-blind: ripgrep self-walks the targets (``Input.NONE``) under the base ``--json -U --multiline-dotall -P
    --hidden --glob '!.git'`` row, respecting ``.gitignore`` while searching tracked dot-dirs — the SAME tree the
    ast-grep ``search`` arm covers. ``--language`` refines via the suffix globs of that ``Language`` (reusing
    ``Language.suffixes`` rather than an rg-type-name table); ``language=None`` searches every file.
    """
    globs = tuple(arg for suffix in (params.language.suffixes if params.language is not None else ()) for arg in ("--glob", f"*{suffix}"))
    return msgspec.structs.replace(tool, command=(*tool.command, *globs, "-e", params.pattern, "--", *(params.paths or _DEFAULT_TARGET)))


def _rg_status(returncode: int, stderr: str, *, has_rows: bool) -> tuple[RailStatus, tuple[str, ...]]:
    """Map ripgrep's exit code + hit presence to a resilient ``(status, notes)`` pair.

    ripgrep exits 0 on a hit, 1 on no match, 2 on an operational fault — but exit 2 is overloaded: a bad ``-P``
    regex (total failure) AND a missing/unreadable target among valid ones (partial success). ``from_returncode``
    would fold both 1 and 2 to ``FAILED``, masking a no-match as a defect and a partial-success as a total failure.
    So: ANY exit WITH matches → ``OK`` (a non-clean exit rides a ``ripgrep warning`` note, surfacing the
    skipped/unreadable target without nuking the hits); a clean no-match → ``EMPTY``; a non-zero exit with NO matches →
    ``FAILED`` carrying ripgrep's stderr so an agent sees the bad-regex/no-target diagnostic, never a bare exit code.
    """
    match (returncode, has_rows):
        case (_, True):
            warn = f"; ripgrep warning: {stderr[:200]}" if returncode not in {0, 1} and stderr else ""
            return RailStatus.OK, (f"content match{warn}",)
        case (0 | 1, False):
            return RailStatus.EMPTY, ("no content matches",)
        case _:
            return RailStatus.FAILED, (f"ripgrep failed (exit {returncode}): {stderr[:400] or 'error'}",)


def _rg_rows(completeds: tuple[Completed, ...], cap: int) -> tuple[tuple[Match, ...], str]:
    """Decode the ripgrep ``--json`` NDJSON streams into ranked ``Match`` rows + the full ``file:line`` listing.

    Each ``Completed.stdout`` is line-delimited JSON; only ``kind == "match"`` events carry a hit. The full listing
    rides the ``Artifact`` while ``Report.results`` stays bounded by ``cap`` (mirrors ``_ag_rows``/``_ts_rows``).
    """
    matches = tuple(
        e.data for done in completeds for line in (done.stdout or b"").splitlines() if line for e in (RG_EVENT.decode(line),) if e.kind == "match"
    )
    listing = "\n".join(f"{d.path.text}:{d.line_number}: {d.lines.text.rstrip()}" for d in matches)
    rows = tuple(
        Match(id=f"{d.path.text}:{d.line_number}", kind=ArtifactKind.CODE, text=d.lines.text.rstrip()[:_TEXT_CAP], line=d.line_number, score=100)
        for d in matches[:cap]
    )
    return rows, listing


def _apply_report(verb: str, pattern: str, completeds: tuple[Completed, ...]) -> Report:
    """Fold the ast-grep ``-U`` apply run: ``Applied N changes`` notes + a clean ``OK``/``EMPTY``/``FAILED`` status.

    ast-grep exits 0 on a successful apply, printing ``Applied N changes`` to STDERR (``run -U`` streams
    the summary there, not stdout), so a clean run with any file-set promotes to ``OK`` and a genuine
    non-zero exit keeps its ``FAILED`` status. The per-slice stderr summaries ride ``Report.notes`` since
    the apply produces no JSON to project into ``results``.
    """
    notes = tuple((d.stderr or d.stdout).decode(errors="replace").strip() for d in completeds if d.stderr or d.stdout) or ("no changes",)
    failed = tuple(d for d in completeds if d.status.severity > RailStatus.OK.severity)
    status = failed[0].status if failed else (RailStatus.OK if completeds else RailStatus.EMPTY)
    done = Completed(("code", verb, pattern), 1 if failed else 0, status=status, notes=notes)
    return fold(Claim.CODE, verb, (done,))


# --- [COMPOSITION] ----------------------------------------------------------------------


def search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """``code search --pattern <p> [--language L] [--paths …]``: ONE verb, two modalities on the ``$``-metavar discriminator.

    A pattern carrying an ast-grep ``$``-metavar (``$VAR``/``$$$``/``$$VAR``) routes to the grammar-aware ast-grep
    structural search — the per-language fan, ranked ``Match`` rows + full ``file:line`` listing ``Artifact``. A
    literal pattern (no metavar) routes to the grammar-blind ripgrep content search over the SAME tree (restoring the
    removed ``grep``). Zero flags — the metavar IS the discriminator; ``query`` (tree-sitter) and ``rewrite`` stay
    distinct verbs, so there is no brittle S-expression auto-detection.
    """
    match bool(_METAVAR.search(params.pattern)):
        case True:
            return _fan(settings, scope, params, mode=Mode.CHECK, splice=_search_splice(params, Path(str(settings.root)))).map(
                lambda done: _report(settings, "search", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results))
            )
        case False:
            return _content_search(settings, scope, params)


def _content_search(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """The ripgrep content arm: one ``Runner.DIRECT`` self-walk folded into a resilient ``Report``.

    ONE check (grammar-blind), so no per-language fan — the targets ride ``tool.command`` (``Input.NONE``) and a
    synthetic ``Routed`` satisfies the engine's per-spawn shape. A missing catalog row faults; ripgrep's exit code
    is interpreted by ``_rg_status`` so a partial run (a missing target among valid ones) still surfaces its hits.
    """
    match next((t for t in select(Claim.CODE) if t.mode is Mode.CONTENT), None):
        case None:
            return Error(Fault(("code", "search"), status=RailStatus.FAULTED, message="no ripgrep content catalog row"))
        case Tool() as tool:
            check = Check(tool=_content_splice(tool, params), paths=tuple(params.paths or _DEFAULT_TARGET))
            routed = Routed(language=tool.language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=scope, routed=routed).map(lambda done: _content_report(settings, params, done))


def _content_report(settings: AssaySettings, params: CodeParams, done: Completed) -> Report:
    """Fold one ripgrep ``Completed`` into a ``Report``: ``_rg_status`` decides status/notes from exit code + hit presence.

    Distinct from ``_report`` because ripgrep's exit-code semantics (1 = no-match, 2 = overloaded fault/partial) need
    the hit-aware interpretation ``_rg_status`` owns; the ranked ``Match`` rows + full-listing ``Artifact`` are built identically.
    """
    rows, listing = _rg_rows((done,), params.max_results)
    status, notes = _rg_status(done.returncode, (done.stderr or b"").decode(errors="replace").strip(), has_rows=bool(rows))
    synthetic = Completed(("code", "search", params.pattern), 1 if status is RailStatus.FAILED else 0, status=status, notes=notes)
    return msgspec.structs.replace(
        fold(Claim.CODE, "search", (synthetic,)), artifacts=(_artifact(settings, "search", listing),) if listing else (), results=rows
    )


def rewrite(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """``code rewrite --pattern <p> --rewrite <fix> [--apply]``: ast-grep rewrite preview, or in-place apply under the ``code`` lease.

    Preview (``--apply`` absent) folds the ``--json=compact`` matches into ``Match`` rows surfacing
    ``text => replacement`` and a diff-shaped listing ``Artifact``. Apply (``--apply``) runs ``-U`` under the
    exclusive ``code`` lease — a busy lease short-circuits to ``Fault(BUSY)`` so concurrent in-place edits
    never race a file — and folds the ``Applied N changes`` summaries onto ``Report.notes``.
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
                lambda done: _report(settings, "rewrite", params.pattern, _ag_normalize(done), *_ag_rows(done, params.max_results))
            )


def query(settings: AssaySettings, scope: ArtifactScope, params: CodeParams) -> Result[Report, Fault]:
    """``code query --pattern <s-expr> [--language L] [--paths …]``: tree-sitter AST query via ``Runner.INPROC``.

    Fans every grammar-backed language, splicing a bound in-process thunk that parses each routed file and
    collects the S-expression query's captures; the thunk's ``Completed`` rides the same fold as every
    subprocess tool, so the ranked capture ``Match`` rows and full listing ``Artifact`` are built identically.
    """
    return _fan(settings, scope, params, mode=Mode.QUERY, splice=_query_splice(params, Path(str(settings.root)))).map(
        lambda done: _report(settings, "query", params.pattern, done, *_ts_rows(done, params.max_results))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeParams", "query", "rewrite", "search"]

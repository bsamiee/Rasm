# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence as one tagged-union fact stream. `Evidence` is the single slot/kind evidence union the `assay code` rail consumes — `member` rows carry an official distribution surface a source may later name, `span` rows carry a `tree-sitter` capture's byte/`Point` extent, and `drift` rows carry a cross-language re-mint of a canonical wire-projection name the topology law forbids — never three parallel evidence carriers. `GrammarRegistry` is the reused-`Parser`/compiled-`Query`/resolved-id owner the `tree-sitter` parsing law demands: grammars wrap once into a `Map` row, each probe's per-grammar S-expression source compiles once against its own grammar (a node kind absent from a grammar's symbol table never reaches that grammar's `Query`, so no cross-grammar pattern raises `QueryError` at build), and capture/node-kind strings resolve to integer ids once at build so the hot match loop compares ints. `ApiCatalogue` reflects one distribution's `importlib.metadata` surface into `member` evidence through the branch `RuntimeRail` fault lift, so a missing distribution is one `BoundaryFault` `import_` row the faults-owner `CLASSIFY` `ImportError` dispatch mints directly, never a raised `PackageNotFoundError` and never a re-tag. The page weaves four admitted surfaces as one structural rail — `tree-sitter` (the reused `Parser`, the per-grammar compiled `Query`, the `Point`/byte extents), `expression` (the `Evidence` `@tagged_union`, the `Block`/`Seq`/`Map` drift fold), `opentelemetry-api` (the `code.query` scan span carrying probe/lang/capture attributes), and the runtime `reliability/faults#FAULT` `boundary`/`traversed` (the import lift and the accumulating multi-grammar traversal) — so a structural scan is a traced, fault-railed leg the way the sibling `evidence/identity#IDENTITY` and `transport/wire#WIRE_RAIL` pages are. The surface produces evidence the rail reads — never a competing search owner, never a guessed environment status, never an exception into domain flow, never an untraced parse.

## [01]-[INDEX]

- [01]-[EVIDENCE]: the evidence union, the grammar registry with reused parsers and resolved ids, the probe table, the API catalogue, the cross-language drift fold.

## [02]-[EVIDENCE]

- Owner: `Evidence` — the one slot/kind evidence union the `assay code` rail consumes, its `member`/`span`/`drift` cases the API-surface row, the structural-capture extent, and the cross-language re-mint defect; `Lang` the closed grammar discriminant; `Grammar` the value object binding one `Language` to its single reused `Parser`; `Probe` the value object binding one named probe to its per-grammar `Query` compiled once against each grammar's own-language source through `PROBE_SOURCES`; `GrammarRegistry` the static owner folding the `GRAMMARS` rows into the reused-parser/compile-once registry the parsing law mandates; `ApiCatalogue` the static surface reflecting one distribution into `member` evidence through the `RuntimeRail` import lift.
- Cases: the two evidence families collapse into one union with slot/kind metadata — `span` carries `(Lang, str, str, Point, int, int)` (grammar, capture name, the bound source text, start `tree_sitter.Point`, start/end byte) and `drift` carries `(str, frozenset[Lang], Lang, Point, int)` (the offending name, the binding language set proving multiplicity, this binding's grammar and `Point` location), the `tree_sitter.Point` `(row, column)` value object read straight off `Node.start_point` rather than a re-minted `tuple[int, int]` alias the package already owns, because a structural capture and a cross-language re-mint are one self-describing extent fact discriminated by whether the carried name binds in more than one grammar namespace, not two carriers; the name rides on the `span` payload (read once off `Node.text`) so the drift fold never re-touches the source buffer, the carried text the single identity both the raw consumer and the multiplicity filter read; `member` carries `(str, str, str, str)` (distribution, import name, symbol family, official spelling) because an API surface row is a distinct distribution fact, never a re-shaped span; correlation flows through the `assay code` receipt the rail mints, never a per-case id field.
- Entry: `Evidence.of` is the one capture-keyed factory minting a `span` from a `(Lang, capture, Node)` triple, reading the bound text off `node.text` and the extent off `node.start_point`/`start_byte`/`end_byte` rather than recomputing positions from the source buffer; `GrammarRegistry.query` is the one polymorphic structural-extraction entry parameterized on its output through the `into: Callable[[Lang, str, Node], R]` projector — the raw `SpanRow` projector for a span consumer and `Evidence.of` for the receipt rail, so the same scan serves both shapes without a second method — running one `QueryCursor` over the reused `Parser`'s `Tree` under one `code.query` `opentelemetry-api` span (carrying the `probe`/`lang`/`captures` attributes so a structural scan is a traceable leg the way `evidence/identity#IDENTITY` `content.derive` and `transport/wire#WIRE_RAIL` `Decode` are, never a silent untraced parse) with `set_byte_range`/`set_max_start_depth` scoping and a `progress_callback` step-budget abort, the run's bound surfaced through one total `match` over `(did_exceed_match_limit, budget-remaining)` — a clean run returns `Ok`, a match-limit overflow a `BoundaryFault` `resource` row, and an exhausted step budget a `deadline` row carrying the budget — so neither bound clips the result silently; `GrammarRegistry.drift` runs the `binding` `Probe` per `GRAMMARS` row over a `Block[(Lang, source)]` corpus through `traversed(accumulate=True)`, folds the captured canonical-name bindings into a persistent name-to-languages `Map` through `Seq.fold`, and emits one `drift` `Evidence` per binding only where the carried name binds across more than one grammar namespace; `ApiCatalogue.reflect` lifts `metadata.distribution` through the `reflected` aspect — `boundary` catching `PackageNotFoundError`/`ImportError` so the faults-owner `CLASSIFY` `ImportError` row lands the lift directly in the typed `import_` case (no second `map_error` re-tag, since `PackageNotFoundError` is itself an `ImportError` subclass `CLASSIFY` already routes) — into `RuntimeRail[Block[Evidence]]`, mining one `member` per entry point plus the importable top-level surface.
- Auto: `GRAMMARS` is the `Map[Lang, Grammar]` built once at module import — each `Grammar` wraps its capsule exactly once (`Language(tree_sitter_python.language())`, `Language(tree_sitter_typescript.language_typescript())`, `Language(tree_sitter_typescript.language_tsx())`, the deprecated `int`-pointer/`Language.query` forms refused) and constructs the one reusable `Parser` bound to that grammar, the reused-parser-per-grammar topology the `runtime/.api/tree-sitter.md` `[PARSING_TOPOLOGY]`/`[LOCAL_ADMISSION]` law fixes against per-parse construction; `PROBE_SOURCES` is the `Map[str, Map[Lang, str]]` pairing each probe name with its per-grammar S-expression source, and `PROBES` is the `Map[str, Probe]` `PROBE_SOURCES.map`s into compiled queries — `Probe.of` compiles each grammar against ITS OWN-language source through `Query(grammar.language, sources[lang])` so the compile cost is paid once per grammar at import, never per query run, and a node kind absent from a grammar's symbol table never reaches that grammar's `Query` (the cross-grammar `[...]` alternation that mixed Python `class_definition`/`type_alias_statement` with TypeScript `class_declaration`/`interface_declaration`/`type_alias_declaration` against one `pattern` was a `QueryError` at compile for every grammar, since each arm names node kinds absent from the others — the deleted form); the `binding` probe is the per-grammar declaration alternation capturing `(identifier) @name` (`_BINDING_PY` the Python `class_definition`/`type_alias_statement` arm, `_BINDING_TS` the shared `class_declaration`/`interface_declaration`/`type_alias_declaration` arm both TypeScript and TSX bind) already scoped by the grammar to exactly its own type/class/interface declaration nodes (the structural scoping replacing the deleted node-type string matching the law forbids, so no `kind_id` post-filter is re-derived), and the `tags`/`highlights` probes pull each grammar's OWN bundled `TAGS_QUERY`/`HIGHLIGHTS_QUERY` `Final[str]` source (`ts_py` for Python, `ts_ts` for both TypeScript dialects) the grammar packages ship lazily through `importlib.resources` (mined per grammar, never re-authored, never a Python tags source compiled against the TypeScript grammar); `query` reads the bound name straight off `Node.text` and the extent off `Node.start_point` (the `tree_sitter.Point` value object)/`start_byte`/`end_byte` rather than recomputing positions from the source buffer, scopes the run with `set_byte_range`/`set_max_start_depth`, opens the `code.query` span around the parse-and-capture leg so the probe, language, and capture count ride one trace span, and bounds it with a `progress_callback` decrementing a step budget (the modern abort the law mandates over the `@deprecated` `timeout_micros`), surfacing `did_exceed_match_limit` truncation as a `BoundaryFault` `resource` row; the drift fold threads the multi-grammar corpus through `traversed(..., accumulate=True)` so a parse fault on one grammar combines rather than aborting the others, then folds the canonical-name bindings into the persistent `Map[str, frozenset[Lang]]` through `Seq.fold` over the span `Seq` so a legitimately distinct same-named concept in one namespace yields no defect and a re-minted identity seed, receipt rail, or capability descriptor across Python and TypeScript yields one `drift` row per binding location of a multi-namespace name — a false positive is filtered by namespace multiplicity, never a blanket name match, and the fold is persistent rather than a mutated accumulator; the `reflected` aspect (a signature-preserving `boundary`-weaving decorator per the `surfaces-and-dispatch#ASPECTS` definition-time-aspect law) materializes the import lift once at definition so `ApiCatalogue.reflect` returns `RuntimeRail` with `metadata`'s `PackageNotFoundError`/`ImportError` converted to a `BoundaryFault` `import_` row exactly once through the faults-owner `CLASSIFY` `ImportError` dispatch row — the aspect re-tags nothing, since the one classification table already routes both the `PackageNotFoundError` subclass and a bare `ImportError` to the `import_` case, so a second `map_error` reaching into a non-`boundary` case slot is the deleted form — never raised into the fold; the `canonical` set arrives as the shared one-name-one-owner registry the topology law owns, never re-minted here.
- Packages: `importlib.metadata` (`distribution`, `entry_points`, `PackageNotFoundError`), `tree-sitter` (`Language`/`Parser.parse`/`Query`/`QueryCursor.captures`/`set_byte_range`/`set_max_start_depth`/`did_exceed_match_limit`/`Point`/`Node.text`/`Node.start_point`/`Node.start_byte`/`Node.end_byte`), `tree-sitter-python` (`language`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`), `tree-sitter-typescript` (`language_typescript`/`language_tsx`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`), `expression` (`Ok`/`Error`/`Some`/`Nothing`/`Block`/`Seq`/`Map`/`tagged_union`/`case`/`tag`), `msgspec` (`Struct`), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.set_attribute` the `code.query` structural-scan span), `rasm.runtime.faults` (`RuntimeRail`/`BoundaryFault`/`boundary`/`traversed`).
- Growth: a new evidence family is one `Evidence` case with its own match arm; a new language is one `Lang` member, one `GRAMMARS` `Grammar` row binding its capsule and reused parser, and one own-language source column per `PROBE_SOURCES` probe (its `binding` declaration alternation plus its grammar-bundled `tags`/`highlights`), each compiling once against the new grammar; a new structural probe is one `PROBE_SOURCES` row carrying its per-grammar source, compiled once; a new canonical name is one entry the caller adds to the `canonical` set; a new mined member field is one column on the `member` case payload; zero new surface, zero new parser class, zero per-language parser branch.
- Boundary: no package version tables in planning pages, no guessed environment status, no parallel canonical-name registry minted here, one `rasm.runtime.evidence` tracer and one fault rail (the `code.query` span is this page's single derivation span, never a second tracer per probe); a source cannot name a member absent from the catalogue evidence; the deleted forms are per-parse `Parser`/`Query` construction, positional untyped `(name, start, end)` triples, a re-minted `tuple[int, int]` `Point2` alias where the package owns the `tree_sitter.Point` value object, a second `map_error` re-tag indexing a non-`boundary` case slot (the faults-owner `CLASSIFY` `ImportError` row already lands the `import_` case, so the redundant re-tag both crashes on the unset `boundary` slot and duplicates the dispatch), a raised `PackageNotFoundError` crossing into domain flow, a mutated `Map` accumulator in the drift fold, node-type string matching in a recursion where the compiled query fits, a blanket same-name match without namespace multiplicity, three parallel evidence carriers, an untraced structural scan, and a second structural-search owner; the structural-parsing, drift, and API surfaces emit `Evidence` the `assay code` rail consumes, never a competing search owner, and the `canonical` registry and the per-run `assay code` receipt are upstream/downstream owners this page reads, never re-mints.

```python signature
from collections.abc import Callable
from functools import wraps
from importlib import metadata
from typing import Final, Literal

from expression import Block, Error, Nothing, Ok, Seq, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from tree_sitter import Language, Node, Parser, Point, Query, QueryCursor
import tree_sitter_python as ts_py
import tree_sitter_typescript as ts_ts

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, traversed


type Lang = Literal["python", "typescript", "tsx"]
type SpanRow = tuple[Lang, str, str, Point, int, int]

_TRACER: Final = trace.get_tracer("rasm.runtime.evidence")


_BINDING_PY: Final[str] = (
    "[(class_definition name: (identifier) @name)"
    " (type_alias_statement (type (identifier) @name))] @decl"
)
_BINDING_TS: Final[str] = (
    "[(class_declaration name: (type_identifier) @name)"
    " (interface_declaration name: (type_identifier) @name)"
    " (type_alias_declaration name: (type_identifier) @name)] @decl"
)


@tagged_union(frozen=True)
class Evidence:
    tag: Literal["member", "span", "drift"] = tag()
    member: tuple[str, str, str, str] = case()
    span: tuple[Lang, str, str, Point, int, int] = case()
    drift: tuple[str, frozenset[Lang], Lang, Point, int] = case()

    @staticmethod
    def of(lang: Lang, capture: str, node: Node) -> "Evidence":
        text = node.text.decode() if node.text is not None else ""
        return Evidence(span=(lang, capture, text, node.start_point, node.start_byte, node.end_byte))


class Grammar(Struct, frozen=True):
    lang: Lang
    language: Language
    parser: Parser

    @staticmethod
    def of(lang: Lang, capsule: object) -> "Grammar":
        language = Language(capsule)
        return Grammar(lang=lang, language=language, parser=Parser(language))


class Probe(Struct, frozen=True):
    name: str
    queries: Map[Lang, Query]

    @staticmethod
    def of(name: str, sources: "Map[Lang, str]", grammars: "Map[Lang, Grammar]") -> "Probe":
        compiled = grammars.map(lambda lang, grammar: Query(grammar.language, sources[lang]))
        return Probe(name=name, queries=compiled)


GRAMMARS: Final[Map[Lang, Grammar]] = Map.of_seq(
    (lang, Grammar.of(lang, capsule))
    for lang, capsule in (
        ("python", ts_py.language()),
        ("typescript", ts_ts.language_typescript()),
        ("tsx", ts_ts.language_tsx()),
    )
)
PROBE_SOURCES: Final[Map[str, Map[Lang, str]]] = Map.of_seq(
    (name, Map.of_seq(rows))
    for name, rows in (
        ("binding", (("python", _BINDING_PY), ("typescript", _BINDING_TS), ("tsx", _BINDING_TS))),
        ("tags", (("python", ts_py.TAGS_QUERY), ("typescript", ts_ts.TAGS_QUERY), ("tsx", ts_ts.TAGS_QUERY))),
        ("highlights", (("python", ts_py.HIGHLIGHTS_QUERY), ("typescript", ts_ts.HIGHLIGHTS_QUERY), ("tsx", ts_ts.HIGHLIGHTS_QUERY))),
    )
)
PROBES: Final[Map[str, Probe]] = PROBE_SOURCES.map(lambda name, sources: Probe.of(name, sources, GRAMMARS))


def reflected[**P](operation: Callable[P, Block[Evidence]], /) -> Callable[P, RuntimeRail[Block[Evidence]]]:
    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[Block[Evidence]]:
        return boundary(
            operation.__name__, lambda: operation(*args, **kwargs), catch=(metadata.PackageNotFoundError, ImportError)
        )

    return call


class GrammarRegistry:
    @staticmethod
    def query[R](
        lang: Lang,
        source: bytes,
        probe: str,
        into: Callable[[Lang, str, Node], R],
        *,
        max_depth: int | None = None,
        byte_range: tuple[int, int] | None = None,
        budget: int = 1 << 20,
    ) -> RuntimeRail[Block[R]]:
        grammar = GRAMMARS[lang]
        query = PROBES[probe].queries[lang]
        cursor = QueryCursor(query)
        if max_depth is not None:
            cursor.set_max_start_depth(max_depth)
        if byte_range is not None:
            cursor.set_byte_range(*byte_range)
        remaining: list[int] = [budget]

        def progress(_visited: int, /) -> bool:
            remaining[0] -= 1
            return remaining[0] > 0

        with _TRACER.start_as_current_span("code.query") as scope:
            scope.set_attribute("evidence.probe", probe)
            scope.set_attribute("evidence.lang", lang)
            tree = grammar.parser.parse(source)
            captures = cursor.captures(tree.root_node, progress_callback=progress)
            scope.set_attribute("evidence.captures", sum(len(nodes) for nodes in captures.values()))
            match (cursor.did_exceed_match_limit, remaining[0] > 0):
                case (False, True):
                    return Ok(Block.of_seq(into(lang, name, node) for name, nodes in captures.items() for node in nodes))
                case (True, _):
                    return Error(BoundaryFault(resource=(f"{probe}:{lang}", "match-limit")))
                case _:
                    return Error(BoundaryFault(deadline=(f"{probe}:{lang}", float(budget))))

    @staticmethod
    def drift(corpora: Block[tuple[Lang, bytes]], canonical: frozenset[str]) -> RuntimeRail[Block[Evidence]]:
        def bound(lang: Lang, source: bytes) -> RuntimeRail[Block[Evidence]]:
            return GrammarRegistry.query(lang, source, "binding", Evidence.of).map(
                lambda spans: spans.filter(lambda ev: ev.span[1] == "name" and ev.span[2] in canonical)
            )

        return traversed(corpora.map(lambda c: bound(*c)), accumulate=True).map(
            lambda nested: GrammarRegistry._cross(nested.collect(lambda spans: spans))
        )

    @staticmethod
    def _cross(spans: Block[Evidence]) -> Block[Evidence]:
        seed: Map[str, frozenset[Lang]] = Map.empty()
        languages = Seq.of_iterable(spans).fold(
            lambda acc, ev: acc.add(ev.span[2], acc.try_find(ev.span[2]).default_value(frozenset()) | {ev.span[0]}),
            seed,
        )
        return spans.choose(
            lambda ev: Some(Evidence(drift=(ev.span[2], languages[ev.span[2]], ev.span[0], ev.span[3], ev.span[4])))
            if len(languages[ev.span[2]]) > 1
            else Nothing
        )


class ApiCatalogue:
    @staticmethod
    @reflected
    def reflect(distribution: str, family: str) -> Block[Evidence]:
        dist = metadata.distribution(distribution)
        import_name = distribution.replace("-", "_")
        return Block.of_seq(
            Evidence(member=(distribution, import_name, family, ep.name)) for ep in dist.entry_points
        ).cons(Evidence(member=(distribution, import_name, family, import_name)))
```

## [03]-[RESEARCH]

[TREE_SITTER_QUERY] and [DRIFT_GRAMMARS] are reflection-confirmed against `runtime/.api/tree-sitter.md`: the `tree-sitter` binding compiles patterns through the `Query(language, source)` constructor (the deprecated `Language.query(source)` shim is refused) once per grammar at registry build, runs captures through `QueryCursor(query).captures(node, progress_callback=...)` returning the flattened `dict[str, list[Node]]`, bounds a run with the modern `progress_callback` (the single-`int`-arg `captures`/`matches` callback returning `False` to abort, the `@deprecated` `timeout_micros` refused), scopes with `set_byte_range`/`set_max_start_depth`, and surfaces truncation through `did_exceed_match_limit`. A `Query` source naming a node kind absent from its grammar's symbol table raises `QueryError` (a `ValueError`) at compile time, so each probe carries a PER-GRAMMAR source: the `binding` declaration alternation is `_BINDING_PY` for the Python grammar (`class_definition`/`type_alias_statement`) and `_BINDING_TS` for both TypeScript dialects (`class_declaration`/`interface_declaration`/`type_alias_declaration`), and `tags`/`highlights` pull each grammar's own bundled source — a single cross-grammar `[...]` alternation, or the Python `TAGS_QUERY` compiled against the TypeScript grammar, would not compile. Each per-grammar source scopes captures to that grammar's own declaration nodes so node-type string matching in a recursion is replaced by the compiled query, and `Node.text`/`start_point` (the `tree_sitter.Point` value object)/`byte_range` read the bound text and positions off the node rather than recomputing from the source. The grammar pointers are `tree_sitter_python.language()`, `tree_sitter_typescript.language_typescript()`, and `tree_sitter_typescript.language_tsx()`, each wrapped once via `tree_sitter.Language(...)` into a reused `Parser`, and the bundled `TAGS_QUERY`/`HIGHLIGHTS_QUERY` `Final[str]` sources (`tree_sitter_python` ships highlights/tags; `tree_sitter_typescript` ships highlights/locals/tags shared by both dialects) load lazily through `importlib.resources` on first attribute access. No open RESEARCH seam remains on this page.

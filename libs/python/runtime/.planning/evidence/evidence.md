# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence ride one tagged-union fact stream the `assay code` rail consumes: `member` a `MemberFact` row of a distribution's official surface, `span` a `SpanFact` tree-sitter capture, `drift` a `DriftFact` cross-language re-mint of a canonical wire-projection name the topology law forbids. This surface produces evidence the rail reads — never a competing search owner, a guessed environment status, or an exception into domain flow.

`GrammarRegistry` holds the reused-`Parser`/compile-once topology the `runtime/.api/tree-sitter.md` parsing law fixes, and `scan` is the one polymorphic extraction entry — probe, multi-language `Corpus`, `Disposition` output shape, `Into[R]` projector — the disposition-keyed mirror of the `reliability/faults#FAULT` `traversed`. `ApiCatalogue.reflect` rides the canonical `@trapped(EVIDENCE_REFLECT, catch=ImportError)` faults aspect, so a missing distribution or failing root import lands as the one `import_` row. `EvidenceScan` streams each scan onto the `observability/receipts#RECEIPT` rail; the page tracer mints from the `SCOPES[Scope.EVIDENCE]` row; the `canonical` name registry arrives from the topology law and is never re-minted here.

## [01]-[INDEX]

- [02]-[EVIDENCE]: the value-object evidence union, the compile-once grammar registry with its one polymorphic `scan`, the member-level API catalogue, the drift fold, and the `EvidenceScan` receipt seam.

## [02]-[EVIDENCE]

- Owner: `Evidence` — the one slot/kind union whose cases carry frozen value objects, never positional tuples read by index; `Locus` the one extent value object both span-shaped facts share, so no `(lang, point, start_byte)` triple re-declares per fact and the extent rides the package-owned `tree_sitter.Point`, never a re-minted pair alias. `MemberFact` carries the distribution `version` and its REAL group — `ep.group`, `_GROUP_MEMBER`, `_GROUP_NESTED`, `_GROUP_IMPORT` — never a caller-supplied family constant, the inventory backing the boundary claim that a source cannot name a member absent from the catalogue; a `_GROUP_NESTED` row's symbol IS the dotted spelling a citation writes, so a membership question answers by equality rather than by reassembling a module and a leaf. Correlation flows through the `EvidenceScan` receipt and the rail's own `assay code` receipt, never a per-case id field.
- Entry: coverage is registry data on both entries — `scan` pre-filters the corpus to the probe's covered columns, so a `locals` scan over a Python row yields no evidence and no fault, while a direct `run` on an uncovered grammar returns the typed `config` `uncovered-grammar` fault; every bundled source is a first-class producer, `locals` the live partial-coverage column. `drift` flags a canonical name only where it binds in more than one grammar namespace, so a legitimately distinct same-named concept in one namespace never yields a defect. `reflect` mines the surface to a stated TWO-LEVEL reach — entry points, each mapped root's own members, and every submodule's exported members under the dotted spelling a citation writes — because an entry-points-only or root-only reflect cannot back the member-absence boundary claim against any nested member, which is the only kind a source actually names. The bound is what keeps that claim honest in the other direction: a distribution re-exports its dependencies' surfaces, so a deeper descent would report another project's members as this one's.
- Auto: the registry build pays everything once per grammar at import — compile, `PROBE_KEEP` prune, capture-name-to-id resolution — and each probe compiles its OWN-language source per grammar, because a cross-grammar alternation mixing Python and TypeScript node kinds in one pattern is a `QueryError` at compile for every grammar. `progress_callback` is the hang-guard cancel hook.
- Packages: `msgspec` — `gc=False` only on the container-free `MemberFact`; `Point`, the nested `Locus`, and `frozenset[Lang]` keep the span-shaped records GC-tracked. `tree-sitter-python`/`tree-sitter-typescript` ship the bundled `tags`/`highlights`/`locals` sources, and those sources carry only the internally-evaluated `#match?` predicate, so no `QueryPredicate` handler threads through the build.
- Growth: a new evidence family is one `Evidence` case with its value object; a new language one `Lang` member, one `GRAMMARS` row, and one own-language source column per probe; a new probe one `Probe` literal with one `PROBE_SOURCES` row, partial coverage included; a new capture allowlist one `PROBE_KEEP` row; a new traversal shape one `Disposition` member the faults owner adds; a custom-predicate probe earns the `QueryPredicate` handler as its own row when a non-built-in directive ships; a new member group one `_GROUP_*` constant; a widened catalogue reach is one `_DEPTH` value, and every level past the second widens the claim onto surfaces the distribution only re-exports.
- Boundary: the scan is one-shot by charter — an incremental `Tree.edit`/`changed_ranges` re-scan cache is the ruled-out form for this owner. One tracer spans `code.scan` over the per-file `code.query` legs — never a second tracer per probe — and the trace status and the rail outcome are the same fact on every arm.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from importlib import import_module, metadata
from itertools import count
from pkgutil import walk_packages
from types import ModuleType
from typing import Annotated, Final, Literal, overload

from expression import Error, Nothing, Ok, Option, Some, case, identity, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Meta, Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from tree_sitter import Language, Node, Parser, Point, Query, QueryCursor
import tree_sitter_python as ts_py
import tree_sitter_typescript as ts_ts

from rasm.runtime.faults import (
    EVIDENCE_BUDGET,
    EVIDENCE_GRAMMAR,
    EVIDENCE_MATCHES,
    EVIDENCE_REFLECT,
    SCOPES,
    BoundaryFault,
    Disposition,
    RuntimeRail,
    Scope,
    scoped,
    trapped,
    traversed,
)
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type Lang = Literal["python", "typescript", "tsx"]
type Probe = Literal["binding", "tags", "highlights", "locals"]
type Byte = Annotated[int, Meta(ge=0)]
type Corpus = Block[tuple[Lang, bytes]]
type Into[R] = Callable[[Lang, str, int, Node], R]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BUDGET: Final[int] = 1 << 20
_CAPTURE_NAME: Final[str] = "name"
_GROUP_IMPORT: Final[str] = "import"
_GROUP_MEMBER: Final[str] = "member"
_GROUP_NESTED: Final[str] = "nested"
_DEPTH: Final[int] = 2
_SPAN_QUERY: Final[str] = "code.query"
_SPAN_SCAN: Final[str] = "code.scan"

_BINDING_PY: Final[str] = "[(class_definition name: (identifier) @name) (type_alias_statement left: (type (identifier) @name))] @decl"
_BINDING_TS: Final[str] = (
    "[(class_declaration name: (type_identifier) @name)"
    " (interface_declaration name: (type_identifier) @name)"
    " (type_alias_declaration name: (type_identifier) @name)] @decl"
)


# --- [MODELS] ---------------------------------------------------------------------------


class Locus(Struct, frozen=True):
    lang: Lang
    point: Point
    start_byte: Byte
    end_byte: Byte

    @staticmethod
    def of(lang: Lang, node: Node) -> "Locus":
        return Locus(lang, node.start_point, node.start_byte, node.end_byte)


class SpanFact(Struct, frozen=True):
    capture: str
    capture_id: int
    text: str
    locus: Locus

    @staticmethod
    def of(lang: Lang, capture: str, capture_id: int, node: Node) -> "SpanFact":
        return SpanFact(capture, capture_id, node.text.decode() if node.text is not None else "", Locus.of(lang, node))


class MemberFact(Struct, frozen=True, gc=False):
    distribution: str
    version: str
    import_name: str
    group: str
    symbol: str


class DriftFact(Struct, frozen=True):
    name: str
    bindings: frozenset[Lang]
    locus: Locus


@tagged_union(frozen=True)
class Evidence:
    tag: Literal["member", "span", "drift"] = tag()
    member: MemberFact = case()
    span: SpanFact = case()
    drift: DriftFact = case()

    @staticmethod
    def of(lang: Lang, capture: str, capture_id: int, node: Node) -> "Evidence":
        return Evidence(span=SpanFact.of(lang, capture, capture_id, node))


class Grammar(Struct, frozen=True):
    lang: Lang
    language: Language
    parser: Parser

    @staticmethod
    def of(lang: Lang, capsule: object) -> "Grammar":
        language = Language(capsule)
        return Grammar(lang=lang, language=language, parser=Parser(language))


class CompiledProbe(Struct, frozen=True):
    name: Probe
    queries: Map[Lang, Query]
    captures: Map[Lang, Map[str, int]]

    @staticmethod
    def of(name: Probe, sources: "Map[Lang, str]", grammars: "Map[Lang, Grammar]", kept: Option[frozenset[str]]) -> "CompiledProbe":
        covered = grammars.filter(lambda lang, _grammar: lang in sources)
        queries = covered.map(lambda lang, grammar: CompiledProbe._pruned(Query(grammar.language, sources[lang]), kept))
        return CompiledProbe(
            name=name,
            queries=queries,
            captures=queries.map(lambda _lang, query: Map.of_seq((query.capture_name(i), i) for i in range(query.capture_count))),
        )

    @staticmethod
    def _pruned(query: Query, kept: Option[frozenset[str]]) -> Query:
        kept.map(
            lambda keep: Block.of_seq(query.capture_name(i) for i in range(query.capture_count))
            .filter(lambda cap: cap not in keep)
            .map(query.disable_capture)
        )
        return query


# --- [TABLES] ---------------------------------------------------------------------------

GRAMMARS: Final[Map[Lang, Grammar]] = Map.of_seq(
    (lang, Grammar.of(lang, capsule))
    for lang, capsule in (("python", ts_py.language()), ("typescript", ts_ts.language_typescript()), ("tsx", ts_ts.language_tsx()))
)
PROBE_SOURCES: Final[Map[Probe, Map[Lang, str]]] = Map.of_seq(
    (name, Map.of_seq(rows))
    for name, rows in (
        ("binding", (("python", _BINDING_PY), ("typescript", _BINDING_TS), ("tsx", _BINDING_TS))),
        ("tags", (("python", ts_py.TAGS_QUERY), ("typescript", ts_ts.TAGS_QUERY), ("tsx", ts_ts.TAGS_QUERY))),
        ("highlights", (("python", ts_py.HIGHLIGHTS_QUERY), ("typescript", ts_ts.HIGHLIGHTS_QUERY), ("tsx", ts_ts.HIGHLIGHTS_QUERY))),
        ("locals", (("typescript", ts_ts.LOCALS_QUERY), ("tsx", ts_ts.LOCALS_QUERY))),
    )
)
PROBE_KEEP: Final[Map[Probe, frozenset[str]]] = Map.of_seq((("binding", frozenset((_CAPTURE_NAME,))),))
PROBES: Final[Map[Probe, CompiledProbe]] = PROBE_SOURCES.map(
    lambda name, sources: CompiledProbe.of(name, sources, GRAMMARS, PROBE_KEEP.try_find(name))
)
_BINDING_NAME_ID: Final[Map[Lang, int]] = PROBES["binding"].captures.map(lambda _lang, table: table[_CAPTURE_NAME])


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final[trace.Tracer] = scoped(trace.get_tracer, SCOPES[Scope.EVIDENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _flattened[R](nested: Block[Block[R]]) -> Block[R]:
    return nested.collect(identity)


class GrammarRegistry:
    @staticmethod
    def run[R](
        lang: Lang,
        source: bytes,
        probe: Probe,
        into: Into[R],
        *,
        max_depth: int | None = None,
        byte_range: tuple[int, int] | None = None,
        budget: int = _BUDGET,
    ) -> RuntimeRail[Block[R]]:
        probed = PROBES[probe]
        if lang not in probed.queries:
            return Error(EVIDENCE_GRAMMAR.raised(probe, lang))
        cursor = QueryCursor(probed.queries[lang], match_limit=budget)
        if max_depth is not None:
            cursor.set_max_start_depth(max_depth)
        if byte_range is not None:
            cursor.set_byte_range(*byte_range)

        ids = probed.captures[lang]
        with _TRACER.start_as_current_span(_SPAN_QUERY) as scope:
            if scope.is_recording():
                scope.set_attributes({"evidence.probe": probe, "evidence.lang": lang})
            tree = GRAMMARS[lang].parser.parse(source)
            visited = tree.root_node.descendant_count
            if visited >= budget:
                fault = BoundaryFault(deadline=(EVIDENCE_BUDGET.subject, float(budget), f"descendant-count:{probe}:{lang}"))
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            ticks = count(1)
            captures = cursor.captures(tree.root_node, progress_callback=lambda _offset: next(ticks) >= budget)
            truncated = cursor.did_exceed_match_limit
            if scope.is_recording():
                scope.set_attributes({
                    "evidence.nodes": visited,
                    "evidence.captures": sum(map(len, captures.values())),
                    "evidence.truncated": truncated,
                    "evidence.flawed": tree.root_node.has_error,
                })
            if truncated:
                fault = EVIDENCE_MATCHES.raised(probe, lang)
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            scope.set_status(Status(StatusCode.OK))
            return Ok(Block.of_seq(into(lang, name, ids[name], node) for name, nodes in captures.items() for node in nodes))

    @overload
    @staticmethod
    def scan(
        probe: Probe, corpus: Corpus, *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...
    ) -> RuntimeRail[Block[Evidence]]: ...
    @overload
    @staticmethod
    def scan(
        probe: Probe, corpus: Corpus, *, by: Literal[Disposition.PARTITION], budget: int = ...
    ) -> RuntimeRail[tuple[Block[Evidence], Block[BoundaryFault]]]: ...
    @overload
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...
    ) -> RuntimeRail[Block[R]]: ...
    @overload
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.PARTITION], budget: int = ...
    ) -> RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]: ...
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R] = Evidence.of, *, by: Disposition = Disposition.ACCUMULATE, budget: int = _BUDGET
    ) -> RuntimeRail[Block[R]] | RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]:
        covered = corpus.filter(lambda row: row[0] in PROBES[probe].queries)
        with _TRACER.start_as_current_span(_SPAN_SCAN) as scope:
            if scope.is_recording():
                scope.set_attributes({"evidence.probe": probe, "evidence.corpus": len(corpus), "evidence.covered": len(covered)})
            rails = covered.map(lambda row: GrammarRegistry.run(row[0], row[1], probe, into, budget=budget))
            graded = (
                traversed(rails, by=by).map(lambda split: (_flattened(split[0]), split[1]))
                if by is Disposition.PARTITION
                else traversed(rails, by=by).map(_flattened)
            )
            scope.set_status(graded.map(lambda _ok: Status(StatusCode.OK)).default_with(lambda fault: Status(StatusCode.ERROR, fault.tag)))
            return graded

    @staticmethod
    def drift(corpus: Corpus, canonical: frozenset[str]) -> RuntimeRail[Block[Evidence]]:
        return GrammarRegistry.scan("binding", corpus, SpanFact.of).map(
            lambda spans: GrammarRegistry._cross(
                spans.filter(lambda fact: fact.capture_id == _BINDING_NAME_ID[fact.locus.lang] and fact.text in canonical)
            )
        )

    @staticmethod
    def _cross(spans: Block[SpanFact]) -> Block[Evidence]:
        seed: Map[str, frozenset[Lang]] = Map.empty()
        bindings = spans.fold(lambda acc, fact: acc.change(fact.text, lambda b: Some(b.default_value(frozenset()) | {fact.locus.lang})), seed)
        return spans.choose(
            lambda fact: Some(Evidence(drift=DriftFact(fact.text, bindings[fact.text], fact.locus))) if len(bindings[fact.text]) > 1 else Nothing
        )


class ApiCatalogue:
    @staticmethod
    @trapped(EVIDENCE_REFLECT, catch=ImportError)
    def reflect(distribution: str) -> Block[Evidence]:
        dist = metadata.distribution(distribution)
        version = dist.version
        mapped = Block.of_seq(sorted(mod for mod, dists in metadata.packages_distributions().items() if distribution in dists))
        roots = mapped if len(mapped) else Block.of_seq((distribution.replace("-", "_"),))
        return Block.of_seq(
            Evidence(member=MemberFact(distribution, version, ep.module.partition(".")[0], ep.group, ep.name))
            for ep in dist.entry_points
        ).append(roots.collect(lambda root: ApiCatalogue._mined(distribution, version, root)))

    @staticmethod
    def _exported(surface: ModuleType) -> Block[str]:
        return (
            Option.of_optional(getattr(surface, "__all__", None))
            .map(Block.of_seq)
            .default_with(lambda: Block.of_seq(name for name in vars(surface) if not name.startswith("_")))
        )

    @staticmethod
    def _mined(distribution: str, version: str, root: str) -> Block[Evidence]:
        surface = import_module(root)
        rooted = Evidence(member=MemberFact(distribution, version, root, _GROUP_IMPORT, root))
        own = ApiCatalogue._exported(surface).map(lambda symbol: Evidence(member=MemberFact(distribution, version, root, _GROUP_MEMBER, symbol)))
        nested = Block.of_seq(
            info.name for info in walk_packages(getattr(surface, "__path__", ()), f"{root}.") if info.name.count(".") < _DEPTH
        ).collect(lambda name: ApiCatalogue._nested(distribution, version, name))
        return own.cons(rooted).append(nested)

    @staticmethod
    def _nested(distribution: str, version: str, module: str) -> Block[Evidence]:
        loaded = import_module(module)
        return ApiCatalogue._exported(loaded).map(
            lambda symbol: Evidence(member=MemberFact(distribution, version, module, _GROUP_NESTED, f"{module}.{symbol}"))
        ).cons(Evidence(member=MemberFact(distribution, version, module, _GROUP_IMPORT, module)))


# --- [COMPOSITION] ----------------------------------------------------------------------


class EvidenceScan(Struct, frozen=True):
    owner: str
    evidence: Block[Evidence]

    def contribute(self) -> Iterable[Receipt]:
        counts = self.evidence.fold(lambda acc, ev: acc.change(f"count.{ev.tag}", lambda n: Some(n.default_value(0) + 1)), Map.empty())
        drifts = self.evidence.choose(
            lambda ev: Some((f"drift.{ev.drift.name}", ",".join(sorted(ev.drift.bindings)))) if ev.tag == "drift" else Nothing
        )
        facts: dict[str, object] = dict(counts.items()) | dict(drifts)
        return (Receipt.of(self.owner, ("emitted", _SPAN_SCAN, facts)),)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

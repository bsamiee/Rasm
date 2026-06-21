# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence ride one tagged-union fact stream the `assay code` rail consumes. `Evidence` is the single slot/kind union whose three cases carry frozen value objects, never positional tuples: `member` a `MemberFact` (the official distribution surface a source may later name), `span` a `SpanFact` (one `tree-sitter` capture), and `drift` a `DriftFact` (a cross-language re-mint of a canonical wire-projection name the topology law forbids). The byte/`Point` extent every span-shaped fact carries collapses into one `Locus` value object, so `SpanFact` and `DriftFact` share a single `locus` field rather than re-declaring a `(lang, point, start_byte)` triple each, and every fold reads `fact.text`/`fact.locus.lang` by name.

`GrammarRegistry.scan` is the one polymorphic structural-extraction entry, parameterized over the probe, the multi-language `Corpus` input, the `Disposition` traversal output, and the `Into[R]` projector. Any probe — the `binding` declaration alternation, the grammar-bundled `tags`, or `highlights` — runs through the same `scan`, so `tags`/`highlights` are first-class `Evidence` producers, not compiled-but-unreachable columns. `drift` is a `binding` scan post-filtered to the canonical names; `run` is the single-corpus leg `scan` folds across the corpus through `traversed`.

`GrammarRegistry` is the reused-`Parser`/compile-once owner the `tree-sitter` parsing law demands. Grammars wrap once into a `Map[Lang, Grammar]` row, each probe's per-grammar S-expression source compiles once against its own grammar through `CompiledProbe.of`, and a probe missing a language column compiles total over its covered keys via `Map.filter` before the compile `map`. A node kind absent from a grammar's symbol table never reaches that grammar's `Query`, so no cross-grammar pattern raises `QueryError` at build and no `sources[lang]` index raises `KeyError` over an uncovered grammar.

`ApiCatalogue.reflect` reflects one distribution's `importlib.metadata` surface into `member` evidence through the canonical `reliability/faults#FAULT` `@trapped("reflect", catch=ImportError)` aspect — the faults owner's signature-preserving `iscoroutinefunction`-dispatched `_guard`/`async_boundary`-weaving decorator, the definition-time aspect form the `surfaces-and-dispatch#ASPECTS` law prefers over an inline `boundary` lambda at every call site — so a missing distribution lands as one `BoundaryFault` `import_` row the faults-owner `CLASSIFY` `ImportError` dispatch mints, the `PackageNotFoundError` subclass routing through that same row, never a raised exception and never a re-tag. Each `MemberFact` carries the distribution `version` and the real entry-point `group` read off `ep.group`, never a caller-supplied family constant repeated per member.

The page weaves five admitted surfaces as one structural rail: `tree-sitter` (the reused `Parser`, the per-grammar compiled `Query`, the `Node.descendant_count` budget signal, the `Point`/byte extents), `msgspec` (the `Locus`/`SpanFact`/`MemberFact`/`DriftFact` `gc=False` value objects), `expression` (the `Evidence` `@tagged_union`, the `Block`/`Map` drift fold), `opentelemetry-api` (the `code.query` span carrying probe/lang/capture attributes batched through `set_attributes` under an `is_recording()` gate plus a total `Status(StatusCode.OK | ERROR)`), and runtime `reliability/faults#FAULT`/`observability/receipts#RECEIPT` (the `@trapped` import lift, the `Disposition`-keyed multi-grammar `traversed` fold, the `EvidenceScan` `ReceiptContributor` streaming the scan onto the one receipt rail). A structural scan is therefore a traced, fault-railed, receipted leg the way the sibling `evidence/identity#IDENTITY` and `transport/wire#WIRE_RAIL` pages are.

The surface produces evidence the rail reads, never a competing search owner, a guessed environment status, an exception into domain flow, an untraced parse, or a span whose trace status disagrees with its rail outcome.

## [01]-[INDEX]

- [01]-[EVIDENCE]: the value-object-carrying evidence union over one shared `Locus`, the grammar registry with reused parsers and compile-once probes, the probe table compiled total over partial coverage, the one `scan` entry polymorphic over probe/corpus/disposition/output, the API catalogue, the cross-language drift fold, the `EvidenceScan` receipt seam.

## [02]-[EVIDENCE]

- Owner: `Evidence` — the one slot/kind evidence union the `assay code` rail consumes, its `member`/`span`/`drift` cases carrying the `MemberFact`/`SpanFact`/`DriftFact` frozen `gc=False` value objects (the API-surface row, the structural-capture extent, the cross-language re-mint defect); `Locus` the shared `gc=False` extent value object (`lang`, the `tree_sitter.Point`, the `Byte` byte offsets) both `SpanFact` and `DriftFact` carry by one `locus` field; `Lang` the closed grammar discriminant, `Probe` the closed probe-name discriminant, `Corpus` the multi-language `(Lang, bytes)` input row, `Into[R]` the output-projector alias, `Byte` the `Meta(ge=0)`-bounded offset; `Grammar` the value object binding one `Language` to its single reused `Parser`; `CompiledProbe` the value object binding one named probe to its per-grammar `Query` compiled once against each grammar's own-language source through `PROBE_SOURCES`; `GrammarRegistry` the static owner folding the `GRAMMARS` rows into the reused-parser/compile-once registry and owning the one `scan` extraction entry over the single-corpus `run`; `ApiCatalogue` the static surface reflecting one distribution into `member` evidence through the canonical `@trapped("reflect", catch=ImportError)` faults aspect; `EvidenceScan` the `ReceiptContributor`-shaped value object folding a scan's evidence into one `emitted` `Receipt`.
- Cases: the three evidence families collapse into one union whose cases carry frozen value objects, not positional tuples — `span` carries a `SpanFact(capture, text, locus)`, `drift` a `DriftFact(name, bindings, locus)`, and `member` a `MemberFact(distribution, version, import_name, group, symbol)` — each a `gc=False` leaf the `msgspec.md` topology law admits for a container-free record. `Locus(lang, point, start_byte, end_byte)` is the one extent value object both span-shaped facts share: the `tree_sitter.Point` `(row, column)` read straight off `Node.start_point` rather than a re-minted `tuple[int, int]` alias the package already owns, the byte offsets `Meta(ge=0)`-bounded `Byte`. A structural capture and a cross-language re-mint are one self-describing extent fact discriminated by whether the carried name binds in more than one grammar namespace; the name rides on the `SpanFact.text` field read once off `Node.text`, so the `_cross` fold and the canonical filter read `fact.text`/`fact.locus.lang` by name, never re-touch the source buffer, and the deleted six-wide `ev.span[2]` index-guessing and the duplicated `(lang, point, start_byte)` triple are gone. Correlation flows through the `EvidenceScan` receipt and the `assay code` receipt the rail mints, never a per-case id field.
- Entry: `Evidence.of` is the one capture-keyed factory minting a `span` `Evidence` carrying a `SpanFact` from a `(Lang, capture, Node)` triple, reading the bound text off `node.text` and the extent through `Locus.of` off `node.start_point`/`start_byte`/`end_byte` rather than recomputing positions from the source buffer. `GrammarRegistry.scan` is the one polymorphic structural-extraction entry, parameterized over both input and output: the `Probe`, the `Corpus` multi-language `(Lang, bytes)` `Block`, the `Disposition` that grades the multi-grammar outcome AND selects the return shape, and the `Into[R]` projector — `Evidence.of` (the default) for the receipt rail's `Evidence` stream, `SpanFact.of` for the drift fold's typed-field `SpanFact`, both sharing the one `SpanFact.of` extent-extraction primitive `Evidence.of` delegates to rather than re-minting a throwaway `Evidence` only to project its `.span` slot. It folds `run` across the corpus through `traversed(rails, by=by)` and flattens the nested `Block` through `Block.collect`, so `binding`, `tags`, and `highlights` each produce evidence through one entry; the default `Disposition.ACCUMULATE` combines a parse fault on one grammar rather than aborting the others and returns `RuntimeRail[Block[R]]`, while `Disposition.PARTITION` returns the `RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]` split a corpus-health scan reads to keep the parsed evidence and the per-file parse faults on one pass. The output shape is carried statically by `@overload` arms keyed on the `Disposition` `Literal` exactly as the faults owner's `traversed` carries it, so a caller narrows on the disposition it passes rather than re-matching the bare widened union — `scan` is the disposition-keyed mirror of `traversed`, never a second widened return forcing a caller re-narrow. `GrammarRegistry.run` is the single-corpus leg, running one `QueryCursor` over the reused `Parser`'s `Tree` under one `code.query` `opentelemetry-api` span whose `probe`/`lang`/`nodes`/`captures`/`truncated` attributes batch through `set_attributes` only behind an `is_recording()` gate (the OTel hot-path law) so a structural scan is a traceable leg the way `evidence/identity#IDENTITY` `content.derive` and `transport/wire#WIRE_RAIL` `Decode` are, with `set_byte_range`/`set_max_start_depth` scoping the run. The run grades on two observable axes plus one hang guard. The `deadline` arm is a deterministic PRE-gate: `tree.root_node.descendant_count` is a static tree-size signal known the instant the parse returns, so a tree at or over `budget` short-circuits to `Status(StatusCode.ERROR, "deadline")` plus a `BoundaryFault` `deadline` row carrying the budget BEFORE the query runs, and the bounded `captures` call executes only within budget. The `QueryCursor` is built `match_limit=budget` so `did_exceed_match_limit` is a LIVE post-query grade — the default `0xFFFFFFFF` cap never trips, leaving the `resource` arm dead and a match-explosion silently clipped onto a clean `Ok`. `did_exceed_match_limit` is therefore the one post-query grade: `True` sets `Status(StatusCode.ERROR, "resource")` plus a `resource` row, otherwise the run sets `Status(StatusCode.OK)` and returns the captured `Ok` `Block`. `progress_callback` (`step < budget`) is the third axis — a step ceiling against pathological query backtracking on an in-budget tree — whose abort tree-sitter surfaces as a bounded partial result rather than a grade, so the size pre-gate is sized to keep it unreachable and the ceiling stays a hang guard, not a rail outcome. The trace status and the rail outcome are the same fact, and these arms return `Error` without raising, so `run` sets the span status itself rather than the faults owner's raised-exception weave. `GrammarRegistry.drift` is the `binding` scan post-filtered to the `canonical` names, folding the captured `SpanFact` bindings into a persistent name-to-languages `Map` through `Block.fold`/`Map.change` and emitting one `drift` `Evidence` per binding only where the carried name binds across more than one grammar namespace. `ApiCatalogue.reflect` carries the `@trapped("reflect", catch=ImportError)` faults aspect directly so the faults-owner `CLASSIFY` `ImportError` row lands the lift in the typed `import_` case (no tuple of exception types and no second `map_error` re-tag, since `PackageNotFoundError` is itself an `ImportError` subclass `CLASSIFY` already routes), returning `RuntimeRail[Block[Evidence]]` mining one `member` per entry point (the `dist.version`, the `ep.group` family, the `ep.name` symbol) plus the importable top-level surface under the `"import"` group — the local re-minted `reflected` wrapper deleted in favor of the canonical owner aspect. `EvidenceScan.contribute` folds a scan's `Block[Evidence]` into one `emitted` `Receipt` carrying the per-kind tally and the multi-namespace drift names through `Receipt.of`, so a structural scan streams onto the one `observability/receipts#RECEIPT` rail rather than minting a private evidence log.
- Auto: `GRAMMARS` is the `Map[Lang, Grammar]` built once at module import — each `Grammar` wraps its capsule exactly once (`Language(tree_sitter_python.language())`, `Language(tree_sitter_typescript.language_typescript())`, `Language(tree_sitter_typescript.language_tsx())`, the deprecated `int`-pointer/`Language.query` forms refused) and constructs the one reusable `Parser` bound to that grammar, the reused-parser-per-grammar topology the `runtime/.api/tree-sitter.md` `[PARSING_TOPOLOGY]`/`[LOCAL_ADMISSION]` law fixes against per-parse construction. `PROBE_SOURCES` is the `Map[Probe, Map[Lang, str]]` pairing each probe name with its per-grammar S-expression source, and `PROBES` is the `Map[Probe, CompiledProbe]` `PROBE_SOURCES.map`s into compiled queries — `CompiledProbe.of` compiles each grammar against ITS OWN-language source by `Map.filter`ing the grammars to the covered keys (`lang in sources`) then `map`ping `Query(grammar.language, sources[lang])`, so the compile cost is paid once per grammar at import (never per run), a node kind absent from a grammar's symbol table never reaches that grammar's `Query` (the cross-grammar `[...]` alternation mixing Python `class_definition`/`type_alias_statement` with TypeScript `class_declaration`/`interface_declaration`/`type_alias_declaration` against one `pattern` was a `QueryError` at compile for every grammar, the deleted form), and a probe omitting a language column compiles total over its covered languages rather than raising `KeyError` on a `sources[lang]` index. The `binding` probe is the per-grammar declaration alternation capturing `(identifier) @name` (`_BINDING_PY` the Python arm, `_BINDING_TS` the shared `class_declaration`/`interface_declaration`/`type_alias_declaration` arm both TypeScript and TSX bind) scoped by the grammar to exactly its own declaration nodes, replacing the deleted node-type string matching the law forbids; the `tags`/`highlights` probes pull each grammar's OWN bundled `TAGS_QUERY`/`HIGHLIGHTS_QUERY` `Final[str]` source (`ts_py` for Python, `ts_ts` for both TypeScript dialects) the grammar packages ship lazily through `importlib.resources`, never a Python tags source compiled against the TypeScript grammar. `run` reads the bound name off `Node.text` and the extent through `Locus.of` off `Node.start_point`/`start_byte`/`end_byte`, opens the `code.query` span around the parse-and-capture leg behind an `is_recording()` gate batched through `set_attributes` (no attribute computed when no SDK records), gates the `deadline` arm on the static `tree.root_node.descendant_count >= budget` tree-size signal BEFORE running the query so the bounded `captures` call executes only within budget, builds the `QueryCursor` with `match_limit=budget` so truncation is a detectable `did_exceed_match_limit` grade rather than the dead default-`0xFFFFFFFF` cap, and caps the run with a `progress_callback` returning `step < budget` over `tree-sitter`'s supplied running count (the modern step-ceiling hang guard the law mandates over the `@deprecated` `timeout_micros`, a pure predicate over the C-callback count rather than a decrementing local cell); the post-query grade is the single `did_exceed_match_limit` check — `Status(StatusCode.ERROR, "resource")` plus a `BoundaryFault` `resource` row on truncation, otherwise `Status(StatusCode.OK)` plus the captured `Block` — so the span status the trace reads and the rail outcome the caller binds are the same fact, the size pre-gate and the match-limit grade cover the two observable axes the rail decides on while the `progress_callback` abort (a tree-sitter partial result, not a grade) stays an unreachable hang guard sized out by the pre-gate, and the grading folds into `run` rather than a single-caller helper threading wide positional state. `scan` maps `run` across the `Corpus`, threads the rails through `traversed(rails, by=by)` so the `Disposition` row selects the multi-grammar output shape, and flattens the nested `Block` through `Block.collect` — the `PARTITION` arm flattening only the ok-block of the `(oks, faults)` split and threading the fault block through, gated on `by is Disposition.PARTITION` so the block and tuple return shapes never cross; `drift` filters the `binding` scan's spans to `fact.capture == "name" and fact.text in canonical`, folds them into the persistent `Map[str, frozenset[Lang]]` through `Block.fold`/`Map.change` reading `fact.text`/`fact.locus.lang` by name so a legitimately distinct same-named concept in one namespace yields no defect and a re-minted identity seed, receipt rail, or capability descriptor across Python and TypeScript yields one `drift` row per binding location of a multi-namespace name — a false positive filtered by namespace multiplicity, never a blanket name match, and the fold persistent rather than a mutated accumulator. `ApiCatalogue.reflect` binds the canonical `reliability/faults#FAULT` `@trapped("reflect", catch=ImportError)` aspect (the faults owner's signature-preserving `_guard`/`async_boundary`-weaving definition-time decorator per the `surfaces-and-dispatch#ASPECTS` law, the decorator-aspect form over an inline `boundary` lambda at the call site) so the lift returns `RuntimeRail` with `metadata`'s raise converted to a `BoundaryFault` `import_` row exactly once through a single `catch=ImportError` — the aspect re-tags nothing and passes no tuple of exception types, since the one `CLASSIFY` table already routes both the `PackageNotFoundError` subclass and a bare `ImportError` to the `import_` case, so a second `map_error` reaching into a non-`boundary` case slot and a hand-rolled local wrapper duplicating `trapped` are the deleted forms; the `MemberFact` carries the `dist.version` and the `ep.group` read off the metadata surface rather than a caller-passed `family` constant repeated per member, never raised into the fold. `EvidenceScan.contribute` folds the scan's `Block[Evidence]` into a per-tag count `Map` through `Map.change` keyed `f"count.{ev.tag}"` and drains the `drift` rows through one `Block.choose` yielding `f"drift.{name}"`-keyed pairs, the disjoint `count.*` and `drift.*` prefixes keeping any drifting symbol name (even one literally named `member`/`span`/`drift`/`count`) from colliding with a tally key in the merged flat fact map, then mints one `emitted` `Receipt` through `Receipt.of` the `observability/receipts#RECEIPT` `Signals.emit` consumes, so the scan satisfies the `ReceiptContributor` Protocol structurally exactly as the sibling `evidence/identity#IDENTITY` `SeedReproduction` does; the `canonical` set arrives as the shared one-name-one-owner registry the topology law owns, never re-minted here.
- Packages: `importlib.metadata` (`distribution`, `Distribution.entry_points`, `Distribution.version`, `EntryPoint.group`/`EntryPoint.name`, `PackageNotFoundError`), `tree-sitter` (`Language`/`Parser.parse`/`Query`/`QueryCursor(query, match_limit=budget)`/`QueryCursor.captures`/`set_byte_range`/`set_max_start_depth`/`did_exceed_match_limit`/`Point`/`Node.text`/`Node.start_point`/`Node.start_byte`/`Node.end_byte`/`Node.descendant_count`), `tree-sitter-python` (`language`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`), `tree-sitter-typescript` (`language_typescript`/`language_tsx`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`), `expression` (`Ok`/`Error`/`Some`/`Nothing`/`Block.of_seq`/`Block.cons`/`Block.choose`/`Block.filter`/`Block.collect`/`Block.map`/`Block.fold`/`Map.of_seq`/`Map.empty`/`Map.filter`/`Map.map`/`Map.change`/`Option.default_value`/`tagged_union`/`case`/`tag`), `msgspec` (`Struct`, `Meta` the `Byte` offset bound, `gc=False` on the container-free `Locus`/`SpanFact`/`MemberFact`/`DriftFact` leaves), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.set_attributes`/`Span.is_recording`/`Span.set_status`/`Status`/`StatusCode` the `code.query` structural-scan span with a total OK/ERROR status), `rasm.runtime.faults` (`RuntimeRail`/`BoundaryFault`/`Disposition`/`trapped`/`traversed` — the `@trapped` import-lift aspect, the `Disposition`-keyed traversal), `rasm.runtime.receipts` (`Receipt` the `EvidenceScan` contribution mints).
- Growth: a new evidence family is one `Evidence` case carrying its own value object plus one match arm; a new language is one `Lang` member, one `GRAMMARS` `Grammar` row binding its capsule and reused parser, and one own-language source column per `PROBE_SOURCES` probe (its `binding` declaration alternation plus its grammar-bundled `tags`/`highlights`), each compiling once against the new grammar through the `Map.filter`-total `CompiledProbe.of`; a new structural probe is one `Probe` literal member plus one `PROBE_SOURCES` row carrying its per-grammar source, reached by `scan` with zero new entry; a new traversal output shape is one `Disposition` member the faults owner adds, threaded by `scan`'s `by` parameter; a new canonical name is one entry the caller adds to the `canonical` set; a new mined member field is one field on the `MemberFact` value object; the per-tag scan tally absorbs a new evidence case through the existing `Map.change` fold with zero new arm; zero new surface, zero new parser class, zero per-language parser branch.
- Boundary: no package version tables in planning pages, no guessed environment status, no parallel canonical-name registry minted here, one `rasm.runtime.evidence` tracer and one fault rail (the `code.query` span is this page's single derivation span, never a second tracer per probe); a source cannot name a member absent from the catalogue evidence; the deleted forms are per-parse `Parser`/`Query` construction, positional untyped tuple payloads read by `ev.span[2]` index where a value object names the field, a `(lang, point, start_byte)` triple re-declared on both `SpanFact` and `DriftFact` where one shared `Locus` carries the extent, a re-minted `tuple[int, int]` `Point2` alias where the package owns the `tree_sitter.Point` value object, a `sources[lang]` `KeyError` on a probe missing a language column where the `Map.filter`-then-`map` compile is total over the covered keys, `tags`/`highlights` compiled but unreachable where `scan` makes every probe an `Evidence` producer, a parallel per-corpus `query` plus a `drift`-only fold where one `scan` parameterized over probe/corpus/disposition/output owns the multi-grammar traversal, a bare widened `RuntimeRail[Block[R]] | RuntimeRail[tuple[...]]` `scan` return forcing every caller to re-narrow where the `@overload` arms keyed on the `Disposition` `Literal` carry the precise per-disposition output type the faults owner's `traversed` carries, an `into: Into[R] = ...` default on a `scan` overload binding `R` to `Evidence` where the `into`-omitted arms pin `Block[Evidence]` and only the `into`-supplied arms range over `R` (a defaulted generic projector leaves `R` unbound at the omitted call), a `traversed(accumulate=True)` keyword the faults owner does not expose where `by=Disposition.ACCUMULATE` is the one disposition-keyed traversal, a post-query `match (did_exceed_match_limit, descendant_count >= budget)` that runs the unbounded `captures` BEFORE the deadline check so a `progress_callback` abort lands in the clean arm as a silently clipped result where the static `descendant_count` pre-gate short-circuits the oversized tree before the query and the post-query grade reads only `did_exceed_match_limit`, a `QueryCursor` left on the dead default `match_limit=0xFFFFFFFF` where `match_limit=budget` makes the truncation grade live rather than a silent clip onto a clean `Ok`, a `progress_callback` decrementing a mutable local cell where the pure `step < budget` predicate reads `tree-sitter`'s supplied count, a `catch=(PackageNotFoundError, ImportError)` tuple and a second `map_error` re-tag indexing a non-`boundary` case slot (the faults-owner `CLASSIFY` `ImportError` row already lands the `import_` case from `catch=ImportError`, so the redundant re-tag both crashes on the unset `boundary` slot and duplicates the dispatch), a raised `PackageNotFoundError` crossing into domain flow, a caller-passed `family` constant repeated per `MemberFact` where `ep.group` carries the real entry-point family, a hand-rolled local `reflected` wrapper duplicating the canonical `@trapped` faults aspect, a single-caller `_outcome`/`_drift_facts`/`_grade` helper threading wide positional state where the grading folds into `run`, a mutated `Map` accumulator in the drift fold, a `drift` projector re-minting a throwaway `Evidence` only to read its `.span` slot where `SpanFact.of` is the shared extent primitive `Evidence.of` itself delegates to, node-type string matching in a recursion where the compiled query fits, a blanket same-name match without namespace multiplicity, a flat receipt fact map where a drifting symbol named for a tag collides with a count key absent the disjoint `count.*`/`drift.*` prefixes, a `code.query` span left `UNSET` on a fault arm where the trace status must match the rail outcome, an attribute computed without an `is_recording()` gate, a private evidence log where the `EvidenceScan` `ReceiptContributor` streams onto the one receipt rail, and a second structural-search owner; the structural-parsing, drift, and API surfaces emit `Evidence` the `assay code` rail consumes, never a competing search owner, and the `canonical` registry and the per-run `assay code` receipt are upstream/downstream owners this page reads, never re-mints.

```python signature
from collections.abc import Callable, Iterable
from importlib import metadata
from typing import Annotated, Final, Literal, overload

from expression import Block, Error, Nothing, Ok, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Meta, Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from tree_sitter import Language, Node, Parser, Point, Query, QueryCursor
import tree_sitter_python as ts_py
import tree_sitter_typescript as ts_ts

from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, trapped, traversed
from rasm.runtime.receipts import Receipt


type Lang = Literal["python", "typescript", "tsx"]
type Probe = Literal["binding", "tags", "highlights"]
type Byte = Annotated[int, Meta(ge=0)]
type Corpus = Block[tuple[Lang, bytes]]
type Into[R] = Callable[[Lang, str, Node], R]

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


# --- [MODELS] ---------------------------------------------------------------------------


class Locus(Struct, frozen=True, gc=False):
    lang: Lang
    point: Point
    start_byte: Byte
    end_byte: Byte

    @staticmethod
    def of(lang: Lang, node: Node) -> "Locus":
        return Locus(lang, node.start_point, node.start_byte, node.end_byte)


class SpanFact(Struct, frozen=True, gc=False):
    capture: str
    text: str
    locus: Locus

    @staticmethod
    def of(lang: Lang, capture: str, node: Node) -> "SpanFact":
        return SpanFact(capture, node.text.decode() if node.text is not None else "", Locus.of(lang, node))


class MemberFact(Struct, frozen=True, gc=False):
    distribution: str
    version: str
    import_name: str
    group: str
    symbol: str


class DriftFact(Struct, frozen=True, gc=False):
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
    def of(lang: Lang, capture: str, node: Node) -> "Evidence":
        return Evidence(span=SpanFact.of(lang, capture, node))


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

    @staticmethod
    def of(name: Probe, sources: "Map[Lang, str]", grammars: "Map[Lang, Grammar]") -> "CompiledProbe":
        covered = grammars.filter(lambda lang, _grammar: lang in sources)
        return CompiledProbe(name=name, queries=covered.map(lambda lang, grammar: Query(grammar.language, sources[lang])))


GRAMMARS: Final[Map[Lang, Grammar]] = Map.of_seq(
    (lang, Grammar.of(lang, capsule))
    for lang, capsule in (
        ("python", ts_py.language()),
        ("typescript", ts_ts.language_typescript()),
        ("tsx", ts_ts.language_tsx()),
    )
)
PROBE_SOURCES: Final[Map[Probe, Map[Lang, str]]] = Map.of_seq(
    (name, Map.of_seq(rows))
    for name, rows in (
        ("binding", (("python", _BINDING_PY), ("typescript", _BINDING_TS), ("tsx", _BINDING_TS))),
        ("tags", (("python", ts_py.TAGS_QUERY), ("typescript", ts_ts.TAGS_QUERY), ("tsx", ts_ts.TAGS_QUERY))),
        ("highlights", (("python", ts_py.HIGHLIGHTS_QUERY), ("typescript", ts_ts.HIGHLIGHTS_QUERY), ("tsx", ts_ts.HIGHLIGHTS_QUERY))),
    )
)
PROBES: Final[Map[Probe, CompiledProbe]] = PROBE_SOURCES.map(lambda name, sources: CompiledProbe.of(name, sources, GRAMMARS))


# --- [OPERATIONS] -----------------------------------------------------------------------


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
        budget: int = 1 << 20,
    ) -> RuntimeRail[Block[R]]:
        # `match_limit=budget` makes `did_exceed_match_limit` a LIVE truncation grade — the
        # default `0xFFFFFFFF` cap never trips, so the `resource` arm would be dead and a
        # match-explosion would return silently clipped captures as a clean `Ok`.
        cursor = QueryCursor(PROBES[probe].queries[lang], match_limit=budget)
        if max_depth is not None:
            cursor.set_max_start_depth(max_depth)
        if byte_range is not None:
            cursor.set_byte_range(*byte_range)

        with _TRACER.start_as_current_span("code.query") as scope:
            if scope.is_recording():
                scope.set_attributes({"evidence.probe": probe, "evidence.lang": lang})
            tree = GRAMMARS[lang].parser.parse(source)
            # Two observable axes bound the run and grade it: `descendant_count` (static tree
            # size, gated FIRST so the query never starts on an oversized tree) and the
            # `match_limit=budget` post-query `did_exceed_match_limit`. `progress_callback` is the
            # third axis — a hard step ceiling against pathological query backtracking on an
            # in-budget tree — whose abort tree-sitter surfaces as a partial result, not a grade;
            # the size pre-gate is sized to keep that abort unreachable in practice, so the ceiling
            # is a hang guard rather than a routine outcome the rail decides on.
            visited = tree.root_node.descendant_count
            if visited >= budget:
                fault = BoundaryFault(deadline=(f"{probe}:{lang}", float(budget)))
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            captures = cursor.captures(tree.root_node, progress_callback=lambda step: step < budget)
            truncated = cursor.did_exceed_match_limit
            if scope.is_recording():
                scope.set_attributes({"evidence.nodes": visited, "evidence.captures": sum(map(len, captures.values())), "evidence.truncated": truncated})
            if truncated:
                fault = BoundaryFault(resource=(f"{probe}:{lang}", "match-limit"))
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            scope.set_status(Status(StatusCode.OK))
            return Ok(Block.of_seq(into(lang, name, node) for name, nodes in captures.items() for node in nodes))

    @overload
    @staticmethod
    def scan(probe: Probe, corpus: Corpus, *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...) -> RuntimeRail[Block[Evidence]]: ...
    @overload
    @staticmethod
    def scan(probe: Probe, corpus: Corpus, *, by: Literal[Disposition.PARTITION], budget: int = ...) -> RuntimeRail[tuple[Block[Evidence], Block[BoundaryFault]]]: ...
    @overload
    @staticmethod
    def scan[R](probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...) -> RuntimeRail[Block[R]]: ...
    @overload
    @staticmethod
    def scan[R](probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.PARTITION], budget: int = ...) -> RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]: ...
    @staticmethod
    def scan[R](
        probe: Probe,
        corpus: Corpus,
        into: Into[R] = Evidence.of,
        *,
        by: Disposition = Disposition.ACCUMULATE,
        budget: int = 1 << 20,
    ) -> RuntimeRail[Block[R]] | RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]:
        # The `by` row selects the multi-grammar output shape exactly as the faults owner's
        # `traversed` overloads carry it, so a caller narrows on the disposition it passes
        # rather than re-matching the runtime union the bare widened return would force.
        rails = corpus.map(lambda row: GrammarRegistry.run(row[0], row[1], probe, into, budget=budget))
        flatten: Callable[[Block[Block[R]]], Block[R]] = lambda nested: nested.collect(lambda out: out)
        if by is Disposition.PARTITION:
            return traversed(rails, by=by).map(lambda split: (flatten(split[0]), split[1]))
        return traversed(rails, by=by).map(flatten)

    @staticmethod
    def drift(corpus: Corpus, canonical: frozenset[str]) -> RuntimeRail[Block[Evidence]]:
        return GrammarRegistry.scan("binding", corpus, SpanFact.of).map(
            lambda spans: GrammarRegistry._cross(spans.filter(lambda fact: fact.capture == "name" and fact.text in canonical))
        )

    @staticmethod
    def _cross(spans: Block[SpanFact]) -> Block[Evidence]:
        seed: Map[str, frozenset[Lang]] = Map.empty()
        bindings = spans.fold(
            lambda acc, fact: acc.change(fact.text, lambda b: Some(b.default_value(frozenset()) | {fact.locus.lang})),
            seed,
        )
        return spans.choose(
            lambda fact: Some(Evidence(drift=DriftFact(fact.text, bindings[fact.text], fact.locus)))
            if len(bindings[fact.text]) > 1
            else Nothing
        )


class ApiCatalogue:
    @staticmethod
    @trapped("reflect", catch=ImportError)
    def reflect(distribution: str) -> Block[Evidence]:
        dist = metadata.distribution(distribution)
        import_name = distribution.replace("-", "_")
        version = dist.version
        return Block.of_seq(
            Evidence(member=MemberFact(distribution, version, import_name, ep.group, ep.name)) for ep in dist.entry_points
        ).cons(Evidence(member=MemberFact(distribution, version, import_name, "import", import_name)))


# --- [COMPOSITION] ----------------------------------------------------------------------


class EvidenceScan(Struct, frozen=True):
    owner: str
    evidence: Block[Evidence]

    def contribute(self) -> Iterable[Receipt]:
        # The per-tag `Map.change` fold tallies every case under `count.{tag}`; `Block.choose`
        # drains the `drift` arm into `(drift.{name}, langs)` pairs, the disjoint `count.*`/`drift.*`
        # prefixes keeping a symbol named for a tag from colliding with a tally key in the flat map.
        counts = self.evidence.fold(
            lambda acc, ev: acc.change(f"count.{ev.tag}", lambda n: Some(n.default_value(0) + 1)), Map.empty()
        )
        drifts = self.evidence.choose(
            lambda ev: Some((f"drift.{ev.drift.name}", ",".join(sorted(ev.drift.bindings)))) if ev.tag == "drift" else Nothing
        )
        facts: dict[str, object] = dict(counts.items()) | dict(drifts)
        return (Receipt.of(self.owner, ("emitted", "code.scan", facts)),)
```

## [03]-[RESEARCH]

[TREE_SITTER_QUERY] and [DRIFT_GRAMMARS] are reflection-confirmed against `runtime/.api/tree-sitter.md`: the `tree-sitter` binding compiles patterns through the `Query(language, source)` constructor (the deprecated `Language.query(source)` shim is refused) once per grammar at registry build, and `run` executes captures through `QueryCursor(query, match_limit=budget).captures(node, progress_callback=...)` returning the flattened `dict[str, list[Node]]`. The run scopes with `set_byte_range`/`set_max_start_depth` and grades on two observable axes: the static tree-size pre-gate and the post-query match-limit grade. The `deadline` arm is a deterministic pre-gate: `tree.root_node.descendant_count >= budget` is a static tree-size signal known the instant the parse returns, so an oversized tree short-circuits to the `deadline` fault BEFORE the query runs, and the budget bound is deterministic and pure without observing any callback invocation count. The bounded `captures` call therefore executes only within budget, and the `QueryCursor` carries `match_limit=budget` so `did_exceed_match_limit` is a LIVE truncation grade (the default `0xFFFFFFFF` cap never trips and would leave the `resource` arm dead). The modern `progress_callback` — the single-`int`-arg callback receiving `tree-sitter`'s own running count and returning `step < budget`, a pure predicate rather than a decrementing local cell, the `@deprecated` `timeout_micros` refused — is the step-ceiling hang guard the `tree-sitter` bound law mandates against pathological query backtracking on an in-budget tree; its abort surfaces as a tree-sitter partial result, not a grade, so the size pre-gate is sized to keep it unreachable. The one post-query grade is `did_exceed_match_limit`, which flags match-limit truncation (the `resource` arm); a clean run sets `Status(StatusCode.OK)` and returns the captured `Block`. A `Query` source naming a node kind absent from its grammar's symbol table raises `QueryError` (a `ValueError`) at compile time, so each probe carries a PER-GRAMMAR source: the `binding` declaration alternation is `_BINDING_PY` for the Python grammar (`class_definition`/`type_alias_statement`) and `_BINDING_TS` for both TypeScript dialects (`class_declaration`/`interface_declaration`/`type_alias_declaration`), and `tags`/`highlights` pull each grammar's own bundled source — a single cross-grammar `[...]` alternation, or the Python `TAGS_QUERY` compiled against the TypeScript grammar, would not compile. Each per-grammar source scopes captures to that grammar's own declaration nodes so node-type string matching in a recursion is replaced by the compiled query, and `Node.text`/`start_point` (the `tree_sitter.Point` value object)/`start_byte`/`end_byte` read the bound text and the `Locus` extent off the node rather than recomputing from the source. `GrammarRegistry.scan` runs ANY probe — `binding`, `tags`, or `highlights` — through the one entry, mapping `run` across the `Corpus` under a `Disposition`-keyed `traversed` fold, so the bundled `tags`/`highlights` sources are first-class `Evidence` producers rather than compiled-but-unreachable columns. The grammar pointers are `tree_sitter_python.language()`, `tree_sitter_typescript.language_typescript()`, and `tree_sitter_typescript.language_tsx()`, each wrapped once via `tree_sitter.Language(...)` into a reused `Parser`, and the bundled `TAGS_QUERY`/`HIGHLIGHTS_QUERY` `Final[str]` sources (`tree_sitter_python` ships highlights/tags; `tree_sitter_typescript` ships highlights/locals/tags shared by both dialects) load lazily through `importlib.resources` on first attribute access. No open RESEARCH seam remains on this page.

# [PY_GEOMETRY_IFC_SELECTOR]

The validated element-selection grammar admitting a structured query before `ifcopenshell.util.selector.filter_elements` — the typed boundary the `geometry:ifc/analysis.md#ANALYSIS` quantity/pset arms and the `geometry:ifc/costing.md#LIFECYCLE` take-off arm thread their free-form `query` string into. `IfcSelector` compiles one `lark` EBNF grammar that is faithful terminal-by-terminal to the upstream `ifcopenshell.util.selector.filter_elements_grammar` — the `+`-unioned `facet_list` of `,`-chained `facet`s over the eleven IFC selection facets (`instance`, `entity`, `attribute`, `type`, `material`, `classification`, `location`, `group`, `parent`, `property`, `query`) — folds the parse `Tree` through one `SelectorTransformer` into a frozen `SelectorQuery` of `Facet` cases, and lifts an `UnexpectedInput` parse failure into the `RuntimeRail` through the `reliability/faults#FAULT` `boundary` fence at admission, so a malformed selector is a typed `BoundaryFault(boundary=(subject, detail))` whose `subject` names the offending query at the fence, never a silent empty `filter_elements` match three arms deep. The validated query carries its canonical `filter_elements` string back out through one self-projecting `Facet.render` fold, and that re-serialized string round-trips: the upstream engine re-accepts every `filter_string` this owner emits, so the grammar admits and re-serializes the selection without standing up a second selection engine — `ifcopenshell` runs the filter, `lark` owns the closed query vocabulary the string parses against.

The grammar is not re-invented: it mirrors the upstream rule shapes, operators, and value forms exactly so a query the real engine accepts this owner accepts and a string this owner renders the real engine accepts. The two semantic operators are upstream's, not a private dialect — `+` unions `facet_list` groups (`get_results` folds `|=` across the appended lists) and `,` chains additive/subtractive facets within one list against a running set; there is no juxtaposition-intersection, no `@` decomposition operator, no leading-`/` pset prefix, and no `classification.<system>=<code>` qualifier, because none exist upstream. The contains operator is `*=` and negation is the upstream `!` prefix on either an identifier (`!IfcSlab`) or a comparison (`Name!=Foo`).

The parse is not a bare call: `SelectorQuery` is itself the `observability/receipts#RECEIPT` `ReceiptContributor`, and `@receipted(_REDACTION)` is the egress aspect over the private `_emit` harvest point the rail's `Ok` arm maps through — `SelectorQuery.contribute` yields the validated-axis facts and the decorator emits them on exit through `Signals.emit`, so the parse-once gate the two siblings share streams its admission/rejection fact and a child span without an inline `emit` threaded through `parse`. The grammar compiles once: the `@cache`-memoized `_engine` builds the `Lark` parser and the `SelectorTransformer` on first parse so the boundary reuses one handle across every query rather than rebuilding the EBNF per call, and `Transformer_NonRecursive` folds the wide `+`/`,` facet spine without a Python recursion limit on a whole-model selector.

## [01]-[INDEX]

- [01]-[SELECTOR]: the one `lark`-grammar selector surface — the EBNF grammar constant faithful to `filter_elements_grammar`, the `Facet` `@tagged_union` row algebra whose case carries the facet shape and whose self-projecting `render` re-serializes to the `filter_elements` string, the `SelectorComparison` comparison value object owning the operator/negate/value triple and its own `render`, the `Transformer_NonRecursive` fold to `SelectorQuery`, the `parse` boundary lifting `UnexpectedInput` through the `CLASSIFY` table into the `RuntimeRail` under the `@receipted` egress aspect, and the `filter` leg the validated query drives `ifcopenshell.util.selector.filter_elements` through.

## [02]-[SELECTOR]

- Owner: `IfcSelector` — the `@staticmethod` boundary capsule whose `@cache`-memoized `_engine` builds the `Lark` parser plus the `SelectorTransformer` once on first parse, exposing the polymorphic `parse` entry, the `filter` leg, and the private `@receipted` `_emit` harvest point, mirroring the sibling `IfcAnalysis`/`IfcLifecycle`; `Facet` the `@tagged_union(frozen=True)` row algebra collapsing the eleven upstream facets onto four cases by shared shape — `identified` carrying `(IdentifyAxis, str, bool)` (the `instance`/`entity` axis, the GlobalId-or-IfcClass identifier, and a negate flag) since both are a negatable bare identifier, `attribute` carrying `(str, SelectorComparison)` (a capital-initial attribute name plus a comparison), `keyed` carrying `(SelectorKeyword, SelectorComparison)` since `type`/`material`/`classification`/`location`/`group`/`parent` are one `keyword comparison value` shape discriminated by the keyword token, and `qualified` carrying `(QualifyAxis, str, str | None, SelectorComparison)` (the `property`/`query` axis, the pset/keys head, the optional property-name tail, and a comparison) since both qualify a comparison by a dotted path — so four cases span eleven facets rather than eleven parallel cases or the prior five fabricated axes; `SelectorComparison` the frozen `msgspec.Struct` value object carrying `(SelectorOperator, bool, str)` (operator, negate, value) and owning the comparison `render`, so the operator/negate/value triple is one typed carrier every comparing facet shares rather than three loose fold-positional children re-discriminated per case; `SelectorQuery` the frozen `msgspec.Struct` the fold produces, holding `tuple[tuple[Facet, ...], ...]` facet groups, owning the `filter_string`/`axes`/`span_facts` projections, and implementing the runtime `ReceiptContributor` Protocol (`contribute -> Iterable[Receipt]`) so it is itself the receipt the `@receipted` aspect harvests — no parallel `SelectorReceipt` carrier; `SelectorOperator`/`IdentifyAxis`/`QualifyAxis`/`SelectorKeyword` the closed operator, identify-axis, qualify-axis, and keyword `StrEnum` vocabularies; `SelectorTransformer` the `lark.Transformer_NonRecursive` subclass folding the parse `Tree` bottom-up.
- Cases: the grammar's `start` is a single `filter_group`, a `+`-union of `facet_list` groups, each a `,`-chain of `facet`s over the eleven axes. Each `facet` folds to one `Facet` case — `identified` (a GlobalId `instance` or an IfcClass `entity`, optionally `!`-negated), `attribute` (a capital-initial attribute name plus a comparison against a value), `keyed` (one of the six `keyword comparison value` facets), and `qualified` (a `property` pset-`.`-prop predicate or a `query:keys` predicate) — matched by `match`/`assert_never` on both the fold and the `render` re-serialization, never an open string re-discriminated by an `axis` field at the boundary. `Facet.render` is the one total `match` projecting each case to its `filter_elements` fragment through the `SelectorComparison.render` the comparing cases delegate to, mirroring the `geometry:ifc/analysis.md#ANALYSIS` `AnalysisRow.facts` self-projecting row; the rendered fragment round-trips — the upstream engine re-accepts it.
- Entry: `IfcSelector.parse` is polymorphic over input — a single selector `str` parses one query, an `Iterable[str]` folds through `reliability/faults#FAULT` `traversed(..., by=Disposition.ABORT)` into one `RuntimeRail[Block[SelectorQuery]]` so a batch of arm selectors validates under one rail short-circuiting on the first malformed member, never a per-arm parse loop. The single-string arm returns `RuntimeRail[SelectorQuery]` via `boundary(f"selector.parse:{text}", ...)`; the thunk runs `parser.parse(text)` then `transformer.transform(tree)`, so an `UnexpectedInput` (`UnexpectedToken`/`UnexpectedCharacters`/`UnexpectedEOF`) raised by the lexer/parser is the one cause `boundary` lifts — the `CLASSIFY` catch-all lands it in the `boundary` case whose `detail` is `str(cause) or type(cause).__name__`, and `UnexpectedInput.__str__` renders the offending-token-plus-context window, so the parse-error message rides the `detail` slot the `rejected` projection carries. No dedicated `lark` `CLASSIFY` row is added: the universal faults owner rows only branch-universal infra families, and a geometry-domain `lark` import into the runtime fault owner would invert the strata dependency — a parse failure is exactly the unclassified domain exception the catch-all owns, now message-carrying. A `CLASSIFY` builder receives only `(subject, cause)` and never the source `text`, so the offending query rides the fence `subject` the receipt's `BoundaryFault.facts()["subject"]` carries — the malformed selector is named at the boundary rather than a bare `selector.parse`, never a re-raised string. Threading the lexer's raw `pos_in_stream` integer separately into the rejected receipt is the one remaining faults enhancement: it needs the source `text` at the classifier, which a `(subject, cause)` builder cannot reach, a deferred faults-owner edit (never a runtime→`lark` coupling). `SelectorQuery.filter_string` re-serializes the validated query to the exact `filter_elements` grammar `ifcopenshell` consumes, so the analysis/costing arms call `IfcSelector.filter(model, query)` — `parse(query).map(lambda q: tuple(filter_elements(model, q.filter_string)))` — validated once, never a raw passthrough.
- Auto: `@receipted(_REDACTION)` decorates the private `_emit(query) -> query` harvest point — the same shape the analysis sibling's `_emit` leg takes — so the aspect emits `SelectorQuery.contribute()` on the `Ok` exit and `parse` maps its boundary `Ok` arm through `_emit`, threading no `Signals.emit` through its body; `filter` emits transitively because it composes `parse`, never a second decorated leg. The contributed `emitted`-phase fact carries the parsed axis frozenset, the facet-group count, and the validated `filter_string`, and `span_facts` maps the same native `dict[str, object]` onto the active span the faults owner records a parse fault on, so an admitted selector logs at `emitted` and a malformed one rides the `BoundaryFault.facts()` `rejected` projection — the `{tag, subject, detail}` slot map whose `subject` names the offending query at the fence — through the one chain. The parser is `Lark(SelectorGrammar, start="start", parser="earley")` behind the `@cache`-memoized `_engine` — `earley` for the ambiguous `+`/`,`/predicate selection grammar (the algorithm upstream itself builds with), the default `auto` lexer the Earley parser resolves to its dynamic lexer, and the `@cache` memoization compiling the EBNF once on first parse rather than eagerly at module import or per call; `cache=` is NOT set, since `lark` rejects parser-cache serialization for any parser other than `lalr` (`Lark(..., parser="earley", cache=True)` raises `ConfigurationError` at construction), and the `@cache`-memoized accessor is the build-once mechanism the grammar needs. `SelectorTransformer` subclasses `Transformer_NonRecursive` so the wide `+`/`,` facet spine of a whole-model selector folds iteratively without a Python recursion limit.
- Packages: `lark` (`Lark(grammar, start="start", parser="earley")` the build-once parser mirroring upstream's algorithm, `Transformer_NonRecursive().transform(tree)` the iterative bottom-up fold, `v_args(inline=True)` binding rule children as positional fold arguments, `UnexpectedInput.pos_in_stream`/`get_context(text)` the offending-column scalar and human-readable window a future faults `lark` row could carry once the source text reaches the classifier, the algorithm/lexer a constructor knob never a parser-per-algorithm family; `cache=` excluded because it is `lalr`-only), `ifcopenshell` (`util.selector.filter_elements(ifc_file, query, elements=None, edit_in_place=False)` consuming the re-serialized `filter_string`, returning a `set[entity_instance]` the `filter` leg fixes to a `tuple` — the only selection engine), `reliability/faults#FAULT` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`FAULT_CONF` — the parse fault rail folding `UnexpectedInput` through the `CLASSIFY` catch-all to the `boundary` case whose `detail` preserves the `str(cause)` parse-error window, the batch fold, and the shared `BeartypeConf` the contract guard binds; no dedicated `lark` `CLASSIFY` row, since the universal faults owner never imports a geometry-domain grammar library), `observability/receipts#RECEIPT` (`Receipt`/`ReceiptContributor`/`Redaction`/`Signals.emit` through the `@receipted` aspect), `expression` (`tagged_union`/`case`/`tag` the `Facet` algebra, `Block` the batch carrier), `msgspec` (`Struct` the frozen `SelectorComparison`/`SelectorQuery` value carriers with `gc=False` on the leaf comparison), `beartype` (`@beartype(conf=FAULT_CONF)` the call-boundary contract on `parse`).
- Growth: a new selection facet upstream adds is one EBNF rule alternative, one `Facet` case (or one `SelectorKeyword`/`IdentifyAxis`/`QualifyAxis` row when it folds onto an existing case shape), one `SelectorTransformer` method, and one `render` match arm; a new comparison operator is one `SelectorOperator` row plus one grammar `OP` alternative; zero new surface, no second parser, no per-facet sibling class, no receipt edit.
- Boundary: no hand-rolled regex/split query parser where the grammar owns the structure (the deleted form); no privately re-invented grammar dialect that drifts from upstream — `SelectorGrammar` mirrors `filter_elements_grammar` rule-by-rule and operator-by-operator, so the fabricated `@`-decomposition facet, the leading-`/` pset prefix, the `classification.<system>=<code>` qualifier, the bare-`*` existence token, the `,`-as-union/juxtaposition-as-intersection inversion, and the `DOTTED`-only attribute that rejected a single-segment `Name=Foo` are all removed as forms upstream rejects or misreads; no second selection engine — the validated query re-serializes to the `filter_elements` string `ifcopenshell` owns and the engine re-accepts it; no stringly-typed passthrough of the raw query past admission; no flat `SelectorPredicate` bag re-discriminated by an `axis` field where the `Facet` case carries the per-facet shape; no eleven parallel facet cases where four collapse the shared shapes; no `raise UnexpectedInput(...)` in a fold body where the grammar's `comparison value` rule already bounds the children and the `CLASSIFY` table owns the lift; no `SelectorOperator(str(token))` `ValueError` in the fold where the grammar `OP` terminal is the closed operator set; no `cache=True` on an Earley parser where `lark` raises `ConfigurationError`; a malformed selector is a `BoundaryFault(boundary=...)` whose subject names the offending query at the boundary, never a silent empty match. The `SelectorTransformer` is one `Transformer_NonRecursive` with one method per rule, never a visitor-per-node sibling family.

```python contract
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Final, assert_never, overload

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from lark import Lark, Token, Transformer_NonRecursive, v_args
from msgspec import Struct

from rasm.runtime.faults import FAULT_CONF, Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

if TYPE_CHECKING:
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class SelectorOperator(StrEnum):
    EQ = "="
    GE = ">="
    LE = "<="
    GT = ">"
    LT = "<"
    CONTAINS = "*="


class IdentifyAxis(StrEnum):
    INSTANCE = "instance"  # a 22-char IFC GlobalId
    ENTITY = "entity"  # an `Ifc...` class name


class QualifyAxis(StrEnum):
    PROPERTY = "property"  # `<pset>.<prop> <comparison>`
    QUERY = "query"  # `query:<keys> <comparison>`


class SelectorKeyword(StrEnum):
    TYPE = "type"
    MATERIAL = "material"
    CLASSIFICATION = "classification"
    LOCATION = "location"
    GROUP = "group"
    PARENT = "parent"


# --- [CONSTANTS] -----------------------------------------------------------------------

# Faithful to `ifcopenshell.util.selector.filter_elements_grammar`: `+` unions facet_list
# groups, `,` chains facets within a group, the six `KEYWORD comparison value` facets share
# one rule, and the value/operator/identifier terminals match upstream so a query the real
# engine accepts parses here and a `filter_string` rendered here the real engine re-accepts.
SelectorGrammar: Final[str] = r"""
    start        : filter_group
    filter_group : facet_list ("+" facet_list)*
    facet_list   : facet ("," facet)*

    ?facet       : instance | entity | attribute | keyed | property | query

    instance     : NOT? GLOBALID
    entity       : NOT? IFC_CLASS
    attribute    : ATTR_NAME comparison value
    keyed        : KEYWORD comparison value
    property     : name "." name comparison value
    query        : "query:" name comparison value

    comparison   : NOT? OP
    name         : ESCAPED_STRING | REGEX | UNQUOTED
    value        : SPECIAL | ESCAPED_STRING | REGEX | UNQUOTED

    KEYWORD      : "type" | "material" | "classification" | "location" | "group" | "parent"
    GLOBALID     : /[0-3][a-zA-Z0-9_$]{21}/
    IFC_CLASS    : /Ifc\w+/
    ATTR_NAME    : /[A-Z]\w+/
    OP           : "*=" | ">=" | "<=" | "=" | ">" | "<"
    NOT          : "!"
    SPECIAL      : "NULL" | "TRUE" | "FALSE"
    REGEX        : "/" /[^\/]+/ "/"
    UNQUOTED     : /[^,.=><*!\s]+/

    _STRING_INNER     : /.*?/
    _STRING_ESC_INNER : _STRING_INNER /(?<!\\)(\\\\)*?/
    ESCAPED_STRING    : "\"" _STRING_ESC_INNER "\""
    WS                : /[ \t\f\r\n]/+
    %ignore WS
"""

# the selector facts carry no classified field, so the keep-all policy rides every emit;
# the `observability/receipts#RECEIPT` keep-all `_OPEN` is exactly `Redaction(classified=Map.empty())`.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# the upstream `value`/`name` lexemes: the `SPECIAL` keywords render bare, a `/.../` regex
# renders verbatim, an `UNQUOTED`-clean run renders as a bare token, and anything carrying a
# `UNQUOTED`-terminating delimiter must re-quote as `ESCAPED_STRING` so the rendered facet
# round-trips. The delimiter set is exactly upstream's `unquoted_string` exclusion `[^,.=><*!\s]`
# plus the `"` quote char — `.` IS a delimiter (a decimal value or a `.`-bearing pset name must
# quote: `Length="1.5"`, `"Pset.Weird".Foo`), while `/` and `+` are valid inside a bare token.
_SPECIALS: Final[frozenset[str]] = frozenset({"NULL", "TRUE", "FALSE"})
_TOKEN_DELIMS: Final[frozenset[str]] = frozenset(',.=><*! \t\f\r\n"')

# --- [BOUNDARIES] ----------------------------------------------------------------------


def _emit_token(text: str) -> str:
    # The value/name re-serializer the `render` projections delegate to; depends on the
    # terminal-vocabulary constants above, so it anchors here ahead of the models that read it.
    if text in _SPECIALS or (len(text) >= 2 and text[0] == "/" and text[-1] == "/"):
        return text
    if text and not (set(text) & _TOKEN_DELIMS):
        return text
    return '"' + text.replace('"', '\\"') + '"'


# --- [MODELS] --------------------------------------------------------------------------


class SelectorComparison(Struct, frozen=True, gc=False):
    operator: SelectorOperator
    value: str
    negate: bool = False

    def render(self) -> str:
        return f"{'!' if self.negate else ''}{self.operator.value}{_emit_token(self.value)}"


@tagged_union(frozen=True)
class Facet:
    tag: str = tag()
    identified: tuple[IdentifyAxis, str, bool] = case()  # axis, GlobalId|IfcClass, negate
    attribute: tuple[str, SelectorComparison] = case()  # capital-initial attribute name
    keyed: tuple[SelectorKeyword, SelectorComparison] = case()  # type/material/classification/...
    qualified: tuple[QualifyAxis, str, str | None, SelectorComparison] = case()  # property|query

    def render(self) -> str:
        match self:
            case Facet(tag="identified", identified=(_, identifier, negate)):
                return f"!{identifier}" if negate else identifier
            case Facet(tag="attribute", attribute=(name, comparison)):
                return f"{name}{comparison.render()}"
            case Facet(tag="keyed", keyed=(keyword, comparison)):
                return f"{keyword.value}{comparison.render()}"
            case Facet(tag="qualified", qualified=(QualifyAxis.PROPERTY, pset, prop, comparison)):
                return f"{_emit_token(pset)}.{_emit_token(prop or '')}{comparison.render()}"
            case Facet(tag="qualified", qualified=(QualifyAxis.QUERY, keys, _, comparison)):
                return f"query:{_emit_token(keys)}{comparison.render()}"
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def axis(self) -> str:
        match self:
            case Facet(tag="identified", identified=(axis, _, _)):
                return axis.value
            case Facet(tag="keyed", keyed=(keyword, _)):
                return keyword.value
            case Facet(tag="qualified", qualified=(axis, _, _, _)):
                return axis.value
            case Facet(tag="attribute"):
                return "attribute"
            case _ as unreachable:
                assert_never(unreachable)


class SelectorQuery(Struct, frozen=True, gc=False):
    groups: tuple[tuple[Facet, ...], ...]

    @property
    def axes(self) -> frozenset[str]:
        return frozenset(facet.axis for group in self.groups for facet in group)

    @property
    def filter_string(self) -> str:
        # `+` unions facet_list groups, `, ` chains facets within a group — upstream's two
        # operators, so the rendered string round-trips through `filter_elements_grammar`.
        return " + ".join(", ".join(facet.render() for facet in group) for group in self.groups)

    @property
    def span_facts(self) -> dict[str, object]:
        return {"selector.filter_string": self.filter_string, "selector.axes": sorted(self.axes), "selector.groups": len(self.groups)}

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.ifc.selector", ("emitted", self.filter_string, self.span_facts))


# --- [SERVICES] ------------------------------------------------------------------------


@v_args(inline=True)
class SelectorTransformer(Transformer_NonRecursive):
    def comparison(self, *parts: Token) -> SelectorComparison:
        # `NOT? OP value` — the `value` child is consumed by the owning facet method, so a
        # comparison node carries `(OP,)` or `(NOT, OP)`; the value is threaded in by the caller.
        return SelectorComparison(SelectorOperator(str(parts[-1])), "", len(parts) == 2)

    def name(self, token: Token) -> str:
        return SelectorTransformer._literal(token)

    def value(self, token: Token) -> str:
        return SelectorTransformer._literal(token)

    def instance(self, *parts: Token) -> Facet:
        return Facet(identified=(IdentifyAxis.INSTANCE, str(parts[-1]), len(parts) == 2))

    def entity(self, *parts: Token) -> Facet:
        return Facet(identified=(IdentifyAxis.ENTITY, str(parts[-1]), len(parts) == 2))

    def attribute(self, name: Token, comparison: SelectorComparison, value: str) -> Facet:
        return Facet(attribute=(str(name), SelectorTransformer._bind(comparison, value)))

    def keyed(self, keyword: Token, comparison: SelectorComparison, value: str) -> Facet:
        return Facet(keyed=(SelectorKeyword(str(keyword)), SelectorTransformer._bind(comparison, value)))

    def property(self, pset: str, prop: str, comparison: SelectorComparison, value: str) -> Facet:
        return Facet(qualified=(QualifyAxis.PROPERTY, pset, prop, SelectorTransformer._bind(comparison, value)))

    def query(self, keys: str, comparison: SelectorComparison, value: str) -> Facet:
        return Facet(qualified=(QualifyAxis.QUERY, keys, None, SelectorTransformer._bind(comparison, value)))

    def facet_list(self, *facets: Facet) -> tuple[Facet, ...]:
        return facets

    def filter_group(self, *groups: tuple[Facet, ...]) -> SelectorQuery:
        return SelectorQuery(groups)

    def start(self, query: SelectorQuery) -> SelectorQuery:
        return query

    @staticmethod
    def _bind(comparison: SelectorComparison, value: str) -> SelectorComparison:
        return SelectorComparison(comparison.operator, value, comparison.negate)

    @staticmethod
    def _literal(token: object) -> str:
        text = str(token)
        return text[1:-1] if text.startswith('"') and text.endswith('"') else text


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcSelector:
    @staticmethod
    @cache
    def _engine() -> tuple[Lark, SelectorTransformer]:
        # @cache makes the EBNF compile once on first parse rather than per call or eagerly at
        # module load; `cache=` is omitted because `lark` rejects it for any parser != `lalr`.
        return Lark(SelectorGrammar, start="start", parser="earley"), SelectorTransformer()

    @overload
    @staticmethod
    def parse(text: str) -> "RuntimeRail[SelectorQuery]": ...
    @overload
    @staticmethod
    def parse(text: Iterable[str]) -> "RuntimeRail[Block[SelectorQuery]]": ...
    @staticmethod
    @beartype(conf=FAULT_CONF)
    def parse(text: str | Iterable[str]) -> "RuntimeRail[SelectorQuery] | RuntimeRail[Block[SelectorQuery]]":
        match text:
            case str():
                return IfcSelector._parse_one(text)
            case _:
                return traversed(Block.of_seq(IfcSelector._parse_one(one) for one in text), by=Disposition.ABORT)

    @staticmethod
    def filter(model: "ifcopenshell.file", text: str) -> "RuntimeRail[tuple[ifcopenshell.entity_instance, ...]]":
        import ifcopenshell.util.selector  # noqa: PLC0415

        return IfcSelector.parse(text).map(lambda query: tuple(ifcopenshell.util.selector.filter_elements(model, query.filter_string)))

    @staticmethod
    def _parse_one(text: str) -> "RuntimeRail[SelectorQuery]":
        # The lexer/parser `UnexpectedInput` raised inside the thunk is the one cause `boundary`
        # lifts: the faults `CLASSIFY` catch-all lands it on the `boundary` case. The offending
        # query rides the fence subject the rejected receipt's `BoundaryFault.facts()["subject"]`
        # carries, since a `(subject, cause)` `CLASSIFY` builder never sees the source text.
        return boundary(f"selector.parse:{text}", lambda: IfcSelector._fold(text)).map(IfcSelector._emit)

    @staticmethod
    def _fold(text: str) -> SelectorQuery:
        parser, transformer = IfcSelector._engine()
        return transformer.transform(parser.parse(text))

    @staticmethod
    @receipted(_REDACTION)
    def _emit(query: SelectorQuery) -> SelectorQuery:
        # the @receipted harvest point: the aspect emits `query.contribute()` on the Ok exit, so
        # `parse` threads no `Signals.emit` through its body — mirrors the analysis `_emit` leg.
        return query
```

## [03]-[RESEARCH]

- [SELECTOR_FILTER_GRAMMAR]: the upstream `ifcopenshell.util.selector` source fixes the selector grammar terminal-by-terminal, and `SelectorGrammar` mirrors it: `filter_elements(ifc_file, query, elements=None, edit_in_place=False) -> set[entity_instance]` parses `query` with the module-level `filter_elements_grammar` whose `start: filter_group`, `filter_group: facet_list ("+" facet_list)*`, and `facet_list: facet ("," facet)*` make `+` the group union and `,` the in-group chain — `FacetTransformer.get_results` folds `|=` across the appended `facet_list` results and each facet adds (`|=`) or subtracts (`-=`, on a leading `!`) against the running set, so there is no juxtaposition-intersection operator and intersection is the chained-`elements` call the docstring shows (`filter_elements(file, "IfcDoor", elements)`). The eleven facets are `instance` (a `[0-3][a-zA-Z0-9_$]{21}` GlobalId, negatable), `entity` (an `Ifc\w+` class, negatable), `attribute` (`[A-Z]\w+` name + comparison + value), the six `keyword comparison value` facets `type`/`material`/`classification`/`location`/`group`/`parent`, `property` (`pset "." prop comparison value`, no leading `/`), and `query` (`"query:" keys comparison value`). The comparison is `not? (equals | morethanequalto | lessthanequalto | morethan | lessthan) | not? contains` with `contains` the `*=` operator and `not` the `!` prefix; the value is `special` (`NULL`/`TRUE`/`FALSE`) | `quoted_string` (`ESCAPED_STRING`) | `regex_string` (`/.../`) | `unquoted_string`. The prior page's grammar was fabricated on every axis — a `@<class>` decomposition facet, a leading-`/` pset, a `classification.<system>=<code>` qualifier, a bare-`*` existence token, `,`-as-union with `clause+`-juxtaposition-as-intersection, and a `DOTTED` attribute requiring a dot that rejected a single-segment `Name=Foo` — none of which upstream admits, so its `Clause.render`/`filter_string` re-serialized to strings the real engine rejects (`@`, leading `/`, `classification.X=Y`, bare `*`) or silently misreads (`,`/space). The rebuilt grammar parses every upstream-valid query and renders only upstream-valid strings, verified against the live `filter_elements_grammar`: parse-parity holds across the eleven facets and both operators, and every `SelectorQuery.filter_string` round-trips back through the real engine without a parse failure. The parser admits the query before `filter_elements` runs, so a malformed selector is an `UnexpectedInput` fault at admission, never a silent empty match (`lark.md#51`–`#52` failure/boundary axes).
- [FACET_ROW_ALGEBRA]: the eleven upstream facets collapse onto four `Facet` `@tagged_union(frozen=True)` cases by shared shape rather than eleven parallel cases or the prior five fabricated axes — `identified` carries `(IdentifyAxis, str, bool)` since `instance` and `entity` are one `NOT? <identifier>` shape distinguished only by whether the identifier is a GlobalId or an IfcClass, `keyed` carries `(SelectorKeyword, SelectorComparison)` since `type`/`material`/`classification`/`location`/`group`/`parent` are one `keyword comparison value` shape discriminated by the keyword token, `qualified` carries `(QualifyAxis, str, str | None, SelectorComparison)` since `property` and `query` both qualify a comparison by a dotted path (the `property` pset+prop pair, the `query` keys with a `None` tail), and `attribute` carries `(str, SelectorComparison)` — so the case discriminates the real shape and the axis enum carries the leaf variant, the same per-case-shape discipline the `geometry:ifc/analysis.md#ANALYSIS` `AnalysisRow` and `geometry:ifc/costing.md#LIFECYCLE` `LifecycleRow` tagged unions hold (`expression.md#54`–`#56` `tagged_union`/`tag`/`case`). `SelectorComparison` is the frozen `gc=False` `msgspec.Struct` value object carrying `(operator, value, negate)` and owning the comparison `render`, so the operator/negate/value triple every comparing facet shares is one typed carrier rather than three loose fold-positional children re-discriminated per case (`msgspec.md#147` `gc=False` on a leaf struct of non-container fields). `Facet.render` is the one total `match`/`assert_never` self-projecting each case to its `filter_elements` fragment through `SelectorComparison.render`, so the serialization lives on the row the parse produces rather than a forward reach into an operations-class private — the inverted ownership the prior page carried, removed. The fragment round-trips only because `render` re-serializes through `_emit_token`: the transformer strips a quoted value to its inner text, so a value or pset/prop/keys name carrying a `UNQUOTED`-terminating delimiter (a space such as `location="Level 1"`, a `.`, a comparator char) must re-quote as an `ESCAPED_STRING` on the way out, while a `SPECIAL` keyword renders bare and a `/.../` regex renders verbatim — a bare un-re-quoted emit (`location=Level 1`) is the form upstream rejects at the first delimiter, so the round-trip and idempotence both fail without it.
- [TRANSFORMER_FOLD_BINDING]: the branch `lark` catalogue confirms `Transformer_NonRecursive().transform(tree)` is the iterative bottom-up fold (`lark.md#28`, the deep-tree no-recursion-limit fold the wide `+`/`,` facet spine takes) and `v_args(inline=True)` binds rule children as positional fold-method arguments (`lark.md#31`/`#78`); the `@v_args(inline=True)`-on-class application binding every method, the `Token` `str()` coercion the value/name methods read, and the optional `NOT?` child the `comparison`/`instance`/`entity` methods detect by arity (`(OP,)` versus `(NOT, OP)`) confirm against the installed cp315 `lark==1.3.1` distribution — `lark` is pure-Python cp315-clean, so reflection resolves on the project venv directly with no companion-lane gate (`lark.md#63`/`#107`). The `value` child of each comparing rule is consumed by the owning facet method, not the `comparison` node, so `comparison` mints a `SelectorComparison` with an empty value the facet rebinds through `_bind` once the sibling `value` child is in hand — the upstream `comparison`/`value` split folded into the typed carrier in one pass. The `functools.cache`-memoized `_engine` compiles the EBNF once on first `parse` rather than eagerly at module import or per call, the parser-build-once the `STACKING_LAW` names — the build deferred to a boundary accessor so the pure-Python `lark` import stays the only module-level ecosystem dependency while the heavy `ifcopenshell.util.selector` import is lazy in `filter`. `Lark(..., cache=True)` is NOT used: `lark` raises `ConfigurationError("cache only works with parser='lalr' for now")` for an Earley parser, so the prior page's `cache=True` would crash `_engine` on first call, and the `@cache` accessor is the correct and sufficient build-once mechanism for the ambiguous grammar Earley owns.
- [UNEXPECTED_INPUT_PAYLOAD]: the branch `lark` catalogue confirms `UnexpectedInput` carries `pos_in_stream` and `get_context(text)` (`lark.md#42`, the parse-failure base of `UnexpectedToken`/`UnexpectedCharacters`/`UnexpectedEOF`) and that the failure axis lifts once at the parse boundary (`lark.md#91`). The parse boundary does not re-raise a bare string — the `reliability/faults#FAULT` `CLASSIFY` fold catches the `UnexpectedInput` raised inside `boundary(f"selector.parse:{text}", ...)` and lands it in the subject-carrying `boundary` catch-all case, the `lark.md#91` failure-axis lift made concrete on the one rail. The catch-all `detail` is `str(cause) or type(cause).__name__` (the `faults.md#72` catch-all shape), and `UnexpectedInput.__str__` already renders the offending-token-plus-context window, so the parse-error message — column, expected-token set, and the human-readable context window — rides the `detail` slot the `BoundaryFault.facts()` `rejected` projection carries WITHOUT a dedicated `lark` row: the catch-all preserving `str(cause)` is exactly the parse-failure-message survival a per-`lark` row would add, while the offending query also rides the fence `subject`. A dedicated `lark` `CLASSIFY` row is deliberately NOT added: the universal `reliability/faults#FAULT` owner rows only branch-universal infra families (`anyio`/`msgspec`/`beartype`/stdlib), and importing the geometry-domain `lark` into the runtime fault owner would couple runtime to a domain grammar library against the strata dependency direction — a `lark` parse failure is precisely an unclassified domain exception the catch-all owns, now carrying its message. The remaining faults enhancement is the position-AWARE classify path threading the source `text` to the classifier so the rejected receipt could carry the raw `pos_in_stream` integer separately from the rendered message — a `(subject, cause)` builder cannot reach the source `text`, so it is a deferred faults-owner edit, not an in-page fold and not a runtime→`lark` coupling. No defensive `raise UnexpectedInput(...)` lives in a fold body: the grammar's `comparison value` rule bounds each comparing facet to exactly one comparison and one value, the optional `NOT?` is an arity check the transformer reads without a raise, and `SelectorOperator(str(token))` over a grammar-guaranteed `OP` terminal never reaches a `ValueError`.
- [SELECTOR_RECEIPT_RAIL]: `SelectorQuery` itself implements the `observability/receipts#RECEIPT` `ReceiptContributor` Protocol (`receipts.md#147` `contribute -> Iterable[Receipt]`), so the validated query is its own receipt and no parallel `SelectorReceipt` carrier exists — the same collapse the analysis `AnalysisResult.contribute`/costing `LifecycleReceipt.contribute` take. `@receipted(_REDACTION)` (`receipts.md#286` the `Contributing[P, R: ReceiptContributor]`-bound aspect whose `harvested` arm calls the operation, `Signals.emit`s the returned contributor, and returns it; `#7`/`#22` the `emit`/`emit_async` sink routed off `iscoroutinefunction`) decorates the private `_emit(query: SelectorQuery) -> SelectorQuery` rather than `parse` — `@receipted` harvests the function's *return value* as the contributor, and `parse` returns a `RuntimeRail` (a `Result`, not a `ReceiptContributor`), so the rail's `Ok` arm maps through `_emit` exactly as the analysis sibling's `run` maps its boundary `Ok` through its own `@receipted` `_emit` leg. `SelectorQuery.contribute` yields one `emitted`-phase `Receipt.of("rasm.geometry.ifc.selector", ("emitted", filter_string, span_facts))` (`receipts.md#21`/`#117` the shape-polymorphic `of` over the `(Phase, subject, facts)` triple, never the legacy four-positional form) carrying the parsed `axes` frozenset, the facet-group count, and the validated `filter_string`; `span_facts` feeds the same native `dict[str, object]` the faults owner records a parse fault on. A malformed selector rides the `BoundaryFault.facts()` `rejected` projection (`faults.md#93`/`receipts.md#20`) through the one chain, so the selector boundary contributes evidence on both the admit and reject paths the way the sibling owners' `@receipted` legs do, never a silent untraced gate. `filter` emits transitively through the `parse` it composes, never a second decorated leg. `@beartype(conf=FAULT_CONF)` (`faults.md#127` the shared domain `BeartypeConf(violation_type=BeartypeCallHintViolation)`) is the call-boundary contract so a non-`str`/`Iterable[str]` argument raises the canonical violation the `CLASSIFY` `api` row folds onto the rail rather than a raw `TypeError`.
- [POLYMORPHIC_PARSE_BATCH]: `IfcSelector.parse` discriminates its input shape through one `match` rather than a `parse`/`parse_many` sibling family — a single `str` parses one `SelectorQuery`, an `Iterable[str]` (the analysis `CLASH`/`BCF` `a#b` side pair, a multi-arm costing run) folds each member's `boundary("selector.parse", ...)` rail through `reliability/faults#FAULT` `traversed(..., by=Disposition.ABORT)` (`faults.md#184`/`#189`) into one `RuntimeRail[Block[SelectorQuery]]` short-circuiting on the first malformed member, the `@overload` arms keyed on the input type carrying the per-input output shape so a caller narrows on the argument it passes (`expression.md` `Block` carrier, `faults.md#180`–`#183` the disposition-`Literal` overload pattern). The single-string output stays the `RuntimeRail[SelectorQuery]` the `geometry:ifc/analysis.md#ANALYSIS` and `geometry:ifc/costing.md#LIFECYCLE` arms `.map`/`.bind` over and `SelectorQuery.filter_string` they re-serialize, so the consuming contract is preserved while the batch path replaces a per-arm parse loop at the two callers.
- [IDS_SELECTOR_GRAMMAR]: reconciles the `analysis.md#3-RESEARCH` `IDS_SELECTOR_GRAMMAR` residual without duplicating it — that residual confirms `util.selector.filter_elements` resolves against the branch `ifcopenshell` catalogue for the analysis arms; this page realizes the validated grammar those arms consume, so the analysis/costing `query` string is parsed once through `IfcSelector.parse` and reused across quantity/pset/IDS/clash subject derivation, the unvalidated raw-string boundary the `VALIDATED_SELECTOR_GRAMMAR` idea names closed at admission rather than three arms deep.

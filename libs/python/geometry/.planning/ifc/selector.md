# [PY_GEOMETRY_IFC_SELECTOR]

`IfcSelector` validates an element-selection query before `ifcopenshell.util.selector.filter_elements`: one `lark` EBNF faithful to the upstream `filter_elements_grammar` compiles, the parse `Tree` folds into a frozen `SelectorQuery` of `Facet` cases, and an `UnexpectedInput` parse failure lifts into the `RuntimeRail` at admission, so a malformed selector is a typed `BoundaryFault` whose `subject` names the offending query at the fence, never a silent empty match three arms deep. `ifcopenshell` runs the filter; `lark` owns the closed query vocabulary the string parses against — one grammar admits and re-serializes selection without a second engine.

`ifc/analysis#ANALYSIS` quantity/pset arms, the `ifc/costing#LIFECYCLE` take-off arm, and the `ifc/structural#STRUCTURAL` profile partition thread their free-form `query` through this boundary, driving elements off `IfcSelector.filter`, the only `filter_elements` caller. `SelectorQuery.filter_string` re-serializes the validated query to the exact `filter_elements` grammar and round-trips — the upstream engine re-accepts every string this owner emits, the frozen wire name the siblings pass back — and `SelectorMatch` carries that spelling home beside the match, so every consumer's evidence key names the query the engine actually ran. Parse admits through the `rasm.runtime.faults` `boundary`/`traversed` rail, and `SelectorQuery` is the `rasm.runtime.receipts` contributor the `@receipted` egress aspect harvests, so the parse-once gate the two siblings share streams its admission/rejection fact without an inline emit.

## [01]-[INDEX]

- [02]-[SELECTOR]: one `lark`-grammar selector surface — vocabulary-rendered EBNF, the `Facet` row algebra whose case renders back to the `filter_elements` string, the `parse` boundary lifting `UnexpectedInput` into the `RuntimeRail`, and the `filter` leg driving `filter_elements` into one `SelectorMatch`.

## [02]-[SELECTOR]

- Owner: `IfcSelector` — `@staticmethod` boundary capsule whose `@cache`-memoized `_engine` builds the `Lark` parser and `SelectorTransformer` once, exposing polymorphic `parse`, the `filter` leg, and the private `@receipted` `_emit` point. `Facet` `@tagged_union(frozen=True)` collapses the upstream facets onto four shared-shape cases — `identified` a negatable `instance` GlobalId or `entity` IfcClass, `attribute` a capital-initial name and a comparison, `keyed` the `keyword comparison value` facets, `qualified` a `property`/`query` dotted-path predicate — never a parallel case per facet or a flat `axis`-tagged bag. `SelectorComparison` frozen value object owns the operator/negate/value triple and its `render`, one carrier every comparing facet shares rather than three fold-positional children re-discriminated per case. `SelectorQuery` frozen fold product holds the facet groups, owns the `filter_string`/`axes`/`span_facts` projections, and implements `ReceiptContributor` itself — no parallel `SelectorReceipt`. `SelectorMatch` is the `filter` leg's one product, the validated query beside its element match, so a consumer keys its evidence on the canonical spelling the engine ran without a second parse. `SelectorOperator`/`IdentifyAxis`/`QualifyAxis`/`SelectorKeyword` closed `StrEnum` vocabularies; `SelectorTransformer` the `Transformer_NonRecursive` folding the wide `+`/`,` spine iteratively, no Python recursion limit.
- Cases: grammar `start` is one `filter_group` — a `+`-union of `,`-chained `facet_list`s over upstream's two operators: `+` unions groups (`|=` across the appended lists), `,` chains additive/subtractive facets against a running set. Contains is `*=`, negation the `!` prefix on an identifier or comparison. Each `facet` folds to one `Facet` case matched by `match`/`assert_never` on both the fold and the `render` re-serialization, mirroring the `ifc/analysis#ANALYSIS` `AnalysisRow.facts` self-projecting row.
- Entry: `IfcSelector.parse` is polymorphic — a `str` parses one query, an `Iterable[str]` folds through `traversed(..., by=Disposition.ABORT)` into one `RuntimeRail[Block[SelectorQuery]]` so a batch validates under one rail short-circuiting on the first malformed member, never a per-arm loop. Single-string arm runs `parser.parse` then `transformer.transform` under `boundary(f"selector.parse:{text}", ...)`, so the offending query rides the fence `subject` the rejected receipt's `BoundaryFault.facts()["subject"]` carries — a `CLASSIFY` `(subject, cause)` builder never sees the source `text`.
- Auto: `SELECTOR_GRAMMAR` is an f-string over the Python vocabularies, never a second transcription of them — `_alts` renders the `KEYWORD`/`OP`/`SPECIAL` alternations longest-literal-first off `SelectorKeyword`/`SelectorOperator`/`_SPECIALS` (the length key also fixing the render a `frozenset`'s hash order would otherwise reshuffle per run), and `_delim_class` renders `UNQUOTED`'s negated class off `_TOKEN_DELIMS` with whitespace collapsed to `\s` so the class and the ignored `WS` terminal agree on one whitespace domain. That same class compiles once as `_UNQUOTED`, which IS `_emit_token`'s re-quote test, so a bare-rendered token cannot fall outside the terminal the parser re-accepts it under. Parser is `Lark(SELECTOR_GRAMMAR, start="start", parser="earley")` — Earley for the ambiguous `+`/`,`/predicate grammar, the algorithm upstream itself builds. `cache=` stays unset: `lark` raises `ConfigurationError` on parser-cache serialization for any parser but `lalr`, so the `@cache`-memoized `_engine` compiling the EBNF once on first parse is the build-once mechanism. `@receipted(OPEN)` decorates the private `_emit`; `filter` emits transitively because it composes `parse`, never a second decorated leg.
- Packages: `lark` (`Lark(..., parser="earley")`, `Transformer_NonRecursive().transform`, `v_args(inline=True)`, `UnexpectedInput` — `cache=` excluded, `lalr`-only), `ifcopenshell` (`util.selector.filter_elements` consuming `filter_string`, the only selection engine), `rasm.runtime.faults` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`FAULT_CONF` — no dedicated `lark` `CLASSIFY` row, since the universal faults owner never imports a geometry-domain grammar and a parse failure is exactly the message-carrying catch-all case), `rasm.runtime.receipts`, `expression` (`tagged_union` the `Facet` algebra, `Block` the batch carrier), `msgspec` (`Struct` the frozen `SelectorComparison`/`SelectorQuery`), `beartype` (`@beartype(conf=FAULT_CONF)` on `parse`), stdlib `re` (`escape` rendering the delimiter class, one module-level compiled `Pattern` serving the re-quote test).
- Growth: a new operator is one `SelectorOperator` row, a new keyword one `SelectorKeyword` row, a new special literal one `_SPECIALS` member, and a new delimiter one `_TOKEN_DELIMS` member — the terminal, the re-quote test, and the round-trip all re-render from that one row. A new upstream facet is one EBNF alternative, one `Facet` case (or one `IdentifyAxis`/`QualifyAxis` row when it folds onto an existing shape), one transformer method, and one `render` arm — no second parser, no per-facet sibling class, no receipt edit. Threading the lexer's raw `pos_in_stream` into the rejected receipt needs the source `text` at a classifier the `(subject, cause)` builder cannot reach — a faults-owner edit, never a runtime→`lark` coupling.
- Boundary: no privately re-invented dialect — `SELECTOR_GRAMMAR` mirrors `filter_elements_grammar` rule-by-rule, so fabricated operators, prefixes, and qualifiers upstream rejects never enter; no hand-rolled regex/split parser; no second selection engine past the `filter_string` round-trip; no stringly passthrough of the raw query past admission; no `cache=True` on an Earley parser, and no `SelectorOperator(str(token))` or `raise UnexpectedInput` in a fold body where the grammar terminal already bounds the children. No terminal restates a Python vocabulary as a literal alternation, and no second delimiter set sits beside `_TOKEN_DELIMS`. `parse` and `filter` stay caller-floor by charter — parse is a short pure fold and `filter_elements` an attribute walk over the live in-process model, a pybind11 handle no pickle seam carries, so no lane crossing exists here; any future kernel wrapping a mutating script declares `idempotent=False`.

```python signature
import re
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from typing import Final, assert_never, overload

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block
from lark import Lark, Token, Transformer_NonRecursive, v_args
from msgspec import Struct

lazy import ifcopenshell.util.selector

from rasm.runtime.faults import FAULT_CONF, Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.receipts import OPEN, Receipt, receipted

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

# One vocabulary per axis, single-edit-site: the EBNF terminals below RENDER from these rows, so a new operator,
# keyword, or special literal lands once and the grammar re-derives with it. `_TOKEN_DELIMS` is one set serving two
# renders — the UNQUOTED negated class and the re-quote admission test — so `.` delimits (`Length="1.5"`,
# `"Pset.Weird".Foo`), `/` and `+` stay bare, and `"` drops from the class because ESCAPED_STRING is its own terminal.
_SPECIALS: Final[frozenset[str]] = frozenset({"NULL", "TRUE", "FALSE"})
_WHITESPACE: Final[frozenset[str]] = frozenset(" \t\f\r\n")
_QUOTE: Final[str] = '"'
_TOKEN_DELIMS: Final[frozenset[str]] = frozenset(",.=><*!") | _WHITESPACE | frozenset(_QUOTE)


# Read-before-use: these two renderers build the module constants beneath them, so they seat with the table they
# derive rather than in `[OPERATIONS]` behind the value that calls them.
def _alts(vocabulary: Iterable[str]) -> str:
    # EBNF alternation over one Python vocabulary, longest literal first so `*=` wins the Earley terminal match over
    # `=`; the `(-len, token)` key also FIXES the render, since a `frozenset` iterates in hash order and a
    # hash-seeded grammar string would differ run to run. A `StrEnum` member renders as its value under the f-string.
    return " | ".join(f'"{token}"' for token in sorted(vocabulary, key=lambda token: (-len(token), str(token))))


def _delim_class(delims: frozenset[str]) -> str:
    # negated character class off the SAME delimiter set the re-quote test reads: whitespace collapses to `\s` so the
    # class and the ignored WS terminal agree on one whitespace domain, and the quote drops to its own terminal.
    return rf"[^{re.escape(''.join(sorted(delims - _WHITESPACE - frozenset(_QUOTE))))}\s]+"


# Faithful to `filter_elements_grammar` — `+` unions facet_list groups, `,` chains facets — with every closed-vocabulary
# terminal a render of its Python row, so the string round-trips and no alternation restates a table.
SELECTOR_GRAMMAR: Final[str] = rf"""
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

    KEYWORD      : {_alts(SelectorKeyword)}
    GLOBALID     : /[0-3][a-zA-Z0-9_$]{{21}}/
    IFC_CLASS    : /Ifc\w+/
    ATTR_NAME    : /[A-Z]\w+/
    OP           : {_alts(SelectorOperator)}
    NOT          : "!"
    SPECIAL      : {_alts(_SPECIALS)}
    REGEX        : "/" /[^\/]+/ "/"
    UNQUOTED     : /{_delim_class(_TOKEN_DELIMS)}/

    _STRING_INNER     : /.*?/
    _STRING_ESC_INNER : _STRING_INNER /(?<!\\)(\\\\)*?/
    ESCAPED_STRING    : "\"" _STRING_ESC_INNER "\""
    WS                : /\s/+
    %ignore WS
"""

# The re-quote admission test IS the UNQUOTED terminal, compiled once from the one class render, so a token this owner
# emits bare can never fall outside the class the parser re-accepts it under.
_UNQUOTED: Final[re.Pattern[str]] = re.compile(_delim_class(_TOKEN_DELIMS))

# keep-all redaction — no classified field on the selector facts.

# --- [BOUNDARIES] ----------------------------------------------------------------------


def _emit_token(text: str) -> str:
    # SPECIAL and a `/.../` regex render verbatim; a token the UNQUOTED terminal accepts whole renders bare; every
    # other token — empty, delimiter-bearing, quote-bearing — re-quotes as ESCAPED_STRING so the query round-trips.
    if text in _SPECIALS or (len(text) >= 2 and text[0] == "/" and text[-1] == "/"):
        return text
    return text if _UNQUOTED.fullmatch(text) else _QUOTE + text.replace(_QUOTE, "\\" + _QUOTE) + _QUOTE


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
        return " + ".join(", ".join(facet.render() for facet in group) for group in self.groups)

    @property
    def span_facts(self) -> dict[str, object]:
        return {"selector.filter_string": self.filter_string, "selector.axes": sorted(self.axes), "selector.groups": len(self.groups)}

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.ifc.selector", ("emitted", self.filter_string, self.span_facts))


class SelectorMatch(Struct, frozen=True, gc=False):
    # the validated query travels WITH its match, so a consumer keys its evidence on the canonical `filter_string`
    # the engine actually ran rather than the raw text it handed in, and none re-parses to recover that spelling.
    query: SelectorQuery
    elements: tuple["ifcopenshell.entity_instance", ...]


# --- [SERVICES] ------------------------------------------------------------------------


@v_args(inline=True)
class SelectorTransformer(Transformer_NonRecursive):
    def comparison(self, *parts: Token) -> SelectorComparison:
        # `NOT? OP value` — the facet method threads the `value` in, so this node carries `(OP,)` or `(NOT, OP)`.
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
        # @cache compiles the EBNF once on first parse; `cache=` is omitted — `lark` rejects it for any parser but `lalr`.
        return Lark(SELECTOR_GRAMMAR, start="start", parser="earley"), SelectorTransformer()

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
    def filter(model: "ifcopenshell.file", text: str) -> "RuntimeRail[SelectorMatch]":
        # one parse serves both products: the engine consumes the re-serialized `filter_string` and the caller
        # receives that same validated query beside its match, so no consumer parses twice to name what it ran.
        return IfcSelector.parse(text).map(
            lambda query: SelectorMatch(query=query, elements=tuple(ifcopenshell.util.selector.filter_elements(model, query.filter_string)))
        )

    @staticmethod
    def _parse_one(text: str) -> "RuntimeRail[SelectorQuery]":
        # `UnexpectedInput` is the one cause `boundary` lifts to the `CLASSIFY` catch-all; the offending query
        # rides the fence subject the `(subject, cause)` builder gets, never the source text.
        return boundary(f"selector.parse:{text}", lambda: IfcSelector._fold(text)).map(IfcSelector._emit)

    @staticmethod
    def _fold(text: str) -> SelectorQuery:
        parser, transformer = IfcSelector._engine()
        return transformer.transform(parser.parse(text))

    @staticmethod
    @receipted(OPEN)  # selector facts carry no secret field, so the runtime keep-all policy binds
    def _emit(query: SelectorQuery) -> SelectorQuery:
        # @receipted harvest point — the aspect emits `query.contribute()` on the Ok exit, so `parse` threads no emit.
        return query
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

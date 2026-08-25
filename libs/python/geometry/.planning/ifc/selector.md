# [PY_GEOMETRY_IFC_SELECTOR]

`IfcSelector` validates an element-selection query before `ifcopenshell.util.selector.filter_elements`: one `lark` EBNF faithful to the upstream `filter_elements_grammar` compiles, the parse `Tree` folds into a frozen `SelectorQuery` of `Facet` cases, and an `UnexpectedInput` parse failure translates at the fence onto `IfcFault.unparsed_query` before lifting into the `RuntimeRail`, so a malformed selector reaches a consumer as the offending query beside the stop kind, its offset, and the terminals that were admissible there, never a silent empty match three arms deep. `ifcopenshell` runs the filter; `lark` owns the closed query vocabulary the string parses against — one grammar admits and re-serializes selection without a second engine.

This page also seats `IfcFault`, the whole IFC band's ONE closed refusal family: one case per refusal LAW, each carrying its own coordinate tuple rather than a rendered `(subject, cause)` string a consumer re-parses, and each reaching the rail through the runtime's own conversion door. The seat follows the band's reachability — `analysis`, `costing`, and `structural` already import `IfcSelector`, and this page imports no band sibling — so the floor introduces no cycle and no sixth page.

`ifc/analysis#ANALYSIS` quantity/pset arms, the `ifc/costing#LIFECYCLE` take-off arm, and the `ifc/structural#STRUCTURAL` profile partition thread their free-form `query` through this boundary, driving elements off `IfcSelector.filter`, the only `filter_elements` caller. `SelectorQuery.filter_string` re-serializes the validated query to the exact `filter_elements` grammar and round-trips — the upstream engine re-accepts every string this owner emits, the frozen wire name the siblings pass back — and `SelectorMatch` carries that spelling home beside the match, so every consumer's evidence key names the query the engine actually ran. Parse admits through the `rasm.runtime.faults` `boundary`/`traversed` rail, so the parse-once gate the two siblings share refuses typed at one seam.

## [01]-[INDEX]

- [02]-[SELECTOR]: one `lark`-grammar selector surface — vocabulary-rendered EBNF, the `Facet` row algebra whose case renders back to the `filter_elements` string, the `parse` boundary translating `UnexpectedInput` onto `IfcFault` and lifting it into the `RuntimeRail`, the band-wide `IfcFault` refusal family with its coordinate vocabularies, and the `filter` leg driving `filter_elements` into one `SelectorMatch`.

## [02]-[SELECTOR]

- Owner: `IfcSelector` — `@staticmethod` boundary capsule whose `@cache`-memoized `_engine` builds the `Lark` parser and `SelectorTransformer` once, exposing polymorphic `parse` and the `filter` leg. `Facet` `@tagged_union(frozen=True)` collapses the upstream facets onto four shared-shape cases — `identified` a negatable `instance` GlobalId or `entity` IfcClass, `attribute` a capital-initial name and a comparison, `keyed` the `keyword comparison value` facets, `qualified` a `property`/`query` dotted-path predicate — never a parallel case per facet or a flat `axis`-tagged bag. `SelectorComparison` frozen value object owns the operator/negate/value triple and its `render`, one carrier every comparing facet shares rather than three fold-positional children re-discriminated per case. `SelectorQuery` frozen fold product holds the facet groups and owns the `filter_string`/`axes` projections. `SelectorMatch` is the `filter` leg's one product, the validated query beside its element match, so a consumer keys its evidence on the canonical spelling the engine ran without a second parse. `SelectorOperator`/`IdentifyAxis`/`QualifyAxis`/`SelectorKeyword` closed `StrEnum` vocabularies; `SelectorTransformer` the `Transformer_NonRecursive` folding the wide `+`/`,` spine iteratively, no Python recursion limit.
- Owner: `IfcFault` `@tagged_union(frozen=True)` Exception is the IFC band's floor refusal family, its nine cases one per refusal LAW rather than one per raising site — `unrostered` covers every foreign token a band vocabulary refuses, `empty_roster` every roster a fold requires non-empty, `degenerate_measure` every measure at or under its kernel floor, and `flawed_curve` both the closed-ring and the centreline-offset census under one flaw vocabulary at a typed index PATH. `ParseStop`/`IfcRoster`/`SectionMeasure`/`CurveFlaw`/`ArgumentFlaw`/`GeoDrop` close each coordinate axis a case slot carries, so a defect is a member a consumer matches rather than a substring it splits, and absence rides `Option` — never an `"absent"` literal, a `-1` position, or a fabricated `wire` code standing in for a protocol that issued none. `IfcFault.of_stop` is the two-tier constructor over `_STOPS`, the one site naming lark's divergent `allowed`/`expected` roster spellings.
- Law: a case reaches the rail through `runtime/reliability/faults#FAULTS`' own conversion — `raise` inside a converting fence, `BoundaryFault.of(at, IfcFault(...))` on a pure helper reachable outside one — never a band-local converter. That door admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so a case crosses WHOLE on the `domain` case and no render stands between it and its consumer. A worker seam carries it whole too — a kwarg-only `@tagged_union` Exception pickles on no arm, so `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA and re-mints this family's own case parent-side, and a producer edits nothing. `__str__` serves the LOG and HOST edge alone, where `Exception.__str__` answers the empty string for this shape.
- Cases: grammar `start` is one `filter_group` — a `+`-union of `,`-chained `facet_list`s over upstream's two operators: `+` unions groups (`|=` across the appended lists), `,` chains additive/subtractive facets against a running set. Contains is `*=`, negation the `!` prefix on an identifier or comparison. Each `facet` folds to one `Facet` case matched by `match`/`assert_never` on both the fold and the `render` re-serialization, mirroring the `ifc/analysis#ANALYSIS` `AnalysisRow.facts` self-projecting row.
- Entry: `IfcSelector.parse` is polymorphic — a `str` parses one query, an `Iterable[str]` folds through `traversed(..., by=Disposition.ABORT)` into one `RuntimeRail[Block[SelectorQuery]]` so a batch validates under one rail short-circuiting on the first malformed member, never a per-arm loop. Single-string arm runs `parser.parse` then `transformer.transform` under `boundary(SELECTOR_PARSE, ..., catch=(IfcFault, VisitError))`, so the refusal rides this module's own rostered anchor and the stop TRANSLATES to `IfcFault.of_stop` inside that thunk, where the exception object is still in hand — so the offending query, the offset, and the admissible-terminal roster all cross on `BoundaryFault.domain` as one typed coordinate the `(subject, cause)` builder alone would erase.
- Auto: `SELECTOR_GRAMMAR` is an f-string over the Python vocabularies, never a second transcription of them — `_alts` renders the `KEYWORD`/`OP`/`SPECIAL` alternations longest-literal-first off `SelectorKeyword`/`SelectorOperator`/`_SPECIALS` (the length key also fixing the render a `frozenset`'s hash order would otherwise reshuffle per run), and `_delim_class` renders `UNQUOTED`'s negated class off `_TOKEN_DELIMS` with whitespace collapsed to `\s` so the class and the ignored `WS` terminal agree on one whitespace domain. That same class compiles once as `_UNQUOTED`, which IS `_emit_token`'s re-quote test, so a bare-rendered token cannot fall outside the terminal the parser re-accepts it under. Parser is `Lark(SELECTOR_GRAMMAR, start="start", parser="earley")` — Earley for the ambiguous `+`/`,`/predicate grammar, the algorithm upstream itself builds. `cache=` stays unset: `lark` raises `ConfigurationError` on parser-cache serialization for any parser but `lalr`, so the `@cache`-memoized `_engine` compiling the EBNF once on first parse is the build-once mechanism.
- Packages: `lark` (`Lark(..., parser="earley")`, `Transformer_NonRecursive().transform`, `v_args(inline=True)`, `UnexpectedInput` and its three `UnexpectedCharacters`/`UnexpectedToken`/`UnexpectedEOF` leaves with `pos_in_stream` and their own roster attributes, `lark.exceptions.VisitError` — the top-level namespace exports the parse leaves and not this one — as the fold-defect half of the parse fence's declared catch set; `cache=` excluded, `lalr`-only), `ifcopenshell` (`util.selector.filter_elements` consuming `filter_string`, the only selection engine), `rasm.runtime.faults` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`FAULT_CONF` plus `FaultRow`/`TERMINAL`/`rostered` as the row shape, posture, and seat door this module's one raise anchor takes — no dedicated `lark` `CLASSIFY` row, since the universal faults owner never imports a geometry-domain grammar and a `VisitError` carries its own wrapper class into the message-carrying catch-all), `rasm.geometry.graduation` (`GeometryLeg` alone — the folder's raise-leg roster this page's `FaultRow` anchors on, the one S0 seat every raiser reaches without a back-edge), `expression` (`tagged_union` the `Facet` and `IfcFault` algebras, `Block` the batch carrier and the `_STOPS` rows, `Option` the coordinate absence axis), `msgspec` (`Struct` the frozen `SelectorComparison`/`SelectorQuery`), `beartype` (`@beartype(conf=FAULT_CONF)` on `parse`), stdlib `re` (`escape` rendering the delimiter class, one module-level compiled `Pattern` serving the re-quote test).
- Growth: a new operator is one `SelectorOperator` row, a new keyword one `SelectorKeyword` row, a new special literal one `_SPECIALS` member, and a new delimiter one `_TOKEN_DELIMS` member — the terminal, the re-quote test, and the round-trip all re-render from that one row. A new upstream facet is one EBNF alternative, one `Facet` case (or one `IdentifyAxis`/`QualifyAxis` row when it folds onto an existing shape), one transformer method, and one `render` arm — no second parser, no per-facet sibling class. A new band refusal is one `IfcFault` case carrying its own coordinate tuple, minted at the page that raises it and read by every consumer off the tag; a new coordinate member is one row in the axis vocabulary it belongs to, and a defect near-identical to a landed one takes that case's subject slot instead of minting a sibling. A new lark stop leaf is one `_STOPS` row naming its own roster attribute, which `ParseStop.UNCLASSIFIED` already floors until it lands.
- Boundary: no privately re-invented dialect — `SELECTOR_GRAMMAR` mirrors `filter_elements_grammar` rule-by-rule, so fabricated operators, prefixes, and qualifiers upstream rejects never enter; no hand-rolled regex/split parser; no second selection engine past the `filter_string` round-trip; no stringly passthrough of the raw query past admission; no `cache=True` on an Earley parser, and no `SelectorOperator(str(token))` or `raise UnexpectedInput` in a fold body where the grammar terminal already bounds the children. No terminal restates a Python vocabulary as a literal alternation, and no second delimiter set sits beside `_TOKEN_DELIMS`. `IfcFault` carries DOMAIN refusals alone — a provider raise and a worker death classify through the runtime's own `CLASSIFY` rows, never a band-local twin — and no band page mints a second refusal vocabulary, re-types a case at an outer seat, or renders a coordinate into a subject string a sibling then splits. `parse` and `filter` stay caller-floor by charter — parse is a short pure fold and `filter_elements` an attribute walk over the live in-process model, a pybind11 handle no pickle seam carries, so no lane crossing exists here; any future kernel wrapping a mutating script declares `idempotent=False`.

```python
import re
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from typing import Final, Literal, assert_never, overload

from beartype import beartype
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from lark import Lark, Token, Transformer_NonRecursive, UnexpectedCharacters, UnexpectedEOF, UnexpectedInput, UnexpectedToken, v_args
from lark.exceptions import VisitError
from msgspec import Struct

lazy import ifcopenshell.util.selector

from rasm.geometry.graduation import GeometryLeg
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Disposition, FaultRow, RuntimeRail, boundary, rostered, traversed

# --- [TYPES] ----------------------------------------------------------------------------


class SelectorOperator(StrEnum):
    EQ = "="
    GE = ">="
    LE = "<="
    GT = ">"
    LT = "<"
    CONTAINS = "*="


class IdentifyAxis(StrEnum):
    INSTANCE = "instance"
    ENTITY = "entity"


class QualifyAxis(StrEnum):
    PROPERTY = "property"
    QUERY = "query"


class SelectorKeyword(StrEnum):
    TYPE = "type"
    MATERIAL = "material"
    CLASSIFICATION = "classification"
    LOCATION = "location"
    GROUP = "group"
    PARENT = "parent"


# --- [CONSTANTS] ------------------------------------------------------------------------

_SPECIALS: Final[frozenset[str]] = frozenset({"NULL", "TRUE", "FALSE"})
_WHITESPACE: Final[frozenset[str]] = frozenset(" \t\f\r\n")
_QUOTE: Final[str] = '"'
_TOKEN_DELIMS: Final[frozenset[str]] = frozenset(",.=><*!") | _WHITESPACE | frozenset(_QUOTE)


def _alts(vocabulary: Iterable[str]) -> str:
    return " | ".join(f'"{token}"' for token in sorted(vocabulary, key=lambda token: (-len(token), str(token))))


def _delim_class(delims: frozenset[str]) -> str:
    return rf"[^{re.escape(''.join(sorted(delims - _WHITESPACE - frozenset(_QUOTE))))}\s]+"


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

_UNQUOTED: Final[re.Pattern[str]] = re.compile(_delim_class(_TOKEN_DELIMS))


# --- [BOUNDARIES] -----------------------------------------------------------------------


def _emit_token(text: str) -> str:
    if text in _SPECIALS or (len(text) >= 2 and text[0] == "/" and text[-1] == "/"):
        return text
    return text if _UNQUOTED.fullmatch(text) else _QUOTE + text.replace(_QUOTE, "\\" + _QUOTE) + _QUOTE


# --- [ERRORS] ---------------------------------------------------------------------------

# --- [REFUSAL_COORDINATES]


class ParseStop(StrEnum):
    CHARACTER = "character"
    TOKEN = "token"
    EXHAUSTED = "exhausted"
    UNCLASSIFIED = "unclassified"


class IfcRoster(StrEnum):
    PROFILE_ELEMENT = "profile-element"
    STRUCTURAL_MEMBER = "structural-member"
    EXPORT_COLUMN = "export-column"


class SectionMeasure(StrEnum):
    CENTRELINE_VERTICES = "centreline-vertices"
    PROFILE_THICKNESS = "profile-thickness"
    SECTION_AREA = "section-area"


class CurveFlaw(StrEnum):
    MALFORMED = "malformed"
    ZERO_AREA = "zero-area"
    SELF_INTERSECTS = "self-intersects"
    REVERSAL = "reversal"
    OFFSET_SELF_INTERSECTS = "offset-self-intersects"


class ArgumentFlaw(StrEnum):
    UNKNOWN = "unknown"
    NOT_ENTITY = "not-entity"
    UNSUPPLIED = "unsupplied"
    ARITY = "arity"


class GeoDrop(StrEnum):
    NON_UNIFORM_FACTORS = "non-uniform-factors"
    UNNAMED_CRS = "unnamed-crs"


_STOPS: Final[Block[tuple[type[UnexpectedInput], ParseStop, str]]] = Block.of_seq([
    (UnexpectedCharacters, ParseStop.CHARACTER, "allowed"),
    (UnexpectedToken, ParseStop.TOKEN, "expected"),
    (UnexpectedEOF, ParseStop.EXHAUSTED, "expected"),
])


@tagged_union(frozen=True)
class IfcFault(Exception):
    tag: Literal[
        "unrostered", "unserved", "empty_roster", "unresolved_slots", "degenerate_measure",
        "flawed_curve", "divergent_arguments", "unwirable_georeference", "unparsed_query",
    ] = tag()
    unrostered: tuple[str, str] = case()
    unserved: tuple[str, str] = case()
    empty_roster: tuple[str, IfcRoster] = case()
    unresolved_slots: tuple[str, tuple[str, ...]] = case()
    degenerate_measure: tuple[str, SectionMeasure, Option[float]] = case()
    flawed_curve: tuple[str, tuple[tuple[CurveFlaw, tuple[int, ...]], ...]] = case()
    divergent_arguments: tuple[str, tuple[tuple[ArgumentFlaw, str, Option[int]], ...]] = case()
    unwirable_georeference: tuple[tuple[GeoDrop, str], ...] = case()
    unparsed_query: tuple[str, ParseStop, Option[int], tuple[str, ...]] = case()

    @staticmethod
    def of_stop(text: str, stop: UnexpectedInput) -> "IfcFault":
        row = _STOPS.choose(lambda member: Some(member) if isinstance(stop, member[0]) else Nothing).try_head()
        admissible = row.map(lambda member: getattr(stop, member[2]) or ()).default_value(())
        return IfcFault(unparsed_query=(
            text,
            row.map(lambda member: member[1]).default_value(ParseStop.UNCLASSIFIED),
            Some(stop.pos_in_stream) if stop.pos_in_stream >= 0 else Nothing,
            tuple(sorted(frozenset(str(name) for name in admissible))),
        ))

    def __str__(self) -> str:
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case IfcFault(tag="unrostered", unrostered=(vocabulary, token)):
                return f"{vocabulary}={token}"
            case IfcFault(tag="unserved", unserved=(capability, member)):
                return f"{capability}={member}"
            case IfcFault(tag="empty_roster", empty_roster=(subject, roster)):
                return f"{subject}[{roster.value}]"
            case IfcFault(tag="unresolved_slots", unresolved_slots=(subject, slots)):
                return f"{subject}[{','.join(slots)}]"
            case IfcFault(tag="degenerate_measure", degenerate_measure=(subject, measure, value)):
                return f"{subject}[{measure.value}={value.map(lambda held: format(held, '.6g')).default_value('absent')}]"
            case IfcFault(tag="flawed_curve", flawed_curve=(subject, census)):
                loci = ";".join(f"{flaw.value}@{'.'.join(map(str, at))}" for flaw, at in census)
                return f"{subject}[{loci}]"
            case IfcFault(tag="divergent_arguments", divergent_arguments=(usecase, census)):
                spelled = ";".join(f"{flaw.value}:{keyword}{count.map(lambda held: f'={held}').default_value('')}" for flaw, keyword, count in census)
                return f"{usecase}[{spelled}]"
            case IfcFault(tag="unwirable_georeference", unwirable_georeference=census):
                return ";".join(f"{drop.value}={spelling}" for drop, spelling in census)
            case IfcFault(tag="unparsed_query", unparsed_query=(query, stop, offset, terminals)):
                return f"{query}@{offset.map(str).default_value('eof')}[{stop.value}:{','.join(terminals)}]"
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

SELECTOR_PARSE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SELECTOR, point="parse", arm="boundary", defect="query-unparsed", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SELECTOR_PARSE]))


# --- [MODELS] ---------------------------------------------------------------------------


class SelectorComparison(Struct, frozen=True, gc=False):
    operator: SelectorOperator
    value: str
    negate: bool = False

    def render(self) -> str:
        return f"{'!' if self.negate else ''}{self.operator.value}{_emit_token(self.value)}"


@tagged_union(frozen=True)
class Facet:
    tag: str = tag()
    identified: tuple[IdentifyAxis, str, bool] = case()
    attribute: tuple[str, SelectorComparison] = case()
    keyed: tuple[SelectorKeyword, SelectorComparison] = case()
    qualified: tuple[QualifyAxis, str, str | None, SelectorComparison] = case()

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


class SelectorMatch(Struct, frozen=True, gc=False):
    query: SelectorQuery
    elements: tuple["ifcopenshell.entity_instance", ...]


# --- [SERVICES] -------------------------------------------------------------------------


@v_args(inline=True)
class SelectorTransformer(Transformer_NonRecursive):
    def comparison(self, *parts: Token) -> SelectorComparison:
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


# --- [OPERATIONS] -----------------------------------------------------------------------


class IfcSelector:
    @staticmethod
    @cache
    def _engine() -> tuple[Lark, SelectorTransformer]:
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
        return IfcSelector.parse(text).map(
            lambda query: SelectorMatch(query=query, elements=tuple(ifcopenshell.util.selector.filter_elements(model, query.filter_string)))
        )

    @staticmethod
    def _parse_one(text: str) -> "RuntimeRail[SelectorQuery]":
        return boundary(SELECTOR_PARSE, lambda: IfcSelector._fold(text), catch=(IfcFault, VisitError))

    @staticmethod
    def _fold(text: str) -> SelectorQuery:
        parser, transformer = IfcSelector._engine()
        try:
            tree = parser.parse(text)
        except UnexpectedInput as stop:
            raise IfcFault.of_stop(text, stop) from stop
        return transformer.transform(tree)
```


## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

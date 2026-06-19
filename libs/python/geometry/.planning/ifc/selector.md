# [PY_GEOMETRY_IFC_SELECTOR]

The validated element-selection grammar admitting a structured query before `ifcopenshell.util.selector.filter_elements` — the typed boundary the `ifc-analysis:Path/analysis.md#ANALYSIS` quantity/pset arms and the `ifc-analysis:Path/costing.md#LIFECYCLE` take-off arms thread their free-form `query` string into today. `IfcSelector` compiles one `lark` EBNF grammar over the full IFC selection vocabulary — entity class, attribute predicate, pset/property predicate, classification reference, and spatial containment — folds the parse `Tree` through one `SelectorTransformer` into a frozen `SelectorQuery`, and lifts an `UnexpectedInput` parse failure into the runtime fault rail once at admission, so a malformed selector is a typed fault at the boundary, never a silent empty `filter_elements` match three arms deep. The validated query carries its canonical `filter_elements` string back out, so the grammar admits and re-serializes the selection without ever standing up a second selection engine: `ifcopenshell` still runs the filter, `lark` only owns the closed query vocabulary the string must parse against.

## [01]-[INDEX]

- [01]-[SELECTOR]: the one `lark`-grammar selector surface — EBNF grammar constant, `Transformer` fold to `SelectorQuery`, and the `parse` boundary lifting `UnexpectedInput` into the fault rail before the validated query drives `filter_elements`.

## [02]-[SELECTOR]

- Owner: `IfcSelector` — the boundary capsule holding the compiled `Lark` parser plus the `SelectorTransformer`, exposing one `parse` entry; `SelectorQuery` the frozen structured query the fold produces; `SelectorGrammar` the owner-authored EBNF constant that is the closed query vocabulary every terminal traces to; `SelectorTransformer` the `lark.Transformer` subclass folding the parse `Tree` bottom-up.
- Cases: the grammar's `selection` is a `union` of `,`-joined `filter` groups, each an intersection of `clause` predicates over the five IFC selection axes — `entity` (an IFC class name, optionally `!`-negated), `attribute` (a dotted attribute name plus a comparator against a value or the `null`/`*` existence tokens), `pset` (a `/`-qualified pset-name `.` property-name predicate), `classification` (a `classification.<system>=<code>` reference), and `containment` (a `@<spatial-class>` decomposition filter). Each `clause` folds to one `SelectorPredicate` row; the closed `comparator` and `axis` vocabularies are `StrEnum` families matched by `match`/`assert_never`, never an open string.
- Entry: `IfcSelector.parse` takes a selector `str` and returns a `RuntimeRail[SelectorQuery]` via `boundary("selector.parse", ...)`; the boundary runs `parser.parse(text)` then `transformer.transform(tree)`, so an `UnexpectedInput` (`UnexpectedToken`/`UnexpectedCharacters`) raised by the lexer/parser is the one fault lifted into the rail at admission. The returned `SelectorQuery.filter_string` re-serializes the validated query to the exact `filter_elements` grammar `ifcopenshell` consumes, so the analysis/costing arms call `IfcSelector.filter(model, query)` — `parse(query).map(lambda q: filter_elements(model, q.filter_string))` — validated once, never a raw passthrough.
- Packages: `lark` (`Lark(grammar, start="selection", parser="earley")` parser build — `earley` for the ambiguous `,`/predicate selection grammar, the algorithm a constructor knob never a parser-per-algorithm family; `Transformer.transform(tree)` bottom-up fold; `v_args(inline=True)` binding rule children as positional fold arguments; `UnexpectedInput` lifted once at the parse boundary), `ifcopenshell` (`util.selector.filter_elements` consuming the re-serialized `filter_string` — the only selection engine, never a parallel filter), runtime (`RuntimeRail`/`boundary` for the parse fault rail).
- Growth: a new selection axis is one EBNF `clause` alternative plus one `SelectorAxis` row plus one `SelectorTransformer` method; a new comparator is one `SelectorComparator` row plus one grammar terminal; zero new surface, no second parser, no per-axis sibling class.
- Boundary: no hand-rolled regex/split query parser where the grammar owns the structure (the deleted form); no second selection engine — the validated query re-serializes to the `filter_elements` string `ifcopenshell` owns; no stringly-typed passthrough of the raw query past admission; a malformed selector is an `UnexpectedInput`-derived fault at the boundary, never a silent empty match. The `SelectorTransformer` is one `Transformer` with one method per rule, never a visitor-per-node sibling family.

```python contract
import ifcopenshell
import ifcopenshell.util.selector
from enum import StrEnum
from typing import assert_never

from lark import Lark, Transformer, UnexpectedInput, v_args
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary

# --- [TYPES] ---------------------------------------------------------------------------


class SelectorAxis(StrEnum):
    ENTITY = "entity"
    ATTRIBUTE = "attribute"
    PSET = "pset"
    CLASSIFICATION = "classification"
    CONTAINMENT = "containment"


class SelectorComparator(StrEnum):
    EQ = "="
    NE = "!="
    GT = ">"
    GE = ">="
    LT = "<"
    LE = "<="
    EXISTS = "*"
    NULL = "null"


# --- [CONSTANTS] -----------------------------------------------------------------------

SelectorGrammar: str = r"""
    selection   : filter ("," filter)*

    filter      : clause+

    ?clause     : entity
                | attribute
                | pset
                | classification
                | containment

    entity      : NEGATE? CLASS
    attribute   : DOTTED comparator value?
    pset        : "/" NAME "." NAME comparator value?
    classification : "classification" "." NAME "=" value
    containment : "@" CLASS

    ?comparator : EQ | NE | GE | LE | GT | LT | EXISTS
    ?value      : NUMBER | STRING | NAME | NULL

    NEGATE      : "!"
    EQ          : "="
    NE          : "!="
    GE          : ">="
    LE          : "<="
    GT          : ">"
    LT          : "<"
    EXISTS      : "*"
    NULL        : "null"
    CLASS       : /Ifc[A-Za-z0-9_]+/
    DOTTED      : /[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+/
    NAME        : /[A-Za-z_][A-Za-z0-9_ ]*/
    STRING      : /"[^"]*"/
    NUMBER      : /-?\d+(\.\d+)?/

    %import common.WS
    %ignore WS
"""

# --- [MODELS] --------------------------------------------------------------------------


class SelectorPredicate(Struct, frozen=True):
    axis: SelectorAxis
    name: str
    comparator: SelectorComparator
    value: str


class SelectorFilter(Struct, frozen=True):
    predicates: tuple[SelectorPredicate, ...]

    @property
    def clause_string(self) -> str:
        return " ".join(IfcSelector._predicate_string(p) for p in self.predicates)


class SelectorQuery(Struct, frozen=True):
    filters: tuple[SelectorFilter, ...]

    @property
    def axes(self) -> frozenset[SelectorAxis]:
        return frozenset(p.axis for f in self.filters for p in f.predicates)

    @property
    def filter_string(self) -> str:
        return ", ".join(f.clause_string for f in self.filters)


# --- [SERVICES] ------------------------------------------------------------------------


@v_args(inline=True)
class SelectorTransformer(Transformer):
    def entity(self, *parts: object) -> SelectorPredicate:
        negate = len(parts) == 2
        klass = str(parts[-1])
        return SelectorPredicate(SelectorAxis.ENTITY, klass, SelectorComparator.NE if negate else SelectorComparator.EQ, klass)

    def attribute(self, dotted: object, *rest: object) -> SelectorPredicate:
        comparator, value = SelectorTransformer._compare(rest)
        return SelectorPredicate(SelectorAxis.ATTRIBUTE, str(dotted), comparator, value)

    def pset(self, pset_name: object, prop_name: object, *rest: object) -> SelectorPredicate:
        comparator, value = SelectorTransformer._compare(rest)
        return SelectorPredicate(SelectorAxis.PSET, f"{str(pset_name).strip()}.{str(prop_name).strip()}", comparator, value)

    def classification(self, system: object, value: object) -> SelectorPredicate:
        return SelectorPredicate(SelectorAxis.CLASSIFICATION, str(system).strip(), SelectorComparator.EQ, SelectorTransformer._literal(value))

    def containment(self, klass: object) -> SelectorPredicate:
        return SelectorPredicate(SelectorAxis.CONTAINMENT, str(klass), SelectorComparator.EQ, str(klass))

    def filter(self, *clauses: SelectorPredicate) -> SelectorFilter:
        return SelectorFilter(tuple(clauses))

    def selection(self, *filters: SelectorFilter) -> SelectorQuery:
        return SelectorQuery(tuple(filters))

    @staticmethod
    def _compare(rest: tuple[object, ...]) -> tuple[SelectorComparator, str]:
        match rest:
            case ():
                return SelectorComparator.EXISTS, ""
            case (token,):
                return SelectorComparator(str(token)), ""
            case (token, value):
                return SelectorComparator(str(token)), SelectorTransformer._literal(value)
            case _:
                raise UnexpectedInput("selector clause carried more than a comparator and a value")

    @staticmethod
    def _literal(value: object) -> str:
        text = str(value)
        return text[1:-1] if text.startswith('"') and text.endswith('"') else text.strip()


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcSelector:
    _parser = Lark(SelectorGrammar, start="selection", parser="earley")
    _transformer = SelectorTransformer()

    @staticmethod
    def parse(text: str) -> "RuntimeRail[SelectorQuery]":
        return boundary("selector.parse", lambda: IfcSelector._fold(text))

    @staticmethod
    def _fold(text: str) -> SelectorQuery:
        tree = IfcSelector._parser.parse(text)
        return IfcSelector._transformer.transform(tree)

    @staticmethod
    def filter(model: "ifcopenshell.file", text: str) -> "RuntimeRail[tuple[ifcopenshell.entity_instance, ...]]":
        return IfcSelector.parse(text).map(lambda query: tuple(ifcopenshell.util.selector.filter_elements(model, query.filter_string)))

    @staticmethod
    def _predicate_string(predicate: SelectorPredicate) -> str:
        match predicate.axis:
            case SelectorAxis.ENTITY:
                return f"!{predicate.value}" if predicate.comparator is SelectorComparator.NE else predicate.value
            case SelectorAxis.ATTRIBUTE:
                return IfcSelector._compare_string(predicate.name, predicate)
            case SelectorAxis.PSET:
                return IfcSelector._compare_string(f"/{predicate.name}", predicate)
            case SelectorAxis.CLASSIFICATION:
                return f"classification.{predicate.name}={predicate.value}"
            case SelectorAxis.CONTAINMENT:
                return f"@{predicate.value}"
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _compare_string(lhs: str, predicate: SelectorPredicate) -> str:
        if predicate.comparator is SelectorComparator.EXISTS:
            return f"{lhs}*"
        return f"{lhs}{predicate.comparator.value}{predicate.value}"
```

## [03]-[RESEARCH]

- [SELECTOR_FILTER_GRAMMAR]: the branch `ifcopenshell` catalogue confirms `util.selector.filter_elements(model, query)` is the selector-grammar element filter the validated `SelectorQuery.filter_string` feeds; the published `filter_elements` query syntax — `IfcWall, IfcSlab` union groups, `material=concrete` attribute predicates, `/Pset_WallCommon.IsExternal=TRUE` pset predicates, `classification.Uniclass=...` references, and the `IfcSpatialElement` decomposition filters — is the exact target string `_predicate_string` re-serializes to, the remaining published-syntax detail the live `ifcopenshell.util.selector` source confirms terminal-by-terminal against `SelectorGrammar`. The `SelectorGrammar` is the closed query vocabulary `lark.md#49` fixes: every terminal (`CLASS`/`DOTTED`/`NAME`/comparator family) traces to one IFC selection axis, and the parser admits the query before `filter_elements` ever runs, so a malformed selector is an `UnexpectedInput` fault at admission, never a silent empty match (`lark.md#51`–`#52` failure/boundary axes).
- [TRANSFORMER_FOLD_BINDING]: the branch `lark` catalogue confirms `Transformer().transform(tree)` is the bottom-up fold and `v_args(inline=True)` binds rule children as positional fold-method arguments; the `@v_args(inline=True)`-on-class application binding every method, the `Token` `str()` coercion the comparator/value methods read, and the variadic-children shape of the optional `comparator value?` tail (zero, one, or two trailing children) confirm against the installed cp315 `lark==1.3.1` distribution — `lark` is pure-Python cp315-clean, so reflection resolves on the project venv directly with no companion-lane gate (`lark.md#63`).
- [IDS_SELECTOR_GRAMMAR]: reconciles the `analysis.md#3-RESEARCH` `IDS_SELECTOR_GRAMMAR` residual without duplicating it — that residual confirms `util.selector.filter_elements` resolves against the branch `ifcopenshell` catalogue for the analysis arms; this page realizes the validated grammar those arms consume, so the analysis/costing `query` string is parsed once through `IfcSelector.parse` and reused across quantity/pset/IDS/clash subject derivation, the unvalidated raw-string boundary the `VALIDATED_SELECTOR_GRAMMAR` idea names closed at admission rather than three arms deep.

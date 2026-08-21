# [PY_GEOMETRY_IFC_SELECTOR]

`IfcSelector` validates an element-selection query before `ifcopenshell.util.selector.filter_elements`: one `lark` EBNF faithful to the upstream `filter_elements_grammar` compiles, the parse `Tree` folds into a frozen `SelectorQuery` of `Facet` cases, and an `UnexpectedInput` parse failure translates at the fence onto `IfcFault.unparsed_query` before lifting into the `RuntimeRail`, so a malformed selector reaches a consumer as the offending query beside the stop kind, its offset, and the terminals that were admissible there, never a silent empty match three arms deep. `ifcopenshell` runs the filter; `lark` owns the closed query vocabulary the string parses against — one grammar admits and re-serializes selection without a second engine.

This page also seats `IfcFault`, the whole IFC band's ONE closed refusal family: one case per refusal LAW, each carrying its own coordinate tuple rather than a rendered `(subject, cause)` string a consumer re-parses, and each reaching the rail through the runtime's own conversion door. The seat follows the band's reachability — `analysis`, `costing`, and `structural` already import `IfcSelector`, and this page imports no band sibling — so the floor introduces no cycle and no sixth page.

`ifc/analysis#ANALYSIS` quantity/pset arms, the `ifc/costing#LIFECYCLE` take-off arm, and the `ifc/structural#STRUCTURAL` profile partition thread their free-form `query` through this boundary, driving elements off `IfcSelector.filter`, the only `filter_elements` caller. `SelectorQuery.filter_string` re-serializes the validated query to the exact `filter_elements` grammar and round-trips — the upstream engine re-accepts every string this owner emits, the frozen wire name the siblings pass back — and `SelectorMatch` carries that spelling home beside the match, so every consumer's evidence key names the query the engine actually ran. Parse admits through the `rasm.runtime.faults` `boundary`/`traversed` rail, and `SelectorQuery` is the `rasm.runtime.receipts` contributor the `@receipted` egress aspect harvests, so the parse-once gate the two siblings share streams its admission/rejection fact without an inline emit.

## [01]-[INDEX]

- [02]-[SELECTOR]: one `lark`-grammar selector surface — vocabulary-rendered EBNF, the `Facet` row algebra whose case renders back to the `filter_elements` string, the `parse` boundary translating `UnexpectedInput` onto `IfcFault` and lifting it into the `RuntimeRail`, the band-wide `IfcFault` refusal family with its coordinate vocabularies, and the `filter` leg driving `filter_elements` into one `SelectorMatch`.

## [02]-[SELECTOR]

- Owner: `IfcSelector` — `@staticmethod` boundary capsule whose `@cache`-memoized `_engine` builds the `Lark` parser and `SelectorTransformer` once, exposing polymorphic `parse`, the `filter` leg, and the private `@receipted` `_emit` point. `Facet` `@tagged_union(frozen=True)` collapses the upstream facets onto four shared-shape cases — `identified` a negatable `instance` GlobalId or `entity` IfcClass, `attribute` a capital-initial name and a comparison, `keyed` the `keyword comparison value` facets, `qualified` a `property`/`query` dotted-path predicate — never a parallel case per facet or a flat `axis`-tagged bag. `SelectorComparison` frozen value object owns the operator/negate/value triple and its `render`, one carrier every comparing facet shares rather than three fold-positional children re-discriminated per case. `SelectorQuery` frozen fold product holds the facet groups, owns the `filter_string`/`axes`/`span_facts` projections, and implements `ReceiptContributor` itself — no parallel `SelectorReceipt`. `SelectorMatch` is the `filter` leg's one product, the validated query beside its element match, so a consumer keys its evidence on the canonical spelling the engine ran without a second parse. `SelectorOperator`/`IdentifyAxis`/`QualifyAxis`/`SelectorKeyword` closed `StrEnum` vocabularies; `SelectorTransformer` the `Transformer_NonRecursive` folding the wide `+`/`,` spine iteratively, no Python recursion limit.
- Owner: `IfcFault` `@tagged_union(frozen=True)` Exception is the IFC band's floor refusal family, its nine cases one per refusal LAW rather than one per raising site — `unrostered` covers every foreign token a band vocabulary refuses, `empty_roster` every roster a fold requires non-empty, `degenerate_measure` every measure at or under its kernel floor, and `flawed_curve` both the closed-ring and the centreline-offset census under one flaw vocabulary at a typed index PATH. `ParseStop`/`IfcRoster`/`SectionMeasure`/`CurveFlaw`/`ArgumentFlaw`/`GeoDrop` close each coordinate axis a case slot carries, so a defect is a member a consumer matches rather than a substring it splits, and absence rides `Option` — never an `"absent"` literal, a `-1` position, or a fabricated `wire` code standing in for a protocol that issued none. `IfcFault.of_stop` is the two-tier constructor over `_STOPS`, the one site naming lark's divergent `allowed`/`expected` roster spellings.
- Law: a case reaches the rail through `runtime/reliability/faults#FAULTS`' own conversion — `raise` inside a converting fence, `BoundaryFault.of(at, IfcFault(...))` on a pure helper reachable outside one — never a band-local converter. That door admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so a case crosses WHOLE on the `domain` case and no render stands between it and its consumer. A worker seam carries it whole too — a kwarg-only `@tagged_union` Exception pickles on no arm, so `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA and re-mints this family's own case parent-side, and a producer edits nothing. `__str__` serves the LOG and HOST edge alone, where `Exception.__str__` answers the empty string for this shape.
- Cases: grammar `start` is one `filter_group` — a `+`-union of `,`-chained `facet_list`s over upstream's two operators: `+` unions groups (`|=` across the appended lists), `,` chains additive/subtractive facets against a running set. Contains is `*=`, negation the `!` prefix on an identifier or comparison. Each `facet` folds to one `Facet` case matched by `match`/`assert_never` on both the fold and the `render` re-serialization, mirroring the `ifc/analysis#ANALYSIS` `AnalysisRow.facts` self-projecting row.
- Entry: `IfcSelector.parse` is polymorphic — a `str` parses one query, an `Iterable[str]` folds through `traversed(..., by=Disposition.ABORT)` into one `RuntimeRail[Block[SelectorQuery]]` so a batch validates under one rail short-circuiting on the first malformed member, never a per-arm loop. Single-string arm runs `parser.parse` then `transformer.transform` under `boundary(SELECTOR_PARSE, ..., catch=(IfcFault, VisitError))`, so the refusal rides this module's own rostered anchor and the stop TRANSLATES to `IfcFault.of_stop` inside that thunk, where the exception object is still in hand — so the offending query, the offset, and the admissible-terminal roster all cross on `BoundaryFault.domain` as one typed coordinate the `(subject, cause)` builder alone would erase.
- Auto: `SELECTOR_GRAMMAR` is an f-string over the Python vocabularies, never a second transcription of them — `_alts` renders the `KEYWORD`/`OP`/`SPECIAL` alternations longest-literal-first off `SelectorKeyword`/`SelectorOperator`/`_SPECIALS` (the length key also fixing the render a `frozenset`'s hash order would otherwise reshuffle per run), and `_delim_class` renders `UNQUOTED`'s negated class off `_TOKEN_DELIMS` with whitespace collapsed to `\s` so the class and the ignored `WS` terminal agree on one whitespace domain. That same class compiles once as `_UNQUOTED`, which IS `_emit_token`'s re-quote test, so a bare-rendered token cannot fall outside the terminal the parser re-accepts it under. Parser is `Lark(SELECTOR_GRAMMAR, start="start", parser="earley")` — Earley for the ambiguous `+`/`,`/predicate grammar, the algorithm upstream itself builds. `cache=` stays unset: `lark` raises `ConfigurationError` on parser-cache serialization for any parser but `lalr`, so the `@cache`-memoized `_engine` compiling the EBNF once on first parse is the build-once mechanism. `@receipted(OPEN)` decorates the private `_emit`; `filter` emits transitively because it composes `parse`, never a second decorated leg.
- Packages: `lark` (`Lark(..., parser="earley")`, `Transformer_NonRecursive().transform`, `v_args(inline=True)`, `UnexpectedInput` and its three `UnexpectedCharacters`/`UnexpectedToken`/`UnexpectedEOF` leaves with `pos_in_stream` and their own roster attributes, `lark.exceptions.VisitError` — the top-level namespace exports the parse leaves and not this one — as the fold-defect half of the parse fence's declared catch set; `cache=` excluded, `lalr`-only), `ifcopenshell` (`util.selector.filter_elements` consuming `filter_string`, the only selection engine), `rasm.runtime.faults` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`FAULT_CONF` plus `FaultRow`/`TERMINAL`/`rostered` as the row shape, posture, and seat door this module's one raise anchor takes — no dedicated `lark` `CLASSIFY` row, since the universal faults owner never imports a geometry-domain grammar and a `VisitError` carries its own wrapper class into the message-carrying catch-all), `rasm.geometry.graduation` (`GeometryLeg` alone — the folder's raise-leg roster this page's `FaultRow` anchors on, the one S0 seat every raiser reaches without a back-edge), `rasm.runtime.receipts`, `expression` (`tagged_union` the `Facet` and `IfcFault` algebras, `Block` the batch carrier and the `_STOPS` rows, `Option` the coordinate absence axis), `msgspec` (`Struct` the frozen `SelectorComparison`/`SelectorQuery`), `beartype` (`@beartype(conf=FAULT_CONF)` on `parse`), stdlib `re` (`escape` rendering the delimiter class, one module-level compiled `Pattern` serving the re-quote test).
- Growth: a new operator is one `SelectorOperator` row, a new keyword one `SelectorKeyword` row, a new special literal one `_SPECIALS` member, and a new delimiter one `_TOKEN_DELIMS` member — the terminal, the re-quote test, and the round-trip all re-render from that one row. A new upstream facet is one EBNF alternative, one `Facet` case (or one `IdentifyAxis`/`QualifyAxis` row when it folds onto an existing shape), one transformer method, and one `render` arm — no second parser, no per-facet sibling class, no receipt edit. A new band refusal is one `IfcFault` case carrying its own coordinate tuple, minted at the page that raises it and read by every consumer off the tag; a new coordinate member is one row in the axis vocabulary it belongs to, and a defect near-identical to a landed one takes that case's subject slot instead of minting a sibling. A new lark stop leaf is one `_STOPS` row naming its own roster attribute, which `ParseStop.UNCLASSIFIED` already floors until it lands.
- Boundary: no privately re-invented dialect — `SELECTOR_GRAMMAR` mirrors `filter_elements_grammar` rule-by-rule, so fabricated operators, prefixes, and qualifiers upstream rejects never enter; no hand-rolled regex/split parser; no second selection engine past the `filter_string` round-trip; no stringly passthrough of the raw query past admission; no `cache=True` on an Earley parser, and no `SelectorOperator(str(token))` or `raise UnexpectedInput` in a fold body where the grammar terminal already bounds the children. No terminal restates a Python vocabulary as a literal alternation, and no second delimiter set sits beside `_TOKEN_DELIMS`. `IfcFault` carries DOMAIN refusals alone — a provider raise and a worker death classify through the runtime's own `CLASSIFY` rows, never a band-local twin — and no band page mints a second refusal vocabulary, re-types a case at an outer seat, or renders a coordinate into a subject string a sibling then splits. `parse` and `filter` stay caller-floor by charter — parse is a short pure fold and `filter_elements` an attribute walk over the live in-process model, a pybind11 handle no pickle seam carries, so no lane crossing exists here; any future kernel wrapping a mutating script declares `idempotent=False`.

```python signature
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


# --- [ERRORS] ---------------------------------------------------------------------------

# --- [REFUSAL_COORDINATES]
# One closed vocabulary per refusal AXIS, seated with the family whose case slots carry them: a consumer reads the
# defect off a member instead of re-parsing a rendered cause, and a new axis member is one row here. Every member
# below names a refusal a band seam raises today; a member no seam mints is deleted, never diagrammed.


class ParseStop(StrEnum):
    CHARACTER = "character"  # UnexpectedCharacters — the lexer refused a character no terminal admits
    TOKEN = "token"  # UnexpectedToken — the parser refused a matched terminal at its position
    EXHAUSTED = "exhausted"  # UnexpectedEOF — input ended mid-rule, and lark spells its position `-1`
    UNCLASSIFIED = "unclassified"  # an `UnexpectedInput` leaf `_STOPS` does not roster; its terminal roster reads empty


class IfcRoster(StrEnum):
    PROFILE_ELEMENT = "profile-element"  # the selector's own match set, empty-gated before the profile partition
    STRUCTURAL_MEMBER = "structural-member"  # the IfcRelAssignsToGroup members a structural group carries
    EXPORT_COLUMN = "export-column"  # the trimmed column contract an EXPORT spec resolves to


class SectionMeasure(StrEnum):
    CENTRELINE_VERTICES = "centreline-vertices"  # distinct vertices surviving the centreline dedupe
    PROFILE_THICKNESS = "profile-thickness"  # the constant offset width a centreline profile declares
    SECTION_AREA = "section-area"  # the assembled contour area every moment divides through


class CurveFlaw(StrEnum):
    MALFORMED = "malformed"  # the ClosedRing refinement rejects the shape outright
    ZERO_AREA = "zero-area"  # sub-epsilon signed area — the collinear or zero-extent loop a centroid divide cannot survive
    SELF_INTERSECTS = "self-intersects"  # crossed lobes the shoelace and every contour moment silently mis-sign
    REVERSAL = "reversal"  # a centreline vertex whose turning angle leaves the miter unbounded
    OFFSET_SELF_INTERSECTS = "offset-self-intersects"  # an inner retraction reaching past the shorter adjacent span


class ArgumentFlaw(StrEnum):
    UNKNOWN = "unknown"  # an argument the usecase never declares
    NOT_ENTITY = "not-entity"  # a literal bound where an entity parameter is wanted
    UNSUPPLIED = "unsupplied"  # a required argument left unbound
    ARITY = "arity"  # a scalar entity parameter bound to anything but one slot


class GeoDrop(StrEnum):
    NON_UNIFORM_FACTORS = "non-uniform-factors"  # a factor triple one scale cannot carry across the eight wire fields
    UNNAMED_CRS = "unnamed-crs"  # a coordinate operation whose target CRS names nothing a consumer resolves


# One row per lark stop class, each naming the terminal-roster attribute ITS class spells — `UnexpectedCharacters`
# publishes `allowed`, both parser stops publish `expected`, and all three answer plain terminal names. Row order is
# free here, unlike the runtime `CLASSIFY` fold it mirrors: the three are DISJOINT leaves under `UnexpectedInput`,
# none subclassing another, so a first-match fold cannot coalesce a lexer stop into a parser row.
_STOPS: Final[Block[tuple[type[UnexpectedInput], ParseStop, str]]] = Block.of_seq([
    (UnexpectedCharacters, ParseStop.CHARACTER, "allowed"),
    (UnexpectedToken, ParseStop.TOKEN, "expected"),
    (UnexpectedEOF, ParseStop.EXHAUSTED, "expected"),
])


@tagged_union(frozen=True)
class IfcFault(Exception):
    # The IFC band's ONE structured refusal, seated beside the grammar owner for the reachability the band already
    # has: `analysis`, `costing`, and `structural` thread their query through `IfcSelector` already and this page
    # imports no band sibling, so the floor costs `authoring` one intra-band import and no page a cycle. One case per
    # refusal LAW, each carrying its own coordinate TUPLE — near-identical defects collapse onto one parameterized
    # case taking its subject, so the roster tracks the laws the band holds rather than the sites that raise them.
    # A case reaches the rail through the runtime's OWN door, never a local twin: `raise` inside a converting fence
    # (`boundary`/`async_boundary` on a rail capsule, the `evidence_run` weave on a caller-floor fold), and
    # `BoundaryFault.of(at, IfcFault(...))` on a pure helper reachable outside one, which is the same
    # `runtime/reliability/faults#FAULTS` fold the fence calls. That owner admits a `Tagged()` token AHEAD of every
    # `CLASSIFY` row, so this family crosses the door WHOLE on the `domain` case and the catch-all's `str(cause)` half
    # NEVER renders it — consumers inside the band match the CASE, and the coordinate reaches a receipt as the
    # `evidence` half of `facts()`, not as a string. A WORKER SEAM carries it whole too, which is the reason the
    # crossing owner exists: a kwarg-only `@tagged_union` Exception pickles on NO arm — its empty `args` plus a
    # `__dict__` carrying `_index` re-enter this union's own one-case guard — so `execution/workers#CROSSING` lowers
    # the token onto `CrossedFault` DATA at `shipped` and re-mints this family's own case parent-side, and
    # `ifc/costing#LIFECYCLE` drives `IfcSelector` worker-side under exactly that carriage while editing nothing.
    # `__str__` therefore serves the LOG and HOST edge alone — a token surfacing in a worker traceback or a log line
    # before the seam lowers it — where `Exception.__str__` answers the EMPTY string for a kwarg-only union.
    tag: Literal[
        "unrostered", "unserved", "empty_roster", "unresolved_slots", "degenerate_measure",
        "flawed_curve", "divergent_arguments", "unwirable_georeference", "unparsed_query",
    ] = tag()
    unrostered: tuple[str, str] = case()  # (the closed vocabulary refusing, the foreign token it does not carry)
    unserved: tuple[str, str] = case()  # (directional capability, the rostered member it serves in one direction only)
    empty_roster: tuple[str, IfcRoster] = case()  # (subject, the roster a fold requires non-empty and resolved empty)
    unresolved_slots: tuple[str, tuple[str, ...]] = case()  # (subject, the named slots it answers with nothing)
    degenerate_measure: tuple[str, SectionMeasure, Option[float]] = case()  # (subject, measure, value — `Nothing` where the model declares none)
    flawed_curve: tuple[str, tuple[tuple[CurveFlaw, tuple[int, ...]], ...]] = case()  # (subject, defect census at its index PATH)
    divergent_arguments: tuple[str, tuple[tuple[ArgumentFlaw, str, Option[int]], ...]] = case()  # (usecase, per-keyword divergence census)
    unwirable_georeference: tuple[tuple[GeoDrop, str], ...] = case()  # the drop census beside each offending spelling
    unparsed_query: tuple[str, ParseStop, Option[int], tuple[str, ...]] = case()  # (query, stop, `pos_in_stream` offset, admissible terminals)

    @staticmethod
    def of_stop(text: str, stop: UnexpectedInput) -> "IfcFault":
        # The two-tier constructor the branch rail law binds, so the interior never hand-builds this case and lark's
        # two divergent terminal-roster spellings are named ONCE. An exhausted parse carries `pos_in_stream == -1`,
        # a structural absence rather than an offset, so it rides `Nothing` instead of a `-1` a reader would index
        # the query with. The roster dedupes and sorts because `UnexpectedEOF` repeats a terminal per pending rule.
        row = _STOPS.choose(lambda member: Some(member) if isinstance(stop, member[0]) else Nothing).try_head()
        admissible = row.map(lambda member: getattr(stop, member[2]) or ()).default_value(())
        return IfcFault(unparsed_query=(
            text,
            row.map(lambda member: member[1]).default_value(ParseStop.UNCLASSIFIED),
            Some(stop.pos_in_stream) if stop.pos_in_stream >= 0 else Nothing,
            tuple(sorted(frozenset(str(name) for name in admissible))),
        ))

    def __str__(self) -> str:
        # the law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift from its render.
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


# --- [TABLES] ----------------------------------------------------------------------------

# this module's whole raise roster, seated beside the family it fences: the one parsing leg anchors one row, so the
# fence spells no subject and `rostered` seats the coordinate on the branch census, proving `geometry.ifc.selector`
# against a real module at import. The row carries no `slots` because it is a LIFT anchor — the offending query, the
# stop kind, its offset, and the admissible-terminal roster all ride `IfcFault.unparsed_query`'s own coordinate
# through `BoundaryFault.domain`, where a subject string would have carried one of the four and unbounded cardinality
# besides. TERMINAL: a query the grammar refuses refuses identically on every re-issue.
SELECTOR_PARSE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SELECTOR, point="parse", arm="boundary", defect="query-unparsed", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SELECTOR_PARSE]))


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
        # the fence declares the two classes its thunk raises and nothing wider: `IfcFault` for the translated stop,
        # which the faults owner admits AHEAD of every `CLASSIFY` row and carries whole onto `BoundaryFault.domain`
        # with its query, offset, and terminal roster intact, and `VisitError` for a fold defect, which reaches the
        # catch-all under lark's own wrapper class. The offending query therefore rides the typed coordinate rather
        # than the subject, where an unbounded-cardinality string also resolved no census row and dropped the
        # emitting leg from every log line the refusal reached.
        return boundary(SELECTOR_PARSE, lambda: IfcSelector._fold(text), catch=(IfcFault, VisitError)).map(IfcSelector._emit)

    @staticmethod
    def _fold(text: str) -> SelectorQuery:
        # lark's stop TRANSLATES here, inside `boundary`'s own thunk, so the `pos_in_stream` offset and the
        # admissible-terminal roster the exception carries reach the rail on a named case rather than dying at the
        # `(subject, cause)` builder — which is why the position needs no faults-owner edit and no runtime-to-`lark`
        # coupling. `transform` stays OUTSIDE the narrow catch: a `VisitError` is a fold defect, not a malformed
        # query, and it reaches the runtime catch-all unrenamed through this fence's declared `catch` set — `boundary` publishes no default.
        parser, transformer = IfcSelector._engine()
        try:
            tree = parser.parse(text)
        except UnexpectedInput as stop:
            raise IfcFault.of_stop(text, stop) from stop
        return transformer.transform(tree)

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

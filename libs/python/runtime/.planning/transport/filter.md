# [PY_RUNTIME_FILTER]

Delivery predicates over a message envelope seat here: `Cesql` is the table-driven expression owner behind the `sql` dialect, `FilterDialect` closes the seven-dialect predicate family, and `Subscription` is the specification's own resource carrying the filter AND-set beside its sink and `protocolsettings` slice. Filters decide DELIVERY and never mutate what they read, so an expression is a pure read over admitted attributes and a subscription carrying none delivers everything.

Evaluation is TOTAL by specification: every operator, function, and cast answers a VALUE beside an accumulated fault list, so a runtime error withholds one event rather than darkening a subscription, and an expression naming a missing attribute keeps matching every event whose attribute is present. `parse` alone reaches a rail, at subscription admission, where an unparseable expression refuses the subscription itself. Composed owners: `transport/event#MESSAGE` the attribute roster and its numeric carve, `transport/event#GRAMMAR` the `EventType` a subscription prefilters on, `transport/binding#BINDING` the row whose `pushdown` column decides where a dialect resolves and whose `settings` roster admits the `protocolsettings` slice, `reliability/faults#FAULT` the admission rail.

## [01]-[INDEX]

- [02]-[CESQL]: `Cesql` — the one built-once grammar, the three-space value family, the seven error types, the function and operator tables, the implicit-cast matrix, and the accumulating evaluator.
- [03]-[DIALECT]: `Subscription` and `FilterDialect` — the seven dialects, the total `Verdict`, the AND-set fold, the `protocolsettings` gate, and the pushdown derivation off the binding rows.

## [02]-[CESQL]

- Owner: `Cesql` is the compiled expression — one built-once `Lark` grammar, one `_Lower` transformer folding each production straight into a total closure, and that closure as the held value. `CesqlValue` closes the specification's three value spaces, `Outcome` pairs a value with its accumulated faults, and `CesqlFault` closes the seven specification error types. `FUNCTIONS`, `OPERATORS`, and `CASTS` are the three tables every arm reads.
- Law: `Integer` is 32-BIT and Python's is not, so the width is BRANCH-owned outright. `_bounded` is the one guard every arithmetic arm and `ABS` reads, answering the wrapped value beside a `math` fault where the true result leaves the range. `ABS(-2147483648)` is the discriminating vector — the negation of the minimum has no 32-bit representation, and an unguarded Python `abs` answers `2147483648` silently, the inverse of the .NET peer's checked throw and a value every naive test passes. Specification overflow answers a fault row beside a defined value, never a raise and never a widened answer.
- Law: `CesqlValue` is a `@tagged_union` and never a `str | int | bool` union, because `bool` SUBCLASSES `int` in Python — a structural match routes `True` through the number arm and an `isinstance` ladder depends on a bool-first ordering every later editor must remember. `tag` makes that misread unspellable and IS the key the cast matrix reads.
- Law: evaluation compiles to CLOSURES rather than to a node family beside an evaluator. `_Lower` runs INSIDE the LALR parse and answers a `Callable[[Reading], Outcome]` per production, so the parser's own discrimination is the only dispatch and no second `match` re-reads a tree it already resolved. Growth lands as one rule beside one method, one `FUNCTIONS` row, or one `OPERATORS` row, with the parser, the cast matrix, and the evaluator untouched.
- Law: the grammar is one LAYERED precedence cascade — disjunction, exclusive, conjunction, negation, predicate, comparison, additive, term, factor, primary — each level left-recursive so `parser="lalr"` builds a deterministic table and no rung re-enters a mutable parser state. `NOT LIKE` and `NOT IN` spell their OWN alternatives rather than factoring an optional `NOT` ahead of both, because the factored form needs two tokens of lookahead to separate the suffixes while the spelled form needs one; a prefix `NOT` never collides with either, since it can only open an expression.
- Law: a `LIKE` pattern compiles at COMPILE time and never per event — `_pattern` translates the specification's `%` and `_` wildcards under their backslash escapes into one `re.Pattern` the closure holds, so a subscription pays one translation for its whole life and each delivery pays a `fullmatch`.
- Law: an attribute an expression names resolves against the reading the message-envelope owner published, so an unrostered name answers `attribute` beside the empty string rather than reaching whatever untyped value a producer happened to set. `EXISTS` is the one arm reading absence WITHOUT that fault, which is the whole reason the specification gives it an operator.
- Law: casts are TOTAL — a cast that cannot succeed answers the target space's ZERO beside a `cast` fault, because a raise makes one malformed attribute value darken an otherwise-matching subscription. `_number` and `_flag` read the specification's own spellings and guess at nothing, so a truthiness read never makes every non-empty attribute satisfy a boolean filter.
- Law: faults UNION across operands and every lift carries its own — `_carried` is the one combination law, so a binary node reports both sides, a unary arm reports the cast it took, and no arm reaches a value by dropping a diagnosis it raised.
- Law: both refusals resolve a `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.FILTER` — `FILTER_PARSE` the admission fence and `FILTER_SETTINGS` the protocol-slice gate carrying the protocol beside the stray keys as NAMED coordinates.
- Entry: `Cesql.compiled(source)` parses once and rails `parse` through the faults owner; `Cesql.answered(reading)` runs the held closure per event. That pair is the whole surface — no second entry takes a tree, a token stream, or a pre-parsed node.
- Auto: a compiled expression is a VALUE held for the subscription's life, because a grammar rebuilt per event reconstructs the whole closure graph on every delivery.
- Packages: `lark` (`Lark(grammar, parser="lalr", start=, maybe_placeholders=False, transformer=)` the built-once parser folding in-parse, `Transformer` + `v_args(inline=True)` the fold, `Token` the terminal carrier, `UnexpectedInput` the parse refusal, `VisitError` the wrapper a transformer raise arrives under — `cache=` stays unset, since a cache file is a filesystem side effect no composition asked for); `expression` (`tagged_union` the value and fault families, `Block`/`Map` the immutable carriers); `msgspec` (`Struct` the frozen rows and the outcome); runtime (`event.MessageEnvelope`/`NUMERIC_EXTENSIONS` the attribute reading, `faults.RuntimeRail`/`boundary` the admission rail).
- Growth: a new function is one `FUNCTIONS` row carrying its arity and its total body; a new operator is one `OPERATORS` row beside one grammar terminal alternative; a new value space or error type is a specification move, not a branch one.
- Boundary: expression compilation and total evaluation only. Mints no dialect, no subscription, and no binding. Rejected: a recursive-descent walk over mutable parser state; a node family beside an evaluator re-dispatching what the parser already discriminated; a raise escaping any arm; a widened 64-bit answer where the specification's `Integer` is 32-bit; a `LIKE` pattern translated per event; a `str | int | bool` value union; a unary arm dropping its cast's fault.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import re
from collections.abc import Callable
from typing import Final, Literal, Self, assert_never

from expression import case, tag, tagged_union
from expression.collections import Block, Map
from lark import Lark, Token, Transformer, UnexpectedInput, VisitError, v_args
from msgspec import Struct

from rasm.runtime.event import NUMERIC_EXTENSIONS, MessageEnvelope
from rasm.runtime.faults import FILTER_PARSE, RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

type Space = Literal["text", "number", "flag"]
type Reading = Map[str, "CesqlValue"]
type Eval = Callable[[Reading], "Outcome"]

# --- [CONSTANTS] ------------------------------------------------------------------------

INT32_MIN: Final[int] = -(2**31)
INT32_MAX: Final[int] = 2**31 - 1
INT32_SPAN: Final[int] = INT32_MAX - INT32_MIN + 1
TRUE_TEXT: Final[frozenset[str]] = frozenset({"true", "TRUE", "True"})
FALSE_TEXT: Final[frozenset[str]] = frozenset({"false", "FALSE", "False"})
LIKE_WILDCARD: Final[Map[str, str]] = Map.of_seq([("%", ".*"), ("_", ".")])

GRAMMAR: Final[str] = r"""
?expression:  disjunction
?disjunction: disjunction OR exclusive            -> binary
            | exclusive
?exclusive:   exclusive XOR conjunction           -> binary
            | conjunction
?conjunction: conjunction AND negation            -> binary
            | negation
?negation:    NOT negation                        -> negate
            | predicate
?predicate:   comparison
            | comparison LIKE STRING              -> like
            | comparison NOT LIKE STRING          -> unlike
            | comparison IN "(" arguments ")"     -> within
            | comparison NOT IN "(" arguments ")" -> without
?comparison:  comparison COMPARE additive         -> binary
            | additive
?additive:    additive ADDOP term                 -> binary
            | term
?term:        term MULOP factor                   -> binary
            | factor
?factor:      "-" factor                          -> negative
            | EXISTS IDENTIFIER                   -> exists
            | primary
?primary:     INT                                 -> number
            | STRING                              -> text
            | BOOL                                -> flag
            | IDENTIFIER "(" arguments ")"        -> call
            | IDENTIFIER                          -> attribute
            | "(" expression ")"
arguments:    expression ("," expression)*

OR:      "OR"i
XOR:     "XOR"i
AND:     "AND"i
NOT:     "NOT"i
LIKE:    "LIKE"i
IN:      "IN"i
EXISTS:  "EXISTS"i
BOOL:    "TRUE"i | "FALSE"i
COMPARE: "<=" | ">=" | "<>" | "!=" | "=" | "<" | ">"
ADDOP:   "+" | "-"
MULOP:   "*" | "/" | "%"
IDENTIFIER: /[A-Za-z_][A-Za-z0-9_]*/
INT:     /[0-9]+/
STRING:  /'([^'\\]|\\.)*'/ | /"([^"\\]|\\.)*"/

%import common.WS
%ignore WS
"""

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class CesqlValue:
    tag: Space = tag()
    text: str = case()
    number: int = case()
    flag: bool = case()


@tagged_union(frozen=True)
class CesqlFault:
    tag: Literal["parse", "math", "cast", "attribute", "function", "evaluation", "generic"] = tag()
    parse: str = case()
    math: tuple[str, str] = case()
    cast: tuple[Space, Space] = case()
    attribute: str = case()
    function: str = case()
    evaluation: tuple[str, str] = case()
    generic: str = case()


NO_FAULTS: Final[Block[CesqlFault]] = Block.empty()
EMPTY_TEXT: Final[CesqlValue] = CesqlValue(text="")


class Outcome(Struct, frozen=True, gc=False):
    value: CesqlValue
    faults: Block[CesqlFault] = NO_FAULTS

    @classmethod
    def of(cls, value: CesqlValue, /) -> Self:
        return cls(value=value)

    def faulted(self, fault: CesqlFault, /) -> Self:
        return Outcome(value=self.value, faults=self.faults.cons(fault))


class FunctionRow(Struct, frozen=True, gc=False):
    key: str
    arity: int
    body: Callable[[Block[CesqlValue]], Outcome]

    def admits(self, count: int, /) -> bool:
        return count > 0 if self.arity < 0 else count == self.arity


class OperatorRow(Struct, frozen=True, gc=False):
    symbol: str
    operand: Space
    body: Callable[[CesqlValue, CesqlValue], Outcome]


class Cesql(Struct, frozen=True, gc=False):
    source: str
    run: Eval

    @classmethod
    def compiled(cls, source: str, /) -> RuntimeRail[Self]:
        return boundary(FILTER_PARSE, lambda: _PARSER.parse(source), catch=(UnexpectedInput, VisitError)).map(
            lambda run: cls(source=source, run=run)
        )

    def answered(self, reading: Reading, /) -> Outcome:
        return self.run(reading)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _carried(answered: Outcome, faults: Block[CesqlFault], /) -> Outcome:
    return Outcome(value=answered.value, faults=faults.append(answered.faults))


def _bounded(operator: str, held: int, /) -> Outcome:
    wrapped = (held - INT32_MIN) % INT32_SPAN + INT32_MIN
    answered = Outcome.of(CesqlValue(number=wrapped))
    return answered if wrapped == held else answered.faulted(CesqlFault(math=(operator, "<int32-overflow>")))


def _divided(operator: str, left: int, right: int, /) -> Outcome:
    return (
        Outcome.of(CesqlValue(number=0)).faulted(CesqlFault(math=(operator, "<divide-by-zero>")))
        if right == 0
        else _bounded(operator, left // right if operator == "/" else left % right)
    )


def _text(value: CesqlValue, /) -> Outcome:
    match value:
        case CesqlValue(tag="text"):
            return Outcome.of(value)
        case CesqlValue(tag="number", number=held):
            return Outcome.of(CesqlValue(text=str(held)))
        case CesqlValue(tag="flag", flag=held):
            return Outcome.of(CesqlValue(text="true" if held else "false"))
        case _ as unreachable:
            assert_never(unreachable)


def _number(value: CesqlValue, /) -> Outcome:
    match value:
        case CesqlValue(tag="number"):
            return Outcome.of(value)
        case CesqlValue(tag="flag", flag=held):
            return Outcome.of(CesqlValue(number=1 if held else 0))
        case CesqlValue(tag="text", text=held) if held.lstrip("+-").isdigit():
            return _bounded("CAST", int(held))
        case CesqlValue(tag="text"):
            return Outcome.of(CesqlValue(number=0)).faulted(CesqlFault(cast=("text", "number")))
        case _ as unreachable:
            assert_never(unreachable)


def _flag(value: CesqlValue, /) -> Outcome:
    match value:
        case CesqlValue(tag="flag"):
            return Outcome.of(value)
        case CesqlValue(tag="number", number=held):
            return Outcome.of(CesqlValue(flag=held != 0))
        case CesqlValue(tag="text", text=held) if held in TRUE_TEXT or held in FALSE_TEXT:
            return Outcome.of(CesqlValue(flag=held in TRUE_TEXT))
        case CesqlValue(tag="text"):
            return Outcome.of(CesqlValue(flag=False)).faulted(CesqlFault(cast=("text", "flag")))
        case _ as unreachable:
            assert_never(unreachable)


def _sliced(args: Block[CesqlValue], /, *, from_start: bool) -> Outcome:
    held, width = _text(args[0]), _number(args[1])
    span = min(max(width.value.number, 0), len(held.value.text))
    return _carried(
        Outcome.of(CesqlValue(text=held.value.text[:span] if from_start else held.value.text[len(held.value.text) - span :])),
        held.faults.append(width.faults),
    )


def _substring(args: Block[CesqlValue], /) -> Outcome:
    held, at, width = _text(args[0]), _number(args[1]), _number(args[2])
    carried = held.faults.append(at.faults).append(width.faults)
    return _carried(
        Outcome.of(CesqlValue(text=held.value.text[at.value.number - 1 : at.value.number - 1 + width.value.number]))
        if at.value.number >= 1 and width.value.number >= 0 and at.value.number - 1 + width.value.number <= len(held.value.text)
        else Outcome.of(EMPTY_TEXT).faulted(CesqlFault(evaluation=("SUBSTRING", "<range>"))),
        carried,
    )


def _pattern(literal: str, /) -> re.Pattern[str]:
    return re.compile(
        "".join(
            LIKE_WILDCARD.try_find(part).default_with(lambda: re.escape(part.removeprefix("\\")))
            for part in re.split(r"(\\.|[%_])", literal)
        )
    )


def _reading(envelope: MessageEnvelope, /) -> Reading:
    return Map.of_seq(
        (name, CesqlValue(number=int(held)) if name in NUMERIC_EXTENSIONS else CesqlValue(text=str(held)))
        for name, held in envelope.attributes().items()
    )


# --- [TABLES] ---------------------------------------------------------------------------

CASTS: Final[Map[Space, Callable[[CesqlValue], Outcome]]] = Map.of_seq([("text", _text), ("number", _number), ("flag", _flag)])

FUNCTIONS: Final[Map[str, FunctionRow]] = Map.of_seq(
    (row.key, row)
    for row in (
        FunctionRow("LENGTH", 1, lambda args: _bounded("LENGTH", len(_text(args[0]).value.text))),
        FunctionRow("CONCAT", -1, lambda args: Outcome.of(CesqlValue(text="".join(_text(held).value.text for held in args)))),
        FunctionRow("LOWER", 1, lambda args: _carried(Outcome.of(CesqlValue(text=_text(args[0]).value.text.casefold())), NO_FAULTS)),
        FunctionRow("UPPER", 1, lambda args: Outcome.of(CesqlValue(text=_text(args[0]).value.text.upper()))),
        FunctionRow("TRIM", 1, lambda args: Outcome.of(CesqlValue(text=_text(args[0]).value.text.strip()))),
        FunctionRow("LEFT", 2, lambda args: _sliced(args, from_start=True)),
        FunctionRow("RIGHT", 2, lambda args: _sliced(args, from_start=False)),
        FunctionRow("SUBSTRING", 3, _substring),
        FunctionRow("ABS", 1, lambda args: _carried(_bounded("ABS", abs(_number(args[0]).value.number)), _number(args[0]).faults)),
    )
)

OPERATORS: Final[Map[str, OperatorRow]] = Map.of_seq(
    (row.symbol, row)
    for row in (
        OperatorRow("+", "number", lambda left, right: _bounded("+", left.number + right.number)),
        OperatorRow("-", "number", lambda left, right: _bounded("-", left.number - right.number)),
        OperatorRow("*", "number", lambda left, right: _bounded("*", left.number * right.number)),
        OperatorRow("/", "number", lambda left, right: _divided("/", left.number, right.number)),
        OperatorRow("%", "number", lambda left, right: _divided("%", left.number, right.number)),
        OperatorRow("=", "text", lambda left, right: Outcome.of(CesqlValue(flag=left == right))),
        OperatorRow("!=", "text", lambda left, right: Outcome.of(CesqlValue(flag=left != right))),
        OperatorRow("<>", "text", lambda left, right: Outcome.of(CesqlValue(flag=left != right))),
        OperatorRow("<", "number", lambda left, right: Outcome.of(CesqlValue(flag=left.number < right.number))),
        OperatorRow("<=", "number", lambda left, right: Outcome.of(CesqlValue(flag=left.number <= right.number))),
        OperatorRow(">", "number", lambda left, right: Outcome.of(CesqlValue(flag=left.number > right.number))),
        OperatorRow(">=", "number", lambda left, right: Outcome.of(CesqlValue(flag=left.number >= right.number))),
        OperatorRow("AND", "flag", lambda left, right: Outcome.of(CesqlValue(flag=left.flag and right.flag))),
        OperatorRow("OR", "flag", lambda left, right: Outcome.of(CesqlValue(flag=left.flag or right.flag))),
        OperatorRow("XOR", "flag", lambda left, right: Outcome.of(CesqlValue(flag=left.flag is not right.flag))),
    )
)

# --- [COMPOSITION] ----------------------------------------------------------------------


def _lifted(held: Outcome, space: Space, project: Callable[[CesqlValue], CesqlValue], /) -> Outcome:
    cast = CASTS[space](held.value)
    return Outcome(value=project(cast.value), faults=held.faults.append(cast.faults))


def _applied(row: OperatorRow, left: Outcome, right: Outcome, /) -> Outcome:
    cast = CASTS[row.operand]
    lifted, righted = cast(left.value), cast(right.value)
    return _carried(row.body(lifted.value, righted.value), left.faults.append(right.faults).append(lifted.faults).append(righted.faults))


def _invoked(row: FunctionRow, arguments: Block[Eval], reading: Reading, /) -> Outcome:
    answered = arguments.map(lambda held: held(reading))
    carried = answered.fold(lambda held, outcome: held.append(outcome.faults), NO_FAULTS)
    return (
        Outcome(value=EMPTY_TEXT, faults=carried.cons(CesqlFault(evaluation=(row.key, "<arity>"))))
        if not row.admits(len(answered))
        else _carried(row.body(answered.map(lambda outcome: outcome.value)), carried)
    )


def _member(held: Outcome, members: Block[Eval], reading: Reading, /) -> Outcome:
    cast = CASTS[held.value.tag]
    answered = members.map(lambda member: cast(member(reading).value))
    return Outcome(
        value=CesqlValue(flag=not answered.forall(lambda member: member.value != held.value)),
        faults=answered.fold(lambda carried, member: carried.append(member.faults), held.faults),
    )


@v_args(inline=True)
class _Lower(Transformer[Token, Eval]):
    def number(self, token: Token, /) -> Eval:
        held = _bounded("LITERAL", int(token))
        return lambda _reading: held

    def text(self, token: Token, /) -> Eval:
        held = Outcome.of(CesqlValue(text=str(token)[1:-1]))
        return lambda _reading: held

    def flag(self, token: Token, /) -> Eval:
        held = Outcome.of(CesqlValue(flag=str(token).upper() == "TRUE"))
        return lambda _reading: held

    def attribute(self, token: Token, /) -> Eval:
        name = str(token)
        return lambda reading: (
            reading.try_find(name).map(Outcome.of).default_with(lambda: Outcome.of(EMPTY_TEXT).faulted(CesqlFault(attribute=name)))
        )

    def exists(self, _keyword: Token, token: Token, /) -> Eval:
        name = str(token)
        return lambda reading: Outcome.of(CesqlValue(flag=reading.try_find(name).is_some()))

    def call(self, token: Token, arguments: Block[Eval], /) -> Eval:
        name = str(token)
        return lambda reading: (
            FUNCTIONS.try_find(name.upper())
            .map(lambda row: _invoked(row, arguments, reading))
            .default_with(lambda: Outcome.of(EMPTY_TEXT).faulted(CesqlFault(function=name)))
        )

    def binary(self, left: Eval, token: Token, right: Eval, /) -> Eval:
        row = OPERATORS[str(token).upper()]
        return lambda reading: _applied(row, left(reading), right(reading))

    def negate(self, _keyword: Token, held: Eval, /) -> Eval:
        return lambda reading: _lifted(held(reading), "flag", lambda value: CesqlValue(flag=not value.flag))

    def negative(self, held: Eval, /) -> Eval:
        return lambda reading: _negated(held(reading))

    def like(self, held: Eval, _keyword: Token, token: Token, /) -> Eval:
        matcher = _pattern(str(token)[1:-1])
        return lambda reading: _lifted(held(reading), "text", lambda value: CesqlValue(flag=matcher.fullmatch(value.text) is not None))

    def unlike(self, held: Eval, _negation: Token, keyword: Token, token: Token, /) -> Eval:
        matched = self.like(held, keyword, token)
        return lambda reading: _flipped(matched(reading))

    def within(self, held: Eval, members: Block[Eval], /) -> Eval:
        return lambda reading: _member(held(reading), members, reading)

    def without(self, held: Eval, _negation: Token, members: Block[Eval], /) -> Eval:
        return lambda reading: _flipped(_member(held(reading), members, reading))

    def arguments(self, *held: Eval) -> Block[Eval]:
        return Block.of_seq(held)


def _negated(held: Outcome, /) -> Outcome:
    cast = CASTS["number"](held.value)
    return _carried(_bounded("-", -cast.value.number), held.faults.append(cast.faults))


def _flipped(held: Outcome, /) -> Outcome:
    return Outcome(value=CesqlValue(flag=not held.value.flag), faults=held.faults)


_PARSER: Final[Lark] = Lark(GRAMMAR, parser="lalr", start="expression", maybe_placeholders=False, transformer=_Lower())
```

## [03]-[DIALECT]

- Owner: `Subscription` is the specification's resource as one frozen value — `id`, `source`, the `types` prefilter, `config`, the `filters` AND-set, `sink`, `protocol`, and its `settings` slice — and `FilterDialect` the closed seven-case family it carries. `Verdict` is the total answer every dialect returns, a delivery bit beside the faults its evaluation accumulated, so no arm chooses between reporting a value and reporting a diagnosis.
- Cases: `exact` case-sensitive equality, `prefix` and `suffix` the two affix tests, `all` and `any` the recursive conjunction and disjunction, `not_` the recursive negation, `sql` a COMPILED expression. Specification law makes `sql` OPTIONAL and this fabric makes it mandatory, because a subscription able to express only attribute affixes pushes every real routing decision back into a consumer that must decode the payload to make it.
- Law: the case tag `not_` carries a trailing underscore because `not` is a keyword while the wire dialect name is not; `wired` strips it, so the wire vocabulary derives from the family rather than a second roster a case rename leaves standing.
- Law: the AND-set's identity is DELIVERY, so a subscription carrying no filter delivers, `all` folds from `PASS`, and `any` folds from `WITHHOLD` — each empty child set answers its own operator's identity rather than a fabricated verdict. `not_` inverts the bit and PRESERVES the faults, because a fault under a negation is still a fault the operator never observed.
- Law: the three affix dialects are MEMBERSHIP tests and an absent attribute answers false with NO fault, which is exactly the distinction `sql`'s `attribute` fault draws — a missing attribute is a legitimate non-match for an affix and a diagnosable read for an expression. All four read one `_reading` projection per event, so N filters over one message envelope share one attribute view rather than re-deriving it per arm.
- Law: `settings` admits against the binding row's OWN `settings` roster, so a key outside that slice refuses at subscription admission and never at a delivery. `sink` and `protocol` name where the fan lands; the connection is `transport/binding#ADAPTER`'s and no lane reaches this page.
- Law: pushdown is the BINDING row's `pushdown` column joined to the dialect, never a dialect property — `exact` and `prefix` resolve at a broker row on its routing attribute, `suffix` and `not_` never do, a composite pushes only where every child does, and `sql` is consumer-side on every row because no broker in the roster evaluates it. `pushed` derives that join off `BINDINGS`, so a seventh binding row reaches it untouched.
- Entry: `Subscription.admitted(...)` is the one construction — it proves the `settings` slice and COMPILES every `sql` expression once, so an unparseable expression rails here and no delivery parses text. `Subscription.delivered(envelope)` answers the folded `Verdict`, and `FilterDialect.verdict(reading)` is the one recursive arm every case rides.
- Output: accumulated faults ride the emitter's own `Delivery` beside the withheld count at `transport/binding#EMISSION`, so an expression quietly erroring on every event reads as a rate rather than as silence; this owner mints none.
- Growth: a new dialect is one `FilterDialect` case, one `verdict` arm, and one `BROKER_ELIGIBLE` membership; a new protocol setting is one key on the binding row's own `settings`; a new pushdown mechanism is one `Pushdown` value at the binding owner.
- Boundary: delivery predicates only. Mints no binding, no lane, no message envelope, no outcome semantics, and no persistence for the subscription itself. Rejected: a filter mutating what it reads; an affix dialect faulting on an absent attribute; a dialect carrying its own pushdown column beside the binding row that owns it; a `sql` expression parsed at delivery; a `settings` key outside its row's slice.

| [INDEX] | [DIALECT] | [SHAPE]                                     | [PUSHDOWN]                                   |
| :-----: | :-------- | :------------------------------------------ | :------------------------------------------- |
|  [01]   | `exact`   | attribute to value, case-sensitive equality | broker rows, on the routing attribute        |
|  [02]   | `prefix`  | attribute to value `startswith`             | broker rows where the attribute IS the route |
|  [03]   | `suffix`  | attribute to value `endswith`               | consumer-side on every row                   |
|  [04]   | `all`     | recursive conjunction over nested dialects  | only where every child pushes down           |
|  [05]   | `any`     | recursive disjunction over nested dialects  | only where every child pushes down           |
|  [06]   | `not_`    | recursive negation                          | consumer-side on every row                   |
|  [07]   | `sql`     | a compiled CESQL expression                 | consumer-side always                         |

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, Literal, Self, assert_never

from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.binding import BINDINGS, Binding, Pushdown
from rasm.runtime.event import EventType, MessageEnvelope
from rasm.runtime.faults import FILTER_SETTINGS, RuntimeRail


# --- [TYPES] ----------------------------------------------------------------------------

type Dialect = Literal["exact", "prefix", "suffix", "all", "any", "not_", "sql"]
type Affix = Callable[[str], bool]

# --- [CONSTANTS] ------------------------------------------------------------------------

BROKER_ELIGIBLE: Final[frozenset[Dialect]] = frozenset({"exact", "prefix"})
NO_FILTERS: Final[Block["FilterDialect"]] = Block.empty()
NO_TYPES: Final[Block[EventType]] = Block.empty()

# --- [MODELS] ---------------------------------------------------------------------------


class Verdict(Struct, frozen=True, gc=False):
    delivers: bool
    faults: Block[CesqlFault] = NO_FAULTS

    def anded(self, other: Self, /) -> Self:
        return Verdict(delivers=self.delivers and other.delivers, faults=self.faults.append(other.faults))

    def ored(self, other: Self, /) -> Self:
        return Verdict(delivers=self.delivers or other.delivers, faults=self.faults.append(other.faults))

    def negated(self, /) -> Self:
        return Verdict(delivers=not self.delivers, faults=self.faults)


PASS: Final[Verdict] = Verdict(delivers=True)
WITHHOLD: Final[Verdict] = Verdict(delivers=False)


@tagged_union(frozen=True)
class FilterDialect:
    tag: Dialect = tag()
    exact: tuple[str, str] = case()
    prefix: tuple[str, str] = case()
    suffix: tuple[str, str] = case()
    all: tuple["FilterDialect", ...] = case()
    any: tuple["FilterDialect", ...] = case()
    not_: "FilterDialect" = case()
    sql: Cesql = case()

    @property
    def wired(self) -> str:
        return self.tag.rstrip("_")

    def verdict(self, reading: Reading, /) -> Verdict:
        match self:
            case FilterDialect(tag="exact", exact=(name, held)):
                return _held(reading, name, lambda text: text == held)
            case FilterDialect(tag="prefix", prefix=(name, held)):
                return _held(reading, name, lambda text: text.startswith(held))
            case FilterDialect(tag="suffix", suffix=(name, held)):
                return _held(reading, name, lambda text: text.endswith(held))
            case FilterDialect(tag="all", all=children):
                return Block.of_seq(children).fold(lambda carried, child: carried.anded(child.verdict(reading)), PASS)
            case FilterDialect(tag="any", any=children):
                return Block.of_seq(children).fold(lambda carried, child: carried.ored(child.verdict(reading)), WITHHOLD)
            case FilterDialect(tag="not_", not_=child):
                return child.verdict(reading).negated()
            case FilterDialect(tag="sql", sql=expression):
                answered = expression.answered(reading)
                flagged = CASTS["flag"](answered.value)
                return Verdict(delivers=flagged.value.flag, faults=answered.faults.append(flagged.faults))
            case _ as unreachable:
                assert_never(unreachable)


class Subscription(Struct, frozen=True, gc=False):
    id: str
    source: str
    sink: str
    protocol: Binding
    settings: Map[str, str]
    types: Block[EventType] = NO_TYPES
    filters: Block[FilterDialect] = NO_FILTERS
    config: Map[str, str] = Map.empty()

    @classmethod
    def admitted(
        cls,
        /,
        *,
        id: str,
        source: str,
        sink: str,
        protocol: Binding,
        settings: Map[str, str],
        types: Block[EventType] = NO_TYPES,
        filters: Block[FilterDialect] = NO_FILTERS,
        config: Map[str, str] = Map.empty(),
    ) -> RuntimeRail[Self]:
        stray = Block.of_seq(sorted(key for key in settings.keys() if key not in BINDINGS[protocol].settings))
        return (
            Error(FILTER_SETTINGS.raised(protocol.value, ",".join(stray)))
            if not stray.is_empty()
            else Ok(cls(id=id, source=source, sink=sink, protocol=protocol, settings=settings, types=types, filters=filters, config=config))
        )

    def delivered(self, envelope: MessageEnvelope, /) -> Verdict:
        reading = _reading(envelope)
        typed = self.types.is_empty() or not self.types.forall(lambda held: held != envelope.event_type)
        return self.filters.fold(lambda carried, child: carried.anded(child.verdict(reading)), PASS if typed else WITHHOLD)

    def pushed(self, /) -> Block[FilterDialect]:
        return self.filters.filter(lambda child: _pushes(child, BINDINGS[self.protocol].pushdown))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _held(reading: Reading, name: str, test: Affix, /) -> Verdict:
    return reading.try_find(name).map(lambda value: Verdict(delivers=test(CASTS["text"](value).value.text))).default_value(WITHHOLD)


def _pushes(dialect: FilterDialect, pushdown: Pushdown, /) -> bool:
    match dialect:
        case FilterDialect(tag="all", all=children) | FilterDialect(tag="any", any=children):
            return Block.of_seq(children).forall(lambda child: _pushes(child, pushdown))
        case _:
            return dialect.tag in BROKER_ELIGIBLE and pushdown is not Pushdown.CONSUMER
```

## [04]-[RESEARCH]

(none)

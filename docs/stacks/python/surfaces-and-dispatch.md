# [PYTHON_SURFACES_AND_DISPATCH]

A concern with many features keeps one dense surface, never a family of shallow ones: one entrypoint absorbs every verb, arity, and modality — verbs collapse into a tagged-union request under one total `match`, arity collapses into one iterable-or-scalar parameter the body discriminates by shape, and the discriminant is the value, never a mode flag beside it. Knob sets collapse into policy values that carry their own behavior; optional context enters as `Option[T]` or one frozen settings owner whose default derives from the policy. Five dispatch forms are selected by where ownership lives — one `Protocol` core, one `frozendict` table, one `match`, one closed-family fold, one `@singledispatch` over a genuinely open type set — while the rail stays orthogonal to the form. The named defect this page refuses is surface spam: parallel near-identical functions, `get`/`get_many`/`get_by_id` families, stringly-typed dispatch ladders, one-method classes, thin rename wrappers, and barrel re-export files.

## [1]-[FORM_CHOOSER]

When a concern matches several rows, the most specific wins; the rail axis is read after the form is fixed.

| [INDEX] | [CONCERN_SIGNATURE]                         | [FORM]                               | [REJECTED_FORM]                       |
| :-----: | :------------------------------------------ | :----------------------------------- | :------------------------------------ |
|   [1]   | verb family, shared preamble                | request tagged union + total `match` | sibling `create`/`update` functions   |
|   [2]   | one verb, varying arity                     | one iterable parameter, shape-tested | per-arity overload family             |
|   [3]   | closed family owns the behavior             | case fold method on the owner        | external `match` repeated per call    |
|   [4]   | key is a value, result is static data       | module-level `frozendict` table      | `if`/`elif` returning constants       |
|   [5]   | receiver is foreign, behavior is local      | one function over the foreign value  | wrapper class renaming the receiver   |
|   [6]   | input shape, not nominal tag, discriminates | structural `match` patterns          | `isinstance` ladder over open input   |
|   [7]   | one body serving every rail                 | rail-polymorphic function            | `run_result`/`run_option` sibling set |
|   [8]   | open type set, foreign code adds arms       | `functools.singledispatch`           | `match` editing the owner per type    |
|   [9]   | optional context with identity              | one `Option[Context]`                | `a=None, b=None, strict=False` tail   |
|  [10]   | replaceable capability, many implementers   | one `Protocol` port                  | concrete dependency hardcoded inline  |

## [2]-[ENTRYPOINT_LAW]

[REQUEST_COLLAPSE]:
- Law: one concern exposes one entrypoint; a verb family is a `@tagged_union` with one `case()` per verb under one total `match`.
- Law: each sibling's distinct parameters become its case payload, and the shared preamble becomes the dispatch prologue executed once before the `match`.
- Law: `match` over the closed case set is the totality proof: a new case forces a new arm, and `case _ as unreachable: assert_never(unreachable)` makes the missing arm a static-checker failure, never a runtime-silent fall-through.
- Use: `@tagged_union` named cases when verbs carry distinct fields; a `StrEnum` discriminant on one frozen owner when the verbs share one field set and differ only by tag.
- Reject: a request shaped as success-or-failure; rails own outcome transport. A `dispatch(verb: str, **kwargs)` signature where `verb` is an open string and `kwargs` is an untyped bag.

```python conceptual
from typing import Literal, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union


type RequestFault = Literal["<empty-code>"]


@tagged_union(frozen=True)
class Request:
    tag: Literal["open", "amend", "close"] = tag()
    open: str = case()
    amend: tuple[str, int] = case()
    close: str = case()

    @staticmethod
    def of_open(code: str, /) -> Result["Request", RequestFault]:
        return Error("<empty-code>") if code == "" else Ok(Request(open=code))


def dispatched(request: Request, ledger: "Ledger", /) -> Result["Receipt", RequestFault]:
    match request:
        case Request(tag="open"):
            return ledger.open(request.open)
        case Request(tag="amend"):
            code, delta = request.amend
            return ledger.amend(code, delta)
        case Request(tag="close"):
            return ledger.close(request.close)
        case unreachable:
            assert_never(unreachable)
```

## [3]-[MODAL_ARITY]

[ARITY_ABSORPTION]:
- Law: singular, multi-item, and empty call sites collapse into one parameter typed `T | Iterable[T]`; the body normalizes once at the head and the rest of the function sees one shape, so arity is a property of the argument, never of the signature.
- Law: the normalization is one structural `match` — a lone owner, a sequence of owners, an empty sequence — and the discriminant is recoverable from the value, never restated by a `many: bool` or a `count: int` parameter the value's length already answers.
- Use: `*items: T` only when the call site genuinely lists positional arguments; a single `T | Iterable[T]` parameter when the caller already holds a collection, because `*items` forces an unpack the caller did not ask for.
- Exemption: Python has no span boundary, and `str`/`bytes` are themselves `Iterable`, so the `Iterable()` normalization arm carries one guard — `if not isinstance(stream, (str, bytes))` — to keep a string from shattering into characters; this `str`/`bytes` exclusion is the named platform-forced seam, not a free-form guard, and a domain whose lone-value type is itself iterable normalizes by a closed-owner match instead, never by a second guard.
- Reject: a `batch` flag, a `mode` flag, or sibling `process`/`process_many` functions; `singular`/`plural`/`stream` name suffixes that re-describe the input the value already carries; a guard beyond the named `str`/`bytes` seam that smuggles a discriminant the value already answers.

[MODALITY_FOLD]:
- Law: singular, plural-preserving, and plural-reducing are one arm under three combinators — `map` for singular, a comprehension or `Block.map` for plural-preserving with shape intact, a `functools.reduce` or `Block.fold` keyed on the monoid policy value for plural-reducing — the same arm, the rail or the reducer alone switching the algebra.
- Law: fail-fast traversal threads the rail through the comprehension and short-circuits on the first `Error`; accumulating traversal folds every result and combines faults — the choice is the traversal operator, never a tuning flag.
- Use: the rail's own `bind`/`map` inside a `reduce` seed-and-step so the carrier owns short-circuit; a list comprehension over already-railed values only when every element must run regardless of failure.
- Reject: a manual index counter, a mutable accumulator list appended in a `for` loop, and `zip` across unequal lengths without `zip(strict=True)` proving the arity invariant.

```python conceptual
from collections.abc import Iterable
from functools import reduce

from expression import Error, Ok, Result


def normalized[T](items: T | Iterable[T], /) -> tuple[T, ...]:
    match items:
        case tuple() | list() as many:
            return tuple(many)
        case Iterable() as stream if not isinstance(stream, (str, bytes)):
            return tuple(stream)
        case lone:
            return (lone,)


def reduced(items: "Request | Iterable[Request]", ledger: "Ledger", /) -> Result[tuple["Receipt", ...], RequestFault]:
    seed: Result[tuple["Receipt", ...], RequestFault] = Ok(())
    return reduce(
        lambda acc, request: acc.bind(lambda done: dispatched(request, ledger).map(lambda receipt: (*done, receipt))), normalized(items), seed
    )
```

## [4]-[PARAMETER_ALGEBRA]

[POLICY_VALUES]:
- Law: a policy parameter arrives pre-constructed and carries its own behavior; the entrypoint invokes the value it was handed and no `if`/`match` reconstructs at the call site what the value already encodes.
- Law: behavior-bearing policy is one frozen owner whose fields include the callable step, or a `frozendict` policy table keyed on the discriminant; a `StrEnum` member carries a column only when the column is data, and the callable lives on the owner the enum selects.
- Use: a frozen dataclass with a `Callable` field and named module-level instances (`STRICT`, `LENIENT`) as the policy family; the caller passes the instance, the entrypoint calls `policy.step(...)`.
- Reject: a `strict: bool` parameter selecting between two bodies; a behavioral near-twin chosen by flag rather than by the value that encodes the boundary behavior.

[OPTIONAL_CONTEXT]:
- Law: `Option[T]` is the single optional-parameter form; declaration is `context: Option[T] = Nothing`, consumption is `context.default_value(policy.canonical)` so the fallback derives once from the policy owner.
- Law: a nullable flag tail — `a: T | None = None, b: T | None = None, strict: bool = False` — fragments one context into parallel parameters; the collapse is one `Option[ContextRecord]` carrying the override bundle, with `ctx: T | None = None` the boundary-only spelling projected to `Option[T]` at admission.
- Law: caller-omission distinct from a valid `None` is `sentinel("MISSING")` compared by `is`, never a second boolean; the sentinel rides directly in the parameter's union type.
- Boundary: a capability orthogonal to the discriminant — a cancellation scope, a runtime settings record — describes how work runs, not which case it is, and rides the runtime owner, never the signature.

[KNOB_TEST]:
- Law: the knob test is removal — delete the parameter, and if no information is lost that the value cannot reconstruct, the parameter was a knob and the value already discriminates.
- Reject: a timeout, retry count, or deadline as an entrypoint parameter; the bound is a definition-time aspect or an `anyio` scope around the call, and the signature never grows a token tail for it.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass

from expression import Error, Nothing, Ok, Option, Result


@dataclass(frozen=True, slots=True, kw_only=True)
class Context:
    ceiling: int


@dataclass(frozen=True, slots=True, kw_only=True)
class Policy:
    canonical: Context
    step: Callable[["Input", Context], Result["Receipt", str]]


STRICT = Policy(
    canonical=Context(ceiling=1), step=lambda value, ctx: Ok(Receipt.EMPTY) if value.score <= ctx.ceiling else Error(f"<over:{value.score}>")
)
LENIENT = Policy(canonical=Context(ceiling=8), step=lambda _value, _ctx: Ok(Receipt.DEGRADED))


def run(policy: Policy, value: "Input", context: Option[Context] = Nothing, /) -> Result["Receipt", str]:
    return policy.step(value, context.default_value(policy.canonical))
```

## [5]-[DISPATCH_FORMS]

[FORM_SELECTION]:
- Law: the five forms are selected by where ownership lives — the chooser's ownership signatures are the selection law — and when two forms both fit, the one whose owner already holds the exhaustiveness obligation wins.
- Law: a closed vocabulary the program owns dispatches through a fold method on the owner or a `frozendict` table; `functools.singledispatch` is reserved for a genuinely open type set foreign code extends, where editing one `match` per new type is impossible.
- Reject: a `frozendict` table restating a closed family's own cases — a duplicate-entry burden with a silent missed-case failure when a member is added; `singledispatch` over a closed family the program owns, which trades the static exhaustiveness proof for a runtime registration scan; an `if`/`elif` ladder over a `StrEnum`'s `.value` strings.
- Boundary: mixing forms at one site signals an unresolved ownership boundary, except the valid composition — a `match` arm reading a `frozendict` table is dispatch plus data retrieval, and a function owning foreign-to-domain translation while its inner `match` owns per-case projection.

[TABLE_DISPATCH]:
- Law: a key-to-static-data correspondence is one module-level `frozendict` declared once; secondary maps derive from the primary table's values through a comprehension, never re-declared, so the correspondence has one edit site.
- Law: the table lookup returns `None` for an absent key and the call site projects that to `Option` or a typed fault at the boundary; the table never carries a sentinel value standing in for absence.
- Use: a `frozendict[K, tuple[...]]` row when the key maps to several derived values at once; the `enum` member as the key when the vocabulary is closed and process-local.
- Law: enum-value membership admits through `EnumType(value)` under `try`/`except ValueError` projected to `Option`/`Result` at the boundary, never a reach into `EnumType._value2member_map_` or any other private dunder — the public constructor is the membership test.
- Reject: a `dict` mutated after construction as a policy table; `MappingProxyType` over mutable storage; a table whose values are callables the owning vocabulary carries as case methods; an `if value in EnumType._value2member_map_` private-internal membership probe.

[OPEN_DISPATCH]:
- Law: `@singledispatch` registers one arm per concrete type and a base arm; the registered functions are the open extension axis, so foreign code adds a type by registering an arm without editing the owner — the inverse of the closed `match`.
- Law: the base `@singledispatch` arm raises or returns a typed fault for an unregistered type; dispatch resolves on the first argument's runtime type through the method resolution order, so an `ABC` registration covers its subclasses in one arm.
- Use: `singledispatch` only at a true plugin seam — a renderer over a host type hierarchy the library does not own; the closed `match` everywhere the program owns the type set.
- Boundary: the registered arms each return the rail the owner declares, so the open axis stays rail-consistent with the closed forms.

```python conceptual
from enum import StrEnum
from functools import singledispatch

from expression import Error, Nothing, Ok, Option, Result, Some
from builtins import frozendict


class Marker(StrEnum):
    PRIMARY = "<key-a>"
    SECONDARY = "<key-b>"


WEIGHT: frozendict[Marker, tuple[int, str]] = frozendict({Marker.PRIMARY: (1, "<label-a>"), Marker.SECONDARY: (2, "<label-b>")})
LABEL: frozendict[Marker, str] = frozendict({marker: label for marker, (_rank, label) in WEIGHT.items()})


def admitted_marker(raw: str, /) -> Option[Marker]:
    try:
        return Some(Marker(raw))
    except ValueError:
        return Nothing


def weighted(raw: str, /) -> Result[int, str]:
    return admitted_marker(raw).map(lambda marker: WEIGHT[marker][0]).to_result(f"<unknown:{raw}>")


@singledispatch
def rendered(value: object, /) -> str:
    raise TypeError(f"<no-arm:{type(value).__name__}>")


@rendered.register
def _(value: int, /) -> str:
    return f"<int:{value}>"


@rendered.register
def _(value: str, /) -> str:
    return f"<str:{value}>"
```

## [6]-[RAIL_POLYMORPHIC_DISPATCH]

[ONE_RAIL_SURFACE]:
- Law: the form selects which arm runs; the rail the arms return selects how results combine — orthogonal axes. One function whose body composes through `bind`/`map` serves `Result` and `Option` by member-name overlap over the union `Result[T, E] | Option[T]`, not by a shared higher-kinded carrier — `expression` exposes no common monadic supertype — so the per-rail sibling family is the rejected form while the neutral body stays restricted to the literal `map`/`bind`/`default_value` names both carriers expose.
- Law: the rail is chosen once at the function's boundary and threaded unchanged; `Result.to_option` and `Option.to_result` migrate the rail exactly once at a boundary, never mid-pipeline, because the round trip stamps over the original fault.
- Use: a generic `[T, E]` signature returning `Result[T, E]` so the function admits any fault vocabulary; `effect.result` / `effect.option` builders when sequential `bind` chains read more clearly as `yield from`.
- Boundary: the `effect.result` generator builder is the do-notation form — each `yield from` is a `bind`, the first `Error` short-circuits the generator, and the `return` is the final `Ok`; the builder never sees a raw exception because admission already railed it.

[INDEPENDENT_JOIN]:
- Law: independent computations combine through `map2` (two operands) or a railed comprehension over a fixed tuple, and dependent steps chain through `bind`; the choice is load-bearing because a `bind` chain over independent operands reports only the first failure.
- Law: `Result.map2(other, f)` runs both operands and applies `f` to the pair, short-circuiting on the first `Error`; an accumulating join that reports every independent fault folds the faults through the error type's own combination before reporting.
- Reject: a branch inside the join whose only legitimate content is total construction over already-railed values; a branch is a fourth dispatch smuggled into combination and lifts into its own arm.
- Boundary: all operands share one rail by construction, so failure semantics decide once at the function boundary.

```python conceptual
from expression import Error, Ok, Result, effect


@effect.result[int, str]()
def assembled(source: str, band: str, tag: str):
    code = yield from (Ok(source) if source != "" else Error("<empty-source>"))
    rank = yield from (Ok(len(band)) if band != "" else Error("<empty-band>"))
    key = yield from (Ok(tag.upper()) if tag != "" else Error("<empty-tag>"))
    return len(code) + rank + len(key)


def joined(left: Result[int, str], right: Result[int, str], /) -> Result[int, str]:
    return left.map2(right, lambda a, b: a + b)
```

## [7]-[TYPE_LEVEL_DISPATCH]

[PROTOCOL_PORT]:
- Law: a `Protocol` is the open boundary — the inversion of the closed `match`: the `match` is closed over one owner's cases, the protocol is open over the unbounded family of implementers, resolved structurally with no registration, no nominal base, no `isinstance`.
- Law: the port declares the smallest replaceable operation family and every method returns the rail the domain declares, so an implementer is verified structurally against the protocol and the failure surface stays a closed fault family across implementers.
- Use: one generic function constrained `[S: Port]` that composes the port's operations through the rail; the port keyed by `type[Port]` when injected through a runtime settings owner.
- Reject: a one-method `Protocol` where `Callable` already states the contract; a protocol per variant simulating a closed family; a `@runtime_checkable` protocol used as an `isinstance` gate where a `TypeIs` predicate proves exact membership.

[REACH_LIMIT]:
- Law: structural typing is static; a runtime-discovered implementer needs `@runtime_checkable` plus an `isinstance` gate, which checks method presence only, never signatures — so a runtime gate pairs with a `TypeIs` predicate when semantic membership exceeds member presence.
- Use: `typing.get_protocol_members` for registration or proof of port coverage; never per-request structural validation, which re-pays the scan on every call.
- Boundary: choosing a runtime-checkable protocol for static-resolvable dispatch reintroduces a reflective scan the structural form deletes.

```python conceptual
from dataclasses import dataclass, field, replace
from typing import Protocol

from expression import Error, Ok, Result
from builtins import frozendict


@dataclass(frozen=True, slots=True, kw_only=True)
class Shape:
    key: str
    value: str


class ShapeStore(Protocol):
    def loaded(self, key: str, /) -> Result[Shape, str]: ...
    def stored(self, shape: Shape, /) -> Result["ShapeStore", str]: ...


@dataclass(frozen=True, slots=True, kw_only=True)
class MemoryStore:
    rows: frozendict[str, Shape] = field(default_factory=frozendict)

    def loaded(self, key: str, /) -> Result[Shape, str]:
        return Ok(self.rows[key]) if key in self.rows else Error("<missing>")

    def stored(self, shape: Shape, /) -> Result["MemoryStore", str]:
        return Ok(replace(self, rows=self.rows | {shape.key: shape}))


def refreshed[S: ShapeStore](store: S, key: str, value: str, /) -> Result[S, str]:
    return store.loaded(key).map(lambda shape: replace(shape, value=value)).bind(store.stored)
```

## [8]-[ASPECTS]

[WEAVE_TAXONOMY]:
- Law: a definition-time aspect is a property of the function — declared by a signature-preserving decorator, present at every call site; a call-site aspect is a property of one invocation — attached as a scope or combinator around the call it modifies.
- Law: the classification test is per-site variance — a concern present at every use weaves at definition (retry policy, runtime contract, observability span, memoization), a concern that varies per site composes at the site (one deadline, one cancellation scope, one resource bracket).
- Law: a definition-time aspect preserves the signature and the rail through inline `**P` and `functools.wraps`, materializes policy from its arguments, and never raises inside domain flow — a failing aspect returns the rail's `Error`, it does not throw past the wrapped function.

[DECORATOR_STACKING]:
- Law: decorators apply bottom-up at definition and execute top-down at call; the same two aspects in two orders are two policies — retry around contract validation re-validates per attempt, contract validation around retry validates once and retries only the body.
- Law: two-to-four recurring wrappers that always co-occur collapse into one parameterized aspect factory whose arguments select the composed policy, so the stack is one decision, not a fixed tower re-typed at every owner.
- Use: `stamina.retry(on=..., attempts=...)` as the retry aspect, `beartype` as the runtime-contract aspect, and a structlog-bound span as the observability aspect; one factory composing them when they recur together.
- Reject: a bare `try`/`except` retry loop; a hand-written timer for an observability span; a wrapper that erases the wrapped signature by typing it `Callable[..., Any]`.

```python conceptual
from collections.abc import Callable
from functools import wraps

import stamina
from beartype import beartype
from expression import Error, Result


def aspected[**P, T, E](*, on: type[Exception], attempts: int = 3) -> Callable[[Callable[P, Result[T, E]]], Callable[P, Result[T, E]]]:
    def weave(operation: Callable[P, Result[T, E]], /) -> Callable[P, Result[T, E]]:
        retried = stamina.retry(on=on, attempts=attempts)(beartype(operation))

        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> Result[T, E]:
            return retried(*args, **kwargs)

        return call

    return weave


@aspected(on=ConnectionError, attempts=5)
def fetched(code: str, /) -> Result[str, str]:
    return Error("<unreachable>") if code == "" else Result.Ok(code.upper())
```

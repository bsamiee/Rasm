# [PYTHON_SURFACES_AND_DISPATCH]

A concern with many features keeps one dense surface, never a family of shallow ones: one entrypoint absorbs every verb, arity, and modality — verbs collapse into a `@tagged_union` request under one total `match` so a new verb breaks every dispatch site instead of growing a sibling, arity collapses into one `T | Iterable[T]` parameter the body discriminates by shape, and a dependent dispatch loop is one `tailrec` trampoline over a closed continue-or-done step rather than an open recursion. The discriminant is the value, never a mode flag beside it; knob sets collapse into policy values that carry their own behavior, and optional context enters as `Option[T]` or one frozen settings owner whose default derives from the policy. Six dispatch forms are selected by where ownership lives — one `Protocol` core, one `frozendict` table, one closed-family `match`, behavior carried on the vocabulary member, one `tailrec` step, one `functools.singledispatch` over a genuinely open type set — while the rail stays orthogonal to the form and alone decides accumulate-versus-abort. Aspects split at one seam: a definition-time weave preserves the signature and the rail through inline `**P` and `functools.wraps` and never raises into domain flow, a call-site combinator composes around the one invocation it modifies. The named defect this page refuses is surface spam: parallel near-identical functions, `get`/`get_many`/`get_by_id` families, stringly-typed dispatch ladders, one-method classes, thin rename wrappers, and barrel re-export files.

## [01]-[FORM_CHOOSER]

When a concern matches several rows, the most specific wins; the rail axis is read after the form is fixed.

| [INDEX] | [CONCERN_SIGNATURE]                         | [FORM]                               | [REJECTED_FORM]                       |
| :-----: | :------------------------------------------ | :----------------------------------- | :------------------------------------ |
|  [01]   | verb family, shared preamble                | request tagged union + total `match` | sibling `create`/`update` functions   |
|  [02]   | one verb, varying arity                     | one `T \| Iterable[T]`, shape-tested | per-arity overload family             |
|  [03]   | consumer owns logic, family owns coverage   | closed-family `match` on the owner   | external `match` repeated per call    |
|  [04]   | the vocabulary member is the behavior       | callable carried on the member       | full-coverage `match` per call site   |
|  [05]   | key is a value, result is static data       | module-level `frozendict` table      | `if`/`elif` returning constants       |
|  [06]   | dependent dispatch loops to a fixpoint      | `tailrec` continue-or-done step      | open recursion or a `while` mutator   |
|  [07]   | receiver is foreign, behavior is local      | one function over the foreign value  | wrapper class renaming the receiver   |
|  [08]   | input shape, not nominal tag, discriminates | structural `match` patterns          | `isinstance` ladder over open input   |
|  [09]   | one body serving every rail                 | union member-overlap function        | `run_result`/`run_option` sibling set |
|  [10]   | open type set, foreign code adds arms       | `functools.singledispatch`           | `match` editing the owner per type    |
|  [11]   | optional context with identity              | one `Option[Context]`                | `a=None, b=None, strict=False` tail   |
|  [12]   | replaceable capability, many implementers   | one `Protocol` port                  | concrete dependency hardcoded inline  |

## [02]-[ENTRYPOINT_LAW]

[REQUEST_COLLAPSE]:
- Law: one concern exposes one entrypoint; a verb family is a `@tagged_union` with one `case()` per verb under the settled total `match`, and each sibling's distinct parameters become its case payload while the shared preamble becomes the dispatch prologue executed once before the `match` — the verb set is the closed family the exhaustiveness mechanic proves, so a new verb is one case that breaks every dispatch arm at type-check, never a sibling `create`/`update` function beside the union.
- Law: admission is two-tier — the private `@tagged_union` constructor seals the case family, and one `of_*` classmethod returning `Result[Request, Fault]` is the validated ingress, so the dispatch interior is total over well-formed requests and never re-checks a field the factory already proved; a verb's independent field checks compose applicatively through `map2` over the field rails rather than a nested abort ladder, and whether that composition short-circuits or accumulates is the `[06]` join decision, fixed once at the factory boundary.
- Use: `@tagged_union` named cases when verbs carry distinct fields; a `StrEnum` discriminant on one frozen owner when the verbs share one field set and differ only by tag.
- Reject: a request shaped as success-or-failure — rails own outcome transport; a `dispatch(verb: str, **kwargs)` signature where `verb` is an open string and `kwargs` is an untyped bag.

```python conceptual
from typing import Literal, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union


type RequestFault = Literal["<empty-code>", "<bad-delta>"]


def _checked_code(code: str, /) -> Result[str, RequestFault]:
    return Ok(code) if code else Error("<empty-code>")


def _checked_delta(delta: int, /) -> Result[int, RequestFault]:
    return Ok(delta) if delta != 0 else Error("<bad-delta>")


@tagged_union(frozen=True)
class Request:
    tag: Literal["open", "amend", "close"] = tag()
    open: str = case()
    amend: tuple[str, int] = case()
    close: str = case()

    @staticmethod
    def of_open(code: str, /) -> Result["Request", RequestFault]:
        return _checked_code(code).map(lambda valid: Request(open=valid))

    @staticmethod
    def of_amend(code: str, delta: int, /) -> Result["Request", RequestFault]:
        return _checked_code(code).map2(_checked_delta(delta), lambda valid, step: Request(amend=(valid, step)))


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

## [03]-[MODAL_ARITY]

[ARITY_ABSORPTION]:
- Law: singular, multi-item, and empty call sites collapse into one parameter typed `T | Iterable[T]`; the body normalizes once at the head and the rest of the function sees one shape, so arity is a property of the argument, never of the signature, and the discriminant is recoverable from the value, never restated by a `many: bool` or a `count: int` the value's length already answers.
- Law: the normalization is one structural `match` — a sequence of owners, an empty sequence, a lone owner — read in that order so the broadest container pattern wins before the lone fallback.
- Exemption: Python has no span boundary, and `str`/`bytes` are themselves `Iterable`, so the `Iterable()` normalization arm carries one guard — `if not isinstance(stream, (str, bytes))` — to keep a string from shattering into characters; this `str`/`bytes` exclusion is the named platform-forced seam, not a free-form guard, and a domain whose lone-value type is itself iterable normalizes by a closed-owner match instead, never by a second guard.
- Use: `*items: T` only when the call site genuinely lists positional arguments; a single `T | Iterable[T]` parameter when the caller already holds a collection, because `*items` forces an unpack the caller did not ask for.
- Reject: a `batch` flag, a `mode` flag, or sibling `process`/`process_many` functions; `singular`/`plural`/`stream` name suffixes that re-describe the input the value already carries; a guard beyond the named `str`/`bytes` seam that smuggles a discriminant the value already answers.

[MODALITY_FOLD]:
- Law: singular, plural-preserving, and plural-reducing are one arm under three combinators — `Block.map` for singular and plural-preserving with shape intact, `Block.fold` keyed on the monoid policy value for plural-reducing — the same arm, the reducer alone switching the algebra; a case whose payload is a tuple spreads through `Result.map2`/`Option.starmap` rather than an unpacking lambda, so the join names no positional accessor.
- Boundary: the fail-fast-versus-accumulate sequencing operator and the seed-`bind` reducer it drives are the rail page's `threaded`; this surface composes that reducer to carry the failure semantics and spends its own lines on the arity normalization and the monoid-keyed reduction, never re-deriving the fold.
- Use: the rail's own `bind`/`map` so the carrier owns short-circuit; the combinator is the sequencing policy, never a tuning flag.
- Reject: a manual index counter, a mutable accumulator list appended in a `for` loop, and `zip` across unequal lengths without `zip(strict=True)` proving the arity invariant.

```python conceptual
from collections.abc import Callable, Iterable
from dataclasses import dataclass

from expression import Ok, Result
from expression.collections import Block


@dataclass(frozen=True, slots=True, kw_only=True)
class Monoid[R]:
    empty: R
    combine: Callable[[R, R], R]


def threaded[T, E](acc: Result[Block[T], E], result: Result[T, E], /) -> Result[Block[T], E]:  # the rail page's reducer
    return acc.bind(lambda done: result.map(lambda value: done.append(Block.singleton(value))))


def normalized[T](items: T | Iterable[T], /) -> Block[T]:
    match items:
        case tuple() | list() as many:
            return Block.of_seq(many)
        case Iterable() as stream if not isinstance(stream, (str, bytes)):
            return Block.of_seq(stream)
        case lone:
            return Block.singleton(lone)


def swept[T, R, E](items: T | Iterable[T], step: Callable[[T], Result[R, E]], reduce: Monoid[R], /) -> Result[R, E]:
    railed = normalized(items).map(step)
    return railed.fold(threaded, Ok(Block.empty())).map(lambda kept: kept.fold(reduce.combine, reduce.empty))
```

## [04]-[PARAMETER_ALGEBRA]

[POLICY_VALUES]:
- Law: a policy parameter arrives pre-constructed and carries its own behavior; the entrypoint invokes the value it was handed and no `if`/`match` reconstructs at the call site what the value already encodes.
- Law: behavior-bearing policy is one frozen owner whose fields include the callable step, or a `frozendict` policy table keyed on the discriminant; a `StrEnum` member carries a column only when the column is data, and the callable lives on the owner the enum selects — chooser row `[04]` is exactly this, the vocabulary member owning its behavior so the dispatch is the member's own method, never a full-coverage `match` re-spelled per call site.
- Law: a package whose entry point already accepts a behavior value takes the policy directly — `stamina.retry`'s `on=` is an `ExcOrBackoffHook` discriminator, so a `Callable[[Exception], bool | float | timedelta]` that decides retry and overrides the backoff per error is the policy value, never a `bool` plus a separate `delay` parameter the body recombines.
- Use: a frozen dataclass with a `Callable` field and named module-level instances (`STRICT`, `LENIENT`) as the policy family, the caller passing the instance and the entrypoint calling `policy.step(...)`; the package's own behavior-valued knob where it owns the concern.
- Reject: a `strict: bool` parameter selecting between two bodies; a behavioral near-twin chosen by flag rather than by the value that encodes the boundary behavior.

[OPTIONAL_CONTEXT]:
- Law: `Option[T]` is the single optional-parameter form — declaration is `context: Option[T] = Nothing`, consumption is `context.default_value(policy.canonical)` so the fallback derives once from the policy owner — and a nullable flag tail (`a: T | None = None, b: T | None = None, strict: bool = False`) fragments one context into parallel parameters that collapse to one `Option[ContextRecord]` carrying the override bundle, with `ctx: T | None = None` the boundary-only spelling projected to `Option[T]` at admission.
- Law: when caller-omission must stay distinct from a domain-valid `None`, the settled `sentinel` admits as the parameter's own union member and the entrypoint projects it to `Option` before any interior sees it — the signature carries the omission discriminant, never a parallel `present: bool`.
- Boundary: a capability orthogonal to the discriminant — a cancellation scope, a runtime settings record — describes how work runs, not which case it is, and rides the runtime owner, never the signature.

[KNOB_TEST]:
- Law: the knob test is removal — delete the parameter, and if no information is lost that the value cannot reconstruct, the parameter was a knob and the value already discriminates.
- Reject: a timeout, retry count, or deadline as an entrypoint parameter; the bound is a definition-time aspect or an `anyio` scope around the call, and the signature never grows a token tail for it.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass
from datetime import timedelta

from expression import Error, Nothing, Ok, Option, Result


@dataclass(frozen=True, slots=True, kw_only=True)
class Context:
    ceiling: int


@dataclass(frozen=True, slots=True, kw_only=True)
class Policy:
    canonical: Context
    step: Callable[["Input", Context], Result["Receipt", str]]
    backoff: Callable[[Exception], bool | float | timedelta]


STRICT = Policy(
    canonical=Context(ceiling=1),
    step=lambda value, ctx: Ok(Receipt.EMPTY) if value.score <= ctx.ceiling else Error(f"<over:{value.score}>"),
    backoff=lambda exc: float(exc.args[0]) if isinstance(exc, TimeoutError) else False,
)
LENIENT = Policy(canonical=Context(ceiling=8), step=lambda _v, _c: Ok(Receipt.DEGRADED), backoff=lambda _e: 0.5)


def run(policy: Policy, value: "Input", context: Option[Context] = Nothing, /) -> Result["Receipt", str]:
    return policy.step(value, context.default_value(policy.canonical))
```

## [05]-[DISPATCH_FORMS]

[FORM_SELECTION]:
- Law: the six forms are selected by where ownership lives — the chooser's ownership signatures are the selection law — and when two forms both fit, the one whose owner already holds the exhaustiveness obligation wins.
- Law: a closed vocabulary the program owns dispatches through a `match` over the owner or a `frozendict` table; `functools.singledispatch` is reserved for a genuinely open type set foreign code extends, where editing one `match` per new type is impossible.
- Reject: a `frozendict` table restating a closed family's own cases — a duplicate-entry burden with a silent missed-case failure when a member is added; `singledispatch` over a closed family the program owns, which trades the static exhaustiveness proof for a runtime registration scan; an `if`/`elif` ladder over a `StrEnum`'s `.value` strings.
- Boundary: mixing forms at one site signals an unresolved ownership boundary, except the valid composition — a `match` arm reading a `frozendict` table is dispatch plus data retrieval, and a function owning foreign-to-domain translation while its inner `match` owns per-case projection.

[TABLE_DISPATCH]:
- Law: a key-to-static-data correspondence is one module-level `frozendict` declared once; secondary maps derive from the primary table's values through a comprehension, never re-declared, so the correspondence has one edit site.
- Law: the table lookup returns `None` for an absent key and the call site projects that to `Option` or a typed fault at the boundary; the table never carries a sentinel value standing in for absence, and enum-value membership admits through `EnumType(value)` under `try`/`except ValueError` projected to `Option`/`Result`, never a reach into `EnumType._value2member_map_` or any other private dunder.
- Use: a `frozendict[K, tuple[...]]` row when the key maps to several derived values at once; the `enum` member as the key when the vocabulary is closed and process-local.
- Reject: a `dict` mutated after construction as a policy table; `MappingProxyType` over mutable storage; a table whose values are callables the owning vocabulary carries as case methods; an `if value in EnumType._value2member_map_` private-internal membership probe.

[OPEN_DISPATCH]:
- Law: `@singledispatch` registers one arm per concrete type and a base arm; the registered functions are the open extension axis, so foreign code adds a type by registering an arm without editing the owner — the inverse of the closed `match`.
- Law: the base `@singledispatch` arm returns the rail's typed fault for an unregistered type; dispatch resolves on the first argument's runtime type through the method resolution order, so an `ABC` registration covers its subclasses in one arm, and every registered arm returns the rail the owner declares so the open axis stays rail-consistent with the closed forms.
- Use: `singledispatch` only at a true plugin seam — a renderer over a host type hierarchy the library does not own; the closed `match` everywhere the program owns the type set.

```python conceptual
from collections.abc import Sequence
from enum import StrEnum
from functools import singledispatch

from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some


class Marker(StrEnum):
    PRIMARY = "<key-a>"
    SECONDARY = "<key-b>"


WEIGHT: frozendict[Marker, tuple[int, str]] = frozendict({Marker.PRIMARY: (1, "<label-a>"), Marker.SECONDARY: (2, "<label-b>")})
LABEL: frozendict[Marker, str] = frozendict({marker: label for marker, (_rank, label) in WEIGHT.items()})


def weighted(raw: str, /) -> Result[int, str]:
    try:
        marker = Some(Marker(raw))
    except ValueError:
        marker = Nothing
    return marker.map(lambda m: WEIGHT[m][0]).to_result(f"<unknown:{raw}>")


@singledispatch
def rendered(value: object, /) -> Result[str, str]:
    return Error(f"<no-arm:{type(value).__name__}>")


@rendered.register
def _(value: Marker, /) -> Result[str, str]:
    return Ok(LABEL[value])


rendered.register(Sequence, lambda value: Ok(f"<seq:{len(value)}>"))
```

## [06]-[RAIL_POLYMORPHIC_DISPATCH]

[ONE_RAIL_SURFACE]:
- Law: the form selects which arm runs; the rail the arms return selects how results combine — orthogonal axes. One function whose body composes through `bind`/`map` serves `Result` and `Option` by member-name overlap over the union `Result[T, E] | Option[T]`, not by a shared higher-kinded carrier — `expression` exposes no common monadic supertype, the only shared bases being `Iterable`, `PipeMixin`, and `object` — so the per-rail sibling family is the rejected form while the neutral body stays restricted to the literal `map`/`bind`/`default_value` names both carriers expose and never names `Ok`/`Some`/`Error`/`Nothing`, because the moment it constructs a carrier by name it commits to one and the union stops type-checking.
- Law: the rail is chosen once at the function's boundary and threaded unchanged; `Result.to_option` and `Option.to_result` migrate the rail exactly once at a boundary, never mid-pipeline, because the round trip stamps over the original fault.
- Use: a generic `[T, E]` signature whose body names only `map`/`bind`/`default_value`, deleting the `run_result`/`run_option` sibling pair and admitting any fault vocabulary; the rail page's `@effect.result`/`@effect.option` builders own the sequential `yield from` form, reached only when one carrier is already fixed.

[INDEPENDENT_JOIN]:
- Law: independent computations combine through `map2` (two operands), `starmap` (one tuple-carrying operand), or a railed comprehension over a fixed tuple, and dependent steps chain through `bind`; the choice is load-bearing because a `bind` chain over independent operands reports only the first failure and silently discards the rest.
- Law: `Result.map2(other, f)` runs both operands and applies `f` to the pair, short-circuiting on the first `Error`; an accumulating join that reports every independent fault folds the faults through the error type's own combination before reporting.
- Reject: a branch inside the join whose only legitimate content is total construction over already-railed values; a branch is a fourth dispatch smuggled into combination and lifts into its own arm.

[ITERATIVE_DISPATCH]:
- Law: a dependent dispatch loop — each step's successor decided by the prior step's case — is one `tailrec` function over a closed continue-or-done step, the live case picking the next seed and the done case returning the fixpoint, so the stack never grows and the loop carries no mutable index; the continue-or-done split is itself a two-case `@tagged_union`, never a sentinel return or a `None` terminator the caller re-reads.
- Law: the step is a pure function returning `TailCall(*next_seed)` to recurse or a plain terminal to complete; a fallible step threads the rail by returning the carrier as the terminal, so the trampoline drives dependent dispatch to a fixpoint and the boundary lifts the terminal once — `match` over the step's own two cases is the dispatch, `tailrec` is the driver, and the two stay orthogonal exactly as form and rail do.
- Use: `tailrec` for bounded fixpoint iteration the call modality folds over — a settle loop, a refinement sweep, a state-machine advance whose verb the prior state selects; the `@effect.result` do-notation when the chain is dependent but not iterative.
- Reject: an open `def f(): ... return f(...)` recursion that grows the stack on deep input; a `while` loop mutating an accumulator; a `count` parameter bounding a loop whose terminal case already answers when to stop.

```python conceptual
from typing import Literal, assert_never

from expression import Error, Ok, Result, TailCall, TailCallResult, case, tag, tagged_union, tailrec


type StepFault = Literal["<diverged>"]


@tagged_union(frozen=True)
class Step:
    tag: Literal["advance", "settle", "diverge"] = tag()
    advance: tuple[int, int] = case()
    settle: int = case()
    diverge: None = case()


def stepped(state: int, epoch: int, ceiling: int, /) -> Step:
    if epoch <= 0:
        return Step(diverge=None)
    if state <= 1:
        return Step(settle=state)
    folded = state // 2 if state % 2 == 0 else (3 * state + 1) // 2
    return Step(diverge=None) if folded >= ceiling else Step(advance=(folded, epoch - 1))


@tailrec
def driven(state: int, epoch: int, ceiling: int, /) -> TailCallResult[Result[int, StepFault], ...]:
    match stepped(state, epoch, ceiling):
        case Step(tag="advance", advance=(nxt, left)):
            return TailCall(nxt, left, ceiling)
        case Step(tag="settle", settle=fixed):
            return Ok(fixed)
        case Step(tag="diverge"):
            return Error("<diverged>")
        case unreachable:
            assert_never(unreachable)


def converged(seed: int, ceiling: int, /) -> Result[int, StepFault]:
    return driven(seed, ceiling, ceiling)
```

## [07]-[TYPE_LEVEL_DISPATCH]

[PROTOCOL_PORT]:
- Law: a `Protocol` is the open boundary — the inversion of the closed `match`: the `match` is closed over one owner's cases, the protocol is open over the unbounded family of implementers, and the generic constraint `[S: Port]` is itself the dispatch, resolved by static structural subtyping with no registration, no nominal base, no `isinstance` at the call site.
- Law: the port declares the smallest replaceable operation family, every method returns the rail the domain declares, and the constrained function returns `S` so an immutable implementer threads its own successor — one dispatch surface over every present and future implementer, the failure surface a closed fault family across all of them.
- Use: one generic function constrained `[S: Port]` composing the port's operations through the rail; the port keyed by `type[Port]` when injected through a runtime settings owner.
- Reject: a one-method `Protocol` where `Callable` already states the contract; a protocol per variant simulating a closed family; a `@runtime_checkable` protocol used as an `isinstance` gate where a `TypeIs` predicate proves exact membership.

[REACH_LIMIT]:
- Law: structural typing is static, so the `[S: Port]` form serves implementers fixed at the call site; a runtime-discovered candidate needs `@runtime_checkable` plus an `isinstance` gate, which checks method presence only, never signatures — so the runtime gate pairs with a `TypeIs` predicate that narrows to the exact port when semantic membership exceeds member presence, and the narrowed value re-enters the same static `[S: Port]` surface.
- Law: port coverage is proved once at admission — `typing.get_protocol_members` reads the required member set and `beartype.door.is_subhint` decides a candidate hint against the port hint as a variance-aware subtype query — never a per-request structural validation that re-pays the reflective scan the static form deletes on every call.
- Boundary: the runtime-checkable protocol exists only at the discovery seam; everywhere the implementer is statically known, the bare structural `Protocol` resolves with zero runtime cost.

```python conceptual
from copy import replace
from typing import Protocol, TypeIs, runtime_checkable

from expression import Error, Ok, Option, Result, Some


@runtime_checkable
class ShapeStore(Protocol):
    def loaded(self, key: str, /) -> Result["Shape", str]: ...
    def stored(self, shape: "Shape", /) -> Result["ShapeStore", str]: ...


def is_store(value: object, /) -> TypeIs[ShapeStore]:
    return isinstance(value, ShapeStore) and not isinstance(value, type)


def refreshed[S: ShapeStore](store: S, key: str, value: str, /) -> Result[S, str]:
    return store.loaded(key).map(lambda shape: replace(shape, value=value)).bind(store.stored)


def selected(candidate: object, key: str, value: str, /) -> Option[Result[object, str]]:
    return Some(refreshed(candidate, key, value)) if is_store(candidate) else Nothing
```

## [08]-[ASPECTS]

[WEAVE_TAXONOMY]:
- Law: a definition-time aspect is a property of the function — declared by a signature-preserving decorator, present at every call site; a call-site aspect is a property of one invocation — attached as a scope or combinator around the call it modifies, and the classification test is per-site variance: a concern present at every use weaves at definition (retry policy, runtime contract, observability span, memoization), a concern that varies per site composes at the site (one deadline, one cancellation scope, one resource bracket).
- Law: a definition-time aspect preserves the signature and the rail through inline `**P` and `functools.wraps`, materializes policy from its arguments, and never raises inside domain flow — a failing aspect returns the rail's `Error`, it does not throw past the wrapped function, so the contract weave is `BeartypeConf(violation_type=...)` redirecting a type violation onto the fault rather than a bare `@beartype` that throws `BeartypeCallHintViolation` into the interior.

[DECORATOR_STACKING]:
- Law: decorators apply bottom-up at definition and execute top-down at call, so the stack is an ordered sequence and the same aspects in two orders are two policies — a retry weave outside the contract re-validates per attempt, the contract folded innermost validates once and the outer weaves retry or trace only the validated body; the order is therefore the `*composed` tuple's order, weaves the factory folds right-to-left onto its own contract arm, not a fixed tower re-spelled at every owner, so a further co-occurring concern is one more entry in the same call with no body edited.
- Law: every weave in the fold satisfies one signature — `Callable[[Callable[P, Result[T, E]]], Callable[P, Result[T, E]]]` — which is what keeps `**P`, the return rail, and `functools.wraps` identity intact through an arbitrary-depth stack; a package decorator whose own signature unifies to this shape (`stamina.retry` over a `Result`-returning core) enters directly with no wrapper, while a weave typed `Callable[..., Any]` erases the parameter list and severs the chain, and a weave returning a bare `T` instead of the rail breaks the next weave's `Result` contract.
- Law: the contract weave is this surface's own aspect and the factory's fixed innermost arm — `beartype(conf=BeartypeConf(violation_type=...))` redirects a `BeartypeCallHintViolation` onto the fault and the `except` arm maps it to the rail's `Error`, so a violation never throws past the wrapped function; every other concern — the rail page's `stamina.retry(on=...)` weave, the domain observability page's emission weave — enters as a `*composed` entry already built at its owner, threaded through this fold and never re-derived here.
- Use: the variadic `*composed` weave tuple folded onto the contract arm, the rail-safe `beartype(conf=BeartypeConf(violation_type=...))` contract weave as this page's spotlight; this factory when two-to-four owner-built weaves recur together over one pure core.
- Reject: a bare `try`/`except` retry loop; a hand-rolled span timer or a `structlog.get_logger` chain re-built here where the emission weave already owns it; a bare `@beartype` that raises into the rail; a weave typed `Callable[..., Any]` that erases the wrapped signature.

```python conceptual
from collections.abc import Callable
from functools import reduce, wraps
from typing import Literal

import stamina
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result

type FetchFault = Literal["<empty>", "<contract>"]
type Weave[**P, T, E] = Callable[[Callable[P, Result[T, E]]], Callable[P, Result[T, E]]]


def contracted[**P, T, E](lifted: Callable[[BeartypeCallHintViolation], E], /) -> Weave[P, T, E]:
    guard = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))

    def weave(operation: Callable[P, Result[T, E]], /) -> Callable[P, Result[T, E]]:
        guarded = guard(operation)

        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> Result[T, E]:
            try:
                return guarded(*args, **kwargs)
            except BeartypeCallHintViolation as violation:
                return Error(lifted(violation))

        return call

    return weave


def aspected[**P, T, E](
    *composed: Weave[P, T, E], lifted: Callable[[BeartypeCallHintViolation], E]
) -> Weave[P, T, E]:
    stack: tuple[Weave[P, T, E], ...] = (*composed, contracted(lifted))
    return lambda operation: reduce(lambda wrapped, weave: weave(wrapped), reversed(stack), operation)


@aspected(stamina.retry(on=ConnectionError, attempts=5), lifted=lambda _v: "<contract>")
def fetched(code: str, /) -> Result[str, FetchFault]:
    return Error("<empty>") if code == "" else Ok(code.upper())
```

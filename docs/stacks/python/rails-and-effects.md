# [PYTHON_RAILS_AND_EFFECTS]

`expression` owns the result and absence carriers, the do-notation builders, and immutable traversal; `anyio` owns the structured-concurrency runtime — task groups, cancellation scopes, deadlines, blocking-call offload, and resource brackets; `stamina` owns retry policy and its instrumentation. A carrier is chosen once at admission and threaded unchanged: the narrowest carrier that states the real outcome carries the value, reusable transforms keep it, and collapse to a bare value or a raised exception happens only at the process, CLI, network, or persistence edge. The interior is total over admitted carriers — raw provider shapes, `None`-as-failure, unclassified exceptions, and cancellation never travel it; each converts once at the owning boundary into the carrier or re-raises as itself.

## [01]-[RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider rail is earned only by a capability the narrower one cannot carry: typed failure cause, accumulated faults, a structured effect, or async cancellation. The carrier choice is the sequencing algebra — `Option` and `Result` short-circuit, the accumulating fold combines — never a flag the body re-reads.

| [INDEX] | [SURFACE]                  | [OWNS]                         | [REJECT]                          |
| :-----: | :------------------------- | :----------------------------- | :-------------------------------- |
|  [01]   | `Option[T]`                | non-failing absence            | `None`-as-hidden-failure          |
|  [02]   | `Result[T, E]`             | typed fallibility, fail-fast   | raised control flow in domain     |
|  [03]   | `@effect.result[T, E]()`   | sequential `bind` do-notation  | nested `bind` lambda ladders      |
|  [04]   | fault-combining fold       | accumulated independent faults | first-failure `bind` over a batch |
|  [05]   | `Block[T]` / `Map[K, V]`   | immutable traversal and lookup | mutable list/dict domain flow     |
|  [06]   | `anyio` task group / scope | structured async effect        | `asyncio.gather` task sets        |
|  [07]   | `stamina.retry`            | retry and backoff policy       | ad-hoc sleep-and-loop retry       |
|  [08]   | threaded frozen owner      | single-producer state          | shared mutable global             |

`Option[T]` carries absence with zero failure semantics; promote to `Result[T, E]` when the caller must know why, where the error `E` is a closed fault vocabulary, never a bare `str` for a domain with more than one mode. `Try[T]` is not a sixth carrier — it is `Result[T, Exception]` pinned to the exception side through `effect.try_`, minted at a foreign boundary and mapped into the domain fault vocabulary in the same expression, so it lives in the boundary card, never the interior. The accumulating fold is not a carrier but a sequencing discipline over `Result`; it earns its own row because abort-versus-accumulate is a correctness decision the carrier alone does not make. The serialized many-producer state cell is the agent boundary owner's surface, not a rail chosen here — this page chooses between a frozen owner threaded for one producer and that cell, and stops at the carrier decision.

[REPRESENTATION_DEFAULT]:
- Law: `Nothing` is the zero-value absence — a singleton, total over the missing case, never `None` past the seam; a fallible field carries `Result`, never `T | None`, and `None` survives only as a genuine domain or wire value where absence-with-cause stays a closed family.
- Law: `Result` equality compares tag then payload, so a `set` or `dict` keyed on `Result` distinguishes `Ok(x)` from `Error(e)` and distinct faults from each other — a single-fault collapse coalesces every failure to one key, which an accumulating boundary must never do.
- Boundary: a frozen owner's optional field defaults to `Nothing` or a `sentinel`, never a bare `None` that conflates omission with a valid null.

## [02]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome; reusable transforms keep that carrier and never re-project mid-pipeline. The interior never sees the provider exception, the provider sentinel, or the carrier-to-carrier round trip that erases a fault.

[EXCEPTION_CAPTURE]:
- Use: one boundary adapter wrapping the throwing provider call in `try`/`except`, naming the exact exception types the provider raises — `pydantic.ValidationError`, `msgspec.ValidationError`, `OSError` — never a bare `except Exception`, so an unexpected exception propagates as a defect rather than being silently railed; `effect.try_` is the single-trap form when one provider exception maps to one fault, the explicit multi-`except` adapter the form when distinct provider exceptions discriminate into distinct domain faults.
- Law: `except*` groups a multi-failure provider boundary and `BaseException.add_note()` preserves causal context when the fault is re-spelled, so the failure set survives the conversion into the domain vocabulary.
- Law: a `@beartype` runtime-contract violation is one more provider exception this adapter captures — catch `beartype.roar.BeartypeCallHintViolation` at the egress and map it into the fault vocabulary like any other foreign raise; the alternative that redirects the violation onto the rail without a `try` is the surface page's contract weave, composed there at definition time, not a second capture form taught here.
- Reject: `contextlib.suppress` hiding fallibility; a bare `try`/`except` wrapping interior rail transforms; discarding the caught message when mapping to the fault.

[CROSS_RAIL_PROJECTION]:
- Use: the instance matrix — `Option.to_result(error)`, `Result.to_option()`, `Option.of_result(...)`, `Option.of_optional(value)` — to migrate a carrier exactly once at a boundary; widening supplies the missing structure, narrowing discards it.
- Law: the `Result -> Option -> Result` round trip erases the original fault and breaks every later `map_error` or fault-code predicate; `option.to_result(specific_fault)` re-supplies a precise fault instead of a placeholder.
- Reject: round trips where an explicit projection carries the fault through; `Option.value` access without a prior `is_some` proof; collapse inside a pure projection that stays railed.

```python conceptual
from typing import Literal

import msgspec
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result

type AdmitFault = Literal["<invalid-payload>", "<decode-failed>", "<contract>"]

_DECODER = msgspec.json.Decoder(type="Shape")


@beartype
def _refined(value: "Shape", /) -> "Shape":
    return value


def admitted(raw: bytes, /) -> Result["Shape", AdmitFault]:
    try:
        return Ok(_refined(_DECODER.decode(raw)))
    except msgspec.ValidationError as fault:
        fault.add_note(f"<at:admitted len={len(raw)}>")
        return Error("<invalid-payload>")
    except msgspec.DecodeError:
        return Error("<decode-failed>")
    except BeartypeCallHintViolation:
        return Error("<contract>")
```

## [03]-[TRAVERSAL_FLOW]

Traversal is rail policy: the collection owner and the sequencing operator together decide how failures, strictness, and order compose. The operator is the sequencing decision, never a performance choice — fail-fast threads the carrier and aborts on the first `Error`, accumulating folds every element and combines the faults.

[COLLECTION_OWNER]:
- Law: `Block[T]` is the immutable sequence owner — persistent, structurally shared, `cons`/`append`/`fold`/`choose`/`partition` — for domain traversal; `Map[K, V]` is the persistent keyed owner with `add`/`change`/`fold`; a plain `tuple` carries fixed-arity evidence, not a growing sequence.
- Law: `Block.fold(folder, seed)` folds left with an immutable accumulator; incremental build is one `fold` or `cons`-then-reverse, never repeated mutation of a list then a freeze.
- Use: `Block.choose(f)` for atomic single-pass filter-map into `Option`, replacing a `filter` then `map`; `Block.partition(p)` when both the kept and rejected halves are needed.
- Reject: a mutable `list` appended in a `for` loop as domain flow; `zip` across unequal lengths without `zip(strict=True)`; re-enumerating a lazy `Seq` whose cost is paid per pass.

[SEQUENCING_OPERATOR]:
- Law: fail-fast traversal is one named reducer — `threaded` binds the running `Result[Block[T], E]` accumulator and appends each `Ok` value, short-circuiting the whole `Block.fold` on the first `Error` — because `expression` ships no `Block.traverse`/`sequence`, so the seed-`bind` fold over `Ok(Block.empty())` is the substrate's traversal primitive and the page declares it once rather than re-inlining the lambda per site; the accumulating form keeps both halves with one `Block.choose` over `to_option` and `swap().to_option()`, then combines the fault half through the vocabulary's own combination law.
- Law: the `@effect.result[T, E]()` builder is the do-notation form of the fail-fast fold — a generator where each `yield from` is a `bind` that short-circuits the whole block on the first `Error` and the `return` is the final `Ok`; it linearizes a dependent `for ... yield from` chain that a nested-`bind` lambda ladder obscures, and is the readable form once the chain exceeds three steps.
- Boundary: the dispatch over a modality (`T | Iterable[T]`) and the seed-`bind` reducer that drives it are owned by the surfaces page; this page composes that reducer to state the failure semantics it carries, never to re-teach the arity normalization.
- Reject: `map` then a manual rail unwrap; an index-threaded fold unless the fold genuinely carries algorithm state; a `match` on the carrier tag where `bind`/`choose` already routes both arms.

```python conceptual
from typing import Literal

from expression import Error, Ok, Result, effect
from expression.collections import Block

type TraverseFault = Literal["<empty>", "<over>"]


def threaded[T, E](acc: Result[Block[T], E], result: Result[T, E], /) -> Result[Block[T], E]:
    return acc.bind(lambda done: result.map(lambda value: done.append(Block.singleton(value))))


def admitted_one(raw: str, ceiling: int, /) -> Result[int, TraverseFault]:
    return Error("<empty>") if raw == "" else Error("<over>") if len(raw) > ceiling else Ok(len(raw))


def traversed_fail_fast(raws: Block[str], ceiling: int, /) -> Result[Block[int], TraverseFault]:
    seed: Result[Block[int], TraverseFault] = Ok(Block.empty())
    return raws.map(lambda raw: admitted_one(raw, ceiling)).fold(threaded, seed)


def traversed_accumulate(raws: Block[str], ceiling: int, /) -> tuple[Block[int], Block[TraverseFault]]:
    railed = raws.map(lambda raw: admitted_one(raw, ceiling))
    return railed.choose(lambda r: r.to_option()), railed.choose(lambda r: r.swap().to_option())


@effect.result[int, TraverseFault]()
def assembled(head: str, tail: str, ceiling: int):
    first = yield from admitted_one(head, ceiling)
    second = yield from admitted_one(tail, ceiling)
    return first + second
```

## [04]-[FAILURE_HANDLING]

Apply rail-qualified failure transforms before collapse; a rail transform never raises. Recovery keys on the fault's own code or case, never on a reconstructed message, so a fault re-spelled with `map_error` still routes to the right arm.

| [INDEX] | [COMBINATOR]         | [CARRIER]          | [USE]                          |
| :-----: | :------------------- | :----------------- | :----------------------------- |
|  [01]   | `.map_error(f)`      | `Result`, `Try`    | re-spell the fault, keep cause |
|  [02]   | `.or_else_with(f)`   | `Result`, `Option` | recover from the fault         |
|  [03]   | `.swap()`            | `Result`           | route the fault half           |
|  [04]   | `.merge()`           | `Result`           | collapse same-typed arms       |
|  [05]   | `.filter_with(p, f)` | `Result`           | guard with a fault on fail     |
|  [06]   | `.default_with(f)`   | `Result`, `Option` | terminal lazy fallback         |

[FAULT_VOCABULARY]:
- Law: the failure type is a closed vocabulary the program owns — a `Literal` set for a handful of causes, a `StrEnum` when the causes are iterated or wire-carried, a `@tagged_union` fault family when a cause carries a structured payload — and the family separates the two dispositions: conjunctive faults that accumulate (every validation failure of one admission combines) and disjunctive faults that abort (the first irrecoverable cause wins). The carrier realizes the disposition — the accumulating fold for the conjunctive set, `bind` short-circuit for the disjunctive set.
- Law: an accumulating boundary needs a fault type with a combination law — the aggregate case holds the typed members so all-fault reporting keeps each member structurally addressable, never flattened to one concatenated string; the aggregate combines members, it does not stringify them.
- Law: construction is two-tier — a private constructor builds the well-formed fault and a public `of_*` classmethod returns `Result[Fault, ...]` (or the fault directly when construction cannot fail), so an interior never hand-builds a malformed fault, and `map_error` re-spells without losing the provider message, validation detail, or category.
- Reject: `Exception` subclasses as the interior fault type; a bare `str` fault for a multi-cause domain; `None` standing in for failure; recovery by `==` over a message; an aggregate that joins members into one string and erases their codes.

```python conceptual
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block


@tagged_union(frozen=True)
class Fault:
    tag: Literal["bounds", "absent", "aggregate"] = tag()
    bounds: tuple[int, int] = case()
    absent: str = case()
    aggregate: tuple["Fault", ...] = case()

    @staticmethod
    def combined(left: "Fault", right: "Fault", /) -> "Fault":
        match left, right:
            case Fault(tag="aggregate"), Fault(tag="aggregate"):
                return Fault(aggregate=(*left.aggregate, *right.aggregate))
            case Fault(tag="aggregate"), _:
                return Fault(aggregate=(*left.aggregate, right))
            case _, _:
                return Fault(aggregate=(left, right))


def guarded(score: int, low: int, high: int, /) -> Result[int, Fault]:
    return Ok(score) if low <= score <= high else Error(Fault(bounds=(low, high)))


def accumulated(scores: Block[tuple[int, int, int]], /) -> Result[Block[int], Fault]:
    railed = scores.map(lambda triple: guarded(*triple))
    faults = railed.choose(lambda r: r.swap().to_option())
    return Ok(railed.choose(lambda r: r.to_option())) if faults.is_empty() else Error(faults.reduce(Fault.combined))
```

## [05]-[EFFECT_RUNTIME]

An async effect carries structured concurrency: `anyio` defers boundary work, owns the cancellation scope and the deadline, and runs the effect at one composition edge. The task group is the failure boundary, the cancel scope owns the bound, and the resource bracket disposes on every exit including cancellation.

[STRUCTURED_CONCURRENCY]:
- Law: concurrent work runs inside one `anyio.create_task_group()` whose `__aexit__` awaits every child and cancels siblings on the first failure, so an orphaned task is structurally impossible; `group.start_soon(coro)` returns a `TaskHandle` and every handle's `return_value` reads its child's carrier once the group exits, never a shared mutable list nor a `create_memory_object_stream` collection rig that re-implements result-gathering the handle already owns. The stream pair stays the producer-consumer log the boundary page owns, not the gather carrier.
- Law: a deadline is `anyio.fail_after(seconds)` (raises `TimeoutError` on expiry) or `anyio.move_on_after(seconds)` (returns, `scope.cancelled_caught` true), so the scope owns the bound, never a `timeout` parameter threaded through the signature; the task group's exception group converts to the domain fault at the group's edge through `except*`, which binds the fault into a carrier because `return`/`break`/`continue` are illegal inside an `except*` block.
- Law: a cancelled child's `TaskHandle.return_value` raises `TaskCancelled`, so the deadline and the `except*` fault short-circuit the terminal before any handle is drained — `cancelled_caught` and the bound fault are read first, and the `threaded` reducer folds the handles only on the clean path.
- Use: `anyio.to_thread.run_sync(fn, limiter=...)` with an explicit `CapacityLimiter` to offload a blocking provider call, bounding the subsystem's concurrency at the boundary.

[CANCELLATION_RAIL]:
- Law: cancellation is not a failure — it is the `anyio.get_cancelled_exc_class()` exception, re-raised after cleanup, never swallowed into a `Result.Error`; a `finally` under a shielded scope owns cleanup and the cancelled exception propagates.
- Law: a retry predicate refuses cancellation first — a cancelled scope never retries — so the retry aspect's `on=` names the provider exceptions, never the cancellation class.
- Reject: `asyncio.gather` task sets without a structured boundary; catching the cancellation exception and returning a value; cleanup-hostile process termination.

```python conceptual
from collections.abc import Awaitable, Callable
from typing import Literal

import anyio
from anyio import BrokenWorkerProcess, TaskHandle
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block


type RunFault = Literal["<deadline>", "<worker>", "<child>"]


async def gathered[T](work: Block[Callable[[], Awaitable[Result[T, RunFault]]]], /, *, seconds: float, slots: int) -> Result[Block[T], RunFault]:
    limiter = anyio.CapacityLimiter(slots)
    handles: list[TaskHandle[Result[T, RunFault]]] = []
    fault: Option[RunFault] = Nothing

    async def run(operation: Callable[[], Awaitable[Result[T, RunFault]]], /) -> Result[T, RunFault]:
        async with limiter:
            return await operation()

    with anyio.move_on_after(seconds) as scope:
        try:
            async with anyio.create_task_group() as group:
                handles.extend(group.start_soon(run, operation) for operation in work)
        except* BrokenWorkerProcess:
            fault = Some("<worker>")
        except* Exception:
            fault = Some("<child>")

    if scope.cancelled_caught:
        return Error("<deadline>")
    seed: Result[Block[T], RunFault] = Ok(Block.empty())
    return fault.map(Error).default_with(lambda: Block.of_seq(handle.return_value for handle in handles).fold(threaded, seed))
```

[RESOURCE_BRACKET]:
- Law: the owner that acquires a resource disposes it on every exit — success, domain fault, raised exception, and cancellation — through `contextlib.AsyncExitStack` (`stack.enter_async_context`) or one `async with` per handle; acquisition order is teardown order reversed, and the stack releases every earlier handle when a later acquisition fails.
- Law: cleanup runs under `with anyio.CancelScope(shield=True):` because an outer deadline or sibling failure cancels the scope mid-teardown otherwise — an unshielded `finally` that awaits is aborted before the handle closes, leaking it; the shielded scope is the named platform-forced statement seam.
- Law: a release that itself raises folds into the failure set with `BaseException.add_note()` or an `except*` group rather than masking the in-flight fault.
- Use: `AsyncExitStack` for a dynamic, fault-ordered set of handles, pooled connections, or leases; one `async with` when the lifetime is statically scoped to one block.
- Reject: a bare `try`/`finally` whose `finally` awaits without a shield; a handle acquired before the stack that no exit path releases; cleanup that swallows the cancellation exception instead of re-raising after release.

```python conceptual
from collections.abc import AsyncIterator, Callable
from contextlib import AsyncExitStack, asynccontextmanager
from typing import Literal

import anyio
from expression import Error, Result

type BracketFault = Literal["<acquire-deadline>"]


@asynccontextmanager
async def leased(name: str, /) -> AsyncIterator[str]:
    handle = await _acquire(name)
    try:
        yield handle
    finally:
        with anyio.CancelScope(shield=True):
            await _release(handle)


async def bracketed[T, E](names: tuple[str, ...], body: Callable[[tuple[str, ...]], Result[T, E]], /, *, seconds: float) -> Result[T, E | BracketFault]:
    async with AsyncExitStack() as stack:
        with anyio.move_on_after(seconds) as scope:
            handles = tuple([await stack.enter_async_context(leased(name)) for name in names])
        return Error("<acquire-deadline>") if scope.cancelled_caught else body(handles)
```

[SCHEDULE_POLICY]:
- Law: retry triggers only on a raised transient at the effect boundary and wraps each attempt independently — a domain fault already railed as `Result.Error` is not an exception and is never retried, so the boundary maps the retried call's terminal outcome onto the carrier after the schedule returns, and re-raising an `Error` to force a retry is the rejected inversion. The `stamina.retry(on=...)` schedule is the surface page's policy value and definition-time weave; this page composes that settled weave and owns the raised-versus-railed boundary law that bounds what the weave may see — the `[CANCELLATION_RAIL]` exclusion already keeps a cancelled scope out of the transient set, so a backoff sleep (itself an `anyio` checkpoint) never re-arms work an outer deadline cancelled.
- Use: one `RetryingCaller`/`AsyncRetryingCaller` built once with `.on(exc)` pre-binding the transient set when the same schedule recurs across call sites without a decorated definition; `set_testing(True)` to collapse backoff and cap attempts in a spec without the production code branching on a test flag.
- Reject: retrying a `Result.Error` by re-raising it; an unbounded retry trusted to stop itself; a second retry implementation beside the composed weave.

[RETRY_INSTRUMENTATION]:
- Law: the on-retry signal is registered once, process-globally, through `stamina.instrumentation.set_on_retry_hooks((StructlogOnRetryHook, PrometheusOnRetryHook))` — not duplicated as a `log.warning` inside the retried body; the shipped emitters are `RetryHookFactory` instances built lazily on the first scheduled retry.
- Law: a `RetryHook` returning an `AbstractContextManager[None]` spans the scheduled wait — entered when the retry is scheduled, exited right before the retry runs — so an OpenTelemetry span wraps each backoff interval as a child span; the `RetryDetails` fields (`caused_by`, `retry_num`, `wait_for`, `waited_so_far`) map field-for-field onto a receipt slot, never re-derived.
- Reject: a per-call retry log inside the body; a hand-rolled backoff timer for the span; `set_on_retry_hooks(())` left in production, which deactivates instrumentation entirely.

```python conceptual
from contextlib import contextmanager
from typing import Iterator

from opentelemetry import trace
from stamina.instrumentation import RetryDetails, RetryHookFactory, StructlogOnRetryHook, set_on_retry_hooks

_TRACER = trace.get_tracer(__name__)


@contextmanager
def spanned(details: RetryDetails, /) -> Iterator[None]:
    with _TRACER.start_as_current_span("retry.wait") as span:
        span.set_attributes(
            {
                "retry.num": details.retry_num,
                "retry.wait_for": details.wait_for,
                "retry.waited": details.waited_so_far,
                "retry.cause": type(details.caused_by).__name__,
            }
        )
        yield


set_on_retry_hooks((StructlogOnRetryHook, RetryHookFactory(lambda: spanned)))
```

## [06]-[STATE_RECEIPTS]

State belongs at a boundary or session owner, threaded immutably for one producer and promoted to the boundary owner's serialized cell for many. A typed receipt carries evidence; a fact stream carries chronology; neither collapses into a generic ledger.

[STATE_CARRIER]:
- Law: the carrier choice is producer count and nothing else — a single producer threads the frozen owner immutably through the pipeline, each step returning the next owner with no cell and no contention, while a many-producer cell is the boundary owner's serialized agent; this page selects the carrier and stops, the agent's drain, token, apply, and reply mechanics being the cell owner's law, not restated here.
- Reject: a cell where one producer's immutable thread carries the state without contention; `expression.MailboxProcessor` as the cell — the `anyio` substrate routes that rejection to the cell owner.

[RECEIPTS]:
- Law: the split is capability — a fact stream answers what happened and when, a typed receipt answers how this computation resolved; one `@tagged_union` fact owner makes resolve, supersede, retire, and fault its cases, the slot a derived projection and the payload each case's own evidence, so a total `match` with `assert_never` flags an unhandled kind and parallel record types synced by hand never appear.
- Law: keep a typed receipt — a frozen owner whose fields carry solver, sampling, route, status, metric, or proof evidence — when the fields are evidence; projection by kind, group-by-slot-last-wins, and full chronology are pure folds over the one fact stream, and the retry receipt's `of_retry` absorbs the `RetryDetails` slots the instrumentation emitted, never re-deriving them.
- Reject: parallel `added`/`updated`/`removed` lists kept in sync; a `Literal` tag beside a `Receipt | str` payload where the case family carries the evidence; a generic `dict[str, object]` receipt erasing the typed fields; payload bytes carried on a receipt where a hash suffices.

```python conceptual
from dataclasses import dataclass
from typing import Literal, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from stamina.instrumentation import RetryDetails


@dataclass(frozen=True, slots=True, kw_only=True)
class Receipt:
    cause: str
    retries: int
    waited: float

    @staticmethod
    def of_retry(details: RetryDetails, /) -> "Receipt":
        return Receipt(cause=type(details.caused_by).__name__, retries=details.retry_num, waited=details.waited_so_far)

    @staticmethod
    def combined(left: "Receipt", right: "Receipt", /) -> "Receipt":
        return Receipt(cause=right.cause, retries=left.retries + right.retries, waited=left.waited + right.waited)


@tagged_union(frozen=True)
class Fact:
    tag: Literal["resolved", "superseded", "retired", "faulted"] = tag()
    resolved: tuple[str, Receipt] = case()
    superseded: tuple[str, Receipt] = case()
    retired: str = case()
    faulted: tuple[str, str] = case()

    @property
    def slot(self) -> str:
        match self:
            case Fact(tag="resolved"):
                return self.resolved[0]
            case Fact(tag="superseded"):
                return self.superseded[0]
            case Fact(tag="faulted"):
                return self.faulted[0]
            case Fact(tag="retired"):
                return self.retired
            case unreachable:
                assert_never(unreachable)


def witnessed(fact: Fact, /) -> Option[Receipt]:
    match fact:
        case Fact(tag="resolved"):
            return Some(fact.resolved[1])
        case Fact(tag="superseded"):
            return Some(fact.superseded[1])
        case Fact(tag="retired") | Fact(tag="faulted"):
            return Nothing
        case unreachable:
            assert_never(unreachable)


def collapsed(facts: Block[Fact], /) -> Block[Receipt]:
    return facts.choose(witnessed)


def grouped_last(facts: Block[Fact], /) -> Map[str, Fact]:
    return facts.fold(lambda acc, fact: acc.add(fact.slot, fact), Map.empty())
```

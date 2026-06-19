# [PYTHON_RAILS_AND_EFFECTS]

`expression` owns result and absence rails, do-notation builders, and immutable traversal; `anyio` owns structured concurrency, cancellation scopes, and effect execution; `stamina` owns retry and backoff policy. A rail is chosen once at admission and never re-chosen mid-pipeline: the narrowest carrier that states the real outcome carries the value, reusable transforms keep it, and collapse to a bare value or a raised exception happens only at the process, CLI, network, or persistence edge. Admitted domain values enter these surfaces; raw provider shapes, `None`-as-failure, and unclassified exceptions never travel the interior — they convert once at the owning boundary.

## [1]-[RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider rail is earned only by a capability the narrower one cannot carry: typed failure cause, accumulated faults, structured effects, or async cancellation.

| [INDEX] | [SURFACE]                  | [OWNS]                         | [REJECT]                      |
| :-----: | :------------------------- | :----------------------------- | :---------------------------- |
|   [1]   | `Option[T]`                | non-failing absence            | `None`-as-hidden-failure      |
|   [2]   | `Result[T, E]`             | typed fallibility              | raised control flow in domain |
|   [3]   | `effect.result` builder    | sequential `bind` do-notation  | nested `bind` lambda ladders  |
|   [4]   | `Try[T]`                   | captured-exception boundary    | bare `try`/`except` interior  |
|   [5]   | `Block[T]` / `Map[K, V]`   | immutable traversal and lookup | mutable list/dict domain flow |
|   [6]   | `anyio` task group / scope | structured async effect        | `asyncio.gather` task sets    |
|   [7]   | `stamina.retry`            | retry and backoff policy       | ad-hoc sleep-and-loop retry   |
|   [8]   | memory-object-stream agent | serialized boundary state cell | shared mutable global         |

`Option[T]` carries absence with zero failure semantics; promote to `Result[T, E]` when the caller must know why; the error type `E` is a closed fault vocabulary — a `Literal` set, `StrEnum`, or `@tagged_union` fault family — never a bare `str` for a domain that has more than one failure mode. `Try[T]` is `Result[T, Exception]` pinned to the exception side; it is the one carrier produced at a foreign boundary and immediately mapped into the domain fault vocabulary.

[REPRESENTATION_DEFAULT]:
- Law: `Nothing` is the zero-value absence — a singleton, total over the missing case, never `None` leaking past the seam.
- Law: a domain field that can fail carries `Result`, never `T | None`; `None` survives only where it is a genuine domain or wire value, and even then absence-with-cause is a closed family, never `None`.
- Boundary: a frozen owner's optional field defaults to `Nothing` or a `sentinel`, never to a bare `None` that conflates omission with a valid null.

[CARRIER_IDENTITY]:
- Law: `Result` equality compares tag then payload, so a `set` or `dict` keyed on `Result` distinguishes `Ok(x)` from `Error(e)` and distinct errors from each other — unlike a single-error collapse where every failure coalesces.
- Law: `Result.map` and `Result.bind` are total over both arms — `map` transforms the `Ok` and passes `Error` through, `bind` chains the `Ok` and short-circuits the `Error` — so a transform never inspects the tag by hand.
- Use: `result.is_ok()` / `result.is_error()` — both are methods called with parens, never properties — only at a terminal collapse; interior code composes through `map`/`bind`/`map_error` and never branches on the tag.

## [2]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome; reusable transforms keep that carrier and never re-project mid-pipeline.

[EXCEPTION_CAPTURE]:
- Use: one boundary adapter wrapping a throwing provider call in `try`/`except`, mapping the caught exception into the domain fault vocabulary, and returning `Result`; the interior never sees the exception.
- Law: the `except` clause names the exact exception types the provider raises — `pydantic.ValidationError`, `msgspec.ValidationError`, `OSError` — never a bare `except Exception`, so an unexpected exception still propagates as a defect rather than being silently railed.
- Law: `except*` groups a multi-failure provider boundary, and `BaseException.add_note()` preserves causal context when the fault is re-spelled; the failure set survives the conversion.
- Reject: `contextlib.suppress` hiding fallibility; a bare `try`/`except` wrapping interior rail transforms; discarding the caught message when mapping to the fault.

```python conceptual
from typing import Literal

import msgspec
from expression import Error, Ok, Result

type AdmitFault = Literal["<invalid-payload>", "<decode-failed>"]

_DECODER = msgspec.json.Decoder(type="Shape")


def admitted(raw: bytes, /) -> Result["Shape", AdmitFault]:
    try:
        return Ok(_DECODER.decode(raw))
    except msgspec.ValidationError as fault:
        fault.add_note(f"<at:admitted len={len(raw)}>")
        return Error("<invalid-payload>")
    except msgspec.DecodeError:
        return Error("<decode-failed>")
```

[CROSS_RAIL_PROJECTION]:
- Use: the instance matrix — `Option.to_result(error)`, `Result.to_option()`, `Option.of_result(...)`, `Option.of_optional(value)` — to migrate a carrier exactly once at a boundary.
- Law: widening supplies the missing structure — `Option -> Result` needs the error value, `T | None -> Option` needs `of_optional`; narrowing discards it — `Result -> Option` drops the fault.
- Law: the `Result -> Option -> Result` round trip erases the original fault and breaks every later `map_error` or fault-code predicate; `option.to_result(specific_fault)` re-supplies a precise fault instead of a placeholder.
- Reject: round trips where an explicit projection carries the fault through; `Option.value` access without a prior `is_some` proof.

[TERMINAL_COLLAPSE]:
- Use: `Result.default_value`, `default_with`, `map`-then-`is_ok` branch, or the `match` collapse only at the process, CLI, network, or persistence edge.
- Law: reusable domain transforms keep the carrier; `.value` and direct attribute access on a rail are never the interior exit — they assert a proof the type does not carry.
- Reject: mid-pipeline collapse inside a pure projection; a `match` on the rail tag inside an expression that stays railed.

## [3]-[TRAVERSAL_FLOW]

Traversal is rail policy: the collection owner and the sequencing operator together decide how failures, strictness, and order compose.

[COLLECTION_OWNER]:
- Law: `Block[T]` is the immutable sequence owner — persistent, structurally shared, `cons`/`append`/`fold`/`map`/`partition` — used for domain traversal; `Map[K, V]` is the persistent keyed owner with `add`/`change`/`fold`; a plain `tuple` carries fixed-arity evidence, not a growing sequence.
- Law: `Block.fold(folder, seed)` folds left with an immutable accumulator; incremental build is `cons`-then-reverse or one `fold`, never repeated mutation of a list then a freeze.
- Use: `Block.choose(f)` for atomic single-pass filter-map into `Option`, replacing a `filter` then `map`; `Block.partition(p)` when both the kept and rejected halves are needed.
- Reject: a mutable `list` appended in a `for` loop as domain flow; `zip` across unequal lengths without `zip(strict=True)`; re-enumerating a lazy `Seq` whose cost is paid per pass.

[RAIL_TRAVERSAL]:
- Law: fail-fast traversal threads the rail through a `reduce` seeded with `Ok(())` and short-circuits on the first `Error`; accumulating traversal folds every element regardless and combines the faults — the operator is the sequencing policy, not a performance choice.
- Use: the seed-and-`bind` fold when the first failure aborts the batch; a comprehension over already-railed values plus a fault-collecting fold when every element must run and all faults report.
- Law: the `effect.result` builder linearizes a fail-fast traversal as `for ... yield from`, each `yield from` a `bind` that short-circuits; the builder is the readable form of the seed-and-`bind` fold.
- Reject: `map` then a manual rail unwrap; an index-threaded fold unless the fold genuinely carries algorithm state.

```python conceptual
from functools import reduce
from typing import Literal

from expression import Error, Ok, Result
from expression.collections import Block

type TraverseFault = Literal["<empty>"]


def admitted_one(raw: str, /) -> Result[int, TraverseFault]:
    return Error("<empty>") if raw == "" else Ok(len(raw))


def traversed_fail_fast(raws: Block[str], /) -> Result[Block[int], TraverseFault]:
    seed: Result[Block[int], TraverseFault] = Ok(Block.empty())
    return raws.fold(lambda acc, raw: acc.bind(lambda done: admitted_one(raw).map(done.append)), seed)


def traversed_accumulate(raws: Block[str], /) -> tuple[Block[int], Block[TraverseFault]]:
    railed = raws.map(admitted_one)
    oks = railed.choose(lambda r: r.to_option())
    errs = railed.choose(lambda r: r.swap().to_option())
    return oks, errs
```

## [4]-[FAILURE_HANDLING]

Apply rail-qualified failure transforms before collapse; a rail transform never raises.

| [INDEX] | [COMBINATOR]         | [CARRIER]          | [USE]                      |
| :-----: | :------------------- | :----------------- | :------------------------- |
|   [1]   | `.map_error(f)`      | `Result`, `Try`    | map the fault              |
|   [2]   | `.or_else(other)`    | `Result`, `Option` | recover with a fallback    |
|   [3]   | `.or_else_with(f)`   | `Result`, `Option` | recover from the fault     |
|   [4]   | `.swap()`            | `Result`           | exchange `Ok` and `Error`  |
|   [5]   | `.merge()`           | `Result`           | collapse same-typed arms   |
|   [6]   | `.to_result(error)`  | `Option`           | project to `Result`        |
|   [7]   | `.to_option()`       | `Result`           | discard fault detail       |
|   [8]   | `.filter_with(p, f)` | `Result`           | guard with a fault on fail |
|   [9]   | `.default_with(f)`   | `Result`, `Option` | terminal lazy fallback     |

[FAULT_VOCABULARY]:
- Law: the failure type is a closed vocabulary the program owns — a `Literal` set for a handful of causes, a `StrEnum` when the causes are iterated or carried on the wire, a `@tagged_union` fault family when a cause carries a structured payload — and the family separates the two dispositions: expected, conjunctive faults that accumulate (every validation failure of one admission combines) and exceptional, disjunctive faults that abort (the first irrecoverable cause wins). The carrier choice realizes the disposition — `map2`/accumulating fold for the conjunctive set, `bind` short-circuit for the disjunctive set.
- Law: recovery keys on the fault's own code or case, never on equality of a reconstructed message — `match` on the tagged case, a `code in {...}` membership test, or a `TypeIs` predicate selects the recoverable set, so a fault re-spelled with `map_error` still routes to the right recovery arm.
- Law: an accumulating boundary needs a fault type with a combination law — a `Block[Fault]` of distinct faults, or a fault family whose aggregate case holds the typed members — so all-fault reporting keeps each member structurally addressable, never flattened to one concatenated string; the aggregate combines members, it does not stringify them.
- Law: construction is two-tier — a private constructor builds the well-formed fault, and a public `admit`/`of_*` classmethod returns `Result[Fault, ...]` (or the fault directly when construction cannot itself fail), so an interior never hand-builds a malformed fault.
- Law: `map_error` re-spells the fault and preserves cause; losing a provider message, a validation detail, or a fault category during conversion is the defect.
- Reject: `Exception` subclasses as the domain fault type in interior signatures; a bare `str` fault for a multi-cause domain; `None` standing in for a failure; recovery by `==` over a message string; an aggregate that joins members into one string and erases their codes.

```python conceptual
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union


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
```

## [5]-[EFFECT_RUNTIME]

An async effect carries structured concurrency; `anyio` defers boundary work, owns cancellation scope, and runs the effect at one composition edge.

[STRUCTURED_CONCURRENCY]:
- Law: concurrent work runs inside one `anyio.create_task_group()` whose `__aexit__` awaits every child and cancels siblings on the first failure — the task group is the failure boundary, so an orphaned task is structurally impossible.
- Law: a deadline is `anyio.fail_after(seconds)` (raises on expiry) or `anyio.move_on_after(seconds)` (returns silently); the scope owns the bound, never a `timeout` parameter threaded through the signature.
- Use: `anyio.to_thread.run_sync(fn, limiter=...)` with a `CapacityLimiter` for blocking calls; `task_group.start_soon(coro)` for fire-and-structured launch; results collected through a memory object stream, never a shared mutable list.
- Boundary: each child task returns its result through a rail or a stream, and the task group's exception group converts to the domain fault at the group's edge through `except*`.

[CANCELLATION_RAIL]:
- Law: cancellation is not a failure — it is an `anyio.get_cancelled_exc_class()` exception that must re-raise after cleanup, never be swallowed into a `Result.Error`; a `finally` or a shielded scope owns cleanup, and the cancelled exception propagates.
- Law: a retry predicate refuses cancellation first — a cancelled scope never retries — so the retry aspect's `on=` names the provider exceptions, never the cancellation class.
- Reject: `asyncio.gather` task sets without a structured boundary; catching the cancellation exception and returning a value; cleanup-hostile process termination.

```python conceptual
from collections.abc import Awaitable, Callable

import anyio
from anyio.streams.memory import MemoryObjectSendStream
from expression import Error, Ok, Result
from expression.collections import Block

type RunFault = str


async def gathered[T](work: Block[Callable[[], Awaitable[Result[T, RunFault]]]], /, *, seconds: float) -> Result[Block[T], RunFault]:
    send, receive = anyio.create_memory_object_stream[Result[T, RunFault]](max_buffer_size=len(work))

    async def run(operation: Callable[[], Awaitable[Result[T, RunFault]]], sink: MemoryObjectSendStream, /) -> None:
        async with sink:
            await sink.send(await operation())

    with anyio.fail_after(seconds):
        async with anyio.create_task_group() as group, send:
            for operation in work:
                group.start_soon(run, operation, send.clone())

    results = Block.of_seq([item async for item in receive])
    folded: Result[Block[T], RunFault] = Ok(Block.empty())
    return results.fold(lambda acc, r: acc.bind(lambda done: r.map(done.append)), folded)
```

[RESOURCE_BRACKET]:
- Law: the owner that acquires a resource disposes it on every exit — success, domain fault, raised exception, and cancellation — through `contextlib.AsyncExitStack` (`async with` / `stack.enter_async_context`) or one `async with` per handle; acquisition order is teardown order reversed, and the stack guarantees a failed later acquisition still releases every earlier one.
- Law: cleanup runs under `with anyio.CancelScope(shield=True):` because an outer deadline or sibling failure cancels the scope mid-teardown otherwise — an unshielded `finally` that awaits is aborted before the handle closes, leaking it; the shielded scope is the named platform-forced statement seam.
- Law: a release that itself raises folds into the failure set with `BaseException.add_note()` or an `except*` group rather than masking the original cause; the bracket never lets a teardown exception erase the in-flight fault.
- Use: `AsyncExitStack` for a dynamic, fault-ordered set of native handles, pooled connections, or leases; one `async with` when the lifetime is statically scoped to one block.
- Reject: a bare `try`/`finally` whose `finally` awaits without a shield; a handle acquired before the stack that no exit path releases; cleanup that swallows the cancellation exception instead of re-raising after release.

```python conceptual
from collections.abc import AsyncIterator, Callable
from contextlib import AsyncExitStack, asynccontextmanager

import anyio
from expression import Error, Ok, Result

type BracketFault = str


@asynccontextmanager
async def leased(name: str, /) -> AsyncIterator[str]:
    handle = await _acquire(name)
    try:
        yield handle
    finally:
        with anyio.CancelScope(shield=True):
            await _release(handle)


async def bracketed[T](
    names: tuple[str, ...], body: Callable[[tuple[str, ...]], Result[T, BracketFault]], /, *, seconds: float
) -> Result[T, BracketFault]:
    async with AsyncExitStack() as stack:
        with anyio.fail_after(seconds):
            handles = tuple([await stack.enter_async_context(leased(name)) for name in names])
        return body(handles)
    return Error("<unreachable>")
```

[SCHEDULE_POLICY]:
- Use: `stamina.retry(on=Exc, attempts=n, timeout=..., wait_initial=..., wait_max=..., wait_exp_base=...)` as the decorator owning exponential backoff with jitter and a cumulative timeout; the policy is the decorator's arguments, never a hand-coded loop.
- Law: `on=` is the exact exception type or a predicate selecting transient faults; a non-transient fault is not retried, and the cancellation class is never in `on=`.
- Law: retry wraps each attempt independently so a successful attempt's effect survives; a domain fault returned as `Result.Error` is not an exception and is not retried — retry triggers on raised transient faults at the boundary, and the boundary maps the final outcome to the rail.
- Reject: a `while` loop with `anyio.sleep` as backoff; trusting an unbounded retry to stop itself; retrying a `Result.Error` by re-raising it.

```python conceptual
import stamina
from expression import Error, Ok, Result


@stamina.retry(on=ConnectionError, attempts=5, wait_initial=0.05, wait_max=2.0, wait_exp_base=2.0)
def fetched(url: str, /) -> bytes:
    return _provider_get(url)


def admitted(url: str, /) -> Result[bytes, str]:
    try:
        return Ok(fetched(url))
    except ConnectionError as fault:
        return Error(f"<unreachable:{fault}>")
```

## [6]-[STATE_RECEIPTS]

State belongs at a boundary or session owner, not inside pure domain transforms.

[BOUNDARY_CELL]:
- Law: a single producer threads state immutably through the pipeline — each step returns the next frozen owner, no cell, no contention; many producers across tasks require a serialized cell.
- Law: the serialized cell is one agent task draining a single `anyio` memory-object-stream inbox — every producer sends a message, the one agent owns the frozen state and applies each message, and a request carrying a reply stream gets a snapshot back; mutation is serialized by the single reader, never by a lock around shared mutable state. `expression.MailboxProcessor` is the rejected form here: it calls `asyncio.get_event_loop()` directly, which the manifest's `asyncio` ban and the `anyio`-only concurrency owner both forbid.
- Use: immutable threading for one run; the memory-object-stream agent for cross-task accumulation, a session register, or a memoization owner observed outside the producing pipeline.
- Reject: a module-global mutable dict as a cache; a `threading.Lock` around shared mutation where the agent serializes; read-modify-write outside the agent; `MailboxProcessor` on the `anyio` substrate.

```python conceptual
from collections.abc import Callable
from dataclasses import dataclass

import anyio
from anyio.streams.memory import MemoryObjectSendStream
from expression.collections import Map


@dataclass(frozen=True, slots=True, kw_only=True)
class Post:
    key: str
    value: int


@dataclass(frozen=True, slots=True, kw_only=True)
class Snapshot:
    reply: MemoryObjectSendStream[Map[str, int]]


type Message = Post | Snapshot


async def register(inbox: anyio.abc.ObjectReceiveStream[Message], /) -> None:
    state: Map[str, int] = Map.empty()
    async for message in inbox:
        match message:
            case Post(key=key, value=value):
                state = state.add(key, value)
            case Snapshot(reply=reply):
                async with reply:
                    await reply.send(state)
```

[RECEIPTS]:
- Law: the split is capability — a fact stream answers what happened and when, a typed receipt answers how this computation resolved; collapsing a typed receipt into a generic log, or a generic ledger over typed proof, erases the evidence.
- Law: one fact owner carries a kind discriminant, a slot identifier, and a payload; adds, updates, removals, and errors are kind cases over one immutable `Block[Fact]`, never parallel record types synced by hand.
- Law: keep a typed receipt — a frozen owner whose fields carry solver, sampling, route, status, metric, or proof evidence — when the fields are evidence; projection by kind, group-by-slot-last-wins, and full chronology are pure folds over the one fact stream.
- Reject: parallel `added`/`updated`/`removed` lists kept in sync; a generic `dict[str, object]` receipt erasing the typed fields; payload bytes carried on a receipt where a hash suffices.

```python conceptual
from dataclasses import dataclass
from typing import Literal

from expression import Nothing, Option, Some
from expression.collections import Block


@dataclass(frozen=True, slots=True, kw_only=True)
class Receipt:
    code: str
    count: int

    @staticmethod
    def combined(left: "Receipt", right: "Receipt", /) -> "Receipt":
        return Receipt(code=left.code, count=left.count + right.count)


@dataclass(frozen=True, slots=True, kw_only=True)
class Fact:
    kind: Literal["add", "update", "remove", "error"]
    slot: str
    payload: Receipt | str


def collapsed(facts: Block[Fact], /) -> Block[Receipt]:
    return facts.choose(lambda fact: Some(fact.payload) if isinstance(fact.payload, Receipt) else Nothing)


def grouped_last(facts: Block[Fact], /) -> dict[str, Fact]:
    return facts.fold(lambda acc, fact: {**acc, fact.slot: fact}, {})
```

## [7]-[INTEROP]

One implementation crosses rails through the shared Kleisli surface; host values cross into rails at adapter edges only.

[RAIL_POLYMORPHIC]:
- Law: `expression`'s `Result` and `Option` share no monadic supertype — their only common bases are `Iterable`, `PipeMixin`, and `object`, so there is no higher-kinded `K[F, A]` carrier the body can quantify over. Carrier-neutrality is therefore name-overlap only: a body that calls exactly `map`/`bind`/`default_value` type-checks over the union `Result[T, E] | Option[T]` because both expose those member names, not because a shared trait abstracts them.
- Law: the rail is fixed once at the function's boundary and the per-carrier sibling family is the rejected form; a body claiming neutrality never constructs `Ok`/`Some`/`Error`/`Nothing` by name, because the moment it does it commits to one carrier and the union no longer type-checks.
- Boundary: a transform that must differ by carrier crosses the rail once with `Result.to_option` / `Option.to_result(fault)` at the boundary, then runs one fixed-carrier body — the union-typed neutral form is reserved for the literal shared-member pipeline, never a substitute for the missing HKT.
- Boundary: host lists, dicts, and trees convert to `Block`/`Map` at the adapter edge, not as domain flow; rail policy never selects a numeric replacement.
- Reject: duplicated `Result` and `Option` pipelines for one shared-member transform; a `bind` chain re-typed per carrier; a "carrier-neutral" body that names a constructor and so silently fixes the carrier the signature claims is open.

[ASYNC_RAIL_BRIDGE]:
- Law: an async boundary returns `Result` from an `async def` — the rail composes inside the coroutine through `bind`/`map`, and the `await` happens at the boundary, never inside a pure rail transform.
- Law: the effect's structured-concurrency scope, deadline, and retry compose at the call site as `anyio` scopes and the `stamina` aspect; the rail carries only the typed outcome, the scope carries how it ran.
- Reject: `await` inside a synchronous rail transform; an effect that owns acquisition without an `async with` resource scope; a coroutine returning `T | None` where `Result` states the outcome.

```python conceptual
from collections.abc import Awaitable, Callable

import anyio
from expression import Error, Ok, Option, Result


async def bridged[T](operation: Callable[[], Awaitable[Result[T, str]]], fallback: Callable[[], T], /, *, seconds: float) -> Result[T, str]:
    outcome: Result[T, str] = Error("<deadline>")
    with anyio.move_on_after(seconds) as scope:
        outcome = await operation()
    return Ok(fallback()) if scope.cancelled_caught else outcome


def carrier_neutral[T, U](value: "Result[T, str] | Option[T]", step: Callable[[T], U], /) -> "Result[U, str] | Option[U]":
    return value.map(step)
```

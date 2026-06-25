# [PYTHON_RAILS_AND_EFFECTS]

This page is the carrier algebra: `expression` owns the result and absence carriers, the do-notation builders, and immutable traversal, and the page legislates which carrier states an outcome, how a boundary mints it, how a collection threads it, how a fault vocabulary combines, and how state and receipts carry evidence. The carrier is the sequencing algebra — `Option` and `Result` short-circuit, the accumulating fold combines — never a flag the body re-reads. A carrier is chosen once at admission and threaded unchanged: the narrowest carrier that states the real outcome carries the value, reusable transforms keep it, and collapse to a bare value or a raised exception happens only at the process, CLI, network, or persistence edge. The interior is total over admitted carriers — raw provider shapes, `None`-as-failure, unclassified exceptions, and cancellation never travel it. The structured-concurrency runtime that runs these carriers — task groups, deadline and cancel scopes, blocking-call offload, resource brackets, the `stamina` retry weave — is `concurrency.md`'s; this page composes that rail only to state which deadline, group fault, or cancellation crosses the seam into the fault vocabulary, never to re-teach the scope mechanics.

## [01]-[RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider carrier is earned only by a capability the narrower one cannot carry: a typed failure cause, or accumulated independent faults. The carrier choice is itself the sequencing algebra; the collection owner and the do-notation builder are the same algebra at traversal scale, not a sixth and seventh carrier.

| [INDEX] | [SURFACE]                | [OWNS]                         | [REJECT]                          |
| :-----: | :----------------------- | :----------------------------- | :-------------------------------- |
|  [01]   | `Option[T]`              | non-failing absence            | `None`-as-hidden-failure          |
|  [02]   | `Result[T, E]`           | typed fallibility, fail-fast   | raised control flow in domain     |
|  [03]   | `@effect.result[T, E]()` | sequential `bind` do-notation  | nested `bind` lambda ladders      |
|  [04]   | fault-combining fold     | accumulated independent faults | first-failure `bind` over a batch |
|  [05]   | `Block[T]` / `Map[K, V]` | immutable traversal and lookup | mutable list/dict domain flow     |
|  [06]   | threaded frozen owner    | single-producer state          | shared mutable global             |

`Option[T]` carries absence with zero failure semantics; promote to `Result[T, E]` when the caller must know why, where the error `E` is a closed fault vocabulary, never a bare `str` for a domain with more than one mode. `Try[T]` is not a seventh carrier — it is `Result[T, Exception]` pinned to the exception side through `effect.try_`, minted at a foreign boundary and mapped into the domain fault vocabulary in the same expression, so it lives in the boundary card, never the interior. The accumulating fold is not a carrier but a sequencing discipline over `Result`; it earns its own row because abort-versus-accumulate is a correctness decision the carrier alone does not make. The serialized many-producer state cell is the boundary owner's surface and the async scope that bounds an effect is the concurrency page's; this page chooses between a frozen owner threaded for one producer and that cell, and stops at the carrier decision.

[REPRESENTATION_DEFAULT]:
- Law: `Nothing` is the zero-value absence — a singleton, total over the missing case, never `None` past the seam; a fallible field carries `Result`, never `T | None`, and `None` survives only as a genuine domain or wire value where absence-with-cause stays a closed family.
- Law: `Result` equality compares tag then payload, so a `set` or `dict` keyed on `Result` distinguishes `Ok(x)` from `Error(e)` and distinct faults from each other — a single-fault collapse coalesces every failure to one key, which an accumulating boundary must never do.
- Law: a carrier is migrated, never re-chosen — `Result.swap()` routes the fault half through the same transforms as the success half, and `Result.merge()` collapses a same-typed `Ok`/`Error` to one value only at a point where both arms are already the egress type, never as a mid-pipeline escape from the rail.
- Boundary: a frozen owner's optional field defaults to `Nothing` or a `sentinel`, never a bare `None` that conflates omission with a valid null.

## [02]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome; reusable transforms keep that carrier and never re-project mid-pipeline. The interior never sees the provider exception, the provider sentinel, or the carrier-to-carrier round trip that erases a fault.

[EXCEPTION_CAPTURE]:
- Law: one boundary adapter wraps the throwing provider call, naming the exact exception types the provider raises — `pydantic.ValidationError`, `msgspec.ValidationError`, `msgspec.DecodeError`, `OSError` — never a bare `except Exception`, so an unexpected exception propagates as a defect rather than being silently railed; `effect.try_` is the single-trap form when one provider exception maps to one fault, and the explicit multi-`except` adapter is the form when distinct provider exceptions discriminate into distinct domain faults under one closed vocabulary.
- Law: `except*` groups a multi-failure provider boundary and `BaseException.add_note()` preserves causal context when the fault is re-spelled, so the failure set survives the conversion into the domain vocabulary; the `concurrency.md` task group raises that group and the deadline raises `TimeoutError`, and the adapter that converts either into a fault case is this page's, the scope that produced it the concurrency page's.
- Law: a `@beartype` runtime-contract violation is one more provider exception this adapter captures — catch `beartype.roar.BeartypeCallHintViolation` at the egress and map it into the fault vocabulary like any other foreign raise; the alternative that redirects the violation onto the carrier without a `try` is the surface page's contract weave, composed there at definition time, not a second capture form taught here.
- Reject: `contextlib.suppress` hiding fallibility; a bare `try`/`except` wrapping interior carrier transforms; discarding the caught message when mapping to the fault.

[CROSS_RAIL_PROJECTION]:
- Law: the instance matrix — `Option.to_result(error)`, `Result.to_option()`, `Option.of_result(...)`, `Option.of_optional(value)`, `Result.of_option(error)` — migrates a carrier exactly once at a boundary; widening supplies the missing structure, narrowing discards it, and `to_result_with`/`of_option_with` supply a computed fault when the cause depends on the absent value's site.
- Law: the `Result -> Option -> Result` round trip erases the original fault and breaks every later `map_error` or fault-code predicate; `option.to_result(specific_fault)` re-supplies a precise fault instead of a placeholder.
- Reject: round trips where an explicit projection carries the fault through; `Option.value` access without a prior `is_some` proof; collapse inside a pure projection that stays railed.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from typing import Literal

import anyio
import msgspec
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result

type AdmitFault = Literal["<invalid-payload>", "<decode-failed>", "<contract>", "<deadline>", "<unreachable>"]

# --- [MODELS] ---------------------------------------------------------------------------
_DECODER = msgspec.json.Decoder(type="Shape")


@beartype
def _refined(value: "Shape", /) -> "Shape":
    return value


# --- [OPERATIONS] -----------------------------------------------------------------------
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


async def fetched(channel: "Channel", raw: bytes, /, *, seconds: float) -> Result["Shape", AdmitFault]:
    outcome: Result["Shape", AdmitFault] = Error("<deadline>")
    try:
        with anyio.move_on_after(seconds) as scope:
            outcome = admitted(await channel.receive(raw))
    except* OSError as group:
        group.add_note(f"<reasons:{[type(exc).__name__ for exc in group.exceptions]}>")
        outcome = Error("<unreachable>")
    else:
        outcome = Error("<deadline>") if scope.cancelled_caught else outcome
    return outcome
```

## [03]-[TRAVERSAL_FLOW]

Traversal is carrier policy: the collection owner and the sequencing operator together decide how failures, strictness, and order compose. The operator is the sequencing decision, never a performance choice — fail-fast threads the carrier and aborts on the first `Error`, accumulating folds every element and combines the faults.

[COLLECTION_OWNER]:
- Law: `Block[T]` is the immutable sequence owner — persistent, structurally shared, `cons`/`append`/`fold`/`reduce`/`choose`/`partition` — for domain traversal; `Map[K, V]` is the persistent keyed owner with `add`/`change`/`fold`/`try_find`; a plain `tuple` carries fixed-arity evidence, not a growing sequence.
- Law: `Block.fold(folder, seed)` folds left with an immutable accumulator and `Block.reduce(reducer)` folds a non-empty `Block` with no seed; incremental build is one `fold` or `cons`-then-reverse, never repeated mutation of a list then a freeze.
- Use: `Block.choose(f)` for atomic single-pass filter-map into `Option`, replacing a `filter` then `map`; `Block.partition(p)` when both the kept and rejected halves are needed; `Block.collect(f)` to flat-map a per-element `Block` without an intermediate nesting.
- Reject: a mutable `list` appended in a `for` loop as domain flow; `zip` across unequal lengths without `zip(strict=True)`; re-enumerating a lazy `Seq` whose cost is paid per pass.

[SEQUENCING_OPERATOR]:
- Law: fail-fast traversal is one named reducer — `threaded` binds the running `Result[Block[T], E]` accumulator and appends each `Ok` value, short-circuiting the whole `Block.fold` on the first `Error` — because `expression` ships no `Block.traverse`/`sequence`, so the seed-`bind` fold over `Ok(Block.empty())` is the substrate's traversal primitive and the page declares it once rather than re-inlining the lambda per site.
- Law: the accumulating form keeps both halves with one `Block.choose` over `to_option` and one over `swap().to_option()`, then combines the fault half through the vocabulary's own combination law — the partition is the sequencing discipline that reports every casualty, the reducer the one that aborts, and the carrier alone switches between them.
- Law: the `@effect.result[T, E]()` builder is the do-notation form of the fail-fast fold — a generator where each `yield from` is a `bind` that short-circuits the whole block on the first `Error` and the `return` is the final `Ok`; it linearizes a dependent `for ... yield from` chain that a nested-`bind` lambda ladder obscures, and is the readable form once the chain exceeds three steps.
- Boundary: the dispatch over a modality (`T | Iterable[T]`) and the head-normalization that drives it are owned by the surfaces page; this page composes the reducer it hands that dispatch to state the failure semantics it carries, never to re-teach the arity normalization.
- Reject: `map` then a manual carrier unwrap; an index-threaded fold unless the fold genuinely carries algorithm state; a `match` on the carrier tag where `bind`/`choose` already routes both arms.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from typing import Literal

from expression import Error, Ok, Result, effect
from expression.collections import Block

type TraverseFault = Literal["<empty>", "<over>"]


# --- [OPERATIONS] -----------------------------------------------------------------------
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

## [04]-[FAULT_VOCABULARY]

The failure type is a closed vocabulary the program owns, and the carrier realizes its disposition: conjunctive faults accumulate, disjunctive faults abort. Apply carrier-qualified failure transforms before collapse; a carrier transform never raises, and recovery keys on the fault's own code or case, never on a reconstructed message, so a fault re-spelled with `map_error` still routes to the right arm.

| [INDEX] | [COMBINATOR]         | [CARRIER]          | [USE]                          |
| :-----: | :------------------- | :----------------- | :----------------------------- |
|  [01]   | `.map_error(f)`      | `Result`, `Try`    | re-spell the fault, keep cause |
|  [02]   | `.or_else_with(f)`   | `Result`, `Option` | recover from the fault         |
|  [03]   | `.swap()`            | `Result`           | route the fault half           |
|  [04]   | `.merge()`           | `Result`           | collapse same-typed arms       |
|  [05]   | `.filter_with(p, f)` | `Result`           | guard with a fault on fail     |
|  [06]   | `.default_with(f)`   | `Result`, `Option` | terminal lazy fallback         |

[VOCABULARY_SHAPE]:
- Law: the failure type is a closed family — a `Literal` set for a handful of causes, a `StrEnum` when the causes are iterated or wire-carried, a `@tagged_union` fault family when a cause carries a structured payload — and the family separates the two dispositions: conjunctive faults that accumulate (every validation failure of one admission combines) and disjunctive faults that abort (the first irrecoverable cause wins). The carrier realizes the disposition — the accumulating fold for the conjunctive set, `bind` short-circuit for the disjunctive set — and a fault that a deadline, a cancelled scope, or a task-group failure produces enters as one more case here, the scope that raised it owned by the concurrency page.
- Law: an accumulating boundary needs a fault type with a combination law — the aggregate case holds the typed members so all-fault reporting keeps each member structurally addressable, never flattened to one concatenated string; the aggregate combines members, it does not stringify them, and `Block.reduce(Fault.combined)` is the one fold that closes the casualty set.
- Law: construction is two-tier — a private constructor builds the well-formed fault and a public `of_*` classmethod returns `Result[Fault, ...]` (or the fault directly when construction cannot fail), so an interior never hand-builds a malformed fault, and `map_error` re-spells without losing the provider message, validation detail, or category.
- Reject: `Exception` subclasses as the interior fault type; a bare `str` fault for a multi-cause domain; `None` standing in for failure; recovery by `==` over a message; an aggregate that joins members into one string and erases their codes.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class Fault:
    tag: Literal["bounds", "absent", "deadline", "aggregate"] = tag()
    bounds: tuple[int, int] = case()
    absent: str = case()
    deadline: float = case()
    aggregate: tuple["Fault", ...] = case()

    @staticmethod
    def combined(left: "Fault", right: "Fault", /) -> "Fault":
        match left, right:
            case Fault(tag="aggregate"), Fault(tag="aggregate"):
                return Fault(aggregate=(*left.aggregate, *right.aggregate))
            case Fault(tag="aggregate"), _:
                return Fault(aggregate=(*left.aggregate, right))
            case _, Fault(tag="aggregate"):
                return Fault(aggregate=(left, *right.aggregate))
            case _, _:
                return Fault(aggregate=(left, right))


# --- [OPERATIONS] -----------------------------------------------------------------------
def guarded(score: int, low: int, high: int, /) -> Result[int, Fault]:
    return Ok(score).filter_with(lambda s: low <= s <= high, lambda _s: Fault(bounds=(low, high)))


def accumulated(scores: Block[tuple[int, int, int]], /) -> Result[Block[int], Fault]:
    railed = scores.map(lambda triple: guarded(*triple))
    faults = railed.choose(lambda r: r.swap().to_option())
    return Ok(railed.choose(lambda r: r.to_option())) if faults.is_empty() else Error(faults.reduce(Fault.combined))


def recovered(outcome: Result[int, Fault], /) -> Result[int, Fault]:
    return outcome.or_else_with(lambda fault: Ok(0) if fault.tag == "deadline" else Error(fault))
```

## [05]-[STATE_RECEIPTS]

State belongs at a boundary or session owner, threaded immutably for one producer and promoted to the boundary owner's serialized cell for many. A typed receipt carries evidence; a fact stream carries chronology; neither collapses into a generic ledger.

[STATE_CARRIER]:
- Law: the carrier choice is producer count and nothing else — a single producer threads the frozen owner immutably through the pipeline, each step returning the next owner with no cell and no contention, while a many-producer cell is the boundary owner's serialized agent; this page selects the carrier and stops, the agent's drain, token, apply, and reply mechanics being the cell owner's law, not restated here.
- Reject: a cell where one producer's immutable thread carries the state without contention; `expression.MailboxProcessor` as the cell — the `anyio` substrate routes that rejection to the cell owner on the boundary page.

[RECEIPTS]:
- Law: the split is capability — a fact stream answers what happened and when, a typed receipt answers how this computation resolved; one `@tagged_union` fact owner makes resolve, supersede, retire, and fault its cases, the slot a derived projection and the payload each case's own evidence, so a total `match` with `assert_never` flags an unhandled kind and parallel record types synced by hand never appear.
- Law: keep a typed receipt — a frozen owner whose fields carry solver, sampling, route, status, metric, or proof evidence — when the fields are evidence; projection by kind, group-by-slot-last-wins, and full chronology are pure folds over the one fact stream, and a receipt combines through its own monoidal `combined`, never a generic accumulator.
- Reject: parallel `added`/`updated`/`removed` lists kept in sync; a `Literal` tag beside a `Receipt | str` payload where the case family carries the evidence; a generic `dict[str, object]` receipt erasing the typed fields; payload bytes carried on a receipt where a hash suffices.

```python conceptual
# --- [MODELS] ---------------------------------------------------------------------------
from dataclasses import dataclass
from typing import Literal, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map


@dataclass(frozen=True, slots=True, kw_only=True)
class Receipt:
    route: str
    samples: int
    residual: float

    @staticmethod
    def combined(left: "Receipt", right: "Receipt", /) -> "Receipt":
        return Receipt(route=right.route, samples=left.samples + right.samples, residual=min(left.residual, right.residual))


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


# --- [OPERATIONS] -----------------------------------------------------------------------
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


def collapsed(facts: Block[Fact], /) -> Option[Receipt]:
    return Nothing if (witnessed_all := facts.choose(witnessed)).is_empty() else Some(witnessed_all.reduce(Receipt.combined))


def grouped_last(facts: Block[Fact], /) -> Map[str, Fact]:
    return facts.fold(lambda acc, fact: acc.add(fact.slot, fact), Map.empty())
```

# [PYTHON_RAILS_AND_EFFECTS]

This page is the carrier algebra: `expression` owns the result and absence carriers, the do-notation builders, and immutable traversal, and the page legislates which carrier states an outcome, how a boundary mints it, how a collection threads it, how a fault vocabulary combines, and how state and receipts carry evidence. The carrier is the sequencing algebra — `Option` and `Result` short-circuit, the accumulating fold combines — never a flag the body re-reads. A carrier is chosen once at admission and threaded unchanged: the narrowest carrier that states the real outcome carries the value, reusable transforms keep it, and collapse to a bare value or a raised exception happens only at the process, CLI, network, or persistence edge. The interior is total over admitted carriers — raw provider shapes, `None`-as-failure, unclassified exceptions, and cancellation never travel it. The structured-concurrency runtime that runs these carriers is `concurrency.md`'s; this page owns only the algebra, composing that rail solely to state which raised deadline, group fault, or cancellation crosses the seam into the fault vocabulary.

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
- Law: the carrier states the outcome, never the value's representation — a fallible step returns `Result`, a non-failing absent step returns `Option`, and the interior is total over both because `shapes.md`'s admission already settled where `Nothing`, a `sentinel`, and a genuine wire `None` each live; this page never re-derives that representation, it only chooses which carrier transports the step.
- Law: `Result` equality compares tag then payload, so a `set` or `dict` keyed on `Result` distinguishes `Ok(x)` from `Error(e)` and distinct faults from each other — a single-fault collapse coalesces every failure to one key, which an accumulating boundary must never do, and is why the fault vocabulary stays structurally addressable rather than message-collapsed.
- Law: a carrier is migrated, never re-chosen — `Result.swap()` routes the fault half through the same transforms as the success half, and `Result.merge()` collapses a same-typed `Ok`/`Error` to one value only at a point where both arms are already the egress type, never as a mid-pipeline escape from the rail.

## [02]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome; reusable transforms keep that carrier and never re-project mid-pipeline. The interior never sees the provider exception, the provider sentinel, or the carrier-to-carrier round trip that erases a fault.

[EXCEPTION_CAPTURE]:
- Law: one boundary adapter wraps the throwing provider call, naming the exact exception types the provider raises — `pydantic.ValidationError`, `msgspec.ValidationError`, `msgspec.DecodeError`, `OSError` — never a bare `except Exception`, so an unexpected exception propagates as a defect rather than being silently railed; `effect.try_` is the single-trap form when one provider exception maps to one fault, and the explicit multi-`except` adapter is the form when distinct provider exceptions discriminate into distinct domain faults under one closed vocabulary.
- Law: `except*` groups a multi-failure provider boundary and `BaseException.add_note()` preserves causal context when the fault is re-spelled, so the failure set survives the conversion into the domain vocabulary; the `concurrency.md` task group raises that group and the deadline raises `TimeoutError`, and the adapter that converts either into a fault case is this page's, the scope that produced it the concurrency page's.
- Law: a `@beartype` runtime-contract violation is one more provider exception this adapter captures — catch `beartype.roar.BeartypeCallHintViolation` at the egress and map it into the fault vocabulary like any other foreign raise; the alternative that redirects the violation onto the carrier without a `try` is the surface page's contract weave, composed there at definition time, not a second capture form taught here.
- Reject: `contextlib.suppress` hiding fallibility; a bare `try`/`except` wrapping interior carrier transforms; discarding the caught message when mapping to the fault.

[CROSS_RAIL_PROJECTION]:
- Law: the instance matrix — `Option.to_result(error)`, `Result.to_option()`, `Option.of_result(...)`, `Option.of_optional(value)`, `Result.of_option(error)` — migrates a carrier exactly once at a boundary; widening supplies the missing structure, narrowing discards it, and `to_result_with`/`of_option_with` take a zero-argument thunk that mints the fault lazily on the absent branch alone, so a fault whose construction is itself costly is paid only when the carrier is `Nothing`, never eagerly on the present path.
- Law: the `Result -> Option -> Result` round trip erases the original fault and breaks every later `map_error` or fault-code predicate; `option.to_result(specific_fault)` re-supplies a precise fault instead of a placeholder.
- Reject: round trips where an explicit projection carries the fault through; `Option.value` access without a prior `is_some` proof; collapse inside a pure projection that stays railed.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Literal

import msgspec
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Option, Result

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


async def converted(scoped: Callable[[], Awaitable[bytes]], cached: Option[bytes], /) -> Result["Shape", AdmitFault]:
    outcome: Result["Shape", AdmitFault] = Error("<unreachable>")
    try:
        return admitted(await scoped())
    except* TimeoutError:
        outcome = cached.to_result_with(lambda: "<deadline>").bind(admitted)
    except* OSError as group:
        group.add_note(f"<reasons:{[type(exc).__name__ for exc in group.exceptions]}>")
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
- Law: fail-fast traversal is `expression.extra.result.traverse(step, block)` — the substrate's own applicative threader that maps each element through a `Result`-returning step, short-circuits the whole `Block` on the first `Error`, and collects every `Ok` into `Result[Block[T], E]` — and `sequence(block_of_results)` is `traverse(identity, ...)` for an already-railed `Block`; the hand-rolled seed-`bind` fold over `Ok(Block.empty())` is the rejected reimplementation of a primitive the package already ships.
- Law: the named `threaded` reducer is reserved for a fold the substrate primitive cannot express — a non-append accumulation order, an interleaved running state beside the carrier, a heterogeneous seed — and is then explicit through `Block.fold(threaded, seed)`; a `threaded` re-spelling of plain `traverse` is the COLLAPSE_SCAN wrapper signal, dissolved into the `traverse` call.
- Law: the accumulating form is the page's own primitive because the substrate ships only the fail-fast threader — one `Block.choose` over `to_option` keeps the survivors and one over `swap().to_option()` keeps the casualties, then the fault half combines through the vocabulary's own law; the partition reports every casualty, `traverse` aborts on the first, and the carrier alone switches between them.
- Law: the `@effect.result[T, E]()` builder is the do-notation form of a dependent chain — a generator where each `yield from` is a `bind` short-circuiting the whole block on the first `Error` and the `return` is the final `Ok`; it linearizes a dependent step ladder that `traverse` cannot express because each step's input is the prior step's output, the readable form once the chain exceeds three steps.
- Boundary: the dispatch over a modality (`T | Iterable[T]`) and the head-normalization that drives it are owned by the surfaces page; this page composes `traverse` to state the failure semantics that dispatch carries, never to re-teach the arity normalization.
- Reject: a hand-rolled seed-`bind` fold where `traverse`/`sequence` states it; `map` then a manual carrier unwrap; an index-threaded fold unless the fold genuinely carries algorithm state; a `match` on the carrier tag where `bind`/`choose` already routes both arms.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from functools import partial
from typing import Literal

from expression import Error, Ok, Result, effect
from expression.collections import Block
from expression.extra.result import sequence, traverse

type TraverseFault = Literal["<empty>", "<over>"]


# --- [OPERATIONS] -----------------------------------------------------------------------
def admitted_one(ceiling: int, raw: str, /) -> Result[int, TraverseFault]:
    return Error("<empty>") if raw == "" else Error("<over>") if len(raw) > ceiling else Ok(len(raw))


def traversed_fail_fast(raws: Block[str], ceiling: int, /) -> Result[Block[int], TraverseFault]:
    return traverse(partial(admitted_one, ceiling), raws)


def resequenced(railed: Block[Result[int, TraverseFault]], /) -> Result[Block[int], TraverseFault]:
    return sequence(railed)


def traversed_accumulate(raws: Block[str], ceiling: int, /) -> tuple[Block[int], Block[TraverseFault]]:
    railed = raws.map(partial(admitted_one, ceiling))
    return railed.choose(lambda r: r.to_option()), railed.choose(lambda r: r.swap().to_option())


@effect.result[int, TraverseFault]()
def assembled(head: str, tail: str, ceiling: int):
    first = yield from admitted_one(ceiling, head)
    second = yield from admitted_one(first, tail)
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
- Law: the failure type is a closed family — a `Literal` set for a handful of causes, a `StrEnum` when the causes are iterated or wire-carried, a `@tagged_union` fault family when a cause carries a structured payload — chosen by `shapes.md`'s owner discriminants and admitted through its two-tier `of_*` constructor, so the interior never hand-builds a malformed fault; this card spends its lines on the one thing the shape alone does not decide — which disposition the carrier realizes.
- Law: the disposition is the correctness decision this page owns — conjunctive faults accumulate (every independent validation of one admission combines) and disjunctive faults abort (the first dependent cause wins) — and the carrier alone realizes it: the partitioning fold for the conjunctive set, `bind` short-circuit for the disjunctive set. A deadline, a cancelled scope, or a task-group fault enters as one more case in the family, the scope that raised it owned by the concurrency page; the disposition rides as the surfaces page's policy value (`Disposition`), never a `strict: bool` the body re-derives.
- Law: the conjunctive disposition is only realizable when the fault type carries a combination law — `Fault.combined` is an associative monoid whose `aggregate` case holds the typed members and whose `_members` normalization flattens nested aggregates so `Block.reduce(Fault.combined)` closes the casualty set keeping each member structurally addressable instead of message-collapsed; this monoid is the fault-side instance of the same combination law a receipt carries in `[05]`, and a fault family without it can only abort.
- Law: a fault is migrated through the same combinators as the success half — `map_error` re-spells the category later without losing the `of_detail` provider message it carries, `to_result_with` supplies the fault as a zero-argument thunk paid only on the absent branch, and recovery keys on the case or code the fault still carries, never the re-spelled message.
- Reject: `Exception` subclasses as the interior fault type; a bare `str` fault for a multi-cause domain; `None` standing in for failure; recovery by `==` over a message; an aggregate that joins members into one string and erases their codes; a disposition re-derived from a flag the policy value already carries.

```python conceptual
# --- [TYPES] ----------------------------------------------------------------------------
from enum import StrEnum
from typing import Literal

from expression import Error, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import sequence

type FaultDefect = Literal["<empty-detail>"]


class Disposition(StrEnum):
    ABORT = "<abort>"
    ACCUMULATE = "<accumulate>"


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class Fault:
    tag: Literal["bounds", "detail", "deadline", "aggregate"] = tag()
    bounds: tuple[int, int] = case()
    detail: tuple[str, str] = case()
    deadline: float = case()
    aggregate: tuple["Fault", ...] = case()

    @staticmethod
    def of_detail(category: str, message: str, /) -> Result["Fault", FaultDefect]:
        return Ok(Fault(detail=(category, message))) if message else Error("<empty-detail>")

    @staticmethod
    def _members(fault: "Fault", /) -> tuple["Fault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "Fault", right: "Fault", /) -> "Fault":
        return Fault(aggregate=(*Fault._members(left), *Fault._members(right)))


# --- [OPERATIONS] -----------------------------------------------------------------------
def guarded(found: Option[int], low: int, high: int, /) -> Result[int, Fault]:
    return found.to_result_with(lambda: Fault(bounds=(low, high))).filter_with(
        lambda score: low <= score <= high, lambda _s: Fault(bounds=(low, high))
    )


def reported(scores: Block[tuple[Option[int], int, int]], disposition: Disposition, /) -> Result[Block[int], Fault]:
    railed = scores.map(lambda probe: guarded(*probe))
    match disposition:
        case Disposition.ABORT:
            return sequence(railed)
        case Disposition.ACCUMULATE:
            faults = railed.choose(lambda r: r.swap().to_option())
            return Ok(railed.choose(lambda r: r.to_option())) if faults.is_empty() else Error(faults.reduce(Fault.combined))


def recovered(outcome: Result[int, Fault], /) -> Result[int, Fault]:
    return outcome.map_error(lambda fault: Fault.of_detail("<recovered>", fault.tag).default_value(fault))
```

## [05]-[STATE_RECEIPTS]

State belongs at a boundary or session owner, threaded immutably for one producer and promoted to the boundary owner's serialized cell for many. A typed receipt carries evidence; a fact stream carries chronology; neither collapses into a generic ledger.

[STATE_CARRIER]:
- Law: the carrier choice is producer count and nothing else — a single producer threads the frozen owner immutably through the pipeline, each step returning the next owner with no cell and no contention, while a many-producer cell is the boundary owner's serialized agent; this page selects the carrier and stops, the agent's drain, token, apply, and reply mechanics being the cell owner's law, not restated here.
- Reject: a cell where one producer's immutable thread carries the state without contention; `expression.MailboxProcessor` as the cell — the `anyio` substrate routes that rejection to the cell owner on the boundary page.

[RECEIPTS]:
- Law: the split is capability — a fact stream answers what happened and when, a typed receipt answers how this computation resolved — and it is the page's region because no upstream owner carries chronology-versus-evidence; one `@tagged_union` fact owner makes resolve, supersede, retire, and fault its cases over the settled exhaustiveness mechanic `shapes.md` owns, the slot a derived projection and the payload each case's own evidence, so parallel record types synced by hand never appear.
- Law: keep a typed receipt — a frozen owner whose fields carry solver, sampling, route, status, metric, or proof evidence — when the fields are evidence; projection by kind, group-by-slot-last-wins, and full chronology are pure folds over the one fact stream, and a receipt combines through the same monoidal `combined` law the fault carries in `[04]` — `Receipt.combined` is its evidence-side instance — never a generic accumulator.
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
            case Fact(tag="resolved", resolved=(key, _)) | Fact(tag="superseded", superseded=(key, _)) | Fact(tag="faulted", faulted=(key, _)):
                return key
            case Fact(tag="retired", retired=key):
                return key
            case unreachable:
                assert_never(unreachable)

    @property
    def evidence(self) -> Option[Receipt]:
        match self:
            case Fact(tag="resolved", resolved=(_, receipt)) | Fact(tag="superseded", superseded=(_, receipt)):
                return Some(receipt)
            case Fact(tag="retired") | Fact(tag="faulted"):
                return Nothing
            case unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
def collapsed(facts: Block[Fact], /) -> Option[Receipt]:
    return Nothing if (witnessed := facts.choose(lambda fact: fact.evidence)).is_empty() else Some(witnessed.reduce(Receipt.combined))


def latest_per_slot(facts: Block[Fact], /) -> Map[str, Fact]:
    return facts.fold(lambda acc, fact: acc.add(fact.slot, fact), Map.empty())


def faults_in_order(facts: Block[Fact], /) -> Block[tuple[str, str]]:
    return facts.choose(lambda fact: Some(fact.faulted) if fact.tag == "faulted" else Nothing)
```

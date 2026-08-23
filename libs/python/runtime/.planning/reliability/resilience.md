# [PY_RUNTIME_RESILIENCE]

One retry-policy table rules the whole branch: `RetryClass` is the single behavior-carrying `stamina`-backed `StrEnum` the fault, transport, lane, and concurrency clusters consume through `guard`/`guarded`/`retrying` only, each member binding one frozen `Policy` row — attempts, timeout, one `Backoff` target, the row's re-offer route, and optional backoff columns as row data, never a flag the caller re-derives. Retry is TWO axes here: `Backoff` answers the `reliability/faults#FAULT` `Recovery` verdict — terminal, transient, or a peer-stated throttle window — and `Reoffer` answers the route that verdict takes, so a refusal a caller can still satisfy NARROWED is a declared value rather than a sentence on a consumer page. This is a BASE-tier module importing no sibling-tier package: every provider-discriminated target is import-free by construction, matching a gated provider's raise structurally instead of importing its class.

`guarded`/`guarded_sync` are the primary consumer envelopes — failure window, admission pacing, the cached bound caller, and one retry span off the `reliability/faults#FAULT` `SCOPES[Scope.RESILIENCE]` row, terminal raise lifted through the faults boundary once. Each stage reaches a class through its own table row, so an absent `CIRCUIT` or `RATES` row skips that stage. Every envelope takes the caller's own rostered `FaultRow[L]`, so one coordinate names the arc, the bucket, the span, and the lifted fault, and the lift's `catch` derives from the row's target rather than defaulting at the call site. Retriability is ONE vocabulary across two owners: `Backoff.retriability` answers the LIVE raise this table classifies and `BoundaryFault.retriability` the lifted fault a caller holds, both in `Recovery`, so the yes-or-no `recoverable` collapse stays a caller's own fence and never a second predicate here. `install(mode)` owns the one `set_on_retry_hooks` registration any second registration clobbers, and the DAG-proven `faults < receipts < metrics < resilience` order makes the module-load `Metrics.retry_hook()` bind eager.

## [01]-[INDEX]

- [02]-[RESILIENCE]: `RetryClass` policy table, the `Backoff` target owning both retry axes, the `guard`/`guarded`/`retrying` family over one row, and the installed retry-hook stack.
- [03]-[CIRCUIT]: `Breaker` — the per-dependency failure window, its three-state arc, and the fail-fast arm `guarded` folds ahead of every dial.
- [04]-[RATE]: `RateGate` — the per-destination token bucket, its peer-directed re-seat, and the waited-seconds evidence a debit that queued publishes.

## [02]-[RESILIENCE]

- Owner: `RetryClass`, `Policy`, `Backoff`, and the hook stack per the fence. `guard`/`guard_sync` memoise through a module-level `@cache` rather than a `cached_property` — an `Enum` member carries no writable `__dict__` — and only the reusable bound caller is safe to cache, the one-shot `retry_context` rebuilding per call.
- Cases: `Backoff` answers `Recovery` and `Policy.route` answers `Reoffer`, two closed axes over one raise. `Recovery` states WHETHER a refusal may be re-offered — the fault owner's own vocabulary, read here rather than re-minted — and `Reoffer` states HOW: `wait` re-invokes the identical call the schedule already timed, `restart` re-establishes the dependency handle first, and `rescope` names the LEG a caller takes instead. Fusing the two loses the state an operator most needs: a terminal the caller can still satisfy NARROWED.
- Entry: `guarded`/`guarded_sync` own the whole circuit/rate/retry/span/lift chain for every fetch-shaped leg, so a budget-exhausted transient surfaces as the `boundary` case naming the final cause and a non-transient raise surfaces immediately. Both take TWO coordinates answering different questions: the caller's rostered `at: FaultRow[L]` names WHICH CALL raised, so a fence cannot spell a leg its package never declares and the span, the lift, and a routed refusal all derive from one declaration; `on: Option[str]` names WHICH PEER the call reached, and the two stateful stages key on that alone through `Dependency`. `guard(cls)` is the bare bound caller for the one consumer that already owns its span and fault rail — the `execution/lanes#LANE` `retried` admission row, where a second span and boundary lift doubles the lane's rail; `retrying(cls)` is the inline form for blocks the caller cannot pre-shape as a coroutine; `install(mode)` returns the finalized `get_on_retry_hooks()` tuple as typed registration evidence.
- Law: the three-state verdict collapses at ONE edge — `Backoff.__call__`, lowering `Recovery` onto the `bool | float | timedelta` contract `runtime/.api/stamina.md` `[02]` declares for `on=`. Every interior reader takes the VALUE, because a bool read fuses a stated `retry_after=0.0` with a refusal and a stated `5.0` with a bare transient, leaving the breaker unable to separate an immediate re-offer from a terminal or a dependency that is DOWN from one that ANSWERED.
- Law: a window keys the dependency INSTANCE and never the fence row. A rostered row names the CALL, and one row serves every destination its lane dials, so keying arcs there fuses two peers into one and sheds every healthy caller of the peer that never went down; keying them at the fence ALSO splits one origin's arc across every fence that dials it, so no arc reaches its trip and an open arc stops meaning the dependency is out. `_keyed` is the one gate: a class declaring a `CIRCUIT` or `RATES` row refuses `config` when no peer is stated, a class declaring neither answers `Nothing` and both stages no-op on it, and a peer stated for such a class is KEPT so its first window row lands correctly keyed instead of breaking every call site at once.
- Law: the lift's `catch` DERIVES from the row's target rather than defaulting at the call site — a row naming only importable classes narrows the fence to exactly those, so an unexpected raise propagates as the defect it is, and a row matching a provider class by dotted spelling or structural probe widens to `Exception` because this BASE tier refuses the import that would name it. The widening is a declared property of the row, and `Exception` is the ceiling the fault owner fixes so a cancellation never converts.
- Auto: `Policy.schedule` folds only the present columns, so a `stamina` default stands for an absent column, and the schedule alone owns the wait a `transient` verdict earns while a `throttled` verdict overrides that attempt's curve with the peer's own window. No classified field rides a retry fact — the hook mints under the receipts-owned `OPEN` keep-all, never a local `Redaction` re-mint.
- Auto: `BROKER` reads the broker's OWN verdict rather than a transcribed code roster — `KafkaException.args[0]` is a `KafkaError` answering `retriable()` beside `fatal()` across a code space librdkafka revises per release, so a name roster here goes stale one bump later while the verdict never does. Three families ship a taxonomy and no verdict member, so each matches by dotted spelling at its narrowest transient arm: `pika`'s connection root beside the authentication and wrong-state subclasses it would otherwise absorb, `nats`'s reconnect, stale-connection, server-reach, and buffer arms beside the JetStream unavailability pair, and `paho`'s bare socket raises, since its own `MQTTException` names a property or packet misuse a re-dial never clears. `MaxPayloadError` holds the row's `rescope` seat: a payload past the negotiated ceiling is terminal for THIS call and satisfiable on the `dataref` leg, so the envelope hands that leg back as the refusal's own detail and no consumer page carries the alternative in prose.
- Growth: a new retry class is one `RetryClass` member with one `POLICY` row; a new provider family is one `_backoff` roster or one `Probe` on an existing row; a new re-offer route is one `Reoffer` case with one arm at the metered settle, the standing `assert_never` breaking every collapse that lacks it; a new narrowed alternative is one `rescope` pair on the row that refuses it; a new refusal this module raises is one `FaultRow` in `RAISES`, which the faults-owned `rostered` door seats into the ONE census so its posture and leg resolve.
- Boundary: no retry around a pure transform — `stamina` rides only flaky external oracles through this table. Exported contract is branch-consumer law: the entry family and its `FaultRow` parameter, every `POLICY` row with its route column, the `Reoffer` vocabulary, and the `RetryClass` vocabulary; narrowing the `OCCT` target below the `BrokenWorkerInterpreter | BrokenWorkerProcess` pair is a cross-folder break.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import time
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import AbstractContextManager
from datetime import timedelta
from enum import Enum, StrEnum
from functools import cache
from threading import Lock
from typing import ClassVar, Final, Literal, Protocol, assert_never, runtime_checkable

import anyio
import stamina
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from keyring.errors import KeyringLocked
from msgspec import UNSET, Struct, UnsetType
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from stamina.instrumentation import RetryDetails, RetryHook, RetryHookFactory, StructlogOnRetryHook, get_on_retry_hooks, set_on_retry_hooks

from rasm.runtime.faults import (
    SCOPES,
    TERMINAL,
    TRANSIENT,
    BoundaryFault,
    FaultRow,
    Leg,
    Recovery,
    RuntimeLeg,
    RuntimeRail,
    Scope,
    async_boundary,
    boundary,
    rostered,
    scoped,
    spelled,
)
from rasm.runtime.metrics import Dimension, Metrics
from rasm.runtime.receipts import OPEN, Receipt, Signals

# `Breaker` and `RateGate` are this module's [03]-[CIRCUIT] and [04]-[RATE] owners — one module, three regions.

# --- [TYPES] ----------------------------------------------------------------------------

# a per-family structural read answering the PROVIDER's own verdict where it publishes one and `Nothing` where the
# raise belongs to no such family, so the row's roster stays the fallback rather than a second competing predicate.
# stamina's own `on=` hook shape needs no alias beside it: `Backoff.__call__` spells that signature directly, and
# stamina exports nothing to import — `stamina.typing` carries only `RetryDetails`/`RetryHook`, `_core` is private.
type Probe = Callable[[Exception], Option[Recovery]]


@runtime_checkable
class RetryAfter(Protocol):
    retry_after: float | None


# structural slot of an adbc DBAPI error: `status_code` is the driver's status enum member.
@runtime_checkable
class StatusCarrier(Protocol):
    status_code: Enum


# structural slot of a Connect `ConnectError`: `code` carries the `connectrpc.code.Code` member the peer answered.
@runtime_checkable
class StatusCoded(Protocol):
    code: Enum


# structural slot of a librdkafka `KafkaError`, which `KafkaException` carries at `args[0]`: the client answers the
# transience verdict itself over its whole code space, so no roster of broker codes is transcribed into this table.
@runtime_checkable
class Verdicted(Protocol):
    def retriable(self) -> bool: ...
    def fatal(self) -> bool: ...


# `Reoffer` closes the ROUTE axis beside the fault owner's `Recovery` verdict: that value states WHETHER a refusal may
# be re-offered, this states HOW, and one value carrying both loses the case an operator most needs — a terminal a
# caller can still satisfy NARROWED. `wait` re-invokes the identical call the schedule already timed and is what every
# row meant before the axis was spellable; `restart` re-establishes the dependency handle first, so the pacing a dead
# handle earned never meters the fresh one; `rescope` names the leg the caller takes instead. That leg is a CONSUMER
# coordinate spelled structurally for the same reason a provider class is — this BASE tier reaches no sibling, so it
# names neither by import — and it rides the refusal's own detail rather than a sentence on the page that must act.
@tagged_union(frozen=True)
class Reoffer:
    tag: Literal["wait", "restart", "rescope"] = tag()
    wait: None = case()
    restart: None = case()
    rescope: str = case()  # the narrower leg a caller re-offers on — a `dataref` hand-off for a payload past a ceiling


class RetryClass(StrEnum):
    OBJECT_STORE = "object-store"
    HTTP = "http"
    SSH = "ssh"
    WIRE = "wire"
    SCAN = "scan"
    SECRET = "secret"
    ENGINE = "engine"
    ORACLE = "oracle"
    OCCT = "occt"
    OCC_NATIVE = "occ-native"
    WORKER = "worker"
    SPAWN = "spawn"
    RPC = "rpc"
    LAKE_COMMIT = "lake-commit"
    REMOTE_DB = "remote-db"
    STREAMING = "streaming"
    BROKER = "broker"

    @property
    def policy(self) -> "Policy":
        # RAW index because `POLICY` is TOTAL over this vocabulary, and that totality is a BOOT PROOF rather than a
        # convention: the `transport/shapes#BOOT_CENSUS` census the serve boot fold runs ahead of every custody-claiming
        # install carries the closed-roster arm that refuses a member holding no row, so an unrostered class kills the
        # boot instead of raising `KeyError` at a caller's first dial. `CIRCUIT`/`RATES` index through `try_find`
        # because their partiality is DECLARED — absence IS the no-op — so no census may ever close them.
        return POLICY[self]

    @property
    def circuit(self) -> Option["CircuitPolicy"]:
        # a class facing no dependency that can go DOWN carries no row, so the breaker is a no-op for it by absence
        # rather than by a boolean every envelope re-reads.
        return CIRCUIT.try_find(self)

    @property
    def rate(self) -> Option["RatePolicy"]:
        return RATES.try_find(self)


class RetryMode(StrEnum):
    EMIT = "emit"
    SILENT = "silent"
    TEST = "test"


# --- [CONSTANTS] ------------------------------------------------------------------------

# optional schedule columns whose `UNSET` value defers to the `stamina` default.
_WAIT_COLUMNS: Final[tuple[str, ...]] = ("wait_initial", "wait_max", "wait_jitter", "wait_exp_base")
# transient Connect status trio the `connectrpc` client-fault law names retriable; the `Code` member names ARE the wire spelling.
_WIRE_STATUS: Final[frozenset[str]] = frozenset({"UNAVAILABLE", "DEADLINE_EXCEEDED", "RESOURCE_EXHAUSTED"})
# adbc transport statuses a re-issue can clear; every other status names a request the driver refuses identically.
_ADBC_STATUS: Final[frozenset[str]] = frozenset({"TIMEOUT", "IO"})

# two stateless routes bind every row; `rescope` carries a named leg, so it mints per row and takes no anchor.
WAIT: Final[Reoffer] = Reoffer(wait=None)
RESTART: Final[Reoffer] = Reoffer(restart=None)


# --- [MODELS] ---------------------------------------------------------------------------


class Backoff(Struct, frozen=True):
    # ONE retry target for every row: the transient roster, the refusals that outrank it, the narrowed re-offers a
    # refusal earns, and the optional provider probe that outranks all three. A row is therefore DATA the substrate
    # consumes directly — `__call__` makes it `stamina`'s own `on=` policy value — rather than a closure hiding the
    # columns behind a call, which is what lets the breaker, the lift's catch, and the route read the same declaration.
    wanted: tuple[type[Exception], ...]
    named: frozenset[str]
    denied: tuple[type[Exception], ...]
    refused: frozenset[str]
    rescopes: Map[str, str]
    probe: Option[Probe]

    def retriability(self, cause: BaseException, /) -> Recovery:
        # ONE predicate with a DECLARED source per rung, so the breaker trips on exactly what the retry loop already
        # burned attempts against. Deriving a second predicate at the breaker opens a circuit on a malformed payload no
        # re-dial repairs, which sheds every healthy caller of a dependency that never went down. The provider's own
        # probe outranks this row's roster exactly as a peer-stated posture outranks a derived one at
        # `reliability/faults#FAULT`, and a refusal reads before the transient base it subclasses. A `BaseException`
        # that is no `Exception` — the cancellation class — is TERMINAL here and unreachable through `catch` below,
        # because cancellation is scope-owned flow control the retry edge never re-offers.
        if not isinstance(cause, Exception):
            return TERMINAL
        match self.probe.bind(lambda read: read(cause)):
            case Option(tag="some", some=stated):
                return stated
            case _:
                spellings = spelled(cause)
                denied = isinstance(cause, self.denied) or bool(spellings & self.refused)
                wanted = isinstance(cause, self.wanted) or bool(spellings & self.named)
                return TRANSIENT if wanted and not denied else TERMINAL

    def rescoped(self, cause: BaseException, /) -> Option[Reoffer]:
        # the NARROWED re-offer a refusal earns, read off the same MRO spelling set the roster matches on. Rows seat
        # narrowest arms, so at most one spelling on a raise's chain carries a seat and the head IS the answer.
        return Block.of_seq(spelled(cause)).choose(self.rescopes.try_find).try_head().map(lambda leg: Reoffer(rescope=leg))

    def __call__(self, exc: Exception) -> bool | float | timedelta:
        # THE ONE lowering to stamina's tri-state `on=` contract (`runtime/.api/stamina.md` `[02]` retry-target row:
        # `True`/`False` decides, and a `float`/`timedelta` both decides YES and overrides that attempt's computed
        # backoff), and the only place in the branch the three-state verdict collapses. A bool read here fuses a stated
        # `retry_after=0.0` with a refusal and a stated `5.0` with a bare transient, so the two states an operator most
        # needs separated — a dependency that is DOWN against one that ANSWERED — would reach the breaker as one.
        match self.retriability(exc):
            case Recovery(tag="throttled", throttled=seconds):
                return seconds
            case Recovery(tag="transient"):
                return True
            case Recovery(tag="terminal"):
                return False
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def catch(self) -> tuple[type[Exception], ...]:
        # the lift's catch DERIVES from this row: a target naming only importable classes narrows the fence to exactly
        # those, so an unexpected raise propagates as the defect it is. A row matching a provider class by dotted
        # spelling or through a structural probe cannot narrow — this BASE tier refuses the import that would name the
        # class — so the widening is a DECLARED property of the row rather than a call-site default, and it stops at
        # `Exception`, the ceiling `reliability/faults#FAULT` fixes so a cancellation never converts to a fault.
        return (Exception,) if self.named or self.refused or self.probe.is_some() else (*self.wanted, *self.denied)


class Verdict(Struct, frozen=True):
    # ONE read of a row per failed attempt carrying BOTH axes, so the retry loop, the breaker, the rate gate, and the
    # route cannot disagree about one raise — the fusion two independent reads of the same roster invites.
    recovery: Recovery
    route: Reoffer


class Policy(Struct, frozen=True):
    attempts: int
    timeout: float
    target: Backoff
    # the row's own re-offer route, `wait` where a re-invocation of the identical call is the whole re-offer; a
    # `rescope` seat on the target outranks it for the raises that name one.
    route: Reoffer = WAIT
    wait_initial: float | UnsetType = UNSET
    wait_max: float | UnsetType = UNSET
    wait_jitter: float | UnsetType = UNSET
    wait_exp_base: float | UnsetType = UNSET

    @property
    def schedule(self) -> dict[str, object]:
        base: dict[str, object] = {"attempts": self.attempts, "timeout": self.timeout}
        return base | {col: value for col in _WAIT_COLUMNS if (value := getattr(self, col)) is not UNSET}

    def verdict(self, cause: BaseException, /) -> Verdict:
        return Verdict(recovery=self.target.retriability(cause), route=self.target.rescoped(cause).default_value(self.route))


class Dependency(Struct, frozen=True):
    # THE coordinate both stateful stages key on, and the reason neither keys on `at.subject`: a fence row names the
    # CALL, and a call is the wrong grain for a window in BOTH directions. One rostered row serves every destination
    # a lane dials — `transport/roots#STORE` runs every store op through one row — so keying arcs there fuses two
    # buckets into one and sheds every healthy caller of the peer that never went down; and two fences dialing ONE
    # origin would keep two arcs, so neither ever reaches its trip and an open arc stops meaning the dependency is
    # out. The class names the dependency KIND and `instance` the peer that can go DOWN or pace — an origin, a
    # bucket, a cluster, a channel, a database — which is exactly the grain `CIRCUIT` and `RATES` rows are written at.
    retry: RetryClass
    instance: str


class CircuitPolicy(Struct, frozen=True):
    # window per DEPENDENCY CLASS, applied per dependency INSTANCE: `trips` consecutive transient terminals open the
    # circuit, `cooldown` seconds later exactly `probes` attempts cross to test it, and one success closes it whole.
    # Budgeting a half-open window above one probe re-storms a recovering peer with the concurrency that felled it.
    trips: int
    cooldown: float
    probes: int = 1


class RatePolicy(Struct, frozen=True):
    # steady-state permits per second beside the burst a quiet window banks. `permits` is the DEFAULT a destination
    # opens at; a peer's own stated wait — a `Retry-After` or librdkafka throttle window — re-seats it through
    # `RateGate.directed` off the `throttled` verdict the metered settle reads, so the negotiated rate is the operating
    # one and this row is the floor a silent destination keeps.
    permits: float
    burst: float


# --- [SERVICES] -------------------------------------------------------------------------

# minted from the faults-owned scope row off the proxy-until-install provider.
_TRACER: Final = scoped(trace.get_tracer, SCOPES[Scope.RESILIENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _windowed(exc: Exception) -> Option[Recovery]:
    # a producer that ANSWERED with its own wait: the stated seconds ride the `throttled` case whole, so a legitimate
    # `0.0` — re-dial immediately — stays distinguishable from the refusal a falsy read coalesces it with.
    match exc:
        case RetryAfter(retry_after=float() as seconds):
            return Some(Recovery(throttled=seconds))
        case _:
            return Nothing


def _statused(exc: Exception) -> Option[Recovery]:
    # an adbc `OperationalError` re-offers ONLY where `status_code` names a transport transient (`TIMEOUT`/`IO` — never
    # `INVALID_ARGUMENT`/`NOT_FOUND`, which a re-issue cannot clear); a raise from any other module is not this
    # driver's and defers to the row's own roster.
    match exc:
        case StatusCarrier(status_code=Enum() as code) if type(exc).__module__.partition(".")[0] == "adbc_driver_manager":
            return Some(TRANSIENT if code.name in _ADBC_STATUS else TERMINAL)
        case _:
            return Nothing


def _coded(exc: Exception) -> Option[Recovery]:
    # a `ConnectError` re-offers only on the transient status trio; the qualname guard keeps a foreign class that
    # happens to carry a `code` out of this verdict, and every other raise defers to the row's own roster.
    match exc:
        case StatusCoded(code=Enum() as code) if type(exc).__qualname__ == "ConnectError":
            return Some(TRANSIENT if code.name in _WIRE_STATUS else TERMINAL)
        case _:
            return Nothing


def _verdicted(exc: Exception) -> Option[Recovery]:
    # librdkafka answers its own verdict: `KafkaException.args[0]` is a `KafkaError` carrying `retriable()` beside
    # `fatal()` across a code space the client revises per release, so the verdict rides and no roster of broker codes
    # is transcribed here to go stale. The qualname guard is what keeps the structural match honest — a foreign
    # exception whose first argument happens to answer both members is not this client's error. `fatal()` reads FIRST
    # because a fenced producer and a poisoned transaction each report a code the retry column would otherwise burn
    # attempts against. Every other broker family defers to the row's own spellings.
    match exc.args:
        case (Verdicted() as error, *_) if type(error).__qualname__ == "KafkaError":
            return Some(TRANSIENT if not error.fatal() and error.retriable() else TERMINAL)
        case _:
            return Nothing


def _backoff(
    *targets: type[Exception] | str,
    refuse: tuple[type[Exception] | str, ...] = (),
    rescope: tuple[tuple[str, str], ...] = (),
    probe: Option[Probe] = Nothing,
) -> Backoff:
    # ONE polymorphic MRO-matching law over both target spellings, the value's own shape the discriminant: an importable
    # class (stdlib, BASE-tier dep) matches by isinstance, a gated provider's class by the faults-owned `spelled` set —
    # identical subclass semantics, zero provider imports, a bare-name collision unspellable, and one derivation shared
    # with the `CLASSIFY` frozenset rows rather than a second copy of the dotted-spelling convention. `refuse` rides the
    # same polymorphic axis and reads first, pinning the terminal subclasses a transient base would otherwise absorb
    # (asyncssh auth/host-key failures subclass DisconnectError; a missing binary subclasses OSError). Every `rescope`
    # key joins that refusal set BY CONSTRUCTION — a raise the row hands a narrower leg is a raise no schedule
    # re-drives — so the two columns can never disagree about one class, and the per-family `probe` rides one column
    # here rather than four near-identical factories each re-spelling the same fallback beneath its own structural read.
    seats = Map.of_seq(rescope)
    return Backoff(
        wanted=tuple(t for t in targets if isinstance(t, type)),
        named=frozenset(t for t in targets if isinstance(t, str)),
        denied=tuple(t for t in refuse if isinstance(t, type)),
        refused=frozenset(t for t in refuse if isinstance(t, str)) | frozenset(seats.keys()),
        rescopes=seats,
        probe=probe,
    )


def _retry_receipt() -> RetryHook:
    # stamina's `RetryHook` is a synchronous callable even on the async retry path, so the
    # receipt mints through the sync `Signals.emit`, never the loop-only `emit_async` mirror.
    def hook(details: RetryDetails) -> AbstractContextManager[None]:
        cause = type(details.caused_by).__qualname__
        Signals.emit(
            Receipt.of(
                "resilience",
                (
                    "planned",
                    details.name,
                    {"retry_num": details.retry_num, "wait_for": details.wait_for, "waited_so_far": details.waited_so_far, "caused_by": cause},
                ),
            ),
            OPEN,
        )
        span = _TRACER.start_span(
            "resilience.retry", attributes={"rasm.retry_num": details.retry_num, "rasm.wait_for": details.wait_for, "rasm.caused_by": cause}
        )
        span.set_status(Status(StatusCode.ERROR, cause))
        return trace.use_span(span, end_on_exit=True)

    return hook


# memoised per member; each `__call__` opens a fresh internal `retry_context`.
@cache
def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    row = cls.policy
    return stamina.AsyncRetryingCaller(**row.schedule).on(row.target)


# sync mirror: same `Policy.schedule` source, second runtime arm, one table.
@cache
def guard_sync(cls: RetryClass) -> stamina.BoundRetryingCaller:
    row = cls.policy
    return stamina.RetryingCaller(**row.schedule).on(row.target)


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    # one-shot iterator, rebuilt per call — never cached.
    row = cls.policy
    return stamina.retry_context(on=row.target, **row.schedule)


def _marks[L: Leg](cls: RetryClass, at: FaultRow[L], on: Option[Dependency]) -> dict[str, str]:
    # both envelope spans stamp ONE attribute fold, so the sync mirror cannot drift a key from the async arm. The
    # peer key OMITS where a class keys no window, per `runtime/RULINGS.md` — an empty-string value names a series a
    # board groups on and nobody fills.
    return {"rasm.retry_class": cls.value, "rasm.subject": at.subject} | on.map(lambda dep: {"rasm.peer": dep.instance}).default_value({})


def _keyed(cls: RetryClass, peer: Option[str]) -> RuntimeRail[Option[Dependency]]:
    # ONE gate over the window key, and the only place the instance axis is decided. A class declaring EITHER a
    # `CIRCUIT` or a `RATES` row faces a peer with state, so its caller states WHICH peer and an unstated one refuses
    # `config` — the caller repairs it deterministically by naming its own destination, and the refusal is what makes
    # the per-INSTANCE keying law true by construction rather than by call-site discipline. A class declaring neither
    # row reaches nothing stateful, so `Nothing` IS its key and both stages no-op on it exactly as they already
    # no-op on an absent policy row. A peer stated for such a class is kept, never refused: the day its `CIRCUIT` row
    # lands, the window is already correctly keyed instead of every call site breaking at once.
    match peer:
        case Option(tag="some", some=named):
            return Ok(Some(Dependency(retry=cls, instance=named)))
        case _ if cls.circuit.is_some() or cls.rate.is_some():
            return Error(WINDOW_UNKEYED.raised(cls.value))
        case _:
            return Ok(Nothing)


def _settled[L: Leg](at: FaultRow[L], on: Option[Dependency], cls: RetryClass, raised: BaseException) -> Option[BoundaryFault]:
    # ONE settle over one verdict read, driven by both metered arms: the arc counts, the bucket re-seats or clears, and
    # a routed refusal answers the fault that ENDS the schedule. `Some` therefore means "stop and hand this back",
    # `Nothing` means "re-raise and let the loop decide", so neither arm re-derives the row and the sync mirror cannot
    # drift from the async one.
    subject = at.subject
    verdict = cls.policy.verdict(raised)
    Breaker.failed(on, verdict.recovery)
    match verdict:
        case Verdict(route=Reoffer(tag="rescope", rescope=leg)):
            # a refusal the row hands a NARROWER re-offer never re-drives: the routed fault rides back as this
            # attempt's settled outcome, so the caller reads the leg it must take instead. A re-raise would surface
            # the same refusal as a boundary fault whose detail names the provider class and never the alternative.
            return Some(RESCOPED.raised(cls.value, subject, leg))
        case Verdict(recovery=Recovery(tag="throttled", throttled=window)):
            # THE re-seat producer: the peer's own stated window becomes this destination's operating rate, so the
            # NEXT dial is paced by what the peer answered rather than by the floor its row keeps. `stamina` waits
            # exactly this window on this attempt and that same span refills one permit at the re-seated rate, so the
            # two bounds compose instead of charging the caller for the wait twice.
            RateGate.directed(on, window=window)
        case Verdict(recovery=Recovery(tag="transient"), route=Reoffer(tag="restart")):
            # a class whose re-offer RE-ESTABLISHES the handle pays no pacing debt across the re-dial: the level this
            # bucket holds was earned by a connection the next attempt no longer has, so the fresh handle meters from
            # the row's own floor rather than queueing behind a dead one's debits.
            RateGate.retired(on)
        case _:
            pass
    return Nothing


async def _metered[T, L: Leg](
    at: FaultRow[L], on: Option[Dependency], cls: RetryClass, fn: Callable[..., Awaitable[T]], args: tuple[object, ...],
    kwargs: dict[str, object],
) -> RuntimeRail[T]:
    # metered and BREAKER-SETTLED unit is one ATTEMPT, not one call: the bucket debits per dial so a retry storm
    # queues behind the same permits a first attempt spends, and the arc reads the LIVE raise here rather than the
    # lifted fault a rail carries, since the fault has already lost the exception the class's own target classifies.
    await anyio.sleep(RateGate.delay(on))
    try:  # Exemption: settle seam — every raise but a routed refusal re-raises untouched so the loop and fence own it
        held = await fn(*args, **kwargs)
    except BaseException as raised:
        match _settled(at, on, cls, raised):
            case Option(tag="some", some=routed):
                return Error(routed)
            case _:
                raise
    Breaker.passed(on)
    return Ok(held)


def _metered_sync[T, L: Leg](
    at: FaultRow[L], on: Option[Dependency], cls: RetryClass, fn: Callable[..., T], args: tuple[object, ...],
    kwargs: dict[str, object],
) -> RuntimeRail[T]:
    # sync mirror over the SAME bucket, the SAME arc, and the SAME settle: `delay` is a pure debit, so both arms read
    # one law and a thread-lane consumer never runs under a second schedule.
    time.sleep(RateGate.delay(on))
    try:  # Exemption: settle seam — every raise but a routed refusal re-raises untouched so the loop and fence own it
        held = fn(*args, **kwargs)
    except BaseException as raised:
        match _settled(at, on, cls, raised):
            case Option(tag="some", some=routed):
                return Error(routed)
            case _:
                raise
    Breaker.passed(on)
    return Ok(held)


async def guarded[T, L: Leg](
    cls: RetryClass, fn: Callable[..., Awaitable[T]], *args: object, at: FaultRow[L], on: Option[str] = Nothing, **kwargs: object
) -> RuntimeRail[T]:
    # ONE envelope, three data-driven stages in the only order that costs nothing twice: the breaker answers AHEAD of
    # every dial so an open circuit spends no connection and no permit, the rate gate meters each attempt from inside
    # that retried unit, and the terminal raise lifts through the faults boundary exactly once. A class carrying no
    # `CIRCUIT` or `RATES` row skips its stage by ABSENCE, so one call shape serves every consumer unchanged. TWO
    # coordinates ride, and they answer different questions: `at` is the caller's own rostered fence row and names
    # WHICH CALL raised — the span, the lifted fault, and a routed refusal all derive from it — while `on` names WHICH
    # PEER the call reached, and the stateful stages key on that alone. A stateful class dialed with no peer refuses
    # here rather than fusing every destination the row serves onto one arc. The lift narrows to the classes the
    # retry class itself discriminates, and the retried unit answers its own rail — a routed refusal settles it — so
    # the boundary that carried it flattens once.
    match _keyed(cls, on):
        case Result(tag="error") as unkeyed:
            return unkeyed
        case Result(tag="ok", ok=keyed):
            with _TRACER.start_as_current_span("resilience.guarded", attributes=_marks(cls, at, keyed)):
                match Breaker.refused(keyed):
                    case Option(tag="some", some=fault):
                        return Error(fault)
                    case _:
                        lifted = await async_boundary(
                            at, lambda: guard(cls)(_metered, at, keyed, cls, fn, args, kwargs), catch=cls.policy.target.catch
                        )
                        return lifted.bind(lambda rail: rail)


def guarded_sync[T, L: Leg](
    cls: RetryClass, fn: Callable[..., T], *args: object, at: FaultRow[L], on: Option[str] = Nothing, **kwargs: object
) -> RuntimeRail[T]:
    match _keyed(cls, on):
        case Result(tag="error") as unkeyed:
            return unkeyed
        case Result(tag="ok", ok=keyed):
            with _TRACER.start_as_current_span("resilience.guarded", attributes=_marks(cls, at, keyed)):
                match Breaker.refused(keyed):
                    case Option(tag="some", some=fault):
                        return Error(fault)
                    case _:
                        lifted = boundary(
                            at, lambda: guard_sync(cls)(_metered_sync, at, keyed, cls, fn, args, kwargs), catch=cls.policy.target.catch
                        )
                        return lifted.bind(lambda rail: rail)


def install(mode: RetryMode = RetryMode.EMIT) -> tuple[RetryHook, ...]:
    # returns the finalized hook tuple (factories executed) as the registration evidence.
    match mode:
        case RetryMode.EMIT:
            set_on_retry_hooks(RETRY_HOOKS)
        case RetryMode.SILENT:
            set_on_retry_hooks(())
        case RetryMode.TEST:
            stamina.set_testing(True)
            set_on_retry_hooks(())
        case _ as unreachable:
            assert_never(unreachable)
    return get_on_retry_hooks()


# --- [TABLES] ---------------------------------------------------------------------------

# the ONE refusal the breaker raises, seated as a ROW rather than the literal `BoundaryFault` construction the
# `reliability/faults#FAULT` raise-side roster retires: `resource` because an open arc names a dependency the caller
# cannot reach at all, and TRANSIENT because the cooldown this raise states as its own coordinate is exactly what
# clears it — a terminal posture here would tell a consumer the dependency never comes back.
CIRCUIT_OPEN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="circuit", arm="resource", defect="circuit-open", retriability=TRANSIENT,
    slots=("class", "peer", "cooldown"),
)

# the ONE refusal this envelope itself raises: a row's declared narrower leg, handed back as the attempt's settled
# outcome. `config` is its arm because the caller repairs it deterministically by re-offering on the named leg, and
# `TERMINAL` because re-driving the identical call only re-proves the ceiling that refused it. `Leg` is the CONTRACT
# the fault owner exports and `RuntimeLeg` this folder's own roster, so the anchor seats on the member and the
# envelopes stay parameterized over whichever roster the CALLER's row was minted under.
RESCOPED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="rescope", arm="config", defect="re-offer-narrowed", retriability=TERMINAL,
    slots=("class", "subject", "leg"),
)

# the window-key refusal: a class declaring a `CIRCUIT` or `RATES` row dialed with no peer named. `config` because
# the caller repairs it by naming its own destination, and TERMINAL because the identical unkeyed call refuses again.
WINDOW_UNKEYED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="window", arm="config", defect="peer-unstated", retriability=TERMINAL, slots=("class",)
)

# every refusal this module raises, pushed through the faults-owned seat DOOR at its own module scope: `retriability`
# and `facts` resolve a fault's declared posture and emitting leg by SUBJECT off the ONE census, so a row seated only
# in a local map is unreachable from both. `runtime` is S0 and the fault tier cannot pull this table in, so it pushes.
RAISES: Final[Block[FaultRow[RuntimeLeg]]] = rostered(Block.of_seq([CIRCUIT_OPEN, RESCOPED, WINDOW_UNKEYED]))

# keyed by the `RetryClass` member itself (a `.value` string key is the deleted spelling); a new class is one member plus one row.
POLICY: Final[Map[RetryClass, Policy]] = Map.of_seq([
    # obstore transients raise as subclasses of `obstore.exceptions.BaseError` — MRO-matched.
    (
        RetryClass.OBJECT_STORE,
        Policy(
            attempts=4,
            timeout=30.0,
            target=_backoff(
                "obstore.exceptions.BaseError",
                TimeoutError,
                # transport/roots lifts provider NotFound onto its typed StoreFault before this envelope sees it;
                # absence is terminal and must cross whole, never burn four attempts or collapse into boundary text.
                refuse=("rasm.runtime.roots.StoreFault",),
            ),
        ),
    ),
    # the origin's own `Retry-After` outranks the roster and rides back as the window `stamina` waits and the bucket re-seats on.
    (RetryClass.HTTP, Policy(attempts=3, timeout=20.0, target=_backoff(TimeoutError, ConnectionError, probe=Some(_windowed)))),
    # asyncssh transients (`ConnectionLost`/`DisconnectError`) subclass `asyncssh.Error`, never builtin `ConnectionError` —
    # name-matched so the row catches both families with no transport import; `refuse` pins the auth/host-key subclasses of
    # DisconnectError terminal, and `wait_initial` opens for the channel re-dial.
    (
        RetryClass.SSH,
        Policy(
            attempts=3,
            timeout=30.0,
            target=_backoff(
                "asyncssh.misc.ConnectionLost", "asyncssh.misc.DisconnectError", ConnectionError, TimeoutError,
                refuse=("asyncssh.misc.HostKeyNotVerifiable", "asyncssh.misc.PermissionDenied"),
            ),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
    # the serve page's dial fence owns the terminal `ConnectError` lift so the typed `FaultDetail` detail survives.
    (RetryClass.WIRE, Policy(attempts=5, timeout=15.0, target=_backoff(ConnectionError, probe=Some(_coded)))),
    (RetryClass.SCAN, Policy(attempts=2, timeout=60.0, target=_backoff(OSError), wait_max=30.0)),
    # secret-derivation band: the keystore lock and file-mount hiccups match by class, while the three cloud
    # providers' transport transients match by dotted spelling — NONE of them subclasses OSError (probed: the
    # google, hvac, and azure families all stop at Exception), so a class-only target left every cloud transient
    # failing on its first RPC error while three catalogs read as retried. Each family's rows are enumerated at the
    # narrowest transient arm, never at a shared base: hvac's taxonomy is FLAT (every status derives straight from
    # `VaultError`, so no base exists to name), and google's `ServerError` base would drag `MethodNotImplemented`
    # and `DataLoss` — neither of which a retry window clears — into the target with it. `TooManyRequests` carries
    # google's throttle arm and `ResourceExhausted` with it through the MRO; the azure throttle is NOT here, since
    # azure-core spells a 429 as a bare `HttpResponseError` carrying `status_code` and no name to match. The azure
    # timeout arms need no row either — `ServiceRequestTimeoutError`/`ServiceResponseTimeoutError` subclass the two
    # spelled here. Permanent file-tier refusals a retry cannot clear refuse rather than burn attempts;
    # CredentialUnavailableError stays OUT — absent material never heals inside a retry window.
    (
        RetryClass.SECRET,
        Policy(
            attempts=3,
            timeout=10.0,
            target=_backoff(
                KeyringLocked,
                OSError,
                "google.api_core.exceptions.ServiceUnavailable",
                "google.api_core.exceptions.DeadlineExceeded",
                "google.api_core.exceptions.InternalServerError",
                "google.api_core.exceptions.TooManyRequests",
                "hvac.exceptions.VaultDown",
                "hvac.exceptions.BadGateway",
                "hvac.exceptions.InternalServerError",
                "hvac.exceptions.RateLimitExceeded",
                "azure.core.exceptions.ServiceRequestError",
                "azure.core.exceptions.ServiceResponseError",
                refuse=(PermissionError, IsADirectoryError, NotADirectoryError),
            ),
            wait_initial=0.2,
        ),
    ),
    # engine prechecks: a transiently-locked config folder retries tightly, a missing engine exhausts fast.
    (RetryClass.ENGINE, Policy(attempts=2, timeout=10.0, target=_backoff(OSError, TimeoutError))),
    # flaky external-oracle subprocess verdicts, name-matched so no subprocess import rides this tier.
    (RetryClass.ORACLE, Policy(attempts=2, timeout=30.0, target=_backoff("subprocess.CalledProcessError", TimeoutError, OSError))),
    # subinterpreter/process worker-death band; the re-offer RE-SPAWNS and `wait_initial` opens wide for it.
    (
        RetryClass.OCCT,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_backoff(anyio.BrokenWorkerInterpreter, anyio.BrokenWorkerProcess),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
    # in-process transient OCC `RuntimeError` band; tight attempts so a genuinely broken kernel surfaces fast.
    (RetryClass.OCC_NATIVE, Policy(attempts=2, timeout=20.0, target=_backoff(RuntimeError))),
    # pool-executor worker-death band (loky respawn, pebble expiry), name-matched so no executor import rides this
    # BASE tier; `wait_initial` opens wide for the whole-pool respawn a TerminatedWorkerError implies.
    (
        RetryClass.WORKER,
        Policy(
            attempts=3,
            timeout=120.0,
            # loky's TerminatedWorkerError subclasses the stdlib BrokenProcessPool, so the stdlib spelling covers both names via the MRO
            target=_backoff(
                "loky.process_executor.TerminatedWorkerError",
                "concurrent.futures.process.BrokenProcessPool",
                "pebble.common.types.ProcessExpired",
            ),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
    # daemon child spawn/exec transients — the supervisor respawn band; pool deaths never ride it, and the permanent
    # spawn failures a re-exec cannot clear (missing binary, refused permission, bad path) refuse rather than retry.
    (
        RetryClass.SPAWN,
        Policy(
            attempts=3,
            timeout=30.0,
            target=_backoff(OSError, refuse=(FileNotFoundError, PermissionError, NotADirectoryError)),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
    # compas RPC bring-up, name-matched (compas is gated dark); the long timeout covers the ping-loop bring-up.
    (
        RetryClass.RPC,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_backoff("compas.rpc.errors.RPCServerError", "compas.rpc.errors.RPCClientError"),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
    # lakehouse commit-conflict transients, name-matched; `wait_initial` lets a competing commit land before the re-read.
    (
        RetryClass.LAKE_COMMIT,
        Policy(
            attempts=4,
            timeout=60.0,
            # deltalake's CommitFailedError is a pyo3 class whose __module__ is the Rust "_internal" module — its verified spelling
            target=_backoff(
                "_internal.CommitFailedError",
                "pyiceberg.exceptions.CommitFailedException",
                "pyiceberg.exceptions.CommitStateUnknownException",
            ),
            wait_initial=0.2,
        ),
    ),
    # ADBC/Flight SQL transport stalls, status-discriminated so a permanent driver fault never retries.
    (
        RetryClass.REMOTE_DB,
        Policy(attempts=3, timeout=30.0, target=_backoff(ConnectionError, TimeoutError, probe=Some(_statused)), wait_initial=0.2),
    ),
    # daft's Rust-backed transient base plus the stdlib pair; `wait_max` widens for a long scan without inflating attempts.
    (
        RetryClass.STREAMING,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_backoff("daft.exceptions.DaftTransientError", TimeoutError, ConnectionError),
            wait_max=30.0,
        ),
    ),
    # ONE broker band across every `transport/binding#BINDING` row: Kafka rides its own `retriable()` verdict while
    # pika, nats, and paho match by dotted spelling at their narrowest transient arms. The re-offer RE-DIALS, so the
    # lane's connection re-establishment is this row's declared route rather than a fact its own page carries alone.
    # `refuse` pins what a re-dial never clears — every authentication and wrong-state connection subclass, a subject
    # or bucket name the server rejected, and an already-acknowledged message — while the payload ceiling takes the
    # `rescope` seat instead: terminal for THIS call, satisfiable on the binding row's `dataref` leg.
    (
        RetryClass.BROKER,
        Policy(
            attempts=4,
            timeout=45.0,
            target=_backoff(
                "pika.exceptions.AMQPConnectionError",
                "nats.errors.ConnectionReconnectingError",
                "nats.errors.StaleConnectionError",
                "nats.errors.NoServersError",
                "nats.errors.OutboundBufferLimitError",
                "nats.errors.FlushTimeoutError",
                "nats.js.errors.ServiceUnavailableError",
                "nats.js.errors.NoStreamResponseError",
                ConnectionError,
                TimeoutError,
                OSError,
                refuse=(
                    "pika.exceptions.AuthenticationError",
                    "pika.exceptions.ProbableAuthenticationError",
                    "pika.exceptions.ProbableAccessDeniedError",
                    "pika.exceptions.ConnectionWrongStateError",
                    "pika.exceptions.IncompatibleProtocolError",
                    "nats.errors.BadSubjectError",
                    "nats.errors.AuthorizationError",
                    "nats.errors.MsgAlreadyAckdError",
                    "nats.js.errors.BadRequestError",
                    "nats.js.errors.NotFoundError",
                    "paho.mqtt.MQTTException",
                    PermissionError,
                    FileNotFoundError,
                ),
                rescope=(("nats.errors.MaxPayloadError", "dataref"),),
                probe=Some(_verdicted),
            ),
            route=RESTART,
            wait_initial=0.25,
            wait_max=20.0,
        ),
    ),
])

# breaker rows reach only the classes whose dependency can go DOWN as a whole rather than fail one call: a broker
# cluster, an HTTP origin, a companion channel, a remote database, an object store. A class with no row is unbroken,
# so absence is the declaration and no envelope reads a flag. `trips` stays low for the classes a caller re-drives
# cheaply and higher where a false open costs more than the storm it prevents.
CIRCUIT: Final[Map[RetryClass, CircuitPolicy]] = Map.of_seq([
    (RetryClass.BROKER, CircuitPolicy(trips=5, cooldown=15.0)),
    (RetryClass.HTTP, CircuitPolicy(trips=5, cooldown=10.0)),
    (RetryClass.OBJECT_STORE, CircuitPolicy(trips=8, cooldown=10.0)),
    (RetryClass.WIRE, CircuitPolicy(trips=5, cooldown=5.0)),
    (RetryClass.REMOTE_DB, CircuitPolicy(trips=5, cooldown=10.0)),
    (RetryClass.SSH, CircuitPolicy(trips=3, cooldown=30.0)),
])

# rate rows reach the classes whose peer publishes pacing evidence or whose local floor bounds admission. Broker
# `ThrottleEvent` and HTTP `Retry-After` re-seat through `directed`; a silent destination keeps its row floor.
RATES: Final[Map[RetryClass, RatePolicy]] = Map.of_seq([
    (RetryClass.BROKER, RatePolicy(permits=2000.0, burst=4000.0)),
    (RetryClass.HTTP, RatePolicy(permits=50.0, burst=100.0)),
    (RetryClass.SECRET, RatePolicy(permits=20.0, burst=40.0)),
])


# --- [COMPOSITION] ----------------------------------------------------------------------

# lazy build per the RetryHookFactory contract.
RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)

# receipt+span, the metrics-owned rasm.retry.attempts counter, and the structlog warning — one RetryDetails payload.
RETRY_HOOKS: Final[tuple[RetryHook | RetryHookFactory, ...]] = (RetryReceiptHook, Metrics.retry_hook(), StructlogOnRetryHook)
```

## [03]-[CIRCUIT]

- Owner: `Breaker` holds one `_Arc` per `Dependency` — the class names the dependency KIND and `instance` the PEER that can go down, so a broker outage never darkens the object store, two buckets behind one store row keep two arcs, and one origin dialed by two fences keeps one. Every member takes the optional key and answers its own no-op on absence, so an unkeyed class and an unrowed one read one law. `CircuitPolicy` is the window as row data: `trips` consecutive transient terminals open the arc, `cooldown` seconds later exactly `probes` attempts cross, and one success closes it whole.
- Cases: `BreakerState` is `CLOSED | OPEN | HALF_OPEN` and the three arms are total. `CLOSED` counts consecutive terminals and resets on any success, so an intermittent peer never accumulates its way open. `OPEN` refuses without dialing until the cooldown elapses, then leases exactly `probes` crossings. `HALF_OPEN` admits its leased probes and closes on the first success or re-opens on the first terminal, so the recovering peer meets one caller rather than the concurrency that felled it.
- Law: the arc trips on TRANSIENCE alone, read off the `Recovery` the class's own row already answered and never a second predicate — a malformed payload, a refused classification, and an unroutable key are terminal at the first attempt and prove nothing about the dependency, so counting them opens a circuit on a peer that never went down and sheds every healthy caller of it. `guard` already burned its attempts against exactly this verdict, which is what makes one open arc mean the dependency is out rather than that one caller sent something wrong.
- Law: a THROTTLED verdict counts nothing. A peer that stated a window ANSWERED — it is up and pacing its callers — so counting its directive toward the trip opens a circuit on a healthy dependency and sheds the very callers it just admitted; the window re-seats that destination's rate at the metered arm instead, which is the whole difference between a dependency that is DOWN and one that is BUSY.
- Law: state settles from inside the retried unit where the RAISE is live. Lifting to `BoundaryFault` surrenders the exception the target classifies, so a breaker reading the rail instead of the raise cannot tell a transient exhaustion from a first-attempt refusal and counts every terminal fault toward the trip.
- Law: no timer, no background sweep, and no expiry task — the arc re-reads `time.monotonic()` on each read, so an arc nobody consults costs nothing and a process that stops calling a dependency leaves no work behind. Cooldown is a monotonic span rather than a wall instant, so a host clock step never re-opens or prematurely closes a live arc.
- Entry: `refused(on)` is the pre-flight read every `guarded`/`guarded_sync` call folds ahead of its dial, answering `Some(fault)` off the `CIRCUIT_OPEN` row where the circuit refuses and `Nothing` where the attempt may cross; `passed` and `failed` are the settle pair `_settled` drives, `failed` taking the `Recovery` already read rather than re-deriving one from the raise. `state(on)` is the operator read the bundle capsule projects, and `retired(on)` drops one arc where a composition releases the peer it named.
- Auto: every transition emits its own `Receipt.of` row under the receipts-owned `OPEN` keep-all beside one `rasm.circuit.transitions` count keyed on the subject and the state reached, so an open circuit is evidence a board reads rather than a silent shed a caller infers from latency. Refusals carry the subject and the remaining cooldown on the fault, so an operator reads WHICH dependency is out and for how long.
- Growth: a new guarded dependency class is one `CIRCUIT` row and no envelope edit, and the row is what turns that class's unkeyed dials into refusals — a loud break at the call rather than a silent fan of callers sharing one arc; a new state is one `BreakerState` member with its arm on the one `state` fold, the standing `assert_never` breaking every arm that lacks it; a new transition dimension is one key on the emitted row; a new breaker refusal is one `FaultRow` beside `CIRCUIT_OPEN`.
- Boundary: failure windows over a named dependency only. Mints no retry curve, no rate, no receipt semantics, and no health verdict — the serve health flip stays `execution/workers#POOL`'s, and an arc is per-process state a peer never observes.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------


class BreakerState(StrEnum):
    CLOSED = "closed"
    OPEN = "open"
    HALF_OPEN = "half-open"


# --- [MODELS] ---------------------------------------------------------------------------


class _Arc(Struct, frozen=True, gc=False):
    # one dependency instance's live arc: consecutive transient terminals, the MONOTONIC instant the circuit opened,
    # and the probes a half-open window still owes. Frozen, so a transition returns a successor and two threads never
    # observe a half-written arc through the map they share.
    trips: int = 0
    opened_at: float = 0.0
    leased: int = 0


# --- [SERVICES] -------------------------------------------------------------------------


class Breaker:
    # one gate serializes each arc's read-modify-write; nothing else runs under it, so a held lock never spans a dial.
    # EVERY member takes the optional window key and answers its own no-op on absence, so one law — absence IS the
    # pass-through — covers a class that keys no window and a class whose `CIRCUIT` row is simply not rowed, and no
    # caller branches on either. `Dependency` is the whole key: a frozen `Struct` hashes by value, so the pair the
    # map holds carries its own meaning instead of a positional tuple two members could order differently.
    _arcs: ClassVar[dict[Dependency, _Arc]] = {}
    _gate = Lock()

    @classmethod
    def refused(cls, on: Option[Dependency], /) -> Option[BoundaryFault]:
        # ONE pre-flight read, and the only place a call is refused without touching the dependency. One `bind`
        # collapses BOTH absences — an unkeyed class and an unrowed one — into the single arm that answers `Nothing`,
        # so no envelope branches on either. Named for what it ANSWERS, so the arc's own refusal and `Policy.verdict`'s
        # two-axis read of a raise never wear one word for two concepts.
        match on.bind(lambda dep: dep.retry.circuit.map(lambda policy: (dep, policy))):
            case Option(tag="some", some=(dep, policy)):
                with cls._gate:
                    return cls._leased(dep, policy, time.monotonic())
            case _:
                return Nothing

    @classmethod
    def _leased(cls, on: Dependency, policy: CircuitPolicy, now: float) -> Option[BoundaryFault]:
        arc = cls._arcs.get(on, _Arc())
        if arc.trips < policy.trips:
            return Nothing
        remaining = policy.cooldown - (now - arc.opened_at)
        if remaining > 0.0 and arc.leased >= policy.probes:
            return Some(CIRCUIT_OPEN.raised(on.retry.value, on.instance, f"{remaining:.1f}s"))
        # cooldown elapsed, or a probe lease is still free: hand out exactly one crossing and record it, so the
        # recovering peer meets `probes` callers and never the whole fan that was queued behind the open arc.
        cls._arcs[on] = _Arc(trips=arc.trips, opened_at=arc.opened_at, leased=arc.leased + 1)
        return Nothing

    @classmethod
    def passed(cls, on: Option[Dependency], /) -> None:
        # one success closes the arc WHOLE rather than decrementing: a half-open probe that answered proves the
        # dependency is serving, and a decrement leaves a peer that recovered still one failure from re-opening.
        match on:
            case Option(tag="some", some=dep):
                with cls._gate:
                    if cls._arcs.pop(dep, None) is not None:
                        cls._transitioned(dep, BreakerState.CLOSED)
            case _:
                return

    @classmethod
    def failed(cls, on: Option[Dependency], recovery: Recovery, /) -> None:
        # THREE states settle here, not two: a TERMINAL proves nothing about the dependency and never counts, a
        # THROTTLED peer answered and is up — its window re-seats the rate at the metered arm and opens no circuit —
        # and only a TRANSIENT counts toward the trip. The verdict arrives READ rather than re-derived, so the
        # predicate the retry loop burned attempts against is the predicate the arc trips on, by construction.
        match (on.bind(lambda dep: dep.retry.circuit.map(lambda policy: (dep, policy))), recovery):
            case (Option(tag="none"), _) | (_, Recovery(tag="terminal") | Recovery(tag="throttled")):
                return
            case (Option(some=(dep, policy)), _):
                with cls._gate:
                    arc = cls._arcs.get(dep, _Arc())
                    trips = arc.trips + 1
                    cls._arcs[dep] = _Arc(trips=trips, opened_at=time.monotonic(), leased=0)
                    if trips == policy.trips:
                        cls._transitioned(dep, BreakerState.OPEN)

    @classmethod
    def state(cls, on: Option[Dependency], /) -> BreakerState:
        # operator read, derived from the arc rather than stored beside it: a stored state and a counted arc are two
        # facts one transition has to keep agreeing, and the derivation cannot disagree with itself. An unkeyed or
        # unrowed dependency is CLOSED because it has no window to be open in, which is the same answer the pre-flight
        # read gives it.
        match on.bind(lambda dep: dep.retry.circuit.map(lambda policy: (cls._arcs.get(dep), policy))):
            case Option(tag="some", some=(_Arc() as live, row)) if live.trips >= row.trips:
                return BreakerState.OPEN if time.monotonic() - live.opened_at < row.cooldown else BreakerState.HALF_OPEN
            case _:
                return BreakerState.CLOSED

    @classmethod
    def retired(cls, on: Option[Dependency], /) -> None:
        match on:
            case Option(tag="some", some=dep):
                with cls._gate:
                    cls._arcs.pop(dep, None)
            case _:
                return

    @staticmethod
    def _transitioned(on: Dependency, state: BreakerState) -> None:
        # transitions alone publish, never the steady state: an arc that never trips emits nothing and a board reads
        # its edges. Receipt and count carry ONE payload, so the log line and the series cannot disagree, and both
        # key the PEER rather than the fence, so a board reads WHICH destination went out.
        Signals.emit(
            Receipt.of("resilience", ("planned", f"circuit.{on.retry.value}", {"peer": on.instance, "state": state.value})), OPEN
        )
        Metrics.record(
            {"transitions": 1.0}, domain="circuit", kind=on.retry.value,
            dimensions={Dimension.TARGET: on.instance, Dimension.OUTCOME: state.value},
        )
```

## [04]-[RATE]

- Owner: `RateGate` holds one token bucket per `Dependency` beside `RATES`, the per-class default rate — the SAME key the breaker holds, so one destination's pacing and one destination's failure window can never name two different peers. A peer that states its own wait re-seats that bucket through `directed`, so the negotiated rate is the operating one and the row is the floor a destination negotiating nothing keeps.
- Cases: two peer directives state a wait this branch must honor — a broker throttle event carrying its window and an origin answering `Retry-After`. Both reach `directed` through ONE producer: the `throttled` verdict a class's own target answers, read once at the metered settle, so a stated window becomes pacing rather than per-protocol sleeps at call sites.
- Law: the directive arrives as the WINDOW the peer stated, and the steady rate it seats is that window's reciprocal — one dial per window — because a wait is what every one of the three producers actually publishes and a rate answer converts to it at the seam that decoded it. A peer stating NO wait re-seats the row's own floor, never an unbounded rate no ceiling then holds.
- Law: `delay` is a PURE debit answering seconds, and the caller performs its own wait — the async envelope through `anyio.sleep` and the sync mirror through `time.sleep`. One bucket therefore serves both arms with one law, and the wait is a cancellation checkpoint the caller's scope reaches rather than a block inside a provider call it cannot interrupt.
- Law: the gate WAITS and never refuses. Any ceiling here is a second refusal beside the deadline `execution/admission#CONTEXT` already carries, and two refusals over one queue disagree the moment either moves; the caller's own budget bounds the wait, and the waited seconds publish as evidence so a saturated bucket reads as a measured queue rather than a silent stall.
- Law: the bucket refills by ELAPSED monotonic time on read rather than by a ticking task, so an idle destination costs nothing, a burst banks exactly `burst` permits, and a host clock step never mints or destroys permits.
- Entry: `delay(on, permits=1.0)` is the one debit `_metered` and `_metered_sync` drive; `directed(on, window)` re-seats a bucket from the peer's own stated wait; `retired(on)` drops one bucket where a composition releases the destination it named, and is what a `restart`-routed re-offer spends so a re-established handle meters from the floor rather than a dead handle's debt.
- Auto: a debit that waited publishes its seconds on `rasm.rate.wait` keyed by the destination, so throttling is visible before it becomes latency nobody attributes. Zero waits publish nothing, because a series a healthy destination fills with zeros drowns the one reading that matters.
- Growth: a new rate-governed class is one `RATES` row and no call-site edit; a new peer directive is one `Probe` answering `throttled` on the class that decoded it, never a second `directed` call site; a new metered arm is one call to the same `delay`.
- Boundary: admission pacing over a named destination only. Mints no retry curve, no failure window, no queue, and no receipt semantics — a rate gate delays a caller and never sheds one.

```python signature
# --- [MODELS] ---------------------------------------------------------------------------


class _Bucket(Struct, frozen=True, gc=False):
    # a token bucket as a VALUE: `level` at `stamped`, refilled by elapsed monotonic time on read. Frozen, so a debit
    # returns a successor and two threads never observe a half-written level through the map they share.
    permits: float
    burst: float
    level: float
    stamped: float

    def debited(self, permits: float, now: float) -> tuple["_Bucket", float]:
        # refill FIRST, then debit, and let the level go negative: the shortfall divided by the rate IS the wait, so
        # one arithmetic answers both the queue position and the new level with no loop and no sleep-then-recheck.
        level = min(self.burst, self.level + (now - self.stamped) * self.permits) - permits
        return (_Bucket(self.permits, self.burst, level, now), 0.0 if level >= 0.0 else -level / self.permits)


# --- [SERVICES] -------------------------------------------------------------------------


class RateGate:
    # keyed on the same `Dependency` the breaker holds, so one destination's pacing and one destination's failure
    # window are the same peer by construction and a re-seat can never land on a bucket the arc does not name.
    _buckets: ClassVar[dict[Dependency, _Bucket]] = {}
    _gate = Lock()

    @classmethod
    def delay(cls, on: Option[Dependency], /, *, permits: float = 1.0) -> float:
        # an unkeyed class and a class with no `RATES` row alike spend nothing and wait nothing, so absence IS the
        # pass-through here exactly as it is at the breaker and no metered arm branches on a flag.
        match on.bind(lambda dep: dep.retry.rate.map(lambda policy: (dep, policy))):
            case Option(tag="some", some=(dep, policy)):
                now = time.monotonic()
                with cls._gate:
                    held = cls._buckets.get(dep, _Bucket(policy.permits, policy.burst, policy.burst, now))
                    cls._buckets[dep], waited = held.debited(permits, now)
                if waited > 0.0:
                    Metrics.record({"wait": waited * 1000.0}, domain="rate", kind=dep.retry.value, dimensions={Dimension.TARGET: dep.instance})
                return waited
            case _:
                return 0.0

    @classmethod
    def directed(cls, on: Option[Dependency], /, *, window: float) -> None:
        # a peer's own stated wait re-seats the STEADY rate and keeps the live level, so a directive slows the next
        # debit rather than refunding the permits already spent or stalling a caller the old rate already admitted.
        # The window IS the rate inverted — one dial per window — and a stated zero re-seats the row's floor rather
        # than the unbounded rate a reciprocal would mint there. An unkeyed class and one with no `RATES` row seat no
        # bucket at all, so absence stays the declaration here exactly as it is at the debit.
        match on.bind(lambda dep: dep.retry.rate.map(lambda policy: (dep, policy))):
            case Option(tag="some", some=(dep, policy)):
                permits = 1.0 / window if window > 0.0 else policy.permits
                with cls._gate:
                    match cls._buckets.get(dep):
                        case _Bucket() as held:
                            cls._buckets[dep] = _Bucket(permits, held.burst, held.level, held.stamped)
                        case None:
                            cls._buckets[dep] = _Bucket(permits, policy.burst, policy.burst, time.monotonic())
            case _:
                return

    @classmethod
    def retired(cls, on: Option[Dependency], /) -> None:
        match on:
            case Option(tag="some", some=dep):
                with cls._gate:
                    cls._buckets.pop(dep, None)
            case _:
                return
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

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

```python
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


# --- [TYPES] ----------------------------------------------------------------------------

type Probe = Callable[[Exception], Option[Recovery]]


@runtime_checkable
class RetryAfter(Protocol):
    retry_after: float | None


@runtime_checkable
class StatusCarrier(Protocol):
    status_code: Enum


@runtime_checkable
class StatusCoded(Protocol):
    code: Enum


@runtime_checkable
class Verdicted(Protocol):
    def retriable(self) -> bool: ...
    def fatal(self) -> bool: ...


@tagged_union(frozen=True)
class Reoffer:
    tag: Literal["wait", "restart", "rescope"] = tag()
    wait: None = case()
    restart: None = case()
    rescope: str = case()


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
        return POLICY[self]

    @property
    def circuit(self) -> Option["CircuitPolicy"]:
        return CIRCUIT.try_find(self)

    @property
    def rate(self) -> Option["RatePolicy"]:
        return RATES.try_find(self)


class RetryMode(StrEnum):
    EMIT = "emit"
    SILENT = "silent"
    TEST = "test"


# --- [CONSTANTS] ------------------------------------------------------------------------

_WAIT_COLUMNS: Final[tuple[str, ...]] = ("wait_initial", "wait_max", "wait_jitter", "wait_exp_base")
_WIRE_STATUS: Final[frozenset[str]] = frozenset({"UNAVAILABLE", "DEADLINE_EXCEEDED", "RESOURCE_EXHAUSTED"})
_ADBC_STATUS: Final[frozenset[str]] = frozenset({"TIMEOUT", "IO"})

WAIT: Final[Reoffer] = Reoffer(wait=None)
RESTART: Final[Reoffer] = Reoffer(restart=None)


# --- [MODELS] ---------------------------------------------------------------------------


class Backoff(Struct, frozen=True):
    wanted: tuple[type[Exception], ...]
    named: frozenset[str]
    denied: tuple[type[Exception], ...]
    refused: frozenset[str]
    rescopes: Map[str, str]
    probe: Option[Probe]

    def retriability(self, cause: BaseException, /) -> Recovery:
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
        return Block.of_seq(spelled(cause)).choose(self.rescopes.try_find).try_head().map(lambda leg: Reoffer(rescope=leg))

    def __call__(self, exc: Exception) -> bool | float | timedelta:
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
        return (Exception,) if self.named or self.refused or self.probe.is_some() else (*self.wanted, *self.denied)


class Verdict(Struct, frozen=True):
    recovery: Recovery
    route: Reoffer


class Policy(Struct, frozen=True):
    attempts: int
    timeout: float
    target: Backoff
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
    retry: RetryClass
    instance: str


class CircuitPolicy(Struct, frozen=True):
    trips: int
    cooldown: float
    probes: int = 1


class RatePolicy(Struct, frozen=True):
    permits: float
    burst: float


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, SCOPES[Scope.RESILIENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _windowed(exc: Exception) -> Option[Recovery]:
    match exc:
        case RetryAfter(retry_after=float() as seconds):
            return Some(Recovery(throttled=seconds))
        case _:
            return Nothing


def _statused(exc: Exception) -> Option[Recovery]:
    match exc:
        case StatusCarrier(status_code=Enum() as code) if type(exc).__module__.partition(".")[0] == "adbc_driver_manager":
            return Some(TRANSIENT if code.name in _ADBC_STATUS else TERMINAL)
        case _:
            return Nothing


def _coded(exc: Exception) -> Option[Recovery]:
    match exc:
        case StatusCoded(code=Enum() as code) if type(exc).__qualname__ == "ConnectError":
            return Some(TRANSIENT if code.name in _WIRE_STATUS else TERMINAL)
        case _:
            return Nothing


def _verdicted(exc: Exception) -> Option[Recovery]:
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


@cache
def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    row = cls.policy
    return stamina.AsyncRetryingCaller(**row.schedule).on(row.target)


@cache
def guard_sync(cls: RetryClass) -> stamina.BoundRetryingCaller:
    row = cls.policy
    return stamina.RetryingCaller(**row.schedule).on(row.target)


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    row = cls.policy
    return stamina.retry_context(on=row.target, **row.schedule)


def _marks[L: Leg](cls: RetryClass, at: FaultRow[L], on: Option[Dependency]) -> dict[str, str]:
    return {"rasm.retry_class": cls.value, "rasm.subject": at.subject} | on.map(lambda dep: {"rasm.peer": dep.instance}).default_value({})


def _keyed(cls: RetryClass, peer: Option[str]) -> RuntimeRail[Option[Dependency]]:
    match peer:
        case Option(tag="some", some=named):
            return Ok(Some(Dependency(retry=cls, instance=named)))
        case _ if cls.circuit.is_some() or cls.rate.is_some():
            return Error(WINDOW_UNKEYED.raised(cls.value))
        case _:
            return Ok(Nothing)


def _settled[L: Leg](at: FaultRow[L], on: Option[Dependency], cls: RetryClass, raised: BaseException) -> Option[BoundaryFault]:
    subject = at.subject
    verdict = cls.policy.verdict(raised)
    Breaker.failed(on, verdict.recovery)
    match verdict:
        case Verdict(route=Reoffer(tag="rescope", rescope=leg)):
            return Some(RESCOPED.raised(cls.value, subject, leg))
        case Verdict(recovery=Recovery(tag="throttled", throttled=window)):
            RateGate.directed(on, window=window)
        case Verdict(recovery=Recovery(tag="transient"), route=Reoffer(tag="restart")):
            RateGate.retired(on)
        case _:
            pass
    return Nothing


async def _metered[T, L: Leg](
    at: FaultRow[L], on: Option[Dependency], cls: RetryClass, fn: Callable[..., Awaitable[T]], args: tuple[object, ...],
    kwargs: dict[str, object],
) -> RuntimeRail[T]:
    await anyio.sleep(RateGate.delay(on))
    try:
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
    time.sleep(RateGate.delay(on))
    try:
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

CIRCUIT_OPEN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="circuit", arm="resource", defect="circuit-open", retriability=TRANSIENT,
    slots=("class", "peer", "cooldown"),
)

RESCOPED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="rescope", arm="config", defect="re-offer-narrowed", retriability=TERMINAL,
    slots=("class", "subject", "leg"),
)

WINDOW_UNKEYED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RESILIENCE, point="window", arm="config", defect="peer-unstated", retriability=TERMINAL, slots=("class",)
)

RAISES: Final[Block[FaultRow[RuntimeLeg]]] = rostered(Block.of_seq([CIRCUIT_OPEN, RESCOPED, WINDOW_UNKEYED]))

POLICY: Final[Map[RetryClass, Policy]] = Map.of_seq([
    (
        RetryClass.OBJECT_STORE,
        Policy(
            attempts=4,
            timeout=30.0,
            target=_backoff(
                "obstore.exceptions.BaseError",
                TimeoutError,
                refuse=("rasm.runtime.roots.StoreFault",),
            ),
        ),
    ),
    (RetryClass.HTTP, Policy(attempts=3, timeout=20.0, target=_backoff(TimeoutError, ConnectionError, probe=Some(_windowed)))),
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
    (RetryClass.WIRE, Policy(attempts=5, timeout=15.0, target=_backoff(ConnectionError, probe=Some(_coded)))),
    (RetryClass.SCAN, Policy(attempts=2, timeout=60.0, target=_backoff(OSError), wait_max=30.0)),
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
    (RetryClass.ENGINE, Policy(attempts=2, timeout=10.0, target=_backoff(OSError, TimeoutError))),
    (RetryClass.ORACLE, Policy(attempts=2, timeout=30.0, target=_backoff("subprocess.CalledProcessError", TimeoutError, OSError))),
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
    (RetryClass.OCC_NATIVE, Policy(attempts=2, timeout=20.0, target=_backoff(RuntimeError))),
    (
        RetryClass.WORKER,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_backoff(
                "loky.process_executor.TerminatedWorkerError",
                "concurrent.futures.process.BrokenProcessPool",
                "pebble.common.types.ProcessExpired",
            ),
            route=RESTART,
            wait_initial=0.5,
        ),
    ),
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
    (
        RetryClass.LAKE_COMMIT,
        Policy(
            attempts=4,
            timeout=60.0,
            target=_backoff(
                "_internal.CommitFailedError",
                "pyiceberg.exceptions.CommitFailedException",
                "pyiceberg.exceptions.CommitStateUnknownException",
            ),
            wait_initial=0.2,
        ),
    ),
    (
        RetryClass.REMOTE_DB,
        Policy(attempts=3, timeout=30.0, target=_backoff(ConnectionError, TimeoutError, probe=Some(_statused)), wait_initial=0.2),
    ),
    (
        RetryClass.STREAMING,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_backoff("daft.exceptions.DaftTransientError", TimeoutError, ConnectionError),
            wait_max=30.0,
        ),
    ),
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

CIRCUIT: Final[Map[RetryClass, CircuitPolicy]] = Map.of_seq([
    (RetryClass.BROKER, CircuitPolicy(trips=5, cooldown=15.0)),
    (RetryClass.HTTP, CircuitPolicy(trips=5, cooldown=10.0)),
    (RetryClass.OBJECT_STORE, CircuitPolicy(trips=8, cooldown=10.0)),
    (RetryClass.WIRE, CircuitPolicy(trips=5, cooldown=5.0)),
    (RetryClass.REMOTE_DB, CircuitPolicy(trips=5, cooldown=10.0)),
    (RetryClass.SSH, CircuitPolicy(trips=3, cooldown=30.0)),
])

RATES: Final[Map[RetryClass, RatePolicy]] = Map.of_seq([
    (RetryClass.BROKER, RatePolicy(permits=2000.0, burst=4000.0)),
    (RetryClass.HTTP, RatePolicy(permits=50.0, burst=100.0)),
    (RetryClass.SECRET, RatePolicy(permits=20.0, burst=40.0)),
])


# --- [COMPOSITION] ----------------------------------------------------------------------

RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)

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

```python
# --- [TYPES] ----------------------------------------------------------------------------


class BreakerState(StrEnum):
    CLOSED = "closed"
    OPEN = "open"
    HALF_OPEN = "half-open"


# --- [MODELS] ---------------------------------------------------------------------------


class _Arc(Struct, frozen=True, gc=False):
    trips: int = 0
    opened_at: float = 0.0
    leased: int = 0


# --- [SERVICES] -------------------------------------------------------------------------


class Breaker:
    _arcs: ClassVar[dict[Dependency, _Arc]] = {}
    _gate = Lock()

    @classmethod
    def refused(cls, on: Option[Dependency], /) -> Option[BoundaryFault]:
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
        cls._arcs[on] = _Arc(trips=arc.trips, opened_at=arc.opened_at, leased=arc.leased + 1)
        return Nothing

    @classmethod
    def passed(cls, on: Option[Dependency], /) -> None:
        match on:
            case Option(tag="some", some=dep):
                with cls._gate:
                    if cls._arcs.pop(dep, None) is not None:
                        cls._transitioned(dep, BreakerState.CLOSED)
            case _:
                return

    @classmethod
    def failed(cls, on: Option[Dependency], recovery: Recovery, /) -> None:
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

```python
# --- [MODELS] ---------------------------------------------------------------------------


class _Bucket(Struct, frozen=True, gc=False):
    permits: float
    burst: float
    level: float
    stamped: float

    def debited(self, permits: float, now: float) -> tuple["_Bucket", float]:
        level = min(self.burst, self.level + (now - self.stamped) * self.permits) - permits
        return (_Bucket(self.permits, self.burst, level, now), 0.0 if level >= 0.0 else -level / self.permits)


# --- [SERVICES] -------------------------------------------------------------------------


class RateGate:
    _buckets: ClassVar[dict[Dependency, _Bucket]] = {}
    _gate = Lock()

    @classmethod
    def delay(cls, on: Option[Dependency], /, *, permits: float = 1.0) -> float:
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

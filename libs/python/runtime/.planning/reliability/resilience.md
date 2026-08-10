# [PY_RUNTIME_RESILIENCE]

One retry-policy table rules the whole branch: `RetryClass` is the single behavior-carrying `stamina`-backed `StrEnum` the fault, transport, lane, and concurrency clusters consume through `guard`/`guarded`/`retrying` only, each member binding one frozen `Policy` row — attempts, timeout, `RetryTarget`, and optional backoff columns as row data, never a flag the caller re-derives. This is a BASE-tier module importing no sibling-tier package: every provider-discriminated target is import-free by construction, matching a gated provider's raise structurally instead of importing its class.

`guarded`/`guarded_sync` are the primary consumer envelopes — failure window, admission pacing, the cached bound caller, and one retry span off the `reliability/faults#FAULT` `SCOPES[Scope.RESILIENCE]` row, terminal raise lifted through the faults boundary once. Each stage reaches a class through its own table row, so an absent `CIRCUIT` or `RATES` row skips that stage. Recoverability stays `BoundaryFault.recoverable` at the caller. `install(mode)` owns the one `set_on_retry_hooks` registration any second registration clobbers, and the DAG-proven `faults < receipts < metrics < resilience` order makes the module-load `Metrics.retry_hook()` bind eager.

## [01]-[INDEX]

- [02]-[RESILIENCE]: `RetryClass` policy table, the import-free backoff discriminators, the `guard`/`guarded`/`retrying` family over one row, and the installed retry-hook stack.
- [03]-[CIRCUIT]: `Breaker` — the per-dependency failure window, its three-state verdict, and the fail-fast arm `guarded` folds ahead of every dial.
- [04]-[RATE]: `RateGate` — the per-destination token bucket, its server-directed re-seat, and the waited-seconds evidence every metered attempt publishes.

## [02]-[RESILIENCE]

- Owner: `RetryClass`, `Policy`, and the hook stack per the fence. `guard`/`guard_sync` memoise through a module-level `@cache` rather than a `cached_property` — an `Enum` member carries no writable `__dict__` — and only the reusable bound caller is safe to cache, the one-shot `retry_context` rebuilding per call.
- Entry: `guarded`/`guarded_sync` own the whole circuit/rate/retry/span/lift chain for every fetch-shaped leg, so a budget-exhausted transient surfaces as the `boundary` case naming the final cause and a non-transient raise surfaces immediately. `guard(cls)` is the bare bound caller for the one consumer that already owns its span and fault rail — the `execution/lanes#LANE` `retried` admission row, where a second span and boundary lift doubles the lane's rail; `retrying(cls)` is the inline form for blocks the caller cannot pre-shape as a coroutine; `install(mode)` returns the finalized `get_on_retry_hooks()` tuple as typed registration evidence.
- Auto: `Policy.schedule` folds only the present columns, so a `stamina` default stands for an absent column; every `BackoffHook` arm returns `bool` or a server-directed delay, leaving the wait to the `stamina` schedule. No classified field rides a retry fact — the hook mints under the receipts-owned `OPEN` keep-all, never a local `Redaction` re-mint.
- Auto: `BROKER` reads the broker's OWN verdict rather than a transcribed code roster — `KafkaException.args[0]` is a `KafkaError` answering `retriable()` beside `fatal()` over more than a hundred codes librdkafka revises per release, so a name roster here goes stale one bump later while the verdict never does. Three families ship a taxonomy and no verdict member, so each matches by dotted spelling at its narrowest transient arm: `pika`'s connection root beside its four terminal authentication and wrong-state subclasses, `nats`'s reconnect, stale-connection, and buffer-limit arms beside the JetStream unavailability pair, and `paho`'s bare socket raises, since its own `MQTTException` names a property or packet misuse a re-dial never clears. `MaxPayloadError` refuses by name: a payload past the negotiated ceiling takes the binding row's `dataref` leg, and a retry only re-proves the ceiling.
- Boundary: no retry around a pure transform — `stamina` rides only flaky external oracles through this table. Exported contract is branch-consumer law: the entry family, every `POLICY` row, and the `RetryClass` vocabulary; narrowing the `OCCT` target below the `BrokenWorkerInterpreter | BrokenWorkerProcess` pair is a cross-folder break.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import time
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import AbstractContextManager
from datetime import timedelta
from enum import Enum, StrEnum
from functools import cache
from threading import Lock
from typing import ClassVar, Final, Protocol, assert_never, runtime_checkable

import anyio
import stamina
from expression import Error, Nothing, Option, Some
from expression.collections import Map
from keyring.errors import KeyringLocked
from msgspec import UNSET, Struct, UnsetType
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from stamina.instrumentation import RetryDetails, RetryHook, RetryHookFactory, StructlogOnRetryHook, get_on_retry_hooks, set_on_retry_hooks

from rasm.runtime.faults import SCOPES, BoundaryFault, RuntimeRail, Scope, async_boundary, boundary, scoped, spelled
from rasm.runtime.metrics import Dimension, Metrics
from rasm.runtime.receipts import OPEN, Receipt, Signals

# `Breaker` and `RateGate` are this module's [03]-[CIRCUIT] and [04]-[RATE] owners — one module, three regions.

# --- [TYPES] ----------------------------------------------------------------------------

# stamina's `on=` discriminator shape spelled locally: stamina exports no public alias for it
# (`stamina.typing` carries only `RetryDetails`/`RetryHook`; `stamina._core` is private).
type BackoffHook = Callable[[Exception], bool | float | timedelta]
type RetryTarget = type[Exception] | tuple[type[Exception], ...] | BackoffHook


@runtime_checkable
class RetryAfter(Protocol):
    retry_after: float | None


# structural slot of an adbc DBAPI error: `status_code` is the driver's status enum member.
@runtime_checkable
class StatusCarrier(Protocol):
    status_code: Enum


# structural slot of a gRPC `AioRpcError`: `code()` returns the `grpc.StatusCode` member.
@runtime_checkable
class StatusCoded(Protocol):
    def code(self) -> Enum: ...


# structural slot of a librdkafka `KafkaError`, which `KafkaException` carries at `args[0]`: the client answers the
# transience verdict itself over its whole code space, so no roster of broker codes is transcribed into this table.
@runtime_checkable
class Verdicted(Protocol):
    def retriable(self) -> bool: ...
    def fatal(self) -> bool: ...


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
# transient gRPC status trio the grpcio client-fault law names retriable.
_WIRE_STATUS: Final[frozenset[str]] = frozenset({"UNAVAILABLE", "DEADLINE_EXCEEDED", "RESOURCE_EXHAUSTED"})


# --- [MODELS] ---------------------------------------------------------------------------


class Policy(Struct, frozen=True):
    attempts: int
    timeout: float
    target: RetryTarget
    wait_initial: float | UnsetType = UNSET
    wait_max: float | UnsetType = UNSET
    wait_jitter: float | UnsetType = UNSET
    wait_exp_base: float | UnsetType = UNSET

    @property
    def schedule(self) -> dict[str, object]:
        base: dict[str, object] = {"attempts": self.attempts, "timeout": self.timeout}
        return base | {col: value for col in _WAIT_COLUMNS if (value := getattr(self, col)) is not UNSET}

    def transient(self, exc: BaseException, /) -> bool:
        # ONE transience read over both `target` spellings, so the breaker trips on exactly what the retry loop already
        # burned attempts against. Deriving a second predicate at the breaker opens a circuit on a malformed payload no
        # re-dial repairs, which sheds every healthy caller of a dependency that never went down.
        match self.target:
            case type() | tuple() as classes:
                return isinstance(exc, classes)
            case hook:
                return isinstance(exc, Exception) and bool(hook(exc))


class CircuitPolicy(Struct, frozen=True):
    # window per DEPENDENCY CLASS, applied per dependency INSTANCE: `trips` consecutive transient terminals open the
    # circuit, `cooldown` seconds later exactly `probes` attempts cross to test it, and one success closes it whole.
    # Budgeting a half-open window above one probe re-storms a recovering peer with the concurrency that felled it.
    trips: int
    cooldown: float
    probes: int = 1


class RatePolicy(Struct, frozen=True):
    # steady-state permits per second beside the burst a quiet window banks. `permits` is the DEFAULT a destination
    # opens at; a peer's own directive — a `WebHook-Allowed-Rate` answer, a librdkafka `ThrottleEvent`, a `Retry-After`
    # — re-seats it through `RateGate.directed`, so the negotiated rate is the operating one and this row is the floor
    # a destination that negotiates nothing keeps.
    permits: float
    burst: float


# --- [SERVICES] -------------------------------------------------------------------------

# minted from the faults-owned scope row off the proxy-until-install provider.
_TRACER: Final = scoped(trace.get_tracer, SCOPES[Scope.RESILIENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transient(*targets: type[Exception] | str, refuse: tuple[type[Exception] | str, ...] = ()) -> BackoffHook:
    # ONE polymorphic MRO-matching law over both target spellings, the value's own shape the discriminant: an importable
    # class (stdlib, BASE-tier dep) matches by isinstance, a gated provider's class by the faults-owned `spelled` set —
    # identical subclass semantics, zero provider imports, a bare-name collision unspellable, and one derivation shared
    # with the `CLASSIFY` frozenset rows rather than a second copy of the dotted-spelling convention. `refuse` rides the
    # same polymorphic axis and reads first, pinning the terminal subclasses a transient base would otherwise absorb
    # (asyncssh auth/host-key failures subclass DisconnectError; a missing binary subclasses OSError).
    wanted_types = tuple(t for t in targets if isinstance(t, type))
    wanted_names = frozenset(t for t in targets if isinstance(t, str))
    denied_types = tuple(t for t in refuse if isinstance(t, type))
    denied_names = frozenset(t for t in refuse if isinstance(t, str))

    def backoff(exc: Exception) -> bool:
        spellings = spelled(exc)
        denied = isinstance(exc, denied_types) or bool(spellings & denied_names)
        return not denied and (isinstance(exc, wanted_types) or bool(spellings & wanted_names))

    return backoff


def _retry_after(*targets: type[Exception] | str) -> BackoffHook:
    # server-directed delay overlay: a `RetryAfter` carrier returns its own wait; everything else folds to the one matching law.
    fallback = _transient(*targets)

    def backoff(exc: Exception) -> bool | float | timedelta:
        match exc:
            case RetryAfter(retry_after=float() as seconds):
                return seconds
            case _:
                return fallback(exc)

    return backoff


def _adbc_transient(*targets: type[Exception] | str) -> BackoffHook:
    # an adbc `OperationalError` retries ONLY when `status_code` names a transport transient (`TIMEOUT`/`IO` —
    # never `INVALID_ARGUMENT`/`NOT_FOUND` a re-issue cannot clear); every other raise folds to the one matching law.
    statuses = frozenset({"TIMEOUT", "IO"})
    fallback = _transient(*targets)

    def backoff(exc: Exception) -> bool:
        match exc:
            case StatusCarrier(status_code=Enum() as code) if type(exc).__module__.partition(".")[0] == "adbc_driver_manager":
                return code.name in statuses
            case _:
                return fallback(exc)

    return backoff


def _wire_transient(*targets: type[Exception] | str) -> BackoffHook:
    # an `AioRpcError` retries only on the transient status trio; every other raise folds to the one matching law.
    fallback = _transient(*targets)

    def backoff(exc: Exception) -> bool:
        match exc:
            case StatusCoded() as coded if type(exc).__qualname__ == "AioRpcError":
                return coded.code().name in _WIRE_STATUS
            case _:
                return fallback(exc)

    return backoff


def _broker_transient(*targets: type[Exception] | str, refuse: tuple[type[Exception] | str, ...] = ()) -> BackoffHook:
    # librdkafka answers its own verdict: `KafkaException.args[0]` is a `KafkaError` carrying `retriable()` beside
    # `fatal()` across a code space the client revises per release, so the verdict rides and no roster of broker codes
    # is transcribed here to go stale. The qualname guard is what keeps the structural match honest — a foreign
    # exception whose first argument happens to answer both members is not this client's error. `fatal()` reads FIRST
    # because a fenced producer and a poisoned transaction each report a code the retry column would otherwise burn
    # attempts against. Every other broker family falls through to the one matching law on its own spellings.
    fallback = _transient(*targets, refuse=refuse)

    def backoff(exc: Exception) -> bool:
        match exc.args:
            case (Verdicted() as error, *_) if type(error).__qualname__ == "KafkaError":
                return not error.fatal() and error.retriable()
            case _:
                return fallback(exc)

    return backoff


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


async def _metered[T](
    cls: RetryClass, subject: str, fn: Callable[..., Awaitable[T]], args: tuple[object, ...], kwargs: dict[str, object]
) -> T:
    # metered and BREAKER-SETTLED unit is one ATTEMPT, not one call: the bucket debits per dial so a retry storm
    # queues behind the same permits a first attempt spends, and the arc reads the LIVE raise here rather than the
    # lifted fault a rail carries, since the fault has already lost the exception the class's own target classifies.
    await anyio.sleep(RateGate.delay(cls, subject))
    try:  # Exemption: settle seam — the raise re-raises untouched so the retry loop and the fence still own it
        held = await fn(*args, **kwargs)
    except BaseException as raised:
        Breaker.failed(cls, subject, raised)
        raise
    Breaker.passed(cls, subject)
    return held


def _metered_sync[T](cls: RetryClass, subject: str, fn: Callable[..., T], args: tuple[object, ...], kwargs: dict[str, object]) -> T:
    # sync mirror over the SAME bucket and the SAME arc: `delay` is a pure debit, so both arms read one law and a
    # thread-lane consumer never runs under a second schedule.
    time.sleep(RateGate.delay(cls, subject))
    try:  # Exemption: settle seam — the raise re-raises untouched so the retry loop and the fence still own it
        held = fn(*args, **kwargs)
    except BaseException as raised:
        Breaker.failed(cls, subject, raised)
        raise
    Breaker.passed(cls, subject)
    return held


async def guarded[T](cls: RetryClass, fn: Callable[..., Awaitable[T]], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    # ONE envelope, three data-driven stages in the only order that costs nothing twice: the breaker answers AHEAD of
    # every dial so an open circuit spends no connection and no permit, the rate gate meters each attempt from inside
    # that retried unit, and the terminal raise lifts through the faults boundary exactly once. A class carrying no
    # `CIRCUIT` or `RATES` row skips its stage by ABSENCE, so one call shape serves every consumer unchanged.
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        match Breaker.verdict(cls, subject):
            case Option(tag="some", some=fault):
                return Error(fault)
            case _:
                return await async_boundary(subject, lambda: guard(cls)(_metered, cls, subject, fn, args, kwargs))


def guarded_sync[T](cls: RetryClass, fn: Callable[..., T], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        match Breaker.verdict(cls, subject):
            case Option(tag="some", some=fault):
                return Error(fault)
            case _:
                return boundary(subject, lambda: guard_sync(cls)(_metered_sync, cls, subject, fn, args, kwargs))


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

# keyed by the `RetryClass` member itself (a `.value` string key is the deleted spelling); a new class is one member plus one row.
POLICY: Final[Map[RetryClass, Policy]] = Map.of_seq([
    # obstore transients raise as subclasses of `obstore.exceptions.BaseError` — MRO-matched.
    (RetryClass.OBJECT_STORE, Policy(attempts=4, timeout=30.0, target=_transient("obstore.exceptions.BaseError", TimeoutError))),
    (RetryClass.HTTP, Policy(attempts=3, timeout=20.0, target=_retry_after(TimeoutError, ConnectionError))),
    # asyncssh transients (`ConnectionLost`/`DisconnectError`) subclass `asyncssh.Error`, never builtin `ConnectionError` —
    # name-matched so the row catches both families with no transport import; `refuse` pins the auth/host-key subclasses of
    # DisconnectError terminal, and `wait_initial` opens for the channel re-dial.
    (
        RetryClass.SSH,
        Policy(
            attempts=3,
            timeout=30.0,
            target=_transient(
                "asyncssh.misc.ConnectionLost", "asyncssh.misc.DisconnectError", ConnectionError, TimeoutError,
                refuse=("asyncssh.misc.HostKeyNotVerifiable", "asyncssh.misc.PermissionDenied"),
            ),
            wait_initial=0.5,
        ),
    ),
    # consumer trailer fence owns the terminal `AioRpcError` lift so the typed detail survives.
    (RetryClass.WIRE, Policy(attempts=5, timeout=15.0, target=_wire_transient(ConnectionError))),
    (RetryClass.SCAN, Policy(attempts=2, timeout=60.0, target=(OSError,), wait_max=30.0)),
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
            target=_transient(
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
    (RetryClass.ENGINE, Policy(attempts=2, timeout=10.0, target=(OSError, TimeoutError))),
    # flaky external-oracle subprocess verdicts, name-matched so no subprocess import rides this tier.
    (RetryClass.ORACLE, Policy(attempts=2, timeout=30.0, target=_transient("subprocess.CalledProcessError", TimeoutError, OSError))),
    # subinterpreter/process worker-death band; `wait_initial` opens wide for the re-spawn.
    (RetryClass.OCCT, Policy(attempts=3, timeout=120.0, target=(anyio.BrokenWorkerInterpreter, anyio.BrokenWorkerProcess), wait_initial=0.5)),
    # in-process transient OCC `RuntimeError` band; tight attempts so a genuinely broken kernel surfaces fast.
    (RetryClass.OCC_NATIVE, Policy(attempts=2, timeout=20.0, target=(RuntimeError,))),
    # pool-executor worker-death band (loky respawn, pebble expiry), name-matched so no executor import rides this
    # BASE tier; `wait_initial` opens wide for the whole-pool respawn a TerminatedWorkerError implies.
    (
        RetryClass.WORKER,
        Policy(
            attempts=3,
            timeout=120.0,
            # loky's TerminatedWorkerError subclasses the stdlib BrokenProcessPool, so the stdlib spelling covers both names via the MRO
            target=_transient(
                "loky.process_executor.TerminatedWorkerError",
                "concurrent.futures.process.BrokenProcessPool",
                "pebble.common.types.ProcessExpired",
            ),
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
            target=_transient(OSError, refuse=(FileNotFoundError, PermissionError, NotADirectoryError)),
            wait_initial=0.5,
        ),
    ),
    # compas RPC bring-up, name-matched (compas is gated dark); the long timeout covers the ping-loop bring-up.
    (
        RetryClass.RPC,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_transient("compas.rpc.errors.RPCServerError", "compas.rpc.errors.RPCClientError"),
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
            target=_transient(
                "_internal.CommitFailedError",
                "pyiceberg.exceptions.CommitFailedException",
                "pyiceberg.exceptions.CommitStateUnknownException",
            ),
            wait_initial=0.2,
        ),
    ),
    # ADBC/Flight SQL transport stalls, status-discriminated so a permanent driver fault never retries.
    (RetryClass.REMOTE_DB, Policy(attempts=3, timeout=30.0, target=_adbc_transient(ConnectionError, TimeoutError), wait_initial=0.2)),
    # daft's Rust-backed transient base plus the stdlib pair; `wait_max` widens for a long scan without inflating attempts.
    (
        RetryClass.STREAMING,
        Policy(
            attempts=3,
            timeout=120.0,
            target=_transient("daft.exceptions.DaftTransientError", TimeoutError, ConnectionError),
            wait_max=30.0,
        ),
    ),
    # ONE broker band across every `transport/binding#BINDING` row: Kafka rides its own `retriable()` verdict while
    # pika, nats, and paho match by dotted spelling at their narrowest transient arms. `refuse` pins what a re-dial
    # never clears — every authentication and wrong-state connection subclass, a subject or bucket name the server
    # rejected, an already-acknowledged message, and the payload ceiling whose answer is the row's `dataref` leg.
    (
        RetryClass.BROKER,
        Policy(
            attempts=4,
            timeout=45.0,
            target=_broker_transient(
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
                    "nats.errors.MaxPayloadError",
                    "nats.errors.BadSubjectError",
                    "nats.errors.AuthorizationError",
                    "nats.errors.MsgAlreadyAckdError",
                    "nats.js.errors.BadRequestError",
                    "nats.js.errors.NotFoundError",
                    "paho.mqtt.MQTTException",
                    PermissionError,
                    FileNotFoundError,
                ),
            ),
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

# rate rows reach the classes whose peer PUBLISHES a rate — a webhook target answering `WebHook-Allowed-Rate`, a
# broker raising `ThrottleEvent`, an origin answering `Retry-After` — so the floor here is what a destination
# negotiating nothing keeps and `directed` seats whatever it does negotiate. A class with no row spends no permits.
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

- Owner: `Breaker` holds one `_Arc` per `(RetryClass, subject)` pair — the class names the dependency KIND and the subject one dependency INSTANCE, so a dead Kafka cluster never sheds a healthy one and a broker outage never darkens the object store. `CircuitPolicy` is the window as row data: `trips` consecutive transient terminals open the arc, `cooldown` seconds later exactly `probes` attempts cross, and one success closes it whole.
- Cases: `BreakerState` is `CLOSED | OPEN | HALF_OPEN` and the three arms are total. `CLOSED` counts consecutive terminals and resets on any success, so an intermittent peer never accumulates its way open. `OPEN` refuses without dialing until the cooldown elapses, then leases exactly `probes` crossings. `HALF_OPEN` admits its leased probes and closes on the first success or re-opens on the first terminal, so the recovering peer meets one caller rather than the concurrency that felled it.
- Law: the arc trips on TRANSIENCE alone, read through the class's own `Policy.transient` and never a second predicate — a malformed payload, a refused classification, and an unroutable key are terminal at the first attempt and prove nothing about the dependency, so counting them opens a circuit on a peer that never went down and sheds every healthy caller of it. `guard` already burned its attempts against exactly this predicate, which is what makes one open arc mean the dependency is out rather than that one caller sent something wrong.
- Law: state settles from inside the retried unit where the RAISE is live. Lifting to `BoundaryFault` surrenders the exception the target classifies, so a breaker reading the rail instead of the raise cannot tell a transient exhaustion from a first-attempt refusal and counts every terminal fault toward the trip.
- Law: no timer, no background sweep, and no expiry task — the arc re-reads `time.monotonic()` on each verdict, so an arc nobody consults costs nothing and a process that stops calling a dependency leaves no work behind. Cooldown is a monotonic span rather than a wall instant, so a host clock step never re-opens or prematurely closes a live arc.
- Entry: `verdict(cls, subject)` is the pre-flight read every `guarded`/`guarded_sync` call folds ahead of its dial, answering `Some(fault)` where the circuit refuses and `Nothing` where the attempt may cross; `passed` and `failed` are the settle pair `_metered` drives. `state(cls, subject)` is the operator read the bundle capsule projects, and `retired(cls, subject)` drops one arc where a composition releases the dependency it named.
- Auto: every transition emits its own `Receipt.of` row under the receipts-owned `OPEN` keep-all beside one `rasm.circuit.transitions` count keyed on the subject and the state reached, so an open circuit is evidence a board reads rather than a silent shed a caller infers from latency. Refusals carry the subject and the remaining cooldown on the fault, so an operator reads WHICH dependency is out and for how long.
- Growth: a new guarded dependency class is one `CIRCUIT` row and no envelope edit; a new state is one `BreakerState` member with its arm on the one verdict fold, the standing `assert_never` breaking every arm that lacks it; a new transition dimension is one key on the emitted row.
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
    _arcs: ClassVar[dict[tuple[RetryClass, str], _Arc]] = {}
    _gate = Lock()

    @classmethod
    def verdict(cls, retry: RetryClass, subject: str, /) -> Option[BoundaryFault]:
        # ONE pre-flight read, and the only place a call is refused without touching the dependency. A class with
        # no `CIRCUIT` row answers `Nothing` unconditionally, so absence IS the pass-through and no envelope branches.
        match retry.circuit:
            case Option(tag="none"):
                return Nothing
            case Option(some=policy):
                with cls._gate:
                    return cls._leased(retry, subject, policy, time.monotonic())

    @classmethod
    def _leased(cls, retry: RetryClass, subject: str, policy: CircuitPolicy, now: float) -> Option[BoundaryFault]:
        arc = cls._arcs.get((retry, subject), _Arc())
        if arc.trips < policy.trips:
            return Nothing
        remaining = policy.cooldown - (now - arc.opened_at)
        if remaining > 0.0 and arc.leased >= policy.probes:
            return Some(BoundaryFault(boundary=(f"circuit.{retry.value}", f"{subject} is open for a further {remaining:.1f}s")))
        # cooldown elapsed, or a probe lease is still free: hand out exactly one crossing and record it, so the
        # recovering peer meets `probes` callers and never the whole fan that was queued behind the open arc.
        cls._arcs[(retry, subject)] = _Arc(trips=arc.trips, opened_at=arc.opened_at, leased=arc.leased + 1)
        return Nothing

    @classmethod
    def passed(cls, retry: RetryClass, subject: str, /) -> None:
        # one success closes the arc WHOLE rather than decrementing: a half-open probe that answered proves the
        # dependency is serving, and a decrement leaves a peer that recovered still one failure from re-opening.
        with cls._gate:
            if cls._arcs.pop((retry, subject), None) is not None:
                cls._transitioned(retry, subject, BreakerState.CLOSED)

    @classmethod
    def failed(cls, retry: RetryClass, subject: str, raised: BaseException, /) -> None:
        match retry.circuit:
            case Option(tag="none"):
                return
            case Option(some=policy) if not retry.policy.transient(raised):
                return
            case Option(some=policy):
                with cls._gate:
                    arc = cls._arcs.get((retry, subject), _Arc())
                    trips = arc.trips + 1
                    cls._arcs[(retry, subject)] = _Arc(trips=trips, opened_at=time.monotonic(), leased=0)
                    if trips == policy.trips:
                        cls._transitioned(retry, subject, BreakerState.OPEN)

    @classmethod
    def state(cls, retry: RetryClass, subject: str, /) -> BreakerState:
        # operator read, derived from the arc rather than stored beside it: a stored state and a counted arc are two
        # facts one transition has to keep agreeing, and the derivation cannot disagree with itself.
        arc, policy = cls._arcs.get((retry, subject)), retry.circuit
        match (arc, policy):
            case (None, _) | (_, Option(tag="none")):
                return BreakerState.CLOSED
            case (_Arc() as live, Option(some=row)) if live.trips < row.trips:
                return BreakerState.CLOSED
            case (_Arc() as live, Option(some=row)):
                return BreakerState.OPEN if time.monotonic() - live.opened_at < row.cooldown else BreakerState.HALF_OPEN

    @classmethod
    def retired(cls, retry: RetryClass, subject: str, /) -> None:
        with cls._gate:
            cls._arcs.pop((retry, subject), None)

    @staticmethod
    def _transitioned(retry: RetryClass, subject: str, state: BreakerState) -> None:
        # transitions alone publish, never the steady state: an arc that never trips emits nothing and a board reads
        # its edges. Receipt and count carry ONE payload, so the log line and the series cannot disagree.
        Signals.emit(Receipt.of("resilience", ("planned", f"circuit.{retry.value}", {"subject": subject, "state": state.value})), OPEN)
        Metrics.record(
            {"transitions": 1.0}, domain="circuit", kind=retry.value, dimensions={Dimension.TARGET: subject, Dimension.OUTCOME: state.value}
        )
```

## [04]-[RATE]

- Owner: `RateGate` holds one token bucket per `(RetryClass, subject)` pair beside `RATES`, the per-class default rate. Peers that PUBLISH their own rate re-seat that bucket through `directed`, so the negotiated rate is the operating one and the row is the floor a destination negotiating nothing keeps.
- Cases: three peers publish a rate this branch must honor — a webhook target answering `WebHook-Allowed-Rate` on the abuse-protection handshake, a broker raising a throttle event carrying its own throttle window, and an origin answering `Retry-After`. Each reaches one `directed` call, so the three arrive as data on one bucket rather than three per-protocol sleeps at three call sites.
- Law: `delay` is a PURE debit answering seconds, and the caller performs its own wait — the async envelope through `anyio.sleep` and the sync mirror through `time.sleep`. One bucket therefore serves both arms with one law, and the wait is a cancellation checkpoint the caller's scope reaches rather than a block inside a provider call it cannot interrupt.
- Law: the gate WAITS and never refuses. Any ceiling here is a second refusal beside the deadline `execution/admission#CONTEXT` already carries, and two refusals over one queue disagree the moment either moves; the caller's own budget bounds the wait, and the waited seconds publish as evidence so a saturated bucket reads as a measured queue rather than a silent stall.
- Law: the bucket refills by ELAPSED monotonic time on read rather than by a ticking task, so an idle destination costs nothing, a burst banks exactly `burst` permits, and a host clock step never mints or destroys permits.
- Entry: `delay(cls, subject, permits=1.0)` is the one debit `_metered` and `_metered_sync` drive; `directed(cls, subject, permits)` re-seats a bucket from a peer's own published rate; `retired(cls, subject)` drops one bucket where a composition releases the destination it named.
- Auto: a debit that waited publishes its seconds on `rasm.rate.wait` keyed by the destination, so throttling is visible before it becomes latency nobody attributes. Zero waits publish nothing, because a series a healthy destination fills with zeros drowns the one reading that matters.
- Growth: a new rate-governed class is one `RATES` row and no call-site edit; a new peer directive is one `directed` call at the surface that decoded it; a new metered arm is one call to the same `delay`.
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
    _buckets: ClassVar[dict[tuple[RetryClass, str], _Bucket]] = {}
    _gate = Lock()

    @classmethod
    def delay(cls, retry: RetryClass, subject: str, /, *, permits: float = 1.0) -> float:
        # a class with no `RATES` row spends nothing and waits nothing, so absence IS the pass-through here exactly as
        # it is at the breaker and no metered arm branches on a flag.
        match retry.rate:
            case Option(tag="none"):
                return 0.0
            case Option(some=policy):
                now = time.monotonic()
                with cls._gate:
                    held = cls._buckets.get((retry, subject), _Bucket(policy.permits, policy.burst, policy.burst, now))
                    cls._buckets[(retry, subject)], waited = held.debited(permits, now)
                if waited > 0.0:
                    Metrics.record({"wait": waited * 1000.0}, domain="rate", kind=retry.value, dimensions={Dimension.TARGET: subject})
                return waited

    @classmethod
    def directed(cls, retry: RetryClass, subject: str, /, *, permits: float) -> None:
        # a peer's own published rate re-seats the STEADY rate and keeps the live level, so a directive slows the next
        # debit rather than refunding the permits already spent or stalling a caller the old rate already admitted.
        with cls._gate:
            match cls._buckets.get((retry, subject)):
                case _Bucket() as held:
                    cls._buckets[(retry, subject)] = _Bucket(permits, held.burst, held.level, held.stamped)
                case None:
                    match retry.rate:
                        case Option(some=policy):
                            cls._buckets[(retry, subject)] = _Bucket(permits, policy.burst, policy.burst, time.monotonic())
                        case _:
                            pass

    @classmethod
    def retired(cls, retry: RetryClass, subject: str, /) -> None:
        with cls._gate:
            cls._buckets.pop((retry, subject), None)
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

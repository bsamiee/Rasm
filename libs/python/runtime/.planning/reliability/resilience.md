# [PY_RUNTIME_RESILIENCE]

One retry-policy table rules the whole branch: `RetryClass` is the single behavior-carrying `stamina`-backed `StrEnum` the fault, transport, lane, and concurrency clusters consume through `guard`/`guarded`/`retrying` only, each member binding one frozen `Policy` row — attempts, timeout, `RetryTarget`, and optional backoff columns as row data, never a flag the caller re-derives. This is a BASE-tier module importing no sibling-tier package: every provider-discriminated target is import-free by construction, matching a gated provider's raise structurally instead of importing its class.

`guarded`/`guarded_sync` are the primary consumer envelopes — the cached bound caller inside one retry span (minted from the `reliability/faults#FAULT` `SCOPES[Scope.RESILIENCE]` row) with the terminal raise lifted through the faults boundary exactly once — and a fetch-shaped leg re-spelling a bare caller inside a hand-opened span and boundary fence never lands. Recoverability over the resulting rail stays the faults owner's `BoundaryFault.recoverable` at the caller. `install(mode)` owns the one process-global `set_on_retry_hooks` registration — the hook table is process-global, so a second registration anywhere clobbers the stack — and the hook composition is DAG-proven `faults < receipts < metrics < resilience`, so the module-load `Metrics.retry_hook()` bind inside `RETRY_HOOKS` is eager against strictly-earlier modules.

## [01]-[INDEX]

- [01]-[RESILIENCE]: the `RetryClass` policy table, the import-free backoff discriminators, the `guard`/`guarded`/`retrying` family over one row, and the installed retry-hook stack.

## [02]-[RESILIENCE]

- Owner: `RetryClass`, `Policy`, and the hook stack per the fence. `guard`/`guard_sync` memoise through a module-level `@cache` rather than a `cached_property` — an `Enum` member carries no writable `__dict__` — and only the reusable bound caller is safe to cache, the one-shot `retry_context` rebuilding per call.
- Entry: `guarded`/`guarded_sync` own the whole retry/span/lift triplet for every fetch-shaped leg, so a budget-exhausted transient surfaces as the `boundary` case naming the final cause and a non-transient raise surfaces immediately. `guard(cls)` is the bare bound caller for the one consumer that already owns its span and fault rail — the `execution/lanes#LANE` `retried` admission row, where a second span and boundary lift doubles the lane's rail; `retrying(cls)` is the inline form for blocks the caller cannot pre-shape as a coroutine; `install(mode)` returns the finalized `get_on_retry_hooks()` tuple as typed registration evidence.
- Auto: `Policy.schedule` folds only the present columns, so a `stamina` default stands for an absent column; every `BackoffHook` arm returns `bool` or a server-directed delay, leaving the wait to the `stamina` schedule. No classified field rides a retry fact — the hook mints under the receipts-owned `OPEN` keep-all, never a local `Redaction` re-mint.
- Boundary: no retry around a pure transform — `stamina` rides only flaky external oracles through this table. Exported contract is branch-consumer law: the entry family, every `POLICY` row, and the `RetryClass` vocabulary; narrowing the `OCCT` target below the `BrokenWorkerInterpreter | BrokenWorkerProcess` pair is a cross-folder break.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import AbstractContextManager
from datetime import timedelta
from enum import Enum, StrEnum
from functools import cache
from typing import Final, Protocol, assert_never, runtime_checkable

import anyio
import stamina
from expression.collections import Map
from keyring.errors import KeyringLocked
from msgspec import UNSET, Struct, UnsetType
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from stamina.instrumentation import RetryDetails, RetryHook, RetryHookFactory, StructlogOnRetryHook, get_on_retry_hooks, set_on_retry_hooks

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, async_boundary, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import OPEN, Receipt, Signals

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

    @property
    def policy(self) -> "Policy":
        return POLICY[self]


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


# --- [SERVICES] -------------------------------------------------------------------------

# minted from the faults-owned scope row off the proxy-until-install provider.
_TRACER: Final = trace.get_tracer(SCOPES[Scope.RESILIENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transient(*targets: type[Exception] | str, refuse: tuple[type[Exception] | str, ...] = ()) -> BackoffHook:
    # ONE polymorphic MRO-matching law over both target spellings, the value's own shape the discriminant: an importable
    # class (stdlib, BASE-tier dep) matches by isinstance, a gated provider's class by module-qualified name over the
    # raise's MRO — identical subclass semantics, zero provider imports, a bare-name collision unspellable. `refuse`
    # rides the same polymorphic axis and reads first, pinning the terminal subclasses a transient base would otherwise
    # absorb (asyncssh auth/host-key failures subclass DisconnectError; a missing binary subclasses OSError).
    wanted_types = tuple(t for t in targets if isinstance(t, type))
    wanted_names = frozenset(t for t in targets if isinstance(t, str))
    denied_types = tuple(t for t in refuse if isinstance(t, type))
    denied_names = frozenset(t for t in refuse if isinstance(t, str))

    def backoff(exc: Exception) -> bool:
        spellings = frozenset(f"{k.__module__}.{k.__qualname__}" for k in type(exc).__mro__)
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


async def guarded[T](cls: RetryClass, fn: Callable[..., Awaitable[T]], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        return await async_boundary(subject, lambda: guard(cls)(fn, *args, **kwargs))


def guarded_sync[T](cls: RetryClass, fn: Callable[..., T], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        return boundary(subject, lambda: guard_sync(cls)(fn, *args, **kwargs))


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
    (RetryClass.SECRET, Policy(attempts=3, timeout=10.0, target=(KeyringLocked, OSError))),
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
            target=_transient("loky.process_executor.TerminatedWorkerError", "concurrent.futures.process.BrokenProcessPool", "pebble.common.types.ProcessExpired"),
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
    (RetryClass.RPC, Policy(attempts=3, timeout=120.0, target=_transient("compas.rpc.errors.RPCServerError", "compas.rpc.errors.RPCClientError"), wait_initial=0.5)),
    # lakehouse commit-conflict transients, name-matched; `wait_initial` lets a competing commit land before the re-read.
    (
        RetryClass.LAKE_COMMIT,
        Policy(
            attempts=4,
            timeout=60.0,
            # deltalake's CommitFailedError is a pyo3 class whose __module__ is the Rust "_internal" module — its verified spelling
            target=_transient("_internal.CommitFailedError", "pyiceberg.exceptions.CommitFailedException", "pyiceberg.exceptions.CommitStateUnknownException"),
            wait_initial=0.2,
        ),
    ),
    # ADBC/Flight SQL transport stalls, status-discriminated so a permanent driver fault never retries.
    (RetryClass.REMOTE_DB, Policy(attempts=3, timeout=30.0, target=_adbc_transient(ConnectionError, TimeoutError), wait_initial=0.2)),
    # daft's Rust-backed transient base plus the stdlib pair; `wait_max` widens for a long scan without inflating attempts.
    (RetryClass.STREAMING, Policy(attempts=3, timeout=120.0, target=_transient("daft.exceptions.DaftTransientError", TimeoutError, ConnectionError), wait_max=30.0)),
])


# --- [COMPOSITION] ----------------------------------------------------------------------

# lazy build per the RetryHookFactory contract.
RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)

# receipt+span, the metrics-owned rasm.retry.attempts counter, and the structlog warning — one RetryDetails payload.
RETRY_HOOKS: Final[tuple[RetryHook | RetryHookFactory, ...]] = (RetryReceiptHook, Metrics.retry_hook(), StructlogOnRetryHook)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

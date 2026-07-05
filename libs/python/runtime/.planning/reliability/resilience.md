# [PY_RUNTIME_RESILIENCE]

The one retry-policy table for the whole branch. `RetryClass` is the single behavior-carrying `stamina`-backed `StrEnum` the fault, transport, lane, and concurrency clusters consume through `guard`/`guarded`/`retrying` only. Each member binds one frozen `Policy` row per retryable resource class (`attempts`, `timeout`, the `RetryTarget` `target`, and four optional backoff columns), never a flag the caller re-derives.

This is a BASE-tier module: it imports no sibling-tier package. Every provider-discriminated row is import-free by construction — the `target` column is the full `stamina` discriminator over one axis, an exception-set tuple where the classes are stdlib or branch-substrate, and a `BackoffHook` predicate everywhere else. `_named` matches the raise's type or any base over the MRO by qualname (dotted names compare module-qualified), so a gated provider's subclass tree retries with zero imports; `_adbc_transient` reads the remote-database `status_code` name structurally; `_wire_transient` admits exactly the transient gRPC status trio (`UNAVAILABLE`/`DEADLINE_EXCEEDED`/`RESOURCE_EXHAUSTED`) off an `AioRpcError`'s `code().name` — the `(ConnectionError,)` tuple that could never match it is the deleted form — and `_retry_after` honors a server-directed delay. Backoff geometry is row data: `wait_initial`/`wait_max`/`wait_jitter`/`wait_exp_base` are `UNSET`-defaulting `Policy` columns, and `Policy.schedule` is the one projection spreading the present columns into the `**`-passable `stamina` keyword schema both caller shapes read.

The triad is three native `stamina` application shapes over the one row, with `guarded` the primary consumer envelope: it drives the `functools.cache`-held bound caller around `fn` inside one `resilience.guarded` span (minted from the `reliability/faults#FAULT` `SCOPES[Scope.RESILIENCE]` row) and lifts the terminal raise through `async_boundary` exactly once into a `RuntimeRail[T]`. `guarded_sync` is its synchronous mirror over `guard_sync` and the sync `boundary`; the sync and async callers are one `Policy.schedule` source's two runtime arms, never two policy tables. `guard(cls)` is the lower bare bound caller for the one consumer that already owns its span and fault rail — the `execution/lanes#LANE` `ADMIT_TABLE` `retried` row binds it as a per-unit retry aspect inside the lane's own deadline scope, where a second `guarded` span and `async_boundary` would double the lane's rail. `retrying(cls)` rebuilds the one-shot `stamina.retry_context` per call, typed `AsyncIterator[stamina.Attempt]`, for inline blocks the caller cannot pre-shape as a coroutine. A fetch-shaped leg re-spelling a bare caller inside a hand-opened span and boundary fence is the deleted form every consuming owner names. Recoverability over the resulting rail stays the faults owner's `BoundaryFault.recoverable` at the caller.

`install(mode)` owns the one process-global `set_on_retry_hooks` registration and returns the finalized hook tuple `get_on_retry_hooks()` reads back (factories executed) as the registration evidence. `RETRY_HOOKS` weaves three observability surfaces from one `RetryDetails` payload — `RetryReceiptHook` mints the `observability/receipts#RECEIPT` `planned`-phase fact with the native `retry_num`/`wait_for`/`waited_so_far` scalars plus the child retry span, the `observability/metrics#METRIC`-owned `Metrics.retry_hook()` increments the `retry.attempts` counter, and `StructlogOnRetryHook` emits the structlog warning. The composition is DAG-proven: `faults < receipts < metrics < resilience`, so the module-load `Metrics.retry_hook()` call inside `RETRY_HOOKS` binds eagerly against strictly-earlier modules. `install` is total over `RetryMode`: `EMIT` registers the stack, `SILENT` registers the empty iterable, and `TEST` collapses backoff through `set_testing(True)` *and* registers `()` so a deterministic spec is both fast and telemetry-quiet. The hook table is process-global; a second `set_on_retry_hooks` anywhere clobbers this stack.

## [01]-[INDEX]

- [01]-[RESILIENCE]: the one behavior-carrying `RetryClass` enum, its per-member `Policy` row projected to the `stamina` keyword schema through one `Policy.schedule`, the member-keyed cached bound callers, the four import-free `BackoffHook` arms (`_retry_after` server-rate-limit, `_named` MRO-qualname, `_adbc_transient` remote-database status, `_wire_transient` gRPC status trio), the `guarded`/`guarded_sync`/`retrying`/`guard`/`guard_sync` family over the one row, and the one `RETRY_HOOKS` stack `install` registers total over the run mode, returning the finalized tuple.

## [02]-[RESILIENCE]

- Owner: `RetryClass` — the single closed `StrEnum` resilience vocabulary whose every member carries a frozen `Policy` row over `stamina`, resolving its row through the `policy` property; `Policy` the frozen `msgspec.Struct` row owning `Policy.schedule` — the one projection folding the present (non-`UNSET`) schedule columns into the keyword dict so the bound-caller builds and the `retrying` inline context consume one source; `POLICY` the one `expression` `Map[RetryClass, Policy]` keyed by the member itself, the `policy` property reading `POLICY[self]` so no `.value` string re-spells the vocabulary; `RetryTarget`/`BackoffHook` the page-local aliases of the `stamina` `on=` discriminator shape (`type[Exception] | tuple[...] | Callable[[Exception], bool | float | timedelta]`) — `stamina` exports no public alias for it, `stamina.typing` carrying only `RetryDetails`/`RetryHook`; `guard`/`guard_sync` the `functools.cache`-memoised member-keyed entries minting the reusable `stamina.BoundAsyncRetryingCaller`/`BoundRetryingCaller` once per class (a module-level cache rather than a `cached_property`, since an `Enum` member carries no writable `__dict__`; only the reusable bound caller is safe to cache — the one-shot `retry_context` rebuilds per call); `RetryAfter` and `StatusCarrier`/`StatusCoded` the structural protocols the backoff hooks read typed slots through instead of importing a provider shape; `RetryMode` the closed install-mode vocabulary; `RetryReceiptHook` the one `RetryHookFactory` mapping `RetryDetails` onto the receipt and the active span; `RETRY_HOOKS` the one composed hook stack.
- Entry: `guarded(cls, fn, *args, subject, **kwargs)` is the primary consumer envelope — the cached bound caller inside one `start_as_current_span` retry span, the terminal raise lifted through `async_boundary` exactly once, so a budget-exhausted transient surfaces as the `boundary` case naming the final cause and a non-transient raise surfaces immediately. Every fetch-shaped leg delegates the whole retry/span/lift triplet: `transport/roots#RESOURCE` `Transfer.run`, `transport/wire#WIRE_RAIL` `Decode.acquired`, `transport/serve#SERVE` dispatch (`guarded(RetryClass.WIRE, ...)` — real now that the row's `_wire_transient` hook matches the transient status codes), and `execution/admission#SETTINGS` `_probe`. `guarded_sync(cls, fn, *args, subject, **kwargs)` is the sync mirror over `guard_sync` and the sync `boundary` — the `data:tabular/lakehouse#LAKEHOUSE` commit legs bind it at their fifteen synchronous sites. `guard(cls)` is the bare bound caller driven `await guard(cls)(fn, *args, **kwargs)` — the lanes `retried` admission row's per-unit aspect. `retrying(cls)` is the inline `async for attempt in retrying(cls): with attempt: ...` form with `attempt.num`/`attempt.next_wait` for inline instrumentation. `install(mode)` registers the hook stack and returns `get_on_retry_hooks()` — the finalized `tuple[RetryHook, ...]` with factories executed, the typed registration evidence a spec or composition root reads.
- Auto: `Policy.schedule` seeds `{"attempts", "timeout"}` and folds each non-`UNSET` `wait_*` column in, so a `stamina` default stands for an absent column; `guard` spreads it as `AsyncRetryingCaller(**row.schedule).on(row.target)` and `retrying` as `retry_context(on=row.target, **row.schedule)`. Every `BackoffHook` arm returns `bool` (or the `_retry_after` server delay) so the `stamina` exponential schedule owns the wait; the hooks are import-free — the MRO walk in `_named` gives isinstance semantics over a provider's subclass tree without the class object, and the two status predicates read `status_code.name`/`code().name` through structural protocols. `RetryReceiptHook`'s built hook reads the `RetryDetails` payload field-for-field, mints the `planned` fact through `Receipt.of("resilience", ("planned", name, facts))` under the receipts-owned `OPEN` keep-all redaction (no classified field rides a retry fact — a local `Redaction(classified=Map.empty())` re-mint is the deleted form), carrying native scalars (the receipts `enc_hook=repr` renderer owns JSON coercion), sets the child span's `Status(StatusCode.ERROR, cause)`, and returns the `trace.use_span` context manager wrapping the scheduled wait so each retry is a child span entered when scheduled and exited before the retry runs; the `stamina` `RetryHook` is synchronous even on the async path, so the receipt mints through the sync `Signals.emit`.
- Boundary: no sibling-tier module-top import (the `adbc-driver-manager`/`daft`/`obstore` imports are the deleted forms their `_named`/`_adbc_transient` rows replace); no second policy table, no hand-rolled sleep loop, no scope literal beside the `SCOPES` row, no second `set_on_retry_hooks` owner, no retry around a pure transform (`stamina` rides only flaky external oracles through this table); a `POLICY[cls.value]` string-keyed read beside the member-keyed `Map`, a `stamina.BackoffHook`/`stamina.ExcOrBackoffHook` attribute reach (the aliases live in `stamina._core`, private — the page-local `RetryTarget`/`BackoffHook` are the spelling), and a re-minted `Redaction(classified=Map.empty())` beside the receipts-owned `OPEN` keep-all are each the deleted form; and no narrowing of the exported consumer contract — `guarded`/`guarded_sync`/`retrying`/`guard`/`guard_sync`, every `POLICY` row, and the `RetryClass` vocabulary are branch-consumer law (`data` binds both fused envelopes and the `OBJECT_STORE`/`HTTP`/`WIRE`/`RPC`/`LAKE_COMMIT`/`REMOTE_DB`/`STREAMING` rows; `geometry` binds `OCCT`/`RPC`/`OCC_NATIVE`; `compute` and `artifacts` bind the offload axis, `artifacts` the `ORACLE` row); narrowing the `OCCT` target below the `BrokenWorkerInterpreter | BrokenWorkerProcess` pair is a cross-folder break.

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

# the stamina `on=` discriminator shape spelled locally: stamina exports no public alias for it
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

# the four optional schedule columns whose `UNSET` value defers to the `stamina` default;
# `Policy.schedule` folds only the columns the row actually set.
_WAIT_COLUMNS: Final[tuple[str, ...]] = ("wait_initial", "wait_max", "wait_jitter", "wait_exp_base")
# the transient gRPC status trio the grpcio client-fault law names retriable.
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

# the one tracer handle, minted from the faults-owned scope row off the proxy-until-install
# provider; the retry child span the on-retry hook opens is minted here.
_TRACER: Final = trace.get_tracer(SCOPES[Scope.RESILIENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _retry_after(*transient: type[Exception]) -> BackoffHook:
    def backoff(exc: Exception) -> bool | float | timedelta:
        match exc:
            case RetryAfter(retry_after=float() as seconds):
                return seconds
            case _:
                return isinstance(exc, transient)

    return backoff


def _named(*names: str) -> BackoffHook:
    # import-free transient discriminator: matches the raise's type OR any base over the MRO by
    # qualname (a dotted name compares module-qualified) — isinstance semantics over a gated
    # provider's subclass tree with zero imports; `bool` return leaves the wait to the schedule.
    wanted = frozenset(names)
    return lambda exc: any(k.__qualname__ in wanted or f"{k.__module__}.{k.__qualname__}" in wanted for k in type(exc).__mro__)


def _adbc_transient(*transient: type[Exception]) -> BackoffHook:
    # the REMOTE_DB discriminator, import-free: an adbc `OperationalError` retries ONLY when its
    # `status_code` names a transport transient (`TIMEOUT`/`IO` — never `INVALID_ARGUMENT`/`NOT_FOUND`
    # a re-issue cannot clear), read structurally; the stdlib classes retry unconditionally.
    statuses = frozenset({"TIMEOUT", "IO"})

    def backoff(exc: Exception) -> bool:
        match exc:
            case StatusCarrier(status_code=Enum() as code) if type(exc).__module__.partition(".")[0] == "adbc_driver_manager":
                return code.name in statuses
            case _:
                return isinstance(exc, transient)

    return backoff


def _wire_transient(*transient: type[Exception]) -> BackoffHook:
    # the WIRE discriminator, import-free: an `AioRpcError` retries only on the transient status
    # trio; every other status is terminal, and the stdlib classes retry unconditionally.
    def backoff(exc: Exception) -> bool:
        match exc:
            case StatusCoded() as coded if type(exc).__qualname__ == "AioRpcError":
                return coded.code().name in _WIRE_STATUS
            case _:
                return isinstance(exc, transient)

    return backoff


def _retry_receipt() -> RetryHook:
    # the `stamina` `RetryHook` is a synchronous callable even on the async retry path, so the
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


# the lower bare bound caller, memoised per member off the row's keyword schedule (each
# `__call__` opens a fresh internal `retry_context`); `guarded` is the primary fetch-leg entry
# built on it, while the lanes `retried` admission row binds this bare caller as a per-unit
# retry aspect inside its own already-railed drain.
@cache
def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    row = cls.policy
    return stamina.AsyncRetryingCaller(**row.schedule).on(row.target)


# the sync mirror of `guard` for a transient-failure boundary that cannot be a coroutine
# (the lakehouse commit legs): same `Policy.schedule` source, second runtime arm, one table.
@cache
def guard_sync(cls: RetryClass) -> stamina.BoundRetryingCaller:
    row = cls.policy
    return stamina.RetryingCaller(**row.schedule).on(row.target)


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    # one-shot iterator, rebuilt per call — never cached.
    row = cls.policy
    return stamina.retry_context(on=row.target, **row.schedule)


# the one fused consumer envelope: the cached bound caller around `fn` inside one retry span,
# the terminal raise lifted through the faults owner's `async_boundary` exactly once.
async def guarded[T](cls: RetryClass, fn: Callable[..., Awaitable[T]], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        return await async_boundary(subject, lambda: guard(cls)(fn, *args, **kwargs))


# the sync fused envelope: the same retry/span/terminal-lift triplet over `guard_sync` and the
# synchronous `boundary` — never a hand-opened `retry_context` block at the call site.
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

# the one row-per-member policy table, keyed by the `RetryClass` member itself (the `.value`
# string key is the deleted spelling); a new class is one member plus one row, every
# provider-discriminated target import-free.
POLICY: Final[Map[RetryClass, Policy]] = Map.of_seq([
    # obstore transients raise as subclasses of `obstore.exceptions.BaseError` — MRO-matched.
    (RetryClass.OBJECT_STORE, Policy(attempts=4, timeout=30.0, target=_named("obstore.exceptions.BaseError", "TimeoutError"))),
    (RetryClass.HTTP, Policy(attempts=3, timeout=20.0, target=_retry_after(TimeoutError, ConnectionError))),
    (RetryClass.SSH, Policy(attempts=3, timeout=30.0, target=(ConnectionError, TimeoutError))),
    # the serve outbound leg retries the transient status trio through the cached `guard(RetryClass.WIRE)`
    # caller; its trailer fence owns the terminal `AioRpcError` lift so the typed detail survives.
    (RetryClass.WIRE, Policy(attempts=5, timeout=15.0, target=_wire_transient(ConnectionError))),
    (RetryClass.SCAN, Policy(attempts=2, timeout=60.0, target=(OSError,), wait_max=30.0)),
    (RetryClass.SECRET, Policy(attempts=3, timeout=10.0, target=(KeyringLocked, OSError))),
    # `execution/recipe#RECIPE` runs the lbt engine prechecks through `guarded_sync(RetryClass.ENGINE, ...)`:
    # a transiently-locked config folder retries tightly, a missing engine exhausts fast, before the subprocess.
    (RetryClass.ENGINE, Policy(attempts=2, timeout=10.0, target=(OSError, TimeoutError))),
    # flaky external-oracle subprocess verdicts (artifacts `exchange/conformance` is the demanding
    # consumer): name-matched so no subprocess import rides this tier; tight attempts.
    (RetryClass.ORACLE, Policy(attempts=2, timeout=30.0, target=_named("CalledProcessError", "TimeoutError", "OSError"))),
    # subinterpreter/process worker-death band: the pair is exported law — artifacts and compute bind
    # the process half, geometry the interpreter half; `wait_initial` opens wide for the re-spawn.
    (RetryClass.OCCT, Policy(attempts=3, timeout=120.0, target=(anyio.BrokenWorkerInterpreter, anyio.BrokenWorkerProcess), wait_initial=0.5)),
    # in-process transient OCC `RuntimeError` band (geometry `ifc/analysis` clash-tree boundary);
    # tight attempts so a genuinely broken kernel surfaces fast.
    (RetryClass.OCC_NATIVE, Policy(attempts=2, timeout=20.0, target=(RuntimeError,))),
    # `compas.rpc.Proxy` bring-up: cold-start `RPCServerError`/remote-dispatch `RPCClientError`,
    # name-matched (compas is gated dark); long timeout covers the worst-case ping-loop bring-up.
    (RetryClass.RPC, Policy(attempts=3, timeout=120.0, target=_named("RPCServerError", "RPCClientError"), wait_initial=0.5)),
    # lakehouse commit-conflict transients (deltalake/pyiceberg conflict classes, name-matched);
    # the mutating arms ride `guarded_sync(RetryClass.LAKE_COMMIT, ...)`; `wait_initial` lets a
    # competing commit land before the re-read.
    (
        RetryClass.LAKE_COMMIT,
        Policy(
            attempts=4, timeout=60.0, target=_named("CommitFailedError", "CommitFailedException", "CommitStateUnknownException"), wait_initial=0.2
        ),
    ),
    # ADBC/Flight SQL transport stalls, status-discriminated so a permanent driver fault is never
    # retried; `data:tabular/query#QUERY` rides `guarded(RetryClass.REMOTE_DB, ...)`.
    (RetryClass.REMOTE_DB, Policy(attempts=3, timeout=30.0, target=_adbc_transient(ConnectionError, TimeoutError), wait_initial=0.2)),
    # daft's one Rust-backed transient base plus the stdlib pair, MRO-matched; `wait_max` widens
    # for a long distributed scan without inflating attempts.
    (RetryClass.STREAMING, Policy(attempts=3, timeout=120.0, target=_named("DaftTransientError", "TimeoutError", "ConnectionError"), wait_max=30.0)),
])


# --- [COMPOSITION] ----------------------------------------------------------------------

# the one on-retry signal: a RetryHookFactory whose built hook mints the receipt fact and
# the child span from one RetryDetails payload (lazy build per the factory contract).
RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)

# the one stacked hook set EMIT registers — receipt+span, the metrics-owned retry.attempts
# counter, and the structlog warning, woven from one RetryDetails payload in one call; the
# eager `Metrics.retry_hook()` bind is DAG-legal (metrics < resilience).
RETRY_HOOKS: Final[tuple[RetryHook | RetryHookFactory, ...]] = (RetryReceiptHook, Metrics.retry_hook(), StructlogOnRetryHook)
```

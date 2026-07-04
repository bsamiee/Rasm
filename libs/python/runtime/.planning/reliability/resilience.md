# [PY_RUNTIME_RESILIENCE]

The one retry-policy table for the whole branch. `RetryClass` is the single behavior-carrying `stamina`-backed `StrEnum` the fault, transport, lane, and concurrency clusters consume through `guard`/`guarded`/`retrying` only. Each member binds one frozen `Policy` row per retryable resource class (`attempts`, `timeout`, the `ExcOrBackoffHook` `target`, and four optional backoff columns), never a flag the caller re-derives.

The `target` column is the full `stamina` discriminator over one axis: an exception-set class binds a tuple, and a class whose transient needs a predicate binds a `BackoffHook` — a server-rate-limited class a `Retry-After` hook, a gated-dark provider a `_named` by-`__qualname__` hook, a status-discriminated remote-database class an `_adbc_transient` `status_code` hook — and none grows a parallel sleep path. Backoff geometry is row data, not a process-global default — `wait_initial`/`wait_max`/`wait_jitter`/`wait_exp_base` are `UNSET`-defaulting `Policy` columns the row overrides per class. `Policy.schedule` is the one projection dropping the `UNSET` columns and spreading the present ones into the `**`-passable `stamina` keyword schema, so the `guard` bound-caller build and the `retrying` inline `retry_context` read one source rather than re-spelling `attempts=…, timeout=…, wait_*=…` independently.

The triad is three native `stamina` application shapes over the one row, with `guarded` the primary consumer envelope. `guarded(cls, fn, *args, subject, **kwargs)` is the one fused entry every transport, lane, and secret leg delegates to — it drives the held bound caller around `fn` inside one `resilience.guarded` derivation span and lifts the terminal raise through the `reliability/faults#FAULT` `async_boundary` exactly once into a `RuntimeRail[T]`, the span opened around the `async_boundary` so the faults owner's `_convert` records the terminal raise on the retry span. No fetch-shaped leg re-spells the retry/span/lift triplet inline: `transport/roots#RESOURCE`, `transport/wire#WIRE_RAIL`, `transport/serve#SERVE`, and `execution/admission#SETTINGS` each pass `guarded(cls, fn, *args, subject=...)` rather than composing a bare `guard(cls)` caller inside a hand-opened span and `async_boundary` fence, which is the duplicated-rail deleted form those owners reject. `guard(cls)` is the lower bare bound caller `guarded` builds on, memoised per member under `functools.cache`: it holds the `stamina.BoundAsyncRetryingCaller` once and each `__call__` opens a fresh internal `retry_context`. It is the public entry for the one consumer that already owns its span and fault rail and wants only the retry: the `execution/lanes#LANE` `ADMIT_TABLE` `retried` row binds `guard(cls)` as a per-unit retry aspect (`make=lambda unit: lambda: guard(unit.retried[0])(unit.retried[1])`) inside the lane's own task-group deadline scope, the lane coroutine's `RuntimeRail[T]` flowing into the drain's `traversed` fault fold — so wrapping that unit in `guarded`'s second span and `async_boundary` would double the lane's own rail. A fetch-shaped leg reaching past `guarded` to `guard` and re-opening the resilience envelope is the deleted form those legs reject. `retrying(cls)` rebuilds the one-shot `stamina.retry_context` per call, typed `AsyncIterator[stamina.Attempt]` so the caller `async for attempt in retrying(cls)` reads `attempt.num`/`attempt.next_wait` natively (a one-shot iterator silently exhausts on a second drive), for inline blocks the caller cannot pre-shape as a coroutine. Recoverability over the resulting rail stays the faults owner's `BoundaryFault.recoverable` at the caller, never minted here.

`install(mode)` owns the one process-global `set_on_retry_hooks` registration: `RETRY_HOOKS` weaves three observability surfaces from one `RetryDetails` payload — `RetryReceiptHook` (`RetryHookFactory`) mints the `observability/receipts#RECEIPT` `planned`-phase fact carrying the native `retry_num`/`wait_for`/`waited_so_far` values straight off the payload (no `str()` coerce, since the receipts `Evidence` facts map is `dict[str, object]` and its `msgspec` `enc_hook=repr`/`order="deterministic"` renderer serializes the native scalars) plus the child span, the `observability/metrics#METRIC`-owned `Metrics.retry_hook()` increments the `retry.attempts` `Counter`, and `StructlogOnRetryHook` emits the structlog warning — so retry logging, the receipt fact, the span status, and the exhaustion counter mint once from one payload. `install` is total over `RetryMode`: `EMIT` registers the stack, `SILENT` registers the empty iterable, and `TEST` collapses backoff through `set_testing(True)` *and* registers `()` so a deterministic spec is both fast and telemetry-quiet rather than inheriting whatever a prior `install` last left in the process-global table. The metrics owner never re-registers; a second `set_on_retry_hooks` anywhere clobbers this stack because the hook table is process-global.

## [01]-[INDEX]

- [01]-[RESILIENCE]: the one behavior-carrying `RetryClass` enum, its per-member `Policy` row (attempts, timeout, `ExcOrBackoffHook` `target`, and the `UNSET`-defaulting `wait_*` backoff columns) projected to the `stamina` keyword schema through one `Policy.schedule`, the member-keyed cached `stamina` bound caller, the `ExcOrBackoffHook` backoff-hook arms (`_retry_after` server-rate-limit, `_named` gated-dark by-`__qualname__`, `_adbc_transient` remote-database `status_code` predicate), the `guarded`/`retrying`/`guard` triad over the one row (the primary fused span+`async_boundary` envelope every consumer delegates to, the inline `retry_context` block, the lower bare bound caller), and the one `RETRY_HOOKS` stack (receipt+span, metrics-owned `retry.attempts` counter, structlog warning) `install` registers once total over the run mode.

## [02]-[RESILIENCE]

- Owner: `RetryClass` — the single closed `StrEnum` resilience vocabulary whose every member carries a frozen `Policy` row over `stamina` (`attempts`, `timeout`, the `ExcOrBackoffHook` `target`, and the four `UNSET`-defaulting `wait_*` backoff columns), resolving its row through the `policy` property; `Policy` the frozen `msgspec.Struct` row the member binds, owning `Policy.schedule` — the one projection folding the present (non-`UNSET`) schedule columns into the `**`-passable `stamina` keyword dict so the `guard` bound-caller build and the `retrying` inline context consume one keyword source rather than re-spelling the schedule twice; `POLICY` the one `expression` `Map[str, Policy]` of typed rows the member resolves through by its string value; `guard` the `functools.cache`-memoised member-keyed entry minting the reusable `stamina.BoundAsyncRetryingCaller` once per class (a module-level cache rather than a `functools.cached_property` on the member, since an `Enum` instance carries no writable per-member `__dict__` for the descriptor to populate, and only the reusable bound caller is safe to cache — the one-shot `retry_context` is rebuilt per call); `RetryAfter` the host-neutral structural protocol a server-rate-limit-carrying exception satisfies so the `HTTP` backoff hook reads a typed `retry_after` slot rather than introspecting a provider response shape; `RetryMode` the closed install-mode vocabulary; `RetryReceiptHook` the one `RetryHookFactory` mapping `stamina.instrumentation.RetryDetails` onto `observability/receipts#RECEIPT` and the active span; `RETRY_HOOKS` the one composed hook stack `install` registers — `RetryReceiptHook` plus the `observability/metrics#METRIC`-owned `Metrics.retry_hook()` counter increment plus `StructlogOnRetryHook` — so the single `set_on_retry_hooks` call fans the on-retry signal across receipt, metric, and structlog surfaces.
- Entry: `guarded(cls, fn, *args, subject, **kwargs)` is the primary consumer envelope, parameterized over the `(fn, *args, **kwargs)` input and the `RuntimeRail[T]` output: it drives the member-cached bound caller around `fn` inside one `start_as_current_span` retry span and lifts the terminal raise through `reliability/faults#FAULT` `async_boundary` exactly once, so a budget-exhausted transient class surfaces as the `boundary` case naming the final cause and a non-transient raise the row never named surfaces immediately, the call site deciding recovery through `BoundaryFault.recoverable` against its own code set. Every fetch-shaped leg delegates the whole retry/span/lift triplet to it: `transport/roots#RESOURCE` `Transfer.run`'s `guarded(plan.retry_class, plan.whole, subject=plan.subject)`, `transport/wire#WIRE_RAIL` `Decode.acquired`'s `guarded(RetryClass.WIRE, fetch, subject="wire")`, `transport/serve#SERVE` dispatch's `guarded(RetryClass.WIRE, method, request, subject="wire")`, and `execution/admission#SETTINGS` `_probe`'s `guarded(row.retry_class, anyio.to_thread.run_sync, <tier-read>, subject="secret")` — none re-spelling a bare `guard(cls)` caller inside a hand-opened span and `async_boundary`, which each owner names a deleted form. `guard(cls)` is the lower `functools.cache`-held `stamina.BoundAsyncRetryingCaller` `guarded` builds on, driven `await guard(cls)(fn, *args, **kwargs)` — the precise `(callable, *args, **kw)` signature `stamina`'s `.on(...)` bound caller preserves — and the public entry for the one consumer that owns its own span and fault rail: the `execution/lanes#LANE` `ADMIT_TABLE` `retried` row binds it as a per-unit retry aspect inside the lane's task-group deadline scope, the unit's `RuntimeRail[T]` flowing into the drain's own `traversed` fold, so a second `guarded` span and `async_boundary` would double the lane's rail. A fetch-shaped leg composing the bare caller inside its own fence is the doubled-span/doubled-lift form those legs reject. `retrying(cls)` rebuilds the one-shot `stamina.retry_context` from the row per call, typed `AsyncIterator[stamina.Attempt]` for inline blocks the caller cannot pre-shape as a coroutine, driven `async for attempt in retrying(cls): with attempt: ...` with `attempt.num`/`attempt.next_wait` for inline instrumentation. `guarded_sync(cls, fn, *args, subject, **kwargs)` is the synchronous mirror of `guarded` — the same retry/span/terminal-lift triplet fused over the `functools.cache`-held sync `guard_sync(cls)` `stamina.BoundRetryingCaller` and the faults owner's synchronous `boundary` — the one entry a transient-failure boundary that cannot be a coroutine delegates to, the `data:tabular/lakehouse#LAKEHOUSE` `Lakehouse.run` commit-conflict legs over the synchronous `deltalake`/`pyiceberg` provider call riding `guarded_sync(RetryClass.LAKE_COMMIT, ...)` since `run` is synchronous, never a hand-opened `stamina.retry_context` block at the call site. The sync and async callers are one `Policy.schedule` source's two runtime arms (`guard`/`guard_sync` over `AsyncRetryingCaller`/`RetryingCaller`), never two policy tables — one policy row, the async `guarded`/`guard`/`retrying` and the sync `guarded_sync`/`guard_sync` shapes over it, never a hand-rolled loop.
- Auto: `Policy.schedule` seeds `{"attempts", "timeout"}` and folds each non-`UNSET` `wait_*` column in, so a `stamina` default stands for an absent column and a present one overrides; `guard` spreads it as `AsyncRetryingCaller(**row.schedule).on(row.target)` and `retrying` as `retry_context(on=row.target, **row.schedule)` — one keyword source, never re-spelled. `Policy.target` is one positional `ExcOrBackoffHook` to both shapes — a retryable-exception tuple or a `BackoffHook`, never a spread. `install(mode)` is the one-shot toggle over `RetryMode` with an `assert_never` totality tail: `EMIT` registers the composed `RETRY_HOOKS` stack through one `set_on_retry_hooks`, `SILENT` passes the empty iterable to deactivate instrumentation, `TEST` calls `set_testing(True)` to collapse backoff and cap attempts *and* registers `()` so a deterministic spec runs fast and silent rather than inheriting a prior install's hook table — production code branches on neither `is_active` nor `is_testing`. `RetryReceiptHook` is the `RetryHookFactory` whose built hook reads the `RetryDetails` payload (`name`, `retry_num`, `wait_for`, `waited_so_far`, `caused_by`) field-for-field, mints a `planned`-phase fact through `Receipt.of("resilience", ("planned", name, facts))` (the two-argument `(owner, evidence)` factory routing the `(Phase, subject, facts)` triple — never a four-positional shape the receipts owner does not expose) carrying the native `retry_num: int` and `wait_for`/`waited_so_far: float` straight onto the `dict[str, object]` facts map rather than pre-`str()`-formatting them (the receipts `enc_hook=repr` renderer owns the JSON coercion, so a numeric fact reaches the line as a number), sets the child span's `Status(StatusCode.ERROR, caused_by)`, and returns the `trace.use_span` context manager wrapping the scheduled wait so each retry is a child span entered when scheduled and exited before the retry runs.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import AbstractContextManager
from datetime import timedelta
from enum import StrEnum
from functools import cache
from typing import Final, Protocol, assert_never, runtime_checkable

import anyio
import stamina
from adbc_driver_manager import AdbcStatusCode
from adbc_driver_manager.dbapi import OperationalError as AdbcOperationalError
from daft.exceptions import DaftTransientError
from expression.collections import Map
from keyring.errors import KeyringLocked
from msgspec import UNSET, Struct, UnsetType
from obstore.exceptions import BaseError as ObjectStoreTransient
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from stamina.instrumentation import RetryDetails, RetryHook, RetryHookFactory, StructlogOnRetryHook, set_on_retry_hooks

from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt, Redaction, Signals

# --- [CONSTANTS] ------------------------------------------------------------------------

_RETRY_FACTS: Final = Redaction(classified=Map.empty())
# the four optional schedule columns whose `UNSET` value defers to the `stamina` default;
# `Policy.schedule` folds only the columns the row actually set.
_WAIT_COLUMNS: Final[tuple[str, ...]] = ("wait_initial", "wait_max", "wait_jitter", "wait_exp_base")


# --- [TYPES] ----------------------------------------------------------------------------


@runtime_checkable
class RetryAfter(Protocol):
    retry_after: float | None


class RetryClass(StrEnum):
    OBJECT_STORE = "object-store"
    HTTP = "http"
    SSH = "ssh"
    WIRE = "wire"
    SCAN = "scan"
    SECRET = "secret"
    ENGINE = "engine"
    OCCT = "occt"
    RPC = "rpc"
    LAKE_COMMIT = "lake-commit"
    REMOTE_DB = "remote-db"
    STREAMING = "streaming"

    @property
    def policy(self) -> "Policy":
        return POLICY[self.value]


class RetryMode(StrEnum):
    EMIT = "emit"
    SILENT = "silent"
    TEST = "test"


# --- [MODELS] ---------------------------------------------------------------------------


class Policy(Struct, frozen=True):
    attempts: int
    timeout: float
    target: stamina.ExcOrBackoffHook
    wait_initial: float | UnsetType = UNSET
    wait_max: float | UnsetType = UNSET
    wait_jitter: float | UnsetType = UNSET
    wait_exp_base: float | UnsetType = UNSET

    @property
    def schedule(self) -> dict[str, object]:
        base: dict[str, object] = {"attempts": self.attempts, "timeout": self.timeout}
        return base | {col: value for col in _WAIT_COLUMNS if (value := getattr(self, col)) is not UNSET}


# --- [SERVICES] -------------------------------------------------------------------------

# the one tracer handle, resolved once off the proxy-until-install provider; the retry
# child span the on-retry hook opens around each scheduled wait is minted here.
_TRACER: Final = trace.get_tracer("rasm.runtime.resilience")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _retry_after(*transient: type[Exception]) -> stamina.BackoffHook:
    def backoff(exc: Exception) -> bool | float | timedelta:
        match exc:
            case RetryAfter(retry_after=float() as seconds):
                return seconds
            case _:
                return isinstance(exc, transient)

    return backoff


def _named(*qualnames: str) -> stamina.BackoffHook:
    # the structural-by-`__qualname__` transient discriminator for a provider whose package is
    # `RPC` row names `RPCServerError`/`RPCClientError` here rather than importing the type, so the
    # matches. A `BackoffHook` returning `bool` (never a delay) so the `stamina` exponential
    # schedule owns the wait, exactly the bare-tuple semantics without the import.
    names = frozenset(qualnames)
    return lambda exc: type(exc).__qualname__ in names


def _adbc_transient(*transient: type[Exception]) -> stamina.BackoffHook:
    # the remote-database transient discriminator for the `REMOTE_DB` row: an ADBC `OperationalError`
    # (the shared DBAPI `Error` base both `adbc-driver-manager` and `adbc-driver-flightsql` raise,
    # `_named`) retries ONLY when its `status_code` is a genuine transport transient (`TIMEOUT`/`IO`,
    # never `INVALID_ARGUMENT`/`NOT_FOUND` which a re-issue cannot clear), while the stdlib transients
    # in `transient` (`ConnectionError`/`TimeoutError`) retry unconditionally. A `BackoffHook` returning
    # `bool` so the `stamina` exponential schedule owns the wait — the status-code predicate is the
    # reason this row is a hook rather than a bare tuple, since a bare `(OperationalError, ...)` tuple
    # would over-retry a non-transient ADBC fault the status code marks permanent.
    transients = frozenset({AdbcStatusCode.TIMEOUT, AdbcStatusCode.IO})

    def backoff(exc: Exception) -> bool:
        match exc:
            case AdbcOperationalError(status_code=code):
                return code in transients
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
            _RETRY_FACTS,
        )
        span = _TRACER.start_span(
            "resilience.retry", attributes={"rasm.retry_num": details.retry_num, "rasm.wait_for": details.wait_for, "rasm.caused_by": cause}
        )
        span.set_status(Status(StatusCode.ERROR, cause))
        return trace.use_span(span, end_on_exit=True)

    return hook


# the lower bare bound caller, memoised per member off the row's keyword schedule (each
# `__call__` opens a fresh internal `retry_context`, so the binding is paid once and reused);
# `guarded` is the primary fetch-leg entry built on it, while the lanes `retried` admission
# row binds this bare caller as a per-unit retry aspect inside its own already-railed drain.
# The inline `retry_context` iterator is one-shot and rebuilt per call in `retrying` — never cached.
@cache
def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    row = cls.policy
    return stamina.AsyncRetryingCaller(**row.schedule).on(row.target)


# the sync mirror of `guard` for a synchronous transient-failure boundary that cannot be shaped
# as a coroutine (the `data:tabular/lakehouse#LAKEHOUSE` `Lakehouse.run` commit-conflict legs over
# the synchronous `deltalake`/`pyiceberg` provider call): the reusable `stamina.RetryingCaller`
# bound to the row's `target`, memoised per member off the same `Policy.schedule` source the async
# caller reads, so the sync and async callers are one row's two runtime arms, never two policy tables.
@cache
def guard_sync(cls: RetryClass) -> stamina.BoundRetryingCaller:
    row = cls.policy
    return stamina.RetryingCaller(**row.schedule).on(row.target)


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    row = cls.policy
    return stamina.retry_context(on=row.target, **row.schedule)


# the one fused consumer envelope: the cached bound caller around `fn` inside one retry span,
# the terminal raise lifted through the faults owner's `async_boundary` exactly once.
async def guarded[T](cls: RetryClass, fn: Callable[..., Awaitable[T]], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        return await async_boundary(subject, lambda: guard(cls)(fn, *args, **kwargs))


# the sync fused envelope: the same retry/span/terminal-lift triplet `guarded` fuses, over the sync
# `guard_sync` caller and the faults owner's synchronous `boundary` — the one entry a synchronous
# transient-failure boundary delegates to (the lakehouse commit legs), never a hand-opened
# `stamina.retry_context` block at the call site (the single-policy-table deleted form).
def guarded_sync[T](cls: RetryClass, fn: Callable[..., T], *args: object, subject: str, **kwargs: object) -> RuntimeRail[T]:
    with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
        return boundary(subject, lambda: guard_sync(cls)(fn, *args, **kwargs))


def install(mode: RetryMode = RetryMode.EMIT) -> None:
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


# --- [TABLES] ---------------------------------------------------------------------------

# the one row-per-member policy table; the bare-string key is the `RetryClass.value`
# the member resolves through, so a new class is one member plus one `POLICY.add` row.
# `WIRE` keeps the tight `stamina` default; `SCAN` widens its cap to back a long-poll scan.
POLICY: Final[Map[str, Policy]] = Map.of_seq([
    ("object-store", Policy(attempts=4, timeout=30.0, target=(ObjectStoreTransient, TimeoutError))),
    ("http", Policy(attempts=3, timeout=20.0, target=_retry_after(TimeoutError, ConnectionError))),
    ("ssh", Policy(attempts=3, timeout=30.0, target=(ConnectionError, TimeoutError))),
    ("wire", Policy(attempts=5, timeout=15.0, target=(ConnectionError,))),
    ("scan", Policy(attempts=2, timeout=60.0, target=(OSError,), wait_max=30.0)),
    ("secret", Policy(attempts=3, timeout=10.0, target=(KeyringLocked, OSError))),
    # the external-simulation-engine precheck transient: the `execution/recipe#RECIPE` gate runs the
    # lbt_recipes `version.check_*` probes (Radiance/OpenStudio/EnergyPlus folder+binary reads) through
    # `guarded_sync(RetryClass.ENGINE, ...)` — a transiently-locked config folder or slow first probe
    # retries tightly, while a genuinely missing engine exhausts fast and fails BEFORE the subprocess.
    ("engine", Policy(attempts=2, timeout=10.0, target=(OSError, TimeoutError))),
    # the subinterpreter-offload transient: a `BrokenWorkerInterpreter` cold-start crash on the PEP-734
    # hop (the OCCT/native-geometry 200-400ms warm-up surface) retries under one stamina row, while a
    # `BrokenWorkerProcess` covers the to_thread degradation path; `wait_initial` opens wide so a
    # re-spawn clears the failed worker before the next attempt. The `execution/lanes#LANE` `offload`
    # retry leg rides this class so a transient worker crash retries before the lane's `async_boundary`
    # resolves the rail — the daemon/scan offload consumers pass `RetryClass.OCCT`, never a per-call loop.
    ("occt", Policy(attempts=3, timeout=120.0, target=(anyio.BrokenWorkerInterpreter, anyio.BrokenWorkerProcess), wait_initial=0.5)),
    # the cross-process `compas.rpc.Proxy` bring-up transient: a fresh proxy whose `_try_reconnect` finds
    # no localhost server spawns one through a blocking `start_server()` (`Popen` + `time.sleep(0.1)` ping
    # loop, `max_conn_attempts=100`) that raises `RPCServerError` on a cold-start timeout / `RPCClientError`
    # on a remote-dispatch fault. The `geometry/graph/algebra.md#ALGEBRA` `solver_proxy` scope-entry offload
    # retries this class so the bring-up clears before the scope yields — the XML-RPC analog of `OCCT`.
    # `target` is the `_named` structural `BackoffHook` (NOT a type tuple) because `compas` is the gated
    # its port before the next attempt, under a long `timeout` covering the worst-case ping-loop bring-up.
    ("rpc", Policy(attempts=3, timeout=120.0, target=_named("RPCServerError", "RPCClientError"), wait_initial=0.5)),
    # the transactional-commit-conflict transient: a concurrent-writer conflict on the synchronous
    # `data:tabular/lakehouse#LAKEHOUSE` commit legs surfaces as `deltalake.exceptions.CommitFailedError`
    # or pyiceberg `CommitFailedException`/`CommitStateUnknownException` — a re-read-and-retry clears it.
    # `target` is the `_named` structural `BackoffHook` (NOT a type tuple) because `pyiceberg` is the
    # conflict classes by `__qualname__` rather than importing them, and the lakehouse mutating arms
    # ride `guarded_sync(RetryClass.LAKE_COMMIT, ...)` (the sync envelope) since `Lakehouse.run` is
    # synchronous; `wait_initial` opens moderately wide so a competing commit lands before the re-read.
    (
        "lake-commit",
        Policy(
            attempts=4, timeout=60.0, target=_named("CommitFailedError", "CommitFailedException", "CommitStateUnknownException"), wait_initial=0.2
        ),
    ),
    # the remote-database transport transient the `data:tabular/query#QUERY` `QueryEngine.run` `Remote`
    # leg rides through `guarded(RetryClass.REMOTE_DB, ...)`: an ADBC `OperationalError` whose
    # `status_code` is `TIMEOUT`/`IO` (the Flight SQL / ADBC manager driver call hitting a transient
    # gRPC/transport stall) plus the stdlib `ConnectionError`/`TimeoutError` a reconnection clears,
    # discriminated by the `_adbc_transient` hook so a non-transient ADBC fault the status code marks
    # permanent is not retried. The `WIRE`/`RPC` rows catch none of these (their `target`s are
    # `(ConnectionError,)` intra-mesh and the `_named` COMPAS XML-RPC qualnames), so the remote leg
    # imports module-top rather than `_named`. `wait_initial` opens moderately wide for a re-dial.
    ("remote-db", Policy(attempts=3, timeout=30.0, target=_adbc_transient(ConnectionError, TimeoutError), wait_initial=0.2)),
    # the out-of-core streaming transient the `data:tabular/query#QUERY` `QueryEngine.run` `Streaming`
    # (daft) leg rides through `guarded(RetryClass.STREAMING, ...)`: `daft.exceptions.DaftTransientError`
    # is the single Rust-backed base for the `ConnectTimeoutError`/`ReadTimeoutError`/`ByteStreamError`/
    # `SocketError`/`ThrottleError`/`MiscTransientError` object-store-and-network transients a re-scan
    # clears, plus the stdlib `TimeoutError`/`ConnectionError` — a bare tuple over the one transient base
    # of the daft Rust faults). `wait_max` widens for a long distributed scan without inflating attempts.
    ("streaming", Policy(attempts=3, timeout=120.0, target=(DaftTransientError, TimeoutError, ConnectionError), wait_max=30.0)),
])


# --- [COMPOSITION] ----------------------------------------------------------------------

# the one on-retry signal: a RetryHookFactory whose built hook mints the receipt fact and
# the child span from one RetryDetails payload (lazy build per the factory contract).
RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)

# the one stacked hook set EMIT registers — the receipt+span fact, the metrics-owned
# retry.attempts counter the observability/metrics#METRIC owner composes here (the single
# process-global registration that owns the on-retry signal), and the structlog warning,
# all three woven from one RetryDetails payload in one set_on_retry_hooks call.
RETRY_HOOKS: Final[tuple[RetryHook | RetryHookFactory, ...]] = (RetryReceiptHook, Metrics.retry_hook(), StructlogOnRetryHook)
```

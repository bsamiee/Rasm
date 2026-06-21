# [PY_RUNTIME_RESILIENCE]

The one resilience policy table for the whole branch. `RetryClass` is the single behavior-carrying `stamina`-backed enum the fault rail and the transport/concurrency/lane clusters consume — one member per retryable resource class, each member *carrying* its own policy row (attempts, total timeout budget, the exact retryable target) rather than a flag the caller re-derives, with exponential backoff and jitter riding the `stamina` defaults. The `on=` target is the full `stamina` `ExcOrBackoffHook` discriminator, so a server-rate-limited class binds a `Retry-After` backoff hook in the same column an exception-set class binds a tuple — one axis, two shapes, never a parallel sleep path. The reusable bound caller is the only memoised projection (`stamina` opens a fresh internal `retry_context` on each `__call__`, so one bound caller serves every call site for its class), resolved through a member-keyed `functools.cache`; the inline `retry_context` iterator is one-shot and is rebuilt per call, never cached — caching an async iterator would exhaust on the second drive. `guard(cls)` is the one canonical surface every sibling consumes — it returns the held `stamina.BoundAsyncRetryingCaller` the call site drives as `guard(cls)(fn, *args, **kwargs)`, the `(callable, *args, **kw)` precise signature `stamina`'s `.on(...)` bound caller preserves; `guarded` is the output-parameterized convenience that additionally fuses the `reliability/faults#FAULT` boundary lift and the OTel span-around-the-retries into a single `RuntimeRail[T]` for the leg that wants the rail composed here rather than at the call site; `retrying(cls)` is the inline-context shape for blocks the caller cannot pre-shape as a coroutine. The on-retry signal registers once as a `RetryHookFactory` that maps `RetryDetails` field-for-field onto `observability/receipts#RECEIPT` and the active span, so per-call retry logging, the receipt fact, and the span attributes are minted from one payload and never duplicated; recoverability over the resulting rail is never minted here — it stays the faults owner's `BoundaryFault.recoverable` at the caller.

## [01]-[INDEX]

- [01]-[RESILIENCE]: the one behavior-carrying `RetryClass` enum, its per-member policy row and the member-keyed cached `stamina` bound caller, the `ExcOrBackoffHook` backoff-hook arm, the `guard`/`guarded`/`retrying` triad over the one row (bound caller, railed convenience, inline context), and the one-shot `RetryReceiptHook` instrumentation parameterized over the run mode.

## [02]-[RESILIENCE]

- Owner: `RetryClass` — the single closed `StrEnum` resilience vocabulary whose every member carries a frozen `Policy` row over `stamina` (`attempts`, `timeout`, and the `ExcOrBackoffHook` `target`), resolving its row through the `policy` property; `Policy` the frozen `msgspec.Struct` row the member binds; `POLICY` the one `expression` `Map[str, Policy]` of typed rows the member resolves through by its string value; `_caller` the one `functools.cache`-memoised member-keyed projection minting the reusable `stamina.BoundAsyncRetryingCaller` once per class (kept off the member because an `Enum` instance carries no writable per-member `__dict__` for `functools.cached_property` to populate, and because only the reusable bound caller is safe to cache — the one-shot `retry_context` is rebuilt per call); `RetryAfter` the host-neutral structural protocol a server-rate-limit-carrying exception satisfies so the `HTTP` backoff hook reads a typed `retry_after` slot rather than introspecting a provider response shape; `RetryMode` the closed install-mode vocabulary; `RetryReceiptHook` the one `RetryHookFactory` mapping `stamina.instrumentation.RetryDetails` onto `observability/receipts#RECEIPT` and the active span. The fault, transport, lane, and concurrency clusters read resilience only through `guard`, `guarded`, and `retrying` — never a second retry owner.
- Cases: `RetryClass` members `OBJECT_STORE` · `HTTP` · `SSH` · `WIRE` · `SCAN`, each one row in `POLICY` carrying its own attempts, timeout budget, and `target` as behavior columns. The `target` column is the full `stamina` discriminator: `HTTP` binds a `BackoffHook` that honours a server-directed `Retry-After` delay (an exception satisfying the `RetryAfter` structural protocol the transport boundary populates from the `429`/`503` response header) before falling through to the `stamina` exponential schedule on its transient exception set, while `OBJECT_STORE`/`SSH`/`WIRE`/`SCAN` bind a bare retryable-exception tuple — `OBJECT_STORE` is the deliberate outer envelope over `obstore`'s Rust-core `RetryConfig`, so the stamina row catches only the `BaseError` transients that survive the store's internal retry and never re-introspects a provider header the store owns. The class is the key, the policy is the value, and the row carries its behavior including whether a server-rate-limit hook overrides the schedule, never a `streaming=True`-style flag the caller re-derives.
- Entry: `guard(cls)` is the one canonical entrypoint every sibling consumes — it returns the `functools.cache`-held `stamina.BoundAsyncRetryingCaller` the call site drives as `await guard(cls)(fn, *args, **kwargs)` (the transport `_http_body` leg's `guard(retry_class)(client.get, url, headers=headers)`, the lane `ADMIT_TABLE` `retried` row's `guard(unit.retried[0])(unit.retried[1])`, the wire/serve `guard(RetryClass.WIRE)(method, request)`, the object-store `guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, relative)`, and the context-manager-returning `await guard(retry_class)(sftp.open, relative, "rb")`), the precise `(callable, *args, **kw)` signature `stamina`'s `.on(...)` bound caller preserves so the call site passes only the callable and its arguments. `guarded(cls, thunk, *, subject)` is the railed convenience parameterized on output as `RuntimeRail[T]` — it drives the same held caller around the awaitable-returning thunk inside one `start_as_current_span` retry span and lifts the terminal raise through `reliability/faults#FAULT` `async_boundary` exactly once, so a budget-exhausted transient class surfaces as the `boundary` case naming the final cause and a non-transient raise the row never named surfaces immediately; the call site receives the rail, never a raw exception, and decides recovery through the faults owner's `BoundaryFault.recoverable` against its own code set rather than a second classification minted here — `guarded` is for the leg that wants the span and the fault lift composed in resilience, while the transport/wire/serve legs that already open their own span and run their own `async_boundary` fence at the call site compose the bare `guard(cls)` caller inside it, never a doubled span or doubled lift. `retrying(cls)` rebuilds the `stamina.retry_context` iterator from the same row per call (the iterator is one-shot, never cached) for inline blocks the caller cannot pre-shape as a coroutine, driven as `async for attempt in retrying(cls): with attempt: ...`, each `attempt.num`/`attempt.next_wait` available for inline instrumentation — the same policy row, three native `stamina` application shapes (bound caller, railed thunk, inline context), never a hand-rolled loop.
- Auto: the reusable bound caller is the one memoised projection — `_caller(cls)` resolves the member's `Policy` through `POLICY[self.value]`, constructs `AsyncRetryingCaller(attempts, timeout).on(target)` once, and holds it under a member-keyed `functools.cache` (kept a module-level resolver rather than a `cached_property` because an `Enum` member exposes no writable per-instance `__dict__` the descriptor could populate, so a `cached_property` on the enum is a phantom that fails at first touch); the inline `retry_context` iterator is rebuilt per call in `retrying` because it is a single-use async iterator that exhausts on a second drive, so caching it would silently yield an empty loop — the `Map` is the single source of row truth both shapes derive from, the binding paid once and the iterator minted fresh. `Policy.target` is one positional `ExcOrBackoffHook` argument to both `stamina` shapes — a tuple of retryable exceptions or a `BackoffHook`, never a spread; exponential backoff with jitter and the cumulative timeout ride the `stamina` defaults whenever the schedule is not overridden by a hook return. `install(mode)` is the one-shot instrumentation/toggle entry parameterized over `RetryMode` with an `assert_never` tail proving the match total over the closed enum: `EMIT` registers the stacked hook set `(RetryReceiptHook, StructlogOnRetryHook)` through `set_on_retry_hooks` so the on-retry signal mints both the structured `observability/receipts#RECEIPT` fact and the structlog `stamina` warning in one registration; `SILENT` passes the empty iterable to deactivate instrumentation; `TEST` calls `set_testing(True)` to collapse backoff and cap attempts for deterministic specs — production code branches on neither `is_active` nor `is_testing`, the mode is selected once at the composition root. `RetryReceiptHook` is the `RetryHookFactory` whose built hook reads the `RetryDetails` payload (`name`, `retry_num`, `wait_for`, `waited_so_far`, `caused_by`) field-for-field, mints a `planned`-phase fact through the receipts owner's two-argument `Receipt.of(owner, evidence)` factory (`Receipt.of("resilience", ("planned", name, facts))`, the `(Phase, subject, facts)` evidence tuple the `fact` case routes — never a four-positional shape the receipts owner does not expose), sets the child span's `Status(StatusCode.ERROR, caused_by)` so the retry span carries the triggering cause as its status, and returns the OTel span context-manager that wraps the scheduled wait so each retry is a child span entered when the retry is scheduled and exited right before the retry runs — the receipt slots and the span status are recorded from the one payload, never re-derived.
- Packages: `stamina` (`AsyncRetryingCaller`, `BoundAsyncRetryingCaller`, `retry_context`, `Attempt`, `ExcOrBackoffHook`, `BackoffHook`, `set_testing`, `instrumentation.set_on_retry_hooks`, `instrumentation.StructlogOnRetryHook`, `instrumentation.RetryHook`, `instrumentation.RetryHookFactory`, `instrumentation.RetryDetails`), `obstore` (`exceptions.BaseError` the `OBJECT_STORE` row's transient base — the store's typed root the stamina envelope catches after the Rust-core `RetryConfig`, never `OSError` which would miss every obstore fault), `expression` (`Map.of_seq`, `Map.empty`, `Map.__getitem__`, `Map.add`), `opentelemetry-api` (`trace.get_tracer` ENTRYPOINTS [03], `Tracer.start_as_current_span` ENTRYPOINTS [02], `Tracer.start_span` ENTRYPOINTS [01], `trace.use_span` ENTRYPOINTS [08], `Span.set_attribute` ENTRYPOINTS [03], `Span.set_status`/`Status`/`StatusCode` ENTRYPOINTS [06]/PUBLIC_TYPES [11]), `reliability/faults#FAULT` (`RuntimeRail`, `async_boundary`), `observability/receipts#RECEIPT` (`Receipt.of(owner, evidence)`, `Redaction`, `Signals.emit`), `msgspec` (`Struct`), `functools` (`cache`), `typing` (`Protocol`, `runtime_checkable`, `assert_never`).
- Growth: a new retryable class is one `RetryClass` member plus one `POLICY.add` row; the `_caller` bound-caller cache, the inline `retrying` context, the railed `guarded`, and the receipt hook all derive from that row with zero new surface. A presign-vs-payload split, a distinct circuit envelope, or a `Retry-After`-honouring variant of an existing class is one new member with its own `target` column — never a `guard`/`guarded` parameter or a parallel method. `POLICY` carries exactly one row per `RetryClass` member, so `_caller`, `guard`, `guarded`, and `retrying` are total over the closed `StrEnum` by construction — a `Map.__getitem__` miss can only arise from adding a member without its `POLICY.add`, a build/wiring error rather than a domain fault, which is the single way to break totality.
- Boundary: product outbound resilience, circuit breaking, and hop policy stay AppHost-owned; a manual retry loop with hand-coded `sleep` backoff, a parallel `Retry-After` sleep path outside the `on=` hook, resilience introspecting an httpx/provider response shape to read `Retry-After` (the transport owner maps that header into the host-neutral `RetryAfter` slot resilience reads), a second stamina envelope duplicating `obstore`'s Rust-core `RetryConfig`, and blanket exception retrying are the deleted forms; re-binding a caller per call is likewise deleted, the binding is memoised once in the member-keyed `_caller` cache (a `functools.cached_property` on the enum is itself a deleted form — an `Enum` member has no writable per-instance `__dict__` to back it), while the one-shot `retry_context` is deliberately rebuilt per call because caching a single-use async iterator silently exhausts it; backoff geometry is a deliberate branch-wide invariant, not an unstated omission — `wait_initial`, `wait_max`, `wait_jitter`, and `wait_exp_base` ride one uniform `stamina` default across every class so the only per-class schedule axes are `attempts`, `timeout`, and whether the `target` hook overrides the schedule, and a class needing distinct wait geometry adds those columns to `Policy` rather than tuning a caller knob; a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` is not an exception and is never retried — retry triggers only on the raised transient exceptions or the hook the row's `target` names, the `guarded` rail lifts the terminal raise through `async_boundary` exactly once, and the recoverability decision over the resulting rail stays the faults owner's `BoundaryFault.recoverable` at the caller, never a duplicate classification minted in resilience. The receipt and span are minted by the registered `RetryReceiptHook`, never by a second tracer or a per-call log line.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import AbstractContextManager
from datetime import timedelta
from enum import StrEnum
from functools import cache
from typing import Final, Protocol, assert_never, runtime_checkable

import stamina
from expression.collections import Map
from msgspec import Struct
from obstore.exceptions import GenericError as ObjectStoreTransient
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from stamina.instrumentation import (
    RetryDetails,
    RetryHook,
    RetryHookFactory,
    StructlogOnRetryHook,
    set_on_retry_hooks,
)

from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, Signals

_TRACER: Final = trace.get_tracer("rasm.runtime.resilience")
_RETRY_FACTS: Final = Redaction(classified=Map.empty())


@runtime_checkable
class RetryAfter(Protocol):
    retry_after: float | None


def _retry_after(*on: type[Exception]) -> stamina.BackoffHook:
    transient = on or (TimeoutError, ConnectionError)

    def backoff(exc: Exception) -> bool | float | timedelta:
        match exc:
            case RetryAfter(retry_after=float() as seconds):
                return seconds
            case _:
                return isinstance(exc, transient)

    return backoff


class Policy(Struct, frozen=True):
    attempts: int
    timeout: float
    target: stamina.ExcOrBackoffHook


# the one row-per-member policy table; the bare-string key is the `RetryClass.value`
# the member resolves through, so a new class is one member plus one `POLICY.add` row.
POLICY: Final[Map[str, Policy]] = Map.of_seq([
    ("object-store", Policy(attempts=4, timeout=30.0, target=(ObjectStoreError, TimeoutError))),
    ("http", Policy(attempts=3, timeout=20.0, target=_retry_after(TimeoutError, ConnectionError))),
    ("ssh", Policy(attempts=3, timeout=30.0, target=(ConnectionError, TimeoutError))),
    ("wire", Policy(attempts=5, timeout=15.0, target=(ConnectionError,))),
    ("scan", Policy(attempts=2, timeout=60.0, target=(OSError,))),
])


class RetryClass(StrEnum):
    OBJECT_STORE = "object-store"
    HTTP = "http"
    SSH = "ssh"
    WIRE = "wire"
    SCAN = "scan"

    @property
    def policy(self) -> Policy:
        return POLICY[self.value]


# the reusable bound caller is memoised per member (each `__call__` opens a fresh internal
# `retry_context`, so the binding is paid once and reused); the inline `retry_context`
# iterator is one-shot and is rebuilt per call — never cached.
@cache
def _caller(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    row = cls.policy
    return stamina.AsyncRetryingCaller(attempts=row.attempts, timeout=row.timeout).on(row.target)


class RetryMode(StrEnum):
    EMIT = "emit"
    SILENT = "silent"
    TEST = "test"


def _retry_receipt() -> RetryHook:
    def hook(details: RetryDetails) -> AbstractContextManager[trace.Span]:
        cause = type(details.caused_by).__name__
        Signals.emit(
            Receipt.of("resilience", ("planned", details.name, {
                "retry_num": str(details.retry_num),
                "wait_for": f"{details.wait_for:.3f}",
                "waited_so_far": f"{details.waited_so_far:.3f}",
                "caused_by": cause,
            })),
            _RETRY_FACTS,
        )
        span = _TRACER.start_span("resilience.retry", attributes={
            "rasm.retry_num": details.retry_num,
            "rasm.wait_for": details.wait_for,
            "rasm.caused_by": cause,
        })
        span.set_status(Status(StatusCode.ERROR, cause))
        return trace.use_span(span, end_on_exit=True)

    return hook


RetryReceiptHook: Final = RetryHookFactory(hook_factory=_retry_receipt)


def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    return _caller(cls)


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    row = cls.policy
    return stamina.retry_context(on=row.target, attempts=row.attempts, timeout=row.timeout)


async def guarded[T](cls: RetryClass, thunk: Callable[[], Awaitable[T]], *, subject: str) -> RuntimeRail[T]:
    async def attempt() -> T:
        with _TRACER.start_as_current_span("resilience.guarded", attributes={"rasm.retry_class": cls.value}):
            return await guard(cls)(thunk)

    return await async_boundary(subject, attempt)


def install(mode: RetryMode = RetryMode.EMIT) -> None:
    match mode:
        case RetryMode.EMIT:
            set_on_retry_hooks((RetryReceiptHook, StructlogOnRetryHook))
        case RetryMode.SILENT:
            set_on_retry_hooks(())
        case RetryMode.TEST:
            stamina.set_testing(True)
        case _ as unreachable:
            assert_never(unreachable)
```

# [PY_RUNTIME_RESILIENCE]

The one resilience policy table for the whole branch. `Retry` is the single `stamina`-backed policy table the fault rail and the transport/concurrency clusters consume — one row per retryable resource class carrying attempts, the total timeout budget, and the exact retryable exception set, with exponential backoff and jitter riding the `stamina` defaults. The typed rows live in one persistent `Map` keyed by class; the bound callers are resolved from those rows exactly once into a second persistent `Map` so no call site reconstructs a caller; the on-retry signal registers once and feeds the receipt surface; and non-transient faults surface immediately as `Error(BoundaryFault)` and are never retried.

## [01]-[INDEX]

- [01]-[RESILIENCE]: the one `Retry` policy table, one persistent-map row per retryable class, the bound-caller and inline-context entries resolved once, and the one-shot on-retry instrumentation.

## [02]-[RESILIENCE]

- Owner: `Retry` — the one resilience policy row over `stamina`, carrying the retryable class, attempts, the total timeout budget, and the exact retryable exception set; `RetryClass` the closed `StrEnum` vocabulary; `POLICIES` the one `expression` `Map[RetryClass, Retry]` of typed rows, and `BOUND` the one `Map[RetryClass, BoundAsyncRetryingCaller]` the fault, transport, and concurrency clusters read through `guard` — never a second retry owner.
- Cases: `RetryClass` rows `OBJECT_STORE` · `HTTP` · `SSH` · `WIRE` · `SCAN`, each one row in `POLICIES` carrying its own attempts, timeout budget, and exception set as behavior columns — the class is the key, the policy is the value, and the row carries its behavior rather than a flag the caller re-derives.
- Entry: `guard(cls)` returns the `stamina` `BoundAsyncRetryingCaller` resolved once into `BOUND`, a single `Map` lookup with no per-call construction; the call site applies the held caller to the fallible coroutine as `await guard(cls)(coro, *args)`. `retrying(cls)` returns the `stamina.retry_context` async iterator built from the same typed row for inline blocks the caller cannot pre-shape as a coroutine, driven as `async for attempt in retrying(cls): with attempt: ...` — the same policy row, the two native `stamina` application shapes, never a hand-rolled loop.
- Auto: `Retry.caller` constructs the `AsyncRetryingCaller(attempts, timeout)` once and binds the exception set through `.on(self.on)` — the tuple of retryable exceptions is one positional argument, never a spread; `Retry.context` projects the same row onto `stamina.retry_context(on, attempts, timeout)`; exponential backoff with jitter and the cumulative timeout ride the `stamina` defaults on both shapes. `BOUND` materializes `POLICIES` into bound callers a single time at module load, so binding is paid once per class, not once per call. `instrument` registers `StructlogOnRetryHook()` once through `set_on_retry_hooks`, feeding `observability/receipts#RECEIPT`, so per-call retry logging is never duplicated.
- Packages: `stamina` (`AsyncRetryingCaller`, `BoundAsyncRetryingCaller`, `retry_context`, `Attempt`, `instrumentation.set_on_retry_hooks`, `instrumentation.StructlogOnRetryHook`), `expression` (`Map`), `msgspec`.
- Growth: a new retryable class is one `RetryClass` row plus one `POLICIES.add` entry; `BOUND` derives from `POLICIES` via `POLICIES.map` so the bound map follows automatically; zero new surface. `POLICIES` carries exactly one row per `RetryClass` member, so `BOUND`, `guard`, and `retrying` are total over the closed `StrEnum` by construction — a `Map.__getitem__` miss can only arise from adding a `RetryClass` case without its `POLICIES.add`, a build/wiring error rather than a domain fault, which is the single way to break totality.
- Boundary: product outbound resilience, circuit breaking, and hop policy stay AppHost-owned; a manual retry loop with hand-coded `sleep` backoff and blanket exception retrying are the deleted forms; re-binding a caller per call is likewise deleted, the binding is paid once into `BOUND`; backoff geometry is a deliberate branch-wide invariant, not an unstated omission — `wait_initial`, `wait_max`, `wait_jitter`, and `wait_exp_base` ride one uniform `stamina` default across every class so the only per-class policy axes are `attempts` and `timeout`, and a class needing distinct wait geometry adds those columns to `Retry` rather than tuning a caller knob; a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` is not an exception and is never retried — retry triggers only on the raised transient exceptions the row names, and the boundary maps the final outcome back to the rail.

```python signature
from collections.abc import AsyncIterator
from enum import StrEnum
from typing import Final

import stamina
from expression.collections import Map
from msgspec import Struct


class RetryClass(StrEnum):
    OBJECT_STORE = "object-store"
    HTTP = "http"
    SSH = "ssh"
    WIRE = "wire"
    SCAN = "scan"


class Retry(Struct, frozen=True):
    cls: RetryClass
    attempts: int
    timeout: float
    on: tuple[type[Exception], ...]

    @property
    def caller(self) -> stamina.BoundAsyncRetryingCaller:
        return stamina.AsyncRetryingCaller(attempts=self.attempts, timeout=self.timeout).on(self.on)

    @property
    def context(self) -> AsyncIterator[stamina.Attempt]:
        return stamina.retry_context(on=self.on, attempts=self.attempts, timeout=self.timeout)


POLICIES: Final[Map[RetryClass, Retry]] = Map.of_seq([
    (RetryClass.OBJECT_STORE, Retry(RetryClass.OBJECT_STORE, attempts=4, timeout=30.0, on=(OSError, TimeoutError))),
    (RetryClass.HTTP, Retry(RetryClass.HTTP, attempts=3, timeout=20.0, on=(TimeoutError, ConnectionError))),
    (RetryClass.SSH, Retry(RetryClass.SSH, attempts=3, timeout=30.0, on=(ConnectionError, TimeoutError))),
    (RetryClass.WIRE, Retry(RetryClass.WIRE, attempts=5, timeout=15.0, on=(ConnectionError,))),
    (RetryClass.SCAN, Retry(RetryClass.SCAN, attempts=2, timeout=60.0, on=(OSError,))),
])

BOUND: Final[Map[RetryClass, stamina.BoundAsyncRetryingCaller]] = POLICIES.map(lambda _, row: row.caller)


def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    return BOUND[cls]


def retrying(cls: RetryClass) -> AsyncIterator[stamina.Attempt]:
    return POLICIES[cls].context


def instrument() -> None:
    stamina.instrumentation.set_on_retry_hooks([stamina.instrumentation.StructlogOnRetryHook()])
```

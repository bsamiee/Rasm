# [PY_RUNTIME_RESILIENCE]

The one resilience policy table for the whole branch. `Retry` is the single `stamina`-backed policy table the fault rail and the transport/concurrency clusters consume — one row per retryable resource class carrying attempts, the total timeout budget, and the exact retryable exception set, with exponential backoff and jitter riding the `stamina` defaults. The policy lives in one persistent `Map` keyed by class, the on-retry signal registers once and feeds the receipt surface, and non-transient faults surface immediately as `Error(BoundaryFault)` and are never retried.

## [1]-[INDEX]

One cluster: `[2]-[RESILIENCE]` — the one `Retry` policy table, one persistent-map row per retryable class, the bound-caller entry, and the one-shot on-retry instrumentation.

## [2]-[RESILIENCE]

- Owner: `Retry` — the one resilience policy row over `stamina`, carrying the retryable class, attempts, the total timeout budget, and the exact retryable exception set; `RetryClass` the closed `StrEnum` vocabulary; `POLICIES` the one `expression` `Map[RetryClass, Retry]` the fault, transport, and concurrency clusters read through `guard`, never a second retry owner.
- Cases: `RetryClass` rows `OBJECT_STORE` · `HTTP` · `SSH` · `WIRE` · `SCAN`, each one row in `POLICIES` carrying its own attempts, timeout budget, and exception set as behavior columns — the class is the key, the policy is the value, and the row carries its behavior rather than a flag the caller re-derives.
- Entry: `guard(cls)` reads the row from `POLICIES` and returns the `stamina` `BoundAsyncRetryingCaller` for it; the call site applies the bound caller to the fallible coroutine as `await guard(cls)(coro, *args)`, so the retry policy is a value the caller holds, never a wrapper re-typed per call.
- Auto: `Retry.caller` constructs the `AsyncRetryingCaller(attempts, timeout)` and binds the exception set through `.on(self.on)` — the tuple of retryable exceptions is one positional argument, never a spread; exponential backoff with jitter and the cumulative timeout ride the `stamina` defaults; `instrument` registers `StructlogOnRetryHook()` once through `set_on_retry_hooks`, feeding `observability/receipts#RECEIPT`, so per-call retry logging is never duplicated.
- Packages: `stamina` (`AsyncRetryingCaller`, `BoundAsyncRetryingCaller`, `instrumentation.set_on_retry_hooks`, `instrumentation.StructlogOnRetryHook`), `expression` (`Map`), `msgspec`.
- Growth: a new retryable class is one `RetryClass` row plus one `POLICIES.add` entry; zero new surface.
- Boundary: product outbound resilience, circuit breaking, and hop policy stay AppHost-owned; a manual retry loop with hand-coded `sleep` backoff and blanket exception retrying are the deleted forms; a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` is not an exception and is never retried — retry triggers only on the raised transient exceptions the row names, and the boundary maps the final outcome back to the rail.

```python signature
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


POLICIES: Final[Map[RetryClass, Retry]] = Map.of_seq([
    (RetryClass.OBJECT_STORE, Retry(RetryClass.OBJECT_STORE, attempts=4, timeout=30.0, on=(OSError, TimeoutError))),
    (RetryClass.HTTP, Retry(RetryClass.HTTP, attempts=3, timeout=20.0, on=(TimeoutError, ConnectionError))),
    (RetryClass.SSH, Retry(RetryClass.SSH, attempts=3, timeout=30.0, on=(ConnectionError, TimeoutError))),
    (RetryClass.WIRE, Retry(RetryClass.WIRE, attempts=5, timeout=15.0, on=(ConnectionError,))),
    (RetryClass.SCAN, Retry(RetryClass.SCAN, attempts=2, timeout=60.0, on=(OSError,))),
])


def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    return POLICIES[cls].caller


def instrument() -> None:
    stamina.instrumentation.set_on_retry_hooks([stamina.instrumentation.StructlogOnRetryHook()])
```

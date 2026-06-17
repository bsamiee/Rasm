# [PY_RUNTIME_RESILIENCE]

The one resilience policy table for the whole branch. `Retry` is the single `stamina`-backed policy table the fault rail and the transport/concurrency clusters consume — one row per retryable resource class carrying attempts, the total timeout budget, and the exact retryable exception set, with exponential backoff and jitter riding the `stamina` defaults. The on-retry signal registers once and feeds the receipt surface; non-transient faults surface immediately as `Error(BoundaryFault(...))` and are never retried.

## [1]-[INDEX]

One cluster: `[2]-[RESILIENCE]` — the one `Retry` policy table, one row per retryable class.

## [2]-[RESILIENCE]

- Owner: `Retry` — the one resilience policy table over `stamina`, one row per retryable resource class carrying attempts, the total timeout budget, and the exact retryable exception set; the fault rail and the transport/concurrency clusters consume `Retry` rows through `guard`, never a second retry owner.
- Cases: `OBJECT_STORE` · `HTTP` · `SSH` · `WIRE` · `SCAN` — frozen rows on the `RetryClass` `StrEnum`, each mapping to one `Retry` policy in the frozen table.
- Entry: `guard(cls)` returns the `stamina` `BoundAsyncRetryingCaller` for the row's policy; the call site applies the bound caller to the fallible coroutine, so the retry policy is a value the caller holds, never a wrapper re-typed per call.
- Auto: `instrument` registers `StructlogOnRetryHook` once through `set_on_retry_hooks`, feeding the `observability/receipts#RECEIPT` surface, so per-call retry logging is not duplicated; exponential backoff with jitter rides the `stamina` defaults.
- Packages: `stamina` (`AsyncRetryingCaller`, `BoundAsyncRetryingCaller`, `instrumentation.set_on_retry_hooks`, `instrumentation.StructlogOnRetryHook`), `expression`, `msgspec`.
- Growth: a new retryable class is one `RetryClass` row plus one `Retry` policy entry in the frozen table; zero new surface.
- Boundary: product outbound resilience, circuit breaking, and hop policy stay AppHost-owned; a manual retry loop with hand-coded `sleep` backoff and blanket exception retrying are the deleted forms; non-transient faults surface immediately as `Error(BoundaryFault(...))` from `reliability/faults#FAULT` and are never retried.

```python signature
from builtins import frozendict
from enum import StrEnum
from typing import Final

import stamina
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
        return stamina.AsyncRetryingCaller(attempts=self.attempts, timeout=self.timeout).on(*self.on)


POLICIES: Final[frozendict[RetryClass, Retry]] = frozendict({
    RetryClass.OBJECT_STORE: Retry(RetryClass.OBJECT_STORE, attempts=4, timeout=30.0, on=(OSError, TimeoutError)),
    RetryClass.HTTP: Retry(RetryClass.HTTP, attempts=3, timeout=20.0, on=(TimeoutError, ConnectionError)),
    RetryClass.SSH: Retry(RetryClass.SSH, attempts=3, timeout=30.0, on=(ConnectionError, TimeoutError)),
    RetryClass.WIRE: Retry(RetryClass.WIRE, attempts=5, timeout=15.0, on=(ConnectionError,)),
    RetryClass.SCAN: Retry(RetryClass.SCAN, attempts=2, timeout=60.0, on=(OSError,)),
})


def guard(cls: RetryClass) -> stamina.BoundAsyncRetryingCaller:
    return POLICIES[cls].caller


def instrument() -> None:
    stamina.instrumentation.set_on_retry_hooks([stamina.instrumentation.StructlogOnRetryHook])
```

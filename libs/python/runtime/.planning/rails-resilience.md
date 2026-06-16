# [PY_RUNTIME_RAILS_RESILIENCE]

The single fault family, Result/Option rail, and resilience policy for the whole branch. `BoundaryFault` is the one tagged union every package raises through; `RuntimeRail` is the carrier that selects abort versus accumulate by its monadic algebra; `Retry` is the one `stamina`-backed policy table both the rails and the transport/lane clusters consume. Domain logic returns `Result`/`Option` and never raises; exceptions convert to a fault exactly once at the owning boundary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                        |
| :-----: | :--------- | :------------------------------------------------------------ |
|   [1]   | FAULT      | the boundary-fault tagged union, the rail carrier, conversion |
|   [2]   | RESILIENCE | one Retry policy table, rows per retryable class              |

## [2]-[FAULT]

- Owner: `BoundaryFault` — the single closed fault family discriminating config, resource, deadline, api, import, wire, and boundary cases; `RuntimeRail` the `Result`/`Option` alias every fallible function returns; `boundary` the one exception-to-fault conversion surface at ingress/egress.
- Cases: `Config` · `Resource` · `Deadline` · `Api` · `Import` · `Wire` · `Boundary` — each a frozen `case()` with the subject, the originating spelling, and the optional cause.
- Entry: `boundary` lifts a raised exception into `Error(BoundaryFault(...))` once; interior code receives only the rail.
- Packages: `expression` (`Result`/`Option`/`tagged_union`/`case`), `beartype`, `msgspec`.
- Growth: a new fault class is one `case()` on `BoundaryFault`; the abort/accumulate choice is the rail carrier, never a flag; zero new surface.
- Boundary: no C# `Expected` clone, no product receipt minting, no broad exception taxonomy copied from C# owners; the `try`/`except` lives only inside `boundary`, never in domain flow — a sentinel return or a `None`-as-failure is the deleted form.

```python signature
from collections.abc import Callable
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union


@tagged_union(frozen=True)
class BoundaryFault:
    tag: Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary"] = tag()
    config: tuple[str, str] = case()
    resource: tuple[str, str] = case()
    deadline: tuple[str, float] = case()
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()

    @staticmethod
    def Config(subject: str, detail: str) -> "BoundaryFault":
        return BoundaryFault(config=(subject, detail))

    @staticmethod
    def Resource(subject: str, detail: str) -> "BoundaryFault":
        return BoundaryFault(resource=(subject, detail))

    @staticmethod
    def Wire(subject: str, code: int) -> "BoundaryFault":
        return BoundaryFault(wire=(subject, code))

    @staticmethod
    def Boundary(subject: str, detail: str) -> "BoundaryFault":
        return BoundaryFault(boundary=(subject, detail))

    @staticmethod
    def of(subject: str, cause: BaseException) -> "BoundaryFault":
        return BoundaryFault(boundary=(subject, type(cause).__name__))


type RuntimeRail[T] = Result[T, BoundaryFault]


def boundary[T](subject: str, thunk: Callable[[], T], *, retryable: type[BaseException] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except retryable as cause:
        return Error(BoundaryFault.of(subject, cause))
```

## [3]-[RESILIENCE]

- Owner: `Retry` — the one resilience policy table over `stamina`, one row per retryable resource class carrying attempts, backoff cap, jitter, and the exact retryable exception set; both the rails and the transport/lane clusters consume `Retry` rows, never a second retry owner.
- Cases: `OBJECT_STORE` · `HTTP` · `SSH` · `WIRE` · `SCAN` — frozen rows on the `RetryClass` `StrEnum`, each mapping to a `Retry` policy.
- Entry: `Retry.guard` returns a `stamina`-configured async caller bound to the row's policy; `Retry.context` yields the inline `retry_context` for a statement block.
- Auto: the on-retry signal registers once through `stamina.instrumentation.set_on_retry_hooks([StructlogOnRetryHook()])`, feeding the `observability#RECEIPT` surface; per-call retry logging is not duplicated.
- Packages: `stamina` (`retry`, `retry_context`, `AsyncRetryingCaller`, `instrumentation`), `expression`.
- Growth: a new retryable class is one `RetryClass` row plus one `Retry` policy entry in the frozen table; zero new surface.
- Boundary: product outbound resilience, circuit breaking, and hop policy stay AppHost-owned; a manual retry loop with hand-coded `sleep` backoff and blanket exception retrying are the deleted forms; non-transient faults surface immediately as `Error(BoundaryFault(...))` and are never retried.

```python signature
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

    def guard[**P, T](self, fn: "stamina.typing.RetryProtocol[P, T]") -> stamina.AsyncRetryingCaller:
        return stamina.AsyncRetryingCaller(attempts=self.attempts, timeout=self.timeout).on(*self.on)


POLICIES: Final[dict[RetryClass, Retry]] = {
    RetryClass.OBJECT_STORE: Retry(RetryClass.OBJECT_STORE, attempts=4, timeout=30.0, on=(OSError, TimeoutError)),
    RetryClass.HTTP: Retry(RetryClass.HTTP, attempts=3, timeout=20.0, on=(TimeoutError, ConnectionError)),
    RetryClass.SSH: Retry(RetryClass.SSH, attempts=3, timeout=30.0, on=(ConnectionError, TimeoutError)),
    RetryClass.WIRE: Retry(RetryClass.WIRE, attempts=5, timeout=15.0, on=(ConnectionError,)),
    RetryClass.SCAN: Retry(RetryClass.SCAN, attempts=2, timeout=60.0, on=(OSError,)),
}
```

## [4]-[RESEARCH]

- [STAMINA_TYPING]: the `stamina.typing.RetryProtocol` parameter spelling for `guard` is verified against the installed `stamina==26.1.0` typing module; the `AsyncRetryingCaller.on` arity confirms at fence transcription.

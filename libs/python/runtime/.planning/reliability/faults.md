# [PY_RUNTIME_FAULTS]

The single fault family and Result/Option rail for the whole branch. `BoundaryFault` is the one tagged union every package raises through; `RuntimeRail` is the carrier that selects abort versus accumulate by its monadic algebra; `boundary` is the single exception-to-fault conversion surface. Domain logic returns `Result`/`Option` and never raises; exceptions convert to a fault exactly once at the owning boundary, and interior code receives only the rail.

## [1]-[INDEX]

One cluster: `[2]-[FAULT]` — the boundary-fault tagged union, the rail carrier, the one exception-to-fault conversion.

## [2]-[FAULT]

- Owner: `BoundaryFault` — the single closed fault family discriminating config, resource, deadline, api, import, wire, and boundary cases; `RuntimeRail` the `Result`/`Option` alias every fallible function returns; `boundary` the one exception-to-fault conversion surface at ingress/egress.
- Cases: `Config` · `Resource` · `Deadline` · `Api` · `Import` · `Wire` · `Boundary` — each a frozen `case()` with the subject, the originating spelling, and the optional cause.
- Entry: `boundary` lifts a raised exception into `Error(BoundaryFault(...))` once; the abort/accumulate choice rides the rail carrier algebra, never a flag.
- Packages: `expression` (`Result`/`Option`/`tagged_union`/`case`/`tag`), `beartype`, `msgspec`.
- Growth: a new fault class is one `case()` on `BoundaryFault`; zero new surface.
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
    def Deadline(subject: str, budget: float) -> "BoundaryFault":
        return BoundaryFault(deadline=(subject, budget))

    @staticmethod
    def Api(subject: str, detail: str) -> "BoundaryFault":
        return BoundaryFault(api=(subject, detail))

    @staticmethod
    def Import(subject: str, detail: str) -> "BoundaryFault":
        return BoundaryFault(import_=(subject, detail))

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


def boundary[T](subject: str, thunk: Callable[[], T], *, catch: type[BaseException] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(BoundaryFault.of(subject, cause))
```

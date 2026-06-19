# [PY_RUNTIME_FAULTS]

The single fault family and Result/Option rail for the whole branch. `BoundaryFault` is the one tagged union every package returns through, discriminating the seven ingress classes and carrying an `Aggregate` case so an accumulating boundary keeps every member structurally addressable; `RuntimeRail` is the carrier whose monadic algebra selects abort versus accumulate; `boundary`/`async_boundary` is the single exception-to-fault conversion surface, one sync and one awaitable entry over the same fault lift so a subprocess-seam or coroutine boundary never grows a parallel async rail. Domain logic returns `Result`/`Option` and never raises; exceptions convert to a fault exactly once at the owning boundary, and interior code receives only the rail.

## [1]-[INDEX]

- [1]-[FAULT]: the boundary-fault tagged union with the aggregate combination law, the rail carrier, the fail-fast and accumulating traversals, the one sync/async exception-to-fault conversion.

## [2]-[FAULT]

- Owner: `BoundaryFault` — the single closed fault family discriminating `config`, `resource`, `deadline`, `api`, `import_`, `wire`, and `boundary` ingress classes plus an `aggregate` case holding typed members; `RuntimeRail` the `Result`/`Option` alias every fallible function returns; `boundary` the one exception-to-fault conversion at ingress/egress.
- Cases: each non-aggregate case is a frozen `case()` keyword-constructed with its typed payload — `config`/`resource`/`api`/`import_`/`boundary` carry `(subject, detail)`, `deadline` carries `(subject, budget)`, `wire` carries `(subject, code)`; `aggregate` carries `tuple[BoundaryFault, ...]` so the conjunctive boundary combines members without flattening to a string. `BoundaryFault.of` is the one factory with logic — it maps a caught exception's subject and type name into the `boundary` case.
- Entry: `boundary` lifts a raised exception into `Error(BoundaryFault.of(...))` exactly once, polymorphic over the thunk shape — a `Callable[[], T]` runs in-line and an awaitable-returning thunk is awaited before the lift, so a synchronous compute and a subprocess-seam (`anyio.to_process.run_sync`) or coroutine boundary share one fault conversion rather than a second async rail; `traversed` threads a `Block` of railed values, the `accumulate` flag selecting the `combine` fold over the first-`Error` short-circuit, so the abort/accumulate disposition is one parameter on the traversal, never a parallel rail.
- Auto: `combine` is the conjunctive combination law — each side spreads to its members (an aggregate's tuple, or itself as a singleton) and the two spreads concatenate into one aggregate, so a nested aggregate flattens and a leaf wraps without a per-arm branch; recovery keys on `fault.tag` or membership in `fault.aggregate`, never on a reconstructed message; `recoverable` folds the membership test over the aggregate spine; the carrier compares tag then payload, so a `set`/`Map` keyed on `RuntimeRail` distinguishes distinct faults rather than coalescing them.
- Packages: `expression` (`Result`/`Option`/`Block`/`tagged_union`/`case`/`tag`), `anyio` (`to_process.run_sync` the subprocess-seam the async boundary closes over), `beartype`, `msgspec`.
- Growth: a new fault class is one `case()` plus one row on the recovery membership set; the accumulation law absorbs it through the existing `aggregate` case with zero new surface.
- Boundary: no C# `Expected` clone, no product receipt minting, no broad exception taxonomy copied from C# owners; the `try`/`except` lives only inside `boundary`, never in domain flow; a sentinel return, a `None`-as-failure, a bare-`str` fault for the multi-cause domain, and an aggregate that joins members into one string are the deleted forms.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block


@tagged_union(frozen=True)
class BoundaryFault:
    tag: Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary", "aggregate"] = tag()
    config: tuple[str, str] = case()
    resource: tuple[str, str] = case()
    deadline: tuple[str, float] = case()
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()
    aggregate: tuple["BoundaryFault", ...] = case()

    @staticmethod
    def of(subject: str, cause: BaseException) -> "BoundaryFault":
        return BoundaryFault(boundary=(subject, type(cause).__name__))

    @staticmethod
    def combine(left: "BoundaryFault", right: "BoundaryFault") -> "BoundaryFault":
        members = lambda fault: fault.aggregate if fault.tag == "aggregate" else (fault,)
        return BoundaryFault(aggregate=(*members(left), *members(right)))

    def recoverable(self, codes: frozenset[str]) -> bool:
        match self:
            case BoundaryFault(tag="aggregate"):
                return any(member.recoverable(codes) for member in self.aggregate)
            case _:
                return self.tag in codes


type RuntimeRail[T] = Result[T, BoundaryFault]


def boundary[T](subject: str, thunk: Callable[[], T], *, catch: type[BaseException] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(BoundaryFault.of(subject, cause))


async def async_boundary[T](subject: str, thunk: Callable[[], Awaitable[T]], *, catch: type[BaseException] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(await thunk())
    except catch as cause:
        return Error(BoundaryFault.of(subject, cause))


def traversed[T](rails: Block[RuntimeRail[T]], *, accumulate: bool) -> RuntimeRail[Block[T]]:
    if not accumulate:
        seed: RuntimeRail[Block[T]] = Ok(Block.empty())
        return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
    faults = rails.choose(lambda rail: rail.swap().to_option())
    oks = rails.choose(lambda rail: rail.to_option())
    return Ok(oks) if faults.is_empty() else Error(faults.reduce(BoundaryFault.combine))
```

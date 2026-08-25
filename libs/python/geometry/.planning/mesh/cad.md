# [PY_GEOMETRY_MESH_CAD]

`CadClient` is geometry's client edge to the OCCT provider, and it is ONE entry over the whole generated `CadService` rpc set. One `CadRoute` policy value carries each rpc: its request and reply classes, the dial already woven with its own refusal capture, and the fault row that dial publishes under — so a second rpc is one route row and the entry, its callers, and the refusal projection stand untouched. Each unary reply carries an output `ArtifactRef`; the operation that consumes the body resolves that reference through `ArtifactTransfer.fetch` for exactly its own scope, and this boundary neither materializes the body nor extends a helper-owned path lifetime. Geometry owns no STEP parser, OCCT proxy, protocol mirror, stream state machine, or second triangle census.

`transport/shapes#VOCABULARY`'s `dialed` weave is the one client-edge refusal capture and arrives settled: the client-side admission phase lands on the route's own row and a peer `ConnectError` lifts whole through `remote_fault`, so no arm is re-spelled here and the two sibling wrappers that each carried their own copy of it are gone. `Deadline` is this seam's one reading of a caller budget, projected once into the wire's millisecond unit and threaded unchanged, so no call site re-derives a bound the value already carries.

## [01]-[INDEX]

- [02]-[BRIDGE]: `CadRoute` rosters every rpc, `CadClient.call` folds them into one entry, and `Deadline` reads a caller budget once.

## [02]-[BRIDGE]

- Owner: `CadClient.call` — the one entry over every generated `CadService` rpc, reached on a prebuilt generated client injected at composition. `CadServiceClient.tessellate` and `CadServiceClient.execute` are the two members it reaches, each bound once inside its own route.
- Cases: `TESSELLATE` carries `TessellateRequest` to `TessellateResponse` and `EXECUTE` carries `ExecuteRequest` to `ExecuteResponse`. Each route, never a method-name suffix or a mode flag, discriminates and carries its own behaviour: the caller hands the value and the entry invokes it.
- Law: `dialed` weaves ONCE, where the route is declared — each row is a property of its rpc, so re-weaving the same capture on every dial rebuilds it per call and re-opens the drift the shared weave closed. Every route row therefore declares `slots=("phase",)`, the coordinate that weave's own token fills, and the weave refuses any other at import.
- Law: `TessellateResponse.artifact` crosses whole. Body consumers open `ArtifactTransfer.fetch` around the operation that reads it; this client neither fetches speculatively nor retains a temporary path, so it holds no artifact custody and weaves no `custody` arm.
- Law: element and triangle counts arrive only from the provider's emitted-GLB census; geometry does not parse GLB a second time.
- Law: `Deadline` reads a budget ONCE and threads it as the value. Whole milliseconds are the wire unit, so a budget under one millisecond states a bound the transport cannot carry and REFUSES; the deleted `max(1, int(seconds * 1000.0))` floor silently widened such a budget to a full millisecond and turned an already-spent bound into a fresh one.
- Growth: a new `CadService` rpc is one `CadRoute` row beside one row on `RAISES`; the entry, every caller, and the refusal projection stand untouched, and a route whose reply class the caller does not name is unspellable.
- Boundary: STEP/IGES and sealed B-rep cross `CadService`; every body crosses `ArtifactService` by reference and no `OCP.*` import exists in this package. `mesh/brep#BREP` composes `EXECUTE` and owns the B-rep evidence projection; this page owns the seam alone and no evidence of its own.

```python
from collections.abc import Awaitable, Callable
from math import isfinite
from typing import Final, assert_never

from expression import Error, Ok, Option, Result
from expression.collections import Block
from msgspec import Struct
from protobuf import Message
# Contracts are retired from this logic.

from rasm.geometry.graduation import GeometryLeg
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.shapes import Dialed, dialed

# --- [TABLES] ---------------------------------------------------------------------------

CAD_TESSELLATE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.CAD,
    point="tessellate",
    arm="config",
    defect="cad-request-refused",
    retriability=TERMINAL,
    slots=("phase",),
)
BREP_EXECUTE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP,
    point="execute",
    arm="config",
    defect="cad-request-refused",
    retriability=TERMINAL,
    slots=("phase",),
)
CAD_BUDGET: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.CAD,
    point="budget",
    arm="config",
    defect="budget-unspellable",
    retriability=TERMINAL,
    slots=("seconds",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([CAD_TESSELLATE, BREP_EXECUTE, CAD_BUDGET]))

# --- [MODELS] ---------------------------------------------------------------------------


class Deadline(Struct, frozen=True, gc=False):
    milliseconds: int | None

    @staticmethod
    def of(budget: Option[float], /) -> RuntimeRail["Deadline"]:
        return budget.map(Deadline._spelled).default_value(Ok(Deadline(milliseconds=None)))

    @staticmethod
    def _spelled(seconds: float, /) -> RuntimeRail["Deadline"]:
        return (
            Ok(Deadline(milliseconds=int(seconds * 1000.0)))
            if isfinite(seconds) and seconds >= 0.001
            else Error(CAD_BUDGET.raised(repr(seconds)))
        )


class CadRoute[Q: Message, R: Message](Struct, frozen=True, gc=False):
    reply: type[R]
    dial: Dialed[[CadServiceClient, Q, int | None], R]


def _route[Q: Message, R: Message](
    reply: type[R], reach: Callable[[CadServiceClient], Callable[..., Awaitable[R]]], row: FaultRow[GeometryLeg], /
) -> CadRoute[Q, R]:
    @dialed(row)
    async def bound(client: CadServiceClient, request: Q, budget: int | None, /) -> R:
        return await reach(client)(request, timeout_ms=budget)

    return CadRoute(reply=reply, dial=bound)


TESSELLATE: Final[CadRoute[TessellateRequest, TessellateResponse]] = _route(
    TessellateResponse, lambda client: client.tessellate, CAD_TESSELLATE
)
EXECUTE: Final[CadRoute[ExecuteRequest, ExecuteResponse]] = _route(ExecuteResponse, lambda client: client.execute, BREP_EXECUTE)

# --- [SERVICES] -------------------------------------------------------------------------


class CadClient:
    def __init__(self, client: CadServiceClient) -> None:
        self._client = client

    async def call[Q: Message, R: Message](self, route: CadRoute[Q, R], request: Q, *, budget: Option[float]) -> RuntimeRail[R]:
        match Deadline.of(budget):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=deadline):
                return await route.dial(self._client, request, deadline.milliseconds)
            case _ as unreachable:
                assert_never(unreachable)
```

## [03]-[RESEARCH]

(none)

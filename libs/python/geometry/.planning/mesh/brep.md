# [PY_GEOMETRY_MESH_BREP]

Geometry's B-rep surface is a generated `ExecuteRequest` crossing, not a Python operation mirror. `apply` hands the typed construct, Boolean, profile, or feature arm to `mesh/cad#BRIDGE`'s one client entry under its `EXECUTE` route and projects the returned `ExecuteResponse` — the generated sealed STEP and `BrepKernelReceipt` — whole. Geometry contributes the provider evidence without inventing mesh-quality measures; it never unseals a native handle or repeats kernel evidence.

`mesh/cad#BRIDGE` settles the client seam, its refusal capture, and its budget reading: each route carries the rpc, its reply class, and the row its refusals publish under, so this page holds no dial, no `except` arm, and no fault row of its own. What survives here is the one thing that seam cannot own — how a returned kernel receipt becomes geometry's graduating evidence.

## [01]-[INDEX]

- [02]-[BREP]: `BrepResult` projects `ExecuteResponse` into evidence and `apply` drives it over the `EXECUTE` route.

## [02]-[BREP]

- Owner: `apply` — the evidence-scoped B-rep entry; `CadServiceClient.execute` is the generated member it reaches, bound once inside `mesh/cad#BRIDGE`'s `EXECUTE` route on a client injected at composition.
- Law: callers construct only generated operation arms; local verb enums, parameter tuples, request structs, and aliases are absent, and `ExecuteRequest.operation.field` names the evidence span rather than a second local verb roster.
- Law: generated `SealedStep` and `BrepKernelReceipt` return unchanged; B-rep evidence contributes directly and does not masquerade as a mesh-quality graduation.
- Receipt: `BrepResult.contribute` yields the one `emitted`-phase receipt off the returned kernel census, so receipt and provider evidence read one fold and geometry measures nothing twice.
- Boundary: `remote_fault` preserves the provider's typed `FaultDetail`, status, correlation, causal stamp, tenant, and recovery whole at the client edge `mesh/cad#BRIDGE` owns; this page never re-spells that capture. No native package or handle crosses Python 3.15.

```python signature
from collections.abc import Iterable
from functools import partial

from expression import Option
from msgspec import Struct
from rasm.contracts.rasm.contracts.cad.operations_pb import ExecuteRequest, ExecuteResponse

from rasm.geometry.graduation import EvidenceScope, GeometrySubject, evidence_run
from rasm.geometry.mesh.cad import EXECUTE, CadClient
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

# --- [MODELS] ---------------------------------------------------------------------------


class BrepResult(Struct, frozen=True, gc=False):
    reply: ExecuteResponse

    def contribute(self) -> Iterable[Receipt]:
        held = self.reply.receipt
        yield Receipt.of(
            "rasm.geometry.mesh.brep",
            (
                "emitted",
                GeometrySubject.MESH_ALGEBRA,
                {
                    "volume_m3": held.volume_m3,
                    "area_m2": held.area_m2,
                    "vertices": held.topology.vertices,
                    "edges": held.topology.edges,
                    "faces": held.topology.faces,
                    "solids": held.topology.solids,
                },
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(
    request: ExecuteRequest,
    client: CadClient,
    *,
    budget: Option[float],
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[BrepResult]:
    # `EXECUTE` discriminates and the client entry owns the whole dial; this page adds the evidence scope and
    # its receipt projection alone, so the sibling wrapper class that once re-spelled that capture here is gone.
    return (
        await evidence_run(
            EvidenceScope.MESH_BREP,
            f"apply.{request.operation.field}",
            partial(client.call, EXECUTE, request, budget=budget),
            composition=composition,
        )
    ).map(BrepResult)
```

## [03]-[RESEARCH]

(none)

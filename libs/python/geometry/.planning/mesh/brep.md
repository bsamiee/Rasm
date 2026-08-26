# [PY_GEOMETRY_MESH_BREP]

Geometry's B-rep surface is a generated `ExecuteRequest` crossing, not a Python operation mirror. `apply` hands the typed construct, Boolean, profile, or feature arm to `mesh/cad#BRIDGE`'s one client entry under its `EXECUTE` route and projects the returned `ExecuteResponse` — the generated sealed STEP and `BrepMeasure` — whole. Geometry stamps the provider's measures on its span without inventing mesh-quality measures; it never unseals a native handle or re-measures the kernel's census.

`mesh/cad#BRIDGE` settles the client boundary, its refusal capture, and its budget reading: each route carries the rpc, its reply class, and the row its refusals publish under, so this page holds no dial, no `except` arm, and no fault row of its own. What survives here is the one thing that boundary cannot own — how a returned kernel measure reaches geometry's span.

## [01]-[INDEX]

- [02]-[BREP]: `BrepResult` carries `ExecuteResponse` and projects its `BrepMeasure` onto the span; `apply` drives it over the `EXECUTE` route.

## [02]-[BREP]

- Owner: `apply` — the evidence-scoped B-rep entry; `CadServiceClient.execute` is the generated member it reaches, bound once inside `mesh/cad#BRIDGE`'s `EXECUTE` route on a client injected at composition.
- Law: callers construct only generated operation arms; local verb enums, parameter tuples, request structs, and aliases are absent, and `ExecuteRequest.operation.field` names the evidence span rather than a second local verb roster.
- Law: generated `SealedStep` and `BrepMeasure` return unchanged; B-rep measures reach the span as facts and never masquerade as a mesh-quality graduation.
- Output: `BrepResult.span_facts` projects the returned `BrepMeasure` — mass properties beside the topology census — so the weave stamps the kernel's own measures on the live span and geometry measures nothing twice.
- Boundary: `remote_fault` preserves the provider's typed `FaultDetail`, status, correlation, causal stamp, tenant, and recovery whole at the client edge `mesh/cad#BRIDGE` owns; this page never re-spells that capture. No native package or handle crosses Python 3.15.

```python
from collections.abc import Mapping
from functools import partial

from expression import Option
from msgspec import Struct
# Contracts are retired from this logic.

from rasm.geometry.graduation import EvidenceScope, GeometrySubject, evidence_run
from rasm.geometry.mesh.cad import EXECUTE, CadClient
from rasm.runtime.faults import RuntimeResult
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

# --- [MODELS] ---------------------------------------------------------------------------


class BrepResult(Struct, frozen=True, gc=False):
    reply: ExecuteResponse

    @property
    def span_facts(self) -> Mapping[str, object]:
        held = self.reply.measure
        return {
            "subject": GeometrySubject.MESH_ALGEBRA.value,
            "volume_m3": held.volume_m3,
            "area_m2": held.area_m2,
            "vertices": held.topology.vertices,
            "edges": held.topology.edges,
            "faces": held.topology.faces,
            "solids": held.topology.solids,
        }



# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(
    request: ExecuteRequest,
    client: CadClient,
    *,
    budget: Option[float],
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeResult[BrepResult]:
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

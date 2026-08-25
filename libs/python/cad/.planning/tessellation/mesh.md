# [PY_CAD_MESH]

`tessellate` is the provider's one discrete-result spine: it takes an admitted transfer, drives `BRepMesh_IncrementalMesh` under the caller's `TessellationPolicy`, admits the kernel verdict, sums the located triangulation against the budget before any byte is written, then hands the document to the writer and the emitted file to the census. Meshing mutates the transferred shape's stored triangulation in place, so the mesher runs exactly once over one root and every later reader sees the same triangulation.

Policy arrives whole on the wire as `deflection_m`, `angle_tolerance_rad`, and `triangle_budget`, already bounded by protovalidate, so no arm re-checks a knob the request proved. `exchange/assembly#ROOTS` owns the reader, its mode set, the XCAF document, and the free-shape root; `tessellation/emission#EMISSION` owns the writer and byte admission; `metrology/census#CENSUS` owns the emitted-file counts and `metrology/properties#MEASURE` the kernel measure. This owner sequences those four and adds the mesher, the preflight, and the budget verdict.

## [01]-[INDEX]

- [02]-[MESHER]: `Custody` over the mesher's boolean parallel flag, the policy lowering, and the `IsDone`/`GetStatusFlags` admission.
- [03]-[PREFLIGHT]: Located triangulation read-back over every face, its traversal fold, and the pre-emission budget refusal.
- [04]-[MESH]: `tessellate` sequencing transfer, mesh, preflight, emission, and census into one `TessellationEvidence`.

## [02]-[MESHER]

- Owner: `_meshed` — one mesher call per admitted root; `BRepMesh_IncrementalMesh` mutates the shape's stored triangulation in place, so a second call over one root re-triangulates what every later reader already holds.
- Cases: `Custody` carries `SERIAL` and `WHOLE_LANE`; the value, never a bare argument, states which the caller proved.
- Law: `isInParallel` is a BOOLEAN, never a thread count, so enabling it lets OCCT saturate every core the process reaches.
- Law: whole-lane custody is therefore a claim, not a preference — `service/lane#LANE` holds the one-slot lane and mints the value that admits it.
- Law: a caller unable to prove sole occupancy passes `SERIAL`; a bare `parallel: bool` argument at the call site is the deleted form.
- Law: `IsDone()` false and a non-zero `GetStatusFlags()` are independent verdicts, and both refuse on `MESH_KERNEL` carrying the flag word.
- Law: `deflection_m`, `angle_tolerance_rad`, and `triangle_budget` arrive protovalidate-bounded, so this owner lowers them and re-checks none.
- Law: `isRelative` stays false — a relative deflection rescales tolerance per sub-shape and makes one emitted budget unreproducible across sources.
- Growth: a new mesher knob is one `TessellationPolicy` field lowered here; a new custody regime is one `Custody` value minted at the lane.
- Boundary: this owner meshes. Building the document and its root belongs to `exchange/assembly#ROOTS`, and no reader is reachable here.

```python
from typing import Final

from OCP.BRepMesh import BRepMesh_IncrementalMesh
from OCP.TopoDS import TopoDS_Shape
from expression import Error, Ok
from msgspec import Struct
from rasm.contracts.rasm.contracts.geometry.tessellation_pb import TessellationPolicy

from rasm.cad.faults import MESH_KERNEL, CadRail

# --- [MODELS] ---------------------------------------------------------------------------


class Custody(Struct, frozen=True, gc=False):
    parallel: bool


SERIAL: Final[Custody] = Custody(parallel=False)
WHOLE_LANE: Final[Custody] = Custody(parallel=True)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _meshed(root: TopoDS_Shape, policy: TessellationPolicy, custody: Custody, /) -> CadRail[TopoDS_Shape]:
    mesher = BRepMesh_IncrementalMesh(root, policy.deflection_m, False, policy.angle_tolerance_rad, custody.parallel)
    done, flags = mesher.IsDone(), mesher.GetStatusFlags()
    return Ok(root) if done and flags == 0 else Error(MESH_KERNEL.at(f"BRepMesh_IncrementalMesh:done={done};flags={flags}"))
```

## [03]-[PREFLIGHT]

- Owner: `_preflight` — one traversal of the meshed root proving every face carries a triangulation and the total fits the budget.
- Law: preflight runs after the mesher and before the writer, so an over-budget emission never reaches the filesystem.
- Law: a face whose `Triangulation_s` returns absent refuses on `MESH_KERNEL`; the mesher reported done, so an absent face is a kernel defect.
- Law: `traverse` short-circuits on the first absent face, replacing a `while` cursor whose two mid-walk raises made the refusal order implicit.
- Law: the budget verdict reads one summed total, so its coordinate names the whole shape rather than whichever face first crossed the line.
- Law: `TopLoc_Location()` is the out-parameter the read-back fills, never an input placement, so the returned triangulation stays in its own frame.
- Law: preflight counts native triangles; the emitted-file census stays the measurement authority and gates the budget a second time after emission.
- Growth: a new pre-emission proof is one arrow inside the traversal step; the traversal itself takes no new argument.
- Boundary: this owner reads `NbTriangles()` alone. `Poly_Triangulation.Node`/`Triangle` and `Poly_Triangle.Value` are verified present and unread, so the vertex and face buffers a mesh consumer wants have no owner; the gap is the carrier, never the spelling.

```python
from collections.abc import Iterator

from OCP.BRep import BRep_Tool
from OCP.Poly import Poly_Triangulation
from OCP.TopAbs import TopAbs_FACE
from OCP.TopExp import TopExp_Explorer
from OCP.TopLoc import TopLoc_Location
from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape
from expression import Error, Ok
from expression.collections import Block
from expression.extra.result import traverse
from rasm.contracts.rasm.contracts.geometry.tessellation_pb import TessellationPolicy

from rasm.cad.faults import MESH_BUDGET, MESH_KERNEL, CadRail

# --- [OPERATIONS] -----------------------------------------------------------------------


def _faces(shape: TopoDS_Shape, /) -> Iterator[TopoDS_Face]:
    explorer = TopExp_Explorer(shape, TopAbs_FACE)
    while explorer.More():
        yield TopoDS.Face_s(explorer.Current())
        explorer.Next()


def _triangles(face: TopoDS_Face, /) -> CadRail[int]:
    triangulation: Poly_Triangulation | None = BRep_Tool.Triangulation_s(face, TopLoc_Location())
    return Ok(triangulation.NbTriangles()) if triangulation is not None else Error(MESH_KERNEL.at("mesh.face-triangulation"))


def _preflight(root: TopoDS_Shape, policy: TessellationPolicy, /) -> CadRail[int]:
    def gated(counts: Block[int], /) -> CadRail[int]:
        total = sum(counts)
        return Ok(total) if total <= policy.triangle_budget else Error(MESH_BUDGET.at(f"preflight:{total}>{policy.triangle_budget}"))

    return traverse(_triangles, Block.of_seq(_faces(root))).bind(gated)
```

## [04]-[MESH]

- Owner: `tessellate` — the one entry the native lane calls; it sequences transfer, mesh, preflight, emission, and census, and returns one `TessellationEvidence` value.
- Entry: do-notation, never `pipeline` — the root feeds mesher, preflight, and measure while the document feeds the writer, so two intermediates outlive their successor step and a kleisli chain cannot name them.
- Law: the emitted census gates the budget a second time, because welding and primitive splitting move the emitted total away from the native sum.
- Law: `MESH_BUDGET` carries both gates under distinct coordinates, so the refusal names whether the native fold or the emitted file crossed the line.
- Law: `GlbCensus.closure` elects the `Closure` arm `measured` consumes, so watertight and delta are never decided twice.
- Output: `TessellationEvidence` crosses the `to_process` seam as a value; the emitted bytes never do, and the parent owns the path they sit on.
- Output: `artifact_bytes` rides the evidence as emission proof `service/spool#SPOOL` confirms its publish against; the wire reserves the field.
- Growth: a new spine stage is one `yield from` arrow; a new source kind lands at `exchange/assembly#ROOTS` and this spine stands unchanged.
- Boundary: `TessellateResponse` is assembled at `service/provider#PROVIDER`, which owns the `ArtifactRef` this worker never sees.

```python
from pathlib import Path

from expression import Error, Ok, effect
from msgspec import Struct
from rasm.contracts.rasm.contracts.cad.service_pb import TessellateRequest
from rasm.contracts.rasm.contracts.cad.types_pb import BrepMeasure

from rasm.cad.exchange.assembly import transferred
from rasm.cad.faults import MESH_BUDGET, CadFault, CadRail
from rasm.cad.metrology.census import GlbCensus
from rasm.cad.metrology.properties import measured
from rasm.cad.tessellation.emission import emitted

# --- [MODELS] ---------------------------------------------------------------------------


class TessellationEvidence(Struct, frozen=True, kw_only=True):
    element_count: int
    triangle_count: int
    artifact_bytes: int
    measure: BrepMeasure


# --- [OPERATIONS] -----------------------------------------------------------------------


def _budgeted(census: GlbCensus, budget: int, /) -> CadRail[GlbCensus]:
    return Ok(census) if census.triangles <= budget else Error(MESH_BUDGET.at(f"emitted:{census.triangles}>{budget}"))


@effect.result[TessellationEvidence, CadFault]()
def tessellate(request: TessellateRequest, source_path: Path, glb_path: Path, ceiling: int, custody: Custody, /):
    document, root = yield from transferred(request, source_path)
    meshed = yield from _meshed(root, request.policy, custody)
    yield from _preflight(meshed, request.policy)
    artifact_bytes = yield from emitted(document, glb_path, ceiling)
    census = yield from GlbCensus.of(glb_path).bind(lambda read: _budgeted(read, request.policy.triangle_budget))
    measure = yield from measured(meshed, census.closure)
    return TessellationEvidence(
        element_count=census.instances, triangle_count=census.triangles, artifact_bytes=artifact_bytes, measure=measure
    )
```

## [05]-[RESEARCH]

(none)

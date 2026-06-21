# [PY_GEOMETRY_MESH_REPAIR]

Robust mesh algebra — the shared downstream primitive the tessellation, scan-reconstruction, and step hops compose. `MeshOp` is one tagged union discriminating by operation kind: watertight detection plus hole-fill plus winding/normal repair over `trimesh.repair`, and exact union/difference/intersection boolean over the `trimesh.boolean` `manifold3d` backend. Every arm returns a `MeshResult` pairing the in-memory `trimesh.Trimesh` triangulation with a `MeshReceipt` carrying the watertight verdict, the volume, and the vertex/face counts; the boolean arm requires watertight input the repair arm guarantees. Reconstructed and CSG meshes graduate via the compute `HandoffAxis` geometry `reconstructed-mesh`/`mesh-algebra` subject. This owner is the pure kernel: it conditions and combines triangulations and hands the result across the wire as in-memory geometry — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), which already binds the three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview; the geometry shed never opens or writes a mesh file.

## [01]-[INDEX]

- [01]-[MESH]: the repair and boolean operations under one tagged union over the `trimesh`/`manifold3d` surfaces, returning in-memory triangulation.

## [02]-[MESH]

- Owner: `MeshOp` — the tagged union discriminating by operation; `BooleanOp` the closed `StrEnum` selecting the CSG verb so the boolean arm is one row rather than three; `MeshResult` the carrier pairing the conditioned `trimesh.Trimesh` with `MeshReceipt`; `MeshReceipt` the typed receipt carrying the watertight verdict, volume, area, and vertex/face counts read off the result `trimesh.Trimesh`.
- Cases: `MeshOp` cases `Repair(mesh, weld)` (the `trimesh.repair` winding/normal/hole-fill conditioning pass plus the `merge_vertices` weld) and `Boolean(meshes, op)` (the n-ary `trimesh.boolean` union/difference/intersection over the robust `manifold3d` engine) — matched by `match`/`assert_never`, each binding the package that owns the operation. Inputs and outputs are in-memory `trimesh.Trimesh`; no mesh-file format axis and no `Codec` arm live here — decode/encode is the data `MeshPayload` owner across the `mesh ← data/spatial` seam.
- Entry: `apply` dispatches the case and returns a `RuntimeRail[MeshResult]`; the `Repair` arm runs `repair.fix_winding`/`repair.fix_normals`/`repair.fill_holes` in place over the supplied in-memory `Trimesh`, optionally welds, and reads `is_watertight`/`volume`/`area`; the `Boolean` arm requires every input watertight and folds the mesh sequence through `boolean.union`/`difference`/`intersection`. The conditioned `Trimesh` rides back in `MeshResult.mesh` across the wire; any persisted GLB/PLY/STL/3dm/VTK encode is a downstream `MeshPayload` call in `rasm.data.spatial.mesh`, never an arm here.
- Auto: `trimesh.repair.fix_winding`/`fix_normals` mutate the mesh in place toward consistent outward orientation and `repair.fill_holes` closes boundary loops, so a non-watertight reconstruction becomes a valid solid before the boolean arm consumes it; `trimesh.boolean.union`/`difference`/`intersection` route to the `manifold3d` `Manifold` kernel (`OpType.Add`/`Subtract`/`Intersect`), the robust default the legacy mesh-boolean path is the deleted alternative to; `Trimesh.is_watertight`/`is_winding_consistent`/`volume`/`area` are lazily cached properties read once after the op.
- Receipt: each op contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the op tag, the watertight verdict, the volume/area, and the vertex/face counts; a reconstructed solid carries the geometry `GraduationReceipt` subject `reconstructed-mesh`, an n-ary CSG result the `mesh-algebra` subject, both the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`) so the cleaned mesh graduates through the one geometry rail.
- Packages: `trimesh` (`Trimesh`/`Trimesh.is_watertight`/`Trimesh.is_winding_consistent`/`Trimesh.volume`/`Trimesh.area`/`Trimesh.merge_vertices`/`repair.fix_winding`/`repair.fix_normals`/`repair.fill_holes`/`boolean.union`/`boolean.difference`/`boolean.intersection`), `manifold3d` (the `trimesh.boolean` backend engine — `Manifold`/`OpType`, never called directly where `trimesh.boolean` dispatches), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new repair conditioning step is one `trimesh.repair` call inside the `Repair` arm; a new boolean verb is one `BooleanOp` row; clash-volume computation for `ifc-analysis` composes the `Boolean(meshes, INTERSECTION)` arm and reads `MeshReceipt.volume` rather than re-implementing overlap volume; the `manifold3d` boolean kernel (the CPU-bound CSG over the `<'3.15'` companion band, `manifold3d` verified manifest-present) hands across the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — the lane never imports the kernel; zero new surface, no parallel per-operation class family.
- Boundary: no point-cloud registration (that is `scan-registration`); no IFC tessellation (that is `tessellation`); the `manifold3d` kernel is reached only through the `trimesh.boolean` backend, never a second direct CSG owner; mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` extension router, the named-array `pyarrow` egress, the FE time-series rail, and the GLB `preview` export, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file; the legacy non-`manifold3d` mesh-boolean path, a hand-rolled watertight-repair or hole-fill kernel, a `union`/`difference`/`intersection` method family over the `BooleanOp` row, ANY `MeshFormat`/`Codec`/`load`/`export` codec arm re-deriving the `MeshPayload` seam, and a co-equal compas-datastructure fold are the deleted forms — robust mesh conditioning and CSG are this owner, the compas half-edge datastructure algebra is the `computational-geometry` sibling, the codec is the data sibling.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import trimesh
from msgspec import Struct
from expression import case, tag, tagged_union

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class BooleanOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"


class MeshReceipt(Struct, frozen=True):
    op: str
    watertight: bool
    volume: float
    area: float
    vertex_count: int
    face_count: int
    subject: GeometrySubject


@tagged_union(frozen=True)
class MeshOp:
    tag: Literal["repair", "boolean"] = tag()
    repair: tuple[trimesh.Trimesh, bool] = case()
    boolean: tuple[tuple[trimesh.Trimesh, ...], BooleanOp] = case()

    @staticmethod
    def Repair(mesh: trimesh.Trimesh, weld: bool = True) -> "MeshOp":
        return MeshOp(repair=(mesh, weld))

    @staticmethod
    def Boolean(meshes: tuple[trimesh.Trimesh, ...], op: BooleanOp) -> "MeshOp":
        return MeshOp(boolean=(meshes, op))


class MeshResult(Struct, frozen=True):
    mesh: trimesh.Trimesh
    receipt: MeshReceipt


def apply(op: MeshOp) -> "RuntimeRail[MeshResult]":
    return boundary(f"mesh.{op.tag}", lambda: _dispatch(op))


def _result(op: str, mesh: "trimesh.Trimesh", subject: GeometrySubject) -> MeshResult:
    receipt = MeshReceipt(
        op,
        bool(mesh.is_watertight),
        float(mesh.volume),
        float(mesh.area),
        len(mesh.vertices),
        len(mesh.faces),
        subject,
    )
    return MeshResult(mesh, receipt)


def _dispatch(op: MeshOp) -> MeshResult:
    match op:
        case MeshOp(tag="repair", repair=(mesh, weld)):
            trimesh.repair.fix_winding(mesh)
            trimesh.repair.fix_normals(mesh)
            trimesh.repair.fill_holes(mesh)
            if weld:
                mesh.merge_vertices()
            return _result("repair", mesh, "reconstructed-mesh")
        case MeshOp(tag="boolean", boolean=(meshes, kind)):
            match kind:
                case BooleanOp.UNION:
                    result = trimesh.boolean.union(meshes)
                case BooleanOp.DIFFERENCE:
                    result = trimesh.boolean.difference(meshes)
                case BooleanOp.INTERSECTION:
                    result = trimesh.boolean.intersection(meshes)
                case unreachable:
                    assert_never(unreachable)
            return _result(f"boolean.{kind}", result, "mesh-algebra")
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

- [TRIMESH_CONDITIONING]: the in-place conditioning verbs `repair.fix_winding`/`fix_normals`/`fill_holes`, the n-ary `boolean.union`/`difference`/`intersection`, and the `is_watertight`/`is_winding_consistent`/`volume`/`area` lazy properties confirm against the branch `trimesh` catalogue on the cp312 companion; the `Trimesh.merge_vertices` weld verb is the unconfirmed member. The repair shed takes an in-memory `Trimesh` in and hands one back — byte-buffer intake (`wrap_as_stream`/`load`) belongs to the data `MeshPayload` codec, not this owner.
- [CODEC_SEAM]: cross-format mesh decode/encode — the `.3dm` leg over `rhino3dm.File3dm`, the FEM leg over `meshio.read`/`write`, the trimesh-native `glb`/`ply`/`stl`/`obj` rows, and the GLB `preview` — is NOT this owner: the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` extension router, and the named-array `pyarrow` egress. The geometry repair shed receives an in-memory `trimesh.Trimesh` and returns one in-memory `MeshResult.mesh` across the `mesh ← data/spatial` seam; the consumer reaches `MeshPayload` for any file IO. No codec member resolves against the `rhino3dm`/`meshio` catalogues here because no codec arm exists here.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `manifold3d` ships cp310-cp314 wheels and rides the `python_version<'3.15'` companion band the branch manifest gates as the robust `trimesh.boolean` backend; `trimesh`/`rhino3dm`/`meshio` are pure-Python or prebuilt-wheel admitted, but the boolean arm's `manifold3d` backend resolves only on the companion the daemon hosts — the Forge `python312` lane (`forge-companion-env`), the cp312 floor inside the `<'3.15'` band. The cp315 project venv carries no `manifold3d` wheel, so the boolean arm runs companion-band only — repair and codec arms run on the cp315 core.

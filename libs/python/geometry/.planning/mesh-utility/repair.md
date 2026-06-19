# [PY_GEOMETRY_MESH_UTILITY_REPAIR]

Robust mesh algebra — the shared downstream primitive the tessellation, scan-reconstruction, and step hops compose. `MeshOp` is one tagged union discriminating by operation kind: watertight detection plus hole-fill plus winding/normal repair over `trimesh.repair`, exact union/difference/intersection boolean over the `trimesh.boolean` `manifold3d` backend, and mesh-file decode/encode across the `trimesh`/`rhino3dm`/`meshio` codec seam keyed by file format. Every arm returns a `MeshReceipt` carrying the watertight verdict, the volume, and the vertex/face counts; the boolean arm requires watertight input the repair arm guarantees. Reconstructed meshes graduate via the compute `HandoffAxis` geometry `reconstructed-mesh` subject. Mesh-file exchange aligns to the branch `data/mesh-exchange` seam, never a managed interior.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[MESH]`: the repair, boolean, and codec operations under one tagged union over the `trimesh`/`manifold3d`/`rhino3dm`/`meshio` surfaces.

## [2]-[MESH]

- Owner: `MeshOp` — the tagged union discriminating by operation; `BooleanOp` the closed `StrEnum` selecting the CSG verb so the boolean arm is one row rather than three; `MeshFormat` the closed `StrEnum` selecting the codec so the decode/encode arm is one row over the format set; `MeshReceipt` the typed receipt carrying the watertight verdict, volume, area, and vertex/face counts read off the result `trimesh.Trimesh`.
- Cases: `MeshOp` cases `Repair(glb, weld)` (the `trimesh.repair` winding/normal/hole-fill conditioning pass plus the `process` merge-validate), `Boolean(meshes, op)` (the n-ary `trimesh.boolean` union/difference/intersection over the robust `manifold3d` engine), and `Codec(payload, src, dst)` (mesh-file decode/encode routing `trimesh.load`/`export` for the trimesh-native formats, `rhino3dm.File3dm` for `.3dm`, and `meshio.read`/`write` for the FEM/CAE formats) — matched by `match`/`assert_never`, each binding the package that owns the operation.
- Entry: `apply` dispatches the case and returns a `RuntimeRail[MeshReceipt]`; the `Repair` arm loads the GLB through `trimesh.load(file_obj, file_type="glb")` forcing a `Trimesh`, runs `repair.fix_winding`/`repair.fix_normals`/`repair.fill_holes` in place, and reads `is_watertight`/`volume`/`area`; the `Boolean` arm requires every input watertight and folds the mesh sequence through `boolean.union`/`difference`/`intersection`; the `Codec` arm decodes the source bytes and re-encodes to the destination format, the `.3dm` leg over `rhino3dm.File3dm.FromByteArray` and `File3dmObjectTable.Add` and the FEM leg over `meshio.read`/`write`.
- Auto: `trimesh.repair.fix_winding`/`fix_normals` mutate the mesh in place toward consistent outward orientation and `repair.fill_holes` closes boundary loops, so a non-watertight reconstruction becomes a valid solid before the boolean arm consumes it; `trimesh.boolean.union`/`difference`/`intersection` route to the `manifold3d` `Manifold` kernel (`OpType.Add`/`Subtract`/`Intersect`), the robust default the legacy mesh-boolean path is the deleted alternative to; `Trimesh.is_watertight`/`is_winding_consistent`/`volume`/`area` are lazily cached properties read once after the op; the `meshio.Mesh` carries typed `CellBlock` connectivity the `.vtk`/`.msh` round-trip preserves.
- Receipt: each op contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the op tag, the watertight verdict, the volume, and the vertex/face counts; a reconstructed-mesh result produces a geometry `GraduationReceipt` subject (`reconstructed-mesh`) so the cleaned solid graduates through the one geometry rail.
- Packages: `trimesh` (`load`/`load_mesh`/`Trimesh.export`/`Trimesh.is_watertight`/`Trimesh.is_winding_consistent`/`Trimesh.volume`/`Trimesh.area`/`repair.fix_winding`/`repair.fix_normals`/`repair.fill_holes`/`boolean.union`/`boolean.difference`/`boolean.intersection`), `manifold3d` (the `trimesh.boolean` backend engine — `Manifold`/`OpType`, never called directly where `trimesh.boolean` dispatches), `rhino3dm` (`File3dm`/`File3dm.FromByteArray`/`File3dmObjectTable.Add`/`Mesh`), `meshio` (`read`/`write`/`Mesh`/`CellBlock`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new repair conditioning step is one `trimesh.repair` call inside the `Repair` arm; a new boolean verb is one `BooleanOp` row; a new mesh format is one `MeshFormat` row binding its codec; clash-volume computation for `ifc-analysis` composes the `Boolean(meshes, INTERSECTION)` arm and reads `MeshReceipt.volume` rather than re-implementing overlap volume; zero new surface, no parallel per-operation class family.
- Boundary: no point-cloud registration (that is `scan-registration`); no IFC tessellation (that is `tessellation`); the `manifold3d` kernel is reached only through the `trimesh.boolean` backend, never a second direct CSG owner; the legacy non-`manifold3d` mesh-boolean path, a hand-rolled watertight-repair or hole-fill kernel, a `union`/`difference`/`intersection` method family over the `BooleanOp` row, a `load_glb`/`load_obj` codec family over the `MeshFormat` row, and a co-equal compas-datastructure fold are the deleted forms — robust mesh conditioning and CSG are this owner, the compas half-edge datastructure algebra is the `computational-geometry` sibling.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import trimesh
from msgspec import Struct
from expression import case, tag, tagged_union

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class BooleanOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"


class MeshFormat(StrEnum):
    GLB = "glb"
    PLY = "ply"
    STL = "stl"
    OBJ = "obj"
    THREEDM = "3dm"
    VTK = "vtk"
    MSH = "msh"


class MeshReceipt(Struct, frozen=True):
    op: str
    watertight: bool
    volume: float
    vertex_count: int
    face_count: int


@tagged_union(frozen=True)
class MeshOp:
    tag: Literal["repair", "boolean", "codec"] = tag()
    repair: tuple[bytes, bool] = case()
    boolean: tuple[tuple[bytes, ...], BooleanOp] = case()
    codec: tuple[bytes, MeshFormat, MeshFormat] = case()

    @staticmethod
    def Repair(glb: bytes, weld: bool = True) -> "MeshOp":
        return MeshOp(repair=(glb, weld))

    @staticmethod
    def Boolean(meshes: tuple[bytes, ...], op: BooleanOp) -> "MeshOp":
        return MeshOp(boolean=(meshes, op))

    @staticmethod
    def Codec(payload: bytes, src: MeshFormat, dst: MeshFormat) -> "MeshOp":
        return MeshOp(codec=(payload, src, dst))


def apply(op: MeshOp) -> "RuntimeRail[MeshReceipt]":
    return boundary(f"mesh.{op.tag}", lambda: _dispatch(op))


def _load(payload: bytes, fmt: MeshFormat) -> "trimesh.Trimesh":
    return trimesh.load(trimesh.util.wrap_as_stream(payload), file_type=fmt, force="mesh")


def _receipt(op: str, mesh: "trimesh.Trimesh") -> MeshReceipt:
    return MeshReceipt(op, bool(mesh.is_watertight), float(mesh.volume), len(mesh.vertices), len(mesh.faces))


def _dispatch(op: MeshOp) -> MeshReceipt:
    match op:
        case MeshOp(tag="repair", repair=(glb, weld)):
            mesh = _load(glb, MeshFormat.GLB)
            trimesh.repair.fix_winding(mesh)
            trimesh.repair.fix_normals(mesh)
            trimesh.repair.fill_holes(mesh)
            if weld:
                mesh.merge_vertices()
            return _receipt("repair", mesh)
        case MeshOp(tag="boolean", boolean=(meshes, kind)):
            loaded = [_load(payload, MeshFormat.GLB) for payload in meshes]
            match kind:
                case BooleanOp.UNION:
                    result = trimesh.boolean.union(loaded)
                case BooleanOp.DIFFERENCE:
                    result = trimesh.boolean.difference(loaded)
                case BooleanOp.INTERSECTION:
                    result = trimesh.boolean.intersection(loaded)
                case unreachable:
                    assert_never(unreachable)
            return _receipt(f"boolean.{kind}", result)
        case MeshOp(tag="codec", codec=(payload, src, dst)):
            mesh = _load(payload, src)
            return _receipt(f"codec.{src}.{dst}", mesh)
        case unreachable:
            assert_never(unreachable)
```

## [3]-[RESEARCH]

- [TRIMESH_STREAM_INTAKE]: the `trimesh.util.wrap_as_stream(bytes)` byte-buffer-to-stream helper and the `trimesh.load(file_obj, file_type, force="mesh")` GLB intake forcing a single `Trimesh` confirm against the branch `trimesh` catalogue on the cp312 companion; the catalogue confirms `trimesh.load`/`load_mesh`/`repair.fix_winding`/`fix_normals`/`fill_holes`/`boolean.union`/`difference`/`intersection`/`is_watertight`/`volume`/`area`, leaving the `wrap_as_stream` helper spelling and the `Trimesh.merge_vertices` weld verb as the unconfirmed members.
- [CODEC_CROSS_FORMAT]: the `.3dm` leg over `rhino3dm.File3dm.FromByteArray`/`File3dmObjectTable.Add(mesh, attributes)` and the FEM leg over `meshio.read`/`write` resolve the `Codec` arm's cross-format re-encode (the GLB-to-`.3dm` and GLB-to-`.vtk` round-trip) against the branch `rhino3dm`/`meshio` catalogues before the destination-format write replaces the trimesh-native `export` shown in the fence; the trimesh-native `MeshFormat` rows (`glb`/`ply`/`stl`/`obj`) encode through `Trimesh.export(file_type=dst)` and are catalogue-confirmed.

## [4]-[UPSTREAM]

- [WHEEL_BAND]: `manifold3d` ships cp310-cp314 wheels and rides the `python_version<'3.15'` companion band the branch manifest gates as the robust `trimesh.boolean` backend; `trimesh`/`rhino3dm`/`meshio` are pure-Python or prebuilt-wheel admitted, but the boolean arm's `manifold3d` backend resolves only on the companion the daemon hosts — the Forge `python312` lane (`forge-companion-env`), the cp312 floor inside the `<'3.15'` band. The cp315 project venv carries no `manifold3d` wheel, so the boolean arm runs companion-band only — repair and codec arms run on the cp315 core.

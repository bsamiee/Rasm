# [PY_DATA_EXCHANGE]

The mesh-file exchange owner over a `MeshBackend` axis, plus the point-cloud interchange row. `MeshPayload` carries mesh-file identity/cell-block topology/units/metadata/preview-export over `meshio.read`/`write` for FE volume/cell-block meshes and `trimesh.load`/`Trimesh.export` for surface meshes; the backend is recovered from the source extension, never a knob, and the cell-block topology folds both engines onto one `cell_blocks` field. The LAS/LAZ/COPC point-cloud interchange row over laspy/pdal feeds the geometry scan-processing/registration companion at the mesh seam. This is file exchange and identity — the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here. Every payload keys by runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[MESH]`: mesh-file identity, cell-block topology, units, GLB preview export, the point-cloud interchange row.

## [2]-[MESH]

- Owner: `MeshPayload` — mesh-file identity/cell-block topology/units/metadata/preview-export over a `MeshBackend` axis: `meshio.read`/`write` for FE volume/cell-block meshes, `trimesh.load`/`Trimesh.export` for surface meshes. `MeshBackend` is recovered from the source extension; the cell-block topology folds both engines onto one `cell_blocks` field (meshio `CellBlock.type`, trimesh's single `triangle` block).
- Entry: `MeshPayload.read` admits a mesh file and returns the frozen owner keyed by `ContentIdentity` with the backend recovered from the source shape; `MeshPayload.preview` emits a `glb` preview render through whichever engine owns the loaded mesh; `MeshPayload.write` round-trips through the format the requested extension selects. Read/preview/write all return a `RuntimeRail`, never raising in the boundary.
- Auto: the backend resolves off the source extension through `_backend_of` — `trimesh` owns the surface (display/exchange) extensions, `meshio` owns the FE volume/cell-block formats; a new surface format is one `trimesh` extension string and a new FE format is one `meshio` format string.
- Packages: `meshio` (`read`/`write`/`Mesh`/`CellBlock.type`), `trimesh` (`load`/`Trimesh`/`Trimesh.export`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`), `laspy` (LAS/LAZ point-record decode/encode), `pdal` (COPC octree-chunked spatial-subset pipeline), runtime (`ContentIdentity`/`ContentKey`/`ResourceRef`/`RuntimeRail`/`boundary`/`Receipt`).
- Growth: a new surface format is one `trimesh` extension string; a new FE format is one `meshio` format string; the LAS/LAZ/COPC point-cloud interchange is one `MeshBackend` row plus a numpy/Arrow columnar point-record bridge feeding the geometry scan companion; a new engine is one `MeshBackend` tag plus one `_read`/`_export` dispatch arm; zero new surface and never a per-format `read_*` family.
- Boundary: no geometry kernel (that is the geometry package), no bridge lifecycle; the IFC-to-GLB tessellation rail is geometry-owned, never re-derived here; the point-cloud row is host-free file exchange feeding the geometry scan companion at the wire, never a managed-interior coupling; a hand-rolled mesh parser and a parallel `MeshioPayload`/`TrimeshPayload` pair are the deleted forms.

```python signature
from typing import Literal, assert_never

import meshio
import trimesh
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef


type MeshBackend = Literal["meshio", "trimesh"]

_TRIMESH_EXTS: frozenset[str] = frozenset({".stl", ".obj", ".ply", ".glb", ".gltf", ".off", ".3mf"})


class MeshPayload(Struct, frozen=True):
    backend: MeshBackend
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    units: str

    @classmethod
    def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        return boundary("mesh.read", lambda: _read(ref))

    def preview(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return boundary("mesh.preview", lambda: _export(ref, out, "glb"))

    def write(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return boundary("mesh.write", lambda: _export(ref, out, out.path.suffix.lstrip(".")))

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", "mesh", self.backend,
            {"points": str(self.point_count), "blocks": ",".join(self.cell_blocks), "units": self.units},
        )


def _backend_of(ref: ResourceRef) -> MeshBackend:
    return "trimesh" if ref.path.suffix.lower() in _TRIMESH_EXTS else "meshio"


def _read(ref: ResourceRef) -> MeshPayload:
    match _backend_of(ref):
        case "meshio":
            mesh = meshio.read(str(ref.path))
            return MeshPayload(
                backend="meshio",
                content_key=ContentIdentity.of("mesh", mesh.points.tobytes()),
                point_count=len(mesh.points),
                cell_blocks=tuple(block.type for block in mesh.cells),
                units="m",
            )
        case "trimesh":
            surface = trimesh.load(str(ref.path), force="mesh")
            return MeshPayload(
                backend="trimesh",
                content_key=ContentIdentity.of("mesh", surface.vertices.tobytes()),
                point_count=len(surface.vertices),
                cell_blocks=("triangle",) if len(surface.faces) else (),
                units=surface.units or "m",
            )
        case unreachable:
            assert_never(unreachable)


def _export(ref: ResourceRef, out: ResourceRef, fmt: str) -> ContentKey:
    match _backend_of(ref):
        case "meshio":
            meshio.write(str(out.path), meshio.read(str(ref.path)), file_format=fmt)
        case "trimesh":
            trimesh.load(str(ref.path), force="mesh").export(str(out.path), file_type=fmt)
        case unreachable:
            assert_never(unreachable)
    return ContentIdentity.of("mesh.export", out.path.read_bytes())
```

# [PY_DATA_MESH]

The mesh-file exchange owner over a `MeshBackend` axis, plus the point-cloud interchange row. `MeshPayload` carries mesh-file identity/cell-block topology/units/metadata/preview-export over `meshio.read`/`write` for FE volume/cell-block meshes and `trimesh.load`/`Trimesh.export` for surface meshes; the backend is recovered from the source extension, never a knob, and the cell-block topology folds both engines onto one `cell_blocks` field. `PointCloud` is the LAS/LAZ/COPC interchange row over `laspy` (LAS/LAZ point-record decode/encode) and `pdal` (the COPC octree-chunked spatial-subset pipeline), bridging to a numpy/Arrow columnar point-record table that feeds the geometry scan-processing/registration companion at the mesh seam. This is file exchange and identity — the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here. Every payload keys by runtime `ContentIdentity`.

## [1]-[INDEX]

- [1]-[MESH]: mesh-file identity, cell-block topology, units, GLB preview export.
- [2]-[POINTCLOUD]: the LAS/LAZ/COPC point-cloud interchange row and the columnar point-record bridge.

## [2]-[MESH]

- Owner: `MeshPayload` — mesh-file identity/cell-block topology/units/metadata/preview-export over a `MeshBackend` axis: `meshio.read`/`write` for FE volume/cell-block meshes, `trimesh.load`/`Trimesh.export` for surface meshes. `MeshBackend` is recovered from the source extension; the cell-block topology folds both engines onto one `cell_blocks` field (meshio `CellBlock.type`, trimesh's single `triangle` block).
- Entry: `MeshPayload.read` admits a mesh file and returns the frozen owner keyed by `ContentIdentity` with the backend recovered from the source shape; `MeshPayload.preview` emits a `glb` preview render through whichever engine owns the loaded mesh; `MeshPayload.write` round-trips through the format the requested extension selects. Read/preview/write all return a `RuntimeRail`, never raising in the boundary.
- Auto: the backend resolves off the source extension through `_backend_of` — `trimesh` owns the surface (display/exchange) extensions, `meshio` owns the FE volume/cell-block formats; a new surface format is one `trimesh` extension string and a new FE format is one `meshio` format string.
- Packages: `meshio` (`read`/`write`/`Mesh`/`CellBlock.type`), `trimesh` (`load`/`Trimesh`/`Trimesh.export`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`), `laspy` (LAS/LAZ point-record decode/encode), `pdal` (COPC octree-chunked spatial-subset pipeline), runtime (`ContentIdentity`/`ContentKey`/`ResourceRef`/`RuntimeRail`/`boundary`/`Receipt`).
- Growth: a new surface format is one `trimesh` extension string; a new FE format is one `meshio` format string; the LAS/LAZ/COPC point-cloud interchange is the `[3]-[POINTCLOUD]` `PointCloud` row with its numpy/Arrow columnar point-record bridge feeding the geometry scan companion; a new engine is one `MeshBackend` tag plus one `_read`/`_export` dispatch arm; zero new surface and never a per-format `read_*` family.
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
            "emitted", "mesh", self.backend, {"points": str(self.point_count), "blocks": ",".join(self.cell_blocks), "units": self.units}
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

## [3]-[POINTCLOUD]

- Owner: `PointCloud` — the LAS/LAZ/COPC interchange row over `laspy` (LAS/LAZ point-record decode/encode) and `pdal` (the COPC octree-chunked spatial-subset pipeline); `PointBounds` the optional octree subset box; the point records cross to the geometry scan companion as a columnar `pyarrow.Table` (`x`/`y`/`z` plus the LAS dimension columns), never a laspy- or pdal-specific object. `PointFormat` carries the LAS point-format id and the CRS WKT recovered from the header.
- Entry: `PointCloud.read` admits a LAS/LAZ `ResourceRef` and returns the frozen owner keyed by `ContentIdentity` over the point coordinates, the backend the source extension selects (`.las`/`.laz` -> laspy, `.copc.laz` -> pdal octree); `PointCloud.subset` reads a COPC octree spatial subset over a `PointBounds` box through the `pdal` COPC reader rather than a full read; `PointCloud.to_arrow` folds the laspy `LasData` dimensions into one `pyarrow.Table` columnar bridge; `PointCloud.write` round-trips LAS/LAZ through `laspy`. All return a `RuntimeRail`.
- Auto: the backend resolves off the source extension — a `.copc.laz` request rides the COPC octree subset and a plain `.las`/`.laz` rides the laspy full decode; COPC is the cloud-native scan standard, so the subset request rides the COPC octree (`pdal` `readers.copc` with a `bounds` option) rather than a full read; the columnar bridge stays Arrow so the geometry companion consumes `pyarrow.Table`, not a pdal pipeline object. `laspy` is cp315-clean and imports module-top; `pdal` rides the `python_version<'3.15'` gated band (sdist-only, no cp315 wheel), so its COPC arm imports the dist function-local under `# noqa: PLC0415`, never a module-top import on this cp315-core page.
- Boundary: no geometry kernel registration, no scan-to-BIM compute (that is the geometry scan companion), no host coupling; the point-cloud row is host-free file exchange feeding the geometry companion at the wire (content-identity plus the columnar bridge), never a managed-interior coupling; a hand-rolled LAS parser and a pdal-specific object crossing the seam are the deleted forms.

```python signature
from typing import Literal

import laspy
import numpy as np
import pyarrow as pa
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.roots import ResourceRef

type PointBackend = Literal["laspy", "pdal"]

_COPC_SUFFIX: str = ".copc.laz"


class PointBounds(Struct, frozen=True):
    minx: float
    miny: float
    minz: float
    maxx: float
    maxy: float
    maxz: float

    def as_pdal(self) -> str:
        return f"([{self.minx},{self.maxx}],[{self.miny},{self.maxy}],[{self.minz},{self.maxz}])"


class PointCloud(Struct, frozen=True):
    backend: PointBackend
    content_key: ContentKey
    point_count: int
    point_format: int
    crs_wkt: str

    @classmethod
    def read(cls, ref: ResourceRef) -> "RuntimeRail[PointCloud]":
        return boundary("pointcloud.read", lambda: _read_las(ref))

    @staticmethod
    def subset(ref: ResourceRef, bounds: PointBounds) -> "RuntimeRail[pa.Table]":
        return boundary("pointcloud.subset", lambda: _copc_subset(ref, bounds))

    @staticmethod
    def to_arrow(ref: ResourceRef) -> "RuntimeRail[pa.Table]":
        return boundary("pointcloud.to_arrow", lambda: _las_to_arrow(laspy.read(str(ref.path))))

    def write(self, source: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return boundary("pointcloud.write", lambda: _write_las(source, out))


def _backend_of(ref: ResourceRef) -> PointBackend:
    return "pdal" if ref.path.name.lower().endswith(_COPC_SUFFIX) else "laspy"


def _read_las(ref: ResourceRef) -> PointCloud:
    data = laspy.read(str(ref.path))
    coords = np.column_stack((data.x, data.y, data.z)).astype("float64")
    return PointCloud(
        backend=_backend_of(ref),
        content_key=ContentIdentity.of("pointcloud", coords.tobytes()),
        point_count=int(data.header.point_count),
        point_format=int(data.header.point_format.id),
        crs_wkt=str(data.header.parse_crs() or ""),
    )


def _las_to_arrow(data: "laspy.LasData") -> pa.Table:
    columns = {name: np.asarray(data[name]) for name in data.point_format.dimension_names}
    return pa.table(columns)


def _write_las(source: ResourceRef, out: ResourceRef) -> ContentKey:
    laspy.read(str(source.path)).write(str(out.path))
    return ContentIdentity.of("pointcloud.write", out.path.read_bytes())


def _copc_subset(ref: ResourceRef, bounds: PointBounds) -> pa.Table:
    import pdal  # noqa: PLC0415

    pipeline = pdal.Reader.copc(str(ref.path), bounds=bounds.as_pdal()).pipeline()
    pipeline.execute()
    structured = pipeline.arrays[0]
    return pa.table({name: structured[name] for name in structured.dtype.names})
```

## [4]-[RESEARCH]

- [LASPY_SURFACE]: the `laspy` `read(path)`/`LasData.{x,y,z}`/`LasData.header.{point_count,point_format,parse_crs}`/`PointFormat.{id,dimension_names}`/`LasData.write(path)` surface the `PointCloud` LAS/LAZ arm transcribes confirms against a folder `laspy` `.api` catalogue authored on admission; `laspy` (2.7.0) is cp315-clean and lock-resolved but is uninstalled on the authoring host, so the LAS point-record member surface stays a catalogue-pending settled form until the catalogue lands by reflection against the installed distribution.
- [PDAL_COPC]: the `pdal` `Reader.copc(path, bounds=)`/`Pipeline.{execute,arrays}` COPC octree-subset surface the `_copc_subset` arm binds rides the `python_version<'3.15'` gated band (sdist-only, no cp315 wheel) and imports function-local; the catalogue is unauthorable on the cp315 core, so the COPC subset member surface stays a marked RESEARCH item — never settled fence code — until the `pdal` provenance installs on a `<3.15` companion host and the catalogue captures the `Reader.copc` bounds-option spelling and the `Pipeline.arrays` structured-array egress. A module-top `pdal` import on this cp315-core page is the floor-violating form.

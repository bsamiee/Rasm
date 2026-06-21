# [PY_DATA_MESH]

The mesh-file exchange owner over a `MeshBackend` axis, plus the point-cloud interchange row. `MeshPayload` carries mesh-file identity, cell-block topology, units, named point/cell/field-array references, the FE time-series rail, and preview export over one `_BACKEND` behavior table pairing each `MeshBackend` tag with its loader, frame projector, exporter, unit reader, fault root, and extension set — `meshio.read`/`write` for FE volume/cell-block meshes, `trimesh.load`/`Trimesh.export` for surface meshes, and `rhino3dm.File3dm.Read` for `.3dm` mesh exchange — so a new engine is one `MeshBackend` case plus one `_BACKEND` row, never a parallel `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder family and never a per-engine `match` arm in `frame`/`export`. The backend is one `MeshBackend` tagged-union case recovered from the source extension, never a knob; the cell-block topology folds `meshio.Mesh.cells_dict`, the single `trimesh` `triangle` block, and the `rhino3dm.Mesh` `triangle`/`quad` block onto one `cell_blocks` field; the source engine loads exactly once per operation and the loaded object threads through the row reader so `read`/`arrays`/`preview`/`write` never re-open the file. Large point/cell/field arrays cross by `ContentKey` reference through `tabular/egress#EGRESS`, never inlined in the frozen struct, and the named-array egress builds Arrow columns straight from the NumPy buffers through one `_column` fold — `pyarrow.array` for a 1D dimension, `pyarrow.FixedSizeListArray.from_arrays` over the flat buffer for a multi-dim stack — rather than a `.tolist()` row materialization. `PointCloud` is the LAS/LAZ/COPC interchange row over `laspy` alone — `laspy.read`/`LasData.write` for the full-file LAS/LAZ decode/encode and `laspy.copc.CopcReader` for the octree-chunked spatial subset — bridging to a content-keyed `pyarrow.Table` columnar point-record table through one polymorphic dimension fold that serves both the full `LasData` read and the `ScaleAwarePointRecord` subset, feeding the geometry scan-processing/registration companion at the mesh seam. cp315-core `laspy.copc` reads uncompressed COPC and resolves the `lazrs` COPC codec internally; the `LasData.write` LAZ transcode binds the `lazrs`/`laszip` codec backend through the verified `LazBackend.{Lazrs, LazrsParallel, Laszip}` selector function-local on the `python_version<'3.15'` companion band. Every boundary narrows `catch=` to the engine's own fault root so a non-engine exception escapes rather than masquerading as a mesh fault, and the raised fault converts exactly once through the runtime `boundary` owner into `BoundaryFault`. This is file exchange and identity — the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here, and the geometry `pdal` filter-graph (SMRF/PMF/outlier/decimation/range) stays geometry-owned. Every payload keys by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[MESH]: mesh-file identity, cell-block topology, named point/cell/field-array references, the FE time-series rail, units, and GLB preview export over one `MeshBackend` axis.
- [02]-[POINTCLOUD]: the LAS/LAZ/COPC point-cloud interchange row over `laspy`, the uncompressed/compressed COPC octree-subset split, and the columnar point-record bridge.

## [02]-[MESH]

- Owner: `MeshPayload` — mesh-file identity, cell-block topology, named point/cell/field-array references, units, the FE time-series rail, and preview export over the `MeshBackend` tagged-union axis routed through one `_BACKEND` behavior table: `meshio` for FE volume/cell-block meshes, `trimesh` for surface meshes, `rhino3dm` for `.3dm` mesh exchange. Each `_Backend` row pairs the loader, one engine-specific `extract` accessor pack projecting `(points, cell_blocks, point_data, cell_data, field_data)` into the engine-agnostic `_Extract`, the exporter, the unit reader, the raised-fault root the `boundary` narrows `catch=` to, and the extension set; the `_Backend.frame` method composes the one shared `_frame` fold over `extract`, so the `MeshFrame` assembly — point-bytes `ContentKey`, count, and the `ArrayRef` tuples — is written once and a new engine is one `MeshBackend` case plus one `_BACKEND` row carrying its `extract` arm, never a parallel `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder triple and never a per-engine arm in `frame`/`export`. `cell_blocks` folds `meshio.Mesh.cells_dict` keys, the single `trimesh` `triangle` block, and the `rhino3dm.Mesh` `triangle`/`quad` block onto one field; the named `point_data`/`cell_data`/`field_data` arrays cross by `ContentKey` reference into `tabular/egress#EGRESS`, never inlined in the frozen struct. `MeshReceipt` is the typed mesh receipt — backend, point count, cell blocks, point/cell/field array counts, units, content key — satisfying the runtime `ReceiptContributor` Protocol.
- Entry: `MeshPayload.read` admits a mesh file, loads the engine once through the resolved `_Backend.load`, projects the engine-agnostic `MeshFrame` through the row `frame`, and returns the frozen owner keyed by `ContentIdentity` over the canonical point bytes; `MeshPayload.arrays` materializes the named `point_data`/`cell_data` arrays as a `pyarrow.Table` for egress through one `_columns` fold over the one `_column` builder — a 1D array folds as `pyarrow.array`, a multi-dim array (the vertex-normal/color stacks) folds as `pyarrow.FixedSizeListArray.from_arrays` over the flat NumPy buffer, never a `.tolist()` row materialization; `MeshPayload.timeseries` streams an `meshio.xdmf.TimeSeriesReader` FE-result rail one timestep at a time as `(time, point_data, cell_data)` `pyarrow.Table` frames through the same `_columns` fold; `MeshPayload.preview` emits a `glb` preview render through the row exporter over the once-loaded engine; `MeshPayload.write` round-trips through the format the requested extension selects over the same once-loaded engine. Every entrypoint returns a `RuntimeRail`, the `boundary` narrowing `catch=` to the row fault root so a non-engine exception escapes rather than converting to a mesh fault.
- Auto: `MeshBackend.of` resolves the case off the source extension through the one `_EXT` table layered in explicit precedence — the `meshio.extension_to_filetypes` FE base, then the `trimesh` surface override winning the shared `.obj`/`.off`/`.ply`/`.stl` extensions, then the `rhino3dm` `.3dm` override — so the precedence is independent of the key-sorted `expression.Map` iteration order, defaulting an unrecognized suffix to the `meshio` tag rather than an `if`-ladder over the three sets; `trimesh` owns the surface (display/exchange) extensions, `meshio` owns the FE volume/cell-block formats keyed by `meshio.extension_to_filetypes` rather than an empty fallthrough set, and `rhino3dm` owns `.3dm`, and the row owns its `load`/`extract`/`export`/`units`/`fault` arms; a new surface format is one extension string on the `trimesh` row, a new FE format is already in `meshio.extension_to_filetypes`, and the `.3dm` row is the `rhino3dm` case folding `File3dm.Read` over every `File3dmObjectTable` mesh plus `DracoCompression` transport. The `meshio` row mines `Mesh.cells_dict` for the block topology, `Mesh.cell_data_dict` for the per-type named cell arrays, `Mesh.point_data` for the point arrays, and `Mesh.field_data` for the named scalar metadata; the `trimesh` row mines `Trimesh.faces` for the triangle block and connectivity cell array and the `ColorVisuals` `Trimesh.visual.vertex_colors` for the per-vertex color point array off the one `Trimesh.units` reader; the `rhino3dm` row null-checks `File3dm.Read` (the binding signals failure by null, never exception) before folding the object-table meshes, mining `Mesh.Vertices` for the point stack, `Mesh.Faces` for the triangle/quad block split, and `Mesh.Normals`/`Mesh.VertexColors` for the per-vertex normal and color point arrays so the `.3dm` row preserves the same per-vertex data the surface row does rather than dropping it, so the absent-file case raises a typed fault rather than the `None`-attribute crash the bare boundary masks. The cell-block topology, the named-array dicts, and the time-series rail (`xdmf.TimeSeriesReader.num_steps`/`read_data`) all read off the once-loaded engine the row selects, never a second loader.
- Receipt: a mesh read contributes an emitted-phase `Receipt.of` row through `ReceiptContributor.contribute` and produces a `MeshReceipt` keyed by the canonical point-bytes `ContentKey`, carrying the backend tag, point count, cell-block tuple, point/cell/field array counts, and units; the array egress reuses this content key for the per-array `tabular/egress#EGRESS` reference, never a second receipt rail.
- Packages: `meshio` (`read`/`write`/`Mesh`/`Mesh.points`/`Mesh.cells_dict`/`Mesh.cell_data_dict`/`Mesh.point_data`/`Mesh.field_data`/`extension_to_filetypes`/`ReadError`/`WriteError`/`xdmf.TimeSeriesReader.{read_points_cells,num_steps,read_data}`), `trimesh` (`load`/`Trimesh.export`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`/`Trimesh.visual`), `rhino3dm` (`File3dm.Read`/`File3dmObjectTable`/`Mesh`/`Mesh.Vertices`/`Mesh.Faces`/`Mesh.Normals`/`Mesh.VertexColors`/`DracoCompression.Compress`/`DracoCompressionOptions`), `laspy`/`lazrs`/`laszip` (the `[03]-[POINTCLOUD]` row), `numpy` (`ascontiguousarray`/`concatenate` the canonical array bytes), `pyarrow` (`array`/`table`/`FixedSizeListArray.from_arrays` the named-array egress columns), runtime (`ContentIdentity`/`ContentKey`/`ResourceRef`/`RuntimeRail`/`BoundaryFault`/`boundary`/`Receipt`/`ReceiptContributor`).
- Growth: a new surface format is one extension string on the `_BACKEND` `trimesh` row; a new FE format already routes through `meshio.extension_to_filetypes`; a new mesh engine is one `MeshBackend` case plus one `_Backend` row carrying its load/extract/export/units/fault arms; a new named array kind is one more dict the row `extract` folds into the `_Extract` data; named point/cell/field arrays are one `ContentKey` reference per array folded into `tabular/egress#EGRESS`; the FE time-series rail is the existing `timeseries` generator over `xdmf.TimeSeriesReader`; zero per-format `read_*`/`write_*` family, never a per-engine `match` arm beside the `_BACKEND` table, and never a parallel `MeshioPayload`/`TrimeshPayload`/`Rhino3dmPayload` triple.
- Boundary: no geometry kernel (that is the geometry package), no bridge lifecycle, no NURBS/Brep/SubD construction (the `rhino3dm` row reads `File3dm.Read` meshes only, the offline 3dm reader per the geometry-flow law, never a geometry kernel); the IFC-to-GLB tessellation rail is geometry-owned; the point-cloud row is host-free file exchange feeding the geometry scan companion at the wire, never a managed-interior coupling; a hand-rolled mesh parser, a parallel `MeshioPayload`/`TrimeshPayload` pair, an `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder triple beside the one `_frame` fold over the `_BACKEND` `extract` rows, a per-engine `match` arm in `frame`/`export`, an `if`-ladder `MeshBackend.of` beside the one `_EXT` table, an empty meshio fallthrough set where `meshio.extension_to_filetypes` enumerates the FE formats, a second engine load per operation, an un-narrowed `catch=Exception` boundary, an inlined large array in the frozen struct, an unchecked `File3dm.Read` null, a `.tolist()` row materialization where `FixedSizeListArray.from_arrays` folds the multi-dim buffer, an asymmetric `.3dm` row dropping the per-vertex normal/color arrays the surface row keeps, and a `rhino3dm` NURBS construction call are the deleted forms.

```python signature
from collections.abc import Callable, Iterator, Mapping
from typing import Any, Final, Literal

import meshio
import numpy as np
import pyarrow as pa
import rhino3dm
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

type CellType = Literal["vertex", "line", "triangle", "quad", "tetra", "hexahedron"]
type Arrays = Mapping[str, np.ndarray]
type Frames = Iterator[tuple[float, pa.Table, pa.Table]]


class ArrayRef(Struct, frozen=True):
    name: str
    content_key: ContentKey
    cell_type: CellType
    dtype: str


class _Extract(Struct, frozen=True):
    points: np.ndarray
    cell_blocks: tuple[str, ...]
    point_data: Arrays
    cell_data: Arrays
    field_data: Arrays


class MeshFrame(Struct, frozen=True):
    points: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: tuple[ArrayRef, ...]
    cell_arrays: tuple[ArrayRef, ...]
    field_arrays: tuple[ArrayRef, ...]
    point_data: Arrays
    cell_data: Arrays


def _ref(prefix: str, name: str, array: np.ndarray, cell_type: str) -> ArrayRef:
    return ArrayRef(name, ContentIdentity.of(f"{prefix}.{name}", np.ascontiguousarray(array).tobytes()), cell_type, str(array.dtype))  # type: ignore[arg-type]


def _frame(extract: _Extract) -> MeshFrame:
    canonical = np.ascontiguousarray(extract.points, dtype="float64")
    return MeshFrame(
        points=ContentIdentity.of("mesh", canonical.tobytes()), point_count=len(canonical), cell_blocks=extract.cell_blocks,
        point_arrays=tuple(_ref("mesh.point_data", name, array, "vertex") for name, array in extract.point_data.items()),
        cell_arrays=tuple(_ref("mesh.cell_data", name, array, name.rsplit(".", 1)[-1]) for name, array in extract.cell_data.items()),
        field_arrays=tuple(_ref("mesh.field_data", name, array, "vertex") for name, array in extract.field_data.items()),
        point_data=dict(extract.point_data), cell_data=dict(extract.cell_data))


def _meshio_extract(mesh: "meshio.Mesh") -> _Extract:
    return _Extract(
        points=np.asarray(mesh.points),
        cell_blocks=tuple(mesh.cells_dict.keys()),
        point_data={name: np.asarray(array) for name, array in mesh.point_data.items()},
        cell_data={f"{name}.{cell_type}": np.asarray(array)
                   for name, per_type in mesh.cell_data_dict.items() for cell_type, array in per_type.items()},
        field_data={name: np.asarray(array) for name, array in mesh.field_data.items()})


def _trimesh_extract(surface: "trimesh.Trimesh") -> _Extract:
    colors = getattr(surface.visual, "vertex_colors", None)
    return _Extract(
        points=np.asarray(surface.vertices),
        cell_blocks=("triangle",) if len(surface.faces) else (),
        point_data={"color": np.asarray(colors)} if colors is not None else {},
        cell_data={"triangle": np.asarray(surface.faces)} if len(surface.faces) else {},
        field_data={})


def _rhino3dm_extract(model: "rhino3dm.File3dm") -> _Extract:
    meshes = [obj.Geometry for obj in model.Objects if isinstance(obj.Geometry, rhino3dm.Mesh)]
    points = [np.array([(v.X, v.Y, v.Z) for v in mesh.Vertices], dtype="float64") for mesh in meshes]
    normals = [np.array([(n.X, n.Y, n.Z) for n in mesh.Normals], dtype="float32") for mesh in meshes if len(mesh.Normals)]
    colors = [np.array([tuple(c) for c in mesh.VertexColors], dtype="uint8") for mesh in meshes if len(mesh.VertexColors)]
    blocks = {"quad" if face[3] != face[2] else "triangle" for mesh in meshes for face in mesh.Faces}
    return _Extract(
        points=np.concatenate(points) if points else np.empty((0, 3), dtype="float64"),
        cell_blocks=tuple(sorted(blocks)),
        point_data=({"normal": np.concatenate(normals)} if normals else {}) | ({"color": np.concatenate(colors)} if colors else {}),
        cell_data={}, field_data={})


class _Backend(Struct, frozen=True):
    load: Callable[[ResourceRef], Any]
    extract: Callable[[Any], _Extract]
    export: Callable[[Any, ResourceRef, str], None]
    units: Callable[[Any], str]
    fault: type[Exception]
    exts: frozenset[str]

    def frame(self, engine: Any) -> MeshFrame:
        return _frame(self.extract(engine))


def _meshio_load(ref: ResourceRef) -> "meshio.Mesh":
    return meshio.read(str(ref.path))


def _trimesh_load(ref: ResourceRef) -> "trimesh.Trimesh":
    return trimesh.load(str(ref.path), force="mesh")


def _rhino3dm_load(ref: ResourceRef) -> "rhino3dm.File3dm":
    model = rhino3dm.File3dm.Read(str(ref.path))
    if model is None:
        raise FileNotFoundError(str(ref.path))
    return model


def _rhino3dm_export(model: "rhino3dm.File3dm", out: ResourceRef, fmt: str) -> None:
    meshes = [obj.Geometry for obj in model.Objects if isinstance(obj.Geometry, rhino3dm.Mesh)]
    if fmt == "drc" and meshes:
        out.path.write_bytes(b"".join(rhino3dm.DracoCompression.Compress(mesh, rhino3dm.DracoCompressionOptions()) for mesh in meshes))
    else:
        model.Write(str(out.path), 0)


_BACKEND: Final[Map[str, _Backend]] = Map.of_seq([
    ("rhino3dm", _Backend(_rhino3dm_load, _rhino3dm_extract, _rhino3dm_export, lambda _: "m", Exception, frozenset({".3dm"}))),
    ("trimesh", _Backend(_trimesh_load, _trimesh_extract,
        lambda mesh, out, fmt: mesh.export(str(out.path), file_type=fmt),
        lambda mesh: mesh.units or "m", Exception, frozenset({".stl", ".obj", ".ply", ".glb", ".gltf", ".off", ".3mf"}))),
    ("meshio", _Backend(_meshio_load, _meshio_extract,
        lambda mesh, out, fmt: meshio.write(str(out.path), mesh, file_format=fmt),
        lambda _: "m", meshio.ReadError, frozenset(meshio.extension_to_filetypes.keys()))),
])

_EXT: Final[dict[str, str]] = (
    {ext: "meshio" for ext in _BACKEND["meshio"].exts}
    | {ext: "trimesh" for ext in _BACKEND["trimesh"].exts}
    | {ext: "rhino3dm" for ext in _BACKEND["rhino3dm"].exts})


@tagged_union(frozen=True)
class MeshBackend:
    tag: Literal["meshio", "trimesh", "rhino3dm"] = tag()
    meshio: str = case()
    trimesh: str = case()
    rhino3dm: str = case()

    @staticmethod
    def of(ref: ResourceRef) -> "MeshBackend":
        suffix = ref.path.suffix.lower()
        return MeshBackend(**{_EXT.get(suffix, "meshio"): suffix})

    @property
    def row(self) -> _Backend:
        return _BACKEND[self.tag]


def _column(array: np.ndarray) -> pa.Array:
    flat = np.ascontiguousarray(array)
    return (pa.array(flat) if flat.ndim == 1
            else pa.FixedSizeListArray.from_arrays(pa.array(flat.reshape(-1)), flat.shape[1]))


def _columns(arrays: Arrays) -> pa.Table:
    return pa.table({name: _column(array) for name, array in arrays.items()})


class MeshReceipt(Struct, frozen=True):
    backend: str
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    units: str

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "mesh", self.content_key.hex,
            {"backend": self.backend, "points": str(self.point_count), "blocks": ",".join(self.cell_blocks),
             "point_arrays": str(self.point_arrays), "cell_arrays": str(self.cell_arrays),
             "field_arrays": str(self.field_arrays), "units": self.units})


class MeshPayload(Struct, frozen=True):
    backend: MeshBackend
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: tuple[ArrayRef, ...]
    cell_arrays: tuple[ArrayRef, ...]
    field_arrays: tuple[ArrayRef, ...]
    units: str

    @classmethod
    def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        backend = MeshBackend.of(ref)
        row = backend.row
        def run() -> "MeshPayload":
            engine = row.load(ref)
            return cls._build(backend, row.frame(engine), row.units(engine))
        return boundary("mesh.read", run, catch=row.fault)

    def arrays(self, ref: ResourceRef) -> "RuntimeRail[pa.Table]":
        row = self.backend.row
        def run() -> pa.Table:
            frame = row.frame(row.load(ref))
            return _columns({**frame.point_data, **frame.cell_data})
        return boundary("mesh.arrays", run, catch=row.fault)

    def timeseries(self, ref: ResourceRef) -> "RuntimeRail[Frames]":
        return boundary("mesh.timeseries", lambda: _timeseries(ref), catch=meshio.ReadError)

    def preview(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return self._emit(ref, out, "glb", "mesh.preview")

    def write(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return self._emit(ref, out, out.path.suffix.lstrip("."), "mesh.write")

    def contribute(self) -> Receipt:
        return MeshReceipt(self.backend.tag, self.content_key, self.point_count, self.cell_blocks,
            len(self.point_arrays), len(self.cell_arrays), len(self.field_arrays), self.units).contribute()

    @classmethod
    def _build(cls, backend: MeshBackend, frame: MeshFrame, units: str) -> "MeshPayload":
        return cls(backend, frame.points, frame.point_count, frame.cell_blocks,
            frame.point_arrays, frame.cell_arrays, frame.field_arrays, units)

    def _emit(self, ref: ResourceRef, out: ResourceRef, fmt: str, subject: str) -> "RuntimeRail[ContentKey]":
        row = self.backend.row
        def run() -> ContentKey:
            row.export(row.load(ref), out, fmt)
            return ContentIdentity.of("mesh.export", out.path.read_bytes())
        return boundary(subject, run, catch=row.fault)


def _timeseries(ref: ResourceRef) -> Frames:
    with meshio.xdmf.TimeSeriesReader(str(ref.path)) as reader:
        reader.read_points_cells()
        for step in range(reader.num_steps):
            time, point_data, cell_data = reader.read_data(step)
            yield (float(time), _columns({name: np.asarray(array) for name, array in point_data.items()}),
                   _columns({f"{name}.{cell_type}": np.asarray(array)
                             for name, per_type in cell_data.items() for cell_type, array in per_type.items()}))
```

## [03]-[POINTCLOUD]

- Owner: `PointCloud` — the LAS/LAZ/COPC interchange row over `laspy` alone: `laspy.read`/`LasData.write` for the full-file LAS/LAZ decode/encode and `laspy.copc.CopcReader` for the octree-chunked spatial subset over local paths and HTTP URLs. `PointBounds` is the axis-aligned octree query box folding into `laspy.copc.Bounds` and promoting a 2D box to 3D through the catalogued `Bounds.ensure_3d`; `CopcQuery` is the closed tagged union carrying the mutually-exclusive `bounds`/`resolution`/`level` query modalities the catalogue `query` law fixes. A `Selection` (`laspy.DecompressionSelection | None`) threads the catalogue-confirmed `decompression_selection` keyword identically through `laspy.read` and `CopcReader.open`/`query`, so a cloud subset skips RGB/GPS-time/extra-byte fields it never reads — one optional carrier on every entry, never a parallel selective-read method. The point records cross to the geometry scan companion through one polymorphic `_to_arrow` dimension fold serving both the full `laspy.LasData` read and the `ScaleAwarePointRecord` subset — the `LasData.xyz` coordinate stack columnated as `x`/`y`/`z` plus every `PointFormat.dimension_names` column off one iteration through one `_column` builder that folds a 1D dimension as `pyarrow.array` and a multi-dim dimension as `pyarrow.FixedSizeListArray.from_arrays` over the flat NumPy buffer rather than a `.tolist()` row materialization — keyed by `ContentIdentity` over the coordinate bytes, never a `laspy`- or `pdal`-specific object. The single `laspy` owner replaces the former `laspy`/`pdal` two-backend split — the data COPC arm is `laspy.copc`, and the geometry `pdal` filter-graph stays geometry-owned. `PointRecordTable` is the typed bridge receipt — point count, point-format id, CRS WKT, content key — satisfying the runtime `ReceiptContributor` Protocol so the columnar table crosses the seam content-keyed; `PointCloud` carries the same point-format id plus CRS WKT directly on the frozen owner, never a second `PointFormat` struct duplicating the LAS format id the receipt already names.
- Entry: `PointCloud.read` admits a LAS/LAZ/COPC `ResourceRef` plus an optional `Selection` and returns the frozen owner keyed by `ContentIdentity` over the `LasData.xyz` coordinate bytes, carrying the LAS point-format id and the CRS WKT recovered from `LasHeader.parse_crs`; `PointCloud.subset` reads a COPC octree spatial subset over a `CopcQuery` plus `Selection` through `laspy.copc.CopcReader.open(...).query(...)` rather than a full read, returning the content-keyed `PointRecordTable`; `PointCloud.to_arrow` folds the `laspy.LasData` dimensions under the same `Selection` into one `PointRecordTable` columnar bridge; `PointCloud.write` round-trips LAS/LAZ through `laspy.LasData.write`, the `do_compress` flag selecting LAS→LAZ transcode over the band-resolved `laz_backend` rather than a silent format-preserving copy. Every entrypoint returns a `RuntimeRail`, the `boundary` narrowing `catch=` to `laspy.LaspyException` so a non-`laspy` exception escapes rather than converting to a point-cloud fault.
- Auto: `CopcReader.open` reads the COPC `copc_info` eagerly and serves `query` over `http_num_threads`/`decompression_selection` only — the live `open` surface carries no `laz_backend` keyword, so COPC chunk decompression resolves `lazrs` internally and the lazrs-absent case raises a `LazError` (a `LaspyException`) the one `boundary` converts, never a caller-selected COPC backend keyword. The `_laz_backend` selector binds the first available `LazBackend.{LazrsParallel, Lazrs, Laszip}` member imported function-local on the `python_version<'3.15'` companion band under `# noqa: PLC0415`, and feeds only the `LasData.write` LAZ-transcode path where `laz_backend` is a real keyword, never a module-top import on this cp315-core page and never a `getattr` over backend name strings the enum already names. The `query` modality is the `CopcQuery` discriminant — a `PointBounds` box folded through `laspy.copc.Bounds(mins, maxs)`, the `resolution`/`level` LOD the mutually-exclusive query keywords the catalogue fixes — never a `full=True` knob; a `ScaleAwarePointRecord` returns from `query` and folds straight into the one `_to_arrow` bridge so the columnar table is the single egress shape both the full read and the subset produce, the `LasData.xyz` coordinate stack content-keyed identically across both legs. `laspy` is cp315-clean and imports module-top; the `DecompressionSelection.all`/field-mask producers that mint a `Selection` value stay `[04]-[RESEARCH]`-gated until the catalogue enumerates their member spelling, so the page composes only the live-confirmed `decompression_selection`/`laz_backend`/`do_compress` keywords and a caller-supplied `Selection` value.
- Boundary: no geometry kernel registration, no scan-to-BIM compute (that is the geometry scan companion), no `pdal` filter-graph (SMRF/PMF/outlier/decimation/range stays geometry-owned), no host coupling; the point-cloud row is host-free file exchange feeding the geometry companion at the wire (content-identity plus the columnar bridge), never a managed-interior coupling; a hand-rolled LAS parser, a hand-rolled `column_stack` where `LasData.xyz` answers, a `_record_to_arrow`/`_las_to_arrow` twin beside the one `_to_arrow` fold, a second `PointFormat` struct duplicating the format id the receipt and owner already carry, a `.tolist()` row materialization where `FixedSizeListArray.from_arrays` folds the multi-dim buffer, a `CopcCompression` StrEnum keyed on the unverified `are_points_compressed` probe, an un-narrowed `catch=Exception` boundary, a `getattr` over `LazBackend` name strings, a `_open_copc_compressed` twin beside the one `_laz_backend` selector, an `HTTP_THREADS` literal restating the catalogue `http_num_threads` default for the local-path leg, a `pdal`-specific object crossing the seam, a `pdal` module-top import, a `PointBounds.as_pdal` PDAL-pipeline string, a write that drops the `do_compress`/`laz_backend` transcode keywords, and a filename-keyed compression guess are the deleted forms.

```python signature
from collections.abc import Sequence
from typing import Any, Final, Literal, assert_never

import laspy
import laspy.copc
import numpy as np
import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

type Record = laspy.LasData | laspy.ScaleAwarePointRecord
type Selection = laspy.DecompressionSelection | None

COPC_HTTP: Final[int] = 80


class PointBounds(Struct, frozen=True):
    minx: float
    miny: float
    minz: float = -np.inf
    maxx: float = np.inf
    maxy: float = np.inf
    maxz: float = np.inf

    def as_copc(self) -> "laspy.copc.Bounds":
        return laspy.copc.Bounds(
            mins=np.array([self.minx, self.miny, self.minz], dtype="float64"),
            maxs=np.array([self.maxx, self.maxy, self.maxz], dtype="float64")).ensure_3d(
            np.array([self.minx, self.miny], dtype="float64"), np.array([self.maxx, self.maxy], dtype="float64"))


@tagged_union(frozen=True)
class CopcQuery:
    tag: Literal["bounds", "resolution", "level"] = tag()
    bounds: PointBounds = case()
    resolution: tuple[PointBounds, float] = case()
    level: tuple[PointBounds, int] = case()

    def query(self, reader: "laspy.copc.CopcReader") -> "laspy.ScaleAwarePointRecord":
        match self:
            case CopcQuery(tag="bounds", bounds=box):
                return reader.query(bounds=box.as_copc())
            case CopcQuery(tag="resolution", resolution=(box, resolution)):
                return reader.query(bounds=box.as_copc(), resolution=resolution)
            case CopcQuery(tag="level", level=(box, level)):
                return reader.query(bounds=box.as_copc(), level=level)
            case _ as unreachable:
                assert_never(unreachable)


def _column(array: np.ndarray) -> pa.Array:
    flat = np.ascontiguousarray(array)
    return (pa.array(flat) if flat.ndim == 1
            else pa.FixedSizeListArray.from_arrays(pa.array(flat.reshape(-1)), flat.shape[1]))


def _coords(record: Record) -> np.ndarray:
    return np.ascontiguousarray(record.xyz, dtype="float64")


def _read(path: str, selection: Selection) -> "laspy.LasData":
    return laspy.read(path, decompression_selection=selection) if selection is not None else laspy.read(path)


def _to_arrow(record: Record) -> pa.Table:
    coords = _coords(record)
    base = {"x": coords[:, 0], "y": coords[:, 1], "z": coords[:, 2]}
    columns = base | {name: np.asarray(record[name]) for name in record.point_format.dimension_names if name not in base}
    return pa.table({name: _column(array) for name, array in columns.items()})


class PointRecordTable(Struct, frozen=True):
    table: pa.Table
    point_count: int
    point_format: int
    crs_wkt: str
    content_key: ContentKey

    @classmethod
    def of(cls, record: Record, crs_wkt: str = "") -> "PointRecordTable":
        coords = _coords(record)
        return cls(table=_to_arrow(record), point_count=len(coords), point_format=int(record.point_format.id),
            crs_wkt=crs_wkt, content_key=ContentIdentity.of("pointcloud", coords.tobytes()))

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "pointcloud", self.content_key.hex,
            {"points": str(self.point_count), "format": str(self.point_format), "crs": self.crs_wkt})


class PointCloud(Struct, frozen=True):
    content_key: ContentKey
    point_count: int
    point_format: int
    crs_wkt: str

    @classmethod
    def read(cls, ref: ResourceRef, selection: Selection = None) -> "RuntimeRail[PointCloud]":
        def run() -> "PointCloud":
            data = _read(str(ref.path), selection)
            return cls(content_key=ContentIdentity.of("pointcloud", _coords(data).tobytes()),
                point_count=int(data.header.point_count), point_format=int(data.header.point_format.id),
                crs_wkt=str(data.header.parse_crs() or ""))
        return boundary("pointcloud.read", run, catch=laspy.LaspyException)

    @staticmethod
    def subset(ref: ResourceRef, query: CopcQuery, selection: Selection = None) -> "RuntimeRail[PointRecordTable]":
        return boundary("pointcloud.subset", lambda: PointRecordTable.of(query.query(_open_copc(str(ref.path), selection))), catch=laspy.LaspyException)

    @staticmethod
    def to_arrow(ref: ResourceRef, selection: Selection = None) -> "RuntimeRail[PointRecordTable]":
        def run() -> PointRecordTable:
            data = _read(str(ref.path), selection)
            return PointRecordTable.of(data, crs_wkt=str(data.header.parse_crs() or ""))
        return boundary("pointcloud.to_arrow", run, catch=laspy.LaspyException)

    def write(self, source: ResourceRef, out: ResourceRef, do_compress: bool | None = None) -> "RuntimeRail[ContentKey]":
        def run() -> ContentKey:
            data = laspy.read(str(source.path))
            data.write(str(out.path), do_compress=do_compress, laz_backend=_laz_backend()) if do_compress else data.write(str(out.path))
            return ContentIdentity.of("pointcloud.write", out.path.read_bytes())
        return boundary("pointcloud.write", run, catch=laspy.LaspyException)

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "pointcloud", self.content_key.hex,
            {"points": str(self.point_count), "format": str(self.point_format), "crs": self.crs_wkt})


def _laz_backend() -> "Any":
    from laspy import LazBackend  # noqa: PLC0415

    backend = next((b for b in (LazBackend.LazrsParallel, LazBackend.Lazrs, LazBackend.Laszip) if b.is_available()), None)
    if backend is None:
        raise laspy.LaspyException("compressed LAZ/COPC requires lazrs or laszip on the python_version<'3.15' companion band")
    return backend


def _open_copc(path: str, selection: Selection) -> "laspy.copc.CopcReader":
    http = COPC_HTTP if path.startswith(("http://", "https://")) else 1
    args: dict[str, Any] = {"http_num_threads": http} | ({"decompression_selection": selection} if selection is not None else {})
    return laspy.copc.CopcReader.open(path, **args)
```

## [04]-[RESEARCH]

- [MESH_BACKEND]: the `_BACKEND` row members the three engines transcribe are catalogue-confirmed — `meshio.read`/`write`/`Mesh.points`/`Mesh.cells_dict`/`Mesh.cell_data_dict`/`Mesh.point_data`/`Mesh.field_data`/`extension_to_filetypes`/`ReadError`/`WriteError`/`xdmf.TimeSeriesReader.{read_points_cells,num_steps,read_data}` against `.api/meshio.md` (PUBLIC_TYPES [01]-[04], ENTRYPOINTS top-level [01]-[02]/[06], `Mesh` accessors [04]-[05], per-format [03]-[04], IMPLEMENTATION_LAW `field_data`), with `extension_to_filetypes` the ENTRYPOINTS [06] format-registry dict the `meshio` row folds into its extension set rather than an empty fallthrough; `trimesh.load(force="mesh")`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`/`Trimesh.visual`/`Trimesh.export(file_type=)` against `.api/trimesh.md` (PUBLIC_TYPES [01]/[07], ENTRYPOINTS [01]/[07], `MESH_AEC` load/mesh axis); `rhino3dm.File3dm.Read`/`File3dmObjectTable`/`Mesh.Vertices`/`Mesh.Faces`/`Mesh.Normals`/`Mesh.VertexColors`/`DracoCompression.Compress`/`DracoCompressionOptions` against `.api/rhino3dm.md` (PUBLIC_TYPES enumerating `MeshVertexList`/`MeshFaceList`/`MeshNormalList`/`MeshVertexColorList`, ENTRYPOINTS document IO/mesh build/compression). The `File3dm.Read` null-on-failure contract the `_rhino3dm_load` guard reads is the catalogue IMPLEMENTATION_LAW law ("signals failure by null rather than exception"), so the typed `FileNotFoundError` raise is settled. The `rhino3dm.Mesh.Faces` 4-int-tuple subscript the `face[3] != face[2]` quad probe reads, the `Mesh.Normals` element subscript (`n.X`/`n.Y`/`n.Z` on the `MeshNormalList` `Vector3f`), and the `Mesh.VertexColors` element as a 4-int `(R, G, B, A)` tuple the `_rhino3dm_extract` folds through `tuple(c)` into the `normal`/`color` point arrays are live-reflection-confirmed against the installed cp315 `rhino3dm` distribution — the `MeshVertexColorList` element is a plain tuple, not a `Color` object with `.R`/`.G`/`.B`/`.A` accessors, so the fence reads `tuple(c)` rather than per-channel attributes. `Trimesh.visual.vertex_colors` (the `ColorVisuals` per-vertex color member the `_trimesh_extract` mines under a defensive `getattr`) stays marked RESEARCH until the catalogue `ColorVisuals` row enumerates the member against the installed distribution; `trimesh` reflects on cp313, so it is docs-derived pending env provisioning. `pyarrow.FixedSizeListArray.from_arrays(values, list_size)` (the multi-dim column builder the `_column` fold composes over the flat NumPy buffer for the vertex-normal/color stacks) is live-reflection-confirmed against the installed cp315 `pyarrow` distribution, the `.api/pyarrow.md` PUBLIC_TYPES nested-type [03] `FixedSizeListArray` carrying the `from_arrays` classmethod, so both the 1D `pyarrow.array` leg and the multi-dim `FixedSizeListArray.from_arrays` leg are settled fence code.
- [LASPY_SURFACE]: the `laspy` `read(source, *, laz_backend, decompression_selection)`/`LasData.{x,y,z,xyz,header,write}`/`LasData.write(destination, *, do_compress, laz_backend)`/`LasHeader.{point_count,point_format,parse_crs}`/`PointFormat.{id,dimension_names}`/`LaspyException` surface the `PointCloud` LAS/LAZ arm transcribes is catalogue-confirmed against `.api/laspy.md` (ENTRYPOINTS [01]-[07], PUBLIC_TYPES [01]-[04] enumerating `LasData.xyz` and the `__getitem__` dimension access); `laspy` (2.7.0) is the pure-Python cp315-clean wheel reflection-verified in the catalogue surface, so the LAS point-record member surface — the `LasData.xyz` coordinate stack the `_coords` fold reads in place of a hand-rolled `column_stack`, the `record[name]` dimension subscript the one `_to_arrow` fold reads across both the `LasData` and `ScaleAwarePointRecord` receivers, the `decompression_selection` keyword the `read`/`to_arrow`/`subset` entries thread, and the `do_compress`/`laz_backend` keywords the `write` LAS→LAZ transcode passes — is settled fence code. The `laspy.DecompressionSelection` value the `Selection` carrier types is a caller-supplied opaque mask; its constructor and `.all()`/field-mask producer members stay `[LAZRS_LASZIP_BACKEND]`-gated, so the fence only threads a supplied `Selection` and never mints one.
- [LASPY_COPC]: the `laspy.copc` octree-subset surface — `CopcReader.open(source, *, http_num_threads, decompression_selection) -> CopcReader`, `CopcReader.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord`, `Bounds(mins, maxs)`/`Bounds.ensure_3d(mins, maxs)`, and `ScaleAwarePointRecord` with scaled `x`/`y`/`z`/`xyz` — is live-reflection-confirmed against the installed cp315 `laspy.copc` distribution and `.api/laspy.md` COPC_SUBSET ([01]-[03] types, [01]-[04] surfaces) plus IMPLEMENTATION_LAW `[COPC_TOPOLOGY]`; the `bounds`/`resolution`/`level` mutual-exclusion the `CopcQuery.query` dispatcher encodes is the catalogue `query` law (`resolution` and `level` mutually exclusive), the `http_num_threads` and `decompression_selection` keywords the HTTP-URL COPC read passes are the live `open` surface, and `Bounds.ensure_3d` is COPC_SUBSET [04]. The catalogue COPC_SUBSET [01] `CopcReader.open(... laz_backend ...)` keyword is a catalogue error the live `open` signature `(source, http_num_threads, _http_strategy, decompression_selection)` refutes — COPC requires `lazrs` and resolves it internally, raising `LazError` (a `LaspyException`) when absent, so the `_open_copc` arm passes no `laz_backend` and the page never transcribes that phantom keyword. The cp315-core uncompressed-COPC read and the one `_to_arrow` fold over the returned `ScaleAwarePointRecord` are settled fence code.
- [LAZRS_LASZIP_BACKEND]: the LAZ-write arm binds `laspy.LazBackend.{Lazrs, LazrsParallel, Laszip}` selected function-local through the one `_laz_backend` selector and passed as the `LasData.write` `laz_backend` keyword, riding `lazrs` (`.api/lazrs.md`, the default Rust `laz-rs` codec backend, `LazVlr`/`LasZipDecompressor`/`ParLasZipDecompressor` composed by `laspy`) and `laszip` (`.api/laszip.md`, the native LASzip `LasUnZipper`/`LasZipper` backend `laspy` invokes as `LazBackend.Laszip`) on the `python_version<'3.15'` companion band declared in the one branch manifest (`lazrs; python_version<'3.15'`, `laszip; python_version<'3.15'`). The `LazBackend.{Lazrs, LazrsParallel, Laszip}` member spelling, the per-backend `LazBackend.<member>.is_available()` probe the selector iterates, and the `LasData.write(... laz_backend=...)` keyword are live-reflection-confirmed against the installed cp315 `laspy` distribution, so the `_laz_backend` selector composes the `LazBackend` members directly through their named attributes rather than a `getattr` over name strings or a constructed `LasZipDecompressor`. The COPC subset takes no `laz_backend` (the `[LASPY_COPC]` finding) — `lazrs` is the internally-resolved required COPC codec — so the selector feeds only the `write` transcode and the compressed-COPC path raises `LazError` through the one `boundary` when `lazrs` is absent. The `laspy.DecompressionSelection.all()`/field-mask producer that mints a `Selection` value stays RESEARCH-gated until the `.api/laspy.md` PUBLIC_TYPES `DecompressionSelection` row enumerates its constructor and producer-member spelling against the installed distribution; the fence threads only the live-confirmed caller-supplied `decompression_selection` keyword and never constructs the mask, the `laszip.DECOMPRESS_SELECTIVE_*` and `lazrs.DecompressionSelection` codec-internal mask constants (`.api/laszip.md` ENTRYPOINTS module-level, `.api/lazrs.md` block-functions [07]) staying the companion-band codec surface `laspy` owns rather than a data-page member.

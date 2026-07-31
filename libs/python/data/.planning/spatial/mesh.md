# [PY_DATA_MESH]

Mesh-file exchange owner over a `MeshBackend` axis with the point-cloud interchange row and the produced-archive sink: `MeshPayload` carries mesh-file identity, cell-block topology, units, named array arities, the FE time-series rail, and preview export over one `_BACKEND` behavior table — `meshio` for FE volume/cell-block meshes, `trimesh` for surface meshes, `rhino3dm` for `.3dm` exchange — `PointCloud` is the LAS/LAZ/COPC row over `laspy` alone, and `ProductKind` is the peer axis over the produced byte payloads a geometry analysis hands down for durable landing. This is file exchange and identity: the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here, the geometry `pdal` filter-graph stays geometry-owned, and the product sink lands bytes a producer already minted rather than authoring any of them.

Every payload keys by runtime `ContentIdentity` over the canonical `float64` point buffer, and the named-array egress rides the shared `tabular/columnar#SCAN` `QueryReceipt.railed` Arrow rail — the same `(table, QueryReceipt)` pair every sibling Arrow producer returns. Source engines load exactly once per operation and threads through the row reader, so `read`/`arrays`/`preview`/`write` never re-open the file. Network-bearing COPC reads route through `guarded(RetryClass.HTTP, on_thread, ...)`, the `THREAD_BAND`-bounded hop — the same retry/span/lift triplet the sibling spatial pages delegate to the runtime resilience owner.

## [01]-[INDEX]

- [02]-[MESH]: the `MeshPayload` owner over the `_BACKEND` behavior table — topology, named arrays, time-series, preview, write.
- [03]-[POINTCLOUD]: the `PointCloud` LAS/LAZ/COPC row — octree subset, the columnar point-record bridge, the remote resilience envelope.
- [04]-[PRODUCT]: the `ProductKind` produced-archive sink — the BCF/cost/diff byte payloads the IFC analysis and costing planes defer here, landed content-keyed.

## [02]-[MESH]

- Owner: each `_Backend` row pairs the loader, the engine-specific `extract` pack, the exporter, the unit reader, the per-engine fault set the `boundary` narrows `catch=` to, and the extension set, so engine variation collapses to one row, never a parallel per-engine builder family or a per-engine `match` arm in `frame`/`export`.
- Auto: `MeshBackend.of` resolves the case off the source extension through the `_EXT` table, an unrecognized suffix defaulting to the `meshio` tag. `rhino3dm` rows read units from `File3dm.Settings.ModelUnitSystem.name` (default `Millimeters`), never a hardcoded `"m"`.
- Receipt: `MeshReceipt` (the geometry/topology proof) and `QueryReceipt` (the columnar table proof) are two typed receipts disjoint by evidence axis, never one rail straddling both.
- Growth: a new surface format is one extension string on the `trimesh` row; a new FE format already routes through `meshio.extension_to_filetypes`; a new engine is one `MeshBackend` case plus one `_Backend` row; a new named array kind is one more dict the row `extract` folds, surfacing as one more egress column and receipt arity with no frame edit; zero per-format `read_*`/`write_*` family.
- Boundary: no geometry kernel, no bridge lifecycle, no NURBS/Brep/SubD construction — the `rhino3dm` row reads `File3dm.Read` meshes only, the offline 3dm reader per the geometry-flow law.

```python signature
from collections.abc import Callable, Iterator, Mapping
from typing import Final, Literal

import meshio
import numpy as np
import pyarrow as pa
import rhino3dm
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary, scoped
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.mesh")

type Engine = meshio.Mesh | trimesh.Trimesh | rhino3dm.File3dm
type Arrays = Mapping[str, np.ndarray]
type Frames = Iterator[tuple[float, pa.Table, pa.Table]]


class _Extract(Struct, frozen=True):
    points: np.ndarray
    cell_blocks: tuple[str, ...]
    point_data: Arrays
    cell_data: Arrays
    field_data: Arrays


# frame carries only the canonical-geometry identity (the `float64` point-buffer `ContentKey`) plus the named-array arities the receipt
# reports; the egress-driving column NAMES live on the transient `_Extract` the `arrays` fence reads, never re-stored. Mesh identity is its point
# geometry, so `_frame` is one `ContentIdentity.of(...).map(...)` rail, never a per-array key-derivation fold whose keys no consumer reads off the
# once-dropped buffers.
class MeshFrame(Struct, frozen=True):
    points: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int


def _frame(extract: _Extract) -> RuntimeRail[MeshFrame]:
    canonical = np.ascontiguousarray(extract.points, dtype="float64")
    return ContentIdentity.of("mesh", canonical.tobytes()).map(
        lambda points: MeshFrame(
            points, len(canonical), extract.cell_blocks, len(extract.point_data), len(extract.cell_data), len(extract.field_data)
        )
    )


def _meshio_extract(mesh: "meshio.Mesh") -> _Extract:
    return _Extract(
        points=np.asarray(mesh.points),
        cell_blocks=tuple(mesh.cells_dict.keys()),
        point_data={name: np.asarray(array) for name, array in mesh.point_data.items()},
        cell_data={
            f"{name}.{cell_type}": np.asarray(array) for name, per_type in mesh.cell_data_dict.items() for cell_type, array in per_type.items()
        },
        field_data={name: np.asarray(array) for name, array in mesh.field_data.items()},
    )


def _trimesh_extract(surface: "trimesh.Trimesh") -> _Extract:
    # `Trimesh.visual` is the `ColorVisuals | TextureVisuals` union: only `ColorVisuals` carries the `vertex_colors` `(N,4)` `uint8` RGBA property
    # (synthesizing defaults when no color is defined), while `TextureVisuals` exposes UV/material and no per-vertex color, so the color point array
    # is a typed `isinstance` discriminant over the union rather than a stringly-typed `getattr` probe — the texture arm yields no color array (the
    # PIL-backed `to_color` bake is outside this exchange owner).
    visual = surface.visual
    color = {"color": np.asarray(visual.vertex_colors)} if isinstance(visual, trimesh.visual.ColorVisuals) else {}
    return _Extract(
        points=np.asarray(surface.vertices),
        cell_blocks=("triangle",) if len(surface.faces) else (),
        point_data=color,
        cell_data={"triangle": np.asarray(surface.faces)} if len(surface.faces) else {},
        field_data={},
    )


# A per-vertex aux stack (`normal`/`color`) is emitted only when EVERY object-table mesh carries it, so the concatenated array is row-aligned with the
# vertex stack `point_count` keys: a mixed model where one mesh defines normals and another does not would otherwise concatenate a short array a
# "vertex"-keyed consumer reads off the end of, so the absent-on-any case drops the whole aux array rather than minting a length-mismatched one —
# `all(...)`-gated `_stack` fold, never a filtered `[... for mesh in meshes if len(mesh.Normals)]` comprehension tracking the present subset.
def _rhino3dm_extract(model: "rhino3dm.File3dm") -> _Extract:
    meshes = [obj.Geometry for obj in model.Objects if isinstance(obj.Geometry, rhino3dm.Mesh)]
    points = [np.array([(v.X, v.Y, v.Z) for v in mesh.Vertices], dtype="float64") for mesh in meshes]
    normals = _stack(meshes, lambda mesh: [(n.X, n.Y, n.Z) for n in mesh.Normals], "float32")
    colors = _stack(meshes, lambda mesh: [tuple(c) for c in mesh.VertexColors], "uint8")
    blocks = {"quad" if face[3] != face[2] else "triangle" for mesh in meshes for face in mesh.Faces}
    return _Extract(
        points=np.concatenate(points) if points else np.empty((0, 3), dtype="float64"),
        cell_blocks=tuple(sorted(blocks)),
        point_data={name: stack for name, stack in (("normal", normals), ("color", colors)) if stack is not None},
        cell_data={},
        field_data={},
    )


def _stack(meshes: list["rhino3dm.Mesh"], rows: "Callable[[rhino3dm.Mesh], list[tuple[float, ...]]]", dtype: str) -> np.ndarray | None:
    per_mesh = [rows(mesh) for mesh in meshes]
    return (
        np.concatenate([np.array(rs, dtype=dtype) for rs in per_mesh])
        if meshes and all(len(rs) == len(mesh.Vertices) for rs, mesh in zip(per_mesh, meshes, strict=True))
        else None
    )


class _Backend(Struct, frozen=True):
    load: Callable[[ResourceRef], Engine]
    extract: Callable[[Engine], _Extract]
    export: Callable[[Engine, ResourceRef, str], None]
    units: Callable[[Engine], str]
    fault: type[Exception] | tuple[type[Exception], ...]
    exts: frozenset[str]

    def frame(self, engine: Engine) -> RuntimeRail[MeshFrame]:
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


# Each row narrows `fault` to the engine's real raise surface so a non-engine exception escapes rather than masquerading as a mesh fault: `rhino3dm`
# signals load failure by null (the `_rhino3dm_load` `FileNotFoundError`) plus `OSError` on the Draco/`Write` egress; `trimesh.load` raises a bare
# `ValueError` on a malformed/unknown source plus `OSError`; `meshio` carries its own `ReadError`/`WriteError` codec roots. The tuple `catch` is the
# `runtime/reliability/faults#FAULT` `boundary` widening (`type[Exception] | tuple[...]`) the `except` clause accepts natively.
_BACKEND: Final[Map[str, _Backend]] = Map.of_seq([
    (
        "rhino3dm",
        _Backend(
            _rhino3dm_load,
            _rhino3dm_extract,
            _rhino3dm_export,
            lambda model: model.Settings.ModelUnitSystem.name.lower(),
            (FileNotFoundError, OSError),
            frozenset({".3dm"}),
        ),
    ),
    (
        "trimesh",
        _Backend(
            _trimesh_load,
            _trimesh_extract,
            lambda mesh, out, fmt: mesh.export(str(out.path), file_type=fmt),
            lambda mesh: mesh.units or "m",
            (ValueError, OSError),
            frozenset({".stl", ".obj", ".ply", ".glb", ".gltf", ".off", ".3mf"}),
        ),
    ),
    (
        "meshio",
        _Backend(
            _meshio_load,
            _meshio_extract,
            lambda mesh, out, fmt: meshio.write(str(out.path), mesh, file_format=fmt),
            lambda _: "m",
            (meshio.ReadError, meshio.WriteError),
            frozenset(meshio.extension_to_filetypes.keys()),
        ),
    ),
])

# precedence is made EXPLICIT before the rail: the plain-dict left-to-right union fixes
# later-key-wins (trimesh overrides the shared .obj/.off/.ply/.stl meshio rows, rhino3dm owns
# .3dm), then the settled mapping lifts onto the one `Map` rail — the precedence never rests
# on a map iteration order because it is resolved in the dict union before `of_seq` sees it.
_EXT: Final[Map[str, str]] = Map.of_seq(
    (
        {ext: "meshio" for ext in _BACKEND["meshio"].exts}
        | {ext: "trimesh" for ext in _BACKEND["trimesh"].exts}
        | {ext: "rhino3dm" for ext in _BACKEND["rhino3dm"].exts}
    ).items()
)


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
    return pa.array(flat) if flat.ndim == 1 else pa.FixedSizeListArray.from_arrays(pa.array(flat.reshape(-1)), flat.shape[1])


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

    def contribute(self) -> Iterator[Receipt]:
        # `domain`/`kind`/`key` are the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads —
        # the SAME pair handed `Metrics.record` beside the identity this payload minted — so the durable row lands in
        # the `mesh` partition a predicate prunes and rejoins the live series its twin emitted. Cells stay a receipt
        # fact rather than a second instrument: one vertex stack is the volume a regression moves, and a per-block
        # cell count is a topology shape a distribution over one number cannot carry.
        Metrics.record({"rasm.mesh.points": float(self.point_count)}, domain="mesh", kind=self.backend)
        yield Receipt.of(
            "mesh",
            (
                "emitted",
                self.content_key.hex,
                {
                    "domain": "mesh",
                    "kind": self.backend,
                    "key": self.content_key.hex,
                    "backend": self.backend,
                    "points": self.point_count,
                    "blocks": ",".join(self.cell_blocks),
                    "point_arrays": self.point_arrays,
                    "cell_arrays": self.cell_arrays,
                    "field_arrays": self.field_arrays,
                    "units": self.units,
                },
            ),
        )


class MeshPayload(Struct, frozen=True):
    backend: MeshBackend
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    units: str

    @classmethod
    async def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        # a whole-file provider load blocks on disk — the banded thread hop, never the loop.
        backend = MeshBackend.of(ref)
        row = backend.row

        def run() -> RuntimeRail[MeshPayload]:
            engine = row.load(ref)
            return row.frame(engine).map(lambda frame: cls._build(backend, frame, row.units(engine)))

        with _TRACER.start_as_current_span("mesh.read", attributes={"rasm.mesh.backend": backend.tag}):
            return (await async_boundary("mesh.read", lambda: on_thread(run), catch=row.fault)).bind(lambda rail: rail)

    async def arrays(self, ref: ResourceRef) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
        row = self.backend.row

        def run() -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
            extract = row.extract(row.load(ref))
            table = _columns({**extract.point_data, **extract.cell_data})
            return QueryReceipt.railed(self.backend.tag, self.content_key.hex, table).map(lambda receipt: (table, receipt))

        with _TRACER.start_as_current_span("mesh.arrays", attributes={"rasm.mesh.backend": self.backend.tag}):
            return (await async_boundary("mesh.arrays", lambda: on_thread(run), catch=row.fault)).bind(lambda rail: rail)

    async def timeseries(self, ref: ResourceRef) -> "RuntimeRail[Frames]":
        # reader open and `read_points_cells` (where `ReadError` surfaces) run eagerly inside the
        # fence; only the per-step `read_data` loop stays lazy, its provider-fault lift deferred to the
        # consumer that drains it — the same STREAM-arm convention `runtime/transport/roots#RESOURCE` holds, the
        # generator's own `with` owning the HDF5 `TimeSeriesReader.__exit__` close on exhaustion or break.
        def run() -> Frames:
            reader = meshio.xdmf.TimeSeriesReader(str(ref.path))
            reader.read_points_cells()
            return _frames(reader)

        with _TRACER.start_as_current_span("mesh.timeseries"):
            return await async_boundary("mesh.timeseries", lambda: on_thread(run), catch=(meshio.ReadError, meshio.WriteError))

    async def preview(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return await self._emit(ref, out, "glb", "mesh.preview")

    async def write(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return await self._emit(ref, out, out.path.suffix.lstrip("."), "mesh.write")

    def contribute(self) -> Iterator[Receipt]:
        return MeshReceipt(
            self.backend.tag, self.content_key, self.point_count, self.cell_blocks, self.point_arrays, self.cell_arrays, self.field_arrays, self.units
        ).contribute()

    @classmethod
    def _build(cls, backend: MeshBackend, frame: MeshFrame, units: str) -> "MeshPayload":
        return cls(backend, frame.points, frame.point_count, frame.cell_blocks, frame.point_arrays, frame.cell_arrays, frame.field_arrays, units)

    async def _emit(self, ref: ResourceRef, out: ResourceRef, fmt: str, subject: str) -> "RuntimeRail[ContentKey]":
        # load-export-readback is disk-bound end to end — the banded thread hop, never the loop.
        row = self.backend.row

        def run() -> RuntimeRail[ContentKey]:
            row.export(row.load(ref), out, fmt)
            return ContentIdentity.of("mesh.export", out.path.read_bytes())

        with _TRACER.start_as_current_span(subject, attributes={"rasm.mesh.backend": self.backend.tag, "rasm.mesh.format": fmt}):
            return (await async_boundary(subject, lambda: on_thread(run), catch=row.fault)).bind(lambda rail: rail)


def _frames(reader: "meshio.xdmf.TimeSeriesReader") -> Frames:
    with reader:
        for step in range(reader.num_steps):
            time, point_data, cell_data = reader.read_data(step)
            yield (
                float(time),
                _columns({name: np.asarray(array) for name, array in point_data.items()}),
                _columns({f"{name}.{cell_type}": np.asarray(array) for name, per_type in cell_data.items() for cell_type, array in per_type.items()}),
            )
```

## [03]-[POINTCLOUD]

- Owner: `PointCloud` — the LAS/LAZ/COPC row over `laspy` alone. A `Selection` threads the `decompression_selection` mask identically through `laspy.read` and `CopcReader.open`, fixed once at `open`, so a cloud subset skips fields it never reads — one optional carrier, never a parallel selective-read method. This owner carries the point-format id plus CRS WKT directly, never a second `PointFormat` struct duplicating what the receipt names.
- Entry: the remote COPC leg drives the blocking eager `open`-plus-`query` sequence through the `guarded` HTTP envelope — `CopcReader.open` reads the `copc_info` header and root octree page eagerly over `requests` before `query` pages a chunk, which is why the leg is network-bearing — while a local-path source threads the same body off the event loop with no retry budget; the subset keeps the COPC header's declared CRS, never dropping it to an empty string. `write` routes the closed `WriteMode` through the one `_WRITE` table, `compress`/`store`/`preserve` one row each.
- Boundary: no geometry kernel registration, no scan-to-BIM compute, no `pdal` filter-graph, no host coupling — host-free file exchange feeding the geometry companion at the wire; the point records cross as one content-keyed `PointRecordTable` through the shared `_column` builder, never a `laspy`- or `pdal`-specific object and never a re-spelled column fold.

```python signature
from collections.abc import Callable, Iterator
from typing import Final, Literal, assert_never

import laspy
import laspy.copc
import numpy as np
import pyarrow as pa
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import SpanKind

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef

# `_TRACER` and `_column` are the [02]-[MESH] owners in this same module: one module-scope scope handle serves every
# leg, so a second `scoped` mint beside it would fork one instrumentation coordinate in two.

type Record = laspy.LasData | laspy.ScaleAwarePointRecord
type Selection = laspy.DecompressionSelection | None
type WriteMode = Literal["compress", "store", "preserve"]


class PointBounds(Struct, frozen=True):
    minx: float
    miny: float
    minz: float = -np.inf
    maxx: float = np.inf
    maxy: float = np.inf
    maxz: float = np.inf

    def as_copc(self) -> "laspy.copc.Bounds":
        # a fully-3D `Bounds` built directly: the `minz`/`maxz` defaults are `±inf` so an unset Z
        # box admits every depth (the catalogue "2D bounds skip Z filtering" outcome) without the
        # `ensure_3d` 2D-promotion, which is a pure no-op over an already-3D `mins`/`maxs` pair.
        return laspy.copc.Bounds(
            mins=np.array([self.minx, self.miny, self.minz], dtype="float64"), maxs=np.array([self.maxx, self.maxy, self.maxz], dtype="float64")
        )


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


def _coords(record: Record) -> np.ndarray:
    return np.ascontiguousarray(record.xyz, dtype="float64")


def _read(path: str, selection: Selection) -> "laspy.LasData":
    return laspy.read(path, decompression_selection=selection) if selection is not None else laspy.read(path)


def _to_arrow(record: Record) -> pa.Table:
    coords = _coords(record)
    base = {"x": coords[:, 0], "y": coords[:, 1], "z": coords[:, 2]}
    columns = base | {name: np.asarray(record[name]) for name in record.point_format.dimension_names if name not in base}
    return pa.table({name: _column(array) for name, array in columns.items()})


# one point-cloud emitted-phase evidence both the content-keyed table and the frozen owner contribute, native scalars
# the receipts `Encoder(enc_hook=repr)` serializes without a `str()` coerce. `domain`/`kind`/`key` are the lifted
# evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads — the SAME pair handed `Metrics.record` beside
# the minted identity — and `kind` is the LAS point-format id because that closed eleven-member set is the bounded
# dimension a board joins on, where a CRS WKT or a content key would fork the series per file.
def _pointcloud_receipt(content_key: ContentKey, point_count: int, point_format: int, crs_wkt: str) -> Iterator[Receipt]:
    Metrics.record({"rasm.pointcloud.points": float(point_count)}, domain="pointcloud", kind=str(point_format))
    yield Receipt.of(
        "pointcloud",
        (
            "emitted",
            content_key.hex,
            {
                "domain": "pointcloud",
                "kind": str(point_format),
                "key": content_key.hex,
                "points": point_count,
                "format": point_format,
                "crs": crs_wkt,
            },
        ),
    )


class PointRecordTable(Struct, frozen=True):
    table: pa.Table
    point_count: int
    point_format: int
    crs_wkt: str
    content_key: ContentKey

    @classmethod
    def of(cls, record: Record, crs_wkt: str = "") -> "RuntimeRail[PointRecordTable]":
        coords = _coords(record)
        return ContentIdentity.of("pointcloud", coords.tobytes()).map(
            lambda key: cls(
                table=_to_arrow(record), point_count=len(coords), point_format=int(record.point_format.id), crs_wkt=crs_wkt, content_key=key
            )
        )

    def contribute(self) -> Iterator[Receipt]:
        return _pointcloud_receipt(self.content_key, self.point_count, self.point_format, self.crs_wkt)


class PointCloud(Struct, frozen=True):
    content_key: ContentKey
    point_count: int
    point_format: int
    crs_wkt: str

    @classmethod
    def read(cls, ref: ResourceRef, selection: Selection = None) -> "RuntimeRail[PointCloud]":
        def run() -> RuntimeRail[PointCloud]:
            data = _read(str(ref.path), selection)
            return ContentIdentity.of("pointcloud", _coords(data).tobytes()).map(
                lambda key: cls(
                    content_key=key,
                    point_count=int(data.header.point_count),
                    point_format=int(data.header.point_format.id),
                    crs_wkt=str(data.header.parse_crs() or ""),
                )
            )

        with _TRACER.start_as_current_span("pointcloud.read"):
            return boundary("pointcloud.read", run, catch=laspy.LaspyException).bind(lambda rail: rail)

    @staticmethod
    async def subset(ref: ResourceRef, query: CopcQuery, selection: Selection = None) -> "RuntimeRail[PointRecordTable]":
        path = str(ref.path)
        remote = path.startswith(("http://", "https://"))

        def run() -> RuntimeRail[PointRecordTable]:
            reader = _open_copc(path, selection)
            return PointRecordTable.of(query.query(reader), crs_wkt=str(reader.header.parse_crs() or ""))

        # the remote COPC leg is an outbound network read — kind=CLIENT per the store span-kind law; a local path stays INTERNAL.
        with _TRACER.start_as_current_span(
            "pointcloud.subset", kind=SpanKind.CLIENT if remote else SpanKind.INTERNAL, attributes={"rasm.pointcloud.remote": remote}
        ):
            railed_rail = (
                await guarded(RetryClass.HTTP, on_thread, run, abandon=True, subject="pointcloud.subset")
                if remote
                else await async_boundary("pointcloud.subset", lambda: on_thread(run), catch=laspy.LaspyException)
            )
            return railed_rail.bind(lambda rail: rail)

    @staticmethod
    def to_arrow(ref: ResourceRef, selection: Selection = None) -> "RuntimeRail[PointRecordTable]":
        def run() -> RuntimeRail[PointRecordTable]:
            data = _read(str(ref.path), selection)
            return PointRecordTable.of(data, crs_wkt=str(data.header.parse_crs() or ""))

        with _TRACER.start_as_current_span("pointcloud.to_arrow"):
            return boundary("pointcloud.to_arrow", run, catch=laspy.LaspyException).bind(lambda rail: rail)

    @staticmethod
    def write(source: ResourceRef, out: ResourceRef, mode: "WriteMode" = "preserve") -> "RuntimeRail[ContentKey]":
        def run() -> RuntimeRail[ContentKey]:
            data = laspy.read(str(source.path))
            _WRITE[mode](data, str(out.path))
            return ContentIdentity.of("pointcloud.write", out.path.read_bytes())

        with _TRACER.start_as_current_span("pointcloud.write", attributes={"rasm.pointcloud.compress": mode}):
            return boundary("pointcloud.write", run, catch=laspy.LaspyException).bind(lambda rail: rail)

    def contribute(self) -> Iterator[Receipt]:
        return _pointcloud_receipt(self.content_key, self.point_count, self.point_format, self.crs_wkt)


def _laz_backend() -> laspy.LazBackend:
    from laspy import LazBackend  # ruff:ignore[import-outside-top-level]

    backend = next((b for b in (LazBackend.LazrsParallel, LazBackend.Lazrs, LazBackend.Laszip) if b.is_available()), None)
    if backend is None:
        raise laspy.LaspyException("compressed LAZ/COPC requires lazrs or laszip on the worker lane")
    return backend


# `do_compress` tri-state is a closed `WriteMode` vocabulary, not a `bool | None` truthiness fork: `compress` transcodes LAS->LAZ over the
# band-resolved `LazBackend`, `store` forces an explicit uncompressed write, and `preserve` round-trips the source's own format — each one row,
# never an `if do_compress`/`elif do_compress is not None` ladder collapsing the `store` write into the `preserve` path.
_WRITE: Final[Map[WriteMode, Callable[["laspy.LasData", str], None]]] = Map.of_seq([
    ("compress", lambda data, dst: data.write(dst, do_compress=True, laz_backend=_laz_backend())),
    ("store", lambda data, dst: data.write(dst, do_compress=False)),
    ("preserve", lambda data, dst: data.write(dst)),
])


def _open_copc(path: str, selection: Selection) -> "laspy.copc.CopcReader":
    # remote legs take the catalogue `http_num_threads=80` default by omission (never a local
    # constant restating it); only the local leg forces `1` to serialize the single-file read.
    threads: dict[str, object] = {} if path.startswith(("http://", "https://")) else {"http_num_threads": 1}
    selected: dict[str, object] = {"decompression_selection": selection} if selection is not None else {}
    return laspy.copc.CopcReader.open(path, **threads, **selected)
```

## [04]-[PRODUCT]

- Owner: `ProductKind` — the produced-archive peer axis beside `MeshBackend`, each member carrying its own `_PRODUCT` row (suffix, media type, receipt subject) so the sink is a row per product and never a `write_bcf`/`write_costs`/`write_diff` family. This is the sink `geometry/ifc/analysis` and `geometry/ifc/costing` both defer their durable `.bcfzip`, cost-spreadsheet, and diff-export writes to: a producer hands the bytes it already minted, and this owner lands them content-keyed with the suffix its row declares.
- Cases: `BCF_ARCHIVE` is the BCF 3.0 zip a `BcfXml` document serializes, `COST_TABLE` the cost spreadsheet an IFC quantity fold renders, `DIFF_REPORT` the change export a model comparison emits. Each is a byte payload with a declared container, never a live document handle, because this tier holds no BCF, spreadsheet, or diff engine and a handle crossing here would drag one.
- Entry: one `archived` fold over the `(kind, payload, out)` triple — the suffix the row declares is PROVEN against the destination rather than appended, so a caller writing a BCF payload to a `.xlsx` path refuses by name instead of landing a mislabelled archive downstream readers sniff wrong.
- Receipt: one `ProductReceipt` keyed by `ContentIdentity` over the landed bytes, sharing the `mesh` partition its sibling receipts write so an archive and the model it describes prune on one predicate. It records NO measure: byte volume on the object plane is `tabular/egress#EGRESS`'s instrument, and a second series over the same bytes would double whatever an egress leg already metered.
- Growth: a new product is one `ProductKind` member with its `_PRODUCT` row; a new container for an existing product is one suffix change on that row; zero new surface and no per-product entrypoint.
- Boundary: byte landing and identity only — no BCF authoring, no spreadsheet rendering, no model diffing, and no IFC read of any kind. The producing geometry planes own every payload; a `ProductKind` row whose bytes this tier would have to author is the rejected shape.

```python signature
from collections.abc import Iterator
from enum import StrEnum
from typing import Final

from expression import Error
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

# `_TRACER` is the [02]-[MESH] owner in this same module.


# suffix + media + subject per product: the suffix is the PROVEN container, the media type the wire label a
# downstream reader admits on, and the subject the receipt phase-subject so one partition holds every product row.
class _Product(Struct, frozen=True, gc=False):
    suffix: str
    media: str
    subject: str


class ProductKind(StrEnum):
    BCF_ARCHIVE = "bcf_archive"
    COST_TABLE = "cost_table"
    DIFF_REPORT = "diff_report"

    @property
    def row(self) -> _Product:
        return _PRODUCT[self]


_PRODUCT: Final[Map[ProductKind, _Product]] = Map.of_seq([
    (ProductKind.BCF_ARCHIVE, _Product(".bcfzip", "application/octet-stream", "bcf")),
    (ProductKind.COST_TABLE, _Product(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cost")),
    (ProductKind.DIFF_REPORT, _Product(".json", "application/json", "diff")),
])


class ProductReceipt(Struct, frozen=True):
    kind: ProductKind
    path: str
    media: str
    byte_length: int
    content_key: ContentKey

    def contribute(self) -> Iterator[Receipt]:
        # the `mesh` partition holds every spatial product row, so an archive and the model it describes prune on one
        # predicate; no measure records here because the object plane's own egress leg meters the bytes it moves.
        yield Receipt.of(
            "product",
            (
                "emitted",
                self.kind.row.subject,
                {
                    "domain": "mesh",
                    "kind": self.kind.value,
                    "key": self.content_key.hex,
                    "media": self.media,
                    "bytes": self.byte_length,
                    "path": self.path,
                },
            ),
        )


def archived(kind: ProductKind, payload: bytes, out: ResourceRef) -> RuntimeRail[ProductReceipt]:
    # the container proof leads the write: a suffix mismatch is a labelling fault the producer owns, and landing the
    # bytes first would leave a mislabelled archive on disk for a reader to sniff wrong.
    row = kind.row
    if out.path.suffix.lower() != row.suffix:
        return Error(BoundaryFault(config=(f"product.{row.subject}", f"expected {row.suffix}, destination names {out.path.suffix.lower() or 'none'}")))

    def run() -> RuntimeRail[ProductReceipt]:
        out.path.write_bytes(payload)
        return ContentIdentity.of(f"product.{row.subject}", payload).map(
            lambda key: ProductReceipt(kind=kind, path=str(out.path), media=row.media, byte_length=len(payload), content_key=key)
        )

    with _TRACER.start_as_current_span(f"product.{row.subject}", attributes={"rasm.product.kind": kind.value}):
        return boundary(f"product.{row.subject}", run).bind(lambda rail: rail)
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

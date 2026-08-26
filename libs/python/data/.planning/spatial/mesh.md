# [PY_DATA_MESH]

Mesh-file exchange owner over a `MeshBackend` axis with the point-cloud interchange row and the produced-archive sink: `MeshExchange` admits one file once and folds mesh-file identity, cell-block topology, unit posture, named array arities, the FE time-series axis, and preview/format export onto one `MeshOp` union over one `_BACKEND` behavior table — `meshio` for FE volume/cell-block meshes, `trimesh` for surface meshes, `rhino3dm` for `.3dm` exchange — `CloudExchange` is the LAS/LAZ/COPC row over `laspy` alone, and `ProductKind` is the peer axis over the produced byte payloads a geometry analysis hands down for durable landing. This is file exchange and identity: the IFC-to-GLB tessellation path belongs to the geometry package, never re-derived here, the geometry `pdal` filter-graph stays geometry-owned, and the product sink lands bytes a producer already minted rather than authoring any of them.

Every payload keys by runtime `ContentIdentity` over the canonical `float64` point buffer, and named arrays egress as arity-banded Arrow tables. Source admission loads the provider engine exactly once and every op reads that one open, so no leg re-opens the file it was handed. Foreign facts a format may omit — a `.3dm` object name, a `Trimesh` unit hint, a LAS CRS VLR — cross as `Posture`, so declared, defaulted-from-a-named-source, and absent stay three states rather than one empty string. Network-bearing COPC reads route through `guarded(RetryClass.HTTP, on_thread, ...)`, the `THREAD_BAND`-bounded hop elected by the source row rather than by which entrypoint a caller picked — the same retry/span/lift triplet the sibling spatial pages delegate to the runtime resilience owner.

## [01]-[INDEX]

- [02]-[MESH]: the `MeshExchange` owner over the `_BACKEND` behavior table — one `MeshOp` entry across topology, named arrays, time-series, preview, and export.
- [03]-[POINTCLOUD]: the `CloudExchange` LAS/LAZ/COPC row — the closed `CloudSource` axis, octree subset, the columnar point-record bridge, the remote resilience envelope.
- [04]-[PRODUCT]: the `ProductKind` produced-archive sink — the BCF/cost/diff byte payloads the IFC analysis and costing planes defer here, landed content-keyed.

## [02]-[MESH]

- Owner: `MeshExchange` holds the admitted triple — elected backend, source ref, loaded engine — and each `_Backend` row pairs the loader, the hierarchy `walk`, the exporter, the unit posture, the optional frame-stream reader, the per-engine fault set the `boundary` narrows `catch=` to, and the extension set, so engine variation collapses to one row, never a parallel per-engine builder family or a per-engine `match` arm in `run`.
- Entry: one `run(op)` discriminates `MeshOp` — `read | arrays | timeseries | export(fmt) | preview` — onto one closed `MeshProduct`, both directions on one surface. Per-leg sibling entrypoints each re-run `row.load(ref)`, so a caller reading a payload and then its arrays pays two whole-file provider loads over one file, and a sibling `write` entry forks the span and backend election.
- Auto: `MeshBackend.of` resolves the case off the source extension through `_EXT.try_find` and REFUSES an unrecognized suffix by name. Defaulting `.get(suffix, "meshio")` elects the FE reader for every unknown extension, which then dies inside its codec naming the file rather than whichever routing decision mis-sent it.
- Walk: `_rhino3dm_walk` publishes one node per object carrying layer path, object name, object id, and the composed instance transform, expanding every `InstanceReference` through its definition under a cycle-pinned branch set. Flattening `model.Objects` into one comprehension concatenates a single buffer instead, dropping every block-instanced mesh whole and erasing all four facts from the meshes it keeps; `meshio` and `trimesh` publish one whole-file node because neither carries an object graph past the load.
- Units: each row answers `units` as a `Posture` — `rhino3dm` DEFAULTED from `ModelUnitSystem` because a document declaring no unit still reports `Millimeters`, `trimesh` DECLARED where `Trimesh.units` is non-null and ABSENT where it is not, `meshio` ABSENT because that package carries no unit surface at all. Both `mesh.units or "m"` and a meshio row spelling `lambda _: "m"` publish a measurement no producer took.
- Law: `MeshColumns` bands the columnar egress by ARITY — `point` over the vertex-aligned columns, `cell.<block>` over each block's own cell census — and proves each band's widths at construction, because `pa.table` admits one length across every column handed it and one table spanning both arities raises `ArrowInvalid` on every mesh carrying cell data. `MeshPayload` carries topology, array censuses, nodes, units, and the canonical point-buffer `ContentKey` directly.
- Growth: a new surface format is one extension string on the `trimesh` row; a new FE format already routes through `meshio.extension_to_filetypes`; a new engine is one `MeshBackend` case plus one `_Backend` row; a new leg is one `MeshOp` case, one `run` arm, and one `MeshProduct` case; a new named array kind is one more dict the row `walk` folds, surfacing as one more egress column and arity band with no frame edit; zero per-format `read_*`/`write_*` family; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.MESH` in this module's one `RAISES` table, which every section anchors on.
- Boundary: no geometry kernel, no bridge lifecycle, no NURBS/Brep/SubD construction — the `rhino3dm` row reads `File3dm` meshes and instance references only, the offline 3dm reader per the geometry-flow law. Frame streaming is XDMF's alone, so a surface row carrying no frame reader refuses the leg by name instead of dying inside a codec it never opened.

```python
from collections.abc import Callable, Iterator, Mapping
from itertools import accumulate
from typing import Final, Literal, assert_never
from uuid import UUID

import meshio
import numpy as np
import pyarrow as pa
import rhino3dm
import trimesh
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Disposition, FaultRow, Posture, RuntimeResult, async_boundary, rostered, scoped, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.roots import ResourceRef

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.mesh")

POINTCLOUD_CLOUD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="cloud", arm="boundary", defect="cloud-read", retriability=TRANSIENT
)
MESH_BACKEND: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="backend", arm="config", defect="suffix-unrouted", retriability=TERMINAL, slots=("suffix",)
)
MESH_ARITY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="arrays", arm="config", defect="band-width-disagreement", retriability=TERMINAL, slots=("band", "widths")
)
MESH_OPEN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="open", arm="boundary", defect="engine-open", retriability=TRANSIENT
)
MESH_WORK: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="work", arm="boundary", defect="mesh-leg", retriability=TRANSIENT
)
MESH_FRAMES: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="timeseries", arm="boundary", defect="frames-read", retriability=TRANSIENT
)
MESH_UNSTREAMED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="timeseries.reader", arm="config", defect="no-frame-reader", retriability=TERMINAL, slots=("backend",)
)
CLOUD_SOURCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="cloud.source", arm="config", defect="remote-unaddressable", retriability=TERMINAL, slots=("url",)
)
PRODUCT_SUFFIX: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="product.suffix", arm="config", defect="suffix-mismatch", retriability=TERMINAL,
    slots=("product", "expected", "named"),
)
PRODUCT_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="product.write", arm="boundary", defect="product-write", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    POINTCLOUD_CLOUD,
    MESH_BACKEND,
    MESH_ARITY,
    MESH_OPEN,
    MESH_WORK,
    MESH_FRAMES,
    MESH_UNSTREAMED,
    CLOUD_SOURCE,
    PRODUCT_SUFFIX,
    PRODUCT_WRITE,
]))
_PREVIEW: Final[str] = "glb"
_UNSET: Final[UUID] = UUID(int=0)
_POINT_BAND: Final[str] = "point"

type Engine = meshio.Mesh | trimesh.Trimesh | rhino3dm.File3dm
type Arrays = Mapping[str, np.ndarray]
type Blocks = Mapping[str, Arrays]
type Frames = Iterator[RuntimeResult[tuple[float, "MeshColumns"]]]


class _Node(Struct, frozen=True):
    identity: Posture[str]
    name: Posture[str]
    path: Posture[str]
    transform_applied: bool
    vertex_span: tuple[int, int]


class _Piece(Struct, frozen=True):
    identity: Posture[str]
    name: Posture[str]
    path: Posture[str]
    transform_applied: bool
    points: np.ndarray
    cell_blocks: tuple[str, ...]
    point_data: Arrays
    cell_data: Blocks
    field_data: Arrays


class _Extract(Struct, frozen=True):
    points: np.ndarray
    cell_blocks: tuple[str, ...]
    point_data: Arrays
    cell_data: Blocks
    field_data: Arrays
    nodes: tuple[_Node, ...]


class MeshFrame(Struct, frozen=True):
    points: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    nodes: tuple[_Node, ...]


def _stacked(blocks: tuple[np.ndarray, ...]) -> np.ndarray:
    return blocks[0] if len(blocks) == 1 else np.concatenate(blocks)


def _folded(pieces: Block[_Piece]) -> _Extract:
    stacks = tuple(piece.points for piece in pieces)
    starts = tuple(accumulate((len(block) for block in stacks), initial=0))
    aligned = sorted({
        name
        for piece in pieces
        for name in piece.point_data
        if all(name in other.point_data and len(other.point_data[name]) == len(other.points) for other in pieces)
    })
    return _Extract(
        points=_stacked(stacks) if stacks else np.empty((0, 3), dtype="float64"),
        cell_blocks=tuple(sorted({block for piece in pieces for block in piece.cell_blocks})),
        point_data={name: _stacked(tuple(piece.point_data[name] for piece in pieces)) for name in aligned},
        cell_data={
            block: {name: array for piece in pieces if block in piece.cell_data for name, array in piece.cell_data[block].items()}
            for block in sorted({block for piece in pieces for block in piece.cell_data})
        },
        field_data={name: array for piece in pieces for name, array in piece.field_data.items()},
        nodes=tuple(
            _Node(piece.identity, piece.name, piece.path, piece.transform_applied, (start, start + len(piece.points)))
            for piece, start in zip(pieces, starts, strict=False)
        ),
    )


def _frame(extract: _Extract) -> RuntimeResult[MeshFrame]:
    canonical = np.ascontiguousarray(extract.points, dtype="float64")
    return ContentIdentity.of("mesh", canonical.tobytes()).map(
        lambda points: MeshFrame(
            points,
            len(canonical),
            extract.cell_blocks,
            len(extract.point_data),
            sum(len(arrays) for arrays in extract.cell_data.values()),
            len(extract.field_data),
            extract.nodes,
        )
    )


def _blocked(per_name: Mapping[str, Mapping[str, np.ndarray]]) -> Blocks:
    blocks = {cell_type for per_type in per_name.values() for cell_type in per_type}
    return {
        cell_type: {name: np.asarray(per_type[cell_type]) for name, per_type in per_name.items() if cell_type in per_type}
        for cell_type in sorted(blocks)
    }


def _meshio_walk(mesh: "meshio.Mesh") -> Block[_Piece]:
    return Block.singleton(
        _Piece(
            identity=Posture(absent=None),
            name=Posture(absent=None),
            path=Posture(absent=None),
            transform_applied=False,
            points=np.asarray(mesh.points),
            cell_blocks=tuple(mesh.cells_dict.keys()),
            point_data={name: np.asarray(array) for name, array in mesh.point_data.items()},
            cell_data=_blocked(mesh.cell_data_dict),
            field_data={name: np.asarray(array) for name, array in mesh.field_data.items()},
        )
    )


def _trimesh_walk(surface: "trimesh.Trimesh") -> Block[_Piece]:
    visual = surface.visual
    faces = np.asarray(surface.faces)
    return Block.singleton(
        _Piece(
            identity=Posture(absent=None),
            name=Posture(absent=None),
            path=Posture(absent=None),
            transform_applied=True,
            points=np.asarray(surface.vertices),
            cell_blocks=("triangle",) if len(faces) else (),
            point_data={"color": np.asarray(visual.vertex_colors)} if isinstance(visual, trimesh.visual.ColorVisuals) else {},
            cell_data={"triangle": {"vertex_index": faces}} if len(faces) else {},
            field_data={},
        )
    )


def _rhino3dm_walk(model: "rhino3dm.File3dm") -> Block[_Piece]:
    layers = Map.of_seq((layer.Index, layer.FullPath) for layer in model.Layers)
    roots = Block.of_seq(obj for obj in model.Objects if not obj.Attributes.IsInstanceDefinitionObject)
    return roots.collect(lambda obj: _placed(model, layers, obj, Nothing, frozenset()))


def _placed(
    model: "rhino3dm.File3dm",
    layers: Map[int, str],
    obj: "rhino3dm.File3dmObject",
    xform: Option["rhino3dm.Transform"],
    branch: frozenset[str],
) -> Block[_Piece]:
    attrs = obj.Attributes
    node = (_identified(attrs), _named(attrs), _pathed(layers, attrs))
    match obj.Geometry:
        case rhino3dm.Mesh() as mesh:
            return Block.singleton(_meshed(node, mesh, xform))
        case rhino3dm.InstanceReference() as reference if str(reference.ParentIdefId) not in branch:
            composed = Some(xform.map(lambda held: rhino3dm.Transform.Multiply(held, reference.Xform)).default_value(reference.Xform))
            definition = model.InstanceDefinitions.FindId(reference.ParentIdefId)
            members = Block.of_seq(definition.GetObjectIds()).choose(lambda oid: Option.of_optional(model.Objects.FindId(oid)))
            return members.collect(lambda member: _placed(model, layers, member, composed, branch | {str(reference.ParentIdefId)}))
        case _:
            return Block.empty()


def _meshed(node: tuple[Posture[str], Posture[str], Posture[str]], mesh: "rhino3dm.Mesh", xform: Option["rhino3dm.Transform"]) -> _Piece:
    placed = xform.map(lambda held: _transformed(mesh, held)).default_value(mesh)
    identity, name, path = node
    bands = Block.of_seq((
        ("normal", _band(placed, lambda held: [(n.X, n.Y, n.Z) for n in held.Normals], "float32")),
        ("color", _band(placed, lambda held: [tuple(c) for c in held.VertexColors], "uint8")),
    ))
    return _Piece(
        identity=identity,
        name=name,
        path=path,
        transform_applied=xform.is_some(),
        points=np.array([(v.X, v.Y, v.Z) for v in placed.Vertices], dtype="float64").reshape(-1, 3),
        cell_blocks=tuple(sorted({"quad" if face[3] != face[2] else "triangle" for face in placed.Faces})),
        point_data=dict(bands.choose(lambda pair: pair[1].map(lambda array: (pair[0], array)))),
        cell_data={},
        field_data={},
    )


def _transformed(mesh: "rhino3dm.Mesh", xform: "rhino3dm.Transform") -> "rhino3dm.Mesh":
    placed = mesh.Duplicate()
    placed.Transform(xform)
    return placed


def _band(mesh: "rhino3dm.Mesh", read: "Callable[[rhino3dm.Mesh], list[tuple[float, ...]]]", dtype: str) -> Option[np.ndarray]:
    rows = read(mesh)
    return Some(np.array(rows, dtype=dtype)) if len(rows) == len(mesh.Vertices) else Nothing


def _identified(attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    return Posture.of_optional(None if attrs.Id == _UNSET else str(attrs.Id))


def _named(attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    return Posture.of_optional(attrs.Name or None)


def _pathed(layers: Map[int, str], attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    return Posture.of_option(layers.try_find(attrs.LayerIndex))


def _xdmf_frames(ref: ResourceRef) -> Frames:
    reader = meshio.xdmf.TimeSeriesReader(str(ref.path))
    reader.read_points_cells()
    return _framed(reader)


def _framed(reader: "meshio.xdmf.TimeSeriesReader") -> Frames:
    with reader:
        for step in range(reader.num_steps):
            time, point_data, cell_data = reader.read_data(step)
            points = {name: np.asarray(array) for name, array in point_data.items()}
            yield MeshColumns.of(points, _blocked(cell_data), Nothing).map(lambda held: (float(time), held))


class _Backend(Struct, frozen=True):
    load: Callable[[ResourceRef], Engine]
    walk: Callable[[Engine], Block[_Piece]]
    export: Callable[[Engine, ResourceRef, str], None]
    units: Callable[[Engine], Posture[str]]
    frames: Option[Callable[[ResourceRef], Frames]]
    fault: type[Exception] | tuple[type[Exception], ...]
    exts: frozenset[str]

    def extract(self, engine: Engine) -> _Extract:
        return _folded(self.walk(engine))

    def frame(self, engine: Engine) -> RuntimeResult[MeshFrame]:
        return _frame(self.extract(engine))


def _meshio_load(ref: ResourceRef) -> "meshio.Mesh":
    return meshio.read(str(ref.path))


def _trimesh_load(ref: ResourceRef) -> "trimesh.Trimesh":
    return trimesh.load_mesh(str(ref.path))


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
    (
        "rhino3dm",
        _Backend(
            _rhino3dm_load,
            _rhino3dm_walk,
            _rhino3dm_export,
            lambda model: Posture(defaulted=(model.Settings.ModelUnitSystem.name.lower(), "ModelUnitSystem")),
            Nothing,
            (FileNotFoundError, OSError),
            frozenset({".3dm"}),
        ),
    ),
    (
        "trimesh",
        _Backend(
            _trimesh_load,
            _trimesh_walk,
            lambda mesh, out, fmt: mesh.export(str(out.path), file_type=fmt),
            lambda mesh: Posture.of_optional(mesh.units),
            Nothing,
            (NotImplementedError, ValueError, OSError),
            frozenset({".stl", ".obj", ".ply", ".glb", ".gltf", ".off", ".3mf"}),
        ),
    ),
    (
        "meshio",
        _Backend(
            _meshio_load,
            _meshio_walk,
            lambda mesh, out, fmt: meshio.write(str(out.path), mesh, file_format=fmt),
            lambda _: Posture(absent=None),
            Some(_xdmf_frames),
            (meshio.ReadError, meshio.WriteError),
            frozenset(meshio.extension_to_filetypes.keys()),
        ),
    ),
])

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
    def of(ref: ResourceRef) -> "RuntimeResult[MeshBackend]":
        suffix = ref.path.suffix.lower()
        return (
            _EXT.try_find(suffix)
            .map(lambda name: MeshBackend(**{name: suffix}))
            .to_result_with(lambda: MESH_BACKEND.raised(suffix))
        )

    @property
    def row(self) -> _Backend:
        return _BACKEND[self.tag]


def _column(array: np.ndarray) -> pa.Array:
    flat = np.ascontiguousarray(array)
    return pa.array(flat) if flat.ndim == 1 else pa.FixedSizeListArray.from_arrays(pa.array(flat.reshape(-1)), flat.shape[1])


def _columns(arrays: Arrays) -> pa.Table:
    return pa.table({name: _column(array) for name, array in arrays.items()})


class MeshColumns(Struct, frozen=True):
    bands: Map[str, pa.Table]

    @staticmethod
    def of(points: Arrays, cells: Blocks, rows: Option[int]) -> "RuntimeResult[MeshColumns]":
        def admit(band: str, arrays: Arrays, expected: Option[int]) -> "RuntimeResult[tuple[str, pa.Table]]":
            census = frozenset(len(array) for array in arrays.values()) | frozenset(expected.to_list())
            return (
                Error(MESH_ARITY.raised(band, ",".join(str(width) for width in sorted(census))))
                if len(census) > 1
                else Ok((band, _columns(arrays)))
            )

        celled = Block.of_seq(cells.items()).map(lambda row: admit(f"cell.{row[0]}", row[1], Nothing))
        banded = Block.singleton(admit(_POINT_BAND, points, rows)).append(celled)
        return traversed(banded, by=Disposition.ACCUMULATE).map(lambda pairs: MeshColumns(Map.of_seq(pairs)))


@tagged_union(frozen=True)
class MeshOp:
    tag: Literal["read", "arrays", "timeseries", "export", "preview"] = tag()
    read: None = case()
    arrays: None = case()
    timeseries: None = case()
    export: tuple[ResourceRef, str] = case()
    preview: ResourceRef = case()


@tagged_union(frozen=True)
class MeshProduct:
    tag: Literal["payload", "columns", "frames", "written"] = tag()
    payload: "MeshPayload" = case()
    columns: MeshColumns = case()
    frames: Frames = case()
    written: ContentKey = case()


def _format(op: MeshOp) -> Option[str]:
    match op:
        case MeshOp(tag="export", export=(_, fmt)):
            return Some(fmt)
        case MeshOp(tag="preview"):
            return Some(_PREVIEW)
        case _:
            return Nothing


class MeshPayload(Struct, frozen=True):
    backend: MeshBackend
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    nodes: tuple[_Node, ...]
    units: Posture[str]

    @classmethod
    def of(cls, backend: MeshBackend, frame: MeshFrame, units: Posture[str]) -> "MeshPayload":
        return cls(
            backend,
            frame.points,
            frame.point_count,
            frame.cell_blocks,
            frame.point_arrays,
            frame.cell_arrays,
            frame.field_arrays,
            frame.nodes,
            units,
        )

class MeshExchange(Struct, frozen=True):
    backend: MeshBackend
    ref: ResourceRef
    engine: Engine

    @staticmethod
    async def of(ref: ResourceRef) -> "RuntimeResult[MeshExchange]":
        match MeshBackend.of(ref):
            case Result(tag="ok", ok=backend):
                row = backend.row
                with _TRACER.start_as_current_span("mesh.open", attributes={"rasm.mesh.backend": backend.tag}):
                    opened = await async_boundary(MESH_OPEN, lambda: on_thread(lambda: row.load(ref)), catch=row.fault)
                    return opened.map(lambda engine: MeshExchange(backend, ref, engine))
            case Result(tag="error") as refused:
                return refused

    async def run(self, op: MeshOp) -> "RuntimeResult[MeshProduct]":
        subject = f"mesh.{op.tag}"
        marks = {"rasm.mesh.backend": self.backend.tag} | _format(op).map(lambda fmt: {"rasm.mesh.format": fmt}).default_value({})
        with _TRACER.start_as_current_span(subject, attributes=marks):
            match op:
                case MeshOp(tag="read"):
                    return await self._banded(self._payload)
                case MeshOp(tag="arrays"):
                    return await self._banded(self._columned)
                case MeshOp(tag="timeseries"):
                    return await self._streamed()
                case MeshOp(tag="export", export=(out, fmt)):
                    return await self._banded(lambda: self._written(out, fmt))
                case MeshOp(tag="preview", preview=out):
                    return await self._banded(lambda: self._written(out, _PREVIEW))
                case _ as unreachable:
                    assert_never(unreachable)

    async def _banded(self, work: Callable[[], RuntimeResult[MeshProduct]]) -> "RuntimeResult[MeshProduct]":
        return (await async_boundary(MESH_WORK, lambda: on_thread(work), catch=self.backend.row.fault)).bind(lambda held: held)

    async def _streamed(self) -> "RuntimeResult[MeshProduct]":
        row = self.backend.row
        match row.frames:
            case Option(tag="some", some=open_frames):
                opened = await async_boundary(MESH_FRAMES, lambda: on_thread(lambda: open_frames(self.ref)), catch=row.fault)
                return opened.map(lambda frames: MeshProduct(frames=frames))
            case _:
                return Error(MESH_UNSTREAMED.raised(self.backend.tag))

    def _payload(self) -> RuntimeResult[MeshProduct]:
        row = self.backend.row
        return row.frame(self.engine).map(lambda frame: MeshProduct(payload=MeshPayload.of(self.backend, frame, row.units(self.engine))))

    def _columned(self) -> RuntimeResult[MeshProduct]:
        extract = self.backend.row.extract(self.engine)
        return _frame(extract).bind(
            lambda frame: MeshColumns.of(extract.point_data, extract.cell_data, Some(frame.point_count)).map(
                lambda columns: MeshProduct(columns=columns)
            )
        )

    def _written(self, out: ResourceRef, fmt: str) -> RuntimeResult[MeshProduct]:
        self.backend.row.export(self.engine, out, fmt)
        return ContentIdentity.of("mesh.export", out.path.read_bytes()).map(lambda key: MeshProduct(written=key))
```

## [03]-[POINTCLOUD]

- Owner: `CloudExchange` — the LAS/LAZ/COPC row over `laspy` alone. One `Selection` threads the `decompression_selection` mask identically through `laspy.read` and `CopcReader.open`, fixed once at admission, so a cloud subset skips fields it never reads — one optional carrier, never a parallel selective-read method. `PointCloud` and `PointRecordTable` carry the point-format id and CRS posture directly.
- Entry: one `run(op)` discriminates `CloudOp` — `read | subset | to_arrow | write` — onto one closed `CloudProduct`. Per-leg sibling statics each re-derive the source discriminant and the CRS projection, and the whole-file ones run the `laspy` decode ON the event loop while only the subset leg hops the band; one entry puts every leg behind the same `THREAD_BAND`-bounded hop.
- Source: `CloudSource` is the closed remote/local/handle axis admitted ONCE at the head, and its `_SOURCE` row elects the span kind, the retry class, and the `http_num_threads` value together. Election reads the ref's own declared `scheme` column, never a `path.startswith(("http://", "https://"))` test — that test spells one discriminant per call site and classifies nothing at all for the file OBJECT `CopcReader.open` also admits.
- Reach: `laspy.read` takes a path or a stream and no url, so the whole-file legs refuse a remote source BY NAME while the subset leg admits all three cases — each projection cites the surface its provider declares rather than sharing one address every leg pretends to serve.
- CRS: `LasHeader.parse_crs` answers `None` for a file carrying no CRS VLR AND for a file whose VLR it cannot understand, so `str(... or "")` fuses both with a subset that carried none forward. Absence rides `Posture` from the single read site rather than publishing an empty CRS a consumer reprojects against.
- Boundary: no geometry kernel registration, no scan-to-BIM compute, no `pdal` filter-graph, no host coupling — host-free file exchange feeding the geometry companion at the wire; the point records cross as one content-keyed `PointRecordTable` through the shared `_column` builder, never a `laspy`- or `pdal`-specific object and never a re-spelled column fold.

```python
from collections.abc import Callable
from typing import BinaryIO, Final, Literal, assert_never

import laspy
import laspy.copc
import numpy as np
import pyarrow as pa
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry.trace import SpanKind
from upath import UPath

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import Posture, RuntimeResult, async_boundary
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef, origin


type Record = laspy.LasData | laspy.ScaleAwarePointRecord
type Selection = Option[laspy.DecompressionSelection]
type WriteMode = Literal["compress", "store", "preserve"]

_REMOTE: Final[frozenset[str]] = frozenset({"http", "https"})


class PointBounds(Struct, frozen=True):
    minx: float
    miny: float
    minz: float = -np.inf
    maxx: float = np.inf
    maxy: float = np.inf
    maxz: float = np.inf

    def as_copc(self) -> "laspy.copc.Bounds":
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


class _SourceRow(Struct, frozen=True, gc=False):
    retry: Option[RetryClass]
    span_kind: SpanKind
    threads: Option[int]


_SOURCE: Final[Map[str, _SourceRow]] = Map.of_seq([
    ("local", _SourceRow(Nothing, SpanKind.INTERNAL, Some(1))),
    ("remote", _SourceRow(Some(RetryClass.HTTP), SpanKind.CLIENT, Nothing)),
    ("handle", _SourceRow(Nothing, SpanKind.INTERNAL, Some(1))),
])


@tagged_union(frozen=True)
class CloudSource:
    tag: Literal["local", "remote", "handle"] = tag()
    local: UPath = case()
    remote: str = case()
    handle: BinaryIO = case()

    @staticmethod
    def of(ref: ResourceRef) -> "CloudSource":
        return CloudSource(remote=str(ref.path)) if ref.scheme in _REMOTE else CloudSource(local=ref.path)

    @property
    def row(self) -> _SourceRow:
        return _SOURCE[self.tag]

    @property
    def peer(self) -> Option[str]:
        return Some(origin(self.remote)) if self.tag == "remote" else Nothing

    @property
    def addressed(self) -> "str | BinaryIO":
        match self:
            case CloudSource(tag="local", local=path):
                return str(path)
            case CloudSource(tag="remote", remote=url):
                return url
            case CloudSource(tag="handle", handle=stream):
                return stream
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def whole(self) -> "RuntimeResult[str | BinaryIO]":
        match self:
            case CloudSource(tag="remote", remote=url):
                return Error(CLOUD_SOURCE.raised(url))
            case _:
                return Ok(self.addressed)


def _masked(selection: Selection) -> dict[str, object]:
    return selection.map(lambda mask: {"decompression_selection": mask}).default_value({})


def _coords(record: Record) -> np.ndarray:
    return np.ascontiguousarray(record.xyz, dtype="float64")


def _crs(header: "laspy.LasHeader") -> Posture[str]:
    return Posture.of_option(Option.of_optional(header.parse_crs()).map(str))


def _to_arrow(record: Record) -> pa.Table:
    coords = _coords(record)
    base = {"x": coords[:, 0], "y": coords[:, 1], "z": coords[:, 2]}
    columns = base | {name: np.asarray(record[name]) for name in record.point_format.dimension_names if name not in base}
    return pa.table({name: _column(array) for name, array in columns.items()})


class PointRecordTable(Struct, frozen=True):
    table: pa.Table
    point_count: int
    point_format: int
    crs: Posture[str]
    content_key: ContentKey

    @classmethod
    def of(cls, record: Record, crs: Posture[str]) -> "RuntimeResult[PointRecordTable]":
        coords = _coords(record)
        return ContentIdentity.of("pointcloud", coords.tobytes()).map(
            lambda key: cls(table=_to_arrow(record), point_count=len(coords), point_format=int(record.point_format.id), crs=crs, content_key=key)
        )

class PointCloud(Struct, frozen=True):
    content_key: ContentKey
    point_count: int
    point_format: int
    crs: Posture[str]

@tagged_union(frozen=True)
class CloudOp:
    tag: Literal["read", "subset", "to_arrow", "write"] = tag()
    read: None = case()
    subset: CopcQuery = case()
    to_arrow: None = case()
    write: tuple[ResourceRef, WriteMode] = case()


@tagged_union(frozen=True)
class CloudProduct:
    tag: Literal["cloud", "records", "written"] = tag()
    cloud: PointCloud = case()
    records: PointRecordTable = case()
    written: ContentKey = case()


def _laz_backend() -> laspy.LazBackend:
    backends = (laspy.LazBackend.LazrsParallel, laspy.LazBackend.Lazrs, laspy.LazBackend.Laszip)
    backend = next((b for b in backends if b.is_available()), None)
    if backend is None:
        raise laspy.LaspyException("compressed LAZ/COPC requires lazrs or laszip on the worker lane")
    return backend


_WRITE: Final[Map[WriteMode, Callable[["laspy.LasData", str], None]]] = Map.of_seq([
    ("compress", lambda data, dst: data.write(dst, do_compress=True, laz_backend=_laz_backend())),
    ("store", lambda data, dst: data.write(dst, do_compress=False)),
    ("preserve", lambda data, dst: data.write(dst)),
])


def _open_copc(source: CloudSource, selection: Selection) -> "laspy.copc.CopcReader":
    threads = source.row.threads.map(lambda count: {"http_num_threads": count}).default_value({})
    return laspy.copc.CopcReader.open(source.addressed, **threads, **_masked(selection))


class CloudExchange(Struct, frozen=True):
    source: CloudSource
    selection: Selection = Nothing

    async def run(self, op: CloudOp) -> "RuntimeResult[CloudProduct]":
        subject = f"pointcloud.{op.tag}"
        row = self.source.row
        with _TRACER.start_as_current_span(subject, kind=row.span_kind, attributes={"rasm.pointcloud.source": self.source.tag}):
            return (await self._enveloped(row, self._work(op))).bind(lambda held: held)

    def _work(self, op: CloudOp) -> Callable[[], RuntimeResult[CloudProduct]]:
        match op:
            case CloudOp(tag="read"):
                return self._cloud
            case CloudOp(tag="subset", subset=query):
                return lambda: self._records(query)
            case CloudOp(tag="to_arrow"):
                return self._table
            case CloudOp(tag="write", write=(out, mode)):
                return lambda: self._written(out, mode)
            case _ as unreachable:
                assert_never(unreachable)

    async def _enveloped(self, row: _SourceRow, work: Callable[[], RuntimeResult[CloudProduct]]) -> "RuntimeResult[RuntimeResult[CloudProduct]]":
        match row.retry:
            case Option(tag="some", some=cls):
                return await guarded(cls, on_thread, work, abandon=True, at=POINTCLOUD_CLOUD, on=self.source.peer)
            case _:
                return await async_boundary(POINTCLOUD_CLOUD, lambda: on_thread(work), catch=laspy.LaspyException)

    def _cloud(self) -> RuntimeResult[CloudProduct]:
        return self.source.whole.bind(lambda address: _headed(laspy.read(address, **_masked(self.selection))))

    def _table(self) -> RuntimeResult[CloudProduct]:
        return self.source.whole.bind(lambda address: _recorded(laspy.read(address, **_masked(self.selection))))

    def _records(self, query: CopcQuery) -> RuntimeResult[CloudProduct]:
        reader = _open_copc(self.source, self.selection)
        return PointRecordTable.of(query.query(reader), _crs(reader.header)).map(lambda records: CloudProduct(records=records))

    def _written(self, out: ResourceRef, mode: WriteMode) -> RuntimeResult[CloudProduct]:
        return self.source.whole.bind(lambda address: _landed(laspy.read(address), out, mode))


def _headed(data: "laspy.LasData") -> RuntimeResult[CloudProduct]:
    return ContentIdentity.of("pointcloud", _coords(data).tobytes()).map(
        lambda key: CloudProduct(
            cloud=PointCloud(
                content_key=key,
                point_count=int(data.header.point_count),
                point_format=int(data.header.point_format.id),
                crs=_crs(data.header),
            )
        )
    )


def _recorded(data: "laspy.LasData") -> RuntimeResult[CloudProduct]:
    return PointRecordTable.of(data, _crs(data.header)).map(lambda records: CloudProduct(records=records))


def _landed(data: "laspy.LasData", out: ResourceRef, mode: WriteMode) -> RuntimeResult[CloudProduct]:
    _WRITE[mode](data, str(out.path))
    return ContentIdentity.of("pointcloud.write", out.path.read_bytes()).map(lambda key: CloudProduct(written=key))
```

## [04]-[PRODUCT]

- Owner: `ProductKind` — the produced-archive peer axis beside `MeshBackend`, each member carrying its own `_PRODUCT` row (suffix and identity subject) so the sink is a row per product and never a `write_bcf`/`write_costs`/`write_diff` family. This is the sink `geometry/ifc/analysis` and `geometry/ifc/costing` both defer their durable `.bcfzip`, cost-spreadsheet, and diff-export writes to: a producer hands the bytes it already minted, and this owner lands them content-keyed with the suffix its row declares.
- Cases: `BCF_ARCHIVE` is the BCF 3.0 zip a `BcfXml` document serializes, `COST_TABLE` the cost spreadsheet an IFC quantity fold renders, `DIFF_REPORT` the change export a model comparison emits. Each is a byte payload with a declared container, never a live document handle, because this tier holds no BCF, spreadsheet, or diff engine and a handle crossing here would drag one.
- Entry: one `archived` fold over the `(kind, payload, out)` triple — the suffix the row declares is PROVEN against the destination rather than appended, so a caller writing a BCF payload to a `.xlsx` path refuses by name instead of landing a mislabelled archive downstream readers sniff wrong.
- Output: `archived` returns the `ContentKey` minted over the landed bytes.
- Growth: a new product is one `ProductKind` member with its `_PRODUCT` row; a new container for an existing product is one suffix change on that row; zero new surface and no per-product entrypoint.
- Boundary: byte landing and identity only — no BCF authoring, no spreadsheet rendering, no model diffing, and no IFC read of any kind. The producing geometry planes own every payload; a `ProductKind` row whose bytes this tier would have to author is the rejected shape.

```python
from enum import StrEnum
from typing import Final

from expression import Error
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeResult, boundary
from rasm.runtime.roots import ResourceRef


class _Product(Struct, frozen=True, gc=False):
    suffix: str
    subject: str


class ProductKind(StrEnum):
    BCF_ARCHIVE = "bcf_archive"
    COST_TABLE = "cost_table"
    DIFF_REPORT = "diff_report"

    @property
    def row(self) -> _Product:
        return _PRODUCT[self]


_PRODUCT: Final[Map[ProductKind, _Product]] = Map.of_seq([
    (ProductKind.BCF_ARCHIVE, _Product(".bcfzip", "bcf")),
    (ProductKind.COST_TABLE, _Product(".xlsx", "cost")),
    (ProductKind.DIFF_REPORT, _Product(".json", "diff")),
])


def archived(kind: ProductKind, payload: bytes, out: ResourceRef) -> RuntimeResult[ContentKey]:
    row = kind.row
    if out.path.suffix.lower() != row.suffix:
        named = out.path.suffix.lower() or "none"
        return Error(PRODUCT_SUFFIX.raised(row.subject, row.suffix, named))

    def run() -> RuntimeResult[ContentKey]:
        out.path.write_bytes(payload)
        return ContentIdentity.of(f"product.{row.subject}", payload)

    with _TRACER.start_as_current_span(f"product.{row.subject}", attributes={"rasm.product.kind": kind.value}):
        return boundary(PRODUCT_WRITE, run, catch=OSError).bind(lambda held: held)
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

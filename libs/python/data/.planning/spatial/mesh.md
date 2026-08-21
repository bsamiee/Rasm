# [PY_DATA_MESH]

Mesh-file exchange owner over a `MeshBackend` axis with the point-cloud interchange row and the produced-archive sink: `MeshExchange` admits one file once and folds mesh-file identity, cell-block topology, unit posture, named array arities, the FE time-series rail, and preview/format export onto one `MeshOp` union over one `_BACKEND` behavior table — `meshio` for FE volume/cell-block meshes, `trimesh` for surface meshes, `rhino3dm` for `.3dm` exchange — `CloudExchange` is the LAS/LAZ/COPC row over `laspy` alone, and `ProductKind` is the peer axis over the produced byte payloads a geometry analysis hands down for durable landing. This is file exchange and identity: the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here, the geometry `pdal` filter-graph stays geometry-owned, and the product sink lands bytes a producer already minted rather than authoring any of them.

Every payload keys by runtime `ContentIdentity` over the canonical `float64` point buffer, and the named-array egress rides the shared `tabular/columnar#SCAN` `QueryReceipt.railed` Arrow rail — the same `(table, QueryReceipt)` pair every sibling Arrow producer returns. Source admission loads the provider engine exactly once and every op reads that one open, so no leg re-opens the file it was handed. Foreign facts a format may omit — a `.3dm` object name, a `Trimesh` unit hint, a LAS CRS VLR — cross as `Posture`, so declared, defaulted-from-a-named-source, and absent stay three states rather than one empty string. Network-bearing COPC reads route through `guarded(RetryClass.HTTP, on_thread, ...)`, the `THREAD_BAND`-bounded hop elected by the source row rather than by which entrypoint a caller picked — the same retry/span/lift triplet the sibling spatial pages delegate to the runtime resilience owner.

## [01]-[INDEX]

- [02]-[MESH]: the `MeshExchange` owner over the `_BACKEND` behavior table — one `MeshOp` entry across topology, named arrays, time-series, preview, and export.
- [03]-[POINTCLOUD]: the `CloudExchange` LAS/LAZ/COPC row — the closed `CloudSource` axis, octree subset, the columnar point-record bridge, the remote resilience envelope.
- [04]-[PRODUCT]: the `ProductKind` produced-archive sink — the BCF/cost/diff byte payloads the IFC analysis and costing planes defer here, landed content-keyed.

## [02]-[MESH]

- Owner: `MeshExchange` holds the admitted triple — elected backend, source ref, loaded engine — and each `_Backend` row pairs the loader, the hierarchy `walk`, the exporter, the unit posture, the optional frame-stream reader, the per-engine fault set the `boundary` narrows `catch=` to, and the extension set, so engine variation collapses to one row, never a parallel per-engine builder family or a per-engine `match` arm in `run`.
- Entry: one `run(op)` discriminates `MeshOp` — `read | arrays | timeseries | export(fmt) | preview` — onto one closed `MeshProduct`, both directions on one surface. Per-leg sibling entrypoints each re-run `row.load(ref)`, so a caller reading a payload and then its arrays pays two whole-file provider loads over one file, and a sibling `write` entry forks the span, the receipt, and the backend election three ways.
- Auto: `MeshBackend.of` resolves the case off the source extension through `_EXT.try_find` and REFUSES an unrecognized suffix by name. Defaulting `.get(suffix, "meshio")` elects the FE reader for every unknown extension, which then dies inside its codec naming the file rather than whichever routing decision mis-sent it.
- Walk: `_rhino3dm_walk` publishes one node per object carrying layer path, object name, object id, and the composed instance transform, expanding every `InstanceReference` through its definition under a cycle-pinned branch set. Flattening `model.Objects` into one comprehension concatenates a single buffer instead, dropping every block-instanced mesh whole and erasing all four facts from the meshes it keeps; `meshio` and `trimesh` publish one whole-file node because neither carries an object graph past the load.
- Units: each row answers `units` as a `Posture` — `rhino3dm` DEFAULTED from `ModelUnitSystem` because a document declaring no unit still reports `Millimeters`, `trimesh` DECLARED where `Trimesh.units` is non-null and ABSENT where it is not, `meshio` ABSENT because that package carries no unit surface at all. Both `mesh.units or "m"` and a meshio row spelling `lambda _: "m"` publish a measurement no producer took.
- Receipt: `MeshReceipt` (the geometry/topology proof) and `QueryReceipt` (the columnar table proof) are two typed receipts disjoint by evidence axis, never one rail straddling both. `MeshColumns` bands the columnar egress by ARITY — `point` over the vertex-aligned columns, `cell.<block>` over each block's own cell census — and proves each band's widths at construction, because `pa.table` admits one length across every column handed it and one table spanning both arities raises `ArrowInvalid` on every mesh carrying cell data. `MeshFrame` and `MeshPayload` carry the node ROWS and `MeshReceipt` the node CENSUS, and a posture-carried fact OMITS its receipt key when absent rather than publishing an empty string a board reads as a declaration.
- Growth: a new surface format is one extension string on the `trimesh` row; a new FE format already routes through `meshio.extension_to_filetypes`; a new engine is one `MeshBackend` case plus one `_Backend` row; a new leg is one `MeshOp` case, one `run` arm, and one `MeshProduct` case; a new named array kind is one more dict the row `walk` folds, surfacing as one more egress column and receipt arity with no frame edit; zero per-format `read_*`/`write_*` family; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.MESH` in this module's one `RAISES` table, which every section anchors on.
- Boundary: no geometry kernel, no bridge lifecycle, no NURBS/Brep/SubD construction — the `rhino3dm` row reads `File3dm` meshes and instance references only, the offline 3dm reader per the geometry-flow law. Frame streaming is XDMF's alone, so a surface row carrying no frame reader refuses the leg by name instead of dying inside a codec it never opened.

```python signature
from collections.abc import Callable, Iterator, Mapping
from itertools import accumulate
from typing import Final, Literal, assert_never
from uuid import UUID

import meshio
import numpy as np
import pyarrow as pa
import rhino3dm
import trimesh
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Disposition, FaultRow, Posture, RuntimeRail, async_boundary, rostered, scoped, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.mesh")

# the raise anchors the retried legs on this page key on: `reliability/resilience#RESILIENCE` `guarded` takes the
# caller's own rostered `at: FaultRow[L]`, so the breaker arc, the rate bucket, the span, and the lifted fault all
# derive ONE coordinate the roster proves against a real module — the free `subject=<str>` it retired could spell a
# leg this package never declares. Every row here is a network-bearing leg, so each declares TRANSIENT.
POINTCLOUD_CLOUD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="cloud", arm="boundary", defect="cloud-read", retriability=TRANSIENT
)
# the rest of this module's raise roster beside it. Posture splits on what a re-offer can clear, never on the
# entrypoint: every leg crossing a file or a codec declares TRANSIENT, while each caller-repairable routing and
# shape gate declares TERMINAL. `slots` NAMES each gate's coordinates, so the joined message bodies these rows
# replace become fields a consumer gates on rather than prose it parses.
MESH_BACKEND: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="backend", arm="config", defect="suffix-unrouted", retriability=TERMINAL, slots=("suffix",)
)
MESH_ARITY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="arrays", arm="config", defect="band-width-disagreement", retriability=TERMINAL, slots=("band", "widths")
)
MESH_OPEN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MESH, point="open", arm="boundary", defect="engine-open", retriability=TRANSIENT
)
# ONE row for every banded leg — read, arrays, export, preview — because they share one thread hop, one engine, and
# one narrowed raise surface; the op that ran rides the span this row's fence opens beneath.
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
_PREVIEW: Final[str] = "glb"  # the format the preview case elects; the export case reads the caller's own token
_UNSET: Final[UUID] = UUID(int=0)  # `ObjectAttributes.Id` on an attribute block no document ever keyed
_POINT_BAND: Final[str] = "point"  # the vertex-arity band name; every cell band spells `cell.<block>`

type Engine = meshio.Mesh | trimesh.Trimesh | rhino3dm.File3dm
type Arrays = Mapping[str, np.ndarray]
type Blocks = Mapping[str, Arrays]  # cell arrays keyed BLOCK-first, so one band's arrays share one cell census
type Frames = Iterator[RuntimeRail[tuple[float, "MeshColumns"]]]


def _posted(key: str, posture: Posture[str]) -> dict[str, object]:
    # receipt projection for one posture-carried fact: an ABSENT fact omits its key rather than publishing `""` a
    # board joins as a declaration, and a DEFAULTED one publishes the standing-in source beside the value, so "the
    # file said millimetres" and "the document default said millimetres" never collapse into one series.
    sourced = posture.source.map(lambda origin: {f"{key}_source": origin}).default_value({})
    return posture.option().map(lambda value: {key: value} | sourced).default_value({})


# one walked node names WHICH object contributed WHICH slice of the concatenated vertex stack. A flattening walk
# strips layer, object name, object id, and instance transform before the frame ever sees a `.3dm` model, so
# nothing downstream can say which object a vertex came from. Every identity column is a posture because an object
# may carry no name and a solver deck carries no object graph at all — an empty string in either slot fuses
# "unnamed here" with "this format has no such concept".
class _Node(Struct, frozen=True):
    identity: Posture[str]
    name: Posture[str]
    path: Posture[str]
    transform_applied: bool
    vertex_span: tuple[int, int]


# one walked node's identity beside the arrays it contributed; `_folded` assigns the spans and stacks the arrays in
# one pass, so no engine arm carries a running offset and the per-vertex alignment law lives at one owner.
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


# frame carries the canonical-geometry identity (the `float64` point-buffer `ContentKey`), whatever named-array
# arities the receipt reports, and the node ROWS the payload crosses with; egress-driving column NAMES live on
# whichever transient `_Extract` the `arrays` leg reads, never re-stored. Mesh identity is its point geometry, so `_frame` is
# one `ContentIdentity.of(...).map(...)` rail, never a per-array key-derivation fold whose keys no consumer reads
# off the once-dropped buffers.
class MeshFrame(Struct, frozen=True):
    points: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    nodes: tuple[_Node, ...]


def _stacked(blocks: tuple[np.ndarray, ...]) -> np.ndarray:
    # one block passes THROUGH: `np.concatenate` over a single array copies a whole point buffer on every read,
    # and both whole-file engines walk exactly one node per file.
    return blocks[0] if len(blocks) == 1 else np.concatenate(blocks)


def _folded(pieces: Block[_Piece]) -> _Extract:
    # ONE fold over the walk: vertex spans assign here so no engine arm carries a running offset, and the aligned-aux
    # law generalizes from a `rhino3dm`-only stack to every engine — a per-vertex array stacks only when EVERY piece
    # declares it AND its row count equals that piece's own vertex count, because a model where one node defines
    # normals and another does not would otherwise concatenate a short array a "vertex"-keyed consumer reads off the
    # end of. Cell and field arrays are whole-file facts the single-node engines publish: the multi-node `rhino3dm`
    # walk publishes block NAMES and no connectivity, since vertex-indexed cells would have to rebase onto the
    # concatenated stack and the point buffer stays the sole identity preimage.
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


def _frame(extract: _Extract) -> RuntimeRail[MeshFrame]:
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
    # meshio keys cell data NAME-first (`{name: {cell_type: array}}`) and arrays under ONE name have different
    # lengths per cell type, so this transposes to BLOCK-first: inside one block every array shares that block's
    # own cell census, which is the single arity a table can carry. Flattening to `f"{name}.{cell_type}"` keys
    # instead hands `pa.table` columns of two lengths, which it refuses with `ArrowInvalid`.
    blocks = {cell_type for per_type in per_name.values() for cell_type in per_type}
    return {
        cell_type: {name: np.asarray(per_type[cell_type]) for name, per_type in per_name.items() if cell_type in per_type}
        for cell_type in sorted(blocks)
    }


def _meshio_walk(mesh: "meshio.Mesh") -> Block[_Piece]:
    # a solver deck is one flat mesh with no object graph, so the walk publishes one whole-file node whose identity,
    # name, and layer path are ABSENT rather than a filename standing in for a declaration the format never made,
    # and `transform_applied` is a measured `False` because a deck declares no instance graph to compose.
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
    # `Trimesh.visual` is the `ColorVisuals | TextureVisuals` union: only `ColorVisuals` carries the `vertex_colors`
    # `(N,4)` `uint8` RGBA property (synthesizing defaults when no color is defined), while `TextureVisuals` exposes
    # UV/material and no per-vertex color, so the color point array is a typed `isinstance` discriminant over the
    # union rather than a stringly-typed `getattr` probe — the texture arm yields no color array (the PIL-backed
    # `to_color` bake is outside this exchange owner). `transform_applied` is TRUE because the forced-mesh load
    # collapses the scene graph, so the vertices published already rode their node transforms.
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
    # layer roster reads ONCE per model and keys on each layer's own `Index`, never on table position:
    # `File3dmLayerTable.FindIndex` RAISES `IndexError` off the row set, which no `_Backend` row lists as a fault and
    # which would therefore cross the boundary as an unclassified defect. Definition members are skipped at the top
    # level because the instance-reference arm reaches them under their own composed transform; admitting them twice
    # would stack every block's geometry once in its authoring frame and again in each placement.
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
    # ONE recursive placement over the object graph: a mesh yields its own node, an instance reference yields its
    # definition's members under the composed transform, and every other geometry kind yields nothing. `branch`
    # pins every definition id already on this path, so a self-including definition terminates by name instead of
    # recursing until the interpreter stack dies inside an offloaded thread.
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
    # `GeometryBase.Transform` MUTATES in place and answers a bool, so a placed mesh COPIES first: transforming the
    # definition's own mesh would move every later reference of that block. A face is a 4-int tuple whose triangle
    # repeats index 3, so `face[3] != face[2]` is the quad probe the catalogue declares.
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
    # a per-vertex aux band is admitted for THIS node only when its arity matches this node's own vertex count; the
    # cross-node all-or-nothing rule is `_folded`'s, so neither owner re-derives the other's half of the law.
    rows = read(mesh)
    return Some(np.array(rows, dtype=dtype)) if len(rows) == len(mesh.Vertices) else Nothing


def _identified(attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    # `Id` reads the nil uuid on an attribute block no document ever keyed and `Name` reads `""` on an unnamed
    # object: both are the provider's own unset spellings and both project HERE, at the single read site.
    return Posture.of_optional(None if attrs.Id == _UNSET else str(attrs.Id))


def _named(attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    return Posture.of_optional(attrs.Name or None)


def _pathed(layers: Map[int, str], attrs: "rhino3dm.ObjectAttributes") -> Posture[str]:
    return Posture.of_option(layers.try_find(attrs.LayerIndex))


def _xdmf_frames(engine: str, ref: ResourceRef) -> Frames:
    # reader open and `read_points_cells` (where `ReadError` surfaces) run eagerly inside the fence; only the
    # per-step `read_data` loop stays lazy, its provider-fault lift deferred to the consumer that drains it — the
    # same STREAM-arm convention `runtime/transport/roots#RESOURCE` holds, the generator's own `with` owning the HDF5
    # `TimeSeriesReader.__exit__` close on exhaustion or break.
    reader = meshio.xdmf.TimeSeriesReader(str(ref.path))
    reader.read_points_cells()
    return _framed(engine, str(ref.path), reader)


def _framed(engine: str, source: str, reader: "meshio.xdmf.TimeSeriesReader") -> Frames:
    # each step bands exactly as the whole-file egress does, so a time series and a static read hand a consumer one
    # shape; the step index joins the receipt source because a series writes one table set per step over one file.
    with reader:
        for step in range(reader.num_steps):
            time, point_data, cell_data = reader.read_data(step)
            points = {name: np.asarray(array) for name, array in point_data.items()}
            yield MeshColumns.of(engine, f"{source}#{step}", points, _blocked(cell_data), Nothing).map(lambda held: (float(time), held))


class _Backend(Struct, frozen=True):
    load: Callable[[ResourceRef], Engine]
    walk: Callable[[Engine], Block[_Piece]]
    export: Callable[[Engine, ResourceRef, str], None]
    units: Callable[[Engine], Posture[str]]
    frames: Option[Callable[[str, ResourceRef], Frames]]
    fault: type[Exception] | tuple[type[Exception], ...]
    exts: frozenset[str]

    def extract(self, engine: Engine) -> _Extract:
        return _folded(self.walk(engine))

    def frame(self, engine: Engine) -> RuntimeRail[MeshFrame]:
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


# Each row narrows `fault` to the engine's real raise surface so a non-engine exception escapes rather than
# masquerading as a mesh fault: `rhino3dm` signals load failure by null (the `_rhino3dm_load` `FileNotFoundError`)
# plus `OSError` on the Draco/`Write` egress; `trimesh.load_mesh` splits its refusals across TWO builtins and the row
# carries both — an unregistered or undetectable `file_type` raises `NotImplementedError`, while a malformed source,
# an unset `file_type` on a file object, and a refused remote raise `ValueError` — plus `OSError` for the read and
# the `export` leg this same row narrows; `meshio` carries its own `ReadError`/`WriteError` codec roots. The tuple
# `catch` is the `runtime/reliability/faults#FAULT` `boundary` widening (`type[Exception] | tuple[...]`) the `except`
# clause accepts natively. `units` answers a POSTURE per row: a `.3dm` document with no declared unit still reports `Millimeters`,
# so that row is DEFAULTED against the member that supplied it; `Trimesh.units` is genuinely nullable, so its row
# declares what it holds and admits absence otherwise; `meshio` exposes no unit surface at all, so its row is ABSENT
# and a `"m"` published from that row is a measurement with no producer. `frames` is the XDMF reader, held
# by the FE row alone — the surface rows skip that leg by ABSENCE rather than by a boolean every arm re-reads.
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
    def of(ref: ResourceRef) -> "RuntimeRail[MeshBackend]":
        # an unrecognized suffix is a REFUSAL, never a silent meshio election: a `.get(suffix, "meshio")` default
        # hands every unknown extension to the FE reader, which dies inside its codec naming the file rather
        # than whichever routing decision mis-sent it. `to_result_with` builds the fault on the failing arm alone.
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
    # one PROVED `(table, QueryReceipt)` pair per ARITY band: `point` over the vertex-aligned columns and
    # `cell.<block>` over each cell block, whose rows are that block's own cell census. `pa.table` admits ONE length
    # across every column handed to it, so a single table spanning point-arity and cell-arity columns raises
    # `ArrowInvalid` on every mesh carrying both — banding makes that shape unrepresentable rather than rostering
    # `ArrowInvalid` on a `catch=` tuple, and each band still hands back the `(table, QueryReceipt)` pair every
    # sibling Arrow producer returns.
    bands: Map[str, tuple[pa.Table, QueryReceipt]]

    @staticmethod
    def of(engine: str, source: str, points: Arrays, cells: Blocks, rows: Option[int]) -> "RuntimeRail[MeshColumns]":
        # ACCUMULATE, so a mesh carrying two malformed blocks names both at once instead of one per re-run.
        celled = Block.of_seq(cells.items()).map(lambda row: _banded(engine, source, f"cell.{row[0]}", row[1], Nothing))
        banded = Block.singleton(_banded(engine, source, _POINT_BAND, points, rows)).append(celled)
        return traversed(banded, by=Disposition.ACCUMULATE).map(lambda pairs: MeshColumns(Map.of_seq(pairs)))


def _banded(engine: str, source: str, band: str, arrays: Arrays, rows: Option[int]) -> RuntimeRail[tuple[str, tuple[pa.Table, QueryReceipt]]]:
    # arity PROVES here, before anything is built: every column inside one band shares one length, measured against
    # the band's declared census wherever it has one. A disagreement refuses by band name carrying the widths found,
    # so no consumer meets a half-built table and no `catch=` tuple has to roster a provider raise for a shape this
    # owner decides on its own inputs.
    census = frozenset(len(array) for array in arrays.values()) | frozenset(rows.to_list())
    if len(census) > 1:
        return Error(MESH_ARITY.raised(band, ",".join(str(width) for width in sorted(census))))
    table = _columns(arrays)
    return QueryReceipt.railed(engine, source, table).map(lambda receipt: (band, (table, receipt)))


@tagged_union(frozen=True)
class MeshOp:
    # ONE surface over the whole file-exchange concern in both directions, the tag its own discriminant. `export`
    # carries the codec token explicitly because `Trimesh.export` and `meshio.write` both take one, so an
    # extensionless sink is servable; `preview` carries the destination alone because the format is this owner's own
    # `glb` election rather than the caller's, and fusing the two would erase which side chose the codec.
    tag: Literal["read", "arrays", "timeseries", "export", "preview"] = tag()
    read: None = case()
    arrays: None = case()
    timeseries: None = case()
    export: tuple[ResourceRef, str] = case()
    preview: ResourceRef = case()


@tagged_union(frozen=True)
class MeshProduct:
    # closed outcome family: identity payload, columnar pair, lazy FE frame stream, or content key of what was
    # written — never an erased `object` a consumer re-discriminates.
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


class MeshReceipt(Struct, frozen=True):
    backend: str
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    point_arrays: int
    cell_arrays: int
    field_arrays: int
    node_count: int
    units: Posture[str]

    def contribute(self) -> Iterator[Receipt]:
        # `domain`/`kind`/`key` are the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads —
        # the SAME pair handed `Metrics.record` beside the identity this payload minted — so the durable row lands in
        # the `mesh` partition a predicate prunes and rejoins the live series its twin emitted. Cells stay a receipt
        # fact rather than a second instrument: one vertex stack is the volume a regression moves, and a per-block
        # cell count is a topology shape a distribution over one number cannot carry. Nodes ride as CENSUS here
        # while `MeshPayload` carries rows, and an absent unit omits its key rather than publishing an empty string.
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
                    "nodes": self.node_count,
                }
                | _posted("units", self.units),
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

    def contribute(self) -> Iterator[Receipt]:
        return MeshReceipt(
            self.backend.tag,
            self.content_key,
            self.point_count,
            self.cell_blocks,
            self.point_arrays,
            self.cell_arrays,
            self.field_arrays,
            len(self.nodes),
            self.units,
        ).contribute()


class MeshExchange(Struct, frozen=True):
    # one file admitted ONCE: backend elected off the suffix, engine loaded on the banded thread hop, ref kept for
    # frame streaming and the export readback. Every op reads this one open.
    backend: MeshBackend
    ref: ResourceRef
    engine: Engine

    @staticmethod
    async def of(ref: ResourceRef) -> "RuntimeRail[MeshExchange]":
        # a whole-file provider load blocks on disk — the banded thread hop, never the loop. Election refuses ahead
        # of the hop, so an unroutable suffix never opens a span it would immediately close on a codec fault.
        match MeshBackend.of(ref):
            case Result(tag="ok", ok=backend):
                row = backend.row
                with _TRACER.start_as_current_span("mesh.open", attributes={"rasm.mesh.backend": backend.tag}):
                    opened = await async_boundary(MESH_OPEN, lambda: on_thread(lambda: row.load(ref)), catch=row.fault)
                    return opened.map(lambda engine: MeshExchange(backend, ref, engine))
            case Result(tag="error") as refused:
                return refused

    async def run(self, op: MeshOp) -> "RuntimeRail[MeshProduct]":
        # ONE entry over the whole concern: the tag IS the discriminant, the engine is already in hand, and the
        # closed `MeshProduct` keeps the outcome family addressable instead of an erased value a consumer
        # re-discriminates. A sibling `preview`/`write` pair forks the span, the receipt, and the election three
        # ways for one file, and each sibling re-runs the provider load this admission already paid.
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

    async def _banded(self, work: Callable[[], RuntimeRail[MeshProduct]]) -> "RuntimeRail[MeshProduct]":
        # every leg is disk- or CPU-bound inside a provider holding the GIL, so one banded thread hop serves them all
        # and the row's own `fault` tuple narrows the catch to that engine's real raise surface.
        return (await async_boundary(MESH_WORK, lambda: on_thread(work), catch=self.backend.row.fault)).bind(lambda rail: rail)

    async def _streamed(self) -> "RuntimeRail[MeshProduct]":
        # frame streaming is XDMF's alone, so a row holding no reader REFUSES by backend name; an ungated leg opens
        # `meshio.xdmf.TimeSeriesReader` over a file no meshio codec ever read.
        row = self.backend.row
        match row.frames:
            case Option(tag="some", some=open_frames):
                tag, ref = self.backend.tag, self.ref
                opened = await async_boundary(MESH_FRAMES, lambda: on_thread(lambda: open_frames(tag, ref)), catch=row.fault)
                return opened.map(lambda frames: MeshProduct(frames=frames))
            case _:
                return Error(MESH_UNSTREAMED.raised(self.backend.tag))

    def _payload(self) -> RuntimeRail[MeshProduct]:
        row = self.backend.row
        return row.frame(self.engine).map(lambda frame: MeshProduct(payload=MeshPayload.of(self.backend, frame, row.units(self.engine))))

    def _columned(self) -> RuntimeRail[MeshProduct]:
        # every band keys on the SAME canonical point-buffer identity the payload publishes, so a table and the mesh
        # it came from join on one key; deriving it here rather than reading it off a caller-held payload is what
        # lets the arrays leg run without a prior read. Point columns and cell columns ride SEPARATE bands because
        # they carry different row counts, which one table cannot hold.
        extract = self.backend.row.extract(self.engine)
        return _frame(extract).bind(
            lambda frame: MeshColumns.of(
                self.backend.tag, frame.points.hex, extract.point_data, extract.cell_data, Some(extract.point_count)
            ).map(lambda columns: MeshProduct(columns=columns))
        )

    def _written(self, out: ResourceRef, fmt: str) -> RuntimeRail[MeshProduct]:
        self.backend.row.export(self.engine, out, fmt)
        return ContentIdentity.of("mesh.export", out.path.read_bytes()).map(lambda key: MeshProduct(written=key))
```

## [03]-[POINTCLOUD]

- Owner: `CloudExchange` — the LAS/LAZ/COPC row over `laspy` alone. One `Selection` threads the `decompression_selection` mask identically through `laspy.read` and `CopcReader.open`, fixed once at admission, so a cloud subset skips fields it never reads — one optional carrier, never a parallel selective-read method. This owner carries the point-format id and CRS posture directly, never a second `PointFormat` struct duplicating what the receipt names.
- Entry: one `run(op)` discriminates `CloudOp` — `read | subset | to_arrow | write` — onto one closed `CloudProduct`. Per-leg sibling statics each re-derive the source discriminant and the CRS projection, and the whole-file ones run the `laspy` decode ON the event loop while only the subset leg hops the band; one entry puts every leg behind the same `THREAD_BAND`-bounded hop.
- Source: `CloudSource` is the closed remote/local/handle axis admitted ONCE at the head, and its `_SOURCE` row elects the span kind, the retry class, and the `http_num_threads` value together. Election reads the ref's own declared `scheme` column, never a `path.startswith(("http://", "https://"))` test — that test spells one discriminant per call site and classifies nothing at all for the file OBJECT `CopcReader.open` also admits.
- Reach: `laspy.read` takes a path or a stream and no url, so the whole-file legs refuse a remote source BY NAME while the subset leg admits all three cases — each projection cites the surface its provider declares rather than sharing one address every leg pretends to serve.
- CRS: `LasHeader.parse_crs` answers `None` for a file carrying no CRS VLR AND for a file whose VLR it cannot understand, so `str(... or "")` fuses both with a subset that carried none forward. Absence rides `Posture` from the single read site and the receipt omits the fact key rather than publishing an empty CRS a consumer reprojects against.
- Boundary: no geometry kernel registration, no scan-to-BIM compute, no `pdal` filter-graph, no host coupling — host-free file exchange feeding the geometry companion at the wire; the point records cross as one content-keyed `PointRecordTable` through the shared `_column` builder, never a `laspy`- or `pdal`-specific object and never a re-spelled column fold.

```python signature
from collections.abc import Callable, Iterator
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
from rasm.runtime.faults import Posture, RuntimeRail, async_boundary
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef, origin

# `_TRACER`, `_column`, and `_posted` are the [02]-[MESH] owners in this same module: one module-scope scope
# handle serves every leg, so a second `scoped` mint beside it would fork one instrumentation coordinate in two,
# and one receipt omit-fold serves both foreign edges.

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


# one row per source kind: what the leg costs (retry class), how a trace joins it (span kind), and the COPC thread
# election. The `Option` columns skip their stage by ABSENCE — a non-network read takes no retry budget, and a
# network read takes the catalogue's own `http_num_threads=80` default by omission rather than a local constant
# restating it.
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
    # ONE remote/local discriminant, admitted at the head: span kind, retry class, and thread election all read
    # this value through `_SOURCE`. A `path.startswith(("http://", "https://"))` test spells one decision once per
    # call site and classifies nothing at all for the file OBJECT `CopcReader.open` also admits.
    tag: Literal["local", "remote", "handle"] = tag()
    local: UPath = case()
    remote: str = case()
    handle: BinaryIO = case()

    @staticmethod
    def of(ref: ResourceRef) -> "CloudSource":
        # discrimination reads the ref's OWN declared scheme column, never a prefix test over its rendered path:
        # a residence that already stated its scheme at admission is not re-parsed here.
        return CloudSource(remote=str(ref.path)) if ref.scheme in _REMOTE else CloudSource(local=ref.path)

    @property
    def row(self) -> _SourceRow:
        return _SOURCE[self.tag]

    @property
    def peer(self) -> Option[str]:
        # WHICH ORIGIN this source dials, derived from the same value `row` reads for the crossing — the
        # `reliability/resilience#RESILIENCE` window keys the dependency INSTANCE, so the raw COPC href would mint
        # one arc per file and no arc would reach its trip. The local and handle arms answer `Nothing`, which is
        # exactly what their non-retried crossing needs and what `_keyed` no-ops on.
        return Some(origin(self.remote)) if self.tag == "remote" else Nothing

    @property
    def addressed(self) -> "str | BinaryIO":
        # `CopcReader.open` admits a path, an http(s) url, or a file object alike, so every case projects.
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
    def whole(self) -> "RuntimeRail[str | BinaryIO]":
        # `laspy.read` takes a path or a stream and NO url, so the whole-file legs refuse a remote source by name
        # rather than handing a url to a codec that would open it as a filename.
        match self:
            case CloudSource(tag="remote", remote=url):
                return Error(CLOUD_SOURCE.raised(url))
            case _:
                return Ok(self.addressed)


def _masked(selection: Selection) -> dict[str, object]:
    # mask rides as an OMITTED keyword when absent, so neither reader carries an `is not None` fork and the
    # provider's own `DecompressionSelection.all()` default stands where this owner declares nothing.
    return selection.map(lambda mask: {"decompression_selection": mask}).default_value({})


def _coords(record: Record) -> np.ndarray:
    return np.ascontiguousarray(record.xyz, dtype="float64")


def _crs(header: "laspy.LasHeader") -> Posture[str]:
    # `parse_crs` answers `pyproj.CRS | None` and reads `None` for BOTH a file carrying no CRS VLR and a file whose
    # VLR it cannot understand; `str(... or "")` fuses those two with a subset that carried none forward, so all
    # three publish one declared-empty CRS. The sentinel projects HERE, at the single read site.
    return Posture.of_option(Option.of_optional(header.parse_crs()).map(str))


def _to_arrow(record: Record) -> pa.Table:
    coords = _coords(record)
    base = {"x": coords[:, 0], "y": coords[:, 1], "z": coords[:, 2]}
    columns = base | {name: np.asarray(record[name]) for name in record.point_format.dimension_names if name not in base}
    return pa.table({name: _column(array) for name, array in columns.items()})


# one point-cloud emitted-phase evidence both the content-keyed table and the frozen owner contribute, native scalars
# the receipts `Encoder(enc_hook=repr)` serializes without a `str()` coerce. `domain`/`kind`/`key` are the lifted
# evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads — the SAME pair handed `Metrics.record` beside
# the minted identity — and `kind` is the LAS point-format id because that closed eleven-member set is the bounded
# dimension a board joins on, where a CRS or a content key would fork the series per file. An absent CRS omits its
# key, so a file that declared none never joins a reprojection predicate as though it had.
def _pointcloud_receipt(content_key: ContentKey, point_count: int, point_format: int, crs: Posture[str]) -> Iterator[Receipt]:
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
            }
            | _posted("crs", crs),
        ),
    )


class PointRecordTable(Struct, frozen=True):
    table: pa.Table
    point_count: int
    point_format: int
    crs: Posture[str]
    content_key: ContentKey

    @classmethod
    def of(cls, record: Record, crs: Posture[str]) -> "RuntimeRail[PointRecordTable]":
        coords = _coords(record)
        return ContentIdentity.of("pointcloud", coords.tobytes()).map(
            lambda key: cls(table=_to_arrow(record), point_count=len(coords), point_format=int(record.point_format.id), crs=crs, content_key=key)
        )

    def contribute(self) -> Iterator[Receipt]:
        return _pointcloud_receipt(self.content_key, self.point_count, self.point_format, self.crs)


class PointCloud(Struct, frozen=True):
    content_key: ContentKey
    point_count: int
    point_format: int
    crs: Posture[str]

    def contribute(self) -> Iterator[Receipt]:
        return _pointcloud_receipt(self.content_key, self.point_count, self.point_format, self.crs)


@tagged_union(frozen=True)
class CloudOp:
    # ONE surface over the point-cloud concern in both directions; the four sibling statics each re-derived the
    # source discriminant and the CRS projection, and a caller chose its retry budget by picking a method name.
    tag: Literal["read", "subset", "to_arrow", "write"] = tag()
    read: None = case()
    subset: CopcQuery = case()
    to_arrow: None = case()
    write: tuple[ResourceRef, WriteMode] = case()


@tagged_union(frozen=True)
class CloudProduct:
    # closed outcome family: header-level cloud identity, content-keyed record table, or content key of what was
    # written.
    tag: Literal["cloud", "records", "written"] = tag()
    cloud: PointCloud = case()
    records: PointRecordTable = case()
    written: ContentKey = case()


def _laz_backend() -> laspy.LazBackend:
    # backend probe rides the module-scope `laspy` binding: `is_available()` answers whether the lazrs or
    # laszip native band landed on this lane, so the roster order IS the preference and no second import exists.
    backends = (laspy.LazBackend.LazrsParallel, laspy.LazBackend.Lazrs, laspy.LazBackend.Laszip)
    backend = next((b for b in backends if b.is_available()), None)
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


def _open_copc(source: CloudSource, selection: Selection) -> "laspy.copc.CopcReader":
    # thread election reads the SOURCE row, never a second prefix test: a remote leg takes the catalogue's own
    # `http_num_threads=80` default by omission and every non-network leg forces `1` to serialize the single read.
    threads = source.row.threads.map(lambda count: {"http_num_threads": count}).default_value({})
    return laspy.copc.CopcReader.open(source.addressed, **threads, **_masked(selection))


class CloudExchange(Struct, frozen=True):
    # one source admitted ONCE at the head beside the decompression mask fixed at open; one entry runs every op,
    # and its carried row elects the envelope, so no leg re-derives a discriminant this admission already settled.
    source: CloudSource
    selection: Selection = Nothing

    async def run(self, op: CloudOp) -> "RuntimeRail[CloudProduct]":
        subject = f"pointcloud.{op.tag}"
        row = self.source.row
        # remote COPC legs are outbound network reads — kind=CLIENT per the store span-kind law; every other
        # source stays INTERNAL, and the row answers both that kind and the retry class no caller can override.
        with _TRACER.start_as_current_span(subject, kind=row.span_kind, attributes={"rasm.pointcloud.source": self.source.tag}):
            return (await self._enveloped(row, self._work(op))).bind(lambda rail: rail)

    def _work(self, op: CloudOp) -> Callable[[], RuntimeRail[CloudProduct]]:
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

    async def _enveloped(self, row: _SourceRow, work: Callable[[], RuntimeRail[CloudProduct]]) -> "RuntimeRail[RuntimeRail[CloudProduct]]":
        # retry budget is the ROW's, never the caller's method pick: `guarded` wraps the same banded hop the
        # non-network leg takes bare, so a network-bearing read cannot reach an un-retried entry and a local read
        # cannot spend an HTTP budget. `CopcReader.open` reads the `copc_info` header and root octree page eagerly
        # over `requests` before `query` pages a chunk, which is why the remote leg is network-bearing.
        match row.retry:
            case Option(tag="some", some=cls):
                # `at` names WHICH CALL, `on` WHICH PEER — the source's own derived origin, so every COPC read of one
                # archive shares its arc instead of minting a fresh window per file.
                return await guarded(cls, on_thread, work, abandon=True, at=POINTCLOUD_CLOUD, on=self.source.peer)
            case _:
                return await async_boundary(POINTCLOUD_CLOUD, lambda: on_thread(work), catch=laspy.LaspyException)

    def _cloud(self) -> RuntimeRail[CloudProduct]:
        return self.source.whole.bind(lambda address: _headed(laspy.read(address, **_masked(self.selection))))

    def _table(self) -> RuntimeRail[CloudProduct]:
        return self.source.whole.bind(lambda address: _recorded(laspy.read(address, **_masked(self.selection))))

    def _records(self, query: CopcQuery) -> RuntimeRail[CloudProduct]:
        # subset keeps the COPC header's own CRS posture, never dropping it to an empty string a caller then
        # cannot tell from a file that declared none.
        reader = _open_copc(self.source, self.selection)
        return PointRecordTable.of(query.query(reader), _crs(reader.header)).map(lambda records: CloudProduct(records=records))

    def _written(self, out: ResourceRef, mode: WriteMode) -> RuntimeRail[CloudProduct]:
        return self.source.whole.bind(lambda address: _landed(laspy.read(address), out, mode))


def _headed(data: "laspy.LasData") -> RuntimeRail[CloudProduct]:
    # header-level identity alone: the point buffer keys the cloud and the header answers count, format, and CRS
    # posture, so the read leg never materializes an Arrow table the caller did not ask for.
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


def _recorded(data: "laspy.LasData") -> RuntimeRail[CloudProduct]:
    return PointRecordTable.of(data, _crs(data.header)).map(lambda records: CloudProduct(records=records))


def _landed(data: "laspy.LasData", out: ResourceRef, mode: WriteMode) -> RuntimeRail[CloudProduct]:
    _WRITE[mode](data, str(out.path))
    return ContentIdentity.of("pointcloud.write", out.path.read_bytes()).map(lambda key: CloudProduct(written=key))
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
from rasm.runtime.faults import RuntimeRail, boundary
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
    # bytes first would leave a mislabelled archive on disk for a reader to sniff wrong. The `catch` names this
    # fence's own raise surface — `write_bytes` and `read_bytes` raise `OSError` and nothing else here throws — so a
    # foreign raise propagates as a defect instead of re-keying as a product fault.
    row = kind.row
    if out.path.suffix.lower() != row.suffix:
        named = out.path.suffix.lower() or "none"
        return Error(PRODUCT_SUFFIX.raised(row.subject, row.suffix, named))

    def run() -> RuntimeRail[ProductReceipt]:
        out.path.write_bytes(payload)
        return ContentIdentity.of(f"product.{row.subject}", payload).map(
            lambda key: ProductReceipt(kind=kind, path=str(out.path), media=row.media, byte_length=len(payload), content_key=key)
        )

    with _TRACER.start_as_current_span(f"product.{row.subject}", attributes={"rasm.product.kind": kind.value}):
        return boundary(PRODUCT_WRITE, run, catch=OSError).bind(lambda rail: rail)
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

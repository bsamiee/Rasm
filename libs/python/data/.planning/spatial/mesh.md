# [PY_DATA_MESH]

The mesh-file exchange owner over a `MeshBackend` axis, plus the point-cloud interchange row. `MeshPayload` carries mesh-file identity, cell-block topology, units, named point/cell/field-array arities, the FE time-series rail, and preview export over one `_BACKEND` behavior table pairing each `MeshBackend` tag with its loader, frame projector, exporter, unit reader, fault root, and extension set — `meshio.read`/`write` for FE volume/cell-block meshes, `trimesh.load`/`Trimesh.export` for surface meshes, and `rhino3dm.File3dm.Read` for `.3dm` mesh exchange — so a new engine is one `MeshBackend` case plus one `_BACKEND` row, never a parallel `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder family and never a per-engine `match` arm in `frame`/`export`. The backend is one `MeshBackend` tagged-union case recovered from the source extension, never a knob; the cell-block topology folds `meshio.Mesh.cells_dict`, the single `trimesh` `triangle` block, and the `rhino3dm.Mesh` `triangle`/`quad` block onto one `cell_blocks` field; the source engine loads exactly once per operation and the loaded object threads through the row reader so `read`/`arrays`/`preview`/`write` never re-open the file. The frozen `MeshFrame`/`MeshPayload` carry the canonical-geometry `ContentKey` over the `float64` point buffer plus the named-array arities the receipt reports — never a per-array key the once-dropped buffers no consumer reads, and never the raw NumPy buffers inlined on the struct. The named-array egress materializes off the transient `_Extract` and rides the shared `tabular/columnar#COLUMNAR` `QueryReceipt.railed` Arrow rail — the same `(table, QueryReceipt)` pair `columnar`'s `scan` and `geospatial`'s `SpatialEngine.run` return, content-keyed over the canonical Arrow bytes — building columns straight from the buffers through one shared `_column` fold the point-cloud row imports rather than re-spells (`pyarrow.array` for a 1D dimension, `pyarrow.FixedSizeListArray.from_arrays` over the flat buffer for a multi-dim stack), never a bare un-keyed `pa.Table` and never a `.tolist()` row materialization. Every entrypoint wraps its `boundary`/`async_boundary` fence in one `_TRACER.start_as_current_span(f"mesh.<op>")` so each file-exchange op is an OTel span the runtime `boundary` `_convert` records the terminal raise on — the same per-op span rail `tabular/egress#EGRESS`, `spatial/geospatial#GEO`, and `spatial/catalog#CATALOG` open, never an unspanned fold. `PointCloud` is the LAS/LAZ/COPC interchange row over `laspy` alone — `laspy.read`/`LasData.write` for the full-file LAS/LAZ decode/encode and `laspy.copc.CopcReader` for the octree-chunked spatial subset — bridging to a content-keyed `pyarrow.Table` columnar point-record table through one polymorphic dimension fold that serves both the full `LasData` read and the `ScaleAwarePointRecord` subset, feeding the geometry scan-processing/registration companion at the mesh seam. The COPC subset over an HTTP/HTTPS URL is network-bearing — `CopcReader.open` reads the `copc_info` header and root octree page eagerly over `requests` before `query` pages a chunk — so the remote leg routes through one `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` envelope offloading the blocking `open`-plus-`query` sequence under the one `stamina` `HTTP` `Retry-After`-honouring policy row and lifting the terminal raise through `async_boundary` exactly once, the same retry/span/lift triplet `spatial/catalog#CATALOG` and `spatial/geospatial#GEO` delegate to the runtime resilience owner; a local-path COPC read stays the synchronous spanned `boundary`. runtime `laspy.copc` reads uncompressed COPC and resolves the `lazrs` COPC codec internally; the `LasData.write` LAZ transcode binds the `lazrs`/`laszip` codec backend through the verified `LazBackend.{Lazrs, LazrsParallel, Laszip}` selector function-local on the worker worker lane. Every boundary narrows `catch=` to the engine's own fault root so a non-engine exception escapes rather than masquerading as a mesh fault, and the raised fault converts exactly once through the runtime `boundary` owner into `BoundaryFault`. This is file exchange and identity — the IFC-to-GLB tessellation rail belongs to the geometry package, never re-derived here, and the geometry `pdal` filter-graph (SMRF/PMF/outlier/decimation/range) stays geometry-owned. Every payload keys by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[MESH]: mesh-file identity, cell-block topology, named point/cell/field-array arities, the FE time-series rail, units, and GLB preview export over one `MeshBackend` axis, every op a `_TRACER` span and the named-array egress riding the shared `tabular/columnar#COLUMNAR` `QueryReceipt` Arrow rail over the shared `_column` builder.
- [02]-[POINTCLOUD]: the LAS/LAZ/COPC point-cloud interchange row over `laspy`, the uncompressed/compressed COPC octree-subset split, the columnar point-record bridge over the imported `_column` builder, and the remote-COPC `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` resilience envelope.

## [02]-[MESH]

- Owner: `MeshPayload` — mesh-file identity, cell-block topology, named point/cell/field-array arities, units, the FE time-series rail, and preview export over the `MeshBackend` tagged-union axis routed through one `_BACKEND` behavior table: `meshio` for FE volume/cell-block meshes, `trimesh` for surface meshes, `rhino3dm` for `.3dm` mesh exchange. Each `_Backend` row pairs the loader, one engine-specific `extract` accessor pack projecting `(points, cell_blocks, point_data, cell_data, field_data)` into the engine-agnostic `_Extract`, the exporter, the unit reader, the per-engine raised-fault set the `boundary` narrows `catch=` to (`(FileNotFoundError, OSError)` for `rhino3dm`, `(ValueError, OSError)` for `trimesh`, `(meshio.ReadError, meshio.WriteError)` for `meshio` — a `type[Exception] | tuple[...]`, the `reliability/faults#FAULT` `boundary` widening over the engine's real raise surface, never a single `Exception` catch-all), and the extension set; the `_Backend.frame` method calls the one shared `_frame` over `extract` returning `RuntimeRail[MeshFrame]` — one `ContentIdentity.of(...).map(...)` rail over the canonical `float64` point buffer, because the mesh's identity is its point geometry — so the `MeshFrame` assembly is written once and a new engine is one `MeshBackend` case plus one `_BACKEND` row carrying its `extract` arm, never a parallel `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder triple and never a per-engine arm in `frame`/`export`. `cell_blocks` folds `meshio.Mesh.cells_dict` keys, the single `trimesh` `triangle` block, and the `rhino3dm.Mesh` `triangle`/`quad` block onto one field; the named `point_data`/`cell_data`/`field_data` arrays cross only at the `arrays` egress off the transient `_Extract`, the raw NumPy buffers never inlined on the frozen `MeshFrame`/`MeshPayload` and never a per-array `ContentKey` minted on the read path that no consumer reads back off the dropped buffers — the frame carries the point-geometry key plus the three named-array arities the receipt reports. `MeshReceipt` is the typed mesh receipt — backend, point count, cell blocks, point/cell/field array counts, units, content key — its `contribute` yielding the `Iterable[Receipt]` the runtime `ReceiptContributor` Protocol declares, never a single bare `Receipt`.
- Entry: every entrypoint opens one `_TRACER.start_as_current_span(f"mesh.<op>")` around its `boundary` fence so the file-exchange op is an OTel span the runtime `boundary` `_convert` records the terminal raise on, never a per-op `try`/`except` and never an unspanned fold. `MeshPayload.read` admits a mesh file, loads the engine once through the resolved `_Backend.load`, threads the `RuntimeRail[MeshFrame]` the row `frame` returns through `.map`, and `bind`-flattens the frozen owner keyed by `ContentIdentity` over the canonical point bytes — the one inner-rail flatten the identity-derivation rail forces, the same `.bind(lambda rail: rail)` shape `tabular/egress#EGRESS` `run` holds; `MeshPayload.arrays` materializes the named `point_data`/`cell_data` arrays off the transient `_Extract` the row `extract` projects (never off a re-stored frame) through one `_columns` fold over the one shared `_column` builder — a 1D array folds as `pyarrow.array`, a multi-dim array (the vertex-normal/color stacks) folds as `pyarrow.FixedSizeListArray.from_arrays` over the flat NumPy buffer, never a `.tolist()` row materialization — then rides the shared `tabular/columnar#COLUMNAR` `QueryReceipt.railed(backend, content_key.hex, table)` Arrow rail returning the `(pa.Table, QueryReceipt)` pair content-keyed over the canonical Arrow bytes, the same Arrow-egress shape `columnar.scan`/`geospatial.SpatialEngine.run` hold, never a bare un-keyed `pyarrow.Table`; `MeshPayload.timeseries` opens the `meshio.xdmf.TimeSeriesReader` and reads its topology eagerly inside the fence (where a `ReadError` surfaces), then returns the lazy `_frames` generator streaming one timestep at a time as `(time, point_data, cell_data)` `pyarrow.Table` frames through the same `_columns` fold, the generator's own `with reader` owning the HDF5 `__exit__` close and the per-step `read_data` fault deferred to the draining consumer — the STREAM-arm convention `transport/roots#RESOURCE` holds, never a `boundary` wrapping only the generator's construction; `MeshPayload.preview` emits a `glb` preview render through the row exporter over the once-loaded engine; `MeshPayload.write` round-trips through the format the requested extension selects over the same once-loaded engine, its `ContentIdentity.of` export-bytes rail `bind`-flattened. Every entrypoint returns a `RuntimeRail`, the `boundary` narrowing `catch=` to the row's per-engine fault set so a non-engine exception escapes rather than converting to a mesh fault.
- Auto: `MeshBackend.of` resolves the case off the source extension through the one `_EXT` `frozendict[str, str]` extension-to-tag data table built by left-to-right union — the `meshio.extension_to_filetypes` FE base, then the `trimesh` surface override winning the shared `.obj`/`.off`/`.ply`/`.stl` extensions, then the `rhino3dm` `.3dm` override — so the later-key-wins union fixes the precedence deterministically rather than resting on a `Map` iteration order (the `frozendict` data owner the data-valued extension-to-tag row takes, distinct from the `Map`-keyed `_BACKEND`/`_WRITE` callable-behavior tables), defaulting an unrecognized suffix to the `meshio` tag rather than an `if`-ladder over the three sets; `trimesh` owns the surface (display/exchange) extensions, `meshio` owns the FE volume/cell-block formats keyed by `meshio.extension_to_filetypes` rather than an empty fallthrough set, and `rhino3dm` owns `.3dm`, and the row owns its `load`/`extract`/`export`/`units`/`fault` arms; a new surface format is one extension string on the `trimesh` row, a new FE format is already in `meshio.extension_to_filetypes`, and the `.3dm` row is the `rhino3dm` case folding `File3dm.Read` over every `File3dmObjectTable` mesh plus `DracoCompression` transport. The `meshio` row mines `Mesh.cells_dict` for the block topology, `Mesh.cell_data_dict` for the per-type named cell arrays, `Mesh.point_data` for the point arrays, and `Mesh.field_data` for the named scalar metadata; the `trimesh` row mines `Trimesh.faces` for the triangle block and connectivity cell array and discriminates `Trimesh.visual` (the `ColorVisuals | TextureVisuals` union) by `isinstance` to read `ColorVisuals.vertex_colors` for the per-vertex color point array — the `TextureVisuals` arm yielding no color rather than a stringly-typed `getattr` probe — off the one `Trimesh.units` reader; the `rhino3dm` row null-checks `File3dm.Read` (the binding signals failure by null, never exception) before folding the object-table meshes, mining `Mesh.Vertices` for the point stack, `Mesh.Faces` for the triangle/quad block split, and `Mesh.Normals`/`Mesh.VertexColors` for the per-vertex normal and color point arrays through the one `_stack` fold that emits a `"vertex"`-keyed aux stack only when every object-table mesh carries it at full vertex arity (`all(len(rows) == len(mesh.Vertices))` over the `zip(strict=True)` pairing) so the concatenated array stays row-aligned with the vertex `point_count` rather than a filtered comprehension whose length tracks only the present-subset meshes and reads off the end of a mixed model, and reads the model units from `File3dm.Settings.ModelUnitSystem.name` (the live `UnitSystem` enum, default `Millimeters`) rather than a hardcoded `"m"` that discards the document's declared unit system, so the absent-file case raises a typed fault rather than the `None`-attribute crash the bare boundary masks. The cell-block topology, the named-array dicts, and the time-series rail (`xdmf.TimeSeriesReader.num_steps`/`read_data`) all read off the once-loaded engine the row selects, never a second loader.
- Receipt: a mesh read contributes an emitted-phase fact through `ReceiptContributor.contribute` — a generator yielding one `Receipt.of("mesh", ("emitted", subject, facts))` over the two-argument `(owner, evidence)` factory the `observability/receipts#RECEIPT` owner exposes, never the four-positional `Receipt.of(phase, owner, subject, facts)` and never a single bare `Receipt` against the `Iterable[Receipt]` Protocol — keyed by the canonical point-bytes `ContentKey` `hex`, carrying the backend tag, point count, cell-block tuple, point/cell/field array counts, and units as native scalars on the `dict[str, object]` facts map the receipts `Encoder(enc_hook=repr)` renderer serializes without a `str()` coerce; the named-array egress carries its own `tabular/columnar#COLUMNAR` `QueryReceipt` keyed over the canonical Arrow bytes rather than the mesh point-geometry key, the two typed receipts disjoint by evidence axis — `MeshReceipt` the geometry/topology proof, `QueryReceipt` the columnar table proof — never one rail straddling both.
- Packages: `meshio` (`read`/`write`/`Mesh`/`Mesh.points`/`Mesh.cells_dict`/`Mesh.cell_data_dict`/`Mesh.point_data`/`Mesh.field_data`/`extension_to_filetypes`/`ReadError`/`WriteError`/`xdmf.TimeSeriesReader.{read_points_cells,num_steps,read_data}`), `trimesh` (`load`/`Trimesh.export`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`/`Trimesh.visual`/`visual.ColorVisuals`/`visual.ColorVisuals.vertex_colors`/`visual.TextureVisuals`), `rhino3dm` (`File3dm.Read`/`File3dm.Settings`/`File3dmSettings.ModelUnitSystem`/`UnitSystem`/`File3dmObjectTable`/`Mesh`/`Mesh.Vertices`/`Mesh.Faces`/`Mesh.Normals`/`Mesh.VertexColors`/`DracoCompression.Compress`/`DracoCompressionOptions`), `laspy`/`lazrs`/`laszip` (the `[03]-[POINTCLOUD]` row), `numpy` (`ascontiguousarray`/`concatenate` the canonical array bytes), `pyarrow` (`array`/`table`/`FixedSizeListArray.from_arrays` the named-array egress columns), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-op mesh span), `tabular/columnar` (`QueryReceipt.railed` the shared Arrow-bytes-keyed table receipt the `arrays` egress rides, the same rail `geospatial` imports), runtime (`ContentIdentity.of` the identity rail the `_frame` point key threads, `ContentKey`, `ResourceRef`, `RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new surface format is one extension string on the `_BACKEND` `trimesh` row; a new FE format already routes through `meshio.extension_to_filetypes`; a new mesh engine is one `MeshBackend` case plus one `_Backend` row carrying its load/extract/export/units/fault arms; a new named array kind is one more dict the row `extract` folds into the `_Extract` data, surfacing as one more column in the `arrays` egress and one more in the receipt arity with no frame edit; the named-array egress rides the existing `tabular/columnar#COLUMNAR` `QueryReceipt.railed` Arrow rail; the FE time-series rail is the existing `timeseries` generator over `xdmf.TimeSeriesReader`; zero per-format `read_*`/`write_*` family, never a per-engine `match` arm beside the `_BACKEND` table, and never a parallel `MeshioPayload`/`TrimeshPayload`/`Rhino3dmPayload` triple.
- Boundary: no geometry kernel (that is the geometry admission), no bridge lifecycle, no NURBS/Brep/SubD construction (the `rhino3dm` row reads `File3dm.Read` meshes only, the offline 3dm reader per the geometry-flow law, never a geometry kernel); the IFC-to-GLB tessellation rail is geometry-owned; the point-cloud row is host-free file exchange feeding the geometry scan companion at the wire, never a managed-interior coupling; a hand-rolled mesh parser, a parallel `MeshioPayload`/`TrimeshPayload` pair, an `_meshio_frame`/`_trimesh_frame`/`_rhino3dm_frame` builder triple beside the one `_frame` fold over the `_BACKEND` `extract` rows, a per-engine `match` arm in `frame`/`export`, an `if`-ladder `MeshBackend.of` beside the one `_EXT` table, an empty meshio fallthrough set where `meshio.extension_to_filetypes` enumerates the FE formats, a second engine load per operation, an un-narrowed `catch=Exception` boundary where the row binds its real per-engine fault set, an inlined raw NumPy array on the frozen `MeshFrame`/`MeshPayload` where the buffers live only on the transient `_Extract`, a per-array `ContentKey` minted on the read path that no consumer reads back off the dropped buffers (the dead `ArrayRef`/`_ref`/`_refs`/`traversed` derivation), a bare un-keyed `pyarrow.Table` from `arrays` where the named-array egress rides the shared `tabular/columnar#COLUMNAR` `QueryReceipt.railed` Arrow rail like every sibling Arrow producer, a `# type: ignore`-swallowed `ContentIdentity.of` treated as a bare `ContentKey` where the derivation is the `RuntimeRail` the `.map` threads, an unchecked `File3dm.Read` null, a `.tolist()` row materialization where `FixedSizeListArray.from_arrays` folds the multi-dim buffer, a filtered `[... for mesh in meshes if len(mesh.Normals)]` per-vertex aux comprehension whose concatenated length tracks only the present-subset meshes and reads off the end of the vertex stack on a mixed `.3dm` model where the `_stack` `all(len(rows) == len(mesh.Vertices))` gate emits a row-aligned stack or none, a `rhino3dm` NURBS construction call, an unspanned `boundary` fence where every sibling exchange op opens one `_TRACER.start_as_current_span`, a `trace.get_tracer(__name__)` where the sibling owners resolve the explicit dotted module string, a four-positional `Receipt.of(phase, owner, subject, facts)` against the two-argument `(owner, evidence)` form, a `contribute` returning a single bare `Receipt` against the `Iterable[Receipt]` Protocol, a `str()`-pre-formatted numeric receipt fact where the receipts `Encoder(enc_hook=repr)` renderer serializes the native scalar, and a re-spelled `_column`/`_columns` fold in the point-cloud fence where the one shared builder is imported are the deleted forms.

```python signature
from builtins import frozendict
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
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

_TRACER: Final = trace.get_tracer("rasm.data.spatial.mesh")

type Engine = meshio.Mesh | trimesh.Trimesh | rhino3dm.File3dm
type Arrays = Mapping[str, np.ndarray]
type Frames = Iterator[tuple[float, pa.Table, pa.Table]]


class _Extract(Struct, frozen=True):
    points: np.ndarray
    cell_blocks: tuple[str, ...]
    point_data: Arrays
    cell_data: Arrays
    field_data: Arrays


# The frame carries only the canonical-geometry identity (the `float64` point-buffer
# `ContentKey`) and the named-array arities the receipt reports; the column NAMES that drive
# egress live on the transient `_Extract` the `arrays` fence reads, never re-stored. The mesh
# identity is its point geometry, so `_frame` is one `ContentIdentity.of(...).map(...)` rail —
# never a per-array key-derivation fold whose keys no consumer reads off the once-dropped buffers.
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
    # `Trimesh.visual` is the `ColorVisuals | TextureVisuals` union: only `ColorVisuals` carries the
    # `vertex_colors` `(N,4)` `uint8` RGBA property (synthesizing defaults when no color is defined),
    # while `TextureVisuals` exposes UV/material and no per-vertex color, so the color point array is a
    # typed `isinstance` discriminant over the union rather than a stringly-typed `getattr` probe — the
    # texture arm yields no color array (the PIL-backed `to_color` bake is outside this exchange owner).
    visual = surface.visual
    color = {"color": np.asarray(visual.vertex_colors)} if isinstance(visual, trimesh.visual.ColorVisuals) else {}
    return _Extract(
        points=np.asarray(surface.vertices),
        cell_blocks=("triangle",) if len(surface.faces) else (),
        point_data=color,
        cell_data={"triangle": np.asarray(surface.faces)} if len(surface.faces) else {},
        field_data={},
    )


# A per-vertex aux stack (`normal`/`color`) is emitted only when EVERY object-table mesh carries it,
# so the concatenated array is row-aligned with the vertex stack `point_count` keys: a mixed model
# where one mesh defines normals and another does not would otherwise concatenate a short array a
# "vertex"-keyed consumer reads off the end of, so the absent-on-any case drops the whole aux array
# rather than minting a length-mismatched one — the `all(...)`-gated `_stack` fold, never a filtered
# `[... for mesh in meshes if len(mesh.Normals)]` comprehension whose length tracks the present subset.
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


# Each row narrows `fault` to the engine's real raise surface so a non-engine exception escapes
# rather than masquerading as a mesh fault: `rhino3dm` signals load failure by null (the
# `_rhino3dm_load` `FileNotFoundError`) plus `OSError` on the Draco/`Write` egress; `trimesh.load`
# raises a bare `ValueError` on a malformed/unknown source plus `OSError`; `meshio` carries its own
# `ReadError`/`WriteError` codec roots. The tuple `catch` is the `reliability/faults#FAULT` `boundary`
# widening (`type[Exception] | tuple[...]`) the `except` clause accepts natively.
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

_EXT: Final[frozendict[str, str]] = frozendict(
    {ext: "meshio" for ext in _BACKEND["meshio"].exts}
    | {ext: "trimesh" for ext in _BACKEND["trimesh"].exts}
    | {ext: "rhino3dm" for ext in _BACKEND["rhino3dm"].exts}
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
        yield Receipt.of(
            "mesh",
            (
                "emitted",
                self.content_key.hex,
                {
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
    def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        backend = MeshBackend.of(ref)
        row = backend.row

        def run() -> RuntimeRail[MeshPayload]:
            engine = row.load(ref)
            return row.frame(engine).map(lambda frame: cls._build(backend, frame, row.units(engine)))

        with _TRACER.start_as_current_span("mesh.read", attributes={"rasm.mesh.backend": backend.tag}):
            return boundary("mesh.read", run, catch=row.fault).bind(lambda rail: rail)

    def arrays(self, ref: ResourceRef) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
        row = self.backend.row

        def run() -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
            extract = row.extract(row.load(ref))
            table = _columns({**extract.point_data, **extract.cell_data})
            return QueryReceipt.railed(self.backend.tag, self.content_key.hex, table).map(lambda receipt: (table, receipt))

        with _TRACER.start_as_current_span("mesh.arrays", attributes={"rasm.mesh.backend": self.backend.tag}):
            return boundary("mesh.arrays", run, catch=row.fault).bind(lambda rail: rail)

    def timeseries(self, ref: ResourceRef) -> "RuntimeRail[Frames]":
        # the reader open and `read_points_cells` (where `ReadError` surfaces) run eagerly inside the
        # fence; only the per-step `read_data` loop stays lazy, its provider-fault lift deferred to the
        # consumer that drains it — the same STREAM-arm convention `transport/roots#RESOURCE` holds, the
        # generator's own `with` owning the HDF5 `TimeSeriesReader.__exit__` close on exhaustion or break.
        def run() -> Frames:
            reader = meshio.xdmf.TimeSeriesReader(str(ref.path))
            reader.read_points_cells()
            return _frames(reader)

        with _TRACER.start_as_current_span("mesh.timeseries"):
            return boundary("mesh.timeseries", run, catch=(meshio.ReadError, meshio.WriteError))

    def preview(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return self._emit(ref, out, "glb", "mesh.preview")

    def write(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return self._emit(ref, out, out.path.suffix.lstrip("."), "mesh.write")

    def contribute(self) -> Iterator[Receipt]:
        return MeshReceipt(
            self.backend.tag, self.content_key, self.point_count, self.cell_blocks, self.point_arrays, self.cell_arrays, self.field_arrays, self.units
        ).contribute()

    @classmethod
    def _build(cls, backend: MeshBackend, frame: MeshFrame, units: str) -> "MeshPayload":
        return cls(backend, frame.points, frame.point_count, frame.cell_blocks, frame.point_arrays, frame.cell_arrays, frame.field_arrays, units)

    def _emit(self, ref: ResourceRef, out: ResourceRef, fmt: str, subject: str) -> "RuntimeRail[ContentKey]":
        row = self.backend.row

        def run() -> RuntimeRail[ContentKey]:
            row.export(row.load(ref), out, fmt)
            return ContentIdentity.of("mesh.export", out.path.read_bytes())

        with _TRACER.start_as_current_span(subject, attributes={"rasm.mesh.backend": self.backend.tag, "rasm.mesh.format": fmt}):
            return boundary(subject, run, catch=row.fault).bind(lambda rail: rail)


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

- Owner: `PointCloud` — the LAS/LAZ/COPC interchange row over `laspy` alone: `laspy.read`/`LasData.write` for the full-file LAS/LAZ decode/encode and `laspy.copc.CopcReader` for the octree-chunked spatial subset over local paths and HTTP URLs. `PointBounds` is the axis-aligned octree query box folding directly into a fully-3D `laspy.copc.Bounds`, its `minz`/`maxz` defaulting to `±inf` so an unset Z box admits every depth — the catalogue "2D bounds skip Z filtering" outcome without the `Bounds.ensure_3d` 2D-promotion call, a pure no-op over an already-3D `mins`/`maxs` pair; `CopcQuery` is the closed tagged union carrying the mutually-exclusive `bounds`/`resolution`/`level` query modalities the catalogue `query` law fixes. A `Selection` (`laspy.DecompressionSelection | None`) threads the catalogue-confirmed `decompression_selection` keyword identically through `laspy.read` and `CopcReader.open` (the `query` LOD call carrying only `bounds`/`resolution`/`level`, the decode-field mask fixed once at `open`), so a cloud subset skips RGB/GPS-time/extra-byte fields it never reads — one optional carrier on every entry, never a parallel selective-read method. The point records cross to the geometry scan companion through one polymorphic `_to_arrow` dimension fold serving both the full `laspy.LasData` read and the `ScaleAwarePointRecord` subset — the `LasData.xyz` coordinate stack columnated as `x`/`y`/`z` plus every `PointFormat.dimension_names` column off one iteration through the one shared `_column` builder this row imports from the `[02]-[MESH]` fence (folding a 1D dimension as `pyarrow.array` and a multi-dim dimension as `pyarrow.FixedSizeListArray.from_arrays` over the flat NumPy buffer rather than a `.tolist()` row materialization) — keyed by `ContentIdentity` over the coordinate bytes, never a `laspy`- or `pdal`-specific object and never a re-spelled column fold. The single `laspy` owner replaces the former `laspy`/`pdal` two-backend split — the data COPC arm is `laspy.copc`, and the geometry `pdal` filter-graph stays geometry-owned. `PointRecordTable` is the typed bridge receipt — point count, point-format id, CRS WKT, content key — satisfying the runtime `ReceiptContributor` Protocol so the columnar table crosses the seam content-keyed; `PointCloud` carries the same point-format id plus CRS WKT directly on the frozen owner, never a second `PointFormat` struct duplicating the LAS format id the receipt already names.
- Entry: every entrypoint opens one `_TRACER.start_as_current_span(f"pointcloud.<op>")` around its fence so each point-cloud op is an OTel span the runtime `boundary`/`async_boundary` `_convert` records the terminal raise on. `PointCloud.read` admits a LAS/LAZ/COPC `ResourceRef` plus an optional `Selection` and returns the frozen owner keyed by `ContentIdentity` over the `LasData.xyz` coordinate bytes, carrying the LAS point-format id and the CRS WKT recovered from `LasHeader.parse_crs`; `PointCloud.subset` is the awaitable COPC octree-subset leg over a `CopcQuery` plus `Selection` through `laspy.copc.CopcReader.open(...).query(...)` rather than a full read — a `http://`/`https://` source drives the blocking eager `open`-plus-`query` sequence through `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` so a transient `429`/`5xx`/`TimeoutError`/`ConnectionError` over the COPC header/chunk fetch retries on the one `stamina` `HTTP` `Retry-After`-honouring policy and surfaces through `async_boundary` exactly once, while a local-path source threads the same blocking body through `anyio.to_thread.run_sync` off the event loop with no retry budget — never a hand-opened retry loop and never a blocking `CopcReader.open` left on the event loop, returning the content-keyed `PointRecordTable` carrying the CRS WKT recovered from the opened `CopcReader.header.parse_crs` so the octree subset keeps the COPC header's declared CRS rather than dropping it to an empty string; `PointCloud.to_arrow` folds the `laspy.LasData` dimensions under the same `Selection` into one `PointRecordTable` columnar bridge; `PointCloud.write` round-trips LAS/LAZ through `laspy.LasData.write` over the closed `WriteMode` discriminant routed through the one `_WRITE` dispatch table — `compress` to the LAS→LAZ transcode over the band-resolved `laz_backend`, `store` to a forced-uncompressed `do_compress=False` write, `preserve` to the format-preserving copy — one table row per mode rather than a `bool | None` `if do_compress`/`elif do_compress is not None` truthiness ladder that could collapse the explicit `store` write into the `preserve` path. Each synchronous entrypoint returns a `RuntimeRail` through a spanned `boundary` narrowing `catch=` to `laspy.LaspyException` and `bind`-flattens the `ContentIdentity.of`/`PointRecordTable.of` inner identity rail, and the awaitable `subset` returns a `RuntimeRail` through the `guarded` envelope likewise `bind`-flattened, so a non-`laspy` exception escapes rather than converting to a point-cloud fault and an identity-derivation fault rides the same one carrier.
- Boundary: no geometry kernel registration, no scan-to-BIM compute (that is the geometry scan companion), no `pdal` filter-graph (SMRF/PMF/outlier/decimation/range stays geometry-owned), no host coupling; the point-cloud row is host-free file exchange feeding the geometry companion at the wire (content-identity plus the columnar bridge), never a managed-interior coupling; a hand-rolled LAS parser, a hand-rolled `column_stack` where `LasData.xyz` answers, a `_record_to_arrow`/`_las_to_arrow` twin beside the one `_to_arrow` fold, a re-spelled `_column` fold where the shared mesh-fence builder is imported, a second `PointFormat` struct duplicating the format id the receipt and owner already carry, a `.tolist()` row materialization where `FixedSizeListArray.from_arrays` folds the multi-dim buffer, a `CopcCompression` StrEnum keyed on the unverified `are_points_compressed` probe, an un-narrowed `catch=Exception` boundary, an unspanned point-cloud fence, a blocking `CopcReader.open` left on the event loop or a hand-opened `for attempt in range(n): sleep(...)` retry loop where the remote `subset` rides `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)`, a four-positional `Receipt.of(phase, owner, subject, facts)` against the two-argument `(owner, evidence)` form, a `contribute` returning a single bare `Receipt` against the `Iterable[Receipt]` Protocol, a `# type: ignore`-swallowed `ContentIdentity.of`/`PointRecordTable.of` treated as a bare value where the derivation is the inner `RuntimeRail` the `.map`/`.bind` flatten threads, a `str()`-pre-formatted numeric receipt fact where the receipts `Encoder(enc_hook=repr)` renderer serializes the native scalar, a `getattr` over `LazBackend` name strings, a `_open_copc_compressed` twin beside the one `_laz_backend` selector, a local-leg `http_num_threads` literal restating the catalogue 80 default where the remote leg omits the keyword and only the local leg forces `1`, a `trace.get_tracer(__name__)` where the file resolves the explicit dotted module string, a `pdal`-specific object crossing the seam, a `pdal` module-top import, a `PointBounds.as_pdal` PDAL-pipeline string, a write that drops the `do_compress`/`laz_backend` transcode keywords, a `bool | None` `if do_compress`/`elif do_compress is not None` truthiness ladder collapsing the explicit `store` (`do_compress=False`) write into the `preserve` format-preserving path where the closed `WriteMode` `_WRITE` table separates `compress`/`store`/`preserve`, and a filename-keyed compression guess are the deleted forms.

```python signature
from collections.abc import Callable, Iterator
from typing import Final, Literal, assert_never

import anyio
import laspy
import laspy.copc
import numpy as np
import pyarrow as pa
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.spatial.mesh import _column
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef

_TRACER: Final = trace.get_tracer("rasm.data.spatial.mesh")

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


# The one point-cloud emitted-phase evidence both the content-keyed table and the frozen owner
# contribute, native scalars the receipts `Encoder(enc_hook=repr)` serializes without a `str()` coerce.
def _pointcloud_receipt(content_key: ContentKey, point_count: int, point_format: int, crs_wkt: str) -> Iterator[Receipt]:
    yield Receipt.of("pointcloud", ("emitted", content_key.hex, {"points": point_count, "format": point_format, "crs": crs_wkt}))


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

        with _TRACER.start_as_current_span("pointcloud.subset", attributes={"rasm.pointcloud.remote": remote}):
            railed_rail = (
                await guarded(RetryClass.HTTP, anyio.to_thread.run_sync, run, subject="pointcloud.subset")
                if remote
                else await async_boundary("pointcloud.subset", lambda: anyio.to_thread.run_sync(run), catch=laspy.LaspyException)
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
    from laspy import LazBackend  # noqa: PLC0415

    backend = next((b for b in (LazBackend.LazrsParallel, LazBackend.Lazrs, LazBackend.Laszip) if b.is_available()), None)
    if backend is None:
        raise laspy.LaspyException("compressed LAZ/COPC requires lazrs or laszip on the worker lane")
    return backend


# the `do_compress` tri-state is a closed `WriteMode` vocabulary, not a `bool | None` truthiness
# fork: `compress` transcodes LAS->LAZ over the band-resolved `LazBackend`, `store` forces an
# explicit uncompressed write, and `preserve` round-trips the source's own format — each one row,
# never an `if do_compress`/`elif do_compress is not None` ladder collapsing the `store` write
# into the `preserve` path.
_WRITE: Final[Map[WriteMode, Callable[["laspy.LasData", str], None]]] = Map.of_seq([
    ("compress", lambda data, dst: data.write(dst, do_compress=True, laz_backend=_laz_backend())),
    ("store", lambda data, dst: data.write(dst, do_compress=False)),
    ("preserve", lambda data, dst: data.write(dst)),
])


def _open_copc(path: str, selection: Selection) -> "laspy.copc.CopcReader":
    # the remote leg takes the catalogue `http_num_threads=80` default by omission (never a local
    # constant restating it); only the local leg forces `1` to serialize the single-file read.
    threads: dict[str, object] = {} if path.startswith(("http://", "https://")) else {"http_num_threads": 1}
    selected: dict[str, object] = {"decompression_selection": selection} if selection is not None else {}
    return laspy.copc.CopcReader.open(path, **threads, **selected)
```

## [04]-[RESEARCH]

- [RUNTIME_RAILS]: the per-op OTel span and the remote-COPC resilience envelope compose existing runtime owners verbatim. `_TRACER.start_as_current_span(f"<owner>.<op>")` around every `boundary`/`async_boundary` fence is the per-op span pattern `tabular/egress#EGRESS` (`each run/run_async opens one _TRACER.start_as_current_span("egress.{tag}")`, naming an unspanned op a deleted form), `spatial/geospatial#GEO`, and `spatial/catalog#CATALOG` already establish over `opentelemetry-api` `trace.get_tracer`/`Tracer.start_as_current_span`; the faults owner's `_convert` records the terminal raise on whatever span is active (`reliability/faults#FAULT`), so the span is the seam the fault becomes trace-visible through without the data page minting a second tracer. The remote `subset` envelope is `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, run, subject="pointcloud.subset")` — `RetryClass.HTTP` carries the `Retry-After`-honouring `BackoffHook` over the `(TimeoutError, ConnectionError)` transient set (`reliability/resilience#RESILIENCE` `POLICY` `http` row), `guarded` fuses the retry span plus the `async_boundary` lift exactly once, and `anyio.to_thread.run_sync` offloads the blocking `CopcReader.open`-plus-`query` body off the event loop (`.api/anyio.md` thread-offload [01]) — the identical retry/span/lift triplet `spatial/catalog#CATALOG`'s `discover` (`guarded(RetryClass.HTTP, anyio.to_thread.run_sync, _discover, subject="stac.discover")`) and `spatial/geospatial#GEO`'s `apply_remote` delegate to, so the COPC HTTP read joins the one resilience rail rather than a hand-opened loop. The `Receipt.of("<owner>", ("emitted", subject, facts))` two-argument `(owner, evidence)` shape and the native-scalar facts map are the `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` factory and its `Encoder(enc_hook=repr)` renderer, which name the four-positional `Receipt.of(phase, owner, subject, facts)` call and a `str()`-pre-formatted numeric fact as deleted forms — the prior fences committed both, now corrected. The named-array egress rides the `tabular/columnar#COLUMNAR` `QueryReceipt.railed(engine, source, table)` rail (`from rasm.data.tabular.columnar import QueryReceipt`), the same cross-page Arrow-table receipt seam `spatial/geospatial#GEO` imports at its `SpatialEngine`/raster/grid sites — content-keyed over the canonical Arrow bytes through the columnar owner's `arrow_bytes` (`table.combine_chunks().to_batches()[0].serialize()`), so the mesh `arrays` table egresses with the identical `(pa.Table, QueryReceipt)` shape every data-branch Arrow producer holds rather than a bare un-keyed table, and the prior per-array `ContentKey` derivation over the once-dropped buffers — a `traversed` fold whose keys no consumer ever read — is deleted as dead read-path work.

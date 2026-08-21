# [PY_DATA_GRID]

The discrete-global-grid owner, split out of the geospatial claims plane so the DGG plane is governance-visible: `GridSystem` folds cell indexing, measurement, traversal, and hierarchy as vectorized polars expressions over `h3ronpy` Arrow cell ops, with the `polars-st` frame-native geometry vocabulary beside the cell ops so one frame carries geometry AND cells in one vectorized engine. Grid is the terminal tier — it imports `spatial/query` and `tabular/columnar` and is imported by nothing.

The two-H3-substrate boundary is law: in-frame vectorized cell algebra lives here, in-DB binning on `spatial/query#SPATIAL`, and `engine_bin` composes that engine downward for columnar input. Cells, vertexes, and edges stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline; every run keys by `ContentIdentity` through the shared `columnar` `QueryReceipt`. `GridScheme.S2` is the standing deferred hold — `xarray-spatial`'s `numba` core is the blocker and numba cp315 the activation — so it holds no row on the served-scheme roster and `GridSystem.of` refuses it at construction.

## [01]-[INDEX]

- [02]-[GRID]: the `GridSystem` DGG owner — the one `GridRequest` plane axis behind `run`, the `GridOp` cell-algebra vocabulary, the `CellKind` collapse, the `raster` bridge, the `GeoLift`/`GeoFrameOp` frame-geometry vocabulary, the `engine_bin` in-DB composition.

## [02]-[GRID]

- Owner: `GridSystem` — ONE entry `run(request)` over the `GridRequest` plane axis, whose `cells`/`lift`/`geometry` cases carry the `GridOp`, `GeoLift`, and `GeoFrameOp` vocabularies with exactly the operands each plane takes, so the substrate a request runs on is recoverable from the value instead of from the method a caller picked; every frame-geometry verb is one `_GEO_VERB` row stating the operands it admits and resolved off the `.st` accessor by name, never a per-verb method family, and the `CellKind` axis routes the index-kind prefix once so the three parallel h3ronpy families never grow a parallel `GridOp` row each.
- Cases: `_plan` is the one dispatch minting a request's whole route — the step the span and receipt name, the kernel that ran it, the provider raise set that kernel spells, and the thunk — so no plane forks a second span, catch, or receipt site; `Metric.of_area(unit)` projects an `AreaUnit` into the matching area row, so there is no parallel `Area` sibling case.
- Entry: capability refuses at CONSTRUCTION, never inside the fold — `GridSystem.of` admits only a scheme the `_SCHEME_ENGINE` roster serves and hands the admitted system its kernel name as evidence, `GridOp.Boundary` proves its `(form, kind)` corner against the same `_BOUNDARY` roster the fold reads, `GridOp.Rasterize` refuses a non-positive extent, and the `GeoFrameOp` factories refuse through one `_admitted` gate whose comparison closes both halves of every operand corner — a distanceless `dwithin`, a distance on a geometry-only verb, a scalar handed to a nullary measure; the grid boundary then catches the h3ronpy FFI fault family per plane row, never an un-narrowed `Exception`.
- Auto: the geometry-to-cells leg reads the polars-st WKB `GeoExpr` column directly, so the DGGS index shares the one WKB encoding the `spatial/geospatial#GEO` claims and the `spatial/query#SPATIAL` engine speak; a raster nodata is the CALLER's declaration on the raster factories — the ingress carries `Option[float]` whose `Nothing` masks no pixel and the egress carries a required fill, because the provider's own `nodata_value=0` default writes a real zero over unwritten pixels and a scene holding genuine zeros then cannot tell fill from measurement; `set_failing_to_invalid` keeps array length stable on parse failure, so an invalid cell is a null data row, never a raised exception in the array pipeline; `H3_CRS` and `DEFAULT_CELL_COLUMN` are page-owned anchors with one declaration site, never a per-arm literal; every `arro3` return crosses into polars through `pl.from_arrow` over the Arrow PyCapsule interface, never a positional `pl.Series(name, array)` intake.
- Receipt: the shared `tabular/columnar` `QueryReceipt.railed` over the result frame, the `engine` carrying the route the plan row named — the scheme's own kernel for a cell request, `polars-st` for the frame-geometry planes; `GridResult` pairs the frame with that receipt, no new receipt rail.
- Packages: `h3ronpy` and `polars-st` ride the Forge scientific source build band and bind in-process, never across a subprocess seam — each declares ONE module-scope `lazy import`/`lazy from` line and reifies on the first cell operation, so the compiled band costs an unrelated import nothing while the eager module-level form the manifest bans never appears; the `_BOUNDARY` row and the `Containment` mode carry member NAMES resolved at the call seam, because a module-scope cell over a live kernel attribute reifies the whole band at import. The polars-st LGPL-2.1 dynamic-linkage posture stays recorded on `data/.api/polars-st.md`.
- Growth: a new cell operation is one `GridOp` case; a new request plane is one `GridRequest` case with one `_plan` arm carrying its route, rostered row, catch set, and thunk; a new refusal law is one `FaultRow` row under `DataLeg.GRID` in this module's one `RAISES` table; a new index kind one `CellKind` row; a new scalar metric one `Metric`/`AreaUnit` row; a new coverage policy one `ContainmentMode` row; a new cell egress one `_BOUNDARY` row the construction gate reads free; a new frame-geometry verb one `_GEO_VERB` row beside its literal, the gate and the fold reading it free; a new grid scheme one `GridScheme` member and the `_SCHEME_ENGINE` row that serves it.
- Boundary: no host coupling, no durable cell store, no lonboard/GeoArrow visualization (`artifacts` owns it); the claims plane is `spatial/geospatial#GEO`, the in-DB engine `spatial/query#SPATIAL`, and never a second WKB geometry encoding or a parallel H3 column owner beside them.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

# the compiled DGG band declares once here and reifies on first cell operation: the bare module bindings serve the
# dynamic `getattr` seams (`{kind}_valid`, the `_BOUNDARY` member NAME), the `lazy from` lists the statically
# named kernels the `_grid` arms call. Every module-scope row over this band carries a NAME or a thunk — a cell
# holding a live kernel attribute would reify the whole native band at import.
lazy import h3ronpy
lazy import h3ronpy.vector as vector
lazy import polars as pl
lazy import polars_st as st
lazy from h3ronpy import (
    ContainmentMode,
    cells_to_localij,
    change_resolution,
    change_resolution_list,
    change_resolution_paired,
    compact,
    grid_disk,
    grid_disk_aggregate_k,
    grid_disk_distances,
    grid_ring_distances,
    localij_to_cells,
    uncompact,
)
lazy from h3ronpy.raster import nearest_h3_resolution, raster_to_dataframe, rasterize_cells
lazy from h3ronpy.vector import cells_bounds_arrays, coordinates_to_cells, geometry_to_cells, wkb_to_cells

from rasm.data.spatial.query import SpatialEngine, SpatialQuery, SpatialResult
from rasm.data.tabular.columnar import QueryReceipt
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped

if TYPE_CHECKING:
    import numpy as np
    import pyarrow as pa

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.grid")

# this module's whole raise roster: every construction corner and every fenced kernel on this page resolves ONE
# anchor here, so no call site spells a subject and `FaultRow.seated` proves the leg against a real module at import.
# The four construction corners are caller-repairable and TERMINAL — an operand set, an unserved egress pair, a
# zero extent, and a scheme whose kernel has no wheel all refuse identically on a re-offer. The `cells` and
# `geometry` kernels are pure in-frame folds and declare the same; only `lift` declares TRANSIENT, its `file` arm
# reading a source a re-issue may clear. `slots` NAMES each corner's coordinates, so the free-string message bodies
# these rows replace become fields a consumer gates on rather than prose it parses.
GRID_ARITY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="geo.arity", arm="config", defect="operand-mismatch", retriability=TERMINAL,
    slots=("verb", "demanded", "supplied"),
)
GRID_EGRESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="boundary", arm="config", defect="no-cell-egress", retriability=TERMINAL, slots=("form", "kind")
)
GRID_EXTENT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="rasterize", arm="config", defect="non-positive-extent", retriability=TERMINAL, slots=("extent",)
)
# the scheme gate rides `import_` because the defect IS an absent module: the `xarray-spatial` numba core has no
# cp315 wheel, so the row this scheme needs is unreachable rather than merely unconfigured.
GRID_SCHEME: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="scheme", arm="import_", defect="scheme-unreached", retriability=TERMINAL, slots=("scheme",)
)
GRID_CELLS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="cells", arm="boundary", defect="cell-op", retriability=TERMINAL
)
GRID_LIFT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="lift", arm="boundary", defect="lift-refused", retriability=TRANSIENT
)
GRID_GEOMETRY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRID, point="geometry", arm="boundary", defect="geo-op", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    GRID_ARITY,
    GRID_EGRESS,
    GRID_EXTENT,
    GRID_SCHEME,
    GRID_CELLS,
    GRID_LIFT,
    GRID_GEOMETRY,
]))


type ResolutionShape = Literal["single", "list", "paired"]
type DiskMode = Literal["cells", "distances", "aggregate"]
type Aggregation = Literal["min", "max"]
type CompactDirection = Literal["compact", "uncompact"]
type BoundaryForm = Literal["polygon", "point", "centroid", "linestring"]
type CellEgress = Literal["column", "frame"]
type IjDirection = Literal["to_localij", "from_localij"]
type ValidateMode = Literal["valid", "parse", "format"]
type HierarchyDirection = Literal["parent", "children"]
type GeoPredicate = Literal["intersects", "contains", "within", "dwithin", "covers"]
type GeoMeasure = Literal["area", "length", "distance"]
type GeoShape = Literal["buffer", "simplify", "centroid", "convex_hull"]
type JoinHow = Literal["inner", "left", "right"]
type GeoArity = Literal["nullary", "column", "scalar", "column_scalar"]
type GridPlane = Literal["cells", "lift", "geometry"]

H3_CRS: Final[str] = "EPSG:4326"
DEFAULT_CELL_COLUMN: Final[str] = "cell"


class GridScheme(StrEnum):
    H3 = "h3"
    S2 = "s2"


class CellKind(StrEnum):
    CELL = "cells"
    VERTEX = "vertexes"
    EDGE = "directededges"


class AreaUnit(StrEnum):
    KM2 = "km2"
    M2 = "m2"
    RADS2 = "rads2"


class Metric(StrEnum):
    AREA_KM2 = "cells_area_km2"
    AREA_M2 = "cells_area_m2"
    AREA_RADS2 = "cells_area_rads2"
    RESOLUTION = "cells_resolution"

    @staticmethod
    def of_area(unit: AreaUnit) -> "Metric":
        return Metric[f"AREA_{unit.name}"]


class Containment(StrEnum):
    CENTROID = "ContainsCentroid"
    BOUNDARY = "ContainsBoundary"
    COVERS = "Covers"
    INTERSECTS = "IntersectsBoundary"

    def mode(self) -> object:
        # the StrEnum value MIRRORS the provider member name, so the mode resolves at the call seam and no
        # module-scope row ever holds a live `ContainmentMode` member.
        return getattr(ContainmentMode, self.value)


@tagged_union(frozen=True)
class CellSource:
    tag: Literal["coordinates", "wkb", "geometry"] = tag()
    coordinates: tuple[str, str] = case()
    wkb: tuple[str, Containment] = case()
    geometry: tuple[str, Containment] = case()

    @staticmethod
    def Coordinates(lat_col: str, lng_col: str) -> "CellSource":
        return CellSource(coordinates=(lat_col, lng_col))

    @staticmethod
    def Wkb(geometry_col: str = "geometry", containment: Containment = Containment.CENTROID) -> "CellSource":
        return CellSource(wkb=(geometry_col, containment))

    @staticmethod
    def Geometry(geometry_col: str = "geometry", containment: Containment = Containment.CENTROID) -> "CellSource":
        return CellSource(geometry=(geometry_col, containment))


@tagged_union(frozen=True)
class GeoLift:
    tag: Literal["wkb", "shapely", "geopandas", "file"] = tag()
    wkb: str = case()
    shapely: str = case()
    geopandas: object = case()
    file: str = case()


# the ONE operand-arity correspondence over the `.st` verbs this page speaks, each row reading the accessor
# signature `data/.api/polars-st.md` declares — nullary measures and derived geometries (:120, :123), the binary
# predicates and `distance(other)` (:71, :78), the scalar-bearing `buffer`/`simplify` (:90, :94), and the one
# `dwithin(other, distance)` row (:81). The factories gate on it and `_geo` lowers each carrier straight into the
# call, so no arm re-decides an arity and no operand slot stands open for a verb that cannot take it.
_GEO_VERB: Final[Map[str, GeoArity]] = Map.of_seq([
    ("area", "nullary"),
    ("length", "nullary"),
    ("centroid", "nullary"),
    ("convex_hull", "nullary"),
    ("intersects", "column"),
    ("contains", "column"),
    ("within", "column"),
    ("covers", "column"),
    ("distance", "column"),
    ("buffer", "scalar"),
    ("simplify", "scalar"),
    ("dwithin", "column_scalar"),
])


def _admitted(verb: str, *, column: bool, scalar: bool) -> "RuntimeRail[str]":
    # ONE gate for every frame-geometry factory: the operand set the caller supplied must EQUAL the set the verb's
    # row admits, so a distanceless `dwithin` and a distance handed to a geometry-only verb refuse in one
    # comparison, at construction, where the caller can still repair them.
    supplied: GeoArity = "column_scalar" if column and scalar else "column" if column else "scalar" if scalar else "nullary"
    demanded = _GEO_VERB[verb]
    return (
        Ok(verb)
        if supplied == demanded
        else Error(GRID_ARITY.raised(verb, demanded, supplied))
    )


@tagged_union(frozen=True)
class GeoFrameOp:
    tag: Literal["predicate", "measure", "shape", "sjoin"] = tag()
    predicate: tuple[GeoPredicate, str, str, Option[float]] = case()
    measure: tuple[GeoMeasure, str, Option[str]] = case()
    shape: tuple[GeoShape, str, Option[float]] = case()
    sjoin: tuple[object, GeoPredicate, JoinHow, Option[float]] = case()

    @staticmethod
    def Predicate(verb: GeoPredicate, geometry_col: str, other_col: str, distance: Option[float] = Nothing) -> "RuntimeRail[GeoFrameOp]":
        return _admitted(verb, column=True, scalar=distance.is_some()).map(
            lambda _row: GeoFrameOp(predicate=(verb, geometry_col, other_col, distance))
        )

    @staticmethod
    def Measure(verb: GeoMeasure, geometry_col: str, operand_col: Option[str] = Nothing) -> "RuntimeRail[GeoFrameOp]":
        return _admitted(verb, column=operand_col.is_some(), scalar=False).map(
            lambda _row: GeoFrameOp(measure=(verb, geometry_col, operand_col))
        )

    @staticmethod
    def Shape(verb: GeoShape, geometry_col: str, param: Option[float] = Nothing) -> "RuntimeRail[GeoFrameOp]":
        return _admitted(verb, column=False, scalar=param.is_some()).map(
            lambda _row: GeoFrameOp(shape=(verb, geometry_col, param))
        )

    @staticmethod
    def Sjoin(
        other: object, predicate: GeoPredicate = "intersects", how: JoinHow = "inner", distance: Option[float] = Nothing
    ) -> "RuntimeRail[GeoFrameOp]":
        # the join carries the frame as its column operand, so the same row that gates an expression predicate
        # gates the join one: `sjoin(predicate="dwithin")` without a distance is the provider corner it closes.
        return _admitted(predicate, column=True, scalar=distance.is_some()).map(
            lambda _row: GeoFrameOp(sjoin=(other, predicate, how, distance))
        )


# the data-valued (form, kind) -> vector-member correspondence on the folder's ONE Map rail, and the SOLE legal-corner
# roster: `GridOp.Boundary` admits through it and `_boundary` reads the row it admitted, so a gate and a fold cannot
# disagree about which pair the kernel serves. The egress column states the member's own arity — a single-column WKB
# result attaches to the caller's frame, the multi-column centroid batch derives its own — and `centroid` seats here
# under `CellKind.CELL` alone, the vertex and edge kinds having no coordinate kernel to answer them.
_BOUNDARY: Final[Map[tuple[BoundaryForm, CellKind], tuple[str, CellEgress]]] = Map.of_seq([
    (("polygon", CellKind.CELL), ("cells_to_wkb_polygons", "column")),
    (("point", CellKind.CELL), ("cells_to_wkb_points", "column")),
    (("point", CellKind.VERTEX), ("vertexes_to_wkb_points", "column")),
    (("linestring", CellKind.EDGE), ("directededges_to_wkb_linestrings", "column")),
    (("centroid", CellKind.CELL), ("cells_to_coordinates", "frame")),
])


@tagged_union(frozen=True)
class GridOp:
    tag: Literal[
        "index", "resolution", "disk", "ring", "measure", "bounds", "compact",
        "boundary", "local_ij", "validate", "raster_index", "raster_egress", "hierarchy",
    ] = tag()
    index: tuple[int, CellSource] = case()
    resolution: tuple[int, ResolutionShape] = case()
    disk: tuple[int, DiskMode, Aggregation] = case()
    ring: tuple[int, int] = case()
    measure: Metric = case()
    bounds: bool = case()
    compact: tuple[CompactDirection, int] = case()
    boundary: tuple[BoundaryForm, CellKind, bool] = case()
    local_ij: tuple[int, IjDirection] = case()
    validate: tuple[ValidateMode, CellKind] = case()
    raster_index: tuple["np.ndarray", tuple[float, ...], Option[float], Option[int]] = case()
    raster_egress: tuple["np.ndarray", int | tuple[int, int], float] = case()
    hierarchy: tuple[HierarchyDirection, int] = case()

    @staticmethod
    def Index(resolution: int, source: CellSource) -> "GridOp":
        return GridOp(index=(resolution, source))

    @staticmethod
    def Resolution(target: int, shape: ResolutionShape = "single") -> "GridOp":
        return GridOp(resolution=(target, shape))

    @staticmethod
    def Disk(k: int, mode: DiskMode = "cells", aggregation: Aggregation = "max") -> "GridOp":
        return GridOp(disk=(k, mode, aggregation))

    @staticmethod
    def Ring(k_min: int, k_max: int) -> "GridOp":
        return GridOp(ring=(k_min, k_max))

    @staticmethod
    def Measure(metric: Metric = Metric.AREA_KM2) -> "GridOp":
        return GridOp(measure=metric)

    @staticmethod
    def Bounds() -> "GridOp":
        return GridOp(bounds=True)

    @staticmethod
    def Compact(direction: CompactDirection = "compact", target: int = 0) -> "GridOp":
        return GridOp(compact=(direction, target))

    @staticmethod
    def Boundary(form: BoundaryForm = "polygon", kind: CellKind = CellKind.CELL, radians: bool = False) -> "RuntimeRail[GridOp]":
        # the corner refuses HERE, where the caller can still repair it: an unserved (form, kind) pair never reaches
        # the fold, so `_boundary` holds no refusal arm and no mis-applied `assert_never` stands in for one.
        return _BOUNDARY.try_find((form, kind)).to_result_with(
            lambda: GRID_EGRESS.raised(form, kind.value)
        ).map(lambda _row: GridOp(boundary=(form, kind, radians)))

    @staticmethod
    def LocalIj(anchor: int, direction: IjDirection = "to_localij") -> "GridOp":
        return GridOp(local_ij=(anchor, direction))

    @staticmethod
    def Validate(mode: ValidateMode = "valid", kind: CellKind = CellKind.CELL) -> "GridOp":
        return GridOp(validate=(mode, kind))

    @staticmethod
    def Raster(values: "np.ndarray", transform: tuple[float, ...], nodata: Option[float], resolution: Option[int] = Nothing) -> "GridOp":
        # `nodata` takes no default: the caller DECLARES the pixel value its scene reserves, or declares `Nothing`
        # and masks none. `resolution` defaults absent because the raster's own shape and transform derive it.
        return GridOp(raster_index=(values, transform, nodata, resolution))

    @staticmethod
    def Rasterize(values: "np.ndarray", size: int | tuple[int, int], nodata: float) -> "RuntimeRail[GridOp]":
        # a zero-extent raster is the fabricated-size outcome this refusal deletes; the size itself is required, so
        # an absent one is unrepresentable rather than substituted, and `nodata` is the fill the egress must state.
        extent = size if isinstance(size, tuple) else (size, size)
        return (
            Ok(GridOp(raster_egress=(values, size, nodata)))
            if min(extent) > 0
            else Error(GRID_EXTENT.raised("x".join(map(str, extent))))
        )

    @staticmethod
    def Hierarchy(direction: HierarchyDirection = "parent", target: int = 0) -> "GridOp":
        return GridOp(hierarchy=(direction, target))


@tagged_union(frozen=True)
class GridRequest:
    # ONE closed request axis behind ONE entry: the plane a request runs on rides the value, so the cell kernel, the
    # geometry ingress, and the frame-geometry verbs no longer split `GridSystem` into three public entrypoints whose
    # only discriminant was the method a caller picked. Each case carries exactly its own operands — a lift takes no
    # frame because it MINTS one, so the frameless corner is unrepresentable rather than gated.
    tag: GridPlane = tag()
    cells: tuple[GridOp, "pl.DataFrame"] = case()
    lift: GeoLift = case()
    geometry: tuple[GeoFrameOp, "pl.DataFrame"] = case()

    @staticmethod
    def Cells(op: GridOp, frame: "pl.DataFrame") -> "GridRequest":
        return GridRequest(cells=(op, frame))

    @staticmethod
    def Lift(lift: GeoLift) -> "GridRequest":
        return GridRequest(lift=lift)

    @staticmethod
    def Geometry(op: GeoFrameOp, frame: "pl.DataFrame") -> "GridRequest":
        return GridRequest(geometry=(op, frame))


class GridResult(Struct, frozen=True):
    frame: "pl.DataFrame"
    receipt: QueryReceipt


class _Plan(Struct, frozen=True):
    # the per-request route as one value: what the span and receipt name it, which kernel ran it, the rostered row
    # that kernel refuses under, that kernel's own raise surface, and the work. Three entrypoints carried these
    # facts as copied literal sets. `at` joins them for the same reason `catch` did — the request's own arm knows
    # which leg it is, so the one lift below spells neither a subject nor a provider set of its own.
    step: str
    engine: str
    at: "FaultRow[DataLeg]"
    catch: Catch
    work: Callable[[], "pl.DataFrame"]


# the served-scheme roster IS the capability gate AND the receipt route: `GridSystem.of` admits a scheme only where a
# row names the kernel serving it, and that same row spells the engine every cell receipt carries, so no second table
# states which scheme runs. `GridScheme.S2` holds no row — the numba blocker on `xarray-spatial` is the standing
# deferral — so the refusal lands at construction with its reason, not as a raise five frames inside the fold.
_SCHEME_ENGINE: Final[Map[GridScheme, str]] = Map.of_seq([(GridScheme.H3, "h3ronpy.h3")])
_FRAME_ENGINE: Final[str] = "polars-st"


class GridSystem(Struct, frozen=True):
    engine: str
    scheme: GridScheme = GridScheme.H3
    cell_column: str = DEFAULT_CELL_COLUMN
    crs: str = H3_CRS

    @classmethod
    def of(
        cls, scheme: GridScheme = GridScheme.H3, cell_column: str = DEFAULT_CELL_COLUMN, crs: str = H3_CRS
    ) -> "RuntimeRail[GridSystem]":
        # one admission gate over the served roster; the admitted system then CARRIES its kernel name as evidence, so
        # nothing below re-tests the scheme and `engine` has no default a bare construction could forge.
        return _SCHEME_ENGINE.try_find(scheme).to_result_with(
            lambda: GRID_SCHEME.raised(scheme.value)
        ).map(lambda engine: cls(engine=engine, scheme=scheme, cell_column=cell_column, crs=crs))

    def run(self, request: GridRequest) -> "RuntimeRail[GridResult]":
        plan = self._plan(request)
        subject = f"spatial.grid.{self.scheme.value}.{request.tag}.{plan.step}"
        with _TRACER.start_as_current_span(
            subject, attributes={"rasm.geo.scheme": self.scheme.value, "rasm.geo.plane": request.tag, "rasm.geo.op": plan.step}
        ):
            return boundary(plan.at, plan.work, catch=plan.catch).bind(
                lambda frame: QueryReceipt.railed(plan.engine, plan.step, frame.to_arrow()).map(
                    lambda receipt: GridResult(frame=frame, receipt=receipt)
                )
            )

    def engine_bin(self, table: "pa.Table", geometry_view: str, resolution: int) -> "RuntimeRail[SpatialResult]":
        # the in-DB half of the two-substrate law: an already-columnar frame bins through the
        # spatial/query engine's `h3` extension SQL; the in-frame half is the h3ronpy plane above.
        return SpatialEngine.of({geometry_view: table}).run(SpatialQuery.H3Bin(geometry_view, resolution))

    def _plan(self, request: GridRequest) -> _Plan:
        match request:
            case GridRequest(tag="cells", cells=(op, frame)):
                # `h3ronpy` publishes no exception class of its own: an out-of-range resolution, a malformed cell,
                # and an unlowerable array all raise `ValueError`, an absent column `KeyError`, and the Rust core
                # surfaces `RuntimeError`. `polars`/`polars_st` answer the same builtins, the `file` arm adding
                # `OSError` for the source read no other arm performs.
                return _Plan(
                    step=op.tag, engine=self.engine, at=GRID_CELLS, catch=(ValueError, KeyError, RuntimeError),
                    work=lambda: self._grid(op, frame),
                )
            case GridRequest(tag="lift", lift=lift):
                return _Plan(step=lift.tag, engine=_FRAME_ENGINE, at=GRID_LIFT, catch=(ValueError, OSError), work=lambda: _lift(lift))
            case GridRequest(tag="geometry", geometry=(op, frame)):
                return _Plan(step=op.tag, engine=_FRAME_ENGINE, at=GRID_GEOMETRY, catch=(ValueError, KeyError), work=lambda: _geo(op, frame))
            case unreachable:
                assert_never(unreachable)

    def _grid(self, op: GridOp, frame: "pl.DataFrame") -> "pl.DataFrame":
        def attach(name: str, array: object) -> "pl.DataFrame":
            return frame.with_columns(pl.from_arrow(array).rename(name))

        def derive(name: str, array: object) -> "pl.DataFrame":
            return pl.from_arrow(array).rename(name).to_frame()

        cells = frame[self.cell_column] if op.tag not in {"index", "raster_index"} else None
        match op:
            case GridOp(tag="index", index=(resolution, source)):
                return attach(self.cell_column, self._index(frame, resolution, source))
            case GridOp(tag="resolution", resolution=(target, "single")):
                return attach(self.cell_column, change_resolution(cells, target))
            case GridOp(tag="resolution", resolution=(target, "list")):
                return attach("children", change_resolution_list(cells, target))
            case GridOp(tag="resolution", resolution=(target, "paired")):
                return pl.from_arrow(change_resolution_paired(cells, target))
            case GridOp(tag="disk", disk=(k, "cells", _)):
                return attach("disk", grid_disk(cells, k, flatten=False))
            case GridOp(tag="disk", disk=(k, "distances", _)):
                return pl.from_arrow(grid_disk_distances(cells, k, flatten=False))
            case GridOp(tag="disk", disk=(k, "aggregate", aggregation)):
                return pl.from_arrow(grid_disk_aggregate_k(cells, k, aggregation))
            case GridOp(tag="ring", ring=(k_min, k_max)):
                return pl.from_arrow(grid_ring_distances(cells, k_min, k_max, flatten=False))
            case GridOp(tag="measure", measure=metric):
                return attach(metric.name.lower(), getattr(h3ronpy, metric.value)(cells))
            case GridOp(tag="bounds", bounds=True):
                return pl.from_arrow(cells_bounds_arrays(cells))
            case GridOp(tag="compact", compact=("compact", _)):
                return derive(self.cell_column, compact(cells, mixed_resolutions=False))
            case GridOp(tag="compact", compact=("uncompact", target)):
                return derive(self.cell_column, uncompact(cells, target))
            case GridOp(tag="boundary", boundary=(form, kind, radians)):
                return self._boundary(frame, cells, form, kind, radians)
            case GridOp(tag="local_ij", local_ij=(anchor, "to_localij")):
                return pl.from_arrow(cells_to_localij(cells, anchor, set_failing_to_invalid=True))
            case GridOp(tag="local_ij", local_ij=(anchor, "from_localij")):
                return attach(self.cell_column, localij_to_cells(anchor, frame["i"], frame["j"], set_failing_to_invalid=True))
            case GridOp(tag="validate", validate=(mode, kind)):
                return self._validate(frame, cells, mode, kind)
            case GridOp(tag="raster_index", raster_index=(values, transform, nodata, resolution)):
                return pl.from_arrow(self._raster_index(values, transform, nodata, resolution))
            case GridOp(tag="raster_egress", raster_egress=(values, size, nodata)):
                array, geotransform = self._raster_egress(cells, values, size, nodata)
                return pl.DataFrame({"raster": [array], "transform": [geotransform]})
            case GridOp(tag="hierarchy", hierarchy=("parent", target)):
                return attach(self.cell_column, change_resolution(cells, target))
            case GridOp(tag="hierarchy", hierarchy=("children", target)):
                return attach("children", change_resolution_list(cells, target))
            case unreachable:
                assert_never(unreachable)

    def _boundary(self, frame: "pl.DataFrame", cells: object, form: BoundaryForm, kind: CellKind, radians: bool) -> "pl.DataFrame":
        # total by construction: `GridOp.Boundary` admitted this pair through the same roster, so the read is a
        # lookup and never a refusal — the egress column decides whether the member's result attaches or derives.
        member, egress = _BOUNDARY[(form, kind)]
        egressed = pl.from_arrow(getattr(vector, member)(cells, radians=radians))
        return frame.with_columns(egressed.rename("boundary")) if egress == "column" else egressed

    def _validate(self, frame: "pl.DataFrame", cells: object, mode: ValidateMode, kind: CellKind) -> "pl.DataFrame":
        column, call = {
            "valid": ("valid", lambda: getattr(h3ronpy, f"{kind.value}_valid")(cells, booleanarray=True)),
            "parse": (self.cell_column, lambda: getattr(h3ronpy, f"{kind.value}_parse")(cells, set_failing_to_invalid=True)),
            "format": ("hex", lambda: getattr(h3ronpy, f"{kind.value}_to_string")(cells)),
        }[mode]
        return frame.with_columns(pl.from_arrow(call()).rename(column))

    def _raster_index(
        self, values: "np.ndarray", transform: tuple[float, ...], nodata: Option[float], resolution: Option[int]
    ) -> "pa.Table":
        # the LAST line naming the provider's own null: the declared `Nothing` lowers to `nodata_value=None`, which
        # masks nothing, so an undeclared reserve never becomes a real pixel value the ingress silently drops.
        return raster_to_dataframe(
            values,
            transform,
            resolution.default_with(lambda: nearest_h3_resolution(values.shape, transform)),
            nodata_value=nodata.to_optional(),
            compact=False,
        )

    def _raster_egress(
        self, cells: object, values: "np.ndarray", size: "int | tuple[int, int]", nodata: float
    ) -> "tuple[np.ndarray, tuple[float, ...]]":
        # the provider's own `nodata_value=0` default fills every unwritten pixel with a real measurement value, so
        # the fill rides the op as the caller's declaration and this seam never supplies one.
        return rasterize_cells(cells, values, size, nodata_value=nodata)

    def _index(self, frame: "pl.DataFrame", resolution: int, source: CellSource) -> object:
        match source:
            case CellSource(tag="coordinates", coordinates=(lat_col, lng_col)):
                return coordinates_to_cells(frame[lat_col], frame[lng_col], resolution, radians=False)
            case CellSource(tag="wkb", wkb=(geometry_col, containment)):
                # `flatten=False` keeps one per-row cell list so the `cell` column stays 1:1 with the
                # frame rows the `attach` `with_columns` requires; the exploded form is the engine
                # `H3Bin` leg on `spatial/query`.
                return wkb_to_cells(frame[geometry_col], resolution, containment_mode=containment.mode(), compact=False, flatten=False)
            case CellSource(tag="geometry", geometry=(geometry_col, containment)):
                return geometry_to_cells(frame[geometry_col], resolution, containment_mode=containment.mode(), compact=False)
            case unreachable:
                assert_never(unreachable)


def _lift(lift: GeoLift) -> "pl.DataFrame":
    match lift:
        case GeoLift(tag="wkb", wkb=col):
            return pl.select(st.from_wkb(pl.col(col)))
        case GeoLift(tag="shapely", shapely=col):
            return pl.select(st.from_shapely(pl.col(col)))
        case GeoLift(tag="geopandas", geopandas=frame):
            return st.from_geopandas(frame)
        case GeoLift(tag="file", file=path):
            return st.read_file(path)
        case unreachable:
            assert_never(unreachable)


def _geo(op: GeoFrameOp, frame: "pl.DataFrame") -> "pl.DataFrame":
    match op:
        case GeoFrameOp(tag="predicate", predicate=(verb, geometry_col, other_col, distance)):
            # each carrier lowers straight into the call — the option's own spread IS the argument list — because
            # the factory already settled which operands the verb takes; no arm re-tests a verb name.
            expr = getattr(pl.col(geometry_col).st, verb)(pl.col(other_col), *distance.to_list())
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="measure", measure=(verb, geometry_col, operand_col)):
            expr = getattr(pl.col(geometry_col).st, verb)(*operand_col.map(pl.col).to_list())
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="shape", shape=(verb, geometry_col, param)):
            expr = getattr(pl.col(geometry_col).st, verb)(*param.to_list())
            return frame.with_columns(expr.alias(geometry_col))
        case GeoFrameOp(tag="sjoin", sjoin=(other, predicate, how, distance)):
            # the LAST line naming the provider's own null: `sjoin` declares `distance=None` as its absent form,
            # and the factory proved the predicate admits the operand this lowering hands it.
            return frame.st.sjoin(other, predicate=predicate, how=how, distance=distance.to_optional())
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

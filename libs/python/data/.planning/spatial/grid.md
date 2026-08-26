# [PY_DATA_GRID]

The discrete-global-grid owner, split out of the geospatial claims plane so the DGG plane is governance-visible: `GridSystem` folds cell indexing, measurement, traversal, and hierarchy as vectorized polars expressions over `h3ronpy` Arrow cell ops, with the `polars-st` frame-native geometry vocabulary beside the cell ops so one frame carries geometry AND cells in one vectorized engine. Grid is the terminal tier — it imports `spatial/query` and is imported by nothing.

The two-H3-substrate boundary is law: in-frame vectorized cell algebra lives here, in-DB binning on `spatial/query#SPATIAL`, and `engine_bin` composes that engine downward for columnar input. Cells, vertexes, and edges stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline. `_SCHEME_ENGINE` admits the served schemes, and `GridSystem.of` refuses any absent row at construction.

## [01]-[INDEX]

- [02]-[GRID]: the `GridSystem` DGG owner — the one `GridRequest` plane axis behind `run`, the `GridOp` cell-algebra vocabulary, the `CellKind` collapse, the `raster` bridge, the `GeoLift`/`GeoFrameOp` frame-geometry vocabulary, the `engine_bin` in-DB composition.

## [02]-[GRID]

- Owner: `GridSystem` — ONE entry `run(request)` over the `GridRequest` plane axis, whose `cells`/`lift`/`geometry` cases carry the `GridOp`, `GeoLift`, and `GeoFrameOp` vocabularies with exactly the operands each plane takes, so the substrate a request runs on is recoverable from the value instead of from the method a caller picked; every frame-geometry verb is one `_GEO_VERB` row stating the operands it admits and resolved off the `.st` accessor by name, never a per-verb method family, and the `CellKind` axis routes the index-kind prefix once so the three parallel h3ronpy families never grow a parallel `GridOp` row each.
- Cases: `_plan` is the one dispatch minting a request's whole route — the step, the provider raise set that kernel spells, and the thunk — so no plane forks a second span or catch site; `Metric.of_area(unit)` projects an `AreaUnit` into the matching area row, so there is no parallel `Area` sibling case.
- Entry: capability refuses at CONSTRUCTION, never inside the fold — `GridSystem.of` admits only a scheme the `_SCHEME_ENGINE` roster serves and hands the admitted system its kernel name as evidence, `GridOp.Boundary` proves its `(form, kind)` corner against the same `_BOUNDARY` roster the fold reads, `GridOp.Rasterize` refuses a non-positive extent, and the `GeoFrameOp` factories refuse through one `_admitted` gate whose comparison closes both halves of every operand corner — a distanceless `dwithin`, a distance on a geometry-only verb, a scalar handed to a nullary measure; the grid boundary then catches the h3ronpy FFI fault family per plane row, never an un-narrowed `Exception`.
- Auto: the geometry-to-cells leg reads the polars-st WKB `GeoExpr` column directly, so the DGGS index shares the one WKB encoding the `spatial/geospatial#GEO` claims and the `spatial/query#SPATIAL` engine speak; a raster nodata is the CALLER's declaration on the raster factories — the ingress carries `Option[float]` whose `Nothing` masks no pixel and the egress carries a required fill, because the provider's own `nodata_value=0` default writes a real zero over unwritten pixels and a scene holding genuine zeros then cannot tell fill from measurement; `set_failing_to_invalid` keeps array length stable on parse failure, so an invalid cell is a null data row, never a raised exception in the array pipeline; `H3_CRS` and `DEFAULT_CELL_COLUMN` are page-owned anchors with one declaration site, never a per-arm literal; every `arro3` return crosses into polars through `pl.from_arrow` over the Arrow PyCapsule interface, never a positional `pl.Series(name, array)` intake.
- Packages: `h3ronpy` and `polars-st` ride the Forge scientific source build band and bind in-process, never across a subprocess boundary — each declares ONE module-scope `lazy import`/`lazy from` line and reifies on the first cell operation, so the compiled band costs an unrelated import nothing while the eager module-level form the manifest bans never appears; the `_BOUNDARY` row and the `Containment` mode carry member NAMES resolved at the call boundary, because a module-scope cell over a live kernel attribute reifies the whole band at import. The polars-st LGPL-2.1 dynamic-linkage posture stays recorded on `data/.api/polars-st.md`.
- Growth: a new cell operation is one `GridOp` case; a new request plane is one `GridRequest` case with one `_plan` arm carrying its route, rostered row, catch set, and thunk; a new refusal law is one `FaultRow` row under `DataLeg.GRID` in this module's one `RAISES` table; a new index kind one `CellKind` row; a new scalar metric one `Metric`/`AreaUnit` row; a new coverage policy one `ContainmentMode` row; a new cell egress one `_BOUNDARY` row the construction gate reads free; a new frame-geometry verb one `_GEO_VERB` row beside its literal, the gate and the fold reading it free; a new grid scheme one `GridScheme` member and the `_SCHEME_ENGINE` row that serves it.
- Boundary: no host coupling, no durable cell store, no lonboard/GeoArrow visualization (`artifacts` owns it); the claims plane is `spatial/geospatial#GEO`, the in-DB engine `spatial/query#SPATIAL`, and never a second WKB geometry encoding or a parallel H3 column owner beside them.

```python
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

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

from rasm.data.spatial.query import SpatialEngine, SpatialQuery
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeResult, boundary, rostered, scoped

if TYPE_CHECKING:
    import numpy as np
    import pyarrow as pa

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.grid")

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


def _admitted(verb: str, *, column: bool, scalar: bool) -> "RuntimeResult[str]":
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
    def Predicate(verb: GeoPredicate, geometry_col: str, other_col: str, distance: Option[float] = Nothing) -> "RuntimeResult[GeoFrameOp]":
        return _admitted(verb, column=True, scalar=distance.is_some()).map(
            lambda _row: GeoFrameOp(predicate=(verb, geometry_col, other_col, distance))
        )

    @staticmethod
    def Measure(verb: GeoMeasure, geometry_col: str, operand_col: Option[str] = Nothing) -> "RuntimeResult[GeoFrameOp]":
        return _admitted(verb, column=operand_col.is_some(), scalar=False).map(
            lambda _row: GeoFrameOp(measure=(verb, geometry_col, operand_col))
        )

    @staticmethod
    def Shape(verb: GeoShape, geometry_col: str, param: Option[float] = Nothing) -> "RuntimeResult[GeoFrameOp]":
        return _admitted(verb, column=False, scalar=param.is_some()).map(
            lambda _row: GeoFrameOp(shape=(verb, geometry_col, param))
        )

    @staticmethod
    def Sjoin(
        other: object, predicate: GeoPredicate = "intersects", how: JoinHow = "inner", distance: Option[float] = Nothing
    ) -> "RuntimeResult[GeoFrameOp]":
        return _admitted(predicate, column=True, scalar=distance.is_some()).map(
            lambda _row: GeoFrameOp(sjoin=(other, predicate, how, distance))
        )


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
    def Boundary(form: BoundaryForm = "polygon", kind: CellKind = CellKind.CELL, radians: bool = False) -> "RuntimeResult[GridOp]":
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
        return GridOp(raster_index=(values, transform, nodata, resolution))

    @staticmethod
    def Rasterize(values: "np.ndarray", size: int | tuple[int, int], nodata: float) -> "RuntimeResult[GridOp]":
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


class _Plan(Struct, frozen=True):
    step: str
    at: "FaultRow[DataLeg]"
    catch: Catch
    work: Callable[[], "pl.DataFrame"]


_SCHEME_ENGINE: Final[Map[GridScheme, str]] = Map.of_seq([(GridScheme.H3, "h3ronpy.h3")])


class GridSystem(Struct, frozen=True):
    scheme: GridScheme = GridScheme.H3
    cell_column: str = DEFAULT_CELL_COLUMN
    crs: str = H3_CRS

    @classmethod
    def of(
        cls, scheme: GridScheme = GridScheme.H3, cell_column: str = DEFAULT_CELL_COLUMN, crs: str = H3_CRS
    ) -> "RuntimeResult[GridSystem]":
        return _SCHEME_ENGINE.try_find(scheme).to_result_with(
            lambda: GRID_SCHEME.raised(scheme.value)
        ).map(lambda _engine: cls(scheme=scheme, cell_column=cell_column, crs=crs))

    def run(self, request: GridRequest) -> "RuntimeResult[pl.DataFrame]":
        plan = self._plan(request)
        subject = f"spatial.grid.{self.scheme.value}.{request.tag}.{plan.step}"
        with _TRACER.start_as_current_span(
            subject, attributes={"rasm.geo.scheme": self.scheme.value, "rasm.geo.plane": request.tag, "rasm.geo.op": plan.step}
        ):
            return boundary(plan.at, plan.work, catch=plan.catch)

    def engine_bin(self, table: "pa.Table", geometry_view: str, resolution: int) -> "RuntimeResult[pa.Table]":
        return SpatialEngine.of({geometry_view: table}).run(SpatialQuery.H3Bin(geometry_view, resolution))

    def _plan(self, request: GridRequest) -> _Plan:
        match request:
            case GridRequest(tag="cells", cells=(op, frame)):
                return _Plan(
                    step=op.tag, at=GRID_CELLS, catch=(ValueError, KeyError, RuntimeError),
                    work=lambda: self._grid(op, frame),
                )
            case GridRequest(tag="lift", lift=lift):
                return _Plan(step=lift.tag, at=GRID_LIFT, catch=(ValueError, OSError), work=lambda: _lift(lift))
            case GridRequest(tag="geometry", geometry=(op, frame)):
                return _Plan(step=op.tag, at=GRID_GEOMETRY, catch=(ValueError, KeyError), work=lambda: _geo(op, frame))
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
        return rasterize_cells(cells, values, size, nodata_value=nodata)

    def _index(self, frame: "pl.DataFrame", resolution: int, source: CellSource) -> object:
        match source:
            case CellSource(tag="coordinates", coordinates=(lat_col, lng_col)):
                return coordinates_to_cells(frame[lat_col], frame[lng_col], resolution, radians=False)
            case CellSource(tag="wkb", wkb=(geometry_col, containment)):
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
            expr = getattr(pl.col(geometry_col).st, verb)(pl.col(other_col), *distance.to_list())
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="measure", measure=(verb, geometry_col, operand_col)):
            expr = getattr(pl.col(geometry_col).st, verb)(*operand_col.map(pl.col).to_list())
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="shape", shape=(verb, geometry_col, param)):
            expr = getattr(pl.col(geometry_col).st, verb)(*param.to_list())
            return frame.with_columns(expr.alias(geometry_col))
        case GeoFrameOp(tag="sjoin", sjoin=(other, predicate, how, distance)):
            return frame.st.sjoin(other, predicate=predicate, how=how, distance=distance.to_optional())
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

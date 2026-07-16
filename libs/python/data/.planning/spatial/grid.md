# [PY_DATA_GRID]

The discrete-global-grid owner, split out of the geospatial claims plane so the DGG plane is governance-visible: `GridSystem` folds cell indexing, measurement, traversal, and hierarchy as vectorized polars expressions over `h3ronpy` Arrow cell ops, with the `polars-st` frame-native geometry vocabulary beside the cell ops so one frame carries geometry AND cells in one vectorized engine. Grid is the terminal tier — it imports `spatial/query` and `tabular/columnar` and is imported by nothing.

The two-H3-substrate boundary is law: in-frame vectorized cell algebra lives here, in-DB binning on `spatial/query#SPATIAL`, and `engine_bin` composes that engine downward for an already-columnar input. Cells, vertexes, and edges stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline; every run keys by `ContentIdentity` through the shared `columnar` `QueryReceipt`. `GridScheme.S2` is the standing deferred hold — `xarray-spatial`'s hard `numba` core dependency is the blocker and numba cp315 the named activation — raising the page-owned `NotImplementedError` the boundary rails.

## [01]-[INDEX]

- [01]-[GRID]: the `GridSystem` DGG owner — the `GridOp` cell-algebra axis, the `CellKind` collapse, the `raster` bridge, the `GeoLift`/`GeoFrameOp` frame-geometry vocabulary, the `engine_bin` in-DB composition.

## [02]-[GRID]

- Owner: `GridSystem` — one owner over the `GridOp`, `GeoLift`, and `GeoFrameOp` axes; every frame-geometry verb is one literal row resolved off the `.st` accessor by name, never a per-verb method family, and the `CellKind` axis routes the index-kind prefix once so the three parallel h3ronpy families never grow a parallel `GridOp` row each.
- Cases: an unsupported `Boundary` `(form, kind)` pair raises the typed boundary fault rather than a mis-applied `assert_never`; `Metric.of_area(unit)` projects an `AreaUnit` into the matching area row, so there is no parallel `Area` sibling case.
- Entry: the grid boundary catches the h3ronpy FFI fault family plus the page-owned S2 deferral, never an un-narrowed `Exception`.
- Auto: the geometry-to-cells leg reads the polars-st WKB `GeoExpr` column directly, so the DGGS index shares the one WKB encoding the `spatial/geospatial#GEO` claims and the `spatial/query#SPATIAL` engine speak; `set_failing_to_invalid` keeps array length stable on parse failure, so an invalid cell is a null data row, never a raised exception in the array pipeline; `H3_CRS` and `DEFAULT_CELL_COLUMN` are page-owned anchors with one declaration site, never a per-arm literal; every `arro3` return crosses into polars through `pl.from_arrow` over the Arrow PyCapsule interface, never a positional `pl.Series(name, array)` intake.
- Receipt: the shared `tabular/columnar` `QueryReceipt.railed` over the result frame, the `engine` carrying the `h3ronpy.<scheme>` route; `GridResult` pairs the frame with that receipt, no new receipt rail.
- Packages: `h3ronpy` and `polars-st` ride the Forge scientific source build band, so every operation body binds the provider function-local under `# noqa: PLC0415`, never a subprocess seam; the polars-st LGPL-2.1 dynamic-linkage posture stays recorded on `data/.api/polars-st.md`.
- Growth: a new cell operation is one `GridOp` case; a new index kind one `CellKind` row; a new scalar metric one `Metric`/`AreaUnit` row; a new coverage policy one `ContainmentMode` row; a new frame-geometry verb one literal row the accessor already answers; a new grid scheme one `GridScheme` member.
- Boundary: no host coupling, no durable cell store, no lonboard/GeoArrow visualization (`artifacts` owns it); the claims plane is `spatial/geospatial#GEO`, the in-DB engine `spatial/query#SPATIAL`, and never a second WKB geometry encoding or a parallel H3 column owner beside them.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.spatial.query import SpatialEngine, SpatialQuery, SpatialResult
from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    import numpy as np
    import polars as pl
    import pyarrow as pa

_TRACER: Final = trace.get_tracer("rasm.data.spatial.grid")


type ResolutionShape = Literal["single", "list", "paired"]
type DiskMode = Literal["cells", "distances", "aggregate"]
type Aggregation = Literal["min", "max"]
type CompactDirection = Literal["compact", "uncompact"]
type BoundaryForm = Literal["polygon", "point", "centroid", "linestring"]
type IjDirection = Literal["to_localij", "from_localij"]
type ValidateMode = Literal["valid", "parse", "format"]
type HierarchyDirection = Literal["parent", "children"]
type RasterDirection = Literal["to_cells", "to_raster"]
type GeoPredicate = Literal["intersects", "contains", "within", "dwithin", "covers"]
type GeoMeasure = Literal["area", "length", "distance"]
type GeoShape = Literal["buffer", "simplify", "centroid", "convex_hull"]
type JoinHow = Literal["inner", "left", "right"]

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
        from h3ronpy import ContainmentMode  # noqa: PLC0415

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


@tagged_union(frozen=True)
class GeoFrameOp:
    tag: Literal["predicate", "measure", "shape", "sjoin"] = tag()
    predicate: tuple[GeoPredicate, str, str, float | None] = case()
    measure: tuple[GeoMeasure, str, str | None] = case()
    shape: tuple[GeoShape, str, float] = case()
    sjoin: tuple[object, GeoPredicate, JoinHow] = case()


@tagged_union(frozen=True)
class GridOp:
    tag: Literal["index", "resolution", "disk", "ring", "measure", "bounds", "compact", "boundary", "local_ij", "validate", "raster", "hierarchy"] = (
        tag()
    )
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
    raster: tuple[RasterDirection, "np.ndarray", tuple[float, ...], int | tuple[int, int] | None] = case()
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
    def Boundary(form: BoundaryForm = "polygon", kind: CellKind = CellKind.CELL, radians: bool = False) -> "GridOp":
        return GridOp(boundary=(form, kind, radians))

    @staticmethod
    def LocalIj(anchor: int, direction: IjDirection = "to_localij") -> "GridOp":
        return GridOp(local_ij=(anchor, direction))

    @staticmethod
    def Validate(mode: ValidateMode = "valid", kind: CellKind = CellKind.CELL) -> "GridOp":
        return GridOp(validate=(mode, kind))

    @staticmethod
    def Raster(values: "np.ndarray", transform: tuple[float, ...], resolution: int | None = None) -> "GridOp":
        return GridOp(raster=("to_cells", values, transform, resolution))

    @staticmethod
    def Rasterize(values: "np.ndarray", size: int | tuple[int, int]) -> "GridOp":
        return GridOp(raster=("to_raster", values, (), size))

    @staticmethod
    def Hierarchy(direction: HierarchyDirection = "parent", target: int = 0) -> "GridOp":
        return GridOp(hierarchy=(direction, target))


class GridResult(Struct, frozen=True):
    frame: "pl.DataFrame"
    receipt: QueryReceipt


# the data-valued (form, kind) -> vector-member correspondence on the folder's ONE Map rail; the
# row value is a member NAME the `_boundary` getattr resolves, an unsupported pair a typed fault.
_BOUNDARY_WKB: Final[Map[tuple[BoundaryForm, CellKind], str]] = Map.of_seq([
    (("polygon", CellKind.CELL), "cells_to_wkb_polygons"),
    (("point", CellKind.CELL), "cells_to_wkb_points"),
    (("point", CellKind.VERTEX), "vertexes_to_wkb_points"),
    (("linestring", CellKind.EDGE), "directededges_to_wkb_linestrings"),
])


class GridSystem(Struct, frozen=True):
    scheme: GridScheme = GridScheme.H3
    cell_column: str = DEFAULT_CELL_COLUMN
    crs: str = H3_CRS

    def apply(self, op: GridOp, frame: "pl.DataFrame") -> "RuntimeRail[GridResult]":
        with _TRACER.start_as_current_span(
            f"spatial.grid.{self.scheme.value}.{op.tag}", attributes={"rasm.geo.scheme": self.scheme.value, "rasm.geo.op": op.tag}
        ):
            return boundary(
                f"spatial.grid.{self.scheme.value}.{op.tag}",
                lambda: self._grid(op, frame),
                catch=(NotImplementedError, ValueError, KeyError, RuntimeError),
            ).bind(lambda result: self._result(op.tag, "h3ronpy", result))

    def lift(self, lift: GeoLift) -> "RuntimeRail[GridResult]":
        with _TRACER.start_as_current_span(f"spatial.grid.lift.{lift.tag}", attributes={"rasm.geo.op": lift.tag}):
            return boundary(f"spatial.grid.lift.{lift.tag}", lambda: _lift(lift), catch=(ValueError, OSError)).bind(
                lambda frame: self._result(lift.tag, "polars-st", frame)
            )

    def geo(self, op: GeoFrameOp, frame: "pl.DataFrame") -> "RuntimeRail[GridResult]":
        with _TRACER.start_as_current_span(f"spatial.grid.geo.{op.tag}", attributes={"rasm.geo.op": op.tag}):
            return boundary(f"spatial.grid.geo.{op.tag}", lambda: _geo(op, frame), catch=(ValueError, KeyError)).bind(
                lambda result: self._result(op.tag, "polars-st", result)
            )

    def engine_bin(self, table: "pa.Table", geometry_view: str, resolution: int) -> "RuntimeRail[SpatialResult]":
        # the in-DB half of the two-substrate law: an already-columnar frame bins through the
        # spatial/query engine's `h3` extension SQL; the in-frame half is the h3ronpy plane above.
        return SpatialEngine.of({geometry_view: table}).run(SpatialQuery.H3Bin(geometry_view, resolution))

    def _result(self, tag: str, route: str, frame: "pl.DataFrame") -> "RuntimeRail[GridResult]":
        engine = f"h3ronpy.{self.scheme.value}" if route == "h3ronpy" else route
        return QueryReceipt.railed(engine, tag, frame.to_arrow()).map(lambda receipt: GridResult(frame=frame, receipt=receipt))

    def _grid(self, op: GridOp, frame: "pl.DataFrame") -> "pl.DataFrame":
        import polars as pl  # noqa: PLC0415

        if self.scheme is GridScheme.S2:
            raise NotImplementedError("grid.s2.deferred")
        import h3ronpy  # noqa: PLC0415
        from h3ronpy import (  # noqa: PLC0415
            change_resolution,
            change_resolution_list,
            change_resolution_paired,
            cells_to_localij,
            compact,
            grid_disk,
            grid_disk_aggregate_k,
            grid_disk_distances,
            grid_ring_distances,
            localij_to_cells,
            uncompact,
        )
        from h3ronpy.vector import cells_bounds_arrays  # noqa: PLC0415

        def attach(name: str, array: object) -> "pl.DataFrame":
            return frame.with_columns(pl.from_arrow(array).rename(name))

        def derive(name: str, array: object) -> "pl.DataFrame":
            return pl.from_arrow(array).rename(name).to_frame()

        cells = frame[self.cell_column] if op.tag not in {"index", "raster"} else None
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
            case GridOp(tag="raster", raster=("to_cells", values, transform, resolution)):
                return pl.from_arrow(self._raster_index(values, transform, resolution if isinstance(resolution, int) else None))
            case GridOp(tag="raster", raster=("to_raster", values, _, size)):
                array, geotransform = self._raster_egress(frame[self.cell_column], values, size if size is not None else 0)
                return pl.DataFrame({"raster": [array], "transform": [geotransform]})
            case GridOp(tag="hierarchy", hierarchy=("parent", target)):
                return attach(self.cell_column, change_resolution(cells, target))
            case GridOp(tag="hierarchy", hierarchy=("children", target)):
                return attach("children", change_resolution_list(cells, target))
            case unreachable:
                assert_never(unreachable)

    def _boundary(self, frame: "pl.DataFrame", cells: object, form: BoundaryForm, kind: CellKind, radians: bool) -> "pl.DataFrame":
        import h3ronpy.vector as vector  # noqa: PLC0415
        import polars as pl  # noqa: PLC0415

        if form == "centroid":
            return pl.from_arrow(vector.cells_to_coordinates(cells, radians=radians))
        member = _BOUNDARY_WKB.try_find((form, kind))
        if member.is_none():
            raise ValueError(f"no WKB egress for ({form}, {kind.value})")
        wkb = getattr(vector, member.value)(cells, radians=radians)
        return frame.with_columns(pl.from_arrow(wkb).rename("boundary"))

    def _validate(self, frame: "pl.DataFrame", cells: object, mode: ValidateMode, kind: CellKind) -> "pl.DataFrame":
        import h3ronpy  # noqa: PLC0415
        import polars as pl  # noqa: PLC0415

        column, call = {
            "valid": ("valid", lambda: getattr(h3ronpy, f"{kind.value}_valid")(cells, booleanarray=True)),
            "parse": (self.cell_column, lambda: getattr(h3ronpy, f"{kind.value}_parse")(cells, set_failing_to_invalid=True)),
            "format": ("hex", lambda: getattr(h3ronpy, f"{kind.value}_to_string")(cells)),
        }[mode]
        return frame.with_columns(pl.from_arrow(call()).rename(column))

    def _raster_index(self, values: "np.ndarray", transform: tuple[float, ...], resolution: int | None) -> "pa.Table":
        from h3ronpy.raster import nearest_h3_resolution, raster_to_dataframe  # noqa: PLC0415

        target = resolution if resolution is not None else nearest_h3_resolution(values.shape, transform)
        return raster_to_dataframe(values, transform, target, nodata_value=None, compact=False)

    def _raster_egress(self, cells: object, values: "np.ndarray", size: "int | tuple[int, int]") -> "tuple[np.ndarray, tuple[float, ...]]":
        from h3ronpy.raster import rasterize_cells  # noqa: PLC0415

        return rasterize_cells(cells, values, size, nodata_value=0)

    def _index(self, frame: "pl.DataFrame", resolution: int, source: CellSource) -> object:
        from h3ronpy.vector import coordinates_to_cells, geometry_to_cells, wkb_to_cells  # noqa: PLC0415

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
    import polars as pl  # noqa: PLC0415
    import polars_st as st  # noqa: PLC0415

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
    import polars as pl  # noqa: PLC0415

    match op:
        case GeoFrameOp(tag="predicate", predicate=(verb, geometry_col, other_col, distance)):
            # `dwithin` is the one distance-bearing predicate row; every other verb is geometry-only.
            head = getattr(pl.col(geometry_col).st, verb)
            expr = head(pl.col(other_col), distance) if verb == "dwithin" else head(pl.col(other_col))
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="measure", measure=(verb, geometry_col, operand_col)):
            head = getattr(pl.col(geometry_col).st, verb)
            expr = head(pl.col(operand_col)) if operand_col is not None else head()
            return frame.with_columns(expr.alias(verb))
        case GeoFrameOp(tag="shape", shape=(verb, geometry_col, param)):
            head = getattr(pl.col(geometry_col).st, verb)
            expr = head(param) if verb in {"buffer", "simplify"} else head()
            return frame.with_columns(expr.alias(geometry_col))
        case GeoFrameOp(tag="sjoin", sjoin=(other, predicate, how)):
            return frame.st.sjoin(other, predicate=predicate, how=how)
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [PY_DATA_GEOSPATIAL]

Geospatial CLAIMS plane — one third of the spatial triptych, beside the `spatial/query#SPATIAL` in-DB engine and the `spatial/grid#GRID` DGG plane. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision over geopandas/shapely/pyogrio with pyproj backing the axis-order-aware `reproject` prelude and one `VectorOp` in-frame vector-algebra axis; `RasterGeoClaim` carries coverage/band/resampling/nodata/CRS with one `RasterOp` coverage axis spanning the in-memory and streaming/remote/VRT/sample/COG-write rows; `EgressFormat` is one `StrEnum` whose member value IS the OGR driver and whose `write` it carries. STAC claims live on `spatial/catalog#CATALOG` — `StacGeoClaim`/`StacIngest` are re-homed to the STAC-table owner, so this page holds no catalog import.

`RasterGeoClaim.transform` is the provenance affine the `spatial/catalog#ASSETS` `AssetFold` constructs from `proj:transform`. `GEOARROW` egress is the NATIVE buffer path — `to_arrow(geometry_encoding="geoarrow")` exports zero-copy extension arrays serialized as Arrow IPC, never a parquet byte-roundtrip — and `geoarrow_wire` is the `geoarrow-rust-compute` hand-off sharing the `csharp:Rasm.Compute` GLB wire layout. Every network-bearing read routes its blocking provider call through `guarded(RetryClass.HTTP, on_thread, ...)`, the `THREAD_BAND`-bounded hop; every bundle keys by one runtime `ContentIdentity` folding the shared `tabular/columnar#SCAN` `QueryReceipt`. geopandas/shapely/rasterio ride the Forge scientific source build band, so every operation body binds its provider function-local under `# noqa: PLC0415`, never a subprocess seam.

## [01]-[INDEX]

- [01]-[GEO]: the `VectorGeoClaim`/`RasterGeoClaim` claim owners — the `VectorOp`/`RasterOp` axes, the `VectorIngress` pushdown row, the `EgressFormat` egress, the `geoarrow_wire` hand-off.
- [02]-[COVERAGE]: the `rioxarray` CF bridge — georeferenced read, the bare-ndarray lift onto the CF plane, the CF-side COG write.

## [02]-[GEO]

- Owner: `resampling` is the claim-level default an op overrides through `resampling or self.resampling`, never a per-op factory literal.
- Cases: `Stream` folds each `block_windows` tile straight into one pre-allocated destination slice through `read(out=)`, so peak memory is the decimated destination plus one block — the one measured streaming-IO kernel where the `for` over tiles is the platform-forced boundary exemption. `VectorIngress`'s bare-`.dbf` attribute reads route through the same ESRI Shapefile driver, closing the struck-`csvkit` foreign-decode gap.
- Entry: every op is an OTel span around a `boundary` fence binding its real provider-fault root (`RasterioError`/`ShapelyError`/`CRSError`/`DataSourceError`), never an un-narrowed `Exception`; the `reproject` prelude normalizes every binary operand onto one CRS with a no-op short-circuit, `to_crs` when the transform has an inverse and `set_crs` for a metadata-only label. Self-opening raster rows enter their own dataset inside one `ExitStack`, so the GDAL handle closes on the boundary exit before the railed receipt derives.
- Growth: a new vector or raster operation is one `VectorOp`/`RasterOp` case; a new linear or geodesic verb one `LinearKind`/`GeodesicKind` row; a new constructive op one `ConstructKind` row plus its `_CONSTRUCT` behavior row; a new binary predicate one `JoinPredicate` row; a new resampling mode one `Resampling` literal arm mapped at the edge; a new VSI scheme one `VsiScheme` row; a new egress format one `EgressFormat` member; zero new surface.
- Boundary: no host mutation, no durable store; no STAC claim or NDJSON-interchange arm on this page — the catalog owner homes them, and the STAC-interchange providers bind only inside it; `WarpedVRT` is GDAL-native streamed reproject, never a second byte-window transport beside the `tabular/egress` `obstore` rail.

```python signature
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import SpanKind

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Callable

    import numpy as np
    import pyarrow as pa
    from geopandas import GeoDataFrame, GeoSeries
    from rasterio import DatasetReader

_TRACER: Final = trace.get_tracer("rasm.data.spatial.geospatial")


type SetOp = Literal["intersection", "union", "difference", "symmetric_difference", "identity"]
type JoinHow = Literal["inner", "left", "right"]
type Resampling = Literal[
    "nearest", "bilinear", "cubic", "cubic_spline", "lanczos", "average", "mode", "gauss", "max", "min", "med", "q1", "q3", "sum", "rms"
]
type OverviewResampling = Literal["nearest", "bilinear", "cubic", "cubic_spline", "lanczos", "average", "mode", "gauss", "rms"]
type MergeMethod = Literal["first", "last", "min", "max", "sum", "count"]
type Compression = Literal["deflate", "zstd", "lzw", "webp", "lerc", "lerc_deflate", "lerc_zstd", "jpeg", "none"]
type Bounds = tuple[float, float, float, float]
type TileShape = tuple[int, int] | None

_DEGREE_GRID: Final[float] = 1.0 / 111_320.0


class VsiScheme(StrEnum):
    CURL = "/vsicurl/"
    S3 = "/vsis3/"
    GS = "/vsigs/"
    AZURE = "/vsiaz/"
    ZIP = "/vsizip/"

    def path(self, href: str) -> str:
        return f"{self.value}{href}"


class GeometryFamily(StrEnum):
    POINT = "point"
    LINESTRING = "linestring"
    POLYGON = "polygon"
    MULTIPOLYGON = "multipolygon"


class JoinPredicate(StrEnum):
    INTERSECTS = "intersects"
    WITHIN = "within"
    CONTAINS = "contains"
    DWITHIN = "dwithin"
    TOUCHES = "touches"
    CROSSES = "crosses"
    OVERLAPS = "overlaps"


class LinearKind(StrEnum):
    POLYGONIZE = "polygonize"
    LOCATE = "line_locate_point"
    INTERPOLATE = "line_interpolate_point"
    SHARED_PATHS = "shared_paths"
    SHORTEST_LINE = "shortest_line"


class GeodesicKind(StrEnum):
    AREA = "area"
    PERIMETER = "perimeter"
    LINE = "line"


class ConstructKind(StrEnum):
    BUFFER = "buffer"
    SIMPLIFY = "simplify"
    CONVEX_HULL = "convex_hull"
    CONCAVE_HULL = "concave_hull"
    VORONOI_POLYGONS = "voronoi_polygons"
    DELAUNAY_TRIANGLES = "delaunay_triangles"


_CONSTRUCT: "Final[Map[ConstructKind, Callable[[GeoSeries, float], GeoSeries]]]" = Map.of_seq([
    (ConstructKind.BUFFER, lambda g, p: g.buffer(p)),
    (ConstructKind.SIMPLIFY, lambda g, p: g.simplify(p)),
    (ConstructKind.CONVEX_HULL, lambda g, _p: g.convex_hull),
    (ConstructKind.CONCAVE_HULL, lambda g, p: g.concave_hull(ratio=p)),
    (ConstructKind.VORONOI_POLYGONS, lambda g, _p: g.voronoi_polygons()),
    (ConstructKind.DELAUNAY_TRIANGLES, lambda g, p: g.delaunay_triangles(tolerance=p)),
])


class EgressFormat(StrEnum):
    GEOJSON = "GeoJSON"
    GEOPARQUET = "GeoParquet"
    GEOARROW = "GeoArrow"
    FLATGEOBUF = "FlatGeobuf"

    def write(self, frame: GeoDataFrame, path: str) -> RuntimeRail[ContentKey]:
        def emit() -> bytes:
            import pyogrio  # noqa: PLC0415

            match self:
                case EgressFormat.GEOPARQUET:
                    frame.to_parquet(path)
                case EgressFormat.GEOARROW:
                    # native zero-copy geoarrow extension arrays written as Arrow IPC, not the pyogrio driver write.
                    import pyarrow as pa  # noqa: PLC0415
                    import pyarrow.feather as paf  # noqa: PLC0415

                    paf.write_feather(pa.table(frame.to_arrow(geometry_encoding="geoarrow")), path)
                case _:
                    pyogrio.write_dataframe(frame, path, driver=self.value, use_arrow=True)
            return Path(path).read_bytes()

        with _TRACER.start_as_current_span(f"geo.egress.{self.value}", attributes={"rasm.geo.format": self.value}):
            from pyogrio.errors import DataSourceError  # noqa: PLC0415

            return boundary(f"geo.egress.{self.value}", emit, catch=(DataSourceError, OSError)).bind(
                lambda payload: ContentIdentity.of(self.value, payload)
            )


@tagged_union(frozen=True)
class VectorOp:
    tag: Literal["join", "overlay", "dissolve", "clip", "construct", "predicate", "linear", "geodesic"] = tag()
    join: tuple[JoinPredicate, "GeoDataFrame", JoinHow, float | None] = case()
    overlay: tuple["GeoDataFrame", SetOp] = case()
    dissolve: tuple[tuple[str, ...], str] = case()
    clip: tuple["GeoDataFrame", bool] = case()
    construct: tuple[ConstructKind, float] = case()
    predicate: tuple[JoinPredicate, "GeoDataFrame", float | None] = case()
    linear: tuple[LinearKind, "GeoDataFrame | None", float] = case()
    geodesic: GeodesicKind = case()

    @staticmethod
    def Join(predicate: JoinPredicate, other: "GeoDataFrame", how: JoinHow = "inner", max_distance: float | None = None) -> "VectorOp":
        return VectorOp(join=(predicate, other, how, max_distance))

    @staticmethod
    def Overlay(other: "GeoDataFrame", how: SetOp = "intersection") -> "VectorOp":
        return VectorOp(overlay=(other, how))

    @staticmethod
    def Dissolve(by: tuple[str, ...], aggfunc: str = "first") -> "VectorOp":
        return VectorOp(dissolve=(by, aggfunc))

    @staticmethod
    def Clip(mask: "GeoDataFrame", keep_geom_type: bool = True) -> "VectorOp":
        return VectorOp(clip=(mask, keep_geom_type))

    @staticmethod
    def Construct(kind: ConstructKind, param: float = 0.0) -> "VectorOp":
        return VectorOp(construct=(kind, param))

    @staticmethod
    def Predicate(name: JoinPredicate, other: "GeoDataFrame", distance: float | None = None) -> "VectorOp":
        return VectorOp(predicate=(name, other, distance))

    @staticmethod
    def Linear(kind: LinearKind, other: "GeoDataFrame | None" = None, param: float = 0.0) -> "VectorOp":
        return VectorOp(linear=(kind, other, param))

    @staticmethod
    def Geodesic(kind: GeodesicKind = GeodesicKind.AREA) -> "VectorOp":
        return VectorOp(geodesic=kind)


@tagged_union(frozen=True)
class RasterOp:
    tag: Literal[
        "window",
        "stream",
        "sample",
        "mosaic",
        "mask",
        "geometry_mask",
        "sieve",
        "vectorize",
        "rasterize",
        "reproject",
        "vrt",
        "remote_read",
        "memory_source",
        "write_cog",
    ] = tag()
    window: tuple[Bounds, bool] = case()
    stream: tuple[int, TileShape, Resampling | None] = case()
    sample: tuple[tuple[tuple[float, float], ...], tuple[int, ...] | None] = case()
    mosaic: tuple[tuple[str, ...], MergeMethod, Resampling | None, Bounds | None, tuple[float, float] | None] = case()
    mask: tuple[tuple[object, ...], bool, bool, bool] = case()
    geometry_mask: tuple[tuple[object, ...], tuple[int, int], bool, bool] = case()
    sieve: tuple[int, int] = case()
    vectorize: tuple[int, int] = case()
    rasterize: tuple[tuple[object, ...], tuple[int, int], Literal["replace", "add"], bool] = case()
    reproject: tuple[str, Resampling | None] = case()
    vrt: tuple[str, Resampling | None, int | None, int | None] = case()
    remote_read: tuple[str, VsiScheme, Bounds | None, int] = case()
    memory_source: tuple[bytes, "RasterOp"] = case()
    write_cog: tuple[str, "np.ndarray", tuple[float, ...], str, CogProfile] = case()

    @staticmethod
    def Window(bounds: Bounds, boundless: bool = False) -> "RasterOp":
        return RasterOp(window=(bounds, boundless))

    @staticmethod
    def Stream(bidx: int = 1, tile_shape: TileShape = None, resampling: Resampling | None = None) -> "RasterOp":
        return RasterOp(stream=(bidx, tile_shape, resampling))

    @staticmethod
    def Sample(coordinates: tuple[tuple[float, float], ...], indexes: tuple[int, ...] | None = None) -> "RasterOp":
        return RasterOp(sample=(coordinates, indexes))

    @staticmethod
    def Mosaic(
        sources: tuple[str, ...],
        method: MergeMethod = "first",
        resampling: Resampling | None = None,
        bounds: Bounds | None = None,
        res: tuple[float, float] | None = None,
    ) -> "RasterOp":
        return RasterOp(mosaic=(sources, method, resampling, bounds, res))

    @staticmethod
    def Mask(shapes: tuple[object, ...], crop: bool = True, all_touched: bool = False, invert: bool = False) -> "RasterOp":
        return RasterOp(mask=(shapes, crop, all_touched, invert))

    @staticmethod
    def GeometryMask(shapes: tuple[object, ...], out_shape: tuple[int, int], all_touched: bool = False, invert: bool = False) -> "RasterOp":
        return RasterOp(geometry_mask=(shapes, out_shape, all_touched, invert))

    @staticmethod
    def Sieve(size: int, connectivity: int = 4) -> "RasterOp":
        return RasterOp(sieve=(size, connectivity))

    @staticmethod
    def Vectorize(connectivity: int = 4, band: int = 1) -> "RasterOp":
        return RasterOp(vectorize=(connectivity, band))

    @staticmethod
    def Rasterize(
        shapes: tuple[object, ...], out_shape: tuple[int, int], merge_alg: Literal["replace", "add"] = "replace", all_touched: bool = False
    ) -> "RasterOp":
        return RasterOp(rasterize=(shapes, out_shape, merge_alg, all_touched))

    @staticmethod
    def Reproject(target_crs: str, resampling: Resampling | None = None) -> "RasterOp":
        return RasterOp(reproject=(target_crs, resampling))

    @staticmethod
    def Vrt(target_crs: str, resampling: Resampling | None = None, width: int | None = None, height: int | None = None) -> "RasterOp":
        return RasterOp(vrt=(target_crs, resampling, width, height))

    @staticmethod
    def RemoteRead(href: str, vsi_scheme: VsiScheme = VsiScheme.CURL, bounds: Bounds | None = None, overview: int = 1) -> "RasterOp":
        return RasterOp(remote_read=(href, vsi_scheme, bounds, overview))

    @staticmethod
    def MemorySource(payload: bytes, op: "RasterOp") -> "RasterOp":
        return RasterOp(memory_source=(payload, op))

    @staticmethod
    def WriteCog(path: str, array: "np.ndarray", transform: tuple[float, ...], crs: str, profile: CogProfile = CogProfile()) -> "RasterOp":
        return RasterOp(write_cog=(path, array, transform, crs, profile))


class _Coverage(Struct, frozen=True):
    array: "np.ndarray"
    transform: tuple[float, ...]
    op_tag: str
    source: str


class CoverageResult(Struct, frozen=True):
    array: "np.ndarray"
    transform: tuple[float, ...]
    receipt: QueryReceipt


class CogProfile(Struct, frozen=True):
    compress: Compression = "deflate"
    blocksize: int = 512
    overviews: Literal["auto", "ignore", "force_use_existing"] = "auto"
    overview_resampling: OverviewResampling = "nearest"
    num_threads: Literal["all_cpus"] | int = "all_cpus"
    predictor: Literal["none", "standard", "floating_point"] = "none"

    def creation(self, array: "np.ndarray", crs: str, nodata: float) -> dict[str, object]:
        return {
            "driver": "COG",
            "dtype": str(array.dtype),
            "crs": crs,
            "nodata": nodata,
            "count": int(array.shape[0]),
            "height": int(array.shape[-2]),
            "width": int(array.shape[-1]),
            "compress": self.compress,
            "blocksize": self.blocksize,
            "overviews": self.overviews,
            "overview_resampling": self.overview_resampling,
            "num_threads": self.num_threads,
            "predictor": self.predictor,
        }


class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    async def apply(self, op: VectorOp, frame: "GeoDataFrame") -> "RuntimeRail[GeoDataFrame]":
        from pyproj.exceptions import CRSError  # noqa: PLC0415
        from shapely.errors import ShapelyError  # noqa: PLC0415

        # overlay/join/dissolve walk whole frames — a blocking leg riding the banded thread hop, never the loop.
        with _TRACER.start_as_current_span(f"geo.vector.{op.tag}", attributes={"rasm.geo.crs": self.crs, "rasm.geo.op": op.tag}):
            return await async_boundary(
                f"geo.vector.{op.tag}", lambda: on_thread(self._vector, op, frame), catch=(ShapelyError, CRSError, KeyError, ValueError)
            )

    def reproject(self, frame: "GeoDataFrame") -> "GeoDataFrame":
        import pyproj  # noqa: PLC0415

        target = pyproj.CRS.from_user_input(self.crs)
        if frame.crs is not None and pyproj.CRS.from_user_input(frame.crs) == target:
            return frame
        transformer = pyproj.Transformer.from_crs(frame.crs, target, always_xy=self.axis_order == "xy")
        return frame.to_crs(target) if transformer.has_inverse else frame.set_crs(target, allow_override=True)

    def _vector(self, op: VectorOp, frame: "GeoDataFrame") -> "GeoDataFrame":
        import geopandas as gpd  # noqa: PLC0415
        import shapely  # noqa: PLC0415

        grid = 10.0**-self.precision * (_DEGREE_GRID if self.units == "degree" else 1.0)
        keep_family = self.family in {GeometryFamily.POLYGON, GeometryFamily.MULTIPOLYGON}
        snapped = self.reproject(frame).assign(geometry=lambda f: shapely.set_precision(f.geometry.to_numpy(), grid))
        match op:
            case VectorOp(tag="join", join=(predicate, other, how, max_distance)):
                aligned = self.reproject(other)
                match predicate, max_distance:
                    case JoinPredicate.DWITHIN, float() as distance:
                        # DWITHIN with a bound is the within-distance relation `sjoin(predicate="dwithin", distance=)`,
                        # never `sjoin_nearest` (which returns the single nearest, a different relation).
                        return gpd.sjoin(snapped, aligned, how=how, predicate="dwithin", distance=distance)
                    case JoinPredicate.DWITHIN, None:
                        # DWITHIN with no bound degenerates to the unbounded nearest-neighbor join.
                        return gpd.sjoin_nearest(snapped, aligned, how=how, distance_col="distance")
                    case _, _:
                        return gpd.sjoin(snapped, aligned, how=how, predicate=predicate.value)
            case VectorOp(tag="overlay", overlay=(other, how)):
                return gpd.overlay(snapped, self.reproject(other), how=how, keep_geom_type=keep_family)
            case VectorOp(tag="dissolve", dissolve=(by, aggfunc)):
                return snapped.dissolve(by=list(by), aggfunc=aggfunc)
            case VectorOp(tag="clip", clip=(mask, keep_geom_type)):
                return gpd.clip(snapped, self.reproject(mask), keep_geom_type=keep_geom_type and keep_family)
            case VectorOp(tag="construct", construct=(kind, param)):
                return snapped.set_geometry(_CONSTRUCT[kind](snapped.geometry, param))
            case VectorOp(tag="predicate", predicate=(name, other, distance)):
                target = self.reproject(other).union_all()
                # `dwithin` is the one binary predicate carrying a distance; every other accessor is geometry-only.
                hits = (
                    snapped.geometry.dwithin(target, distance if distance is not None else 0.0)
                    if name is JoinPredicate.DWITHIN
                    else getattr(snapped.geometry, name.value)(target)
                )
                return snapped.loc[hits]
            case VectorOp(tag="linear", linear=(kind, other, param)):
                lines = snapped.geometry.to_numpy()
                target = self.reproject(other).union_all() if other is not None else None
                match kind:
                    case LinearKind.POLYGONIZE:
                        # a noded line set rebuilds into its polygon set — a row-count change is
                        # inherent to the operation, so the result is a fresh polygon frame.
                        polygons = shapely.polygonize(lines)
                        return gpd.GeoDataFrame(geometry=gpd.GeoSeries(list(polygons.geoms), crs=snapped.crs))
                    case LinearKind.LOCATE:
                        return snapped.assign(measure=shapely.line_locate_point(lines, target))
                    case LinearKind.INTERPOLATE:
                        return snapped.set_geometry(gpd.GeoSeries(shapely.line_interpolate_point(lines, param), crs=snapped.crs))
                    case LinearKind.SHARED_PATHS:
                        return snapped.set_geometry(gpd.GeoSeries(shapely.shared_paths(lines, target), crs=snapped.crs))
                    case LinearKind.SHORTEST_LINE:
                        return snapped.set_geometry(gpd.GeoSeries(shapely.shortest_line(lines, target), crs=snapped.crs))
                    case unreachable_kind:
                        assert_never(unreachable_kind)
            case VectorOp(tag="geodesic", geodesic=kind):
                import pyproj  # noqa: PLC0415

                # WGS84 ellipsoid true-earth values a planar CRS transform cannot give; the
                # geodesic runs over lon/lat geometry, so the claim CRS prelude has already landed 4326.
                geod = pyproj.Geod(ellps="WGS84")
                if kind is GeodesicKind.LINE:
                    values = [geod.line_length(*geom.xy) for geom in snapped.geometry]
                else:
                    pairs = [geod.geometry_area_perimeter(geom) for geom in snapped.geometry]
                    values = [abs(a) if kind is GeodesicKind.AREA else per for a, per in pairs]
                return snapped.assign(**{f"geodesic_{kind.value}": values})
            case unreachable:
                assert_never(unreachable)


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: Resampling
    nodata: float
    # source affine — six coefficients, or empty when unknown; each coverage op REPORTS the transform its
    # operation derives, so this claim slot is provenance, never a stale substitute for the op result.
    transform: tuple[float, ...] = ()

    async def apply(self, op: RasterOp, source: "DatasetReader") -> "RuntimeRail[CoverageResult]":
        from rasterio.errors import RasterioError  # noqa: PLC0415

        # reproject/mask/warp read full arrays — a blocking leg riding the banded thread hop, never the loop.
        with _TRACER.start_as_current_span(
            f"geo.raster.{op.tag}",
            attributes={"rasm.geo.crs": self.crs, "rasm.geo.op": op.tag, "rasm.geo.bands": self.band_count, "rasm.geo.resampling": self.resampling},
        ):
            railed = await async_boundary(f"geo.raster.{op.tag}", lambda: on_thread(self._raster, op, source), catch=(RasterioError, ValueError))
            return railed.bind(self._result)

    async def apply_remote(self, op: RasterOp, source: "DatasetReader | None" = None) -> "RuntimeRail[CoverageResult]":
        # abandon frees the band slot when an enclosing deadline trips — a wedged /vsicurl read runs out unobserved;
        # kind=CLIENT marks the outbound network leg per the catalog span-kind law.
        with _TRACER.start_as_current_span(f"geo.raster.{op.tag}", kind=SpanKind.CLIENT, attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag}):
            acquired = await guarded(RetryClass.HTTP, on_thread, lambda: self._remote_read(op, source), abandon=True, subject=f"geo.raster.{op.tag}")
            return acquired.bind(self._result)

    def _remote_read(self, op: RasterOp, source: "DatasetReader | None") -> "_Coverage":
        # a `/vsicurl/` GDAL transient is a `RasterioIOError` (rooted at `RasterioError`, not a stdlib
        # transient), so it is re-raised as `ConnectionError` for the `RetryClass.HTTP` `_retry_after`
        # set rather than the resilience owner growing a `rasterio` provider-introspection target.
        from rasterio.errors import RasterioIOError  # noqa: PLC0415

        try:
            return self._raster(op, source)
        except RasterioIOError as cause:
            raise ConnectionError(str(cause)) from cause

    def _raster(self, op: RasterOp, source: "DatasetReader | None") -> "_Coverage":
        from contextlib import ExitStack  # noqa: PLC0415

        import numpy as np  # noqa: PLC0415
        import rasterio  # noqa: PLC0415
        from rasterio import features, mask, merge, warp, windows  # noqa: PLC0415
        from rasterio.enums import MergeAlg  # noqa: PLC0415
        from rasterio.enums import Resampling as RioResampling  # noqa: PLC0415
        from rasterio.io import MemoryFile  # noqa: PLC0415
        from rasterio.vrt import WarpedVRT  # noqa: PLC0415

        match op:
            case RasterOp(tag="window", window=(bounds, boundless)):
                window = windows.from_bounds(*bounds, transform=source.transform)
                array = source.read(window=window, boundless=boundless, fill_value=self.nodata)
                return self._cover(np.asarray(array), source.window_transform(window), op.tag, source)
            case RasterOp(tag="stream", stream=(bidx, tile_shape, resampling)):
                row_factor, col_factor = (source.height // tile_shape[0], source.width // tile_shape[1]) if tile_shape else (1, 1)
                destination = np.full((source.height // row_factor, source.width // col_factor), self.nodata, dtype=source.dtypes[bidx - 1])
                for _, block in source.block_windows(bidx):
                    row0, col0 = block.row_off // row_factor, block.col_off // col_factor
                    rows, cols = block.height // row_factor, block.width // col_factor
                    source.read(
                        bidx,
                        window=block,
                        out=destination[row0 : row0 + rows, col0 : col0 + cols],
                        resampling=RioResampling[resampling or self.resampling],
                        fill_value=self.nodata,
                        boundless=True,
                    )
                return self._cover(destination, tuple(source.transform * rasterio.Affine.scale(col_factor, row_factor))[:6], op.tag, source)
            case RasterOp(tag="sample", sample=(coordinates, indexes)):
                picked = np.asarray(list(source.sample(list(coordinates), indexes=list(indexes) if indexes else None)))
                return self._cover(picked, tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="vrt", vrt=(target_crs, resampling, width, height)):
                with WarpedVRT(
                    source, crs=target_crs, resampling=RioResampling[resampling or self.resampling], width=width, height=height, nodata=self.nodata
                ) as warped:
                    return self._cover(np.asarray(warped.read()), tuple(warped.transform)[:6], op.tag, source)
            case RasterOp(tag="remote_read", remote_read=(href, vsi_scheme, bounds, overview)):
                with ExitStack() as stack:
                    stack.enter_context(rasterio.Env(GDAL_DISABLE_READDIR_ON_OPEN="EMPTY_DIR"))
                    remote = stack.enter_context(rasterio.open(vsi_scheme.path(href)))
                    window = windows.from_bounds(*bounds, transform=remote.transform) if bounds is not None else None
                    out_shape = (
                        (
                            (remote.count, int(window.height) // overview, int(window.width) // overview)
                            if window is not None
                            else (remote.count, remote.height // overview, remote.width // overview)
                        )
                        if overview > 1
                        else None
                    )
                    array = remote.read(window=window, out_shape=out_shape, boundless=window is not None, fill_value=self.nodata)
                    base = remote.window_transform(window) if window is not None else remote.transform
                    transform = tuple(base * rasterio.Affine.scale(overview, overview))[:6] if overview > 1 else tuple(base)[:6]
                    return self._cover(np.asarray(array), transform, op.tag, remote)
            case RasterOp(tag="memory_source", memory_source=(payload, inner)):
                with ExitStack() as stack:
                    memfile = stack.enter_context(MemoryFile(payload))
                    opened = stack.enter_context(memfile.open())
                    return self._raster(inner, opened)
            case RasterOp(tag="write_cog", write_cog=(path, array, transform, crs, profile)):
                creation = profile.creation(array, crs, self.nodata) | {"transform": rasterio.Affine(*transform)}
                with ExitStack() as stack:
                    written = stack.enter_context(rasterio.open(path, mode="w", **creation))
                    written.write(array)
                    return self._cover(np.asarray(array), transform, op.tag, written)
            case RasterOp(tag="mosaic", mosaic=(sources, method, resampling, bounds, res)):
                with ExitStack() as stack:
                    opened = [stack.enter_context(rasterio.open(path)) for path in sources]
                    mosaic, out_transform = merge.merge(
                        opened, bounds=bounds, res=res, method=method, resampling=RioResampling[resampling or self.resampling], nodata=self.nodata
                    )
                    return self._cover(np.asarray(mosaic), tuple(out_transform)[:6], op.tag, opened[0])
            case RasterOp(tag="mask", mask=(shapes, crop, all_touched, invert)):
                out_image, out_transform = mask.mask(
                    source, list(shapes), crop=crop, all_touched=all_touched, invert=invert, nodata=self.nodata, filled=True
                )
                return self._cover(np.asarray(out_image), tuple(out_transform)[:6], op.tag, source)
            case RasterOp(tag="geometry_mask", geometry_mask=(shapes, out_shape, all_touched, invert)):
                covered = features.geometry_mask(
                    list(shapes), out_shape=out_shape, transform=source.transform, all_touched=all_touched, invert=invert
                )
                return self._cover(np.asarray(covered), tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="sieve", sieve=(size, connectivity)):
                band = source.read(1)
                sieved = features.sieve(band, size=size, connectivity=connectivity)
                return self._cover(np.asarray(sieved), tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="vectorize", vectorize=(connectivity, band)):
                values = source.read(band)
                valid = source.read_masks(band)
                shapes = np.asarray(list(features.shapes(values, mask=valid, connectivity=connectivity, transform=source.transform)), dtype=object)
                return self._cover(shapes, tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="rasterize", rasterize=(shapes, out_shape, merge_alg, all_touched)):
                array = features.rasterize(
                    list(shapes), out_shape=out_shape, transform=source.transform, merge_alg=MergeAlg[merge_alg], all_touched=all_touched
                )
                return self._cover(np.asarray(array), tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="reproject", reproject=(target_crs, resampling)):
                dst_transform, width, height = warp.calculate_default_transform(source.crs, target_crs, source.width, source.height, *source.bounds)
                destination = np.empty((source.count, height, width), dtype=source.dtypes[0])
                warp.reproject(
                    source.read(),
                    destination,
                    src_transform=source.transform,
                    src_crs=source.crs,
                    dst_transform=dst_transform,
                    dst_crs=target_crs,
                    resampling=RioResampling[resampling or self.resampling],
                    dst_nodata=self.nodata,
                )
                return self._cover(destination, tuple(dst_transform)[:6], op.tag, source)
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _cover(array: "np.ndarray", transform: tuple[float, ...], op_tag: str, source: "DatasetReader") -> "_Coverage":
        return _Coverage(array=array, transform=transform, op_tag=op_tag, source=source.name)

    def _result(self, cover: "_Coverage") -> "RuntimeRail[CoverageResult]":
        import numpy as np  # noqa: PLC0415
        import pyarrow as pa  # noqa: PLC0415
        from msgspec import json as msgjson  # noqa: PLC0415

        # content keys hash the REAL coverage bytes — the C-contiguous pixel buffer, or the
        # canonical msgspec-JSON row per feature for the object `Vectorize` array (a `repr()` byte
        # source is the folder key-law deleted form) — never a shape-shaped null placeholder.
        array = cover.array
        payload = (
            b"\x1f".join(msgjson.encode(item) for item in array.reshape(-1).tolist())
            if array.dtype == object
            else np.ascontiguousarray(array).tobytes()
        )
        table = pa.table({"coverage": pa.array([payload], type=pa.binary()), "shape": pa.array([list(array.shape)])})
        return QueryReceipt.railed("rasterio", f"{cover.source}:{cover.op_tag}", table).map(
            lambda receipt: CoverageResult(array=array, transform=cover.transform, receipt=receipt)
        )


class VectorIngress(Struct, frozen=True):
    # OGR pushdown ingress row: selection lands in the driver scan (`columns`/`where`/`bbox`/
    # `mask`/`sql`), never a post-load filter; `use_arrow=True` rides the GDAL Arrow-capable build.
    path: str
    layer: str | None = None
    columns: tuple[str, ...] = ()
    where: str | None = None
    bbox: Bounds | None = None
    mask: object | None = None
    sql: str | None = None
    use_arrow: bool = True


def read_vector(spec: VectorIngress) -> "RuntimeRail[GeoDataFrame]":
    def emit() -> "GeoDataFrame":
        import pyogrio  # noqa: PLC0415

        return pyogrio.read_dataframe(
            spec.path,
            layer=spec.layer,
            columns=list(spec.columns) or None,
            where=spec.where,
            bbox=spec.bbox,
            mask=spec.mask,
            sql=spec.sql,
            use_arrow=spec.use_arrow,
        )

    with _TRACER.start_as_current_span("geo.ingress", attributes={"rasm.geo.op": "ingress"}):
        from pyogrio.errors import DataSourceError  # noqa: PLC0415

        return boundary("geo.ingress", emit, catch=(DataSourceError, OSError, ValueError))


def geoarrow_wire(frame: "GeoDataFrame") -> "RuntimeRail[tuple[pa.Table, Bounds]]":
    # exports zero-copy geoarrow extension arrays the `geoarrow-rust-compute` kernel reads natively;
    # `total_bounds` over the combined-chunk geometry column is the wire-evidence fold.
    def emit() -> "tuple[pa.Table, Bounds]":
        import pyarrow as pa  # noqa: PLC0415
        from geoarrow.rust.compute import total_bounds  # noqa: PLC0415

        table = pa.table(frame.to_arrow(geometry_encoding="geoarrow"))
        return table, tuple(total_bounds(table.column("geometry").combine_chunks()))

    with _TRACER.start_as_current_span("geo.wire.geoarrow", attributes={"rasm.geo.op": "geoarrow"}):
        return boundary("geo.wire.geoarrow", emit, catch=(ValueError, KeyError))
```

## [03]-[COVERAGE]

- Owner: `CoverageCf` — the `rioxarray` CF bridge: `lift` writes the claim CRS and the op-derived affine onto a bare-ndarray coverage through the `.rio` accessor (the CF `grid_mapping` convention, never a hand-copied CRS attribute), and `write_cog` is the LABELLED write — a CF cube lands as a COG without dropping to the bare array, the `odc-stac` coverage cube from `spatial/catalog#ASSETS` round-tripping through the same accessor. `[02]-[GEO]`s `WriteCog` row stays the ndarray-plane egress; each writer owns its carrier, never a second COG writer on either plane.
- Growth: a new CF raster verb is one accessor row; a new COG creation knob threads the `to_raster(**profile)` kwargs; zero new surface.

```python signature
class CoverageCf(Struct, frozen=True):
    crs: str

    def open(self, path: str, *, masked: bool = True, chunks: dict[str, int] | None = None) -> "RuntimeRail[object]":
        def emit() -> object:
            import rioxarray  # noqa: PLC0415

            return rioxarray.open_rasterio(path, masked=masked, chunks=chunks)

        with _TRACER.start_as_current_span("geo.coverage.open", attributes={"rasm.geo.op": "coverage.open"}):
            return boundary("geo.coverage.open", emit, catch=(OSError, ValueError))

    def lift(self, result: CoverageResult, dims: tuple[str, ...] = ("band", "y", "x")) -> "RuntimeRail[object]":
        def emit() -> object:
            import rioxarray  # noqa: F401, PLC0415
            import xarray as xr  # noqa: PLC0415
            from rasterio import Affine  # noqa: PLC0415

            cube = xr.DataArray(result.array, dims=dims[-result.array.ndim :])
            cube = cube.rio.set_spatial_dims(x_dim="x", y_dim="y")
            return cube.rio.write_crs(self.crs).rio.write_transform(Affine(*result.transform))

        with _TRACER.start_as_current_span("geo.coverage.lift", attributes={"rasm.geo.op": "coverage.lift", "rasm.geo.crs": self.crs}):
            return boundary("geo.coverage.lift", emit, catch=(ValueError, KeyError))

    def write_cog(self, cube: object, path: str) -> "RuntimeRail[ContentKey]":
        def emit() -> bytes:
            cube.rio.to_raster(path, driver="COG")
            return Path(path).read_bytes()

        with _TRACER.start_as_current_span("geo.coverage.write_cog", attributes={"rasm.geo.op": "coverage.write_cog"}):
            return boundary("geo.coverage.write_cog", emit, catch=(OSError, ValueError)).bind(lambda payload: ContentIdentity.of("cog", payload))
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

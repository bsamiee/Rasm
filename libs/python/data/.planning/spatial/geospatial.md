# [PY_DATA_GEOSPATIAL]

The geospatial CLAIMS plane — one third of the spatial triptych, beside the `spatial/query#SPATIAL` in-DB engine and the `spatial/grid#GRID` DGG plane, each its own page and codemap charter. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision over geopandas/shapely/pyogrio with pyproj backing the axis-order-aware `reproject` CRS prelude and exposes one `VectorOp` tagged-union in-frame vector-algebra axis (`apply`) — join/overlay/dissolve/clip/construct/predicate PLUS the `Linear` shapely linear-referencing family (`polygonize`/`line_locate_point`/`line_interpolate_point`/`shared_paths`/`shortest_line`) and the `Geodesic` pyproj true-earth measurement family (`Geod.geometry_area_perimeter`/`line_length`, the ellipsoidal values a planar CRS cannot give); `VectorIngress` is the pushdown ingress row — `pyogrio.read_dataframe` with `columns`/`where`/`bbox`/`mask`/`sql` pushed into the OGR scan so filtering happens at the source (the bare-`.dbf` attribute table reads through the same ESRI Shapefile driver, closing the struck-`csvkit` foreign-decode gap). `RasterGeoClaim` carries coverage/band/resampling/nodata/CRS PLUS the `transform` affine tuple the `spatial/catalog#ASSETS` `AssetFold` constructs from `proj:transform` — the claim owner extends, the constructor stops passing a phantom — and exposes one `RasterOp` tagged-union coverage axis (`apply`) spanning the in-memory window/mosaic/mask/geometry-mask/sieve/vectorize/rasterize/reproject ops and the streaming/remote/VRT/in-memory/sample/COG-write rows, each coverage row reporting the transform the operation actually derives from its `source` (or the op-carried target). `EgressFormat` is one `StrEnum` carrying its own `write` — GeoJSON/GeoParquet/FlatGeobuf/GeoArrow, the OGR driver IS the enum value, GeoParquet rides geopandas `to_parquet`, and GEOARROW rides the NATIVE buffer path: `to_arrow(geometry_encoding="geoarrow")` exports zero-copy geoarrow extension arrays serialized as Arrow IPC, never a parquet byte-roundtrip — with `geoarrow_wire` the `geoarrow-rust-compute` buffer hand-off sharing the `csharp:Rasm.Compute` GLB wire layout. The `[03]-[COVERAGE]` cluster is the `rioxarray` CF bridge — `open_rasterio` reads a georeferenced `DataArray`, `CoverageCf.lift` lifts a bare-ndarray `CoverageResult` onto the CF plane through `write_crs`/`write_transform`, and `.rio.to_raster(driver="COG")` closes the CF-side raster-WRITE gap. STAC claims live on `spatial/catalog#CATALOG` (`StacGeoClaim`/`StacIngest` re-homed to the STAC-table owner), so this page holds no catalog import and the former lazy cycle-dodge is dead. Every owner wraps its `boundary` fence in one `_TRACER.start_as_current_span` so each op is an OTel span, and every network-bearing read carries an awaitable `apply_remote` leg that drives the blocking provider call through `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` so a transient remote fault retries on the one `stamina` `HTTP` policy row. Every bundle keys by exactly one runtime `ContentIdentity`, folding the shared `tabular/columnar.md#SCAN` `QueryReceipt`.

## [01]-[INDEX]

- [01]-[GEO]: vector and raster geospatial claims, the in-frame `VectorOp`/`RasterOp` operation axes spanning the in-memory and streaming/remote/VRT/in-memory/sample/sieve/geometry-mask/COG-write rows plus the `Linear`/`Geodesic` families, the `VectorIngress` OGR pushdown row, spatial egress with the native GeoArrow buffer path, and the `geoarrow_wire` Compute-GLB hand-off.
- [02]-[COVERAGE]: the `rioxarray` CF bridge — `open_rasterio` georeferenced read, the bare-ndarray `CoverageResult` lift onto the CF plane, reproject/clip rows, and the `.rio.to_raster(driver="COG")` CF-side COG write.

## [02]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio, with one `VectorOp` tagged-union axis folded by `apply(op, frame)`; `RasterGeoClaim` — coverage/band/resampling/nodata/CRS over rasterio (`resampling` the claim-level default an op overrides through `resampling or self.resampling`), with one `RasterOp` tagged-union axis folded by `apply(op, source)`, each coverage transform derived from the live `source` or the op target rather than a declared claim transform; `EgressFormat` the GeoJSON/GeoParquet/GeoArrow/flat-vector export as one `StrEnum` whose member value is the OGR driver and whose `write` selects `to_parquet` for GeoParquet, the native `to_arrow(geometry_encoding="geoarrow")` Arrow-IPC path for GeoArrow, and `pyogrio.write_dataframe(driver=value)` otherwise, not a per-format writer family.
- Cases: `VectorOp` rows `Join(predicate, other, how, max_distance)` (geopandas `sjoin` for every binary predicate, index-accelerated by `GeoDataFrame.sindex`; `DWITHIN` with a bound is the within-distance relation `sjoin(predicate="dwithin", distance=)` and `DWITHIN` with no bound degenerates to the unbounded `sjoin_nearest` nearest-neighbor join, never `sjoin_nearest` mis-applied as the within-distance relation) · `Overlay(other, how)` (`overlay` intersection/union/difference/symmetric-difference/identity set algebra) · `Dissolve(by, aggfunc)` (`dissolve` spatial group-aggregate) · `Clip(mask, keep_geom_type)` (`clip` mask) · `Construct(kind, param)` (the `ConstructKind` constructive family folded through the one frozen `_CONSTRUCT` behavior table — each row a `(geometry, param)` callable carrying its own positional/keyword/property call shape, never a six-arm `match`) · `Predicate(name, other, distance)` (the vectorized `JoinPredicate` binary-predicate filter against `other.union_all()`, the optional `distance` carried only by the `DWITHIN` within-distance accessor `GeoSeries.dwithin(target, distance)` — every other predicate is geometry-only, so the distance is the one knob the distance-bearing member needs and `None` elsewhere). `RasterOp` is one coverage axis spanning the in-memory and streaming/remote rows: `Window(bounds, boundless)` (`windows.from_bounds` + `DatasetReader.read(window=, boundless=, fill_value=)` bounded read) · `Stream(bidx, tile_shape, resampling)` (`DatasetReader.block_windows(bidx)` tile-aligned generator folding each block straight into one pre-allocated destination slice through `read(window=, out=, resampling=)`, so peak memory is the decimated destination plus one block and no per-tile list ever forms, the `tile_shape` an optional global decimation driving the per-axis `factor` the destination and its `Affine.scale` transform share — the one measured streaming-IO kernel where the `for` over `block_windows` is the platform-forced boundary exemption) · `Sample(coordinates, indexes)` (`DatasetReader.sample(xy, indexes=)` pixel point-query over a coordinate iterable, the timeseries/extraction fast-path) · `Mosaic(sources, method, resampling, bounds, res)` (`merge.merge` with the optional target `bounds`/`res` window the mosaic snaps to) · `Mask(shapes, crop, all_touched, invert)` (`mask.mask`) · `GeometryMask(shapes, out_shape, all_touched, invert)` (`features.geometry_mask` the boolean coverage mask that never reads a band, the lightweight sibling of `mask.mask`) · `Sieve(size, connectivity)` (`features.sieve` minimum-region speckle removal on the first band) · `Vectorize(connectivity, band)` (`features.shapes` raster-to-vector over `read_masks(band)` so nodata never vectorizes) · `Rasterize(shapes, out_shape, merge_alg, all_touched)` (`features.rasterize` vector-to-raster) · `Reproject(target_crs, resampling)` (`warp.calculate_default_transform` + `warp.reproject` eager) · `Vrt(target_crs, resampling, width, height)` (`vrt.WarpedVRT` GDAL-native streamed reproject — a lazy warped view read tile-by-tile, not a second transport) · `RemoteRead(href, vsi_scheme, bounds, overview)` (`Env` GDAL context + a `/vsicurl/`-prefixed VSI path on `rasterio.open` so a remote COG reads its intersecting window over one HTTP range request, the `overview` decimation factor driving `read(out_shape=)` so a coarse pyramid level streams over a tiny range rather than the full resolution, the COG overview fast-path the `out_shape` read the page names binds) · `MemorySource(payload, op)` (`io.MemoryFile` lifts STAC-asset bytes into a dataset and re-folds the inner `RasterOp` without a temp file) · `WriteCog(path, array, transform, crs, profile)` (the typed `CogProfile` projecting its `compress`/`blocksize`/`overviews`/`overview_resampling`/`num_threads`/`predictor` GDAL creation knobs through `CogProfile.creation` + `rasterio.open(mode="w")` + `DatasetWriter.write` cloud-optimized GeoTIFF egress, the `"COG"` driver and the typed creation row replacing a `default_gtiff_profile()`-seeded mutable `dict.update` accumulation). `Linear(kind, other, param)` is the shapely linear-referencing/topology family — `POLYGONIZE` rebuilding a polygon set from a noded line set through `shapely.polygonize`, `LOCATE` the measure-along-line `line_locate_point` against the operand's `union_all()`, `INTERPOLATE` the point-at-measure `line_interpolate_point(param)`, `SHARED_PATHS`/`SHORTEST_LINE` the topological relations against the operand — one case, the kind a row. `Geodesic(kind)` is the pyproj true-earth measurement family — `AREA`/`PERIMETER` off `Geod.geometry_area_perimeter` and `LINE` off `Geod.line_length(*geom.xy)`, the ellipsoidal WGS84 values a planar CRS transform cannot give, landing as one named column per kind. `VectorIngress` is the OGR pushdown ingress row: `pyogrio.read_dataframe(path, layer=, columns=, where=, bbox=, mask=, sql=, use_arrow=)` pushes selection into the driver scan so filtering happens at the source — the bare-`.dbf` attribute table and fixed-width foreign decodes route through this row and `pandas.read_fwf`, the struck-`csvkit` gap closure. `EgressFormat` rows `GEOJSON`/`GEOPARQUET`/`GEOARROW`/`FLATGEOBUF` — the `GEOARROW` arm the NATIVE path (`to_arrow(geometry_encoding="geoarrow")` -> Arrow IPC, never a parquet roundtrip) with `geoarrow_wire` the `geoarrow-rust-compute` zero-copy hand-off (`total_bounds` the wire-evidence fold) sharing the `csharp:Rasm.Compute` GLB layout. A new vector operation is one `VectorOp` case; a new raster operation is one `RasterOp` case; a new linear/geodesic verb is one `LinearKind`/`GeodesicKind` row; a new ingress knob is one `VectorIngress` field the driver already answers; a new constructive op is one `ConstructKind` row; a new spatial predicate is one `JoinPredicate` row; a new resampling mode is one `Resampling` literal arm mapped to `rasterio.enums.Resampling` at the edge; a new VSI scheme is one `VsiScheme` row; a new egress format is one `EgressFormat` member.
- Entry: every owner opens one `_TRACER.start_as_current_span` keyed by `f"geo.<axis>.<tag>"` and carrying the op-discriminant `attributes` (`rasm.geo.op`, the per-claim `rasm.geo.crs`/`rasm.geo.scheme`/`rasm.geo.remote`) around its `boundary` fence so each op is an OTel span the runtime `boundary` `_convert` records the terminal raise on, the tracer resolved off the explicit `trace.get_tracer("rasm.data.spatial.geospatial")` dotted-module handle rather than `__name__` (each spatial page owns its own dotted handle — `spatial/query` and `spatial/grid` mint theirs), and every `boundary` binding the real provider-fault root its body raises (`rasterio.errors.RasterioError`, `shapely.errors.ShapelyError`, `pyproj.exceptions.CRSError`, `pyogrio.errors.DataSourceError`) through `catch=` rather than a bare `Exception`, never a per-op `try`/`except` and never an unspanned or un-narrowed fold. `VectorGeoClaim.apply(op, frame)` folds one `VectorOp` over the live `GeoDataFrame` through the closed `match`/`assert_never` `_vector` kernel, normalizing every binary-operand and source frame through the one `VectorGeoClaim.reproject` pyproj prelude (the no-op short-circuit when the operand already shares the target CRS, `Transformer.from_crs(always_xy=)` honoring `axis_order`, `to_crs` when the transform has an inverse else `set_crs` for a metadata-only label) so `Join`/`Overlay`/`Clip`/`Predicate` operands share one CRS, returning a `RuntimeRail[GeoDataFrame]`; `RasterGeoClaim.apply(op, source)` folds one `RasterOp` over the rasterio source through the `_raster` kernel into a pure `_Coverage` (the NumPy egress — a `(count, rows, cols)` cube, a `(points, bands)` sample matrix, or an object geometry array — its operation-derived affine transform, and the eagerly-captured dataset name) then `.bind`s the railed `_result` whose receipt content key `ContentIdentity` hashes the coverage's real C-contiguous pixel buffer (or per-feature geometry repr for the object `Vectorize` array), never a shape-shaped null placeholder two distinct same-shape rasters would collide on, the self-opening rows (`RemoteRead`/`MemorySource`/`Mosaic`/`WriteCog`) entering their own dataset inside one `ExitStack` and threading their opened handle (`written`/`opened[0]`/`remote`) into `_cover` so the receipt names the real dataset and the GDAL handle closes on the boundary exit before the railed receipt derives, the `Stream` row folding each `block_windows` tile straight into one pre-allocated destination slice through `read(out=)` so no full-resolution copy ever forms; `RasterGeoClaim.apply_remote(op, source=None)` is the awaitable leg the network-bearing rows (`RemoteRead` over `/vsicurl/`, a remote-href `Mosaic`/`MemorySource`) route through, wrapping the blocking `_remote_read` body in `anyio.to_thread.run_sync` under `guarded(RetryClass.HTTP, ...)` so a transient connection/timeout read retries on the one `stamina` `HTTP` policy row — `_remote_read` re-raises a `/vsicurl/` GDAL `rasterio.errors.RasterioIOError` (rooted at `RasterioError`, not a `ConnectionError`/`TimeoutError` subclass) as `ConnectionError` at the geospatial boundary so the runtime `reliability/resilience#RESILIENCE` `HTTP` `target` (the generic `_retry_after(TimeoutError, ConnectionError)` set with the `Retry-After` backoff hook) retries the dropped range read without the resilience owner growing a `rasterio` provider-introspection target — and `guarded` surfaces the budget-exhausted cause as one `RuntimeRail` error before the same `.bind(_result)` railed-receipt projection runs — the same retry/span/lift triplet the `transport/roots#RESOURCE` fetch legs delegate to `guarded`, never a hand-opened retry loop; `EgressFormat.write` runs the side-effecting OGR/GeoParquet write under one `boundary` then `.bind`s the railed `ContentIdentity.of` over the written bytes; the raster path threads the shared `columnar` `QueryReceipt.railed` into `CoverageResult`, keyed by the `ContentIdentity` the railed receipt derives from the result Arrow bytes.
- Packages: `geopandas` (`sjoin`/`sjoin_nearest`/`overlay`/`clip`/`dissolve`/the predicate accessors/`buffer`/`simplify`/`convex_hull`/`concave_hull`/`voronoi_polygons`/`delaunay_triangles`/`sindex`/`to_crs`/`set_crs`/`to_parquet(geometry_encoding=)`/`union_all`), `shapely` (`set_precision` vectorized over the geometry array, `make_valid` constructive backing; `polygonize`/`line_locate_point`/`line_interpolate_point`/`shared_paths`/`shortest_line` the `Linear` family rows), `geoarrow-rust-compute` (`total_bounds` the wire-evidence fold over the natively-exported geoarrow geometry column — the `geoarrow_wire` hand-off the `csharp:Rasm.Compute` GLB seam consumes; `area`/`centroid`/`simplify` the sibling kernel rows the same buffers reach), `pyogrio` (`write_dataframe(driver=, use_arrow=)` the OGR-driver vector write the GeoJSON/FlatGeobuf egress binds directly, not the geopandas `to_file` indirection; `read_dataframe(path, layer=, columns=, where=, bbox=, mask=, sql=, use_arrow=)` the pushdown ingress the `VectorIngress` row threads — selection lands in the OGR scan, never a post-load filter), `pyproj` (`CRS.from_user_input`/`Transformer.from_crs(always_xy=)`/`Transformer.has_inverse` the axis-order-aware CRS prelude `reproject` owns; `Geod(ellps="WGS84")`/`Geod.geometry_area_perimeter`/`Geod.line_length` the `Geodesic` true-earth rows), `rasterio` (`open`/`Env`/`Affine`/`Affine.scale`/`io.MemoryFile`/`windows.from_bounds`/`DatasetReader.read(window=, out=, out_shape=, boundless=, resampling=)`/`DatasetReader.read_masks`/`DatasetReader.block_windows`/`DatasetReader.window_transform`/`DatasetReader.sample`/`io.DatasetWriter.write`/`merge.merge(bounds=, res=)`/`mask.mask`/`features.shapes`/`features.geometry_mask`/`features.sieve`/`features.rasterize`/`warp.reproject`/`warp.calculate_default_transform`/`enums.Resampling`/`enums.MergeAlg`/`vrt.WarpedVRT`/`open(mode="w", driver="COG", ...)` the typed-`CogProfile`-projected COG creation profile),  `numpy` (`full`/`asarray` the raster array egress and the streaming destination, `ascontiguousarray`/`reshape`/`ndarray.tobytes` the C-contiguous pixel-buffer the receipt content key hashes), `pyarrow` (`table`/`array`/`binary` the one-row `coverage`-buffer/`shape` receipt table whose Arrow bytes the `QueryReceipt.railed` content key derives from the REAL coverage content; `feather.write_feather` the GEOARROW native IPC egress), `expression` (`tagged_union`/`tag`/`case` the `VectorOp`/`RasterOp` ADTs, `expression.collections.Map`/`Map.of_seq` the `_CONSTRUCT` constructive behavior table — the folder's ONE dispatch rail, a frozendict-typed table the deleted contrast, the same `Map`-keyed dispatch the sibling `tabular/contract#QUALITY` `_CMP` and `graph#GRAPH` `RX_CENTRALITY` rows own), `anyio` (`to_thread.run_sync` lifting the blocking GDAL/NDJSON remote read into the awaitable `guarded` leg), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-op geo span), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`boundary`/`QueryReceipt`, `RetryClass.HTTP`/`guarded` the remote-read retry envelope that itself owns the `async_boundary` terminal lift, never re-spelled here).
- Growth: a new vector operation is one `VectorOp` case (the next geopandas op, `segmentize`/`offset_curve`, lands as one `ConstructKind` enum row plus its `_CONSTRUCT` behavior row); a new raster operation is one `RasterOp` case (streaming, remote, in-memory, sample, sieve, geometry-mask, and COG-write rows all land on the one axis); a new linear or geodesic verb is one `LinearKind`/`GeodesicKind` row; a new constructive op is one `ConstructKind` row plus its `_CONSTRUCT` behavior row; a new binary predicate is one `JoinPredicate` row; a new resampling mode is one `Resampling` literal arm mapped to `rasterio.enums.Resampling` at the edge; a new VSI scheme is one `VsiScheme` row; a new egress format is one `EgressFormat` member (GeoArrow lands as `GEOARROW`); the DuckDB join/transform/H3 engine is the `spatial/query#SPATIAL` page; the H3/S2 grid plane is the `spatial/grid#GRID` page; a CF coverage write is the `[02]-[COVERAGE]` `CoverageCf` cluster; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape, a `sjoin`/`overlay`/`dissolve`/`do_merge`/`clip_raster`/`rasterize_features`/`stream_tiles`/`read_remote`/`open_memory`/`write_cog`/`read_stac_ndjson` method family, a Python STRtree/`isinstance` spatial-join loop where `GeoDataFrame.sindex` accelerates, a full-raster read where a `Window`/`Stream`/`Vrt` suffices, a band read where `features.geometry_mask` answers coverage without one, a temp-file round-trip where `io.MemoryFile` lifts asset bytes in place, a STAC claim or NDJSON-interchange arm on this page where the `spatial/catalog#CATALOG` owner homes `StacGeoClaim`/`StacIngest`, a parquet byte-roundtrip GEOARROW egress where the native `to_arrow(geometry_encoding="geoarrow")` Arrow-IPC path and the `geoarrow_wire` buffer hand-off own the wire, a post-load filter where the `VectorIngress` OGR pushdown row selects at the source, a planar area/length claim where the `Geodesic` rows own true-earth measurement, a second byte-window transport beside the `tabular/egress` `obstore` rail (`WarpedVRT` is GDAL-native streamed reproject, never a competing transport), a six-arm constructive `match` where the frozen `_CONSTRUCT` behavior table folds the per-kind call shape, a tagged union whose case payload only restates its tag, a per-format writer family, an unspanned remote-href read or a hand-opened `for attempt in range(n): sleep(...)` retry loop where the `_TRACER` span and the `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` envelope own observability and transient-retry, a second `RetryConfig`-style sleep path duplicating the one `stamina` `HTTP` policy row, an un-narrowed `catch=Exception` boundary where the fence binds its real provider-fault root (`RasterioError`/`ShapelyError`/`CRSError`/`DataSourceError`), an attribute-free span dropping the `rasm.geo.op`/`rasm.geo.crs` op discriminant, a `pa.nulls`-shaped receipt table keying the coverage `ContentIdentity` off a `(bands, rows)` null placeholder two distinct same-shape rasters collide on where the C-contiguous pixel buffer is the real content the key must hash, a per-op `resampling="nearest"` factory literal re-deriving at every row where `self.resampling` is the claim-level default an op overrides through `resampling or self.resampling`, a stale claim-declared `transform` field no operation reads where each coverage reports its `source`-derived or op-target transform, and a `trace.get_tracer(__name__)` where the sibling owners resolve the explicit `"rasm.data.spatial.geospatial"` dotted module string are the deleted forms. The content key derives from the real result bytes through exactly one canonical `ContentIdentity.of`. geopandas/shapely/rasterio ride the Forge scientific source build band, so every operation body binds the provider function-local under `# noqa: PLC0415`, never a subprocess seam; the STAC-interchange providers are bound only inside the `spatial/catalog#CATALOG` owner.

```python signature
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
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
                    # NATIVE GeoArrow: zero-copy geoarrow extension arrays serialized as Arrow IPC —
                    # the parquet byte-roundtrip is the deleted form; the same buffers feed
                    # `geoarrow_wire` at the Rasm.Compute GLB seam.
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

    def apply(self, op: VectorOp, frame: "GeoDataFrame") -> "RuntimeRail[GeoDataFrame]":
        from pyproj.exceptions import CRSError  # noqa: PLC0415
        from shapely.errors import ShapelyError  # noqa: PLC0415

        with _TRACER.start_as_current_span(f"geo.vector.{op.tag}", attributes={"rasm.geo.crs": self.crs, "rasm.geo.op": op.tag}):
            return boundary(f"geo.vector.{op.tag}", lambda: self._vector(op, frame), catch=(ShapelyError, CRSError, KeyError, ValueError))

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

                # the WGS84 ellipsoid true-earth values a planar CRS transform cannot give; the
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
    # the source affine — six coefficients (or empty when unknown) the `spatial/catalog#ASSETS`
    # AssetFold constructs from `proj:transform`; each coverage op still REPORTS the transform its
    # operation derives, so this claim slot is provenance, never a stale substitute for the op result.
    transform: tuple[float, ...] = ()

    def apply(self, op: RasterOp, source: "DatasetReader") -> "RuntimeRail[CoverageResult]":
        from rasterio.errors import RasterioError  # noqa: PLC0415

        with _TRACER.start_as_current_span(
            f"geo.raster.{op.tag}",
            attributes={"rasm.geo.crs": self.crs, "rasm.geo.op": op.tag, "rasm.geo.bands": self.band_count, "rasm.geo.resampling": self.resampling},
        ):
            return boundary(f"geo.raster.{op.tag}", lambda: self._raster(op, source), catch=(RasterioError, ValueError)).bind(self._result)

    async def apply_remote(self, op: RasterOp, source: "DatasetReader | None" = None) -> "RuntimeRail[CoverageResult]":
        import anyio  # noqa: PLC0415

        with _TRACER.start_as_current_span(f"geo.raster.{op.tag}", attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag}):
            acquired = await guarded(RetryClass.HTTP, anyio.to_thread.run_sync, lambda: self._remote_read(op, source), subject=f"geo.raster.{op.tag}")
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

        # the content key hashes the REAL coverage bytes — the C-contiguous pixel buffer, or the
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
    # the OGR pushdown ingress row: selection lands in the driver scan (`columns`/`where`/`bbox`/
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
    # the native GeoArrow buffer hand-off sharing the Rasm.Compute GLB wire layout: the frame
    # exports zero-copy geoarrow extension arrays and the `geoarrow-rust-compute` kernel reads
    # them natively (`total_bounds` the wire-evidence fold) — no parquet serialize/deserialize
    # roundtrip stands between the frame and the wire.
    def emit() -> "tuple[pa.Table, Bounds]":
        import pyarrow as pa  # noqa: PLC0415
        from geoarrow.rust.compute import total_bounds  # noqa: PLC0415

        table = pa.table(frame.to_arrow(geometry_encoding="geoarrow"))
        return table, tuple(total_bounds(table.column("geometry").combine_chunks()))

    with _TRACER.start_as_current_span("geo.wire.geoarrow", attributes={"rasm.geo.op": "geoarrow"}):
        return boundary("geo.wire.geoarrow", emit, catch=(ValueError, KeyError))
```

## [03]-[COVERAGE]

- Owner: `CoverageCf` — the `rioxarray` CF bridge closing the CF-side raster gap: `open` reads a georeferenced `xarray.DataArray` through `rioxarray.open_rasterio` (auto pixel-center coordinates, `masked=`/`chunks=` the lazy/mask knobs), `lift` lifts a bare-ndarray `CoverageResult` (the `[01]-[GEO]` raster egress) onto the CF plane by writing the claim CRS and the op-derived affine through the `.rio` accessor (`write_crs`/`write_transform`, the CF `grid_mapping` convention), `reproject`/`clip` compose the accessor's GIS rows, and `write_cog` closes the CF-side raster-WRITE gap through `.rio.to_raster(path, driver="COG")` keyed by `ContentIdentity` over the written bytes. The rasterio `WriteCog` row on `[01]-[GEO]` stays the in-plane ndarray egress; this cluster is the LABELLED write — a CF cube with dims/coords/CRS metadata lands as a COG without dropping to the bare array, the `odc-stac` coverage cube (`spatial/catalog#ASSETS`) round-tripping through the same accessor.
- Entry: every entry opens one `_TRACER.start_as_current_span(f"geo.coverage.<op>")` around a `catch=`-narrowed `boundary`; `lift` returns the accessor-carrying `DataArray`, `write_cog` binds the railed `ContentIdentity.of` over the written file bytes into a `RuntimeRail[ContentKey]` — the same written-bytes key discipline `EgressFormat.write` holds.
- Packages: `rioxarray` (`open_rasterio(filename, *, parse_coordinates=, chunks=, masked=, mask_and_scale=)`/`XRasterBase.write_crs(input_crs)`/`write_transform(transform)`/`set_spatial_dims(x_dim, y_dim)`/`RasterArray.reproject(dst_crs, resampling=)`/`reproject_match`/`clip`/`to_raster(path, driver=, **profile)` — the `.rio` accessor registers on import), `xarray` (`DataArray` the CF carrier, banned-module-level, function-local), `rasterio` (`Affine` the transform lift), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`).
- Growth: a new CF raster verb is one accessor row on this cluster; a new COG creation knob threads the `to_raster(**profile)` kwargs; zero new surface.
- Boundary: rioxarray owns the CF↔rasterio bridge and the `.rio` write path; the bare-ndarray raster ops stay `[01]-[GEO]` `RasterOp`; no second COG writer beside `WriteCog` (ndarray plane) and `write_cog` (CF plane) — each owns its carrier; a hand-copied CRS attribute where `write_crs` writes the CF `grid_mapping`, and an unlabelled ndarray round-trip where the CF cube carries dims/coords are the deleted forms.

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

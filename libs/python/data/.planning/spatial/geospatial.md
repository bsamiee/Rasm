# [PY_DATA_GEOSPATIAL]

The three-axis geospatial surface, spatial egress, the columnar spatial-query engine, and the discrete-global-grid plane. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision over geopandas/shapely/pyogrio with pyproj backing the axis-order-aware `reproject` CRS prelude and exposes one `VectorOp` tagged-union in-frame vector-algebra axis (`apply`); `RasterGeoClaim` carries coverage/band/resampling/nodata/CRS over rasterio — `resampling` the claim-level default an op row overrides only when it carries its own (`resampling or self.resampling`), never a per-op `"nearest"` literal re-derived at every factory — and exposes one `RasterOp` tagged-union coverage axis (`apply`) spanning the in-memory window/mosaic/mask/geometry-mask/sieve/vectorize/rasterize/reproject ops and the streaming/remote/VRT/in-memory/sample/COG-write rows, each coverage row reporting the transform the operation actually derives from its `source` (or the op-carried target), never a stale claim-declared transform; `StacGeoClaim` folds one `StacIngest` axis (`ToArrow`/`ToDelta`/`Rehydrate`) by composing the canonical `spatial/catalog#TABLE` `stac_table`/`stac_table_direct`/`stac_table_rehydrate` STAC-interchange owner — the geospatial-tier claim re-binds no `stac_geoparquet.arrow` surface, wrapping the catalog owner's railed result in its own `QueryReceipt`/span/`apply_remote` retry rail. Vector and raster are two value objects with two distinct operation axes because band/resampling/window semantics differ from vector CRS/axis-order; the operation knobs (`how`/`predicate`/`aggfunc`/`max_distance`/`method`/`crop`/`merge_alg`/`resampling`/`tile_shape`/`vsi_scheme`/`profile`) are case payload rows, never a `sjoin`/`overlay`/`do_merge`/`clip_raster`/`stream_tiles`/`read_remote`/`write_cog` method family. `EgressFormat` is one `StrEnum` carrying its own `write` — GeoJSON/GeoParquet/FlatGeobuf/GeoArrow, the OGR driver IS the enum value, GeoParquet rides geopandas `to_parquet` and GeoArrow rides `to_parquet(geometry_encoding="geoarrow")` — not a tagged union over redundant payloads or a per-format writer family. `SpatialQuery` is one tagged-union axis over the DuckDB engine whose every case projects through one `QueryPlan` fold carrying SQL, bound parameters, the extension requirement, and the predicate count in a single traversal — in DuckDB 1.5.4 every `ST_` function lives in the `spatial` extension, so `spatial` is the unconditional `install_extension("spatial", repository=None)`-then-`load_extension` geometry-view prelude the engine loads before any `ST_GeomFromWKB`/`ST_Intersects`/`ST_Transform` resolves, and `h3` rides the community repository on top of it; the bbox-cached `SPATIAL_JOIN` operator is the automatic join prefilter, and the spatial engine owns the extension-load and `ST_GeomFromWKB` geometry-view prelude the generic `query` engine lacks, threading the railed `QueryReceipt` law rather than routing through `QueryEngine.Sql`. `GridSystem` is one discrete-global-grid owner over an H3/S2 axis whose cell indexing/measurement/traversal/hierarchy fold as vectorized polars expressions over h3ronpy Arrow cell ops and polars-st spatial frames, one `CellKind` axis collapsing the parallel cell/vertex/edge parse-validate-stringify families and the `raster` bridge lifting numpy coverage into cells, composing the DuckDB `h3` SQL path for the engine leg. Every owner wraps its `boundary` fence in one `_TRACER.start_as_current_span` so each op is an OTel span, and every network-bearing read (`RasterOp.RemoteRead`, the remote-NDJSON `StacIngest` rows) carries an awaitable `apply_remote` leg that drives the blocking provider call through `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` so a transient remote fault retries on the one `stamina` `HTTP` policy row — the retry/span/lift triplet delegated to the runtime resilience owner, never a hand-rolled loop. Every bundle keys by exactly one runtime `ContentIdentity`, folding the shared `tabular/columnar.md#SCAN` `QueryReceipt`.

## [01]-[INDEX]

- [01]-[GEO]: vector and raster geospatial claims, the in-frame `VectorOp`/`RasterOp` operation axes spanning the in-memory and streaming/remote/VRT/in-memory/sample/sieve/geometry-mask/COG-write rows, the `StacIngest` STAC-NDJSON Arrow/Delta interchange axis composing the `spatial/catalog#TABLE` `stac_table` owner, spatial egress, the GeoArrow encoding.
- [02]-[SPATIAL]: the DuckDB join/transform/H3 columnar query engine over the `spatial`-extension `GEOMETRY` type, the core-repository `spatial` extension loaded as the unconditional geometry-view prelude, the community-repository `h3` extension, the one `QueryPlan` projection folding SQL/parameters/extensions/predicate-count, and the bbox-cached `SPATIAL_JOIN` prefilter.
- [03]-[GRID]: the `GridSystem` discrete-global-grid owner over the H3/S2 axis as vectorized polars-native cell ops on h3ronpy + polars-st, the `CellKind` cell/vertex/edge collapse and the bidirectional `raster` numpy↔cell bridge, composing the DuckDB `h3` SQL path.

## [02]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio, with one `VectorOp` tagged-union axis folded by `apply(op, frame)`; `RasterGeoClaim` — coverage/band/resampling/nodata/CRS over rasterio (`resampling` the claim-level default an op overrides through `resampling or self.resampling`), with one `RasterOp` tagged-union axis folded by `apply(op, source)`, each coverage transform derived from the live `source` or the op target rather than a declared claim transform; `EgressFormat` the GeoJSON/GeoParquet/flat-vector export as one `StrEnum` whose member value is the OGR driver and whose `write` selects `to_parquet` for GeoParquet and `to_file(driver=value)` otherwise, not a per-format writer family.
- Cases: `VectorOp` rows `Join(predicate, other, how, max_distance)` (geopandas `sjoin` for every binary predicate, index-accelerated by `GeoDataFrame.sindex`; `DWITHIN` with a bound is the within-distance relation `sjoin(predicate="dwithin", distance=)` and `DWITHIN` with no bound degenerates to the unbounded `sjoin_nearest` nearest-neighbor join, never `sjoin_nearest` mis-applied as the within-distance relation) · `Overlay(other, how)` (`overlay` intersection/union/difference/symmetric-difference/identity set algebra) · `Dissolve(by, aggfunc)` (`dissolve` spatial group-aggregate) · `Clip(mask, keep_geom_type)` (`clip` mask) · `Construct(kind, param)` (the `ConstructKind` constructive family folded through the one frozen `_CONSTRUCT` behavior table — each row a `(geometry, param)` callable carrying its own positional/keyword/property call shape, never a six-arm `match`) · `Predicate(name, other, distance)` (the vectorized `JoinPredicate` binary-predicate filter against `other.union_all()`, the optional `distance` carried only by the `DWITHIN` within-distance accessor `GeoSeries.dwithin(target, distance)` — every other predicate is geometry-only, so the distance is the one knob the distance-bearing member needs and `None` elsewhere). `RasterOp` is one coverage axis spanning the in-memory and streaming/remote rows: `Window(bounds, boundless)` (`windows.from_bounds` + `DatasetReader.read(window=, boundless=, fill_value=)` bounded read) · `Stream(bidx, tile_shape, resampling)` (`DatasetReader.block_windows(bidx)` tile-aligned generator folding each block straight into one pre-allocated destination slice through `read(window=, out=, resampling=)`, so peak memory is the decimated destination plus one block and no per-tile list ever forms, the `tile_shape` an optional global decimation driving the per-axis `factor` the destination and its `Affine.scale` transform share — the one measured streaming-IO kernel where the `for` over `block_windows` is the platform-forced boundary exemption) · `Sample(coordinates, indexes)` (`DatasetReader.sample(xy, indexes=)` pixel point-query over a coordinate iterable, the timeseries/extraction fast-path) · `Mosaic(sources, method, resampling, bounds, res)` (`merge.merge` with the optional target `bounds`/`res` window the mosaic snaps to) · `Mask(shapes, crop, all_touched, invert)` (`mask.mask`) · `GeometryMask(shapes, out_shape, all_touched, invert)` (`features.geometry_mask` the boolean coverage mask that never reads a band, the lightweight sibling of `mask.mask`) · `Sieve(size, connectivity)` (`features.sieve` minimum-region speckle removal on the first band) · `Vectorize(connectivity, band)` (`features.shapes` raster-to-vector over `read_masks(band)` so nodata never vectorizes) · `Rasterize(shapes, out_shape, merge_alg, all_touched)` (`features.rasterize` vector-to-raster) · `Reproject(target_crs, resampling)` (`warp.calculate_default_transform` + `warp.reproject` eager) · `Vrt(target_crs, resampling, width, height)` (`vrt.WarpedVRT` GDAL-native streamed reproject — a lazy warped view read tile-by-tile, not a second transport) · `RemoteRead(href, vsi_scheme, bounds, overview)` (`Env` GDAL context + a `/vsicurl/`-prefixed VSI path on `rasterio.open` so a remote COG reads its intersecting window over one HTTP range request, the `overview` decimation factor driving `read(out_shape=)` so a coarse pyramid level streams over a tiny range rather than the full resolution, the COG overview fast-path the `out_shape` read the page names binds) · `MemorySource(payload, op)` (`io.MemoryFile` lifts STAC-asset bytes into a dataset and re-folds the inner `RasterOp` without a temp file) · `WriteCog(path, array, transform, crs, profile)` (the typed `CogProfile` projecting its `compress`/`blocksize`/`overviews`/`overview_resampling`/`num_threads`/`predictor` GDAL creation knobs through `CogProfile.creation` + `rasterio.open(mode="w")` + `DatasetWriter.write` cloud-optimized GeoTIFF egress, the `"COG"` driver and the typed creation row replacing a `default_gtiff_profile()`-seeded mutable `dict.update` accumulation). `StacIngest` is one STAC-NDJSON interchange axis composing the canonical `spatial/catalog#TABLE` `stac_table` owner: `ToArrow(path, schema, limit)` (`stac_table(TableSource.Ndjson(...), schema=)` the NDJSON-to-`RecordBatchReader` source threading the `SchemaInference` row), `ToDelta(input_path, table_uri)` (`stac_table_direct(TableSource.Ndjson(...), TableSink.DeltaLake(...))` the one-call Delta sink), `Rehydrate(table)` (`stac_table_rehydrate` yielding rebuilt `pystac.Item` objects), each a railed catalog call this claim wraps in the shared `QueryReceipt` keyed by one `ContentIdentity`. `EgressFormat` rows `GEOJSON`/`GEOPARQUET`/`GEOARROW`/`FLATGEOBUF`. A new vector operation is one `VectorOp` case; a new raster operation is one `RasterOp` case; a new STAC interchange row is one `StacIngest` case; a new constructive op is one `ConstructKind` row; a new spatial predicate is one `JoinPredicate` row; a new resampling mode is one `Resampling` literal arm mapped to `rasterio.enums.Resampling` at the edge; a new VSI scheme is one `VsiScheme` row; a new egress format is one `EgressFormat` member.
- Entry: every owner opens one `_TRACER.start_as_current_span` keyed by `f"geo.<axis>.<tag>"` and carrying the op-discriminant `attributes` (`rasm.geo.op`, the per-claim `rasm.geo.crs`/`rasm.geo.scheme`/`rasm.geo.remote`) around its `boundary` fence so each op is an OTel span the runtime `boundary` `_convert` records the terminal raise on, the tracer resolved off the explicit `trace.get_tracer("rasm.data.spatial.geospatial")` dotted-module handle the sibling owners share rather than `__name__`, and every `boundary` binding the real provider-fault root its body raises (`rasterio.errors.RasterioError`, `shapely.errors.ShapelyError`, `pyproj.exceptions.CRSError`, `pyogrio.errors.DataSourceError`, `duckdb.Error`, the h3ronpy FFI `ValueError`/`KeyError`/`RuntimeError` plus the page-owned `NotImplementedError` S2 deferral) through `catch=` rather than a bare `Exception`, never a per-op `try`/`except` and never an unspanned or un-narrowed fold. `VectorGeoClaim.apply(op, frame)` folds one `VectorOp` over the live `GeoDataFrame` through the closed `match`/`assert_never` `_vector` kernel, normalizing every binary-operand and source frame through the one `VectorGeoClaim.reproject` pyproj prelude (the no-op short-circuit when the operand already shares the target CRS, `Transformer.from_crs(always_xy=)` honoring `axis_order`, `to_crs` when the transform has an inverse else `set_crs` for a metadata-only label) so `Join`/`Overlay`/`Clip`/`Predicate` operands share one CRS, returning a `RuntimeRail[GeoDataFrame]`; `RasterGeoClaim.apply(op, source)` folds one `RasterOp` over the rasterio source through the `_raster` kernel into a pure `_Coverage` (the NumPy egress — a `(count, rows, cols)` cube, a `(points, bands)` sample matrix, or an object geometry array — its operation-derived affine transform, and the eagerly-captured dataset name) then `.bind`s the railed `_result` whose receipt content key `ContentIdentity` hashes the coverage's real C-contiguous pixel buffer (or per-feature geometry repr for the object `Vectorize` array), never a shape-shaped null placeholder two distinct same-shape rasters would collide on, the self-opening rows (`RemoteRead`/`MemorySource`/`Mosaic`/`WriteCog`) entering their own dataset inside one `ExitStack` and threading their opened handle (`written`/`opened[0]`/`remote`) into `_cover` so the receipt names the real dataset and the GDAL handle closes on the boundary exit before the railed receipt derives, the `Stream` row folding each `block_windows` tile straight into one pre-allocated destination slice through `read(out=)` so no full-resolution copy ever forms; `RasterGeoClaim.apply_remote(op, source=None)` is the awaitable leg the network-bearing rows (`RemoteRead` over `/vsicurl/`, a remote-href `Mosaic`/`MemorySource`) route through, wrapping the blocking `_remote_read` body in `anyio.to_thread.run_sync` under `guarded(RetryClass.HTTP, ...)` so a transient connection/timeout read retries on the one `stamina` `HTTP` policy row — `_remote_read` re-raises a `/vsicurl/` GDAL `rasterio.errors.RasterioIOError` (rooted at `RasterioError`, not a `ConnectionError`/`TimeoutError` subclass) as `ConnectionError` at the geospatial boundary so the runtime `reliability/resilience#RESILIENCE` `HTTP` `target` (the generic `_retry_after(TimeoutError, ConnectionError)` set with the `Retry-After` backoff hook) retries the dropped range read without the resilience owner growing a `rasterio` provider-introspection target — and `guarded` surfaces the budget-exhausted cause as one `RuntimeRail` error before the same `.bind(_result)` railed-receipt projection runs — the same retry/span/lift triplet the `transport/roots#RESOURCE` fetch legs delegate to `guarded`, never a hand-opened retry loop; `StacGeoClaim.apply(op)` folds one `StacIngest` by composing the canonical `spatial/catalog#TABLE` `stac_table`/`stac_table_direct`/`stac_table_rehydrate` owner — `_stac` returns the catalog owner's `RuntimeRail[StacPayload]` (the `ToArrow` `stac_table(TableSource.Ndjson(...))` `RecordBatchReader.read_all()` table, the `ToDelta` `stac_table_direct(..., TableSink.DeltaLake(...))` `table_uri`, or the `stac_table_rehydrate` rebuilt-`pystac.Item` tuple) then `.bind`s the railed `_result`, with `StacGeoClaim.apply_remote(op)` the awaitable leg the remote-NDJSON `ToArrow`/`ToDelta` reads route through under the same `guarded(RetryClass.HTTP, ...)` envelope, the catalog owner the single binding of the `stac_geoparquet.arrow` surface this claim never re-binds; `EgressFormat.write` runs the side-effecting OGR/GeoParquet write under one `boundary` then `.bind`s the railed `ContentIdentity.of` over the written bytes; the raster, STAC, and grid paths thread the shared `columnar` `QueryReceipt.railed` into `CoverageResult`/`StacResult`/`GridResult`, each keyed by the `ContentIdentity` the railed receipt derives from the result Arrow bytes.
- Packages: `geopandas` (`sjoin`/`sjoin_nearest`/`overlay`/`clip`/`dissolve`/the predicate accessors/`buffer`/`simplify`/`convex_hull`/`concave_hull`/`voronoi_polygons`/`delaunay_triangles`/`sindex`/`to_crs`/`set_crs`/`to_parquet(geometry_encoding=)`/`union_all`), `shapely` (`set_precision` vectorized over the geometry array, `make_valid` constructive backing), `pyogrio` (`write_dataframe(driver=, use_arrow=)` the OGR-driver vector write the GeoJSON/FlatGeobuf egress binds directly, not the geopandas `to_file` indirection), `pyproj` (`CRS.from_user_input`/`Transformer.from_crs(always_xy=)`/`Transformer.has_inverse` the axis-order-aware CRS prelude `reproject` owns), `rasterio` (`open`/`Env`/`Affine`/`Affine.scale`/`io.MemoryFile`/`windows.from_bounds`/`DatasetReader.read(window=, out=, out_shape=, boundless=, resampling=)`/`DatasetReader.read_masks`/`DatasetReader.block_windows`/`DatasetReader.window_transform`/`DatasetReader.sample`/`io.DatasetWriter.write`/`merge.merge(bounds=, res=)`/`mask.mask`/`features.shapes`/`features.geometry_mask`/`features.sieve`/`features.rasterize`/`warp.reproject`/`warp.calculate_default_transform`/`enums.Resampling`/`enums.MergeAlg`/`vrt.WarpedVRT`/`open(mode="w", driver="COG", ...)` the typed-`CogProfile`-projected COG creation profile), `spatial/catalog#TABLE` (`stac_table`/`stac_table_direct`/`stac_table_rehydrate`/`TableSource`/`TableSink` the canonical STAC-NDJSON interchange owner the `StacIngest` axis composes rather than re-binding `stac_geoparquet.arrow`), `numpy` (`full`/`asarray` the raster array egress and the streaming destination, `ascontiguousarray`/`reshape`/`ndarray.tobytes` the C-contiguous pixel-buffer the receipt content key hashes), `pyarrow` (`table`/`array`/`binary` the one-row `coverage`-buffer/`shape` receipt table whose Arrow bytes the `QueryReceipt.railed` content key derives from the REAL coverage content, `Table.from_pylist` the rehydrated-items lowering, `pystac.Item.to_dict` lowering the rehydrated items), `expression` (`tagged_union`/`tag`/`case` the `VectorOp`/`RasterOp`/`StacIngest`/`StacPayload` ADTs, `expression.collections.Map`/`Map.of_seq` the `_CONSTRUCT` constructive behavior table — never a `from builtins import frozendict` table for callable behavior, the same `Map`-keyed dispatch the sibling `tabular/contract#QUALITY` `_CMP` and `graph#ANALYSIS` `RX_CENTRALITY` rows own), `duckdb` (the `[03]-[SPATIAL]` engine), `anyio` (`to_thread.run_sync` lifting the blocking GDAL/NDJSON remote read into the awaitable `guarded` leg), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-op geo span), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`boundary`/`QueryReceipt`, `RetryClass.HTTP`/`guarded` the remote-read retry envelope that itself owns the `async_boundary` terminal lift, never re-spelled here).
- Growth: a new vector operation is one `VectorOp` case (the next geopandas op, `segmentize`/`offset_curve`, lands as one `ConstructKind` enum row plus its `_CONSTRUCT` behavior row); a new raster operation is one `RasterOp` case (streaming, remote, in-memory, sample, sieve, geometry-mask, and COG-write rows all land on the one axis); a new STAC interchange row is one `StacIngest` case; a new constructive op is one `ConstructKind` row plus its `_CONSTRUCT` behavior row; a new binary predicate is one `JoinPredicate` row; a new resampling mode is one `Resampling` literal arm mapped to `rasterio.enums.Resampling` at the edge; a new VSI scheme is one `VsiScheme` row; a new egress format is one `EgressFormat` member (GeoArrow lands as `GEOARROW`); the DuckDB join/transform/H3 engine is the `[03]-[SPATIAL]` `SpatialQuery` axis; the H3/S2 grid plane is the `[04]-[GRID]` `GridSystem` axis; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape, a `sjoin`/`overlay`/`dissolve`/`do_merge`/`clip_raster`/`rasterize_features`/`stream_tiles`/`read_remote`/`open_memory`/`write_cog`/`read_stac_ndjson` method family, a Python STRtree/`isinstance` spatial-join loop where `GeoDataFrame.sindex` accelerates, a full-raster read where a `Window`/`Stream`/`Vrt` suffices, a band read where `features.geometry_mask` answers coverage without one, a temp-file round-trip where `io.MemoryFile` lifts asset bytes in place, a second `stac_geoparquet.arrow` binding where the `spatial/catalog#TABLE` `stac_table`/`stac_table_direct`/`stac_table_rehydrate` owner is the one STAC-interchange surface this claim composes, a hand-rolled parquet writer where the catalog `stac_table_direct` Delta sink writes it, a re-minted STAC item where the catalog `stac_table_rehydrate` rebuilds it, a second byte-window transport beside the `tabular/egress` `obstore` rail (`WarpedVRT` is GDAL-native streamed reproject, never a competing transport), a six-arm constructive `match` where the frozen `_CONSTRUCT` behavior table folds the per-kind call shape, a tagged union whose case payload only restates its tag, a per-format writer family, an unspanned remote-href read or a hand-opened `for attempt in range(n): sleep(...)` retry loop where the `_TRACER` span and the `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` envelope own observability and transient-retry, a second `RetryConfig`-style sleep path duplicating the one `stamina` `HTTP` policy row, an un-narrowed `catch=Exception` boundary where the fence binds its real provider-fault root (`RasterioError`/`ShapelyError`/`CRSError`/`DataSourceError`), an attribute-free span dropping the `rasm.geo.op`/`rasm.geo.crs` op discriminant, a `pa.nulls`-shaped receipt table keying the coverage `ContentIdentity` off a `(bands, rows)` null placeholder two distinct same-shape rasters collide on where the C-contiguous pixel buffer is the real content the key must hash, a per-op `resampling="nearest"` factory literal re-deriving at every row where `self.resampling` is the claim-level default an op overrides through `resampling or self.resampling`, a stale claim-declared `transform` field no operation reads where each coverage reports its `source`-derived or op-target transform, and a `trace.get_tracer(__name__)` where the sibling owners resolve the explicit `"rasm.data.spatial.geospatial"` dotted module string are the deleted forms. The content key derives from the real result bytes through exactly one canonical `ContentIdentity.of`. geopandas/shapely/rasterio ride the Forge scientific source build band, so every operation body binds the provider function-local under `# noqa: PLC0415`, never a subprocess seam; the STAC-interchange providers are bound only inside the `spatial/catalog#TABLE` owner this claim composes.

```python signature
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
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
# the `spatial/catalog#TABLE` schema-inference literal the composed `stac_table` owner keys.
type SchemaInference = Literal["FullFile", "FirstBatch"]

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
                    frame.to_parquet(path, geometry_encoding="geoarrow")
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
    tag: Literal["join", "overlay", "dissolve", "clip", "construct", "predicate"] = tag()
    join: tuple[JoinPredicate, "GeoDataFrame", JoinHow, float | None] = case()
    overlay: tuple["GeoDataFrame", SetOp] = case()
    dissolve: tuple[tuple[str, ...], str] = case()
    clip: tuple["GeoDataFrame", bool] = case()
    construct: tuple[ConstructKind, float] = case()
    predicate: tuple[JoinPredicate, "GeoDataFrame", float | None] = case()

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
            case unreachable:
                assert_never(unreachable)


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: Resampling
    nodata: float

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

        # the content key hashes the REAL coverage bytes — the C-contiguous pixel buffer, or the
        # per-feature repr for the object `Vectorize` array — never a shape-shaped null placeholder.
        array = cover.array
        payload = "\x1f".join(repr(item) for item in array.reshape(-1)).encode() if array.dtype == object else np.ascontiguousarray(array).tobytes()
        table = pa.table({"coverage": pa.array([payload], type=pa.binary()), "shape": pa.array([list(array.shape)])})
        return QueryReceipt.railed("rasterio", f"{cover.source}:{cover.op_tag}", table).map(
            lambda receipt: CoverageResult(array=array, transform=cover.transform, receipt=receipt)
        )


@tagged_union(frozen=True)
class StacIngest:
    tag: Literal["to_arrow", "to_delta", "rehydrate"] = tag()
    to_arrow: tuple[str, SchemaInference, int | None] = case()
    to_delta: tuple[str, str] = case()
    rehydrate: "pa.Table" = case()

    @staticmethod
    def ToArrow(path: str, schema: SchemaInference = "FullFile", limit: int | None = None) -> "StacIngest":
        return StacIngest(to_arrow=(path, schema, limit))

    @staticmethod
    def ToDelta(input_path: str, table_uri: str) -> "StacIngest":
        return StacIngest(to_delta=(input_path, table_uri))

    @staticmethod
    def Rehydrate(table: "pa.Table") -> "StacIngest":
        return StacIngest(rehydrate=table)


@tagged_union(frozen=True)
class StacPayload:
    tag: Literal["arrow", "delta", "items"] = tag()
    arrow: "pa.Table" = case()
    delta: str = case()
    items: tuple[object, ...] = case()


class StacResult(Struct, frozen=True):
    payload: StacPayload
    receipt: QueryReceipt


class StacGeoClaim(Struct, frozen=True):
    def apply(self, op: StacIngest) -> "RuntimeRail[StacResult]":
        with _TRACER.start_as_current_span(f"geo.stac.{op.tag}", attributes={"rasm.geo.op": op.tag}):
            return self._stac(op).bind(lambda payload: self._result(payload, op.tag))

    async def apply_remote(self, op: StacIngest) -> "RuntimeRail[StacResult]":
        import anyio  # noqa: PLC0415

        with _TRACER.start_as_current_span(f"geo.stac.{op.tag}", attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag}):
            acquired = await guarded(RetryClass.HTTP, anyio.to_thread.run_sync, lambda: self._stac(op), subject=f"geo.stac.{op.tag}")
            # `_stac` is itself railed, so `guarded` over it yields `RuntimeRail[RuntimeRail[...]]`; the
            # identity `bind` is the monadic join that flattens the doubled rail before projection.
            return acquired.bind(lambda inner: inner).bind(lambda payload: self._result(payload, op.tag))

    def _stac(self, op: StacIngest) -> "RuntimeRail[StacPayload]":
        # the STAC-NDJSON interchange is the `spatial/catalog#TABLE` `stac_table` owner — this
        # geospatial-tier claim composes that one bounded owner rather than re-binding the
        # `stac_geoparquet.arrow` surface a second time, then wraps the railed result in its own
        # `QueryReceipt`/span/`apply_remote` retry rail at `_result`.
        from rasm.data.catalog import TableSink, TableSource, stac_table, stac_table_direct, stac_table_rehydrate  # noqa: PLC0415

        match op:
            case StacIngest(tag="to_arrow", to_arrow=(path, schema, limit)):
                return stac_table(TableSource.Ndjson(path, limit), schema=schema).map(lambda reader: StacPayload(arrow=reader.read_all()))
            case StacIngest(tag="to_delta", to_delta=(input_path, table_uri)):
                return stac_table_direct(TableSource.Ndjson(input_path), TableSink.DeltaLake(table_uri)).map(
                    lambda _key: StacPayload(delta=table_uri)
                )
            case StacIngest(tag="rehydrate", rehydrate=table):
                return stac_table_rehydrate(table).map(lambda items: StacPayload(items=items))
            case unreachable:
                assert_never(unreachable)

    def _result(self, payload: StacPayload, op_tag: str) -> "RuntimeRail[StacResult]":
        import pyarrow as pa  # noqa: PLC0415

        match payload:
            case StacPayload(tag="arrow", arrow=table):
                pass
            case StacPayload(tag="delta", delta=uri):
                table = pa.table({"table_uri": [uri]})
            case StacPayload(tag="items", items=items):
                # `stac_table_rehydrate` returns rebuilt `pystac.Item` objects; lower each to its
                # dict for the Arrow receipt table the `QueryReceipt` keys.
                table = pa.Table.from_pylist([item.to_dict() for item in items])
            case unreachable:
                assert_never(unreachable)
        return QueryReceipt.railed("stac_geoparquet", op_tag, table).map(lambda receipt: StacResult(payload=payload, receipt=receipt))
```

## [03]-[SPATIAL]

- Owner: `SpatialQuery` — one tagged-union spatial-query axis over the DuckDB engine; `SpatialEngine` the request-scoped connection that loads the `spatial` geometry-view prelude plus each query's supplemental extensions and registers the input Arrow frames. In DuckDB 1.5.4 every `ST_` function — the `ST_GeomFromWKB`/`ST_X`/`ST_Y` geometry-view prelude and the `ST_Intersects`/`ST_Contains`/`ST_Within`/`ST_DWithin`/`ST_Transform` predicate-and-reprojection family alike — lives in the `spatial` extension, so `spatial` is the unconditional prelude every engine run loads before it can build a `GEOMETRY` view; the only axis between extensions is the repository: `spatial` rides the default core repository (`install_extension("spatial", repository=None)` then `load_extension`, no `FROM community`) and `h3` rides the community repository (`install_extension("h3", repository="community")` then `load_extension`). `SpatialQuery` cases `Join(predicate, left, right, distance)` (an `ST_` binary-predicate join — `ST_Intersects`/`ST_Contains`/`ST_Within`/`ST_DWithin` — the bbox-cached `SPATIAL_JOIN` operator the optimizer inserts as the automatic prefilter, `PointInPolygon` the named `ST_Contains` containment constructor) · `Transform(geometry, source_crs, target_crs)` (the GDAL-backed `ST_Transform(geom, src, dst)` in-engine reprojection the WKB inputs reach without a pyproj round-trip) · `H3Bin(geometry, resolution)` (`h3_latlng_to_cell` binning into one H3 index column over `ST_Y`/`ST_X` centroids), matched by `match`/`case` closed by `assert_never`. Every case projects through one `QueryPlan` — the `(sql, parameters, extensions, predicate_count)` record one closed `match` folds — so the SQL string, the extension requirement, and the receipt's predicate count derive from one traversal, never three parallel folds over the same family: `Join`/`Transform` carry `(SpatialExtension.SPATIAL,)`, `H3Bin` carries `(SpatialExtension.SPATIAL, SpatialExtension.H3)`, and the engine prepends the `spatial` prelude and loads the deduplicated union of the run's requirements exactly once through `dict.fromkeys`, never a per-call reinstall. The spatial axis emits plain `pyarrow.Table` keyed by one `ContentIdentity`, never a parallel index owner.
- Entry: `SpatialEngine.of` admits the named Arrow inputs; `SpatialEngine.run` computes the pure `QueryPlan` once, opens one `_TRACER.start_as_current_span(f"geo.spatial.{tag}")` carrying the `rasm.geo.op`/`rasm.geo.predicates` `attributes` around its `catch=duckdb.Error` `boundary` fence so the engine run is an OTel span binding the DuckDB DB-API root, and binds the railed `QueryReceipt` through `.bind`/`.map` so the result table and its content key derive in one rail. The fenced `_dispatch` opens one request-scoped `duckdb.connect()` inside a `closing` context so the DuckDB handle releases on the boundary exit, prepends the `spatial` prelude to `plan.extensions` and runs each `SpatialExtension.load` (`install_extension(value, repository=repository)` then `load_extension(value)`) over the `dict.fromkeys` deduplicated union, registers each named Arrow input through `con.register` and projects it into a `ST_GeomFromWKB` geometry view, then binds the parametrized plan SQL through `con.execute(sql, parameters)` so the distance/resolution literal never interpolates into the string and returns the `fetch_arrow_table()` egress; the spatial engine is the distinct owner of the `spatial`/`h3` extension-load and the `ST_GeomFromWKB` geometry-view admission lifecycle the `tabular/query#QUERY` `QueryEngine.Sql` path carries neither, sharing only the `QueryReceipt`/`ContentIdentity` law with the `query`/`columnar` owners — never a second generic SQL surface duplicating `QueryEngine`'s relational dispatch, the geometry-view prelude the one thing it adds; the join is accelerated by the DuckDB optimizer's bbox-cached `SPATIAL_JOIN` operator, never an STRtree/sjoin Python loop and never a hand-built R-tree.
- Auto: the geometry crosses as WKB (`GeoDataFrame.to_wkb`/`GeoExpr.st.to_wkb`) so the columnar input stays pyarrow-native at the wire and the engine decodes once with the `spatial`-extension `ST_GeomFromWKB`; CRS normalization rides either `VectorGeoClaim.reproject` (the pyproj `to_crs` on the host frame) or the `Transform` case's in-engine `ST_Transform` for an already-columnar input, so the join operands share one CRS without a second transport; the extension load is the `spatial` prelude unioned with each plan's supplemental `extensions` and deduplicated, so a `Join`/`Transform` run loads only the core-repository `spatial` and an `H3Bin` run adds the community-repository `h3` on top of it; the H3 resolution and the join distance are bound parameters on the plan, never string-spliced literals.
- Receipt: a spatial run threads the shared `columnar` `QueryReceipt.railed` keyed by `ContentIdentity` over the result Arrow bytes, the `predicate_count` carried from the one `QueryPlan`; no new receipt rail.
- Packages: `duckdb` (`connect`/`install_extension(repository=)`/`load_extension`/`execute`/`register`/`fetch_arrow_table`, the `spatial`-extension `GEOMETRY` type, the core-repository `spatial` extension, the community-repository `h3` extension), `geopandas` (`GeoDataFrame.to_wkb`/`to_arrow`), `polars-st` (`GeoExpr.st.to_wkb` the in-frame WKB input), `pyproj` (the host-frame CRS engine), `pyarrow` (`Table` the egress carrier), `expression` (`tagged_union`/`tag`/`case` the `SpatialQuery` ADT; `builtins.frozendict` the `SpatialEngine.inputs` immutable named-Arrow-frame evidence the `of` admission folds a foreign `Mapping` into), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-run engine span), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`/`QueryReceipt`).
- Growth: a new spatial predicate is one `SpatialPredicate` literal that the `Join` plan threads under the shared `spatial` extension; a new spatial intent is one `SpatialQuery` case projected by the one `QueryPlan` fold; a new loadable extension is one `SpatialExtension` member plus its `repository` row; the H3 hierarchy (`h3_cell_to_parent`, `h3_grid_disk`) composes on the existing `H3Bin` SQL and shares the `[04]-[GRID]` `GridSystem` cell ops; zero new surface and never a per-operation `st_join_*` family.
- Boundary: no GIS host coupling, no lonboard/GeoArrow visualization (that is `artifacts`), no durable store; a `load_extension("spatial")` without the preceding `install_extension` (the locked 1.5.4 distribution raises `IOException: Extension not found` for an uninstalled `spatial`), an `INSTALL h3` from the core repository where `h3` rides the community repository, three parallel `match` folds over the closed family where one `QueryPlan` projection carries SQL/parameters/extensions/predicate-count, a string-interpolated distance or resolution literal where `con.execute(sql, parameters)` binds it, an STRtree/`sjoin` Python join, a hand-rolled R-tree where the bbox-cached `SPATIAL_JOIN` operator is the automatic prefilter, a parallel H3 index owner, a `QueryReceipt.of` without a derived `ContentKey` where `QueryReceipt.railed` derives the key from the result bytes, a second DuckDB SQL surface duplicating the `query` engine, an un-narrowed `catch=Exception` engine boundary where `catch=duckdb.Error` binds the DB-API root, an attribute-free engine span dropping the `rasm.geo.op`/`rasm.geo.predicates` discriminant, and a `trace.get_tracer(__name__)` where the explicit `"rasm.data.spatial.geospatial"` dotted module string is the shared handle are the deleted forms.

```python signature
from enum import StrEnum
from typing import Final, Literal, assert_never

from builtins import frozendict
from collections.abc import Mapping

import duckdb
import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary

_TRACER: Final = trace.get_tracer("rasm.data.spatial.geospatial")

type SpatialPredicate = Literal["ST_Intersects", "ST_Contains", "ST_Within", "ST_DWithin"]


class SpatialExtension(StrEnum):
    SPATIAL = "spatial"
    H3 = "h3"

    @property
    def repository(self) -> str | None:
        return None if self is SpatialExtension.SPATIAL else "community"

    def load(self, con: "duckdb.DuckDBPyConnection") -> None:
        con.install_extension(self.value, repository=self.repository)
        con.load_extension(self.value)


class QueryPlan(Struct, frozen=True):
    sql: str
    parameters: tuple[object, ...]
    extensions: tuple[SpatialExtension, ...]
    predicate_count: int


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Literal["join", "transform", "h3_bin"] = tag()
    join: tuple[SpatialPredicate, str, str, float | None] = case()
    transform: tuple[str, str, str] = case()
    h3_bin: tuple[str, int] = case()

    @staticmethod
    def Join(predicate: SpatialPredicate, left: str, right: str, distance: float | None = None) -> "SpatialQuery":
        return SpatialQuery(join=(predicate, left, right, distance))

    @staticmethod
    def PointInPolygon(points: str, polygons: str) -> "SpatialQuery":
        return SpatialQuery(join=("ST_Contains", polygons, points, None))

    @staticmethod
    def Transform(geometry: str, source_crs: str, target_crs: str) -> "SpatialQuery":
        return SpatialQuery(transform=(geometry, source_crs, target_crs))

    @staticmethod
    def H3Bin(geometry: str, resolution: int = 9) -> "SpatialQuery":
        return SpatialQuery(h3_bin=(geometry, resolution))

    def plan(self) -> QueryPlan:
        match self:
            case SpatialQuery(tag="join", join=(predicate, left, right, distance)):
                on = f"{predicate}(l.geom, r.geom, ?)" if predicate == "ST_DWithin" else f"{predicate}(l.geom, r.geom)"
                return QueryPlan(
                    sql=f"SELECT l.*, r.* FROM {left} l JOIN {right} r ON {on}",
                    parameters=() if distance is None else (distance,),
                    extensions=(SpatialExtension.SPATIAL,),
                    predicate_count=1,
                )
            case SpatialQuery(tag="transform", transform=(geometry, source_crs, target_crs)):
                return QueryPlan(
                    sql=f"SELECT * EXCLUDE geom, ST_Transform(geom, ?, ?) AS geom FROM {geometry}",
                    parameters=(source_crs, target_crs),
                    extensions=(SpatialExtension.SPATIAL,),
                    predicate_count=0,
                )
            case SpatialQuery(tag="h3_bin", h3_bin=(geometry, resolution)):
                return QueryPlan(
                    sql=f"SELECT *, h3_latlng_to_cell(ST_Y(geom), ST_X(geom), ?) AS h3 FROM {geometry}",
                    parameters=(resolution,),
                    extensions=(SpatialExtension.SPATIAL, SpatialExtension.H3),
                    predicate_count=0,
                )
            case unreachable:
                assert_never(unreachable)


class SpatialResult(Struct, frozen=True):
    table: pa.Table
    receipt: QueryReceipt


class SpatialEngine(Struct, frozen=True):
    inputs: frozendict[str, pa.Table]

    @classmethod
    def of(cls, inputs: Mapping[str, pa.Table]) -> "SpatialEngine":
        return cls(inputs=frozendict(inputs))

    def run(self, query: SpatialQuery) -> "RuntimeRail[SpatialResult]":
        plan = query.plan()
        with _TRACER.start_as_current_span(
            f"geo.spatial.{query.tag}", attributes={"rasm.geo.op": query.tag, "rasm.geo.predicates": plan.predicate_count}
        ):
            return boundary(f"geo.spatial.{query.tag}", lambda: self._dispatch(plan), catch=duckdb.Error).bind(
                lambda table: QueryReceipt.railed("duckdb_spatial", query.tag, table, predicate_count=plan.predicate_count).map(
                    lambda receipt: SpatialResult(table=table, receipt=receipt)
                )
            )

    def _dispatch(self, plan: QueryPlan) -> pa.Table:
        from contextlib import closing  # noqa: PLC0415

        with closing(duckdb.connect()) as con:
            for extension in dict.fromkeys((SpatialExtension.SPATIAL, *plan.extensions)):
                extension.load(con)
            for name, table in self.inputs.items():
                con.register(f"{name}_raw", table)
                con.execute(f"CREATE VIEW {name} AS SELECT * EXCLUDE wkb, ST_GeomFromWKB(wkb) AS geom FROM {name}_raw")
            return con.execute(plan.sql, list(plan.parameters)).fetch_arrow_table()
```

## [04]-[GRID]

- Cases: `GridOp` is one cell-algebra axis. `Index(resolution, source)` covers the lat/lng-or-geometry-to-cells entry — a `CellSource` row (`Coordinates(lat_col, lng_col)` through `coordinates_to_cells`, `Wkb(geometry_col, containment)` through `wkb_to_cells` discriminating coverage by `ContainmentMode`, `Geometry(geometry_col, containment)` through `geometry_to_cells`) lifting a frame column into the `cell` column · `Resolution(target, shape)` changes each cell's resolution through `change_resolution` (`shape="single"`), the per-cell children list through `change_resolution_list` (`shape="list"`), or the source/target pairing through `change_resolution_paired` (`shape="paired"`) · `Disk(k, mode, aggregation)` the k-ring neighborhood through `grid_disk` (`mode="cells"`), `grid_disk_distances` (`mode="distances"`), or `grid_disk_aggregate_k` (`mode="aggregate"`, `aggregation` ∈ `min`/`max`) · `Ring(k_min, k_max)` the annulus through `grid_ring_distances` · `Measure(metric)` the one per-cell scalar entry over the `Metric` row — `cells_area_km2`/`cells_area_m2`/`cells_area_rads2` reached through `Metric.of_area(unit)` projecting an `AreaUnit` into the matching area row, and `cells_resolution` through `Metric.RESOLUTION` — one measure axis with no parallel `Area` sibling · `Bounds()` the per-cell bounding-box columns through `cells_bounds_arrays` · `Compact(direction, target)` compaction through `compact` (`direction="compact"`) and expansion through `uncompact` (`direction="uncompact"`, `target` the resolution) · `Boundary(form, kind, radians)` the cell/vertex/edge-to-geometry WKB egress through the one frozen `_BOUNDARY_WKB` `(form, kind)` table — `cells_to_wkb_polygons`/`cells_to_wkb_points` (`cell`), `vertexes_to_wkb_points` (`vertex`), `directededges_to_wkb_linestrings` (`edge`) reached by one `getattr(vector, ...)` — and the `centroid` form routing to the `cells_to_coordinates` RecordBatch independent of kind, an unsupported `(form, kind)` pair raising the typed boundary fault rather than a mis-applied `assert_never` · `LocalIj(anchor, direction)` the local-IJ transform through `cells_to_localij`/`localij_to_cells` · `Validate(mode, kind)` the parse/validate/stringify over the one `CellKind` axis — `{kind}_valid`/`{kind}_parse`/`{kind}_to_string` collapsing the three parallel `cells_*`/`vertexes_*`/`directededges_*` families into one `CellKind`-keyed dispatch · `Raster(direction, values, transform, size)` the one bidirectional numpy↔cell bridge — `direction="to_cells"` lifts a coverage through `raster.raster_to_dataframe` with `raster.nearest_h3_resolution` deriving the resolution when omitted, `direction="to_raster"` (`GridOp.Rasterize`) burns the cell/value frame back to a numpy array plus its affine through `raster.rasterize_cells`, one bridge axis never a parallel `cells_to_raster` surface · `Hierarchy(direction, target)` the parent walk through `change_resolution` to the coarser `target` (`direction="parent"`) and the child expansion through `change_resolution_list` to the finer `target` (`direction="children"`), the in-frame h3ronpy hierarchy leg the in-DB `engine_bin` `h3_cell_to_parent` SQL mirrors. A new cell operation is one `GridOp` case; a new index kind (cell/vertex/edge) is one `CellKind` row; a new scalar metric is one `Metric`/`AreaUnit` row; a new coverage policy is one `ContainmentMode` row; a new grid scheme is one `GridScheme` member.
- Entry: `GridSystem.apply(op, frame)` opens one `_TRACER.start_as_current_span(f"geo.grid.{scheme}.{tag}")` carrying the `rasm.geo.scheme`/`rasm.geo.op` `attributes` around its `catch=(NotImplementedError, ValueError, KeyError, RuntimeError)` `boundary` fence (the h3ronpy FFI fault set plus the page-owned S2 deferral) then folds one `GridOp` over a `polars.DataFrame` through the closed `match`/`assert_never` `_grid` kernel returning a `RuntimeRail[GridResult]`, dispatching the H3 leg to the h3ronpy array op over the named column (the polars `.h3` namespace where the op is a single-column expression, the `h3ronpy` Arrow function where the result is a multi-column `RecordBatch` paired back onto the frame) and raising the S2 deferral on the S2 scheme; `GridSystem.engine_bin(table, geometry_view, resolution)` composes the `[03]-[SPATIAL]` `SpatialEngine.run(SpatialQuery.H3Bin(...))` SQL leg for an already-columnar in-DB frame, the same `h3` extension the `[03]-[SPATIAL]` engine loads, so the in-frame and in-DB legs share one binning law. The fold keys each result by `ContentIdentity` over the `cell` column bytes and contributes the shared `columnar` `QueryReceipt`.
- Auto: cells/vertexes/edges stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline, so a 10⁷-cell binning is one vectorized h3ronpy call, never a Python loop; the geometry-to-cells leg reads the polars-st WKB `GeoExpr` column directly (`geom.st.to_wkb()` decoded once by `wkb_to_cells`), so the DGGS index shares the one WKB geometry encoding the `[02]-[GEO]` vector claims and the `[03]-[SPATIAL]` engine speak; the `CellKind` axis routes the index-kind prefix once (`cell`/`vertex`/`edge` selecting `{kind}_valid`/`{kind}_parse`/`{kind}_to_string` and the matching WKB egress), so the three parallel h3ronpy families never grow a parallel `GridOp` row each; the `ContainmentMode` discriminates coverage (`ContainsCentroid`/`ContainsBoundary`/`Covers`/`IntersectsBoundary`) as an enum row, never a boolean-flag tangle; `set_failing_to_invalid` keeps array length stable on parse failure so an invalid cell is a null data row, never a raised exception in the array pipeline; the geometry CRS is fixed to `EPSG:4326` (the h3ronpy `H3_CRS`) and the default cell column is `cell` (the h3ronpy `DEFAULT_CELL_COLUMN_NAME`), both runtime-owned constants the page never re-mints.
- Receipt: a grid run threads the shared `tabular/columnar` `QueryReceipt.railed(engine, op.tag, frame.to_arrow())` over the result frame, the `engine` carrying the `h3ronpy.<scheme>` route, the `source` the `op.tag`, and the row/column counts the cell-frame shape, keyed by the `ContentIdentity` `QueryReceipt.railed` derives from the Arrow bytes; the `GridResult` pairs the frame with that receipt, no new receipt rail.
- Packages: `h3ronpy` (`change_resolution`/`change_resolution_list`/`change_resolution_paired`/`compact`/`uncompact`/`grid_disk`/`grid_disk_distances`/`grid_disk_aggregate_k`/`grid_ring_distances`/`cells_resolution`/`cells_area_km2`/`cells_area_m2`/`cells_area_rads2`/`cells_valid`/`cells_parse`/`cells_to_string`/`vertexes_valid`/`vertexes_parse`/`vertexes_to_string`/`directededges_valid`/`directededges_parse`/`directededges_to_string`/`cells_to_localij`/`localij_to_cells`/`version`, `vector.coordinates_to_cells`/`vector.wkb_to_cells`/`vector.geometry_to_cells`/`vector.cells_to_coordinates`/`vector.cells_to_wkb_points`/`vector.cells_to_wkb_polygons`/`vector.vertexes_to_wkb_points`/`vector.directededges_to_wkb_linestrings`/`vector.cells_bounds_arrays`, `raster.raster_to_dataframe`/`raster.nearest_h3_resolution`/`raster.rasterize_cells`, the `ContainmentMode` enum, the `polars.H3Expr`/`polars.H3SeriesShortcuts` `.h3` namespace; every `arro3.core.Array`/`RecordBatch` return crosses into polars through `pl.from_arrow` over the Arrow PyCapsule interface, never a positional `pl.Series(name, array)` intake), `polars-st` (`geom`/`from_wkb`/`GeoExpr.st.to_wkb`/`GeoExpr.st.centroid`/`GeoDataFrame` the WKB geometry frame the geometry-to-cells leg reads and the centroid-prep the coordinate index reads), `polars` (`DataFrame`/`Expr`/`Series` the carrier), `expression` (`tagged_union`/`tag`/`case` the `CellSource`/`GridOp` ADTs; `builtins.frozendict` the data-valued `_BOUNDARY_WKB` `(form, kind)->fn-name` correspondence table — a `frozendict` because the row value is a function-name `str` not a callable, the same data-table form the sibling `tabular/lakehouse#LAKE` `_PORTABLE` and `tabular/columnar#SCAN` `_SCAN_READER` rows own), `duckdb` (the `[03]-[SPATIAL]` `h3` SQL engine leg), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-op grid span), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`).
- Boundary: no host coupling, no durable cell store, no lonboard/GeoArrow visualization (that is `artifacts`); a per-row scalar cell loop where the h3ronpy array functions vectorize, a pandas detour when the polars `.h3` namespace owns the column, a hand-rolled H3 cell-index codec or grid-traversal kernel where the Rust `h3o` backend owns it, a parallel per-resolution, per-mode, per-index-kind, or per-raster-direction function family where the `GridOp` case, `CellKind` row, and `RasterDirection` row discriminate, a positional `pl.Series(name, arro3_array)` intake where `pl.from_arrow` is the catalogued Arrow-PyCapsule crossing, a second WKB geometry encoding where the polars-st `GeoExpr` already carries it, a parallel H3 column owner beside the `[03]-[SPATIAL]` engine, identity/CRS minting the runtime owns, an un-narrowed `catch=Exception` grid boundary where `catch=(NotImplementedError, ValueError, KeyError, RuntimeError)` binds the h3ronpy FFI faults plus the page-owned S2 deferral, an attribute-free grid span dropping the `rasm.geo.scheme`/`rasm.geo.op` discriminant, and a `trace.get_tracer(__name__)` where the explicit `"rasm.data.spatial.geospatial"` dotted module string is the shared handle are the deleted forms. h3ronpy/polars-st ride the Forge scientific source build band, so every operation body binds the provider function-local under `# noqa: PLC0415`, never a subprocess seam.

```python signature
from builtins import frozendict
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    import numpy as np
    import polars as pl
    import pyarrow as pa

_TRACER: Final = trace.get_tracer("rasm.data.spatial.geospatial")


type ResolutionShape = Literal["single", "list", "paired"]
type DiskMode = Literal["cells", "distances", "aggregate"]
type Aggregation = Literal["min", "max"]
type CompactDirection = Literal["compact", "uncompact"]
type BoundaryForm = Literal["polygon", "point", "centroid", "linestring"]
type IjDirection = Literal["to_localij", "from_localij"]
type ValidateMode = Literal["valid", "parse", "format"]
type HierarchyDirection = Literal["parent", "children"]
type RasterDirection = Literal["to_cells", "to_raster"]

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


_BOUNDARY_WKB: Final[frozendict[tuple[BoundaryForm, CellKind], str]] = frozendict({
    ("polygon", CellKind.CELL): "cells_to_wkb_polygons",
    ("point", CellKind.CELL): "cells_to_wkb_points",
    ("point", CellKind.VERTEX): "vertexes_to_wkb_points",
    ("linestring", CellKind.EDGE): "directededges_to_wkb_linestrings",
})


class GridSystem(Struct, frozen=True):
    scheme: GridScheme = GridScheme.H3
    cell_column: str = DEFAULT_CELL_COLUMN
    crs: str = H3_CRS

    def apply(self, op: GridOp, frame: "pl.DataFrame") -> "RuntimeRail[GridResult]":
        with _TRACER.start_as_current_span(
            f"geo.grid.{self.scheme.value}.{op.tag}", attributes={"rasm.geo.scheme": self.scheme.value, "rasm.geo.op": op.tag}
        ):
            return boundary(
                f"geo.grid.{self.scheme.value}.{op.tag}",
                lambda: self._grid(op, frame),
                catch=(NotImplementedError, ValueError, KeyError, RuntimeError),
            ).bind(lambda result: self._result(op, result))

    def _result(self, op: GridOp, frame: "pl.DataFrame") -> "RuntimeRail[GridResult]":
        return QueryReceipt.railed(f"h3ronpy.{self.scheme.value}", op.tag, frame.to_arrow()).map(
            lambda receipt: GridResult(frame=frame, receipt=receipt)
        )

    def engine_bin(self, table: "pa.Table", geometry_view: str, resolution: int) -> "RuntimeRail[SpatialResult]":
        return SpatialEngine.of({geometry_view: table}).run(SpatialQuery.H3Bin(geometry_view, resolution))

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
        wkb = getattr(vector, _BOUNDARY_WKB[form, kind])(cells, radians=radians)
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
                # frame rows the `attach` `with_columns` requires; `flatten=True` would explode the
                # length and break the row alignment (the exploded form is the engine `H3Bin` leg).
                return wkb_to_cells(frame[geometry_col], resolution, containment_mode=containment.mode(), compact=False, flatten=False)
            case CellSource(tag="geometry", geometry=(geometry_col, containment)):
                return geometry_to_cells(frame[geometry_col], resolution, containment_mode=containment.mode(), compact=False)
            case unreachable:
                assert_never(unreachable)
```

## [05]-[RESEARCH]

- [GEOARROW_ENCODING]: the `geopandas` `GeoDataFrame.to_parquet(geometry_encoding=)`/`to_arrow(geometry_encoding=)`/`to_wkb()` surface the `EgressFormat.GEOARROW` and the spatial-input WKB bridge transcribe is catalogue-confirmed against the folder `geopandas` `.api`; the catalogue lists `geometry_encoding='WKB'` as the default, so the `"geoarrow"` value confirms as the GeoParquet 1.1 native-encoding member against the live distribution before the GeoArrow row treats it as the settled smaller-egress encoding.
- [DUCKDB_SPATIAL_H3]: the `duckdb` `connect`/`install_extension(repository=)`/`load_extension`/`register`/`execute`/`sql`/`fetch_arrow_table` Python surface is catalogue-confirmed against the folder `duckdb` `.api` — `execute(query, parameters)` returns the connection and `fetch_arrow_table()`/`arrow()` is the enumerated Arrow egress, so the parametrized `con.execute(plan.sql, list(plan.parameters)).fetch_arrow_table()` path binds the distance/resolution literal as a bound parameter, never string interpolation. Verified against the locked DuckDB 1.5.4 distribution (`osx_arm64`): every `ST_` function lives in the `spatial` extension — a bare `SELECT ST_Point(1,2)` on a fresh connection raises `CatalogException: Scalar Function with name "st_point" is not in the catalog, but it exists in the spatial extension`, and `LOAD spatial` against an uninstalled extension raises `IOException: Extension "…/spatial.duckdb_extension" not found`. So `spatial` is NOT statically bundled and NOT core-callable: the engine runs `install_extension("spatial", repository=None)` (the core repository, confirmed by a successful bare `INSTALL spatial`) then `load_extension("spatial")` as the unconditional geometry-view prelude, and `ST_GeomFromWKB`/`ST_X`/`ST_Y` (the view prelude) and `ST_Intersects`/`ST_Contains`/`ST_Within`/`ST_DWithin`/`ST_Transform` (the predicate-and-reprojection family) all resolve once `spatial` is loaded (`SELECT ST_AsText(ST_GeomFromWKB(ST_AsWKB(ST_Point(1,2))))` returns `POINT (1 2)`, `ST_Intersects`/`ST_Transform` succeed). The only inter-extension axis is the repository: `spatial` is core (`repository=None`), `h3` is community (`install_extension("h3", repository="community")`). The automatic join prefilter is the optimizer's bbox-cached `SPATIAL_JOIN` operator, not an R-tree the extension builds. The `h3` community-extension `h3_latlng_to_cell`/`h3_cell_to_parent` spellings and the `ST_Transform(geom, source, target)` three-argument GDAL signature are settled against the loaded `spatial` catalog; the published `h3.duckdb_extension` build tracks the DuckDB version per platform, so an `install_extension("h3", repository="community")` against a not-yet-published `v1.5.4` build surfaces a transient `HTTPException` the `boundary` rail traps, never a settled fence.
- [RASTERIO_STREAM_VRT]: the `rasterio` streaming/remote/cleanup surface the `RasterOp` `Stream`/`Sample`/`Vrt`/`RemoteRead`/`MemorySource`/`WriteCog`/`Mosaic`/`GeometryMask`/`Sieve`/`Vectorize` rows bind is catalogue-confirmed against the folder `rasterio` `.api` — `DatasetReader.block_windows(bidx)` (the tile-aligned generator), `DatasetReader.read(window=, out_shape=, boundless=, fill_value=, resampling=)`, `DatasetReader.read_masks(...)`, `DatasetReader.sample(xy, indexes=)`, `DatasetReader.window_transform(window)`, `windows.from_bounds`, `merge.merge(datasets, bounds=, res=, method=, resampling=, nodata=)`, `features.shapes(source, mask=, connectivity=, transform=)`, `features.geometry_mask(geometries, out_shape, transform, ...)`, `features.sieve(source, size, connectivity, ...)`, `vrt.WarpedVRT`, `Env`, `io.MemoryFile`, and `io.DatasetWriter.write` are all enumerated entrypoints. The `WarpedVRT(source, crs=, resampling=, width=, height=, nodata=)` constructor keyword spelling, and the `features.geometry_mask`/`features.sieve` positional shapes confirm against the synced rasterio distribution; the `"COG"` driver and its `compress`/`blocksize`/`overviews`/`overview_resampling`/`predictor`/`num_threads` creation keys the typed `CogProfile.creation` projects, and the `GDAL_DISABLE_READDIR_ON_OPEN` `Env` config key are GDAL-surface values the rasterio Python catalogue does not enumerate, confirmed against the loaded GDAL driver catalog before the `WriteCog`/`RemoteRead` rows treat them as settled. `WarpedVRT` is GDAL-native streamed reproject, never a second byte-window transport beside the `tabular/egress` `obstore` rail.
- [STAC_NDJSON_INTERCHANGE]: SETTLED — the STAC-NDJSON interchange is the canonical `spatial/catalog#TABLE` `stac_table` owner, and the `StacIngest` `ToArrow`/`ToDelta`/`Rehydrate` rows compose `stac_table(TableSource.Ndjson(...))`, `stac_table_direct(TableSource.Ndjson(...), TableSink.DeltaLake(...))`, and `stac_table_rehydrate(table)` rather than re-binding the `stac_geoparquet.arrow` surface (WORKSPACE_LAW: one owner per bounded concept). The catalog owner's RESEARCH (`spatial/catalog.md` `[03]-[TABLE]`) settles the `parse_stac_ndjson_to_arrow`/`parse_stac_ndjson_to_delta_lake`/`stac_table_to_items` spellings, the `chunk_size=DEFAULT_JSON_CHUNK_SIZE` default, and the `"FullFile"`/`"FirstBatch"` `SchemaInference` literal; `ToDelta` carries no `schema_version` because the catalog `stac_table_direct` Delta arm binds `parse_stac_ndjson_to_delta_lake(path, table_or_uri)` with no schema-version keyword, and the optional-`deltalake`-extra `ModuleNotFoundError` is the catalog owner's `boundary` rail to trap. `stac_table_rehydrate` returns rebuilt `pystac.Item` objects this claim lowers through `Item.to_dict` for the `QueryReceipt` Arrow table. The geospatial-tier wrapper (the `StacResult`/`QueryReceipt`/span/`apply_remote` retry rail over the railed catalog call) is settled fence code.

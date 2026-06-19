# [PY_DATA_CLAIM]

The two-axis geospatial surface, spatial egress, and the columnar spatial-query engine. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio with pyproj backing the CRS engine and exposes one `VectorOp` tagged-union in-frame vector-algebra axis (`apply`); `RasterGeoClaim` carries coverage/band/resampling/nodata/transform/CRS over rasterio and exposes one `RasterOp` tagged-union coverage axis (`apply`). Vector and raster are two value objects with two distinct operation axes because band/resampling/window semantics differ from vector CRS/axis-order; the operation knobs (`how`/`predicate`/`aggfunc`/`max_distance`/`method`/`crop`/`merge_alg`/`resampling`) are case payload rows, never a `sjoin`/`overlay`/`do_merge`/`clip_raster` method family. `EgressFormat` is one `StrEnum` carrying its own `write` — GeoJSON/GeoParquet/FlatGeobuf/GeoArrow, the OGR driver IS the enum value, GeoParquet rides geopandas `to_parquet` and GeoArrow rides `to_parquet(geometry_encoding="geoarrow")` — not a tagged union over redundant payloads or a per-format writer family. `SpatialQuery` is one tagged-union axis over the DuckDB-spatial extension (ST joins, point-in-polygon, R-tree prefilter, H3 binning) composing the `query` engine for the SQL path. Every bundle keys by exactly one runtime `ContentIdentity`, folding the shared `columnar/dataset.md#SCAN` `QueryReceipt`.

## [1]-[INDEX]

- `[2]-[GEO]`: vector and raster geospatial claims, the in-frame `VectorOp`/`RasterOp` operation axes, spatial egress, the GeoArrow encoding.
- `[3]-[SPATIAL]`: the DuckDB-spatial join/index/H3 columnar query engine.

## [2]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio, with one `VectorOp` tagged-union axis folded by `apply(op, frame)`; `RasterGeoClaim` — coverage/band/resampling/nodata/transform/CRS over rasterio, with one `RasterOp` tagged-union axis folded by `apply(op, source)`; `EgressFormat` the GeoJSON/GeoParquet/flat-vector export as one `StrEnum` whose member value is the OGR driver and whose `write` selects `to_parquet` for GeoParquet and `to_file(driver=value)` otherwise, not a per-format writer family.
- Cases: `VectorOp` rows `Join(predicate, other, how, max_distance)` (geopandas `sjoin`/`sjoin_nearest`, index-accelerated by `GeoDataFrame.sindex`) · `Overlay(other, how)` (`overlay` intersection/union/difference/symmetric-difference/identity set algebra) · `Dissolve(by, aggfunc)` (`dissolve` spatial group-aggregate) · `Clip(mask, keep_geom_type)` (`clip` mask) · `Construct(kind, param)` (the `ConstructKind` constructive family) · `Predicate(name, other)` (the vectorized `JoinPredicate` binary-predicate filter). `RasterOp` rows `Window(bounds, boundless)` (`windows.from_bounds` + `DatasetReader.read(window=, boundless=)` streamed over `block_windows`) · `Mosaic(sources, method, resampling)` (`merge.merge`) · `Mask(shapes, crop, all_touched, invert)` (`mask.mask`) · `Vectorize(connectivity)` (`features.shapes` raster-to-vector) · `Rasterize(shapes, out_shape, merge_alg, all_touched)` (`features.rasterize` vector-to-raster) · `Reproject(target_crs, resampling)` (`warp.calculate_default_transform` + `warp.reproject`). `EgressFormat` rows `GEOJSON`/`GEOPARQUET`/`GEOARROW`/`FLATGEOBUF`. A new vector operation is one `VectorOp` case; a new raster operation is one `RasterOp` case; a new constructive op is one `ConstructKind` row; a new spatial predicate is one `JoinPredicate` row; a new resampling mode is one `rasterio.enums.Resampling` row; a new egress format is one `EgressFormat` member.
- Entry: `VectorGeoClaim.apply(op, frame)` folds one `VectorOp` over the live `GeoDataFrame` through the closed `match`/`assert_never` `_vector` kernel, normalizing the CRS of binary-operand inputs through the pyproj-backed `to_crs` prelude so `Join`/`Overlay` operands share one CRS, returning a `RuntimeRail[GeoDataFrame]`; `RasterGeoClaim.apply(op, source)` folds one `RasterOp` over the rasterio source through the `_raster` kernel returning a `RuntimeRail[CoverageResult]` (the `(count, rows, cols)` NumPy egress paired with its affine transform); `EgressFormat.write` emits the bundle keyed by one `ContentIdentity.of`; both `apply` paths fold the shared `columnar` `QueryReceipt` keyed by `ContentIdentity`.
- Packages: `geopandas` (`sjoin`/`sjoin_nearest`/`overlay`/`clip`/`dissolve`/the predicate accessors/`buffer`/`simplify`/`convex_hull`/`concave_hull`/`voronoi_polygons`/`delaunay_triangles`/`sindex`/`to_crs`/`to_parquet`/`to_file`), `shapely` (`set_precision`/`make_valid` constructive backing), `pyogrio` (`write_dataframe` vector I/O), `pyproj` (the CRS engine the binary operands share), `rasterio` (`open`/`windows.from_bounds`/`block_windows`/`merge.merge`/`mask.mask`/`features.shapes`/`features.rasterize`/`warp.reproject`/`warp.calculate_default_transform`/`enums.Resampling`/`enums.MergeAlg`/`vrt.WarpedVRT`), `numpy` (the raster `(count, rows, cols)` array egress), `duckdb` (the `[3]-[SPATIAL]` engine), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`/`Receipt`).
- Growth: a new vector operation is one `VectorOp` case (the next geopandas op, `segmentize`/`offset_curve`, lands as one `ConstructKind` row); a new raster operation is one `RasterOp` case; a new constructive op is one `ConstructKind` row; a new binary predicate is one `JoinPredicate` row; a new resampling mode is one `rasterio.enums.Resampling` row; a new egress format is one `EgressFormat` member (GeoArrow lands as `GEOARROW`); the DuckDB-spatial join/index/H3 engine is the `[3]-[SPATIAL]` `SpatialQuery` axis; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape, a `sjoin`/`overlay`/`dissolve`/`do_merge`/`clip_raster`/`rasterize_features` method family, a Python STRtree/`isinstance` spatial-join loop where `GeoDataFrame.sindex` accelerates, a full-raster read where a window suffices, a tagged union whose case payload only restates its tag, and a per-format writer family are the deleted forms. The content key derives from the egress bytes through exactly one canonical `ContentIdentity.of`. geopandas/shapely/rasterio ride the Forge scientific source-build band, so every operation body binds the provider function-local under `# noqa: PLC0415`, never a subprocess seam.

```python signature
from __future__ import annotations

from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.data.columnar.dataset import QueryReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    import numpy as np
    from geopandas import GeoDataFrame
    from rasterio import DatasetReader


type SetOp = Literal["intersection", "union", "difference", "symmetric_difference", "identity"]
type JoinHow = Literal["inner", "left", "right"]


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


class EgressFormat(StrEnum):
    GEOJSON = "GeoJSON"
    GEOPARQUET = "GeoParquet"
    GEOARROW = "GeoArrow"
    FLATGEOBUF = "FlatGeobuf"

    def write(self, frame: GeoDataFrame, path: str) -> RuntimeRail[ContentKey]:
        emit = {
            EgressFormat.GEOPARQUET: lambda: frame.to_parquet(path),
            EgressFormat.GEOARROW: lambda: frame.to_parquet(path, geometry_encoding="geoarrow"),
        }.get(self, lambda: frame.to_file(path, driver=self.value))
        return boundary(f"geo.egress.{self.value}", lambda: (emit(), ContentIdentity.of(self.value, Path(path).read_bytes()))[1])


@tagged_union(frozen=True)
class VectorOp:
    tag: Literal["join", "overlay", "dissolve", "clip", "construct", "predicate"] = tag()
    join: tuple[JoinPredicate, "GeoDataFrame", JoinHow, float | None] = case()
    overlay: tuple["GeoDataFrame", SetOp] = case()
    dissolve: tuple[tuple[str, ...], str] = case()
    clip: tuple["GeoDataFrame", bool] = case()
    construct: tuple[ConstructKind, float] = case()
    predicate: tuple[JoinPredicate, "GeoDataFrame"] = case()

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
    def Predicate(name: JoinPredicate, other: "GeoDataFrame") -> "VectorOp":
        return VectorOp(predicate=(name, other))


@tagged_union(frozen=True)
class RasterOp:
    tag: Literal["window", "mosaic", "mask", "vectorize", "rasterize", "reproject"] = tag()
    window: tuple[tuple[float, float, float, float], bool] = case()
    mosaic: tuple[tuple[str, ...], str, str] = case()
    mask: tuple[tuple[object, ...], bool, bool, bool] = case()
    vectorize: int = case()
    rasterize: tuple[tuple[object, ...], tuple[int, int], str, bool] = case()
    reproject: tuple[str, str] = case()

    @staticmethod
    def Window(bounds: tuple[float, float, float, float], boundless: bool = False) -> "RasterOp":
        return RasterOp(window=(bounds, boundless))

    @staticmethod
    def Mosaic(sources: tuple[str, ...], method: str = "first", resampling: str = "nearest") -> "RasterOp":
        return RasterOp(mosaic=(sources, method, resampling))

    @staticmethod
    def Mask(shapes: tuple[object, ...], crop: bool = True, all_touched: bool = False, invert: bool = False) -> "RasterOp":
        return RasterOp(mask=(shapes, crop, all_touched, invert))

    @staticmethod
    def Vectorize(connectivity: int = 4) -> "RasterOp":
        return RasterOp(vectorize=connectivity)

    @staticmethod
    def Rasterize(shapes: tuple[object, ...], out_shape: tuple[int, int], merge_alg: str = "replace", all_touched: bool = False) -> "RasterOp":
        return RasterOp(rasterize=(shapes, out_shape, merge_alg, all_touched))

    @staticmethod
    def Reproject(target_crs: str, resampling: str = "nearest") -> "RasterOp":
        return RasterOp(reproject=(target_crs, resampling))


class CoverageResult(Struct, frozen=True):
    array: "np.ndarray"
    transform: tuple[float, ...]
    receipt: QueryReceipt


class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    def apply(self, op: VectorOp, frame: "GeoDataFrame") -> "RuntimeRail[GeoDataFrame]":
        return boundary(f"geo.vector.{op.tag}", lambda: self._vector(op, frame))

    def _vector(self, op: VectorOp, frame: "GeoDataFrame") -> "GeoDataFrame":
        import geopandas as gpd  # noqa: PLC0415
        import shapely  # noqa: PLC0415

        snapped = frame.assign(geometry=frame.geometry.apply(lambda g: shapely.set_precision(g, 10.0**-self.precision)))
        match op:
            case VectorOp(tag="join", join=(predicate, other, how, max_distance)):
                aligned = other.to_crs(self.crs)
                return (
                    gpd.sjoin_nearest(snapped, aligned, how=how, max_distance=max_distance, distance_col="distance")
                    if predicate is JoinPredicate.DWITHIN and max_distance is not None
                    else gpd.sjoin(snapped, aligned, how=how, predicate=predicate.value)
                )
            case VectorOp(tag="overlay", overlay=(other, how)):
                return gpd.overlay(snapped, other.to_crs(self.crs), how=how, keep_geom_type=True)
            case VectorOp(tag="dissolve", dissolve=(by, aggfunc)):
                return snapped.dissolve(by=list(by), aggfunc=aggfunc)
            case VectorOp(tag="clip", clip=(mask, keep_geom_type)):
                return gpd.clip(snapped, mask.to_crs(self.crs), keep_geom_type=keep_geom_type)
            case VectorOp(tag="construct", construct=(kind, param)):
                geometry = self._construct(snapped.geometry, kind, param)
                return snapped.set_geometry(geometry)
            case VectorOp(tag="predicate", predicate=(name, other)):
                hits = getattr(snapped.geometry, name.value)(other.to_crs(self.crs).union_all())
                return snapped.loc[hits]
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _construct(geometry: object, kind: ConstructKind, param: float) -> object:
        match kind:
            case ConstructKind.BUFFER:
                return geometry.buffer(param)
            case ConstructKind.SIMPLIFY:
                return geometry.simplify(param)
            case ConstructKind.CONVEX_HULL:
                return geometry.convex_hull
            case ConstructKind.CONCAVE_HULL:
                return geometry.concave_hull(ratio=param)
            case ConstructKind.VORONOI_POLYGONS:
                return geometry.voronoi_polygons()
            case ConstructKind.DELAUNAY_TRIANGLES:
                return geometry.delaunay_triangles(tolerance=param)
            case unreachable:
                assert_never(unreachable)


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: str
    nodata: float
    transform: tuple[float, ...]

    def apply(self, op: RasterOp, source: "DatasetReader") -> "RuntimeRail[CoverageResult]":
        return boundary(f"geo.raster.{op.tag}", lambda: self._raster(op, source))

    def _raster(self, op: RasterOp, source: "DatasetReader") -> CoverageResult:
        from contextlib import ExitStack  # noqa: PLC0415

        import numpy as np  # noqa: PLC0415
        import rasterio  # noqa: PLC0415
        from rasterio import features, mask, merge, warp, windows  # noqa: PLC0415
        from rasterio.enums import MergeAlg, Resampling  # noqa: PLC0415

        match op:
            case RasterOp(tag="window", window=(bounds, boundless)):
                window = windows.from_bounds(*bounds, transform=source.transform)
                array = source.read(window=window, boundless=boundless, fill_value=self.nodata)
                return self._result(np.asarray(array), source.window_transform(window), op.tag, source)
            case RasterOp(tag="mosaic", mosaic=(sources, method, resampling)):
                with ExitStack() as stack:
                    opened = [stack.enter_context(rasterio.open(path)) for path in sources]
                    mosaic, out_transform = merge.merge(opened, method=method, resampling=Resampling[resampling], nodata=self.nodata)
                return self._result(np.asarray(mosaic), tuple(out_transform)[:6], op.tag, source)
            case RasterOp(tag="mask", mask=(shapes, crop, all_touched, invert)):
                out_image, out_transform = mask.mask(source, list(shapes), crop=crop, all_touched=all_touched, invert=invert, nodata=self.nodata, filled=True)
                return self._result(np.asarray(out_image), tuple(out_transform)[:6], op.tag, source)
            case RasterOp(tag="vectorize", vectorize=connectivity):
                band = source.read(1)
                shapes = np.asarray(list(features.shapes(band, mask=band != self.nodata, connectivity=connectivity, transform=source.transform)), dtype=object)
                return self._result(shapes, self.transform, op.tag, source)
            case RasterOp(tag="rasterize", rasterize=(shapes, out_shape, merge_alg, all_touched)):
                array = features.rasterize(list(shapes), out_shape=out_shape, transform=source.transform, merge_alg=MergeAlg[merge_alg], all_touched=all_touched)
                return self._result(np.asarray(array), self.transform, op.tag, source)
            case RasterOp(tag="reproject", reproject=(target_crs, resampling)):
                dst_transform, width, height = warp.calculate_default_transform(source.crs, target_crs, source.width, source.height, *source.bounds)
                destination = np.empty((source.count, height, width), dtype=source.dtypes[0])
                warp.reproject(source.read(), destination, src_transform=source.transform, src_crs=source.crs, dst_transform=dst_transform, dst_crs=target_crs, resampling=Resampling[resampling], dst_nodata=self.nodata)
                return self._result(destination, tuple(dst_transform)[:6], op.tag, source)
            case unreachable:
                assert_never(unreachable)

    def _result(self, array: "np.ndarray", transform: tuple[float, ...], op_tag: str, source: "DatasetReader") -> CoverageResult:
        key = ContentIdentity.of(f"geo.raster.{op_tag}", array.tobytes())
        return CoverageResult(
            array=array,
            transform=transform,
            receipt=QueryReceipt(engine="rasterio", source=str(source.name), columns=int(array.shape[0]) if array.ndim == 3 else 1, predicate_count=0, row_count=int(array.size), content_key=key),
        )
```

## [3]-[SPATIAL]

- Owner: `SpatialQuery` — one tagged-union spatial-query axis over the DuckDB `spatial` + `h3` extensions; `SpatialEngine` the request-scoped connection that loads both extensions once and registers the input Arrow GeoDataFrames. `SpatialQuery` cases `Join(predicate, left, right)` (an ST binary-predicate join — `ST_Intersects`/`ST_Contains`/`ST_DWithin` — the R-tree the extension builds backing the prefilter) · `PointInPolygon(points, polygons)` (the containment join) · `H3Bin(geometry, resolution)` (`h3_latlng_to_cell` binning into one H3 index column), matched by `match`/`case` closed by `assert_never`. The spatial axis emits plain `pyarrow.Table` keyed by one `ContentIdentity`, never a parallel index owner.
- Entry: `SpatialEngine.of` opens `duckdb.connect()`, runs `INSTALL spatial; LOAD spatial; INSTALL h3 FROM community; LOAD h3` once, and registers each named Arrow input as `WKB_BLOB`-decoded `ST_GeomFromWKB` views; `SpatialEngine.run` folds one `SpatialQuery` to a `RuntimeRail[pa.Table]`, composing the `query/relational#QUERY` `QueryEngine.Sql` path so the SQL string the spatial arm builds runs through the one query owner rather than a second SQL surface; the join is index-accelerated by the DuckDB R-tree, never an STRtree/sjoin Python loop.
- Auto: the geometry crosses as WKB (`GeoDataFrame.to_wkb`) so the columnar input stays pyarrow-free at the wire and the extension decodes once with `ST_GeomFromWKB`; CRS normalization rides `VectorGeoClaim.reproject` (the pyproj-backed `to_crs`) on the inputs before the join, so the join operands share one CRS; the H3 resolution is the one knob on `H3Bin`, the binning a single `h3_latlng_to_cell(ST_Y(geom), ST_X(geom), resolution)` projection, never a parallel H3 owner.
- Receipt: a spatial run contributes the shared `columnar` `QueryReceipt` keyed by `ContentIdentity` over the result bytes; no new receipt rail.
- Packages: `duckdb` (`connect`/`install_extension`/`load_extension`/`sql`/`register`/`from_arrow`/`to_arrow_table`, the `spatial` and `h3` community extensions), `geopandas` (`GeoDataFrame.to_wkb`/`to_arrow`), `pyproj` (the CRS engine on the join inputs), `pyarrow` (`Table`), runtime (`RuntimeRail`/`ContentIdentity`).
- Growth: a new spatial predicate is one `SpatialQuery.Join` predicate literal; a new spatial intent is one `SpatialQuery` case; the H3 hierarchy (`h3_cell_to_parent`, `h3_grid_disk`) composes on the existing `H3Bin` SQL; zero new surface and never a per-operation `st_join_*` family.
- Boundary: no GIS host coupling, no lonboard/GeoArrow visualization (that is `artifacts`), no durable store; an STRtree/`sjoin` Python join, a hand-rolled R-tree, a parallel H3 index owner, and a second DuckDB SQL surface duplicating the `query` engine are the deleted forms.

```python signature
from typing import Literal, assert_never

import duckdb
import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, boundary

type SpatialPredicate = Literal["ST_Intersects", "ST_Contains", "ST_DWithin"]


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Literal["join", "point_in_polygon", "h3_bin"] = tag()
    join: tuple[SpatialPredicate, str, str] = case()
    point_in_polygon: tuple[str, str] = case()
    h3_bin: tuple[str, int] = case()

    @staticmethod
    def Join(predicate: SpatialPredicate, left: str, right: str) -> "SpatialQuery":
        return SpatialQuery(join=(predicate, left, right))

    @staticmethod
    def PointInPolygon(points: str, polygons: str) -> "SpatialQuery":
        return SpatialQuery(point_in_polygon=(points, polygons))

    @staticmethod
    def H3Bin(geometry: str, resolution: int = 9) -> "SpatialQuery":
        return SpatialQuery(h3_bin=(geometry, resolution))

    def sql(self) -> str:
        match self:
            case SpatialQuery(tag="join", join=(predicate, left, right)):
                return f"SELECT l.*, r.* FROM {left} l JOIN {right} r ON {predicate}(l.geom, r.geom)"
            case SpatialQuery(tag="point_in_polygon", point_in_polygon=(points, polygons)):
                return f"SELECT p.*, g.* FROM {points} p JOIN {polygons} g ON ST_Contains(g.geom, p.geom)"
            case SpatialQuery(tag="h3_bin", h3_bin=(geometry, resolution)):
                return f"SELECT *, h3_latlng_to_cell(ST_Y(geom), ST_X(geom), {resolution}) AS h3 FROM {geometry}"
            case unreachable:
                assert_never(unreachable)


class SpatialEngine(Struct, frozen=True):
    inputs: dict[str, pa.Table]

    @classmethod
    def of(cls, inputs: dict[str, pa.Table]) -> "SpatialEngine":
        return cls(inputs=inputs)

    def run(self, query: SpatialQuery) -> "RuntimeRail[pa.Table]":
        return boundary(f"geo.spatial.{query.tag}", lambda: self._dispatch(query))

    def _dispatch(self, query: SpatialQuery) -> pa.Table:
        con = duckdb.connect()
        con.install_extension("spatial")
        con.load_extension("spatial")
        con.install_extension("h3", repository="community")
        con.load_extension("h3")
        for name, table in self.inputs.items():
            con.register(f"{name}_raw", table)
            con.sql(f"CREATE VIEW {name} AS SELECT * EXCLUDE wkb, ST_GeomFromWKB(wkb) AS geom FROM {name}_raw")
        result = con.sql(query.sql()).to_arrow_table()
        _ = ContentIdentity.of(f"geo.spatial.{query.tag}", result.to_batches()[0].serialize() if result.num_rows else b"")
        return result
```

## [4]-[RESEARCH]

- [GEOARROW_ENCODING]: the `geopandas` `GeoDataFrame.to_parquet(geometry_encoding=)`/`to_arrow(geometry_encoding=)`/`to_wkb()` surface the `EgressFormat.GEOARROW` and the spatial-input WKB bridge transcribe is catalogue-confirmed against the folder `geopandas` `.api`; the catalogue lists `geometry_encoding='WKB'` as the default, so the `"geoarrow"` value confirms as the GeoParquet 1.1 native-encoding member against the live distribution before the GeoArrow row treats it as the settled smaller-egress encoding.
- [DUCKDB_SPATIAL_H3]: the `duckdb` `connect`/`install_extension`/`load_extension`/`register`/`sql`/`to_arrow_table` Python surface is catalogue-confirmed against the folder `duckdb` `.api`; the extension-loaded SQL functions the `SpatialQuery.sql()` builds — `ST_GeomFromWKB`, `ST_Intersects`/`ST_Contains`/`ST_DWithin`, `ST_X`/`ST_Y`, and the `h3` community-extension `h3_latlng_to_cell` — are SQL-surface members the Python catalogue does not enumerate, so the `spatial`/`h3` extension function spellings and the `install_extension("h3", repository="community")` community-repository keyword confirm against the loaded DuckDB extension catalogs before the spatial SQL treats them as settled. The R-tree prefilter is the DuckDB `spatial` extension's automatic index, not a hand-built structure.

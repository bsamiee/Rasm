# [PY_DATA_CLAIM]

The two-axis geospatial surface, spatial egress, and the columnar spatial-query engine. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio with pyproj backing the CRS engine; `RasterGeoClaim` carries coverage/band/resampling/nodata/transform/CRS over rasterio. Vector and raster are two value objects because band/resampling semantics differ from vector CRS/axis-order. `EgressFormat` is one `StrEnum` carrying its own `write` — GeoJSON/GeoParquet/FlatGeobuf/GeoArrow, the OGR driver IS the enum value, GeoParquet rides geopandas `to_parquet` and GeoArrow rides `to_arrow(geometry_encoding="geoarrow")` — not a tagged union over redundant payloads or a per-format writer family. `SpatialQuery` is one tagged-union axis over the DuckDB-spatial extension (ST joins, point-in-polygon, R-tree prefilter, H3 binning) composing the `query` engine for the SQL path. Every bundle keys by exactly one runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[GEO]`: vector and raster geospatial claims, spatial egress, the GeoArrow encoding.
- `[3]-[SPATIAL]`: the DuckDB-spatial join/index/H3 columnar query engine.

## [2]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio; `RasterGeoClaim` — coverage/band/resampling/nodata/transform/CRS over rasterio; `EgressFormat` the GeoJSON/GeoParquet/flat-vector export as one `StrEnum` whose member value is the OGR driver and whose `write` selects `to_parquet` for GeoParquet and `to_file(driver=value)` otherwise, not a per-format writer family.
- Cases: `EgressFormat` rows `GEOJSON` (`GeoDataFrame.to_file(driver="GeoJSON")`) · `GEOPARQUET` (`GeoDataFrame.to_parquet`, the default GeoParquet 1.1 WKB encoding) · `GEOARROW` (`GeoDataFrame.to_parquet(geometry_encoding="geoarrow")` — the native GeoArrow-encoded GeoParquet 1.1 column, smaller and zero-parse on the wire) · `FLATGEOBUF` (`to_file(driver="FlatGeobuf")` over pyogrio). A new egress format is one `EgressFormat` member; a new geometry family is one `GeometryFamily` row; a new resampling mode is one `rasterio.enums.Resampling` row.
- Entry: `VectorGeoClaim.reproject` reprojects the live geo path through `geopandas.GeoDataFrame.to_crs` (the pyproj-backed CRS engine, axis-order normalized by the geopandas CRS) returning a `RuntimeRail`; `VectorGeoClaim.set_precision` snaps geometry coordinates through `shapely.set_precision(geom, grid_size)` before egress; `RasterGeoClaim.resample` resamples through `rasterio.warp.reproject` keyed by the `rasterio.enums.Resampling` row; `EgressFormat.write` emits the bundle keyed by one `ContentIdentity.of`.
- Packages: `geopandas` (`GeoDataFrame`/`to_crs`/`to_parquet`/`to_file`), `shapely` (`set_precision`/`make_valid`/`to_wkb`), `pyogrio` (`write_dataframe` vector I/O), `pyproj` (the CRS engine), `rasterio` (`open`/`warp.reproject`/`enums.Resampling`), `duckdb` (the spatial join/index engine), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`/`BoundaryFault`).
- Growth: a new geometry family is one column on `VectorGeoClaim`; a new resampling mode is one `rasterio.enums.Resampling` row; a new egress format is one `EgressFormat` member (GeoArrow lands as the `GEOARROW` member); the DuckDB-spatial join/index/H3 engine is the `[3]-[SPATIAL]` `SpatialQuery` axis; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape, a tagged union whose case payload only restates its tag, and a per-format writer family are the deleted forms. The content key derives from the written bytes through exactly one canonical `ContentIdentity.of`.

```python
from __future__ import annotations

from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Any

from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from geopandas import GeoDataFrame


class GeometryFamily(StrEnum):
    POINT = "point"
    LINESTRING = "linestring"
    POLYGON = "polygon"
    MULTIPOLYGON = "multipolygon"


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


class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    def reproject(self, frame: GeoDataFrame, target_crs: str) -> RuntimeRail[GeoDataFrame]:
        return boundary("geo.reproject", lambda: frame.to_crs(target_crs))

    def set_precision(self, frame: GeoDataFrame) -> RuntimeRail[GeoDataFrame]:
        return boundary("geo.precision", lambda: _snap(frame, self.precision))


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: str
    nodata: float
    transform: tuple[float, ...]

    def resample(self, source: Any, target_transform: tuple[float, ...], target_crs: str) -> RuntimeRail[Any]:
        return boundary("geo.resample", lambda: _warp(source, self, target_transform, target_crs))


def _snap(frame: GeoDataFrame, precision: int) -> GeoDataFrame:
    import shapely  # noqa: PLC0415

    return frame.assign(geometry=frame.geometry.apply(lambda g: shapely.set_precision(g, 10.0**-precision)))


def _warp(source: Any, claim: RasterGeoClaim, target_transform: tuple[float, ...], target_crs: str) -> Any:
    from rasterio.enums import Resampling  # noqa: PLC0415
    from rasterio.warp import reproject  # noqa: PLC0415

    return reproject(source, dst_transform=target_transform, dst_crs=target_crs, resampling=Resampling[claim.resampling], dst_nodata=claim.nodata)
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

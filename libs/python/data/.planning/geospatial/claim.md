# [PY_DATA_CLAIM]

The two-axis geospatial surface and spatial egress. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio with pyproj backing the CRS engine; `RasterGeoClaim` carries coverage/band/resampling/nodata/transform/CRS over rasterio. Vector and raster are two value objects because band/resampling semantics differ from vector CRS/axis-order. `EgressFormat` is one `StrEnum` carrying its own `write` — GeoJSON/GeoParquet/FlatGeobuf, the OGR driver IS the enum value and GeoParquet rides geopandas `to_parquet` — not a tagged union over redundant payloads or a per-format writer family; native GeoArrow encoding and a DuckDB-spatial join/index engine land as rows on this same surface. Every bundle keys by exactly one runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[GEO]`: vector and raster geospatial claims, spatial egress, the GeoArrow/DuckDB-spatial engine.

## [2]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio; `RasterGeoClaim` — coverage/band/resampling/nodata/transform/CRS over rasterio; `EgressFormat` the GeoJSON/GeoParquet/flat-vector export as one `StrEnum` whose member value is the OGR driver and whose `write` selects `to_parquet` for GeoParquet and `to_file(driver=value)` otherwise, not a per-format writer family.
- Cases: `EgressFormat` rows `GEOJSON` (`GeoDataFrame.to_file(driver="GeoJSON")`) · `GEOPARQUET` (`GeoDataFrame.to_parquet`) · `FLATGEOBUF` (`to_file(driver="FlatGeobuf")` over pyogrio). A new egress format is one `EgressFormat` member; a new geometry family is one `GeometryFamily` row; a new resampling mode is one `rasterio.enums.Resampling` row.
- Entry: `VectorGeoClaim.reproject` reprojects the live geo path through `geopandas.GeoDataFrame.to_crs` (the pyproj-backed CRS engine, axis-order normalized by the geopandas CRS) returning a `RuntimeRail`; `VectorGeoClaim.set_precision` snaps geometry coordinates through `shapely.set_precision(geom, grid_size)` before egress; `RasterGeoClaim.resample` resamples through `rasterio.warp.reproject` keyed by the `rasterio.enums.Resampling` row; `EgressFormat.write` emits the bundle keyed by one `ContentIdentity.of`.
- Packages: `geopandas` (`GeoDataFrame`/`to_crs`/`to_parquet`/`to_file`), `shapely` (`set_precision`/`make_valid`/`to_wkb`), `pyogrio` (`write_dataframe` vector I/O), `pyproj` (the CRS engine), `rasterio` (`open`/`warp.reproject`/`enums.Resampling`), `duckdb` (the spatial join/index engine), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`/`BoundaryFault`).
- Growth: a new geometry family is one column on `VectorGeoClaim`; a new resampling mode is one `rasterio.enums.Resampling` row; a new egress format is one `EgressFormat` member; native GeoArrow `geometry_encoding="geoarrow"` egress and the DuckDB-spatial join/H3 engine land as rows on this surface; zero new surface.
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
    FLATGEOBUF = "FlatGeobuf"

    def write(self, frame: GeoDataFrame, path: str) -> RuntimeRail[ContentKey]:
        emit = (
            (lambda: frame.to_parquet(path))
            if self is EgressFormat.GEOPARQUET
            else (lambda: frame.to_file(path, driver=self.value))
        )
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

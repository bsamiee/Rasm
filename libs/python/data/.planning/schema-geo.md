# [PY_DATA_SCHEMA_GEO]

Schema claims, the data-contract validation gate, and the two-axis geospatial surface. `SchemaClaim` records field/logical-type/required/nullable/source-evidence; `ContractGate` asserts schema and quality rules over `pandera` (the enforcement `SchemaClaim` records but does not run); `VectorGeoClaim` carries CRS/units/axis-order/geometry-family over geopandas/shapely (pyproj backing the CRS engine); `RasterGeoClaim` carries coverage/band/resampling/nodata over rasterio. Vector and raster are two value objects because band/resampling semantics differ from vector CRS/axis-order.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | SCHEMA    | schema claims, frame admission, the contract gate              |
|   [2]   | GEO       | vector and raster geospatial claims, spatial egress            |

## [2]-[SCHEMA]

- Owner: `SchemaClaim` — the field/logical-type/required/nullable/source-evidence value object; `FrameAdmission` the admission over Arrow/Polars/Pandas/Xarray; `ContractGate` the rail asserting schema/quality rules over `pandera`.
- Entry: `FrameAdmission.admit` admits a frame against a tuple of `SchemaClaim`s returning a `RuntimeRail[AdmittedFrame]`; `ContractGate.assert_rules` runs the `pandera` schema and returns a `RuntimeRail` of the validated frame or an `Error(BoundaryFault)` carrying the failure cases.
- Packages: `pandera` (`DataFrameSchema`/`Column`/`Check`/`validate`), `pyarrow`, `polars`, `pandas`, runtime (`RuntimeRail`/`BoundaryFault`).
- Growth: a new logical type is one column on `SchemaClaim`; a new quality rule is one `pandera.Check` row; zero new surface.
- Boundary: no Persistence migration law, no live Rhino/GH mutation; `SchemaClaim` records the shape and `ContractGate` enforces it — a hand-rolled validation loop and a stringly-typed rule set are the deleted forms; the validation-gate concern the prior topology left a categorical gap is owned here over `pandera`.

```python signature
import pandera as pa
from msgspec import Struct


class SchemaClaim(Struct, frozen=True):
    field: str
    logical_type: str
    required: bool
    nullable: bool
    source_evidence: str


class ContractGate(Struct, frozen=True):
    claims: tuple[SchemaClaim, ...]

    def schema(self) -> pa.DataFrameSchema:
        return pa.DataFrameSchema({
            c.field: pa.Column(c.logical_type, nullable=c.nullable, required=c.required)
            for c in self.claims
        })

    def assert_rules[F](self, frame: F) -> "RuntimeRail[F]":
        return boundary("contract.validate", lambda: self.schema().validate(frame))
```

## [3]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio; `RasterGeoClaim` — coverage/band/resampling/nodata/transform/CRS over rasterio; `SpatialEgress` the GeoJSON/GeoParquet/flat-vector/CRS-normalized export.
- Entry: `VectorGeoClaim.reproject` reprojects through `geopandas.to_crs` (its pyproj-backed CRS engine, axis-order normalized by the geopandas CRS) returning a `RuntimeRail`; `RasterGeoClaim.resample` resamples through `rasterio.warp.reproject`; `SpatialEgress.write` emits GeoJSON/GeoParquet keyed by `ContentIdentity`.
- Packages: `geopandas` (`GeoDataFrame`/`to_crs`/`to_parquet`), `shapely` (geometry ops), `pyogrio` (vector I/O), `rasterio` (`open`/`warp.reproject`/`Resampling`), runtime (`RuntimeRail`/`ContentIdentity`).
- Growth: a new geometry family is one column on `VectorGeoClaim`; a new resampling mode is one `rasterio.Resampling` row; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape is the deleted form; `rasterio` rides the `python_version<'3.15'` marker floor. Both geo owners are `SPIKE` on the wheel floor.

```python signature
from enum import StrEnum

from msgspec import Struct


class GeometryFamily(StrEnum):
    POINT = "point"
    LINESTRING = "linestring"
    POLYGON = "polygon"
    MULTIPOLYGON = "multipolygon"


class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    def reproject[G](self, frame: G, target_crs: str) -> "RuntimeRail[G]":
        return boundary("geo.reproject", lambda: frame.to_crs(target_crs))


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: str
    nodata: float
    transform: tuple[float, ...]
```

## [4]-[RESEARCH]

- [GEO_REPROJECTION]: the `geopandas.to_crs` reprojection axis-order contract, the `to_parquet` egress spelling, and the `rasterio.warp.reproject`/`Resampling` raster path are verified against `.api/api-geopandas.md` and `.api/api-rasterio.md` once a cp315 wheel or the `python_version<'3.15'` marker-floor environment installs the engines.

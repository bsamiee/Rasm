# [PY_DATA_SCHEMA_GEO]

The dataframe-agnostic interop boundary, structural frame admission, and the two-axis geospatial surface. `FrameInterop` is ONE backend-agnostic owner translating polars/pandas/pyarrow frames through `narwhals` keyed by a single `narwhals.Implementation` backend axis, with the Arrow C Data Interface zero-copy rows authored deploy-gated over `nanoarrow`; `FieldShape` is the structural field/dtype/nullable shape derived agnostically from any backend frame through `FrameInterop` (the null-mask read from the live frame, never inferred from dtype kind); `FrameAdmission` proves required `FieldShape`s resolve against the live agnostic schema and routes data-contract enforcement to the canonical `DataQuality`/`SchemaClaim` owned by the sibling `columnar-query` page — there is no second `SchemaClaim` and no second pandera gate here; `VectorGeoClaim` carries CRS/units/axis-order/geometry-family over geopandas/shapely (pyproj backing the CRS engine); `RasterGeoClaim` carries coverage/band/resampling/nodata over rasterio. Vector and raster are two value objects because band/resampling semantics differ from vector CRS/axis-order. The backend axis lives on one interop owner; no parallel polars/pandas/pyarrow adapter types.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                    |
| :-----: | :-------- | :-------------------------------------------------------- |
|   [1]   | INTEROP   | dataframe-agnostic backend translation, Arrow C-data rows |
|   [2]   | ADMISSION | structural field shapes, narwhals frame admission         |
|   [3]   | GEO       | vector and raster geospatial claims, spatial egress       |

## [2]-[INTEROP]

- Owner: `FrameInterop` — ONE backend-agnostic translation owner over `narwhals`, the backend discriminated by a single `narwhals.Implementation` axis (POLARS/PANDAS/PYARROW the admitted rows). `narwhals.from_native` lifts any admitted native frame into the agnostic `nw.DataFrame`; `narwhals.to_native` lowers it back to the requested backend; the backend is never branched into parallel adapter classes. `ArrowCStream` is the deploy-gated Arrow C Data Interface zero-copy carrier over `nanoarrow` — one row on the same interop owner, not a parallel module.
- Entry: `FrameInterop.translate` lifts a native frame through `narwhals.from_native(..., eager_only=True)`, then lowers to the requested `Backend` row via the `_LOWER` dispatch table (`to_polars`/`to_pandas`/`to_arrow`), returning a `RuntimeRail` of the native frame in the target backend; `FrameInterop.schema_of` reads the backend-agnostic `nw.Schema` through `collect_schema()`, reads the per-column null-mask off the live frame, and folds both into a tuple of `FieldShape`s without touching any backend API directly; `FrameInterop.c_stream` exports the zero-copy Arrow C stream capsule through `nanoarrow.c_array_stream`. The narwhals frame-agnostic spine (`from_native`/`to_native`/`Implementation`/`collect_schema`/`to_native_namespace`) runs live on 3.15; `to_arrow`/`to_pandas` and the `nanoarrow` C-data row need pyarrow/nanoarrow and ride the deploy-asset-gate marker floor.
- Packages: `narwhals` (`from_native`/`to_native`/`from_arrow`/`Implementation`/`Schema`/`DataFrame.{to_polars,to_pandas,to_arrow,collect_schema,implementation,to_native,null_count}`), `nanoarrow` (`c_array_stream`/`ArrayStream`), runtime (`RuntimeRail`/`boundary`/`BoundaryFault`).
- Growth: a new backend is one `Backend` enum row plus one `_LOWER` dispatch row; a new interchange protocol is one method on the same owner; zero new surface. The interop concern that a per-backend adapter family (`PolarsAdapter`/`PandasAdapter`/`ArrowAdapter`) would have fragmented is collapsed into one owner with a backend axis.
- Boundary: no compute (the numeric trio / labelled-array ownership stays in `compute`), no durable store, no engine-specific query rail; `narwhals` owns ONLY the agnostic frame translation hop. A per-backend adapter trio, a hand-rolled `isinstance` dispatch over native frame types, and a second schema-derivation path are the deleted forms. `nanoarrow` rides the deploy-asset-gate marker (`python_version<'3.15'`, no cp315 wheel); pyarrow-backed `to_arrow`/`to_pandas` lowering rides the same floor.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final

import narwhals as nw
from msgspec import Struct

from rasm.runtime.rails_resilience import BoundaryFault, Error, RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable


# --- [TYPES] ---------------------------------------------------------------------------
class Backend(StrEnum):
    """The admitted dataframe-agnostic backend axis; the value mirrors narwhals.Implementation.name.lower()."""

    POLARS = "polars"
    PANDAS = "pandas"
    PYARROW = "pyarrow"

    @property
    def implementation(self) -> nw.Implementation:
        return nw.Implementation.from_backend(self.value)


# --- [CONSTANTS] -----------------------------------------------------------------------
# pandas/pyarrow lowering needs the gated engines; only POLARS lowers pyarrow-free on the 3.15 floor.
_GATED_BACKENDS: Final[frozenset[Backend]] = frozenset({Backend.PANDAS, Backend.PYARROW})


# --- [MODELS] --------------------------------------------------------------------------
class FrameInterop(Struct, frozen=True):
    """ONE dataframe-agnostic interop owner. The backend lives on the `Backend` axis, never in parallel adapters.

    The narwhals spine (from_native / to_native / Implementation / collect_schema) is pyarrow-free and runs live
    on 3.15; the `to_arrow`/`to_pandas` lowering rows and the nanoarrow C-data row need the gated engines.
    """

    source: Backend

    def translate(self, frame: Any, target: Backend) -> RuntimeRail[Any]:
        """Lift a native frame into the agnostic nw.DataFrame, then lower to `target` via the _LOWER dispatch row.

        A same-backend translate is the identity lowering; a cross-backend translate routes through the gated
        lowering row when the target is pandas/pyarrow (pyarrow-backed); polars lowering is pyarrow-free.
        """
        return boundary(f"interop.translate.{self.source}->{target}", lambda: _lower(nw.from_native(frame, eager_only=True), target))

    def schema_of(self, frame: Any) -> RuntimeRail[tuple[FieldShape, ...]]:
        """Derive structural FieldShapes agnostically: collect_schema() reads the nw.Schema and null_count() reads
        the live per-column null-mask, both through narwhals without any backend API call. Nullability is the
        observed null-mask, never inferred from dtype kind."""
        return boundary(f"interop.schema.{self.source}", lambda: _shapes(nw.from_native(frame, eager_only=True)))

    def namespace(self) -> RuntimeRail[Any]:
        """Resolve the live native namespace module for the source backend (narwhals Implementation reflection)."""
        return boundary(f"interop.namespace.{self.source}", self.source.implementation.to_native_namespace)

    def c_stream(self, frame: Any) -> RuntimeRail[ArrowCStream]:
        """Export the zero-copy Arrow C Data Interface stream capsule over nanoarrow (deploy-gated)."""
        return _CStreamGate(frame).export()


# --- [OPERATIONS] ----------------------------------------------------------------------
def _shapes(frame: nw.DataFrame[Any]) -> tuple[FieldShape, ...]:
    """Fold a backend-agnostic frame into structural FieldShapes: collect_schema() carries the logical dtype and
    null_count() carries the observed null-mask, so nullability is the live frame's actual state, not a dtype guess."""
    schema = frame.collect_schema()
    nulls = frame.null_count().to_native()
    null_by_field = {name: bool(int(nulls[name][0])) for name in schema.names()}
    return tuple(
        FieldShape(field=name, logical_type=str(dtype), nullable=null_by_field.get(name, False), source_evidence=f"narwhals:{dtype!r}")
        for name, dtype in schema.items()
    )


# The backend-lowering dispatch table: each Backend row owns its narwhals lowering method name. A new backend
# is one row here, never a new adapter class. POLARS lowers pyarrow-free; PANDAS/PYARROW route to gated engines.
_LOWER: Final[dict[Backend, Callable[[nw.DataFrame[Any]], Any]]] = {
    Backend.POLARS: lambda f: f.to_polars(),
    Backend.PANDAS: lambda f: f.to_pandas(),
    Backend.PYARROW: lambda f: f.to_arrow(),
}


def _lower(frame: nw.DataFrame[Any], target: Backend) -> Any:
    """Dispatch the agnostic frame to its native target. A pyarrow/pandas target needs the gated engine present;
    when absent the boundary rail folds the underlying ModuleNotFoundError into a BoundaryFault."""
    return _LOWER[target](frame)


# --- [BOUNDARIES] (deploy-asset-gate: nanoarrow carries python_version<'3.15', no cp315 wheel) ----------
class ArrowCStream(Struct, frozen=True):
    """The Arrow C Data Interface zero-copy carrier: a PyCapsule stream handle plus its schema fingerprint.

    Documented nanoarrow spelling (verified-by-stability, the native-BLAS deploy posture): nanoarrow.c_array_stream
    wraps any __arrow_c_stream__-exporting object into a nanoarrow.ArrayStream whose .__arrow_c_stream__() yields
    the capsule consumed zero-copy by any Arrow C-data importer. Carried as Any because the capsule is opaque.
    """

    capsule: Any
    schema_repr: str


class _CStreamGate(Struct, frozen=True):
    """nanoarrow C-data export behind the deploy-asset-gate marker floor; the body transcribes the documented
    c_array_stream spelling and folds an absent engine into a BoundaryFault through the canonical Boundary
    factory rather than raising or constructing the case slot directly."""

    frame: Any

    def export(self) -> RuntimeRail[ArrowCStream]:
        try:
            import nanoarrow  # noqa: PLC0415
        except ModuleNotFoundError:
            return Error(BoundaryFault.Boundary("nanoarrow", "deploy-asset-gate: install nanoarrow on the marker floor before re-reflect"))
        stream = nanoarrow.c_array_stream(self.frame)
        return boundary("interop.c_stream", lambda: ArrowCStream(capsule=stream.__arrow_c_stream__(), schema_repr=repr(stream.schema)))
```

## [3]-[ADMISSION]

- Owner: `FieldShape` — the structural field/logical-type/nullable/source-evidence value object derived agnostically by `FrameInterop.schema_of` from any backend frame; `FrameAdmission` the one admission path over any `narwhals`-admitted backend. There is NO local `SchemaClaim` and NO local pandera gate: the data-contract enforcement and its failure-case receipt are the canonical `DataQuality`/`QualityRule`/`SchemaClaim` owned by the sibling `columnar-query` page over `pandera.polars`. `FieldShape` is a distinct *structural* shape (field presence + dtype), not a re-mint of the quality `SchemaClaim`.
- Entry: `FrameAdmission.admit` lifts a native frame through `narwhals.from_native`, reads `collect_schema()`, asserts every required `FieldShape` resolves against the live agnostic schema, and returns a `RuntimeRail[AdmittedFrame]` — one admission path for every backend, never a per-backend branch. Data-contract validation is then the canonical `DataQuality.validate(frame)` from `columnar-query`, which folds `QualityRule` rows into one `pandera.polars.DataFrameSchema` and records a `SchemaClaim` carrying `failure_cases`; this page never authors a second pandera schema.
- Packages: `narwhals` (`from_native`/`collect_schema`/`Implementation.name`), runtime (`RuntimeRail`/`BoundaryFault`). The pandera enforcement path is reflected (`pandera.polars`, 0.31.1) and owned by `columnar-query`; it is not re-declared here.
- Growth: a new structural attribute is one column on `FieldShape`; a new quality rule is one `QualityRule`/`CheckKind` row on the canonical `DataQuality`; a new backend is admitted free by `narwhals` with zero admission-cluster change.
- Boundary: no Persistence migration law, no live Rhino/GH mutation; `FieldShape` records the structural shape and `FrameAdmission` proves required presence — a hand-rolled validation loop, a stringly-typed rule set, a per-backend admission branch, a duplicate `SchemaClaim`, and a second pandera gate are the deleted forms. Quality enforcement routes to the canonical `DataQuality` owner, never re-minted here.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from typing import Any

import narwhals as nw
from expression import Error, Ok
from msgspec import Struct

from rasm.data.columnar_query import DataQuality, QualityRule, SchemaClaim
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail


# --- [MODELS] --------------------------------------------------------------------------
class FieldShape(Struct, frozen=True):
    """The structural shape of one frame column: field name, logical dtype, observed nullability, and the
    narwhals dtype evidence. Distinct from the canonical quality SchemaClaim — this carries structure, not
    pass/fail. Nullability is the observed null-mask read by FrameInterop.schema_of, never a dtype-kind guess."""

    field: str
    logical_type: str
    nullable: bool
    source_evidence: str


class AdmittedFrame(Struct, frozen=True):
    """A frame proven to satisfy every required FieldShape against its live agnostic narwhals schema, carrying
    the structural shapes; data-contract enforcement is then the canonical DataQuality.validate over this frame."""

    frame: Any
    backend: str
    shapes: tuple[FieldShape, ...]


class FrameAdmission(Struct, frozen=True):
    """ONE admission path over any narwhals-admitted backend; the backend is read off the agnostic frame, never
    branched. Required FieldShapes absent from the live schema fold into a BoundaryFault.Boundary listing the
    misses. `quality` carries the canonical DataQuality contract enforced after structural admission, not a
    re-minted pandera gate."""

    required: tuple[FieldShape, ...]
    quality: DataQuality

    @classmethod
    def of(cls, required: tuple[FieldShape, ...], *rules: QualityRule) -> "FrameAdmission":
        return cls(required=required, quality=DataQuality.of(*rules))

    def admit(self, frame: Any) -> RuntimeRail[AdmittedFrame]:
        agnostic = nw.from_native(frame, eager_only=True)
        present = set(agnostic.collect_schema().names())
        missing = tuple(s.field for s in self.required if s.field not in present)
        if missing:
            return Error(BoundaryFault.Boundary("frame.admit", f"missing required fields: {', '.join(missing)}"))
        return Ok(AdmittedFrame(frame=frame, backend=agnostic.implementation.name.lower(), shapes=self.required))

    def enforce(self, admitted: AdmittedFrame) -> RuntimeRail[SchemaClaim]:
        """Route data-contract enforcement to the canonical DataQuality owner over its pandera.polars backend; the
        SchemaClaim it records (status / failure_count / failures / content_key) is the one quality receipt, never
        re-minted here. The agnostic frame lowers to a polars LazyFrame so the polars validation pushes into scan."""
        return self.quality.validate(nw.from_native(admitted.frame, eager_only=True).to_polars().lazy())
```

## [4]-[GEO]

- Owner: `VectorGeoClaim` — CRS/units/axis-order/geometry-family/precision/transform-provenance over geopandas/shapely/pyogrio; `RasterGeoClaim` — coverage/band/resampling/nodata/transform/CRS over rasterio; `SpatialEgress` the GeoJSON/GeoParquet/flat-vector/CRS-normalized export as ONE tagged union over an `EgressFormat` axis, not a per-format writer family.
- Cases: `SpatialEgress` tagged union — `GeoJson` (`GeoDataFrame.to_file(driver="GeoJSON")`) · `GeoParquet` (`GeoDataFrame.to_parquet`) · `FlatGeobuf` (`to_file(driver="FlatGeobuf")` over pyogrio) — one discriminant folded total by `match`/`case` in `_emit`, not a parallel writer family. A new egress format is one `SpatialEgress` row; a new geometry family is one `GeometryFamily` row; a new resampling mode is one `rasterio.enums.Resampling` row; zero new surface.
- Entry: `VectorGeoClaim.reproject` reprojects the live geo path through `geopandas.GeoDataFrame.to_crs` (its pyproj-backed CRS engine, axis-order normalized by the geopandas CRS) returning a `RuntimeRail`; `VectorGeoClaim.set_precision` snaps geometry coordinates through the documented `shapely.set_precision(geom, grid_size)` grid before egress; `RasterGeoClaim.resample` resamples through `rasterio.warp.reproject` keyed by the `rasterio.enums.Resampling` row; `SpatialEgress.write` discriminates the `EgressFormat` and emits the bundle keyed by one `ContentIdentity.key`.
- Packages: `geopandas` (`GeoDataFrame`/`to_crs`/`to_parquet`/`to_file`), `shapely` (`set_precision`/`make_valid`/`to_wkb`), `pyogrio` (`write_dataframe` vector I/O), `rasterio` (`open`/`warp.reproject`/`enums.Resampling`), runtime (`RuntimeRail`/`ContentIdentity`/`boundary`/`BoundaryFault`). The geo engines transitively gate through `shapely`/`pyproj`/`pyogrio` and are authored against the documented API behind the deploy-asset-gate marker floor, never invented members.
- Growth: a new geometry family is one column on `VectorGeoClaim`; a new resampling mode is one `rasterio.enums.Resampling` row; a new egress format is one `SpatialEgress` case; zero new surface.
- Boundary: no host mutation, no durable store; a single `GeoClaim` folding band/resampling and CRS/axis-order into one under-collapsed shape and a per-format writer family are the deleted forms; `geopandas`/`shapely`/`pyogrio`/`rasterio` all ride the `python_version<'3.15'` marker floor (no cp315 wheel), authored against the documented API behind the deploy-asset-gate marker. The egress `match` folds the `EgressFormat` discriminant total and the content key derives from the written bytes through exactly one canonical `ContentIdentity.key`.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from enum import StrEnum
from typing import TYPE_CHECKING, Any, Literal

from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable

    from geopandas import GeoDataFrame  # deploy-asset-gate marker floor — documented to_crs/to_parquet/to_file spellings


# --- [TYPES] ---------------------------------------------------------------------------
class GeometryFamily(StrEnum):
    POINT = "point"
    LINESTRING = "linestring"
    POLYGON = "polygon"
    MULTIPOLYGON = "multipolygon"


class EgressFormat(StrEnum):
    """The spatial egress format axis; each row pins one OGR driver / GeoParquet writer."""

    GEOJSON = "GeoJSON"
    GEOPARQUET = "GeoParquet"
    FLATGEOBUF = "FlatGeobuf"


# --- [MODELS] --------------------------------------------------------------------------
class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    def reproject(self, frame: GeoDataFrame, target_crs: str) -> RuntimeRail[GeoDataFrame]:
        """Live geo path: geopandas.GeoDataFrame.to_crs through the pyproj-backed CRS engine. GATED — documented
        spelling, axis-order normalized by the geopandas CRS, verified-by-stability on the marker floor."""
        return boundary("geo.reproject", lambda: frame.to_crs(target_crs))

    def set_precision(self, frame: GeoDataFrame) -> RuntimeRail[GeoDataFrame]:
        """Snap coordinates to the claim's precision grid via shapely.set_precision before egress. GATED —
        documented shapely spelling; grid_size derives from the declared decimal precision."""
        return boundary("geo.precision", lambda: _snap(frame, self.precision))


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: str
    nodata: float
    transform: tuple[float, ...]

    def resample(self, source: Any, target_transform: tuple[float, ...], target_crs: str) -> RuntimeRail[Any]:
        """Resample/reproject a raster through rasterio.warp.reproject keyed by the rasterio.enums.Resampling
        row named on the claim. GATED — documented spelling; rasterio carries python_version<'3.15'."""
        return boundary("geo.resample", lambda: _warp(source, self, target_transform, target_crs))


@tagged_union(frozen=True)
class SpatialEgress:
    """ONE export owner over the EgressFormat axis. Each case carries the format row; _emit folds the
    discriminant total and emits the bundle keyed by exactly one ContentIdentity.key, never re-minted."""

    tag: Literal["geo_json", "geo_parquet", "flat_geobuf"] = tag()
    geo_json: EgressFormat = case()
    geo_parquet: EgressFormat = case()
    flat_geobuf: EgressFormat = case()

    @staticmethod
    def GeoJson() -> SpatialEgress:
        return SpatialEgress(geo_json=EgressFormat.GEOJSON)

    @staticmethod
    def GeoParquet() -> SpatialEgress:
        return SpatialEgress(geo_parquet=EgressFormat.GEOPARQUET)

    @staticmethod
    def FlatGeobuf() -> SpatialEgress:
        return SpatialEgress(flat_geobuf=EgressFormat.FLATGEOBUF)

    def write(self, frame: GeoDataFrame, path: str) -> RuntimeRail[ContentKey]:
        """Fold the egress discriminant total: an unmatched tag rails to Error(BoundaryFault) rather than
        falling through, so adding an EgressFormat row without a writer arm fails closed, never silently keys
        an unwritten file. The write side-effect rides one boundary; the content key derives from the bytes."""
        return _emit(self, frame, path)


# --- [BOUNDARIES] (deploy-asset-gate: geopandas/shapely/pyogrio/rasterio carry python_version<'3.15', no cp315 wheel) ---
def _snap(frame: GeoDataFrame, precision: int) -> GeoDataFrame:
    """CATALOGUE_PENDING — documented shapely.set_precision spelling (TASKLOG PY_API_002a).

    shapely.set_precision(geometry, grid_size=10**-precision, mode="valid_output") snaps each geometry's
    coordinates onto the grid; geopandas applies it elementwise over the active geometry column.
    """
    import shapely  # noqa: PLC0415

    return frame.assign(geometry=frame.geometry.apply(lambda g: shapely.set_precision(g, 10.0**-precision)))


def _warp(source: Any, claim: RasterGeoClaim, target_transform: tuple[float, ...], target_crs: str) -> Any:
    """CATALOGUE_PENDING — documented rasterio.warp.reproject spelling (TASKLOG PY_API_002a).

    rasterio.warp.reproject(source, destination, src_transform, src_crs, dst_transform=target_transform,
    dst_crs=target_crs, resampling=rasterio.enums.Resampling[claim.resampling]) reprojects band-by-band.
    """
    from rasterio.enums import Resampling  # noqa: PLC0415
    from rasterio.warp import reproject  # noqa: PLC0415

    return reproject(source, dst_transform=target_transform, dst_crs=target_crs, resampling=Resampling[claim.resampling], dst_nodata=claim.nodata)


def _writer(egress: SpatialEgress, frame: GeoDataFrame, path: str) -> RuntimeRail[Callable[[], None]]:
    """Resolve the documented geopandas/pyogrio writer thunk for the discriminant. Total fold: an unmatched tag
    rails to Error(BoundaryFault.Boundary) so a new EgressFormat row without a writer arm fails closed."""
    match egress.tag:
        case "geo_parquet":
            return Ok(lambda: frame.to_parquet(path))
        case "geo_json":
            return Ok(lambda: frame.to_file(path, driver="GeoJSON"))
        case "flat_geobuf":
            return Ok(lambda: frame.to_file(path, driver="FlatGeobuf"))
        case other:
            return Error(BoundaryFault.Boundary("geo.egress", f"unmatched egress format: {other}"))


def _emit(egress: SpatialEgress, frame: GeoDataFrame, path: str) -> RuntimeRail[ContentKey]:
    """Bind the total writer fold, run the write side-effect under one boundary, then derive the content key from
    the written bytes through the canonical ContentIdentity.key(fmt, payload) — never a second egress key."""
    from pathlib import Path  # noqa: PLC0415

    return _writer(egress, frame, path).bind(
        lambda thunk: boundary(
            f"geo.egress.{egress.tag}",
            lambda: (thunk(), ContentIdentity.key(fmt=egress.tag, payload=Path(path).read_bytes()))[1],
        )
    )
```

## [5]-[RESEARCH]

- [INTEROP_NARWHALS]: the `narwhals` frame-agnostic spine (`from_native`/`to_native`/`Implementation.from_backend`/`to_native_namespace`/`DataFrame.collect_schema`/`DataFrame.implementation`/`DataFrame.null_count`) is REFLECTED live against narwhals 2.22.1 and verified pyarrow-free; `null_count()` returns a single-row frame of per-column null counts read by `FrameInterop.schema_of` so `FieldShape.nullable` is the observed mask, never a dtype-kind guess. The `to_arrow`/`to_pandas` lowering rows and the `nanoarrow.c_array_stream` zero-copy C-data row need pyarrow/nanoarrow and are authored deploy-gated until a cp315 wheel or the marker-floor environment installs them (suite TASKLOG `PY_API_002a`).
- [SCHEMA_OWNERSHIP]: the data-contract enforcement and its failure-case receipt are the canonical `DataQuality`/`QualityRule`/`CheckKind`/`SchemaClaim` owned by the sibling `columnar-query` page over `pandera.polars` (REFLECTED 0.31.1, folding `pandera.errors.SchemaErrors.failure_cases`). This page consumes that owner and minted no second `SchemaClaim` and no second pandera gate. ALIGN NOTE for the orchestrator: `.api/api-pandera.md` still records pandera as "un-reflectable / no cp315 wheel" — STALE; reconcile against the reflected 0.31.1 capture (out of this page's write-scope).
- [GEO_REPROJECTION]: the `geopandas.GeoDataFrame.to_crs` reprojection axis-order contract, the `to_parquet`/`to_file(driver=...)` egress spelling, the `shapely.set_precision` grid snap, and the `rasterio.warp.reproject`/`rasterio.enums.Resampling` raster path are authored against `.api/api-geopandas.md`/`.api/api-shapely.md`/`.api/api-rasterio.md` behind the deploy-asset-gate marker; they verify once a cp315 wheel or the `python_version<'3.15'` marker-floor environment installs the engines. ALIGN NOTE: `geopandas` and `rasterio` are absent from the campaign GATED roster (which names `pyarrow, scipy, shapely, icechunk, nanoarrow, connectorx, pyogrio, pyproj`); the floor is sound because both gate transitively through `shapely`/`pyproj`/`pyogrio` (all on the roster), but the roster gap needs classification by the orchestrator.

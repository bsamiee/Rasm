# [PY_DATA_GEOSPATIAL]

Geospatial CLAIMS plane — one third of the spatial triptych, beside the `spatial/query#SPATIAL` in-DB engine and the `spatial/grid#GRID` DGG plane. `VectorGeoClaim` carries CRS/units/axis-order/geometry-family/precision over geopandas/shapely/pyogrio with pyproj backing the axis-order-aware `reproject` prelude and one `VectorOp` in-frame vector-algebra axis; `RasterGeoClaim` carries coverage/band/resampling/nodata/CRS with one `RasterOp` coverage axis spanning the in-memory and streaming/remote/VRT/sample/COG-write rows; `EgressFormat` is one `StrEnum` whose member value IS the OGR driver and whose `write` it carries. STAC claims live on `spatial/catalog#CATALOG` — `StacGeoClaim`/`StacTableOp` are re-homed to the STAC-table owner, so this page holds no catalog import.

`RasterGeoClaim.transform` is the provenance affine the `spatial/catalog#ASSETS` `AssetFold` constructs from `proj:transform`, or the typed ABSENCE it carries where the asset declares none. `GEOARROW` egress is the NATIVE buffer path — `to_arrow(geometry_encoding="geoarrow")` exports zero-copy extension arrays serialized as Arrow IPC, never a parquet byte-roundtrip — and `geoarrow_wire` is the `geoarrow-rust-compute` hand-off sharing the `dotnet:Rasm.Compute` GLB wire layout. The GDAL split predicate is format coverage: a format the `geoarrow-rust-io` readers spell natively rides the `[04]-[NATIVE]` band and its `EgressFormat` writer arm, and pyogrio keeps the OGR long tail — shapefile, GPKG, and every driver the rust surface does not parse — so the native family is a fast path beside the GDAL owner, never a second format owner. In-frame kernels split on the same law: a verb GEOS spells vectorized stays a shapely arm, and only the ellipsoidal metric family the rust surface alone vectorizes rides the geoarrow export leg. Every network-bearing read routes its blocking provider call through `guarded(RetryClass.HTTP, on_thread, ...)`, the `THREAD_BAND`-bounded hop, elected by the dispatch row the op's OWN value resolves rather than by which entrypoint a caller picked; every bundle keys by one runtime `ContentIdentity` folding the shared `tabular/columnar#SCAN` `QueryReceipt`. geopandas/shapely/rasterio ride the Forge scientific source build band and bind in-process, never across a subprocess seam: each declares ONE module-scope `lazy import`/`lazy from` line and reifies on the first operation that dereferences it, so the eager module-level form the manifest bans never appears and an unearned function-local one is the same deleted form. The `[03]-[COVERAGE]` `_registered` seam is the page's ONE survivor — importing `rioxarray` IS the `.rio` accessor registration, a module-body side effect a lazy binding no leg touches would never fire.

## [01]-[INDEX]

- [02]-[GEO]: the `VectorGeoClaim`/`RasterGeoClaim` claim owners — the `VectorOp`/`RasterOp` axes, the `_RASTER_ROW` crossing rows, the `GeoreferenceFact` CRS source, the `VectorIngress` pushdown row, the `EgressFormat` egress, the `geoarrow_wire` hand-off.
- [03]-[COVERAGE]: the `rioxarray` CF bridge — georeferenced read, the bare-ndarray lift onto the CF plane, the CF-side COG write.
- [04]-[NATIVE]: the GDAL-free `geoarrow-rust-io` ingress band — the `_NATIVE_ROW` capability roster and its `NativeIngress` gate, the PostGIS query row, the extension-type registration latch.

## [02]-[GEO]

- Owner: `resampling` is the claim-level default an op overrides through `resampling or self.resampling`, never a per-op factory literal. `nodata` is DECLARED absence and `_fill` is its ONE authority: a claim whose asset declared none carries `Nothing`, and every rasterio call then elects that provider's own absent form by OMITTING the keyword its `NodataSlot` member names, because a float standing in for an undeclared sentinel — `0.0` above all — masks every genuine pixel equal to it across the window, stream, VRT, remote, mosaic, mask, reproject, and COG-creation reads at once.
- Cases: `VectorOp.Predicate` rails because `dwithin` is the one accessor carrying a radius — presence and predicate must agree or no servable request exists, so the arm below reads the bound unwrapped and never substitutes a zero for an unbounded one. `Stream` folds each `block_windows` tile straight into one pre-allocated destination slice through `read(out=)`, so peak memory is the decimated destination plus one block — the one measured streaming-IO kernel where the `for` over tiles is the platform-forced boundary exemption. `VectorIngress`'s bare-`.dbf` attribute reads route through the same ESRI Shapefile driver, closing the struck-`csvkit` foreign-decode gap.
- Entry: `RasterGeoClaim.apply` is the ONE raster entry over the whole `RasterOp` union, and `_RASTER_ROW` — read off the op's OWN value through `_row`, never off which method a caller reached for — elects everything the crossing decides: a `Crossing.REMOTE` row rides `guarded(RetryClass.HTTP, on_thread, ...)` under a CLIENT span and may abandon its band slot on a tripped deadline, a `Crossing.LOCAL` row rides the plain banded hop under an INTERNAL one. That same row answers whether the arm opens its OWN dataset off the op value, so an arm that reads a caller handle and was handed none refuses BEFORE the band hop instead of dying inside the worker. Every op is an OTel span around a `boundary` fence binding its real provider-fault root (`RasterioError`/`ShapelyError`/`CRSError`/`DataSourceError`), never an un-narrowed `Exception`; the `reproject` prelude normalizes every binary operand onto one CRS with a no-op short-circuit, `to_crs` when the transform has an inverse and `set_crs` for a metadata-only label. Self-opening raster rows enter their own dataset inside one `ExitStack`, so the GDAL handle closes on the boundary exit before the railed receipt derives.
- Law: `GeoreferenceFact` is the model-minted CRS source on the `reproject` prelude — the geometry sibling's IFC georeference band crosses map conversion, projected CRS, and true north as ONE decoded wire fact, and a fact-bearing reproject lifts site-local engineering coordinates through the helmert similarity onto the model's own projected CRS, so site claims reach map frames off model truth instead of a caller-supplied CRS guess. Every one of the eight fields is REQUIRED on the wire and the prelude takes the fact as `Option`, because the producer already answers an ungeoreferenced model with typed absence: a defaulted abscissa, ordinate, or scale here collapses a fact declaring NO map conversion onto one declaring the identity, and `to_map` then publishes site-local engineering coordinates as map coordinates. Two coefficients the similarity cannot invert — a zero-length direction and a zero scale — refuse together at `decoded`, the one admission seam, so the transform divides by a norm already proven rather than by a fabricated fallback. A dataset's own file CRS stays the claim-carried origin this fact never overrides; `true_north` rides the fact as declared evidence and never enters the map transform, because the map conversion already orients the eastings axis.
- Growth: a new vector operation is one `VectorOp` case; a new raster operation is one `RasterOp` case and one `_RASTER_ROW` row naming its crossing, handle demand, and abandon posture; a new transport class is one `Crossing` member whose retry envelope and span kind DERIVE on the member; a new nodata-bearing provider call is one `NodataSlot` member spelling that provider's own keyword; a new linear or geodesic verb one `LinearKind`/`GeodesicKind` row; a new constructive op one `ConstructKind` row plus its `_CONSTRUCT` behavior row; a new binary predicate one `JoinPredicate` row; a new resampling mode one `Resampling` literal arm mapped at the edge; a new VSI scheme one `VsiScheme` row; a new egress format one `EgressFormat` member, its writer riding the native arm when `geoarrow-rust-io` spells it and the OGR driver value otherwise; a new CRS source is one field on `GeoreferenceFact` landed at BOTH ends of the seam in the same pass; zero new surface; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.GEOSPATIAL` in this module's one `RAISES` table, which every section anchors on.
- Boundary: no host mutation, no durable store; no STAC claim or NDJSON-interchange arm on this page — the catalog owner homes them, and the STAC-interchange providers bind only inside it; `WarpedVRT` is GDAL-native streamed reproject, never a second byte-window transport beside the `tabular/egress` `obstore` rail.

```python signature
from contextlib import ExitStack
from enum import StrEnum
from pathlib import Path
from types import ModuleType
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace
from opentelemetry.trace import SpanKind

# the Forge scientific band — GDAL, GEOS, PROJ, and the rust geoarrow kernels — declares once here and reifies on
# first operation, so an unrelated import of this module pays none of it. Every module-scope row over the band
# carries a member NAME (`_NATIVE_WRITER`) or a call-time thunk; a cell holding a live provider attribute would
# reify the proxy at import and re-open the eager band the manifest bans.
lazy import geopandas as gpd
lazy import pyarrow as pa
lazy import pyarrow.feather as paf
lazy import pyogrio
lazy import pyproj
lazy import rasterio
lazy import shapely
lazy import xarray as xr
lazy from geoarrow import pyarrow as ga
lazy from geoarrow.rust import compute as gac
lazy from geoarrow.rust import io as gio
lazy from geoarrow.rust.core import from_geopandas
lazy from pyogrio.errors import DataSourceError
lazy from pyproj.exceptions import CRSError
lazy from rasterio import features, mask, merge, warp, windows
lazy from rasterio.enums import MergeAlg, Resampling as RioResampling
lazy from rasterio.errors import RasterioError, RasterioIOError
lazy from rasterio.io import MemoryFile
lazy from rasterio.vrt import WarpedVRT
lazy from shapely.errors import ShapelyError

from rasm.data.tabular.columnar import QueryReceipt
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, FaultRow, RuntimeRail, async_boundary, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.roots import origin
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Callable

    from geopandas import GeoDataFrame, GeoSeries
    from rasterio import DatasetReader

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.geospatial")

# the raise anchors the retried legs on this page key on: `reliability/resilience#RESILIENCE` `guarded` takes the
# caller's own rostered `at: FaultRow[L]`, so the breaker arc, the rate bucket, the span, and the lifted fault all
# derive ONE coordinate the roster proves against a real module — the free `subject=<str>` it retired could spell a
# leg this package never declares. Both rows are network-bearing legs, so both declare TRANSIENT.
RASTER_REMOTE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="raster.remote", arm="boundary", defect="raster-read", retriability=TRANSIENT
)
# the local twin of the row above: the crossing row elects which one the entry reaches, so the two arms of one
# dispatch never share a posture the way one subject string made them. A `/vsicurl/` read is a dependency a
# re-issue may clear; a local GDAL read over the same file refuses identically.
RASTER_LOCAL: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="raster.local", arm="boundary", defect="raster-read", retriability=TERMINAL
)
POSTGIS_QUERY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="postgis", arm="boundary", defect="postgis-query", retriability=TRANSIENT
)
# the rest of this module's raise roster beside them. Posture splits on what a re-offer can clear, never on the
# entrypoint: every leg crossing a file, a store, or a driver declares TRANSIENT, while the pure in-memory lowerings
# and every caller-repairable construction gate declare TERMINAL. `slots` NAMES each gate's coordinates, so the
# semicolon-joined message bodies these rows replace become fields a consumer gates on rather than prose it parses.
GEO_EGRESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="egress", arm="boundary", defect="egress-refused", retriability=TRANSIENT
)
GEO_INGRESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="ingress", arm="boundary", defect="ingress-refused", retriability=TRANSIENT
)
GEO_WIRE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="wire", arm="boundary", defect="geoarrow-wire", retriability=TERMINAL
)
# ONE row for the whole vector axis: every arm walks frames already in memory on one banded hop under one narrowed
# raise surface, so the op that ran rides `rasm.geo.op` on the span this row's fence opens beneath.
GEO_VECTOR: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="vector", arm="boundary", defect="vector-op", retriability=TERMINAL
)
GEO_DECODE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="georef.decode", arm="boundary", defect="georef-decode", retriability=TERMINAL
)
# ONE row for the whole degenerate-coefficient fold: the producer's own `unwirable` census arrives as one subject,
# so a caller repairing the direction does not discover the scale by decoding again.
GEO_DEGENERATE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="georef.invert", arm="config", defect="coefficient-degenerate", retriability=TERMINAL,
    slots=("coefficients",),
)
GEO_HANDLE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="raster.handle", arm="config", defect="source-handle-absent", retriability=TERMINAL
)
# the `dwithin` distance is a BICONDITIONAL the factory proves before the op exists, exactly as `spatial/query#SPATIAL`
# `SpatialQuery.Join` proves its own: `dwithin` is the one predicate accessor carrying a radius and no other admits
# one. The deleted `distance if distance is not None else 0.0` forged a ZERO radius for an undeclared bound, so an
# unbounded request returned the exactly-touching set and reported it as a within-distance answer — the
# `docs/laws/scars.md` `[FORGED_ZERO]` class. The `join` arm already degenerated honestly onto `sjoin_nearest`; the
# predicate accessor has no unbounded counterpart, so its corner refuses where the caller can still repair it.
GEO_UNBOUNDED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="predicate", arm="config", defect="distance-mismatch", retriability=TERMINAL, slots=("predicate",)
)
COVERAGE_OPEN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="coverage.open", arm="boundary", defect="coverage-open", retriability=TRANSIENT
)
COVERAGE_LIFT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="coverage.lift", arm="boundary", defect="coverage-lift", retriability=TERMINAL
)
COVERAGE_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="coverage.write", arm="boundary", defect="cog-write", retriability=TRANSIENT
)
# the native gate names the FORMAT beside the capability census it outran, so one row spans every format's corner
# and the format never has to ride a minted subject to stay recoverable.
NATIVE_UNSERVED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="native.gate", arm="config", defect="capability-unserved", retriability=TERMINAL,
    slots=("format", "unserved"),
)
NATIVE_READ: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GEOSPATIAL, point="native", arm="boundary", defect="native-read", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    RASTER_REMOTE,
    RASTER_LOCAL,
    POSTGIS_QUERY,
    GEO_EGRESS,
    GEO_INGRESS,
    GEO_WIRE,
    GEO_VECTOR,
    GEO_DECODE,
    GEO_DEGENERATE,
    GEO_HANDLE,
    GEO_UNBOUNDED,
    COVERAGE_OPEN,
    COVERAGE_LIFT,
    COVERAGE_WRITE,
    NATIVE_UNSERVED,
    NATIVE_READ,
]))


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
    FRECHET = "frechet_distance"


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
            match self:
                case EgressFormat.GEOARROW:
                    # native zero-copy geoarrow extension arrays written as Arrow IPC, not a driver write.
                    paf.write_feather(pa.table(frame.to_arrow(geometry_encoding="geoarrow")), path)
                case _ if self in _NATIVE_WRITER:
                    # rust-spelled formats write GDAL-free: one from_geopandas crossing, then the native writer —
                    # write_flatgeobuf packs its R-tree index by default, write_parquet keeps the interoperable
                    # WKB encoding, and the writer name resolves at the call seam so no table row reifies a proxy.
                    getattr(gio, _NATIVE_WRITER[self])(from_geopandas(frame), path)
                case _:
                    # OGR long-tail arm: the member value IS the driver for every format the rust surface does not spell.
                    pyogrio.write_dataframe(frame, path, driver=self.value, use_arrow=True)
            return Path(path).read_bytes()

        with _TRACER.start_as_current_span(f"geo.egress.{self.value}", attributes={"rasm.geo.format": self.value}):
            return boundary(GEO_EGRESS, emit, catch=(DataSourceError, OSError, ValueError)).bind(
                lambda payload: ContentIdentity.of(self.value, payload)
            )


# rust-spelled egress rows: the named GDAL split as data — member -> `geoarrow.rust.io` writer name.
_NATIVE_WRITER: Final[Map[EgressFormat, str]] = Map.of_seq([
    (EgressFormat.GEOPARQUET, "write_parquet"),
    (EgressFormat.FLATGEOBUF, "write_flatgeobuf"),
    (EgressFormat.GEOJSON, "write_geojson"),
])


@tagged_union(frozen=True)
class VectorOp:
    tag: Literal["join", "overlay", "dissolve", "clip", "construct", "predicate", "linear", "geodesic"] = tag()
    join: tuple[JoinPredicate, "GeoDataFrame", JoinHow, float | None] = case()
    overlay: tuple["GeoDataFrame", SetOp] = case()
    dissolve: tuple[tuple[str, ...], str] = case()
    clip: tuple["GeoDataFrame", bool] = case()
    construct: tuple[ConstructKind, float] = case()
    predicate: tuple[JoinPredicate, "GeoDataFrame", Option[float]] = case()
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
    def Predicate(name: JoinPredicate, other: "GeoDataFrame", distance: Option[float] = Nothing) -> "RuntimeRail[VectorOp]":
        # the ONE distance corner on this axis, proved where the caller can still repair it: `dwithin` is the only
        # accessor taking a radius, so presence and predicate must agree or no servable request exists. Below the
        # gate the arm reads the bound unwrapped and no fold re-decides legality.
        if (name is JoinPredicate.DWITHIN) != distance.is_some():
            return Error(GEO_UNBOUNDED.raised(name.value))
        return Ok(VectorOp(predicate=(name, other, distance)))

    @staticmethod
    def Linear(kind: LinearKind, other: "GeoDataFrame | None" = None, param: float = 0.0) -> "VectorOp":
        return VectorOp(linear=(kind, other, param))

    @staticmethod
    def Geodesic(kind: GeodesicKind = GeodesicKind.AREA) -> "VectorOp":
        return VectorOp(geodesic=kind)


class Crossing(StrEnum):
    # `Crossing` names the transport class an arm's OWN value implies, never the caller's entrypoint pick. Every
    # policy this class carries DERIVES on the member, the way `RetryClass.policy`/`.circuit`/`.rate` derive one
    # tier down, so no row spells a CLIENT span with no retry envelope; retriability vocabulary itself stays the
    # runtime resilience owner's, because this enum ELECTS a class and never mints one.
    LOCAL = "local"
    REMOTE = "remote"

    @property
    def retry(self) -> Option[RetryClass]:
        # a class facing no network spends no retry budget, so the envelope is absent by ABSENCE rather than by a
        # boolean every arm re-reads before deciding which of two call shapes to write.
        return Some(RetryClass.HTTP) if self is Crossing.REMOTE else Nothing

    @property
    def span_kind(self) -> SpanKind:
        # span kind follows the store law: an outbound leg opens CLIENT, an in-process GDAL leg stays INTERNAL.
        return SpanKind.CLIENT if self is Crossing.REMOTE else SpanKind.INTERNAL


class NodataSlot(StrEnum):
    # member value IS the provider keyword spelling the nodata sentinel on that call — `DatasetReader.read` takes
    # `fill_value`, `WarpedVRT`/`mask.mask`/`merge.merge`/`rasterio.open` take `nodata`, and `warp.reproject` takes
    # `dst_nodata`. A member-to-keyword table beside this roster is the deleted form.
    FILL = "fill_value"
    NODATA = "nodata"
    DESTINATION = "dst_nodata"


class CogProfile(Struct, frozen=True):
    compress: Compression = "deflate"
    blocksize: int = 512
    overviews: Literal["auto", "ignore", "force_use_existing"] = "auto"
    overview_resampling: OverviewResampling = "nearest"
    num_threads: Literal["all_cpus"] | int = "all_cpus"
    predictor: Literal["none", "standard", "floating_point"] = "none"

    def creation(self, array: "np.ndarray", crs: str) -> dict[str, object]:
        # `CogProfile` owns its compression, tiling, and overview knobs ALONE — nodata is the claim's declared
        # fact and merges in from `RasterGeoClaim._fill` at the write arm, so no profile forges one or holds a
        # second copy of the one slot five other reads already share.
        return {
            "driver": "COG",
            "dtype": str(array.dtype),
            "crs": crs,
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
    mosaic: tuple[tuple[str, ...], Option[VsiScheme], MergeMethod, Resampling | None, Bounds | None, tuple[float, float] | None] = case()
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

    @property
    def peer(self) -> Option[str]:
        # WHICH ORIGIN this op dials, derived from the op's OWN value exactly as `_row` derives its crossing — the
        # `reliability/resilience#RESILIENCE` window keys the dependency INSTANCE, so a raw href would mint one arc
        # per scene and no arc would reach its trip. `mosaic` answers its FIRST source: every member of one mosaic
        # is read from one archive, so the set shares one arc rather than splitting it per tile. `memory_source`
        # answers its inner op, since only that arm puts bytes on a wire. Every local arm answers `Nothing`, which
        # is exactly what the non-retried crossing needs and what `_keyed` no-ops on.
        match self:
            case RasterOp(tag="remote_read", remote_read=(href, _scheme, _bounds, _bidx)):
                return Some(origin(href))
            case RasterOp(tag="mosaic", mosaic=(sources, *_rest)) if sources:
                return Some(origin(sources[0]))
            case RasterOp(tag="memory_source", memory_source=(_payload, inner)):
                return inner.peer
            case _:
                return Nothing

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
        scheme: Option[VsiScheme] = Nothing,
        method: MergeMethod = "first",
        resampling: Resampling | None = None,
        bounds: Bounds | None = None,
        res: tuple[float, float] | None = None,
    ) -> "RasterOp":
        # scheme rides as a COLUMN because this arm opens every source itself, so its crossing must be
        # recoverable from the op value: a `str.startswith` probe over the paths classifies neither an already
        # prefixed `/vsis3/` source nor a signed href, and that probe is the discriminant this page refuses.
        return RasterOp(mosaic=(sources, scheme, method, resampling, bounds, res))

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


class _OpRow(Struct, frozen=True):
    # ONE dispatch row per `RasterOp` tag — the crossing the arm makes, whether it opens its OWN dataset off the op
    # value, and whether a tripped deadline may drop the band slot. `apply` reads nothing else to route.
    crossing: Crossing
    opens: bool
    # a tripped deadline abandons the band slot only where the arm has no half-applied effect to strand; it stays a
    # per-arm column rather than a `Crossing` derivation so a future REMOTE write row can refuse it by name.
    abandon: bool


# raster dispatch rows as DATA: no network-bearing arm reaches an un-retried entry any more, and no local arm
# pays an HTTP budget, because class rides the op's tag rather than a caller's method pick. Ten caller-handle arms
# share one row BY CONSTRUCTION — each reads a dataset somebody else opened, so it crosses nothing, opens nothing,
# and has nothing to abandon — while only the four arms that open a dataset off the op value carry their own.
_RASTER_ROW: Final[Map[str, _OpRow]] = Map.of_seq([
    *(
        (handled, _OpRow(crossing=Crossing.LOCAL, opens=False, abandon=False))
        for handled in ("window", "stream", "sample", "vrt", "mask", "geometry_mask", "sieve", "vectorize", "rasterize", "reproject")
    ),
    # opens `vsi_scheme.path(href)` itself under the `EMPTY_DIR` env — a side-effect-free read, so a wedged
    # `/vsicurl/` socket gives its slot back on the enclosing deadline instead of running out unobserved.
    ("remote_read", _OpRow(crossing=Crossing.REMOTE, opens=True, abandon=True)),
    # opens every `sources` path itself; this is the un-schemed row and `_row` swaps the crossing when the op
    # carries a `VsiScheme`.
    ("mosaic", _OpRow(crossing=Crossing.LOCAL, opens=True, abandon=True)),
    # opens the `MemoryFile` itself and then delegates: `_row` keeps this handle answer and takes the crossing and
    # abandon posture from the INNER arm, which alone decides whether anything reaches a wire.
    ("memory_source", _OpRow(crossing=Crossing.LOCAL, opens=True, abandon=False)),
    # opens the destination for WRITE: a half-written COG is state, so the slot is held to the end.
    ("write_cog", _OpRow(crossing=Crossing.LOCAL, opens=True, abandon=False)),
])


def _row(op: RasterOp) -> _OpRow:
    # crossing DERIVES from the op's own value. `memory_source` inherits its inner arm's class and abandon
    # posture — payload is already in hand, so only the inner op puts bytes on a wire — while keeping its own
    # handle answer, and `mosaic` reads its own `VsiScheme` column. Every other arm's class is fixed by its tag,
    # so no arm threads a routing flag the value cannot reconstruct.
    row = _RASTER_ROW[op.tag]
    match op:
        case RasterOp(tag="memory_source", memory_source=(_payload, inner)):
            return msgspec.structs.replace(_row(inner), opens=row.opens)
        case RasterOp(tag="mosaic", mosaic=(_sources, scheme, *_rest)):
            return msgspec.structs.replace(row, crossing=scheme.map(lambda _vsi: Crossing.REMOTE).default_value(Crossing.LOCAL))
        case _:
            return row


class _Coverage(Struct, frozen=True):
    array: "np.ndarray"
    transform: tuple[float, ...]
    op_tag: str
    source: str


class CoverageResult(Struct, frozen=True):
    array: "np.ndarray"
    transform: tuple[float, ...]
    receipt: QueryReceipt


class GeoreferenceFact(Struct, frozen=True):
    # one decoded model-georeference wire fact — projected CRS, IFC map-conversion helmert, declared north; minted
    # at the geometry sibling's IFC band, decoded HERE exactly once, never re-derived from geometry. Every field is
    # REQUIRED: the producer answers an ungeoreferenced model with typed absence, so a defaulted identity abscissa,
    # ordinate, or scale here would decode a fact declaring NO map conversion identically to one declaring the
    # identity rotation, and `to_map` would publish site-local engineering coordinates as map coordinates.
    # `true_north` is a genuine wire null the producer reads off the context's own declaration — three states are
    # two here, and it enters no transform, the map conversion having already oriented the eastings axis.
    crs: str
    eastings: float
    northings: float
    orthogonal_height: float
    x_axis_abscissa: float
    x_axis_ordinate: float
    scale: float
    true_north: float | None

    @staticmethod
    def decoded(raw: bytes) -> "RuntimeRail[GeoreferenceFact]":
        return boundary(
            GEO_DECODE, lambda: _GEOREF_DECODER.decode(raw), catch=(msgspec.ValidationError, msgspec.DecodeError)
        ).bind(GeoreferenceFact._invertible)

    @staticmethod
    def _invertible(fact: "GeoreferenceFact") -> "RuntimeRail[GeoreferenceFact]":
        # with the identity defaults gone every coefficient is a STATED fact, so the two the similarity cannot
        # invert refuse at this one admission seam rather than being rescued downstream by a fallback that
        # fabricates exactly the identity the defaults used to forge. ONE refusal names every unusable coefficient
        # at once — the producer's own `unwirable` fold spelled at the receiving end — so a caller repairing the
        # direction does not discover the scale by decoding again.
        degenerate = (
            *(("direction:zero-length",) if float(np.hypot(fact.x_axis_abscissa, fact.x_axis_ordinate)) == 0.0 else ()),
            *(("scale:zero",) if fact.scale == 0.0 else ()),
        )
        return Error(GEO_DEGENERATE.raised(";".join(degenerate))) if degenerate else Ok(fact)

    def to_map(self, frame: "GeoDataFrame") -> "GeoDataFrame":
        # unnormalized abscissa/ordinate admit — the direction normalizes, so authoring-tool magnitude
        # noise never scales the map; similarity = scale * rotation + (eastings, northings) translation.
        # Admission already proved the direction non-degenerate, so the norm divides with no fabricated floor.
        norm = float(np.hypot(self.x_axis_abscissa, self.x_axis_ordinate))
        cos_t, sin_t = self.x_axis_abscissa / norm, self.x_axis_ordinate / norm
        coefficients = [self.scale * cos_t, -self.scale * sin_t, self.scale * sin_t, self.scale * cos_t, self.eastings, self.northings]
        return frame.set_geometry(frame.geometry.affine_transform(coefficients)).set_crs(self.crs, allow_override=True)


_GEOREF_DECODER: Final = msgspec.json.Decoder(GeoreferenceFact)


class VectorGeoClaim(Struct, frozen=True):
    crs: str
    units: str
    axis_order: str
    family: GeometryFamily
    precision: int

    async def apply(self, op: VectorOp, frame: "GeoDataFrame") -> "RuntimeRail[GeoDataFrame]":
        # overlay/join/dissolve walk whole frames — a blocking leg riding the banded thread hop, never the loop.
        with _TRACER.start_as_current_span(f"geo.vector.{op.tag}", attributes={"rasm.geo.crs": self.crs, "rasm.geo.op": op.tag}):
            return await async_boundary(
                GEO_VECTOR, lambda: on_thread(self._vector, op, frame), catch=(ShapelyError, CRSError, KeyError, ValueError)
            )

    def reproject(self, frame: "GeoDataFrame", source: Option[GeoreferenceFact] = Nothing) -> "GeoDataFrame":
        # a model-minted CRS source lifts site-local coordinates through the helmert similarity FIRST, then the
        # claim prelude below normalizes exactly as for any already-georeferenced operand. The carrier is the
        # producer's own — `IfcAnalysis.georeference` answers `Option[GeoreferenceFact]`, so the seam threads one
        # shape end to end and no `None` stands where an ungeoreferenced model is the real answer.
        frame = source.map(lambda fact: fact.to_map(frame)).default_value(frame)
        target = pyproj.CRS.from_user_input(self.crs)
        if frame.crs is not None and pyproj.CRS.from_user_input(frame.crs) == target:
            return frame
        transformer = pyproj.Transformer.from_crs(frame.crs, target, always_xy=self.axis_order == "xy")
        return frame.to_crs(target) if transformer.has_inverse else frame.set_crs(target, allow_override=True)

    def _vector(self, op: VectorOp, frame: "GeoDataFrame") -> "GeoDataFrame":
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
                # TOTAL below the factory gate: the option's presence IS the accessor election and its own value IS
                # the radius, so the two cannot disagree and no fallback stands where an unbounded request is a
                # refusal the caller already read.
                hits = distance.map(lambda bound: snapped.geometry.dwithin(target, bound)).default_with(
                    lambda: getattr(snapped.geometry, name.value)(target)
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
                    case LinearKind.FRECHET:
                        # GEOS spells the Fréchet ufunc vectorized, so the trajectory-similarity measure
                        # stays a shapely arm under the kernel split law — one column per target broadcast.
                        return snapped.assign(frechet=shapely.frechet_distance(lines, target))
                    case unreachable_kind:
                        assert_never(unreachable_kind)
            case VectorOp(tag="geodesic", geodesic=kind):
                # WGS84 ellipsoid true-earth values a planar CRS transform cannot give; the geodesic runs
                # over lon/lat geometry, so the claim CRS prelude has already landed 4326. The rust surface
                # alone vectorizes the ellipsoidal (Karney) metrics, so the whole column rides one geoarrow
                # export instead of a per-geometry pyproj.Geod scalar loop.
                ga.register_extension_types()
                column = pa.table(snapped.to_arrow(geometry_encoding="geoarrow")).column("geometry").combine_chunks()
                values = np.asarray(
                    gac.area(column, method="ellipsoidal")
                    if kind is GeodesicKind.AREA
                    else gac.geodesic_perimeter(column)
                    if kind is GeodesicKind.PERIMETER
                    else gac.length(column, method="ellipsoidal")
                )
                return snapped.assign(**{f"geodesic_{kind.value}": np.abs(values) if kind is GeodesicKind.AREA else values})
            case unreachable:
                assert_never(unreachable)


class RasterGeoClaim(Struct, frozen=True):
    crs: str
    band_count: int
    resampling: Resampling
    # DECLARED absence, never a fabricated number: an asset that declares no nodata HAS none, and any float
    # standing in for that — `0.0` above all — masks every genuine pixel equal to it across eight provider reads.
    nodata: Option[float]
    # source affine — the six coefficients, or ABSENT where the producing asset declared none; each coverage op
    # REPORTS the transform its own operation derives, so this claim slot is provenance, never a stale substitute
    # for the op result, and an empty tuple would spell "six coefficients, all missing" instead of "not declared".
    transform: Option[tuple[float, ...]] = Nothing

    async def apply(self, op: RasterOp, source: "DatasetReader | None" = None) -> "RuntimeRail[CoverageResult]":
        # THE raster entry over the whole union: the row the op's own value resolves elects the crossing, so a
        # network-bearing arm can no longer reach an un-retried leg and a local one can no longer be charged an HTTP
        # budget because a caller reached for the other method. reproject/mask/warp read full arrays, so every arm
        # is a blocking leg on the banded thread hop, never the loop.
        row, subject = _row(op), f"geo.raster.{op.tag}"
        if not row.opens and source is None:
            # handle corner refuses BEFORE the band hop and before any GDAL handle moves: folding the two entries
            # otherwise admits `apply(RasterOp.Window(...))` with nothing to read, which dies inside the worker.
            return Error(GEO_HANDLE.raised())
        with _TRACER.start_as_current_span(
            subject,
            kind=row.crossing.span_kind,
            attributes={
                "rasm.geo.crs": self.crs,
                "rasm.geo.op": op.tag,
                "rasm.geo.bands": self.band_count,
                "rasm.geo.resampling": self.resampling,
                "rasm.geo.crossing": row.crossing.value,
            },
        ):
            acquired = await (
                row.crossing.retry.map(
                    # `abandon` frees the band slot when an enclosing deadline trips: a wedged `/vsicurl/` read
                    # otherwise runs out unobserved still holding it.
                    # the op tag no longer rides a minted subject: the row owns the coordinate and `rasm.geo.op` on
                    # the span above carries which arm ran, so one leg can never report two ways.
                    # `at` names WHICH CALL, `on` WHICH PEER — the op's own derived origin, so two fences dialing one
                    # archive share its arc and one fence dialing two archives keeps them apart.
                    lambda cls: guarded(cls, on_thread, lambda: self._remote_read(op, source), abandon=row.abandon, at=RASTER_REMOTE, on=op.peer)
                ).default_with(
                    lambda: async_boundary(
                        RASTER_LOCAL, lambda: on_thread(self._raster, op, source, abandon=row.abandon), catch=(RasterioError, ValueError)
                    )
                )
            )
            return acquired.bind(self._result)

    def _fill(self, slot: NodataSlot) -> dict[str, float]:
        # ONE nodata authority for the whole plane: a declared value spells the provider's own keyword, and an
        # undeclared one spells NOTHING, so `read`/`WarpedVRT`/`mask`/`merge`/`reproject`/`open` each fall to the
        # `None` default their own signature documents instead of masking every pixel equal to a fabricated number.
        return self.nodata.map(lambda value: {slot.value: value}).default_value({})

    def _remote_read(self, op: RasterOp, source: "DatasetReader | None") -> "_Coverage":
        # a `/vsicurl/` GDAL transient is a `RasterioIOError` (rooted at `RasterioError`, not a stdlib
        # transient), so it is re-raised as `ConnectionError` for the `RetryClass.HTTP` `_retry_after`
        # set rather than the resilience owner growing a `rasterio` provider-introspection target.
        try:
            return self._raster(op, source)
        except RasterioIOError as cause:
            raise ConnectionError(str(cause)) from cause

    def _raster(self, op: RasterOp, source: "DatasetReader | None") -> "_Coverage":
        match op:
            case RasterOp(tag="window", window=(bounds, boundless)):
                window = windows.from_bounds(*bounds, transform=source.transform)
                array = source.read(window=window, boundless=boundless, **self._fill(NodataSlot.FILL))
                return self._cover(np.asarray(array), source.window_transform(window), op.tag, source)
            case RasterOp(tag="stream", stream=(bidx, tile_shape, resampling)):
                row_factor, col_factor = (source.height // tile_shape[0], source.width // tile_shape[1]) if tile_shape else (1, 1)
                shape, dtype = (source.height // row_factor, source.width // col_factor), source.dtypes[bidx - 1]
                # a declared nodata SEEDS the destination, so a sliver the decimated block grid never covers reads
                # as absent; with none declared the raster spells no absent pixel at all, and the buffer is then the
                # uninitialized one the `reproject` arm already allocates, every cell written by a block below.
                destination = self.nodata.map(lambda fill: np.full(shape, fill, dtype=dtype)).default_with(lambda: np.empty(shape, dtype=dtype))
                for _, block in source.block_windows(bidx):
                    row0, col0 = block.row_off // row_factor, block.col_off // col_factor
                    rows, cols = block.height // row_factor, block.width // col_factor
                    source.read(
                        bidx,
                        window=block,
                        out=destination[row0 : row0 + rows, col0 : col0 + cols],
                        resampling=RioResampling[resampling or self.resampling],
                        boundless=True,
                        **self._fill(NodataSlot.FILL),
                    )
                return self._cover(destination, tuple(source.transform * rasterio.Affine.scale(col_factor, row_factor))[:6], op.tag, source)
            case RasterOp(tag="sample", sample=(coordinates, indexes)):
                picked = np.asarray(list(source.sample(list(coordinates), indexes=list(indexes) if indexes else None)))
                return self._cover(picked, tuple(source.transform)[:6], op.tag, source)
            case RasterOp(tag="vrt", vrt=(target_crs, resampling, width, height)):
                with WarpedVRT(
                    source,
                    crs=target_crs,
                    resampling=RioResampling[resampling or self.resampling],
                    width=width,
                    height=height,
                    **self._fill(NodataSlot.NODATA),
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
                    array = remote.read(window=window, out_shape=out_shape, boundless=window is not None, **self._fill(NodataSlot.FILL))
                    base = remote.window_transform(window) if window is not None else remote.transform
                    transform = tuple(base * rasterio.Affine.scale(overview, overview))[:6] if overview > 1 else tuple(base)[:6]
                    return self._cover(np.asarray(array), transform, op.tag, remote)
            case RasterOp(tag="memory_source", memory_source=(payload, inner)):
                with ExitStack() as stack:
                    memfile = stack.enter_context(MemoryFile(payload))
                    opened = stack.enter_context(memfile.open())
                    return self._raster(inner, opened)
            case RasterOp(tag="write_cog", write_cog=(path, array, transform, crs, profile)):
                creation = profile.creation(array, crs) | self._fill(NodataSlot.NODATA) | {"transform": rasterio.Affine(*transform)}
                with ExitStack() as stack:
                    written = stack.enter_context(rasterio.open(path, mode="w", **creation))
                    written.write(array)
                    return self._cover(np.asarray(array), transform, op.tag, written)
            case RasterOp(tag="mosaic", mosaic=(sources, scheme, method, resampling, bounds, res)):
                with ExitStack() as stack:
                    match scheme:
                        case Option(tag="some", some=vsi):
                            # a schemed mosaic crosses the wire once per source, so it takes the same `EMPTY_DIR`
                            # env the `remote_read` row takes — GDAL otherwise probes sidecar files on every open.
                            stack.enter_context(rasterio.Env(GDAL_DISABLE_READDIR_ON_OPEN="EMPTY_DIR"))
                            paths = [vsi.path(href) for href in sources]
                        case _:
                            paths = list(sources)
                    opened = [stack.enter_context(rasterio.open(path)) for path in paths]
                    mosaic, out_transform = merge.merge(
                        opened,
                        bounds=bounds,
                        res=res,
                        method=method,
                        resampling=RioResampling[resampling or self.resampling],
                        **self._fill(NodataSlot.NODATA),
                    )
                    return self._cover(np.asarray(mosaic), tuple(out_transform)[:6], op.tag, opened[0])
            case RasterOp(tag="mask", mask=(shapes, crop, all_touched, invert)):
                out_image, out_transform = mask.mask(
                    source, list(shapes), crop=crop, all_touched=all_touched, invert=invert, filled=True, **self._fill(NodataSlot.NODATA)
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
                    **self._fill(NodataSlot.DESTINATION),
                )
                return self._cover(destination, tuple(dst_transform)[:6], op.tag, source)
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _cover(array: "np.ndarray", transform: tuple[float, ...], op_tag: str, source: "DatasetReader") -> "_Coverage":
        return _Coverage(array=array, transform=transform, op_tag=op_tag, source=source.name)

    def _result(self, cover: "_Coverage") -> "RuntimeRail[CoverageResult]":
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
        return boundary(GEO_INGRESS, emit, catch=(DataSourceError, OSError, ValueError))


def geoarrow_wire(frame: "GeoDataFrame") -> "RuntimeRail[tuple[pa.Table, Bounds]]":
    # exports zero-copy geoarrow extension arrays the `geoarrow-rust-compute` kernel reads natively;
    # `total_bounds` over the combined-chunk geometry column is the wire-evidence fold.
    def emit() -> "tuple[pa.Table, Bounds]":
        table = pa.table(frame.to_arrow(geometry_encoding="geoarrow"))
        return table, tuple(gac.total_bounds(table.column("geometry").combine_chunks()))

    with _TRACER.start_as_current_span("geo.wire.geoarrow", attributes={"rasm.geo.op": "geoarrow"}):
        return boundary(GEO_WIRE, emit, catch=(ValueError, KeyError))
```

## [03]-[COVERAGE]

- Owner: `CoverageCf` — the `rioxarray` CF bridge: `lift` writes the claim CRS and the op-derived affine onto a bare-ndarray coverage through the `.rio` accessor (the CF `grid_mapping` convention, never a hand-copied CRS attribute), and `write_cog` is the LABELLED write — a CF cube lands as a COG without dropping to the bare array, the `odc-stac` coverage cube from `spatial/catalog#ASSETS` round-tripping through the same accessor. `[02]-[GEO]`s `WriteCog` row stays the ndarray-plane egress; each writer owns its carrier, never a second COG writer on either plane.
- Growth: a new CF raster verb is one accessor row; a new COG creation knob threads the `to_raster(**profile)` kwargs; zero new surface.

```python signature
# composes the [02]-[GEO] prelude: `rasterio`/`xarray as xr` lazy module-top, `ModuleType`, `Path`, `ContentIdentity`, `boundary`.


def _registered() -> "ModuleType":
    # THE ONE registration seam of the CF plane, and the page's only surviving function-local import: importing
    # rioxarray IS the `.rio` accessor registration, a module-body side effect no attribute access stands for. A
    # module-scope binding is refused in BOTH forms — an eager one runs the whole CF band on every import of this
    # page, and a lazy one never fires for the `lift` leg, which dereferences no rioxarray member at all — so the
    # effect fires at the call seam every accessor leg crosses first, never at an arbitrary first use.
    import rioxarray  # ruff:ignore[import-outside-top-level]

    return rioxarray


class CoverageCf(Struct, frozen=True):
    crs: str

    def open(self, path: str, *, masked: bool = True, chunks: dict[str, int] | None = None) -> "RuntimeRail[object]":
        def emit() -> object:
            return _registered().open_rasterio(path, masked=masked, chunks=chunks)

        with _TRACER.start_as_current_span("geo.coverage.open", attributes={"rasm.geo.op": "coverage.open"}):
            return boundary(COVERAGE_OPEN, emit, catch=(OSError, ValueError))

    def lift(self, result: CoverageResult, dims: tuple[str, ...] = ("band", "y", "x")) -> "RuntimeRail[object]":
        def emit() -> object:
            _registered()
            cube = xr.DataArray(result.array, dims=dims[-result.array.ndim :])
            cube = cube.rio.set_spatial_dims(x_dim="x", y_dim="y")
            return cube.rio.write_crs(self.crs).rio.write_transform(rasterio.Affine(*result.transform))

        with _TRACER.start_as_current_span("geo.coverage.lift", attributes={"rasm.geo.op": "coverage.lift", "rasm.geo.crs": self.crs}):
            return boundary(COVERAGE_LIFT, emit, catch=(ValueError, KeyError))

    def write_cog(self, cube: object, path: str) -> "RuntimeRail[ContentKey]":
        def emit() -> bytes:
            # a cube minted off-page — the `odc-stac` coverage from `spatial/catalog#ASSETS` — reaches the
            # accessor only through this same seam, so the write never assumes another owner registered it.
            _registered()
            cube.rio.to_raster(path, driver="COG")
            return Path(path).read_bytes()

        with _TRACER.start_as_current_span("geo.coverage.write_cog", attributes={"rasm.geo.op": "coverage.write_cog"}):
            return boundary(COVERAGE_WRITE, emit, catch=(OSError, ValueError)).bind(lambda payload: ContentIdentity.of("cog", payload))
```

## [04]-[NATIVE]

- Owner: `NativeIngress` — the GDAL-free ingress row over `geoarrow-rust-io`: rust-spelled formats parse straight into GeoArrow extension memory lifted zero-copy into pyarrow, `NativeFormat`'s member value IS the reader name resolved at the call seam, and `query_postgis` is the live-PostGIS spatial-SQL row on the same band. `_NATIVE_ROW` is the declared capability set, one row per format whose every column is read straight off that reader's own `geoarrow-rust-io` catalog call shape, and the row PROJECTS its own call arguments so no arm re-derives what its reader spells.
- Law: `read_native` output is the typed pyarrow table — geometry rides a registered `geoarrow.*` extension column decoded by name, never a WKB `binary` column re-parsed downstream; `register_extension_types` is the idempotent process latch every native entry crosses first. Spatial pushdown lands in the scan: `bbox` pushes into the FlatGeobuf packed R-tree on any source and into the GeoParquet row-group scan over an `ObjectStore` handle. Unservable corners are unrepresentable rather than refused mid-fold: `NativeIngress.of` demands the request through `_NATIVE_ROW` BEFORE the latch fires and before any file opens, naming every capability the request outran at once, so `read_native` holds no refusal arm and a `bbox` never degrades to a silent full scan.
- Boundary: the claims plane crosses to frames only where a `VectorOp` needs GeoSeries semantics — `geoarrow.pyarrow.to_geopandas` is that sole lowering; a format outside `NativeFormat` stays a `VectorIngress` pyogrio row, the OGR long-tail half of the split predicate.
- Growth: a new rust-spelled format is one `NativeFormat` member and one `_NATIVE_ROW` row citing its catalog call shape; a new pushdown mechanism is one `BboxPush` member with one `arguments` arm; a new remote source is the `store` handle row threaded from the runtime store lane; zero new surface.

```python signature
# composes the [02]-[GEO] prelude: `pyarrow as pa`, `geoarrow.pyarrow as ga`, `geoarrow.rust.io as gio` lazy module-top,
# and the runtime `origin` fold beside the `Some`/`Nothing` carriers this section's peer key rides.


class NativeFormat(StrEnum):
    # member value IS the `geoarrow.rust.io` reader name, resolved at the call seam.
    FLATGEOBUF = "read_flatgeobuf"
    GEOPARQUET = "read_parquet"
    GEOJSON = "read_geojson"
    GEOJSON_LINES = "read_geojson_lines"
    CSV = "read_csv"


class BboxPush(StrEnum):
    # HOW a format admits a spatial predicate, read off its own reader signature rather than asserted by a caller.
    # READER: the reader itself takes `bbox=` — `read_flatgeobuf(file, *, fs=None, batch_size, bbox=None)`.
    # HANDLE: only the `ParquetFile(path, fs).read(bbox=)` scan handle pushes, and its `fs` is positional-required,
    # so a bbox on that format demands the store with it. NONE: the reader spells no `bbox` slot at all.
    READER = "reader"
    HANDLE = "handle"
    NONE = "none"


class _NativeReader(Struct, frozen=True):
    # one capability row per format, every column read straight off that reader's `geoarrow-rust-io` catalog shape.
    bbox: BboxPush
    takes_store: bool
    needs_geometry_column: bool

    def arguments(self, spec: "NativeIngress") -> tuple[tuple[object, ...], dict[str, object]]:
        # row PROJECTS its own call shape: a positional geometry column where `read_csv` demands one, `fs` where
        # that reader spells it, `bbox` only where the reader itself pushes it. Knobs this row does not carry never
        # reach the call, so no reader is handed a keyword its signature has no slot for.
        return (
            (spec.source, spec.geometry_column) if self.needs_geometry_column else (spec.source,),
            {"batch_size": spec.batch_size}
            | ({"fs": spec.store} if self.takes_store else {})
            | ({"bbox": spec.bbox} if self.bbox is BboxPush.READER else {}),
        )


# GDAL-free reader rows: the declared capability set of the native band, each row citing its own catalog call shape.
_NATIVE_ROW: Final[Map[NativeFormat, _NativeReader]] = Map.of_seq([
    # `read_flatgeobuf(file, *, fs=None, batch_size=65536, bbox=None)` — the packed R-tree pushes on any source.
    (NativeFormat.FLATGEOBUF, _NativeReader(bbox=BboxPush.READER, takes_store=True, needs_geometry_column=False)),
    # `read_parquet(path, *, fs=None, batch_size=65536)` spells no bbox; `ParquetFile(path, fs)` plus
    # `read(*, batch_size, bbox, ...)` is the only row-group pushdown, so a bbox here rides the handle arm.
    (NativeFormat.GEOPARQUET, _NativeReader(bbox=BboxPush.HANDLE, takes_store=True, needs_geometry_column=False)),
    # `read_geojson(file, *, batch_size=65536)` and `read_geojson_lines(file, *, batch_size=65536)` — neither.
    (NativeFormat.GEOJSON, _NativeReader(bbox=BboxPush.NONE, takes_store=False, needs_geometry_column=False)),
    (NativeFormat.GEOJSON_LINES, _NativeReader(bbox=BboxPush.NONE, takes_store=False, needs_geometry_column=False)),
    # `read_csv(file, geometry_column_name, *, batch_size=65536)` — the geometry column is positional-required.
    (NativeFormat.CSV, _NativeReader(bbox=BboxPush.NONE, takes_store=False, needs_geometry_column=True)),
])


class NativeIngress(Struct, frozen=True):
    # bound through `of` alone: every field below is proved servable against `_NATIVE_ROW` before the value exists,
    # so `read_native` reads a spec whose corners are already legal instead of re-testing them per arm.
    format: NativeFormat
    source: str
    batch_size: int = 65536
    bbox: Bounds | None = None
    geometry_column: str | None = None
    # `geoarrow.rust.io.ObjectStore` handle for remote sources; construction takes the branch store
    # envelope as one pre-constructed config VALUE per the provider-store ruling, never a re-asserted mirror.
    store: object | None = None

    @staticmethod
    def of(
        format: NativeFormat,
        source: str,
        *,
        batch_size: int = 65536,
        bbox: Bounds | None = None,
        geometry_column: str | None = None,
        store: object | None = None,
    ) -> "RuntimeRail[NativeIngress]":
        # ONE gate over the declared capability set, run before the extension-type latch and before any file opens,
        # so an unservable corner never reaches the reader. Refusal NAMES every capability row the request outran at
        # once — a caller repairing the bbox does not discover the missing geometry column by calling again — and
        # `read_native` therefore carries no corner ladder at its own tail.
        row = _NATIVE_ROW[format]
        unserved = (
            *((f"bbox:{row.bbox.value}",) if bbox is not None and row.bbox is BboxPush.NONE else ()),
            *((f"bbox:{row.bbox.value}:store-absent",) if bbox is not None and row.bbox is BboxPush.HANDLE and store is None else ()),
            *(("store:unspelled",) if store is not None and not row.takes_store else ()),
            *(("geometry_column:required",) if row.needs_geometry_column and geometry_column is None else ()),
            *(("geometry_column:unspelled",) if geometry_column is not None and not row.needs_geometry_column else ()),
        )
        return (
            Error(NATIVE_UNSERVED.raised(format.name.lower(), ";".join(unserved)))
            if unserved
            else Ok(NativeIngress(format=format, source=source, batch_size=batch_size, bbox=bbox, geometry_column=geometry_column, store=store))
        )


def read_native(spec: NativeIngress) -> "RuntimeRail[pa.Table]":
    def emit() -> "pa.Table":
        ga.register_extension_types()
        row = _NATIVE_ROW[spec.format]
        match row.bbox, spec.bbox:
            case BboxPush.HANDLE, tuple() as bbox:
                # only pushdown this format spells rides the scan handle, whose `fs` the gate already demanded.
                table = gio.ParquetFile(spec.source, spec.store).read(bbox=bbox, batch_size=spec.batch_size)
            case _, _:
                positional, keywords = row.arguments(spec)
                table = getattr(gio, spec.format.value)(*positional, **keywords)
        return pa.table(table)

    with _TRACER.start_as_current_span(f"geo.native.{spec.format.name.lower()}", attributes={"rasm.geo.format": spec.format.name.lower()}):
        return boundary(NATIVE_READ, emit, catch=(OSError, ValueError))


async def query_postgis(connection_url: str, sql: str) -> "RuntimeRail[pa.Table]":
    # live PostGIS spatial SQL straight to GeoArrow memory — an outbound network leg: CLIENT span,
    # HTTP retry class on the banded thread hop, abandon frees the slot when an enclosing deadline trips.
    def emit() -> "pa.Table":
        ga.register_extension_types()
        return pa.table(gio.read_postgis(connection_url, sql))

    with _TRACER.start_as_current_span("geo.native.postgis", kind=SpanKind.CLIENT, attributes={"rasm.geo.format": "postgis"}):
        # the DSN crosses the runtime `origin` fold before it names a peer: that fold builds `scheme://host:port`
        # off `hostname`/`port` and never the raw netloc, so the `user:password@` userinfo a connection string
        # carries reaches neither the window key nor the `rasm.peer` span attribute the envelope stamps from it.
        return await guarded(RetryClass.HTTP, on_thread, emit, abandon=True, at=POSTGIS_QUERY, on=Some(origin(connection_url)))
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

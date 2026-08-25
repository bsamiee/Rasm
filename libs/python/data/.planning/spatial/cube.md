# [PY_DATA_CUBE]

Vector-data-cube owner bridging the gridded and spatial planes: `ZoneCube` carries an xarray cube whose zone dimension is INDEXED BY GEOMETRY through the `xvec` `GeometryIndex`, so per-zone, per-room, and per-sensor-location simulation results — energy series by thermal zone, daylight grids by room — select by spatial predicate and join the vector claims plane with no hand-rolled zone-id join table. The page composes the `gridded/field#FIELD` CF owner for its cubes and the `spatial/geospatial#GEO` claim for its CRS law, minting neither: a cube leaf is an `xr.Dataset`, the zone coordinate is shapely geometry, and every predicate operand crosses the claim's own `reproject` prelude before it touches the index.

Egress is the shared field family: a cube's geometry coordinate WKB-encodes through the accessor's own codec before the Zarr store lands, so the persisted cube round-trips through any CF reader and returns its `ContentKey`. The long-form frame lowering is the claims-plane join: `to_geodataframe` yields the zone-keyed `GeoDataFrame` a `VectorGeoClaim` operates on directly, so a cube variable joins vector claims as an in-frame column, never a re-keyed copy.

## [01]-[INDEX]

- [02]-[CUBE]: the `ZoneCube` owner — geometry-indexed lift, the `CubeOp` predicate/extract/frame family, the claim-composed CRS prelude, content-keyed egress.

## [02]-[CUBE]

- Owner: `ZoneCube` — one frozen owner carrying the geometry-indexed cube beside the composed `VectorGeoClaim`, so the CRS a predicate must land in never decouples from the indexed data; `CubeOp` the operation family — geometry-predicate `query`, point `extract`, and the long-form `frame` lowering — folded under one total `match`.
- Law: the claim owns the CRS law — `of` lifts the zone coordinate under the CLAIM's CRS, and every predicate or point operand crosses `claim.reproject` on a one-row frame before touching the `GeometryIndex`, so a mis-referenced operand lands in the cube's frame by the same prelude every vector operand crosses; a bare `set_crs`-style override on the index is the re-derived CRS law this composition deletes.
- Law: `frame` is the ONE bridge onto the claims plane — the long-form `GeoDataFrame` keyed by zone geometry — so a cube variable reaches vector claims as a column join at the claims plane, and no zone-id correspondence table exists anywhere: the geometry IS the key on both ends.
- Entry: `ZoneCube.of(cube, coord, claim)` lifts the named coordinate through `set_geom_indexes` under the claim CRS; `apply(op)` answers the operation family; `write(target)` WKB-encodes the geometry coordinate through `encode_wkb`, lands one Zarr store, and returns the `ContentKey` minted from its `zarr.json` root-metadata bytes.
- Packages: `xvec` (`set_geom_indexes`, `query(coord, geometry, predicate=, distance=)`, `extract_points`, `to_geodataframe(geometry=, long=True)`, `encode_wkb` — the `.xvec` accessor surface), `shapely` (the geometry operands), `xarray` (the cube substrate), `msgspec` (the frozen owner), runtime (`RuntimeRail`/`boundary`/`Catch`/`FaultRow`/`ContentIdentity`/`scoped`), `spatial/geospatial#GEO` (`VectorGeoClaim`, the composed CRS law).
- Growth: a new spatial verb is one `CubeOp` case plus one arm over the accessor member that spells it (`zonal_stats` lands this way when a raster-backed consumer names it); a new predicate is the accessor's own `predicate=` vocabulary, no arm edit; zero new surface.
- Boundary: no raster coverage (the `rioxarray` bridge is `spatial/geospatial#COVERAGE`'s), no CF engine axis (cube leaves arrive as datasets the field owner opened), no second labelled-array store, no DGG cell algebra (`spatial/grid#GRID` owns cells); the accessor's plotting surface is out of scope — artifacts owns rendering.

```python
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

from expression import Option, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from opentelemetry import trace
from pyproj.exceptions import ProjError
from shapely.errors import ShapelyError
from zarr.errors import BaseZarrError

lazy import geopandas as gpd

from rasm.data.spatial.geospatial import VectorGeoClaim
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    import xarray as xr

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.cube")

type Predicate = Literal["intersects", "within", "contains", "overlaps", "crosses", "touches", "covers", "covered_by", "dwithin"]

_XVEC_RAISES: Final[Catch] = (ShapelyError, ProjError, KeyError, TypeError, ValueError, ImportError)

_ZARR_RAISES: Final[Catch] = (BaseZarrError, ShapelyError, KeyError, TypeError, ValueError, OSError)

CUBE_LIFT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CUBE, point="lift", arm="boundary", defect="geometry-index", retriability=TERMINAL
)
CUBE_APPLY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CUBE, point="apply", arm="boundary", defect="cube-op", retriability=TERMINAL
)
CUBE_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CUBE, point="write", arm="boundary", defect="cube-write", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([CUBE_LIFT, CUBE_APPLY, CUBE_WRITE]))


@tagged_union(frozen=True)
class CubeOp:
    tag: Literal["query", "extract", "frame"] = tag()
    query: tuple[Any, Predicate, Option[float]] = case()
    extract: tuple[tuple[Any, ...], str, str] = case()
    frame: None = case()


class ZoneCube(Struct, frozen=True):
    cube: Any
    coord: str
    claim: VectorGeoClaim

    @classmethod
    def of(cls, cube: "xr.Dataset", coord: str, claim: VectorGeoClaim) -> "RuntimeRail[ZoneCube]":
        return boundary(
            CUBE_LIFT, lambda: cls(cube=cube.xvec.set_geom_indexes(coord, crs=claim.crs), coord=coord, claim=claim), catch=_XVEC_RAISES
        )

    def apply(self, op: CubeOp) -> "RuntimeRail[Any]":
        with _TRACER.start_as_current_span(f"cube.{op.tag}", attributes={"rasm.geo.crs": self.claim.crs, "rasm.geo.op": op.tag}):
            return boundary(CUBE_APPLY, lambda: self._apply(op), catch=_XVEC_RAISES)

    def _apply(self, op: CubeOp) -> Any:
        match op:
            case CubeOp(tag="query", query=(geometry, predicate, distance)):
                return self.cube.xvec.query(
                    self.coord, self._aligned(geometry), predicate=predicate, distance=distance.default_value(None)
                )
            case CubeOp(tag="extract", extract=(points, x_coord, y_coord)):
                aligned = tuple(self._aligned(point) for point in points)
                return self.cube.xvec.extract_points(list(aligned), x_coord, y_coord)
            case CubeOp(tag="frame"):
                return self.cube.xvec.to_geodataframe(geometry=self.coord, long=True)
            case unreachable:
                assert_never(unreachable)

    def _aligned(self, geometry: Any) -> Any:
        return self.claim.reproject(gpd.GeoDataFrame(geometry=gpd.GeoSeries([geometry]))).geometry.iloc[0]

    def write(self, target: ResourceRef) -> "RuntimeRail[ContentKey]":
        def emit() -> "RuntimeRail[ContentKey]":
            encoded = self.cube.xvec.encode_wkb()
            encoded.to_zarr(str(target.path))
            source = (target.path / "zarr.json").read_bytes()
            return ContentIdentity.of("field", source)

        with _TRACER.start_as_current_span("cube.write", attributes={"rasm.geo.op": "cube.write"}):
            return boundary(CUBE_WRITE, emit, catch=_ZARR_RAISES).bind(lambda rail: rail)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

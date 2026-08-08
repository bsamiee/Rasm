# [PY_DATA_CUBE]

Vector-data-cube owner bridging the gridded and spatial planes: `ZoneCube` carries an xarray cube whose zone dimension is INDEXED BY GEOMETRY through the `xvec` `GeometryIndex`, so per-zone, per-room, and per-sensor-location simulation results — energy series by thermal zone, daylight grids by room — select by spatial predicate and join the vector claims plane with no hand-rolled zone-id join table. The page composes the `gridded/field#FIELD` CF owner for its cubes and the `spatial/geospatial#GEO` claim for its CRS law, minting neither: a cube leaf is an `xr.Dataset`, the zone coordinate is shapely geometry, and every predicate operand crosses the claim's own `reproject` prelude before it touches the index.

Egress is the shared field family: a cube's geometry coordinate WKB-encodes through the accessor's own codec before the Zarr store lands, so the persisted cube round-trips through any CF reader, and the receipt rides `FieldReceipt` under the `cube` tag — one receipt family for every labelled-plane egress. The long-form frame lowering is the claims-plane join: `to_geodataframe` yields the zone-keyed `GeoDataFrame` a `VectorGeoClaim` operates on directly, so a cube variable joins vector claims as an in-frame column, never a re-keyed copy.

## [01]-[INDEX]

- [02]-[CUBE]: the `ZoneCube` owner — geometry-indexed lift, the `CubeOp` predicate/extract/frame family, the claim-composed CRS prelude, content-keyed `FieldReceipt` egress.

## [02]-[CUBE]

- Owner: `ZoneCube` — one frozen owner carrying the geometry-indexed cube beside the composed `VectorGeoClaim`, so the CRS a predicate must land in never decouples from the indexed data; `CubeOp` the operation family — geometry-predicate `query`, point `extract`, and the long-form `frame` lowering — folded under one total `match`.
- Law: the claim owns the CRS law — `of` lifts the zone coordinate under the CLAIM's CRS, and every predicate or point operand crosses `claim.reproject` on a one-row frame before touching the `GeometryIndex`, so a mis-referenced operand lands in the cube's frame by the same prelude every vector operand crosses; a bare `set_crs`-style override on the index is the re-derived CRS law this composition deletes.
- Law: `frame` is the ONE bridge onto the claims plane — the long-form `GeoDataFrame` keyed by zone geometry — so a cube variable reaches vector claims as a column join at the claims plane, and no zone-id correspondence table exists anywhere: the geometry IS the key on both ends.
- Entry: `ZoneCube.of(cube, coord, claim)` lifts the named coordinate through `set_geom_indexes` under the claim CRS; `apply(op)` answers the operation family; `write(target)` WKB-encodes the geometry coordinate through `encode_wkb`, lands one Zarr store, and mints the shared `FieldReceipt` keyed off the store's `zarr.json` root-metadata bytes.
- Receipt: one `FieldReceipt` per egress under `engine="cube"`, riding the family's `domain="field"` projection — zero new receipt surface.
- Packages: `xvec` (`set_geom_indexes`, `query(coord, geometry, predicate=, distance=)`, `extract_points`, `to_geodataframe(geometry=, long=True)`, `encode_wkb` — the `.xvec` accessor surface), `shapely` (the geometry operands), `xarray` (the cube substrate), `msgspec` (the frozen owner), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`scoped`), `spatial/geospatial#GEO` (`VectorGeoClaim`, the composed CRS law), `gridded/field#EGRESS` (`FieldReceipt`).
- Growth: a new spatial verb is one `CubeOp` case plus one arm over the accessor member that spells it (`zonal_stats` lands this way when a raster-backed consumer names it); a new predicate is the accessor's own `predicate=` vocabulary, no arm edit; a new receipt fact is one entry on the family's fact dict; zero new surface.
- Boundary: no raster coverage (the `rioxarray` bridge is `spatial/geospatial#COVERAGE`'s), no CF engine axis (cube leaves arrive as datasets the field owner opened), no second labelled-array store, no DGG cell algebra (`spatial/grid#GRID` owns cells); the accessor's plotting surface is out of scope — artifacts owns rendering.

```python signature
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.data.gridded.field import FieldReceipt
from rasm.data.spatial.geospatial import VectorGeoClaim
from rasm.runtime.faults import RuntimeRail, boundary, scoped
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    import xarray as xr

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.cube")

type Predicate = Literal["intersects", "within", "contains", "overlaps", "crosses", "touches", "covers", "covered_by", "dwithin"]


@tagged_union(frozen=True)
class CubeOp:
    tag: Literal["query", "extract", "frame"] = tag()
    # (predicate geometry, predicate name, dwithin distance | None) — the operand crosses the claim prelude first.
    query: tuple[Any, Predicate, float | None] = case()
    # (points, x coord name, y coord name) — nearest-cell lift for sensor locations on gridded variables.
    extract: tuple[tuple[Any, ...], str, str] = case()
    frame: None = case()


class ZoneCube(Struct, frozen=True):
    cube: Any
    coord: str
    claim: VectorGeoClaim

    @classmethod
    def of(cls, cube: "xr.Dataset", coord: str, claim: VectorGeoClaim) -> "RuntimeRail[ZoneCube]":
        # lift indexes the named coordinate under the CLAIM's CRS — the one CRS authority every operand
        # then meets through the same prelude; geometry arrives as the coordinate's own shapely values.
        return boundary(
            "cube.of", lambda: cls(cube=cube.xvec.set_geom_indexes(coord, crs=claim.crs), coord=coord, claim=claim)
        )

    def apply(self, op: CubeOp) -> "RuntimeRail[Any]":
        with _TRACER.start_as_current_span(f"cube.{op.tag}", attributes={"rasm.geo.crs": self.claim.crs, "rasm.geo.op": op.tag}):
            return boundary(f"cube.{op.tag}", lambda: self._apply(op))

    def _apply(self, op: CubeOp) -> Any:
        match op:
            case CubeOp(tag="query", query=(geometry, predicate, distance)):
                return self.cube.xvec.query(self.coord, self._aligned(geometry), predicate=predicate, distance=distance)
            case CubeOp(tag="extract", extract=(points, x_coord, y_coord)):
                aligned = tuple(self._aligned(point) for point in points)
                return self.cube.xvec.extract_points(list(aligned), x_coord, y_coord)
            case CubeOp(tag="frame"):
                # ONE claims-plane bridge: long-form zone-keyed GeoDataFrame a VectorGeoClaim operates on.
                return self.cube.xvec.to_geodataframe(geometry=self.coord, long=True)
            case unreachable:
                assert_never(unreachable)

    def _aligned(self, geometry: Any) -> Any:
        # every operand crosses the claim's OWN prelude on a one-row frame — the same reproject law every
        # vector operand crosses — so a mis-referenced predicate lands in the cube's frame, never raw.
        import geopandas as gpd  # ruff:ignore[import-outside-top-level]

        return self.claim.reproject(gpd.GeoDataFrame(geometry=gpd.GeoSeries([geometry]))).geometry.iloc[0]

    def write(self, target: ResourceRef) -> "RuntimeRail[FieldReceipt]":
        # WKB-encode the geometry coordinate through the accessor's own codec, then one Zarr store; the key
        # folds the v3 `zarr.json` root-metadata bytes, the field Zarr key law, under the `cube` receipt tag.
        def emit() -> "RuntimeRail[FieldReceipt]":
            encoded = self.cube.xvec.encode_wkb()
            encoded.to_zarr(str(target.path))
            source = (target.path / "zarr.json").read_bytes()
            return ContentIdentity.of("field", source).map(
                lambda key: FieldReceipt(
                    engine="cube",
                    dims=tuple(self.cube.sizes),
                    variables=len(self.cube.data_vars),
                    bytes_stored=int(self.cube.nbytes),
                    content_key=key,
                )
            )

        with _TRACER.start_as_current_span("cube.write", attributes={"rasm.geo.op": "cube.write"}):
            return boundary("cube.write", emit).bind(lambda rail: rail)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

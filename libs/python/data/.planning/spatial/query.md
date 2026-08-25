# [PY_DATA_SPATIAL_QUERY]

The columnar spatial-query engine: one `SpatialQuery` tagged-union axis over DuckDB's `spatial`-extension `GEOMETRY` type, split out of the geospatial claims plane so the codemap charters one engine owner. The one capability this engine adds over the generic `tabular/query#QUERY` relational dispatch is the `ST_GeomFromWKB` geometry-view admission — it owns exactly the in-DB plane, never a second generic SQL surface and never a parallel index owner.

The engine composes the `tabular/columnar#SCAN` `DuckDbSession` rail downward — `DuckDbExtension.SPATIAL` the unconditional prelude row, `DuckDbExtension.H3` the community-repository supplement the `H3Bin` case adds — and emits plain `pyarrow.Table`. The two-H3-substrate boundary is law: in-DB binning lives here, in-frame vectorized cell algebra on `spatial/grid#GRID`, and `grid`'s `engine_bin` composes this engine downward for an already-columnar input — one binning law, two substrates, each owned once.

## [01]-[INDEX]

- [02]-[SPATIAL]: the DuckDB join/transform/H3 spatial engine — one total `QueryPlan` projection per `SpatialQuery` case over the shared session rail.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one tagged-union axis whose every case projects through the one `QueryPlan` fold carrying SQL, bound parameters, extension supplement, and predicate count in a single traversal, never three parallel folds over the same family. The join rides the optimizer's bbox-cached `SPATIAL_JOIN` automatic prefilter, never an STRtree/`sjoin` Python loop or a hand-built R-tree.
- Cases: the distance corner is a BICONDITIONAL the `Join` factory proves before a query exists — `ST_DWithin` is the only predicate whose SQL carries a placeholder, so a distanceless `ST_DWithin` and a distance handed to a geometry-only predicate are both refused on the typed `config` rail at construction, the second being the corner that binds a parameter the statement has no slot for and dies inside duckdb. `PointInPolygon` fixes its own corner at the call and stays total.
- Entry: `plan()` is TOTAL — every corner it once refused is unrepresentable by the time a `SpatialQuery` exists, so the projection reads the union and never re-decides legality; `run` folds plan, span, and boundary in one pass with the duckdb raise surface named at the seam.
- Auto: geometry crosses as WKB (`GeoDataFrame.to_wkb`/`GeoExpr.st.to_wkb`) so the columnar input stays pyarrow-native at the wire and the engine decodes once; the join's distance rides `Option[float]`, whose own lowering IS the parameter tuple and whose presence IS the placeholder, so the SQL text and the bound values cannot drift apart; CRS normalization rides either `spatial/geospatial#GEO` `VectorGeoClaim.reproject` on the host frame or the `Transform` case's in-engine `ST_Transform` for an already-columnar input, so join operands share one CRS without a second transport.
- Packages: `duckdb`, `sqlglot`, `pyarrow`, `expression`, `opentelemetry-api` per the fence imports; `geopandas`/`polars-st` are the upstream WKB producers and `pyproj` the host-frame CRS engine — none crosses into this fence.
- Growth: a new geometry-only spatial predicate is one `SpatialPredicate` literal the `Join` gate admits free; a predicate carrying its own operand is one `SpatialQuery` case, never a second optional slot on the join payload; a new spatial intent is one `SpatialQuery` case projected by the one `QueryPlan` fold; a new loadable extension is one `DuckDbExtension` row on the shared `columnar` table, never a local enum; a new refusal law is one `FaultRow` row under `DataLeg.SPATIAL_QUERY`; the H3 hierarchy (`h3_cell_to_parent`, `h3_grid_disk`) composes on the existing `H3Bin` SQL; zero new surface.
- Boundary: the session rail owns connect-install-load, so this page carries no `duckdb.connect()`/`install_extension` site; no GIS host coupling, no lonboard/GeoArrow visualization (`artifacts` owns it), no durable store — the claims plane is `spatial/geospatial#GEO`, the in-frame DGG plane `spatial/grid#GRID`.

```python
from collections.abc import Mapping
from typing import Final, Literal, assert_never

import duckdb
import pyarrow as pa
from expression import Error, Nothing, Ok, Option, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from sqlglot import exp

from rasm.data.tabular.columnar import DuckDbExtension, DuckDbSession
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.query")

type SpatialPredicate = Literal["ST_Intersects", "ST_Contains", "ST_Within", "ST_DWithin"]

_QUERY_RAISES: Final[Catch] = (duckdb.Error,)

QUERY_DISTANCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SPATIAL_QUERY, point="join", arm="config", defect="distance-mismatch", retriability=TERMINAL, slots=("predicate",)
)
QUERY_RUN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SPATIAL_QUERY, point="run", arm="boundary", defect="engine-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([QUERY_DISTANCE, QUERY_RUN]))


def _ident(name: str) -> str:
    return exp.Identifier(this=name, quoted=True).sql(dialect="duckdb")


class QueryPlan(Struct, frozen=True):
    sql: str
    parameters: tuple[object, ...]
    extensions: tuple[DuckDbExtension, ...]
    predicate_count: int


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Literal["join", "transform", "h3_bin"] = tag()
    join: tuple[SpatialPredicate, str, str, Option[float]] = case()
    transform: tuple[str, str, str] = case()
    h3_bin: tuple[str, int] = case()

    @staticmethod
    def Join(predicate: SpatialPredicate, left: str, right: str, distance: Option[float] = Nothing) -> "RuntimeRail[SpatialQuery]":
        if (predicate == "ST_DWithin") != distance.is_some():
            return Error(QUERY_DISTANCE.raised(predicate))
        return Ok(SpatialQuery(join=(predicate, left, right, distance)))

    @staticmethod
    def PointInPolygon(points: str, polygons: str) -> "SpatialQuery":
        return SpatialQuery(join=("ST_Contains", polygons, points, Nothing))

    @staticmethod
    def Transform(geometry: str, source_crs: str, target_crs: str) -> "SpatialQuery":
        return SpatialQuery(transform=(geometry, source_crs, target_crs))

    @staticmethod
    def H3Bin(geometry: str, resolution: int = 9) -> "SpatialQuery":
        return SpatialQuery(h3_bin=(geometry, resolution))

    def plan(self) -> QueryPlan:
        match self:
            case SpatialQuery(tag="join", join=(predicate, left, right, distance)):
                on = f"{predicate}(l.geom, r.geom, ?)" if distance.is_some() else f"{predicate}(l.geom, r.geom)"
                return QueryPlan(
                    sql=f"SELECT l.*, r.* FROM {_ident(left)} l JOIN {_ident(right)} r ON {on}",
                    parameters=tuple(distance.to_list()),
                    extensions=(),
                    predicate_count=1,
                )
            case SpatialQuery(tag="transform", transform=(geometry, source_crs, target_crs)):
                return QueryPlan(
                    sql=f"SELECT * EXCLUDE geom, ST_Transform(geom, ?, ?) AS geom FROM {_ident(geometry)}",
                    parameters=(source_crs, target_crs),
                    extensions=(),
                    predicate_count=0,
                )
            case SpatialQuery(tag="h3_bin", h3_bin=(geometry, resolution)):
                return QueryPlan(
                    sql=f"SELECT *, h3_latlng_to_cell(ST_Y(geom), ST_X(geom), ?) AS h3 FROM {_ident(geometry)}",
                    parameters=(resolution,),
                    extensions=(DuckDbExtension.H3,),
                    predicate_count=0,
                )
            case unreachable:
                assert_never(unreachable)


class SpatialEngine(Struct, frozen=True):
    inputs: Map[str, pa.Table]

    @classmethod
    def of(cls, inputs: Mapping[str, pa.Table]) -> "SpatialEngine":
        return cls(inputs=Map.of_seq(inputs.items()))

    def run(self, query: SpatialQuery) -> "RuntimeRail[pa.Table]":
        plan = query.plan()
        with _TRACER.start_as_current_span(
            f"spatial.query.{query.tag}", attributes={"rasm.geo.op": query.tag, "rasm.geo.predicates": plan.predicate_count}
        ):
            return boundary(QUERY_RUN, lambda: self._dispatch(plan), catch=_QUERY_RAISES)

    def _dispatch(self, plan: QueryPlan) -> pa.Table:
        with DuckDbSession(extensions=(DuckDbExtension.SPATIAL, *plan.extensions)).connect() as con:
            for name, table in self.inputs.items():
                raw = f"{name}_raw"
                con.register(raw, table)
                con.execute(f"CREATE VIEW {_ident(name)} AS SELECT * EXCLUDE wkb, ST_GeomFromWKB(wkb) AS geom FROM {_ident(raw)}")
            return con.execute(plan.sql, list(plan.parameters)).to_arrow_table()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

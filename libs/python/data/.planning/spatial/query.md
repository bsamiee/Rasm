# [PY_DATA_SPATIAL_QUERY]

The columnar spatial-query engine: one `SpatialQuery` tagged-union axis over DuckDB's `spatial`-extension `GEOMETRY` type, split out of the geospatial claims plane so the codemap charters one engine owner. The one capability this engine adds over the generic `tabular/query#QUERY` relational dispatch is the `ST_GeomFromWKB` geometry-view admission — it owns exactly the in-DB plane, never a second generic SQL surface and never a parallel index owner.

The engine composes the `tabular/columnar#SCAN` `DuckDbSession` rail downward — `DuckDbExtension.SPATIAL` the unconditional prelude row, `DuckDbExtension.H3` the community-repository supplement the `H3Bin` case adds — and emits plain `pyarrow.Table` keyed through the shared `columnar` `QueryReceipt`. The two-H3-substrate boundary is law: in-DB binning lives here, in-frame vectorized cell algebra on `spatial/grid#GRID`, and `grid`'s `engine_bin` composes this engine downward for an already-columnar input — one binning law, two substrates, each owned once.

## [01]-[INDEX]

- [01]-[SPATIAL]: the DuckDB join/transform/H3 spatial engine — one `QueryPlan` projection per `SpatialQuery` case over the shared session rail.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one tagged-union axis whose every case projects through the one `QueryPlan` fold carrying SQL, bound parameters, extension supplement, and predicate count in a single traversal, never three parallel folds over the same family; the fold fails early on the typed `config` rail when an `ST_DWithin` join carries no distance. The join rides the optimizer's bbox-cached `SPATIAL_JOIN` automatic prefilter, never an STRtree/`sjoin` Python loop or a hand-built R-tree.
- Auto: geometry crosses as WKB (`GeoDataFrame.to_wkb`/`GeoExpr.st.to_wkb`) so the columnar input stays pyarrow-native at the wire and the engine decodes once; CRS normalization rides either `spatial/geospatial#GEO` `VectorGeoClaim.reproject` on the host frame or the `Transform` case's in-engine `ST_Transform` for an already-columnar input, so join operands share one CRS without a second transport.
- Receipt: the shared `columnar` `QueryReceipt.railed` keyed over the result Arrow bytes; no new receipt rail.
- Packages: `duckdb`, `sqlglot`, `pyarrow`, `expression`, `opentelemetry-api` per the fence imports; `geopandas`/`polars-st` are the upstream WKB producers and `pyproj` the host-frame CRS engine — none crosses into this fence.
- Growth: a new spatial predicate is one `SpatialPredicate` literal; a new spatial intent is one `SpatialQuery` case projected by the one `QueryPlan` fold; a new loadable extension is one `DuckDbExtension` row on the shared `columnar` table, never a local enum; the H3 hierarchy (`h3_cell_to_parent`, `h3_grid_disk`) composes on the existing `H3Bin` SQL; zero new surface.
- Boundary: the session rail owns connect-install-load, so this page carries no `duckdb.connect()`/`install_extension` site; no GIS host coupling, no lonboard/GeoArrow visualization (`artifacts` owns it), no durable store — the claims plane is `spatial/geospatial#GEO`, the in-frame DGG plane `spatial/grid#GRID`.

```python signature
from collections.abc import Mapping
from typing import Final, Literal, assert_never

import duckdb
import pyarrow as pa
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from sqlglot import exp

from rasm.data.tabular.columnar import DuckDbExtension, DuckDbSession, QueryReceipt
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary

_TRACER: Final = trace.get_tracer("rasm.data.spatial.query")

type SpatialPredicate = Literal["ST_Intersects", "ST_Contains", "ST_Within", "ST_DWithin"]


def _ident(name: str) -> str:
    # every interpolated table/view/relation name routes through sqlglot's dialect-correct
    # identifier quoting before it reaches the SQL string; the VALUES stay bound parameters.
    return exp.Identifier(this=name, quoted=True).sql(dialect="duckdb")


class QueryPlan(Struct, frozen=True):
    sql: str
    parameters: tuple[object, ...]
    extensions: tuple[DuckDbExtension, ...]
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

    def plan(self) -> "RuntimeRail[QueryPlan]":
        match self:
            case SpatialQuery(tag="join", join=(predicate, left, right, distance)):
                if predicate == "ST_DWithin" and distance is None:
                    return Error(BoundaryFault(config=("spatial.query.join", "ST_DWithin requires a distance")))
                on = f"{predicate}(l.geom, r.geom, ?)" if predicate == "ST_DWithin" else f"{predicate}(l.geom, r.geom)"
                return Ok(QueryPlan(
                    sql=f"SELECT l.*, r.* FROM {_ident(left)} l JOIN {_ident(right)} r ON {on}",
                    parameters=() if distance is None else (distance,),
                    extensions=(),
                    predicate_count=1,
                ))
            case SpatialQuery(tag="transform", transform=(geometry, source_crs, target_crs)):
                return Ok(QueryPlan(
                    sql=f"SELECT * EXCLUDE geom, ST_Transform(geom, ?, ?) AS geom FROM {_ident(geometry)}",
                    parameters=(source_crs, target_crs),
                    extensions=(),
                    predicate_count=0,
                ))
            case SpatialQuery(tag="h3_bin", h3_bin=(geometry, resolution)):
                return Ok(QueryPlan(
                    sql=f"SELECT *, h3_latlng_to_cell(ST_Y(geom), ST_X(geom), ?) AS h3 FROM {_ident(geometry)}",
                    parameters=(resolution,),
                    extensions=(DuckDbExtension.H3,),
                    predicate_count=0,
                ))
            case unreachable:
                assert_never(unreachable)


class SpatialResult(Struct, frozen=True):
    table: pa.Table
    receipt: QueryReceipt


class SpatialEngine(Struct, frozen=True):
    inputs: Map[str, pa.Table]

    @classmethod
    def of(cls, inputs: Mapping[str, pa.Table]) -> "SpatialEngine":
        return cls(inputs=Map.of_seq(inputs.items()))

    def run(self, query: SpatialQuery) -> "RuntimeRail[SpatialResult]":
        def executed(plan: QueryPlan) -> "RuntimeRail[SpatialResult]":
            with _TRACER.start_as_current_span(
                f"spatial.query.{query.tag}", attributes={"rasm.geo.op": query.tag, "rasm.geo.predicates": plan.predicate_count}
            ):
                return boundary(f"spatial.query.{query.tag}", lambda: self._dispatch(plan), catch=duckdb.Error).bind(
                    lambda table: QueryReceipt.railed("duckdb_spatial", query.tag, table, predicate_count=plan.predicate_count).map(
                        lambda receipt: SpatialResult(table=table, receipt=receipt)
                    )
                )

        return query.plan().bind(executed)

    def _dispatch(self, plan: QueryPlan) -> pa.Table:
        # SPATIAL is the unconditional prelude — every `ST_` function lives in the spatial extension; the plan supplies supplements.
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

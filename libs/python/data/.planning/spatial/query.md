# [PY_DATA_SPATIAL_QUERY]

The columnar spatial-query engine: one `SpatialQuery` tagged-union axis over DuckDB's `spatial`-extension `GEOMETRY` type, split out of the geospatial claims plane so the codemap charters one engine owner. `SpatialEngine` is the request-scoped run composing the `tabular/columnar#SCAN` `DuckDbSession` rail downward — the ONE thing this engine adds over the generic `tabular/query#QUERY` relational dispatch is the `ST_GeomFromWKB` geometry-view admission prelude — with the shared `DuckDbExtension.SPATIAL` row the unconditional prelude every run loads before any `ST_` function resolves (in DuckDB every `ST_` function lives in the `spatial` extension) and `DuckDbExtension.H3` the community-repository supplement the `H3Bin` case adds. Every case projects through one `QueryPlan` fold carrying SQL, bound parameters, the extension requirement, and the predicate count in a single traversal; the bbox-cached `SPATIAL_JOIN` operator is the optimizer's automatic join prefilter. The two-H3-substrate boundary is law: in-DB binning lives HERE (`h3_latlng_to_cell` SQL over the loaded `h3` extension), in-frame vectorized cell algebra lives on `spatial/grid#GRID` (h3ronpy Arrow ops), and `grid`'s `engine_bin` composes this engine downward for an already-columnar input — one binning law, two substrates, each owned once. The spatial axis emits plain `pyarrow.Table` keyed by one `ContentIdentity` through the shared `columnar` `QueryReceipt`, never a parallel index owner.

## [01]-[INDEX]

- [01]-[SPATIAL]: the DuckDB join/transform/H3 columnar query engine over the `spatial`-extension `GEOMETRY` type — the shared `DuckDbSession` rail with the `SPATIAL` prelude row and the `H3` supplement row, the one `QueryPlan` projection folding SQL/parameters/extensions/predicate-count, the `ST_GeomFromWKB` geometry-view admission, and the bbox-cached `SPATIAL_JOIN` prefilter.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one tagged-union spatial-query axis over the DuckDB engine; `SpatialEngine` the request-scoped run that composes the `tabular/columnar#SCAN` `DuckDbSession` (the folder's ONE connect-install-load owner) with the `spatial` geometry-view prelude plus each query's supplemental extension rows and registers the input Arrow frames. In DuckDB every `ST_` function — the `ST_GeomFromWKB`/`ST_X`/`ST_Y` geometry-view prelude and the `ST_Intersects`/`ST_Contains`/`ST_Within`/`ST_DWithin`/`ST_Transform` predicate-and-reprojection family alike — lives in the `spatial` extension, so `DuckDbExtension.SPATIAL` is the unconditional prelude row every engine run supplies before it can build a `GEOMETRY` view; the only inter-extension axis is the install repository, which is the `DuckDbExtension.repository` ROW PROPERTY the shared session owner already carries (`spatial` core, `h3` community). `SpatialQuery` cases `Join(predicate, left, right, distance)` (an `ST_` binary-predicate join — `ST_Intersects`/`ST_Contains`/`ST_Within`/`ST_DWithin` — the bbox-cached `SPATIAL_JOIN` operator the optimizer inserts as the automatic prefilter, `PointInPolygon` the named `ST_Contains` containment constructor) · `Transform(geometry, source_crs, target_crs)` (the GDAL-backed `ST_Transform(geom, src, dst)` in-engine reprojection the WKB inputs reach without a pyproj round-trip) · `H3Bin(geometry, resolution)` (`h3_latlng_to_cell` binning into one H3 index column over `ST_Y`/`ST_X` centroids — the in-DB half of the two-substrate law, the in-frame half `spatial/grid#GRID`), matched by `match`/`case` closed by `assert_never`. Every case projects through one `QueryPlan` — the `(sql, parameters, extensions, predicate_count)` record one closed `match` folds into a `RuntimeRail[QueryPlan]`, failing early with a `config` fault when an `ST_DWithin` join carries no distance — so the SQL string, the extension requirement, and the receipt's predicate count derive from one traversal, never three parallel folds over the same family: `Join`/`Transform` carry no supplement (the session prepends `SPATIAL`), `H3Bin` carries `(DuckDbExtension.H3,)`, and the session loads the deduplicated union of the run's requirements exactly once, never a per-call reinstall.
- Entry: `SpatialEngine.of` admits the named Arrow inputs into one `Map[str, pa.Table]` (the folder's one immutable-map rail); `SpatialEngine.run` binds the `plan()` rail once so a rejected plan short-circuits before any span or session, then over the resolved plan opens one `_TRACER.start_as_current_span(f"spatial.query.{tag}")` carrying the `rasm.geo.op`/`rasm.geo.predicates` `attributes` around its `catch=duckdb.Error` `boundary` fence so the engine run is an OTel span binding the DuckDB DB-API root, and binds the railed `QueryReceipt` through `.bind`/`.map` so the result table and its content key derive in one rail. The fenced `_dispatch` opens one `DuckDbSession(extensions=(DuckDbExtension.SPATIAL, *plan.extensions)).connect()` bracket — the session owner authors the connect-install-load lifecycle and releases the handle on the boundary exit, so this page carries NO `duckdb.connect()`/`install_extension` site of its own — registers each named Arrow input through `con.register` and projects it into a `ST_GeomFromWKB` geometry view under a `sqlglot` `exp.Identifier` dialect-quoted name, then binds the parametrized plan SQL through `con.execute(sql, parameters)` so the distance/resolution literal never interpolates into the string and every table/view identifier crossing the SQL string is `sqlglot`-quoted while the VALUES stay bound parameters, and returns the `fetch_arrow_table()` egress; the spatial engine is the distinct owner of the geometry-view admission lifecycle the `tabular/query#QUERY` `QueryEngine.Sql` path carries not at all, sharing only the `QueryReceipt`/`ContentIdentity` law with the `query`/`columnar` owners — never a second generic SQL surface duplicating `QueryEngine`'s relational dispatch; the join is accelerated by the DuckDB optimizer's bbox-cached `SPATIAL_JOIN` operator, never an STRtree/sjoin Python loop and never a hand-built R-tree.
- Auto: the geometry crosses as WKB (`GeoDataFrame.to_wkb`/`GeoExpr.st.to_wkb`) so the columnar input stays pyarrow-native at the wire and the engine decodes once with the `spatial`-extension `ST_GeomFromWKB`; CRS normalization rides either the `spatial/geospatial#GEO` `VectorGeoClaim.reproject` (the pyproj `to_crs` on the host frame) or the `Transform` case's in-engine `ST_Transform` for an already-columnar input, so the join operands share one CRS without a second transport; the extension load is the `SPATIAL` prelude unioned with each plan's supplemental rows and deduplicated by the session, so a `Join`/`Transform` run loads only the core-repository `spatial` and an `H3Bin` run adds the community-repository `h3` on top of it; the H3 resolution and the join distance are bound parameters on the plan, never string-spliced literals.
- Receipt: a spatial run threads the shared `columnar` `QueryReceipt.railed("duckdb_spatial", query.tag, table, predicate_count=plan.predicate_count)` keyed by `ContentIdentity` over the result Arrow bytes; no new receipt rail.
- Packages: `tabular/columnar#SCAN` (`DuckDbSession`/`DuckDbExtension` the ONE session rail this engine composes downward — the `SPATIAL`/`H3` rows seed data on the shared table, the hand-rolled connect-install-load site the rail deletes; `QueryReceipt.railed` the shared Arrow-bytes-keyed receipt), `duckdb` (`register`/`execute(query, parameters)`/`fetch_arrow_table`, the `spatial`-extension `GEOMETRY` type; the `spatial`/`h3` SQL surfaces are `data/.api/duckdb.md` `[EXTENSIONS]` rows), `geopandas` (`GeoDataFrame.to_wkb`/`to_arrow`), `polars-st` (`GeoExpr.st.to_wkb` the in-frame WKB input), `pyproj` (the host-frame CRS engine), `pyarrow` (`Table` the egress carrier), `expression` (`tagged_union`/`tag`/`case` the `SpatialQuery` ADT; `Map`/`Map.of_seq` the immutable named-Arrow-frame admission the `of` factory folds a foreign `Mapping` into — the one rail, never a frozendict; `Ok`/`Error` the railed `plan()` fold's rail lift), `sqlglot` (`exp.Identifier(this=name, quoted=True).sql(dialect="duckdb")` the dialect-correct identifier quoting every interpolated table/view name crosses before insertion — the data folder's admitted SQL-structure owner, never a hand-rolled quote-doubling), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-run engine span over the page's own `"rasm.data.spatial.query"` dotted-module handle), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` the railed `plan()` fold's early `config` fault).
- Growth: a new spatial predicate is one `SpatialPredicate` literal the `Join` plan threads under the shared `spatial` extension; a new spatial intent is one `SpatialQuery` case projected by the one `QueryPlan` fold; a new loadable extension is one `DuckDbExtension` row on the SHARED `columnar` table (never a local extension enum); the H3 hierarchy (`h3_cell_to_parent`, `h3_grid_disk`) composes on the existing `H3Bin` SQL and mirrors the `spatial/grid#GRID` in-frame cell ops; zero new surface and never a per-operation `st_join_*` family.
- Boundary: no GIS host coupling, no lonboard/GeoArrow visualization (that is `artifacts`), no durable store; the claims plane is `spatial/geospatial#GEO` and the in-frame DGG plane is `spatial/grid#GRID` — this page owns exactly the in-DB engine. A hand-rolled `duckdb.connect()`-plus-`install_extension`/`load_extension` site where the shared `DuckDbSession` owns the lifecycle, a local `SpatialExtension` enum re-declaring the shared `DuckDbExtension` rows, a `load_extension("spatial")` without the install the session row authors, three parallel `match` folds over the closed family where one `QueryPlan` projection carries SQL/parameters/extensions/predicate-count, a string-interpolated distance or resolution literal where `con.execute(sql, parameters)` binds it, a raw-interpolated `left`/`right`/`geometry`/view identifier where `sqlglot` `exp.Identifier` dialect-quotes it, an `ST_DWithin` join admitted with no distance where the `plan` fold fails early on the typed `config` rail, an STRtree/`sjoin` Python join or a hand-built R-tree where the bbox-cached `SPATIAL_JOIN` operator is the automatic prefilter, a parallel H3 index owner beside the `spatial/grid#GRID` plane, a `QueryReceipt.of` without a derived `ContentKey` where `QueryReceipt.railed` derives the key from the result bytes, a second DuckDB SQL surface duplicating the `query` engine, an un-narrowed `catch=Exception` engine boundary where `catch=duckdb.Error` binds the DB-API root, an attribute-free engine span, and a `trace.get_tracer(__name__)` where the explicit `"rasm.data.spatial.query"` dotted module string is the handle are the deleted forms.

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
        # the shared session rail owns connect-install-load; SPATIAL is the unconditional prelude
        # row (every ST_ function lives in the spatial extension) and the plan supplies supplements.
        with DuckDbSession(extensions=(DuckDbExtension.SPATIAL, *plan.extensions)).connect() as con:
            for name, table in self.inputs.items():
                raw = f"{name}_raw"
                con.register(raw, table)
                con.execute(f"CREATE VIEW {_ident(name)} AS SELECT * EXCLUDE wkb, ST_GeomFromWKB(wkb) AS geom FROM {_ident(raw)}")
            return con.execute(plan.sql, list(plan.parameters)).fetch_arrow_table()
```

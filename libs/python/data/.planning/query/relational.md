# [PY_DATA_RELATIONAL]

The relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union axis — DuckDB SQL, the chained DuckDB relational API, and the dataframe-agnostic narwhals surface — onto one `pyarrow.Table` result; the frontend IS the spec shape, never a parallel backend `StrEnum` knob. The admitted Ibis backend-agnostic expression-IR and the ADBC/ConnectorX remote transport land as additional `QuerySpec` cases on this same owner, acquiring connections through the runtime `TransportResource`. The result fold is spec-agnostic, so the egress, content-key, and `QueryReceipt` are shared with the `columnar` scan owner.

## [1]-[INDEX]

- `[2]-[QUERY]`: the relational query engine over one `QuerySpec` axis materializing to uniform Arrow.

## [2]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner over `duckdb`/`narwhals` discriminating by the `QuerySpec` tagged-union axis (the single discriminant; the frontend IS the spec shape). `QuerySpec` cases `Sql` (relational SQL) · `Rel` (the chained relational API) · `Agnostic` (the dataframe-agnostic expression surface). A new query frontend is one `QuerySpec` case, never a `sql_query`/`rel_query`/`nw_query` method family.
- Cases: `QuerySpec.Sql(text)` runs `duckdb.connect().sql(text)` over registered Arrow/Delta inputs and binds the result with `to_arrow_table()` (zero-copy Arrow) · `Rel(filter, project, group_by)` chains `from_arrow(...).filter(...).project(...).aggregate(...)` returning a `DuckDBPyRelation`, terminal `to_arrow_table()` · `Agnostic(select, filter, group_by)` admits any native frame via `narwhals.from_native`, composes `filter`/`group_by`/`agg`/`select` against `narwhals.col`/`narwhals.Expr` (a grouped select folds each selected column through a `sum` aggregation expression, never a bare column reference `group_by` rejects), and lands Arrow through `narwhals.DataFrame.to_arrow`, so one query authored once runs on polars, pandas, pyarrow, or duckdb relations underneath.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; `QueryEngine.run` folds the `QuerySpec` through `match`/`case` closed by `assert_never` (the totality proof — a new case forces a new arm) and returns a `RuntimeRail[pa.Table]`; the DuckDB connection is request-scoped, never a module global.
- Auto: Arrow result binding is uniform — every case terminates in a `pyarrow.Table` via `DuckDBPyRelation.to_arrow_table` or `narwhals.DataFrame.to_arrow`, so the egress, content-key, and `QueryReceipt` fold are spec-agnostic; the narwhals path keeps the query lazy (`narwhals.LazyFrame`) until `collect()`, preserving polars/duckdb predicate-pushdown.
- Receipt: a run contributes a `QueryReceipt` (the `columnar` scan-receipt owner) keyed by `ContentIdentity` over the result bytes; no new receipt rail.
- Packages: `duckdb` (`connect`/`sql`/`from_arrow`/`DuckDBPyRelation.filter`/`project`/`aggregate`/`to_arrow_table`), `narwhals` (`from_native`/`col`/`Expr`/`LazyFrame.collect`/`DataFrame.to_arrow`), `ibis-framework` (the backend-agnostic expression IR), `adbc-driver-manager`, `connectorx`, `pyarrow` (`Table`), runtime (`RuntimeRail`/`ContentIdentity`/`TransportResource`).
- Growth: a new query frontend is one `QuerySpec` case (the Ibis IR and the ADBC/ConnectorX remote transport land here, ConnectorX behind the `<3.15` gated band with a function-local import, never a module-top import on this cp315-core page); a new relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail, no global connection, no SQL-string templating engine, no parallel `StrEnum` backend discriminant duplicating the `QuerySpec` tag; remote-connection acquisition routes through the runtime `TransportResource`, never a second transport owner; a per-frontend query class family and a generic dataframe wrapper are the deleted forms.

```python signature
from typing import Any, Literal, assert_never

import duckdb
import narwhals as nw
import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, boundary


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic"] = tag()
    sql: str = case()
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[tuple[str, ...], str | None, tuple[str, ...]] = case()

    @staticmethod
    def Sql(text: str) -> "QuerySpec":
        return QuerySpec(sql=text)

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(select: tuple[str, ...], filter_expr: str | None = None, group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(agnostic=(select, filter_expr, group_by))


class QueryEngine(Struct, frozen=True):
    inputs: dict[str, Any]

    @classmethod
    def of(cls, inputs: dict[str, Any]) -> "QueryEngine":
        return cls(inputs=inputs)

    def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":
        return boundary(f"query.{spec.tag}", lambda: self._dispatch(spec))

    def _dispatch(self, spec: QuerySpec) -> pa.Table:
        match spec:
            case QuerySpec(tag="sql", sql=text):
                con = duckdb.connect()
                for name, frame in self.inputs.items():
                    con.register(name, frame)
                return con.sql(text).to_arrow_table()
            case QuerySpec(tag="rel", rel=(flt, project, group_by)):
                con = duckdb.connect()
                _, frame = next(iter(self.inputs.items()))
                rel = con.from_arrow(frame)
                rel = rel.filter(flt) if flt else rel
                rel = rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))
                return rel.to_arrow_table()
            case QuerySpec(tag="agnostic", agnostic=(select, flt, group_by)):
                _, frame = next(iter(self.inputs.items()))
                lf = nw.from_native(frame).lazy()
                lf = lf.filter(nw.col(flt)) if flt else lf
                shaped = (
                    lf.group_by(*group_by).agg(*(nw.col(c).sum() for c in select))
                    if group_by
                    else lf.select(*select)
                )
                return shaped.collect().to_arrow()
            case unreachable:
                assert_never(unreachable)
```

## [3]-[RESEARCH]

- [NARWHALS_LAZY]: the `narwhals` `from_native`/`lazy`/`col`/`Expr.sum`/`LazyFrame.{filter,group_by,agg,select,collect}`/`DataFrame.to_arrow` surface the `Agnostic` arm transcribes confirms against a folder `narwhals` `.api` catalogue authored on admission; the lazy agnostic-query surface stays a catalogue-pending settled form until that catalogue lands.
- [IBIS_REMOTE]: the `ibis-framework` expression-IR-to-Arrow materialization, the `adbc-driver-manager` cp315 source-build transport, and the `<3.15`-gated `connectorx` fast SQL-to-Arrow transport (imported function-local) confirm against folder `.api` catalogues before the Ibis and remote-transport `QuerySpec` cases admit as settled fence code.

# [PY_DATA_QUERY]

The relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union axis — DuckDB SQL, the chained DuckDB relational API, and the dataframe-agnostic narwhals surface — onto one `pyarrow.Table` result; the frontend IS the spec shape, never a parallel backend `StrEnum` knob. The admitted Ibis backend-agnostic expression-IR and the ADBC/ConnectorX remote transport land as additional `QuerySpec` cases on this same owner, acquiring connections through the runtime `TransportResource`. The result fold is spec-agnostic, so the egress, content-key, and `QueryReceipt` are shared with the `columnar` scan owner.

## [01]-[INDEX]

- [01]-[QUERY]: the relational query engine over one `QuerySpec` axis materializing to uniform Arrow.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner over `duckdb`/`narwhals`/`ibis`/ADBC discriminating by the `QuerySpec` tagged-union axis (the single discriminant; the frontend IS the spec shape). `QuerySpec` cases `Sql` (relational SQL) · `Rel` (the chained relational API) · `Agnostic` (the dataframe-agnostic expression surface) · `Ir` (the Ibis backend-agnostic expression IR) · `Remote` (ADBC/ConnectorX remote SQL-to-Arrow transport). A new query frontend is one `QuerySpec` case, never a `sql_query`/`rel_query`/`nw_query`/`ibis_query` method family.
- Cases: `QuerySpec.Sql(text)` runs `duckdb.connect().sql(text)` over registered Arrow/Delta inputs and binds the result with `to_arrow_table()` (zero-copy Arrow) · `Rel(filter, project, group_by)` chains `from_arrow(...).filter(...).project(...).aggregate(...)` returning a `DuckDBPyRelation`, terminal `to_arrow_table()` · `Agnostic(select, filter, group_by)` admits any native frame via `narwhals.from_native`, composes `filter`/`group_by`/`agg`/`select` against `narwhals.col`/`narwhals.Expr` (a grouped select folds each selected column through a `sum` aggregation expression, never a bare column reference `group_by` rejects), and lands Arrow through `narwhals.DataFrame.to_arrow` · `Ir(expr, backend_uri)` binds one `ibis` expression to the backend `ibis.connect(backend_uri)` selects (DuckDB the default when `backend_uri` is `None`) and materializes through `BaseBackend.to_pyarrow(expr)` — the same lazy SQL-via-`sqlglot` expression compiles to DuckDB/DataFusion/Polars/remote SQL with no rewrite · `Remote(sql, transport, driver)` acquires the remote connection through the runtime `TransportResource`, dispatching the ADBC `dbapi.connect(driver, uri).cursor().execute(sql).fetch_arrow_table()` path on the cp315 core and the `connectorx.read_sql(uri, sql, return_type="arrow")` partitioned path when the `<3.15` band is live, both terminating in the uniform `pyarrow.Table`.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; `QueryEngine.run` folds the `QuerySpec` through `match`/`case` closed by `assert_never` (the totality proof — a new case forces a new arm) and returns a `RuntimeRail[pa.Table]`; the DuckDB connection is request-scoped, never a module global; the Ibis and remote connections acquire and release per run.
- Auto: Arrow result binding is uniform — every case terminates in a `pyarrow.Table` via `DuckDBPyRelation.to_arrow_table`, `narwhals.DataFrame.to_arrow`, `ibis.BaseBackend.to_pyarrow`, or `Cursor.fetch_arrow_table`/`connectorx.read_sql(return_type="arrow")`, so the egress, content-key, and `QueryReceipt` fold are spec-agnostic; the narwhals path keeps the query lazy (`narwhals.LazyFrame`) until `collect()`; the Ibis path stays an unbound expression until `to_pyarrow`, so predicate pushdown crosses the backend; Ibis (analytical-intent portability across 20+ backends) and narwhals (input-type agnosticism) are distinct axes both carried on the owner. `ibis` and `adbc-driver-manager` are cp315-clean and import module-top, while `connectorx` rides the `python_version<'3.15'` gated band so its arm imports the dist function-local under `# noqa: PLC0415`, never a module-top import on this cp315-core page.
- Receipt: a run contributes a `QueryReceipt` (the `columnar` scan-receipt owner) keyed by `ContentIdentity` over the result bytes; no new receipt rail.
- Packages: `duckdb` (`connect`/`sql`/`from_arrow`/`DuckDBPyRelation.filter`/`project`/`aggregate`/`to_arrow_table`), `narwhals` (`from_native`/`col`/`Expr`/`LazyFrame.collect`/`DataFrame.to_arrow`), `ibis-framework` (`connect`/`BaseBackend.to_pyarrow`, the backend-agnostic expression IR), `adbc-driver-manager` (`dbapi.connect`/`Cursor.execute`/`fetch_arrow_table`), `connectorx` (`read_sql(return_type="arrow")`, the `<3.15` gated arm), `pyarrow` (`Table`), runtime (`RuntimeRail`/`ContentIdentity`/`TransportResource`).
- Growth: a new query frontend is one `QuerySpec` case; a new remote driver is one `RemoteDriver` literal the `Remote` arm dispatches; a new relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail, no global connection, no SQL-string templating engine, no parallel `StrEnum` backend discriminant duplicating the `QuerySpec` tag; remote-connection acquisition routes through the runtime `TransportResource`, never a second transport owner; a per-frontend query class family, a backend-specific SQL string inside an Ibis pipeline, and a generic dataframe wrapper are the deleted forms.

```python signature
from typing import Any, Literal, assert_never

import duckdb
import ibis
import narwhals as nw
import pyarrow as pa
from adbc_driver_manager import dbapi
from expression import case, tag, tagged_union
from ibis.expr.types import Table as IbisTable
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.roots import TransportResource

type RemoteDriver = Literal["adbc", "connectorx"]


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic", "ir", "remote"] = tag()
    sql: str = case()
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[tuple[str, ...], str | None, tuple[str, ...]] = case()
    ir: tuple[IbisTable, str | None] = case()
    remote: tuple[str, str, RemoteDriver] = case()

    @staticmethod
    def Sql(text: str) -> "QuerySpec":
        return QuerySpec(sql=text)

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(select: tuple[str, ...], filter_expr: str | None = None, group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(agnostic=(select, filter_expr, group_by))

    @staticmethod
    def Ir(expr: IbisTable, backend_uri: str | None = None) -> "QuerySpec":
        return QuerySpec(ir=(expr, backend_uri))

    @staticmethod
    def Remote(sql: str, dsn: str, driver: RemoteDriver = "adbc") -> "QuerySpec":
        return QuerySpec(remote=(sql, dsn, driver))


class QueryEngine(Struct, frozen=True):
    inputs: dict[str, Any]

    @classmethod
    def of(cls, inputs: dict[str, Any]) -> "QueryEngine":
        return cls(inputs=inputs)

    def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":
        return boundary(f"query.{spec.tag}", lambda: self._dispatch(spec))

    def _dispatch(self, spec: QuerySpec) -> pa.Table:  # noqa: PLR0911
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
                shaped = lf.group_by(*group_by).agg(*(nw.col(c).sum() for c in select)) if group_by else lf.select(*select)
                return shaped.collect().to_arrow()
            case QuerySpec(tag="ir", ir=(expr, backend_uri)):
                backend = ibis.connect(backend_uri) if backend_uri else ibis.duckdb.connect()
                return backend.to_pyarrow(expr)
            case QuerySpec(tag="remote", remote=(sql, dsn, driver)):
                return _remote(sql, dsn, driver)
            case unreachable:
                assert_never(unreachable)


def _remote(sql: str, dsn: str, driver: RemoteDriver) -> pa.Table:
    if driver == "adbc":
        with dbapi.connect(uri=dsn) as conn, conn.cursor() as cur:
            cur.execute(sql)
            return cur.fetch_arrow_table()
    import connectorx as cx  # noqa: PLC0415

    return cx.read_sql(dsn, sql, return_type="arrow")
```

## [03]-[RESEARCH]

- [IBIS_BACKEND_NAMESPACE]: the `ibis` `connect(uri)`/`BaseBackend.to_pyarrow(expr)` surface the `Ir` arm transcribes is catalogue-confirmed against the folder `ibis-framework` `.api`; the `ibis.duckdb.connect()` default-backend namespace accessor the no-`backend_uri` branch binds is the one spelling the catalogue lists only as the implicit DuckDB default, so the default-backend access confirms the `ibis.duckdb` submodule entry against the live distribution before the unbound-default branch treats it as settled.
- [REMOTE_TRANSPORT_DSN]: the `adbc-driver-manager` `dbapi.connect(uri=)`/`Connection.cursor()`/`Cursor.execute(sql)`/`fetch_arrow_table()` and the `<3.15`-gated `connectorx` `read_sql(conn, query, return_type="arrow")` surfaces the `Remote` arm transcribes are catalogue-confirmed against the folder `adbc-driver-manager`/`connectorx` `.api`; the one open seam is the `dsn` derivation from the runtime `TransportResource` — the `Remote` arm receives a resolved DSN string, and the `TransportResource`-to-DSN projection lands on the runtime `roots` seam rather than re-minting a second transport owner here.

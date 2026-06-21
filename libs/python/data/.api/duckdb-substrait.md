# [PY_DATA_API_DUCKDB_SUBSTRAIT]

`duckdb-substrait` is the DuckDB community extension that bridges DuckDB query plans and the Substrait portable relational algebra for the data substrait-portability rail: it serializes a DuckDB-parsed-and-bound SQL query into a binary or JSON Substrait plan through `get_substrait`/`get_substrait_json`, and consumes a foreign Substrait plan back into an executable DuckDB relation through `from_substrait`/`from_substrait_json`. The data owner loads the extension into a live `DuckDBPyConnection`, drives the four connection-bound methods (and their SQL table-function twins) to round-trip plans across engines, and never re-implements the Substrait protobuf encoder/decoder the extension already owns; the plan BLOB and its JSON twin are the cross-engine wire artifact for `SUBSTRAIT_PORTABILITY`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `duckdb-substrait`
- package: `duckdb-substrait` (loadable DuckDB extension `substrait`; PyPI wheel distribution `duckdb-extension-substrait`)
- import: `import duckdb` then `con.load_extension("substrait")` (no top-level Python module; surface attaches to `DuckDBPyConnection`)
- owner: `data`
- rail: substrait-portability
- license: `MIT` (community extension, tracking DuckDB core)
- distribution: loadable DuckDB community extension `substrait`, rebuilt per DuckDB release and resolved against the locked `duckdb 1.5.4` engine; there is no PyPI module floor — the surface is a connection capability fetched from the community repository, not an importable wheel
- entry points: connection methods `con.get_substrait` / `con.get_substrait_json` / `con.from_substrait` / `con.from_substrait_json`; SQL table functions `get_substrait` / `get_substrait_json` / `from_substrait` / `from_substrait_json`; no console script
- capability: serialize a DuckDB SQL `SELECT` query into a binary (BLOB) or JSON Substrait plan, and execute a foreign binary or JSON Substrait plan against DuckDB returning a `DuckDBPyRelation`; optimizer participation governed by `enable_optimizer` on generation and by connection-level settings on consumption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and relation roots
- rail: substrait-portability

The extension surfaces no extension-owned Python class; it attaches table functions to the host engine and exposes them as bound methods on the DuckDB Python client types. `get_substrait`/`get_substrait_json` return a `DuckDBPyRelation` whose first column carries the plan artifact (BLOB or JSON string), read via `.fetchone()[0]`. `from_substrait`/`from_substrait_json` return a directly queryable `DuckDBPyRelation` over the executed plan.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [RAIL]                                                      |
| :-----: | :-------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `DuckDBPyConnection`  | host connection | live DuckDB connection that loads and hosts the extension   |
|  [02]   | `DuckDBPyRelation`    | host relation   | lazy relation carrying the plan artifact or executed result |
|  [03]   | substrait plan (BLOB) | wire artifact   | binary Substrait protobuf plan; the cross-engine payload    |
|  [04]   | substrait plan (JSON) | wire artifact   | JSON-encoded Substrait plan; human-inspectable twin         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection plan serialization and execution
- rail: substrait-portability

The serialization rows accept a SQL `SELECT` string and the keyword-only `enable_optimizer` (default `True`); they return a single-column `DuckDBPyRelation` whose first cell holds the plan. The execution rows accept the plan artifact and return an executable relation; they respect connection-level optimizer settings rather than an inline flag.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                           | [CAPABILITY]                                       |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `con.get_substrait`       | `get_substrait(query: str, *, enable_optimizer: bool = True) -> DuckDBPyRelation`      | serialize a SQL query to a binary Substrait plan   |
|  [02]   | `con.get_substrait_json`  | `get_substrait_json(query: str, *, enable_optimizer: bool = True) -> DuckDBPyRelation` | serialize a SQL query to a JSON Substrait plan     |
|  [03]   | `con.from_substrait`      | `from_substrait(proto: bytes) -> DuckDBPyRelation`                                     | execute a binary Substrait plan, return a relation |
|  [04]   | `con.from_substrait_json` | `from_substrait_json(json: str) -> DuckDBPyRelation`                                   | execute a JSON Substrait plan, return a relation   |

[ENTRYPOINT_SCOPE]: SQL table functions
- rail: substrait-portability

The same four operations exist as DuckDB table functions invoked via `con.sql("CALL ...")` once the extension is loaded; `enable_optimizer` is a named function argument on the generation functions. `get_substrait` yields a BLOB, `get_substrait_json` yields a `JSON`/`VARCHAR`, and the `from_*` functions yield the executed result rows.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                           | [CAPABILITY]                         |
| :-----: | :-------------------- | :--------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `get_substrait`       | `CALL get_substrait('<select-sql>'[, enable_optimizer = <bool>])`      | produce binary Substrait plan (BLOB) |
|  [02]   | `get_substrait_json`  | `CALL get_substrait_json('<select-sql>'[, enable_optimizer = <bool>])` | produce JSON Substrait plan          |
|  [03]   | `from_substrait`      | `CALL from_substrait(<plan-blob>)`                                     | execute binary Substrait plan        |
|  [04]   | `from_substrait_json` | `CALL from_substrait_json('<plan-json>')`                              | execute JSON Substrait plan          |

[ENTRYPOINT_SCOPE]: extension load
- rail: substrait-portability

The extension is fetched from the community repository and loaded into the connection before any plan call; loading attaches the four methods and table functions to that connection.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                             | [CAPABILITY]                          |
| :-----: | :---------------------- | :------------------------------------------------------- | :------------------------------------ |
|  [01]   | `con.install_extension` | `install_extension("substrait", repository="community")` | fetch the community extension binary  |
|  [02]   | `con.load_extension`    | `load_extension("substrait")`                            | attach the substrait surface to `con` |
|  [03]   | SQL install/load        | `INSTALL substrait FROM community; LOAD substrait;`      | SQL-side install and load             |

## [04]-[IMPLEMENTATION_LAW]

[SUBSTRAIT_PORTABILITY]:
- load axis: `con.install_extension("substrait", repository="community")` then `con.load_extension("substrait")` is the single attach path; the surface is connection-scoped, so the data owner loads it on the live `DuckDBPyConnection` before invoking any plan method, never as a top-level Python import.
- serialize axis: `get_substrait` owns binary-plan emission and `get_substrait_json` owns JSON-plan emission from one SQL `SELECT` string; both return a single-column `DuckDBPyRelation` read via `.fetchone()[0]` — the BLOB is the canonical cross-engine artifact and the JSON is its inspectable twin, never two parallel plan models.
- consume axis: `from_substrait(proto=...)` executes a binary plan and `from_substrait_json(json=...)` executes a JSON plan, each returning an executable `DuckDBPyRelation`; foreign plans produced by other Substrait producers are accepted on this row, never re-parsed by a hand-rolled decoder.
- optimizer axis: `enable_optimizer` (default `True`) is the keyword-only generation flag controlling whether DuckDB optimizes before serializing; consumption (`from_substrait`/`from_substrait_json`) always respects connection-level optimizer settings rather than an inline flag, so the data owner sets optimizer policy on the connection for execution.
- SQL-twin axis: the four operations are equivalently reachable as `CALL get_substrait(...)` / `CALL from_substrait(...)` table functions; the Python connection methods are the preferred owner surface, and the `CALL` form is used only where a raw SQL plan string is the input boundary.
- evidence: each round-trip captures the source SQL, the plan kind (binary vs JSON), the plan byte length, the `enable_optimizer` setting, and the executed relation's column schema as a portability receipt.
- boundary: the extension owns Substrait protobuf encode/decode and DuckDB plan binding; the data owner composes it for cross-engine plan exchange and never re-implements the Substrait codec, the DuckDB binder, or a parallel plan struct; query inputs are `SELECT`-shaped, and execution is delegated to the returned `DuckDBPyRelation`.

[INTEGRATION_STACKING]:
- two-engine spine: this extension is the DuckDB end of the same wire `Plan` that `datafusion.substrait` owns — a DuckDB `con.get_substrait(sql)` BLOB binds on DataFusion via `substrait.Consumer.from_substrait_plan`, and a DataFusion `substrait.Producer.to_substrait_plan(...).encode()` binds on DuckDB via `con.from_substrait(...)`; the protobuf BLOB and its JSON twin are the one cross-engine artifact, never re-serialized per engine.
- arrow egress continuity: `from_substrait` returns a `DuckDBPyRelation` whose `fetch_arrow_table`/`fetch_record_batch`/`pl()` egress feeds polars/pyarrow or a `write_deltalake` commit with no copy, so a foreign plan executes and lands in the lakehouse on one Arrow seam.
- optimizer placement: set DuckDB optimizer policy on the connection before `from_substrait` (which respects connection settings) and toggle `enable_optimizer` only on the `get_substrait` generation call, so optimizer control stays on the two real surfaces rather than a wrapper flag.

[RAIL_LAW]:
- Package: `duckdb-substrait`
- Owns: bidirectional translation between DuckDB query plans and Substrait binary/JSON plans, and execution of foreign Substrait plans against DuckDB
- Accept: serializing DuckDB `SELECT` queries to portable Substrait plans and executing foreign Substrait plans (including plans produced by `datafusion.substrait`), feeding the data portability and cross-engine exchange owners over one shared wire `Plan`
- Reject: wrapper-renames of `get_substrait`/`from_substrait`; a hand-rolled Substrait protobuf codec; a parallel plan model per encoding; re-serializing a plan per engine where one BLOB crosses DuckDB and DataFusion; a top-level module import path where the surface is connection-bound; optimizer control invented outside `enable_optimizer` and connection settings

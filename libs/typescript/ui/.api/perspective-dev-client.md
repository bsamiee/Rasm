# [TS_UI_API_PERSPECTIVE_DEV_CLIENT]

`@perspective-dev/client` owns the millions-of-rows streaming-analytics engine: a WASM core holds appendable indexed `Table`s from which `View`s materialize, each maintained INCREMENTALLY as updates land. One `Client` API fronts an in-page `worker()`, a remote `websocket(url)` host, or an in-browser `init_server` — data location is a wiring decision, never an API fork. Arrow is the wire: `Table` ingests IPC buffers, `view.to_arrow()` emits them, and `on_update` row-mode deltas ARE Arrow buffers with no JSON detour.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/client`
- package: `@perspective-dev/client` (Apache-2.0)
- module: `sideEffects: false`; `.` condition-splits `node` (synchronous singleton client + `WebSocketServer` host) against the browser Worker/WASM boot; subpaths `./node`, `./inline` (base64-embedded WASM, no separate fetch), `./virtual_servers/*`
- runtime: isomorphic browser + node — one `Client`/`Table`/`View` surface, the node build swapping Worker spawn for a required `MessagePort`; no peers
- plane: `plane:runtime` (W4 `ui`); rail: streaming tabular analytics — the pivot/aggregation engine

## [02]-[CLIENT_AND_TABLE]

[PERSPECTIVE]: `perspective.worker(Promise<SharedWorker|ServiceWorker|Worker|MessagePort>?) -> Promise<Client>` `perspective.websocket(string|URL) -> Promise<Client>` `perspective.init_client(unknown) -> void` `perspective.init_server(unknown,unknown?) -> void` `perspective.getCompiledClientWasm() -> Promise<WebAssembly.Module>`
[CLIENT]: `Client.table(string|ArrayBuffer|Record<string,unknown[]>|Record<string,unknown>[]|Schema,{name?:string;format?:"csv"|"json"|"columns"|"arrow"|"ndjson";index?:string;limit?:number;page_to_disk?:boolean}?) -> Promise<Table>` `Client.open_table(string) -> Promise<Table>` `Client.join(Table|string,Table|string,string,{join_type?:"inner"|"left"|"outer";name?:string;right_on?:string}?) -> Promise<Table>` `Client.get_hosted_table_names() -> Promise<string[]>` `Client.on_hosted_tables_update(unknown) -> Promise<number>` `Client.system_info() -> Promise<SystemInfo>` `Client.new_proxy_session(unknown) -> ProxySession` `Client.on_error(unknown) -> Promise<number>` `Client.terminate() -> void`
[TABLE]: `Table.view(ViewConfigUpdate?) -> Promise<View>` `Table.update(TableData,{port_id?:number;format?:string}?) -> Promise<void>` `Table.remove(unknown,unknown?) -> Promise<void>` `Table.replace(unknown,unknown?) -> Promise<void>` `Table.clear() -> Promise<void>` `Table.schema() -> Promise<Schema>` `Table.columns() -> Promise<string[]>` `Table.size() -> Promise<number>` `Table.validate_expressions(Record<string,string>) -> Promise<unknown>` `Table.make_port() -> Promise<number>` `Table.get_index() -> Promise<string|null>` `Table.get_limit() -> Promise<number|null>` `Table.get_name() -> string` `Table.get_client() -> Client` `Table.delete({lazy?:boolean}?) -> Promise<void>` `Table.on_delete(unknown) -> Promise<number>`

- `index` upserts on the key column; `limit` ring-buffers rows; `page_to_disk` spills canonical data to a memory-mapped file (native) or OPFS (WASM Worker) past the memory ceiling — per-feed table modes over the in-memory default.
- `join` returns a LIVE reactive table re-deriving on either side's update; a hand-maintained merged copy beside it is the defect.

## [03]-[VIEW_PROTOCOL]

`ViewConfigUpdate` is the whole query surface; `sort` directions carry column-axis and absolute variants, and `expressions` validate through `Table.validate_expressions`:

| [INDEX] | [FIELD]                                | [SHAPE]                                        | [ROLE]                                     |
| :-----: | :------------------------------------- | :--------------------------------------------- | :----------------------------------------- |
|  [01]   | `group_by` / `split_by`                | `string[]`                                     | row pivots / column pivots                 |
|  [02]   | `columns`                              | `(string \| null)[]`                           | projected columns                          |
|  [03]   | `aggregates`                           | `Record<string, string \| [string, string[]]>` | per-column aggregate; tuple for dependent  |
|  [04]   | `filter` + `filter_op`                 | `[column, operator, term][]` · `"and" \| "or"` | predicate rows                             |
|  [05]   | `sort`                                 | `[column, direction][]`                        | sort rows; column-axis + absolute variants |
|  [06]   | `expressions`                          | `Record<string, string>`                       | ExprTK computed columns                    |
|  [07]   | `group_by_depth` / `group_rollup_mode` | `number` · `"rollup" \| "flat" \| "total"`     | tree depth and rollup presentation         |

Aggregate roster (numeric): `sum` `abs sum` `sum not null` `avg` `mean` `count` `distinct count` `dominant` `first` `last` `last by index` `high` `low` `max` `min` `high minus low` `last minus first` `median` `q1` `q3` `pct sum parent` `pct sum total` `stddev` `var` `unique` `weighted mean` `min by` `max by`; string adds `join`; date/datetime and boolean carry the applicable subset.

[VIEW]: `View.to_arrow(ViewWindow?) -> Promise<ArrayBuffer>` `View.to_columns(unknown?) -> Promise<Record<string,unknown[]>>` `View.to_json(unknown?) -> Promise<Record<string,unknown>[]>` `View.to_csv(unknown?) -> Promise<string>` `View.to_ndjson(unknown?) -> Promise<string>` `View.on_update((updated:{port_id:number;delta?:ArrayBuffer})=>void,{mode?:"row"}?) -> Promise<number>` `View.schema() -> Promise<Schema>` `View.expression_schema() -> Promise<Schema>` `View.dimensions() -> Promise<{num_table_rows:number;num_table_columns:number;num_view_rows:number;num_view_columns:number}>` `View.get_min_max(string) -> Promise<[unknown,unknown]>` `View.collapse(number) -> Promise<number>` `View.expand(number) -> Promise<number>` `View.set_depth(number) -> Promise<void>` `View.on_delete(unknown) -> Promise<number>` `View.delete() -> Promise<void>`

## [04]-[HOST_TOPOLOGY]

- Browser: `worker()` runs the engine in a dedicated/shared Worker off the UI thread; `init_server(SERVER_WASM)` (the lockstep `@perspective-dev/server` binary) hosts the engine in-page for the no-backend case, and `getCompiledClientWasm()` hands a spawned Worker a structured-cloneable module to self-instantiate its `Client` with no re-fetch.
- Node: `WebSocketServer` (with `make_session` / `make_client`) publishes named `Table`s over `ws`; a browser client attaches through `websocket(url)` then `open_table(name)`, and every client's `View`s update incrementally off the server's indexed feed.
- Virtual servers: `./virtual_servers/*` with `VirtualServer` / `VirtualDataSlice` / `GenericSQLVirtualServerModel` carry the session machinery over custom transports, `GenericSQLVirtualServerModel` fronting a SQL-backed source.

## [05]-[IMPLEMENTATION_LAW]

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): one columnar format end to end — a wire Arrow frame feeds `Client.table(buf,{format:"arrow",index})`, `Table.update(arrowBuf)` appends deltas, and `View.to_arrow()` hands results to `tableFromIPC` for any Arrow consumer; never a JSON re-materialization between engines.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): each handle is a `Scope`-bracketed resource with explicit `delete()`; `on_update`/`on_error` bridge into streams at the async seam, and a rendered surface's `ViewConfigUpdate` derives from an atom — a config change creates a new `View` and deletes the old.
- `@perspective-dev/viewer`(`.api/perspective-dev-viewer.md`): the viewer element consumes this client's `Table` via `load()` and drives its own `View` lifecycle; headless consumers (exports, alerts, derived feeds) hold `View`s directly.
- `@tanstack/react-table`(`.api/tanstack-react-table.md`): boundary — the `Grid` fold owns interactive accessible grids over client-modeled rows, perspective owns engine-scale streaming pivot/aggregation over a feed; derivation locus decides, client-modeled rows to `Grid` and engine-maintained aggregates to perspective, one surface never both.

[RAIL_LAW]:
- Package: `@perspective-dev/client`
- Owns: the streaming analytics engine — Client/Table/View lifecycle, indexed / limited / spill table modes, the `ViewConfigUpdate` query vocabulary, the aggregate roster, ExprTK expression columns, reactive `join`, Arrow ingress/egress/deltas, and the worker / websocket / in-browser-server / node-publisher host topologies.
- Accept: Arrow as the only inter-engine format; `index` upserts, `limit` ring buffers, and `page_to_disk` spill chosen per feed; view configs as atom-derived values rotated create-new-delete-old; `validate_expressions` before shipping an expression; `open_table` against host-published names; `Scope`-bracketed `delete()` on every handle.
- Reject: JSON round-trips between Arrow-capable engines; a hand-maintained aggregate or join copy beside a live `View`/`join`; polling where `on_update` streams; engine work on the UI thread when `worker()` exists; a leaked handle; perspective standing in for the `Grid` interactive-collection regime.

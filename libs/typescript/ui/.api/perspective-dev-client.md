# [TS_UI_API_PERSPECTIVE_DEV_CLIENT]

[PACKAGE_SURFACE]:
- package: `@perspective-dev/client` (Apache-2.0)
- module: `sideEffects: false`; condition-split `exports["."]` — `node` → `dist/esm/perspective.node.js` (pre-instantiated synchronous singleton client + `WebSocketServer` host), default → `dist/esm/perspective.js` (browser, Worker/WASM bootstrapping); subpaths `./node`, `./inline` (base64-embedded WASM, no separate `.wasm` fetch), `./virtual_servers/*`.
- asset: deps `@perspective-dev/server` (the WASM engine binary, lockstep), `pro_self_extracting_wasm` (self-extracting WASM loader), `ws` (node), `stoppable`; no peers. Browser WASM assets: the client's own `dist/wasm/perspective-js.wasm` (auto-bootstrapped inside `worker()`), `@perspective-dev/server/dist/wasm/perspective-server.wasm` for in-browser/in-worker server hosting via `init_server(wasm)`.
- bundling: Vite imports each `.wasm` with `?url` + `fetch(url)` and requires `build.target: "esnext"`; the CDN `dist/cdn/*` build needs no bootstrap.
- runtime: `Client`/`Table`/`View` are one API across browser and node — the node build swaps Worker spawning for a required `MessagePort` and adds the server host.
- plane: `plane:runtime` (W4 `ui`); rail: streaming tabular analytics — the pivot/aggregation engine.

`@perspective-dev/client` is the millions-of-rows live-analytics engine: a WASM core holding `Table`s (indexed, appendable, streamable) from which `View`s materialize — each View one `ViewConfigUpdate` value of group/split/aggregate/filter/sort/expressions that the engine maintains INCREMENTALLY as updates land. Its protocol is location-transparent: the same `Client` API fronts an in-page worker (`worker()`), a remote node host (`websocket(url)`), or an in-browser server (`init_server`), so where the data lives is a wiring decision, not an API fork. Arrow is the wire: tables ingest Arrow IPC buffers, `view.to_arrow()` emits them, and `on_update` row-mode deltas ARE Arrow buffers — the engine and the `apache-arrow` substrate speak one columnar format with no JSON detour.

## [01]-[CLIENT_AND_TABLE]

[PERSPECTIVE]: `perspective.worker(Promise<SharedWorker|ServiceWorker|Worker|MessagePort>?) -> Promise<Client>` `perspective.websocket(string|URL) -> Promise<Client>` `perspective.init_client(unknown) -> void` `perspective.init_server(unknown,unknown?) -> void` `perspective.getCompiledClientWasm() -> Promise<WebAssembly.Module>`
[CLIENT]: `Client.table(string|ArrayBuffer|Record<string,unknown[]>|Record<string,unknown>[]|Schema,{name?:string;format?:"csv"|"json"|"columns"|"arrow"|"ndjson";index?:string;limit?:number;page_to_disk?:boolean}?) -> Promise<Table>` `Client.open_table(string) -> Promise<Table>` `Client.join(Table|string,Table|string,string,{join_type?:"inner"|"left"|"outer";name?:string;right_on?:string}?) -> Promise<Table>` `Client.get_hosted_table_names() -> Promise<string[]>` `Client.on_hosted_tables_update(unknown) -> Promise<number>` `Client.system_info() -> Promise<SystemInfo>` `Client.new_proxy_session(unknown) -> ProxySession` `Client.on_error(unknown) -> Promise<number>` `Client.terminate() -> void`
[TABLE]: `Table.view(ViewConfigUpdate?) -> Promise<View>` `Table.update(TableData,{port_id?:number;format?:string}?) -> Promise<void>` `Table.remove(unknown,unknown?) -> Promise<void>` `Table.replace(unknown,unknown?) -> Promise<void>` `Table.clear() -> Promise<void>` `Table.schema() -> Promise<Schema>` `Table.columns() -> Promise<string[]>` `Table.size() -> Promise<number>` `Table.validate_expressions(Record<string,string>) -> Promise<unknown>` `Table.make_port() -> Promise<number>` `Table.get_index() -> Promise<string|null>` `Table.get_limit() -> Promise<number|null>` `Table.get_name() -> string` `Table.get_client() -> Client` `Table.delete({lazy?:boolean}?) -> Promise<void>` `Table.on_delete(unknown) -> Promise<number>`

- `index` makes updates UPSERTS on the key column; `limit` caps rows ring-buffer style — the two table modes every streaming feed chooses between.
- `page_to_disk` backs the table's canonical data with on-disk storage — a memory-mapped file on native, OPFS on a WASM Worker — for feeds past the memory ceiling; in-memory is the default.
- `join` returns a LIVE table — updates on either side re-derive; a hand-maintained merged copy beside it is the defect.

## [02]-[VIEW_PROTOCOL]

`ViewConfigUpdate` is the whole query surface (current spellings — the retired `row_pivots`/`column_pivots` names are dead); `sort` directions span `asc`, `desc`, `col asc`, `col desc`, `asc abs`, `desc abs`, and further variants, and `expressions` validate through `table.validate_expressions`:

| [INDEX] | [FIELD]                                | [SHAPE]                                        | [ROLE]                                     |
| :-----: | :------------------------------------- | :--------------------------------------------- | :----------------------------------------- |
|  [01]   | `group_by` / `split_by`                | `string[]`                                     | row pivots / column pivots                 |
|  [02]   | `columns`                              | `(string \| null)[]`                           | projected columns                          |
|  [03]   | `aggregates`                           | `Record<string, string \| [string, string[]]>` | per-column aggregate; tuple for dependent  |
|  [04]   | `filter` + `filter_op`                 | `[column, operator, term][]` · `"and" \| "or"` | predicate rows                             |
|  [05]   | `sort`                                 | `[column, direction][]`                        | sort rows; column-axis + absolute variants |
|  [06]   | `expressions`                          | `Record<string, string>`                       | ExprTK computed columns                    |
|  [07]   | `group_by_depth` / `group_rollup_mode` | `number` · `"rollup" \| "flat" \| "total"`     | tree depth and rollup presentation         |

Aggregate roster (numeric): `sum` `abs sum` `sum not null` `avg` `mean` `count` `distinct count` `dominant` `first` `last` `last by index` `high` `low` `max` `min` `high minus low` `last minus first` `median` `q1` `q3` `pct sum parent` `pct sum total` `stddev` `var` `unique` `weighted mean` `min by` `max by`; string adds `join`, date/datetime and boolean carry the applicable subset.

[VIEW]: `View.to_arrow(ViewWindow?) -> Promise<ArrayBuffer>` `View.to_columns(unknown?) -> Promise<Record<string,unknown[]>>` `View.to_json(unknown?) -> Promise<Record<string,unknown>[]>` `View.to_csv(unknown?) -> Promise<string>` `View.to_ndjson(unknown?) -> Promise<string>` `View.on_update((updated:{port_id:number;delta?:ArrayBuffer})=>void,{mode?:"row"}?) -> Promise<number>` `View.schema() -> Promise<Schema>` `View.expression_schema() -> Promise<Schema>` `View.dimensions() -> Promise<{num_table_rows:number;num_table_columns:number;num_view_rows:number;num_view_columns:number}>` `View.get_min_max(string) -> Promise<[unknown,unknown]>` `View.collapse(number) -> Promise<number>` `View.expand(number) -> Promise<number>` `View.set_depth(number) -> Promise<void>` `View.on_delete(unknown) -> Promise<number>` `View.delete() -> Promise<void>`

## [03]-[HOST_TOPOLOGY]

- Browser default: `worker()` — engine in a dedicated/shared worker, UI thread free; `init_server(SERVER_WASM)` hosts the engine in-page/in-worker for the no-backend case.
- Node host: the node build's `WebSocketServer` (plus `make_session`/`make_client`) publishes named tables over `ws`; browser clients attach with `websocket(url)` → `open_table(name)` — the streaming dashboard wire: server ingests the feed into an indexed `Table`, every client's `View`s update incrementally.
- Virtual-server subpaths (`./virtual_servers/*`) plus the `VirtualServer`/`VirtualDataSlice`/`GenericSQLVirtualServerModel` exports carry the same session machinery over custom transports; `GenericSQLVirtualServerModel` fronts a SQL-backed source.
- `getCompiledClientWasm()` returns the structured-cloneable client WASM module a `postMessage` hands a spawned Worker, so the Worker instantiates its own `Client` with no re-fetch or recompile.

## [04]-[INTEGRATION]

[STACK: `apache-arrow` (`.api/apache-arrow.md`)] — one columnar format end to end: a wire Arrow frame feeds `client.table(buf, { format: "arrow", index })`, deltas append via `table.update(arrowBuf)`, `view.to_arrow()` hands results to `tableFromIPC` for any Arrow consumer (GeoArrow deck layers, Plot's Arrow-native marks) — never a JSON re-materialization between engines.

[STACK: the Effect rail + atom store (`.api/effect-atom-atom-react.md`)] — every handle is a scoped resource with an explicit `delete()`: client/table/view acquisition brackets in `Scope`-owned boundary adapters, `on_update`/`on_error` callbacks bridge into streams at the one async seam, and the `ViewConfigUpdate` a surface renders derives from an atom — config changes create a new view and delete the old, updates flow engine→view→viewer without touching React state.

[STACK: `@perspective-dev/viewer` (`.api/perspective-dev-viewer.md`)] — the viewer element consumes this client's `Table` via `load()` and drives its own `View` lifecycle; headless consumers (exports, alerts, derived feeds) hold `View`s directly. `@perspective-dev/react` binds react/react-dom as plain dependencies (not peers) — the element seam stays the admitted React integration.

[BOUNDARY: `view/table` `Grid` (`.api/tanstack-react-table.md`)] — the TanStack fold owns interactive accessible data grids over decoded row schemas at DOM scale; perspective owns streaming pivot/aggregation analytics at engine scale (millions of rows, live deltas, ExprTK columns). Decision axis is derivation locus: client-modeled rows → `Grid`; engine-maintained aggregates over a feed → perspective. One surface never runs both.

## [05]-[RAIL_LAW]

- Owns: the streaming analytics engine — Client/Table/View lifecycle, indexed/limited table modes, the `ViewConfigUpdate` query vocabulary, the aggregate roster, ExprTK expression columns, reactive `join`, Arrow ingress/egress/deltas, worker/websocket/in-browser-server host topologies, and the node `WebSocketServer` publisher.
- Accept: Arrow as the only inter-engine format; `index` upserts / `limit` ring buffers / `page_to_disk` spill chosen per feed; view configs as atom-derived values with create-new-delete-old rotation; `validate_expressions` before shipping an expression; `open_table` against host-published names; `Scope`-bracketed `delete()` on every handle; explicit WASM bootstrap per the bundler wiring.
- Reject: any `@finos/perspective` reference; JSON round-trips between Arrow-capable engines; a hand-maintained aggregate/join copy beside a live `View`/`join`; polling where `on_update` streams; engine work on the UI thread when `worker()` exists; leaked handles (missing `delete()`); perspective standing in for the `Grid` interactive-collection regime.

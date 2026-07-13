# [TS_UI_API_PERSPECTIVE_DEV_CLIENT]

[PACKAGE_SURFACE]:
- package: `@perspective-dev/client` · license `Apache-2.0` — the LIVE scope (`perspective-dev/perspective`); the `@finos/perspective` scope is dead and never cited.
- module: `sideEffects: false`; condition-split `exports["."]` — `node` → `dist/esm/perspective.node.js` (pre-instantiated synchronous singleton client + `WebSocketServer` host), default → `dist/esm/perspective.js` (browser, Worker/WASM bootstrapping); subpaths `./node`, `./virtual_servers/*` (`./inline` is deprecated-for-removal).
- asset: deps `@perspective-dev/server` (the WASM engine binary, lockstep), `ws` (node), `stoppable`; no peers. Browser WASM assets: the client's own `perspective-js.wasm` (auto-bootstrapped inside `worker()`), `@perspective-dev/server/dist/wasm/perspective-server.wasm` for in-browser/in-worker server hosting via `init_server(wasm)`.
- bundling: Vite imports each `.wasm` with `?url` + `fetch(url)` and requires `build.target: "esnext"`; the CDN `dist/cdn/*` build needs no bootstrap.
- runtime: `Client`/`Table`/`View` are one API across browser and node — the node build swaps Worker spawning for a required `MessagePort` and adds the server host.
- plane: `plane:runtime` (W4 `ui`); rail: streaming tabular analytics — the pivot/aggregation engine.

`@perspective-dev/client` is the millions-of-rows live-analytics engine: a WASM core holding `Table`s (indexed, appendable, streamable) from which `View`s materialize — each View one `ViewConfigUpdate` value of group/split/aggregate/filter/sort/expressions that the engine maintains INCREMENTALLY as updates land. The protocol is location-transparent: the same `Client` API fronts an in-page worker (`worker()`), a remote node host (`websocket(url)`), or an in-browser server (`init_server`), so where the data lives is a wiring decision, not an API fork. Arrow is the wire: tables ingest Arrow IPC buffers, `view.to_arrow()` emits them, and `on_update` row-mode deltas ARE Arrow buffers — the engine and the `apache-arrow` substrate speak one columnar format with no JSON detour.

## [01]-[CLIENT_AND_TABLE]

```ts signature
declare const perspective: {
  worker(worker?: Promise<SharedWorker | ServiceWorker | Worker | MessagePort>): Promise<Client>   // browser: no-arg spawns a dedicated Worker
  websocket(url: string): Promise<Client>                                                          // remote host — same Client API over the wire
}
interface Client {
  table(value: string | ArrayBuffer | Record<string, unknown[]> | Record<string, unknown>[] | Schema,
        options?: { name?: string; format?: "csv" | "json" | "columns" | "arrow" | "ndjson"; index?: string; limit?: number }): Promise<Table>
  open_table(entity_id: string): Promise<Table>                        // attach to a host-published table by name
  join(left: Table, right: Table, on: unknown, options?: { join_type?: string; name?: string }): Promise<Table>   // reactive live join
  get_hosted_table_names(): Promise<string[]>; on_hosted_tables_update(cb): Promise<number>; system_info(): Promise<SystemInfo>
  on_error(cb): Promise<number>; terminate(): void
}
interface Table {
  view(config?: ViewConfigUpdate): Promise<View>
  update(input: TableData, options?: { port_id?: number; format?: string }): Promise<void>          // Arrow ArrayBuffer is a first-class delta
  remove(value: unknown, options?): Promise<void>; replace(input: unknown, options?): Promise<void>; clear(): Promise<void>
  schema(): Promise<Schema>; columns(): Promise<string[]>; size(): Promise<number>
  validate_expressions(exprs: Record<string, string>): Promise<unknown>; make_port(): Promise<number>
  get_index(): Promise<string | null>; get_limit(): Promise<number | null>; get_name(): string; get_client(): Client
  delete(options?: { lazy?: boolean }): Promise<void>; on_delete(cb): Promise<number>
}
```

- `index` makes updates UPSERTS on the key column; `limit` caps rows ring-buffer style — the two table modes every streaming feed chooses between.
- `join` returns a LIVE table — updates on either side re-derive; a hand-maintained merged copy beside it is the defect.

## [02]-[VIEW_PROTOCOL]

The `ViewConfigUpdate` value is the whole query surface (current spellings — the retired `row_pivots`/`column_pivots` names are dead); `sort` directions span `asc`, `desc`, `col asc`, `col desc`, `asc abs`, `desc abs`, and further variants, and `expressions` validate through `table.validate_expressions`:

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

```ts signature
interface View {
  to_arrow(window?: ViewWindow): Promise<ArrayBuffer>                    // the columnar egress — feeds apache-arrow tableFromIPC zero-copy
  to_columns(window?): Promise<Record<string, unknown[]>>; to_json(window?): Promise<Record<string, unknown>[]>; to_csv(window?): Promise<string>; to_ndjson(window?): Promise<string>
  on_update(cb: (updated: { port_id: number; delta?: ArrayBuffer }) => void, options?: { mode?: "row" }): Promise<number>  // mode:"row" ⇒ delta IS an Arrow buffer of changed rows
  schema(): Promise<Schema>; expression_schema(): Promise<Schema>
  dimensions(): Promise<{ num_table_rows: number; num_table_columns: number; num_view_rows: number; num_view_columns: number }>
  get_min_max(name: string): Promise<[unknown, unknown]>; collapse(row: number): Promise<number>; expand(row: number): Promise<number>; set_depth(depth: number): Promise<void>
  on_delete(cb): Promise<number>; delete(): Promise<void>
}
```

## [03]-[HOST_TOPOLOGY]

- Browser default: `worker()` — engine in a dedicated/shared worker, UI thread free; `init_server(SERVER_WASM)` hosts the engine in-page/in-worker for the no-backend case.
- Node host: the node build's `WebSocketServer` (plus `make_session`/`make_client`) publishes named tables over `ws`; browser clients attach with `websocket(url)` → `open_table(name)` — the streaming dashboard wire: server ingests the feed into an indexed `Table`, every client's `View`s update incrementally.
- The virtual-server subpaths (`./virtual_servers/*`) carry the same session machinery over custom transports.

## [04]-[INTEGRATION]

[STACK: `apache-arrow` (`.api/apache-arrow.md`)] — one columnar format end to end: a wire Arrow frame feeds `client.table(buf, { format: "arrow", index })`, deltas append via `table.update(arrowBuf)`, `view.to_arrow()` hands results to `tableFromIPC` for any Arrow consumer (GeoArrow deck layers, Plot's Arrow-native marks) — never a JSON re-materialization between engines.

[STACK: the Effect rail + atom store (`.api/effect-atom-atom-react.md`)] — every handle is a scoped resource with an explicit `delete()`: client/table/view acquisition brackets in `Scope`-owned boundary adapters, `on_update`/`on_error` callbacks bridge into streams at the one async seam, and the `ViewConfigUpdate` a surface renders derives from an atom — config changes create a new view and delete the old, updates flow engine→view→viewer without touching React state.

[STACK: `@perspective-dev/viewer` (`.api/perspective-dev-viewer.md`)] — the viewer element consumes this client's `Table` via `load()` and drives its own `View` lifecycle; headless consumers (exports, alerts, derived feeds) hold `View`s directly. `@perspective-dev/react` binds react/react-dom as plain dependencies (not peers) — the element seam stays the admitted React integration.

[BOUNDARY: `view/table` `Grid` (`.api/tanstack-react-table.md`)] — the TanStack fold owns interactive accessible data grids over decoded row schemas at DOM scale; perspective owns streaming pivot/aggregation analytics at engine scale (millions of rows, live deltas, ExprTK columns). The decision axis is derivation locus: client-modeled rows → `Grid`; engine-maintained aggregates over a feed → perspective. One surface never runs both.

## [05]-[RAIL_LAW]

- Owns: the streaming analytics engine — Client/Table/View lifecycle, indexed/limited table modes, the `ViewConfigUpdate` query vocabulary, the aggregate roster, ExprTK expression columns, reactive `join`, Arrow ingress/egress/deltas, worker/websocket/in-browser-server host topologies, and the node `WebSocketServer` publisher.
- Accept: Arrow as the only inter-engine format; `index` upserts / `limit` ring buffers chosen per feed; view configs as atom-derived values with create-new-delete-old rotation; `validate_expressions` before shipping an expression; `open_table` against host-published names; `Scope`-bracketed `delete()` on every handle; explicit WASM bootstrap per the bundler wiring.
- Reject: any `@finos/perspective` reference; JSON round-trips between Arrow-capable engines; a hand-maintained aggregate/join copy beside a live `View`/`join`; polling where `on_update` streams; the deprecated `./inline` builds; engine work on the UI thread when `worker()` exists; leaked handles (missing `delete()`); perspective standing in for the `Grid` interactive-collection regime.

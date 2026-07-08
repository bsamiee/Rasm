# [TS_UI_API_PERSPECTIVE_DEV_VIEWER]

[PACKAGE_SURFACE]:
- package: `@perspective-dev/viewer` · license `Apache-2.0` — the LIVE scope; `@finos/*` is dead.
- module: `sideEffects: true` — importing the package REGISTERS `<perspective-viewer>` (the import is the API); exports `.`, `./plugin` (the plugin author contract), `./themes` + `./themes/*.css` + `./themes/intl/*.css` (`./inline` deprecated-for-removal).
- asset: deps `@perspective-dev/client` (lockstep); WASM `dist/wasm/perspective-viewer.wasm` bootstrapped via `init_client(wasm)` beside the client's `init_server` — the two-line boot the bundler wiring in `.api/perspective-dev-client.md` carries.
- plugins: `@perspective-dev/viewer-datagrid` (regular-table grid) and `@perspective-dev/viewer-charts` (WebGL charts, the current default chart plugin) release lockstep; `viewer-d3fc` and `viewer-openlayers` trail a minor and are superseded by `viewer-charts` for new admission. Each registers by import side effect through the `./plugin` contract.
- runtime: framework-agnostic custom element; the React seam is a ref on the element — `@perspective-dev/react` exists but binds react as plain dependencies, so the element seam stays the admitted integration.
- plane: `plane:runtime` (W4 `ui`); rail: the pivot-analytics surface over the perspective engine.

`<perspective-viewer>` is the interactive face of the engine: it consumes a client `Table`, owns its own `View` lifecycle, and exposes EVERYTHING as one serializable config value — `save()` emits the `ViewerConfig` (plugin, plugin_config, theme, settings, title, plus the full `ViewConfigUpdate` query fields), `restore(update)` applies any subset. That one round-trippable value is the whole state law: persistence, presets, cross-session restore, and programmatic control are all `restore` of a decoded config, never attribute pokes. Rendering is delegated to a registered plugin element (datagrid, charts) selected by the config's `plugin` field — the viewer is the query/config chrome, the plugin is the paint.

## [01]-[ELEMENT_SURFACE]

```ts contract
interface PerspectiveViewerElement extends HTMLElement {
  load(table: Client | Table | Promise<Client | Table>): Promise<void>   // a Table auto-renders; a bare Client only enables restore({ table: name }) lookup
  restore(update: ViewerConfigUpdate): Promise<void>
  save(): Promise<ViewerConfig>                                          // { version, plugin, plugin_config, settings, theme, title, columns_config, table, group_by, split_by, columns, aggregates, sort, filter, filter_op, expressions, group_by_depth, group_rollup_mode }
  reset(reset_all?: boolean): Promise<void>; flush(): Promise<void>
  getTable(wait_for_table?: boolean): Promise<Table>; getView(): Promise<View>; getViewConfig(): Promise<ViewConfigUpdate>; getClient(wait_for_client?: boolean): Promise<Client>
  toggleConfig(force?: boolean): Promise<void>                           // THE settings-panel toggle; openColumnSettings/toggleColumnSettings drive the per-column drawer
  getSelection(): unknown; setSelection(window?: unknown): Promise<void>; getEditPort(): number
  getAllPlugins(): HTMLElement[]; getPlugin(name?: string): Promise<HTMLElement>; getRenderStats(): unknown
  resetThemes(themes?: string[]): Promise<void>; restyleElement(): Promise<void>; resize(options?: unknown): Promise<void>
  setAutoSize(v: boolean): void; setAutoPause(v: boolean): void; setThrottle(ms?: number): void
  copy(method?: unknown): Promise<void>; download(method?: unknown): Promise<void>; export(method?: unknown): Promise<unknown>
  eject(): Promise<void>; delete(): Promise<void>                        // frees viewer View/WASM state — never deletes the supplied Table
  resetError(): Promise<void>
}
```

Events (typed on the element): `perspective-config-update` (the state-echo seam — fires with the new config on user-driven change), `perspective-click` (`{ config, row, column_names }`), `perspective-select` (a `ViewWindow`), `perspective-global-filter`, `perspective-toggle-settings`, `perspective-table-delete`. Theme is a CONFIG FIELD (`restore({ theme })` from the `./themes/*.css` roster via `resetThemes`), not an HTML attribute.

## [02]-[PLUGIN_ROSTER]

- Plugin selection is the config's `plugin` field plus `plugin_config` (plugin-owned settings — datagrid edit mode, chart axes); both round-trip through `save`/`restore`.
- `viewer-datagrid`: the regular-table grid — virtual scrolling, tree pivoted rows, editing through the edit port.
- `viewer-charts`: the WebGL chart family — the default chart plugin at the current line; per-chart d3fc subpaths belong to the trailing `viewer-d3fc` and are not admitted beside it.
- Authoring: `./plugin` exports the `IPerspectiveViewerPlugin`/`HTMLPerspectiveViewerPluginElement` contract (`get_static_config`, `draw`, `update`, `clear`, `resize`, `restyle`, `restore`, `delete`) — a custom visualization is one element implementing it and registering, never a fork of the viewer.

## [03]-[INTEGRATION]

[STACK: `@perspective-dev/client` (`.api/perspective-dev-client.md`)] — `load()` takes the client's `Table` (local worker or `websocket`+`open_table` remote — the viewer is host-agnostic); engine deltas landing on the table repaint the viewer incrementally with zero consumer code. `delete()` on teardown releases viewer state while the Table's own scoped lifecycle stays with its owner.

[STACK: the atom store (`.api/effect-atom-atom-react.md`)] — the `ViewerConfig` is the ONE state value: an atom (backed by `Atom.kvs` + its schema for persistence) holds it, a `perspective-config-update` listener writes user-driven changes back through the atom, and atom-driven changes apply via `restore` — the same fold-echo law `view/table`'s `Grid` follows for TanStack state. Config presets are decoded values, never DOM scraping.

[STACK: the React seam (`.api/react.md`)] — a component renders `<perspective-viewer>` via ref, runs `load` in the effect bracket, `delete()` on cleanup; React never reaches inside the element. The custom element is the boundary — props do not flow, config does.

[STACK: `system/token` theming] — the `./themes/*.css` stylesheets import once through the token stylesheet; the viewer restyles via CSS custom properties (`restyleElement` after a live property flip), so a theme change is the token authority's flip plus one `restore({ theme })`/`restyleElement`, never per-instance CSS.

[BOUNDARY: the grid/chart siblings] — the viewer earns a surface when the USER drives the query (pivot/aggregate/filter/expression exploration over a live feed). A fixed-shape interactive grid is `view/table` `Grid` (`.api/tanstack-react-table.md`); a declared statistical chart is Plot (`.api/observablehq-plot.md`); a streaming time-series panel is uplot (`.api/uplot.md`). One surface, one engine.

## [04]-[RAIL_LAW]

- Owns: the interactive analytics element — registration-by-import, `load`/`save`/`restore` config round-trip, the settings/column chrome, selection/click/config events, plugin selection and the plugin author contract, theme roster application, and export/copy/download egress.
- Accept: the config value as the one state law (atom-held, schema-decoded, `restore`-applied); `perspective-config-update` as the echo seam; `Table`-level streaming with the viewer as a passive consumer; `viewer-datagrid` + `viewer-charts` as the admitted plugin pair; ref + effect-bracket mounting with `delete()` teardown; themes through the token stylesheet.
- Reject: any `@finos/*` reference; attribute/DOM pokes where `restore` carries the change; per-viewer state stores beside the config atom; `viewer-d3fc`/`viewer-openlayers` admitted beside `viewer-charts`; React wrappers reaching inside the element; the deprecated `./inline` build; the viewer standing in for `Grid`-regime fixed grids or Plot-regime declared charts.

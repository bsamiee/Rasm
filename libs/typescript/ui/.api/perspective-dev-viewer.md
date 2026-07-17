# [TS_UI_API_PERSPECTIVE_DEV_VIEWER]

[PACKAGE_SURFACE]:
- package: `@perspective-dev/viewer` · license `Apache-2.0` — the LIVE scope; `@finos/*` is dead.
- module: `sideEffects: true` — importing REGISTERS `<perspective-viewer>`, and the main entry also exports the WASM boot (`init_client`, also the default export), the plugin author contract (`IPerspectiveViewerPlugin`/`HTMLPerspectiveViewerPluginElement`), the cross-plugin column-format helpers, `PerspectiveSelectDetail`, and the full config-type surface (`ViewerConfig`/`ViewerConfigUpdate`/`Filter`/`Sort`/`Aggregate`).
- subpaths: `.`, `./inline` (self-contained WASM-inlined build variant), `./themes` (bundled roster), `./themes/*.css`, `./themes/intl/*.css`.
- asset: deps `@perspective-dev/client` (lockstep) + `pro_self_extracting_wasm`; WASM `dist/wasm/perspective-viewer.wasm` boots via `init_client(wasm_binary, wasm_module?)` beside the client's `init_server` — the two-line boot the bundler wiring in `.api/perspective-dev-client.md` carries.
- plugins: `@perspective-dev/viewer-datagrid` (regular-table grid) and `@perspective-dev/viewer-charts` (WebGL charts, the current default chart plugin) release lockstep; `viewer-d3fc` and `viewer-openlayers` trail a minor and are superseded by `viewer-charts` for new admission. Each registers by import side effect against the viewer's plugin registry.
- runtime: framework-agnostic custom element; the React seam is a ref on the element — `@perspective-dev/react` exists but binds react as plain dependencies, so the element seam stays the admitted integration.
- plane: `plane:runtime` (W4 `ui`); rail: the pivot-analytics surface over the perspective engine.

`<perspective-viewer>` is the interactive face of the engine: it consumes a client `Table`, owns its own `View` lifecycle, and exposes EVERYTHING as one serializable config value — `save()` emits the `ViewerConfig` (plugin, plugin_config, theme, settings, title, plus the full `ViewConfigUpdate` query fields), `restore(update)` applies any subset. That one round-trippable value is the whole state law: persistence, presets, cross-session restore, and programmatic control are all `restore` of a decoded config, never attribute pokes. Rendering is delegated to a registered plugin element (datagrid, charts) selected by the config's `plugin` field — the viewer is the query/config chrome, the plugin is the paint.

## [01]-[ELEMENT_SURFACE]

```ts signature
interface HTMLPerspectiveViewerElement extends HTMLElement {
  load(table: Client | Table | Promise<Client | Table>): Promise<void>   // a Table auto-renders; a bare Client only enables restore({ table: name }) lookup
  restore(update: ViewerConfigUpdate): Promise<void>
  save(): Promise<ViewerConfig>                                          // plugin, plugin_config, settings, theme, title, columns_config, table, plus the full ViewConfigUpdate query fields
  reset(reset_all?: boolean): Promise<void>; flush(): Promise<void>
  getTable(wait_for_table?: boolean): Promise<Table>; getView(): Promise<View>; getViewConfig(): Promise<ViewConfigUpdate>; getClient(wait_for_client?: boolean): Promise<Client>
  toggleConfig(force?: boolean): Promise<void>                           // settings-panel toggle; openColumnSettings(column_name?, toggle?)/toggleColumnSettings(column_name) drive the per-column drawer
  getSelection(): ViewWindow | undefined; setSelection(window?: ViewWindow): void; getEditPort(): number
  getAllPlugins(): HTMLElement[]; getPlugin(name?: string): HTMLElement; getRenderStats(): unknown; registerPlugin(name: string): Promise<void>
  resetThemes(themes?: string[]): Promise<void>; restyleElement(): Promise<void>; resize(options?: unknown): Promise<void>
  setAutoSize(v: boolean): void; setAutoPause(v: boolean): Promise<void>; setThrottle(ms?: number): void
  copy(method?: string): Promise<void>; download(method?: string): Promise<void>; export(method?: string): Promise<unknown>
  eject(): Promise<void>; delete(): Promise<void>                        // frees viewer View/WASM state — never deletes the supplied Table
  resetError(): Promise<void>
}
```

Events (overloaded `addEventListener` on the element): `perspective-config-update` (state-echo seam, fires with the new config on user-driven change), `perspective-click` (`PerspectiveClickEventDetail` — `{ config, row, column_names }`), `perspective-select` and `perspective-global-filter` (`PerspectiveSelectEventDetail` — `{ view_window }`), `perspective-toggle-settings`, `perspective-statusbar-pointerdown`, `perspective-table-delete`, each destructive seam carrying a `-before` pre-dispatch twin (`perspective-toggle-settings-before`, `perspective-table-delete-before`). Theme is a CONFIG FIELD (`restore({ theme })` from the `./themes/*.css` roster via `resetThemes`), never an HTML attribute.

Column formatting is one cross-plugin law: `createNumberFormatter`/`createDatetimeFormatter`/`createDateFormatter` build `Intl` formatters from a column's `NumberFormatConfig`/`DateFormatConfig` (mirroring `Intl.NumberFormatOptions`/`Intl.DateTimeFormatOptions` one-for-one), and `sourceColumn(path)` recovers the source column from a synthetic split-by path — keyed into `columns_config` on the config value so datagrid cells and chart axes/tooltips/legends render identically.

## [02]-[PLUGIN_ROSTER]

- Plugin selection is the config's `plugin` field plus `plugin_config` (plugin-owned settings — datagrid edit mode, chart axes); both round-trip through `save`/`restore`.
- `viewer-datagrid`: regular-table grid — virtual scrolling, tree-pivoted rows, editing through the edit port.
- `viewer-charts`: WebGL chart family — default chart plugin; per-chart d3fc subpaths belong to the trailing `viewer-d3fc`, not admitted beside it.
- Default selection follows `PluginStaticConfig.priority` (highest wins) until `restore({ plugin })` overrides; `can_render_column_styles` gates the per-column StyleTab.
- Authoring: main entry exports the `IPerspectiveViewerPlugin`/`HTMLPerspectiveViewerPluginElement` contract (`get_static_config`, `column_style_config`, `draw`, `update`, `clear`, `resize`, `restyle`, `restore`, `delete`) — a custom visualization extends `HTMLPerspectiveViewerPluginElement` (the default `<perspective-viewer-plugin>` base) and registers via `registerPlugin(name)`, never a fork of the viewer.

## [03]-[INTEGRATION]

[STACK: `@perspective-dev/client` (`.api/perspective-dev-client.md`)] — `load()` takes the client's `Table` (local worker or `websocket`+`open_table` remote — the viewer is host-agnostic); engine deltas landing on the table repaint the viewer incrementally with zero consumer code. `delete()` on teardown releases viewer state while the Table's own scoped lifecycle stays with its owner.

[STACK: the atom store (`.api/effect-atom-atom-react.md`)] — the `ViewerConfig` is the ONE state value: an atom (backed by `Atom.kvs` + its schema for persistence) holds it, a `perspective-config-update` listener writes user-driven changes back through the atom, and atom-driven changes apply via `restore` — the same fold-echo law `view/table`'s `Grid` follows for TanStack state. Config presets are decoded values, never DOM scraping.

[STACK: the React seam (`.api/react.md`)] — a component renders `<perspective-viewer>` via ref, runs `load` in the effect bracket, `delete()` on cleanup; React never reaches inside the element. Custom element is the boundary — props do not flow, config does.

[STACK: `system/token` theming] — the `./themes/*.css` stylesheets import once through the token stylesheet; the viewer restyles via CSS custom properties (`restyleElement` after a live property flip), so a theme change is the token authority's flip plus one `restore({ theme })`/`restyleElement`, never per-instance CSS.

[BOUNDARY: the grid/chart siblings] — the viewer earns a surface when the USER drives the query (pivot/aggregate/filter/expression exploration over a live feed). A fixed-shape interactive grid is `view/table` `Grid` (`.api/tanstack-react-table.md`); a declared statistical chart is Plot (`.api/observablehq-plot.md`); a streaming time-series panel is uplot (`.api/uplot.md`). One surface, one engine.

## [04]-[RAIL_LAW]

- Owns: the interactive analytics element — registration-by-import, `load`/`save`/`restore` config round-trip, the settings/column chrome, selection/click/config events, plugin selection and the plugin author contract, theme roster application, and export/copy/download egress.
- Accept: the config value as the one state law (atom-held, schema-decoded, `restore`-applied); `perspective-config-update` as the echo seam; `Table`-level streaming with the viewer as a passive consumer; `viewer-datagrid` + `viewer-charts` as the admitted plugin pair; ref + effect-bracket mounting with `delete()` teardown; themes through the token stylesheet.
- Reject: any `@finos/*` reference; attribute/DOM pokes where `restore` carries the change; per-viewer state stores beside the config atom; `viewer-d3fc`/`viewer-openlayers` admitted beside `viewer-charts`; React wrappers reaching inside the element; the deprecated `./inline` build; the viewer standing in for `Grid`-regime fixed grids or Plot-regime declared charts.

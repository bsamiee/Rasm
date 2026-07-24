# [TS_UI_API_PERSPECTIVE_DEV_VIEWER]

`<perspective-viewer>` is the interactive face of the `@perspective-dev/client` engine: it consumes a client `Table`, drives its own `View` lifecycle, and folds all state into one round-trippable config value `save()` emits and `restore()` applies. Rendering delegates to the registered plugin element the config's `plugin` field selects — the viewer owns the query and config chrome, the plugin owns the paint.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer`
- package: `@perspective-dev/viewer` (Apache-2.0)
- module: `sideEffects: true` — import REGISTERS `<perspective-viewer>` and exports the WASM boot (`init_client`, also the default), the plugin author contract, the cross-plugin column formatters, and the config-type surface
- subpaths: `.`, `./inline` (WASM-inlined variant), `./themes` + `./themes/*.css` + `./themes/intl/*.css` (bundled theme roster)
- asset: deps `@perspective-dev/client` (lockstep) + `pro_self_extracting_wasm`; WASM `dist/wasm/perspective-viewer.wasm` boots via `init_client(wasm_binary, wasm_module?)` beside the client's `init_server`
- plugins: `@perspective-dev/viewer-datagrid` and `@perspective-dev/viewer-charts` register lockstep by import side effect against the viewer's plugin registry
- runtime: framework-agnostic custom element; the React seam is a ref on the element, so the element seam stays the admitted integration; no peers
- plane: `plane:runtime` (W4 `ui`); rail: pivot-analytics over the perspective engine

## [02]-[ELEMENT_SURFACE]

Every method is async, awaiting the wasm instance; all state I/O runs through `save`/`restore`, never attribute pokes.

[CONFIG]: `load(Client|Table|Promise<Client|Table>) -> Promise<void>` `save() -> Promise<ViewerConfig>` `restore(ViewerConfigUpdate) -> Promise<void>` `reset(boolean?) -> Promise<void>` `flush() -> Promise<void>`
[ACCESS]: `getTable(boolean?) -> Promise<Table>` `getView() -> Promise<View>` `getViewConfig() -> Promise<ViewConfigUpdate>` `getClient(boolean?) -> Promise<Client>` `getEditPort() -> number` `getRenderStats()` `getSelection() -> ViewWindow|undefined` `setSelection(ViewWindow?)`
[CHROME]: `toggleConfig(boolean?) -> Promise<void>` `resize(unknown?) -> Promise<void>` `setAutoSize(boolean)` `setAutoPause(boolean) -> Promise<void>` `setThrottle(number?)` `resetThemes(string[]?) -> Promise<void>` `restyleElement() -> Promise<void>`
[PLUGINS]: `registerPlugin(string) -> Promise<void>` `getAllPlugins() -> HTMLElement[]` `getPlugin(string?) -> HTMLElement`
[EGRESS]: `copy(string?) -> Promise<void>` `download(string?) -> Promise<void>` `export(string?) -> Promise<unknown>` `eject() -> Promise<void>` `delete() -> Promise<void>` `resetError() -> Promise<void>`
[MODULE]: `init_client(wasm_binary, wasm_module?) -> Promise<void>` `createNumberFormatter(type, cfg?) -> Intl.NumberFormat` `createDatetimeFormatter(cfg?) -> Intl.DateTimeFormat` `createDateFormatter(cfg?) -> Intl.DateTimeFormat` `sourceColumn(path) -> string`
[EVENTS]: `perspective-config-update` `perspective-click` `perspective-select` `perspective-global-filter` `perspective-toggle-settings` `perspective-statusbar-pointerdown` `perspective-table-delete`

- `perspective-config-update` echoes the new config on every user-driven change; each destructive event carries a `-before` pre-dispatch twin, and theme is a config field (`restore({ theme })` from `./themes/*.css`), never an HTML attribute.
- Column formatting is one cross-plugin law: `createNumberFormatter` / `createDatetimeFormatter` / `createDateFormatter` build `Intl` formatters from a column's `NumberFormatConfig` / `DateFormatConfig` keyed into `columns_config`, so datagrid cells and chart axes render identically; `sourceColumn` recovers the source column from a split-by path.

## [03]-[PLUGIN_ROSTER]

Plugin selection is the config's `plugin` field with `plugin_config`; both round-trip through `save`/`restore`.

- `@perspective-dev/viewer-datagrid`: regular-table grid — virtual scrolling, tree-pivoted rows, editing through the edit port.
- `@perspective-dev/viewer-charts`: WebGL chart family — the default chart plugin.
- Default selection follows `PluginStaticConfig.priority` (highest wins) until `restore({ plugin })` overrides; `can_render_column_styles` gates the per-column StyleTab.
- Authoring: a custom visualization extends `HTMLPerspectiveViewerPluginElement` (the `<perspective-viewer-plugin>` base implementing `IPerspectiveViewerPlugin` — `get_static_config`, `draw`, `update`, `clear`, `resize`, `restyle`, `restore`, `delete`) and registers via `registerPlugin(name)`, never a fork of the viewer.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ViewerConfig` is the one state value — `plugin`, `plugin_config`, `theme`, `settings`, `title`, `columns_config`, and the `ViewConfigUpdate` query fields; `save()` emits it whole and `restore(update)` applies any subset, so persistence, presets, and programmatic control are all `restore` of a decoded config.
- Rendering delegates to the plugin the `plugin` field selects; the viewer is the query and config chrome, the plugin is the paint.

[STACKING]:
- `@perspective-dev/client`(`.api/perspective-dev-client.md`): `load()` takes the client `Table` (worker-local or `websocket` + `open_table` remote); engine deltas on the table repaint the viewer incrementally with zero consumer code, and `delete()` releases viewer state while the `Table` lifecycle stays with its owner.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): the `ViewerConfig` is the one atom value (`Atom.kvs`-backed for persistence); a `perspective-config-update` listener writes user changes back and atom changes apply via `restore` — the fold-echo law `view/table`'s `Grid` follows, presets decoded values never DOM scraping.
- `react`(`.api/react.md`): a component renders `<perspective-viewer>` via ref, runs `load` in the effect bracket and `delete()` on cleanup; config flows across the custom-element boundary, props do not.
- `system/token` theming: `./themes/*.css` import once through the token stylesheet and the viewer restyles via CSS custom properties, so a theme change is the token flip and one `restore({ theme })` / `restyleElement`, never per-instance CSS.
- `view/table` composition: the viewer earns a surface only when the USER drives the query — pivot, aggregate, filter, or expression exploration over a live feed; a fixed-shape interactive grid stays `Grid` (`.api/tanstack-react-table.md`), a declared statistical chart stays Plot (`.api/observablehq-plot.md`), a streaming time-series panel stays uplot (`.api/uplot.md`).

[RAIL_LAW]:
- Package: `@perspective-dev/viewer`
- Owns: the interactive analytics element — registration-by-import, the `load`/`save`/`restore` config round-trip, the settings and column chrome, selection/click/config events, plugin selection and the plugin author contract, theme roster application, and export/copy/download egress.
- Accept: the config value as the one state law (atom-held, schema-decoded, `restore`-applied); `perspective-config-update` as the echo seam; `Table`-level streaming with the viewer a passive consumer; `viewer-datagrid` + `viewer-charts` as the admitted plugin pair; ref + effect-bracket mounting with `delete()` teardown; themes through the token stylesheet.
- Reject: any `@finos/*` reference; attribute or DOM pokes where `restore` carries the change; a per-viewer state store beside the config atom; `viewer-d3fc` or `viewer-openlayers` beside `viewer-charts`; a React wrapper reaching inside the element; the viewer standing in for a `Grid` fixed grid or a Plot declared chart.

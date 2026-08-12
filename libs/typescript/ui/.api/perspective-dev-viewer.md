# [TS_UI_API_PERSPECTIVE_DEV_VIEWER]

`<perspective-viewer>` is the interactive face of the `@perspective-dev/client` engine: it consumes a client `Table`, drives its own `View` lifecycle, and folds all state into round-trippable config values `save`/`restore` carry per panel and `saveWorkspace`/`restoreWorkspace` carry for the whole element. Rendering delegates to the registered plugin element the config's `plugin` field selects — the viewer owns the query, panel, and config chrome, the plugin owns the paint.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer`
- package: `@perspective-dev/viewer` (Apache-2.0)
- module: `sideEffects: true` — import REGISTERS `<perspective-viewer>` and exports the WASM boot (`init_client`, also the default), the plugin author contract, the cross-plugin column formatters, the LLM provider presets, and the config-type surface
- subpaths: `.`, `./themes` + `./themes/*.css` + `./themes/intl/*.css` (bundled theme roster)
- asset: deps `@perspective-dev/client` (lockstep), `pro_self_extracting_wasm`, and `regular-layout` (the panel layout engine); WASM `dist/wasm/perspective-viewer.wasm` boots via `init_client(wasm_binary, wasm_module?)` beside the client's `init_server`
- plugins: `@perspective-dev/viewer-datagrid` and `@perspective-dev/viewer-charts` register lockstep by import side effect against the viewer's plugin registry
- runtime: framework-agnostic custom element; the React seam is a ref on the element, so the element seam stays the admitted integration; no peers
- plane: `plane:runtime` (W4 `ui`); rail: pivot-analytics over the perspective engine

## [02]-[ELEMENT_SURFACE]

Every method is async, awaiting the wasm instance; all state I/O runs through the config round trip, never attribute pokes. Each panel-scoped method takes its target as an options field and defaults to the ACTIVE panel.

[BINDING]: `load(Client|Table|Promise<Client|Table>) -> Promise<any>` `eject(ClientOptions?) -> Promise<any>` `delete() -> Promise<any>` `getClient(GetClientOptions?) -> Promise<any>` `getTable(GetTableOptions?) -> Promise<any>` `getView(PanelOptions?) -> Promise<any>` `getViewConfig(PanelOptions?) -> Promise<any>`
[CONFIG]: `save(PanelOptions?) -> Promise<ViewerConfig>` `restore(ViewerConfigUpdate,RestoreOptions?) -> Promise<void>` `saveWorkspace() -> Promise<WorkspaceConfig>` `restoreWorkspace(WorkspaceConfigUpdate) -> Promise<void>` `reset(boolean?,PanelOptions?) -> Promise<any>` `resetError() -> Promise<any>` `flush() -> Promise<any>`
[PANELS]: `addPanel(ViewerConfigInitial) -> Promise<any>` `removePanel(string) -> Promise<any>` `getPanelNames() -> Array<any>` `getActivePanel() -> any` `setActivePanel(string) -> Promise<any>`

- `addPanel` resolves to the GENERATED panel id and rejects a table-less config before the layout is touched; `removePanel` disposes the panel's engines and resolves after teardown, except against the LAST remaining panel, which resolves as a no-op. `getPanelNames` answers insertion order and `getActivePanel` answers `null` at zero panels — both SYNCHRONOUS, so a config-update handler can read the active id in its own dispatch turn. `ViewerConfigInitial` is unexported, so a consumer reads it off `addPanel`'s parameter.
[EGRESS]: `export(ExportOptions?) -> Promise<any>` `download(ExportOptions?) -> Promise<any>` `copy(ExportOptions?) -> Promise<any>` `getSelection(PanelOptions?) -> ViewWindow|undefined` `setSelection(ViewWindow?,PanelOptions?) -> void` `getEditPort(PanelOptions?) -> number` `getRenderStats(PanelOptions?) -> any`
[CHROME]: `toggleConfig(boolean?) -> Promise<any>` `toggleColumnSettings(string,PanelOptions?) -> Promise<any>` `resize(unknown?) -> Promise<any>` `setAutoSize(boolean) -> void` `setAutoPause(boolean) -> Promise<any>` `setThrottle(number?) -> void` `resetThemes(any[]?) -> Promise<any>` `restyleElement() -> Promise<any>`
[PLUGINS]: `registerPlugin(string) -> Promise<void>` `getAllPlugins() -> Array<any>` `getPlugin(string?) -> any`
[AGENT]: `agentConfig(unknown) -> void` `agentPrompt(string) -> Promise<any>` `agentReset() -> Promise<any>`
[MODULE]: `init_client(wasm_binary,wasm_module?) -> Promise<void>` `createNumberFormatter(string,NumberFormatConfig?) -> Intl.NumberFormat` `createDatetimeFormatter(DateFormatConfig?) -> Intl.DateTimeFormat` `createDateFormatter(DateFormatConfig?) -> Intl.DateTimeFormat` `sourceColumn(string) -> string` `providers`
[EVENTS]: `perspective-config-update` `perspective-click` `perspective-select` `perspective-global-filter` `perspective-global-filter-update` `perspective-layout-update` `perspective-active-panel-update` `perspective-agent-tool` `perspective-toggle-settings` `perspective-statusbar-pointerdown` `perspective-table-delete`

- `perspective-config-update` carries `getConfig()` rather than the config itself, deferred so a change no listener reads costs nothing; the backing closure releases immediately after dispatch, so the call is valid ONLY synchronously inside the handler and a stashed `getConfig` throws — read config outside a handler through `save()` / `saveWorkspace()`. `getConfig` answers a `ViewerConfigUpdate` — the PANEL grain of the panel that changed, never the workspace — so a workspace-grained holder pairs it with the synchronous `getActivePanel()` in the same turn.
- `perspective-select` and `perspective-global-filter` carry a `PerspectiveSelectDetail` whose `removeConfigs` apply BEFORE its `insertConfigs`, the pair that lets a selection clear a filter on a column outside the source's own pivots; `removeFilters` / `insertFilters` flatten each side to `Filter[]`.
- `perspective-toggle-settings` and `perspective-table-delete` each carry a `-before` pre-dispatch twin; `perspective-layout-update` fires on panel add and remove alone, so a divider drag or tab reorder is read back through `saveWorkspace()`.
- Column formatting is one cross-plugin law: `createNumberFormatter` / `createDatetimeFormatter` / `createDateFormatter` build `Intl` formatters from a column's `NumberFormatConfig` / `DateFormatConfig` keyed into `columns_config`, so datagrid cells and chart axes render identically; `sourceColumn` recovers the source column from a split-by path.
- `ExportMethod` selects the egress body: `csv` `json` `ndjson` `arrow` each with `-all` and `-selected` variants, alongside `html`, `plugin`, and `json-config`.
- `providers` carries spreadable `AgentProviderPreset` connection records (`anthropic`, `gemini`, `openai`, `openrouter`, `lmstudio`, `ollama`) for `agentConfig`; the agent core speaks one OpenAI-chat-completions protocol over primitive `url` / `headers` / `apiKey` fields, so any compatible service needs no preset. `agentConfig` alone reveals the chat surface and permits its first request.

## [03]-[PANEL_TOPOLOGY]

`<perspective-viewer>` hosts one or more named panels over the `regular-layout` engine, each binding a `Table` and holding its own plugin, columns, and query config.

- `ViewerConfig` is one panel's state (`plugin`, `plugin_config`, `theme`, `settings`, `title`, `table`, `columns_config`, `version`, and the `ViewConfigUpdate` query fields); `save()` emits it and `restore(update)` applies any subset to the target panel.
- `WorkspaceConfig` is the whole element (`version`, `active`, `layout`, `panels`, `global_filters`, `masters`); `saveWorkspace()` emits it with `panels` a `BTreeMap` in sorted key order so consecutive saves stay byte-stable, and `restoreWorkspace(update)` rebuilds the element with every `panels` entry a NEW panel — hence `ViewerConfigInitial`, where `table` is required by type and an absent field means default rather than unchanged.
- `WorkspaceConfig.layout` types `Layout | null` while `WorkspaceConfigUpdate.layout` is absent-or-present — the one field where the save and restore shapes differ — so an unlaid element's emitted `null` folds away rather than crossing back. Restoring also EJECTS every pre-existing panel and REMAPS the saved layout's panel ids onto the newly minted ones, so a `tab-layout` node's `tabs` entries are a save-local naming, never a durable handle.
- `restore({ panel })` naming no existing panel UPSERTS it rather than failing; the argument is a PATCH, so a table-less upsert yields a DEFERRED panel the next `load()` binds — it renders, and `save()` rejects with "Panel has no `table`" until the binding lands. `restore` handed a `WorkspaceConfig` token drops its `panels` and `layout` keys silently, while `restoreWorkspace` handed a panel token REJECTS because `panels` is non-defaulted — the asymmetry, not a symmetry, is what keeps the two formats from substituting.
- `RestoreOptions.suppress_errors` rejects the returned promise without committing the fault to the viewer's visible error state, keeping a programmatic caller's session usable; the view config rolls back to its pre-call value so a rejected patch cannot re-merge into a later restore, but element-level state the call already applied (theme, title, a plugin swap) is NOT undone — recovery for those is a restore of a known-good config.
- Master/detail: a master panel's selection contributes clauses to the element-level `global_filters`, applied as a TRANSIENT overlay on every detail panel and never written into their saved configs. Restored `global_filters` are one unattributed bucket the next master selection replaces, a `masters` id absent from `panels` warns and drops, and a restored master re-enters its row-tree selection edit mode. NEITHER field has a JS setter: `saveWorkspace`/`restoreWorkspace` and master-panel interaction are the only writes, so a cross-filter kept beside the workspace token is a second owner of state the overlay already holds.
- `eject({ client })` releases a loaded client by name and disposes every panel bound to it.

## [04]-[PLUGIN_ROSTER]

Plugin selection is the config's `plugin` field with `plugin_config`; both round-trip through `save`/`restore`.

- `@perspective-dev/viewer-datagrid`: regular-table grid — virtual scrolling, tree-pivoted rows, rollup column groups, editing through the edit port.
- `@perspective-dev/viewer-charts`: WebGL chart family — series, cartesian, hierarchical, financial, and map charts.
- Default selection follows `PluginStaticConfig.priority` (highest wins, ties by registration order) until `restore({ plugin })` overrides; `can_render_column_styles` gates the per-column StyleTab.
- `PluginStaticConfig` declares what the config MEANS for a plugin: `config_column_names` names the positional `columns` slots, `group_by_role` / `split_by_role` say what those pivots draw, `group_rollup_modes` / `split_rollup_modes` list accepted rollups in preference order, `max_columns` / `max_cells` set the soft render warning, `select_mode` and `min_config_columns` drive drag-and-drop, and `connects_row_order` marks a plugin whose drawing exposes row order.
- Authoring: a custom visualization extends `HTMLPerspectiveViewerPluginElement` (the `<perspective-viewer-plugin>` base implementing `IPerspectiveViewerPlugin` — `get_static_config`, `draw`, `update`, `clear`, `resize`, `restyle`, `restore`, `delete`, with optional `column_style_config` and `deselect`) and registers via `registerPlugin(name)`, never a fork of the viewer. Registering a name already defined warns and skips rather than throwing, so two runtime copies coexist on one page.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Config is the one state law at two grains — panel through `save`/`restore`, element through `saveWorkspace`/`restoreWorkspace` — so persistence, presets, and programmatic control are all a restore of a decoded config. Multi-panel elements persist at the ELEMENT grain, because that is the only token carrying layout, active panel, and cross-filter state; the panel grain is a patch against it, never its substitute.
- Rendering delegates to the plugin the `plugin` field selects; the viewer is the query, panel, and config chrome, the plugin is the paint.
- Each plugin method fires for exactly one reason and never defensively: `draw` only for a `View` new to the plugin (reset zoom and selection state), `update` for changed data or config against the same `View`, `resize` for geometry or visibility alone, `restyle` immediately before the render that paints the new theme, and `restore` for state transfer that must neither render nor re-enter the host's public surface. Rendering methods and `delete` serialize per element.

[STACKING]:
- `@perspective-dev/client`(`.api/perspective-dev-client.md`): `load()` takes the client `Table` (worker-local or `websocket` + `open_table` remote); engine deltas on the table repaint every bound panel incrementally with zero consumer code, `getEditPort()` carries cell edits back to the `Table`, and `delete()` releases viewer state while the `Table` lifecycle stays with its owner.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): the config is the one atom value (`Atom.kvs`-backed for persistence, a `Schema` its codec); a multi-panel element persists at the WORKSPACE grain, so a `perspective-config-update` listener reads `getConfig()` and `getActivePanel()` synchronously and folds the panel patch into that entry, whole-element changes apply via `restoreWorkspace` and per-panel ones via `restore({ panel })` — the fold-echo law `view/table`'s `Grid` follows, presets decoded values never DOM scraping.
- `react`(`.api/react.md`): a component renders `<perspective-viewer>` via ref, runs `load` in the effect bracket and `delete()` on cleanup; config flows across the custom-element boundary, props do not.
- `system/token` theming: `./themes/*.css` import once through the token stylesheet and the config's `theme` field selects one by NAME, so a theme change is the token flip with `restore({ theme })` / `restyleElement`, never per-instance CSS.
- `view/chart` composition: the viewer earns a surface only when the USER drives the query — pivot, aggregate, filter, window, or expression exploration over a live feed; a fixed-shape interactive grid stays `Grid` (`.api/tanstack-react-table.md`), a declared statistical chart stays Plot (`.api/observablehq-plot.md`), a streaming time-series panel stays uplot (`.api/uplot.md`).

[RAIL_LAW]:
- Package: `@perspective-dev/viewer`
- Owns: the interactive analytics element — registration-by-import, the panel and workspace config round trips, the multi-panel layout with master/detail global filters, the settings and column chrome, selection/click/config/layout events, plugin selection and the plugin author contract, theme roster application, the LLM agent seam, and export/copy/download egress.
- Accept: config values as the one state law (atom-held, schema-decoded, restore-applied) at the element grain for a multi-panel viewer; `getConfig()` paired with `getActivePanel()` synchronously in its handler as the echo seam; panel moves through `addPanel`/`removePanel`/`setActivePanel` with the roster read back as evidence; `Table`-level streaming with the viewer a passive consumer; `viewer-datagrid` + `viewer-charts` as the admitted plugin pair; ref + effect-bracket mounting with `delete()` teardown; themes by name through the token stylesheet; `suppress_errors` for a programmatic restore whose failure is feedback.
- Reject: any `@finos/*` reference; attribute or DOM pokes where `restore` carries the change; a per-viewer state store beside the config atom; a workspace token handed to `restore` or a panel token to `restoreWorkspace`; a saved `layout: null` crossing into a restore; a saved panel id treated as durable across a workspace restore; a cross-filter or master roster written anywhere but the workspace token; a stashed `getConfig` called after dispatch; a React wrapper reaching inside the element; the viewer standing in for a `Grid` fixed grid or a Plot declared chart.

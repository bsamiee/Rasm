# [TS_UI_API_PERSPECTIVE_DEV_VIEWER_DATAGRID]

`@perspective-dev/viewer-datagrid` registers the `perspective-viewer-datagrid` grid plugin against `<perspective-viewer>` by import side effect — evaluating the module defines the custom elements, calls `registerPlugin`, and makes `restore({ plugin: "Datagrid" })` select it. Grid presentation rides `plugin_config` and `columns_config` on the panel config: `regular-table` virtual scroll, tree-pivoted rows with rollup column groups, sticky headers, per-column styling, and cell editing through the viewer edit port, never an element attribute.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-datagrid`
- package: `@perspective-dev/viewer-datagrid` (Apache-2.0)
- deps: `@perspective-dev/client`, `@perspective-dev/viewer` (lockstep), `regular-table`
- module: ESM only — exports `.` (`types: dist/esm/index.d.ts`, `default: dist/esm/perspective-viewer-datagrid.js`) with `./dist/*`/`./src/*` passthroughs
- runtime: browser custom element; evaluating the import registers, so it never tree-shakes or defers behind the first `restore`
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the registration residue and the config shapes a caller writes into the panel config

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `HTMLPerspectiveViewerDatagridPluginElement`  | class         | `instanceof` narrowing on `getPlugin`            |
|  [02]   | `HTMLPerspectiveViewerDatagridToolbarElement` | class         | edit-mode toolbar the plugin mounts              |
|  [03]   | `PRIVATE_PLUGIN_SYMBOL`                       | symbol        | plugin-internal model key                        |
|  [04]   | `DatagridPluginConfig`                        | interface     | the `plugin_config` slot this plugin round-trips |
|  [05]   | `ColumnConfig`                                | interface     | one `columns_config` value's styling keys        |
|  [06]   | `EditMode`                                    | union         | the closed cell-interaction vocabulary           |
|  [07]   | `ColumnConfigSchema`                          | interface     | the Style-tab control set the plugin declares    |

## [03]-[GRID_CONFIG]

`EditMode` decides what a cell gesture does: `READ_ONLY` (default), `EDIT` (cells write back through the edit port, requiring an editable table), and the `SELECT_COLUMN` / `SELECT_ROW` / `SELECT_REGION` / `SELECT_ROW_TREE` modes, which emit selection events instead of editing.

`DatagridPluginConfig` carries the grid-wide state: `edit_mode`, `scroll_lock` (pin the scroll position through updates instead of following appended rows), `columns` (per-column state keyed by name), and `column_size_override`.

`ColumnConfig` carries one column's presentation, keyed by type. Numeric columns take `pos_fg_color` / `neg_fg_color` / `pos_bg_color` / `neg_bg_color` with `number_fg_mode` (`color`, `bar`, `label-bar`, `disabled`) and `number_bg_mode` (`disabled`, `color`, `gradient`) scaled by `fg_gradient` / `bg_gradient`, with `number_format`. String columns take `color` with `string_color_mode` (`foreground`, `background`, `series`) and `format` (`link`, `image`, `bold`). Datetime columns take `color` with `datetime_color_mode` and `date_format`. Every type takes `column_size_override`, and `aggregate_depth` overrides the rollup depth for a pivoted column.

`column_config_schema()` declares, per column and per current value, exactly which controls that column carries — the Style tab and the agent's style-schema tool both read it rather than guessing keys, and the aggregate-depth control surfaces only when the view carries a non-empty `group_by` under rollup mode.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Evaluating the module defines `perspective-viewer-datagrid` + `-toolbar` and calls `registerPlugin`, so the bare side-effect import is the whole entry — no element class is constructed by hand.
- Grid state rides `plugin_config` and `columns_config` on the panel config; every presentation change flows through `restore`, never an element attribute or DOM poke.
- `presize(width, height)` stages the pending box: the plugin runs the data fetch and viewport calculation for it without touching visible DOM, then resolves the commit closure the host invokes in the layout-commit task, so a resize or divider drag lands geometry and cells in one paint with no warp or mirror flash.

[STACKING]:
- `@perspective-dev/viewer` (`.api/perspective-dev-viewer.md`): the plugin implements the viewer's `IPerspectiveViewerPlugin` contract and registers via `registerPlugin("perspective-viewer-datagrid")`; `restore({ plugin: "Datagrid", plugin_config })` selects it, `getPlugin("Datagrid")` returns the element for `instanceof` narrowing, cell edits write through the viewer's `getEditPort()`, and `deselect()` clears selection visuals when the global filter bar drops the clause they produced.
- `@perspective-dev/client` (`.api/perspective-dev-client.md`): the grid renders the `View`'s windowed reads directly, so `group_rollup_mode` draws the row tree and `split_rollup_mode` draws the subtotal and grand-total COLUMN groups — the one plugin realizing both rollup axes.
- `view/chart`: `chart.md` evaluates the bare import once beside the viewer boot; datagrid settings ride `plugin_config` inside the panel config the atom holds and round-trip through `save`/`restore`.

[RAIL_LAW]:
- Package: `@perspective-dev/viewer-datagrid`
- Owns: grid presentation for the viewer — `regular-table` virtual scroll, pivot trees, rollup column groups, per-column styling and formatting, selection modes, and cell editing; selected as `plugin: "Datagrid"` with settings under `plugin_config`.
- Accept: the bare root import at chart-plane module scope; interaction chosen through `edit_mode`; per-column presentation through `columns_config` keys `column_config_schema()` declares; the viewer catalog's plugin-pair ruling.
- Reject: constructing the element classes directly; DOM pokes where `restore` carries the change; a hand-written control set where `column_config_schema()` declares the valid keys; `regular-table` imported beside it as a second grid regime — fixed grids are the `Grid` owner's.

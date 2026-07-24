# [TS_UI_API_PERSPECTIVE_DEV_VIEWER_DATAGRID]

`@perspective-dev/viewer-datagrid` registers the `perspective-viewer-datagrid` grid plugin against `<perspective-viewer>` by import side effect — evaluating the module defines the custom elements, calls `registerPlugin`, and makes `restore({ plugin: "Datagrid" })` select it. Grid presentation rides `plugin_config` on the one config value: `regular-table` virtual scroll, tree-pivoted rows, sticky headers, and cell editing through the viewer edit port, never an element attribute.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-datagrid`
- package: `@perspective-dev/viewer-datagrid` (Apache-2.0)
- deps: `@perspective-dev/client`, `@perspective-dev/viewer` (lockstep), `regular-table`
- module: ESM only — exports `.` (`types: dist/esm/index.d.ts`, `default: dist/esm/perspective-viewer-datagrid.js`) with `./dist/*`/`./src/*` passthroughs
- runtime: browser custom element; evaluating the import registers, so it never tree-shakes or defers behind the first `restore`
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the registration residue — element classes for `instanceof` narrowing and the plugin-internal symbol, none required for normal composition

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `HTMLPerspectiveViewerDatagridPluginElement`  | class         | `instanceof` narrowing on `getPlugin` |
|  [02]   | `HTMLPerspectiveViewerDatagridToolbarElement` | class         | edit-mode toolbar the plugin mounts   |
|  [03]   | `PRIVATE_PLUGIN_SYMBOL`                       | symbol        | plugin-internal model key             |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Evaluating the module defines `perspective-viewer-datagrid` + `-toolbar` and calls `registerPlugin`, so the bare side-effect import is the whole entry — no element class is constructed by hand.
- Grid state rides `plugin_config` on the one `ViewerConfig`; every presentation change flows through `restore`, never an element attribute or DOM poke.

[STACKING]:
- `@perspective-dev/viewer` (`.api/perspective-dev-viewer.md`): the plugin implements the viewer's `IPerspectiveViewerPlugin` contract and registers via `registerPlugin("perspective-viewer-datagrid")`; `restore({ plugin: "Datagrid", plugin_config })` selects it, `getPlugin("Datagrid")` returns the element for `instanceof` narrowing, and cell edits write through the viewer's `getEditPort()`.
- `view/chart`: `chart.md` evaluates the bare import once beside the viewer boot; datagrid settings ride `plugin_config` inside the `ViewerConfig` the atom holds and round-trip through `save`/`restore`.

[RAIL_LAW]:
- Package: `@perspective-dev/viewer-datagrid`
- Owns: grid presentation for the viewer — `regular-table` virtual scroll, pivot trees, column styling, and cell editing; selected as `plugin: "Datagrid"` with settings under `plugin_config`.
- Accept: the bare root import at chart-plane module scope; edit mode through `plugin_config`; the viewer catalog's plugin-pair ruling.
- Reject: constructing the element classes directly; DOM pokes where `restore` carries the change; `regular-table` imported beside it as a second grid regime — fixed grids are the `Grid` owner's.

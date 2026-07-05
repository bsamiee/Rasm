# [@perspective-dev/viewer-datagrid] — the regular-table grid plugin the viewer element selects by name

`@perspective-dev/viewer-datagrid` is a registration-by-import plugin for `<perspective-viewer>`: the bare root import defines the `perspective-viewer-datagrid` and `perspective-viewer-datagrid-toolbar` custom elements and calls `registerPlugin("perspective-viewer-datagrid")` against the viewer's plugin registry — after the import, `"Datagrid"` is selectable through the config's `plugin` field and `restore`. The grid itself is `regular-table` virtual scrolling with tree-pivoted rows, sticky headers, column sizing, and cell editing through the viewer edit port; every behavior knob rides `plugin_config` inside the one config value the atom holds — no element attribute is poked. The exported element classes exist for `instanceof` narrowing and plugin-contract reference, never for manual construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-datagrid`
- package: `@perspective-dev/viewer-datagrid`
- version: `4.5.1`
- license: `Apache-2.0`
- deps: `@perspective-dev/client`, `@perspective-dev/viewer` (lockstep pins), `regular-table`
- module: ESM only — exports `.` (`types: dist/esm/index.d.ts`, `default: dist/esm/perspective-viewer-datagrid.js`), plus `./dist/*`/`./src/*` passthroughs
- catalog-verdict: KEEP — the admitted grid half of the viewer plugin pair (`.api/perspective-dev-viewer.md` carries the pair ruling)
- runtime: browser custom element; the import is the API — evaluation registers, so the import must never be tree-shaken or deferred behind the first `restore`
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the registration residue — three exports, none required for normal composition
- rail: view/chart

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :--------------------------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `HTMLPerspectiveViewerDatagridPluginElement`    | element class    | the registered plugin element — `instanceof` narrowing on `getPlugin()` only |
|  [02]   | `HTMLPerspectiveViewerDatagridToolbarElement`   | element class    | the edit-mode toolbar element the plugin mounts itself                        |
|  [03]   | `PRIVATE_PLUGIN_SYMBOL`                         | internal symbol  | plugin-internal model key — never read by consumers                          |

## [03]-[INTEGRATION]

- Owns: grid presentation for the viewer — virtual scroll, pivot trees, column styling, and cell editing; selected as `plugin: "Datagrid"` with grid settings under `plugin_config`, both round-tripping through `save`/`restore`.
- Accept: the bare root import evaluated once at chart-plane module scope; edit mode enabled through `plugin_config` on the config value; the viewer catalog's plugin-pair ruling.
- Reject: constructing the element classes directly; DOM pokes on the registered element where `restore` carries the change; `regular-table` imported beside it as a second grid regime — fixed grids are the `Grid` owner's.

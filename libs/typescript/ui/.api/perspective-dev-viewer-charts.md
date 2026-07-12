# [TS_UI_API_PERSPECTIVE_DEV_VIEWER_CHARTS]

`@perspective-dev/viewer-charts` is the default chart plugin family for `<perspective-viewer>`: the bare root import evaluates the bundled renderer and registers the full chart roster — series charts (`X Bar`, `Y Bar`, `Y Area`, `X/Y Line`, `X/Y Scatter`), hierarchical charts (`Treemap`, `Sunburst`), `Heatmap`/`Density`, financial charts (`Candlestick`, `OHLC`), and the map family (`Map Scatter`, `Map Line`, `Map Density`) — each selectable through the config's `plugin` field and `restore`, with axis, color, size, label, and tooltip channels driven by the config's column assignments. The package bundles its rendering stack (zero runtime deps) and supersedes the trailing `viewer-d catalogfc` per-chart subpaths; chart-level options ride `plugin_config` inside the one config value, never element attributes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-charts`
- package: `@perspective-dev/viewer-charts`
- license: `Apache-2.0`
- deps: none — the rendering stack is bundled
- module: ESM only — exports `.` (`types: dist/esm/index.d.ts`, `default: dist/esm/perspective-viewer-charts.js`), plus `./src/*`/`./dist/*` passthroughs
- catalog-verdict: KEEP — the admitted chart half of the viewer plugin pair; `viewer-d3fc`/`viewer-openlayers` are rejected beside it (`.api/perspective-dev-viewer.md`)
- runtime: browser custom elements over WebGL; the import is the API — evaluation registers the roster
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: selective registration and the interaction event details
- rail: view/chart

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                                                                             |
| :-----: | :------------------------------------------ | :-------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `register(...plugin_names: string[]): void` | selective entry | narrows the registered roster to named charts when a build carries only a subset; the bare import registers all |
|  [02]   | `PerspectiveSelectDetail`                   | event detail    | the `detail` payload of chart selection events — pairs with the viewer's click/select seam                      |
|  [03]   | `PerspectiveClickDetail` (type)             | event detail    | the click event `detail` shape — row values and config under the pointer                                        |

## [03]-[INTEGRATION]

- Owns: every declared-chart presentation of the viewer — plugin names registered at import, channel assignment from the config's columns, chart interaction events surfacing as element events with typed `detail` payloads.
- Accept: the bare root import evaluated once at chart-plane module scope; `register(names)` only when a build deliberately narrows the roster; chart options under `plugin_config` on the config value.
- Reject: `viewer-d3fc` or `viewer-openlayers` imported beside it; reading chart internals instead of the typed event details; the chart family standing in for Plot-regime declared charts — analytic pivot charts live here, declarative statistical graphics live on the Plot owner.

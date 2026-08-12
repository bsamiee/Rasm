# [TS_UI_API_PERSPECTIVE_DEV_VIEWER_CHARTS]

`@perspective-dev/viewer-charts` paints the declared-chart half of `<perspective-viewer>`: importing the root registers the bundled WebGL chart roster against the viewer's plugin registry, each chart selected through the config's `plugin` field, driven by its positional column slots, and tuned through `plugin_config` on the panel config. Rendering runs off an `OffscreenCanvas` in a worker or in-process renderer over a pooled GL context set, so a page holds more charts than the browser's per-agent context cap.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@perspective-dev/viewer-charts`
- package: `@perspective-dev/viewer-charts` (Apache-2.0)
- module: ESM only — `.` exports `register` and the event-detail types; `./src/*`/`./dist/*` passthroughs
- runtime: browser custom element over a bundled WebGL renderer; evaluating the root import registers the chart roster, and the map subsystem loads as a separate chunk only when a map chart is selected
- rail: view/chart — `chart.md` composes the bare side-effect import beside the viewer boot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the chart interaction event details

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `PerspectiveSelectDetail` | class         | `perspective-select` detail, re-exported from the viewer surface                     |
|  [02]   | `PerspectiveClickDetail`  | interface     | `perspective-click` detail — `row`, `column_names`, restore-filter `config`, `panel` |

## [03]-[CHART_ROSTER]

Every chart is one registered plugin selected by name; `initial` names the positional `columns` slots the settings panel labels.

| [INDEX] | [PLUGIN]      | [TAG]         | [CATEGORY]   | [COLUMN_SLOTS]                                          |
| :-----: | :------------ | :------------ | :----------- | :------------------------------------------------------ |
|  [01]   | `X Bar`       | `x-bar`       | Series       | `X Axis`                                                |
|  [02]   | `Y Bar`       | `y-bar`       | Series       | `Y Axis`                                                |
|  [03]   | `Y Line`      | `y-line`      | Series       | `Y Axis`                                                |
|  [04]   | `Y Scatter`   | `y-scatter`   | Series       | `Y Axis`                                                |
|  [05]   | `Y Area`      | `y-area`      | Series       | `Y Axis`                                                |
|  [06]   | `X/Y Scatter` | `scatter`     | Cartesian    | `X Axis` `Y Axis` `Color` `Size` `Label` `Tooltip`      |
|  [07]   | `X/Y Line`    | `line`        | Cartesian    | `X Axis` `Y Axis` `Tooltip`                             |
|  [08]   | `Density`     | `density`     | Cartesian    | `X Axis` `Y Axis` `Color` `Tooltip`                     |
|  [09]   | `Treemap`     | `treemap`     | Hierarchical | `Size` `Color` `Tooltip`                                |
|  [10]   | `Sunburst`    | `sunburst`    | Hierarchical | `Size` `Color` `Tooltip`                                |
|  [11]   | `Heatmap`     | `heatmap`     | Hierarchical | `Color`                                                 |
|  [12]   | `Candlestick` | `candlestick` | Financial    | `Open` `Close` `High` `Low` `Tooltip`                   |
|  [13]   | `OHLC`        | `ohlc`        | Financial    | `Open` `Close` `High` `Low` `Tooltip`                   |
|  [14]   | `Map Scatter` | `map-scatter` | Map          | `Longitude` `Latitude` `Color` `Size` `Label` `Tooltip` |
|  [15]   | `Map Line`    | `map-line`    | Map          | `Longitude` `Latitude` `Tooltip`                        |
|  [16]   | `Map Density` | `map-density` | Map          | `Longitude` `Latitude` `Color` `Tooltip`                |

Pivot roles differ by family and each chart declares its own: the Y-series and financial charts draw `group_by` as the X axis and `split_by` as series, `X Bar` swaps that to a Y axis, `Treemap` and `Sunburst` read `group_by` as a hierarchy and `split_by` as facets, `Heatmap` takes `group_by` as X and `split_by` as Y, and the cartesian and map charts take both axes from `columns` so their pivots are plain aggregation keys. `X/Y Line` and `Map Line` connect points in ROW order, so an unsorted config draws the table's natural order — those two earn a `sort`.

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: roster registration at import

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]                                                                |
| :-----: | :---------------------- | :------ | :-------------------------------------------------------------------------- |
|  [01]   | `register(...string[])` | static  | narrow registration to the named charts; the bare root import registers all |

`plugin_config` carries the chart-wide settings, each chart declaring which fields it renders: `facet_mode` (`grid` small multiples against `overlay` one plot) with `facet_zoom_mode`, `series_zoom_mode`, `auto_alt_y_axis`, and `include_zero` for axis behavior; `domain_mode` choosing whether successive updates refit (`fit`) or monotonically grow (`expand`) the rendered domain; the glyph metrics `line_width_px`, `point_size_px`, `band_inner_frac`, `bar_inner_pad`, `wick_width_px`, and `ohlc_line_width_px`; the density controls `gradient_color_mode` (`mean`, `density`, `extreme`, `signed`), `gradient_radius_px`, `gradient_intensity`, and `gradient_heat_max`; and the map controls `map_tile_provider` (`carto-positron`, `carto-dark-matter`, `carto-voyager`) with `map_tile_alpha`. Defaults resolve per chart family, so `include_zero` holds for the bar and area charts and `facet_mode` starts at `overlay` for the band-pipeline families.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every chart is one plugin registered at import and selected through the config's `plugin` field; the config's positional column slots drive its channels, and interaction returns as element events with typed `detail`.
- Each chart releases its renderer, GL context, and compiled programs when the host removes it from the DOM on a chart-type switch, so cycling the roster never accumulates contexts against the browser cap.

[STACKING]:
- `@perspective-dev/viewer`(`.api/perspective-dev-viewer.md`): the side-effect import registers these charts against the viewer's plugin registry; `restore({ plugin, plugin_config })` selects a chart and its options and `save()` round-trips them, so this surface paints while the viewer owns the config value. Neither rollup mode reaches these charts — `split_rollup_mode` and `group_rollup_mode` are the datagrid's to draw.
- `@perspective-dev/client`(`.api/perspective-dev-client.md`): a chart reads its frame from the bound `View` and resolves `group_by` level types from `Table.schema()` with `View.expression_schema()`, since pivot levels surface as row-path columns absent from `View.schema()`.
- within-lib: `chart.md` evaluates the bare import once beside the viewer boot and reads `perspective-click`/`perspective-select` through the typed `detail`, never chart internals.

[LOCAL_ADMISSION]:
- Admission pairs this chart plugin with `@perspective-dev/viewer`; the two release lockstep.

[RAIL_LAW]:
- Package: `@perspective-dev/viewer-charts`
- Owns: every declared-chart presentation of the viewer — plugin registration at import, the series/cartesian/hierarchical/financial/map roster, channel assignment from the config's column slots, faceting and zoom behavior, basemap tiles, and chart interaction as element events with typed `detail`.
- Accept: the bare root import evaluated once at chart-plane module scope; `register(names)` only where a build narrows the roster; chart options under `plugin_config`; a `sort` wherever a row-order-connecting chart is selected.
- Reject: reading chart internals over the typed event details; a hand-rolled tile source where `map_tile_provider` selects the basemap; the chart family standing in for Plot-regime declared statistical graphics or for `viewer/geo`'s live basemap.

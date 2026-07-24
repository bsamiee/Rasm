# [TS_IAC_API_GRAFANA_GRAFANA_FOUNDATION_SDK]

Typed dashboard, panel, and query construction: every builder is a fluent `cog.Builder<T>` whose `.build()` emits the plain Grafana JSON model, and that JSON is the sole boundary the `@pulumiverse/grafana` apply consumes.

## [01]-[PACKAGE_SURFACE]

- package: `@grafana/grafana-foundation-sdk` (Apache-2.0)
- module: exports-map subpaths, one module per builder domain; each resolves `dist/<domain>/index.d.ts` re-exporting its `types.gen` and per-builder `*Builder.gen`
- runtime: isomorphic — builders emit plain JSON, no runtime peer
- plane: `plane:deploy` — folded only by `operate/observe.md`'s `_compiled`; no runtime module resolves it
- rail: deployment / dashboard-compile

## [02]-[DASHBOARD_MODULE]

[DASHBOARD_TYPE_SCOPE]: `./dashboard` owns `DashboardBuilder` (root) beside the companion builders `RowBuilder`, the variable-builder family, `ThresholdsConfigBuilder`, `TimePickerBuilder`, `DashboardLinkBuilder`, `AnnotationQueryBuilder`. Every member is a fluent instance setter terminating at `.build()`.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `new DashboardBuilder(title)` / `.build()`                        | root construction and JSON emission     |
|  [02]   | `.uid(uid)` / `.title(title)` / `.tags(tags)`                     | identity fields                         |
|  [03]   | `.refresh(refresh)` / `.time({ from, to })`                       | refresh cadence and time range          |
|  [04]   | `.withPanel(panel)` / `.withRow(rowPanel)`                        | panel composition from per-tag builders |
|  [05]   | `.withVariable(variable)` / `.variables(rows)`                    | template variables                      |
|  [06]   | `.annotation(row)` / `.annotations(rows)`                         | annotation queries                      |
|  [07]   | `.link(row)` / `.timepicker(row)` / `.editable()` / `.readonly()` | presentation policy                     |

[ThresholdsConfigBuilder]: `.mode(ThresholdsMode)` `.steps(Threshold[])` — `ThresholdsMode.Absolute | .Percentage`; steps sort ascending by `value` over `Threshold { value: number | null, color: string }`, the first row `value: null` as the mandatory -Infinity base.
[AnnotationQueryBuilder]: `.name(string)` `.iconColor(string)` `.enable(boolean)`
[DashboardLinkBuilder]: `.title(string)` `.type(DashboardLinkType)` `.icon(string)` `.tooltip(string)` `.url(string)` `.tags(string[])` `.asDropdown(boolean)` `.placement(DashboardLinkPlacement.InControlsMenu)` `.targetBlank(boolean)` `.includeVars(boolean)` `.keepTime(boolean)` — `DashboardLinkType = Link | Dashboards`; panel `.links(...)` takes `cog.Builder<dashboard.DashboardLink>[]`, so links stay typed through emission.

## [03]-[PANEL_MODULES]

[PANEL_ENTRY_SCOPE]: one `PanelBuilder` per visualization subpath; the shared members below ride every panel module (verified on `timeseries`) and map onto the core panel family's `_PanelFields`. Every member is a fluent instance setter.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `new PanelBuilder()` / `.build()`                        | one panel row; feeds `.withPanel`            |
|  [02]   | `.title(t)` / `.description(d)` / `.transparent(b)`      | shared emission fields                       |
|  [03]   | `.gridPos({ h, w, x, y })` / `.span(w)` / `.height(h)`   | placement — `DashboardModel.laid` lands here |
|  [04]   | `.withTarget(dataquery)` / `.datasource(ref)`            | query binding and datasource pin             |
|  [05]   | `.unit(u)` / `.min(n)` / `.max(n)` / `.thresholds(b)`    | value display                                |
|  [06]   | `.legend(b)` / `.tooltip(b)`                             | common-options builders from `./common`      |
|  [07]   | `.repeat(r)` / `.links(rows)` / `.withTransformation(t)` | repetition, panel links, transform rows      |

`.datasource(ref)` takes `common.DataSourceRef { type?, uid? }` and pins `uid` to the `_SOURCES` row key.
[logs.PanelBuilder]: `.showTime(boolean)` `.wrapLogMessage(boolean)` `.sortOrder(LogsSortOrder)` `.dedupStrategy(LogsDedupStrategy)` — `LogsSortOrder = Descending | Ascending`; `LogsDedupStrategy = none | exact | numbers | signature`.

### [03.1]-[GEOMAP]

`./geomap` carries `PanelBuilder`, `MapViewConfigBuilder`, and `ControlsOptionsBuilder`; map-layer and geometry-source builders live in `./common`. Every companion constructor is zero-argument and `.build()` returns its plain model.

[geomap.PanelBuilder]: `.view(MapViewConfigBuilder)` `.controls(ControlsOptionsBuilder)` `.basemap(MapLayerOptionsBuilder)` `.layers(MapLayerOptionsBuilder[])`
[MapViewConfigBuilder]: `.id` `.lat` `.lon` `.zoom` `.minZoom` `.maxZoom` `.padding` `.allLayers` `.lastOnly` `.layer` `.shared`
[MapLayerOptionsBuilder]: `.type` `.name` `.config` `.location(FrameGeometrySourceBuilder)` `.filterData` `.opacity` `.tooltip(boolean)`
[FrameGeometrySourceBuilder]: `.mode(Auto | Geohash | Coords | Lookup)` `.geohash` `.latitude` `.longitude` `.wkt` `.lookup` `.gazetteer`

### [03.2]-[TABLE]

[table.PanelBuilder]: `.frameIndex(number)` `.showHeader(boolean)` `.showTypeIcons(boolean)` `.sortBy(cog.Builder<TableSortByFieldState>[])` `.footer(cog.Builder<TableFooterOptions>)`
[TableSortByFieldStateBuilder]: `.displayName(string)` `.desc(boolean)`
[TableFooterOptionsBuilder]: `.show(boolean)` `.reducer(string[])` `.fields(string[])` `.enablePagination(boolean)` `.countRows(boolean)`

### [03.3]-[TIMESERIES]

[timeseries.PanelBuilder]: `.axisPlacement(AxisPlacement)` `.axisColorMode(AxisColorMode)` `.axisLabel(string)` `.axisWidth(number)` `.axisSoftMin(number)` `.axisSoftMax(number)` `.axisGridShow(boolean)` `.scaleDistribution(cog.Builder<ScaleDistributionConfig>)` `.axisCenteredZero(boolean)` `.axisBorderShow(boolean)` — `AxisPlacement = Auto | Top | Right | Bottom | Left | Hidden`; `AxisColorMode = Text | Series`.
[ScaleDistributionConfigBuilder]: `.type(Linear | Log | Ordinal | Symlog)` `.log(number)` `.linearThreshold(number)`
[VizTooltipOptionsBuilder]: `.mode(Single | Multi | None)` `.sort(Ascending | Descending | None)` `.maxWidth(number)` `.maxHeight(number)` `.hideZeros(boolean)` — consumed by `PanelBuilder.tooltip`.

## [04]-[QUERY_MODULES]

[prometheus.DataqueryBuilder]: `.expr(expr)` `.refId(id)` `.exemplar(boolean)` `.legendFormat(f)` `.instant()` `.range()` `.datasource(ref)` `.format(PromQueryFormat)` `.hide(boolean)` — `PromQueryFormat = TimeSeries | Table | Heatmap`; `.instant()`/`.range()` are zero-argument mode selectors.
[loki.DataqueryBuilder]: `.expr(expr)` `.refId(id)` `.legendFormat(f)` `.maxLines(number)` `.instant(boolean)` `.range(boolean)` `.datasource(ref)`
[grafanapyroscope.DataqueryBuilder]: `.labelSelector(string)` `.spanSelector(string[])` `.profileTypeId(string)` `.groupBy(string[])` `.limit(number)` `.maxNodes(number)` `.refId(string)` `.hide(boolean)` `.queryType(string)` `.datasource(DataSourceRef)` — `PyroscopeQueryType = Metrics | Profile | Both`.

`./grafanapyroscope` ships no visualization or panel builder: a Pyroscope panel arm has no SDK member to compile, while Pyroscope query rows stay fully typed.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DashboardModel` is the authority and the builder the emitter: `_compiled` reads `typeof DashboardModel.Encoded` and maps every model field onto one builder member — `laid` positions become `gridPos`, rendered `Query` strings become `expr` — so a model field with no builder member fails the fold at compile time.

[STACKING]:
- `@pulumiverse/grafana`(`.api/pulumiverse-grafana.md`): `.build()` output feeds `oss.Dashboard.configJson` through `pulumi.jsonStringify`, and `storeDashboardSha256` diffs by content hash, so one builder-emitted byte change is exactly one drift row.
- `@rasm/ts/core` `DashboardModel`: `_compiled` folds each encoded model through `DashboardBuilder` and the per-tag `PanelBuilder`s, one builder row per core panel tag, inventing no name, threshold, or layout.

[LOCAL_ADMISSION]:
- builders resolve only inside the `_compiled` fold on the deploy plane; `.build()` is the single emission seam.

[RAIL_LAW]:
- Package: `@grafana/grafana-foundation-sdk`
- Owns: typed dashboard construction — panel and query typing survive through to emission
- Accept: one builder row per core panel tag; `.build()` as the single JSON seam
- Reject: hand-authored dashboard JSON where a builder subpath exists; a second model authority beside `DashboardModel`

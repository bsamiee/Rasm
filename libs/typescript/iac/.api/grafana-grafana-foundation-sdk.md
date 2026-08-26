# [TS_IAC_API_GRAFANA_GRAFANA_FOUNDATION_SDK]

Typed dashboard, panel, and query construction: every builder is a fluent `cog.Builder<T>` whose `.build()` emits the plain Grafana JSON model, and that JSON is the sole boundary the `@pulumiverse/grafana` apply consumes.

## [01]-[DASHBOARD_MODULE]

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

## [02]-[PANEL_MODULES]

[PANEL_ENTRY_SCOPE]: one `PanelBuilder` per visualization subpath; the shared members below ride every panel module (verified on `timeseries`) and map onto the core panel family's `_PanelFields`. Every member is a fluent instance setter.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `new PanelBuilder()` / `.build()`                        | one panel row; feeds `.withPanel`            |
|  [02]   | `.title(t)` / `.description(d)` / `.transparent(b)`      | shared emission fields                       |
|  [03]   | `.gridPos({ h, w, x, y })` / `.span(w)` / `.height(h)`   | placement — `DashboardModel.laid` lands here |
|  [04]   | `.withTarget(dataquery)` / `.datasource(ref)`            | query binding and datasource pin             |
|  [05]   | `.unit(u)` / `.min(n)` / `.max(n)` / `.thresholds(b)`    | value display — `u` is a `./units` id        |
|  [06]   | `.legend(b)` / `.tooltip(b)`                             | common-options builders from `./common`      |
|  [07]   | `.repeat(r)` / `.links(rows)` / `.withTransformation(t)` | repetition, panel links, transform rows      |

`.datasource(ref)` takes `common.DataSourceRef { type?, uid? }` and pins `uid` to the `_SOURCES` row key.

[UNITS_REGISTRY]: `./units` ships the display-unit registry `.unit(u)` resolves against — flat `export declare const` string literals, one per registry entry, so a display unit is a named constant rather than a guessed word and an unrecognized id renders its value bare. `.unit` types as plain `string`, so the roster and never the signature proves an id.

Entries the branch's UCUM vocabulary maps onto: `NoUnit = "none"`, `Short = "short"`, `SiShort = "sishort"`, `Percent = "percent"`, `PercentUnit = "percentunit"`, `BytesIEC = "bytes"`, `BytesSI = "decbytes"`, `Kibibytes = "kbytes"`, `BytesPerSecondIEC = "binBps"`, `BytesPerSecondSI = "Bps"`, `Nanoseconds = "ns"`, `Microseconds = "\u00B5s"`, `Milliseconds = "ms"`, `Seconds = "s"`, `DurationMilliseconds = "dtdurationms"`, `DurationSeconds = "dtdurations"`, `CountsPerSecond = "cps"`, `CountsPerMinute = "cpm"`. Byte entries split IEC from SI at every scale, so a level and its throughput answer from one base or the panel rescales mid-axis.

[ENUM_HOME]: every enum a builder member takes ships from the module that owns the shape, never from the panel subpath that consumes it — `./common` declares `AxisPlacement` (`Auto|Top|Right|Bottom|Left|Hidden`), `AxisColorMode` (`Text|Series`), `ScaleDistribution` (`Linear|Log|Ordinal|Symlog`), `TooltipDisplayMode` (`Single|Multi|None`), `SortOrder`, `FrameGeometrySourceMode` (`Auto|Geohash|Coords|Lookup`), `LogsSortOrder`, and `LogsDedupStrategy`, while `./dashboard` declares `DashboardLinkType` (`Link|Dashboards`), `DashboardLinkPlacement`, and `ThresholdsMode`; importing one from its consuming panel subpath resolves nothing.
[logs.PanelBuilder]: `.showTime(boolean)` `.wrapLogMessage(boolean)` `.sortOrder(LogsSortOrder)` `.dedupStrategy(LogsDedupStrategy)` — `LogsSortOrder = Descending | Ascending`; `LogsDedupStrategy = none | exact | numbers | signature`.

### [02.1]-[GEOMAP]

`./geomap` carries `PanelBuilder`, `MapViewConfigBuilder`, and `ControlsOptionsBuilder`; map-layer and geometry-source builders live in `./common`. Every companion constructor is zero-argument and `.build()` returns its plain model.

[geomap.PanelBuilder]: `.view(MapViewConfigBuilder)` `.controls(ControlsOptionsBuilder)` `.basemap(MapLayerOptionsBuilder)` `.layers(MapLayerOptionsBuilder[])` — `.view` and `.controls` come from `./geomap`, both layer arguments from `./common`
[MapViewConfigBuilder]: `.id(string)` `.lat(number)` `.lon(number)` `.zoom(number)` `.minZoom(number)` `.maxZoom(number)` `.padding(number)` `.allLayers(boolean)` `.lastOnly(boolean)` `.layer(string)` `.shared(boolean)` — `.allLayers` frames every layer's data instead of pinning a centre no dataset shares
[ControlsOptionsBuilder]: `.showZoom(boolean)` `.mouseWheelZoom(boolean)` `.showAttribution(boolean)` `.showScale(boolean)` `.showDebug(boolean)` `.showMeasure(boolean)` — the visible control and the wheel binding are independent halves, so a reader-facing zoom posture answers both
[MapLayerOptionsBuilder]: `.type` `.name` `.config(any)` `.location(FrameGeometrySourceBuilder)` `.filterData` `.opacity` `.tooltip(boolean)` — `config` is UNTYPED (`MapLayerOptions.config?: any`); the markers style envelope is `{ showLegend, style: { color?, text?, size?, rotation?, symbol? } }`, each slot filled by a typed `./common` dimension builder below
[FrameGeometrySourceBuilder]: `.mode(Auto | Geohash | Coords | Lookup)` `.geohash` `.latitude` `.longitude` `.wkt` `.lookup` `.gazetteer`
[DIMENSION_BUILDERS]: `./common` ships the typed field-binding builders every geomap style slot compiles through — `ColorDimensionConfigBuilder` (`.field` `.fixed`), `TextDimensionConfigBuilder` (`.mode(TextDimensionMode)` REQUIRED for a field-driven label, `.field`, `.fixed`; `TextDimensionMode = Fixed | Field | Template`), `ScaleDimensionConfigBuilder` (`.field` `.fixed` `.min` `.max` both REQUIRED, `.mode(ScaleDimensionMode)`; `ScaleDimensionMode = Linear | Quad`), `ScalarDimensionConfigBuilder`, `ResourceDimensionConfigBuilder` — so the `.field` spellings type-check even though the enclosing `config` envelope does not

### [02.2]-[TABLE]

[table.PanelBuilder]: `.frameIndex(number)` `.showHeader(boolean)` `.showTypeIcons(boolean)` `.sortBy(cog.Builder<TableSortByFieldState>[])` `.footer(cog.Builder<TableFooterOptions>)`
[TableSortByFieldStateBuilder]: `.displayName(string)` `.desc(boolean)`
[TableFooterOptionsBuilder]: `.show(boolean)` `.reducer(string[])` `.fields(string[])` `.enablePagination(boolean)` `.countRows(boolean)`

### [02.3]-[NODEGRAPH]

[nodegraph.PanelBuilder]: `.nodes(cog.Builder<NodeOptions>)` `.edges(cog.Builder<EdgeOptions>)` `.zoomMode(ZoomMode)` — `ZoomMode = Cooperative | Greedy`, greedy taking the wheel outright and cooperative demanding a modifier; `NodeOptions` carries `mainStatUnit`/`secondaryStatUnit`/`arcs`, `EdgeOptions` the two stat units. Node and edge IDENTITY is frame-column convention, never a builder member, so an identity mapping lands as a rename transformation.
[NODEGRAPH_FRAMES]: frame ADMISSION requires one of `meta.preferredVisualisationType === "nodeGraph"`, a frame `name`/`refId` of `nodes`/`edges`, or a field named exactly `id`; the nodes-vs-edges SPLIT keys on `name === "edges"` or a field named exactly `source` — both lookups CASE-SENSITIVE even though later column reads lowercase. Node columns: `id` (required), `title`/`subtitle`, `mainstat`/`secondarystat` (first numeric field is the `mainstat` fallback), `color` XOR `arc__<suffix>` (sum to 1), `detail__<suffix>`, `noderadius` (pixels), `highlighted`, `icon`, `fixedx`/`fixedy`. Edge columns: `id` (REQUIRED), `source`, `target`, `mainstat`/`secondarystat`, `thickness`, `color`, `strokedasharray`. No "weight" column exists on either frame — a stat magnitude lands on `mainstat`.

### [02.4]-[TIMESERIES]

[timeseries.PanelBuilder]: `.axisPlacement(AxisPlacement)` `.axisColorMode(AxisColorMode)` `.axisLabel(string)` `.axisWidth(number)` `.axisSoftMin(number)` `.axisSoftMax(number)` `.axisGridShow(boolean)` `.scaleDistribution(cog.Builder<ScaleDistributionConfig>)` `.axisCenteredZero(boolean)` `.axisBorderShow(boolean)` — `AxisPlacement = Auto | Top | Right | Bottom | Left | Hidden`; `AxisColorMode = Text | Series`.
[ScaleDistributionConfigBuilder]: `.type(Linear | Log | Ordinal | Symlog)` `.log(number)` `.linearThreshold(number)`
[VizTooltipOptionsBuilder]: `.mode(Single | Multi | None)` `.sort(Ascending | Descending | None)` `.maxWidth(number)` `.maxHeight(number)` `.hideZeros(boolean)` — consumed by `PanelBuilder.tooltip`.

## [03]-[QUERY_MODULES]

[prometheus.DataqueryBuilder]: `.expr(expr)` `.refId(id)` `.exemplar(boolean)` `.legendFormat(f)` `.instant()` `.range()` `.datasource(ref)` `.format(PromQueryFormat)` `.hide(boolean)` — `PromQueryFormat = TimeSeries | Table | Heatmap`; `.instant()`/`.range()` are zero-argument mode selectors.
[loki.DataqueryBuilder]: `.expr(expr)` `.refId(id)` `.legendFormat(f)` `.maxLines(number)` `.instant(boolean)` `.range(boolean)` `.datasource(ref)`
[grafanapyroscope.DataqueryBuilder]: `.labelSelector(string)` `.spanSelector(string[])` `.profileTypeId(string)` `.groupBy(string[])` `.limit(number)` `.maxNodes(number)` `.refId(string)` `.hide(boolean)` `.queryType(string)` `.datasource(DataSourceRef)` — `PyroscopeQueryType = Metrics | Profile | Both`.

`./grafanapyroscope` ships no visualization or panel builder: a Pyroscope panel arm has no SDK member to compile, while Pyroscope query rows stay fully typed. `operate/observe#BOARD_APPLY` `_profiled` composes that typed query and carries its `.build()` output inside a board-level `DashboardLinkBuilder` URL, so the profile plane reaches an operator through Explore rather than through hand-authored panel JSON.

[QUERY_MODULE_CLOSURE]: codegen ships one query module per datasource Grafana BUNDLES — `athena`, `azuremonitor`, `bigquery`, `cloudwatch`, `elasticsearch`, `googlecloudmonitoring`, `grafanapyroscope`, `loki`, `parca`, `prometheus`, `tempo`, `testdata` — and a third-party plugin appears in none of them. That roster closes CONVENIENCE, never admission: `[TARGET_CONTRACT]` below rules the panel-target interface structural, so an unbundled datasource is typed by writing its builder in the consuming branch. `./datasource` exposes the generic `DataqueryBuilder` (`.panelId` `.refId` `.hide` `.withTransforms` `.datasource`) over a `Dataquery` carrying those five fields beside `queryType` and NO query body, so it types an envelope and never an expression — and it refuses as a base to extend, because `defaultDataquery()` seeds `panelId: 0` and `withTransforms: false`, two keys a foreign plugin's config never declares yet every derived query then ships.
[TARGET_CONTRACT]: `./cog` publishes exactly `Builder<T>`, `isBuilder`, and `Dataquery` — `interface Builder<T> { build: () => T }` and `interface Dataquery { _implementsDataqueryVariant(): void }` — while `timeseries/panelBuilder.gen.d.ts` declares `withTarget(target: cog.Builder<cog.Dataquery>): this` beside `targets(targets: cog.Builder<cog.Dataquery>[]): this`, and `dashboard.Panel` carries `targets?: cog.Dataquery[]`. Neither interface carries a brand, a private field, or a registry probe, so ANY class whose `build()` returns an object bearing that marker method is a valid target by construction. Every generated builder stands alone — `cog` ships no base class and no mixin — so the reusable machinery IS that shape: a `protected readonly internal` draft seeded by the module's `default<Shape>()` factory, fluent setters returning `this`, one `build()` emission. Emission drops the marker because `_implementsDataqueryVariant: () => { }` is a real runtime function property, so a serialized query carries data fields alone.
[CLICKHOUSE_ABSENCE]: `grafana-clickhouse-datasource` is a third-party plugin, so no `./clickhouse` module ships and the tier dataquery is branch-owned at `operate/observe#BOARD_APPLY` under `[TARGET_CONTRACT]`, transcribing the plugin's own `CHSqlQuery` off `.api/clickhouse.md` `[QUERY_CONTRACT]`. No bump reaches it: the plugin is not Grafana-bundled at any line.
[PIN_DIRECTION]: the `latest` dist-tag is the SUPERSET and every per-Grafana-version tag is a subset, so a bump onto one is a REGRESSION — the resource-model modules (dashboard v2, folder v1, playlist, preferences, stars) drop whole and one panel module arrives in trade. Pin `latest`; a per-version tag is admissible only when a consumer needs a panel model that line alone carries, and it lands as a stated capability loss.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DashboardModel` is the authority and the builder the emitter: `_compiled` reads `typeof DashboardModel.Encoded` and maps every model field onto one builder member — `laid` positions become `gridPos`, rendered `Query` strings become `expr`, `links` become `DashboardLinkBuilder` rows, `axes` and `interaction.tooltip` reach the Timeseries axis and tooltip members, `interaction.zoom` reaches the Geomap controls pair and the Nodes zoom mode, `Table.sort` reaches `sortBy`, `panel.source` reaches `datasource`, and `Geomap.mapping` reaches one `Coords`-mode `location` layer — so a model field with no builder member fails the fold at compile time and none survives as an inert emission fact.

[STACKING]:
- `@pulumiverse/grafana`(`.api/pulumiverse-grafana.md`): `.build()` output feeds `oss.Dashboard.configJson` through `pulumi.jsonStringify`, and `storeDashboardSha256` diffs by content hash, so one builder-emitted byte change is exactly one drift row.
- `@rasm/core` `DashboardModel`: `_compiled` folds each encoded model through `DashboardBuilder` and the per-tag `PanelBuilder`s, one builder row per core panel tag, inventing no name, threshold, or layout.
- `grafana-clickhouse-datasource`(`.api/clickhouse.md`): `operate/observe#BOARD_APPLY`'s `_TierQuery` implements `cog.Builder<cog.Dataquery>` over that plugin's `CHSqlQuery`, so a tier tile reaches `withTarget` through the identical interface a bundled dataquery takes and the compile leg keeps one target fold.
- `@rasm/core` `Convention.grafanaUnit`: the core owner carries the UCUM-code-to-display-id correspondence as data transcribed from `./units`, so a pane names its own metric's code and the id reaching `.unit(u)` is a registry entry; the deploy plane spells no display word and the core plane imports no builder.

[LOCAL_ADMISSION]:
- builders resolve only inside the `_compiled` fold on the deploy plane; `.build()` is the single emission point.

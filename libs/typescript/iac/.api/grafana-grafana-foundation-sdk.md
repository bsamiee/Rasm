# [TS_IAC_API_GRAFANA_GRAFANA_FOUNDATION_SDK]

[PACKAGE_SURFACE]:
- package: `@grafana/grafana-foundation-sdk` · license `Apache-2.0`
- module: exports-map subpaths only — one module per builder domain (`./dashboard`, `./timeseries`, `./stat`, `./gauge`, `./heatmap`, `./logs`, `./table`, `./geomap`, `./nodegraph`, `./prometheus`, `./loki`, `./units`, `./common`, `./cog`, …); each subpath resolves `dist/<domain>/index.d.ts` re-exporting its `types.gen` and one `*Builder.gen` file per builder.
- shape: every builder is a fluent `cog.Builder<T>` class — chainable members, terminal `.build()` emitting the plain Grafana JSON model — so authoring is typed and the emission boundary is one `.build()` call feeding `oss.Dashboard.configJson`.
- plane: `plane:deploy` — consumed only by `operate/observe.md`'s `_compiled` fold; no runtime module resolves it.
- rail: deployment / dashboard-compile.

`@grafana/grafana-foundation-sdk` is the typed compile leg between the core `DashboardModel` and the `@pulumiverse/grafana` apply: `observe/board` emits encoded models, `_compiled` folds them through `DashboardBuilder` and the per-tag panel builders, and the provider posts the built JSON. Structure is the SDK's concern — compile-time member checking against the Grafana schema — and provisioning (apply, diff, drift) is the provider's. A hand-authored dashboard JSON blob where a builder subpath exists is the rejected form.

## [01]-[DASHBOARD_MODULE]

`./dashboard` carries the root builder and the shared companion builders (`RowBuilder`, `QueryVariableBuilder` and the variable-builder family, `ThresholdsConfigBuilder`, `TimePickerBuilder`, `DashboardLinkBuilder`, `AnnotationQueryBuilder`). `DashboardBuilder` members, verified against the shipped declarations:

| [INDEX] | [MEMBER]                                                          | [ROLE]                                            |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `new DashboardBuilder(title)` / `.build()`                        | root construction and JSON emission               |
|  [02]   | `.uid(uid)` / `.title(title)` / `.tags(tags)`                     | identity — the core model's `uid`/`title`/`tags`  |
|  [03]   | `.refresh(refresh)` / `.time({ from, to })`                       | cadence and range — the model's `refresh`/`since` |
|  [04]   | `.withPanel(panel)` / `.withRow(rowPanel)`                        | panel composition from the per-tag builders       |
|  [05]   | `.withVariable(variable)` / `.variables(rows)`                    | template variables — the tenant row lands here    |
|  [06]   | `.annotation(row)` / `.annotations(rows)`                         | annotation queries — the slo spec annotations     |
|  [07]   | `.link(row)` / `.timepicker(row)` / `.editable()` / `.readonly()` | presentation policy rows                          |

Companion-builder members, verified against the module reference: `ThresholdsConfigBuilder.mode(mode)` (`ThresholdsMode.Absolute | .Percentage`) and `.steps(steps)` over `Threshold { value: number | null, color: string }` rows sorted ascending by `value` with the first row `value: null` — the mandatory -Infinity base; `AnnotationQueryBuilder.name(name)` / `.iconColor(color)` / `.enable(enable)` — the rows the core model's annotation slug/tone land on.

## [02]-[PANEL_MODULES]

One `PanelBuilder` per visualization subpath; the shared members below ride every panel module, verified on the `timeseries` declaration — the shared surface the core panel family's `_PanelFields` maps onto:

| [INDEX] | [MEMBER]                                                         | [ROLE]                                                        |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `new PanelBuilder()` / implements `cog.Builder<dashboard.Panel>` | one panel row; feeds `.withPanel`                             |
|  [02]   | `.title(t)` / `.description(d)` / `.transparent(b)`              | the shared emission fields                                    |
|  [03]   | `.gridPos({ h, w, x, y })` / `.span(w)` / `.height(h)`           | placement — `DashboardModel.laid` positions land on `gridPos` |
|  [04]   | `.withTarget(dataquery)` / `.datasource(ref)`                    | query binding — the prometheus or loki dataquery rows         |
|  [05]   | `.unit(u)` / `.min(n)` / `.max(n)` / `.thresholds(b)`            | value display — the model's unit/ceiling/steps columns        |
|  [06]   | `.legend(b)` / `.tooltip(b)`                                     | common-options builders from `./common`                       |
|  [07]   | `.repeat(r)` / `.links(rows)` / `.withTransformation(t)`         | repetition, panel links, transform rows — on every subpath    |

`.datasource(ref)` takes `common.DataSourceRef { type?: string; uid?: string }` — the compile leg pins `uid` to the `_SOURCES` row key; the logs subpath adds `.showTime(b)` / `.wrapLogMessage(b)` / `.sortOrder(common.LogsSortOrder)` (`Descending | Ascending`) / `.dedupStrategy(common.LogsDedupStrategy)` (`none | exact | numbers | signature`) — the display rows the model's Logs case lands.

## [03]-[QUERY_MODULES]

`./prometheus` `DataqueryBuilder` — the query row every metrics-backed panel target rides:

| [INDEX] | [MEMBER]                                       | [ROLE]                                             |
| :-----: | :--------------------------------------------- | :------------------------------------------------- |
|  [01]   | `.expr(expr)` / `.refId(id)`                   | the rendered `Query` string and its slot letter    |
|  [02]   | `.exemplar(bool)`                              | exemplar overlay — gated on the store row's column |
|  [03]   | `.legendFormat(f)` / `.instant()` / `.range()` | series labeling and query mode                     |
|  [04]   | `.datasource(ref)` / `.format(f)` / `.hide(b)` | binding and display posture                        |

`.format(f)` takes `PromQueryFormat` (`time_series | table | heatmap` as `PromQueryFormat.TimeSeries`/`.Table`/`.Heatmap`); `.instant()`/`.range()` are zero-argument mode selectors. `./loki` `DataqueryBuilder` is the Logs-panel target row — `.expr(expr)`, `.refId(id)`, `.legendFormat(f)`, `.maxLines(n)`, `.instant(b)`/`.range(b)`, `.datasource(ref)`.

## [04]-[INTEGRATION]

[STACK: `@rasm/ts/core` `DashboardModel`] — the model is the authority, the builder is the emitter: `_compiled` reads `typeof DashboardModel.Encoded`, maps model fields onto builder members one-for-one, and never invents a name, threshold, or layout — `DashboardModel.laid` positions become `gridPos`, rendered `Query` strings become `expr` rows, and a model field with no builder member fails the fold at compile time.

[STACK: `@pulumiverse/grafana` `oss.Dashboard`] — `.build()` output feeds `configJson` through `pulumi.jsonStringify`; `storeDashboardSha256: true` then diffs by content hash, so a builder-emitted byte change is exactly one drift row.

[STACK: version posture] — the emitted-JSON boundary insulates the provider from SDK churn: a builder bump re-emits JSON, the provider applies it, and no other surface moves.

## [05]-[RAIL_LAW]

- Package: `@grafana/grafana-foundation-sdk`
- Owns: typed dashboard construction — the builder members panel and query typing survive through
- Accept: builders inside `operate/observe.md`'s `_compiled` fold only; one builder row per core panel tag; `.build()` as the single emission seam
- Reject: hand-authored dashboard JSON where a builder subpath exists, builder use outside the deploy plane, a second model authority beside `DashboardModel`

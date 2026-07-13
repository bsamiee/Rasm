# [PY_ARTIFACTS_API_ALTAIR]

`altair` supplies the declarative Vega-Lite chart-specification surface for the artifacts visuals rail: a `Chart` builder with `mark_*` (17) / `encode` / `transform_*` (19) / `configure_*` (55) method families, the `param`/`when`/`condition`/`binding_*` interaction algebra, the `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` composition operators (mirrored by `+`/`|`/`&`), the `project` geographic configurator and `graticule`/`sphere`/`sequence`/`topo_feature` geo-data generators, the `resolve_axis`/`resolve_legend`/`resolve_scale` composite resolution, the narwhals-backed `transformed_data` local pre-execution, the `theme`/`renderers`/`data_transformers`/`vegalite_compilers` plugin registries, the `expr` Vega-expression algebra, the `JupyterChart` anywidget with bidirectional `Params`/`Selections` traitlet state, and the `to_dict`/`to_json`/`to_html`/`to_url`/`save` emit family that drive grammar-of-graphics chart construction from any narwhals-compatible frame (polars/pandas/pyarrow) into Vega-Lite JSON. The package owner composes the builder, its method families, the interaction surface, `transformed_data`, and the emit family into the visuals owner; it never re-implements the Vega-Lite grammar altair already owns nor hand-builds the spec dict where the builder is admitted. The spec stacks with the sibling admitted render band: `vl-convert-python` (`.api/vl-convert-python.md`) is the registered `vl-convert` compiler that lowers a spec to static SVG/PNG/JPEG/PDF/HTML bytes, `vegafusion` (`.api/vegafusion.md`) is the registered server-side `vegafusion` `data_transformer`/pre-aggregation engine for large datasets, and `great-tables`/`lets-plot` own the orthogonal display-table and host-free self-render paths — one `Chart` spec drives all backends, never a per-backend chart definition. The producer folds onto the universal rails: each chart contributes a `msgspec` (`.api/msgspec.md`) receipt under one `structlog` (`.api/structlog.md`) event inside an `opentelemetry` (`.api/opentelemetry-api.md`) span, a `SchemaValidationError` folds onto the `expression.Result` (`.api/expression.md`) rail, and every native render offloads through `anyio` (`.api/anyio.md`) `to_thread`/`to_process`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `altair`
- package: `altair`
- import: `altair` (canonical alias `import altair as alt`)
- owner: `artifacts`
- rail: visuals
- license: BSD-3-Clause (Vega-Altair Developers)
- installed: `6.2.2`
- asset: pure-Python wheel (`py3-none-any`, `Root-Is-Purelib: true`, no cp-gate, `Requires-Python>=3.10`). Hard deps `narwhals>=2.4.0` (frame interchange — the polars/pandas/pyarrow ingest backbone), `jinja2` (HTML template), `jsonschema>=3.0` (spec validation), `packaging`. The render/transform backends are the `extra='all'` optional set, NOT hard deps: `vl-convert-python>=1.9.0`, `vegafusion>=2.0.3`, `anywidget>=0.9.0`, `pandas`/`pyarrow`/`numpy` — admitted separately in the manifest and reached as the registered `vl-convert`/`vegafusion` plugins, so altair alone constructs and emits the spec dict but defers rasterization/pre-aggregation to those sibling owners
- entry points: none (library only); `to_dict`/`to_json` emit the spec, `Chart.save`/`to_html`/`to_url` and the `vl-convert`/`vegafusion` plugins perform export
- capability: declarative Vega-Lite v6 chart construction — 17 `mark_*` geometric marks, ~40 `encode` channels, 19 `transform_*` data transforms (including statistical `regression`/`loess`/`density`/`quantile`/`window`/`aggregate`), 55 `configure_*` theme arms, the `param`/`when`/`condition`/`binding_*` interaction algebra, `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` composition with `resolve_*` conflict resolution, the `project` geographic projection configurator, narwhals-backed `transformed_data` local pre-execution, the `expr` Vega-expression vocabulary, the `graticule`/`sphere`/`sequence`/`topo_feature` geo/numeric data generators, the data-utility family (`sample`/`limit_rows`/`to_values`/`to_json`/`to_csv`/`InlineDataset`), pluggable `theme`/`renderers`/`data_transformers`/`vegalite_compilers` registries, the `JupyterChart` anywidget with bidirectional `Params`/`Selections` state, the `VEGALITE_VERSION`/`VEGA_VERSION`/`VEGAEMBED_VERSION`/`SCHEMA_VERSION`/`SCHEMA_URL` version constants, and the `to_dict`/`to_json`/`to_html`/`to_url`/`open_editor`/`save` emit family (JSON/HTML/PNG/SVG/PDF/JPEG via the registered compiler)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart and composition roots
- rail: visuals

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [RAIL]                                                                                   |
| :-----: | :------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Chart`        | chart builder  | single-view chart with mark/encode/transform/param                                       |
|  [02]   | `LayerChart`   | layered chart  | overlaid views (`+` operator / `layer`)                                                  |
|  [03]   | `HConcatChart` | concat chart   | horizontal side-by-side composition (`\|` operator)                                      |
|  [04]   | `VConcatChart` | concat chart   | vertical stacked composition (`&` operator)                                              |
|  [05]   | `ConcatChart`  | concat chart   | wrappable grid composition                                                               |
|  [06]   | `FacetChart`   | faceted chart  | small-multiples by field                                                                 |
|  [07]   | `RepeatChart`  | repeated chart | repeat one spec across a field list (row/column/layer)                                   |
|  [08]   | `JupyterChart` | widget         | `anywidget` widget; `Params`/`Selections` mirror selection to Python; `enable_offline()` |

`VEGALITE_VERSION`/`VEGA_VERSION`/`VEGAEMBED_VERSION`/`SCHEMA_VERSION`/`SCHEMA_URL` pin the Vega-Lite/Vega/Vega-Embed/JSON-schema versions the `save`/`to_html` defaults bind and pass through to the `vl-convert` compiler so the rendered spec version matches the builder.

[PUBLIC_TYPE_SCOPE]: plugin registries
- rail: visuals

| [INDEX] | [REGISTRY]           | [REGISTERED]                                                                                                |
| :-----: | :------------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `data_transformers`  | `default`/`json`/`csv`/`vegafusion` (the `vegafusion` server-side pre-aggregation switch)                   |
|  [02]   | `renderers` display  | `html`/`json`/`png`/`svg`/`mimetype`/`browser`                                                              |
|  [03]   | `renderers` notebook | `jupyter`/`jupyterlab`/`colab`/`kaggle`/`nteract`/`zeppelin`/`olli`                                         |
|  [04]   | `vegalite_compilers` | `vl-convert` (the sole admitted compiler, enabled by default)                                               |
|  [05]   | `theme`              | `theme.enable`/`active`/`names()`/`get()`, `@theme.register` over `() -> ThemeConfig`, typed `*Kwds` family |

[PUBLIC_TYPE_SCOPE]: encoding channels, guides, and the interaction algebra
- rail: visuals

The encoding channel constructors (`X`/`Y`/`Color`/...) and guides (`Scale`/`Axis`/`Legend`) are typed value objects passed to `encode`; each accepts a `field`/`type` shorthand or full keyword config. `param` is the single canonical interaction primitive: a variable `param(value=, bind=, expr=)` binds a UI control or holds a derived expression, and `selection_point`/`selection_interval` are `param` factories returning point/interval selection `Parameter` objects; a `Parameter` carries `Parameter.to_dict()` for its spec reference (`Parameter.ref()` is a deprecated no-op — the bare object now references directly). `when(predicate).then(stmt).otherwise(stmt)` is the conditional-encoding chain; `Then.when(...)` adds an else-if branch via `ChainedWhen`, so a multi-case conditional is one fluent chain rather than nested `condition` calls. `value`/`datum` lift literals and data references into encodings. The legacy interaction members survive only as version-stamped `@utils.deprecated` aliases that the owner never mints — `selection_single`/`selection_multi` (release → `selection_point`), `selection(type=)` (→ `selection_point`/`selection_interval`), `Chart.add_selection` (release → `add_params`), `param(init=)` (→ `value`) — each names its exact replacement; canonical code uses `param`/`add_params`/`selection_point`/`selection_interval`/`when` only.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `X` / `Y` / `X2` / `Y2`           | channel       | positional encoding constructors                    |
|  [02]   | `XOffset` / `YOffset`             | channel       | sub-position offset (grouped/dodged marks)          |
|  [03]   | `Color` / `Fill` / `Stroke`       | channel       | color encoding (mark fill/stroke)                   |
|  [04]   | `Size` / `Opacity` / `Angle`      | channel       | size / opacity / rotation encoding                  |
|  [05]   | `Shape` / `Tooltip` / `Detail`    | channel       | shape, tooltip, and grouping-detail encoding        |
|  [06]   | `Theta` / `Radius`                | channel       | polar (arc/pie) encoding                            |
|  [07]   | `Latitude` / `Longitude`          | channel       | geographic encoding for `mark_geoshape`             |
|  [08]   | `Row` / `Column` / `Facet`        | channel       | facet-field encoding                                |
|  [09]   | `Order` / `Text` / `Href` / `Key` | channel       | sort order, text label, link, and join-key encoding |
|  [10]   | `Scale` / `Axis` / `Legend`       | guide         | scale, axis, and legend configuration               |
|  [11]   | `Bin` / `Impute` / `Sort`         | encoding spec | bin spec, imputation spec, and channel sort order   |

[PUBLIC_TYPE_SCOPE]: interaction algebra objects
- rail: visuals

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY]       | [RAIL]                                        |
| :-----: | :---------------------------------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `param`                                                           | parameter           | interaction primitive (variable/selection)    |
|  [02]   | `selection_point`                                                 | parameter           | point selection `param` factory               |
|  [03]   | `selection_interval`                                              | parameter           | interval (brush) selection `param` factory    |
|  [04]   | `when` / `condition`                                              | conditional         | `when().then().otherwise()` conditional       |
|  [05]   | `value` / `datum` / `expr`                                        | literal / ref       | encoding literal, datum ref, expr namespace   |
|  [06]   | `binding_range` / `binding_select` / `binding_radio`              | binding             | bind a `param` to an HTML input               |
|  [07]   | `binding_checkbox` / `binding`                                    | binding             | `binding(input, *, element=, name=)` generic  |
|  [08]   | `Data` / `InlineData` / `UrlData` / `NamedData` / `InlineDataset` | data source         | inline/URL/named dataset references           |
|  [09]   | `Parameter` / `When` / `Then` / `ChainedWhen`                     | interaction objects | `param`/selection + `when` chain return types |
|  [10]   | `topo_feature`                                                    | geo data ref        | TopoJSON collection ref for `mark_geoshape`   |
|  [11]   | `PredicateComposition` / `FieldName`                              | predicate / field   | predicate composition + field-name VO         |

## [03]-[ENTRYPOINTS]

`Chart(data)` admits any narwhals-native frame, a `pandas`/`polars`/`pyarrow` frame, a URL string, or a `Data` object. The `mark_*` family is the single geometric-mark axis (17 rows); `encode` binds fields to channels; `transform_*` is the single data-pipeline axis (19 rows, including the statistical `regression`/`loess`/`density`/`quantile`/`window`); `add_params` registers interaction `param`s; `properties`/`configure*` set view and theme config. `transform_regression` fits `method` ∈ `linear`/`log`/`exp`/`pow`/`quad`/`poly` with `order`/`params`, `params=True` emitting `rSquared`/coef rows; `transform_density` carries `counts`/`maxsteps`/`minsteps`/`resolve` beyond the shown knobs; the `mark_*`/`transform_*`/`configure_*` family rosters ride the fence below.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                    | [CAPABILITY]                           |
| :-----: | :--------------------------- | :-------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `Chart`                      | `Chart(data=None, *, mark, width, height, title, ...)`          | chart over a frame / URL / `Data`      |
|  [02]   | `Chart.mark_*`               | 17-mark family — roster in the fence below                      | set the geometric mark                 |
|  [03]   | `Chart.encode`               | `encode(*args, x, y, color, size, theta, tooltip, ...) -> Self` | bind fields to ~40 channels            |
|  [04]   | `Chart.transform_*`          | 19-transform family — roster in the fence below                 | server-side Vega transform pipeline    |
|  [05]   | `Chart.transform_regression` | `transform_regression(on, regression, *, ...) -> Self`          | regression trend; emits coef rows      |
|  [06]   | `Chart.transform_density`    | `transform_density(density, *, bandwidth, ...) -> Self`         | KDE density estimate as a transform    |
|  [07]   | `Chart.transform_extent`     | `transform_extent(extent, param) -> Self`                       | field extent INTO a named `param`      |
|  [08]   | `Chart.transform_loess`      | `transform_loess(on, loess, *, bandwidth, groupby) -> Self`     | loess smoother                         |
|  [09]   | `Chart.transform_quantile`   | `transform_quantile(quantile, probs, step, groupby) -> Self`    | quantile / Q-Q transform               |
|  [10]   | `Chart.add_params`           | `add_params(*params) -> Self`                                   | register interaction `param`s          |
|  [11]   | `Chart.interactive`          | `interactive(name=None, bind_x=True, bind_y=True) -> Self`      | default pan/zoom interval selection    |
|  [12]   | `Chart.properties`           | `properties(width=..., height=..., title=..., **kwds) -> Self`  | set view-level width/height/title      |
|  [13]   | `Chart.configure_*`          | 55-arm family — roster in the fence below                       | one arm per Vega-Lite config block     |
|  [14]   | `Chart.project`              | `project(type, center, rotate, scale, translate, ...) -> Self`  | projection for `mark_geoshape`         |
|  [15]   | `Chart.resolve_*`            | `resolve_scale` / `resolve_axis` / `resolve_legend`             | composite scale/axis/legend resolution |

```text
mark_* (17) — set the geometric mark, each taking mark-config kwargs
mark_bar mark_line mark_point mark_area mark_circle mark_square mark_tick mark_rect mark_rule
mark_text mark_arc mark_geoshape mark_image mark_trail mark_boxplot mark_errorbar mark_errorband

transform_* (19) — server-side Vega transforms chained left-to-right; statistical transforms run in Vega, never a hand-rolled numpy fit
transform_filter transform_calculate transform_aggregate transform_bin transform_fold transform_pivot
transform_window transform_joinaggregate transform_lookup transform_density transform_regression transform_loess
transform_quantile transform_impute transform_flatten transform_sample transform_stack transform_timeunit transform_extent

configure_* (55) — one arm per Vega-Lite config block, never a hand-merged dict
configure configure_axis (+ axisX/Y/Band/Top/Bottom/Left/Right/Discrete/Quantitative/Temporal/Point)
configure_legend configure_mark configure_<mark> configure_view configure_title configure_scale configure_range
configure_header configure_projection configure_selection configure_concat configure_facet
```

[ENTRYPOINT_SCOPE]: interaction algebra
- rail: visuals

`param` is the single interaction primitive; `selection_point`/`selection_interval` are its selection factories; `when().then().otherwise()` builds conditional encodings; `binding_*` attach UI controls. A typical interactive chart binds a `param` via `add_params`, then references it in `when(param)` inside an `encode` channel — the deprecated `add_selection`/`condition`-only path is never minted. Both selection factories carry `fields`/`encodings`/`on`/`clear`/`resolve`; `selection_point` adds `toggle`/`nearest` and `selection_interval` adds `mark`/`translate`/`zoom`. `binding_range` mirrors as `binding_select`/`binding_radio`/`binding_checkbox`/`binding`.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                 | [CAPABILITY]                    |
| :-----: | :------------------- | :--------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `param`              | `param(name, value, bind, empty, expr, **kwds) -> Parameter`                 | variable / selection parameter  |
|  [02]   | `selection_point`    | `selection_point(name, value, bind, ...) -> Parameter`                       | point selection (click/hover)   |
|  [03]   | `selection_interval` | `selection_interval(name, value, bind, ...) -> Parameter`                    | interval (brush) selection      |
|  [04]   | `when` / `Then.when` | `when(pred, *more, empty, **constraints) -> When`; `.then()`; `.otherwise()` | multi-case conditional builder  |
|  [05]   | `condition`          | `condition(predicate, if_true, if_false, *, empty) -> SchemaBase`            | single-shot conditional         |
|  [06]   | `binding_range`      | `binding_range(min, max, step, name)` (`binding_*` mirrors)                  | bind a `param` to an HTML input |

[ENTRYPOINT_SCOPE]: composition, transform execution, and export
- rail: visuals

`layer`/`hconcat`/`vconcat`/`concat` are the composition operators (mirrored by `+`/`|`/`&`); `repeat`/`facet` template one spec across a field list; `resolve_scale`/`resolve_axis`/`resolve_legend` resolve guide conflicts on the composite. `transformed_data` executes the transform pipeline locally through narwhals and returns the resulting frame (for inspection or hand-off — the in-process counterpart to the `vegafusion` server-side pre-pass). `to_dict`/`to_json` emit the spec; `to_html` renders a self-contained or CDN-linked HTML string; `save` is the single multi-format file export — `format` ∈ `json`/`html`/`png`/`svg`/`pdf`, `engine` selects `vl-convert` (default) or `vegafusion`, `inline=True` embeds JS for an offline HTML; `to_url`/`open_editor` mint or open a Vega-editor URL. The data utilities (`sample`/`limit_rows`/`to_values`/`to_json`/`to_csv`) are the `data_transformers` building blocks for custom dataset handling.

`save` binds `vegalite_version`/`vega_version`/`vegaembed_version` version defaults and `scale_factor`/`mode`/`embed_options`; `expr.<vega_fn>(...)`/`expr.<CONST>` build `FunctionExpression`/`ConstExpression`/`GetAttrExpression` ASTs (driven by `expr.FUNCTION_LISTING`/`NAME_MAP`/`CONST_LISTING`) feeding `transform_calculate`/`param(expr=)`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                | [CAPABILITY]                        |
| :-----: | :--------------------------------- | :---------------------------------------------------------- | :---------------------------------- |
|  [01]   | `layer`                            | `layer(*charts, **kwargs) -> LayerChart`                    | overlay chart views                 |
|  [02]   | `hconcat` / `vconcat`              | `hconcat(*charts)` / `vconcat(*charts)`                     | horizontal / vertical concat        |
|  [03]   | `concat`                           | `concat(*charts, **kwargs) -> ConcatChart`                  | wrappable grid concat               |
|  [04]   | `Chart.facet`                      | `facet(facet, row, column, columns, data)`                  | small-multiples by field            |
|  [05]   | `Chart.repeat`                     | `repeat(repeat, row, column, layer, columns)`               | repeat a spec across a field list   |
|  [06]   | `Chart.transformed_data`           | `transformed_data(row_limit=None, exclude=None) -> frame`   | local narwhals transform → frame    |
|  [07]   | `Chart.to_dict`                    | `to_dict(validate=True, *, format, ...) -> dict`            | emit the Vega-Lite spec dict        |
|  [08]   | `Chart.to_json`                    | `to_json(validate=True, indent=2, sort_keys=True) -> str`   | emit the spec JSON string           |
|  [09]   | `Chart.save`                       | `save(fp, format=None, engine=None, inline=False) -> None`  | file export json/html/png/svg/pdf   |
|  [10]   | `Chart.to_html`                    | `to_html(base_url, output_div, inline=False, ...) -> str`   | self-contained / CDN HTML string    |
|  [11]   | `Chart.to_url` / `open_editor`     | `to_url(*, fullscreen, validate)` / `open_editor(...)`      | mint / open a Vega-editor URL       |
|  [12]   | `theme.register`                   | `@theme.register(name, *, enable)` over `() -> ThemeConfig` | register / enable / inspect a theme |
|  [13]   | `data_transformers.enable`         | `data_transformers.enable('vegafusion'\|'default'\|...)`    | select dataset handling             |
|  [14]   | `graticule`                        | `graticule(**kwds) -> GraticuleGenerator`                   | geo graticule generator             |
|  [15]   | `sphere`                           | `sphere() -> SphereGenerator`                               | globe-outline generator             |
|  [16]   | `sequence`                         | `sequence(start, stop=None, step=..., as_=...)`             | numeric sequence generator          |
|  [17]   | `topo_feature`                     | `topo_feature(url, feature) -> UrlData`                     | TopoJSON feature reference          |
|  [18]   | `sample`                           | `sample(data=None, n=None, frac=None)`                      | `data_transformers` sampler         |
|  [19]   | `limit_rows`                       | `limit_rows(data=None, max_rows=5000)`                      | row-cap primitive                   |
|  [20]   | `to_values` / `to_json` / `to_csv` | serialize a frame into a `Data` payload                     | `data_transformers` serializers     |
|  [21]   | `expr`                             | `expr.<vega_fn>(...)` / `expr.<CONST>` -> ASTs              | typed Vega-expression algebra       |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_VEGALITE]:
- import: `import altair as alt` at boundary scope only; module-level import is banned by the manifest import policy.
- ingest axis: `Chart(data)` admits any narwhals-native frame; the frame interchange is delegated to `narwhals>=2.4.0` (altair's one hard frame dep), never re-implemented as a pandas-only or per-backend ingest path. pandas/pyarrow/numpy are the `extra='all'` optionals, not a default ingest assumption — the polars-native path is the canonical Rasm frame.
- chart axis: one `Chart` builder owns the single-view grammar; the 17 `mark_*`, ~40 `encode` channels, 19 `transform_*`, and 55 `configure_*` are method-family rows on the builder, never parallel chart types per mark; statistical transforms (`regression`/`loess`/`density`/`quantile`/`window`) run server-side in Vega, never a hand-rolled numpy fit pre-baked into the data; `mark_geoshape` is configured through `project` + the `graticule`/`sphere`/`topo_feature` generators, not a hand-built projection dict.
- interaction axis: `param` is the single interaction primitive registered via `add_params`; `selection_point`/`selection_interval` are its factories and `when(...).then(...).otherwise(...)` (with `Then.when(...)` else-if chaining) the conditional-encoding chain. The legacy aliases are version-stamped `@utils.deprecated` shims the owner never mints: `selection_single`/`selection_multi`/`selection` (→ `selection_point`/`selection_interval`), `Chart.add_selection` (release → `add_params`), `param(init=)` (→ `value`), `Parameter.ref()` (no-op), bare-`condition` for multi-case (→ the `when` chain).
- composition axis: `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` compose `Chart` instances into the composite roots (mirrored by `+`/`|`/`&`); `resolve_scale`/`resolve_axis`/`resolve_legend` resolve shared-vs-independent guide conflicts on the composite. Composition is an operator, never a duplicated chart definition.
- registry axis: `theme`/`renderers`/`data_transformers`/`vegalite_compilers` are the four plugin registries; a custom theme is a `@theme.register(name, *, enable)`-decorated `() -> ThemeConfig` factory (typed via the `theme.*Kwds` family), inspected through `theme.active`/`names()`/`get()`, not a manually-merged config dict; `renderers.enable(...)` selects from the registered `html`/`png`/`svg`/`json`/`mimetype`/`browser`/`jupyter*`/`colab`/`kaggle` set; `vegalite_compilers` holds the `vl-convert` compiler (default); `data_transformers.enable('vegafusion')` is the large-dataset server-side pre-aggregation switch.
- concurrency axis: the spec build is pure/in-process, but every native render and the `vegafusion` pre-pass offload through `anyio` (`.api/anyio.md`) — vl-convert and lets-plot ride `to_thread` (GIL-releasing native cores), the `vegafusion` pre-pass and matplotlib ride `to_process`, all under one `CapacityLimiter`, so no heavy native render blocks the event loop (the pattern `visualization/chart/export#EXPORT` lands).
- export axis: `to_dict`/`to_json` emit the spec and `transformed_data` executes the transform pipeline locally through narwhals; `to_html` (self-contained via `inline=True`) and `to_url`/`open_editor` cover the HTML/editor paths; `save` with `format` is the single file export — `engine` selects the `vl-convert` compiler (static SVG/PNG/JPEG/PDF) or `vegafusion` (pre-transformed), never a re-minted rasterizer or JS bundler. vl-convert exposes no external-dataset feed, so the `vegafusion` reduction inlines INSIDE the spec (a `vegalite_to_*` `inline_datasets=` kwarg does not exist — `.api/vl-convert-python.md`).
- evidence: each chart captures mark kind, encoded channel set, transform chain length, registered param names, active theme/renderer/compiler, output format, and output spec/byte size as a `msgspec.Struct` (`.api/msgspec.md`) visuals receipt — emitted under one `structlog` (`.api/structlog.md`) event inside an `opentelemetry` (`.api/opentelemetry-api.md`) span; a `SchemaValidationError` (`altair.utils.schemapi`) from `to_dict`/`to_json` validation folds onto the `expression.Result` (`.api/expression.md`) rail rather than raising into the producer. These universal `libs/python/.api` rails stack ON TOP of this folder catalog — the catalog documents the spec-construction surface, the rails own the receipt/log/span/result discipline around it.
- boundary: altair owns Vega-Lite spec construction; static rendering routes to `vl-convert-python` (`.api/vl-convert-python.md`, the registered `vl-convert` compiler) and large-data pre-transform to `vegafusion` (`.api/vegafusion.md`); publication display tables route to `great-tables`; host-free self-render to `lets-plot`; non-COLR SVG rasterization to `resvg-py`; a genuine columnar data export (not a render side channel) routes to `data/tabular/columnar#COLUMNAR`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `altair`
- Owns: declarative Vega-Lite v6 chart construction over narwhals frames, the `param`/`when` interaction algebra, statistical/geo transforms, `configure_*`/`project`/`resolve_*` configuration, composition, local `transformed_data` execution, the `expr` algebra, the `JupyterChart` bidirectional widget, and spec/multi-format export through the registered `vl-convert`/`vegafusion` backends
- Reject: wrapper-renames of `mark_*`/`encode`/`param`/`configure_*`; a hand-built Vega-Lite dict where the builder is admitted; the deprecated `selection_*`/`selection`/`add_selection`/`param(init=)` aliases where `param`/`when`/`add_params`/`value` are canonical; a pandas-only ingest where `narwhals` owns interchange; a re-minted rasterizer, JS bundler, or numpy statistical fit where `vl-convert`/server-side transforms render; an `inline_datasets=` feed to vl-convert that does not exist; identity minting the runtime owns

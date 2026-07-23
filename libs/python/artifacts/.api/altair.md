# [PY_ARTIFACTS_API_ALTAIR]

`altair` mints the declarative Vega-Lite chart-specification surface for the artifacts visuals rail: one `Chart` builder folds the `mark_*`/`encode`/`transform_*`/`configure_*` method families, the `param`/`when`/`condition`/`binding_*` interaction algebra, and the `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` operators into Vega-Lite JSON over any narwhals frame. altair constructs and emits the spec alone — `transformed_data` pre-executes transforms locally and the plugin registries own every backend; rasterization, pre-aggregation, and self-render defer to the sibling render band.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `altair`
- package: `altair` (BSD-3-Clause, Vega-Altair Developers)
- module: `altair` (canonical alias `import altair as alt`)
- owner: `artifacts`
- rail: visuals
- asset: pure-Python wheel (`py3-none-any`, `Root-Is-Purelib: true`, no cp-gate). Hard deps `narwhals` (polars/pandas/pyarrow frame interchange), `jinja2` (HTML template), `jsonschema` (spec validation), `packaging`; the render/transform backends `vl-convert-python`, `vegafusion`, `anywidget`, and `pandas`/`pyarrow`/`numpy` are optional extras reached only as registered plugins, admitted separately in the manifest.
- capability: declarative Vega-Lite chart construction over narwhals frames — the `mark_*`/`encode`/`transform_*`/`configure_*` builder families, the `param`/`when` interaction algebra, composition with `resolve_*` conflict resolution, the `project` geo configurator, local `transformed_data` execution, the `expr` algebra, the plugin registries, the `JupyterChart` widget, and the `to_dict`/`save` emit family.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart and composition roots

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [CAPABILITY]                                                                             |
| :-----: | :------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Chart`        | chart builder  | single-view chart with mark/encode/transform/param                                       |
|  [02]   | `LayerChart`   | layered chart  | overlaid views (`+` operator / `layer`)                                                  |
|  [03]   | `HConcatChart` | concat chart   | horizontal side-by-side composition (`\|` operator)                                      |
|  [04]   | `VConcatChart` | concat chart   | vertical stacked composition (`&` operator)                                              |
|  [05]   | `ConcatChart`  | concat chart   | wrappable grid composition                                                               |
|  [06]   | `FacetChart`   | faceted chart  | small-multiples by field                                                                 |
|  [07]   | `RepeatChart`  | repeated chart | repeat one spec across a field list (row/column/layer)                                   |
|  [08]   | `JupyterChart` | widget         | `anywidget` widget; `Params`/`Selections` mirror selection to Python; `enable_offline()` |

`VEGALITE_VERSION`/`VEGA_VERSION`/`VEGAEMBED_VERSION`/`SCHEMA_VERSION`/`SCHEMA_URL` set the target schema the emitted spec binds; `save`/`to_html` read them and pass them to the registered compiler so the render matches the builder.

[PUBLIC_TYPE_SCOPE]: plugin registries

| [INDEX] | [REGISTRY]           | [REGISTERED]                                                        |
| :-----: | :------------------- | :------------------------------------------------------------------ |
|  [01]   | `data_transformers`  | `default`/`json`/`csv`/`vegafusion` (server-side pre-aggregation)   |
|  [02]   | `renderers` display  | `html`/`json`/`png`/`svg`/`mimetype`/`browser`                      |
|  [03]   | `renderers` notebook | `jupyter`/`jupyterlab`/`colab`/`kaggle`/`nteract`/`zeppelin`/`olli` |
|  [04]   | `vegalite_compilers` | `vl-convert` (the sole admitted compiler, default-enabled)          |
|  [05]   | `theme`              | `@theme.register` over `() -> ThemeConfig`, typed `*Kwds` family    |

[PUBLIC_TYPE_SCOPE]: encoding channels, guides, and interaction objects

Encoding constructors (`X`/`Y`/`Color`/…) and guides (`Scale`/`Axis`/`Legend`) are typed value objects passed to `encode`, each taking a `field`/`type` shorthand or a full keyword config; `Parameter`/`When`/`Then`/`ChainedWhen` are the return types the `param`/selection factories and the `when` chain produce.

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY]       | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `X` / `Y` / `X2` / `Y2`                                           | channel             | positional encoding constructors              |
|  [02]   | `XOffset` / `YOffset`                                             | channel             | sub-position offset (grouped/dodged marks)    |
|  [03]   | `Color` / `Fill` / `Stroke`                                       | channel             | color encoding (mark fill/stroke)             |
|  [04]   | `Size` / `Opacity` / `Angle`                                      | channel             | size / opacity / rotation encoding            |
|  [05]   | `Shape` / `Tooltip` / `Detail`                                    | channel             | shape, tooltip, grouping-detail encoding      |
|  [06]   | `Theta` / `Radius`                                                | channel             | polar (arc/pie) encoding                      |
|  [07]   | `Latitude` / `Longitude`                                          | channel             | geographic encoding for `mark_geoshape`       |
|  [08]   | `Row` / `Column` / `Facet`                                        | channel             | facet-field encoding                          |
|  [09]   | `Order` / `Text` / `Href` / `Key`                                 | channel             | sort, text, link, and join-key encoding       |
|  [10]   | `Scale` / `Axis` / `Legend`                                       | guide               | scale, axis, and legend configuration         |
|  [11]   | `Bin` / `Impute` / `Sort`                                         | encoding spec       | bin, imputation, and channel-sort specs       |
|  [12]   | `Data` / `InlineData` / `UrlData` / `NamedData` / `InlineDataset` | data source         | inline / URL / named dataset references       |
|  [13]   | `Parameter` / `When` / `Then` / `ChainedWhen`                     | interaction objects | `param`/selection + `when` chain return types |
|  [14]   | `PredicateComposition` / `FieldName`                              | predicate / field   | predicate composition + field-name VO         |

## [03]-[ENTRYPOINTS]

`Chart(data)` admits any narwhals-native frame, a `pandas`/`polars`/`pyarrow` frame, a URL string, or a `Data` object, and every `Chart.*` builder method returns `Self` for chaining. `mark_*`, `transform_*`, and `configure_*` family rosters ride the fence below. `transform_regression` fits `method` ∈ `linear`/`log`/`exp`/`pow`/`quad`/`poly` with `order`/`params`, `params=True` emitting `rSquared`/coefficient rows; `transform_density` carries `counts`/`maxsteps`/`minsteps`/`resolve` beyond the shown knobs.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `Chart(data=None, *, mark, width, height, title, ...)`        | ctor     | chart over a frame / URL / `Data`                |
|  [02]   | `Chart.mark_*`                                                | instance | set the geometric mark; roster below             |
|  [03]   | `Chart.encode(*args, x, y, color, size, theta, tooltip, ...)` | instance | bind fields to encoding channels                 |
|  [04]   | `Chart.transform_*`                                           | instance | server-side Vega transform; roster below         |
|  [05]   | `Chart.transform_regression(on, regression, *, ...)`          | instance | regression trend; emits coefficient rows         |
|  [06]   | `Chart.transform_density(density, *, bandwidth, ...)`         | instance | KDE density estimate as a transform              |
|  [07]   | `Chart.transform_extent(extent, param)`                       | instance | field extent INTO a named `param`                |
|  [08]   | `Chart.transform_loess(on, loess, *, bandwidth, groupby)`     | instance | loess smoother                                   |
|  [09]   | `Chart.transform_quantile(quantile, probs, step, groupby)`    | instance | quantile / Q-Q transform                         |
|  [10]   | `Chart.add_params(*params)`                                   | instance | register interaction `param`s                    |
|  [11]   | `Chart.interactive(name=None, bind_x=True, bind_y=True)`      | instance | default pan/zoom interval selection              |
|  [12]   | `Chart.properties(width, height, title, **kwds)`              | instance | set view-level width/height/title                |
|  [13]   | `Chart.configure_*`                                           | instance | one arm per Vega-Lite config block; roster below |
|  [14]   | `Chart.project(type, center, rotate, scale, translate, ...)`  | instance | projection for `mark_geoshape`                   |
|  [15]   | `Chart.resolve_scale` / `resolve_axis` / `resolve_legend`     | instance | composite scale/axis/legend resolution           |

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

`param` is the single interaction primitive; `selection_point`/`selection_interval` are its factories carrying `fields`/`encodings`/`on`/`clear`/`resolve` (point adds `toggle`/`nearest`, interval adds `mark`/`translate`/`zoom`); `when().then().otherwise()` builds the conditional chain and `binding_*` attach UI controls. Every surface is a factory returning a `Parameter`, `When`, or binding object.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `param(name, value, bind, empty, expr, **kwds) -> Parameter`                 | variable / selection parameter  |
|  [02]   | `selection_point(name, value, bind, ...) -> Parameter`                       | point selection (click/hover)   |
|  [03]   | `selection_interval(name, value, bind, ...) -> Parameter`                    | interval (brush) selection      |
|  [04]   | `when(pred, *more, empty, **constraints) -> When`; `.then()`; `.otherwise()` | multi-case conditional builder  |
|  [05]   | `condition(predicate, if_true, if_false, *, empty) -> SchemaBase`            | single-shot conditional         |
|  [06]   | `binding_range(min, max, step, name)` (`binding_*` mirrors)                  | bind a `param` to an HTML input |

[ENTRYPOINT_SCOPE]: composition, transform execution, and export

`save` is the single multi-format file export: `format` ∈ `json`/`html`/`png`/`svg`/`pdf`, `engine` selects `vl-convert` (default) or `vegafusion`, and `inline=True` embeds JS for offline HTML. `expr.<vega_fn>(...)`/`expr.<CONST>` build `FunctionExpression`/`ConstExpression`/`GetAttrExpression` ASTs — driven by `expr.FUNCTION_LISTING`/`NAME_MAP`/`CONST_LISTING` — feeding `transform_calculate`/`param(expr=)`.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `layer(*charts, **kwargs) -> LayerChart`                    | factory  | overlay chart views                   |
|  [02]   | `hconcat(*charts)` / `vconcat(*charts)`                     | factory  | horizontal / vertical concat          |
|  [03]   | `concat(*charts, **kwargs) -> ConcatChart`                  | factory  | wrappable grid concat                 |
|  [04]   | `Chart.facet(facet, row, column, columns, data)`            | instance | small-multiples by field              |
|  [05]   | `Chart.repeat(repeat, row, column, layer, columns)`         | instance | repeat a spec across a field list     |
|  [06]   | `Chart.transformed_data(row_limit=None, exclude=None)`      | instance | local narwhals transform → frame      |
|  [07]   | `Chart.to_dict(validate=True, *, format, ...) -> dict`      | instance | emit the Vega-Lite spec dict          |
|  [08]   | `Chart.to_json(validate=True, indent=2, sort_keys=True)`    | instance | emit the spec JSON string             |
|  [09]   | `Chart.save(fp, format=None, engine=None, inline=False)`    | instance | file export json/html/png/svg/pdf     |
|  [10]   | `Chart.to_html(base_url, output_div, inline=False, ...)`    | instance | self-contained / CDN HTML string      |
|  [11]   | `Chart.to_url(*, fullscreen, validate)` / `open_editor()`   | instance | mint / open a Vega-editor URL         |
|  [12]   | `@theme.register(name, *, enable)` over `() -> ThemeConfig` | factory  | register / enable / inspect a theme   |
|  [13]   | `data_transformers.enable('vegafusion'\|'default'\|...)`    | instance | select dataset handling               |
|  [14]   | `graticule(**kwds) -> GraticuleGenerator`                   | factory  | geo graticule generator               |
|  [15]   | `sphere() -> SphereGenerator`                               | factory  | globe-outline generator               |
|  [16]   | `sequence(start, stop=None, step=..., as_=...)`             | factory  | numeric sequence generator            |
|  [17]   | `topo_feature(url, feature) -> UrlData`                     | factory  | TopoJSON feature reference            |
|  [18]   | `sample(data=None, n=None, frac=None)`                      | static   | `data_transformers` sampler           |
|  [19]   | `limit_rows(data=None, max_rows=5000)`                      | static   | row-cap primitive                     |
|  [20]   | `to_values` / `to_json` / `to_csv`                          | static   | `data_transformers` frame serializers |
|  [21]   | `expr.<vega_fn>(...)` / `expr.<CONST>` -> ASTs              | factory  | typed Vega-expression algebra         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Chart` builder owns the single-view grammar; `mark_*`, `encode`, `transform_*`, and `configure_*` are chainable method families returning `Self`, never parallel chart types per mark, and a new mark, transform, or config arm is one method on the builder.
- Statistical transforms (`regression`/`loess`/`density`/`quantile`/`window`/`aggregate`) run server-side in Vega; a numpy fit pre-baked into the data is the deleted form. `mark_geoshape` configures through `project` and the `graticule`/`sphere`/`topo_feature` generators, never a hand-built projection dict.
- `param` is the single interaction primitive registered via `add_params`; `selection_point`/`selection_interval` are its factories and `when(...).then(...).otherwise(...)` with `Then.when(...)` else-if chaining is the conditional-encoding chain, `value`/`datum` lifting literals and data refs. A `Parameter` references directly through `Parameter.to_dict()`.
- `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` compose `Chart` instances into the composite roots (mirrored by `+`/`|`/`&`); `resolve_scale`/`resolve_axis`/`resolve_legend` resolve shared-vs-independent guide conflicts. Composition is an operator over one spec, never a duplicated chart definition.
- `theme`/`renderers`/`data_transformers`/`vegalite_compilers` own every plugin; a custom theme is a `@theme.register`-decorated `() -> ThemeConfig` factory typed through the `theme.*Kwds` family, inspected via `theme.active`/`names()`/`get()`, never a hand-merged config dict.
- `import altair as alt` binds at boundary scope only; the frame interchange rides `narwhals` (the one hard frame dep), never a pandas-only or per-backend ingest path, and the polars-native frame is the canonical Rasm input.

[STACKING]:
- `vl-convert-python`(`.api/vl-convert-python.md`): the registered `vl-convert` `vegalite_compilers` entry lowers a `to_dict` spec to static SVG/PNG/JPEG/PDF/HTML bytes; it feeds no external dataset, so a `vegafusion` reduction inlines inside the spec (`inline_datasets=` does not exist on the compiler).
- `vegafusion`(`.api/vegafusion.md`): the registered `data_transformers.enable('vegafusion')` server-side pre-aggregation engine for large datasets; `transformed_data` is the in-process narwhals counterpart.
- `msgspec`(`.api/msgspec.md`): each chart captures mark kind, encoded channel set, transform-chain length, registered param names, active theme/renderer/compiler, output format, and spec/byte size as a `Struct` visuals receipt.
- `structlog`(`.api/structlog.md`) + `opentelemetry`(`.api/opentelemetry-api.md`): the receipt emits under one `structlog` event inside one `opentelemetry` span.
- `expression`(`.api/expression.md`): a `SchemaValidationError` from `to_dict`/`to_json` validation folds onto the `Result` rail rather than raising into the producer.
- `anyio`(`.api/anyio.md`): every native render offloads under one `CapacityLimiter` — `vl-convert`/`lets-plot` ride `to_thread`, the `vegafusion` pre-pass and matplotlib ride `to_process`, so no native render blocks the event loop.
- within-lib: the visuals owner composes the `Chart` builder, its method families, the interaction surface, `transformed_data`, and the emit family into one spec that drives every backend, never a per-backend chart definition.

[LOCAL_ADMISSION]:
- altair is admitted for Vega-Lite spec construction; static rendering routes to `vl-convert-python`, large-data pre-transform to `vegafusion`, publication display tables to `great-tables`, host-free self-render to `lets-plot`, non-COLR SVG rasterization to `resvg-py`, and a genuine columnar data export to the columnar data owner; live UI stays outside this package.

[RAIL_LAW]:
- Package: `altair`
- Owns: declarative Vega-Lite chart construction over narwhals frames — the `param`/`when` interaction algebra, statistical/geo transforms, `configure_*`/`project`/`resolve_*` configuration, composition, local `transformed_data` execution, the `expr` algebra, the `JupyterChart` bidirectional widget, and spec/multi-format export through the registered `vl-convert`/`vegafusion` backends.
- Accept: a `Chart` spec built from the admitted method families, emitted through `to_dict`/`save`, and rendered by the registered compiler.
- Reject: a hand-built Vega-Lite dict where the builder is admitted; a pandas-only ingest where `narwhals` owns interchange; a re-minted rasterizer, JS bundler, or numpy statistical fit where `vl-convert` and server-side transforms render; an `inline_datasets=` feed to `vl-convert` that does not exist; identity minting the runtime owns.

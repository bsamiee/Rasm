# [PY_ARTIFACTS_API_ALTAIR]

`altair` supplies the declarative Vega-Lite chart-specification surface for the artifacts visuals rail: a `Chart` builder with mark/encode/transform method families, the `param`/`when`/`condition` interaction algebra, composition operators, and a multi-format save path that drive grammar-of-graphics chart construction from any narwhals-compatible frame (polars/pandas/pyarrow) into Vega-Lite JSON. The package owner composes `Chart`, the mark/encode/transform families, the `param`/`when` interaction surface, `transformed_data`, and `to_dict`/`save` into the visuals owner; it never re-implements the Vega-Lite grammar altair already owns. The spec stacks with the sibling admitted render band: `vl-convert-python` is the registered `vl-convert` compiler that lowers a spec to static PNG/SVG/PDF bytes, `vegafusion` is the registered server-side `data_transformer`/pre-aggregation engine for large datasets, and `great-tables`/`lets-plot` own the orthogonal display-table and host-free self-render paths — one `Chart` spec drives all backends, never a per-backend chart definition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `altair`
- package: `altair`
- import: `altair`
- owner: `artifacts`
- rail: visuals
- version: `6.2.1`; license `BSD-3-Clause` (Vega-Altair Developers); pure-Python, markerless in the manifest (cp315-clean)
- deps: dataframe ingest is delegated to `narwhals` (polars/pandas/pyarrow/any narwhals-native frame, cp315-clean); spec validation uses `jsonschema` (cp315-clean); static export delegates to `vl-convert-python` (cp315-clean, in-process); server-side pre-transform to `vegafusion` (`python_version<'3.15'`-gated, sub-3.15 worker only)
- entry points: none (library only); `Chart.save` / `vl-convert` / `vegafusion` perform export
- capability: declarative Vega-Lite v6 chart construction — 17 `mark_*` geometric marks, ~40 encoding channels, 19 `transform_*` data transforms (including statistical `regression`/`loess`/`density`/`quantile`/`window`/`aggregate`), the `param`/`when`/`condition`/`binding_*` interaction algebra, `layer`/`hconcat`/`vconcat`/`facet`/`repeat` composition, narwhals-backed `transformed_data` pre-execution, the `expr` Vega-expression vocabulary, the `graticule`/`sphere`/`sequence` data generators, pluggable `theme`/`renderers`/`data_transformers`/`vegalite_compilers` registries, and multi-format `save` to JSON/HTML/PNG/SVG/PDF

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart and composition roots
- rail: visuals

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [RAIL]                                                |
| :-----: | :-------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `Chart`               | chart builder   | single-view chart with mark/encode/transform/param    |
|  [02]   | `LayerChart`          | layered chart   | overlaid views (`+` operator / `layer`)               |
|  [03]   | `HConcatChart`        | concat chart    | horizontal side-by-side composition (`\|` operator)   |
|  [04]   | `VConcatChart`        | concat chart    | vertical stacked composition (`&` operator)           |
|  [05]   | `ConcatChart`         | concat chart    | wrappable grid composition                            |
|  [06]   | `FacetChart`          | faceted chart   | small-multiples by field                              |
|  [07]   | `RepeatChart`         | repeated chart  | repeat one spec across a field list (row/column/layer) |
|  [08]   | `JupyterChart`        | widget          | anywidget-backed interactive widget with bidirectional param state |
|  [09]   | `data_transformers`   | plugin registry | dataset handling axis (`default`/`json`/`csv`/`vegafusion`) |
|  [10]   | `renderers`           | plugin registry | output renderer axis (`html`/`json`/`png`/`svg`/`jupyter`/`mimetype`/...) |
|  [11]   | `vegalite_compilers`  | plugin registry | spec-to-vega compiler axis (`vl-convert`)             |
|  [12]   | `theme`               | plugin registry | named chart theme axis + `theme.register` decorator + `theme.ThemeConfig` (a `@theme.register(name, *, enable)`-decorated function returns one) |

[PUBLIC_TYPE_SCOPE]: encoding channels, guides, and the interaction algebra
- rail: visuals

The encoding channel constructors (`X`/`Y`/`Color`/...) and guides (`Scale`/`Axis`/`Legend`) are typed value objects passed to `encode`; each accepts a `field`/`type` shorthand or full keyword config. `param` is the single canonical interaction primitive (the modern replacement for the deprecated `selection_*`/`add_selection`): a variable `param(value=, bind=)` binds a UI control, and `selection_point`/`selection_interval` are `param` factories that return point/interval selection `Parameter` objects. `when(predicate).then(stmt).otherwise(stmt)` is the modern conditional-encoding chain that supersedes the bare `condition(...)` call; `value`/`datum` lift literals and data references into encodings.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `X` / `Y` / `X2` / `Y2`           | channel       | positional encoding constructors                        |
|  [02]   | `XOffset` / `YOffset`             | channel       | sub-position offset (grouped/dodged marks)              |
|  [03]   | `Color` / `Fill` / `Stroke`       | channel       | color encoding (mark fill/stroke)                       |
|  [04]   | `Size` / `Opacity` / `Angle`      | channel       | size / opacity / rotation encoding                      |
|  [05]   | `Shape` / `Tooltip` / `Detail`    | channel       | shape, tooltip, and grouping-detail encoding            |
|  [06]   | `Theta` / `Radius`                | channel       | polar (arc/pie) encoding                                |
|  [07]   | `Latitude` / `Longitude`          | channel       | geographic encoding for `mark_geoshape`                 |
|  [08]   | `Row` / `Column` / `Facet`        | channel       | facet-field encoding                                    |
|  [09]   | `Order` / `Text` / `Href` / `Key` | channel       | sort order, text label, link, and join-key encoding     |
|  [10]   | `Scale` / `Axis` / `Legend`       | guide         | scale, axis, and legend configuration                   |
|  [11]   | `Bin` / `Impute` / `Sort`         | encoding spec | bin spec, imputation spec, and channel sort order       |
|  [12]   | `param`                           | parameter     | canonical interaction primitive (variable / selection)  |
|  [13]   | `selection_point`                 | parameter     | point selection `param` factory                         |
|  [14]   | `selection_interval`              | parameter     | interval (brush) selection `param` factory              |
|  [15]   | `when` / `condition`              | conditional   | predicate-driven conditional encoding (`when().then().otherwise()`) |
|  [16]   | `value` / `datum` / `expr`        | literal / ref | encoding-value literal, datum reference, Vega-expression namespace |
|  [17]   | `binding_range` / `binding_select` / `binding_radio` / `binding_checkbox` | binding | bind a `param` to an HTML input control |
|  [18]   | `Data` / `InlineData` / `UrlData` / `NamedData` | data source | inline / URL / named dataset references     |

## [03]-[ENTRYPOINTS]

`Chart(data)` admits any narwhals-native frame, a `pandas`/`polars`/`pyarrow` frame, a URL string, or a `Data` object. The `mark_*` family is the single geometric-mark axis (17 rows); `encode` binds fields to channels; `transform_*` is the single data-pipeline axis (19 rows, including the statistical `regression`/`loess`/`density`/`quantile`/`window`); `add_params` registers interaction `param`s; `properties`/`configure*` set view and theme config.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                                          | [CAPABILITY]                                            |
| :-----: | :------------------ | :------------------------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `Chart`             | `Chart(data=None, *, mark=..., width=..., height=..., title=..., ...)`                | build a chart over a frame / URL / `Data`               |
|  [02]   | `Chart.mark_*`      | `mark_bar` / `mark_line` / `mark_point` / `mark_area` / `mark_circle` / `mark_square` / `mark_tick` / `mark_rect` / `mark_rule` / `mark_text` / `mark_arc` / `mark_geoshape` / `mark_image` / `mark_trail` / `mark_boxplot` / `mark_errorbar` / `mark_errorband` | set the geometric mark (each takes mark-config kwargs)  |
|  [03]   | `Chart.encode`      | `encode(*args, x=..., y=..., color=..., size=..., theta=..., tooltip=..., ...) -> Self` | bind data fields to ~40 encoding channels             |
|  [04]   | `Chart.transform_*` | `transform_filter` / `transform_calculate` / `transform_aggregate` / `transform_bin` / `transform_fold` / `transform_pivot` / `transform_window` / `transform_joinaggregate` / `transform_lookup` / `transform_density` / `transform_regression` / `transform_loess` / `transform_quantile` / `transform_impute` / `transform_flatten` / `transform_sample` / `transform_stack` / `transform_timeunit` / `transform_extent` | data transform pipeline (server-side Vega transforms) |
|  [05]   | `Chart.transform_regression` | `transform_regression(on, regression, method='linear', order=..., extent=..., groupby=..., params=...) -> Self` | fit a regression (`linear`/`log`/`exp`/`pow`/`quad`/`poly`) trend |
|  [06]   | `Chart.transform_density` | `transform_density(density, bandwidth=..., cumulative=..., counts=..., extent=..., groupby=..., steps=...) -> Self` | KDE density estimate as a transform                |
|  [07]   | `Chart.add_params`  | `add_params(*params) -> Self`                                                         | register interaction `param`s on the chart              |
|  [08]   | `Chart.interactive` | `interactive(name=None, bind_x=True, bind_y=True) -> Self`                            | bind a default pan/zoom interval selection              |
|  [09]   | `Chart.properties`  | `properties(width=..., height=..., title=..., **kwds) -> Self`                        | set view-level width/height/title                       |
|  [10]   | `Chart.configure_*` | `configure(...)` / `configure_axis(...)` / `configure_legend(...)` / `configure_mark(...)` / `configure_view(...)` / `configure_title(...)` | top-level theme/config (per-component arms) |

[ENTRYPOINT_SCOPE]: interaction algebra
- rail: visuals

`param` is the single interaction primitive; `selection_point`/`selection_interval` are its selection factories; `when().then().otherwise()` builds conditional encodings; `binding_*` attach UI controls. A typical interactive chart binds a `param` via `add_params`, then references it in `when(param)` inside an `encode` channel — the deprecated `add_selection`/`condition`-only path is never minted.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                 | [CAPABILITY]                                            |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `param`                | `param(name=None, value=..., bind=..., empty=..., expr=..., **kwds) -> Parameter`            | a variable / selection parameter                        |
|  [02]   | `selection_point`      | `selection_point(name=None, fields=..., encodings=..., on=..., nearest=..., toggle=..., bind=..., ...) -> Parameter` | point selection `param`               |
|  [03]   | `selection_interval`   | `selection_interval(name=None, encodings=..., mark=..., translate=..., zoom=..., ...) -> Parameter` | interval (brush) selection `param`                |
|  [04]   | `when`                 | `when(predicate=..., *more, empty=..., **constraints) -> When`; chained `.then(stmt).otherwise(stmt)` | conditional-encoding builder                  |
|  [05]   | `condition`            | `condition(predicate, if_true, if_false, *, empty=...) -> SchemaBase`                        | single-shot conditional encoding                        |
|  [06]   | `binding_range`        | `binding_range(min=..., max=..., step=..., name=...)` (`binding_select`/`binding_radio`/`binding_checkbox` mirrors) | bind a `param` to an HTML input         |

[ENTRYPOINT_SCOPE]: composition, transform execution, and export
- rail: visuals

`layer`/`hconcat`/`vconcat`/`concat` are the composition operators (mirrored by `+`/`|`/`&`); `repeat`/`facet` template one spec across a field list. `transformed_data` executes the transform pipeline locally through narwhals and returns the resulting frame (for inspection or hand-off). `to_dict`/`to_json` emit the spec; `save` is the single multi-format export — `engine` selects `vl-convert` (default) or `vegafusion`, `inline=True` embeds JS for an offline HTML.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                                       | [CAPABILITY]                                          |
| :-----: | :--------------------- | :----------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `layer`                | `layer(*charts, **kwargs) -> LayerChart`                                                                           | overlay multiple chart views                          |
|  [02]   | `hconcat` / `vconcat`  | `hconcat(*charts) -> HConcatChart`; `vconcat(*charts) -> VConcatChart`                                             | horizontal / vertical concatenation                   |
|  [03]   | `concat`               | `concat(*charts, **kwargs) -> ConcatChart`                                                                         | wrappable grid concatenation                          |
|  [04]   | `Chart.facet`          | `facet(facet=..., row=..., column=..., columns=..., data=...) -> FacetChart`                                       | small-multiples by field                              |
|  [05]   | `Chart.repeat`         | `repeat(repeat=..., row=..., column=..., layer=..., columns=...) -> RepeatChart`                                   | repeat one spec across a field list                   |
|  [06]   | `Chart.transformed_data` | `transformed_data(row_limit=None, exclude=None) -> DataFrameLike \| None`                                        | execute transforms locally (narwhals) -> frame        |
|  [07]   | `Chart.to_dict`        | `to_dict(*, format='vega-lite', ...) -> dict`                                                                     | emit the Vega-Lite spec as dict                       |
|  [08]   | `Chart.to_json`        | `to_json(...) -> str`                                                                                             | emit spec as JSON string                              |
|  [09]   | `Chart.save`           | `save(fp, format=None ('json'/'html'/'png'/'svg'/'pdf'), scale_factor=1.0, engine=None, inline=False, embed_options=None, **kwargs) -> None` | export to file via `vl-convert`/`vegafusion` |
|  [10]   | `Chart.to_url`         | `to_url(...) -> str`                                                                                              | shareable Vega editor URL                             |
|  [11]   | `theme.register`       | `@theme.register(name, *, enable)` decorating a `() -> ThemeConfig` factory; `theme.enable(name)`                | register / enable a named chart theme                 |
|  [12]   | `data_transformers.enable` | `data_transformers.enable('vegafusion' \| 'default' \| 'json' \| 'csv')`                                       | select dataset handling (server-side pre-aggregation) |
|  [13]   | `graticule` / `sphere` / `sequence` | `graticule(extent=..., precision=..., step=...) -> GraticuleGenerator`; `sphere() -> SphereGenerator`; `sequence(start, stop, step=...) -> SequenceGenerator` | generator-data sources for `mark_geoshape` (geo graticule / globe outline) and numeric sequences |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_VEGALITE]:
- import: `import altair as alt` at boundary scope only; module-level import is banned by the manifest import policy.
- ingest axis: `Chart(data)` admits any narwhals-native frame (polars/pandas/pyarrow); the frame interchange is delegated to `narwhals`, never re-implemented as a pandas-only or per-backend ingest path.
- chart axis: one `Chart` builder owns the single-view grammar; the 17 `mark_*`, ~40 `encode` channels, and 19 `transform_*` are method-family rows on the builder, never parallel chart types per mark; statistical transforms (`regression`/`loess`/`density`/`quantile`/`window`) run server-side in Vega, never a hand-rolled numpy fit pre-baked into the data.
- interaction axis: `param` is the single interaction primitive registered via `add_params`; `selection_point`/`selection_interval` are its factories and `when().then().otherwise()` the conditional-encoding chain — the deprecated `add_selection`/bare-`condition` and `selection`/`selection_single`/`selection_multi` family is never minted.
- composition axis: `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` compose `Chart` instances into the composite roots (mirrored by `+`/`|`/`&`); composition is an operator, never a duplicated chart definition.
- registry axis: `theme`/`renderers`/`data_transformers`/`vegalite_compilers` are the four plugin registries; a custom theme is a `@theme.register(name, *, enable)`-decorated factory returning a `theme.ThemeConfig`, not a manually-merged config dict; `data_transformers.enable('vegafusion')` is the large-dataset server-side pre-aggregation switch.
- export axis: `to_dict`/`to_json` emit the spec; `transformed_data` executes the transform pipeline locally through narwhals; `save` with `format` is the single export entry — `engine` selects the `vl-convert` compiler (static PNG/SVG/PDF) or `vegafusion` (pre-transformed), and `inline=True` produces an offline HTML, never a re-minted rasterizer or JS bundler.
- runtime seam: altair itself is markerless and builds the spec on the cp315 core (narwhals is cp315-clean, `transformed_data` executes locally), but the gated render backends differ — `vl-convert-python` is cp315-clean and rasterizes in-process, whereas `data_transformers.enable('vegafusion')` and the `lets-plot` sibling are `python_version<'3.15'`-gated; the vegafusion pre-aggregation crosses the runtime `anyio.to_process` (`.api/anyio.md`) subprocess seam onto the sub-3.15 worker (the `Chart` spec — a JSON-serializable dict from `to_dict` — is what crosses, never a live builder), while the `lets-plot` self-render rides the gated band IN-PROCESS (`PlotSpec.to_*` is pure-Python over the bundled core, no subprocess hop — see `.api/lets-plot.md`).
- evidence: each chart captures mark kind, encoded channel set, transform chain length, registered param names, active theme/renderer/compiler, output format, and output spec/byte size as a `msgspec.Struct` (`.api/msgspec.md`) visuals receipt — emitted under one `structlog` (`.api/structlog.md`) event inside an OpenTelemetry (`.api/opentelemetry-api.md`) span; a `SchemaValidationError` (`altair.utils.schemapi`) from `to_dict` validation folds onto the `expression.Result` (`.api/expression.md`) rail rather than raising into the producer.
- boundary: altair owns Vega-Lite spec construction; static rendering routes to `vl-convert-python` (the `vl-convert` compiler) and large-data pre-transform to `vegafusion`; publication display tables route to `great-tables`; host-free self-render to `lets-plot`; non-COLR SVG rasterization to `resvg-py`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `altair`
- Owns: declarative Vega-Lite v6 chart construction over narwhals frames, the `param`/`when` interaction algebra, statistical transforms, composition, local transform execution, and spec/multi-format export through the `vl-convert`/`vegafusion` backends
- Accept: grammar-of-graphics chart specs built on cp315-core feeding the visuals owner; in-process `vl-convert` rasterization on cp315, the gated `vegafusion` pre-aggregation dispatched over the runtime `anyio.to_process` subprocess seam (the `to_dict` spec crosses, bytes return), the orthogonal gated `lets-plot` self-render running in-process on the sub-3.15 band
- Reject: wrapper-renames of `mark_*`/`encode`/`param`; a hand-built Vega-Lite dict where the builder is admitted; the deprecated `selection_*`/`add_selection` interaction family where `param`/`when` is canonical; a pandas-only ingest where narwhals owns interchange; a re-minted rasterizer or numpy statistical fit where `vl-convert`/server-side transforms render; identity minting the runtime owns

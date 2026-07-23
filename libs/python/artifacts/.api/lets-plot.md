# [PY_ARTIFACTS_API_LETS_PLOT]

`lets-plot` mints the host-free grammar-of-graphics chart surface for the artifacts visuals rail: `ggplot` + `aes` seed a `PlotSpec`, the `geom_*`/`stat_*`/`scale_*`/`position_*`/`coord_*`/`facet_*`/`theme*` grammar families `+`-compose onto it, and `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` + `lets_plot.export.ggsave` self-render to bytes or files in-process. It is a complete self-render engine — unlike `altair` (`.api/altair.md`), which builds a Vega-Lite spec for `vl-convert-python` (`.api/vl-convert-python.md`) to lower — rendering with no browser, Node, or Vega binary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lets-plot`
- package: `lets-plot`
- module: `lets_plot` (`lets_plot.export`, `lets_plot.bistro`)
- owner: `artifacts`
- rail: visuals
- asset: cp-tagged native wheel bundling the Kotlin/JS grammar-and-render core; SVG and HTML serialize self-contained, PNG and PDF rasterize the rendered SVG through `pillow` (`.api/pillow.md`) in-process, and a `geom_livemap` map plot serializes to interactive HTML only.
- capability: declarative ggplot2-grammar charts over a polars/pandas or dict frame — the `geom_*`/`stat_*` layers, the `scale_*`/`scale_*_manual` scale algebra, `position_*` adjustments, `sampling_*` reduction, `coord_*`/`facet_*`/`theme*`/`flavor_*`/`element_*` modifiers, `guide_*`/`labs`/`lims` annotation, the `gggrid`/`ggbunch`/`ggdeck`/`ggmarginal` composition roots, the `lets_plot.bistro` recipes, and in-process `PlotSpec.to_*` + `ggsave` self-render export.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plot and composition roots

`ggplot(data, mapping)` returns a `PlotSpec`; every `geom_*`/`stat_*`/`scale_*`/`coord_*`/`facet_*`/`theme*`/`guide_*`/`labs` call returns a `FeatureSpec`/`LayerSpec` that `+`-composes onto it. `PlotSpec` and `SupPlotsSpec` own the `to_*` serializers, and `mapping` is the `aes` alias.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [CAPABILITY]                                |
| :-----: | :---------------- | :---------------- | :------------------------------------------ |
|  [01]   | `PlotSpec`        | plot builder      | single-view ggplot grammar builder          |
|  [02]   | `FeatureSpec`     | feature spec      | base spec the grammar calls return          |
|  [03]   | `LayerSpec`       | layer spec        | a `geom_*`/`stat_*` layer feature           |
|  [04]   | `SupPlotsSpec`    | composite root    | grid/overlay/region composition             |
|  [05]   | `LetsPlot`        | library options   | init and options owner (`setup_html`/`set`) |
|  [06]   | `aes` / `mapping` | aesthetic mapping | `aes(x, y, **kwargs)` channel mapping       |
|  [07]   | `layer`           | raw layer         | low-level `geom`/`stat` layer constructor   |

[PUBLIC_TYPE_SCOPE]: geometry layers, scales, and styling modifiers

`geom_*`/`stat_*` are the layer axis, `scale_*` the scale-mapping axis, `position_*`/`coord_*`/`facet_*`/`theme*`/`flavor_*`/`element_*` the modifiers, and `guide_*`/`labs`/`lims` the annotation grammar; `scale_color_manual`/`scale_fill_manual` are the palette-injection members `DERIVE` reaches.

| [INDEX] | [SYMBOL]                                                                              | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `geom_point` / `geom_jitter` / `geom_line` / `geom_path`                              | point/jitter/line/path geometry                |
|  [02]   | `geom_step` / `geom_curve` / `geom_spoke`                                             | step/curve/spoke geometry                      |
|  [03]   | `geom_bar` / `geom_lollipop` / `geom_histogram` / `geom_freqpoly`                     | bar/lollipop/histogram geometry                |
|  [04]   | `geom_dotplot` / `geom_ydotplot` / `geom_pie`                                         | dot-plot (x & y) / pie geometry                |
|  [05]   | `geom_area` / `geom_area_ridges` / `geom_ribbon`                                      | area/ridgeline/ribbon geometry                 |
|  [06]   | `geom_band` / `geom_density`                                                          | band / KDE-density geometry                    |
|  [07]   | `geom_tile` / `geom_raster` / `geom_bin2d` / `geom_hex`                               | tile/raster/2D-bin/hexbin geometry             |
|  [08]   | `geom_count` / `geom_pointdensity`                                                    | count-bubble / point-density geometry          |
|  [09]   | `geom_boxplot` / `geom_violin` / `geom_sina` / `geom_crossbar`                        | box/violin/sina/crossbar geometry              |
|  [10]   | `geom_pointrange` / `geom_linerange` / `geom_errorbar`                                | point-range / error-bar geometry               |
|  [11]   | `geom_smooth` / `geom_density2d` / `geom_density2df`                                  | regression / 2D-density geometry               |
|  [12]   | `geom_contour` / `geom_contourf` / `geom_function`                                    | contour / analytic-function geometry           |
|  [13]   | `geom_qq` / `geom_qq_line` / `geom_qq2` / `geom_qq2_line`                             | quantile-quantile geometry                     |
|  [14]   | `geom_text` / `geom_label` / `geom_text_repel`                                        | text / label annotation                        |
|  [15]   | `geom_label_repel` / `geom_bracket` / `geom_bracket_dodge`                            | repel-label / bracket annotation               |
|  [16]   | `geom_rect` / `geom_segment` / `geom_polygon` / `geom_map`                            | rect/segment/polygon/map geometry              |
|  [17]   | `geom_imshow` / `geom_blank`                                                          | image-raster / scale-training-blank            |
|  [18]   | `geom_abline` / `geom_hline` / `geom_vline`                                           | reference-line geometry                        |
|  [19]   | `geom_livemap`                                                                        | interactive base-map (`maptiles_*`); HTML only |
|  [20]   | `stat_summary` / `stat_summary_bin` / `stat_ecdf` / `stat_sum`                        | standalone-statistic layer                     |
|  [21]   | `scale_color_manual` / `scale_fill_manual`                                            | canonical palette injection (`DERIVE`)         |
|  [22]   | `scale_shape_manual` / `scale_size_manual` / `scale_alpha_manual`                     | shape/size/alpha manual mapping                |
|  [23]   | `scale_linetype_manual` / `scale_manual`                                              | linetype / generic manual mapping              |
|  [24]   | `scale_color_gradient` / `scale_color_gradient2`                                      | two-stop / diverging color scale               |
|  [25]   | `scale_color_gradientn` / `scale_color_viridis`                                       | n-stop / viridis color scale                   |
|  [26]   | `scale_color_brewer` / `scale_color_grey` / `scale_color_hue` / `scale_color_cmapmpl` | ColorBrewer/grey/hue/colormap scale            |
|  [27]   | `scale_color_continuous` / `scale_color_discrete` / `scale_color_identity`            | default/identity color scale                   |
|  [28]   | `scale_alpha` / `scale_size` / `scale_size_area`                                      | alpha / size aesthetic scale                   |
|  [29]   | `scale_shape` / `scale_linewidth` / `scale_stroke`                                    | shape/linewidth/stroke scale                   |
|  [30]   | `scale_x_continuous` / `scale_x_discrete` / `scale_x_log10`                           | x-axis transform scale (mirrors `scale_y_*`)   |
|  [31]   | `scale_x_log2` / `scale_x_reverse` / `scale_x_datetime` / `scale_x_time`              | x-axis log/reverse/datetime scale              |
|  [32]   | `scale_x_discrete_reversed`                                                           | x-axis reversed discrete scale                 |
|  [33]   | `position_dodge` / `position_dodgev` / `position_stack` / `position_fill`             | per-layer `position=` adjustment               |
|  [34]   | `position_jitter` / `position_jitterdodge` / `position_nudge`                         | jitter/nudge `position=` adjustment            |
|  [35]   | `coord_cartesian` / `coord_fixed` / `coord_flip` / `coord_map` / `coord_polar`        | coordinate-system modifier                     |
|  [36]   | `facet_grid` / `facet_wrap`                                                           | small-multiples facet modifier                 |
|  [37]   | `theme` / `theme_bw` / `theme_minimal` / `theme_minimal2`                             | base config + predefined theme                 |
|  [38]   | `theme_classic` / `theme_light` / `theme_grey` / `theme_gray`                         | predefined theme                               |
|  [39]   | `theme_void` / `theme_none`                                                           | predefined empty theme                         |
|  [40]   | `element_text` / `element_line` / `element_rect`                                      | typed `theme(...)`-slot values                 |
|  [41]   | `element_geom` / `element_markdown` / `element_blank`                                 | typed slot values (`element_blank()`)          |
|  [42]   | `flavor_darcula` / `flavor_solarized_dark` / `flavor_solarized_light`                 | color-flavor scheme                            |
|  [43]   | `flavor_high_contrast_dark` / `flavor_high_contrast_light` / `flavor_standard`        | high-contrast/standard flavor                  |
|  [44]   | `guide_legend` / `guide_colorbar` / `guides`                                          | legend/colorbar guide (`guides`)               |
|  [45]   | `ggsize` / `labs` / `ggtitle` / `xlab` / `ylab`                                       | view size / axis+title labels                  |
|  [46]   | `lims` / `xlim` / `ylim` / `expand_limits`                                            | axis limits                                    |
|  [47]   | `margin` / `arrow` / `as_discrete` / `layer_tooltips` / `layer_labels`                | margins/arrows/tooltips                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar construction

`LetsPlot.setup_html()` initializes the in-process HTML render context once at boundary scope; every grammar surface is a module factory returning a `FeatureSpec`/`LayerSpec` that `+`-composes onto the `PlotSpec`, and `setup_html`/`set`/`set_theme` are static config.

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `ggplot(data=None, mapping=None) -> PlotSpec`                               | seed a plot over a frame / column dict + `aes` mapping |
|  [02]   | `aes(x=None, y=None, **kwargs) -> FeatureSpec`                              | map columns to channels via `**kwargs`                 |
|  [03]   | `geom_*(mapping=None, *, data=, stat=, position=, ...) -> LayerSpec`        | add a geometry layer                                   |
|  [04]   | `stat_*(mapping=None, *, data=, geom=, position=, fun=, ...) -> LayerSpec`  | add a standalone-statistic layer with explicit `geom=` |
|  [05]   | `geom_smooth(mapping=None, *, method=, se=, span=, deg=, ...) -> LayerSpec` | bundled-core LOESS/LM/GLM fit                          |
|  [06]   | `geom_function(mapping=None, *, fun=, xlim=, n=, ...) -> LayerSpec`         | sample an analytic `fun` over `xlim` into a curve      |
|  [07]   | `geom_imshow(image_data, cmap=None, *, norm=, vmin=, vmax=, ...)`           | render a 2D/3D numpy array as an image raster          |
|  [08]   | `scale_color_manual(values, name=, breaks=, labels=, ...) -> FeatureSpec`   | inject a discrete palette/shape/size mapping           |
|  [09]   | `scale_color_gradient(...)` / `scale_color_viridis(...) -> FeatureSpec`     | continuous color scale (two-stop/viridis)              |
|  [10]   | `scale_x_continuous(name=None, *, breaks=, labels=, limits=, ...)`          | positional axis transform / formatting scale           |
|  [11]   | `position_dodge(...)` / `position_jitterdodge(...)` / `position_nudge(...)` | the value passed to a layer's `position=` arm          |
|  [12]   | `facet_grid(x=, y=, scales=)` / `facet_wrap(facets, ncol=, nrow=)`          | small-multiples facet modifier                         |
|  [13]   | `theme(*, line=, rect=, text=, title=, axis=, ...) -> FeatureSpec`          | base theme config (slots `element_*`)                  |
|  [14]   | `element_*(...)` / `element_blank()` / `flavor_darcula()`                   | typed `theme(...)` slot value / color-flavor modifier  |
|  [15]   | `guide_legend(...)` / `guide_colorbar(...)` / `guides(**aes)`               | per-scale guide config bundled by aesthetic            |
|  [16]   | `labs(title=, subtitle=, caption=, ...)` / `ggtitle(...)` / `lims(x, y)`    | axis/title/legend labels + axis limits                 |
|  [17]   | `ggsize(width, height) -> FeatureSpec`                                      | set the plot pixel size                                |
|  [18]   | `LetsPlot.setup_html(...)` / `set` / `set_theme`                            | init the render context; global config                 |

[ENTRYPOINT_SCOPE]: composition

Each factory returns a `SupPlotsSpec` that is a valid `ggsave` input owning its own `to_*` serializers; `ggmarginal` attaches a marginal plot to one side of a base `PlotSpec`.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `gggrid(plots, ncol=None, *, sharex=, sharey=, ...) -> SupPlotsSpec`         | combine plots in a regular grid                         |
|  [02]   | `ggbunch(plots, regions) -> SupPlotsSpec` (regions `(x, y, w, h[, dx, dy])`) | combine plots with a custom-region layout               |
|  [03]   | `ggdeck(plots, *, scale_share=None) -> SupPlotsSpec`                         | overlay plots with aligned drawing areas                |
|  [04]   | `ggmarginal(sides, *, size=None, layer) -> FeatureSpec`                      | attach a marginal plot (`layer` is a required `geom_*`) |

[ENTRYPOINT_SCOPE]: large-data reduction

Each factory returns the value passed to a geom/stat's `sampling=` arm, capping the points the bundled core renders before serialization.

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `sampling_random(n, seed=None) -> object`                                | uniform random down-sample to `n` rows                      |
|  [02]   | `sampling_random_stratified(n, seed=None, min_subsample=None) -> object` | per-group stratified random down-sample                     |
|  [03]   | `sampling_systematic(n) -> object`                                       | every-k-th systematic down-sample                           |
|  [04]   | `sampling_pick(n) -> object`                                             | keep the first `n` distinct x-values' groups                |
|  [05]   | `sampling_group_random(n, seed=None)` / `sampling_group_systematic(n)`   | group-wise random / systematic sampling                     |
|  [06]   | `sampling_vertex_dp(n)` / `sampling_vertex_vw(n)`                        | polygon vertex simplification (Douglas-Peucker/Visvalingam) |

[ENTRYPOINT_SCOPE]: high-level recipe plots

Each `lets_plot.bistro` constructor returns a fully-assembled `PlotSpec` or `SupPlotsSpec` from one call, composing the same `geom_*`/`stat_*`/`scale_*` grammar internally.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `corr_plot(data, show_legend=True, flip=True, ...)` then `.points()/.tiles()/.labels()` | correlation-matrix figure (builder-chained)  |
|  [02]   | `qq_plot(data=None, sample=None, *, x=, y=, distribution=, ...) -> PlotSpec`            | quantile-quantile diagnostic (1- & 2-sample) |
|  [03]   | `joint_plot(data, x, y, *, geom=, bins=, color_by=, reg_line=, ...) -> PlotSpec`        | bivariate scatter + marginal distributions   |
|  [04]   | `residual_plot(data=None, x=None, y=None, *, method='lm', deg=1, ...) -> PlotSpec`      | regression-residual diagnostic figure        |
|  [05]   | `waterfall_plot(data, x, y, *, measure=, group=, sorted_value=, ...) -> PlotSpec`       | financial/contribution waterfall figure      |
|  [06]   | `image_matrix(image_data_array, *, norm=None, scale=1, ...) -> SupPlotsSpec`            | grid of numpy-array image rasters            |

[ENTRYPOINT_SCOPE]: in-process export

`to_svg`/`to_png`/`to_pdf`/`to_html`/`as_dict`/`props` are instance methods on the plot object, mirrored on `SupPlotsSpec`, and `export#EXPORT` `LP_RENDER` keys the serializers; `ggsave` is the module factory for format-by-extension file export. lets-plot ships no `to_jpeg` — the JPEG lane rasterizes the lets-plot SVG through the `vl-convert-python` resvg core, and `as_dict` is the receipt evidence.

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `to_svg(path=None, w=None, h=None, unit=None) -> str`                       | render to SVG (file-like `path` -> bytes/`None`)         |
|  [02]   | `to_png(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str`      | render to PNG via `pillow` (file-like `path` -> bytes)   |
|  [03]   | `to_pdf(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str`      | render to PDF via `pillow` (file-like `path` -> bytes)   |
|  [04]   | `to_html(path=None, iframe=None) -> str`                                    | render to HTML (file-like `path` -> bytes/`None`)        |
|  [05]   | `as_dict() -> dict`                                                         | recursively-resolved plot-spec dict (receipt evidence)   |
|  [06]   | `props()` / `has_layers()` / `duplicate() -> PlotSpec`                      | spec-property map / layer probe / deep copy              |
|  [07]   | `ggsave(plot, filename, *, path=None, iframe=True, scale=None, ...) -> str` | canonical multi-format file export (format by extension) |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ggplot(data, aes(...))` admits a polars/pandas frame or a `{column: sequence}` dict; `aes(x, y, **kwargs)` binds columns to aesthetic channels, every channel beyond `x`/`y` arriving as a kwarg.
- One `PlotSpec` owns the single-view grammar; the `geom_*`/`stat_*` layers, `scale_*` scales, `position_*` adjustments, `coord_*`/`facet_*` modifiers, and `theme*`/`flavor_*`/`element_*` styling are `+`-composed `FeatureSpec`/`LayerSpec` objects, never a parallel chart type per geom.
- Statistical geoms and the `stat_*` layers run the statistic in the bundled Kotlin/JS core; `lets_plot.bistro` recipes assemble the same grammar from one call.
- `sampling_*` is the per-layer `sampling=` data cap — the host-free analogue of `vegafusion` (`.api/vegafusion.md`) server-side pre-aggregation — so a large frame renders without a browser or external feed.
- `gggrid`/`ggbunch`/`ggdeck` fold `PlotSpec` instances into one `SupPlotsSpec` composite root, `ggmarginal` attaching a marginal view; composition is a constructor over a plot list.
- `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` are the single per-format byte serializers (mirrored on `SupPlotsSpec`); a file-like `path` (`io.BytesIO()`) captures the rendered bytes in-memory and returns `None`, a filename returns the pathname, and `ggsave` infers the format from the `.svg`/`.png`/`.pdf`/`.html` extension.
- One `Palette` array threads `scale_color_manual`/`scale_fill_manual` exactly as it threads the Vega and matplotlib bands, so the one-color-source invariant holds across every backend.
- `PlotSpec` construction stays on the worker lane, never the runtime; the raw spec crosses the boundary already built.

[STACKING]:
- `pillow`(`.api/pillow.md`): `to_png`/`to_pdf` drive it as the in-process raster backend over the rendered SVG; SVG and HTML need no raster hop.
- `vl-convert-python`(`.api/vl-convert-python.md`): the JPEG lane rasterizes the lets-plot SVG through its resvg core, since lets-plot exposes no `to_jpeg`.
- `anyio`(`.api/anyio.md`): the native render rides `to_thread` under one `CapacityLimiter`, GIL-releasing off the runtime event loop.
- `msgspec`(`.api/msgspec.md`): each render captures geom/stat-layer kinds, the mapped channels (read from `PlotSpec.as_dict`), the registered manual-palette values, active theme/flavor, output format, and byte length as a `Struct` visuals receipt, projected onto `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` — engine the matched `ChartSpec.tag` `"lets_plot"`, dialect the `ExportFormat` value, scale/theme the `RenderPolicy` knobs.
- `structlog`(`.api/structlog.md`) + `opentelemetry`(`.api/opentelemetry-api.md`): the receipt emits under one `structlog` event inside one span.
- `expression`(`.api/expression.md`): an export failure — a bad `path`, a raster format without `pillow`, a `geom_livemap` non-HTML request — folds onto the `Result` rail rather than raising into the producer.
- `graphic/color/derive#DERIVE` + `visualization/chart/spec#CHART`: `DERIVE` `ColorReceipt.coords` threads the `Palette` through `scale_color_manual`/`scale_fill_manual` (lazily imported by `export#EXPORT`) and the shared `hex_ramp` RGB-to-hex projection on `CHART`.
- within-lib: `export#EXPORT` `LP_RENDER` maps `ExportFormat -> method-name` and keys the `to_*` serializers, and `ChartSpec.LetsPlot(plot, palette)` on `spec#CHART` carries the raw `PlotSpec` to the worker lane, detected by `type(engine).__module__.startswith("lets_plot")`.

[LOCAL_ADMISSION]:
- lets-plot is admitted for host-free ggplot2-grammar self-render; the declarative-Vega path routes to `altair`→`vl-convert-python`→`vegafusion`, publication display tables to `great-tables` (`.api/great-tables.md`), standalone SVG-to-PNG rasterization to `resvg-py` (`.api/resvg-py.md`), and a `geom_livemap` interactive map UI or live web UI stays outside this package.

[RAIL_LAW]:
- Package: `lets-plot`
- Owns: declarative ggplot2-grammar chart construction over polars/pandas frames — the `geom_*`/`stat_*` layer family, the `scale_*`/`scale_*_manual` scale algebra, `position_*` adjustments, `sampling_*` large-data reduction, `coord_*`/`facet_*`/`theme*`/`flavor_*`/`element_*` modifiers, `guide_*`/`labs`/`lims` annotation, the `gggrid`/`ggbunch`/`ggdeck`/`ggmarginal` composition roots, the `lets_plot.bistro` recipes, and the in-process `PlotSpec.to_*` + `ggsave` self-render export.
- Accept: a `PlotSpec` built from the grammar families, `+`-composed, and self-rendered through `to_*`/`ggsave` on the worker lane, its `Palette` injected by `scale_*_manual`.
- Reject: a wrapper-rename of `ggplot`/`geom_*`/`scale_*`; a per-geom parallel plot type where `+`-composition layers; a hand-rolled numpy fit where the bundled core and `stat_*` render; a producer-side pre-decimation where `sampling_*` caps the layer; an ad-hoc per-engine color pick where `scale_*_manual` injects the `Palette`; a Vega or `vl-convert` route where lets-plot self-renders (the JPEG resvg hop excepted); a headless-browser, Cairo, or ImageMagick raster path where `to_png`/`to_pdf` rasterize via `pillow`; constructing the `PlotSpec` on the runtime instead of the worker lane; identity minting the runtime owns.

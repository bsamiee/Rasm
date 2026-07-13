# [PY_ARTIFACTS_API_LETS_PLOT]

`lets-plot` supplies the second host-free grammar-of-graphics chart surface for the artifacts visuals rail: a ggplot2-grammar builder (`ggplot` + `aes` + the 58-member `geom_*` layer family + the `stat_*` standalone-statistic, `scale_*` mapping, `position_*` adjustment, `coord_*`/`facet_*` coordinate-and-facet, `theme`/`theme_*`/`flavor_*`/`element_*` styling, and `guide_*`/`labs`/`lims` annotation modifiers, all composed by the `+` operator onto a `PlotSpec`) whose `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` serializers and the `lets_plot.export.ggsave` multi-format export render entirely in-process to bytes/files with no browser, no Node, and no Vega binary. The package owner composes `ggplot`, the `geom_*`/`stat_*`/`scale_*_manual`/`sampling_*` builder family, the `LetsPlot` initialization owner, and the `PlotSpec.to_*`/`ggsave` serializers into the `visualization/chart/spec#CHART` `ChartSpec.LetsPlot(plot, palette)` case rendered on the worker lane by `visualization/chart/export#EXPORT` `LP_RENDER`; it never re-implements the grammar-of-graphics layer algebra or the SVG/PNG/PDF serialization lets-plot already owns. It is the orthogonal self-render path beside the Vega band — `vl-convert-python` (`.api/vl-convert-python.md`) lowers an `altair` (`.api/altair.md`) Vega-Lite spec, `vegafusion` (`.api/vegafusion.md`) pre-transforms it, `great-tables` (`.api/great-tables.md`) owns publication tables, `resvg-py` (`.api/resvg-py.md`) owns standalone SVG-to-PNG — where lets-plot self-renders its own grammar to bytes, so one host-free engine never substitutes for another and `graphic/color/derive#DERIVE` threads the same `Palette` array into every backend via `scale_color_manual`/`scale_fill_manual`.

This catalog is tagged `(source-route, cp314-reflected)`: the distribution is admitted `; python_version<'3.15'` (no cp315 wheel — wheels ship cp310–cp314/cp314t only), so on the cp315 reflection core `api resolve lets-plot` returns `unsupported`. Every member, signature, and family count below is reflected from the consumer-equivalent `cp314` wheel of the current `4.11.0` release (the asset the gate binds until a cp315 wheel lands — its public surface is the surface the cp315 build will expose) and cross-checked against the published `lets_plot` / `lets_plot.export` / `lets_plot.bistro` API reference, never from training-data recall.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lets-plot`
- package: `lets-plot`
- import: `lets_plot`
- owner: `artifacts`
- rail: visuals
- version: `4.11.0`
- marker: `; python_version<'3.15'` — admitted gated (no cp315 wheel; the gate drops when one lands)
- deps: SVG/HTML serialization is self-contained (bundled Kotlin/JS core compiled to the native render path); PNG and PDF export require `pillow` (`.api/pillow.md`) — the in-process raster backend lets-plot drives to rasterize the rendered SVG, NOT a headless browser, Cairo, or ImageMagick; lets-plot ships no `to_jpeg`, so the JPEG export lane rasterizes the lets-plot SVG through the shared `vl-convert-python` (`.api/vl-convert-python.md`) resvg core, never pillow; `geom_livemap` map plots serialize to interactive HTML only
- entry points: none (library only); `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` and `lets_plot.export.ggsave` perform export
- capability: declarative ggplot2-grammar chart construction — `ggplot(data, aes(...))` over a polars/pandas frame or `{column: sequence}` dict, the 58-member `geom_*` layer family (statistical `geom_smooth`/`geom_density`/`geom_density2d`/`geom_density2df`/`geom_boxplot`/`geom_violin`/`geom_sina`/`geom_histogram`/`geom_freqpoly`/`geom_bin2d`/`geom_hex`/`geom_contour`/`geom_contourf`/`geom_qq`/`geom_qq2`/`geom_dotplot`/`geom_ydotplot`/`geom_area_ridges`/`geom_count`/`geom_pointdensity`; analytic `geom_function`/`geom_abline`/`geom_hline`/`geom_vline`; the `geom_imshow`/`geom_raster`/`geom_map`/`geom_livemap` raster/map geoms; plus point/line/bar/area/segment/ribbon/text-label families), the `stat_*` standalone-statistic layers (`stat_summary`/`stat_summary_bin`/`stat_ecdf`/`stat_sum`), the full `scale_*`/`scale_*_manual`/`scale_*_gradient*`/`scale_*_viridis`/`scale_*_brewer`/`scale_*_identity` scale algebra, the `position_*` adjustment family (`position_dodge`/`position_stack`/`position_fill`/`position_jitter`/`position_jitterdodge`/`position_nudge`/`position_dodgev`), the `sampling_*` large-data reduction family, `coord_*`/`facet_grid`/`facet_wrap` coordinate and facet modifiers, `theme`/`theme_*`/`flavor_*`/`element_*`/`ggsize` styling, the `guide_legend`/`guide_colorbar`/`guides`/`labs`/`lims`/`margin`/`as_discrete`/`layer_tooltips` annotation grammar, the `gggrid`/`ggbunch`/`ggdeck`/`ggmarginal` plot-composition roots, the `lets_plot.bistro` high-level `corr_plot`/`qq_plot`/`joint_plot`/`residual_plot`/`waterfall_plot`/`image_matrix` recipe constructors, and the in-process `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` byte serializers plus the format-by-extension `ggsave` multi-format export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plot and composition roots
- rail: visuals

`ggplot(data=None, mapping=None)` returns a `PlotSpec` (`lets_plot.plot.core`); `geom_*`/`stat_*`/`scale_*`/`coord_*`/`facet_*`/`theme*`/`guide_*`/`labs` calls return `FeatureSpec`/`LayerSpec` objects that compose onto a `PlotSpec` via the `+` operator (the grammar is operator-composed, never a parallel per-geom plot type). `aes(x=None, y=None, **kwargs)` is the channel-mapping object — every aesthetic beyond `x`/`y` (`color`/`fill`/`size`/`shape`/`alpha`/`group`/`linetype`/`paint_a`/...) arrives through `**kwargs`, never a fixed keyword roster. `LetsPlot` is the library-options/initialization owner. `gggrid`/`ggbunch`/`ggdeck` all compose multiple `PlotSpec` instances into one `SupPlotsSpec` composite root that `ggsave` also accepts; `GGBunch` (`lets_plot.plot.plot`) is the legacy custom-region composite still accepted by `ggsave`, superseded by `ggbunch` which now returns `SupPlotsSpec`.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [RAIL]                                                                                           |
| :-----: | :---------------- | :---------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `PlotSpec`        | plot builder      | single-view ggplot grammar (`lets_plot.plot.core`); `+`-composed; owns the `to_*` serializers    |
|  [02]   | `FeatureSpec`     | feature spec      | base spec the grammar calls return, `+`-added onto a `PlotSpec`; `FeatureSpecArray` bundles      |
|  [03]   | `LayerSpec`       | layer spec        | a `geom_*`/`stat_*` layer feature added onto a `PlotSpec`                                        |
|  [04]   | `SupPlotsSpec`    | composite root    | grid/overlay/region composition from `gggrid`/`ggdeck`/`ggbunch`; owns its `to_*` serializers    |
|  [05]   | `GGBunch`         | composite root    | legacy custom-region composition (`lets_plot.plot.plot`); `ggsave` input superseded by `ggbunch` |
|  [06]   | `LetsPlot`        | library options   | init/options owner; `setup_html`/`set`/`set_theme`/`setup_show_ext`/`NO_JS`/`OFFLINE`            |
|  [07]   | `aes` / `mapping` | aesthetic mapping | `aes(x=None, y=None, **kwargs)` channel-mapping for `ggplot`/`geom_*`/`stat_*`; `mapping` alias  |
|  [08]   | `layer`           | raw layer         | `layer(geom=, stat=, ...)` low-level constructor under the `geom_*`/`stat_*` shorthands          |

[PUBLIC_TYPE_SCOPE]: geometry layers, scales, and styling modifiers
- rail: visuals

The 58-member `geom_*` family is the single geometric-layer axis; `stat_*` the standalone-statistic layer axis; the 68-member `scale_*` family the single scale-mapping axis; `position_*` the per-layer position-adjustment axis; `coord_*`/`facet_*`/`theme*`/`flavor_*`/`element_*` the coordinate/facet/style modifiers; `guide_*`/`guides`/`labs`/`lims`/`margin`/`arrow`/`as_discrete` the annotation/guide grammar. Every one returns a `FeatureSpec`/`LayerSpec` composed onto a `PlotSpec` by `+`, never a parallel chart type. `scale_color_manual`/`scale_fill_manual` are the canonical palette-injection members the `visualization/chart/spec#CHART` palette-thread reaches (imported lazily by `visualization/chart/export#EXPORT`); the bundled core runs every `stat`-bearing geom's statistic, never a hand-rolled numpy fit.

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
- rail: visuals

`ggplot(data, mapping)` seeds a `PlotSpec` over a polars/pandas frame or a `{column: sequence}` dict; `aes(x, y, **kwargs)` maps columns to aesthetic channels (every channel beyond `x`/`y` is a kwarg); the `geom_*`/`stat_*` family adds layers; `scale_*`/`position_*`/`coord_*`/`facet_*`/`theme*`/`guide_*` modify the result — all composed by the `+` operator, never a parallel per-geom plot type. `LetsPlot.setup_html()` initializes the in-process HTML render context once at boundary scope.

| [INDEX] | [CALL_SHAPE]                                                                 | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `ggplot(data=None, mapping=None) -> PlotSpec`                                | seed a plot over a frame / column dict + `aes` mapping  |
|  [02]   | `aes(x=None, y=None, **kwargs) -> FeatureSpec`                               | map columns to channels via `**kwargs`                  |
|  [03]   | `geom_*(mapping=None, *, data=, stat=, position=, ...) -> LayerSpec`         | add a geometry layer (`+`-composed onto the `PlotSpec`) |
|  [04]   | `stat_*(mapping=None, *, data=, geom=, position=, fun=, ...) -> LayerSpec`   | add a standalone-statistic layer with explicit `geom=`  |
|  [05]   | `geom_smooth(mapping=None, *, method=, se=, span=, deg=, ...) -> LayerSpec`  | bundled-core LOESS/LM/GLM fit, no numpy fit             |
|  [06]   | `geom_function(mapping=None, *, fun=, xlim=, n=, ...) -> LayerSpec`          | sample an analytic `fun` over `xlim` into a curve       |
|  [07]   | `geom_imshow(image_data, cmap=None, *, norm=, vmin=, vmax=, ...)`            | render a 2D/3D numpy array as an image raster           |
|  [08]   | `scale_color_manual(values, name=, breaks=, labels=, ...) -> FeatureSpec`    | inject a discrete palette/shape/size mapping            |
|  [09]   | `scale_color_gradient(...)` / `scale_color_viridis(...) -> FeatureSpec`      | continuous color scale (two-stop/viridis)               |
|  [10]   | `scale_x_continuous(name=None, *, breaks=, labels=, limits=, ...)`           | positional axis transform / formatting scale            |
|  [11]   | `position_dodge(...)` / `position_jitterdodge(...)` / `position_nudge(...)`  | the value passed to a layer's `position=` arm           |
|  [12]   | `facet_grid(x=, y=, scales=, ...)` / `facet_wrap(facets, ncol=, nrow=, ...)` | small-multiples facet modifier                          |
|  [13]   | `theme(*, line=, rect=, text=, title=, axis=, ...) -> FeatureSpec`           | base theme config (slots `element_*`)                   |
|  [14]   | `element_*(...)` / `element_blank()` / `flavor_darcula()`                    | typed `theme(...)` slot value / color-flavor modifier   |
|  [15]   | `guide_legend(...)` / `guide_colorbar(...)` / `guides(**aes) -> FeatureSpec` | per-scale guide config bundled by aesthetic             |
|  [16]   | `labs(title=, subtitle=, caption=, ...)` / `ggtitle(...)` / `lims(x, y)`     | axis/title/legend labels + axis limits                  |
|  [17]   | `ggsize(width, height) -> FeatureSpec`                                       | set the plot pixel size                                 |
|  [18]   | `LetsPlot.setup_html(...)` / `set` / `set_theme`                             | init the in-process HTML render context; global config  |

[ENTRYPOINT_SCOPE]: composition
- rail: visuals

`gggrid`/`ggbunch`/`ggdeck` compose multiple `PlotSpec` instances into the `SupPlotsSpec` composite root — composition is a constructor over a plot list, never a duplicated plot definition. The composite root is also a valid `ggsave` input and owns its own `to_svg`/`to_png`/`to_pdf`/`to_html` serializers. `ggmarginal` attaches a marginal plot to one side of a base `PlotSpec`.

| [INDEX] | [CALL_SHAPE]                                                                 | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `gggrid(plots, ncol=None, *, sharex=, sharey=, ...) -> SupPlotsSpec`         | combine plots in a regular grid                         |
|  [02]   | `ggbunch(plots, regions) -> SupPlotsSpec` (regions `(x, y, w, h[, dx, dy])`) | combine plots with a custom-region layout               |
|  [03]   | `ggdeck(plots, *, scale_share=None) -> SupPlotsSpec`                         | overlay plots with aligned drawing areas                |
|  [04]   | `ggmarginal(sides, *, size=None, layer) -> FeatureSpec`                      | attach a marginal plot (`layer` is a required `geom_*`) |

[ENTRYPOINT_SCOPE]: large-data reduction
- rail: visuals

`sampling_*` is the layer-level data-reduction axis passed through a geom/stat's `sampling=` arm — the host-free analogue of the Vega band's `vegafusion` (`.api/vegafusion.md`) server-side pre-aggregation: it caps the points the bundled core renders before serialization, so a million-row frame charts without a browser or an external data feed. Reduction is a `sampling=` value, never a hand-rolled pre-decimation on the producer side.

| [INDEX] | [CALL_SHAPE]                                                             | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `sampling_random(n, seed=None) -> object`                                | uniform random down-sample to `n` rows                      |
|  [02]   | `sampling_random_stratified(n, seed=None, min_subsample=None) -> object` | per-group stratified random down-sample                     |
|  [03]   | `sampling_systematic(n) -> object`                                       | every-k-th systematic down-sample                           |
|  [04]   | `sampling_pick(n) -> object`                                             | keep the first `n` distinct x-values' groups                |
|  [05]   | `sampling_group_random(n, seed=None)` / `sampling_group_systematic(n)`   | group-wise random / systematic sampling                     |
|  [06]   | `sampling_vertex_dp(n)` / `sampling_vertex_vw(n)`                        | polygon vertex simplification (Douglas-Peucker/Visvalingam) |

[ENTRYPOINT_SCOPE]: high-level recipe plots
- rail: visuals

`lets_plot.bistro` is the high-level statistical-recipe layer — each constructor returns a fully-assembled `PlotSpec` (or `SupPlotsSpec`) from a single call, the host-free analogue of a publication-ready figure recipe. They compose the same `geom_*`/`stat_*`/`scale_*` grammar internally and self-render through the identical `PlotSpec.to_*` path; the owner reaches them for a settled chart kind instead of re-assembling the grammar.

| [INDEX] | [CALL_SHAPE]                                                                            | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `corr_plot(data, show_legend=True, flip=True, ...)` then `.points()/.tiles()/.labels()` | correlation-matrix figure (builder-chained)  |
|  [02]   | `qq_plot(data=None, sample=None, *, x=, y=, distribution=, ...) -> PlotSpec`            | quantile-quantile diagnostic (1- & 2-sample) |
|  [03]   | `joint_plot(data, x, y, *, geom=, bins=, color_by=, reg_line=, ...) -> PlotSpec`        | bivariate scatter + marginal distributions   |
|  [04]   | `residual_plot(data=None, x=None, y=None, *, method='lm', deg=1, ...) -> PlotSpec`      | regression-residual diagnostic figure        |
|  [05]   | `waterfall_plot(data, x, y, *, measure=, group=, sorted_value=, ...) -> PlotSpec`       | financial/contribution waterfall figure      |
|  [06]   | `image_matrix(image_data_array, *, norm=None, scale=1, ...) -> SupPlotsSpec`            | grid of numpy-array image rasters            |

[ENTRYPOINT_SCOPE]: in-process export
- rail: visuals

`PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` are the single per-format byte-serializer axis on the plot object (mirrored on `SupPlotsSpec`) — the `LP_RENDER` `ExportFormat -> method-name` row members `visualization/chart/export#EXPORT` keys. The `path` argument is a filename `str` OR a file-like object: a file-like sink (`io.BytesIO()`) captures the rendered bytes IN-MEMORY and the method returns `None`, while a filename returns the pathname `str` (the reflected `-> str` annotation covers the filename path; the file-like-returns-`None` behavior is the in-memory variant `export#EXPORT` `_lp_native` relies on). `ggsave(plot, filename, ...)` is the canonical multi-format file export that infers the format from the filename extension (`.svg`/`.png`/`.pdf`/`.html`) and returns the absolute pathname. PNG/PDF rasterize through `pillow` (`.api/pillow.md`); SVG/HTML are self-contained. lets-plot ships **no `to_jpeg`** — the JPEG export lane rasterizes the lets-plot SVG through the shared `vl-convert-python` resvg core, never pillow. Rendering is entirely in-process — no subprocess, browser, or Vega binary.

| [INDEX] | [CALL_SHAPE]                                                                | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `to_svg(path=None, w=None, h=None, unit=None) -> str`                       | render to SVG (file-like `path` -> bytes/`None`)         |
|  [02]   | `to_png(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str`      | render to PNG via `pillow` (file-like `path` -> bytes)   |
|  [03]   | `to_pdf(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str`      | render to PDF via `pillow` (file-like `path` -> bytes)   |
|  [04]   | `to_html(path=None, iframe=None) -> str`                                    | render to HTML (file-like `path` -> bytes/`None`)        |
|  [05]   | `as_dict() -> dict`                                                         | recursively-resolved plot-spec dict (receipt evidence)   |
|  [06]   | `props()` / `has_layers()` / `duplicate() -> PlotSpec`                      | spec-property map / layer probe / deep copy              |
|  [07]   | `ggsave(plot, filename, *, path=None, iframe=True, scale=None, ...) -> str` | canonical multi-format file export (format by extension) |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_GGGRAMMAR]:
- ingest axis: `ggplot(data, aes(...))` admits a polars/pandas frame or a `{column: sequence}` dict; the grammar binds columns by name through `aes(x, y, **kwargs)`, never a per-backend ingest path and never a fixed-keyword aesthetic roster.
- plot axis: one `PlotSpec` owns the single-view grammar; the 58 `geom_*` + 4 `stat_*` layers, the 68 `scale_*` scales, the `position_*` adjustments, `coord_*`/`facet_*` modifiers, and `theme*`/`flavor_*`/`element_*` styling are `+`-composed `FeatureSpec`/`LayerSpec` objects, never parallel chart types per geom; statistical geometries (`geom_smooth`/`geom_density`/`geom_density2d`/`geom_boxplot`/`geom_violin`/`geom_histogram`/`geom_qq`) and the standalone `stat_summary`/`stat_ecdf` layers run the stat in the bundled core, never a hand-rolled numpy fit baked into the data; the `lets_plot.bistro` `corr_plot`/`qq_plot`/`joint_plot`/`residual_plot`/`waterfall_plot` recipes assemble the same grammar from one call.
- reduction axis: `sampling_*` is the per-layer `sampling=` data-cap — the host-free analogue of the Vega band's `vegafusion` server-side pre-aggregation — so a large frame renders without a browser or external feed; reduction is a `sampling=` value on the layer, never a producer-side pre-decimation.
- composition axis: `gggrid`/`ggbunch`/`ggdeck` compose `PlotSpec` instances into one `SupPlotsSpec` composite root (`ggmarginal` attaches a marginal view); composition is a constructor over a plot list, never a duplicated plot definition.
- palette axis: `graphic/color/derive#DERIVE` `ColorReceipt.coords` arrives as the `Palette` array threaded through `scale_color_manual`/`scale_fill_manual` (lazily imported by `export#EXPORT`) and the shared `hex_ramp` RGB-to-hex projection declared on `visualization/chart/spec#CHART`; never an ad-hoc per-engine color pick, and the same `Palette` threads the Vega and matplotlib bands so the one-color-source invariant holds across every backend.
- export axis: `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` are the single per-format byte serializers (the `LP_RENDER` `ExportFormat -> method-name` row table `visualization/chart/export#EXPORT` keys, mirrored on `SupPlotsSpec`), each taking a file-like `path` (`io.BytesIO()`) to capture the rendered bytes in-memory and return `None`; `ggsave` is the canonical format-by-extension file export returning the pathname. PNG/PDF rasterize through `pillow` (`.api/pillow.md`); SVG/HTML are self-contained; the JPEG lane rasterizes the lets-plot SVG through the shared `vl-convert-python` resvg core (lets-plot ships no `to_jpeg`) — no re-minted rasterizer, no headless browser, no Vega binary, no Cairo, no ImageMagick. The native render rides `to_thread` under one `CapacityLimiter` (`.api/anyio.md`), GIL-releasing off the runtime event loop.
- evidence: each render captures geom/stat-layer kinds, the mapped aesthetic channels (read from `PlotSpec.as_dict`), the registered manual-palette values, active theme/flavor, output format, and output byte length as a `msgspec.Struct` (`.api/msgspec.md`) visuals receipt — projected onto the settled `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact (engine the matched `ChartSpec.tag` `"lets_plot"`, dialect the `ExportFormat` value, scale/theme the `RenderPolicy` knobs, byte_len the rendered length) — emitted under one `structlog` (`.api/structlog.md`) event inside an OpenTelemetry (`.api/opentelemetry-api.md`) span; an export failure (bad `path`, missing `pillow` for a raster format, a `geom_livemap` non-HTML request) folds onto the `expression.Result` (`.api/expression.md`) rail rather than raising into the producer.
- boundary: lets-plot owns its own grammar construction AND its own SVG/PNG/PDF/HTML serialization (it is a complete self-render engine, unlike `altair` (`.api/altair.md`) which builds a spec for `vl-convert-python` (`.api/vl-convert-python.md`) to lower); the Vega band (`altair` -> `vl-convert-python` -> `vegafusion` (`.api/vegafusion.md`)) is the orthogonal declarative-Vega path, `great-tables` (`.api/great-tables.md`) owns publication display tables, `resvg-py` (`.api/resvg-py.md`) owns standalone SVG-to-PNG rasterization — lets-plot self-renders its own grammar and never routes through the Vega compiler (except the JPEG-only resvg hop); the `ChartSpec.LetsPlot` engine is detected by `type(engine).__module__.startswith("lets_plot")`, so the raw `PlotSpec` crosses to the worker lane and never constructs on the runtime; `geom_livemap` interactive map UI and live web UI stay outside this package.

[RAIL_LAW]:
- Package: `lets-plot`
- Owns: declarative ggplot2-grammar chart construction over polars/pandas frames — the 58-member `geom_*` + `stat_*` layer family, the 68-member `scale_*`/`scale_*_manual` scale algebra, the `position_*` adjustments, `sampling_*` large-data reduction, `coord_*`/`facet_*`/`theme*`/`flavor_*`/`element_*` modifiers, `guide_*`/`labs`/`lims` annotation, the `gggrid`/`ggbunch`/`ggdeck`/`ggmarginal` composition roots, the `lets_plot.bistro` high-level recipes, and the in-process `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` + `ggsave` self-render export
- Reject: wrapper-renames of `ggplot`/`geom_*`/`stat_*`/`scale_*`; a per-geom parallel plot type where `+`-composition owns layering; a hand-rolled grammar-of-graphics stat or numpy fit where the bundled core / `stat_*` renders; a producer-side pre-decimation where `sampling_*` caps the layer; an ad-hoc per-engine color pick where `scale_*_manual` injects the canonical `Palette`; routing lets-plot through `vl-convert`/Vega where it self-renders its own grammar (the JPEG resvg hop excepted); a headless-browser / Cairo / ImageMagick raster path where `PlotSpec.to_png`/`to_pdf` rasterize in-process via `pillow`; a subprocess hop the in-process render does not need; constructing the `PlotSpec` on the runtime instead of the worker lane; identity minting the runtime owns

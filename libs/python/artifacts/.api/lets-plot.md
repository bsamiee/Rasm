# [PY_ARTIFACTS_API_LETS_PLOT]

`lets-plot` supplies the second host-free grammar-of-graphics chart surface for the artifacts visuals rail: a ggplot2-grammar builder (`ggplot` + `aes` + the `geom_*` layer family + `scale`/`theme`/`flavor` modifiers, composed by the `+` operator into a `PlotSpec`) whose `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` serializers and the module-level `ggsave` multi-format export render entirely in-process to bytes/files with no browser, no Node, and no Vega binary. The package owner composes `ggplot`, the `geom_*`/`scale_*_manual` builder family, and the `PlotSpec.to_*`/`ggsave` serializers into the `figures/chart` `ChartSpec.LetsPlot` case; it never re-implements the grammar-of-graphics layer algebra or the SVG/PNG/PDF serialization lets-plot already owns. It is the orthogonal self-render path beside the Vega band — `vl-convert-python` lowers an `altair` Vega-Lite spec, `vegafusion` pre-transforms it, `great-tables` owns publication tables — where lets-plot self-renders its own grammar to bytes, so one host-free engine never substitutes for another and `figures/color#COLOR` threads the same palette into every backend.

This catalog is authored from the official Lets-Plot Python API surface and tagged `(source-route, reflection-blocked)`: the distribution is manifest row worker and is not installed on the runtime reflection core (nor cp3.12/3.13/3.14), so `api resolve lets-plot` returns `unsupported`; every member below traces to the published `lets_plot` / `lets_plot.export` API reference, not to ad-hoc reflection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lets-plot`
- package: `lets-plot`
- import: `lets_plot`
- owner: `artifacts`
- rail: visuals
- version: `4.10.1`
- deps: SVG/HTML serialization is self-contained (bundled Kotlin/JS core); PNG and PDF export require `pillow` (`.api/pillow.md`) — the in-process raster backend lets-plot drives to rasterize the rendered SVG, NOT a headless browser, Cairo, or ImageMagick; `geom_livemap` map plots serialize to HTML only
- entry points: none (library only); `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` and `ggsave` perform export
- capability: declarative ggplot2-grammar chart construction — `ggplot(data, aes(...))` over a polars/pandas frame or column dict, the `geom_*` layer family (~60 geometries including statistical `geom_smooth`/`geom_density`/`geom_density2d`/`geom_boxplot`/`geom_violin`/`geom_histogram`/`geom_bin2d`/`geom_hex`/`geom_contour`/`geom_qq` and the `geom_imshow`/`geom_livemap` raster/map geoms), the `scale_*`/`scale_*_manual` scale algebra, `coord_*`/`facet_grid`/`facet_wrap` coordinate and facet modifiers, `theme`/`theme_*`/`flavor_*`/`ggsize` styling, the `gggrid`/`ggbunch`/`ggdeck` plot-composition roots, and the in-process `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` byte serializers plus the format-by-extension `ggsave` multi-format export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plot and composition roots
- rail: visuals

`ggplot(data, mapping)` returns a `PlotSpec` (`lets_plot.plot.core`); `geom_*`/`scale_*`/`coord_*`/`facet_*`/`theme*` calls return `FeatureSpec`/`LayerSpec` objects that compose onto a `PlotSpec` via the `+` operator (the grammar is operator-composed, never a parallel per-geom plot type). `LetsPlot` is the library-options/initialization owner; `gggrid`/`ggbunch`/`ggdeck` compose multiple `PlotSpec` instances into the `SupPlotsSpec`/`GGBunch` composite roots that `ggsave` also accepts.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                                                              |
| :-----: | :--------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `PlotSpec`       | plot builder     | single-view ggplot grammar object (`lets_plot.plot.core`); `+`-composed, owns the `to_svg`/`to_png`/`to_pdf`/`to_html` serializers |
|  [02]   | `FeatureSpec`    | feature spec     | base spec the `geom_*`/`scale_*`/`coord_*`/`facet_*`/`theme*` calls return, `+`-added onto a `PlotSpec` |
|  [03]   | `LayerSpec`      | layer spec       | a `geom_*` layer feature added onto a `PlotSpec`                    |
|  [04]   | `SupPlotsSpec`   | composite root   | grid/overlay composition produced by `gggrid`/`ggdeck`             |
|  [05]   | `GGBunch`        | composite root   | custom-layout composition produced by `ggbunch`                    |
|  [06]   | `LetsPlot`       | library options  | initialization + global options owner (`LetsPlot.setup_html()`, `LetsPlot.set_theme(...)`) |
|  [07]   | `aes`            | aesthetic mapping | the `aes(x=, y=, color=, fill=, size=, shape=, ...)` channel-mapping object passed to `ggplot`/`geom_*` |

[PUBLIC_TYPE_SCOPE]: geometry layers, scales, and styling modifiers
- rail: visuals

The `geom_*` family is the single geometric-layer axis; the `scale_*` family the single scale-mapping axis; `coord_*`/`facet_*`/`theme*`/`flavor_*` the coordinate/facet/style modifiers. Every one returns a `FeatureSpec` composed onto a `PlotSpec` by `+`, never a parallel chart type. `scale_color_manual`/`scale_fill_manual` are the canonical palette-injection members the `figures/chart` palette-thread reaches.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `geom_point` / `geom_line` / `geom_path` / `geom_step` | layer  | point / line / path / step geometry                            |
|  [02]   | `geom_bar` / `geom_histogram` / `geom_freqpoly` / `geom_pie` | layer | bar / histogram / frequency-polygon / pie geometry          |
|  [03]   | `geom_area` / `geom_ribbon` / `geom_band` / `geom_density` | layer | filled area / ribbon / band / KDE-density geometry          |
|  [04]   | `geom_tile` / `geom_raster` / `geom_bin2d` / `geom_hex` | layer  | heatmap-tile / raster / 2D-bin / hexbin geometry               |
|  [05]   | `geom_boxplot` / `geom_violin` / `geom_sina` / `geom_pointrange` / `geom_errorbar` / `geom_crossbar` | layer | statistical-summary geometry              |
|  [06]   | `geom_smooth` / `geom_density2d` / `geom_contour` / `geom_contourf` / `geom_qq` / `geom_qq_line` | layer | statistical-fit / density / contour / Q-Q geometry |
|  [07]   | `geom_text` / `geom_label` / `geom_text_repel` / `geom_label_repel` | layer | text / label (with repel) annotation geometry        |
|  [08]   | `geom_rect` / `geom_segment` / `geom_polygon` / `geom_map` / `geom_imshow` | layer | rectangle / segment / polygon / GeoJSON-map / image-array geometry |
|  [09]   | `geom_livemap`                                  | layer (map)   | interactive base-map geometry — serializes to HTML only        |
|  [10]   | `scale_color_manual` / `scale_fill_manual`      | scale (manual) | canonical discrete palette injection (`figures/color#COLOR` thread) |
|  [11]   | `scale_shape_manual` / `scale_size_manual` / `scale_alpha_manual` / `scale_linetype_manual` | scale (manual) | discrete shape/size/alpha/linetype mapping |
|  [12]   | `scale_x_continuous` / `scale_y_continuous` / `scale_x_discrete` / `scale_color_gradient` / `scale_fill_gradient` / `scale_color_brewer` | scale | continuous / discrete / gradient / ColorBrewer scales |
|  [13]   | `coord_cartesian` / `coord_fixed` / `coord_flip` / `coord_map` / `coord_polar` | coordinate | coordinate-system modifier                       |
|  [14]   | `facet_grid` / `facet_wrap`                     | facet         | small-multiples facet modifier                                 |
|  [15]   | `theme` / `theme_bw` / `theme_minimal2` / `theme_classic` / `theme_void` | theme | base + predefined theme                              |
|  [16]   | `flavor_darcula` / `flavor_solarized_dark` / `flavor_solarized_light` / `flavor_high_contrast_dark` / `flavor_high_contrast_light` / `flavor_standard` | flavor | predefined color-flavor scheme |
|  [17]   | `ggsize` / `labs` / `ggtitle` / `xlab` / `ylab` | sizing / label | view size and axis/title labels                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar construction
- rail: visuals

`ggplot(data, mapping)` seeds a `PlotSpec` over a polars/pandas frame or a `{column: sequence}` dict; `aes(...)` maps columns to aesthetic channels; the `geom_*` family adds geometry layers; `scale_*`/`coord_*`/`facet_*`/`theme*` modify the result — all composed by the `+` operator, never a parallel per-geom plot type. `LetsPlot.setup_html()` initializes the in-process HTML render context once at boundary scope.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                            | [CAPABILITY]                                            |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `ggplot`               | `ggplot(data=None, mapping=None, *, x=None, y=None) -> PlotSpec`                         | seed a plot over a frame / column dict with an `aes` mapping |
|  [02]   | `aes`                  | `aes(x=None, y=None, *, color=None, fill=None, size=None, shape=None, alpha=None, group=None, ...) -> FeatureSpec` | map data columns to aesthetic channels |
|  [03]   | `geom_*`               | `geom_point(mapping=None, *, data=None, stat=None, position=None, color=None, fill=None, size=None, ...) -> LayerSpec` (every `geom_*` mirrors: optional `mapping`/`data`/`stat`/`position` plus geom-specific aesthetics) | add a geometry layer (`+`-composed onto the `PlotSpec`) |
|  [04]   | `scale_*_manual`       | `scale_color_manual(values, *, name=None, breaks=None, labels=None, limits=None, na_value=None, ...) -> FeatureSpec` (`scale_fill_manual`/`scale_shape_manual`/... mirror) | inject a discrete palette / shape / size mapping |
|  [05]   | `facet_grid` / `facet_wrap` | `facet_grid(x=None, y=None, ...) -> FeatureSpec`; `facet_wrap(facets, *, ncol=None, nrow=None, ...) -> FeatureSpec` | small-multiples facet modifier        |
|  [06]   | `theme` / `flavor_*`   | `theme(*, axis_title=None, legend_position=None, plot_background=None, ...) -> FeatureSpec`; `flavor_darcula() -> FeatureSpec` | style / color-flavor modifier        |
|  [07]   | `ggsize`               | `ggsize(width, height) -> FeatureSpec`                                                   | set the plot pixel size                                 |
|  [08]   | `LetsPlot.setup_html`  | `LetsPlot.setup_html(isolated_frame=None, offline=None, no_js=None, show_status=None) -> None` | initialize the in-process HTML render context  |

[ENTRYPOINT_SCOPE]: composition
- rail: visuals

`gggrid`/`ggbunch`/`ggdeck` compose multiple `PlotSpec` instances into the composite roots (`SupPlotsSpec`/`GGBunch`) — composition is a constructor over a plot list, never a duplicated plot definition. The composite roots are also valid `ggsave` inputs.

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                                                          | [CAPABILITY]                                          |
| :-----: | :---------- | :------------------------------------------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | `gggrid`    | `gggrid(plots, ncol=None, *, sharex=None, sharey=None, widths=None, heights=None, hspace=None, vspace=None, align=None) -> SupPlotsSpec` | combine plots in a regular grid    |
|  [02]   | `ggbunch`   | `ggbunch(plots, regions=None, *, width=None, height=None) -> GGBunch`                  | combine plots with a custom-region layout             |
|  [03]   | `ggdeck`    | `ggdeck(plots, *, ...) -> SupPlotsSpec`                                                | overlay plots with aligned drawing areas              |

[ENTRYPOINT_SCOPE]: in-process export
- rail: visuals

`PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` are the single per-format byte-serializer axis on the plot object — the `LP_RENDER` row members the `figures/chart` export fold keys by `ExportFormat`. The `path` argument is a filename `str` OR a file-like object: a file-like sink (`io.BytesIO()`) captures the rendered bytes IN-MEMORY and the method returns `None` (a filename returns the absolute pathname). `ggsave(plot, filename, ...)` is the canonical multi-format export that infers the format from the filename extension (`.svg`/`.png`/`.pdf`/`.html`) and returns the absolute pathname. PNG/PDF render through `pillow`; SVG/HTML are self-contained. Rendering is entirely in-process — no subprocess, browser, or Vega binary.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                               | [CAPABILITY]                                          |
| :-----: | :------------------- | :-------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PlotSpec.to_svg`    | `to_svg(path=None, w=None, h=None, unit=None) -> str \| None`                                              | render to SVG — file-like `path` captures bytes, returns `None`; filename returns the pathname; SVG content returned when `path=None` |
|  [02]   | `PlotSpec.to_png`    | `to_png(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str \| None`                             | render to PNG via `pillow` — file-like `path` captures bytes (returns `None`), filename returns the pathname |
|  [03]   | `PlotSpec.to_pdf`    | `to_pdf(path, scale=None, w=None, h=None, unit=None, dpi=None) -> str \| None`                             | render to PDF via `pillow` — file-like `path` captures bytes (returns `None`), filename returns the pathname |
|  [04]   | `PlotSpec.to_html`   | `to_html(path=None, iframe=None) -> str \| None`                                                           | render to HTML — file-like `path` captures bytes (returns `None`); HTML content returned when `path=None` |
|  [05]   | `PlotSpec.as_dict`   | `as_dict() -> dict`                                                                                        | the recursively-resolved plot-spec dict               |
|  [06]   | `ggsave`             | `ggsave(plot, filename, *, path=None, iframe=True, scale=None, w=None, h=None, unit=None, dpi=None) -> str` (`plot`: `PlotSpec \| SupPlotsSpec \| GGBunch`; format inferred from `filename` extension) | canonical multi-format file export — `lets_plot.export`, returns the absolute pathname |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_GGGRAMMAR]:
- ingest axis: `ggplot(data, aes(...))` admits a polars/pandas frame or a `{column: sequence}` dict; the grammar binds columns by name through `aes`, never a per-backend ingest path.
- plot axis: one `PlotSpec` owns the single-view grammar; the `geom_*` layers, `scale_*` scales, `coord_*`/`facet_*` modifiers, and `theme*`/`flavor_*` styling are `+`-composed `FeatureSpec` objects, never parallel chart types per geom; statistical geometries (`geom_smooth`/`geom_density`/`geom_density2d`/`geom_boxplot`/`geom_histogram`/`geom_qq`) run the stat in the bundled core, never a hand-rolled numpy fit baked into the data.
- composition axis: `gggrid`/`ggbunch`/`ggdeck` compose `PlotSpec` instances into the `SupPlotsSpec`/`GGBunch` composite roots; composition is a constructor over a plot list, never a duplicated plot definition.
- export axis: `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` are the single per-format byte serializers (the `LP_RENDER` `ExportFormat -> method-name` row table the `figures/chart` export fold keys), each taking a file-like `path` (`io.BytesIO()`) to capture the rendered bytes in-memory and return `None`; `ggsave` is the canonical format-by-extension file export returning the pathname. PNG/PDF rasterize through `pillow` (`.api/pillow.md`), SVG/HTML are self-contained — no re-minted rasterizer, no headless browser, no Vega binary, no Cairo, no ImageMagick.
- evidence: each render captures geom-layer kinds, the mapped aesthetic channels, the registered manual-palette values, active theme/flavor, output format, and output byte length as a `msgspec.Struct` (`.api/msgspec.md`) visuals receipt — folded into the `figures/chart` `ArtifactReceipt.Chart(key, engine='lets_plot', dialect, scale, theme, byte_len)` six-field fact — emitted under one `structlog` (`.api/structlog.md`) event inside an OpenTelemetry (`.api/opentelemetry-api.md`) span; an export failure (bad `path`, missing `pillow` for a raster format, a `geom_livemap` non-HTML request) folds onto the `expression.Result` (`.api/expression.md`) rail rather than raising into the producer.
- boundary: lets-plot owns its own grammar construction AND its own SVG/PNG/PDF/HTML serialization (it is a complete self-render engine, unlike `altair` which builds a spec for `vl-convert-python` to lower); the Vega band (`altair` -> `vl-convert-python` -> `vegafusion`) is the orthogonal declarative-Vega path, `great-tables` owns publication display tables, `resvg-py` owns standalone SVG-to-PNG rasterization — lets-plot self-renders its own grammar and never routes through the Vega compiler; live UI stays outside this package.

[RAIL_LAW]:
- Package: `lets-plot`
- Owns: declarative ggplot2-grammar chart construction over polars/pandas frames — the `geom_*` layer family, the `scale_*`/`scale_*_manual` scale algebra, `coord_*`/`facet_*`/`theme*`/`flavor_*` modifiers, the `gggrid`/`ggbunch`/`ggdeck` composition roots, and the in-process `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html` + `ggsave` self-render export
- Reject: wrapper-renames of `ggplot`/`geom_*`/`scale_*`; a per-geom parallel plot type where `+`-composition owns layering; a hand-rolled grammar-of-graphics stat or numpy fit where the bundled core renders; an ad-hoc per-engine color pick where `scale_*_manual` injects the canonical palette; routing lets-plot through `vl-convert`/Vega where it self-renders its own grammar; a headless-browser / Cairo / ImageMagick raster path where `PlotSpec.to_png`/`to_pdf` rasterize in-process via `pillow`; a subprocess hop the in-process render does not need; identity minting the runtime owns

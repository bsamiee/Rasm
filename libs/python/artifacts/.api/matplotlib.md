# [PY_ARTIFACTS_API_MATPLOTLIB]

`matplotlib` owns the artifacts visuals rail's imperative publication 2D plotting and offscreen raster/vector export through the Agg/PDF/SVG/PS/PGF backend canvases. Every `ChartSpec.Matplotlib(figure, palette)` case renders through `_matplotlib_savefig` on the `anyio.to_process` subprocess seam, so matplotlib never resolves in the runtime process and re-implements neither the rendering backends nor the colormap registry it already owns. It holds the imperative-plotting band alone; declarative Vega/grammar charts, 3D scenes, display tables, and color-science models route to their own owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `matplotlib`
- package: `matplotlib` (Matplotlib License)
- import: `matplotlib`
- owner: `artifacts`
- rail: visuals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: figure, axes, layout, and backend types

`Figure`/`Axes`/`GridSpec` is the canonical object-oriented offscreen-render owner; `SubFigure` nests a figure inside a figure for composite multi-panel layouts.

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                                                   |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `figure.Figure`                        | figure root    | top figure container owning the axes grid, save, colorbar, layout engine       |
|  [02]   | `axes.Axes`                            | plotting unit  | one plot region owning the full plot-method family + axis/legend control       |
|  [03]   | `figure.SubFigure`                     | nested figure  | sub-figure nested for composite multi-panel layouts (own `suptitle`)           |
|  [04]   | `gridspec.GridSpec`                    | layout grid    | row/column layout with `width_ratios`/`height_ratios`; `.subplots()` builds it |
|  [05]   | `gridspec.SubplotSpec`                 | layout cell    | a single grid cell                                                             |
|  [06]   | `gridspec.GridSpecFromSubplotSpec`     | nested grid    | a grid carved from one parent cell (insets, ragged panels)                     |
|  [07]   | `colorbar.Colorbar`                    | colorbar       | scalar-mappable color legend (`set_label`/`set_ticks`/`minorticks_on`)         |
|  [08]   | `backends.backend_agg.FigureCanvasAgg` | raster canvas  | Agg rasterizer exposing the writable-format set + in-memory RGBA buffer        |
|  [09]   | `backends.backend_pdf.PdfPages`        | pdf sink       | multi-page PDF writer; `savefig` per page, `infodict()`, `attach_note`         |
|  [10]   | `backends.backend_svg.FigureCanvasSVG` | svg canvas     | SVG / SVGZ vector writer                                                       |

[PUBLIC_TYPE_SCOPE]: color registry, mapper, and norm types

`colormaps` and `color_sequences` are module singletons (`ColormapRegistry`/`ColorSequenceRegistry`); `Colorizer` shares one cmap+norm state across multiple artists (the multi-image consistent-color path) with `ScalarMappable` the per-artist base. Unqualified `[SYMBOL]` names are `matplotlib.colors.*`; `cm.`/`colorizer.` carry their module.

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `colormaps`                            | registry       | `ColormapRegistry`: name->`Colormap`; register/unregister custom cmaps       |
|  [02]   | `color_sequences`                      | registry       | `ColorSequenceRegistry`: name->color list; the qualitative-palette registry  |
|  [03]   | `Colormap`                             | colormap base  | scalar->RGBA; `resampled(n)`/`reversed()`/`with_extremes(bad, under, over)`  |
|  [04]   | `ListedColormap`                       | colormap       | `ListedColormap(colors, name, *, bad, under, over)` discrete colormap        |
|  [05]   | `LinearSegmentedColormap`              | colormap       | `.from_list(name, colors)` interpolated colormap                             |
|  [06]   | `cm.ScalarMappable`                    | color mapper   | `ScalarMappable(norm, cmap, *, colorizer)`; `to_rgba`/`set_array`/`set_clim` |
|  [07]   | `colorizer.Colorizer`                  | shared mapper  | `Colorizer(cmap, norm)` one norm shared across artists via `colorizer=`      |
|  [08]   | `colorizer.ColorizingArtist`           | shared mapper  | the artist bound to a shared `Colorizer`                                     |
|  [09]   | `Normalize`                            | norm           | linear data->unit normalization (`vmin`/`vmax`/`clip`)                       |
|  [10]   | `LogNorm` / `SymLogNorm`               | norm           | logarithmic / symmetric-log                                                  |
|  [11]   | `PowerNorm` / `AsinhNorm`              | norm           | power-law / asinh (smooth log-near-zero)                                     |
|  [12]   | `BoundaryNorm` / `TwoSlopeNorm`        | norm           | discrete-boundary / diverging two-slope                                      |
|  [13]   | `CenteredNorm` / `FuncNorm`            | norm           | zero-centered / arbitrary forward-inverse-function                           |
|  [14]   | `to_rgba` / `to_hex` / `to_rgba_array` | color util     | scalar->RGBA/hex parse, vectorized array parse                               |
|  [15]   | `LightSource`                          | color util     | hillshade relief shading                                                     |

[PUBLIC_TYPE_SCOPE]: tick locator and formatter types (`matplotlib.ticker`)

`EngFormatter` is the engineering SI-prefix formatter for AEC drawing-scale and units labelling; `PercentFormatter`/`ScalarFormatter` carry the common publication cases. Every `[SYMBOL]` is `matplotlib.ticker.*`.

| [INDEX] | [SYMBOL]                                                       | [PACKAGE_ROLE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `MaxNLocator` / `AutoLocator` / `MultipleLocator`              | locator        | bounded-count / automatic / fixed-step          |
|  [02]   | `LinearLocator` / `FixedLocator`                               | locator        | evenly-spaced / explicit                        |
|  [03]   | `LogLocator` / `SymmetricalLogLocator`                         | locator        | log / symlog                                    |
|  [04]   | `AsinhLocator` / `IndexLocator` / `NullLocator`                | locator        | asinh / index-stride / no-tick                  |
|  [05]   | `ScalarFormatter` / `FuncFormatter`                            | formatter      | scalar offset/sci / callable `func(value, pos)` |
|  [06]   | `PercentFormatter` / `EngFormatter`                            | formatter      | percent / engineering SI-prefix                 |
|  [07]   | `FormatStrFormatter` / `StrMethodFormatter`                    | formatter      | `%`-style / `str.format`                        |
|  [08]   | `FixedFormatter` / `LogFormatterSciNotation` / `NullFormatter` | formatter      | explicit-list / log-sci / no-label              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: figure construction, layout, and save

`Figure.subplots`/`add_subplot`/`add_gridspec`/`subplot_mosaic` build the axes grid; `subplot_mosaic` is the named-region layout (ASCII/list mosaic -> `dict[label, Axes]`) and `set_layout_engine('constrained'|'tight'|'compressed')` auto-fits spacing as a figure-level policy. Every `[SURFACE]` is `Figure.*` unless noted.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                               |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Figure`                   | `Figure(figsize, dpi, *, facecolor, edgecolor, layout, ...)`                                               |
|  [02]   | `subplots`                 | `subplots(nrows, ncols, *, sharex, sharey, squeeze, width_ratios, height_ratios, subplot_kw, gridspec_kw)` |
|  [03]   | `subplot_mosaic`           | `subplot_mosaic(mosaic, *, sharex, sharey, width_ratios, height_ratios, empty_sentinel, per_subplot_kw)`   |
|  [04]   | `add_subplot`              | grid-position axes                                                                                         |
|  [05]   | `add_axes`                 | explicit `[l, b, w, h]` rect axes                                                                          |
|  [06]   | `add_gridspec`             | `add_gridspec(nrows, ncols, **kw)` grid spec                                                               |
|  [07]   | `set_layout_engine`        | `set_layout_engine('constrained'\|'tight'\|'compressed'\|'none', **kw)`; `get_layout_engine` reads it      |
|  [08]   | `savefig`                  | `savefig(fname, *, transparent, dpi, format, bbox_inches, pad_inches, facecolor, edgecolor, metadata)`     |
|  [09]   | `colorbar`                 | `colorbar(mappable, cax, ax, use_gridspec=True, **kwargs)`                                                 |
|  [10]   | `sup{title,xlabel,ylabel}` | figure-spanning title/label text                                                                           |
|  [11]   | `align_labels`             | register shared-label alignment across axes                                                                |
|  [12]   | `tight_layout`             | one-shot auto-fit pad policy                                                                               |
|  [13]   | `subplots_adjust`          | explicit `(left, bottom, right, top, wspace, hspace)` margins                                              |

[ENTRYPOINT_SCOPE]: axes plotting methods (the one plotting surface)

Every `[SURFACE]` is `Axes.*`, and the `c=`/`cmap=`/`norm=`/`colorizer=` ScalarMappable arguments make `scatter`/`imshow`/`pcolormesh`/`contourf`/`hexbin`/`hist2d`/`tripcolor`/`figimage` colorbar-mappable while `set_prop_cycle` rebinds the per-artist color/style cycle. ScalarMappable calls carry `cmap`, `norm`, `vmin`, `vmax`, `colorizer`:
- `scatter(x, y, s, c, *, marker, alpha, linewidths, edgecolors)`
- `hist(x, bins, *, range, density, weights, cumulative, histtype)`
- `violinplot(dataset, positions, *, orientation, widths, showmeans, showmedians, quantiles)`
- `imshow(X, *, aspect, interpolation, alpha, origin, extent)`
- `pcolormesh(*args, *, alpha, shading)`

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `plot` / `step` / `stairs` / `errorbar`                             | line / step / stepwise-edge / error-bar plot                |
|  [02]   | `scatter`                                                           | scatter, ScalarMappable when `c` is data                    |
|  [03]   | `bar` / `barh` / `broken_barh` / `stem`                             | bar / horizontal-bar / broken-bar / stem                    |
|  [04]   | `hist` / `hist2d` / `hexbin` / `ecdf`                               | 1D / 2D histogram / hexbin density / empirical CDF          |
|  [05]   | `boxplot` / `bxp` / `violinplot` / `eventplot`                      | box / pre-computed box / violin / raster-event distribution |
|  [06]   | `imshow` / `matshow` / `spy`                                        | raster image / matrix display / sparsity pattern            |
|  [07]   | `contour` / `contourf` / `tricontour` / `tricontourf` / `tripcolor` | line / filled contour / unstructured contour + pseudocolor  |
|  [08]   | `pcolormesh` / `pcolor` / `pcolorfast`                              | pseudocolor mesh (quad / fast paths)                        |
|  [09]   | `quiver` / `streamplot` / `barbs`                                   | vector-field / streamline / wind-barb                       |
|  [10]   | `fill_between` / `fill_betweenx` / `stackplot` / `pie`              | filled band / stacked area / pie                            |
|  [11]   | `set_prop_cycle`                                                    | rebind the color/style cycle (`cycler` or `color=[...]`)    |
|  [12]   | `axhline` / `axvline` / `axline` / `axhspan` / `axvspan`            | reference lines and shaded reference spans                  |
|  [13]   | `secondary_{x,y}axis` / `inset_axes` / `twin{x,y}`                  | paired / inset / twinned axes (dual-scale)                  |
|  [14]   | `set_{title,xlabel,ylabel,xscale,yscale}`                           | axes labelling + log/linear/symlog scale                    |
|  [15]   | `legend` / `annotate` / `grid` / `table`                            | legend, annotation, grid, embedded table                    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- backend axis: select a non-interactive backend (`Agg`/`pdf`/`svg`) via `matplotlib.use("Agg")` at boundary scope before constructing figures; `matplotlib.style`/`pyplot.style` (`use`/`context`/`available`) and `matplotlib.rc_context` set rcParams as a scoped context, never global mutation in domain code.
- figure axis: the object-oriented `Figure` with `subplots`/`subplot_mosaic`/`add_subplot`/`add_axes`/`add_gridspec` is the canonical owner and `set_layout_engine('constrained')` auto-fits spacing; the stateful `pyplot` factory is convenience-only and carries no cross-figure state into the owner.
- plotting axis: each chart kind is an `Axes` method, never a parallel chart type — the plot-method family is one plotting surface.
- color axis: colormaps resolve by name through the `colormaps` `ColormapRegistry` (`colormaps['viridis']`) or a registered `ListedColormap`/`LinearSegmentedColormap.from_list`, qualitative palettes through `color_sequences`, paired with a `Normalize` subclass on a `ScalarMappable` or one shared `Colorizer(cmap, norm)` passed as `colorizer=` for cross-image consistent color; an unknown colormap name raises `KeyError` off the registry.
- tick axis: tick placement is a `ticker` locator + formatter pair set on `Axes.xaxis`/`yaxis` (`set_major_locator`/`set_major_formatter`), never a hand-rolled loop.
- save axis: `Figure.savefig(sink, format=, dpi=)` keys output by extension or explicit `format` across `png`/`pdf`/`svg`/`svgz`/`eps`/`ps`/`pgf`/`raw`/`rgba`/`jpeg`/`tiff`/`webp`/`avif`/`gif`; `dpi`/`bbox_inches`/`pad_inches`/`transparent`/`facecolor`/`metadata` ride save kwargs; a `BytesIO` sink keeps the render in-memory and multi-page PDF rides `backend_pdf.PdfPages` (`savefig` per page, `infodict()`, `attach_note`).

[STACKING]:
- `numpy` (`libs/python/.api/numpy.md`): every `Axes` plot method consumes `numpy` arrays with zero marshalling — the `graphic/color/derive#DERIVE` `ColorReceipt.coords` palette feeds `ListedColormap(palette)` and `set_prop_cycle(color=hex_ramp(palette))` with no per-row loop, and `FigureCanvasAgg.buffer_rgba()` yields a numpy-addressable RGBA buffer so a render becomes a zero-copy raster frame for `graphic/raster#RASTER` without a file round-trip.
- `anyio` (`libs/python/.api/anyio.md`): `_matplotlib_savefig` rides `anyio.to_process.run_sync(_matplotlib_savefig, figure, palette, fmt, ppi, limiter=_RENDER_LIMITER)` under one shared `CapacityLimiter`, so the GIL-heavy Agg render crosses the subprocess seam off the event loop while the vega/lets-plot bands ride `to_thread`; the `Figure` and its `Axes` construct on the worker, matching the `ChartSpec.Matplotlib(figure, palette)` case carry.
- palette source: `graphic/color/derive#DERIVE` is the single color source — `_matplotlib_savefig` registers `colors.ListedColormap(palette, name=f"chart-{id(figure):x}")` with `force=True` (idempotent re-register), then threads `axes.set_prop_cycle(color=hex_ramp(palette))` per axes with `hex_ramp` declared once on `visualization/chart/spec#CHART`, so the palette-thread is data, never a per-engine pick.
- `msgspec` (`libs/python/.api/msgspec.md`): rendered bytes fold into the `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` case through the runtime `ReceiptContributor` port — `engine` the `ChartSpec.tag` `"matplotlib"`, `dialect` the `ExportFormat` value, `byte_len` the `BytesIO.getvalue()` length; `ContentIdentity.of(f"chart-{fmt}", data)` content-keys the render for the `Rasm.Persistence` artifact index.
- `structlog`/`opentelemetry` (`libs/python/.api/structlog.md`, `libs/python/.api/opentelemetry-api.md`): a render event carries figure size, dpi, axes count, colormap/norm identity, format, and byte length as the visuals receipt evidence, and `async_boundary("chart.export.{fmt}", ...)` emits an OpenTelemetry span over the subprocess render when an OTLP endpoint is configured.
- downstream handoff: flat-SVG/raster bytes hand to the `composition/compose#COMPOSE` placement owner, which re-renders nothing; a multi-page `PdfPages` sink feeds `package/archive`; a vector SVG/PDF `savefig` feeds `export/dxf` and the imposition rail.

[RAIL_LAW]:
- Package: `matplotlib`
- Owns: imperative 2D plotting, figure layout (subplots/mosaic/gridspec/layout-engines), the colormap/color-sequence registries and the `Normalize`/`Colorizer` color machinery, ticking/formatting, and offscreen raster/vector export to PNG/PDF/SVG/EPS/PS/PGF (and pillow-backed JPEG/TIFF/WebP/AVIF/GIF)
- Accept: a `ChartSpec.Matplotlib(figure, palette)` figure rendered through `_matplotlib_savefig` on the `anyio.to_process` subprocess seam; `numpy` arrays into every plot method; a `ListedColormap`/`Colorizer` palette-thread; a `BytesIO` save sink
- Reject: wrapper-renames of `savefig`; an interactive backend on the host-free path; a figure constructed on the runtime process instead of the worker; a hand-rolled colormap or tick loop where the `colormaps` registry and `ticker` families own it; a per-chart-kind type where axes methods suffice; a `matplotlib.animation` writer where PyAV owns temporal media; a declarative-Vega, grammar, or 3D claim matplotlib does not own

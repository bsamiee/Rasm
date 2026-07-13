# [PY_ARTIFACTS_API_MATPLOTLIB]

`matplotlib` supplies the imperative publication 2D plotting surface for the artifacts visuals rail: a `Figure` container, an `Axes` plotting unit owning the full plot-method family, the `ColormapRegistry`/`ColorSequenceRegistry` singletons and the `ScalarMappable`/`Colorizer` cmap+norm machinery, the `Normalize` family, the ticker locator/formatter families, and the Agg/PDF/SVG/PS/PGF backend canvases that drive offscreen rasterization and vectorization to PNG/PDF/SVG/EPS/PS/PGF (plus the pillow-backed JPEG/TIFF/WebP/AVIF/GIF formats). The `visualization/chart/spec#CHART` `ChartSpec.Matplotlib(figure, palette)` case carries the object-oriented `Figure` plus its `graphic/color/derive#DERIVE` palette array; `visualization/chart/export#EXPORT` `_matplotlib_savefig` selects the `Agg` backend, registers the palette as a `ListedColormap`, threads it onto each axes through `set_prop_cycle`, and folds `Figure.savefig` into a `BytesIO` sink on the `anyio.to_process` subprocess seam — matplotlib never resolves in the runtime process and never re-implements the rendering backends or the colormap registry it already owns. matplotlib is the imperative-plotting band of the host-free chart axis where `vl-convert-python`/`vegafusion`/`altair`/`lets-plot` own declarative Vega/grammar charts, `pyvista`/`vtk` own 3D scenes, `great-tables` owns display tables, and `coloraide`/`colour-science`/`colour-cxf` own the color-science models its colormaps draw from.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `matplotlib`
- package: `matplotlib`
- import: `matplotlib`
- owner: `artifacts`
- rail: visuals
- installed: `3.11.0`
- license: Matplotlib License (PSF-derived, BSD-compatible; permissive, commercial-safe)
- build-floor: cp315 wheel present on this interpreter (Python 3.15.0b2) — no marker/cp-gate required; depends on `numpy>=1.25`, `pillow>=9` (raster encode), `contourpy>=1.0.1`/`kiwisolver>=1.3.1` (C-extension contour + Cassowary layout), `pyparsing>=3`, `cycler>=0.10` (the prop-cycle owner `set_prop_cycle` consumes), `fonttools>=4.22`, `python-dateutil>=2.7`
- entry points: none (library only)
- capability: imperative 2D plotting (line/step/scatter/bar/stem/hist/hist2d/hexbin/ecdf/box/violin/image/contour/tricontour/pcolormesh/quiver/streamplot/barbs/stackplot/fill-between/pie/stairs), figure layout (subplots/mosaic/gridspec/layout-engines), the colormap/color-sequence registries and the `Normalize`/`Colorizer` color machinery, ticking/formatting, and offscreen raster/vector export via the Agg/PDF/SVG/PS/PGF backends

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: figure, axes, layout, and backend types
- rail: visuals

The object-oriented `Figure`/`Axes`/`GridSpec` surface is the canonical owner for the host-free offscreen render; the stateful `pyplot` factory is convenience-only and never carries cross-figure state into the owner. `SubFigure` nests a figure inside a figure for composite panels; the layout-engine handles (`tight`/`constrained`/`compressed`) replace the per-call `tight_layout` knob.

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
- rail: visuals

`matplotlib.colormaps` is the singleton `ColormapRegistry` (`get`/`get_cmap`/`register(cmap, *, name=None, force=False)`/`unregister`/`keys`/`items`/`values`/`__getitem__`/`__call__`); `matplotlib.color_sequences` is the singleton `ColorSequenceRegistry` (`register`/`unregister`/`__getitem__`/`keys`; qualitative-palette registry). The module-level `cm.get_cmap`/`cm.register_cmap` accessors are REMOVED in 3.11 — the registry mapping is the only colormap-lookup surface, and an unknown colormap name raises `KeyError` off the registry rather than returning a silent default. `Colorizer` is the release shared cmap+norm owner that lets multiple artists share ONE normalization/scaling state (the canonical multi-image consistent-color path), with `ScalarMappable` the per-artist base. Unqualified `[SYMBOL]` names are `matplotlib.colors.*`; `colormaps`/`color_sequences` are the module singletons and `cm.`/`colorizer.` carry their module.

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
- rail: visuals

Tick placement and labelling are a locator + formatter pair set on `Axes.xaxis`/`yaxis`, never a hand-rolled tick loop. `EngFormatter` is the engineering SI-prefix formatter (AEC drawing-scale and units labelling); `PercentFormatter`/`ScalarFormatter` carry the common publication cases. Every `[SYMBOL]` is `matplotlib.ticker.*`.

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
- rail: visuals

`Figure.subplots`/`add_subplot`/`add_gridspec`/`subplot_mosaic` build the axes grid without `pyplot` global state; `subplot_mosaic` is the named-region layout (ASCII/list mosaic -> `dict[label, Axes]`). The layout engine (`set_layout_engine('constrained'|'tight'|'compressed')`) auto-fits spacing as a figure-level policy rather than a per-call `tight_layout`. Surfaces are `Figure.*` unless noted and signatures elide trivial `=None`/`=False` defaults; the stateful `pyplot.figure`/`pyplot.subplots`/`pyplot.subplot_mosaic`/`pyplot.close` factory is convenience-only, never the owner path. Full mosaic call: `subplot_mosaic(mosaic, *, sharex, sharey, width_ratios, height_ratios, empty_sentinel, subplot_kw, per_subplot_kw, gridspec_kw) -> dict[label, Axes]`.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                               |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Figure`                   | `Figure(figsize, dpi, *, facecolor, edgecolor, layout, ...)`                                               |
|  [02]   | `subplots`                 | `subplots(nrows, ncols, *, sharex, sharey, squeeze, width_ratios, height_ratios, subplot_kw, gridspec_kw)` |
|  [03]   | `subplot_mosaic`           | named-region ragged mosaic -> `dict[label, Axes]` (full call in the lead)                                  |
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
- rail: visuals

Each chart kind is an `Axes` method row (every `[SURFACE]` is `Axes.*`), never a parallel chart type. The `c=`/`cmap=`/`norm=`/`colorizer=` ScalarMappable arguments make `scatter`/`imshow`/`pcolormesh`/`contourf`/`hexbin`/`hist2d`/`tripcolor`/`figimage` colorbar-mappable; `set_prop_cycle` rebinds the per-artist color/style cycle (the chart palette-thread path). The ScalarMappable call shapes:
- `scatter(x, y, s, c, *, marker, cmap, norm, vmin, vmax, alpha, linewidths, edgecolors, colorizer, ...)`
- `hist(x, bins, *, range, density, weights, cumulative, histtype, ...)`
- `violinplot(dataset, positions, *, orientation, widths, showmeans, showmedians, quantiles, ...)`
- `imshow(X, cmap, norm, *, aspect, interpolation, alpha, vmin, vmax, colorizer, origin, extent, ...)`
- `pcolormesh(*args, alpha, norm, cmap, vmin, vmax, colorizer, shading, ...)`

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

[CHART_PUBLICATION]:
- import: `import matplotlib; matplotlib.use("Agg"); from matplotlib.figure import Figure` at boundary scope only; select a non-interactive backend (`Agg`/`pdf`/`svg`) before constructing figures for the host-free offscreen path. `matplotlib.style`/`pyplot.style` (`use`/`context`/`available`) and `matplotlib.rc_context` set rcParams as a scoped context, never global mutation in domain code.
- figure axis: the object-oriented `Figure` plus `subplots`/`subplot_mosaic`/`add_subplot`/`add_axes`/`add_gridspec` is the canonical owner; `set_layout_engine('constrained')` auto-fits spacing; the stateful `pyplot` factory is convenience-only and never carries cross-figure state into the owner.
- plotting axis: the `Axes` plot-method family (`plot`/`step`/`stairs`/`scatter`/`bar`/`stem`/`hist`/`hist2d`/`hexbin`/`ecdf`/`boxplot`/`violinplot`/`imshow`/`contourf`/`tricontourf`/`tripcolor`/`pcolormesh`/`quiver`/`streamplot`/`barbs`/`stackplot`/`fill_between`/`pie`) is one plotting surface; each chart kind is an axes method row, never a parallel chart type. `numpy` arrays (`libs/python/.api/numpy.md`) feed every plot method directly with zero marshalling, and the Agg `buffer_rgba()` returns a numpy-addressable RGBA buffer for a zero-copy frame handoff.
- color axis: colormaps come from the `matplotlib.colormaps` `ColormapRegistry` by name (`colormaps['viridis']`) or by registering a `ListedColormap`/`LinearSegmentedColormap.from_list`; qualitative palettes from `color_sequences`; paired with a `Normalize`/`LogNorm`/`SymLogNorm`/`AsinhNorm`/`PowerNorm`/`BoundaryNorm`/`TwoSlopeNorm`/`CenteredNorm`/`FuncNorm` norm on a `ScalarMappable`, or one shared `Colorizer(cmap, norm)` passed as `colorizer=` for cross-image consistent color. Color is a registry-plus-norm row, never an ad-hoc palette and never the removed `cm.get_cmap` accessor.
- tick axis: tick placement and labelling are a `ticker` locator + formatter pair set on `Axes.xaxis`/`yaxis` (`set_major_locator`/`set_major_formatter`), never a hand-rolled tick loop; `EngFormatter`/`PercentFormatter`/`ScalarFormatter`/`FuncFormatter` carry the publication cases.
- save axis: `Figure.savefig(sink, format=, dpi=)` keys the output format by the target extension or explicit `format`; the canvas-supported set reflected from `FigureCanvasAgg.get_supported_filetypes()` is `png`/`pdf`/`svg`/`svgz`/`eps`/`ps`/`pgf`/`raw`/`rgba`/`jpeg`/`jpg`/`tiff`/`tif`/`webp`/`avif`/`gif`; `dpi`/`bbox_inches`/`pad_inches`/`transparent`/`facecolor`/`metadata` ride save kwargs, never a parallel exporter; a `BytesIO` sink keeps the render in-memory; multi-page PDF rides `backend_pdf.PdfPages` (`savefig` per page, `infodict()` metadata, `attach_note` page note).
- evidence: each render captures figure size, dpi, axes count, colormap/norm identity, output format, and output byte length as a visuals receipt fact.
- boundary: matplotlib owns imperative publication 2D plotting and offscreen raster/vector export; declarative Vega/grammar charts route to `vl-convert-python`/`vegafusion`/`altair`/`lets-plot`; 3D scientific scenes route to `pyvista`/`vtk`; publication tables route to `great-tables`; color-science models route to `coloraide`/`colour-science`/`colour-cxf`; raster encode rides the bundled `pillow`; `matplotlib.animation` (`FuncAnimation`/`ArtistAnimation` + writers) is out of the host-free chart path — temporal media rides PyAV (`.api/av.md`), not a matplotlib animation writer; live interactive backends stay outside this package.

[STACKING]:
- universal `numpy` tier (`libs/python/.api/numpy.md`): every `Axes` plot method consumes `numpy` arrays directly — the `graphic/color/derive#DERIVE` `ColorReceipt.coords` palette is an `NDArray[np.float64]` fed straight into `ListedColormap(palette)` and `Axes.set_prop_cycle(color=hex_ramp(palette))` with no per-row Python loop; `FigureCanvasAgg.buffer_rgba()` yields a numpy-addressable RGBA buffer so a rendered figure becomes a zero-copy raster frame for `graphic/raster#RASTER` without a file round-trip.
- universal `anyio` tier (`libs/python/.api/anyio.md`): matplotlib never resolves on the runtime process — `visualization/chart/export#EXPORT` `_matplotlib_savefig` rides `anyio.to_process.run_sync(_matplotlib_savefig, figure, palette, fmt, ppi, limiter=_RENDER_LIMITER)` under one shared `CapacityLimiter`, so the GIL-heavy Agg render crosses the subprocess seam off the event loop while the vega/lets-plot bands ride `to_thread`. The `Figure` and its `Axes` construct on the worker, never the runtime, matching the `ChartSpec.Matplotlib(figure, palette)` gated-case carry.
- palette source: the single color source is `graphic/color/derive#DERIVE`; `_matplotlib_savefig` reaches it through `colormaps.register(colors.ListedColormap(palette, name=f"chart-{id(figure):x}"), force=True)` then `axes.set_prop_cycle(color=hex_ramp(palette))` per axes — the `hex_ramp` RGB-to-hex projection is declared once on `visualization/chart/spec#CHART` and imported, so the palette-thread is data, never an ad-hoc per-engine pick, and the registry `force=True` re-register is idempotent across renders.
- universal `msgspec` tier (`libs/python/.api/msgspec.md`): the rendered bytes fold into the `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` case (a `msgspec.Struct` fact) — `engine` the matched `ChartSpec.tag` (`"matplotlib"`), `dialect` the `ExportFormat` value, `byte_len` the `BytesIO.getvalue()` length — contributed through the runtime `ReceiptContributor` port, never a parallel matplotlib-receipt shape. `ContentIdentity.of(f"chart-{fmt}", data)` content-keys the render for the `Rasm.Persistence` artifact index.
- universal `structlog`/`opentelemetry` tier (`libs/python/.api/structlog.md`, `libs/python/.api/opentelemetry-api.md`): a `structlog`-bound render event carries the figure size, dpi, axes count, colormap/norm identity, format, and byte length as the visuals receipt evidence; the `async_boundary("chart.export.{fmt}", ...)` rail emits an OpenTelemetry span over the subprocess render when an OTLP endpoint is configured.
- downstream handoff: the flat-SVG/raster bytes hand to the regrouped `composition/compose#COMPOSE` placement owner, which lays the chart beside its siblings and re-renders nothing; a multi-page `PdfPages` sink feeds `package/archive` directly; a vector SVG/PDF `savefig` (no rasterization) feeds `export/dxf` and the imposition rail.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `matplotlib`
- Owns: imperative 2D plotting, figure layout (subplots/mosaic/gridspec/layout-engines), the colormap/color-sequence registries and the `Normalize`/`Colorizer` color machinery, ticking/formatting, and offscreen rasterization/vectorization to PNG/PDF/SVG/EPS/PS/PGF (and pillow-backed JPEG/TIFF/WebP/AVIF/GIF)
- Accept: a `ChartSpec.Matplotlib(figure, palette)` `Figure` plus its `graphic/color/derive#DERIVE` palette array, rendered through `_matplotlib_savefig` on the `anyio.to_process` gated subprocess seam feeding the chart and export-bundle owners; `numpy` arrays into every plot method; a `ListedColormap`/`Colorizer` palette-thread; a `BytesIO` save sink
- Reject: wrapper-renames of `savefig`; an interactive backend on the host-free path; a matplotlib figure constructed on the runtime process instead of the worker; a hand-rolled colormap, a hand-rolled tick loop, or the removed `cm.get_cmap`/`cm.register_cmap` accessor where the `colormaps` registry and `ticker` families exist; a per-chart-kind type where axes methods suffice; a matplotlib `animation` writer where PyAV owns temporal media; a declarative-Vega, grammar, or 3D claim matplotlib does not own

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

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                                                                          |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `figure.Figure`                        | figure root    | `Figure(figsize, dpi, *, facecolor, edgecolor, layout, ...)` — top container owning axes, `savefig`, `colorbar`, `subplots`/`add_subplot`/`add_gridspec`/`subplot_mosaic`, `set_layout_engine`, `suptitle`/`supxlabel`/`supylabel`, `align_labels`, `figimage` |
|  [02]   | `axes.Axes`                            | plotting unit  | one plot region owning the full plot-method family plus axis/scale/legend/annotation control          |
|  [03]   | `figure.SubFigure`                     | nested figure  | a sub-figure inside a figure for composite multi-panel layouts (own axes grid, own `suptitle`)        |
|  [04]   | `gridspec.GridSpec`                    | layout grid    | row/column axes layout with `width_ratios`/`height_ratios`; `GridSpec.subplots()` materializes the grid |
|  [05]   | `gridspec.SubplotSpec` / `gridspec.GridSpecFromSubplotSpec` | layout cell | a single grid cell / a nested grid carved from one parent cell (insets, ragged panels)       |
|  [06]   | `colorbar.Colorbar`                    | colorbar       | scalar-mappable color legend (`set_label`/`set_ticks`/`minorticks_on`)                                |
|  [07]   | `backends.backend_agg.FigureCanvasAgg` | raster canvas  | Agg rasterizer; `get_supported_filetypes()` enumerates the writable set, `buffer_rgba()`/`print_to_buffer()` expose the in-memory RGBA buffer for a numpy zero-copy frame handoff |
|  [08]   | `backends.backend_pdf.PdfPages`        | pdf sink       | `PdfPages(filename, metadata=None)` multi-page PDF writer (context manager); `savefig(figure, **kw)` per page, `infodict()` document metadata, `attach_note(text)` page annotation |
|  [09]   | `backends.backend_svg.FigureCanvasSVG` | svg canvas     | SVG / SVGZ vector writer                                                                              |

[PUBLIC_TYPE_SCOPE]: color registry, mapper, and norm types
- rail: visuals

`matplotlib.colormaps` is the singleton `ColormapRegistry` (`get`/`get_cmap`/`register(cmap, *, name=None, force=False)`/`unregister`/`keys`/`items`/`values`/`__getitem__`/`__call__`); `matplotlib.color_sequences` is the singleton `ColorSequenceRegistry` (`register`/`unregister`/`__getitem__`/`keys`; qualitative-palette registry). The module-level `cm.get_cmap`/`cm.register_cmap` accessors are REMOVED in 3.11 — the registry mapping is the only colormap-lookup surface, and an unknown colormap name raises `KeyError` off the registry rather than returning a silent default. `Colorizer` is the 3.10+ shared cmap+norm owner that lets multiple artists share ONE normalization/scaling state (the canonical multi-image consistent-color path), with `ScalarMappable` the per-artist base.

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE] | [CAPABILITY]                                                                                  |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `colormaps` (`ColormapRegistry`) / `color_sequences` (`ColorSequenceRegistry`) | registry | name -> `Colormap` (`colormaps['viridis']`) / name -> color list — register/unregister custom entries |
|  [02]   | `colors.Colormap`                                          | colormap base  | scalar-to-RGBA mapping; `resampled(n)`, `reversed()`, `with_extremes(bad, under, over)`, `__call__(X, alpha)` |
|  [03]   | `colors.ListedColormap` / `colors.LinearSegmentedColormap` | colormap       | `ListedColormap(colors, name, *, bad, under, over)` discrete / `LinearSegmentedColormap.from_list(name, colors)` interpolated colormap — `ListedColormap(palette)` is the chart palette-to-colormap path |
|  [04]   | `cm.ScalarMappable`                                        | color mapper   | `ScalarMappable(norm=None, cmap=None, *, colorizer=None)`; `to_rgba`/`set_array`/`set_clim`/`autoscale`/`autoscale_None`/`set_cmap`/`set_norm` — the cmap+norm base of images/scatters and the `Figure.colorbar` mappable |
|  [05]   | `colorizer.Colorizer` / `colorizer.ColorizingArtist`      | shared mapper  | `Colorizer(cmap=None, norm=None)` one shared normalization across many artists (`to_rgba`/`autoscale`/`set_clim`/`norm`/`cmap`); pass as `colorizer=` to `imshow`/`scatter`/`pcolormesh`/`figimage` for cross-image consistent color |
|  [06]   | `colors.Normalize`                                        | norm           | linear data-to-unit normalization (`vmin`/`vmax`/`clip`)                                       |
|  [07]   | `colors.LogNorm` / `colors.SymLogNorm` / `colors.PowerNorm` / `colors.AsinhNorm` | norm | logarithmic / symmetric-log / power-law / asinh (smooth log-near-zero) normalization        |
|  [08]   | `colors.BoundaryNorm` / `colors.TwoSlopeNorm` / `colors.CenteredNorm` / `colors.FuncNorm` | norm | discrete-boundary / diverging two-slope / zero-centered / arbitrary forward/inverse-function normalization |
|  [09]   | `colors.to_rgba` / `colors.to_hex` / `colors.to_rgba_array` / `colors.LightSource` | color util | scalar color parse to RGBA/hex, vectorized array parse, hillshade relief shading              |

[PUBLIC_TYPE_SCOPE]: tick locator and formatter types (`matplotlib.ticker`)
- rail: visuals

Tick placement and labelling are a locator + formatter pair set on `Axes.xaxis`/`yaxis`, never a hand-rolled tick loop. `EngFormatter` is the engineering SI-prefix formatter (AEC drawing-scale and units labelling); `PercentFormatter`/`ScalarFormatter` carry the common publication cases.

| [INDEX] | [SYMBOL]                                                                | [PACKAGE_ROLE] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `ticker.MaxNLocator` / `ticker.AutoLocator` / `ticker.MultipleLocator` / `ticker.LinearLocator` / `ticker.FixedLocator` | locator | bounded-count / automatic / fixed-step / evenly-spaced / explicit tick locators |
|  [02]   | `ticker.LogLocator` / `ticker.SymmetricalLogLocator` / `ticker.AsinhLocator` / `ticker.IndexLocator` / `ticker.NullLocator` | locator | log / symlog / asinh / index-stride / no-tick locators                |
|  [03]   | `ticker.ScalarFormatter` / `ticker.FuncFormatter` / `ticker.PercentFormatter` / `ticker.EngFormatter` | formatter | scalar (offset/sci) / callable `func(value, pos)` / percent / engineering SI-prefix label formatters |
|  [04]   | `ticker.FormatStrFormatter` / `ticker.StrMethodFormatter` / `ticker.FixedFormatter` / `ticker.LogFormatterSciNotation` / `ticker.NullFormatter` | formatter | `%`-style / `str.format` / explicit-list / log-sci / no-label formatters |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: figure construction, layout, and save
- rail: visuals

`Figure.subplots`/`add_subplot`/`add_gridspec`/`subplot_mosaic` build the axes grid without `pyplot` global state; `subplot_mosaic` is the named-region layout (ASCII/list mosaic -> `dict[label, Axes]`). The layout engine (`set_layout_engine('constrained'|'tight'|'compressed')`) auto-fits spacing as a figure-level policy rather than a per-call `tight_layout`.

| [INDEX] | [SURFACE]                                                            | [CALL_SHAPE]                                                                                       | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `figure.Figure`                                                     | `Figure(figsize=None, dpi=None, *, facecolor=None, edgecolor=None, layout=None, ...)`             | construct a figure (object-oriented owner)    |
|  [02]   | `Figure.subplots`                                                   | `subplots(nrows=1, ncols=1, *, sharex=False, sharey=False, squeeze=True, width_ratios=None, height_ratios=None, subplot_kw=None, gridspec_kw=None)` | OO axes grid (no pyplot state) |
|  [03]   | `Figure.subplot_mosaic`                                             | `subplot_mosaic(mosaic, *, sharex=False, sharey=False, width_ratios=None, height_ratios=None, empty_sentinel='.', subplot_kw=None, per_subplot_kw=None, gridspec_kw=None) -> dict[Any, Axes]` | named-region ragged layout |
|  [04]   | `Figure.add_subplot` / `Figure.add_axes` / `Figure.add_gridspec`   | grid position / explicit `[l,b,w,h]` rect / `add_gridspec(nrows, ncols, **kw)`                    | add one axes / explicit-rect axes / grid spec |
|  [05]   | `Figure.set_layout_engine` / `Figure.get_layout_engine`            | `set_layout_engine(layout='constrained'\|'tight'\|'compressed'\|'none', **kw)`                    | figure-level auto-spacing engine              |
|  [06]   | `Figure.savefig`                                                    | `savefig(fname, *, transparent=None, dpi=, format=, bbox_inches=, pad_inches=, facecolor=, edgecolor=, metadata=, **kwargs)` | render to file/stream/`BytesIO` keyed by extension or explicit `format` |
|  [07]   | `Figure.colorbar`                                                   | `colorbar(mappable, cax=None, ax=None, use_gridspec=True, **kwargs)`                              | add a colorbar from a `ScalarMappable`        |
|  [08]   | `Figure.suptitle` / `Figure.supxlabel` / `Figure.supylabel` / `Figure.align_labels` | figure-spanning text / shared-label alignment                                    | composite-figure titling and label registration |
|  [09]   | `Figure.tight_layout` / `Figure.subplots_adjust`                   | pad policy / explicit `(left, bottom, right, top, wspace, hspace)` margins                        | one-shot auto-fit / manual axes spacing       |
|  [10]   | `pyplot.figure` / `pyplot.subplots` / `pyplot.subplot_mosaic` / `pyplot.close` | stateful factory; `close(fig\|'all')`                                                  | convenience figure creation / release (never the owner path) |

[ENTRYPOINT_SCOPE]: axes plotting methods (the one plotting surface)
- rail: visuals

Each chart kind is an `Axes` method row, never a parallel chart type. The `c=`/`cmap=`/`norm=`/`colorizer=` ScalarMappable arguments make `scatter`/`imshow`/`pcolormesh`/`contourf`/`hexbin`/`hist2d`/`tripcolor`/`figimage` colorbar-mappable; `set_prop_cycle` rebinds the per-artist color/style cycle (the chart palette-thread path).

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]                                       | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Axes.plot` / `Axes.step` / `Axes.stairs` / `Axes.errorbar`                | x/y plus style/error/baseline spec                 | line / step / stepwise-edge / error-bar plot                         |
|  [02]   | `Axes.scatter`                                                             | `scatter(x, y, s=None, c=None, *, marker, cmap, norm, vmin, vmax, alpha, linewidths, edgecolors, colorizer, ...)` | scatter (ScalarMappable when `c` is data)        |
|  [03]   | `Axes.bar` / `Axes.barh` / `Axes.broken_barh` / `Axes.stem`                | x/height plus width / (xrange, yrange) spans        | bar / horizontal-bar / broken-bar / stem plot                        |
|  [04]   | `Axes.hist` / `Axes.hist2d` / `Axes.hexbin` / `Axes.ecdf`                  | `hist(x, bins=None, *, range, density, weights, cumulative, histtype, ...)` | 1D histogram / 2D histogram / hexbin density / empirical CDF |
|  [05]   | `Axes.boxplot` / `Axes.bxp` / `Axes.violinplot` / `Axes.eventplot`         | `violinplot(dataset, positions=None, *, orientation='vertical', widths, showmeans, showmedians, quantiles, ...)` | box / pre-computed box / violin / raster-event distribution plots |
|  [06]   | `Axes.imshow` / `Axes.matshow` / `Axes.spy`                               | `imshow(X, cmap=None, norm=None, *, aspect, interpolation, alpha, vmin, vmax, colorizer, origin, extent, ...)` | raster image / matrix display / sparsity pattern |
|  [07]   | `Axes.contour` / `Axes.contourf` / `Axes.tricontour` / `Axes.tricontourf` / `Axes.tripcolor` | x/y/z plus levels/cmap (regular grid or triangulation) | line / filled contour / unstructured-triangulation contour and pseudocolor |
|  [08]   | `Axes.pcolormesh` / `Axes.pcolor` / `Axes.pcolorfast`                      | `pcolormesh(*args, alpha, norm, cmap, vmin, vmax, colorizer, shading, ...)` | pseudocolor mesh (quad / fast paths)                          |
|  [09]   | `Axes.quiver` / `Axes.streamplot` / `Axes.barbs`                          | x/y/u/v plus arrow/stream/barb policy               | vector-field / streamline / wind-barb plot                           |
|  [10]   | `Axes.fill_between` / `Axes.fill_betweenx` / `Axes.stackplot` / `Axes.pie` | x plus y1/y2 / stacked series / wedge fractions     | filled band / stacked area / pie chart                               |
|  [11]   | `Axes.set_prop_cycle`                                                      | `set_prop_cycle(*args, **kwargs)` (a `cycler` or `color=[...]`) | rebind the per-artist color/style cycle (the palette-thread entry) |
|  [12]   | `Axes.axhline` / `Axes.axvline` / `Axes.axline` / `Axes.axhspan` / `Axes.axvspan` | y/x position / two-point line / span bounds  | reference lines and shaded reference spans                           |
|  [13]   | `Axes.secondary_xaxis` / `Axes.secondary_yaxis` / `Axes.inset_axes` / `Axes.twinx` / `Axes.twiny` | `secondary_xaxis(location, functions=None, *, transform=None)` | paired / inset / twinned axes (dual-scale annotation) |
|  [14]   | `Axes.set_title` / `set_xlabel` / `set_ylabel` / `set_xscale` / `set_yscale` / `legend` / `annotate` / `grid` / `table` | text/scale/legend/annotation/grid policy | axes labelling, log/linear/symlog scale, legend, annotation, grid, embedded table |

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

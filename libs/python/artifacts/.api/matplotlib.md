# [PY_ARTIFACTS_API_MATPLOTLIB]

`matplotlib` supplies the publication 2D plotting surface for the artifacts visuals rail: a `Figure` container, an `Axes` plotting unit with the full plot-method family, the `ColormapRegistry`/`ScalarMappable` color machinery, the `Normalize` family, the ticker locators/formatters, and the Agg/PDF/SVG backend canvases that drive imperative chart construction and offscreen rasterization/vectorization to PNG/PDF/SVG/EPS/PGF. The package owner composes the object-oriented `Figure`/`Axes` over the Agg backend and `Figure.savefig` into the chart owner; it never re-implements the rendering backends or the colormap registry matplotlib already owns. matplotlib is the imperative-plotting half of the visuals rail where `vl-convert-python`/`vegafusion`/`altair` own declarative Vega charts, `pyvista`/`vtk` own 3D scenes, `great-tables` owns display tables, and `coloraide`/`colour-science` own the color-science models its colormaps draw from.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `matplotlib`
- package: `matplotlib`
- import: `matplotlib`
- owner: `artifacts`
- rail: visuals
- installed: `3.11.0` (PyPI) — manifest-gated `python_version<'3.15'` (contourpy/kiwisolver lack CPython 3.15 wheels; the cp315-core owner never imports it and renders on the runtime subprocess seam)
- license: matplotlib license (PSF-derived, BSD-compatible); depends on `numpy`, `pillow` (raster encode), `contourpy`/`kiwisolver` (C-extensions), `pyparsing`, `cycler`, `fonttools`, `python-dateutil`
- entry points: none (library only)
- capability: imperative 2D plotting (line/scatter/bar/hist/image/contour/pcolormesh/pie/box/violin/quiver/stream/stem/errorbar), figure layout, colormap registry and norms, ticking/formatting, and offscreen raster/vector export via the Agg/PDF/SVG/PS/PGF backends

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: figure, axes, and backend types
- rail: visuals

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `figure.Figure`                        | figure root    | top container owning axes, `savefig`, `colorbar`, `add_subplot`/`subplots`/`add_gridspec` |
|  [02]   | `axes.Axes`                            | plotting unit  | one plot region with the plot-method family |
|  [03]   | `figure.SubFigure`                     | nested figure  | sub-figure inside a figure                  |
|  [04]   | `gridspec.GridSpec`                    | layout grid    | row/column axes layout with width/height ratios |
|  [05]   | `colorbar.Colorbar`                    | colorbar       | scalar-mappable color legend                |
|  [06]   | `cm.ScalarMappable`                    | color mapper   | `to_rgba`/`set_array`/`set_clim`/`autoscale`/`get_cmap` (cmap+norm base of images/scatters) |
|  [07]   | `backends.backend_agg.FigureCanvasAgg` | raster canvas  | Agg PNG/raw rasterizer (`get_supported_filetypes`) |
|  [08]   | `backends.backend_pdf.PdfPages`        | pdf sink       | multi-page PDF writer (context manager)     |
|  [09]   | `backends.backend_svg.FigureCanvasSVG` | svg canvas     | SVG/SVGZ vector writer                       |

[PUBLIC_TYPE_SCOPE]: color, norm, and tick types
- rail: visuals

`matplotlib.colormaps` is the singleton `ColormapRegistry` (`get`/`get_cmap`/`register`/`unregister`/`keys`/`items`); `matplotlib.color_sequences` is the qualitative-palette registry. The module-level `cm.get_cmap`/`register_cmap` accessors are removed in 3.11 — the registry mapping is the only colormap-lookup surface.

| [INDEX] | [SYMBOL]                                                  | [PACKAGE_ROLE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `colormaps` (`ColormapRegistry`) / `color_sequences`     | registry       | name -> `Colormap` / name -> color list (`colormaps['viridis']`) |
|  [02]   | `colors.Colormap`                                        | colormap base  | scalar-to-RGBA mapping            |
|  [03]   | `colors.ListedColormap` / `colors.LinearSegmentedColormap` | colormap     | discrete-listed / interpolated colormap |
|  [04]   | `colors.Normalize`                                       | norm           | linear data-to-unit normalization |
|  [05]   | `colors.LogNorm` / `colors.SymLogNorm` / `colors.PowerNorm` | norm        | logarithmic / symmetric-log / power normalization |
|  [06]   | `colors.BoundaryNorm` / `colors.TwoSlopeNorm`            | norm           | discrete-boundary / diverging two-slope normalization |
|  [07]   | `ticker.MaxNLocator` / `ticker.LogLocator`               | locator        | tick locators                     |
|  [08]   | `ticker.FuncFormatter` / `ticker.ScalarFormatter` / `ticker.PercentFormatter` | formatter | callable / scalar / percent tick label formatters |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: figure construction and save
- rail: visuals

`pyplot` rows are the stateful factory; the `Figure` rows are the object-oriented surface the offscreen path uses. `Figure.subplots`/`add_subplot`/`add_gridspec` build the axes grid without `pyplot` global state.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                  | [CAPABILITY]                         |
| :-----: | :-------------------- | :--------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `pyplot.figure`       | `figure(num=None, figsize=None, dpi=None, ...)`                              | create or activate a figure          |
|  [02]   | `pyplot.subplots`     | `subplots(nrows=1, ncols=1, ...) -> (Figure, Axes)`                          | create a figure plus axes grid       |
|  [03]   | `figure.Figure`       | `Figure(figsize=None, dpi=None, layout=None, ...)`                           | construct a figure (object-oriented) |
|  [04]   | `Figure.subplots`     | `subplots(nrows=1, ncols=1, *, sharex=False, sharey=False, width_ratios=None, height_ratios=None, subplot_kw=None, ...)` | OO axes grid (no pyplot state) |
|  [05]   | `Figure.add_subplot` / `Figure.add_axes` / `Figure.add_gridspec` | grid position / rect / grid spec               | add one axes / explicit-rect axes / grid |
|  [06]   | `Figure.savefig`      | `savefig(fname, *, transparent=None, dpi=, format=, bbox_inches=, pad_inches=, metadata=, **kwargs)` | render to file/stream by extension |
|  [07]   | `Figure.colorbar`     | `colorbar(mappable, cax=None, ax=None, use_gridspec=True, **kwargs)`         | add a colorbar from a ScalarMappable |
|  [08]   | `Figure.tight_layout` / `Figure.subplots_adjust` | pad policy / explicit margins                     | auto-fit / manual axes spacing       |
|  [09]   | `pyplot.close`        | figure handle or `'all'`                                                     | release a figure                     |

[ENTRYPOINT_SCOPE]: axes plotting methods
- rail: visuals

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                            | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------- | :-------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Axes.plot` / `Axes.step` / `Axes.errorbar` | x/y plus style/error spec            | line / step / error-bar plot                                   |
|  [02]   | `Axes.scatter`                           | x/y plus size/color/cmap/norm           | scatter plot (ScalarMappable when `c` is data)                 |
|  [03]   | `Axes.bar` / `Axes.barh` / `Axes.stem`   | x/height plus width                     | bar / horizontal-bar / stem plot                               |
|  [04]   | `Axes.hist` / `Axes.hist2d` / `Axes.hexbin` | data plus bins/range                 | 1D / 2D histogram / hexbin density                             |
|  [05]   | `Axes.boxplot` / `Axes.violinplot`       | data plus position/width                | box / violin distribution plot                                 |
|  [06]   | `Axes.imshow` / `Axes.matshow`           | array plus cmap/norm/extent             | raster image display                                           |
|  [07]   | `Axes.contour` / `Axes.contourf`         | x/y/z plus levels/cmap                  | line / filled contour                                          |
|  [08]   | `Axes.pcolormesh` / `Axes.pcolorfast`    | x/y/c plus cmap/norm                    | pseudocolor mesh                                               |
|  [09]   | `Axes.quiver` / `Axes.streamplot`        | x/y/u/v plus arrow/stream policy        | vector-field / streamline plot                                 |
|  [10]   | `Axes.fill_between` / `Axes.stackplot`   | x plus y1/y2 / stacked series           | filled band / stacked area                                     |
|  [11]   | `Axes.set_title` / `set_xlabel` / `set_ylabel` / `legend` / `annotate` / `set_xscale` | text/scale policy | axes labelling, legend, annotation, log/linear scale |

## [04]-[IMPLEMENTATION_LAW]

[CHART_PUBLICATION]:
- import: `import matplotlib; matplotlib.use("Agg"); from matplotlib.figure import Figure` at boundary scope only; select a non-interactive backend (`Agg`/`pdf`/`svg`) before constructing figures for the host-free offscreen path.
- figure axis: the object-oriented `Figure` plus `subplots`/`add_subplot`/`add_axes`/`add_gridspec` is the canonical owner for the offscreen render; the stateful `pyplot` factory is convenience-only and never carries cross-figure state into the owner.
- plotting axis: the `Axes` plot-method family (`plot`/`scatter`/`bar`/`hist`/`boxplot`/`violinplot`/`imshow`/`contourf`/`pcolormesh`/`quiver`/`streamplot`/`stackplot`) is one plotting surface; each chart kind is an axes method row, never a parallel chart type.
- color axis: colormaps come from the `matplotlib.colormaps` `ColormapRegistry` by name (`colormaps['viridis']`) and qualitative palettes from `color_sequences`, paired with a `Normalize`/`LogNorm`/`SymLogNorm`/`BoundaryNorm`/`TwoSlopeNorm` norm on a `ScalarMappable`; color is a registry-plus-norm row, never an ad-hoc palette and never the removed `cm.get_cmap` accessor.
- save axis: `Figure.savefig` keys the output format by the target extension or explicit `format`; the canvas-supported set is `png`/`pdf`/`svg`/`svgz`/`eps`/`ps`/`pgf`/`raw`/`rgba`/`jpeg`/`jpg`/`tiff`/`tif`/`webp`/`avif`/`gif`; `dpi`/`bbox_inches`/`pad_inches`/`transparent`/`metadata` ride save kwargs, never a parallel exporter; multi-page PDF rides `backend_pdf.PdfPages`.
- evidence: each render captures figure size, dpi, axes count, colormap/norm identity, output format, and output byte length as a visuals receipt.
- boundary: matplotlib owns imperative publication 2D plotting and offscreen raster/vector export; declarative Vega charts route to `vl-convert-python`/`vegafusion`/`altair`; 3D scientific scenes route to `pyvista`/`vtk`; publication tables route to `great-tables`; color-science models route to `coloraide`/`colour-science`; raster encode rides the bundled `pillow`; live interactive backends stay outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `matplotlib`
- Owns: imperative 2D plotting, figure layout, the colormap registry and norm family, ticking/formatting, offscreen rasterization/vectorization to PNG/PDF/SVG/EPS/PS/PGF (and pillow-backed JPEG/TIFF/WebP/AVIF)
- Accept: publication chart render on the gated subprocess seam feeding the chart and export-bundle owners
- Reject: wrapper-renames of `savefig`; an interactive backend on the host-free path; a hand-rolled colormap or the removed `cm.get_cmap` accessor where the `colormaps` registry exists; a per-chart-kind type where axes methods suffice; a declarative-Vega or 3D claim matplotlib does not own

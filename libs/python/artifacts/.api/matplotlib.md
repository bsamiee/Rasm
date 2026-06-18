# [PY_ARTIFACTS_API_MATPLOTLIB]

`matplotlib` supplies the publication 2D plotting surface for the artifacts visuals rail: a `Figure` container, an `Axes` plotting unit, the stateful `pyplot` factory family, and the Agg/PDF/SVG backend canvases that drive imperative chart construction and offscreen rasterization to PNG/PDF/SVG. The package owner composes `Figure`, `Axes`, and `Figure.savefig` on the Agg backend into the chart owner; it never re-implements the rendering backends or the colormap registry matplotlib already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `matplotlib`
- package: `matplotlib`
- import: `matplotlib`
- owner: `artifacts`
- rail: visuals
- installed: `3.11.0` reflected via `python -c "import matplotlib"` on the gated `python_version<'3.15'` band (cp313); the cp315-core owner never imports it and renders on the runtime subprocess seam
- entry points: none (library only)
- capability: imperative 2D plotting (line/scatter/bar/hist/image/contour/pie/box), figure layout, colormaps and norms, ticking/formatting, and offscreen rasterization to PNG/PDF/SVG/EPS/PGF via the Agg backend

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: figure, axes, and backend types
- rail: visuals

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `figure.Figure`                        | figure root    | top container owning axes and `savefig`     |
|   [2]   | `axes.Axes`                            | plotting unit  | one plot region with the plot-method family |
|   [3]   | `figure.SubFigure`                     | nested figure  | sub-figure inside a figure                  |
|   [4]   | `gridspec.GridSpec`                    | layout grid    | row/column axes layout                      |
|   [5]   | `colorbar.Colorbar`                    | colorbar       | scalar-mappable color legend                |
|   [6]   | `backends.backend_agg.FigureCanvasAgg` | raster canvas  | Agg PNG/raw rasterizer                      |
|   [7]   | `backends.backend_pdf.PdfPages`        | pdf sink       | multi-page PDF writer                       |

[PUBLIC_TYPE_SCOPE]: color, norm, and tick types
- rail: visuals

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE] | [CAPABILITY]                      |
| :-----: | :------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `colors.Colormap`                | colormap base  | scalar-to-RGBA mapping            |
|   [2]   | `colors.ListedColormap`          | colormap       | discrete listed colormap          |
|   [3]   | `colors.LinearSegmentedColormap` | colormap       | interpolated colormap             |
|   [4]   | `colors.Normalize`               | norm           | linear data-to-unit normalization |
|   [5]   | `colors.LogNorm`                 | norm           | logarithmic normalization         |
|   [6]   | `colors.BoundaryNorm`            | norm           | discrete-boundary normalization   |
|   [7]   | `ticker.MaxNLocator`             | locator        | tick locator (also `LogLocator`)  |
|   [8]   | `ticker.FuncFormatter`           | formatter      | callable tick label formatter     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: figure construction and save
- rail: visuals

`pyplot` rows are the stateful factory; the `Figure` rows are the object-oriented surface the offscreen path uses.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                        | [CAPABILITY]                         |
| :-----: | :-------------------- | :-------------------------------------------------- | :----------------------------------- |
|   [1]   | `pyplot.figure`       | `figure(num=None, figsize=None, dpi=None, ...)`     | create or activate a figure          |
|   [2]   | `pyplot.subplots`     | `subplots(nrows=1, ncols=1, ...) -> (Figure, Axes)` | create a figure plus axes grid       |
|   [3]   | `figure.Figure`       | `Figure(figsize=None, dpi=None, ...)`               | construct a figure (object-oriented) |
|   [4]   | `Figure.add_subplot`  | grid position plus projection                       | add one axes to a figure             |
|   [5]   | `Figure.savefig`      | `savefig(fname, *, transparent=None, **kwargs)`     | render to file/stream by extension   |
|   [6]   | `Figure.tight_layout` | optional pad policy                                 | auto-fit axes within the figure      |
|   [7]   | `Figure.colorbar`     | mappable plus axes target                           | add a colorbar to the figure         |
|   [8]   | `pyplot.close`        | figure handle or `'all'`                            | release a figure                     |

[ENTRYPOINT_SCOPE]: axes plotting methods
- rail: visuals

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                            | [CAPABILITY]                                                    |
| :-----: | :------------------ | :-------------------------------------- | :-------------------------------------------------------------- |
|   [1]   | `Axes.plot`         | x/y plus style spec                     | line plot                                                       |
|   [2]   | `Axes.scatter`      | x/y plus size/color/cmap                | scatter plot                                                    |
|   [3]   | `Axes.bar`          | x/height plus width (also `barh`)       | bar plot                                                        |
|   [4]   | `Axes.hist`         | data plus bins/range                    | histogram                                                       |
|   [5]   | `Axes.imshow`       | array plus cmap/norm/extent             | raster image display                                            |
|   [6]   | `Axes.contourf`     | x/y/z plus levels/cmap (also `contour`) | filled contour                                                  |
|   [7]   | `Axes.pcolormesh`   | x/y/c plus cmap/norm                    | pseudocolor mesh                                                |
|   [8]   | `Axes.fill_between` | x plus y1/y2 and where mask             | filled band between curves                                      |
|   [9]   | `Axes.set_title`    | text plus font policy                   | axes title (also `set_xlabel`/`set_ylabel`/`legend`/`annotate`) |

## [4]-[IMPLEMENTATION_LAW]

[CHART_PUBLICATION]:
- import: `import matplotlib; matplotlib.use("Agg"); from matplotlib.figure import Figure` at boundary scope only; select a non-interactive backend (`Agg`/`pdf`/`svg`) before constructing figures for the host-free offscreen path.
- figure axis: the object-oriented `Figure` plus `add_subplot`/`add_axes` is the canonical owner for the offscreen render; the stateful `pyplot` factory is convenience-only and never carries cross-figure state into the owner.
- plotting axis: the `Axes` plot-method family (`plot`/`scatter`/`bar`/`hist`/`imshow`/`contourf`/`pcolormesh`) is one plotting surface; each chart kind is an axes method row, never a parallel chart type.
- color axis: colormaps come from the `colormaps`/`cm` registry by name and pair with a `Normalize`/`LogNorm`/`BoundaryNorm` norm; color is a registry-plus-norm row, never an ad-hoc palette.
- save axis: `Figure.savefig` keys the output format by the target extension or explicit `format`; the supported set includes `png`/`pdf`/`svg`/`svgz`/`eps`/`ps`/`pgf`/`jpeg`/`webp`/`tiff`/`raw`; `dpi`/`bbox_inches`/`transparent` ride save kwargs, never a parallel exporter.
- evidence: each render captures figure size, dpi, axes count, output format, and output byte length as a visuals receipt.
- boundary: matplotlib owns publication 2D plotting and offscreen raster/vector export; declarative Vega charts route to `vl-convert-python`; interactive plotly charts route to `plotly`/`kaleido`; 3D scientific scenes route to `pyvista`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `matplotlib`
- Owns: imperative 2D plotting, figure layout, colormaps/norms, ticking/formatting, offscreen rasterization to PNG/PDF/SVG/EPS/PGF
- Accept: publication chart render on the gated subprocess seam feeding the chart and export-bundle owners
- Reject: wrapper-renames of `savefig`; an interactive backend on the host-free path; a hand-rolled colormap where the registry exists; a per-chart-kind type where axes methods suffice

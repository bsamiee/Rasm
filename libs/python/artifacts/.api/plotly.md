# [PY_ARTIFACTS_API_PLOTLY]

`plotly` supplies the interactive figure surface for the artifacts visuals rail across two submodules: `plotly.graph_objects` for the `Figure` model and trace types, and `plotly.io` for JSON/HTML/static-image serialization. The package owner composes `graph_objects.Figure`, `io.to_image`, and `io.write_html` into the visuals owner; it never re-implements the plotly.js rendering grammar the library already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `plotly`
- package: `plotly`
- import: `plotly.graph_objects` / `plotly.io`
- owner: `artifacts`
- rail: visuals
- installed: `6.8.0` reflected via `python -c "import plotly"` on cp315
- entry points: none (library only)
- capability: interactive figure construction (2D/3D/statistical/geo traces), JSON/HTML serialization, static image export via the kaleido engine, templated themes

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: figure model and submodules
- rail: visuals

Trace rows cover `Scatter`, `Bar`, `Heatmap`, `Surface`, `Scatter3d`, `Mesh3d`, and `Choropleth` as one figure trace family.

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]       | [CAPABILITY]                    |
| :-----: | :----------------------- | :------------------- | :------------------------------ |
|   [1]   | `graph_objects.Figure`   | figure model         | traces, layout, and frames      |
|   [2]   | `graph_objects.*` traces | trace types          | 2D/3D/statistical/geo traces    |
|   [3]   | `graph_objects.Layout`   | layout               | axes, legend, annotation, scene |
|   [4]   | `io`                     | serialization module | JSON/HTML/image emit            |
|   [5]   | `io.templates`           | template registry    | named figure-theme axis         |
|   [6]   | `io.renderers`           | renderer registry    | output renderer axis            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: figure construction
- rail: visuals

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                       | [CAPABILITY]     |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------- | :--------------- |
|   [1]   | `graph_objects.Figure` | `Figure(data=None, layout=None, frames=None, skip_invalid=False, **kwargs)`        | build a figure   |
|   [2]   | `Figure.add_trace`     | `add_trace(trace, row=None, col=None, secondary_y=None) -> Figure`                 | append a trace   |
|   [3]   | `Figure.update_layout` | `update_layout(dict1=None, overwrite=False, **kwargs) -> Figure`                   | configure layout |
|   [4]   | `Figure.update_traces` | `update_traces(patch=None, selector=None, row=None, col=None, **kwargs) -> Figure` | patch traces     |

[ENTRYPOINT_SCOPE]: serialization and export (`plotly.io`)
- rail: visuals

Static export rows share format, size, scale, validation, and `kaleido` engine policy; HTML rows share config, bundle, animation, size, and div-id policy.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                     | [CAPABILITY]                     |
| :-----: | :--------------- | :------------------------------- | :------------------------------- |
|   [1]   | `io.to_image`    | figure plus static export policy | static image bytes (PNG/SVG/PDF) |
|   [2]   | `io.write_image` | figure, file, and export policy  | write static image to file       |
|   [3]   | `io.to_json`     | figure plus JSON policy          | figure JSON                      |
|   [4]   | `io.from_json`   | JSON value plus output policy    | parse figure JSON                |
|   [5]   | `io.write_html`  | figure, file, and HTML policy    | interactive HTML export          |
|   [6]   | `io.to_html`     | figure plus HTML policy -> `str` | interactive HTML string          |

## [4]-[IMPLEMENTATION_LAW]

[VISUALS_PLOTLY]:
- import: `import plotly.graph_objects as go` / `import plotly.io as pio` at boundary scope only; module-level import is banned by the manifest import policy.
- figure axis: one `graph_objects.Figure` owns every trace kind; `add_trace` with a trace type is a row, never a parallel figure type per chart.
- export axis: `io.to_image`/`write_image` (static, via the `kaleido` engine) and `io.write_html`/`to_html` (interactive) are the export rows; the static path names `engine='kaleido'`, never a re-minted rasterizer.
- serialization axis: `io.to_json`/`from_json` is the round-trip pair feeding the wire/document owners.
- evidence: each figure captures trace count, trace kinds, export format, and output byte length as a visuals receipt.
- boundary: plotly owns interactive figures; static rendering uses `kaleido`; declarative Vega-Lite routes to `altair`; raster matplotlib output to `matplotlib`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `plotly`
- Owns: interactive figure construction, JSON/HTML serialization, static image export via kaleido, templated themes
- Accept: interactive figures and their static/HTML exports feeding the visuals owner
- Reject: wrapper-renames of `add_trace`/`to_image`; a hand-built figure dict where the model is admitted; a re-minted rasterizer where `kaleido` renders; identity minting the runtime owns

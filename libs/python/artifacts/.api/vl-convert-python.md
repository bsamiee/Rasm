# [PY_ARTIFACTS_API_VL_CONVERT_PYTHON]

`vl-convert-python` supplies the Rust-backed Vega/Vega-Lite static-render surface for the artifacts visuals rail: a converter function family that renders Vega-Lite and Vega specs to SVG/PNG/JPEG/PDF/HTML/scenegraph plus font-registration and version-query helpers, with no browser or Node runtime. The package owner composes `vegalite_to_svg`, `vegalite_to_png`, and the format-converter family into the visuals render path; it never re-implements the Vega rendering pipeline the embedded engine already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vl-convert-python`
- package: `vl-convert-python`
- import: `vl_convert`
- owner: `artifacts`
- rail: visuals
- installed: `1.9.0.post1` reflected via `python -c "import vl_convert"` on cp315
- entry points: none (library only)
- capability: headless Vega-Lite/Vega-to-SVG/PNG/JPEG/PDF/HTML/scenegraph static rendering, font-directory registration, theme/version queries, locale formatting

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module surface
- rail: visuals

The module exposes a flat converter function family and helper functions; there are no public classes. The converter family is the render axis; the query/registration functions are the configuration axis.

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Vega-Lite converters
- rail: visuals

Converter rows share version, config, theme, warning, allowed-base-url, and locale policy; raster rows also carry scale, ppi, or quality policy.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                        | [CAPABILITY]           |
| :-----: | :----------------------- | :---------------------------------- | :--------------------- |
|   [1]   | `vegalite_to_svg`        | spec plus render policy -> `str`    | render to SVG          |
|   [2]   | `vegalite_to_png`        | spec plus raster policy -> `bytes`  | render to PNG          |
|   [3]   | `vegalite_to_jpeg`       | spec plus quality policy -> `bytes` | render to JPEG         |
|   [4]   | `vegalite_to_pdf`        | spec plus render policy -> `bytes`  | render to PDF          |
|   [5]   | `vegalite_to_html`       | spec plus bundle policy -> `str`    | render to HTML         |
|   [6]   | `vegalite_to_vega`       | spec plus compile policy -> `dict`  | compile to a Vega spec |
|   [7]   | `vegalite_to_scenegraph` | spec plus render policy -> `dict`   | render to scenegraph   |
|   [8]   | `vegalite_to_url`        | spec plus fullscreen flag -> `str`  | editor URL             |

[ENTRYPOINT_SCOPE]: Vega converters and configuration
- rail: visuals

Vega converter rows share allowed-base-url and locale policy; helper rows query or register render configuration.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                       | [CAPABILITY]                 |
| :-----: | :------------------------ | :--------------------------------- | :--------------------------- |
|   [1]   | `vega_to_svg`             | spec plus render policy -> `str`   | render a Vega spec to SVG    |
|   [2]   | `vega_to_png`             | spec plus raster policy -> `bytes` | render a Vega spec to PNG    |
|   [3]   | `vega_to_pdf`             | spec plus render policy -> `bytes` | render a Vega spec to PDF    |
|   [4]   | `register_font_directory` | font directory path                | register custom fonts        |
|   [5]   | `get_themes`              | no-arg query -> `dict`             | available named themes       |
|   [6]   | `get_vegalite_versions`   | no-arg query -> `list[str]`        | supported Vega-Lite versions |
|   [7]   | `javascript_bundle`       | snippet plus Vega-Lite version     | bundle JS for embedding      |

## [4]-[IMPLEMENTATION_LAW]

[VISUALS_RENDER]:
- import: `import vl_convert` at boundary scope only; module-level import is banned by the manifest import policy.
- render axis: the `vegalite_to_*`/`vega_to_*` family is one converter surface keyed by output format and spec dialect; format and dialect are the two row axes, never a per-format renderer class.
- font axis: `register_font_directory` is the single font-provisioning hook called once before render; custom fonts are a row, never a parallel render path.
- version axis: `get_vegalite_versions`/`get_themes` answer the spec-compatibility query; the chart owner pins `vl_version` from this list.
- evidence: each render captures spec dialect, output format, scale/ppi, theme, and output byte length as a visuals receipt.
- boundary: vl-convert owns the headless Vega static render; `altair` produces the input spec; live UI stays outside this package.

[RAIL_LAW]:
- Package: `vl-convert-python`
- Owns: headless Vega-Lite/Vega static rendering to SVG/PNG/JPEG/PDF/HTML/scenegraph, font registration, version/theme queries
- Accept: spec-to-bytes rendering for `altair`/Vega output feeding the visuals and document owners
- Reject: wrapper-renames of `vegalite_to_*`; a Node/browser render path where the embedded engine renders; identity minting the runtime owns

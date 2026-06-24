# [PY_ARTIFACTS_API_VL_CONVERT_PYTHON]

`vl-convert-python` supplies the Rust-backed (Deno/V8-embedded) Vega/Vega-Lite static-render surface for the artifacts visuals rail: a flat converter family that renders Vega-Lite and Vega specs to SVG/PNG/JPEG/PDF/HTML/scenegraph, a standalone `svg_to_*` rasterizer that re-renders any SVG string (including a `typst`/`altair` SVG) to PNG/JPEG/PDF without re-invoking the chart engine, font-directory registration, d3-format/d3-time-format locale resolution, and a version/theme/JS-bundle query family — with no browser or Node runtime. The package owner composes the `vegalite_to_*`/`vega_to_*` render rows and the `svg_to_*` raster rows into the visuals render path; it never re-implements the Vega rendering pipeline the embedded engine already owns and never shells a headless browser for rasterization.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vl-convert-python`
- package: `vl-convert-python`
- import: `vl_convert`
- owner: `artifacts`
- rail: visuals
- license: BSD-3-Clause (bundles Deno/V8, Vega/Vega-Lite/Vega-Embed JS, and `resvg` for SVG rasterization)
- asset: runtime library (PyO3 extension over a Rust core embedding a Deno V8 isolate); `cp37-abi3` wheel, cp315-clean (`Requires-Python>=3.7`, no `python_version` marker)
- installed: `1.9.0` reflected via `import vl_convert` on cp315 (wheel `vl_convert_python-1.9.0.post1`; module `__version__` is `1.9.0`, the `.post1` is a wheel-only re-tag)
- entry points: none (library only)
- capability: headless Vega-Lite/Vega-to-SVG/PNG/JPEG/PDF/HTML/scenegraph static rendering; standalone SVG-string rasterization to PNG/JPEG/PDF; Vega-Lite-to-Vega compilation; editor-URL minting; font-directory registration; named-theme/config application; d3-format and d3-time-format locale resolution; Vega-Lite/Vega/Vega-Embed/Vega-Themes version queries; self-contained JS bundle synthesis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module surface
- rail: visuals

The module exposes a flat function family and no public classes. Three orthogonal axes: the spec converter family (`vegalite_to_*`/`vega_to_*`) keyed by dialect x output format; the SVG rasterizer family (`svg_to_*`) that re-renders a finished SVG string independent of any chart spec; and the configuration/query family (font registration, locale resolution, version/theme/bundle queries). Specs accept `str` (JSON) or `dict`; raster outputs are `bytes`, vector/text outputs are `str`, compile/scenegraph/theme/locale outputs are `dict`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Vega-Lite converters
- rail: visuals

Every Vega-Lite row shares `vl_version` (pin to a `get_vegalite_versions` entry; defaults to latest), `config` (chart-config dict), `theme` (named theme from `get_themes`), `show_warnings`, `allowed_base_urls` (external-data SSRF fence; default allows any), `format_locale`, and `time_format_locale` (name `str` or inline dict). Raster rows add `scale` (factor) and `ppi`; JPEG adds `quality`; HTML adds `bundle` (inline all deps vs CDN) and `renderer`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                                                                                                                            | [CAPABILITY]                            |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `vegalite_to_svg`        | `vegalite_to_svg(vl_spec, vl_version=None, config=None, theme=None, show_warnings=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `str`                  | render to SVG                           |
|  [02]   | `vegalite_to_png`        | `vegalite_to_png(vl_spec, vl_version=None, scale=None, ppi=None, config=None, theme=None, show_warnings=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes` | render to PNG (`scale`+`ppi`)    |
|  [03]   | `vegalite_to_jpeg`       | `vegalite_to_jpeg(vl_spec, vl_version=None, scale=None, quality=None, config=None, theme=None, show_warnings=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes` | render to JPEG (`quality`) |
|  [04]   | `vegalite_to_pdf`        | `vegalite_to_pdf(vl_spec, vl_version=None, scale=None, config=None, theme=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes`                        | render to vector PDF                    |
|  [05]   | `vegalite_to_html`       | `vegalite_to_html(vl_spec, vl_version=None, bundle=None, config=None, theme=None, format_locale=None, time_format_locale=None, renderer=None)` -> `str`                                 | self-contained HTML (`bundle`=inline)   |
|  [06]   | `vegalite_to_vega`       | `vegalite_to_vega(vl_spec, vl_version=None, config=None, theme=None, show_warnings=None)` -> `dict`                                                                                     | compile to a Vega spec                  |
|  [07]   | `vegalite_to_scenegraph` | `vegalite_to_scenegraph(vl_spec, vl_version=None, config=None, theme=None, show_warnings=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `dict`          | render to Vega scenegraph               |
|  [08]   | `vegalite_to_url`        | `vegalite_to_url(vl_spec, fullscreen=None)` -> `str`                                                                                                                                   | Vega editor share URL                   |

[ENTRYPOINT_SCOPE]: Vega converters and SVG rasterizer
- rail: visuals

Vega rows take a compiled `vg_spec` and share `allowed_base_urls`/`format_locale`/`time_format_locale`; the `svg_to_*` family re-renders a finished SVG string (the same `resvg` core that raster Vega rows use) so a `vegalite_to_svg`/`typst`-SVG/`altair`-SVG output rasterizes without re-running the chart engine.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                              | [CAPABILITY]                          |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `vega_to_svg`           | `vega_to_svg(vg_spec, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `str`                      | render a Vega spec to SVG             |
|  [02]   | `vega_to_png`           | `vega_to_png(vg_spec, scale=None, ppi=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes` | render a Vega spec to PNG          |
|  [03]   | `vega_to_jpeg`          | `vega_to_jpeg(vg_spec, scale=None, quality=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes` | render a Vega spec to JPEG      |
|  [04]   | `vega_to_pdf`           | `vega_to_pdf(vg_spec, scale=None, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `bytes`        | render a Vega spec to vector PDF      |
|  [05]   | `vega_to_html`          | `vega_to_html(vg_spec, bundle=None, format_locale=None, time_format_locale=None, renderer=None)` -> `str`                 | self-contained HTML for a Vega spec   |
|  [06]   | `vega_to_scenegraph`    | `vega_to_scenegraph(vg_spec, allowed_base_urls=None, format_locale=None, time_format_locale=None)` -> `dict`              | render a Vega spec to scenegraph      |
|  [07]   | `vega_to_url`           | `vega_to_url(vg_spec, fullscreen=None)` -> `str`                                                                          | Vega editor share URL                 |
|  [08]   | `svg_to_png`            | `svg_to_png(svg, scale=None, ppi=None)` -> `bytes`                                                                        | rasterize any SVG string to PNG       |
|  [09]   | `svg_to_jpeg`           | `svg_to_jpeg(svg, scale=None, quality=None)` -> `bytes`                                                                   | rasterize any SVG string to JPEG      |
|  [10]   | `svg_to_pdf`            | `svg_to_pdf(svg, scale=None)` -> `bytes`                                                                                  | wrap any SVG string in a vector PDF   |

[ENTRYPOINT_SCOPE]: configuration, locale, and version queries
- rail: visuals

`register_font_directory` provisions fonts once before any render (the embedded `resvg` resolves only registered + system faces). The `get_*_locale` helpers return d3 locale dicts that feed the `format_locale`/`time_format_locale` render args. The version/theme/bundle queries pin the engine's compatibility surface.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                              | [CAPABILITY]                                          |
| :-----: | :------------------------- | :------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `register_font_directory`  | `register_font_directory(font_dir)` -> `None`            | register a custom-font directory for later renders    |
|  [02]   | `get_themes`               | `get_themes()` -> `dict`                                 | config dict for each built-in named theme             |
|  [03]   | `get_format_locale`        | `get_format_locale(name)` -> `dict`                      | d3-format locale dict for a named locale (`'it-IT'`)  |
|  [04]   | `get_time_format_locale`   | `get_time_format_locale(name)` -> `dict`                 | d3-time-format locale dict for a named locale         |
|  [05]   | `get_vegalite_versions`    | `get_vegalite_versions()` -> `list[str]`                 | bundled Vega-Lite versions (`vl_version` domain)      |
|  [06]   | `get_vega_version`         | `get_vega_version()` -> `str`                            | bundled Vega runtime version                          |
|  [07]   | `get_vega_embed_version`   | `get_vega_embed_version()` -> `str`                      | bundled Vega-Embed version (HTML/bundle path)         |
|  [08]   | `get_vega_themes_version`  | `get_vega_themes_version()` -> `str`                     | bundled Vega-Themes version (`theme` domain)          |
|  [09]   | `javascript_bundle`        | `javascript_bundle(snippet=None, vl_version=None)` -> `str` | self-contained Vega/Vega-Lite/Vega-Embed JS bundle |
|  [10]   | `get_local_tz`             | `get_local_tz()` -> `str \| None`                        | the IANA timezone Vega uses for time-axis math (e.g. `'America/New_York'`); pin it so a server render matches an authored time axis |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_RENDER]:
- import: `import vl_convert` at boundary scope only; module-level import is banned by the manifest import policy.
- render axis: the `vegalite_to_*`/`vega_to_*` family is one converter surface keyed by output format x spec dialect; format and dialect are the two row axes, never a per-format renderer class. Vega-Lite is the authoring dialect; `vegalite_to_vega` materializes the compiled Vega when the owner wants to cache/inspect the compilation before raster.
- raster reuse axis: `svg_to_png`/`svg_to_jpeg`/`svg_to_pdf` rasterize a finished SVG string through the same embedded `resvg` core the raster chart rows use; the owner renders `vegalite_to_svg` once and rasters that SVG to multiple targets rather than re-running the V8 chart pipeline per format. This is also the seam that rasters a `typst` SVG or an `altair` SVG without admitting a second rasterizer.
- font axis: `register_font_directory` is the single font-provisioning hook called once before render; the embedded engine resolves only registered + system faces, so custom fonts are a row, never a parallel render path. Fonts feeding this hook are the same OTF/TTF the `fonttools`/`uharfbuzz` rails own — the SVG/raster glyphs match the document's shaped text only when the directory is registered first.
- locale axis: `get_format_locale`/`get_time_format_locale` return d3 locale dicts the owner threads into `format_locale`/`time_format_locale`; `get_local_tz()` reports the IANA tz Vega applies to time axes, pinned alongside the time-format locale so a headless server render reproduces the authored time axis deterministically (no host-tz drift); localization is a render-arg row, never a re-formatted spec.
- version axis: `get_vegalite_versions`/`get_themes`/`get_vega_version`/`get_vega_embed_version`/`get_vega_themes_version` answer the spec-compatibility query; the chart owner pins `vl_version`/`theme` from these lists so the static render matches the spec's authored dialect.
- evidence: each render captures spec dialect, output format, scale/ppi, `vl_version`, theme, allowed-base-url policy, and output byte length as a visuals receipt.
- boundary: vl-convert owns the headless Vega static render and SVG rasterization; `altair` produces the input Vega-Lite spec; `vegafusion` pre-evaluates the spec's data transforms server-side before this render; the rendered PDF/PNG composes into `pymupdf`/`reportlab` document owners; live UI stays outside this package.

[STACK_INTEGRATION]:
- `altair` -> `vegafusion` -> `vl_convert`: `altair.Chart.to_dict()` yields the Vega-Lite spec; `vegafusion.runtime.pre_transform_spec` server-evaluates its transforms (collapsing large inline datasets to pre-computed tables); the pre-transformed spec then renders here via `vegalite_to_svg`/`vegalite_to_png` with no client-side transform work and no oversized embedded data.
- `vl_convert` -> document owners: `vegalite_to_pdf`/`vega_to_pdf` emit a vector PDF page the `pymupdf` rail merges into a multi-page artifact; `vegalite_to_svg` -> `svg_to_pdf` is the alternate SVG-first path when the owner post-edits the SVG (scale-to-fit, crop via `svgelements`) before rasterizing.
- shared-rasterizer rail: `svg_to_png`/`svg_to_pdf` is the one place a `typst`-emitted SVG or any `altair`/Vega SVG converges on the bundled `resvg` core, so the visuals rail keeps a single SVG-to-raster owner instead of admitting `resvg-py` and this engine in parallel for chart-origin SVGs.

[RAIL_LAW]:
- Package: `vl-convert-python`
- Owns: headless Vega-Lite/Vega static rendering to SVG/PNG/JPEG/PDF/HTML/scenegraph, standalone SVG-string rasterization, Vega-Lite-to-Vega compilation, font registration, d3 locale resolution, and version/theme/bundle queries
- Accept: spec-to-bytes rendering for `altair`/`vegafusion` output feeding the visuals and document owners; SVG-to-raster for chart-origin SVG strings
- Reject: wrapper-renames of `vegalite_to_*`; a per-format renderer class where format is a row; a Node/browser render path where the embedded V8 core renders; a second SVG rasterizer admitted in parallel for chart-origin SVGs; identity minting the runtime owns

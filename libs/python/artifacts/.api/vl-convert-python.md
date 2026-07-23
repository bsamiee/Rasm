# [PY_ARTIFACTS_API_VL_CONVERT_PYTHON]

`vl-convert-python` renders a Vega-Lite or Vega spec to SVG/PNG/JPEG/PDF/HTML/scenegraph and rasterizes any finished SVG string to PNG/JPEG/PDF, both over one embedded Rust stack — `deno_core`/V8 running the inlined Vega JS, `resvg`/`usvg`/`tiny-skia` doing the raster — with no browser, Node, or external process. It is the `artifacts` host-free chart-export engine and the shared chart-SVG raster floor every `altair`/Vega/`lets-plot`/`typst` SVG converges on; the universal `libs/python/.api` rails own the receipt, log, span, result, and retry discipline around each render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vl-convert-python`
- package: `vl-convert-python` (`BSD-3-Clause` wheel; embedded `deno_core`/V8, the Vega/Vega-Lite/Vega-Embed JS, and `resvg`/`usvg`/`tiny-skia` are `MPL-2.0`/`zlib` compiled into the `.abi3.so`, no copyleft on the consuming Python)
- import: `vl_convert` (render alias `import vl_convert as vlc`)
- owner: `artifacts`
- rail: visuals — the `visualization/chart/export#EXPORT` host-free engine and the shared chart-SVG raster floor
- entry points: none (library only)
- capability: Vega-Lite/Vega static render to SVG/PNG/JPEG/PDF/HTML/scenegraph; standalone SVG-string rasterization to PNG/JPEG/PDF over the same embedded `resvg` core; Vega-Lite-to-Vega compilation; Vega-editor share-URL minting; font-directory registration; named-theme/config application; d3-format and d3-time-format locale resolution; IANA timezone reporting; bundled Vega-Lite/Vega/Vega-Embed/Vega-Themes version queries; self-contained JS-bundle synthesis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module surface and the typed render vocabularies

`__all__` is the flat function family (`vegalite_to_*`/`vega_to_*` spec converters, `svg_to_*` rasterizers, config/locale/tz/version helpers) with the `__version__` constant; no public class, no options object. A `vl_convert` submodule attribute binds on the native core, excluded from `__all__`; every call routes through the top-level names. `__init__.pyi` ships the `TYPE_CHECKING` vocabularies the design page composes against rather than raw strings — `theme`/`renderer`/`format_locale`/`time_format_locale` are closed domains.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `VlSpec`               | union         | `str \| dict[str, Any]` — every `*_spec`/`vg_spec` arg; JSON string or spec dict  |
|  [02]   | `VegaThemes`           | literal       | bundled named-theme domain; the `theme=` / `get_themes()` domain (values at [02]) |
|  [03]   | `Renderer`             | literal       | `'svg'` (default)/`'canvas'`/`'hybrid'` (`hybrid`=text SVG + marks canvas)        |
|  [04]   | `FormatLocaleName`     | literal       | d3-format named-locale; `FormatLocale` also admits an inline dict                 |
|  [05]   | `TimeFormatLocaleName` | literal       | d3-time-format named-locale; `TimeFormatLocale` also admits an inline dict        |

- [02]-[VEGATHEMES]: `carbong10` `carbong100` `carbong90` `carbonwhite` `dark` `excel` `fivethirtyeight` `ggplot2` `googlecharts` `latimes` `powerbi` `quartz` `urbaninstitute` `vox`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Vega-Lite converters

Every Vega-Lite converter shares the tail `vl_version` (a `get_vegalite_versions` entry, default latest), `config`, `theme`, `format_locale`, and `time_format_locale`; raster/SVG/scenegraph rows add `show_warnings` and `allowed_base_urls` (external-data allow-list — default `None` permits ANY base URL, so the owner passes an explicit `list[str]` to fence external data); raster rows add `scale`/`ppi`, JPEG adds `quality` (0-100, default 90), HTML adds `bundle`/`renderer`. `vegalite_to_vega`/`vegalite_to_url` carry no locale/base-url tail; `...` abbreviates the shared tail.

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------- | :------------------------------- |
|  [01]   | `vegalite_to_svg(vl_spec, ...)` -> `str`                    | render to SVG                    |
|  [02]   | `vegalite_to_png(vl_spec, scale, ppi, ...)` -> `bytes`      | render to PNG                    |
|  [03]   | `vegalite_to_jpeg(vl_spec, scale, quality, ...)` -> `bytes` | render to JPEG                   |
|  [04]   | `vegalite_to_pdf(vl_spec, scale, ...)` -> `bytes`           | render to vector PDF             |
|  [05]   | `vegalite_to_html(vl_spec, bundle, renderer, ...)` -> `str` | self-contained HTML              |
|  [06]   | `vegalite_to_vega(vl_spec, ...)` -> `dict`                  | compile to a Vega spec dict      |
|  [07]   | `vegalite_to_scenegraph(vl_spec, ...)` -> `dict`            | render to a Vega scenegraph dict |
|  [08]   | `vegalite_to_url(vl_spec, fullscreen)` -> `str`             | Vega-editor share URL            |

[ENTRYPOINT_SCOPE]: Vega converters and SVG rasterizer

Vega rows take a compiled `vg_spec` (`VlSpec`) and share `allowed_base_urls`/`format_locale`/`time_format_locale` (with `scale`/`ppi`/`quality`/`bundle`/`renderer` where the output mode applies) but carry no `vl_version`/`config`/`theme` — Vega-Lite compile-time knobs already baked into the compiled spec. `svg_to_*` re-renders a finished SVG string through the same embedded `resvg` core the raster Vega rows use — only `svg`/`scale`/`ppi`/`quality`, no spec args.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `vega_to_svg(vg_spec, ...)` -> `str`                    | render a Vega spec to SVG                        |
|  [02]   | `vega_to_png(vg_spec, scale, ppi, ...)` -> `bytes`      | render a Vega spec to PNG                        |
|  [03]   | `vega_to_jpeg(vg_spec, scale, quality, ...)` -> `bytes` | render a Vega spec to JPEG                       |
|  [04]   | `vega_to_pdf(vg_spec, scale, ...)` -> `bytes`           | render a Vega spec to vector PDF                 |
|  [05]   | `vega_to_html(vg_spec, bundle, renderer, ...)` -> `str` | self-contained HTML for a Vega spec              |
|  [06]   | `vega_to_scenegraph(vg_spec, ...)` -> `dict`            | render a Vega spec to a scenegraph               |
|  [07]   | `vega_to_url(vg_spec, fullscreen)` -> `str`             | Vega-editor share URL                            |
|  [08]   | `svg_to_png(svg, scale, ppi)` -> `bytes`                | rasterize any SVG string to PNG                  |
|  [09]   | `svg_to_jpeg(svg, scale, quality)` -> `bytes`           | rasterize any SVG to JPEG (`quality` default 90) |
|  [10]   | `svg_to_pdf(svg, scale)` -> `bytes`                     | wrap any SVG string in a vector PDF              |

[ENTRYPOINT_SCOPE]: configuration, locale, timezone, and version queries

`register_font_directory` provisions fonts once before any render — the embedded engine resolves only registered + system faces. `get_format_locale`/`get_time_format_locale` return d3 locale dicts feeding the `format_locale`/`time_format_locale` render args; `get_local_tz` reports the IANA tz Vega applies to time axes. `javascript_bundle` requires its `snippet` (no default); pass `""` for the bare library bundle. Version/theme queries answer the engine's compatibility surface live.

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | `register_font_directory(font_dir)` -> `None`     | register a custom-font directory for later renders   |
|  [02]   | `get_themes()` -> `dict[VegaThemes, dict]`        | config dict for each built-in named theme            |
|  [03]   | `get_format_locale(name)` -> `dict`               | d3-format locale dict for a named locale             |
|  [04]   | `get_time_format_locale(name)` -> `dict`          | d3-time-format locale dict for a named locale        |
|  [05]   | `get_vegalite_versions()` -> `list[str]`          | bundled Vega-Lite versions (the `vl_version` domain) |
|  [06]   | `get_vega_version()` -> `str`                     | bundled Vega runtime version                         |
|  [07]   | `get_vega_embed_version()` -> `str`               | bundled Vega-Embed version (HTML/bundle path)        |
|  [08]   | `get_vega_themes_version()` -> `str`              | bundled Vega-Themes version (the `theme` domain)     |
|  [09]   | `javascript_bundle(snippet, vl_version)` -> `str` | self-contained Vega/Vega-Lite/Vega-Embed JS bundle   |
|  [10]   | `get_local_tz()` -> `str \| None`                 | the IANA tz Vega applies to time axes                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Render axis: `import vl_convert as vlc`, and `vegalite_to_*`/`vega_to_*` is one converter surface keyed by output format × spec dialect; `visualization/chart/export#EXPORT` carries the per-format member pair as a `VlRow(vl, vega, text)` cell in the `VL_RENDER` table totalled over `ExportFormat`, each typed cell calling the provider with explicit format-correct keywords while the spec's `$schema` selects the dialect. Vega-Lite is the authoring dialect; `vegalite_to_vega` materializes the compiled Vega for cache or inspection; `vegalite_to_scenegraph`/`vega_to_scenegraph` expose the resolved scenegraph dict for measurement without committing a raster.
- Raster-reuse axis: `svg_to_png`/`svg_to_jpeg`/`svg_to_pdf` rasterize a finished SVG string through the same embedded `resvg` core the raster chart rows use; the owner renders `vegalite_to_svg` once and rasters that SVG to every target rather than re-running the V8 pipeline per format.
- Font axis: `register_font_directory` is the one font hook, called once before render; the engine resolves only registered + system faces, so a custom face is a one-time registration, never a parallel render path.
- Locale/tz axis: `get_format_locale`/`get_time_format_locale` return d3 dicts threaded into `format_locale`/`time_format_locale`; `get_local_tz()` reports the IANA tz Vega applies to time axes, pinned beside the time-format locale so a headless render reproduces the authored time axis with no host-tz drift — localization is a render-arg row, never a re-formatted spec.
- Version axis: `get_vegalite_versions`/`get_themes`/`get_vega_version`/`get_vega_embed_version`/`get_vega_themes_version` answer the spec-compatibility query live; the owner pins `vl_version`/`theme` from them and records the returned actuals on the receipt — the live query is the truth, never the `.pyi` docstring strings.
- Fault axis: a malformed spec, unparseable option, or render failure raises one `ValueError` (a compile error carries the Vega-Lite warning text under `show_warnings`); the owner names that raise at the render arm and folds it onto the `expression.Result` rail as `RuntimeRail[ArtifactReceipt]`, never a bare `except Exception`.

[STACKING]:
- `altair`(`altair.md`) -> `vegafusion`(`vegafusion.md`) -> here: `altair.Chart.to_dict()` yields the Vega-Lite spec, the `visualization/chart/export#PREPASS` `VegaTransform.apply` server-evaluates its transforms on the `vegafusion` subprocess seam and inlines the reduced tables INTO one self-contained spec, which then renders through `vegalite_to_svg`/`vegalite_to_png`; no `inline_datasets=` feed exists, so the pre-computed data crosses inside the spec, and the `ChartState` self-contained spec serves the interactive HTML row with no live server.
- `pymupdf`(`pymupdf.md`)/`reportlab`: `vegalite_to_pdf`/`vega_to_pdf` emit a vector PDF page `pymupdf` merges into a multi-page artifact; `vegalite_to_svg` -> `svg_to_pdf` is the SVG-first path when the owner post-edits the SVG (scale-to-fit, crop via the `graphic/vector#VECTOR` `svgelements` algebra) before rasterizing.
- `resvg-py`(`resvg-py.md`): `svg_to_png`/`svg_to_jpeg`/`svg_to_pdf` is the sole chart-origin SVG-to-raster owner over the bundled `resvg` core; `resvg-py` `svg_to_bytes` is the disjoint floor for the net-new vector/glyph/QR/schematic/composed-figure SVG the `graphic/vector#VECTOR` primitive owns, never admitted in parallel for one SVG origin.
- `fonttools`(`../../.api/fonttools.md`)/`uharfbuzz`(`../../.api/uharfbuzz.md`): `register_font_directory` takes the exact OTF/TTF faces the subset + shaping rails produce, so the rasterized glyph outlines match the document's shaped text across hosts independent of system fonts.
- concurrency: runtime `LanePolicy` composes `anyio`(`../../.api/anyio.md`) offload and `stamina`(`../../.api/stamina.md`) retry — each render crosses `ChartExport.lane.offload` as a `KernelTrait.RELEASING` kernel and the `PREPASS` vegafusion work as `KernelTrait.HOSTILE` (worker-death retry on the HOSTILE row), so no V8 render blocks the event loop and no folder-local limiter exists.
- receipt: the rendered bytes contribute one `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, format, scale, theme, byte_len)` fact under one `structlog`(`../../.api/structlog.md`) event inside one `opentelemetry`(`../../.api/opentelemetry-api.md`) span, `msgspec`(`../../.api/msgspec.md`) carrying the fact and `expression.Result`(`../../.api/expression.md`) the typed failure; `ContentIdentity.key` keys the node from the length-framed canonical input preimage.

[LOCAL_ADMISSION]:
- Admitted as the primary host-free chart export for `altair`/`vegafusion` output feeding `visualization/chart/export#EXPORT`, and the shared chart-SVG raster floor every chart-origin (`altair`/Vega/`lets-plot`/`typst`) SVG converges on over the bundled `resvg` core. `altair` produces the input spec and `vegafusion` pre-evaluates its transforms; the rendered PDF/PNG composes into `composition/compose#COMPOSE` and the `pymupdf`/`reportlab` owners; live UI stays outside.

[RAIL_LAW]:
- Package: `vl-convert-python`
- Owns: headless Vega-Lite/Vega static render to SVG/PNG/JPEG/PDF/HTML/scenegraph, chart-origin SVG-string rasterization over the embedded `resvg` core, Vega-Lite-to-Vega compilation, Vega-editor URL minting, font registration, d3 locale + IANA tz resolution, and bundled version/theme/JS-bundle queries — no browser, no Node, no external process
- Accept: spec-to-bytes render for `altair`/`vegafusion` output feeding `visualization/chart/export#EXPORT` and the `composition/compose#COMPOSE`/document owners; chart-origin (`altair`/Vega/`lets-plot`/`typst`) SVG-to-raster; all wrapped by the universal `anyio`/`msgspec`/`structlog`/`opentelemetry`/`expression`/`stamina` rails
- Reject: a wrapper-rename of `vegalite_to_*`; a per-format renderer class where the `VL_RENDER` row table discriminates; a Node/browser render path the embedded V8 core forecloses; a `javascript_bundle` call omitting the required `snippet`; trust in a `.pyi` version string where the live `get_*_version` query is the receipt truth; an `inline_datasets=` feed no `vegalite_to_*` row carries; a second rasterizer for chart-origin SVG the bundled `resvg` core owns; an inline render on the event loop or a folder-local limiter beside `LanePolicy`; a parallel visuals-receipt type where `ArtifactReceipt.Chart` is the fact; identity the runtime owns

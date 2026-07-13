# [PY_ARTIFACTS_API_VL_CONVERT_PYTHON]

`vl-convert-python` supplies the Rust-backed (embedded `deno_core`/V8, inlined Vega/Vega-Lite/Vega-Embed JS, embedded `resvg`/`usvg`/`tiny-skia` rasterizer) Vega/Vega-Lite static-render surface the `visualization/chart/export#EXPORT` owner folds as its host-free engine: a flat converter family that renders Vega-Lite and Vega specs to SVG/PNG/JPEG/PDF/HTML/scenegraph, a standalone `svg_to_*` rasterizer that re-renders any SVG string (a `vegalite_to_svg`/`lets-plot`/`typst` SVG) to PNG/JPEG/PDF without re-invoking the chart engine, font-directory registration, d3-format/d3-time-format locale resolution, timezone reporting, and a bundled-version/named-theme/JS-bundle query family — with no browser, no Node, and no external process. The package owner places `vlc.vegalite_to_*`/`vlc.vega_to_*` converter members as cells in the `VL_RENDER` row table (each cell selected by `ExportFormat`, each fed exactly the policy params the row's `keys` column names through `RenderPolicy.projected`) and the `svg_to_*` raster rows as the shared chart-SVG raster floor; it never re-implements the Vega rendering pipeline the embedded engine already owns, never shells a headless browser for rasterization, and never admits a second rasterizer for chart-origin SVGs. The producer folds onto the universal `libs/python/.api` rails: each render is offloaded off the event loop through `anyio` (`../../.api/anyio.md`) `to_thread`/`to_process` under one `CapacityLimiter`, contributes one `msgspec` (`../../.api/msgspec.md`) `ArtifactReceipt.Chart` fact under one `structlog` (`../../.api/structlog.md`) event inside an `opentelemetry` (`../../.api/opentelemetry-api.md`) span, folds the engine `ValueError` onto the `expression.Result` (`../../.api/expression.md`) rail as a `RuntimeRail[ArtifactReceipt]`, and recovers a transient worker death through a bounded `stamina` (`../../.api/stamina.md`) retry — the catalog documents the render/raster surface, the universal rails own the receipt/log/span/result discipline around it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vl-convert-python`
- package: `vl-convert-python`
- import: `vl_convert` (canonical render-path alias `import vl_convert as vlc`)
- owner: `artifacts`
- rail: visuals (the `visualization/chart/export#EXPORT` host-free engine; the shared chart-SVG raster floor)
- license: BSD-3-Clause (PyO3/maturin extension; statically links embedded Deno/V8 `deno_core`, the Vega/Vega-Lite/Vega-Embed JS sources, and `resvg`/`usvg`/`tiny-skia` for SVG rasterization — MPL-2.0 + zlib engine compiled into the `.so`, no copyleft obligation on the consuming Python)
- build-floor: abi3 native wheel; cp315 wheel present and bound on this interpreter (`vl_convert/vl_convert.abi3.so`, `py.typed` + shipped `__init__.pyi`) — no `python_version` gate, no source build
- installed: `1.9.0.post1` (module `__version__ == '1.9.0'`)
- entry points: none (library only)
- capability: headless Vega-Lite/Vega-to-SVG/PNG/JPEG/PDF/HTML/scenegraph static rendering; standalone SVG-string rasterization to PNG/JPEG/PDF over the same embedded `resvg` core; Vega-Lite-to-Vega compilation; Vega-editor share-URL minting; font-directory registration; named-theme/config application; d3-format and d3-time-format locale resolution; IANA timezone reporting; bundled Vega-Lite/Vega/Vega-Embed/Vega-Themes version queries; self-contained Vega/Vega-Lite/Vega-Embed JS bundle synthesis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module surface and the typed render vocabularies
- rail: visuals

The reflected `__all__` is exactly 29 names — the 28-function flat family (the four `get_vega*`/`get_vegalite_versions` queries plus the 24 alphabetized converters/helpers) plus the `__version__` constant; there are no public classes and no options object. The native core also binds a `vl_convert` submodule attribute on the package, but it is excluded from `__all__` and never the owner's surface — every call routes through the top-level function names. The shipped `__init__.pyi` carries five `TYPE_CHECKING` `Literal`/union vocabularies the design page composes against rather than passing raw strings: `theme`/`renderer`/`format_locale`/`time_format_locale` are closed domains, not open `str`. Three orthogonal converter axes: the spec converter family (`vegalite_to_*`/`vega_to_*`) keyed by dialect × output format; the SVG rasterizer family (`svg_to_*`) re-rendering a finished SVG string independent of any chart spec; and the configuration/query family (font registration, locale/tz resolution, version/theme/bundle queries). Specs accept `str` (JSON) or `dict` (the `VlSpec` union); raster outputs are `bytes`, vector/text outputs are `str`, compile/scenegraph/theme/locale outputs are `dict`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                                                                                     |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `VlSpec`               | spec union         | `str \| dict[str, Any]` — every `*_spec`/`vg_spec` arg; JSON string or spec dict           |
|  [02]   | `VegaThemes`           | theme `Literal`    | the 14 bundled named themes (values at [02] below); the `theme=` / `get_themes()` domain   |
|  [03]   | `Renderer`             | renderer `Literal` | `'svg'` (default)/`'canvas'`/`'hybrid'`; the HTML `renderer=` domain (`hybrid`=SVG+canvas) |
|  [04]   | `FormatLocaleName`     | locale `Literal`   | d3-format named-locale (`'ar-001'`…`'zh-CN'`); `FormatLocale` also admits an inline dict   |
|  [05]   | `TimeFormatLocaleName` | locale `Literal`   | d3-time-format named-locale (`'ar-EG'`…`'zh-TW'`); `TimeFormatLocale` admits inline dict   |

- [02]-[VEGATHEMES]: `carbong10`/`carbong100`/`carbong90`/`carbonwhite`/`dark`/`excel`/`fivethirtyeight`/`ggplot2`/`googlecharts`/`latimes`/`powerbi`/`quartz`/`urbaninstitute`/`vox`.

[PUBLIC_TYPE_SCOPE]: bundled engine versions
- rail: visuals

The `get_*` version queries report the JS engine the `1.9.0.post1` wheel statically bundles; the chart owner pins `vl_version`/`theme` from these so the static render matches the spec's authored dialect. These are the live values on the installed wheel (the `.pyi` docstrings carry older illustrative strings — the runtime queries are the truth the receipt records).

| [INDEX] | [QUERY]                     | [BUNDLED_ACTUAL]                                                                                         |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `get_vega_version()`        | `'6.2.0'`                                                                                                |
|  [02]   | `get_vega_embed_version()`  | `'7.0.2'` (the HTML/`javascript_bundle` path)                                                            |
|  [03]   | `get_vega_themes_version()` | `'3.0.0'` (the `VegaThemes`/`theme=` domain)                                                             |
|  [04]   | `get_vegalite_versions()`   | `['5.8','5.14','5.15','5.16','5.17','5.20','5.21','6.1','6.4']`; the `vl_version` domain, default latest |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Vega-Lite converters
- rail: visuals

Every Vega-Lite row shares `vl_version` (a `get_vegalite_versions` entry, e.g. `'v6.4'`/`'6.4'`; default latest), `config` (chart-config dict), `theme` (a `VegaThemes` name from `get_themes`), `format_locale`, and `time_format_locale` (a `*LocaleName` or an inline dict). Raster/SVG/scenegraph rows add `show_warnings` and `allowed_base_urls` (the external-data SSRF fence — a `list[str]` of permitted base URLs; default allows any); raster rows add `scale` (factor) and `ppi`; JPEG adds `quality`; HTML adds `bundle` (inline all deps vs CDN) and `renderer` (a `Renderer`). `vegalite_to_vega`/`url` carry no locale/base-url args. The owner places `vlc.vegalite_to_svg`/`png`/`pdf`/`html`/`jpeg` as `VL_RENDER` table cells; `RenderPolicy.projected(row.keys)` spreads exactly the per-format admitted subset (`scale`/`ppi`/`theme`/`vl_version` on PNG, `theme`/`vl_version` on SVG/HTML, `scale`/`quality`/`theme`/`vl_version` on JPEG), never a positional wall and never a per-format render variant. Each call shape below abbreviates that shared tail as `...`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                | [CAPABILITY]                                    |
| :-----: | :----------------------- | :---------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `vegalite_to_svg`        | `vegalite_to_svg(vl_spec, ...)` -> `str`                    | render to SVG                                   |
|  [02]   | `vegalite_to_png`        | `vegalite_to_png(vl_spec, scale, ppi, ...)` -> `bytes`      | render to PNG (`scale`+`ppi`)                   |
|  [03]   | `vegalite_to_jpeg`       | `vegalite_to_jpeg(vl_spec, scale, quality, ...)` -> `bytes` | render to JPEG (`quality` 0–100, default 90)    |
|  [04]   | `vegalite_to_pdf`        | `vegalite_to_pdf(vl_spec, scale, ...)` -> `bytes`           | render to vector PDF                            |
|  [05]   | `vegalite_to_html`       | `vegalite_to_html(vl_spec, bundle, renderer, ...)` -> `str` | self-contained HTML; `bundle`, `renderer` knobs |
|  [06]   | `vegalite_to_vega`       | `vegalite_to_vega(vl_spec, ...)` -> `dict`                  | compile to a Vega spec dict                     |
|  [07]   | `vegalite_to_scenegraph` | `vegalite_to_scenegraph(vl_spec, ...)` -> `dict`            | render to a Vega scenegraph dict                |
|  [08]   | `vegalite_to_url`        | `vegalite_to_url(vl_spec, fullscreen=None)` -> `str`        | Vega-editor share URL                           |

[ENTRYPOINT_SCOPE]: Vega converters and SVG rasterizer
- rail: visuals

Vega rows take a compiled `vg_spec` (`VlSpec`) and share `allowed_base_urls`/`format_locale`/`time_format_locale` (and `scale`/`ppi`/`quality`/`bundle`/`renderer` where the output mode applies), but carry no `vl_version`/`config`/`theme` (those are Vega-Lite compile-time knobs already baked into the compiled spec). The `svg_to_*` family re-renders a finished SVG string through the same embedded `resvg` core the raster Vega rows use — only `svg`/`scale`/`ppi`/`quality`, no spec args — so a `vegalite_to_svg`/`lets-plot`/`typst` SVG rasterizes without re-running the V8 chart pipeline. Each Vega call shape abbreviates the shared tail as `...`.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                            | [CAPABILITY]                                     |
| :-----: | :------------------- | :------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `vega_to_svg`        | `vega_to_svg(vg_spec, ...)` -> `str`                    | render a Vega spec to SVG                        |
|  [02]   | `vega_to_png`        | `vega_to_png(vg_spec, scale, ppi, ...)` -> `bytes`      | render a Vega spec to PNG                        |
|  [03]   | `vega_to_jpeg`       | `vega_to_jpeg(vg_spec, scale, quality, ...)` -> `bytes` | render a Vega spec to JPEG                       |
|  [04]   | `vega_to_pdf`        | `vega_to_pdf(vg_spec, scale, ...)` -> `bytes`           | render a Vega spec to vector PDF                 |
|  [05]   | `vega_to_html`       | `vega_to_html(vg_spec, bundle, renderer, ...)` -> `str` | self-contained HTML for a Vega spec; `renderer`  |
|  [06]   | `vega_to_scenegraph` | `vega_to_scenegraph(vg_spec, ...)` -> `dict`            | render a Vega spec to a scenegraph               |
|  [07]   | `vega_to_url`        | `vega_to_url(vg_spec, fullscreen=None)` -> `str`        | Vega-editor share URL                            |
|  [08]   | `svg_to_png`         | `svg_to_png(svg, scale=None, ppi=None)` -> `bytes`      | rasterize any SVG string to PNG                  |
|  [09]   | `svg_to_jpeg`        | `svg_to_jpeg(svg, scale=None, quality=None)` -> `bytes` | rasterize any SVG to JPEG (`quality` default 90) |
|  [10]   | `svg_to_pdf`         | `svg_to_pdf(svg, scale=None)` -> `bytes`                | wrap any SVG string in a vector PDF              |

[ENTRYPOINT_SCOPE]: configuration, locale, timezone, and version queries
- rail: visuals

`register_font_directory` provisions fonts once before any render (the embedded `resvg` resolves only registered + system faces). The `get_*_locale` helpers return d3 locale dicts that feed the `format_locale`/`time_format_locale` render args; `get_local_tz` reports the IANA tz Vega applies to time axes. `javascript_bundle` requires its `snippet` argument (positional, no default — an empty default is a phantom; pass `""` only to suppress the global-window assignment). The version/theme queries pin the engine's compatibility surface.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                           | [CAPABILITY]                                       |
| :-----: | :------------------------ | :----------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `register_font_directory` | `register_font_directory(font_dir: str)` -> `None`     | register a custom-font directory for later renders |
|  [02]   | `get_themes`              | `get_themes()` -> `dict[VegaThemes, dict]`             | config dict for each built-in named theme          |
|  [03]   | `get_format_locale`       | `get_format_locale(name)` -> `dict`                    | d3-format locale dict for a named locale           |
|  [04]   | `get_time_format_locale`  | `get_time_format_locale(name)` -> `dict`               | d3-time-format locale dict for a named locale      |
|  [05]   | `get_vegalite_versions`   | `get_vegalite_versions()` -> `list[str]`               | bundled Vega-Lite versions (`vl_version` domain)   |
|  [06]   | `get_vega_version`        | `get_vega_version()` -> `str`                          | bundled Vega runtime version                       |
|  [07]   | `get_vega_embed_version`  | `get_vega_embed_version()` -> `str`                    | bundled Vega-Embed version (HTML/bundle path)      |
|  [08]   | `get_vega_themes_version` | `get_vega_themes_version()` -> `str`                   | bundled Vega-Themes version (the `theme` domain)   |
|  [09]   | `javascript_bundle`       | `javascript_bundle(snippet, vl_version=None)` -> `str` | self-contained Vega/Vega-Lite/Vega-Embed JS bundle |
|  [10]   | `get_local_tz`            | `get_local_tz()` -> `str \| None`                      | the IANA tz Vega applies to time axes              |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_RENDER]:
- import: `import vl_convert as vlc` at boundary scope only; module-level import is banned by the manifest import policy, and the native `.so` rides the render lane, never the event-loop import path.
- render axis: the `vegalite_to_*`/`vega_to_*` family is one converter surface keyed by output format × spec dialect; the `visualization/chart/export#EXPORT` owner carries the per-format member as a `VlRow(convert, text, keys)` cell in the module-level `VL_RENDER` table totalled over `ExportFormat`, never a per-format renderer class or a `match` arm. Vega-Lite is the authoring dialect; `vegalite_to_vega` materializes the compiled Vega when the owner caches/inspects the compilation before raster. `vegalite_to_scenegraph`/`vega_to_scenegraph` expose the resolved scenegraph dict for measurement/inspection without committing to a raster.
- raster reuse axis: `svg_to_png`/`svg_to_jpeg`/`svg_to_pdf` rasterize a finished SVG string through the same embedded `resvg` core the raster chart rows use; the owner renders `vegalite_to_svg` once and rasters that SVG to multiple targets rather than re-running the V8 chart pipeline per format. This is the seam that rasters a `typst` SVG, a `lets-plot` SVG (the `visualization/chart/export#EXPORT` lets-plot JPEG row rasterizes its SVG here since lets-plot ships no `to_jpeg`), or an `altair` SVG without admitting a second rasterizer.
- font axis: `register_font_directory` is the single font-provisioning hook called once before render; the embedded engine resolves only registered + system faces, so custom fonts are a one-time registration, never a parallel render path. Fonts feeding this hook are the same OTF/TTF the `fonttools`/`uharfbuzz` rails own — the SVG/raster glyphs match the document's shaped text only when the directory is registered first.
- locale/tz axis: `get_format_locale`/`get_time_format_locale` return d3 locale dicts the owner threads into `format_locale`/`time_format_locale` (a `*LocaleName` Literal or an inline dict); `get_local_tz()` reports the IANA tz Vega applies to time axes, pinned alongside the time-format locale so a headless server render reproduces the authored time axis deterministically (no host-tz drift); localization is a render-arg row, never a re-formatted spec.
- version axis: `get_vegalite_versions`/`get_themes`/`get_vega_version`/`get_vega_embed_version`/`get_vega_themes_version` answer the spec-compatibility query; the chart owner pins `vl_version`/`theme` from these so the static render matches the spec's authored dialect, recording the bundled actuals (`get_vega_version()=='6.2.0'`, both v5/v6 Vega-Lite) on the receipt.
- concurrency axis: every native render offloads off the event loop through `anyio` (`../../.api/anyio.md`) — `vlc.vegalite_to_*`/`svg_to_*` ride `to_thread.run_sync(_vl_render, …, limiter=_RENDER_LIMITER)` (GIL-releasing native core, zero-copy of the spec the worker shares) under one shared `CapacityLimiter`, while the `visualization/chart/export#PREPASS` vegafusion pre-pass rides `to_process` across the subprocess seam — so no V8 render blocks the loop. A transient `BrokenWorkerProcess` death recovers through a bounded `stamina` (`../../.api/stamina.md`) retry before the rail surfaces an exhausted failure.
- fault axis: a malformed spec, an unparseable option, or a render failure raises a single `ValueError` (a `vegalite_to_*` compile error carries the Vega-Lite warning text when `show_warnings`); the owner names that raise at the render arm and folds it onto the `expression.Result` (`../../.api/expression.md`) rail as a `RuntimeRail[ArtifactReceipt]`, never a bare `except Exception`.
- evidence: the bytes are keyed through `ContentIdentity.of` and contribute exactly the `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact (engine = the matched `ChartSpec.tag`, dialect = the `ExportFormat` value, scale/theme = the `RenderPolicy` knobs, byte_len = the rendered output length) — emitted under one `structlog` (`../../.api/structlog.md`) event inside an `opentelemetry` (`../../.api/opentelemetry-api.md`) span; this engine mints no identity and contributes no parallel visuals-receipt type. These universal `libs/python/.api` rails stack ON TOP of this folder catalog.
- boundary: vl-convert owns the headless Vega static render and chart-origin SVG rasterization; `altair` (`altair.md`) produces the input Vega-Lite spec; `vegafusion` (`vegafusion.md`) pre-evaluates the spec's data transforms server-side; the rendered PDF/PNG composes into the `composition/compose#COMPOSE` placement plane and the `pymupdf`/`reportlab` document owners; live UI stays outside this package.

[STACK_INTEGRATION]:
- `altair` -> `vegafusion` -> `vlc`, one self-contained spec: `altair.Chart.to_dict()` yields the Vega-Lite spec; the `visualization/chart/export#PREPASS` `VegaTransform.apply` pre-pass server-evaluates its transforms on the `vegafusion` runtime subprocess seam (collapsing large inline datasets to pre-computed tables) and inlines the reduced result INTO one self-contained spec; that spec then renders here via `vlc.vegalite_to_svg`/`vegalite_to_png` with no client-side transform work. vl-convert exposes NO external-dataset feed — there is no `vegalite_to_*` `inline_datasets=` kwarg — so the pre-computed data MUST cross inside the spec (the constraint mirrored on `altair.md` and the transform owner), and the `ChartState` self-contained spec serves the interactive HTML row with no live server.
- universal-rail weave: the render is wrapped by the four shared `libs/python/.api` rails — `anyio` (`../../.api/anyio.md`) `to_thread`/`CapacityLimiter` offload, `msgspec` (`../../.api/msgspec.md`) `ArtifactReceipt.Chart` fact, `structlog` (`../../.api/structlog.md`) + `opentelemetry` (`../../.api/opentelemetry-api.md`) event/span, `expression.Result` (`../../.api/expression.md`) rail folding the engine `ValueError`, and `stamina` (`../../.api/stamina.md`) worker-death retry — never re-implemented per render.
- `vlc` -> document owners: `vegalite_to_pdf`/`vega_to_pdf` emit a vector PDF page the `pymupdf` (`pymupdf.md`) rail merges into a multi-page artifact; `vegalite_to_svg` -> `svg_to_pdf` is the alternate SVG-first path when the owner post-edits the SVG (scale-to-fit, crop via the `graphic/vector#VECTOR` `svgelements` algebra) before rasterizing.
- shared chart-SVG rasterizer rail: `svg_to_png`/`svg_to_jpeg`/`svg_to_pdf` is the ONE place a `typst`-emitted SVG, a `lets-plot` SVG, or any `altair`/Vega SVG converges on the bundled `resvg` core — the visuals rail keeps a single chart-SVG-to-raster owner here, while `resvg-py` (`resvg-py.md`) `svg_to_bytes` is the disjoint raster floor for the net-new vector/glyph/QR/schematic/composed-figure SVGs the `graphic/vector#VECTOR` primitive owns; the two are never admitted in parallel for the same SVG origin.
- deterministic host-font-independent render: `register_font_directory` stacks with the `fonttools` subset + `uharfbuzz` shaping rails — register the exact OTF/TTF faces a producer shaped before render, and the rasterized glyph outlines match the document's shaped text across hosts independent of the machine's system fonts.

[RAIL_LAW]:
- Package: `vl-convert-python`
- Owns: headless Vega-Lite/Vega static rendering to SVG/PNG/JPEG/PDF/HTML/scenegraph, standalone chart-origin SVG-string rasterization over the embedded `resvg` core, Vega-Lite-to-Vega compilation, Vega-editor URL minting, font registration, d3 locale + IANA tz resolution, and bundled version/theme/JS-bundle queries — no browser, no Node, no external process
- Accept: spec-to-bytes rendering for `altair`/`vegafusion` output feeding the `visualization/chart/export#EXPORT` host-free engine and the `composition/compose#COMPOSE` / document owners; SVG-to-raster for chart-origin (`altair`/Vega/`lets-plot`/`typst`) SVG strings; all wrapped by the universal `anyio`/`msgspec`/`structlog`/`opentelemetry`/`expression`/`stamina` rails
- Reject: wrapper-renames of `vegalite_to_*`; a per-format renderer class where the `VL_RENDER` row table discriminates; a Node/browser render path where the embedded V8 core renders; a phantom `javascript_bundle(snippet=None)` default where `snippet` is required; a stale-`.pyi` version string where the live `get_*_version` query is the receipt truth; an `inline_datasets=` feed to a `vegalite_to_*` row that does not exist; a second rasterizer admitted in parallel for chart-origin SVGs (`resvg-py` owns the disjoint marks/figure floor); an inline render on the event loop where the `to_thread`/`CapacityLimiter` seam owns native CPU work; a parallel visuals-receipt type where `ArtifactReceipt.Chart` is the fact; identity minting the runtime owns

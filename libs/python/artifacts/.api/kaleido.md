# [PY_ARTIFACTS_API_KALEIDO]

`kaleido` supplies the headless-Chrome static-image export surface for the artifacts visuals rail: a `Kaleido` pool manager plus sync/async figure-write and figure-calc functions that render Plotly (and Plotly-compatible) figures to PNG/SVG/PDF/JPEG bytes or files without an interactive browser. The package owner composes `Kaleido`, `write_fig`, and `calc_fig` into the Plotly static-render path; it never re-implements the Chromium render engine kaleido already drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `kaleido`
- package: `kaleido`
- import: `kaleido`
- owner: `artifacts`
- rail: visuals
- installed: `1.3.0` reflected via `python -c "import kaleido"` on cp315
- entry points: none (library only)
- capability: headless-Chrome static export of Plotly figures to PNG/SVG/PDF/JPEG, browser-pool management, sync and async render APIs, Chrome provisioning

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pool and page generator
- rail: visuals

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                         |
| :-----: | :-------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `Kaleido`       | pool manager   | a managed pool of headless-Chrome tabs for rendering |
|  [02]   | `PageGenerator` | page template  | the HTML page template injecting plotly.js/MathJax   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool construction and managed render
- rail: visuals

Pool rows carry tab count, timeout, page generator, Plotly.js, MathJax, header, topology, and engine kwargs policy.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                    | [CAPABILITY]                     |
| :-----: | :------------------------------ | :------------------------------ | :------------------------------- |
|  [01]   | `Kaleido`                       | render-pool policy              | build a render pool              |
|  [02]   | `Kaleido.write_fig`             | figure, path, and render policy | render and write via the pool    |
|  [03]   | `Kaleido.calc_fig`              | figure plus render policy       | render to bytes via the pool     |
|  [04]   | `Kaleido.write_fig_from_object` | figure-generator input          | render from a fig-spec generator |

[ENTRYPOINT_SCOPE]: one-shot sync and async functions
- rail: visuals

One-shot rows share figure, path, layout options, topojson, and Kaleido-options policy.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]               | [CAPABILITY]                         |
| :-----: | :------------------------------- | :------------------------- | :----------------------------------- |
|  [01]   | `write_fig`                      | async write policy         | async write to file                  |
|  [02]   | `write_fig_sync`                 | sync write policy          | sync write to file                   |
|  [03]   | `calc_fig`                       | async bytes-render policy  | async render to bytes                |
|  [04]   | `calc_fig_sync`                  | sync bytes-render policy   | sync render to bytes                 |
|  [05]   | `get_chrome` / `get_chrome_sync` | sync or async provisioning | provision the headless Chrome binary |

## [04]-[IMPLEMENTATION_LAW]

[VISUALS_EXPORT]:
- import: `import kaleido` at boundary scope only; module-level import is banned by the manifest import policy.
- pool axis: one `Kaleido` instance owns the browser-tab pool; `n` and `timeout` are constructor rows; reuse the pool across renders, never spawn a browser per figure.
- modality axis: `write_fig`/`calc_fig` (async) and `write_fig_sync`/`calc_fig_sync` (sync) are the one render pair across two execution modes, never parallel exporter types.
- engine axis: kaleido is the static engine `plotly.io.to_image` delegates to (`engine='kaleido'`); the owner drives it through the plotly export call or directly for batch pools.
- provisioning axis: `get_chrome`/`get_chrome_sync` provision the Chromium binary once; this is a setup row, named in the export receipt as the engine version.
- evidence: each export captures figure trace count, output format, scale, pool size, and output byte length as a visuals receipt.
- boundary: kaleido owns Plotly static export; `vl-convert-python` owns Vega/Vega-Lite static export; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `kaleido`
- Owns: headless-Chrome static export of Plotly figures, browser-pool management, sync/async render, Chrome provisioning
- Accept: Plotly figure-to-bytes rendering feeding the visuals and document owners
- Reject: wrapper-renames of `write_fig`/`calc_fig`; a per-figure browser spawn where the pool is reusable; a second static engine where `vl-convert` covers Vega; identity minting the runtime owns

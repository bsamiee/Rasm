# [PY_ARTIFACTS_API_RESVG_PY]

`resvg-py` rasterizes SVG markup or an `.svg`/`.svgz` file to PNG-encoded `bytes` through one native `svg_to_bytes` sink over an embedded Rust `resvg`/`usvg`/`tiny-skia` stack — parse, font resolution, and PNG encoding all in-extension, with no Cairo, headless browser, or external process. It is the host-free raster floor the `graphic/vector/region#REGION` primitive folds as its terminal `Rasterize` arm; it emits raster PNG only, since vector-PDF egress routes to `typst`/`weasyprint`/`reportlab` and chart/Vega-origin SVG rasters through `vl-convert-python`'s bundled `resvg` core.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: rasterizer entrypoint and version constants

`__all__` is exactly the one render function and three string constants — no public class, enum, or submodule. `svg_to_bytes` is a native builtin on the Rust extension carrying `__doc__`, an `inspect.signature`-recoverable signature, a shipped `.pyi` stub, and `py.typed`. Startup observation binds `__resvg_version__` beside `__version__` because behavior tracks the embedded engine.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------ | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `svg_to_bytes`      | function      | native SVG-to-PNG rasterizer returning `bytes` (the `Rasterize` floor) |
|  [02]   | `__version__`       | constant      | installed `resvg-py` package version                                   |
|  [03]   | `__resvg_version__` | constant      | embedded Rust `resvg` engine version                                   |
|  [04]   | `__author__`        | constant      | package author string                                                  |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SVG-to-PNG render

| [INDEX] | [SURFACE]                    | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `svg_to_bytes(...) -> bytes` | function | render SVG markup or `.svg`/`.svgz` file to PNG bytes |

[ENTRYPOINT_SCOPE]: `svg_to_bytes` parameter axes

Every parameter is keyword-defaulted, so `RenderPolicy` projects only the axes a render needs through one `**`-spread. Generic-family overrides carry `font_family`, `serif_family`, `sans_serif_family`, `cursive_family`, `fantasy_family`, `monospace_family`. Policy params are `Literal`: `shape_rendering` = `optimize_speed`/`crisp_edges`/`geometric_precision` (default last), `text_rendering` = `optimize_speed`/`optimize_legibility`/`geometric_precision` (default middle), `image_rendering` = `optimize_quality`/`optimize_speed` (default first).

| [INDEX] | [AXIS]  | [KEYWORDS]                                                  | [CAPABILITY]                                                    |
| :-----: | :------ | :---------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | source  | `svg_string`, `svg_path`                                    | UTF-8 SVG markup or `.svg`/`.svgz` path (exactly one)           |
|  [02]   | sizing  | `width`, `height`, `zoom`, `dpi`                            | pixel size, zoom, or DPI (`dpi=0.0` = SVG-declared size)        |
|  [03]   | parsing | `background`, `style_sheet`, `resources_dir`, `languages`   | background, stylesheet, `xlink:href` root, `<switch>` languages |
|  [04]   | font    | `skip_system_fonts`, `font_files`, `font_dirs`, `font_size` | system/file/directory loading + the generic-family overrides    |
|  [05]   | policy  | `shape_rendering`, `text_rendering`, `image_rendering`      | per-axis `Literal` quality policy                               |
|  [06]   | logging | `log_information`                                           | resvg debug logs to stdout (diagnostic)                         |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `svg_to_bytes` sink owns rasterization: `svg_string`/`svg_path` are input rows (`.svgz` decompresses on the path arm), sizing/parsing/font/policy/logging are keyword rows, each `Literal` policy one `RenderPolicy` field — never an options object, renderer class, or per-input/per-mode variant. `RenderPolicy` grows a knob as one field carried into the one `asdict`-driven `**`-spread that coerces each `()`-default tuple to the catalogued `list[str] | None`.
- `graphic/vector/region#REGION` `Rasterize(document, render)` feeds the placed SVG `bytes` it serialized in-process through the `svg_string` arm (decoded to `str`); the file arm stays for an external `.svg`/`.svgz` source. `svgelements` `Length.value(ppi=, viewbox=)` resolves `width`/`height` against the target bbox so they arrive computed, and `dpi=0.0` defers to the SVG-declared size.
- One `ValueError` covers empty/invalid SVG, a bad option, an unparseable `background`, or a render failure; the `Rasterize` arm names that raise and maps it to `graphic/vector/region#REGION` `RegionFault.render`, never a bare `except Exception` nor a result-free body trusting `async_boundary` to swallow it.

[STACKING]:
- `svgelements` (`.api/svgelements.md`) is the sibling parse/transform/bbox owner the `graphic/vector/region#REGION` primitive folds ahead of the raster: it parses, scale-to-fits, n-ups, crops, and resolves the exact `width`/`height`/`dpi` against the target viewBox; `svg_to_bytes` rasterizes the placed document, and the PNG feeds `pillow` (`.api/pillow.md`) `PIL.Image.open(io.BytesIO(...))` for the `ImageCms` soft-proof + `Image.Quantize` arm or `pyvips` (`.api/pyvips.md`) for a fused downscale.
- SVG producers `segno` (`.api/segno.md`) QR, `ziafont`/`ziamath` (`.api/ziafont.md`, `.api/ziamath.md`) glyph-and-math, `schemdraw` (`.api/schemdraw.md`) schematic, `drawsvg` (`.api/drawsvg.md`) figure, and the `graphic/marks/mark#MARK`/figures-`ANNOTATE` composed SVG all emit into this one raster domain via `svgelements`; `blackrenderer` COLRv1 glyph SVG joins the same producer set.
- `runtime/execution/workers#POOL` `WORKER_BAND` carries the synchronous native render: `graphic/vector/region#REGION` `Region.over(...).of(lane)` crosses the whole op batch (parse + measure + rasterize) as one `KernelTrait.HOSTILE` kernel onto the warm process pool off the event loop, and the typed `Block[RegionResult]` returns across the pickle hop; a rasterize-carrying batch runs `Enforcement.TERMINAL` under the owner's raster deadline, so a malformed-SVG wedge dies at wall-clock instead of outliving a cooperative cancel, as the `pillow`/`exchange/detect#DETECT` arms sharing the band do.
- Deterministic host-font-independent render stacks with `fonttools` (`.api/fonttools.md`) subset + `uharfbuzz` (`.api/uharfbuzz.md`) shaping: subset and freeze the exact faces a producer shaped, pass them as `font_files` with `skip_system_fonts=True`, and rasterized glyph outlines match the shaped text byte-for-byte across hosts.

[LOCAL_ADMISSION]:
- Admitted as the sole SVG-to-PNG raster floor for the net-new vector/glyph/QR/schematic/composed-figure SVG the `graphic/vector/region#REGION` primitive owns, where deterministic font-file injection and per-axis rendering policy are load-bearing; chart/Vega-origin SVG rasters through `vl-convert-python` (`.api/vl-convert-python.md`)'s bundled `resvg` core (`svg_to_png`/`svg_to_pdf`), never a second admission here.

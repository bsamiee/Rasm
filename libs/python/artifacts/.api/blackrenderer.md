# [PY_ARTIFACTS_API_BLACKRENDERER]

`blackrenderer` supplies the reference COLRv1/COLRv0 color-glyph rasterization surface for the artifacts RASTERIZE rail (`typography/shape#SHAPE`): a `BlackRendererFont` that loads an OpenType font through `fontTools` plus `uharfbuzz`, decodes the `COLR`/`CPAL` paint graph, and draws each glyph onto a backend-neutral `Canvas`; a backend registry that selects a concrete `Surface` (skia, cairo, coregraphics, svg) by backend name and output extension; and a `renderText` one-shot that shapes a string, computes pixel bounds, and serializes to PNG/PDF/SVG. The package owner composes `BlackRendererFont.drawGlyph`, `getSurfaceClass`, and the `Surface.canvas`/`saveImage` pair into the typography conformance RASTERIZE path; it removes any hand-rolled COLRv1 paint-format dispatch because the full 32-member `PaintFormat` graph (solid, linear/radial/sweep gradients, transforms, composite, and every `PaintVar*` mirror) is decoded in-package over the `Paint` node object, and it never re-implements HarfBuzz shaping or the skia/cairo color-glyph blend the backends already own. The package is pure-Python: `blackrenderer` itself loads no native library — the only native surfaces are the optional `[skia]`/`[cairo]`/`[cg]` backend extras, and the pure-Python SVG backend needs none, so a COLRv1 SVG render runs with zero binary dependency beyond the always-present `fontTools`+`uharfbuzz`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `blackrenderer`
- package: `blackrenderer`
- import: `blackrenderer`
- owner: `artifacts`
- rail: rasterize (the `typography/shape#SHAPE` COLRv1 `RASTERIZE` arm)
- installed: `0.8.2`
- build-floor: `Requires-Python >=3.10`
- distribution: pure-Python universal wheel (`py3-none-any`, `Root-Is-Purelib: true`); `blackrenderer` loads no compiled extension — gating: none, it imports and runs natively on this cp315 interpreter with `pyproject` carrying it unpinned and marker-free
- license: Apache-2.0 (Apache Software License 2.0; commercial-safe, no Pantone/paid data)
- deps: requires `fontTools>=4.62.1` + `uharfbuzz>=0.53.2` (always present — the COLR/CPAL decode and the HarfBuzz shaping); the rasterizing backends are optional extras (`[skia]` -> `skia-python`+`numpy`, `[cairo]` -> `pycairo`, `[cg]` -> `pyobjc` on darwin only) — the pure-Python `svg` backend needs no extra
- entry points: console script `blackrenderer` (`blackrenderer.__main__:main`); library use is import-only via `blackrenderer.font`, `blackrenderer.render`, and `blackrenderer.backends`
- capability: COLRv1 and COLRv0 color-glyph decoding and rasterization — `COLR`/`CPAL` paint-graph traversal of the full 32-format `PaintFormat` set over the decoded `Paint` node, HarfBuzz text shaping, variable-font location instancing through `VarStoreInstancer`, palette selection, glyph/clip-box bounds, the 28-mode `CompositeMode` blend set, and serialization to PNG/PDF/SVG through pluggable skia, cairo, coregraphics, and pure-Python SVG backends

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font, render, and backend roots
- rail: rasterize

`BlackRendererFont` owns font loading and the COLRv1 paint dispatch (the same object is re-exported from both `blackrenderer.font` and `blackrenderer.render`). `Canvas`/`Surface` are the abstract backend protocols (`Surface.fileExtension` names its output suffix); each concrete `*Surface` is one backend-plus-format pair selected by `getSurfaceClass`. `GlyphInfo` is the shaped-glyph `NamedTuple` `buildGlyphLine` emits. `PaintFormat` is the 32-member COLRv1 paint-graph vocabulary and `CompositeMode` the 28-member Porter-Duff/HSL blend set both decoded in-package. `VarStoreInstancer`/`VarColorLine` resolve COLRv1 `PaintVar*` deltas at a variable-font location. `BackendUnavailableError` is raised (and re-exported from `blackrenderer.render`) when the requested backend module is not importable. Every symbol below lives under `blackrenderer`: `font.*` owns the font/decode roots, `render.*` the render helpers, `backends.base.*` the protocols, and `backends.<backend>.*` the concrete surfaces in the backend sub-table.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]       | [RAIL]                                                    |
| :-----: | :------------------------------- | :------------------ | :-------------------------------------------------------- |
|  [01]   | `font.BlackRendererFont`         | font owner          | font load, COLR/CPAL decode, glyph draw, palette          |
|  [02]   | `font.PaintFormat`               | enum (32 members)   | COLRv1 paint-graph vocabulary (solid/gradient/Var)        |
|  [03]   | `font.CompositeMode`             | enum (28 members)   | COLRv1 blend set (CLEAR/SRC_OVER..HSL_LUMINOSITY)         |
|  [04]   | `font.VarStoreInstancer`         | var resolver        | resolve `PaintVar*` deltas at an axis location            |
|  [05]   | `render.GlyphInfo`               | record (NamedTuple) | `(name, gid, xAdvance, yAdvance, xOffset, yOffset)`       |
|  [06]   | `render.BackendUnavailableError` | error               | requested backend module is not importable                |
|  [07]   | `backends.base.Canvas`           | abstract protocol   | path/transform/clip/solid/gradient/rect draw, composite   |
|  [08]   | `backends.base.Surface`          | abstract protocol   | `canvas(boundingBox)`, `saveImage(path)`, `fileExtension` |

[PUBLIC_TYPE_SCOPE]: concrete backend surfaces (`backends.<backend>.*`)
- rail: rasterize

| [INDEX] | [BACKEND]      | [SURFACE]                  | [FORMAT]             |
| :-----: | :------------- | :------------------------- | :------------------- |
|  [01]   | `skia`         | `SkiaPixelSurface`         | `.png`               |
|  [02]   | `skia`         | `SkiaPDFSurface`           | `.pdf`               |
|  [03]   | `skia`         | `SkiaSVGSurface`           | `.svg`               |
|  [04]   | `cairo`        | `CairoPixelSurface`        | `.png`               |
|  [05]   | `cairo`        | `CairoPDFSurface`          | `.pdf`               |
|  [06]   | `cairo`        | `CairoSVGSurface`          | `.svg`               |
|  [07]   | `coregraphics` | `CoreGraphicsPixelSurface` | `.png` (macOS)       |
|  [08]   | `coregraphics` | `CoreGraphicsPDFSurface`   | `.pdf` (macOS)       |
|  [09]   | `svg`          | `SVGSurface`               | `.svg` (pure-Python) |

[PUBLIC_TYPE_SCOPE]: COLRv1 paint-graph decode objects (fontTools COLR machinery surfaced in `blackrenderer.font`)
- rail: rasterize

These are the `fontTools.ttLib.tables.otTables`/`fontTools.misc.transform` COLR-decode symbols re-exported into the `blackrenderer.font` namespace — the owner reads them when it must walk the paint graph itself (clip-box precompute, transform composition, var-stop resolution) instead of letting `drawGlyph` traverse end-to-end. `Paint` is the per-node decoded paint table; `PAINT_NAMES`/`PAINT_VAR_MAPPING` are the format-id lookup tables `PaintFormat` indexes; `ClipBoxFormat` discriminates a static versus variable COLRv1 clip box. They are decode-side reads, never re-implemented blackrenderer types; every symbol lives under `blackrenderer.font`. `Paint` carries `traverse`/`getChildren`/`getTransform`/`computeClipBox`/`iterPaintSubTables`/`getFormatName`; `Identity == [1 0 0 1 0 0]` is the identity affine the transform arms compose.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]                 | [RAIL]                          |
| :-----: | :------------------------------------------------------------------ | :---------------------------- | :------------------------------ |
|  [01]   | `font.Paint`                                                        | COLR paint node               | decoded COLRv1 paint node       |
|  [02]   | `font.ClipBoxFormat`                                                | enum (`Static`/`Variable`)    | static/variable `ClipBox`       |
|  [03]   | `font.PAINT_NAMES`                                                  | `dict[int, str]` (32)         | format-id -> `PaintFormat` name |
|  [04]   | `font.PAINT_VAR_MAPPING`                                            | `dict[int, PaintFormat]` (14) | `PaintVar*` id -> non-var base  |
|  [05]   | `font.Transform` / `font.Identity`                                  | affine value                  | 6-tuple affine + identity       |
|  [06]   | `font.VarColorLine`/`VarColorStop`/`VarAffine2x3`/`VarTableWrapper` | var-delta wrappers            | COLRv1 var-delta wrappers       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render one-shot and backend selection
- rail: rasterize

`renderText` is the single string-to-file surface: it loads the font, shapes the text with HarfBuzz, computes pixel bounds with `margin`, selects the backend by `backendName` (or infers `svg` for `.svg`, else `skia`), and serializes; it also carries `fontSize=250`/`margin=20`/`features`/`variations`/`paletteIndex=0`/`lang`/`script`. `buildGlyphLine`/`calcGlyphLineBounds` are the composable layer the `typography/shape#SHAPE` owner drives directly (it never calls `renderText`, which hides the palette/location/bounds evidence the receipt carries). `getSurfaceClass(backendName, imageExtension)` indexes the `_surfaces[extension][backend]` registry and returns the concrete `Surface` class, or `None` when the backend module is unimportable (also re-exported from `blackrenderer.render`). `listBackends` enumerates the `(backendName, suffixes)` registry rows; the live matrix is `cairo -> [.pdf, .png, .svg]`, `coregraphics -> [.pdf, .png]`, `skia -> [.pdf, .png, .svg]`, `svg -> [.svg]`. The `unionRect`/`insetRect`/`offsetRect`/`scaleRect`/`intRect` rect algebra is the bounds-arithmetic surface a caller composing `buildGlyphLine` reuses instead of re-deriving the margin-inset/scale math.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                               | [CAPABILITY]                              |
| :-----: | :--------------------------- | :--------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `render.renderText`          | `renderText(fontPath, textString, outputPath, *, ...)`     | shape + rasterize a string to PNG/PDF/SVG |
|  [02]   | `render.buildGlyphLine`      | `buildGlyphLine(infos, positions, glyphNames)`             | map shaping output to glyph records       |
|  [03]   | `render.calcGlyphLineBounds` | `calcGlyphLineBounds(glyphLine, font) -> tuple \| None`    | union font-unit bounds of a glyph line    |
|  [04]   | `backends.getSurfaceClass`   | `getSurfaceClass(backendName, imageExtension=None)`        | resolve surface by backend + extension    |
|  [05]   | `render.unionRect`           | `unionRect`/`insetRect`/`offsetRect`/`scaleRect`/`intRect` | rect bounds arithmetic (margin-inset)     |
|  [06]   | `backends.listBackends`      | `listBackends() -> list[tuple[str, list[str]]]`            | enumerate `(backend, suffixes)` rows      |

[ENTRYPOINT_SCOPE]: `BlackRendererFont` load, decode, and draw
- rail: rasterize

The font constructor admits either a `path` or a paired `ttFont`+`hbFont` (the `typography/shape#SHAPE` owner takes the paired form, sharing one font-byte buffer across the HarfBuzz shaper and the renderer), plus `fontNumber=0`/`lazy=True`; `drawGlyph` defaults `palette=None`/`textColor=(0, 0, 0, 1)`. `drawGlyph` dispatches COLRv1 (paint graph), then COLRv0 (layer list), then a plain outline — internal arms keyed by glyph membership, never caller-selected. `setLocation` drives variable-font instancing; `getPalette` clamps the palette index. Glyph-name and bounds properties feed the shaping and layout path.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                           | [CAPABILITY]                                            |
| :-----: | :----------------- | :----------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `__init__`         | `BlackRendererFont(path=None, *, ttFont, hbFont, ...)` | load `COLR`/`CPAL`/`fvar`; both forms raise `TypeError` |
|  [02]   | `drawGlyph`        | `drawGlyph(glyphName, canvas, *, palette, textColor)`  | draw one glyph onto a `Canvas` (COLRv1/v0/outline)      |
|  [03]   | `getGlyphBounds`   | `getGlyphBounds(glyphName) -> tuple \| None`           | font-unit bounds (COLRv1 clip box when present)         |
|  [04]   | `setLocation`      | `setLocation(location)`                                | apply variable-font normalized axis location            |
|  [05]   | `getPalette`       | `getPalette(paletteIndex) -> list \| None`             | clamped CPAL palette as RGBA float tuples               |
|  [06]   | `unitsPerEm`       | property                                               | font units-per-em from the HarfBuzz face                |
|  [07]   | `glyphNames`       | property                                               | glyph order from the `TTFont`                           |
|  [08]   | `colrV0GlyphNames` | property                                               | COLRv0 base-glyph names                                 |
|  [09]   | `colrV1GlyphNames` | property                                               | COLRv1 base-glyph names                                 |

[ENTRYPOINT_SCOPE]: `Surface`/`Canvas` backend protocol
- rail: rasterize

`Surface.canvas(boundingBox)` is the context manager that yields a `Canvas` flipped into font space; `saveImage(path)` writes the accumulated drawing; `fileExtension` is the surface's output suffix (every backend `saveImage` writes only to a real filesystem path via `open(path)`/`os.fspath`, never a file-like — a caller wanting bytes round-trips a `NamedTemporaryFile` keyed off `fileExtension`). `Canvas` is the draw protocol every backend implements: it carries both path- and rect-keyed solid/gradient draw arms (the COLRv1 paint graph fills a clipped path; the `drawRect*` mirror delegates to the `drawPath*` arm for the rect-fill fast path), the affine helpers `transform`/`scale`/`translate`, and the `compositeMode`/`savedState`/`clipPath` state-management context managers. The skia pixel surface accepts an explicit `format` on save. Every gradient arm takes `path`/`colorLine`/`extendMode`/`gradientTransform` and mirrors as `drawRect*`; the linear arm adds `pt1`/`pt2`, the radial arm `startCenter`/`startRadius`/`endCenter`/`endRadius`, the sweep arm `center`/`startAngle`/`endAngle`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                               | [CAPABILITY]                             |
| :-----: | :------------------------------ | :--------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `Surface.canvas`                | `canvas(boundingBox)` -> ctx yielding `Canvas`             | open a draw context (bbox)               |
|  [02]   | `Surface.saveImage`             | `saveImage(path)`                                          | serialize the drawing to the file format |
|  [03]   | `Surface.fileExtension`         | property -> `str`                                          | output suffix (`.png`/`.pdf`/`.svg`)     |
|  [04]   | `SkiaPixelSurface.saveImage`    | `saveImage(path, format=skia.kPNG)`                        | skia raster with explicit encode format  |
|  [05]   | `Canvas.newPath`                | `newPath()` -> path pen                                    | new backend path/pen for an outline      |
|  [06]   | `Canvas.drawPathSolid`          | `drawPathSolid(path, color)`; `drawRectSolid(rect, color)` | fill path/rect with an RGBA tuple        |
|  [07]   | `Canvas.drawPathLinearGradient` | `drawPathLinearGradient(path, colorLine, pt1, pt2, ...)`   | linear gradient fill (path/rect)         |
|  [08]   | `Canvas.drawPathRadialGradient` | `drawPathRadialGradient(path, colorLine, ...)`             | radial two-circle gradient fill          |
|  [09]   | `Canvas.drawPathSweepGradient`  | `drawPathSweepGradient(path, colorLine, ...)`              | sweep gradient fill (center/angles)      |
|  [10]   | `Canvas.compositeMode`          | `compositeMode(compositeMode)` -> ctx                      | push a `CompositeMode` blend layer       |
|  [11]   | `Canvas.savedState`             | `savedState()` -> ctx                                      | save/restore transform + clip state      |
|  [12]   | `Canvas.transform`              | `transform(t)`; `scale(sx, sy=None)`; `translate(x, y)`    | concat affine / scale / translate        |
|  [13]   | `Canvas.clipPath`               | `clipPath(path)`                                           | intersect the clip region with a path    |

[ENTRYPOINT_SCOPE]: COLRv1 var-instancing + paint-graph decode and the CLI
- rail: rasterize

`VarStoreInstancer` is the variable-font delta resolver `setLocation` drives: construct it over the font's `ItemVariationStore` and `fvar` axes at a location, then it interpolates COLRv1 `PaintVar*` deltas. `Paint.traverse`/`computeClipBox`/`getTransform` are the decode-side reads for a caller that must precompute a COLRv1 clip box or compose a paint transform without a full `drawGlyph`. `axisValuesToLocation(normalizedAxisValues, axisTags)` converts normalized axis values to the location dict the renderer takes; `interpolateFromDeltas(varDataIndex, deltas)` resolves one delta set. The `__main__` CLI helpers (`parseFeatures`/`parseVariations`) parse the `=value`/`axis=value` argument syntax into the `features`/`variations` dicts `renderText` consumes.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                            | [CAPABILITY]                      |
| :-----: | :----------------------------------------- | :------------------------------------------------------ | :-------------------------------- |
|  [01]   | `font.VarStoreInstancer.__init__`          | `VarStoreInstancer(varstore, fvar_axes, location={})`   | bind store + axes at a location   |
|  [02]   | `VarStoreInstancer.setLocation`            | `setLocation(location)`; `interpolateFromDeltas(...)`   | re-point; interpolate a delta set |
|  [03]   | `font.axisValuesToLocation`                | `axisValuesToLocation(values, axisTags) -> dict`        | axis values -> location dict      |
|  [04]   | `font.Paint.traverse`                      | `traverse(colr, cb)`; `getChildren`; `getFormatName`    | walk a COLRv1 paint subgraph      |
|  [05]   | `font.Paint.computeClipBox`                | `computeClipBox(colr, glyphSet, quantization=1)`        | precompute a COLRv1 clip box      |
|  [06]   | `font.Paint.getTransform`                  | `getTransform() -> Transform`                           | compose a paint transform         |
|  [07]   | `render.parseFeatures` / `parseVariations` | `parseFeatures(src)`; `parseVariations(string) -> dict` | parse CLI feature/axis syntax     |

## [04]-[UNIVERSAL_STACK]

[UNIVERSAL_STACK_SCOPE]: the shared `libs/python/.api` rails layered onto the COLRv1 raster surface
- rail: rasterize

`blackrenderer` is the folder-specific COLR decode/raster owner; the dense `typography/shape#SHAPE` rail stacks it ONTO the universal substrate rails so a render is one offloaded, typed, evidenced, contract-checked unit rather than a flat call. The stacking is concrete:

- universal `anyio` tier (`libs/python/.api/anyio.md`): `drawGlyph` and `Surface.saveImage` are GIL-releasing native draw (skia/cairo) or CPU-bound Python (the SVG backend), so the owner offloads every render through `anyio.to_thread.run_sync` under one shared `CapacityLimiter` — the worker holds the `BlackRendererFont` and the `Surface` and shares the font bytes zero-copy, never the event loop. A render farm is N bounded workers each owning one font + one surface, never an unbounded pool; an inline `drawGlyph` on the loop stalls the scheduler on every glyph.
- universal `expression` tier (`libs/python/.api/expression.md`): `BackendUnavailableError` (the `getSurfaceClass`-returned-`None` arm) and the `TypeError` of a both-`path`-and-paired constructor map at the boundary to a `Result[RasterReceipt, RasterFault]` — the unimportable-backend miss is a typed `Error` case carrying the requested backend name, a successful `saveImage` yields the `Ok` receipt; the `try/except` lives only in the boundary adapter, never in the COLRv1 traversal. A batch of glyph renders folds through `Block`-collected `Result`s.
- universal `msgspec`/`pydantic` tier (`libs/python/.api/msgspec.md`): the per-render evidence — font path, glyph count, decoded COLR version (0 vs 1), the resolved `PaintFormat` set the graph used, resolved palette index, backend name, output format, `margin`-inset pixel bounds (from the `calcGlyphLineBounds` + `insetRect`/`scaleRect` walk), and output byte length — is one `msgspec.Struct` rasterize-receipt case on the shared `ArtifactReceipt` family, never a parallel ad-hoc dict; the `GlyphInfo` `NamedTuple` and the bounds tuple are the structured carriers a consumer addresses by field.
- universal `structlog`/`opentelemetry` tier (`libs/python/.api/structlog.md`, `opentelemetry-api.md`): the same receipt fields are the structured event/span payload of the render span; the resolved backend name and `fileExtension` ride the span so a deployment's available-backend matrix (skia vs cairo vs svg fallback) is a queryable deployment fact, read once at boundary init rather than per glyph.
- universal `beartype` tier (`libs/python/.api/beartype.md`): the boundary signatures — `glyphName: str`, `palette: list[tuple[float, float, float, float]] | None`, `location: dict[str, float]`, the `(xMin, yMin, xMax, yMax)` bounds tuple — are beartype-checked at the rail edge so a malformed palette or a non-normalized location fails at the call boundary, not deep inside the skia/cairo native draw where the traceback is opaque.
- universal `numpy` tier (`libs/python/.api/numpy.md`): the `[skia]` backend extra pulls `numpy`; `SkiaPixelSurface` exposes its raster as a numpy-addressable buffer, so a COLRv1 PNG can hand its pixels to the `graphic/raster` / `pillow` / `pyvips` post-process (ICC, composite, downscale) as a canonical `uint8` array without a re-decode round-trip — the same host pixel surface the rest of the imaging rail speaks.

## [05]-[IMPLEMENTATION_LAW]

[RASTERIZE_COLR]:
- import: `from blackrenderer.render import renderText` and `from blackrenderer.font import BlackRendererFont` / `from blackrenderer.backends import getSurfaceClass` at boundary scope only; module-level import is banned by the manifest import policy because the skia/cairo/coregraphics backend modules load native libraries on backend resolution (the `blackrenderer` core itself is pure-Python, but the `_surfaces` registry imports the native backend lazily on `getSurfaceClass`).
- font axis: one `BlackRendererFont` owns load and decode; `path` versus paired `ttFont`+`hbFont` is a constructor row (the `typography/shape#SHAPE` owner takes the paired form so one font-byte buffer feeds both the HarfBuzz shaper and the renderer), never a parallel loader type — passing both forms raises `TypeError`; the COLR version (0 versus 1), `CPAL` palettes, and `fvar` axes are decoded once at construction, never re-parsed per glyph.
- draw axis: `drawGlyph` is the single per-glyph surface; COLRv1 paint-graph traversal, COLRv0 layer iteration, and plain-outline fallback are internal dispatch arms keyed by glyph membership, never caller-selected modes — the full `PaintFormat` set (solid, linear/radial/sweep gradient, transform/translate/rotate/scale/skew and their around-center forms, composite, location, var-delta wrapping) is owned in `font` over the decoded `Paint` node, never re-implemented at the call site.
- backend axis: `getSurfaceClass(backendName, imageExtension)` is the single selector over the `_surfaces` registry; backend (`skia`/`cairo`/`coregraphics`/`svg`) and output format (`.png`/`.pdf`/`.svg`) are registry rows, never a per-combination factory function; `None` return signals an unimportable native backend and routes to `BackendUnavailableError`; `listBackends()` enumerates the live `(backend, suffixes)` matrix for a deployment's available-backend probe.
- paint axis: `PaintFormat` is the decoded COLRv1 paint-graph vocabulary and `CompositeMode` the blend set; the full set (solid, linear/radial/sweep gradient with `extendMode`, transform/translate/rotate/scale/skew and around-center forms, composite, ColrLayers, ColrGlyph, plus every `PaintVar*` mirror via `PAINT_VAR_MAPPING`) is owned in `font` and lowered onto the `Canvas` path/rect draw arms, never re-decoded or re-dispatched at the call site; `Paint.traverse`/`computeClipBox`/`getTransform` are the decode-side reads only where a clip box or paint transform must be precomputed without a full `drawGlyph`.
- shaping axis: `renderText` delegates segmentation, script/language, features, and variations to `uharfbuzz` (the sibling admitted HarfBuzz binding); `buildGlyphLine`/`calcGlyphLineBounds` map shaping output to `GlyphInfo` records and font-unit bounds and are the composable layer the `typography/shape#SHAPE` owner drives directly, never a hand-rolled layout engine — the `renderText` one-shot is the rejected lower-capability form because it hides the palette, location, and glyph-bounds evidence the receipt carries.
- location axis: `setLocation` is the variable-font row; normalized axis values flow through `VarStoreInstancer(varstore, fvar_axes, location)` so COLRv1 `PaintVar*` deltas resolve (and are mirrored onto the shaping font via `hb_font.set_variations` for paint-versus-advance agreement), never a static-instance pre-bake per location; the `fvar`/`avar` decode reuses the sibling `fontTools` tables already loaded, and `axisValuesToLocation` converts normalized values to the location dict.
- stacking: this is the COLR arm of the typography rail — `fontTools` (the admitted subsetter/instancer) prepares the `TTFont`, `uharfbuzz` shapes the run, `blackrenderer` decodes and rasterizes the COLR paint graph, and the chosen backend serializes; the pure-Python SVG backend feeds the `document`/`composition/compose` SVG path with zero native dependency while skia/cairo back the raster/PDF path; non-COLR vector SVG rasterization routes to the sibling `resvg-py`/`vl-convert` owners, never re-implemented here. The shared `libs/python/.api` rails (anyio offload, expression `Result`, msgspec receipt, structlog/otel span, beartype edge, numpy skia buffer) wrap the render per [04].
- evidence: each render captures font path, glyph count, COLR version, decoded `PaintFormat` set, resolved palette index, backend name, output format, pixel bounds (`margin`-inset via `insetRect`/`scaleRect`), and output byte length as a rasterize receipt — one `ArtifactReceipt` case, never a parallel media-style shape.
- boundary: `blackrenderer` owns COLRv1/COLRv0 decode and color-glyph rasterization; the SVG backend serializes with no native dependency and feeds the `document` and `composition/compose` owners directly; PNG/PDF raster output routes through the skia or cairo backend (their optional extras); HarfBuzz shaping and font-table decode stay inside `uharfbuzz`/`fontTools`; font subsetting/instancing stays at `typography/font#FONT`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `blackrenderer`
- Owns: COLRv1/COLRv0 color-glyph decoding and rasterization — paint-graph traversal over the decoded `Paint` node, HarfBuzz-shaped text rendering, variable-font location instancing through `VarStoreInstancer`, palette selection, and PNG/PDF/SVG serialization through pluggable backends
- Accept: COLR color-glyph rasterization and typography conformance feeding the rasterize, document, and visuals owners; the composable `buildGlyphLine`/`calcGlyphLineBounds`/`drawGlyph`/`Surface.canvas` layer wrapped by the shared anyio/expression/msgspec/structlog rails
- Reject: wrapper-renames of `renderText`/`drawGlyph`/`getSurfaceClass`; a hand-rolled COLRv1 `PaintFormat` dispatch where the in-package `Paint`-node traversal owns it; a hand-rolled HarfBuzz shaper or `COLR`/`CPAL` decoder; a parallel surface factory per backend-plus-format pair where the `_surfaces` registry already selects; a forced native (skia/cairo) path where the pure-Python SVG backend needs no dependency; the `renderText` one-shot where the page needs the palette/location/bounds evidence the composable layer exposes

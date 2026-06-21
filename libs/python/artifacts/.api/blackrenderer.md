# [PY_ARTIFACTS_API_BLACKRENDERER]

`blackrenderer` supplies the reference COLRv1/COLRv0 color-glyph rasterization surface for the artifacts RASTERIZE rail: a `BlackRendererFont` that loads an OpenType font through `fontTools` plus `uharfbuzz`, decodes the `COLR`/`CPAL` paint graph, and draws each glyph onto a backend-neutral `Canvas`; a backend registry that selects a concrete `Surface` (skia, cairo, coregraphics, svg) by backend name and output extension; and a `renderText` one-shot that shapes a string, computes pixel bounds, and serializes to PNG/PDF/SVG. The package owner composes `BlackRendererFont.drawGlyph`, `getSurfaceClass`, and the `Surface.canvas`/`saveImage` pair into the typography conformance RASTERIZE path; it removes any hand-rolled COLRv1 paint-format dispatch because the full `PaintFormat` graph (solid, linear/radial/sweep gradients, transforms, composite, var deltas) is decoded in-package, and it never re-implements HarfBuzz shaping or the skia/cairo color-glyph blend the backends already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `blackrenderer`
- package: `blackrenderer`
- import: `blackrenderer`
- owner: `artifacts`
- rail: rasterize
- installed: `0.8.2` reflected via `assay api` on cp315
- entry points: console script `blackrenderer` (`blackrenderer.__main__:main` CLI); library use is import-only via `blackrenderer.font`, `blackrenderer.render`, and `blackrenderer.backends`
- capability: COLRv1 and COLRv0 color-glyph decoding and rasterization — `COLR`/`CPAL` paint-graph traversal, HarfBuzz text shaping, variable-font location instancing, palette selection, glyph bounds, and serialization to PNG/PDF/SVG through pluggable skia, cairo, coregraphics, and pure-Python SVG backends

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font, render, and backend roots
- rail: rasterize

`BlackRendererFont` owns font loading and the COLRv1 paint dispatch; `Canvas`/`Surface` are the abstract backend protocol; each concrete `*Surface` is one backend-plus-format pair selected by `getSurfaceClass`. `GlyphInfo` is the shaped-glyph record `buildGlyphLine` emits. `BackendUnavailableError` is raised when the requested backend module is not importable.

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]       | [RAIL]                                                      |
| :-----: | :------------------------------------------------------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `blackrenderer.font.BlackRendererFont`                         | font owner          | font load, COLR/CPAL decode, glyph draw, palette, location  |
|  [02]   | `blackrenderer.render.GlyphInfo`                               | record (NamedTuple) | one shaped glyph: name, gid, advances, offsets              |
|  [03]   | `blackrenderer.render.BackendUnavailableError`                 | error               | requested backend module is not importable                  |
|  [04]   | `blackrenderer.backends.base.Canvas`                           | abstract protocol   | path build, transform, clip, solid/gradient draw, composite |
|  [05]   | `blackrenderer.backends.base.Surface`                          | abstract protocol   | `canvas(boundingBox)` context plus `saveImage(path)`        |
|  [06]   | `blackrenderer.backends.skia.SkiaPixelSurface`                 | surface             | skia PNG raster surface (`.png`)                            |
|  [07]   | `blackrenderer.backends.skia.SkiaPDFSurface`                   | surface             | skia PDF surface (`.pdf`)                                   |
|  [08]   | `blackrenderer.backends.skia.SkiaSVGSurface`                   | surface             | skia SVG surface (`.svg`)                                   |
|  [09]   | `blackrenderer.backends.cairo.CairoPixelSurface`               | surface             | cairo PNG raster surface (`.png`)                           |
|  [10]   | `blackrenderer.backends.cairo.CairoPDFSurface`                 | surface             | cairo PDF surface (`.pdf`)                                  |
|  [11]   | `blackrenderer.backends.cairo.CairoSVGSurface`                 | surface             | cairo SVG surface (`.svg`)                                  |
|  [12]   | `blackrenderer.backends.coregraphics.CoreGraphicsPixelSurface` | surface             | CoreGraphics PNG raster surface (`.png`, macOS)             |
|  [13]   | `blackrenderer.backends.coregraphics.CoreGraphicsPDFSurface`   | surface             | CoreGraphics PDF surface (`.pdf`, macOS)                    |
|  [14]   | `blackrenderer.backends.svg.SVGSurface`                        | surface             | pure-Python SVG surface, no native dependency (`.svg`)      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render one-shot and backend selection
- rail: rasterize

`renderText` is the single string-to-file surface: it loads the font, shapes the text with HarfBuzz, computes pixel bounds with `margin`, selects the backend by `backendName` (or infers `svg` for `.svg`, else `skia`), and serializes. `getSurfaceClass` returns the concrete `Surface` class or `None` when the backend module is unimportable; `listBackends` enumerates the `(backendName, suffixes)` registry rows.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                                                                                                         | [CAPABILITY]                                         |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `render.renderText`          | `renderText(fontPath, textString, outputPath, *, fontSize=250, margin=20, features=None, variations=None, paletteIndex=0, backendName=None, lang=None, script=None)` | shape + rasterize a string to PNG/PDF/SVG            |
|  [02]   | `render.buildGlyphLine`      | `buildGlyphLine(infos, positions, glyphNames) -> list[GlyphInfo]`                                                                                                    | map HarfBuzz infos/positions to shaped glyph records |
|  [03]   | `render.calcGlyphLineBounds` | `calcGlyphLineBounds(glyphLine, font) -> tuple                                                                                                                       | None`                                                | union font-unit bounds across a shaped glyph line |
|  [04]   | `backends.getSurfaceClass`   | `getSurfaceClass(backendName, imageExtension=None) -> type[Surface]                                                                                                  | None`                                                | resolve concrete surface by backend + extension   |
|  [05]   | `backends.listBackends`      | `listBackends() -> list[tuple[str, list[str]]]`                                                                                                                      | enumerate registered `(backend, suffixes)` rows      |

[ENTRYPOINT_SCOPE]: `BlackRendererFont` load, decode, and draw
- rail: rasterize

The font constructor admits either a `path` or a paired `ttFont`+`hbFont`; passing both forms raises `TypeError`. `drawGlyph` dispatches COLRv1 (paint graph), then COLRv0 (layer list), then a plain outline. `setLocation` drives variable-font instancing; `getPalette` clamps the palette index. Glyph-name and bounds properties feed the shaping and layout path.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                                                         | [CAPABILITY]                                         |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `BlackRendererFont.__init__`         | `BlackRendererFont(path=None, *, fontNumber=0, lazy=True, ttFont=None, hbFont=None)` | load `COLR`/`CPAL`/`fvar`; decode color glyph tables |
|  [02]   | `BlackRendererFont.drawGlyph`        | `drawGlyph(glyphName, canvas, *, palette=None, textColor=(0, 0, 0, 1))`              | draw one glyph onto a `Canvas` (COLRv1/v0/outline)   |
|  [03]   | `BlackRendererFont.getGlyphBounds`   | `getGlyphBounds(glyphName) -> tuple                                                  | None`                                                | font-unit bounds (clip box for COLRv1)    |
|  [04]   | `BlackRendererFont.setLocation`      | `setLocation(location)`                                                              | apply variable-font normalized axis location         |
|  [05]   | `BlackRendererFont.getPalette`       | `getPalette(paletteIndex) -> list                                                    | None`                                                | clamped CPAL palette as RGBA float tuples |
|  [06]   | `BlackRendererFont.unitsPerEm`       | property                                                                             | font units-per-em from the HarfBuzz face             |
|  [07]   | `BlackRendererFont.glyphNames`       | property                                                                             | glyph order from the `TTFont`                        |
|  [08]   | `BlackRendererFont.colrV0GlyphNames` | property                                                                             | COLRv0 base-glyph names                              |
|  [09]   | `BlackRendererFont.colrV1GlyphNames` | property                                                                             | COLRv1 base-glyph names                              |

[ENTRYPOINT_SCOPE]: `Surface`/`Canvas` backend protocol
- rail: rasterize

`Surface.canvas(boundingBox)` is the context manager that yields a `Canvas` flipped into font space; `saveImage(path)` writes the accumulated drawing. `Canvas` is the draw protocol every backend implements; the skia pixel surface accepts an explicit `format` on save.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                                             | [CAPABILITY]                                        |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Surface.canvas`                | `canvas(boundingBox)` -> context manager yielding `Canvas`                                                               | open a draw context for `(xMin, yMin, xMax, yMax)`  |
|  [02]   | `Surface.saveImage`             | `saveImage(path)`                                                                                                        | serialize the drawing to the backend file format    |
|  [03]   | `SkiaPixelSurface.saveImage`    | `saveImage(path, format=skia.kPNG)`                                                                                      | write skia raster image with explicit encode format |
|  [04]   | `Canvas.newPath`                | `newPath()` -> path pen                                                                                                  | new backend path/pen for outline construction       |
|  [05]   | `Canvas.drawPathSolid`          | `drawPathSolid(path, color)`                                                                                             | fill a path with an RGBA color tuple                |
|  [06]   | `Canvas.drawPathLinearGradient` | `drawPathLinearGradient(path, colorLine, pt1, pt2, extendMode, gradientTransform)`                                       | linear COLRv1 gradient fill                         |
|  [07]   | `Canvas.drawPathRadialGradient` | `drawPathRadialGradient(path, colorLine, startCenter, startRadius, endCenter, endRadius, extendMode, gradientTransform)` | radial gradient fill                                |
|  [08]   | `Canvas.drawPathSweepGradient`  | `drawPathSweepGradient(path, colorLine, center, startAngle, endAngle, extendMode, gradientTransform)`                    | sweep gradient fill                                 |
|  [09]   | `Canvas.compositeMode`          | `compositeMode(compositeMode)` -> context manager                                                                        | push a COLRv1 `CompositeMode` blend layer           |
|  [10]   | `Canvas.savedState`             | `savedState()` -> context manager                                                                                        | save/restore transform + clip state                 |
|  [11]   | `Canvas.transform`              | `transform(transform)`                                                                                                   | concat a 6-tuple affine onto the canvas             |
|  [12]   | `Canvas.clipPath`               | `clipPath(path)`                                                                                                         | intersect the clip region with a path               |

## [04]-[IMPLEMENTATION_LAW]

[RASTERIZE_COLR]:
- import: `from blackrenderer.render import renderText` and `from blackrenderer.font import BlackRendererFont` at boundary scope only; module-level import is banned by the manifest import policy because skia/cairo/coregraphics load native libraries on backend resolution.
- font axis: one `BlackRendererFont` owns load and decode; `path` versus paired `ttFont`+`hbFont` is a constructor row, never a parallel loader type; the COLR version (0 versus 1), `CPAL` palettes, and `fvar` axes are decoded once at construction, never re-parsed per glyph.
- draw axis: `drawGlyph` is the single per-glyph surface; COLRv1 paint-graph traversal, COLRv0 layer iteration, and plain-outline fallback are internal dispatch arms keyed by glyph membership, never caller-selected modes — the full `PaintFormat` set (solid, linear/radial/sweep gradient, transform/translate/rotate/scale/skew and their around-center forms, composite, location, var-delta wrapping) is owned in `font`, never re-implemented at the call site.
- backend axis: `getSurfaceClass(backendName, imageExtension)` is the single selector over the `_surfaces` registry; backend (`skia`/`cairo`/`coregraphics`/`svg`) and output format (`.png`/`.pdf`/`.svg`) are registry rows, never a per-combination factory function; `None` return signals an unimportable native backend and routes to `BackendUnavailableError`.
- shaping axis: `renderText` delegates segmentation, script/language, features, and variations to `uharfbuzz`; `buildGlyphLine`/`calcGlyphLineBounds` map shaping output to `GlyphInfo` records and font-unit bounds, never a hand-rolled layout engine.
- location axis: `setLocation` is the variable-font row; normalized axis values flow through the `VarStoreInstancer` so COLRv1 `PaintVar*` deltas resolve, never a static-instance pre-bake per location.
- evidence: each render captures font path, glyph count, COLR version, resolved palette index, backend name, output format, pixel bounds (`margin`-inset), and output byte length as a rasterize receipt.
- boundary: `blackrenderer` owns COLRv1/COLRv0 decode and color-glyph rasterization; the SVG backend serializes with no native dependency and feeds the document and visuals owners directly; PNG/PDF raster output routes through the skia or cairo backend; HarfBuzz shaping and font-table decode stay inside `uharfbuzz`/`fontTools`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `blackrenderer`
- Owns: COLRv1/COLRv0 color-glyph decoding and rasterization — paint-graph traversal, HarfBuzz-shaped text rendering, variable-font location instancing, palette selection, and PNG/PDF/SVG serialization through pluggable backends
- Accept: COLR color-glyph rasterization and typography conformance feeding the rasterize, document, and visuals owners
- Reject: wrapper-renames of `renderText`/`drawGlyph`/`getSurfaceClass`; a hand-rolled COLRv1 `PaintFormat` dispatch; a hand-rolled HarfBuzz shaper or `COLR`/`CPAL` decoder; a parallel surface factory per backend-plus-format pair where the `_surfaces` registry already selects; a forced native (skia/cairo) path where the pure-Python SVG backend needs no dependency

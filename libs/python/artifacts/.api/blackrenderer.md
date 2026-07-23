# [PY_ARTIFACTS_API_BLACKRENDERER]

`blackrenderer` owns COLRv1/COLRv0 color-glyph decode and rasterization for the artifacts RASTERIZE arm: it loads an OpenType font through `fontTools` and `uharfbuzz`, walks the `COLR`/`CPAL` paint graph over a decoded `Paint` node, and draws each glyph onto a backend-neutral `Canvas` a registry-selected `Surface` serializes. `PaintFormat` and `CompositeMode` decode in-package, so a hand-rolled paint-format dispatch is the rejected form; HarfBuzz shaping and font-table decode stay in `uharfbuzz`/`fontTools`, and non-COLR vector rasterization routes to `resvg-py`/`vl-convert`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `blackrenderer`
- package: `blackrenderer` (Apache-2.0)
- module: `blackrenderer`
- namespaces: `blackrenderer.font`, `blackrenderer.render`, `blackrenderer.backends`
- owner: `artifacts`
- rail: rasterize — the `typography/shape` COLRv1 `RASTERIZE` arm
- depends: `fontTools` and `uharfbuzz` always present (COLR/CPAL decode, HarfBuzz shaping); `skia`/`cairo`/`coregraphics` backends are optional extras, `svg` needs none
- entry points: console script `blackrenderer`; library use imports `blackrenderer.font`, `blackrenderer.render`, `blackrenderer.backends`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font, render, and backend roots
`BlackRendererFont` owns font load and COLRv1 paint dispatch, re-exported from `blackrenderer.font` and `blackrenderer.render`. `Canvas`/`Surface` are the abstract backend protocols; each concrete `<Backend><Format>Surface` is a `getSurfaceClass`-selected class, never imported directly. `PaintFormat`, `CompositeMode`, and the `Paint`-node decode symbols (`PAINT_NAMES`, `PAINT_VAR_MAPPING`, `Transform`/`Identity`, the `VarColor*` wrappers) are `fontTools` COLR machinery re-exported into `blackrenderer.font`; `Identity` is the identity affine `[1 0 0 1 0 0]`.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]       | [CAPABILITY]                                                     |
| :-----: | :--------------------------------- | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `font.BlackRendererFont`           | font owner          | font load, COLR/CPAL decode, glyph draw, palette                 |
|  [02]   | `font.PaintFormat`                 | enum                | COLRv1 paint-graph vocabulary (solid/gradient/Var)               |
|  [03]   | `font.CompositeMode`               | enum                | Porter-Duff/HSL blend set (CLEAR/SRC_OVER..HSL_LUM)              |
|  [04]   | `font.VarStoreInstancer`           | var resolver        | resolve `PaintVar*` deltas at an axis location                   |
|  [05]   | `font.Paint`                       | COLR paint node     | decoded node; traverse/getChildren/getTransform/clip             |
|  [06]   | `font.ClipBoxFormat`               | enum                | static vs variable `ClipBox`                                     |
|  [07]   | `font.PAINT_NAMES`                 | `dict[int, str]`    | format-id -> `PaintFormat` name                                  |
|  [08]   | `font.PAINT_VAR_MAPPING`           | `dict[int, fmt]`    | `PaintVar*` id -> non-var base                                   |
|  [09]   | `font.Transform` / `font.Identity` | affine value        | 6-tuple affine + identity                                        |
|  [10]   | `font.VarColorLine`                | var-delta wrappers  | `VarColorStop`/`VarAffine2x3`/`VarTableWrapper` COLRv1 var-delta |
|  [11]   | `render.GlyphInfo`                 | record (NamedTuple) | `(name, gid, xAdvance, yAdvance, xOffset, yOffset)`              |
|  [12]   | `render.BackendUnavailableError`   | error               | requested backend module is not importable                       |
|  [13]   | `backends.base.Canvas`             | abstract protocol   | path/rect/clip solid/gradient draw, composite                    |
|  [14]   | `backends.base.Surface`            | abstract protocol   | `canvas`/`saveImage`/`fileExtension`                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render one-shot, backend selection, and rect algebra
`renderText` is the string-to-file one-shot; `buildGlyphLine`/`calcGlyphLineBounds` are the composable layer the shape owner drives directly. `getSurfaceClass` indexes the `_surfaces` registry and returns the `Surface` class or `None` when the backend is unimportable; `listBackends` enumerates the live `(backend, suffixes)` matrix for a deployment probe. All rows are module-level functions.
- `renderText` carry: `fontSize`, `margin`, `features`, `variations`, `paletteIndex`, `lang`, `script`

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `renderText(fontPath, textString, outputPath, *, backendName)` | shape + rasterize a string to PNG/PDF/SVG   |
|  [02]   | `buildGlyphLine(infos, positions, glyphNames)`                 | map shaping output to `GlyphInfo` records   |
|  [03]   | `calcGlyphLineBounds(glyphLine, font)`                         | union font-unit bounds of a glyph line      |
|  [04]   | `getSurfaceClass(backendName, imageExtension=None)`            | resolve `Surface`; `None` if unimportable   |
|  [05]   | `unionRect(rect, rect)`                                        | rect algebra (union/inset/offset/scale/int) |
|  [06]   | `listBackends()`                                               | enumerate `(backend, suffixes)` rows        |

[ENTRYPOINT_SCOPE]: `BlackRendererFont` load, decode, and draw
`BlackRendererFont` admits a `path` or a paired `ttFont`+`hbFont` at construction (the shape owner takes the paired form to share one font-byte buffer across shaper and renderer); passing both raises `TypeError`. `drawGlyph` dispatches COLRv1 paint graph, then COLRv0 layer list, then plain outline by glyph membership, never caller-selected.
- `BlackRendererFont` carry: `fontNumber=0`, `lazy=True`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `BlackRendererFont(path=None, *, ttFont=None, hbFont=None)` | ctor     | load `COLR`/`CPAL`/`fvar`; `path` xor paired form  |
|  [02]   | `drawGlyph(glyphName, canvas, *, palette=None, textColor)`  | instance | draw one glyph onto a `Canvas` (COLRv1/v0/outline) |
|  [03]   | `getGlyphBounds(glyphName)`                                 | instance | font-unit bounds (COLRv1 clip box when present)    |
|  [04]   | `setLocation(location)`                                     | instance | apply variable-font normalized axis location       |
|  [05]   | `getPalette(paletteIndex)`                                  | instance | clamped CPAL palette as RGBA float tuples          |
|  [06]   | `unitsPerEm`                                                | property | units-per-em from the HarfBuzz face                |
|  [07]   | `glyphNames`                                                | property | glyph order from the `TTFont`                      |
|  [08]   | `colrV0GlyphNames` / `colrV1GlyphNames`                     | property | COLRv0 / COLRv1 base-glyph names                   |

[ENTRYPOINT_SCOPE]: `Surface`/`Canvas` backend protocol
`Surface.canvas(boundingBox)` yields a `Canvas` flipped into font space; `saveImage(path)` writes to a real filesystem path only (`open(path)`/`os.fspath`, never a file-like — a bytes consumer round-trips a `NamedTemporaryFile` keyed off `fileExtension`). Every `drawPath*` gradient arm takes `path`/`colorLine`/`extendMode`/`gradientTransform` and mirrors as `drawRect*`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `Surface.canvas(boundingBox)`                                    | instance | draw context yielding a `Canvas` (bbox)  |
|  [02]   | `Surface.saveImage(path)`                                        | instance | serialize the drawing to the file format |
|  [03]   | `Surface.fileExtension`                                          | property | output suffix (`.png`/`.pdf`/`.svg`)     |
|  [04]   | `SkiaPixelSurface.saveImage(path, format=skia.kPNG)`             | instance | skia raster with explicit encode format  |
|  [05]   | `Canvas.newPath()`                                               | instance | new backend path/pen for an outline      |
|  [06]   | `Canvas.drawPathSolid(path, color)` / `drawRectSolid`            | instance | fill path/rect with an RGBA tuple        |
|  [07]   | `Canvas.drawPathLinearGradient(path, colorLine, pt1, pt2)`       | instance | linear gradient fill (path/rect mirror)  |
|  [08]   | `Canvas.drawPathRadialGradient(path, colorLine, ...)`            | instance | radial two-circle gradient fill          |
|  [09]   | `Canvas.drawPathSweepGradient(path, colorLine, ...)`             | instance | sweep gradient fill (center/angles)      |
|  [10]   | `Canvas.compositeMode(compositeMode)`                            | instance | push a `CompositeMode` blend layer       |
|  [11]   | `Canvas.savedState()`                                            | instance | save/restore transform + clip state      |
|  [12]   | `Canvas.transform(t)` / `scale(sx, sy=None)` / `translate(x, y)` | instance | concat affine / scale / translate        |
|  [13]   | `Canvas.clipPath(path)`                                          | instance | intersect the clip region with a path    |

[ENTRYPOINT_SCOPE]: COLRv1 var-instancing, paint-graph decode, and the CLI
`VarStoreInstancer` is the delta resolver `setLocation` drives: construct it over the font's `ItemVariationStore` and `fvar` axes at a location, then it interpolates COLRv1 `PaintVar*` deltas. `Paint.traverse`/`computeClipBox`/`getTransform` are the decode-side reads for precomputing a clip box or paint transform without a full `drawGlyph`. `blackrenderer`'s CLI parses `=value`/`axis=value` syntax through `parseFeatures`/`parseVariations` in `__main__` into the `features`/`variations` dicts `renderText` consumes.

| [INDEX] | [SURFACE]                                                                                 | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `VarStoreInstancer(varstore, fvar_axes, location={})`                                     | ctor     | bind store + axes at a location   |
|  [02]   | `VarStoreInstancer.setLocation(location)` / `interpolateFromDeltas(varDataIndex, deltas)` | instance | re-point; interpolate a delta set |
|  [03]   | `axisValuesToLocation(normalizedAxisValues, axisTags)`                                    | static   | axis values -> location dict      |
|  [04]   | `Paint.traverse(colr, callback)` / `getChildren()` / `getFormatName()`                    | instance | walk a COLRv1 paint subgraph      |
|  [05]   | `Paint.computeClipBox(colr, glyphSet, quantization=1)`                                    | instance | precompute a COLRv1 clip box      |
|  [06]   | `Paint.getTransform()`                                                                    | instance | compose a paint transform         |
|  [07]   | `__main__.parseFeatures(src)` / `parseVariations(string)`                                 | static   | parse CLI feature/axis syntax     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `BlackRendererFont` owns load and decode; the COLR version, `CPAL` palettes, and `fvar` axes decode once at construction, never per glyph, and `path` xor paired `ttFont`+`hbFont` is a constructor row that raises `TypeError` on both.
- `drawGlyph` is the single per-glyph surface; COLRv1 paint-graph traversal, COLRv0 layer iteration, and plain-outline fallback are internal arms keyed by glyph membership. `PaintFormat`'s full set (solid, linear/radial/sweep gradient with `extendMode`, transform/rotate/scale/skew and around-center forms, composite, and `PaintVar*` mirrors via `PAINT_VAR_MAPPING`) lowers onto the `Canvas` path/rect arms; `Paint.traverse`/`computeClipBox`/`getTransform` are the decode-side reads for a precomputed clip box or transform.
- `getSurfaceClass(backendName, imageExtension)` selects over the `_surfaces` registry; backend and output format are registry rows, `None` routes to `BackendUnavailableError`, and `listBackends()` enumerates the live matrix.
- `setLocation` drives variable-font instancing through `VarStoreInstancer` so COLRv1 `PaintVar*` deltas resolve, mirrored onto the shaping font via `hb_font.set_variations` for paint-versus-advance agreement.
- `buildGlyphLine`/`calcGlyphLineBounds` map shaping output to `GlyphInfo` records and font-unit bounds — the composable layer the owner drives; `renderText` is the rejected one-shot where the page needs palette, location, and glyph-bounds evidence.

[STACKING]:
- `anyio`(`.api/anyio.md`): `drawGlyph` and `Surface.saveImage` offload through `anyio.to_thread.run_sync` under one `CapacityLimiter`; a render farm is N bounded workers each owning one font + one surface, never an inline `drawGlyph` on the event loop.
- `expression`(`.api/expression.md`): `BackendUnavailableError` (the `getSurfaceClass`-`None` arm) and the both-form `TypeError` map at the boundary to `Result[RasterReceipt, RasterFault]`; the `try/except` lives only in the boundary adapter, and a glyph batch folds through `Block`-collected `Result`s.
- `msgspec`(`.api/msgspec.md`): per-render evidence — font path, glyph count, COLR version, resolved `PaintFormat` set, palette index, backend name, output format, `margin`-inset bounds, byte length — is one rasterize-receipt case on `ArtifactReceipt`; `GlyphInfo` and the bounds tuple are its structured carriers.
- `structlog`/`opentelemetry`(`.api/structlog.md`): the receipt fields ride the render span, and the resolved backend + `fileExtension` make a deployment's available-backend matrix a queryable fact read once at boundary init.
- `beartype`(`.api/beartype.md`): the boundary signatures — `glyphName: str`, `palette` RGBA-tuple list, `location` dict, the `(xMin, yMin, xMax, yMax)` bounds tuple — check at the rail edge so a malformed palette or non-normalized location fails before the native draw.
- `numpy`(`.api/numpy.md`): the `[skia]` backend exposes `SkiaPixelSurface` pixels as a `uint8` array, handing a COLRv1 PNG to the `graphic/raster`/`pillow`/`pyvips` post-process without a re-decode round-trip.
- within-lib: `fontTools` prepares the `TTFont`, `uharfbuzz` shapes the run, `blackrenderer` decodes and rasterizes the COLR paint graph, and the selected backend serializes; the `svg` backend feeds `document`/`composition/compose` with zero native dependency while `skia`/`cairo` back the raster/PDF path.

[LOCAL_ADMISSION]:
- Import `blackrenderer.font`/`.render`/`.backends` at boundary scope only; `getSurfaceClass` imports the native backend lazily on resolution, so a module-level import loads native libraries against the manifest import policy.

[RAIL_LAW]:
- Package: `blackrenderer`
- Owns: COLRv1/COLRv0 color-glyph decode and rasterization — paint-graph traversal over the decoded `Paint` node, HarfBuzz-shaped text rendering, variable-font instancing through `VarStoreInstancer`, palette selection, and PNG/PDF/SVG serialization through pluggable backends
- Accept: COLR color-glyph rasterization and typography conformance feeding the rasterize, document, and visuals owners via the composable `buildGlyphLine`/`calcGlyphLineBounds`/`drawGlyph`/`Surface.canvas` layer wrapped by the shared rails
- Reject: a hand-rolled COLRv1 `PaintFormat` dispatch where the in-package `Paint`-node traversal owns it; a hand-rolled HarfBuzz shaper or `COLR`/`CPAL` decoder; a per-backend-plus-format surface factory where the `_surfaces` registry selects; a forced native path where the `svg` backend needs no dependency; the `renderText` one-shot where the page needs the palette/location/bounds evidence

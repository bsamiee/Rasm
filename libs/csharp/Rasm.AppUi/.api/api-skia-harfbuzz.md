# [RASM_APPUI_API_SKIA_HARFBUZZ]

`SkiaSharp.HarfBuzz` supplies HarfBuzz-backed text shaping, shaped-text drawing, font-stream-to-blob conversion, and HarfBuzz font-scale interop for Skia visuals. It is a thin bridge assembly: it shapes through `HarfBuzzSharp` and renders through `SkiaSharp.SKTextBlob`, owning no glyph data of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.HarfBuzz`
- package: `SkiaSharp.HarfBuzz`
- version: `3.119.4`
- license: `MIT`
- assembly: `SkiaSharp.HarfBuzz`
- namespace: `SkiaSharp.HarfBuzz`
- types: 5 public types in 1 namespace (`SKShaper`, `SKShaper.Result`, `CanvasExtensions`, `FontExtensions`, `BlobExtensions`)
- dependency: `SkiaSharp` `3.119.4` (managed `SKCanvas`/`SKFont`/`SKTextBlobBuilder` surface)
- dependency: `HarfBuzzSharp` `8.3.1.5` (managed `Blob`/`Buffer`/`Font` shaping surface)
- native: `HarfBuzzSharp.NativeAssets.macOS` / `.Linux` carry `libHarfBuzzSharp`; `SkiaSharp.NativeAssets.*` carry `libSkiaSharp`. Shaping faults at runtime if the native HarfBuzz asset for the active RID is absent — this assembly is managed-only.
- asset: runtime library
- rail: typography

## [02]-[PUBLIC_TYPES]

[SHAPING_TYPES]: shaping owner and its result record — rail: typography

| [INDEX] | [SYMBOL]                 | [KIND]                  | [SHAPE]                                                                |
| :-----: | :----------------------- | :---------------------- | :-------------------------------------------------------------------- |
|  [01]   | `SKShaper : IDisposable` | per-typeface shaper     | wraps one `HarfBuzzSharp.Font` + reusable `Buffer`; dispose-bound      |
|  [02]   | `SKShaper.Result`        | shaped-run value record | parallel `Codepoints`/`Clusters`/`Points` arrays + total `Width`      |
|  [03]   | `CanvasExtensions`       | shaped-draw extensions  | `SKCanvas.DrawShapedText` family (shape + build `SKTextBlob` + draw)   |
|  [04]   | `FontExtensions`         | HarfBuzz scale interop  | `HarfBuzzSharp.Font` scale get/set marshalled through `SKSizeI`        |
|  [05]   | `BlobExtensions`         | stream-to-blob bridge   | `SKStreamAsset -> HarfBuzzSharp.Blob` with lifetime-bound release      |

[SUBSTRATE_TYPES]: admitted shaping/render substrate consumed by the bridge (other assemblies) — rail: typography

| [INDEX] | [SYMBOL]                  | [ASSEMBLY]      | [ROLE]                                                            |
| :-----: | :------------------------ | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `HarfBuzzSharp.Buffer`    | `HarfBuzzSharp` | accepts UTF-8/16/32 text, `GuessSegmentProperties`, glyph output |
|  [02]   | `HarfBuzzSharp.Font`      | `HarfBuzzSharp` | shaping font; `Shape(buffer, features)` drives layout            |
|  [03]   | `HarfBuzzSharp.Blob`      | `HarfBuzzSharp` | immutable font-face bytes backing the HarfBuzz `Face`            |
|  [04]   | `SKTypeface`              | `SkiaSharp`     | `SKShaper` ctor input; `OpenStream` feeds `ToHarfBuzzBlob`       |
|  [05]   | `SKFont`                  | `SkiaSharp`     | size/scale/typeface carrier for shape + draw                     |
|  [06]   | `SKTextBlobBuilder`       | `SkiaSharp`     | `AllocateRawPositionedRun` consumes shaped glyphs+points         |
|  [07]   | `SKRawRunBuffer<ushort>`  | `SkiaSharp`     | positioned-run `Glyphs`/`Positions` spans filled per glyph       |
|  [08]   | `SKTextBlob`              | `SkiaSharp`     | built shaped blob handed to `SKCanvas.DrawText`                  |

## [03]-[ENTRYPOINTS]

[SHAPING_ENTRYPOINTS]: shape and draw — the obsolete `SKPaint` overloads are superseded by the `SKFont`/`SKTextAlign` forms; bind the `SKFont` forms — rail: typography

| [INDEX] | [SURFACE]                                                                        | [SURFACE_ROOT]     | [NOTE]                                                          |
| :-----: | :------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `Shape(string, SKFont) : Result`                                                 | `SKShaper`         | shapes from origin (0,0); allocates a transient `Buffer`       |
|  [02]   | `Shape(string, float xOffset, float yOffset, SKFont) : Result`                   | `SKShaper`         | shapes from an explicit pen origin                             |
|  [03]   | `Shape(Buffer, SKFont) : Result`                                                 | `SKShaper`         | caller-owned `Buffer` (reuse + manual segment properties)      |
|  [04]   | `Shape(Buffer, float xOffset, float yOffset, SKFont) : Result`                   | `SKShaper`         | caller-owned `Buffer` with explicit origin                     |
|  [05]   | `DrawShapedText(this SKCanvas, string, float x, float y, SKTextAlign, SKFont, SKPaint)` | `CanvasExtensions` | shapes + builds `SKTextBlob` + draws; one-shot internal `SKShaper` |
|  [06]   | `DrawShapedText(this SKCanvas, SKShaper, string, float x, float y, SKTextAlign, SKFont, SKPaint)` | `CanvasExtensions` | shaper-reuse overload — amortizes face load across draws       |
|  [07]   | `DrawShapedText(this SKCanvas, string, SKPoint, SKTextAlign, SKFont, SKPaint)`   | `CanvasExtensions` | `SKPoint` origin variant of [05]                               |
|  [08]   | `ToHarfBuzzBlob(this SKStreamAsset) : HarfBuzzSharp.Blob`                         | `BlobExtensions`   | wraps the typeface stream; release disposes the asset          |
|  [09]   | `GetScale(this HarfBuzzSharp.Font) : SKSizeI`                                     | `FontExtensions`   | reads HarfBuzz integer scale as `SKSizeI`                       |
|  [10]   | `SetScale(this HarfBuzzSharp.Font, SKSizeI)`                                      | `FontExtensions`   | writes HarfBuzz integer scale from `SKSizeI`                    |

[RESULT_ENTRYPOINTS]: `SKShaper.Result` members — parallel arrays index 1:1 by glyph; advance the flow with `Width` — rail: typography

| [INDEX] | [SURFACE]            | [SURFACE_ROOT]    | [SHAPE]                                                       |
| :-----: | :------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `Codepoints : uint[]`| `SKShaper.Result` | shaped glyph ids (cast to `ushort` for `SKTextBlobBuilder`)  |
|  [02]   | `Clusters : uint[]`  | `SKShaper.Result` | source UTF cluster index per glyph (hit-test / caret map)    |
|  [03]   | `Points : SKPoint[]` | `SKShaper.Result` | absolute glyph origins in the shaped run                     |
|  [04]   | `Width : float`      | `SKShaper.Result` | total advance — line-break, alignment, and column-flow input |
|  [05]   | `Result(uint[] codepoints, uint[] clusters, SKPoint[] points, float width)` | `SKShaper.Result` | full ctor for synthesizing/transforming runs outside the shaper |

## [04]-[IMPLEMENTATION_LAW]

[SHAPING_PIPELINE]:
- `SKShaper(SKTypeface)` opens the typeface stream, converts it via `ToHarfBuzzBlob`, builds a `HarfBuzzSharp.Face`/`Font`, sets the internal `FONT_SIZE_SCALE` (512) HarfBuzz scale, and selects OpenType functions — all face setup happens once per shaper.
- `Shape(...)` runs `hbFont.Shape(buffer, Array.Empty<Feature>())`, then rescales HarfBuzz integer advances by `font.Size / 512 * font.ScaleX` into `Result.Points`/`Width`. The `string` overloads `AddUtf8` + `GuessSegmentProperties` on a transient buffer; the `Buffer` overloads trust caller-prepared script/direction/language.
- `DrawShapedText` shapes the run, allocates a positioned run on `SKTextBlobBuilder.AllocateRawPositionedRun(font, count, null)`, copies `Codepoints`->`Glyphs` and `Points`->`Positions`, builds an `SKTextBlob`, applies the `SKTextAlign` offset (`Left`=0, `Center`=-Width/2, `Right`=-Width), and draws via `SKCanvas.DrawText`.

[STACKING]:
- One `SKShaper` per typeface is the reuse unit: a custom-control text rail, a chart-axis labeler, an SVG `<text>` flow, and a diagnostics overlay all hold a shaper per face and call the shaper-reuse `DrawShapedText` overload — the one-shot `string` overload reloads the face on every draw and is for incidental labels only.
- For typeset layout (line breaking, bidi runs, ligature-aware caret hit-testing), drive `Shape(Buffer, SKFont)` with a caller-prepared `HarfBuzzSharp.Buffer` (explicit `Direction`/`Script`/`Language`, feature tags) and consume `Result.Clusters` for the source-index map; the `string` convenience path discards that control by calling `GuessSegmentProperties`.
- The shaped `Result` is the seam between this rail and the GPU/raster render path: feed `Codepoints`+`Points` straight into `SKTextBlobBuilder` (or an Avalonia `GlyphRun`) rather than re-shaping, so shaping cost is paid once and the render backend (Skia GL/Vulkan via `Avalonia.Skia`) only rasterizes.
- Font-stream-to-blob (`ToHarfBuzzBlob`) is the single admitted path from a Skia typeface to HarfBuzz face bytes; it binds the blob's release to the asset's `Dispose`, so the caller must keep the `SKStreamAsset` alive for the shaper's lifetime — `SKShaper` already owns this internally.

[TYPOGRAPHY_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: HarfBuzz-backed shaping for Skia text, glyph runs, and rendered labels
- Accept: typography roles shape through one `SKShaper` per face, reuse it across draws, and bind the `SKFont`/`SKTextAlign` overloads
- Reject: the `[Obsolete]` `SKPaint` overloads; manual glyph placement; re-shaping a stable string per frame

[VISUAL_TEXT_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: shaped-text output for custom controls, chart labels, SVG text, diagnostics, and rendered evidence
- Accept: text shaping is one typography rail across all AppUi modalities, with `Result` as the shared shaped-run record
- Reject: per-control glyph positioning code; per-control face reloads

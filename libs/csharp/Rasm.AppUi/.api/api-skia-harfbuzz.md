# [RASM_APPUI_API_SKIA_HARFBUZZ]

`SkiaSharp.HarfBuzz` owns HarfBuzz-backed text shaping for Skia: `SKShaper` shapes a run through `HarfBuzzSharp`, then `CanvasExtensions` builds an `SKTextBlob` and draws it through `SkiaSharp`. A managed-only bridge holding no glyph data of its own, it feeds the typography rail across every AppUi text surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.HarfBuzz`
- package: `SkiaSharp.HarfBuzz` (MIT, Microsoft)
- assembly: `SkiaSharp.HarfBuzz`
- namespace: `SkiaSharp.HarfBuzz`
- depends: `SkiaSharp` (`SKCanvas`/`SKFont`/`SKTextBlobBuilder`), `HarfBuzzSharp` (`Blob`/`Buffer`/`Font`)
- native: managed-only bridge; `libHarfBuzzSharp` (`api-harfbuzz-native.md`) backs shaping and faults at first shape on a missing-RID asset
- rail: typography

## [02]-[PUBLIC_TYPES]

[SHAPING_TYPES]: shaping owner, its result record, and the draw and interop extension classes

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :----------------------- | :------------ | :-------------------------------- |
|  [01]   | `SKShaper : IDisposable` | class         | dispose-bound per-typeface shaper |
|  [02]   | `SKShaper.Result`        | value record  | shaped-run arrays with advance    |
|  [03]   | `CanvasExtensions`       | static class  | canvas shaped-text draw           |
|  [04]   | `FontExtensions`         | static class  | HarfBuzz scale interop            |
|  [05]   | `BlobExtensions`         | static class  | typeface stream to HarfBuzz blob  |

[SUBSTRATE_TYPES]: shaping and render substrate the bridge consumes

| [INDEX] | [SYMBOL]                  | [ASSEMBLY]      | [CAPABILITY]                         |
| :-----: | :------------------------ | :-------------- | :----------------------------------- |
|  [01]   | `HarfBuzzSharp.Buffer`    | `HarfBuzzSharp` | text-to-glyph shaping buffer         |
|  [02]   | `HarfBuzzSharp.Font`      | `HarfBuzzSharp` | shaping font driving layout          |
|  [03]   | `HarfBuzzSharp.Blob`      | `HarfBuzzSharp` | immutable font-face bytes            |
|  [04]   | `SKTypeface`              | `SkiaSharp`     | shaper ctor typeface source          |
|  [05]   | `SKFont`                  | `SkiaSharp`     | size and typeface carrier            |
|  [06]   | `SKTextBlobBuilder`       | `SkiaSharp`     | positioned-run blob builder          |
|  [07]   | `SKRawRunBuffer<SKPoint>` | `SkiaSharp`     | per-glyph `Glyphs`/`Positions` spans |
|  [08]   | `SKTextBlob`              | `SkiaSharp`     | built shaped blob to draw            |

## [03]-[ENTRYPOINTS]

[SHAPING_ENTRYPOINTS]: shape a run from a transient string or a caller-prepared buffer, returning a `Result`
- root: `SKShaper`

| [INDEX] | [SURFACE] | [CALL]                                           |
| :-----: | :-------- | :----------------------------------------------- |
|  [01]   | `Shape`   | `(string, SKFont)` transient buffer from origin  |
|  [02]   | `Shape`   | `(string, x, y, SKFont)` transient, explicit pen |
|  [03]   | `Shape`   | `(Buffer, SKFont)` caller buffer from origin     |
|  [04]   | `Shape`   | `(Buffer, x, y, SKFont)` caller, explicit pen    |

[DRAW_ENTRYPOINTS]: shape, build an `SKTextBlob`, and draw it
- root: `SKCanvas`
- form: without `SKShaper` a transient shaper, without `SKTextAlign` the `paint.TextAlign`; each row's `(x, y)` origin has an `SKPoint` twin

| [INDEX] | [SURFACE]        | [CALL]                                                                     |
| :-----: | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `DrawShapedText` | `(string, x, y, SKFont, SKPaint)` transient shaper, paint align            |
|  [02]   | `DrawShapedText` | `(string, x, y, SKTextAlign, SKFont, SKPaint)` transient shaper, set align |
|  [03]   | `DrawShapedText` | `(SKShaper, string, x, y, SKFont, SKPaint)` reused shaper, paint align     |
|  [04]   | `DrawShapedText` | `(SKShaper, string, x, y, SKTextAlign, SKFont, SKPaint)` reused, set align |

[INTEROP_ENTRYPOINTS]: font-stream-to-blob admission and HarfBuzz scale marshalling

| [INDEX] | [SURFACE]        | [ROOT]           | [CALL]                                                        |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `ToHarfBuzzBlob` | `BlobExtensions` | `(this SKStreamAsset) -> Blob` release bound to asset dispose |
|  [02]   | `GetScale`       | `FontExtensions` | `(this HarfBuzzSharp.Font) -> SKSizeI`                        |
|  [03]   | `SetScale`       | `FontExtensions` | `(this HarfBuzzSharp.Font, SKSizeI)`                          |

[RESULT_ENTRYPOINTS]: parallel arrays index 1:1 by glyph; advance the flow with `Width`
- root: `SKShaper.Result`

| [INDEX] | [SURFACE]    | [CALL]                                                                   |
| :-----: | :----------- | :----------------------------------------------------------------------- |
|  [01]   | `Codepoints` | `uint[]` shaped glyph ids, cast `ushort` for the blob builder            |
|  [02]   | `Clusters`   | `uint[]` source UTF cluster indices for hit-test and caret               |
|  [03]   | `Points`     | `SKPoint[]` absolute glyph origins                                       |
|  [04]   | `Width`      | `float` total advance for line-break, align, column flow                 |
|  [05]   | `Result`     | `(uint[], uint[], SKPoint[], float)` synthesize a run outside the shaper |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SKShaper(SKTypeface)` opens the typeface stream through `ToHarfBuzzBlob`, builds a `Face`/`Font`, sets the internal 512 HarfBuzz scale, and selects OpenType functions — face setup runs once per shaper.
- `Shape` runs `hbFont.Shape(buffer, Array.Empty<Feature>())`, then rescales integer advances by `font.Size / 512 * font.ScaleX` into `Points`/`Width`; a `string` overload calls `AddUtf8` + `GuessSegmentProperties` on a transient buffer, a `Buffer` overload trusts caller-prepared script, direction, and language.
- `DrawShapedText` shapes the run, allocates `SKTextBlobBuilder.AllocateRawPositionedRun(font, count, null)`, copies `Codepoints`->`Glyphs` and `Points`->`Positions`, builds the `SKTextBlob`, applies the `SKTextAlign` offset (`Left`=0, `Center`=-Width/2, `Right`=-Width), and draws `SKCanvas.DrawText`.

[STACKING]:
- `SkiaSharp`(`api-skiasharp.md`): `SKShaper.Shape` feeds `Codepoints`+`Points` into `SKTextBlobBuilder.AllocateRawPositionedRun`, and the built `SKTextBlob` draws through `SKCanvas.DrawText`; `SKTypeface.OpenStream` and `SKFont` supply the shaper's face and scale.
- `HarfBuzzSharp`(`api-harfbuzz-native.md`): `SKShaper` P/Invokes `libHarfBuzzSharp` through `HarfBuzzSharp.Font.Shape` over a `Buffer`; that catalog owns the native payload identity and RID fault.
- `Avalonia.Skia`(`api-avalonia-skia.md`): a custom control draws the shaped `SKTextBlob` onto the leased live `SKCanvas`, sharing Avalonia's GPU context so shaping cost is paid once and the backend only rasterizes.
- within-lib: one `SKShaper` per typeface is the reuse unit across the text rail — control labels, chart axes, SVG `<text>`, and diagnostics each hold a shaper per face and call the `SKShaper`-carrying `DrawShapedText`; typeset layout drives `Shape(Buffer, SKFont)` with a caller-prepared `Buffer` (explicit `Direction`/`Script`/`Language`, feature tags) and reads `Result.Clusters` for the source-index map.

[LOCAL_ADMISSION]:
- `ToHarfBuzzBlob` is the single admitted path from an `SKStreamAsset` to HarfBuzz face bytes, binding blob release to the asset `Dispose`; `SKShaper` owns this internally, so a caller keeps the `SKStreamAsset` alive for the shaper's lifetime.
- One-shot `string` overloads reload the face per draw and admit only incidental labels; a reused per-face `SKShaper` is the admitted form for repeated text.

[RAIL_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: HarfBuzz-backed shaping and shaped-text draw for every AppUi typography surface — custom controls, chart labels, SVG text, diagnostics, rendered evidence
- Accept: one `SKShaper` per typeface reused across draws, the `SKFont`/`SKTextAlign` overloads, `Result` as the shared shaped-run record
- Reject: per-control glyph placement, per-control face reloads, re-shaping a stable string per frame

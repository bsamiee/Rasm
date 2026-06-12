# [RASM_APPUI_API_SKIA_HARFBUZZ]

`SkiaSharp.HarfBuzz` supplies HarfBuzz-backed text shaping, shaped text drawing, font conversion, blob conversion, and text blob output for Skia visuals.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.HarfBuzz`
- package: `SkiaSharp.HarfBuzz`
- assembly: `SkiaSharp.HarfBuzz`
- namespace: `SkiaSharp.HarfBuzz`
- dependency: `HarfBuzzSharp`
- dependency: `SkiaSharp`
- asset: runtime library
- rail: typography

## [2]-[PUBLIC_TYPES]

[SHAPING_TYPES]: text shaping extensions
- rail: typography

| [INDEX] | [SYMBOL]             | [RAIL]           |
| :-----: | :------------------- | :--------------- |
|   [1]   | `SKShaper`           | text shaper      |
|   [2]   | `SKShaperExtensions` | shape extensions |
|   [3]   | `SKCanvasExtensions` | shaped draw      |
|   [4]   | `SKDataExtensions`   | blob conversion  |
|   [5]   | `FontExtensions`     | font conversion  |
|   [6]   | `BlobExtensions`     | blob conversion  |

[DEPENDENCY_TYPES]: admitted shaping substrate
- rail: typography

| [INDEX] | [SYMBOL]               | [RAIL]            |
| :-----: | :--------------------- | :---------------- |
|   [1]   | `HarfBuzzSharp.Blob`   | text data         |
|   [2]   | `HarfBuzzSharp.Buffer` | shaping buffer    |
|   [3]   | `HarfBuzzSharp.Font`   | shaping font      |
|   [4]   | `SKFont`               | Skia font         |
|   [5]   | `SKTextBlob`           | shaped text       |
|   [6]   | `SKTextBlobBuilder`    | shaped text build |
|   [7]   | `SKRawRunBuffer<T>`    | glyph run buffer  |

## [3]-[ENTRYPOINTS]

[SHAPING_ENTRYPOINTS]: text shaping and drawing operations
- rail: typography

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]       | [RAIL]          |
| :-----: | :--------------- | :------------------- | :-------------- |
|   [1]   | `Shape`          | `SKShaper`           | shape text      |
|   [2]   | `Shape`          | `SKShaperExtensions` | shape helper    |
|   [3]   | `DrawShapedText` | `SKCanvasExtensions` | draw glyphs     |
|   [4]   | `ToHarfBuzzBlob` | `SKDataExtensions`   | blob conversion |
|   [5]   | `GetFont`        | `FontExtensions`     | font conversion |

## [4]-[IMPLEMENTATION_LAW]

[TYPOGRAPHY_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: HarfBuzz-backed shaping for Skia text, glyph runs, and rendered labels
- Accept: typography roles shape through HarfBuzz before they draw through Skia
- Reject: manual glyph placement

[VISUAL_TEXT_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: shaped text output for custom controls, chart labels, SVG text, diagnostics, and rendered evidence
- Accept: text shaping remains one typography rail across all AppUi modalities
- Reject: per-control glyph positioning code

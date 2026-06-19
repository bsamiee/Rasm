# [RASM_APPUI_API_SKIA_HARFBUZZ]

`SkiaSharp.HarfBuzz` supplies HarfBuzz-backed text shaping, shaped text drawing, font conversion, blob conversion, and text blob output for Skia visuals.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.HarfBuzz`
- package: `SkiaSharp.HarfBuzz`
- assembly: `SkiaSharp.HarfBuzz`
- namespace: `SkiaSharp.HarfBuzz`
- dependency: `HarfBuzzSharp`
- dependency: `SkiaSharp`
- asset: runtime library
- rail: typography

## [02]-[PUBLIC_TYPES]

[SHAPING_TYPES]: text shaping extensions — rail: typography

| [INDEX] | [SYMBOL]           | [KIND]          |
| :-----: | :----------------- | :-------------- |
|  [01]   | `SKShaper`         | text shaper     |
|  [02]   | `SKShaper.Result`  | shape result    |
|  [03]   | `CanvasExtensions` | shaped draw     |
|  [04]   | `FontExtensions`   | font scale      |
|  [05]   | `BlobExtensions`   | blob conversion |

[DEPENDENCY_TYPES]: admitted shaping substrate — rail: typography

| [INDEX] | [SYMBOL]               | [KIND]            |
| :-----: | :--------------------- | :---------------- |
|  [01]   | `HarfBuzzSharp.Blob`   | text data         |
|  [02]   | `HarfBuzzSharp.Buffer` | shaping buffer    |
|  [03]   | `HarfBuzzSharp.Font`   | shaping font      |
|  [04]   | `SKFont`               | Skia font         |
|  [05]   | `SKTextBlob`           | shaped text       |
|  [06]   | `SKTextBlobBuilder`    | shaped text build |
|  [07]   | `SKRawRunBuffer<T>`    | glyph run buffer  |

## [03]-[ENTRYPOINTS]

[SHAPING_ENTRYPOINTS]: text shaping and drawing operations
- rail: typography

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]     | [RAIL]          |
| :-----: | :--------------- | :----------------- | :-------------- |
|  [01]   | `Shape`          | `SKShaper`         | shape text      |
|  [02]   | `DrawShapedText` | `CanvasExtensions` | draw glyphs     |
|  [03]   | `ToHarfBuzzBlob` | `BlobExtensions`   | blob conversion |
|  [04]   | `GetScale`       | `FontExtensions`   | scale read      |
|  [05]   | `SetScale`       | `FontExtensions`   | scale write     |

[RESULT_ENTRYPOINTS]: shaped-run metrics for flow advance
- rail: typography

| [INDEX] | [SURFACE]    | [SURFACE_ROOT]    | [RAIL]            |
| :-----: | :----------- | :---------------- | :---------------- |
|  [01]   | `Width`      | `SKShaper.Result` | run advance       |
|  [02]   | `Clusters`   | `SKShaper.Result` | cluster boundary  |
|  [03]   | `Codepoints` | `SKShaper.Result` | glyph identifiers |
|  [04]   | `Points`     | `SKShaper.Result` | glyph positions   |

## [04]-[IMPLEMENTATION_LAW]

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

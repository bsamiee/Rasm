# [RASM_APPUI_API_SKIA_HARFBUZZ]

`SkiaSharp.HarfBuzz` supplies shaped text surfaces for typography and custom visual rendering.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.HarfBuzz`
- package: `SkiaSharp.HarfBuzz`
- assembly: `SkiaSharp.HarfBuzz`
- namespace: `SkiaSharp.HarfBuzz`
- asset: runtime library
- rail: typography

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: text shaping family
- rail: typography

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]      | [CAPABILITY]                |
| :-----: | :------------------- | :------------------ | :-------------------------- |
|   [1]   | `SKShaper`           | text shaper         | anchors typography contract |
|   [2]   | `SKShaperExtensions` | shaping extensions  | shapes Skia text            |
|   [3]   | `SKCanvasExtensions` | draw extensions     | draws shaped text           |
|   [4]   | `SKDataExtensions`   | HarfBuzz conversion | converts Skia data          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: text operations
- rail: typography

| [INDEX] | [SURFACE]        | [CALL_SHAPE]      | [CAPABILITY]          |
| :-----: | :--------------- | :---------------- | :-------------------- |
|   [1]   | `Shape`          | shape method      | shapes glyphs         |
|   [2]   | `DrawShapedText` | text draw method  | draws shaped glyphs   |
|   [3]   | `ToHarfBuzzBlob` | conversion method | creates HarfBuzz blob |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `SkiaSharp.HarfBuzz`
- Owns: text shaping
- Accept: typography roles shape through HarfBuzz
- Reject: manual glyph placement

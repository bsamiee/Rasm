# [RASM_APPUI_API_SKIA_NATIVE]

`SkiaSharp.NativeAssets.macOS` supplies macOS native Skia runtime assets for AppUi visual evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.macOS`
- package: `SkiaSharp.NativeAssets.macOS`
- assembly: `SkiaSharp.NativeAssets.macOS`
- namespace: `runtimes/osx/native`
- asset: native assets
- rail: visuals

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native asset family
- rail: visuals

| [INDEX] | [ASSET]               | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :-------------------- | :------------- | :---------------------- |
|   [1]   | `libSkiaSharp`        | native library | loads Skia runtime      |
|   [2]   | `runtimes/osx/native` | asset identity | selects macOS asset set |

## [3]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINT_SCOPE]: package assets
- rail: visuals

| [INDEX] | [SURFACE]                  | [ASSET_KIND]  | [CAPABILITY]              |
| :-----: | :------------------------- | :------------ | :------------------------ |
|   [1]   | `native library asset`     | runtime asset | supplies native binary    |
|   [2]   | `runtime identifier asset` | RID asset     | binds `osx` asset group   |
|   [3]   | `copy-local runtime asset` | build asset   | reaches app output folder |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `SkiaSharp.NativeAssets.macOS`
- Owns: macOS native Skia load identity
- Accept: native asset identity is evidence
- Reject: missing native load proof

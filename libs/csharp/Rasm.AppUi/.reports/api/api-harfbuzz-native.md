# [RASM_APPUI_API_HARFBUZZ_NATIVE]

`HarfBuzzSharp.NativeAssets.macOS` supplies macOS HarfBuzz native assets for shaped text.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.macOS`
- package: `HarfBuzzSharp.NativeAssets.macOS`
- assembly: `HarfBuzzSharp.NativeAssets.macOS`
- namespace: `runtimes/osx/native`
- asset: native assets
- rail: typography

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native asset family
- rail: typography

| [INDEX] | [ASSET]               | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :-------------------- | :------------- | :---------------------- |
|   [1]   | `libHarfBuzzSharp`    | native library | loads HarfBuzz runtime  |
|   [2]   | `runtimes/osx/native` | asset identity | selects macOS asset set |

## [3]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINT_SCOPE]: package assets
- rail: typography

| [INDEX] | [SURFACE]                  | [ASSET_KIND]  | [CAPABILITY]              |
| :-----: | :------------------------- | :------------ | :------------------------ |
|   [1]   | `native library asset`     | runtime asset | supplies native binary    |
|   [2]   | `runtime identifier asset` | RID asset     | binds `osx` asset group   |
|   [3]   | `copy-local runtime asset` | build asset   | reaches app output folder |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS`
- Owns: macOS HarfBuzz load identity
- Accept: text evidence records native assets
- Reject: system HarfBuzz dependency

# [RASM_APPUI_API_SKIA_NATIVE]

`SkiaSharp.NativeAssets.macOS` supplies macOS native Skia runtime assets, target imports, and compile-placeholder assets for AppUi visual evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.macOS`
- package: `SkiaSharp.NativeAssets.macOS`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: visuals

## [2]-[PACKAGE_ASSETS]

[NATIVE_ASSETS]: macOS Skia payload
- rail: visuals

| [INDEX] | [ASSET]                                | [RAIL]          |
| :-----: | :------------------------------------- | :-------------- |
|   [1]   | `libSkiaSharp.dylib`                   | native library  |
|   [2]   | `runtimes/osx/native`                  | RID payload     |
|   [3]   | `lib/net10.0/_._`                      | compile marker  |
|   [4]   | `lib/net10.0-macos26.2/_._`            | platform marker |
|   [5]   | `SkiaSharp.NativeAssets.macOS.targets` | target import   |

[TARGET_ASSETS]: buildTransitive target groups
- rail: visuals

| [INDEX] | [ASSET]                             | [RAIL]           |
| :-----: | :---------------------------------- | :--------------- |
|   [1]   | `buildTransitive/net10.0-macos26.2` | macOS target     |
|   [2]   | `buildTransitive/net9.0-macos15.0`  | macOS target     |
|   [3]   | `buildTransitive/net48`             | framework target |
|   [4]   | `buildTransitive/net462`            | framework target |

## [3]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: native asset operations
- rail: visuals

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------------------- | :------------- | :------------- |
|   [1]   | `native library asset`     | `runtimes/osx` | Skia load      |
|   [2]   | `runtime identifier asset` | `osx`          | RID selection  |
|   [3]   | `copy-local runtime asset` | build targets  | output copy    |
|   [4]   | `compile marker`           | `lib/net10.0`  | no managed API |

## [4]-[IMPLEMENTATION_LAW]

[NATIVE_ASSET_LAW]:
- Package: `SkiaSharp.NativeAssets.macOS`
- Owns: macOS native Skia load identity, target import, and output asset presence
- Accept: native asset identity is part of visual evidence
- Reject: missing native load proof

[API_BOUNDARY_LAW]:
- Package: `SkiaSharp.NativeAssets.macOS`
- Owns: native payload only
- Accept: managed API facts remain in `SkiaSharp`
- Reject: documenting native assets as public managed types

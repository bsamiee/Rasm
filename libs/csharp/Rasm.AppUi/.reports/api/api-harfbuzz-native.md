# [RASM_APPUI_API_HARFBUZZ_NATIVE]

`HarfBuzzSharp.NativeAssets.macOS` supplies macOS HarfBuzz native assets, target imports, and compile-placeholder assets for shaped text.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.macOS`
- package: `HarfBuzzSharp.NativeAssets.macOS`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: typography

## [2]-[PACKAGE_ASSETS]

[NATIVE_ASSETS]: macOS HarfBuzz payload
- rail: typography

| [INDEX] | [ASSET]                                    | [RAIL]          |
| :-----: | :----------------------------------------- | :-------------- |
|   [1]   | `libHarfBuzzSharp.dylib`                   | native library  |
|   [2]   | `runtimes/osx/native`                      | RID payload     |
|   [3]   | `lib/net10.0/_._`                          | compile marker  |
|   [4]   | `lib/net10.0-macos26.2/_._`                | platform marker |
|   [5]   | `HarfBuzzSharp.NativeAssets.macOS.targets` | target import   |

[TARGET_ASSETS]: buildTransitive target groups
- rail: typography

| [INDEX] | [ASSET]                             | [RAIL]           |
| :-----: | :---------------------------------- | :--------------- |
|   [1]   | `buildTransitive/net10.0-macos26.2` | macOS target     |
|   [2]   | `buildTransitive/net9.0-macos15.0`  | macOS target     |
|   [3]   | `buildTransitive/net48`             | framework target |
|   [4]   | `buildTransitive/net462`            | framework target |

## [3]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: native asset operations
- rail: typography

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------------------- | :------------- | :------------- |
|   [1]   | `native library asset`     | `runtimes/osx` | HarfBuzz load  |
|   [2]   | `runtime identifier asset` | `osx`          | RID selection  |
|   [3]   | `copy-local runtime asset` | build targets  | output copy    |
|   [4]   | `compile marker`           | `lib/net10.0`  | no managed API |

## [4]-[IMPLEMENTATION_LAW]

[NATIVE_ASSET_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS`
- Owns: macOS HarfBuzz load identity, target import, and output asset presence
- Accept: text evidence records native assets as part of typography proof
- Reject: system HarfBuzz dependency

[API_BOUNDARY_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS`
- Owns: native payload only
- Accept: managed API facts remain in `HarfBuzzSharp` and `SkiaSharp.HarfBuzz`
- Reject: documenting native assets as public managed types

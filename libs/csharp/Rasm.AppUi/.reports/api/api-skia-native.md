# [RASM_APPUI_API_SKIA_NATIVE]

`SkiaSharp.NativeAssets.macOS`, `SkiaSharp.NativeAssets.Win32`, and `SkiaSharp.NativeAssets.Linux.NoDependencies` supply per-platform native Skia runtime assets, target imports, and compile-placeholder assets for AppUi visual evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.macOS`
- package: `SkiaSharp.NativeAssets.macOS`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: visuals

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.Win32`
- package: `SkiaSharp.NativeAssets.Win32`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: visuals

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.Linux.NoDependencies`
- package: `SkiaSharp.NativeAssets.Linux.NoDependencies`
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

[WINDOWS_NATIVE_ASSETS]: Windows Skia payload
- rail: visuals

| [INDEX] | [ASSET]                                | [RAIL]          |
| :-----: | :------------------------------------- | :-------------- |
|   [1]   | `libSkiaSharp.dll`                     | native library  |
|   [2]   | `runtimes/win-x64/native`              | RID payload     |
|   [3]   | `runtimes/win-x86/native`              | RID payload     |
|   [4]   | `runtimes/win-arm64/native`            | RID payload     |
|   [5]   | `lib/net10.0/_._`                      | compile marker  |
|   [6]   | `lib/net10.0-windows10.0.19041/_._`    | platform marker |
|   [7]   | `SkiaSharp.NativeAssets.Win32.targets` | target import   |

[LINUX_NATIVE_ASSETS]: Linux Skia payload (statically linked, no fontconfig dependency)
- rail: visuals

| [INDEX] | [ASSET]                                                          | [RAIL]         |
| :-----: | :--------------------------------------------------------------- | :------------- |
|   [1]   | `libSkiaSharp.so`                                                | native library |
|   [2]   | `runtimes/linux-{x86,x64,arm,arm64}/native`                      | glibc RIDs     |
|   [3]   | `runtimes/linux-{riscv64,loongarch64}/native`                    | glibc RIDs     |
|   [4]   | `runtimes/linux-musl-{x64,arm,arm64,riscv64,loongarch64}/native` | musl RIDs      |
|   [5]   | `runtimes/linux-bionic-{x64,arm64}/native`                       | bionic RIDs    |
|   [6]   | `lib/net10.0/_._`                                                | compile marker |
|   [7]   | `SkiaSharp.NativeAssets.Linux.NoDependencies.targets`            | target import  |

[TARGET_ASSETS]: buildTransitive target groups
- rail: visuals

| [INDEX] | [ASSET]                             | [RAIL]                 |
| :-----: | :---------------------------------- | :--------------------- |
|   [1]   | `buildTransitive/net10.0-macos26.2` | macOS target           |
|   [2]   | `buildTransitive/net9.0-macos15.0`  | macOS target           |
|   [3]   | `buildTransitive/net48`             | framework target (all) |
|   [4]   | `buildTransitive/net462`            | framework target (all) |

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
- Package: `SkiaSharp.NativeAssets.macOS` + `SkiaSharp.NativeAssets.Win32` + `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: per-platform native Skia load identity, target import, and output asset presence
- Accept: native asset identity is part of visual evidence
- Reject: missing native load proof

[LINUX_VARIANT_LAW]:
- Package: `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: the Linux Skia payload; the glibc/fontconfig variant `SkiaSharp.NativeAssets.Linux` stays centrally pinned but referenced with `ExcludeAssets="all" PrivateAssets="all"`
- Accept: one Linux native payload flows to output — the statically linked `NoDependencies` build
- Reject: dual Linux `libSkiaSharp.so` payloads or a runtime fontconfig dependency

[API_BOUNDARY_LAW]:
- Package: `SkiaSharp.NativeAssets.macOS` + `SkiaSharp.NativeAssets.Win32` + `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: native payload only
- Accept: managed API facts remain in `SkiaSharp`
- Reject: documenting native assets as public managed types

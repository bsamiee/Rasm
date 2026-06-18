# [RASM_APPUI_API_HARFBUZZ_NATIVE]

`HarfBuzzSharp.NativeAssets.macOS`, `HarfBuzzSharp.NativeAssets.Win32`, `HarfBuzzSharp.NativeAssets.Linux`, and `HarfBuzzSharp.NativeAssets.WebAssembly` supply per-platform HarfBuzz native assets, target imports, and compile-placeholder assets for shaped text. No package ships managed types.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.macOS`
- package: `HarfBuzzSharp.NativeAssets.macOS`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: typography

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.Win32`
- package: `HarfBuzzSharp.NativeAssets.Win32`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: typography

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.Linux`
- package: `HarfBuzzSharp.NativeAssets.Linux`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets
- asset: buildTransitive targets
- asset: compile placeholder
- rail: typography

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.WebAssembly`
- package: `HarfBuzzSharp.NativeAssets.WebAssembly`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: static archive payload (no `runtimes/<rid>/native` folder)
- asset: buildTransitive props + targets
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

[WINDOWS_NATIVE_ASSETS]: Windows HarfBuzz payload
- rail: typography

| [INDEX] | [ASSET]                                    | [RAIL]          |
| :-----: | :----------------------------------------- | :-------------- |
|   [1]   | `libHarfBuzzSharp.dll`                     | native library  |
|   [2]   | `runtimes/win-x64/native`                  | RID payload     |
|   [3]   | `runtimes/win-x86/native`                  | RID payload     |
|   [4]   | `runtimes/win-arm64/native`                | RID payload     |
|   [5]   | `lib/net10.0/_._`                          | compile marker  |
|   [6]   | `lib/net10.0-windows10.0.19041/_._`        | platform marker |
|   [7]   | `HarfBuzzSharp.NativeAssets.Win32.targets` | target import   |

[LINUX_NATIVE_ASSETS]: Linux HarfBuzz payload
- rail: typography

| [INDEX] | [ASSET]                                    | [RAIL]         |
| :-----: | :----------------------------------------- | :------------- |
|   [1]   | `libHarfBuzzSharp.so`                      | native library |
|   [2]   | `runtimes/linux-x64/native`                | RID payload    |
|   [3]   | `runtimes/linux-x86/native`                | RID payload    |
|   [4]   | `runtimes/linux-arm/native`                | RID payload    |
|   [5]   | `runtimes/linux-arm64/native`              | RID payload    |
|   [6]   | `runtimes/linux-riscv64/native`            | RID payload    |
|   [7]   | `runtimes/linux-loongarch64/native`        | RID payload    |
|   [8]   | `runtimes/linux-musl-x64/native`           | RID payload    |
|   [9]   | `runtimes/linux-musl-arm/native`           | RID payload    |
|  [10]   | `runtimes/linux-musl-arm64/native`         | RID payload    |
|  [11]   | `runtimes/linux-musl-riscv64/native`       | RID payload    |
|  [12]   | `runtimes/linux-musl-loongarch64/native`   | RID payload    |
|  [13]   | `runtimes/linux-bionic-x64/native`         | RID payload    |
|  [14]   | `runtimes/linux-bionic-arm64/native`       | RID payload    |
|  [15]   | `lib/net10.0/_._`                          | compile marker |
|  [16]   | `HarfBuzzSharp.NativeAssets.Linux.targets` | target import  |

[WASM_NATIVE_ASSETS]: WebAssembly HarfBuzz floor
- rail: typography
- no `runtimes/<rid>/native` payload; static `libHarfBuzzSharp.a` archives keyed by Emscripten version and threading/SIMD flavor under `buildTransitive/netstandard1.0/libHarfBuzzSharp.a/`

| [INDEX] | [ASSET]                                                | [RAIL]            |
| :-----: | :----------------------------------------------------- | :---------------- |
|   [1]   | `libHarfBuzzSharp.a/2.0.23/libHarfBuzzSharp.a`         | static archive    |
|   [2]   | `libHarfBuzzSharp.a/3.1.7/libHarfBuzzSharp.a`          | static archive    |
|   [3]   | `libHarfBuzzSharp.a/3.1.12/{st\|mt\|st,simd\|mt,simd}` | flavored archives |
|   [4]   | `libHarfBuzzSharp.a/3.1.34/{st\|mt\|st,simd\|mt,simd}` | flavored archives |
|   [5]   | `libHarfBuzzSharp.a/3.1.56/{st\|mt\|st,simd\|mt,simd}` | flavored archives |
|   [6]   | `HarfBuzzSharp.NativeAssets.WebAssembly.props`         | props import      |
|   [7]   | `HarfBuzzSharp.NativeAssets.WebAssembly.targets`       | target import     |
|   [8]   | `lib/net10.0/_._`                                      | compile marker    |

[TARGET_ASSETS]: buildTransitive target groups
- rail: typography

| [INDEX] | [ASSET]                             | [RAIL]                      |
| :-----: | :---------------------------------- | :-------------------------- |
|   [1]   | `buildTransitive/net10.0-macos26.2` | macOS target                |
|   [2]   | `buildTransitive/net9.0-macos15.0`  | macOS target                |
|   [3]   | `buildTransitive/net48`             | framework target (all)      |
|   [4]   | `buildTransitive/net462`            | framework target (all)      |
|   [5]   | `buildTransitive/netstandard1.0`    | WebAssembly props + targets |

## [3]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: native asset operations
- rail: typography

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT]                             | [RAIL]           |
| :-----: | :------------------------- | :----------------------------------------- | :--------------- |
|   [1]   | `native library asset`     | `runtimes/osx`                             | HarfBuzz load    |
|   [2]   | `runtime identifier asset` | `osx\|win-*\|linux-*`                      | RID selection    |
|   [3]   | `copy-local runtime asset` | build targets                              | output copy      |
|   [4]   | `static archive asset`     | `libHarfBuzzSharp.a/<emscripten>/<flavor>` | wasm static link |
|   [5]   | `compile marker`           | `lib/net10.0`                              | no managed API   |

## [4]-[IMPLEMENTATION_LAW]

[NATIVE_ASSET_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS` + `HarfBuzzSharp.NativeAssets.Win32` + `HarfBuzzSharp.NativeAssets.Linux` + `HarfBuzzSharp.NativeAssets.WebAssembly`
- Owns: per-platform HarfBuzz load identity, target import, and output asset presence
- Accept: text evidence records native assets as part of typography proof
- Reject: system HarfBuzz dependency

[WASM_FLOOR_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.WebAssembly`
- Owns: Emscripten static-archive floor only; no shared library, no RID payload
- Accept: archive selection by Emscripten version and `st`/`mt`/`,simd` flavor through the package props and targets
- Reject: treating wasm assets as copy-local runtime libraries

[API_BOUNDARY_LAW]:
- Package: all four `HarfBuzzSharp.NativeAssets.*` packages
- Owns: native payload only
- Accept: managed API facts remain in `HarfBuzzSharp` and `SkiaSharp.HarfBuzz`
- Reject: documenting native assets as public managed types

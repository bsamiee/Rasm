# [RASM_APPUI_API_HARFBUZZ_NATIVE]

`HarfBuzzSharp.NativeAssets.macOS` and `HarfBuzzSharp.NativeAssets.Linux` are the two AppUi-admitted per-platform HarfBuzz native-asset packages: they ship no managed assembly, only the per-RID shared library, the lib compile placeholders, and the `buildTransitive` MSBuild targets that surface the native into the build output. The managed shaping API lives entirely in `HarfBuzzSharp` and `SkiaSharp.HarfBuzz` (`.api/api-skia-harfbuzz.md`); these packages exist to make `libHarfBuzzSharp` load-resolvable on every admitted profile — live macOS desktop (`osx-arm64`) and headless/server Linux — and to emit the load-identity (version, path, RID) the typography evidence stream records. `HarfBuzzSharp.NativeAssets.Win32` and `HarfBuzzSharp.NativeAssets.WebAssembly` are NOT AppUi references; they arrive only transitively in the restore graph (the `SkiaSharp.HarfBuzz` / Avalonia closure) and are documented here as the restore-floor only, never as copy-local AppUi runtime assets.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.macOS`
- package: `HarfBuzzSharp.NativeAssets.macOS` (MIT, `requireLicenseAcceptance=true`, © Microsoft Corporation)
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: `runtimes/osx/native/libHarfBuzzSharp.dylib` (single fat RID payload)
- asset: `buildTransitive` targets (the `net462`/`net48` group carries the copy logic; the `net9.0-macos15.0`/`net10.0-macos26.2` groups are empty no-ops)
- asset: `lib/<tfm>/_._` compile placeholders (no managed API)
- rail: typography

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.Linux`
- package: `HarfBuzzSharp.NativeAssets.Linux` (MIT, `requireLicenseAcceptance=true`, © Microsoft Corporation)
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: `runtimes/<rid>/native/libHarfBuzzSharp.so` across 14 Linux RIDs (glibc + musl + bionic + riscv/loongarch)
- asset: `buildTransitive/net462` + `buildTransitive/net48` targets (copy logic)
- asset: `lib/<tfm>/_._` compile placeholders (no managed API)
- rail: typography

[RESTORE_FLOOR]: `HarfBuzzSharp.NativeAssets.Win32`, `HarfBuzzSharp.NativeAssets.WebAssembly`
- not an AppUi `PackageReference`: present only in the transitive restore graph, never copy-local for the macOS/headless-Linux AppUi posture
- Win32 (MIT): `runtimes/win-{x64,x86,arm64}/native/libHarfBuzzSharp.dll`, `buildTransitive/{net462,net48}` copy targets, `lib/<tfm>/_._` markers
- WebAssembly (MIT): no `runtimes/<rid>/native` payload — static `libHarfBuzzSharp.a` archives keyed by Emscripten version and threading/SIMD flavor under `buildTransitive/netstandard1.0/libHarfBuzzSharp.a/`, plus `.props` + `.targets`
- rail: typography (out-of-posture; only the macOS/Linux pair is the AppUi text-native line)

## [02]-[PACKAGE_ASSETS]

[MACOS_NATIVE_ASSETS]: macOS HarfBuzz payload (`runtimes/osx/native`)
- rail: typography

| [INDEX] | [ASSET]                                            | [RAIL]              |
| :-----: | :------------------------------------------------- | :------------------ |
|  [01]   | `runtimes/osx/native/libHarfBuzzSharp.dylib`       | fat native (arm64+x64) |
|  [02]   | `buildTransitive/net462/*.targets`                 | copy logic (full-fw) |
|  [03]   | `buildTransitive/net48/*.targets`                  | copy logic (full-fw) |
|  [04]   | `buildTransitive/net9.0-macos15.0/*.targets`       | empty no-op         |
|  [05]   | `buildTransitive/net10.0-macos26.2/*.targets`      | empty no-op         |
|  [06]   | `lib/net10.0/_._` + `lib/net10.0-macos26.2/_._`    | compile marker (consumer TFM) |
|  [07]   | `lib/{net9.0,net9.0-macos15.0,net6.0,net462,net48,netstandard2.0,netstandard2.1}/_._` | compile markers |

[LINUX_NATIVE_ASSETS]: Linux HarfBuzz payload (14 RIDs)
- rail: typography

| [INDEX] | [ASSET]                                        | [RAIL]         |
| :-----: | :--------------------------------------------- | :------------- |
|  [01]   | `runtimes/linux-{x64,x86,arm,arm64}/native/libHarfBuzzSharp.so` | glibc payload |
|  [02]   | `runtimes/linux-{riscv64,loongarch64}/native/libHarfBuzzSharp.so` | glibc payload |
|  [03]   | `runtimes/linux-musl-{x64,arm,arm64,riscv64,loongarch64}/native/libHarfBuzzSharp.so` | musl payload |
|  [04]   | `runtimes/linux-bionic-{x64,arm64}/native/libHarfBuzzSharp.so` | Android-bionic payload |
|  [05]   | `buildTransitive/net462/*.targets` + `buildTransitive/net48/*.targets` | copy logic (no macos groups) |
|  [06]   | `lib/{net10.0,net9.0,net6.0,net462,net48,netstandard2.0,netstandard2.1}/_._` | compile markers |

[WASM_RESTORE_FLOOR]: WebAssembly Emscripten static-archive floor (out-of-posture)
- rail: typography
- no `runtimes/<rid>/native` payload; static `libHarfBuzzSharp.a` archives keyed by Emscripten version and threading/SIMD flavor under `buildTransitive/netstandard1.0/libHarfBuzzSharp.a/`

| [INDEX] | [ASSET]                                                | [RAIL]            |
| :-----: | :----------------------------------------------------- | :---------------- |
|  [01]   | `libHarfBuzzSharp.a/2.0.23/libHarfBuzzSharp.a`         | static archive (flat) |
|  [02]   | `libHarfBuzzSharp.a/3.1.7/libHarfBuzzSharp.a`          | static archive (flat) |
|  [03]   | `libHarfBuzzSharp.a/3.1.12/{st\|mt\|st,simd\|mt,simd}/libHarfBuzzSharp.a` | flavored archives |
|  [04]   | `libHarfBuzzSharp.a/3.1.34/{st\|mt\|st,simd\|mt,simd}/libHarfBuzzSharp.a` | flavored archives |
|  [05]   | `libHarfBuzzSharp.a/3.1.56/{st\|mt\|st,simd\|mt,simd}/libHarfBuzzSharp.a` | flavored archives |
|  [06]   | `buildTransitive/netstandard1.0/HarfBuzzSharp.NativeAssets.WebAssembly.props`   | props import      |
|  [07]   | `buildTransitive/netstandard1.0/HarfBuzzSharp.NativeAssets.WebAssembly.targets` | target import     |

## [03]-[ASSET_ENTRYPOINTS]

[LOAD_MECHANISM]: how the native reaches the build output and what it stacks with
- rail: typography

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT]                       | [RAIL]                       |
| :-----: | :--------------------------------- | :----------------------------------- | :--------------------------- |
|  [01]   | `ShouldIncludeNativeHarfBuzzSharp` | `buildTransitive/net462\|net48` targets | MSBuild opt-out property (default `True`) |
|  [02]   | `_NativeHarfBuzzSharpFile` glob    | `runtimes/<rid>/native/libHarfBuzzSharp.{dylib,so}` | full-framework `None`/`CopyToOutputDirectory=PreserveNewest` copy |
|  [03]   | RID-asset graph (modern TFM)       | `runtimes/<rid>/native` + SDK         | `net10.0` copy path (targets file is the empty no-op) |
|  [04]   | `runtimes/osx/native` / `runtimes/linux-*/native` | RID selection             | `osx-arm64` live + `linux-*` headless resolution |
|  [05]   | load-identity probe                | loaded `libHarfBuzzSharp` handle      | version + path + RID -> typography evidence (`Render/capture#EVIDENCE`) |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_ASSET_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS` + `HarfBuzzSharp.NativeAssets.Linux` (the two AppUi-admitted text natives)
- Owns: per-platform HarfBuzz load identity, the `buildTransitive` copy targets, and per-RID output asset presence for the live-macOS + headless-Linux posture
- Stacks: `SkiaSharp.HarfBuzz` (`.api/api-skia-harfbuzz.md`) is the managed shaper that P/Invokes the native these packages place; one resolved `libHarfBuzzSharp` serves both the retained Avalonia text stack and the `SKShaper`/`Font.Shape` shaped rail (`Theme/typography#SHAPING_RAIL`); the first shaped draw folds the native version/path/RID into the typography evidence stream alongside the `libSkiaSharp` identity (`Render/capture#EVIDENCE`)
- Accept: the native arrives through the package `runtimes/<rid>/native` payload + RID-asset/targets copy; the typography evidence records it as part of typography proof
- Reject: a system HarfBuzz dependency; documenting any native asset as a public managed type; treating Win32/WebAssembly as AppUi copy-local assets

[LOAD_MECHANISM_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS` + `HarfBuzzSharp.NativeAssets.Linux`
- Owns: the divergence between full-framework copy (`net462`/`net48` targets run the `_NativeHarfBuzzSharpFile` glob under `ShouldIncludeNativeHarfBuzzSharp`) and modern-TFM copy (`net10.0` resolves the native through the SDK RID-asset graph; the matching `buildTransitive/net10.0-*`/`net9.0-*` targets are deliberately empty)
- Accept: the AppUi `net10.0` consumer binds the RID-asset path; `ShouldIncludeNativeHarfBuzzSharp=False` is the supported opt-out when a higher layer (app-host distribution) owns native placement
- Reject: assuming the macOS/Linux `buildTransitive` targets file carries copy logic on net10 (it does not); hand-copying the dylib/so beside the RID-asset graph

[WASM_FLOOR_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.WebAssembly` (restore-floor only, not AppUi-referenced)
- Owns: the Emscripten static-archive floor — no shared library, no RID payload, archives selected by Emscripten version (`2.0.23`/`3.1.7`/`3.1.12`/`3.1.34`/`3.1.56`) and `st`/`mt`/`,simd` flavor through the package `.props`/`.targets`
- Accept: a Blazor/wasm consumer links the flavor-matched `.a` at AOT publish through the package props and targets
- Reject: treating wasm `.a` archives as copy-local runtime libraries; admitting this package into the AppUi macOS/Linux posture

[API_BOUNDARY_LAW]:
- Package: all four `HarfBuzzSharp.NativeAssets.*` packages
- Owns: native payload, copy targets, and lib compile placeholders only
- Accept: every managed shaping API fact (`HarfBuzzSharp.Blob`/`Face`/`Font`/`Buffer`/`Feature`, `SkiaSharp.HarfBuzz.SKShaper`) stays in `.api/api-skia-harfbuzz.md`
- Reject: documenting native assets as public managed types

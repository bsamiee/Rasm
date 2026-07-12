# [RASM_APPUI_API_SKIA_NATIVE]

`SkiaSharp.NativeAssets.macOS` and `SkiaSharp.NativeAssets.Linux.NoDependencies` supply the per-platform `libSkiaSharp` native payload that the managed `SkiaSharp` bindings (`api-skiasharp.md`) P/Invoke into — plus the buildTransitive `.targets` that copy the matching RID asset to output. No package ships a managed runtime assembly; the `lib/<tfm>/_._` files are compile placeholders, not managed APIs. These packages own the native load identity that the AppUi render, capture, headless, and Verify rails prove.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.macOS`

- package: `SkiaSharp.NativeAssets.macOS` `3.119.4`
- license: MIT
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets (`runtimes/osx/native/libSkiaSharp.dylib`, universal x64+arm64)
- asset: buildTransitive targets (`net10.0-macos26.2`, `net9.0-macos15.0`, `net48`, `net462`)
- asset: compile placeholder (`lib/net10.0/_._`, `lib/net10.0-macos26.2/_._`)
- rail: visuals

[PACKAGE_SURFACE]: `SkiaSharp.NativeAssets.Linux.NoDependencies`

- package: `SkiaSharp.NativeAssets.Linux.NoDependencies` `3.119.4`
- license: MIT
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: native assets (`runtimes/linux-*/native/libSkiaSharp.so`, statically linked — no fontconfig/freetype system dependency)
- asset: buildTransitive targets (`net48`, `net462`; RID-copy via the SDK runtime graph for `net10.0`, so no net10 target file is shipped)
- asset: compile placeholder (`lib/net10.0/_._`)
- rail: visuals

## [02]-[PACKAGE_ASSETS]

[NATIVE_ASSETS]: macOS Skia payload

- rail: visuals

| [INDEX] | [ASSET]                                | [RAIL]          |
| :-----: | :------------------------------------- | :-------------- |
|  [01]   | `libSkiaSharp.dylib`                   | native library  |
|  [02]   | `runtimes/osx/native`                  | RID payload     |
|  [03]   | `lib/net10.0/_._`                      | compile marker  |
|  [04]   | `lib/net10.0-macos26.2/_._`            | platform marker |
|  [05]   | `SkiaSharp.NativeAssets.macOS.targets` | target import   |

[LINUX_NATIVE_ASSETS]: Linux Skia payload (statically linked, no fontconfig dependency)

- rail: visuals

| [INDEX] | [ASSET]                                                          | [RAIL]         |
| :-----: | :--------------------------------------------------------------- | :------------- |
|  [01]   | `libSkiaSharp.so`                                                | native library |
|  [02]   | `runtimes/linux-{x86,x64,arm,arm64}/native`                      | glibc RIDs     |
|  [03]   | `runtimes/linux-{riscv64,loongarch64}/native`                    | glibc RIDs     |
|  [04]   | `runtimes/linux-musl-{x64,arm,arm64,riscv64,loongarch64}/native` | musl RIDs      |
|  [05]   | `runtimes/linux-bionic-{x64,arm64}/native`                       | bionic RIDs    |
|  [06]   | `lib/net10.0/_._`                                                | compile marker |
|  [07]   | `SkiaSharp.NativeAssets.Linux.NoDependencies.targets`            | target import  |

[TARGET_ASSETS]: buildTransitive target groups, per package (not shared — only macOS ships the macOS-workload targets)

- rail: visuals

| [INDEX] | [PACKAGE]                                     | [ROLE]                      |
| :-----: | :-------------------------------------------- | :-------------------------- |
|  [01]   | `SkiaSharp.NativeAssets.macOS`                | workload and legacy targets |
|  [02]   | `SkiaSharp.NativeAssets.Linux.NoDependencies` | legacy targets              |

[TARGET_FRAMEWORK_GROUPS]: The macOS package carries `net10.0-macos26.2`, `net9.0-macos15.0`, `net48`, and `net462` build-transitive groups. The Linux package carries `net48` and `net462`; .NET 10 resolves RID copies through the SDK runtime graph.

## [03]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: native asset operations — RID selection and output copy, no managed call surface

- rail: visuals

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT]             | [NOTE]                                                                   |
| :-----: | :------------------------- | :------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `native library asset`     | `runtimes/<rid>/native`    | `libSkiaSharp.{dylib,dll,so}` the managed binding `dlopen`s              |
|  [02]   | `runtime identifier asset` | `osx \| linux-*`           | RID the SDK resolves against the host/publish target                     |
|  [03]   | `copy-local runtime asset` | buildTransitive `.targets` | copies the selected RID payload next to the app at build                 |
|  [04]   | `compile marker`           | `lib/<tfm>/_._`            | empty placeholder so the package is reference-compatible; no managed API |

## [04]-[INTEGRATION_STACKING]

[MANAGED_LOAD_HANDSHAKE]: the native payload is the unmanaged half of every `SkiaSharp` `SKObject`.

- `api-skiasharp.md` types are managed P/Invoke shims; the first call into any `SKCanvas`/`SKSurface`/`SKImage`/`GRContext` `dlopen`s `libSkiaSharp` from the RID payload these packages copy to output. A missing/wrong-RID payload is a load-time `DllNotFoundException`, never a compile error — hence the native asset identity is part of visual evidence, not an optional runtime concern.
- The `SKObject.Dispose` lifecycle (`api-skiasharp.md` ASSET_LAW) frees the unmanaged handles these payloads back; the managed and native halves are one disposable resource.

[BACKEND_TRIGGERED_LOAD]: the `Avalonia.Skia` backend boot is what forces the load on the live path.

- `Avalonia.Desktop` `UsePlatformDetect` -> `Avalonia.Skia` `UseSkia` (`api-avalonia-desktop.md`, `api-avalonia-skia.md`) initializes `SkiaPlatform`, which constructs the first `GRContext`/raster surface and triggers the `libSkiaSharp` `dlopen`. On macOS that is the `runtimes/osx/native/libSkiaSharp.dylib` from `SkiaSharp.NativeAssets.macOS`.
- The headless/server/container render path (`api-headless.md`) loads the SAME `libSkiaSharp` for its raster `SKSurface`; on Linux that is the statically-linked `NoDependencies` `.so`, deliberately self-contained so no system fontconfig/freetype is required in a minimal container.

[VERIFY_PROOF]: native-load identity is a bridge/Verify fact.

- The capture/evidence rails (`Render/capture.md`, `Diagnostics/proof.md`) that emit `SKImage`/`SKData` receipts implicitly prove the native payload loaded; the Verify lane (`uv run python -m tools.assay bridge verify` / package publish staging) excludes host assemblies but must carry the RID-correct `libSkiaSharp` so deterministic raster evidence is reproducible per platform.

## [05]-[IMPLEMENTATION_LAW]

[NATIVE_ASSET_LAW]:

- Package: `SkiaSharp.NativeAssets.macOS` + `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: per-platform native Skia load identity, target import, and output asset presence
- Accept: native asset identity is part of visual evidence
- Reject: missing native load proof

[LINUX_VARIANT_LAW]:

- Package: `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: the Linux Skia payload; the glibc/fontconfig variant `SkiaSharp.NativeAssets.Linux` stays centrally pinned but referenced with `ExcludeAssets="all" PrivateAssets="all"`
- Accept: one Linux native payload flows to output — the statically linked `NoDependencies` build
- Reject: dual Linux `libSkiaSharp.so` payloads or a runtime fontconfig dependency

[API_BOUNDARY_LAW]:

- Package: `SkiaSharp.NativeAssets.macOS` + `SkiaSharp.NativeAssets.Linux.NoDependencies`
- Owns: native payload only
- Accept: managed API facts remain in `SkiaSharp`
- Reject: documenting native assets as public managed types

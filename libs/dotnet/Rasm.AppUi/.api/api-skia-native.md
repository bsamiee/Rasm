# [RASM_APPUI_API_SKIA_NATIVE]

`SkiaSharp.NativeAssets.macOS` and `SkiaSharp.NativeAssets.Linux.NoDependencies` supply the per-platform `libSkiaSharp` native payload the managed `SkiaSharp` bindings P/Invoke, and the buildTransitive `.targets` that copy the RID-matched asset to output. Neither ships a managed assembly — the `lib/<tfm>/_._` files are compile placeholders — so these packages own only the native load identity the AppUi render, capture, and headless paths resolve against.

## [01]-[PACKAGE_ASSETS]

[MACOS_NATIVE]: universal `libSkiaSharp.dylib`; buildTransitive `.targets` group the macOS-workload and legacy copy logic.

| [INDEX] | [ASSET]                                                                       | [ROLE]              |
| :-----: | :---------------------------------------------------------------------------- | :------------------ |
|  [01]   | `runtimes/osx/native/libSkiaSharp.dylib`                                      | universal arm64+x64 |
|  [02]   | `buildTransitive/{net10.0-macos26.0,net9.0-macos15.0,net48,net462}/*.targets` | RID copy logic      |
|  [03]   | `lib/{net10.0,net10.0-macos26.0}/_._`                                         | compile marker      |

[LINUX_NATIVE]: statically-linked `libSkiaSharp.so`, no system fontconfig/freetype; net10.0 resolves RID copies through the SDK runtime graph, so no net10 `.targets` ships.

| [INDEX] | [ASSET]                                                                          | [ROLE]         |
| :-----: | :------------------------------------------------------------------------------- | :------------- |
|  [01]   | `runtimes/linux-{x86,x64,arm,arm64,riscv64,loongarch64}/native/libSkiaSharp.so`  | glibc RID      |
|  [02]   | `runtimes/linux-musl-{x64,arm,arm64,riscv64,loongarch64}/native/libSkiaSharp.so` | musl RID       |
|  [03]   | `runtimes/linux-bionic-{x64,arm64}/native/libSkiaSharp.so`                       | bionic RID     |
|  [04]   | `buildTransitive/{net48,net462}/*.targets`                                       | legacy copy    |
|  [05]   | `lib/net10.0/_._`                                                                | compile marker |

## [02]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: RID selection and output copy — no managed call surface.

| [INDEX] | [SURFACE]                 | [ROOT]                      | [ROLE]                 |
| :-----: | :------------------------ | :-------------------------- | :--------------------- |
|  [01]   | `libSkiaSharp.{dylib,so}` | `runtimes/<rid>/native`     | dlopen target          |
|  [02]   | RID asset graph           | `osx` / `linux-*`           | SDK RID selection      |
|  [03]   | copy-local targets        | `buildTransitive/*.targets` | build-time output copy |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `libSkiaSharp` reaches build output at the RID-correct path for every admitted profile; a missing or wrong-RID payload surfaces as a load-time `DllNotFoundException` at the first `SkiaSharp` call, never a compile error.

[STACKING]:
- `api-skiasharp.md`: every `SKCanvas`/`SKSurface`/`SKImage`/`GRContext` call P/Invokes the `libSkiaSharp` these packages place, and `SKObject.Dispose` frees the unmanaged handles it backs — the managed shim and native payload are one disposable resource.
- `api-avalonia-desktop.md`/`api-avalonia-skia.md`: `UsePlatformDetect` -> `UseSkia` boots `SkiaPlatform`, whose first `GRContext`/raster surface dlopens `runtimes/osx/native/libSkiaSharp.dylib` on live macOS.
- `api-headless.md`: headless/server/container raster `SKSurface` loads the same `libSkiaSharp`; on Linux the statically-linked `NoDependencies` `.so` carries its own freetype/fontconfig, so a minimal container needs no system font stack.

[LOCAL_ADMISSION]:
- Exactly one Linux native payload reaches output — the statically-linked `NoDependencies` build; the glibc/fontconfig-dependent `SkiaSharp.NativeAssets.Linux` variant is never an AppUi copy-local asset.

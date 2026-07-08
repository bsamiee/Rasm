# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` is a meta-aggregation package: its only managed contribution is the umbrella `AppBuilderDesktopExtensions.UsePlatformDetect` extension, which selects and wires the correct per-OS windowing + rendering backend (`Avalonia.Native` on macOS, `Avalonia.Win32` on Windows, `Avalonia.X11` on Linux, `Avalonia.Skia` renderer) at boot from one call. The per-backend `Use*` extensions and their assets are transitive dependencies it orchestrates internally — they are not Avalonia.Desktop's own public surface. The retained desktop shell rail composes this single entry to mount the AppUi `SurfaceHost` onto any admitted desktop substrate without a per-host boot fork.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Desktop`
- package: `Avalonia.Desktop` `12.0.5`
- assembly: `Avalonia.Desktop` (bound asset `lib/net10.0/Avalonia.Desktop.dll`; also ships `lib/net8.0`)
- license: MIT
- namespace: `Avalonia`
- asset: aggregation runtime library — exactly one public type, one public method
- asset: backend dependency graph (`Avalonia.Native`, `Avalonia.Win32`, `Avalonia.X11`, `Avalonia.Skia`) admitted transitively
- rail: desktop-shell

## [02]-[PUBLIC_TYPES]

[BOOTSTRAP_EXTENSION]: the single public managed type — rail: desktop-shell

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]      | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :------------------ | :------------------------------------------------------------- |
|  [01]   | `AppBuilderDesktopExtensions` | umbrella extensions | one method `UsePlatformDetect` that selects+wires the OS backend |

[BACKEND_DEPENDENCY_GRAPH]: transitively-admitted backend packages — orchestrated by `UsePlatformDetect`, not Avalonia.Desktop public types — rail: desktop-shell

| [INDEX] | [PACKAGE]         | [OWNS_THE_TYPE]                    | [INTERNAL_WIRE]   |
| :-----: | :---------------- | :--------------------------------- | :---------------- |
|  [01]   | `Avalonia.Native` | `AvaloniaNativePlatformExtensions` (`UseAvaloniaNative`) | macOS native host |
|  [02]   | `Avalonia.Win32`  | `Win32ApplicationExtensions` (`UseWin32`)               | Windows backend   |
|  [03]   | `Avalonia.X11`    | `AvaloniaX11PlatformExtensions` (`UseX11`)              | Linux X11 backend |
|  [04]   | `Avalonia.Skia`   | `SkiaApplicationExtensions` (`UseSkia`) — see `api-avalonia-skia.md` | raster renderer   |

Note: the four `Use*` extensions above live in their respective backend assemblies, NOT in `Avalonia.Desktop`. AppUi never calls them directly; `UsePlatformDetect` invokes them internally on the matching OS. Documenting them as Avalonia.Desktop public entrypoints is a phantom.

## [03]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINT]: the single boot entry — rail: desktop-shell

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]                | [CALL_SHAPE_NOTE]                                                       |
| :-----: | :------------------ | :---------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `UsePlatformDetect` | `AppBuilderDesktopExtensions` | `(this AppBuilder) -> AppBuilder` — RID-detects and calls `UseWin32` / `UseAvaloniaNative` / `UseX11` plus `UseSkia` internally; fluent so it chains into the `AppBuilder` configured by `Avalonia` core |

[BACKEND_RUNTIME_ASSETS]: native payload each selected backend loads — identity, not a managed API — rail: desktop-shell

| [INDEX] | [ASSET]                   | [FROM_PACKAGE]    | [LOAD]            |
| :-----: | :------------------------ | :---------------- | :---------------- |
|  [01]   | `libAvaloniaNative.dylib` | `Avalonia.Native` | macOS native load |
|  [02]   | `Avalonia.Win32.dll`      | `Avalonia.Win32`  | Windows host load |
|  [03]   | `Avalonia.X11.dll`        | `Avalonia.X11`    | Linux host load   |
|  [04]   | `Avalonia.Skia.dll`       | `Avalonia.Skia`   | renderer load     |

## [04]-[INTEGRATION_STACKING]

[ONE_BOOT_RAIL]: `UsePlatformDetect` collapses host-substrate selection into the shared AppUi boot.
- The AppUi boot (`Shell/hosts.md`) configures the `AppBuilder` (from `Avalonia` core — `AppBuilder.Configure<App>()`, `api-avalonia.md`) then chains `.UsePlatformDetect()` and the classic-desktop lifetime from `Avalonia` core; the standalone-desktop, sidecar-shell, and companion-window modalities all enter the same `SurfaceHost` axis through this one call, satisfying the README "one shell mounts onto any substrate" law.
- The Rhino panel/modal and GH2 companion modalities mount the SAME `App`/`SurfaceHost` but boot via the host's own embedding (Avalonia-in-host), not `UsePlatformDetect`; the desktop entry is the standalone/sidecar branch of the host axis, not a parallel app.

[RENDERER_HANDSHAKE]: the renderer `UsePlatformDetect` selects is the same Skia backend the visual rail leases.
- `UsePlatformDetect` internally calls `Avalonia.Skia` `UseSkia`, admitting the `ISkiaSharpApiLeaseFeature` that the custom-visual rail leases for raw `SKCanvas` access (`api-avalonia-skia.md`, `api-skiasharp.md`); desktop boot and custom-render lease share one renderer, so leased Skia draws present in-airspace.
- The native `libSkiaSharp` payload the selected backend loads is supplied by `SkiaSharp.NativeAssets.*` (`api-skia-native.md`), keeping the macOS-native and headless-Linux render paths self-contained.

[HEADLESS_DIVERGENCE]: server/container/CI proof does NOT use the desktop entry.
- Headless render and Verify surfaces (`api-headless.md`) replace `UsePlatformDetect` with `UseHeadless` + `Avalonia.Skia`'s raster path; the desktop entry is the live-host branch only. The shared shell/render code is host-neutral; only the `AppBuilder` boot tail differs (desktop-detect vs headless), so one `SurfaceHost` serves both.

## [05]-[IMPLEMENTATION_LAW]

[DESKTOP_ADMISSION]:
- Package: `Avalonia.Desktop`
- Owns: one umbrella boot entry (`UsePlatformDetect`) that detects the OS and wires the matching windowing backend plus the Skia renderer
- Accept: standalone, sidecar, and companion desktop hosts enter the same AppUi shell rail through this single call; backend selection is internal to `UsePlatformDetect`
- Reject: host-specific boot forks; AppUi calling `UseWin32`/`UseX11`/`UseAvaloniaNative`/`UseSkia` directly (that is `UsePlatformDetect`'s internal job)

[ASSET_LAW]:
- Package: `Avalonia.Desktop`
- Owns: transitive admission of the backend dependency graph through one package reference; no public managed types beyond `AppBuilderDesktopExtensions`
- Accept: native backend assets (`libAvaloniaNative.dylib`, `Avalonia.Win32.dll`, `Avalonia.X11.dll`, `Avalonia.Skia.dll`) are part of the desktop shell proof rail
- Reject: direct backend package references as separate AppUi architectural families; documenting backend-owned `Use*` extensions as Avalonia.Desktop public API

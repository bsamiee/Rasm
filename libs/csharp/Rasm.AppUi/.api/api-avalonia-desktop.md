# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` contributes one managed entry, `AppBuilderDesktopExtensions.UsePlatformDetect`, detecting the running OS and wiring the matching windowing backend and the Skia renderer from a single boot call. Per-backend `Use*` extensions and their native payloads ride as transitive dependencies it orchestrates internally, never this package's own public surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Desktop`
- package: `Avalonia.Desktop` (MIT)
- assembly: `Avalonia.Desktop` (bound `lib/net10.0`; `lib/net8.0` present)
- namespace: `Avalonia`
- asset: aggregation runtime library admitting the per-OS backend graph transitively
- rail: desktop-shell

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `AppBuilderDesktopExtensions` (`Avalonia` namespace, static class) is the sole public managed type; its `UsePlatformDetect` method dispatches to the four backend `Use*` extensions below, each a static class owned by its own backend assembly rather than `Avalonia.Desktop`.

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                            |
| :-----: | :--------------------------------- | :-------------------------------------- |
|  [01]   | `AvaloniaNativePlatformExtensions` | `UseAvaloniaNative` — macOS native host |
|  [02]   | `Win32ApplicationExtensions`       | `UseWin32` — Windows backend            |
|  [03]   | `AvaloniaX11PlatformExtensions`    | `UseX11` — Linux X11 backend            |
|  [04]   | `SkiaApplicationExtensions`        | `UseSkia` — raster renderer             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one boot entry mounts any admitted desktop substrate

[UsePlatformDetect]:
- Shape: `(this AppBuilder) -> AppBuilder` factory extension, fluent-chained from the `Avalonia` core configuration
- Dispatch: loads HarfBuzz, then wires the running-OS backend and `UseSkia` — `UseWin32` on Windows, `UseAvaloniaNative` on macOS, `UseX11` on Linux

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UsePlatformDetect` collapses host-substrate selection into the shared AppUi boot, so the standalone, sidecar, and companion-window modalities all enter one `SurfaceHost` axis with no per-host boot fork.
- Each selected backend loads its native payload — `libAvaloniaNative.dylib` on macOS, `Avalonia.Win32.dll` on Windows, `Avalonia.X11.dll` on Linux — alongside the `Avalonia.Skia.dll` renderer every OS branch wires.

[STACKING]:
- `Avalonia`(`.api/api-avalonia.md`): `UsePlatformDetect` chains off `AppBuilder.Configure<App>()` and returns the builder for the classic-desktop lifetime tail.
- `Avalonia.Skia`(`.api/api-avalonia-skia.md`): the internal `UseSkia` admits `ISkiaSharpApiLeaseFeature`, so desktop boot and the custom-visual rail's leased `SKCanvas` share one renderer and leased draws present in-airspace.
- `SkiaSharp.NativeAssets.*`(`.api/api-skia-native.md`): the `libSkiaSharp` payload the selected backend loads keeps the macOS-native and headless-Linux render paths self-contained.
- `Avalonia.Headless`(`.api/api-headless.md`): server, container, and CI proof swap `UsePlatformDetect` for `UseHeadless` with `Avalonia.Skia`'s raster path; the host-neutral `SurfaceHost` serves both branches and only the boot tail differs.
- Rhino panel and GH2 companion modalities mount the same `App`/`SurfaceHost` through the host's own Avalonia-in-host embedding, never `UsePlatformDetect`.

[LOCAL_ADMISSION]:
- One package reference transitively admits the backend graph; the desktop shell composes only `UsePlatformDetect`, never a backend `Use*` at a call site.

[RAIL_LAW]:
- Package: `Avalonia.Desktop`
- Owns: the one umbrella boot entry that detects the OS and wires the matching windowing backend and the Skia renderer
- Accept: standalone, sidecar, and companion desktop hosts enter the AppUi shell rail through `UsePlatformDetect`, backend selection staying internal to it
- Reject: a host-specific boot fork, or AppUi calling `UseWin32`/`UseX11`/`UseAvaloniaNative`/`UseSkia` directly

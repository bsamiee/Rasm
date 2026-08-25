# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` contributes one managed entry, `AppBuilderDesktopExtensions.UsePlatformDetect`, detecting the running OS and wiring the matching windowing backend and the Skia renderer from a single boot call. Per-backend `Use*` extensions and their native payloads ride as transitive dependencies it orchestrates internally, never this package's own public surface.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `AppBuilderDesktopExtensions` (`Avalonia` namespace, static class) is the sole public managed type; its `UsePlatformDetect` method dispatches to the four backend `Use*` extensions below, each a static class owned by its own backend assembly rather than `Avalonia.Desktop`.

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                            |
| :-----: | :--------------------------------- | :-------------------------------------- |
|  [01]   | `AvaloniaNativePlatformExtensions` | `UseAvaloniaNative` — macOS native host |
|  [02]   | `Win32ApplicationExtensions`       | `UseWin32` — Windows backend            |
|  [03]   | `AvaloniaX11PlatformExtensions`    | `UseX11` — Linux X11 backend            |
|  [04]   | `SkiaApplicationExtensions`        | `UseSkia` — raster renderer             |

[NATIVE_PLATFORM_TYPES]: `Avalonia.Native` (transitive, `Avalonia` namespace) — the macOS backend's own option records, the one surface an in-host embed configures directly because `UsePlatformDetect` never runs under a foreign run loop.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `MacOSPlatformOptions`          | class         | app-identity and menu policy              |
|  [02]   | `AvaloniaNativePlatformOptions` | class         | backend, sandbox, and popup policy        |
|  [03]   | `AvaloniaNativeRenderingMode`   | enum          | `OpenGl = 1`, `Software = 2`, `Metal = 3` |

- Every one of the three declares into the `Avalonia` namespace despite shipping in `Avalonia.Native.dll`, so an `Avalonia.Native` import resolves none of them.

[EMBED_RUNTIME_TYPES]: `Avalonia.Native` — the implementation identities an embedded mount resolves at runtime; none is a compile-time surface, each is what the corresponding public member answers on the embed path.

| [INDEX] | [SYMBOL]                 | [RESOLVED_BY]                        | [IDENTITY]                                                   |
| :-----: | :----------------------- | :----------------------------------- | :----------------------------------------------------------- |
|  [01]   | `EmbeddableTopLevelImpl` | `EmbeddableControlRoot.PlatformImpl` | `TopLevelImpl` subclass, every native member inherited whole |
|  [02]   | `MacOSTopLevelHandle`    | `TopLevel.TryGetPlatformHandle()`    | `IMacOSTopLevelPlatformHandle`, descriptor `NSView`          |
|  [03]   | `StorageProviderImpl`    | `TopLevel.StorageProvider`           | native provider, all three capabilities true                 |
|  [04]   | `MetalPlatformGraphics`  | default `[Metal, OpenGl, Software]`  | `IPlatformGraphics` beside the Skia render interface         |
|  [05]   | `AvaloniaNativePlatform` | `UseAvaloniaNative()`                | one windowing platform per process                           |

- One `AppBuilder` serves the whole process: a second `Setup*` call throws `InvalidOperationException` — `Setup was already called on one of AppBuilder instances` — so every embedded root in a process shares that platform, its graphics, and its dispatcher, while additional roots construct freely once it is up.
- `RenderScaling` answers `1` on an embedded root regardless of the host's backing scale, so a DPI-aware mount reads scale from the host, never from the root.
- `StorageProviderImpl` answers `CanOpen`, `CanSave`, and `CanPickFolder` all true on an embedded root, yet a picker launched while the root's native view has no window returns a task that stays `WaitingForActivation` — no exception, no sheet, no completion — so a caller gates on a shown host window beside the capability read.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one boot entry mounts any admitted desktop substrate

[UsePlatformDetect]:
- Shape: `(this AppBuilder) -> AppBuilder` factory extension, fluent-chained from the `Avalonia` core configuration
- Dispatch: loads HarfBuzz, then wires the running-OS backend and `UseSkia` — `UseWin32` on Windows, `UseAvaloniaNative` on macOS, `UseX11` on Linux

[NATIVE_PLATFORM_OPERATIONS]: the option knobs an embedded macOS mount admits through `AppBuilder.With<T>`, all instance properties; surface cells drop the owning type root named in each table's lead.

`AvaloniaNativePlatformExtensions.UseAvaloniaNative(this AppBuilder) -> AppBuilder` wires the macOS backend the two option records configure.

`MacOSPlatformOptions` — app identity and menu policy:

| [INDEX] | [SURFACE]                            | [TYPE] | [CAPABILITY]                             |
| :-----: | :----------------------------------- | :----- | :--------------------------------------- |
|  [01]   | `ShowInDock` (default true)          | `bool` | Dock presence                            |
|  [02]   | `DisableDefaultApplicationMenuItems` | `bool` | strip Avalonia's app-menu items          |
|  [03]   | `DisableNativeMenus`                 | `bool` | disable the native menu bar              |
|  [04]   | `DisableSetProcessName`              | `bool` | leave `NSProcessInfo` name alone         |
|  [05]   | `DisableAvaloniaAppDelegate`         | `bool` | leave the host's `AppDelegate` installed |

`AvaloniaNativePlatformOptions` — backend, sandbox, and popup policy:

| [INDEX] | [SURFACE]                          | [TYPE]                                       | [CAPABILITY]                     |
| :-----: | :--------------------------------- | :------------------------------------------- | :------------------------------- |
|  [01]   | `RenderingMode`                    | `IReadOnlyList<AvaloniaNativeRenderingMode>` | ordered backend preference       |
|  [02]   | `OverlayPopups`                    | `bool`                                       | embed popups into the window     |
|  [03]   | `AppSandboxEnabled` (default true) | `bool`                                       | sandbox-scoped storage bookmarks |
|  [04]   | `AvaloniaNativeLibraryPath`        | `string?`                                    | native-binary override           |

- `RenderingMode` defaults to `[Metal, OpenGl, Software]` — first element wins, and an empty or fully unmatched list throws `InvalidOperationException` at boot rather than degrading.
- `DisableAvaloniaAppDelegate` together with `DisableSetProcessName` and `DisableNativeMenus` is the plugin-host posture: the foreign application keeps its delegate, process name, and menu bar while the embedded root renders inside its view.

[EMBED_TRANSPARENCY]: `EmbeddableControlRoot : TopLevel` seats rows [01]-[02] on the embedded root; `EmbeddableTopLevelImpl : TopLevelImpl` inherits row [03], folding the hint list onto the macOS native mode.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `TransparencyLevelHint`                                            | property | `IReadOnlyList<WindowTransparencyLevel>` preference |
|  [02]   | `ActualTransparencyLevel`                                          | property | backend-applied level, raised back through the impl |
|  [03]   | `SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel>)` | instance | drives `IAvnTopLevel.SetTransparencyMode`           |

- `TopLevelImpl.SetTransparencyLevelHint`: maps `None -> Opaque`, `Transparent -> Transparent`, `AcrylicBlur -> Blur`; `Blur` and `Mica` map to nothing and the walk skips to the next hint, and a list mapping nothing at all resets the root to `Opaque`.
- `IAvnTopLevel.SetTransparencyMode`: UNPROVEN on a host-owned `NSView` — the managed call lands and `ActualTransparencyLevel` updates, while no run confirms the foreign view composites the requested mode.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UsePlatformDetect` collapses host-substrate selection into the shared AppUi boot, so the standalone, sidecar, and companion-window modalities all enter the one `SurfaceMount` axis with no per-host boot fork.
- Each selected backend loads its native payload — `libAvaloniaNative.dylib` on macOS, `Avalonia.Win32.dll` on Windows, `Avalonia.X11.dll` on Linux — alongside the `Avalonia.Skia.dll` renderer every OS branch wires.

[STACKING]:
- `Avalonia`(`.api/api-avalonia.md`): `UsePlatformDetect` chains off `AppBuilder.Configure<App>()` and returns the builder for the classic-desktop lifetime tail.
- `Avalonia.Skia`(`.api/api-avalonia-skia.md`): the internal `UseSkia` admits `ISkiaSharpApiLeaseFeature`, so desktop boot and the custom-visual rail's leased `SKCanvas` share one renderer and leased draws present in-airspace.
- `SkiaSharp.NativeAssets.*`(`.api/api-skia-native.md`): the `libSkiaSharp` payload the selected backend loads keeps the macOS-native and headless-Linux render paths self-contained.
- `Avalonia.Headless`(`.api/api-headless.md`): server, container, and CI proof swap `UsePlatformDetect` for `UseHeadless` with `Avalonia.Skia`'s raster path; the host-neutral `SurfaceMount` axis serves both branches and only the boot tail differs.
- Rhino panel and GH2 companion modalities mount the same `App` through the host's own Avalonia-in-host embedding, never `UsePlatformDetect`.

[LOCAL_ADMISSION]:
- One package reference transitively admits the backend graph; the desktop shell composes only `UsePlatformDetect`, never a backend `Use*` at a call site.
- In-host embedding takes the one carve: a mount under a foreign macOS run loop names `UseAvaloniaNative` beside `UseSkia` because it must seat `MacOSPlatformOptions` and `AvaloniaNativePlatformOptions` values `UsePlatformDetect` never exposes, and it does so from one admission fold (`Shell/hosts#EMBED_CAPSULE` `EmbedOptions.Admit`), never from a boot-code knob.

# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` supplies desktop platform detection and native desktop boot assets for the retained shell rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Desktop`
- package: `Avalonia.Desktop`
- assembly: `Avalonia.Desktop`
- namespace: `Avalonia`
- asset: desktop runtime library
- asset: backend dependency graph
- rail: desktop-shell

## [02]-[PUBLIC_TYPES]

[BOOTSTRAP_EXTENSIONS]: desktop platform extension family
- rail: desktop-shell

| [INDEX] | [SYMBOL]                           | [RAIL]          |
| :-----: | :--------------------------------- | :-------------- |
|  [01]   | `AppBuilderDesktopExtensions`      | platform detect |
|  [02]   | `AvaloniaNativePlatformExtensions` | native backend  |
|  [03]   | `AvaloniaX11PlatformExtensions`    | X11 backend     |
|  [04]   | `Win32ApplicationExtensions`       | Win32 backend   |
|  [05]   | `SkiaApplicationExtensions`        | Skia renderer   |

[BACKEND_ASSETS]: admitted desktop payload packages
- rail: desktop-shell

| [INDEX] | [SYMBOL]          | [RAIL]            |
| :-----: | :---------------- | :---------------- |
|  [01]   | `Avalonia.Native` | macOS native host |
|  [02]   | `Avalonia.Win32`  | Windows backend   |
|  [03]   | `Avalonia.X11`    | Linux X11 backend |
|  [04]   | `Avalonia.Skia`   | raster renderer   |

## [03]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: desktop builder operations
- rail: desktop-shell

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]                     | [RAIL]             |
| :-----: | :------------------ | :--------------------------------- | :----------------- |
|  [01]   | `UsePlatformDetect` | `AppBuilderDesktopExtensions`      | platform detect    |
|  [02]   | `UseAvaloniaNative` | `AvaloniaNativePlatformExtensions` | native backend     |
|  [03]   | `UseX11`            | `AvaloniaX11PlatformExtensions`    | X11 backend        |
|  [04]   | `UseWin32`          | `Win32ApplicationExtensions`       | Win32 backend      |
|  [05]   | `UseSkia`           | `SkiaApplicationExtensions`        | renderer admission |

[RUNTIME_ASSETS]: desktop asset identity
- rail: desktop-shell

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT]    | [RAIL]            |
| :-----: | :------------------------ | :---------------- | :---------------- |
|  [01]   | `libAvaloniaNative.dylib` | `Avalonia.Native` | macOS native load |
|  [02]   | `Avalonia.Win32.dll`      | `Avalonia.Win32`  | Windows host load |
|  [03]   | `Avalonia.X11.dll`        | `Avalonia.X11`    | Linux host load   |
|  [04]   | `Avalonia.Skia.dll`       | `Avalonia.Skia`   | renderer load     |

## [04]-[IMPLEMENTATION_LAW]

[DESKTOP_ADMISSION]:
- Package: `Avalonia.Desktop`
- Owns: desktop platform selection and backend admission
- Accept: desktop, companion, and sidecar hosts enter the same AppUi shell rail
- Reject: host-specific boot forks

[ASSET_LAW]:
- Package: `Avalonia.Desktop`
- Owns: backend dependency admission through one package entry
- Accept: native backend assets remain part of the desktop shell proof rail
- Reject: direct backend package references as separate AppUi architectural families

# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` supplies desktop platform detection and native desktop boot assets for the retained shell rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Desktop`
- package: `Avalonia.Desktop`
- assembly: `Avalonia.Desktop`
- namespace: `Avalonia`
- asset: desktop runtime library
- asset: backend dependency graph
- rail: desktop-shell

## [2]-[PUBLIC_TYPES]

[BOOTSTRAP_EXTENSIONS]: desktop platform extension family
- rail: desktop-shell

| [INDEX] | [SYMBOL]                           | [RAIL]          |
| :-----: | :--------------------------------- | :-------------- |
|   [1]   | `AppBuilderDesktopExtensions`      | platform detect |
|   [2]   | `AvaloniaNativePlatformExtensions` | native backend  |
|   [3]   | `AvaloniaX11PlatformExtensions`    | X11 backend     |
|   [4]   | `Win32ApplicationExtensions`       | Win32 backend   |
|   [5]   | `SkiaApplicationExtensions`        | Skia renderer   |
|   [6]   | `HarfBuzzApplicationExtensions`    | text shaping    |

[BACKEND_ASSETS]: admitted desktop payload packages
- rail: desktop-shell

| [INDEX] | [SYMBOL]            | [RAIL]            |
| :-----: | :------------------ | :---------------- |
|   [1]   | `Avalonia.Native`   | macOS native host |
|   [2]   | `Avalonia.Win32`    | Windows backend   |
|   [3]   | `Avalonia.X11`      | Linux X11 backend |
|   [4]   | `Avalonia.Skia`     | raster renderer   |
|   [5]   | `Avalonia.HarfBuzz` | text shaping      |

## [3]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: desktop builder operations
- rail: desktop-shell

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]                     | [RAIL]             |
| :-----: | :------------------ | :--------------------------------- | :----------------- |
|   [1]   | `UsePlatformDetect` | `AppBuilderDesktopExtensions`      | platform detect    |
|   [2]   | `UseAvaloniaNative` | `AvaloniaNativePlatformExtensions` | native backend     |
|   [3]   | `UseX11`            | `AvaloniaX11PlatformExtensions`    | X11 backend        |
|   [4]   | `UseWin32`          | `Win32ApplicationExtensions`       | Win32 backend      |
|   [5]   | `UseSkia`           | `SkiaApplicationExtensions`        | renderer admission |
|   [6]   | `UseHarfBuzz`       | `HarfBuzzApplicationExtensions`    | text shaping       |

[RUNTIME_ASSETS]: desktop asset identity
- rail: desktop-shell

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT]      | [RAIL]            |
| :-----: | :------------------------ | :------------------ | :---------------- |
|   [1]   | `libAvaloniaNative.dylib` | `Avalonia.Native`   | macOS native load |
|   [2]   | `Avalonia.Win32.dll`      | `Avalonia.Win32`    | Windows host load |
|   [3]   | `Avalonia.X11.dll`        | `Avalonia.X11`      | Linux host load   |
|   [4]   | `Avalonia.Skia.dll`       | `Avalonia.Skia`     | renderer load     |
|   [5]   | `Avalonia.HarfBuzz.dll`   | `Avalonia.HarfBuzz` | shaping load      |

## [4]-[IMPLEMENTATION_LAW]

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

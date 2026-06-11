# [RASM_APPUI_API_AVALONIA_DESKTOP]

`Avalonia.Desktop` supplies desktop platform detection and native desktop boot assets for the retained shell rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Desktop`
- package: `Avalonia.Desktop`
- assembly: `Avalonia.Desktop`
- namespace: `Avalonia`
- asset: runtime library
- rail: desktop-shell

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: desktop bootstrap family
- rail: desktop-shell

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :--------------------------------- | :---------------- | :------------------------ |
|   [1]   | `AvaloniaNativePlatformExtensions` | native extension  | admits native platform    |
|   [2]   | `AvaloniaX11PlatformExtensions`    | X11 extension     | admits Linux desktop path |
|   [3]   | `UsePlatformDetect`                | builder extension | selects desktop platform  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: desktop operations
- rail: desktop-shell

| [INDEX] | [SURFACE]               | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :---------------------- | :---------------- | :------------------------ |
|   [1]   | `UsePlatformDetect`     | builder extension | selects desktop platform  |
|   [2]   | `native platform asset` | runtime asset     | supplies desktop backend  |
|   [3]   | `desktop boot asset`    | package asset     | reaches app output folder |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia.Desktop`
- Owns: desktop platform admission
- Accept: desktop hosts enter AppUi shell rail
- Reject: host-specific boot forks

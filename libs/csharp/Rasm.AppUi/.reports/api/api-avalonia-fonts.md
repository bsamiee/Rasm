# [RASM_APPUI_API_AVALONIA_FONTS]

`Avalonia.Fonts.Inter` supplies the embedded Inter font collection and app-builder font admission surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Fonts.Inter`
- package: `Avalonia.Fonts.Inter`
- assembly: `Avalonia.Fonts.Inter`
- namespace: `Avalonia.Media`
- asset: asset package
- rail: typography

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font family
- rail: typography

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]       | [CAPABILITY]              |
| :-----: | :----------------------- | :------------------- | :------------------------ |
|   [1]   | `InterFontCollection`    | font collection      | supplies Inter font faces |
|   [2]   | `EmbeddedFontCollection` | font collection base | loads embedded font data  |
|   [3]   | `IFontCollection`        | font contract        | admits collection surface |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font operations
- rail: typography

| [INDEX] | [SURFACE]           | [CALL_SHAPE]       | [CAPABILITY]              |
| :-----: | :------------------ | :----------------- | :------------------------ |
|   [1]   | `WithInterFont`     | builder extension  | admits Inter collection   |
|   [2]   | `ConfigureFonts`    | font configuration | registers font collection |
|   [3]   | `AddFontCollection` | collection call    | adds embedded fonts       |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia.Fonts.Inter`
- Owns: embedded font family assets
- Accept: typography roles resolve font assets
- Reject: system font assumptions

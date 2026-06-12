# [RASM_APPUI_API_AVALONIA_FONTS]

`Avalonia.Fonts.Inter` supplies the embedded Inter font collection and app-builder font admission surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Fonts.Inter`
- package: `Avalonia.Fonts.Inter`
- assembly: `Avalonia.Fonts.Inter`
- namespace: `Avalonia.Fonts.Inter`
- namespace: `Avalonia.Media`
- namespace: `Avalonia.Media.Fonts`
- asset: runtime library
- asset: embedded TTF resources
- rail: typography

## [2]-[PUBLIC_TYPES]

[FONT_TYPES]: font collection surface
- rail: typography

| [INDEX] | [SYMBOL]                 | [RAIL]           |
| :-----: | :----------------------- | :--------------- |
|   [1]   | `InterFontCollection`    | Inter assets     |
|   [2]   | `EmbeddedFontCollection` | embedded loading |
|   [3]   | `IFontCollection`        | font contract    |
|   [4]   | `FontManager`            | font registry    |

[FONT_ASSETS]: embedded Inter faces
- rail: typography

| [INDEX] | [SYMBOL]             | [RAIL]        |
| :-----: | :------------------- | :------------ |
|   [1]   | `Inter-Thin.ttf`     | lightest face |
|   [2]   | `Inter-Light.ttf`    | light face    |
|   [3]   | `Inter-Regular.ttf`  | regular face  |
|   [4]   | `Inter-Medium.ttf`   | medium face   |
|   [5]   | `Inter-SemiBold.ttf` | semibold face |
|   [6]   | `Inter-Bold.ttf`     | bold face     |

## [3]-[ENTRYPOINTS]

[FONT_ENTRYPOINTS]: font admission operations
- rail: typography

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]          | [RAIL]             |
| :-----: | :------------------ | :---------------------- | :----------------- |
|   [1]   | `WithInterFont`     | builder extension       | Inter admission    |
|   [2]   | `ConfigureFonts`    | `FontManager`           | font configuration |
|   [3]   | `AddFontCollection` | font collection manager | collection load    |
|   [4]   | constructor         | `InterFontCollection`   | asset collection   |

## [4]-[IMPLEMENTATION_LAW]

[TYPOGRAPHY_LAW]:
- Package: `Avalonia.Fonts.Inter`
- Owns: embedded font family assets
- Accept: typography roles resolve through the embedded Inter collection
- Reject: system font assumptions

[ASSET_LAW]:
- Package: `Avalonia.Fonts.Inter`
- Owns: deterministic font resource admission for every AppUi modality
- Accept: host panels, companion windows, sidecars, diagnostics, and support views share one font rail
- Reject: per-host font fallbacks as public package behavior

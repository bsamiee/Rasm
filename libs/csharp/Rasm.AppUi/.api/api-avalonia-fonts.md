# [RASM_APPUI_API_AVALONIA_FONTS]

`Avalonia.Fonts.Inter` supplies the embedded Inter font collection and app-builder font admission surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Fonts.Inter`
- package: `Avalonia.Fonts.Inter`
- assembly: `Avalonia.Fonts.Inter`
- namespace: `Avalonia.Fonts.Inter`
- namespace: `Avalonia.Media`
- namespace: `Avalonia.Media.Fonts`
- asset: runtime library
- asset: embedded TTF resources
- rail: typography

## [02]-[PUBLIC_TYPES]

[FONT_TYPES]: font collection surface
- rail: typography

| [INDEX] | [SYMBOL]                 | [RAIL]           |
| :-----: | :----------------------- | :--------------- |
|  [01]   | `InterFontCollection`    | Inter assets     |
|  [02]   | `EmbeddedFontCollection` | embedded loading |
|  [03]   | `IFontCollection`        | font contract    |
|  [04]   | `FontManager`            | font registry    |

[FONT_ASSETS]: embedded Inter faces
- rail: typography

| [INDEX] | [SYMBOL]             | [RAIL]        |
| :-----: | :------------------- | :------------ |
|  [01]   | `Inter-Thin.ttf`     | lightest face |
|  [02]   | `Inter-Light.ttf`    | light face    |
|  [03]   | `Inter-Regular.ttf`  | regular face  |
|  [04]   | `Inter-Medium.ttf`   | medium face   |
|  [05]   | `Inter-SemiBold.ttf` | semibold face |
|  [06]   | `Inter-Bold.ttf`     | bold face     |

## [03]-[ENTRYPOINTS]

[FONT_ENTRYPOINTS]: font admission operations
- rail: typography

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]        | [RAIL]             |
| :-----: | :------------------ | :-------------------- | :----------------- |
|  [01]   | `WithInterFont`     | `AppBuilderExtension` | Inter admission    |
|  [02]   | `ConfigureFonts`    | `AppBuilder`          | font configuration |
|  [03]   | `AddFontCollection` | `FontManager`         | collection load    |
|  [04]   | constructor         | `InterFontCollection` | asset collection   |

## [04]-[IMPLEMENTATION_LAW]

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

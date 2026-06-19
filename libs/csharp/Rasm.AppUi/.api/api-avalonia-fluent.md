# [RASM_APPUI_API_AVALONIA_FLUENT]

`Avalonia.Themes.Fluent` supplies Fluent theme resources, palette tokens, density styles, and control themes for the theme rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Themes.Fluent`
- package: `Avalonia.Themes.Fluent`
- assembly: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent.Accents`
- asset: runtime library
- asset: embedded XAML resources
- rail: theme

## [02]-[PUBLIC_TYPES]

[THEME_TYPES]: public theme objects
- rail: theme

| [INDEX] | [SYMBOL]                | [RAIL]           |
| :-----: | :---------------------- | :--------------- |
|  [01]   | `FluentTheme`           | theme root       |
|  [02]   | `ColorPaletteResources` | palette resource |

[THEME_ASSET_GROUPS]: embedded XAML families
- rail: theme

| [INDEX] | [SYMBOL]                 | [RAIL]            |
| :-----: | :----------------------- | :---------------- |
|  [01]   | `FluentTheme.xaml`       | theme root        |
|  [02]   | `FluentControls.xaml`    | control themes    |
|  [03]   | `BaseColorsPalette.xaml` | base palette      |
|  [04]   | `Compact.xaml`           | compact density   |
|  [05]   | `Controls/*.xaml`        | control resources |

## [03]-[ENTRYPOINTS]

[THEME_ENTRYPOINTS]: theme operations
- rail: theme
- surface: `FluentTheme`

| [INDEX] | [SURFACE]      | [RAIL]            |
| :-----: | :------------- | :---------------- |
|  [01]   | constructor    | theme admission   |
|  [02]   | `Palettes`     | palette mapping   |
|  [03]   | `DensityStyle` | density selection |
|  [04]   | `Resources`    | resource exposure |

[PALETTE_ENTRYPOINTS]: color resource properties
- rail: theme
- surface: `ColorPaletteResources`

| [INDEX] | [SURFACE]      | [RAIL]         |
| :-----: | :------------- | :------------- |
|  [01]   | `Accent`       | accent token   |
|  [02]   | `BaseHigh`     | base contrast  |
|  [03]   | `BaseMedium`   | base tone      |
|  [04]   | `BaseLow`      | base tone      |
|  [05]   | `AltHigh`      | alternate tone |
|  [06]   | `AltMedium`    | alternate tone |
|  [07]   | `ChromeHigh`   | chrome tone    |
|  [08]   | `ChromeMedium` | chrome tone    |
|  [09]   | `ChromeLow`    | chrome tone    |
|  [10]   | `ErrorText`    | error token    |
|  [11]   | `ListLow`      | list tone      |
|  [12]   | `RegionColor`  | region token   |

## [04]-[IMPLEMENTATION_LAW]

[THEME_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: base theme resources, palette tokens, density styles, and control themes
- Accept: product theme tokens resolve through Fluent resources
- Reject: parallel theme frameworks

[DENSITY_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: compact and default density selection
- Accept: shell, sidecar, panel, diagnostics, and support views use one density vocabulary
- Reject: host-specific spacing systems

# [RASM_APPUI_API_AVALONIA_FLUENT]

`Avalonia.Themes.Fluent` supplies Fluent theme resources, palette tokens, density styles, and control themes for the theme rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Themes.Fluent`
- package: `Avalonia.Themes.Fluent`
- assembly: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent.Accents`
- asset: runtime library
- asset: embedded XAML resources
- rail: theme

## [2]-[PUBLIC_TYPES]

[THEME_TYPES]: public theme objects
- rail: theme

| [INDEX] | [SYMBOL]                | [RAIL]           |
| :-----: | :---------------------- | :--------------- |
|   [1]   | `FluentTheme`           | theme root       |
|   [2]   | `ColorPaletteResources` | palette resource |

[THEME_ASSET_GROUPS]: embedded XAML families
- rail: theme

| [INDEX] | [SYMBOL]                 | [RAIL]            |
| :-----: | :----------------------- | :---------------- |
|   [1]   | `FluentTheme.xaml`       | theme root        |
|   [2]   | `FluentControls.xaml`    | control themes    |
|   [3]   | `BaseColorsPalette.xaml` | base palette      |
|   [4]   | `Compact.xaml`           | compact density   |
|   [5]   | `Controls/*.xaml`        | control resources |

## [3]-[ENTRYPOINTS]

[THEME_ENTRYPOINTS]: theme operations
- rail: theme
- surface: `FluentTheme`

| [INDEX] | [SURFACE]      | [RAIL]            |
| :-----: | :------------- | :---------------- |
|   [1]   | constructor    | theme admission   |
|   [2]   | `Palettes`     | palette mapping   |
|   [3]   | `DensityStyle` | density selection |
|   [4]   | `Resources`    | resource exposure |

[PALETTE_ENTRYPOINTS]: color resource properties
- rail: theme
- surface: `ColorPaletteResources`

| [INDEX] | [SURFACE]      | [RAIL]         |
| :-----: | :------------- | :------------- |
|   [1]   | `Accent`       | accent token   |
|   [2]   | `BaseHigh`     | base contrast  |
|   [3]   | `BaseMedium`   | base tone      |
|   [4]   | `BaseLow`      | base tone      |
|   [5]   | `AltHigh`      | alternate tone |
|   [6]   | `AltMedium`    | alternate tone |
|   [7]   | `ChromeHigh`   | chrome tone    |
|   [8]   | `ChromeMedium` | chrome tone    |
|   [9]   | `ChromeLow`    | chrome tone    |
|  [10]   | `ErrorText`    | error token    |
|  [11]   | `ListLow`      | list tone      |
|  [12]   | `RegionColor`  | region token   |

## [4]-[IMPLEMENTATION_LAW]

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

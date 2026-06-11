# [RASM_APPUI_API_AVALONIA_FLUENT]

`Avalonia.Themes.Fluent` supplies Fluent theme resources, palette resources, and density style for the theme rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Themes.Fluent`
- package: `Avalonia.Themes.Fluent`
- assembly: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent`
- asset: runtime library
- rail: theme

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: theme family
- rail: theme

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]           |
| :-----: | :---------------------- | :---------------- | :--------------------- |
|   [1]   | `FluentTheme`           | theme root        | anchors theme contract |
|   [2]   | `ColorPaletteResources` | palette resources | carries color tokens   |
|   [3]   | `DensityStyle`          | density mode      | controls theme density |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: theme operations
- rail: theme

| [INDEX] | [SURFACE]      | [CALL_SHAPE]     | [CAPABILITY]           |
| :-----: | :------------- | :--------------- | :--------------------- |
|   [1]   | `Palettes`     | theme property   | maps palette resources |
|   [2]   | `DensityStyle` | theme property   | selects density mode   |
|   [3]   | `Accent`       | palette property | sets accent color      |
|   [4]   | `BaseHigh`     | palette property | sets base contrast     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: base theme resources
- Accept: tokens layer over Fluent resources
- Reject: parallel theme frameworks

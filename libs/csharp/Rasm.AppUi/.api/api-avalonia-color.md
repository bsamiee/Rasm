# [RASM_APPUI_API_AVALONIA_COLOR]

`Avalonia.Controls.ColorPicker` supplies color selection controls, color views, palette families, HSV/RGB primitives, and color-change events.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.ColorPicker`
- package: `Avalonia.Controls.ColorPicker`
- assembly: `Avalonia.Controls.ColorPicker`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Controls.Converters`
- asset: runtime library
- rail: controls

## [2]-[PUBLIC_TYPES]

[COLOR_CONTROLS]: color editor surfaces
- rail: controls

| [INDEX] | [SYMBOL]         | [RAIL]           |
| :-----: | :--------------- | :--------------- |
|   [1]   | `ColorPicker`    | picker shell     |
|   [2]   | `ColorView`      | editor surface   |
|   [3]   | `ColorSpectrum`  | spectrum surface |
|   [4]   | `ColorSlider`    | slider surface   |
|   [5]   | `ColorPreviewer` | preview surface  |

[COLOR_MODELS]: color models and events
- rail: controls

| [INDEX] | [SYMBOL]                  | [RAIL]         |
| :-----: | :------------------------ | :------------- |
|   [1]   | `ColorModel`              | color model    |
|   [2]   | `ColorComponent`          | component key  |
|   [3]   | `HsvComponent`            | HSV component  |
|   [4]   | `RgbComponent`            | RGB component  |
|   [5]   | `AlphaComponentPosition`  | alpha position |
|   [6]   | `ColorChangedEventArgs`   | change event   |
|   [7]   | `ColorSpectrumShape`      | spectrum shape |
|   [8]   | `ColorSpectrumComponents` | spectrum axes  |

[PALETTE_TYPES]: palette families
- rail: controls

| [INDEX] | [SYMBOL]                   | [RAIL]           |
| :-----: | :------------------------- | :--------------- |
|   [1]   | `IColorPalette`            | palette contract |
|   [2]   | `FlatColorPalette`         | flat palette     |
|   [3]   | `FlatHalfColorPalette`     | compact flat     |
|   [4]   | `FluentColorPalette`       | Fluent palette   |
|   [5]   | `MaterialColorPalette`     | Material palette |
|   [6]   | `MaterialHalfColorPalette` | compact material |
|   [7]   | `SixteenColorPalette`      | fixed palette    |

[PRIMITIVES_AND_CONVERTERS]: color helpers
- rail: controls

| [INDEX] | [SYMBOL]              | [RAIL]           |
| :-----: | :-------------------- | :--------------- |
|   [1]   | `Hsv`                 | HSV primitive    |
|   [2]   | `Rgb`                 | RGB primitive    |
|   [3]   | `ColorHelper`         | color metadata   |
|   [4]   | `ColorPickerHelpers`  | bitmap helpers   |
|   [5]   | `ColorToHexConverter` | hex conversion   |
|   [6]   | `ToBrushConverter`    | brush conversion |
|   [7]   | `ToColorConverter`    | color conversion |

## [3]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: color editor operations
- rail: controls
- surface: `ColorView`

| [INDEX] | [SURFACE]               | [RAIL]          |
| :-----: | :---------------------- | :-------------- |
|   [1]   | `Color`                 | selected color  |
|   [2]   | `HsvColor`              | selected HSV    |
|   [3]   | `ColorChanged`          | change event    |
|   [4]   | `HexInputAlphaPosition` | alpha placement |
|   [5]   | `IsAlphaEnabled`        | alpha toggle    |
|   [6]   | `IsColorPaletteVisible` | palette toggle  |
|   [7]   | `Palette`               | palette source  |
|   [8]   | `PaletteColumnCount`    | palette layout  |

[PRIMITIVE_ENTRYPOINTS]: primitive and helper operations
- rail: controls

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]       | [RAIL]            |
| :-----: | :--------------------------- | :------------------- | :---------------- |
|   [1]   | `GetColor`                   | `IColorPalette`      | palette lookup    |
|   [2]   | `ToHsvColor`                 | `Hsv`                | HSV conversion    |
|   [3]   | `ToRgb`                      | `Hsv`                | RGB conversion    |
|   [4]   | `ToColor`                    | `Rgb`                | color conversion  |
|   [5]   | `ToHsv`                      | `Rgb`                | HSV conversion    |
|   [6]   | `GetRelativeLuminance`       | `ColorHelper`        | contrast input    |
|   [7]   | `ToDisplayName`              | `ColorHelper`        | display label     |
|   [8]   | `CreateComponentBitmapAsync` | `ColorPickerHelpers` | bitmap generation |

## [4]-[IMPLEMENTATION_LAW]

[COLOR_EDITOR_LAW]:
- Package: `Avalonia.Controls.ColorPicker`
- Owns: color editor controls, palette contracts, HSV/RGB primitives, and color event flow
- Accept: color choices enter typed editors with explicit model, palette, alpha, and spectrum state
- Reject: custom color picker forks

[MODEL_LAW]:
- Package: `Avalonia.Controls.ColorPicker`
- Owns: RGB, HSV, alpha, palette, and hex conversion vocabulary
- Accept: product color state remains typed across shell, diagnostics, support, and downstream app surfaces
- Reject: string-only color transport as a UI model

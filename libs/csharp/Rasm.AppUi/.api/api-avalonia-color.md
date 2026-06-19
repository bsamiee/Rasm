# [RASM_APPUI_API_AVALONIA_COLOR]

`Avalonia.Controls.ColorPicker` supplies color selection controls, color views, palette families, HSV/RGB primitives, and color-change events.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.ColorPicker`
- package: `Avalonia.Controls.ColorPicker`
- assembly: `Avalonia.Controls.ColorPicker`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Controls.Converters`
- asset: runtime library
- rail: controls

## [02]-[PUBLIC_TYPES]

[COLOR_CONTROLS]: color editor surfaces
- rail: controls

| [INDEX] | [SYMBOL]         | [RAIL]           |
| :-----: | :--------------- | :--------------- |
|  [01]   | `ColorPicker`    | picker shell     |
|  [02]   | `ColorView`      | editor surface   |
|  [03]   | `ColorSpectrum`  | spectrum surface |
|  [04]   | `ColorSlider`    | slider surface   |
|  [05]   | `ColorPreviewer` | preview surface  |

[COLOR_MODELS]: color models and events
- rail: controls

| [INDEX] | [SYMBOL]                  | [RAIL]         |
| :-----: | :------------------------ | :------------- |
|  [01]   | `ColorModel`              | color model    |
|  [02]   | `ColorComponent`          | component key  |
|  [03]   | `HsvComponent`            | HSV component  |
|  [04]   | `RgbComponent`            | RGB component  |
|  [05]   | `AlphaComponentPosition`  | alpha position |
|  [06]   | `ColorChangedEventArgs`   | change event   |
|  [07]   | `ColorSpectrumShape`      | spectrum shape |
|  [08]   | `ColorSpectrumComponents` | spectrum axes  |

[PALETTE_TYPES]: palette families
- rail: controls

| [INDEX] | [SYMBOL]                   | [RAIL]           |
| :-----: | :------------------------- | :--------------- |
|  [01]   | `IColorPalette`            | palette contract |
|  [02]   | `FlatColorPalette`         | flat palette     |
|  [03]   | `FlatHalfColorPalette`     | compact flat     |
|  [04]   | `FluentColorPalette`       | Fluent palette   |
|  [05]   | `MaterialColorPalette`     | Material palette |
|  [06]   | `MaterialHalfColorPalette` | compact material |
|  [07]   | `SixteenColorPalette`      | fixed palette    |

[PRIMITIVES_AND_CONVERTERS]: color helpers
- rail: controls

| [INDEX] | [SYMBOL]              | [RAIL]           |
| :-----: | :-------------------- | :--------------- |
|  [01]   | `Hsv`                 | HSV primitive    |
|  [02]   | `Rgb`                 | RGB primitive    |
|  [03]   | `ColorHelper`         | color metadata   |
|  [04]   | `ColorPickerHelpers`  | bitmap helpers   |
|  [05]   | `ColorToHexConverter` | hex conversion   |
|  [06]   | `ToBrushConverter`    | brush conversion |
|  [07]   | `ToColorConverter`    | color conversion |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: color editor operations
- rail: controls
- surface: `ColorView`

| [INDEX] | [SURFACE]               | [RAIL]          |
| :-----: | :---------------------- | :-------------- |
|  [01]   | `Color`                 | selected color  |
|  [02]   | `HsvColor`              | selected HSV    |
|  [03]   | `ColorChanged`          | change event    |
|  [04]   | `HexInputAlphaPosition` | alpha placement |
|  [05]   | `IsAlphaEnabled`        | alpha toggle    |
|  [06]   | `IsColorPaletteVisible` | palette toggle  |
|  [07]   | `Palette`               | palette source  |
|  [08]   | `PaletteColumnCount`    | palette layout  |

[PRIMITIVE_ENTRYPOINTS]: primitive and helper operations
- rail: controls

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]       | [RAIL]            |
| :-----: | :--------------------------- | :------------------- | :---------------- |
|  [01]   | `GetColor`                   | `IColorPalette`      | palette lookup    |
|  [02]   | `ToHsvColor`                 | `Hsv`                | HSV conversion    |
|  [03]   | `ToRgb`                      | `Hsv`                | RGB conversion    |
|  [04]   | `ToColor`                    | `Rgb`                | color conversion  |
|  [05]   | `ToHsv`                      | `Rgb`                | HSV conversion    |
|  [06]   | `GetRelativeLuminance`       | `ColorHelper`        | contrast input    |
|  [07]   | `ToDisplayName`              | `ColorHelper`        | display label     |
|  [08]   | `CreateComponentBitmapAsync` | `ColorPickerHelpers` | bitmap generation |

## [04]-[IMPLEMENTATION_LAW]

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

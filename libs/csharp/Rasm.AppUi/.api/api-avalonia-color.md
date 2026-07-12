# [RASM_APPUI_API_AVALONIA_COLOR]

`Avalonia.Controls.ColorPicker` is the Avalonia 12 color-editing family: `ColorView` is the full editor (spectrum, sliders, palette grid, hex input, preview) exposing the selected color in both representations (`Color` and `HsvColor`) with a `ColorChanged` event carrying old/new, and `ColorPicker : ColorView` wraps it in a flyout button. The editor's subviews are individually toggled (`IsColorSpectrumVisible`/`IsColorPaletteVisible`/`IsHexInputVisible`/`IsAlphaEnabled`/…), the active tab is `ColorViewTab` (`Spectrum`/`Palette`/`Components`), and the working model is `ColorModel` (`Hsva`/`Rgba`). Palettes are pluggable through `IColorPalette` (`GetColor(colorIndex, shadeIndex)` over a `ColorCount`×`ShadeCount` grid) with `Fluent`/`Material`/`Flat`/`SixteenColor` families. Hex transport is the static `ColorToHexConverter.ToHexString`/`ParseHexString` codec (the public conversion surface), and `ColorHelper` gives `GetRelativeLuminance`/`ToDisplayName`. The HSV/RGB primitive structs (`Hsv`/`Rgb`) and the bitmap/increment helpers are internal to the package — consumers convert through Avalonia.Media's framework `Color`/`HsvColor` value types, which `ColorView.Color`/`HsvColor` expose directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.ColorPicker`
- package: `Avalonia.Controls.ColorPicker`
- assembly: `Avalonia.Controls.ColorPicker`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Controls.Converters`
- asset: managed runtime library + embedded `avares://` XAML control templates
- tfm: `net10.0` (consumer-bound; the package multi-targets `net8.0`/`net10.0`, the workspace binds `net10.0`)
- license: `MIT`
- rail: controls

## [02]-[PUBLIC_TYPES]

[COLOR_CONTROLS]: editor controls and slider/spectrum primitives
- rail: controls

| [INDEX] | [SYMBOL]         | [BASE]             | [RAIL]                                           |
| :-----: | :--------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `ColorView`      | `TemplatedControl` | full color editor (spectrum/palette/hex/sliders) |
|  [02]   | `ColorPicker`    | `ColorView`        | flyout-button wrapper exposing `Content`         |
|  [03]   | `ColorSpectrum`  | `TemplatedControl` | box/ring spectrum primitive (`Primitives`)       |
|  [04]   | `ColorSlider`    | `Slider`           | single-component gradient slider (`Primitives`)  |
|  [05]   | `ColorPreviewer` | `TemplatedControl` | hover/preview swatch primitive (`Primitives`)    |

[COLOR_MODELS]: model/component vocabularies and the change event
- rail: controls

| [INDEX] | [SYMBOL]                  | [KIND]      | [RAIL]                                          |
| :-----: | :------------------------ | :---------- | :---------------------------------------------- |
|  [01]   | `ColorModel`              | enum        | `Hsva` / `Rgba` — the editor's working model    |
|  [02]   | `ColorViewTab`            | enum        | `Spectrum` / `Palette` / `Components` subview   |
|  [03]   | `ColorComponent`          | enum        | a single component selector (Alpha/R/G/B/H/S/V) |
|  [04]   | `HsvComponent`            | enum        | `Hue` / `Saturation` / `Value`                  |
|  [05]   | `RgbComponent`            | enum        | `Red` / `Green` / `Blue`                        |
|  [06]   | `AlphaComponentPosition`  | enum        | leading/trailing alpha placement in hex/inputs  |
|  [07]   | `ColorSpectrumShape`      | enum        | `Box` / `Ring` spectrum geometry                |
|  [08]   | `ColorSpectrumComponents` | enum        | the two axes the spectrum plots                 |
|  [09]   | `ColorChangedEventArgs`   | `EventArgs` | `OldColor` / `NewColor` on a color change       |

[PALETTE_TYPES]: pluggable palette families over `IColorPalette`
- rail: controls

| [INDEX] | [SYMBOL]                   | [KIND]           | [RAIL]                                      |
| :-----: | :------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `IColorPalette`            | palette contract | `GetColor(colorIndex, shadeIndex)` + counts |
|  [02]   | `FluentColorPalette`       | palette          | Fluent design swatch grid                   |
|  [03]   | `MaterialColorPalette`     | palette          | Material design swatch grid                 |
|  [04]   | `MaterialHalfColorPalette` | palette          | compact Material grid                       |
|  [05]   | `FlatColorPalette`         | palette          | flat-UI swatch grid                         |
|  [06]   | `FlatHalfColorPalette`     | palette          | compact flat grid                           |
|  [07]   | `SixteenColorPalette`      | palette          | fixed 16-color grid                         |

[CONVERTERS]: public value converters (the consumable conversion surface)
- rail: controls

| [INDEX] | [SYMBOL]                      | [KIND]            | [RAIL]                                                              |
| :-----: | :---------------------------- | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `ColorToHexConverter`         | `IValueConverter` | `Color` <-> hex string; static `ToHexString`/`ParseHexString` codec |
|  [02]   | `ToBrushConverter`            | `IValueConverter` | `Color`/`HsvColor` -> `IBrush` for binding                          |
|  [03]   | `ToColorConverter`            | `IValueConverter` | `HsvColor`/string -> `Color` for binding                            |
|  [04]   | `ColorToDisplayNameConverter` | `IValueConverter` | `Color` -> human display name                                       |
|  [05]   | `ColorHelper`                 | static class      | `GetRelativeLuminance`/`ToDisplayName` color metadata               |

[INTERNAL_PRIMITIVES]: `internal`, not consumable — listed so a design page does not compose them
- rail: controls

| [INDEX] | [SYMBOL]                        | [VISIBILITY] | [NOTE]                                                               |
| :-----: | :------------------------------ | :----------- | :------------------------------------------------------------------- |
|  [01]   | `Primitives.Hsv`                | `internal`   | convert via Avalonia.Media `HsvColor` (`ColorView.HsvColor`) instead |
|  [02]   | `Primitives.Rgb`                | `internal`   | convert via Avalonia.Media `Color` (`ColorView.Color`) instead       |
|  [03]   | `Primitives.ColorPickerHelpers` | `internal`   | bitmap/increment helpers used by the templates only                  |

## [03]-[ENTRYPOINTS]

[EDITOR_STATE]: `ColorView` selected-color, model, and subview-visibility surface
- rail: controls
- surface: `ColorView` (inherited unchanged by `ColorPicker`)

All entries are styled properties except the `ColorChanged` event.

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------- | :---------------------- |
|  [01]   | `Color`                                                   | selected `Color`        |
|  [02]   | `HsvColor`                                                | selected `HsvColor`     |
|  [03]   | `ColorChanged`                                            | old/new color event     |
|  [04]   | `ColorModel`                                              | `Hsva` or `Rgba` model  |
|  [05]   | `SelectedIndex`                                           | active tab index        |
|  [06]   | `IsAlphaEnabled` / `IsAlphaVisible`                       | alpha editing and view  |
|  [07]   | `IsColorSpectrumVisible` / `IsColorSpectrumSliderVisible` | spectrum controls       |
|  [08]   | `IsColorPaletteVisible`                                   | palette view            |
|  [09]   | `IsColorComponentsVisible`                                | component view          |
|  [10]   | `IsComponentSliderVisible`                                | component sliders       |
|  [11]   | `IsComponentTextInputVisible`                             | component inputs        |
|  [12]   | `IsHexInputVisible` / `HexInputAlphaPosition`             | hex input policy        |
|  [13]   | `IsColorPreviewVisible` / `IsColorModelVisible`           | preview and model views |
|  [14]   | `IsAccentColorsVisible`                                   | accent swatches         |
|  [15]   | `Palette` / `PaletteColors` / `PaletteColumnCount`        | palette source and grid |
|  [16]   | `MaxHue` / `MaxSaturation` / `MaxValue`                   | upper HSV bounds        |
|  [17]   | `MinHue` / `MinSaturation` / `MinValue`                   | lower HSV bounds        |

[PALETTE_AND_CONVERTER_OPS]: palette lookup and the hex/luminance statics
- rail: controls

| [INDEX] | [SURFACE]                                                  | [SURFACE_ROOT]        | [CAPABILITY]       |
| :-----: | :--------------------------------------------------------- | :-------------------- | :----------------- |
|  [01]   | `GetColor(int colorIndex, int shadeIndex) -> Color`        | `IColorPalette`       | grid lookup        |
|  [02]   | `ColorCount` / `ShadeCount`                                | `IColorPalette`       | grid dimensions    |
|  [03]   | `ToHexString`                                              | `ColorToHexConverter` | color-to-hex codec |
|  [04]   | `ParseHexString(string, AlphaComponentPosition) -> Color?` | `ColorToHexConverter` | hex-to-color codec |
|  [05]   | `Convert` / `ConvertBack`                                  | `ColorToHexConverter` | binding path       |
|  [06]   | `GetRelativeLuminance(Color) -> double`                    | `ColorHelper`         | WCAG input         |
|  [07]   | `ToDisplayName(Color) / ToDisplayNameExists`               | `ColorHelper`         | named-color probe  |

[HEX_SERIALIZATION_SIGNATURE]: `ToHexString(Color, AlphaComponentPosition, bool includeAlpha = true, bool includeSymbol = false)` controls alpha and prefix emission.

[SLIDER_AND_SPECTRUM_OPS]: primitive control state
- rail: controls

| [INDEX] | [SURFACE]                                                         | [SURFACE_ROOT]  | [RAIL]                                       |
| :-----: | :---------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `Color` / `HsvColor` / `ColorChanged`                             | `ColorSlider`   | bound color + change event                   |
|  [02]   | `ColorComponent` / `ColorModel`                                   | `ColorSlider`   | which component this slider edits            |
|  [03]   | `IsAlphaVisible` / `IsPerceptive` / `IsRoundingEnabled`           | `ColorSlider`   | alpha track / perceptual gradient / rounding |
|  [04]   | `Color` / `HsvColor` / `ColorChanged`                             | `ColorSpectrum` | bound color + change event                   |
|  [05]   | `Shape` / `Components` / `ThirdComponent`                         | `ColorSpectrum` | box/ring, plotted axes, third-axis slider    |
|  [06]   | `MinHue/MaxHue` `MinSaturation/MaxSaturation` `MinValue/MaxValue` | `ColorSpectrum` | spectrum HSV bounds                          |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_EDITOR_LAW]:
- `ColorView` is the editor; `ColorPicker` is the flyout wrapper that inherits the entire `ColorView` surface and adds only `Content`/`ContentTemplate` — a design that needs an inline editor binds `ColorView`, one that needs a popup binds `ColorPicker`.
- The selected color is dual-represented: `Color` (Avalonia.Media `Color`) and `HsvColor` (Avalonia.Media `HsvColor`) stay in sync, and `ColorChanged` carries `OldColor`/`NewColor` — the product color state is a typed framework value, never a string.
- Subview visibility is a property family (`Is*Visible`/`Is*Enabled`), so a constrained editor (e.g. spectrum-only, no alpha) is configured by toggling rows, never by forking a control.

[STACKING_LAW]:
- Color↔hex transport is the static codec, not a hand-rolled formatter: `ColorToHexConverter.ToHexString(color, alphaPosition, includeAlpha, includeSymbol)` and `ParseHexString(text, alphaPosition)` are the canonical hex rail — a settings persistence layer or a swatch import reads/writes hex through these statics, and the same converter binds in XAML for live hex fields.
- Conversion between RGB and HSV uses Avalonia.Media's framework types (`Color.ToHsv()`/`HsvColor.ToRgb()` live on the framework value types), not this package's `internal` `Hsv`/`Rgb` structs — a design page composing those internal primitives is composing a non-surface and is the rejected form.
- Palette swatches stack with the app theme: an `IColorPalette` implementation feeds `ColorView.Palette` and the grid lays out by `ColorCount`×`ShadeCount` with `PaletteColumnCount` columns, so a brand palette is a data source, not a templated fork.
- `[V10]` caveat — `ColorHelper.GetRelativeLuminance` is the DELETED form: the WCAG contrast/luminance transform routes through the Unicolour kernel (`tokens.md:20`, the one suite colour owner that owns the sRGB->luminance transform beside OKLab mix and colormap sampling), never Avalonia's `ColorHelper`. `ColorHelper.ToDisplayName` STAYS (the announced color label has no Unicolour counterpart). The accessibility rail's `ContrastGate` composes the Unicolour luminance, not `GetRelativeLuminance`.
- The `ColorView` editor is the inspector's color-value row (`Editing/inspector.md`, the `EditorFactory` color editor) — selected color crosses as an Avalonia.Media `Color`/`HsvColor`, mapped at the boundary onto the inspector's typed value.

[MODEL_LAW]:
- Package: `Avalonia.Controls.ColorPicker`
- Owns: color editor controls, the `ColorModel`/`ColorViewTab`/component vocabularies, the `IColorPalette` grid contract, the public hex codec, and the color-change event flow.
- Accept: color choices enter typed editors with explicit `ColorModel`, palette, alpha, and spectrum state; selected color crosses as Avalonia.Media `Color`/`HsvColor`; hex transport goes through the `ColorToHexConverter` statics.
- Reject: custom color-picker forks; string-only color transport as a UI model; composing the `internal` `Hsv`/`Rgb`/`ColorPickerHelpers` primitives as if public; hand-rolled hex formatting where `ToHexString`/`ParseHexString` is the codec.

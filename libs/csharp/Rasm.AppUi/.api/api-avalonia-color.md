# [RASM_APPUI_API_AVALONIA_COLOR]

`Avalonia.Controls.ColorPicker` owns the color-editing control family: `ColorView` is the full editor (spectrum, palette grid, hex input, component sliders, preview) and `ColorPicker : ColorView` wraps it in a flyout button. Selected color crosses as the typed Avalonia.Media `Color`/`HsvColor` pair kept in sync under a `ColorChanged` event, hex transport rides the static `ColorToHexConverter` codec, and swatch grids plug through `IColorPalette` — the sRGB luminance transport defers to the shared `Wacton.Unicolour` kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.ColorPicker`
- package: `Avalonia.Controls.ColorPicker` (`MIT`, AvaloniaUI)
- assembly: `Avalonia.Controls.ColorPicker`
- namespace: `Avalonia.Controls`, `Avalonia.Controls.Primitives`, `Avalonia.Controls.Converters`
- asset: managed runtime library with embedded `avares://` XAML control templates
- rail: controls

## [02]-[PUBLIC_TYPES]

[COLOR_CONTROLS]: editor and slider/spectrum control classes over the Avalonia templated-control base

| [INDEX] | [SYMBOL]         | [CAPABILITY]                                     |
| :-----: | :--------------- | :----------------------------------------------- |
|  [01]   | `ColorView`      | full color editor (spectrum/palette/hex/sliders) |
|  [02]   | `ColorPicker`    | flyout-button wrapper over `ColorView`           |
|  [03]   | `ColorSpectrum`  | box/ring spectrum primitive (`Primitives`)       |
|  [04]   | `ColorSlider`    | single-component gradient slider (`Primitives`)  |
|  [05]   | `ColorPreviewer` | hover/preview swatch primitive (`Primitives`)    |

[COLOR_MODELS]: component vocabularies and the change event

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `ColorModel`              | enum          | `Hsva`/`Rgba` working model               |
|  [02]   | `ColorViewTab`            | enum          | `Spectrum`/`Palette`/`Components` tab     |
|  [03]   | `ColorComponent`          | enum          | single-component selector (A/R/G/B/H/S/V) |
|  [04]   | `HsvComponent`            | enum          | `Hue`/`Saturation`/`Value`                |
|  [05]   | `RgbComponent`            | enum          | `Red`/`Green`/`Blue`                      |
|  [06]   | `AlphaComponentPosition`  | enum          | leading/trailing alpha placement          |
|  [07]   | `ColorSpectrumShape`      | enum          | `Box`/`Ring` geometry                     |
|  [08]   | `ColorSpectrumComponents` | enum          | the two plotted spectrum axes             |
|  [09]   | `ColorChangedEventArgs`   | class         | `OldColor`/`NewColor` on a change         |

[PALETTE_TYPES]: pluggable swatch grids over `IColorPalette`
- `IColorPalette` mints `GetColor(int colorIndex, int shadeIndex) -> Color` across a `ColorCount`×`ShadeCount` grid.
- [PALETTE_FAMILIES]: `FluentColorPalette` `MaterialColorPalette` `MaterialHalfColorPalette` `FlatColorPalette` `FlatHalfColorPalette` `SixteenColorPalette`

[CONVERTERS]: consumable value converters and color metadata

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ColorToHexConverter`         | converter     | `Color`<->hex; hosts the static hex codec      |
|  [02]   | `ToBrushConverter`            | converter     | `Color`/`HsvColor` -> `IBrush`                 |
|  [03]   | `ToColorConverter`            | converter     | `HsvColor`/string -> `Color`                   |
|  [04]   | `ColorToDisplayNameConverter` | converter     | `Color` -> display name                        |
|  [05]   | `ColorHelper`                 | static        | `ToDisplayName`/`ToDisplayNameExists` metadata |

## [03]-[ENTRYPOINTS]

[EDITOR_STATE]: styled properties on `ColorView`, inherited unchanged by `ColorPicker`; `ColorChanged` is the sole event

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `Color` / `HsvColor`                                                    | dual-represented selected color    |
|  [02]   | `ColorChanged`                                                          | old/new color event                |
|  [03]   | `ColorModel`                                                            | `Hsva` or `Rgba` (default `Rgba`)  |
|  [04]   | `SelectedIndex`                                                         | active tab index                   |
|  [05]   | `Palette` / `PaletteColors` / `PaletteColumnCount`                      | palette source and grid width      |
|  [06]   | `MinHue`/`MaxHue` `MinSaturation`/`MaxSaturation` `MinValue`/`MaxValue` | HSV bounds                         |
|  [07]   | `ColorSpectrumShape` / `ColorSpectrumComponents`                        | spectrum geometry and plotted axes |

- [SUBVIEW_TOGGLES]: `IsAlphaEnabled` `IsAlphaVisible` `IsColorSpectrumVisible` `IsColorSpectrumSliderVisible` `IsColorPaletteVisible` `IsColorComponentsVisible` `IsComponentSliderVisible` `IsComponentTextInputVisible` `IsHexInputVisible` `HexInputAlphaPosition` `IsColorPreviewVisible` `IsColorModelVisible` `IsAccentColorsVisible`

[PALETTE_AND_CODEC_OPS]: grid lookup, the hex codec statics, and the color-label probe

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------------------------- | :------- | :----------------- |
|  [01]   | `IColorPalette.GetColor(int, int) -> Color`                                    | instance | grid lookup        |
|  [02]   | `IColorPalette.ColorCount` / `.ShadeCount`                                     | property | grid dimensions    |
|  [03]   | `ColorToHexConverter.ToHexString(Color, AlphaComponentPosition) -> string`     | static   | color-to-hex codec |
|  [04]   | `ColorToHexConverter.ParseHexString(string, AlphaComponentPosition) -> Color?` | static   | hex-to-color codec |
|  [05]   | `ColorToHexConverter.Convert` / `.ConvertBack`                                 | instance | XAML binding path  |
|  [06]   | `ColorHelper.ToDisplayName(Color) -> string` / `.ToDisplayNameExists`          | static   | named-color label  |

- `ColorToHexConverter.ToHexString`: `includeAlpha` (default true) and `includeSymbol` (default false) control alpha and `#` emission.

[SLIDER_AND_SPECTRUM_OPS]: primitive control state

| [INDEX] | [SURFACE]                                                             | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `ColorSlider.Color` / `.HsvColor` / `.ColorChanged`                   | bound color and change event               |
|  [02]   | `ColorSlider.ColorComponent` / `.ColorModel`                          | which component the slider edits           |
|  [03]   | `ColorSlider.IsAlphaVisible` / `.IsPerceptive` / `.IsRoundingEnabled` | alpha track, perceptual gradient, rounding |
|  [04]   | `ColorSpectrum.Color` / `.HsvColor` / `.ColorChanged`                 | bound color and change event               |
|  [05]   | `ColorSpectrum.Shape` / `.Components` / `.ThirdComponent`             | box/ring, plotted axes, third-axis slider  |
|  [06]   | `ColorSpectrum.{Min,Max}{Hue,Saturation,Value}`                       | spectrum HSV bounds                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ColorView` is the editor and `ColorPicker` its flyout wrapper inheriting the whole `ColorView` surface and its `Content`/`ContentTemplate`; an inline editor binds `ColorView`, a popup binds `ColorPicker`.
- Selected color is dual-represented as Avalonia.Media `Color` and `HsvColor` kept in sync, and `ColorChanged` carries `OldColor`/`NewColor`, so color state crosses as a typed framework value.
- Subview visibility is a bool property family, so a constrained editor is one set of toggled rows.

[STACKING]:
- `Wacton.Unicolour`(`libs/csharp/.api/api-unicolour.md`, shared tier): the sRGB->relative-luminance transform routes through the Unicolour kernel that also owns OKLab mix and colormap sampling, and the accessibility `ContrastGate` composes that luminance; `ColorHelper.ToDisplayName` supplies the color label with no Unicolour counterpart.
- `Avalonia.Themes.Fluent`(`api-avalonia-fluent.md`): the theme accent/system palettes (`FluentTheme.Palettes`) are distinct from the editor `IColorPalette` swatch grids, meeting only at a resolved `Color`.
- `EditorFactory` inspector: the color-value row binds `ColorView`, mapping the selected Avalonia.Media `Color`/`HsvColor` onto the inspector's typed value at the boundary.
- Hex persistence and swatch import read and write through the `ColorToHexConverter.ToHexString`/`ParseHexString` statics, the same converter binding live hex fields in XAML.

[LOCAL_ADMISSION]:
- A brand palette is an `IColorPalette` data source feeding `ColorView.Palette`, laid out by `ColorCount`×`ShadeCount` under `PaletteColumnCount`.

[RAIL_LAW]:
- Package: `Avalonia.Controls.ColorPicker`
- Owns: the color-editor controls, the `ColorModel`/`ColorViewTab`/component vocabularies, the `IColorPalette` grid contract, the public hex codec, and the color-change event flow.
- Accept: typed editors with explicit `ColorModel`, palette, alpha, and spectrum state; selected color crossing as Avalonia.Media `Color`/`HsvColor`; RGB<->HSV via Avalonia.Media `Color.ToHsv()`/`HsvColor.ToRgb()`; hex through the `ColorToHexConverter` statics.
- Reject: custom picker forks; string-only color transport as a UI model; composing the `internal` `Primitives.Hsv`/`Rgb`/`ColorPickerHelpers` structs; hand-rolled hex formatting past `ToHexString`/`ParseHexString`; luminance via `ColorHelper.GetRelativeLuminance` where the Unicolour kernel owns the transform.

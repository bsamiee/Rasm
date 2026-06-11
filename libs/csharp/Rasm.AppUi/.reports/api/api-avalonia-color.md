# [RASM_APPUI_API_AVALONIA_COLOR]

`Avalonia.Controls.ColorPicker` supplies color selection controls, color views, and color-change events.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.ColorPicker`
- package: `Avalonia.Controls.ColorPicker`
- assembly: `Avalonia.Controls.ColorPicker`
- namespace: `Avalonia.Controls`
- asset: runtime library
- rail: controls

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: color family
- rail: controls

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :---------------------- | :-------------- | :---------------------- |
|   [1]   | `ColorPicker`           | drawing surface | draws visual evidence   |
|   [2]   | `ColorView`             | UI surface      | renders product surface |
|   [3]   | `ColorSpectrum`         | drawing surface | draws visual evidence   |
|   [4]   | `ColorSlider`           | drawing surface | draws visual evidence   |
|   [5]   | `ColorPreviewer`        | drawing surface | draws visual evidence   |
|   [6]   | `ColorChangedEventArgs` | drawing surface | draws visual evidence   |
|   [7]   | `ColorModel`            | drawing surface | draws visual evidence   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: color operations
- rail: controls

| [INDEX] | [SURFACE]               | [CALL_SHAPE]     | [CAPABILITY]         |
| :-----: | :---------------------- | :--------------- | :------------------- |
|   [1]   | `Color`                 | property surface | binds surface state  |
|   [2]   | `ColorChanged`          | change event     | emits color change   |
|   [3]   | `HexInputAlphaPosition` | layout property  | places alpha text    |
|   [4]   | `IsAlphaEnabled`        | alpha toggle     | toggles alpha input  |
|   [5]   | `IsColorPaletteVisible` | palette toggle   | toggles palette view |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia.Controls.ColorPicker`
- Owns: color editor controls
- Accept: color choices enter typed editors
- Reject: custom color picker forks

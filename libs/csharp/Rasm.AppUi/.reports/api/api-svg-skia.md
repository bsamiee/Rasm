# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` supplies Avalonia SVG controls backed by Skia rendering.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia`
- package: `Svg.Controls.Skia.Avalonia`
- assembly: `Svg.Controls.Skia.Avalonia`
- namespace: `Svg.Controls.Skia.Avalonia`
- asset: runtime library
- rail: assets

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SVG family
- rail: assets

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE]    | [CAPABILITY]            |
| :-----: | :---------- | :---------------- | :---------------------- |
|   [1]   | `Svg`       | SVG control       | anchors assets contract |
|   [2]   | `SvgImage`  | drawing surface   | draws visual evidence   |
|   [3]   | `SvgSource` | SVG source        | anchors assets contract |
|   [4]   | `SKSvg`     | Skia SVG document | anchors assets contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SVG operations
- rail: assets

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]        |
| :-----: | :----------------- | :--------------- | :------------------ |
|   [1]   | `Path`             | property surface | binds surface state |
|   [2]   | `Source`           | property surface | binds surface state |
|   [3]   | `Stretch`          | layout property  | sets SVG scaling    |
|   [4]   | `InvalidateVisual` | rendering call   | renders evidence    |
|   [5]   | `Load`             | load method      | loads asset content |
|   [6]   | `Draw`             | draw method      | renders SVG content |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Svg.Controls.Skia.Avalonia`
- Owns: SVG asset rendering
- Accept: SVG assets enter asset rail
- Reject: bitmap-only asset policy


# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` supplies Avalonia chart controls and Skia-backed chart series.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`
- package: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia`
- namespace: `LiveChartsCore.SkiaSharpView.Avalonia`
- asset: runtime library
- rail: charts

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart family
- rail: charts

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]    | [CAPABILITY]             |
| :-----: | :------------------------ | :---------------- | :----------------------- |
|   [1]   | `CartesianChart`          | Avalonia control  | renders Cartesian charts |
|   [2]   | `PieChart`                | Avalonia control  | renders pie charts       |
|   [3]   | `PolarChart`              | Avalonia control  | renders polar charts     |
|   [4]   | `XamlAxis`                | XAML axis         | binds axis state         |
|   [5]   | `XamlLineSeries`          | XAML series       | binds line series        |
|   [6]   | `XamlColumnSeries`        | XAML series       | binds column series      |
|   [7]   | `XamlPieSeries`           | XAML series       | binds pie series         |
|   [8]   | `SourceGenCartesianChart` | generated control | exposes chart properties |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chart operations
- rail: charts

| [INDEX] | [SURFACE]         | [CALL_SHAPE]     | [CAPABILITY]           |
| :-----: | :---------------- | :--------------- | :--------------------- |
|   [1]   | `Series`          | property surface | binds surface state    |
|   [2]   | `XAxes`           | property surface | binds surface state    |
|   [3]   | `YAxes`           | property surface | binds surface state    |
|   [4]   | `TooltipPosition` | member surface   | drives charts behavior |
|   [5]   | `LegendPosition`  | member surface   | drives charts behavior |
|   [6]   | `DataLabelsPaint` | member surface   | drives charts behavior |
|   [7]   | `GeometrySize`    | member surface   | drives charts behavior |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: retained charts
- Accept: chart intent maps to series rows
- Reject: hand-drawn chart controls

# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` supplies Avalonia chart controls, generated chart properties, axes, series, gauges, visual elements, and Skia paint extensions.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`
- package: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsGeneratedCode`
- namespace: `LiveChartsCore.SkiaSharpView.Avalonia`
- namespace: `LiveChartsGeneratedCode`
- asset: runtime library
- rail: charts

## [2]-[PUBLIC_TYPES]

[CHART_CONTROLS]: chart and canvas controls
- rail: charts

| [INDEX] | [SYMBOL]                  | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|   [1]   | `CartesianChart`          | Cartesian chart |
|   [2]   | `PieChart`                | pie chart       |
|   [3]   | `PolarChart`              | polar chart     |
|   [4]   | `GeoMap`                  | map chart       |
|   [5]   | `MotionCanvas`            | drawing canvas  |
|   [6]   | `SourceGenChart`          | generated base  |
|   [7]   | `SourceGenCartesianChart` | generated chart |
|   [8]   | `SourceGenPieChart`       | generated pie   |
|   [9]   | `SourceGenPolarChart`     | generated polar |
|  [10]   | `SourceGenMapChart`       | generated map   |

[AXIS_AND_SECTION_TYPES]: axes, sections, and visual collections
- rail: charts

| [INDEX] | [SYMBOL]              | [RAIL]                |
| :-----: | :-------------------- | :-------------------- |
|   [1]   | `BaseXamlAxis<T>`     | axis base             |
|   [2]   | `XamlAxis`            | numeric axis          |
|   [3]   | `XamlDateTimeAxis`    | date-time axis        |
|   [4]   | `XamlTimeSpanAxis`    | time-span axis        |
|   [5]   | `XamlLogarithmicAxis` | logarithmic axis      |
|   [6]   | `XamlPolarAxis`       | polar axis            |
|   [7]   | `AxesCollection`      | axis collection       |
|   [8]   | `PolarAxesCollection` | polar axis collection |
|   [9]   | `SectionsCollection`  | section collection    |
|  [10]   | `VisualsCollection`   | visual collection     |

[SERIES_TYPES]: XAML series families
- rail: charts

| [INDEX] | [SYMBOL]                  | [RAIL]         |
| :-----: | :------------------------ | :------------- |
|   [1]   | `XamlSeries`              | series base    |
|   [2]   | `SeriesCollection`        | series list    |
|   [3]   | `XamlColumnSeries`        | column series  |
|   [4]   | `XamlRowSeries`           | row series     |
|   [5]   | `XamlLineSeries`          | line series    |
|   [6]   | `XamlStepLineSeries`      | step series    |
|   [7]   | `XamlScatterSeries`       | scatter series |
|   [8]   | `XamlCandlesticksSeries`  | candle series  |
|   [9]   | `XamlBoxSeries`           | box series     |
|  [10]   | `XamlHeatSeries`          | heat series    |
|  [11]   | `XamlPieSeries`           | pie series     |
|  [12]   | `XamlPolarLineSeries`     | polar series   |
|  [13]   | `XamlStackedAreaSeries`   | stacked area   |
|  [14]   | `XamlStackedColumnSeries` | stacked column |

[GAUGE_AND_EXTENSION_TYPES]: gauges and XAML value extensions
- rail: charts

| [INDEX] | [SYMBOL]                       | [RAIL]           |
| :-----: | :----------------------------- | :--------------- |
|   [1]   | `XamlGaugeSeries`              | gauge series     |
|   [2]   | `XamlGaugeBackgroundSeries`    | gauge background |
|   [3]   | `XamlAngularGaugeSeries`       | angular gauge    |
|   [4]   | `XamlNeedle`                   | gauge needle     |
|   [5]   | `XamlAngularTicks`             | gauge ticks      |
|   [6]   | `SolidColorPaintExtension`     | solid paint      |
|   [7]   | `LinearGradientPaintExtension` | linear paint     |
|   [8]   | `RadialGradientPaintExtension` | radial paint     |
|   [9]   | `ColorExtension`               | color value      |
|  [10]   | `ValuesExtension`              | values value     |

[GEO_TYPES]: map chart binding surfaces
- rail: charts

| [INDEX] | [SYMBOL]        | [RAIL]            |
| :-----: | :-------------- | :---------------- |
|   [1]   | `IGeoMapView`   | map view contract |
|   [2]   | `DrawnMap`      | active map record |
|   [3]   | `MapProjection` | projection mode   |

## [3]-[ENTRYPOINTS]

[CHART_ENTRYPOINTS]: chart control properties
- rail: charts

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]            | [RAIL]          |
| :-----: | :--------------- | :------------------------ | :-------------- |
|   [1]   | `Series`         | `SourceGenChart`          | series input    |
|   [2]   | `SeriesSource`   | `SourceGenChart`          | source input    |
|   [3]   | `SeriesTemplate` | `SourceGenChart`          | series template |
|   [4]   | `VisualElements` | `SourceGenChart`          | overlay visuals |
|   [5]   | `XAxes`          | `SourceGenCartesianChart` | X axes          |
|   [6]   | `YAxes`          | `SourceGenCartesianChart` | Y axes          |
|   [7]   | `Sections`       | `SourceGenCartesianChart` | chart sections  |
|   [8]   | `ZoomMode`       | `SourceGenCartesianChart` | zoom mode       |
|   [9]   | `SyncContext`    | generated charts          | sync scope      |

[PRESENTATION_ENTRYPOINTS]: legend, tooltip, paint, and animation properties
- rail: charts

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]            | [RAIL]           |
| :-----: | :-------------------------- | :------------------------ | :--------------- |
|   [1]   | `Legend`                    | `SourceGenChart`          | legend object    |
|   [2]   | `LegendPosition`            | `SourceGenChart`          | legend position  |
|   [3]   | `LegendTextPaint`           | `SourceGenChart`          | legend text      |
|   [4]   | `Tooltip`                   | `SourceGenChart`          | tooltip object   |
|   [5]   | `TooltipPosition`           | `SourceGenChart`          | tooltip position |
|   [6]   | `TooltipTextPaint`          | `SourceGenChart`          | tooltip text     |
|   [7]   | `DrawMargin`                | `SourceGenChart`          | draw bounds      |
|   [8]   | `DrawMarginFrame`           | `SourceGenCartesianChart` | draw frame       |
|   [9]   | `AnimationsSpeed`           | `SourceGenChart`          | animation timing |
|  [10]   | `VisualElementsPointerDown` | `SourceGenChart`          | visual event     |

[SERIES_ENTRYPOINTS]: series and gauge properties
- rail: charts

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]    | [RAIL]           |
| :-----: | :----------------------- | :---------------- | :--------------- |
|   [1]   | `WrappedSeries`          | `XamlSeries`      | runtime series   |
|   [2]   | `ValuesMap`              | `XamlSeries`      | value projection |
|   [3]   | `AdditionalVisualStates` | `XamlSeries`      | visual states    |
|   [4]   | `GaugeValue`             | `XamlGaugeSeries` | gauge value      |
|   [5]   | `Invalidate`             | `XamlGaugeSeries` | series refresh   |

[GEO_ENTRYPOINTS]: map chart binding members
- rail: charts

| [INDEX] | [SURFACE]       | [SURFACE_ROOT]      | [RAIL]            |
| :-----: | :-------------- | :------------------ | :---------------- |
|   [1]   | `ActiveMap`     | `SourceGenMapChart` | active map source |
|   [2]   | `MapProjection` | `SourceGenMapChart` | projection mode   |
|   [3]   | `Series`        | `SourceGenMapChart` | geo series        |
|   [4]   | `Stroke`        | `SourceGenMapChart` | land stroke paint |
|   [5]   | `Fill`          | `SourceGenMapChart` | land fill paint   |

## [4]-[IMPLEMENTATION_LAW]

[CHART_LAW]:
- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: retained charts, generated chart properties, XAML axes, series, gauges, visual elements, and Skia paints
- Accept: chart intent maps to explicit series, axes, sections, visuals, legends, tooltips, and animation state
- Reject: hand-drawn chart controls

[VISUALIZATION_LAW]:
- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: dense product charts for panels, companion windows, sidecars, diagnostics, support views, and downstream shells
- Accept: chart state remains data-driven and composable through one chart rail
- Reject: one-off drawing code for chart semantics

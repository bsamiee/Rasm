# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` supplies Avalonia chart controls, generated chart properties, axes, series, gauges, visual elements, and Skia paint extensions.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`
- package: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsGeneratedCode`
- namespace: `LiveChartsCore.SkiaSharpView.Avalonia`
- namespace: `LiveChartsGeneratedCode`
- asset: runtime library
- rail: charts

## [02]-[PUBLIC_TYPES]

[CHART_CONTROLS]: chart and canvas controls
- rail: charts

| [INDEX] | [SYMBOL]                  | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|  [01]   | `CartesianChart`          | Cartesian chart |
|  [02]   | `PieChart`                | pie chart       |
|  [03]   | `PolarChart`              | polar chart     |
|  [04]   | `GeoMap`                  | map chart       |
|  [05]   | `MotionCanvas`            | drawing canvas  |
|  [06]   | `SourceGenChart`          | generated base  |
|  [07]   | `SourceGenCartesianChart` | generated chart |
|  [08]   | `SourceGenPieChart`       | generated pie   |
|  [09]   | `SourceGenPolarChart`     | generated polar |
|  [10]   | `SourceGenMapChart`       | generated map   |

[AXIS_AND_SECTION_TYPES]: axes, sections, and visual collections
- rail: charts

| [INDEX] | [SYMBOL]              | [RAIL]                |
| :-----: | :-------------------- | :-------------------- |
|  [01]   | `BaseXamlAxis<T>`     | axis base             |
|  [02]   | `XamlAxis`            | numeric axis          |
|  [03]   | `XamlDateTimeAxis`    | date-time axis        |
|  [04]   | `XamlTimeSpanAxis`    | time-span axis        |
|  [05]   | `XamlLogarithmicAxis` | logarithmic axis      |
|  [06]   | `XamlPolarAxis`       | polar axis            |
|  [07]   | `AxesCollection`      | axis collection       |
|  [08]   | `PolarAxesCollection` | polar axis collection |
|  [09]   | `SectionsCollection`  | section collection    |
|  [10]   | `VisualsCollection`   | visual collection     |

[SERIES_TYPES]: XAML series families
- rail: charts

| [INDEX] | [SYMBOL]                  | [RAIL]         |
| :-----: | :------------------------ | :------------- |
|  [01]   | `XamlSeries`              | series base    |
|  [02]   | `SeriesCollection`        | series list    |
|  [03]   | `XamlColumnSeries`        | column series  |
|  [04]   | `XamlRowSeries`           | row series     |
|  [05]   | `XamlLineSeries`          | line series    |
|  [06]   | `XamlStepLineSeries`      | step series    |
|  [07]   | `XamlScatterSeries`       | scatter series |
|  [08]   | `XamlCandlesticksSeries`  | candle series  |
|  [09]   | `XamlBoxSeries`           | box series     |
|  [10]   | `XamlHeatSeries`          | heat series    |
|  [11]   | `XamlPieSeries`           | pie series     |
|  [12]   | `XamlPolarLineSeries`     | polar series   |
|  [13]   | `XamlStackedAreaSeries`   | stacked area   |
|  [14]   | `XamlStackedColumnSeries` | stacked column |

[GAUGE_AND_EXTENSION_TYPES]: gauges and XAML value extensions
- rail: charts

| [INDEX] | [SYMBOL]                       | [RAIL]           |
| :-----: | :----------------------------- | :--------------- |
|  [01]   | `XamlGaugeSeries`              | gauge series     |
|  [02]   | `XamlGaugeBackgroundSeries`    | gauge background |
|  [03]   | `XamlAngularGaugeSeries`       | angular gauge    |
|  [04]   | `XamlNeedle`                   | gauge needle     |
|  [05]   | `XamlAngularTicks`             | gauge ticks      |
|  [06]   | `SolidColorPaintExtension`     | solid paint      |
|  [07]   | `LinearGradientPaintExtension` | linear paint     |
|  [08]   | `RadialGradientPaintExtension` | radial paint     |
|  [09]   | `ColorExtension`               | color value      |
|  [10]   | `ValuesExtension`              | values value     |

[GEO_TYPES]: map chart binding surfaces
- rail: charts

| [INDEX] | [SYMBOL]        | [RAIL]            |
| :-----: | :-------------- | :---------------- |
|  [01]   | `IGeoMapView`   | map view contract |
|  [02]   | `DrawnMap`      | active map record |
|  [03]   | `MapProjection` | projection mode   |

## [03]-[ENTRYPOINTS]

[CHART_ENTRYPOINTS]: chart control properties
- rail: charts

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]            | [RAIL]          |
| :-----: | :--------------- | :------------------------ | :-------------- |
|  [01]   | `Series`         | `SourceGenChart`          | series input    |
|  [02]   | `SeriesSource`   | `SourceGenChart`          | source input    |
|  [03]   | `SeriesTemplate` | `SourceGenChart`          | series template |
|  [04]   | `VisualElements` | `SourceGenChart`          | overlay visuals |
|  [05]   | `XAxes`          | `SourceGenCartesianChart` | X axes          |
|  [06]   | `YAxes`          | `SourceGenCartesianChart` | Y axes          |
|  [07]   | `Sections`       | `SourceGenCartesianChart` | chart sections  |
|  [08]   | `ZoomMode`       | `SourceGenCartesianChart` | zoom mode       |
|  [09]   | `SyncContext`    | generated charts          | sync scope      |

[PRESENTATION_ENTRYPOINTS]: legend, tooltip, paint, and animation properties
- rail: charts

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]            | [RAIL]           |
| :-----: | :-------------------------- | :------------------------ | :--------------- |
|  [01]   | `Legend`                    | `SourceGenChart`          | legend object    |
|  [02]   | `LegendPosition`            | `SourceGenChart`          | legend position  |
|  [03]   | `LegendTextPaint`           | `SourceGenChart`          | legend text      |
|  [04]   | `Tooltip`                   | `SourceGenChart`          | tooltip object   |
|  [05]   | `TooltipPosition`           | `SourceGenChart`          | tooltip position |
|  [06]   | `TooltipTextPaint`          | `SourceGenChart`          | tooltip text     |
|  [07]   | `DrawMargin`                | `SourceGenChart`          | draw bounds      |
|  [08]   | `DrawMarginFrame`           | `SourceGenCartesianChart` | draw frame       |
|  [09]   | `AnimationsSpeed`           | `SourceGenChart`          | animation timing |
|  [10]   | `VisualElementsPointerDown` | `SourceGenChart`          | visual event     |

[SERIES_ENTRYPOINTS]: series and gauge properties
- rail: charts

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]    | [RAIL]           |
| :-----: | :----------------------- | :---------------- | :--------------- |
|  [01]   | `WrappedSeries`          | `XamlSeries`      | runtime series   |
|  [02]   | `ValuesMap`              | `XamlSeries`      | value projection |
|  [03]   | `AdditionalVisualStates` | `XamlSeries`      | visual states    |
|  [04]   | `GaugeValue`             | `XamlGaugeSeries` | gauge value      |
|  [05]   | `Invalidate`             | `XamlGaugeSeries` | series refresh   |

[GEO_ENTRYPOINTS]: map chart binding members
- rail: charts
- surface-root: `SourceGenMapChart`

| [INDEX] | [SURFACE]       | [RAIL]            |
| :-----: | :-------------- | :---------------- |
|  [01]   | `ActiveMap`     | active map source |
|  [02]   | `MapProjection` | projection mode   |
|  [03]   | `Series`        | geo series        |
|  [04]   | `Stroke`        | land stroke paint |
|  [05]   | `Fill`          | land fill paint   |

## [04]-[IMPLEMENTATION_LAW]

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

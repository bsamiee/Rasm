# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` supplies the Avalonia binding layer of LiveCharts2 — chart `UserControl`s, source-generated UI properties, XAML axes/series/gauges/sections/visual elements, and Skia paint `MarkupExtension`s. The chart math, series model, and Skia drawing runtime live in the transitive `LiveChartsCore` and `LiveChartsCore.SkiaSharpView` assemblies; this package is the thin Avalonia-control + XAML-markup wrapper over them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`

- package: `LiveChartsCore.SkiaSharpView.Avalonia`
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia` (the single shipped assembly; `LiveChartsGeneratedCode` is a NAMESPACE inside it, not a separate assembly)
- namespace: `LiveChartsCore.SkiaSharpView.Avalonia` — `Xaml*`, `*Collection`, `*Extension`, and the public `CartesianChart`/`PieChart`/`PolarChart`/`GeoMap`/`MotionCanvas`
- namespace: `LiveChartsGeneratedCode` — the source-generated `SourceGen*` control bases the public charts derive from
- asset: runtime library
- build-floor: ships `lib/net8.0` (top TFM; no `net10.0`/`net9.0`), so the `net10.0` consumer binds `lib/net8.0` — the documented surface
- dependency: transitively requires `LiveChartsCore.SkiaSharpView` (Skia draw + series model) and `Avalonia`/`Avalonia.Skia` ≥ (consumer runs Avalonia 12, forward-compatible)
- rail: charts

## [02]-[PUBLIC_TYPES]

[CHART_CONTROLS]: chart and canvas controls

- rail: charts
- public charts (namespace `LiveChartsCore.SkiaSharpView.Avalonia`) derive from the source-generated `SourceGen*` bases (namespace `LiveChartsGeneratedCode`); bind to the public type and read the generated properties off the base

| [INDEX] | [SYMBOL]                  | [ROLE]                   |
| :-----: | :------------------------ | :----------------------- |
|  [01]   | `CartesianChart`          | Cartesian chart          |
|  [02]   | `PieChart`                | pie chart                |
|  [03]   | `PolarChart`              | polar chart              |
|  [04]   | `GeoMap`                  | map chart                |
|  [05]   | `MotionCanvas`            | raw Skia drawing canvas  |
|  [06]   | `SourceGenChart`          | abstract generated base  |
|  [07]   | `SourceGenCartesianChart` | generated Cartesian base |
|  [08]   | `SourceGenPieChart`       | generated pie base       |
|  [09]   | `SourceGenPolarChart`     | generated polar base     |
|  [10]   | `SourceGenMapChart`       | generated map base       |

[CONTROL_BASES]: The public Avalonia controls derive respectively from `SourceGenCartesianChart`, `SourceGenPieChart`, `SourceGenPolarChart`, and `SourceGenMapChart`; `MotionCanvas` derives from `UserControl`. In `LiveChartsGeneratedCode`, `SourceGenChart` derives from `UserControl` and implements `IChartView` plus `ICustomHitTest`, while `SourceGenMapChart` implements `IGeoMapView`.

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
- every `Xaml*Series` is generic `<TModel, TVisual, TLabel>` (`TVisual : *Geometry`, `TLabel : BaseLabelGeometry`) implementing `IXamlWrapper<…Series<…>>` over the `LiveChartsCore` runtime series; non-generic and single-arg convenience subclasses default `TVisual`/`TLabel` (e.g. `XamlColumnSeries<TModel>`)

| [INDEX] | [SYMBOL]                    | [WRAPS_LIVECHARTSCORE]          | [RAIL]             |
| :-----: | :-------------------------- | :------------------------------ | :----------------- |
|  [01]   | `XamlSeries`                | `abstract Control` series base  | series base        |
|  [02]   | `SeriesCollection`          | `ObservableCollection<ISeries>` | series list        |
|  [03]   | `XamlColumnSeries`          | `ColumnSeries`                  | column series      |
|  [04]   | `XamlRowSeries`             | `RowSeries`                     | row series         |
|  [05]   | `XamlLineSeries`            | `LineSeries`                    | line series        |
|  [06]   | `XamlStepLineSeries`        | `StepLineSeries`                | step series        |
|  [07]   | `XamlScatterSeries`         | `ScatterSeries`                 | scatter series     |
|  [08]   | `XamlCandlesticksSeries`    | `CandlesticksSeries`            | financial series   |
|  [09]   | `XamlBoxSeries`             | `BoxSeries`                     | box/whisker series |
|  [10]   | `XamlHeatSeries`            | `HeatSeries`                    | heat series        |
|  [11]   | `XamlPieSeries`             | `PieSeries`                     | pie series         |
|  [12]   | `XamlPolarLineSeries`       | `PolarLineSeries`               | polar series       |
|  [13]   | `XamlStackedAreaSeries`     | `StackedAreaSeries`             | stacked area       |
|  [14]   | `XamlStackedStepAreaSeries` | `StackedStepAreaSeries`         | stacked step area  |
|  [15]   | `XamlStackedColumnSeries`   | `StackedColumnSeries`           | stacked column     |
|  [16]   | `XamlStackedRowSeries`      | `StackedRowSeries`              | stacked row        |

[GAUGE_TYPES]: gauge series and visual elements

- rail: charts

| [INDEX] | [SYMBOL]                    | [WRAPS]                               | [RAIL]                |
| :-----: | :-------------------------- | :------------------------------------ | :-------------------- |
|  [01]   | `XamlGaugeSeries<TV,TL>`    | `PieSeries<ObservableValue,TV,TL>`    | gauge series base     |
|  [02]   | `XamlGaugeBackgroundSeries` | `XamlGaugeSeries<DoughnutGeometry,…>` | gauge background ring |
|  [03]   | `XamlAngularGaugeSeries`    | `XamlGaugeSeries<DoughnutGeometry,…>` | angular gauge         |
|  [04]   | `XamlNeedle`                | `NeedleVisual`                        | gauge needle          |
|  [05]   | `XamlAngularTicks`          | `AngularTicksVisual`                  | gauge ticks           |
|  [06]   | `XamlDrawnLabelVisual`      | `DrawnLabelVisual`                    | free-floating label   |
|  [07]   | `XamlRectangularSection`    | `RectangularSection`                  | axis band / threshold |

[EXTENSION_TYPES]: XAML `MarkupExtension` value providers (namespace `LiveChartsCore.SkiaSharpView.Avalonia`)

- rail: charts
- all derive from `BaseExtension : MarkupExtension`; the paint trio derives from `BaseSkiaPaintExtention`

| [INDEX] | [SYMBOL]                                 | [RAIL]                              |
| :-----: | :--------------------------------------- | :---------------------------------- |
|  [01]   | `SolidColorPaintExtension`               | solid `SolidColorPaint`             |
|  [02]   | `LinearGradientPaintExtension`           | linear-gradient paint               |
|  [03]   | `RadialGradientPaintExtension`           | radial-gradient paint               |
|  [04]   | `DashedExtension`                        | dashed-stroke paint effect          |
|  [05]   | `ShadowExtension`                        | drop-shadow paint effect            |
|  [06]   | `FrameExtension`                         | `DrawMarginFrame` value             |
|  [07]   | `FromSharedAxesExtension`                | shared-axis pairing (`PairElement`) |
|  [08]   | `PaddingExtension` / `MarginExtension`   | `Padding` / margin value            |
|  [09]   | `PointExtension`                         | `LvcPoint` value                    |
|  [10]   | `ColorExtension` / `ColorArrayExtension` | `LvcColor` / color array            |
|  [11]   | `ValuesExtension`                        | inline series-values literal        |

[GEO_TYPES]: map chart binding surfaces (defined in transitive `LiveChartsCore.Geo`, bound through `GeoMap`/`SourceGenMapChart`)

- rail: charts

| [INDEX] | [SYMBOL]        | [OWNER_ASSEMBLY] | [RAIL]                                             |
| :-----: | :-------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `IGeoMapView`   | `LiveChartsCore` | map view contract (`SourceGenMapChart` implements) |
|  [02]   | `DrawnMap`      | `LiveChartsCore` | active map record (`ActiveMap` property type)      |
|  [03]   | `MapProjection` | `LiveChartsCore` | projection-mode enum                               |

## [03]-[ENTRYPOINTS]

[CHART_ENTRYPOINTS]: chart control properties

- rail: charts

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]            | [RAIL]                               |
| :-----: | :---------------- | :------------------------ | :----------------------------------- |
|  [01]   | `Series`          | `SourceGenChart`          | series input                         |
|  [02]   | `SeriesSource`    | `SourceGenChart`          | source input                         |
|  [03]   | `SeriesTemplate`  | `SourceGenChart`          | series template                      |
|  [04]   | `VisualElements`  | `SourceGenChart`          | overlay visuals                      |
|  [05]   | `Title`           | `SourceGenChart`          | chart title visual                   |
|  [06]   | `XAxes`           | `SourceGenCartesianChart` | X axes                               |
|  [07]   | `YAxes`           | `SourceGenCartesianChart` | Y axes                               |
|  [08]   | `Sections`        | `SourceGenCartesianChart` | chart sections                       |
|  [09]   | `ZoomMode`        | `SourceGenCartesianChart` | zoom mode                            |
|  [10]   | `FindingStrategy` | `SourceGenCartesianChart` | hover/hit-test point strategy        |
|  [11]   | `SyncContext`     | generated charts          | cross-chart sync scope (shared lock) |

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

[GENERATED_TOPOLOGY]:

- The package is a three-assembly stack: `LiveChartsCore` owns the chart math, the `ISeries`/`ICartesianAxis`/`IChartElement` model, and the Skia draw kernel (`MotionCanvas`, `DrawnMap`, `MapProjection` live here under `LiveChartsCore.Geo`); `LiveChartsCore.SkiaSharpView` owns the `SolidColorPaint`/`LinearGradientPaint` concretes; this package owns only the Avalonia `UserControl` + XAML-markup layer.
- Chart controls are source-generated: `SourceGen*` bases (namespace `LiveChartsGeneratedCode`) carry every chart property as a `UIProperty`/`AvaloniaProperty`, and the public `CartesianChart`/`PieChart`/`PolarChart`/`GeoMap` (namespace `LiveChartsCore.SkiaSharpView.Avalonia`) derive from them — bind to the public type, read the property off the generated base. The chart owns no `IChartView` reimplementation; it is the generated control.
- Series/axes are XAML wrappers over the core model: every `Xaml*Series<TModel,TVisual,TLabel>` implements `IXamlWrapper<…Series<…>>` and exposes its built runtime series via `WrappedSeries`; a code-path that needs the live `ISeries` reads `WrappedSeries`, while XAML declares the `Xaml*` shell. `ValuesMap`/`Values` projects the bound model collection into chart points.

[CHART_LAW]:

- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: retained Avalonia charts, source-generated chart properties, XAML axes/series/gauges/sections/visual elements, and Skia paint markup extensions
- Accept: chart intent maps to explicit series, axes, sections, visuals, legends, tooltips, and animation state through the generated property surface; paints declared via the `*PaintExtension` markup extensions
- Reject: hand-drawn chart controls; a reimplemented `IChartView`; mutating the bound values collection outside the live-data rail

[VISUALIZATION_LAW]:

- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: dense product charts for panels, companion windows, sidecars, diagnostics, support views, and downstream shells
- Accept: chart state remains data-driven and composable through one chart rail
- Reject: one-off drawing code for chart semantics

[STACKING]:

- Live data into series: a `DynamicData` `SourceCache.Connect().Transform(...).ToCollection()` (or a bound `ObservableCollectionExtended`) is the `ISeries.Values` source, so a chart redraws off the same change-set the grid and aggregate tiles read — the cache is the one source of truth, the chart a projection. `XamlSeries.ValuesMap`/`Values` binds that collection into chart points without a copy.
- Aggregate tiles share the rail: `DynamicData.Aggregation` (`Count`/`Sum`/`Maximum`/`Minimum`/`StdDev`) feeds gauge `XamlGaugeSeries.GaugeValue` and KPI labels from the same cache, so a dashboard tile and its chart never diverge.
- Skia stack alignment: this package binds against `SkiaSharp` 3.119.x (the central-pinned runtime family shared with `Avalonia.Skia`, `Svg.Skia`, and `SkiaSharp.HarfBuzz`); paints are declared as `*PaintExtension` markup, never hand-built `SKPaint`, so theme colour tokens flow through `ColorExtension`/`SolidColorPaintExtension` into one paint family.
- Cross-chart sync: shared `SyncContext` (a common lock object) ties multiple charts' pointer/zoom/animation passes onto one frame so a synchronized dashboard pans together.

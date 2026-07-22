# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` binds LiveCharts2 to Avalonia: retained chart `UserControl`s, source-generated chart properties, and XAML axes, series, gauges, sections, and Skia paint markup extensions. Every `Xaml*` shell wraps a `LiveChartsCore` runtime type, so a chart is a data-driven projection over one series model, never a hand-drawn surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`
- package: `LiveChartsCore.SkiaSharpView.Avalonia` (MIT)
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia`
- namespaces: `LiveChartsCore.SkiaSharpView.Avalonia` (public charts, `Xaml*`, `*Collection`, `*Extension`), `LiveChartsGeneratedCode` (source-generated `SourceGen*` bases)
- target: `lib/net8.0`
- depends: `LiveChartsCore.SkiaSharpView`, `Avalonia`, `Avalonia.Skia`
- rail: charts

## [02]-[PUBLIC_TYPES]

[CHART_CONTROLS]: chart and canvas controls

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------ | :------------ | :---------------------------------- |
|  [01]   | `CartesianChart`          | class         | cartesian XY chart                  |
|  [02]   | `PieChart`                | class         | pie / doughnut chart                |
|  [03]   | `PolarChart`              | class         | polar chart                         |
|  [04]   | `GeoMap`                  | class         | geographic map chart                |
|  [05]   | `MotionCanvas`            | class         | raw Skia drawing canvas             |
|  [06]   | `SourceGenChart`          | abstract      | generated chart base (`IChartView`) |
|  [07]   | `SourceGenCartesianChart` | class         | generated cartesian base            |
|  [08]   | `SourceGenPieChart`       | class         | generated pie base                  |
|  [09]   | `SourceGenPolarChart`     | class         | generated polar base                |
|  [10]   | `SourceGenMapChart`       | class         | generated map base (`IGeoMapView`)  |

[AXIS_AND_SECTION_TYPES]: axes, sections, and visual collections

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :-------------------- | :------------ | :------------------------ |
|  [01]   | `BaseXamlAxis<T>`     | abstract      | axis wrapper base         |
|  [02]   | `XamlAxis`            | class         | numeric axis              |
|  [03]   | `XamlDateTimeAxis`    | class         | date-time axis            |
|  [04]   | `XamlTimeSpanAxis`    | class         | time-span axis            |
|  [05]   | `XamlLogarithmicAxis` | class         | logarithmic axis          |
|  [06]   | `XamlPolarAxis`       | class         | polar axis                |
|  [07]   | `AxesCollection`      | collection    | cartesian axis collection |
|  [08]   | `PolarAxesCollection` | collection    | polar axis collection     |
|  [09]   | `SectionsCollection`  | collection    | section collection        |
|  [10]   | `VisualsCollection`   | collection    | visual collection         |

[SERIES_TYPES]: XAML series wrappers; every `Xaml<Kind>Series<TModel,TVisual,TLabel>` implements `IXamlWrapper<…Series<…>>` over the same-named `LiveChartsCore` runtime series, and single-arg subclasses default `TVisual`/`TLabel`.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :-------------------------- | :------------ | :--------------------- |
|  [01]   | `XamlSeries`                | abstract      | series wrapper base    |
|  [02]   | `SeriesCollection`          | collection    | `ISeries` collection   |
|  [03]   | `XamlColumnSeries`          | class         | column bars            |
|  [04]   | `XamlRowSeries`             | class         | horizontal bars        |
|  [05]   | `XamlLineSeries`            | class         | line series            |
|  [06]   | `XamlStepLineSeries`        | class         | step-line series       |
|  [07]   | `XamlScatterSeries`         | class         | scatter series         |
|  [08]   | `XamlCandlesticksSeries`    | class         | financial candlesticks |
|  [09]   | `XamlBoxSeries`             | class         | box / whisker series   |
|  [10]   | `XamlHeatSeries`            | class         | heat series            |
|  [11]   | `XamlPieSeries`             | class         | pie series             |
|  [12]   | `XamlPolarLineSeries`       | class         | polar line series      |
|  [13]   | `XamlStackedAreaSeries`     | class         | stacked area           |
|  [14]   | `XamlStackedStepAreaSeries` | class         | stacked step area      |
|  [15]   | `XamlStackedColumnSeries`   | class         | stacked column         |
|  [16]   | `XamlStackedRowSeries`      | class         | stacked row            |

[GAUGE_AND_VISUAL_TYPES]: gauge series and free visual elements; each wraps the drawn `LiveChartsCore` visual it names.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `XamlGaugeSeries<TV,TL>`    | class         | gauge base over `PieSeries<ObservableValue,…>` |
|  [02]   | `XamlGaugeBackgroundSeries` | class         | gauge background ring                          |
|  [03]   | `XamlAngularGaugeSeries`    | class         | angular gauge                                  |
|  [04]   | `XamlNeedle`                | class         | gauge needle (`NeedleVisual`)                  |
|  [05]   | `XamlAngularTicks`          | class         | gauge ticks (`AngularTicksVisual`)             |
|  [06]   | `XamlDrawnLabelVisual`      | class         | free label (`DrawnLabelVisual`)                |
|  [07]   | `XamlRectangularSection`    | class         | axis band (`RectangularSection`)               |

[EXTENSION_TYPES]: XAML `MarkupExtension` value providers; all derive from `BaseExtension : MarkupExtension`, the paint trio from `BaseSkiaPaintExtention`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :----------------------------- | :------------ | :---------------------------------- |
|  [01]   | `SolidColorPaintExtension`     | class         | `SolidColorPaint` value             |
|  [02]   | `LinearGradientPaintExtension` | class         | linear-gradient paint               |
|  [03]   | `RadialGradientPaintExtension` | class         | radial-gradient paint               |
|  [04]   | `DashedExtension`              | class         | dashed-stroke effect                |
|  [05]   | `ShadowExtension`              | class         | drop-shadow effect                  |
|  [06]   | `FrameExtension`               | class         | `DrawMarginFrame` value             |
|  [07]   | `FromSharedAxesExtension`      | class         | shared-axis pairing (`PairElement`) |
|  [08]   | `PaddingExtension`             | class         | `Padding` value                     |
|  [09]   | `MarginExtension`              | class         | margin value                        |
|  [10]   | `PointExtension`               | class         | `LvcPoint` value                    |
|  [11]   | `ColorExtension`               | class         | `LvcColor` value                    |
|  [12]   | `ColorArrayExtension`          | class         | `LvcColor` array                    |
|  [13]   | `ValuesExtension`              | class         | inline series-values literal        |

[GEO_TYPES]: map binding surfaces in transitive `LiveChartsCore.Geo`, bound through `GeoMap`/`SourceGenMapChart`.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :-------------- | :------------ | :------------------------------ |
|  [01]   | `IGeoMapView`   | interface     | map view contract               |
|  [02]   | `DrawnMap`      | class         | active map record (`ActiveMap`) |
|  [03]   | `MapProjection` | enum          | projection mode                 |

## [03]-[ENTRYPOINTS]

[CHART_ENTRYPOINTS]: properties on `SourceGenChart`, exposed by every public chart

| [INDEX] | [SURFACE]                   | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------- | :------- | :---------------------- |
|  [01]   | `Series`                    | property | series input            |
|  [02]   | `SeriesSource`              | property | source-collection input |
|  [03]   | `SeriesTemplate`            | property | series template         |
|  [04]   | `VisualElements`            | property | overlay visuals         |
|  [05]   | `Title`                     | property | chart title visual      |
|  [06]   | `Legend`                    | property | legend object           |
|  [07]   | `LegendPosition`            | property | legend position         |
|  [08]   | `LegendTextPaint`           | property | legend text paint       |
|  [09]   | `Tooltip`                   | property | tooltip object          |
|  [10]   | `TooltipPosition`           | property | tooltip position        |
|  [11]   | `TooltipTextPaint`          | property | tooltip text paint      |
|  [12]   | `DrawMargin`                | property | draw bounds             |
|  [13]   | `AnimationsSpeed`           | property | animation timing        |
|  [14]   | `SyncContext`               | property | cross-chart sync lock   |
|  [15]   | `VisualElementsPointerDown` | event    | visual pointer-down     |

[CARTESIAN_ENTRYPOINTS]: additional properties on `SourceGenCartesianChart`

| [INDEX] | [SURFACE]         | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------- | :------- | :---------------------------- |
|  [01]   | `XAxes`           | property | X axes                        |
|  [02]   | `YAxes`           | property | Y axes                        |
|  [03]   | `Sections`        | property | chart sections                |
|  [04]   | `ZoomMode`        | property | zoom mode                     |
|  [05]   | `FindingStrategy` | property | hover/hit-test point strategy |
|  [06]   | `DrawMarginFrame` | property | draw frame                    |

[SERIES_ENTRYPOINTS]: series and gauge members on `XamlSeries` and `XamlGaugeSeries`

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------- | :------- | :------------------------ |
|  [01]   | `XamlSeries.WrappedSeries`          | property | built runtime `ISeries`   |
|  [02]   | `XamlSeries.ValuesMap`              | property | model-to-point projection |
|  [03]   | `XamlSeries.AdditionalVisualStates` | property | extra visual states       |
|  [04]   | `XamlGaugeSeries.GaugeValue`        | property | gauge value               |
|  [05]   | `XamlGaugeSeries.Invalidate(Chart)` | method   | series refresh            |

[GEO_ENTRYPOINTS]: properties on `SourceGenMapChart`

| [INDEX] | [SURFACE]       | [SHAPE]  | [CAPABILITY]      |
| :-----: | :-------------- | :------- | :---------------- |
|  [01]   | `ActiveMap`     | property | active map source |
|  [02]   | `MapProjection` | property | projection mode   |
|  [03]   | `Series`        | property | geo series        |
|  [04]   | `Stroke`        | property | land stroke paint |
|  [05]   | `Fill`          | property | land fill paint   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three assemblies stack behind one Avalonia surface: `LiveChartsCore` owns the chart math, the `ISeries`/`ICartesianAxis`/`IChartElement` model, and the Skia draw kernel (`LiveChartsCore.Geo` carries `DrawnMap`/`MapProjection`/`IGeoMapView`); `LiveChartsCore.SkiaSharpView` owns the `SolidColorPaint`/`LinearGradientPaint` concretes; this package owns the `UserControl` and XAML-markup layer over them.
- Chart controls are source-generated: each `SourceGen*` base carries every chart property as an `AvaloniaProperty`, and each public chart derives from its base (`MotionCanvas` derives from `UserControl` directly); bind the public control and read the property off the generated base, never reimplement `IChartView`.
- Series and axes are XAML shells over the runtime model: every `Xaml*Series` exposes its built `ISeries` through `WrappedSeries`, and `ValuesMap`/`Values` projects the bound model collection into chart points; a code path needing the live series reads `WrappedSeries` while XAML declares the shell.

[STACKING]:
- `api-dynamicdata.md` (`DynamicData`): a `SourceCache.Connect().Transform(…).ToCollection()` or a bound `ObservableCollectionExtended` is the `ISeries.Values` source, so a chart redraws off the same change-set the grid and tiles read, and `XamlSeries.ValuesMap`/`Values` binds that collection into chart points without a copy.
- `api-dynamicdata.md` (`DynamicData.Aggregation`): `Count`/`Sum`/`Maximum`/`Minimum`/`StdDev` feed gauge `XamlGaugeSeries.GaugeValue` and KPI labels off the same cache, so a tile and its chart never diverge.
- `api-skiasharp.md` (`SkiaSharp`): paints declare as `*PaintExtension` markup over the shared Skia paint family, so theme colour tokens flow through `ColorExtension`/`SolidColorPaintExtension` into one paint family, never a hand-built `SKPaint`.
- within-lib: one shared `SyncContext` lock ties multiple charts' pointer, zoom, and animation passes onto one frame, so a synchronized dashboard pans together.

[LOCAL_ADMISSION]:
- A chart in the AppUi shell is admitted only as an `Xaml*` control whose `ISeries.Values` binds a `DynamicData` projection; a bespoke Skia surface drawing chart semantics is rejected.

[RAIL_LAW]:
- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: the product chart rail — retained Avalonia charts, source-generated chart properties, XAML axes, series, gauges, sections, visual elements, and Skia paint markup extensions across panels, companion windows, sidecars, and diagnostics.
- Accept: chart intent maps to explicit series, axes, sections, visuals, legends, tooltips, and animation state through the generated property surface, paints declared as `*PaintExtension` markup, and state stays data-driven off one chart rail.
- Reject: hand-drawn chart controls, a reimplemented `IChartView`, one-off drawing code for chart semantics, and mutating the bound values collection outside the live-data rail.

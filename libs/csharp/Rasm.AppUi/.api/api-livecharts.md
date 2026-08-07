# [RASM_APPUI_API_LIVECHARTS]

`LiveChartsCore.SkiaSharpView.Avalonia` binds LiveCharts2 to Avalonia: retained chart `UserControl`s, source-generated chart properties, and XAML axes, series, gauges, sections, and Skia paint markup extensions. Every `Xaml*` shell implements the `LiveChartsCore` contract it declares, so a chart projects one data-driven series model onto a Skia canvas the process-wide theme rail styles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LiveChartsCore.SkiaSharpView.Avalonia`
- package: `LiveChartsCore.SkiaSharpView.Avalonia` (MIT)
- assembly: `LiveChartsCore.SkiaSharpView.Avalonia`
- namespaces: `LiveChartsCore.SkiaSharpView.Avalonia` (public charts, `Xaml*`, `*Collection`, `*Extension`), `LiveChartsGeneratedCode` (source-generated `SourceGen*` bases)
- target: `lib/net8.0`
- depends: `LiveChartsCore.SkiaSharpView` (paints, `SKCharts`, drawn visuals, theme registration), `LiveChartsCore` (chart math, `Themes`, `Geo`), `Avalonia`, `Avalonia.Skia`
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

[AXIS_AND_SECTION_TYPES]: axes, sections, and visual collections; `BaseXamlAxis<T>` implements `ICartesianAxis` itself, so the XAML element seats directly in `XAxes`/`YAxes`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------- | :------------ | :------------------------------------ |
|  [01]   | `BaseXamlAxis<T>`     | abstract      | cartesian axis base, `T : Axis`       |
|  [02]   | `XamlAxis`            | class         | numeric axis                          |
|  [03]   | `XamlDateTimeAxis`    | class         | date-time axis                        |
|  [04]   | `XamlTimeSpanAxis`    | class         | time-span axis                        |
|  [05]   | `XamlLogarithmicAxis` | class         | logarithmic axis                      |
|  [06]   | `XamlPolarAxis`       | class         | polar axis (`IPolarAxis`)             |
|  [07]   | `SharedAxesPair`      | class         | `First`/`Second` shared-range pairing |
|  [08]   | `AxesCollection`      | collection    | cartesian axis collection             |
|  [09]   | `PolarAxesCollection` | collection    | polar axis collection                 |
|  [10]   | `SectionsCollection`  | collection    | section collection                    |
|  [11]   | `VisualsCollection`   | collection    | visual collection                     |

[SERIES_TYPES]: XAML series wrappers; every `Xaml<Kind>Series<TModel,TVisual,TLabel>` implements `ISeries` and its family contract, so the shell binds straight into `Series`, and single-arg subclasses default `TVisual`/`TLabel`.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `XamlSeries`                | abstract      | series wrapper base                     |
|  [02]   | `SeriesCollection`          | collection    | `ISeries` collection                    |
|  [03]   | `XamlColumnSeries`          | class         | column bars                             |
|  [04]   | `XamlRowSeries`             | class         | horizontal bars                         |
|  [05]   | `XamlLineSeries`            | class         | line series                             |
|  [06]   | `XamlStepLineSeries`        | class         | step-line series                        |
|  [07]   | `XamlScatterSeries`         | class         | scatter series                          |
|  [08]   | `XamlCandlesticksSeries`    | class         | financial candlesticks                  |
|  [09]   | `XamlBoxSeries`             | class         | box / whisker series                    |
|  [10]   | `XamlHeatSeries`            | class         | heat series                             |
|  [11]   | `XamlPieSeries`             | class         | pie series                              |
|  [12]   | `XamlPolarLineSeries`       | class         | polar line series                       |
|  [13]   | `XamlStackedAreaSeries`     | class         | stacked area                            |
|  [14]   | `XamlStackedStepAreaSeries` | class         | stacked step area                       |
|  [15]   | `XamlStackedColumnSeries`   | class         | stacked column                          |
|  [16]   | `XamlStackedRowSeries`      | class         | stacked row                             |
|  [17]   | `ChartPointState`           | class         | named state carrying a `Set` collection |
|  [18]   | `Set`                       | class         | `PropertyName`/`Value` state setter row |

[POINT_MODEL_TYPES]: the bound-value model in `LiveChartsCore.Kernel` every `Mapping` delegate answers in, and the axis contracts in `LiveChartsCore.Kernel.Sketches` every theme and chrome rule writes through.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :---------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Coordinate`      | readonly struct | one plotted point across every series arity             |
|  [02]   | `Error`           | readonly struct | per-point error extents on both axes                    |
|  [03]   | `ChartPoint`      | class           | measured point with its context and visual              |
|  [04]   | `IPlane`          | interface       | axis members every scale carries                        |
|  [05]   | `ICartesianAxis`  | interface       | `IPlane` plus tick, subseparator, and crosshair members |
|  [06]   | `IPolarAxis`      | interface       | `IPlane` plus angular label and radius members          |
|  [07]   | `FindingStrategy` | enum            | hover and hit-test comparison policy                    |
|  [08]   | `ClipMode`        | flags enum      | draw-margin clip axes                                   |

[`Coordinate`]: `PrimaryValue` `SecondaryValue` `TertiaryValue` `QuaternaryValue` `QuinaryValue` `SenaryValue` `PointError` `IsEmpty` `Empty`
[`Error`]: `Xi` `Xj` `Yi` `Yj` `IsEmpty` `Empty` — ctors `(double xi, double xj, double yi, double yj)` and `(double x, double y)`
[`IPlane`]: `Name` `NameTextSize` `NamePadding` `Labeler` `Labels` `MinStep` `ForceStepToMin` `MinSeparators` `UnitWidth` `MinLimit` `MaxLimit` `IsInverted` `LabelsRotation` `TextSize` `ShowSeparatorLines` `CustomSeparators` `NamePaint` `LabelsPaint` `SeparatorsPaint` `DataBounds` `VisibleDataBounds` `AnimationsSpeed` `EasingFunction` `GetPossibleSize` `GetNameLabelSize`
[`ICartesianAxis`]: `Orientation` `Position` `Padding` `LabelsDensity` `LabelsAlignment` `InLineNamePlacement` `SeparatorsAtCenter` `TicksAtCenter` `SubseparatorsPaint` `SubseparatorsCount` `DrawTicksPath` `TicksPaint` `SubticksPaint` `ZeroPaint` `CrosshairPaint` `CrosshairLabelsPaint` `CrosshairLabelsBackground` `CrosshairPadding` `CrosshairSnapEnabled` `SharedWith` `MinZoomDelta` `BouncingDistance` `GetLimits` `SetLimits` `InvalidateCrosshair` `ClearCrosshair` `SetLogBase`
[`IPolarAxis`]: `Orientation` `Ro` `LabelsAngle` `LabelsVerticalAlignment` `LabelsHorizontalAlignment` `LabelsPadding` `LabelsBackground` `Initialize` `Initialized`
[`FindingStrategy`]: `Automatic` `CompareAll` `CompareOnlyX` `CompareOnlyY` `CompareAllTakeClosest` `CompareOnlyXTakeClosest` `CompareOnlyYTakeClosest` `ExactMatch` `ExactMatchTakeClosest`
[`ClipMode`]: `None`(0) `X`(1) `Y`(2) `XY`(3)

- `Coordinate` ctor arities map to series families directly: `(x, y)` for line, column, and pie; `(x, y, weight)` for scatter and heat; `(x, high, open, close, low)` for candlesticks; `(x, maximum, thirdQuartile, firstQuartile, minimum, median)` for box; and the seven-argument form `(primary, secondary, tertiary, quaternary, quinary, senary, Error)` for an error-bearing point. The two-argument form assigns `y` to `PrimaryValue` and `x` to `SecondaryValue`, so a hand-built coordinate that swaps them plots transposed.
- `ZoomAndPanMode` carries composite members beside its flags: `PanX`(1) `ZoomX`(2) `PanY`(4) `ZoomY`(8) `NoFit`(0x10) `NoZoomBySection`(0x20) `InvertPanningPointerTrigger`(0x40) `X`(3) `Y`(0xC) `Both`(0xF).
- `LegendPosition` carries no `Auto` member (`Hidden` `Top` `Left` `Right` `Bottom`) where `TooltipPosition` does (`Hidden` `Auto` `Top` `Bottom` `Left` `Right` `Center`), so a shared placement vocabulary maps its auto row onto a chosen side for the legend.

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

[DRAWN_VISUAL_TYPES]: drawn visual and gauge-builder owners in `LiveChartsCore.SkiaSharpView.{VisualElements,Extensions}`, reachable from code-behind and from `VisualElements` without a XAML shell.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `DrawnLabelVisual`          | class         | label visual over a `LabelGeometry`              |
|  [02]   | `LabelVisual`               | class         | measured label visual; `DefaultValues` singleton |
|  [03]   | `GeometryVisual<TGeometry>` | class         | arbitrary bounded geometry as a visual           |
|  [04]   | `SVGVisual`                 | class         | `SKPath`-backed SVG visual                       |
|  [05]   | `NeedleVisual`              | class         | gauge needle geometry visual                     |
|  [06]   | `AngularTicksVisual`        | class         | arc, line, and label tick visual                 |
|  [07]   | `GaugeGenerator`            | static        | gauge series arrays from `GaugeItem` rows        |
|  [08]   | `GaugeItem`                 | class         | one gauge value plus its series builder          |
|  [09]   | `BaseGaugeItem<TSeries>`    | class         | gauge item base; `Background` sentinel value     |
|  [10]   | `GaugeOptions`              | enum          | `None` / `Solid` / `Angular`                     |
|  [11]   | `PieChartExtensions`        | static        | `AsPieSeries` projections over any sequence      |

[THEME_TYPES]: the process-wide theming and settings rail in `LiveChartsCore.{Kernel,Themes}` and `LiveChartsCore.SkiaSharpView`.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `LiveCharts`                | static        | process-wide settings root and sentinels             |
|  [02]   | `LiveChartsSettings`        | class         | fluent default settings; owns the single `Theme`     |
|  [03]   | `RenderingSettings`         | class         | GPU, vsync, render-loop FPS, FPS overlay             |
|  [04]   | `Theme`                     | class         | resolved palette, animation, and per-family builders |
|  [05]   | `LvcThemeKind`              | enum          | `Unknown` (system) / `Light` / `Dark`                |
|  [06]   | `ColorPalletes`             | static        | `FluentDesign`, `MaterialDesign{200,500,800}` ramps  |
|  [07]   | `ThemesExtensions`          | static        | default, light, and dark theme registration          |
|  [08]   | `LiveChartsthemeExtensions` | static        | `HasRuleFor*` theme-rule builders                    |
|  [09]   | `LiveChartsSkiaSharp`       | static        | Skia backend registration and colour bridging        |
|  [10]   | `TextSettings`              | class         | global typeface, font builder, and RTL resolution    |

[PAINT_TYPES]: the paint tier — `Paint` in `LiveChartsCore.Painting`, every concrete in `LiveChartsCore.SkiaSharpView.Painting`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `Paint`               | abstract      | animatable draw task bound to a canvas            |
|  [02]   | `SkiaPaint`           | abstract      | Skia paint base — font, cap, join, effect, filter |
|  [03]   | `SolidColorPaint`     | class         | single `SKColor` fill or stroke                   |
|  [04]   | `LinearGradientPaint` | class         | gradient between two points                       |
|  [05]   | `RadialGradientPaint` | class         | gradient from a centre and radius                 |
|  [06]   | `PathEffect`          | abstract      | transitionable stroke-path effect                 |
|  [07]   | `DashEffect`          | class         | dash array and phase                              |
|  [08]   | `ImageFilter`         | abstract      | draw-time image filter                            |
|  [09]   | `DropShadow`          | class         | offset, sigma, and colour shadow                  |
|  [10]   | `Blur`                | class         | two-axis gaussian blur                            |

[OFFSCREEN_TYPES]: headless `LiveChartsCore.SkiaSharpView.SKCharts` renderers and the drawn tooltip and legend concretes.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `InMemorySkiaSharpChart` | abstract      | offscreen render base — size, background, output |
|  [02]   | `SKCartesianChart`       | class         | offscreen cartesian chart                        |
|  [03]   | `SKPieChart`             | class         | offscreen pie chart                              |
|  [04]   | `SKPolarChart`           | class         | offscreen polar chart                            |
|  [05]   | `SKGeoMap`               | class         | offscreen geographic map                         |
|  [06]   | `SKDefaultTooltip`       | class         | drawn tooltip (`IChartTooltip`)                  |
|  [07]   | `SKDefaultLegend`        | class         | drawn legend (`IChartLegend`)                    |
|  [08]   | `SKHeatLegend`           | class         | drawn heat-ramp legend with badge and formatter  |

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

[TYPE_CONVERTER_TYPES]: `LiveChartsCore.SkiaSharpView.TypeConverters` — `System.ComponentModel.TypeConverter` rows parsing XAML attribute strings into chart values.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :--------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `HexToLvcColorTypeConverter`       | class         | hex string to `LvcColor`          |
|  [02]   | `HexToLvcColorArrayTypeConverter`  | class         | hex list to `LvcColor[]`          |
|  [03]   | `HexToPaintTypeConverter`          | class         | hex string to `Paint`             |
|  [04]   | `MarginTypeConverter`              | class         | string to `Margin`                |
|  [05]   | `PaddingTypeConverter`             | class         | string to `Padding`               |
|  [06]   | `PointTypeConverter`               | class         | string to `LvcPoint`              |
|  [07]   | `PointDTypeConverter`              | class         | string to `LvcPointD`             |
|  [08]   | `StringArrayTypeConverter`         | class         | delimited string to `string[]`    |
|  [09]   | `StringToDoubleArrayTypeConverter` | class         | delimited string to `double[]`    |
|  [10]   | `ValuesTypeConverter`              | class         | delimited string to series values |

[GEO_TYPES]: map binding surfaces in transitive `LiveChartsCore.Geo` (the heat series concretes in `LiveChartsCore.SkiaSharpView`), bound through `GeoMap`/`SourceGenMapChart`.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------ | :------------ | :-------------------------------------------------- |
|  [01]   | `IGeoMapView`                               | interface     | map view contract                                   |
|  [02]   | `DrawnMap : IDisposable`                    | class         | active map record (`ActiveMap`)                     |
|  [03]   | `MapProjection`                             | enum          | projection mode                                     |
|  [04]   | `IWeigthedMapLand : INotifyPropertyChanged` | interface     | settable `Name`/`Value` land model                  |
|  [05]   | `CoreHeatLandSeries<TModel> : IGeoSeries`   | class         | heat-series base, `TModel : IWeigthedMapLand`       |
|  [06]   | `HeatLandSeries<TModel>` / `HeatLandSeries` | class         | Skia heat series; non-generic binds `HeatLand`      |
|  [07]   | `HeatLand`                                  | class         | shipped `IWeigthedMapLand`; `()`/`(string, double)` |
|  [08]   | `LandDefinition`                            | class         | resolved land record — the `FindLand` result        |
|  [09]   | `MapLayer`                                  | class         | loaded layer record — every `AddLayerFrom*` result  |

## [03]-[ENTRYPOINTS]

[CHART_ENTRYPOINTS]: members on `SourceGenChart`, exposed by every public chart

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `Series`                                                      | property | series input                                    |
|  [02]   | `SeriesSource`                                                | property | source-collection input                         |
|  [03]   | `SeriesTemplate`                                              | property | per-item `DataTemplate`                         |
|  [04]   | `VisualElements`                                              | property | overlay visuals                                 |
|  [05]   | `Title`                                                       | property | chart title visual                              |
|  [06]   | `Legend`                                                      | property | settable `IChartLegend`                         |
|  [07]   | `LegendPosition`                                              | property | legend placement                                |
|  [08]   | `LegendTextPaint` / `LegendBackgroundPaint`                   | property | legend text and background paints               |
|  [09]   | `LegendTextSize`                                              | property | legend text size                                |
|  [10]   | `Tooltip`                                                     | property | settable `IChartTooltip`                        |
|  [11]   | `TooltipPosition`                                             | property | tooltip placement                               |
|  [12]   | `TooltipTextPaint` / `TooltipBackgroundPaint`                 | property | tooltip text and background paints              |
|  [13]   | `TooltipTextSize`                                             | property | tooltip text size                               |
|  [14]   | `ChartTheme`                                                  | property | per-control `Theme` override                    |
|  [15]   | `DrawMargin`                                                  | property | draw bounds                                     |
|  [16]   | `AnimationsSpeed` / `EasingFunction`                          | property | animation duration and curve                    |
|  [17]   | `UpdaterThrottler`                                            | property | redraw coalescing window                        |
|  [18]   | `AutoUpdateEnabled`                                           | property | automatic redraw switch                         |
|  [19]   | `SyncContext`                                                 | property | cross-chart sync lock                           |
|  [20]   | `CoreCanvas` / `CoreChart`                                    | property | live `CoreMotionCanvas` and `Chart`             |
|  [21]   | `GetPointsAt(LvcPointD, FindingStrategy, FindPointFor)`       | instance | hit-test data points                            |
|  [22]   | `GetVisualsAt(LvcPointD)`                                     | instance | hit-test visual elements                        |
|  [23]   | `Measuring` / `UpdateStarted` / `UpdateFinished`              | event    | measure and redraw lifecycle                    |
|  [24]   | `DataPointerDown` / `HoveredPointsChanged`                    | event    | point pointer-down and hover set                |
|  [25]   | `VisualElementsPointerDown`                                   | event    | visual pointer-down                             |
|  [26]   | `{UpdateStarted,DataPointerDown,HoveredPointsChanged}Command` | property | `ICommand` peers of the update and point events |
|  [27]   | `{ChartPointPointerDown,VisualElementsPointerDown}Command`    | property | `ICommand` peers of the pointer events          |
|  [28]   | `{PointerPressed,PointerMove,PointerReleased}Command`         | property | raw pointer `ICommand` rail                     |

[CARTESIAN_ENTRYPOINTS]: additional members on `SourceGenCartesianChart`

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `XAxes` / `YAxes`                        | property | X and Y axis collections              |
|  [02]   | `Sections`                               | property | chart sections                        |
|  [03]   | `DrawMarginFrame`                        | property | draw frame                            |
|  [04]   | `ZoomMode` / `ZoomingSpeed`              | property | zoom and pan mode, wheel step         |
|  [05]   | `FindingStrategy`                        | property | hover and hit-test point strategy     |
|  [06]   | `MatchAxesScreenDataRatio`               | property | equal pixel-per-unit across both axes |
|  [07]   | `ScalePixelsToData(LvcPointD, int, int)` | instance | pixel to data coordinate              |
|  [08]   | `ScaleDataToPixels(LvcPointD, int, int)` | instance | data to pixel coordinate              |

[RADIAL_CHART_ENTRYPOINTS]: pie and polar chart members on `SourceGenPieChart` and `SourceGenPolarChart`

| [INDEX] | [SURFACE]                                        | [OWNER]               | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------- | :-------------------- | :------- | :---------------------------------- |
|  [01]   | `IsClockwise` / `InitialRotation` / `MaxAngle`   | `SourceGenPieChart`   | property | sweep direction, start, and extent  |
|  [02]   | `MinValue` / `MaxValue`                          | `SourceGenPieChart`   | property | value range the sweep normalizes to |
|  [03]   | `AngleAxes` / `RadiusAxes`                       | `SourceGenPolarChart` | property | polar axis collections              |
|  [04]   | `InitialRotation` / `TotalAngle` / `InnerRadius` | `SourceGenPolarChart` | property | polar sweep start, extent, and hole |
|  [05]   | `FitToBounds`                                    | `SourceGenPolarChart` | property | radial fit to measured bounds       |
|  [06]   | `ScalePixelsToData` / `ScaleDataToPixels`        | `SourceGenPolarChart` | instance | pixel and polar data conversion     |

[AXIS_ENTRYPOINTS]: members on `BaseXamlAxis<T>`, inherited by every cartesian axis shell

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `AxisName` / `NamePaint` / `NameTextSize` / `NamePadding`        | property | axis name label and its paint            |
|  [02]   | `InLineNamePlacement`                                            | property | name drawn inside the plot area          |
|  [03]   | `Labeler` / `Labels` / `LabelsPaint` / `TextSize`                | property | tick label text and paint                |
|  [04]   | `LabelsRotation` / `LabelsAlignment` / `LabelsDensity`           | property | tick label orientation and thinning      |
|  [05]   | `Padding`                                                        | property | label padding against the plot           |
|  [06]   | `MinStep` / `ForceStepToMin` / `MinSeparators`                   | property | separator cadence floor                  |
|  [07]   | `CustomSeparators`                                               | property | explicit separator positions             |
|  [08]   | `MinLimit` / `MaxLimit` / `UnitWidth` / `IsInverted`             | property | visible range, unit span, direction      |
|  [09]   | `Position` / `SeparatorsAtCenter` / `TicksAtCenter`              | property | axis side and separator anchoring        |
|  [10]   | `ShowSeparatorLines` / `SeparatorsPaint`                         | property | major grid lines                         |
|  [11]   | `SubseparatorsPaint` / `SubseparatorsCount`                      | property | minor grid lines and their count         |
|  [12]   | `DrawTicksPath` / `TicksPaint` / `SubticksPaint`                 | property | tick marks and the tick baseline         |
|  [13]   | `ZeroPaint`                                                      | property | zero-value reference line                |
|  [14]   | `CrosshairPaint` / `CrosshairLabelsPaint`                        | property | crosshair line and label paints          |
|  [15]   | `CrosshairLabelsBackground` / `CrosshairPadding`                 | property | crosshair label background and padding   |
|  [16]   | `CrosshairSnapEnabled`                                           | property | crosshair snapping to the closest point  |
|  [17]   | `SharedWith`                                                     | property | axes sharing one range                   |
|  [18]   | `MinZoomDelta` / `BouncingDistance`                              | property | zoom floor and overscroll rebound        |
|  [19]   | `AnimationsSpeed` / `EasingFunction`                             | property | per-axis animation override              |
|  [20]   | `DataBounds` / `VisibleDataBounds` / `Orientation`               | property | measured bounds and resolved orientation |
|  [21]   | `GetLimits() -> AxisLimit`                                       | instance | current min, max, and step               |
|  [22]   | `SetLimits(double, double, double, bool, bool)`                  | instance | range write with shared propagation      |
|  [23]   | `InvalidateCrosshair(Chart, LvcPoint)` / `ClearCrosshair(Chart)` | instance | crosshair draw and clear                 |
|  [24]   | `GetNameLabelSize(Chart)` / `GetPossibleSize(Chart)`             | instance | measured name and axis extents           |
|  [25]   | `MeasureStarted` / `MeasureStartedCommand`                       | member   | measure-pass hook and its `ICommand`     |

- `XamlPolarAxis` declares its own roster: `PolarAxisName`, `LabelsAngle`, `LabelsPadding`, `LabelsVerticalAlignment`, `LabelsHorizontalAlignment`, `LabelsBackground`, and an `Initialized` event, and carries no crosshair, tick, or shared-range member.

[SECTION_ENTRYPOINTS]: members on `XamlRectangularSection`; a null bound extends that edge to the draw margin.

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Xi` / `Xj` / `Yi` / `Yj`            | property | band bounds in data coordinates       |
|  [02]   | `Stroke` / `Fill`                    | property | band stroke and fill paints           |
|  [03]   | `Label` / `LabelPaint` / `LabelSize` | property | band label text, paint, and size      |
|  [04]   | `ScalesXAt` / `ScalesYAt`            | property | axis index the bounds resolve against |
|  [05]   | `ZIndex` / `IsVisible` / `Tag`       | property | draw order, visibility, identity      |
|  [06]   | `Invalidate(Chart)`                  | instance | section redraw                        |

[SERIES_ENTRYPOINTS]: the member tail every `Xaml*Series` carries; `WrappedSeries` stays protected, so consumer code binds the shell itself as `ISeries`.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `Values` / `Mapping` / `Pivot`                                | property | bound collection, projection, baseline   |
|  [02]   | `SeriesName` / `Tag` / `IsVisible` / `ZIndex`                 | property | identity, visibility, draw order         |
|  [03]   | `Stroke` / `Fill`                                             | property | series stroke and fill paints            |
|  [04]   | `GeometryFill` / `GeometryStroke` / `GeometrySize`            | property | point marker paints and size             |
|  [05]   | `GeometrySvg`                                                 | property | point marker SVG path override           |
|  [06]   | `LineSmoothness` / `EnableNullSplitting`                      | property | spline tension and null-gap breaks       |
|  [07]   | `ShowDataLabels` / `DataLabelsPaint` / `DataLabelsFormatter`  | property | label switch, paint, and text            |
|  [08]   | `DataLabelsPosition` / `DataLabelsTranslate`                  | property | label anchor and offset                  |
|  [09]   | `DataLabelsSize` / `DataLabelsRotation` / `DataLabelsPadding` | property | label metrics                            |
|  [10]   | `DataLabelsMaxWidth`                                          | property | label wrap width                         |
|  [11]   | `XToolTipLabelFormatter` / `YToolTipLabelFormatter`           | property | per-axis tooltip text                    |
|  [12]   | `ClippingMode`                                                | property | draw-margin clip policy                  |
|  [13]   | `IsHoverable` / `IsVisibleAtLegend`                           | property | hit-testing and legend admission         |
|  [14]   | `MiniatureShapeSize` / `MiniatureStrokeThickness`             | property | legend and tooltip swatch geometry       |
|  [15]   | `DataPadding` / `ScalesXAt` / `ScalesYAt`                     | property | bounds padding and axis binding          |
|  [16]   | `ShowError` / `ErrorPaint`                                    | property | error-bar rendering                      |
|  [17]   | `AnimationsSpeed` / `EasingFunction`                          | property | per-series animation override            |
|  [18]   | `AdditionalVisualStates`                                      | property | extra `ChartPointState` rows             |
|  [19]   | `VisualStates` / `DataFactory`                                | property | resolved state table and point factory   |
|  [20]   | `Invalidate(Chart)`                                           | instance | series redraw                            |
|  [21]   | `GetMiniatureGeometry(ChartPoint?)`                           | instance | legend swatch geometry                   |
|  [22]   | `ConvertToTypedChartPoint(ChartPoint)`                        | instance | typed point projection                   |
|  [23]   | `GetBounds(Chart, ICartesianAxis, ICartesianAxis)`            | instance | measured series bounds                   |
|  [24]   | `GetPrimaryToolTipText` / `GetSecondaryToolTipText`           | instance | resolved tooltip text                    |
|  [25]   | `GetDataLabelText(ChartPoint)`                                | instance | resolved data-label text                 |
|  [26]   | `RestartAnimations()` / `SoftDeleteOrDispose(IChartView)`     | instance | animation restart and teardown           |
|  [27]   | `PointMeasured` / `PointCreated`                              | event    | typed point lifecycle                    |
|  [28]   | `DataPointerDown` / `ChartPointPointerDown`                   | event    | typed point pointer-down                 |
|  [29]   | `ChartPointPointerHover` / `ChartPointPointerHoverLost`       | event    | hover enter and leave                    |
|  [30]   | `{PointMeasured,PointCreated,DataPointerDown}Command`         | property | `ICommand` peers of the lifecycle events |
|  [31]   | `{ChartPointPointerHover,ChartPointPointerHoverLost}Command`  | property | `ICommand` peers of the hover events     |

- `ClippingMode`, `ShowError`/`ErrorPaint`, `ScalesXAt`/`ScalesYAt`, and the `X`/`Y` tooltip formatter pair ride cartesian series; `LineSmoothness`/`EnableNullSplitting` ride the line and step-line families.

[RADIAL_SERIES_ENTRYPOINTS]: pie and gauge members on `XamlPieSeries` and `XamlGaugeSeries`, replacing the cartesian geometry and tooltip tail.

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `InnerRadius` / `OuterRadiusOffset`           | property | doughnut hole and outer inset           |
|  [02]   | `RelativeInnerRadius` / `RelativeOuterRadius` | property | radii as a fraction of the plot         |
|  [03]   | `Pushout` / `HoverPushout`                    | property | slice offset at rest and on hover       |
|  [04]   | `CornerRadius` / `InvertedCornerRadius`       | property | slice corner rounding and its direction |
|  [05]   | `MaxRadialColumnWidth` / `RadialAlign`        | property | radial column width and alignment       |
|  [06]   | `IsFillSeries` / `IsRelativeToMinValue`       | property | background ring and min-relative sweep  |
|  [07]   | `DataLabelsPosition` (`PolarLabelsPosition`)  | property | polar label anchor                      |
|  [08]   | `ToolTipLabelFormatter`                       | property | tooltip text                            |
|  [09]   | `GaugeValue`                                  | property | gauge value (`XamlGaugeSeries`)         |
|  [10]   | `GetBounds(Chart)`                            | instance | measured radial bounds                  |

[GAUGE_BUILD_ENTRYPOINTS]: code-behind gauge and pie construction in `LiveChartsCore.SkiaSharpView.Extensions`

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `GaugeGenerator.BuildSolidGauge(params GaugeItem[])`                            | static   | solid gauge series array           |
|  [02]   | `GaugeGenerator.BuildAngularGaugeSections(params GaugeItem[])`                  | static   | angular gauge section array        |
|  [03]   | `GaugeItem(double, Action<PieSeries<ObservableValue>>?)`                        | ctor     | gauge item from a raw value        |
|  [04]   | `GaugeItem(ObservableValue, Action<PieSeries<ObservableValue>>?)`               | ctor     | gauge item from a live value       |
|  [05]   | `BaseGaugeItem<TSeries>.Value` / `.Builder`                                     | property | item value and per-series builder  |
|  [06]   | `BaseGaugeItem<TSeries>.Background`                                             | static   | sentinel marking the backdrop item |
|  [07]   | `PieChartExtensions.AsPieSeries(IEnumerable<TModel>, Action<…>?, GaugeOptions)` | static   | sequence to pie or gauge series    |

[VISUAL_ENTRYPOINTS]: free-visual members on `XamlNeedle`, `XamlAngularTicks`, and `XamlDrawnLabelVisual`

| [INDEX] | [SURFACE]                                                   | [OWNER]                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :--------------------- | :------- | :--------------------------------- |
|  [01]   | `X` / `Y` / `LocationUnit` / `ScalesXAt` / `ScalesYAt`      | needle, ticks          | property | placement and its measure unit     |
|  [02]   | `Rotation` / `Translate` / `ZIndex`                         | needle, ticks          | property | angle, offset, and draw order      |
|  [03]   | `Value` / `Width` / `Fill`                                  | `XamlNeedle`           | property | needle value, girth, and paint     |
|  [04]   | `Stroke` / `LabelsPaint` / `Labeler` / `LabelsSize`         | `XamlAngularTicks`     | property | tick and tick-label rendering      |
|  [05]   | `TicksLength` / `OuterOffset` / `LabelsOuterOffset`         | `XamlAngularTicks`     | property | tick length and radial offsets     |
|  [06]   | `Measure(Chart)`                                            | needle, ticks          | instance | measured visual size               |
|  [07]   | `Text` / `Paint` / `TextSize` / `Background` / `Padding`    | `XamlDrawnLabelVisual` | property | label text, paint, and box         |
|  [08]   | `VerticalAlign` / `HorizontalAlign` / `MaxWidth`            | `XamlDrawnLabelVisual` | property | label alignment and wrap width     |
|  [09]   | `TranslateTransform` / `RotateTransform` / `ScaleTransform` | `XamlDrawnLabelVisual` | property | transform stack over the drawn box |
|  [10]   | `SkewTransform` / `TransformOrigin` / `HasTransform`        | `XamlDrawnLabelVisual` | property | skew, pivot, and transform flag    |
|  [11]   | `DropShadow` / `Opacity` / `ClippingBounds`                 | `XamlDrawnLabelVisual` | property | shadow, alpha, and clip            |
|  [12]   | `AnimationSpeed` / `Easing` / `RemoveOnCompleted`           | `XamlDrawnLabelVisual` | property | reveal animation and self-removal  |
|  [13]   | `Invalidate(Chart)` / `GetHitBox()`                         | every visual shell     | instance | redraw and hit rectangle           |
|  [14]   | `PointerDown` / `PointerDownCommand`                        | every visual shell     | member   | visual pointer-down and `ICommand` |

[THEME_ENTRYPOINTS]: process-wide theming; `LiveChartsSettings.HasTheme` replaces the whole `Theme`, so the last `Add*Theme` call wins and every rule builder chains onto that one instance.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `LiveCharts.Configure(Action<LiveChartsSettings>)`                      | static   | mutate the process-wide defaults         |
|  [02]   | `LiveCharts.DefaultSettings`                                            | static   | the single `LiveChartsSettings` instance |
|  [03]   | `LiveCharts.DisableAnimations`                                          | static   | 1 ms sentinel for `AnimationsSpeed`      |
|  [04]   | `LiveCharts.RenderingSettings`                                          | static   | GPU, vsync, `LiveChartsRenderLoopFPS`    |
|  [05]   | `LiveCharts.EnableLogging`                                              | static   | diagnostic logging switch                |
|  [06]   | `LiveCharts.IgnoreToolTipLabel` / `.IgnoreSeriesName`                   | static   | sentinels suppressing a label            |
|  [07]   | `LiveChartsSkiaSharp.UseDefaults()` / `.AddSkiaSharp()`                 | static   | register the Skia backend and mappers    |
|  [08]   | `LiveChartsSkiaSharp.HasGlobalSKTypeface(SKTypeface)`                   | static   | one typeface across every drawn label    |
|  [09]   | `LiveChartsSkiaSharp.HasTextSettings(TextSettings)`                     | static   | typeface, font builder, and RTL policy   |
|  [10]   | `LiveChartsSkiaSharp.HasRenderingFactory(…)`                            | static   | swap the motion-canvas rendering factory |
|  [11]   | `ThemesExtensions.AddDefaultTheme(Action<Theme>?, LvcThemeKind)`        | static   | seed the shipped theme at a chosen kind  |
|  [12]   | `ThemesExtensions.AddLightTheme(Action<Theme>?)`                        | static   | seed the light theme                     |
|  [13]   | `ThemesExtensions.AddDarkTheme(Action<Theme>?)`                         | static   | seed the dark theme                      |
|  [14]   | `LiveChartsSettings.HasTheme(Action<Theme>)` / `.GetTheme()`            | instance | mint and read the active `Theme`         |
|  [15]   | `LiveChartsSettings.HasRenderingSettings(…)`                            | instance | configure `RenderingSettings` fluently   |
|  [16]   | `LiveChartsSettings.With{AnimationsSpeed,EasingFunction}(…)`            | instance | default animation duration and curve     |
|  [17]   | `LiveChartsSettings.With{ZoomSpeed,ZoomMode}(…)`                        | instance | default zoom step and mode               |
|  [18]   | `LiveChartsSettings.WithUpdateThrottlingTimeout(TimeSpan)`              | instance | default redraw coalescing window         |
|  [19]   | `LiveChartsSettings.WithLegend{BackgroundPaint,TextPaint,TextSize}(…)`  | instance | default legend styling                   |
|  [20]   | `LiveChartsSettings.WithTooltip{BackgroundPaint,TextPaint,TextSize}(…)` | instance | default tooltip styling                  |
|  [21]   | `LiveChartsSettings.HasMap<TModel>(Func<TModel,int,Coordinate>)`        | instance | register a model-to-coordinate mapper    |
|  [22]   | `LiveChartsSettings.GetMap<TModel>()` / `.RemoveMap<TModel>()`          | instance | read and drop a registered mapper        |
|  [23]   | `LiveChartsSettings.AddDefaultMappers()`                                | instance | admit the numeric primitive mappers      |
|  [24]   | `LiveChartsSettings.UseRightToLeftSettings()`                           | instance | RTL label and legend flow                |
|  [25]   | `Theme.RequestedTheme` / `.IsDark` / `.ThemeId`                         | property | requested kind, resolved kind, identity  |
|  [26]   | `Theme.Colors` / `.VirtualBackroundColor`                               | property | series colour ramp and backdrop colour   |
|  [27]   | `Theme.AnimationsSpeed` / `.EasingFunction`                             | property | theme-level animation duration and curve |
|  [28]   | `Theme.Tooltip{TextPaint,BackgroundPaint,TextSize}`                     | property | theme tooltip styling                    |
|  [29]   | `Theme.Legend{TextPaint,BackgroundPaint,TextSize}`                      | property | theme legend styling                     |
|  [30]   | `Theme.GetDefaultTooltip` / `.GetDefaultLegend`                         | property | factory per chart for tooltip and legend |
|  [31]   | `Theme.ApplyStyleToSeries(ISeries)`                                     | instance | run the series rule chain                |
|  [32]   | `Theme.ApplyStyleToAxis(IPlane)`                                        | instance | run the axis rule chain                  |
|  [33]   | `Theme.ApplyStyleToDrawMarginFrame(CoreDrawMarginFrame)`                | instance | run the frame rule chain                 |
|  [34]   | `Theme.ApplyStyleTo<TChartElement>(IChartElement)`                      | instance | run one element type's rule chain        |
|  [35]   | `Theme.GetSeriesColor(ISeries)`                                         | instance | next ramp colour for a series            |
|  [36]   | `Theme.AxisBuilder` / `.SeriesBuilder` / `.DrawMarginFrameBuilder`      | property | mutable `List<Action<…>>` rule chains    |
|  [37]   | `Theme.DrawMarginFrameGetter` / `.ChartElementElementBuilder`           | property | frame source and per-type rule table     |
|  [38]   | `Theme.Initialized`                                                     | property | `List<Action>` run once at resolve       |
|  [39]   | `ColorPalletes.{FluentDesign,MaterialDesign200,500,800}`                | static   | nine-colour `LvcColor[]` ramps           |

[THEME_RULE_ENTRYPOINTS]: `LiveChartsthemeExtensions` — `HasRuleForX` appends its predicate to `Theme.XBuilder` and returns the same `Theme`, so rules chain and never replace.

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `HasRuleForAxes(Action<IPlane>)`                                              | static  | every cartesian and polar axis             |
|  [02]   | `HasRuleForDrawMarginFrame(Func<…>, Action<…>)`                               | static  | frame getter plus its styling rule         |
|  [03]   | `HasRuleForAnySeries(Action<ISeries>)`                                        | static  | every series family                        |
|  [04]   | `HasRuleForLineSeries` / `HasRuleForStackedLineSeries`                        | static  | line and stacked-line series               |
|  [05]   | `HasRuleForStepLineSeries` / `HasRuleForStackedStepLineSeries`                | static  | step-line and stacked-step-line series     |
|  [06]   | `HasRuleForBarSeries` / `HasRuleForColumnSeries` / `HasRuleForRowSeries`      | static  | bar, column, and row series                |
|  [07]   | `HasRuleForStackedBarSeries` / `…ColumnSeries` / `…RowSeries`                 | static  | stacked bar, column, and row series        |
|  [08]   | `HasRuleForPieSeries` / `HasRuleForGaugeSeries` / `HasRuleForGaugeFillSeries` | static  | pie, gauge, and gauge backdrop             |
|  [09]   | `HasRuleForScatterSeries` / `HasRuleForBoxSeries`                             | static  | scatter and box series                     |
|  [10]   | `HasRuleForHeatSeries` / `HasRuleForFinancialSeries`                          | static  | heat and financial series                  |
|  [11]   | `HasRuleForPolaSeries` / `HasRuleForPolarLineSeries`                          | static  | polar and polar-line series                |
|  [12]   | `HasRuleFor<TChartElement>(Action<TChartElement>)`                            | static  | one `ChartElement` type, visuals included  |
|  [13]   | `HasDefaultTooltip(Func<IChartTooltip>)`                                      | static  | tooltip factory the theme hands each chart |
|  [14]   | `HasDefaultLegend(Func<IChartLegend>)`                                        | static  | legend factory the theme hands each chart  |
|  [15]   | `OnInitialized(Action)`                                                       | static  | hook running once the theme resolves       |

[PAINT_ENTRYPOINTS]: paint construction and configuration

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Paint.Default`                                                                | static   | no-op paint singleton               |
|  [02]   | `Paint.Parse(string) -> Paint?`                                                | static   | hex string to a solid paint         |
|  [03]   | `Paint.ZIndex` / `.IsStroke` / `.IsAntialias`                                  | property | draw order, role, and smoothing     |
|  [04]   | `Paint.StrokeThickness` / `.StrokeMiter` / `.IsPaused`                         | property | stroke metrics and animation pause  |
|  [05]   | `Paint.CloneTask()`                                                            | instance | independent copy of the draw task   |
|  [06]   | `SkiaPaint.FontFamily` / `.SKFontStyle` / `.SKTypeface`                        | property | typeface resolution for label paint |
|  [07]   | `SkiaPaint.StrokeCap` / `.StrokeJoin`                                          | property | Skia cap and join                   |
|  [08]   | `SkiaPaint.PathEffect` / `.ImageFilter`                                        | property | dash effect and image filter        |
|  [09]   | `SkiaPaint.ConfigureSkiaSharpFont(FontBuilderDelegate)`                        | instance | build the `SKFont` per draw         |
|  [10]   | `SolidColorPaint()` / `(SKColor)` / `(SKColor, float)`                         | ctor     | bare paint, colour, stroke width    |
|  [11]   | `SolidColorPaint.Color`                                                        | property | settable paint colour               |
|  [12]   | `LinearGradientPaint(SKColor[], SKPoint, SKPoint, float[]?, SKShaderTileMode)` | ctor     | full linear gradient                |
|  [13]   | `LinearGradientPaint(SKColor, SKColor)` / `(SKColor[])`                        | ctor     | two-stop and ramp shorthands        |
|  [14]   | `LinearGradientPaint.DefaultStartPoint` / `.DefaultEndPoint`                   | static   | left-to-right default axis          |
|  [15]   | `RadialGradientPaint(SKColor[], SKPoint?, float, float[]?, SKShaderTileMode)`  | ctor     | full radial gradient                |
|  [16]   | `RadialGradientPaint(SKColor, SKColor)`                                        | ctor     | centre-to-outer shorthand           |
|  [17]   | `DashEffect(float[], float)`                                                   | ctor     | dash array and phase                |
|  [18]   | `DropShadow(float, float, float, float, SKColor)`                              | ctor     | offset, sigma pair, and colour      |
|  [19]   | `Blur(float, float)`                                                           | ctor     | two-axis gaussian blur              |

[OFFSCREEN_ENTRYPOINTS]: headless rendering and the drawn tooltip and legend concretes

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `InMemorySkiaSharpChart.Width` / `.Height` / `.Background`              | property | render size and backdrop colour   |
|  [02]   | `InMemorySkiaSharpChart.CoreCanvas`                                     | property | live `CoreMotionCanvas`           |
|  [03]   | `InMemorySkiaSharpChart.GetImage() -> SKImage`                          | instance | rendered image in memory          |
|  [04]   | `InMemorySkiaSharpChart.SaveImage(Stream, SKEncodedImageFormat, int)`   | instance | encode to a stream                |
|  [05]   | `InMemorySkiaSharpChart.SaveImage(string, SKEncodedImageFormat, int)`   | instance | encode to a file path             |
|  [06]   | `InMemorySkiaSharpChart.DrawOnCanvas(SKCanvas)`                         | instance | draw into a caller-owned canvas   |
|  [07]   | `SKCartesianChart(IChartView)` / `SKPieChart` / `SKPolarChart`          | ctor     | mirror a live chart offscreen     |
|  [08]   | `SKGeoMap(IGeoMapView)`                                                 | ctor     | mirror a live map offscreen       |
|  [09]   | `SKCartesianChart()` / `SKPieChart()` / `SKPolarChart()` / `SKGeoMap()` | ctor     | standalone offscreen chart        |
|  [10]   | `InMemorySkiaSharpChart.SaveImage(SKCanvas)`                            | instance | encode into a caller-owned canvas |
|  [11]   | `SKDefaultTooltip.Wedge` / `.Easing` / `.AnimationsSpeed`               | property | pointer size and reveal animation |
|  [12]   | `SKDefaultLegend.Easing` / `.AnimationsSpeed`                           | property | legend reveal animation           |
|  [13]   | `SKHeatLegend.Formatter` (`Func<double,string>`)                        | property | the heat legend's ONLY label text |
|  [14]   | `SKHeatLegend.BadgePadding` (`Padding`) / `.BadgeWidth` (`double?`)     | property | ramp bar inset and thickness      |

- Both drawn legends build their content in `GetLayout(Chart)` and neither admits a caller-supplied entry set, so what each can DRAW is fixed: `SKDefaultLegend` iterates `chart.Series.Where(x => x.IsVisibleAtLegend)` and emits exactly one `ISeries.GetMiniatureGeometry(null)` plus one `ISeries.Name` label per entry — no value column, no statistics cell, and no explicitly declared domain is reachable through it; `SKHeatLegend` takes the FIRST visible `IHeatSeries`, reads that series' own `HeatMap` and `WeightBounds`, and draws one `LinearGradientPaint` bar with exactly TWO `LabelGeometry` labels, `Formatter(WeightBounds.Min)` and `Formatter(WeightBounds.Max)`, so its whole label surface is that delegate and an intermediate stop label is undrawable. A heat legend whose chart has no visible heat series, or whose ramp is empty, calls `Hide(chart)` and draws nothing.
- Both legends derive ORIENTATION from `chart.LegendPosition` alone (`Left` and `Right` lay out vertically, `Top` and `Bottom` horizontally) and both read `chart.GetLegendPosition()` for their own origin, so a caller-declared flow column would be overridden on two of the five positions and a corner placement is unreachable — `LegendPosition` spells four sides plus `Hidden` and no corner.
- `chart.View.LegendTextSize` below zero falls back to `Theme.LegendTextSize`, and `SKDefaultLegend` resolves its text paint as `chart.View.LegendTextPaint ?? Theme.LegendTextPaint ?? SolidColorPaint(30,30,30)`, so an unset legend paint reaches a near-black shipped default rather than a theme miss.
- `LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth` caps every drawn legend and tooltip label's `MaxWidth`, so a long series name wraps at a process-wide bound rather than at a per-legend one.

[GEO_ENTRYPOINTS]: properties on `SourceGenMapChart`

| [INDEX] | [SURFACE]       | [SHAPE]  | [CAPABILITY]      |
| :-----: | :-------------- | :------- | :---------------- |
|  [01]   | `ActiveMap`     | property | active map source |
|  [02]   | `MapProjection` | property | projection mode   |
|  [03]   | `Series`        | property | geo series        |
|  [04]   | `Stroke`        | property | land stroke paint |
|  [05]   | `Fill`          | property | land fill paint   |

[GEO_HEAT]: heat-land series columns and `DrawnMap` layer load; every `layerName` is a trailing `string layerName = "default"`, each `AddLayerFrom*` carries a `(source, Paint stroke, Paint fill, …)` overload and an `…Async` peer returning `Task<MapLayer>`, and each `GetMapFrom*` an `…Async` peer

| [INDEX] | [SURFACE]                                              | [OWNER]                      | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------- | :--------------------------- | :------- | :------------------------------- |
|  [01]   | `Lands` (`ICollection<TModel>?`)                       | `CoreHeatLandSeries<TModel>` | property | live land set, deep-observed     |
|  [02]   | `HeatMap` (`LvcColor[]`) / `ColorStops` (`double[]?`)  | `CoreHeatLandSeries<TModel>` | property | ramp colours and stops           |
|  [03]   | `Name` / `IsVisible` / `PropertyChanged`               | `CoreHeatLandSeries<TModel>` | member   | identity, visibility, and change |
|  [04]   | `Measure(MapContext)` / `Delete(MapContext)`           | `CoreHeatLandSeries<TModel>` | instance | map-pass measure and teardown    |
|  [05]   | `()` / `(ICollection<TModel>?)` / `(params TModel[]?)` | `HeatLandSeries<TModel>`     | ctor     | land-set ctor arities            |
|  [06]   | `(ICollection<TModel>?, LvcColor[] heatMap)`           | `HeatLandSeries<TModel>`     | ctor     | land-set and ramp ctor           |
|  [07]   | `FindLand(shortName, layerName)` -> `LandDefinition?`  | `DrawnMap`                   | instance | land lookup, null when absent    |
|  [08]   | `AddLayerFromStreamReader(StreamReader, …)`            | `DrawnMap`                   | instance | layer load from a reader         |
|  [09]   | `AddLayerFromDirectory(string path, …)`                | `DrawnMap`                   | instance | layer load from a directory      |
|  [10]   | `GetWorldMap()` / `GetMapFrom{Directory,StreamReader}` | `DrawnMap`                   | static   | map mint from world, path, read  |
|  [11]   | `Layers` (`Dictionary<string, MapLayer>`)              | `DrawnMap`                   | property | loaded layers by name            |

- `CoreHeatLandSeries<TModel>` declares one ctor, `(ICollection<TModel>? lands)`; the four arities are `HeatLandSeries<TModel>`'s own, and `HeatLandSeries : HeatLandSeries<HeatLand>` fixes the model to the shipped `HeatLand`. Bind a domain land type implementing `IWeigthedMapLand` as `HeatLandSeries<TLand>` directly, so an in-place `Value` write on a member of `Lands` IS the invalidation the deep observer redraws on — a projected parallel collection is never watched.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three assemblies stack behind one Avalonia surface: `LiveChartsCore` owns the chart math, the `ISeries`/`ICartesianAxis`/`IChartElement` model, the `Themes` rail, and the Skia draw kernel (`LiveChartsCore.Geo` carries `DrawnMap`/`MapProjection`/`IGeoMapView`); `LiveChartsCore.SkiaSharpView` owns the paint concretes, drawn visuals, `SKCharts`, and theme registration; this package owns the `UserControl` and XAML-markup layer over them.
- Chart controls are source-generated: each `SourceGen*` base carries every chart property as an `AvaloniaProperty`, and each public chart derives from its base (`MotionCanvas` derives from `UserControl` directly); bind the public control and read the property off the generated base, never reimplement `IChartView`.
- Every `Xaml*` shell implements the runtime contract it declares — `XamlLineSeries` is an `ISeries`, `BaseXamlAxis<T>` an `ICartesianAxis`, `XamlRectangularSection` an `IChartElement` — so the XAML element drops straight into `Series`, `XAxes`, or `Sections`; `WrappedSeries` stays protected, and code-behind reaching for the runtime type constructs a `LiveChartsCore.SkiaSharpView` series instead.
- `LiveCharts.DefaultSettings` is one process-wide instance and `LiveChartsSettings.HasTheme` replaces the whole `Theme`, so a single `LiveCharts.Configure` call seeds the theme through one `Add{Default,Light,Dark}Theme` and chains every `HasRuleFor*` onto that instance; `SourceGenChart.ChartTheme` overrides one control against the process theme.
- Theme rules are ordered builder lists, not overwrites: `Theme.ApplyStyleToSeries` folds `SeriesBuilder` then the family list over each series, and `Theme.GetSeriesColor` walks `Colors` by series index, so a palette swap re-colours the whole dashboard from one `ColorPalletes` row.
- `Theme.GetSeriesColor` is exactly `Colors[series.SeriesId % Colors.Length]`, so re-running `ApplyStyleToSeries` over already-attached series after a theme re-registration is deterministic and idempotent — a series keeps its ramp position across a swap rather than advancing one slot per re-apply, which is what makes a live re-tint of mounted charts a supported operation rather than a re-mount.
- `Coordinate.Empty` is the NULL POINT: `ChartPoint.IsEmpty` reads `Coordinate.IsEmpty`, the measure pass skips empty points when computing bounds, and `EnableNullSplitting` breaks a line into segments at them. A `Mapping` delegate returning `Coordinate.Empty` therefore renders a genuine gap, where returning a zero would draw a value the data never carried.
- `ISeries.DataLabelsFormatter` is `Func<ChartPoint, string>` over the UNTYPED point, and the bound model reaches it as `ChartPoint.Context.DataSource` (`object?`), so a formatter recovers its own row by pattern-matching that member. `ChartPoint.AsDataLabel` is NOT usable inside one: it resolves through `Context.Series.GetDataLabelText(this)`, which invokes the very formatter being defined, so a formatter reading it recurses until the stack ends.
- `LiveCharts.AsDate(this double ticks) -> DateTime` and `AsTimeSpan(this double ticks) -> TimeSpan` are the ONLY axis-value conversions and both clamp a negative input to zero; no `AsChartValue` inverse exists, so the outbound direction is `DateTime.Ticks` and a temporal value crossing into chart space carries no package helper.
- `LiveChartsCore.Measure.MeasureUnit` (`Pixels`, `ChartValues`) is the enum behind every `LocationUnit` property, and only `XamlNeedle` and `XamlAngularTicks` carry the axis-anchored placement trio `LocationUnit`/`ScalesXAt`/`ScalesYAt`. `XamlDrawnLabelVisual` and its `DrawnLabelVisual` base carry `X` and `Y` as raw `float` PIXELS with no measure unit and no axis index, so a drawn label anchored to a datum is re-placed by the caller on every pan, zoom, and re-range — the data-anchored planes are the section and the series, not the label visual.
- `Paint.IsStroke` is settable and backed by `PaintStyle`, so `SolidColorPaint(SKColor, float strokeWidth)` yields a stroke paint and `SolidColorPaint(SKColor)` a fill; `Color`, `IsStroke`, `StrokeThickness`, `PathEffect`, and `ZIndex` are all settable on a live paint, and the parameterless ctor yields one carrying none of them — so a mounted chart re-styles by writing the draw tasks it already holds, and every slot a mint set is a slot a re-style must set, since `PathEffect` held from a prior resolve dashes a row that never asked for one.
- `LiveCharts.MaxFps` sits beside `RenderingSettings` as the process frame ceiling; `RenderingSettings` itself carries `UseGPU`, `TryUseVSync`, `LiveChartsRenderLoopFPS`, and `ShowFPS`.

[STACKING]:
- `api-dynamicdata.md` (`DynamicData`): a `SourceCache.Connect().Transform(…).ToCollection()` or a bound `ObservableCollectionExtended` is the `Values` source on an `Xaml*Series`, so a chart redraws off the same change-set the grid and tiles read, without a copy.
- `api-dynamicdata.md` (`DynamicData.Aggregation`): `Count`/`Sum`/`Maximum`/`Minimum`/`StdDev` feed `XamlGaugeSeries.GaugeValue` and KPI labels off the same cache, so a tile and its chart never diverge.
- `api-skiasharp.md` (`SkiaSharp`): `SolidColorPaint(SKColor)`, `LinearGradientPaint(SKColor[], SKPoint, SKPoint, float[]?, SKShaderTileMode)`, and `SkiaPaint.SKTypeface` take Skia values directly, `LiveChartsSkiaSharp.AsSKColor`/`AsLvcColor` bridge both colour models, and `SkiaPaint.ConfigureSkiaSharpFont` seats an `SKFont` builder on every label paint.
- `api-skiasharp.md` (`SkiaSharp`): `InMemorySkiaSharpChart.GetImage() -> SKImage` and `SaveImage(Stream, SKEncodedImageFormat, int)` render a chart headless, so a report export and an on-screen panel share one series model.
- `api-avalonia-fluent.md` (`Avalonia.Themes.Fluent`): the app's light or dark variant selects `LvcThemeKind` on `AddDefaultTheme`, so the chart palette flips with the shell instead of carrying its own switch.
- within-lib: one shared `SyncContext` lock ties multiple charts' pointer, zoom, and animation passes onto one frame, so a synchronized dashboard pans together.

[LOCAL_ADMISSION]:
- AppUi admits a chart only as an `Xaml*` control whose `Values` binds a `DynamicData` projection and whose paints resolve through the process `Theme`, and rejects a bespoke Skia surface drawing chart semantics.

[RAIL_LAW]:
- Package: `LiveChartsCore.SkiaSharpView.Avalonia`
- Owns: the product chart rail — retained Avalonia charts, source-generated chart properties, XAML axes, series, gauges, sections, visual elements, Skia paint markup extensions, the process-wide theme, and headless chart export across panels, companion windows, sidecars, and diagnostics.
- Accept: chart intent maps to explicit series, axes, sections, visuals, legends, tooltips, and animation state through the generated property surface, paints declared as `*PaintExtension` markup or theme rules, and state stays data-driven off one chart rail.
- Reject: hand-drawn chart controls, a reimplemented `IChartView`, per-control colour literals a theme rule owns, one-off drawing code for chart semantics, and mutating the bound values collection outside the live-data rail.

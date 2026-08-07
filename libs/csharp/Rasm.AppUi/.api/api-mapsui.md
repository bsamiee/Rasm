# [RASM_APPUI_API_MAPSUI]

`Mapsui.Avalonia12` binds the Mapsui slippy-map engine to Avalonia: one `MapControl` drives one `Map` — an ordered `LayerCollection` under a `Navigator` camera — onto the shared Skia surface.

This catalog owns the control binding, the `Mapsui` core model, layer, style, thematic, widget, and projection stack, and the `Mapsui.Tiling` tile rail; `.api/api-mapsui-nts.md` owns NTS geometry, providers, and the editing session.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Mapsui.Avalonia12`
- package: `Mapsui.Avalonia12` (MIT)
- assembly: `Mapsui.UI.Avalonia` — package id and assembly id differ; ships `MapControl`, `RenderController`, `PointExtensions`
- namespace: `Mapsui.UI.Avalonia`, `Mapsui.UI.Avalonia.Extensions`, `Mapsui.Rendering`
- depends: `Avalonia`, `Avalonia.Skia`, `SkiaSharp`, `Svg.Skia`, `Topten.RichTextKit`, `Mapsui.Rendering.Skia`, `Mapsui.Tiling`, `BruTile`
- rail: map — the Avalonia viewport binding

[PACKAGE_SURFACE]: `Mapsui`
- package: `Mapsui` (MIT)
- assembly: `Mapsui` — zero package dependencies; pure managed
- namespace: `Mapsui`, `Mapsui.Layers`, `Mapsui.Styles`, `Mapsui.Widgets`, `Mapsui.Projections`, `Mapsui.Extensions`
- rail: map — model, camera, layers, features, styles, widgets, projection

[PACKAGE_SURFACE]: `Mapsui.Tiling`
- package: `Mapsui.Tiling` (MIT)
- assembly: `Mapsui.Tiling`
- namespace: `Mapsui.Tiling`, `Mapsui.Tiling.Layers`, `Mapsui.Tiling.Provider`, `Mapsui.Tiling.Fetcher`, `Mapsui.Tiling.Rendering`
- depends: `BruTile`, `Mapsui`
- rail: map — BruTile tile sources, tile layers, fetch planning, and render strategy

## [02]-[PUBLIC_TYPES]

[CONTROL_TYPE_SCOPE]: the Avalonia binding — `Mapsui.UI.Avalonia`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :----------------- | :------------ | :-------------------------- |
|  [01]   | `MapControl`       | class         | slippy-map viewport control |
|  [02]   | `RenderController` | sealed class  | render-loop driver          |
|  [03]   | `PointExtensions`  | static class  | Avalonia point conversion   |

- `MapControl` : `UserControl`, `IMapControl`, `IDisposable`, `INotifyPropertyChanged`; `RenderController` : `IDisposable`.
- `RenderController` declares in `Mapsui.Rendering` and `PointExtensions` in `Mapsui.UI.Avalonia.Extensions`, both inside the `Mapsui.UI.Avalonia` assembly.

[MODEL_TYPE_SCOPE]: the map model, camera, and geometry primitives — `Mapsui`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]              |
| :-----: | :---------------- | :-------------- | :------------------------ |
|  [01]   | `Map`             | class           | viewport state root       |
|  [02]   | `MapBuilder`      | class           | fluent map assembly       |
|  [03]   | `Navigator`       | class           | animated camera           |
|  [04]   | `LayerCollection` | class           | grouped ordered draw list |
|  [05]   | `Viewport`        | readonly struct | world window              |
|  [06]   | `MRect`           | class           | world rectangle           |
|  [07]   | `MPoint`          | class           | world point               |
|  [08]   | `MapInfo`         | class           | hit-test result           |
|  [09]   | `MapEventArgs`    | class           | pointer and tap payload   |
|  [10]   | `MBoxFit`         | enum            | box-fit vocabulary        |

- `Map` : `INotifyPropertyChanged`, `IDisposable`; `LayerCollection` : `IEnumerable<ILayer>`, drawing index zero first and the last entry on top.
- `MapEventArgs` and `MapInfoEventArgs` derive from `BaseEventArgs` : `HandledEventArgs`, carrying `ScreenPosition` `WorldPosition` `GestureType` `Map` `GetMapInfo` `GetRemoteMapInfoAsync` `Handled`.
- `MapInfo` resolves `Feature` `Layer` `Style` from the first `MapInfoRecord`, alongside `MapInfoRecords` `WorldPosition` `ScreenPosition` `Resolution`.
- support vocabularies: `Mapsui.Manipulations` `GestureType` `ScreenPosition` `Manipulation`; `Mapsui.Rendering` `RenderFormat` `RenderService`; `Mapsui.Animations` `Easing`; `Mapsui.Utilities` `Performance`; `Mapsui.Fetcher` `FetchMachine`; `Mapsui.Layers` `FetchInfo` `MSection`.
- `FetchInfo(MSection section, string? crs = null, ChangeType changeType = ChangeType.Discrete)` declares in `Mapsui.Layers`, not beside `FetchMachine`, and projects `Extent` and `Resolution` off its own section — the pair every `IProvider.GetFeaturesAsync` body reads its window and its LOD from.

[LAYER_TYPE_SCOPE]: the layer family — `Mapsui.Layers` unless a row names its own namespace

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]              |
| :-----: | :-------------------------- | :------------- | :------------------------ |
|  [01]   | `ILayer`                    | interface      | draw-stack member         |
|  [02]   | `BaseLayer`                 | abstract class | layer foundation          |
|  [03]   | `MemoryLayer`               | class          | direct feature set        |
|  [04]   | `WritableLayer`             | class          | in-place mutable set      |
|  [05]   | `ObservableMemoryLayer<T>`  | class          | observable-bound features |
|  [06]   | `GenericCollectionLayer<T>` | class          | typed collection features |
|  [07]   | `Layer`                     | class          | async provider overlay    |
|  [08]   | `ImageLayer`                | class          | async single-image source |
|  [09]   | `RasterizingLayer`          | class          | vectors baked to raster   |
|  [10]   | `AnimatedPointLayer`        | class          | interpolated point motion |
|  [11]   | `MyLocationLayer`           | class          | device-position marker    |
|  [12]   | `GridLayer`                 | class          | resolution grid overlay   |
|  [13]   | `TileLayer`                 | class          | BruTile tile basemap      |
|  [14]   | `RasterizingTileLayer`      | class          | vectors baked to tiles    |

- `AnimatedPointLayer` declares in `Mapsui.Layers.AnimatedLayers`; `TileLayer` and `RasterizingTileLayer` in `Mapsui.Tiling.Layers`.
- `ObservableMemoryLayer<T>` takes `Func<T, IFeature?>` at construction and rebinds on `ObservableCollection` change; `GenericCollectionLayer<T>` holds any `IEnumerable<IFeature>` in `Features`.
- `GridLayer` carries `LineColor` `LineWidth` and the `LayerRendererName` renderer key, drawn only inside its `MinVisible`/`MaxVisible` band.
- `ILayer` : `IAnimatable`, `INotifyPropertyChanged`, `IDisposable` and carries `Id` `Tag` `Name` `Enabled` `Opacity` `Style` `Extent` `MinVisible` `MaxVisible` `Busy` `Resolutions` `SortFeatures` `Attribution` `CustomLayerRendererName` `DataChanged` `GetFeatures(MRect, double)` `DataHasChanged()`.
- `BaseLayer.Attribution` is a `HyperlinkWidget`, so every layer contributes its own attribution chrome through `Map.GetWidgetsOfMapAndLayers()`.
- `RasterizingTileLayer` : `TileLayer` and `RasterizingLayer` : `BaseLayer` both expose `ISourceLayer.SourceLayer`, wrapping a vector layer as cached imagery.

[FEATURE_TYPE_SCOPE]: the feature model — `Mapsui` and `Mapsui.Layers`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CAPABILITY]               |
| :-----: | :-------------- | :------------- | :------------------------- |
|  [01]   | `IFeature`      | interface      | feature contract           |
|  [02]   | `BaseFeature`   | abstract class | field and style carrier    |
|  [03]   | `PointFeature`  | class          | located marker or label    |
|  [04]   | `RasterFeature` | class          | georeferenced tile image   |
|  [05]   | `MRaster`       | class          | raster payload and bounds  |
|  [06]   | `IProvider`     | interface      | asynchronous feature fetch |

- `IFeature` : `ICloneable` and carries `Id` `GenerationId` `Styles` `Fields` `Extent` `Data` `this[string]` `Modified()` `CoordinateVisitor(Action<double,double,CoordinateSetter>)`.
- `PointFeature` constructs from `MPoint`, `(double x, double y)`, a `(double, double)` tuple, or a copy source.
- `IProvider` carries `CRS`, `GetExtent() -> MRect?`, and `GetFeaturesAsync(FetchInfo)`; every concrete provider, the geometry-backed feature, and the editing session are `.api/api-mapsui-nts.md`'s.

[STYLE_TYPE_SCOPE]: presentation values — `Mapsui.Styles` and `Mapsui.Styles.Thematics`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]          | [CAPABILITY]                |
| :-----: | :----------------- | :--------------------- | :-------------------------- |
|  [01]   | `IStyle`           | interface              | style contract              |
|  [02]   | `BaseStyle`        | abstract class         | opacity and zoom band       |
|  [03]   | `VectorStyle`      | class                  | line, outline, and fill     |
|  [04]   | `SymbolStyle`      | class                  | built-in point symbol       |
|  [05]   | `ImageStyle`       | class                  | image or SVG marker         |
|  [06]   | `CustomPointStyle` | class                  | caller-drawn point          |
|  [07]   | `LabelStyle`       | class                  | text label                  |
|  [08]   | `CalloutStyle`     | class                  | callout balloon             |
|  [09]   | `RasterStyle`      | class                  | tile and raster paint       |
|  [10]   | `StyleCollection`  | class                  | style composition           |
|  [11]   | `ThemeStyle`       | class                  | per-feature style selection |
|  [12]   | `GradientTheme`    | class                  | value-interpolated style    |
|  [13]   | `ColorBlend`       | class                  | positioned colour ramp      |
|  [14]   | `Pen`              | class                  | stroke value                |
|  [15]   | `Brush`            | class                  | fill value                  |
|  [16]   | `Color`            | readonly record struct | colour value                |
|  [17]   | `Font`             | class                  | typeface value              |
|  [18]   | `Image`            | class                  | source-keyed image value    |

- `IStyle` carries `GenerationId` `MinVisible` `MaxVisible` `Enabled` `Opacity` `Modified()`, so every style — `RasterStyle` included — takes opacity and a zoom band from `BaseStyle`.
- `RasterStyle` adds `Outline` alone; `VectorStyle` composes `Line` `Outline` `Fill`; `SymbolStyle` : `VectorStyle` and `CalloutStyle` : `ImageStyle` : `BasePointStyle`.
- `ThemeStyle` and `GradientTheme` both implement `IThemeStyle`, resolving through `GetStyle(IFeature, Viewport)`.
- `Color` presets ship as static properties over the CSS name set and `Transparent`; `Image.BlendModeColor` tints a symbol image and `Image.SvgFillColor`/`SvgStrokeColor` recolour an SVG source.
- vocabularies: `SymbolType` `UnitType` `PenStyle` `PenStrokeCap` `StrokeJoin` `FillStyle` `CalloutType` `TailAlignment`; `LabelStyle` nests `HorizontalAlignmentEnum` `VerticalAlignmentEnum` `LineBreakMode`.

[WIDGET_TYPE_SCOPE]: screen-anchored chrome — four sibling namespaces under `Mapsui.Widgets`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]              |
| :-----: | :----------------------- | :------------- | :------------------------ |
|  [01]   | `IWidget`                | interface      | screen overlay contract   |
|  [02]   | `BaseWidget`             | abstract class | alignment and input base  |
|  [03]   | `InputOnlyWidget`        | abstract class | input without paint       |
|  [04]   | `BoxWidget`              | class          | rounded filled box        |
|  [05]   | `TextBoxWidget`          | class          | text inside a box         |
|  [06]   | `ButtonWidget`           | class          | tappable text box         |
|  [07]   | `ImageButtonWidget`      | class          | icon button               |
|  [08]   | `HyperlinkWidget`        | class          | URL-opening button        |
|  [09]   | `ZoomInOutWidget`        | class          | plus and minus zoom pad   |
|  [10]   | `ScaleBarWidget`         | class          | scale bar                 |
|  [11]   | `MapInfoWidget`          | class          | tapped-feature readout    |
|  [12]   | `MouseCoordinatesWidget` | class          | pointer world coordinates |
|  [13]   | `RulerWidget`            | class          | drag distance measure     |
|  [14]   | `LoggingWidget`          | class          | in-map log entries        |
|  [15]   | `PerformanceWidget`      | class          | draw-timing readout       |
|  [16]   | `IUnitConverter`         | interface      | scale-bar unit vocabulary |

- `Mapsui.Widgets`: `IWidget` `BaseWidget` `InputOnlyWidget` `WidgetEventArgs` `WidgetInput` `Alignment` `HorizontalAlignment` `VerticalAlignment` `Orientation` `InputAreaType` `ActiveMode`
- `Mapsui.Widgets.BoxWidgets`: `BoxWidget` `TextBoxWidget`
- `Mapsui.Widgets.ButtonWidgets`: `ButtonWidget` `ImageButtonWidget` `HyperlinkWidget` `ZoomInOutWidget`
- `Mapsui.Widgets.InfoWidgets`: `MapInfoWidget` `MouseCoordinatesWidget` `RulerWidget` `LoggingWidget` `PerformanceWidget`
- `Mapsui.Widgets.ScaleBar`: `ScaleBarWidget` `ScaleBarMode` `IUnitConverter` `MetricUnitConverter` `ImperialUnitConverter` `NauticalUnitConverter`
- inheritance: `BoxWidget` : `BaseWidget`; `TextBoxWidget` : `BoxWidget`; `ButtonWidget` : `TextBoxWidget`; `ImageButtonWidget` : `BoxWidget`; `HyperlinkWidget` and `PerformanceWidget` : `ButtonWidget`; `MapInfoWidget` `MouseCoordinatesWidget` `LoggingWidget` : `TextBoxWidget`; `ZoomInOutWidget` `RulerWidget` `ScaleBarWidget` : `BaseWidget`.
- each unit converter exposes `MeterRatio` `ScaleBarValues` `GetScaleText(int)` behind a static `Instance` singleton.

[PROJECTION_TYPE_SCOPE]: CRS reprojection — `Mapsui.Projections`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]            |
| :-----: | :--------------------- | :------------ | :---------------------- |
|  [01]   | `SphericalMercator`    | class         | web-mercator bridge     |
|  [02]   | `IProjection`          | interface     | projection contract     |
|  [03]   | `Projection`           | class         | registry-backed engine  |
|  [04]   | `ProjectionDefaults`   | class         | ambient projection slot |
|  [05]   | `CrsHelper`            | static class  | CRS string parsing      |
|  [06]   | `CrsAxisOrderRegistry` | class         | per-CRS axis order      |

- `SphericalMercator` exposes static conversions only; `Projection` : `IProjection` covers the mercator and WGS-84 pair and reports coverage through `IsProjectionSupported`.
- `CrsIdentifier` and `CrsType` carry the parsed authority and code `CrsHelper` yields.

[TILING_TYPE_SCOPE]: tile sourcing, fetch, and render strategy — `Mapsui.Tiling`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :--------------------------- | :------------ | :--------------------------- |
|  [01]   | `OpenStreetMap`              | static class  | OSM XYZ source factory       |
|  [02]   | `TmsTileSourceBuilder`       | static class  | TMS capabilities parse       |
|  [03]   | `RasterizingTileSource`      | class         | vector layer as tile source  |
|  [04]   | `IDataFetchStrategy`         | interface     | tile-set selection contract  |
|  [05]   | `DataFetchStrategy`          | class         | level-walking tile selection |
|  [06]   | `MinimalDataFetchStrategy`   | class         | single-level tile selection  |
|  [07]   | `IRenderFetchStrategy`       | interface     | draw-set selection contract  |
|  [08]   | `RenderFetchStrategy`        | class         | cached-tile draw set         |
|  [09]   | `MinimalRenderFetchStrategy` | class         | current-level draw set       |
|  [10]   | `TilingRenderFetchStrategy`  | class         | seam-free tile draw set      |
|  [11]   | `TileFetchPlanner`           | class         | fetch job scheduling         |
|  [12]   | `TileFetchStatus`            | enum          | per-tile fetch state         |
|  [13]   | `HttpClientTools`            | static class  | default user-agent string    |

- `TileFetchPlanner` : `IFetchableSource`, `INotifyPropertyChanged` and exposes `NumberTilesNeeded` `Busy` `GetFetchJobs(int, int)` `ViewportChanged(FetchInfo)` `ClearCache()` under the static `DefaultNumberOfSimultaneousFetches`.
- `TileFetchStatus` carries `PermanentlyUnavailable` (the source confirmed the tile does not exist) and `GaveUp` (retries exhausted) — the two TERMINAL verdicts only; in-flight and success states ride `TileLayer.Busy` and the `DataChanged` payload instead.
- `TileLayer` : `BaseLayer`, `IFetchableSource` over `TileLayer(ITileSource, int minTiles = 200, int maxTiles = 300, IDataFetchStrategy?, IRenderFetchStrategy?, int minExtraTiles = -1, int maxExtraTiles = -1, Func<TileInfo, Task<IFeature?>>?, HttpClient?)`, exposing `TileSource` `ClearCache()` `GetFetchJobs(int, int)` and the `FetchRequested` event; the ctor SEEDS `BaseLayer.Attribution.Text`/`Url` from `ITileSource.Attribution`, mirrors the planner's `Busy` onto its own, and forwards each fetch outcome as `OnDataChanged(new DataChangedEventArgs(ex, Name))`.
- The `fetchTileAsFeature` hook answers ONE `IFeature?` per tile into a `MemoryCache<IFeature?>`, and the ctor seats `Style = new RasterStyle()` — so the layer is a RASTER carrier by construction and a vector-tile payload decoding to a feature SET rides an `IProvider` on a vector `Layer`, never this hook. `HttpTileSource.GetTileAsync` THROWS when neither the `HttpClient` default headers nor `ConfigureHttpRequestMessage` supply a `User-Agent`, so the agent write is an admission requirement rather than a courtesy.
- `Mapsui.Fetcher.DataChangedEventArgs(Exception? error, string layerName)` carries `Error` and `LayerName`, so a fetch failure and a fetch success arrive on ONE event distinguished by a nullable payload.
- `HttpClientTools.GetDefaultApplicationUserAgent()` supplies the user-agent `OpenStreetMap.CreateTileLayer` sends when the caller passes none; a per-source agent rides `HttpTileSource.ConfigureHttpRequestMessage` or an injected `HttpClient` on the `TileLayer` ctor.

[BRUTILE_SOURCE_SCOPE]: the tile-source contract `Mapsui.Tiling` composes — `BruTile`, `BruTile.Web`, `BruTile.Predefined`, `BruTile.Cache`

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `new HttpTileSource(ITileSchema, string urlFormatter, …)`                      | ctor     | XYZ/TMS source from a template      |
|  [02]   | `new HttpTileSource(ITileSchema, IUrlBuilder, …)`                              | ctor     | source over a custom URL builder    |
|  [03]   | `HttpTileSource.PersistentCache`                                               | property | settable `IPersistentCache<byte[]>` |
|  [04]   | `HttpTileSource.Attribution`                                                   | property | settable `Attribution` credit       |
|  [05]   | `HttpTileSource.ConfigureHttpRequestMessage`                                   | property | per-request header hook             |
|  [06]   | `new Attribution(string Text = "", string Url = "")`                           | ctor     | credit value                        |
|  [07]   | `new GlobalSphericalMercator(format, YAxis, minZoomLevel, maxZoomLevel, name)` | ctor     | EPSG:3857 tile schema               |
|  [08]   | `new FileCache(string directory, string format, TimeSpan cacheExpireTime)`     | ctor     | on-disk persistent tile cache       |
|  [09]   | `NullCache`                                                                    | class    | the no-op default cache             |

- `HttpTileSource` : `IHttpTileSource`, `ITileSource`, `IUrlBuilder`; the template arity forwards to `BasicUrlBuilder(urlFormatter, serverNodes, apiKey)`, and `GetTileAsync` reads the persistent cache first and writes every fetched tile back into it, so an offline session serves from `FileCache` with no second code path.
- `Attribution` is a `record struct` with defaulted members, so an absent credit is an EMPTY string rather than a null — a caller requiring attribution tests the text, never the reference.
- `GlobalSphericalMercator` : `TileSchema`; its ctor arities strip `(format, yAxis, minZoomLevel, maxZoomLevel, name)` from the left and one further arity takes an explicit `zoomLevels` set with an `Extent`, every parameter defaulted — so a zoom-band-only call resolves to the narrowest `(minZoomLevel, maxZoomLevel, name)` arity by the fewest-omitted-optionals rule and takes `"png"` with `YAxis.OSM`, the slippy-map orientation every `{z}/{x}/{y}` template assumes.
- `IPersistentCache<T>` : `ITileCache<T>` adds no member of its own — it is the marker distinguishing a durable store from the in-memory `MemoryCache<T>` the layer keeps around the viewport.

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRY_SCOPE]: the `MapControl` binding surface — `Mapsui.UI.Avalonia`

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `MapControl.Map`                                                  | property | model binding, via `MapProperty`   |
|  [02]   | `MapControl.MapProperty`                                          | static   | `DirectProperty<MapControl,Map>`   |
|  [03]   | `MapControl.Info`                                                 | event    | `EventHandler<MapInfoEventArgs>`   |
|  [04]   | `MapControl.MapTapped`                                            | event    | `EventHandler<MapEventArgs>`       |
|  [05]   | `MapControl.MapPointerPressed`                                    | event    | pointer press                      |
|  [06]   | `MapControl.MapPointerMoved`                                      | event    | pointer movement                   |
|  [07]   | `MapControl.MapPointerReleased`                                   | event    | pointer release                    |
|  [08]   | `MapControl.GetMapInfo(ScreenPosition, IEnumerable<ILayer>)`      | instance | feature hit test at a screen point |
|  [09]   | `MapControl.GetSnapshot(IEnumerable<ILayer>?, RenderFormat, int)` | instance | map or layer image capture         |
|  [10]   | `MapControl.Refresh(ChangeType)`                                  | instance | data refetch and redraw            |
|  [11]   | `MapControl.RefreshData(ChangeType)`                              | instance | data refetch                       |
|  [12]   | `MapControl.RefreshGraphics()`                                    | instance | canvas invalidation                |
|  [13]   | `MapControl.InvalidateCanvas()`                                   | instance | immediate canvas invalidation      |
|  [14]   | `MapControl.ForceUpdate()`                                        | instance | forced update                      |
|  [15]   | `MapControl.SetMapRenderer(IMapRenderer)`                         | instance | renderer override                  |
|  [16]   | `MapControl.GetPixelDensity()`                                    | instance | device scale factor                |
|  [17]   | `MapControl.ClearTouchState()`                                    | instance | manipulation reset                 |
|  [18]   | `MapControl.Unsubscribe()`                                        | instance | event detach                       |
|  [19]   | `MapControl.UseContinuousMouseWheelZoom`                          | property | wheel-zoom mode (`bool`)           |
|  [20]   | `MapControl.ContinuousMouseWheelZoomStepSize`                     | property | wheel step (`double`)              |
|  [21]   | `MapControl.UseFling`                                             | property | fling mode (`bool`)                |
|  [22]   | `MapControl.MaxTapGestureMovement`                                | property | tap threshold (`int`)              |
|  [23]   | `PointExtensions.ToScreenPosition(Point)`                         | static   | Avalonia point to map position     |

- `MapControl.GetSnapshot`: `layers` defaults to the whole map and `renderFormat` to `RenderFormat.Png`; `Skp` yields a Skia picture stream rather than a bitmap.

[MODEL_ENTRY_SCOPE]: the map model and camera — `Mapsui`

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new Map()`                                                          | ctor     | model construction                                   |
|  [02]   | `Map.CRS`                                                            | property | coordinate system (`string?`, default `"EPSG:3857"`) |
|  [03]   | `Map.Extent`                                                         | property | joined layer bounds (`MRect?`)                       |
|  [04]   | `Map.BackColor`                                                      | property | backdrop (`Mapsui.Styles.Color`)                     |
|  [05]   | `Map.Layers`                                                         | property | `LayerCollection` draw stack                         |
|  [06]   | `Map.Widgets`                                                        | property | `ConcurrentQueue<IWidget>` chrome                    |
|  [07]   | `Map.Navigator`                                                      | property | camera owner                                         |
|  [08]   | `Map.RenderService`                                                  | property | drawable, tile, image, and font caches               |
|  [09]   | `Map.Performance`                                                    | property | rolling draw-time statistics                         |
|  [10]   | `Map.FetchMachine`                                                   | property | fetch worker pool                                    |
|  [11]   | `Map.RefreshDataAsync(Viewport?)`                                    | instance | awaited data fetch                                   |
|  [12]   | `Map.RefreshData(ChangeType, Viewport?)`                             | instance | fire-and-forget data fetch                           |
|  [13]   | `Map.RefreshGraphics()`                                              | instance | full-surface redraw request                          |
|  [14]   | `Map.RefreshGraphics(MRect, CoordinateSpace)`                        | instance | dirty-rect redraw request                            |
|  [15]   | `Map.ClearCache()`                                                   | instance | tile and feature cache reset                         |
|  [16]   | `Map.AbortFetch()`                                                   | instance | in-flight fetch cancel                               |
|  [17]   | `Map.UpdateAnimations()`                                             | instance | per-frame layer animation tick                       |
|  [18]   | `Map.GetWidgetsOfMapAndLayers()`                                     | instance | enabled chrome and layer attributions                |
|  [19]   | `Map.Tapped` / `PointerPressed` / `PointerMoved` / `PointerReleased` | event    | model-level pointer stream                           |
|  [20]   | `Navigator.CenterOn(MPoint, long, Easing?)`                          | instance | animated pan                                         |
|  [21]   | `Navigator.ZoomTo(double, long, Easing?)`                            | instance | resolution zoom                                      |
|  [22]   | `Navigator.ZoomToBox(MRect?, MBoxFit, long, Easing?)`                | instance | box fit                                              |
|  [23]   | `Navigator.ZoomToPanBounds(MBoxFit, long, Easing?)`                  | instance | full-extent fit                                      |
|  [24]   | `Navigator.ZoomToLevel(int)`                                         | instance | level zoom                                           |
|  [25]   | `Navigator.ZoomIn(long, Easing?)`                                    | instance | zoom increment                                       |
|  [26]   | `Navigator.ZoomOut(long, Easing?)`                                   | instance | zoom decrement                                       |
|  [27]   | `Navigator.CenterOnAndZoomTo(MPoint, double, long, Easing?)`         | instance | combined pan and zoom                                |
|  [28]   | `Navigator.FlyTo(MPoint, double, long)`                              | instance | parabolic move                                       |
|  [29]   | `Navigator.RotateTo(double, long, Easing?)`                          | instance | bearing rotation                                     |
|  [30]   | `Navigator.Fling(double, double, long)`                              | instance | inertial pan                                         |
|  [31]   | `Navigator.SetViewport(Viewport, long, Easing?)`                     | instance | whole-viewport set                                   |
|  [32]   | `Navigator.PanLock` / `ZoomLock` / `RotationLock`                    | property | per-axis manipulation lock                           |
|  [33]   | `Navigator.OverridePanBounds` / `OverrideZoomBounds`                 | property | caller-pinned pan and zoom limits                    |
|  [34]   | `Navigator.OverrideResolutions`                                      | property | caller-pinned resolution ladder                      |

- `Navigator.CenterOn`, `ZoomIn`, `ZoomOut`, and `ZoomTo` each carry a second arity taking `(double x, double y, …)` or a `ScreenPosition` anchor; `duration` defaults to `-1` for an immediate move.
- `Map.BackColor` takes `Mapsui.Styles.Color` and starts at `Color.White`, so a transparent backdrop is an explicit `Color.Transparent` write.
- `Map.Widgets` is a `ConcurrentQueue<IWidget>`; `Add`, `AddRange`, and `Clear` arrive from `Mapsui.Extensions.ConcurrentQueueExtensions`, so widget mounting requires that `using`.
- `Map` seats a `LoggingWidget` and a `PerformanceWidget` at construction, both gated by `ActiveMode.OnlyInDebugMode`.

[BUILDER_ENTRY_SCOPE]: fluent map assembly — `Mapsui` and `Mapsui.Tiling.Extensions`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `new MapBuilder()`                                                 | ctor     | builder construction         |
|  [02]   | `MapBuilder.WithMapConfiguration(ConfigureMap)`                    | instance | post-assembly map mutation   |
|  [03]   | `MapBuilder.WithLayer(AddLayer, ConfigureLayer?)`                  | instance | overlay row registration     |
|  [04]   | `MapBuilder.WithBaseLayer(AddLayer)`                               | instance | basemap row registration     |
|  [05]   | `MapBuilder.WithWidget(AddWidget, ConfigureWidget)`                | instance | chrome row registration      |
|  [06]   | `MapBuilder.WithZoomButtons()`                                     | instance | preset `ZoomInOutWidget` row |
|  [07]   | `MapBuilder.WithMapCRS(string)`                                    | instance | target CRS                   |
|  [08]   | `MapBuilder.Build() -> Map`                                        | instance | map realization              |
|  [09]   | `MapBuilderExtensions.WithOpenStreetMapLayer(ConfigureLayer)`      | static   | OSM tile row                 |
|  [10]   | `MapBuilderExtensions.WithScaleBarWidget(ConfigureScaleBarWidget)` | static   | typed scale-bar row          |

- `MapBuilder.WithBaseLayer`: `Build()` never consumes the registered factory, so a basemap mounts through `WithLayer` or `WithOpenStreetMapLayer` and stays first in registration order.
- `MapBuilder` nests its delegate vocabulary — `AddLayer(Map) -> ILayer`, `AddWidget(Map) -> IWidget`, `ConfigureMap(Map)`, `ConfigureLayer(ILayer, Map)`, `ConfigureWidget(IWidget)` — and `WithWidget` also carries the single-argument arity.
- `Build()` runs layers, then widgets, then map configurators, so a `WithMapConfiguration` body observes the fully populated stack.

[LAYER_ENTRY_SCOPE]: draw-stack construction and ordering

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `LayerCollection.Add(ILayer, int group)`                                   | instance | append into a group               |
|  [02]   | `LayerCollection.Add(IEnumerable<ILayer>, int group)`                      | instance | bulk append                       |
|  [03]   | `LayerCollection.AddOnTop(ILayer, int group)`                              | instance | topmost insert                    |
|  [04]   | `LayerCollection.AddOnBottom(ILayer, int group)`                           | instance | bottom insert                     |
|  [05]   | `LayerCollection.Insert(int, ILayer, int group)`                           | instance | positional insert                 |
|  [06]   | `LayerCollection.Get(int, int group) -> ILayer`                            | instance | positional read                   |
|  [07]   | `LayerCollection.GetLayers(int group)`                                     | instance | one group in draw order           |
|  [08]   | `LayerCollection.GetLayersOfAllGroups()`                                   | instance | every group in draw order         |
|  [09]   | `LayerCollection.Move(int, ILayer)`                                        | instance | reposition                        |
|  [10]   | `LayerCollection.MoveToTop(ILayer)` / `MoveToBottom(ILayer)`               | instance | extreme reposition                |
|  [11]   | `LayerCollection.MoveUp(ILayer)` / `MoveDown(ILayer)`                      | instance | single-step reposition            |
|  [12]   | `LayerCollection.Remove(ILayer)`                                           | instance | drop one layer                    |
|  [13]   | `LayerCollection.Remove(Func<ILayer, bool>)`                               | instance | predicate drop                    |
|  [14]   | `LayerCollection.Modify(Func<ILayer,bool>, IEnumerable<ILayer>)`           | instance | atomic swap, one Changed event    |
|  [15]   | `LayerCollection.FindLayer(string)`                                        | instance | lookup by name                    |
|  [16]   | `LayerCollection.Clear(int group)` / `ClearAllGroups()`                    | instance | group or whole-stack reset        |
|  [17]   | `LayerCollection.Changed`                                                  | event    | added and removed layer batch     |
|  [18]   | `OpenStreetMap.CreateTileLayer(string? userAgent)`                         | static   | named OSM tile layer              |
|  [19]   | `OpenStreetMap.DefaultCache`                                               | static   | `IPersistentCache<byte[]>?` field |
|  [20]   | `new TileLayer(ITileSource, …)`                                            | ctor     | any BruTile source as a layer     |
|  [21]   | `new RasterizingTileLayer(ILayer, …)`                                      | ctor     | vector layer baked to tiles       |
|  [22]   | `new RasterizingTileSource(ILayer, …)`                                     | ctor     | vector layer as a tile source     |
|  [23]   | `TmsTileSourceBuilder.BuildAsync(string, bool, IPersistentCache<byte[]>?)` | static   | TMS endpoint to `ITileSource`     |
|  [24]   | `new MemoryLayer(string?) { Features, Style }`                             | ctor     | in-memory feature overlay         |
|  [25]   | `new WritableLayer { Style }`                                              | ctor     | mutable feature set for authoring |
|  [26]   | `new Layer(string?) { DataSource, Style }`                                 | ctor     | async provider-fed vector overlay |
|  [27]   | `new RasterizingLayer(ILayer, int, IMapRenderer?, float, RenderFormat)`    | ctor     | vectors baked to one raster       |
|  [28]   | `new AnimatedPointLayer(IProvider) { AnimationDuration, Easing }`          | ctor     | interpolated point motion         |

- Every group-taking member defaults to group `0`; a higher group always draws above a lower one, so widgets-as-layers and pinned overlays ride a group rather than an index.
- `OpenStreetMap.DefaultCache` writes before the first `CreateTileLayer` call — the source captures the field value at construction.
- `WritableLayer` carries `Add(IFeature)`, `AddRange(IEnumerable<IFeature>)`, `Find(IFeature) -> IFeature?`, `TryRemove(IFeature, Func<IFeature,IFeature,bool>?)`, `Clear()`, and `GetFeatures()` — the shape `Mapsui.Nts` `EditManager` authors onto.
- `Layer(string layerName)` is the primary ctor forwarding to `BaseLayer(layerName)`; the parameterless arity chains it with the literal `"Layer"`, leaving the settable `BaseLayer.Name` as the only other naming path.
- `TileLayer` and `RasterizingTileLayer` accept `IDataFetchStrategy` and `IRenderFetchStrategy` overrides — `MinimalDataFetchStrategy` fetches the current level alone, `TilingRenderFetchStrategy` fills seams from coarser cached levels.

[STYLE_ENTRY_SCOPE]: style, theme, and projection construction

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `new VectorStyle { Line, Outline, Fill }`                      | ctor     | vector paint                            |
|  [02]   | `new RasterStyle { Outline, Opacity }`                         | ctor     | tile and raster paint                   |
|  [03]   | `ImageStyles.CreatePinStyle(Color?, Color?, double)`           | static   | built-in pin marker style               |
|  [04]   | `new Pen(Color, double)`                                       | ctor     | stroke value                            |
|  [05]   | `new Brush(Color)`                                             | ctor     | fill value                              |
|  [06]   | `Color.FromArgb(int, int, int, int)`                           | static   | ARGB colour                             |
|  [07]   | `Color.FromString(string)`                                     | static   | hex or CSS-name colour                  |
|  [08]   | `Color.FromHsl(float, float, float, int)`                      | static   | HSL colour                              |
|  [09]   | `Color.Opacity(Color, float?)`                                 | static   | alpha-adjusted copy                     |
|  [10]   | `new ThemeStyle(Func<IFeature, Viewport, IStyle?>)`            | ctor     | per-feature style from attribute + zoom |
|  [11]   | `new GradientTheme(string, double, double, IStyle, IStyle)`    | ctor     | column-value interpolated style         |
|  [12]   | `GradientTheme.GetStyle(IFeature, Viewport)`                   | instance | resolved interpolated style             |
|  [13]   | `new ColorBlend(Color[], double[])`                            | ctor     | positioned colour ramp                  |
|  [14]   | `ColorBlend.GetColor(double)`                                  | instance | ramp sample at a position               |
|  [15]   | `ColorBlend.TwoColors(Color, Color)`                           | static   | two-stop ramp                           |
|  [16]   | `ColorBlend.ThreeColors(Color, Color, Color)`                  | static   | three-stop ramp                         |
|  [17]   | `SphericalMercator.FromLonLat(double, double)`                 | static   | WGS-84 to EPSG:3857                     |
|  [18]   | `SphericalMercator.ToLonLat(double, double)`                   | static   | EPSG:3857 to WGS-84                     |
|  [19]   | `ProjectionDefaults.Projection`                                | static   | ambient `IProjection` slot              |
|  [20]   | `IProjection.Project(string, string, MPoint)`                  | instance | in-place point reprojection             |
|  [21]   | `IProjection.IsProjectionSupported(string?, string?)`          | instance | CRS pair coverage probe                 |
|  [22]   | `ViewportExtensions.ScreenToWorldXY(Viewport, double, double)` | static   | screen point to world coordinate pair   |
|  [23]   | `ViewportExtensions.WorldToScreenXY(Viewport, double, double)` | static   | world coordinate pair to screen point   |

- `ThemeStyle` also ships the `Func<IFeature, IStyle?>` arity where viewport-relative styling is not needed.
- `GradientTheme` interpolates only when `MinStyle` and `MaxStyle` share one type drawn from `VectorStyle`, `ImageStyle`, or `LabelStyle`, and only over a numeric column value; every other pairing throws.
- `GradientTheme` carries init-only `ColumnName` `Min` `Max` `MinStyle` `MaxStyle` `TextColorBlend` `LineColorBlend` `FillColorBlend`, so colour follows a ramp while width and offset interpolate linearly.
- `ColorBlend` presets: `Rainbow7` `Rainbow5` `BlackToWhite` `WhiteToBlack` `RedToGreen` `GreenToRed` `BlueToGreen` `GreenToBlue` `RedToBlue` `BlueToRed`; `Colors` and `Positions` stay settable for a custom ramp.
- `SphericalMercator.FromLonLat` and `ToLonLat` each carry an `MPoint` arity beside the tuple arity.
- `Viewport` is a `readonly record struct` of `CenterX` `CenterY` `Resolution` `Rotation` `Width` `Height` and arithmetic operators alone — every screen-world crossing is a `Mapsui.Extensions.ViewportExtensions` member, so a pointer sample reaching world coordinates without a hit test rides `ScreenToWorldXY` off `Map.Navigator.Viewport`; `Mapsui.Tiling.Extensions` carries the `MRect.ToExtent()`/`Extent.ToMRect()` pair the BruTile schema and the Mapsui window meet through.

[WIDGET_ENTRY_SCOPE]: chrome construction and property surface

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `new ScaleBarWidget(Map, IProjection?)`                     | ctor     | scale bar bound to one map         |
|  [02]   | `new MapInfoWidget(Map, IEnumerable<ILayer>)`               | ctor     | readout over a fixed layer set     |
|  [03]   | `new MapInfoWidget(Map, Func<ILayer, bool>)`                | ctor     | readout over a layer predicate     |
|  [04]   | `new MapInfoWidget(Map, Func<IEnumerable<ILayer>>)`         | ctor     | readout over a live layer selector |
|  [05]   | `new LoggingWidget(Action refreshGraphics)`                 | ctor     | in-map log sink                    |
|  [06]   | `new PerformanceWidget(Performance)`                        | ctor     | draw-timing readout                |
|  [07]   | `new ZoomInOutWidget()`                                     | ctor     | zoom pad                           |
|  [08]   | `new RulerWidget()`                                         | ctor     | drag distance measure              |
|  [09]   | `new MouseCoordinatesWidget()`                              | ctor     | pointer world coordinates          |
|  [10]   | `new BoxWidget()` / `new TextBoxWidget()`                   | ctor     | box and text-box chrome            |
|  [11]   | `new ButtonWidget()` / `new ImageButtonWidget()`            | ctor     | tappable chrome                    |
|  [12]   | `new HyperlinkWidget { Url }`                               | ctor     | URL-opening chrome                 |
|  [13]   | `BaseWidget.UpdateEnvelope(double, double, double, double)` | instance | hit-box recompute                  |
|  [14]   | `RulerWidget.SnapToFeature(Func<MPoint?, IFeature?>)`       | instance | endpoint snap to features          |
|  [15]   | `RulerWidget.Reset()`                                       | instance | clear both endpoints               |
|  [16]   | `LoggingWidget.ShowLoggingInMap`                            | static   | global `ActiveMode` gate           |
|  [17]   | `Performance.DefaultIsActive`                               | static   | global timing `ActiveMode` gate    |

- `ScaleBarWidget` and `MapInfoWidget` bind their `Map` at construction and expose no parameterless arity — a widget row factory must close over the map it decorates.
- `BaseWidget`: `HorizontalAlignment` `VerticalAlignment` `Margin` `Position` `Width` `Height` `Envelope` `Enabled` `InputAreaType` `InputTransparent`, events `Tapped` `PointerPressed` `PointerMoved` `PointerReleased`, and init-only `WithTappedEvent` `WithPointerPressedEvent` `WithPointerMovedEvent` `WithPointerReleased`.
- `BoxWidget`: `CornerRadius` `BackColor` (`Color?`) `Opacity`. `TextBoxWidget` adds `Padding` `Text` `TextSize` `TextColor` `Font` — a set `Font.Size` overrides `TextSize`.
- `ZoomInOutWidget`: `Size` `Orientation` `StrokeColor` `TextColor` `BackColor` `Opacity`; a tap on the leading half zooms in, the trailing half out, over a 500 ms animation.
- `ScaleBarWidget`: `MaxWidth` `TextColor` `Halo` `StrokeWidth` `StrokeWidthHalo` `TickLength` `TextAlignment` `Font` `UnitConverter` `SecondaryUnitConverter` `ScaleBarMode` `ShowEnvelop` `Scale` `TextMargin`; layout folds through `GetScaleBarLengthAndText(Viewport)` `GetScaleBarLinePositions(…)` `GetScaleBarTextPositions(…)` `CanProject()`.
- `ScaleBarMode.Both` draws the secondary bar, so `SecondaryUnitConverter` decides nothing under `Single`; `UnitConverter` starts at `MetricUnitConverter.Instance`.
- `MapInfoWidget.FeatureToText` is a settable `Func<IFeature?, string>` — the whole readout body is one caller projection, never a per-field knob.
- `RulerWidget`: `Color` `ColorOfBeginAndEndDots` `IsActive` `ShowInfoNextToRuler` `InfoBox` with read-only `StartPosition` `CurrentPosition` `DistanceInKilometers` and the `DistanceUpdated` event; distance folds through `SphericalMercator.ToLonLat` and haversine, so it reads kilometres over the sphere.
- `LoggingWidget`: `LogLevelFilter` `MaxNumberOfLogEntriesToKeep` `ErrorTextColor` `WarningTextColor` `InformationTextColor` `ListOfLogEntries`; `ImageButtonWidget`: `Padding` `Image` `Rotation` over a transparent `BackColor`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every draw folds through one `Map`: its grouped `LayerCollection` painted by the transitive `Mapsui.Rendering.Skia` renderer, `Map.Widgets` painted as screen-space chrome above it, and the control only binding and hosting.
- `MapControl` renders through one `ICustomDrawOperation` into the Avalonia Skia compositor — the `SKCanvas` surface `Avalonia.Skia` owns; no second graphics backend.
- Tile appearance is source identity: `RasterStyleRenderer` draws a tile under layer and style `Opacity` and strokes `RasterStyle.Outline`, and no colour-matrix, tint, or blend hook reaches the raster path. `Image.BlendModeColor` tints symbol imagery alone, so a dark basemap is a different `ITileSource` on the tile row, never a post-effect over an existing one.

[STACKING]:
- `api-avalonia-skia.md`/`api-skiasharp.md`: `Mapsui.Rendering.Skia` draws the map on the shared `SkiaSharp` `SKCanvas` `Avalonia.Skia` owns; theme colour tokens flow into `Mapsui.Styles.Color`/`Pen`/`Brush`, the same paints `api-svg-skia.md`/`api-livecharts.md` consume, never a hand-built second `SKPaint` path.
- `api-silk-webgpu-wgpu`(`libs/csharp/.api/api-silk-webgpu-wgpu.md`): `Mapsui` renders the 2D georeferenced overlay on the same Avalonia compositor beside the `Silk.NET.WebGPU` 3D viewport, owning the 2D geo plane only.
- `Mapsui.Nts`(`.api/api-mapsui-nts.md`): the NTS half of one map — `GeometryFeature` sets mount on this catalog's `Layer.DataSource` or `WritableLayer`, `GetMapInfo` supplies the `MapInfo` every `EditManager` hit test takes, `EditingWidget` rides `Map.Widgets` as an ordinary `BaseWidget`, and `SphericalMercator` reprojects the coordinates those geometries carry.
- Bim geodesy seam: features arrive carrying the Bim-owned `GeoReference` from `GeoReferenceProjector` lowering `IfcMapConversion` and `IfcProjectedCRS`; `GeoFeature.Reproject` owns geodesy at that Bim seam, and AppUi reprojects only presentation WGS-84 input through `SphericalMercator.FromLonLat` (or `ProjectionDefaults.Projection` for non-mercator) into the EPSG:3857 `Map.CRS` at the layer-build edge — the internal model carries one CRS, the boundary owns the transform.
- capture rail: `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` yields the `byte[]` image the export owner (PDF/OOXML embed) consumes, the geo analogue of the 3D viewport capture.
- command rail: `MapControl.Info`/`MapTapped` + `GetMapInfo(screenPos, layers)` hit-test a click to the feature the Shell/Editing inspector binds.
- within-lib: a Bim-owned or GDAL/OGR-decoded geometry becomes a `Mapsui.Nts` feature on a `Layer` or `WritableLayer`, drawn above an OSM `TileLayer`; screen-anchored widgets live on `Map.Widgets` through `Mapsui.Extensions.ConcurrentQueueExtensions.Add`, never entering the world-space feature and CRS pipeline.

[LOCAL_ADMISSION]:
- One `MapControl` over a `Map` is the Shell 2D geo viewport; the model builds from `Mapsui` core, and every camera move routes through `Navigator` under animation.
- `ScaleBarWidget`, `MapInfoWidget`, `PerformanceWidget`, and `LoggingWidget` bind their map at construction, so a widget row carries `Func<Map, IWidget>` rather than a bare `Func<IWidget>`.

[RAIL_LAW]:
- Package: `Mapsui.Avalonia12` over `Mapsui` and `Mapsui.Tiling`, composing the transitive `Mapsui.Rendering.Skia`, the directly admitted `Mapsui.Nts`, and the `BruTile` tile-source contract `Mapsui.Tiling` is spelled in
- Owns: the Shell 2D slippy-map, basemap, and vector-overlay viewport — one `MapControl` over a `Map` with a tile basemap, feature overlays, and screen-anchored chrome on the shared Skia surface
- Accept: `MapControl` bound to a `Map` via `MapProperty`; a BruTile `TileLayer` basemap over an `HttpTileSource` carrying its own schema, credit, cache, and request hook; a `MemoryLayer`, `WritableLayer`, or provider-fed `Layer` overlay; `LayerCollection` groups for z-banding; `Navigator` for every camera move; `SphericalMercator`/`ProjectionDefaults` for CRS at the boundary; `GradientTheme`/`ThemeStyle`/`ColorBlend` for data-driven symbology; `Mapsui.Styles` colours from theme tokens; `GetSnapshot` for capture; `Info`/`GetMapInfo` for feature pick
- Reject: a second graphics backend beside the shared `SkiaSharp` family; a hand-built `SKPaint` for map styling; a post-render tile recolour standing in for a dark tile source; domain coordinates entering the model un-reprojected; a widget modeled as a world-space feature; a z-order branch where a layer group decides; reimplementing the `Mapsui` core model behind `Mapsui.UI.Avalonia`; re-declaring the `Mapsui.Nts` geometry, provider, or editing members here

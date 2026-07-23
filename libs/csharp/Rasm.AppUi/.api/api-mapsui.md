# [RASM_APPUI_API_MAPSUI]

`Mapsui.Avalonia12` binds the Mapsui slippy-map engine to Avalonia (assembly `Mapsui.UI.Avalonia`): one `MapControl` over a `Map` renders a tiled basemap with NTS vector overlays on the shared Skia surface. `Mapsui.UI.Avalonia` ships only `MapControl` and `RenderController`; this catalog documents the whole transitive stack it composes. A `Map` holds an ordered `LayerCollection` and a `Navigator` camera — a `TileLayer` paints the basemap, a `MemoryLayer` over `GeometryFeature` sets draws overlays, and `SphericalMercator` reprojects WGS-84 into EPSG:3857.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Mapsui.Avalonia12`
- package: `Mapsui.Avalonia12` (MIT)
- assembly: `Mapsui.UI.Avalonia` (`MapControl`, `RenderController` only; the shipped assembly id differs from the package id)
- namespace: `Mapsui.UI.Avalonia`
- transitive: `Mapsui` (model, layers, styles, widgets, navigator, projection), `Mapsui.Tiling` (`TileLayer` + BruTile sources), `Mapsui.Rendering.Skia` (Skia renderer), `Mapsui.Nts` (NTS `Geometry` bridge + providers) — all MIT, pure-managed
- depends: `Avalonia`, `Avalonia.Skia`, `SkiaSharp`
- rail: map

## [02]-[PUBLIC_TYPES]

[CONTROL_TYPES]: the Avalonia binding — `Mapsui.UI.Avalonia`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :----------------- | :------------ | :------------------ |
|  [01]   | `MapControl`       | control       | slippy-map viewport |
|  [02]   | `RenderController` | render driver | render-loop driver  |

- `MapControl` : `UserControl`, `IMapControl`, `IDisposable`, `INotifyPropertyChanged`; `RenderController` : `IDisposable`.

[MODEL_TYPES]: the map model and camera — `Mapsui` core

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------- | :------------ | :------------------ |
|  [01]   | `Map`             | map model     | viewport state      |
|  [02]   | `Navigator`       | camera        | animated navigation |
|  [03]   | `LayerCollection` | layer list    | ordered draw stack  |
|  [04]   | `Viewport`        | geometry      | world window        |
|  [05]   | `MRect`           | geometry      | world rectangle     |
|  [06]   | `MPoint`          | geometry      | world point         |

- `Map` : `INotifyPropertyChanged`, `IDisposable`; owns `Layers`, `Navigator`, `CRS`, `Extent`, `Widgets`, `BackColor`, and default `CRS` `"EPSG:3857"`.
- `LayerCollection` : `IEnumerable<ILayer>`; the basemap sits at index zero, overlays draw above it.

[LAYER_TYPES]: the layer family — `Mapsui.Layers`
- every layer is `BaseLayer : ILayer`; `ILayer` carries `Name`/`Enabled`/`Opacity`/`Style`/`Extent`/`MinVisible`/`MaxVisible` and `IEnumerable<IFeature> GetFeatures(MRect, double resolution)`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]         |
| :-----: | :------------------- | :-------------- | :------------------- |
|  [01]   | `ILayer`             | layer contract  | draw-stack member    |
|  [02]   | `BaseLayer`          | abstract base   | layer foundation     |
|  [03]   | `MemoryLayer`        | in-memory layer | direct features      |
|  [04]   | `Layer`              | provider layer  | asynchronous source  |
|  [05]   | `TileLayer`          | tile layer      | BruTile basemap      |
|  [06]   | `ImageLayer`         | specialized     | asynchronous imagery |
|  [07]   | `RasterizingLayer`   | specialized     | rasterized vectors   |
|  [08]   | `AnimatedPointLayer` | specialized     | animated points      |

- `MemoryLayer` and `Layer` derive from `BaseLayer`; `MemoryLayer.Features` holds a direct `IFeature` set, `Layer.DataSource` an async `IProvider?`.
- `TileLayer` (`Mapsui.Tiling`) admits XYZ, WMTS, and MBTiles BruTile sources.

[FEATURE_TYPES]: the feature model and the NTS bridge — `Mapsui` + `Mapsui.Nts`

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]        |
| :-----: | :----------------------------- | :--------------- | :------------------ |
|  [01]   | `IFeature`                     | feature contract | feature contract    |
|  [02]   | `BaseFeature`                  | feature base     | feature base        |
|  [03]   | `PointFeature`                 | feature          | located feature     |
|  [04]   | `GeometryFeature`              | NTS feature      | NTS bridge          |
|  [05]   | `GeoJsonProvider`              | provider         | GeoJSON provider    |
|  [06]   | `GeometrySimplifyProvider`     | provider         | viewport simplify   |
|  [07]   | `GeometryIntersectionProvider` | provider         | viewport clip       |
|  [08]   | `EditManager`                  | service          | geometry editing    |
|  [09]   | `EditingWidget`                | widget           | editing interaction |

- `IFeature` : `ICloneable`; `BaseFeature` owns geometry, `Styles`, and per-feature fields; `PointFeature` locates markers or labels at an `MPoint`.
- `GeometryFeature` (`Mapsui.Nts`) wraps a NetTopologySuite `Geometry?` as a drawable feature — the NTS bridge for GDAL/OGR and Bim overlays.
- `GeoJsonProvider` materializes NTS geometry from a GeoJSON string; `EditManager`/`EditingWidget` add, drag, and rotate vertices over NTS geometry.

[STYLE_AND_WIDGET_TYPES]: presentation — `Mapsui.Styles` / `Mapsui.Widgets`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]      |
| :-----: | :---------------- | :-------------- | :---------------- |
|  [01]   | `IStyle`          | style contract  | feature styling   |
|  [02]   | `VectorStyle`     | style           | vector paint      |
|  [03]   | `SymbolStyle`     | style           | point symbol      |
|  [04]   | `LabelStyle`      | style           | text label        |
|  [05]   | `CalloutStyle`    | style           | callout balloon   |
|  [06]   | `ImageStyle`      | style           | image marker      |
|  [07]   | `ThemeStyle`      | style           | feature selection |
|  [08]   | `StyleCollection` | style           | style composition |
|  [09]   | `Pen`             | style primitive | stroke value      |
|  [10]   | `Brush`           | style primitive | fill value        |
|  [11]   | `Color`           | style primitive | colour value      |
|  [12]   | `Font`            | style primitive | font value        |
|  [13]   | `IWidget`         | widget contract | screen overlay    |
|  [14]   | `ScaleBarWidget`  | widget          | scale bar         |
|  [15]   | `ZoomInOutWidget` | widget          | zoom controls     |
|  [16]   | `MapInfoWidget`   | widget          | information box   |
|  [17]   | `ButtonWidget`    | widget          | button control    |

- `VectorStyle` composes `Pen` and `Brush`; `ThemeStyle` : `IThemeStyle` selects styles from feature data.

[PROJECTION_TYPES]: CRS reprojection — `Mapsui.Projections`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]       | [CAPABILITY]        |
| :-----: | :------------------- | :------------------ | :------------------ |
|  [01]   | `SphericalMercator`  | projection          | web-mercator bridge |
|  [02]   | `IProjection`        | projection contract | projection contract |
|  [03]   | `Projection`         | projection          | projection engine   |
|  [04]   | `ProjectionDefaults` | static              | default projection  |

- `SphericalMercator.FromLonLat`/`.ToLonLat` bridge WGS-84 and EPSG:3857; `ProjectionDefaults.Projection` admits non-mercator reprojection through `IProjection`.

## [03]-[ENTRYPOINTS]

[CONTROL]: the `MapControl` binding surface — `Mapsui.UI.Avalonia`

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Map`                                                            | property | model binding, via `MapProperty`   |
|  [02]   | `MapProperty`                                                    | static   | `DirectProperty<MapControl,Map>`   |
|  [03]   | `Info`                                                           | event    | `EventHandler<MapInfoEventArgs>`   |
|  [04]   | `MapTapped`                                                      | event    | `EventHandler<MapEventArgs>` tap   |
|  [05]   | `MapPointerPressed`                                              | event    | pointer press                      |
|  [06]   | `MapPointerMoved`                                                | event    | pointer movement                   |
|  [07]   | `MapPointerReleased`                                             | event    | pointer release                    |
|  [08]   | `GetMapInfo(ScreenPosition, IEnumerable<ILayer>) -> MapInfo`     | instance | feature hit test at a screen point |
|  [09]   | `GetSnapshot(IEnumerable<ILayer>?, RenderFormat, int) -> byte[]` | instance | map or layer image capture         |
|  [10]   | `Refresh(ChangeType)`                                            | instance | redraw                             |
|  [11]   | `RefreshData(ChangeType)`                                        | instance | data refetch                       |
|  [12]   | `RefreshGraphics()`                                              | instance | canvas invalidation                |
|  [13]   | `ForceUpdate()`                                                  | instance | forced update                      |
|  [14]   | `UseContinuousMouseWheelZoom`                                    | property | wheel-zoom mode (`bool`)           |
|  [15]   | `ContinuousMouseWheelZoomStepSize`                               | property | wheel step (`double`)              |
|  [16]   | `UseFling`                                                       | property | fling mode (`bool`)                |
|  [17]   | `MaxTapGestureMovement`                                          | property | tap threshold (`int`)              |

[MAP_AND_CAMERA]: the model + navigator surface — `Mapsui` core

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new Map()`                                                  | ctor     | model construction                                   |
|  [02]   | `Map.CRS`                                                    | property | coordinate system (`string?`, default `"EPSG:3857"`) |
|  [03]   | `Map.Extent`                                                 | property | full bounds (`MRect?`)                               |
|  [04]   | `Map.BackColor`                                              | property | backdrop (`Brush?`)                                  |
|  [05]   | `Navigator.CenterOn(MPoint, long, Easing?)`                  | instance | animated pan                                         |
|  [06]   | `Navigator.ZoomTo(double, long, Easing?)`                    | instance | resolution zoom                                      |
|  [07]   | `Navigator.ZoomToBox(MRect?, MBoxFit, long, Easing?)`        | instance | box fit                                              |
|  [08]   | `Navigator.ZoomToLevel(int)`                                 | instance | level zoom                                           |
|  [09]   | `Navigator.ZoomIn(long, Easing?)`                            | instance | zoom increment                                       |
|  [10]   | `Navigator.ZoomOut(long, Easing?)`                           | instance | zoom decrement                                       |
|  [11]   | `Navigator.CenterOnAndZoomTo(MPoint, double, long, Easing?)` | instance | combined pan and zoom                                |
|  [12]   | `Navigator.FlyTo(MPoint, double, long)`                      | instance | parabolic move                                       |
|  [13]   | `Navigator.RotateTo(double, long, Easing?)`                  | instance | bearing rotation                                     |
|  [14]   | `Map.RefreshDataAsync(Viewport?) -> Task`                    | instance | data fetch                                           |
|  [15]   | `Map.ClearCache()`                                           | instance | tile and feature cache reset                         |

- `map.Layers.Add(layer)` pushes a layer onto the ordered draw stack; `CenterOn` also accepts `(double x, double y, ...)`.

[BASEMAP_AND_OVERLAY]: the tile + NTS-feature layer construction

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `OpenStreetMap.CreateTileLayer(string?) -> TileLayer` | static  | OSM XYZ basemap (`Mapsui.Tiling`)         |
|  [02]   | `new MemoryLayer { Features, Style }`                 | ctor    | in-memory feature overlay                 |
|  [03]   | `new GeometryFeature(Geometry?)`                      | ctor    | NTS geometry as a drawable feature        |
|  [04]   | `SphericalMercator.FromLonLat(double, double)`        | static  | WGS-84 to EPSG:3857 (`.ToLonLat` inverse) |
|  [05]   | `new Layer { DataSource, Style }`                     | ctor    | async provider-fed vector overlay         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every draw folds through one `Map`: its ordered `LayerCollection` painted by the transitive `Mapsui.Rendering.Skia` renderer, the control only binding and hosting.
- `MapControl` renders through a `MapsuiCustomDrawOperation` (`ICustomDrawOperation`) into the Avalonia Skia compositor — the `SKCanvas` surface `Avalonia.Skia` owns; no second graphics backend.

[STACKING]:
- `api-avalonia-skia.md`/`api-skiasharp.md`: `Mapsui.Rendering.Skia` draws the map on the shared `SkiaSharp` `SKCanvas` `Avalonia.Skia` owns; theme colour tokens flow into `Mapsui.Styles.Color`/`Pen`/`Brush`, the same paints `api-svg-skia.md`/`api-livecharts.md` consume, never a hand-built second `SKPaint` path.
- `api-silk-webgpu-wgpu.md`: `Mapsui` renders the 2D georeferenced overlay on the same Avalonia compositor beside the `Silk.NET.WebGPU` 3D viewport, owning the 2D geo plane only.
- Bim geodesy seam: features arrive carrying the Bim-owned `GeoReference` from `GeoReferenceProjector` lowering `IfcMapConversion` and `IfcProjectedCRS`; `GeoFeature.Reproject` owns geodesy at that Bim seam, and AppUi reprojects only presentation WGS-84 input through `SphericalMercator.FromLonLat` (or `ProjectionDefaults.Projection` for non-mercator) into the EPSG:3857 `Map.CRS` at the layer-build edge — the internal model carries one CRS, the boundary owns the transform.
- capture rail: `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` yields the `byte[]` image the export owner (PDF/OOXML embed) consumes, the geo analogue of the 3D viewport capture.
- command rail: `MapControl.Info`/`MapTapped` + `GetMapInfo(screenPos, layers)` hit-test a click to the feature the Shell/Editing inspector binds; `Mapsui.Nts.EditManager`/`EditingWidget` author overlay geometry.
- within-lib: a Bim-owned or GDAL/OGR-decoded `Geometry` becomes a `Mapsui.Nts.GeometryFeature` in a `MemoryLayer.Features`, drawn above an OSM `TileLayer`; screen-anchored widgets (`ScaleBarWidget`/`ZoomInOutWidget`/`MapInfoWidget`) live on `Map.Widgets`, never entering the world-space feature/CRS pipeline.

[LOCAL_ADMISSION]:
- One `MapControl` over a `Map` is the Shell 2D geo viewport; the model builds from `Mapsui` core, and every camera move routes through `Navigator` under animation.

[RAIL_LAW]:
- Package: `Mapsui.Avalonia12` composing the transitive `Mapsui`/`Mapsui.Tiling`/`Mapsui.Rendering.Skia`/`Mapsui.Nts`
- Owns: the Shell 2D slippy-map, basemap, and vector-overlay viewport — one `MapControl` over a `Map` with a tile basemap and NTS-feature overlays on the shared Skia surface
- Accept: `MapControl` bound to a `Map` via `MapProperty`; an OSM `TileLayer` basemap; a `MemoryLayer` over `GeometryFeature` for GDAL/OGR overlays; `Navigator` for every camera move; `SphericalMercator`/`ProjectionDefaults` for CRS at the boundary; `Mapsui.Styles` colours from theme tokens; `GetSnapshot` for capture; `Info`/`GetMapInfo` for feature pick
- Reject: a second graphics backend beside the shared `SkiaSharp` family; a hand-built `SKPaint` for map styling; domain coordinates entering the model un-reprojected; a widget modeled as a world-space feature; reimplementing the `Mapsui` core model behind `Mapsui.UI.Avalonia`

# [RASM_APPUI_API_MAPSUI]

`Mapsui.Avalonia12` is the Avalonia 12 binding (assembly `Mapsui.UI.Avalonia`) of the Mapsui slippy-map engine — realizing `Charts/basemap.md` (`[V8]`b, the declared Bim NTS basemap seam): the interactive basemap / vector-overlay viewport rendering through the admitted `SkiaSharp` + `Avalonia.Skia` and binding the Bim-owned `NetTopologySuite`, so GDAL/OGR-sourced features draw as live overlays beside the `Wgpu` 3D viewport. The package ships ONLY the `MapControl` Avalonia control + its `RenderController`; the map model, layer/feature/style/widget/navigator vocabulary, the tile pipeline, and the NTS geometry bridge live in the transitive `Mapsui` core, `Mapsui.Tiling`, `Mapsui.Rendering.Skia`, and `Mapsui.Nts` assemblies — this catalog documents the whole stack `Charts/basemap.md` composes (`[V8]`b, the realized `ARCH:67` seam). Mapsui is the TILED-basemap owner; the LiveCharts `GeoMap`/`DrawnMap` chart-projection (`api-livecharts.md`, `Charts/dashboards.md`) is the disjoint CHART-projection row — separate charters. One `Map` owns an ordered `LayerCollection` and a `Navigator` (the camera); a `TileLayer` paints the basemap; a `MemoryLayer`/`Layer` over a `GeometryFeature` set (NTS `Geometry`) draws domain overlays; `SphericalMercator` reprojects WGS-84 into the EPSG:3857 web-mercator the basemap uses.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Mapsui.Avalonia12`

- package: `Mapsui.Avalonia12`
- assembly: `Mapsui.UI.Avalonia` (the shipped assembly id differs from the package id; `Avalonia12` is the package-name discriminant for the Avalonia-12 build)
- namespace: `Mapsui.UI.Avalonia` (`MapControl`, `RenderController` only)
- license: MIT (`<license type="expression">MIT</license>`)
- build-floor: ships `lib/net8.0` + `lib/net9.0` (top TFM `net9.0`, no `net10.0`); the `net10.0` consumer binds `lib/net9.0` — the documented surface, forward-compatible
- transitive core (the real API): `Mapsui` (model/layers/styles/widgets/navigator/projection — `lib/net8.0`), `Mapsui.Tiling` (`TileLayer` + BruTile sources), `Mapsui.Rendering.Skia` (the Skia renderer), `Mapsui.Nts` (the NTS `Geometry` bridge + providers); all MIT, pure-managed
- dependency: `Avalonia` / `Avalonia.Skia` ≥ 12 (consumer runs Avalonia 12); the Skia draw rides the central-pinned `SkiaSharp` family shared with `Avalonia.Skia`/`Svg.Skia`/`LiveCharts`
- rail: map

## [02]-[PUBLIC_TYPES]

[CONTROL_TYPES]: the Avalonia binding — `Mapsui.UI.Avalonia`

- rail: map
- `MapControl` derives from `UserControl` and implements `IMapControl`, `IDisposable`, and `INotifyPropertyChanged`.
- `RenderController` implements `IDisposable`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :----------------- | :------------ | :------------------ |
|  [01]   | `MapControl`       | control       | slippy-map viewport |
|  [02]   | `RenderController` | render driver | render-loop driver  |

[CONTROL_DETAILS]:

- [01]-[VIEWPORT]: `MapControl` hosts a `Map` and owns pointer, wheel, fling, and tap input.
- [02]-[RENDERING]: `MapControl` renders through `MapsuiCustomDrawOperation` into the Avalonia Skia compositor.
- [03]-[LOOP]: `RenderController` feeds the per-control Skia draw operation.

[MODEL_TYPES]: the map model and camera — `Mapsui` core

- rail: map

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------- | :------------ | :------------------ |
|  [01]   | `Map`             | map model     | viewport state      |
|  [02]   | `Navigator`       | camera        | animated navigation |
|  [03]   | `LayerCollection` | layer list    | ordered draw stack  |
|  [04]   | `Viewport`        | geometry      | world window        |
|  [05]   | `MRect`           | geometry      | world rectangle     |
|  [06]   | `MPoint`          | geometry      | world point         |

[MODEL_DETAILS]:

- [01]-[MAP]: `Map` implements `INotifyPropertyChanged` and `IDisposable`; it owns `Layers`, `Navigator`, `CRS`, `Extent`, `Widgets`, and `BackColor`.
- [02]-[MAP_EVENTS]: `Map` publishes `Info`, `Tapped`, and `DataChanged`; its default `CRS` is `"EPSG:3857"`, and `Extent` is `MRect?`.
- [03]-[NAVIGATOR]: `Navigator` owns `CenterOn`, `ZoomTo`, `ZoomToBox`, `FlyTo`, `RotateTo`, and `ZoomToLevel` with duration and `Easing` animation.
- [04]-[LAYERS]: `LayerCollection` implements `IEnumerable<ILayer>` and keeps the basemap at index zero with overlays above it.
- [05]-[RECT]: `MRect` exposes `Min`, `Max`, `Width`, and `Centroid`.
- [06]-[POINT]: `MPoint(double x, double y)` constructs a world point.

[LAYER_TYPES]: the layer family — `Mapsui.Layers`

- rail: map
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

[LAYER_DETAILS]:

- [01]-[MEMORY]: `MemoryLayer(string)` derives from `BaseLayer`; its `IEnumerable<IFeature> Features` receives the GDAL/OGR-derived overlay.
- [02]-[PROVIDER]: `Layer(string)` derives from `BaseLayer`; its `IProvider? DataSource` supplies an asynchronously fetched `ILayerDataSource<IProvider>`.
- [03]-[TILES]: `TileLayer` derives from `BaseLayer` in `Mapsui.Tiling` and admits XYZ, WMTS, and MBTiles BruTile sources.

[FEATURE_TYPES]: the feature model and the NTS bridge — `Mapsui` + `Mapsui.Nts`

- rail: map

| [INDEX] | [SYMBOL]                       | [OWNER_ASM]  | [CAPABILITY]        |
| :-----: | :----------------------------- | :----------- | :------------------ |
|  [01]   | `IFeature`                     | `Mapsui`     | feature contract    |
|  [02]   | `BaseFeature`                  | `Mapsui`     | feature base        |
|  [03]   | `PointFeature`                 | `Mapsui`     | located feature     |
|  [04]   | `GeometryFeature`              | `Mapsui.Nts` | NTS bridge          |
|  [05]   | `GeoJsonProvider`              | `Mapsui.Nts` | GeoJSON provider    |
|  [06]   | `GeometrySimplifyProvider`     | `Mapsui.Nts` | viewport simplify   |
|  [07]   | `GeometryIntersectionProvider` | `Mapsui.Nts` | viewport clip       |
|  [08]   | `EditManager`                  | `Mapsui.Nts` | geometry editing    |
|  [09]   | `EditingWidget`                | `Mapsui.Nts` | editing interaction |

[FEATURE_DETAILS]:

- [01]-[BASE]: `IFeature` implements `ICloneable`; `BaseFeature` owns geometry, `Styles`, and per-feature fields.
- [02]-[POINT]: `PointFeature` derives from `BaseFeature` and locates markers or labels at an `MPoint`.
- [03]-[GEOMETRY]: `GeometryFeature` derives from `BaseFeature`; its `Geometry? Geometry` carries the NetTopologySuite geometry for GDAL/OGR and Bim overlays.
- [04]-[GEOJSON]: `GeoJsonProvider(string)` implements `IProvider` and materializes NTS geometry from a GeoJSON string.
- [05]-[EDITING]: `EditManager` and `EditingWidget` add, drag, and rotate vertices over NTS geometry.

[STYLE_AND_WIDGET_TYPES]: presentation — `Mapsui.Styles` / `Mapsui.Widgets`

- rail: map

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

[STYLE_DETAILS]:

- [01]-[VECTOR]: `VectorStyle` composes `Pen` and `Brush` for vector stroke and fill.
- [02]-[THEME]: `ThemeStyle` implements `IThemeStyle` and selects styles from feature data.
- [03]-[WIDGETS]: Every widget is screen-anchored on `Map.Widgets`.

[PROJECTION_TYPES]: CRS reprojection — `Mapsui.Projections`

- rail: map

| [INDEX] | [SYMBOL]             | [CAPABILITY]        |
| :-----: | :------------------- | :------------------ |
|  [01]   | `SphericalMercator`  | web-mercator bridge |
|  [02]   | `IProjection`        | projection contract |
|  [03]   | `Projection`         | projection engine   |
|  [04]   | `ProjectionDefaults` | default projection  |

[PROJECTION_DETAILS]:

- [01]-[FORWARD]: `SphericalMercator.FromLonLat` accepts `MPoint` or `double lon, double lat` and returns EPSG:3857 coordinates.
- [02]-[INVERSE]: `SphericalMercator.ToLonLat` returns WGS-84 coordinates from web-mercator input.
- [03]-[PLUGGABLE]: `ProjectionDefaults.Projection` admits non-mercator reprojection through `IProjection`.

## [03]-[ENTRYPOINTS]

[CONTROL]: the `MapControl` binding surface — `Mapsui.UI.Avalonia`

- rail: map

| [INDEX] | [SURFACE]                          | [CAPABILITY]        |
| :-----: | :--------------------------------- | :------------------ |
|  [01]   | `Map`                              | model binding       |
|  [02]   | `MapProperty`                      | Avalonia property   |
|  [03]   | `Info`                             | feature pick event  |
|  [04]   | `MapTapped`                        | tap event           |
|  [05]   | `MapPointerPressed`                | pointer press       |
|  [06]   | `MapPointerMoved`                  | pointer movement    |
|  [07]   | `MapPointerReleased`               | pointer release     |
|  [08]   | `GetMapInfo`                       | layer hit test      |
|  [09]   | `GetSnapshot`                      | image capture       |
|  [10]   | `Refresh`                          | redraw              |
|  [11]   | `RefreshData`                      | data refetch        |
|  [12]   | `RefreshGraphics`                  | canvas invalidation |
|  [13]   | `ForceUpdate`                      | forced update       |
|  [14]   | `UseContinuousMouseWheelZoom`      | wheel mode          |
|  [15]   | `ContinuousMouseWheelZoomStepSize` | wheel step          |
|  [16]   | `UseFling`                         | fling mode          |
|  [17]   | `MaxTapGestureMovement`            | tap threshold       |

[CONTROL_SIGNATURES]:

- [01]-[MAP]: `Map Map { get; set; }` binds through `MapProperty`, a `DirectProperty<MapControl, Map>` for XAML and MVVM.
- [02]-[INFO]: `Info` is an `EventHandler<MapInfoEventArgs>?`.
- [03]-[HIT_TEST]: `MapInfo GetMapInfo(ScreenPosition, IEnumerable<ILayer>)` identifies the feature at a screen point.
- [04]-[SNAPSHOT]: `byte[] GetSnapshot(IEnumerable<ILayer>? = null, RenderFormat = Png, int quality = 100)` renders the map or a layer subset.
- [05]-[REFRESH]: `Refresh(ChangeType)` and `RefreshData(ChangeType)` carry the change classification; `RefreshGraphics()` and `ForceUpdate()` are parameterless.

[MAP_AND_CAMERA]: the model + navigator surface — `Mapsui` core

- rail: map

| [INDEX] | [SURFACE]                     | [CAPABILITY]       |
| :-----: | :---------------------------- | :----------------- |
|  [01]   | `Map`                         | model construction |
|  [02]   | `Map.CRS`                     | coordinate system  |
|  [03]   | `Map.Extent`                  | full bounds        |
|  [04]   | `Map.BackColor`               | backdrop           |
|  [05]   | `Navigator.CenterOn`          | animated pan       |
|  [06]   | `Navigator.ZoomTo`            | resolution zoom    |
|  [07]   | `Navigator.ZoomToBox`         | box fit            |
|  [08]   | `Navigator.ZoomToLevel`       | level zoom         |
|  [09]   | `Navigator.ZoomIn`            | zoom increment     |
|  [10]   | `Navigator.ZoomOut`           | zoom decrement     |
|  [11]   | `Navigator.CenterOnAndZoomTo` | combined move      |
|  [12]   | `Navigator.FlyTo`             | parabolic move     |
|  [13]   | `Navigator.RotateTo`          | bearing rotation   |
|  [14]   | `Map.RefreshDataAsync`        | data fetch         |
|  [15]   | `Map.ClearCache`              | cache reset        |

[MAP_SIGNATURES]:

- [01]-[CONSTRUCTION]: `new Map()` creates the model, and `map.Layers.Add(layer)` pushes a layer onto the ordered draw stack.
- [02]-[PAN]: `Navigator.CenterOn` accepts `MPoint` or `x, y` coordinates with duration and optional `Easing`.
- [03]-[ZOOM]: `Navigator.ZoomTo` accepts a resolution; `ZoomToBox` accepts `MRect` and `MBoxFit`; `ZoomToLevel` accepts an integer level.
- [04]-[FLIGHT]: `CenterOnAndZoomTo` accepts an `MPoint` and resolution; `FlyTo` accepts an `MPoint`, maximum resolution, and duration.
- [05]-[ROTATION]: `Navigator.RotateTo` accepts a rotation and animation arguments.
- [06]-[DATA]: `async Task Map.RefreshDataAsync(Viewport?)` fetches data, and `Map.ClearCache()` drops tile and feature caches.

[BASEMAP_AND_OVERLAY]: the tile + NTS-feature layer construction

- rail: map

| [INDEX] | [SURFACE]                       | [CAPABILITY]      |
| :-----: | :------------------------------ | :---------------- |
|  [01]   | `OpenStreetMap.CreateTileLayer` | OSM XYZ basemap   |
|  [02]   | `MemoryLayer`                   | in-memory overlay |
|  [03]   | `GeometryFeature`               | NTS overlay       |
|  [04]   | `SphericalMercator.FromLonLat`  | web reprojection  |
|  [05]   | `Layer`                         | provider overlay  |

[OVERLAY_SIGNATURES]:

- [01]-[BASEMAP]: `Mapsui.Tiling.OpenStreetMap.CreateTileLayer(string? userAgent = null)` returns a ready `TileLayer`.
- [02]-[MEMORY]: `new MemoryLayer(name) { Features = features, Style = style }` draws a GDAL/OGR-derived `IFeature` set.
- [03]-[GEOMETRY]: `new GeometryFeature(Geometry)` in `Mapsui.Nts` wraps a NetTopologySuite geometry as a drawable feature.
- [04]-[PROJECTION]: `SphericalMercator.FromLonLat(lon, lat)` returns world `(x, y)` coordinates in the basemap's EPSG:3857 CRS.
- [05]-[PROVIDER]: `new Layer(name) { DataSource = provider, Style = style }` builds an asynchronously fetched vector layer.

## [04]-[IMPLEMENTATION_LAW]

[ASSEMBLY_TOPOLOGY]:

- `Mapsui.UI.Avalonia` ships only `MapControl` and `RenderController` in the five-assembly stack.
- `Mapsui` owns `Map`, `Navigator`, `LayerCollection`, `MRect`, `MPoint`, the layer, feature, style, and widget vocabulary, and the projection seam.
- `Mapsui.Tiling` owns `TileLayer` and the BruTile tile sources.
- `Mapsui.Rendering.Skia` owns the Skia renderer invoked by the control's draw operation.
- `Mapsui.Nts` owns the `GeometryFeature` NTS bridge, the GeoJSON, simplify, and intersection providers, and `EditManager`.
- Bind `MapControl.Map`, build the model from `Mapsui`, and paint through the transitive Skia renderer.
- The control renders into the Avalonia Skia compositor via a `MapsuiCustomDrawOperation` (an `ICustomDrawOperation` Avalonia executes inside its render pass) — the same `SkiaSharp` `SKCanvas` surface `Avalonia.Skia` owns, so the map shares the central-pinned Skia family with the chart (`LiveCharts`), SVG (`Svg.Skia`), and text (`SkiaSharp.HarfBuzz`) stacks; no second graphics backend.

## [05]-[STACKING_AND_RAIL]

[STACKING]:

- NTS overlay beside the 3D viewport: a Bim-owned `NetTopologySuite` `Geometry` (or a GDAL/OGR-decoded feature) becomes a `Mapsui.Nts.GeometryFeature`, dropped into a `MemoryLayer.Features`, drawn above an OSM `TileLayer` — the 2D georeferenced overlay rendering on the SAME Skia surface as, and laid out beside, the `Silk.NET.WebGPU` 3D viewport. The map and the 3D scene share the Avalonia compositor; Mapsui owns the 2D geo plane only.
- one Skia family: the map draws through `Mapsui.Rendering.Skia` against the central-pinned `SkiaSharp` 3.119.x runtime shared with `Avalonia.Skia`/`Svg.Skia`/`LiveCharts`/`SkiaSharp.HarfBuzz` — theme colour tokens flow into `Mapsui.Styles.Color`/`Pen`/`Brush` the same way they flow into chart paints, never a hand-built second `SKPaint` path.
- reprojection at the boundary: Bim owns geodesy, and features arrive with the Bim-owned `GeoReference` from `GeoReferenceProjector` lowering `IfcMapConversion` and `IfcProjectedCRS`.
- `GeoFeature.Reproject` owns geodesy under the seam declared at `Rasm.Bim/ARCHITECTURE.md:25,80,82`.
- AppUi reprojects WGS-84 input through `SphericalMercator.FromLonLat`, or through `ProjectionDefaults.Projection` for non-mercator input, into the EPSG:3857 `Map.CRS` at the layer-build edge.
- The internal model carries one CRS, and the boundary owns the transform; `GeoFeature.Reproject` excludes a local geodesy kernel.
- snapshot into the capture rail: `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` is the capture leg for the map surface — a `byte[]` image the `Document/export.md` owner (PDF/OOXML embed) consumes, the geo analogue of the 3D viewport capture.
- interaction into the command rail: `MapControl.Info`/`MapTapped` + `GetMapInfo(screenPos, layers)` hit-test a click to a feature — the pick the Shell/Editing inspector binds (select a geo feature → show its fields); the `Mapsui.Nts.EditManager`/`EditingWidget` is the interactive geometry-edit surface for authoring overlay features.
- widgets are screen-anchored, not features: `ScaleBarWidget`/`ZoomInOutWidget`/`MapInfoWidget` live on `Map.Widgets` (screen space), distinct from `ILayer` features (world space) — the chrome (scale bar, zoom buttons) never enters the feature/CRS pipeline.

[RAIL_LAW]:

- Packages: `Mapsui.Avalonia12` (assembly `Mapsui.UI.Avalonia`) composing the transitive `Mapsui` / `Mapsui.Tiling` / `Mapsui.Rendering.Skia` / `Mapsui.Nts`
- Owns: the Shell 2D slippy-map / basemap / vector-overlay viewport — one `MapControl` over a `Map` model with a tile basemap and NTS-feature overlays, rendered on the shared Skia surface
- Accept: one `MapControl` bound to a `Map` via `MapProperty`; an OSM `TileLayer` (`OpenStreetMap.CreateTileLayer`) as the basemap; a `MemoryLayer` over `GeometryFeature` (NTS `Geometry`) for GDAL/OGR-derived overlays; `Navigator` for all camera moves (animated); `SphericalMercator`/`ProjectionDefaults` for CRS transforms at the boundary; `Mapsui.Styles` colours from the theme tokens; `GetSnapshot` for capture; `Info`/`GetMapInfo` for feature pick
- Reject: a second graphics backend beside the shared `SkiaSharp` family; a hand-built `SKPaint` for map styling (use `Mapsui.Styles`); domain coordinates entering the model un-reprojected (transform at the layer-build edge); a widget modeled as a world-space feature; reaching past `Mapsui.UI.Avalonia` to reimplement the model the `Mapsui` core already owns

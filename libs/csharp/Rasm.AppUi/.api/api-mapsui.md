# [RASM_APPUI_API_MAPSUI]

`Mapsui.Avalonia12` is the Avalonia 12 binding (assembly `Mapsui.UI.Avalonia`) of the Mapsui slippy-map engine — the interactive basemap / vector-overlay viewport rendering through the admitted `SkiaSharp` + `Avalonia.Skia` and binding the Bim-owned `NetTopologySuite`, so GDAL/OGR-sourced features draw as live overlays beside the `Wgpu` 3D viewport. The package ships ONLY the `MapControl` Avalonia control + its `RenderController`; the map model, layer/feature/style/widget/navigator vocabulary, the tile pipeline, and the NTS geometry bridge live in the transitive `Mapsui` core, `Mapsui.Tiling`, `Mapsui.Rendering.Skia`, and `Mapsui.Nts` assemblies — this catalog documents the whole stack `Charts/basemap.md` composes (`[V8]`b, the realized `ARCH:67` seam). Mapsui is the TILED-basemap owner; the LiveCharts `GeoMap`/`DrawnMap` chart-projection (`api-livecharts.md`, `Charts/dashboards.md`) is the disjoint CHART-projection row — separate charters. One `Map` owns an ordered `LayerCollection` and a `Navigator` (the camera); a `TileLayer` paints the basemap; a `MemoryLayer`/`Layer` over a `GeometryFeature` set (NTS `Geometry`) draws domain overlays; `SphericalMercator` reprojects WGS-84 into the EPSG:3857 web-mercator the basemap uses.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Mapsui.Avalonia12`
- package: `Mapsui.Avalonia12`
- version: `5.1.0`
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

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `MapControl : UserControl, IMapControl, IDisposable, INotifyPropertyChanged` | control | the slippy-map viewport — hosts a `Map`, renders via a `MapsuiCustomDrawOperation` into the Avalonia Skia compositor, owns pointer/wheel/fling/tap input |
|  [02]   | `RenderController : IDisposable`                         | render driver | the per-control render-loop driver feeding the Skia draw operation |

[MODEL_TYPES]: the map model and camera — `Mapsui` core
- rail: map

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Map : INotifyPropertyChanged, IDisposable` | map model     | `Layers` (`LayerCollection`), `Navigator`, `CRS` (default `"EPSG:3857"`), `Extent` (`MRect?`), `Widgets`, `BackColor`; the `Info`/`Tapped`/`DataChanged` events |
|  [02]   | `Navigator`                               | camera          | the viewport camera — `CenterOn`/`ZoomTo`/`ZoomToBox`/`FlyTo`/`RotateTo`/`ZoomToLevel`, animated (duration + `Easing`) |
|  [03]   | `LayerCollection : IEnumerable<ILayer>`   | layer list      | the ordered, mutable draw stack (basemap at index 0, overlays above) |
|  [04]   | `Viewport` / `MRect` / `MPoint`           | geometry        | the world window (`Viewport`), world-rect (`MRect` — `Min`/`Max`/`Width`/`Centroid`), and world-point (`MPoint(double x, double y)`) primitives |

[LAYER_TYPES]: the layer family — `Mapsui.Layers`
- rail: map
- every layer is `BaseLayer : ILayer`; `ILayer` carries `Name`/`Enabled`/`Opacity`/`Style`/`Extent`/`MinVisible`/`MaxVisible` and `IEnumerable<IFeature> GetFeatures(MRect, double resolution)`

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `ILayer` / `BaseLayer`                     | layer contract  | the draw-stack member contract / abstract base               |
|  [02]   | `MemoryLayer(string) : BaseLayer`         | in-memory layer | `IEnumerable<IFeature> Features` — the directly-set overlay feature source (the GDAL/OGR-derived overlay) |
|  [03]   | `Layer(string) : BaseLayer`               | provider layer  | `IProvider? DataSource` — an async-fetched provider-backed layer (`ILayerDataSource<IProvider>`) |
|  [04]   | `TileLayer : BaseLayer` (`Mapsui.Tiling`) | tile layer      | a BruTile-backed basemap layer (XYZ/WMTS/MBTiles)            |
|  [05]   | `ImageLayer` / `RasterizingLayer` / `AnimatedPointLayer` | specialized | async-image / pre-rasterized vector / animated-point layers  |

[FEATURE_TYPES]: the feature model and the NTS bridge — `Mapsui` + `Mapsui.Nts`
- rail: map

| [INDEX] | [SYMBOL]                                  | [OWNER ASM]    | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `IFeature : ICloneable` / `BaseFeature`   | `Mapsui`       | a drawable feature — geometry + `Styles` + per-feature fields |
|  [02]   | `PointFeature : BaseFeature`              | `Mapsui`       | an `MPoint`-located feature (markers, labels)                |
|  [03]   | `GeometryFeature : BaseFeature`           | `Mapsui.Nts`   | **the NTS bridge** — `Geometry? Geometry` (a NetTopologySuite `Geometry`); the GDAL/OGR / Bim-`NetTopologySuite` overlay feature |
|  [04]   | `GeoJsonProvider(string) : IProvider`     | `Mapsui.Nts`   | a GeoJSON-string feature provider materializing NTS geometry |
|  [05]   | `GeometrySimplifyProvider` / `GeometryIntersectionProvider` | `Mapsui.Nts` | provider decorators — viewport simplify / clip of NTS features |
|  [06]   | `EditManager` / `EditingWidget`           | `Mapsui.Nts`   | interactive feature editing (add/drag/rotate vertices) over NTS geometry |

[STYLE_AND_WIDGET_TYPES]: presentation — `Mapsui.Styles` / `Mapsui.Widgets`
- rail: map

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `IStyle` / `VectorStyle` / `SymbolStyle`  | style           | the feature draw style — `Pen`/`Brush` vector fill+stroke, point symbol |
|  [02]   | `LabelStyle` / `CalloutStyle` / `ImageStyle` | style        | text label / callout balloon / image-marker styles           |
|  [03]   | `ThemeStyle : IThemeStyle` / `StyleCollection` | style    | data-driven per-feature style selection / composed style set |
|  [04]   | `Pen` / `Brush` / `Color` / `Font`        | style primitive | stroke / fill / colour / font value carriers                 |
|  [05]   | `IWidget` / `ScaleBarWidget` / `ZoomInOutWidget` / `MapInfoWidget` / `ButtonWidget` | widget | screen-anchored overlay widgets (scale bar, zoom buttons, info box) on `Map.Widgets` |

[PROJECTION_TYPES]: CRS reprojection — `Mapsui.Projections`
- rail: map

| [INDEX] | [SYMBOL]                                  | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `SphericalMercator`                       | `static MPoint FromLonLat(MPoint)` / `(double x,double y) FromLonLat(double lon, double lat)` + `ToLonLat` — WGS-84 ⇄ EPSG:3857 web-mercator |
|  [02]   | `IProjection` / `Projection` / `ProjectionDefaults` | the pluggable CRS-transform seam (`ProjectionDefaults.Projection`) for non-mercator reprojection |

## [03]-[ENTRYPOINTS]

[CONTROL]: the `MapControl` binding surface — `Mapsui.UI.Avalonia`
- rail: map

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Map Map { get; set; }` / `MapProperty` (`DirectProperty<MapControl,Map>`) | bind the map model (the `AvaloniaProperty` for XAML/MVVM)    |
|  [02]   | `event EventHandler<MapInfoEventArgs>? Info` / `MapTapped` / `MapPointerPressed`/`Moved`/`Released` | hit-test + pointer interaction events (feature pick, draw) |
|  [03]   | `MapInfo GetMapInfo(ScreenPosition, IEnumerable<ILayer>)`                  | hit-test the layers at a screen point (which feature was clicked) |
|  [04]   | `byte[] GetSnapshot(IEnumerable<ILayer>? = null, RenderFormat = Png, int quality = 100)` | render the map (or a layer subset) to an image — the Render/capture leg |
|  [05]   | `void Refresh(ChangeType)` / `RefreshData(ChangeType)` / `RefreshGraphics()` / `ForceUpdate()` | redraw / refetch / invalidate the canvas                     |
|  [06]   | `UseContinuousMouseWheelZoom` / `ContinuousMouseWheelZoomStepSize` / `UseFling` / `MaxTapGestureMovement` | input-behaviour knobs                                        |

[MAP_AND_CAMERA]: the model + navigator surface — `Mapsui` core
- rail: map

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new Map()` → `map.Layers.Add(layer)`                                      | construct the model, push layers onto the ordered draw stack |
|  [02]   | `Map.CRS` / `Map.Extent` / `Map.BackColor`                                | coordinate system (default EPSG:3857), full bounds, backdrop  |
|  [03]   | `Navigator.CenterOn(MPoint, duration, Easing?)` / `CenterOn(x, y, ...)`    | pan the camera to a world point (animated)                   |
|  [04]   | `Navigator.ZoomTo(resolution, ...)` / `ZoomToBox(MRect, MBoxFit, ...)` / `ZoomToLevel(int)` / `ZoomIn`/`ZoomOut` | zoom to a resolution / fit a box / discrete level |
|  [05]   | `Navigator.CenterOnAndZoomTo(MPoint, resolution, ...)` / `FlyTo(MPoint, maxResolution, duration)` | combined center+zoom / parabolic fly-to                      |
|  [06]   | `Navigator.RotateTo(rotation, ...)`                                        | rotate the viewport (bearing)                                |
|  [07]   | `async Task Map.RefreshDataAsync(Viewport?)` / `Map.ClearCache()`         | trigger an async data fetch / drop the tile+feature cache    |

[BASEMAP_AND_OVERLAY]: the tile + NTS-feature layer construction
- rail: map

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Mapsui.Tiling.OpenStreetMap.CreateTileLayer(string? userAgent = null)` → `TileLayer` | the ready OSM XYZ basemap layer factory                       |
|  [02]   | `new MemoryLayer(name){ Features = features, Style = style }`              | an overlay layer over an in-memory `IFeature` set (GDAL/OGR-derived) |
|  [03]   | `new GeometryFeature(Geometry)` (`Mapsui.Nts`)                             | wrap a NetTopologySuite `Geometry` as a drawable overlay feature |
|  [04]   | `SphericalMercator.FromLonLat(lon, lat)` → world `(x, y)`                  | reproject WGS-84 input to the EPSG:3857 the basemap draws in  |
|  [05]   | `new Layer(name){ DataSource = provider, Style = style }`                  | a provider-backed (async-fetched) vector layer               |

## [04]-[IMPLEMENTATION_LAW]

[ASSEMBLY_TOPOLOGY]:
- The package is a five-assembly stack: `Mapsui.UI.Avalonia` (this package) ships only `MapControl` + `RenderController`; `Mapsui` owns the model (`Map`/`Navigator`/`LayerCollection`/`MRect`/`MPoint`), the layer/feature/style/widget vocabulary, and the projection seam; `Mapsui.Tiling` owns `TileLayer` + the BruTile tile sources; `Mapsui.Rendering.Skia` owns the Skia renderer the control's draw operation invokes; `Mapsui.Nts` owns the `GeometryFeature` NTS bridge + the GeoJSON/simplify/intersection providers + the `EditManager`. Bind to `MapControl.Map`, build the model from the `Mapsui` core, paint via the transitive Skia renderer.
- The control renders into the Avalonia Skia compositor via a `MapsuiCustomDrawOperation` (an `ICustomDrawOperation` Avalonia executes inside its render pass) — the same `SkiaSharp` `SKCanvas` surface `Avalonia.Skia` owns, so the map shares the central-pinned Skia family with the chart (`LiveCharts`), SVG (`Svg.Skia`), and text (`SkiaSharp.HarfBuzz`) stacks; no second graphics backend.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- NTS overlay beside the 3D viewport: a Bim-owned `NetTopologySuite` `Geometry` (or a GDAL/OGR-decoded feature) becomes a `Mapsui.Nts.GeometryFeature`, dropped into a `MemoryLayer.Features`, drawn above an OSM `TileLayer` — the 2D georeferenced overlay rendering on the SAME Skia surface as, and laid out beside, the `Silk.NET.WebGPU` 3D viewport. The map and the 3D scene share the Avalonia compositor; Mapsui owns the 2D geo plane only.
- one Skia family: the map draws through `Mapsui.Rendering.Skia` against the central-pinned `SkiaSharp` 3.119.x runtime shared with `Avalonia.Skia`/`Svg.Skia`/`LiveCharts`/`SkiaSharp.HarfBuzz` — theme colour tokens flow into `Mapsui.Styles.Color`/`Pen`/`Brush` the same way they flow into chart paints, never a hand-built second `SKPaint` path.
- reprojection at the boundary — Bim owns geodesy: features arrive carrying the Bim-owned `GeoReference` (the `GeoReferenceProjector` IfcMapConversion/IfcProjectedCRS lowering) and Bim's `GeoFeature.Reproject` owns the geodesy (`Rasm.Bim/ARCHITECTURE.md:25,80,82`, the seam declared both sides); AppUi's ONLY reprojection is WGS-84 input through `SphericalMercator.FromLonLat` (or `ProjectionDefaults.Projection` for non-mercator) to the EPSG:3857 `Map.CRS` at the layer-build edge — a local geodesy kernel beside `GeoFeature.Reproject` is the forbidden form. The internal model is one CRS; the boundary owns the transform.
- snapshot into the capture rail: `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` is the capture leg for the map surface — a `byte[]` image the `Document/export.md` owner (PDF/OOXML embed) consumes, the geo analogue of the 3D viewport capture.
- interaction into the command rail: `MapControl.Info`/`MapTapped` + `GetMapInfo(screenPos, layers)` hit-test a click to a feature — the pick the Shell/Editing inspector binds (select a geo feature → show its fields); the `Mapsui.Nts.EditManager`/`EditingWidget` is the interactive geometry-edit surface for authoring overlay features.
- widgets are screen-anchored, not features: `ScaleBarWidget`/`ZoomInOutWidget`/`MapInfoWidget` live on `Map.Widgets` (screen space), distinct from `ILayer` features (world space) — the chrome (scale bar, zoom buttons) never enters the feature/CRS pipeline.

[RAIL_LAW]:
- Packages: `Mapsui.Avalonia12` (assembly `Mapsui.UI.Avalonia`) composing the transitive `Mapsui` / `Mapsui.Tiling` / `Mapsui.Rendering.Skia` / `Mapsui.Nts`
- Owns: the Shell 2D slippy-map / basemap / vector-overlay viewport — one `MapControl` over a `Map` model with a tile basemap and NTS-feature overlays, rendered on the shared Skia surface
- Accept: one `MapControl` bound to a `Map` via `MapProperty`; an OSM `TileLayer` (`OpenStreetMap.CreateTileLayer`) as the basemap; a `MemoryLayer` over `GeometryFeature` (NTS `Geometry`) for GDAL/OGR-derived overlays; `Navigator` for all camera moves (animated); `SphericalMercator`/`ProjectionDefaults` for CRS transforms at the boundary; `Mapsui.Styles` colours from the theme tokens; `GetSnapshot` for capture; `Info`/`GetMapInfo` for feature pick
- Reject: a second graphics backend beside the shared `SkiaSharp` family; a hand-built `SKPaint` for map styling (use `Mapsui.Styles`); domain coordinates entering the model un-reprojected (transform at the layer-build edge); a widget modeled as a world-space feature; reaching past `Mapsui.UI.Avalonia` to reimplement the model the `Mapsui` core already owns

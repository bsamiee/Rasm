# [RASM_APPUI_API_MAPSUI_NTS]

`Mapsui.Nts` is the NetTopologySuite bridge for the Mapsui map model: `GeometryFeature` carries an NTS `Geometry` as a drawable feature, four provider decorators compose one `Layer.DataSource` into an envelope-indexed, viewport-clipped, resolution-decimated fetch, `EditManager` drives an interactive vertex-authoring session over a `WritableLayer`, and one extension family converts between the Mapsui `MPoint`/`MRect` world primitives and the NTS `Coordinate`/`Point`/`Envelope` geometry primitives. The package holds NTS geometry alone — the map model, camera, styles, widgets, and CRS reprojection stay `Mapsui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Mapsui.Nts`
- package: `Mapsui.Nts` (MIT)
- assembly: `Mapsui.Nts`
- namespaces: `Mapsui.Nts`, `Mapsui.Nts.Providers`, `Mapsui.Nts.Providers.Shapefile`, `Mapsui.Nts.Editing`, `Mapsui.Nts.Widgets`, `Mapsui.Nts.Extensions`, `Mapsui.Providers.Wfs`
- target: `lib/net9.0`
- depends: `Mapsui`, `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`
- rail: map

## [02]-[PUBLIC_TYPES]

[FEATURE_AND_PROVIDER_TYPES]: the drawable NTS feature and the `IProvider` family — `Mapsui.Nts` / `Mapsui.Nts.Providers`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :------------------------------------ | :------------ | :--------------------------- |
|  [01]   | `GeometryFeature`                     | NTS feature   | NTS geometry as a feature    |
|  [02]   | `IndexedMemoryProvider`               | provider      | envelope-indexed feature set |
|  [03]   | `GeometryIntersectionProvider`        | decorator     | viewport clip per fetch      |
|  [04]   | `GeometrySimplifyProvider`            | decorator     | resolution-driven decimation |
|  [05]   | `GeometrySimplifyAndClippingProvider` | decorator     | fused clip-and-decimate      |
|  [06]   | `ObservableCollectionProvider<T>`     | provider      | live feature collection      |
|  [07]   | `GeoJsonProvider`                     | provider      | GeoJSON string source        |
|  [08]   | `ShapeFile`                           | provider      | ESRI shapefile source        |
|  [09]   | `DbaseReader`                         | reader        | shapefile attribute table    |
|  [10]   | `WFSProvider`                         | provider      | OGC WFS feature source       |

- `GeometryFeature` : `BaseFeature`, `IFeature`, `ICloneable`; its `Geometry?` member is the settable NTS payload and `Extent` projects the geometry envelope as an `MRect?`.
- Every provider is an `IProvider` carrying `CRS`, `GetExtent() -> MRect?`, and `GetFeaturesAsync(FetchInfo) -> Task<IEnumerable<IFeature>>`; the three decorators and `GeoJsonProvider` also carry `IProviderExtended` (`Id`, a `FeatureKeyCreator<T>` cache key).
- `ObservableCollectionProvider<T>` constrains `T : IFeatureProvider`, so a bound `ObservableCollection<T>` is the live source; `ShapeFile` is additionally `IDisposable` and owns a rebuildable spatial index.

[EDITING_TYPES]: the interactive authoring session — `Mapsui.Nts.Editing` / `Mapsui.Nts.Widgets`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :----------------- | :------------ | :----------------------------- |
|  [01]   | `EditManager`      | session       | vertex authoring state machine |
|  [02]   | `EditMode`         | enum          | nine-member session vocabulary |
|  [03]   | `EditManipulation` | static class  | pointer-event to session verbs |
|  [04]   | `EditingWidget`    | widget        | session bound to map input     |
|  [05]   | `EditHelper`       | static class  | segment-insert hit test        |
|  [06]   | `Geomorpher`       | static class  | in-place rotate and scale      |
|  [07]   | `AddInfo`          | session state | in-progress add geometry       |
|  [08]   | `DragInfo`         | session state | in-progress drag vertex        |
|  [09]   | `RotateInfo`       | session state | in-progress rotation centre    |
|  [10]   | `ScaleInfo`        | session state | in-progress scale centre       |

- `EditMode`: `None` `AddPoint` `AddLine` `DrawingLine` `AddPolygon` `DrawingPolygon` `Modify` `Rotate` `Scale` — `None` ends a session and each `Drawing*` member is the transient state its `Add*` peer enters mid-stroke.
- `EditingWidget` : `InputOnlyWidget`; it takes one `EditManager` at construction, re-exposes `EditMode`/`SelectMode`/`Layer` off it, and forwards `OnPointerPressed`/`OnPointerMoved`/`OnPointerReleased`/`OnTapped` into `EditManipulation`.
- The four `*Info` carriers are `EditManager`-private session state; a consumer reads the session through `IsManipulating()` and `GetGrownExtent()` rather than through them.

[CONVERSION_TYPES]: the Mapsui-to-NTS primitive bridge — `Mapsui.Nts.Extensions`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------------- | :------------ | :---------------------------- |
|  [01]   | `GeometryExtensions`   | static class  | geometry to feature, vertices |
|  [02]   | `CoordinateExtensions` | static class  | coordinate to point and ring  |
|  [03]   | `MPointExtensions`     | static class  | `MPoint` to NTS               |
|  [04]   | `PointExtensions`      | static class  | NTS `Point` to `MPoint`       |
|  [05]   | `MRectExtensions`      | static class  | `MRect` to envelope, polygon  |
|  [06]   | `EnvelopeExtensions`   | static class  | envelope to `MRect`           |
|  [07]   | `LineStringExtensions` | static class  | line to linear ring           |
|  [08]   | `TupleExtensions`      | static class  | `(x, y)` tuple to coordinate  |

## [03]-[ENTRYPOINTS]

[FEATURE_AND_PROVIDER]: feature construction and the decorator chain

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `new GeometryFeature(Geometry?)`                                                | ctor     | NTS geometry as a drawable feature    |
|  [02]   | `new GeometryFeature(GeometryFeature, long)`                                    | ctor     | copy carrying a new feature id        |
|  [03]   | `GeometryFeature.Geometry`                                                      | property | settable `Geometry?` payload          |
|  [04]   | `GeometryFeature.Modified()`                                                    | instance | invalidates the cached extent         |
|  [05]   | `new IndexedMemoryProvider(IEnumerable<IFeature>)`                              | ctor     | envelope-indexed feature source       |
|  [06]   | `IndexedMemoryProvider.Find(object?, string) -> IFeature?`                      | instance | attribute lookup by field name        |
|  [07]   | `IndexedMemoryProvider.SymbolSize`                                              | property | hit-test padding (`double`, 64)       |
|  [08]   | `new GeometryIntersectionProvider(IProvider)`                                   | ctor     | clips each fetch to the viewport      |
|  [09]   | `new GeometrySimplifyProvider(IProvider, Func<Geometry,double,Geometry>?, ...)` | ctor     | decimation, tolerance off resolution  |
|  [10]   | `new GeometrySimplifyAndClippingProvider(IProvider, ..., double?)`              | ctor     | one decorator doing both              |
|  [11]   | `new ObservableCollectionProvider<T>(ObservableCollection<T>)`                  | ctor     | live collection as a source           |
|  [12]   | `new GeoJsonProvider(string)`                                                   | ctor     | GeoJSON string as a source            |
|  [13]   | `new ShapeFile(string, bool, bool, IProjectionCrs?, bool)`                      | ctor     | shapefile with optional index and prj |
|  [14]   | `ShapeFile.RebuildSpatialIndex()`                                               | instance | rebuilds the on-disk index            |

[EDIT_SESSION]: `EditManager` — every redline verb by name

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `EditManager.Layer`                                | property | edit target (`WritableLayer?`)          |
|  [02]   | `EditManager.EditMode`                             | property | session start and stop (`EditMode`)     |
|  [03]   | `EditManager.VertexRadius`                         | property | hit radius in pixels (`int`, 12)        |
|  [04]   | `EditManager.SelectMode`                           | property | selection instead of authoring (`bool`) |
|  [05]   | `EndEdit() -> bool`                                | instance | seals a `Drawing*` stroke               |
|  [06]   | `HoveringVertex(MapInfo)`                          | instance | tracks the pending vertex               |
|  [07]   | `AddVertex(Coordinate) -> bool`                    | instance | appends one authored vertex             |
|  [08]   | `StartDragging(MapInfo, double) -> bool`           | instance | begins a vertex drag                    |
|  [09]   | `Dragging(Point?) -> bool`                         | instance | moves the dragged vertex                |
|  [10]   | `StopDragging()`                                   | instance | ends the drag                           |
|  [11]   | `TryInsertCoordinate(MapInfo) -> bool`             | instance | splits the touched segment              |
|  [12]   | `TryDeleteCoordinate(MapInfo, double) -> bool`     | instance | removes the touched vertex              |
|  [13]   | `StartRotating(MapInfo) -> bool`                   | instance | begins a rotation                       |
|  [14]   | `Rotating(Point?) -> bool`                         | instance | applies the live rotation               |
|  [15]   | `StopRotating()`                                   | instance | ends the rotation                       |
|  [16]   | `StartScaling(MapInfo) -> bool`                    | instance | begins a scale                          |
|  [17]   | `Scaling(Point?) -> bool`                          | instance | applies the live scale                  |
|  [18]   | `StopScaling()`                                    | instance | ends the scale                          |
|  [19]   | `ResetManipulations()`                             | instance | clears drag, rotate, and scale state    |
|  [20]   | `IsManipulating() -> bool`                         | instance | reports an in-flight manipulation       |
|  [21]   | `GetGrownExtent() -> MRect?`                       | instance | manipulation extent for invalidation    |
|  [22]   | `EditManager.AngleBetween(Point, Point) -> double` | static   | rotation angle in degrees               |

[EDIT_INPUT]: the widget and the pointer-event bridge

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `new EditingWidget(EditManager)`                                             | ctor    | session mounted on `Map.Widgets`    |
|  [02]   | `EditManipulation.OnPointerPressed(WidgetEventArgs, EditManager) -> bool`    | static  | press to add, drag, rotate, scale   |
|  [03]   | `EditManipulation.OnPointerMoved(WidgetEventArgs, EditManager) -> bool`      | static  | move to hover, drag, rotate, scale  |
|  [04]   | `EditManipulation.OnPointerReleased(EditManager) -> bool`                    | static  | release to stop every manipulation  |
|  [05]   | `EditManipulation.OnTapped(WidgetEventArgs, EditManager) -> bool`            | static  | tap to insert, delete, end a stroke |
|  [06]   | `EditHelper.ShouldInsert(MPoint, double, List<Coordinate>, double, out int)` | static  | segment index under the pointer     |
|  [07]   | `Geomorpher.Rotate(Geometry, double, Point)`                                 | static  | rotates geometry in place           |
|  [08]   | `Geomorpher.Scale(Geometry, double, Point)`                                  | static  | scales geometry in place            |

[GEOMETRY_BRIDGE]: the conversion extensions

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Geometry.ToFeature() -> GeometryFeature`                                  | static  | one geometry as one feature       |
|  [02]   | `IEnumerable<Geometry>.ToFeatures() -> IEnumerable<GeometryFeature>`       | static  | a geometry set as features        |
|  [03]   | `Geometry.GetVertexLists() -> IList<IList<Coordinate>>`                    | static  | per-ring editable vertex lists    |
|  [04]   | `Geometry.MainCoordinates() -> List<Coordinate>`                           | static  | the primary ring or line vertices |
|  [05]   | `Geometry?.InsertCoordinate(Coordinate, int) -> Geometry?`                 | static  | rebuilds with a split segment     |
|  [06]   | `Geometry.DeleteCoordinate(int) -> Geometry?`                              | static  | rebuilds without one vertex       |
|  [07]   | `Coordinate.ToMPoint() -> MPoint` / `Coordinate.ToPoint() -> Point`        | static  | coordinate lift                   |
|  [08]   | `IEnumerable<Coordinate>.ToLineString()` / `.ToLinearRing()`               | static  | vertex set to line or ring        |
|  [09]   | `IEnumerable<Coordinate>.ToPolygon(IEnumerable<IEnumerable<Coordinate>>?)` | static  | shell plus holes to a polygon     |
|  [10]   | `Coordinate?.SetXY(Coordinate?)` / `Coordinate?.SetXY(MPoint?)`            | static  | in-place vertex write             |
|  [11]   | `MPoint?.ToPoint() -> Point?` / `MPoint?.ToCoordinate() -> Coordinate?`    | static  | world point to NTS                |
|  [12]   | `Point.ToMPoint() -> MPoint`                                               | static  | NTS point to world point          |
|  [13]   | `MRect.ToEnvelope() -> Envelope` / `MRect.ToPolygon() -> Polygon`          | static  | world rectangle to NTS            |
|  [14]   | `Envelope.ToMRect() -> MRect?`                                             | static  | NTS envelope to world rectangle   |
|  [15]   | `LineString.ToLinearRing() -> LinearRing`                                  | static  | closes a line into a ring         |
|  [16]   | `(double x, double y).ToCoordinate() -> Coordinate`                        | static  | tuple lift                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The three decorators wrap one inner `IProvider` and compose on one `Layer.DataSource`: `IndexedMemoryProvider` answers a fetch off an envelope index rather than a linear scan, `GeometryIntersectionProvider` clips each fetched geometry to the fetch `MRect`, and `GeometrySimplifyProvider` decimates the clipped result — its `simplify` argument defaults to `TopologyPreservingSimplifier.Simplify` and a null `distanceTolerance` drives the tolerance from `fetchInfo.Resolution`, so decimation tracks the live zoom with no caller-side band. `GeometrySimplifyAndClippingProvider` fuses the last two into one decorator and carries no `IProviderExtended` cache key.
- `EditManager` is a mode-driven state machine over one `WritableLayer`: `EditMode` selects the verb family, each `Start*`/`*ing`/`Stop*` trio brackets one manipulation against private session state, and `EndEdit` seals a `DrawingLine` or `DrawingPolygon` stroke by rebuilding the feature geometry and dropping back to its `Add*` mode. Every authored geometry is written through `Layer.Add`/`TryRemove` and invalidated through `GeometryFeature.Modified()` plus `BaseLayer.DataHasChanged()`, so the edit layer holds features directly and no provider decorator sits under it.
- Coordinates enter authoring in WORLD units: `AddVertex` takes an NTS `Coordinate`, the drag, rotate, and scale legs take an NTS `Point?`, and the hit tests take a Mapsui `MapInfo` with a pixel `screenDistance` — the two primitive families meet only through `Mapsui.Nts.Extensions`.
- `ShapeFile` and `GeoJsonProvider` decode to the same `GeometryFeature` set every other provider answers, so a file-backed layer and an in-memory layer are one draw path.

[STACKING]:
- `Mapsui.Avalonia12`(`.api/api-mapsui.md`): this package supplies the NTS half of that catalog's map — a `GeometryFeature` set mounts as a `Layer.DataSource` or a `WritableLayer` on the `Map.Layers` stack `Mapsui.Rendering.Skia` paints, `MapControl.GetMapInfo(ScreenPosition, IEnumerable<ILayer>)` yields the `MapInfo` every `EditManager` hit test takes, and `SphericalMercator.FromLonLat`/`.ToLonLat` reprojects the coordinates these geometries carry. The map model, camera, styles, widgets, and CRS stay that catalog's; NTS geometry and the editing session are this one's, so neither catalog restates the other's members.
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): `Geometry`, `Coordinate`, `Point`, `LineString`, `LinearRing`, `Polygon`, `GeometryCollection`, `Envelope`, and `ICoordinateSequenceFilter` arrive whole — a coordinate-sequence filter applied through `Geometry.Apply` is the one reprojection sweep over an authored or ingested geometry, and `Geometry.Copy()` before that sweep keeps the source frame intact.
- `EditingWidget` on `Map.Widgets` is the shipped input binding; a host that owns its own pointer routing calls the four `EditManipulation` statics directly against the same `EditManager`, so the session has one state owner under either binding.
- Bim geodesy seam: features arriving from `Rasm.Bim` carry their `GeoReference` lineage and reproject at that seam; this package holds no CRS authority beyond the `CRS` string every `IProvider` carries, so a datum transform inside a provider decorator is unrepresentable.

[LOCAL_ADMISSION]:
- One `GeometryFeature` per drawn NTS geometry, mounted either through the provider decorator chain on a `Layer` (read-only overlays, where residency is provider policy) or directly on a `WritableLayer.Features` (authoring, where `EditManager` writes in place).
- One `EditManager` per authoring surface; `EditMode` is the session vocabulary and `None` is the end state, so a parallel authoring flag beside it governs nothing.
- Vertex geometry converts through `Mapsui.Nts.Extensions` at the one point the Mapsui and NTS primitive families meet.

[RAIL_LAW]:
- Package: `Mapsui.Nts`
- Owns: NTS geometry inside the Mapsui map — the drawable geometry feature, the provider decorator chain, the file and service geometry sources, the interactive editing session, and the Mapsui-to-NTS primitive bridge
- Accept: `GeometryFeature` over a `Geometry?`; the `IndexedMemoryProvider`/`GeometryIntersectionProvider`/`GeometrySimplifyProvider` chain on one `Layer.DataSource`; `EditManager` over a `WritableLayer` under `EditMode`; `EditingWidget` or the `EditManipulation` statics for input; `Mapsui.Nts.Extensions` for every primitive conversion
- Reject: a hand-rolled viewport cull or per-zoom decimation branch beside the provider decorators; a decorator chain under the edit layer, which `EditManager` writes in place; a second authoring state machine beside `EditManager`; a geodesy or datum transform inside a provider; re-declaring the `Mapsui` core map, camera, style, or widget members this catalog composes

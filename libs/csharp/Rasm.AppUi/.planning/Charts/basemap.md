# [APPUI_CHARTS_BASEMAP]

The basemap is the tiled 2D geographic plane beside the Wgpu viewport: one Mapsui `MapControl` hosts one `Map` whose layer stack is data rows — a tile basemap row, NTS overlay rows projecting Bim-owned geospatial features, and widget rows — with navigation through the one `Navigator`, feature picking through `GetMapInfo`, snapshots through the capture encode fold, and design-review redlining through the `EditManager` surface committing as `EditIntent.Annotation`. The page owns the layer row family, the overlay projection, the pick and snapshot folds, the redline authoring surface, and the CRS ingress law: Bim owns geodesy (`GeoReference`, `GeoFeature.Reproject`, IfcMapConversion lowering) and AppUi reprojects ONLY WGS-84 input through `SphericalMercator` — a local geodesy kernel is the forbidden form. LiveCharts `GeoMap`/`DrawnMap` stays the CHART-projection row on `dashboards.md`; this page is the TILED-basemap owner — disjoint charters.

## [01]-[INDEX]

- [02]-[MAP_SURFACE]: One `MapControl`/`Map`; the layer row family; navigation verbs.
- [03]-[NTS_OVERLAY]: Bim geospatial features as `GeometryFeature` overlay rows; CRS ingress.
- [04]-[PICK_AND_SNAPSHOT]: Feature hit-test into the pick state; capture snapshots.
- [05]-[REDLINE]: Design-review markup over `EditManager`; commit as `EditIntent.Annotation`.

## [02]-[MAP_SURFACE]

- Owner: `BasemapLayerRow` [Union] — the closed layer vocabulary; `BasemapSurface` — the one map owner; `MapNav` [Union] — the navigation verb vocabulary.
- Cases: `BasemapLayerRow` = Tile · Overlay · Widget; `MapNav` = CenterOn · ZoomTo · ZoomToLevel · ZoomToBox · CenterAndZoom · FlyTo · RotateTo — the verified `Navigator` camera surface, one case per move, so a caller composes verbs and never touches the `Navigator` directly.
- Entry: `public Fin<Map> Build(Seq<BasemapLayerRow> rows)` — one fold from layer rows to the mounted `Map`; `public IO<Unit> Navigate(MapNav verb)` — every camera move discriminates on the verb union through the one `Navigator`.
- Auto: the tile row defaults to `OpenStreetMap.CreateTileLayer` and any slippy-tile source is one row value; the map chrome ships as named widget rows — `ScaleBar` (`ScaleBarWidget`), `ZoomButtons` (`ZoomInOutWidget`), `InfoBox` (`MapInfoWidget`) — screen-anchored on `Map.Widgets`, never world-space features; layer z-order is seq order so a stacking change is a row reorder, never an imperative insert; the control binds `Map` through `MapControl.Map` at mount and a row-set change re-runs `Build`, whose terminal `RefreshGraphics` invalidates the canvas so the new stack draws without a data refetch.
- Receipt: layer-set changes and navigation verbs contribute through `AppUiTelemetry.Contribute` instrument rows; faults are typed `ChartFault` cases deriving through the `Diagnostics/evidence.md#FAULT_TABLES` `AppUiFaultBand.Chart` row (6200) — the one Charts band shared with dashboards and custom.
- Packages: Mapsui.Avalonia12, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new basemap source, overlay family, or widget is one `BasemapLayerRow` value; a new camera move is one `MapNav` case; zero new surface.
- Boundary: ONE `MapControl` and ONE `Map` per basemap surface — a second map control, a per-overlay map, or a parallel tile engine is the deleted form; the transitive Mapsui/Tiling/Nts/Rendering.Skia set stays transitive (the admitted pin is `Mapsui.Avalonia12`); the basemap draws BESIDE the Wgpu viewport as an Avalonia control — it never enters the render graph, and geographic dashboards that need chart-projected geography stay on the LiveCharts `GeoMap` row (`dashboards.md`), the charter split stated on both pages.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BasemapLayerRow {
    private BasemapLayerRow() { }
    public sealed record Tile(string Key, Func<ILayer> Source) : BasemapLayerRow;
    public sealed record Overlay(string Key, Seq<GeoOverlayRow> Features, VectorStyle Style) : BasemapLayerRow;
    public sealed record Widget(string Key, Func<Mapsui.Widgets.IWidget> Source) : BasemapLayerRow;

    public static readonly BasemapLayerRow Osm = new Tile("osm", static () => Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
    public static readonly BasemapLayerRow ScaleBar = new Widget("scale-bar", static () => new ScaleBarWidget());
    public static readonly BasemapLayerRow ZoomButtons = new Widget("zoom-buttons", static () => new ZoomInOutWidget());
    public static readonly BasemapLayerRow InfoBox = new Widget("info-box", static () => new MapInfoWidget());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MapNav {
    private MapNav() { }
    public sealed record CenterOn(MPoint Center) : MapNav;
    public sealed record ZoomTo(double Resolution) : MapNav;
    public sealed record ZoomToLevel(int Level) : MapNav;
    public sealed record ZoomToBox(MRect Box) : MapNav;
    public sealed record CenterAndZoom(MPoint Center, double Resolution) : MapNav;
    public sealed record FlyTo(MPoint Center, double Resolution, MapFlight Flight) : MapNav;
    public sealed record RotateTo(double Degrees) : MapNav;
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapFlight {
    public static readonly MapFlight Direct = new("direct", 0L);
    public static readonly MapFlight Focus = new("focus", 240L);
    public static readonly MapFlight Traverse = new("traverse", 480L);

    public long DurationMs { get; }
}

public sealed record BasemapSurface(MapControl Control) {
    public Fin<Map> Build(Seq<BasemapLayerRow> rows) {
        Map candidate = new();
        return rows.Fold(Fin.Succ(candidate), (rail, row) => rail.Bind(map => Mount(map, row))).Match(
            Succ: map => {
                Map previous = Control.Map;
                Control.Map = map;
                Control.RefreshGraphics();
                previous.Dispose();
                return Fin.Succ(map);
            },
            Fail: error => {
                candidate.Dispose();
                return Fin.Fail<Map>(error);
            });
    }

    public IO<Unit> Navigate(MapNav verb) =>
        IO.lift(() => ignore(verb.Switch(
            state: Control.Map.Navigator,
            centerOn: static (nav, v) => fun(() => nav.CenterOn(v.Center))(),
            zoomTo: static (nav, v) => fun(() => nav.ZoomTo(v.Resolution))(),
            zoomToLevel: static (nav, v) => fun(() => nav.ZoomToLevel(v.Level))(),
            zoomToBox: static (nav, v) => fun(() => nav.ZoomToBox(v.Box))(),
            centerAndZoom: static (nav, v) => fun(() => nav.CenterOnAndZoomTo(v.Center, v.Resolution))(),
            flyTo: static (nav, v) => fun(() => nav.FlyTo(v.Center, v.Resolution, v.Flight.DurationMs))(),
            rotateTo: static (nav, v) => fun(() => nav.RotateTo(v.Degrees))())));

    // Generated total Switch over the closed family — a new BasemapLayerRow case breaks THIS dispatch at
    // compile time; the runtime-silent `_` arm over the closed family is the deleted form.
    static Fin<Map> Mount(Map map, BasemapLayerRow row) => row.Switch(
        state: map,
        tile: static (m, t) => Fin.Succ(fun(() => { m.Layers.Add(t.Source()); return m; })()),
        overlay: static (m, o) => GeoOverlay.Layer(o).Map(layer => { m.Layers.Add(layer); return m; }),
        widget: static (m, w) => Fin.Succ(fun(() => { m.Widgets.Add(w.Source()); return m; })()));
}
```

## [03]-[NTS_OVERLAY]

- Owner: `GeoOverlayRow` — the per-feature overlay row carrying the Bim-owned `GeoFeature` WHOLE (geometry, attribute table, declared `SourceCrs`) plus its display label; `GeoOverlay` — the projection fold from Bim geospatial output to a Mapsui `MemoryLayer`.
- Entry: `public static Fin<ILayer> Layer(BasemapLayerRow.Overlay overlay)` — one fold; each row's consumed feature wraps as `Mapsui.Nts.GeometryFeature`, styles resolve from the row's `VectorStyle`, and the layer mounts as one `MemoryLayer`.
- Auto: features ARRIVE as Bim-owned `GeoFeature` rows carrying their `GeoReference` lineage (the `GeoReferenceProjector` IfcMapConversion/IfcProjectedCRS lowering) already reprojected to WGS-84 by Bim's `GeoFeature.Reproject` — the declared seam, both sides (`Rasm.Bim` Semantics/geospatial -> AppUi Charts) — so the row's `SourceCrs`/SRID state IS the CRS evidence the gate reads; AppUi's ONLY reprojection is WGS-84 lon/lat -> EPSG:3857 through `SphericalMercator.FromLonLat` under `ProjectionDefaults.Projection` at the layer-build edge.
- Receipt: an overlay row whose feature still declares a projected frame (or a non-4326 SRID) folds to `ChartFault.CrsUnresolved` — the ingress law enforced as a typed fault, never a silent draw at wrong coordinates.
- Packages: Mapsui.Avalonia12 (Mapsui.Nts transitive), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new overlay family (site boundary, utility run, parcel, analysis heat cells) is a seq of rows on one Overlay layer row; zero new surface.
- Boundary: geodesy stays Bim's — `GeoFeature.Reproject`, datum transforms, and projected-CRS handling never re-implement here, and a local geodesy kernel (a proj4 port, a datum table, a second `SphericalMercator` beside the Mapsui primitive) is the FORBIDDEN form; NTS geometry types cross the seam as values inside Bim-owned features and are wrapped, never re-modeled; `GeoTiles.Catalog` TileJSON from Bim's `GeoModel.ToTiles` mounts as one Tile row when the vector-tile lane ships — a row, not a new surface.

```csharp signature
// The ingress row carries the Bim-owned GeoFeature WHOLE — geometry + attribute table + declared
// SourceCrs — so CRS authority is the seam contract the feature itself carries, never an SRID sniff on
// a bare geometry; Label is the only display column minted beside the consumed feature.
public sealed record GeoOverlayRow(
    string FeatureId,
    Rasm.Bim.Semantics.GeoFeature Feature,
    Option<string> Label);

public static class GeoOverlay {
    public static Fin<ILayer> Layer(BasemapLayerRow.Overlay overlay) =>
        overlay.Features
            .Traverse(Project)
            .Map(features => (ILayer)new MemoryLayer(overlay.Key) {
                Features = features.ToArray(),
                Style = overlay.Style,
            })
            .As();

    // The ingress gate: a feature is admissible only when its OWN declared frame is the WGS-84 baseline
    // the seam promises (post-Reproject: no residual projected frame, SRID 4326 exactly — an unstamped
    // SRID 0 is un-reprojected input and faults) — a residual SourceCrs is the typed CrsUnresolved fault,
    // never a silent draw at wrong coordinates; the transformed copy re-stamps SRID 3857 so downstream
    // reads the web-mercator frame it actually holds, never the source frame.
    static Fin<GeometryFeature> Project(GeoOverlayRow row) =>
        row.Feature.SourceCrs.IsNone && row.Feature.Geometry.SRID is 4326
            ? Fin.Succ(fun(() => {
                NetTopologySuite.Geometries.Geometry mercator = row.Feature.Geometry.Copy();
                mercator.Apply(MercatorFilter.Forward);
                mercator.SRID = 3857;
                GeometryFeature feature = new(mercator);
                feature["id"] = row.FeatureId;
                row.Label.Iter(label => feature["label"] = label);
                return feature;
            })())
            : Fin.Fail<GeometryFeature>(new ChartFault.CrsUnresolved(row.FeatureId, row.Feature.Geometry.SRID));
}

// ONE parameterized coordinate-sequence filter over the Mapsui projection primitive — Forward lifts
// WGS-84 into EPSG:3857 at layer build, Inverse returns authored view geometry to WGS-84 at commit;
// a direction-named sibling filter class is the deleted form.
public sealed class MercatorFilter(Func<double, double, (double X, double Y)> project) : NetTopologySuite.Geometries.ICoordinateSequenceFilter {
    public static readonly MercatorFilter Forward = new(static (x, y) => SphericalMercator.FromLonLat(x, y));
    public static readonly MercatorFilter Inverse = new(static (x, y) => SphericalMercator.ToLonLat(x, y));

    public bool Done => false;
    public bool GeometryChanged => true;

    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i) {
        (double x, double y) = project(seq.GetX(i), seq.GetY(i));
        seq.SetX(i, x);
        seq.SetY(i, y);
    }
}
```

## [04]-[PICK_AND_SNAPSHOT]

- Owner: `MapPick` — the hit-test fold; the snapshot lane is a `CaptureRow` sibling riding the capture encode fold.
- Entry: `public static Option<BasemapPickReceipt> Pick(MapControl control, ScreenPosition screen)` — `GetMapInfo(screen, layers)` (the cataloged overload, hit-testing the mounted layer set) resolves the topmost feature at the screen point and projects its `id` attribute into the Shell pick state.
- Auto: a pick lands in the same selection vocabulary the viewport pick uses — the Shell selection owner receives the scalar `BasemapPickReceipt` and never a Mapsui type; `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` yields the encoded basemap bytes that decode through `VisualCodec.Decode` and re-encode through `VisualCodec.Encode` as kind basemap, so a basemap baseline rides the same render-hash proof lanes as every visual.
- Packages: Mapsui.Avalonia12, SkiaSharp, LanguageExt.Core
- Growth: a new pick projection is one attribute read on the fold; zero new surface.
- Boundary: no Mapsui type crosses out of this page — picks project to the scalar receipt and snapshots cross as encoded bytes through the capture codec; dashboard geo adjacency (a dashboard tile embedding a basemap) mounts THIS surface as a tile body, never a second map engine.

```csharp signature
// The page-owned pick receipt — scalars only; a Mapsui MPoint or MapInfo never crosses out of this page.
public readonly record struct BasemapPickReceipt(string FeatureId, double WorldX, double WorldY);

public static class MapPick {
    // An absent world position projects to absence — a zero-coordinate sentinel receipt is the deleted form.
    public static Option<BasemapPickReceipt> Pick(MapControl control, ScreenPosition screen) =>
        Optional(control.GetMapInfo(screen, control.Map.Layers))
            .Bind(info => Optional(info.Feature)
                .Bind(feature => Optional(feature["id"] as string)
                    .Bind(id => Optional(info.WorldPosition)
                        .Map(world => new BasemapPickReceipt(id, world.X, world.Y)))));

    // The snapshot lane: GetSnapshot's encoded bytes decode and re-seal through the one capture codec so a
    // basemap baseline carries the same content-hashed RenderReceipt evidence as every visual.
    public static IO<RenderReceipt> Snapshot(VisualRuntime runtime, MapControl control, string key) =>
        from bytes in IO.lift(() => control.GetSnapshot(control.Map.Layers, RenderFormat.Png, quality: 100))
        from image in VisualCodec.Decode(bytes)
        from receipt in VisualCodec.Encode(runtime, image, VisualCodec.Png, "basemap", key)
            .Map(sealed_ => (fun(image.Dispose)(), sealed_).Item2)
        select receipt;
}
```

## [05]-[REDLINE]

- Owner: `RedlineVerb` [Union] — the closed markup-verb vocabulary; `RedlineSurface` — the one `EditManager` authoring owner; the commit leg projects onto the `Collab/sync.md#DURABLE_INTENT` `EditIntent.Annotation` case, never a basemap-local op union.
- Cases: `RedlineVerb` = BeginMark · Modify · Delete · Commit · Discard — begin opens an authoring session with the mark kind (point, polyline, polygon), modify and delete ride `EditManager`'s vertex add/drag/rotate interaction, commit seals the session, discard drops it.
- Entry: `public IO<Fin<Option<EditIntent>>> Drive(RedlineVerb verb)` — every markup gesture discriminates on the verb union; only the `Commit` arm yields `Some(EditIntent.Annotation)`, every other arm yields `None`, so the caller composes one rail and the intent ledger commit stays caller-side (`IntentLedger.Commit` is `Collab/sync.md`'s one transaction rail).
- Auto: authoring runs on a dedicated redline `MemoryLayer` above the overlay stack — the `EditingWidget` binds the interaction and the `EditManager` mutates only that layer, so overlay and tile rows never receive an authored vertex; commit reads the authored `GeometryFeature` geometry in view coordinates, returns it to WGS-84 through `Apply(MercatorFilter.Inverse)` with the copy re-stamped SRID 4326, and projects the ring through `Geometry.Coordinates` into the `RedlineMark` payload `JsonSerializer.SerializeToElement` carries as the `Annotation` `JsonElement` — the exact symmetric leg `MercatorFilter.Forward` runs at overlay ingress, so the round-trip is one parameterized filter, never a second projection path.
- Receipt: a committed redline is one `EditIntent.Annotation(DocKey, TargetId, Payload)` row on the single edit-intent union — durable truth rides the Persistence `OpLogEntry` projection per the `[04]-[PROHIBITIONS]` Loro-byte clause, and the redline layer re-renders from the committed intent, never from retained authoring state; commits and discards contribute a `redline.commit` count through `AppUiTelemetry.Contribute`.
- Packages: Mapsui.Avalonia12 (Mapsui.Nts transitive), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new mark kind is one `BeginMark` kind value; a new markup verb is one `RedlineVerb` case; zero new surface.
- Boundary: `EditManager`/`EditingWidget` stay inside this section — no Mapsui editing type crosses out, the authored geometry leaves only as the WGS-84 `RedlineMark` payload; the `EditManager` session-member spellings (mode start/stop, the bound edit layer slot) bind through the `drive` delegate at composition under the REDLINE_EDIT_SURFACE research row, exactly as the dashboards `GeoLandFold` binds its unverified swap; a redline over the 3D viewport is `Collab/issues.md`'s BCF markup charter — this section owns only the 2D geographic plane.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineShape {
    private RedlineShape() { }
    public sealed record Point(double Lon, double Lat) : RedlineShape;
    public sealed record Path(Seq<(double Lon, double Lat)> Vertices) : RedlineShape;
    public sealed record Area(Seq<(double Lon, double Lat)> Ring) : RedlineShape;
}

public sealed record RedlineMark(RedlineShape Shape);

[SmartEnum<string>]
public sealed partial class RedlineKind {
    public static readonly RedlineKind Point = new("point");
    public static readonly RedlineKind Path = new("path");
    public static readonly RedlineKind Area = new("area");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineVerb {
    private RedlineVerb() { }
    public sealed record BeginMark(RedlineKind Kind) : RedlineVerb;
    public sealed record Modify : RedlineVerb;
    public sealed record Delete : RedlineVerb;
    public sealed record Commit(string DocKey, string TargetId) : RedlineVerb;
    public sealed record Discard : RedlineVerb;
}

public sealed record RedlineSurface(
    EditManager Manager,
    MemoryLayer Marks,
    Func<EditManager, RedlineVerb, Fin<Option<GeometryFeature>>> Apply) {
    public IO<Fin<Option<EditIntent>>> Drive(RedlineVerb verb) =>
        IO.lift(() => Apply(Manager, verb).Bind(authored => verb.Switch(
            state: authored,
            beginMark: static (_, _) => Fin.Succ(Option<EditIntent>.None),
            modify: static (_, _) => Fin.Succ(Option<EditIntent>.None),
            delete: static (_, _) => Fin.Succ(Option<EditIntent>.None),
            commit: static (candidate, commit) => candidate
                .ToFin(new ChartFault.VisualEmpty("redline: commit has no authored feature"))
                .Bind(feature => Sealed(commit, feature))
                .Map(Some),
            discard: static (_, _) => Fin.Succ(Option<EditIntent>.None))));

    // Inverse leg of the one MercatorFilter: authored view geometry returns to WGS-84 before it crosses
    // the intent seam, so no EPSG:3857 coordinate ever lands in durable truth.
    static Fin<EditIntent> Sealed(RedlineVerb.Commit commit, GeometryFeature feature) =>
        Optional(feature.Geometry)
            .ToFin(new ChartFault.VisualEmpty("redline: authored feature has no geometry"))
            .Bind(geometry => fun(() => {
                NetTopologySuite.Geometries.Geometry wgs84 = geometry.Copy();
                wgs84.Apply(MercatorFilter.Inverse);
                wgs84.SRID = 4326;
                return Shape(wgs84).Map(shape => (EditIntent)new EditIntent.Annotation(
                    commit.DocKey,
                    commit.TargetId,
                    JsonSerializer.SerializeToElement(new RedlineMark(shape))));
            })());

    static Fin<RedlineShape> Shape(NetTopologySuite.Geometries.Geometry geometry) => geometry switch {
        NetTopologySuite.Geometries.Point point => Fin.Succ<RedlineShape>(new RedlineShape.Point(point.X, point.Y)),
        NetTopologySuite.Geometries.LineString line => Fin.Succ<RedlineShape>(new RedlineShape.Path(toSeq(line.Coordinates).Map(static at => (at.X, at.Y)))),
        NetTopologySuite.Geometries.Polygon area => Fin.Succ<RedlineShape>(new RedlineShape.Area(toSeq(area.ExteriorRing.Coordinates).Map(static at => (at.X, at.Y)))),
        _ => Fin.Fail<RedlineShape>(new ChartFault.VisualDegenerate($"redline: {geometry.OgcGeometryType} is not an annotation shape")),
    };
}
```

## [06]-[RESEARCH]

- [REDLINE_EDIT_SURFACE]: the `EditManager` session-member spellings the `Drive` delegate binds — the edit-mode start/stop members, the bound edit-layer slot, and the vertex add/drag/rotate verb members — resolve at implementation against the decompiled `Mapsui.Nts` surface; the `EditManager`/`EditingWidget` types, the dedicated redline `MemoryLayer`, the `MercatorFilter.Inverse` return leg, and the `EditIntent.Annotation` commit projection are settled, the session-member spellings inside the delegate are the unverified surface bound at composition.

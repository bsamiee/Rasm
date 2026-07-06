# [APPUI_CHARTS_BASEMAP]

The basemap is the tiled 2D geographic plane beside the Wgpu viewport: one Mapsui `MapControl` hosts one `Map` whose layer stack is data rows — a tile basemap row, NTS overlay rows projecting Bim-owned geospatial features, and widget rows — with navigation through the one `Navigator`, feature picking through `GetMapInfo`, and snapshots through the capture encode fold. The page owns the layer row family, the overlay projection, the pick fold, and the CRS ingress law: Bim owns geodesy (`GeoReference`, `GeoFeature.Reproject`, IfcMapConversion lowering) and AppUi reprojects ONLY WGS-84 input through `SphericalMercator` — a local geodesy kernel is the forbidden form. LiveCharts `GeoMap`/`DrawnMap` stays the CHART-projection row on `dashboards.md`; this page is the TILED-basemap owner — disjoint charters.

## [01]-[INDEX]

- [02]-[MAP_SURFACE]: One `MapControl`/`Map`; the layer row family; navigation verbs.
- [03]-[NTS_OVERLAY]: Bim geospatial features as `GeometryFeature` overlay rows; CRS ingress.
- [04]-[PICK_AND_SNAPSHOT]: Feature hit-test into the pick state; capture snapshots.

## [02]-[MAP_SURFACE]

- Owner: `BasemapLayerRow` [Union] — the closed layer vocabulary; `BasemapSurface` — the one map owner; `MapNav` [Union] — the navigation verb vocabulary.
- Cases: `BasemapLayerRow` = Tile · Overlay · Widget; `MapNav` = CenterOn · ZoomTo · ZoomToBox · FlyTo · RotateTo.
- Entry: `public Fin<Map> Build(Seq<BasemapLayerRow> rows)` — one fold from layer rows to the mounted `Map`; `public IO<Unit> Navigate(MapNav verb)` — every camera move discriminates on the verb union through the one `Navigator`.
- Auto: the tile row defaults to `OpenStreetMap.CreateTileLayer` and any slippy-tile source is one row value; layer z-order is seq order so a stacking change is a row reorder, never an imperative insert; the control binds `Map` through `MapControl.Map` at mount and refreshes through `MapControl.Refresh` on row-set change.
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MapNav {
    private MapNav() { }
    public sealed record CenterOn(MPoint Center) : MapNav;
    public sealed record ZoomTo(double Resolution) : MapNav;
    public sealed record ZoomToBox(MRect Box) : MapNav;
    public sealed record FlyTo(MPoint Center, double Resolution, long DurationMs) : MapNav;
    public sealed record RotateTo(double Degrees) : MapNav;
}

public sealed record BasemapSurface(MapControl Control) {
    public Fin<Map> Build(Seq<BasemapLayerRow> rows) =>
        rows.Fold(Fin.Succ(new Map()), (rail, row) => rail.Bind(map => Mount(map, row)))
            .Map(map => { Control.Map = map; return map; });

    public IO<Unit> Navigate(MapNav verb) =>
        IO.lift(() => ignore(verb.Switch(
            state: Control.Map.Navigator,
            centerOn: static (nav, v) => fun(() => nav.CenterOn(v.Center))(),
            zoomTo: static (nav, v) => fun(() => nav.ZoomTo(v.Resolution))(),
            zoomToBox: static (nav, v) => fun(() => nav.ZoomToBox(v.Box))(),
            flyTo: static (nav, v) => fun(() => nav.FlyTo(v.Center, v.Resolution, v.DurationMs))(),
            rotateTo: static (nav, v) => fun(() => nav.RotateTo(v.Degrees))())));

    static Fin<Map> Mount(Map map, BasemapLayerRow row) =>
        row switch {
            BasemapLayerRow.Tile tile => Fin.Succ(fun(() => { map.Layers.Add(tile.Source()); return map; })()),
            BasemapLayerRow.Overlay overlay => GeoOverlay.Layer(overlay).Map(layer => { map.Layers.Add(layer); return map; }),
            BasemapLayerRow.Widget widget => Fin.Succ(fun(() => { map.Widgets.Add(widget.Source()); return map; })()),
            _ => Fin.Fail<Map>(new ChartFault.LayerRejected(row.GetType().Name)),
        };
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
                mercator.Apply(new MercatorFilter());
                mercator.SRID = 3857;
                GeometryFeature feature = new(mercator);
                feature["id"] = row.FeatureId;
                row.Label.Iter(label => feature["label"] = label);
                return feature;
            })())
            : Fin.Fail<GeometryFeature>(new ChartFault.CrsUnresolved(row.FeatureId, row.Feature.Geometry.SRID));
}

// Coordinate-sequence filter applying SphericalMercator.FromLonLat in place — the ONE AppUi-side reprojection.
public sealed class MercatorFilter : NetTopologySuite.Geometries.ICoordinateSequenceFilter {
    public bool Done => false;
    public bool GeometryChanged => true;

    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i) {
        (double x, double y) = SphericalMercator.FromLonLat(seq.GetX(i), seq.GetY(i));
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
    public static Option<BasemapPickReceipt> Pick(MapControl control, ScreenPosition screen) =>
        Optional(control.GetMapInfo(screen, control.Map.Layers))
            .Bind(info => Optional(info.Feature)
                .Bind(feature => Optional(feature["id"] as string)
                    .Map(id => new BasemapPickReceipt(id, info.WorldPosition?.X ?? 0d, info.WorldPosition?.Y ?? 0d))));
}
```

# [APPUI_CHARTS_BASEMAP]

The basemap is the tiled 2D geographic plane beside the Wgpu viewport: one Mapsui `MapControl` hosts one `Map` whose layer stack is data rows — a tile basemap row, NTS overlay rows projecting Bim-owned geospatial features, and widget rows — with navigation through the one `Navigator`, feature picking through `GetMapInfo`, snapshots through the capture encode fold, and design-review redlining through the `EditManager` surface committing as `EditIntent.Annotation`. The page owns the layer row family, the overlay projection, the pick and snapshot folds, the redline authoring surface, and the CRS ingress law: Bim owns geodesy (`GeoReference`, `GeoFeature.Reproject`, IfcMapConversion lowering) and AppUi reprojects ONLY WGS-84 input through `SphericalMercator` — a local geodesy kernel is the forbidden form. LiveCharts `GeoMap`/`DrawnMap` stays the CHART-projection row on `dashboards.md`; this page is the TILED-basemap owner — disjoint charters.

## [01]-[INDEX]

- [02]-[MAP_SURFACE]: One `MapControl`/`Map`; the layer row family; navigation verbs.
- [03]-[NTS_OVERLAY]: Bim geospatial features as `GeometryFeature` overlay rows; CRS ingress.
- [04]-[PICK_AND_SNAPSHOT]: Feature hit-test into the pick state; capture snapshots.
- [05]-[REDLINE]: Design-review markup over `EditManager`; commit as `EditIntent.Annotation`.

## [02]-[MAP_SURFACE]

- Owner: `BasemapLayerRow` [Union] — the closed layer vocabulary; `BasemapSurface` — the one map owner; `MapNav` [Union] — the navigation verb vocabulary.
- Cases: `BasemapLayerRow` = Tile · Overlay · Widget; `MapNav` = CenterOn · ZoomTo · ZoomToLevel · ZoomToBox · CenterAndZoom · FlyTo · RotateTo; `MapFlight` = Direct · Focus · Traverse — flight timing is declared policy data rather than a caller duration knob.
- Entry: `public Fin<Map> Build(Seq<BasemapLayerRow> rows)` — one fold from layer rows to the mounted `Map`; `public IO<Unit> Navigate(MapNav verb)` — every camera move discriminates on the verb union through the one `Navigator`.
- Auto: the tile row defaults to `OpenStreetMap.CreateTileLayer` and any slippy-tile source is one row value; the map chrome ships as named widget rows — `ScaleBar`, `ZoomButtons`, and `InfoBox` — screen-anchored on `Map.Widgets`; layer z-order is sequence order. `Build` stages a candidate map, disposes it on any row failure, swaps only after complete admission, calls `RefreshGraphics`, and then disposes the replaced map, so a failed rebuild preserves the mounted surface and every successful replacement has one owner; EVERY arm of the row fold returns `Fin` — a source delegate that throws is captured as the row's own `LayerRejected` rather than escaping past the arm that disposes the candidate.
- Receipt: `BasemapSurface.Observe` folds one observation per successful swap and one per dispatched verb under the verb's own intent key, and `RedlineSurface.Observe` one per markup gesture under its disposition, each through `AppUiTelemetry.Contribute` instrument rows whose specs DECLARE those dimensions; faults are typed `ChartFault` cases deriving through `AppUiFaultBand.Chart` (6200) — the one Charts band shared with dashboards and custom.
- Packages: Mapsui.Avalonia12, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new basemap source, overlay family, or widget is one `BasemapLayerRow` value; a new camera move is one `MapNav` case; zero new surface.
- Boundary: ONE `MapControl` and ONE `Map` per basemap surface — a second map control, a per-overlay map, or a parallel tile engine is the deleted form; the transitive Mapsui/Tiling/Nts/Rendering.Skia set stays transitive (the admitted pin is `Mapsui.Avalonia12`); the basemap draws BESIDE the Wgpu viewport as an Avalonia control — it never enters the render graph, and geographic dashboards that need chart-projected geography stay on the LiveCharts `GeoMap` row (`dashboards.md`), the charter split stated on both pages.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BasemapLayerRow {
    private BasemapLayerRow() { }
    public sealed record Tile(string Key, Func<ILayer> Source) : BasemapLayerRow;
    // Symbology is one selector column, never a flat style beside a themed one: every overlay binds
    // `ThemeStyle`, so a parcel set coloured by zoning class, a utility run coloured by service, and a
    // uniform site boundary are ONE shape — `GeoOverlay.Uniform` is the constant selector — and the
    // layer-per-attribute-value proliferation the Growth line rules out is unrepresentable. The two
    // resolution bounds are columns because the provider chain decimates BY resolution and a row that
    // cannot state its visible band cannot state its simplify band either.
    public sealed record Overlay(
        string Key,
        Seq<GeoOverlayRow> Features,
        Func<IFeature, Viewport, IStyle?> Symbology,
        double MinVisible,
        double MaxVisible) : BasemapLayerRow;
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

    // The verb's own dimension value, projected by a total Switch so an eighth case breaks HERE at compile
    // time; a reflected type name would widen the metric's key space silently on any rename.
    public string Key => Switch(
        centerOn: static _ => "center-on",
        zoomTo: static _ => "zoom-to",
        zoomToLevel: static _ => "zoom-to-level",
        zoomToBox: static _ => "zoom-to-box",
        centerAndZoom: static _ => "center-and-zoom",
        flyTo: static _ => "fly-to",
        rotateTo: static _ => "rotate-to");
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
    // compile time; the runtime-silent `_` arm over the closed family is the deleted form. Every arm lands
    // on the SAME Fin the overlay arm already returned, so the fold's Fail arm reaches every row failure.
    static Fin<Map> Mount(Map map, BasemapLayerRow row) => row.Switch(
        state: map,
        tile: static (m, t) => Staged(m, t.Key, () => m.Layers.Add(t.Source())),
        overlay: static (m, o) => GeoOverlay.Layer(o).Map(layer => { m.Layers.Add(layer); return m; }),
        widget: static (m, w) => Staged(m, w.Key, () => m.Widgets.Add(w.Source())));

    // A row's source is an unconstrained composition delegate that does real work — the OSM tile row opens
    // an HTTP client and reads a tile schema — so an escaping throw would carry the staged candidate PAST
    // the `Fail` arm that disposes it, leaking every layer already mounted on it and falsifying the
    // dispose-on-any-row-failure law at the one place it is load-bearing. The capture is the row's own
    // typed refusal, so a bad source names itself instead of surfacing as a provider exception.
    static Fin<Map> Staged(Map map, string key, Action mount) =>
        Try.lift(() => { mount(); return map; }).Run()
            .MapFail(error => (Error)new ChartFault.LayerRejected($"{key}: {error.Message}"));

    public const string LayersInstrument = "rasm.appui.basemap.layers";
    public const string NavigatedInstrument = "rasm.appui.basemap.navigated";

    // Both projections bind at the fold that already holds the fact — the successful swap inside `Build`,
    // the dispatched verb inside `Navigate` — and the per-verb breakdown the description promises rides a
    // DECLARED dimension: an instrument claiming a breakdown its spec never declares yields one
    // undifferentiated series no board can split. The verb's own `Key` is a total `Switch`, so a new
    // `MapNav` case breaks the projection at compile time rather than widening the metric's key space.
    public static Fin<Unit> Observe(InstrumentSet set) => set.Write(LayersInstrument, 1L);

    public static Fin<Unit> Observe(InstrumentSet set, MapNav verb) =>
        set.Write(NavigatedInstrument, 1L,
            new KeyValuePair<string, object?>(AppUiTelemetry.IntentSlot, verb.Key));

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            InstrumentSpec.Count(LayersInstrument, "{rebuild}", "layer-set rebuilds swapped onto the mounted map", MeasureForm.Whole),
            InstrumentSpec.Count(NavigatedInstrument, "{navigation}", "camera moves by verb case", MeasureForm.Whole,
                AppUiTelemetry.IntentSlot));
}
```

## [03]-[NTS_OVERLAY]

- Owner: `GeoOverlayRow` — the per-feature overlay row carrying the Bim-owned `GeoFeature` WHOLE (geometry, attribute table, declared `SourceCrs`) plus its display label; `GeoOverlay` — the projection fold from Bim geospatial output to a Mapsui `MemoryLayer`.
- Entry: `public static Fin<ILayer> Layer(BasemapLayerRow.Overlay overlay)` — one fold; each row's consumed feature wraps as `Mapsui.Nts.GeometryFeature`, styles resolve from the row's `VectorStyle`, and the layer mounts as one `MemoryLayer`.
- Auto: features ARRIVE as Bim-owned `GeoFeature` rows carrying their `GeoReference` lineage (the `GeoReferenceProjector` IfcMapConversion/IfcProjectedCRS lowering) already reprojected to WGS-84 by Bim's `GeoFeature.Reproject` — the declared seam, both sides (`Rasm.Bim` Semantics/geospatial -> AppUi Charts) — so the row's `SourceCrs`/SRID state IS the CRS evidence the gate reads; AppUi's ONLY reprojection is WGS-84 lon/lat -> EPSG:3857 through `SphericalMercator.FromLonLat` under `ProjectionDefaults.Projection` at the layer-build edge.
- Receipt: an overlay row whose feature still declares a projected frame (or a non-4326 SRID) folds to `ChartFault.CrsUnresolved` — the ingress law enforced as a typed fault, never a silent draw at wrong coordinates.
- Packages: Mapsui.Avalonia12 (Mapsui.Nts transitive), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new overlay family (site boundary, utility run, parcel, analysis heat cells) is a seq of rows on one Overlay layer row, and attribute-driven symbology within a family is one `Symbology` selector reading the consumed `GeoFeature` attribute table — never a layer per attribute value; zero new surface.
- Boundary: an overlay mounts as `Layer` over the `IndexedMemoryProvider` → `GeometryIntersectionProvider` → `GeometrySimplifyProvider` decorator chain under one `ThemeStyle`, so viewport clipping, resolution-driven decimation, and per-feature symbology are all provider and style policy — a hand-rolled cull, a resolution branch at the bind edge, and a `MemoryLayer` holding every vertex resident at every zoom are the deleted forms; geodesy stays Bim's — `GeoFeature.Reproject`, datum transforms, and projected-CRS handling never re-implement here, and a local geodesy kernel (a proj4 port, a datum table, a second `SphericalMercator` beside the Mapsui primitive) is the FORBIDDEN form; NTS geometry types cross the seam as values inside Bim-owned features and are wrapped, never re-modeled; `GeoTiles.Catalog` TileJSON from Bim's `GeoModel.ToTiles` mounts as one Tile row when the vector-tile lane ships — a row, not a new surface.

```csharp signature
// The ingress row carries the Bim-owned GeoFeature WHOLE — geometry + attribute table + declared
// SourceCrs — so CRS authority is the seam contract the feature itself carries, never an SRID sniff on
// a bare geometry; Label is the only display column minted beside the consumed feature.
public sealed record GeoOverlayRow(
    string FeatureId,
    Rasm.Bim.Semantics.GeoFeature Feature,
    Option<string> Label);

public static class GeoOverlay {
    // The residency law of the 2D plane, spelled as the three admitted provider decorators rather than a
    // hand-rolled cull: the indexed provider replaces the linear feature array so a fetch is an envelope
    // query, the intersection provider clips each fetch to the viewport `MRect`, and the simplify provider
    // decimates by the LIVE fetch resolution (a null tolerance drives it off `fetchInfo.Resolution`), so a
    // parcel ring drawn at city zoom carries tens of vertices instead of thousands. `MemoryLayer.Features`
    // holding every vertex of every family at every zoom is the deleted form — this is the same budget law
    // `Render/meshlets` `ResidencyBudget` enforces for geometry VRAM, applied on the plane.
    public static Fin<ILayer> Layer(BasemapLayerRow.Overlay overlay) =>
        overlay.Features
            .Traverse(Project)
            .Map(features => (ILayer)new Layer(overlay.Key) {
                DataSource = new GeometrySimplifyProvider(new GeometryIntersectionProvider(new IndexedMemoryProvider(features.ToArray()))),
                Style = new ThemeStyle(overlay.Symbology),
                MinVisible = overlay.MinVisible,
                MaxVisible = overlay.MaxVisible,
            })
            .As();

    // The constant selector: a uniform overlay states its one style HERE, so `Overlay` carries no second
    // style column and the themed and uniform families never fork into sibling layer builders.
    public static Func<IFeature, Viewport, IStyle?> Uniform(VectorStyle style) => (_, _) => style;

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
- Entry: `public static Option<BasemapPickReceipt> Pick(MapControl control, ScreenPosition screen)` — `GetMapInfo(screen, layers)` (the cataloged overload, hit-testing the mounted layer set) resolves the topmost feature at the screen point and projects its `id` attribute into the Shell pick state; `public static IO<RenderReceipt> Snapshot(VisualRuntime runtime, MapControl control, string key)` — the encoded basemap bytes re-sealed through the one capture codec.
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
    // basemap baseline carries the same content-hashed RenderReceipt evidence as every visual. Release
    // brackets the ACQUISITION — the decoded image is a native the encode may fail against, and disposing
    // it inside a `.Map` on the encode RESULT bound release to success and leaked the image on every
    // refused encode.
    public static IO<RenderReceipt> Snapshot(VisualRuntime runtime, MapControl control, string key) =>
        from bytes in IO.lift(() => control.GetSnapshot(control.Map.Layers, RenderFormat.Png, quality: 100))
        from receipt in VisualCodec.Decode(bytes).Bracket(
            image => VisualCodec.Encode(runtime, image, VisualCodec.Png, "basemap", key),
            static image => IO.lift(() => { image.Dispose(); return unit; }))
        select receipt;
}
```

## [05]-[REDLINE]

- Owner: `RedlineVerb` [Union] — the closed markup-verb vocabulary; `RedlineSurface` — the one `EditManager` authoring owner; the commit leg projects onto `EditIntent.Annotation`, never a basemap-local op union.
- Cases: `RedlineVerb` = BeginMark · Modify · Delete · Commit · Discard; `RedlineKind` = Point · Path · Area; `RedlineDelta` = Upsert · Delete; `RedlineShape` = Point · Path · Area · Collection. `BeginMark` carries the kind, commit carries document, target, and the stroke/fill/opacity/dash policy, and delete carries the durable target identity.
- Entry: `public MemoryLayer Mounted()` — the marks layer resolved once under the shared symbology selector; `public IO<Fin<Option<EditIntent>>> Drive(RedlineVerb verb)` — every markup gesture discriminates on the verb union; `Commit` emits an upsert annotation and `Delete` emits a delete annotation, while local begin, modify, and discard return `None`; the caller composes one rail and the intent ledger commit stays caller-side (`IntentLedger.Commit` is `Collab/sync.md`'s one transaction rail).
- Auto: authoring runs on a dedicated redline `MemoryLayer` above the overlay stack, mounted through `Mounted` under one `ThemeStyle(Symbology)` so a mark colours by its own `IssueStatus` attribute on the same symbology axis every overlay binds. `Apply` is the composition adapter over the research-gated `EditManager` session members and returns the current `GeometryFeature` for a sealed commit. `Sealed` admits document and target identities, copies geometry, applies `MercatorFilter.Inverse`, stamps `SRID` `4326`, and preserves finite points, paths, polygon shells and holes, multi-geometries, and heterogeneous collections before serializing `RedlineDelta.Upsert`; `Delete` needs no surviving feature and serializes `RedlineDelta.Delete`. Degenerate geometry and invalid paint, fill, width, opacity, or dash policy fail as `ChartFault.VisualDegenerate`.
- Receipt: a committed redline is one `EditIntent.Annotation(DocKey, TargetId, Payload)` row on the single edit-intent union — durable truth rides the Persistence `OpLogEntry` projection per the `[04]-[BOUNDARIES]` Loro-byte clause, and the redline layer re-renders from the committed intent, never from retained authoring state; every gesture folds one `redline.commit` observation at `Drive` under the outcome slot its spec declares, so commit, discard, and local authoring stay separable series.
- Packages: Mapsui.Avalonia12 (Mapsui.Nts transitive), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new mark kind is one `RedlineKind` row plus its corresponding `RedlineShape` case when the payload arity differs; a new markup verb is one `RedlineVerb` case; a new review-state colouring is one branch inside the bound `Symbology` selector, never a second marks layer; zero new surface.
- Boundary: `EditManager` and `EditingWidget` remain inside this section; authored geometry leaves only as the WGS-84 `RedlineDelta` payload, and delete carries no stale geometry. The marks layer is the ONE `MemoryLayer` this page keeps: `EditManager` writes its `.Features` in place, which the overlay provider chain structurally cannot receive, so the residency decorators stop at the overlay boundary while the symbology selector crosses it — a decorated edit layer and a flat per-layer redline style are both deleted forms. The session-member spellings bind through `Apply` at composition under `REDLINE_EDIT_SURFACE`. A redline over the 3D viewport remains the BCF markup charter; this section owns only the 2D geographic plane.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineShape {
    private RedlineShape() { }
    public sealed record Point(double Lon, double Lat) : RedlineShape;
    public sealed record Path(Seq<(double Lon, double Lat)> Vertices) : RedlineShape;
    public sealed record Area(Seq<(double Lon, double Lat)> Shell, Seq<Seq<(double Lon, double Lat)>> Holes) : RedlineShape;
    public sealed record Collection(Seq<RedlineShape> Members) : RedlineShape;
}

public sealed record RedlineStyle(string PaintKey, Option<string> FillKey, double Width, double Opacity, Seq<double> Dash);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineDelta {
    private RedlineDelta() { }
    public sealed record Upsert(RedlineMark Mark) : RedlineDelta;
    public sealed record Delete : RedlineDelta;
}

public sealed record RedlineMark(RedlineShape Shape, RedlineStyle Style);

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
    public sealed record Delete(string DocKey, string TargetId) : RedlineVerb;
    public sealed record Commit(string DocKey, string TargetId, RedlineStyle Style) : RedlineVerb;
    public sealed record Discard : RedlineVerb;
}

// Marks stays a MemoryLayer because EditManager writes its `.Features` collection DIRECTLY — the
// overlay provider chain decorates a fetch and cannot receive an in-place authoring write, so the
// residency law that governs overlays does not reach the edit layer and a decorated redline layer is
// unspellable. Symbology is a different axis and DOES reach it: the layer takes the same
// `ThemeStyle(Func<IFeature, Viewport, IStyle?>)` selector every overlay takes, so a redline mark
// colours by its own `IssueStatus` attribute through the one symbology owner. A flat per-layer
// VectorStyle is the deleted form — it forced every mark to one colour and forked review state into a
// layer-per-status roster the page's own Growth line rules out.
public sealed record RedlineSurface(
    EditManager Manager,
    MemoryLayer Marks,
    Func<IFeature, Viewport, IStyle?> Symbology,
    Func<EditManager, RedlineVerb, Fin<Option<GeometryFeature>>> Apply) {
    // Display style resolves once at mount from the SAME selector shape overlays bind, so the review
    // board and the redline plane read one symbology vocabulary.
    public MemoryLayer Mounted() {
        Marks.Style = new ThemeStyle(Symbology);
        return Marks;
    }

    public IO<Fin<Option<EditIntent>>> Drive(RedlineVerb verb) =>
        IO.lift(() => Apply(Manager, verb).Bind(authored => verb.Switch(
            state: authored,
            beginMark: static (_, _) => Fin.Succ(Option<EditIntent>.None),
            modify: static (_, _) => Fin.Succ(Option<EditIntent>.None),
            delete: static (_, command) => AdmitIdentity(command.DocKey, command.TargetId).Map(_ => Some<EditIntent>(new EditIntent.Annotation(
                command.DocKey,
                command.TargetId,
                JsonSerializer.SerializeToElement<RedlineDelta>(new RedlineDelta.Delete())))),
            commit: static (candidate, commit) => candidate
                .ToFin(new ChartFault.VisualEmpty("redline: commit has no authored feature"))
                .Bind(feature => Sealed(commit, feature))
                .Map(Some),
            discard: static (_, _) => Fin.Succ(Option<EditIntent>.None))));

    // Inverse leg of the one MercatorFilter: authored view geometry returns to WGS-84 before it crosses
    // the intent seam, so no EPSG:3857 coordinate ever lands in durable truth.
    static Fin<EditIntent> Sealed(RedlineVerb.Commit commit, GeometryFeature feature) =>
        AdmitIdentity(commit.DocKey, commit.TargetId)
            .Bind(_ => Optional(feature.Geometry)
            .ToFin(new ChartFault.VisualEmpty("redline: authored feature has no geometry"))
            .Bind(geometry => fun(() => {
                NetTopologySuite.Geometries.Geometry wgs84 = geometry.Copy();
                wgs84.Apply(MercatorFilter.Inverse);
                wgs84.SRID = 4326;
                return Shape(wgs84).Bind(shape => !string.IsNullOrWhiteSpace(commit.Style.PaintKey)
                    && commit.Style.FillKey.ForAll(static key => !string.IsNullOrWhiteSpace(key))
                    && double.IsFinite(commit.Style.Width) && commit.Style.Width > 0d
                    && double.IsFinite(commit.Style.Opacity) && commit.Style.Opacity is >= 0d and <= 1d
                    && commit.Style.Dash.ForAll(static interval => double.IsFinite(interval) && interval > 0d)
                    ? Fin.Succ((EditIntent)new EditIntent.Annotation(
                    commit.DocKey,
                    commit.TargetId,
                    JsonSerializer.SerializeToElement<RedlineDelta>(new RedlineDelta.Upsert(new RedlineMark(shape, commit.Style)))))
                    : Fin.Fail<EditIntent>(new ChartFault.VisualDegenerate("redline: paint, fill, width, opacity, or dash is invalid")));
            })()));

    static Fin<Unit> AdmitIdentity(string docKey, string targetId) =>
        !string.IsNullOrWhiteSpace(docKey) && !string.IsNullOrWhiteSpace(targetId)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ChartFault.VisualDegenerate("redline: document and target identities are required"));

    static Fin<RedlineShape> Shape(NetTopologySuite.Geometries.Geometry geometry) => geometry switch {
        NetTopologySuite.Geometries.Point point when double.IsFinite(point.X) && double.IsFinite(point.Y) =>
            Fin.Succ<RedlineShape>(new RedlineShape.Point(point.X, point.Y)),
        NetTopologySuite.Geometries.LineString line => Vertices(line.Coordinates, minimum: 2, kind: "path")
            .Map(vertices => (RedlineShape)new RedlineShape.Path(vertices)),
        NetTopologySuite.Geometries.Polygon area => Vertices(area.ExteriorRing.Coordinates, minimum: 4, kind: "area shell")
            .Bind(shell => toSeq(area.InteriorRings)
                .Traverse(ring => Vertices(ring.Coordinates, minimum: 4, kind: "area hole"))
                .As()
                .Map(holes => (RedlineShape)new RedlineShape.Area(shell, holes))),
        NetTopologySuite.Geometries.GeometryCollection collection when collection.NumGeometries > 0 => toSeq(Enumerable.Range(0, collection.NumGeometries))
            .Traverse(index => Shape(collection.GetGeometryN(index)))
            .As()
            .Map(members => (RedlineShape)new RedlineShape.Collection(members)),
        _ => Fin.Fail<RedlineShape>(new ChartFault.VisualDegenerate($"redline: {geometry.OgcGeometryType} is not an annotation shape")),
    };

    static Fin<Seq<(double Lon, double Lat)>> Vertices(NetTopologySuite.Geometries.Coordinate[] coordinates, int minimum, string kind) {
        Seq<(double Lon, double Lat)> vertices = toSeq(coordinates).Map(static at => (at.X, at.Y));
        return vertices.Count >= minimum && vertices.ForAll(static at => double.IsFinite(at.Lon) && double.IsFinite(at.Lat))
            ? Fin.Succ(vertices)
            : Fin.Fail<Seq<(double Lon, double Lat)>>(new ChartFault.VisualDegenerate($"redline: {kind} vertices are invalid"));
    }

    public const string CommitInstrument = "rasm.appui.redline.commit";

    // `Drive` is the one place a disposition exists, so the projection binds there and the count carries it
    // as the DECLARED outcome dimension — a spec promising a per-disposition breakdown with no declared
    // slot folds commits, discards, and every local gesture into one number no board can separate. The
    // disposition is a total Switch, so a sixth verb breaks this projection rather than counting untagged.
    public static Fin<Unit> Observe(InstrumentSet set, RedlineVerb verb) =>
        set.Write(CommitInstrument, 1L,
            new KeyValuePair<string, object?>(AppUiTelemetry.OutcomeSlot, Disposition(verb)));

    static string Disposition(RedlineVerb verb) => verb.Switch(
        beginMark: static _ => "begin",
        modify: static _ => "modify",
        delete: static _ => "delete",
        commit: static _ => "commit",
        discard: static _ => "discard");

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            InstrumentSpec.Count(CommitInstrument, "{commit}", "redline gestures by disposition", MeasureForm.Whole,
                AppUiTelemetry.OutcomeSlot));
}
```

## [06]-[RESEARCH]

- [REDLINE_EDIT_SURFACE]-[BLOCKED]: which `EditManager` members drive session start/stop, edit-layer selection, and vertex add/drag/rotate, so `Apply` binds each by name; route: `tools.assay api query --key Mapsui.Nts --symbol EditManager` once `Mapsui.Nts` registers as an assay source — the rail answers `no 'Mapsui.Nts' source` today.

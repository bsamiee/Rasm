# [APPUI_CHARTS_BASEMAP]

The basemap is the tiled 2D geographic plane beside the Wgpu viewport: one Mapsui `MapControl` hosts one `Map` whose layer stack is data rows — a `BasemapSource` tile row carrying its own attribution, cache, and user-agent posture, NTS overlay rows projecting Bim-owned geospatial features — resident sets and the Bim MVT pyramid alike — under uniform, themed, or colormap-driven choropleth symbology, and widget rows re-tinted from the one token resolve — with navigation through the one `Navigator`, tile health rendered as fact through the layer's own fetch state, feature picking through `GetMapInfo` with tolerance and multi-hit disambiguation, snapshots through the capture encode fold, and design-review redlining through the `EditManager` surface committing as `EditIntent.Annotation` and recording onto the one `EditHistory` recorder. The page owns the layer row family, the tile-source catalog, the overlay and choropleth projection, the pick, hover, and snapshot folds, the redline authoring surface, and the CRS ingress law: Bim owns geodesy (`GeoReference`, `GeoFeature.Reproject`, IfcMapConversion lowering) and AppUi reprojects ONLY WGS-84 input through `SphericalMercator` — a local geodesy kernel on this plane is the forbidden form. LiveCharts `GeoMap`/`DrawnMap` stays the CHART-projection row on `dashboards.md`; this page is the TILED-basemap owner — disjoint charters.

## [01]-[INDEX]

- [02]-[MAP_SURFACE]: One `MapControl`/`Map`; the layer row family; tile sources, attribution, health; navigation verbs.
- [03]-[NTS_OVERLAY]: Bim geospatial features as `GeometryFeature` overlay rows; CRS ingress; the choropleth seam.
- [04]-[PICK_AND_SNAPSHOT]: Feature hit-test into the pick state; tolerance, disambiguation, hover; capture snapshots.
- [05]-[REDLINE]: Design-review markup over `EditManager`; commit as `EditIntent.Annotation` onto the recorder.

## [02]-[MAP_SURFACE]

- Owner: `BasemapLayerRow` `[Union]` — the closed layer vocabulary; `BasemapSource` `[SmartEnum<string>]` — the tile-source catalog carrying URL template, zoom band, attribution, and dark-variant affinity; `TilePolicy` — the cache-and-agent posture a tile row mounts under; `WidgetRow` `[SmartEnum<string>]` — the chrome catalog carrying its map-taking constructor beside its token re-tint; `TileHealth` `[Union]` — the rendered fetch state; `BasemapSurface` — the one map owner; `MapNav` `[Union]` — the navigation verb vocabulary.
- Cases: `BasemapLayerRow` = Tile · Overlay · Vectors · Widget; `BasemapSource` = osm · carto-light · carto-dark, each carrying its dark-variant affinity so a variant flip selects a source rather than filtering pixels; `TileHealth` = Idle · Fetching · Ready · Failed · Offline; `WidgetRow` = scale-bar · zoom-buttons · info-box · coordinates · ruler, with each tile layer's own attribution credit riding the layer rather than a row of its own; `MapNav` = CenterOn · ZoomTo · ZoomToLevel · ZoomToBox · CenterAndZoom · FlyTo · RotateTo; `MapFlight` = Direct · Focus · Traverse — flight timing is declared policy data rather than a caller duration knob.
- Entry: `public Fin<Map> Build(Seq<BasemapLayerRow> rows, ChartInk ink, ResolvedTheme theme)` — one fold from layer rows to the mounted `Map`, seating the background colour and re-tinting every widget from the same resolve; `public IO<Fin<Unit>> Navigate(MapNav verb)` — every camera move discriminates on the verb union through the one `Navigator` on the same `Fin` rail every sibling entry answers; `public IDisposable Watch(Action<TileHealth> onHealth)` — the tile-health subscription over the mounted tile layers' own fetch signals; `public Fin<Unit> Retint(ChartInk ink, ResolvedTheme theme)` — the in-place chrome and background swap.
- Auto: a tile row names a `BasemapSource` row and a `TilePolicy`, and the row's own `Layer()` builds the `TileLayer` over an `HttpTileSource` carrying that source's URL template, zoom band, `Attribution`, and the policy's `IPersistentCache<byte[]>` — so a DARK basemap is a source row beside the light one rather than a colour filter over tiles, which the renderer structurally cannot apply because the raster path exposes no tint, blend, or colour-matrix hook at all. The user-agent posture is the policy's own column written onto the source's `HttpClient` through `ConfigureHttpRequestMessage`, defaulting to `HttpClientTools.GetDefaultApplicationUserAgent()`, because the default source's terms require an identifying agent and a shared default agent is how a product gets rate-limited off a public tile service. ATTRIBUTION IS REQUIRED: every tile row contributes its source's own `Attribution` onto the layer's `HyperlinkWidget`, `Map.GetWidgetsOfMapAndLayers()` surfaces it, and `Build` REFUSES a row set carrying a tile row whose source declares no attribution text — a basemap drawing another party's tiles without its credit is a licence breach, so absence refuses at admission rather than shipping. `Map.BackColor` seats the resolved panel token so the gap around a partially-fetched world reads as the product's own surface rather than as white. The map chrome ships as `WidgetRow` values carrying the constructor arity each widget's signature demands — `ScaleBarWidget(Map)` and `MapInfoWidget(Map, …)` bind their map at construction and expose NO parameterless arity — beside the re-tint each widget's own colour members take, so a theme swap writes `TextColor`, `BackColor`, and `StrokeColor` in place and the chrome follows the variant with no re-mount. Layer z-order is sequence order. `Build` stages a candidate map, disposes it on any row failure, swaps only after complete admission, calls `RefreshGraphics`, and then disposes the replaced map, so a failed rebuild preserves the mounted surface and every successful replacement has one owner; EVERY arm of the row fold returns `Fin` — a source delegate that throws is captured as the row's own `LayerRejected` rather than escaping past the arm that disposes the candidate. `Watch` folds each tile layer's `Busy` transitions and its `DataChanged` exception payload into the `TileHealth` union, so loading, error, and offline are RENDERED FACTS a surface states rather than a blank tile grid the viewer must interpret; an offline verdict is the connectivity-refused arm of that same payload, distinguished because a viewer whose tiles are cached-only needs a different message than one whose source rejected the request.
- Receipt: `BasemapSurface.Observe` folds one observation per successful swap, one per dispatched verb under the verb's own intent key, and one per tile-health transition under its own state key, and `RedlineSurface.Observe` one per markup gesture under its disposition, each through `AppUiTelemetry.Contribute` instrument rows whose specs DECLARE those dimensions; faults are typed `ChartFault` cases deriving through `AppUiFaultBand.Chart` (6200) — the one Charts band shared with dashboards and custom.
- Packages: Mapsui.Avalonia12, BruTile, System.Reactive, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new basemap appearance is one `BasemapSource` row carrying its template, band, and attribution; a new cache or agent posture is one `TilePolicy` value; a new chrome surface is one `WidgetRow` row carrying its constructor and its re-tint; a new health state is one `TileHealth` case; a new overlay family is one `BasemapLayerRow.Overlay` value and a new tiled vector source one `BasemapLayerRow.Vectors` value; a new camera move is one `MapNav` case; zero new surface.
- Boundary: ONE `MapControl` and ONE `Map` per basemap surface — a second map control, a per-overlay map, or a parallel tile engine is the deleted form; the Mapsui/Tiling/Rendering.Skia set stays transitive under the `Mapsui.Avalonia12` pin while `Mapsui.Nts` is admitted directly, because `[03]` and `[05]` compose its geometry, provider, and editing members by name, and `BruTile` is admitted directly because `HttpTileSource`, `GlobalSphericalMercator`, `Attribution`, and `FileCache` are the source vocabulary a non-default tile row is spelled in and `ITileSchema.GetTileInfos` is the covering-tile arithmetic the vector pyramid's provider reads its LOD off. A dark basemap is a SOURCE, never a post-effect: `RasterStyleRenderer` draws a tile under layer and style opacity and strokes `RasterStyle.Outline` alone, and no colour-matrix, tint, or blend hook reaches the raster path — `Image.BlendModeColor` tints symbol imagery only — so an inverted or hue-rotated basemap is unspellable rather than merely discouraged, and a row claiming one names a member the renderer never reads. EVERY entry on this surface answers `Fin`: `Navigate` returning a bare `IO<Unit>` swallowed an out-of-band level, a non-finite resolution, and a degenerate box into a silent no-op while `Build`, `Retint`, and every `[03]`/`[05]` entry beside it refused by name, so a caller could not compose the two without discriminating on the shape of the rail. The basemap draws BESIDE the Wgpu viewport as an Avalonia control — it never enters the render graph, and geographic dashboards that need chart-projected geography stay on the LiveCharts `GeoMap` row (`dashboards.md`), the charter split stated on both pages.

```csharp signature
// The tile-source catalog. A row carries everything a slippy source IS — the URL template its tiles resolve
// through, the zoom band its schema covers, the attribution its licence requires, and the theme appearance
// it belongs to — so selecting a dark basemap under a dark variant is a ROW LOOKUP and a new source is one
// row. Attribution rides the row rather than the caller because the credit is a property of the source's
// terms, not of the surface that happens to mount it, and a caller-supplied credit drifts from the source
// it credits the first time a row is reused.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BasemapSource {
    public static readonly BasemapSource Osm = new("osm",
        "https://tile.openstreetmap.org/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 19,
        new Attribution("OpenStreetMap contributors", "https://www.openstreetmap.org/copyright"), dark: false);
    public static readonly BasemapSource CartoLight = new("carto-light",
        "https://basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 20,
        new Attribution("CARTO, OpenStreetMap contributors", "https://carto.com/attributions"), dark: false);
    public static readonly BasemapSource CartoDark = new("carto-dark",
        "https://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 20,
        new Attribution("CARTO, OpenStreetMap contributors", "https://carto.com/attributions"), dark: true);

    public string Template { get; }

    public int MinZoom { get; }

    public int MaxZoom { get; }

    public Attribution Credit { get; }

    public bool Dark { get; }

    // The variant-matched row, so a theme swap re-selects the SOURCE rather than filtering the pixels the
    // previous one already fetched; a variant with no matching row falls to the OSM baseline, because a
    // basemap that renders nothing is worse than one whose palette lags its chrome by one variant. The
    // column is the variant's own `Dark` read, so the high-contrast rows resolve with the plain ones and no
    // second appearance vocabulary exists here.
    public static BasemapSource For(ThemeVariantRow variant) =>
        toSeq(Items).Find(row => row.Dark == variant.Dark).IfNone(Osm);

    // The one source construction: the global spherical-mercator schema bounded by the row's own zoom band,
    // the policy's persistent cache, the row's own attribution, and the policy's agent write. A source built
    // without the agent write inherits the shared library default, which is exactly the identity a public
    // tile service rate-limits.
    // The absent cache writes a null slot, which the package's own signature takes as "no persistent store";
    // the probe carries its own presence proof, so the payload read is admitted rather than an unguarded
    // peek, and no unsafe-match escape hatch exists on the carrier to reach for instead.
    public ITileSource Source(TilePolicy policy) =>
        new HttpTileSource(
            new GlobalSphericalMercator(minZoomLevel: MinZoom, maxZoomLevel: MaxZoom),
            Template,
            name: Key,
            persistentCache: policy.Cache is { IsSome: true, Case: IPersistentCache<byte[]> cache } ? cache : null,
            attribution: Credit,
            configureHttpRequestMessage: request => request.Headers.TryAddWithoutValidation("User-Agent", policy.Agent));
}

// The fetch posture a tile row mounts under. `Agent` is required by the default source's terms; `Cache` is
// the persistent store an offline session reads from, absent for a memory-only session; `MinTiles`/`MaxTiles`
// bound the in-memory tile cache the layer keeps around the viewport. The values are a POLICY value rather
// than four call-site arguments, so one posture serves every source and a retune is one value.
public sealed record TilePolicy(string Agent, Option<IPersistentCache<byte[]>> Cache, int MinTiles, int MaxTiles) {
    public static TilePolicy Of(string product, Option<string> cacheDirectory) =>
        new($"{product} ({HttpClientTools.GetDefaultApplicationUserAgent()})",
            cacheDirectory.Map(directory => (IPersistentCache<byte[]>)new FileCache(directory, "png", Duration.FromDays(14).ToTimeSpan())),
            MinTiles: 200,
            MaxTiles: 300);
}

// What a tile stack is DOING, as a value a surface renders rather than as a blank grid a viewer interprets.
// `Offline` is distinguished from `Failed` because the two need different messages and different affordances:
// a refused request retries, an absent network waits, and a cached-only session is usable while the second is
// not. The layer publishes both signals — `Busy` transitions and a `DataChanged` exception payload — so the
// state is READ off the package rather than guessed from tile arrival.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileHealth {
    private TileHealth() { }
    public sealed record Idle : TileHealth;
    public sealed record Fetching(int Outstanding) : TileHealth;
    public sealed record Ready : TileHealth;
    public sealed record Failed(string Layer, string Detail) : TileHealth;
    public sealed record Offline(string Layer) : TileHealth;

    // The state's own dimension value, projected by a total Switch so a sixth case breaks HERE at compile
    // time rather than widening the metric's key space silently.
    public string Key => Switch(
        idle: static _ => "idle",
        fetching: static _ => "fetching",
        ready: static _ => "ready",
        failed: static _ => "failed",
        offline: static _ => "offline");

    // The one classification of a layer's published signals. A socket or DNS failure is OFFLINE — the
    // request never reached a server — while anything the server answered is FAILED, so a viewer sees "no
    // network" rather than "tile server error" when the cause is the former.
    public static TileHealth Of(ILayer layer, Option<Exception> fault) =>
        fault.Match(
            Some: error => error is HttpRequestException { InnerException: SocketException } or SocketException
                ? new Offline(layer.Name)
                : (TileHealth)new Failed(layer.Name, error.Message),
            None: () => layer.Busy ? new Fetching(1) : new Ready());
}

// The chrome catalog: each row carries the map-taking factory its widget's signature actually demands and
// the re-tint its own colour members expose. `ScaleBarWidget` and `MapInfoWidget` bind their map at
// construction and publish no parameterless arity at all, so a zero-argument mint does not compile — the
// row therefore holds `Func<Map, IWidget>` and the widget column is a real constructor rather than a
// placeholder. The re-tint is the second column because a widget mounted once must FOLLOW the variant: a
// theme swap writes these members in place and the chrome re-draws on the next frame with no re-mount.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WidgetRow {
    public static readonly WidgetRow ScaleBar = new("scale-bar", typeof(ScaleBarWidget),
        static map => new ScaleBarWidget(map),
        static (widget, ink) => { if (widget is ScaleBarWidget bar) { bar.TextColor = Tint(ink, ChartChrome.AxisLabel); bar.Halo = Tint(ink, ChartChrome.FrameFill); } });
    public static readonly WidgetRow ZoomButtons = new("zoom-buttons", typeof(ZoomInOutWidget),
        static _ => new ZoomInOutWidget(),
        static (widget, ink) => { if (widget is ZoomInOutWidget zoom) { zoom.StrokeColor = Tint(ink, ChartChrome.FrameStroke); zoom.TextColor = Tint(ink, ChartChrome.TooltipText); zoom.BackColor = Tint(ink, ChartChrome.TooltipBack); } });
    public static readonly WidgetRow InfoBox = new("info-box", typeof(MapInfoWidget),
        static map => new MapInfoWidget(map, () => map.Layers),
        static (widget, ink) => Text(widget, ink));
    public static readonly WidgetRow Coordinates = new("coordinates", typeof(MouseCoordinatesWidget),
        static _ => new MouseCoordinatesWidget(),
        static (widget, ink) => Text(widget, ink));
    public static readonly WidgetRow Ruler = new("ruler", typeof(RulerWidget),
        static _ => new RulerWidget(),
        static (widget, ink) => { if (widget is RulerWidget ruler) { ruler.Color = Tint(ink, ChartChrome.Crosshair); ruler.ColorOfBeginAndEndDots = Tint(ink, ChartChrome.CrosshairChip); } });

    // The declared widget shape, so the re-tint sweep matches a mounted widget to its row by TYPE without
    // constructing one to compare against — a mint-to-match walk allocates a widget per row per swap and
    // opens a live `Map` binding for a comparison that discards it.
    public Type Shape { get; }

    [UseDelegateFromConstructor]
    public partial IWidget Mint(Map map);

    [UseDelegateFromConstructor]
    public partial void Retint(IWidget widget, ChartInk ink);

    // Every text-box descendant re-tints through one write, so the readout, the coordinate strip, and every
    // layer's own attribution credit share one pigment and a per-widget colour ladder is unspellable —
    // `HyperlinkWidget`, `MapInfoWidget`, and `MouseCoordinatesWidget` all descend from `TextBoxWidget`, so
    // the one pattern reaches the credit the tile layer mints without a row of its own.
    internal static void Text(IWidget widget, ChartInk ink) {
        if (widget is TextBoxWidget box) {
            box.TextColor = Tint(ink, ChartChrome.TooltipText);
            box.BackColor = Tint(ink, ChartChrome.TooltipBack);
        }
    }

    // The ONE crossing from the chart chrome roster onto the map's own colour value. `ChartInk` resolves and
    // re-tints every chrome row already, so the map chrome reads that same resolve rather than a second
    // token lookup — a widget colour and a chart hairline are then one generated rung by construction, and a
    // basemap-local paint roster is the deleted form.
    internal static Mapsui.Styles.Color Tint(ChartInk ink, ChartChrome chrome) =>
        ink.Tint(chrome) switch { var lvc => Mapsui.Styles.Color.FromArgb(lvc.A, lvc.R, lvc.G, lvc.B) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BasemapLayerRow {
    private BasemapLayerRow() { }
    // The tile row names its SOURCE and its POSTURE rather than carrying an opaque layer delegate: the row
    // is then a value a variant flip can re-select, an attribution admission can read, and a health watch
    // can identify, none of which a bare `Func<ILayer>` permits.
    public sealed record Tile(string Key, BasemapSource Source, TilePolicy Policy) : BasemapLayerRow;
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
    // The tiled vector row. It is NOT a `Tile`: a raster tile row answers image bytes the renderer draws
    // under a `RasterStyle`, while this one answers FEATURES the symbology selector inks per attribute, so
    // collapsing the two would force a vector pyramid through a fetch hook that admits one feature per tile.
    // The visibility band is a column here for the same reason it is on `Overlay` — a pyramid still declares
    // the resolutions its content is authored for, and the schema's zoom band alone cannot state that.
    public sealed record Vectors(
        string Key,
        GeoTileProvider Tiles,
        Func<IFeature, Viewport, IStyle?> Symbology,
        double MinVisible,
        double MaxVisible) : BasemapLayerRow;
    // The widget row names a catalog ROW, so the map-taking constructor and the token re-tint both ride the
    // vocabulary rather than a bare factory the swap could never reach a second time.
    public sealed record Widget(WidgetRow Row) : BasemapLayerRow;

    // The tile layer is built HERE because the row holds everything the construction needs: the source's own
    // schema, template, band, and credit, and the policy's cache, agent, and residency bounds. The layer
    // seeds its own `Attribution` hyperlink from the source's credit, so the map's attribution surface is
    // populated by mounting the tile rather than by a caller remembering to add a widget beside it.
    public Fin<ILayer> Layer() => Switch(
        tile: static row => Try.lift(() => (ILayer)new TileLayer(
                row.Source.Source(row.Policy), minTiles: row.Policy.MinTiles, maxTiles: row.Policy.MaxTiles) { Name = row.Key })
            .Run()
            .MapFail(error => (Error)new ChartFault.LayerRejected($"{row.Key}: {error.Message}")),
        overlay: static row => GeoOverlay.Layer(row),
        vectors: static row => GeoOverlay.Layer(row),
        widget: static row => Fin.Fail<ILayer>(new ChartFault.LayerRejected($"{row.Row.Key}: chrome mounts on Widgets")));

    // Default rows so a surface is callable from settled vocabulary alone: the light and dark tile pairs
    // under one policy, and the three chrome rows every map carries.
    public static BasemapLayerRow Basemap(ThemeVariantRow variant, TilePolicy policy) {
        BasemapSource source = BasemapSource.For(variant);
        return new Tile(source.Key, source, policy);
    }

    public static readonly BasemapLayerRow ScaleBar = new Widget(WidgetRow.ScaleBar);
    public static readonly BasemapLayerRow ZoomButtons = new Widget(WidgetRow.ZoomButtons);
    public static readonly BasemapLayerRow InfoBox = new Widget(WidgetRow.InfoBox);
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
    // Attribution admission runs BEFORE anything is staged, because a licence refusal is a property of the
    // declaration rather than of the mount and refusing after opening HTTP clients is work done to reach a
    // verdict the row set already carried.
    public Fin<Map> Build(Seq<BasemapLayerRow> rows, ChartInk ink, ResolvedTheme theme) =>
        Credited(rows).Bind(_ => {
            Map candidate = new();
            return rows.Fold(Fin.Succ(candidate), (rail, row) => rail.Bind(map => Mount(map, row))).Match(
                Succ: map => Background(map, theme).Match(
                    Succ: seated => {
                        Map previous = Control.Map;
                        Control.Map = seated;
                        Retint(seated, ink);
                        Control.RefreshGraphics();
                        previous.Dispose();
                        return Fin.Succ(seated);
                    },
                    Fail: error => { map.Dispose(); return Fin.Fail<Map>(error); }),
                Fail: error => {
                    candidate.Dispose();
                    return Fin.Fail<Map>(error);
                });
        });

    // The licence gate. Every tile row draws another party's imagery, so its source must carry the credit
    // that imagery's terms require; a row whose credit is blank refuses by name here rather than shipping a
    // map that violates the terms it renders under.
    static Fin<Unit> Credited(Seq<BasemapLayerRow> rows) =>
        rows.Choose(static row => row is BasemapLayerRow.Tile tile ? Some(tile) : None)
            .Traverse(static tile => string.IsNullOrWhiteSpace(tile.Source.Credit.Text)
                ? Fin.Fail<Unit>(new ChartFault.LayerRejected($"{tile.Key}: tile source declares no attribution"))
                : Fin.Succ(unit))
            .As()
            .Map(static _ => unit);

    // The backdrop is the resolved panel token rather than the package default white, so the gap around a
    // partially fetched world reads as the product's own surface under either variant instead of flashing a
    // light plate through a dark board.
    static Fin<Map> Background(Map map, ResolvedTheme theme) =>
        theme.Paint(PaintRole.Surface, rung: 0)
            .ToFin((Error)new ChartFault.PaintUnresolved($"{PaintRole.Surface.Key}+0"))
            .Map(token => { map.BackColor = Mapsui.Styles.Color.FromArgb(token.A, token.R, token.G, token.B); return map; });

    // The chrome swap. It walks `GetWidgetsOfMapAndLayers()` rather than `Map.Widgets`, so every tile
    // layer's own attribution credit re-tints beside the mounted chrome and a dark variant does not leave
    // the one legally required caption unreadable; the row lookup is by widget TYPE because the layer mints
    // its credit widget itself and no row ever held it.
    public Fin<Unit> Retint(ChartInk ink, ResolvedTheme theme) =>
        Background(Control.Map, theme).Map(map => { Retint(map, ink); Control.RefreshGraphics(); return unit; });

    static void Retint(Map map, ChartInk ink) =>
        toSeq(map.GetWidgetsOfMapAndLayers()).Iter(widget =>
            toSeq(WidgetRow.Items).Find(row => row.Shape == widget.GetType())
                .Match(Some: row => row.Retint(widget, ink), None: () => WidgetRow.Text(widget, ink)));

    // Every camera move admits its own argument before it dispatches, and answers the SAME `Fin` rail every
    // sibling entry on this page answers. A bare `IO<Unit>` swallowed a non-finite resolution, an
    // out-of-band zoom level, and a degenerate box into a silent no-op the caller could neither observe nor
    // compose with `Build`; the capture converts a navigator throw into the page's typed refusal on that
    // same rail, so one `Bind` chain carries a rebuild and the camera move that follows it.
    public IO<Fin<Unit>> Navigate(MapNav verb) =>
        IO.lift(() => Admit(verb).Bind(admitted => Try.lift(() => ignore(admitted.Switch(
                state: Control.Map.Navigator,
                centerOn: static (nav, v) => fun(() => nav.CenterOn(v.Center))(),
                zoomTo: static (nav, v) => fun(() => nav.ZoomTo(v.Resolution))(),
                zoomToLevel: static (nav, v) => fun(() => nav.ZoomToLevel(v.Level))(),
                zoomToBox: static (nav, v) => fun(() => nav.ZoomToBox(v.Box))(),
                centerAndZoom: static (nav, v) => fun(() => nav.CenterOnAndZoomTo(v.Center, v.Resolution))(),
                flyTo: static (nav, v) => fun(() => nav.FlyTo(v.Center, v.Resolution, v.Flight.DurationMs))(),
                rotateTo: static (nav, v) => fun(() => nav.RotateTo(v.Degrees))())))
            .Run()
            .MapFail(error => (Error)new ChartFault.LayerRejected($"navigate/{admitted.Key}: {error.Message}"))));

    // One admission over the verb union, total by construction: a resolution must be finite and positive
    // because zero divides the viewport scale, a level must sit inside the mounted source's own band, a box
    // must have extent, and a bearing must be finite.
    static Fin<MapNav> Admit(MapNav verb) => verb.Switch(
        state: verb,
        centerOn: static (row, v) => Finite(row, v.Center),
        zoomTo: static (row, v) => Positive(row, v.Resolution),
        zoomToLevel: static (row, v) => v.Level >= 0 ? Fin.Succ(row) : Refused(row),
        zoomToBox: static (row, v) => v.Box.Width > 0d && v.Box.Height > 0d ? Fin.Succ(row) : Refused(row),
        centerAndZoom: static (row, v) => Finite(row, v.Center).Bind(_ => Positive(row, v.Resolution)),
        flyTo: static (row, v) => Finite(row, v.Center).Bind(_ => Positive(row, v.Resolution)),
        rotateTo: static (row, v) => double.IsFinite(v.Degrees) ? Fin.Succ(row) : Refused(row));

    static Fin<MapNav> Finite(MapNav row, MPoint center) =>
        double.IsFinite(center.X) && double.IsFinite(center.Y) ? Fin.Succ(row) : Refused(row);

    static Fin<MapNav> Positive(MapNav row, double resolution) =>
        double.IsFinite(resolution) && resolution > 0d ? Fin.Succ(row) : Refused(row);

    static Fin<MapNav> Refused(MapNav row) =>
        Fin.Fail<MapNav>(new ChartFault.VisualDegenerate($"navigate/{row.Key}: argument is out of range"));

    // Generated total Switch over the closed family — a new BasemapLayerRow case breaks THIS dispatch at
    // compile time; the runtime-silent `_` arm over the closed family is the deleted form. Every arm lands
    // on the SAME Fin the layer construction already returned, so the fold's Fail arm reaches every failure.
    static Fin<Map> Mount(Map map, BasemapLayerRow row) => row.Switch(
        state: map,
        tile: static (m, t) => t.Layer().Map(layer => { m.Layers.Add(layer); return m; }),
        overlay: static (m, o) => GeoOverlay.Layer(o).Map(layer => { m.Layers.Add(layer); return m; }),
        vectors: static (m, v) => GeoOverlay.Layer(v).Map(layer => { m.Layers.Add(layer); return m; }),
        widget: static (m, w) => Staged(m, w.Row.Key, () => m.Widgets.Add(w.Row.Mint(m))));

    // A widget mint is an unconstrained composition delegate that does real work, so an escaping throw would
    // carry the staged candidate PAST the `Fail` arm that disposes it, leaking every layer already mounted
    // on it and falsifying the dispose-on-any-row-failure law at the one place it is load-bearing. The
    // capture is the row's own typed refusal, so a bad mint names itself instead of surfacing raw.
    static Fin<Map> Staged(Map map, string key, Action mount) =>
        Try.lift(() => { mount(); return map; }).Run()
            .MapFail(error => (Error)new ChartFault.LayerRejected($"{key}: {error.Message}"));

    // Tile health is a SUBSCRIPTION over signals the package already publishes: `Busy` transitions arrive on
    // each layer's `PropertyChanged` and a fetch outcome arrives on `DataChanged` carrying the exception or
    // its absence. Polling the tile cache for arrivals is the deleted form — it cannot distinguish a slow
    // fetch from a refused one, which is exactly the distinction a surface must render.
    public IDisposable Watch(Action<TileHealth> onHealth) {
        Seq<ILayer> layers = toSeq(Control.Map.Layers).Filter(static layer => layer is TileLayer).Strict();
        Seq<IDisposable> handles = layers.Map(layer => {
            void OnData(object? _, DataChangedEventArgs args) => onHealth(TileHealth.Of(layer, Optional(args.Error)));
            void OnBusy(object? _, PropertyChangedEventArgs args) {
                if (StringComparer.Ordinal.Equals(args.PropertyName, nameof(ILayer.Busy))) { onHealth(TileHealth.Of(layer, None)); }
            }
            layer.DataChanged += OnData;
            layer.PropertyChanged += OnBusy;
            return Disposable.Create(() => {
                layer.DataChanged -= OnData;
                layer.PropertyChanged -= OnBusy;
            });
        }).Strict();
        onHealth(layers.IsEmpty ? new TileHealth.Idle() : new TileHealth.Fetching(layers.Count));
        return Disposable.Create(() => handles.Iter(static handle => handle.Dispose()));
    }

    public const string LayersInstrument = "rasm.appui.basemap.layers";
    public const string NavigatedInstrument = "rasm.appui.basemap.navigated";
    public const string TileHealthInstrument = "rasm.appui.basemap.tile.health";

    // Every projection binds at the fold that already holds the fact — the successful swap inside `Build`,
    // the dispatched verb inside `Navigate`, the classified transition inside `Watch` — and each breakdown
    // rides a DECLARED dimension materialized through the one tag entry: neither a `KeyValuePair` nor a
    // pre-built tag list converts to the pair element the write consumes, so a fact spelled either way
    // reaches no write at all, and an instrument claiming a breakdown its spec never declares yields one
    // undifferentiated series no board can split. Both `Key` projections are total `Switch`es, so a new
    // `MapNav` or `TileHealth` case breaks them at compile time rather than widening the metric's key space.
    public static Fin<Unit> Observe(InstrumentSet set) => set.Write(LayersInstrument, 1L);

    public static Fin<Unit> Observe(InstrumentSet set, MapNav verb) =>
        set.Write(NavigatedInstrument, 1L, InstrumentSet.Tags((AppUiTelemetry.IntentSlot, verb.Key)));

    public static Fin<Unit> Observe(InstrumentSet set, TileHealth health) =>
        set.Write(TileHealthInstrument, 1L, InstrumentSet.Tags((AppUiTelemetry.OutcomeSlot, health.Key)));

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            InstrumentSpec.Count(LayersInstrument, "{rebuild}", "layer-set rebuilds swapped onto the mounted map", MeasureForm.Whole),
            InstrumentSpec.Count(NavigatedInstrument, "{navigation}", "camera moves by verb case", MeasureForm.Whole,
                AppUiTelemetry.IntentSlot),
            InstrumentSpec.Count(TileHealthInstrument, "{transition}", "tile fetch-state transitions by state", MeasureForm.Whole,
                AppUiTelemetry.OutcomeSlot));
}
```

## [03]-[NTS_OVERLAY]

- Owner: `GeoOverlayRow` — the per-feature overlay row carrying the Bim-owned `GeoFeature` WHOLE (geometry, attribute table, declared `SourceCrs`) plus its display label and its source-layer key; `GeoOverlay` — the projection fold from Bim geospatial output to a Mapsui vector layer, and the choropleth seam projecting the theme colormap vocabulary onto the package's own gradient thematics; `GeoTileProvider` — the Bim MVT pyramid as one ordinary `IProvider`, decoding each covering tile through the seam codec into the same row shape a resident overlay carries.
- Entry: `public static Fin<ILayer> Layer(BasemapLayerRow.Overlay overlay)` — one fold; each row's consumed feature wraps as `Mapsui.Nts.GeometryFeature`, styles resolve from the row's `Symbology` selector, and the layer mounts as one provider-decorated `Layer`; `public static Fin<ILayer> Layer(BasemapLayerRow.Vectors vectors)` — the same mount over the tiled provider, so a resident set and a pyramid differ by their source alone; `public static Fin<Func<IFeature, Viewport, IStyle?>> Choropleth(Colormap map, string column, double floor, double ceiling, int steps)` — the graduated selector, the colormap's own sampled stops handed to the package's `GradientTheme` through one `ColorBlend`.
- Auto: features ARRIVE as Bim-owned `GeoFeature` rows carrying their `GeoReference` lineage (the `GeoReferenceProjector` IfcMapConversion/IfcProjectedCRS lowering) already reprojected to WGS-84 by Bim's `GeoFeature.Reproject` — the declared seam, both sides (`Rasm.Bim` Semantics/geospatial -> AppUi Charts) — so the row's `SourceCrs`/SRID state IS the CRS evidence the gate reads; AppUi's ONLY reprojection is WGS-84 lon/lat -> EPSG:3857 through `SphericalMercator.FromLonLat` under `ProjectionDefaults.Projection` at the layer-build edge.
- Receipt: an overlay row whose feature still declares a projected frame (or a non-4326 SRID) folds to `ChartFault.CrsUnresolved` — the ingress law enforced as a typed fault, never a silent draw at wrong coordinates.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, BruTile, Rasm.Bim (project), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new overlay family (site boundary, utility run, parcel, analysis heat cells) is a seq of rows on one Overlay layer row, and attribute-driven symbology within a family is one `Symbology` selector reading the consumed `GeoFeature` attribute table — never a layer per attribute value; a new tiled vector source is one `Vectors` row naming its schema and its fetch; a new graduated thematic is one `Colormap` row and one `Choropleth` call, never a per-class style roster; zero new surface.
- Boundary: an overlay mounts as `Layer` over the `IndexedMemoryProvider` → `GeometryIntersectionProvider` → `GeometrySimplifyProvider` decorator chain under one `ThemeStyle`, so viewport clipping, resolution-driven decimation, and per-feature symbology are all provider and style policy — a hand-rolled cull, a resolution branch at the bind edge, and a `MemoryLayer` holding every vertex resident at every zoom are the deleted forms; geodesy stays Bim's — `GeoFeature.Reproject`, datum transforms, and projected-CRS handling never re-implement here, and on the MAP-VIEWPORT CRS plane a local geodesy kernel (a proj4 port, a datum table, a second `SphericalMercator` beside the Mapsui primitive) is the FORBIDDEN form. `[NOT]` that plane: `custom#SKIA_KINDS` `GeoProjection` answers pixels of one `SKImageInfo` extent and carries no CRS, no datum, and no map engine, so the forbidden form here is a second CRS owner, never every mercator expression in the package. the CHOROPLETH is the package's own `GradientTheme` fed by the theme rail's colormap sampling: `Colormap.HeatMap` produces the stop table, one `ColorBlend` positions those stops, and the theme interpolates fill, line, and text along it against the feature's own numeric column — so a graduated map reads the SAME perceptual ramp a chart heat series and a custom-visual ink axis read, and a hand-built per-class `VectorStyle` roster, a locally interpolated two-colour lerp, and a basemap-local colormap table are the deleted forms. `GradientTheme` interpolates only when its two styles share one type drawn from `VectorStyle`, `ImageStyle`, or `LabelStyle` and only over a numeric column, so the selector states both bounds and refuses a non-numeric column rather than throwing inside the render pass. NTS geometry types cross the seam as values inside Bim-owned features and are wrapped, never re-modeled. The Bim MVT pyramid is a VECTOR row, never a tile row: `GeoModel.ToTiles` emits the pyramid, `GeoTiles.Catalog` serves the TileJSON the source discovers its template and zoom band through, and `GeoTiles.Decode` answers each tile's own `(layer, GeoFeature)` rows, every one crossing the SAME `Project` gate a resident row crosses — so a tiled site model and a resident parcel set are one draw path under one CRS law. `TileLayer` structurally cannot carry it: its fetch hook answers ONE `IFeature` per tile and its constructor seats a `RasterStyle`, so a tile decoding to a feature SET would collapse into a single geometry and lose every attribute the symbology selector reads, which is exactly the reading a vector tile exists to carry.

```csharp signature
// The ingress row carries the Bim-owned GeoFeature WHOLE — geometry + attribute table + declared
// SourceCrs — so CRS authority is the seam contract the feature itself carries, never an SRID sniff on
// a bare geometry; Label is the display column minted beside the consumed feature and Source is the seam's
// own layer key, which the MVT codec threads per row and the symbology selector reads to tell a road from a
// parcel. A resident row carries no source and a decoded one carries the layer it arrived in, so one row
// shape serves both ingresses and neither needs a family of its own.
public sealed record GeoOverlayRow(
    string FeatureId,
    Rasm.Bim.Semantics.GeoFeature Feature,
    Option<string> Label,
    Option<string> Source);

// The Bim MVT pyramid as an ordinary provider. `GetFeaturesAsync` resolves the tiles covering the fetch
// extent at the fetch's OWN resolution through the schema's `GetTileInfos`, so LOD selection is the schema's
// arithmetic rather than a zoom branch here, and every decoded row crosses the same `Project` gate a resident
// row crosses — one CRS law, one wrap, one attribute stamp. The fetch delegate is composition-bound because
// the transport is the caller's (an HTTP template off `GeoTiles.Catalog`, a local pyramid, a cache), and the
// decode is the seam codec's, so this owner holds neither a protobuf reader nor an HTTP client of its own.
public sealed class GeoTileProvider(
    ITileSchema schema,
    Func<TileInfo, Task<ReadOnlyMemory<byte>>> fetch) : IProvider {
    public string? CRS { get; set; } = "EPSG:3857";

    public MRect? GetExtent() => schema.Extent.ToMRect();

    // A tile that refuses contributes NOTHING rather than failing the whole fetch: a pyramid is a set of
    // independent tiles and one refused tile is a hole in the draw, while a failed fetch is a blank map — the
    // same verdict the tile rail's own health fold states for a raster tile that never arrived. The feature id
    // is the tile address plus the ordinal within it, so a pick answers WHICH tile a feature came from and two
    // adjacent tiles cannot collide on an id the seam never promised to make unique across the pyramid.
    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo info) {
        Seq<TileInfo> covering = toSeq(schema.GetTileInfos(info.Extent.ToExtent(), info.Resolution)).Strict();
        IFeature[][] tiles = await Task.WhenAll(covering.Map(async tile => {
            ReadOnlyMemory<byte> bytes = await fetch(tile);
            return Rasm.Bim.Semantics.GeoTiles
                .Decode(bytes, tile.Index.Col, tile.Index.Row, tile.Index.Level, Op.Of(name: "basemap-mvt"))
                .Map(rows => rows.Map((row, ordinal) => new GeoOverlayRow(
                    $"{tile.Index.Level}/{tile.Index.Col}/{tile.Index.Row}/{ordinal}", row.Feature, None, Some(row.Layer))))
                .Bind(static rows => rows.Traverse(GeoOverlay.Project).As())
                .Match(Succ: static features => features.Map(static feature => (IFeature)feature).ToArray(),
                    Fail: static _ => []);
        }));
        return tiles.SelectMany(static rows => rows);
    }
}

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

    // The pyramid mount. Only the DataSource differs from the resident mount — the style selector, the
    // visibility band, and the layer identity are the same columns — so a tiled site model and a resident
    // parcel set are one draw path and the residency decorators stay off a source that is already tiled by
    // construction: clipping and decimation are what the pyramid's own LOD levels already performed.
    public static Fin<ILayer> Layer(BasemapLayerRow.Vectors vectors) =>
        Try.lift(() => (ILayer)new Layer(vectors.Key) {
                DataSource = vectors.Tiles,
                Style = new ThemeStyle(vectors.Symbology),
                MinVisible = vectors.MinVisible,
                MaxVisible = vectors.MaxVisible,
            })
            .Run()
            .MapFail(error => (Error)new ChartFault.LayerRejected($"{vectors.Key}: {error.Message}"));

    // The constant selector: a uniform overlay states its one style HERE, so `Overlay` carries no second
    // style column and the themed and uniform families never fork into sibling layer builders.
    public static Func<IFeature, Viewport, IStyle?> Uniform(VectorStyle style) => (_, _) => style;

    // The choropleth seam: the theme rail's colormap is the ONE ramp authority, so its sampled stops become
    // the package's `ColorBlend` and the package's own `GradientTheme` does the interpolation against each
    // feature's numeric column. Nothing here interpolates colour — a page-local lerp between two hues would
    // be a second ramp drifting from the perceptually-uniform one the token page's class discipline proves,
    // and a per-class style roster would fork one thematic into a layer per bucket. `Colormap.HeatMap` is
    // the same projection the chart series ramp and the custom-visual ink axis read, so a graduated map, a
    // heat series, and a diagram's ink band are one generation at three surfaces.
    public static Fin<Func<IFeature, Viewport, IStyle?>> Choropleth(
        Colormap map, string column, double floor, double ceiling, int steps) =>
        string.IsNullOrWhiteSpace(column) || steps < 2 || !double.IsFinite(floor) || !double.IsFinite(ceiling) || ceiling <= floor
            ? Fin.Fail<Func<IFeature, Viewport, IStyle?>>(new ChartFault.VisualDegenerate(
                $"choropleth: {column} needs an ascending finite range and at least two stops"))
            : map.HeatMap(steps, static token => Mapsui.Styles.Color.FromArgb(token.A, token.R, token.G, token.B))
                .Map(stops => {
                    // Positions are the stop ordinals normalized onto the unit interval, which is exactly the
                    // domain the blend samples, so the ramp the colormap generated survives unresampled.
                    ColorBlend blend = new(stops, Enumerable.Range(0, stops.Length)
                        .Select(index => index / (double)(stops.Length - 1)).ToArray());
                    GradientTheme theme = new(column, floor, ceiling,
                        new VectorStyle { Fill = new Brush(stops[0]), Outline = new Pen(stops[0], 1d) },
                        new VectorStyle { Fill = new Brush(stops[^1]), Outline = new Pen(stops[^1], 1d) }) {
                        FillColorBlend = blend,
                        LineColorBlend = blend,
                    };
                    return (Func<IFeature, Viewport, IStyle?>)((feature, viewport) => theme.GetStyle(feature, viewport));
                });

    // The ingress gate: a feature is admissible only when its OWN declared frame is the WGS-84 baseline
    // the seam promises (post-Reproject: no residual projected frame, SRID 4326 exactly — an unstamped
    // SRID 0 is un-reprojected input and faults) — a residual SourceCrs is the typed CrsUnresolved fault,
    // never a silent draw at wrong coordinates; the transformed copy re-stamps SRID 3857 so downstream
    // reads the web-mercator frame it actually holds, never the source frame.
    internal static Fin<GeometryFeature> Project(GeoOverlayRow row) =>
        row.Feature.SourceCrs.IsNone && row.Feature.Geometry.SRID is 4326
            ? Fin.Succ(fun(() => {
                NetTopologySuite.Geometries.Geometry mercator = row.Feature.Geometry.Copy();
                mercator.Apply(MercatorFilter.Forward);
                mercator.SRID = 3857;
                GeometryFeature feature = new(mercator);
                feature["id"] = row.FeatureId;
                row.Label.Iter(label => feature["label"] = label);
                row.Source.Iter(source => feature["source"] = source);
                return feature;
            })())
            : Fin.Fail<GeometryFeature>(new ChartFault.CrsUnresolved(row.FeatureId, row.Feature.Geometry.SRID));
}

// ONE parameterized coordinate-sequence filter over the Mapsui projection primitive — Forward lifts
// WGS-84 into EPSG:3857 at layer build, Inverse returns authored view geometry to WGS-84 at commit;
// a direction-named sibling filter class is the deleted form.
public sealed class MercatorFilter(Func<double, double, (double X, double Y)> project) : NetTopologySuite.Geometries.ICoordinateSequenceFilter {
    public static readonly MercatorFilter Forward = new(static (x, y) => SphericalMercator.FromLonLat(x, y));
    public static readonly MercatorFilter Inverse = new(static (x, y) =>
        SphericalMercator.ToLonLat(x, y) switch { var (lon, lat) => (Principal(lon), lat) });

    // The latitude the web-mercator projection clamps at. Beyond it the projection is asymptotic, so a world
    // ordinate outside the clamp answers a coordinate that never had ground — the band a pan past the top of
    // the world exposes — and the admission gate refuses it rather than committing it.
    internal const double PoleClampDeg = 85.05112878d;

    // Longitude folds onto its principal range because web mercator is CYLINDRICAL: a map panned past the
    // antimeridian keeps producing world x beyond the seam, so an unwrapped inverse commits a longitude of
    // two hundred degrees that no WGS-84 consumer admits and that re-renders a world away from the place the
    // operator drew on. The fold rides the INVERSE ROW itself, so every crossing carries it and no commit leg
    // re-spells the wrap.
    static double Principal(double lon) => lon - (360d * Math.Floor((lon + 180d) / 360d));

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

- Owner: `MapPick` — the hit-test fold; `PickPolicy` — the tolerance-and-arity posture a pick resolves under; `BasemapPickReceipt` with `BasemapHover` — the page-owned scalar results; the snapshot lane is a `CaptureRow` sibling riding the capture encode fold.
- Entry: `public static Fin<Seq<BasemapPickReceipt>> Pick(MapControl control, ScreenPosition screen, PickPolicy policy)` — `GetMapInfo(screen, layers)` resolves the hit ROSTER at the screen point, tolerance widens the probe to the finger-and-pointer band, and every hit projects its `id` attribute into the Shell pick state in nearest-first order; `public static Option<BasemapHover> Hover(MapControl control, ScreenPosition screen, PickPolicy policy)` — the same fold at hover grain, answering the top hit and the ambiguity count; `public static IO<RenderReceipt> Snapshot(VisualRuntime runtime, MapControl control, string key)` — the encoded basemap bytes re-sealed through the one capture codec.
- Auto: a pick lands in the same selection vocabulary the viewport pick uses — the Shell selection owner receives scalar receipts and never a Mapsui type. TOLERANCE is a screen-space band the policy declares and the fold resolves into world units through the hit's own `MapInfo.Resolution`, because a pointer lands within a few pixels of a hairline utility run rather than on it and a zero-tolerance probe makes thin geometry unselectable at every zoom. MULTI-HIT is the `MapInfo.MapInfoRecords` roster rather than the convenience `Feature` alone, ordered nearest-first by the geometry's own NTS `Distance` to the pick's world position, so a click where a parcel, a boundary, and a utility run overlap answers all three in a defensible order and the caller disambiguates instead of receiving whichever the render pass drew last; the policy's `Arity` bounds the roster so a dense stack cannot answer hundreds. HOVER is the same fold at a lighter grain — the top hit plus how many others share the point — so a surface can state the ambiguity before the click that resolves it. `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` yields the encoded basemap bytes that decode through `VisualCodec.Decode` and re-encode through `VisualCodec.Encode` as kind basemap, so a basemap baseline rides the same render-hash proof lanes as every visual.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, NetTopologySuite, SkiaSharp, LanguageExt.Core
- Growth: a new pick projection is one attribute read on the fold; a retuned tolerance or arity is one `PickPolicy` value; zero new surface.
- Boundary: no Mapsui type crosses out of this page — picks project to scalar receipts and snapshots cross as encoded bytes through the capture codec; dashboard geo adjacency (a dashboard tile embedding a basemap) mounts THIS surface as a tile body, never a second map engine. Distance ranking reads the ADMITTED geometry engine's own `Geometry.Distance` against the pick point rather than a centroid or envelope proxy, because a long utility run's centroid can sit kilometres from the vertex under the pointer and an envelope test makes every large parcel win every contest; a feature carrying no NTS geometry ranks last rather than being dropped, since the renderer hit it and a pick that discards a hit the render produced is a disagreement between two truths. Tolerance is declared in SCREEN pixels and converted at the fold — a tolerance carried in world units would tighten as the viewer zoomed in, which is the opposite of what a pointer needs.

```csharp signature
// The pick posture. `TolerancePx` is the screen band a probe widens to, `Arity` bounds the roster a dense
// stack answers, and both are policy DATA so a touch surface and a mouse surface differ by value rather than
// by a second fold. The default band is the same order the editing session's own vertex radius uses, so a
// pick and a vertex grab agree about what "near" means on one plane.
public readonly record struct PickPolicy(double TolerancePx, int Arity) {
    public static readonly PickPolicy Pointer = new(TolerancePx: 8d, Arity: 8);
    public static readonly PickPolicy Touch = new(TolerancePx: 16d, Arity: 8);

    public Fin<PickPolicy> Admit() =>
        double.IsFinite(TolerancePx) && TolerancePx >= 0d && Arity > 0
            ? Fin.Succ(this)
            : Fin.Fail<PickPolicy>(new ChartFault.VisualDegenerate("pick: tolerance must be finite and non-negative, arity positive"));
}

// The page-owned pick receipt — scalars only; a Mapsui MPoint or MapInfo never crosses out of this page.
// `Distance` is the world-unit separation between the pick point and the feature, carried because a caller
// disambiguating a stack needs the ranking evidence rather than only the order it happened to arrive in; it
// is ABSENT for a feature the renderer hit that carries no measurable geometry, which is a different fact
// from a distance of zero and reads as one.
public readonly record struct BasemapPickReceipt(string FeatureId, string Layer, double WorldX, double WorldY, Option<double> Distance);

// Hover answers the top hit and how many hits share the point, so a surface can state "3 features here"
// before the click; a hover that answered only the top hit made an ambiguous stack indistinguishable from
// an unambiguous one and every disambiguation a surprise.
public readonly record struct BasemapHover(BasemapPickReceipt Top, int Ambiguity);

public static class MapPick {
    // The pick fold: probe, admit, rank, bound. `GetMapInfo` answers the whole `MapInfoRecords` roster and
    // the convenience `Feature` is only its head, so reading that head alone discarded every feature under
    // the top one — which on an overlay plane is the normal case, not the exception. Tolerance converts
    // through the hit's OWN `MapInfo.Resolution` (world units per pixel at the moment of the probe), so one
    // declared pixel band means the same thing at every zoom.
    public static Fin<Seq<BasemapPickReceipt>> Pick(MapControl control, ScreenPosition screen, PickPolicy policy) =>
        policy.Admit().Bind(admitted => Optional(control.GetMapInfo(screen, control.Map.Layers))
            .Bind(info => Optional(info.WorldPosition).Map(world => (Info: info, World: world)))
            .Match(
                Some: hit => Fin.Succ(Ranked(hit.Info, hit.World, admitted)),
                None: static () => Fin.Succ(Seq<BasemapPickReceipt>())));

    public static Option<BasemapHover> Hover(MapControl control, ScreenPosition screen, PickPolicy policy) =>
        Pick(control, screen, policy).ToOption()
            .Bind(hits => hits.Head.Map(top => new BasemapHover(top, hits.Count - 1)));

    // Ranking is nearest-first by the ADMITTED geometry engine's own distance from the pick point, bounded
    // by the policy's arity. A feature the renderer hit but whose payload carries no NTS geometry ranks last
    // rather than dropping out, because the render and the pick disagreeing about what exists at a point is
    // a worse failure than an unranked entry at the tail.
    static Seq<BasemapPickReceipt> Ranked(MapInfo info, MPoint world, PickPolicy policy) {
        NetTopologySuite.Geometries.Point at = new(world.X, world.Y);
        double tolerance = policy.TolerancePx * info.Resolution;
        // The ranking and the arity bound both leave the carrier, so the bounded run re-enters through
        // `toSeq` before the receipt projection reads it — the carrier's own projection reaches no ordered
        // enumerable, and this fold is where that would land as a pick that answers nothing at all.
        return toSeq(toSeq(info.MapInfoRecords)
                .Choose(record => Optional(record.Feature)
                    .Bind(feature => Optional(feature["id"] as string)
                        .Map(id => (Id: id, Layer: record.Layer?.Name ?? string.Empty, Distance: Separation(feature, at)))))
                // An absent distance PASSES the band: the renderer hit that feature, and a pick discarding a
                // hit the render produced makes the two surfaces disagree about what exists at a point.
                .Filter(row => row.Distance.ForAll(distance => distance <= tolerance))
                .OrderBy(static row => row.Distance.IfNone(double.MaxValue))
                .Take(policy.Arity))
            .Map(row => new BasemapPickReceipt(row.Id, row.Layer, world.X, world.Y, row.Distance))
            .Strict();
    }

    static Option<double> Separation(IFeature feature, NetTopologySuite.Geometries.Point at) =>
        feature is GeometryFeature { Geometry: { } geometry } ? Some(geometry.Distance(at)) : None;

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

- Owner: `RedlineVerb` `[Union]` — the closed markup-verb vocabulary; `Collab/issues#REDLINE_TOOLS` `RedlineTool`, `RedlineToolState`, `RedlineStroke`, and `StrokeCapture` arrive settled as the tool and stroke owners; `RedlineKind` — the mark vocabulary carrying its own `EditMode` session column; `RedlineOrigin` `[Union]` — the two authoring ingresses one commit reads; `RedlineSurface` — the one `EditManager` authoring owner binding the composition-supplied `EditHistory` recorder; the commit leg projects onto `EditIntent.Annotation` and records one `RevertibleOp` on the same pass, never a basemap-local op union and never a second undo stack.
- Cases: `RedlineVerb` = BeginMark · Modify · Delete · Commit · Discard; `RedlineKind` = Point · Path · Area; `RedlineOrigin` = Session · Stroke, the stroke arm carrying the captured `Viewport` its samples were taken under; `RedlineDelta` = Upsert · Delete; `RedlineShape` = Point · Path · Area · Collection. `BeginMark` carries the kind, commit carries document, target, and the origin its shape and its paint policy both resolve from, and delete carries the durable target identity.
- Entry: `public WritableLayer Mounted()` — the marks layer resolved once under the shared symbology selector; `public IO<Fin<Option<(EditIntent Intent, EditReceipt Receipt)>>> Drive(RedlineVerb verb, ClockPolicy clocks, CorrelationId correlation)` — every markup gesture discriminates on the verb union, and the two durable verbs record onto the recorder before answering; `Commit` emits an upsert annotation and `Delete` emits a delete annotation, each beside the `EditReceipt` its recorder sealed, while local begin, modify, and discard return `None`; the caller composes one rail and the intent ledger commit stays caller-side (`IntentLedger.Commit` is `Collab/sync.md`'s one transaction rail).
- Auto: authoring runs on a dedicated redline `WritableLayer` above the overlay stack, mounted through `Mounted` under one `ThemeStyle(Symbology)` so a mark colours by its own `IssueStatus` attribute on the same symbology axis every overlay binds. `RedlineSurface.Apply` is the total session adapter: `BeginMark` writes the kind's own `EditMode` column, `Modify` writes `EditMode.Modify`, a session `Commit` calls `EndEdit()` and reads back the sealed feature while a stroke `Commit` snapshots and closes without it, and `Delete` and `Discard` both `ResetManipulations()` and land `EditMode.None`, `Discard` additionally dropping the in-flight feature through `Layer.TryRemove`. `Sealed` admits document and target identities, copies geometry, applies `MercatorFilter.Inverse`, stamps `SRID` `4326`, and preserves finite points, paths, polygon shells and holes, multi-geometries, and heterogeneous collections before serializing `RedlineDelta.Upsert`; `Delete` needs no surviving feature and serializes `RedlineDelta.Delete`. COMMIT READS AN ORIGIN, not a session alone: `RedlineOrigin.Session` seals the vertex-authored feature the manager wrote and carries the caller's declared paint, while `RedlineOrigin.Stroke` carries the `Viewport` the samples were TAKEN under and unprojects the captured pen stroke through that frame's own `ScreenToWorldXY` into the view frame the inverse filter then lifts to WGS-84, and resolves its paint from the stroke's own tool row and mean nib weight — so one commit verb, one durable leg, and one recorder hop serve both ingresses, and an eraser-channel stroke lands `RedlineDelta.Delete` regardless of the selected tool exactly as its capture already decided. Degenerate geometry, a stroke short of two projectable samples, a vertex outside the WGS-84 domain, a removal over a target the layer never held, and an out-of-range width, opacity, or dash interval all fail on the typed rail; paint and fill are `PaintRole` rows, so an unresolvable pigment is unspellable rather than admitted. THE RECORDER HOP: `Sealing` snapshots the target's PRIOR payload off the marks layer BEFORE `EndEdit()` seals the stroke, so a commit carries both the payload it replaced and the payload it wrote and lands as one `RevertDelta.Set(Before, After)`; a delete carries the prior payload with a JSON null after it, and a first authoring of a target carries a JSON null before it — a `null` value kind is DEFINED, so both admit on the recorder's own rail while an unsnapshotted commit would be undefined and refuse. The op records through `EditHistory.Record` with the same apply fold the layer re-renders through, so an undo re-applies the inverse payload onto `Marks` exactly as a committed intent does and no second application law exists.
- Receipt: a committed redline is one `EditIntent.Annotation(DocKey, TargetId, Payload)` row on the single edit-intent union BESIDE one `EditReceipt` the recorder seals — durable truth rides the Persistence `OpLogEntry` projection per the `[04]-[BOUNDARIES]` Loro-byte clause, and the redline layer re-renders from the committed intent, never from retained authoring state; every gesture folds one `redline.gesture` observation at `Drive` under the outcome slot its spec declares, so commit, discard, and local authoring stay separable series.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, bodong.PropertyModels, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new mark kind is one `RedlineKind` row carrying its `EditMode` column plus its corresponding `RedlineShape` case when the payload arity differs — and the shape case carries its `[JsonDerivedType]` row by the same edit; a new markup verb is one `RedlineVerb` case; a new review-state colouring is one branch inside the bound `Symbology` selector, never a second marks layer; zero new surface.
- Boundary: `EditManager` and `EditingWidget` remain inside this section; authored geometry leaves only as the WGS-84 `RedlineDelta` payload, and delete carries no stale geometry. The marks layer is a `WritableLayer` because that is the type `EditManager.Layer` admits and the one layer shape carrying in-place `Add`/`TryRemove` — the overlay provider chain decorates a FETCH and structurally cannot receive an authoring write, so the residency decorators stop at the overlay boundary while the symbology selector crosses it; a decorated edit layer, a `MemoryLayer` under an `EditManager`, and a flat per-layer redline style are all deleted forms. Pointer routing is `EditingWidget` on `Map.Widgets` over THIS manager — it forwards press, move, release, and tap into `EditManipulation`, which drives `AddVertex(Coordinate)`, `StartDragging(MapInfo, double)`/`Dragging(Point?)`/`StopDragging()`, `TryInsertCoordinate(MapInfo)`/`TryDeleteCoordinate(MapInfo, double)`, and the rotate and scale trios — so `RedlineVerb` owns SESSION lifecycle alone and a page-local pointer router beside the widget is the deleted form. The payload crosses `System.Text.Json` under the one composition-bound wire options and both payload unions carry their `[JsonDerivedType]` roster: `[Union]` generates no JSON support, so a union serialized as its abstract base emits an empty object, and an annotation blob that round-trips is a discriminated one. UNDO IS THE ONE REVERT ALGEBRA: a committed mark records onto the `Editing/history` `EditHistory` recorder as a `RevertibleOp` whose delta is the before-and-after annotation payload, so a redline undoes through the same `history.undo` intent every other edit does and a basemap-local mark stack, a re-authoring replay, and a geometry-level inverse are the deleted forms — an annotation that committed without recording was durable and unreachable by undo, which is the one shape a review surface must never have. The recorder's apply fold is the SAME layer write the committed-intent re-render performs, so the two paths cannot drift. The TOOL is not this page's: `Collab/issues#REDLINE_TOOLS` owns `RedlineToolState` — the active tool, its weight, its tint, and the review posture — and `StrokeCapture` owns the pressure-weighted fold from raw samples to a stroke, so the mode toolbar over a map and the mode toolbar over a viewpoint drive ONE tool state and a basemap-local pen vocabulary is the deleted form; this section owns the session lifecycle, the layer, and the commit leg alone. PEN input arrives as `Shell/input#POINTER_GESTURES` `PenSample` rows off the one pointer ingress — the whole coalesced burst, because the platform batches every sample it took between two frames and a nib's pressure and tilt live in the ones a per-frame read discards — so `StrokeCapture.Capture` sees the same reading here it sees on the viewpoint plane, `PenAxis.Pressure` has already scaled each `StrokePoint`'s weight and `PenAxis.Eraser` has already routed the stroke before it reaches this page, and a mouse-driven mark carries the tool row's own default weight rather than the constant pressure a mouse reports. The stroke's own ingress is `RedlineOrigin.Stroke` and its projection is THIS page's, exactly as that owner's boundary states: the screen samples cross `ScreenToWorldXY` against the CAPTURED frame into the EPSG:3857 view frame and the one `MercatorFilter.Inverse` lifts them to WGS-84, so the inverse filter and the SRID stamp stay the single crossing they already are and a capture-side unprojection is the deleted form. THE FRAME IS THE GESTURE'S, never the mounted camera: a stroke is screen coordinates and its commit runs after the gesture ends, so reading the live viewport lands every mark displaced by exactly the pan, zoom, or rotation that happened in between — a displacement neither leg reports, because both succeed — and the viewport is a six-field value, so carrying it makes the stale-frame commit unrepresentable rather than guarded. The inverse row FOLDS longitude onto its principal range because web mercator is cylindrical and a pan past the antimeridian keeps producing world ordinates beyond the seam, while a latitude outside the mercator clamp is a coordinate the projection is asymptotic at and refuses at the shape gate — a wrap and a refusal rather than one rule for both, because the seam names the same place and the clamp names none. A REMOVAL over a target the layer never held refuses at the one serialize both durable verbs cross: the layer re-renders from committed intent, so an absent target was never committed, and recording that removal seals a delta empty on both halves — a durable no-op the ledger keeps, the fan counts as an applied edit, and an undo restores nothing from. A redline over the 3D viewport remains the BCF markup charter; this section owns only the 2D geographic plane.

```csharp signature
// Both payload unions carry the `[JsonDerivedType]` roster because the annotation payload IS a wire and
// `[Union]` generates NO JSON support: the generator stamps a converter onto keyed owners alone, so a
// union reaches `System.Text.Json` as a bare abstract record and serializes the DECLARED type — an empty
// object per case, a total data loss no decode fails on. The kind literals are the durable discriminator,
// so a case renamed later keeps its literal and a case added later carries its row in the same edit the
// Growth line names.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(RedlineShape.Point), "point")]
[JsonDerivedType(typeof(RedlineShape.Path), "path")]
[JsonDerivedType(typeof(RedlineShape.Area), "area")]
[JsonDerivedType(typeof(RedlineShape.Collection), "collection")]
public abstract partial record RedlineShape {
    private RedlineShape() { }
    public sealed record Point(double Lon, double Lat) : RedlineShape;
    public sealed record Path(Seq<(double Lon, double Lat)> Vertices) : RedlineShape;
    public sealed record Area(Seq<(double Lon, double Lat)> Shell, Seq<Seq<(double Lon, double Lat)>> Holes) : RedlineShape;
    public sealed record Collection(Seq<RedlineShape> Members) : RedlineShape;
}

// Paint is the TOKEN ROW, never its key spelled as text: `PaintRole` is a keyed owner the Thinktecture
// generator stamps its own converter onto, so the durable blob still carries the key scalar while a role that
// does not exist is unspellable here rather than a string that resolves to nothing at re-render. That is also
// what makes the two ingresses one policy — `RedlineTool.Ink()` answers exactly this type, so a pen stroke's
// paint is the tool row's own and a vertex-authored mark's is the caller's, with no key transcription between.
public sealed record RedlineStyle(PaintRole Paint, Option<PaintRole> Fill, double Width, double Opacity, Seq<double> Dash) {
    // The stroke's own policy: the tool row's ink, the mean resolved nib weight so a pressure-tapered stroke
    // commits at the weight it was drawn at rather than at the tool's declared base, and no fill because a pen
    // path encloses nothing. A per-point width would need a variable-width mark the symbology axis cannot
    // express, so the fold states the one weight the mark carries.
    public static RedlineStyle Of(RedlineStroke stroke, double opacity, Seq<double> dash) =>
        new(stroke.Ink, None,
            stroke.Points.IsEmpty ? stroke.Tool.Weight : stroke.Points.Sum(static point => point.Weight) / stroke.Points.Count,
            opacity, dash);

    public Fin<RedlineStyle> Admit() =>
        double.IsFinite(Width) && Width > 0d
        && double.IsFinite(Opacity) && Opacity is >= 0d and <= 1d
        && Dash.ForAll(static interval => double.IsFinite(interval) && interval > 0d)
            ? Fin.Succ(this)
            : Fin.Fail<RedlineStyle>(new ChartFault.VisualDegenerate("redline: width, opacity, or dash is invalid"));
}

// The two authoring ingresses one commit reads. A vertex session and a pen stroke differ in where the geometry
// COMES FROM and in nothing the durable leg does — same admission, same inverse filter, same SRID stamp, same
// recorder hop — so a second commit verb beside this one would be a second copy of that leg maintained apart.
// The stroke arm carries the settled `Collab/issues#REDLINE_TOOLS` capture whole, so its tool, its ink, its
// per-point weights, and its erase routing all arrive already decided by the owner that decided them.
// `Frame` is the camera the samples were TAKEN under, carried because a stroke's coordinates are screen
// coordinates and a commit runs after the gesture ends: unprojecting them through whatever camera is mounted
// at commit time lands the mark displaced by exactly whatever pan, zoom, or rotation happened in between, and
// the displacement is invisible because both the capture and the commit succeed. The viewport is a six-field
// value, so carrying it costs nothing and makes the stale-frame commit unrepresentable rather than guarded.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineOrigin {
    private RedlineOrigin() { }
    public sealed record Session(RedlineStyle Style) : RedlineOrigin;
    public sealed record Stroke(RedlineStroke Captured, Viewport Frame, double Opacity, Seq<double> Dash) : RedlineOrigin;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(RedlineDelta.Upsert), "upsert")]
[JsonDerivedType(typeof(RedlineDelta.Delete), "delete")]
public abstract partial record RedlineDelta {
    private RedlineDelta() { }
    public sealed record Upsert(RedlineMark Mark) : RedlineDelta;
    public sealed record Delete : RedlineDelta;
}

public sealed record RedlineMark(RedlineShape Shape, RedlineStyle Style);

// The kind carries its own `EditMode` column, so `BeginMark` is one write rather than a verb-to-mode
// ladder at the adapter: a fourth mark kind names the session mode it opens on the row that declares it,
// and the nine-member `EditMode` vocabulary stays the package's, never mirrored as a second enum.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RedlineKind {
    public static readonly RedlineKind Point = new("point", EditMode.AddPoint);
    public static readonly RedlineKind Path = new("path", EditMode.AddLine);
    public static readonly RedlineKind Area = new("area", EditMode.AddPolygon);

    public EditMode Mode { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineVerb {
    private RedlineVerb() { }
    public sealed record BeginMark(RedlineKind Kind) : RedlineVerb;
    public sealed record Modify : RedlineVerb;
    public sealed record Delete(string DocKey, string TargetId) : RedlineVerb;
    public sealed record Commit(string DocKey, string TargetId, RedlineOrigin Origin) : RedlineVerb;
    public sealed record Discard : RedlineVerb;
}

// Marks is a WritableLayer because that is the type `EditManager.Layer` admits and the one layer shape
// carrying in-place `Add`/`TryRemove` — the overlay provider chain decorates a fetch and cannot receive
// an authoring write, so the residency law that governs overlays does not reach the edit layer and a
// decorated redline layer is unspellable. Symbology is a different axis and DOES reach it: the layer
// takes the same `ThemeStyle(Func<IFeature, Viewport, IStyle?>)` selector every overlay takes, so a
// redline mark colours by its own `IssueStatus` attribute through the one symbology owner. A flat
// per-layer VectorStyle is the deleted form — it forced every mark to one colour and forked review state
// into a layer-per-status roster the page's own Growth line rules out. `Wire` is the composition-bound
// serializer options every AppUi persisted payload shares; a page-local `JsonSerializerOptions` mint
// beside it forks the converter set the annotation blob decodes under. `History` and `Stamp` arrive by
// composition exactly as `Editing/forms` `ParameterLane` takes them, so the recorder and the causal stamp
// have one owner each across every editing surface and this page mints neither.
public sealed record RedlineSurface(
    EditManager Manager,
    WritableLayer Marks,
    Func<IFeature, Viewport, IStyle?> Symbology,
    JsonSerializerOptions Wire,
    EditHistory History,
    Func<HlcStamp> Stamp) {
    // Display style resolves once at mount from the SAME selector shape overlays bind, so the review
    // board and the redline plane read one symbology vocabulary — and the session takes that same layer
    // instance here, so the layer a mark draws on and the layer a session authors into are one object.
    public WritableLayer Mounted() {
        Marks.Style = new ThemeStyle(Symbology);
        Manager.Layer = Marks;
        return Marks;
    }

    // Every durable verb produces the intent AND records its inverse on one pass, so a committed mark is
    // reachable by the one `history.undo` intent rather than being durable-and-unreachable. The two local
    // verbs and the discard answer `None` and record nothing, because a session write no ledger saw has
    // nothing to revert. The `Before` payload is the snapshot `Apply` took ahead of the seal, so the delta
    // is a `Set` over two DEFINED payloads — a first authoring reads `null` before, a delete reads `null`
    // after, and a null value kind admits on the recorder's rail while an undefined one refuses.
    public IO<Fin<Option<(EditIntent Intent, EditReceipt Receipt)>>> Drive(
        RedlineVerb verb, ClockPolicy clocks, CorrelationId correlation) =>
        Apply(Manager, verb).Match(
            Succ: session => verb.Switch(
                state: (Session: session, Surface: this),
                beginMark: static (_, _) => IO.pure(Fin.Succ(Option<(EditIntent, EditReceipt)>.None)),
                modify: static (_, _) => IO.pure(Fin.Succ(Option<(EditIntent, EditReceipt)>.None)),
                delete: static (scope, command) => scope.Surface.Durable(
                    AdmitIdentity(command.DocKey, command.TargetId)
                        .Bind(_ => scope.Surface.Annotation(command.DocKey, command.TargetId, None, scope.Session.Before)),
                    command.DocKey, command.TargetId, scope.Session.Before, clocks, correlation),
                commit: static (scope, commit) => scope.Surface.Durable(
                    scope.Surface.Sealed(commit, scope.Session),
                    commit.DocKey, commit.TargetId, scope.Session.Before, clocks, correlation),
                discard: static (_, _) => IO.pure(Fin.Succ(Option<(EditIntent, EditReceipt)>.None))),
            Fail: error => IO.pure(Fin.Fail<Option<(EditIntent, EditReceipt)>>(error)));

    // The recorder hop. The op's delta is the payload the target CARRIED beside the payload it now carries,
    // both read off the one annotation blob, so the inverse is the same `Set` with its halves swapped and no
    // geometry-level inversion exists. The apply fold is the SAME layer write the committed-intent re-render
    // performs, so undo and re-render cannot drift; `EditHistory.Record` owns the recorder enqueue and the
    // receipt, so nothing here re-implements the client window or the durable arm.
    IO<Fin<Option<(EditIntent Intent, EditReceipt Receipt)>>> Durable(
        Fin<EditIntent> intent, string docKey, string targetId, JsonElement before,
        ClockPolicy clocks, CorrelationId correlation) =>
        intent.Match(
            Succ: sealed_ => History
                .Record(
                    new RevertibleOp(targetId, docKey,
                        new RevertDelta.Set(before, ((EditIntent.Annotation)sealed_).Payload),
                        Stamp()),
                    op => Replay(op),
                    clocks,
                    correlation)
                .Map(recorded => recorded.Map(receipt => Some((sealed_, receipt)))),
            Fail: error => IO.pure(Fin.Fail<Option<(EditIntent, EditReceipt)>>(error)));

    // The one layer write both a revert and a committed-intent re-render take: decode the payload, drop any
    // feature already stamped with the target, and re-add the mark the payload describes projected forward
    // into the view frame. A `Delete` payload writes nothing back, which is exactly what makes the undo of a
    // delete an upsert of the payload the recorder held.
    Fin<Unit> Replay(RevertibleOp op) =>
        op.Delta is RevertDelta.Set set
            ? Try.lift(() => {
                toSeq(Marks.GetFeatures())
                    .Choose(feature => feature is GeometryFeature geometry
                        && geometry["id"] is string id && StringComparer.Ordinal.Equals(id, op.Target)
                        ? Some(geometry) : None)
                    .Iter(stale => ignore(Marks.TryRemove(stale)));
                Optional(JsonSerializer.Deserialize<RedlineDelta>(set.After, Wire))
                    .Bind(delta => delta is RedlineDelta.Upsert upsert ? Some(upsert.Mark) : None)
                    .Iter(mark => Marks.Add(Drawn(mark, op.Target)));
                Marks.DataHasChanged();
                return unit;
            }).Run().MapFail(error => (Error)new ChartFault.VisualDegenerate($"redline replay: {error.Message}"))
            : Fin.Fail<Unit>(new ChartFault.VisualDegenerate($"redline replay: {op.Kind.Key} is not an annotation delta"));

    // The forward leg of the one MercatorFilter, mirroring `Sealed`'s inverse: a payload is WGS-84 and the
    // view frame is EPSG:3857, so a mark re-entering the layer crosses exactly once and the two directions
    // stay one filter.
    GeometryFeature Drawn(RedlineMark mark, string target) {
        NetTopologySuite.Geometries.Geometry view = Geometry(mark.Shape);
        view.Apply(MercatorFilter.Forward);
        view.SRID = 3857;
        GeometryFeature feature = new(view);
        feature["id"] = target;
        // The committed blob rides the feature so the NEXT gesture's snapshot reads the exact bytes the
        // ledger holds; re-deriving a payload from the drawn geometry would lose the style policy entirely
        // and make every undo restore a mark in the wrong ink.
        feature["payload"] = JsonSerializer.Serialize<RedlineDelta>(new RedlineDelta.Upsert(mark), Wire);
        return feature;
    }

    // The shape union back to NTS, the exact inverse of `Shape`: one arm per case, so a case added to the
    // payload breaks BOTH directions at compile time rather than round-tripping as an absence.
    static NetTopologySuite.Geometries.Geometry Geometry(RedlineShape shape) => shape.Switch(
        point: static value => (NetTopologySuite.Geometries.Geometry)new NetTopologySuite.Geometries.Point(value.Lon, value.Lat),
        path: static value => new NetTopologySuite.Geometries.LineString(Ring(value.Vertices)),
        area: static value => new NetTopologySuite.Geometries.Polygon(
            new NetTopologySuite.Geometries.LinearRing(Ring(value.Shell)),
            value.Holes.Map(static hole => new NetTopologySuite.Geometries.LinearRing(Ring(hole))).ToArray()),
        collection: static value => new NetTopologySuite.Geometries.GeometryCollection(
            value.Members.Map(Geometry).ToArray()));

    static NetTopologySuite.Geometries.Coordinate[] Ring(Seq<(double Lon, double Lat)> vertices) =>
        vertices.Map(static at => new NetTopologySuite.Geometries.Coordinate(at.Lon, at.Lat)).ToArray();

    // The one adapter over the `EditManager` session, total over the verb union. Session lifecycle is the
    // `EditMode` write — the kind's own column opens a stroke, `EndEdit()` seals a `Drawing*` stroke into
    // `Layer`, `EditMode.None` ends the session — and every gesture BETWEEN those writes is `EditingWidget`
    // routing pointer input into `EditManipulation` against this same manager, so no vertex, drag, insert,
    // rotate, or scale call appears here and a page-local pointer router is unspellable. The manager is a
    // mutable host state machine, the named statement carve-out on this page; the capture converts a
    // throwing member into the page's own typed refusal so an editing fault never escapes as a package
    // exception past the arm that closes the session.
    Fin<RedlineSession> Apply(EditManager manager, RedlineVerb verb) =>
        Try.lift(() => verb.Switch(
                state: (Manager: manager, Wire),
                beginMark: static (scope, begin) => Opened(scope.Manager, begin.Kind.Mode),
                modify: static (scope, _) => Opened(scope.Manager, EditMode.Modify),
                // Delete snapshots the payload it is about to remove: without it the recorded delta would
                // carry an undefined `Before` and the undo of a delete could restore nothing.
                delete: static (scope, command) => Closing(scope.Manager, command.TargetId, scope.Wire),
                // A session commit seals the in-flight stroke off the manager; a pen commit has nothing in
                // flight there, so it snapshots and closes without `EndEdit` — sealing an empty session would
                // drop back into the previous `Add*` mode and re-arm a stroke the gesture already finished.
                commit: static (scope, commit) => commit.Origin.Switch(
                    state: (scope.Manager, scope.Wire, commit.TargetId),
                    session: static (s, _) => Sealing(s.Manager, s.TargetId, s.Wire),
                    stroke: static (s, _) => Closing(s.Manager, s.TargetId, s.Wire)),
                discard: static (scope, _) => RedlineSession.Empty with { Authored = Close(scope.Manager) }))
            .Run()
            .MapFail(error => (Error)new ChartFault.VisualDegenerate($"redline: {error.Message}"));

    // Opening writes the mode and nothing else — the widget authors from there, so nothing is in flight
    // yet and the arm has no feature and no prior payload to answer with.
    static RedlineSession Opened(EditManager manager, EditMode mode) {
        manager.EditMode = mode;
        return RedlineSession.Empty;
    }

    // The session a verb leaves behind: the feature it sealed, if any, and the payload the target CARRIED
    // before this gesture ran. Both halves exist because the recorder's delta is a before-and-after pair and
    // the "before" is unrecoverable once `EndEdit` has written over the layer.
    public readonly record struct RedlineSession(Option<GeometryFeature> Authored, JsonElement Before) {
        // A JSON null is a DEFINED value kind, so it admits on the recorder's rail while `default` would be
        // undefined and refuse — which is what makes "this target had nothing before" recordable at all.
        public static readonly JsonElement Nothing = JsonDocument.Parse("null").RootElement.Clone();

        public static readonly RedlineSession Empty = new(None, Nothing);
    }

    // The prior payload, read off the feature's own field rather than re-derived from its geometry: `Drawn`
    // stamps the committed blob beside the id, so the layer carries the exact bytes the ledger holds and a
    // reconstruction that had to re-infer the style policy from a bare geometry is unspellable.
    static JsonElement Prior(EditManager manager, string target, JsonSerializerOptions wire) =>
        Stamped(manager, target)
            .Bind(feature => Optional(feature["payload"] as string))
            .Map(text => JsonSerializer.Deserialize<JsonElement>(text, wire))
            .IfNone(RedlineSession.Nothing);

    // Delete closes the session AFTER snapshotting, so the payload the recorder inverts is the one the layer
    // actually held at the moment of the gesture.
    static RedlineSession Closing(EditManager manager, string target, JsonSerializerOptions wire) {
        JsonElement before = Prior(manager, target, wire);
        return new RedlineSession(Close(manager), before);
    }

    // `EndEdit` seals an in-progress `DrawingLine` or `DrawingPolygon` stroke into `Layer` and answers
    // `false` on every path of the shipped member, so the sealed feature is READ BACK off the layer and
    // never taken from that return. A new mark is the layer's draft; a modified one is already stamped
    // with this commit's target — the two reads are disjoint, and a session that sealed neither answers
    // `None` so `Drive` lands the typed empty-commit refusal rather than an annotation over nothing.
    // The snapshot precedes `EndEdit` by construction: the seal writes over the layer, so a prior payload
    // read after it would be the payload this very gesture just wrote and every recorded delta would be a
    // no-op whose undo restored the edit it was meant to reverse.
    static RedlineSession Sealing(EditManager manager, string target, JsonSerializerOptions wire) {
        JsonElement before = Prior(manager, target, wire);
        ignore(manager.EndEdit());
        Option<GeometryFeature> authored = Drafted(manager) | Stamped(manager, target);
        Close(manager);
        return new RedlineSession(authored, before);
    }

    // The one closing move, shared by commit, delete, and discard. The DRAFT is dropped because no
    // committed intent will ever rebuild it, while a modified mark is left in place — the layer re-renders
    // from intent and that rebuild restores its pre-edit geometry, so discarding a modification by
    // removing the feature would delete a committed mark. Reset precedes the mode write because an armed
    // drag, rotation, or scale left in the session re-enters the next one mid-manipulation against a
    // feature that no longer exists.
    static Option<GeometryFeature> Close(EditManager manager) {
        Drafted(manager).Iter(draft => ignore(manager.Layer?.TryRemove(draft)));
        manager.ResetManipulations();
        manager.EditMode = EditMode.None;
        return None;
    }

    static Option<GeometryFeature> Drafted(EditManager manager) => Held(manager, static geometry => geometry["id"] is null);

    static Option<GeometryFeature> Stamped(EditManager manager, string target) =>
        Held(manager, geometry => geometry["id"] is string id && StringComparer.Ordinal.Equals(id, target));

    // Every mark rebuilt from committed intent carries its target id in the feature's own field set, so
    // "unstamped draft" and "stamped with this target" partition the layer and neither read rests on an
    // enumeration order `WritableLayer` never promises.
    static Option<GeometryFeature> Held(EditManager manager, Func<GeometryFeature, bool> admits) =>
        Optional(manager.Layer)
            .Bind(layer => toSeq(layer.GetFeatures())
                .Choose(feature => feature is GeometryFeature geometry && admits(geometry) ? Some(geometry) : None)
                .Head);

    // The ONE commit seal, total over the origin union: identity admits once, the shape and the paint policy
    // resolve per origin, and the delta serializes once — so the vertex session and the pen stroke reach
    // durable truth through one body and cannot drift on the first repair to either. An ERASING stroke lands
    // `RedlineDelta.Delete`, because the capture already read that intent off the stylus axis and a mark that
    // committed an upsert after the nib was inverted would draw exactly what the gesture asked to remove.
    Fin<EditIntent> Sealed(RedlineVerb.Commit commit, RedlineSession session) =>
        AdmitIdentity(commit.DocKey, commit.TargetId).Bind(_ => commit.Origin.Switch(
            state: (Surface: this, Commit: commit, Session: session),
            session: static (scope, origin) => scope.Session.Authored
                .ToFin((Error)new ChartFault.VisualEmpty("redline: commit has no authored feature"))
                .Bind(feature => Optional(feature.Geometry)
                    .ToFin((Error)new ChartFault.VisualEmpty("redline: authored feature has no geometry")))
                .Bind(Wgs84)
                .Bind(Shape)
                .Bind(shape => origin.Style.Admit().Map(style => (Shape: shape, Style: style)))
                .Bind(mark => scope.Surface.Annotation(
                    scope.Commit.DocKey, scope.Commit.TargetId, Some(mark), scope.Session.Before)),
            // The stroke arm never reads the manager: the capture IS the geometry, so the screen samples
            // unproject through the frame the ORIGIN carries and the same inverse filter lifts them out of the
            // view frame. A stroke short of two surviving vertices is a degenerate mark rather than a line
            // pinned to one point, which is the same verdict the viewport leg reaches on its own unprojection.
            stroke: static (scope, origin) => origin.Captured.Erases
                ? scope.Surface.Annotation(
                    scope.Commit.DocKey, scope.Commit.TargetId, None, scope.Session.Before)
                : RedlineStyle.Of(origin.Captured, origin.Opacity, origin.Dash).Admit()
                    .Bind(style => Traced(origin.Captured, origin.Frame)
                        .Map(shape => (Shape: shape, Style: style)))
                    .Bind(mark => scope.Surface.Annotation(
                        scope.Commit.DocKey, scope.Commit.TargetId, Some(mark), scope.Session.Before))));

    // Inverse leg of the one MercatorFilter: authored view geometry returns to WGS-84 before it crosses the
    // intent seam, so no EPSG:3857 coordinate ever lands in durable truth.
    static Fin<NetTopologySuite.Geometries.Geometry> Wgs84(NetTopologySuite.Geometries.Geometry view) =>
        Try.lift(() => {
            NetTopologySuite.Geometries.Geometry wgs84 = view.Copy();
            wgs84.Apply(MercatorFilter.Inverse);
            wgs84.SRID = 4326;
            return wgs84;
        }).Run().MapFail(error => (Error)new ChartFault.VisualDegenerate($"redline: {error.Message}"));

    // The pen stroke's own projection. Screen samples cross the camera the GESTURE was drawn under — the
    // origin carries it — rather than whatever camera is mounted when the commit runs: a stroke commits after
    // the gesture ends, so reading the live viewport lands every mark displaced by exactly the pan, zoom, or
    // rotation between the last sample and the commit, and nothing on either leg reports it. The world result
    // is EPSG:3857, so the one inverse filter carries it the rest of the way and no second mercator
    // expression exists on this page.
    static Fin<RedlineShape> Traced(RedlineStroke stroke, Viewport frame) =>
        stroke.Points
            .Map(point => frame.ScreenToWorldXY(point.X, point.Y))
            .Filter(static at => double.IsFinite(at.worldX) && double.IsFinite(at.worldY)) switch {
            var world when world.Count < 2 =>
                Fin.Fail<RedlineShape>(new ChartFault.VisualDegenerate($"redline: stroke carries {world.Count} projectable samples")),
            var world => Wgs84(new NetTopologySuite.Geometries.LineString(
                    world.Map(static at => new NetTopologySuite.Geometries.Coordinate(at.worldX, at.worldY)).ToArray()))
                .Bind(Shape),
        };

    // The ONE serialize both durable verbs cross, and the one REMOVAL gate beside it. An absent mark is the
    // delete delta, so an erasing stroke and an explicit delete verb produce the identical payload and the
    // recorder inverts both the same way; a second `SerializeToElement` call site would let the two payloads
    // drift on the first wire repair. A removal whose target carried NOTHING refuses here: the layer
    // re-renders from committed intent, so a target absent from it was never committed, and recording that
    // removal seals a delta whose before and after are both empty — a durable no-op the ledger keeps, the fan
    // counts as an applied edit, and an undo restores nothing from. A first AUTHORING reads the same empty
    // prior and admits, because the removal is what the emptiness contradicts rather than the emptiness itself.
    Fin<EditIntent> Annotation(
        string docKey, string targetId, Option<(RedlineShape Shape, RedlineStyle Style)> mark, JsonElement before) =>
        mark.IsNone && before.ValueKind is JsonValueKind.Null
            ? Fin.Fail<EditIntent>(new ChartFault.VisualEmpty($"redline: {targetId} carries no mark to remove"))
            : Fin.Succ((EditIntent)new EditIntent.Annotation(
                docKey,
                targetId,
                JsonSerializer.SerializeToElement<RedlineDelta>(
                    mark.Match<RedlineDelta>(
                        Some: static row => new RedlineDelta.Upsert(new RedlineMark(row.Shape, row.Style)),
                        None: static () => new RedlineDelta.Delete()),
                    Wire)));

    static Fin<Unit> AdmitIdentity(string docKey, string targetId) =>
        !string.IsNullOrWhiteSpace(docKey) && !string.IsNullOrWhiteSpace(targetId)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ChartFault.VisualDegenerate("redline: document and target identities are required"));

    static Fin<RedlineShape> Shape(NetTopologySuite.Geometries.Geometry geometry) => geometry switch {
        NetTopologySuite.Geometries.Point point when Grounded(point.X, point.Y) =>
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

    // Admission is on the WGS-84 DOMAIN rather than on finiteness alone: an ordinate outside the mercator
    // clamp is a coordinate the projection is asymptotic at, so a sample taken in the band a pan past the
    // pole exposes is a point that never had ground and refuses by name instead of committing a latitude no
    // consumer admits. Longitude carries no gate here — the inverse row folds it onto its principal range,
    // because a cylindrical projection panned past the antimeridian still names the place it points at.
    static bool Grounded(double lon, double lat) =>
        double.IsFinite(lon) && double.IsFinite(lat) && Math.Abs(lat) <= MercatorFilter.PoleClampDeg;

    static Fin<Seq<(double Lon, double Lat)>> Vertices(NetTopologySuite.Geometries.Coordinate[] coordinates, int minimum, string kind) {
        Seq<(double Lon, double Lat)> vertices = toSeq(coordinates).Map(static at => (at.X, at.Y));
        return vertices.Count >= minimum && vertices.ForAll(static at => Grounded(at.Lon, at.Lat))
            ? Fin.Succ(vertices)
            : Fin.Fail<Seq<(double Lon, double Lat)>>(new ChartFault.VisualDegenerate($"redline: {kind} vertices are invalid"));
    }

    public const string GestureInstrument = "rasm.appui.redline.gesture";

    // `Drive` is the one place a disposition exists, so the projection binds there and the count carries it
    // as the DECLARED outcome dimension — a spec promising a per-disposition breakdown with no declared
    // slot folds commits, discards, and every local gesture into one number no board can separate. The
    // disposition is a total Switch, so a sixth verb breaks this projection rather than counting untagged.
    public static Fin<Unit> Observe(InstrumentSet set, RedlineVerb verb) =>
        set.Write(GestureInstrument, 1L, InstrumentSet.Tags((AppUiTelemetry.OutcomeSlot, Disposition(verb))));

    static string Disposition(RedlineVerb verb) => verb.Switch(
        beginMark: static _ => "begin",
        modify: static _ => "modify",
        delete: static _ => "delete",
        commit: static _ => "commit",
        discard: static _ => "discard");

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version,
            InstrumentSpec.Count(GestureInstrument, "{gesture}", "redline gestures by disposition", MeasureForm.Whole,
                AppUiTelemetry.OutcomeSlot));
}
```

## [06]-[RESEARCH]

(none)

# [APPUI_CHARTS_BASEMAP]

The basemap is the tiled 2D geographic plane beside the Wgpu viewport: one Mapsui `MapControl` hosts one `Map` whose layer stack is data rows — a `BasemapSource` tile row carrying its own attribution, cache, and user-agent posture, feature rows projecting Bim-owned geospatial geometry from a resident set or the Bim MVT pyramid under one `Symbology` selector, and widget rows re-tinted from the one token resolve — with navigation through the one `Navigator`, tile health rendered as fact off the layer's own fetch signals, feature picking through `GetMapInfo` under the kernel bounded-selection law, snapshots through the capture encode fold, and design-review redlining through the `EditManager` surface committing as `EditIntent.Annotation` and recording onto the one `EditHistory` recorder. The page owns the layer row family, the tile-source catalog, the symbology axis, the CRS ingress law, the pick and snapshot folds, and the redline authoring session: Bim owns geodesy (`GeoReference`, `GeoFeature.Reproject`, IfcMapConversion lowering) and AppUi reprojects ONLY WGS-84 input through `SphericalMercator` — a local geodesy kernel on this plane is the forbidden form. LiveCharts `GeoMap`/`DrawnMap` stays the CHART-projection row on `Charts/grammar.md`; this page is the TILED-basemap owner — disjoint charters.

## [01]-[INDEX]

- [02]-[MAP_SURFACE]: One `MapControl`/`Map`; the layer row family; tile sources, attribution, health; navigation verbs.
- [03]-[NTS_OVERLAY]: Bim geospatial features as `GeometryFeature` rows; the CRS ingress gate; the MVT pyramid pump.
- [04]-[PICK_AND_SNAPSHOT]: Feature hit-test bounded by the kernel selection cell; tolerance, disambiguation, hover; capture snapshots.
- [05]-[REDLINE]: Design-review markup over `EditManager`; commit as `EditIntent.Annotation` onto the recorder.

## [02]-[MAP_SURFACE]

- Owner: `BasemapLayerRow` `[Union]` — the closed layer vocabulary; `BasemapSource` `[SmartEnum<string>]` — the tile-source catalog carrying URL template, zoom band, and attribution, with the polarity election beside it; `TilePolicy` `[ComplexValueObject]` — the admitted cache-and-agent posture a tile row mounts under; `WidgetRow` `[SmartEnum<string>]` — the chrome catalog whose rows derive their shape, mint, and re-tint from one typed row factory; `Symbology` — the ONE per-feature style selector three planes bind; `TileHealth` with `TileState` `[Union]` — the rendered fetch state carrying its retriability; `BasemapSurface` — the one map owner; `MapNav` `[Union]` with `MapMove` — the navigation verb vocabulary and its one projection.
- Cases: `BasemapLayerRow` = Tile · Features · Widget; `BasemapSource` = osm · carto-light · carto-dark, the light/dark election a declared table keyed on `SurfacePolarity` rather than a per-row affinity bool; `TileState` = Fetching · Ready · Failed · Offline; `WidgetRow` = scale-bar · zoom-buttons · info-box · coordinates · ruler, with each tile layer's own attribution credit riding the layer rather than a row of its own; `MapNav` = CenterOn · ZoomTo · ZoomToLevel · ZoomToBox · CenterAndZoom · FlyTo · RotateTo; `MapFlight` = Direct · Focus · Traverse, carrying a NodaTime `Duration` rather than a raw millisecond count; `BasemapFact` = Swapped · Navigated · Health — the one observation vocabulary the surface folds.
- Entry: `public Fin<Map> Build(Seq<BasemapLayerRow> rows, ChartInk ink, ResolvedTheme theme)` — one fold from layer rows to the seated `Map`, staging under kernel `Custody.Rollback` and seating through one `Cell.Commit` transition; `public IO<Fin<Unit>> Navigate(MapNav verb)` — every camera move reads the verb's one `MapMove` projection and dispatches through the one `Navigator`; `public IAsyncEnumerable<TileHealth> Watch(CancellationToken token)` — the tile-health stream over a bounded `Channel`; `public Fin<Unit> Retint(ChartInk ink, ResolvedTheme theme)` — the in-place chrome and background swap.
- Auto: a tile row names a `BasemapSource` row and a `TilePolicy`, and `Mount` builds the `TileLayer` over an `HttpTileSource` carrying that source's URL template, zoom band, `Attribution`, and the policy's `IPersistentCache<byte[]>` — so a DARK basemap is a source row beside the light one rather than a colour filter over tiles, which the renderer structurally cannot apply because the raster path exposes no tint, blend, or colour-matrix hook at all. The user-agent posture is the policy's own admitted column written onto the source's `HttpClient` through `ConfigureHttpRequestMessage`: `HttpTileSource.GetTileAsync` THROWS when neither the client's default headers nor that hook supply one, so the agent is an admission requirement rather than a courtesy. ATTRIBUTION IS REQUIRED: every tile row contributes its source's own `Attribution` onto the layer's `HyperlinkWidget`, `Map.GetWidgetsOfMapAndLayers()` surfaces it, and `Build` REFUSES a row set carrying a tile row whose source declares no attribution text — a basemap drawing another party's tiles without its credit is a licence breach, so absence refuses at admission rather than shipping. `Map.BackColor` seats the resolved panel token so the gap around a partially-fetched world reads as the product's own surface rather than as white. The map chrome ships as `WidgetRow` values whose row factory carries the widget TYPE, its map-taking constructor, and its typed re-tint from one type argument, so the sweep's `Shape` match and the re-tint's own cast are one discrimination rather than three per widget per swap. Layer z-order is sequence order. `Build` stages a candidate map under `Custody.Rollback` so any row failure releases it on the failure arm alone, seats it through one `Cell.Commit` whose committed post-state ANSWERS the map it retired, and disposes that retired map — so a losing concurrent rebuild releases its own mint instead of leaking it. `Watch` folds each tile layer's `Busy` transitions and its `DataChanged` exception payload into the `TileHealth` stream, so loading, error, and offline are RENDERED FACTS a surface states rather than a blank tile grid the viewer must interpret; the offline verdict is the connectivity-refused arm of that same payload, and every arm carries the kernel `Retriability` its classification implies so a viewer's affordance and a transport's re-drive read one column.
- Receipt: `Build`, `Navigate`, and `Watch` fold their own `BasemapFact` at the producing site; failures remain concrete `ChartFault` leaves.
- Packages: Mapsui.Avalonia12, BruTile, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Cell.Commit`/`Transition`, `Custody.Rollback`, `Retriability`, `InstrumentSpec`), BCL inbox (`System.Threading.Channels`)
- Growth: a new basemap appearance is one `BasemapSource` row carrying its template, band, and attribution plus its election entry; a new cache or agent posture is one `TilePolicy` value; a new chrome surface is one `WidgetRow.Of<TWidget>` row; a new health state is one `TileState` case; a new feature family is one `BasemapLayerRow.Features` value over a `FeatureSource` arm; a new camera move is one `MapNav` case answering one `MapMove`; zero new surface.
- Boundary: ONE `MapControl` and ONE `Map` per basemap surface — a second map control, a per-overlay map, or a parallel tile engine is the deleted form; the Mapsui/Tiling/Rendering.Skia set stays transitive under the `Mapsui.Avalonia12` pin while `Mapsui.Nts` is admitted directly, because `[03]` and `[05]` compose its geometry, provider, and editing members by name, and `BruTile` is admitted directly because `HttpTileSource`, `GlobalSphericalMercator`, `Attribution`, and `FileCache` are the source vocabulary a non-default tile row is spelled in and `ITileSchema.GetTileInfos` is the covering-tile arithmetic the vector pyramid's provider reads its LOD off. A dark basemap is a SOURCE, never a post-effect: `RasterStyleRenderer` draws a tile under layer and style opacity and strokes `RasterStyle.Outline` alone, and no colour-matrix, tint, or blend hook reaches the raster path — `Image.BlendModeColor` tints symbol imagery only — so an inverted or hue-rotated basemap is unspellable rather than merely discouraged. RASTER TILE RE-DRIVE IS THE PACKAGE'S: `TileFetchPlanner` owns the fetch queue and `TileFetchStatus.GaveUp` is its exhausted-retry verdict, so a `RedrivePolicy` wrapped around `ITileSource` here would be a second retry layer stacked on one the layer already runs — the kernel re-drive rides `[03]`'s pyramid fetch, which is the transport this page actually owns. EVERY entry on this surface answers `Fin`: `Navigate` returning a bare `IO<Unit>` swallowed an out-of-band level, a non-finite resolution, and a degenerate box into a silent no-op while `Build`, `Retint`, and every `[03]`/`[05]` entry beside it refused by name, so a caller could not compose the two without discriminating on the shape of the rail. The basemap draws BESIDE the Wgpu viewport as an Avalonia control — it never enters the render graph, and geographic dashboards that need chart-projected geography stay on the LiveCharts `GeoMap` row (`Charts/grammar.md`), the charter split stated on both pages.
- Boundary: `BasemapSurface` holds a LIVE `MapControl` and a live seat cell, so it is a sealed class whose transition answers what it retired — folder RULINGS `[02]` rules a record copy sharing that cell by reference the deleted form.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// The tile-source catalog. A row carries everything a slippy source IS — the URL template its tiles resolve
// through, the zoom band its schema covers, and the attribution its licence requires. Appearance is NOT a row
// column: two of three rows are light, so a per-row affinity bool left `For` answering whichever row
// enumerated first under a light variant — an election nothing declared. `Election` is that election.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BasemapSource {
    public static readonly BasemapSource Osm = new("osm",
        "https://tile.openstreetmap.org/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 19,
        new Attribution("OpenStreetMap contributors", "https://www.openstreetmap.org/copyright"));
    public static readonly BasemapSource CartoLight = new("carto-light",
        "https://basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 20,
        new Attribution("CARTO, OpenStreetMap contributors", "https://carto.com/attributions"));
    public static readonly BasemapSource CartoDark = new("carto-dark",
        "https://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png", minZoom: 0, maxZoom: 20,
        new Attribution("CARTO, OpenStreetMap contributors", "https://carto.com/attributions"));

    // The ONE polarity correspondence. The high-contrast rows project the same polarity as the plain ones, so
    // four variants resolve through two entries and no second appearance vocabulary exists here; `Osm` is the
    // declared baseline for a polarity the table does not name, because a basemap that renders nothing is
    // worse than one whose palette lags its chrome by one variant.
    static readonly HashMap<SurfacePolarity, BasemapSource> Election = HashMap(
        (SurfacePolarity.Light, CartoLight),
        (SurfacePolarity.Dark, CartoDark));

    public string Template { get; }

    public int MinZoom { get; }

    public int MaxZoom { get; }

    public Attribution Credit { get; }

    public static BasemapSource For(ThemeVariantRow variant) =>
        Election.Find(variant.Projection.Polarity).IfNone(Osm);

    // The one source construction: the global spherical-mercator schema bounded by the row's own zoom band,
    // the policy's persistent cache, the row's own attribution, and the policy's agent write.
    // The absent cache writes a null slot, which the package's own signature takes as "no persistent store";
    // the probe carries its own presence proof, so the payload read is admitted rather than an unguarded peek.
    public ITileSource Source(TilePolicy policy) =>
        new HttpTileSource(
            new GlobalSphericalMercator(minZoomLevel: MinZoom, maxZoomLevel: MaxZoom),
            Template,
            name: Key,
            persistentCache: policy.Cache is { IsSome: true, Case: IPersistentCache<byte[]> cache } ? cache : null,
            attribution: Credit,
            configureHttpRequestMessage: request => request.Headers.TryAddWithoutValidation("User-Agent", policy.Agent));
}

// The chrome catalog. `Of<TWidget>` is the ONE type discrimination: the row's `Shape`, its map-taking mint, and
// its typed re-tint all derive from one type argument, so a hand-transcribed `typeof` column that could disagree
// with the delegate beside it is unspellable and the per-row `is` ladder the sweep already decided is gone.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WidgetRow {
    // `ScaleBarWidget` and `MapInfoWidget` bind their map at construction and publish no parameterless arity,
    // so the mint column is a real constructor rather than a placeholder a zero-argument row could carry.
    public static readonly WidgetRow ScaleBar = Of<ScaleBarWidget>("scale-bar",
        static map => new ScaleBarWidget(map),
        static (bar, ink) => { bar.TextColor = Tint(ink, ChartChrome.AxisLabel); bar.Halo = Tint(ink, ChartChrome.FrameFill); });
    public static readonly WidgetRow ZoomButtons = Of<ZoomInOutWidget>("zoom-buttons",
        static _ => new ZoomInOutWidget(),
        static (zoom, ink) => {
            zoom.StrokeColor = Tint(ink, ChartChrome.FrameStroke);
            zoom.TextColor = Tint(ink, ChartChrome.TooltipText);
            zoom.BackColor = Tint(ink, ChartChrome.TooltipBack);
        });
    public static readonly WidgetRow InfoBox = Of<MapInfoWidget>("info-box",
        static map => new MapInfoWidget(map, () => map.Layers),
        static (box, ink) => Text(box, ink));
    public static readonly WidgetRow Coordinates = Of<MouseCoordinatesWidget>("coordinates",
        static _ => new MouseCoordinatesWidget(),
        static (strip, ink) => Text(strip, ink));
    public static readonly WidgetRow Ruler = Of<RulerWidget>("ruler",
        static _ => new RulerWidget(),
        static (ruler, ink) => { ruler.Color = Tint(ink, ChartChrome.Crosshair); ruler.ColorOfBeginAndEndDots = Tint(ink, ChartChrome.CrosshairChip); });

    // The declared widget shape, so the re-tint sweep matches a mounted widget to its row by TYPE without
    // constructing one to compare against — a mint-to-match walk allocates a widget per row per swap and opens
    // a live `Map` binding for a comparison that discards it.
    public Type Shape { get; }

    [UseDelegateFromConstructor]
    public partial IWidget Mint(Map map);

    [UseDelegateFromConstructor]
    public partial void Retint(IWidget widget, ChartInk ink);

    static WidgetRow Of<TWidget>(string key, Func<Map, TWidget> mint, Action<TWidget, ChartInk> retint)
        where TWidget : class, IWidget =>
        new(key, typeof(TWidget), map => mint(map),
            (widget, ink) => { if (widget is TWidget typed) { retint(typed, ink); } });

    // Every text-box descendant re-tints through one write, so the readout, the coordinate strip, and every
    // layer's own attribution credit share one pigment — `HyperlinkWidget`, `MapInfoWidget`, and
    // `MouseCoordinatesWidget` all descend from `TextBoxWidget`, so the one pattern reaches the credit the tile
    // layer mints without a row of its own.
    internal static void Text(IWidget widget, ChartInk ink) {
        if (widget is TextBoxWidget box) {
            box.TextColor = Tint(ink, ChartChrome.TooltipText);
            box.BackColor = Tint(ink, ChartChrome.TooltipBack);
        }
    }

    // The ONE crossing from the chart chrome roster onto the map's own colour value, so a widget colour and a
    // chart hairline are one generated rung by construction and a basemap-local paint roster is unspellable.
    internal static Mapsui.Styles.Color Tint(ChartInk ink, ChartChrome chrome) =>
        ink.Tint(chrome) switch { var lvc => Mapsui.Styles.Color.FromArgb(lvc.A, lvc.R, lvc.G, lvc.B) };
}

// The flight-timing catalog. `Duration` rather than a bare millisecond `long`: the count crosses the package
// boundary exactly once, at the one `FlyTo` call the package's own signature demands it in.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapFlight {
    public static readonly MapFlight Direct = new("direct", Duration.Zero);
    public static readonly MapFlight Focus = new("focus", Duration.FromMilliseconds(240));
    public static readonly MapFlight Traverse = new("traverse", Duration.FromMilliseconds(480));

    public Duration Flight { get; }
}

// --- [CONSTANTS] ------------------------------------------------------------------------
// Feature field names are DECLARED constants on the surface that writes them (folder RULINGS `[02]`): the pick
// fold, the redline replay, and the CRS gate all address the same four slots, and one literal over two key
// roles binds by accident the first time a role is renamed at one site.
public static class FeatureSlot {
    public const string Id = "id";
    public const string Label = "label";
    public const string Source = "source";
    public const string Payload = "payload";
}

// --- [MODELS] ---------------------------------------------------------------------------
// The admitted fetch posture. `[ComplexValueObject]` rather than a plain record with a separate `Admit`: the
// agent is what keeps `HttpTileSource.GetTileAsync` from throwing and the residency pair bounds the in-memory
// tile cache, so an inadmissible posture is unspellable at declaration rather than refused at some later call.
[ComplexValueObject]
[ValidationError]
public sealed partial class TilePolicy {
    public string Agent { get; }
    public Option<IPersistentCache<byte[]>> Cache { get; }
    public int MinTiles { get; }
    public int MaxTiles { get; }

    // The residency band the layer keeps around the viewport; a caller retunes by handing a different pair to
    // `Create`, so the default pair is a policy value and never a knob threaded through `Of`.
    const int ResidentFloor = 200;
    const int ResidentCeiling = 300;
    static readonly Duration CacheLife = Duration.FromDays(14);

    public static Fin<TilePolicy> Of(string product, Option<string> cacheDirectory, Op? key = null) =>
        key.OrDefault().AcceptValidated<TilePolicy>(
            Validate(
                $"{product} ({HttpClientTools.GetDefaultApplicationUserAgent()})",
                cacheDirectory.Map(directory => (IPersistentCache<byte[]>)new FileCache(directory, "png", CacheLife.ToTimeSpan())),
                ResidentFloor,
                ResidentCeiling,
                out TilePolicy? admitted),
            admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string agent,
        ref Option<IPersistentCache<byte[]>> cache,
        ref int minTiles,
        ref int maxTiles) {
        agent = agent.Trim();
        validationError = (agent.Length, minTiles, maxTiles) switch {
            (0, _, _) => new ValidationError(string.Join(" | ", new object?[] { "tile policy: the source refuses a blank user agent" })),
            (_, <= 0, _) => new ValidationError(string.Join(" | ", new object?[] { $"tile policy: resident floor {minTiles} is not positive" })),
            (_, _, _) when maxTiles < minTiles => new ValidationError(string.Join(" | ", new object?[] { $"tile policy: ceiling {maxTiles} is under floor {minTiles}" })),
            _ => null,
        };
    }
}

// What a tile stack is DOING, as a value a surface renders rather than as a blank grid a viewer interprets.
// `Offline` is distinguished from `Failed` because the two need different messages: a refused request retries,
// an absent network waits, and a cached-only session is usable while the second is not. Every arm carries its
// kernel `Retriability`, so the viewer's affordance and the transport's re-drive read ONE column instead of
// each re-deriving the classification from the exception the layer published.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileState {
    private TileState() { }
    public sealed record Fetching : TileState;
    public sealed record Ready : TileState;
    public sealed record Failed(string Detail, Retriability Retry) : TileState;
    public sealed record Offline : TileState;

    // The floor a throttled public tile service is re-offered after. The transport hands back no `Retry-After`
    // through the package's `DataChangedEventArgs`, so the deferral is this page's declared floor rather than a
    // header read the seam does not carry.
    internal static readonly Duration ThrottleFloor = Duration.FromSeconds(30);

    // The state's own dimension value, projected by a total Switch so a fifth case breaks HERE at compile time
    // rather than widening the metric's key space silently.
    public string Key => Switch(
        fetching: static _ => "fetching",
        ready: static _ => "ready",
        failed: static _ => "failed",
        offline: static _ => "offline");

    // The retriability the metric's cause dimension carries; the two live arms are terminal by construction
    // because there is nothing to re-drive about an arrival. The projection is NOT named for its type — a
    // member named `Retriability` shadows that type inside this declaration and the rows below stop resolving.
    public Retriability Recovery => Switch(
        fetching: static _ => Retriability.Terminal,
        ready: static _ => Retriability.Terminal,
        failed: static row => row.Retry,
        offline: static _ => Retriability.Transient);

    // The ONE classification of a transport failure, read by BOTH the rendered health arm and the pyramid
    // fetch's re-drive predicate, so a viewer's message and a re-offer decision can never disagree. A socket or
    // DNS failure never reached a server and is offline; a throttle exits the in-process curve carrying the
    // declared floor, because re-drawing a 429 on the growth law hammers the window it was told to respect; a
    // confirmed absence is terminal, and anything else the server answered is transient.
    public static TileState Of(Option<Exception> fault) =>
        fault.Match(
            Some: static error => error switch {
                HttpRequestException { InnerException: SocketException } or SocketException => new Offline(),
                HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } throttle =>
                    new Failed(throttle.Message, Retriability.Throttled(ThrottleFloor)),
                HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.Gone } absent =>
                    new Failed(absent.Message, Retriability.Terminal),
                _ => (TileState)new Failed(error.Message, Retriability.Transient),
            },
            None: static () => new Ready());

    // Rail-side twin of the same classifier: an `Error` carries its captured exception, so the pyramid's
    // re-drive predicate and the layer's health fold read one law. When the shared `ChartFault` family
    // declares its own `Retriability` overrides, this reduces to `Redrive.Run` and the predicate deletes.
    public static Retriability Posture(Error error) => Of(error.Exception).Recovery;
}

// The per-layer fact. Every arm names the layer it describes, because a stream consumer that could not tell
// WHICH layer went ready read a mounted stack as one undifferentiated surface. A map holding no tile layer
// yields nothing at all — NAMED LOSS against the retired `Idle` arm, whose only producer was that emptiness.
public readonly record struct TileHealth(string Layer, TileState State) {
    public static TileHealth Of(ILayer layer, Option<Exception> fault) =>
        new(layer.Name, layer.Busy && fault.IsNone ? new TileState.Fetching() : TileState.Of(fault));
}

// The ONE per-feature style selector, shared by the resident overlay, the tiled pyramid, and the redline marks
// layer. Three sites carried the identical bare `Func` column and named no owner, so the constant selector and
// the graduated one had nowhere to live and neither had a consumer; the column IS this type now, and its three
// mints are that type's constructors rather than orphan statics.
public sealed record Symbology(Func<IFeature, Viewport, IStyle?> Select) {
    // A uniform family states its one style HERE, so a row carries no second style column and the themed and
    // uniform families never fork into sibling layer builders.
    public static Symbology Uniform(VectorStyle style) => new((_, _) => style);

    // The attribute-driven arm: the selector reads the consumed feature's own attribute table, so a parcel set
    // coloured by zoning class and a utility run coloured by service are one shape and the
    // layer-per-attribute-value proliferation the Growth line rules out is unrepresentable.
    public static Symbology Themed(Func<IFeature, Viewport, IStyle?> select) => new(select);

    // The choropleth seam: the theme rail's colormap is the ONE ramp authority, so its sampled stops become the
    // package's `ColorBlend` and the package's own `GradientTheme` interpolates against each feature's numeric
    // column. Nothing here interpolates colour — a page-local lerp between two hues would be a second ramp
    // drifting from the perceptually-uniform one the token page's class discipline proves, and a per-class style
    // roster would fork one thematic into a layer per bucket. `GradientTheme` interpolates only when its two
    // styles share one type drawn from `VectorStyle`, `ImageStyle`, or `LabelStyle` and only over a numeric
    // column, so the spec states both bounds and refuses rather than throwing inside the render pass. Every
    // defect refuses at once: a caller with a blank column AND an inverted range learns both.
    public static Fin<Symbology> Graduated(Colormap map, ResolvedTheme theme, ChoroplethSpec spec) =>
        (map.HeatMap(spec.Steps, static token => Mapsui.Styles.Color.FromArgb(token.A, token.R, token.G, token.B)),
            theme.Metric(MetricFamily.Stroke, step: 0)
                .ToFin((Error)new ChartFault.SpecRejected("choropleth: the stroke family resolves no step")))
        switch {
            var (stops, width) => stops.Bind(ramp => width.Map(stroke => Graded(ramp, stroke, spec))),
        };

    static Symbology Graded(Mapsui.Styles.Color[] stops, double stroke, ChoroplethSpec spec) {
        // Positions are the stop ordinals normalized onto the unit interval, which is exactly the domain the
        // blend samples, so the ramp the colormap generated survives unresampled.
        ColorBlend blend = new(stops, Enumerable.Range(0, stops.Length)
            .Select(index => index / (double)(stops.Length - 1)).ToArray());
        GradientTheme theme = new(spec.Column, spec.Floor, spec.Ceiling,
            new VectorStyle { Fill = new Brush(stops[0]), Outline = new Pen(stops[0], stroke) },
            new VectorStyle { Fill = new Brush(stops[^1]), Outline = new Pen(stops[^1], stroke) }) {
            FillColorBlend = blend,
            LineColorBlend = blend,
        };
        return new Symbology((feature, viewport) => theme.GetStyle(feature, viewport));
    }
}

// The graduated thematic's own admitted shape. Every column accumulates through the applicative, so the refusal
// names every defect rather than the first one a `&&` ladder short-circuited on.
[ComplexValueObject]
[ValidationError]
public sealed partial class ChoroplethSpec {
    public string Column { get; }
    public double Floor { get; }
    public double Ceiling { get; }
    public int Steps { get; }

    public static Fin<ChoroplethSpec> Of(string column, double floor, double ceiling, int steps, Op? key = null) =>
        (Named(column), Bound(floor, "floor"), Bound(ceiling, "ceiling"), Stops(steps))
            .Apply(static (name, low, high, count) => (Name: name, Low: low, High: high, Count: count))
            .As()
            .ToFin()
            .Bind(admitted => key.OrDefault().AcceptValidated<ChoroplethSpec>(
                Validate(admitted.Name, admitted.Low, admitted.High, admitted.Count, out ChoroplethSpec? spec), spec));

    static Validation<Error, string> Named(string column) =>
        string.IsNullOrWhiteSpace(column)
            ? Fail<Error, string>(new ChartFault.SpecRejected("choropleth: the numeric column is unnamed"))
            : Success<Error, string>(column.Trim());

    static Validation<Error, double> Bound(double value, string edge) =>
        double.IsFinite(value)
            ? Success<Error, double>(value)
            : Fail<Error, double>(new ChartFault.SpecRejected($"choropleth: {edge} is not finite"));

    static Validation<Error, int> Stops(int steps) =>
        steps >= 2
            ? Success<Error, int>(steps)
            : Fail<Error, int>(new ChartFault.SpecRejected($"choropleth: {steps} stops cannot ramp"));

    // The ordering claim the applicative cannot state, because it spans two admitted columns.
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string column, ref double floor, ref double ceiling, ref int steps) =>
        validationError = ceiling <= floor
            ? new ValidationError(string.Join(" | ", new object?[] { $"choropleth: {column} needs an ascending range, not [{floor}, {ceiling}]" }))
            : null;
}

// The closed layer vocabulary. `Overlay` and `Vectors` were ONE shape — identical key, symbology, and
// visibility columns differing only in where the features came from — so the source is the arm and the row is
// one case. NAMED LOSS: the residency-decorator election stops being implicit in the case name and becomes a
// column of the source arm, which is where `[03]` reads it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BasemapLayerRow {
    private BasemapLayerRow() { }
    // The tile row names its SOURCE and its POSTURE rather than carrying an opaque layer delegate: the row is
    // then a value a variant flip can re-select, an attribution admission can read, and a health watch can
    // identify, none of which a bare `Func<ILayer>` permits.
    public sealed record Tile(string Key, BasemapSource Source, TilePolicy Policy) : BasemapLayerRow;
    // The two resolution bounds are columns because the provider chain decimates BY resolution: a row that
    // cannot state its visible band cannot state its simplify band either, and a pyramid still declares the
    // resolutions its content was authored for even though its own LOD levels already clipped it.
    public sealed record Features(
        string Key,
        FeatureSource Source,
        Symbology Symbology,
        double MinVisible,
        double MaxVisible) : BasemapLayerRow;
    // The widget row names a catalog ROW, so the map-taking constructor and the token re-tint both ride the
    // vocabulary rather than a bare factory the swap could never reach a second time.
    public sealed record Widget(WidgetRow Row) : BasemapLayerRow;

    public static BasemapLayerRow Basemap(ThemeVariantRow variant, TilePolicy policy) {
        BasemapSource source = BasemapSource.For(variant);
        return new Tile(source.Key, source, policy);
    }
}

// The camera verb vocabulary. Three separate total switches once walked this one union — a key projection, an
// admission, and a dispatch — so a case landed three compile breaks and three chances to disagree. `Move` is
// the ONE projection: each arm answers its key, its admitted verdict, and the navigator call it applies.
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

    // Admission is total by construction: a resolution must be finite and positive because zero divides the
    // viewport scale, a level must sit inside the mounted source's own band, a box must have extent, and a
    // bearing must be finite.
    public MapMove Move => Switch(
        centerOn: static v => new MapMove("center-on", Finite("center-on", v.Center), nav => nav.CenterOn(v.Center)),
        zoomTo: static v => new MapMove("zoom-to", Positive("zoom-to", v.Resolution), nav => nav.ZoomTo(v.Resolution)),
        zoomToLevel: static v => new MapMove("zoom-to-level",
            v.Level >= 0 ? Fin.Succ(unit) : Refused("zoom-to-level"), nav => nav.ZoomToLevel(v.Level)),
        zoomToBox: static v => new MapMove("zoom-to-box",
            v.Box.Width > 0d && v.Box.Height > 0d ? Fin.Succ(unit) : Refused("zoom-to-box"), nav => nav.ZoomToBox(v.Box)),
        centerAndZoom: static v => new MapMove("center-and-zoom",
            Finite("center-and-zoom", v.Center).Bind(_ => Positive("center-and-zoom", v.Resolution)),
            nav => nav.CenterOnAndZoomTo(v.Center, v.Resolution)),
        flyTo: static v => new MapMove("fly-to",
            Finite("fly-to", v.Center).Bind(_ => Positive("fly-to", v.Resolution)),
            // The one crossing where the flight duration leaves the `Duration` carrier, because the package's
            // own signature takes milliseconds.
            nav => nav.FlyTo(v.Center, v.Resolution, (long)v.Flight.Flight.TotalMilliseconds)),
        rotateTo: static v => new MapMove("rotate-to",
            double.IsFinite(v.Degrees) ? Fin.Succ(unit) : Refused("rotate-to"), nav => nav.RotateTo(v.Degrees)));

    static Fin<Unit> Finite(string key, MPoint center) =>
        double.IsFinite(center.X) && double.IsFinite(center.Y) ? Fin.Succ(unit) : Refused(key);

    static Fin<Unit> Positive(string key, double resolution) =>
        double.IsFinite(resolution) && resolution > 0d ? Fin.Succ(unit) : Refused(key);

    static Fin<Unit> Refused(string key) =>
        Fin.Fail<Unit>(new ChartFault.VisualDegenerate($"navigate/{key}: argument is out of range"));
}

// One verb's whole projection: the metric dimension, the admission verdict, and the navigator call. The apply
// column is an `Action<Navigator>` rather than seven `fun(() => …)()` thunks that existed only to give a void
// call an expression shape inside a switch.
public readonly record struct MapMove(string Key, Fin<Unit> Admitted, Action<Navigator> Apply);

// The observation vocabulary. Three arity twins once spelled one instrument write each and no fold called any
// of them; one entry over a closed fact family is reachable from every fold that holds a fact.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BasemapFact {
    private BasemapFact() { }
    public sealed record Swapped : BasemapFact;
    public sealed record Navigated(MapNav Verb) : BasemapFact;
    public sealed record Health(TileHealth Row) : BasemapFact;
}

// The seat cell's state. The transition ANSWERS the map it retired, which is the whole reason the cell holds a
// pair: a bare `Atom<Map>` commits the swap and loses the handle the caller must dispose, and a read-then-write
// on `Control.Map` let two concurrent rebuilds each believe they had retired the other's map.
public readonly record struct MapSeat(Option<Map> Live, Option<Map> Retired) {
    public static readonly MapSeat Empty = new(None, None);
}

// --- [SERVICES] -------------------------------------------------------------------------
public sealed partial class BasemapSurface {
    // Health arrives on host event handlers that must not block, so the pump drops the OLDEST transition under
    // a burst — NAMED LOSS: a consumer reading a saturated stream sees the settled state and not every
    // intermediate one, which is the reading a surface renders anyway.
    const int HealthDepth = 64;

    readonly Atom<MapSeat> seat = Atom(MapSeat.Empty);

    public BasemapSurface(MapControl control, InstrumentSet instruments) =>
        (Control, Instruments) = (control, instruments);

    public MapControl Control { get; }

    public InstrumentSet Instruments { get; }

    // --- [OPERATIONS]
    // Attribution admission runs BEFORE anything is staged, because a licence refusal is a property of the
    // declaration rather than of the mount, and refusing after opening HTTP clients is work done to reach a
    // verdict the row set already carried.
    public Fin<Map> Build(Seq<BasemapLayerRow> rows, ChartInk ink, ResolvedTheme theme) =>
        Credited(rows).Bind(_ => Candidate(rows, theme)).Bind(candidate => Seat(candidate, ink));

    // The licence gate. Every tile row draws another party's imagery, so its source must carry the credit that
    // imagery's terms require; a row whose credit is blank refuses by name here rather than shipping a map that
    // violates the terms it renders under.
    static Fin<Unit> Credited(Seq<BasemapLayerRow> rows) =>
        rows.Choose(static row => row is BasemapLayerRow.Tile tile ? Some(tile) : None)
            .Traverse(static tile => string.IsNullOrWhiteSpace(tile.Source.Credit.Text)
                ? Fin.Fail<Unit>(new ChartFault.LayerRejected($"{tile.Key}: tile source declares no attribution"))
                : Fin.Succ(unit))
            .As()
            .Map(static _ => unit);

    // The candidate is an acquire chain whose SUCCESS value takes ownership, so custody is `Rollback` and never
    // `Bracket`: bracketing releases on success too and would dispose the map the seat is about to mount. The
    // failure arm now releases in ONE place instead of two hand-written `Fail:` arms an escaping widget-mint
    // throw could sail past.
    static Fin<Map> Candidate(Seq<BasemapLayerRow> rows, ResolvedTheme theme) {
        Map candidate = new();
        return rows.Fold(Fin.Succ(candidate), static (rail, row) => rail.Bind(map => Mount(map, row)))
            .Bind(map => Background(map, theme))
            .Rollback(candidate);
    }

    // Generated total Switch over the closed family — a new `BasemapLayerRow` case breaks THIS dispatch at
    // compile time, and it is the ONLY dispatch over the family: the row-side `Layer()` twin carried three arms
    // no caller could reach, including a chrome refusal nothing could observe.
    static Fin<Map> Mount(Map map, BasemapLayerRow row) => row.Switch(
        state: map,
        tile: static (m, t) => Op.Of(name: "appui.basemap.tile").Catch(() => Fin.Succ((ILayer)new TileLayer(
                    t.Source.Source(t.Policy), minTiles: t.Policy.MinTiles, maxTiles: t.Policy.MaxTiles) { Name = t.Key }))
            .Map(layer => { m.Layers.Add(layer); return m; }),
        features: static (m, f) => GeoOverlay.Layer(f).Map(layer => { m.Layers.Add(layer); return m; }),
        widget: static (m, w) => Widgeted(m, w.Row.Key, () => m.Widgets.Add(w.Row.Mint(m))));

    // A widget mint is an unconstrained composition delegate that does real work, so an escaping throw would
    // carry the staged candidate PAST the rollback that releases it. The capture is the row's own typed refusal.
    static Fin<Map> Widgeted(Map map, string key, Action mount) =>
        Op.Of(name: "appui.basemap.widget").Catch(() => { mount(); return Fin.Succ(map); });

    // The backdrop is the resolved panel token rather than the package default white, so the gap around a
    // partially fetched world reads as the product's own surface under either variant.
    static Fin<Map> Background(Map map, ResolvedTheme theme) =>
        theme.Paint(PaintRole.Surface, rung: 0)
            .ToFin((Error)new ChartFault.PaintUnresolved($"{PaintRole.Surface.Key}+0"))
            .Map(token => { map.BackColor = Mapsui.Styles.Color.FromArgb(token.A, token.R, token.G, token.B); return map; });

    // The one swap. `Cell.Commit` compares by snapshot, so a concurrent rebuild that lost the compare gets a
    // `Contended` verdict and releases the map IT minted — the leak a read-then-write on `Control.Map` produced.
    Fin<Map> Seat(Map candidate, ChartInk ink) =>
        Cell.Commit(seat, held => new MapSeat(Some(candidate), held.Live)) switch {
            Transition<MapSeat>.Committed committed => Landed(committed.State, candidate, ink),
            Transition<MapSeat> contended => Released(candidate, contended),
        };

    Fin<Map> Landed(MapSeat settled, Map candidate, ChartInk ink) {
        Control.Map = candidate;
        Chrome(candidate, ink);
        Control.RefreshGraphics();
        settled.Retired.Iter(static retired => retired.Dispose());
        return Observe(new BasemapFact.Swapped()).Map(_ => candidate);
    }

    static Fin<Map> Released(Map candidate, Transition<MapSeat> declined) {
        candidate.Dispose();
        return Fin.Fail<Map>(declined is Transition<MapSeat>.Refused refused
            ? refused.Cause
            : new ChartFault.LayerRejected("basemap: a concurrent rebuild holds the seat"));
    }

    // The chrome swap walks `GetWidgetsOfMapAndLayers()` rather than `Map.Widgets`, so every tile layer's own
    // attribution credit re-tints beside the mounted chrome and a dark variant does not leave the one legally
    // required caption unreadable; the row lookup is by widget TYPE because the layer mints its credit widget
    // itself and no row ever held it.
    public Fin<Unit> Retint(ChartInk ink, ResolvedTheme theme) =>
        Background(Control.Map, theme).Map(map => { Chrome(map, ink); Control.RefreshGraphics(); return unit; });

    static void Chrome(Map map, ChartInk ink) =>
        toSeq(map.GetWidgetsOfMapAndLayers()).Iter(widget =>
            toSeq(WidgetRow.Items).Find(row => row.Shape == widget.GetType())
                .Match(Some: row => row.Retint(widget, ink), None: () => WidgetRow.Text(widget, ink)));

    // Every camera move reads its verb's ONE projection and answers the SAME `Fin` rail every sibling entry on
    // this page answers; the capture converts a navigator throw into the page's typed refusal on that same rail,
    // so one `Bind` chain carries a rebuild and the camera move that follows it.
    public IO<Fin<Unit>> Navigate(MapNav verb) =>
        IO.lift(() => verb.Move switch {
            var move => move.Admitted
                .Bind(_ => Op.Of(name: "appui.basemap.navigate").Catch(() => { move.Apply(Control.Map.Navigator); return Fin.Succ(unit); }))
                .Bind(_ => Observe(new BasemapFact.Navigated(verb))),
        });

    // Tile health is a SUBSCRIPTION over signals the package already publishes — `Busy` transitions arrive on
    // each layer's `PropertyChanged` and a fetch outcome on `DataChanged` carrying the exception or its absence.
    // The stream is the carrier because a fire-and-forget `Action<TileHealth>` no rail carried could compose
    // with neither the retriability column nor the instrument write; the handler detach rides the iterator's own
    // `finally`, which is the named statement carve-out for a host event subscription.
    public async IAsyncEnumerable<TileHealth> Watch([EnumeratorCancellation] CancellationToken token = default) {
        Channel<TileHealth> pump = Channel.CreateBounded<TileHealth>(
            new BoundedChannelOptions(HealthDepth) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
        Seq<ILayer> layers = toSeq(Control.Map.Layers).Filter(static layer => layer is TileLayer).Strict();
        Seq<IDisposable> handles = layers.Map(layer => Subscribe(layer, pump.Writer)).Strict();
        try {
            layers.Iter(layer => ignore(pump.Writer.TryWrite(TileHealth.Of(layer, None))));
            await foreach (TileHealth health in pump.Reader.ReadAllAsync(token)) {
                ignore(Observe(new BasemapFact.Health(health)));
                yield return health;
            }
        }
        finally {
            handles.Iter(static handle => handle.Dispose());
            pump.Writer.TryComplete();
        }
    }

    static IDisposable Subscribe(ILayer layer, ChannelWriter<TileHealth> sink) {
        void OnData(object? _, DataChangedEventArgs args) =>
            ignore(sink.TryWrite(TileHealth.Of(layer, Optional(args.Error))));
        void OnBusy(object? _, PropertyChangedEventArgs args) {
            if (StringComparer.Ordinal.Equals(args.PropertyName, nameof(ILayer.Busy))) {
                ignore(sink.TryWrite(TileHealth.Of(layer, None)));
            }
        }
        layer.DataChanged += OnData;
        layer.PropertyChanged += OnBusy;
        return Disposable.Create(() => {
            layer.DataChanged -= OnData;
            layer.PropertyChanged -= OnBusy;
        });
    }

    // --- [COMPOSITION]
    public static readonly InstrumentSpec Layers = InstrumentSpec.Create(
        "rasm.appui.basemap.layers", InstrumentKind.Count, MeasureForm.Whole, "{rebuild}",
        "layer-set rebuilds seated onto the mounted map", Seq<string>(), None, None, None);

    public static readonly InstrumentSpec Navigated = InstrumentSpec.Create(
        "rasm.appui.basemap.navigated", InstrumentKind.Count, MeasureForm.Whole, "{navigation}",
        "camera moves by verb case", Seq(AppUiTelemetry.IntentSlot), None, None, None);

    // The row stem `TileHealth` collides with the health type this class reads, so the name takes the metric's
    // last dotted segment.
    public static readonly InstrumentSpec Health = InstrumentSpec.Create(
        "rasm.appui.basemap.tile.health", InstrumentKind.Count, MeasureForm.Whole, "{transition}",
        "tile fetch-state transitions by state and retriability",
        Seq(AppUiTelemetry.OutcomeSlot, AppUiTelemetry.CauseSlot), None, None, None);

    // Every projection binds at the fold that already holds the fact, and each breakdown rides a DECLARED
    // dimension materialized through the one tag entry: neither a `KeyValuePair` nor a pre-built tag list
    // converts to the pair element the write consumes, so a fact spelled either way reaches no write at all.
    public Fin<Unit> Observe(BasemapFact fact) => fact.Switch(
        state: Instruments,
        swapped: static (set, _) => set.Write(Layers, 1d),
        navigated: static (set, row) => set.Write(Navigated, 1d,
            InstrumentSet.Tags((AppUiTelemetry.IntentSlot, row.Verb.Move.Key))),
        health: static (set, row) => set.Write(Health, 1d, InstrumentSet.Tags(
            (AppUiTelemetry.OutcomeSlot, row.Row.State.Key),
            (AppUiTelemetry.CauseSlot, row.Row.State.Recovery.Key))));

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Layers, Navigated, Health);
}
```

## [03]-[NTS_OVERLAY]

- Owner: `GeoOverlayRow` — the per-feature row carrying the Bim-owned `GeoFeature` WHOLE (geometry, attribute table, declared `SourceCrs`) plus its display label and its source-layer key; `FeatureSource` `[Union]` — the two feature ingresses one layer mount reads, the residency-decorator election riding the arm; `GeoTileProvider` — the Bim MVT pyramid as one ordinary `IProvider` over a bounded channel pump under one kernel `RedrivePolicy`; `MapFrame` `[SmartEnum<string>]` — the two CRS frames this plane crosses between, each carrying its SRID beside its authority text; `MercatorFilter` — the ONE parameterized coordinate-sequence filter and the ONE reprojection both directions and all four crossings take; `GeoOverlay` — the layer mount and the CRS ingress gate.
- Cases: `FeatureSource` = Resident · Pyramid — a resident set decorates, a pyramid declines the decorators because its own LOD levels already clipped and decimated; `MapFrame` = wgs-84 (4326) · web-mercator (3857).
- Entry: `public static Fin<ILayer> Layer(BasemapLayerRow.Features row)` — ONE mount over both ingresses, the source arm electing the decorator chain; `public Fin<Geometry> Reproject(Geometry geometry)` on `MercatorFilter` — the one copy-apply-stamp crossing four sites once re-spelled statement for statement; `internal static Fin<GeometryFeature> Project(GeoOverlayRow row)` — the CRS ingress gate every resident and decoded row crosses.
- Auto: features ARRIVE as Bim-owned `GeoFeature` rows carrying their `GeoReference` lineage (the `GeoReferenceProjector` IfcMapConversion/IfcProjectedCRS lowering) already reprojected to WGS-84 by Bim's `GeoFeature.Reproject` — the declared seam, both sides (`Rasm.Bim` Semantics/feature -> AppUi Charts) — so the row's `SourceCrs`/SRID state IS the CRS evidence the gate reads; AppUi's ONLY reprojection is WGS-84 lon/lat -> EPSG:3857 through `SphericalMercator.FromLonLat` under `ProjectionDefaults.Projection` at the layer-build edge, spelled once as `MercatorFilter.Forward.Reproject`. The pyramid pump is a bounded producer/consumer seam: the covering tile set fills a bounded `Channel<TileInfo>` that a fixed lane count drains, so a viewport covering a hundred tiles opens a bounded number of transports instead of the unbounded `Task.WhenAll` fan the prior shape opened, and each lane's fetch re-drives under the kernel `RedrivePolicy` for exactly the transient failures `TileState.Posture` admits. A REFUSED TILE IS EVIDENCE: its typed fault parks on the composition-supplied `FaultCell` carrying the tile address and its retriability, so a hole in the draw is a recorded fact rather than the silent empty array the prior `Fail:` arm answered on a page whose whole law is that tile state is a rendered fact.
- Receipt: an overlay row whose feature still declares a projected frame (or a non-4326 SRID) folds to `ChartFault.CrsUnresolved` — the ingress law enforced as a typed fault, never a silent draw at wrong coordinates; a refused tile parks one `ChartFault.LayerRejected` per tile address on the fault cell.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, BruTile, NetTopologySuite, Rasm.Bim (project), Rasm (project — `RedrivePolicy`, `FaultCell`, `Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Threading.Channels`)
- Growth: a new feature family is a seq of rows on one `Features` layer row, and attribute-driven symbology within a family is one `Symbology.Themed` selector reading the consumed `GeoFeature` attribute table — never a layer per attribute value; a new tiled vector source is one `FeatureSource.Pyramid` value naming its schema and its fetch; a new graduated thematic is one `Colormap` row and one `Symbology.Graduated` call; a new CRS frame is one `MapFrame` row and one `MercatorFilter` value; zero new surface.
- Boundary: a resident set mounts as `Layer` over the `IndexedMemoryProvider` → `GeometryIntersectionProvider` → `GeometrySimplifyProvider` decorator chain under one `ThemeStyle`, so viewport clipping, resolution-driven decimation, and per-feature symbology are all provider and style policy — a hand-rolled cull, a resolution branch at the bind edge, and a `MemoryLayer` holding every vertex resident at every zoom are the deleted forms; geodesy stays Bim's — `GeoFeature.Reproject`, datum transforms, and projected-CRS handling never re-implement here, and on the MAP-VIEWPORT CRS plane a local geodesy kernel (a proj4 port, a datum table, a second `SphericalMercator` beside the Mapsui primitive) is the FORBIDDEN form. `[NOT]` that plane: `Charts/custom#SKIA_KINDS` `GeoProjection` answers pixels of one `SKImageInfo` extent and carries no CRS, no datum, and no map engine, so the forbidden form here is a second CRS owner, never every mercator expression in the package — but the web-mercator POLE CLAMP is not a plane, it is this projection's own asymptote, so `MercatorFilter.PoleClampDeg` is its single owner and a transcribed literal beside it is the deleted form. NTS geometry types cross the seam as values inside Bim-owned features and are wrapped, never re-modeled. The Bim MVT pyramid is a `FeatureSource.Pyramid`, never a tile row: `GeoModel.ToTiles` emits the pyramid, `GeoTiles.Catalog` serves the TileJSON the source discovers its template and zoom band through, and `Rasm.Bim.GeoTiles.Decode` answers each tile's own `(layer, GeoFeature)` rows, every one crossing the SAME `Project` gate a resident row crosses — so a tiled site model and a resident parcel set are one draw path under one CRS law. `TileLayer` structurally cannot carry it: its fetch hook answers ONE `IFeature` per tile and its constructor seats a `RasterStyle`, so a tile decoding to a feature SET would collapse into a single geometry and lose every attribute the symbology selector reads, which is exactly the reading a vector tile exists to carry.
- Boundary: the pyramid's re-drive predicate reads this page's `TileState.Posture`; the direct `ChartFault` family publishes no retriability override. `IProvider.GetFeaturesAsync` has no cancellation token, so the channel seam remains internal.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// The two CRS frames this plane crosses between, each carrying its SRID beside the authority text a provider
// declares. Six bare integer literals and one settable CRS string once spelled the same two facts across three
// sites; folder RULINGS `[02]` rules a registry-resolved literal a DECLARED constant on its owning surface.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapFrame {
    public static readonly MapFrame Wgs84 = new("wgs-84", srid: 4326, "EPSG:4326");
    public static readonly MapFrame WebMercator = new("web-mercator", srid: 3857, "EPSG:3857");

    public int Srid { get; }

    public string Authority { get; }
}

// The two feature ingresses one layer mount reads. NAMED LOSS on the collapse of the prior `Overlay`/`Vectors`
// case pair: the residency-decorator election stopped being implicit in a case name and became this arm's own
// column — a pyramid declines the chain because its LOD levels already clipped and decimated, and a resident
// set takes it because its features are all resident at every zoom.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FeatureSource {
    private FeatureSource() { }
    public sealed record Resident(Seq<GeoOverlayRow> Rows) : FeatureSource;
    public sealed record Pyramid(GeoTileProvider Tiles) : FeatureSource;
}

// --- [MODELS] ---------------------------------------------------------------------------
// The ingress row carries the Bim-owned GeoFeature WHOLE — geometry + attribute table + declared SourceCrs —
// so CRS authority is the seam contract the feature itself carries, never an SRID sniff on a bare geometry;
// Label is the display column minted beside the consumed feature and Source is the seam's own layer key, which
// the MVT codec threads per row and the symbology selector reads to tell a road from a parcel. A resident row
// carries no source and a decoded one carries the layer it arrived in, so one row shape serves both ingresses.
public sealed record GeoOverlayRow(
    string FeatureId,
    Rasm.Bim.GeoFeature Feature,
    Option<string> Label,
    Option<string> Source);

// --- [SERVICES] -------------------------------------------------------------------------
// The Bim MVT pyramid as an ordinary provider. `GetFeaturesAsync` resolves the tiles covering the fetch extent
// at the fetch's OWN resolution through the schema's `GetTileInfos`, so LOD selection is the schema's arithmetic
// rather than a zoom branch here. The fetch delegate is composition-bound because the transport is the caller's
// (an HTTP template off `GeoTiles.Catalog`, a local pyramid, a cache) and the decode is the seam codec's, so
// this owner holds neither a protobuf reader nor an HTTP client of its own — and because the transport IS the
// caller's, the kernel re-drive rides HERE, where the package's own `TileFetchPlanner` never reaches.
public sealed class GeoTileProvider(
    ITileSchema schema,
    Func<TileInfo, CancellationToken, Task<ReadOnlyMemory<byte>>> fetch,
    RedrivePolicy redrive,
    FaultCell faults) : IProvider {
    // The transport lane count: a viewport can cover a hundred tiles and the prior unbounded fan opened one
    // request per tile at once, which a public pyramid endpoint answers by throttling every one of them.
    const int Lanes = 4;

    static readonly HookId TileHook = HookId.Of("appui.basemap.pyramid");

    public string? CRS { get; set; } = MapFrame.WebMercator.Authority;

    public MRect? GetExtent() => schema.Extent.ToMRect();

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo info) {
        Channel<TileInfo> pending = Channel.CreateBounded<TileInfo>(
            new BoundedChannelOptions(Lanes) { SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
        Channel<IFeature> drawn = Channel.CreateUnbounded<IFeature>(new UnboundedChannelOptions { SingleReader = true });
        Task filled = Fill(pending.Writer, schema.GetTileInfos(info.Extent.ToExtent(), info.Resolution));
        Task[] lanes = [.. Enumerable.Range(0, Lanes).Select(_ => Drain(pending.Reader, drawn.Writer))];
        await filled;
        await Task.WhenAll(lanes);
        drawn.Writer.Complete();
        List<IFeature> features = [];
        await foreach (IFeature feature in drawn.Reader.ReadAllAsync()) { features.Add(feature); }
        return features;
    }

    static async Task Fill(ChannelWriter<TileInfo> pending, IEnumerable<TileInfo> covering) {
        foreach (TileInfo tile in covering) { await pending.WriteAsync(tile); }
        pending.Complete();
    }

    // A tile that refuses contributes NOTHING to the draw but everything to the evidence: a pyramid is a set of
    // independent tiles and one refused tile is a hole, while a failed fetch is a blank map — but the prior
    // shape swallowed the refusal entirely, so a hole and a healthy sparse region read identically. The park
    // carries the tile address and the retriability its classification implies.
    async Task Drain(ChannelReader<TileInfo> pending, ChannelWriter<IFeature> drawn) {
        await foreach (TileInfo tile in pending.ReadAllAsync()) {
            Fin<Seq<GeometryFeature>> decoded = await Fetched(tile).RunAsync();
            decoded.Match(
                Succ: rows => rows.Iter(row => ignore(drawn.TryWrite(row))),
                Fail: error => ignore(faults.Park(TileHook, error)));
        }
    }

    // The re-drive rides the kernel policy's derived curve, so the bound cannot drift from the law beside it,
    // and the predicate is the ONE transport classifier the rendered health arm also reads.
    IO<Seq<GeometryFeature>> Fetched(TileInfo tile) =>
        IO.liftAsync(async () => await fetch(tile, CancellationToken.None))
            .Bind(bytes => IO.lift(() => Decoded(tile, bytes)))
            .RetryWhile(schedule: redrive.Curve, predicate: static error => TileState.Posture(error) is Retriability.TransientCase);

    // The feature id is the tile address plus the ordinal within it, so a pick answers WHICH tile a feature came
    // from and two adjacent tiles cannot collide on an id the seam never promised to make unique.
    Fin<Seq<GeometryFeature>> Decoded(TileInfo tile, ReadOnlyMemory<byte> bytes) =>
        Rasm.Bim.GeoTiles
            .Decode(bytes, tile.Index.Col, tile.Index.Row, tile.Index.Level, Op.Of(name: "basemap-mvt"))
            .Map(rows => rows.Map((row, ordinal) => new GeoOverlayRow(
                $"{tile.Index.Level}/{tile.Index.Col}/{tile.Index.Row}/{ordinal}", row.Feature, None, Some(row.Layer))))
            .Bind(static rows => rows.Traverse(GeoOverlay.Project).As());
}

// ONE parameterized coordinate-sequence filter over the Mapsui projection primitive: `Forward` lifts WGS-84
// into EPSG:3857 at layer build, `Inverse` returns authored view geometry to WGS-84 at commit, and the target
// frame is a COLUMN, so the SRID stamp travels with the direction and a direction-named sibling filter class is
// the deleted form.
public sealed class MercatorFilter(Func<double, double, (double X, double Y)> project, MapFrame target)
    : NetTopologySuite.Geometries.ICoordinateSequenceFilter {
    public static readonly MercatorFilter Forward =
        new(static (x, y) => SphericalMercator.FromLonLat(x, y), MapFrame.WebMercator);
    public static readonly MercatorFilter Inverse =
        new(static (x, y) => SphericalMercator.ToLonLat(x, y) switch { var (lon, lat) => (Principal(lon), lat) },
            MapFrame.Wgs84);

    // The latitude the web-mercator projection clamps at, owned HERE for the whole branch. Beyond it the
    // projection is asymptotic, so a world ordinate outside the clamp answers a coordinate that never had
    // ground — the band a pan past the top of the world exposes — and the admission gate refuses it.
    public const double PoleClampDeg = 85.05112878d;

    public MapFrame Target { get; } = target;

    // The ONE crossing. Four sites once re-spelled copy, apply, and stamp statement for statement, so a repair
    // to any of them repaired one quarter of the law.
    public Fin<NetTopologySuite.Geometries.Geometry> Reproject(NetTopologySuite.Geometries.Geometry geometry) =>
        Op.Of(name: "appui.basemap.reproject").Catch(() => {
            NetTopologySuite.Geometries.Geometry crossed = geometry.Copy();
            crossed.Apply(this);
            crossed.SRID = Target.Srid;
            return Fin.Succ(crossed);
        });

    // Longitude folds onto its principal range because web mercator is CYLINDRICAL: a map panned past the
    // antimeridian keeps producing world x beyond the seam, so an unwrapped inverse commits a longitude of two
    // hundred degrees that no WGS-84 consumer admits and that re-renders a world away from the place the
    // operator drew on. The fold rides the INVERSE ROW itself, so every crossing carries it.
    static double Principal(double lon) => lon - (360d * Math.Floor((lon + 180d) / 360d));

    public bool Done => false;
    public bool GeometryChanged => true;

    public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i) {
        (double x, double y) = project(seq.GetX(i), seq.GetY(i));
        seq.SetX(i, x);
        seq.SetY(i, y);
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class GeoOverlay {
    // ONE mount over both ingresses. The residency law of the 2D plane is the three admitted provider
    // decorators rather than a hand-rolled cull: the indexed provider replaces the linear feature array so a
    // fetch is an envelope query, the intersection provider clips each fetch to the viewport `MRect`, and the
    // simplify provider decimates by the LIVE fetch resolution (a null tolerance drives it off
    // `fetchInfo.Resolution`). `MemoryLayer.Features` holding every vertex of every family at every zoom is the
    // deleted form — the same budget law `Render/meshlets` `ResidencyBudget` enforces for geometry VRAM.
    public static Fin<ILayer> Layer(BasemapLayerRow.Features row) =>
        // The mount entry and the package's layer type share the simple name `Layer`, so the construction is
        // namespace-qualified: an unqualified `new Layer(...)` inside a class declaring this member resolves the
        // member and refuses to compile.
        Source(row.Source).Bind(source => Op.Of(name: "appui.basemap.feature-layer").Catch(() => Fin.Succ((ILayer)new Mapsui.Layers.Layer(row.Key) {
                DataSource = source,
                Style = new ThemeStyle(row.Symbology.Select),
                MinVisible = row.MinVisible,
                MaxVisible = row.MaxVisible,
            })));

    static Fin<IProvider> Source(FeatureSource source) => source.Switch(
        resident: static row => row.Rows
            .Traverse(Project)
            .Map(static features => (IProvider)new GeometrySimplifyProvider(
                new GeometryIntersectionProvider(new IndexedMemoryProvider(features.ToArray()))))
            .As(),
        pyramid: static row => Fin.Succ((IProvider)row.Tiles));

    // The ingress gate: a feature is admissible only when its OWN declared frame is the WGS-84 baseline the seam
    // promises (post-Reproject: no residual projected frame, SRID 4326 exactly — an unstamped SRID 0 is
    // un-reprojected input and faults) — a residual SourceCrs is the typed CrsUnresolved fault, never a silent
    // draw at wrong coordinates; the transformed copy re-stamps the forward frame so downstream reads the frame
    // it actually holds, never the source frame.
    internal static Fin<GeometryFeature> Project(GeoOverlayRow row) =>
        row.Feature.SourceCrs.IsNone && row.Feature.Geometry.SRID == MapFrame.Wgs84.Srid
            ? MercatorFilter.Forward.Reproject(row.Feature.Geometry).Map(mercator => Stamped(mercator, row))
            : Fin.Fail<GeometryFeature>(new ChartFault.CrsUnresolved(row.FeatureId, row.Feature.Geometry.SRID));

    static GeometryFeature Stamped(NetTopologySuite.Geometries.Geometry mercator, GeoOverlayRow row) {
        GeometryFeature feature = new(mercator);
        feature[FeatureSlot.Id] = row.FeatureId;
        row.Label.Iter(label => feature[FeatureSlot.Label] = label);
        row.Source.Iter(source => feature[FeatureSlot.Source] = source);
        return feature;
    }
}
```

## [04]-[PICK_AND_SNAPSHOT]

- Owner: `PickPolicy` `[ComplexValueObject]` — the admitted tolerance-and-arity posture a pick resolves under; `BasemapPickReceipt` with `BasemapHover` — the page-owned scalar results; the pick, hover, and snapshot folds are members of `BasemapSurface`, the map owner that already holds the control every one of them probes.
- Entry: `public Fin<Seq<BasemapPickReceipt>> Pick(ScreenPosition screen, PickPolicy policy)` — `GetMapInfo(screen, layers)` resolves the hit ROSTER at the screen point, tolerance widens the probe to the finger-and-pointer band, and the kernel bounded-selection cell answers the nearest rows; `public Option<BasemapHover> Hover(ScreenPosition screen, PickPolicy policy)` — the same fold at hover grain, answering the top hit and the ambiguity count; `public IO<RenderReceipt> Snapshot(VisualRuntime runtime, string key)` — the encoded basemap bytes re-sealed through the one capture codec under this page's own `ArtifactKind` row.
- Auto: a pick lands in the same selection vocabulary the viewport pick uses — the selection owner receives scalar receipts and never a Mapsui type. TOLERANCE is a screen-space band the policy declares and the fold resolves into world units through the hit's own `MapInfo.Resolution`, because a pointer lands within a few pixels of a hairline utility run rather than on it and a zero-tolerance probe makes thin geometry unselectable at every zoom. MULTI-HIT is the `MapInfo.MapInfoRecords` roster rather than the convenience `Feature` alone, and the ranking is the kernel `Ranked.Top` bounded selection at O(n log k) rather than a full sort of every hit under the pointer: the branch's ONE top-K law reads here exactly as it reads for a spatial walk or a corpus scan, and the `double.MaxValue` sentinel that once ordered an absent distance is the exact mis-spelling of `Ranked.Bound` the kernel row names. AN UNMEASURABLE HIT IS PARTITIONED, not sentinel-ordered: features carrying NTS geometry rank by distance and features the renderer hit whose payload carries none append at the tail, so "unranked" is structure rather than a magic number, and the arity bound applies to the concatenation. `MapControl.GetSnapshot(layers, RenderFormat.Png, quality)` yields the encoded basemap bytes that decode through `VisualCodec.Decode` and re-encode through `VisualCodec.Encode` under `BasemapSurface.Artifact`, so a basemap baseline rides the same render-hash proof lanes as every visual.
- Receipt: one `RenderReceipt` of kind `basemap` per snapshot, keyed over the encoded payload inside `RenderReceipt.Of` and projected onto the evidence plane through the generated `EvidenceMap.ToEvidence(RenderReceipt)` seam by the runtime's own `Sink`.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, NetTopologySuite, SkiaSharp, Rasm (project — `Ranked.Top`, `ExtremumDirection`, `ContentHash` through the codec), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new pick projection is one attribute read on the fold; a retuned tolerance or arity is one `PickPolicy` row; a new capture kind is one `ArtifactKind` row on its owning surface; zero new surface.
- Boundary: no Mapsui type crosses out of this page — picks project to scalar receipts and snapshots cross as encoded bytes through the capture codec. Distance ranking reads the ADMITTED geometry engine's own `Geometry.Distance` against the pick point rather than a centroid or bounding-envelope proxy, because a long utility run's centroid can sit kilometres from the vertex under the pointer and a bounding-envelope test makes every large parcel win every contest. `[NOT]` the indexed-locator ruling: folder RULINGS `[02]` binds POINT-IN-AREA CONTAINMENT to `IndexedPointInAreaLocator`, and this is a nearest-distance rank over a renderer-narrowed hit roster where each candidate geometry differs and the point is fixed — `PreparedGeometryFactory.Prepare` amortizes ONE fixed geometry across many candidates, which is the inverse shape, so preparing per candidate would build an index used exactly once. Tolerance is declared in SCREEN pixels and converted at the fold — a tolerance carried in world units would tighten as the viewer zoomed in, which is the opposite of what a pointer needs. The layer name is `Option<string>` on the receipt because a hit whose record carries no layer is a different fact from a hit on a layer named the empty string, and the prior `?? string.Empty` erased the first into the second.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
// The pick posture. `TolerancePx` is the screen band a probe widens to and `Arity` bounds the roster a dense
// stack answers; both are admitted at declaration so an inadmissible posture is unspellable rather than refused
// at the one call the prior `Admit()` guarded. The touch row bounds a LARGER roster than the pointer row
// because a 16px band admits more overlapping features than an 8px one — an arity identical across two bands
// carried no variation the roster expressed.
[ComplexValueObject]
[ValidationError]
public sealed partial class PickPolicy {
    public static readonly PickPolicy Pointer = Create(TolerancePx: 8d, Arity: 8);
    public static readonly PickPolicy Touch = Create(TolerancePx: 16d, Arity: 12);

    public double TolerancePx { get; }
    public int Arity { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double tolerancePx, ref int arity) =>
        validationError = double.IsFinite(tolerancePx) && tolerancePx >= 0d && arity > 0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { "pick: tolerance must be finite and non-negative, arity positive" }));
}

// The page-owned pick receipt — scalars only; a Mapsui MPoint or MapInfo never crosses out of this page.
// `Distance` is the world-unit separation between the pick point and the feature, carried because a caller
// disambiguating a stack needs the ranking evidence rather than only the order it happened to arrive in; it is
// ABSENT for a feature the renderer hit that carries no measurable geometry, which is a different fact from a
// distance of zero and reads as one.
public readonly record struct BasemapPickReceipt(
    string FeatureId, Option<string> Layer, double WorldX, double WorldY, Option<double> Distance);

// Hover answers the top hit and how many hits share the point, so a surface can state "3 features here" before
// the click; a hover that answered only the top hit made an ambiguous stack indistinguishable from an
// unambiguous one and every disambiguation a surprise.
public readonly record struct BasemapHover(BasemapPickReceipt Top, int Ambiguity);

// --- [OPERATIONS] -----------------------------------------------------------------------
// The pick and snapshot folds are members of the map owner: as free statics they addressed a `MapControl` the
// surface already holds, and nothing in the package reached them.
public sealed partial class BasemapSurface {
    // This page's row in the OPEN artifact-kind space `Render/capture#ENCODE_IDENTITY` declares — the kind
    // roster spans six owners, so each declares its own address rather than the codec re-declaring all of them.
    public static readonly ArtifactKind Artifact = ArtifactKind.Create("basemap");

    // The pick fold: probe, rank, bound. `GetMapInfo` answers the whole `MapInfoRecords` roster and the
    // convenience `Feature` is only its head, so reading that head alone discarded every feature under the top
    // one — which on an overlay plane is the normal case, not the exception. Tolerance converts through the
    // hit's OWN `MapInfo.Resolution` (world units per pixel at the moment of the probe), so one declared pixel
    // band means the same thing at every zoom.
    public Fin<Seq<BasemapPickReceipt>> Pick(ScreenPosition screen, PickPolicy policy) =>
        Optional(Control.GetMapInfo(screen, Control.Map.Layers))
            .Bind(info => Optional(info.WorldPosition).Map(world => (Info: info, World: world)))
            .Match(
                Some: hit => Fin.Succ(Nearest(hit.Info, hit.World, policy)),
                None: static () => Fin.Succ(Seq<BasemapPickReceipt>()));

    public Option<BasemapHover> Hover(ScreenPosition screen, PickPolicy policy) =>
        Pick(screen, policy).ToOption()
            .Bind(hits => hits.Head.Map(top => new BasemapHover(top, hits.Count - 1)));

    // Ranking is the kernel's ONE bounded top-K selection, nearest-first from the pick point. The measurable and
    // unmeasurable halves PARTITION rather than share one comparison: a feature the renderer hit whose payload
    // carries no NTS geometry ranks after every measured one, because the render and the pick disagreeing about
    // what exists at a point is a worse failure than an unranked entry at the tail — and expressing that as a
    // `double.MaxValue` key made the tail an ordering fact instead of a structural one.
    static Seq<BasemapPickReceipt> Nearest(MapInfo info, MPoint world, PickPolicy policy) {
        NetTopologySuite.Geometries.Point at = new(world.X, world.Y);
        double tolerance = policy.TolerancePx * info.Resolution;
        Seq<BasemapPickReceipt> hits = toSeq(info.MapInfoRecords)
            .Choose(record => Optional(record.Feature)
                .Bind(feature => Optional(feature[FeatureSlot.Id] as string)
                    .Map(id => new BasemapPickReceipt(
                        id, Optional(record.Layer).Bind(layer => Optional(layer.Name)),
                        world.X, world.Y, Separation(feature, at)))))
            // An absent distance PASSES the band: the renderer hit that feature, and a pick discarding a hit the
            // render produced makes the two surfaces disagree about what exists at a point.
            .Filter(row => row.Distance.ForAll(distance => distance <= tolerance))
            .Strict();
        (Seq<BasemapPickReceipt> Measured, Seq<BasemapPickReceipt> Unmeasured) split =
            hits.Partition(static row => row.Distance.IsSome) switch { var (yes, no) => (toSeq(yes), toSeq(no)) };
        return Ranked
            .Top(split.Measured, policy.Arity, static row => row.Distance.IfNone(0d), ExtremumDirection.Minimum)
            .Concat(split.Unmeasured)
            .Take(policy.Arity)
            .Strict();
    }

    static Option<double> Separation(IFeature feature, NetTopologySuite.Geometries.Point at) =>
        feature is GeometryFeature { Geometry: { } geometry } ? Some(geometry.Distance(at)) : None;

    // The snapshot lane: GetSnapshot's encoded bytes decode and re-seal through the one capture codec so a
    // basemap baseline carries the same content-hashed RenderReceipt evidence as every visual. Release brackets
    // the ACQUISITION — the decoded image is a native the encode may fail against, and disposing it inside a
    // `.Map` on the encode RESULT bound release to success and leaked the image on every refused encode.
    public IO<RenderReceipt> Snapshot(VisualRuntime runtime, string key) =>
        from bytes in IO.lift(() => Control.GetSnapshot(Control.Map.Layers, RenderFormat.Png, quality: 100))
        from receipt in VisualCodec.Decode(bytes).Bracket(
            image => VisualCodec.Encode(runtime, image, VisualCodec.Png, Artifact, key),
            static image => IO.lift(() => { image.Dispose(); return unit; }))
        select receipt;
}
```

## [05]-[REDLINE]

- Owner: `RedlineVerb` `[Union]` — the closed markup-verb vocabulary carrying typed document and target identities; `Collab/issues#REDLINE_TOOLS` `RedlineTool`, `RedlineToolState`, `RedlineStroke`, and `StrokeCapture` arrive settled as the tool and stroke owners; `RedlineKind` — the mark vocabulary carrying its own `EditMode` session column; `RedlineOrigin` `[Union]` — the two authoring ingresses one commit reads; `RedlineSession` — the session-transition owner over the `EditManager` host machine; `RedlineGeometry` — the ONE shape correspondence, both directions on one owner; `RedlineLane` — the gauge roster the commit and replay spans are judged against; `RedlineSurface` — the one authoring owner binding the composition-supplied `EditHistory` recorder; the commit leg projects onto `EditIntent.Annotation` and records one `RevertibleOp` on the same pass, never a basemap-local op union and never a second undo stack.
- Cases: `RedlineVerb` = BeginMark · Modify · Delete · Commit · Discard; `RedlineKind` = Point · Path · Area; `RedlineOrigin` = Session · Stroke, the stroke arm carrying the captured `Viewport` its samples were taken under; `RedlineDelta` = Upsert · Delete; `RedlineShape` = Point · Path · Area · Collection; `RingArity` = path (2) · ring (4) — the two vertex floors three sites once spelled as bare literals; `RedlineLane` = commit · replay.
- Entry: `public WritableLayer Mounted()` — the marks layer resolved once under the shared symbology selector; `public IO<Fin<Option<RedlineCommit>>> Drive(RedlineVerb verb, RevertCursor cursor, MonotonicTimeline line, IClock clock, CorrelationId correlation)` — every markup gesture discriminates on the verb union, the two durable verbs record onto the recorder before answering, and each answer carries the intent, the sealed receipt, the advanced cursor, and the gauged span its seal took; `Commit` emits an upsert annotation and `Delete` emits a delete annotation, while local begin, modify, and discard return `None`; the caller composes one rail and the intent ledger commit stays caller-side (`IntentLedger.Commit(doc, intent, origin)` is `Collab/sync.md`'s one transaction rail).
- Auto: authoring runs on a dedicated redline `WritableLayer` above the feature stack, mounted through `Mounted` under one `ThemeStyle(Symbology.Select)` so a mark colours by its own `IssueStatus` attribute on the same symbology axis every feature row binds. `RedlineSession.Of` is the total session adapter: `BeginMark` writes the kind's own `EditMode` column, `Modify` writes `EditMode.Modify`, a session `Commit` calls `EndEdit()` and reads back the sealed feature while a stroke `Commit` snapshots and closes without it, and `Delete` and `Discard` both `ResetManipulations()` and land `EditMode.None`, `Discard` dropping the in-flight feature through `Layer.TryRemove`. `Sealed` admits the origin, resolves the shape through the one `RedlineGeometry` correspondence, applies `MercatorFilter.Inverse.Reproject` — the same crossing `[03]` owns — and preserves finite points, paths, polygon shells and holes, multi-geometries, and heterogeneous collections before serializing `RedlineDelta.Upsert`; `Delete` needs no surviving feature and serializes `RedlineDelta.Delete`. IDENTITY IS ADMITTED BY TYPE: `DocumentKey` and `ContainerKey` are the settled `Collab/sync` value objects `EditIntent.Annotation` declares, so a blank document or target is unspellable on the verb and the prior per-call identity guard has nothing left to guard. COMMIT READS AN ORIGIN, not a session alone: `RedlineOrigin.Session` seals the vertex-authored feature the manager wrote and carries the caller's declared paint, while `RedlineOrigin.Stroke` carries the `Viewport` the samples were TAKEN under and unprojects the captured pen stroke through that frame's own `ScreenToWorldXY` into the view frame the inverse filter then lifts to WGS-84, and resolves its paint from the stroke's own tool row and mean nib weight — so one commit verb, one durable leg, and one recorder hop serve both ingresses, and an eraser-channel stroke lands `RedlineDelta.Delete` regardless of the selected tool exactly as its capture already decided. Degenerate geometry, a stroke short of two projectable samples, a vertex outside the WGS-84 domain, a removal over a target the layer never held, and an out-of-range width, opacity, or dash interval all fail on the typed rail; paint and fill are `PaintRole` rows, so an unresolvable pigment is unspellable rather than admitted. THE RECORDER HOP: the session snapshots the target's PRIOR payload off the marks layer BEFORE `EndEdit()` seals the stroke, so a commit carries both the payload it replaced and the payload it wrote; absence is `Option<JsonElement>` inside this page and the recorder's own defined JSON null is minted once, at the one edge whose rail demands it. The op records through `EditHistory.Record` with the same apply fold the layer re-renders through, so an undo re-applies the inverse payload onto `Marks` exactly as a committed intent does and no second application law exists.
- Receipt: a committed redline is one `EditIntent.Annotation(DocumentKey, ContainerKey, JsonElement)` row on the single edit-intent union BESIDE one `EditReceipt` the recorder seals and one `GaugedSpan<RedlineLane>` the kernel timeline measured — durable truth rides the Persistence `OpLogEntry` projection per the `[04]-[BOUNDARIES]` Loro-byte clause, and the redline layer re-renders from the committed intent, never from retained authoring state; every gesture folds one `redline.gesture` observation at `Drive` under the outcome slot its spec declares, so commit, discard, and local authoring stay separable series.
- Packages: Mapsui.Avalonia12, Mapsui.Nts, NetTopologySuite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `MonotonicTimeline`, `IGaugeLane`/`GaugedSpan`, `Op`), Rasm.Persistence (project — `Hlc`), BCL inbox
- Growth: a new mark kind is one `RedlineKind` row carrying its `EditMode` column plus its corresponding `RedlineShape` case when the payload arity differs — and the shape case carries its `[JsonDerivedType]` row and its `RingArity` floor by the same edit; a new markup verb is one `RedlineVerb` case; a new gauged concern is one `RedlineLane` row; a new review-state colouring is one branch inside the bound `Symbology` selector, never a second marks layer; zero new surface.
- Boundary: `EditManager` and `EditingWidget` remain inside this section; authored geometry leaves only as the WGS-84 `RedlineDelta` payload, and delete carries no stale geometry. `RedlineSurface` holds a live `EditManager` host machine, so it is a sealed class per folder RULINGS `[02]` and never a record whose copy shares that machine by reference. The marks layer is a `WritableLayer` because that is the type `EditManager.Layer` admits and the one layer shape carrying in-place `Add`/`TryRemove` — the feature provider chain decorates a FETCH and structurally cannot receive an authoring write, so the residency decorators stop at the feature boundary while the symbology selector crosses it; a decorated edit layer, a `MemoryLayer` under an `EditManager`, and a flat per-layer redline style are all deleted forms. Pointer routing is `EditingWidget` on `Map.Widgets` over THIS manager — it forwards press, move, release, and tap into `EditManipulation`, which drives `AddVertex(Coordinate)`, the drag, insert, delete, rotate, and scale trios — so `RedlineVerb` owns SESSION lifecycle alone and a page-local pointer router beside the widget is the deleted form. The payload crosses `System.Text.Json` under `EvidenceOps.Wire`, the composition-seated merged suite options every AppUi persisted payload already shares, so the page-local `JsonSerializerOptions` column that once forked the converter set is deleted; both payload unions carry their `[JsonDerivedType]` roster because `[Union]` generates no JSON support, so a union serialized as its abstract base emits an empty object and an annotation blob that round-trips is a discriminated one. UNDO IS THE ONE REVERT ALGEBRA: a committed mark records onto the `Editing/history` `EditHistory` recorder as a `RevertibleOp` whose delta is the before-and-after annotation payload, so a redline undoes through the same `history.undo` intent every other edit does and a basemap-local mark stack, a re-authoring replay, and a geometry-level inverse are the deleted forms. The recorder's apply fold is the SAME layer write the committed-intent re-render performs, so the two paths cannot drift.
- Boundary: the TOOL is not this page's — `Collab/issues#REDLINE_TOOLS` owns `RedlineToolState` and `StrokeCapture` owns the pressure-weighted fold from raw samples to a stroke, so the mode toolbar over a map and the mode toolbar over a viewpoint drive ONE tool state and a basemap-local pen vocabulary is the deleted form; this section owns the session lifecycle, the layer, and the commit leg alone. PEN input arrives as `Shell/input#POINTER_GESTURES` `PenSample` rows off the one pointer ingress — the whole coalesced burst, because the platform batches every sample it took between two frames and a nib's pressure and tilt live in the ones a per-frame read discards — so `PenAxis.Pressure` has already scaled each `StrokePoint`'s weight and `PenAxis.Eraser` has already routed the stroke before it reaches this page.
- Law: a deferred commit carries the frame its samples were taken under (folder RULINGS `[02]`) — the viewport is a six-field value, so carrying it makes the stale-frame commit unrepresentable rather than guarded.
- Law: `MercatorFilter.Inverse` is admitted on its own domain (folder RULINGS `[02]`) — the cylindrical axis WRAPS at the antimeridian because the seam names a place, and the mercator clamp REFUSES because the asymptote names none.
- Law: durable removals prove their target existed at the ONE serialize both durable verbs cross (folder RULINGS `[02]`) — an absent prior payload was never committed, and recording that removal seals a delta empty on both halves.
- Law: `RedlineVerb` edits record on the pass that commits them (folder RULINGS `[04]`), the delta being the prior payload beside the new — a post-seal snapshot yields a no-op whose undo restores the edit it was meant to reverse.
- Law: a redline over the 3D viewport remains the BCF markup charter; this section owns only the 2D geographic plane.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// Both payload unions carry the `[JsonDerivedType]` roster because the annotation payload IS a wire and
// `[Union]` generates NO JSON support: the generator stamps a converter onto keyed owners alone, so a union
// reaches `System.Text.Json` as a bare abstract record and serializes the DECLARED type — an empty object per
// case, a total data loss no decode fails on. The kind literals are the durable discriminator, so a case
// renamed later keeps its literal.
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

// The vertex floors, one owner for three sites: a path needs two points to be a line and a ring needs four to
// close, and the projected-sample gate reads the SAME path floor rather than a bare literal beside it.
[SmartEnum<int>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RingArity {
    public static readonly RingArity Path = new(2);
    public static readonly RingArity Ring = new(4);
}

// The kind carries its own `EditMode` column, so `BeginMark` is one write rather than a verb-to-mode ladder at
// the adapter: a fourth mark kind names the session mode it opens on the row that declares it, and the
// nine-member `EditMode` vocabulary stays the package's, never mirrored as a second enum.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RedlineKind {
    public static readonly RedlineKind Point = new("point", EditMode.AddPoint);
    public static readonly RedlineKind Path = new("path", EditMode.AddLine);
    public static readonly RedlineKind Area = new("area", EditMode.AddPolygon);

    public EditMode Mode { get; }
}

// The gauge vocabulary. The bounds are what a review surface must hold to feel direct: a commit that misses its
// bound is a `Breached` span the receipt carries, derived rather than stored, so moving a bound never falsifies
// a measurement already taken.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RedlineLane : IGaugeLane<RedlineLane> {
    public static readonly RedlineLane Commit = new("commit", TimeSpan.FromMilliseconds(250));
    public static readonly RedlineLane Replay = new("replay", TimeSpan.FromMilliseconds(100));

    public TimeSpan Bound { get; }

    static IReadOnlyList<RedlineLane> IGaugeLane<RedlineLane>.Items => Items;
}

// --- [MODELS] ---------------------------------------------------------------------------
// Paint is the TOKEN ROW, never its key spelled as text: `PaintRole` is a keyed owner the Thinktecture
// generator stamps its own converter onto, so the durable blob still carries the key scalar while a role that
// does not exist is unspellable here rather than a string that resolves to nothing at re-render. That is also
// what makes the two ingresses one policy — `RedlineTool.Ink()` answers exactly this type, so a pen stroke's
// paint is the tool row's own and a vertex-authored mark's is the caller's, with no key transcription between.
[ComplexValueObject]
[ValidationError]
public sealed partial class RedlineStyle {
    public PaintRole Paint { get; }
    public Option<PaintRole> Fill { get; }
    public double Width { get; }
    public double Opacity { get; }
    public Seq<double> Dash { get; }

    // The stroke's own policy: the tool row's ink, the mean resolved nib weight so a pressure-tapered stroke
    // commits at the weight it was drawn at rather than at the tool's declared base, and no fill because a pen
    // path encloses nothing. A per-point width would need a variable-width mark the symbology axis cannot
    // express, so the fold states the one weight the mark carries.
    public static Fin<RedlineStyle> Of(RedlineStroke stroke, double opacity, Seq<double> dash, Op? key = null) =>
        key.OrDefault().AcceptValidated<RedlineStyle>(
            Validate(
                stroke.Ink,
                Option<PaintRole>.None,
                stroke.Points.IsEmpty ? stroke.Tool.Weight : stroke.Points.Sum(static point => point.Weight) / stroke.Points.Count,
                opacity,
                dash,
                out RedlineStyle? admitted),
            admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PaintRole paint,
        ref Option<PaintRole> fill,
        ref double width,
        ref double opacity,
        ref Seq<double> dash) =>
        validationError =
            double.IsFinite(width) && width > 0d
            && double.IsFinite(opacity) && opacity is >= 0d and <= 1d
            && dash.ForAll(static interval => double.IsFinite(interval) && interval > 0d)
                ? null
                : new ValidationError(string.Join(" | ", new object?[] { "redline: width, opacity, or dash is invalid" }));
}

// The two authoring ingresses one commit reads. A vertex session and a pen stroke differ in where the geometry
// COMES FROM and in nothing the durable leg does — same admission, same inverse crossing, same recorder hop —
// so a second commit verb beside this one would be a second copy of that leg maintained apart. The stroke arm
// carries the settled `Collab/issues#REDLINE_TOOLS` capture whole, so its tool, its ink, its per-point weights,
// and its erase routing all arrive already decided by the owner that decided them.
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

// Identities are the settled `Collab/sync` value objects the intent union declares, so a blank document or
// target key cannot be spelled onto a verb and the per-call identity guard the prior shape ran at two sites has
// nothing left to prove.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedlineVerb {
    private RedlineVerb() { }
    public sealed record BeginMark(RedlineKind Kind) : RedlineVerb;
    public sealed record Modify : RedlineVerb;
    public sealed record Delete(DocumentKey DocKey, ContainerKey TargetId) : RedlineVerb;
    public sealed record Commit(DocumentKey DocKey, ContainerKey TargetId, RedlineOrigin Origin) : RedlineVerb;
    public sealed record Discard : RedlineVerb;

    // The disposition dimension, total so a sixth verb breaks this projection rather than counting untagged.
    public string Disposition => Switch(
        beginMark: static _ => "begin",
        modify: static _ => "modify",
        delete: static _ => "delete",
        commit: static _ => "commit",
        discard: static _ => "discard");
}

// What a durable gesture answers: the intent, the receipt the recorder sealed, the cursor the recorder
// advanced, and the span the seal took. The prior 2-tuple dropped the cursor entirely, so a caller had no way
// to address the next undo position the recorder had just invalidated.
public readonly record struct RedlineCommit(
    EditIntent Intent, EditReceipt Receipt, RevertCursor Next, GaugedSpan<RedlineLane> Span);

// The session a verb leaves behind: the feature it sealed, if any, and the payload the target CARRIED before
// this gesture ran. Absence is an `Option` and not a parsed JSON null — the recorder's rail demands a DEFINED
// value kind, so that null is minted at the one edge that demands it and never threaded through the interior
// where every read had to test for it.
public readonly record struct RedlineSession(Option<GeometryFeature> Authored, Option<JsonElement> Before) {
    public static readonly RedlineSession Empty = new(None, None);

    // The session-transition family, seated on the state it transitions rather than scattered as eight private
    // statics on the surface. The manager is a mutable host state machine, the named statement carve-out on
    // this page — not an `Atom`, so the kernel `Transition` verdict has nothing to answer here and minting one
    // would fabricate a contention outcome no compare-and-swap produced.
    public static RedlineSession Open(EditManager manager, EditMode mode) {
        manager.EditMode = mode;
        return Empty;
    }

    // Delete and a pen commit both close AFTER snapshotting, so the payload the recorder inverts is the one the
    // layer actually held at the moment of the gesture. A pen commit has nothing in flight on the manager, so
    // sealing an empty session would drop back into the previous `Add*` mode and re-arm a stroke the gesture
    // already finished.
    public static RedlineSession Snapshot(EditManager manager, ContainerKey target) {
        Option<JsonElement> before = Prior(manager, target);
        return new RedlineSession(Close(manager), before);
    }

    // `EndEdit` seals an in-progress `DrawingLine` or `DrawingPolygon` stroke into `Layer` and answers `false`
    // on every path of the shipped member, so the sealed feature is READ BACK off the layer and never taken
    // from that return. A new mark is the layer's draft; a modified one is already stamped with this commit's
    // target — the two reads are disjoint, and a session that sealed neither answers `None` so `Drive` lands
    // the typed empty-commit refusal rather than an annotation over nothing. The snapshot precedes `EndEdit` by
    // construction: the seal writes over the layer, so a prior payload read after it would be the payload this
    // very gesture just wrote.
    public static RedlineSession Seal(EditManager manager, ContainerKey target) {
        Option<JsonElement> before = Prior(manager, target);
        ignore(manager.EndEdit());
        Option<GeometryFeature> authored = Held(manager, None) | Held(manager, Some(target));
        Close(manager);
        return new RedlineSession(authored, before);
    }

    // The one closing move, shared by commit, delete, and discard. The DRAFT is dropped because no committed
    // intent will ever rebuild it, while a modified mark is left in place — the layer re-renders from intent and
    // that rebuild restores its pre-edit geometry, so discarding a modification by removing the feature would
    // delete a committed mark. Reset precedes the mode write because an armed drag, rotation, or scale left in
    // the session re-enters the next one mid-manipulation against a feature that no longer exists.
    public static Option<GeometryFeature> Close(EditManager manager) {
        Held(manager, None).Iter(draft => ignore(manager.Layer?.TryRemove(draft)));
        manager.ResetManipulations();
        manager.EditMode = EditMode.None;
        return None;
    }

    // The prior payload, read off the feature's own field rather than re-derived from its geometry: the layer
    // write stamps the committed blob beside the id, so the layer carries the exact bytes the ledger holds and
    // a reconstruction that had to re-infer the style policy from a bare geometry is unspellable.
    static Option<JsonElement> Prior(EditManager manager, ContainerKey target) =>
        Held(manager, Some(target))
            .Bind(feature => Optional(feature[FeatureSlot.Payload] as string))
            .Map(static text => JsonSerializer.Deserialize<JsonElement>(text, EvidenceOps.Wire));

    // ONE probe over two arities of one question: an absent target names the unstamped DRAFT and a present one
    // names the feature already carrying that id. Every mark rebuilt from committed intent carries its target
    // in the feature's own field set, so the two reads partition the layer and neither rests on an enumeration
    // order `WritableLayer` never promises.
    static Option<GeometryFeature> Held(EditManager manager, Option<ContainerKey> target) =>
        Optional(manager.Layer)
            .Bind(layer => toSeq(layer.GetFeatures())
                .Choose(feature => feature is GeometryFeature geometry && Stamped(geometry, target) ? Some(geometry) : None)
                .Head);

    static bool Stamped(GeometryFeature geometry, Option<ContainerKey> target) =>
        target.Match(
            Some: key => geometry[FeatureSlot.Id] is string id && StringComparer.Ordinal.Equals(id, key.ToValue()),
            None: () => geometry[FeatureSlot.Id] is null);
}

// --- [OPERATIONS] -----------------------------------------------------------------------
// FORWARD AND INVERSE OF ONE CORRESPONDENCE SHARE ONE OWNER. The two legs sat as separate statics on the
// surface — one a `switch` over NTS runtime types, one a generated `Switch` over the payload union — so a fifth
// shape case broke one of them and round-tripped as an absence through the other.
public static class RedlineGeometry {
    public static Fin<RedlineShape> Shape(NetTopologySuite.Geometries.Geometry geometry) => geometry switch {
        NetTopologySuite.Geometries.Point point when Grounded(point.X, point.Y) =>
            Fin.Succ<RedlineShape>(new RedlineShape.Point(point.X, point.Y)),
        NetTopologySuite.Geometries.LineString line => Vertices(line.Coordinates, RingArity.Path, "path")
            .Map(static vertices => (RedlineShape)new RedlineShape.Path(vertices)),
        NetTopologySuite.Geometries.Polygon area => Vertices(area.ExteriorRing.Coordinates, RingArity.Ring, "area shell")
            .Bind(shell => toSeq(area.InteriorRings)
                .Traverse(ring => Vertices(ring.Coordinates, RingArity.Ring, "area hole"))
                .As()
                .Map(holes => (RedlineShape)new RedlineShape.Area(shell, holes))),
        NetTopologySuite.Geometries.GeometryCollection collection when collection.NumGeometries > 0 =>
            toSeq(Enumerable.Range(0, collection.NumGeometries))
                .Traverse(index => Shape(collection.GetGeometryN(index)))
                .As()
                .Map(static members => (RedlineShape)new RedlineShape.Collection(members)),
        // The fall-through is total by OMISSION over an open NTS hierarchy, which is why the inverse leg below
        // is a generated total `Switch`: the closed side of the correspondence is the one that must break.
        _ => Fin.Fail<RedlineShape>(new ChartFault.VisualDegenerate($"redline: {geometry.OgcGeometryType} is not an annotation shape")),
    };

    public static NetTopologySuite.Geometries.Geometry Geometry(RedlineShape shape) => shape.Switch(
        point: static value => (NetTopologySuite.Geometries.Geometry)new NetTopologySuite.Geometries.Point(value.Lon, value.Lat),
        path: static value => new NetTopologySuite.Geometries.LineString(Ring(value.Vertices)),
        area: static value => new NetTopologySuite.Geometries.Polygon(
            new NetTopologySuite.Geometries.LinearRing(Ring(value.Shell)),
            value.Holes.Map(static hole => new NetTopologySuite.Geometries.LinearRing(Ring(hole))).ToArray()),
        collection: static value => new NetTopologySuite.Geometries.GeometryCollection(
            value.Members.Map(Geometry).ToArray()));

    // The pen stroke's own projection. Screen samples cross the camera the GESTURE was drawn under — the origin
    // carries it — rather than whatever camera is mounted when the commit runs: a stroke commits after the
    // gesture ends, so reading the live viewport lands every mark displaced by exactly the pan, zoom, or
    // rotation between the last sample and the commit, and nothing on either leg reports it. The world result is
    // EPSG:3857, so the ONE inverse crossing carries it the rest of the way.
    public static Fin<RedlineShape> Traced(RedlineStroke stroke, Viewport frame) =>
        stroke.Points
            .Map(point => frame.ScreenToWorldXY(point.X, point.Y))
            .Filter(static at => double.IsFinite(at.worldX) && double.IsFinite(at.worldY)) switch {
            var world when world.Count < RingArity.Path.Key =>
                Fin.Fail<RedlineShape>(new ChartFault.VisualDegenerate($"redline: stroke carries {world.Count} projectable samples")),
            var world => MercatorFilter.Inverse
                .Reproject(new NetTopologySuite.Geometries.LineString(
                    world.Map(static at => new NetTopologySuite.Geometries.Coordinate(at.worldX, at.worldY)).ToArray()))
                .Bind(Shape),
        };

    static NetTopologySuite.Geometries.Coordinate[] Ring(Seq<(double Lon, double Lat)> vertices) =>
        vertices.Map(static at => new NetTopologySuite.Geometries.Coordinate(at.Lon, at.Lat)).ToArray();

    // Admission is on the WGS-84 DOMAIN rather than on finiteness alone: an ordinate outside the mercator clamp
    // is a coordinate the projection is asymptotic at, so a sample taken in the band a pan past the pole exposes
    // is a point that never had ground. Longitude carries no gate — the inverse row folds it onto its principal
    // range, because a cylindrical projection panned past the antimeridian still names the place it points at.
    static bool Grounded(double lon, double lat) =>
        double.IsFinite(lon) && double.IsFinite(lat) && Math.Abs(lat) <= MercatorFilter.PoleClampDeg;

    static Fin<Seq<(double Lon, double Lat)>> Vertices(
        NetTopologySuite.Geometries.Coordinate[] coordinates, RingArity arity, string kind) {
        Seq<(double Lon, double Lat)> vertices = toSeq(coordinates).Map(static at => (at.X, at.Y));
        return vertices.Count >= arity.Key && vertices.ForAll(static at => Grounded(at.Lon, at.Lat))
            ? Fin.Succ(vertices)
            : Fin.Fail<Seq<(double Lon, double Lat)>>(new ChartFault.VisualDegenerate($"redline: {kind} vertices are invalid"));
    }
}

// --- [COMPOSITION] ----------------------------------------------------------------------
// `History`, `Stamp`, and `Instruments` arrive by composition exactly as `Editing/forms` `ParameterLane` takes
// them, so the recorder, the causal stamp, and the meter have one owner each across every editing surface and
// this page mints none of the three. `Stamp` answers the Persistence `Hlc` cell the op log orders on.
public sealed class RedlineSurface(
    EditManager Manager,
    WritableLayer Marks,
    Symbology Symbology,
    EditHistory History,
    Func<Hlc> Stamp,
    InstrumentSet Instruments) {
    // Display style resolves once at mount from the SAME selector every feature row binds, so the review board
    // and the redline plane read one symbology vocabulary — and the session takes that same layer instance here,
    // so the layer a mark draws on and the layer a session authors into are one object.
    public WritableLayer Mounted() {
        Marks.Style = new ThemeStyle(Symbology.Select);
        Manager.Layer = Marks;
        return Marks;
    }

    // Every durable verb produces the intent AND records its inverse on one pass, so a committed mark is
    // reachable by the one `history.undo` intent rather than being durable-and-unreachable. The two local verbs
    // and the discard answer `None` and record nothing, because a session write no ledger saw has nothing to
    // revert. Every arm folds its own observation, so a disposition series exists for gestures that never
    // reached the recorder at all.
    public IO<Fin<Option<RedlineCommit>>> Drive(
        RedlineVerb verb, RevertCursor cursor, MonotonicTimeline line, IClock clock, CorrelationId correlation) =>
        Observe(verb).Match(
            Succ: _ => Opened(verb).Match(
                Succ: session => verb.Switch(
                    state: (Session: session, Surface: this),
                    beginMark: static (_, _) => Local(),
                    modify: static (_, _) => Local(),
                    delete: static (scope, command) => scope.Surface.Durable(
                        RedlineLane.Commit, line,
                        () => scope.Surface.Annotation(command.DocKey, command.TargetId, None, scope.Session.Before),
                        command.DocKey, command.TargetId, scope.Session.Before, cursor, clock, correlation),
                    commit: static (scope, commit) => scope.Surface.Durable(
                        RedlineLane.Commit, line,
                        () => scope.Surface.Sealed(commit, scope.Session),
                        commit.DocKey, commit.TargetId, scope.Session.Before, cursor, clock, correlation),
                    discard: static (_, _) => Local()),
                Fail: error => IO.pure(Fin.Fail<Option<RedlineCommit>>(error))),
            Fail: error => IO.pure(Fin.Fail<Option<RedlineCommit>>(error)));

    static IO<Fin<Option<RedlineCommit>>> Local() => IO.pure(Fin.Succ(Option<RedlineCommit>.None));

    // The one adapter over the `EditManager` session, total over the verb union. Session lifecycle is the
    // `EditMode` write — the kind's own column opens a stroke, `EndEdit()` seals a `Drawing*` stroke into
    // `Layer`, `EditMode.None` ends the session — and every gesture BETWEEN those writes is `EditingWidget`
    // routing pointer input into `EditManipulation` against this same manager, so no vertex, drag, insert,
    // rotate, or scale call appears here. The capture converts a throwing member into the page's own typed
    // refusal so an editing fault never escapes as a package exception past the arm that closes the session.
    Fin<RedlineSession> Opened(RedlineVerb verb) =>
        Op.Of(name: "appui.redline.open").Catch(() => Fin.Succ(verb.Switch(
                state: Manager,
                beginMark: static (manager, begin) => RedlineSession.Open(manager, begin.Kind.Mode),
                modify: static (manager, _) => RedlineSession.Open(manager, EditMode.Modify),
                delete: static (manager, command) => RedlineSession.Snapshot(manager, command.TargetId),
                commit: static (manager, commit) => commit.Origin.Switch(
                    state: (Manager: manager, commit.TargetId),
                    session: static (s, _) => RedlineSession.Seal(s.Manager, s.TargetId),
                    stroke: static (s, _) => RedlineSession.Snapshot(s.Manager, s.TargetId)),
                discard: static (manager, _) => RedlineSession.Empty with { Authored = RedlineSession.Close(manager) })));

    // The recorder hop. The op's delta is the payload the target CARRIED beside the payload it now carries, both
    // read off the one annotation blob, so the inverse is the same `Set` with its halves swapped and no
    // geometry-level inversion exists. The recorder's rail demands two DEFINED payloads, so the JSON null is
    // minted HERE — the one edge whose contract needs it — rather than threaded through the whole session type.
    // The seal is gauged: a span lands whether the seal succeeded or refused, because a refused commit still
    // took the time the review surface felt.
    IO<Fin<Option<RedlineCommit>>> Durable(
        RedlineLane lane, MonotonicTimeline line, Func<Fin<EditIntent>> seal,
        DocumentKey docKey, ContainerKey targetId, Option<JsonElement> before,
        RevertCursor cursor, IClock clock, CorrelationId correlation) =>
        line.Gauged(lane, Op.Of(name: nameof(Durable)), seal).Match(
            Succ: gauged => gauged.Value.Match(
                Succ: intent => History
                    .Record(
                        new RevertibleOp(
                            targetId.ToValue(), docKey.ToValue(), History.Actor,
                            new RevertDelta.Set(Defined(before), ((EditIntent.Annotation)intent).Payload),
                            Stamp()),
                        cursor, clock, correlation)
                    .Map(recorded => recorded.Map(sealed_ =>
                        Some(new RedlineCommit(intent, sealed_.Receipt, sealed_.Next, gauged.Span)))),
                Fail: error => IO.pure(Fin.Fail<Option<RedlineCommit>>(error))),
            Fail: error => IO.pure(Fin.Fail<Option<RedlineCommit>>(error)));

    // A JSON null is a DEFINED value kind, so it admits on the recorder's rail while `default` would be
    // undefined and refuse — which is what makes "this target had nothing before" recordable at all.
    static readonly JsonElement Nothing = JsonDocument.Parse("null").RootElement.Clone();

    static JsonElement Defined(Option<JsonElement> payload) => payload.IfNone(Nothing);

    // The ONE commit seal, total over the origin union: the shape and the paint policy resolve per origin and
    // the delta serializes once, so the vertex session and the pen stroke reach durable truth through one body
    // and cannot drift on the first repair to either. An ERASING stroke lands `RedlineDelta.Delete`, because the
    // capture already read that intent off the stylus axis and a mark that committed an upsert after the nib was
    // inverted would draw exactly what the gesture asked to remove.
    Fin<EditIntent> Sealed(RedlineVerb.Commit commit, RedlineSession session) => commit.Origin.Switch(
        state: (Surface: this, Commit: commit, Session: session),
        session: static (scope, origin) => scope.Session.Authored
            .ToFin((Error)new ChartFault.VisualEmpty("redline: commit has no authored feature"))
            .Bind(static feature => Optional(feature.Geometry)
                .ToFin((Error)new ChartFault.VisualEmpty("redline: authored feature has no geometry")))
            // Authored view geometry returns to WGS-84 before it crosses the intent seam, so no EPSG:3857
            // coordinate ever lands in durable truth.
            .Bind(MercatorFilter.Inverse.Reproject)
            .Bind(RedlineGeometry.Shape)
            // The origin's paint was admitted at its own construction, so the seal carries it rather than
            // re-running an admission the value already proved.
            .Map(shape => (Shape: shape, Style: origin.Style))
            .Bind(mark => scope.Surface.Annotation(
                scope.Commit.DocKey, scope.Commit.TargetId, Some(mark), scope.Session.Before)),
        // The stroke arm never reads the manager: the capture IS the geometry, so the screen samples unproject
        // through the frame the ORIGIN carries and the same inverse crossing lifts them out of the view frame.
        stroke: static (scope, origin) => origin.Captured.Erases
            ? scope.Surface.Annotation(scope.Commit.DocKey, scope.Commit.TargetId, None, scope.Session.Before)
            : RedlineStyle.Of(origin.Captured, origin.Opacity, origin.Dash)
                .Bind(style => RedlineGeometry.Traced(origin.Captured, origin.Frame)
                    .Map(shape => (Shape: shape, Style: style)))
                .Bind(mark => scope.Surface.Annotation(
                    scope.Commit.DocKey, scope.Commit.TargetId, Some(mark), scope.Session.Before)));

    // The ONE serialize both durable verbs cross, and the one REMOVAL gate beside it. An absent mark is the
    // delete delta, so an erasing stroke and an explicit delete verb produce the identical payload and the
    // recorder inverts both the same way. A removal whose target carried NOTHING refuses here: the layer
    // re-renders from committed intent, so a target absent from it was never committed, and recording that
    // removal seals a delta whose halves are both empty — a durable no-op the ledger keeps, the fan counts as an
    // applied edit, and an undo restores nothing from. A first AUTHORING reads the same empty prior and admits,
    // because the removal is what the emptiness contradicts rather than the emptiness itself. The gate is an
    // `Option` pair rather than a `JsonValueKind` test, so "nothing before" is a shape and not a value.
    Fin<EditIntent> Annotation(
        DocumentKey docKey, ContainerKey targetId,
        Option<(RedlineShape Shape, RedlineStyle Style)> mark, Option<JsonElement> before) =>
        mark.IsNone && before.IsNone
            ? Fin.Fail<EditIntent>(new ChartFault.VisualEmpty($"redline: {targetId} carries no mark to remove"))
            : Fin.Succ((EditIntent)new EditIntent.Annotation(
                docKey,
                targetId,
                JsonSerializer.SerializeToElement<RedlineDelta>(
                    mark.Match<RedlineDelta>(
                        Some: static row => new RedlineDelta.Upsert(new RedlineMark(row.Shape, row.Style)),
                        None: static () => new RedlineDelta.Delete()),
                    EvidenceOps.Wire)));

    // The one layer write both a revert and a committed-intent re-render take: decode the payload, drop any
    // feature already stamped with the target, and re-add the mark the payload describes projected forward into
    // the view frame. A `Delete` payload writes nothing back, which is exactly what makes the undo of a delete
    // an upsert of the payload the recorder held. The span is gauged on the replay lane, so an undo that stalls
    // the review surface is evidence rather than a report.
    public Fin<(Fin<Unit> Applied, GaugedSpan<RedlineLane> Span)> Replay(RevertibleOp op, MonotonicTimeline line) =>
        line.Gauged(RedlineLane.Replay, Op.Of(name: nameof(Replay)), () => Applied(op));

    Fin<Unit> Applied(RevertibleOp op) =>
        op.Delta is RevertDelta.Set set
            ? Op.Of(name: "appui.redline.replay").Catch(() => {
                toSeq(Marks.GetFeatures())
                    .Choose(feature => feature is GeometryFeature geometry
                        && geometry[FeatureSlot.Id] is string id && StringComparer.Ordinal.Equals(id, op.Target)
                        ? Some(geometry) : None)
                    .Iter(stale => ignore(Marks.TryRemove(stale)));
                return Fin.Succ(Optional(JsonSerializer.Deserialize<RedlineDelta>(set.After, EvidenceOps.Wire))
                    .Bind(static delta => delta is RedlineDelta.Upsert upsert ? Some(upsert.Mark) : None));
            })
                .Bind(mark => mark.Match(
                    Some: row => Drawn(row, op.Target).Map(feature => { Marks.Add(feature); return unit; }),
                    None: static () => Fin.Succ(unit)))
                .Map(_ => { Marks.DataHasChanged(); return unit; })
            : Fin.Fail<Unit>(new ChartFault.VisualDegenerate($"redline replay: {op.Kind.Key} is not an annotation delta"));

    // The forward leg of the one crossing, mirroring the seal's inverse: a payload is WGS-84 and the view frame
    // is EPSG:3857, so a mark re-entering the layer crosses exactly once. The committed blob rides the feature
    // so the NEXT gesture's snapshot reads the exact bytes the ledger holds — re-deriving a payload from the
    // drawn geometry would lose the style policy entirely and make every undo restore a mark in the wrong ink.
    // The crossing answers a rail rather than a bare feature: a refused reprojection is the page's typed fault,
    // and this section carries no `throw` at all.
    static Fin<GeometryFeature> Drawn(RedlineMark mark, string target) =>
        MercatorFilter.Forward.Reproject(RedlineGeometry.Geometry(mark.Shape))
            .Map(view => {
                GeometryFeature feature = new(view);
                feature[FeatureSlot.Id] = target;
                feature[FeatureSlot.Payload] = JsonSerializer.Serialize<RedlineDelta>(new RedlineDelta.Upsert(mark), EvidenceOps.Wire);
                return feature;
            });

    public static readonly InstrumentSpec Gesture = InstrumentSpec.Create(
        "rasm.appui.redline.gesture", InstrumentKind.Count, MeasureForm.Whole, "{gesture}",
        "redline gestures by disposition", Seq(AppUiTelemetry.OutcomeSlot), None, None, None);

    // `Drive` is the one place a disposition exists, so the projection binds there and the count carries it as
    // the DECLARED outcome dimension — a spec promising a per-disposition breakdown with no declared slot folds
    // commits, discards, and every local gesture into one number no board can separate.
    public Fin<Unit> Observe(RedlineVerb verb) =>
        Instruments.Write(Gesture, 1d, InstrumentSet.Tags((AppUiTelemetry.OutcomeSlot, verb.Disposition)));

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Gesture);
}
```

## [06]-[RESEARCH]

(none)

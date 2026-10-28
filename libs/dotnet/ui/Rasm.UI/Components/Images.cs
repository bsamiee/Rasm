using System.Globalization;
using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ImageRegion {
    public static ImageRegion Whole { get; } = new(new SKRect(0f, 0f, 1f, 1f));

    public SKRect Bounds { get; }

    internal Vector2 At(Vector2 anchor) => Vector2.Lerp(new Vector2(Bounds.Left, Bounds.Top), new Vector2(Bounds.Right, Bounds.Bottom), anchor);

    internal Option<ImageRegion> Moved(Vector2 anchor, Vector2 to, Option<float> aspect) {
        Vector2 at = Vector2.Clamp(to, Vector2.Zero, Vector2.One);
        SKRect moved = anchor == new Vector2(0.5f)
            ? SKRect.Create(float.Clamp(at.X - (Bounds.Width / 2f), 0f, 1f - Bounds.Width), float.Clamp(at.Y - (Bounds.Height / 2f), 0f, 1f - Bounds.Height), Bounds.Width, Bounds.Height)
            : new SKRect(anchor.X is 0f ? at.X : Bounds.Left, anchor.Y is 0f ? at.Y : Bounds.Top, anchor.X is 1f ? at.X : Bounds.Right, anchor.Y is 1f ? at.Y : Bounds.Bottom);
        SKRect held = aspect.Match(
            Some: wide => anchor switch {
                { X: not 0.5f } => anchor.Y is 0f ? moved with { Top = moved.Bottom - (moved.Width / wide) } : moved with { Bottom = moved.Top + (moved.Width / wide) },
                { Y: not 0.5f } => moved with { Right = moved.Left + (moved.Height * wide) },
                _ => moved,
            },
            None: () => moved);
        return Validated(held.Standardized).ToOption();
    }

    public static Fin<ImageRegion> Validated(SKRect edges) =>
        UIFault.Unless(edges is { Left: >= 0f, Top: >= 0f, Right: <= 1f, Bottom: <= 1f } && edges.Left < edges.Right && edges.Top < edges.Bottom, UIReason.Range, edges).Map(_ => new ImageRegion(edges));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageTool {
    private static readonly Seq<Vector2> HandlePositions = toSeq(from y in Seq(0f, 0.5f, 1f) from x in Seq(0f, 0.5f, 1f) select new Vector2(x, y));
    private static readonly Key Picked = new Key.Named("pick");

    internal abstract Node Drawn(PlotPlane view, Vector2 extent);

    internal static Option<PlotPlane> Framed(SKRect pixels) =>
        (AxisRange.Validated(pixels.Left, pixels.Right), AxisRange.Validated(pixels.Bottom, pixels.Top))
            .Apply(static (across, down) => (PlotPlane)new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(down))).As().ToOption();

    internal static Option<Node> Clipped(SKRect pixels, Vector2 extent, Node image) =>
        Framed(pixels).Map(plane => (Node)new Node.Pin(new Vector2(pixels.Left, pixels.Top), new Node.Plot(plane, [new Node.Pin(Vector2.Zero, image) { Span = extent }]) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        }) { Span = new Vector2(pixels.Width, pixels.Height) });

    public sealed record Region(ImageRegion Value, Func<Edit<ImageRegion>, IO<Unit>> Edited) : ImageTool {
        public Option<float> Aspect { get; init; }

        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<ImageRegion>(Value, Points<ImageRegion>.Fixed(HandlePositions.Map(anchor => Lens<ImageRegion, Vector2>.New(
                    held => held.At(anchor) * extent,
                    to => held => held.Moved(anchor, to / extent, Aspect.Map(wide => wide * extent.Y / extent.X)).IfNone(held)))) with { Placed = static (_, _, _) => None, },
                Edited).Drawn(view, RegionMark(extent).ToSeq());

        private Option<Node> RegionMark(Vector2 extent) =>
            (AxisRange.Validated(Value.Bounds.Left * extent.X, Value.Bounds.Right * extent.X), AxisRange.Validated(Value.Bounds.Top * extent.Y, Value.Bounds.Bottom * extent.Y))
                .Apply(static (across, down) => (Node)new Node.Mark(new Figure.Region { First = Some(across), Second = Some(down) }) { Role = Role.Ring }).As().ToOption();
    }

    public sealed record Pick(Option<Vector2> At, Func<Edit<Option<Vector2>>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<Option<Vector2>>(At, new Points<Option<Vector2>>(
                    static at => at.ToSeq().Map(static held => new HandlePoint(Picked, held)),
                    static (at, targets, _) => new Dragged<Option<Vector2>>(targets.Last.Map(static target => target.At) | at, targets.Map(static target => target.Key))) {
                Placed = static (_, at, _) => Some(new Dragged<Option<Vector2>>(Some(at), [Picked])),
            },
                Edited) { BackgroundCursor = CursorShape.Crosshair }.Drawn(view, []);
    }

    public sealed record Compare(Content<ImageSource> Before, Fraction At, Func<Edit<Fraction>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            ((float)At * extent.X) switch {
                var split => new Handles<Fraction>(At, Points<Fraction>.Fixed([Lens<Fraction, Vector2>.New(
                        at => new Vector2((float)at * extent.X, extent.Y / 2f),
                        to => held => Fraction.Validated(to.X / extent.X).ToOption().IfNone(held))]), Edited)
                    .Drawn(view, Before.Current.Bind(source => Clipped(new SKRect(0f, 0f, split, extent.Y), extent, new Node.Image(source) { Rendering = ImageRendering.Pixelated })).ToSeq()
                        .Add(new Node.Mark(new Figure.Path([new Vector2(split, 0f), new Vector2(split, extent.Y)])) { Role = Role.Needle })),
            };
    }
}

public sealed record Loupe(SKRect Lens, Filter.Magnify Magnify);

public sealed record ImagePlot(Content<ImageSource> Image, Func<Edit<PlotPlane>, IO<Unit>> Navigated) {
    public Option<PlotPlane> View { get; init; }
    public Option<string> Caption { get; init; }
    public Seq<Node> Overlays { get; init; }
    public Option<ImageTool> Tool { get; init; }
    public Option<Loupe> Loupe { get; init; }
    public Option<ColorRamp> Scale { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public Node Drawn =>
        Shown.Match(
            Some: view => (Node)new Node.Overlay(
                new Node.Plot(view, Image.Current.ToSeq().Bind(source => Imaged(source, view))) {
                    Part = Fit.Map(rest => (Part)new Part.Viewport(rest, Navigated) { Natural = Some(Vector2.One) }),
                    Width = Sizing.Grow.Unit,
                    Aspect = Image.Current.Bind(static source => Factor.Validated((float)source.Info.Width / source.Info.Height).ToOption()),
                    Conditions = Image.Conditions,
                    Hovered = Hovered,
                    Accessible = Described,
                },
                Caption.Map(static caption => (Node)new Node.Text([caption]) { Role = Role.OverlayCaption }).ToSeq()),
            None: () => new Node.Box { Width = Sizing.Grow.Unit, Conditions = Image.Conditions, Accessible = Described });

    public Option<float> Zoom(float across) => Shown.Bind(static view => view.Dimension(PhysicalAxis.Horizontal)).Map(axis => across / MathF.Abs(axis.Domain.Span));

    public Option<PlotPlane> Fit => Image.Current.Bind(static source => ImageTool.Framed(source.Info.Rect));

    private Option<PlotPlane> Shown => View | Fit;

    private Accessible Described => new() { Role = AccessRole.Image, Label = Caption, Help = Image.Message };

    private Seq<Node> Imaged(ImageSource source, PlotPlane view) {
        Vector2 extent = new(source.Info.Width, source.Info.Height);
        Node.Image image = new(source) { Rendering = ImageRendering.Pixelated, Scale = Scale };
        return Seq<Node>(new Node.Pin(Vector2.Zero, image) { Span = extent })
            + Overlays
            + Tool.Map(tool => tool.Drawn(view, extent)).ToSeq()
            + (from loupe in Loupe
               from lens in ImageTool.Clipped(loupe.Lens, extent, image)
               select lens with { Role = Role.Ring, Sheet = new StyleSheet(HashMap((Role.Ring, new Look { Filters = Seq<Filter>(loupe.Magnify) }))) }).ToSeq();
    }
}

public sealed record ImageTile(Content<SKImage> Image) {
    public Option<IO<Seq<string>>> Files { get; init; }
    public Option<Func<Seq<string>, IO<Unit>>> Dropped { get; init; }

    public Node Drawn =>
        Image.Current.Map(static image => (Node)new Node.Image(new ImageSource.Still(image)) { Fit = ImageFit.Contain }).IfNone(static () => new Node.Box()) with {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = Image.Conditions,
            Lift = Files.Map(static files => new Lift(_ => files.Map(static paths => (Transfer)new Transfer.Files(paths)))),
            Drop = Dropped.Map(static dropped => Drop.Of<Transfer.Files>((files, _, _) => dropped(files.Paths))),
            Accessible = new Accessible { Role = AccessRole.Image, Help = Image.Message },
        };
}

public sealed record Tile<TItem>(Key Key, TItem Item, SKSizeI Pixels) where TItem : notnull {
    internal Content<SKImage> Content { get; init; } = new Content<SKImage>.Pending();
    internal int Revision { get; init; }
}

public sealed record TileCache<TItem> where TItem : notnull {
    public static TileCache<TItem> Empty { get; } = new();

    private HashMap<Key, Tile<TItem>> Tiles { get; init; }
    private int Revision { get; init; }

    public TileCache<TItem> Reloaded => this with { Revision = Revision + 1 };

    public Content<SKImage> Shown(Key key) => Tiles.Find(key).Map(static tile => tile.Content).IfNone(static () => new Content<SKImage>.Pending());

    public (TileCache<TItem> Cache, Seq<Tile<TItem>> Wanted, Seq<SKImage> Released) Requested(Seq<(Key Key, TItem Item)> shown, SKSizeI pixels) {
        HashSet<Key> keys = toHashSet(shown.Map(static entry => entry.Key));
        Seq<Tile<TItem>> wanted = shown.Filter(entry => !Tiles.Find(entry.Key).Exists(tile => tile.Pixels == pixels && tile.Revision == Revision && tile.Item.Equals(entry.Item)))
            .Map(entry => new Tile<TItem>(entry.Key, entry.Item, pixels) { Content = Shown(entry.Key).Requested(None), Revision = Revision }).Strict();
        TileCache<TItem> next = this with { Tiles = wanted.Fold(Tiles.Filter((key, _) => keys.Contains(key)), static (held, tile) => held.AddOrUpdate(tile.Key, tile)) };
        return (next, wanted, Released(next));
    }

    public (TileCache<TItem> Cache, Seq<SKImage> Released) Arrived(Tile<TItem> asked, Fin<SKImage> arrival) =>
        Tiles.Find(asked.Key) == Some(asked)
            ? (this with { Tiles = Tiles.AddOrUpdate(asked.Key, asked with { Content = Content<SKImage>.Arrived(arrival) }) }) switch { var next => (next, Released(next)) }
            : (this, arrival.ToOption().ToSeq());

    private Seq<SKImage> Images => toSeq(Tiles.Values).Bind(static tile => tile.Content.Current.ToSeq());

    private Seq<SKImage> Released(TileCache<TItem> next) {
        HashSet<SKImage> kept = toHashSet(next.Images);
        return Images.Filter(image => !kept.Contains(image));
    }
}

public sealed record Gallery<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, string> Label, TileCache<TItem> Tiles, Func<Seq<Key>, Part.Choice> Choice, float TileSide) where TItem : notnull {
    public Factor Aspect { get; init; } = Factor.Unit;
    public HashSet<Key> Realized { get; init; }
    public bool Numbered { get; init; }
    public Func<TItem, Seq<Node>> Overlay { get; init; } = static _ => [];
    public Option<Func<TItem, Option<string>>> File { get; init; }
    public Option<Func<Seq<string>, Option<Key>, Placement, IO<Unit>>> Dropped { get; init; }

    public Node.Scroll Drawn(CultureInfo culture) {
        Seq<(Key Key, TItem Item, int Number)> entries = Items.Map((item, index) => (Key: Keyed(item), Item: item, Number: index + 1)).Strict();
        HashMap<Key, (Key Key, TItem Item, int Number)> keyed = toHashMap(entries.Map(static entry => (entry.Key, entry)));
        return new Node.Scroll(PhysicalAxis.Vertical, Choice(entries.Map(static entry => entry.Key)).Drawn(
            key => TileNode(keyed[key], culture), PhysicalAxis.Vertical, new Tracks([new Sizing.Fixed(TileSide)]) { AutoFill = true }, Realized) with {
            Extent = TileSide / (float)Aspect,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Lift = File.Map(file => new Lift(keys => IO.pure<Transfer>(new Transfer.Files(keys.Choose(key => keyed.Find(key)).Choose(entry => file(entry.Item)))))),
            Drop = Dropped.Map(static dropped => Drop.Of<Transfer.Files>((files, target, placement) => dropped(files.Paths, target, placement))),
        }) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    public (TileCache<TItem> Cache, Seq<Tile<TItem>> Wanted, Seq<SKImage> Released) Requested(float scale) =>
        Tiles.Requested(
            Items.Map(item => (Key: Keyed(item), Item: item)).Filter(entry => Realized.Contains(entry.Key)),
            SKRectI.Ceiling(SKRect.Create(TileSide * scale, TileSide / (float)Aspect * scale)).Size);

    private Node.Overlay TileNode((Key Key, TItem Item, int Number) entry, CultureInfo culture) =>
        new(new ImageTile(Tiles.Shown(entry.Key)).Drawn,
            (Numbered ? Seq<Node>(new Node.Text([entry.Number.ToString(culture)]) { Role = Role.OverlayCaption }) : [])
            + Overlay(entry.Item).Add(new Node.Text([Label(entry.Item)]) { Role = Role.OverlayCaption, MaxLines = 1, Width = Sizing.Grow.Unit, Align = UnitPoint.Bottom })) {
            Accessible = new Accessible { Label = Label(entry.Item), Shortcut = Numbered && entry.Number <= 9 ? Some(entry.Number.ToString(CultureInfo.InvariantCulture)) : None },
        };
}

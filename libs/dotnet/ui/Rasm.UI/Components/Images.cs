using System.Globalization;
using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ImageRegion {
    public static ImageRegion Whole { get; } = new(new SKRect(0f, 0f, 1f, 1f));

    public SKRect Edges { get; }

    public Vector2 At(Vector2 anchor) => Vector2.Lerp(new Vector2(Edges.Left, Edges.Top), new Vector2(Edges.Right, Edges.Bottom), anchor);

    public Option<ImageRegion> Moved(Vector2 anchor, Vector2 to, Option<float> aspect) {
        Vector2 at = Vector2.Clamp(to, Vector2.Zero, Vector2.One);
        SKRect moved = anchor == new Vector2(0.5f)
            ? SKRect.Create(float.Clamp(at.X - (Edges.Width / 2f), 0f, 1f - Edges.Width), float.Clamp(at.Y - (Edges.Height / 2f), 0f, 1f - Edges.Height), Edges.Width, Edges.Height)
            : new SKRect(anchor.X is 0f ? at.X : Edges.Left, anchor.Y is 0f ? at.Y : Edges.Top, anchor.X is 1f ? at.X : Edges.Right, anchor.Y is 1f ? at.Y : Edges.Bottom);
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
    private static readonly Seq<Vector2> Anchors = toSeq(from y in Seq(0f, 0.5f, 1f) from x in Seq(0f, 0.5f, 1f) select new Vector2(x, y));
    private static readonly Key Picked = new Key.Named("pick");

    internal abstract Node Drawn(PlotPlane view, Vector2 extent);

    public sealed record Region(ImageRegion Value, Option<float> Aspect, Func<Edit<ImageRegion>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<ImageRegion>(Value, Points<ImageRegion>.Fixed(Anchors.Map(anchor => Lens<ImageRegion, Vector2>.New(
                    held => held.At(anchor) * extent,
                    to => held => held.Moved(anchor, to / extent, Aspect.Map(wide => wide * extent.Y / extent.X)).IfNone(held)))) with { Placed = static (_, _, _) => None, },
                Edited).Drawn(view, Ringed(extent).ToSeq());

        private Option<Node> Ringed(Vector2 extent) =>
            (AxisRange.Validated(Value.Edges.Left * extent.X, Value.Edges.Right * extent.X), AxisRange.Validated(Value.Edges.Top * extent.Y, Value.Edges.Bottom * extent.Y))
                .Apply(static (across, down) => (Node)new Node.Mark(new Figure.Region { First = Some(across), Second = Some(down) }) { Role = Role.Ring }).As().ToOption();
    }

    public sealed record Pick(Option<Vector2> At, Func<Edit<Option<Vector2>>, IO<Unit>> Edited) : ImageTool {
        internal override Node Drawn(PlotPlane view, Vector2 extent) =>
            new Handles<Option<Vector2>>(At, new Points<Option<Vector2>>(
                    static at => at.Map(static held => Seq(new Anchor(Picked, held))).IfNone([]),
                    static (at, targets, _) => new Dragged<Option<Vector2>>(targets.Last.Map(static target => target.At) | at, targets.Map(static target => target.Key))) {
                Placed = static (_, at, _) => Some(new Added<Option<Vector2>>(Some(at), Picked)),
            },
                Edited) { GroundCursor = CursorShape.Crosshair }.Drawn(view, []);
    }
}

public sealed record ImagePlot(Content<SKImage> Image, SKSizeI Pixels, PlotPlane View, Func<Edit<PlotPlane>, IO<Unit>> Navigated) {
    public Option<string> Caption { get; init; }
    public Seq<Node> Overlays { get; init; }
    public Option<ImageTool> Tool { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

    public Option<PlotPlane> Fit =>
        (AxisRange.Validated(0f, Pixels.Width), AxisRange.Validated(Pixels.Height, 0f))
            .Apply(static (across, down) => (PlotPlane)new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(down))).As().ToOption();

    public Node Drawn =>
        new Node.Overlay(
            new Node.Plot(View, Image.Held.Map(image => (Node)Covering(new Node.Image(image) { Rendering = ImageRendering.Pixelated })).ToSeq() + Overlays + Tool.Map(tool => tool.Drawn(View, Extent)).ToSeq()) {
                Part = Fit.Map(rest => (Part)new Part.Viewport(rest, Navigated)),
                Width = Sizing.Grow.Unit,
                Aspect = Factor.Validated((float)Pixels.Width / Pixels.Height).ToOption(),
                Conditions = ImageTile.Conditioned(Image),
                Hovered = Hovered,
                Accessible = new Accessible { Role = AccessRole.Image, Label = Caption },
            },
            Caption.Map(static caption => (Node)new Node.Text([caption]) { Role = Role.OverlayCaption }).ToSeq()) {
            Width = Sizing.Grow.Unit,
        };

    public Node.Pin Covering(Node content) => new(Vector2.Zero, content) { Span = Extent };

    public Option<PlotPlane> ActualSize(float across) =>
        View.Dimension(PhysicalAxis.Horizontal)
            .Bind(axis => View.Scaled(View.Dimensions.Fold(Vector2.Zero, static (center, held) => held.Along.With(center, (held.Axis.Domain.From + held.Axis.Domain.To) / 2f)), across / MathF.Abs(axis.Domain.Span)));

    public Option<string> Zoom(float across, CultureInfo culture) => View.Dimension(PhysicalAxis.Horizontal).Map(axis => (across / MathF.Abs(axis.Domain.Span)).ToString("P0", culture));

    internal Option<IO<Unit>> Navigation(PlotPlane plane) => plane == View ? None : Some(Navigated(Edit.Commit(plane)));

    private Vector2 Extent => new(Pixels.Width, Pixels.Height);
}

[SmartEnum<string>]
public sealed partial class ImageOperation {
    public static readonly ImageOperation Fit = new("fit", static (view, _) => view.Fit.Bind(view.Navigation));
    public static readonly ImageOperation ActualSize = new("actual-size", static (view, across) => view.ActualSize(across).Bind(view.Navigation));

    [UseDelegateFromConstructor]
    public partial Option<IO<Unit>> Applied(ImagePlot view, float across);

    public Command<TGate, TInvoke>.Run Commanded<TGate, TInvoke>(string caption, Func<ImageOperation, TGate> enabled, Func<ImageOperation, TInvoke> invoke) where TGate : notnull where TInvoke : notnull =>
        new(new CommandFace(Key, caption), Some(enabled(this)), invoke(this));
}

public sealed record ImageTile(Content<SKImage> Image) {
    public Node Drawn =>
        new Node.Overlay(
            new Node.Box { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
            Image.Held.Map(static image => (Node)new Node.Image(image) { Fit = ImageFit.Contain, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }).ToSeq()) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = Conditioned(Image),
            Accessible = new Accessible {
                Role = AccessRole.Image,
                Help = Image.Switch(
                    pending: static _ => Option<string>.None,
                    ready: static _ => Option<string>.None,
                    empty: static empty => Some(empty.Reason),
                    failed: static failed => failed.Error.IsExpected ? Some(failed.Error.Message) : None),
            },
        };

    internal static HashSet<Condition> Conditioned(Content<SKImage> image) =>
        image.Map<HashSet<Condition>>(pending: [Condition.Busy], ready: [], empty: [], failed: [Condition.Faulted]);
}

public sealed record Tile<TItem>(Content<SKImage> Content, TItem Item, int Revision, SKSizeI Pixels) where TItem : notnull;

public sealed record TileCache<TItem>(HashMap<Key, Tile<TItem>> Held, int Revision) where TItem : notnull {
    public static TileCache<TItem> Empty { get; } = new([], 0);

    public TileCache<TItem> Reloaded => this with { Revision = Revision + 1 };

    public Content<SKImage> Shown(Key key) => Held.Find(key).Map(static tile => tile.Content).IfNone(static () => new Content<SKImage>.Pending());

    public Requested<TItem> Requested(Seq<KeyedItem<TItem>> shown, SKSizeI pixels) {
        Seq<TileAsk<TItem>> wanted = shown.Filter(entry => !Held.Find(entry.Key).Exists(tile => tile.Pixels == pixels && tile.Revision == Revision && EqDefault<TItem>.Equals(tile.Item, entry.Item)))
            .Map(entry => new TileAsk<TItem>(entry.Key, new Tile<TItem>(Shown(entry.Key).Requested(None), entry.Item, Revision, pixels))).Strict();
        return new(this with { Held = wanted.Fold(Held, static (held, ask) => held.AddOrUpdate(ask.Key, ask.Tile)) }, wanted);
    }

    public Kept<TItem> Arrived(Key key, Tile<TItem> asked, Fin<SKImage> arrival) =>
        Held.Find(key) == Some(asked)
            ? Released(this with { Held = Held.AddOrUpdate(key, asked with { Content = Content<SKImage>.Arrived(arrival) }) })
            : new Kept<TItem>(this, arrival.ToOption().ToSeq());

    public Kept<TItem> Kept(HashSet<Key> keys) => Released(this with { Held = Held.Filter((key, _) => keys.Contains(key)) });

    private Seq<SKImage> Images => toSeq(Held.Values).Bind(static tile => tile.Content.Held.ToSeq());

    private Kept<TItem> Released(TileCache<TItem> next) => new(next, Images.Filter(image => !next.Images.Exists(kept => ReferenceEquals(kept, image))));
}

public sealed record KeyedItem<TItem>(Key Key, TItem Item) where TItem : notnull;

public sealed record TileAsk<TItem>(Key Key, Tile<TItem> Tile) where TItem : notnull;

public sealed record Requested<TItem>(TileCache<TItem> Cache, Seq<TileAsk<TItem>> Wanted) where TItem : notnull;

public sealed record Kept<TItem>(TileCache<TItem> Cache, Seq<SKImage> Released) where TItem : notnull;

public sealed record Resizing(string Label, Func<Edit<float>, IO<Unit>> Edited);

public sealed record Gallery<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, string> Label, TileCache<TItem> Tiles, Func<Seq<Key>, Part.Choice> Choice) where TItem : notnull {
    private static readonly AxisRange Sides = AxisRange.Validated(32f, 256f).ThrowIfFail();

    public Option<float> TileSize { get; init; }
    public AxisRange TileSizes { get; init; } = Sides;
    public Factor Aspect { get; init; } = Factor.Unit;
    public HashSet<Key> Realized { get; init; }
    public Option<Key> Current { get; init; }
    public bool Numbered { get; init; }
    public Func<TItem, Seq<Node>> Overlay { get; init; } = static _ => [];
    public Option<Resizing> Resized { get; init; }
    public Option<string> Empty { get; init; }

    private float Side => float.Clamp(TileSize.IfNone(96f), TileSizes.Low, TileSizes.High);

    private Seq<KeyedItem<TItem>> Shown => Items.Map(item => new KeyedItem<TItem>(Keyed(item), item)).Filter(entry => Realized.Contains(entry.Key));

    public Node Drawn {
        get {
            Seq<(Key Key, TItem Item, int Number)> entries = Items.Map((item, index) => (Key: Keyed(item), Item: item, Number: index + 1)).Strict();
            HashMap<Key, (Key Key, TItem Item, int Number)> keyed = toHashMap(entries.Map(static entry => (entry.Key, entry)));
            return new Node.Flex(PhysicalAxis.Vertical, Resizer.ToSeq().Add(Empty.Filter(_ => Items.IsEmpty).Match(
                Some: static empty => (Node)new Node.Text([empty]) { Role = Role.Label, Align = UnitPoint.Center, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
                None: () => new Node.Scroll(PhysicalAxis.Vertical, Choice(entries.Map(static entry => entry.Key)).Drawn(
                    key => Tiled(keyed[key].Item, key, keyed[key].Number), PhysicalAxis.Vertical, new Tracks.AutoFill(new Sizing.Fixed(Side)), Side / (float)Aspect, Realized) with {
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                }) {
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                }))) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
        }
    }

    public SKSizeI Pixels(float scale) => new((int)MathF.Ceiling(Side * scale), (int)MathF.Ceiling(Side / (float)Aspect * scale));

    public Requested<TItem> Requested(float scale) => Tiles.Requested(Shown, Pixels(scale));

    public Kept<TItem> Pruned => Tiles.Kept(toHashSet(Shown.Map(static entry => entry.Key)));

    private Node.Overlay Tiled(TItem item, Key key, int number) =>
        new(new ImageTile(Tiles.Shown(key)).Drawn, [
            new Node.Flex(PhysicalAxis.Vertical, [
                new Node.Flex(PhysicalAxis.Horizontal, (Numbered ? Some<Node>(new Node.Text([number.ToString(CultureInfo.InvariantCulture)]) { Role = Role.OverlayCaption }) : None).ToSeq()
                    .Add(new Node.Box { Width = Sizing.Grow.Unit }) + Overlay(item)) { Width = Sizing.Grow.Unit },
                new Node.Box { Height = Sizing.Grow.Unit },
                new Node.Text([Label(item)]) { Role = Role.OverlayCaption, MaxLines = 1, Width = Sizing.Grow.Unit },
            ]) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
        ]) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = Current == Some(key) ? [Condition.Modified] : [],
            Accessible = new Accessible { Label = Label(item), Shortcut = Numbered && number <= 9 ? Some(number.ToString(CultureInfo.InvariantCulture)) : None },
        };

    private Option<Node> Resizer =>
        Resized.Map(resized => {
            PlotPlane.Track plane = new(PhysicalAxis.Horizontal, new Axis.Linear(TileSizes));
            return new Handles<float>(Side, Points<float>.Scalar(plane, Lens.identity<float>()), resized.Edited) { Decimals = 0 }
                .Drawn(plane, [new Node.Box { Role = Role.Track, Width = Sizing.Grow.Unit }]) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Fit.Content,
                Accessible = new Accessible { Label = resized.Label },
            };
        });
}

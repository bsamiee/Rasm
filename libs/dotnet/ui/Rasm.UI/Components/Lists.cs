using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldRow(string Label, Node Value) {
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Node> Trail { get; init; }

    private Node.Text Captioned => new([Label]) { Role = Role.Label, Conditions = Conditions };

    private Node Valued =>
        new Node.Flex(PhysicalAxis.Horizontal, (Value with { Accessible = Value.Accessible.Combine(new Accessible { Label = Label }) }).Cons(Trail.ToSeq())) {
            Key = new Key.Named(Label),
            Width = Sizing.Grow.Unit,
            Conditions = Conditions,
        };

    public static Node Drawn(Seq<FieldRow> rows) =>
        new Node.Choice(PhysicalAxis.Horizontal, [
            new Node.Grid(new Tracks.Explicit([Sizing.Fit.Content, Sizing.Grow.Unit]), rows.Map(static row => Seq(row.Captioned, row.Valued))),
            new Node.Grid(new Tracks.Explicit([Sizing.Grow.Unit]), rows.Bind(static row => Seq(Seq<Node>(row.Captioned), Seq(row.Valued)))),
        ]);
}

public sealed record Tile<TItem>(Content<SKImage> Content, TItem Item, int Revision, SKSizeI Pixels) where TItem : notnull;

public sealed record TileCache<TItem>(HashMap<Key, Tile<TItem>> Held, int Revision) where TItem : notnull {
    public static TileCache<TItem> Empty { get; } = new([], 0);

    public TileCache<TItem> Reloaded => this with { Revision = Revision + 1 };

    public Content<SKImage> Shown(Key key) => Held.Find(key).Map(static tile => tile.Content).IfNone(static () => new Content<SKImage>.Pending(None, None));

    public (TileCache<TItem> Cache, Seq<(Key Key, Tile<TItem> Tile)> Wanted) Requested(Seq<(Key Key, TItem Item)> shown, SKSizeI pixels) =>
        shown.Filter(entry => !Held.Find(entry.Key).Exists(tile => tile.Pixels == pixels && tile.Revision == Revision && EqualityComparer<TItem>.Default.Equals(tile.Item, entry.Item)))
            .Map(entry => (entry.Key, Tile: new Tile<TItem>(Shown(entry.Key).Requested(None), entry.Item, Revision, pixels))).Strict() switch {
                var wanted => (this with { Held = wanted.Fold(Held, static (held, ask) => held.AddOrUpdate(ask.Key, ask.Tile)) }, wanted),
            };

    public (TileCache<TItem> Cache, Seq<SKImage> Released) Arrived(Key key, Tile<TItem> asked, Fin<SKImage> arrival) =>
        Held.Find(key) == Some(asked)
            ? Released(this with { Held = Held.AddOrUpdate(key, asked with { Content = Content<SKImage>.Arrived(arrival) }) })
            : (this, arrival.ToOption().ToSeq());

    public (TileCache<TItem> Cache, Seq<SKImage> Released) Kept(LanguageExt.HashSet<Key> keys) => Released(this with { Held = Held.Filter((key, _) => keys.Contains(key)) });

    private Seq<SKImage> Images => toSeq(Held.Values).Bind(static tile => tile.Content.Held.ToSeq());

    private (TileCache<TItem> Cache, Seq<SKImage> Released) Released(TileCache<TItem> next) => (next, Images.Filter(image => !next.Images.Exists(kept => ReferenceEquals(kept, image))));
}

public sealed record Gallery<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, string> Label, TileCache<TItem> Tiles, Func<Seq<Key>, Part.Choice> Choice) where TItem : notnull {
    public Option<float> TileSize { get; init; }
    public AxisRange TileSizes { get; init; } = AxisRange.Create(32f, 256f);
    public Factor Aspect { get; init; } = Factor.Unit;
    public LanguageExt.HashSet<Key> Realized { get; init; }
    public Option<Key> Current { get; init; }
    public bool Numbered { get; init; }
    public Func<TItem, Seq<Node>> Overlay { get; init; } = static _ => [];
    public Option<(string Label, Func<Edit<float>, IO<Unit>> Edited)> Resized { get; init; }
    public Option<string> Empty { get; init; }

    private float Side => float.Clamp(TileSize.IfNone(96f), TileSizes.Low, TileSizes.High);

    private Seq<(Key Key, TItem Item)> Shown => Items.Map(item => (Key: Keyed(item), Item: item)).Filter(entry => Realized.Contains(entry.Key));

    public Node Drawn =>
        Items.Map((item, index) => (Key: Keyed(item), Item: item, Number: index + 1)).Strict() switch {
            var entries => toHashMap(entries.Map(static entry => (entry.Key, entry))) switch {
                var keyed => new Node.Flex(PhysicalAxis.Vertical, Resizer.ToSeq().Add(Empty.Filter(_ => Items.IsEmpty).Match(
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
                },
            },
        };

    public SKSizeI Pixels(float scale) => new((int)MathF.Ceiling(Side * scale), (int)MathF.Ceiling(Side / (float)Aspect * scale));

    public (TileCache<TItem> Tiles, Seq<(Key Key, Tile<TItem> Tile)> Wanted) Requested(float scale) => Tiles.Requested(Shown, Pixels(scale));

    public (TileCache<TItem> Tiles, Seq<SKImage> Released) Pruned => Tiles.Kept(toHashSet(Shown.Map(static entry => entry.Key)));

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
        Resized.Map(resized => new PlotPlane.Track(PhysicalAxis.Horizontal, new Axis.Linear(TileSizes)) switch {
            var plane => new Handles<float>(Side, Points<float>.Scalar(plane, Lens.identity<float>(), static extent => extent.ToString("0", CultureInfo.InvariantCulture)), resized.Edited)
                .Drawn(plane, [new Node.Box { Role = Role.Track, Width = Sizing.Grow.Unit }]) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Fit.Content,
                Accessible = new Accessible { Label = resized.Label },
            },
        });
}

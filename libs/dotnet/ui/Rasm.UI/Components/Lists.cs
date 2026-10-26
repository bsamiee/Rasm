using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldRow(Key Key, Seq<Span> Caption, Node Value) {
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Node> Trail { get; init; }

    private Node.Text Captioned => new(Caption) { Role = Role.Label, Conditions = Conditions };

    private Node Valued =>
        new Node.Flex(Orientation.Horizontal, (Value with { Accessible = Value.Accessible.Combine(new Accessible { Label = Captioned.Content }) }).Cons(Trail.ToSeq())) {
            Key = Key,
            Width = Sizing.Grow.Unit,
            Conditions = Conditions,
        };

    public static FieldRow Of<TRecord>(IStateParameter<TRecord> parameter, Node value) =>
        new(new Key.Named(parameter.Key), [parameter.Text.Caption], value with { Accessible = value.Accessible.Combine(new Accessible { Help = parameter.Text.Help }) });

    public static Node Drawn(Seq<FieldRow> rows) =>
        new Node.Choice(Orientation.Horizontal, [
            new Node.Grid(new Tracks.Explicit([Sizing.Fit.Content, Sizing.Grow.Unit]), rows.Map(static row => Seq(row.Captioned, row.Valued))),
            new Node.Grid(new Tracks.Explicit([Sizing.Grow.Unit]), rows.Bind(static row => Seq(Seq<Node>(row.Captioned), Seq(row.Valued)))),
        ]);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileArrangement : ICaptioned {
    public static readonly TileArrangement Grid = new("grid", 96f, Role.OverlayCaption,
        static tile => new Tracks.AutoFill(new Sizing.Fixed(tile)),
        static (_, image, caption, number, marks) => new Node.Overlay(image, [
            new Node.Flex(Orientation.Vertical, [
                new Node.Flex(Orientation.Horizontal, number.ToSeq().Add(new Node.Box { Width = Sizing.Grow.Unit }) + marks) { Width = Sizing.Grow.Unit },
                new Node.Box { Height = Sizing.Grow.Unit },
                caption,
            ]) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
        ]));
    public static readonly TileArrangement List = new("list", 48f, Role.Label,
        static _ => new Tracks.Explicit([Sizing.Grow.Unit]),
        static (tile, image, caption, number, marks) => new Node.Flex(Orientation.Horizontal, Seq(image with { Width = new Sizing.Fixed(tile) }) + number.ToSeq() + Seq(caption) + marks));

    public float Extent { get; }
    public Role Captioned { get; }

    [UseDelegateFromConstructor]
    public partial Tracks Across(float tile);

    [UseDelegateFromConstructor]
    public partial Node Tiled(float tile, Node image, Node caption, Option<Node> number, Seq<Node> marks);
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

public sealed record GalleryMark<TItem>(string Caption, GlyphSource On, GlyphSource Off, Func<TItem, bool> Marked, Func<TItem, bool, IO<Unit>> Toggled) where TItem : notnull;

public sealed record GalleryRename(Option<Key> Active, Func<Key, IO<Unit>> Opened, Func<Key, Edit<string>, IO<Unit>> Edited);

public sealed record Gallery<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, string> Label, TileCache<TItem> Tiles, Func<Seq<Key>, Part.Choice> Choice) where TItem : notnull {
    public TileArrangement Arrangement { get; init; } = TileArrangement.Grid;
    public Option<float> TileSize { get; init; }
    public AxisRange TileSizes { get; init; } = AxisRange.Create(32f, 256f);
    public Factor Aspect { get; init; } = Factor.Unit;
    public LanguageExt.HashSet<Key> Realized { get; init; }
    public Option<Key> Hovered { get; init; }
    public Option<Key> Current { get; init; }
    public bool Numbered { get; init; }
    public Seq<GalleryMark<TItem>> Marks { get; init; }
    public Option<GalleryRename> Renames { get; init; }
    public Option<Func<Edit<float>, IO<Unit>>> Resized { get; init; }
    public Option<string> Empty { get; init; }

    private float Side => float.Clamp(TileSize.IfNone(Arrangement.Extent), TileSizes.Low, TileSizes.High);

    private Seq<(Key Key, TItem Item)> Shown => Items.Map(item => (Key: Keyed(item), Item: item)).Filter(entry => Realized.Contains(entry.Key));

    public Node Drawn =>
        Items.Map((item, index) => (Key: Keyed(item), Item: item, Number: index + 1)).Strict() switch {
            var entries => toHashMap(entries.Map(static entry => (entry.Key, entry))) switch {
                var keyed => new Node.Flex(Orientation.Vertical, Resizer.ToSeq().Add(Empty.Filter(_ => Items.IsEmpty).Match(
                    Some: static empty => (Node)new Node.Text([empty]) { Role = Role.Label, Align = UnitPoint.Center, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
                    None: () => new Node.Scroll(Orientation.Vertical, Choice(entries.Map(static entry => entry.Key)).Drawn(
                        key => Tiled(keyed[key].Item, key, keyed[key].Number), Orientation.Vertical, Arrangement.Across(Side), Side / (float)Aspect, Realized) with {
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

    private Node Tiled(TItem item, Key key, int number) =>
        Arrangement.Tiled(
            Side,
            new ImageTile(Tiles.Shown(key)).Drawn,
            Captioned(item, key),
            Numbered ? Some<Node>(new Node.Text([number.ToString(CultureInfo.InvariantCulture)]) { Role = Arrangement.Captioned }) : None,
            Marks.Choose(mark => mark.Marked(item) switch {
                var on when on || Hovered == Some(key) => Some<Node>(new Part.Press(_ => mark.Toggled(item, !on)).Drawn(new Node.Glyph(on ? mark.On : mark.Off) {
                    Accessible = AccessRole.CheckBox.Marked(new Accessible { Role = AccessRole.CheckBox, Label = mark.Caption }, on ? Check.On : Check.Off),
                })),
                _ => Option<Node>.None,
            })) with {
            Role = Role.ListRow,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = Current == Some(key) ? [Condition.Modified] : [],
            Accessible = new Accessible { Label = Label(item), Shortcut = Numbered && number <= 9 ? Some(number.ToString(CultureInfo.InvariantCulture)) : None },
        };

    private Node Captioned(TItem item, Key key) =>
        (Rename: Renames, Text: new Node.Text([Label(item)]) { Role = Arrangement.Captioned, MaxLines = 1, Width = Sizing.Grow.Unit }) switch {
            { Rename.Case: GalleryRename rename } held when rename.Active == Some(key) => new Part.Field(edit => rename.Edited(key, edit)).Drawn(held.Text with { Role = Role.Field }),
            { Rename.Case: GalleryRename rename } held when Current == Some(key) => new Part.Press(_ => rename.Opened(key)).Drawn(held.Text),
            var held => held.Text,
        };

    private Option<Node> Resizer =>
        Resized.Map(resized => new PlotPlane.Track(Orientation.Horizontal, new Axis.Linear(TileSizes)) switch {
            var plane => new Handles<float>(Side, Points<float>.Scalar(plane, Lens.identity<float>(), static extent => extent.ToString("0", CultureInfo.InvariantCulture)), resized)
                .Drawn(plane, [new Node.Box { Role = Role.Track, Width = Sizing.Grow.Unit }]) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Fit.Content,
                Accessible = new Accessible { Label = Captions.Of(nameof(TileSize)) },
            },
        });
}

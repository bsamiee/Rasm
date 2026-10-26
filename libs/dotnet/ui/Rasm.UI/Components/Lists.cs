using System.Globalization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public Fraction Fraction => Fraction.Create((float)((double)Done / Total));

    public static Option<Progress> Present(long done, long total) => TryCreate(done, total, out Progress? progress) ? progress : None;

    public static Node Meter(Option<Progress> work) =>
        new Node.Flex(PhysicalAxis.Horizontal, work.Map(static progress => (Node)new Node.Box { Role = Role.Span, Width = new Sizing.Percent(progress.Fraction), Height = Sizing.Grow.Unit }).ToSeq()) {
            Role = Role.Track,
            Width = Sizing.Grow.Unit,
            Conditions = [Condition.Busy],
            Accessible = new Accessible { Role = AccessRole.ProgressBar, Value = work.Map(static progress => (AccessValue)new AccessValue.Ranged(progress.Done, 0d, progress.Total)) },
        };

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref long done, ref long total) =>
        validationError = total > 0L
            ? done is >= 0L && done <= total ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"progress {done} lies outside [0, {total}]"))
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"progress total {total} is not positive"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Content<T> where T : notnull {
    public Option<T> Held => Switch(pending: static pending => pending.Prior, ready: static ready => Some(ready.Value), empty: static _ => Option<T>.None, failed: static _ => Option<T>.None);

    public Content<T> Requested(Option<Progress> work) => new Pending { Prior = Held, Work = work };

    public static Content<T> Arrived(Fin<T> arrival) => arrival.Match<Content<T>>(Succ: static value => new Ready(value), Fail: static error => new Failed(error));

    public Node Drawn(Func<T, Node> ready, string unexpected) =>
        Switch(
            (Ready: ready, Unexpected: unexpected),
            pending: static (held, pending) => pending.Prior.Map(value => held.Ready(value).Conditioned(Condition.Busy)).IfNone(() => Progress.Meter(pending.Work)),
            ready: static (held, ready) => held.Ready(ready.Value),
            empty: static (_, empty) => new Node.Text([empty.Reason]) { Role = Role.Label },
            failed: static (held, failed) => new Node.Text([failed.Error.IsExpected ? failed.Error.Message : held.Unexpected]) { Role = Role.Label, Conditions = [Condition.Faulted] });

    public sealed record Pending : Content<T> {
        public Option<T> Prior { get; init; }
        public Option<Progress> Work { get; init; }
    }

    public sealed record Ready(T Value) : Content<T>;

    public sealed record Empty(string Reason) : Content<T>;

    public sealed record Failed(Error Error) : Content<T>;
}

public sealed record Row<TItem>(TItem Item, int Depth, Option<bool> Open) where TItem : notnull {
    public static Seq<Row<TItem>> Flatten<TKey>(Seq<TItem> roots, Func<TItem, Seq<TItem>> children, Func<TItem, TKey> key, LanguageExt.HashSet<TKey> open, Func<TItem, bool> keep) where TKey : notnull {
        return Rows(roots.Filter(Holds), 0);

        bool Holds(TItem item) => keep(item) || children(item).Exists(Holds);

        Seq<Row<TItem>> Rows(Seq<TItem> items, int depth) =>
            items.Bind(item => (Opened: open.Contains(key(item)), Below: children(item).Filter(Holds)) switch {
                var held => new Row<TItem>(item, depth, held.Below.IsEmpty ? None : Some(held.Opened)).Cons(held.Opened ? Rows(held.Below, depth + 1) : []),
            });
    }
}

public sealed record Toast(Key Key, Node Content, Option<Duration> Lifetime);

public sealed record Toasts(Seq<Toast> Shown, Role Role, Func<Key, IO<Unit>> Dismissed) {
    public UnitPoint Corner { get; init; } = new(Fraction.MaxValue, Fraction.MaxValue);
    public int Cap { get; init; } = 3;

    public Node Over(Node beneath) =>
        new Node.Overlay(beneath, [
            new Node.Flex(PhysicalAxis.Vertical, Shown.Skip(int.Max(0, Shown.Count - Cap)).Map(toast => (Node)new Node.Flex(PhysicalAxis.Horizontal, [toast.Content]) {
                Key = Some(toast.Key),
                Role = Role,
                Expires = toast.Lifetime.Map(lifetime => new Expiry(lifetime, Dismissed(toast.Key))),
                Accessible = new Accessible { Live = Live.Polite },
            })) {
                Align = Corner,
            },
        ]);
}

public sealed record Tile<TItem>(Content<SKImage> Content, TItem Item, int Revision, SKSizeI Pixels) where TItem : notnull;

public sealed record TileCache<TItem>(HashMap<Key, Tile<TItem>> Held, int Revision) where TItem : notnull {
    public static TileCache<TItem> Empty { get; } = new([], 0);

    public TileCache<TItem> Reloaded => this with { Revision = Revision + 1 };

    public Content<SKImage> Shown(Key key) => Held.Find(key).Map(static tile => tile.Content).IfNone(static () => new Content<SKImage>.Pending());

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
            var plane => new Handles<float>(Side, Points<float>.Scalar(plane, Lens.identity<float>()), resized.Edited) { Decimals = 0 }
                .Drawn(plane, [new Node.Box { Role = Role.Track, Width = Sizing.Grow.Unit }]) with {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Fit.Content,
                Accessible = new Accessible { Label = resized.Label },
            },
        });
}

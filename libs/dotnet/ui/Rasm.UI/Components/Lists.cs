using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }
    public Fraction Fraction { get; }

    public static Fin<Progress> Validated(long done, long total) =>
        UIFault.Unless(total > 0L && done >= 0L && done <= total, UIReason.Range, (done, total)).Bind(_ => Fraction.Validated((float)((double)done / total))).Map(fraction => new Progress(done, total, fraction));

    public static Node Meter(Option<Progress> work) =>
        new Node.Flex(PhysicalAxis.Horizontal, work.Map(static progress => (Node)new Node.Box { Role = Role.Span, Width = new Sizing.Percent(progress.Fraction), Height = Sizing.Grow.Unit }).ToSeq()) {
            Role = Role.Track,
            Width = Sizing.Grow.Unit,
            Conditions = [Condition.Busy],
            Accessible = new Accessible { Role = AccessRole.ProgressBar, Value = work.Map(static progress => (AccessValue)new AccessValue.Ranged(progress.Done, 0d, progress.Total)) },
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Content<T> where T : notnull {
    public Option<T> Current => Switch(pending: static pending => pending.Prior, ready: static ready => Some(ready.Value), empty: static _ => Option<T>.None, failed: static _ => Option<T>.None);

    public Content<T> Requested(Option<Progress> work) => new Pending { Prior = Current, Work = work };

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
    public static Seq<Row<TItem>> Flatten<TKey>(Seq<TItem> roots, Func<TItem, Seq<TItem>> children, Func<TItem, TKey> key, HashSet<TKey> open, Func<TItem, bool> keep) where TKey : notnull {
        return Rows(roots.Filter(Holds), 0);

        bool Holds(TItem item) => keep(item) || children(item).Exists(Holds);

        Seq<Row<TItem>> Rows(Seq<TItem> items, int depth) =>
            items.Bind(item => {
                bool opened = open.Contains(key(item));
                Seq<TItem> below = children(item).Filter(Holds);
                return new Row<TItem>(item, depth, below.IsEmpty ? None : Some(opened)).Cons(opened ? Rows(below, depth + 1) : []);
            });
    }
}

public sealed record Toast(Key Key, Node Content, Option<Duration> Lifetime);

public sealed record Toasts(Seq<Toast> Shown, Role Role, Func<Key, IO<Unit>> Dismissed) {
    public UnitPoint Corner { get; init; } = new(Fraction.MaxValue, Fraction.MaxValue);
    public int Limit { get; init; } = 3;

    public Node Overlaid(Node beneath) =>
        new Node.Overlay(beneath, [
            new Node.Flex(PhysicalAxis.Vertical, Shown.Skip(int.Max(0, Shown.Count - Limit)).Map(toast => (Node)new Node.Flex(PhysicalAxis.Horizontal, [toast.Content]) {
                Key = Some(toast.Key),
                Role = Role,
                Expires = toast.Lifetime.Map(lifetime => new Expiry(lifetime, Dismissed(toast.Key))),
                Accessible = new Accessible { Live = Live.Polite },
            })) {
                Align = Corner,
            },
        ]);
}

public sealed record Swatch(SKColorF Color, Func<SKRect, IO<Unit>> Pressed) {
    public Node Drawn => new Part.Press(Pressed).Drawn(Patch(Color));

    public static Node Patch(SKColorF color) =>
        new Node.Styled(new Node.Box { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
            Sheet = new StyleSheet { Looks = HashMap((Role.Field, new Look { Fills = Some(Seq<Material>(new Material.Solid(new ColorValue.Fixed(color.ToUnicolour())))) })) },
            Accessible = new Accessible { Value = new AccessValue.Text(color.ToUnicolour().Hex) },
        };
}

public sealed record SwatchList(Seq<SKColorF> Colors, Selection Selection, Func<Edit<Selection>, IO<Unit>> Selected) {
    public Node Drawn {
        get {
            Seq<(Key Key, SKColorF Color)> keyed = Colors.Map(static (color, index) => ((Key)new Key.Ordinal((uint)index), color));
            HashMap<Key, SKColorF> faces = toHashMap(keyed);
            return new Part.Choice(keyed.Map(static held => held.Key), Selection, Selected)
                .Drawn(key => Swatch.Patch(faces[key]), static items => new Node.Flex(PhysicalAxis.Horizontal, items) { Width = Sizing.Grow.Unit });
        }
    }
}

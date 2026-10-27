using System.Globalization;
using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Progress {
    private static readonly PlotPlane.Polar Clockwise = new(new Axis.Linear(AxisRange.Validated(0.25f, -0.75f).ThrowIfFail()), Some<Axis>(new Axis.Linear(AxisRange.Unit)));
    private static readonly AxisRange Annulus = AxisRange.Validated((float)Fraction.Half, 1f).ThrowIfFail();

    public long Done { get; }
    public long Total { get; }

    public Fraction Fraction => Fraction.Validated((float)((double)Done / Total)).ThrowIfFail();

    public static Fin<Progress> Validated(long done, long total) => UIFault.Unless(total > 0L && done >= 0L && done <= total, UIReason.Range, (done, total)).Map(_ => new Progress(done, total));

    public static Node Meter(Option<Progress> work) =>
        new Node.Flex(PhysicalAxis.Horizontal, work.Map(static progress => (Node)new Node.Box { Role = Role.Span, Width = new Sizing.Percent(progress.Fraction), Height = Sizing.Grow.Unit }).ToSeq()) {
            Role = Role.Track,
            Width = Sizing.Grow.Unit,
            Conditions = [Condition.Busy],
            Accessible = Announced(work),
        };

    public static Node Ring(Option<Progress> work) =>
        work.Match(
            Some: static progress => (Node)new Node.Plot(Clockwise, Seq<Node>(new Node.Mark(new Figure.Region { Second = Annulus }) { Role = Role.Track }) + progress.Swept) {
                Role = Role.Spinner,
                Accessible = Announced(Some(progress)),
            },
            None: static () => new Node.Box { Role = Role.Spinner, Conditions = [Condition.Busy], Accessible = Announced(None) });

    private Seq<Node> Swept =>
        AxisRange.Validated(0f, (float)((double)Done / Total)).ToOption().Map(static span => (Node)new Node.Mark(new Figure.Region { First = span, Second = Annulus }) { Role = Role.Span }).ToSeq();

    private static Accessible Announced(Option<Progress> work) =>
        new() { Role = AccessRole.ProgressBar, Value = work.Map(static progress => (AccessValue)new AccessValue.Ranged(progress.Done, 0d, progress.Total)) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Content<T> where T : notnull {
    public Option<T> Current => Switch(pending: static pending => pending.Prior, ready: static ready => Some(ready.Value), empty: static _ => Option<T>.None, failed: static _ => Option<T>.None);

    public HashSet<Condition> Conditions => Map<HashSet<Condition>>(pending: [Condition.Busy], ready: [], empty: [], failed: [Condition.Faulted]);

    public Option<string> Message =>
        Switch(
            pending: static _ => Option<string>.None,
            ready: static _ => Option<string>.None,
            empty: static empty => Some(empty.Reason),
            failed: static failed => failed.Error.IsExpected ? Some(failed.Error.Message) : None);

    public Content<T> Requested(Option<Progress> work) => new Pending { Prior = Current, Work = work };

    public static Content<T> Arrived(Fin<T> arrival) => arrival.Match<Content<T>>(Succ: static value => new Ready(value), Fail: static error => new Failed(error));

    public Node Drawn(Func<T, Node> ready, string unexpected) =>
        Switch(
            (Ready: ready, Said: new Node.Text([Message.IfNone(unexpected)]) { Role = Role.Label, Conditions = Conditions }),
            pending: static (held, pending) => pending.Prior.Map(value => held.Ready(value).Conditioned(Condition.Busy)).IfNone(() => Progress.Meter(pending.Work)),
            ready: static (held, ready) => held.Ready(ready.Value),
            empty: static (held, _) => held.Said,
            failed: static (held, _) => held.Said);

    public sealed record Pending : Content<T> {
        public Option<T> Prior { get; init; }
        public Option<Progress> Work { get; init; }
    }

    public sealed record Ready(T Value) : Content<T>;

    public sealed record Empty(string Reason) : Content<T>;

    public sealed record Failed(Error Error) : Content<T>;
}

public sealed record Row<TItem>(TItem Item, Key Key, int Depth, Option<bool> Open) where TItem : notnull {
    public static Seq<Row<TItem>> Flatten(Seq<TItem> roots, Func<TItem, Seq<TItem>> children, Func<TItem, Key> key, HashSet<Key> open, Func<TItem, bool> keep) {
        return Rows(roots.Filter(Holds), 0);

        bool Holds(TItem item) => keep(item) || children(item).Exists(Holds);

        Seq<Row<TItem>> Rows(Seq<TItem> items, int depth) =>
            items.Bind(item => {
                Key keyed = key(item);
                bool opened = open.Contains(keyed);
                Seq<TItem> below = children(item).Filter(Holds);
                return new Row<TItem>(item, keyed, depth, below.IsEmpty ? None : Some(opened)).Cons(opened ? Rows(below, depth + 1) : []);
            });
    }

    public static Node.Lazy Keyed(Seq<Row<TItem>> rows, Func<Seq<Key>, Part.Choice> choice, Func<Row<TItem>, Node> drawn, float extent, HashSet<Key> realized) {
        HashMap<Key, Row<TItem>> keyed = toHashMap(rows.Map(static row => (row.Key, row)));
        return choice(rows.Map(static row => row.Key)).Drawn(
            key => keyed[key] switch { var row => drawn(row) with { Sticks = row.Open.Map(static _ => PhysicalAxis.Vertical) } },
            PhysicalAxis.Vertical, new Tracks.Explicit([Sizing.Grow.Unit]), extent, realized);
    }
}

public sealed record Search {
    private const CompareOptions Folded = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public Search(string pattern) => (Pattern, Segments) = (pattern, toSeq(pattern.Split('*', StringSplitOptions.RemoveEmptyEntries)));

    public string Pattern { get; }
    private Seq<string> Segments { get; }

    public bool Matches(string text, CultureInfo culture) => Found(text, culture).IsSome;

    public Seq<Span> Marked(string text, CultureInfo culture) =>
        Found(text, culture).Match(
            Some: hits => hits.Fold((At: 0, Spans: Seq<Span>()), (held, hit) => (hit.Start + hit.Length, held.Spans + Plain(text[held.At..hit.Start]).Add(new Span(text.Substring(hit.Start, hit.Length)) { Role = Role.Mark })))
                switch { var held => held.Spans + Plain(text[held.At..]), },
            None: () => [text]);

    private static Seq<Span> Plain(string text) => text.Length > 0 ? [text] : [];

    private Option<Seq<Hit>> Found(string text, CultureInfo culture) =>
        Segments.FoldM((At: 0, Hits: Seq<Hit>()), (held, segment) => {
            int index = culture.CompareInfo.IndexOf(text.AsSpan(held.At), segment.AsSpan(), Folded, out int length);
            return index < 0 ? Option<(int At, Seq<Hit> Hits)>.None : Some((held.At + index + length, held.Hits.Add(new Hit(held.At + index, length))));
        }).As().Map(static held => held.Hits);

    private sealed record Hit(int Start, int Length);
}

public sealed record Toast(Key Key, Node Content, Option<Duration> Lifetime);

public sealed record Toasts(Seq<Toast> Shown, Role Role, Func<Key, IO<Unit>> Dismissed) {
    private static readonly Key Surface = new Key.Ordinal(0);
    private static readonly PlotPlane Square = new PlotPlane.Cartesian(new Axis.Linear(AxisRange.Unit), new Axis.Linear(AxisRange.Unit));
    private static readonly Points<bool> Swiped = new(static _ => [], static (shown, _, _) => new Dragged<bool>(shown, [])) {
        Placed = static (shown, _, _) => Some(new Added<bool>(shown, Surface)),
        DraggedOut = Some<Func<bool, Key, Option<bool>>>(static (_, _) => Some(value: false)),
    };

    public UnitPoint Corner { get; init; } = new(Fraction.MaxValue, Fraction.MaxValue);
    public int Limit { get; init; } = 3;

    public Node.Layer Drawn => new(None, new Node.Flex(PhysicalAxis.Vertical, toSeq(Shown.TakeLast(Limit)).Map(Toasted)) { Align = Corner }) { Side = Corner, Align = Corner };

    private Node Toasted(Toast toast) =>
        new Node.Plot(Square, [
            new Handles<bool>(Value: true, Swiped, edit => when(edit.Phase == EditPhase.Commit && edit.Value.Exists(static shown => !shown), Dismissed(toast.Key)).As()).Drawn(Square, []),
            new Node.Flex(PhysicalAxis.Horizontal, [toast.Content]) { Role = Role, Accessible = new Accessible { Role = AccessRole.Status, Live = Live.Polite } },
        ]) {
            Key = toast.Key,
            Expires = toast.Lifetime.Map(lifetime => new Expiry(lifetime, Dismissed(toast.Key))),
        };
}

public sealed record Swatch(SKColorF Color, Func<SKRect, IO<Unit>> Pressed) {
    public Option<Func<SKColorF, IO<Unit>>> Dropped { get; init; }

    public Node Drawn =>
        new Part.Press(Pressed).Drawn(Patch(Color)) with {
            Lift = new Lift(_ => IO.pure<Transfer>(new Transfer.Color(Color))),
            Drop = Dropped.Map(static dropped => new Drop((transfer, _, _) => Colored(transfer).Map(dropped))),
        };

    public static Node Patch(SKColorF color) =>
        new Node.Styled(new Node.Box { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) {
            Sheet = new StyleSheet { Looks = HashMap((Role.Field, new Look { Fills = Some(Seq<Material>(new Material.Solid(new ColorValue.Fixed(color.ToUnicolour())))) })) },
            Accessible = new Accessible { Value = new AccessValue.Text(color.ToUnicolour().Hex) },
        };

    internal static Option<SKColorF> Colored(Transfer transfer) =>
        transfer.Switch(keys: static _ => Option<SKColorF>.None, files: static _ => Option<SKColorF>.None, text: static _ => Option<SKColorF>.None, color: static color => Some(color.Value));
}

public sealed record SwatchList(Seq<SKColorF> Colors, Selection Selection, Func<Edit<Selection>, IO<Unit>> Selected) {
    public Option<Func<Edit<Seq<SKColorF>>, IO<Unit>>> Edited { get; init; }

    public Node Drawn {
        get {
            Seq<(Key Key, SKColorF Color)> keyed = Colors.Map(static (color, index) => ((Key)new Key.Ordinal((uint)index), color)).Strict();
            HashMap<Key, SKColorF> faces = toHashMap(keyed);
            Seq<Key> keys = keyed.Map(static held => held.Key);
            return new Part.Choice(keys, Selection, Selected).Drawn(key => Swatch.Patch(faces[key]), static items => new Node.Flex(PhysicalAxis.Horizontal, items) { Width = Sizing.Grow.Unit }) with {
                Lift = new Lift(moved => IO.lift(moved.Head.Bind(faces.Find).Map(static color => (Transfer)new Transfer.Color(color)).ToFin(new UIFault(moved.ToString(), UIReason.Missing)))),
                Drop = Edited.Map(edited => new Drop((transfer, target, placement) => Swatch.Colored(transfer).Map(color => edited(Edit.Commit(Inserted(keys, color, target, placement)))))),
            };
        }
    }

    private Seq<SKColorF> Inserted(Seq<Key> keys, SKColorF color, Option<Key> target, Placement placement) =>
        target.Bind(key => Selection.Position(keys, key)).Match(
            Some: index => placement.Map(
                before: Colors.Take(index) + Seq(color) + Colors.Skip(index),
                inside: Colors.Take(index) + Seq(color) + Colors.Skip(index + 1),
                after: Colors.Take(index + 1) + Seq(color) + Colors.Skip(index + 1)),
            None: () => Colors.Add(color));
}

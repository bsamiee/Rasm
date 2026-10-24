using System.Globalization;
using Rasm.UI.Styling;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public double Fraction => (double)Done / Total;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long done, ref long total) =>
        validationError = total > 0L
            ? ValidationResults.OutsideRange(done, 0L, total)
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"progress total {total} is not positive"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Content<T> where T : notnull {
    public Option<T> Held => Switch(pending: static pending => pending.Prior, ready: static ready => Some(ready.Value), empty: static _ => Option<T>.None, failed: static _ => Option<T>.None);

    public Content<T> Requested(Option<Progress> work) => new Pending(Held, work);

    public static Content<T> Arrived(Fin<T> arrival) => arrival.Match<Content<T>>(Succ: static value => new Ready(value), Fail: static error => new Failed(error));

    public Node Drawn(Func<T, Node> ready, string unexpected) =>
        Switch(
            (Ready: ready, Unexpected: unexpected),
            pending: static (held, pending) => pending.Prior.Map(value => (Node)Kept(held.Ready(value), [Veil])).IfNone(() => Meter(pending.Work)),
            ready: static (held, ready) => Kept(held.Ready(ready.Value), []),
            empty: static (_, empty) => new Node.Text(empty.Reason) { Role = Role.Label },
            failed: static (held, failed) => new Node.Text(failed.Error.IsExpected ? failed.Error.Message : held.Unexpected) { Role = Role.Label, Conditions = [Condition.Faulted] });

    private static Node.Flex Veil { get; } =
        new(Orientation.Horizontal, []) { Role = Role.Veil, Width = new Sizing.Grow(1f), Height = new Sizing.Grow(1f), Conditions = [Condition.Busy] };

    private static Node.Overlay Kept(Node held, Seq<Node> above) => new(held.Cons(above)) { Key = held.Key, Width = held.Width, Height = held.Height };

    private static Node.Flex Meter(Option<Progress> work) =>
        new(Orientation.Horizontal, work.Map(static progress => (Node)new Node.Flex(Orientation.Horizontal, []) { Role = Role.Span, Width = new Sizing.Percent((float)progress.Fraction), Height = new Sizing.Grow(1f) }).ToSeq()) {
            Role = Role.Track,
            Width = new Sizing.Grow(1f),
            Conditions = [Condition.Busy],
        };

    public sealed record Pending(Option<T> Prior, Option<Progress> Work) : Content<T>;

    public sealed record Ready(T Value) : Content<T>;

    public sealed record Empty(string Reason) : Content<T>;

    public sealed record Failed(Error Error) : Content<T>;
}

public sealed record Row<TItem>(TItem Item, int Depth, Option<bool> Open, bool Visible) where TItem : notnull {
    public static Seq<Row<TItem>> Flatten<TKey>(Seq<TItem> roots, Func<TItem, Seq<TItem>> children, Func<TItem, TKey> key, LanguageExt.HashSet<TKey> open, Func<TItem, bool> keep) where TKey : notnull {
        return Rows(roots, 0, visible: true);

        Seq<Row<TItem>> Rows(Seq<TItem> items, int depth, bool visible) =>
            items.Bind(item => open.Contains(key(item)) switch {
                var opened => Rows(children(item), depth + 1, visible && opened) switch {
                    var below when below.IsEmpty && !keep(item) => Seq<Row<TItem>>(),
                    var below => new Row<TItem>(item, depth, below.IsEmpty ? None : Some(opened), visible).Cons(below),
                },
            });
    }
}

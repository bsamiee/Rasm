using System.Globalization;
using Rasm.UI.Styling;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public Fraction Fraction => Fraction.Create((float)((double)Done / Total));

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

    public Content<T> Requested(Option<Progress> work) => new Pending(Held, work);

    public static Content<T> Arrived(Fin<T> arrival) => arrival.Match<Content<T>>(Succ: static value => new Ready(value), Fail: static error => new Failed(error));

    public Node Drawn(Func<T, Node> ready, string unexpected) =>
        Switch(
            (Ready: ready, Unexpected: unexpected),
            pending: static (held, pending) => pending.Prior.Map(value => held.Ready(value).Conditioned(Condition.Busy)).IfNone(() => Progress.Meter(pending.Work)),
            ready: static (held, ready) => held.Ready(ready.Value),
            empty: static (_, empty) => new Node.Text([empty.Reason]) { Role = Role.Label },
            failed: static (held, failed) => new Node.Text([failed.Error.IsExpected ? failed.Error.Message : held.Unexpected]) { Role = Role.Label, Conditions = [Condition.Faulted] });

    public sealed record Pending(Option<T> Prior, Option<Progress> Work) : Content<T>;

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

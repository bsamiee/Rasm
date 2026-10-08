using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RowRule(Seq<EntryKey> Reads, Seq<RowSource> Sources, Func<RowScope, IO<bool>> Holds) {
    public static RowRule When<TRecord>(RowSource<TRecord> source, Func<TRecord, bool> holds, params Seq<IStateParameter<TRecord>> reads) where TRecord : notnull =>
        new(reads.Bind(source.Keys), Seq<RowSource>(source), scope => scope.Read(source).Map(holds));

    public static RowRule operator &(RowRule left, RowRule right) => BitwiseAnd(left, right);

    public static RowRule operator |(RowRule left, RowRule right) => BitwiseOr(left, right);

    public static RowRule operator !(RowRule rule) => LogicalNot(rule);

    public static RowRule BitwiseAnd(RowRule left, RowRule right) =>
        new(left.Reads.Concat(right.Reads).Distinct(), left.Sources.Concat(right.Sources).Distinct(),
            scope => left.Holds(scope).Bind(held => held ? right.Holds(scope) : IO.pure(value: false)));

    public static RowRule BitwiseOr(RowRule left, RowRule right) =>
        new(left.Reads.Concat(right.Reads).Distinct(), left.Sources.Concat(right.Sources).Distinct(),
            scope => left.Holds(scope).Bind(held => held ? IO.pure(value: true) : right.Holds(scope)));

    public static RowRule LogicalNot(RowRule rule) =>
        rule with { Holds = scope => rule.Holds(scope).Map(static held => !held) };
}

public sealed record RowRules(Option<RowRule> Enabled, Option<RowRule> Visible) {
    public static RowRules Always { get; } = new(None, None);

    public Seq<RowRule> Each => Enabled.ToSeq().Concat(Visible.ToSeq());

    public static IO<bool> Holds(Option<RowRule> rule, RowScope scope) => rule.Match(Some: held => held.Holds(scope), None: static () => IO.pure(value: true));

    public IO<bool> Enables(RowScope scope) => Holds(Enabled, scope);

    public IO<bool> Shows(RowScope scope) => Holds(Visible, scope);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RuleIndex {
    public static HashMap<EntryKey, Seq<T>> Of<T>(Seq<(Seq<EntryKey> Reads, T Target)> rows) =>
        rows.Fold(HashMap<EntryKey, Seq<T>>(), static (index, row) =>
            row.Reads.Fold(index, (held, key) => held.AddOrUpdate(key, targets => targets.Add(row.Target), Seq(row.Target))));

    public static Seq<T> Affected<T>(HashMap<EntryKey, Seq<T>> index, Seq<EntryKey> changed) =>
        changed.Bind(key => index.Find(key).IfNone(Seq<T>())).Distinct();
}

using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RowRule(Seq<EntryKey> Reads, Seq<RowSource> Sources, Func<RowScope, IO<bool>> Holds) {
    public static RowRule When<TRecord>(RowSource<TRecord> source, Func<TRecord, bool> holds, params Seq<IStateParameter<TRecord>> reads) where TRecord : notnull =>
        new(reads.Bind(parameter => source.Keys(parameter)), Seq<RowSource>(source), scope => scope.Read(source).Map(holds));

    public static RowRule operator &(RowRule left, RowRule right) =>
        new(left.Reads.Concat(right.Reads).Distinct(), left.Sources.Concat(right.Sources).Distinct(),
            scope => left.Holds(scope).Bind(held => held ? right.Holds(scope) : IO.pure(false)));

    public static RowRule operator |(RowRule left, RowRule right) =>
        new(left.Reads.Concat(right.Reads).Distinct(), left.Sources.Concat(right.Sources).Distinct(),
            scope => left.Holds(scope).Bind(held => held ? IO.pure(true) : right.Holds(scope)));

    public static RowRule operator !(RowRule rule) =>
        rule with { Holds = scope => rule.Holds(scope).Map(static held => !held) };
}

public sealed record RowRules(Option<RowRule> Enabled, Option<RowRule> Visible) {
    public static RowRules Always { get; } = new(None, None);

    public Seq<RowRule> Each => Enabled.ToSeq().Concat(Visible.ToSeq());

    public static IO<bool> Holds(Option<RowRule> rule, RowScope scope) => rule.Match(Some: held => held.Holds(scope), None: static () => IO.pure(true));

    public IO<bool> Enables(RowScope scope) => Holds(Enabled, scope);

    public IO<bool> Shows(RowScope scope) => Holds(Visible, scope);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RuleIndex {
    public static HashMap<EntryKey, Seq<A>> Of<A>(Seq<(Seq<EntryKey> Reads, A Target)> rows) =>
        rows.Fold(HashMap<EntryKey, Seq<A>>(), static (index, row) =>
            row.Reads.Fold(index, (held, key) => held.AddOrUpdate(key, targets => targets.Add(row.Target), Seq(row.Target))));

    public static Seq<A> Affected<A>(HashMap<EntryKey, Seq<A>> index, Seq<EntryKey> changed) =>
        changed.Bind(key => index.Find(key).IfNone(Seq<A>())).Distinct();
}

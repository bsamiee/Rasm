using Rasm.Rhino.Events;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class Applied {
    private static readonly IO<Unit> Flush =
        from flushed in IO.lift(static () => PlugIn.FlushSettingsSavedQueue())
        from raised in IO.lift(static () => PlugIn.RaiseOnPlugInSettingsSavedEvent())
        select raised;

    public static readonly Applied Live = new(Option<IO<Unit>>.None);
    public static readonly Applied IdleSave = new(Some(Flush));

    public Option<IO<Unit>> Drain { get; }
}

public sealed record ValueStore<T>(IO<Option<T>> Read, Func<Option<T>, IO<Change<T>>> Put, Func<T, T, bool> Same, Applied Applied, Option<HostEvent<Unit>> Changed)
    where T : notnull {
    public IO<Change<T>> Write(Option<T> value) =>
        from applied in Apply(value)
        from drained in applied.Owed.Traverse(static drain => drain).As()
        select applied.Value;

    public IO<Option<IO<Unit>>> Edit(Option<T> value) =>
        Apply(value).Map(static applied => applied.Owed);

    internal IO<(Change<T> Value, Option<IO<Unit>> Owed)> Apply(Option<T> value) =>
        from change in Put(value)
        select (change, Applied.Drain.Filter(_ => change.HasChanged));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueStores {
    // --- [CONSTRUCTION]
    public static ValueStore<T> Of<T>(IO<Option<T>> read, Func<Option<T>, IO<Unit>> put, Applied applied, Option<HostEvent<Unit>> changed, Option<Func<T, T, bool>> same = default) where T : notnull =>
        same.IfNone(EqualityComparer<T>.Default.Equals) switch {
            var equal => new(read, value =>
                from held in read
                let change = Between(held, value, equal)
                from written in when(change.HasChanged, put(value)).As()
                select change,
                equal, applied, changed),
        };

    public static ValueStore<T> Defaulted<T>(IO<T> read, IO<T> factory, Func<T, IO<Unit>> put, Func<T, T, bool> same, Applied applied, Option<HostEvent<Unit>> changed) where T : notnull =>
        new(
            from held in read
            from initial in factory
            select Some(held).Filter(value => !same(value, initial)),
            value =>
                from initial in factory
                from before in read
                let target = value.IfNone(initial)
                let change = Between(Some(before).Filter(held => !same(held, initial)), Some(target).Filter(held => !same(held, initial)), same)
                from written in when(change.HasChanged, put(target)).As()
                select change,
            same, applied, changed);

    public static ValueStore<T> Readout<T>(IO<T> read, Option<HostEvent<Unit>> changed) where T : notnull =>
        new(read.Map(static value => Some(value)), static _ => IO.pure(Change<T>.None), EqualityComparer<T>.Default.Equals, Applied.Live, changed);

    public static Change<T> Between<T>(Option<T> before, Option<T> after, Option<Func<T, T, bool>> same = default) =>
        (before.Case, after.Case) switch {
            (T held, T written) => same.IfNone(EqualityComparer<T>.Default.Equals)(held, written) ? Change<T>.None : Change<T>.Mapped(held, written),
            (T held, _) => Change<T>.Removed(held),
            (_, T written) => Change<T>.Added(written),
            _ => Change<T>.None,
        };

    public static Option<T> Before<T>(Change<T> change) =>
        change switch {
            EntryRemoved<T> removed => removed.OldValue,
            EntryMapped<T, T> mapped => mapped.From,
            _ => None,
        };

    public static Change<TAfter> Map<TBefore, TAfter>(Change<TBefore> change, Func<TBefore, TAfter> project, Func<TAfter, TAfter, bool> same) =>
        Between(Before(change).Map(project), change.ToOption().Map(project), same);

    // --- [COMMIT]
    public static IO<Unit> Commit(Seq<IO<Option<IO<Unit>>>> edits) =>
        Commit(edits.Map(static edit => edit.Map(static owed => (Value: unit, Owed: owed)))).Map(static _ => unit);

    public static IO<Seq<T>> Commit<T>(Seq<IO<(T Value, Option<IO<Unit>> Owed)>> edits, Option<IO<Unit>> owed = default) =>
        from parts in edits.Map<K<IO, (T Value, Option<IO<Unit>> Owed)>>(static edit => edit).PartitionFallible().As()
        from drained in (owed | parts.Succs.Choose(static part => part.Owed).Head).ToSeq().Map<K<IO, Unit>>(static drain => drain).PartitionFallible().As()
        let failures = parts.Fails + drained.Fails
        from failed in unless(failures.IsEmpty, IO.fail<Unit>(Error.Many(failures))).As()
        select parts.Succs.Map(static part => part.Value);

    // --- [SIGNAL]
    public static HostEvent<Unit> Signal<TArgs>(HostEvent<TArgs> row, Func<TArgs, bool> concerns) =>
        row.Choose(args => Callbacks.Found(concerns(args), unit));
}

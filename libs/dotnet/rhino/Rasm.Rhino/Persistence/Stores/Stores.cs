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
    public static readonly Applied Relaunch = new(Some(Flush));

    public Option<IO<Unit>> Drain { get; }
}

public sealed record ValueStore<T>(IO<Option<T>> Read, Func<Option<T>, IO<Unit>> Put, Func<T, T, bool> Same, Applied Applied, Option<HostEvent<Unit>> Changed)
    where T : notnull {
    public IO<Change<T>> Write(Option<T> value) =>
        from applied in Apply(value)
        from drained in applied.Owed.Traverse(static drain => drain).As()
        select applied.Change;

    public IO<Option<IO<Unit>>> Edit(Option<T> value) =>
        Apply(value).Map(static applied => applied.Owed);

    private IO<(Change<T> Change, Option<IO<Unit>> Owed)> Apply(Option<T> value) =>
        from held in Read
        let same = (from before in held from after in value select Same(before, after)).IfNone(noneValue: false)
        let change = same ? Change<T>.None : held.Map(Change<T>.Removed).IfNone(Change<T>.None).Combine(value.Map(Change<T>.Added).IfNone(Change<T>.None))
        from put in when(change.HasChanged, Put(value)).As()
        select (change, Applied.Drain.Filter(_ => change.HasChanged));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueStore {
    // --- [CONSTRUCTION]
    public static ValueStore<T> Of<T>(IO<Option<T>> read, Func<Option<T>, IO<Unit>> put, Applied applied, Option<HostEvent<Unit>> changed) where T : notnull =>
        new(read, put, EqualityComparer<T>.Default.Equals, applied, changed);

    // --- [COMMIT]
    public static IO<Unit> Commit(Seq<IO<Option<IO<Unit>>>> edits) =>
        from parts in edits.PartitionFallible().As()
        from drained in parts.Succs.Somes().Head.ToSeq().PartitionFallible().As()
        let failures = parts.Fails + drained.Fails
        from failed in unless(failures.IsEmpty, IO.fail<Unit>(Error.Many(failures))).As()
        select unit;

    // --- [SIGNAL]
    public static HostEvent<Unit> Signal<TArgs>(HostEvent<TArgs> row, Func<TArgs, bool> concerns) =>
        row.Choose(args => Callbacks.Found(concerns(args), unit));
}

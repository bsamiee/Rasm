using Rasm.Rhino.Events;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Applied {
    private static readonly IO<Unit> Flush =
        from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
        from raised in IO.lift(PlugIn.RaiseOnPlugInSettingsSavedEvent)
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
        from held in Read.Map(static found => Some(found)) | @catch(error => value.IsNone && error.IsExpected, static _ => IO.pure(Option<Option<T>>.None))
        let change = Diff(held.Flatten(), value)
        let written = held.ForAll(_ => change.HasChanged)
        from put in when(written, Put(value)).As()
        select (change, Applied.Drain.Filter(_ => written));

    private Change<T> Diff(Option<T> held, Option<T> value) =>
        held.Match(
            Some: before => value.Match(
                Some: after => Same(before, after) ? Change<T>.None : Change<T>.Mapped(before, after),
                None: () => Change<T>.Removed(before)),
            None: () => value.Match(Some: Change<T>.Added, None: static () => Change<T>.None));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueStore {
    // --- [CONSTRUCTION]
    public static ValueStore<T> Of<T>(IO<Option<T>> read, Func<Option<T>, IO<Unit>> put, Applied applied, Option<HostEvent<Unit>> changed) where T : notnull =>
        new(read, put, EqualityComparer<T>.Default.Equals, applied, changed);

    // --- [COMMIT]
    public static IO<Unit> Commit(Seq<IO<Option<IO<Unit>>>> edits) =>
        from parts in edits.PartitionFallible().As()
        from drained in parts.Succs.Somes().Head.Traverse(static drain => drain).As()
        from failed in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()
        select unit;

    // --- [SIGNAL]
    public static HostEvent<Unit> Signal<TArgs>(HostEvent<TArgs> row, Func<TArgs, bool> concerns) =>
        row.Choose(args => Callbacks.Found(concerns(args), unit));
}

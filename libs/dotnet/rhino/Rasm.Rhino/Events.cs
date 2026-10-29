using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
public record HostEvent<TArgs>(string Label, Func<Func<TArgs, IO<Unit>>, IO<Unit>> Attach) {
    public HostEvent<TValue> Choose<TValue>(Func<TArgs, Option<TValue>> choose) =>
        new(Label, deliver => Attach(args => IO.lift(() => choose(args)).Bind(chosen => chosen.Match(deliver, static () => IO.pure(unit)))));

    public HostEvent<TArgs> DistinctUntilChanged<TKey, TMark>(Func<TArgs, TKey> key, Func<TArgs, TMark> mark) where TKey : notnull =>
        new(Label, deliver =>
            from marks in IO.lift(static () => Ref(HashMap<TKey, TMark>()))
            from attached in Attach(args =>
                from repeat in atomicIO(() => {
                    HashMap<TKey, TMark> seen = marks.Value;
                    TKey raised = key(args);
                    TMark marked = mark(args);
                    marks.Value = seen.AddOrUpdate(raised, marked);
                    return seen.Find(raised) == Some(marked);
                })
                from delivered in unless(repeat, deliver(args)).As()
                select unit)
            select attached);

    public HostEvent<Seq<TArgs>> Debounced(Duration quiet, TimeProvider clock) =>
        new(Label, deliver =>
            from debouncer in use(() => new Debouncer<TArgs>(clock, quiet, deliver))
            from attached in Attach(debouncer.Post)
            select attached);
}

public sealed record DocumentEvent<TArgs>(string Label, Func<Func<TArgs, IO<Unit>>, IO<Unit>> Attach, Func<TArgs, Option<uint>> Serial)
    : HostEvent<TArgs>(Label, Attach) {

    public HostEvent<TArgs> In(uint serial) => Choose(args => Conversions.Present(Serial(args) == Some(serial), args));
}

internal sealed record Pending<A>(long Since, Seq<A> Changes);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Debouncer<A> : IDisposable {
    // --- [STATE]
    private readonly Ref<Option<Pending<A>>> held = Ref(Option<Pending<A>>.None);
    private readonly TimeProvider clock;
    private readonly TimeSpan quiet;
    private readonly ITimer timer;

    public Debouncer(TimeProvider clock, Duration quiet, Func<Seq<A>, IO<Unit>> deliver) =>
        (this.clock, this.quiet, timer) = (clock, quiet.ToTimeSpan(), clock.CreateTimer(_ => Session.Run(Tick(deliver)), state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan));

    // --- [POST]
    public IO<Unit> Post(A change) =>
        from since in IO.lift(clock.GetTimestamp)
        from posted in atomicIO(() => held.Swap(pending => Some(new Pending<A>(since, pending.ToSeq().Bind(static batch => batch.Changes).Add(change)))))
        from armed in IO.lift(() => timer.Change(quiet, Timeout.InfiniteTimeSpan))
        select unit;

    public void Dispose() => timer.Dispose();

    private IO<Unit> Tick(Func<Seq<A>, IO<Unit>> deliver) =>
        from now in IO.lift(clock.GetTimestamp)
        from prior in atomicIO(() => {
            Option<Pending<A>> pending = held.Value;
            held.Value = pending.Filter(batch => clock.GetElapsedTime(batch.Since, now) < quiet);
            return pending;
        })
        from fired in prior.Traverse(batch => clock.GetElapsedTime(batch.Since, now) switch {
            var elapsed when elapsed >= quiet => deliver(batch.Changes),
            var elapsed => IO.lift(() => timer.Change(quiet - elapsed, Timeout.InfiniteTimeSpan)).Map(unit),
        }).As()
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Subscriptions {
    // --- [HOST]
    public static HostEvent<TArgs> On<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        On<EventHandler<TArgs>, TArgs>(add, remove, static handler => handler, label);

    public static HostEvent<TArgs> On<THandler, TArgs>(
        Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerArgumentExpression(nameof(add))] string label = "")
        where THandler : Delegate =>
        new(label, deliver => Attached(add, remove, adapt((_, args) => Session.Start(IO.pure(args).Bind(deliver), label))));

    public static HostEvent<TArgs> Inline<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Inline<EventHandler<TArgs>, TArgs>(add, remove, static handler => handler, label);

    public static HostEvent<TArgs> Inline<THandler, TArgs>(
        Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerArgumentExpression(nameof(add))] string label = "")
        where THandler : Delegate =>
        new(label, deliver => Attached(add, remove, adapt((_, args) => Session.Run(IO.pure(args).Bind(deliver), label))));

    public static HostEvent<TArgs> Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> add, [CallerArgumentExpression(nameof(add))] string label = "") =>
        new(label, deliver =>
            from watcher in use(static () => new EventWatcher(includeHeadlessDocuments: true))
            from added in IO.lift(() => add(watcher, (_, args) => Session.Start(IO.pure(args).Bind(deliver), label)))
            select unit);

    private static IO<Unit> Attached<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(unit);

    // --- [CELLS]
    public static HostEvent<TValue> Changed<TValue>(Atom<TValue> cell, [CallerArgumentExpression(nameof(cell))] string label = "") =>
        On<AtomChangedEvent<TValue>, TValue>(handler => cell.Change += handler, handler => cell.Change -= handler, static handler => value => handler(sender: null, value), label);

    public static HostEvent<TValue> Changed<TValue>(Ref<TValue> cell, [CallerArgumentExpression(nameof(cell))] string label = "") =>
        On<AtomChangedEvent<TValue>, TValue>(handler => cell.Change += handler, handler => cell.Change -= handler, static handler => value => handler(sender: null, value), label);

    // --- [COMBINATION]
    public static HostEvent<TArgs> Merged<TArgs>(params Seq<HostEvent<TArgs>> rows) =>
        new(string.Join(" | ", rows.Map(static row => row.Label)), deliver => rows.TraverseM(row => row.Attach(deliver)).As().Map(unit));

    // --- [DOCUMENTS]
    extension<TArgs>(HostEvent<TArgs> row) where TArgs : DocumentEventArgs {
        public DocumentEvent<TArgs> Documents() => new(row.Label, row.Attach, static args => Conversions.Present(args.DocumentSerialNumber));
    }

    extension<TArgs>(HostEvent<TArgs> row) {
        public DocumentEvent<TArgs> Documents(Func<TArgs, uint> serial) => new(row.Label, row.Attach, args => Conversions.Present(serial(args)));

        public DocumentEvent<TArgs> Documents(Func<TArgs, RhinoDoc?> document) => new(row.Label, row.Attach, args => Conversions.Serial(document(args)));
    }

    // --- [FILES]
    public static HostEvent<Either<ErrorEventArgs, FileSystemEventArgs>> Files(DirectoryInfo directory, Seq<string> filters, bool subdirectories) =>
        new(directory.FullName, deliver =>
            from watcher in use(() => new FileSystemWatcher(directory.FullName) { NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size, IncludeSubdirectories = subdirectories })
                .Catch(
                    static error => error.HasException<ArgumentException>() || error.HasException<FileNotFoundException>(),
                    error => IO.fail<FileSystemWatcher>(new RhinoFault(directory.FullName, RhinoReason.Missing, Inner: error)))
            from changed in Seq(
                    On<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Changed += h, h => watcher.Changed -= h, static handler => handler.Invoke),
                    On<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Created += h, h => watcher.Created -= h, static handler => handler.Invoke),
                    On<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Deleted += h, h => watcher.Deleted -= h, static handler => handler.Invoke),
                    On<RenamedEventHandler, FileSystemEventArgs>(h => watcher.Renamed += h, h => watcher.Renamed -= h, static handler => handler.Invoke))
                .TraverseM(row => row.Attach(args => deliver(Right(args)))).As()
            from failed in On<ErrorEventHandler, ErrorEventArgs>(h => watcher.Error += h, h => watcher.Error -= h, static handler => handler.Invoke)
                .Attach(args => deliver(Left(args)))
            from enabled in IO.lift(() => {
                _ = filters.Iter(watcher.Filters.Add);
                watcher.EnableRaisingEvents = true;
            })
            select unit);
}

public static class Mailboxes {
    // --- [IDLE]
    public static IO<Func<TKey, TValue, IO<Unit>>> Idle<TKey, TValue>(Func<HashMap<TKey, TValue>, IO<Unit>> drain) =>
        from scope in IO.env
        from pending in IO.lift(static () => Ref(HashMap<TKey, TValue>()))
        from waiting in IO.lift(static () => Ref(Option<Owned<Unit>>.None))
        let idle = Subscriptions.On<EventHandler, EventArgs>(static h => RhinoApp.Idle += h, static h => RhinoApp.Idle -= h, static handler => handler.Invoke)
            .Attach(_ =>
                from detached in Products.Clear(waiting).Scoped(scope)
                from taken in atomicIO(() => {
                    HashMap<TKey, TValue> batch = pending.Value;
                    pending.Value = [];
                    return batch;
                })
                from drained in unless(taken.IsEmpty, drain(taken)).As()
                select unit)
        select fun((TKey key, TValue value) =>
            atomicIO(() => {
                bool first = pending.Value.IsEmpty;
                pending.Value = pending.Value.AddOrUpdate(key, value);
                return first;
            }).Bind(first => when(first, Products.Replace(waiting, idle).Scoped(scope)).As()));

    // --- [QUEUE]
    public static IO<Func<TItem, IO<Unit>>> Queued<TItem>(Buffer<TItem> buffer, Func<TItem, IO<Unit>> consume, Func<Error, IO<Unit>> failed) =>
        from conduit in use(IO.lift(() => Conduit.make(buffer)), static open => open.Complete())
        from forked in conduit.Reduce(unit, (_, item) => IO.pure(item).Bind(consume).Catch(failed).As().Map(Reduced.Continue))
            .Finally(conduit.Complete()).Fork()
        select fun((TItem item) => conduit.Post(item) | @catch(Errors.SinkFull, failed));
}

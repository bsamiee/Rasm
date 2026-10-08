using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino.Events;

// --- [MODELS] --------------------------------------------------------------------------
public record HostEvent<TArgs>(Type Owner, string Member, Func<Func<TArgs, IO<Unit>>, CallbackSite, IO<IDisposable>> Attach) {
    public IO<IDisposable> Inline(Func<TArgs, IO<Unit>> deliver, IPlugInSink sink) =>
        IO.pure(Site(sink)).Bind(site => Attach(deliver, site));

    public IO<IDisposable> Idle<TKey, TValue>(Func<TArgs, (TKey Key, TValue Value)> copy, Func<HashMap<TKey, TValue>, IO<Unit>> drain, IPlugInSink sink) where TKey : notnull =>
        Through(site => Subscriptions.Idle(drain, site), copy, Site(sink));

    public IO<IDisposable> Queued<TItem>(Buffer<TItem> buffer, Func<TArgs, TItem> copy, Func<TItem, IO<Unit>> consume, IPlugInSink sink) =>
        Through(site => Subscriptions.Queued(buffer, consume, error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member))), copy, Site(sink));

    private CallbackSite Site(IPlugInSink sink) => new(sink, Owner, Member);

    private IO<IDisposable> Through<TItem>(Func<CallbackSite, IO<(Func<TItem, IO<Unit>> Post, IDisposable Release)>> open, Func<TArgs, TItem> copy, CallbackSite site) =>
        from opened in open(site)
        from attached in DisposalOps.OnFailure(
            IO.pure(site).Bind(current => Attach(args => IO.lift(() => copy(args)).Bind(opened.Post), current)),
            IO.lift(opened.Release.Dispose))
        select (IDisposable)DisposalOps.Composite(Seq(opened.Release, attached), site.Sink);
}

public sealed record DocumentEvent<TArgs>(
    Type Owner,
    string Member,
    Func<Func<TArgs, IO<Unit>>, CallbackSite, IO<IDisposable>> Attach,
    Func<TArgs, Option<uint>> DocumentSerial) : HostEvent<TArgs>(Owner, Member, Attach) {
    public HostEvent<TArgs> In(uint serial) =>
        new(Owner, Member, (deliver, site) =>
            Attach(args => IO.lift(() => DocumentSerial(args).Exists(found => found == serial)).Bind(heard => when(heard, deliver(args)).As()), site));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Subscriptions {
    // --- [ATTACH]
    public static IO<IDisposable> Attach<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        IO.lift(() => add(handler)).Map<IDisposable>(_ => new Disposal<THandler>(handler, remove));

    public static IO<TResult> Detached<THandler, TResult>(Action<THandler> add, Action<THandler> remove, THandler handler, IO<TResult> effect) where THandler : Delegate =>
        IO.lift(() => remove(handler)).Bind(_ => effect).Finally(IO.lift(() => add(handler)));

    // --- [ROWS]
    public static HostEvent<TArgs> Host<TArgs>(Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerMemberName] string member = "") =>
        new(owner, member, (deliver, site) => Attach(add, remove, Callbacks.Handler(deliver, site)));

    public static HostEvent<TArgs> Host<THandler, TArgs>(
        Type owner, Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerMemberName] string member = "")
        where THandler : Delegate =>
        new(owner, member, (deliver, site) => Attach(add, remove, adapt(Callbacks.Handler(deliver, site))));

    public static DocumentEvent<TArgs> Host<TArgs>(
        Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(owner, member, (deliver, site) => Attach(add, remove, Callbacks.Handler(deliver, site)), documentSerial);

    public static DocumentEvent<TArgs> Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> add, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(typeof(EventWatcher), member, (deliver, site) =>
            from watcher in IO.lift(static () => new EventWatcher(includeHeadlessDocuments: true))
            from added in DisposalOps.OnFailure(IO.lift(() => add(watcher, Callbacks.Handler(deliver, site))), IO.lift(watcher.Dispose))
            select (IDisposable)new Disposal<EventWatcher>(watcher, static held => held.Dispose()), documentSerial);

    // --- [MAILBOXES]
    public static IO<(Func<(TKey Key, TValue Value), IO<Unit>> Post, IDisposable Release)> Idle<TKey, TValue>(Func<HashMap<TKey, TValue>, IO<Unit>> drain, CallbackSite site)
        where TKey : notnull =>
        from pending in IO.lift(static () => Atom((Pending: HashMap<TKey, TValue>(), Taken: HashMap<TKey, TValue>())))
        from idle in Attach<EventHandler>(
            static handler => RhinoApp.Idle += handler,
            static handler => RhinoApp.Idle -= handler,
            Callbacks.Handler<EventArgs>(
                _ =>
                    from held in pending.SwapIO(static held => (Pending: HashMap<TKey, TValue>(), Taken: held.Pending))
                    from drained in unless(held.Taken.IsEmpty, drain(held.Taken)).As()
                    select drained,
                site).Invoke)
        select (
            Post: fun(((TKey Key, TValue Value) entry) => pending.SwapIO(held => held with { Pending = held.Pending.AddOrUpdate(entry.Key, entry.Value) }).Map(static _ => unit)),
            Release: idle);

    public static IO<(Func<TItem, IO<Unit>> Post, IDisposable Release)> Queued<TItem>(Buffer<TItem> buffer, Func<TItem, IO<Unit>> consume, Func<Error, IO<Unit>> failed) =>
        from conduit in IO.lift(() => Conduit.make(buffer))
        from forked in DisposalOps.OnFailure(
            conduit.Reduce(unit, (_, item) => IO.pure(item).Bind(consume).Catch(failed).As().Map(Reduced.Continue)).Fork(),
            conduit.Complete())
        select (
            Post: fun((TItem item) => conduit.Post(item).Catch(static error => error.Is(Errors.SinkFull), static _ => IO.pure(unit))),
            Release: (IDisposable)new Disposal<Conduit<TItem, TItem>>(conduit, static open => open.Complete().Run()));
}

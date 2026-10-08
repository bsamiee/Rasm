using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino.Events;

// --- [MODELS] --------------------------------------------------------------------------
public record HostEvent<TArgs>(Type Owner, string Member, Func<Func<TArgs, IO<Unit>>, CallbackSite, IO<IDisposable>> Attach) {
    public IO<IDisposable> Inline(Func<TArgs, IO<Unit>> deliver, IPlugInSink sink) =>
        IO.pure(Site(sink)).Bind(site => Attach(deliver, site));

    public IO<IDisposable> Through(Func<CallbackSite, IO<(Func<TArgs, IO<Unit>> Post, IDisposable Release)>> mailbox, IPlugInSink sink) =>
        from site in IO.pure(Site(sink))
        from opened in mailbox(site)
        from attached in DisposalOps.OnFailure(Inline(opened.Post, sink), IO.lift(opened.Release.Dispose))
        select DisposalOps.Composite(Seq(opened.Release, attached), site);

    public HostEvent<TValue> Choose<TValue>(Func<TArgs, Option<TValue>> choose) =>
        new(Owner, Member, (deliver, site) => Attach(args => IO.lift(() => choose(args)).Bind(chosen => chosen.Match(deliver, static () => IO.pure(unit))), site));

    private CallbackSite Site(IPlugInSink sink) => new(sink, Owner, Member);
}

public sealed record DocumentEvent<TArgs>(Type Owner, string Member, Func<Func<TArgs, IO<Unit>>, CallbackSite, IO<IDisposable>> Attach, Func<TArgs, Option<uint>> DocumentSerial)
    : HostEvent<TArgs>(Owner, Member, Attach) {
    public HostEvent<TArgs> In(uint serial) => Choose(args => Callbacks.Found(DocumentSerial(args) == Some(serial), args));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Subscriptions {
    // --- [ATTACH]
    public static IO<IDisposable> Attach<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        IO.lift(() => add(handler)).Map<IDisposable>(_ => new Disposal<THandler>(handler, remove));

    public static IO<TResult> Detached<THandler, TResult>(Action<THandler> add, Action<THandler> remove, THandler handler, IO<TResult> effect) where THandler : Delegate =>
        IO.lift(() => remove(handler)).Bind(_ => effect).Finally(IO.lift(() => add(handler)));

    // --- [ROWS]
    public static HostEvent<TArgs> Host<THandler, TArgs>(Type owner, Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerMemberName] string member = "")
        where THandler : Delegate =>
        new(owner, member, (deliver, site) => Attach(add, remove, adapt(Callbacks.Handler(deliver, site))));

    public static HostEvent<TArgs> Host<TArgs>(Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerMemberName] string member = "") =>
        Host<EventHandler<TArgs>, TArgs>(owner, add, remove, static handler => handler, member);

    public static DocumentEvent<TArgs> Host<TArgs>(Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(owner, member, Host(owner, add, remove, member).Attach, documentSerial);

    public static DocumentEvent<TArgs> Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> add, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(typeof(EventWatcher), member, (deliver, site) => IO.lift(static () => new EventWatcher(includeHeadlessDocuments: true))
            .Bind(watcher => Attach(handler => add(watcher, handler), _ => watcher.Dispose(), Callbacks.Handler(deliver, site))), documentSerial);

    // --- [MAILBOXES]
    public static Func<CallbackSite, IO<(Func<(TKey Key, TValue Value), IO<Unit>> Post, IDisposable Release)>> Idle<TKey, TValue>(Func<HashMap<TKey, TValue>, IO<Unit>> drain) =>
        site =>
            from pending in IO.lift(static () => Atom((Pending: HashMap<TKey, TValue>(), Taken: HashMap<TKey, TValue>())))
            from idle in Attach<EventHandler>(static handler => RhinoApp.Idle += handler, static handler => RhinoApp.Idle -= handler, Callbacks.Handler<EventArgs>(
                _ => pending.SwapIO(static held => (HashMap<TKey, TValue>(), held.Pending)).Bind(held => unless(held.Taken.IsEmpty, IO.pure(held.Taken).Bind(drain)).As()), site).Invoke)
            select (fun(((TKey Key, TValue Value) entry) => pending.SwapIO(held => held with { Pending = held.Pending.AddOrUpdate(entry.Key, entry.Value) }).Map(static _ => unit)), idle);

    public static IO<(Func<TItem, IO<Unit>> Post, IDisposable Release)> Queued<TItem>(Buffer<TItem> buffer, Func<TItem, IO<Unit>> consume, Func<Error, IO<Unit>> failed) =>
        from conduit in IO.lift(() => Conduit.make(buffer))
        from forked in conduit.Reduce(unit, (_, item) => IO.pure(item).Bind(consume).Catch(failed).As().Map(Reduced.Continue)).Fork()
        select (fun((TItem item) => conduit.Post(item) | @catch(Errors.SinkFull, IO.pure(unit))), (IDisposable)new Disposal<Conduit<TItem, TItem>>(conduit, static open => open.Complete().Run()));
}

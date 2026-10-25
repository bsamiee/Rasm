using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Rhino;

namespace Rasm.Rhino.Events;

// --- [MODELS] --------------------------------------------------------------------------
public record HostEvent<TArgs>(Type Owner, string Member, Func<Func<TArgs, IO<Unit>>, CallbackSite, Action<IDisposable>, IO<Unit>> Attach) {
    public IO<Unit> Inline(Func<TArgs, IO<Unit>> deliver, IPlugInSink sink, Action<IDisposable> retain) =>
        IO.pure(Site(sink)).Bind(site => Attach(deliver, site, retain));

    public IO<Unit> Through(Func<CallbackSite, Action<IDisposable>, IO<Func<TArgs, IO<Unit>>>> mailbox, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from site in IO.pure(Site(sink))
            from post in mailbox(site, owned)
            from attached in Attach(post, site, owned)
            select unit, retain);

    public HostEvent<TValue> Choose<TValue>(Func<TArgs, Option<TValue>> choose) =>
        new(Owner, Member, (deliver, site, retain) => Attach(args => IO.lift(() => choose(args)).Bind(chosen => chosen.Match(deliver, static () => IO.pure(unit))), site, retain));

    private CallbackSite Site(IPlugInSink sink) => new(sink, Owner, Member);
}

public sealed record DocumentEvent<TArgs>(Type Owner, string Member, Func<Func<TArgs, IO<Unit>>, CallbackSite, Action<IDisposable>, IO<Unit>> Attach, Func<TArgs, Option<uint>> DocumentSerial)
    : HostEvent<TArgs>(Owner, Member, Attach) {
    public HostEvent<TArgs> In(uint serial) => Choose(args => Callbacks.Found(DocumentSerial(args) == Some(serial), args));
}

public readonly record struct JournalEntry<TKey, TValue>(Option<TKey> Partition, BigInteger Ordinal, Instant At, TValue Value);

public sealed record JournalSnapshot<TKey, TValue>(Seq<JournalEntry<TKey, TValue>> Rows, BigInteger Observed, BigInteger QueueDropped, BigInteger Refused, BigInteger RetentionDropped, Option<Error> Failure) {
    internal JournalSnapshot<TKey, TValue> Append((Option<TKey> Partition, Instant At, TValue Value) entry, uint retainedPerPartition) {
        Seq<JournalEntry<TKey, TValue>> partition = Rows.Filter(row => row.Partition == entry.Partition).Strict();
        JournalEntry<TKey, TValue> recorded = new(entry.Partition, Observed + 1, entry.At, entry.Value);
        bool full = partition.Count >= retainedPerPartition;
        return this with {
            Observed = recorded.Ordinal,
            Rows = retainedPerPartition == 0 ? Rows
                : (full ? Rows.Filter(row => row.Ordinal != partition[0].Ordinal) : Rows).Add(recorded).Strict(),
            RetentionDropped = RetentionDropped + (full ? 1 : 0),
        };
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EventJournal<TKey, TValue> : IDisposable {
    // --- [STATE]
    private readonly Atom<JournalSnapshot<TKey, TValue>> state;
    private readonly Channel<(Option<TKey> Partition, Instant At, TValue Value)> channel;
    private readonly ForkIO<Unit> worker;

    private EventJournal(Atom<JournalSnapshot<TKey, TValue>> state, Channel<(Option<TKey> Partition, Instant At, TValue Value)> channel, ForkIO<Unit> worker) =>
        (this.state, this.channel, this.worker) = (state, channel, worker);

    public JournalSnapshot<TKey, TValue> Snapshot => state.Value;

    // --- [OPEN]
    public static IO<EventJournal<TKey, TValue>> Open(BoundedChannelOptions queue, uint retainedPerPartition, CallbackSite site, Action<IDisposable> retain) =>
        (from state in atomIO(new JournalSnapshot<TKey, TValue>([], 0, 0, 0, 0, None))
         from channel in IO.lift(() => Channel.CreateBounded<(Option<TKey> Partition, Instant At, TValue Value)>(
             queue, _ => state.Swap(static held => held with { QueueDropped = held.QueueDropped + 1 })))
         from journal in DisposalOps.OnFailure(
             from worker in Source.lift(channel)
             .ReduceIO(unit, (_, entry) => state.SwapIO(held => held.Append(entry, retainedPerPartition)).Map(static _ => Reduced.Continue(unit)))
             .Catch(error => state.SwapIO(held => held with { Failure = Some(error) })
                 .Bind(_ => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member))))
             .Finally(IO.lift(() => channel.Writer.TryComplete()).Map(unit))
             .As()
             .Fork()
             from created in DisposalOps.Acquire(IO.lift(() => new EventJournal<TKey, TValue>(state, channel, worker)), static open => open.Close().Map(unit), retain)
             select created,
             IO.lift(() => channel.Writer.TryComplete()).Map(unit))
         select journal).Uninterruptible();

    // --- [RECORD]
    public IO<Unit> Post((Option<TKey> Partition, Instant At, TValue Value) entry) =>
        IO.lift(() => channel.Writer.TryWrite(entry)).Bind(written => unless(written,
            state.SwapIO(static held => held with { Refused = held.Refused + 1 }).Bind(static _ => IO.fail<Unit>(Errors.SinkFull))).As()).Uninterruptible();

    // --- [CLOSE]
    public IO<JournalSnapshot<TKey, TValue>> Close() =>
        IO.lift(() => channel.Writer.TryComplete()).Bind(_ => worker.Await).Bind(_ => state.ValueIO).Uninterruptible();

    public void Dispose() => _ = Callbacks.Settled(Close(), CancellationToken.None).ThrowIfFail();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Subscriptions {
    // --- [ATTACH]
    public static IO<Unit> Attach<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler, Action<IDisposable> retain) where THandler : Delegate =>
        DisposalOps.Acquire(owned =>
            DisposalOps.Acquire(IO.lift(() => new Disposal<THandler>(handler, remove)), lease => {
                add(handler);
                owned(lease);
            }).Map(unit), retain);

    public static IO<TResult> Detached<THandler, TResult>(Action<THandler> add, Action<THandler> remove, THandler handler, IO<TResult> effect) where THandler : Delegate =>
        DisposalOps.Within(fun(() => remove(handler)), _ => effect, _ => IO.lift(() => add(handler)));

    // --- [ROWS]
    public static HostEvent<TArgs> Host<THandler, TArgs>(Type owner, Action<THandler> add, Action<THandler> remove, Func<EventHandler<TArgs>, THandler> adapt, [CallerMemberName] string member = "")
        where THandler : Delegate =>
        new(owner, member, (deliver, site, retain) => Attach(add, remove, adapt(Callbacks.Handler(deliver, site)), retain));

    public static HostEvent<TArgs> Host<TArgs>(Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, [CallerMemberName] string member = "") =>
        Host<EventHandler<TArgs>, TArgs>(owner, add, remove, static handler => handler, member);

    public static DocumentEvent<TArgs> Host<TArgs>(Type owner, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(owner, member, Host(owner, add, remove, member).Attach, documentSerial);

    public static HostEvent<TArgs> Host<TArgs>(Type owner, Func<Seq<HostEvent<TArgs>>> sources, [CallerMemberName] string member = "") =>
        new(owner, member, (deliver, site, retain) =>
            DisposalOps.Acquire(owned => IO.lift(sources)
                .Bind(rows => rows.TraverseM(row => row.Inline(deliver, site.Sink, owned)).As()).Map(unit), retain));

    public static HostEvent<TValue> Changed<TValue>(Atom<TValue> atom, [CallerMemberName] string member = "") =>
        Host<AtomChangedEvent<TValue>, TValue>(typeof(Atom<TValue>), handler => atom.Change += handler, handler => atom.Change -= handler, static handler => value => handler(sender: null, value), member);

    public static HostEvent<TValue> Changed<TValue>(Ref<TValue> cell, [CallerMemberName] string member = "") =>
        Host<AtomChangedEvent<TValue>, TValue>(typeof(Ref<TValue>), handler => cell.Change += handler, handler => cell.Change -= handler, static handler => value => handler(sender: null, value), member);

    public static DocumentEvent<TArgs> Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> add, Func<TArgs, Option<uint>> documentSerial, [CallerMemberName] string member = "") =>
        new(typeof(EventWatcher), member, (deliver, site, retain) =>
            DisposalOps.Acquire(owned =>
                DisposalOps.Acquire(IO.lift(static () => new EventWatcher(includeHeadlessDocuments: true)), watcher => {
                    add(watcher, Callbacks.Handler(deliver, site));
                    owned(watcher);
                }).Map(unit), retain), documentSerial);

    // --- [MAILBOXES]
    public static Func<CallbackSite, Action<IDisposable>, IO<Func<(TKey Key, TValue Value), IO<Unit>>>> Idle<TKey, TValue>(Func<HashMap<TKey, TValue>, IO<Unit>> drain) =>
        (site, retain) =>
            from pending in IO.lift(static () => Ref(HashMap<TKey, TValue>()))
            from idle in Attach<EventHandler>(static handler => RhinoApp.Idle += handler, static handler => RhinoApp.Idle -= handler, Callbacks.Handler<EventArgs>(
                _ => atomicIO(() => {
                    HashMap<TKey, TValue> taken = pending.Value;
                    pending.Value = [];
                    return taken;
                }).Bind(taken => unless(taken.IsEmpty, drain(taken)).As()), site).Invoke, retain)
            select fun(((TKey Key, TValue Value) entry) => atomicIO(() => pending.Swap(held => held.AddOrUpdate(entry.Key, entry.Value))).Map(unit));

    public static IO<Func<TItem, IO<Unit>>> Queued<TItem>(Buffer<TItem> buffer, Func<TItem, IO<Unit>> consume, Func<Error, IO<Unit>> failed, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from conduit in DisposalOps.Acquire(IO.lift(() => Conduit.make(buffer)), static open => open.Complete(), open => owned(new Disposal<Conduit<TItem, TItem>>(open, static held => held.Complete().Run())))
            from forked in conduit.Reduce(unit, (_, item) => IO.pure(item).Bind(consume).Catch(failed).As().Map(Reduced.Continue))
                .Finally(conduit.Complete()).Fork()
            select fun((TItem item) => conduit.Post(item) | @catch(Errors.SinkFull, IO.pure(unit))),
            retain).Uninterruptible();
}

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Rhino;

namespace Rasm.RhinoCommon.Events;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Transition<TState>(TState Before, TState After);

internal sealed record LiveState<TState>(TState Before, TState After, int Shown, bool Stale, long Revision);

internal sealed record Pending<A>(long Since, Seq<A> Changes);

public sealed record JournalEntry<TKey, TValue>(Option<TKey> Partition, BigInteger Ordinal, Instant At, TValue Value);

internal sealed record JournalPost<TKey, TValue>(Option<TKey> Partition, Instant At, TValue Value);

public sealed record JournalSnapshot<TKey, TValue>(Seq<JournalEntry<TKey, TValue>> Rows, BigInteger Observed, BigInteger QueueDropped, BigInteger Refused, BigInteger RetentionDropped, Option<Error> Failure) {
    internal JournalSnapshot<TKey, TValue> Append(JournalPost<TKey, TValue> post, uint retainedPerPartition) {
        Seq<JournalEntry<TKey, TValue>> partition = Rows.Filter(row => row.Partition == post.Partition).Strict();
        JournalEntry<TKey, TValue> recorded = new(post.Partition, Observed + 1, post.At, post.Value);
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
public sealed class LiveModel<TState> where TState : notnull {
    // --- [STATE]
    private readonly Ref<LiveState<TState>> state;

    internal LiveModel(TState initial) => state = Ref(new LiveState<TState>(initial, initial, Shown: 0, Stale: false, Revision: 0));

    public TState Value => state.Value.After;

    public HostEvent<Transition<TState>> Changed =>
        Subscriptions.Changed(state).DistinctUntilChanged(static _ => unit, static held => held.Revision).Choose(static held => Some(new Transition<TState>(held.Before, held.After)));

    // --- [VISIBILITY]
    internal IO<Unit> Show(IO<TState> read) =>
        from shown in atomicIO(() => state.Swap(static held => held with { Shown = held.Shown + 1 }))
        from hidden in Products.Released(() => atomic(() => state.Swap(static held => held with { Shown = held.Shown - 1 })))
        from fresh in when(shown.Stale, read.Bind(Publish)).As()
        select unit;

    internal IO<Unit> Refresh(IO<TState> read) =>
        atomicIO(() => Some(state.Value).Filter(static held => !held.Stale).Map(held => state.Value = held with { Stale = true }).Exists(static held => held.Shown > 0))
            .Bind(shown => when(shown, read.Bind(Publish)).As());

    private IO<Unit> Publish(TState next) =>
        atomicIO(() => state.Value = state.Value switch {
            var held when EqualityComparer<TState>.Default.Equals(held.After, next) => held with { Stale = false },
            var held => held with { Before = held.After, After = next, Stale = false, Revision = held.Revision + 1 },
        }).Map(unit);
}

public sealed class ReadModel<TState>(Seq<HostEvent<Option<uint>>> marks, Func<RhinoDoc, IO<TState>> read) where TState : notnull {
    private string Label { get; } = string.Join(" | ", marks.Map(static mark => mark.Label));

    public IO<Unit> Attach(object owner) =>
        from post in Mailboxes.Idle<Option<uint>, Unit>(Refresh)(owner)
        from attached in marks.TraverseM(mark => mark.Inline(serial => post(serial, unit), owner)).As()
        select unit;

    public IO<LiveModel<TState>> Watch(RhinoDoc doc) =>
        from found in IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
        from live in found.Match(Some: static held => IO.pure(held), None: () => Registered(doc))
        from shown in live.Show(read(doc))
        select live;

    public HostEvent<Transition<TState>> Changed(RhinoDoc doc) =>
        new(Label, (deliver, owner) => Watch(doc).Bind(live => live.Changed.Inline(deliver, owner)));

    private IO<LiveModel<TState>> Registered(RhinoDoc doc) =>
        read(doc).Bind(initial => IO.lift(() => doc.RuntimeData.GetValue(this, _ => new LiveModel<TState>(initial))));

    private IO<Unit> Refresh(HashMap<Option<uint>, Unit> marked) =>
        from docs in IO.lift(() => toSeq(marked.Keys).Exists(static serial => serial.IsNone)
            ? Conversions.Rows(RhinoDoc.OpenDocuments())
            : toSeq(marked.Keys).Somes().Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Strict())
        from refreshed in Callbacks.Each(docs.Map(doc =>
            IO.lift(() => Optional(doc.RuntimeData.TryGetValue<LiveModel<TState>>(this)))
                .Bind(found => found.Match(Some: live => live.Refresh(read(doc)), None: static () => IO.pure(unit)))))
        select unit;
}

internal sealed class Debouncer<A> : IDisposable {
    // --- [STATE]
    private readonly Ref<Option<Pending<A>>> held = Ref(Option<Pending<A>>.None);
    private readonly TimeProvider clock;
    private readonly TimeSpan quiet;
    private readonly ITimer timer;

    public Debouncer(TimeProvider clock, Duration quiet, Func<Seq<A>, IO<Unit>> deliver, object owner) =>
        (this.clock, this.quiet, timer) = (clock, quiet.ToTimeSpan(), clock.CreateTimer(_ => owner.Run(Tick(deliver)), state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan));

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

public sealed class EventJournal<TKey, TValue> : IDisposable {
    // --- [STATE]
    private readonly Atom<JournalSnapshot<TKey, TValue>> state;
    private readonly Channel<JournalPost<TKey, TValue>> channel;
    private readonly ForkIO<Unit> worker;
    private readonly object owner;

    private EventJournal(Atom<JournalSnapshot<TKey, TValue>> state, Channel<JournalPost<TKey, TValue>> channel, ForkIO<Unit> worker, object owner) =>
        (this.state, this.channel, this.worker, this.owner) = (state, channel, worker, owner);

    public JournalSnapshot<TKey, TValue> Snapshot => state.Value;

    // --- [OPEN]
    public static IO<EventJournal<TKey, TValue>> Open(BoundedChannelOptions queue, uint retainedPerPartition, object owner, [CallerMemberName] string member = "") =>
        from state in atomIO(new JournalSnapshot<TKey, TValue>([], 0, 0, 0, 0, None))
        from channel in IO.lift(() => Channel.CreateBounded<JournalPost<TKey, TValue>>(queue, _ => state.Swap(static held => held with { QueueDropped = held.QueueDropped + 1 })))
        from worker in Source.lift(channel)
            .ReduceIO(unit, (_, post) => state.SwapIO(held => held.Append(post, retainedPerPartition)).Map(static _ => Reduced.Continue(unit)))
            .Catch(error => state.SwapIO(held => held with { Failure = Some(error) }).Bind(_ => IO.lift(() => PlugInLog.Of(owner).Failed(error, owner.GetType(), member))))
            .Finally(IO.lift(() => channel.Writer.TryComplete()))
            .As()
            .Fork()
        from journal in use(() => new EventJournal<TKey, TValue>(state, channel, worker, owner))
        select journal;

    // --- [RECORD]
    public IO<Unit> Post(Option<TKey> partition, Instant at, TValue value) =>
        IO.lift(() => channel.Writer.TryWrite(new JournalPost<TKey, TValue>(partition, at, value))).Bind(written => unless(written,
            state.SwapIO(static held => held with { Refused = held.Refused + 1 }).Bind(static _ => IO.fail<Unit>(Errors.SinkFull))).As());

    // --- [CLOSE]
    public IO<JournalSnapshot<TKey, TValue>> Close() =>
        IO.lift(() => channel.Writer.TryComplete()).Bind(_ => worker.Await).Bind(_ => state.ValueIO).Uninterruptible();

    public void Dispose() => _ = owner.Run(Close());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReadModels {
    public static HostEvent<Option<uint>> Marks<TArgs>(DocumentEvent<TArgs> row) =>
        row.Choose(args => Some(row.Serial(args)));

    public static HostEvent<Option<uint>> Marks<TArgs>(HostEvent<TArgs> row) =>
        row.Choose(static _ => Some(Option<uint>.None));
}

public static class Mailboxes {
    public static Func<object, IO<Func<TKey, TValue, IO<Unit>>>> Idle<TKey, TValue>(Func<HashMap<TKey, TValue>, IO<Unit>> drain) =>
        owner =>
            from pending in IO.lift(static () => Ref(HashMap<TKey, TValue>()))
            from idle in Subscriptions.Host<EventHandler, EventArgs>(static handler => RhinoApp.Idle += handler, static handler => RhinoApp.Idle -= handler, static handler => handler.Invoke)
                .Inline(_ => atomicIO(() => {
                    HashMap<TKey, TValue> taken = pending.Value;
                    pending.Value = [];
                    return taken;
                }).Bind(taken => unless(taken.IsEmpty, drain(taken)).As()), owner)
            select fun((TKey key, TValue value) => atomicIO(() => pending.Swap(held => held.AddOrUpdate(key, value))).Map(unit));

    public static IO<Func<TItem, IO<Unit>>> Queued<TItem>(Buffer<TItem> buffer, Func<TItem, IO<Unit>> consume, Func<Error, IO<Unit>> failed) =>
        from conduit in use(IO.lift(() => Conduit.make(buffer)), static open => open.Complete())
        from forked in conduit.Reduce(unit, (_, item) => IO.pure(item).Bind(consume).Catch(failed).As().Map(Reduced.Continue))
            .Finally(conduit.Complete()).Fork()
        select fun((TItem item) => conduit.Post(item) | @catch(Errors.SinkFull, failed));
}

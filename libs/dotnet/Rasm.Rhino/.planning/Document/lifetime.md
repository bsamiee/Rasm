# [RASM_RHINO_LIFETIME]

`Document/lifetime` owns the package-wide lifetime primitives every boundary lease composes: the one claims/close/retry lifecycle capsule, the symmetric attach/detach subscription, the reentrancy guard, and the bounded idle-deferred pump over the host idle clock. Host-resource teardown composes the kernel `Custody` algebra rather than owning a Rhino-local release fold.

Everything here is host-light: `LifecycleGate`, `Subscription`, and `Reentrancy` touch no Rhino type at all, and `IdlePump<TTag>` names exactly one host member — `RhinoApp.Idle`, the host's own quiet-moment clock — while its drain crosses the kernel dispatch on the deferred lane so idle work spends a budget the kernel gauges rather than an unmeasured slice of the idle callback.

## [01]-[INDEX]

- [02]-[GATE]: `LeaseState`, `LifecycleGate` — the bounded claims/close/retry lifecycle capsule.
- [03]-[SUBSCRIPTION]: `Subscription`, `SubscriptionRelease`, `Reentrancy` — symmetric attach/detach over kernel custody and the reentrancy guard.
- [04]-[PUMP]: `PumpLoss`, `IdlePump<TTag>` — the bounded idle-deferred pump under the host idle clock and the kernel deferred lane.

## [02]-[GATE]

- Owner: `LifecycleGate` — the package's ONE claims/close/retry lifecycle capsule; `LeaseState` its closed four-state custody union. Every bounded-settle lease across the boundary composes it from this namespace, and a sibling hand-rolling a `lock`/`Monitor` lifecycle machine beside it is the collapsed form.
- Cases: `Open(Claims)` admits work, `Closing` drains it under a one-owner token, `Reopenable(Claims)` is a close whose settle refused and may be re-driven, `Closed` is terminal. `Reopenable` — never `Retryable`: the branch makes `Retriability`/`Redrive` the ONE retry vocabulary, and a close-state case wearing that word reads as a retry capsule where none exists.
- Entry: `Of(settleWithin)` admits the drain bound; `Within(body, refused)` claims, runs, and releases; `Close(stop, settle)` is the blocking one-owner close; `Begin(stop, settle)` arms the close and hands back the completion for an owner that must not block its own callback thread.
- Law: a claim runs to completion on the thread that took it, so a close issued from a thread already inside a claim would wait on its own release forever — the claiming-thread set is the structural refusal for that re-entrancy, and it is what keeps a bounded blocking close safe on the host callback thread.
- Law: the drain is bounded but still BLOCKING, so it never rides the closing caller's thread: `Begin` arms the close, runs `stop` on the caller's own thread so a marshalled arm keeps its affinity, and hands back the completion — a host UI-thread owner settles that completion off-thread, because blocking there stalls the very callbacks the drain waits to see released.
- Law: the owning close alone drives the drain, and it drives it as a scheduler continuation: `stop` runs inline on the caller's thread, then the bounded wait and the settle ride the pool; concurrent closers join the in-flight completion rather than double-driving it.
- Law: a refused settle lands `Reopenable`, never `Closed` — the resources the settle failed to release are still held, so a terminal reading would certify a leak as a close; a later `Begin` re-drives the same drain over the surviving claims.
- Output: the close outcome is the one `Fin<Unit>` every faulted step aggregated into through the `Error` monoid — stop, drain, and settle refusals all survive, never the newest alone.
- Packages: LanguageExt.Core (`Atom`, `Fin`, `Set`, the `Error` monoid); BCL inbox (`TaskCompletionSource`, `TaskScheduler`).
- Growth: a new custody posture is one `LeaseState` case every generated dispatch breaks on loudly.
- Boundary: the gate holds no resource of its own — `stop` and `settle` are the owner's, so the capsule is reusable across pointer leases, content streams, and watch custody without knowing any of them.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Threading;
using System.Threading.Tasks;
using Rasm.Domain;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LeaseState {
    private LeaseState() { }
    internal sealed record Open(int Claims) : LeaseState;
    internal sealed record Closing(int Claims, Guid Token, TaskCompletionSource<Unit> Quiesced, TaskCompletionSource<Fin<Unit>> Completed) : LeaseState;
    internal sealed record Reopenable(int Claims) : LeaseState;
    internal sealed record Closed : LeaseState;
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class LifecycleGate {
    private readonly Atom<LeaseState> state = Atom<LeaseState>(new LeaseState.Open(Claims: 0));
    private readonly Atom<Set<int>> claiming = Atom(Set<int>());
    private readonly TimeSpan settleWithin;
    private LifecycleGate(TimeSpan settleWithin) => this.settleWithin = settleWithin;
    internal static Fin<LifecycleGate> Of(TimeSpan settleWithin) =>
        guard(settleWithin > TimeSpan.Zero, new KernelFault.InvalidInput()).ToFin().Map(_ => new LifecycleGate(settleWithin));

    internal Fin<T> Within<T>(Func<Fin<T>> body, Func<Fin<T>> refused) =>
        TryClaim()
            ? Marked(body).Settled(release: () => Fin.Succ(Release()))
            : Try.lift(refused).Run().Bind(static inner => inner);

    internal Fin<Unit> Close(Func<Fin<Unit>> stop, Func<Fin<Unit>> settle) =>
        Begin(stop, settle).Bind(completion => Await(completion)).Bind(static outcome => outcome);

    internal Fin<Task<Fin<Unit>>> Begin(Func<Fin<Unit>> stop, Func<Fin<Unit>> settle) {
        if (claiming.Value.Contains(Environment.CurrentManagedThreadId)) { return Fin.Fail<Task<Fin<Unit>>>(new KernelFault.InvalidContext()); }
        Guid token = Guid.NewGuid();
        TaskCompletionSource<Unit> quiesced = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Fin<Unit>> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        LeaseState next = state.Swap(current => current.Switch(
            (Token: token, Quiesced: quiesced, Completed: completed),
            open: static (ctx, row) => (LeaseState)new LeaseState.Closing(row.Claims, ctx.Token, ctx.Quiesced, ctx.Completed),
            closing: static (_, row) => row,
            reopenable: static (ctx, row) => new LeaseState.Closing(row.Claims, ctx.Token, ctx.Quiesced, ctx.Completed),
            closed: static (_, row) => row));
        return next.Switch(
            (Gate: this, Token: token, Stop: stop, Settle: settle),
            open: static (ctx, _) => Fin.Fail<Task<Fin<Unit>>>(new KernelFault.InvalidContext()),
            closing: static (ctx, row) => Fin.Succ(row.Token == ctx.Token
                ? ctx.Gate.Drain(row, ctx.Stop, ctx.Settle)
                : row.Completed.Task),
            reopenable: static (ctx, _) => Fin.Fail<Task<Fin<Unit>>>(new KernelFault.InvalidContext()),
            closed: static (_, _) => Fin.Succ(Task.FromResult(Fin.Succ(unit))));
    }

    private bool TryClaim() => state.Swap(current => current.Switch(
        open: static row => (LeaseState)new LeaseState.Open(row.Claims + 1),
        closing: static row => row,
        reopenable: static row => row,
        closed: static row => row)).Switch(
            open: static _ => true,
            closing: static _ => false,
            reopenable: static _ => false,
            closed: static _ => false);

    private Fin<T> Marked<T>(Func<Fin<T>> body) {
        int thread = Environment.CurrentManagedThreadId;
        _ = claiming.Swap(rows => rows.Add(thread));
        try { return Try.lift(body).Run().Bind(static inner => inner); }
        finally { _ = claiming.Swap(rows => rows.Remove(thread)); }
    }

    private Unit Release() => state.Swap(current => current.Switch(
        open: static row => (LeaseState)new LeaseState.Open(row.Claims - 1),
        closing: static row => new LeaseState.Closing(row.Claims - 1, row.Token, row.Quiesced, row.Completed),
        reopenable: static row => new LeaseState.Reopenable(row.Claims - 1),
        closed: static row => row)).Switch(
            open: static _ => unit,
            closing: static row => row.Claims == 0 ? HostEdge.Side(() => row.Quiesced.TrySetResult(unit)) : unit,
            reopenable: static _ => unit,
            closed: static _ => unit);

    private Task<Fin<Unit>> Drain(LeaseState.Closing row, Func<Fin<Unit>> stop, Func<Fin<Unit>> settle) {
        Fin<Unit> stopped = Try.lift(stop).Run().Bind(static inner => inner);
        if (row.Claims == 0) { ignore(row.Quiesced.TrySetResult(unit)); }
        return row.Quiesced.Task.WaitAsync(settleWithin).ContinueWith(
            drained => Conclude(
                row,
                stopped,
                drained.Status == TaskStatus.RanToCompletion ? Fin.Succ(unit) : Fin.Fail<Unit>(new KernelFault.InvalidContext()),
                settle),
            CancellationToken.None,
            TaskContinuationOptions.RunContinuationsAsynchronously,
            TaskScheduler.Default);
    }

    private Fin<Unit> Conclude(LeaseState.Closing row, Fin<Unit> stopped, Fin<Unit> drained, Func<Fin<Unit>> settle) {
        Fin<Unit> settled = drained.Match(
            Succ: _ => Try.lift(settle).Run().Bind(static inner => inner),
            Fail: static _ => Fin.Succ(unit));
        Seq<Error> trouble = Seq(
                stopped,
                drained,
                settled)
            .Choose(static step => step.Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)));
        Fin<Unit> outcome = trouble.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Error.Many(trouble));
        _ = state.Swap(current => current.Switch(
            open: static value => (LeaseState)value,
            closing: value => value.Token == row.Token
                ? trouble.IsEmpty ? new LeaseState.Closed() : new LeaseState.Reopenable(value.Claims)
                : value,
            reopenable: static value => value,
            closed: static value => value));
        _ = row.Completed.TrySetResult(outcome);
        return outcome;
    }

    private Fin<T> Await<T>(Task<T> signal) => Try.lift(() =>
        signal.Wait(settleWithin) ? Fin.Succ(signal.Result) : Fin.Fail<T>(new KernelFault.InvalidContext())).Run().Bind(static inner => inner);
}
```

## [03]-[SUBSCRIPTION]

- Owner: `Subscription` — symmetric attach/detach custody with rollback and retry retention; `SubscriptionRelease` its closed release evidence; `Reentrancy` the per-owner delivery guard.
- Entry: `Subscription.Attach(subscribe, unsubscribe, handler)` pairs one host event add with its remove; `Acquire(acquire, release)` brackets a non-event resource; `AttachAll` folds a roster and rolls back the admitted prefix on refusal; `Close` runs every retained detacher, retains the failed ones for a later attempt, and settles concurrent closers on one result.
- Law: kernel `Custody.Release` is the ONE failure-release fold: reverse acquisition order, every release attempted, exact exception capture, and `Error.Many` over the complete refusal set. A Rhino page spelling another release fold is the deleted form.
- Law: kernel `Custody.Rollback` is the failure-arm custody case and `Custody.Settled` its BOTH-ARMS sibling. Cleanup refusals append to the primary; release never rides `ignore` or a success-only `.Map`.
- Law: a throwing attach rolls its own subscribe back before the fault leaves — `Attach` and `Acquire` both run the inverse on the refusal arm and aggregate a failing inverse into the primary, so no half-attached handler survives an admission refusal.
- Law: `Close` is all-attempted with retry retention: a failing detacher stays on the subscription for a later close, the settled release names every fault beside the attempted count, and concurrent closers join one in-flight settlement rather than racing the detach roster.
- Law: `Reentrancy` answers a VERDICT the caller records — `Guarded` returns absence when the guard suppressed a recursive delivery and the ran outcome otherwise, so the guard stays journal-free and each composing owner posts its own suppression evidence; a guard that posted for its caller would couple every composer to one journal shape.
- Output: `SubscriptionRelease` — `Open`, `Released(Attempted)`, or `Faulted(Attempted, Errors)`; the errors are the whole refusal set, never the newest.
- Packages: kernel `Domain/results` (`Custody.Release`, `Custody.Rollback`, `Custody.Settled`); LanguageExt.Core (`Fin`, `Seq`, the `Error` monoid); BCL inbox (`TaskCompletionSource`, `Lock`).
- Growth: a new release posture is one `SubscriptionRelease` case; generic custody postures belong on the kernel `Custody` algebra.
- Exemption: `Subscription` and its closure records ride a `Lock` — close claims its detacher roster, runs callbacks after release, and publishes retry custody with one settled result atomically, a sequence whose steps must each run after an earlier one refused; the platform-forced lifetime boundary is contained here and no composer writes one.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Threading.Tasks;
using Rasm.Domain;

namespace Rasm.Rhino.Document;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Subscription : IDisposable {
    private readonly Lock gate = new();
    private SubscriptionClosure closure;

    private Subscription(Seq<Action> detach) =>
        closure = new SubscriptionClosure.Ready(Pending: detach, Release: new SubscriptionRelease.Open());

    public SubscriptionRelease Release {
        get {
            Task<SubscriptionRelease>? waiting;
            lock (gate) {
                if (closure is SubscriptionClosure.Ready ready) {
                    return ready.Release;
                }
                waiting = ((SubscriptionClosure.Closing)closure).Settled;
            }
            return SubscriptionRelease.Join(waiting);
        }
    }

    internal static Subscription Of(Action detach) {
        return new(detach: Seq(detach));
    }

    public static Fin<Subscription> Attach<THandler>(Action<THandler> subscribe, Action<THandler> unsubscribe, THandler handler)
        where THandler : Delegate {
        return Try.lift(() => { subscribe(obj: handler); return Fin.Succ(value: Of(detach: () => unsubscribe(obj: handler))); }).Run().Bind(static inner => inner)
            .Rollback(release: () => Try.lift(() => { unsubscribe(obj: handler); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner));
    }

    public static Fin<Subscription> Acquire(Action acquire, Action release) {
        return Try.lift(() => { acquire(); return Fin.Succ(value: Of(detach: release)); }).Run().Bind(static inner => inner)
            .Rollback(release: () => Try.lift(() => { release(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner));
    }

    public static Fin<Subscription> AttachAll(Seq<Func<Fin<Subscription>>> attach) =>
        attach.Fold(
            Fin.Succ(value: new Subscription(detach: Seq<Action>())),
            static (result, start) => result.Bind(held => start()
                .Map(held.Combine)
                .MapFail(held.Rollback)));

    internal Subscription Combine(Subscription other) {
        return new(detach: other.Snapshot().Concat(Snapshot()));
    }

    public SubscriptionRelease Close() {
        SubscriptionClosure.Ready? claimed = null;
        Task<SubscriptionRelease>? waiting = null;
        TaskCompletionSource<SubscriptionRelease>? flight = null;
        lock (gate) {
            if (closure is SubscriptionClosure.Closing closing) {
                waiting = closing.Settled;
            } else {
                claimed = (SubscriptionClosure.Ready)closure;
                if (claimed.Pending.IsEmpty) {
                    SubscriptionRelease settled = claimed.Release is SubscriptionRelease.Open
                        ? new SubscriptionRelease.Released(Attempted: 0)
                        : claimed.Release;
                    closure = claimed with { Release = settled };
                    return settled;
                }
                flight = SubscriptionRelease.BeginClose();
                closure = new SubscriptionClosure.Closing(Settled: flight.Task);
            }
        }
        if (waiting is not null) {
            return SubscriptionRelease.Join(waiting);
        }
        SubscriptionClosure.Ready owner = claimed!;
        (Seq<Action> Retry, Seq<Error> Errors) outcome = owner.Pending.Fold(
            (Retry: Seq<Action>(), Errors: Seq<Error>()),
            static (state, action) => Try.lift(() => { action(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)
                .Match(
                    Succ: _ => state,
                    Fail: error => (
                        Retry: state.Retry.Add(value: action),
                        Errors: state.Errors.Add(value: error))));
        SubscriptionRelease settled = outcome.Errors.IsEmpty
            ? new SubscriptionRelease.Released(Attempted: owner.Pending.Count)
            : new SubscriptionRelease.Faulted(Attempted: owner.Pending.Count, Errors: outcome.Errors);
        lock (gate) {
            closure = new SubscriptionClosure.Ready(Pending: outcome.Retry, Release: settled);
            return SubscriptionRelease.Publish(pending: flight!, release: settled);
        }
    }

    public void Dispose() => ignore(Close());

    internal Error Rollback(Error primary) => Close() switch {
        SubscriptionRelease.Faulted faulted => faulted.Errors.Fold(primary, static (error, cleanup) => error + cleanup),
        SubscriptionRelease.Open or SubscriptionRelease.Released => primary,
    };

    private Seq<Action> Snapshot() {
        while (true) {
            Task<SubscriptionRelease>? waiting;
            lock (gate) {
                if (closure is SubscriptionClosure.Ready ready) {
                    return ready.Pending;
                }
                waiting = ((SubscriptionClosure.Closing)closure).Settled;
            }
            ignore(SubscriptionRelease.Join(waiting));
        }
    }

    private abstract record SubscriptionClosure {
        private SubscriptionClosure() { }

        internal sealed record Ready(Seq<Action> Pending, SubscriptionRelease Release) : SubscriptionClosure;
        internal sealed record Closing(Task<SubscriptionRelease> Settled) : SubscriptionClosure;
    }
}

[Union]
public abstract partial record SubscriptionRelease {
    private SubscriptionRelease() { }
    public sealed record Open : SubscriptionRelease;
    public sealed record Released(int Attempted) : SubscriptionRelease;
    public sealed record Faulted(int Attempted, Seq<Error> Errors) : SubscriptionRelease;

    internal static TaskCompletionSource<SubscriptionRelease> BeginClose() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal static SubscriptionRelease Join(Task<SubscriptionRelease> pending) =>
        pending.GetAwaiter().GetResult();

    internal static SubscriptionRelease Publish(
        TaskCompletionSource<SubscriptionRelease> pending,
        SubscriptionRelease release) {
        pending.SetResult(release);
        return release;
    }

    internal static SubscriptionRelease All(params ReadOnlySpan<SubscriptionRelease> releases) {
        int attempted = 0;
        bool open = false;
        Seq<Error> errors = Seq<Error>();
        foreach (SubscriptionRelease release in releases) {
            switch (release) {
                case Open:
                    open = true;
                    break;
                case Released ready:
                    attempted = checked(attempted + ready.Attempted);
                    break;
                case Faulted faulted:
                    attempted = checked(attempted + faulted.Attempted);
                    errors = errors.Concat(faulted.Errors);
                    break;
            }
        }
        return !errors.IsEmpty
            ? new Faulted(Attempted: attempted, Errors: errors)
            : open
                ? new Open()
                : new Released(Attempted: attempted);
    }

    internal static Error AddTo(Error primary, SubscriptionRelease release) => release switch {
        Faulted faulted => faulted.Errors.Fold(primary, static (error, cleanup) => error + cleanup),
        Open or Released => primary,
    };
}

internal sealed class Reentrancy {
    private readonly AsyncLocal<int> depth = new();

    internal bool Active => depth.Value > 0;

    internal Option<Fin<Unit>> Guarded(Func<Fin<Unit>> run) {
        if (Active) {
            return Option<Fin<Unit>>.None;
        }
        depth.Value++;
        try {
            return Some(Try.lift(run).Run().Bind(static inner => inner));
        } finally {
            depth.Value--;
        }
    }
}
```

## [04]-[PUMP]

- Owner: `IdlePump<TTag>` — the bounded idle-deferred pump: work parks tagged until the host's next quiet moment, the pending queue is capacity-bounded, every loss is a typed row the owner's own callback records, and the drain crosses the kernel dispatch on the deferred lane so idle work spends a gauged frame budget; `PumpLoss` the two-row loss vocabulary.
- Entry: `Open(capacity, lost)` attaches the one `RhinoApp.Idle` hook and admits the bound; `Enqueue(tag, alive, run)` parks one unit of work; `Close` cancels the pending roster, reports each as `Cancelled`, and detaches the hook.
- Law: the pump is GENERIC over the tag its loss callback names, so the delivery owner instantiates it over its own origin vocabulary and the pump holds no journal, no fact shape, and no event type — a pump that posted for its caller would couple every deferred consumer to one journal.
- Law: admission is a guarded step whose verdict rides the transition — a full queue DECLINES and the loss callback records `Overflow`, a closed pump records `Cancelled`, and neither outcome is inferred from a count read beside the swap.
- Law: the drain is take-and-clear through the kernel `Cell.Take`, so the drained roster is the `Committed` payload of one transition and a batch enqueued during the drain waits for the next idle tick rather than racing the sweep.
- Law: the drain crosses `UiThread.Run` as a `Blocking` crossing on `DispatchLane.Deferred` — the idle callback already holds the host UI thread, so the crossing runs in-frame and buys the gauged deferred-lane span and its breach evidence; a headless host whose crossing refuses runs the drain bare, because dropping deferred work on a marshal refusal would turn a missing application into silent loss. `RhinoApp.Idle` stays the host clock either way.
- Law: a drained unit whose `alive` probe answers false records `Cancelled` and never runs — liveness is the enqueuer's own predicate, so a watch cancelled between park and drain drops its work as evidence rather than running against a dead owner.
- Output: none of its own — every loss reaches the owner's `lost` callback with its tag, and the ran outcome is the work's own result, recorded by the closure the enqueuer parked.
- Packages: RhinoCommon (`RhinoApp.Idle`); `Rasm.Interaction` (`UiThread`, `UiDispatch<T>`, `DispatchLane`); LanguageExt.Core (`Atom`, `Seq`, `Fin`); `Rasm.Domain` (`Cell`, `Transition`, `Dimension`).
- Growth: a new loss posture is one `PumpLoss` row; a new deferral clock is a different pump owner, never a mode knob here.
- Boundary: the pump owns the ONE idle hook per instance and nothing else — the work closures carry their own custody, and the pump never reads what a unit of work does.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rhino;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PumpLoss {
    public static readonly PumpLoss Overflow = new(key: 0);
    public static readonly PumpLoss Cancelled = new(key: 1);
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class IdlePump<TTag> : IDisposable {
    private readonly Atom<Seq<PumpWork>> pending = Atom(Seq<PumpWork>());
    private readonly Atom<bool> open = Atom(true);
    private readonly Rasm.Numerics.Dimension capacity;
    private readonly Action<PumpLoss, TTag> lost;
    private Subscription? subscription;

    private IdlePump(Rasm.Numerics.Dimension capacity, Action<PumpLoss, TTag> lost) =>
        (this.capacity, this.lost, this.key) = (capacity, lost);

    internal static Fin<IdlePump<TTag>> Open(Rasm.Numerics.Dimension capacity, Action<PumpLoss, TTag> lost) {
        IdlePump<TTag> pump = new(capacity: capacity, lost: lost);
        return Subscription.Attach<EventHandler>(
                subscribe: handler => RhinoApp.Idle += handler,
                unsubscribe: handler => RhinoApp.Idle -= handler,
                handler: pump.OnIdle)
            .Map(attached => {
                pump.subscription = attached;
                return pump;
            });
    }

    internal Fin<Unit> Enqueue(TTag tag, Func<bool> alive, Func<Fin<Unit>> run) {
        if (!open.Value) {
            return Fin.Succ(HostEdge.Side(() => lost(PumpLoss.Cancelled, tag)));
        }
        PumpWork work = new(Tag: tag, Alive: alive, Run: run);
        return Cell.Step(
                cell: pending,
                step: rows => rows.Count < capacity.Value ? Some(rows.Add(work)) : None,
                declined: new KernelFault.InvalidResult())
            .Switch(
                state: (Lost: lost, Tag: tag),
                committed: static (_, _) => Fin.Succ(unit),
                ceded: static (ctx, _) => Fin.Succ(HostEdge.Side(() => ctx.Lost(PumpLoss.Overflow, ctx.Tag))),
                refused: static (ctx, _) => Fin.Succ(HostEdge.Side(() => ctx.Lost(PumpLoss.Overflow, ctx.Tag))),
                contended: static (ctx, _) => Fin.Succ(HostEdge.Side(() => ctx.Lost(PumpLoss.Overflow, ctx.Tag))));
    }

    internal SubscriptionRelease Close() {
        _ = open.Swap(static _ => false);
        Drained().Iter(work => lost(PumpLoss.Cancelled, work.Tag));
        Subscription? claimed = subscription;
        subscription = null;
        return claimed?.Close() ?? new SubscriptionRelease.Released(Attempted: 0);
    }

    public void Dispose() => ignore(Close());

    private void OnIdle(object? _, EventArgs __) {
        Seq<PumpWork> works = Drained();
        if (works.IsEmpty) {
            return;
        }
        Fin<Unit> crossed = UiThread.Run(
            new UiDispatch<Unit>.Blocking(() => Fin.Succ(RunAll(works))),
            DispatchLane.Deferred);
        _ = crossed.IfFail(_ => RunAll(works));
    }

    private Seq<PumpWork> Drained() => Cell.Take(pending) switch {
        Transition<Seq<PumpWork>>.Committed committed => committed.State,
        _ => Seq<PumpWork>(),
    };

    private Unit RunAll(Seq<PumpWork> works) => works.Fold(unit, (_, work) => work.Alive()
        ? ignore(Try.lift(work.Run).Run().Bind(static inner => inner))
        : HostEdge.Side(() => lost(PumpLoss.Cancelled, work.Tag)));

    private readonly record struct PumpWork(TTag Tag, Func<bool> Alive, Func<Fin<Unit>> Run);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

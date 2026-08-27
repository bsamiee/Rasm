# [RASM_HOOKS]

`Rasm.Domain` owns the branch's one extension mechanism: a hook point is a declared seat on a folder's roster, a bus is the per-composition capsule that fires it, and a mount is the ask-and-grant binding a plugin rider claims. Veto admission, observe isolation, replay retention, bounded fault custody, scoped release, and the frozen mount census are this owner's — a folder declares its `<Package>Point` roster and its closed fact union and nothing else.

`Rasm.Domain` closes the mechanism against re-spelling: the roster is a TYPE PARAMETER, so a folder minting ids as inline `HookId.Create` literals does not compile; the fact family is a closed union, so a stringly payload cannot enter; the fault cell is bounded, so a tap storm sheds rather than growing for process lifetime; and the span bracket is an open floor the signal capsule conforms to, so tracing composes downward with no upward edge.

## [01]-[INDEX]

- [02]-[HOOK_POINT]: `HookId`, `TraceScope`, `HookModality`, `IsolatedFault`, `HookDetacher`, `IHookPoint`, `HookPoint<TFact>` — the seat, its plane and delivery semantics, and the fire/veto/observe/drain capsule.
- [03]-[HOOKS]: `IHookRoster<TSelf>`, `IHookSpan`, `HookGate`, `HookTap`, `Ring<T>`, `FaultCell`, `HookSet<TPoint,TFact,TOwner>` — the one per-composition bus over a folder's roster.
- [04]-[HOOK_MOUNT]: `IHookBinding`, `HookBinding`, `HookMounts<TPoint,TOwner>` — ask-and-grant seats with rider custody and partial-mount rollback.
- [05]-[HOOK_REGISTRY]: `HookRegistry` — the composition's one frozen point census.

## [02]-[HOOK_POINT]

- Owner: `HookId` keys points under the solution grammar `rasm.<pkg>.<domain>.<point>`; `TraceScope` is the `rasm.<package>.<plane>` plane identity a hook roster row, an instrument mount, and a span bracket all read — seated HERE because this page is the lowest of its three readers, so the causal frame above and the span band above both point down; `HookModality` is the delivery-capability vocabulary carrying `CanVeto` and its option-shaped `Retention` depth as row data, so veto admission and replay retention are the modality's own columns and a held set is a `CapabilitySet<HookModality>`; `IsolatedFault` is the point-attributed, clock-stamped evidence a shielded subscriber parks — the cell stamps arrival off its own `TimeProvider`, so a parked fault is orderable evidence and never a bare pair; `HookDetacher` is the detach value every attach returns; `IHookPoint` is the untyped census view a registry freezes; `HookPoint<TFact>` is the INTERNAL capsule holding one seat's vetoes, taps, and retention buffer — only the bus constructs and fires it, and an external consumer reads the untyped census alone.
- Cases: `Veto` transforms or refuses, `Observe` taps fault-isolated, `Replay` buffers for late drain. Each roster row declares a SET of modalities and both admission gates read the set's own COLUMNS, so a point that both vetoes and retains is one row rather than two seats — a row-identity probe against `Replay` is the deleted form, and so is a second `Admits` member beside `CapabilitySet.Admits`.
- Entry: `Fire` discriminates by call shape — unary publishes a settled fact, the guarded form hands its body the ADMITTED fact so a veto transform reaches the point it guards and runs observe taps only from its success path; `Veto`, `Observe`, and `Drain` are the subscriber entries, and `Observe` discriminates its arm's result shape so an effectful tap and a typed-result projection reach one entry. Every delegate admission refuses on the typed result through `Admit.Need`, so no null reaches mount or dispatch and no argument contract survives beside the bus on one owner.
- Auto: fire order is law — retention first so replay truth is the last fact even under a veto refusal; the veto left-fold second in ATTACH order, its first refusal the verdict parked beside the return; observe taps last, each forked before its shielded run so the synchronous path returns without waiting. Veto gates fold INSIDE `Try.lift`, so a throwing gate parks as evidence instead of escaping into the host callback that fired it. Fork refusals and throwing taps park as `IsolatedFault` while delivery continues; a replay point prunes its buffer oldest-first per fire and hands a fresh subscriber the held window on attach.
- Law: retention is the MODALITY's one option-shaped column, not a capsule constructor knob — `Retention` is `None` on a non-retaining row and a positive bounded `Dimension` on a retaining one, so a depth beside a non-retaining point is unrepresentable rather than a dead parameter.
- Law: `CanVeto` is the delivery discriminant BOTH gates read — `Veto` admits a point holding any vetoing row, `Observe` a point holding any non-vetoing one — so neither gate names `Veto`, `Observe`, or `Replay` by identity and a fourth delivery semantics joins both gates by declaring its column alone.
- Law: every capture funnels through `Try.lift`, so a cancelled subscriber keeps `KernelFault.Cancelled` instead of parking as an ordinary isolated fault; a bare `Try.lift(...).Run().Match` on this capsule is the deleted form.
- Law: a point mints nothing — the fire IS the evidence event and the emitter's typed result already carries the fact; the shared `FaultCell` records veto refusals and shielded tap faults point-attributed.
- Packages: Thinktecture.Runtime.Extensions for the generated owners, LanguageExt.Core for the `Fin`/`IO`/`Seq`/`Atom` types, BCL inbox.
- Growth: a new delivery semantics is one `HookModality` row with its column values, breaking every modality dispatch at compile time; a consuming folder's new point is one row on its own `<Package>Point` roster — the capsule type never widens per folder.
- Boundary: `TFact` closes at declaration as the owning folder's closed union, so a stringly payload cannot enter the bus; a subscriber failure is evidence or a refusal, never a broken emitter or a starved sibling, because every tap runs inside its own shield. Evidence cells enter as constructor material from the owning composition, never process-static — two compositions in one process hold two cells.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct HookId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", var pkg, var domain, var point]
            && pkg.Length > 0 && domain.Length > 0 && point.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-' or '_')
            ? null
            : new ValidationError(message: $"HookId requires the rasm.<pkg>.<domain>.<point> grammar: {value}");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct TraceScope {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", var package, var plane]
            && package.Length > 0 && plane.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-' or '_')
            ? null
            : new ValidationError(message: $"TraceScope requires the rasm.<package>.<plane> grammar: {value}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookModality : ICapability<HookModality> {
    public static readonly HookModality Veto = new("veto", canVeto: true, retention: None);
    public static readonly HookModality Observe = new("observe", canVeto: false, retention: None);
    public static readonly HookModality Replay = new("replay", canVeto: false, retention: Some(Dimension.Create(value: 64)));

    public bool CanVeto { get; }

    public Option<Dimension> Retention { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct IsolatedFault(HookId Point, Error Cause, DateTimeOffset At);

[StructLayout(LayoutKind.Auto)]
public readonly record struct HookDetacher(Action Detach) : IDisposable {
    public void Dispose() => Detach();
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IHookPoint {
    HookId Id { get; }
    CapabilitySet<HookModality> Modalities { get; }
}

internal sealed class HookPoint<TFact>(HookId id, CapabilitySet<HookModality> modalities, FaultCell faults) : IHookPoint {
    private readonly Atom<Seq<Func<TFact, Fin<TFact>>>> vetoes = Atom(Seq<Func<TFact, Fin<TFact>>>());
    private readonly Atom<Seq<Func<TFact, IO<Unit>>>> taps = Atom(Seq<Func<TFact, IO<Unit>>>());
    private readonly Atom<Seq<TFact>> buffer = Atom(Seq<TFact>());
    public HookId Id { get; } = id;
    public CapabilitySet<HookModality> Modalities { get; } = modalities;

    public Fin<TFact> Fire(TFact fact) => Fire(fact: fact, body: Fin.Succ);

    public Fin<T> Fire<T>(TFact fact, Func<TFact, Fin<T>> body) =>
        from guarded in Admit.Need(body)
        from _ in Fin.Succ(Retain(fact: fact))
        from admitted in Admitted(fact: fact)
        from value in guarded(admitted)
        select (taps.Value.Iter(tap => Forked(fact: admitted, tap: tap)), value).Item2;

    public Fin<IDisposable> Veto(Func<TFact, Fin<TFact>> gate) =>
        from admitted in Admit.Need(gate)
        from _ in guard(Modalities.Held.Exists(static row => row.CanVeto), (Error)new KernelFault.InvalidValue(Label: Id.ToValue(), Requirement: "a veto-capable point"))
        select Attach(cell: vetoes, row: admitted);

    public Fin<IDisposable> Observe(Func<TFact, IO<Unit>> tap) =>
        from admitted in Admit.Need(tap)
        from _ in guard(Modalities.Held.Exists(static row => !row.CanVeto), (Error)new KernelFault.InvalidValue(Label: Id.ToValue(), Requirement: "an observable point"))
        select (Attach(cell: taps, row: admitted),
            buffer.Value.Iter(held => Forked(fact: held, tap: admitted))).Item1;

    public Fin<IDisposable> Observe(Func<TFact, Fin<Unit>> arm) =>
        Observe(tap: fact => IO.lift(() => arm(fact)));

    public Seq<TFact> Drain() => buffer.Value;

    private Fin<TFact> Admitted(TFact fact) =>
        vetoes.Value.Fold(Fin.Succ(fact), (state, veto) => state.Bind(admitted => Try.lift(() => veto(admitted)).Run().Bind(static inner => inner)))
            .MapFail(refusal => (faults.Park(point: Id, cause: refusal), refusal).Item2);

    private Unit Retain(TFact fact) =>
        toSeq(Modalities.Held).Choose(static row => row.Retention).Head.Iter(
            depth => ignore(buffer.Swap(held => held.Add(fact) is var next && next.Count > depth.Value
                ? next.Skip(next.Count - depth.Value).Strict()
                : next)));

    private Unit Forked(TFact fact, Func<TFact, IO<Unit>> tap) =>
        Try.lift(() => IO.lift(() => Try.lift(() => tap(fact).Run()).Run().Bind(static inner => inner)
                .IfFail(cause => ignore(faults.Park(point: Id, cause: cause))))
            .Fork(None).Run().Map(static _ => unit)).Run().Bind(static inner => inner)
        .IfFail(cause => ignore(faults.Park(point: Id, cause: cause)));

    private static IDisposable Attach<T>(Atom<Seq<T>> cell, T row) {
        ignore(cell.Swap(held => held.Add(row)));
        return new HookDetacher(Detach: () => ignore(cell.Swap(held => held.Filter(entry => !ReferenceEquals(entry, row)).Strict())));
    }
}
```

## [03]-[HOOKS]

- Owner: `IHookRoster<TSelf>` is what a folder's `<Package>Point` vocabulary realizes, so the bus takes the roster as a TYPE PARAMETER and seats mint from `TPoint.Items` alone; `IHookSpan` is the open bracket floor the signal capsule conforms to; `HookGate` and `HookTap` are the subscriber rows; `Ring<T>` is the ONE versioned bounded oldest-out ring, `RingSettlement<T>` its park-and-cleanup settlement, and `FaultCell` its isolated-fault instance with the stamp clock; `HookSet<TPoint,TFact,TOwner>` is the one bus per composition, roster, and owner key.
- Cases: `RingSettlement<T>` is `Landed` with settled state and cleanup fold, `Ceded` when another writer moved the ring, or `Refused` when no candidate exists; `TOwner` is the identity a subscription and a rider belong to — `TelemetrySource` inside a library tier, `PluginKey` at the Rhino boundary, `HookScope` at the Grasshopper boundary — so scoped release is one shape at every stratum and a collectible load context drops exactly its own subscriptions.
- Entry: `Ring.Park(item)` lands an ordinary row and `Park(item, release)` a custodial row; `Of` mints the bus from its gate and tap rows, an optional span floor, and the composition's own evidence cell; `Fire` is the ONE raise in both arities; `Drain` reads a retaining point's held window; `Replay` RE-FIRES a captured window through `TraverseM` over `Fire`, refusing on a point whose roster row does not retain; `Detach` tears the whole bus down in reverse registration order; `Release` drops one owner's subscriptions.
- Auto: `Points` is the census a `HookRegistry` freezes, derived from `TPoint.Items` so a point outside the roster is unrepresentable rather than merely undeclared. Taps naming a `Scope` pay nothing on the fires they ignore. `Replay` is a re-fire with a verdict result, never a buffer read — the six per-folder `Admitted`/`Settled`/`Planned`/`Ran`/`Marked` members those folders declare become call sites of `Fire`.
- Law: the bus composes tracing through `IHookSpan` and never through the signal capsule's own type, so this page depends downward and the capsule conforms upward — an `Option<SpanBand>` parameter here inverts the dependency the telemetry split exists to straighten. `IHookSpan` takes the PLANE as an argument because the roster row already carries it: the bus reads `TPoint.Plane` at the fire site, so one band serves every plane a composition mounts and a per-plane band roster never appears.
- Law: the evidence cell arrives WHOLE from the composition, never as a clock beside a cap — one `FaultCell` carries the stamp source and the ceiling together, so the tenancy stamp, the interaction shield, and every bus in one process park on one ring and a composition replaying under a fake `TimeProvider` reads deterministic evidence at all three.
- Law: a bounded fault cell sheds rather than grows — `Ring<T>` is the ONE bounded ring (`Park` past its `Dimension` cap evicts OLDEST-FIRST and counts the eviction onto `Shed`), `FaultCell` is `Ring<IsolatedFault>` with the stamp clock, so a tap storm is observable as a number instead of as process memory; an unbounded cell, and a boundary journal or ledger re-declaring cap + oldest-out + shed over its own payload, are the deleted forms.
- Law: eviction ownership transfers only after the versioned whole-state CAS lands. `Landed` carries the reverse/all-attempted cleanup fold beside the settled state, including cleanup failure; retrying that settlement would duplicate an item already parked. `Ceded`/`Refused` alone mean not-landed. `Shed` counts evicted rows and `Lost` counts parks that never landed; cleanup evidence remains on the landed settlement and merges with neither counter.
- Law: `Detach` and owner-scoped `Release` run every registered detacher in reverse order even when one throws, and the whole refusal set settles through `Error.Many` — a teardown that discards a fault or abandons on its first refusal is the deleted form.
- Output: `Faults` is the parked evidence, `Shed` its overflow count, and `Lost` its declined-park count; none is a fire outcome, so a consumer reads them and never branches a fire on them.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`TimeProvider`).
- Growth: a new subscriber kind is one row type composed into `Of`; a new folder is one roster realizing `IHookRoster<TSelf>` with one closed fact union realizing `IHookFact<TPoint>` — its `Seats` a one-line derivation from the union's own fact→point map — and zero bus members.
- Law: `Seats` is the fact union's OWN declared correspondence and `Fire` gates on it twice — at entry (an emitter pairing a fact with a foreign point refuses before any veto runs) and on the veto fold's product (a gate rewriting to a sibling case that does not seat at the point refuses before the body or any tap). A 1:1 union derives `Seats` from its `At`/`Point` map; a broadcast fact answers the point set it lawfully fans to; a case entering only through `Replay` seats at its journal points and says so.
- Boundary: NAMED LOSS (narrowed by E-M16) — folding the per-folder buses onto one mechanism erases the per-point FACT TYPE at compile time: a subscriber to a named `HookPoint<BimFact.Imported>` field could not receive an exported fact, while under one bus every point on a roster shares one `TFact` and subscribers discriminate on the case. What survives is the roster row's modality admission, the union's closure (a foreign case is unspellable), AND per-point fact-CASE narrowing as the RUNTIME `Seats` gate derived from the union's declared correspondence — only the compile-time shape of the narrowing is lost. The roster-COLUMN form was refused: the census view's law bars a `Type` column, and the correspondence is the fact's, not the point's. WITNESS — `Rasm.Bim/Model/observability.md:211-252`'s fourteen `HookPoint<BimFact.*>` columns, its fourteen-line `Live()`, its fourteen-entry census, and its private `Seat<TFact>` mint become one roster, one fact union, and one `HookSet<BimPoint, BimFact, TelemetrySource>.Of(taps: taps)`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Numerics;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IHookRoster<TSelf> where TSelf : IHookRoster<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    HookId Id { get; }
    CapabilitySet<HookModality> Modalities { get; }
    Option<TraceScope> Plane { get; }
}

public interface IHookFact<TPoint> where TPoint : IHookRoster<TPoint> {
    bool Seats(TPoint at);
}

public interface IHookSpan {
    Fin<T> Traced<T>(TraceScope plane, Func<Fin<T>> body);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HookGate<TPoint, TFact, TOwner>(TPoint Point, Func<TFact, Fin<TFact>> Admit, Option<TOwner> Owner = default)
    where TPoint : IHookRoster<TPoint>
    where TOwner : notnull;

public sealed record HookTap<TPoint, TFact, TOwner>(Func<TFact, Fin<Unit>> Observe, Option<Seq<TPoint>> Scope = default, Option<TOwner> Owner = default)
    where TPoint : IHookRoster<TPoint>
    where TOwner : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
[Union]
public abstract partial record RingSettlement<T> {
    private RingSettlement() { }
    public sealed record Landed(Seq<T> State, Fin<Unit> Cleanup) : RingSettlement<T>;
    public sealed record Ceded(Seq<T> State) : RingSettlement<T>;
    public sealed record Refused(Seq<T> State, Error Cause) : RingSettlement<T>;
}

public sealed class Ring<T>(Dimension cap) {
    private sealed record RingState(long Version, Seq<T> Items);

    private readonly Atom<RingState> held = Atom(new RingState(Version: 0L, Items: Seq<T>()));
    private readonly Atom<long> shed = Atom(0L);
    private readonly Atom<long> lost = Atom(0L);

    public Seq<T> Parked => held.Value.Items;
    public long Shed => shed.Value;
    public long Lost => lost.Value;

    public RingSettlement<T> Park(T item) => Park(item: item, release: None);

    public RingSettlement<T> Park(T item, Func<T, Fin<Unit>> release) {
        ArgumentNullException.ThrowIfNull(release);
        return Park(item: item, release: Some(release));
    }

    private RingSettlement<T> Park(T item, Option<Func<T, Fin<Unit>>> release) {
        Seq<T> evicted = Seq<T>();
        Transition<RingState> transition = Cell.Step(
            cell: held,
            step: standing => {
                if (standing.Version == long.MaxValue) { return Option<RingState>.None; }
                int dropped = standing.Items.Count >= cap.Value ? standing.Items.Count - cap.Value + 1 : 0;
                evicted = standing.Items.Take(dropped).Strict();
                return Some(new RingState(
                    Version: standing.Version + 1L,
                    Items: standing.Items.Skip(dropped).Add(item).Strict()));
            },
            declined: new KernelFault.InvalidValue(
                Label: nameof(Ring<T>), Requirement: "a ring version below Int64.MaxValue"));

        static RingSettlement<T> Missed(Atom<long> lost, RingSettlement<T> settlement) {
            ignore(lost.Swap(static count => Saturating(count: count, delta: 1L)));
            return settlement;
        }

        return transition.Switch(
            state: (Evicted: evicted, Release: release, Shed: shed, Lost: lost),
            committed: static (state, committed) => {
                ignore(state.Shed.Swap(count => Saturating(count: count, delta: state.Evicted.Count)));
                Fin<Unit> cleanup = state.Release.Match(
                    Some: dispose => Custody.Release(held: state.Evicted, release: dispose),
                    None: static () => Fin.Succ(unit));
                return (RingSettlement<T>)new RingSettlement<T>.Landed(State: committed.State.Items, Cleanup: cleanup);
            },
            ceded: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Ceded(row.State.Items)),
            refused: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Refused(row.State.Items, row.Cause)),
            contended: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Ceded(row.State.Items)));
    }

    private static long Saturating(long count, long delta) =>
        count >= long.MaxValue - delta ? long.MaxValue : count + delta;
}

public sealed class FaultCell(Dimension cap, TimeProvider clock) {
    private readonly Ring<IsolatedFault> ring = new(cap: cap);

    public Seq<IsolatedFault> Parked => ring.Parked;
    public long Shed => ring.Shed;
    public long Lost => ring.Lost;

    public RingSettlement<IsolatedFault> Park(HookId point, Error cause) =>
        ring.Park(item: new IsolatedFault(Point: point, Cause: cause, At: clock.GetUtcNow()));
}

public sealed class HookSet<TPoint, TFact, TOwner>
    where TPoint : IHookRoster<TPoint>
    where TFact : IHookFact<TPoint>
    where TOwner : notnull {
    private static readonly Dimension DefaultCap = Dimension.Create(value: 512);
    private readonly HashMap<TPoint, HookPoint<TFact>> seats;
    private readonly Option<IHookSpan> span;
    private readonly Atom<Seq<(Option<TOwner> Owner, IDisposable Detach)>> subscriptions = Atom(Seq<(Option<TOwner> Owner, IDisposable Detach)>());

    private HookSet(HashMap<TPoint, HookPoint<TFact>> seats, FaultCell faults, Option<IHookSpan> span) =>
        (this.seats, Faults, this.span) = (seats, faults, span);

    public static Fin<HookSet<TPoint, TFact, TOwner>> Of(Seq<HookGate<TPoint, TFact, TOwner>> gates = default,
        Seq<HookTap<TPoint, TFact, TOwner>> taps = default,
        Option<IHookSpan> span = default,
        Option<FaultCell> cell = default) {
        FaultCell faults = cell.IfNone(static () => new FaultCell(cap: DefaultCap, clock: TimeProvider.System));
        HookSet<TPoint, TFact, TOwner> hooks = new(
            seats: toSeq(TPoint.Items).ToHashMap(static row => row, row => new HookPoint<TFact>(id: row.Id, modalities: row.Modalities, faults: faults)),
            faults: faults, span: span);
        Seq<(Option<TOwner> Owner, Func<Fin<IDisposable>> Attach)> plan =
            gates.Map(gate => (gate.Owner, Attach: new Func<Fin<IDisposable>>(() => hooks.Seat(at: gate.Point).Bind(seat => seat.Veto(gate: gate.Admit)))))
            + taps.Bind(tap => tap.Scope.IfNone(toSeq(TPoint.Items)).Map(point =>
                (tap.Owner, Attach: new Func<Fin<IDisposable>>(() => hooks.Seat(at: point).Bind(seat => seat.Observe(arm: tap.Observe, key: tap.Name))))));
        return plan.Fold(Fin.Succ(Seq<(Option<TOwner> Owner, IDisposable Detach)>()), (held, row) => held.Bind(taken =>
                row.Attach().Match(
                    Succ: detach => Fin.Succ(taken.Add((row.Owner, detach))),
                    Fail: refusal => Fin.Fail<Seq<(Option<TOwner> Owner, IDisposable Detach)>>(refusal)
                        .Rollback(held: taken, release: static row => { row.Detach.Dispose(); return Fin.Succ(unit); }))))
            .Map(taken => (ignore(hooks.subscriptions.Swap(_ => taken)), hooks).Item2);
    }

    public Seq<IHookPoint> Points => toSeq(TPoint.Items).Map(row => (IHookPoint)seats[row]);
    public FaultCell Faults { get; }

    public Fin<TFact> Fire(TPoint at, TFact fact) => Fire(at: at, fact: fact, body: Fin.Succ);
    public Fin<T> Fire<T>(TPoint at, TFact fact, Func<TFact, Fin<T>> body) =>
        Seated(at: at, fact: fact).Bind(_ => Seat(at: at).Bind(seat =>
            Traced(at: at, body: () => seat.Fire(fact: fact, body: admitted => Seated(at: at, fact: admitted).Bind(body)))));

    public Seq<TFact> Drain(TPoint at) => seats.Find(at).Map(static seat => seat.Drain()).IfNone(Seq<TFact>());

    public Fin<Unit> Replay(TPoint at, Seq<TFact> captured) =>
        at.Modalities.Held.Exists(static row => row.Retention.IsSome)
            ? captured.TraverseM(fact => Fire(at: at, fact: fact)).As().Map(static _ => unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue(Label: at.Id.ToValue(), Requirement: "a retaining point"));

    public Fin<Unit> Detach() =>
        Unwind(taken: Cell.Take(cell: subscriptions).Current, key: Op.Of());

    public Fin<Unit> Release(TOwner scope) {
        Seq<(Option<TOwner> Owner, IDisposable Detach)> mine = subscriptions.Value.Filter(row => row.Owner.Exists(owner => owner.Equals(scope))).Strict();
        if (mine.IsEmpty) { return Fin.Fail<Unit>(new KernelFault.InvalidValue(Label: scope.ToString() ?? nameof(scope), Requirement: "an owner holding at least one subscription")); }
        Fin<Unit> released = Unwind(taken: mine);
        ignore(subscriptions.Swap(held => held.Filter(row => !mine.Exists(taken => ReferenceEquals(taken.Detach, row.Detach))).Strict()));
        return released;
    }

    private Fin<HookPoint<TFact>> Seat(TPoint at) => seats.Find(at).ToFin(new KernelFault.InvalidInput());
    private static Fin<TFact> Seated(TPoint at, TFact fact) =>
        fact.Seats(at) ? Fin.Succ(fact) : Fin.Fail<TFact>(new KernelFault.InvalidInput(Axis: Some(at.Id.ToValue())));
    private Fin<T> Traced<T>(TPoint at, Func<Fin<T>> body) =>
        (span, at.Plane) switch {
            ({ IsSome: true, Case: IHookSpan bracket }, { IsSome: true, Case: TraceScope plane }) => bracket.Traced(plane: plane, body: body),
            _ => body(),
        };
    private static Fin<Unit> Unwind(Seq<(Option<TOwner> Owner, IDisposable Detach)> taken) =>
        Custody.Release(held: taken, release: static row => { row.Detach.Dispose(); return Fin.Succ(unit); });
}
```

## [04]-[HOOK_MOUNT]

- Owner: `IHookBinding<TPoint,TOwner>` the public heterogeneous floor that lets a binding roster live in one census without erasing its ask and grant types; `HookBinding<TPoint,TOwner,TAsk,TGrant>` the typed seat a rider claims; `HookMounts<TPoint,TOwner>` the seat table with rider custody, partial-mount rollback, and the bind entry.
- Entry: `Mount` seats one binding and returns its `Lease<IDisposable>`; `MountAll` folds a roster and rolls back every seated binding on the first refusal, so a partial mount never survives; `Bind` resolves one point's grant for one owner.
- Auto: seat transitions ride `Cell.Claim`, so a losing contender reads `Ceded` and drops what it staged against the winner's identity rather than re-deriving the outcome from a state both hold — the three-way reference probe a host registry hand-rolls today is the deleted form.
- Law: ask and grant stay TYPED — the census holds `IHookBinding<TPoint,TOwner>` and each row keeps its `TAsk`/`TGrant`, so no `object`/`Type` erasure pair enters the mechanism and a mismatched claim fails at the call site rather than at a cast.
- Law: `Census` and `Riders` are MOUNT-ORDERED — every seat carries the ordinal its claim minted and both projections sort on it, so a raise fanning over a point's riders reaches them in registration order and a veto left-fold over riders is deterministic; a set-shaped census is the deleted form. Modality is a POINT fact, never a binding fact: `HookPoint.Veto` refuses a point holding no vetoing row and `Observe` a point holding no non-vetoing one, both reading the modality's own `CanVeto` column, so a mount carries no modality to check.
- Law: rollback is reverse-seat order and every disposer runs even when one throws, the whole cleanup set appending to the primary refusal through the kernel custody fold.
- Output: `Census` and `Riders` are the audit projection a composition reads; neither mutates.
- Growth: a new host seat is one `HookBinding` row on the folder's own roster; a new rider is one entry in `Riders`.
- Boundary: keyed instances stay the folder's — a `(point, scope)` seat and a plugin-rider seat are earned by grant custody under branch RULINGS `[02]`, and what those folders need from the kernel is `TOwner` typed once, never a kernel-side registry of their instances.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Domain;

// --- [SERVICES] ------------------------------------------------------------------------
public interface IHookBinding<TPoint, TOwner>
    where TPoint : IHookRoster<TPoint>
    where TOwner : notnull {
    TPoint Point { get; }
    TOwner Owner { get; }
    Fin<Lease<IDisposable>> Mount(HookMounts<TPoint, TOwner> mounts);
}

public sealed record HookBinding<TPoint, TOwner, TAsk, TGrant>(TPoint Point, TOwner Owner, Func<TAsk, Fin<TGrant>> Bind)
    : IHookBinding<TPoint, TOwner>
    where TPoint : IHookRoster<TPoint>
    where TOwner : notnull {
    Fin<Lease<IDisposable>> IHookBinding<TPoint, TOwner>.Mount(HookMounts<TPoint, TOwner> mounts) => mounts.Mount(binding: this);
}

public sealed class HookMounts<TPoint, TOwner>
    where TPoint : IHookRoster<TPoint>
    where TOwner : notnull {
    private readonly Atom<HashMap<(TPoint Point, TOwner Owner), (long Ordinal, IHookBinding<TPoint, TOwner> Binding)>> seats =
        Atom(HashMap<(TPoint Point, TOwner Owner), (long Ordinal, IHookBinding<TPoint, TOwner> Binding)>());
    private readonly Atom<long> minted = Atom(0L);

    public Seq<IHookBinding<TPoint, TOwner>> Census => toSeq(seats.Value.Values.OrderBy(static row => row.Ordinal).Select(static row => row.Binding)).Strict();
    public Seq<(TPoint Point, Seq<TOwner> Riders)> Riders =>
        toSeq(TPoint.Items).Map(point => (Point: point, Riders: Census.Filter(row => row.Point.Equals(point)).Map(static row => row.Owner).Strict()));

    public Fin<Lease<IDisposable>> Mount<TAsk, TGrant>(HookBinding<TPoint, TOwner, TAsk, TGrant> binding) =>
        Cell.Claim(cell: seats, key: (binding.Point, binding.Owner), mint: () => (Ordinal: minted.Swap(static n => n + 1), Binding: (IHookBinding<TPoint, TOwner>)binding)) is Transition<HashMap<(TPoint Point, TOwner Owner), (long Ordinal, IHookBinding<TPoint, TOwner> Binding)>>.Committed
            ? Fin.Succ<Lease<IDisposable>>(new Lease<IDisposable>.Owned(new HookDetacher(Detach: () => ignore(seats.Swap(held => held.Remove((binding.Point, binding.Owner)))))))
            : Fin.Fail<Lease<IDisposable>>(new KernelFault.InvalidValue(Label: $"{binding.Point.Id}/{binding.Owner}", Requirement: "an unclaimed seat"));

    public Fin<Seq<Lease<IDisposable>>> MountAll(Seq<IHookBinding<TPoint, TOwner>> bindings) =>
        bindings.Fold(Fin.Succ(Seq<Lease<IDisposable>>()), (held, binding) => held.Bind(taken =>
            binding.Mount(mounts: this).Match(
                Succ: lease => Fin.Succ(taken.Add(lease)),
                Fail: refusal => Fin.Fail<Seq<Lease<IDisposable>>>(refusal)
                    .Rollback(held: taken, release: static lease => Fin.Succ(lease.Dispose())))));

    public Fin<TGrant> Bind<TAsk, TGrant>(TPoint point, TOwner owner, TAsk ask) =>
        seats.Value.Find((point, owner)).Map(static row => row.Binding).ToFin(new KernelFault.InvalidInput()).Bind(row => row switch {
            HookBinding<TPoint, TOwner, TAsk, TGrant> typed => typed.Bind(arg: ask),
            _ => Fin.Fail<TGrant>(new KernelFault.InvalidValue(Label: $"{point.Id}/{owner}", Requirement: $"a binding from {typeof(TAsk).Name} to {typeof(TGrant).Name}")),
        });
}
```

## [05]-[HOOK_REGISTRY]

- Owner: `HookRegistry` — the composition's ONE frozen `HookId` census, minted by folding every contributing bus's `Points`; the key is the generated owner, so collision, equality, and lookup ride its declared ordinal comparer and no formatting round-trip enters.
- Entry: `Mount(params ReadOnlySpan<IHookPoint>)` is that one freeze, returning `Fin` so a duplicate id names both owners instead of throwing at a frozen-dictionary merge.
- Law: the freeze is the composition's alone — a peer reaching `HookRegistry.Mount` itself splits the audit into partial tables, so every contributing bus hands its census IN and calls nothing (branch RULINGS `[02]`).
- Law: a fired id outside the frozen table is unreachable by construction, because firing takes a declared roster VALUE and the roster is the type parameter.
- Growth: a new contributing bus is one argument at the composition root.
- Boundary: the registry is an audit surface, never a dispatch surface — nothing fires through it, and a lookup that returns a point for firing is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;

namespace Rasm.Domain;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record HookRegistry(FrozenDictionary<HookId, IHookPoint> Points) {
    public static Fin<HookRegistry> Mount(params ReadOnlySpan<IHookPoint> points) {
        Seq<IHookPoint> rows = Iterable<IHookPoint>.FromSpan(points).ToSeq();
        Seq<HookId> collided = rows.Collisions(static row => row.Id);
        return collided.IsEmpty
            ? Fin.Succ(new HookRegistry(Points: rows.ToFrozenDictionary(static row => row.Id, static row => row)))
            : Fin.Fail<HookRegistry>(new KernelFault.InvalidValue(Label: string.Join(", ", collided.Map(static id => id.ToValue())), Requirement: "one point per id across every contributing bus"));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

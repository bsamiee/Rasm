# `hooks.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/hooks.md`

The hook algebra, fire order, bounded custody, typed grants, and composition-frozen census remain unchanged. This ordered plan removes one invalid retention product, two unused public capsule projections, one impossible value-type null guard, one public-signature accessibility defect, and only those private helpers whose bodies become clearer at their sole call site. Ordered total for the target fence: **9 moves, -25 nonblank fenced LOC, -9 authored module members, and -3 net public surface entries**. No new module-level type, helper, abstraction, package, compatibility layer, or relocated logic is introduced.

## Move 1 — Make retention one option-shaped policy column

### 1A — Replace `Retains` plus sentinel depth

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:29`, anchored at the first fence imports; `:62`, anchored at `HookModality`.

**From:**

```csharp
using Thinktecture;

public static readonly HookModality Veto = new("veto", canVeto: true, retains: false, depth: 0);
public static readonly HookModality Observe = new("observe", canVeto: false, retains: false, depth: 0);
public static readonly HookModality Replay = new("replay", canVeto: false, retains: true, depth: 64);

public bool Retains { get; }
public int Depth { get; }
```

**To:**

```csharp
using Rasm.Numerics;
using Thinktecture;

public static readonly HookModality Veto = new("veto", canVeto: true, retention: None);
public static readonly HookModality Observe = new("observe", canVeto: false, retention: None);
public static readonly HookModality Replay = new("replay", canVeto: false, retention: Some(Dimension.Create(value: 64)));

public Option<Dimension> Retention { get; }
```

**Effect:** fenced LOC **0**; public members **-1**; types **0**. The removed property pays for the first-fence `Rasm.Numerics` import. `None` is non-retention and `Some(Dimension)` is positive bounded retention, eliminating `Retains == false` with positive depth and `Retains == true` with zero or negative depth. Thinktecture still generates the single smart-enum constructor; LanguageExt and the admitted `Dimension` carry absence and magnitude without a boolean mirror or integer sentinel.

### 1B — Consume the option at the two retention gates

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:134`, anchored at `Retain`; `:338`, anchored at `Replay`.

**From:**

```csharp
toSeq(Modalities.Held).Find(static row => row.Retains).Iter(
    row => ignore(buffer.Swap(held => held.Add(fact) is var next && next.Count > row.Depth
        ? next.Skip(next.Count - row.Depth).Strict()
        : next)));

at.Modalities.Held.Exists(static row => row.Retains)
```

**To:**

```csharp
toSeq(Modalities.Held).Choose(static row => row.Retention).Head.Iter(
    depth => ignore(buffer.Swap(held => held.Add(fact) is var next && next.Count > depth.Value
        ? next.Skip(next.Count - depth.Value).Strict()
        : next)));

at.Modalities.Held.Exists(static row => row.Retention.IsSome)
```

**Effect:** fenced LOC **0**; authored/public symbols **0**. `Seq.Choose` performs the filter-map to the admitted depth; `Head` preserves the current at-most-one retaining modality posture.

**API/consumer proof:** `libs/dotnet/.api/api-languageext.md:406-410,570` owns `Seq.Choose`, option-shaped `Head`, and presence reads; `Dimension` delegates to `Band.Count` at `libs/dotnet/Rasm/.planning/Numerics/atoms.md:97,121-125`, whose closed floor is one. The only external property read is `libs/dotnet/Rasm.Grasshopper/.planning/Shell/hooks.md:101`.

**Ripples:** update the target's `[02]-[HOOK_POINT]` and `[03]-[HOOKS]` prose to name optional retention; change Grasshopper `Replayable` from `row.Retains` to `row.Retention.IsSome`. No other hook consumer reads these members.

## Move 2 — Keep `HookId` on its generated outbound surface

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:112-117`, anchored at the veto/observe faults; `:341`, anchored at the replay fault; `:359`, anchored at `Seated`.

**From:**

```csharp
Label: Id.ToString()
Label: at.Id.ToString()
key.InvalidInput(at.Id.ToString())
```

**To:**

```csharp
Label: Id.ToValue()
Label: at.Id.ToValue()
key.InvalidInput(at.Id.ToValue())
```

Apply the first replacement at both `HookPoint.Veto` and `HookPoint.Observe`.

**Effect:** fenced LOC **0**; symbols **0**. These paths require the stored identity text, not presentation formatting.

**API/consumer proof:** `libs/dotnet/.api/api-thinktecture-runtime-extensions.md:134-151` verifies generated `Owner.ToValue() -> TKey`; the local catalogue explicitly rejects hand-written key-conversion helpers beside generated owners.

**Ripples:** none outside the target.

## Move 3 — Use each tap's declared operation key

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:318-319`, anchored at the tap half of `HookSet.Of`.

**From:**

```csharp
+ taps.Bind(tap => tap.Scope.IfNone(toSeq(TPoint.Items)).Map(point =>
    (tap.Owner, Attach: new Func<Fin<IDisposable>>(() => hooks.Seat(at: point, key: key).Bind(seat => seat.Observe(arm: tap.Observe, key: key))))));
```

**To:**

```csharp
+ taps.Bind(tap => tap.Scope.IfNone(toSeq(TPoint.Items)).Map(point =>
    (tap.Owner, Attach: new Func<Fin<IDisposable>>(() => hooks.Seat(at: point, key: key).Bind(seat => seat.Observe(arm: tap.Observe, key: tap.Name))))));
```

**Effect:** fenced LOC **0**; symbols **0**. The composition key continues to own seat lookup; `HookTap.Name` now owns replay, fork, and tap capture, the behavior its `Op` column represents. Removing `Name` would discard consumer-supplied identity rather than refine the type.

**Consumer proof:** distinct keys are already supplied by `Rasm.AppUi/.planning/Shell/hosts.md:129-132`, `Rasm.AppHost/.planning/Runtime/lifecycle.md:188-191`, `Rasm.AppHost/.planning/Runtime/profiles.md:533-536`, `Rasm.Bim/.planning/Exchange/events.md:168-171`, and the Bim/Element/Materials instrument taps. The target currently stores but never reads them.

**Ripples:** none; constructors already supply `Name`.

## Move 4 — Compress constructor-only state and close the point capsule

### 4A — Internalize and primary-construct `HookPoint<TFact>`

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:89-99`, anchored at `HookPoint<TFact>`.

Apply these ordered replacements.

**4A.1 — close the type:**

**From:**

```csharp
public sealed class HookPoint<TFact> : IHookPoint {
```

**To:**

```csharp
internal sealed class HookPoint<TFact> : IHookPoint {
```

**4A.2 — capture constructor state:**

**From:**

```csharp
internal sealed class HookPoint<TFact> : IHookPoint {
```

**To:**

```csharp
internal sealed class HookPoint<TFact>(HookId id, CapabilitySet<HookModality> modalities, FaultCell faults) : IHookPoint {
```

Then replace the now-captured field, constructor, and uninitialized projections:

**From:**

```csharp
private readonly FaultCell faults;

public HookPoint(HookId id, CapabilitySet<HookModality> modalities, FaultCell faults) =>
    (Id, Modalities, this.faults) = (id, modalities, faults);

public HookId Id { get; }
public CapabilitySet<HookModality> Modalities { get; }
```

**To:**

```csharp
public HookId Id { get; } = id;
public CapabilitySet<HookModality> Modalities { get; } = modalities;
```

**Effect:** fenced LOC **-3**; private members **-1**; public surface **-1 type**. Only `HookSet` constructs and operates the typed point; external consumers receive the untyped `IHookPoint` census through `Points`. Internalization makes “one mechanism” structural without moving the class.

### 4B — Primary-construct `Ring<T>`, remove the unread projection, and delete the impossible null guard

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:214-225`, anchored at `Ring<T>` and `Cap`.

**From:**

```csharp
public sealed class Ring<T> {
    private readonly Dimension cap;

    public Ring(Dimension cap) => this.cap = cap;

    public Dimension Cap => cap;
```

**To:**

```csharp
public sealed class Ring<T>(Dimension cap) {
```

Then delete the guard that boxes a non-nullable value only to prove it is not null:

**From:**

```csharp
ArgumentNullException.ThrowIfNull(key);
```

**To:**

```csharp
```

**Effect:** fenced LOC **-4**; authored members **-2**; public surface **-1 property**. Constructor shape and every `cap.Value` read remain unchanged. `Op` is a non-nullable `readonly partial struct` at `Domain/results.md:43`, so the deleted guard cannot refuse and only boxes the value. No consumer reads `Ring<T>.Cap`; all repository `.Cap` matches belong to other owners. The adjacent `release` guard remains because `Func<T, Fin<Unit>>` is a reference-typed public input and its runtime check is meaningful.

### 4C — Hide the raw ring behind `FaultCell`'s stamp gate

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:280-291`, anchored at `FaultCell`.

**From:**

```csharp
public sealed class FaultCell {
    private readonly TimeProvider clock;
    public FaultCell(Dimension cap, TimeProvider clock) => (Ring, this.clock) = (new Ring<IsolatedFault>(cap: cap), clock);

    public Ring<IsolatedFault> Ring { get; }
    public Seq<IsolatedFault> Parked => Ring.Parked;
    public long Shed => Ring.Shed;
    public long Lost => Ring.Lost;
```

**To:**

```csharp
public sealed class FaultCell(Dimension cap, TimeProvider clock) {
    private readonly Ring<IsolatedFault> ring = new(cap: cap);

    public Seq<IsolatedFault> Parked => ring.Parked;
    public long Shed => ring.Shed;
    public long Lost => ring.Lost;
```

At `Park`:

**From:**

```csharp
Ring.Park(item: new IsolatedFault(Point: point, Cause: cause, At: clock.GetUtcNow()));
```

**To:**

```csharp
ring.Park(item: new IsolatedFault(Point: point, Cause: cause, At: clock.GetUtcNow()));
```

**Effect:** fenced LOC **-2**; authored members **-1 net**; public surface **-1 property**. External parking can no longer bypass `Park(HookId, Error)` and its timestamp. Consumers read only `Parked`, `Shed`, `Lost`, and `Park`.

**Combined Move 4 proof:** primary constructors are ordinary modern C# state capture; no package API is involved. Repository search finds no construction or member access on `HookPoint<TFact>` outside this target and no read of `FaultCell.Ring` or `Ring<T>.Cap`; fault consumers read only `Parked`, `Shed`, `Lost`, and `Park`. The AppHost hook page names `HookPoint<TFact>` only in its settled-import prose, which is the explicit visibility ripple below rather than evidence that consumers construct it.

**Ripples:** update target `[02]-[HOOK_POINT]` to mark the capsule internal; remove `HookPoint<TFact>` from the public settled-import roster at `libs/dotnet/Rasm.AppHost/.planning/Observability/hooks.md:5`; delete the obsolete `SignalHooks`/per-gate `HookPoint<TFact>` ruling at `libs/dotnet/Rasm/RULINGS.md:20`, because line 19 and the current `HookSet`/`IHookFact` design already own the one mechanism and its seating gate. The remaining cross-package `HookPoint<...>` mentions explicitly describe the deleted pre-collapse form and stay as historical witnesses.

## Move 5 — Inline the three non-owning `HookPoint` helpers

### 5A — Attach and replay in `Observe`

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:115-123`, anchored at the `IO<Unit>` `Observe` and `Seated`.

**From:**

```csharp
public Fin<IDisposable> Observe(Func<TFact, IO<Unit>> tap, Op key) =>
    from admitted in key.Need(tap)
    from _ in guard(Modalities.Held.Exists(static row => !row.CanVeto), (Error)new KernelFault.InvalidValue(Label: Id.ToValue(), Requirement: "an observable point")).ToFin()
    select Seated(admitted: admitted, key: key);

private IDisposable Seated(Func<TFact, IO<Unit>> admitted, Op key) {
    IDisposable detach = Attach(cell: taps, row: admitted);
    return (ignore(buffer.Value.Iter(held => Forked(fact: held, tap: admitted, key: key))), detach).Item2;
}
```

**To:**

```csharp
public Fin<IDisposable> Observe(Func<TFact, IO<Unit>> tap, Op key) =>
    from admitted in key.Need(tap)
    from _ in guard(Modalities.Held.Exists(static row => !row.CanVeto), (Error)new KernelFault.InvalidValue(Label: Id.ToValue(), Requirement: "an observable point")).ToFin()
    select (Attach(cell: taps, row: admitted),
        buffer.Value.Iter(held => Forked(fact: held, tap: admitted, key: key))).Item1;
```

**Effect:** fenced LOC **-3**; private members **-1**. Tuple operands evaluate left-to-right, preserving attach-before-replay; `Seq.Iter` already returns `Unit`.

### 5B — Dispatch at the only call site

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:103-108`, anchored at generic `Fire<T>`; `:140`, anchored at `Dispatch`.

**From:**

```csharp
from value in guarded(admitted)
select (Dispatch(fact: admitted, key: key), value).Item2;

private Unit Dispatch(TFact fact, Op key) => ignore(taps.Value.Iter(tap => Forked(fact: fact, tap: tap, key: key)));
```

**To:**

```csharp
from value in guarded(admitted)
select (taps.Value.Iter(tap => Forked(fact: admitted, tap: tap, key: key)), value).Item2;
```

**Effect:** fenced LOC **-1**; private members **-1**. Fire order remains retention, veto fold, guarded body, then taps.

### 5C — Fold `Shielded` into `Forked`

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:142-148`, anchored at `Forked` and `Shielded`.

**From:**

```csharp
private Unit Forked(TFact fact, Func<TFact, IO<Unit>> tap, Op key) =>
    key.Catch(() => IO.lift(() => Shielded(fact: fact, tap: tap, key: key)).Fork(None).Run().Map(static _ => unit))
        .Match(Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Id, cause: cause)));

private Unit Shielded(TFact fact, Func<TFact, IO<Unit>> tap, Op key) =>
    key.Catch(() => tap(fact).Run())
        .Match(Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Id, cause: cause)));
```

**To:**

```csharp
private Unit Forked(TFact fact, Func<TFact, IO<Unit>> tap, Op key) =>
    key.Catch(() => IO.lift(() => key.Catch(() => tap(fact).Run())
            .IfFail(cause => ignore(faults.Park(point: Id, cause: cause))))
        .Fork(None).Run().Map(static _ => unit))
    .IfFail(cause => ignore(faults.Park(point: Id, cause: cause)));
```

**Effect:** fenced LOC **-1**; private members **-1**. The inner `Catch` still isolates tap execution and the outer `Catch` still isolates fork/run failure.

**API proof:** `libs/dotnet/.api/api-languageext.md:120,272,406-410` verifies `Fin.IfFail(Func<Error,A>)`, `IO.lift(Func<A>)`, `IO.Fork`/`Run`, and `Seq.Iter -> Unit`. The rewritten lambda returns `Unit`, so it selects the value-lifting overload and does not manufacture `IO<Fin<Unit>>`.

**Ripples:** none. `Seated`, `Dispatch`, and `Shielded` are private and absent from the page's declared owner/entry vocabulary.

## Move 6 — Share non-landed ring accounting locally

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:253-273`, anchored at `transition.Switch`.

Apply four small replacements. First insert immediately before the dispatch.

**From:**

```csharp
return transition.Switch(
```

**To:**

```csharp
static RingSettlement<T> Missed(Atom<long> lost, RingSettlement<T> settlement) {
    ignore(lost.Swap(static count => Saturating(count: count, delta: 1L)));
    return settlement;
}

return transition.Switch(
```

Then collapse each repeated arm.

**From:**

```csharp
ceded: static (state, ceded) => {
    ignore(state.Lost.Swap(static count => Saturating(count: count, delta: 1L)));
    return new RingSettlement<T>.Ceded(State: ceded.State.Items);
},
```

**To:**

```csharp
ceded: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Ceded(row.State.Items)),
```

**From:**

```csharp
refused: static (state, refused) => {
    ignore(state.Lost.Swap(static count => Saturating(count: count, delta: 1L)));
    return new RingSettlement<T>.Refused(State: refused.State.Items, Cause: refused.Cause);
},
```

**To:**

```csharp
refused: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Refused(row.State.Items, row.Cause)),
```

**From:**

```csharp
contended: static (state, contended) => {
    ignore(state.Lost.Swap(static count => Saturating(count: count, delta: 1L)));
    return new RingSettlement<T>.Ceded(State: contended.State.Items);
});
```

**To:**

```csharp
contended: static (state, row) => Missed(state.Lost, new RingSettlement<T>.Ceded(row.State.Items)));
```

**Effect:** fenced LOC **-5**; module members **0**; one method-local symbol. Every explicit non-landed arm still increments once, and generated exhaustive `Switch` still breaks when `Transition<TState>` gains a case.

**Why not a catch-all:** `if (transition is not Committed)` or a default arm would silently assign future transition cases the `Lost` posture, violating closed-dispatch law.

**API/consumer proof:** `libs/dotnet/.api/api-thinktecture-runtime-extensions.md:153-168` verifies state-threaded exhaustive `Owner.Switch`; the three current non-committed cases in `libs/dotnet/Rasm/.planning/Domain/results.md` each expose the state/cause read by the replacement. No consumer observes the local accounting shape, only `Ring.Lost`.
**Ripples:** none.

## Move 7 — Use the existing atomic take for whole-bus detach

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:343-348`, anchored at `Detach`.

**From:**

```csharp
public Fin<Unit> Detach() {
    Seq<(Option<TOwner> Owner, IDisposable Detach)> snapshot = subscriptions.Value;
    Fin<Unit> released = Unwind(taken: snapshot, key: Op.Of());
    ignore(subscriptions.Swap(held => held.Filter(row => !snapshot.Exists(taken => ReferenceEquals(taken.Detach, row.Detach))).Strict()));
    return released;
}
```

**To:**

```csharp
public Fin<Unit> Detach() =>
    Unwind(taken: Cell.Take(cell: subscriptions).Current, key: Op.Of());
```

**Effect:** fenced LOC **-4**; symbols **0**. `Cell.Take` atomically drains the pre-state and leaves attachments arriving afterward in the cell; `Unwind` still owns reverse, all-attempted release.

**API/consumer proof:** `libs/dotnet/Rasm/.planning/Domain/results.md:740-748` defines `Cell.Take` as take-and-clear and returns the drained value on `Transition.Current`; external consumers call `Detach` and do not observe the subscription carrier.

**Ripples:** none.

## Move 8 — Repair the heterogeneous binding floor, then delete its forwarder

### 8A — Match visibility to the existing public signatures

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:388`, anchored at `IHookBinding<TPoint, TOwner>`.

**From:**

```csharp
internal interface IHookBinding<TPoint, TOwner>
```

**To:**

```csharp
public interface IHookBinding<TPoint, TOwner>
```

**Effect:** fenced LOC **0**; public surface **+1 required type**. This is a correctness repair, not optional expansion: public `Census` and `MountAll(Seq<IHookBinding<...>>)` currently expose a less-accessible type and are invalid C#.

**Consumer proof:** `Rasm.Rhino/.planning/Display/interaction.md:1015-1024` builds the heterogeneous roster consumed by `MountAll`; `Rasm.Rhino/.planning/Display/conduit.md:576-584` also composes that floor. The interface preserves distinct `TAsk`/`TGrant` rows without `object` payloads or runtime `Type` pairs.

### 8B — Inline `HookMounts.Seat`

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:419-433`, anchored at `MountAll` and private `Seat`.

**From:**

```csharp
Seat(binding: binding, key: key).Match(
    Succ: lease => Fin.Succ(taken.Add(lease)),
    Fail: refusal => Fin.Fail<Seq<Lease<IDisposable>>>(refusal)
        .Rollback(held: taken, release: static lease => Fin.Succ(lease.Dispose()), key: key))));

private Fin<Lease<IDisposable>> Seat(IHookBinding<TPoint, TOwner> binding, Op key) =>
    binding.Mount(mounts: this, key: key);
```

**To:**

```csharp
binding.Mount(mounts: this, key: key).Match(
    Succ: lease => Fin.Succ(taken.Add(lease)),
    Fail: refusal => Fin.Fail<Seq<Lease<IDisposable>>>(refusal)
        .Rollback(held: taken, release: static lease => Fin.Succ(lease.Dispose()), key: key))));
```

**Effect:** fenced LOC **-2**; private members **-1**. Interface dispatch already is the heterogeneous mount operation; the private method adds no admission, policy, custody, or projection.

**Ripples:** update `[04]-[HOOK_MOUNT]` to call `IHookBinding` the public heterogeneous floor. Consumer signatures remain unchanged.

## Move 9 — Keep `HookId` typed through the frozen census

**Location:** `libs/dotnet/Rasm/.planning/Domain/hooks.md:453-459`, anchored at `HookRegistry` and `Mount`.

**From:**

```csharp
public sealed record HookRegistry(FrozenDictionary<string, IHookPoint> Points) {
    Seq<string> collided = rows.Collisions(static row => row.Id.ToString());
    return collided.IsEmpty
        ? Fin.Succ(new HookRegistry(Points: rows.ToFrozenDictionary(static row => row.Id.ToString(), static row => row, StringComparer.Ordinal)))
        : Fin.Fail<HookRegistry>(new KernelFault.InvalidValue(Label: string.Join(", ", collided), Requirement: "one point per id across every contributing bus"));
}
```

**To:**

```csharp
public sealed record HookRegistry(FrozenDictionary<HookId, IHookPoint> Points) {
    Seq<HookId> collided = rows.Collisions(static row => row.Id);
    return collided.IsEmpty
        ? Fin.Succ(new HookRegistry(Points: rows.ToFrozenDictionary(static row => row.Id, static row => row)))
        : Fin.Fail<HookRegistry>(new KernelFault.InvalidValue(Label: string.Join(", ", collided.Map(static id => id.ToValue())), Requirement: "one point per id across every contributing bus"));
}
```

**Effect:** fenced LOC **0**; symbols **0**. Collision key, dictionary key, equality, and rendered evidence now originate from the generated owner; no formatting round-trip or call-site comparer can diverge from `HookId`'s declared ordinal equality.

**API/consumer proof:** `libs/dotnet/.api/api-thinktecture-runtime-extensions.md:134-151,217-228` verifies generated value-object equality and `ToValue`; `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` is already on `HookId`. The live composition at `Rasm.AppHost/.planning/Runtime/modules.md:441-444` registers the census whole and does not require string-keyed lookup.

**Ripples:** update `[05]-[HOOK_REGISTRY]` to call it a frozen `HookId` census. No dispatch lookup exists.

## Deliberate non-moves

- Keep `HookId` and `TraceScope` distinct. Four-segment hook-seat identity and three-segment trace-plane identity have different admission invariants and consumers.
- Keep `RingSettlement<T>` distinct from `Transition<TState>`. It adds committed eviction cleanup; `Rasm.AppUi/.planning/Shell/hosts.md:133-139` consumes `Landed.Cleanup` and `Refused.Cause` distinctly.
- Keep `HookDetacher`. It has live kernel-instrument and Grasshopper-event consumers, and neither `.api` tier supplies an action-backed disposable; another hand-written wrapper would relocate the symbol.
- Keep the `Fin.Succ(Retain(...))` sequencing step: `Retain` mutates the buffer, so `docs/stacks/csharp/results-and-effects.md` `[TERMINAL_COLLAPSE]` expressly permits the query shell to sequence that effect; replacing it with tuple evaluation would be less legible without changing semantics.
- Keep `Retain`, `Admitted`, `Forked`, `Attach`, `HookSet.Seat`, `HookSet.Seated`, `Traced`, `Unwind`, and `Saturating`. After Moves 5A-5C, each remaining helper either has multiple callers or owns a named law-bearing operation whose inline form would increase nesting or repeat logic.
- Keep the rollback folds in `HookSet.Of` and `HookMounts.MountAll`. `TraverseM` would lose the successfully attached prefix needed for reverse rollback.
- Do not collapse `CanVeto` into a row-identity comparison. It is the open behavior column both gates read; comparing to `HookModality.Veto` would make the next veto-capable modality require call-site edits.
- Do not “fix” duplicate delegate attachment by moving the same identity problem into `Seq.Remove`. Independent duplicate attachment needs a unique subscription token and would add state and LOC; it is not a surgical reduction and has no proven consumer demand in this pass.

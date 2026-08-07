# [APPHOST_HOOK_RAIL]

`HookRail` is the AppHost point vocabulary over the kernel signal capsule: each runtime extension point is one declared `HookPoint<TFact>` value — package-qualified id, one closed payload type, one modality row — fired by the owning domain code as an evidence event observability projects and never produces. `HookRail` owns naming, modality, and mount custody over delivery the spine already carries, so a second bus, scheduler, or retry owner stays foreclosed by the Runtime/ports#PORT_RECORDS cardinality invariant.

Settled composition: the kernel signal capsule supplies `HookId`, `HookModality`, `HookPoint<TFact>`, `IHookPoint`, `HookRegistry`, `IsolatedFault`, both `Fire` overloads, `Drain`, and the synchronous-fire law.

Each fact type arrives from its minting owner — `PhaseReceipt` beside `Lifecycle` and `PhaseSubscription` at Runtime/lifecycle#PHASE_FAMILY, `CommandIntent` at Agent/runtime#DISPATCH_FRONT_DOOR, `DeliveryReceipt` at Wire/outbound#DELIVERY_FANOUT, `DegradationReading` at Observability/health#DEGRADATION_RAIL, `ReceiptSinkPort` beside `ReceiptEnvelope` at `Rasm/Domain/telemetry#CAUSAL_FRAME` under the seven-port cardinality Runtime/ports#PORT_RECORDS fixes. Hook ids share the instrumentation-scope discipline — package-qualified, registry-unique — so collision and shadowing die at composition.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: AppHost point roster with its landed firing members, composed capsule instance, guarded receipt-sink lift, the replay and isolation reads, the banded refusal family, and the composition mount.

## [02]-[HOOK_RAIL]

- Owner: `HookRail` — the named point roster the spine fires, one shared `IsolatedFault` evidence cell per composition; `HookFault` the rail's own fault family deriving its codes through `FaultBand.Hook`; the kernel `HookRegistry.Mount` freezes the roster into the composition's audit table.
- Cases: `HookRail` rows, each naming its modality and the member that fires it — `Receipt` (every `ReceiptEnvelope` the sink emits, observe, fired by `Tap`'s sink decoration), `Phase` (every `PhaseReceipt` commit, observe, fired by `Lifecycle.Transition` on the settled commit, the capsule holding this point as its one subscribe seam), `Command` (`CommandIntent` pre-dispatch, veto, fired by `Admitted` inside `CommandDispatch.Run` ahead of the command algebra), `Delivery` (per-channel `DeliveryReceipt`, observe, fired by `Settled` on `DeliveryFanout`'s evidence leg), `Degradation` (every committed `DegradationReading` — derived, forced, or cascaded — replay so a late panel reads the recent path, fired by `Degraded` off the cell's own swap return).
- Law: a fire member is the ROW's own, never the producer's to spell — a declared point whose fire site lives in prose is vocabulary, not a plane, and a veto row in particular advertises an admission gate that admits everything, since the guard runs only where an emitter reaches `Fire`. `Tap` decorates a publication seam the spine already carries, so that row fires with the producing fold untouched, and the `Phase` row needs no member at all because the lifecycle capsule holds the point ITSELF and fires it on the settled commit — a decorator over that capsule's `Subscribe` would register the same point twice and double every phase record; `Admitted`, `Settled`, and `Degraded` are the members a producer composes at its own seam, and the rail — never the producer — owns which point, which modality, and which rail shape that call takes.
- Entry: subscription reaches a point through its declared `HookRail` field — the capsule's own `Veto`/`Observe`/`Drain` are the subscriber entries, so a name-resolved lookup surface never exists; `HookRail.Tap(ReceiptSinkPort sink, HookRail rail)` decorates the sink's `Emit` delegate so every envelope crosses the `Receipt` point before egress; the `Phase` row is handed to `Lifecycle` at construction and reaches subscribers through that capsule's own `Subscribe`, so this roster declares it and spells no fire member for it; `HookRail.Admitted`, `HookRail.Settled`, and `HookRail.Degraded` are the veto and observe fires their producers compose; `HookRail.Recent()` is the `Degradation` replay read a late panel drains; `HookRail.TapFaults` snapshots this roster's own parked subscriber faults; `HookRail.Points` is this roster's census and `HookRail.Mount(params ReadOnlySpan<IHookPoint> contributed)` returns `Fin<HookRegistry>`, folding it with every contributing package's own `Points` census into the one frozen registry the composition audits and railing a collision as `HookFault.RosterCollision`.
- Auto: fire order, veto folding, bounded replay, and fork-shielded observe isolation are the capsule's — a throwing or failing OBSERVE tap parks as `IsolatedFault` on the rail's cell and the emitter's result is untouched, while a VETO refusal IS the emitter's verdict by the modality's own `CanVeto` column and reaches the caller on its rail.
- Law: `TapFaults` reads the isolation evidence as a SNAPSHOT under the spelling every sibling rail carries, because `Atom.Swap` publishes the NEW value and a take-and-clear spelled through it hands back the empty it just installed; the cell is per-ROSTER, each contributing package minting its own inside its own `Live()`, so a composition freezes one audit table over N evidence cells and a fold across them waits on a `Faults` accessor `IHookPoint` does not carry.
- Law: the fault cell's BOUND is the capsule's open coordinate — `Park` adds without pruning where its sibling `Retain` prunes to `depth`, so a persistently refusing tap grows the shared cell at receipt rate until that capsule bound lands; `HookFault.TapSaturated` is the case a drain raises against that growth, so the coordinate stays a NAMED refusal a reader can attribute to its point rather than memory pressure with no evidence of cause.
- Law: refusals band through `FaultBand.Hook` like every sibling section's — the registry reserves those codes against this section by owner string, so a rail refusal reaching a caller untyped is the one shape that reservation exists to forbid, and the three cases cover the three refusals the rail can actually produce: a duplicate id at the frozen merge, a second composition-time freeze, and a saturating parked cell.
- Law: `Recent()` exists because `Replay` retention with no drain is retention nothing reads — the `Degradation` row buffers precisely so a panel attaching after a transition reconstructs the recent path, and a modality whose held window has no reader is an `Observe` row wearing a heavier column.
- Packages: LanguageExt.Core, Rasm, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new hook point is one `HookRail` field, one `Live` seat, and one fire member carrying its modality's rail shape — the census follows the record's own deconstruction arity rather than a hand-listed twin; a new rail refusal is one `HookFault` case on the reserved band; a foreign package declares points on its own roster and hands that census to `Mount`, subscribing to these points through the capsule entries — AppHost points stay declared here.
- Boundary: the runtime spine composes effects, so an effectful seam LIFTS the capsule's synchronous fire and a synchronous seam takes the `Fin` whole — `Admitted` hands its caller the veto rail because the caller's own transaction is what a refusal must stop, while `Settled` and `Degraded` ignore theirs, an observe point having no verdict to carry.
- Boundary: the rail carries no queue, no scheduler, and no retry — ordered delivery is the HLC stamp the envelope already carries and durability is the outbox leg, so a tap that must never lose an event is a durable outbox consumer selected by the delivery-honesty axis, never a hook subscriber.
- Boundary: the frozen mount table is the audit surface — a fired id outside it is unreachable by construction because firing requires the declared point value, `Mount` is the composition's one freeze so a contributed census reaching a second `HookRegistry.Mount` forks that table, and a duplicate id across two rosters dies at the frozen merge rather than shadowing a point.
- Boundary: the decoration order is settled against the `Rasm/Domain/telemetry#CAUSAL_FRAME` fence — `Send` swaps the HLC cell, constructs the stamped envelope, THEN invokes `Emit(envelope)`, so a `with { Emit = ... }` decorator always observes the fully stamped value, and stacking decorators at one root preserves the one-mint law because record-`with` copies the same `Atom<(Instant, ulong)>` reference into every decorated instance — the mint cell is shared identity, never a per-decorator clone.

```csharp signature
public sealed record HookRail(
    HookPoint<ReceiptEnvelope> Receipt,
    HookPoint<PhaseReceipt> Phase,
    HookPoint<CommandIntent> Command,
    HookPoint<DeliveryReceipt> Delivery,
    HookPoint<DegradationReading> Degradation,
    Atom<Seq<IsolatedFault>> Faults) {
    public static HookRail Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.apphost.receipt.emitted"), HookModality.Observe, faults),
            new(HookId.Create("rasm.apphost.lifecycle.phase"), HookModality.Observe, faults),
            new(HookId.Create("rasm.apphost.command.admit"), HookModality.Veto, faults),
            new(HookId.Create("rasm.apphost.delivery.settled"), HookModality.Observe, faults),
            new(HookId.Create("rasm.apphost.health.degradation"), HookModality.Replay, faults),
            faults);
    }

    // Census rides the record's own deconstruction, so a sixth point breaks the arity HERE rather than landing a
    // fireable id outside the frozen audit table — a hand-listed twin drifts silently and firing never consults it.
    public Seq<IHookPoint> Points =>
        this switch {
            var (receipt, phase, command, delivery, degradation, _) =>
                Seq<IHookPoint>(receipt, phase, command, delivery, degradation),
        };

    // `Emit` closes over the UNDECORATED port, so stacking taps composes instead of recursing, and the bind hands
    // egress the ADMITTED envelope — whatever the point settled on — routing a refusal onto the emit rail rather
    // than parking it beside an envelope already sent.
    public static ReceiptSinkPort Tap(ReceiptSinkPort sink, HookRail rail) =>
        sink with {
            Emit = envelope => IO.lift(() => rail.Receipt.Fire(envelope)).Bind(sink.Emit),
        };

    // Admission runs on the dispatch's own input, so a transforming gate governs the transaction itself and a
    // refusal reaches the caller's rail instead of parking beside a command already run.
    public Fin<CommandIntent> Admitted(CommandIntent intent) => Command.Fire(intent);

    public Unit Settled(DeliveryReceipt receipt) => ignore(Delivery.Fire(receipt));

    public Unit Degraded(DegradationReading reading) => ignore(Degradation.Fire(reading));

    // This roster's own cell: every contributing package mints a separate one in its own `Live()`, so this read
    // never answers for a peer's parked taps.
    public Seq<IsolatedFault> TapFaults => Faults.Value;

    // Replay retains for exactly this read — a panel attaching after a transition drains the held window.
    public Seq<DegradationReading> Recent() => Degradation.Drain();

    // Every contributing package hands its own `Points` census in; the kernel's frozen merge kills a duplicate id
    // across rosters, so this is the composition's one freeze and no contributor calls `HookRegistry.Mount` itself.
    // The merge REFUSES by throw — a frozen map cannot carry two rows under one key — so the refusal converts at
    // this boundary into the reserved band's own case: a composition-time roster collision is a typed fault the
    // root folds onto its rail beside every other admission refusal, never a bare exception escaping the one
    // surface that knows which rosters were merged.
    public Fin<HookRegistry> Mount(params ReadOnlySpan<IHookPoint> contributed) =>
        Try.lift(() => HookRegistry.Mount([.. Points, .. contributed])).Run()
            .MapFail(error => (Error)new HookFault.RosterCollision(error.Message));
}

// The reserved band's consumer: a hook-rail refusal is banded evidence like every sibling section's, so a
// duplicate id at the frozen merge and a saturated fault cell each reach a caller as a typed code rather than
// an untyped raise the registry's own `mirror: false` owner string promised would land here.
[Union]
public abstract partial record HookFault : Expected, IValidationError<HookFault> {
    private HookFault(string detail, int code) : base(detail, code, None) { }

    public static HookFault Create(string message) => new Text(message);

    public sealed record Text : HookFault { public Text(string detail) : base(detail, FaultBand.Hook.Code(0)) { } }

    // Two rosters claiming one id: the merge is the composition's one freeze, so the collision is fatal there
    // and never a shadowed point that fires under whichever roster a page happened to cite.
    public sealed record RosterCollision : HookFault { public RosterCollision(string detail) : base(detail, FaultBand.Hook.Code(1)) { } }

    // A second `Mount` on one composition: the frozen table is the audit surface, so a second freeze forks it
    // and every id fired afterwards is unauditable against whichever table its caller resolved.
    public sealed record MountForked : HookFault { public MountForked(string detail) : base(detail, FaultBand.Hook.Code(2)) { } }

    // The parked-fault cell past its declared depth: `Park` adds without pruning, so a persistently refusing tap
    // grows the shared cell at receipt rate — this case is what a drain reads to name the saturating point
    // instead of discovering the growth as memory pressure with no evidence of which tap caused it.
    public sealed record TapSaturated : HookFault { public TapSaturated(string point, int depth) : base($"{point}: {depth}", FaultBand.Hook.Code(3)) { } }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

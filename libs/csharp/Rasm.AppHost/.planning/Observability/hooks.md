# [APPHOST_HOOK_RAIL]

`HookRail` is the AppHost point vocabulary over the kernel signal capsule: each runtime extension point is one declared `HookPoint<TFact>` value — package-qualified id, one closed payload type, one modality row — fired by the owning domain code as an evidence event observability projects and never produces. `HookRail` owns naming, modality, and mount custody over delivery the spine already carries, so a second bus, scheduler, or retry owner stays foreclosed by the Runtime/ports#PORT_RECORDS cardinality invariant.

Settled composition: the kernel signal capsule supplies `HookId`, `HookModality`, `HookPoint<TFact>`, `IHookPoint`, `HookRegistry`, `IsolatedFault`, both `Fire` overloads, `Drain`, and the synchronous-fire law.

Each fact type arrives from its minting owner — `PhaseReceipt` beside `Lifecycle` and `PhaseSubscription` at Runtime/lifecycle#PHASE_FAMILY, `CommandIntent` at Agent/runtime#DISPATCH_FRONT_DOOR, `DeliveryReceipt` at Wire/outbound#DELIVERY_FANOUT, `DegradationReading` at Observability/health#DEGRADATION_RAIL, `ReceiptSinkPort` beside `ReceiptEnvelope` at `Rasm/Domain/telemetry#CAUSAL_FRAME` under the seven-port cardinality Runtime/ports#PORT_RECORDS fixes. Hook ids share the instrumentation-scope discipline — package-qualified, registry-unique — so collision and shadowing die at composition.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: AppHost point roster with its landed firing members, composed capsule instance, guarded receipt-sink lift, the replay and isolation reads, and the composition mount.

## [02]-[HOOK_RAIL]

- Owner: `HookRail` — the named point roster the spine fires, one shared `IsolatedFault` evidence cell per composition; the kernel `HookRegistry.Mount` freezes the roster into the composition's audit table.
- Cases: `HookRail` rows, each naming its modality and the member that fires it — `Receipt` (every `ReceiptEnvelope` the sink emits, observe, fired by `Tap`'s sink decoration), `Phase` (every `PhaseReceipt` commit, observe, fired by `Watch` over the lifecycle capsule's own subscription), `Command` (`CommandIntent` pre-dispatch, veto, fired by `Admitted` inside `CommandDispatch.Run` ahead of the command algebra), `Delivery` (per-channel `DeliveryReceipt`, observe, fired by `Settled` on `DeliveryFanout`'s evidence leg), `Degradation` (every committed `DegradationReading` — derived, forced, or cascaded — replay so a late panel reads the recent path, fired by `Degraded` off the cell's own swap return).
- Law: a fire member is the ROW's own, never the producer's to spell — a declared point whose fire site lives in prose is vocabulary, not a plane, and a veto row in particular advertises an admission gate that admits everything, since the guard runs only where an emitter reaches `Fire`. `Tap` and `Watch` decorate publication seams the spine already carries, so two rows fire with the producing fold untouched; `Admitted`, `Settled`, and `Degraded` are the members a producer composes at its own seam, and the rail — never the producer — owns which point, which modality, and which rail shape that call takes.
- Entry: subscription reaches a point through its declared `HookRail` field — the capsule's own `Veto`/`Observe`/`Drain` are the subscriber entries, so a name-resolved lookup surface never exists; `HookRail.Tap(ReceiptSinkPort sink, HookRail rail)` decorates the sink's `Emit` delegate so every envelope crosses the `Receipt` point before egress; `HookRail.Watch(Lifecycle lifecycle, HookRail rail)` registers the `Phase` fire on the capsule's `Subscribe` seam; `HookRail.Admitted`, `HookRail.Settled`, and `HookRail.Degraded` are the veto and observe fires their producers compose; `HookRail.Recent()` is the `Degradation` replay read a late panel drains; `HookRail.TapFaults` snapshots this roster's own parked subscriber faults; `HookRail.Points` is this roster's census and `HookRail.Mount(params ReadOnlySpan<IHookPoint> contributed)` folds it with every contributing package's own `Points` census into the one frozen `HookRegistry` the composition audits.
- Auto: fire order, veto folding, bounded replay, and fork-shielded observe isolation are the capsule's — a throwing or failing OBSERVE tap parks as `IsolatedFault` on the rail's cell and the emitter's result is untouched, while a VETO refusal IS the emitter's verdict by the modality's own `CanVeto` column and reaches the caller on its rail.
- Law: `TapFaults` reads the isolation evidence as a SNAPSHOT under the spelling every sibling rail carries, because `Atom.Swap` publishes the NEW value and a take-and-clear spelled through it hands back the empty it just installed; the cell is per-ROSTER, each contributing package minting its own inside its own `Live()`, so a composition freezes one audit table over N evidence cells and a fold across them waits on a `Faults` accessor `IHookPoint` does not carry.
- Law: the fault cell's BOUND is the capsule's open coordinate — `Park` adds without pruning where its sibling `Retain` prunes to `depth`, so a persistently refusing tap grows the shared cell at receipt rate until that capsule bound lands.
- Law: `Recent()` exists because `Replay` retention with no drain is retention nothing reads — the `Degradation` row buffers precisely so a panel attaching after a transition reconstructs the recent path, and a modality whose held window has no reader is an `Observe` row wearing a heavier column.
- Packages: LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new hook point is one `HookRail` field, one `Live` seat, and one fire member carrying its modality's rail shape — the census follows the record's own deconstruction arity rather than a hand-listed twin; a foreign package declares points on its own roster and hands that census to `Mount`, subscribing to these points through the capsule entries — AppHost points stay declared here.
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

    // `Lifecycle` publishes every CAS commit on its own subscription, so the Phase row fires by decorating
    // that seam and the transition fold carries no hook call of its own.
    public static PhaseSubscription Watch(Lifecycle lifecycle, HookRail rail) =>
        lifecycle.Subscribe(receipt => ignore(rail.Phase.Fire(receipt)));

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
    public HookRegistry Mount(params ReadOnlySpan<IHookPoint> contributed) =>
        HookRegistry.Mount([.. Points, .. contributed]);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

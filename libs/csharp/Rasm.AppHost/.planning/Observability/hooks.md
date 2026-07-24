# [APPHOST_HOOK_RAIL]

`HookRail` is the AppHost point vocabulary over the kernel signal capsule: each runtime extension point is one declared `HookPoint<TFact>` value — package-qualified id, one closed payload type, one modality row — fired by the owning domain code as an evidence event observability projects and never produces. This rail composes surfaces the spine already owns — the Topics event bus carries asynchronous fan-out, the lifecycle phase subscriptions carry phase taps, and the receipt sink port carries the envelope tap — so the roster is a naming and isolation law over existing delivery, and a second bus, scheduler, or retry owner stays foreclosed by the Runtime/ports#PORT_RECORDS cardinality invariant.

Settled composition: `HookId`, `HookModality`, `HookPoint<TFact>`, `HookRegistry`, `IsolatedFault`, and the synchronous-fire law arrive from the kernel signal capsule; `EventBus.Dispatch`, `Topic`, and `DomainEvent` from Wire/topics#BUS_CONDUCTOR; `PhaseReceipt` and `FaultBand` from Runtime/lifecycle; `ReceiptSinkPort` and `ReceiptEnvelope` from Runtime/ports#PORT_RECORDS. Hook ids share the instrumentation-scope discipline — package-qualified, registry-enforced unique — so collision and shadowing die structurally at composition.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: the AppHost point roster, the composed capsule instance, the receipt-sink lift seam, and the composition mount.

## [02]-[HOOK_RAIL]

- Owner: `HookRail` — the named point roster the spine fires, one shared `IsolatedFault` evidence cell per composition; the kernel `HookRegistry.Mount` freezes the roster into the composition's audit table.
- Cases: `HookRail` rows — `Receipt` (every `ReceiptEnvelope` the sink emits, observe), `Phase` (every `PhaseReceipt` commit, observe), `Command` (command intent pre-dispatch, veto), `Delivery` (per-channel delivery outcome, observe), `Degradation` (level transitions, replay so a late panel reads the recent path).
- Entry: subscription reaches a point through its declared `HookRail` field — the capsule's own `Veto`/`Observe`/`Drain` are the subscriber entries, so a name-resolved lookup surface never exists; `HookRail.Tap(ReceiptSinkPort sink, HookRail rail)` decorates the sink's `Emit` delegate so every envelope fires the `Receipt` point before egress — the one seam where the receipt fan and the hook rail meet, composed at the root, so no emitting page changes.
- Auto: fire order, veto folding, bounded replay, and fork-shielded observe isolation are the capsule's — a throwing or failing tap parks as `IsolatedFault` on the rail's cell and the emitter's result is untouched; the `Faults` cell is the one subscriber-failure evidence surface, projected by the Observability/instruments#RECEIPT_PROJECTION fan and drained by health panels.
- Packages: LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new hook point is one `HookRail` field, one `Live` row, and one `Mount` entry; a foreign package contributes subscribers only — points are declared by the owning AppHost page.
- Boundary: the runtime spine composes effects, so its emission seams LIFT the capsule's synchronous fire — `Emit = envelope => IO.lift(() => rail.Receipt.Fire(envelope)).Bind(_ => sink.Emit(envelope))` is the one-line lift at the decoration seam; the rail carries no queue, no scheduler, and no retry — ordered delivery is the HLC stamp the envelope already carries, and durability is the outbox leg; a tap that must never lose an event is a durable outbox consumer, selected by the delivery-honesty axis, not a hook subscriber; the frozen mount table is the audit surface — a fired id outside it is unreachable by construction because firing requires the declared point value; the decoration order is settled against the Runtime/ports#PORT_RECORDS fence — `Send` swaps the HLC cell, constructs the stamped envelope, THEN invokes `Emit(envelope)`, so a `with { Emit = ... }` decorator always observes the fully stamped value, and stacking decorators at one root preserves the one-mint law because record-`with` copies the same `Atom<(Instant, ulong)>` reference into every decorated instance — the mint cell is shared identity, never a per-decorator clone.

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

    // One effect-rail lift: the spine's IO seam lifts the capsule's synchronous Fire — the capsule
    // never threads the caller's rail.
    public static ReceiptSinkPort Tap(ReceiptSinkPort sink, HookRail rail) =>
        sink with {
            Emit = envelope => IO.lift(() => rail.Receipt.Fire(envelope)).Bind(_ => sink.Emit(envelope)),
        };

    public HookRegistry Mount() => HookRegistry.Mount(Receipt, Phase, Command, Delivery, Degradation);
}
```

## [03]-[RESEARCH]

(none)

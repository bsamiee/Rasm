# [APPHOST_HOOK_RAIL]

`Rasm.AppHost` declares its extension-point roster and the closed fact family that crosses it, and nothing else. Eight points name the spine seams a rider may govern or watch — the receipt stream, the lifecycle commit, command admission, per-channel delivery, the degradation ladder, the benchmark profile, cluster coordination, and the companion transitions — each carrying its delivery capability and its trace plane as ROW DATA, so the guard a point advertises is the guard the mechanism runs. Fire order, veto folding, observe isolation, replay retention, bounded fault custody, scoped release, and the frozen census belong to `Rasm/Domain/hooks`, so this page holds no rail members, no census, and no fault family of its own.

Settled composition: `HookId`, `TraceScope`, `HookModality`, `IHookRoster<TSelf>`, `HookPoint<TFact>`, and `IsolatedFault` arrive from Rasm/Domain/hooks#HOOK_POINT; `HookRail<TPoint,TFact,TOwner>`, `IHookFact<TPoint>`, `HookGate`, `HookTap`, `Ring<T>`, and `FaultCell` from Rasm/Domain/hooks#HOOK_RAIL; `HookMounts<TPoint,TOwner>` from Rasm/Domain/hooks#HOOK_MOUNT; `HookRegistry` from Rasm/Domain/hooks#HOOK_REGISTRY; `CapabilitySet<TCapability>` from Rasm/Domain/validation#CAPABILITY; `TelemetrySource`, `ReceiptEnvelope`, and `ReceiptSinkPort` from Rasm/Domain/frame#SOURCE and #RECEIPT_PORT. Each fact payload arrives from its minting owner — `PhaseReceipt` at Runtime/lifecycle#PHASE_FAMILY, `CommandIntent` at Agent/runtime#DISPATCH_FRONT_DOOR, `DeliveryReceipt` at Wire/outbound#DELIVERY_FANOUT, `DegradationReading` at Observability/health#DEGRADATION_RAIL, `CoordinationSignal` at Wire/coordination#ROLE_ELECTION, `CompanionSignal` at Wire/companion#PROCESS_MODALITY. The rail's owner key is `TelemetrySource`, so a contributing package releases exactly its own subscriptions; the composition mints the one `FaultCell` and the one `SpanBand` and hands both to `HookRail.Of` at Runtime/modules#MODULE_LEDGER.

## [01]-[INDEX]

- [02]-[HOOK_ROSTER]: the `AppHostPoint` roster with its modality and plane columns, the closed `AppHostFact` union carrying each point's seat, the paired receipt-and-rail egress producing pages compose, and the receipt-sink decoration that fires the receipt row.

## [02]-[HOOK_ROSTER]

- Owner: `AppHostPoint` — the `[SmartEnum<string>]` roster the kernel rail takes as its `TPoint` argument, each row carrying the modality set both admission gates read and the optional trace plane the rail brackets under; `AppHostFact` — the closed union every fire carries, realizing the kernel `IHookFact<AppHostPoint>` floor with each case projecting its own seat through `At`; `FactSink<TSignal>` — the paired receipt-and-rail egress a producing page composes for one transition family; `AppHostHooks` — the receipt-sink decoration that is the `Receipt` row's fire site.
- Cases: `Receipt` observes every `ReceiptEnvelope` the sink emits; `Phase` observes every settled `PhaseReceipt`; `Command` vetoes a `CommandIntent` ahead of the command algebra; `Delivery` observes a per-channel `DeliveryReceipt`; `Degradation` replays committed `DegradationReading` values so a panel attaching after a transition reconstructs the recent path; `ProfileSample` observes a benchmark profile sample; `Coordination` and `Companion` each observe a whole transition FAMILY through that page's own closed signal union, because a roster row per receipt type would spread one page's discrimination across the seat vocabulary.
- Entry: `AppHostPoint.Items` is the seat roster `HookRail.Of` mints from and the census `HookRegistry.Mount` freezes; `AppHostFact.At` is the seat every fire resolves through, so `rail.Fire(at: fact.At, fact: fact, key: key)` is the whole emission spelling and the guarded arity `rail.Fire(at: fact.At, fact: fact, key: key, body: …)` hands a veto seam its admitted fact; `AppHostHooks.Tap(sink, rail, key)` decorates `ReceiptSinkPort.Emit` so every stamped envelope crosses the `Receipt` row before egress; `FactSink<TSignal>.Fan` is the paired egress a producing page composes once — one receipt to the lake and one fire at the seat its own fact projects.
- Law: the seat rides the FACT, never the call site — a `(point, fact)` pair spelled independently lets an emitter fire a delivery receipt at the command veto gate. `At` is that seat and the union realizes the kernel `IHookFact<AppHostPoint>` floor over it, so `Seats` is the one-line `at == At` and the rail REFUSES a foreign pair at entry rather than firing what it was handed. `At` projects a roster row rather than storing one, so no case carries a copy that `with` can diverge and no emitter renders an id.
- Law: `Command` holds `Veto` ALONE and every other row holds a non-vetoing modality, so the kernel's two gates settle each subscription by the modality column: a tap on `Command` refuses, a gate on any other row refuses, and neither gate names a row by identity.
- Law: `Degradation` holds `Replay` alone and that row both retains and observes — `HookModality.Replay` is non-vetoing, so the observe gate admits it and the retention depth is the modality's own column. A separate `Observe` row beside it would declare a second answer to a question the capability set already holds.
- Law: the plane column is per ROW because a span per fire is priced per fire — `Command` and `Phase` are decision seams a bracket earns, while `Receipt` and `Delivery` run at envelope rate against a fan that already meters them and `Degradation` fires only on a committed level change. A row without a plane falls through the rail's bracket untraced whatever band the composition supplied.
- Law: ids derive from the row key through one accessor-backed projection, so the roster key is the only authority and a rename moves the id with it; an eager index folds `Items` before the generator has filled it (branch RULINGS `[02]`).
- Receipt: the rail mints none — a fire IS the evidence event and the emitter's own typed receipt carries the payload. Subscriber faults park on the composition's `FaultCell`, whose `Parked`, `Shed`, and `Lost` columns Observability/bundles#ARTIFACT_ROSTER drains into the support bundle.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new point is one `AppHostPoint` row plus one `AppHostFact` case with its `At` override — the union's generated `Switch` breaks every subscriber arm at compile time, the census follows `Items`, and the seating gate follows `At`, so neither takes a second edit. A foreign package declares its own roster and its own fact union and hands its census to the composition's one `HookRegistry.Mount`.
- Boundary: NAMED LOSS — the per-point fact TYPE. Today a subscriber to a `HookPoint<CommandIntent>` field cannot receive a `DeliveryReceipt`; under one rail every point shares `AppHostFact` and subscribers discriminate on the case. What replaces the field's type is stronger at the emitter and weaker at the subscriber: `At` makes a mismatched `(point, fact)` pair unspellable, while a subscriber's arm now runs a total `Switch` the compiler completes. A veto returning a case the seam did not fire was the one residual hazard, and the kernel's own seating gate on the veto fold's PRODUCT now refuses it before the guarded body or any tap runs.
- Boundary: the rail carries no queue, no scheduler, and no retry — ordered delivery is the HLC stamp the message envelope already carries and durability is the outbox leg, so a tap that must never lose an event is a durable outbox consumer selected by the delivery-honesty axis, never a hook subscriber.
- Boundary: decoration order is settled against Rasm/Domain/frame#RECEIPT_PORT — `Send` swaps the HLC cell, constructs the stamped envelope, THEN invokes `Emit`, so a `with { Emit = … }` decorator always observes the fully stamped value, and stacking decorators at one root preserves the one-mint law because record-`with` copies the same `Atom<(Instant, ulong)>` reference into every decorated instance.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Text.Json;
using System.Threading;
using Thinktecture;

namespace Rasm.AppHost.Observability;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AppHostPoint : IHookRoster<AppHostPoint> {
    private const string Head = "rasm.apphost.";

    public static readonly TraceScope HookPlane = TraceScope.Create(value: "rasm.apphost.hooks");

    public static readonly AppHostPoint Receipt = new(
        "receipt.emitted", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Phase = new(
        "lifecycle.phase", CapabilitySet<HookModality>.Of(HookModality.Observe), Some(HookPlane));
    public static readonly AppHostPoint Command = new(
        "command.admit", CapabilitySet<HookModality>.Of(HookModality.Veto), Some(HookPlane));
    public static readonly AppHostPoint Delivery = new(
        "delivery.settled", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Degradation = new(
        "health.degradation", CapabilitySet<HookModality>.Of(HookModality.Replay), None);
    public static readonly AppHostPoint ProfileSample = new(
        "benchmark.profile", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Coordination = new(
        "cluster.coordination", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Companion = new(
        "companion.transition", CapabilitySet<HookModality>.Of(HookModality.Observe), None);

    public CapabilitySet<HookModality> Modalities { get; }
    public Option<TraceScope> Plane { get; }

    public HookId Id => Ids.Value[this];

    private static readonly Lazy<FrozenDictionary<AppHostPoint, HookId>> Ids = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => HookId.Create(value: Head + row.Key)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record AppHostFact : IHookFact<AppHostPoint> {
    private AppHostFact() { }

    public abstract AppHostPoint At { get; }

    public bool Seats(AppHostPoint at) => at == At;

    public sealed record Receipt(ReceiptEnvelope Envelope) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Receipt;
    }

    public sealed record Phase(PhaseReceipt Commit) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Phase;
    }

    public sealed record Command(CommandIntent Intent) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Command;
    }

    public sealed record Delivery(DeliveryReceipt Settled) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Delivery;
    }

    public sealed record Degradation(DegradationReading Reading) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Degradation;
    }

    public sealed record Profile(ProfileSample Sample) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.ProfileSample;
    }

    public sealed record Coordination(CoordinationSignal Settled) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Coordination;
    }

    public sealed record Companion(CompanionSignal Settled) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Companion;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record FactSink<TSignal>(
    ReceiptSinkPort Sink,
    HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail,
    Func<TSignal, AppHostFact> Fact,
    Op Key) where TSignal : notnull {
    public IO<T> Fan<T>(CorrelationId correlation, string kind, T receipt, TSignal signal) where T : notnull =>
        IO.lift(() => Fact(signal))
            .Bind(fact => Sink.Send(correlation, TenantContext.Current, TelemetrySource.AppHost, kind,
                    JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))
                .Bind(_ => IO.lift(() => Rail.Fire(at: fact.At, fact: fact, key: Key))))
            .Map(_ => receipt);
}

public static class AppHostHooks {
    public static ReceiptSinkPort Tap(
        ReceiptSinkPort sink, HookRail<AppHostPoint, AppHostFact, TelemetrySource> rail, Op key) =>
        sink with {
            Emit = envelope => IO.lift(() => rail.Fire(
                    at: AppHostPoint.Receipt, fact: new AppHostFact.Receipt(Envelope: envelope), key: key))
                .Bind(_ => sink.Emit(envelope)),
        };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

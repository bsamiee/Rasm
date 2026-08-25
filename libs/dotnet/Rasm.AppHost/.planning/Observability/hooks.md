# [APPHOST_HOOK_RAIL]

`Rasm.AppHost` declares its extension-point roster and the closed fact family that crosses it, and nothing else. Eight points name the spine seams a rider may govern or watch — the lifecycle commit, command admission, settled command, per-channel delivery, degradation ladder, health alert, binding status, and benchmark profile — each carrying its delivery capability and trace plane as ROW DATA, so the guard a point advertises is the guard the mechanism runs. Fire order, veto folding, observe isolation, replay retention, bounded fault custody, scoped release, and the frozen census belong to `Rasm/Domain/hooks`, so this page holds no rail members, no census, and no fault family of its own.

Settled composition: `HookId`, `TraceScope`, `HookModality`, `IHookRoster<TSelf>`, `HookPoint<TFact>`, and `IsolatedFault` arrive from Rasm/Domain/hooks#HOOK_POINT; `HookRail<TPoint,TFact,TOwner>`, `IHookFact<TPoint>`, `HookGate`, `HookTap`, `Ring<T>`, and `FaultCell` from Rasm/Domain/hooks#HOOK_RAIL; `HookMounts<TPoint,TOwner>` from Rasm/Domain/hooks#HOOK_MOUNT; `HookRegistry` from Rasm/Domain/hooks#HOOK_REGISTRY; `CapabilitySet<TCapability>` from Rasm/Domain/validation#CAPABILITY; `TelemetrySource` from Rasm/Domain/frame#SOURCE and `HlcStamp` from Rasm/Domain/frame#STAMP; `EventSource` from Rasm/Domain/event#EVENT_GRAMMAR; `DomainEvent` and `TopicFabric.Publish` from Wire/topics#TOPIC_FABRIC. Each fact payload arrives from its minting owner — `PhaseCommit` at Runtime/lifecycle#PHASE_FAMILY, `CommandIntent` and `CommandResult` at Agent/runtime#DISPATCH_FRONT_DOOR, `DeliverySettled` at Wire/outbound#DELIVERY_FANOUT, `DegradationReading` at Observability/health#DEGRADATION_RAIL, `Alert` at Observability/health#ALERT_ENGINE, generated `Host.BindingStatus` at Wire/livewire#TS_PROJECTION, and `ProfileSample` at Observability/benchmarks#PROFILE_CORRELATION. The rail's owner key is `TelemetrySource`, so a contributing package releases exactly its own subscriptions; the composition mints the one `FaultCell` and the one `SpanBand`, hands both to `HookRail.Of`, and seats the one observe tap that publishes every durable fact onto its topic row at Runtime/modules#MODULE_LEDGER.

## [01]-[INDEX]

- [02]-[HOOK_ROSTER]: the `AppHostPoint` roster with its modality and plane columns, the closed `AppHostFact` union carrying each point's seat, and the topic projection every case answers.

## [02]-[HOOK_ROSTER]

- Owner: `AppHostPoint` — the `[SmartEnum<string>]` roster the kernel rail takes as its `TPoint` argument, each row carrying the modality set both admission gates read and the optional trace plane the rail brackets under; `AppHostFact` — the closed union every fire carries, realizing the kernel `IHookFact<AppHostPoint>` floor with each case projecting its own seat through `At` and its own bus carrier through `Event`.
- Cases: `Phase` observes every settled `PhaseCommit`; `Command` vetoes a `CommandIntent` ahead of the command algebra and `Outcome` observes the `CommandResult` the algebra committed; `Delivery` observes a per-channel `DeliverySettled`; `Degradation` replays committed `DegradationReading` values so a panel attaching after a transition reconstructs the recent path; `Alert` observes each `Alert` transition the engine fires; `Binding` observes generated status from a committed binding transition; `ProfileSample` observes a benchmark profile sample.
- Entry: `AppHostPoint.Items` is the seat roster `HookRail.Of` mints from and the census `HookRegistry.Mount` freezes; `AppHostFact.At` is the seat every fire resolves through, so `rail.Fire(at: fact.At, fact: fact, key: key)` is the whole emission spelling and the guarded arity `rail.Fire(at: fact.At, fact: fact, key: key, body: …)` hands a veto seam its admitted fact; `AppHostFact.Event(EventSource source, HlcStamp stamp)` returns `Fin<Option<DomainEvent>>` — the case's own projection onto its `Topic` row, `None` for a fact no topic carries — so the composition's one observe tap publishes through `TopicFabric.Publish` with no case switch of its own.
- Law: the seat rides the FACT, never the call site — a `(point, fact)` pair spelled independently lets an emitter fire a delivery settlement at the command veto gate. `At` is that seat and the union realizes the kernel `IHookFact<AppHostPoint>` floor over it, so `Seats` is the one-line `at == At` and the rail REFUSES a foreign pair at entry rather than firing what it was handed. `At` projects a roster row rather than storing one, so no case carries a copy that `with` can diverge and no emitter renders an id.
- Law: `Command` holds `Veto` ALONE and every other row holds a non-vetoing modality, so the kernel's two gates settle each subscription by the modality column: a tap on `Command` refuses, a gate on any other row refuses, and neither gate names a row by identity. The settled command is its own `Outcome` row rather than a second modality on `Command`, because a veto gate reads an intent it may transform while an observer of the commit reads a value nothing may transform.
- Law: `Degradation` holds `Replay` alone and that row both retains and observes — `HookModality.Replay` is non-vetoing, so the observe gate admits it and the retention depth is the modality's own column. A separate `Observe` row beside it would declare a second answer to a question the capability set already holds.
- Law: the plane column is per ROW because a span per fire is priced per fire — `Command`, `Outcome`, and `Phase` are decision seams a bracket earns, while `Delivery` runs at fan rate under a hop span the outbound seam already opened, `Alert` under the sweep, and `Degradation` fires only on a committed level change. A row without a plane falls through the rail's bracket untraced whatever band the composition supplied.
- Law: ids derive from the row key through one accessor-backed projection, so the roster key is the only authority and a rename moves the id with it; an eager index folds `Items` before the generator has filled it (branch RULINGS `[02]`).
- Law: `Event` is ABSTRACT so every case states whether a topic carries it — a durable case answers its value's own `Event(source, stamp)` and an in-process case answers `None` — and the composition's tap is the ONE emitter onto the bus, never a `TopicFabric.Publish` inside a producing fold (`libs/.planning/ARCHITECTURE.md` `[14]` `[HOOK_ORDER]`). Subscriber faults park on the composition's `FaultCell`, whose `Parked`, `Shed`, and `Lost` columns Observability/bundles#ARTIFACT_ROSTER drains into the support bundle.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new point is one `AppHostPoint` row plus one `AppHostFact` case with its `At` and `Event` overrides — the union's generated `Switch` breaks every subscriber arm at compile time, the census follows `Items`, and the seating gate follows `At`, so neither takes a second edit. A foreign package declares its own roster and its own fact union and hands its census to the composition's one `HookRegistry.Mount`.
- Boundary: NAMED LOSS — the per-point fact TYPE. Today a subscriber to a `HookPoint<CommandIntent>` field cannot receive a `DeliverySettled`; under one rail every point shares `AppHostFact` and subscribers discriminate on the case. What replaces the field's type is stronger at the emitter and weaker at the subscriber: `At` makes a mismatched `(point, fact)` pair unspellable, while a subscriber's arm now runs a total `Switch` the compiler completes. A veto returning a case the seam did not fire was the one residual hazard, and the kernel's own seating gate on the veto fold's PRODUCT now refuses it before the guarded body or any tap runs.
- Boundary: the rail carries no queue, no scheduler, and no retry — ordered delivery is the HLC stamp every `DomainEvent` carries and durability is the outbox leg, so a tap that must never lose an event is a durable outbox consumer selected by the delivery-honesty axis, never a hook subscriber.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
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

    public static readonly AppHostPoint Phase = new(
        "lifecycle.phase", CapabilitySet<HookModality>.Of(HookModality.Observe), Some(HookPlane));
    public static readonly AppHostPoint Command = new(
        "command.admit", CapabilitySet<HookModality>.Of(HookModality.Veto), Some(HookPlane));
    public static readonly AppHostPoint Outcome = new(
        "command.settled", CapabilitySet<HookModality>.Of(HookModality.Observe), Some(HookPlane));
    public static readonly AppHostPoint Delivery = new(
        "delivery.settled", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Degradation = new(
        "health.degradation", CapabilitySet<HookModality>.Of(HookModality.Replay), None);
    public static readonly AppHostPoint Alert = new(
        "health.alert", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint Binding = new(
        "binding.status", CapabilitySet<HookModality>.Of(HookModality.Observe), None);
    public static readonly AppHostPoint ProfileSample = new(
        "benchmark.profile", CapabilitySet<HookModality>.Of(HookModality.Observe), None);

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

    public abstract Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp);

    public bool Seats(AppHostPoint at) => at == At;

    public sealed record Phase(PhaseCommit Commit) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Phase;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Commit.Event(source, stamp).Map(Some);
    }

    public sealed record Command(CommandIntent Intent) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Command;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Fin.Succ(Option<DomainEvent>.None);
    }

    public sealed record Outcome(CommandResult Settled) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Outcome;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Settled.Event(source, stamp).Map(Some);
    }

    public sealed record Delivery(DeliverySettled Settled) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Delivery;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Settled.Event(source, stamp).Map(Some);
    }

    public sealed record Degradation(DegradationReading Reading) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Degradation;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Fin.Succ(Option<DomainEvent>.None);
    }

    public sealed record Alert(Observability.Alert Fired) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Alert;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Fired.Event(source, stamp).Map(Some);
    }

    public sealed record Binding(Rasm.Contracts.Binding.BindingStatus Status) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.Binding;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => LiveWireContract.Event(Status, source, stamp).Map(Some);
    }

    public sealed record Profile(ProfileSample Sample) : AppHostFact {
        public override AppHostPoint At => AppHostPoint.ProfileSample;
        public override Fin<Option<DomainEvent>> Event(EventSource source, HlcStamp stamp) => Fin.Succ(Option<DomainEvent>.None);
    }

}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

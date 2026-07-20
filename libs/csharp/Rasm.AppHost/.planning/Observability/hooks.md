# [APPHOST_HOOK_RAIL]

One typed hook registry formalizes every extension point the runtime spine already fires: a hook is a package-qualified id bound to one closed payload type and one modality row, registered once at composition with structural uniqueness, and fired by the owning domain code as an evidence event observability projects and never produces. This rail composes surfaces the spine already owns — the Topics event bus carries asynchronous fan-out, the lifecycle phase subscriptions carry phase taps, and the receipt sink port carries the envelope tap — so the registry is a naming and isolation law over existing delivery, and a second bus, scheduler, or retry owner stays foreclosed by the Runtime/ports#PORT_RECORDS cardinality invariant.

Settled composition: `EventBus.Dispatch`, `Topic`, and `DomainEvent` from Wire/topics#BUS_CONDUCTOR; `PhaseReceipt` and `FaultBand` from Runtime/lifecycle; `ReceiptSinkPort` and `ReceiptEnvelope` from Runtime/ports#PORT_RECORDS; the fault-band row `FaultBand.Hook` derives every hook fault code. Hook ids share the instrumentation-scope discipline — package-qualified, registry-enforced unique — so collision and shadowing die structurally at composition.

## [01]-[INDEX]

- [01]-[HOOK_IDENTITY]: Grammar-admitted `HookId` and the three modality rows.
- [02]-[HOOK_POINTS]: Typed point capsule — veto fold, observe fan, bounded replay.
- [03]-[HOOK_REGISTRY]: Composition-time mount with structural uniqueness and the receipt tap.
- [04]-[FAULT_ISOLATION]: Subscriber failure to typed fault; telemetry-as-tap seams.

## [02]-[HOOK_IDENTITY]

- Owner: `HookId` `[ValueObject<string>]` — the package-qualified hook name; `HookModality` `[SmartEnum<string>]` — the three delivery rows.
- Cases: modality rows `Veto` (synchronous transform-or-reject in the emitter's rail), `Observe` (asynchronous tap), `Replay` (asynchronous tap with a bounded buffer late subscribers drain).
- Entry: `HookId.Validate` admits the `rasm.<pkg>.<domain>.<point>` grammar — four dot-separated lowercase segments, `rasm` first — so a malformed id is a typed refusal at declaration, never a silent registry miss.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new delivery semantics is one `HookModality` row breaking every modality dispatch at compile time.
- Boundary: the id grammar shares the dotted, lowercase, package-first discipline of instrumentation scopes and metric names — hook ids fix four segments while metric names run deeper, so the disciplines are siblings, one spelling habit serving scopes, metrics, and hooks; `Synchronous` is the modality's own column, so an emitter reads its blocking obligation off the row it declared, never a subscriber-supplied flag.

```csharp signature
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<HookFault>]
public readonly partial struct HookId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["rasm", var pkg, var domain, var point]
            && pkg.Length > 0 && domain.Length > 0 && point.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-')
            ? null
            : new HookFault.MalformedId(value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookModality {
    public static readonly HookModality Veto = new("veto", synchronous: true);
    public static readonly HookModality Observe = new("observe", synchronous: false);
    public static readonly HookModality Replay = new("replay", synchronous: false);

    public bool Synchronous { get; }
}
```

## [03]-[HOOK_POINTS]

- Owner: `HookPoint<TFact>` — the typed point capsule; `IHookPoint` — the member-bearing floor the registry mounts over heterogeneous payload types.
- Entry: `Fire(TFact fact)` — the one emitter entry: the veto fold runs first and its refusal returns on the emitter's own rail, the observe fan posts to every tap with per-tap isolation, and a replay point folds the fact into its bounded buffer; `Veto`, `Observe`, and `Drain` are the subscriber entries, each returning the LIFO detacher the port law already mandates.
- Auto: veto subscribers fold left in registration order through `Bind`, so the first refusal short-circuits with its typed fault and a transform threads forward; observe taps run through `Try`-captured `IO` so a throwing tap converts at the seam; the replay buffer prunes to `Depth` on every fire, oldest-first, so a late subscriber drains a bounded recent window, never an unbounded history.
- Receipt: a hook fire is the evidence event itself — the emitter's own receipt already carries the fact, so the point mints nothing.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new hook is one `HookPoint<TFact>` declaration and one registry row; a new subscriber is one `Observe`/`Veto` call at composition.
- Boundary: the payload type closes at declaration — `TFact` is a typed record from the owning page, so a stringly payload cannot enter the rail; `Fire` on an observe point never awaits its taps: delivery rides the same bounded fan the Topics subscriptions use, and a slow tap pressures its own buffer, never the emitter; only a `Veto`-modality point accepts veto subscribers — the modality row gates the subscriber entry with a typed refusal.

```csharp signature
public interface IHookPoint {
    HookId Id { get; }
    HookModality Modality { get; }
    Type Fact { get; }
}

public sealed class HookPoint<TFact>(HookId id, HookModality modality, int depth = 64) : IHookPoint {
    readonly Atom<Seq<Func<TFact, Fin<TFact>>>> vetoes = Atom(Seq<Func<TFact, Fin<TFact>>>());
    readonly Atom<Seq<Func<TFact, IO<Unit>>>> taps = Atom(Seq<Func<TFact, IO<Unit>>>());
    readonly Atom<Seq<TFact>> buffer = Atom(Seq<TFact>());

    public HookId Id => id;
    public HookModality Modality => modality;
    public Type Fact => typeof(TFact);

    public Fin<TFact> Fire(TFact fact) =>
        vetoes.Value.Fold(Fin.Succ(fact), static (state, veto) => state.Bind(veto))
            .Map(admitted => {
                if (Modality == HookModality.Replay)
                    ignore(buffer.Swap(held => (held.Add(admitted) is var next && next.Count > depth ? next.Skip(next.Count - depth) : next).Strict()));
                ignore(taps.Value.Iter(tap => HookIsolation.Shielded(Id, admitted, tap)));
                return admitted;
            });

    public Fin<IDisposable> Veto(Func<TFact, Fin<TFact>> gate) =>
        Modality == HookModality.Veto
            ? Fin.Succ(Attach(vetoes, gate))
            : Fin.Fail<IDisposable>(new HookFault.ModalityRefused(Id.ToString(), Modality.Key));

    public IDisposable Observe(Func<TFact, IO<Unit>> tap) {
        var detach = Attach(taps, tap);
        ignore(buffer.Value.Iter(held => HookIsolation.Shielded(Id, held, tap)));
        return detach;
    }

    public Seq<TFact> Drain() => buffer.Value;

    static IDisposable Attach<T>(Atom<Seq<T>> cell, T row) {
        ignore(cell.Swap(held => held.Add(row)));
        return new HookDetacher(() => ignore(cell.Swap(held => held.Filter(entry => !ReferenceEquals(entry, row)).ToSeq().Strict())));
    }
}

file sealed record HookDetacher(Action Release) : IDisposable {
    public void Dispose() => Release();
}
```

## [04]-[HOOK_REGISTRY]

- Owner: `HookRegistry` — the composition-time mount folding every declared point into one frozen id-keyed table; `HookRail` — the named point roster the spine fires.
- Cases: `HookRail` rows — `Receipt` (every `ReceiptEnvelope` the sink emits, observe), `Phase` (every `PhaseReceipt` commit, observe), `Command` (command intent pre-dispatch, veto), `Delivery` (per-channel delivery outcome, observe), `Degradation` (level transitions, replay so a late panel reads the recent path).
- Entry: `HookRegistry.Mount(params ReadOnlySpan<IHookPoint> points)` — freezes the table and throws on a duplicate id at composition, the same structural-collision law the instrumentation-scope registry carries; subscription reaches a point through its declared `HookRail` field, so a name-resolved lookup surface never exists.
- Auto: `HookRail.Tap(ReceiptSinkPort sink, HookRail rail)` decorates the sink's `Emit` delegate so every envelope fires the `Receipt` point before egress — the one seam where the receipt fan and the hook rail meet, composed at the root, so no emitting page changes; phase taps ride the existing `UiSchedulerPort.Phases` subscription shape.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new hook point is one `HookRail` field and one `Mount` row; a foreign package contributes subscribers only — points are declared by the owning AppHost page.
- Boundary: the registry is a naming-and-isolation law over existing delivery, so it carries no queue, no scheduler, and no retry — asynchronous fan-out is the Topics bus, ordered delivery is the HLC stamp the envelope already carries, and durability is the outbox leg; apps add subscribers at composition and libraries never wire exporters through a tap; the frozen table is the audit surface — a fired id outside it is unreachable by construction because firing requires the declared point value.

```csharp signature
public sealed record HookRail(
    HookPoint<ReceiptEnvelope> Receipt,
    HookPoint<PhaseReceipt> Phase,
    HookPoint<CommandIntent> Command,
    HookPoint<DeliveryReceipt> Delivery,
    HookPoint<DegradationReading> Degradation) {
    public static HookRail Live() => new(
        new HookPoint<ReceiptEnvelope>(HookId.Create("rasm.apphost.receipt.emitted"), HookModality.Observe),
        new HookPoint<PhaseReceipt>(HookId.Create("rasm.apphost.lifecycle.phase"), HookModality.Observe),
        new HookPoint<CommandIntent>(HookId.Create("rasm.apphost.command.admit"), HookModality.Veto),
        new HookPoint<DeliveryReceipt>(HookId.Create("rasm.apphost.delivery.settled"), HookModality.Observe),
        new HookPoint<DegradationReading>(HookId.Create("rasm.apphost.health.degradation"), HookModality.Replay));

    public static ReceiptSinkPort Tap(ReceiptSinkPort sink, HookRail rail) =>
        sink with {
            Emit = envelope => IO.lift(() => rail.Receipt.Fire(envelope)).Bind(_ => sink.Emit(envelope)),
        };
}

public sealed record HookRegistry(FrozenDictionary<string, IHookPoint> Points) {
    public static HookRegistry Mount(params ReadOnlySpan<IHookPoint> points) =>
        new(points.ToArray().ToFrozenDictionary(static point => point.Id.ToString(), static point => point, StringComparer.Ordinal));
}
```

## [05]-[FAULT_ISOLATION]

- Owner: `HookFault` `[Union]` deriving through `FaultBand.Hook`; `HookIsolation` — the shielded tap runner.
- Cases: `Text`, `MalformedId`, `ModalityRefused`, `TapFaulted`.
- Entry: `HookIsolation.Shielded(HookId id, TFact fact, Func<TFact, IO<Unit>> tap)` — every observe delivery runs through it.
- Auto: a throwing or failing tap converts to `HookFault.TapFaulted` carrying the hook id and the tap's error text, folds into the `HookIsolation.Faults` evidence cell the log projection drains as one `SpineLog` event stream, and the emitter's `Fire` result is untouched — subscriber failure is a fault on the fault rail, never a broken emitter; a veto refusal is the point's contract, returned on the emitter's rail as the veto's own typed fault.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: one fault case per new refusal class, breaking every consumer arm at compile time.
- Boundary: telemetry is a tap, never a producer — the Observability/instruments#RECEIPT_PROJECTION fan, the log projection, and any span enrichment subscribe as observe rows here, so domain code fires facts and observability projects them; a tap that must never lose an event does not belong on the rail — it is a durable outbox consumer, selected by the delivery-honesty axis, not a hook subscriber.

```csharp signature
[Union]
public abstract partial record HookFault : Expected, IValidationError<HookFault> {
    private HookFault(string detail, int code) : base(detail, code, None) { }
    public static HookFault Create(string message) => new Text(message);
    public sealed record Text : HookFault { public Text(string detail) : base(detail, FaultBand.Hook.Code(0)) { } }
    public sealed record MalformedId : HookFault { public MalformedId(string id) : base(id, FaultBand.Hook.Code(1)) { } }
    public sealed record ModalityRefused : HookFault { public ModalityRefused(string id, string modality) : base($"{id}:{modality}", FaultBand.Hook.Code(2)) { } }
    public sealed record TapFaulted : HookFault { public TapFaulted(string id, string detail) : base($"{id}: {detail}", FaultBand.Hook.Code(3)) { } }
}

public static class HookIsolation {
    public static readonly Atom<Seq<HookFault>> Faults = Atom(Seq<HookFault>());

    public static Unit Shielded<TFact>(HookId id, TFact fact, Func<TFact, IO<Unit>> tap) =>
        Try.lift(() => tap(fact).Run()).Run().Match(
            Succ: static _ => unit,
            Fail: error => ignore(Faults.Swap(held => held.Add(new HookFault.TapFaulted(id.ToString(), error.Message)))));
}
```

## [06]-[RESEARCH]

- [SINK_DECORATION]-[OPEN]: `ReceiptSinkPort` `with { Emit = ... }` decoration order against the HLC stamp — the tap must observe the stamped envelope, so the decorated `Emit` fires after `Send` composes the stamp; confirm the record-`with` seam preserves the one-mint law when two decorators stack at one root, against the Runtime/ports#PORT_RECORDS fence.

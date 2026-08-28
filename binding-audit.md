# 1. Remove unused flow keys

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:46-60` — `[02]-[FLOW]`, `FlowMode`
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlowMode {
    public static readonly FlowMode Both = new(key: "both", host: DualBindingMode.TwoWay, refresh: BindingUpdateMode.Destination);
    public static readonly FlowMode IntoControl = new(key: "into-control", host: DualBindingMode.OneWay, refresh: BindingUpdateMode.Destination);
    public static readonly FlowMode IntoSource = new(key: "into-source", host: DualBindingMode.OneWayToSource, refresh: BindingUpdateMode.Source);
    public static readonly FlowMode Seed = new(key: "seed", host: DualBindingMode.OneTime, refresh: BindingUpdateMode.Destination);
    public static readonly FlowMode Manual = new(key: "manual", host: DualBindingMode.Manual, refresh: BindingUpdateMode.Destination);

    internal DualBindingMode Host { get; }
    internal BindingUpdateMode Refresh { get; }

    internal bool ToSource => Host is DualBindingMode.TwoWay or DualBindingMode.OneWayToSource;
    internal bool ToControl => Host is DualBindingMode.TwoWay or DualBindingMode.OneWay or DualBindingMode.OneTime;
}
```

To:
```csharp
[SmartEnum]
public sealed partial class FlowMode {
    public static readonly FlowMode Both = new(host: DualBindingMode.TwoWay);
    public static readonly FlowMode IntoControl = new(host: DualBindingMode.OneWay);
    public static readonly FlowMode IntoSource = new(host: DualBindingMode.OneWayToSource);
    public static readonly FlowMode Seed = new(host: DualBindingMode.OneTime);
    public static readonly FlowMode Manual = new(host: DualBindingMode.Manual);

    internal DualBindingMode Host { get; }
    internal BindingUpdateMode Refresh => Host == DualBindingMode.OneWayToSource
        ? BindingUpdateMode.Source
        : BindingUpdateMode.Destination;
    internal bool ToSource => Host is DualBindingMode.TwoWay or DualBindingMode.OneWayToSource;
    internal bool ToControl => Host is DualBindingMode.TwoWay or DualBindingMode.OneWay or DualBindingMode.OneTime;
}
```

Why: The key and five strings are unused after `BindFusion.Wire` is deleted. The keyless owner remains load-bearing because it prevents arbitrary cast values of the foreign enum from entering a plan.

Change: Make the behavior vocabulary keyless and derive refresh direction from its admitted host mode.

Delta: -1 authored LOC, -1 generated key member, and -5 string literals; no module-level type reduction.


# 2. Delete private legality vocabularies

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:62-76` — `[02]-[FLOW]`, `SourceKind` and `CadenceKind`
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SourceKind {
    public static readonly SourceKind State = new(key: "state");
    public static readonly SourceKind Context = new(key: "context");
    public static readonly SourceKind Seed = new(key: "seed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CadenceKind {
    public static readonly CadenceKind Edit = new(key: "edit");
    public static readonly CadenceKind Commit = new(key: "commit");
    public static readonly CadenceKind Debounced = new(key: "debounced");
}
```

To:
```csharp
// SourceKind and CadenceKind DELETED
```

Why: Both types mirror cases already carried by `BindSource<TValue>` and `Cadence`; no consumer needs their generated surfaces.

Change: Match source and timing cases directly in plan admission.

Delta: -15 authored LOC, -2 module-level types, and -6 authored members.

# 3. Name delay behavior by standard terms

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:82-98` — `[02]-[FLOW]`, `Cadence` cases, `Kind`, and `Apply`
```csharp
    public sealed record Edit : Cadence;
    public sealed record Commit : Cadence;
    public sealed record Coalesced(PositiveMagnitude Window) : Cadence;
    public sealed record Restarted(PositiveMagnitude Window) : Cadence;

    public CadenceKind Kind => Switch(
        edit: static _ => CadenceKind.Edit,
        commit: static _ => CadenceKind.Commit,
        coalesced: static _ => CadenceKind.Debounced,
        restarted: static _ => CadenceKind.Debounced);

    internal IndirectBinding<TValue> Apply<TValue>(IndirectBinding<TValue> path) => Switch(
        state: path,
        edit: static (source, _) => source,
        commit: static (source, _) => source,
        coalesced: static (source, cadence) => source.AfterDelay(TimeSpan.FromSeconds(cadence.Window.Value), reset: false),
        restarted: static (source, cadence) => source.AfterDelay(TimeSpan.FromSeconds(cadence.Window.Value), reset: true));
```

To:
```csharp
    public sealed record Edit : Cadence;
    public sealed record Commit : Cadence;
    public sealed record Throttled(PositiveMagnitude Window) : Cadence;
    public sealed record Debounced(PositiveMagnitude Window) : Cadence;

    internal IndirectBinding<TValue> Apply<TValue>(IndirectBinding<TValue> path) => Switch(
        state: path,
        edit: static (source, _) => source,
        commit: static (source, _) => source,
        throttled: static (source, cadence) => source.AfterDelay(TimeSpan.FromSeconds(cadence.Window.Value), reset: false),
        debounced: static (source, cadence) => source.AfterDelay(TimeSpan.FromSeconds(cadence.Window.Value), reset: true));
```

Why: Eto's `reset: false` delays without restarting and `reset: true` restarts the delay; throttle and debounce are the established names. `Kind` duplicates the union.

Change: Rename both delay cases and delete the derived legality member.

Delta: -6 authored LOC and -1 authored member.

Ripples: Rename `RejectReason.DebouncedPath` to `RejectReason.DelayedPath` in `libs/dotnet/Rasm/.planning/Interaction/dispatch.md`, and update constructions of the two renamed cases.

# 4. Keep only host-distinct binding sources

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:134-147` — `[03]-[SOURCE]`, `BindSource<TValue>` cases and `Kind`
```csharp
    public sealed record FromState(DirectBinding<TValue> Channel) : BindSource<TValue>;
    public sealed record FromValue(TValue Value) : BindSource<TValue>;
    public sealed record FromContext(IndirectBinding<TValue> Path) : BindSource<TValue>;
    public sealed record Named(string Property) : BindSource<TValue>;
    public sealed record Delegated(Func<object, TValue> Get, Option<Action<object, TValue>> Put, Option<string> Notify) : BindSource<TValue>;
    public sealed record Child(BindSource<object> Parent, BindSource<TValue> Member) : BindSource<TValue>;

    public SourceKind Kind => Switch(
        fromState: static _ => SourceKind.State,
        fromValue: static _ => SourceKind.Seed,
        fromContext: static _ => SourceKind.Context,
        named: static _ => SourceKind.Context,
        delegated: static _ => SourceKind.Context,
        child: static _ => SourceKind.Context);
```

To:
```csharp
    public sealed record FromState(DirectBinding<TValue> Channel) : BindSource<TValue>;
    public sealed record FromValue(TValue Value) : BindSource<TValue>;
    public sealed record FromContext(IndirectBinding<TValue> Path) : BindSource<TValue>;
```

Why: `Named`, `Delegated`, and `Child` postpone Eto construction before immediately lowering to `IndirectBinding<TValue>`. `Kind` restates the surviving cases.

Change: Construct every context accessor with Eto before creating the source case.

Delta: -11 authored LOC, -3 nested case types, and -1 authored member.

# 5. Delete binding-source forwarding members

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:149-157` — `[03]-[SOURCE]`, `BindSource<TValue>.Path`, `State`, and `Drill`
```csharp
    public static BindSource<TValue> Path<TContext>(Expression<Func<TContext, TValue>> path) =>
        new FromContext(Path: Binding.Property(path));

    public static BindSource<TValue> State<TState>(StateCell<TState> cell, Lens<TState, TValue> lens) =>
        new FromState(Channel: cell.Channel(lens, key));

    public Fin<BindSource<TNext>> Drill<TNext>(Expression<Func<TValue, TNext>> path) => Lower()
        .ToFin(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(path)), Reason: RejectReason.NoChildPath))
        .Map(parent => (BindSource<TNext>)new BindSource<TNext>.FromContext(Path: parent.Child(path)));
```

To:
```csharp
// Path, State, and Drill DELETED
```

Why: Each member forwards one public Eto or `StateCell` call; `State` also references an undefined `key`.

Change: Construct cases directly from `Binding.Property`, `IndirectBinding.Child`, or `StateCell.Channel` results.

Delta: -9 authored LOC and -3 authored members.

# 6. Project only data-context sources

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:159-174` — `[03]-[SOURCE]`, `BindSource<TValue>.Lower`
```csharp
    internal Option<IndirectBinding<TValue>> Lower() => Switch(
        fromState: static _ => Option<IndirectBinding<TValue>>.None,
        fromValue: static _ => Option<IndirectBinding<TValue>>.None,
        fromContext: static source => Some(source.Path),
        named: static source => Some<IndirectBinding<TValue>>(new PropertyBinding<TValue>(source.Property)),
        delegated: static source => Some(source.Notify.Match<IndirectBinding<TValue>>(
            Some: notify => new DelegateBinding<object, TValue>(
                getValue: source.Get,
                setValue: HostEdge.Slot(source.Put),
                notifyProperty: notify),
            None: () => new DelegateBinding<object, TValue>(
                getValue: source.Get,
                setValue: HostEdge.Slot(source.Put)))),
        child: static source => (source.Parent.Lower(), source.Member.Lower())
            .Apply(static member => parent.Child(binding: member))
            .As());
```

To:
```csharp
    internal Option<IndirectBinding<TValue>> Lower() => Switch(
        fromState: static _ => None,
        fromValue: static _ => None,
        fromContext: static source => Some(source.Path));
```

Why: The deleted arms duplicate Eto factories, and the `Child` arm references `parent` outside its binding lambda.

Change: Keep the total projection over the three surviving cases.

Delta: -12 authored LOC.

# 7. Remove the obsolete expression-tree import

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:123` — `[03]-[SOURCE]` imports
```csharp
using System.Linq.Expressions;
```

To:
```csharp
// System.Linq.Expressions import DELETED
```

Why: No surviving source member accepts an expression tree.

Change: Delete the unused import.

Delta: -1 authored LOC.

# 8. Delete the duplicate lens owner

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:178-182` — `[03]-[SOURCE]`, `Lens<TState,TValue>`
```csharp
public sealed record Lens<TState, TValue>(Func<TState, TValue> Get, Func<TState, TValue, TState> Put) {
    public Lens<TState, TNext> Then<TNext>(Lens<TValue, TNext> next) => new(
        Get: state => next.Get(Get(state)),
        Put: (state, value) => Put(state, next.Put(Get(state), value)));
}
```

To:
```csharp
// Lens<TState, TValue> DELETED
```

Why: LanguageExt already owns the composable immutable `Lens<A,B>` used by the branch substrate.

Change: Resolve existing signatures to `LanguageExt.Lens<TState,TValue>`.

Delta: -5 authored LOC, -1 module-level type, and -1 authored member.

# 9. Write through the LanguageExt lens

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:197` — `[03]-[SOURCE]`, `StateCell<TState>.Channel`
```csharp
            setValue: value => Park(Cell.Commit(state, held => lens.Put(held, value))),
```

To:
```csharp
            setValue: value => Park(Cell.Commit(state, held => lens.Set(value, held))),
```

Why: LanguageExt writes a focused value with `Lens.Set(B value, A container)`; `Put` belongs only to the deleted local record.

Change: Use the package-owned immutable setter inside the compare-and-swap computation.

Delta: 0 LOC and no member or type reduction.

# 10. Delete the delegate-pair wrapper

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:258-265` — `[04]-[GATE]`, `ValueGate<TRaw,TModel>`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct ValueGate<TRaw, TModel>(Func<TModel, TRaw> Render, Func<TRaw, Fin<TModel>> Admit) {
    public static Fin<ValueGate<TRaw, TModel>> Of(Func<TModel, TRaw> render, Func<TRaw, Fin<TModel>> admit) {
        return from rendered in Admit.Need(render)
               from admitted in Admit.Need(admit)
               select new ValueGate<TRaw, TModel>(Render: rendered, Admit: admitted);
    }
}
```

To:
```csharp
// ValueGate<TRaw, TModel> DELETED
```

Why: The type only groups two delegates, exposes a public constructor that bypasses `Of`, and is consumed by one plan field.

Change: Carry both delegates directly in the optional conversion tuple and validate them in the plan factory.

Delta: -8 authored LOC, -1 module-level type, and -3 authored members.

# 11. Keep binding implementation state private

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:317-334` — `[04]-[GATE]`, `BindingPlan<TControl,TValue,TModel>` construction and properties
```csharp
public sealed record BindingPlan<TControl, TValue, TModel> : IBindingPlan where TControl : Control {
    private BindingPlan(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        BindFusion fusion,
        Cadence cadence,
        Option<(ValueGate<TValue, TModel> Gate, GatePolicy<TModel> Policy)> gate,
        BindLedger ledger) =>
        (Key, Select, Source, Fusion, Cadence, Gate, Ledger) = (key, select, source, fusion, cadence, gate, ledger);

    public BindingKey Key { get; }
    public Func<TControl, BindableBinding<TControl, TValue>> Select { get; }
    public BindSource<TModel> Source { get; }
    public BindFusion Fusion { get; }
    public Cadence Cadence { get; }
    public Option<(ValueGate<TValue, TModel> Gate, GatePolicy<TModel> Policy)> Gate { get; }
    public BindLedger Ledger { get; }
```

To:
```csharp
public sealed class BindingPlan<TControl, TValue, TModel> : IBindingPlan where TControl : Control {
    private BindingPlan(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        FlowMode mode,
        Cadence timing,
        Option<(Func<TModel, TValue> Render, Func<TValue, Fin<TModel>> Admit, GatePolicy<TModel> Policy)> conversion,
        BindLedger ledger) =>
        (Key, Select, Source, Mode, Timing, Conversion, Ledger) = (key, select, source, mode, timing, conversion, ledger);

    public BindingKey Key { get; }
    private Func<TControl, BindableBinding<TControl, TValue>> Select { get; }
    private BindSource<TModel> Source { get; }
    private FlowMode Mode { get; }
    private Cadence Timing { get; }
    private Option<(Func<TModel, TValue> Render, Func<TValue, Fin<TModel>> Admit, GatePolicy<TModel> Policy)> Conversion { get; }
    private BindLedger Ledger { get; }
```

Why: The plan is an identity-bearing service, not a value with delegate and mutable-ledger equality. Only `Key` and interface behavior are public.

Change: Use a class, retain the admitted flow row, absorb the deleted conversion wrapper, and hide wiring state.

Delta: 0 authored LOC, -6 public properties, and the generated record equality surface.

# 12. Use a direct plan factory signature

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:336-343` — `[04]-[GATE]`, `BindingPlan<TControl,TValue,TModel>.Admitted` signature
```csharp
    public static Fin<BindingPlan<TControl, TValue, TModel>> Admitted(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        FlowMode flow,
        Cadence cadence,
        Option<(ValueGate<TValue, TModel> Gate, GatePolicy<TModel> Policy)> gate,
        BindLedger ledger) {
```

To:
```csharp
    public static Fin<BindingPlan<TControl, TValue, TModel>> Create(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        FlowMode mode,
        Cadence timing,
        Option<(Func<TModel, TValue> Render, Func<TValue, Fin<TModel>> Admit, GatePolicy<TModel> Policy)> conversion,
        BindLedger ledger) {
```

Why: The factory should accept the admitted flow row and the direct conversion components it stores; `Admitted` is not an action verb.

Change: Rename the factory to `Create` and delete the conversion wrapper from its signature.

Delta: 0 LOC and no member or type reduction.

Ripples: Replace `BindingPlan.Admitted` calls with `BindingPlan.Create` and pass the optional conversion tuple directly.

# 13. Validate optional conversion components applicatively

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:344-351` — `[04]-[GATE]`, `BindingPlan<TControl,TValue,TModel>.Admitted` dependency join
```csharp
        return (Admit.Need(value: select).ToValidation(),
                Admit.Need(value: source).ToValidation(),
                Admit.Need(value: flow).ToValidation(),
                Admit.Need(value: cadence).ToValidation(),
                Admit.Need(value: ledger).ToValidation())
            .Apply(static (chosen, admitted, mode, timing, book) =>
                (Select: chosen, Source: admitted, Flow: mode, Cadence: timing, Ledger: book))
            .As()
```

To:
```csharp
        return (Admit.Need(select).ToValidation(), Admit.Need(source).ToValidation(),
                Admit.Need(mode).ToValidation(), Admit.Need(timing).ToValidation(),
                Admit.Need(ledger).ToValidation(),
                conversion.Traverse(static held =>
                    (Admit.Need(held.Render).ToValidation(), Admit.Need(held.Admit).ToValidation(), Admit.Need(held.Policy).ToValidation())
                        .Apply(static (render, admit, policy) => (Render: render, Admit: admit, Policy: policy))).As())
            .Apply(static (chosen, admitted, flow, cadence, book, converted) =>
                (Select: chosen, Source: admitted, Mode: flow, Timing: cadence, Ledger: book, Conversion: converted))
            .As()
```

Why: Required dependencies and the three optional conversion components are independent, so admission must report every absence.

Change: Traverse the optional tuple, accumulate component failures, and join it with the required dependencies.

Delta: +3 authored LOC and no member or type increase.

# 14. Admit binding combinations at the plan boundary

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:353-355` — `[04]-[GATE]`, final `BindingPlan<TControl,TValue,TModel>.Admitted` bind
```csharp
            .Bind(held => BindLaw
                .Admit(new BindFusion(Source: held.Source.Kind, Flow: held.Flow, Timing: held.Cadence.Kind))
                .Map(fusion => new BindingPlan<TControl, TValue, TModel>(held.Select, held.Source, fusion, held.Cadence, gate, held.Ledger)));
```

To:
```csharp
            .Bind(held => (
                (held.Mode == FlowMode.Seed) == (held.Source is BindSource<TModel>.FromValue) ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.SeedFlow)),
                held.Source is not BindSource<TModel>.FromValue || held.Timing is Cadence.Edit ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.SeedTiming)),
                held.Timing is not (Cadence.Throttled or Cadence.Debounced) || held.Source is BindSource<TModel>.FromContext && held.Mode.ToControl ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.DelayedPath)),
                held.Timing is not Cadence.Commit || held.Mode.ToSource ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.CommitFlow)),
                held.Mode != FlowMode.Manual || held.Timing is Cadence.Edit ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.ManualTiming)))
                .Apply((_, _, _, _, _) => new BindingPlan<TControl, TValue, TModel>(
                    key, held.Select, held.Source, held.Mode, held.Timing, held.Conversion, held.Ledger))
                .As().ToFin());
```

Why: The five conditions are independent and belong at the sole constructor boundary; the direct cases and admitted flow row are the full discriminants.

Change: Accumulate every violated condition without a mirrored product, rule rows, or cached cross-product.

Delta: +7 authored LOC locally; tasks 15–17 remove 40 authored LOC, 3 module-level types, and 11 authored members.

# 15. Delete the duplicated binding product

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:268-271` — `[04]-[GATE]`, `BindFusion`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct BindFusion(SourceKind Source, FlowMode Flow, CadenceKind Timing) {
    public string Wire => $"{Source.Key}/{Flow.Key}/{Timing.Key}";
}
```

To:
```csharp
// BindFusion DELETED
```

Why: The record duplicates values already held by the plan, and `Wire` allocates an unused diagnostic string.

Change: Delete the mirrored product and read the plan's source, mode, and timing directly.

Delta: -4 authored LOC, -1 module-level type, and -1 authored member.

# 16. Delete the generated rule wrapper

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:273-291` — `[04]-[GATE]`, `FusionLaw`
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FusionLaw {
    public static readonly FusionLaw SeedFlow = new(key: "seed-flow", reason: RejectReason.SeedFlow,
        admits: static fusion => (fusion.Flow == FlowMode.Seed) == (fusion.Source == SourceKind.Seed));
    public static readonly FusionLaw SeedTiming = new(key: "seed-timing", reason: RejectReason.SeedTiming,
        admits: static fusion => fusion.Source != SourceKind.Seed || fusion.Timing == CadenceKind.Edit);
    public static readonly FusionLaw DebouncedPath = new(key: "debounced-path", reason: RejectReason.DebouncedPath,
        admits: static fusion => fusion.Timing != CadenceKind.Debounced
            || (fusion.Source == SourceKind.Context && fusion.Flow.ToControl));
    public static readonly FusionLaw CommitFlow = new(key: "commit-flow", reason: RejectReason.CommitFlow,
        admits: static fusion => fusion.Timing != CadenceKind.Commit || fusion.Flow.ToSource);
    public static readonly FusionLaw ManualTiming = new(key: "manual-timing", reason: RejectReason.ManualTiming,
        admits: static fusion => fusion.Flow != FlowMode.Manual || fusion.Timing == CadenceKind.Edit);

    internal RejectReason Reason { get; }

    [UseDelegateFromConstructor] internal partial bool Admits(BindFusion fusion);
}
```

To:
```csharp
// FusionLaw DELETED
```

Why: Each row wraps one predicate used by one constructor and duplicates the `RejectReason` identity it carries.

Change: Delete the rule vocabulary after its predicates move to plan admission.

Delta: -19 authored LOC, -1 module-level type, and -7 authored members.

# 17. Delete the legality cache and forwarding owner

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:293-309` — `[04]-[GATE]`, `BindLaw`
```csharp
public static class BindLaw {
    public static FrozenSet<BindFusion> Legal => Roster.Value;

    private static readonly Lazy<FrozenSet<BindFusion>> Roster = new(static () =>
        (from source in SourceKind.Items
         from flow in FlowMode.Items
         from timing in CadenceKind.Items
         let fusion = new BindFusion(Source: source, Flow: flow, Timing: timing)
         where toSeq(FusionLaw.Items).ForAll(law => law.Admits(fusion))
         select fusion).ToFrozenSet());

    public static Fin<BindFusion> Admit(BindFusion fusion, BindingKey key) => Legal.Contains(fusion)
        ? Fin.Succ(fusion)
        : Fin.Fail<BindFusion>(Error.Many(toSeq(FusionLaw.Items)
            .Filter(law => !law.Admits(fusion))
            .Map(law => (Error)new UiFault.Rejected(Field: FieldTag.Create(value: key.Value), Reason: law.Reason))));
}
```

To:
```csharp
// BindLaw DELETED
```

Why: The cache materializes a cross-product to optimize one construction-time check, and `Admit` is a forwarding hop around it.

Change: Delete both after direct applicative admission replaces them.

Delta: -17 authored LOC, -1 module-level type, and -3 authored members.

# 18. Remove legality-cache imports

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:240-241` — `[04]-[GATE]` imports
```csharp
using System.Collections.Frozen;
using System.Linq;
```

To:
```csharp
// System.Collections.Frozen and System.Linq imports DELETED
```

Why: Direct applicative admission uses neither namespace.

Change: Delete both unused imports.

Delta: -2 authored LOC.

# 19. Delete the disconnected commit latch

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:370-384` — `[04]-[GATE]`, `CommitLatch<TPayload>`
```csharp
internal sealed class CommitLatch<TPayload> : IDisposable {
    private readonly Atom<Option<TPayload>> gate = Atom(Option<TPayload>.None);

    internal static Fin<CommitLatch<TPayload>> Arm(Control control, Action<TPayload> commit);

    internal Transition<Option<TPayload>> Offer(TPayload value) =>
        Cell.Commit(gate, _ => Some(value));

    internal Transition<Option<TPayload>> Drain() => Cell.Step(
        gate,
        static held => held.IsSome ? Some(Option<TPayload>.None) : Option<Option<TPayload>>.None,
        new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Drain)), Reason: RejectReason.EmptyLatch));

    public void Dispose();
}
```

To:
```csharp
// CommitLatch<TPayload> DELETED
```

Why: `Drain` clears the option without returning its payload or invoking `commit`, and it misclassifies an empty focus transition as a fault. Eto's live `DualBinding<T>` already owns the pending control value and exposes `Update(BindingUpdateMode.Source)`.

Change: For commit timing, bind `TwoWay` as `OneWay` and `OneWayToSource` as `Manual`, then register one exact `LostFocus` handler that calls `Update(Source)` and is removed by the link's unbind closure.

Delta: -15 authored LOC, -1 module-level type, and -4 authored members.

# 20. Reuse the canonical positive count

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:411-415` — `[05]-[LEDGER]`, `LedgerCapacity`
```csharp
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct LedgerCapacity {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(message: "LedgerCapacity must be positive.");
}
```

To:
```csharp
// LedgerCapacity DELETED
```

Why: `Dimension` already owns positive integral counts through the canonical `Band.Count` admission.

Change: Use `Dimension` as the history bound.

Delta: -5 authored LOC, -1 module-level type, and -1 authored member.

# 21. Construct ledgers from admitted capacity

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:423-434` — `[05]-[LEDGER]`, `BindLedger` fields and construction
```csharp
public sealed class BindLedger {
    private readonly LedgerCapacity capacity;
    private readonly Atom<BindLedgerState> state;

    private BindLedger(LedgerCapacity capacity) {
        this.capacity = capacity;
        state = Atom(new BindLedgerState(Next: 0L, History: Seq<BindLedgerEntry>(), Current: HashMap<BindingKey, UiFault>()));
    }

    public static Fin<BindLedger> Admitted(LedgerCapacity capacity) => capacity.Value > 0
        ? Fin.Succ(new BindLedger(capacity))
        : Fin.Fail<BindLedger>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(capacity)), Reason: RejectReason.Capacity));
```

To:
```csharp
public sealed class BindLedger {
    private readonly Dimension capacity;
    private readonly Atom<BindLedgerState> state;

    public BindLedger(Dimension capacity) {
        this.capacity = capacity;
        state = Atom(new BindLedgerState(Next: 0L, History: Seq<BindLedgerEntry>(), Current: HashMap<BindingKey, UiFault>()));
    }
```

Why: `Dimension` is already admitted, so a result factory that repeats positivity is both duplicate validation and a forwarding constructor.

Change: Accept the admitted count directly and delete `BindLedger.Admitted`.

Delta: -4 authored LOC and -1 authored member.

Ripples: Replace `BindLedger.Admitted(capacity)` with `new BindLedger(capacity)` and admit raw counts through `Dimension` before construction.

# 22. Align ledger imports with retained owners

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:403-406` — `[05]-[LEDGER]` imports
```csharp
using Eto.Forms;
using Rasm.Domain;
using Thinktecture;
```

To:
```csharp
using Eto.Forms;
using Rasm.Domain;
using Rasm.Numerics;
```

Why: Deleting `LedgerCapacity` removes the section's Thinktecture declaration; `Dimension` comes from `Rasm.Numerics`.

Change: Replace the unused generator namespace with the count owner namespace.

Delta: 0 LOC and no member or type reduction.

# 23. Complete ledger entries without imperative locals

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:441-453` — `[05]-[LEDGER]`, `BindLedger.Reject` and `Accept`
```csharp
    public Transition<BindLedgerState> Reject(BindingKey key, UiFault fault) => Cell.Commit(
        state,
        held => {
            BindLedgerEntry entry = new(Ordinal: held.Next + 1L, Fault: fault);
            Seq<BindLedgerEntry> history = held.History.Add(entry);
            return new BindLedgerState(
                Next: entry.Ordinal,
                History: history.Count > capacity.Value ? history.Skip(history.Count - capacity.Value) : history,
                Current: held.Current.AddOrUpdate(key, fault));
        });

    public Transition<BindLedgerState> Accept(BindingKey key) =>
        Cell.Commit(state, held => held with { Current = held.Current.Remove(key) });
```

To:
```csharp
    internal Transition<BindLedgerState> Reject(BindingKey key, UiFault fault) => Cell.Commit(
        state,
        held => new BindLedgerState(
            Next: held.Next + 1L,
            History: held.History
                .Add(new BindLedgerEntry(held.Next + 1L, key, fault))
                .Skip(int.Max(0, held.History.Count + 1 - capacity.Value)),
            Current: held.Current.AddOrUpdate(key, fault)));

    internal Transition<BindLedgerState> Accept(BindingKey key) =>
        Cell.Commit(state, held => held with { Current = held.Current.Remove(key) });
```

Why: The current entry omits its required key, the locals restate derived values, and public methods expose the internal state type.

Change: Construct the complete entry and bounded history in one pure transition; keep mutations internal.

Delta: -2 authored LOC and -2 public members.

# 24. Remove the unused current-map projection

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:436-439` — `[05]-[LEDGER]`, `BindLedger` reads
```csharp
    public Seq<BindLedgerEntry> History => state.Value.History;
    public HashMap<BindingKey, UiFault> Current => state.Value.Current;

    public Option<UiFault> Holds(BindingKey key) => state.Value.Current.Find(key);
```

To:
```csharp
    public Seq<BindLedgerEntry> History => state.Value.History;
    internal Option<UiFault> Holds(BindingKey key) => state.Value.Current.Find(key);
```

Why: No consumer needs the entire current map; `BindLink` alone reads keyed refusal state.

Change: Delete the unused projection and keep the keyed read internal.

Delta: -2 authored LOC, -1 authored member, and -1 public member.

# 25. Use the binding key name consistently

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:461-468` — `[05]-[LEDGER]`, `BindLink` constructor, identity, and validity
```csharp
    internal BindLink(BindingKey identity, BindLedger ledger, Func<Fin<Unit>> refresh, Func<Fin<Unit>> unbind);

    public BindingKey Identity { get; }
    public Seq<Error> ReleaseFaults => teardown.Value;

    public bool IsValid => ValidityClaim.All(
        released.Value is false,
        ledger.Holds(Identity).IsNone);
```

To:
```csharp
    internal BindLink(BindingKey key, BindLedger ledger, Func<Fin<Unit>> refresh, Func<Fin<Unit>> unbind);

    public BindingKey Key { get; }
    public Seq<Error> ReleaseFaults => teardown.Value;

    public bool IsValid => ValidityClaim.All(
        released.Value is false,
        ledger.Holds(Key).IsNone);
```

Why: The plan, entry, and ledger call the same value `Key`; `Identity` introduces a second term without a distinct concept.

Change: Use `Key` across the lifecycle.

Delta: 0 LOC and no member or type reduction.

# 26. Implement ambient data-context assignment

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:475-478` — `[05]-[LEDGER]`, `DataScope`
```csharp
public static class DataScope {
    public static Fin<Unit> Assign(IBindable root, object model);
}
```

To:
```csharp
public static class DataScope {
    public static Fin<Unit> Assign(IBindable root, object model) =>
        (Admit.Need(root).ToValidation(), Admit.Need(model).ToValidation())
            .Apply(static (bindable, context) => (Root: bindable, Model: context))
            .As().ToFin()
            .Bind(static scope => UiThread.Run(
                new UiDispatch<Unit>.Blocking(() => Try.lift(() =>
                    HostEdge.Side(() => scope.Root.DataContext = scope.Model)).Run()),
                DispatchLane.Immediate));
}
```

Why: The declaration has no implementation. Root and model admission are independent; assignment depends on both and is UI-affine.

Change: Accumulate absent inputs, marshal once, and capture the host assignment.

Delta: +6 authored LOC and no member or type increase.

# 27. Name the collection source by its role

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:505-515` — `[06]-[STORE]`, `StoreRow<T>`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreRow<T> where T : class {
    private StoreRow() { }

    public sealed record Eager(DataStoreCollection<T> Rows) : StoreRow<T>;
    public sealed record Virtual(IDataStore<T> Window) : StoreRow<T>;

    internal IEnumerable<object> Carrier() => Switch(
        eager: static row => (IEnumerable<object>)row.Rows,
        @virtual: static row => new DataStoreVirtualCollection<T>(store: row.Window));
}
```

To:
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreSource<T> where T : class {
    private StoreSource() { }

    public sealed record Eager(DataStoreCollection<T> Rows) : StoreSource<T>;
    public sealed record Virtual(IDataStore<T> Window) : StoreSource<T>;

    internal IEnumerable<object> Carrier() => Switch(
        eager: static row => (IEnumerable<object>)row.Rows,
        @virtual: static row => new DataStoreVirtualCollection<T>(store: row.Window));
}
```

Why: The union carries an entire eager collection or virtual window, never one row.

Change: Use the established data-binding term `StoreSource`.

Delta: 0 LOC and no member or type reduction.

# 28. Inline single-consumer store wrappers

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:517-529` — `[06]-[STORE]`, `TreeStore<T>`, `StoreSink<T>`, and `StoreItemLens`
```csharp
public sealed record TreeStore<T>(ITreeGridStore<ITreeGridItem> Store, Func<ITreeGridItem, Option<T>> Element) where T : class;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreSink<T> where T : class {
    private StoreSink() { }

    public sealed record Grid(GridView View, StoreRow<T> Rows) : StoreSink<T>;
    public sealed record List(ListControl View, StoreRow<T> Rows, Option<StoreItemLens> Lens) : StoreSink<T>;
    public sealed record Tree(TreeGridView View, TreeStore<T> Store) : StoreSink<T>;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StoreItemLens(Option<IIndirectBinding<string>> Text, Option<IIndirectBinding<string>> Key);
```

To:
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreSink<T> where T : class {
    private StoreSink() { }

    public sealed record Grid(GridView View, StoreSource<T> Source) : StoreSink<T>;
    public sealed record List(
        ListControl View, StoreSource<T> Source,
        Option<IIndirectBinding<string>> Text, Option<IIndirectBinding<string>> Key) : StoreSink<T>;
    public sealed record Tree(
        TreeGridView View, ITreeGridStore<ITreeGridItem> Source,
        Func<ITreeGridItem, Option<T>> Element) : StoreSink<T>;
}
```

Why: Both records have one consumer and add no invariant. The tree sink remains distinct because Eto requires `ITreeGridStore<ITreeGridItem>`; its typed projection remains on that case.

Change: Inline list projections and tree fields into their owning sink cases without deleting tree capability.

Delta: -2 authored LOC and -2 module-level types.

Ripples: Construct `StoreSink.List` and `StoreSink.Tree` directly at collection-control realization sites in `libs/dotnet/Rasm/.planning/Interaction/control.md`.

# 29. Implement every data-store sink

From:
`libs/dotnet/Rasm/.planning/Interaction/binding.md:532-534` — `[06]-[STORE]`, `StoreGate`
```csharp
public static class StoreGate {
    public static Fin<Unit> Mount<T>(StoreSink<T> sink) where T : class;
}
```

To:
```csharp
public static class StoreGate {
    public static Fin<Unit> Mount<T>(StoreSink<T> sink) where T : class =>
        Admit.Need(sink).Bind(admitted => UiThread.Run(
            new UiDispatch<Unit>.Blocking(() => Try.lift(() => admitted.Switch(
                grid: static row => HostEdge.Side(() => row.View.DataStore = row.Source.Carrier()),
                list: static row => (HostEdge.Side(() => row.View.DataStore = row.Source.Carrier()),
                    row.Text.Iter(text => row.View.ItemTextBinding = text),
                    row.Key.Iter(key => row.View.ItemKeyBinding = key)).ToUnit(),
                tree: static row => HostEdge.Side(() => row.View.DataStore = row.Source))).Run()),
            DispatchLane.Immediate));
}
```

Why: The declaration has no implementation, and Eto exposes three distinct sinks with two enumerable source modalities for grid and list.

Change: Marshal once, dispatch exhaustively, adapt only virtual enumerable sources, and preserve native tree assignment.

Delta: +7 authored LOC and no member or type increase.

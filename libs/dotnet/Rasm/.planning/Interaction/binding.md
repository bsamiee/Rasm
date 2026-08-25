# [RASM_BINDING]

`Rasm.Interaction` owns the one control-to-model fusion. A binding is a PLAN — a keyed value carrying its source shape, its propagation direction, its write timing, and its admission gate — that rigs onto a live control and answers a leased receipt owning refresh and exact unbind. The triple deciding whether a fusion is even expressible is a closed table rather than a guard at each use: a source, a flow, and a timing that do not name a legal row refuse at admission, so no rigged plan can hold a combination the host binding machinery cannot honour.

Both host boundaries built this twice. Rhino carried the atom-state arm, the timing axis, the legality ladder, the commit latch, and the keyed failure ledger; Grasshopper carried the model lens with its delegate and notify arms, the typed value gate with its refusal posture, the ambient context assignment, and the collection-store mount. Neither carried the other's half. This owner is the union: every axis takes the richer side's shape and the poorer side gains it, and both boundaries compose this page with no adapter.

Composition is downward: `Op`, `Lease<T>`, `Atom`, `Transition<TState>`, `Cell`, and `ValidityClaim` come from `Domain/rails`; `PositiveMagnitude` and `Dimension` from `Numerics/atoms`; `UiFault`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`. Every touch of the control tree crosses `UiThread` on the immediate lane, so this owner holds the same affinity contract as every other surface on the sub-domain rather than standing as its unmarshalled exception. Each crossing is a `Blocking` case, whose static type selects the SYNCHRONOUS marshal arity — the rig composes inside its own `Fin` query and blocks on nothing, because a task-shaped return over an in-frame crossing forces every binding site to wait on a result it already holds.

## [01]-[INDEX]

- [02]-[FLOW]: `BindingKey`, `FlowMode`, `Cadence`, `SourceKind`, `CadenceKind` — the identity and the three axes a fusion is keyed on.
- [03]-[SOURCE]: `Lens<TState,TValue>`, `StateCell<TState>`, `BindSource<TValue>` — the model side, from an atom lens to a delegate accessor to a drilled child path.
- [04]-[GATE]: `ValueGate<TRaw,TModel>`, `GatePolicy<TModel>`, `BindFusion`, `BindLaw`, `BindingPlan`, `CommitLatch<TPayload>` — the typed admission seam, the legality table, and the rig.
- [05]-[LEDGER]: `LedgerCapacity`, `BindLedgerEntry`, `BindLedger`, `BindReceipt`, `DataScope` — keyed current failure over bounded history, the leased link, and the ambient model assignment.
- [06]-[STORE]: `StoreRow<T>`, `StoreItemLens`, `TreeStore<T>`, `StoreSink<T>`, `StoreRail` — the collection carriers and the one mount gate over grid, list, and tree.

## [02]-[FLOW]

- Owner: `BindingKey` the fusion identity every ledger row, refusal, and receipt is addressed by; `FlowMode` the propagation row carrying BOTH host direction columns; `Cadence` the write-timing family; `SourceKind` and `CadenceKind` the two remaining discriminants the legality table is keyed on.
- Cases: `Cadence` is `Edit` (every keystroke propagates), `Commit` (writes latch until focus leaves), `Coalesced` (a settling window that does not restart on further input), and `Restarted` (a window that restarts on every input). The host exposes the last two as one call with a reset flag; the two names make which behaviour a plan chose recoverable from the value.
- Auto: a debounce window admits as `PositiveMagnitude`, so a zero or negative window is UNREPRESENTABLE rather than a legality arm. The Rhino form carried a raw `TimeSpan` and paid for it with a `Window.Ticks <= 0` guard inside the legality ladder; that arm has no place to live here.
- Law: `FlowMode` carries `DualBindingMode` AND `BindingUpdateMode` on one row. The Grasshopper form carried direction alone, so an explicit refresh named its own update direction at each call site. The refresh column is the row's, and `ToSource`/`ToControl` DERIVE from the direction column — a stored relay pair would be a second authority over what the host mode already states.
- Law: `Manual` is a row, not the absence of a row. A link the caller pushes explicitly is a declared flow whose timing axis is fixed at `Edit`, and reading it off the direction column is what lets the legality table refuse a manual link that also asked for a latch.
- Growth: a new timing is one `Cadence` case plus one `CadenceKind` row; a new propagation is one `FlowMode` row and the legality table re-derives at type init.
- Law: all three fusion coordinates are string-keyed, because all three are READ into a refusal. `FlowMode` is the middle coordinate of `BindFusion.Wire`, so an integer key there reports `state/0/edit` — a corner naming nothing — while the other two coordinates read plainly beside it.
- Boundary: the two host enums appear on this row set and nowhere else on the sub-domain — every interior consumer reads `FlowMode`, never `DualBindingMode`.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Eto.Forms;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct BindingKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "BindingKey requires a non-blank identity.");
    }
}

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Cadence {
    private Cadence() { }

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
}
```

## [03]-[SOURCE]

- Owner: `Lens<TState,TValue>` the composable read-write pair over one state shape; `StateCell<TState>` the bridge from a kernel `Atom` into a host direct binding; `BindSource<TValue>` the closed model-side family covering an atom channel, a seed value, an admitted context path, a reflected property, a delegate accessor, and a drilled child.
- Cases: `FromState` carries a channel a `StateCell` minted, `FromValue` a one-time seed, `FromContext` an already-lowered context path, `Named` a reflected property, `Delegated` a getter with optional setter and optional change-notify property, and `Child` a parent-member composition recursing on the family itself.
- Entry: `Path` mints a `FromContext` from an expression whose context type the union cannot name; `State` mints the channel arm; `Drill` composes a child path onto any context arm and refuses typed on the state and seed arms, whose values have no live child path.
- Auto: the context band lowers to ONE host accessor through a total dispatch answering `Option`, so the plan holds an access SHAPE a reader can inspect while the host sees one binding, and the two arms with no accessor answer absence rather than a raise. A pre-lowered binding as the only case would erase which access a plan chose, which a refusal, a diagnostic, and a re-rig all need.
- Auto: `Drill`'s refusal reason DERIVES from `Kind`, so the two refusing arms are one expression rather than two restatements of the same sentence.
- Law: every atom transition returns a `Transition<TState>` verdict. `Mutate` is a public unmarshalled entry, so a compute lane may drive it; the marshal sits in the change ADAPTER, where the host handler is raised, and the mutation itself carries no affinity requirement.
- Law: the cell carries its OWN `FaultCell`, because both host seams it wires are `void`. A `void` delegate licenses no discard — a refused write and a raised change notification are facts that vanish otherwise — so the setter's verdict and the adapter's crossing both PARK, and `Faults` is where a consumer reads what the seam could not carry outward. All four transition arms are spelled, so no case reaches `ignore`.
- Law: every compare-and-swap on this page takes the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`). This page measures no lane whose contention differs from the kernel's, so a page-local budget shell would be an unanchored second ceiling.
- Law: the absent setter crosses as `Op.ToHostSlot`, the one place a `null` is a legal spelling — a host slot the domain never reads back. A hand-spelled `Match` onto `null` is the deleted form, because it puts the projection at every call site instead of at its one owner.
- Law: the lens write executes INSIDE the compare-and-swap, so `Put` stays pure — the swap retries under contention and a `Put` carrying an effect runs that effect once per attempt.
- Law: the change-adapter map is keyed on the host handler the binding machinery hands in and removal reads the stored adapter, so an add and a remove of one handler cancel exactly. A re-derived adapter closure compares unequal and leaves the atom subscribed to a released control.
- Growth: a new access shape is one case with one `Lower` arm; the family's recursion is CASE-owned, so a deeper drill costs no consumer an edit.
- Law: NAMED LOSS — `Delegated` and `Child` erase their CONTEXT type. The host accessor the band lowers to addresses `object` (`Binding.Property<TContext,TValue>` answers a context-erased `PropertyBinding<TValue>`), and a case cannot introduce a type parameter its union does not carry, so the erasure sits exactly where the host puts it. `Path<TContext>` is the one typed mint and the recursion stays typed in its VALUE at every level; what is lost is a compile-time context check on the delegate arm, which no host binding surface offers either.
- Exemption: the change-adapter map is a `ConcurrentDictionary` that MUTATES for the life of the cell — the host binding machinery adds and removes handlers from the marshal while a compute lane may still be swapping the atom, so neither a frozen table nor a rail-side keyed map serves. It is the one mutable registry on this page and it is owned by the cell that mutates it.
- Boundary: the reflected `Named` arm is the one site a model member is addressed by text, and every call site spells it through `nameof`, so a renamed property breaks at compile time rather than at first bind.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Eto.Forms;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindSource<TValue> {
    private BindSource() { }

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

    public static BindSource<TValue> Path<TContext>(Expression<Func<TContext, TValue>> path) =>
        new FromContext(Path: Binding.Property(path));

    public static BindSource<TValue> State<TState>(StateCell<TState> cell, Lens<TState, TValue> lens, Op key) =>
        new FromState(Channel: cell.Channel(lens, key));

    public Fin<BindSource<TNext>> Drill<TNext>(Expression<Func<TValue, TNext>> path, Op key) => Lower().Match(
        Some: parent => Fin.Succ<BindSource<TNext>>(new BindSource<TNext>.FromContext(Path: parent.Child(path))),
        None: () => Fin.Fail<BindSource<TNext>>(new UiFault.Rejected(
            Key: key, Field: FieldTag.Create(value: nameof(path)), Reason: RejectReason.NoChildPath)));

    internal Option<IndirectBinding<TValue>> Lower() => Switch(
        fromState: static _ => Option<IndirectBinding<TValue>>.None,
        fromValue: static _ => Option<IndirectBinding<TValue>>.None,
        fromContext: static source => Some(source.Path),
        named: static source => Some<IndirectBinding<TValue>>(new PropertyBinding<TValue>(source.Property)),
        delegated: static source => Some(source.Notify.Match<IndirectBinding<TValue>>(
            Some: notify => new DelegateBinding<object, TValue>(
                getValue: source.Get,
                setValue: Op.ToHostSlot(source.Put),
                notifyProperty: notify),
            None: () => new DelegateBinding<object, TValue>(
                getValue: source.Get,
                setValue: Op.ToHostSlot(source.Put)))),
        child: static source => (source.Parent.Lower(), source.Member.Lower())
            .Apply(static (parent, member) => parent.Child(binding: member))
            .As());
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Lens<TState, TValue>(Func<TState, TValue> Get, Func<TState, TValue, TState> Put) {
    public Lens<TState, TNext> Then<TNext>(Lens<TValue, TNext> next) => new(
        Get: state => next.Get(Get(state)),
        Put: (state, value) => Put(state, next.Put(Get(state), value)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class StateCell<TState>(Atom<TState> state, FaultCell faults) {
    private static readonly HookId Rail = HookId.Create(value: "rasm.kernel.interaction.binding.state");

    public TState Current => state.Value;
    public Seq<IsolatedFault> Faults => faults.Parked;

    public Transition<TState> Mutate(Func<TState, TState> transition) => Cell.Commit(state, transition);

    public DirectBinding<TValue> Channel<TValue>(Lens<TState, TValue> lens, Op op) {
        ConcurrentDictionary<EventHandler<EventArgs>, AtomChangedEvent<TState>> adapters = new();
        return Binding.Delegate<TValue>(
            getValue: () => lens.Get(state.Value),
            setValue: value => Park(Cell.Commit(state, held => lens.Put(held, value)), op),
            addChangeEvent: handler => {
                AtomChangedEvent<TState> adapter = _ => Park(UiThread.Run(
                    new UiDispatch<Unit>.Blocking(() => op.Catch(() => handler(this, EventArgs.Empty))),
                    DispatchLane.Immediate,
                    op));
                if (adapters.TryAdd(handler, adapter)) { state.Change += adapter; }
            },
            removeChangeEvent: handler => {
                if (adapters.TryRemove(handler, out AtomChangedEvent<TState>? adapter)) { state.Change -= adapter; }
            });
    }

    private Unit Park(Transition<TState> verdict, Op op) => verdict.Switch(
        state: op,
        committed: static (_, _) => unit,
        ceded: static (_, _) => unit,
        refused: (_, row) => ignore(faults.Park(point: Rail, cause: row.Cause)),
        contended: (key, _) => ignore(faults.Park(point: Rail, cause: key.InvalidResult())));

    private Unit Park(Fin<Unit> crossing, Op op) => crossing.Match(
        Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Rail, cause: cause)));
}
```

## [04]-[GATE]

- Owner: `ValueGate<TRaw,TModel>` the bidirectional admission seam between a control primitive and a domain value; `GatePolicy<TModel>` the refused-write posture; `BindFusion` the three-coordinate key; `FusionLaw` the five legality clause rows and `BindLaw` the table they derive; `BindingPlan<TControl,TValue,TModel>` the admitted riggable plan; `CommitLatch<TPayload>` the focus-scoped write buffer.
- Cases: `GatePolicy` is `Hold` — the control snaps back to the last admitted model value — or `Fallback` carrying an ALREADY-ADMITTED substitute. The Grasshopper form carried a raw substitute and re-admitted it inside the refusal path, so a fallback could itself refuse and the fold had nothing left but a default; carrying the model value forecloses that.
- Entry: `BindingPlan.Admitted` accumulates every absent dependency through `Validation`, gates the fusion through `BindLaw.Admit`, and stores only a legal plan; `Rig` selects the control binding, wires the source, arms the latch under commit timing, and answers a leased receipt.
- Auto: the legal roster DERIVES from the five `FusionLaw` rows at type init over the full `SourceKind × FlowMode × CadenceKind` cross-product. A clause, a flow row, or a timing kind lands as one declaration and the table re-materializes with no corner edited — that derivation is the executable statement of which fusions both host boundaries actually support.
- Auto: admission reads the roster as one frozen-set probe and reads the clause rows only to NAME a refusal, so the settled path pays a hash and the refusal reports EVERY clause the fusion violated rather than the first — one typed `Rejected` per violated row, joined through the `Error` monoid. The roster is accessor-backed, because all four generated rosters fill from their own static constructors and an eager field materializes the cross-product of three EMPTY sequences.
- Law: a `(source, flow, cadence)` triple outside the roster is unrepresentable in a rigged plan. The Rhino form guarded the same corners as an eight-arm ladder inside its factory, where each arm restated the product it excluded and no reader could enumerate what remained legal.
- Law: a refusal names its clause as a ROW, never as prose. `FusionLaw` rows carry the predicate and the `RejectReason` row a `UiFault.Rejected` reports, so recovery matches a case and a surface renders that row's `Requirement`; a joined sentence in a reason column forced both readers to parse text one side had spelled by hand. `BindFusion.Wire` survives as the plan's own diagnostic projection, read at the diagnostic edge alone.
- Law: the gate is where the branch's admission bridge meets the host binding machinery — a `[ValueObject<T>]` field binds through `Render`/`Admit` and a `[SmartEnum<TKey>]` binds its key through the row's generated validation. A parse-on-commit handler beside the control is the deleted form.
- Law: the host conversion demands totality on the to-model direction, so `Admit` and the policy fold into ONE total function and the refusal lands on the ledger rather than escaping. The exception trap catches a host-thrown conversion fault and records it, because an uncaught cast escaping a binding into the event pump is the defect this trap forecloses.
- Law: `TValue` and `TModel` stay distinct parameters and the ungated fusion is the case where they coincide, so one plan serves the primitive and admitted forms. NAMED LOSS: the primitive fusion loses its own entry signature and gains a plan whose gate reads `None`. Witness: the Grasshopper `Fuse` and `FuseGated` entries (`Eto/binding.md:96`, `:113`) become one `Admitted` call differing in one argument.
- Law: commit timing closes an interlocked latest-value latch drained when focus leaves AND once more on detach, so the value typed before a surface closes is written rather than dropped. Every latch transition answers a `Transition<Option<TPayload>>`, and a drain on an empty latch is `Refused` — a nothing-to-commit verdict, never a fault.
- Receipt: `Fin<Lease<BindReceipt>>` — the link's lifetime is the caller's custody and the receipt's key addresses its own ledger row.
- Packages: Eto.Forms for `BindableBinding`, `DualBinding`, `Convert`, `CatchException`, and `BindDataContext` (verified in `libs/dotnet/.api/api-eto-binding.md`); LanguageExt.Core for `Validation`, `Fin`, `Atom`, and `Lease`.
- Growth: a new refusal posture is one `GatePolicy` case; a new legality clause is one `FusionLaw` row beside its `RejectReason` row, and the two land together.
- Boundary: host binding construction, cadence attach, rollback, and unbind are the binding-provider statement seam, and all four cross `UiThread` on the immediate lane.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using Eto.Forms;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GatePolicy<TModel> {
    private GatePolicy() { }

    public sealed record Hold : GatePolicy<TModel>;
    public sealed record Fallback(TModel Value) : GatePolicy<TModel>;
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ValueGate<TRaw, TModel>(Func<TModel, TRaw> Render, Func<TRaw, Fin<TModel>> Admit) {
    public static Fin<ValueGate<TRaw, TModel>> Of(Func<TModel, TRaw> render, Func<TRaw, Fin<TModel>> admit, Op op) {
        return from rendered in op.Need(render)
               from admitted in op.Need(admit)
               select new ValueGate<TRaw, TModel>(Render: rendered, Admit: admitted);
    }
}

// --- [POLICIES] ------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct BindFusion(SourceKind Source, FlowMode Flow, CadenceKind Timing) {
    public string Wire => $"{Source.Key}/{Flow.Key}/{Timing.Key}";
}

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

public static class BindLaw {
    public static FrozenSet<BindFusion> Legal => Roster.Value;

    private static readonly Lazy<FrozenSet<BindFusion>> Roster = new(static () =>
        (from source in SourceKind.Items
         from flow in FlowMode.Items
         from timing in CadenceKind.Items
         let fusion = new BindFusion(Source: source, Flow: flow, Timing: timing)
         where toSeq(FusionLaw.Items).ForAll(law => law.Admits(fusion))
         select fusion).ToFrozenSet());

    public static Fin<BindFusion> Admit(BindFusion fusion, BindingKey key, Op op) => Legal.Contains(fusion)
        ? Fin.Succ(fusion)
        : Fin.Fail<BindFusion>(Error.Many(toSeq(FusionLaw.Items)
            .Filter(law => !law.Admits(fusion))
            .Map(law => (Error)new UiFault.Rejected(
                Key: op, Field: FieldTag.Create(value: key.Value), Reason: law.Reason))));
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IBindingPlan {
    BindingKey Key { get; }
    Fin<Lease<BindReceipt>> Rig(Control control, Op key);
}

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

    public static Fin<BindingPlan<TControl, TValue, TModel>> Admitted(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        FlowMode flow,
        Cadence cadence,
        Option<(ValueGate<TValue, TModel> Gate, GatePolicy<TModel> Policy)> gate,
        BindLedger ledger,
        Op op) {
        return (op.Need(value: select).ToValidation(),
                op.Need(value: source).ToValidation(),
                op.Need(value: flow).ToValidation(),
                op.Need(value: cadence).ToValidation(),
                op.Need(value: ledger).ToValidation())
            .Apply(static (chosen, admitted, mode, timing, book) =>
                (Select: chosen, Source: admitted, Flow: mode, Cadence: timing, Ledger: book))
            .As()
            .ToFin()
            .Bind(held => BindLaw
                .Admit(new BindFusion(Source: held.Source.Kind, Flow: held.Flow, Timing: held.Cadence.Kind), key, op)
                .Map(fusion => new BindingPlan<TControl, TValue, TModel>(
                    key, held.Select, held.Source, fusion, held.Cadence, gate, held.Ledger)));
    }

    [BoundaryAdapter]
    public Fin<Lease<BindReceipt>> Rig(Control control, Op key) =>
        from typed in control is TControl accepted
            ? Fin.Succ(accepted)
            : Fin.Fail<TControl>(new UiFault.Rejected(
                Key: key, Field: FieldTag.Create(value: Key.Value), Reason: RejectReason.ControlType))
        from receipt in UiThread.Run(
            new UiDispatch<BindReceipt>.Blocking(() => key.Catch(() => Wire(typed, key))),
            DispatchLane.Immediate,
            key)
        select (Lease<BindReceipt>)new Lease<BindReceipt>.Owned(receipt);

    private Fin<BindReceipt> Wire(TControl control, Op key);
}

internal sealed class CommitLatch<TPayload> : IDisposable {
    private readonly Atom<Option<TPayload>> gate = Atom(Option<TPayload>.None);
    private readonly Op key;

    internal static Fin<CommitLatch<TPayload>> Arm(Control control, Action<TPayload> commit, Op key);

    internal Transition<Option<TPayload>> Offer(TPayload value) =>
        Cell.Commit(gate, _ => Some(value));

    internal Transition<Option<TPayload>> Drain() => Cell.Step(
        gate,
        static held => held.IsSome ? Some(Option<TPayload>.None) : Option<Option<TPayload>>.None,
        new UiFault.Rejected(Key: key, Field: FieldTag.Create(value: nameof(Drain)), Reason: RejectReason.EmptyLatch));

    public void Dispose();
}
```

## [05]-[LEDGER]

- Owner: `LedgerCapacity` the admitted history bound; `BindLedgerEntry` one recorded refusal; `BindLedger` the keyed current-failure map beside bounded history; `BindReceipt` the leased link with refresh and exact unbind; `DataScope` the ambient model assignment.
- Entry: `BindLedger.Admitted` refuses a non-positive capacity before allocating; `Reject` and `Accept` are the two ledger transitions; `Holds` reads a key's current refusal; `Refresh` and `Release` are the receipt's lifecycle; `DataScope.Assign` seats a model on a bindable root.
- Auto: current failure and history are INDEPENDENT. History truncates at capacity; the current map never prunes, so a fusion that failed and stayed failed is still refusing after its entry ages out. One bounded log would silently declare a broken binding valid at capacity.
- Auto: every ledger mutation answers a `Transition<BindLedgerState>` verdict, so a caller that must know its record landed reads the case rather than assuming a swap.
- Law: `IsValid` is the ruled evidence fold — an unreleased link with no current refusal under its EXACT key. A field-only key lets two fusions on one control share a rejection state, which is precisely the failure the identity value object exists to prevent.
- Law: the release one-shot is an `Atom<bool>` seated through a guarded transition, so a second release reads a REFUSED verdict rather than no-opping into silence; a hand interlocked integer beside this page's `Atom`/`Cell`/`Transition` custody is the deleted form.
- Law: refusal routing rides `Release`, so the one non-throwing terminal every capsule on this sub-domain shares stays unmodified and this receipt still records its keyed rejection without minting a second terminal.
- Law: a teardown fault lands on the receipt's own ledger, never on the unwinding stack — disposal fires from a `finally` and from a `using` unwind, where a raise REPLACES the primary exception with a teardown fault.
- Law: `DataScope.Assign` is the ONE ambient-model seam. Assignment on a container propagates to every bound descendant, which is what makes per-control source wiring the deleted form; it crosses the marshal because propagation raises host change events across the whole subtree.
- Receipt: `BindReceipt` carries its key, its validity fold, its release faults, and a `Refresh` pushing in the flow row's declared update direction.
- Growth: a new evidence column extends `BindLedgerEntry`; retention and current failure stay independent.
- Boundary: control realization retains receipts and releases them in reverse tree order, so a partially rigged subtree unwinds exactly what it wired.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct LedgerCapacity {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(message: "LedgerCapacity must be positive.");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BindLedgerEntry(long Ordinal, BindingKey Key, UiFault Fault);

internal sealed record BindLedgerState(long Next, Seq<BindLedgerEntry> History, HashMap<BindingKey, UiFault> Current);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class BindLedger {
    private readonly LedgerCapacity capacity;
    private readonly Atom<BindLedgerState> state;

    private BindLedger(LedgerCapacity capacity) {
        this.capacity = capacity;
        state = Atom(new BindLedgerState(Next: 0L, History: Seq<BindLedgerEntry>(), Current: HashMap<BindingKey, UiFault>()));
    }

    public static Fin<BindLedger> Admitted(LedgerCapacity capacity, Op key) => capacity.Value > 0
        ? Fin.Succ(new BindLedger(capacity))
        : Fin.Fail<BindLedger>(new UiFault.Rejected(
            Key: key, Field: FieldTag.Create(value: nameof(capacity)), Reason: RejectReason.Capacity));

    public Seq<BindLedgerEntry> History => state.Value.History;
    public HashMap<BindingKey, UiFault> Current => state.Value.Current;

    public Option<UiFault> Holds(BindingKey key) => state.Value.Current.Find(key);

    public Transition<BindLedgerState> Reject(BindingKey key, UiFault fault) => Cell.Commit(
        state,
        held => {
            BindLedgerEntry entry = new(Ordinal: held.Next + 1L, Key: key, Fault: fault);
            Seq<BindLedgerEntry> history = held.History.Add(entry);
            return new BindLedgerState(
                Next: entry.Ordinal,
                History: history.Count > capacity.Value ? history.Skip(history.Count - capacity.Value) : history,
                Current: held.Current.AddOrUpdate(key, fault));
        });

    public Transition<BindLedgerState> Accept(BindingKey key) =>
        Cell.Commit(state, held => held with { Current = held.Current.Remove(key) });
}

public sealed class BindReceipt : IDisposable, IValidityEvidence {
    private readonly BindLedger ledger;
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());
    private readonly Atom<bool> released = Atom(false);
    private readonly Op key;

    internal BindReceipt(BindingKey identity, BindLedger ledger, Op key, Func<Fin<Unit>> refresh, Func<Fin<Unit>> unbind);

    public BindingKey Identity { get; }
    public Seq<Error> ReleaseFaults => teardown.Value;

    public bool IsValid => ValidityClaim.All(
        released.Value is false,
        ledger.Holds(Identity).IsNone);

    public Fin<Unit> Refresh();
    public Fin<Unit> Release();
    public void Dispose() => ignore(Release());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DataScope {
    [BoundaryAdapter] public static Fin<Unit> Assign(IBindable root, object model, Op? key = null);
}
```

## [06]-[STORE]

- Owner: `StoreRow<T>` the collection carrier family; `StoreItemLens` the list display and key projections; `TreeStore<T>` the tree carrier with its element projection; `StoreSink<T>` the closed mount destination family, every case carrying its own store; `StoreRail` the one mount gate.
- Cases: `StoreRow` is `Eager` over a fully materialized observable source whose mutations refresh the bound view, or `Virtual` over a random-access window contract adapted at mount. `StoreSink<T>` is `Grid` and `List` each carrying their `StoreRow<T>`, or `Tree` carrying its `TreeStore<T>` — the tree's item contract, not the element type, is what discriminates it.
- Entry: `StoreRail.Mount(sink, key)` is the ONE gate; every case carries the store it mounts, so the gate takes no second carrier.
- Law: the store rides its CASE. NAMED LOSS: the mount's `Option<StoreRow<T>>` arity, whose tree case demanded absence and whose two other cases demanded presence — a pairing every call site had to know and no signature stated. The tree-with-rows and the grid-without-rows corners are now unrepresentable rather than refusable. Witness: `StoreRail.Mount(new StoreSink.Tree(view, store), Some(rows), key)` no longer compiles, where before it type-checked and refused at runtime.
- Law: `TreeStore<T>` carries its element projection beside the host store, so the type parameter is RECOVERABLE — a selection read off a tree view answers `Option<T>` rather than the `ITreeGridItem` the host sink erased it to. A carrier branded by a parameter nothing reads is the decorative form this column forecloses.
- Auto: both carriers project through one `Carrier` dispatch onto the enumerable the host view demands, so a virtualized window never reaches a view unadapted and an enumerable source is never wrapped in an adapter its view already accepts.
- Law: mutation flows through the mounted collection and the view refreshes from the collection change — rebuilding a control per row is the deleted form. A snapshot source that never mutates still mounts `Eager`, because the CARRIER is the contract and the mutation rate is not.
- Law: mounting crosses the marshal, and a background producer feeds the collection through that same crossing, because a mounted carrier is UI-affine state.
- Law: the item lens carries display text and key as binding VALUES, so a list's projection travels as data on the mount rather than as a per-view subclass.
- Packages: Eto.Forms for `DataStoreCollection<T>`, `DataStoreVirtualCollection<T>`, `IDataStore<T>`, `ITreeGridStore<T>`, `ITreeGridItem`, and the three `DataStore` sinks (verified in `libs/dotnet/.api/api-eto-binding.md`).
- Growth: a new sink is one case with one mount arm; a new carrier is one case with one `Carrier` arm.
- Boundary: Rhino mounted data through its grid plan alone and carried no list or tree store mount and no virtual carrier; all three gaps close here, and its boundary edit is a deletion.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Generic;
using Eto.Forms;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreRow<T> where T : class {
    private StoreRow() { }

    public sealed record Eager(DataStoreCollection<T> Rows) : StoreRow<T>;
    public sealed record Virtual(IDataStore<T> Window) : StoreRow<T>;

    internal IEnumerable<object> Carrier() => Switch(
        eager: static row => (IEnumerable<object>)row.Rows,
        @virtual: static row => new DataStoreVirtualCollection<T>(store: row.Window));
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StoreRail {
    [BoundaryAdapter]
    public static Fin<Unit> Mount<T>(StoreSink<T> sink, Op? key = null) where T : class;
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

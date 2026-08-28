# [RASM_BINDING]

`Rasm.Interaction` owns the one control-to-model fusion. Every binding is a PLAN — a keyed value carrying its source shape, its propagation direction, its write timing, and its optional admission conversion — that rigs onto a live control and answers a leased link owning refresh and exact unbind. `BindingPlan.Create` accumulates every violated source, flow, and timing condition at admission, so no rigged plan can hold a combination the host binding machinery cannot honour.

Both host boundaries built this twice. Rhino carried the atom-state arm, the timing axis, the legality conditions, commit-on-focus-loss, and the keyed failure ledger; Grasshopper carried the model projection, typed conversion with its refusal posture, ambient context assignment, and collection-store mount. Neither carried the other's half. This owner is the union: every axis takes the richer side's shape and the poorer side gains it, and both boundaries compose this page with no adapter.

Composition is downward: `Lease<T>`, `Atom`, `Transition<TState>`, `Cell`, and `ValidityClaim` come from `Domain/results`; `PositiveMagnitude` and `Dimension` from `Numerics/atoms`; `UiFault`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`. Every touch of the control tree crosses `UiThread` on the immediate lane, so this owner holds the same affinity contract as every other surface on the sub-domain rather than standing as its unmarshalled exception. Each crossing is a `Blocking` case, whose static type selects the SYNCHRONOUS marshal arity — the rig composes inside its own `Fin` query and blocks on nothing, because a task-shaped return over an in-frame crossing forces every binding site to wait on a result it already holds.

## [01]-[INDEX]

- [02]-[FLOW]: `BindingKey`, `FlowMode`, `Cadence` — the fusion identity, propagation direction, and write timing.
- [03]-[SOURCE]: `Lens<TState,TValue>`, `StateCell<TState>`, `BindSource<TValue>` — the model side, from an atom lens to a seed or data-context path.
- [04]-[GATE]: `GatePolicy<TModel>`, `BindingPlan` — the refused-write posture, direct admission conditions, and the rig.
- [05]-[LEDGER]: `Dimension`, `BindLedgerEntry`, `BindLedger`, `BindLink`, `DataScope` — the admitted history bound, keyed current failure, leased link, and ambient model assignment.
- [06]-[STORE]: `StoreSource<T>`, `StoreSink<T>`, `StoreGate` — the collection sources and the one mount gate over grid, list, and tree.

## [02]-[FLOW]

- Owner: `BindingKey` the fusion identity every ledger row, refusal, and link is addressed by; `FlowMode` the propagation row deriving both host directions from one admitted host mode; `Cadence` the write-timing family.
- Cases: `Cadence` is `Edit` (every keystroke propagates), `Commit` (writes on focus loss), `Throttled` (delays without restarting), and `Debounced` (restarts the delay on every input). The host exposes the last two as one call with a reset flag; the two names make which behaviour a plan chose recoverable from the value.
- Auto: a debounce window admits as `PositiveMagnitude`, so a zero or negative window is UNREPRESENTABLE rather than a legality arm. The Rhino form carried a raw `TimeSpan` and paid for it with a `Window.Ticks <= 0` guard inside the legality ladder; that arm has no place to live here.
- Law: `FlowMode` carries `DualBindingMode` and derives `BindingUpdateMode`, `ToSource`, and `ToControl` from that one host value, so no stored relay column can disagree with its direction.
- Law: `Manual` is a row, not the absence of one. A link the caller pushes explicitly is a declared flow whose timing axis is fixed at `Edit`, and direct admission refuses every other timing against it.
- Growth: a new timing is one `Cadence` case and one admission condition where its host support differs; a new propagation is one `FlowMode` row and its direct admission conditions.
- Law: `FlowMode` is keyless because the generated case identity already admits the host mode and no consumer projects a second key.
- Boundary: the two host enums appear on this row set and nowhere else on the sub-domain — every interior consumer reads `FlowMode`, never `DualBindingMode`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Cadence {
    private Cadence() { }

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
}
```

## [03]-[SOURCE]

- Owner: `LanguageExt.Lens<TState,TValue>` the package-owned composable read-write pair over one state shape; `StateCell<TState>` the bridge from a kernel `Atom` into a host direct binding; `BindSource<TValue>` the closed model-side family covering an atom channel, a seed value, and an admitted context path.
- Cases: `FromState` carries a channel a `StateCell` minted, `FromValue` a one-time seed, and `FromContext` an already-lowered context path.
- Entry: callers construct the three cases directly from `StateCell.Channel`, a seed, or an Eto accessor produced by `Binding.Property`, `Binding.Delegate`, or `IndirectBinding.Child`.
- Auto: `Lower` projects only the data-context case to an `IndirectBinding<TValue>` and answers absence for the direct channel and one-time seed.
- Law: every atom transition returns a `Transition<TState>` verdict. `Mutate` is a public unmarshalled entry, so a compute lane may drive it; the marshal sits in the change ADAPTER, where the host handler is raised, and the mutation itself carries no affinity requirement.
- Law: the cell carries its OWN `FaultCell`, because both host hooks it wires are `void`. A `void` delegate licenses no discard — a refused write and a raised change notification are facts that vanish otherwise — so the setter's verdict and the adapter's crossing both PARK, and `Faults` is where a consumer reads what the gate could not carry outward. All four transition arms are spelled, so no case reaches `ignore`.
- Law: every compare-and-swap on this page takes the kernel default `Cell.SwapBudget` (branch RULINGS `[02]`). This page measures no lane whose contention differs from the kernel's, so a page-local budget shell would be an unanchored second ceiling.
- Law: the lens write executes INSIDE the compare-and-swap, so `Set` stays pure — the swap retries under contention and a setter carrying an effect runs that effect once per attempt.
- Law: the change-adapter map is keyed on the host handler the binding machinery hands in and removal reads the stored adapter, so an add and a remove of one handler cancel exactly. A re-derived adapter closure compares unequal and leaves the atom subscribed to a released control.
- Growth: a new host-distinct source is one case with one `Lower` arm; accessor construction and child drilling remain on Eto's own surface.
- Exemption: the change-adapter map is a `ConcurrentDictionary` that MUTATES for the life of the cell — the host binding machinery adds and removes handlers from the marshal while a compute lane may still be swapping the atom, so neither a frozen table nor a caller-side keyed map serves. It is the one mutable registry on this page and it is owned by the cell that mutates it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Concurrent;
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

    internal Option<IndirectBinding<TValue>> Lower() => Switch(
        fromState: static _ => None,
        fromValue: static _ => None,
        fromContext: static source => Some(source.Path));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class StateCell<TState>(Atom<TState> state, FaultCell faults) {
    private static readonly HookId Point = HookId.Create(value: "rasm.kernel.interaction.binding.state");

    public TState Current => state.Value;
    public Seq<IsolatedFault> Faults => faults.Parked;

    public Transition<TState> Mutate(Func<TState, TState> transition) => Cell.Commit(state, transition);

    public DirectBinding<TValue> Channel<TValue>(Lens<TState, TValue> lens) {
        ConcurrentDictionary<EventHandler<EventArgs>, AtomChangedEvent<TState>> adapters = new();
        return Binding.Delegate<TValue>(
            getValue: () => lens.Get(state.Value),
            setValue: value => Park(Cell.Commit(state, held => lens.Set(value, held))),
            addChangeEvent: handler => {
                AtomChangedEvent<TState> adapter = _ => Park(UiThread.Run(
                    new UiDispatch<Unit>.Blocking(() => Try.lift(() => handler(this, EventArgs.Empty)).Run().Bind(static inner => inner)),
                    DispatchLane.Immediate));
                if (adapters.TryAdd(handler, adapter)) { state.Change += adapter; }
            },
            removeChangeEvent: handler => {
                if (adapters.TryRemove(handler, out AtomChangedEvent<TState>? adapter)) { state.Change -= adapter; }
            });
    }

    private Unit Park(Transition<TState> verdict) => verdict.Switch(
        committed: static _ => unit,
        ceded: static _ => unit,
        refused: (row) => ignore(faults.Park(point: Point, cause: row.Cause)),
        contended: (key) => ignore(faults.Park(point: Point, cause: new KernelFault.InvalidResult())));

    private Unit Park(Fin<Unit> crossing) => crossing.Match(
        Succ: static _ => unit, Fail: cause => ignore(faults.Park(point: Point, cause: cause)));
}
```

## [04]-[GATE]

- Owner: `GatePolicy<TModel>` the refused-write posture; `BindingPlan<TControl,TValue,TModel>` the admitted riggable plan carrying its conversion delegates directly.
- Cases: `GatePolicy` is `Hold` — the control snaps back to the last admitted model value — or `Fallback` carrying an ALREADY-ADMITTED substitute. The Grasshopper form carried a raw substitute and re-admitted it inside the refusal path, so a fallback could itself refuse and the fold had nothing left but a default; carrying the model value forecloses that.
- Entry: `BindingPlan.Create` accumulates every absent required dependency and optional conversion component through `Validation`, accumulates all five source-flow-timing refusals, and stores only a legal plan; `Rig` selects the control binding, wires the source, attaches commit timing when requested, and answers a leased link.
- Auto: the five independent conditions match directly on the source case, flow row, and timing case, so admission reports every violated condition rather than the first and never materializes a mirrored product or cross-product cache.
- Law: a `(source, flow, timing)` triple that violates any condition is unrepresentable in a rigged plan.
- Law: each refusal carries the corresponding `RejectReason` singleton in `UiFault.Rejected`, so recovery matches the value and presentation reads its `Requirement` without parsing prose.
- Law: the gate is where the branch's admission bridge meets the host binding machinery — a `[ValueObject<T>]` field binds through `Render`/`Admit` and a `[SmartEnum<TKey>]` binds its key through the row's generated validation. A parse-on-commit handler beside the control is the deleted form.
- Law: the host conversion demands totality on the to-model direction, so `Admit` and the policy fold into ONE total function and the refusal lands on the ledger rather than escaping. The exception trap catches a host-thrown conversion fault and records it, because an uncaught cast escaping a binding into the event pump is the defect this trap forecloses.
- Law: `TValue` and `TModel` stay distinct parameters and the ungated fusion is the case where they coincide, so one plan serves the primitive and admitted forms; the primitive form carries `None` for its conversion.
- Law: commit timing lets Eto's live `DualBinding<T>` retain the pending control value: the effective host mode suppresses source writes during editing, and one exact `LostFocus` handler calls `Update(BindingUpdateMode.Source)` before unbind removes that handler.
- Output: `Fin<Lease<BindLink>>` — the link's lifetime is the caller's custody and its key addresses its own ledger row.
- Packages: Eto.Forms for `BindableBinding`, `DualBinding`, `Convert`, `CatchException`, and `BindDataContext` (verified in `libs/dotnet/.api/api-eto-binding.md`); LanguageExt.Core for `Validation`, `Fin`, `Atom`, and `Lease`.
- Growth: a new refusal posture is one `GatePolicy` case; a new admission clause is one independent validation expression beside its `RejectReason` singleton.
- Boundary: host binding construction, cadence attach, rollback, and unbind are the binding-provider statement form, and all four cross `UiThread` on the immediate lane.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [SERVICES] ------------------------------------------------------------------------
public interface IBindingPlan {
    BindingKey Key { get; }
    Fin<Lease<BindLink>> Rig(Control control);
}

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

    public static Fin<BindingPlan<TControl, TValue, TModel>> Create(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TModel> source,
        FlowMode mode,
        Cadence timing,
        Option<(Func<TModel, TValue> Render, Func<TValue, Fin<TModel>> Admit, GatePolicy<TModel> Policy)> conversion,
        BindLedger ledger) {
        return (Admit.Need(select).ToValidation(), Admit.Need(source).ToValidation(),
                Admit.Need(mode).ToValidation(), Admit.Need(timing).ToValidation(),
                Admit.Need(ledger).ToValidation(),
                conversion.Traverse(static held =>
                    (Admit.Need(held.Render).ToValidation(), Admit.Need(held.Admit).ToValidation(), Admit.Need(held.Policy).ToValidation())
                        .Apply(static (render, admit, policy) => (Render: render, Admit: admit, Policy: policy))).As())
            .Apply(static (chosen, admitted, flow, cadence, book, converted) =>
                (Select: chosen, Source: admitted, Mode: flow, Timing: cadence, Ledger: book, Conversion: converted))
            .As()
            .ToFin()
            .Bind(held => (
                (held.Mode == FlowMode.Seed) == (held.Source is BindSource<TModel>.FromValue) ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.SeedFlow)),
                held.Source is not BindSource<TModel>.FromValue || held.Timing is Cadence.Edit ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.SeedTiming)),
                held.Timing is not (Cadence.Throttled or Cadence.Debounced) || held.Source is BindSource<TModel>.FromContext && held.Mode.ToControl ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.DelayedPath)),
                held.Timing is not Cadence.Commit || held.Mode.ToSource ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.CommitFlow)),
                held.Mode != FlowMode.Manual || held.Timing is Cadence.Edit ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new UiFault.Rejected(FieldTag.Create(key.Value), RejectReason.ManualTiming)))
                .Apply((_, _, _, _, _) => new BindingPlan<TControl, TValue, TModel>(
                    key, held.Select, held.Source, held.Mode, held.Timing, held.Conversion, held.Ledger))
                .As().ToFin());
    }

    public Fin<Lease<BindLink>> Rig(Control control) =>
        from typed in control is TControl accepted
            ? Fin.Succ(accepted)
            : Fin.Fail<TControl>(new UiFault.Rejected(Field: FieldTag.Create(value: Key.Value), Reason: RejectReason.ControlType))
        from link in UiThread.Run(
            new UiDispatch<BindLink>.Blocking(() => Try.lift(() => Wire(typed)).Run().Bind(static inner => inner)),
            DispatchLane.Immediate)
        select (Lease<BindLink>)new Lease<BindLink>.Owned(link);

    private Fin<BindLink> Wire(TControl control);
}

```

## [05]-[LEDGER]

- Owner: `Dimension` the admitted history bound; `BindLedgerEntry` one recorded refusal; `BindLedger` the keyed current-failure map beside bounded history; `BindLink` the leased link with refresh and exact unbind; `DataScope` the ambient model assignment.
- Entry: `BindLedger` accepts an admitted `Dimension` directly; `Reject` and `Accept` are the two ledger transitions; `Holds` reads a key's current refusal; `Refresh` and `Release` are the link's lifecycle; `DataScope.Assign` seats a model on a bindable root.
- Auto: current failure and history are INDEPENDENT. History truncates at capacity; the current map never prunes, so a fusion that failed and stayed failed is still refusing after its entry ages out. One bounded log would silently declare a broken binding valid at capacity.
- Auto: every ledger mutation answers a `Transition<BindLedgerState>` verdict, so a caller that must know its record landed reads the case rather than assuming a swap.
- Law: `IsValid` is the ruled evidence fold — an unreleased link with no current refusal under its EXACT key. A field-only key lets two fusions on one control share a rejection state, which is precisely the failure the identity value object exists to prevent.
- Law: the release one-shot is an `Atom<bool>` seated through a guarded transition, so a second release reads a REFUSED verdict rather than no-opping into silence; a hand interlocked integer beside this page's `Atom`/`Cell`/`Transition` custody is the deleted form.
- Law: refusal routing rides `Release`, so the one non-throwing terminal every capsule on this sub-domain shares stays unmodified and this link still records its keyed rejection without minting a second terminal.
- Law: a teardown fault lands on the link's own ledger, never on the unwinding stack — disposal fires from a `finally` and from a `using` unwind, where a raise REPLACES the primary exception with a teardown fault.
- Law: `DataScope.Assign` is the ONE ambient-model boundary. Assignment on a container propagates to every bound descendant, which is what makes per-control source wiring the deleted form; it crosses the marshal because propagation raises host change events across the whole subtree.
- Output: `BindLink` carries its key, its validity fold, its release faults, and a `Refresh` pushing in the flow row's declared update direction.
- Growth: a new evidence column extends `BindLedgerEntry`; retention and current failure stay independent.
- Boundary: control realization retains links and releases them in reverse tree order, so a partially rigged subtree unwinds exactly what it wired.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BindLedgerEntry(long Ordinal, BindingKey Key, UiFault Fault);

internal sealed record BindLedgerState(long Next, Seq<BindLedgerEntry> History, HashMap<BindingKey, UiFault> Current);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class BindLedger {
    private readonly Dimension capacity;
    private readonly Atom<BindLedgerState> state;

    public BindLedger(Dimension capacity) {
        this.capacity = capacity;
        state = Atom(new BindLedgerState(Next: 0L, History: Seq<BindLedgerEntry>(), Current: HashMap<BindingKey, UiFault>()));
    }

    public Seq<BindLedgerEntry> History => state.Value.History;
    internal Option<UiFault> Holds(BindingKey key) => state.Value.Current.Find(key);

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
}

public sealed class BindLink : IDisposable, IValidityEvidence {
    private readonly BindLedger ledger;
    private readonly Atom<Seq<Error>> teardown = Atom(Seq<Error>());
    private readonly Atom<bool> released = Atom(false);

    internal BindLink(BindingKey key, BindLedger ledger, Func<Fin<Unit>> refresh, Func<Fin<Unit>> unbind);

    public BindingKey Key { get; }
    public Seq<Error> ReleaseFaults => teardown.Value;

    public bool IsValid => ValidityClaim.All(
        released.Value is false,
        ledger.Holds(Key).IsNone);

    public Fin<Unit> Refresh();
    public Fin<Unit> Release();
    public void Dispose() => ignore(Release());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

## [06]-[STORE]

- Owner: `StoreSource<T>` the enumerable collection-source family; `StoreSink<T>` the closed mount-destination family whose cases carry their sources and projections directly; `StoreGate` the one mount gate.
- Cases: `StoreSource<T>` is `Eager` over a fully materialized observable source whose mutations refresh the bound view, or `Virtual` over a random-access window contract adapted at mount. `StoreSink<T>` is `Grid` and `List` over an enumerable source, or `Tree` over Eto's native tree-store contract.
- Entry: `StoreGate.Mount(sink)` is the ONE gate; every case carries the store it mounts, so the gate takes no second carrier.
- Law: the store rides its CASE, so a tree cannot carry an enumerable source and a grid or list cannot omit one.
- Law: the tree case carries its element projection beside the host store, so the type parameter is RECOVERABLE — a selection read off a tree view answers `Option<T>` rather than the `ITreeGridItem` the host sink erased it to.
- Auto: both carriers project through one `Carrier` dispatch onto the enumerable the host view demands, so a virtualized window never reaches a view unadapted and an enumerable source is never wrapped in an adapter its view already accepts.
- Law: mutation flows through the mounted collection and the view refreshes from the collection change — rebuilding a control per row is the deleted form. A snapshot source that never mutates still mounts `Eager`, because the CARRIER is the contract and the mutation rate is not.
- Law: mounting crosses the marshal, and a background producer feeds the collection through that same crossing, because a mounted carrier is UI-affine state.
- Law: the list case carries display text and key as binding VALUES, so its projections travel as data on the mount rather than as a per-view subclass.
- Packages: Eto.Forms for `DataStoreCollection<T>`, `DataStoreVirtualCollection<T>`, `IDataStore<T>`, `ITreeGridStore<T>`, `ITreeGridItem`, and the three `DataStore` sinks (verified in `libs/dotnet/.api/api-eto-binding.md`).
- Growth: a new sink is one case with one mount arm; a new carrier is one case with one `Carrier` arm.
- Boundary: Rhino mounted data through its grid plan alone and carried no list or tree store mount and no virtual carrier; all three gaps close here, and its boundary edit is a deletion.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Generic;
using Eto.Forms;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreSource<T> where T : class {
    private StoreSource() { }

    public sealed record Eager(DataStoreCollection<T> Rows) : StoreSource<T>;
    public sealed record Virtual(IDataStore<T> Window) : StoreSource<T>;

    internal IEnumerable<object> Carrier() => Switch(
        eager: static row => (IEnumerable<object>)row.Rows,
        @virtual: static row => new DataStoreVirtualCollection<T>(store: row.Window));
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
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

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

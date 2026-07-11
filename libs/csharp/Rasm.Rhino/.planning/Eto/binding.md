# [RASM_RHINO_ETO_BINDING]

The data-binding owner of `Rasm.Rhino.Eto` — typed bind rows over the host `IndirectBinding`/`DirectBinding`/`BindableBinding`/`DualBinding` surface, with admission, conversion, child paths, delayed propagation, and validation cadence as row columns. The census carried zero binding usage and synchronized every control by hand-wired change events; this owner deletes that plumbing: a value channel is one `Bind.Rig` row rigging a control's own `BindableBinding` selector against one of three sources — an `Atom`-backed state cell, a `DataContext` property path, or a one-time seed — and every write crosses ONE admission gate that composes the kernel Domain rails, so a rejected value never reaches state and every rejection is a ledgered `UiFault.Rejected`. `BindAttachment` is the erased row `elements.md` specs carry, and `BindReceipt` is the typed evidence one wired channel returns; the receipt's `Unbind` and `Refresh` are the only lifecycle verbs a consumer holds.

## [01]-[INDEX]

- [02]-[MODE_AND_CADENCE]: `FlowMode` — the propagation-direction rows over the host `DualBindingMode` seam — and `Cadence` — the validation-cadence union (edit, debounced, commit) with its applicability law.
- [03]-[STATE_CELL]: `StateCell<TState>` + `Lens<TState, TValue>` — the `Atom`-backed source: one change-adapter registry bridges the kernel cell into a host `DirectBinding<TValue>` through `Binding.Delegate`.
- [04]-[BIND_ROWS]: `BindSource<TValue>` + `Bind.Rig` — the one rigging entry producing the erased `BindAttachment`; admission is woven inside the source channel so no unadmitted value ever lands.
- [05]-[LEDGER]: `BindReceipt` + `BindLedger` — typed wiring evidence, the rejection ledger, and the `IValidityEvidence` fold over a screen's wired channels.

## [02]-[MODE_AND_CADENCE]

- Owner: `FlowMode` `[SmartEnum<int>]` — four rows carrying the host `DualBindingMode` as a column, so direction is a value and the host enum never crosses this seam outward — and `Cadence`, the closed `[Union]` of validation timing: `OnEdit` admits on every change, `Debounced(TimeSpan, bool Reset)` admits after the host `AfterDelay` window, `OnCommit` buffers the last raw value and admits on `LostFocus`.
- Law: cadence applicability is a row fact — `Debounced` rides `IndirectBinding<T>.AfterDelay` and binds only where the channel is context-shaped; a state-cell row carries `OnEdit` or `OnCommit`, and `Bind.Rig` rejects the impossible pairing at rig time with a typed `UiFault.Rejected`, never a silent downgrade.
- Cases: `FlowMode` `Both(TwoWay)` · `IntoControl(OneWay)` · `IntoSource(OneWayToSource)` · `Seed(OneTime)`; `Cadence` `OnEdit` · `Debounced(TimeSpan, bool)` · `OnCommit`.
- Growth: a new host propagation mode is one `FlowMode` row; a new cadence is one case breaking `Bind.Rig` at compile time.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FlowMode {
    public static readonly FlowMode Both = new(key: 0, row: DualBindingMode.TwoWay);
    public static readonly FlowMode IntoControl = new(key: 1, row: DualBindingMode.OneWay);
    public static readonly FlowMode IntoSource = new(key: 2, row: DualBindingMode.OneWayToSource);
    public static readonly FlowMode Seed = new(key: 3, row: DualBindingMode.OneTime);
    internal DualBindingMode Row { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Cadence {
    private Cadence() { }
    public sealed record OnEdit : Cadence;
    public sealed record Debounced(TimeSpan Window, bool Reset = false) : Cadence;
    public sealed record OnCommit : Cadence;
    public static readonly Cadence Edit = new OnEdit();
    public static readonly Cadence Commit = new OnCommit();
    public static Cadence After(TimeSpan window, bool reset = false) => new Debounced(Window: window, Reset: reset);
}
```

## [03]-[STATE_CELL]

- Owner: `StateCell<TState>` — the one bridge from a kernel `Atom<TState>` into host binding: `Lens<TState, TValue>` is the get/put pair addressing one value inside the state record, and `Channel(lens)` mints a `DirectBinding<TValue>` through the verified `Binding.Delegate<TValue>(getValue, setValue, addChangeEvent, removeChangeEvent)` factory, with one adapter registry mapping host `EventHandler<EventArgs>` subscriptions onto the atom's `Change` edge so unsubscribe releases the exact adapter subscribe registered. The census-era pattern — every control mutating the atom in its own event handler, every atom change re-poking controls by hand — collapses into this one bridge.
- Law: the cell owns writes — `Put` runs inside `Swap`, so a lens write is one CAS transition and re-entrant change delivery observes only committed state; a lens whose `Put` closes over anything but its arguments is the rejected form because `Swap` re-runs under contention.
- Law: one `StateCell` per screen state record, minted where the screen composes; two cells over one record fork the change edge and are the deleted form.
- Packages: LanguageExt.Core (`Atom`, `AtomChangedEvent`, `Fin`), Eto.Forms (host — `Binding.Delegate`, `DirectBinding<T>`), Rasm.Domain (project — `Op`).
- Growth: a derived read (a projection two lenses combine) is one `Lens.Of` composition; a second change-notification mechanism is the deleted form.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Lens<TState, TValue>(Func<TState, TValue> Get, Func<TState, TValue, TState> Put) {
    public static Lens<TState, TValue> Of(Func<TState, TValue> get, Func<TState, TValue, TState> put) => new(Get: get, Put: put);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class StateCell<TState>(Atom<TState> cell) {
    private readonly System.Collections.Concurrent.ConcurrentDictionary<EventHandler<EventArgs>, AtomChangedEvent<TState>> adapters = new();

    public TState Current => cell.Value;

    public Unit Mutate(Func<TState, TState> transition) => ignore(cell.Swap(transition));

    public DirectBinding<TValue> Channel<TValue>(Lens<TState, TValue> lens) =>
        Binding.Delegate<TValue>(
            getValue: () => lens.Get(cell.Value),
            setValue: value => ignore(cell.Swap(state => lens.Put(state, value))),
            addChangeEvent: handler => {
                AtomChangedEvent<TState> adapter = _ => handler(this, EventArgs.Empty);
                _ = adapters.TryAdd(handler, adapter);
                cell.Change += adapter;
            },
            removeChangeEvent: handler => Op.SideWhen(adapters.TryRemove(handler, out AtomChangedEvent<TState>? adapter), () => cell.Change -= adapter!));
}
```

## [04]-[BIND_ROWS]

- Owner: `BindSource<TValue>` — the closed source union (`FromState` a pre-minted `DirectBinding` channel, `FromContext` an `IndirectBinding` property path off `DataContext`, `FromValue` a one-time seed) — and `Bind.Rig`, the ONE rigging entry producing the erased `BindAttachment` every `ElementSpec` carries; the row IS the `Rig` signature (field, selector, source, mode, cadence, admission, ledger), so no parallel plan record exists beside the entry. The selector is the control's own verified `*Binding` property (`TextBinding`, `CheckedBinding`, `ValueBinding`, `SelectedIndexBinding`, `SelectedItemBinding` and kin), so the channel inherits the host's change-event wiring and no event name is ever spelled at a call site.
- Law: admission is woven inside the source channel, never a post-hoc check — the state arm runs the gate in the write path of the verified `Convert(getValue, setValue)` overload writing back through the captured channel's `DataValue`, and the context arm re-mints the path through `Binding.Delegate<object, TValue>` over the verified `GetValue`/`SetValue` pair; a `Fin.Fail` keeps the last admitted value, ledgers `UiFault.Rejected(op, field, reason)`, and never writes — so state holds only admitted values by construction, composing the kernel rails as the validation vocabulary.
- Law: `Rig` is where the impossible pairings die — a `Debounced` cadence over a state source, a `Seed` source with an admission gate, a control whose runtime type refuses the selector — each is a typed rig-time rejection; the erased `Wire` runs on the UI thread the caller already holds and returns `Fin<BindReceipt>`.
- Entry: `Bind.Rig<TControl, TValue>(field, selector, source, mode?, cadence?, admit?, ledger?)` → `BindAttachment`; `Bind.Context<TContext, TValue>(expression)` mints the context source over the verified `Binding.Property` expression factory; `ContextScope.Assign(control, context)` is the one `DataContext` write with `UpdateBindings` refresh.
- Boundary: each source's own verified `CatchException` is the host's last-resort funnel for conversion throws inside host code — rigged once on the gated channel so a host-side cast failure surfaces as a ledgered fault, never a crash; the drag/clipboard payload channel is `runtime.md`'s, and command enablement is `chrome.md`'s — neither re-enters this seam.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindSource<TValue> {
    private BindSource() { }
    public sealed record FromState(DirectBinding<TValue> Channel) : BindSource<TValue>;
    public sealed record FromContext(IndirectBinding<TValue> Path) : BindSource<TValue>;
    public sealed record FromValue(TValue Seed) : BindSource<TValue>;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BindAttachment(string Field, Func<Control, Fin<BindReceipt>> Wire);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Bind {
    public static BindSource<TValue> Context<TContext, TValue>(System.Linq.Expressions.Expression<Func<TContext, TValue>> path) =>
        new BindSource<TValue>.FromContext(Path: Binding.Property(propertyExpression: path));

    public static BindSource<TValue> State<TState, TValue>(StateCell<TState> cell, Lens<TState, TValue> lens) =>
        new BindSource<TValue>.FromState(Channel: cell.Channel(lens: lens));

    public static BindAttachment Rig<TControl, TValue>(
        string field,
        Func<TControl, BindableBinding<TControl, TValue>> selector,
        BindSource<TValue> source,
        FlowMode? mode = null,
        Cadence? cadence = null,
        Option<Func<TValue, Fin<TValue>>> admit = default,
        Option<BindLedger> ledger = default,
        Op? key = null) where TControl : Control {
        Op op = key.OrDefault();
        FlowMode flow = mode ?? FlowMode.Both;
        Cadence timing = cadence ?? Cadence.Edit;
        BindLedger book = ledger.IfNone(BindLedger.Shared);
        return new BindAttachment(Field: field, Wire: control =>
            from typed in control is TControl exact ? Fin.Succ(value: exact) : Fin.Fail<TControl>(error: new UiFault.Rejected(Key: op, Field: field, Reason: $"control is {control.GetType().Name}, channel expects {typeof(TControl).Name}"))
            from admitted in Legal(source: source, timing: timing, admit: admit, op: op, field: field)
            from receipt in op.Catch(() => Fin.Succ(value: Wired(control: typed, selector: selector, source: admitted, flow: flow, timing: timing, admit: admit, book: book, op: op, field: field)))
            select receipt);
    }

    private static Fin<BindSource<TValue>> Legal<TValue>(BindSource<TValue> source, Cadence timing, Option<Func<TValue, Fin<TValue>>> admit, Op op, string field) =>
        (source, timing) switch {
            (BindSource<TValue>.FromValue, _) when admit.IsSome => Fin.Fail<BindSource<TValue>>(error: new UiFault.Rejected(Key: op, Field: field, Reason: "a one-time seed admits nothing; bind a live source or drop the gate")),
            (BindSource<TValue>.FromState, Cadence.Debounced) => Fin.Fail<BindSource<TValue>>(error: new UiFault.Rejected(Key: op, Field: field, Reason: "debounce rides AfterDelay and binds only to a context path")),
            (BindSource<TValue>.FromContext context, Cadence.Debounced window) => Fin.Succ<BindSource<TValue>>(value: context with { Path = context.Path.AfterDelay(delay: window.Window, reset: window.Reset) }),
            _ => Fin.Succ(value: source),
        };

    private static BindReceipt Wired<TControl, TValue>(
        TControl control,
        Func<TControl, BindableBinding<TControl, TValue>> selector,
        BindSource<TValue> source,
        FlowMode flow,
        Cadence timing,
        Option<Func<TValue, Fin<TValue>>> admit,
        BindLedger book,
        Op op,
        string field) where TControl : Control {
        BindableBinding<TControl, TValue> channel = selector(control);
        DualBinding<TValue> dual = source.Switch(
            state: (Channel: channel, Flow: flow, Admit: admit, Book: book, Op: op, Field: field, Timing: timing, Control: (Control)control),
            fromState: static (held, from) => held.Channel.Bind(
                sourceBinding: Gated(held.Admit, from.Channel, held.Book, held.Op, held.Field, held.Timing, held.Control)
                    .CatchException(exceptionHandler: thrown => held.Book.Record(fault: new UiFault.HostRejected(Key: held.Op, Detail: thrown.Message))),
                mode: held.Flow.Row),
            fromContext: static (held, from) => held.Channel.BindDataContext(
                dataContextBinding: GatedPath(held.Admit, from.Path, held.Book, held.Op, held.Field)
                    .CatchException(exceptionHandler: thrown => held.Book.Record(fault: new UiFault.HostRejected(Key: held.Op, Detail: thrown.Message))),
                mode: held.Flow.Row),
            fromValue: static (held, from) => held.Channel.Bind(sourceBinding: Binding.Delegate<TValue>(getValue: () => from.Seed), mode: DualBindingMode.OneTime));
        return new BindReceipt(Field: field, Unbind: () => Op.Side(dual.Unbind), Refresh: () => Op.Side(() => dual.Update()));
    }

    private static DirectBinding<TValue> Gated<TValue>(Option<Func<TValue, Fin<TValue>>> admit, DirectBinding<TValue> channel, BindLedger book, Op op, string field, Cadence timing, Control control) =>
        admit.Match(
            Some: gate => timing switch {
                Cadence.OnCommit => Committed(gate: gate, channel: channel, book: book, op: op, field: field, control: control),
                _ => channel.Convert<TValue>(
                    getValue: static value => value,
                    setValue: (_, value) => ignore(gate(value).Match(
                        Succ: admitted => Op.Side(() => channel.DataValue = admitted),
                        Fail: fault => book.Record(fault: new UiFault.Rejected(Key: op, Field: field, Reason: fault.Message))))),
            },
            None: () => channel);

    private static IndirectBinding<TValue> GatedPath<TValue>(Option<Func<TValue, Fin<TValue>>> admit, IndirectBinding<TValue> path, BindLedger book, Op op, string field) =>
        admit.Match(
            Some: gate => Binding.Delegate<object, TValue>(
                getValue: item => path.GetValue(dataItem: item),
                setValue: (item, value) => ignore(gate(value).Match(
                    Succ: admitted => Op.Side(() => path.SetValue(dataItem: item, value: admitted)),
                    Fail: fault => book.Record(fault: new UiFault.Rejected(Key: op, Field: field, Reason: fault.Message))))),
            None: () => path);

    private static DirectBinding<TValue> Committed<TValue>(Func<TValue, Fin<TValue>> gate, DirectBinding<TValue> channel, BindLedger book, Op op, string field, Control control) {
        Atom<Option<TValue>> pending = Atom(Option<TValue>.None);
        control.LostFocus += (_, _) => {
            Option<TValue> buffered = pending.Value;
            _ = pending.Swap(static _ => Option<TValue>.None);
            _ = buffered.Iter(value => ignore(gate(value).Match(
                Succ: admitted => Op.Side(() => channel.DataValue = admitted),
                Fail: fault => book.Record(fault: new UiFault.Rejected(Key: op, Field: field, Reason: fault.Message)))));
        };
        return channel.Convert<TValue>(
            getValue: static value => value,
            setValue: (_, value) => ignore(pending.Swap(_ => Some(value))));
    }
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class ContextScope {
    public static Fin<Unit> Assign(Control root, object context, Op? key = null) =>
        key.OrDefault().Catch(() => {
            root.DataContext = context;
            root.UpdateBindings(mode: BindingUpdateMode.Destination);
            return Fin.Succ(value: unit);
        });
}
```

## [05]-[LEDGER]

- Owner: `BindReceipt` — the typed evidence of one wired channel (field, `Unbind`, `Refresh`) — and `BindLedger`, the one rejection stream: every admission refusal and host conversion throw lands as a `UiFault` fact in one `Atom<Seq<UiFault>>`, projected by pure folds. `Shared` is the process default; a screen that needs isolated forensics mints its own and passes it through `Rig`.
- Law: the ledger is evidence, never control flow — a consumer reads `Latest`/`ForField` to surface validity adorners and gate submits; acting inside `Record` is the deleted form because `Swap` re-runs under contention.
- Law: `BindReceipt.IsValid` registers with the kernel validity oracle through `IValidityEvidence`, so a screen's wired-channel health folds through the same `ValidityClaim.All` every kernel receipt uses — one validity mechanism, zero UI-local predicates.
- Growth: a new evidence axis (write count, last-admitted stamp) is one receipt field and one ledger fold; a per-screen fault array synced by hand is the deleted form.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BindReceipt(string Field, Func<Unit> Unbind, Func<Unit> Refresh) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Field)));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class BindLedger {
    public static readonly BindLedger Shared = new();
    private readonly Atom<Seq<UiFault>> faults = Atom(Seq<UiFault>());

    public Seq<UiFault> Latest => faults.Value;

    public Seq<UiFault> ForField(string field) =>
        faults.Value.Filter(fault => fault is UiFault.Rejected rejected && string.Equals(a: rejected.Field, b: field, comparisonType: StringComparison.Ordinal));

    public bool Record(UiFault fault) => (faults.Swap(held => held.Add(fault)), true).Item2;

    public Unit Clear() => ignore(faults.Swap(static _ => Seq<UiFault>()));
}
```

# [RASM_RHINO_ETO_BINDING]

The data-binding owner of `Rasm.Rhino.Eto` defines typed rows over the host `IndirectBinding`/`DirectBinding`/`BindableBinding`/`DualBinding` surface. One `Bind.Rig` row joins a control binding selector to an `Atom`-backed state cell, a `DataContext` path, or a one-time seed. Every live-source write crosses one admission rail, and rejected values remain outside source state as ledgered `UiFault.Rejected` evidence. `BindAttachment` is the erased row carried by element specs; the binding owner retains each returned `BindReceipt` under its control, and `Bind.Owned`, `Bind.Refresh`, and `Bind.Release` expose the complete lifecycle after element realization discards the immediate result.

## [01]-[INDEX]

- [02]-[MODE_AND_CADENCE]: `FlowMode` — the propagation-direction rows over the host `DualBindingMode` seam — and `Cadence` — the validation-cadence union (edit, debounced, commit) with its applicability law.
- [03]-[STATE_CELL]: `StateCell<TState>` + `Lens<TState, TValue>` — the `Atom` source bridge with subscription-local change adapters.
- [04]-[BIND_ROWS]: `BindSource<TValue>` + `Bind.Rig` — the one rigging entry, retained control scope, admission rail, and detachable cadence wiring.
- [05]-[LEDGER]: `BindReceipt` + `BindLedger` — idempotent wiring evidence and bounded current-validity history.

## [02]-[MODE_AND_CADENCE]

- Owner: `FlowMode` `[SmartEnum<int>]` carries four host `DualBindingMode` rows without exporting the host enum, and `Cadence` closes validation timing over `OnEdit`, `Debounced(TimeSpan, bool Reset)`, and `OnCommit`.
- Law: `OnEdit` admits every source write; `OnCommit` buffers the last raw value, admits it on `LostFocus`, and detaches that exact handler during unbind; `Debounced` uses `IndirectBinding<T>.AfterDelay` to coalesce context-source change delivery into the control without delaying writes into the source.
- Law: cadence applicability is a row fact — context rows admit every cadence, state rows admit `OnEdit` or `OnCommit`, and `Bind.Rig` rejects every impossible pairing with a typed `UiFault.Rejected`.
- Cases: `FlowMode` `Both(TwoWay)` · `IntoControl(OneWay)` · `IntoSource(OneWayToSource)` · `Seed(OneTime)`; `Cadence` `OnEdit` · `Debounced(TimeSpan, bool)` · `OnCommit`.
- Growth: a new host propagation mode is one `FlowMode` row; a new cadence is one case breaking `Bind.Rig` at compile time.

```csharp signature
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

- Owner: `StateCell<TState>` bridges one kernel `Atom<TState>` into host binding. `Lens<TState, TValue>` addresses one value inside the state record, and `Channel(lens)` mints a `DirectBinding<TValue>` through `Binding.Delegate<TValue>`. Each channel owns the adapter registry mapping host `EventHandler<EventArgs>` subscriptions onto the atom's `Change` edge, so handler identity cannot collide across channels and unsubscribe releases the exact adapter registered by subscribe.
- Law: the cell owns writes — `Put` runs inside `Swap`, so a lens write is one CAS transition and re-entrant change delivery observes only committed state; a lens whose `Put` closes over anything but its arguments is the rejected form because `Swap` re-runs under contention.
- Law: one `StateCell` per screen state record, minted where the screen composes; two cells over one record fork the change edge and are the deleted form.
- Packages: LanguageExt.Core (`Atom`, `AtomChangedEvent`, `Fin`), Eto.Forms (host — `Binding.Delegate`, `DirectBinding<T>`), Rasm.Domain (project — `Op`).
- Growth: a derived read (a projection two lenses combine) is one `Lens.Of` composition; a second change-notification mechanism is the deleted form.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Lens<TState, TValue>(Func<TState, TValue> Get, Func<TState, TValue, TState> Put) {
    public static Lens<TState, TValue> Of(Func<TState, TValue> get, Func<TState, TValue, TState> put) => new(Get: get, Put: put);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class StateCell<TState>(Atom<TState> cell) {
    public TState Current => cell.Value;

    public Unit Mutate(Func<TState, TState> transition) => ignore(cell.Swap(transition));

    public DirectBinding<TValue> Channel<TValue>(Lens<TState, TValue> lens) {
        System.Collections.Concurrent.ConcurrentDictionary<EventHandler<EventArgs>, AtomChangedEvent<TState>> adapters = new();
        return Binding.Delegate<TValue>(
            getValue: () => lens.Get(cell.Value),
            setValue: value => ignore(cell.Swap(state => lens.Put(state, value))),
            addChangeEvent: handler => {
                AtomChangedEvent<TState> adapter = _ => handler(this, EventArgs.Empty);
                _ = Op.SideWhen(adapters.TryAdd(handler, adapter), () => cell.Change += adapter);
            },
            removeChangeEvent: handler => Op.SideWhen(adapters.TryRemove(handler, out AtomChangedEvent<TState>? adapter), () => cell.Change -= adapter!));
    }
}
```

## [04]-[BIND_ROWS]

- Owner: `BindSource<TValue>` closes the source union over a pre-minted state `DirectBinding`, a `DataContext` `IndirectBinding` path, and a one-time seed. `Bind.Rig` is the single row-shaped entry producing the erased `BindAttachment` carried by `ElementSpec`; its selector targets the control's native `*Binding` property, so host change wiring remains inside Eto.
- Law: admission is woven inside every state and context write path. The identity admission rail applies when no custom gate exists, so cadence remains effective independently of validation. `Fin.Fail` preserves the last admitted source value and records `UiFault.Rejected`; `Fin.Succ` writes and clears the field's current rejection.
- Law: context adapters use the `DelegateBinding<object, TValue>` constructor and forward the opaque reference returned by `AddValueChangedHandler` into `RemoveValueChangedHandler`. Gating and commit buffering therefore preserve source-change refresh and exact subscription removal.
- Law: `Rig` is where the impossible pairings die — a `Debounced` cadence over a state source, a `Seed` source with an admission gate, a control whose runtime type refuses the selector — each is a typed rig-time rejection; the erased `Wire` runs on the UI thread the caller already holds and returns `Fin<BindReceipt>`.
- Entry: `Bind.Rig<TControl, TValue>(field, selector, source, mode?, cadence?, admit?, ledger?)` returns `BindAttachment`; `Bind.Context<TContext, TValue>(expression)` mints a context source; `Bind.Owned(control)`, `Bind.Refresh(control)`, and `Bind.Release(control)` expose retained receipts and whole-control lifecycle; `ContextScope.Assign(control, context)` assigns `DataContext` and refreshes destinations.
- Boundary: `CatchException` records only non-null host failures because Eto calls the handler with `null` after every completed set, including a semantic admission refusal. Admission success clears current rejection evidence inside the admission rail; host success does not overwrite that decision.

```csharp signature
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
    private sealed record Prepared<TBinding>(TBinding Binding, Func<Unit> Attach, Func<Unit> Detach);
    private sealed record Wiring<TValue>(DualBinding<TValue> Dual, Func<Unit> Detach);

    private sealed class BindScope {
        private readonly Atom<Seq<BindReceipt>> receipts = Atom(Seq<BindReceipt>());

        public Seq<BindReceipt> Receipts => receipts.Value;

        public BindReceipt Retain(BindReceipt receipt) {
            Seq<BindReceipt> displaced = receipts.Value.Filter(held => string.Equals(a: held.Field, b: receipt.Field, comparisonType: StringComparison.Ordinal));
            _ = receipts.Swap(held => held.Filter(bound => !string.Equals(a: bound.Field, b: receipt.Field, comparisonType: StringComparison.Ordinal)).Add(receipt));
            _ = displaced.Iter(static held => ignore(held.Unbind()));
            return receipt;
        }

        public Unit Refresh() => receipts.Value.Iter(static receipt => ignore(receipt.Refresh()));

        public Unit Release() {
            Seq<BindReceipt> held = receipts.Value;
            _ = receipts.Swap(static _ => Seq<BindReceipt>());
            return held.Iter(static receipt => ignore(receipt.Unbind()));
        }
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, BindScope> scopes = new();

    public static BindSource<TValue> Context<TContext, TValue>(System.Linq.Expressions.Expression<Func<TContext, TValue>> path) =>
        new BindSource<TValue>.FromContext(Path: Binding.Property(propertyExpression: path));

    public static BindSource<TValue> State<TState, TValue>(StateCell<TState> cell, Lens<TState, TValue> lens) =>
        new BindSource<TValue>.FromState(Channel: cell.Channel(lens: lens));

    public static Seq<BindReceipt> Owned(Control control) =>
        scopes.TryGetValue(control, out BindScope? scope) ? scope.Receipts : Seq<BindReceipt>();

    public static Unit Refresh(Control control) =>
        scopes.TryGetValue(control, out BindScope? scope) ? scope.Refresh() : unit;

    public static Unit Release(Control control) =>
        scopes.TryGetValue(control, out BindScope? scope)
            ? Op.Side(() => {
                _ = scopes.Remove(control);
                _ = scope.Release();
            })
            : unit;

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
            from receipt in op.Catch(() => Fin.Succ(value: Retain(control: typed, receipt: Wired(control: typed, selector: selector, source: admitted, flow: flow, timing: timing, admit: admit, book: book, op: op, field: field))))
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
        Wiring<TValue> wiring = source.Switch(
            state: (Channel: channel, Flow: flow, Admit: admit, Book: book, Op: op, Field: field, Timing: timing, Control: (Control)control),
            fromState: static (held, from) => {
                Prepared<DirectBinding<TValue>> prepared = Gated(admit: held.Admit, channel: from.Channel, book: held.Book, op: held.Op, field: held.Field, timing: held.Timing, control: held.Control);
                DirectBinding<TValue> guarded = prepared.Binding.CatchException(exceptionHandler: thrown =>
                    thrown is null || held.Book.Record(field: held.Field, fault: new UiFault.HostRejected(Key: held.Op, Detail: thrown.Message)));
                DualBinding<TValue> dual = held.Channel.Bind(sourceBinding: guarded, mode: held.Flow.Row);
                _ = prepared.Attach();
                return new Wiring<TValue>(Dual: dual, Detach: prepared.Detach);
            },
            fromContext: static (held, from) => {
                Prepared<IndirectBinding<TValue>> prepared = GatedPath(admit: held.Admit, path: from.Path, book: held.Book, op: held.Op, field: held.Field, timing: held.Timing, control: held.Control);
                IndirectBinding<TValue> guarded = prepared.Binding.CatchException(exceptionHandler: thrown =>
                    thrown is null || held.Book.Record(field: held.Field, fault: new UiFault.HostRejected(Key: held.Op, Detail: thrown.Message)));
                DualBinding<TValue> dual = held.Channel.BindDataContext(dataContextBinding: guarded, mode: held.Flow.Row);
                _ = prepared.Attach();
                return new Wiring<TValue>(Dual: dual, Detach: prepared.Detach);
            },
            fromValue: static (held, from) => new Wiring<TValue>(
                Dual: held.Channel.Bind(sourceBinding: Binding.Delegate<TValue>(getValue: () => from.Seed), mode: DualBindingMode.OneTime),
                Detach: static () => unit));
        return new BindReceipt(
            field: field,
            ledger: book,
            unbind: () => Op.Side(() => {
                _ = wiring.Detach();
                wiring.Dual.Unbind();
            }),
            refresh: () => Op.Side(() => wiring.Dual.Update()));
    }

    private static Prepared<DirectBinding<TValue>> Gated<TValue>(
        Option<Func<TValue, Fin<TValue>>> admit,
        DirectBinding<TValue> channel,
        BindLedger book,
        Op op,
        string field,
        Cadence timing,
        Control control) =>
        timing switch {
            Cadence.OnCommit => Committed(admit: admit, channel: channel, book: book, op: op, field: field, control: control),
            _ => new Prepared<DirectBinding<TValue>>(
                Binding: channel.Convert<TValue>(
                    getValue: static value => value,
                    setValue: (_, value) => ignore(Admit(admit: admit, value: value, write: admitted => channel.DataValue = admitted, book: book, op: op, field: field))),
                Attach: static () => unit,
                Detach: static () => unit),
        };

    private static Prepared<IndirectBinding<TValue>> GatedPath<TValue>(
        Option<Func<TValue, Fin<TValue>>> admit,
        IndirectBinding<TValue> path,
        BindLedger book,
        Op op,
        string field,
        Cadence timing,
        Control control) =>
        timing switch {
            Cadence.OnCommit => CommittedPath(admit: admit, path: path, book: book, op: op, field: field, control: control),
            _ => new Prepared<IndirectBinding<TValue>>(
                Binding: new DelegateBinding<object, TValue>(
                    getValue: item => path.GetValue(dataItem: item),
                    setValue: (item, value) => ignore(Admit(admit: admit, value: value, write: admitted => path.SetValue(dataItem: item, value: admitted), book: book, op: op, field: field)),
                    addChangeEvent: (item, handler) => {
                        return path.AddValueChangedHandler(dataItem: item, handler: handler);
                    },
                    removeChangeEvent: (reference, handler) => path.RemoveValueChangedHandler(bindingReference: reference, handler: handler)),
                Attach: static () => unit,
                Detach: static () => unit),
        };

    private static Prepared<DirectBinding<TValue>> Committed<TValue>(
        Option<Func<TValue, Fin<TValue>>> admit,
        DirectBinding<TValue> channel,
        BindLedger book,
        Op op,
        string field,
        Control control) {
        Atom<Option<TValue>> pending = Atom(Option<TValue>.None);
        EventHandler<EventArgs> commit = (_, _) => {
            Option<TValue> buffered = pending.Value;
            _ = pending.Swap(static _ => Option<TValue>.None);
            _ = buffered.Iter(value => ignore(Admit(admit: admit, value: value, write: admitted => channel.DataValue = admitted, book: book, op: op, field: field)));
        };
        return new Prepared<DirectBinding<TValue>>(
            Binding: channel.Convert<TValue>(
                getValue: static value => value,
                setValue: (_, value) => ignore(pending.Swap(_ => Some(value)))),
            Attach: () => Op.Side(() => control.LostFocus += commit),
            Detach: () => Op.Side(() => control.LostFocus -= commit));
    }

    private static Prepared<IndirectBinding<TValue>> CommittedPath<TValue>(
        Option<Func<TValue, Fin<TValue>>> admit,
        IndirectBinding<TValue> path,
        BindLedger book,
        Op op,
        string field,
        Control control) {
        Atom<Option<(object Item, TValue Value)>> pending = Atom(Option<(object Item, TValue Value)>.None);
        EventHandler<EventArgs> commit = (_, _) => {
            Option<(object Item, TValue Value)> buffered = pending.Value;
            _ = pending.Swap(static _ => Option<(object Item, TValue Value)>.None);
            _ = buffered.Iter(row => ignore(Admit(admit: admit, value: row.Value, write: admitted => path.SetValue(dataItem: row.Item, value: admitted), book: book, op: op, field: field)));
        };
        return new Prepared<IndirectBinding<TValue>>(
            Binding: new DelegateBinding<object, TValue>(
                getValue: item => path.GetValue(dataItem: item),
                setValue: (item, value) => ignore(pending.Swap(_ => Some((Item: item, Value: value)))),
                addChangeEvent: (item, handler) => {
                    return path.AddValueChangedHandler(dataItem: item, handler: handler);
                },
                removeChangeEvent: (reference, handler) => path.RemoveValueChangedHandler(bindingReference: reference, handler: handler)),
            Attach: () => Op.Side(() => control.LostFocus += commit),
            Detach: () => Op.Side(() => control.LostFocus -= commit));
    }

    private static Unit Admit<TValue>(
        Option<Func<TValue, Fin<TValue>>> admit,
        TValue value,
        Action<TValue> write,
        BindLedger book,
        Op op,
        string field) =>
        admit.Match(
            Some: gate => gate(value),
            None: () => Fin.Succ(value: value)).Match(
            Succ: admitted => Op.Side(() => {
                write(admitted);
                _ = book.Accept(field: field);
            }),
            Fail: fault => Op.Side(() => ignore(book.Record(field: field, fault: new UiFault.Rejected(Key: op, Field: field, Reason: fault.Message)))));

    private static BindReceipt Retain(Control control, BindReceipt receipt) =>
        scopes.GetValue(control, static _ => new BindScope()).Retain(receipt);
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

- Owner: `BindReceipt` is the idempotent lifecycle and validity evidence for one wired field. `BindLedger` stores a bounded ordinal history and one bounded current-failure row per field; `Shared` is the process default, and isolated screens pass their own ledger through `Rig`.
- Law: `Unbind` detaches cadence handlers and the host dual binding exactly once; `Refresh` becomes inert after release; `IsValid` requires a live receipt, a named field, and no current ledger failure for that field.
- Law: `Record(field, fault)` replaces the field's current failure and appends bounded history; `Accept(field)` clears only current failure, preserving forensic history; capacity prunes both projections at the owner boundary.
- Law: ledger mutation remains evidence-only. Every `Atom.Swap` transition is pure, and consumers read `Latest`, `Entries`, `ForField`, or receipt validity without callbacks inside the ledger.
- Growth: a new evidence axis is one `BindLedgerEntry` column and fold; unbounded fault arrays and UI-local validity predicates are rejected forms.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed class BindReceipt(string field, BindLedger ledger, Func<Unit> unbind, Func<Unit> refresh) : IDisposable, IValidityEvidence {
    private int released;

    public string Field { get; } = field;
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(
        holds: !string.IsNullOrWhiteSpace(value: Field) && Volatile.Read(location: ref released) == 0 && ledger.IsValid(field: Field)));

    public Unit Refresh() => Volatile.Read(location: ref released) == 0 ? refresh() : unit;

    public Unit Unbind() => Interlocked.Exchange(location1: ref released, value: 1) == 0 ? unbind() : unit;

    public void Dispose() => ignore(Unbind());
}

public sealed record BindLedgerEntry(long Ordinal, string Field, UiFault Fault);

internal sealed record BindLedgerState(long Next, Seq<BindLedgerEntry> History, Seq<BindLedgerEntry> Current);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class BindLedger {
    public static readonly BindLedger Shared = new(capacity: 256);
    private readonly int capacity;
    private readonly Atom<BindLedgerState> state;

    public BindLedger(int capacity = 256) {
        this.capacity = Math.Max(val1: 1, val2: capacity);
        state = Atom(new BindLedgerState(Next: 0L, History: Seq<BindLedgerEntry>(), Current: Seq<BindLedgerEntry>()));
    }

    public Seq<BindLedgerEntry> Entries => state.Value.History;

    public Seq<UiFault> Latest => state.Value.History.Map(static entry => entry.Fault);

    public Seq<UiFault> ForField(string field) =>
        state.Value.History
            .Filter(entry => string.Equals(a: entry.Field, b: field, comparisonType: StringComparison.Ordinal))
            .Map(static entry => entry.Fault);

    public bool IsValid(string field) =>
        !state.Value.Current.Exists(entry => string.Equals(a: entry.Field, b: field, comparisonType: StringComparison.Ordinal));

    public bool Record(string field, UiFault fault) => (state.Swap(held => {
        BindLedgerEntry entry = new(Ordinal: held.Next + 1L, Field: field, Fault: fault);
        Seq<BindLedgerEntry> current = held.Current.Filter(row => !string.Equals(a: row.Field, b: field, comparisonType: StringComparison.Ordinal));
        return held with {
            Next = entry.Ordinal,
            History = Append(rows: held.History, entry: entry),
            Current = Append(rows: current, entry: entry),
        };
    }), true).Item2;

    public bool Accept(string field) => (state.Swap(held => held with {
        Current = held.Current.Filter(entry => !string.Equals(a: entry.Field, b: field, comparisonType: StringComparison.Ordinal)),
    }), true).Item2;

    public Unit Clear() => ignore(state.Swap(held => held with {
        History = Seq<BindLedgerEntry>(),
        Current = Seq<BindLedgerEntry>(),
    }));

    private Seq<BindLedgerEntry> Append(Seq<BindLedgerEntry> rows, BindLedgerEntry entry) =>
        (rows.Count >= capacity ? rows.Tail : rows).Add(entry);
}
```

# [RASM_RHINO_ETO_BINDING]

`Bind.Rig` joins one typed control selector to state, context, or seed input and returns the receipt that owns refresh, validation evidence, cadence handlers, and exact unbind. Binding identity is explicit, current rejection state never shares a field-only key, and the realized element receipt retains every successful wire until the control tree releases.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `FlowMode`, `Cadence`, `BindingKey`, and `BindSource<TValue>` close propagation, timing, identity, and source shape.
- [03]-[STATE]: `Lens<TState,TValue>` and `StateCell<TState>` bridge one `Atom<TState>` into host binding.
- [04]-[RIG]: `BindingPlan<TControl,TValue>` and `Bind.Rig` own admission, host wiring, commit buffering, and receipt construction.
- [05]-[RECEIPT]: `BindReceipt` and `BindLedger` own deterministic release and bounded history without pruning current failures.

## [02]-[VOCABULARY]

- Owner: `FlowMode` carries host propagation rows; `Cadence` carries timing as distinct cases; `BindSource<TValue>` carries state, context, or seed evidence.
- Cases: `Cadence.Edit`, `Commit`, `Coalesced`, and `Restarted` expose `AfterDelay`'s reset behavior through the discriminant.
- Entry: `BindSource.Context`, `State`, `Seed`, and `Child` admit source paths before `Bind.Rig`.
- Growth: another cadence is one case; another propagation mode is one `FlowMode` row.
- Boundary: debounce applies only to context reads; commit buffers writes for state or context sources.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct BindingKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Binding identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[SmartEnum<int>]
public sealed partial class FlowMode {
    public static readonly FlowMode Both = new(key: 0, host: DualBindingMode.TwoWay, refresh: BindingUpdateMode.Destination);
    public static readonly FlowMode IntoControl = new(key: 1, host: DualBindingMode.OneWay, refresh: BindingUpdateMode.Destination);
    public static readonly FlowMode IntoSource = new(key: 2, host: DualBindingMode.OneWayToSource, refresh: BindingUpdateMode.Source);
    public static readonly FlowMode Seed = new(key: 3, host: DualBindingMode.OneTime, refresh: BindingUpdateMode.Destination);
    internal DualBindingMode Host { get; }
    internal BindingUpdateMode Refresh { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Cadence {
    private Cadence() { }
    public sealed record Edit : Cadence;
    public sealed record Commit : Cadence;
    public sealed record Coalesced(TimeSpan Window) : Cadence;
    public sealed record Restarted(TimeSpan Window) : Cadence;

    internal IndirectBinding<TValue> Apply<TValue>(IndirectBinding<TValue> path) => Switch(
        state: path,
        edit: static (source, _) => source,
        commit: static (source, _) => source,
        coalesced: static (source, cadence) => source.AfterDelay(cadence.Window, reset: false),
        restarted: static (source, cadence) => source.AfterDelay(cadence.Window, reset: true));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindSource<TValue> {
    private BindSource() { }
    public sealed record FromState(DirectBinding<TValue> Channel) : BindSource<TValue>;
    public sealed record FromContext(IndirectBinding<TValue> Path) : BindSource<TValue>;
    public sealed record FromValue(TValue Value) : BindSource<TValue>;

    public static BindSource<TValue> State<TState>(StateCell<TState> state, Lens<TState, TValue> lens) =>
        new FromState(state.Channel(lens));

    public static BindSource<TValue> Context<TContext>(System.Linq.Expressions.Expression<Func<TContext, TValue>> path) =>
        new FromContext(Binding.Property(path));

    public static BindSource<TValue> Seed(TValue value) => new FromValue(value);

    public Fin<BindSource<TNext>> Child<TNext>(System.Linq.Expressions.Expression<Func<TValue, TNext>> path, Op? key = null) => Switch(
        state: (Path: path, Key: key.OrDefault()),
        fromState: static (held, _) => Fin.Fail<BindSource<TNext>>(new UiFault.Rejected(Key: held.Key, Field: nameof(path), Reason: "state lenses compose before binding")),
        fromContext: static (held, source) => Fin.Succ<BindSource<TNext>>(new BindSource<TNext>.FromContext(source.Path.Child(held.Path))),
        fromValue: static (held, _) => Fin.Fail<BindSource<TNext>>(new UiFault.Rejected(Key: held.Key, Field: nameof(path), Reason: "seed values have no live child path")));
}
```

## [03]-[STATE]

- Owner: `StateCell<TState>` exposes one atom through typed lenses and exact event adapters.
- Entry: `StateCell.Channel` mints the `DirectBinding<TValue>` consumed by `BindSource.State`.
- Auto: atom change subscriptions map one-to-one to host handler identity, and removal uses the stored adapter.
- Growth: derived state composes lenses before channel creation; no second notification bus exists.
- Boundary: lens writes execute inside `Atom.Swap`, so `Put` remains pure under retries.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Lens<TState, TValue>(Func<TState, TValue> Get, Func<TState, TValue, TState> Put) {
    public Lens<TState, TNext> Then<TNext>(Lens<TValue, TNext> next) =>
        new(
            Get: state => next.Get(Get(state)),
            Put: (state, value) => Put(state, next.Put(Get(state), value)));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class StateCell<TState>(Atom<TState> state) {
    public TState Current => state.Value;

    public Unit Mutate(Func<TState, TState> transition) => ignore(state.Swap(transition));

    public DirectBinding<TValue> Channel<TValue>(Lens<TState, TValue> lens) {
        System.Collections.Concurrent.ConcurrentDictionary<EventHandler<EventArgs>, AtomChangedEvent<TState>> adapters = new();
        return Binding.Delegate<TValue>(
            getValue: () => lens.Get(state.Value),
            setValue: value => ignore(state.Swap(current => lens.Put(current, value))),
            addChangeEvent: handler => {
                AtomChangedEvent<TState> adapter = _ => handler(this, EventArgs.Empty);
                if (adapters.TryAdd(handler, adapter)) state.Change += adapter;
            },
            removeChangeEvent: handler => {
                if (adapters.TryRemove(handler, out AtomChangedEvent<TState>? adapter)) state.Change -= adapter;
            });
    }
}
```

## [04]-[RIG]

- Owner: `BindingPlan<TControl,TValue>` carries every input `Bind.Rig` needs, and each case consumes all of its evidence.
- Entry: `BindingPlan.Admitted` accumulates every absent dependency, admits the source-flow-cadence product, and stores only a legal plan; `Bind.Rig` selects the control binding, retains every setup fault, and returns one receipt.
- Auto: context cadence derives through `IndirectBinding.AfterDelay`; commit cadence closes one atomically exchanged latest-value latch before detach and drains its final admitted value.
- Receipt: setup and admission faults update `BindLedger` under `BindingKey`; successful rigging or admission clears only that key's current fault.
- Growth: another source is one `BindSource<TValue>` case and one total rig arm.
- Boundary: host `CatchException` records non-null host failures; semantic admission remains on the generated `Fin<TValue>` gate.
- Exemption: host bind, cadence attach, rollback, unbind, and admitted source assignment form the binding-provider statement seam.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BindingPlan<TControl, TValue> : IBindingPlan where TControl : Control {
    private BindingPlan(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TValue> source,
        FlowMode flow,
        Cadence cadence,
        Func<TValue, Fin<TValue>> admit,
        BindLedger ledger) {
        Key = key;
        Select = select;
        Source = source;
        Flow = flow;
        Cadence = cadence;
        Admit = admit;
        Ledger = ledger;
    }

    public BindingKey Key { get; }
    public Func<TControl, BindableBinding<TControl, TValue>> Select { get; }
    public BindSource<TValue> Source { get; }
    public FlowMode Flow { get; }
    public Cadence Cadence { get; }
    public Func<TValue, Fin<TValue>> Admit { get; }
    public BindLedger Ledger { get; }

    public static Fin<BindingPlan<TControl, TValue>> Admitted(
        BindingKey key,
        Func<TControl, BindableBinding<TControl, TValue>> select,
        BindSource<TValue> source,
        FlowMode flow,
        Cadence cadence,
        Func<TValue, Fin<TValue>> admit,
        BindLedger ledger,
        Op? operation = null) {
        Op op = operation.OrDefault();
        return (
            AdmittedKey(key, op),
            Required(select, nameof(select), op),
            Required(source, nameof(source), op),
            Required(flow, nameof(flow), op),
            Required(cadence, nameof(cadence), op),
            Required(admit, nameof(admit), op),
            Required(ledger, nameof(ledger), op))
            .Apply(static (ownedKey, ownedSelect, ownedSource, ownedFlow, ownedCadence, ownedAdmit, ownedLedger) =>
                (Key: ownedKey, Select: ownedSelect, Source: ownedSource, Flow: ownedFlow,
                 Cadence: ownedCadence, Admit: ownedAdmit, Ledger: ownedLedger))
            .As()
            .ToFin()
            .Bind(held => Legal(held.Source, held.Flow, held.Cadence, op, held.Key)
                .Map(source => new BindingPlan<TControl, TValue>(
                    held.Key, held.Select, source, held.Flow, held.Cadence, held.Admit, held.Ledger)));
    }

    private static K<Validation<Error>, BindingKey> AdmittedKey(BindingKey key, Op op) =>
        string.IsNullOrWhiteSpace(key.Value)
            ? Validation<Error, BindingKey>.Fail(new UiFault.Rejected(
                Key: op, Field: nameof(key), Reason: "binding identity requires an admitted key"))
            : Validation<Error, BindingKey>.Success(key);

    private static K<Validation<Error>, T> Required<T>(T? value, string field, Op op) where T : class =>
        value is { } admitted
            ? Validation<Error, T>.Success(admitted)
            : Validation<Error, T>.Fail(new UiFault.Rejected(Key: op, Field: field, Reason: $"{field} is absent"));

    private static Fin<BindSource<TValue>> Legal(
        BindSource<TValue> source,
        FlowMode flow,
        Cadence cadence,
        Op op,
        BindingKey key) =>
        (source, flow, cadence) switch {
            (BindSource<TValue>.FromState, _, Cadence.Coalesced or Cadence.Restarted) =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "debounce requires a context path")),
            (_, _, _) when flow == FlowMode.Seed && source is not BindSource<TValue>.FromValue =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "one-time flow requires seed input")),
            (BindSource<TValue>.FromValue, _, _) when flow != FlowMode.Seed =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "seed input requires one-time flow")),
            (BindSource<TValue>.FromValue, _, not Cadence.Edit) =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "seed input requires edit cadence")),
            (_, _, Cadence.Commit) when flow == FlowMode.IntoControl || flow == FlowMode.Seed =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "commit cadence requires source-directed flow")),
            (BindSource<TValue>.FromContext, _, Cadence.Coalesced or Cadence.Restarted) when flow == FlowMode.IntoSource =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "debounce requires control-directed flow")),
            (_, _, Cadence.Coalesced { Window.Ticks: <= 0 } or Cadence.Restarted { Window.Ticks: <= 0 }) =>
                Fin.Fail<BindSource<TValue>>(new UiFault.Rejected(Key: op, Field: key.Value, Reason: "debounce cadence requires a positive window")),
            (BindSource<TValue>.FromContext context, _, _) => Fin.Succ<BindSource<TValue>>(context with { Path = cadence.Apply(context.Path) }),
            _ => Fin.Succ(source),
        };

    public Fin<BindReceipt> Rig(Control control, Op key) => Bind.Rig(control, this, key);
}

public interface IBindingPlan {
    Fin<BindReceipt> Rig(Control control, Op key);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Bind {
    public static Fin<BindReceipt> Rig<TControl, TValue>(Control control, BindingPlan<TControl, TValue> plan, Op? key = null)
        where TControl : Control {
        Op op = key.OrDefault();
        Fin<BindReceipt> outcome =
            from typed in control is TControl accepted
                ? Fin.Succ(accepted)
                : Fin.Fail<TControl>(new UiFault.Rejected(Key: op, Field: plan.Key.Value, Reason: "control type rejected"))
            from receipt in op.Catch(() => Wire(typed, plan, op))
            select receipt;
        return outcome
            .Map(receipt => (plan.Ledger.Accept(plan.Key), receipt).Item2)
            .MapFail(fault => (
                plan.Ledger.Reject(
                    plan.Key,
                    fault as UiFault ?? new UiFault.HostRejected(Key: op, Detail: fault.Message)),
                fault).Item2);
    }

    private static BindReceipt Wire<TControl, TValue>(
        TControl control,
        BindingPlan<TControl, TValue> plan,
        Op op)
        where TControl : Control {
        BindableBinding<TControl, TValue> target = plan.Select(control);
        (DualBinding<TValue> Binding, Func<Unit> Detach) wired = plan.Source.Switch(
            state: (Target: target, Plan: plan, Op: op, Control: (Control)control),
            fromState: static (held, source) => Direct(held.Target, source.Channel, held.Plan, held.Op, held.Control),
            fromContext: static (held, source) => Context(held.Target, source.Path, held.Plan, held.Op, held.Control),
            fromValue: static (held, source) => (
                held.Target.Bind(Binding.Delegate<TValue>(getValue: () => source.Value), DualBindingMode.OneTime),
                static () => unit));
        return new BindReceipt(
            plan.Key,
            plan.Ledger,
            op,
            refresh: () => op.Catch(() => {
                wired.Binding.Update(plan.Flow.Refresh);
                return Fin.Succ(unit);
            }),
            unbind: () => op.Catch(() => {
                try { _ = wired.Detach(); }
                finally { wired.Binding.Unbind(); }
                return Fin.Succ(unit);
            }));
    }

    private static (DualBinding<TValue>, Func<Unit>) Direct<TControl, TValue>(
        BindableBinding<TControl, TValue> target,
        DirectBinding<TValue> source,
        BindingPlan<TControl, TValue> plan,
        Op op,
        Control control) where TControl : Control {
        (Action<TValue> send, Func<Unit> attach, Func<Unit> detach) =
            Gate<TValue>(plan.Cadence, control, value => Admit(value, accepted => source.DataValue = accepted, plan, op));
        DirectBinding<TValue> caught = source.Convert<TValue>(static value => value, (_, value) => send(value)).CatchException(thrown =>
            thrown is null || (plan.Ledger.Reject(plan.Key, new UiFault.HostRejected(Key: op, Detail: thrown.Message)), true).Item2);
        return Lease(() => target.Bind(caught, plan.Flow.Host), attach, detach);
    }

    private static (DualBinding<TValue>, Func<Unit>) Context<TControl, TValue>(
        BindableBinding<TControl, TValue> target,
        IndirectBinding<TValue> source,
        BindingPlan<TControl, TValue> plan,
        Op op,
        Control control) where TControl : Control {
        (Action<(object Item, TValue Value)> send, Func<Unit> attach, Func<Unit> detach) =
            Gate<(object Item, TValue Value)>(plan.Cadence, control, row => Admit(row.Value, accepted => source.SetValue(row.Item, accepted), plan, op));
        IndirectBinding<TValue> caught = new DelegateBinding<object, TValue>(
            getValue: source.GetValue,
            setValue: (item, value) => send((item, value)),
            addChangeEvent: source.AddValueChangedHandler,
            removeChangeEvent: source.RemoveValueChangedHandler).CatchException(thrown =>
            thrown is null || (plan.Ledger.Reject(plan.Key, new UiFault.HostRejected(Key: op, Detail: thrown.Message)), true).Item2);
        return Lease(() => target.BindDataContext(caught, plan.Flow.Host), attach, detach);
    }

    private static (DualBinding<TValue>, Func<Unit>) Lease<TValue>(
        Func<DualBinding<TValue>> bind,
        Func<Unit> attach,
        Func<Unit> detach) {
        DualBinding<TValue>? binding = null;
        try {
            binding = bind();
            _ = attach();
            return (binding, detach);
        } catch {
            try { _ = detach(); }
            finally { binding?.Unbind(); }
            throw;
        }
    }

    private static (Action<TPayload> Send, Func<Unit> Attach, Func<Unit> Detach) Gate<TPayload>(
        Cadence cadence, Control control, Action<TPayload> commit) => cadence.Switch(
            state: (Control: control, Write: commit),
            edit: static (held, _) => Immediate(held.Write),
            commit: static (held, _) => Buffered(held.Control, held.Write),
            coalesced: static (held, _) => Immediate(held.Write),
            restarted: static (held, _) => Immediate(held.Write));

    private static (Action<TPayload> Send, Func<Unit> Attach, Func<Unit> Detach) Immediate<TPayload>(Action<TPayload> commit) =>
        (commit, static () => unit, static () => unit);

    private static (Action<TPayload> Send, Func<Unit> Attach, Func<Unit> Detach) Buffered<TPayload>(
        Control control, Action<TPayload> commit) {
        System.Runtime.CompilerServices.StrongBox<Option<TPayload>>? gate = new(Option<TPayload>.None);
        Action<TPayload> send = value => {
            while (true) {
                System.Runtime.CompilerServices.StrongBox<Option<TPayload>>? current = Volatile.Read(ref gate);
                if (current is null) return;
                System.Runtime.CompilerServices.StrongBox<Option<TPayload>> next = new(Some(value));
                if (ReferenceEquals(Interlocked.CompareExchange(ref gate, next, current), current)) return;
            }
        };
        Func<Unit> drain = () => {
            while (true) {
                System.Runtime.CompilerServices.StrongBox<Option<TPayload>>? current = Volatile.Read(ref gate);
                if (current is null) return unit;
                System.Runtime.CompilerServices.StrongBox<Option<TPayload>> next = new(Option<TPayload>.None);
                if (ReferenceEquals(Interlocked.CompareExchange(ref gate, next, current), current)) return current.Value.Iter(commit);
            }
        };
        EventHandler<EventArgs> handler = (_, _) => _ = drain();
        return (
            send,
            () => Op.Side(() => control.LostFocus += handler),
            () => Op.Side(() => {
                System.Runtime.CompilerServices.StrongBox<Option<TPayload>>? closed = Interlocked.Exchange(ref gate, null);
                control.LostFocus -= handler;
                _ = closed?.Value.Iter(commit);
            }));
    }

    private static Unit Admit<TControl, TValue>(
        TValue value, Action<TValue> write, BindingPlan<TControl, TValue> plan, Op op) where TControl : Control =>
        op.Catch(() => plan.Admit(value).Map(accepted => Op.Side(() => write(accepted)))).Match(
            Succ: _ => plan.Ledger.Accept(plan.Key),
            Fail: fault => plan.Ledger.Reject(plan.Key, new UiFault.Rejected(Key: op, Field: plan.Key.Value, Reason: fault.Message)));
}
```

## [05]-[RECEIPT]

- Owner: `BindReceipt` owns one dual binding and cadence detach; `BindLedger` owns keyed current validity and bounded historical evidence.
- Entry: `BindLedger.Admitted` rejects default capacity before state allocation; `Refresh`, `Release`, and `Dispose` are the receipt lifecycle; `Accept` and `Reject` are the ledger transitions.
- Receipt: `IsValid` requires an unreleased binding and no current failure under its exact `BindingKey`.
- Growth: another evidence field extends `BindLedgerEntry`; current failures remain independent of history retention.
- Boundary: element realization retains receipts and disposes them in reverse tree order.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct LedgerCapacity {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value <= 0 ? new ValidationError(message: "Ledger capacity must be positive.") : null;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BindLedgerEntry(long Ordinal, BindingKey Key, UiFault Fault);
internal sealed record BindLedgerState(long Next, Seq<BindLedgerEntry> History, HashMap<BindingKey, UiFault> Current);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class BindLedger {
    private readonly LedgerCapacity capacity;
    private readonly Atom<BindLedgerState> state;

    private BindLedger(LedgerCapacity capacity) {
        this.capacity = capacity;
        state = Atom(new BindLedgerState(0L, Seq<BindLedgerEntry>(), HashMap<BindingKey, UiFault>()));
    }

    public static Fin<BindLedger> Admitted(LedgerCapacity capacity, Op? key = null) =>
        capacity.Value > 0
            ? Fin.Succ(new BindLedger(capacity))
            : Fin.Fail<BindLedger>(new UiFault.Rejected(
                Key: key.OrDefault(), Field: nameof(capacity), Reason: "ledger capacity requires an admitted positive value"));

    public Seq<BindLedgerEntry> History => state.Value.History;
    public HashMap<BindingKey, UiFault> Current => state.Value.Current;
    public bool IsValid(BindingKey key) => !state.Value.Current.ContainsKey(key);

    public Unit Reject(BindingKey key, UiFault fault) => ignore(state.Swap(held => {
        BindLedgerEntry entry = new(held.Next + 1L, key, fault);
        Seq<BindLedgerEntry> history = held.History.Add(entry);
        return new BindLedgerState(
            entry.Ordinal,
            history.Count > capacity.Value ? history.Skip(history.Count - capacity.Value) : history,
            held.Current.AddOrUpdate(key, fault));
    }));

    public Unit Accept(BindingKey key) => ignore(state.Swap(held => held with { Current = held.Current.Remove(key) }));
}

public sealed class BindReceipt(
    BindingKey key,
    BindLedger ledger,
    Op op,
    Func<Fin<Unit>> refresh,
    Func<Fin<Unit>> unbind) : UiLease {
    public BindingKey Key { get; } = key;
    public bool IsValid => !Released && ledger.IsValid(Key);

    public Fin<Unit> Refresh() => Released
        ? Fin.Fail<Unit>(new UiFault.Released(Key: op, Resource: Key.Value))
        : refresh().MapFail(fault => (
            ledger.Reject(Key, fault as UiFault ?? new UiFault.HostRejected(Key: op, Detail: fault.Message)), fault).Item2);

    protected override Fin<Unit> Free() => unbind();

    public override void Dispose() => ignore(Release().Match(
        Succ: static released => released,
        Fail: fault => ledger.Reject(Key, new UiFault.HostRejected(Key: op, Detail: fault.Message))));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

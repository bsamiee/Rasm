using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters;
using Rasm.Domain;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IOutputGroup<TState> where TState : notnull {
    public Seq<IPort> Ports { get; }
    public Unit Run(IDataAccess access, int slot, TState state);
    public Unit Empty(IDataAccess access, int slot);
}

public interface IOutputSlot<TState, TSource> where TState : notnull {
    public IPort Port { get; }
    public Unit Write(IDataAccess access, int slot, TState state, Seq<TSource> source);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Hints(
    Seq<(IPort Port, int Slot, Coverage Coverage, bool Changed)> Inputs,
    Seq<(int Slot, IPear Pear)> Pears) {
    public static Hints Capture(Seq<IPort> inputs, IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Seq<(IPort Port, int Slot, Coverage Coverage, bool Changed)> captured = inputs.Map((port, slot) => (
            Port: port,
            Slot: slot,
            Coverage: access.CoverageIn(index: slot),
            Changed: access.HasInputChanged(index: slot)));
        int[] slots = [.. captured.Filter(static input => input.Port.Access is Access.Item).Map(static input => input.Slot)];
        Seq<(int Slot, IPear Pear)> pears = slots.Length switch {
            > 0 => fun((IDataAccess data, int[] itemSlots) => {
                data.GetIPears(pears: out IPear[] values, indexMap: out int[] map, inputs: itemSlots);
                return toSeq(values.Zip(second: map, resultSelector: static (pear, slot) => (Slot: slot, Pear: pear)));
            })(access, slots),
            _ => Seq<(int Slot, IPear Pear)>(),
        };
        return new(Inputs: captured, Pears: pears);
    }
    public Option<int> Index(IDataAccess access, Port<int> port, int limit) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => access.Index(slot: input.Slot, limit: limit));
    }
    public Option<TVal> Value<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => Bridge.Read<TVal>(access: access, slot: input.Slot, port: port).ToOption().Bind(static data => data.Value));
    }
    public Option<PortData<TVal>> Data<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => Bridge.Read<TVal>(access: access, slot: input.Slot, port: port).ToOption());
    }
    public bool HasChanged(IPort port) =>
        Inputs.Find(predicate: input => input.Port.Equals(port)).Map(static input => input.Changed).IfNone(static () => false);
}

public readonly record struct OutputValue<TValue>(TValue Value, MetaData? Meta, bool IsNull);

public static class OutputValue {
    public static OutputValue<TValue> Plain<TValue>(TValue value) =>
        new(Value: value, Meta: null, IsNull: false);
    public static OutputValue<TValue> WithMeta<TValue>(TValue value, MetaData meta) =>
        new(Value: value, Meta: meta, IsNull: false);
    public static OutputValue<TValue> Null<TValue>() =>
        new(Value: default!, Meta: null, IsNull: true);
}

public sealed record OutputSlot<TState, TSource, TOut>(
    Port<TOut> Port,
    Func<TState, Seq<TSource>, Fin<Seq<OutputValue<TOut>>>> Project) : IOutputSlot<TState, TSource>
    where TState : notnull {
    IPort IOutputSlot<TState, TSource>.Port => Port;

    public Unit Write(IDataAccess access, int slot, TState state, Seq<TSource> source) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: state);
        return Project(arg1: state, arg2: source).Match(
            Succ: values => Bridge.Write(access: access, slot: slot, name: Port.Name, targetAccess: Port.Access, values: [.. values]),
            Fail: error => {
                access.AddWarning(text: Port.Name, details: error.Message);
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Bridge.Write(access: access, slot: slot, name: Port.Name, targetAccess: Port.Access, values: System.Array.Empty<OutputValue<TOut>>());
    }
}

public sealed record PreparedGroup<TState, TSource>(
    Seq<IOutputSlot<TState, TSource>> Slots,
    Func<IDataAccess, TState, Fin<Seq<TSource>>> Source,
    bool EmptyUnsupported) : IOutputGroup<TState>
    where TState : notnull {
    public Seq<IPort> Ports =>
        Slots.Map(static slot => slot.Port);

    public Unit Run(IDataAccess access, int slot, TState state) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: state);
        return Source(arg1: access, arg2: state).Match(
            Succ: values => Slots.Iter((offset, output) => output.Write(access: access, slot: slot + offset, state: state, source: values)),
            Fail: error => {
                // BOUNDARY ADAPTER — GH2 warnings are void side effects on IDataAccess.
                switch (EmptyUnsupported, Unsupported(error: error)) {
                    case (true, true):
                        break;
                    default:
                        access.AddWarning(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: error.Message);
                        break;
                }
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(access: access, slot: slot + offset));
    }
    private static bool Unsupported(Error error) =>
        error.Code == OpFault.UnsupportedCode;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Output {
    public static IOutputSlot<TState, TSource> Slot<TState, TSource, TOut>(
        Port<TOut> port,
        Func<TState, Seq<TSource>, Fin<Seq<OutputValue<TOut>>>> project)
        where TState : notnull =>
        new OutputSlot<TState, TSource, TOut>(Port: port, Project: project);
    public static IOutputSlot<TState, TOut> Slot<TState, TOut>(Port<TOut> port)
        where TState : notnull =>
        Slot<TState, TOut, TOut>(
            port: port,
            project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value))));
    public static IOutputGroup<TState> Prepared<TState, TSource>(
        Func<IDataAccess, TState, Fin<Seq<TSource>>> source,
        bool emptyUnsupported,
        params IOutputSlot<TState, TSource>[] slots)
        where TState : notnull =>
        new PreparedGroup<TState, TSource>(Slots: toSeq(slots), Source: source, EmptyUnsupported: emptyUnsupported);
    public static IOutputGroup<TState> Prepared<TState, TSource>(
        Func<IDataAccess, TState, Fin<Seq<TSource>>> source,
        params IOutputSlot<TState, TSource>[] slots)
        where TState : notnull =>
        Prepared(source: source, emptyUnsupported: false, slots: slots);
}

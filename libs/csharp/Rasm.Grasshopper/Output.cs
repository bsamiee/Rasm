using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Rasm.Analysis;
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

public readonly record struct Hints(Seq<(IPort Port, int Slot)> Inputs) {
    public static Hints Capture(Seq<IPort> inputs) =>
        new(Inputs: inputs.Map(static (port, slot) => (Port: port, Slot: slot)));
    public Option<int> Index(IDataAccess access, Port<int> port, int limit) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => access.Index(slot: input.Slot, limit: limit));
    }
    public Option<TVal> Value<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => access.GetPear(index: input.Slot, pear: out Pear<TVal> pear) switch {
            true when !access.GetNull(index: input.Slot) => Some(pear.Item),
            _ => Option<TVal>.None,
        });
    }
}

public readonly record struct OutputValue<TValue>(TValue Value, MetaData? Meta);

public static class OutputValue {
    public static OutputValue<TValue> Plain<TValue>(TValue value) =>
        new(Value: value, Meta: null);
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

public sealed record OutputGroup<TState, TGeometry, TSource>(
    Seq<IOutputSlot<TState, TSource>> Slots,
    Func<TState, Analyze.Scope> Scope,
    Func<TState, TGeometry> Select,
    Func<TState, Query<TGeometry, TSource>> Query) : IOutputGroup<TState>
    where TState : notnull
    where TGeometry : notnull {
    public Seq<IPort> Ports =>
        Slots.Map(static slot => slot.Port);

    public Unit Run(IDataAccess access, int slot, TState state) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: state);
        return Bridge.Values(scope: Scope(arg: state), input: Select(arg: state), query: Query(arg: state)).Match(
            Succ: values => Slots.Iter((offset, output) => output.Write(access: access, slot: slot + offset, state: state, source: values)),
            Fail: error => {
                access.AddWarning(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: error.Message);
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(access: access, slot: slot + offset));
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
                _ = (EmptyUnsupported, Unsupported(error: error)) switch {
                    (true, true) => Unit.Default,
                    _ => Warning(access: access, name: Ports.Head.Map(static port => port.Name).IfNone("Output"), error: error),
                };
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(access: access, slot: slot + offset));
    }
    private static bool Unsupported(Error error) =>
        error.Message.Contains(value: "does not support", comparisonType: StringComparison.Ordinal);
    private static Unit Warning(IDataAccess access, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return Unit.Default;
    }
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
    public static IOutputGroup<TState> Query<TState, TGeometry, TSource>(
        Func<TState, Analyze.Scope> scope,
        Func<TState, TGeometry> select,
        Func<TState, Query<TGeometry, TSource>> query,
        params IOutputSlot<TState, TSource>[] slots)
        where TState : notnull
        where TGeometry : notnull =>
        new OutputGroup<TState, TGeometry, TSource>(Slots: toSeq(slots), Scope: scope, Select: select, Query: query);
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

using Grasshopper2.Components;
using Grasshopper2.Data;
using Rasm.Analysis;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IOutput<TIn> where TIn : notnull {
    public IPort Port { get; }
    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Hints hints, TIn input);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Hints(Seq<(IPort Port, int Slot)> Inputs) {
    public static Hints Capture(Seq<IPort> inputs) =>
        new(Inputs: inputs.Map(static (port, slot) => (Port: port, Slot: slot)));
    public Option<int> Index(IDataAccess access, Port<int> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => access.Index(slot: input.Slot));
    }
    public Option<TVal> Value<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port)).Bind(input => access.GetPear(index: input.Slot, pear: out Pear<TVal> pear) switch {
            true when !access.GetNull(index: input.Slot) => Some(pear.Item),
            _ => Option<TVal>.None,
        });
    }
}

public readonly record struct Output<TIn, TQuery, TOut>(
    Port<TOut> Port,
    Func<TIn, TQuery> Select,
    Func<IDataAccess, Hints, Query<TQuery, TOut>> Build) : IOutput<TIn>
    where TIn : notnull
    where TQuery : notnull {
    IPort IOutput<TIn>.Port => Port;

    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Hints hints, TIn input) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        ArgumentNullException.ThrowIfNull(argument: input);
        return Bridge.Run(access: access, slot: slot, port: Port, scope: scope, input: Select(arg: input), query: Build(arg1: access, arg2: hints));
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Bridge.Write<TOut>(access: access, slot: slot, name: Port.Name, targetAccess: Port.Access, values: []);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Output {
    public static Output<TIn, TQuery, TOut> Of<TIn, TQuery, TOut>(Port<TOut> port, Func<TIn, TQuery> select, Query<TQuery, TOut> query)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: (_, _) => query);
    public static Output<TIn, TQuery, TOut> Indexed<TIn, TQuery, TOut>(
        Port<TOut> port,
        Port<int> index,
        Func<TIn, TQuery> select,
        Func<int?, Query<TQuery, TOut>> build)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: (access, hints) => build(arg: hints.Index(access: access, port: index).Map(static value => (int?)value).IfNone(static () => null)));
    public static Output<TIn, TQuery, TOut> Controlled<TIn, TQuery, THint, TOut>(
        Port<TOut> port,
        Port<THint> control,
        Func<TIn, TQuery> select,
        Func<Option<THint>, Query<TQuery, TOut>> build)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: (access, hints) => build(arg: hints.Value(access: access, port: control)));
    public static Output<TIn, TQuery, TOut> Controlled<TIn, TQuery, THintA, THintB, TOut>(
        Port<TOut> port,
        Port<THintA> controlA,
        Port<THintB> controlB,
        Func<TIn, TQuery> select,
        Func<Option<THintA>, Option<THintB>, Query<TQuery, TOut>> build)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: (access, hints) => build(
            arg1: hints.Value(access: access, port: controlA),
            arg2: hints.Value(access: access, port: controlB)));
}

using Grasshopper2.Components;
using Rasm.Analysis;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IOutput<TIn> where TIn : notnull {
    public IPort Port { get; }
    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Option<int> hint, TIn input);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Output<TIn, TQuery, TOut>(
    Port<TOut> Port,
    Func<TIn, TQuery> Select,
    Func<Option<int>, Query<TQuery, TOut>> Build) : IOutput<TIn>
    where TIn : notnull
    where TQuery : notnull {
    IPort IOutput<TIn>.Port => Port;

    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Option<int> hint, TIn input) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        ArgumentNullException.ThrowIfNull(argument: input);
        return Bridge.Run(access: access, slot: slot, port: Port, scope: scope, input: Select(arg: input), query: Build(arg: hint));
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Bridge.Write<TOut>(access: access, slot: slot, targetAccess: Port.Access, values: []);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Output {
    public static Output<TIn, TQuery, TOut> Of<TIn, TQuery, TOut>(Port<TOut> port, Func<TIn, TQuery> select, Query<TQuery, TOut> query)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: _ => query);
    public static Output<TIn, TQuery, TOut> Indexed<TIn, TQuery, TOut>(
        Port<TOut> port,
        Func<TIn, TQuery> select,
        Func<Option<int>, Query<TQuery, TOut>> build)
        where TIn : notnull
        where TQuery : notnull =>
        new(Port: port, Select: select, Build: build);
}

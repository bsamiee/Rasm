using Analysis;
using Core.Domain;
using Grasshopper2.Components;
namespace Grasshopper;

// --- [TYPES] -----------------------------------------------------------------------------------

public interface IOutput<TIn> where TIn : RhinoGeometry {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public Type Type { get; }
    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Option<int> hint, TIn input);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] ----------------------------------------------------------------------------------

public readonly record struct Output<TIn, TOut>(
    string Name,
    string Code,
    string Info,
    Func<Option<int>, Query<object, TOut>> Build) : IOutput<TIn> where TIn : RhinoGeometry {
    public Output(string name, string code, string info, Query<object, TOut> query)
        : this(Name: name, Code: code, Info: info, Build: _ => query) { }

    public Type Type => typeof(TOut);

    public Unit Run(IDataAccess access, int slot, Analyze.Scope scope, Option<int> hint, TIn input) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        ArgumentNullException.ThrowIfNull(argument: input);
        return Bridge.Run(access: access, slot: slot, name: Name, scope: scope, input: input.Inner, query: Build(arg: hint));
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Bridge.WriteTwig<TOut>(access: access, slot: slot, values: []);
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static class Output {
    public static Output<TIn, TOut> Of<TIn, TOut>(string name, string code, string info, Query<object, TOut> query)
        where TIn : RhinoGeometry =>
        new(name: name, code: code, info: info, query: query);
    public static Output<TIn, TOut> Indexed<TIn, TOut>(string name, string code, string info, Func<Option<int>, Query<object, TOut>> build)
        where TIn : RhinoGeometry =>
        new(Name: name, Code: code, Info: info, Build: build);
}

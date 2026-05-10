using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using Rasm.Analysis;
using Rasm.Domain;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IComponentSpec<TInput, TState>
    where TInput : notnull
    where TState : notnull {
    public static abstract Seq<IPort> Inputs { get; }
    public static abstract Seq<IOutputGroup<TState>> Outputs { get; }
    public static abstract Fin<TInput> Read(IDataAccess access);
    public static abstract Fin<TState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, TInput input);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ShapeState(Analyze.Scope Scope, Hints Hints, Shape Input) {
    public object Geometry =>
        Input.Inner;
}

public static class ComponentNomen {
    public static Nomen Of<TSelf>() =>
        typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen
            ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty);
}

public static class ShapeFoundation {
    public static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Geometry to analyse.");

    public static Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
    public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        ArgumentNullException.ThrowIfNull(argument: input);
        return Fin.Succ(new ShapeState(Scope: scope, Hints: hints, Input: input));
    }
    public static Seq<IPort> Inputs(Seq<IPort> controls) =>
        toSeq(Seq<IPort>(Geometry).Concat(second: controls));
}

// --- [SERVICES] ------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Rasm-family Grasshopper 2 components. Static specs own the complete
/// port and output declaration so GH2 constructor-time registration never reads derived instance state.
/// </summary>
public abstract class Component<TSpec, TInput, TState> : Grasshopper2.Components.Component
    where TSpec : IComponentSpec<TInput, TState>
    where TInput : notnull
    where TState : notnull {
    protected Component(Nomen nomen) : base(nomen: nomen) { }
    protected Component(IReader reader) : base(reader: reader) { }

    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = TSpec.Inputs.Iter(port => port.Param.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = TSpec.Outputs.Bind(static group => group.Ports).Iter(port => port.Param.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access));
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints hints = Hints.Capture(inputs: TSpec.Inputs);
        _ = (
            from scope in access.Scope()
            from input in TSpec.Read(access: access)
            from state in TSpec.Prepare(access: access, scope: scope, hints: hints, input: input)
            select state)
            .Match(
                Succ: state => Run(access: access, state: state),
                Fail: error => (
                    access.MissingInput(error: error),
                    Empty(access: access)).Item2);
    }
    private static Unit Run(IDataAccess access, TState state) =>
        TSpec.Outputs.Fold(
            initialState: 0,
            f: (slot, group) => (group.Run(access: access, slot: slot, state: state), slot + group.Ports.Count).Item2) switch {
                _ => Unit.Default,
            };
    private static Unit Empty(IDataAccess access) =>
        TSpec.Outputs.Fold(
            initialState: 0,
            f: (slot, group) => (group.Empty(access: access, slot: slot), slot + group.Ports.Count).Item2) switch {
                _ => Unit.Default,
            };
}

/// <summary>
/// Shared base for geometry-query components that operate on the canonical Shape input.
/// </summary>
public abstract class ShapeComponent<TSpec> : Component<TSpec, Shape, ShapeState>
    where TSpec : IComponentSpec<Shape, ShapeState> {
    protected ShapeComponent(Nomen nomen) : base(nomen: nomen) { }
    protected ShapeComponent(IReader reader) : base(reader: reader) { }
}

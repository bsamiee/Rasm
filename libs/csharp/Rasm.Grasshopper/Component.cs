using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino.Geometry;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IComponentSpec {
    public static abstract Seq<IPort> Inputs { get; }
    public static abstract Seq<IOutputGroup> Outputs { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct GrasshopperRuntime(Analyze.Scope Scope, Hints Hints) {
    public static Fin<GrasshopperRuntime> Capture(IDataAccess access, Seq<IPort> inputs) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Scope().Map(scope => new GrasshopperRuntime(Scope: scope, Hints: Hints.Capture(inputs: inputs, access: access)));
    }
    public Fin<Shape> Shape(IDataAccess access, Port<Shape> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Hints.Slot(port: port)
            .ToFin(Error.New(message: $"{port.Name} input is required."))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

public static class ComponentNomen {
    public static Nomen Of<TSelf>() =>
        typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen
            ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty);
}

// --- [SERVICES] ------------------------------------------------------------------------

public abstract class Component<TSpec> : Grasshopper2.Components.Component
    where TSpec : IComponentSpec {
    protected Component(Nomen nomen) : base(nomen: nomen) { }
    protected Component(IReader reader) : base(reader: reader) { }

    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = TSpec.Inputs.Iter(port => port.Kind.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement, policy: port.Policy));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = TSpec.Outputs.Bind(static group => group.Ports).Iter(port => port.Kind.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy));
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = GrasshopperRuntime.Capture(access: access, inputs: TSpec.Inputs)
            .Map(runtime => Output.Write(access: access, runtime: runtime, groups: TSpec.Outputs))
            .Match(
                Succ: static _ => Unit.Default,
                Fail: error => (
                    access.MissingInput(error: error),
                    Output.Empty(access: access, groups: TSpec.Outputs)).Item2);
    }
}

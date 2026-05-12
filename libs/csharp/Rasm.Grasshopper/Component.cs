using System.Reflection;

namespace Rasm.Grasshopper;

public interface IComponentSpec {
    public static abstract Seq<IPort> Inputs { get; }
    public static abstract Seq<IOutputGroup> Outputs { get; }
}
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
public abstract class Component<TSelf> : Grasshopper2.Components.Component
    where TSelf : Component<TSelf>, IComponentSpec {
    protected Component() : base(nomen: typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty)) { }
    protected Component(IReader reader) : base(reader: reader) { }
    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = TSelf.Inputs.Iter(port => port.Kind.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement, policy: port.Policy));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = TSelf.Outputs.Bind(static group => group.Ports).Iter(port => port.Kind.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy));
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = GrasshopperRuntime.Capture(access: access, inputs: TSelf.Inputs)
            .Map(runtime => Output.Write(access: access, runtime: runtime, groups: TSelf.Outputs))
            .Match(
                Succ: static _ => Unit.Default,
                Fail: error => (fun((IDataAccess target) => { target.AddError(text: "Input", details: error.Message); return Unit.Default; })(access), Output.Empty(access: access, groups: TSelf.Outputs)).Item2);
    }
}

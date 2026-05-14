using System.Reflection;
using Grasshopper2.Doc;
using Grasshopper2.UI.Icon;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IconAttribute(string name) : Attribute {
    public string Name { get; } = name;
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct PortSpec(IPort Port, bool Hidden = false);
public readonly record struct OutputSpec(IOutputGroup Group, bool Hidden = false);
public readonly record struct ComponentSpec(Seq<PortSpec> Inputs, Seq<OutputSpec> Outputs) {
    public Seq<IPort> InputPorts => Inputs.Map(static spec => spec.Port);
    public Seq<IOutputGroup> OutputGroups => Outputs.Map(static spec => spec.Group);
    public Seq<IPort> OutputPorts => OutputGroups.Bind(static group => group.Ports);
}
public readonly record struct GrasshopperRuntime(IDataAccess Access, Analyze.Scope Scope, Hints Hints, IProgress<double> Progress, CancellationToken Cancellation) {
    public static Fin<GrasshopperRuntime> Capture(IDataAccess access, Seq<IPort> inputs) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Scope().Map(scope => new GrasshopperRuntime(
            Access: access,
            Scope: scope,
            Hints: Hints.Capture(inputs: inputs),
            Progress: new Bridge.Progress(access: access),
            Cancellation: access.Solution.Token));
    }
    internal Fin<Pear<Shape>> Shape(IDataAccess access, Port<Shape> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Hints.Slot(port: port)
            .ToFin(new Fault.InputRequired(PortName: port.Name))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

// --- [COMPOSITION] ----------------------------------------------------------------------
public abstract class Component(Type self, ComponentSpec spec) : Grasshopper2.Components.ModularComponent(nomen: (self ?? throw new ArgumentNullException(paramName: nameof(self))).GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: self.Name, info: string.Empty)) {
    private Seq<IPort> cachedInputs = Seq<IPort>();
    private Seq<IOutputGroup> cachedOutputs = Seq<IOutputGroup>();
    private Type Self { get; } = self;
    public ComponentSpec Spec => spec;
    protected override IIcon IconInternal =>
        Self.GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: Self),
            _ => base.IconInternal,
        };
    protected override void AddInputs(ModularInputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        cachedInputs = spec.InputPorts;
        _ = spec.Inputs.Iter(pair => pair.Port.Kind.Bind(adder: inputs.RegularAdder, name: pair.Port.Name, code: pair.Port.Code, info: pair.Port.Info, access: pair.Port.Access, requirement: pair.Port.Requirement, policy: pair.Port.Policy, hidden: pair.Hidden));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        cachedOutputs = spec.OutputGroups;
        _ = spec.Outputs.Iter(pair => pair.Group.Ports.Iter(port => port.Kind.Bind(adder: outputs.RegularAdder, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy, hidden: pair.Hidden)));
    }
    protected override void BeforeProcess(Solution solution) {
        base.BeforeProcess(solution: solution);
        OnBeforeSolve(solution: solution);
    }
    protected override void PostProcess(Solution solution, FleetingCustomData customData) {
        OnAfterSolve(solution: solution);
        base.PostProcess(solution: solution, customData: customData);
    }
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = GrasshopperRuntime.Capture(access: access, inputs: cachedInputs)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, groups: cachedOutputs),
                Fail: error => {
                    access.AddWarning(text: error.Category(), details: error.Message);
                    return Output.Empty(access: access, groups: cachedOutputs);
                });
    }
    protected virtual void OnBeforeSolve(Solution solution) { }
    protected virtual void OnAfterSolve(Solution solution) { }
}

using System.Reflection;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Icon;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IconAttribute(string name) : Attribute {
    public string Name { get; } = name;
    internal static IIcon Resolve(Type owner, IIcon fallback) =>
        owner.GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: owner),
            _ => fallback,
        };
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct PortSpec(IPort Port, bool Hidden = false);
public readonly record struct OutputSpec(OutputGroup Group, bool Hidden = false);
internal readonly record struct BoundPort(IPort Port, IParameter Parameter);
public readonly record struct ComponentSpec(Seq<PortSpec> Inputs, Seq<OutputSpec> Outputs) {
    public Seq<IPort> InputPorts => Inputs.Map(static spec => spec.Port);
    public Seq<OutputGroup> OutputGroups => Outputs.Map(static spec => spec.Group);
    public Seq<IPort> OutputPorts => OutputGroups.Bind(static group => group.Ports);
    public static ComponentSpec Of(Seq<IPort> inputs, Seq<OutputGroup> outputs) =>
        new(Inputs: inputs.Map(static port => new PortSpec(Port: port)), Outputs: outputs.Map(static group => new OutputSpec(Group: group)));
}
public readonly record struct GrasshopperRuntime(IDataAccess Access, Analyze.Scope Scope, Hints Hints, IProgress<double> Progress, CancellationToken Cancellation) {
    internal static Fin<GrasshopperRuntime> Capture(IDataAccess access, Seq<BoundPort> inputs, ComponentParameters parameters) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: parameters);
        return access.Scope().Map(scope => new GrasshopperRuntime(
            Access: access,
            Scope: scope,
            Hints: Hints.Capture(ports: inputs, index: parameters.IndexOfInput),
            Progress: new Bridge.Progress(access: access),
            Cancellation: access.Solution.Token));
    }
    internal Fin<Seq<Flow<Shape>>> Shape(Port<Shape> port) {
        IDataAccess access = Access;
        return Hints.Slot(port: port)
            .ToFin(new GrasshopperFault.InputRequired(PortName: port.Name))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

// --- [COMPOSITION] ----------------------------------------------------------------------
public abstract class Component(Type self, ComponentSpec spec) : Grasshopper2.Components.ModularComponent(nomen: (self ?? throw new ArgumentNullException(paramName: nameof(self))).GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: self.Name, info: string.Empty)) {
    private Seq<BoundPort> cachedInputs = Seq<BoundPort>();
    private Seq<BoundPort> cachedOutputs = Seq<BoundPort>();
    private Type Self { get; } = self;
    public ComponentSpec Spec => spec;
    protected override IIcon IconInternal => IconAttribute.Resolve(owner: Self, fallback: base.IconInternal);
    protected override void AddInputs(ModularInputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        cachedInputs = spec.Inputs.Map(pair => new BoundPort(
            Port: pair.Port,
            Parameter: pair.Port.Kind.Bind(adder: inputs, name: pair.Port.Name, code: pair.Port.Code, info: pair.Port.Info, access: pair.Port.Access, requirement: pair.Port.Requirement, policy: pair.Port.Policy, hidden: pair.Hidden)));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        cachedOutputs = spec.Outputs.Bind(pair => pair.Group.Ports.Map(port => new BoundPort(
            Port: port,
            Parameter: port.Kind.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy, hidden: pair.Hidden))));
    }
    protected override void PreProcess(Solution solution) {
        base.PreProcess(solution: solution);
        OnBeforeSolve(solution: solution);
    }
    protected override void PostProcess(Solution solution, FleetingCustomData customData) {
        OnAfterSolve(solution: solution);
        base.PostProcess(solution: solution, customData: customData);
    }
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints outputs = Hints.Capture(ports: cachedOutputs, index: Parameters.IndexOfOutput);
        _ = GrasshopperRuntime.Capture(access: access, inputs: cachedInputs, parameters: Parameters)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, groups: spec.OutputGroups, outputs: outputs),
                Fail: error => {
                    access.AddWarning(text: error.Category(), details: error.Message);
                    return Output.Empty(access: access, groups: spec.OutputGroups, outputs: outputs);
                });
    }
    protected virtual void OnBeforeSolve(Solution solution) { }
    protected virtual void OnAfterSolve(Solution solution) { }
}

using System.Collections.Concurrent;
using System.Reflection;

namespace Rasm.Grasshopper;

/// <summary>
/// Marks an instance <see cref="Port{TVal}"/> field as a component input, declaring its registration order.
/// <see cref="ComponentManifest"/> discovers every <see cref="InputAttribute"/>-decorated field, sorts by <see cref="Order"/>,
/// and binds each to GH2's <see cref="InputAdder"/> via the port's <see cref="PortKind"/>. Order values must be unique
/// within a component; ordinals do not need to be contiguous but ascending integers from zero are the canonical convention.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class InputAttribute(int order) : Attribute {
    public int Order { get; } = order;
}
/// <summary>
/// Marks an instance property returning <see cref="IOutputGroup"/> as a component output group, declaring its registration order.
/// <see cref="ComponentManifest"/> discovers every <see cref="OutputAttribute"/>-decorated property, sorts by <see cref="Order"/>,
/// and binds each group's ports to GH2's <see cref="OutputAdder"/>. Order values must be unique within a component.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OutputAttribute(int order) : Attribute {
    public int Order { get; } = order;
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
/// <summary>
/// Per-Type cache of a component's reflective declaration: <see cref="InputAttribute"/>-decorated <see cref="Port{TVal}"/> fields
/// and <see cref="OutputAttribute"/>-decorated <see cref="IOutputGroup"/>-returning properties, each pre-sorted by declared order.
/// One manifest is built on first observation of a component type and reused for every subsequent instance.
/// </summary>
internal sealed record ComponentManifest(Seq<FieldInfo> InputFields, Seq<PropertyInfo> OutputProperties) {
    private static readonly ConcurrentDictionary<Type, ComponentManifest> cache = new();
    public static ComponentManifest For(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return cache.GetOrAdd(key: type, valueFactory: static probed => Build(type: probed));
    }
    public Seq<IPort> ReadInputs(object instance) {
        ArgumentNullException.ThrowIfNull(argument: instance);
        return InputFields.Map(field => (IPort)field.GetValue(obj: instance)!);
    }
    public Seq<IOutputGroup> ReadOutputs(object instance) {
        ArgumentNullException.ThrowIfNull(argument: instance);
        return OutputProperties.Map(property => (IOutputGroup)property.GetValue(obj: instance)!);
    }
    private static ComponentManifest Build(Type type) {
        const BindingFlags instanceMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        Seq<FieldInfo> fields = toSeq(type.GetFields(bindingAttr: instanceMembers))
            .Choose(field => Optional(field.GetCustomAttribute<InputAttribute>())
                .Map(attribute => (attribute.Order, Field: field)))
            .OrderBy(static pair => pair.Order)
            .AsIterable()
            .Map(static pair => pair.Field)
            .ToSeq();
        Seq<PropertyInfo> properties = toSeq(type.GetProperties(bindingAttr: instanceMembers))
            .Choose(property => Optional(property.GetCustomAttribute<OutputAttribute>())
                .Map(attribute => (attribute.Order, Property: property)))
            .OrderBy(static pair => pair.Order)
            .AsIterable()
            .Map(static pair => pair.Property)
            .ToSeq();
        return new ComponentManifest(InputFields: fields, OutputProperties: properties);
    }
}
/// <summary>
/// Polymorphic base for Grasshopper 2 components. Subclasses declare inputs as instance <see cref="Port{TVal}"/> fields
/// decorated with <see cref="InputAttribute"/> and outputs as instance properties decorated with <see cref="OutputAttribute"/>
/// returning <see cref="IOutputGroup"/>. The component lifecycle (<see cref="AddInputs"/>, <see cref="AddOutputs"/>,
/// <see cref="Process"/>) is driven by the cached <see cref="ComponentManifest"/>; subclasses may override these methods
/// to add variable-parameter behavior on top of the manifest-driven defaults.
/// </summary>
public abstract class Component<TSelf> : Grasshopper2.Components.Component
    where TSelf : Component<TSelf> {
    private Seq<IPort> inputs = Seq<IPort>();
    private Seq<IOutputGroup> outputs = Seq<IOutputGroup>();
    protected Component() : base(nomen: typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty)) { }
    protected override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        this.inputs = ComponentManifest.For(type: typeof(TSelf)).ReadInputs(instance: this);
        _ = this.inputs.Iter(port => port.Kind.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement, policy: port.Policy));
    }
    protected override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        this.outputs = ComponentManifest.For(type: typeof(TSelf)).ReadOutputs(instance: this);
        _ = this.outputs.Bind(static group => group.Ports).Iter(port => port.Kind.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy));
    }
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = GrasshopperRuntime.Capture(access: access, inputs: inputs)
            .Map(runtime => Output.Write(access: access, runtime: runtime, groups: outputs))
            .Match(
                Succ: static _ => Unit.Default,
                Fail: error => (fun((IDataAccess target) => { target.AddError(text: "Input", details: error.Message); return Unit.Default; })(access), Output.Empty(access: access, groups: outputs)).Item2);
    }
}

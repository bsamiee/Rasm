using System.Reflection;
using Grasshopper2.Doc;
using Grasshopper2.UI.Icon;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
[AttributeUsage(validOn: AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class InputAttribute : Attribute;
[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OutputAttribute : Attribute;
[AttributeUsage(validOn: AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HiddenAttribute : Attribute;
[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IconAttribute(string name) : Attribute {
    public string Name { get; } = name;
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct GrasshopperRuntime(IDataAccess Access, Analyze.Scope Scope, Hints Hints) {
    public static Fin<GrasshopperRuntime> Capture(IDataAccess access, Seq<IPort> inputs) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Scope().Map(scope => new GrasshopperRuntime(Access: access, Scope: scope, Hints: Hints.Capture(inputs: inputs)));
    }
    internal Fin<Pear<Shape>> Shape(IDataAccess access, Port<Shape> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Hints.Slot(port: port)
            .ToFin(new Fault.InputRequired(PortName: port.Name))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

// --- [SERVICES] -------------------------------------------------------------------------
internal sealed record ComponentManifest(Seq<(FieldInfo Field, bool Hidden)> InputFields, Seq<(PropertyInfo Property, bool Hidden)> OutputProperties) {
    private static readonly AtomHashMap<Type, ComponentManifest> cache = AtomHashMap<Type, ComponentManifest>();
    public static ComponentManifest For(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return cache.Find(type).IfNone(() => {
            ComponentManifest built = Build(type: type);
            _ = cache.AddOrUpdate(type, built);
            return built;
        });
    }
    public Seq<(IPort Port, bool Hidden)> ReadInputs(object instance) {
        ArgumentNullException.ThrowIfNull(argument: instance);
        return InputFields.Map(pair => ((IPort)pair.Field.GetValue(obj: instance)!, pair.Hidden));
    }
    public Seq<(IOutputGroup Group, bool Hidden)> ReadOutputs(object instance) {
        ArgumentNullException.ThrowIfNull(argument: instance);
        return OutputProperties.Map(pair => ((IOutputGroup)pair.Property.GetValue(obj: instance)!, pair.Hidden));
    }
    private static ComponentManifest Build(Type type) {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        return new ComponentManifest(
            InputFields: Members<FieldInfo, InputAttribute>(members: type.GetFields(bindingAttr: flags)),
            OutputProperties: Members<PropertyInfo, OutputAttribute>(members: type.GetProperties(bindingAttr: flags)));
    }
    private static Seq<(TMember Member, bool Hidden)> Members<TMember, TAttr>(TMember[] members)
        where TMember : MemberInfo
        where TAttr : Attribute =>
        toSeq(members)
            .Choose(m => Optional(m.GetCustomAttribute<TAttr>())
                .Map(_ => (Token: m.MetadataToken, Member: m, Hidden: m.IsDefined(attributeType: typeof(HiddenAttribute), inherit: false))))
            .OrderBy(static t => t.Token)
            .AsIterable()
            .Map(static t => (t.Member, t.Hidden))
            .ToSeq();
}

// --- [COMPOSITION] ----------------------------------------------------------------------
public abstract class Component(Type self) : Grasshopper2.Components.ModularComponent(nomen: (self ?? throw new ArgumentNullException(paramName: nameof(self))).GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: self.Name, info: string.Empty)) {
    private Seq<IPort> inputs = Seq<IPort>();
    private Seq<IOutputGroup> outputs = Seq<IOutputGroup>();
    private Type Self { get; } = self;
    protected override IIcon IconInternal =>
        Self.GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: Self),
            _ => base.IconInternal,
        };
    protected override void AddInputs(ModularInputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        Seq<(IPort Port, bool Hidden)> pairs = ComponentManifest.For(type: Self).ReadInputs(instance: this);
        this.inputs = pairs.Map(static pair => pair.Port);
        _ = pairs.Iter(pair => pair.Port.Kind.Bind(adder: inputs.RegularAdder, name: pair.Port.Name, code: pair.Port.Code, info: pair.Port.Info, access: pair.Port.Access, requirement: pair.Port.Requirement, policy: pair.Port.Policy, hidden: pair.Hidden));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        Seq<(IOutputGroup Group, bool Hidden)> pairs = ComponentManifest.For(type: Self).ReadOutputs(instance: this);
        this.outputs = pairs.Map(static pair => pair.Group);
        _ = pairs.Iter(pair => pair.Group.Ports.Iter(port => port.Kind.Bind(adder: outputs.RegularAdder, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy, hidden: pair.Hidden)));
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
        _ = GrasshopperRuntime.Capture(access: access, inputs: inputs)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, groups: outputs),
                Fail: error => {
                    access.AddWarning(text: error.Category(), details: error.Message);
                    return Output.Empty(access: access, groups: outputs);
                });
    }
    protected virtual void OnBeforeSolve(Solution solution) { }
    protected virtual void OnAfterSolve(Solution solution) { }
}

using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using GhPlugin = Grasshopper2.Framework.Plugin;

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
public readonly record struct ComponentItem<T>(T Value, bool Hidden = false);
internal readonly record struct BoundPort(Port Port, IParameter Parameter);
internal interface IRasmComponent {
    public ComponentSpec Spec { get; }
}
public readonly record struct ComponentSpec(Seq<ComponentItem<Port>> Inputs, Seq<ComponentItem<OutputBinding>> Outputs) {
    public Seq<Port> InputPorts => Inputs.Map(static spec => spec.Value);
    public Seq<OutputBinding> OutputBindings => Outputs.Map(static spec => spec.Value);
    public Seq<Port> OutputPorts => OutputBindings.Map(static binding => binding.Port);
    public static ComponentSpec Of(Seq<Port> inputs, Seq<OutputBinding> outputs) =>
        new(Inputs: inputs.Map(static port => new ComponentItem<Port>(Value: port)), Outputs: outputs.Map(static binding => new ComponentItem<OutputBinding>(Value: binding)));
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
            .ToFin(new MissingPortInput(Port: port.Name))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

// --- [COMPOSITION] ----------------------------------------------------------------------
[BoundaryAdapter]
public abstract class Plugin : GhPlugin {
    private readonly string author;
    private readonly string copyright;
    protected Plugin(Guid id, Nomen nomen, Version? version) : base(id: id, nomen: nomen, version: version) {
        Type self = GetType();
        author = self.GetCustomAttribute<Grasshopper2.AuthorAttribute>()?.Author ?? string.Empty;
        copyright = self.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
    }
    public override string Author => author;
    public override string Copyright => copyright;
    public override IIcon Icon => IconAttribute.Resolve(owner: GetType(), fallback: base.Icon);
    public override void OnLoaded() {
        base.OnLoaded();
        Seq<Type> types = toSeq(GetType().Assembly.GetTypes());
        Seq<Type> plugins = types.Filter(static type => typeof(Plugin).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition).Distinct();
        Seq<Type> components = types
            .Filter(static type => typeof(IRasmComponent).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Distinct();
        Seq<string> faults = ValidateCatalog(active: this, plugins: plugins, components: components)
            .Concat(components.Bind(Validate));
        _ = faults.IsEmpty ? Unit.Default : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static Seq<string> ValidateCatalog(Plugin active, Seq<Type> plugins, Seq<Type> components) {
        Type activeType = active.GetType();
        Option<Guid> IoIdOf(Type type) => Optional(type.GetCustomAttribute<IoIdAttribute>(inherit: false)).Map(static attr => attr.Id);
        Option<Nomen> NomenOf(Type type) => Optional(type.GetCustomAttribute<NomenAttribute>(inherit: false)).Map(static attr => attr.Nomen);
        Seq<string> DuplicateTypes(Seq<Type> candidates, string key, Func<Type, string> project) =>
            toSeq(candidates.Map(type => (Type: type, Key: project(arg: type))).Filter(static item => !string.IsNullOrWhiteSpace(value: item.Key))
                .GroupBy(keySelector: static item => item.Key, comparer: StringComparer.Ordinal)
                .Where(static group => group.Skip(1).Any())
                .Select(group => $"duplicate {key} '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static item => item.Type.FullName))}"));
        Seq<Type> owners = plugins.Concat(components).ToSeq();
        return Seq<Option<string>>(
                plugins.Count == 1 ? Option<string>.None : Some($"{activeType.Assembly.GetName().Name}: expected one plugin type, found {plugins.Count}"),
                plugins.Exists(type => type == activeType) ? Option<string>.None : Some($"{activeType.FullName}: active plugin is not the assembly plugin type"),
                IoIdOf(type: activeType).Filter(id => id == active.Id).IsSome ? Option<string>.None : Some($"{activeType.FullName}: plugin Id does not match IoId"))
            .Choose(identity)
            .Concat(owners.Choose(type => IoIdOf(type: type).IsSome ? Option<string>.None : Some($"{type.FullName}: missing IoId")))
            .Concat(DuplicateTypes(candidates: owners, key: "IoId", project: type => IoIdOf(type: type).Map(static id => id.ToString()).IfNone(string.Empty)))
            .Concat(components.Choose(type => NomenOf(type: type).IsSome ? Option<string>.None : Some($"{type.FullName}: missing Nomen")))
            .Concat(DuplicateTypes(candidates: components, key: "Nomen", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Section}/{n.Name}").IfNone(string.Empty)))
            .Concat(DuplicateTypes(candidates: components, key: "category/name", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Name}").IfNone(string.Empty)));
    }
    private static Seq<string> Validate(Type spec) {
        IRasmComponent probe = (IRasmComponent)Activator.CreateInstance(type: spec)!;
        Seq<Port> inputs = probe.Spec.Inputs.Choose(static pair => Optional(pair.Value));
        Seq<Port> outputs = probe.Spec.Outputs.Choose(static pair => Optional(pair.Value)).Map(static binding => binding.Port);
        Seq<(string Side, Seq<Port> Ports)> sides = Seq((Side: "input", Ports: inputs), (Side: "output", Ports: outputs));
        Seq<string> structural = toSeq(Seq(
            ClosedComponentSelf(type: spec) ? null : $"{spec.FullName}: Component<TSelf> does not match concrete component type",
            inputs.IsEmpty ? $"{spec.FullName}: Inputs is empty" : null,
            outputs.IsEmpty ? $"{spec.FullName}: Outputs is empty" : null).Choose(Optional));
        Seq<string> sourceFaults = probe.Spec.Outputs.Choose(pair => Optional(pair.Value)
            .Bind(binding => inputs.Exists(input => ReferenceEquals(objA: input, objB: binding.Input))
                ? Option<string>.None
                : Some($"{spec.FullName}: output input '{binding.Input.Name}' is not a declared input port instance")));
        Seq<string> nullFaults = NullsAt(spec: spec, side: "input", count: probe.Spec.Inputs.Count, missing: i => probe.Spec.Inputs[i].Value is null)
            .Concat(NullsAt(spec: spec, side: "output", count: probe.Spec.Outputs.Count, missing: i => probe.Spec.Outputs[i].Value is null));
        Seq<string> duplicates = sides.Bind(side =>
            Duplicates(spec: spec, side: side.Side, ports: side.Ports, key: "code", project: static port => port.Code, label: static port => port.Name)
                .Concat(Duplicates(spec: spec, side: side.Side, ports: side.Ports, key: "name", project: static port => port.Name, label: static port => port.Code)));
        Seq<string> portCount = outputs.Count > 24 ? Seq($"{spec.FullName}: output port count {outputs.Count} exceeds 24") : Seq<string>();
        Seq<string> codeLengths = sides.Bind(side => side.Ports.Choose(port =>
            port.Code.Length is > 0 and <= 2 ? Option<string>.None : Some($"{spec.FullName}: {side.Side} port '{port.Name}' code '{port.Code}' must be 1-2 characters")));
        return structural.Concat(sourceFaults).Concat(nullFaults).Concat(duplicates).Concat(portCount).Concat(codeLengths).ToSeq();
    }
    private static bool ClosedComponentSelf(Type type) =>
        type.BaseType switch {
            Type parent when parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(Component<>) => parent.GetGenericArguments()[0] == type,
            Type parent => ClosedComponentSelf(type: parent),
            _ => false,
        };
    private static Seq<string> NullsAt(Type spec, string side, int count, Func<int, bool> missing) =>
        toSeq(Enumerable.Range(start: 0, count: count).Where(predicate: missing.Invoke).Select(index => $"{spec.FullName}: {side} {index} is null"));
    private static Seq<string> Duplicates(Type spec, string side, Seq<Port> ports, string key, Func<Port, string> project, Func<Port, string> label) =>
        toSeq(ports.GroupBy(keySelector: project, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(group => $"{spec.FullName}: duplicate {side} port {key} '{group.Key}' on {string.Join(separator: ", ", values: group.Select(label))}"));
}
public abstract class Component<TSelf>(ComponentSpec spec) : Grasshopper2.Components.ModularComponent(nomen: Self.GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: Self.Name, info: string.Empty)), IRasmComponent where TSelf : Component<TSelf> {
    private Seq<BoundPort> cachedInputs = Seq<BoundPort>();
    private Seq<BoundPort> cachedOutputs = Seq<BoundPort>();
    private static Type Self => typeof(TSelf);
    public ComponentSpec Spec => spec;
    protected override IIcon IconInternal => IconAttribute.Resolve(owner: Self, fallback: base.IconInternal);
    protected override void AddInputs(ModularInputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        cachedInputs = spec.Inputs.Map(pair => new BoundPort(
            Port: pair.Value,
            Parameter: pair.Value.Kind.Bind(adder: inputs, name: pair.Value.Name, code: pair.Value.Code, info: pair.Value.Info, access: pair.Value.Access, requirement: pair.Value.Requirement, policy: pair.Value.Policy, hidden: pair.Hidden)));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        cachedOutputs = spec.Outputs.Map(pair => new BoundPort(
            Port: pair.Value.Port,
            Parameter: pair.Value.Port.Kind.Bind(adder: outputs, name: pair.Value.Port.Name, code: pair.Value.Port.Code, info: pair.Value.Port.Info, access: pair.Value.Port.Access, policy: pair.Value.Port.Policy, hidden: pair.Hidden)));
    }
    protected override void PreProcess(Solution solution) {
        base.PreProcess(solution: solution);
        OnPreProcess(solution: solution);
    }
    protected override void PostProcess(Solution solution, FleetingCustomData customData) {
        OnPostProcess(solution: solution);
        base.PostProcess(solution: solution, customData: customData);
    }
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints outputs = Hints.Capture(ports: cachedOutputs, index: Parameters.IndexOfOutput);
        _ = GrasshopperRuntime.Capture(access: access, inputs: cachedInputs, parameters: Parameters)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, bindings: spec.OutputBindings, outputs: outputs),
                Fail: error => {
                    access.AddWarning(text: error.Category(), details: error.Message);
                    return Output.Empty(access: access, bindings: spec.OutputBindings, outputs: outputs);
                });
    }
    protected virtual void OnPreProcess(Solution solution) { }
    protected virtual void OnPostProcess(Solution solution) { }
}

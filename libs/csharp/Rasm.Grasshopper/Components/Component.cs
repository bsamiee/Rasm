using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using GrasshopperIO;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ------------------------------------------------------------------------------
[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IconAttribute(string name) : Attribute {
    public string Name { get; } = name;
    internal static IIcon Resolve(Type owner, IIcon fallback) =>
        owner.GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: owner),
            _ => fallback,
        };
}
public interface IComponentDefinition<TSelf> where TSelf : IComponentDefinition<TSelf> {
    public static abstract ComponentSpec Definition { get; }
}
internal interface IRasmComponent {
    public ComponentSpec Spec { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct ComponentItem<T>(T Value, bool Hidden = false);

public readonly record struct ComponentSpec(
    Seq<ComponentItem<Port>> Inputs,
    Seq<ComponentItem<OutputBinding>> Outputs,
    NameIconMode IconMode = NameIconMode.Application,
    ThreadingState Threading = ThreadingState.MultiThreaded,
    ComponentUi Ui = default) {
    internal const int MaxOutputs = 24; // Rasm grip-layout/UX convention, not a native OutputAdder cap.
    internal const int CodeMin = 1;
    internal const int CodeMax = 2;
    public static ComponentSpec Define(Func<SpecBuilder, SpecBuilder> configure) {
        ArgumentNullException.ThrowIfNull(argument: configure);
        return configure(arg: SpecBuilder.Empty).Build();
    }
}

public sealed record SpecBuilder {
    public static SpecBuilder Empty => new();
    private Seq<ComponentItem<Port>> Inputs { get; init; }
    private Seq<ComponentItem<OutputBinding>> Outputs { get; init; }
    private NameIconMode IconMode { get; init; } = NameIconMode.Application;
    private ThreadingState ThreadingMode { get; init; } = ThreadingState.MultiThreaded;
    private ComponentUi Ui { get; init; } = ComponentUi.Empty;
    public SpecBuilder Input(Port port, bool hidden = false) {
        ArgumentNullException.ThrowIfNull(argument: port);
        return this with { Inputs = Inputs.Add(value: new ComponentItem<Port>(Value: port, Hidden: hidden)) };
    }
    public SpecBuilder Output<TOut>(Port<Shape> input, IAspect aspect, string name, string code, string info, Access access = Access.Item, Capability? policy = null, bool hidden = false) where TOut : notnull =>
        Output(binding: OutputBinding.Of<TOut>(input: input, aspect: aspect, name: name, code: code, info: info, access: access, policy: policy), hidden: hidden);
    public SpecBuilder Output(OutputBinding binding, bool hidden = false) {
        ArgumentNullException.ThrowIfNull(argument: binding);
        return this with { Outputs = Outputs.Add(value: new ComponentItem<OutputBinding>(Value: binding, Hidden: hidden)) };
    }
    public SpecBuilder Threading(ThreadingState threading) => this with { ThreadingMode = threading };
    public SpecBuilder Icon(NameIconMode mode) => this with { IconMode = mode };
    public SpecBuilder Behaviour(ComponentUi ui) => this with { Ui = Ui + ui };
    internal ComponentSpec Build() {
        Seq<ComponentItem<Port>> declared = Inputs
            .Concat(Outputs.Choose(static output => Optional(output.Value).Map(static binding => new ComponentItem<Port>(Value: binding.Input))))
            .Fold(Seq<ComponentItem<Port>>(), static (found, item) => found.Exists(input => ReferenceEquals(objA: input.Value, objB: item.Value)) switch {
                true => found,
                false => item.Cons(found),
            })
            .Rev();
        return new ComponentSpec(Inputs: declared, Outputs: Outputs, IconMode: IconMode, Threading: ThreadingMode, Ui: Ui);
    }
}

internal readonly record struct GrasshopperRuntime(IDataAccess Access, Analyze.Scope Scope, Hints Hints, IProgress<double> Progress, CancellationToken Cancellation) {
    internal static Fin<GrasshopperRuntime> Capture(IDataAccess access, Seq<(Port Port, IParameter Parameter)> inputs, ComponentParameters parameters) {
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
            .ToFin(new PortFault.MissingInput(Port: port.Name, Hint: None))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record ComponentValidationFault : Domain.Expected {
    private ComponentValidationFault() { }
    public sealed record MissingDefinition : ComponentValidationFault;
    public sealed record ClosedSelfMismatch : ComponentValidationFault;
    public sealed record EmptyPorts(Side Side) : ComponentValidationFault;
    public sealed record NullPort(Side Side, int Index) : ComponentValidationFault;
    public sealed record DuplicateCode(Side Side, string PortCode, string Ports) : ComponentValidationFault;
    public sealed record DuplicateName(Side Side, string Name, string Ports) : ComponentValidationFault;
    public sealed record UnsupportedKind(Side Side, string Port, string Kind) : ComponentValidationFault;
    public sealed record CodeLength(Side Side, string Port, string PortCode) : ComponentValidationFault;
    public sealed record PortCountExceeded(int Found, int Max) : ComponentValidationFault;
    public sealed record SourceNotDeclared(string Input) : ComponentValidationFault;
    public sealed record IncompatibleCapability(Side Side, string Port, string Kind) : ComponentValidationFault;
    public sealed record ComponentFault(string Component, ComponentValidationFault Fault) : ComponentValidationFault;
    public sealed record CatalogPluginCount(string Assembly, int Found) : ComponentValidationFault;
    public sealed record CatalogActivePluginMismatch(string Plugin) : ComponentValidationFault;
    public sealed record CatalogPluginIdMismatch(string Plugin) : ComponentValidationFault;
    public sealed record CatalogSatelliteNulls(string Plugin) : ComponentValidationFault;
    public sealed record CatalogDocumentationMissing(string Plugin, string Folder) : ComponentValidationFault;
    public sealed record CatalogMissingIoId(string Owner) : ComponentValidationFault;
    public sealed record CatalogMissingNomen(string Component) : ComponentValidationFault;
    public sealed record CatalogDuplicateKey(string Kind, string Value, string Owners) : ComponentValidationFault;
    public sealed record PortKindTypeCollision(string ClrType, string Kinds) : ComponentValidationFault;
    public override string Category => "Component";
    public override string Message => Switch(
        missingDefinition: static _ => "missing static ComponentSpec Definition",
        closedSelfMismatch: static _ => "Component<TSelf> does not match concrete component type",
        emptyPorts: static fault => $"{fault.Side} ports are empty",
        nullPort: static fault => $"{fault.Side} {fault.Index} is null",
        duplicateCode: static fault => $"duplicate {fault.Side} port code '{fault.PortCode}' on {fault.Ports}",
        duplicateName: static fault => $"duplicate {fault.Side} port name '{fault.Name}' on {fault.Ports}",
        unsupportedKind: static fault => $"{fault.Side} port '{fault.Port}' kind '{fault.Kind}' cannot register on {fault.Side}",
        codeLength: static fault => $"{fault.Side} port '{fault.Port}' code '{fault.PortCode}' must be {ComponentSpec.CodeMin}-{ComponentSpec.CodeMax} characters",
        portCountExceeded: static fault => $"output port count {fault.Found} exceeds {fault.Max}",
        sourceNotDeclared: static fault => $"output input '{fault.Input}' is not a declared input port instance",
        incompatibleCapability: static fault => $"{fault.Side} port '{fault.Port}' policy is incompatible with kind '{fault.Kind}'",
        componentFault: static fault => $"{fault.Component}: {fault.Fault.Message}",
        catalogPluginCount: static fault => $"{fault.Assembly}: expected one plugin type, found {fault.Found}",
        catalogActivePluginMismatch: static fault => $"{fault.Plugin}: active plugin is not the assembly plugin type",
        catalogPluginIdMismatch: static fault => $"{fault.Plugin}: plugin Id does not match IoId",
        catalogSatelliteNulls: static fault => $"{fault.Plugin}: SatelliteAssemblies contains null entries",
        catalogDocumentationMissing: static fault => $"{fault.Plugin}: DocumentationFolder does not exist: {fault.Folder}",
        catalogMissingIoId: static fault => $"{fault.Owner}: missing IoId",
        catalogMissingNomen: static fault => $"{fault.Component}: missing Nomen",
        catalogDuplicateKey: static fault => $"duplicate {fault.Kind} '{fault.Value}' on {fault.Owners}",
        portKindTypeCollision: static fault => $"PortKind CLR type collision for '{fault.ClrType}' on {fault.Kinds}; declare an explicit PortKind or add an explicit default policy");
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class Component<TSelf> : ModularComponent, IRasmComponent where TSelf : Component<TSelf>, IComponentDefinition<TSelf> {
    private Seq<(Port Port, IParameter Parameter)> cachedInputs;
    private Seq<(Port Port, IParameter Parameter)> cachedOutputs;
    protected Component() : base(nomen: Self.GetCustomAttribute<NomenAttribute>()?.Nomen ?? new Nomen(name: Self.Name, info: string.Empty)) {
        ComponentSpec spec = Spec;
        IconMode = spec.IconMode;
        Threading = spec.Threading;
    }
    private static Type Self => typeof(TSelf);
    public ComponentSpec Spec => TSelf.Definition;
    protected override IIcon IconInternal => IconAttribute.Resolve(owner: Self, fallback: base.IconInternal);
    // BOUNDARY ADAPTER -- GH2 asks for canvas attributes; Rasm installs the component UI rail when specified.
    protected override IAttributes CreateAttributes() => Spec.Ui.Attributes(owner: this);
    public override void AppendToInputPanel(InputPanel panel) {
        base.AppendToInputPanel(panel: panel);
        // BOUNDARY ADAPTER -- GH2 owns the input panel; Rasm appends through the same component UI rail.
        _ = Spec.Ui.Append(owner: this, panel: panel);
    }
    protected override void AddInputs(ModularInputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        cachedInputs = Spec.Inputs.Map(pair => (
            Port: pair.Value,
            Parameter: pair.Value.Kind.Bind(adder: inputs, port: pair.Value, hidden: pair.Hidden)));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        cachedOutputs = Spec.Outputs.Map(pair => (
            pair.Value.Port,
            Parameter: pair.Value.Port.Kind.Bind(adder: outputs, port: pair.Value.Port, hidden: pair.Hidden)));
    }
    protected override void BeforeProcess(Solution solution) {
        base.BeforeProcess(solution: solution);
        OnBeforeProcess(solution: solution);
    }
    protected override void PreProcess(Solution solution) {
        base.PreProcess(solution: solution);
        OnPreProcess(solution: solution);
    }
    protected override void PostProcess(Solution solution, FleetingCustomData customData) {
        OnPostProcess(solution: solution);
        base.PostProcess(solution: solution, customData: customData);
    }
    protected override ITree PostProcessTree(ITree tree, int index, Solution solution) =>
        OnPostProcessTree(tree: base.PostProcessTree(tree: tree, index: index, solution: solution), index: index, solution: solution);
    // BOUNDARY ADAPTER -- GH2 solve is synchronous; cancellation flows through Solution.Token, progress through Bridge.Progress.
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints outputs = Hints.Capture(ports: cachedOutputs, index: Parameters.IndexOfOutput);
        Seq<OutputBinding> bindings = Spec.Outputs.Map(static output => output.Value);
        _ = GrasshopperRuntime.Capture(access: access, inputs: cachedInputs, parameters: Parameters)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, bindings: bindings, outputs: outputs),
                // Wiring faults stay recoverable (Warning + empty); a genuine compute/scope failure surfaces as Error.
                Fail: error => access.Emit(severity: PortFault.SeverityOf(error: error), text: error.Category(), details: error.Message) switch {
                    _ => Output.Empty(access: access, bindings: bindings, outputs: outputs),
                });
    }
    protected virtual void OnBeforeProcess(Solution solution) { }
    protected virtual void OnPreProcess(Solution solution) { }
    protected virtual void OnPostProcess(Solution solution) { }
    protected virtual ITree OnPostProcessTree(ITree tree, int index, Solution solution) => tree;
}

// --- [COMPOSITION] ------------------------------------------------------------------------
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
    public override string Contact => string.Empty;
    public override string Copyright => copyright;
    public override string Website => string.Empty;
    public override string LicenceDescription => string.Empty;
    public override string LicenceAgreement => string.Empty;
    public override IIcon Icon => IconAttribute.Resolve(owner: GetType(), fallback: base.Icon);
    protected override Assembly[] SatelliteAssemblies => [];
    public override void OnLoaded() {
        base.OnLoaded();
        Assembly[] satellites = SatelliteAssemblies;
        Seq<Assembly> declaredSatellites = toSeq(satellites).Choose(static assembly => Optional(assembly));
        Seq<Type> types = declaredSatellites.IsEmpty ? toSeq(GetType().Assembly.GetTypes()) : toSeq(ExportedTypes);
        Seq<Type> plugins = types.Filter(static type => typeof(Plugin).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition).Distinct();
        Seq<Type> components = types
            .Filter(static type => typeof(IRasmComponent).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Distinct();
        Seq<ComponentValidationFault> faults = ValidateCatalog(active: this, plugins: plugins, components: components, satellites: satellites, declaredSatellites: declaredSatellites)
            .Concat(components.Bind(type =>
                (type.GetProperty(name: "Definition", bindingAttr: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(obj: null) switch {
                    ComponentSpec probe => Validate(spec: type, probe: probe),
                    _ => Seq<ComponentValidationFault>(new ComponentValidationFault.MissingDefinition()),
                }).Map(fault => new ComponentValidationFault.ComponentFault(Component: type.FullName ?? type.Name, Fault: fault))));
        // BOUNDARY ADAPTER -- GH2 plugin-load entry; a mis-declared catalog is a fatal developer error surfaced fail-fast at load.
        _ = faults.IsEmpty ? Unit.Default : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults.Map(static fault => fault.Message)));
    }
    private static Seq<ComponentValidationFault> ValidateCatalog(Plugin active, Seq<Type> plugins, Seq<Type> components, Assembly[] satellites, Seq<Assembly> declaredSatellites) {
        Type activeType = active.GetType();
        Option<Guid> IoIdOf(Type type) => Optional(type.GetCustomAttribute<IoIdAttribute>(inherit: false)).Map(static attr => attr.Id);
        Option<Nomen> NomenOf(Type type) => Optional(type.GetCustomAttribute<NomenAttribute>(inherit: false)).Map(static attr => attr.Nomen);
        Seq<ComponentValidationFault> DuplicateTypes(Seq<Type> candidates, string key, Func<Type, string> project) =>
            toSeq(candidates.Map(type => (Type: type, Key: project(arg: type))).Filter(static item => !string.IsNullOrWhiteSpace(value: item.Key))
                .GroupBy(keySelector: static item => item.Key, comparer: StringComparer.Ordinal)
                .Where(static group => group.Skip(1).Any())
                .Select(group => (ComponentValidationFault)new ComponentValidationFault.CatalogDuplicateKey(
                    Kind: key,
                    Value: group.Key,
                    Owners: string.Join(separator: ", ", values: group.Select(static item => item.Type.FullName ?? item.Type.Name)))));
        Seq<Type> owners = plugins.Concat(components).ToSeq();
        bool declaredDocumentation = activeType.GetProperty(name: nameof(DocumentationFolder), bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.DeclaringType switch {
            Type owner => owner != typeof(GhPlugin),
            _ => false,
        };
        // One declarative rule table: each fault is constructed lazily only when its guard fails.
        Seq<(bool Valid, Func<ComponentValidationFault> Fault)> rules = Seq<(bool, Func<ComponentValidationFault>)>(
            (plugins.Count == 1, () => new ComponentValidationFault.CatalogPluginCount(Assembly: activeType.Assembly.GetName().Name ?? activeType.Name, Found: plugins.Count)),
            (plugins.Exists(type => type == activeType), () => new ComponentValidationFault.CatalogActivePluginMismatch(Plugin: activeType.FullName ?? activeType.Name)),
            (IoIdOf(type: activeType).Filter(id => id == active.Id).IsSome, () => new ComponentValidationFault.CatalogPluginIdMismatch(Plugin: activeType.FullName ?? activeType.Name)),
            (declaredSatellites.Count == satellites.Length, () => new ComponentValidationFault.CatalogSatelliteNulls(Plugin: activeType.FullName ?? activeType.Name)),
            (!(declaredDocumentation && !string.IsNullOrWhiteSpace(value: active.DocumentationFolder) && !Directory.Exists(path: active.DocumentationFolder)),
                () => new ComponentValidationFault.CatalogDocumentationMissing(Plugin: activeType.FullName ?? activeType.Name, Folder: active.DocumentationFolder)));
        return rules.Choose(static rule => rule.Valid ? Option<ComponentValidationFault>.None : Some(rule.Fault()))
            .Concat(owners.Choose(type => IoIdOf(type: type).IsSome
                ? Option<ComponentValidationFault>.None
                : Some<ComponentValidationFault>(new ComponentValidationFault.CatalogMissingIoId(Owner: type.FullName ?? type.Name))))
            .Concat(DuplicateTypes(candidates: owners, key: "IoId", project: type => IoIdOf(type: type).Map(static id => id.ToString()).IfNone(string.Empty)))
            .Concat(components.Choose(type => NomenOf(type: type).IsSome
                ? Option<ComponentValidationFault>.None
                : Some<ComponentValidationFault>(new ComponentValidationFault.CatalogMissingNomen(Component: type.FullName ?? type.Name))))
            .Concat(DuplicateTypes(candidates: components, key: "Nomen", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Section}/{n.Name}").IfNone(string.Empty)))
            .Concat(DuplicateTypes(candidates: components, key: "category/name", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Name}").IfNone(string.Empty)))
            .Concat(PortKind.TypeCollisions.Map(static collision => (ComponentValidationFault)new ComponentValidationFault.PortKindTypeCollision(
                ClrType: collision.Type.FullName ?? collision.Type.Name,
                Kinds: string.Join(separator: ", ", values: collision.Kinds.Map(static kind => kind.ToString())))));
    }
    private static Seq<ComponentValidationFault> Validate(Type spec, ComponentSpec probe) {
        Seq<Port> inputs = probe.Inputs.Choose(static pair => Optional(pair.Value));
        Seq<Port> outputs = probe.Outputs.Choose(static pair => Optional(pair.Value)).Map(static binding => binding.Port);
        Seq<(Side Side, Seq<Port> Ports)> sides = Seq((Side: Side.Input, Ports: inputs), (Side: Side.Output, Ports: outputs));
        Seq<ComponentValidationFault> structural = Seq(
            ClosedComponentSelf(type: spec) ? Option<ComponentValidationFault>.None : Some<ComponentValidationFault>(new ComponentValidationFault.ClosedSelfMismatch()),
            inputs.IsEmpty ? Some<ComponentValidationFault>(new ComponentValidationFault.EmptyPorts(Side: Side.Input)) : Option<ComponentValidationFault>.None,
            outputs.IsEmpty ? Some<ComponentValidationFault>(new ComponentValidationFault.EmptyPorts(Side: Side.Output)) : Option<ComponentValidationFault>.None,
            outputs.Count > ComponentSpec.MaxOutputs ? Some<ComponentValidationFault>(new ComponentValidationFault.PortCountExceeded(Found: outputs.Count, Max: ComponentSpec.MaxOutputs)) : Option<ComponentValidationFault>.None)
            .Choose(identity);
        Seq<ComponentValidationFault> sourceFaults = probe.Outputs.Choose(pair => Optional(pair.Value)
            .Bind(binding => inputs.Exists(input => ReferenceEquals(objA: input, objB: binding.Input))
                ? Option<ComponentValidationFault>.None
                : Some<ComponentValidationFault>(new ComponentValidationFault.SourceNotDeclared(Input: binding.Input.Name))));
        Seq<ComponentValidationFault> nullFaults = NullsAt(side: Side.Input, count: probe.Inputs.Count, missing: index => probe.Inputs[index].Value is null)
            .Concat(NullsAt(side: Side.Output, count: probe.Outputs.Count, missing: index => probe.Outputs[index].Value is null));
        Seq<ComponentValidationFault> duplicates = sides.Bind(side =>
            Duplicates(side: side.Side, ports: side.Ports, project: static port => port.Code, label: static port => port.Name, make: static (side, key, ports) => new ComponentValidationFault.DuplicateCode(Side: side, PortCode: key, Ports: ports))
                .Concat(Duplicates(side: side.Side, ports: side.Ports, project: static port => port.Name, label: static port => port.Code, make: static (side, key, ports) => new ComponentValidationFault.DuplicateName(Side: side, Name: key, Ports: ports))));
        Seq<ComponentValidationFault> sideSupport = sides.Bind(side => side.Ports.Choose(port =>
            port.Kind.Supports(side: side.Side) ? Option<ComponentValidationFault>.None : Some<ComponentValidationFault>(new ComponentValidationFault.UnsupportedKind(Side: side.Side, Port: port.Name, Kind: port.Kind.ToString()))));
        Seq<ComponentValidationFault> codeLengths = sides.Bind(side => side.Ports.Choose(port =>
            port.Code.Length is >= ComponentSpec.CodeMin and <= ComponentSpec.CodeMax ? Option<ComponentValidationFault>.None : Some<ComponentValidationFault>(new ComponentValidationFault.CodeLength(Side: side.Side, Port: port.Name, PortCode: port.Code))));
        Seq<ComponentValidationFault> capabilityFaults = sides.Bind(side => side.Ports.Choose(port =>
            port.Policy.CompatibleWith(kind: port.Kind)
                ? Option<ComponentValidationFault>.None
                : Some<ComponentValidationFault>(new ComponentValidationFault.IncompatibleCapability(Side: side.Side, Port: port.Name, Kind: port.Kind.ToString()))));
        return structural.Concat(sourceFaults).Concat(nullFaults).Concat(duplicates).Concat(sideSupport).Concat(codeLengths).Concat(capabilityFaults).ToSeq();
    }
    private static bool ClosedComponentSelf(Type type) =>
        type.BaseType switch {
            Type parent when parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(Component<>) => parent.GetGenericArguments()[0] == type,
            Type parent => ClosedComponentSelf(type: parent),
            _ => false,
        };
    private static Seq<ComponentValidationFault> NullsAt(Side side, int count, Func<int, bool> missing) =>
        toSeq(Enumerable.Range(start: 0, count: count).Where(predicate: missing.Invoke).Select(index => (ComponentValidationFault)new ComponentValidationFault.NullPort(Side: side, Index: index)));
    private static Seq<ComponentValidationFault> Duplicates(Side side, Seq<Port> ports, Func<Port, string> project, Func<Port, string> label, Func<Side, string, string, ComponentValidationFault> make) =>
        toSeq(ports.GroupBy(keySelector: project, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(group => make(arg1: side, arg2: group.Key, arg3: string.Join(separator: ", ", values: group.Select(label)))));
}
public abstract class Plugin<TSelf> : Plugin where TSelf : Plugin<TSelf> {
    protected Plugin() : base(id: IdOf(), nomen: NomenOf(), version: Self.Assembly.GetName().Version) { }
    private static Type Self => typeof(TSelf);
    private static Guid IdOf() =>
        Optional(Self.GetCustomAttribute<IoIdAttribute>(inherit: false)).Map(static attr => attr.Id).IfNone(Guid.Empty);
    private static Nomen NomenOf() =>
        Optional(Self.GetCustomAttribute<NomenAttribute>(inherit: false))
            .Map(static attr => attr.Nomen)
            .IfNone(() => new Nomen(
                name: Self.Assembly.GetName().Name ?? Self.Name,
                info: Self.Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty));
}

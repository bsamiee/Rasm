using System.Reflection;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GrasshopperIO;
using GhComponent = Grasshopper2.Components.Component;
using GhPlugin = Grasshopper2.Framework.Plugin;
using UiContext = Eto.Drawing.Context;
using UiResponse = Grasshopper2.UI.Flex.Response;
using UiShape = Grasshopper2.UI.Skinning.Shape;

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
public interface IComponentDefinition<TSelf> where TSelf : IComponentDefinition<TSelf> {
    public static abstract ComponentSpec Definition { get; }
}
internal interface IRasmComponent {
    public ComponentSpec Spec { get; }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ComponentUi {
    private readonly Seq<StepOp> ops;

    private ComponentUi(Seq<StepOp> ops) => this.ops = ops;

    public static ComponentUi Empty => default;

    public static ComponentUi operator +(ComponentUi left, ComponentUi right) =>
        Add(left: left, right: right);

    public static ComponentUi Add(ComponentUi left, ComponentUi right) =>
        new(ops: left.ops + right.ops);

    public static ComponentUi Of(Func<Callback, Fin<Decision>> run) =>
        new(ops: Seq(new StepOp(Run: run)));

    public static ComponentUi When(Phase phase, Func<Callback, Fin<Decision>> run) =>
        Of(run: context => context.Kind == phase ? run(arg: context) : Fin.Succ(value: Decision.Pass));

    internal IAttributes Attributes(GhComponent owner) =>
        ops.IsEmpty ? new ComponentAttributes(owner: owner) : new RasmAttributes(owner: owner, ui: this);

    internal Fin<Unit> Append(GhComponent owner, InputPanel panel) =>
        Run(context: new Callback.Panel(Owner: owner, Value: panel)).Map(static _ => unit);

    internal Fin<Decision> Run(Callback context) {
        Seq<StepOp> current = ops;
        return Try.lift<Fin<Decision>>(f: () => current.Fold(Fin.Succ(value: Decision.Pass), (Fin<Decision> state, StepOp op) =>
                state.Bind(decision => decision.IsTerminal ? Fin.Succ(value: decision) : op.Run(arg: context).Map(next => decision + next))))
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Run)).InvalidResult())
            .Bind(static result => result);
    }

    public enum Phase {
        Layout,
        DrawForeground,
        InputPanel,
        ContextMenu,
        Cursor,
        Hover,
        MouseDown,
        MouseMove,
        MouseUp,
        MouseSingleClick,
        MouseDoubleClick,
        KeyDown,
        KeyUp,
    }

    public abstract record Callback(GhComponent Owner) {
        public abstract Phase Kind { get; }

        public sealed record Bounds(GhComponent Owner, UiShape Shape) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.Layout;
        }

        public sealed record Draw(GhComponent Owner, UiContext Context, Skin Skin, Capsule Capsule, Shade Shade) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.DrawForeground;
        }

        public sealed record Panel(GhComponent Owner, InputPanel Value) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.InputPanel;
        }

        public sealed record Menu(GhComponent Owner, ContextMenu Value) : Callback(Owner: Owner) {
            public override Phase Kind => Phase.ContextMenu;
        }

        public sealed record Pointer(Phase Requested, GhComponent Owner, PointF ContentPoint, Option<PointF> ControlPoint = default) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
        }

        public sealed record Mouse(Phase Requested, GhComponent Owner, MouseEventArgs Args) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
            public PointF Point => Args.Location;
        }

        public sealed record Key(Phase Requested, GhComponent Owner, KeyEventArgs Args) : Callback(Owner: Owner) {
            public override Phase Kind => Requested;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Decision(
        Option<RectangleF> Bounds,
        Option<UiResponse> Response,
        Option<Cursor> Cursor,
        Option<bool> Hover,
        bool IsTerminal) {
        public static Decision Pass => default;
        public static Decision Handled => new(Bounds: None, Response: Optional(UiResponse.Handled), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision Capture => new(Bounds: None, Response: Optional(UiResponse.Capture), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision Release => new(Bounds: None, Response: Optional(UiResponse.Release), Cursor: None, Hover: None, IsTerminal: true);
        public static Decision WithBounds(RectangleF bounds) => new(Bounds: Optional(bounds), Response: None, Cursor: None, Hover: None, IsTerminal: false);
        public static Decision WithCursor(Cursor cursor) => new(Bounds: None, Response: None, Cursor: Optional(cursor), Hover: None, IsTerminal: false);
        public static Decision WithHover(bool value) => new(Bounds: None, Response: None, Cursor: None, Hover: Optional(value), IsTerminal: false);

        public static Decision operator +(Decision left, Decision right) =>
            Add(left: left, right: right);

        public static Decision Add(Decision left, Decision right) =>
            new(
                Bounds: right.Bounds | left.Bounds,
                Response: left.Response | right.Response,
                Cursor: right.Cursor | left.Cursor,
                Hover: right.Hover | left.Hover,
                IsTerminal: left.IsTerminal || right.IsTerminal);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct StepOp(Func<Callback, Fin<Decision>> Run);

    [BoundaryAdapter]
    private sealed class RasmAttributes :
        ComponentAttributes,
        IContextMenuAware,
        ICursorAwareAttributes,
        IMouseHoverAttributes {
        private readonly ComponentUi ui;

        internal RasmAttributes(GhComponent owner, ComponentUi ui) : base(owner: owner) =>
            this.ui = ui;

        protected override void LayoutBounds(UiShape shape) {
            base.LayoutBounds(shape: shape);
            _ = ui.Run(context: new Callback.Bounds(Owner: Owner, Shape: shape)).Map(decision => {
                Bounds = decision.Bounds.IfNone(Bounds);
                return unit;
            });
        }

        protected override void DrawForegroundDecorations(UiContext context, Skin skin, Capsule capsule, Shade shade) {
            base.DrawForegroundDecorations(context: context, skin: skin, capsule: capsule, shade: shade);
            _ = ui.Run(context: new Callback.Draw(Owner: Owner, Context: context, Skin: skin, Capsule: capsule, Shade: shade));
        }

        void IContextMenuAware.AppendToMenu(ContextMenu menu) =>
            _ = ui.Run(context: new Callback.Menu(Owner: Owner, Value: menu));

        Cursor ICursorAwareAttributes.CursorAt(PointF point) =>
            ui.Run(context: new Callback.Pointer(Requested: Phase.Cursor, Owner: Owner, ContentPoint: point))
                .Map(decision => decision.Cursor.IfNone(Cursors.Default))
                .IfFail(_ => Cursors.Default);

        bool IMouseHoverAttributes.RespondToMouseHover(PointF controlPoint, PointF contentPoint) =>
            ui.Run(context: new Callback.Pointer(Requested: Phase.Hover, Owner: Owner, ContentPoint: contentPoint, ControlPoint: Optional(controlPoint)))
                .Map(decision => decision.Hover.IfNone(false))
                .IfFail(_ => false);

        protected override UiResponse HandleMouseDown(MouseEventArgs e) => Respond(phase: Phase.MouseDown, mouse: e);
        protected override UiResponse HandleMouseMove(MouseEventArgs e) => Respond(phase: Phase.MouseMove, mouse: e);
        protected override UiResponse HandleMouseUp(MouseEventArgs e) => Respond(phase: Phase.MouseUp, mouse: e);
        protected override UiResponse HandleSingleClick(MouseEventArgs e) => Respond(phase: Phase.MouseSingleClick, mouse: e);
        protected override UiResponse HandleDoubleClick(MouseEventArgs e) => Respond(phase: Phase.MouseDoubleClick, mouse: e);
        protected override UiResponse HandleKeyDown(KeyEventArgs e) => Respond(phase: Phase.KeyDown, key: e);
        protected override UiResponse HandleKeyUp(KeyEventArgs e) => Respond(phase: Phase.KeyUp, key: e);

        private UiResponse Respond(Phase phase, MouseEventArgs mouse) =>
            ui.Run(context: new Callback.Mouse(Requested: phase, Owner: Owner, Args: mouse))
                .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
                .IfFail(_ => UiResponse.Ignored);

        private UiResponse Respond(Phase phase, KeyEventArgs key) =>
            ui.Run(context: new Callback.Key(Requested: phase, Owner: Owner, Args: key))
                .Map(decision => decision.Response.IfNone(UiResponse.Ignored))
                .IfFail(_ => UiResponse.Ignored);
    }
}

public readonly record struct ComponentSpec(Seq<ComponentItem<Port>> Inputs, Seq<ComponentItem<OutputBinding>> Outputs, NameIconMode IconMode = NameIconMode.Application, ThreadingState Threading = ThreadingState.MultiThreaded, ComponentUi Ui = default) {
    public static ComponentSpec Of(Seq<OutputBinding> outputs, Seq<ComponentItem<Port>> inputs = default, NameIconMode iconMode = NameIconMode.Application, ThreadingState threading = ThreadingState.MultiThreaded, ComponentUi ui = default) =>
        Of(
            outputs: outputs.Map(static binding => new ComponentItem<OutputBinding>(Value: binding)),
            inputs: inputs,
            iconMode: iconMode,
            threading: threading,
            ui: ui);
    public static ComponentSpec Of(Seq<ComponentItem<OutputBinding>> outputs, Seq<ComponentItem<Port>> inputs = default, NameIconMode iconMode = NameIconMode.Application, ThreadingState threading = ThreadingState.MultiThreaded, ComponentUi ui = default) {
        Seq<ComponentItem<Port>> declared = inputs
            .Concat(outputs.Choose(static output => Optional(output.Value).Map(static found => new ComponentItem<Port>(Value: found.Input))))
            .Fold(Seq<ComponentItem<Port>>(), static (found, item) => found.Exists(input => ReferenceEquals(objA: input.Value, objB: item.Value)) switch {
                true => found,
                false => item.Cons(found),
            })
            .Rev();
        return new(
            Inputs: declared,
            Outputs: outputs,
            IconMode: iconMode,
            Threading: threading,
            Ui: ui);
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
            .ToFin(new MissingPortInput(Port: port.Name))
            .Bind(slot => access.ReadShape(slot: slot, port: port));
    }
    internal Fin<Seq<Flow<TVal>>> Read<TVal>(Port<TVal> port) {
        IDataAccess access = Access;
        return Hints.Slot(port: port)
            .ToFin(new MissingPortInput(Port: port.Name))
            .Bind(slot => access.Read<TVal>(slot: slot, port: port));
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
        Seq<string> faults = ValidateCatalog(active: this, plugins: plugins, components: components, satellites: satellites, declaredSatellites: declaredSatellites)
            .Concat(components.Bind(Validate));
        _ = faults.IsEmpty ? Unit.Default : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static Seq<string> ValidateCatalog(Plugin active, Seq<Type> plugins, Seq<Type> components, Assembly[] satellites, Seq<Assembly> declaredSatellites) {
        Type activeType = active.GetType();
        Option<Guid> IoIdOf(Type type) => Optional(type.GetCustomAttribute<IoIdAttribute>(inherit: false)).Map(static attr => attr.Id);
        Option<Nomen> NomenOf(Type type) => Optional(type.GetCustomAttribute<NomenAttribute>(inherit: false)).Map(static attr => attr.Nomen);
        Seq<string> DuplicateTypes(Seq<Type> candidates, string key, Func<Type, string> project) =>
            toSeq(candidates.Map(type => (Type: type, Key: project(arg: type))).Filter(static item => !string.IsNullOrWhiteSpace(value: item.Key))
                .GroupBy(keySelector: static item => item.Key, comparer: StringComparer.Ordinal)
                .Where(static group => group.Skip(1).Any())
                .Select(group => $"duplicate {key} '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static item => item.Type.FullName))}"));
        Seq<Type> owners = plugins.Concat(components).ToSeq();
        bool declaredDocumentation = activeType.GetProperty(name: nameof(DocumentationFolder), bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.DeclaringType switch {
            Type owner => owner != typeof(GhPlugin),
            _ => false,
        };
        return Seq<Option<string>>(
                plugins.Count == 1 ? Option<string>.None : Some($"{activeType.Assembly.GetName().Name}: expected one plugin type, found {plugins.Count}"),
                plugins.Exists(type => type == activeType) ? Option<string>.None : Some($"{activeType.FullName}: active plugin is not the assembly plugin type"),
                IoIdOf(type: activeType).Filter(id => id == active.Id).IsSome ? Option<string>.None : Some($"{activeType.FullName}: plugin Id does not match IoId"),
                declaredSatellites.Count == satellites.Length ? Option<string>.None : Some($"{activeType.FullName}: SatelliteAssemblies contains null entries"),
                declaredDocumentation && !string.IsNullOrWhiteSpace(value: active.DocumentationFolder) && !Directory.Exists(path: active.DocumentationFolder) ? Some($"{activeType.FullName}: DocumentationFolder does not exist: {active.DocumentationFolder}") : Option<string>.None)
            .Choose(identity)
            .Concat(owners.Choose(type => IoIdOf(type: type).IsSome ? Option<string>.None : Some($"{type.FullName}: missing IoId")))
            .Concat(DuplicateTypes(candidates: owners, key: "IoId", project: type => IoIdOf(type: type).Map(static id => id.ToString()).IfNone(string.Empty)))
            .Concat(components.Choose(type => NomenOf(type: type).IsSome ? Option<string>.None : Some($"{type.FullName}: missing Nomen")))
            .Concat(DuplicateTypes(candidates: components, key: "Nomen", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Section}/{n.Name}").IfNone(string.Empty)))
            .Concat(DuplicateTypes(candidates: components, key: "category/name", project: type => NomenOf(type: type).Map(static n => $"{n.Chapter}/{n.Name}").IfNone(string.Empty)));
    }
    private static Seq<string> Validate(Type spec) =>
        spec.GetProperty(name: "Definition", bindingAttr: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(obj: null) switch {
            ComponentSpec probe => Validate(spec: spec, probe: probe),
            _ => Seq($"{spec.FullName}: missing static ComponentSpec Definition"),
        };
    private static Seq<string> Validate(Type spec, ComponentSpec probe) {
        Seq<Port> inputs = probe.Inputs.Choose(static pair => Optional(pair.Value));
        Seq<Port> outputs = probe.Outputs.Choose(static pair => Optional(pair.Value)).Map(static binding => binding.Port);
        Seq<(string Label, Side Side, Seq<Port> Ports)> sides = Seq((Label: "input", Side: Side.Input, Ports: inputs), (Label: "output", Side: Side.Output, Ports: outputs));
        Seq<string> structural = toSeq(Seq(
            ClosedComponentSelf(type: spec) ? null : $"{spec.FullName}: Component<TSelf> does not match concrete component type",
            inputs.IsEmpty ? $"{spec.FullName}: Inputs is empty" : null,
            outputs.IsEmpty ? $"{spec.FullName}: Outputs is empty" : null).Choose(Optional));
        Seq<string> sourceFaults = probe.Outputs.Choose(pair => Optional(pair.Value)
            .Bind(binding => inputs.Exists(input => ReferenceEquals(objA: input, objB: binding.Input))
                ? Option<string>.None
                : Some($"{spec.FullName}: output input '{binding.Input.Name}' is not a declared input port instance")));
        Seq<string> nullFaults = NullsAt(spec: spec, side: "input", count: probe.Inputs.Count, missing: i => probe.Inputs[i].Value is null)
            .Concat(NullsAt(spec: spec, side: "output", count: probe.Outputs.Count, missing: i => probe.Outputs[i].Value is null));
        Seq<string> duplicates = sides.Bind(side =>
            Duplicates(spec: spec, side: side.Label, ports: side.Ports, key: "code", project: static port => port.Code, label: static port => port.Name)
                .Concat(Duplicates(spec: spec, side: side.Label, ports: side.Ports, key: "name", project: static port => port.Name, label: static port => port.Code)));
        Seq<string> sideSupport = sides.Bind(side => side.Ports.Choose(port =>
            port.Kind.Supports(side: side.Side) ? Option<string>.None : Some($"{spec.FullName}: {side.Label} port '{port.Name}' kind '{port.Kind}' cannot register on {side.Side}")));
        Seq<string> portCount = outputs.Count > 24 ? Seq($"{spec.FullName}: output port count {outputs.Count} exceeds 24") : Seq<string>();
        Seq<string> codeLengths = sides.Bind(side => side.Ports.Choose(port =>
            port.Code.Length is > 0 and <= 2 ? Option<string>.None : Some($"{spec.FullName}: {side.Label} port '{port.Name}' code '{port.Code}' must be 1-2 characters")));
        return structural.Concat(sourceFaults).Concat(nullFaults).Concat(duplicates).Concat(sideSupport).Concat(portCount).Concat(codeLengths).ToSeq();
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
public abstract class Component<TSelf> : Grasshopper2.Components.ModularComponent, IRasmComponent where TSelf : Component<TSelf>, IComponentDefinition<TSelf> {
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
            Parameter: pair.Value.Kind.Bind(adder: inputs, name: pair.Value.Name, code: pair.Value.Code, info: pair.Value.Info, access: pair.Value.Access, requirement: pair.Value.Requirement, policy: pair.Value.Policy, hidden: pair.Hidden)));
    }
    protected override void AddOutputs(ModularOutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        cachedOutputs = Spec.Outputs.Map(pair => (
            pair.Value.Port,
            Parameter: pair.Value.Port.Kind.Bind(adder: outputs, name: pair.Value.Port.Name, code: pair.Value.Port.Code, info: pair.Value.Port.Info, access: pair.Value.Port.Access, policy: pair.Value.Port.Policy, hidden: pair.Hidden)));
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
    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints outputs = Hints.Capture(ports: cachedOutputs, index: Parameters.IndexOfOutput);
        Seq<OutputBinding> bindings = Spec.Outputs.Map(static output => output.Value);
        _ = GrasshopperRuntime.Capture(access: access, inputs: cachedInputs, parameters: Parameters)
            .Match(
                Succ: runtime => Output.Write(access: access, runtime: runtime, bindings: bindings, outputs: outputs),
                Fail: error => {
                    access.AddWarning(text: error.Category(), details: error.Message);
                    return Output.Empty(access: access, bindings: bindings, outputs: outputs);
                });
    }
    protected virtual void OnBeforeProcess(Solution solution) { }
    protected virtual void OnPreProcess(Solution solution) { }
    protected virtual void OnPostProcess(Solution solution) { }
    protected virtual ITree OnPostProcessTree(ITree tree, int index, Solution solution) => tree;
}

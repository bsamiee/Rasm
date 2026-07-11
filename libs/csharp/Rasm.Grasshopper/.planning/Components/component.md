# [RASM_GRASSHOPPER_COMPONENT]

`ComponentSpec` is the one component declaration: identity, io id, pin roster, execution, lifecycle, threading, pin-visibility flexing, bake capability, fleeting persistence, panel and icon projection, and canvas chrome ride one record, and every consumer — registration, execution, catalogue validation — reads the same declaration. `Execution` discriminates single, batch, aggregate, and heterogeneous input consumption by the step's own payload shape, with case-versus-depth-and-presence coherence proven at admission. `SpecComponent` is the single `ModularComponent` boundary projection over the verified modular-adder lifecycle, and `PluginSpec`/`SpecPlugin` carry registration as rows the catalogue audit consumes unchanged.

## [01]-[INDEX]

- [02]-[EXECUTION]: the `Execution` topology union, the `ProcessScope` seam value, and the receipt-sealing run fold
- [03]-[SPEC]: the `ComponentSpec` declaration, its policy slots, and the accumulating admission
- [04]-[HOST_PROJECTION]: the `SpecComponent` boundary adapter over the `ModularComponent` lifecycle
- [05]-[PLUGIN]: registration rows, the `SpecPlugin` projection, and the catalogue audit
- [06]-[RESEARCH]

## [02]-[EXECUTION]

- Owner: `Execution` is the closed input-consumption family — the step payload's own shape is the discriminant, so a per-item map, a per-branch fold, a whole-tree fold, and a mixed-depth custom read are four cases of one union under one total run fold; `ProcessScope` is the one seam value a step receives, wrapping typed reads, receipted writes, notices, progress, iteration and change evidence, units, cancellation, and the kernel `Op` key.
- Cases: `Single` maps `Seq<IPear>` to `Seq<IPear>`, `Batch` maps twigs, `Aggregate` maps trees, and `Heterogeneous` drives the scope directly.
- Entry: `Executions.Run` is the one dispatch — it gathers every live input at the case's depth through the verified `CountIn`, runs the step, emits against the live `CountOut`, and seals the receipt.
- Receipt: `ProcessReceipt` carries the operation key, the written pin set in pin order, and the required emissions left unwritten; an uncovered required emission lands a warning notice and stays on the receipt.
- Auto: output arity is proven at emission — a step returning a count other than the live output count is `GhFault.Refused` before any pin writes.
- Growth: a new consumption topology is one `Execution` case plus one run arm; a new receipt fact is one `ProcessReceipt` member; a new scope evidence read is one member over a verified `IDataAccess` accessor.
- Boundary: `Uniform` names the pin depth a case demands, and admission holds every pin to it with `MustExist` presence — a mixed-presence or mixed-depth component is the `Heterogeneous` case by construction; cancellation rides `ProcessScope.Cancel` off the verified `Solution.Token`, never a checked flag.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [TYPES] -----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Execution {
    private Execution() { }

    public sealed record Single(Func<ProcessScope, Seq<IPear>, Fin<Seq<IPear>>> Step) : Execution;
    public sealed record Batch(Func<ProcessScope, Seq<ITwig>, Fin<Seq<ITwig>>> Step) : Execution;
    public sealed record Aggregate(Func<ProcessScope, Seq<ITree>, Fin<Seq<ITree>>> Step) : Execution;
    public sealed record Heterogeneous(Func<ProcessScope, Fin<Unit>> Step) : Execution;

    public Option<PinAccess> Uniform => Map(
        single: Some(PinAccess.Item),
        batch: Some(PinAccess.Twig),
        aggregate: Some(PinAccess.Tree),
        heterogeneous: Option<PinAccess>.None);
}

// --- [MODELS] ----------------------------------------------------------------------------

public readonly record struct Emission(int Pin, bool Required);

public sealed record ProcessReceipt(Op Operation, Seq<int> Written, Seq<int> MissingRequired);

public sealed record ProcessScope {
    public required IDataAccess Access { get; init; }

    public required ComponentSpec Spec { get; init; }

    public required HostUnits Units { get; init; }

    public required CancellationToken Cancel { get; init; }

    public required Op Operation { get; init; }

    internal Atom<LanguageExt.HashSet<int>> Emitted { get; } = Atom(LanguageExt.HashSet<int>());

    public int Iteration => Access.Index;

    public int Iterations => Access.Iterations;

    public Grasshopper2.Doc.FleetingCustomData Custom => Access.CustomData;

    public bool Changed(int pin) => Access.HasInputChanged(pin);

    public bool NullAt(int pin) => Access.GetNull(pin);

    public MetaData MetaOf(int pin) => Access.GetMeta(pin);

    public Fin<Transfer<T>> Read<T>(int pin, PinAccess depth) => GardenData.Read<T>(Access, pin, depth, Operation);

    public Fin<Unit> Write<T>(int pin, Transfer<T> payload, Retention retention) =>
        GardenData.Write(Access, pin, payload, retention, Operation)
            .Map(_ => ignore(Emitted.Swap(held => held.Add(pin))));

    public Unit Notify(Notice notice) => notice.Report(Access);

    public Unit Progress(int percent) => fun(() => Access.SetProgress(percent))();
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class Executions {
    public static Fin<ProcessReceipt> Run(this Execution execution, ProcessScope scope) =>
        execution.Switch(
            state: scope,
            single: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetIPear(pin, out IPear pear) ? Optional(pear) : None)
                    .Bind(pears => run.Step(held, pears))
                    .Bind(outputs => Written(held, outputs, static (access, pin, pear) => access.SetPear(pin, pear))),
            batch: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetITwig(pin, out ITwig twig) ? Optional(twig) : None)
                    .Bind(twigs => run.Step(held, twigs))
                    .Bind(outputs => Written(held, outputs, static (access, pin, twig) => access.SetTwig(pin, twig))),
            aggregate: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetITree(pin, out ITree tree) ? Optional(tree) : None)
                    .Bind(trees => run.Step(held, trees))
                    .Bind(outputs => Written(held, outputs, static (access, pin, tree) => access.SetTree(pin, tree))),
            heterogeneous: static (held, run) => run.Step(held))
        .Bind(_ => Sealed(scope));

    private static Fin<Seq<T>> Gathered<T>(ProcessScope scope, Func<IDataAccess, int, Option<T>> read) =>
        toSeq(Enumerable.Range(0, scope.Access.CountIn))
            .TraverseM(pin => read(scope.Access, pin).ToFin(new GhFault.Absent(scope.Operation, $"pin:{pin}")))
            .As();

    private static Fin<Unit> Written<T>(ProcessScope scope, Seq<T> outputs, Action<IDataAccess, int, T> write) =>
        outputs.Count == scope.Access.CountOut
            ? outputs.Map((value, pin) => Hosted.Bound(() => write(scope.Access, pin, value), scope.Operation)
                    .Map(_ => ignore(scope.Emitted.Swap(held => held.Add(pin)))))
                .TraverseM(identity)
                .As()
                .Map(static _ => unit)
            : Fin.Fail<Unit>(new GhFault.Refused(scope.Operation, $"outputs:{outputs.Count}/{scope.Access.CountOut}"));

    private static Fin<ProcessReceipt> Sealed(ProcessScope scope) =>
        (scope.Emitted.Value, scope.Spec.Emissions.Filter(row => row.Required && !scope.Emitted.Value.Contains(row.Pin))) switch {
            (var written, { IsEmpty: true }) => Fin.Succ(new ProcessReceipt(scope.Operation, toSeq(written.OrderBy(identity)).Strict(), [])),
            (var written, var missing) => (
                scope.Notify(new Notice(Severity.Warning, nameof(Emission), $"unwritten:{missing.Count}", [])),
                Fin.Succ(new ProcessReceipt(scope.Operation, toSeq(written.OrderBy(identity)).Strict(), missing.Map(static row => row.Pin).Strict()))).Item2,
        };
}
```

## [03]-[SPEC]

- Owner: `ComponentSpec` is the one declaration every consumer reads — identity and io id, both pin rosters, the execution case, emission rows, lifecycle stages, threading, maintenance, bake capability, fleeting rows, icon, panel projection, and chrome; the policy slots beside it are values, never subclasses.
- Entry: `Admit` is the accumulating admission — side and hidden capability per pin, emission range, execution depth-and-presence coherence, and io-id parseability report together as one `Validation`.
- Auto: `Lifecycle.None` and the `Option` policy slots make every capability opt-in with zero ceremony at the declaration site.
- Receipt: fleeting rows persist the sealed `ProcessReceipt` into the host `FleetingCustomData` at the post-process stage.
- Growth: a new component capability is one policy slot or one row on an existing record; the declaration never grows a builder family.
- Boundary: pin visibility, category, and colour declare through the row bindings at `ports#PORT_CATALOG`; pin-visibility flexing after declaration rides the host `ModularList` through `SpecComponent.Flex`; the declaration itself stays host-inert data.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record Lifecycle(
    Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> Before,
    Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> Pre,
    Option<Func<Grasshopper2.Doc.Solution, Grasshopper2.Doc.FleetingCustomData, Fin<Unit>>> Post,
    Option<Func<ITree, int, Grasshopper2.Doc.Solution, ITree>> PostTree) {
    public static readonly Lifecycle None = new(default, default, default, default);
}

public sealed record FleetingRow(string Key, Action<Grasshopper2.Doc.FleetingCustomData, ProcessReceipt> Persist);

public sealed record ComponentSpec {
    public required Grasshopper2.UI.Nomen Identity { get; init; }

    public required string IoId { get; init; }

    public required Seq<PinPlan> Inputs { get; init; }

    public required Seq<PinPlan> Outputs { get; init; }

    public required Execution Execution { get; init; }

    public Seq<Emission> Emissions { get; init; } = [];

    public Lifecycle Lifecycle { get; init; } = Lifecycle.None;

    public Option<ThreadingState> Threading { get; init; } = default;

    public Option<Action<ComponentParameters>> Maintain { get; init; } = default;

    public Option<bool> Bakeable { get; init; } = default;

    public Seq<FleetingRow> Fleeting { get; init; } = [];

    public Option<Grasshopper2.UI.Icon.IIcon> Icon { get; init; } = default;

    public Option<Action<Grasshopper2.UI.InputPanel.InputPanel>> Panel { get; init; } = default;

    public Option<ComponentChrome> Chrome { get; init; } = default;

    public Validation<Error, ComponentSpec> Admit() {
        Op op = Op.Of();
        return (
            Inputs.Traverse(plan => Sided(plan, PinSide.Input, op)).As(),
            Outputs.Traverse(plan => Sided(plan, PinSide.Output, op)).As(),
            Emissions.Filter(row => row.Pin < 0 || row.Pin >= Outputs.Count) is { IsEmpty: true }
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new GhFault.Refused(op, nameof(Emissions))),
            Execution.Uniform.Match(
                Some: depth => Inputs.ForAll(plan => plan.Access == depth && plan.Presence == PinPresence.MustExist)
                    && Outputs.ForAll(plan => plan.Access == depth)
                    ? Success<Error, Unit>(unit)
                    : Fail<Error, Unit>(new GhFault.Refused(op, $"{nameof(Execution)}:{depth}")),
                None: static () => Success<Error, Unit>(unit)),
            System.Guid.TryParse(IoId, out _)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new GhFault.Registration(op, nameof(IoId))))
            .Apply((_, _, _, _, _) => this)
            .As();
    }

    private static Validation<Error, PinPlan> Sided(PinPlan plan, PinSide side, Op key) =>
        (plan.Kind.Sides.Accepts(side), plan.Visibility == PinVisibility.Shown || plan.Kind.HiddenSides.Accepts(side)) switch {
            (true, true) => plan,
            (false, _) => new GhFault.Refused(key, $"{plan.Kind.Key}:{side}"),
            _ => new GhFault.Refused(key, $"{plan.Kind.Key}:{nameof(PinVisibility.Hidden)}:{side}"),
        };
}
```

## [04]-[HOST_PROJECTION]

- Owner: `SpecComponent` is the single boundary adapter — an abstract `ModularComponent` whose every override is one dispatch into the declaration; a concrete component is a sealed subclass carrying its `[IoId]` attribute and handing its `ComponentSpec` to this one base.
- Entry: the host drives every entry — pin declaration folds the modular adders through `ports#DECLARATION_FOLD` with trims realized per minted parameter, `Process` builds one `ProcessScope` and runs the execution case, lifecycle stages dispatch their `Option` rows, and maintenance re-realizes through the verified `ComponentParameters` accessors; `Flex` is the one post-declaration pin-visibility gate over the host `ModularList.Show`/`Hide` with its `ActionList` undo.
- Receipt: the latest `ProcessReceipt` rides an `Atom` cell from `Process` to `PostProcess`, where fleeting rows persist it.
- Auto: a rail failure inside `Process` lands on the document as an error notice with cancellation filtered; a declaration failure at construction or pin registration throws at the composition edge, because a mis-declared component is a programming defect, never a runtime state; `Threading` assigns once at construction because the host property is settable state, never a virtual.
- Growth: a new host virtual is one override dispatching an existing declaration slot; the adapter never grows logic of its own.
- Boundary: the overrides, the constructor admission collapse, and the `Flex` body are the named platform-forced statement seam; no other file names a `ModularComponent` member. The host seals plain-adder registration and variable-pin creation on `ModularComponent` — pin flexing through the modular lists is the sole post-declaration pin mutation. Mounting chrome replaces the internal `ModularComponentAttributes`, so a chrome policy owns its own layout decisions.

```csharp signature
// --- [COMPOSITION] -----------------------------------------------------------------------

public abstract class SpecComponent : ModularComponent {
    private readonly ComponentSpec spec;

    private readonly Atom<Option<ProcessReceipt>> latest = Atom(Option<ProcessReceipt>.None);

    protected SpecComponent(ComponentSpec spec) : base(spec.Identity) {
        this.spec = spec.Admit().Match(Succ: identity, Fail: Panic<ComponentSpec>);
        this.spec.Threading.IfSome(state => { Threading = state; });
    }

    public override bool BakeCapable => spec.Bakeable.IfNone(base.BakeCapable);

    protected override void AddInputs(ModularInputAdder inputs) =>
        ignore(Ports.Declare(inputs, spec.Inputs, Op.Of()).Match(Succ: identity, Fail: Panic<Seq<IParameter>>));

    protected override void AddOutputs(ModularOutputAdder outputs) =>
        ignore(Ports.Declare(outputs, spec.Outputs, Op.Of()).Match(Succ: identity, Fail: Panic<Seq<IParameter>>));

    protected override void Process(IDataAccess access) =>
        ignore(HostUnits.Of(access, Op.Of())
            .Map(units => new ProcessScope {
                Access = access,
                Spec = spec,
                Units = units,
                Cancel = access.Solution.Token,
                Operation = Op.Of(),
            })
            .Bind(scope => spec.Execution.Run(scope))
            .Match(
                Succ: receipt => ignore(latest.Swap(_ => Optional(receipt))),
                Fail: fault => ignore(fault is Fault.Cancelled ? unit : Notice.Of(fault).Report(access))));

    protected override void BeforeProcess(Grasshopper2.Doc.Solution solution) =>
        ignore(spec.Lifecycle.Before.Map(stage => stage(solution)));

    protected override void PreProcess(Grasshopper2.Doc.Solution solution) =>
        ignore(spec.Lifecycle.Pre.Map(stage => stage(solution)));

    protected override void PostProcess(Grasshopper2.Doc.Solution solution, Grasshopper2.Doc.FleetingCustomData data) =>
        ignore((
            latest.Value.Map(receipt => spec.Fleeting.Map(row => fun(() => row.Persist(data, receipt))()).Strict()),
            spec.Lifecycle.Post.Map(stage => stage(solution, data))));

    protected override ITree PostProcessTree(ITree tree, int output, Grasshopper2.Doc.Solution solution) =>
        spec.Lifecycle.PostTree.Map(stage => stage(tree, output, solution)).IfNone(tree);

    public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel) =>
        ignore((fun(() => base.AppendToInputPanel(panel))(), spec.Panel.Map(append => fun(() => append(panel))())));

    protected override Grasshopper2.Doc.IAttributes CreateAttributes() =>
        spec.Chrome.Match(Some: chrome => ChromeHost.Mount(this, chrome), None: () => base.CreateAttributes());

    public override void VariableParameterMaintenance() =>
        ignore((
            spec.Maintain.Map(maintain => fun(() => maintain(Parameters))()),
            Ports.Realize(Parameters, spec.Inputs, spec.Outputs, Op.Of())));

    public Fin<Unit> Flex(PinSide side, int index, bool shown, Grasshopper2.Undo.ActionList undo, Op? key = null) {
        ModularList list = side.Switch(state: this, input: static self => self.ModularInputs, output: static self => self.ModularOutputs);
        return Hosted.Bound(() => { if (shown) { list.Show(index, undo); } else { list.Hide(index, undo); } }, key.OrDefault());
    }

    private static T Panic<T>(Error fault) => throw new InvalidOperationException(fault.Message);
}
```

## [05]-[PLUGIN]

- Owner: `PluginSpec` is the registration row — plugin identity, exported types, and satellite assemblies as one record; `SpecPlugin` is its host projection, and `Catalogue` is the audit and load surface consuming the same rows registration consumes.
- Entry: `Catalogue.Audit` accumulates every exported-type violation; `Load` and `Harvest` lift the host plugin-server entries onto the rail; `OwnerOf` resolves a document object to its owning plugin.
- Receipt: a failed load carries the host `FailureInfo` detail inside `GhFault.Registration`.
- Auto: instance-level admission runs in the `SpecComponent` constructor, so a catalogued type that constructs has already passed `Admit` — the audit proves type-level facts, construction proves the declaration.
- Growth: a new plugin asset class is one `PluginSpec` member plus one `SpecPlugin` override.
- Boundary: reflection appears only here — the audit reads type identity and the `[IoId]` attribute, never member shapes.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record PluginSpec(Guid Id, Grasshopper2.UI.Nomen Identity, Version Version, Seq<Type> Exported, Seq<string> Satellites);

// --- [COMPOSITION] -----------------------------------------------------------------------

public abstract class SpecPlugin : Grasshopper2.Framework.Plugin {
    private readonly PluginSpec spec;

    protected SpecPlugin(PluginSpec spec) : base(spec.Id, spec.Identity, spec.Version) => this.spec = spec;

    public override IEnumerable<Type> ExportedTypes => spec.Exported;

    public override IEnumerable<string> SatelliteAssemblies => spec.Satellites;

    public override void OnLoaded() =>
        ignore(Catalogue.Audit(spec).Match(
            Succ: identity,
            Fail: static fault => throw new InvalidOperationException(fault.Message)));
}

public static class Catalogue {
    public static Validation<Error, PluginSpec> Audit(PluginSpec plugin, Op? key = null) =>
        plugin.Exported.Traverse(type => Exported(type, key.OrDefault())).As().Map(_ => plugin);

    public static Fin<Grasshopper2.Framework.Plugin> Load(string path, Op? key = null) =>
        Hosted.Bound(() => Grasshopper2.Framework.PluginServer.LoadPlugin(path, out Grasshopper2.Framework.FailureInfo failure) is { } plugin
                ? Fin.Succ(plugin)
                : Fin.Fail<Grasshopper2.Framework.Plugin>(new GhFault.Registration(key.OrDefault(), $"{path}:{failure}")), key.OrDefault())
            .Bind(identity);

    public static Fin<Grasshopper2.Framework.Plugin> Harvest(System.Reflection.Assembly assembly, Op? key = null) =>
        Hosted.Bound(() => Grasshopper2.Framework.PluginServer.HarvestPluginFromAssembly(assembly, out Grasshopper2.Framework.FailureInfo failure) is { } plugin
                ? Fin.Succ(plugin)
                : Fin.Fail<Grasshopper2.Framework.Plugin>(new GhFault.Registration(key.OrDefault(), $"{assembly.FullName}:{failure}")), key.OrDefault())
            .Bind(identity);

    public static Option<Grasshopper2.Framework.Plugin> OwnerOf(Grasshopper2.Doc.IDocumentObject subject) =>
        Optional(Grasshopper2.Framework.PluginServer.FindPluginForObject(subject));

    private static Validation<Error, Type> Exported(Type type, Op key) =>
        (typeof(SpecComponent).IsAssignableFrom(type), Attribute.IsDefined(type, typeof(GrasshopperIO.IoIdAttribute))) switch {
            (true, true) => type,
            (false, _) => new GhFault.Registration(key, $"{type.Name}:{nameof(SpecComponent)}"),
            (_, false) => new GhFault.Registration(key, $"{type.Name}:{nameof(GrasshopperIO.IoIdAttribute)}"),
        };
}
```

## [06]-[RESEARCH]

- [ICON_INTERNAL]-[OPEN]: the icon projection virtual on the component chain — `Component` carries `IconMode`/`ResolveIconMode` and `Label`, and no `IconInternal` member surfaces on the decompiled `Component`; locate the icon override seam through the decompile rail over the `ActiveObject` base, then wire `ComponentSpec.Icon` onto it.
- [PLUGIN_SERVER_SHAPE]-[OPEN]: whether `PluginServer.LoadPlugin`/`HarvestPluginFromAssembly` are static and return the loaded `Plugin`; the lifts assume static members returning the plugin or null.
- [BATCH_PROCESS]-[OPEN]: the verified `Component.Process(IDataAccess[] iterations, CancellationToken token)` multi-iteration virtual — rule an `Execution` batch-iteration case over it or an explicit kill after its dispatch semantics verify against `ThreadingState`.
- [MODULAR_STORE]-[OPEN]: `ModularList.Store(IWriter)`/`Restore(IReader)` persistence of pin visibility — whether the host persists flexed visibility without adapter involvement; verify before `Flex` grows a persistence row.

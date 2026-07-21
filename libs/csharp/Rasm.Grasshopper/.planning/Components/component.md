# [RASM_GRASSHOPPER_COMPONENT]

`ComponentSpec` is the component declaration consumed unchanged by construction, pin registration, execution, lifecycle, chrome, and catalogue admission. `Execution` owns pin-topology consumption; `IterationPolicy` independently owns the host iteration-array override. `SpecComponent<TSelf>` binds the declaration through a static-abstract self contract before the host base constructor invokes pin registration, and every process path seals partial emission evidence into one run receipt.

## [01]-[INDEX]

- [02]-[EXECUTION]: topology dispatch, output declarations, process scope, and partial-emission receipts
- [03]-[SPEC]: the component declaration, lifecycle policy, and accumulating admission
- [04]-[HOST_PROJECTION]: the constructor-safe generic host adapter and run ledger
- [05]-[PLUGIN]: plugin declarations, public load ingress, and catalogue admission

## [02]-[EXECUTION]

- Owner: `Execution` closes the pear, twig, tree, and mixed topology family under one `Run` dispatch; `IterationPolicy` separately selects the host array driver or one custom whole-array fold.
- Entry: `Executions.Run` gathers at the declared topology, invokes the step, writes in output declaration order, and returns `ProcessRun` with both the result rail and the receipt sealed from the same scope.
- Receipt: `ProcessReceipt` carries `OutputPlan` values rather than raw pin indexes, so required-output evidence and pin declaration share one identity; the receipt survives a late write failure with every earlier emission intact.
- Growth: a topology extends `Execution`; an array strategy extends `IterationPolicy`; output obligation extends `OutputPlan`; none creates a second processing entrypoint.
- Boundary: `ProcessScope` is the only step seam into `IDataAccess`; it carries context, cancellation, iteration evidence, typed reads, receipted writes, notices, and the operation key.

```csharp signature
namespace Rasm.Grasshopper.Components;

// --- [TYPES] -----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Execution {
    private Execution() { }

    public sealed record Pear(Func<ProcessScope, Seq<IPear>, Fin<Seq<IPear>>> Step) : Execution;
    public sealed record Twig(Func<ProcessScope, Seq<ITwig>, Fin<Seq<ITwig>>> Step) : Execution;
    public sealed record Tree(Func<ProcessScope, Seq<ITree>, Fin<Seq<ITree>>> Step) : Execution;
    public sealed record Mixed(Func<ProcessScope, Fin<Unit>> Step) : Execution;

    public Option<PinAccess> Uniform => Switch(
        pear: static _ => Some(PinAccess.Item),
        twig: static _ => Some(PinAccess.Twig),
        tree: static _ => Some(PinAccess.Tree),
        mixed: static _ => Option<PinAccess>.None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IterationPolicy {
    private IterationPolicy() { }

    public sealed record Host : IterationPolicy;
    public sealed record Custom(Func<Seq<ProcessScope>, CancellationToken, Fin<Unit>> Step) : IterationPolicy;

    public static readonly IterationPolicy Default = new Host();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OutputPlan {
    private OutputPlan() { }

    public sealed record Optional(PinPlan Pin) : OutputPlan;
    public sealed record Required(PinPlan Pin) : OutputPlan;

    public PinPlan Plan => Switch(optional: static row => row.Pin, required: static row => row.Pin);

    public bool IsRequired => Switch(optional: static _ => false, required: static _ => true);
}

// --- [MODELS] ----------------------------------------------------------------------------

public sealed record ProcessReceipt(Op Operation, int Iteration, Seq<OutputPlan> Written, Seq<OutputPlan> MissingRequired);

public sealed record ProcessRun(ProcessReceipt Receipt, Fin<Unit> Result);

public sealed record RunReceipt(Seq<ProcessReceipt> Processes, Seq<Error> Faults) {
    public static readonly RunReceipt Empty = new([], []);

    public RunReceipt Add(ProcessRun process) => process.Result.Match(
        Succ: _ => this with { Processes = Processes.Add(process.Receipt) },
        Fail: fault => this with { Processes = Processes.Add(process.Receipt), Faults = Faults.Add(fault) });

    public RunReceipt Add(Error fault) => this with { Faults = Faults.Add(fault) };
}

public sealed record ProcessScope {
    public required IDataAccess Access { get; init; }

    public required ComponentSpec Spec { get; init; }

    public required HostUnits Units { get; init; }

    public required CancellationToken Cancel { get; init; }

    public required Op Operation { get; init; }

    internal Atom<LanguageExt.HashSet<OutputPlan>> Emitted { get; } = Atom(LanguageExt.HashSet<OutputPlan>());

    public int Iteration => Access.Index;

    public int Iterations => Access.Iterations;

    public Grasshopper2.Doc.FleetingCustomData Custom => Access.CustomData;

    public bool Changed(int pin) => Access.HasInputChanged(pin);

    public bool NullAt(int pin) => Access.GetNull(pin);

    public MetaData MetaOf(int pin) => Access.GetMeta(pin);

    public Fin<Transfer<T>> Read<T>(int pin, PinAccess depth) => GardenData.Read<T>(Access, pin, depth, Operation);

    public Fin<Unit> Write<T>(int pin, Transfer<T> payload, Retention retention) =>
        pin is >= 0 && pin < Spec.Outputs.Count
            ? GardenData.Write(Access, pin, payload, retention, Operation)
                .Map(_ => ignore(Emitted.Swap(held => held.Add(Spec.Outputs[pin]))))
            : Fin.Fail<Unit>(new GhFault.Refused(Operation, $"output:{pin}"));

    public Unit Notify(Notice notice) => notice.Report(Access);

    public Unit Progress(int percent) => fun(() => Access.SetProgress(percent))();
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class Executions {
    public static ProcessRun Run(this Execution execution, ProcessScope scope) =>
        Completed(scope, execution.Switch(
            state: scope,
            pear: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetIPear(pin, out IPear pear) ? Optional(pear) : None)
                    .Bind(pears => run.Step(held, pears))
                    .Bind(outputs => Written(held, outputs, static (access, pin, pear) => access.SetPear(pin, pear))),
            twig: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetITwig(pin, out ITwig twig) ? Optional(twig) : None)
                    .Bind(twigs => run.Step(held, twigs))
                    .Bind(outputs => Written(held, outputs, static (access, pin, twig) => access.SetTwig(pin, twig))),
            tree: static (held, run) =>
                Gathered(held, static (access, pin) => access.GetITree(pin, out ITree tree) ? Optional(tree) : None)
                    .Bind(trees => run.Step(held, trees))
                    .Bind(outputs => Written(held, outputs, static (access, pin, tree) => access.SetTree(pin, tree))),
            mixed: static (held, run) => run.Step(held)));

    internal static ProcessReceipt Receipt(ProcessScope scope) {
        LanguageExt.HashSet<OutputPlan> written = scope.Emitted.Value;
        Seq<OutputPlan> ordered = scope.Spec.Outputs.Filter(written.Contains).Strict();
        Seq<OutputPlan> missing = scope.Spec.Outputs.Filter(output => output.IsRequired && !written.Contains(output)).Strict();
        return new ProcessReceipt(scope.Operation, scope.Iteration, ordered, missing);
    }

    private static ProcessRun Completed(ProcessScope scope, Fin<Unit> result) => new(Receipt(scope), result);

    private static Fin<Seq<T>> Gathered<T>(ProcessScope scope, Func<IDataAccess, int, Option<T>> read) =>
        toSeq(Enumerable.Range(0, scope.Access.CountIn))
            .TraverseM(pin => read(scope.Access, pin).ToFin(new GhFault.Absent(scope.Operation, $"input:{pin}")))
            .As();

    private static Fin<Unit> Written<T>(ProcessScope scope, Seq<T> outputs, Action<IDataAccess, int, T> write) =>
        outputs.Count == scope.Spec.Outputs.Count
            ? outputs.Map((value, pin) => Hosted.Bound(() => write(scope.Access, pin, value), scope.Operation)
                    .Map(_ => ignore(scope.Emitted.Swap(held => held.Add(scope.Spec.Outputs[pin])))))
                .TraverseM(identity)
                .As()
                .Map(static _ => unit)
            : Fin.Fail<Unit>(new GhFault.Refused(scope.Operation, $"outputs:{outputs.Count}/{scope.Spec.Outputs.Count}"));
}
```

## [03]-[SPEC]

- Owner: `ComponentSpec` carries identity, pin declarations, topology execution, iteration policy, lifecycle, threading, maintenance, bake, fleeting persistence, icon, panel, and chrome as one immutable declaration.
- Entry: `Admit` accumulates pin-side legality, topology coherence, iteration/threading coherence, and persistent identity before the static component declaration reaches the host constructor.
- Receipt: fleeting persistence consumes the whole `RunReceipt`, so lifecycle faults, custom-array faults, process faults, and partial output evidence cross the post-process boundary together.
- Growth: a component capability is a policy value or an existing declaration row; the record never grows a builder family.
- Boundary: `OutputPlan` owns output obligation beside its `PinPlan`; no second raw-index emission roster exists.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------

public sealed record Lifecycle(
    Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> Before,
    Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> Pre,
    Option<Func<Grasshopper2.Doc.Solution, Grasshopper2.Doc.FleetingCustomData, Fin<Unit>>> Post,
    Option<Func<ITree, int, Grasshopper2.Doc.Solution, Fin<ITree>>> PostTree) {
    public static readonly Lifecycle None = new(default, default, default, default);
}

public sealed record ComponentSpec {
    public required Grasshopper2.UI.Nomen Identity { get; init; }

    public required string IoId { get; init; }

    public required Seq<PinPlan> Inputs { get; init; }

    public required Seq<OutputPlan> Outputs { get; init; }

    public required Execution Execution { get; init; }

    public IterationPolicy Iterations { get; init; } = IterationPolicy.Default;

    public Lifecycle Lifecycle { get; init; } = Lifecycle.None;

    public Option<ThreadingState> Threading { get; init; } = default;

    public Option<Func<ComponentParameters, Fin<Unit>>> Maintain { get; init; } = default;

    public Option<bool> Bakeable { get; init; } = default;

    public Seq<Action<Grasshopper2.Doc.FleetingCustomData, RunReceipt>> Fleeting { get; init; } = [];

    public Option<Grasshopper2.UI.Icon.IIcon> Icon { get; init; } = default;

    public Option<Action<Grasshopper2.UI.InputPanel.InputPanel>> Panel { get; init; } = default;

    public Option<ComponentChrome> Chrome { get; init; } = default;

    public Validation<Error, ComponentSpec> Admit() {
        Op op = Op.Of();
        return (
            Inputs.Traverse(plan => Sided(plan, PinSide.Input, op)).As(),
            Outputs.Traverse(output => Sided(output.Plan, PinSide.Output, op)).As(),
            Topology(op),
            Iteration(op),
            Guid.TryParse(IoId, out _)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new GhFault.Registration(op, nameof(IoId))))
            .Apply((_, _, _, _, _) => this)
            .As();
    }

    private Validation<Error, Unit> Topology(Op key) => Execution.Uniform.Match(
        Some: depth => Inputs.ForAll(plan => plan.Access == depth && plan.Presence == PinPresence.MustExist)
            && Outputs.ForAll(output => output.Plan.Access == depth)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new GhFault.Refused(key, $"{nameof(Execution)}:{depth}")),
        None: static () => Success<Error, Unit>(unit));

    private Validation<Error, Unit> Iteration(Op key) => Iterations.Switch(
        host: static _ => Success<Error, Unit>(unit),
        custom: _ => Threading.IsNone
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new GhFault.Refused(key, nameof(Threading))));

    private static Validation<Error, PinPlan> Sided(PinPlan plan, PinSide side, Op key) =>
        plan.Kind.Accepts(plan: plan, side: side, key: key).Map(_ => plan).ToValidation();
}
```

## [04]-[HOST_PROJECTION]

- Owner: `IComponentDeclaration<TSelf>` binds one static declaration to its concrete component type; `SpecComponent<TSelf>` reads the admitted value from static storage before `ModularComponent` invokes `AddInputs`, `AddOutputs`, and initial maintenance.
- Entry: host callbacks project into the declaration; per-access processing records one `ProcessRun`, custom array processing seals every scope after its whole-array fold, and lifecycle stages join the same ledger.
- Receipt: `RunReceipt` is atomically accumulated across host-parallel iterations and persisted after the post stage; a fault never erases process evidence already emitted.
- Growth: a host virtual adds one declaration projection; declaration, ledger, and rail ownership stay in the generic base.
- Boundary: constructor-time declaration failures and initial maintenance failures throw at composition; runtime failures enter the run ledger and report through `IDataAccess` where that channel exists.
- RESEARCH: the `Connectivity`/`ConnectivityComplete` component virtuals and `ComputeInternal(Solution, CallStack)` carry catalog rows without stated override semantics — whether each is a lifecycle stage the declaration projects (one `Lifecycle` slot each) or host plumbing the base owns resolves at decompile, landing as declaration slots only where the override is consumer-meaningful.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

public interface IComponentDeclaration<TSelf> where TSelf : IComponentDeclaration<TSelf> {
    static abstract ComponentSpec Spec { get; }
}

// --- [COMPOSITION] -----------------------------------------------------------------------

public abstract class SpecComponent<TSelf> : ModularComponent
    where TSelf : SpecComponent<TSelf>, IComponentDeclaration<TSelf> {
    private static readonly ComponentSpec Definition = TSelf.Spec.Admit().Match(Succ: identity, Fail: Panic<ComponentSpec>);

    private readonly Atom<RunReceipt> run = Atom(RunReceipt.Empty);

    private bool ready;

    protected SpecComponent() : base(Definition.Identity) {
        Definition.Threading.IfSome(state => { Threading = state; });
        ready = true;
    }

    public override bool BakeCapable => Definition.Bakeable.IfNone(base.BakeCapable);

    protected override Grasshopper2.UI.Icon.IIcon IconInternal => Definition.Icon.IfNone(base.IconInternal);

    protected override void AddInputs(ModularInputAdder inputs) =>
        ignore(Ports.Declare(inputs, Definition.Inputs, Op.Of()).Match(Succ: identity, Fail: Panic<Seq<IParameter>>));

    protected override void AddOutputs(ModularOutputAdder outputs) =>
        ignore(Ports.Declare(outputs, Definition.Outputs.Map(static output => output.Plan).Strict(), Op.Of())
            .Match(Succ: identity, Fail: Panic<Seq<IParameter>>));

    protected override void Process(IDataAccess access) =>
        ignore(Scope(access, access.Solution.Token, Op.Of()).Match(
            Succ: scope => Record(Definition.Execution.Run(scope), Some(access)),
            Fail: fault => Capture(fault, Some(access))));

    protected override void Process(IDataAccess[] iterations, CancellationToken token) =>
        ignore(Definition.Iterations.Switch(
            state: (Self: this, Iterations: iterations, Token: token, Key: Op.Of()),
            host: static (state, _) => state.Self.HostIterations(state.Iterations, state.Token),
            custom: static (state, policy) => state.Self.CustomIterations(state.Iterations, state.Token, policy, state.Key)));

    protected override void BeforeProcess(Grasshopper2.Doc.Solution solution) =>
        ignore((run.Swap(_ => RunReceipt.Empty), Track(Stage(Definition.Lifecycle.Before, solution, Op.Of()))));

    protected override void PreProcess(Grasshopper2.Doc.Solution solution) =>
        ignore(Track(Stage(Definition.Lifecycle.Pre, solution, Op.Of())));

    protected override void PostProcess(Grasshopper2.Doc.Solution solution, Grasshopper2.Doc.FleetingCustomData data) =>
        ignore((
            Track(Stage(Definition.Lifecycle.Post, solution, data, Op.Of())),
            Definition.Fleeting.Map(persist => Track(Hosted.Bound(() => persist(data, run.Value), Op.Of()))).Strict()));

    protected override ITree PostProcessTree(ITree tree, int output, Grasshopper2.Doc.Solution solution) =>
        Definition.Lifecycle.PostTree.Match(
            Some: stage => Track(Hosted.Bound(() => stage(tree, output, solution), Op.Of()).Bind(identity), tree),
            None: () => tree);

    public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel) =>
        ignore(Track(Hosted.Bound(() => base.AppendToInputPanel(panel), Op.Of())
            .Bind(_ => Definition.Panel.Match(
                Some: append => Hosted.Bound(() => append(panel), Op.Of()),
                None: static () => Fin.Succ(unit)))));

    protected override Grasshopper2.Doc.IAttributes CreateAttributes() =>
        Definition.Chrome.Match(Some: chrome => ChromeHost.Mount(this, chrome), None: () => base.CreateAttributes());

    public override void VariableParameterMaintenance() =>
        ignore(ready
            ? Track(Maintained(Op.Of()))
            : Maintained(Op.Of()).Match(Succ: identity, Fail: Panic<Unit>));

    public Fin<Unit> Flex(PinSide side, int index, bool shown, Grasshopper2.Undo.ActionList undo, Op? key = null) {
        ModularList list = side.Switch(state: this, input: static self => self.ModularInputs, output: static self => self.ModularOutputs);
        return Hosted.Bound(() => { if (shown) { list.Show(index, undo); } else { list.Hide(index, undo); } }, key.OrDefault());
    }

    private static Fin<ProcessScope> Scope(IDataAccess access, CancellationToken cancel, Op key) =>
        HostUnits.Of(access, key).Map(units => new ProcessScope {
            Access = access,
            Spec = Definition,
            Units = units,
            Cancel = cancel,
            Operation = key,
        });

    private static Fin<Seq<ProcessScope>> Scopes(IDataAccess[] iterations, CancellationToken cancel, Op key) =>
        toSeq(iterations).TraverseM(access => Scope(access, cancel, key)).As();

    private static Fin<Unit> Stage(
        Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> stage,
        Grasshopper2.Doc.Solution solution,
        Op key) => stage.Match(
            Some: action => Hosted.Bound(() => action(solution), key).Bind(identity),
            None: static () => Fin.Succ(unit));

    private static Fin<Unit> Stage(
        Option<Func<Grasshopper2.Doc.Solution, Grasshopper2.Doc.FleetingCustomData, Fin<Unit>>> stage,
        Grasshopper2.Doc.Solution solution,
        Grasshopper2.Doc.FleetingCustomData data,
        Op key) => stage.Match(
            Some: action => Hosted.Bound(() => action(solution, data), key).Bind(identity),
            None: static () => Fin.Succ(unit));

    private Fin<Unit> Maintained(Op key) => Definition.Maintain.Match(
            Some: maintain => Hosted.Bound(() => maintain(Parameters), key).Bind(identity),
            None: static () => Fin.Succ(unit))
        .Bind(_ => Ports.Realize(
            Parameters,
            Definition.Inputs,
            Definition.Outputs.Map(static output => output.Plan).Strict(),
            key).ToFin());

    private Unit HostIterations(IDataAccess[] iterations, CancellationToken token) {
        base.Process(iterations, token);
        return unit;
    }

    private Unit CustomIterations(IDataAccess[] iterations, CancellationToken token, IterationPolicy.Custom policy, Op key) =>
        Scopes(iterations, token, key).Match(
            Succ: scopes => Complete(scopes, Hosted.Bound(() => policy.Step(scopes, token), key).Bind(identity)),
            Fail: fault => Capture(fault, None));

    private Unit Complete(Seq<ProcessScope> scopes, Fin<Unit> result) =>
        (ignore(scopes.Map(scope => Record(new ProcessRun(Executions.Receipt(scope), Fin.Succ(unit)), None)).Strict()),
            result.Match(Succ: identity, Fail: fault => Capture(fault, None))).Item2;

    private Unit Record(ProcessRun process, Option<IDataAccess> access) =>
        (ignore(run.Swap(receipt => receipt.Add(process))),
            process.Result.Match(Succ: identity, Fail: fault => Report(fault, access)),
            Warn(process.Receipt, access)).Item3;

    private Unit Warn(ProcessReceipt receipt, Option<IDataAccess> access) =>
        receipt.MissingRequired.IsEmpty
            ? unit
            : access.Map(target => new Notice(
                    Severity.Warning,
                    nameof(OutputPlan.Required),
                    string.Join(",", receipt.MissingRequired.Map(static output => output.Plan.Nick)),
                    []).Report(target))
                .IfNone(unit);

    private Unit Track(Fin<Unit> result) => result.Match(Succ: identity, Fail: fault => Capture(fault, None));

    private T Track<T>(Fin<T> result, T fallback) => result.Match(
        Succ: identity,
        Fail: fault => (Capture(fault, None), fallback).Item2);

    private Unit Capture(Error fault, Option<IDataAccess> access) =>
        (ignore(run.Swap(receipt => receipt.Add(fault))), Report(fault, access)).Item2;

    private static Unit Report(Error fault, Option<IDataAccess> access) =>
        access.Map(target => Notice.Of(fault).Report(target)).IfNone(unit);

    private static T Panic<T>(Error fault) => throw new InvalidOperationException(fault.Message);
}
```

## [05]-[PLUGIN]

- Owner: `PluginSpec` is the registration declaration; `PluginSource` closes public path and assembly loading under one `Catalogue.Load`; `SpecPlugin` projects metadata and audits exported component types at the host load edge.
- Entry: `Catalogue.Load` lifts the host `bool`-and-`FailureInfo` contract into `Fin<PluginReceipt>`; `OwnerOf` resolves a live document object through the same server.
- Receipt: a successful load returns location and assembly identity; a refusal preserves the host failure detail in `GhFault.Registration`.
- Growth: a public plugin ingress is one `PluginSource` case and one load arm; plugin metadata is one `PluginSpec` member and one host override.
- Boundary: assembly harvesting remains inside `PluginServer`; local reflection is limited to exported-type declaration and persistent-id admission.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginSource {
    private PluginSource() { }

    public sealed record Location(string Path) : PluginSource;
    public sealed record Binary(string Location, System.Reflection.Assembly Value) : PluginSource;
}

// --- [MODELS] ----------------------------------------------------------------------------

public sealed record PluginSpec(
    Guid Id,
    Grasshopper2.UI.Nomen Identity,
    Version Version,
    Seq<Type> Exported,
    Seq<string> Satellites,
    Option<string> Author = default,
    Option<string> Copyright = default);

public sealed record PluginReceipt(string Location, Option<string> Assembly);

// --- [COMPOSITION] -----------------------------------------------------------------------

public abstract class SpecPlugin : Grasshopper2.Framework.Plugin {
    private readonly PluginSpec spec;

    protected SpecPlugin(PluginSpec spec) : base(spec.Id, spec.Identity, spec.Version) => this.spec = spec;

    public override IEnumerable<Type> ExportedTypes => spec.Exported;

    public override IEnumerable<string> SatelliteAssemblies => spec.Satellites;

    public override string Author => spec.Author.IfNone(base.Author);

    public override string Copyright => spec.Copyright.IfNone(base.Copyright);

    public override void OnLoaded() =>
        ignore(Catalogue.Audit(spec).Match(Succ: identity, Fail: static fault => throw new InvalidOperationException(fault.Message)));
}

public static class Catalogue {
    public static Validation<Error, PluginSpec> Audit(PluginSpec plugin, Op? key = null) =>
        plugin.Exported.Traverse(type => Exported(type, key.OrDefault())).As().Map(_ => plugin);

    public static Fin<PluginReceipt> Load(PluginSource source, Op? key = null) => source.Switch(
        state: key.OrDefault(),
        location: static (op, row) => Hosted.Bound(() =>
                Grasshopper2.Framework.PluginServer.LoadPlugin(row.Path, out Grasshopper2.Framework.FailureInfo failure)
                    ? Fin.Succ(new PluginReceipt(row.Path, None))
                    : Fin.Fail<PluginReceipt>(new GhFault.Registration(op, $"{row.Path}:{failure}")), op)
            .Bind(identity),
        binary: static (op, row) => Hosted.Bound(() =>
                Grasshopper2.Framework.PluginServer.LoadPlugin(row.Location, row.Value, out Grasshopper2.Framework.FailureInfo failure)
                    ? Fin.Succ(new PluginReceipt(row.Location, Optional(row.Value.FullName)))
                    : Fin.Fail<PluginReceipt>(new GhFault.Registration(op, $"{row.Location}:{failure}")), op)
            .Bind(identity));

    public static Option<Grasshopper2.Framework.Plugin> OwnerOf(Grasshopper2.Doc.IDocumentObject subject) =>
        Optional(Grasshopper2.Framework.PluginServer.FindPluginForObject(subject));

    private static Validation<Error, Type> Exported(Type type, Op key) =>
        (type.GetInterfaces().Any(contract =>
                contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IComponentDeclaration<>)
                && contract.GenericTypeArguments.Single() == type),
            Attribute.IsDefined(type, typeof(GrasshopperIO.IoIdAttribute))) switch {
            (true, true) => type,
            (false, _) => new GhFault.Registration(key, $"{type.Name}:{typeof(IComponentDeclaration<>).Name}"),
            (_, false) => new GhFault.Registration(key, $"{type.Name}:{nameof(GrasshopperIO.IoIdAttribute)}"),
        };
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

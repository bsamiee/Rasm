# [RASM_GRASSHOPPER_COMPONENT]

`ComponentSpec` is the component declaration consumed unchanged by construction, pin registration, execution, lifecycle, chrome, and catalogue admission. `Execution` owns pin-topology consumption; `IterationPolicy` independently owns the host iteration-array override. `SpecComponent<TSelf>` binds the declaration through a static-abstract self contract before the host base constructor invokes pin registration, and every process path reports its own failures and missing required outputs.

## [01]-[INDEX]

- [02]-[EXECUTION]: topology dispatch, output declarations, process scope, and required-output checks
- [03]-[SPEC]: `ComponentSpec` declares identity, lifecycle policy, and accumulating admission
- [04]-[HOST_PROJECTION]: `SpecComponent<TSelf>` adapts the host constructor-safely and owns the run ledger
- [05]-[PLUGIN]: plugin declarations, public load ingress, and catalogue admission

## [02]-[EXECUTION]

- Owner: `Execution` closes at TWO cases — `Uniform(PinAccess Depth, Step)` and `Mixed(Step)` — because the former pear/twig/tree triplet differed only in the carrier its pre-gather plumbing spelled: every step now reads and writes through `ProcessScope`'s own typed results at the declared depth, the three per-carrier gather/write folds delete whole, and the depth survives as the ADMISSION datum topology coherence checks against. NAMED LOSS: the pre-gathered `Seq<IPear/ITwig/ITree>` step argument — a step spells `scope.Read<T>(pin, depth)` per pin, which is the same host call the plumbing made invisibly. `IterationPolicy` separately selects the host array driver or one custom whole-array fold.
- Entry: `Executions.Run` invokes the step inside the scope and returns its `Fin<Unit>` unchanged; the host callback checks the scope's emitted `OutputPlan` set before collapsing the result.
- Growth: a topology extends `Execution`; an array strategy extends `IterationPolicy`; output obligation extends `OutputPlan`; none creates a second processing entrypoint.
- Boundary: `ProcessScope` is the only step boundary into `IDataAccess`; it carries context, cancellation, iteration evidence, typed reads, output writes, notices, and the operation key.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Execution {
    private Execution() { }

    public sealed record Uniform(PinAccess Depth, Func<ProcessScope, Fin<Unit>> Step) : Execution;
    public sealed record Mixed(Func<ProcessScope, Fin<Unit>> Step) : Execution;

    public Option<PinAccess> Declared => Switch(
        uniform: static run => Some(run.Depth),
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

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ProcessScope {
    public required IDataAccess Access { get; init; }

    public required ComponentSpec Spec { get; init; }

    public required HostUnits Units { get; init; }

    public required CancellationToken Cancel { get; init; }


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
                .Bind(_ => Cell.Commit(Emitted, held => held.Add(Spec.Outputs[pin])).Switch(
                    committed: static _ => Fin.Succ(unit),
                    ceded: _ => Fin.Fail<Unit>(new KernelFault.InvalidResult(Detail: Some(nameof(Write)))),
                    refused: static row => Fin.Fail<Unit>(row.Cause),
                    contended: _ => Fin.Fail<Unit>(new KernelFault.InvalidResult(Detail: Some(nameof(Write))))))
            : Fin.Fail<Unit>(new GhFault.ContractRefused(GhContract.Component, new GhEvidence(Operation, $"output:{pin}")));

    public Unit Notify(Notice notice) => notice.Report(Access);

    public Unit Progress(int percent) => fun(() => Access.SetProgress(percent))();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Executions {
    public static Fin<Unit> Run(this Execution execution, ProcessScope scope) =>
        execution.Switch(
            state: scope,
            uniform: static (held, run) => run.Step(held),
            mixed: static (held, run) => run.Step(held));
}
```

## [03]-[SPEC]

- Owner: `ComponentSpec` carries identity, pin declarations, topology execution, iteration policy, lifecycle, threading, maintenance, bake, fleeting persistence, icon, panel, and chrome as one immutable declaration.
- Owner: `BakePolicy` is the whole document-emission declaration — `BakeUpdateMode`, the `UserPattern` attribute defaults, and the `MetaPattern` per-axis override mask — with `Added` and `Updated` as its rows and `Context` minting the host `BakeContext` against either a live `RhinoDoc` or a `File3dm`. Capability is PRESENCE: `ComponentSpec.Bakeable` is `Option<BakePolicy>`, `None` IS not-bakeable, so the `Capable` bool and its `Refused` preset (a policy carrying attribute defaults nothing can ever bake with) are unspellable. `SpecComponent.Emit(BakeContext)` calls the host's own `BakeShapes` and returns its emitted shape ids on `Fin`.
- Entry: `Admit` accumulates pin-side legality, topology coherence, iteration/threading coherence, and persistent identity before the static component declaration reaches the host constructor.
- Law: `BakeCapable` is `virtual` and `BakeShapes` is NOT — the gate overrides and the emitter is a CALL site whose host body folds every output `IBakeAware` parameter — so the override answers `Bakeable.IsSome`, and the policy VALUE behind the presence is what makes the declaration and the emission one fact. Its `string[]` return is the baked-object identity roster the run ledger attributes a bake back to its solution by.
- Law: bake attributes are declaration data, never caller literals — `UserPattern` carries the mode, group, name, layer, colour, linetype, plot, and section defaults the process applies, and `MetaPattern` carries the per-axis opt-in deciding which of those a value's own metadata may override, so two components differing only in bake attribution differ by one row and the emitter body is identical.
- Law: component logic writes fleeting state through `ProcessScope.Custom` or `Lifecycle.Post` at the host-owned boundary; process failures park on the injected `FaultCell`, and missing required outputs report from the scope that observed them. `BakeUpdateMode` rides `BakePolicy.Update` inside `ComponentSpec.Bakeable`, never a second spec column, because the mode and the capability are one declaration.
- Growth: a component capability is a policy value or an existing declaration row; a new bake posture is one `BakePolicy` row; the record never grows a builder family.
- Boundary: `OutputPlan` owns output obligation beside its `PinPlan`; no second raw-index emission roster exists. `BakeKey` coordinates, `BakeDataState` re-find filtering, and layer pre-creation stay `Grasshopper2.Bake`'s and are reached through the minted context, never re-derived here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record BakePolicy(
    Grasshopper2.Bake.BakeUpdateMode Update,
    Grasshopper2.Bake.UserPattern Defaults, Grasshopper2.Bake.MetaPattern Overrides) {
    public static readonly BakePolicy Added = new(
        Update: Grasshopper2.Bake.BakeUpdateMode.Add, Defaults: default,
        Overrides: new Grasshopper2.Bake.MetaPattern(enableAll: true, embed: true));

    public static readonly BakePolicy Updated = Added with { Update = Grasshopper2.Bake.BakeUpdateMode.Update };

    public Grasshopper2.Bake.BakeContext Context(
        string process, Guid id, Rhino.RhinoDoc document, Option<Rhino.DocObjects.ObjectAttributes> attributes = default) =>
        new(name: process, id: id, document: document,
            attributes: HostEdge.Slot(attributes)!, user: Defaults, meta: Overrides);

    public Grasshopper2.Bake.BakeContext Context(
        string process, Guid id, Rhino.FileIO.File3dm file, Option<Rhino.DocObjects.ObjectAttributes> attributes = default) =>
        new(name: process, id: id, file: file,
            attributes: HostEdge.Slot(attributes)!, user: Defaults, meta: Overrides);
}

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

    public Option<BakePolicy> Bakeable { get; init; } = default;

    public Option<Grasshopper2.UI.Icon.IIcon> Icon { get; init; } = default;

    public Option<Action<Grasshopper2.UI.InputPanel.InputPanel>> Panel { get; init; } = default;

    public Option<ComponentChrome> Chrome { get; init; } = default;

    public Validation<Error, ComponentSpec> Admit() {
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

    private Validation<Error, Unit> Topology() => Execution.Declared.Match(
        Some: depth => Inputs.ForAll(plan => plan.Access == depth && plan.Presence == PinPresence.MustExist)
            && Outputs.ForAll(output => output.Plan.Access == depth)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new GhFault.ContractRefused(GhContract.Component, new GhEvidence($"{nameof(Execution)}:{depth}"))),
        None: static () => Success<Error, Unit>(unit));

    private Validation<Error, Unit> Iteration() => Iterations.Switch(
        host: static _ => Success<Error, Unit>(unit),
        custom: _ => Threading.IsNone
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new GhFault.ContractRefused(GhContract.Component, new GhEvidence(nameof(Threading)))));

    private static Validation<Error, PinPlan> Sided(PinPlan plan, PinSide side) =>
        plan.Kind.Accepts(plan: plan, side: side).Map(_ => plan).ToValidation();
}
```

## [04]-[HOST_PROJECTION]

- Owner: `IComponentDeclaration<TSelf>` binds one static declaration to its concrete component type; `SpecComponent<TSelf>` reads the admitted value from static storage before `ModularComponent` invokes `AddInputs`, `AddOutputs`, and initial maintenance.
- Entry: host callbacks project into the declaration; per-access and custom-array processing check required outputs at the callback that owns each scope, and lifecycle stages park failures on the injected fault cell.
- Growth: a host virtual adds one declaration projection; declaration, ledger, and result ownership stay in the generic base.
- Boundary: every ABI-only throw follows injected `FaultCell` custody; runtime failures report through `IDataAccess` where available.
- Law: `Connectivity`/`ConnectivityComplete` exist on no live `Component` surface and `ComputeInternal(Solution, CallStack)` is a nonpublic virtual — all three are host plumbing the base owns, so no `Lifecycle` slot projects them and a catalog row claiming component virtuals for the first two is stale against the shipped assembly.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IComponentDeclaration<TSelf> where TSelf : IComponentDeclaration<TSelf> {
    static abstract ComponentSpec Spec { get; }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public abstract class SpecComponent<TSelf> : ModularComponent
    where TSelf : SpecComponent<TSelf>, IComponentDeclaration<TSelf> {
    private static readonly Lazy<Fin<ComponentSpec>> definition =
        new(static () => TSelf.Spec.Admit().ToFin(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static ComponentSpec Admitted(FaultCell faults, HookId point) =>
        definition.Value.IfFail(fault => Panic<ComponentSpec>(fault, faults, point));

    private readonly ComponentSpec spec;
    private readonly FaultCell faults;
    private readonly HookId faultPoint;
    private MountState state = MountState.Raw;

    protected SpecComponent(FaultCell faults, HookId faultPoint) : base(Admitted(faults, faultPoint).Identity) {
        spec = Admitted(faults, faultPoint);
        (this.faults, this.faultPoint) = (faults, faultPoint);
        spec.Threading.IfSome(held => { Threading = held; });
        state = MountState.Mounted;
    }

    public override bool BakeCapable => spec.Bakeable.IsSome;

    public Fin<string[]> Emit(Grasshopper2.Bake.BakeContext context) {
        return from row in spec.Bakeable.ToFin(new KernelFault.Unsupported(InputType: GetType(), OutputType: typeof(string[])))
               from shapes in Try.lift(() => Fin.Succ(BakeShapes(context: context, mode: row.Update))).Run().Bind(static inner => inner)
               select shapes;
    }

    protected override Grasshopper2.UI.Icon.IIcon IconInternal => spec.Icon.IfNone(base.IconInternal);

    protected override void AddInputs(ModularInputAdder inputs) =>
        ignore(Ports.Declare(inputs, spec.Inputs).IfFail(Panic<Seq<IParameter>>));

    protected override void AddOutputs(ModularOutputAdder outputs) =>
        ignore(Ports.Declare(outputs, spec.Outputs.Map(static output => output.Plan).Strict())
            .IfFail(Panic<Seq<IParameter>>));

    protected override void Process(IDataAccess access) =>
        ignore(Scope(access, access.Solution.Token).Match(
            Succ: scope => Complete(scope, spec.Execution.Run(scope), Some(access)),
            Fail: fault => Capture(fault, Some(access))));

    protected override void Process(IDataAccess[] iterations, CancellationToken token) =>
        ignore(spec.Iterations.Switch(
            state: (Self: this, Iterations: iterations, Token: token),
            host: static (state, _) => state.Self.HostIterations(state.Iterations, state.Token, state.Key),
            custom: static (state, policy) => state.Self.CustomIterations(state.Iterations, state.Token, policy, state.Key)));

    protected override void BeforeProcess(Grasshopper2.Doc.Solution solution) =>
        ignore(Track(Stage(spec.Lifecycle.Before, solution)));

    protected override void PreProcess(Grasshopper2.Doc.Solution solution) =>
        ignore(Track(Stage(spec.Lifecycle.Pre, solution)));

    protected override void PostProcess(Grasshopper2.Doc.Solution solution, Grasshopper2.Doc.FleetingCustomData data) =>
        ignore(Track(Stage(spec.Lifecycle.Post, solution, data)));

    protected override ITree PostProcessTree(ITree tree, int output, Grasshopper2.Doc.Solution solution) =>
        spec.Lifecycle.PostTree.Match(
            Some: stage => Track(Try.lift(() => stage(tree, output, solution)).Run().Bind(static inner => inner), tree),
            None: () => tree);

    public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel) =>
        ignore(Track(Try.lift(() => base.AppendToInputPanel(panel)).Run().Bind(static inner => inner)
            .Bind(_ => spec.Panel
                .TraverseM(append => Try.lift(() => append(panel)).Run().Bind(static inner => inner))
                .As()
                .Map(static _ => unit))));

    protected override Grasshopper2.Doc.IAttributes CreateAttributes() =>
        spec.Chrome.Match(
            Some: chrome => ChromeHost.Mount(this, chrome, faults, faultPoint),
            None: () => base.CreateAttributes());

    public override void VariableParameterMaintenance() =>
        ignore(state == MountState.Mounted
            ? Track(Maintained())
            : Maintained().IfFail(Panic<Unit>));

    public Fin<Unit> Flex(PinSide side, int index, PinVisibility visibility, Grasshopper2.Undo.ActionList undo) {
        ModularList list = side.Switch(state: this, input: static self => self.ModularInputs, output: static self => self.ModularOutputs);
        return Try.lift(() => visibility.Switch(
            state: (List: list, Index: index, Undo: undo),
            shown: static s => s.List.Show(s.Index, s.Undo),
            hidden: static s => s.List.Hide(s.Index, s.Undo))).Run().Bind(static inner => inner);
    }

    private Fin<ProcessScope> Scope(IDataAccess access, CancellationToken cancel) =>
        HostUnits.Of(access).Map(units => new ProcessScope {
            Access = access,
            Spec = spec,
            Units = units,
            Cancel = cancel,
        });

    private Fin<Seq<ProcessScope>> Scopes(IDataAccess[] iterations, CancellationToken cancel) =>
        toSeq(iterations).TraverseM(access => Scope(access, cancel)).As();

    private static Fin<Unit> Stage(
        Option<Func<Grasshopper2.Doc.Solution, Fin<Unit>>> stage,
        Grasshopper2.Doc.Solution solution) => stage
            .TraverseM(action => Try.lift(() => action(solution)).Run().Bind(static inner => inner))
            .As()
            .Map(static _ => unit);

    private static Fin<Unit> Stage(
        Option<Func<Grasshopper2.Doc.Solution, Grasshopper2.Doc.FleetingCustomData, Fin<Unit>>> stage,
        Grasshopper2.Doc.Solution solution,
        Grasshopper2.Doc.FleetingCustomData data) => stage
            .TraverseM(action => Try.lift(() => action(solution, data)).Run().Bind(static inner => inner))
            .As()
            .Map(static _ => unit);

    private Fin<Unit> Maintained() => spec.Maintain
        .TraverseM(maintain => Try.lift(() => maintain(Parameters)).Run().Bind(static inner => inner))
        .As()
        .Map(static _ => unit)
        .Bind(_ => Ports.Realize(
            Parameters,
            spec.Inputs,
            spec.Outputs.Map(static output => output.Plan).Strict()).ToFin());

    private Unit HostIterations(IDataAccess[] iterations, CancellationToken token) =>
        Try.lift(() => Fin.Succ(HostEdge.Side(() => ProcessHost(iterations, token)))).Run().Bind(static inner => inner)
            .IfFail(fault => Capture(fault, None));

    private void ProcessHost(IDataAccess[] iterations, CancellationToken token) => base.Process(iterations, token);

    private Unit CustomIterations(IDataAccess[] iterations, CancellationToken token, IterationPolicy.Custom policy) =>
        Scopes(iterations, token).Match(
            Succ: scopes => Complete(scopes, Try.lift(() => policy.Step(scopes, token)).Run().Bind(static inner => inner)),
            Fail: fault => Capture(fault, None));

    private Unit Complete(Seq<ProcessScope> scopes, Fin<Unit> result) =>
        (ignore(scopes.Map(scope => Warn(scope, None)).Strict()),
            result.IfFail(fault => Capture(fault, None))).Item2;

    private Unit Complete(ProcessScope scope, Fin<Unit> result, Option<IDataAccess> access) =>
        (Warn(scope, access), result.IfFail(fault => Capture(fault, access))).Item2;

    private static Unit Warn(ProcessScope scope, Option<IDataAccess> access) =>
        scope.Spec.Outputs.Filter(output => output.IsRequired && !scope.Emitted.Value.Contains(output)).Strict() is var missing
        && missing.IsEmpty
            ? unit
            : access.Iter(target => new Notice(
                    Severity.Warning,
                    None,
                    nameof(OutputPlan.Required),
                    string.Join(",", missing.Map(static output => output.Plan.Nick)),
                    []).Report(target));

    private Unit Track(Fin<Unit> result) => result.IfFail(fault => Capture(fault, None));

    private T Track<T>(Fin<T> result, T fallback) =>
        result.IfFail(fault => (Capture(fault, None), fallback).Item2);

    private Unit Capture(Error fault, Option<IDataAccess> access) =>
        (ignore(faults.Park(point: faultPoint, cause: fault)), Report(fault, access)).Item2;

    private static Unit Report(Error fault, Option<IDataAccess> access) =>
        access.Iter(target => Notice.Fan(fault).Iter(notice => notice.Report(target)));

    private T Panic<T>(Error fault) {
        Capture(fault, None);
        throw fault.ToException();
    }

    private static T Panic<T>(Error fault, FaultCell faults, HookId point) {
        ignore(faults.Park(point: point, cause: fault));
        throw fault.ToException();
    }
}
```

## [05]-[PLUGIN]

- Owner: `PluginSpec` is the registration declaration; `PluginSource` closes public path and assembly loading under one `Catalogue.Load`; `SpecPlugin` projects metadata and audits exported component types at the host load edge.
- Entry: `Catalogue.Exported(PluginSpec, FaultCell, HookId)` is the audited loader roster with mandatory custody before ABI egress; `Catalogue.Load` lifts the host result onto `Fin<Unit>`.
- Growth: a public plugin ingress is one `PluginSource` case and one load arm; plugin metadata is one `PluginSpec` member and one host override.
- Boundary: assembly harvesting remains inside `PluginServer`; local reflection is limited to exported-type declaration and persistent-id admission.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PluginSource {
    private PluginSource() { }

    public sealed record Location(string Path) : PluginSource;
    public sealed record Binary(string Location, System.Reflection.Assembly Value) : PluginSource;
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record PluginSpec(
    Guid Id,
    Grasshopper2.UI.Nomen Identity,
    Version Version,
    Seq<Type> Exported,
    Seq<string> Satellites,
    Option<string> Author = default,
    Option<string> Copyright = default);

// --- [COMPOSITION] ---------------------------------------------------------------------

public abstract class SpecPlugin : Grasshopper2.Framework.Plugin {
    private readonly PluginSpec spec;
    private readonly FaultCell faults;
    private readonly HookId faultPoint;

    protected SpecPlugin(PluginSpec spec, FaultCell faults, HookId faultPoint) : base(spec.Id, spec.Identity, spec.Version) =>
        (this.spec, this.faults, this.faultPoint) = (spec, faults, faultPoint);

    public override IEnumerable<Type> ExportedTypes => Catalogue.Exported(spec, faults, faultPoint);

    public override IEnumerable<string> SatelliteAssemblies => spec.Satellites;

    public override string Author => spec.Author.IfNone(base.Author);

    public override string Copyright => spec.Copyright.IfNone(base.Copyright);

    public override void OnLoaded() => ignore(Catalogue.Exported(spec, faults, faultPoint));
}

public static class Catalogue {
    public static Validation<Error, PluginSpec> Audit(PluginSpec plugin) =>
        plugin.Exported.Traverse(type => Exported(type)).As().Map(_ => plugin);

    public static Seq<Type> Exported(PluginSpec plugin, FaultCell faults, HookId point) => Audit(plugin).Match(
        Succ: static audited => audited.Exported,
        Fail: fault => {
            ignore(faults.Park(point: point, cause: fault));
            throw fault.ToException();
        });

    public static Fin<Unit> Load(PluginSource source) => source.Switch(
        location: static (row) => Try.lift(() =>
                Grasshopper2.Framework.PluginServer.LoadPlugin(row.Path, out Grasshopper2.Framework.FailureInfo failure)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GhFault.Registration($"{row.Path}:{failure}"))).Run().Bind(static inner => inner),
        binary: static (row) => Try.lift(() =>
                Grasshopper2.Framework.PluginServer.LoadPlugin(row.Location, row.Value, out Grasshopper2.Framework.FailureInfo failure)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GhFault.Registration($"{row.Location}:{failure}"))).Run().Bind(static inner => inner));

    public static Option<Grasshopper2.Framework.Plugin> OwnerOf(Grasshopper2.Doc.IDocumentObject subject) =>
        Optional(Grasshopper2.Framework.PluginServer.FindPluginForObject(subject));

    private static Validation<Error, Type> Exported(Type type) =>
        (type.GetInterfaces().Any(contract =>
                contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IComponentDeclaration<>)
                && contract.GenericTypeArguments.Single() == type),
            Attribute.IsDefined(type, typeof(GrasshopperIO.IoIdAttribute))) switch {
            (true, true) => type,
            (false, _) => new GhFault.Registration($"{type.Name}:{typeof(IComponentDeclaration<>).Name}"),
            (_, false) => new GhFault.Registration($"{type.Name}:{nameof(GrasshopperIO.IoIdAttribute)}"),
        };
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

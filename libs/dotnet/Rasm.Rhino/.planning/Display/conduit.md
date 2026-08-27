# [RASM_RHINO_DISPLAY_CONDUIT]

`Conduits.Mount` owns filtered display-pipeline participation as a leased phase program with balanced render state and a bounded, observable callback-fault cell. Retained overlays and registered analysis remain distinct lifetime shapes under the same host boundary, and both draw through the ONE `Marks.Paint` dispatch — a private draw path beside it is the deleted form.

`ConduitFrame` is the draw boundary `Marks.Paint`'s pipeline canvas consumes. Viewport identity and pass facts detach immediately through the one generated projection, while the raw `DisplayPipeline` remains scoped to the host callback that supplied it.

## [01]-[INDEX]

- [02]-[PROGRAM]: `PhaseCapability`, `PhaseHost`, `ConduitPhase`, `RenderAspect`, `SwitchState`, `RenderSwitch`, `CullUse`, `ConduitCriterion`, `BoundsRole`, `DrawProjector`, `ConduitStep`, `ConduitProgram`, `FramePosture`, `FrameContext`, `ConduitFrame`, `Cases` — phase, filter, state policy, and the once-partitioned step lanes.
- [03]-[MOUNT]: `DisplayFaults`, `ConduitAdapter`, `PipelineScope`, `ConduitLease`, `Conduits`, `ConduitVetoAsk`, `ConduitHooks` — binding, bounded callback faults, disablement, and unbinding.
- [04]-[OVERLAYS]: `AnalysisMode`, `ModeParticipation`, `AnalysisScale`, `AnalysisLaw`, `MeshComponent`, `AnalysisOverlay`, `OverlayVisibility`, `RetainedRequest`, `RetainedState`, `RetainedOverlay` — registered false-colour analysis and the retained `CustomDisplay` capsule.

## [02]-[PROGRAM]

- Owner: `ConduitPhase` is the package-wide draw-boundary vocabulary — each row carries ONE `CapabilitySet<PhaseCapability>` and the `PhaseHost` case naming who hands out the live pipeline; `ConduitStep` closes the phase program at four cases with `DrawProjector` folding the two draw arities into one case; `ConduitProgram` admits the program and partitions its steps into typed lanes ONCE.
- Cases: `PhaseCapability` is `Draws`, `PerObject`, and `WorldSpace` — the 44 four-bool row literals collapse onto eleven set literals; `PhaseHost` is `Conduit` (a `DisplayConduit` override phase — the only host a program MOUNTS), `Engine` (a realtime framebuffer or middleground event), or `Widget` (a registered widget's `OnDraw`) — the `Mounts` bool was this union flattened, and a producer stamping a phase states which host handed it the pipeline rather than a flag consumers re-derive.
- Law: the legal capability corners DERIVE from the roster — `ConduitPhase.Law` is the distinct set of row sets behind an accessor-backed lazy, so a twelfth phase lands as one row and the law re-materializes; a hand-kept corner list beside the rows is two models of one fact.
- Law: veto is host truth — `Cull` can only widen the incoming `CullObjectEventArgs.CullObject` and `Suppress` can only narrow the incoming `DrawObjectEventArgs.DrawObject`, the only two suppression flags the display contract admits; a prior host veto remains set, each decide answers per object per frame, and any deciding step voting to suppress wins.
- Law: the two veto steps answer DISTINCT verdict owners — `CullVerdict` spells visible-versus-culled and `SuppressVerdict` drawn-versus-suppressed — because their predicates are structurally identical over inverted senses, so a swapped delegate is a compile error rather than a silently mirrored frame.
- Law: a drawing step's ARITY is its projector's case — `DrawProjector.PerObject` demands a phase whose set admits `PerObject` and `DrawProjector.Frame` one that does not — so the old sibling cases `Draw`/`ObjectDraw` collapse and a mismatched pairing refuses at admission; the bounds obligation is `BoundsRole`, derived from the phase's `WorldSpace` capability, and the `(bool Supplies, bool Requires)` tuple deletes.
- Law: steps PARTITION at `ConduitProgram.Of` into typed lanes — the cull lane, the suppress lane, and the per-phase bounds and draw maps — so the host callbacks read a frozen lane instead of re-running `Choose` and `Filter` over the whole step roster per object per frame.
- Law: `ConduitCriterion` turns every host filter axis into one case-unique row inside the mount request; case runtime type is the uniqueness key, and `Cases.Unique` is the ONE shared admission fold — criteria here and the mode policies and appearance concerns on `Display/modes.md` all admit through it.
- Law: `FrameContext` detaches through the generated `FrameMap` — the six per-frame host reads are one `[Mapper]` projection, its three posture bools ride ONE `CapabilitySet<FramePosture>`, and its density is an admitted `PositiveMagnitude`.
- Boundary: callback failures park on the lease's bounded fault cell; a host callback never discards a failed result.
- Growth: a pipeline phase is one row; a render state one `RenderAspect` case and one total adapter arm; a filter axis one criterion case.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using System.Collections.Frozen;
using Thinktecture;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhaseCapability : ICapability<PhaseCapability> {
    public static readonly PhaseCapability Draws = new(key: "draws");
    public static readonly PhaseCapability PerObject = new(key: "per-object");
    public static readonly PhaseCapability WorldSpace = new(key: "world-space");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PhaseHost {
    private PhaseHost() { }
    public sealed record Conduit : PhaseHost;
    public sealed record Engine : PhaseHost;
    public sealed record Widget : PhaseHost;
}

[SmartEnum<int>]
public sealed partial class ConduitPhase {
    public static readonly ConduitPhase Culling = new(key: 0, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.PerObject), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase Bounds = new(key: 1, capabilities: CapabilitySet<PhaseCapability>.Of(), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase BoundsZoomExtents = new(key: 2, capabilities: CapabilitySet<PhaseCapability>.Of(), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase PreObjects = new(key: 3, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws, PhaseCapability.WorldSpace), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase PreObject = new(key: 4, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws, PhaseCapability.PerObject, PhaseCapability.WorldSpace), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase PostObjects = new(key: 5, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws, PhaseCapability.WorldSpace), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase Foreground = new(key: 6, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase Overlay = new(key: 7, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws), host: new PhaseHost.Conduit());
    public static readonly ConduitPhase Framebuffer = new(key: 8, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws), host: new PhaseHost.Engine());
    public static readonly ConduitPhase Middleground = new(key: 9, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws, PhaseCapability.WorldSpace), host: new PhaseHost.Engine());
    public static readonly ConduitPhase WidgetOverlay = new(key: 10, capabilities: CapabilitySet<PhaseCapability>.Of(PhaseCapability.Draws, PhaseCapability.WorldSpace), host: new PhaseHost.Widget());

    public CapabilitySet<PhaseCapability> Capabilities { get; }
    public PhaseHost Host { get; }

    public static CapabilityLaw<PhaseCapability> Law => Corners.Value;
    private static readonly Lazy<CapabilityLaw<PhaseCapability>> Corners =
        new(static () => new CapabilityLaw<PhaseCapability>(toSeq(Items).Map(static row => row.Capabilities).Distinct()));
}

[SmartEnum<bool>]
public sealed partial class SwitchState {
    public static readonly SwitchState On = new(key: true);
    public static readonly SwitchState Off = new(key: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderAspect {
    private RenderAspect() { }
    public sealed record Toggle(RenderSwitch Target, SwitchState State) : RenderAspect;
    public sealed record Cull(CullUse Mode) : RenderAspect;
    public sealed record Model(Transform Transform) : RenderAspect;
    public sealed record Screen : RenderAspect;

    internal bool Valid => Switch(
        toggle: static row => row.Target is not null && row.State is not null,
        cull: static row => row.Mode is not null,
        model: static row => row.Transform.IsValid,
        screen: static _ => true);

    internal Fin<Unit> With(DisplayPipeline pipeline, Func<Fin<Unit>> draw) {
        bool acquired = false;
        Fin<Unit> primary = Try.lift(() => {
            Switch(
                pipeline,
                toggle: static (p, row) => row.Target.Push(p, row.State.Key),
                cull: static (p, row) => HostEdge.Side(() => p.PushCullFaceMode(row.Mode.Native)),
                model: static (p, row) => HostEdge.Side(() => p.PushModelTransform(row.Transform)),
                screen: static (p, _) => HostEdge.Side(p.Push2dProjection));
            acquired = true;
            return draw();
        }).Run().Bind(static inner => inner);
        Fin<Unit> cleanup = acquired
            ? Try.lift(() => Fin.Succ(Switch(
                pipeline,
                toggle: static (p, row) => row.Target.Pop(p),
                cull: static (p, _) => HostEdge.Side(p.PopCullFaceMode),
                model: static (p, _) => HostEdge.Side(p.PopModelTransform),
                screen: static (p, _) => HostEdge.Side(p.PopProjection)))).Run().Bind(static inner => inner)
            : Fin.Succ(unit);
        return primary.Match(
            Succ: _ => cleanup,
            Fail: failure => cleanup.Match(
                Succ: _ => Fin.Fail<Unit>(failure),
                Fail: compensation => Fin.Fail<Unit>(failure + compensation)));
    }
}

[SmartEnum<int>]
public sealed partial class RenderSwitch {
    public static readonly RenderSwitch DepthTest = new(
        key: 0,
        push: static (pipeline, enabled) => HostEdge.Side(() => pipeline.PushDepthTesting(enabled)),
        pop: static pipeline => HostEdge.Side(pipeline.PopDepthTesting));
    public static readonly RenderSwitch DepthWrite = new(
        key: 1,
        push: static (pipeline, enabled) => HostEdge.Side(() => pipeline.PushDepthWriting(enabled)),
        pop: static pipeline => HostEdge.Side(pipeline.PopDepthWriting));
    public static readonly RenderSwitch ClipTest = new(
        key: 2,
        push: static (pipeline, enabled) => HostEdge.Side(() => pipeline.PushClipTesting(enabled)),
        pop: static pipeline => HostEdge.Side(pipeline.PopClipTesting));

    [UseDelegateFromConstructor]
    internal partial Unit Push(DisplayPipeline pipeline, bool enabled);

    [UseDelegateFromConstructor]
    internal partial Unit Pop(DisplayPipeline pipeline);
}

[SmartEnum<int>]
public sealed partial class CullUse {
    public static readonly CullUse Both = new(key: 0, native: CullFaceMode.DrawFrontAndBack);
    public static readonly CullUse Front = new(key: 1, native: CullFaceMode.DrawFrontFaces);
    public static readonly CullUse Back = new(key: 2, native: CullFaceMode.DrawBackFaces);

    internal CullFaceMode Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitCriterion {
    private ConduitCriterion() { }
    public sealed record Selection(SelectionUse Use) : ConduitCriterion;
    public sealed record Objects(Seq<Guid> Ids) : ConduitCriterion;
    public sealed record Geometry(ObjectKinds Kinds) : ConduitCriterion;
    public sealed record Space(ActiveSpaceUse Value) : ConduitCriterion;

    internal bool Valid => Switch(
        selection: static row => row.Use is not null,
        objects: static row => !row.Ids.IsEmpty && row.Ids.ForAll(static id => id != Guid.Empty),
        geometry: static row => row.Kinds is not null,
        space: static row => row.Value is not null);

    internal Unit Apply(DisplayConduit conduit) => Switch(
        conduit,
        selection: static (c, row) => HostEdge.Side(() => c.SetSelectionFilter(row.Use.Enabled, row.Use.SubObjects)),
        objects: static (c, row) => HostEdge.Side(() => c.SetObjectIdFilter(row.Ids.Distinct().AsEnumerable())),
        geometry: static (c, row) => HostEdge.Side(() => c.GeometryFilter = row.Kinds.Mask),
        space: static (c, row) => HostEdge.Side(() => c.SpaceFilter = row.Value.Key));
}

[SmartEnum<int>]
public sealed partial class SelectionUse {
    public static readonly SelectionUse Disabled = new(key: 0, enabled: false, subObjects: false);
    public static readonly SelectionUse Objects = new(key: 1, enabled: true, subObjects: false);
    public static readonly SelectionUse SubObjects = new(key: 2, enabled: true, subObjects: true);

    internal bool Enabled { get; }
    internal bool SubObjects { get; }
}

[SmartEnum<int>]
public sealed partial class BindUse {
    public static readonly BindUse Shared = new(
        key: 0,
        bind: static (conduit, viewport) => HostEdge.Side(() => conduit.Bind(viewport)));
    public static readonly BindUse Exclusive = new(
        key: 1,
        bind: static (conduit, viewport) => HostEdge.Side(() => conduit.ExclusiveBind(viewport)));

    [UseDelegateFromConstructor]
    internal partial Unit Bind(DisplayConduit conduit, RhinoViewport viewport);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitBinding {
    private ConduitBinding() { }
    public sealed record Global : ConduitBinding;
    public sealed record Viewport(ViewportTarget Target, BindUse Use) : ConduitBinding;

    internal bool Valid => Switch(
        global: static _ => true,
        viewport: static row => row.Target is not null && row.Use is not null);
}

[SmartEnum<bool>]
public sealed partial class CullVerdict {
    public static readonly CullVerdict Visible = new(key: false);
    public static readonly CullVerdict Culled = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class SuppressVerdict {
    public static readonly SuppressVerdict Drawn = new(key: false);
    public static readonly SuppressVerdict Suppressed = new(key: true);
}

[SmartEnum<int>]
public sealed partial class BoundsRole {
    public static readonly BoundsRole None = new(key: 0);
    public static readonly BoundsRole Supplies = new(key: 1);
    public static readonly BoundsRole Requires = new(key: 2);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FramePosture : ICapability<FramePosture> {
    public static readonly FramePosture Capturing = new(key: "capturing");
    public static readonly FramePosture Printing = new(key: "printing");
    public static readonly FramePosture Dynamic = new(key: "dynamic");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FrameContext(
    CapabilitySet<FramePosture> Postures,
    int RenderPass,
    int NestLevel,
    PositiveMagnitude DpiScale);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class FrameMap {
    [MapProperty(nameof(DisplayPipeline.RenderPass), nameof(FrameContext.RenderPass))]
    [MapProperty(nameof(DisplayPipeline.NestLevel), nameof(FrameContext.NestLevel))]
    [MapProperty(nameof(DisplayPipeline.DpiScale), nameof(FrameContext.DpiScale))]
    internal static partial FrameContext Detach(DisplayPipeline pipeline);

    [UserMapping(Default = false)]
    private static CapabilitySet<FramePosture> Postures(DisplayPipeline pipeline) =>
        CapabilitySet<FramePosture>.Of([
            .. pipeline.IsInViewCapture ? Seq(FramePosture.Capturing) : Seq<FramePosture>(),
            .. pipeline.IsPrinting ? Seq(FramePosture.Printing) : Seq<FramePosture>(),
            .. pipeline.IsDynamicDisplay ? Seq(FramePosture.Dynamic) : Seq<FramePosture>(),
        ]);

    [UserMapping(Default = true)]
    private static PositiveMagnitude Density(float dpiScale) => PositiveMagnitude.Create(value: dpiScale);
}

public readonly record struct ConduitFrame {
    private ConduitFrame(DisplayPipeline pipeline, Guid viewport, uint change, FrameContext context, ConduitPhase phase) =>
        (Pipeline, Viewport, Change, Context, Phase) = (pipeline, viewport, change, context, phase);
    internal DisplayPipeline Pipeline { get; }
    public Guid Viewport { get; }
    public uint Change { get; }
    public FrameContext Context { get; }
    public ConduitPhase Phase { get; }

    internal static ConduitFrame Of(DisplayPipeline pipeline, RhinoViewport viewport, ConduitPhase phase) =>
        new(pipeline, viewport.Id, viewport.ChangeCounter, FrameMap.Detach(pipeline), phase);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DrawProjector {
    private DrawProjector() { }
    public sealed record Frame(Func<ConduitFrame, Fin<Seq<DisplayMark>>> Project) : DrawProjector;
    public sealed record PerObject(Func<Guid, ConduitFrame, Fin<Seq<DisplayMark>>> Project) : DrawProjector;

    internal bool Matches(ConduitPhase phase) => Switch(
        state: phase,
        frame: static (at, _) => !at.Capabilities.Admits(PhaseCapability.PerObject),
        perObject: static (at, _) => at.Capabilities.Admits(PhaseCapability.PerObject));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitStep {
    private ConduitStep() { }
    public sealed record Cull(Func<Guid, ConduitFrame, Fin<CullVerdict>> Decide) : ConduitStep;
    public sealed record Suppress(Func<Guid, ConduitFrame, Fin<SuppressVerdict>> Decide) : ConduitStep;
    public sealed record Bounds(ConduitPhase Phase, Func<ConduitFrame, Fin<BoundingBox>> Contribute) : ConduitStep;
    public sealed record Draw(ConduitPhase Phase, Seq<RenderAspect> State, DrawProjector Projector) : ConduitStep;

    internal bool Valid => Switch(
        cull: static row => row.Decide is not null,
        suppress: static row => row.Decide is not null,
        bounds: static row => row.Contribute is not null && (row.Phase == ConduitPhase.Bounds || row.Phase == ConduitPhase.BoundsZoomExtents),
        draw: static row => row.Projector is not null
            && row.Phase.Host is PhaseHost.Conduit
            && row.Phase.Capabilities.Admits(PhaseCapability.Draws)
            && row.Projector.Matches(row.Phase)
            && row.State.ForAll(static aspect => aspect is not null && aspect.Valid));

    internal BoundsRole Bounds_ => Switch(
        cull: static _ => BoundsRole.None,
        suppress: static _ => BoundsRole.None,
        bounds: static row => row.Phase == ConduitPhase.Bounds ? BoundsRole.Supplies : BoundsRole.None,
        draw: static row => row.Phase.Capabilities.Admits(PhaseCapability.WorldSpace) ? BoundsRole.Requires : BoundsRole.None);
}

public sealed record ConduitProgram {
    private ConduitProgram(
        Seq<ConduitStep> steps,
        ConduitBinding binding,
        Seq<ConduitCriterion> criteria,
        Seq<ConduitStep.Cull> culls,
        Seq<ConduitStep.Suppress> suppresses,
        HashMap<ConduitPhase, Seq<ConduitStep.Bounds>> bounds,
        HashMap<ConduitPhase, Seq<ConduitStep.Draw>> draws) =>
        (Steps, Binding, Criteria, Culls, Suppresses, BoundsLanes, DrawLanes) =
        (steps, binding, criteria, culls, suppresses, bounds, draws);

    public Seq<ConduitStep> Steps { get; }
    public ConduitBinding Binding { get; }
    public Seq<ConduitCriterion> Criteria { get; }
    internal Seq<ConduitStep.Cull> Culls { get; }
    internal Seq<ConduitStep.Suppress> Suppresses { get; }
    internal HashMap<ConduitPhase, Seq<ConduitStep.Bounds>> BoundsLanes { get; }
    internal HashMap<ConduitPhase, Seq<ConduitStep.Draw>> DrawLanes { get; }

    public static Fin<ConduitProgram> Of(
        Seq<ConduitStep> steps,
        ConduitBinding binding,
        Seq<ConduitCriterion> criteria) {
        bool ordered = steps.Map(static step => step.Bounds_)
            .Fold(
                (Supplied: false, Valid: true),
                static (state, role) => (
                    Supplied: state.Supplied || role == BoundsRole.Supplies,
                    Valid: state.Valid && (role != BoundsRole.Requires || state.Supplied)))
            .Valid;
        return (
                (!steps.IsEmpty && steps.ForAll(static step => step is not null && step.Valid)
                    ? Validation<Error, Seq<ConduitStep>>.Success(steps)
                    : Validation<Error, Seq<ConduitStep>>.Fail(new KernelFault.InvalidInput(Axis: Some(nameof(steps))))),
                (criteria.ForAll(static criterion => criterion is not null && criterion.Valid) && Cases.Unique(criteria)
                    ? Validation<Error, Seq<ConduitCriterion>>.Success(criteria)
                    : Validation<Error, Seq<ConduitCriterion>>.Fail(new KernelFault.InvalidInput(Axis: Some(nameof(criteria))))),
                (binding is { Valid: true }
                    ? Validation<Error, ConduitBinding>.Success(binding)
                    : Validation<Error, ConduitBinding>.Fail(new KernelFault.InvalidInput(Axis: Some(nameof(binding))))),
                (ordered
                    ? Validation<Error, Unit>.Success(unit)
                    : Validation<Error, Unit>.Fail(new KernelFault.InvalidInput(Axis: Some(nameof(BoundsRole))))))
            .Apply(static (admitted, rows, bound, _) => (Steps: admitted, Criteria: rows, Binding: bound))
            .As().ToFin()
            .Map(held => new ConduitProgram(
                held.Steps,
                held.Binding,
                held.Criteria,
                culls: held.Steps.Choose(static step => step is ConduitStep.Cull row ? Some(row) : None),
                suppresses: held.Steps.Choose(static step => step is ConduitStep.Suppress row ? Some(row) : None),
                bounds: toSeq(held.Steps.Choose(static step => step is ConduitStep.Bounds row ? Some(row) : None)
                    .GroupBy(static row => row.Phase)).Fold(
                        HashMap<ConduitPhase, Seq<ConduitStep.Bounds>>(),
                        static (map, group) => map.Add(group.Key, toSeq(group))),
                draws: toSeq(held.Steps.Choose(static step => step is ConduitStep.Draw row ? Some(row) : None)
                    .GroupBy(static row => row.Phase)).Fold(
                        HashMap<ConduitPhase, Seq<ConduitStep.Draw>>(),
                        static (map, group) => map.Add(group.Key, toSeq(group)))));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Cases {
    internal static bool Unique<T>(Seq<T> rows) where T : class =>
        rows.Map(static row => row.GetType()).Distinct().Count == rows.Count;
}
```

## [03]-[MOUNT]

- Owner: `ConduitLease` owns the adapter and its bounded callback-fault cell until deterministic release; `DisplayFaults` declares the ONE cap every long-lived display owner's cell mints under; `ConduitHooks` registers the two display veto points as TYPED hook bindings.
- Entry: `ConduitProgram.Of` admits and partitions; `Conduits.Mount` applies the admitted program and arms participation; `ConduitHooks.Mount` seats `rasm.rhino.display.cull` and `rasm.rhino.display.drawobject` on the `MountRegistry` — the ask is a typed `ConduitVetoAsk` whose program must carry the point's own veto step, the grant is the mounted `ConduitLease`, and a program missing the veto step refuses typed before any host participation arms.
- Law: the fault cell is the kernel `FaultCell` — a bounded ring whose parks, sheds, and declined parks all read as numbers — never an unbounded `Atom<Seq<Error>>`; the cap is DECLARED at `DisplayFaults.Cap`, because every one of these owners is host-activated or process-static and no policy reaches a constructor, and a per-owner cap beside the declared one is the fork.
- Law: every parked fault also publishes through `ObjectsTelemetry` under `FaultSite.Conduit` — the cell is the lease's readable fault roster, the publish the process egress, and a second logger sink beside them is the fork.
- Law: a draw lane's REFUSAL rows park too — `Marks.Paint` accounts a capability-illegal mark as a typed refusal on its `DrawTally`, and the adapter parks each cause, so a projector emitting a mark its canvas cannot draw is observable rather than silent.
- Law: release composes kernel `Custody.Release` — disable, `UnbindAll`, sprite disposal, every step running even when an earlier one refuses, failures aggregating through `Error.Many` — and the lease's one-shot is a stepped transition whose failed release re-arms while its verdict parks on the cell.
- Boundary: the adapter is the only `DisplayConduit` subclass and the only statement-shaped host callback boundary.

```csharp
// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class DisplayFaults {
    internal static readonly Rasm.Numerics.Dimension Cap = Rasm.Numerics.Dimension.Create(value: 256);
    internal static FaultCell Cell() => new(cap: Cap, clock: TimeProvider.System);
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ConduitAdapter : DisplayConduit {
    private readonly ConduitProgram program;
    private readonly FaultCell faults;
    private readonly SpriteSheet sprites = new();
    private static readonly HookId HookPoint = HookId.Create(value: "rasm.rhino.display.conduit");

    internal ConduitAdapter(ConduitProgram program, FaultCell faults) =>
        (this.program, this.faults, this.key) = (program, faults);

    protected override void ObjectCulling(CullObjectEventArgs e) => Invoke(() =>
        program.Culls
            .TraverseM(step => step.Decide(
                e.RhinoObject.Id,
                ConduitFrame.Of(e.Display, e.Viewport, ConduitPhase.Culling))).As()
            .Map(verdicts => (e.CullObject = e.CullObject || verdicts.Exists(static verdict => verdict.Key), unit).Item2));

    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) => Invoke(() =>
        Bounds(e, ConduitPhase.Bounds));

    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) => Invoke(() =>
        Bounds(e, ConduitPhase.BoundsZoomExtents));

    protected override void PreDrawObjects(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.PreObjects));

    protected override void PreDrawObject(DrawObjectEventArgs e) => Invoke(() =>
        program.Suppresses
            .TraverseM(step => step.Decide(
                e.RhinoObject.Id,
                ConduitFrame.Of(e.Display, e.Viewport, ConduitPhase.PreObject))).As()
            .Map(verdicts => (e.DrawObject = e.DrawObject && !verdicts.Exists(static verdict => verdict.Key), unit).Item2)
            .Bind(_ => e.DrawObject
                ? program.DrawLanes.Find(ConduitPhase.PreObject).IfNone(Seq<ConduitStep.Draw>())
                    .TraverseM(step => Project(e, step)).As()
                    .Map(static _ => unit)
                : Fin.Succ(value: unit)));

    protected override void PostDrawObjects(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.PostObjects));
    protected override void DrawForeground(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.Foreground));
    protected override void DrawOverlay(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.Overlay));

    private Fin<Unit> Bounds(CalculateBoundingBoxEventArgs e, ConduitPhase phase) =>
        program.BoundsLanes.Find(phase).IfNone(Seq<ConduitStep.Bounds>())
            .TraverseM(step => step.Contribute(ConduitFrame.Of(e.Display, e.Viewport, phase))
                .Bind(box => guard(box.IsValid, new KernelFault.InvalidResult()).ToFin()
                    .Map(_ => HostEdge.Side(() => e.IncludeBoundingBox(box))))).As()
            .Map(static _ => unit);

    private Fin<Unit> Draw(DrawEventArgs e, ConduitPhase phase) =>
        program.DrawLanes.Find(phase).IfNone(Seq<ConduitStep.Draw>())
            .TraverseM(step => {
                ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, phase);
                return step.Projector switch {
                    DrawProjector.Frame projector => Render(frame, step.State, projector.Project(frame)),
                    DrawProjector.PerObject => Fin.Succ(unit),
                    _ => Fin.Fail<Unit>(new KernelFault.InvalidInput()),
                };
            }).As()
            .Map(static _ => unit);

    private Fin<Unit> Render(ConduitFrame frame, Seq<RenderAspect> state, Fin<Seq<DisplayMark>> projected) =>
        PipelineScope.With(frame.Pipeline, state, () => projected
            .Bind(marks => Marks.Paint(new Canvas.Pipeline(frame, sprites), marks))
            .Map(tally => tally.Refused.Fold(unit, (_, cause) => ignore(faults.Park(point: HookPoint, cause: cause)))));

    private Fin<Unit> Project(DrawObjectEventArgs e, ConduitStep.Draw step) {
        ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, step.Phase);
        return step.Projector switch {
            DrawProjector.PerObject projector => Render(frame, step.State, projector.Project(e.RhinoObject.Id, frame)),
            _ => Fin.Succ(unit),
        };
    }

    private void Invoke(Func<Fin<Unit>> callback) => Observe(Try.lift(callback).Run().Bind(static inner => inner));

    private void Observe<T>(Fin<T> outcome) =>
        outcome.IfFail(error => ignore((
            faults.Park(point: HookPoint, cause: error),
            ObjectsTelemetry.Publish(site: FaultSite.Conduit, error: error))));

    internal Fin<Unit> Release() => Custody.Release(
        releases: Seq<Func<Fin<Unit>>>(
            () => Try.lift(() => { Enabled = false; return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
            () => Try.lift(() => { UnbindAll(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
            () => sprites.Release()));
}

internal static class PipelineScope {
    internal static Fin<TResult> With<TResult>(DisplayPipeline pipeline, Seq<RenderAspect> state, Func<Fin<TResult>> draw) {
        Fin<TResult> slot = Fin.Fail<TResult>(new KernelFault.InvalidResult());
        Fin<Unit> crossed = toSeq(state.AsEnumerable().Reverse())
            .Fold<Func<Fin<Unit>>>(
                () => (slot = draw()).Map(static _ => unit),
                (next, aspect) => () => aspect.With(pipeline, next))();
        return crossed.Bind(_ => slot);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LeaseGate {
    private LeaseGate() { }
    internal sealed record Live : LeaseGate;
    internal sealed record Released : LeaseGate;
}

public sealed class ConduitLease : IDisposable {
    private readonly ConduitAdapter adapter;
    private readonly FaultCell faults;
    private readonly Atom<LeaseGate> gate = Atom<LeaseGate>(new LeaseGate.Live());

    internal ConduitLease(ConduitAdapter adapter, FaultCell faults) =>
        (this.adapter, this.faults, this.key) = (adapter, faults);

    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public void Dispose() {
        Transition<LeaseGate> claimed = Cell.Step(
            gate,
            static held => held is LeaseGate.Live ? Some<LeaseGate>(new LeaseGate.Released()) : None,
            new KernelFault.InvalidContext());
        _ = HostEdge.SideWhen(claimed is Transition<LeaseGate>.Committed, () => adapter.Release().IfFail(cause => {
            _ = Cell.Step(gate, static held => held is LeaseGate.Released ? Some<LeaseGate>(new LeaseGate.Live()) : None, Errors.None);
            _ = faults.Park(point: HookId.Create(value: "rasm.rhino.display.conduit"), cause: cause);
        }));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    public static Fin<ConduitLease> Mount(
        DocumentSession session,
        ConduitProgram program) {
        return from owner in Optional(session).ToFin(new KernelFault.MissingContext())
               from admitted in Optional(program).ToFin(new KernelFault.InvalidInput())
               from faults in Fin.Succ(DisplayFaults.Cell())
               from adapter in Fin.Succ(new ConduitAdapter(admitted, faults))
               from lease in (from __ in Try.lift(() => Fin.Succ(admitted.Criteria
                                  .Fold(unit, static (_, criterion) => criterion.Apply(adapter)))).Run().Bind(static inner => inner)
                              from ___ in Bind(owner, adapter, admitted.Binding)
                              from ____ in Try.lift(() => Fin.Succ((adapter.Enabled = true, unit).Item2)).Run().Bind(static inner => inner)
                              select new ConduitLease(adapter, faults))
                                  .Rollback(release: adapter.Release)
               select lease;
    }

    private static Fin<Unit> Bind(DocumentSession session, ConduitAdapter adapter, ConduitBinding binding) => binding.Switch(
        (Session: session, Adapter: adapter),
        global: static (_, _) => Fin.Succ(unit),
        viewport: static (ctx, row) => ViewportLease.Of(ctx.Session, row.Target)
            .Bind(lease => lease.Use(
                borrow => Try.lift(() => Fin.Succ((row.Use.Bind(ctx.Adapter, borrow.Viewport), unit).Item2)).Run().Bind(static inner => inner))));
}

public sealed record ConduitVetoAsk(DocumentSession Session, ConduitProgram Program);

public static class ConduitHooks {
    public static Fin<Seq<Lease<IDisposable>>> Mount(PluginKey plugin) {
        return MountRegistry.MountAll(
            bindings: Seq(
                    (Point: RhinoPoint.DisplayCull, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Cull)),
                    (Point: RhinoPoint.DisplayDrawObject, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Suppress)))
                .Map(row => (IHookBinding<RhinoPoint, PluginKey>)new HookBinding<RhinoPoint, PluginKey, ConduitVetoAsk, ConduitLease>(
                    Point: row.Point,
                    Owner: plugin,
                    Bind: ask => guard(ask.Program.Steps.Exists(row.Carries), new KernelFault.InvalidInput()).ToFin()
                        .Bind(_ => Conduits.Mount(session: ask.Session, program: ask.Program)))));
    }

    public static Fin<Unit> Release(PluginKey plugin) =>
        MountRegistry.Release(scope: plugin);
}
```

## [04]-[OVERLAYS]

- Owner: `AnalysisMode` is the implementation base for registered false-colour overlays; `AnalysisLaw` and `AnalysisScale` carry the one parameterized overlay policy and `AnalysisOverlay` is the mode that runs it; `MeshComponent` closes the two addressable mesh component rows; `RetainedOverlay` is the owned `CustomDisplay` capsule drawing through `Marks.Paint`.
- Entry: `AnalysisMode.Register<TMode>` and `Activate` close registration and object participation — participation is a `ModeParticipation` row, not a bare bool; `AnalysisOverlay.Bind` SEATS the law on the host-owned singleton through `Cell.Seat`, so the first binder wins, a second reads `Ceded` as a typed refusal, and no seat verdict is discarded; `RetainedOverlay.Apply` closes retained requests.
- Law: registered analysis, retained accumulation, and per-frame conduits keep distinct lifecycle owners.
- Law: false-colour overlays COMPOSE `Rasm.Analysis` and compute nothing — `AnalysisLaw` names an `AnalysisQuery` and the fold runs it through `Analyze.In(context:).Run(...)`, so a page-local curvature, defect, or quality measurement beside the kernel query rows is the deleted form. One law value spans the whole overlay space: a new analysis is a `Query` row, a new palette a `BlendPath` and two endpoint colours, and a new banding an `AnalysisScale` case — never a mode class per analysis.
- Law: the sample's component address dispatches through `MeshComponent` — the two addressable rows carry their own paint delegates and admission rides `FactoryBridge.Row` over the host ordinal, so the `_ =>` catch-all over `ComponentIndexType` is closed and an unrostered component refuses by name.
- Law: every overlay fault cell is the kernel `FaultCell` under `DisplayFaults.Cap` — a process-lifetime mode faulting per frame sheds into a bounded ring whose losses read as numbers; the Render pipeline's retention ledger is a deleted twin of this cell, not a composition target.
- Law: `Register` hands back a HOST-OWNED singleton — `VisualAnalysisMode.Register(Type)` constructs the instance itself, so the mode admits no constructor policy and the seated law arrives through `Bind`; an unbound mode refuses its callbacks with context evidence rather than painting a default nobody declared.
- Law: normalization states its own source — `AnalysisScale.Declared` fixes the band so two objects under one mode compare, and `Measured` autoscales to the observed span; a degenerate span resolves to the ramp's cold end, because a zero-width band admits no position and a fabricated midpoint reads as measured contrast.
- Law: `Add` is transactional — the capsule journals every retained mark, the batch draws through `Marks.Paint(Canvas.Retained)`, and a refusal row OR a host fault clears the native display and replays the pre-request journal, so the overlay never holds a half-applied batch; the mark count derives from the journal.
- Exemption: the capsule's `Lock` stays — a `CustomDisplay` write cannot ride a CAS body, and the journal, the native display, and the release flag must move together — the one statement-shaped custody on this page, stated here.
- Boundary: retained geometry never escapes the capsule; disposal composes the one `Custody` and re-arms only an incomplete release.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class ModeParticipation {
    public static readonly ModeParticipation Joined = new(key: true);
    public static readonly ModeParticipation Left = new(key: false);
}

[SmartEnum<int>]
public sealed partial class MeshComponent {
    public static readonly MeshComponent Face = new(
        key: (int)ComponentIndexType.MeshFace,
        paint: static (mesh, index, ink, op) => index >= 0 && index < mesh.Faces.Count
            ? Admit.Confirm(mesh.VertexColors.SetColor(mesh.Faces[index], ink))
            : Fin.Fail<Unit>(new KernelFault.InvalidInput(Axis: Some(nameof(index)))));
    public static readonly MeshComponent Ngon = new(
        key: (int)ComponentIndexType.MeshNgon,
        paint: static (mesh, index, ink, op) => index >= 0 && index < mesh.Ngons.Count
            ? toSeq(mesh.Ngons[index].BoundaryVertexIndexList())
                .TraverseM(at => Admit.Confirm(mesh.VertexColors.SetColor((int)at, ink))).As().Map(static _ => unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidInput(Axis: Some(nameof(index)))));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Paint(Mesh mesh, int index, System.Drawing.Color ink);
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record AnalysisProgram(
    Func<RhinoObject, DisplayPipelineAttributes, Fin<Unit>> Attributes,
    Func<RhinoObject, Mesh[], Fin<Unit>> Colors,
    Option<Func<RhinoObject, Mesh, DisplayPipeline, Fin<Unit>>> Draw);

internal abstract class AnalysisMode : VisualAnalysisMode {
    protected abstract AnalysisProgram Program { get; }

    protected override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        Try.lift(() => Program.Attributes(obj, attributes)).Run().Bind(static inner => inner).IfFail(OnFault);

    protected override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        Try.lift(() => Program.Colors(obj, meshes)).Run().Bind(static inner => inner).IfFail(OnFault);

    protected override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) =>
        Program.Draw.Iter(draw => Try.lift(() => draw(obj, mesh, pipeline)).Run().Bind(static inner => inner).IfFail(OnFault));

    internal static Fin<AnalysisMode> Register<TMode>() where TMode : AnalysisMode {
        return Try.lift(() => Optional(VisualAnalysisMode.Register(typeof(TMode)) as AnalysisMode).ToFin(new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
    }

    internal Fin<Unit> Activate(RhinoObject subject, ModeParticipation participation) {
        return from target in Optional(subject).ToFin(new KernelFault.InvalidInput())
               from _ in Try.lift(() => Admit.Confirm(ObjectSupportsAnalysisMode(target))).Run().Bind(static inner => inner)
               from activated in Try.lift(() => Admit.Confirm(target.EnableVisualAnalysisMode(this, participation.Key))).Run().Bind(static inner => inner)
               select unit;
    }

    protected abstract Unit OnFault(Error error);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalysisScale {
    private AnalysisScale() { }
    public sealed record Declared(double Low, double High) : AnalysisScale;
    public sealed record Measured : AnalysisScale;

    internal Fin<(double Low, double High)> Band(Seq<double> values) => Switch(
        values,
        declared: static (held, row) => double.IsFinite(row.Low) && double.IsFinite(row.High) && row.High >= row.Low
            ? Fin.Succ((row.Low, row.High))
            : Fin.Fail<(double, double)>(new KernelFault.InvalidInput()),
        measured: static (held, _) => held.IsEmpty
            ? Fin.Fail<(double, double)>(new KernelFault.InvalidResult())
            : Fin.Succ((held.Min(double.PositiveInfinity), held.Max(double.NegativeInfinity))));

    internal static UnitInterval Position(double value, (double Low, double High) band) =>
        UnitInterval.Create(value: band.High > band.Low
            ? Math.Clamp(value: (value - band.Low) / (band.High - band.Low), min: 0.0, max: 1.0)
            : 0.0);
}

[ComplexValueObject]
public sealed partial class AnalysisLaw {
    public AnalysisQuery Query { get; }
    public PerceptualColor Cold { get; }
    public PerceptualColor Hot { get; }
    public BlendPath Path { get; }
    public AnalysisScale Scale { get; }
}

internal sealed class AnalysisOverlay : AnalysisMode {
    private readonly Atom<Option<AnalysisLaw>> law = Atom(Option<AnalysisLaw>.None);
    private readonly FaultCell faults = DisplayFaults.Cell();
    private static readonly HookId HookPoint = HookId.Create(value: "rasm.rhino.display.analysis");

    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    internal Fin<Unit> Bind(AnalysisLaw value) {
        return Optional(value).ToFin(new KernelFault.InvalidInput()).Bind(admitted =>
            Cell.Seat(law, () => admitted).Switch(
                state: op,
                committed: static (_, _) => Fin.Succ(unit),
                ceded: static (o, _) => Fin.Fail<Unit>(new KernelFault.InvalidContext()),
                refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
                contended: static (o, _) => Fin.Fail<Unit>(new KernelFault.InvalidResult())));
    }

    protected override AnalysisProgram Program => new(
        Attributes: (_, attributes) => Try.lift(() => Fin.Succ((HostEdge.Side(() => attributes.ShadeVertexColors = true), unit).Item2)).Run().Bind(static inner => inner),
        Colors: (subject, meshes) => Held(subject).Bind(held =>
            toSeq(meshes ?? []).Filter(static mesh => mesh is not null).TraverseM(mesh => Paint(mesh, held.Law, held.Context)).As().Map(static _ => unit)),
        Draw: None);

    private Fin<(AnalysisLaw Law, Context Context)> Held(RhinoObject subject) =>
        from admitted in law.Value.ToFin(Fail: new KernelFault.MissingContext())
        from target in Admit.Need(subject)
        from context in Rasm.Domain.Context.Of(doc: target.Document).ToFin()
        select (admitted, context);

    private Fin<Unit> Paint(Mesh mesh, AnalysisLaw held, Context context) =>
        from samples in Analyze.In(context: context)
            .Run(operation: Analyze.Query<Mesh, MeshMetricSample>(query: held.Query), input: mesh)
            .ToFin()
        from band in held.Scale.Band(values: samples.Map(static row => row.Value).Strict())
        from cold in held.Cold.ToDrawing()
        from _sized in Try.lift(() => Fin.Succ((HostEdge.Side(() => {
            mesh.VertexColors.Clear();
            mesh.VertexColors.CreateMonotoneMesh(cold);
        }), unit).Item2)).Run().Bind(static inner => inner)
        from painted in samples.TraverseM(sample => Ink(mesh, sample, held, band)).As()
        select unit;

    private Fin<Unit> Ink(Mesh mesh, MeshMetricSample sample, AnalysisLaw held, (double Low, double High) band) =>
        from mixed in held.Cold.Mix(
            other: held.Hot, amount: AnalysisScale.Position(value: sample.Value, band: band), path: held.Path).ToFin()
        from ink in mixed.ToDrawing()
        from component in FactoryBridge.Row<ComponentIndexType, MeshComponent>(sample.Source.ComponentIndexType, ordinal: static type => (int)type)
        from painted in Try.lift(() => component.Paint(mesh: mesh, index: sample.Source.Index, ink: ink)).Run().Bind(static inner => inner)
        select painted;

    protected override Unit OnFault(Error error) => ignore(faults.Park(point: HookPoint, cause: error));
}

[SmartEnum<bool>]
public sealed partial class OverlayVisibility {
    public static readonly OverlayVisibility Shown = new(key: true);
    public static readonly OverlayVisibility Hidden = new(key: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetainedRequest {
    private RetainedRequest() { }
    public sealed record Add(Seq<WorldMark> Marks) : RetainedRequest;
    public sealed record Visibility(OverlayVisibility Value) : RetainedRequest;
    public sealed record Clear : RetainedRequest;
    public sealed record Inspect : RetainedRequest;

    internal bool Valid => Switch(
        add: static row => !row.Marks.IsEmpty && row.Marks.ForAll(static mark => mark is not null && mark.Valid),
        visibility: static row => row.Value is not null,
        clear: static _ => true,
        inspect: static _ => true);
}

public readonly record struct RetainedState(OverlayVisibility Visibility, Rasm.Numerics.Dimension Marks);

public sealed class RetainedOverlay : IDisposable {
    private readonly CustomDisplay display;
    private readonly Lock lifecycle = new();
    private readonly FaultCell faults = DisplayFaults.Cell();
    private static readonly HookId HookPoint = HookId.Create(value: "rasm.rhino.display.retained");
    private Seq<WorldMark> journal = Seq<WorldMark>();
    private bool released;

    private RetainedOverlay(CustomDisplay display) => (this.display, this.key) = (display);

    public static Fin<RetainedOverlay> Of(OverlayVisibility visibility) {
        return Try.lift(() => Fin.Succ(new RetainedOverlay(new CustomDisplay(visibility.Key)))).Run().Bind(static inner => inner);
    }

    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public Fin<RetainedState> Apply(RetainedRequest request) {
        lock (lifecycle) {
            return guard(!released, new KernelFault.InvalidContext()).ToFin()
                .Bind(_ => guard(request is not null && request.Valid, new KernelFault.InvalidInput()).ToFin())
                .Bind(_ => request.Switch(
                    this,
                    add: static (ctx, row) => {
                        Seq<WorldMark> prior = ctx.journal;
                        return Marks.Paint(
                                new Canvas.Retained(ctx.display),
                                row.Marks.Map(static mark => (DisplayMark)new DisplayMark.World(mark)))
                            .Bind(tally => tally.IsValid
                                ? Fin.Succ((ctx.journal = prior + row.Marks,
                                    new RetainedState(
                                        ctx.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                                        Rasm.Numerics.Dimension.Create(value: ctx.journal.Count))).Item2)
                                : Fin.Fail<RetainedState>(Error.Many(tally.Refused)))
                            .Rollback(
                                release: () => ctx.Restore(prior));
                    },
                    visibility: static (ctx, row) => Try.lift(() => Fin.Succ((
                        ctx.display.Enabled = row.Value.Key,
                        new RetainedState(row.Value, Rasm.Numerics.Dimension.Create(value: ctx.journal.Count))).Item2)).Run().Bind(static inner => inner),
                    clear: static (ctx, _) => Try.lift(() => Fin.Succ((
                        HostEdge.Side(ctx.display.Clear),
                        ctx.journal = Seq<WorldMark>(),
                        new RetainedState(
                            ctx.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                            Rasm.Numerics.Dimension.Create(value: 0))).Item3)).Run().Bind(static inner => inner),
                    inspect: static (ctx, _) => Fin.Succ(new RetainedState(
                        ctx.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                        Rasm.Numerics.Dimension.Create(value: ctx.journal.Count)))));
        }
    }

    private Fin<Unit> Restore(Seq<WorldMark> prior) => Try.lift(() => {
        _ = HostEdge.Side(display.Clear);
        journal = Seq<WorldMark>();
        return Marks.Paint(
                new Canvas.Retained(display),
                prior.Map(static mark => (DisplayMark)new DisplayMark.World(mark)))
            .Map(_ => (journal = prior, unit).Item2);
    }).Run().Bind(static inner => inner);

    public void Dispose() {
        lock (lifecycle) {
            if (released) { return; }
            released = true;
            _ = Custody.Release(
                    releases: Seq<Func<Fin<Unit>>>(
                        () => Try.lift(() => { display.Clear(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
                        () => Try.lift(() => { display.Dispose(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)))
                .IfFail(cause => {
                    released = false;
                    _ = faults.Park(point: HookPoint, cause: cause);
                });
        }
    }
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-display.md` — `DisplayConduit` phase overrides, `DrawEventArgs`, channel constants); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` phase/lane rows, the `PhaseHost` `[Union]`); `Riok.Mapperly` (`libs/dotnet/.api/api-mapperly.md` — the conduit-state `[Mapper]`); kernel `Domain/results` + `Domain/validation` (`PhaseCapability` rows over `CapabilityLaw`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

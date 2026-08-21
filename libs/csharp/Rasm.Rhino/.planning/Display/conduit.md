# [RASM_RHINO_DISPLAY_CONDUIT]

`Conduits.Mount` owns filtered display-pipeline participation as a leased phase program with balanced render state and a bounded, observable callback-fault cell. Retained overlays and registered analysis remain distinct lifetime shapes under the same host boundary, and both draw through the ONE `Marks.Paint` dispatch — a private draw path beside it is the deleted form.

`ConduitFrame` is the draw seam `Marks.Paint`'s pipeline canvas consumes. Viewport identity and pass facts detach immediately through the one generated projection, while the raw `DisplayPipeline` remains scoped to the host callback that supplied it.

## [01]-[INDEX]

- [02]-[PROGRAM]: `PhaseCapability`, `PhaseHost`, `ConduitPhase`, `RenderAspect`, `SwitchState`, `RenderSwitch`, `CullUse`, `ConduitCriterion`, `BoundsRole`, `DrawProjector`, `ConduitStep`, `ConduitProgram`, `FramePosture`, `FrameContext`, `ConduitFrame`, `Cases` — phase, filter, state policy, and the once-partitioned step lanes.
- [03]-[MOUNT]: `DisplayFaults`, `ConduitAdapter`, `PipelineScope`, `ConduitLease`, `Conduits`, `ConduitVetoAsk`, `ConduitHooks` — binding, bounded callback faults, disablement, and unbinding.
- [04]-[OVERLAYS]: `AnalysisMode`, `ModeParticipation`, `AnalysisScale`, `AnalysisLaw`, `MeshComponent`, `AnalysisOverlay`, `OverlayVisibility`, `RetainedRequest`, `RetainedReceipt`, `RetainedOverlay` — registered false-colour analysis and the retained `CustomDisplay` capsule.

## [02]-[PROGRAM]

- Owner: `ConduitPhase` is the package-wide draw-seam vocabulary — each row carries ONE `CapabilitySet<PhaseCapability>` and the `PhaseHost` case naming who hands out the live pipeline; `ConduitStep` closes the phase program at four cases with `DrawProjector` folding the two draw arities into one case; `ConduitProgram` admits the program and partitions its steps into typed lanes ONCE.
- Cases: `PhaseCapability` is `Draws`, `PerObject`, and `WorldSpace` — the 44 four-bool row literals collapse onto eleven set literals; `PhaseHost` is `Conduit` (a `DisplayConduit` override phase — the only host a program MOUNTS), `Engine` (a realtime framebuffer or middleground event), or `Widget` (a registered widget's `OnDraw`) — the `Mounts` bool was this union flattened, and a producer stamping a phase states which host handed it the pipeline rather than a flag consumers re-derive.
- Law: the legal capability corners DERIVE from the roster — `ConduitPhase.Law` is the distinct set of row sets behind an accessor-backed lazy, so a twelfth phase lands as one row and the law re-materializes; a hand-kept corner list beside the rows is two models of one fact.
- Law: veto is host truth — `Cull` can only widen the incoming `CullObjectEventArgs.CullObject` and `Suppress` can only narrow the incoming `DrawObjectEventArgs.DrawObject`, the only two suppression flags the display contract admits; a prior host veto remains set, each decide answers per object per frame, and any deciding step voting to suppress wins.
- Law: the two veto steps answer DISTINCT verdict owners — `CullVerdict` spells visible-versus-culled and `SuppressVerdict` drawn-versus-suppressed — because their predicates are structurally identical over inverted senses, so a swapped delegate is a compile error rather than a silently mirrored frame.
- Law: a drawing step's ARITY is its projector's case — `DrawProjector.PerObject` demands a phase whose set admits `PerObject` and `DrawProjector.Frame` one that does not — so the old sibling cases `Draw`/`ObjectDraw` collapse and a mismatched pairing refuses at admission; the bounds obligation is `BoundsRole`, derived from the phase's `WorldSpace` capability, and the `(bool Supplies, bool Requires)` tuple deletes.
- Law: steps PARTITION at `ConduitProgram.Of` into typed lanes — the cull lane, the suppress lane, and the per-phase bounds and draw maps — so the host callbacks read a frozen lane instead of re-running `Choose` and `Filter` over the whole step roster per object per frame.
- Law: `ConduitCriterion` turns every host filter axis into one case-unique row inside the mount request; case runtime type is the uniqueness key, and `Cases.Unique` is the ONE shared admission fold — criteria here and the mode policies and appearance concerns on `Display/modes.md` all admit through it.
- Law: `FrameContext` detaches through the generated `FrameMap` — the six per-frame host reads are one `[Mapper]` projection, its three posture bools ride ONE `CapabilitySet<FramePosture>`, and its density is an admitted `PositiveMagnitude`.
- Boundary: callback failures park on the lease's bounded fault cell; a host callback never discards a failed rail.
- Growth: a pipeline phase is one row; a render state one `RenderAspect` case and one total adapter arm; a filter axis one criterion case.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
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

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhaseCapability : ICapability<PhaseCapability> {
    public static readonly PhaseCapability Draws = new(key: "draws");
    public static readonly PhaseCapability PerObject = new(key: "per-object");
    public static readonly PhaseCapability WorldSpace = new(key: "world-space");
}

// WHO hands out the live pipeline: only a `Conduit` phase is a `DisplayConduit` override a program can MOUNT — a
// realtime engine event and a registered widget's draw each hand a pipeline outside the conduit phase order, so
// they carry honest cases and refuse mounting rather than borrowing a conduit phase's name.
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

    // The legal corners DERIVE from the roster behind an accessor-backed lazy — the generated `Items` fills from
    // its own static constructor, so an eager field would freeze an empty law.
    public static CapabilityLaw<PhaseCapability> Law => Corners.Value;
    private static readonly Lazy<CapabilityLaw<PhaseCapability>> Corners =
        new(static () => new CapabilityLaw<PhaseCapability>(toSeq(Items).Map(static row => row.Capabilities).Distinct()));
}

// The row CARRIES the host consequence, so `Toggle` names its state instead of a bare bool a swapped argument inverts.
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

    internal Fin<Unit> With(DisplayPipeline pipeline, Func<Fin<Unit>> draw, Op key) {
        bool acquired = false;
        Fin<Unit> primary = key.Catch(() => {
            Switch(
                pipeline,
                toggle: static (p, row) => row.Target.Push(p, row.State.Key),
                cull: static (p, row) => Op.Side(() => p.PushCullFaceMode(row.Mode.Native)),
                model: static (p, row) => Op.Side(() => p.PushModelTransform(row.Transform)),
                screen: static (p, _) => Op.Side(p.Push2dProjection));
            acquired = true;
            return draw();
        });
        Fin<Unit> cleanup = acquired
            ? key.Catch(() => Fin.Succ(Switch(
                pipeline,
                toggle: static (p, row) => row.Target.Pop(p),
                cull: static (p, _) => Op.Side(p.PopCullFaceMode),
                model: static (p, _) => Op.Side(p.PopModelTransform),
                screen: static (p, _) => Op.Side(p.PopProjection))))
            : Fin.Succ(unit);
        // Cleanup faults AGGREGATE into the primary — the ruled disposal posture — never `ignore`d.
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
        push: static (pipeline, enabled) => Op.Side(() => pipeline.PushDepthTesting(enabled)),
        pop: static pipeline => Op.Side(pipeline.PopDepthTesting));
    public static readonly RenderSwitch DepthWrite = new(
        key: 1,
        push: static (pipeline, enabled) => Op.Side(() => pipeline.PushDepthWriting(enabled)),
        pop: static pipeline => Op.Side(pipeline.PopDepthWriting));
    public static readonly RenderSwitch ClipTest = new(
        key: 2,
        push: static (pipeline, enabled) => Op.Side(() => pipeline.PushClipTesting(enabled)),
        pop: static pipeline => Op.Side(pipeline.PopClipTesting));

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
        selection: static (c, row) => Op.Side(() => c.SetSelectionFilter(row.Use.Enabled, row.Use.SubObjects)),
        objects: static (c, row) => Op.Side(() => c.SetObjectIdFilter(row.Ids.Distinct().AsEnumerable())),
        geometry: static (c, row) => Op.Side(() => c.GeometryFilter = row.Kinds.Mask),
        space: static (c, row) => Op.Side(() => c.SpaceFilter = row.Value.Key));
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
        bind: static (conduit, viewport) => Op.Side(() => conduit.Bind(viewport)));
    public static readonly BindUse Exclusive = new(
        key: 1,
        bind: static (conduit, viewport) => Op.Side(() => conduit.ExclusiveBind(viewport)));

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

// The two veto steps carry structurally identical predicates over INVERTED senses, so each owns its own verdict: a
// `Cull` decide answers whether the object survives the cull walk and a `Suppress` decide whether its draw runs. One
// shared `bool` would let the two delegates trade places and type-check, painting the exact frame neither asked for.
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

// The bounds obligation as ROWS — the `(bool Supplies, bool Requires)` tuple whose `(true, true)` corner nothing
// occupied is the deleted form.
[SmartEnum<int>]
public sealed partial class BoundsRole {
    public static readonly BoundsRole None = new(key: 0);
    public static readonly BoundsRole Supplies = new(key: 1);
    public static readonly BoundsRole Requires = new(key: 2);
}

// The three measured per-frame host postures as one capability set — three parallel bools on a snapshot record
// were the flat form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FramePosture : ICapability<FramePosture> {
    public static readonly FramePosture Capturing = new(key: "capturing");
    public static readonly FramePosture Printing = new(key: "printing");
    public static readonly FramePosture Dynamic = new(key: "dynamic");
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct FrameContext(
    CapabilitySet<FramePosture> Postures,
    int RenderPass,
    int NestLevel,
    PositiveMagnitude DpiScale);

// The six per-frame host reads as ONE generated projection: the posture fold and the density admission are the two
// declared user mappings, and every other column is the generator's.
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

// The two draw arities as CASES of one projector, so `Draw` is one step case and a projector arity that disagrees
// with its phase's `PerObject` capability refuses at admission instead of minting a sibling step.
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

    // The obligation is the phase's own `WorldSpace` capability, so a screen-space step demands no contribution.
    internal BoundsRole Bounds_ => Switch(
        cull: static _ => BoundsRole.None,
        suppress: static _ => BoundsRole.None,
        bounds: static row => row.Phase == ConduitPhase.Bounds ? BoundsRole.Supplies : BoundsRole.None,
        draw: static row => row.Phase.Capabilities.Admits(PhaseCapability.WorldSpace) ? BoundsRole.Requires : BoundsRole.None);
}

// Steps PARTITION here, once: the host callbacks read frozen lanes, never a per-frame `Choose` over the roster.
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

    // Independent admissions ACCUMULATE, so a bad step, a duplicate criterion, and a broken bounds order report together.
    public static Fin<ConduitProgram> Of(
        Seq<ConduitStep> steps,
        ConduitBinding binding,
        Seq<ConduitCriterion> criteria,
        Op? key = null) {
        Op op = key.OrDefault();
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
                    : Validation<Error, Seq<ConduitStep>>.Fail(op.InvalidInput(axis: nameof(steps)))),
                (criteria.ForAll(static criterion => criterion is not null && criterion.Valid) && Cases.Unique(criteria)
                    ? Validation<Error, Seq<ConduitCriterion>>.Success(criteria)
                    : Validation<Error, Seq<ConduitCriterion>>.Fail(op.InvalidInput(axis: nameof(criteria)))),
                (binding is { Valid: true }
                    ? Validation<Error, ConduitBinding>.Success(binding)
                    : Validation<Error, ConduitBinding>.Fail(op.InvalidInput(axis: nameof(binding)))),
                (ordered
                    ? Validation<Error, Unit>.Success(unit)
                    : Validation<Error, Unit>.Fail(op.InvalidInput(axis: nameof(BoundsRole)))))
            .Apply(static (admitted, rows, bound, _) => (Steps: admitted, Criteria: rows, Binding: bound))
            .As().ToFin()
            .Map(held => new ConduitProgram(
                held.Steps,
                held.Binding,
                held.Criteria,
                culls: held.Steps.Choose(static step => step is ConduitStep.Cull row ? Some(row) : None),
                suppresses: held.Steps.Choose(static step => step is ConduitStep.Suppress row ? Some(row) : None),
                bounds: held.Steps.Choose(static step => step is ConduitStep.Bounds row ? Some(row) : None)
                    .GroupBy(static row => row.Phase).Fold(
                        HashMap<ConduitPhase, Seq<ConduitStep.Bounds>>(),
                        static (map, group) => map.Add(group.Key, toSeq(group))),
                draws: held.Steps.Choose(static step => step is ConduitStep.Draw row ? Some(row) : None)
                    .GroupBy(static row => row.Phase).Fold(
                        HashMap<ConduitPhase, Seq<ConduitStep.Draw>>(),
                        static (map, group) => map.Add(group.Key, toSeq(group)))));
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// Case uniqueness is the one admission fold every display program shares: a `[Union]` case's runtime type IS its
// discriminant, so a duplicate row is a later write silently overwriting an earlier one. Criteria here and the mode
// policies and appearance concerns on `Display/modes.md` all admit through this member.
internal static class Cases {
    internal static bool Unique<T>(Seq<T> rows) where T : class =>
        rows.Map(static row => row.GetType()).Distinct().Count == rows.Count;
}
```

## [03]-[MOUNT]

- Owner: `ConduitLease` owns the adapter and its bounded callback-fault cell until deterministic release; `DisplayFaults` declares the ONE cap every long-lived display owner's cell mints under; `ConduitHooks` registers the two display veto points as TYPED hook bindings.
- Entry: `ConduitProgram.Of` admits and partitions; `Conduits.Mount` applies the admitted program and arms participation; `ConduitHooks.Mount` seats `rasm.rhino.display.cull` and `rasm.rhino.display.drawobject` on the `MountRegistry` — the ask is a typed `ConduitVetoAsk` whose program must carry the point's own veto step, the grant is the mounted `ConduitLease`, and a program missing the veto step refuses typed before any host participation arms.
- Law: the fault cell is the kernel `FaultCell` — a bounded ring whose parks, sheds, and declined parks all read as numbers — never an unbounded `Atom<Seq<Error>>`; the cap is DECLARED at `DisplayFaults.Cap`, because every one of these owners is host-activated or process-static and no policy reaches a constructor, and a per-owner cap beside the declared one is the fork.
- Law: every parked fault also publishes through `ObjectsTelemetry` under `FaultSite.Conduit` — the cell is the lease's readable receipt, the publish the process egress, and a second logger sink beside them is the fork.
- Law: a draw lane's REFUSAL rows park too — `Marks.Paint` accounts a capability-illegal mark as a typed refusal on its receipt, and the adapter parks each cause, so a projector emitting a mark its canvas cannot draw is observable rather than silent.
- Law: release composes kernel `Custody.Release` — disable, `UnbindAll`, sprite disposal, every step running even when an earlier one refuses, failures aggregating through `Error.Many` — and the lease's one-shot is a stepped transition whose failed release re-arms while its verdict parks on the cell.
- Boundary: the adapter is the only `DisplayConduit` subclass and the only statement-shaped host callback seam.

```csharp signature
// --- [CONSTANTS] ----------------------------------------------------------------------------
// One declared cap for every long-lived display fault cell — host-activated owners admit no injected policy, and a
// per-owner cap beside this one is the fork. The clock feeds the cell's fault stamps alone.
internal static class DisplayFaults {
    internal static readonly Dimension Cap = Dimension.Create(value: 256);
    internal static FaultCell Cell() => new(cap: Cap, clock: TimeProvider.System);
}

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class ConduitAdapter : DisplayConduit {
    private readonly ConduitProgram program;
    private readonly FaultCell faults;
    private readonly SpriteSheet sprites = new();
    private readonly Op key;
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.conduit");

    internal ConduitAdapter(ConduitProgram program, FaultCell faults, Op key) =>
        (this.program, this.faults, this.key) = (program, faults, key);

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
                .Bind(box => guard(box.IsValid, key.InvalidResult()).ToFin()
                    .Map(_ => Op.Side(() => e.IncludeBoundingBox(box))))).As()
            .Map(static _ => unit);

    private Fin<Unit> Draw(DrawEventArgs e, ConduitPhase phase) =>
        program.DrawLanes.Find(phase).IfNone(Seq<ConduitStep.Draw>())
            .TraverseM(step => {
                ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, phase);
                return step.Projector switch {
                    DrawProjector.Frame projector => Render(frame, step.State, projector.Project(frame)),
                    DrawProjector.PerObject => Fin.Succ(unit),
                    _ => Fin.Fail<Unit>(key.InvalidInput()),
                };
            }).As()
            .Map(static _ => unit);

    // The receipt's refusal rows PARK: a projector emitting a mark the pipeline cannot draw is a number and a
    // cause on the cell, never a silent skip.
    private Fin<Unit> Render(ConduitFrame frame, Seq<RenderAspect> state, Fin<Seq<DisplayMark>> projected) =>
        PipelineScope.With(frame.Pipeline, state, () => projected
            .Bind(marks => Marks.Paint(new Canvas.Pipeline(frame, sprites), marks, key))
            .Map(receipt => receipt.Refused.Fold(unit, (_, cause) => ignore(faults.Park(point: Rail, cause: cause)))), key);

    private Fin<Unit> Project(DrawObjectEventArgs e, ConduitStep.Draw step) {
        ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, step.Phase);
        return step.Projector switch {
            DrawProjector.PerObject projector => Render(frame, step.State, projector.Project(e.RhinoObject.Id, frame)),
            _ => Fin.Succ(unit),
        };
    }

    private void Invoke(Func<Fin<Unit>> callback) => Observe(key.Catch(callback));

    private void Observe<T>(Fin<T> outcome) =>
        outcome.IfFail(error => ignore((
            faults.Park(point: Rail, cause: error),
            ObjectsTelemetry.Publish(site: FaultSite.Conduit, error: error))));

    // The ONE ordered teardown fold — every step runs, failures aggregate — composed from `Document/lifetime.md`.
    internal Fin<Unit> Release() => Custody.Release(
        releases: Seq<Func<Fin<Unit>>>(
            () => key.Catch(() => { Enabled = false; return Fin.Succ(value: unit); }),
            () => key.Catch(() => { UnbindAll(); return Fin.Succ(value: unit); }),
            () => sprites.Release(key)),
        key: key);
}

// Generic over the drawn result so a bracketed body answers its own shape — the receipt-bearing paint dispatch and
// the unit-shaped adapter draws share one bracket.
internal static class PipelineScope {
    internal static Fin<TResult> With<TResult>(DisplayPipeline pipeline, Seq<RenderAspect> state, Func<Fin<TResult>> draw, Op key) {
        Fin<TResult> slot = Fin.Fail<TResult>(key.InvalidResult());
        Fin<Unit> crossed = toSeq(state.AsEnumerable().Reverse())
            .Fold<Func<Fin<Unit>>>(
                () => (slot = draw()).Map(static _ => unit),
                (next, aspect) => () => aspect.With(pipeline, next, key))();
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
    private readonly Op key;

    internal ConduitLease(ConduitAdapter adapter, FaultCell faults, Op key) =>
        (this.adapter, this.faults, this.key) = (adapter, faults, key);

    // BOUNDED evidence: the ring's parked rows and its shed count, never an unbounded sequence.
    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    // One-shot as a stepped TRANSITION: a second dispose reads a declined step, and a failed release re-arms by
    // stepping back to `Live` — the interlocked-int-and-`Volatile.Write` pair is the deleted form.
    public void Dispose() {
        Transition<LeaseGate> claimed = Cell.Step(
            gate,
            static held => held is LeaseGate.Live ? Some<LeaseGate>(new LeaseGate.Released()) : None,
            key.InvalidContext());
        _ = Op.SideWhen(claimed is Transition<LeaseGate>.Committed, () => adapter.Release().IfFail(cause => {
            _ = Cell.Step(gate, static held => held is LeaseGate.Released ? Some<LeaseGate>(new LeaseGate.Live()) : None, Errors.None);
            _ = faults.Park(point: HookId.Create(value: "rasm.rhino.display.conduit"), cause: cause);
        }));
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Conduits {
    public static Fin<ConduitLease> Mount(
        DocumentSession session,
        ConduitProgram program,
        Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(op.MissingContext())
               from admitted in Optional(program).ToFin(op.InvalidInput())
               from faults in Fin.Succ(DisplayFaults.Cell())
               from adapter in Fin.Succ(new ConduitAdapter(admitted, faults, op))
               from lease in (from __ in op.Catch(() => Fin.Succ(admitted.Criteria
                                  .Fold(unit, static (_, criterion) => criterion.Apply(adapter))))
                              from ___ in Bind(owner, adapter, admitted.Binding, op)
                              from ____ in op.Catch(() => Fin.Succ((adapter.Enabled = true, unit).Item2))
                              select new ConduitLease(adapter, faults, op)).BiBind(
                                  Succ: static value => Fin.Succ(value),
                                  Fail: error => adapter.Release().Match(
                                      Succ: _ => Fin.Fail<ConduitLease>(error),
                                      Fail: cleanup => Fin.Fail<ConduitLease>(error + cleanup)))
               select lease;
    }

    private static Fin<Unit> Bind(DocumentSession session, ConduitAdapter adapter, ConduitBinding binding, Op key) => binding.Switch(
        (Session: session, Adapter: adapter, Op: key),
        global: static (_, _) => Fin.Succ(unit),
        viewport: static (ctx, row) => ViewportLease.Of(ctx.Session, row.Target, ctx.Op)
            .Bind(lease => lease.Use(
                borrow => ctx.Op.Catch(() => Fin.Succ((row.Use.Bind(ctx.Adapter, borrow.Viewport), unit).Item2)),
                ctx.Op)));
}

public sealed record ConduitVetoAsk(DocumentSession Session, ConduitProgram Program);

// TYPED end to end: the binding names its ask and grant as type parameters, so the `Type`-pair-and-`Func<object,
// Fin<object>>` erasure and its casts are the deleted form; `MountRegistry` composes the kernel `HookMounts` beneath
// its name-addressed carve, and `Release` drops every mount the plugin's scope seated.
public static class ConduitHooks {
    public static Fin<Seq<Lease<IDisposable>>> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.MountAll(
            bindings: Seq(
                    (Point: RhinoPoint.DisplayCull, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Cull)),
                    (Point: RhinoPoint.DisplayDrawObject, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Suppress)))
                .Map(row => (IHookBinding<RhinoPoint, PluginKey>)new HookBinding<RhinoPoint, PluginKey, ConduitVetoAsk, ConduitLease>(
                    Point: row.Point,
                    Owner: plugin,
                    Bind: ask => guard(ask.Program.Steps.Exists(row.Carries), op.InvalidInput()).ToFin()
                        .Bind(_ => Conduits.Mount(session: ask.Session, program: ask.Program, key: op)))),
            key: op);
    }

    public static Fin<Unit> Release(PluginKey plugin, Op? key = null) =>
        MountRegistry.Release(scope: plugin, key: key.OrDefault());
}
```

## [04]-[OVERLAYS]

- Owner: `AnalysisMode` is the implement seam for registered false-colour overlays; `AnalysisLaw` and `AnalysisScale` carry the one parameterized overlay policy and `AnalysisOverlay` is the mode that runs it; `MeshComponent` closes the two addressable mesh component rows; `RetainedOverlay` is the owned `CustomDisplay` capsule drawing through `Marks.Paint`.
- Entry: `AnalysisMode.Register<TMode>` and `Activate` close registration and object participation — participation is a `ModeParticipation` row, not a bare bool; `AnalysisOverlay.Bind` SEATS the law on the host-owned singleton through `Cell.Seat`, so the first binder wins, a second reads `Ceded` as a typed refusal, and no seat verdict is discarded; `RetainedOverlay.Apply` closes retained requests.
- Law: registered analysis, retained accumulation, and per-frame conduits keep distinct lifecycle owners.
- Law: false-colour overlays COMPOSE `Rasm.Analysis` and compute nothing — `AnalysisLaw` names an `AnalysisQuery` and the fold runs it through `Analyze.In(context:).Run(...)`, so a page-local curvature, defect, or quality measurement beside the kernel query rows is the deleted form. One law value spans the whole overlay space: a new analysis is a `Query` row, a new palette a `BlendPath` and two endpoint colours, and a new banding an `AnalysisScale` case — never a mode class per analysis.
- Law: the sample's component address dispatches through `MeshComponent` — the two addressable rows carry their own paint delegates and admission rides `Op.Row` over the host ordinal, so the `_ =>` catch-all over `ComponentIndexType` is closed and an unrostered component refuses by name.
- Law: every overlay fault cell is the kernel `FaultCell` under `DisplayFaults.Cap` — a process-lifetime mode faulting per frame sheds into a bounded ring whose losses read as numbers; the Render rail's retention ledger is a deleted twin of this cell, not a composition target.
- Law: `Register` hands back a HOST-OWNED singleton — `VisualAnalysisMode.Register(Type)` constructs the instance itself, so the mode admits no constructor policy and the seated law arrives through `Bind`; an unbound mode refuses its callbacks with context evidence rather than painting a default nobody declared.
- Law: normalization states its own source — `AnalysisScale.Declared` fixes the band so two objects under one mode compare, and `Measured` autoscales to the observed span; a degenerate span resolves to the ramp's cold end, because a zero-width band admits no position and a fabricated midpoint reads as measured contrast.
- Law: `Add` is transactional — the capsule journals every retained mark, the batch draws through `Marks.Paint(Canvas.Retained)`, and a refusal row OR a host fault clears the native display and replays the pre-request journal, so the overlay never holds a half-applied batch; the mark count derives from the journal.
- Exemption: the capsule's `Lock` stays — a `CustomDisplay` write cannot ride a CAS body, and the journal, the native display, and the release flag must move together — the one statement-shaped custody on this page, stated here.
- Boundary: retained geometry never escapes the capsule; disposal composes the one `Custody` and re-arms only an incomplete release.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Participation as rows: `Joined` and `Left` carry the host bool, so a swapped literal cannot silently invert an
// activation and the call site names its intent.
[SmartEnum<bool>]
public sealed partial class ModeParticipation {
    public static readonly ModeParticipation Joined = new(key: true);
    public static readonly ModeParticipation Left = new(key: false);
}

// The two component vocabularies a mesh sample can address, each carrying its own paint — the `_ =>` catch-all
// over the host enum is closed, and admission rides the host ordinal through `Op.Row`.
[SmartEnum<int>]
public sealed partial class MeshComponent {
    public static readonly MeshComponent Face = new(
        key: (int)ComponentIndexType.MeshFace,
        paint: static (mesh, index, ink, op) => index >= 0 && index < mesh.Faces.Count
            ? op.Confirm(mesh.VertexColors.SetColor(mesh.Faces[index], ink))
            : Fin.Fail<Unit>(op.InvalidInput(axis: nameof(index))));
    public static readonly MeshComponent Ngon = new(
        key: (int)ComponentIndexType.MeshNgon,
        paint: static (mesh, index, ink, op) => index >= 0 && index < mesh.Ngons.Count
            ? toSeq(mesh.Ngons[index].BoundaryVertexIndexList())
                .TraverseM(at => op.Confirm(mesh.VertexColors.SetColor((int)at, ink))).As().Map(static _ => unit)
            : Fin.Fail<Unit>(op.InvalidInput(axis: nameof(index))));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Paint(Mesh mesh, int index, System.Drawing.Color ink, Op op);
}

// --- [MODELS] -------------------------------------------------------------------------------
internal sealed record AnalysisProgram(
    Func<RhinoObject, DisplayPipelineAttributes, Fin<Unit>> Attributes,
    Func<RhinoObject, Mesh[], Fin<Unit>> Colors,
    Option<Func<RhinoObject, Mesh, DisplayPipeline, Fin<Unit>>> Draw);

internal abstract class AnalysisMode : VisualAnalysisMode {
    private readonly Op key = Op.Of(nameof(AnalysisMode));
    protected abstract AnalysisProgram Program { get; }

    protected override void SetUpDisplayAttributes(RhinoObject obj, DisplayPipelineAttributes attributes) =>
        key.Catch(() => Program.Attributes(obj, attributes)).IfFail(OnFault);

    protected override void UpdateVertexColors(RhinoObject obj, Mesh[] meshes) =>
        key.Catch(() => Program.Colors(obj, meshes)).IfFail(OnFault);

    protected override void DrawMesh(RhinoObject obj, Mesh mesh, DisplayPipeline pipeline) =>
        Program.Draw.Iter(draw => key.Catch(() => draw(obj, mesh, pipeline)).IfFail(OnFault));

    internal static Fin<AnalysisMode> Register<TMode>(Op? key = null) where TMode : AnalysisMode {
        Op op = key.OrDefault();
        return op.Catch(() => Optional(VisualAnalysisMode.Register(typeof(TMode)) as AnalysisMode).ToFin(op.InvalidResult()));
    }

    internal Fin<Unit> Activate(RhinoObject subject, ModeParticipation participation, Op? key = null) {
        Op op = key.OrDefault();
        return from target in Optional(subject).ToFin(op.InvalidInput())
               from _ in op.Catch(() => op.Confirm(ObjectSupportsAnalysisMode(target)))
               from activated in op.Catch(() => op.Confirm(target.EnableVisualAnalysisMode(this, participation.Key)))
               select unit;
    }

    protected abstract Unit OnFault(Error error);
}

// Normalization sources are a CASE, never a nullable pair: Declared fixes the band so two objects under one mode
// are comparable, Measured takes the observed span so one object reads its own contrast, and neither is derivable
// from the other. Position folds through the SAME member for both, so the degenerate-span rule is stated once.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalysisScale {
    private AnalysisScale() { }
    public sealed record Declared(double Low, double High) : AnalysisScale;
    public sealed record Measured : AnalysisScale;

    internal Fin<(double Low, double High)> Band(Seq<double> values, Op key) => Switch(
        (Values: values, Op: key),
        declared: static (held, row) => double.IsFinite(row.Low) && double.IsFinite(row.High) && row.High >= row.Low
            ? Fin.Succ((row.Low, row.High))
            : Fin.Fail<(double, double)>(held.Op.InvalidInput()),
        measured: static (held, _) => held.Values.IsEmpty
            ? Fin.Fail<(double, double)>(held.Op.InvalidResult())
            : Fin.Succ((held.Values.Min(double.PositiveInfinity), held.Values.Max(double.NegativeInfinity))));

    // Zero-width bands admit no position, so the value sits at the cold end: interpolating a fabricated midpoint
    // there would render uniform mid-ramp colour as if a measurement had spread, which is the reading it destroys.
    internal static UnitInterval Position(double value, (double Low, double High) band) =>
        UnitInterval.Create(value: band.High > band.Low
            ? Math.Clamp(value: (value - band.Low) / (band.High - band.Low), min: 0.0, max: 1.0)
            : 0.0);
}

// One law value IS the whole overlay space — the analysis, the two ramp ends, the interpolation row, and the
// normalization source. A second mode class per measurement is what this collapses; the kernel owns every number.
[ComplexValueObject]
public sealed partial class AnalysisLaw {
    public AnalysisQuery Query { get; }
    public PerceptualColor Cold { get; }
    public PerceptualColor Hot { get; }
    public BlendPath Path { get; }
    public AnalysisScale Scale { get; }
}

// ONE registered false-colour mode, and Rhino constructs and owns it — the law arrives through `Bind`'s seat and
// an unbound mode refuses rather than painting an undeclared default.
internal sealed class AnalysisOverlay : AnalysisMode {
    private readonly Atom<Option<AnalysisLaw>> law = Atom(Option<AnalysisLaw>.None);
    private readonly FaultCell faults = DisplayFaults.Cell();
    private readonly Op key = Op.Of(nameof(AnalysisOverlay));
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.analysis");

    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    // First binder WINS and the verdict is read, never discarded: a `Ceded` seat is a typed refusal naming the
    // process-lifetime singleton already bound, which is exactly what the swallowed `Swap` verdict hid.
    internal Fin<Unit> Bind(AnalysisLaw value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).ToFin(op.InvalidInput()).Bind(admitted =>
            Cell.Seat(law, () => admitted).Switch(
                state: op,
                committed: static (_, _) => Fin.Succ(unit),
                ceded: static (o, _) => Fin.Fail<Unit>(o.InvalidContext()),
                refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
                contended: static (o, _) => Fin.Fail<Unit>(o.InvalidResult())));
    }

    protected override AnalysisProgram Program => new(
        // Vertex colours are the channel this mode writes, so the attribute pass arms exactly that and touches no
        // other display axis — a mode that also forced shading or object colour would fight the display mode it runs under.
        Attributes: (_, attributes) => key.Catch(() => Fin.Succ((Op.Side(() => attributes.ShadeVertexColors = true), unit).Item2)),
        Colors: (subject, meshes) => Held(subject).Bind(held =>
            toSeq(meshes ?? []).Filter(static mesh => mesh is not null).TraverseM(mesh => Paint(mesh, held.Law, held.Context)).As().Map(static _ => unit)),
        Draw: None);

    private Fin<(AnalysisLaw Law, Context Context)> Held(RhinoObject subject) =>
        from admitted in law.Value.ToFin(Fail: key.MissingContext())
        from target in key.Need(subject)
        from context in Rasm.Domain.Context.Of(doc: target.Document).ToFin()
        select (admitted, context);

    // Kernels measure and this fold paints: samples arrive addressed by `ComponentIndex`, the row's own paint
    // writes the corners or the boundary ring, and every ink rides the kernel egress rail.
    private Fin<Unit> Paint(Mesh mesh, AnalysisLaw held, Context context) =>
        from samples in Analyze.In(context: context)
            .Run(operation: Analyze.Query<Mesh, MeshMetricSample>(query: held.Query, key: key), input: mesh)
            .ToFin()
        from band in held.Scale.Band(values: samples.Map(static row => row.Value).Strict(), key: key)
        from cold in held.Cold.ToDrawing(key: key)
        from _sized in key.Catch(() => Fin.Succ((Op.Side(() => {
            mesh.VertexColors.Clear();
            mesh.VertexColors.CreateMonotoneMesh(cold);
        }), unit).Item2))
        from painted in samples.TraverseM(sample => Ink(mesh, sample, held, band)).As()
        select unit;

    private Fin<Unit> Ink(Mesh mesh, MeshMetricSample sample, AnalysisLaw held, (double Low, double High) band) =>
        from mixed in held.Cold.Mix(
            other: held.Hot, amount: AnalysisScale.Position(value: sample.Value, band: band), path: held.Path).ToFin()
        from ink in mixed.ToDrawing(key: key)
        from component in key.Row<ComponentIndexType, MeshComponent>(sample.Source.ComponentIndexType, ordinal: static type => (int)type)
        from painted in key.Catch(() => component.Paint(mesh: mesh, index: sample.Source.Index, ink: ink, op: key))
        select painted;

    protected override Unit OnFault(Error error) => ignore(faults.Park(point: Rail, cause: error));
}

// Visibility as rows carrying the host consequence — the request bool and the receipt bool delete together.
[SmartEnum<bool>]
public sealed partial class OverlayVisibility {
    public static readonly OverlayVisibility Shown = new(key: true);
    public static readonly OverlayVisibility Hidden = new(key: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetainedRequest {
    private RetainedRequest() { }
    // The retained payload IS the world band: the parallel eight-case retained vocabulary is deleted, and the
    // `CustomDisplay`-addressable subset is `Marks.Paint`'s corner law, not a second roster here.
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

public readonly record struct RetainedReceipt(OverlayVisibility Visibility, Dimension Marks);

public sealed class RetainedOverlay : IDisposable {
    private readonly CustomDisplay display;
    // EXEMPTION [ATOM_STATE]: a `CustomDisplay` write cannot ride a CAS body, and the journal, the native display,
    // and the release flag move together — the one statement-shaped custody on this page.
    private readonly Lock lifecycle = new();
    private readonly FaultCell faults = DisplayFaults.Cell();
    private readonly Op key;
    private static readonly HookId Rail = HookId.Create(value: "rasm.rhino.display.retained");
    private Seq<WorldMark> journal = Seq<WorldMark>();
    private bool released;

    private RetainedOverlay(CustomDisplay display, Op key) => (this.display, this.key) = (display, key);

    public static Fin<RetainedOverlay> Of(OverlayVisibility visibility, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Fin.Succ(new RetainedOverlay(new CustomDisplay(visibility.Key), op)));
    }

    public Seq<IsolatedFault> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public Fin<RetainedReceipt> Apply(RetainedRequest request, Op? key = null) {
        Op op = key.OrDefault();
        lock (lifecycle) {
            return guard(!released, op.InvalidContext()).ToFin()
                .Bind(_ => guard(request is not null && request.Valid, op.InvalidInput()).ToFin())
                .Bind(_ => request.Switch(
                    (Self: this, Op: op),
                    // Transactional through the ONE dispatch: the batch draws via `Marks.Paint`, and a refusal row
                    // OR a host fault clears the native display and replays the pre-request journal.
                    add: static (ctx, row) => {
                        Seq<WorldMark> prior = ctx.Self.journal;
                        return Marks.Paint(
                                new Canvas.Retained(ctx.Self.display),
                                row.Marks.Map(static mark => (DisplayMark)new DisplayMark.World(mark)),
                                ctx.Op)
                            .Bind(receipt => receipt.IsValid
                                ? Fin.Succ((ctx.Self.journal = prior + row.Marks,
                                    new RetainedReceipt(
                                        ctx.Self.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                                        Dimension.Create(value: ctx.Self.journal.Count))).Item2)
                                : Fin.Fail<RetainedReceipt>(receipt.Refused.Fold(Errors.None, static (folded, cause) => folded + cause)))
                            .BindFail(failure => ctx.Self.Restore(prior, ctx.Op).Match(
                                Succ: _ => Fin.Fail<RetainedReceipt>(failure),
                                Fail: cleanup => Fin.Fail<RetainedReceipt>(failure + cleanup)));
                    },
                    visibility: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ((
                        ctx.Self.display.Enabled = row.Value.Key,
                        new RetainedReceipt(row.Value, Dimension.Create(value: ctx.Self.journal.Count))).Item2)),
                    clear: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ((
                        Op.Side(ctx.Self.display.Clear),
                        ctx.Self.journal = Seq<WorldMark>(),
                        new RetainedReceipt(
                            ctx.Self.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                            Dimension.Create(value: 0))).Item3)),
                    inspect: static (ctx, _) => Fin.Succ(new RetainedReceipt(
                        ctx.Self.display.Enabled ? OverlayVisibility.Shown : OverlayVisibility.Hidden,
                        Dimension.Create(value: ctx.Self.journal.Count)))));
        }
    }

    private Fin<Unit> Restore(Seq<WorldMark> prior, Op key) => key.Catch(() => {
        _ = Op.Side(display.Clear);
        journal = Seq<WorldMark>();
        return Marks.Paint(
                new Canvas.Retained(display),
                prior.Map(static mark => (DisplayMark)new DisplayMark.World(mark)),
                key)
            .Map(_ => (journal = prior, unit).Item2);
    });

    public void Dispose() {
        lock (lifecycle) {
            if (released) { return; }
            released = true;
            _ = Custody.Release(
                    releases: Seq<Func<Fin<Unit>>>(
                        () => key.Catch(() => { display.Clear(); return Fin.Succ(value: unit); }),
                        () => key.Catch(() => { display.Dispose(); return Fin.Succ(value: unit); })),
                    key: key)
                .IfFail(cause => {
                    released = false;
                    _ = faults.Park(point: Rail, cause: cause);
                });
        }
    }
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-display.md` — `DisplayConduit` phase overrides, `DrawEventArgs`, channel constants); `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` phase/lane rows, the `PhaseHost` `[Union]`); `Riok.Mapperly` (`libs/csharp/.api/api-mapperly.md` — the conduit-state `[Mapper]`); kernel `Domain/rails` + `Domain/validation` (`PhaseCapability` rows over `CapabilityLaw`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

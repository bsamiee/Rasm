# [RASM_RHINO_DISPLAY_CONDUIT]

`Conduits.Mount` owns filtered display-pipeline participation as a leased phase program with balanced render state and an observable callback-fault rail. `ModeOp` remains the display-mode table seam consumed by `Modes.Configure`; retained overlays and registered analysis remain distinct lifetime shapes under the same host boundary.

`ConduitFrame` is the draw seam consumed by `Marks.Render`. Viewport identity and pass facts detach immediately, while the raw `DisplayPipeline` remains scoped to the host callback that supplied it.

## [01]-[INDEX]

- [02]-[PROGRAM]: `ConduitStep`, `ConduitCriterion`, and `RenderAspect` close phase, filter, and state policy.
- [03]-[MOUNT]: `ConduitLease` owns binding, callback faults, disablement, and unbinding.
- [04]-[MODE_TABLE]: `ModeOp` owns the display-mode table operation family.
- [05]-[OVERLAYS]: `AnalysisMode` and `RetainedOverlay` own registered and retained overlay lifetimes.

## [02]-[PROGRAM]

- Owner: `ConduitStep` carries culling, draw suppression, bounds, per-object draw, and frame draw as one closed phase family.
- Policy: `RenderAspect` is a push/pop pair folded in declaration order and compensated in reverse order after every completed push.
- Filter: `ConduitCriterion` turns every host filter axis into one case-unique row inside the mount request; case runtime type is the uniqueness key, so no parallel criterion-kind vocabulary exists.
- Law: veto is host truth — `Cull` can only widen the incoming `CullObjectEventArgs.CullObject` in the `ObjectCulling` callback and `Suppress` can only narrow the incoming `DrawObjectEventArgs.DrawObject` in `PreDrawObject`, the only two suppression flags the display contract admits; a prior host veto remains set, each decide answers per object per frame, and any deciding step voting to suppress wins.
- Law: world-space draw steps require a bounds step before the adapter is constructed.
- Boundary: callback failures append to the lease fault cell; a host callback never discards a failed rail.
- Growth: a pipeline phase or render state lands as one case and one total adapter arm.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ConduitPhase {
    public static readonly ConduitPhase Culling = new(key: 0, draws: false, perObject: true, worldSpace: false);
    public static readonly ConduitPhase Bounds = new(key: 1, draws: false, perObject: false, worldSpace: false);
    public static readonly ConduitPhase BoundsZoomExtents = new(key: 2, draws: false, perObject: false, worldSpace: false);
    public static readonly ConduitPhase PreObjects = new(key: 3, draws: true, perObject: false, worldSpace: true);
    public static readonly ConduitPhase PreObject = new(key: 4, draws: true, perObject: true, worldSpace: true);
    public static readonly ConduitPhase PostObjects = new(key: 5, draws: true, perObject: false, worldSpace: true);
    public static readonly ConduitPhase Foreground = new(key: 6, draws: true, perObject: false, worldSpace: false);
    public static readonly ConduitPhase Overlay = new(key: 7, draws: true, perObject: false, worldSpace: false);

    public bool Draws { get; }
    public bool PerObject { get; }
    public bool WorldSpace { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderAspect {
    private RenderAspect() { }
    public sealed record Toggle(RenderSwitch Target, bool Enabled) : RenderAspect;
    public sealed record Cull(CullUse Mode) : RenderAspect;
    public sealed record Model(Transform Transform) : RenderAspect;
    public sealed record Screen : RenderAspect;

    internal bool Valid => Switch(
        toggle: static row => row.Target is not null,
        cull: static row => row.Mode is not null,
        model: static row => row.Transform.IsValid,
        screen: static _ => true);

    internal Fin<Unit> With(DisplayPipeline pipeline, Func<Fin<Unit>> draw, Op key) {
        bool acquired = false;
        Fin<Unit> primary = key.Catch(() => {
            Switch(
                pipeline,
                toggle: static (p, row) => row.Target.Push(p, row.Enabled),
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

[SmartEnum<int>]
public sealed partial class PointUse {
    public static readonly PointUse Simple = Row(0, PointStyle.Simple);
    public static readonly PointUse Control = Row(1, PointStyle.ControlPoint);
    public static readonly PointUse Active = Row(2, PointStyle.ActivePoint);
    public static readonly PointUse Cross = Row(3, PointStyle.X);
    public static readonly PointUse RoundSimple = Row(4, PointStyle.RoundSimple);
    public static readonly PointUse RoundControl = Row(5, PointStyle.RoundControlPoint);
    public static readonly PointUse RoundActive = Row(6, PointStyle.RoundActivePoint);
    public static readonly PointUse Circle = Row(7, PointStyle.Circle);
    public static readonly PointUse Square = Row(8, PointStyle.Square);
    public static readonly PointUse Triangle = Row(9, PointStyle.Triangle);
    public static readonly PointUse Heart = Row(10, PointStyle.Heart);
    public static readonly PointUse Chevron = Row(11, PointStyle.Chevron);
    public static readonly PointUse Clover = Row(12, PointStyle.Clover);
    public static readonly PointUse Tag = Row(13, PointStyle.Tag);
    public static readonly PointUse Asterisk = Row(14, PointStyle.Asterisk);
    public static readonly PointUse Pin = Row(15, PointStyle.Pin);
    public static readonly PointUse ArrowTail = Row(16, PointStyle.ArrowTail);
    public static readonly PointUse ArrowTip = Row(17, PointStyle.ArrowTip);
    public static readonly PointUse VariableDot = Row(18, PointStyle.VariableDot);
    public static readonly PointUse SolidSquare = Row(19, PointStyle.SolidSquare);
    public static readonly PointUse RoundDot = Row(20, PointStyle.RoundDot);
    public static readonly PointUse SolidRound = Row(21, PointStyle.SolidRound);
    public static readonly PointUse SolidCircle = Row(22, PointStyle.SolidCircle);
    public static readonly PointUse None = Row(23, PointStyle.None);

    private static PointUse Row(int key, PointStyle native) => new(key, native);

    internal PointStyle Native { get; }
}

[SmartEnum<int>]
public sealed partial class ActiveSpaceUse {
    public static readonly ActiveSpaceUse None = new(0, ActiveSpace.None);
    public static readonly ActiveSpaceUse Model = new(1, ActiveSpace.ModelSpace);
    public static readonly ActiveSpaceUse Page = new(2, ActiveSpace.PageSpace);
    public static readonly ActiveSpaceUse UvEditor = new(3, ActiveSpace.UVEditorSpace);
    public static readonly ActiveSpaceUse BlockEditor = new(4, ActiveSpace.BlockEditorSpace);
    internal ActiveSpace Native { get; }
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
        objects: static (c, row) => Op.Side(() => c.SetObjectIdFilter(row.Ids.Filter(static id => id != Guid.Empty).Distinct().AsEnumerable())),
        geometry: static (c, row) => Op.Side(() => c.GeometryFilter = row.Kinds.Mask),
        space: static (c, row) => Op.Side(() => c.SpaceFilter = row.Value.Native));
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

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct FrameContext(
    bool Capturing,
    bool Printing,
    bool Dynamic,
    int RenderPass,
    int NestLevel,
    float DpiScale) {
    internal static FrameContext Of(DisplayPipeline pipeline) => new(
        pipeline.IsInViewCapture,
        pipeline.IsPrinting,
        pipeline.IsDynamicDisplay,
        pipeline.RenderPass,
        pipeline.NestLevel,
        pipeline.DpiScale);
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
        new(pipeline, viewport.Id, viewport.ChangeCounter, FrameContext.Of(pipeline), phase);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitStep {
    private ConduitStep() { }
    public sealed record Cull(Func<Guid, ConduitFrame, Fin<bool>> Decide) : ConduitStep;
    public sealed record Suppress(Func<Guid, ConduitFrame, Fin<bool>> Decide) : ConduitStep;
    public sealed record Bounds(ConduitPhase Phase, Func<ConduitFrame, Fin<BoundingBox>> Contribute) : ConduitStep;
    public sealed record ObjectDraw(Seq<RenderAspect> State, Func<Guid, ConduitFrame, Fin<Seq<Mark>>> Project) : ConduitStep;
    public sealed record Draw(ConduitPhase Phase, Seq<RenderAspect> State, Func<ConduitFrame, Fin<Seq<Mark>>> Project) : ConduitStep;

    internal bool Valid => Switch(
        cull: static row => row.Decide is not null,
        suppress: static row => row.Decide is not null,
        bounds: static row => row.Contribute is not null && (row.Phase == ConduitPhase.Bounds || row.Phase == ConduitPhase.BoundsZoomExtents),
        objectDraw: static row => row.Project is not null && row.State.ForAll(static aspect => aspect is not null && aspect.Valid),
        draw: static row => row.Project is not null && row.Phase is not null && row.Phase.Draws && !row.Phase.PerObject && row.State.ForAll(static aspect => aspect is not null && aspect.Valid));

    internal (bool Supplies, bool Requires) BoundsOrder => Switch(
        cull: static _ => (false, false),
        suppress: static _ => (false, false),
        bounds: static row => (row.Phase == ConduitPhase.Bounds, false),
        objectDraw: static _ => (false, true),
        draw: static row => (false, row.Phase.WorldSpace));
}

public sealed record ConduitProgram {
    private ConduitProgram(Seq<ConduitStep> steps, ConduitBinding binding, Seq<ConduitCriterion> criteria) =>
        (Steps, Binding, Criteria) = (steps, binding, criteria);

    public Seq<ConduitStep> Steps { get; }
    public ConduitBinding Binding { get; }
    public Seq<ConduitCriterion> Criteria { get; }

    public static Fin<ConduitProgram> Of(
        Seq<ConduitStep> steps,
        ConduitBinding binding,
        Seq<ConduitCriterion> criteria,
        Op? key = null) {
        Op op = key.OrDefault();
        bool stepsValid = !steps.IsEmpty && steps.ForAll(static step => step is not null && step.Valid);
        bool criteriaValid = criteria.ForAll(static criterion => criterion is not null && criterion.Valid)
            && criteria.Map(static criterion => criterion.GetType()).Distinct().Count == criteria.Count;
        bool bindingValid = binding is not null && binding.Valid;
        bool boundsValid = steps.Map(static step => step.BoundsOrder)
            .Fold(
                (Supplied: false, Valid: true),
                static (state, row) => (
                    Supplied: state.Supplied || row.Supplies,
                    Valid: state.Valid && (!row.Requires || state.Supplied)))
            .Valid;
        return guard(stepsValid && criteriaValid && bindingValid && boundsValid, op.InvalidInput()).ToFin()
            .Map(_ => new ConduitProgram(steps, binding, criteria));
    }
}
```

## [03]-[MOUNT]

- Owner: `ConduitLease` owns the adapter and its callback-fault cell until deterministic release.
- Entry: `ConduitProgram.Of` admits steps, binding, and case-unique criteria; `Conduits.Mount` applies the admitted program and arms participation.
- Owner: `ConduitHooks.Mount` registers the two display veto points — `rasm.rhino.display.cull` and `rasm.rhino.display.drawobject` — on the `MountRegistry`; the ask is a `(DocumentSession, ConduitProgram)` pair whose program must carry the point's own veto step (`Cull` for the cull point, `Suppress` for the drawobject point), the grant is the mounted `ConduitLease`, and a program missing the veto step refuses typed before any host participation arms.
- Law: every fault the cell records also publishes through `ObjectsTelemetry` under `FaultSite.Conduit` — the cell is the lease's readable receipt, the publish the process egress, and a second logger sink beside them is the fork.
- Law: release runs every step in order — disable, `UnbindAll`, sprite disposal — a thrown step never skips the rest, the combined failure lands on the fault cell, and a failed release re-arms `Dispose` for retry; callback faults remain readable after release as detached `Seq<Error>` evidence.
- Boundary: the adapter is the only `DisplayConduit` subclass and the only statement-shaped host callback seam.

```csharp signature
// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class ConduitAdapter : DisplayConduit {
    private readonly ConduitProgram program;
    private readonly Atom<Seq<Error>> faults;
    private readonly SpriteSheet sprites = new();
    private readonly Op key;

    internal ConduitAdapter(ConduitProgram program, Atom<Seq<Error>> faults, Op key) =>
        (this.program, this.faults, this.key) = (program, faults, key);

    protected override void ObjectCulling(CullObjectEventArgs e) => Invoke(() =>
        program.Steps.Choose(static step => step is ConduitStep.Cull row ? Some(row) : None)
            .TraverseM(step => step.Decide(
                e.RhinoObject.Id,
                ConduitFrame.Of(e.Display, e.Viewport, ConduitPhase.Culling))).As()
            .Map(decisions => (e.CullObject = e.CullObject || decisions.Exists(static visible => !visible), unit).Item2));

    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) => Invoke(() =>
        Bounds(e, ConduitPhase.Bounds));

    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) => Invoke(() =>
        Bounds(e, ConduitPhase.BoundsZoomExtents));

    protected override void PreDrawObjects(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.PreObjects));

    protected override void PreDrawObject(DrawObjectEventArgs e) => Invoke(() =>
        program.Steps.Choose(static step => step is ConduitStep.Suppress row ? Some(row) : None)
            .TraverseM(step => step.Decide(
                e.RhinoObject.Id,
                ConduitFrame.Of(e.Display, e.Viewport, ConduitPhase.PreObject))).As()
            .Map(votes => (e.DrawObject = e.DrawObject && !votes.Exists(static suppress => suppress), unit).Item2)
            .Bind(_ => e.DrawObject
                ? program.Steps.Choose(static step => step is ConduitStep.ObjectDraw row ? Some(row) : None)
                    .TraverseM(step => Project(e, step)).As()
                    .Map(static _ => unit)
                : Fin.Succ(value: unit)));

    protected override void PostDrawObjects(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.PostObjects));
    protected override void DrawForeground(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.Foreground));
    protected override void DrawOverlay(DrawEventArgs e) => Invoke(() => Draw(e, ConduitPhase.Overlay));

    private Fin<Unit> Bounds(CalculateBoundingBoxEventArgs e, ConduitPhase phase) =>
        program.Steps.Choose(static step => step is ConduitStep.Bounds row ? Some(row) : None)
            .Filter(step => step.Phase == phase)
            .TraverseM(step => step.Contribute(ConduitFrame.Of(e.Display, e.Viewport, phase))
                .Bind(box => guard(box.IsValid, key.InvalidResult()).ToFin()
                    .Map(_ => Op.Side(() => e.IncludeBoundingBox(box))))).As()
            .Map(static _ => unit);

    private Fin<Unit> Draw(DrawEventArgs e, ConduitPhase phase) =>
        program.Steps.Choose(static step => step is ConduitStep.Draw row ? Some(row) : None)
            .Filter(step => step.Phase == phase)
            .TraverseM(step => {
                ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, phase);
                return Render(frame, step.State, step.Project(frame));
            }).As()
            .Map(static _ => unit);

    private Fin<Unit> Render(ConduitFrame frame, Seq<RenderAspect> state, Fin<Seq<Mark>> projected) =>
        PipelineScope.With(frame.Pipeline, state, () => projected.Bind(marks => Marks.Render(
            new Canvas.Pipeline(frame), sprites, marks).Map(static _ => unit)), key);

    private Fin<Unit> Project(DrawObjectEventArgs e, ConduitStep.ObjectDraw step) {
        ConduitFrame frame = ConduitFrame.Of(e.Display, e.Viewport, ConduitPhase.PreObject);
        return Render(frame, step.State, step.Project(e.RhinoObject.Id, frame));
    }

    private void Invoke(Func<Fin<Unit>> callback) => Observe(key.Catch(callback));

    private void Observe<T>(Fin<T> outcome) =>
        outcome.IfFail(error => ignore((
            faults.Swap(seen => seen.Add(error)),
            ObjectsTelemetry.Publish(site: FaultSite.Conduit, error: error))));

    internal Fin<Unit> Release() {
        Seq<Error> trouble = Seq(
                key.Catch(() => { Enabled = false; return Fin.Succ(value: unit); }),
                key.Catch(() => { UnbindAll(); return Fin.Succ(value: unit); }),
                key.Catch(() => { sprites.Dispose(); return Fin.Succ(value: unit); }))
            .Choose(static step => step.Match(Succ: static _ => Option<Error>.None, Fail: static failure => Some(failure)));
        _ = Op.SideWhen(!trouble.IsEmpty, () => ignore(faults.Swap(seen => seen + trouble)));
        return trouble.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, error) => folded + error));
    }
}

internal static class PipelineScope {
    internal static Fin<Unit> With(DisplayPipeline pipeline, Seq<RenderAspect> state, Func<Fin<Unit>> draw, Op key) =>
        toSeq(state.AsEnumerable().Reverse())
            .Fold(draw, (next, aspect) => () => aspect.With(pipeline, next, key))();
}

public sealed class ConduitLease : IDisposable {
    private readonly ConduitAdapter adapter;
    private readonly Atom<Seq<Error>> faults;
    private int released;

    internal ConduitLease(ConduitAdapter adapter, Atom<Seq<Error>> faults) =>
        (this.adapter, this.faults) = (adapter, faults);

    public Seq<Error> Faults => faults.Value;

    public void Dispose() {
        if (Interlocked.CompareExchange(ref released, 1, 0) != 0) { return; }
        _ = adapter.Release().IfFail(_ => Volatile.Write(ref released, 0));
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
               from faults in Fin.Succ(Atom(Seq<Error>()))
               from adapter in Fin.Succ(new ConduitAdapter(admitted, faults, op))
               from lease in (from __ in op.Catch(() => Fin.Succ(admitted.Criteria
                                  .Fold(unit, static (_, criterion) => criterion.Apply(adapter))))
                              from ___ in Bind(owner, adapter, admitted.Binding, op)
                              from ____ in op.Catch(() => Fin.Succ((adapter.Enabled = true, unit).Item2))
                              select new ConduitLease(adapter, faults)).BiBind(
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

public static class ConduitHooks {
    public static Fin<Seq<IDisposable>> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.MountAll(
            mounts: Seq(
                (Point: HookPoint.DisplayCull, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Cull)),
                (Point: HookPoint.DisplayDrawObject, Carries: (Func<ConduitStep, bool>)(static step => step is ConduitStep.Suppress)))
            .Map(row => (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                mount: new HookMount(
                    Point: row.Point,
                    Plugin: plugin,
                    Ask: typeof(ConduitVetoAsk),
                    Grant: typeof(ConduitLease),
                    Bind: ask => {
                        ConduitVetoAsk request = (ConduitVetoAsk)ask;
                        return guard(request.Program.Steps.Exists(row.Carries), op.InvalidInput()).ToFin()
                            .Bind(_ => Conduits.Mount(session: request.Session, program: request.Program, key: op)
                                .Map(static lease => (object)lease));
                    }),
                key: op))),
            key: op);
    }
}
```

## [04]-[MODE_TABLE]

- Owner: `ModeOp` is the closed table request family consumed by `Modes.Configure`.
- Entry: `ModeOp.Apply` returns resolved descriptors for every operation, including host-minted identities.
- Law: every minted identifier re-resolves before egress; a dangling identifier never becomes a descriptor receipt.
- Growth: a table verb is one request case and one dispatch arm.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record ModeOp {
    private ModeOp() { }
    internal sealed record CensusCase : ModeOp;
    internal sealed record FindCase(ModeId Mode) : ModeOp;
    internal sealed record NamedCase(string Name) : ModeOp;
    internal sealed record AddCase(DisplayModeDescription Mode) : ModeOp;
    internal sealed record BlankCase(string Name) : ModeOp;
    internal sealed record UpdateCase(DisplayModeDescription Mode) : ModeOp;
    internal sealed record CopyCase(ModeId Source, string Name) : ModeOp;
    internal sealed record DeleteCase(ModeId Mode) : ModeOp;
    internal sealed record ImportCase(string Path, bool Interactive) : ModeOp;
    internal sealed record ExportCase(ModeId Mode, string Path) : ModeOp;

    internal Fin<Seq<DisplayModeDescription>> Apply(Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            op,
            censusCase: static (inner, _) => inner.Catch(() => Fin.Succ(toSeq(DisplayModeDescription.GetDisplayModes()))),
            findCase: static (inner, row) => Resolve(row.Mode, inner).Map(static mode => Seq(mode)),
            namedCase: static (inner, row) => inner.Catch(() =>
                Optional(DisplayModeDescription.FindByName(row.Name)).ToFin(inner.InvalidInput()).Map(static mode => Seq(mode))),
            addCase: static (inner, row) => Mint(() => DisplayModeDescription.AddDisplayMode(row.Mode), inner),
            blankCase: static (inner, row) => Mint(() => DisplayModeDescription.AddDisplayMode(row.Name), inner),
            updateCase: static (inner, row) => inner.Catch(() =>
                inner.Confirm(DisplayModeDescription.UpdateDisplayMode(row.Mode)).Map(_ => Seq(row.Mode))),
            copyCase: static (inner, row) => Mint(() => DisplayModeDescription.CopyDisplayMode(row.Source.Value, row.Name), inner),
            deleteCase: static (inner, row) => Resolve(row.Mode, inner)
                .Bind(mode => inner.Catch(() => inner.Confirm(DisplayModeDescription.DeleteDisplayMode(row.Mode.Value)).Map(_ => Seq(mode)))),
            importCase: static (inner, row) => Mint(() => DisplayModeDescription.ImportFromFile(row.Path, row.Interactive), inner),
            exportCase: static (inner, row) => Resolve(row.Mode, inner)
                .Bind(mode => inner.Catch(() => inner.Confirm(DisplayModeDescription.ExportToFile(mode, row.Path)).Map(_ => Seq(mode)))));
    }

    private static Fin<DisplayModeDescription> Resolve(ModeId id, Op key) =>
        key.Catch(() => Optional(DisplayModeDescription.GetDisplayMode(id.Value)).ToFin(key.InvalidInput()));

    private static Fin<Seq<DisplayModeDescription>> Mint(Func<Guid> mint, Op key) =>
        key.Catch(() => mint() is var id && id != Guid.Empty
            ? Resolve(ModeId.Create(id), key).Map(static mode => Seq(mode))
            : Fin.Fail<Seq<DisplayModeDescription>>(key.InvalidResult()));
}
```

## [05]-[OVERLAYS]

- Owner: `AnalysisMode` is the implement seam for registered false-color overlays; `RetainedOverlay` is the owned `CustomDisplay` capsule.
- Entry: `AnalysisMode.Register<TMode>` and `AnalysisMode.Activate` close registration and object participation; `RetainedOverlay.Apply` closes retained requests.
- Law: registered analysis, retained accumulation, and per-frame conduits keep distinct lifecycle owners.
- Law: `Add` is transactional — the capsule journals every retained mark, a mid-batch host refusal clears the native display and replays the pre-request journal, and the mark count derives from the journal; a released overlay refuses with context evidence while a malformed request refuses with input evidence.
- Boundary: retained geometry never escapes the capsule; disposal captures both cleanup steps, retains every refusal, and re-arms only an incomplete release.

```csharp signature
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

    internal Fin<Unit> Activate(RhinoObject subject, bool enabled, Op? key = null) {
        Op op = key.OrDefault();
        return from target in Optional(subject).ToFin(op.InvalidInput())
               from _ in op.Catch(() => op.Confirm(ObjectSupportsAnalysisMode(target)))
               from activated in op.Catch(() => op.Confirm(target.EnableVisualAnalysisMode(this, enabled)))
               select activated;
    }

    protected abstract Unit OnFault(Error error);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetainedMark {
    private RetainedMark() { }
    public sealed record Points(Seq<Point3d> Values, PerceptualColor Color, PointUse Style, int Radius) : RetainedMark;
    public sealed record Line(global::Rhino.Geometry.Line Value, PerceptualColor Color, int Width) : RetainedMark;
    public sealed record Vector(Point3d Anchor, Vector3d Span, PerceptualColor Color, bool AnchorPoint) : RetainedMark;
    public sealed record Arc(global::Rhino.Geometry.Arc Value, PerceptualColor Color, int Width) : RetainedMark;
    public sealed record Circle(global::Rhino.Geometry.Circle Value, PerceptualColor Color, int Width) : RetainedMark;
    public sealed record Curve(global::Rhino.Geometry.Curve Value, PerceptualColor Color, int Width) : RetainedMark;
    public sealed record Polygon(Seq<Point3d> Ring, PerceptualColor Fill, PerceptualColor Edge, bool DrawFill, bool DrawEdge) : RetainedMark;
    public sealed record Text(Text3d Value, PerceptualColor Color) : RetainedMark;

    internal bool Valid => Switch(
        points: static row => !row.Values.IsEmpty && row.Style is not null && row.Radius > 0,
        line: static row => row.Width > 0,
        vector: static _ => true,
        arc: static row => row.Width > 0,
        circle: static row => row.Width > 0,
        curve: static row => row.Value is not null && row.Width > 0,
        polygon: static row => row.Ring.Count >= 3 && (row.DrawFill || row.DrawEdge),
        text: static row => row.Value is not null);

    internal Unit Add(CustomDisplay display) => Switch(
        display,
        points: static (d, row) => Op.Side(() => d.AddPoints(row.Values.AsEnumerable(), Quant.Sys(row.Color), row.Style.Native, row.Radius)),
        line: static (d, row) => Op.Side(() => d.AddLine(row.Value, Quant.Sys(row.Color), row.Width)),
        vector: static (d, row) => Op.Side(() => d.AddVector(row.Anchor, row.Span, Quant.Sys(row.Color), row.AnchorPoint)),
        arc: static (d, row) => Op.Side(() => d.AddArc(row.Value, Quant.Sys(row.Color), row.Width)),
        circle: static (d, row) => Op.Side(() => d.AddCircle(row.Value, Quant.Sys(row.Color), row.Width)),
        curve: static (d, row) => Op.Side(() => d.AddCurve(row.Value, Quant.Sys(row.Color), row.Width)),
        polygon: static (d, row) => Op.Side(() => d.AddPolygon(row.Ring.AsEnumerable(), Quant.Sys(row.Fill), Quant.Sys(row.Edge), row.DrawFill, row.DrawEdge)),
        text: static (d, row) => Op.Side(() => d.AddText(row.Value, Quant.Sys(row.Color))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetainedRequest {
    private RetainedRequest() { }
    public sealed record Add(Seq<RetainedMark> Marks) : RetainedRequest;
    public sealed record Enable(bool Visible) : RetainedRequest;
    public sealed record Clear : RetainedRequest;
    public sealed record Inspect : RetainedRequest;

    internal bool Valid => Switch(
        add: static row => !row.Marks.IsEmpty && row.Marks.ForAll(static mark => mark is not null && mark.Valid),
        enable: static _ => true,
        clear: static _ => true,
        inspect: static _ => true);
}

public readonly record struct RetainedReceipt(bool Visible, int Marks);

public sealed class RetainedOverlay : IDisposable {
    private readonly CustomDisplay display;
    private readonly Lock lifecycle = new();
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly Op key;
    private Seq<RetainedMark> journal = Seq<RetainedMark>();
    private int released;

    private RetainedOverlay(CustomDisplay display, Op key) => (this.display, this.key) = (display, key);

    public static Fin<RetainedOverlay> Of(bool visible, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Fin.Succ(new RetainedOverlay(new CustomDisplay(visible), op)));
    }

    public Seq<Error> Faults => faults.Value;

    public Fin<RetainedReceipt> Apply(RetainedRequest request, Op? key = null) {
        Op op = key.OrDefault();
        lock (lifecycle) {
            return guard(released == 0, op.InvalidContext()).ToFin()
                .Bind(_ => guard(request is not null && request.Valid, op.InvalidInput()).ToFin())
                .Bind(_ => request.Switch(
                (Self: this, Op: op),
                add: static (ctx, row) => {
                    Seq<RetainedMark> prior = ctx.Self.journal;
                    return row.Marks.TraverseM(mark => ctx.Op.Catch(() => Fin.Succ((
                            mark.Add(ctx.Self.display),
                            ctx.Self.journal = ctx.Self.journal.Add(mark)).Item2))).As()
                        .Map(_ => new RetainedReceipt(ctx.Self.display.Enabled, ctx.Self.journal.Count))
                        .BindFail(failure => ctx.Self.Restore(prior, ctx.Op).Match(
                            Succ: _ => Fin.Fail<RetainedReceipt>(failure),
                            Fail: cleanup => Fin.Fail<RetainedReceipt>(failure + cleanup)));
                },
                enable: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ((
                    ctx.Self.display.Enabled = row.Visible,
                    new RetainedReceipt(row.Visible, ctx.Self.journal.Count)).Item2)),
                clear: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ((
                    Op.Side(ctx.Self.display.Clear),
                    ctx.Self.journal = Seq<RetainedMark>(),
                    new RetainedReceipt(ctx.Self.display.Enabled, 0)).Item3)),
                inspect: static (ctx, _) => Fin.Succ(new RetainedReceipt(ctx.Self.display.Enabled, ctx.Self.journal.Count))));
        }
    }

    private Fin<Unit> Restore(Seq<RetainedMark> prior, Op key) => key.Catch(() => {
        _ = Op.Side(display.Clear);
        journal = Seq<RetainedMark>();
        return prior.TraverseM(mark => key.Catch(() => Fin.Succ((
                mark.Add(display),
                journal = journal.Add(mark)).Item2))).As()
            .Map(static _ => unit);
    });

    private Fin<Unit> Release() {
        Seq<Error> trouble = Seq(
                key.Catch(() => { display.Clear(); return Fin.Succ(value: unit); }),
                key.Catch(() => { display.Dispose(); return Fin.Succ(value: unit); }))
            .Choose(static step => step.Match(Succ: static _ => Option<Error>.None, Fail: static failure => Some(failure)));
        _ = Op.SideWhen(!trouble.IsEmpty, () => ignore(faults.Swap(seen => seen + trouble)));
        return trouble.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, error) => folded + error));
    }

    public void Dispose() {
        lock (lifecycle) {
            if (released != 0) { return; }
            released = 1;
            _ = Release().IfFail(_ => released = 0);
        }
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

using System.Globalization;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Geometry.SpatialTree;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Snap;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GuidSet = System.Collections.Generic.HashSet<System.Guid>;
using ISnapElement = Grasshopper2.UI.Snap.ISnapElement;
using Op = Rasm.Domain.Op;
using SnapLine = Grasshopper2.UI.Snap.SnapLine;
using SysAction = System.Action;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LayoutAxis {
    public static readonly LayoutAxis Horizontal = new(
        key: 0,
        origin: static bounds => bounds.Left,
        span: static bounds => bounds.Width,
        delta: static (cursor, bounds) => (Dx: cursor - bounds.Left, Dy: 0f));

    public static readonly LayoutAxis Vertical = new(
        key: 1,
        origin: static bounds => bounds.Top,
        span: static bounds => bounds.Height,
        delta: static (cursor, bounds) => (Dx: 0f, Dy: cursor - bounds.Top));

    [UseDelegateFromConstructor]
    internal partial float Origin(RectangleF bounds);

    [UseDelegateFromConstructor]
    internal partial float Span(RectangleF bounds);

    [UseDelegateFromConstructor]
    internal partial (float Dx, float Dy) Delta(float cursor, RectangleF bounds);

    internal float Offset(float dx, float dy) => Equals(Horizontal) ? dx : dy;
}

[SmartEnum<int>]
public sealed partial class ResizeAxis {
    public static readonly ResizeAxis Horizontal = new(key: 0, mask: static (dx, _) => (dx, 0f));
    public static readonly ResizeAxis Vertical = new(key: 1, mask: static (_, dy) => (0f, dy));
    public static readonly ResizeAxis Both = new(key: 2, mask: static (dx, dy) => (dx, dy));

    [UseDelegateFromConstructor]
    internal partial (float Dx, float Dy) Mask(float dx, float dy);
}

[SmartEnum<int>]
public sealed partial class LayoutGapPolicy {
    // Sub-pixel slack keeps marginal overflows on the fixed cursor before the stretch solver takes over.
    internal float ContentSlack { get; }
    public static readonly LayoutGapPolicy Stretch = new(key: 0, contentSlack: 1e-4f);
    public static readonly LayoutGapPolicy Fixed = new(key: 1, contentSlack: 0f);
}

[ValueObject<float>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<UiFault>]
public readonly partial struct LayoutGap {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float value) =>
        validationError = float.IsFinite(value) && value >= 0f
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(LayoutGap)), message: string.Create(CultureInfo.InvariantCulture, $"must be finite and >= 0 (got {value:R})."));
}

[SkipUnionOps]
[Union]
public partial record LayoutArrangement {
    private LayoutArrangement() { }
    public sealed record MoveCase(Guid Id, float Dx, float Dy, SnappingPolicy Policy = default) : LayoutArrangement;
    public sealed record PlaceCase(Guid Id, PointF Pivot, Option<SnappingPolicy> Policy = default) : LayoutArrangement;
    public sealed record AlignCase(Guid Anchor, Seq<Guid> Targets, OCD.Fixed Fix) : LayoutArrangement;
    public sealed record ChainCase(LayoutChainKind Kind, LayoutAxis Axis, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record GridCase(int Rows, int Cols, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record NudgeCase(Seq<Guid> Ids, float Separation, SnappingPolicy Policy = default) : LayoutArrangement;

    public static LayoutArrangement Align(Guid anchor, Seq<Guid> targets, OCD.Fixed fix = OCD.Fixed.None) =>
        new AlignCase(Anchor: anchor, Targets: targets, Fix: fix);

    public static LayoutArrangement Distribute(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new ChainCase(Kind: LayoutChainKind.Distribute, Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Stretch);

    public static LayoutArrangement Grid(int rows, int cols, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy) =>
        new GridCase(Rows: rows, Cols: cols, Gap: gap, Ids: ids, GapPolicy: gapPolicy);

    public static LayoutArrangement Flow(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new ChainCase(Kind: LayoutChainKind.Flow, Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Stretch);

    public static LayoutArrangement Topology(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new ChainCase(Kind: LayoutChainKind.Flow, Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Fixed);

    public static LayoutArrangement Nudge(Seq<Guid> ids, float separation, SnappingPolicy policy = default) =>
        new NudgeCase(Ids: ids, Separation: separation, Policy: policy);
}

[SmartEnum<int>]
public sealed partial class LayoutChainKind {
    public static readonly LayoutChainKind Distribute = new(key: 0, arrange: static (axis, gap, ids, gapPolicy) => Layout.Distribute(axis: axis, gap: gap, gapPolicy: gapPolicy, ids: [.. ids]));
    public static readonly LayoutChainKind Flow = new(key: 1, arrange: static (axis, gap, ids, gapPolicy) => Layout.Flow(axis: axis, gap: gap, gapPolicy: gapPolicy, ids: [.. ids]));

    [UseDelegateFromConstructor]
    internal partial GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Arrange(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy);
}

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<float>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<UiFault>]
public readonly partial struct PickRadius {
    public static readonly PickRadius Default = Create(value: 32f);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float value) =>
        validationError = float.IsFinite(value) && value > 0f
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(PickRadius)), message: string.Create(CultureInfo.InvariantCulture, $"must be finite and > 0 (got {value:R})."));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutSnapshot(Guid ObjectId, PointF Pivot, RectangleF Bounds, RectangleF AggregateBounds, bool Snappable) {
    public SizeF Size => new(width: Bounds.Width, height: Bounds.Height);
    public SizeF AggregateSize => new(width: AggregateBounds.Width, height: AggregateBounds.Height);
    public PointF Centre => Bounds.Center;
    public PointF AggregateCentre => AggregateBounds.Center;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnapLabel(string Text, PointF Point, Option<TextAnchor> Anchor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingSnapshot(float Dx, float Dy, float Magnitude, Option<SnapLabel> XLabel, Option<SnapLabel> YLabel, Seq<LineF> Lines);

[SkipUnionOps]
[Union]
public partial record SnapSetting {
    private SnapSetting() { }
    public sealed record WithRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record WithoutRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record FeedbackCase(bool Enabled, SnapGuideStyle XStyle, SnapGuideStyle YStyle = default) : SnapSetting;
    public sealed record RadiiCase(Option<int> EdgeRadius = default, Option<int> WireRadius = default, Option<int> VerticalGapSize = default, Option<int> HorizontalGapSize = default) : SnapSetting;
    public sealed record SpaceCase(SnapSpace Space) : SnapSetting;
    public sealed record GeometryCase(SnapProjection Projection, double Radius) : SnapSetting;
    public sealed record SmartGapCase(bool Enabled, int Tolerance = 2) : SnapSetting;
    public sealed record WireBoundsCase(bool Aggregate = true) : SnapSetting;

    public static SnapSetting Grid(SizeF cell, PointF origin = default) =>
        new SpaceCase(Space: SnapSpace.CreateOrthogonal(originX: origin.X, originY: origin.Y, sizeX: cell.Width, sizeY: cell.Height));

    internal SnappingSettings Apply(SnappingSettings settings) =>
        Switch(
            state: settings,
            withRuleCase: static (state, op) => state.WithRules(rules: op.Rule),
            withoutRuleCase: static (state, op) => state.WithoutRules(rules: op.Rule),
            feedbackCase: static (state, op) => state.WithFeedback(drawFeedback: op.Enabled, colour: op.XStyle.Tint),
            radiiCase: static (state, op) => new SnappingSettings(rules: state.Rules, verticalGap: op.VerticalGapSize.Map(static value => Math.Max(0, value)).IfNone(state.VerticalGapSize), horizontalGap: op.HorizontalGapSize.Map(static value => Math.Max(0, value)).IfNone(state.HorizontalGapSize), edgeRadius: op.EdgeRadius.Map(static value => Math.Max(0, value)).IfNone(state.EdgeRadius), wireRadius: op.WireRadius.Map(static value => Math.Max(0, value)).IfNone(state.WireRadius), feedback: state.Feedback, colour: state.Colour),
            spaceCase: static (state, _) => state,
            geometryCase: static (state, _) => state,
            smartGapCase: static (state, _) => state,
            wireBoundsCase: static (state, _) => state);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(bool IncludeSelected = true, bool IncludeUnselected = true, Seq<SnapSetting> Settings = default) {
    internal SnappingSettings Native =>
        Settings.Fold(SnappingSettings.Current, static (state, op) => op.Apply(settings: state));

    // First enabled feedback setting drives the live snap-guide cosmetic; default YStyle falls back to XStyle.
    internal Option<(SnapGuideStyle X, SnapGuideStyle Y)> FeedbackStyle =>
        Settings.Choose(static s => s is SnapSetting.FeedbackCase { Enabled: true } feedback
            ? Some((feedback.XStyle, feedback.YStyle.Equals(default) ? feedback.XStyle : feedback.YStyle))
            : Option<(SnapGuideStyle, SnapGuideStyle)>.None).Head;

    internal Seq<SnapSpace> Spaces =>
        Settings.Choose(static s => s is SnapSetting.SpaceCase space ? Some(space.Space) : Option<SnapSpace>.None);

    internal Option<(SnapProjection Projection, double Radius)> Geometry =>
        Settings.Choose(static s => s is SnapSetting.GeometryCase geo ? Some((geo.Projection, geo.Radius)) : Option<(SnapProjection, double)>.None).Head;

    internal Option<int> SmartGap =>
        Settings.Choose(static s => s is SnapSetting.SmartGapCase { Enabled: true } gap ? Some(gap.Tolerance) : Option<int>.None).Head;

    internal bool UseAggregateWireBounds =>
        Settings.Choose(static s => s is SnapSetting.WireBoundsCase wire ? Some(wire.Aggregate) : Option<bool>.None)
            .Head
            .IfNone(noneValue: true);

    // GeometryCase contributes aggregate-bounds edges plus centre midlines, filtered by projection axis.
    internal Option<SnapSpace> GeometrySpace(GhObjectList objects, GuidSet excludeIds) =>
        Geometry.Map(geo => SnapSpace.Create(elements: [.. toSeq(objects.Forwards)
            .Filter(o => !excludeIds.Contains(o.InstanceId))
            .Bind(o => GeometryLines(bounds: o.Attributes.AggregateBounds, projection: geo.Projection, radius: geo.Radius))]));

    private static Seq<ISnapElement> GeometryLines(RectangleF bounds, SnapProjection projection, double radius) {
        bool wantsX = projection is SnapProjection.X or SnapProjection.XY;
        bool wantsY = projection is SnapProjection.Y or SnapProjection.XY;
        Seq<LineF> vertical = Seq(new LineF(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom), new LineF(bounds.Right, bounds.Top, bounds.Right, bounds.Bottom), new LineF(bounds.Center.X, bounds.Top, bounds.Center.X, bounds.Bottom));
        Seq<LineF> horizontal = Seq(new LineF(bounds.Left, bounds.Top, bounds.Right, bounds.Top), new LineF(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom), new LineF(bounds.Left, bounds.Center.Y, bounds.Right, bounds.Center.Y));
        return ((wantsX ? vertical : []) + (wantsY ? horizontal : []))
            .Map(ISnapElement (line) => new SnapLine(line: new LinearSegment(x0: line.Start.X, y0: line.Start.Y, x1: line.End.X, y1: line.End.Y), infinite: false, radius: radius));
    }
}

[SkipUnionOps]
[Union]
public partial record SnapProbe {
    private SnapProbe() { }
    public sealed record PointCase(Guid ObjectId, PointF Probe, float Radius = 32f, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record RectangleCase(Guid ObjectId, RectangleF Bounds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record ObjectCase(Guid ObjectId, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record GroupCase(Seq<Guid> ObjectIds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record DragCase(Seq<Guid> ObjectIds, RectangleF Bounds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record ResizeCase(Guid ObjectId, ResizeAxis Axis, RectangleF PreviewBounds, SnappingPolicy Policy = default) : SnapProbe;
    // GH2 has no native rotation-snap primitive; this case encodes angle snap as snapshot magnitude.
    public sealed record RotatedCase(Guid ObjectId, PointF Pivot, double Angle, double Increment = 45.0, double Tolerance = 7.5, SnappingPolicy Policy = default) : SnapProbe;

    public static SnapProbe Drag(Seq<Guid> objectIds, RectangleF bounds, SnappingPolicy policy = default) =>
        new DragCase(ObjectIds: objectIds, Bounds: bounds, Policy: policy);
}

[GenerateUnionOps]
[Union]
public partial record LayoutOp : IUiOp<LayoutResult> {
    private LayoutOp() { }
    public sealed partial record MeasureCase(ObjectScope Scope) : LayoutOp;
    public sealed partial record ArrangeCase(LayoutArrangement Arrangement) : LayoutOp;
    public sealed partial record SnapCase(SnapProbe Probe) : LayoutOp;

    GrasshopperUiIntent<LayoutResult> IUiOp<LayoutResult>.Intent() => Switch(
        measureCase: static measure => Layout.Measure(scope: measure.Scope),
        arrangeCase: static arrange => Layout.Arrange(arrangement: arrange.Arrangement).Map(static delta => (LayoutResult)new LayoutResult.MutationResult(Delta: delta)),
        snapCase: static snap => Layout.Snap(probe: snap.Probe).Map(static result => (LayoutResult)new LayoutResult.SnapResult(Snapshot: result)));
}

[SkipUnionOps]
[Union]
public partial record LayoutResult {
    private LayoutResult() { }
    public sealed record SnapshotsResult(Seq<LayoutSnapshot> Snapshots) : LayoutResult;
    public sealed record MutationResult(Snapshot<LayoutArrangeDelta> Delta) : LayoutResult;
    public sealed record SnapResult(Option<SnappingSnapshot> Snapshot) : LayoutResult;
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Layout {
    // Bounded relaxation budget for Nudge: enough passes for typical selections to settle without an open loop.
    private const int NudgeIterations = 24;

    private readonly record struct SnapRun(SnappingPolicy Policy, Func<GrasshopperUi.Scope, Fin<Option<SnappingSnapshot>>> Run);

    internal static GrasshopperUiIntent<LayoutResult> Measure(ObjectScope scope) =>
        scope.Switch(
            selectionCase: static _ => Selection().Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots)),
            objectsCase: static o => GhUi.Document(run: ctx => o.Ids.TraverseM(id => Snapshot(id: id).Run(scope: ctx))
                .Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots))
                .As()),
            primaryCase: static _ => GhUi.Document(run: _ => ScopeUse.LayoutMeasure.RejectPrimary<LayoutResult>(op: Op.Of(name: nameof(Measure)))),
            primaryAndSecondaryCase: static _ => GhUi.Document(run: _ => ScopeUse.LayoutMeasure.RejectPrimary<LayoutResult>(op: Op.Of(name: nameof(Measure)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Arrange(LayoutArrangement arrangement) =>
        arrangement.Switch(
            moveCase: static move => Move(id: move.Id, dx: move.Dx, dy: move.Dy, snap: move.Policy)
                .Map(static delta => delta.Map(static payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            placeCase: static place => Place(id: place.Id, pivot: place.Pivot, snap: place.Policy)
                .Map(static delta => delta.Map(static payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            alignCase: static align => Align(anchor: align.Anchor, fix: align.Fix, targets: [.. align.Targets]),
            chainCase: static chain => chain.Kind.Arrange(axis: chain.Axis, gap: chain.Gap, ids: chain.Ids, gapPolicy: chain.GapPolicy),
            gridCase: static grid => Grid(
                rows: grid.Rows,
                cols: grid.Cols,
                gap: grid.Gap,
                gapPolicy: grid.GapPolicy,
                ids: [.. grid.Ids]),
            nudgeCase: static nudge => Nudge(
                separation: nudge.Separation,
                snap: nudge.Policy,
                ids: [.. nudge.Ids]));

    internal static GrasshopperUiIntent<LayoutSnapshot> Snapshot(Guid id) =>
        GhUi.Document(run: scope =>
            ObjectOf(scope: scope, id: id).Map(obj => SnapshotOf(attributes: obj.Attributes)));

    internal static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        GhUi.Document(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.SelectedObjects.Select(o => SnapshotOf(attributes: o.Attributes)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Move(Guid id, float dx, float dy, SnappingPolicy snap = default) =>
        GhUi.Document(
            repaint: RepaintRequest.Object(id: id),
            run: scope =>
                from canvas in scope.NeedCanvas()
                from moved in UiRail.RunMutation(
                    scope: scope,
                    op: Op.Of(name: nameof(Move)),
                    mutate: (methods, document, objects, actions) =>
                        from delta in Op.Of(name: nameof(Move)).AcceptPoint(value: new PointF(x: dx, y: dy), detail: "non-finite delta")
                        from obj in UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(Move)))
                        let applied = ApplyMove(
                            obj: obj,
                            document: document,
                            delta: delta,
                            snap: Some(snap),
                            visibleLimit: canvas.VisibleFrame,
                            actions: actions)
                        select (DocumentMutationReceipt.Count(changed: 1), applied))
                from _ in EmitSnapGuide(scope: scope, style: snap.FeedbackStyle, snapshot: moved.Payload.Snap)
                select moved);

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Place(Guid id, PointF pivot, Option<SnappingPolicy> snap = default) =>
        GhUi.Document(
            repaint: RepaintRequest.Object(id: id),
            run: scope =>
                from canvas in scope.NeedCanvas()
                from placed in UiRail.RunMutation(
                    scope: scope,
                    op: Op.Of(name: nameof(Place)),
                    mutate: (methods, document, objects, actions) =>
                        from valid in Op.Of(name: nameof(Place)).AcceptPoint(value: pivot, detail: "non-finite pivot")
                        from obj in UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(Place)))
                        let before = SnapshotOf(attributes: obj.Attributes)
                        let applied = ApplyMove(
                            obj: obj,
                            document: document,
                            delta: new PointF(x: valid.X - before.Pivot.X, y: valid.Y - before.Pivot.Y),
                            snap: snap,
                            visibleLimit: canvas.VisibleFrame,
                            actions: actions)
                        select (DocumentMutationReceipt.Count(changed: 1), applied))
                from _ in EmitSnapGuide(scope: scope, style: snap.Bind(static policy => policy.FeedbackStyle), snapshot: placed.Payload.Snap)
                select placed);

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Align(Guid anchor, OCD.Fixed fix = OCD.Fixed.None, params ReadOnlySpan<Guid> targets) {
        Guid[] snapshot = targets.ToArray();
        return GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunMutation(
                scope: scope,
                op: Op.Of(name: nameof(Align)),
                mutate: (methods, document, objects, actions) =>
                    from validTargets in Optional(snapshot)
                        .Filter(static values => values.Length >= 1)
                        .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Align)), detail: "Align requires an anchor and at least one target"))
                    from anchorObj in UiRail.ResolveObject(objects: objects, id: anchor, op: Op.Of(name: nameof(Align)))
                    let anchorBefore = SnapshotOf(attributes: anchorObj.Attributes)
                    // OCD.MoveParameters may move the anchor, so register its PivotAction before target alignment.
                    from _ in Op.Of(name: nameof(Align)).Attempt(body: () => { actions.Add(new PivotAction(obj: anchorObj)); return unit; }, what: "PivotAction")
                    from targetDeltas in toSeq(validTargets)
                        .TraverseM(target => AlignOne(objects: objects, actions: actions, anchor: anchorObj, target: target, fix: fix)).As()
                        .Map(static perTarget => perTarget.Somes())
                    let anchorDelta = MovedDelta(before: anchorBefore, after: SnapshotOf(attributes: anchorObj.Attributes))
                    let deltas = Seq(anchorDelta).Somes() + targetDeltas
                    select (
                        DocumentMutationReceipt.Count(changed: deltas.Count),
                        new LayoutArrangeDelta(Moves: deltas))));
    }

    // OCD.MoveParameters can move both sides; target receipts stay per-target while anchor movement is folded once.
    private static Fin<Option<LayoutMoveDelta>> AlignOne(GhObjectList objects, ActionList actions, IDocumentObject anchor, Guid target, OCD.Fixed fix) =>
        from targetObj in UiRail.ResolveObject(objects: objects, id: target, op: Op.Of(name: nameof(Align)))
        let targetBefore = SnapshotOf(attributes: targetObj.Attributes)
        from _ in Op.Of(name: nameof(Align)).Attempt(body: () => { actions.Add(new PivotAction(obj: targetObj)); return unit; }, what: "PivotAction")
        from aligned in AlignVia(a: anchor, b: targetObj, fix: fix)
        select MovedDelta(before: targetBefore, after: SnapshotOf(attributes: targetObj.Attributes));

    private static Option<LayoutMoveDelta> MovedDelta(LayoutSnapshot before, LayoutSnapshot after) =>
        before.Pivot != after.Pivot
            ? Some(new LayoutMoveDelta(ObjectId: after.ObjectId, Dx: after.Pivot.X - before.Pivot.X, Dy: after.Pivot.Y - before.Pivot.Y, After: after, Snap: Option<SnappingSnapshot>.None))
            : Option<LayoutMoveDelta>.None;

    // Multi-object arrangements share one undo group; optional snap policy threads visible bounds into each move.
    private static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> ArrangeMoves(string noun, int minimum, Func<GhObjectList, Fin<Seq<(Guid Id, float Dx, float Dy)>>> compute, Option<SnappingPolicy> snap = default) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => {
                RectangleF visibleLimit = snap.Bind(_ => scope.NeedCanvas().Map(static canvas => canvas.VisibleFrame).ToOption()).IfNone(RectangleF.Empty);
                return UiRail.RunMutation(
                    scope: scope,
                    op: Op.Of(name: noun),
                    mutate: (methods, document, objects, actions) =>
                        from moves in Optional(compute)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: noun), detail: "layout compute delegate is required"))
                            .Bind(run => run(arg: objects))
                        from _ in moves.Count >= minimum
                            ? Fin.Succ(value: unit)
                            : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: noun), detail: string.Create(CultureInfo.InvariantCulture, $"fewer than {minimum} supplied ids resolved to document objects")))
                        from deltas in moves.TraverseM(move =>
                            from obj in UiRail.ResolveObject(objects: objects, id: move.Id, op: Op.Of(name: noun))
                            let delta = ApplyMove(
                                obj: obj,
                                document: document,
                                delta: new PointF(x: move.Dx, y: move.Dy),
                                snap: snap,
                                visibleLimit: visibleLimit,
                                actions: actions)
                            select delta).As()
                        select (
                            DocumentMutationReceipt.Count(changed: deltas.Count),
                            new LayoutArrangeDelta(Moves: deltas)))
                    .Bind(committed => EmitSnapGuide(scope: scope, style: snap.Bind(static policy => policy.FeedbackStyle), snapshot: committed.Payload.Moves.Choose(static move => move.Snap).Head)
                        .Map(_ => committed));
            });

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Distribute(LayoutAxis axis, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return ArrangeMoves(
            noun: string.Create(CultureInfo.InvariantCulture, $"Distribute {axis}"),
            minimum: 2,
            compute: objs => ComputeDistribution(objects: objs, ids: toSeq(snapshot), axis: axis, gap: gap, gapPolicy: gapPolicy));
    }

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Grid(int rows, int cols, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return rows >= 1 && cols >= 1 && snapshot.Length >= 2
            ? ArrangeMoves(
                noun: string.Create(CultureInfo.InvariantCulture, $"Grid {rows}x{cols}"),
                minimum: 2,
                compute: objs => snapshot.Length == rows * cols
                    ? ComputeGrid(objects: objs, ids: toSeq(snapshot), rows: rows, cols: cols, gap: gap, gapPolicy: gapPolicy)
                    : Fin.Fail<Seq<(Guid Id, float Dx, float Dy)>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Grid)), detail: string.Create(CultureInfo.InvariantCulture, $"Grid {rows}x{cols} requires exactly {rows * cols} ids"))))
            : GhUi.Document(run: _ => Fin.Fail<Snapshot<LayoutArrangeDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Grid)), detail: "Grid requires rows>=1, cols>=1, and at least 2 ids")));
    }

    // Nudge stays bounded on the UI rail through fixed-iteration pairwise repulsion.
    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Nudge(float separation, SnappingPolicy snap = default, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return ArrangeMoves(
            noun: string.Create(CultureInfo.InvariantCulture, $"Nudge {separation:0.###}"),
            minimum: 2,
            compute: objs => ComputeNudge(objects: objs, ids: toSeq(snapshot), separation: separation),
            snap: snap.Equals(default) ? Option<SnappingPolicy>.None : Some(snap));
    }

    // Cleanup sequences Nudge then fixed-gap Distribute so both move ledgers merge through Bind.
    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Cleanup(Seq<Guid> ids, float separation = 12f, LayoutAxis? axis = null, Option<LayoutGap> gap = default, LayoutGapPolicy? gapPolicy = null) {
        Guid[] snapshot = [.. ids];
        return Nudge(separation: separation, snap: default, ids: snapshot)
            .Bind(nudged => Distribute(
                    axis: axis ?? LayoutAxis.Horizontal,
                    gap: gap.IfNone(() => LayoutGap.Create(value: 24f)),
                    gapPolicy: gapPolicy ?? LayoutGapPolicy.Fixed,
                    ids: snapshot)
                .Map(distributed => distributed.Map(payload => new LayoutArrangeDelta(Moves: nudged.Payload.Moves + payload.Moves))));
    }

    // Flow layers causal ranks, then distributes each rank band along the requested axis.
    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Flow(LayoutAxis axis, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return ArrangeMoves(
            noun: string.Create(CultureInfo.InvariantCulture, $"Flow {axis}"),
            minimum: 2,
            compute: objs => SortCausally(objects: objs, ids: toSeq(snapshot))
                .Bind(ranked => toSeq(ranked
                        .GroupBy(static node => node.Rank)
                        .OrderBy(static band => band.Key)
                        .Select(band => toSeq(band.Select(static node => node.Id))))
                    .TraverseM(layer => ComputeDistribution(objects: objs, ids: layer, axis: axis, gap: gap, gapPolicy: gapPolicy, preordered: true))
                    .As()
                    .Map(static bands => bands.Bind(static moves => moves))));
    }

    private static Fin<Seq<(Guid Id, int Rank)>> SortCausally(GhObjectList objects, Seq<Guid> ids) {
        Connectivity connectivity = objects.Connectivity;
        return ids.TraverseM(id => connectivity.Find(id, out ConnectiveObject? co)
                ? Fin.Succ(co!)
                : Fin.Fail<ConnectiveObject>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SortCausally)), detail: $"object {id} not found in connectivity")))
            .As()
            // Connectivity.SortCausally returns dependency order; the index becomes the rank.
            .Map(nodes => toSeq(connectivity.SortCausally(objects: [.. nodes]).Select((co, i) => (co.Id, Rank: i))));
    }

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document(run: scope =>
            from valid in Optional(probe).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
            let plan = SnapPlan(probe: valid)
            from snapshot in plan.Run(arg: scope)
            from _ in EmitSnapGuide(scope: scope, style: plan.Policy.FeedbackStyle, snapshot: snapshot)
            select snapshot);

    // GH2 object drags draw native SnapXAction/SnapYAction feedback; suppress duplicate cosmetics during drag focus.
    private static bool IsNativeSnapFeedbackActive(Canvas canvas) =>
        canvas.FocusObject is ObjectDragInteraction && (canvas.SnapXAction is not null || canvas.SnapYAction is not null);

    // Best-effort haptic/cosmetic feedback cannot fail the snap rail.
    private static Fin<Unit> EmitSnapGuide(GrasshopperUi.Scope scope, Option<(SnapGuideStyle X, SnapGuideStyle Y)> style, Option<SnappingSnapshot> snapshot) =>
        snapshot is { IsSome: true, Case: SnappingSnapshot snap }
            ? Fin.Succ(value: AlignmentHaptic(scope: scope)).Map(_ =>
                style.Filter(_ => !scope.NeedCanvas().Map(static canvas => IsNativeSnapFeedbackActive(canvas: canvas)).IfFail(_ => false))
                    .Map(styles => Motion.Cosmetic(intent: new CosmeticIntent.SnapGuideCase(Snapshot: snap, Style: styles.X, YStyle: Some(styles.Y))).Run(scope: scope).Ignore())
                    .IfNone(unit))
            : Fin.Succ(value: unit);

    private static Unit AlignmentHaptic(GrasshopperUi.Scope scope) =>
        MotionAccessibility.ShouldReduceMotion
            ? unit
            : Motion.Cosmetic(intent: new CosmeticIntent.HapticCase(Pattern: HapticPattern.Alignment)).Run(scope: scope).Ignore();

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static SnapRun SnapPlan(SnapProbe probe) =>
        probe.Switch(
            pointCase: static point => new SnapRun(
                Policy: point.Policy,
                Run: scope =>
                    from probe in Op.Of(name: nameof(Snap)).AcceptPoint(value: point.Probe, detail: "non-finite probe")
                    from radius in Op.Of(name: nameof(Snap)).AcceptFinite(value: point.Radius, detail: "radius must be finite and positive", requirePositive: true)
                    from snapped in SnapRectangle(scope: scope, id: point.ObjectId, bounds: new RectangleF(x: probe.X - radius, y: probe.Y - radius, width: radius * 2f, height: radius * 2f), policy: point.Policy)
                    select snapped),
            rectangleCase: static rectangle => new SnapRun(
                Policy: rectangle.Policy,
                Run: scope => SnapRectangle(scope: scope, id: rectangle.ObjectId, bounds: rectangle.Bounds, policy: rectangle.Policy)),
            objectCase: static obj => new SnapRun(
                Policy: obj.Policy,
                Run: scope => ObjectOf(scope: scope, id: obj.ObjectId).Bind(target =>
                    from document in scope.NeedDocument()
                    from canvas in scope.NeedCanvas()
                    select SnapCore(document: document, obj: target, policy: obj.Policy, visibleLimit: canvas.VisibleFrame, bounds: Option<RectangleF>.None, wireCandidates: Seq(target)))),
            groupCase: static g => new SnapRun(
                Policy: g.Policy,
                Run: scope =>
                    from document in scope.NeedDocument()
                    from canvas in scope.NeedCanvas()
                    from members in g.ObjectIds.TraverseM(id => UiRail.ResolveObject(objects: document.Objects, id: id, op: Op.Of(name: nameof(Snap)))).As()
                    from snapped in members.IsEmpty
                        ? Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "group snap requires at least one resolvable member"))
                        : Fin.Succ(value: SnapGroup(document: document, members: members, policy: g.Policy, visibleLimit: canvas.VisibleFrame))
                    select snapped),
            dragCase: static drag => new SnapRun(
                Policy: drag.Policy,
                Run: scope =>
                    from document in scope.NeedDocument()
                    from canvas in scope.NeedCanvas()
                    from frame in Op.Of(name: nameof(SnapProbe.Drag)).AcceptRect(value: drag.Bounds, detail: "invalid drag bounds", requirePositive: true)
                    from members in drag.ObjectIds.TraverseM(id => UiRail.ResolveObject(objects: document.Objects, id: id, op: Op.Of(name: nameof(Snap)))).As()
                    from snapped in members.IsEmpty
                        ? Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SnapProbe.Drag)), detail: "drag snap requires at least one member"))
                        : Fin.Succ(value: SnapCore(document: document, obj: members[0], policy: drag.Policy, visibleLimit: canvas.VisibleFrame, bounds: Some(frame), wireCandidates: members, excludeIds: Some(new GuidSet([.. members.Bind(static m => toSeq(m.EntireFamily).Map(static f => f.InstanceId))]))))
                    select snapped),
            resizeCase: static resize => new SnapRun(
                Policy: resize.Policy,
                Run: scope =>
                    from frame in Op.Of(name: nameof(SnapProbe.ResizeCase)).AcceptRect(value: resize.PreviewBounds, detail: "invalid resize preview bounds", requirePositive: true)
                    from snapped in SnapRectangle(scope: scope, id: resize.ObjectId, bounds: frame, policy: resize.Policy)
                        // All-zero resize masks collapse to None so consumers see no snap.
                    select snapped.Bind(s => resize.Axis.Mask(dx: s.Dx, dy: s.Dy) is (float mx, float my) && (mx != 0f || my != 0f)
                        ? Some(s with { Dx = mx, Dy = my, Magnitude = MathF.Sqrt((mx * mx) + (my * my)) })
                        : Option<SnappingSnapshot>.None)),
            rotatedCase: static rotated => new SnapRun(
                Policy: rotated.Policy,
                // GH2 SnappingConstraints is translation-only; rotation snap is pure angle quantisation.
                Run: _ => Fin.Succ(value: SnapAngle(angle: rotated.Angle, increment: rotated.Increment, tolerance: rotated.Tolerance, pivot: rotated.Pivot))));

    private static Fin<Option<SnappingSnapshot>> SnapRectangle(GrasshopperUi.Scope scope, Guid id, RectangleF bounds, SnappingPolicy policy) =>
        Op.Of(name: nameof(SnapRectangle)).AcceptRect(value: bounds, detail: "invalid rectangle probe", requirePositive: true)
            .Bind(valid => ObjectOf(scope: scope, id: id).Bind(obj =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                select SnapRectangle(document: document, obj: obj, bounds: valid, policy: policy, visibleLimit: canvas.VisibleFrame)));

    private static Option<SnappingSnapshot> SnapRectangle(GhDocument document, IDocumentObject obj, RectangleF bounds, SnappingPolicy policy, RectangleF visibleLimit) =>
        SnapCore(document: document, obj: obj, policy: policy, visibleLimit: visibleLimit, bounds: Some(bounds), wireCandidates: Seq(obj));

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        UiRail.ResolveObject(scope: scope, id: id, op: Op.Of(name: nameof(ObjectOf)));

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, PointF delta, Option<SnappingPolicy> snap, RectangleF visibleLimit, ActionList actions) {
        actions.Add(new PivotAction(obj: obj));
        IAttributes attributes = obj.Attributes;
        attributes.Layout(shape: Grasshopper2.UI.Skinning.Shape.Default);
        RectangleF bounds = snap.Map(policy => TargetBounds(attributes: attributes, policy: policy)).IfNone(attributes.AggregateBounds);
        Option<SnappingSnapshot> snapped = snap.Bind(policy => attributes.Snappable
            ? SnapRectangle(
                document: document, obj: obj,
                bounds: new RectangleF(x: bounds.X + delta.X, y: bounds.Y + delta.Y, width: bounds.Width, height: bounds.Height),
                policy: policy,
                visibleLimit: visibleLimit)
            : Option<SnappingSnapshot>.None);
        (float deltaDx, float deltaDy) = snapped.Map(static s => (s.Dx, s.Dy)).IfNone((0f, 0f));
        float effDx = delta.X + deltaDx;
        float effDy = delta.Y + deltaDy;
        attributes.Move(dx: effDx, dy: effDy);
        return new LayoutMoveDelta(
            ObjectId: attributes.Owner.InstanceId,
            Dx: effDx, Dy: effDy,
            After: SnapshotOf(attributes: attributes),
            Snap: snapped);
    }

    private static RectangleF TargetBounds(IAttributes attributes, SnappingPolicy policy) =>
        policy.UseAggregateWireBounds ? attributes.AggregateBounds : attributes.Bounds;

    // Group snap uses union bounds and all member wire families, not only members[0].
    private static Option<SnappingSnapshot> SnapGroup(GhDocument document, Seq<IDocumentObject> members, SnappingPolicy policy, RectangleF visibleLimit) =>
        SnapCore(
            document: document,
            obj: members[0],
            policy: policy,
            visibleLimit: visibleLimit,
            bounds: Some(members.Tail.Fold(members[0].Attributes.AggregateBounds, static (acc, m) => RectangleF.Union(rect1: acc, rect2: m.Attributes.AggregateBounds))),
            wireCandidates: members,
            excludeIds: Some(new GuidSet([.. members.Bind(static m => toSeq(m.EntireFamily).Map(static f => f.InstanceId))])));

    internal static Option<SnappingSnapshot> SnapCore(
        GhDocument document,
        IDocumentObject obj,
        SnappingPolicy policy,
        RectangleF visibleLimit,
        Option<RectangleF> bounds,
        Seq<IDocumentObject> wireCandidates = default,
        Option<GuidSet> excludeIds = default) {
        // Exclude the whole component family so multi-part components never snap against their own siblings.
        GuidSet filter = excludeIds.IfNone(() => new GuidSet([.. obj.EntireFamily.Select(static m => m.InstanceId)]));
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: policy.IncludeSelected,
            includeUnselected: policy.IncludeUnselected,
            filter: filter);
        // Bypass SnapObject because it always uses AggregateBounds; policy may request tight Bounds.
        RectangleF wireBounds = bounds.IfNone(policy.UseAggregateWireBounds ? obj.Attributes.AggregateBounds : obj.Attributes.Bounds);
        constraints.SnapRectangle(target: wireBounds, settings: policy.Native, visibleLimit: visibleLimit, snapX: out SnappingAction x, snapY: out SnappingAction y);
        int radius = policy.Native.EdgeRadius;
        // Fold explicit and geometry-derived SnapSpaces through one smaller-magnitude per-axis projection.
        Seq<SnapSpace> spaces = policy.Spaces + policy.GeometrySpace(objects: document.Objects, excludeIds: filter).ToSeq();
        (SnappingAction? spaceX, SnappingAction? spaceY) = spaces.Fold(
            ((SnappingAction?)default, (SnappingAction?)default),
            (acc, space) => SnapActions(target: wireBounds, space: space, radius: radius) is var (sx, sy)
                ? (SnappingAction.SmallerMagnitude(a: acc.Item1, b: sx), SnappingAction.SmallerMagnitude(a: acc.Item2, b: sy))
                : acc);
        // Multi-member probes fold every candidate's wire family into the Y channel.
        SnappingAction wireY = wireCandidates.Fold(y, (acc, m) =>
            SnappingAction.SmallerMagnitude(a: acc, b: SnappingConstraints.SnapWires(target: m, boundsOverride: wireBounds, settings: policy.Native)));
        Seq<LineF> gapGuides = policy.SmartGap.Map(tolerance => EquidistanceGuides(document: document, anchor: wireBounds, excludeIds: filter, tolerance: tolerance)).IfNone([]);
        return UiRail.SnapChannels(
                x: Optional(SnappingAction.SmallerMagnitude(a: x, b: spaceX)),
                y: Optional(SnappingAction.SmallerMagnitude(a: wireY, b: spaceY)))
            .Map(snapshot => gapGuides.IsEmpty ? snapshot : snapshot with { Lines = snapshot.Lines + gapGuides });
    }

    // BOUNDARY ADAPTER -- SnapSpace.Snap is a void out-param API; rebuild per-axis actions from snapped coordinates.
    private static (SnappingAction? X, SnappingAction? Y) SnapActions(RectangleF target, SnapSpace space, int radius) {
        space.Snap(target.Center.X, target.Center.Y, radius, out double lineX, out double lineY, out string description);
        float deltaX = (float)lineX - target.Center.X;
        float deltaY = (float)lineY - target.Center.Y;
        string label = description.Length > 0 ? description : "snap";
        SnappingAction? snapX = float.IsFinite(deltaX) && MathF.Abs(x: deltaX) <= radius && deltaX != 0f
            ? new SnappingAction(dx: deltaX, dy: 0f, text: label, point: new PointF(x: (float)lineX, y: target.Top - radius), anchor: TextAnchor.LowerMiddle, lines: [new LineF((float)lineX, target.Top - radius, (float)lineX, target.Bottom + radius)])
            : default;
        SnappingAction? snapY = float.IsFinite(deltaY) && MathF.Abs(x: deltaY) <= radius && deltaY != 0f
            ? new SnappingAction(dx: 0f, dy: deltaY, text: label, point: new PointF(x: target.Left - radius, y: (float)lineY), anchor: TextAnchor.CentreRight, lines: [new LineF(target.Left - radius, (float)lineY, target.Right + radius, (float)lineY)])
            : default;
        return (snapX, snapY);
    }

    // Equal-spacing feedback emits short tick guides only when all adjacent gaps match within tolerance.
    private static Seq<LineF> EquidistanceGuides(GhDocument document, RectangleF anchor, GuidSet excludeIds, int tolerance) {
        Seq<float> centres = toSeq(document.Objects.Forwards)
            .Filter(o => !excludeIds.Contains(o.InstanceId))
            .Map(static o => o.Attributes.AggregateBounds.Center.X) + Seq(anchor.Center.X);
        Seq<float> centresX = toSeq(centres.Distinct().Order());
        Seq<float> gaps = centresX.Zip(centresX.Tail).Map(static pair => pair.Second - pair.First);
        return gaps.Count >= 2 && gaps.Tail.ForAll(g => MathF.Abs(g - gaps.Head.IfNone(0f)) <= tolerance)
            ? centresX.Map(cx => new LineF(cx, anchor.Top - tolerance, cx, anchor.Top))
            : [];
    }

    // Rotation-only snap: signed angle delta lives in Magnitude; translation channels stay zero.
    private static Option<SnappingSnapshot> SnapAngle(double angle, double increment, double tolerance, PointF pivot) =>
        increment > 0.0 && (Math.Round(angle / increment, MidpointRounding.AwayFromZero) * increment) is double nearest && Math.Abs(nearest - angle) <= tolerance && nearest != angle
            ? Some(new SnappingSnapshot(
                Dx: 0f, Dy: 0f,
                Magnitude: (float)(nearest - angle),
                XLabel: Some(new SnapLabel(Text: string.Create(CultureInfo.InvariantCulture, $"{nearest:0.#}°"), Point: pivot, Anchor: Some(TextAnchor.LowerMiddle))),
                YLabel: Option<SnapLabel>.None,
                Lines: []))
            : Option<SnappingSnapshot>.None;

    // OCD.AlignObjects exposes exactly four (Component|IParameter)^2 overloads; unsupported pairs stay explicit.
    private static Fin<Unit> AlignVia(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) {
        Op op = Op.Of(name: nameof(AlignVia));
        Option<SysAction> overload = (a, b) switch {
            (Component left, Component right) => Some(() => OCD.AlignObjects(left, right, fix)),
            (Component left, IParameter right) => Some(() => OCD.AlignObjects(left, right, fix)),
            (IParameter left, Component right) => Some(() => OCD.AlignObjects(left, right, fix)),
            (IParameter left, IParameter right) => Some(() => OCD.AlignObjects(left, right, fix)),
            _ => Option<SysAction>.None,
        };
        return overload is { IsSome: true, Case: SysAction align }
            ? op.Attempt(body: () => { align(); return unit; }, what: "OCD.AlignObjects")
            : Fin.Fail<Unit>(error: UiFault.MutationRejected(
                op: op,
                detail: $"OCD.AlignObjects unsupported pair ({a.GetType().Name}, {b.GetType().Name})"));
    }

    private static Fin<Seq<(Guid Id, float Dx, float Dy)>> ComputeDistribution(
        GhObjectList objects,
        Seq<Guid> ids,
        LayoutAxis axis,
        LayoutGap gap,
        LayoutGapPolicy gapPolicy,
        bool preordered = false) {
        return ids.TraverseM(id => UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(ComputeDistribution)))
                .Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)))
            .As()
            .Map(resolved => {
                Seq<(Guid Id, RectangleF Bounds)> sorted = preordered
                    ? resolved
                    : toSeq(resolved.OrderBy(s => axis.Origin(bounds: s.Bounds)).ThenBy(s => s.Id));
                return sorted.Count switch {
                    < 2 => Seq<(Guid Id, float Dx, float Dy)>(),
                    _ => DistributeMode.Of(gapPolicy: gapPolicy, sorted: sorted, axis: axis, gap: gap.Value)
                        .Compute(sorted: sorted, axis: axis, gap: gap.Value),
                };
            });
    }

    // Grid derives column-left and row-top targets from per-axis distribution, then emits normal Move deltas.
    private static Fin<Seq<(Guid Id, float Dx, float Dy)>> ComputeGrid(
        GhObjectList objects,
        Seq<Guid> ids,
        int rows,
        int cols,
        LayoutGap gap,
        LayoutGapPolicy gapPolicy) {
        return ids.TraverseM(id => UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(ComputeGrid)))
                .Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)))
            .As()
            .Bind(resolvedAll => {
                Seq<(Guid Id, RectangleF Bounds)> resolved = toSeq(resolvedAll
                        .OrderBy(static s => s.Bounds.Top)
                        .ThenBy(static s => s.Bounds.Left))
                    .Take(Math.Max(0, rows * cols));
                int count = resolved.Count;
                int rowCount = count > 0 ? ((count - 1) / cols) + 1 : 0;
                HashMap<Guid, RectangleF> bounds = toHashMap(resolved.Map(static item => (item.Id, item.Bounds)));

                Fin<HashMap<Guid, float>> AxisTargets(LayoutAxis axis, Func<RectangleF, float> origin, Seq<Guid> repIds) =>
                    ComputeDistribution(objects: objects, ids: repIds, axis: axis, gap: gap, gapPolicy: gapPolicy)
                        .Map(moves => toHashMap(moves.Choose(move => bounds.Find(move.Id).Map(b => (move.Id, origin(b) + axis.Offset(dx: move.Dx, dy: move.Dy))))));

                return from colLeft in AxisTargets(axis: LayoutAxis.Horizontal, origin: static b => b.Left, repIds: resolved.Take(cols).Map(static item => item.Id))
                       from rowTop in AxisTargets(axis: LayoutAxis.Vertical, origin: static b => b.Top, repIds: toSeq(Enumerable.Range(start: 0, count: rowCount).Select(row => resolved[row * cols].Id)))
                       select toSeq(Enumerable.Range(start: 0, count: count).Select(index => {
                           (Guid id, RectangleF box) = resolved[index];
                           float left = colLeft.Find(resolved[index % cols].Id).IfNone(box.Left);
                           float top = rowTop.Find(resolved[index / cols * cols].Id).IfNone(box.Top);
                           return (id, left - box.Left, top - box.Top);
                       }));
            });
    }

    // Pairwise repulsion pushes overlapping centres apart by half the deficit across a bounded schedule.
    private static Fin<Seq<(Guid Id, float Dx, float Dy)>> ComputeNudge(GhObjectList objects, Seq<Guid> ids, float separation) =>
        Op.Of(name: nameof(ComputeNudge)).AcceptFinite(value: separation, detail: "separation must be finite and positive", requirePositive: true)
            .Bind(_ => ids.TraverseM(id => UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(ComputeNudge)))
                    .Map(o => (Id: id, Centre: o.Attributes.AggregateBounds.Center)))
                .As())
            .Map(resolved => {
                Seq<(Guid A, Guid B)> pairs = toSeq(
                    from i in Enumerable.Range(start: 0, count: resolved.Count)
                    from j in Enumerable.Range(start: i + 1, count: resolved.Count - (i + 1))
                    select (A: resolved[i].Id, B: resolved[j].Id));
                HashMap<Guid, PointF> relaxed = toSeq(Enumerable.Range(start: 0, count: NudgeIterations)).Fold(
                    toHashMap(resolved.Map(static s => (s.Id, s.Centre))),
                    (positions, _) => pairs.Fold(positions, (acc, pair) => RepelPair(positions: acc, a: pair.A, b: pair.B, separation: separation)));
                return resolved.Choose(s => relaxed.Find(s.Id)
                    .Map(centre => (s.Id, Dx: centre.X - s.Centre.X, Dy: centre.Y - s.Centre.Y))
                    .Filter(static move => move.Dx != 0f || move.Dy != 0f));
            });

    // Coincident centres fall back to +X so overlap splitting stays deterministic.
    private static HashMap<Guid, PointF> RepelPair(HashMap<Guid, PointF> positions, Guid a, Guid b, float separation) =>
        (positions.Find(a), positions.Find(b)) switch {
            ( { IsSome: true, Case: PointF pa }, { IsSome: true, Case: PointF pb })
                when MathF.Sqrt(x: ((pb.X - pa.X) * (pb.X - pa.X)) + ((pb.Y - pa.Y) * (pb.Y - pa.Y))) is float distance && distance < separation
                    && ((distance > 1e-4f ? (pb.X - pa.X) / distance : 1f) * (separation - distance) * 0.5f) is float ox
                    && ((distance > 1e-4f ? (pb.Y - pa.Y) / distance : 0f) * (separation - distance) * 0.5f) is float oy =>
                positions
                    .AddOrUpdate(a, new PointF(x: pa.X - ox, y: pa.Y - oy))
                    .AddOrUpdate(b, new PointF(x: pb.X + ox, y: pb.Y + oy)),
            _ => positions,
        };

    [SmartEnum<int>]
    private sealed partial class DistributeMode {
        public static readonly DistributeMode Cursor = new(key: 0, compute: ComputeCursor);
        public static readonly DistributeMode Stretch = new(key: 1, compute: ComputeStretch);

        [UseDelegateFromConstructor]
        internal partial Seq<(Guid Id, float Dx, float Dy)> Compute(
            Seq<(Guid Id, RectangleF Bounds)> sorted,
            LayoutAxis axis,
            float gap);

        internal static DistributeMode Of(LayoutGapPolicy gapPolicy, Seq<(Guid Id, RectangleF Bounds)> sorted, LayoutAxis axis, float gap) {
            int count = sorted.Count;
            float extent = axis.Origin(bounds: sorted[count - 1].Bounds) + axis.Span(bounds: sorted[count - 1].Bounds) - axis.Origin(bounds: sorted[0].Bounds);
            float content = sorted.Fold(0f, (sum, item) => sum + axis.Span(bounds: item.Bounds)) + ((count - 1) * gap);
            return gapPolicy.Equals(LayoutGapPolicy.Fixed) || extent <= content + gapPolicy.ContentSlack ? Cursor : Stretch;
        }

        private static Seq<(Guid Id, float Dx, float Dy)> ComputeCursor(
            Seq<(Guid Id, RectangleF Bounds)> sorted,
            LayoutAxis axis,
            float gap) {
            float cursor = axis.Origin(bounds: sorted[0].Bounds);
            return sorted
                .Fold(
                    (Moves: Seq<(Guid Id, float Dx, float Dy)>(), Cursor: cursor, Index: 0),
                    (state, item) => {
                        (float dx, float dy) = axis.Delta(cursor: state.Cursor, bounds: item.Bounds);
                        float nextCursor = state.Cursor + axis.Span(bounds: item.Bounds) + (state.Index < sorted.Count - 1 ? gap : 0f);
                        return (
                            Moves: state.Moves.Add((item.Id, dx, dy)),
                            Cursor: nextCursor,
                            Index: state.Index + 1);
                    })
                .Moves;
        }

        // StretchLayoutSolver interleaves fixed object spans with stretchable gaps, keyed by SpanSlot/GapSlot.
        private static int SpanSlot(int i) => i * 2;
        private static int GapSlot(int i) => (i * 2) + 1;

        [BoundaryAdapter]
        private static Seq<(Guid Id, float Dx, float Dy)> ComputeStretch(
            Seq<(Guid Id, RectangleF Bounds)> sorted,
            LayoutAxis axis,
            float gap) {
            int count = sorted.Count;
            float firstOrigin = axis.Origin(bounds: sorted[0].Bounds);
            float lastEnd = axis.Origin(bounds: sorted[count - 1].Bounds) + axis.Span(bounds: sorted[count - 1].Bounds);
            StretchLayoutSolver solver = new();
            // BOUNDARY ADAPTER -- StretchLayoutSolver requires imperative Add/Solve/Round.
            for (int index = 0; index < count; index++) {
                float span = axis.Span(bounds: sorted[index].Bounds);
                solver.Add(min: span, max: span, ideal: span);
                if (index < count - 1) {
                    solver.Add(min: gap, max: float.MaxValue, ideal: gap);
                }
            }
            _ = solver.Solve(target: lastEnd - firstOrigin);
            solver.Round();
            return toSeq(Enumerable.Range(start: 0, count: count)).Fold(
                (Moves: Seq<(Guid Id, float Dx, float Dy)>(), Cursor: firstOrigin),
                (state, index) => {
                    (Guid id, RectangleF bounds) = sorted[index];
                    (float dx, float dy) = axis.Delta(cursor: state.Cursor, bounds: bounds);
                    float advance = solver[SpanSlot(i: index)] + (index < count - 1 ? solver[GapSlot(i: index)] : 0f);
                    return (Moves: state.Moves.Add((id, dx, dy)), Cursor: state.Cursor + advance);
                }).Moves;
        }
    }
}

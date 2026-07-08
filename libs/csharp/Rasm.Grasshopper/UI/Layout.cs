using System.Globalization;
using Rasm.Csp;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Snap;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GuidSet = System.Collections.Generic.HashSet<System.Guid>;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LayoutAxis {
    public static readonly LayoutAxis Horizontal = new(
        key: 0,
        origin: static bounds => bounds.Left,
        span: static bounds => bounds.Width,
        delta: static (cursor, bounds) => (Dx: cursor - bounds.Left, Dy: 0f),
        offset: static (dx, _) => dx,
        crossExtent: static frame => frame.Height,
        tick: static (centre, anchor, len) => new LineF(centre, anchor.Top - len, centre, anchor.Top));

    public static readonly LayoutAxis Vertical = new(
        key: 1,
        origin: static bounds => bounds.Top,
        span: static bounds => bounds.Height,
        delta: static (cursor, bounds) => (Dx: 0f, Dy: cursor - bounds.Top),
        offset: static (_, dy) => dy,
        crossExtent: static frame => frame.Width,
        tick: static (centre, anchor, len) => new LineF(anchor.Left - len, centre, anchor.Left, centre));

    [UseDelegateFromConstructor]
    internal partial float Origin(RectangleF bounds);

    [UseDelegateFromConstructor]
    internal partial float Span(RectangleF bounds);

    [UseDelegateFromConstructor]
    internal partial (float Dx, float Dy) Delta(float cursor, RectangleF bounds);

    [UseDelegateFromConstructor]
    internal partial float Offset(float dx, float dy);

    // CrossExtent selects the viewport dimension orthogonal to the axis for tick-length scaling; Tick builds the guide
    // segment anchored to the probe rectangle.
    [UseDelegateFromConstructor]
    internal partial float CrossExtent(RectangleF frame);

    [UseDelegateFromConstructor]
    internal partial LineF Tick(float centre, RectangleF anchor, float length);
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
    public static readonly LayoutGapPolicy Stretch = new(key: 0, contentSlack: 1e-4f, forcePack: false);
    public static readonly LayoutGapPolicy Fixed = new(key: 1, contentSlack: 0f, forcePack: true);
    // Sub-pixel slack keeps marginal overflows on the fixed cursor before the stretch solver takes over; ForcePack forces
    // the cursor pack regardless of measured extent.
    internal float ContentSlack { get; }

    internal bool ForcePack { get; }
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

[SmartEnum<int>]
public sealed partial class LayoutChainKind {
    public static readonly LayoutChainKind Distribute = new(key: 0, arrange: static (axis, gap, ids, gapPolicy) => Layout.Distribute(axis: axis, gap: gap, gapPolicy: gapPolicy, ids: [.. ids]));
    public static readonly LayoutChainKind Flow = new(key: 1, arrange: static (axis, gap, ids, gapPolicy) => Layout.Flow(axis: axis, gap: gap, gapPolicy: gapPolicy, ids: [.. ids]));

    [UseDelegateFromConstructor]
    internal partial GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Arrange(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy);
}

[SkipUnionOps]
[Union]
public partial record LayoutArrangement {
    private LayoutArrangement() { }
    public sealed record MoveCase(Guid Id, float Dx, float Dy, SnappingPolicy Policy = default) : LayoutArrangement;
    public sealed record PlaceCase(Guid Id, PointF Pivot, Option<SnappingPolicy> Policy = default) : LayoutArrangement;
    public sealed record AlignCase(Guid Anchor, Seq<Guid> Targets, OCD.Fixed Fix) : LayoutArrangement;
    // AlignWires straightens connecting wires: the geometry source is wire endpoints, not object bounds.
    public sealed record AlignWiresCase(Seq<Guid> Ids, SnappingPolicy Policy = default) : LayoutArrangement;
    public sealed record ChainCase(LayoutChainKind Kind, LayoutAxis Axis, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record GridCase(int Rows, int Cols, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record NudgeCase(Seq<Guid> Ids, float Separation, SnappingPolicy Policy = default) : LayoutArrangement;

    public static LayoutArrangement Align(Guid anchor, Seq<Guid> targets, OCD.Fixed fix = OCD.Fixed.None) =>
        new AlignCase(Anchor: anchor, Targets: targets, Fix: fix);

    public static LayoutArrangement AlignWires(Seq<Guid> ids, SnappingPolicy policy = default) =>
        new AlignWiresCase(Ids: ids, Policy: policy);

    public static LayoutArrangement Distribute(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new ChainCase(Kind: LayoutChainKind.Distribute, Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Stretch);

    public static LayoutArrangement Flow(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new ChainCase(Kind: LayoutChainKind.Flow, Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Stretch);

    public static LayoutArrangement Grid(int rows, int cols, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy) =>
        new GridCase(Rows: rows, Cols: cols, Gap: gap, Ids: ids, GapPolicy: gapPolicy);

    public static LayoutArrangement Nudge(Seq<Guid> ids, float separation, SnappingPolicy policy = default) =>
        new NudgeCase(Ids: ids, Separation: separation, Policy: policy);
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
    public sealed record SmartGapCase(bool Enabled, int Tolerance = 2, Option<LayoutAxis> Axis = default) : SnapSetting;
    public sealed record WireBoundsCase(bool Aggregate = true) : SnapSetting;

    public static SnapSetting Grid(SizeF cell, PointF origin = default) =>
        new SpaceCase(Space: SnapSpace.CreateOrthogonal(originX: origin.X, originY: origin.Y, sizeX: cell.Width, sizeY: cell.Height));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(bool IncludeSelected = true, bool IncludeUnselected = true, Seq<SnapSetting> Settings = default) {
    // One fold lowers all settings into a PolicyDecode carrier so the O(N) settings scan runs once per operation.
    // FeedbackStyle defaults YStyle to XStyle; UseAggregateWireBounds defaults to true when unset.
    internal PolicyDecode Decode() =>
        Settings.Fold(
            new PolicyDecode(Native: SnappingSettings.Current, FeedbackStyle: default, Spaces: Seq<SnapSpace>(), Geometry: default, SmartGap: default, UseAggregateWireBounds: true, IncludeSelected: IncludeSelected, IncludeUnselected: IncludeUnselected),
            static (acc, op) => op switch {
                SnapSetting.WithRuleCase rule => acc with { Native = acc.Native.WithRules(rules: rule.Rule) },
                SnapSetting.WithoutRuleCase rule => acc with { Native = acc.Native.WithoutRules(rules: rule.Rule) },
                SnapSetting.FeedbackCase feedback => acc with {
                    Native = acc.Native.WithFeedback(drawFeedback: feedback.Enabled, colour: feedback.XStyle.Tint),
                    FeedbackStyle = acc.FeedbackStyle.IsSome ? acc.FeedbackStyle : (feedback.Enabled ? Some((feedback.XStyle, feedback.YStyle.Equals(default) ? feedback.XStyle : feedback.YStyle)) : acc.FeedbackStyle),
                },
                SnapSetting.RadiiCase radii => acc with { Native = new SnappingSettings(rules: acc.Native.Rules, verticalGap: radii.VerticalGapSize.Map(static value => Math.Max(0, value)).IfNone(acc.Native.VerticalGapSize), horizontalGap: radii.HorizontalGapSize.Map(static value => Math.Max(0, value)).IfNone(acc.Native.HorizontalGapSize), edgeRadius: radii.EdgeRadius.Map(static value => Math.Max(0, value)).IfNone(acc.Native.EdgeRadius), wireRadius: radii.WireRadius.Map(static value => Math.Max(0, value)).IfNone(acc.Native.WireRadius), feedback: acc.Native.Feedback, colour: acc.Native.Colour) },
                SnapSetting.SpaceCase space => acc with { Spaces = acc.Spaces.Add(space.Space) },
                SnapSetting.GeometryCase geo => acc with { Geometry = acc.Geometry.IsSome ? acc.Geometry : Some((geo.Projection, geo.Radius)) },
                SnapSetting.SmartGapCase { Enabled: true } gap => acc with { SmartGap = acc.SmartGap.IsSome ? acc.SmartGap : Some(gap) },
                SnapSetting.WireBoundsCase wire => acc with { UseAggregateWireBounds = wire.Aggregate },
                _ => acc,
            });
}

// Single-pass projection of a SnappingPolicy's settings; consumed by Measure, SnapCore, and the guide read.
[StructLayout(LayoutKind.Auto)]
internal readonly record struct PolicyDecode(
    SnappingSettings Native,
    Option<(SnapGuideStyle X, SnapGuideStyle Y)> FeedbackStyle,
    Seq<SnapSpace> Spaces,
    Option<(SnapProjection Projection, double Radius)> Geometry,
    Option<SnapSetting.SmartGapCase> SmartGap,
    bool UseAggregateWireBounds,
    bool IncludeSelected,
    bool IncludeUnselected);

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
    private readonly record struct SnapRun(PolicyDecode Decoded, Func<GrasshopperUi.Scope, Fin<Option<SnappingSnapshot>>> Run);

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
            alignWiresCase: static alignWires => AlignWires(snap: alignWires.Policy, ids: [.. alignWires.Ids]),
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
            repaint: RepaintRequest.Batch(requests: Seq(RepaintRequest.Object(id: id))),
            run: scope => {
                PolicyDecode decoded = snap.Decode();
                return
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
                                snap: Some(decoded),
                                visibleLimit: canvas.VisibleFrame,
                                actions: actions)
                            select (DocumentMutationReceipt.Count(changed: 1), applied))
                    from _ in EmitSnapGuide(scope: scope, style: decoded.FeedbackStyle, snapshot: moved.Payload.Snap)
                    select moved;
            });

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Place(Guid id, PointF pivot, Option<SnappingPolicy> snap = default) =>
        GhUi.Document(
            repaint: RepaintRequest.Batch(requests: Seq(RepaintRequest.Object(id: id))),
            run: scope => {
                Option<PolicyDecode> decoded = snap.Map(static policy => policy.Decode());
                return
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
                                snap: decoded,
                                visibleLimit: canvas.VisibleFrame,
                                actions: actions)
                            select (DocumentMutationReceipt.Count(changed: 1), applied))
                    from _ in EmitSnapGuide(scope: scope, style: decoded.Bind(static d => d.FeedbackStyle), snapshot: placed.Payload.Snap)
                    select placed;
            });

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

    // OCD.AlignObjects exposes four (Component|IParameter)^2 overloads bound by static resolution; dispatch on the left
    // operand's alignable shape yields a closure that dispatches the right, so the 2x2 collapses to left-then-right
    // polymorphic dispatch — one rejection rail at each axis, one fenced native seam, never a flat four-tuple of arms.
    private static Fin<Unit> AlignVia(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) =>
        (a switch {
            Component left => b switch {
                Component right => Optional(() => OCD.AlignObjects(left, right, fix)),
                IParameter right => Optional(() => OCD.AlignObjects(left, right, fix)),
                _ => Option<System.Action>.None,
            },
            IParameter left => b switch {
                Component right => Optional(() => OCD.AlignObjects(left, right, fix)),
                IParameter right => Optional(() => OCD.AlignObjects(left, right, fix)),
                _ => Option<System.Action>.None,
            },
            _ => Option<System.Action>.None,
        })
        .Map(align => Op.Of(name: nameof(AlignVia)).Attempt(body: align, what: "OCD.AlignObjects"))
        .IfNone(() => Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(AlignVia)), detail: $"OCD.AlignObjects unsupported pair ({a.GetType().Name}, {b.GetType().Name})")));

    private static Option<LayoutMoveDelta> MovedDelta(LayoutSnapshot before, LayoutSnapshot after) =>
        before.Pivot != after.Pivot
            ? Some(new LayoutMoveDelta(ObjectId: after.ObjectId, Dx: after.Pivot.X - before.Pivot.X, Dy: after.Pivot.Y - before.Pivot.Y, After: after, Snap: Option<SnappingSnapshot>.None))
            : Option<LayoutMoveDelta>.None;

    // Multi-object arrangements share one undo group; optional snap policy threads visible bounds into each move.
    private static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> ArrangeMoves(string noun, int minimum, Func<GhObjectList, Fin<Seq<(Guid Id, float Dx, float Dy)>>> compute, Option<SnappingPolicy> snap = default) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => {
                Option<PolicyDecode> decoded = snap.Map(static policy => policy.Decode());
                RectangleF visibleLimit = decoded.Bind(_ => scope.NeedCanvas().Map(static canvas => canvas.VisibleFrame).ToOption()).IfNone(RectangleF.Empty);
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
                                snap: decoded,
                                visibleLimit: visibleLimit,
                                actions: actions)
                            select delta).As()
                        select (
                            DocumentMutationReceipt.Count(changed: deltas.Count),
                            new LayoutArrangeDelta(Moves: deltas)))
                    .Bind(committed => EmitSnapGuide(scope: scope, style: decoded.Bind(static d => d.FeedbackStyle), snapshot: committed.Payload.Moves.Choose(static move => move.Snap).Head)
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

    // AlignWires straightens connections inside the selection: each object slides on Y to its first predecessor's wire
    // endpoint. The geometry source is Wire.AllWireEnds, not object bounds — the invariant that separates it from Align.
    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> AlignWires(SnappingPolicy snap = default, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return ArrangeMoves(
            noun: nameof(AlignWires),
            minimum: 1,
            compute: objs => ComputeWireAlignment(objects: objs, ids: toSeq(snapshot)),
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
                    .TraverseM(layer => ComputeDistribution(objects: objs, ids: layer, axis: axis, gap: gap, gapPolicy: gapPolicy))
                    .As()
                    .Map(static bands => bands.Bind(static moves => moves))));
    }

    private static Fin<Seq<(Guid Id, int Rank)>> SortCausally(GhObjectList objects, Seq<Guid> ids) {
        Connectivity connectivity = objects.Connectivity;
        return ids.TraverseM(id => connectivity.Find(id, out ConnectiveObject? co)
                ? Fin.Succ(co!)
                : Fin.Fail<ConnectiveObject>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SortCausally)), detail: $"object {id} not found in connectivity")))
            .As()
            // Rank = longest-path depth over the in-selection predecessors (co.In intersected with the selection), so
            // parallel nodes share a band; a topological index would make every rank unique, leaving each band a singleton.
            .Map(nodes => {
                GuidSet idSet = [.. ids];
                Seq<ConnectiveObject> sorted = toSeq(connectivity.SortCausally(objects: [.. nodes]));
                HashMap<Guid, int> ranks = sorted.Fold(HashMap<Guid, int>(), (acc, co) => {
                    int depth = toSeq(co.In).Filter(idSet.Contains).Fold(0, (m, pred) => Math.Max(m, acc.Find(pred).IfNone(0) + 1));
                    return acc.AddOrUpdate(co.Id, depth);
                });
                return toSeq(sorted.Select(co => (co.Id, Rank: ranks.Find(co.Id).IfNone(0))));
            });
    }

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document(run: scope =>
            from valid in Optional(probe).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
            let plan = SnapPlan(probe: valid)
            from snapshot in plan.Run(arg: scope)
            from _ in EmitSnapGuide(scope: scope, style: plan.Decoded.FeedbackStyle, snapshot: snapshot)
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
        AccessibilityMotionGate.ShouldReduceMotion
            ? unit
            : Motion.Cosmetic(intent: new CosmeticIntent.HapticCase(Pattern: HapticPattern.Alignment)).Run(scope: scope).Ignore();

    // Decodes the policy exactly once per probe and threads the carrier through the per-case Run builder, so the settings
    // fold never repeats and FeedbackStyle reaches the guide-emission site.
    private static SnapRun Plan(SnappingPolicy policy, Func<PolicyDecode, Func<GrasshopperUi.Scope, Fin<Option<SnappingSnapshot>>>> build) {
        PolicyDecode decoded = policy.Decode();
        return new SnapRun(Decoded: decoded, Run: build(arg: decoded));
    }

    private static SnapRun SnapPlan(SnapProbe probe) =>
        probe.Switch(
            pointCase: static point => Plan(policy: point.Policy, build: decoded => scope =>
                from p in Op.Of(name: nameof(Snap)).AcceptPoint(value: point.Probe, detail: "non-finite probe")
                from radius in Op.Of(name: nameof(Snap)).AcceptFinite(value: point.Radius, detail: "radius must be finite and positive", requirePositive: true)
                from snapped in SnapRectangle(scope: scope, id: point.ObjectId, bounds: new RectangleF(x: p.X - radius, y: p.Y - radius, width: radius * 2f, height: radius * 2f), decoded: decoded)
                select snapped),
            rectangleCase: static rectangle => Plan(policy: rectangle.Policy, build: decoded => scope =>
                SnapRectangle(scope: scope, id: rectangle.ObjectId, bounds: rectangle.Bounds, decoded: decoded)),
            objectCase: static obj => Plan(policy: obj.Policy, build: decoded => scope =>
                ObjectOf(scope: scope, id: obj.ObjectId).Bind(target =>
                    from document in scope.NeedDocument()
                    from canvas in scope.NeedCanvas()
                    select SnapCore(document: document, obj: target, decoded: decoded, visibleLimit: canvas.VisibleFrame, bounds: Option<RectangleF>.None, wireCandidates: Seq(target)))),
            groupCase: static g => Plan(policy: g.Policy, build: decoded => scope =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                from members in g.ObjectIds.TraverseM(id => UiRail.ResolveObject(objects: document.Objects, id: id, op: Op.Of(name: nameof(Snap)))).As()
                from snapped in members.IsEmpty
                    ? Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "group snap requires at least one resolvable member"))
                    : Fin.Succ(value: SnapGroup(document: document, members: members, decoded: decoded, visibleLimit: canvas.VisibleFrame))
                select snapped),
            dragCase: static drag => Plan(policy: drag.Policy, build: decoded => scope =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                from frame in Op.Of(name: nameof(SnapProbe.Drag)).AcceptRect(value: drag.Bounds, detail: "invalid drag bounds", requirePositive: true)
                from members in drag.ObjectIds.TraverseM(id => UiRail.ResolveObject(objects: document.Objects, id: id, op: Op.Of(name: nameof(Snap)))).As()
                from snapped in members.IsEmpty
                    ? Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SnapProbe.Drag)), detail: "drag snap requires at least one member"))
                    : Fin.Succ(value: SnapCore(document: document, obj: members[0], decoded: decoded, visibleLimit: canvas.VisibleFrame, bounds: Some(frame), wireCandidates: members, excludeIds: Some(new GuidSet([.. members.Bind(static m => toSeq(m.EntireFamily).Map(static f => f.InstanceId))]))))
                select snapped),
            resizeCase: static resize => Plan(policy: resize.Policy, build: decoded => scope =>
                from frame in Op.Of(name: nameof(SnapProbe.ResizeCase)).AcceptRect(value: resize.PreviewBounds, detail: "invalid resize preview bounds", requirePositive: true)
                from snapped in SnapRectangle(scope: scope, id: resize.ObjectId, bounds: frame, decoded: decoded)
                    // All-zero resize masks collapse to None so consumers see no snap.
                select snapped.Bind(s => resize.Axis.Mask(dx: s.Dx, dy: s.Dy) is (float mx, float my) && (mx != 0f || my != 0f)
                    ? Some(s with { Dx = mx, Dy = my, Magnitude = MathF.Sqrt((mx * mx) + (my * my)) })
                    : Option<SnappingSnapshot>.None)),
            rotatedCase: static rotated => Plan(policy: rotated.Policy, build: _ => _ =>
                // GH2 SnappingConstraints is translation-only; rotation snap is pure angle quantisation.
                Fin.Succ(value: SnapAngle(angle: rotated.Angle, increment: rotated.Increment, tolerance: rotated.Tolerance, pivot: rotated.Pivot))));

    // One Scope-marshalled rectangle probe: validate, resolve the object, then SnapCore over the pre-decoded carrier.
    private static Fin<Option<SnappingSnapshot>> SnapRectangle(GrasshopperUi.Scope scope, Guid id, RectangleF bounds, PolicyDecode decoded) =>
        Op.Of(name: nameof(SnapRectangle)).AcceptRect(value: bounds, detail: "invalid rectangle probe", requirePositive: true)
            .Bind(valid => ObjectOf(scope: scope, id: id).Bind(obj =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                select SnapCore(document: document, obj: obj, decoded: decoded, visibleLimit: canvas.VisibleFrame, bounds: Some(valid), wireCandidates: Seq(obj))));

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        UiRail.ResolveObject(scope: scope, id: id, op: Op.Of(name: nameof(ObjectOf)));

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, PointF delta, Option<PolicyDecode> snap, RectangleF visibleLimit, ActionList actions) {
        actions.Add(new PivotAction(obj: obj));
        IAttributes attributes = obj.Attributes;
        attributes.Layout(shape: Grasshopper2.UI.Skinning.Shape.Default);
        RectangleF bounds = snap.Map(decoded => TargetBounds(attributes: attributes, decoded: decoded)).IfNone(attributes.AggregateBounds);
        Option<SnappingSnapshot> snapped = snap.Bind(decoded => attributes.Snappable
            ? SnapCore(
                document: document, obj: obj, decoded: decoded,
                visibleLimit: visibleLimit,
                bounds: Some(new RectangleF(x: bounds.X + delta.X, y: bounds.Y + delta.Y, width: bounds.Width, height: bounds.Height)),
                wireCandidates: Seq(obj))
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

    private static RectangleF TargetBounds(IAttributes attributes, PolicyDecode decoded) =>
        decoded.UseAggregateWireBounds ? attributes.AggregateBounds : attributes.Bounds;

    // Group snap uses union bounds and all member wire families, not only members[0].
    private static Option<SnappingSnapshot> SnapGroup(GhDocument document, Seq<IDocumentObject> members, PolicyDecode decoded, RectangleF visibleLimit) =>
        SnapCore(
            document: document,
            obj: members[0],
            decoded: decoded,
            visibleLimit: visibleLimit,
            bounds: Some(members.Tail.Fold(members[0].Attributes.AggregateBounds, static (acc, m) => RectangleF.Union(rect1: acc, rect2: m.Attributes.AggregateBounds))),
            wireCandidates: members,
            excludeIds: Some(new GuidSet([.. members.Bind(static m => toSeq(m.EntireFamily).Map(static f => f.InstanceId))])));

    // Decoded carrier is threaded in already-folded so the settings fold never repeats per object across a batch.
    internal static Option<SnappingSnapshot> SnapCore(
        GhDocument document,
        IDocumentObject obj,
        PolicyDecode decoded,
        RectangleF visibleLimit,
        Option<RectangleF> bounds,
        Seq<IDocumentObject> wireCandidates = default,
        Option<GuidSet> excludeIds = default) {
        // Exclude the whole component family so multi-part components never snap against their own siblings.
        GuidSet filter = excludeIds.IfNone(() => new GuidSet([.. obj.EntireFamily.Select(static m => m.InstanceId)]));
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: decoded.IncludeSelected,
            includeUnselected: decoded.IncludeUnselected,
            filter: filter);
        // Bypass SnapObject because it always uses AggregateBounds; policy may request tight Bounds.
        RectangleF wireBounds = bounds.IfNone(decoded.UseAggregateWireBounds ? obj.Attributes.AggregateBounds : obj.Attributes.Bounds);
        constraints.SnapRectangle(target: wireBounds, settings: decoded.Native, visibleLimit: visibleLimit, snapX: out SnappingAction x, snapY: out SnappingAction y);
        float radius = decoded.Native.EdgeRadius;
        // Grid SnapSpaces (SnapNumeric, sound) fold through one smaller-magnitude per-axis projection; geometry-edge
        // alignment is computed by direct arithmetic and merged in, bypassing the dead ISnapElement path.
        (SnappingAction? spaceX, SnappingAction? spaceY) = decoded.Spaces.Fold(
            (X: (SnappingAction?)default, Y: (SnappingAction?)default),
            (acc, space) => {
                (SnappingAction? sx, SnappingAction? sy) = SnapActions(target: wireBounds, space: space, radius: radius);
                return (SnappingAction.SmallerMagnitude(a: acc.X, b: sx), SnappingAction.SmallerMagnitude(a: acc.Y, b: sy));
            });
        (SnappingAction? geomX, SnappingAction? geomY) = GeometryActions(geometry: decoded.Geometry, objects: document.Objects, excludeIds: filter, target: wireBounds);
        SnappingAction? edgeX = SnappingAction.SmallerMagnitude(a: spaceX, b: geomX);
        SnappingAction? edgeY = SnappingAction.SmallerMagnitude(a: spaceY, b: geomY);
        // Multi-member probes fold every candidate's wire family into the Y channel.
        SnappingAction wireY = wireCandidates.Fold(y, (acc, m) =>
            SnappingAction.SmallerMagnitude(a: acc, b: SnappingConstraints.SnapWires(target: m, boundsOverride: wireBounds, settings: decoded.Native)));
        // SmartGap guides span one or both axes (None = both); each axis runs the visible-frame pre-filtered scan.
        Seq<LineF> gapGuides = decoded.SmartGap.Map(sg =>
            sg.Axis.Map(static a => Seq(a)).IfNone(Seq(LayoutAxis.Horizontal, LayoutAxis.Vertical))
                .Bind(axis => EquidistanceGuides(document: document, anchor: wireBounds, excludeIds: filter, tolerance: sg.Tolerance, axis: axis, visibleLimit: visibleLimit))).IfNone([]);
        return UiRail.SnapChannels(
                x: Optional(SnappingAction.SmallerMagnitude(a: x, b: edgeX)),
                y: Optional(SnappingAction.SmallerMagnitude(a: wireY, b: edgeY)))
            .Map(snapshot => gapGuides.IsEmpty ? snapshot : snapshot with { Lines = snapshot.Lines + gapGuides });
    }

    // GeometryCase aligns the target's edges/centres to other objects' edges/centres via direct arithmetic over the
    // decoded geometry projection. BOUNDARY ADAPTER — bypasses two GH2 native defects: SnapLine.Project(double,double,
    // bool,out,out,out string) is a stub (returns false), and SnapSpace.Snap's ISnapElement branch measures the Y
    // distance as y'-x not y'-y. Geometry snap must never route through ISnapElement; only the grid SnapSpace path
    // (SnapNumeric) stays sound (SpaceCase). Per-axis feature slots pair a coordinate extractor with the native
    // SnappingAction guide factory; BestAlign retains the winning other-rect so the guide renders at the target feature
    // coordinate, not the moving probe origin.
    private static (SnappingAction? X, SnappingAction? Y) GeometryActions(Option<(SnapProjection Projection, double Radius)> geometry, GhObjectList objects, GuidSet excludeIds, RectangleF target) =>
        geometry.Map(geo => {
            float radius = (float)geo.Radius;
            Seq<RectangleF> others = toSeq(objects.Forwards).Filter(o => !excludeIds.Contains(o.InstanceId)).Map(static o => o.Attributes.AggregateBounds);
            static SnappingAction? BestAlign(RectangleF source, Seq<RectangleF> candidates, float radius, Seq<(Func<RectangleF, float> Feature, Func<RectangleF, RectangleF, float, SnappingAction> Factory)> slots) =>
                toSeq(slots.Bind(slot => candidates.Map(other => (slot.Factory, Other: other, Delta: slot.Feature(other) - slot.Feature(source))))
                        .Filter(c => MathF.Abs(c.Delta) <= radius && c.Delta != 0f)
                        .OrderBy(static c => MathF.Abs(c.Delta)))
                    .Head
                    .Map(won => (SnappingAction?)won.Factory(source, won.Other, won.Delta)).IfNone((SnappingAction?)null);
            SnappingAction? x = geo.Projection is SnapProjection.X or SnapProjection.XY
                ? BestAlign(target, others, radius, Seq(
                    ((Func<RectangleF, float>)(static b => b.Left), (Func<RectangleF, RectangleF, float, SnappingAction>)((s, t, d) => SnappingAction.CreateLeftAlignAction(source: s, target: t, dx: d))),
                    (static b => b.Center.X, (s, t, d) => SnappingAction.CreateCentreAlignAction(source: s, target: t, dx: d)),
                    (static b => b.Right, (s, t, d) => SnappingAction.CreateRightAlignAction(source: s, target: t, dx: d))))
                : default;
            SnappingAction? y = geo.Projection is SnapProjection.Y or SnapProjection.XY
                ? BestAlign(target, others, radius, Seq(
                    ((Func<RectangleF, float>)(static b => b.Top), (Func<RectangleF, RectangleF, float, SnappingAction>)((s, t, d) => SnappingAction.CreateTopAlignAction(source: s, target: t, dy: d))),
                    (static b => b.Bottom, (s, t, d) => SnappingAction.CreateBottomAlignAction(source: s, target: t, dy: d))))
                : default;
            return (X: x, Y: y);
        }).IfNone((default, default));

    // BOUNDARY ADAPTER — SnapSpace.Snap(x, y, cutoff, out x', out y', out description) is a void out-param API whose
    // cutoff is a LINEAR radius (the implementation applies Maths.DistanceToQuadrance internally), so the float radius is
    // passed through unsquared; per-axis actions are rebuilt from the snapped coordinates and the whole probe is fenced
    // because the native out-param call can throw inside a degenerate SnapSpace.
    private static (SnappingAction? X, SnappingAction? Y) SnapActions(RectangleF target, SnapSpace space, float radius) =>
        Op.Of(name: nameof(SnapActions)).Attempt<(SnappingAction?, SnappingAction?)>(
            body: () => {
                space.Snap(target.Center.X, target.Center.Y, (double)radius, out double lineX, out double lineY, out string description);
                float deltaX = (float)lineX - target.Center.X;
                float deltaY = (float)lineY - target.Center.Y;
                string label = description is { Length: > 0 } ? description : "snap";
                SnappingAction? snapX = float.IsFinite(deltaX) && MathF.Abs(x: deltaX) <= radius && deltaX != 0f
                    ? new SnappingAction(dx: deltaX, dy: 0f, text: label, point: new PointF(x: (float)lineX, y: target.Top - radius), anchor: TextAnchor.LowerMiddle, lines: [new LineF((float)lineX, target.Top - radius, (float)lineX, target.Bottom + radius)])
                    : default;
                SnappingAction? snapY = float.IsFinite(deltaY) && MathF.Abs(x: deltaY) <= radius && deltaY != 0f
                    ? new SnappingAction(dx: 0f, dy: deltaY, text: label, point: new PointF(x: target.Left - radius, y: (float)lineY), anchor: TextAnchor.CentreRight, lines: [new LineF(target.Left - radius, (float)lineY, target.Right + radius, (float)lineY)])
                    : default;
                return (snapX, snapY);
            },
            what: "SnapSpace.Snap")
            .IfFail(_ => (null, null));

    // Equal-spacing feedback emits short tick guides only when all adjacent gaps match within tolerance. The axis
    // selects the centre coordinate via the existing Origin/Span delegates; the visible frame pre-filters candidates
    // so a dense document costs O(n_visible) per 60 Hz probe, not O(n_doc). Tolerance gates only equality; the visible
    // tick length scales off the viewport so guides stay perceptible at low zoom where tolerance shrinks to pixels.
    private static Seq<LineF> EquidistanceGuides(GhDocument document, RectangleF anchor, GuidSet excludeIds, int tolerance, LayoutAxis axis, RectangleF visibleLimit) {
        float Centre(RectangleF b) => axis.Origin(bounds: b) + (axis.Span(bounds: b) * 0.5f);
        Seq<float> centres = toSeq(document.Objects.Forwards)
            .Filter(o => !excludeIds.Contains(o.InstanceId) && o.Attributes.AggregateBounds.Intersects(visibleLimit))
            .Map(o => Centre(o.Attributes.AggregateBounds)) + Seq(Centre(anchor));
        Seq<float> ordered = toSeq(centres.Distinct().Order());
        Seq<float> gaps = ordered.Zip(ordered.Tail).Map(static pair => pair.Second - pair.First);
        float tickLength = MathF.Max(tolerance, axis.CrossExtent(frame: visibleLimit) * 0.01f);
        return gaps.Count >= 2 && gaps.Tail.ForAll(g => MathF.Abs(g - gaps.Head.IfNone(0f)) <= tolerance)
            ? ordered.Map(c => axis.Tick(centre: c, anchor: anchor, length: tickLength))
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
                Lines: ArcLines(pivot: pivot, fromDeg: angle, toDeg: nearest, radius: 40f, segments: 8)))
            : Option<SnappingSnapshot>.None;

    // N-segment chord arc around the pivot from the live angle to the snapped angle (GH2 has no native rotation guide).
    private static Seq<LineF> ArcLines(PointF pivot, double fromDeg, double toDeg, float radius, int segments) {
        double from = fromDeg * Math.PI / 180.0;
        double sweep = (toDeg - fromDeg) * Math.PI / 180.0;
        return toSeq(Enumerable.Range(start: 0, count: segments)).Map(i => {
            double a0 = from + (sweep * i / segments);
            double a1 = from + (sweep * (i + 1) / segments);
            return new LineF(
                pivot.X + (radius * (float)Math.Cos(a0)), pivot.Y + (radius * (float)Math.Sin(a0)),
                pivot.X + (radius * (float)Math.Cos(a1)), pivot.Y + (radius * (float)Math.Sin(a1)));
        });
    }

    // The gap policy plus measured extent-vs-content selects the cursor pack or the stretch solve.
    private static Seq<(Guid Id, float Dx, float Dy)> PackDistribution(Seq<(Guid Id, RectangleF Bounds)> sorted, LayoutAxis axis, float gap, LayoutGapPolicy gapPolicy) {
        int count = sorted.Count;
        float extent = axis.Origin(bounds: sorted[count - 1].Bounds) + axis.Span(bounds: sorted[count - 1].Bounds) - axis.Origin(bounds: sorted[0].Bounds);
        float content = sorted.Fold(0f, (sum, item) => sum + axis.Span(bounds: item.Bounds)) + ((count - 1) * gap);
        return gapPolicy.ForcePack || extent <= content + gapPolicy.ContentSlack
            ? PackCursor(sorted: sorted, axis: axis, gap: gap)
            : PackStretch(sorted: sorted, axis: axis, gap: gap);
    }

    // Cursor-pack threads a span-cursor through the fold; moves prepend then reverse once so the fold never forces the
    // lazy Seq backing per element.
    private static Seq<(Guid Id, float Dx, float Dy)> PackCursor(Seq<(Guid Id, RectangleF Bounds)> sorted, LayoutAxis axis, float gap) =>
        sorted
            .Fold(
                (Moves: Seq<(Guid Id, float Dx, float Dy)>(), Cursor: axis.Origin(bounds: sorted[0].Bounds), Index: 0),
                (state, item) => {
                    (float dx, float dy) = axis.Delta(cursor: state.Cursor, bounds: item.Bounds);
                    float nextCursor = state.Cursor + axis.Span(bounds: item.Bounds) + (state.Index < sorted.Count - 1 ? gap : 0f);
                    return (Moves: (item.Id, dx, dy).Cons(state.Moves), Cursor: nextCursor, Index: state.Index + 1);
                })
            .Moves
            .Rev();

    // StretchLayoutSolver interleaves fixed object spans with stretchable gaps at even/odd slots (span 2*i, gap 2*i+1).
    [BoundaryAdapter]
    private static Seq<(Guid Id, float Dx, float Dy)> PackStretch(Seq<(Guid Id, RectangleF Bounds)> sorted, LayoutAxis axis, float gap) {
        ReadOnlySpan<(Guid Id, RectangleF Bounds)> span = [.. sorted];
        int count = span.Length;
        float firstOrigin = axis.Origin(bounds: span[0].Bounds);
        float lastEnd = axis.Origin(bounds: span[count - 1].Bounds) + axis.Span(bounds: span[count - 1].Bounds);
        StretchLayoutSolver solver = new();
        // BOUNDARY ADAPTER — StretchLayoutSolver requires imperative Add/Solve/Round; the assembling and emitting span
        // loops are the named measured-kernel statement exemption.
        for (int index = 0; index < count; index++) {
            float itemSpan = axis.Span(bounds: span[index].Bounds);
            solver.Add(min: itemSpan, max: itemSpan, ideal: itemSpan);
            if (index < count - 1) {
                solver.Add(min: gap, max: float.MaxValue, ideal: gap);
            }
        }
        _ = solver.Solve(target: lastEnd - firstOrigin);
        solver.Round();
        (Guid Id, float Dx, float Dy)[] moves = new (Guid, float, float)[count];
        float cursor = firstOrigin;
        for (int index = 0; index < count; index++) {
            (Guid id, RectangleF bounds) = span[index];
            (float dx, float dy) = axis.Delta(cursor: cursor, bounds: bounds);
            moves[index] = (id, dx, dy);
            cursor += solver[index * 2] + (index < count - 1 ? solver[(index * 2) + 1] : 0f);
        }
        return toSeq(moves);
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
                    _ => PackDistribution(sorted: sorted, axis: axis, gap: gap.Value, gapPolicy: gapPolicy),
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
                (Guid Id, RectangleF Bounds)[] resolved = [.. toSeq(resolvedAll
                        .OrderBy(static s => s.Bounds.Top)
                        .ThenBy(static s => s.Bounds.Left))
                    .Take(Math.Max(0, rows * cols))];
                int count = resolved.Length;
                int rowCount = count > 0 ? ((count - 1) / cols) + 1 : 0;
                HashMap<Guid, RectangleF> bounds = toHashMap(toSeq(resolved).Map(static item => (item.Id, item.Bounds)));
                // Representative ids: the first row spans the columns, the first column spans the rows.
                Seq<Guid> colReps = Seq<Guid>();
                for (int col = 0; col < Math.Min(cols, count); col++) {
                    colReps = colReps.Add(resolved[col].Id);
                }
                Seq<Guid> rowReps = Seq<Guid>();
                for (int row = 0; row < rowCount; row++) {
                    rowReps = rowReps.Add(resolved[row * cols].Id);
                }

                Fin<HashMap<Guid, float>> AxisTargets(LayoutAxis axis, Func<RectangleF, float> origin, Seq<Guid> repIds) =>
                    ComputeDistribution(objects: objects, ids: repIds, axis: axis, gap: gap, gapPolicy: gapPolicy)
                        .Map(moves => toHashMap(moves.Choose(move => bounds.Find(move.Id).Map(b => (move.Id, origin(b) + axis.Offset(dx: move.Dx, dy: move.Dy))))));

                return from colLeft in AxisTargets(axis: LayoutAxis.Horizontal, origin: static b => b.Left, repIds: colReps)
                       from rowTop in AxisTargets(axis: LayoutAxis.Vertical, origin: static b => b.Top, repIds: rowReps)
                       select GridMoves(resolved: resolved, cols: cols, colLeft: colLeft, rowTop: rowTop);
            });
    }

    // Each cell's target reads its column-left and row-top from the per-axis maps keyed on the row/column rep; the index
    // loop is the named measured-kernel exemption.
    private static Seq<(Guid Id, float Dx, float Dy)> GridMoves(ReadOnlySpan<(Guid Id, RectangleF Bounds)> resolved, int cols, HashMap<Guid, float> colLeft, HashMap<Guid, float> rowTop) {
        (Guid Id, float Dx, float Dy)[] moves = new (Guid, float, float)[resolved.Length];
        for (int index = 0; index < resolved.Length; index++) {
            (Guid id, RectangleF box) = resolved[index];
            float left = colLeft.Find(resolved[index % cols].Id).IfNone(box.Left);
            float top = rowTop.Find(resolved[index / cols * cols].Id).IfNone(box.Top);
            moves[index] = (id, left - box.Left, top - box.Top);
        }
        return toSeq(moves);
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
                // Iteration budget tracks selection size: a worst-case repulsion chain settles in O(count) passes, so the
                // schedule scales with the count it relaxes rather than a fixed literal that under- or over-runs.
                HashMap<Guid, PointF> relaxed = toSeq(Enumerable.Range(start: 0, count: NudgePasses(count: resolved.Count))).Fold(
                    toHashMap(resolved.Map(static s => (s.Id, s.Centre))),
                    (positions, _) => pairs.Fold(positions, (acc, pair) => RepelPair(positions: acc, a: pair.A, b: pair.B, separation: separation)));
                return resolved.Choose(s => relaxed.Find(s.Id)
                    .Map(centre => (s.Id, Dx: centre.X - s.Centre.X, Dy: centre.Y - s.Centre.Y))
                    .Filter(static move => move.Dx != 0f || move.Dy != 0f));
            });

    // Pairwise repulsion converges in at most one pass per object; clamp into [4, 64] so tiny selections still settle
    // and a large selection cannot open the UI-thread budget unbounded.
    private static int NudgePasses(int count) => Math.Clamp(value: count, min: 4, max: 64);

    // Straighten wires inside the selection. Wire.AllWireEnds yields parameter-keyed endpoints, so each endpoint maps to
    // its owning selected object through EntireFamily; a single projection picks each object's first in-selection
    // predecessor and aligns its centre Y. One pass (no successor walk) makes cycles inert: a member moves at most once.
    private static Fin<Seq<(Guid Id, float Dx, float Dy)>> ComputeWireAlignment(GhObjectList objects, Seq<Guid> ids) =>
        ids.TraverseM(id => UiRail.ResolveObject(objects: objects, id: id, op: Op.Of(name: nameof(ComputeWireAlignment)))
                .Map(o => (Owner: o, Centre: o.Attributes.AggregateBounds.Center)))
            .As()
            .Map(resolved => {
                HashMap<Guid, Guid> ownerOfParameter = toHashMap(resolved.Bind(r => toSeq(r.Owner.EntireFamily).Map(m => (m.InstanceId, r.Owner.InstanceId))));
                HashMap<Guid, float> centreY = toHashMap(resolved.Map(static r => (r.Owner.InstanceId, r.Centre.Y)));
                // Each selected object inherits the Y of the source owner on its first in-selection incoming wire.
                HashMap<Guid, float> targetY = Wire.AllWireEnds(objects: objects).Fold(
                    HashMap<Guid, float>(),
                    (acc, wire) => (ownerOfParameter.Find(wire.Source), ownerOfParameter.Find(wire.Target)) switch {
                        ( { IsSome: true, Case: Guid src }, { IsSome: true, Case: Guid tgt })
                            when src != tgt && acc.Find(tgt).IsNone =>
                            centreY.Find(src).Map(y => acc.AddOrUpdate(tgt, y)).IfNone(acc),
                        _ => acc,
                    });
                return resolved.Choose(r => targetY.Find(r.Owner.InstanceId)
                    .Map(y => (r.Owner.InstanceId, Dx: 0f, Dy: y - r.Centre.Y))
                    .Filter(static move => move.Dy != 0f));
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

}

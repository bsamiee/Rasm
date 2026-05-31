using System.Globalization;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GuidSet = System.Collections.Generic.HashSet<System.Guid>;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class UiTolerance {
    // Canvas-space hit radius shared by snap probes (Layout.SnapProbe) and proximity finds
    // (Document.FindCriterion.Near) — one anchor so both surfaces move together.
    internal const float PickRadius = 32f;
}

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

/// <summary>
/// GH2 native distribute is stretch-oriented. <see cref="Stretch"/> matches that behaviour via
/// <c>StretchLayoutSolver</c> when the selection extent exceeds content; <see cref="Fixed"/> keeps a
/// constant gap and never stretches gaps between objects.
/// </summary>
[SmartEnum<int>]
public sealed partial class LayoutGapPolicy {
    // Per-policy float slack: Stretch tolerates sub-pixel overflow before invoking the stretch solver so a
    // selection only marginally exceeding packed content still distributes with a fixed cursor; Fixed packs
    // at a constant gap and never stretches, so its slack is irrelevant (0).
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
    public sealed record DistributeCase(LayoutAxis Axis, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record GridCase(int Rows, int Cols, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;
    public sealed record FlowCase(LayoutAxis Axis, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;

    /// <summary>Align every target to the anchor's edge via <c>OCD.AlignObjects</c>; the anchor never moves.</summary>
    public static LayoutArrangement Align(Guid anchor, Seq<Guid> targets, OCD.Fixed fix = OCD.Fixed.None) =>
        new AlignCase(Anchor: anchor, Targets: targets, Fix: fix);

    /// <summary>Distribute selection with an explicit gap policy.</summary>
    /// <param name="axis">Layout axis.</param>
    /// <param name="gap">Minimum gap between objects.</param>
    /// <param name="ids">Object ids to distribute.</param>
    /// <param name="gapPolicy">Prefer <see cref="DistributeStretch"/> unless fixed-gap UX is intended.</param>
    public static LayoutArrangement Distribute(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy) =>
        new DistributeCase(Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy);

    /// <summary>Default distribute: GH2-aligned stretch policy with explicit minimum gap.</summary>
    public static LayoutArrangement DistributeStretch(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids) =>
        Distribute(axis: axis, gap: gap, ids: ids, gapPolicy: LayoutGapPolicy.Stretch);

    /// <summary>Fixed-gap distribute: never invokes <c>StretchLayoutSolver</c>.</summary>
    public static LayoutArrangement DistributeFixed(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids) =>
        Distribute(axis: axis, gap: gap, ids: ids, gapPolicy: LayoutGapPolicy.Fixed);

    /// <summary>Arrange ids into a rows×cols grid, reusing per-axis distribution to fix columns then rows.</summary>
    public static LayoutArrangement Grid(int rows, int cols, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy gapPolicy) =>
        new GridCase(Rows: rows, Cols: cols, Gap: gap, Ids: ids, GapPolicy: gapPolicy);

    /// <summary>Distribute ids in causal (topology-sorted) order rather than spatial order — closes the
    /// WireGraph-read → Layout-write loop by laying out a dataflow chain along the axis in dependency order.</summary>
    public static LayoutArrangement Flow(LayoutAxis axis, LayoutGap gap, Seq<Guid> ids, LayoutGapPolicy? gapPolicy = null) =>
        new FlowCase(Axis: axis, Gap: gap, Ids: ids, GapPolicy: gapPolicy ?? LayoutGapPolicy.Stretch);
}

// --- [MODELS] -----------------------------------------------------------------------------
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
    public sealed record FeedbackCase(bool Enabled, SnapGuideStyle Style) : SnapSetting;
    public sealed record RadiiCase(Option<int> EdgeRadius = default, Option<int> WireRadius = default, Option<int> VerticalGapSize = default, Option<int> HorizontalGapSize = default) : SnapSetting;
    public sealed record GridCase(Option<SizeF> Cell = default, Option<PointF> Origin = default) : SnapSetting;
    public sealed record WireBoundsCase(bool Aggregate = true) : SnapSetting;

    internal SnappingSettings Apply(SnappingSettings settings) =>
        Switch(
            state: settings,
            withRuleCase: static (state, op) => state.WithRules(rules: op.Rule),
            withoutRuleCase: static (state, op) => state.WithoutRules(rules: op.Rule),
            feedbackCase: static (state, op) => state.WithFeedback(drawFeedback: op.Enabled, colour: op.Style.Tint),
            radiiCase: static (state, op) => new SnappingSettings(rules: state.Rules, verticalGap: op.VerticalGapSize.Map(static value => Math.Max(0, value)).IfNone(state.VerticalGapSize), horizontalGap: op.HorizontalGapSize.Map(static value => Math.Max(0, value)).IfNone(state.HorizontalGapSize), edgeRadius: op.EdgeRadius.Map(static value => Math.Max(0, value)).IfNone(state.EdgeRadius), wireRadius: op.WireRadius.Map(static value => Math.Max(0, value)).IfNone(state.WireRadius), feedback: state.Feedback, colour: state.Colour),
            gridCase: static (state, _) => state,
            wireBoundsCase: static (state, _) => state);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(bool IncludeSelected = true, bool IncludeUnselected = true, Seq<SnapSetting> Settings = default) {
    internal SnappingSettings Native =>
        Settings.Fold(SnappingSettings.Current, static (state, op) => op.Apply(settings: state));

    // First enabled feedback setting's full guide style, if any — drives the live snap-guide cosmetic.
    internal Option<SnapGuideStyle> FeedbackStyle =>
        Settings.Choose(static s => s is SnapSetting.FeedbackCase { Enabled: true } feedback ? Some(feedback.Style) : Option<SnapGuideStyle>.None).Head;

    internal Option<(SizeF Cell, PointF Origin)> Grid(Canvas canvas) =>
        Settings.Choose(static s => s is SnapSetting.GridCase grid ? Some(grid) : Option<SnapSetting.GridCase>.None)
            .Head
            .Map(g => (
                Cell: g.Cell.IfNone(canvas.Skin.Canvasses[canvas.Kind].GridSkin.Cell),
                Origin: g.Origin.IfNone(PointF.Empty)))
            .Filter(static g => float.IsFinite(g.Cell.Width) && float.IsFinite(g.Cell.Height) && g.Cell.Width > 0f && g.Cell.Height > 0f);

    internal bool UseAggregateWireBounds =>
        Settings.Choose(static s => s is SnapSetting.WireBoundsCase wire ? Some(wire.Aggregate) : Option<bool>.None)
            .Head
            .IfNone(true);

    internal SnappingPolicy Effective => this;
}

[SkipUnionOps]
[Union]
public partial record SnapProbe {
    private SnapProbe() { }
    public sealed record PointCase(Guid ObjectId, PointF Probe, float Radius = UiTolerance.PickRadius, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record RectangleCase(Guid ObjectId, RectangleF Bounds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record ObjectCase(Guid ObjectId, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record GroupCase(Seq<Guid> ObjectIds, SnappingPolicy Policy = default) : SnapProbe;

    internal Option<SnapGuideStyle> FeedbackStyle => Switch(
        pointCase: static p => p.Policy.FeedbackStyle,
        rectangleCase: static r => r.Policy.FeedbackStyle,
        objectCase: static o => o.Policy.FeedbackStyle,
        groupCase: static g => g.Policy.FeedbackStyle);
}

[GenerateUnionOps]
[Union]
public partial record LayoutOp {
    private LayoutOp() { }
    public sealed partial record MeasureCase(ObjectScope Scope) : LayoutOp;
    public sealed partial record ArrangeCase(LayoutArrangement Arrangement) : LayoutOp;
    public sealed partial record SnapCase(SnapProbe Probe) : LayoutOp;

    internal GrasshopperUiPolicy UiPolicy => Switch(
        measureCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        arrangeCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        snapCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None));
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
    internal static Fin<LayoutResult> Dispatch(GrasshopperUi.Scope scope, LayoutOp op) =>
        op.Switch(
            state: scope,
            measureCase: static (s, measure) => Measure(scope: measure.Scope).Run(scope: s),
            arrangeCase: static (s, a) => Arrange(arrangement: a.Arrangement).Map(static delta => (LayoutResult)new LayoutResult.MutationResult(Delta: delta)).Run(scope: s),
            snapCase: static (s, snap) => Snap(probe: snap.Probe).Map(static result => (LayoutResult)new LayoutResult.SnapResult(Snapshot: result)).Run(scope: s));

    private static GrasshopperUiIntent<LayoutResult> Measure(ObjectScope scope) =>
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
            distributeCase: static distribute => Distribute(
                axis: distribute.Axis,
                gap: distribute.Gap,
                gapPolicy: distribute.GapPolicy,
                ids: [.. distribute.Ids]),
            gridCase: static grid => Grid(
                rows: grid.Rows,
                cols: grid.Cols,
                gap: grid.Gap,
                gapPolicy: grid.GapPolicy,
                ids: [.. grid.Ids]),
            flowCase: static flow => Flow(
                axis: flow.Axis,
                gap: flow.Gap,
                gapPolicy: flow.GapPolicy,
                ids: [.. flow.Ids]));

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
                        let active = snap.Effective
                        let applied = ApplyMove(
                            obj: obj,
                            document: document,
                            delta: delta,
                            snap: Some(active),
                            visibleLimit: canvas.VisibleFrame,
                            grid: active.Grid(canvas: canvas),
                            actions: actions)
                        select (DocumentMutationReceipt.Count(changed: 1), applied))
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
                        let active = snap.Map(static p => p.Effective)
                        let applied = ApplyMove(
                            obj: obj,
                            document: document,
                            delta: new PointF(x: valid.X - before.Pivot.X, y: valid.Y - before.Pivot.Y),
                            snap: active,
                            visibleLimit: canvas.VisibleFrame,
                            grid: active.Bind(policy => policy.Grid(canvas: canvas)),
                            actions: actions)
                        select (DocumentMutationReceipt.Count(changed: 1), applied))
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
                    from deltas in toSeq(validTargets)
                        .TraverseM(target => AlignOne(objects: objects, actions: actions, anchor: anchorObj, target: target, fix: fix)).As()
                    select (
                        DocumentMutationReceipt.Count(changed: deltas.Count),
                        new LayoutArrangeDelta(Moves: deltas))));
    }

    private static Fin<LayoutMoveDelta> AlignOne(GhObjectList objects, ActionList actions, IDocumentObject anchor, Guid target, OCD.Fixed fix) =>
        from targetObj in UiRail.ResolveObject(objects: objects, id: target, op: Op.Of(name: nameof(Align)))
        let before = SnapshotOf(attributes: targetObj.Attributes)
        from _ in Op.Of(name: nameof(Align)).Attempt(body: () => { actions.Add(new PivotAction(obj: targetObj)); return unit; }, what: "PivotAction")
        from aligned in AlignVia(a: anchor, b: targetObj, fix: fix)
        let after = SnapshotOf(attributes: targetObj.Attributes)
        select new LayoutMoveDelta(ObjectId: target, Dx: after.Pivot.X - before.Pivot.X, Dy: after.Pivot.Y - before.Pivot.Y, After: after, Snap: Option<SnappingSnapshot>.None);

    // Shared apply rail for multi-object arrangements: open one UndoGroup, fold the computed moves through the
    // single Move primitive, then commit. Distribute and Grid feed it different per-axis move generators.
    private static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> ArrangeMoves(string noun, int minimum, Func<GhObjectList, Fin<Seq<(Guid Id, float Dx, float Dy)>>> compute) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunMutation(
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
                            snap: Option<SnappingPolicy>.None,
                            visibleLimit: RectangleF.Empty,
                            grid: Option<(SizeF Cell, PointF Origin)>.None,
                            actions: actions)
                        select delta).As()
                    select (
                        DocumentMutationReceipt.Count(changed: deltas.Count),
                        new LayoutArrangeDelta(Moves: deltas))));

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Distribute(LayoutAxis axis, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return snapshot.Length >= 2
            ? ArrangeMoves(
                noun: string.Create(CultureInfo.InvariantCulture, $"Distribute {axis}"),
                minimum: 2,
                compute: objs => ComputeDistribution(objects: objs, ids: toSeq(snapshot), axis: axis, gap: gap, gapPolicy: gapPolicy))
            : GhUi.Document(run: _ => Fin.Fail<Snapshot<LayoutArrangeDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids")));
    }

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Grid(int rows, int cols, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return rows >= 1 && cols >= 1 && snapshot.Length >= 2
            ? ArrangeMoves(
                noun: string.Create(CultureInfo.InvariantCulture, $"Grid {rows}x{cols}"),
                minimum: 2,
                compute: objs => ComputeGrid(objects: objs, ids: toSeq(snapshot), rows: rows, cols: cols, gap: gap, gapPolicy: gapPolicy))
            : GhUi.Document(run: _ => Fin.Fail<Snapshot<LayoutArrangeDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Grid)), detail: "Grid requires rows>=1, cols>=1, and at least 2 ids")));
    }

    // Causal flow: SortCausally orders ids by dataflow dependency before the existing distribution rail
    // (preordered skips the bounds re-sort so the topological order is preserved along the axis).
    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Flow(LayoutAxis axis, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return snapshot.Length >= 2
            ? ArrangeMoves(
                noun: string.Create(CultureInfo.InvariantCulture, $"Flow {axis}"),
                minimum: 2,
                compute: objs =>
                    from sorted in SortCausally(objects: objs, ids: toSeq(snapshot))
                    from moves in ComputeDistribution(objects: objs, ids: sorted, axis: axis, gap: gap, gapPolicy: gapPolicy, preordered: true)
                    select moves)
            : GhUi.Document(run: _ => Fin.Fail<Snapshot<LayoutArrangeDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Flow)), detail: "Flow requires at least 2 ids")));
    }

    private static Fin<Seq<Guid>> SortCausally(GhObjectList objects, Seq<Guid> ids) {
        Connectivity connectivity = objects.Connectivity;
        return ids.TraverseM(id => connectivity.Find(id, out ConnectiveObject? co)
                ? Fin.Succ(co!)
                : Fin.Fail<ConnectiveObject>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SortCausally)), detail: $"object {id} not found in connectivity")))
            .As()
            .Map(nodes => toSeq(connectivity.SortCausally(objects: [.. nodes]).Select(static co => co.Id)));
    }

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document(run: scope =>
            from valid in Optional(probe).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
            from snapshot in valid.Switch(
                state: scope,
                pointCase: static (s, point) =>
                    from probe in Op.Of(name: nameof(Snap)).AcceptPoint(value: point.Probe, detail: "non-finite probe")
                    from radius in Op.Of(name: nameof(Snap)).AcceptFinite(value: point.Radius, detail: "radius must be finite and positive", requirePositive: true)
                    from snapped in SnapRectangle(scope: s, id: point.ObjectId, bounds: new RectangleF(x: probe.X - radius, y: probe.Y - radius, width: radius * 2f, height: radius * 2f), policy: point.Policy)
                    select snapped,
                rectangleCase: static (s, rectangle) =>
                    SnapRectangle(scope: s, id: rectangle.ObjectId, bounds: rectangle.Bounds, policy: rectangle.Policy),
                objectCase: static (s, obj) =>
                    ObjectOf(scope: s, id: obj.ObjectId).Bind(target =>
                        from document in s.NeedDocument()
                        from canvas in s.NeedCanvas()
                        let active = obj.Policy.Effective
                        select SnapCore(document: document, obj: target, policy: active, visibleLimit: canvas.VisibleFrame, bounds: Option<RectangleF>.None, grid: active.Grid(canvas: canvas))),
                groupCase: static (s, g) =>
                    from document in s.NeedDocument()
                    from canvas in s.NeedCanvas()
                    let active = g.Policy.Effective
                    from members in g.ObjectIds.TraverseM(id => UiRail.ResolveObject(objects: document.Objects, id: id, op: Op.Of(name: nameof(Snap)))).As()
                    from snapped in members.IsEmpty
                        ? Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "group snap requires at least one resolvable member"))
                        : Fin.Succ(value: SnapGroup(document: document, members: members, policy: active, visibleLimit: canvas.VisibleFrame, grid: active.Grid(canvas: canvas)))
                    select snapped)
            from _ in EmitSnapGuide(scope: scope, probe: valid, snapshot: snapshot)
            select snapshot);

    // Snap-guide live feedback seam: when the probe's policy enables a SnapSetting feedback colour and a snap
    // landed, emit a fire-and-forget SnapGuideCase cosmetic (dashed CAShapeLayer + distance label) that fades
    // and self-strips. Best-effort — a guide-render failure never fails the snap query.
    // GH2 renders native object-drag snap feedback through SnapXAction/SnapYAction; suppress the cosmetic while
    // a drag is at the focus-stack head so the dashed guide never double-draws over the native feedback.
    private static bool IsNativeSnapFeedbackActive(Canvas canvas) =>
        canvas.FocusObject is ObjectDragInteraction && (canvas.SnapXAction is not null || canvas.SnapYAction is not null);

    private static Fin<Unit> EmitSnapGuide(GrasshopperUi.Scope scope, SnapProbe probe, Option<SnappingSnapshot> snapshot) =>
        probe.FeedbackStyle is { IsSome: true, Case: SnapGuideStyle style } && snapshot is { IsSome: true, Case: SnappingSnapshot snap }
            ? scope.NeedCanvas().Map(static canvas => IsNativeSnapFeedbackActive(canvas: canvas)).IfFail(false)
                ? Fin.Succ(value: unit)
                : Fin.Succ(value: Motion.Cosmetic(intent: new CosmeticIntent.SnapGuideCase(Snapshot: snap, Style: style)).Run(scope: scope).Ignore())
            : Fin.Succ(value: unit);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static Fin<Option<SnappingSnapshot>> SnapRectangle(GrasshopperUi.Scope scope, Guid id, RectangleF bounds, SnappingPolicy policy) =>
        Op.Of(name: nameof(SnapRectangle)).AcceptRect(value: bounds, detail: "invalid rectangle probe", requirePositive: true)
            .Bind(valid => ObjectOf(scope: scope, id: id).Bind(obj =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                let active = policy.Effective
                select SnapRectangle(document: document, obj: obj, bounds: valid, policy: active, visibleLimit: canvas.VisibleFrame, grid: active.Grid(canvas: canvas))));

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        UiRail.ResolveObject(scope: scope, id: id, op: Op.Of(name: nameof(ObjectOf)));

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, PointF delta, Option<SnappingPolicy> snap, RectangleF visibleLimit, Option<(SizeF Cell, PointF Origin)> grid, ActionList actions) {
        actions.Add(new PivotAction(obj: obj));
        IAttributes attributes = obj.Attributes;
        attributes.Layout(shape: Shape.Default);
        RectangleF bounds = snap.Map(policy => TargetBounds(attributes: attributes, policy: policy.Effective)).IfNone(attributes.AggregateBounds);
        Option<SnappingSnapshot> snapped = snap.Bind(policy => attributes.Snappable
            ? SnapRectangle(
                document: document, obj: obj,
                bounds: new RectangleF(x: bounds.X + delta.X, y: bounds.Y + delta.Y, width: bounds.Width, height: bounds.Height),
                policy: policy.Effective,
                visibleLimit: visibleLimit,
                grid: grid)
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

    private static Option<SnappingSnapshot> SnapRectangle(GhDocument document, IDocumentObject obj, RectangleF bounds, SnappingPolicy policy, RectangleF visibleLimit, Option<(SizeF Cell, PointF Origin)> grid) =>
        SnapCore(document: document, obj: obj, policy: policy, visibleLimit: visibleLimit, bounds: Some(bounds), grid: grid);

    private static Option<SnappingSnapshot> SnapCore(
        GhDocument document,
        IDocumentObject obj,
        SnappingPolicy policy,
        RectangleF visibleLimit,
        Option<RectangleF> bounds,
        Option<(SizeF Cell, PointF Origin)> grid) {
        SnappingPolicy active = policy.Effective;
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: active.IncludeSelected,
            includeUnselected: active.IncludeUnselected,
            filter: new GuidSet([obj.InstanceId]));
        RectangleF wireBounds = bounds.IfNone(active.UseAggregateWireBounds ? obj.Attributes.AggregateBounds : obj.Attributes.Bounds);
        (SnappingAction x, SnappingAction y) = bounds
            .Map(SnapRectangle)
            .IfNone(SnapObject);
        (SnappingAction gridX, SnappingAction gridY) = grid.Map(g => SnapGrid(target: wireBounds, cell: g.Cell, origin: g.Origin, radius: active.Native.EdgeRadius)).IfNone((null!, null!));
        SnappingAction wireY = bounds.IsSome
            ? SnappingAction.SmallerMagnitude(a: y, b: SnappingConstraints.SnapWires(target: obj, boundsOverride: wireBounds, settings: active.Native))
            : y;
        return UiRail.SnapChannels(
            x: Optional(SnappingAction.SmallerMagnitude(a: x, b: gridX)),
            y: Optional(SnappingAction.SmallerMagnitude(a: wireY, b: gridY)));

        (SnappingAction X, SnappingAction Y) SnapRectangle(RectangleF rect) {
            constraints.SnapRectangle(target: rect, settings: active.Native, visibleLimit: visibleLimit, snapX: out SnappingAction snapX, snapY: out SnappingAction snapY);
            return (snapX, snapY);
        }
        (SnappingAction X, SnappingAction Y) SnapObject() {
            constraints.SnapObject(target: obj, settings: active.Native, visibleLimit: visibleLimit, snapX: out SnappingAction snapX, snapY: out SnappingAction snapY);
            return (snapX, snapY);
        }
    }

    // Rigid multi-object snap: aggregate every member's bounds, snap the union rectangle against constraints
    // that exclude all members, and return the shared delta the caller applies to every member.
    private static Option<SnappingSnapshot> SnapGroup(GhDocument document, Seq<IDocumentObject> members, SnappingPolicy policy, RectangleF visibleLimit, Option<(SizeF Cell, PointF Origin)> grid) {
        SnappingPolicy active = policy.Effective;
        RectangleF aggregate = members.Tail.Fold(members[0].Attributes.AggregateBounds, static (acc, m) => RectangleF.Union(rect1: acc, rect2: m.Attributes.AggregateBounds));
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: active.IncludeSelected,
            includeUnselected: active.IncludeUnselected,
            filter: new GuidSet([.. members.Map(static m => m.InstanceId)]));
        constraints.SnapRectangle(target: aggregate, settings: active.Native, visibleLimit: visibleLimit, snapX: out SnappingAction snapX, snapY: out SnappingAction snapY);
        (SnappingAction gridX, SnappingAction gridY) = grid.Map(g => SnapGrid(target: aggregate, cell: g.Cell, origin: g.Origin, radius: active.Native.EdgeRadius)).IfNone((null!, null!));
        return UiRail.SnapChannels(
            x: Optional(SnappingAction.SmallerMagnitude(a: snapX, b: gridX)),
            y: Optional(SnappingAction.SmallerMagnitude(a: snapY, b: gridY)));
    }

    private static (SnappingAction X, SnappingAction Y) SnapGrid(RectangleF target, SizeF cell, PointF origin, int radius) {
        (float Delta, float Line, string Label) x = GridAxis(value: target.Center.X, origin: origin.X, cell: cell.Width, radius: radius);
        (float Delta, float Line, string Label) y = GridAxis(value: target.Center.Y, origin: origin.Y, cell: cell.Height, radius: radius);
        SnappingAction snapX = float.IsFinite(x.Delta)
            ? new SnappingAction(
                dx: x.Delta,
                dy: 0f,
                text: x.Label,
                point: new PointF(x: x.Line, y: target.Top - radius),
                anchor: TextAnchor.LowerMiddle,
                lines: [new LineF(x.Line, target.Top - radius, x.Line, target.Bottom + radius)])
            : null!;
        SnappingAction snapY = float.IsFinite(y.Delta)
            ? new SnappingAction(
                dx: 0f,
                dy: y.Delta,
                text: y.Label,
                point: new PointF(x: target.Left - radius, y: y.Line),
                anchor: TextAnchor.CentreRight,
                lines: [new LineF(target.Left - radius, y.Line, target.Right + radius, y.Line)])
            : null!;
        return (snapX, snapY);
    }

    private static (float Delta, float Line, string Label) GridAxis(float value, float origin, float cell, int radius) {
        float line = origin + (MathF.Round(x: (value - origin) / cell) * cell);
        float delta = line - value;
        return MathF.Abs(x: delta) <= radius
            ? (Delta: delta, Line: line, Label: string.Create(CultureInfo.InvariantCulture, $"grid {cell:0.###}"))
            : (Delta: float.NaN, Line: line, Label: string.Empty);
    }

    // OCD.AlignObjects exposes exactly four (Component|IParameter)² overloads — exhaustively keyed here
    // so unsupported pairs fail as typed UiFault without DLR/runtime-binder cost.
    private static Fin<Unit> AlignVia(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) {
        Op op = Op.Of(name: nameof(AlignVia));
        return (a, b) switch {
            (Component left, Component right) => op.Attempt(body: () => OCD.AlignObjects(left, right, fix), what: "OCD.AlignObjects"),
            (Component left, IParameter right) => op.Attempt(body: () => OCD.AlignObjects(left, right, fix), what: "OCD.AlignObjects"),
            (IParameter left, Component right) => op.Attempt(body: () => OCD.AlignObjects(left, right, fix), what: "OCD.AlignObjects"),
            (IParameter left, IParameter right) => op.Attempt(body: () => OCD.AlignObjects(left, right, fix), what: "OCD.AlignObjects"),
            _ => Fin.Fail<Unit>(error: UiFault.MutationRejected(
                op: op,
                detail: string.Create(CultureInfo.InvariantCulture, $"OCD.AlignObjects unsupported pair ({a.GetType().Name}, {b.GetType().Name})"))),
        };
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

    // Grid placement reuses per-axis ComputeDistribution: the first row distributes horizontally to fix each
    // column's left, the first column distributes vertically to fix each row's top, then every cell snaps to
    // (colLeft, rowTop) — aligned columns and rows resolved through the same Move rail, no foreign solver.
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

        // Fixed-gap cursor fold: each object snaps to cursor, then cursor advances by span + gap.
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

        // Interleave fixed object spans with stretchable gaps; StretchLayoutSolver.Solve fits the selection
        // extent so minimum gaps expand when aggregate bounds exceed sum(spans) + (n-1)*gap.
        [BoundaryAdapter]
        private static Seq<(Guid Id, float Dx, float Dy)> ComputeStretch(
            Seq<(Guid Id, RectangleF Bounds)> sorted,
            LayoutAxis axis,
            float gap) {
            int count = sorted.Count;
            float firstOrigin = axis.Origin(bounds: sorted[0].Bounds);
            float lastEnd = axis.Origin(bounds: sorted[count - 1].Bounds) + axis.Span(bounds: sorted[count - 1].Bounds);
            StretchLayoutSolver solver = new();
            for (int index = 0; index < count; index++) {
                float span = axis.Span(bounds: sorted[index].Bounds);
                solver.Add(min: span, max: span, ideal: span);
                if (index < count - 1) {
                    solver.Add(min: gap, max: float.MaxValue, ideal: gap);
                }
            }
            _ = solver.Solve(target: lastEnd - firstOrigin);
            solver.Round();
            // Pure cursor fold over the solved slot widths (solver[] indexer reads only); object spans live at
            // even indices, the stretchable gaps that follow at odd — same shape as ComputeCursor.
            return toSeq(Enumerable.Range(start: 0, count: count)).Fold(
                (Moves: Seq<(Guid Id, float Dx, float Dy)>(), Cursor: firstOrigin),
                (state, index) => {
                    (Guid id, RectangleF bounds) = sorted[index];
                    (float dx, float dy) = axis.Delta(cursor: state.Cursor, bounds: bounds);
                    float advance = solver[index * 2] + (index < count - 1 ? solver[(index * 2) + 1] : 0f);
                    return (Moves: state.Moves.Add((id, dx, dy)), Cursor: state.Cursor + advance);
                }).Moves;
        }
    }
}

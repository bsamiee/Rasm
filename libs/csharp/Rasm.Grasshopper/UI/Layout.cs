using System.Globalization;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GuidSet = System.Collections.Generic.HashSet<System.Guid>;
using Op = Rasm.Domain.Op;
using UiSnapshot = Rasm.Grasshopper.UI.Snapshot;
using UndoAction = Grasshopper2.Undo.Action;

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
}

/// <summary>
/// GH2 native distribute is stretch-oriented. <see cref="Stretch"/> matches that behaviour via
/// <c>StretchLayoutSolver</c> when the selection extent exceeds content; <see cref="Fixed"/> keeps a
/// constant gap and never stretches gaps between objects.
/// </summary>
[SmartEnum<int>]
public sealed partial class LayoutGapPolicy {
    public static readonly LayoutGapPolicy Stretch = new(key: 0);
    public static readonly LayoutGapPolicy Fixed = new(key: 1);
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
    public sealed record MoveCase(Guid Id, float Dx, float Dy) : LayoutArrangement;
    public sealed record PlaceCase(Guid Id, PointF Pivot) : LayoutArrangement;
    public sealed record AlignCase(Guid Left, Guid Right, OCD.Fixed Fix) : LayoutArrangement;
    public sealed record DistributeCase(LayoutAxis Axis, LayoutGap Gap, Seq<Guid> Ids, LayoutGapPolicy GapPolicy) : LayoutArrangement;

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
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutSnapshot(Guid ObjectId, PointF Pivot, RectangleF Bounds, RectangleF AggregateBounds, bool Snappable) {
    public SizeF Size => new(width: Bounds.Width, height: Bounds.Height);
    public SizeF AggregateSize => new(width: AggregateBounds.Width, height: AggregateBounds.Height);
    public PointF Centre => new(x: Bounds.X + (Bounds.Width / 2f), y: Bounds.Y + (Bounds.Height / 2f));
    public PointF AggregateCentre => new(x: AggregateBounds.X + (AggregateBounds.Width / 2f), y: AggregateBounds.Y + (AggregateBounds.Height / 2f));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingSnapshot(float Dx, float Dy, float Magnitude, string XLabel, string YLabel, Seq<LineF> Lines, Option<PointF> LabelPoint, Option<TextAnchor> LabelAnchor);

[SkipUnionOps]
[Union]
public partial record SnapSetting {
    private SnapSetting() { }
    public sealed record WithRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record WithoutRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record FeedbackCase(bool Enabled, Color Colour) : SnapSetting;

    internal SnappingSettings Apply(SnappingSettings settings) =>
        Switch(
            state: settings,
            withRuleCase: static (state, op) => state.WithRules(rules: op.Rule),
            withoutRuleCase: static (state, op) => state.WithoutRules(rules: op.Rule),
            feedbackCase: static (state, op) => state.WithFeedback(drawFeedback: op.Enabled, colour: op.Colour));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(bool IncludeSelected = true, bool IncludeUnselected = true, Seq<SnapSetting> Settings = default) {
    internal SnappingSettings Native =>
        Settings.Fold(SnappingSettings.Current, static (state, op) => op.Apply(settings: state));
}

[SkipUnionOps]
[Union]
public partial record SnapProbe {
    private SnapProbe() { }
    public sealed record PointCase(Guid ObjectId, PointF Probe, float Radius = 32f, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record RectangleCase(Guid ObjectId, RectangleF Bounds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record ObjectCase(Guid ObjectId, SnappingPolicy Policy = default) : SnapProbe;
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
            primaryCase: static _ => GhUi.Document(run: _ => Fin.Fail<LayoutResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Measure)), detail: ScopeUse.LayoutMeasure.RejectsPrimaryDetail()))),
            primaryAndSecondaryCase: static _ => GhUi.Document(run: _ => Fin.Fail<LayoutResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Measure)), detail: ScopeUse.LayoutMeasure.RejectsPrimaryDetail()))));

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Arrange(LayoutArrangement arrangement) =>
        arrangement.Switch(
            moveCase: static move => Move(id: move.Id, dx: move.Dx, dy: move.Dy)
                .Map(static delta => delta.Map(static payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            placeCase: static place => Place(id: place.Id, pivot: place.Pivot)
                .Map(static delta => delta.Map(static payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            alignCase: static align => Align(left: align.Left, right: align.Right, fix: align.Fix)
                .Map(static delta => delta.Map(static payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            distributeCase: static distribute => Distribute(
                axis: distribute.Axis,
                gap: distribute.Gap,
                gapPolicy: distribute.GapPolicy,
                ids: [.. distribute.Ids]));

    internal static GrasshopperUiIntent<LayoutSnapshot> Snapshot(Guid id) =>
        GhUi.Document(run: scope =>
            ObjectOf(scope: scope, id: id).Map(obj => SnapshotOf(attributes: obj.Attributes)));

    internal static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        GhUi.Document(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.SelectedObjects.Select(o => SnapshotOf(attributes: o.Attributes)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Move(Guid id, float dx, float dy, bool snap = true) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: nameof(Move)),
            repaint: RepaintRequest.Object(id: id),
            undo: PivotUndo(noun: "Move", id: id),
            mutate: scope =>
                Op.Of(name: nameof(Move)).AcceptPoint(value: new PointF(x: dx, y: dy), detail: "non-finite delta")
                    .Bind(delta => ObjectOf(scope: scope, id: id).Bind(obj => scope.NeedDocument().Map(doc => {
                        RectangleF snapLimit = scope.Canvas.Map(static canvas => canvas.VisibleFrame).IfNone(doc.Objects.AttributeBounds);
                        return ApplyMove(obj: obj, document: doc, dx: delta.X, dy: delta.Y, snap: snap, snapVisibleLimit: snapLimit);
                    }))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Place(Guid id, PointF pivot) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: nameof(Place)),
            repaint: RepaintRequest.Object(id: id),
            undo: PivotUndo(noun: "Place", id: id),
            mutate: scope =>
                from valid in Op.Of(name: nameof(Place)).AcceptPoint(value: pivot, detail: "non-finite pivot")
                from obj in ObjectOf(scope: scope, id: id)
                from doc in scope.NeedDocument()
                let before = SnapshotOf(attributes: obj.Attributes)
                select ApplyMove(
                    obj: obj,
                    document: doc,
                    dx: valid.X - before.Pivot.X,
                    dy: valid.Y - before.Pivot.Y,
                    snap: false,
                    snapVisibleLimit: doc.Objects.AttributeBounds));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Align(Guid left, Guid right, OCD.Fixed fix = OCD.Fixed.None) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: nameof(Align)),
            repaint: RepaintRequest.Canvas,
            undo: UndoStrategy.Manual(record: s => s.Objects.Match(
                Some: objs => {
                    IDocumentObject? l = objs.Find(instanceId: left);
                    IDocumentObject? r = objs.Find(instanceId: right);
                    return l is not null && r is not null
                        ? new UndoEntry(Name: ("Layout", "Align"), Actions: Seq<UndoAction>(new PivotAction(obj: l.FoundingObject), new PivotAction(obj: r.FoundingObject)))
                        : new UndoEntry(Name: ("Layout", "Align"), Actions: Seq<UndoAction>());
                },
                None: () => new UndoEntry(Name: ("Layout", "Align"), Actions: Seq<UndoAction>()))),
            mutate: scope =>
                from leftObj in ObjectOf(scope: scope, id: left)
                from rightObj in ObjectOf(scope: scope, id: right)
                let before = SnapshotOf(attributes: rightObj.Attributes)
                from _ in AlignVia(a: leftObj, b: rightObj, fix: fix)
                let after = SnapshotOf(attributes: rightObj.Attributes)
                select new LayoutMoveDelta(ObjectId: right, Dx: after.Pivot.X - before.Pivot.X, Dy: after.Pivot.Y - before.Pivot.Y, After: after, Snap: Option<SnappingSnapshot>.None));

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Distribute(LayoutAxis axis, LayoutGap gap, LayoutGapPolicy gapPolicy, params ReadOnlySpan<Guid> ids) {
        Guid[] snapshot = ids.ToArray();
        return GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => Optional(snapshot)
                .Filter(static values => values.Length >= 2)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids"))
                .Bind(validIds => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                    UndoGroup bag = new(verb: "Layout", noun: string.Create(CultureInfo.InvariantCulture, $"Distribute {axis}"));
                    GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
                    Seq<(Guid Id, float Dx, float Dy)> moves = ComputeDistribution(objects: objs, ids: toSeq(validIds), axis: axis, gap: gap, gapPolicy: gapPolicy);
                    return Optional(moves)
                        .Filter(static values => values.Count >= 2)
                        .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "fewer than 2 supplied ids resolved to document objects"))
                        .Bind(_ => moves.TraverseM(m => Move(id: m.Id, dx: m.Dx, dy: m.Dy, snap: false).Run(scope: scoped).Map(static s => s.Payload)).As())
                        .Bind(deltas => bag.Commit(document: doc).Map(_ => UiSnapshot.Of(
                            payload: new LayoutArrangeDelta(Moves: deltas),
                            ownerId: Some(doc.Hash))));
                }))));
    }

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document(run: scope =>
            Optional(probe)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
                .Bind(valid => valid.Switch(
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
                            select SnapCore(document: document, obj: target, policy: obj.Policy, visibleLimit: canvas.VisibleFrame, bounds: Option<RectangleF>.None)))));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static UndoStrategy PivotUndo(string noun, Guid id) =>
        UndoStrategy.Manual(record: s => s.Objects.Match(
            Some: objs => Optional(objs.Find(instanceId: id))
                .Map(obj => new UndoEntry(Name: ("Layout", noun), Actions: Seq<UndoAction>(new PivotAction(obj: obj))))
                .IfNone(new UndoEntry(Name: ("Layout", noun), Actions: Seq<UndoAction>())),
            None: () => new UndoEntry(Name: ("Layout", noun), Actions: Seq<UndoAction>())));

    private static Fin<Option<SnappingSnapshot>> SnapRectangle(GrasshopperUi.Scope scope, Guid id, RectangleF bounds, SnappingPolicy policy) =>
        Op.Of(name: nameof(SnapRectangle)).AcceptRect(value: bounds, detail: "invalid rectangle probe", requirePositive: true)
            .Bind(valid => ObjectOf(scope: scope, id: id).Bind(obj =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                select SnapRectangle(document: document, obj: obj, bounds: valid, policy: policy, visibleLimit: canvas.VisibleFrame)));

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        UiRail.ResolveObject(scope: scope, id: id, op: Op.Of(name: nameof(ObjectOf)));

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, float dx, float dy, bool snap, RectangleF snapVisibleLimit) {
        IAttributes attributes = obj.Attributes;
        RectangleF bounds = attributes.AggregateBounds;
        Option<SnappingSnapshot> snapped = snap && attributes.Snappable
            ? SnapRectangle(
                document: document, obj: obj,
                bounds: new RectangleF(x: bounds.X + dx, y: bounds.Y + dy, width: bounds.Width, height: bounds.Height),
                policy: default, visibleLimit: snapVisibleLimit)
            : Option<SnappingSnapshot>.None;
        (float deltaDx, float deltaDy) = snapped.Map(static s => (s.Dx, s.Dy)).IfNone((0f, 0f));
        float effDx = dx + deltaDx;
        float effDy = dy + deltaDy;
        attributes.Move(dx: effDx, dy: effDy);
        return new LayoutMoveDelta(
            ObjectId: attributes.Owner.InstanceId,
            Dx: effDx, Dy: effDy,
            After: SnapshotOf(attributes: attributes),
            Snap: snapped);
    }

    private static Option<SnappingSnapshot> SnapRectangle(GhDocument document, IDocumentObject obj, RectangleF bounds, SnappingPolicy policy, RectangleF visibleLimit) =>
        SnapCore(document: document, obj: obj, policy: policy, visibleLimit: visibleLimit, bounds: Some(bounds));

    private static Option<SnappingSnapshot> SnapCore(
        GhDocument document,
        IDocumentObject obj,
        SnappingPolicy policy,
        RectangleF visibleLimit,
        Option<RectangleF> bounds) {
        SnappingPolicy active = ActivePolicy(policy: policy);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: active.IncludeSelected,
            includeUnselected: active.IncludeUnselected,
            filter: new GuidSet([obj.InstanceId]));
        RectangleF wireBounds = bounds.IfNone(obj.Attributes.Bounds);
        (SnappingAction x, SnappingAction y) = bounds
            .Map(SnapRectangle)
            .IfNone(SnapObject);
        return UiRail.SnapChannels(
            x: Optional(x),
            y: Optional(SnappingAction.SmallerMagnitude(
                a: y,
                b: SnappingConstraints.SnapWires(target: obj, boundsOverride: wireBounds, settings: active.Native))));

        (SnappingAction X, SnappingAction Y) SnapRectangle(RectangleF rect) {
            constraints.SnapRectangle(target: rect, settings: active.Native, visibleLimit: visibleLimit, snapX: out SnappingAction snapX, snapY: out SnappingAction snapY);
            return (snapX, snapY);
        }
        (SnappingAction X, SnappingAction Y) SnapObject() {
            constraints.SnapObject(target: obj, settings: active.Native, visibleLimit: visibleLimit, snapX: out SnappingAction snapX, snapY: out SnappingAction snapY);
            return (snapX, snapY);
        }
    }

    private static SnappingPolicy ActivePolicy(SnappingPolicy policy) =>
        (policy.IncludeSelected || policy.IncludeUnselected) switch {
            true => policy,
            false => policy with { IncludeSelected = true, IncludeUnselected = true },
        };

    // OCD.AlignObjects exposes exactly four (Component|IParameter)² overloads — exhaustively keyed here
    // so unsupported pairs fail as typed UiFault without DLR/runtime-binder cost.
    private static Fin<Unit> AlignVia(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) {
        Op op = Op.Of(name: nameof(AlignVia));
        return (a, b) switch {
            (Component left, Component right) => InvokeAlign(op: op, run: () => OCD.AlignObjects(left, right, fix), a: a, b: b),
            (Component left, IParameter right) => InvokeAlign(op: op, run: () => OCD.AlignObjects(left, right, fix), a: a, b: b),
            (IParameter left, Component right) => InvokeAlign(op: op, run: () => OCD.AlignObjects(left, right, fix), a: a, b: b),
            (IParameter left, IParameter right) => InvokeAlign(op: op, run: () => OCD.AlignObjects(left, right, fix), a: a, b: b),
            _ => Fin.Fail<Unit>(error: UiFault.MutationRejected(
                op: op,
                detail: string.Create(CultureInfo.InvariantCulture, $"OCD.AlignObjects unsupported pair ({a.GetType().Name}, {b.GetType().Name})"))),
        };
    }

    private static Fin<Unit> InvokeAlign(Op op, Action run, IDocumentObject a, IDocumentObject b) =>
        op.Attempt(body: run, what: "OCD.AlignObjects");

    private static Seq<(Guid Id, float Dx, float Dy)> ComputeDistribution(
        GhObjectList objects,
        Seq<Guid> ids,
        LayoutAxis axis,
        LayoutGap gap,
        LayoutGapPolicy gapPolicy) {
        Seq<(Guid Id, RectangleF Bounds)> sorted = toSeq(ids
            .Choose(id => Optional(objects.Find(instanceId: id)).Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)))
            .OrderBy(s => axis.Origin(bounds: s.Bounds))
            .ThenBy(s => s.Id));
        return sorted.Count switch {
            < 2 => Seq<(Guid Id, float Dx, float Dy)>(),
            _ => DistributeMode.Of(gapPolicy: gapPolicy, sorted: sorted, axis: axis, gap: gap.Value)
                .Compute(sorted: sorted, axis: axis, gap: gap.Value),
        };
    }

    [SmartEnum<int>]
    private sealed partial class DistributeMode {
        // Float slack so a selection whose extent only marginally exceeds packed content still distributes
        // with a fixed cursor instead of invoking the stretch solver on sub-pixel overflow.
        private const float ContentSlack = 1e-4f;

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
            return gapPolicy.Equals(LayoutGapPolicy.Fixed) || extent <= content + ContentSlack ? Cursor : Stretch;
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
            float cursor = firstOrigin;
            Seq<(Guid Id, float Dx, float Dy)> moves = Seq<(Guid Id, float Dx, float Dy)>();
            for (int index = 0; index < count; index++) {
                (Guid id, RectangleF bounds) = sorted[index];
                (float dx, float dy) = axis.Delta(cursor: cursor, bounds: bounds);
                moves = moves.Add((id, dx, dy));
                cursor += solver[index * 2];
                if (index < count - 1) {
                    cursor += solver[(index * 2) + 1];
                }
            }
            return moves;
        }
    }
}

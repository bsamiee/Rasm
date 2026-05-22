using System.Globalization;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;
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

[Union]
public partial record LayoutArrangement {
    private LayoutArrangement() { }
    public sealed record MoveCase(Guid Id, float Dx, float Dy) : LayoutArrangement;
    public sealed record PlaceCase(Guid Id, PointF Pivot) : LayoutArrangement;
    public sealed record AlignCase(Guid Left, Guid Right, OCD.Fixed Fix) : LayoutArrangement;
    public sealed record DistributeCase(LayoutAxis Axis, float Gap, Seq<Guid> Ids) : LayoutArrangement;
    public static LayoutArrangement Move(Guid id, float dx, float dy) => new MoveCase(Id: id, Dx: dx, Dy: dy);
    public static LayoutArrangement Place(Guid id, PointF pivot) => new PlaceCase(Id: id, Pivot: pivot);
    public static LayoutArrangement Align(Guid left, Guid right, OCD.Fixed fix = OCD.Fixed.None) => new AlignCase(Left: left, Right: right, Fix: fix);
    public static LayoutArrangement Distribute(LayoutAxis axis, float gap, Seq<Guid> ids) => new DistributeCase(Axis: axis, Gap: gap, Ids: ids);
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
public readonly record struct SnappingSnapshot(
    float Dx,
    float Dy,
    float Magnitude,
    string XLabel,
    string YLabel,
    Seq<LineF> Lines,
    Option<PointF> LabelPoint,
    Option<TextAnchor> LabelAnchor);

[Union]
public partial record SnapSetting {
    private SnapSetting() { }
    public sealed record WithRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record WithoutRuleCase(SnappingRule Rule) : SnapSetting;
    public sealed record FeedbackCase(bool Enabled, Color Colour) : SnapSetting;

    public static SnapSetting With(SnappingRule rule) => new WithRuleCase(Rule: rule);
    public static SnapSetting Without(SnappingRule rule) => new WithoutRuleCase(Rule: rule);
    public static SnapSetting Feedback(bool enabled, Color colour) => new FeedbackCase(Enabled: enabled, Colour: colour);

    internal SnappingSettings Apply(SnappingSettings settings) =>
        Switch(
            state: settings,
            withRuleCase: static (state, op) => state.WithRules(rules: op.Rule),
            withoutRuleCase: static (state, op) => state.WithoutRules(rules: op.Rule),
            feedbackCase: static (state, op) => state.WithFeedback(drawFeedback: op.Enabled, colour: op.Colour));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(
    bool IncludeSelected = true,
    bool IncludeUnselected = true,
    Seq<SnapSetting> Settings = default) {
    internal SnappingSettings Native =>
        Settings.Fold(SnappingSettings.Current, static (state, op) => op.Apply(settings: state));
}

[Union]
public partial record LayoutMeasure {
    private LayoutMeasure() { }
    public sealed record SelectionCase : LayoutMeasure;
    public sealed record ObjectsCase(Seq<Guid> Ids) : LayoutMeasure;
    public static readonly LayoutMeasure Selection = new SelectionCase();
    public static LayoutMeasure Objects(params Guid[] ids) => new ObjectsCase(Ids: toSeq(ids));
}

[Union]
public partial record SnapProbe {
    private SnapProbe() { }
    public sealed record PointCase(Guid ObjectId, PointF Probe, float Radius = 32f, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record RectangleCase(Guid ObjectId, RectangleF Bounds, SnappingPolicy Policy = default) : SnapProbe;
    public sealed record ObjectCase(Guid ObjectId, SnappingPolicy Policy = default) : SnapProbe;

    public static SnapProbe Point(Guid objectId, PointF probe, float radius = 32f, SnappingPolicy policy = default) =>
        new PointCase(ObjectId: objectId, Probe: probe, Radius: radius, Policy: policy);
    public static SnapProbe Rectangle(Guid objectId, RectangleF bounds, SnappingPolicy policy = default) =>
        new RectangleCase(ObjectId: objectId, Bounds: bounds, Policy: policy);
    public static SnapProbe Object(Guid objectId, SnappingPolicy policy = default) =>
        new ObjectCase(ObjectId: objectId, Policy: policy);
}

[Union]
public partial record LayoutOp {
    private LayoutOp() { }
    public sealed record MeasureCase(LayoutMeasure Scope) : LayoutOp;
    public sealed record ArrangeCase(LayoutArrangement Arrangement) : LayoutOp;
    public sealed record SnapCase(SnapProbe Probe) : LayoutOp;

    public static LayoutOp Measure(LayoutMeasure scope) => new MeasureCase(Scope: scope);
    public static LayoutOp Arrange(LayoutArrangement arrangement) => new ArrangeCase(Arrangement: arrangement);
    public static LayoutOp Snap(SnapProbe probe) => new SnapCase(Probe: probe);
}

[Union]
public partial record LayoutResult {
    private LayoutResult() { }
    public sealed record SnapshotsResult(Seq<LayoutSnapshot> Snapshots) : LayoutResult;
    public sealed record MutationResult(Snapshot<LayoutArrangeDelta> Delta) : LayoutResult;
    public sealed record SnapResult(Option<SnappingSnapshot> Snapshot) : LayoutResult;
}

internal sealed record LayoutRequest(LayoutOp Op) : GhUiRequest<LayoutResult> {
    internal override GrasshopperUiPolicy Policy => Layout.PolicyOf(op: Op);
    internal override Fin<LayoutResult> Apply(GrasshopperUi.Scope scope) => Layout.Dispatch(op: Op).Run(scope: scope);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Layout {
    internal static GrasshopperUiPolicy PolicyOf(LayoutOp op) =>
        GrasshopperUiPolicy.Document(repaint: op switch {
            LayoutOp.ArrangeCase => RepaintRequest.Canvas,
            _ => RepaintRequest.None,
        });

    internal static GrasshopperUiIntent<LayoutResult> Dispatch(LayoutOp op) =>
        op switch {
            LayoutOp.MeasureCase measure => Measure(scope: measure.Scope),
            LayoutOp.ArrangeCase a => Arrange(arrangement: a.Arrangement).Map(delta => (LayoutResult)new LayoutResult.MutationResult(Delta: delta)),
            LayoutOp.SnapCase s => Snap(probe: s.Probe).Map(snap => (LayoutResult)new LayoutResult.SnapResult(Snapshot: snap)),
            _ => GhUi.Document(run: _ => Fin.Fail<LayoutResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Dispatch)), detail: "unknown layout op"))),
        };

    private static GrasshopperUiIntent<LayoutResult> Measure(LayoutMeasure scope) =>
        Optional(scope)
            .Map(static valid => valid.Switch(
                selectionCase: static _ => Selection().Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots)),
                objectsCase: static o => GhUi.Document(run: ctx => o.Ids.TraverseM(id => Snapshot(id: id).Run(scope: ctx))
                    .Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots))
                    .As())))
            .IfNone(GhUi.Document(run: _ => Fin.Fail<LayoutResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Measure)), detail: "layout measure is required"))));

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Arrange(LayoutArrangement arrangement) =>
        arrangement switch {
            LayoutArrangement.MoveCase move =>
                Move(id: move.Id, dx: move.Dx, dy: move.Dy)
                    .Map(delta => delta.Map(payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            LayoutArrangement.PlaceCase place =>
                Place(id: place.Id, pivot: place.Pivot)
                    .Map(delta => delta.Map(payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            LayoutArrangement.AlignCase align =>
                Align(left: align.Left, right: align.Right, fix: align.Fix)
                    .Map(delta => delta.Map(payload => new LayoutArrangeDelta(Moves: Seq(payload)))),
            LayoutArrangement.DistributeCase distribute => Distribute(axis: distribute.Axis, gap: distribute.Gap, ids: [.. distribute.Ids]),
            _ => GhUi.Document(run: _ => Fin.Fail<Snapshot<LayoutArrangeDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Arrange)), detail: "unknown layout arrangement"))),
        };

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
                    .Bind(delta => ObjectOf(scope: scope, id: id).Bind(obj => scope.NeedDocument().Map(doc =>
                        ApplyMove(obj: obj, document: doc, dx: delta.X, dy: delta.Y, snap: snap)))));

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
                select ApplyMove(obj: obj, document: doc, dx: valid.X - before.Pivot.X, dy: valid.Y - before.Pivot.Y, snap: false));

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

    internal static GrasshopperUiIntent<Snapshot<LayoutArrangeDelta>> Distribute(LayoutAxis axis, float gap, params Guid[] ids) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => Optional(ids)
                .Filter(static values => values.Length >= 2)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids"))
                .Bind(validIds => Op.Of(name: nameof(Distribute)).AcceptFinite(value: gap, detail: "non-finite gap")
                    .Bind(validGap => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                        UndoGroup bag = new(verb: "Layout", noun: string.Create(CultureInfo.InvariantCulture, $"Distribute {axis}"));
                        GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
                        Seq<(Guid Id, float Dx, float Dy)> moves = ComputeDistribution(objects: objs, ids: toSeq(validIds), axis: axis, gap: validGap);
                        return Optional(moves)
                            .Filter(static values => values.Count >= 2)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "fewer than 2 supplied ids resolved to document objects"))
                            .Bind(_ => moves.TraverseM(m => Move(id: m.Id, dx: m.Dx, dy: m.Dy, snap: false).Run(scope: scoped).Map(static s => s.Payload)).As())
                            .Bind(deltas => bag.Commit(document: doc).Map(_ => UI.Snapshot.Of(
                                payload: new LayoutArrangeDelta(Moves: deltas),
                                ownerId: Some(doc.Hash))));
                    })))));

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document(run: scope =>
            Optional(probe)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
                .Bind(valid => valid switch {
                    SnapProbe.PointCase point =>
                        from probe in Op.Of(name: nameof(Snap)).AcceptPoint(value: point.Probe, detail: "non-finite probe")
                        from radius in Op.Of(name: nameof(Snap)).AcceptFinite(value: point.Radius, detail: "radius must be finite and positive", requirePositive: true)
                        from snapped in SnapRectangle(scope: scope, id: point.ObjectId, bounds: new RectangleF(x: probe.X - radius, y: probe.Y - radius, width: radius * 2f, height: radius * 2f), policy: point.Policy)
                        select snapped,
                    SnapProbe.RectangleCase rectangle =>
                        SnapRectangle(scope: scope, id: rectangle.ObjectId, bounds: rectangle.Bounds, policy: rectangle.Policy),
                    SnapProbe.ObjectCase obj =>
                        ObjectOf(scope: scope, id: obj.ObjectId).Bind(target =>
                            from document in scope.NeedDocument()
                            from canvas in scope.NeedCanvas()
                            select SnapObject(document: document, obj: target, policy: obj.Policy, visibleLimit: canvas.VisibleFrame)),
                    _ => Fin.Fail<Option<SnappingSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "unknown snap probe")),
                }));

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
        Op.Of(name: nameof(ObjectOf)).AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectOf)), detail: "empty Guid"))
            .Bind(valid => scope.NeedObjects().Bind(objs =>
                Optional(objs.Find(instanceId: valid))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectOf)), detail: $"object {valid} not found"))));

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, float dx, float dy, bool snap) {
        IAttributes attributes = obj.Attributes;
        RectangleF bounds = attributes.AggregateBounds;
        Option<SnappingSnapshot> snapped = snap && attributes.Snappable
            ? SnapRectangle(
                document: document, obj: obj,
                bounds: new RectangleF(x: bounds.X + dx, y: bounds.Y + dy, width: bounds.Width, height: bounds.Height),
                policy: default, visibleLimit: document.Objects.AttributeBounds)
            : Option<SnappingSnapshot>.None;
        (float deltaDx, float deltaDy) = snapped.Map(static s => (s.Dx, s.Dy)).IfNone((0f, 0f));
        float effDx = dx + deltaDx;
        float effDy = dy + deltaDy;
        attributes.Move(dx: effDx, dy: effDy);
        attributes.Invalidate();
        return new LayoutMoveDelta(
            ObjectId: attributes.Owner.InstanceId,
            Dx: effDx, Dy: effDy,
            After: SnapshotOf(attributes: attributes),
            Snap: snapped);
    }

    private static Option<SnappingSnapshot> SnapRectangle(GhDocument document, IDocumentObject obj, RectangleF bounds, SnappingPolicy policy, RectangleF visibleLimit) {
        SnappingPolicy active = ActivePolicy(policy: policy);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: active.IncludeSelected,
            includeUnselected: active.IncludeUnselected,
            filter: new System.Collections.Generic.HashSet<Guid>([obj.InstanceId]));
        constraints.SnapRectangle(
            target: bounds,
            settings: active.Native,
            visibleLimit: visibleLimit,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        return SnapshotOf(x: Optional(x), y: Optional(SnappingAction.SmallerMagnitude(a: y, b: SnappingConstraints.SnapWires(target: obj, boundsOverride: bounds, settings: active.Native))));
    }

    private static Option<SnappingSnapshot> SnapObject(GhDocument document, IDocumentObject obj, SnappingPolicy policy, RectangleF visibleLimit) {
        SnappingPolicy active = ActivePolicy(policy: policy);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(
            document: document,
            includeSelected: active.IncludeSelected,
            includeUnselected: active.IncludeUnselected,
            filter: new System.Collections.Generic.HashSet<Guid>([obj.InstanceId]));
        constraints.SnapObject(
            target: obj,
            settings: active.Native,
            visibleLimit: visibleLimit,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        return SnapshotOf(x: Optional(x), y: Optional(y));
    }

    private static SnappingPolicy ActivePolicy(SnappingPolicy policy) =>
        (policy.IncludeSelected || policy.IncludeUnselected) switch {
            true => policy,
            false => policy with { IncludeSelected = true, IncludeUnselected = true },
        };

    internal static Option<SnappingSnapshot> SnapshotOf(Option<SnappingAction> x, Option<SnappingAction> y) =>
        Optional((X: x, Y: y))
            .Filter(static snap => snap.X.IsSome || snap.Y.IsSome)
            .Map(static snap => new SnappingSnapshot(
                Dx: snap.X.Map(static action => action.ΔX).IfNone(0f),
                Dy: snap.Y.Map(static action => action.ΔY).IfNone(0f),
                Magnitude: snap.X.Map(static action => action.Magnitude).IfNone(0f) + snap.Y.Map(static action => action.Magnitude).IfNone(0f),
                XLabel: snap.X.Map(static action => action.LabelText).IfNone(string.Empty),
                YLabel: snap.Y.Map(static action => action.LabelText).IfNone(string.Empty),
                Lines: snap.X.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>()) + snap.Y.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>()),
                LabelPoint: snap.X.Map(static action => action.LabelPoint) | snap.Y.Map(static action => action.LabelPoint),
                LabelAnchor: snap.X.Map(static action => action.LabelAnchor) | snap.Y.Map(static action => action.LabelAnchor)));

    private readonly record struct LayoutAlignmentCase(
        Func<Type, Type, bool> Supports,
        Func<IDocumentObject, IDocumentObject, OCD.Fixed, Fin<Unit>> Align) {
        internal static LayoutAlignmentCase Pair<TL, TR>(Action<TL, TR, OCD.Fixed> align)
            where TL : class
            where TR : class =>
            new(
                Supports: static (l, r) => typeof(TL).IsAssignableFrom(c: l) && typeof(TR).IsAssignableFrom(c: r),
                Align: (a, b, fix) => (a, b) switch {
                    (TL leftCast, TR rightCast) =>
                        Try.lift(f: () => { align(arg1: leftCast, arg2: rightCast, arg3: fix); return unit; })
                            .Run()
                            .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: "Align"), detail: $"OCD.AlignObjects threw for ({typeof(TL).Name}, {typeof(TR).Name}): {error.Message}")),
                    _ => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: "Align"), detail: $"type mismatch")),
                });
    }

    private static readonly Seq<LayoutAlignmentCase> AlignmentCases = Seq(
        LayoutAlignmentCase.Pair<Component, Component>(align: static (a, b, fix) => OCD.AlignObjects(a, b, fix)),
        LayoutAlignmentCase.Pair<IParameter, Component>(align: static (a, b, fix) => OCD.AlignObjects(a, b, fix)),
        LayoutAlignmentCase.Pair<Component, IParameter>(align: static (a, b, fix) => OCD.AlignObjects(a, b, fix)),
        LayoutAlignmentCase.Pair<IParameter, IParameter>(align: static (a, b, fix) => OCD.AlignObjects(a, b, fix)));

    private static Fin<Unit> AlignVia(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) =>
        AlignmentCases
            .Choose(c => c.Supports(arg1: a.GetType(), arg2: b.GetType())
                ? Some(c.Align(arg1: a, arg2: b, arg3: fix))
                : Option<Fin<Unit>>.None)
            .Head
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(AlignVia)), detail: $"unsupported pair ({a.GetType().Name}, {b.GetType().Name})"))
            .Bind(static r => r)
            .Map(_ => { a.Attributes.Invalidate(); b.Attributes.Invalidate(); return unit; });

    private static Seq<(Guid Id, float Dx, float Dy)> ComputeDistribution(GhObjectList objects, Seq<Guid> ids, LayoutAxis axis, float gap) {
        Seq<(Guid Id, RectangleF Bounds)> snapshots = ids.Choose(id => Optional(objects.Find(instanceId: id))
            .Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)));
        Seq<(Guid Id, RectangleF Bounds)> sorted = toSeq(snapshots.OrderBy(s => axis.Origin(bounds: s.Bounds)));
        return sorted.Head
            .Map(head => sorted.Fold(
                initialState: (Axis: axis, Gap: gap, Cursor: axis.Origin(bounds: head.Bounds), Moves: Seq<(Guid Id, float Dx, float Dy)>()),
                f: static (state, s) => (
                    state.Axis,
                    state.Gap,
                    Cursor: state.Cursor + state.Axis.Span(bounds: s.Bounds) + state.Gap,
                    Moves: state.Axis.Delta(cursor: state.Cursor, bounds: s.Bounds) switch {
                        (float dx, float dy) => state.Moves.Add((s.Id, dx, dy)),
                    }))
                .Moves)
            .IfNone(() => Seq<(Guid Id, float Dx, float Dy)>());
    }
}

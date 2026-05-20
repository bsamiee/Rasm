using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Canvas;
using Grasshopper2.Undo.Actions;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum LayoutAxis { Horizontal, Vertical }

[Union]
public partial record LayoutArrangement {
    private LayoutArrangement() { }
    public sealed record MoveCase(Guid Id, float Dx, float Dy) : LayoutArrangement;
    public sealed record AlignCase(Guid Left, Guid Right, OCD.Fixed Fix) : LayoutArrangement;
    public sealed record DistributeCase(LayoutAxis Axis, float Gap, Seq<Guid> Ids) : LayoutArrangement;
    public static LayoutArrangement Move(Guid id, float dx, float dy) => new MoveCase(Id: id, Dx: dx, Dy: dy);
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
public readonly record struct SnappingSnapshot(float Dx, float Dy, string XLabel, string YLabel);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingPolicy(
    bool IncludeSelected = true,
    bool IncludeUnselected = true,
    Option<SnappingSettings> Settings = default);

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
    public sealed record SnapshotCase(Seq<Guid> Ids) : LayoutOp;
    public sealed record ArrangeCase(LayoutArrangement Arrangement) : LayoutOp;
    public sealed record SnapCase(SnapProbe Probe) : LayoutOp;

    public static LayoutOp Snapshot(params Guid[] ids) => new SnapshotCase(Ids: toSeq(ids));
    public static LayoutOp Arrange(LayoutArrangement arrangement) => new ArrangeCase(Arrangement: arrangement);
    public static LayoutOp Snap(SnapProbe probe) => new SnapCase(Probe: probe);
}

[Union]
public partial record LayoutResult {
    private LayoutResult() { }
    public sealed record SnapshotsResult(Seq<LayoutSnapshot> Snapshots) : LayoutResult;
    public sealed record MutationResult(Snapshot<LayoutMoveDelta> Delta) : LayoutResult;
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
            LayoutOp.SnapshotCase snapshot => Snapshots(ids: snapshot.Ids),
            LayoutOp.ArrangeCase a => Arrange(arrangement: a.Arrangement).Map(delta => (LayoutResult)new LayoutResult.MutationResult(Delta: delta)),
            LayoutOp.SnapCase s => Snap(probe: s.Probe).Map(snap => (LayoutResult)new LayoutResult.SnapResult(Snapshot: snap)),
            _ => GhUi.Document<LayoutResult>(run: _ => Fin.Fail<LayoutResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Dispatch)), detail: "unknown layout op"))),
        };

    private static GrasshopperUiIntent<LayoutResult> Snapshots(Seq<Guid> ids) =>
        GhUi.Document<LayoutResult>(run: scope =>
            ids.IsEmpty
                ? Selection().Run(scope: scope).Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots))
                : ids.TraverseM(id => Snapshot(id: id).Run(scope: scope))
                    .Map(snapshots => (LayoutResult)new LayoutResult.SnapshotsResult(Snapshots: snapshots))
                    .As());

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Arrange(LayoutArrangement arrangement) =>
        arrangement switch {
            LayoutArrangement.MoveCase move => Move(id: move.Id, dx: move.Dx, dy: move.Dy),
            LayoutArrangement.AlignCase align => Align(left: align.Left, right: align.Right, fix: align.Fix),
            LayoutArrangement.DistributeCase distribute => Distribute(axis: distribute.Axis, gap: distribute.Gap, ids: [.. distribute.Ids]),
            _ => GhUi.Document<Snapshot<LayoutMoveDelta>>(run: _ => Fin.Fail<Snapshot<LayoutMoveDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Arrange)), detail: "unknown layout arrangement"))),
        };

    internal static GrasshopperUiIntent<LayoutSnapshot> Snapshot(Guid id) =>
        GhUi.Document<LayoutSnapshot>(run: scope =>
            ObjectOf(scope: scope, id: id).Map(obj => SnapshotOf(attributes: obj.Attributes)));

    internal static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        GhUi.Document<Seq<LayoutSnapshot>>(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.SelectedObjects.Select(o => SnapshotOf(attributes: o.Attributes)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Move(Guid id, float dx, float dy, bool snap = true) =>
        GrasshopperUi.Mutate<LayoutMoveDelta>(
            op: Op.Of(name: nameof(Move)),
            repaint: RepaintRequest.Object(id: id),
            undo: UndoStrategy.Manual(record: s => s.Objects.Match(
                Some: objs => Optional(objs.Find(instanceId: id))
                    .Map(static obj => new UndoEntry(Verb: "Layout", Noun: "Move", Actions: Seq<UndoAction>(new PivotAction(obj: obj))))
                    .IfNone(new UndoEntry(Verb: "Layout", Noun: "Move", Actions: Seq<UndoAction>())),
                None: () => new UndoEntry(Verb: "Layout", Noun: "Move", Actions: Seq<UndoAction>()))),
            mutate: scope =>
                Optional((Dx: dx, Dy: dy))
                    .Filter(static d => float.IsFinite(d.Dx) && float.IsFinite(d.Dy))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Move)), detail: "non-finite delta"))
                    .Bind(delta => ObjectOf(scope: scope, id: id).Bind(obj => scope.NeedDocument().Map(doc =>
                        ApplyMove(obj: obj, document: doc, dx: delta.Dx, dy: delta.Dy, snap: snap)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Align(Guid left, Guid right, OCD.Fixed fix = OCD.Fixed.None) =>
        GrasshopperUi.Mutate<LayoutMoveDelta>(
            op: Op.Of(name: nameof(Align)),
            repaint: RepaintRequest.Canvas,
            undo: UndoStrategy.Manual(record: s => s.Objects.Match(
                Some: objs => {
                    IDocumentObject? l = objs.Find(instanceId: left);
                    IDocumentObject? r = objs.Find(instanceId: right);
                    return l is not null && r is not null
                        ? new UndoEntry(Verb: "Layout", Noun: "Align", Actions: Seq<UndoAction>(new PivotAction(obj: l.FoundingObject), new PivotAction(obj: r.FoundingObject)))
                        : new UndoEntry(Verb: "Layout", Noun: "Align", Actions: Seq<UndoAction>());
                },
                None: () => new UndoEntry(Verb: "Layout", Noun: "Align", Actions: Seq<UndoAction>()))),
            mutate: scope =>
                from leftObj in ObjectOf(scope: scope, id: left)
                from rightObj in ObjectOf(scope: scope, id: right)
                from _ in AlignVia(a: leftObj, b: rightObj, fix: fix)
                select new LayoutMoveDelta(ObjectId: right, Dx: 0f, Dy: 0f, After: SnapshotOf(attributes: rightObj.Attributes), Snap: Option<SnappingSnapshot>.None));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Distribute(LayoutAxis axis, float gap, params Guid[] ids) =>
        GhUi.Document<Snapshot<LayoutMoveDelta>>(
            repaint: RepaintRequest.Canvas,
            run: scope => Optional(ids)
                .Filter(static values => values.Length >= 2)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids"))
                .Bind(validIds => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                    UndoGroup bag = new(verb: "Layout", noun: $"Distribute {axis}");
                    GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
                    Seq<(Guid Id, float Dx, float Dy)> moves = ComputeDistribution(objects: objs, ids: toSeq(validIds), axis: axis, gap: gap);
                    return moves.Head
                        .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "no supplied ids resolved to document objects"))
                        .Bind(_ => moves.TraverseM(m => Move(id: m.Id, dx: m.Dx, dy: m.Dy, snap: false).Run(scope: scoped).Map(static s => s.Payload)).As())
                        .Bind(deltas => deltas.Rev().Head
                            .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(Distribute)), detail: "distribution produced no deltas")))
                        .Bind(last => bag.Commit(document: doc).Map(_ => global::Rasm.Grasshopper.UI.Snapshot.Of<LayoutMoveDelta>(payload: last, ownerId: Some(doc.Hash))));
                }))));

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        GhUi.Document<Option<SnappingSnapshot>>(run: scope =>
            Optional(probe)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "snap probe is required"))
                .Bind(valid => valid switch {
                    SnapProbe.PointCase point =>
                        Optional((point.Probe, point.Radius))
                            .Filter(static p => float.IsFinite(p.Probe.X) && float.IsFinite(p.Probe.Y) && float.IsFinite(p.Radius) && p.Radius > 0f)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Snap)), detail: "invalid point probe"))
                            .Bind(p => SnapRectangle(scope: scope, id: point.ObjectId, bounds: new RectangleF(x: p.Probe.X - p.Radius, y: p.Probe.Y - p.Radius, width: p.Radius * 2f, height: p.Radius * 2f), policy: point.Policy)),
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
    private static Fin<Option<SnappingSnapshot>> SnapRectangle(GrasshopperUi.Scope scope, Guid id, RectangleF bounds, SnappingPolicy policy) =>
        Optional((Id: id, Bounds: bounds))
            .Filter(static p => p.Id != Guid.Empty
                && float.IsFinite(p.Bounds.X)
                && float.IsFinite(p.Bounds.Y)
                && float.IsFinite(p.Bounds.Width)
                && float.IsFinite(p.Bounds.Height)
                && p.Bounds.Width > 0f
                && p.Bounds.Height > 0f)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SnapRectangle)), detail: "invalid rectangle probe"))
            .Bind(valid => ObjectOf(scope: scope, id: valid.Id).Bind(obj =>
                from document in scope.NeedDocument()
                from canvas in scope.NeedCanvas()
                select SnapRectangle(document: document, obj: obj, bounds: valid.Bounds, policy: policy, visibleLimit: canvas.VisibleFrame)));

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        Optional(id).Filter(static g => g != Guid.Empty)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectOf)), detail: "empty Guid"))
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
        Option<SnappingSnapshot> snapped = snap && attributes.Snappable
            ? SnapMove(document: document, obj: obj, dx: dx, dy: dy, policy: default)
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

    private static Option<SnappingSnapshot> SnapMove(GhDocument document, IDocumentObject obj, float dx, float dy, SnappingPolicy policy) {
        IAttributes attributes = obj.Attributes;
        RectangleF bounds = attributes.AggregateBounds;
        RectangleF target = new(x: bounds.X + dx, y: bounds.Y + dy, width: bounds.Width, height: bounds.Height);
        return SnapRectangle(document: document, obj: obj, bounds: target, policy: policy, visibleLimit: document.Objects.AttributeBounds);
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
            settings: active.Settings.IfNone(SnappingSettings.Current),
            visibleLimit: visibleLimit,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        return SnapshotOf(x: Optional(x), y: Optional(y));
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
            settings: active.Settings.IfNone(SnappingSettings.Current),
            visibleLimit: visibleLimit,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        return SnapshotOf(x: Optional(x), y: Optional(y));
    }

    private static SnappingPolicy ActivePolicy(SnappingPolicy policy) =>
        Optional(policy)
            .Filter(static value => value.IncludeSelected || value.IncludeUnselected)
            .IfNone(policy with { IncludeSelected = true, IncludeUnselected = true });

    internal static Option<SnappingSnapshot> SnapshotOf(Option<SnappingAction> x, Option<SnappingAction> y) =>
        Optional((X: x, Y: y))
            .Filter(static snap => snap.X.IsSome || snap.Y.IsSome)
            .Map(static snap => new SnappingSnapshot(
                Dx: snap.X.Map(static action => action.ΔX).IfNone(0f),
                Dy: snap.Y.Map(static action => action.ΔY).IfNone(0f),
                XLabel: snap.X.Map(static action => action.LabelText).IfNone(string.Empty),
                YLabel: snap.Y.Map(static action => action.LabelText).IfNone(string.Empty)));

    private readonly record struct LayoutAlignmentCase(
        Func<Type, Type, bool> Supports,
        Func<IDocumentObject, IDocumentObject, OCD.Fixed, Fin<Unit>> Align) {
        internal static LayoutAlignmentCase Pair<TL, TR>(System.Action<TL, TR, OCD.Fixed> align)
            where TL : class
            where TR : class =>
            new(
                Supports: static (l, r) => typeof(TL).IsAssignableFrom(c: l) && typeof(TR).IsAssignableFrom(c: r),
                Align: (a, b, fix) => (a, b) switch {
                    (TL leftCast, TR rightCast) =>
                        Try.lift<Unit>(f: () => { align(arg1: leftCast, arg2: rightCast, arg3: fix); return unit; })
                            .Run()
                            .MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: "Align"), detail: $"OCD.AlignObjects threw for ({typeof(TL).Name}, {typeof(TR).Name})")),
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

    private readonly record struct LayoutAxisCase(
        LayoutAxis Axis,
        Func<RectangleF, float> Origin,
        Func<RectangleF, float> Span,
        Func<float, RectangleF, (float Dx, float Dy)> Delta);
    private static readonly LayoutAxisCase HorizontalAxis = new(
        Axis: LayoutAxis.Horizontal,
        Origin: static bounds => bounds.Left,
        Span: static bounds => bounds.Width,
        Delta: static (cursor, bounds) => (Dx: cursor - bounds.Left, Dy: 0f));
    private static readonly Seq<LayoutAxisCase> AxisCases = Seq(
        HorizontalAxis,
        new LayoutAxisCase(
            Axis: LayoutAxis.Vertical,
            Origin: static bounds => bounds.Top,
            Span: static bounds => bounds.Height,
            Delta: static (cursor, bounds) => (Dx: 0f, Dy: cursor - bounds.Top)));
    private static LayoutAxisCase AxisCaseOf(LayoutAxis axis) =>
        AxisCases.Find(match => match.Axis == axis).IfNone(HorizontalAxis);

    private static Seq<(Guid Id, float Dx, float Dy)> ComputeDistribution(GhObjectList objects, Seq<Guid> ids, LayoutAxis axis, float gap) {
        LayoutAxisCase projection = AxisCaseOf(axis: axis);
        Seq<(Guid Id, RectangleF Bounds)> snapshots = ids.Choose(id => Optional(objects.Find(instanceId: id))
            .Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)));
        Seq<(Guid Id, RectangleF Bounds)> sorted = toSeq(snapshots.OrderBy(s => projection.Origin(arg: s.Bounds)));
        return sorted.Head
            .Map(head => sorted.Fold(
                initialState: (Cursor: projection.Origin(arg: head.Bounds), Moves: Seq<(Guid Id, float Dx, float Dy)>()),
                f: (state, s) => (
                    Cursor: state.Cursor + projection.Span(arg: s.Bounds) + gap,
                    Moves: (s.Id,
                        projection.Delta(arg1: state.Cursor, arg2: s.Bounds).Dx,
                        projection.Delta(arg1: state.Cursor, arg2: s.Bounds).Dy).Cons(state.Moves)))
                .Moves.Rev())
            .IfNone(() => Seq<(Guid Id, float Dx, float Dy)>());
    }
}

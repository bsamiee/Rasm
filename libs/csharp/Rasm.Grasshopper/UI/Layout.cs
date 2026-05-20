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

// --- [TYPES] -----------------------------------------------------------------------------
public enum LayoutAxis { Horizontal, Vertical }

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutSnapshot(Guid ObjectId, PointF Pivot, RectangleF Bounds, RectangleF AggregateBounds, bool Snappable);

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

public abstract record LayoutRequest<T> : GhUiRequest<T> {
    public sealed record ObjectSnapshot(Guid Id) : LayoutRequest<LayoutSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<LayoutSnapshot> Apply(GrasshopperUi.Scope scope) => Layout.Snapshot(id: Id).Run(scope: scope);
    }
    public sealed record Selection : LayoutRequest<Seq<LayoutSnapshot>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<Seq<LayoutSnapshot>> Apply(GrasshopperUi.Scope scope) => Layout.Selection().Run(scope: scope);
    }
    public sealed record Move(Guid Id, float Dx, float Dy) : LayoutRequest<Snapshot<LayoutMoveDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Object(id: Id));
        internal override Fin<Snapshot<LayoutMoveDelta>> Apply(GrasshopperUi.Scope scope) => Layout.Move(id: Id, dx: Dx, dy: Dy).Run(scope: scope);
    }
    public sealed record Align(Guid Left, Guid Right, OCD.Fixed Fix = OCD.Fixed.None) : LayoutRequest<Snapshot<LayoutMoveDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas);
        internal override Fin<Snapshot<LayoutMoveDelta>> Apply(GrasshopperUi.Scope scope) => Layout.Align(left: Left, right: Right, fix: Fix).Run(scope: scope);
    }
    public sealed record Distribute(LayoutAxis Axis, float Gap, Seq<Guid> Ids) : LayoutRequest<Snapshot<LayoutMoveDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas);
        internal override Fin<Snapshot<LayoutMoveDelta>> Apply(GrasshopperUi.Scope scope) => Layout.Distribute(axis: Axis, gap: Gap, ids: [.. Ids]).Run(scope: scope);
    }
    public sealed record Snapping(SnapProbe Probe) : LayoutRequest<Option<SnappingSnapshot>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<Option<SnappingSnapshot>> Apply(GrasshopperUi.Scope scope) => Layout.Snap(probe: Probe).Run(scope: scope);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class Layout {
    internal static GrasshopperUiIntent<LayoutSnapshot> Snapshot(Guid id) =>
        IntentFactory.Document<LayoutSnapshot>(run: scope =>
            ObjectOf(scope: scope, id: id).Map(obj => SnapshotOf(attributes: obj.Attributes)));

    internal static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        IntentFactory.Document<Seq<LayoutSnapshot>>(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.SelectedObjects.Select(o => SnapshotOf(attributes: o.Attributes)))));

    internal static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Move(Guid id, float dx, float dy) =>
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
                        ApplyMove(obj: obj, document: doc, dx: delta.Dx, dy: delta.Dy)))));

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
        IntentFactory.Document<Snapshot<LayoutMoveDelta>>(
            repaint: RepaintRequest.Canvas,
            run: scope => Optional(ids)
                .Filter(static values => values.Length >= 2)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids"))
                .Bind(validIds => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                    UndoGroup bag = new(verb: "Layout", noun: $"Distribute {axis}");
                    GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
                    Seq<(Guid Id, float Dx, float Dy)> moves = ComputeDistribution(objects: objs, ids: toSeq(validIds), axis: axis, gap: gap);
                    return moves.Fold(
                        initialState: Fin.Succ<LayoutMoveDelta>(default),
                        f: (state, m) =>
                            from _ in state
                            from delta in Move(id: m.Id, dx: m.Dx, dy: m.Dy).Run(scope: scoped).Map(static s => s.Payload)
                            select delta)
                        .Bind(last => bag.Commit(document: doc).Map(_ => global::Rasm.Grasshopper.UI.Snapshot.Of<LayoutMoveDelta>(payload: last, ownerId: Some(doc.Hash))));
                }))));

    internal static GrasshopperUiIntent<Option<SnappingSnapshot>> Snap(SnapProbe probe) =>
        IntentFactory.Document<Option<SnappingSnapshot>>(run: scope =>
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

    // --- [OPERATIONS] ----------------------------------------------------------------------
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

    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, float dx, float dy) {
        IAttributes attributes = obj.Attributes;
        Option<SnappingSnapshot> snap = attributes.Snappable
            ? SnapMove(document: document, obj: obj, dx: dx, dy: dy, policy: default)
            : Option<SnappingSnapshot>.None;
        (float deltaDx, float deltaDy) = snap.Map(static s => (s.Dx, s.Dy)).IfNone((0f, 0f));
        float effDx = dx + deltaDx;
        float effDy = dy + deltaDy;
        attributes.Move(dx: effDx, dy: effDy);
        attributes.Invalidate();
        return new LayoutMoveDelta(
            ObjectId: attributes.Owner.InstanceId,
            Dx: effDx, Dy: effDy,
            After: SnapshotOf(attributes: attributes),
            Snap: snap);
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
        policy.IncludeSelected || policy.IncludeUnselected
            ? policy
            : policy with { IncludeSelected = true, IncludeUnselected = true };

    private static Option<SnappingSnapshot> SnapshotOf(Option<SnappingAction> x, Option<SnappingAction> y) {
        Option<SnappingAction> snapX = x;
        Option<SnappingAction> snapY = y;
        return (snapX.IsSome || snapY.IsSome)
            ? Some(new SnappingSnapshot(
                Dx: snapX.Map(static a => a.ΔX).IfNone(0f),
                Dy: snapY.Map(static a => a.ΔY).IfNone(0f),
                XLabel: snapX.Map(static a => a.LabelText).IfNone(string.Empty),
                YLabel: snapY.Map(static a => a.LabelText).IfNone(string.Empty)))
            : Option<SnappingSnapshot>.None;
    }

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

    private static Seq<(Guid Id, float Dx, float Dy)> ComputeDistribution(GhObjectList objects, Seq<Guid> ids, LayoutAxis axis, float gap) {
        Seq<(Guid Id, RectangleF Bounds)> snapshots = ids.Choose(id => Optional(objects.Find(instanceId: id))
            .Map(o => (Id: id, Bounds: o.Attributes.AggregateBounds)));
        Seq<(Guid Id, RectangleF Bounds)> sorted = toSeq(snapshots.OrderBy(s => axis switch {
            LayoutAxis.Horizontal => s.Bounds.Left,
            LayoutAxis.Vertical => s.Bounds.Top,
            _ => s.Bounds.Left,
        }));
        return sorted.Head
            .Map(head => sorted.Fold(
                initialState: (Cursor: axis switch {
                    LayoutAxis.Horizontal => head.Bounds.Left,
                    LayoutAxis.Vertical => head.Bounds.Top,
                    _ => head.Bounds.Left,
                }, Moves: Seq<(Guid Id, float Dx, float Dy)>()),
                f: (state, s) => (
                    Cursor: state.Cursor + (axis switch {
                        LayoutAxis.Horizontal => s.Bounds.Width,
                        LayoutAxis.Vertical => s.Bounds.Height,
                        _ => s.Bounds.Width,
                    }) + gap,
                    Moves: state.Moves.Add((s.Id,
                        Dx: axis switch {
                            LayoutAxis.Horizontal => state.Cursor - s.Bounds.Left,
                            _ => 0f,
                        },
                        Dy: axis switch {
                            LayoutAxis.Vertical => state.Cursor - s.Bounds.Top,
                            _ => 0f,
                        }))))
                .Moves.Rev())
            .IfNone(() => Seq<(Guid Id, float Dx, float Dy)>());
    }
}

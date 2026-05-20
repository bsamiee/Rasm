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
public readonly record struct SnapCandidate(PointF Target, string XLabel, string YLabel);

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Layout {
    public static GrasshopperUiIntent<LayoutSnapshot> Snapshot(Guid id) =>
        IntentFactory.Document<LayoutSnapshot>(run: scope =>
            ObjectOf(scope: scope, id: id).Map(obj => SnapshotOf(attributes: obj.Attributes)));

    public static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        IntentFactory.Document<Seq<LayoutSnapshot>>(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.SelectedObjects.Select(o => SnapshotOf(attributes: o.Attributes)))));

    // L-005 fix: snap is inferred from Attributes.Snappable. No caller knob.
    public static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Move(Guid id, float dx, float dy) =>
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

    public static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Align(Guid left, Guid right, OCD.Fixed fix = OCD.Fixed.None) =>
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

    public static GrasshopperUiIntent<Snapshot<LayoutMoveDelta>> Distribute(LayoutAxis axis, float gap, params Guid[] ids) =>
        IntentFactory.Document<Snapshot<LayoutMoveDelta>>(
            repaint: RepaintRequest.Canvas,
            run: scope => ids.Length < 2
                ? Fin.Fail<Snapshot<LayoutMoveDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Distribute)), detail: "Distribute requires at least 2 ids"))
                : scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                    UndoGroup bag = new(verb: "Layout", noun: $"Distribute {axis}");
                    GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
                    Seq<(Guid Id, float Dx, float Dy)> moves = ComputeDistribution(objects: objs, ids: toSeq(ids), axis: axis, gap: gap);
                    return moves.Fold(
                        initialState: Fin.Succ<LayoutMoveDelta>(default),
                        f: (state, m) =>
                            from _ in state
                            from delta in Move(id: m.Id, dx: m.Dx, dy: m.Dy).Run(scope: scoped).Map(static s => s.Payload)
                            select delta)
                        .Bind(last => bag.Commit(document: doc).Map(_ => Snapshot.Of<LayoutMoveDelta>(payload: last, ownerId: Some(doc.Hash))));
                })));

    public static GrasshopperUiIntent<Option<SnappingSnapshot>> SnapQuery(Guid id, PointF probe, float radius = 32f) =>
        IntentFactory.Document<Option<SnappingSnapshot>>(run: scope =>
            Optional(probe).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SnapQuery)), detail: "non-finite probe"))
                .Bind(_ => ObjectOf(scope: scope, id: id).Bind(obj => scope.NeedDocument().Map(doc =>
                    SnapProbe(document: doc, obj: obj, probe: probe, radius: radius)))));

    // --- [OPERATIONS] ----------------------------------------------------------------------
    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id) =>
        Optional(id).Filter(static g => g != Guid.Empty)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectOf)), detail: "empty Guid"))
            .Bind(valid => scope.NeedObjects().Bind(objs =>
                Optional(objs.Find(instanceId: valid))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectOf)), detail: $"object {valid} not found"))));

    // L-002 fix: SnapshotOf NO LONGER calls Attributes.Layout(). Bounds read directly.
    private static LayoutSnapshot SnapshotOf(IAttributes attributes) =>
        new(ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);

    // L-003 / L-005 fix: capture pre-state, compute snap only when Snappable, apply Move, return delta.
    // Undo recording happens AFTER successful mutate via UndoStrategy.Manual (RecordUndo in Ui.cs).
    private static LayoutMoveDelta ApplyMove(IDocumentObject obj, GhDocument document, float dx, float dy) {
        IAttributes attributes = obj.Attributes;
        Option<SnappingSnapshot> snap = attributes.Snappable
            ? SnapMove(document: document, obj: obj, dx: dx, dy: dy)
            : Option<SnappingSnapshot>.None;
        (float deltaDx, float deltaDy) = snap.Match(
            Some: s => (s.Dx, s.Dy),
            None: () => (0f, 0f));
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

    private static Option<SnappingSnapshot> SnapMove(GhDocument document, IDocumentObject obj, float dx, float dy) {
        IAttributes attributes = obj.Attributes;
        RectangleF bounds = attributes.AggregateBounds;
        RectangleF target = new(x: bounds.X + dx, y: bounds.Y + dy, width: bounds.Width, height: bounds.Height);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(document, obj.InstanceId);
        constraints.SnapRectangle(
            target: target,
            settings: SnappingSettings.Current,
            visibleLimit: document.Objects.AttributeBounds,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        Option<SnappingAction> snapX = Optional(x);
        Option<SnappingAction> snapY = Optional(y);
        return (snapX.IsSome || snapY.IsSome)
            ? Some(new SnappingSnapshot(
                Dx: snapX.Map(static a => a.ΔX).IfNone(0f),
                Dy: snapY.Map(static a => a.ΔY).IfNone(0f),
                XLabel: snapX.Map(static a => a.LabelText).IfNone(string.Empty),
                YLabel: snapY.Map(static a => a.LabelText).IfNone(string.Empty)))
            : Option<SnappingSnapshot>.None;
    }

    private static SnappingSnapshot SnapProbe(GhDocument document, IDocumentObject obj, PointF probe, float radius) {
        RectangleF probeBounds = new(x: probe.X - radius, y: probe.Y - radius, width: radius * 2f, height: radius * 2f);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(document, obj.InstanceId);
        constraints.SnapRectangle(
            target: probeBounds,
            settings: SnappingSettings.Current,
            visibleLimit: document.Objects.AttributeBounds,
            snapX: out SnappingAction x,
            snapY: out SnappingAction y);
        Option<SnappingAction> snapX = Optional(x);
        Option<SnappingAction> snapY = Optional(y);
        return new SnappingSnapshot(
            Dx: snapX.Map(static a => a.ΔX).IfNone(0f),
            Dy: snapY.Map(static a => a.ΔY).IfNone(0f),
            XLabel: snapX.Map(static a => a.LabelText).IfNone(string.Empty),
            YLabel: snapY.Map(static a => a.LabelText).IfNone(string.Empty));
    }

    // L-001 fix: 4-arm tuple switch becomes a Seq<LayoutAlignmentCase> lattice (matches IntersectionCase pattern).
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
        if (snapshots.IsEmpty) return Seq<(Guid, float, float)>();
        Seq<(Guid Id, RectangleF Bounds)> sorted = toSeq(snapshots.OrderBy(s => axis == LayoutAxis.Horizontal ? s.Bounds.Left : s.Bounds.Top));
        float cursor = axis == LayoutAxis.Horizontal ? sorted.Head().Bounds.Left : sorted.Head().Bounds.Top;
        return sorted.Map(s => {
            float dx = axis == LayoutAxis.Horizontal ? cursor - s.Bounds.Left : 0f;
            float dy = axis == LayoutAxis.Horizontal ? 0f : cursor - s.Bounds.Top;
            cursor += (axis == LayoutAxis.Horizontal ? s.Bounds.Width : s.Bounds.Height) + gap;
            return (s.Id, Dx: dx, Dy: dy);
        });
    }
}

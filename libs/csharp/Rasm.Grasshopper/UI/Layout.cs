using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Canvas;
using Grasshopper2.Undo.Actions;
using UiShape = Grasshopper2.UI.Skinning.Shape;

namespace Rasm.Grasshopper.UI;

[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutSnapshot(Guid ObjectId, PointF Pivot, RectangleF Bounds, RectangleF AggregateBounds, bool Snappable);

[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutMoveSnapshot(Guid ObjectId, float Dx, float Dy, LayoutSnapshot Layout, Option<SnappingSnapshot> Snap);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SnappingSnapshot(float Dx, float Dy, string XLabel, string YLabel);

// --- [INTENTS] --------------------------------------------------------------------------
public static class LayoutIntent {
    public static GrasshopperUiIntent<LayoutSnapshot> Object(Guid id) =>
        new(
            run: scope => ObjectOf(scope: scope, id: id, name: nameof(Object)).Map(static obj => SnapshotOf(attributes: obj.Attributes)),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<LayoutMoveSnapshot> Move(Guid id, float dx, float dy, bool snap = false) =>
        new(
            run: scope => Optional((Id: id, Dx: dx, Dy: dy))
                .Filter(static move => move.Id != Guid.Empty && float.IsFinite(move.Dx) && float.IsFinite(move.Dy))
                .ToFin(Fail: Op.Of(name: nameof(Move)).InvalidInput())
                .Bind(move =>
                    from obj in ObjectOf(scope: scope, id: move.Id, name: nameof(Move))
                    from document in scope.Document.ToFin(Fail: Op.Of(name: nameof(Move)).InvalidInput())
                    select Move(obj: obj, document: document, dx: move.Dx, dy: move.Dy, snap: snap)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<Unit> Align(Guid left, Guid right, OCD.Fixed fix = OCD.Fixed.None) =>
        new(
            run: scope =>
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                from document in scope.Document.ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                from a in Optional(objects.Find(instanceId: left)).ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                from b in Optional(objects.Find(instanceId: right)).ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                from aligned in Align(document: document, a: a, b: b, fix: fix)
                select aligned,
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<Seq<LayoutSnapshot>> Selection() =>
        new(
            run: scope => scope.Objects
                .ToFin(Fail: Op.Of(name: nameof(Selection)).InvalidInput())
                .Map(objects => toSeq(objects.SelectedObjects).Map(static obj => SnapshotOf(attributes: obj.Attributes))),
            policy: GrasshopperUiPolicy.Document());

    private static Fin<IDocumentObject> ObjectOf(GrasshopperUi.Scope scope, Guid id, string name) =>
        from valid in Optional(id).Filter(static value => value != Guid.Empty).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
        from obj in Optional(objects.Find(instanceId: valid)).ToFin(Fail: Op.Of(name: name).InvalidInput())
        select obj;

    private static LayoutSnapshot SnapshotOf(IAttributes attributes) {
        attributes.Layout(shape: UiShape.Default);
        return new(
            ObjectId: attributes.Owner.InstanceId,
            Pivot: attributes.Pivot,
            Bounds: attributes.Bounds,
            AggregateBounds: attributes.AggregateBounds,
            Snappable: attributes.Snappable);
    }

    private static LayoutMoveSnapshot Move(IDocumentObject obj, Document document, float dx, float dy, bool snap) {
        IAttributes attributes = obj.Attributes;
        LayoutSnapshot before = SnapshotOf(attributes: attributes);
        Option<SnappingSnapshot> action = snap switch {
            true => Snap(document: document, obj: obj, dx: dx, dy: dy),
            false => None,
        };
        (float deltaDx, float deltaDy) = action.Map(static found => (found.Dx, found.Dy)).IfNone((0f, 0f));
        float movedDx = dx + deltaDx;
        float movedDy = dy + deltaDy;
        Grasshopper2.Undo.ActionList actions = new([new PivotAction(obj: obj)]);
        attributes.Move(dx: movedDx, dy: movedDy);
        attributes.Invalidate();
        document.Undo.Do(name: ("Move", "Object"), actions: actions);
        return new(ObjectId: before.ObjectId, Dx: movedDx, Dy: movedDy, Layout: SnapshotOf(attributes: attributes), Snap: action);
    }

    private static Option<SnappingSnapshot> Snap(Document document, IDocumentObject obj, float dx, float dy) {
        IAttributes attributes = obj.Attributes;
        attributes.Layout(shape: UiShape.Default);
        RectangleF bounds = attributes.AggregateBounds;
        RectangleF target = new(x: bounds.X + dx, y: bounds.Y + dy, width: bounds.Width, height: bounds.Height);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(document, obj.InstanceId);
        constraints.SnapRectangle(target: target, settings: SnappingSettings.Current, visibleLimit: document.Objects.AttributeBounds, snapX: out SnappingAction x, snapY: out SnappingAction y);
        Option<SnappingAction> snapX = Optional(x);
        Option<SnappingAction> snapY = Optional(y);
        return (snapX.IsSome || snapY.IsSome) switch {
            true => Some(new SnappingSnapshot(
                Dx: snapX.Map(static action => action.ΔX).IfNone(0),
                Dy: snapY.Map(static action => action.ΔY).IfNone(0),
                XLabel: snapX.Map(static action => action.LabelText).IfNone(string.Empty),
                YLabel: snapY.Map(static action => action.LabelText).IfNone(string.Empty))),
            false => None,
        };
    }

    private static Fin<Unit> Align(Document document, IDocumentObject a, IDocumentObject b, OCD.Fixed fix) =>
        (a, b) switch {
            (Component left, Component right) => Aligned(document: document, left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (IParameter left, Component right) => Aligned(document: document, left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (Component left, IParameter right) => Aligned(document: document, left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (IParameter left, IParameter right) => Aligned(document: document, left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Align)).InvalidInput()),
        };

    private static Fin<Unit> Aligned<TLeft, TRight>(Document document, TLeft left, TRight right, OCD.Fixed fix, System.Action<TLeft, TRight, OCD.Fixed> align)
        where TLeft : IDocumentObject
        where TRight : IDocumentObject {
        Grasshopper2.Undo.ActionList actions = new([new PivotAction(obj: left.FoundingObject), new PivotAction(obj: right.FoundingObject)]);
        align(arg1: left, arg2: right, arg3: fix);
        left.Attributes.Invalidate();
        right.Attributes.Invalidate();
        document.Undo.Do(name: ("Align", "Objects"), actions: actions);
        return Fin.Succ(value: unit);
    }
}

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
                from a in Optional(objects.Find(instanceId: left)).ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                from b in Optional(objects.Find(instanceId: right)).ToFin(Fail: Op.Of(name: nameof(Align)).InvalidInput())
                select Align(a: a, b: b, fix: fix),
            policy: GrasshopperUiPolicy.Document(repaint: true));

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
            true => Snap(document: document, obj: obj, dx: dx, dy: dy)
                .Map(found => {
                    dx += found.Dx;
                    dy += found.Dy;
                    return found;
                }),
            false => None,
        };
        Grasshopper2.Undo.ActionList actions = new([new PivotAction(obj: obj)]);
        attributes.Move(dx: dx, dy: dy);
        attributes.Invalidate();
        document.Undo.Do(name: ("Move", "Object"), actions: actions);
        return new(ObjectId: before.ObjectId, Dx: dx, Dy: dy, Layout: SnapshotOf(attributes: attributes), Snap: action);
    }

    private static Option<SnappingSnapshot> Snap(Document document, IDocumentObject obj, float dx, float dy) {
        IAttributes attributes = obj.Attributes;
        attributes.Move(dx: dx, dy: dy);
        attributes.Layout(shape: UiShape.Default);
        SnappingConstraints constraints = SnappingConstraints.CreateFromDocument(document, obj.InstanceId);
        constraints.SnapObject(target: obj, settings: SnappingSettings.Current, visibleLimit: document.Objects.AttributeBounds, snapX: out SnappingAction x, snapY: out SnappingAction y);
        attributes.Move(dx: -dx, dy: -dy);
        attributes.Layout(shape: UiShape.Default);
        return (Optional(x), Optional(y)) switch {
            (Option<SnappingAction> { IsSome: true } sx, Option<SnappingAction> { IsSome: true } sy) => Some(new SnappingSnapshot(Dx: sx.Map(static action => action.ΔX).IfNone(0), Dy: sy.Map(static action => action.ΔY).IfNone(0), XLabel: sx.Map(static action => action.LabelText).IfNone(string.Empty), YLabel: sy.Map(static action => action.LabelText).IfNone(string.Empty))),
            (Option<SnappingAction> { IsSome: true } sx, _) => Some(new SnappingSnapshot(Dx: sx.Map(static action => action.ΔX).IfNone(0), Dy: 0, XLabel: sx.Map(static action => action.LabelText).IfNone(string.Empty), YLabel: string.Empty)),
            (_, Option<SnappingAction> { IsSome: true } sy) => Some(new SnappingSnapshot(Dx: 0, Dy: sy.Map(static action => action.ΔY).IfNone(0), XLabel: string.Empty, YLabel: sy.Map(static action => action.LabelText).IfNone(string.Empty))),
            _ => None,
        };
    }

    private static Unit Align(IDocumentObject a, IDocumentObject b, OCD.Fixed fix) =>
        (a, b) switch {
            (Component left, Component right) => Aligned(left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (IParameter left, Component right) => Aligned(left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (Component left, IParameter right) => Aligned(left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            (IParameter left, IParameter right) => Aligned(left: left, right: right, fix: fix, align: static (x, y, f) => OCD.AlignObjects(x, y, f)),
            _ => unit,
        };

    private static Unit Aligned<TLeft, TRight>(TLeft left, TRight right, OCD.Fixed fix, System.Action<TLeft, TRight, OCD.Fixed> align)
        where TLeft : IDocumentObject
        where TRight : IDocumentObject {
        align(arg1: left, arg2: right, arg3: fix);
        left.Attributes.Invalidate();
        right.Attributes.Invalidate();
        return unit;
    }
}

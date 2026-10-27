using System.Collections.Generic;
using Rasm.RhinoCommon.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;

namespace Rasm.RhinoCommon.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[Union<CustomBrepObject, CustomCurveObject, CustomMeshObject, CustomPointObject>(
    T1Name = "Brep", T2Name = "Curve", T3Name = "Mesh", T4Name = "Point",
    UseSingleBackingField = true, SingleBackingFieldType = typeof(RhinoObject), MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CustomObject;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, Func<CustomObject> create, Option<HistoryRecord> history) =>
        IO.lift(create).Bind(custom => IO.lift(() => {
            custom.Switch(
                (doc.Objects, History: history.ValueUnsafe()),
                brep: static (state, brep) => state.Objects.AddRhinoObject(brep, state.History),
                curve: static (state, curve) => state.Objects.AddRhinoObject(curve, state.History),
                mesh: static (state, mesh) => state.Objects.AddRhinoObject(mesh, state.History),
                point: static (state, point) => state.Objects.AddRhinoObject(point, state.History));
            return Conversions.Found(Conversions.Present(custom.Value.Id));
        }).Finally(IO.lift(custom.Value.Dispose)));

    public static IO<Unit> Replace(RhinoDoc doc, Guid id, Func<CustomObject> create) =>
        (from reference in use(() => new ObjRef(doc, id))
         from custom in IO.lift(create)
         from replaced in IO.lift(() => RhinoFault.Unless(doc.Objects.Replace(reference, custom.Value))).Finally(IO.lift(custom.Value.Dispose))
         select replaced).Bracket();

    // --- [CALLBACKS]
    public static IEnumerable<ObjRef> Pick(Func<IEnumerable<ObjRef>?> candidates, Func<ObjRef, bool> keep, object owner) {
        Seq<ObjRef> offered = owner.Answer(IO.lift(() => Conversions.Rows(candidates())), failed: Seq<ObjRef>());
        try {
            foreach (ObjRef reference in owner.Answer(IO.lift(() => offered.Filter(keep).Strict()), failed: Seq<ObjRef>()))
                yield return reference;
        } finally {
            _ = owner.Run(IO.lift(() => offered.Iter(static reference => reference.Dispose())));
        }
    }

    public static IO<Seq<PickCapture>> Picks(IEnumerable<ObjRef> pickedItems) =>
        Products.Made(pickedItems).Bind(static picked => picked.TraverseM(PickCapture.Of).As()).Bracket();

    public static BoundingBox TightBoundingBox(BoundingBox tightBox, bool growBox, Transform xform, Func<Transform, IO<BoundingBox>> box, object owner) =>
        BoundingBox.Union(growBox ? tightBox : BoundingBox.Empty, owner.Answer(IO.pure(xform).Bind(box), failed: BoundingBox.Empty));
}

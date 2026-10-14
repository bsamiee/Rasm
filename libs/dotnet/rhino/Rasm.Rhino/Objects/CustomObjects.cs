using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[Union<CustomBrepObject, CustomCurveObject, CustomMeshObject, CustomPointObject>(
    T1Name = "Brep", T2Name = "Curve", T3Name = "Mesh", T4Name = "Point",
    UseSingleBackingField = true, SingleBackingFieldType = typeof(RhinoObject), MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CustomObject;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    // --- [REGISTRATION]
    public static IO<Unit> Register(Func<CustomObject> create) =>
        DisposalOps.Within(create, static custom => IO.lift(() => MissingGuid.Unless(custom.Value.GetType())), static custom => IO.lift(custom.Value.Dispose));

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, Func<CustomObject> create, Option<HistoryRecord> history) =>
        DisposalOps.Within(create,
            custom => IO.lift(() => {
                custom.Switch(
                    (doc.Objects, History: history.ValueUnsafe()),
                    brep: static (state, brep) => state.Objects.AddRhinoObject(brep, state.History),
                    curve: static (state, curve) => state.Objects.AddRhinoObject(curve, state.History),
                    mesh: static (state, mesh) => state.Objects.AddRhinoObject(mesh, state.History),
                    point: static (state, point) => state.Objects.AddRhinoObject(point, state.History));
                return Conversions.Required(custom.Value.Id, nameof(ObjectTable.AddRhinoObject));
            }),
            static custom => IO.lift(custom.Value.Dispose));

    public static IO<Unit> Replace(RhinoDoc doc, Guid id, Func<CustomObject> create) =>
        DisposalOps.Within(() => new ObjRef(doc, id),
            reference => DisposalOps.Within(create,
                custom => IO.lift(() => Refused.Unless(doc.Objects.Replace(reference, custom.Value), nameof(ObjectTable.Replace))),
                static custom => IO.lift(custom.Value.Dispose)),
            static reference => IO.lift(reference.Dispose));

    // --- [CALLBACKS]
    public static IEnumerable<ObjRef> Pick(Func<IEnumerable<ObjRef>?> candidates, Func<ObjRef, bool> keep, CallbackSite site) {
        List<ObjRef> offered = [];
        using Disposal<List<ObjRef>> release = new(offered, held => Callbacks.Run(DisposalOps.Release(toSeq(held)), site));
        foreach (ObjRef reference in Callbacks.Answer(
            DisposalOps.Within(candidates,
                source => IO.lift(() => { offered.AddRange(source ?? []); return toSeq(offered).Filter(keep).Strict(); }),
                static source => DisposalOps.Release(Optional(source as IDisposable).ToSeq())),
            static () => Seq<ObjRef>(), site))
            yield return reference;
    }

    public static IO<Seq<PickCapture>> Picks(IEnumerable<ObjRef> pickedItems) =>
        DisposalOps.Within(static () => new List<ObjRef>(),
            picked => from read in IO.lift(() => picked.AddRange(pickedItems))
                      from captures in toSeq(picked).TraverseM(PickCapture.Of).As()
                      select captures,
            static picked => DisposalOps.Release(toSeq(picked)));

    public static BoundingBox TightBoundingBox(BoundingBox tightBox, bool growBox, Transform xform, Func<Transform, IO<BoundingBox>> box, CallbackSite site) =>
        BoundingBox.Union(growBox ? tightBox : BoundingBox.Empty, Callbacks.Answer(xform, box, static () => BoundingBox.Empty, site));
}

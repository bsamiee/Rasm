using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects.Authored;

// --- [MODELS] --------------------------------------------------------------------------
[Union<CustomBrepObject, CustomCurveObject, CustomMeshObject, CustomPointObject>(
    T1Name = "Brep", T2Name = "Curve", T3Name = "Mesh", T4Name = "Point",
    UseSingleBackingField = true, SingleBackingFieldType = typeof(RhinoObject), MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CustomObject;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    // --- [REGISTRATION]
    public static IO<Unit> Register(Func<CustomObject> create) =>
        (from custom in use(create, static custom => custom.Value.Dispose())
         from registered in IO.lift(() => MissingGuid.Unless(custom.Value.GetType()))
         select registered).Bracket();

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, Func<CustomObject> create, Option<HistoryRecord> history) =>
        (from custom in use(create, static custom => custom.Value.Dispose())
         from _ in IO.lift(() => custom.Switch(
             (doc.Objects, History: history.ValueUnsafe()),
             brep: static (state, brep) => state.Objects.AddRhinoObject(brep, state.History),
             curve: static (state, curve) => state.Objects.AddRhinoObject(curve, state.History),
             mesh: static (state, mesh) => state.Objects.AddRhinoObject(mesh, state.History),
             point: static (state, point) => state.Objects.AddRhinoObject(point, state.History)))
         from id in IO.lift(() => Conversions.Required(custom.Value.Id, nameof(ObjectTable.AddRhinoObject)))
         select id).Bracket();

    public static IO<Unit> Replace(RhinoDoc doc, Guid id, Func<CustomObject> create) =>
        (from reference in use(() => new ObjRef(doc, id))
         from replaced in
             (from custom in use(create, static custom => custom.Value.Dispose())
              from accepted in IO.lift(() => Refused.Unless(doc.Objects.Replace(reference, custom.Value), nameof(ObjectTable.Replace)))
              select accepted).Bracket()
         select replaced).Bracket();

    // --- [CALLBACKS]
    public static IEnumerable<ObjRef> Pick(Func<IEnumerable<ObjRef>?> candidates, Func<ObjRef, bool> keep, CallbackSite site) {
        using Disposal<Seq<ObjRef>> offered = new(
            Callbacks.Answer(
                IO.lift(() => Optional(candidates())).Bracket(
                    Use: static source => IO.lift(() => source.ToSeq().Bind(static rows => toSeq(rows)).Strict()),
                    Fin: static source => IO.lift(() => (source.ValueUnsafe() as IDisposable)?.Dispose())),
                static () => Seq<ObjRef>(),
                site),
            held => Callbacks.Answer(DisposalOps.Release(held), static () => unit, site));
        foreach (ObjRef reference in offered.Held.ToSeq().Bind(rows => Callbacks.Answer(IO.lift(() => rows.Filter(keep).Strict()), () => rows, site)))
            yield return reference;
    }

    public static IO<Seq<PickCapture>> Picks(IEnumerable<ObjRef> pickedItems) =>
        (from picked in use(static () => new List<ObjRef>(), static picked => DisposalOps.Release(toSeq(picked)))
         from read in IO.lift(() => picked.AddRange(pickedItems))
         from captures in toSeq(picked).TraverseM(PickCapture.Of).As()
         select captures).Bracket();

    public static BoundingBox TightBoundingBox(BoundingBox tightBox, bool growBox, Transform xform, Func<Transform, IO<BoundingBox>> box, CallbackSite site) =>
        BoundingBox.Union(growBox ? tightBox : BoundingBox.Empty, Callbacks.Answer(xform, box, static () => BoundingBox.Empty, site));
}

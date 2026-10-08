using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Objects.Authored;

// --- [MODELS] --------------------------------------------------------------------------
[Union<CustomBrepObject, CustomCurveObject, CustomMeshObject, CustomPointObject>(
    T1Name = "Brep", T2Name = "Curve", T3Name = "Mesh", T4Name = "Point",
    UseSingleBackingField = true, SingleBackingFieldType = typeof(RhinoObject))]
public sealed partial class CustomObject;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    // --- [REGISTRATION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Func<CustomObject> create) =>
        (_, _) => Created(create)
            .Bind(static custom => IO.lift(() => MissingGuid.Unless(custom.Value.GetType())))
            .Map(static _ => Thinktecture.Empty.Disposable())
            .Bracket();

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, Func<CustomObject> create, Option<HistoryRecord> history) =>
        (from custom in Created(create)
         from _ in IO.lift(() => custom.Switch(
             (doc.Objects, History: history.ValueUnsafe()),
             brep: static (state, brep) => state.Objects.AddRhinoObject(brep, state.History),
             curve: static (state, curve) => state.Objects.AddRhinoObject(curve, state.History),
             mesh: static (state, mesh) => state.Objects.AddRhinoObject(mesh, state.History),
             point: static (state, point) => state.Objects.AddRhinoObject(point, state.History)))
         from id in IO.lift(() => Conversions.Required(custom.Value.Id, nameof(ObjectTable.AddRhinoObject)))
         select id)
        .Bracket();

    public static IO<Unit> Replace(RhinoDoc doc, Guid id, Func<CustomObject> create) =>
        use(() => new ObjRef(doc, id))
            .Bind(reference => Created(create)
                .Bind(custom => IO.lift(() => Refused.Unless(doc.Objects.Replace(reference, custom.Value), nameof(ObjectTable.Replace))))
                .Bracket())
            .Bracket();

    // --- [CALLBACKS]
    public static Seq<ObjRef> Pick(Func<IEnumerable<ObjRef>?> candidates, Func<ObjRef, bool> keep, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => Conversions.Rows(candidates()))
                .Bind(offered => DisposalOps.OnFailure(
                    from split in IO.lift(() => offered.Partition(keep))
                    from _ in DisposalOps.Release(split.Second)
                    select split.First,
                    DisposalOps.Release(offered))),
            () => Conversions.Rows(candidates()),
            site);

    public static IO<Seq<PickCapture>> Picks(IEnumerable<ObjRef> pickedItems) =>
        IO.lift(() => Conversions.Rows(pickedItems)).Bracket(
            Use: static picked => picked.TraverseM(PickCapture.Of).As(),
            Fin: DisposalOps.Release);

    public static bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform, Func<Transform, IO<BoundingBox>> box, CallbackSite site) =>
        (tightBox = BoundingBox.Union(growBox ? tightBox : BoundingBox.Empty, Callbacks.Answer(xform, box, static () => BoundingBox.Empty, site))).IsValid;

    private static IO<CustomObject> Created(Func<CustomObject> create) =>
        use(create, static custom => custom.Value.Dispose());
}

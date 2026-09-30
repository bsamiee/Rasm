using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    public static IO<Guid> AddCustom<T>(RhinoDoc doc, T custom, Option<HistoryRecord> history) where T : RhinoObject, new() =>
        from addable in IO.lift(() => (Invalid.Unless(custom.Document is null, nameof(custom)), Missing.Unless(custom.Geometry, nameof(RhinoObject.Geometry))).Apply(static (_, _) => unit).As())
        from added in custom switch {
            CustomBrepObject brep => IO.lift(() => doc.Objects.AddRhinoObject(brep, history.ValueUnsafe())),
            CustomCurveObject curve => IO.lift(() => doc.Objects.AddRhinoObject(curve, history.ValueUnsafe())),
            CustomMeshObject mesh => IO.lift(() => doc.Objects.AddRhinoObject(mesh, history.ValueUnsafe())),
            CustomPointObject point => IO.lift(() => doc.Objects.AddRhinoObject(point, history.ValueUnsafe())),
            _ => IO.fail<Unit>(new Invalid(nameof(custom))),
        }
        from owned in IO.lift(() => Refused.Unless(custom.Document is not null, nameof(ObjectTable.AddRhinoObject)))
        select custom.Id;
}

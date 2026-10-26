using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Solids;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidConstruction {
    // --- [BREPS]
    public static IO<Seq<Grouped<Brep>>> CreatePlanarBrepsWithIndexMap(Seq<Curve> inputLoops, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Acquire(() => (Brep.CreatePlanarBrepsWithIndexMap(inputLoops, tolerances.Absolute, out int[][] indexMap), indexMap), nameof(Brep.CreatePlanarBrepsWithIndexMap), retain)
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, toSeq(row.Row))));

    // --- [EXTRUSIONS]
    public static IO<Extrusion> ProfiledExtrusion(Line path, Vector3d up, Curve outerProfile, Seq<Curve> innerProfiles, bool cap, Action<IDisposable> retain) =>
        Copies.Owned(
            static () => new Extrusion(),
            made =>
                from pathSet in IO.lift(() => Refused.Unless(made.SetPathAndUp(path.From, path.To, up), nameof(Extrusion.SetPathAndUp)))
                from outerSet in IO.lift(() => Refused.Unless(made.SetOuterProfile(outerProfile, cap), nameof(Extrusion.SetOuterProfile)))
                from innerSet in IO.lift(() => Callbacks.Each(innerProfiles, made.AddInnerProfile, nameof(Extrusion.AddInnerProfile)))
                from valid in IO.lift(() => Measurements.Valid(made, nameof(Extrusion.SetOuterProfile)))
                select valid,
            retain);

    public static IO<Mesh> GetMesh(Extrusion extrusion, MeshType meshType, Action<IDisposable> retain) =>
        IO.lift(() => Refused.UnlessFound(extrusion.GetMesh(meshType), nameof(Extrusion.GetMesh))).Bind(mesh => Copies.Duplicate(mesh, retain));
}

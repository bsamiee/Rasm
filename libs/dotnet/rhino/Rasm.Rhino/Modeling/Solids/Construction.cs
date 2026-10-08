using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BoxCorners(Point3d V0, Point3d V1, Point3d V2, Point3d V3, Point3d V4, Point3d V5, Point3d V6, Point3d V7) {
    public Seq<Point3d> Points => [V0, V1, V2, V3, V4, V5, V6, V7];
}

public sealed record SurfaceEdges(Curve First, Curve Second, Option<(Curve Third, Option<Curve> Fourth)> Rest) {
    public Seq<Curve> Curves => [First, Second, .. Rest.ToSeq().Bind(static rest => rest.Third.Cons(rest.Fourth.ToSeq()))];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidConstruction {
    // --- [BREPS]
    public static IO<Seq<Grouped<Brep>>> CreatePlanarBrepsWithIndexMap(Seq<Curve> inputLoops, Tolerances tolerances) =>
        Copies.Acquire(() => (Brep.CreatePlanarBrepsWithIndexMap(inputLoops, tolerances.Absolute, out int[][] indexMap), indexMap), nameof(Brep.CreatePlanarBrepsWithIndexMap))
            .Map(static rows => rows.Map(static row => new Grouped<Brep>(row.Result, toSeq(row.Row))));

    // --- [EXTRUSIONS]
    public static IO<Extrusion> ProfiledExtrusion(Line path, Vector3d up, Curve outerProfile, Seq<Curve> innerProfiles, bool cap) =>
        Copies.Owned(
            IO.lift(static () => new Extrusion()),
            made => Refused.Unless(made.SetPathAndUp(path.From, path.To, up), nameof(Extrusion.SetPathAndUp))
                .Bind(_ => Refused.Unless(made.SetOuterProfile(outerProfile, cap), nameof(Extrusion.SetOuterProfile)))
                .Bind(_ => Callbacks.Each(innerProfiles, made.AddInnerProfile, nameof(Extrusion.AddInnerProfile)))
                .Bind(_ => Measurements.Valid(made, nameof(Extrusion.SetOuterProfile))));

    public static IO<Mesh> GetMesh(Extrusion extrusion, MeshType meshType) =>
        IO.lift(() => Missing.Unless(extrusion.GetMesh(meshType), nameof(Extrusion.GetMesh))).Bind(Copies.Duplicate);
}

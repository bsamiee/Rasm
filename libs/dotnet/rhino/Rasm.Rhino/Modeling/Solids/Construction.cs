namespace Rasm.Rhino.Modeling.Solids;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidConstruction {
    public static IO<Extrusion> ProfiledExtrusion(Line path, Vector3d up, Curve outerProfile, Seq<Curve> innerProfiles, bool cap, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(ProfiledExtrusion), _ => new Extrusion() switch {
            var made => (
                    Answer.Accepted(made.SetPathAndUp(path.From, path.To, up)),
                    Answer.Accepted(made.SetOuterProfile(outerProfile, cap)),
                    Answer.Accepted(innerProfiles.Map(made.AddInnerProfile).Strict().ForAll(static added => added)),
                    Answer.One(made))
                .Apply(static (_, _, _, extrusion) => extrusion).As(),
        }, retain);

    public static IO<Mesh> GetMesh(Extrusion extrusion, MeshType meshType, Action<IDisposable> retain) =>
        IO.lift(() => Refused.UnlessFound(extrusion.GetMesh(meshType), nameof(Extrusion.GetMesh))).Bind(mesh => Copies.Duplicate(mesh, retain));
}

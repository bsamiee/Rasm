namespace Rasm.RhinoCommon.Modeling.Solids;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidConstruction {
    public static IO<Extrusion> ProfiledExtrusion(Line path, Vector3d up, Curve outerProfile, Seq<Curve> innerProfiles, bool cap) =>
        from made in use(static () => new Extrusion())
        from shaped in IO.lift(() => (
                RhinoFault.Unless(made.SetPathAndUp(path.From, path.To, up)),
                RhinoFault.Unless(made.SetOuterProfile(outerProfile, cap)),
                Callbacks.Each(innerProfiles, (profile, index) => RhinoFault.Unless(made.AddInnerProfile(profile), index)))
            .Apply(static (_, _, _) => unit).As())
        from extrusion in Products.Made(made)
        select extrusion;

    public static IO<Mesh> GetMesh(Extrusion extrusion, MeshType meshType) =>
        IO.lift(() => Conversions.Found(extrusion.GetMesh(meshType))).Bind(static mesh => Copies.Duplicate(mesh));
}

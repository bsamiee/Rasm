using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDSource {
    public sealed record QuadSphere(Sphere Sphere, SubDComponentLocation Location, uint Level) : SubDSource;

    public sealed record GlobeSphere(Sphere Sphere, SubDComponentLocation Location, uint AxialFaces, uint EquatorialFaces) : SubDSource;

    public sealed record TriSphere(Sphere Sphere, SubDComponentLocation Location, uint Level) : SubDSource;

    public sealed record Icosahedron(Sphere Sphere, SubDComponentLocation Location) : SubDSource;

    public sealed record OfCylinder(Cylinder Cylinder, uint CircumferenceFaces, uint HeightFaces, SubDEndCapStyle EndCap, SubDEdgeTag EndCapEdge, SubDComponentLocation Radius) : SubDSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDEdit {
    public sealed record SubdivideAll(int Level) : SubDEdit;

    public sealed record SubdivideFaces(Seq<int> Faces) : SubDEdit;

    public sealed record Interpolate(Seq<Point3d> SurfacePoints) : SubDEdit;

    public sealed record SetVertexSurfacePoint(uint VertexId, Point3d SurfacePoint) : SubDEdit;

    public sealed record MergeCoplanar(double Tolerance, double AngleTolerance) : SubDEdit;

    public sealed record PackFaces() : SubDEdit;

    public sealed record Flip() : SubDEdit;

    public sealed record TagVertices(Seq<int> Vertices, SubDVertexTag Tag) : SubDEdit;

    public sealed record TagEdges(Seq<int> Edges, SubDEdgeTag Tag) : SubDEdit;

    public sealed record MoveComponents(Seq<ComponentIndex> Components, Transform Xform, SubDComponentLocation Location) : SubDEdit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    // --- [CONSTRUCTION]
    public static IO<SubD> FromMesh(Mesh mesh, SubDCreationOptions options) =>
        GeometryResults.Acquire(() => SubD.CreateFromMesh(mesh, options), nameof(SubD.CreateFromMesh));

    public static IO<SubD> FromSurface(Surface surface, SubDFromSurfaceMethods method, bool corners) =>
        GeometryResults.Acquire(() => Invalid.Unless(method != SubDFromSurfaceMethods.Unset, nameof(method)).Map(_ => SubD.CreateFromSurface(surface, method, corners)), nameof(SubD.CreateFromSurface));

    public static IO<SubD> FromLoft(Seq<NurbsCurve> shapes, bool closed, bool corners, bool creases, int divisions) =>
        GeometryResults.Acquire(() => SubD.CreateFromLoft(shapes, closed, corners, creases, divisions), nameof(SubD.CreateFromLoft));

    public static IO<SubD> FromSweepOne(NurbsCurve rail, Seq<NurbsCurve> shapes, bool closed, bool corners, Option<Vector3d> roadlikeUp) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)).Map(_ => SubD.CreateFromSweep(rail, shapes, closed, corners, roadlikeFrame: roadlikeUp.IsSome, roadlikeNormal: roadlikeUp.IfNone(Vector3d.Unset))),
            nameof(SubD.CreateFromSweep));

    public static IO<SubD> FromSweepTwo(NurbsCurve rail1, NurbsCurve rail2, Seq<NurbsCurve> shapes, bool closed, bool corners) =>
        GeometryResults.Acquire(() => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)).Map(_ => SubD.CreateFromSweep(rail1, rail2, shapes, closed, corners)), nameof(SubD.CreateFromSweep));

    public static IO<SubD> Create(SubDSource source) =>
        source.Switch(
            quadSphere: static of => GeometryResults.Acquire(() => SubD.CreateQuadSphere(of.Sphere, of.Location, of.Level), nameof(SubD.CreateQuadSphere)),
            globeSphere: static of => GeometryResults.Acquire(() => SubD.CreateGlobeSphere(of.Sphere, of.Location, of.AxialFaces, of.EquatorialFaces), nameof(SubD.CreateGlobeSphere)),
            triSphere: static of => GeometryResults.Acquire(() => SubD.CreateTriSphere(of.Sphere, of.Location, of.Level), nameof(SubD.CreateTriSphere)),
            icosahedron: static of => GeometryResults.Acquire(() => SubD.CreateIcosahedron(of.Sphere, of.Location), nameof(SubD.CreateIcosahedron)),
            ofCylinder: static of => GeometryResults.Acquire(
                () => SubD.CreateFromCylinder(of.Cylinder, of.CircumferenceFaces, of.HeightFaces, of.EndCap, of.EndCapEdge, of.Radius),
                nameof(SubD.CreateFromCylinder)));

    public static IO<Seq<SubD>> Join(Seq<SubD> subds, double tolerance, bool joinedEdgesAreCreases, bool preserveSymmetry) =>
        GeometryResults.Acquire(() => SubD.JoinSubDs(subds, tolerance, joinedEdgesAreCreases, preserveSymmetry), nameof(SubD.JoinSubDs), emptyFails: false);

    // --- [DERIVATIONS]
    public static IO<Brep> ToBrep(SubD subd, bool packFaces, SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) =>
        DisposalOps.Using(() => new SubDToBrepOptions(packFaces, vertexProcess), options => GeometryResults.Acquire(() => subd.ToBrep(options), nameof(SubD.ToBrep)));

    public static IO<Seq<Curve>> EdgeCurves(SubD subd, bool boundaryOnly, bool interiorOnly, bool smoothOnly, bool sharpOnly, bool creaseOnly, bool clampEnds) =>
        GeometryResults.Acquire(
            () => subd.DuplicateEdgeCurves(boundaryOnly, interiorOnly, smoothOnly, sharpOnly, creaseOnly, clampEnds),
            nameof(SubD.DuplicateEdgeCurves),
            emptyFails: false);

    public static IO<SubD> Offset(SubD subd, double distance, bool solidify) =>
        GeometryResults.Acquire(() => subd.Offset(distance, solidify), nameof(SubD.Offset));

    // --- [EDITS]
    public static IO<SubD> Edit(SubD source, SubDEdit edit) =>
        from apply in IO.lift(() => Validated(source, edit))
        from edited in GeometryResults.EditCopy(source, copy =>
            from applied in apply(copy)
            from tagged in Fin.Succ(ignore(copy.UpdateAllTagsAndSectorCoefficients()))
            from cached in Fin.Succ(ignore(copy.UpdateSurfaceMeshCache(lazyUpdate: true)))
            select unit)
        select edited.Copy;

    private static Fin<Func<SubD, Fin<Unit>>> Validated(SubD source, SubDEdit edit) =>
        edit.Switch(
            source,
            subdivideAll: static (_, of) => fun((SubD copy) => Refused.Unless(copy.Subdivide(of.Level), nameof(SubD.Subdivide))),
            subdivideFaces: static (subd, of) =>
                from inside in GeometryResults.InRange(of.Faces, subd.Faces.Count, nameof(SubD.Faces))
                select fun((SubD copy) => Refused.Unless(copy.Subdivide(of.Faces), nameof(SubD.Subdivide))),
            interpolate: static (subd, of) =>
                from same in CountMismatch.Unless(subd.Vertices.Count, of.SurfacePoints.Count, nameof(SubD.InterpolateSurfacePoints))
                select fun((SubD copy) => Refused.Unless(copy.InterpolateSurfacePoints([.. of.SurfacePoints]), nameof(SubD.InterpolateSurfacePoints))),
            setVertexSurfacePoint: static (_, of) => fun((SubD copy) => Refused.Unless(copy.SetVertexSurfacePoint(of.VertexId, of.SurfacePoint), nameof(SubD.SetVertexSurfacePoint))),
            mergeCoplanar: static (_, of) => fun((SubD copy) => Fin.Succ(ignore(copy.MergeAllCoplanarFaces(of.Tolerance, of.AngleTolerance)))),
            packFaces: static (_, _) => fun(static (SubD copy) => Fin.Succ(ignore(copy.PackFaces()))),
            flip: static (_, _) => fun(static (SubD copy) => Refused.Unless(copy.Flip(), nameof(SubD.Flip))),
            tagVertices: static (subd, of) =>
                from inside in GeometryResults.InRange(of.Vertices, subd.Vertices.Count, nameof(SubD.Vertices))
                select fun((SubD copy) => {
                    copy.Vertices.SetVertexTags(of.Vertices, of.Tag);
                    return Fin.Succ(unit);
                }),
            tagEdges: static (subd, of) =>
                from inside in GeometryResults.InRange(of.Edges, subd.Edges.Count, nameof(SubD.Edges))
                select fun((SubD copy) => {
                    copy.Edges.SetEdgeTags(of.Edges, of.Tag);
                    return Fin.Succ(unit);
                }),
            moveComponents: static (_, of) =>
                from present in Invalid.Unless(!of.Components.IsEmpty, nameof(of.Components))
                select fun((SubD copy) => Fin.Succ(ignore(copy.TransformComponents(of.Components, of.Xform, of.Location)))));
}

using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EdgeSelection(bool Boundary, bool Interior, bool Smooth, bool Sharp, bool Crease, bool ClampEnds);

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
    // --- [LIMITS]
    private const int MinimumClosedLoftShapes = 3;

    // --- [CONSTRUCTION]
    public static IO<SubD> FromMesh(Mesh mesh, SubDCreationOptions options) =>
        IO.lift(() => Missing.Unless(SubD.CreateFromMesh(mesh, options), nameof(SubD.CreateFromMesh)));

    public static IO<SubD> FromSurface(Surface surface, SubDFromSurfaceMethods method, bool corners) =>
        from set in IO.lift(() => Invalid.Unless(method != SubDFromSurfaceMethods.Unset, nameof(method)))
        from subd in IO.lift(() => Missing.Unless(SubD.CreateFromSurface(surface, method, corners), nameof(SubD.CreateFromSurface)))
        select subd;

    public static IO<SubD> FromLoft(Seq<NurbsCurve> shapes, bool closed, bool corners, bool creases, int divisions) =>
        from valid in IO.lift(() =>
            from present in Limits.AtLeast(closed ? MinimumClosedLoftShapes : 1).Check(shapes.Count, nameof(shapes))
            from divided in Limits.AtLeast(1).Check(divisions, nameof(divisions))
            select divided)
        from subd in IO.lift(() => Missing.Unless(SubD.CreateFromLoft(shapes, closed, corners, creases, valid), nameof(SubD.CreateFromLoft)))
        select subd;

    public static IO<SubD> FromSweepOne(NurbsCurve rail, Seq<NurbsCurve> shapes, bool closed, bool corners, RailFrame frame) =>
        from present in IO.lift(() => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)))
        let framed = Lofts.FrameOf(frame)
        from subd in IO.lift(() =>
            Missing.Unless(SubD.CreateFromSweep(rail, shapes, closed, corners, roadlikeFrame: framed.Frame == SweepFrame.Roadlike, roadlikeNormal: framed.Up), nameof(SubD.CreateFromSweep)))
        select subd;

    public static IO<SubD> FromSweepTwo(NurbsCurve rail1, NurbsCurve rail2, Seq<NurbsCurve> shapes, bool closed, bool corners) =>
        from present in IO.lift(() => Invalid.Unless(!shapes.IsEmpty, nameof(shapes)))
        from subd in IO.lift(() => Missing.Unless(SubD.CreateFromSweep(rail1, rail2, shapes, closed, corners), nameof(SubD.CreateFromSweep)))
        select subd;

    public static IO<SubD> Create(SubDSource source) =>
        IO.lift(() => source.Switch(
            quadSphere: static of =>
                from leveled in Limits.AtLeast(1u).Check(of.Level, nameof(of.Level))
                from subd in Missing.Unless(SubD.CreateQuadSphere(of.Sphere, of.Location, leveled), nameof(SubD.CreateQuadSphere))
                select subd,
            globeSphere: static of =>
                from faced in Seq((Faces: of.AxialFaces, Member: nameof(of.AxialFaces)), (Faces: of.EquatorialFaces, Member: nameof(of.EquatorialFaces))).Traverse(static row => Limits.AtLeast(1u).Check(row.Faces, row.Member)).As()
                from subd in Missing.Unless(SubD.CreateGlobeSphere(of.Sphere, of.Location, of.AxialFaces, of.EquatorialFaces), nameof(SubD.CreateGlobeSphere))
                select subd,
            triSphere: static of =>
                from leveled in Limits.AtLeast(1u).Check(of.Level, nameof(of.Level))
                from subd in Missing.Unless(SubD.CreateTriSphere(of.Sphere, of.Location, leveled), nameof(SubD.CreateTriSphere))
                select subd,
            icosahedron: static of => Missing.Unless(SubD.CreateIcosahedron(of.Sphere, of.Location), nameof(SubD.CreateIcosahedron)),
            ofCylinder: static of =>
                from faced in Seq((Faces: of.CircumferenceFaces, Member: nameof(of.CircumferenceFaces)), (Faces: of.HeightFaces, Member: nameof(of.HeightFaces))).Traverse(static row => Limits.AtLeast(1u).Check(row.Faces, row.Member)).As()
                from subd in Missing.Unless(SubD.CreateFromCylinder(of.Cylinder, of.CircumferenceFaces, of.HeightFaces, of.EndCap, of.EndCapEdge, of.Radius), nameof(SubD.CreateFromCylinder))
                select subd));

    public static IO<Seq<SubD>> Join(Seq<SubD> subds, double tolerance, bool joinedEdgesAreCreases, bool preserveSymmetry) =>
        from present in IO.lift(() => Invalid.Unless(!subds.IsEmpty, nameof(subds)))
        from joined in GeometryResults.Acquire(() => SubD.JoinSubDs(subds, tolerance, joinedEdgesAreCreases, preserveSymmetry), nameof(SubD.JoinSubDs))
        select joined;

    // --- [DERIVATIONS]
    public static IO<Brep> ToBrep(SubD subd, bool packFaces, SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) =>
        Disposal.Using(() => new SubDToBrepOptions(packFaces, vertexProcess), options => IO.lift(() => Missing.Unless(subd.ToBrep(options), nameof(SubD.ToBrep))));

    public static IO<Seq<Curve>> EdgeCurves(SubD subd, EdgeSelection selection) =>
        from spread in IO.lift(() => Invalid.Unless(selection.Boundary || selection.Interior || selection.Smooth || selection.Sharp || selection.Crease, nameof(EdgeSelection)))
        from curves in GeometryResults.Acquire(
            () => subd.DuplicateEdgeCurves(selection.Boundary, selection.Interior, selection.Smooth, selection.Sharp, selection.Crease, selection.ClampEnds),
            nameof(SubD.DuplicateEdgeCurves))
        select curves;

    public static IO<SubD> Offset(SubD subd, double distance, bool solidify) =>
        IO.lift(() => Missing.Unless(subd.Offset(distance, solidify), nameof(SubD.Offset)));

    // --- [EDITS]
    public static IO<SubD> Edit(SubD source, SubDEdit edit) =>
        from apply in IO.lift(() => Validated(source, edit))
        from edited in GeometryOps.EditCopy(source, copy =>
            from applied in apply(copy)
            from tagged in Fin.Succ(ignore(copy.UpdateAllTagsAndSectorCoefficients()))
            from cached in Fin.Succ(ignore(copy.UpdateSurfaceMeshCache(lazyUpdate: true)))
            select unit)
        select edited.Copy;

    private static Fin<Func<SubD, Fin<Unit>>> Validated(SubD source, SubDEdit edit) =>
        edit.Switch(
            source,
            subdivideAll: static (_, of) =>
                from leveled in Limits.AtLeast(1).Check(of.Level, nameof(of.Level))
                select fun((SubD copy) => Refused.Unless(copy.Subdivide(leveled), nameof(SubD.Subdivide))),
            subdivideFaces: static (subd, of) =>
                from inside in Answers.InRange(of.Faces, subd.Faces.Count, nameof(SubD.Faces))
                select fun((SubD copy) => Refused.Unless(copy.Subdivide(of.Faces), nameof(SubD.Subdivide))),
            interpolate: static (subd, of) =>
                from same in CountMismatch.Unless(subd.Vertices.Count, of.SurfacePoints.Count, nameof(SubD.InterpolateSurfacePoints))
                select fun((SubD copy) => Refused.Unless(copy.InterpolateSurfacePoints([.. of.SurfacePoints]), nameof(SubD.InterpolateSurfacePoints))),
            setVertexSurfacePoint: static (_, of) => fun((SubD copy) => Refused.Unless(copy.SetVertexSurfacePoint(of.VertexId, of.SurfacePoint), nameof(SubD.SetVertexSurfacePoint))),
            mergeCoplanar: static (_, of) => fun((SubD copy) => Fin.Succ(ignore(copy.MergeAllCoplanarFaces(of.Tolerance, of.AngleTolerance)))),
            packFaces: static (_, _) => fun(static (SubD copy) => Fin.Succ(ignore(copy.PackFaces()))),
            flip: static (_, _) => fun(static (SubD copy) => Refused.Unless(copy.Flip(), nameof(SubD.Flip))),
            tagVertices: static (subd, of) =>
                from inside in Answers.InRange(of.Vertices, subd.Vertices.Count, nameof(SubD.Vertices))
                select fun((SubD copy) => {
                    copy.Vertices.SetVertexTags(of.Vertices, of.Tag);
                    return Fin.Succ(unit);
                }),
            tagEdges: static (subd, of) =>
                from inside in Answers.InRange(of.Edges, subd.Edges.Count, nameof(SubD.Edges))
                select fun((SubD copy) => {
                    copy.Edges.SetEdgeTags(of.Edges, of.Tag);
                    return Fin.Succ(unit);
                }),
            moveComponents: static (_, of) =>
                from present in Invalid.Unless(!of.Components.IsEmpty, nameof(of.Components))
                select fun((SubD copy) => Fin.Succ(ignore(copy.TransformComponents(of.Components, of.Xform, of.Location)))));
}

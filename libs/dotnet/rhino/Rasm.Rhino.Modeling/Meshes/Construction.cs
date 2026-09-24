using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling.Solids;
using Rhino.Geometry.MeshRefinements;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshSource {
    public sealed record OfPlane(Plane Plane, Interval X, Interval Y, int XCount, int YCount) : MeshSource;

    public sealed record OfBox(Box Box, int XCount, int YCount, int ZCount) : MeshSource;

    public sealed record OfBounds(BoundingBox Bounds, int XCount, int YCount, int ZCount) : MeshSource;

    public sealed record OfCorners(Seq<Point3d> Corners, int XCount, int YCount, int ZCount) : MeshSource;

    public sealed record OfSphere(Sphere Sphere, int XCount, int YCount) : MeshSource;

    public sealed record IcoSphere(Sphere Sphere, int Subdivisions) : MeshSource;

    public sealed record QuadSphere(Sphere Sphere, int Subdivisions) : MeshSource;

    public sealed record OfCylinder(Cylinder Cylinder, int Vertical, int Around, bool CapBottom, bool CapTop, bool Circumscribe, bool QuadCaps) : MeshSource;

    public sealed record OfCone(Cone Cone, int Vertical, int Around, bool Solid, bool QuadCaps) : MeshSource;

    public sealed record OfTorus(Torus Torus, int Vertical, int Around) : MeshSource;

    public sealed record ClosedPolyline(Polyline Polyline) : MeshSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CageSource {
    public sealed record OfSubD(SubD SubD, bool TextureCoordinates) : CageSource;

    public sealed record OfSurface(Surface Surface) : CageSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QuadSource {
    public sealed record OfBrep(Brep Brep) : QuadSource;

    public sealed record OfMesh(Mesh Mesh) : QuadSource;

    public sealed record OfMeshFaces(Mesh Mesh, Seq<int> FaceBlocks) : QuadSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShrinkWrapSource {
    public sealed record OfMeshes(Seq<Mesh> Meshes) : ShrinkWrapSource;

    public sealed record OfCloud(PointCloud Cloud) : ShrinkWrapSource;

    public sealed record OfGeometry(Seq<GeometryBase> Geometry, MeshingParameters Meshing) : ShrinkWrapSource;

    public sealed record OfMesh(Mesh Mesh) : ShrinkWrapSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshExtrudeMethod {
    public sealed record Free() : MeshExtrudeMethod;

    public sealed record Fitted(MeshingParameters Meshing) : MeshExtrudeMethod;

    public sealed record Boxed(MeshingParameters Meshing, BoundingBox Bounds) : MeshExtrudeMethod;
}

public sealed record ConvexHullResult(Mesh Mesh, Seq<Seq<int>> Facets);

public sealed record MeshPatchOptions(double AngleToleranceRadians, Option<Surface> Pullback, Seq<Curve> InnerBoundaries, Seq<Curve> InnerBothSides, Seq<Point3d> InnerPoints, bool Trimback, int Divisions);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RefineMethod {
    public sealed record Loop(LoopFormula Formula) : RefineMethod;

    public sealed record CatmullClark() : RefineMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [LIMITS]
    private const int MinimumHullPoints = 4;

    private const int MinimumPipeSegments = 3;

    private static readonly Fin<Limits<double>> AdaptiveSize = Limits.AtLeast(0.0).AtMost(100.0, nameof(AdaptiveSize));

    // --- [FROM_GEOMETRY]
    public static IO<Seq<Mesh>> FromGeometry(GeometryBase source, MeshingParameters parameters) =>
        source switch {
            Brep brep => GeometryResults.Acquire(() => Mesh.CreateFromBrep(brep, parameters), nameof(Mesh.CreateFromBrep)),
            Extrusion extrusion => IO.lift(() => Missing.Unless(Mesh.CreateFromExtrusion(extrusion, parameters), nameof(Mesh.CreateFromExtrusion))).Map(static mesh => Seq(mesh)),
            Surface surface => IO.lift(() => Missing.Unless(Mesh.CreateFromSurface(surface, parameters), nameof(Mesh.CreateFromSurface))).Map(static mesh => Seq(mesh)),
            _ => IO.fail<Seq<Mesh>>(new WrongGeometry(typeof(Brep), source.ObjectType)),
        };

    public static IO<Mesh> FromSubD(SubD subd, SubDDisplayParameters.Density density) =>
        IO.lift(() => Missing.Unless(Mesh.CreateFromSubD(subd, density), nameof(Mesh.CreateFromSubD)));

    public static IO<Mesh> ControlNet(CageSource source) =>
        IO.lift(() => source.Switch(
            ofSubD: static of => of.TextureCoordinates
                ? Missing.Unless(Mesh.CreateFromSubDControlNetWithTextureCoordinates(of.SubD), nameof(Mesh.CreateFromSubDControlNetWithTextureCoordinates))
                : Missing.Unless(Mesh.CreateFromSubDControlNet(of.SubD), nameof(Mesh.CreateFromSubDControlNet)),
            ofSurface: static of => Missing.Unless(Mesh.CreateFromSurfaceControlNet(of.Surface), nameof(Mesh.CreateFromSurfaceControlNet))));

    public static IO<Mesh> FromBoundary(Curve boundary, MeshingParameters parameters, double tolerance) =>
        IO.lift(() => Missing.Unless(Mesh.CreateFromPlanarBoundary(boundary, parameters, tolerance), nameof(Mesh.CreateFromPlanarBoundary)));

    public static IO<Mesh> Create(MeshSource source) =>
        IO.lift(() => source.Switch(
            ofPlane: static of =>
                from counted in Seq((Value: of.XCount, Member: nameof(of.XCount)), (Value: of.YCount, Member: nameof(of.YCount))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromPlane(of.Plane, of.X, of.Y, of.XCount, of.YCount), nameof(Mesh.CreateFromPlane))
                select mesh,
            ofBox: static of =>
                from counted in Seq((Value: of.XCount, Member: nameof(of.XCount)), (Value: of.YCount, Member: nameof(of.YCount)), (Value: of.ZCount, Member: nameof(of.ZCount))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromBox(of.Box, of.XCount, of.YCount, of.ZCount), nameof(Mesh.CreateFromBox))
                select mesh,
            ofBounds: static of =>
                from counted in Seq((Value: of.XCount, Member: nameof(of.XCount)), (Value: of.YCount, Member: nameof(of.YCount)), (Value: of.ZCount, Member: nameof(of.ZCount))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromBox(of.Bounds, of.XCount, of.YCount, of.ZCount), nameof(Mesh.CreateFromBox))
                select mesh,
            ofCorners: static of =>
                from eight in CountMismatch.Unless(SolidConstruction.BoxCorners, of.Corners.Count, nameof(Mesh.CreateFromBox))
                from counted in Seq((Value: of.XCount, Member: nameof(of.XCount)), (Value: of.YCount, Member: nameof(of.YCount)), (Value: of.ZCount, Member: nameof(of.ZCount))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromBox(of.Corners, of.XCount, of.YCount, of.ZCount), nameof(Mesh.CreateFromBox))
                select mesh,
            ofSphere: static of =>
                from counted in Seq((Value: of.XCount, Member: nameof(of.XCount)), (Value: of.YCount, Member: nameof(of.YCount))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromSphere(of.Sphere, of.XCount, of.YCount), nameof(Mesh.CreateFromSphere))
                select mesh,
            icoSphere: static of =>
                from counted in Limits.AtLeast(1).Check(of.Subdivisions, nameof(of.Subdivisions))
                from mesh in Missing.Unless(Mesh.CreateIcoSphere(of.Sphere, of.Subdivisions), nameof(Mesh.CreateIcoSphere))
                select mesh,
            quadSphere: static of =>
                from counted in Limits.AtLeast(1).Check(of.Subdivisions, nameof(of.Subdivisions))
                from mesh in Missing.Unless(Mesh.CreateQuadSphere(of.Sphere, of.Subdivisions), nameof(Mesh.CreateQuadSphere))
                select mesh,
            ofCylinder: static of =>
                from counted in Seq((Value: of.Vertical, Member: nameof(of.Vertical)), (Value: of.Around, Member: nameof(of.Around))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromCylinder(of.Cylinder, of.Vertical, of.Around, of.CapBottom, of.CapTop, of.Circumscribe, of.QuadCaps), nameof(Mesh.CreateFromCylinder))
                select mesh,
            ofCone: static of =>
                from counted in Seq((Value: of.Vertical, Member: nameof(of.Vertical)), (Value: of.Around, Member: nameof(of.Around))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromCone(of.Cone, of.Vertical, of.Around, of.Solid, of.QuadCaps), nameof(Mesh.CreateFromCone))
                select mesh,
            ofTorus: static of =>
                from counted in Seq((Value: of.Vertical, Member: nameof(of.Vertical)), (Value: of.Around, Member: nameof(of.Around))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
                from mesh in Missing.Unless(Mesh.CreateFromTorus(of.Torus, of.Vertical, of.Around), nameof(Mesh.CreateFromTorus))
                select mesh,
            closedPolyline: static of =>
                from closed in Invalid.Unless(of.Polyline.IsClosed, nameof(Polyline.IsClosed))
                from mesh in Missing.Unless(Mesh.CreateFromClosedPolyline(of.Polyline), nameof(Mesh.CreateFromClosedPolyline))
                select mesh));

    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress, CancellationToken cancel) =>
        from sized in IO.lift(() => AdaptiveSize.Bind(limits => limits.Check(parameters.AdaptiveSize, nameof(QuadRemeshParameters.AdaptiveSize))))
        from mesh in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Cancel: cancel),
            ofBrep: static (state, of) =>
                from remeshed in IO.liftAsync(() => state.Guides.IsEmpty
                    ? Mesh.QuadRemeshBrepAsync(of.Brep, state.Parameters, state.Progress, state.Cancel)
                    : Mesh.QuadRemeshBrepAsync(of.Brep, state.Parameters, state.Guides, state.Progress, state.Cancel))
                from mesh in IO.lift(Missing.Unless(remeshed, nameof(Mesh.QuadRemeshBrepAsync)))
                select mesh,
            ofMesh: static (state, of) =>
                from remeshed in IO.liftAsync(() => state.Guides.IsEmpty
                    ? of.Mesh.QuadRemeshAsync(state.Parameters, state.Progress, state.Cancel)
                    : of.Mesh.QuadRemeshAsync(state.Parameters, state.Guides, state.Progress, state.Cancel))
                from mesh in IO.lift(Missing.Unless(remeshed, nameof(Mesh.QuadRemeshAsync)))
                select mesh,
            ofMeshFaces: static (state, of) =>
                from inside in IO.lift(() => Answers.InRange(of.FaceBlocks, of.Mesh.Faces.Count, nameof(Mesh.Faces)))
                from remeshed in IO.liftAsync(() => of.Mesh.QuadRemeshAsync(of.FaceBlocks, state.Parameters, state.Guides, state.Progress, state.Cancel))
                from mesh in IO.lift(Missing.Unless(remeshed, nameof(Mesh.QuadRemeshAsync)))
                select mesh)
        select mesh;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters, CancellationToken cancel) =>
        IO.lift(() => source.Switch(
            (Parameters: parameters, Cancel: cancel),
            ofMeshes: static (state, of) =>
                from present in Invalid.Unless(!of.Meshes.IsEmpty, nameof(of.Meshes))
                from wrapped in Missing.Unless(Mesh.ShrinkWrap(of.Meshes, state.Parameters, state.Cancel), nameof(Mesh.ShrinkWrap))
                select wrapped,
            ofCloud: static (state, of) => Missing.Unless(Mesh.ShrinkWrap(of.Cloud, state.Parameters, state.Cancel), nameof(Mesh.ShrinkWrap)),
            ofGeometry: static (state, of) => Missing.Unless(Mesh.ShrinkWrap(of.Geometry, state.Parameters, of.Meshing, state.Cancel), nameof(Mesh.ShrinkWrap)),
            ofMesh: static (state, of) => Missing.Unless(of.Mesh.ShrinkWrap(state.Parameters, state.Cancel), nameof(Mesh.ShrinkWrap))));

    public static IO<Mesh> CurvePipe(Curve curve, double radius, int segments, int accuracy, MeshPipeCapStyle cap, bool faceted, Seq<Interval> intervals) =>
        from valid in IO.lift(() =>
            from positive in Limits.Above(0.0).Check(radius, nameof(radius))
            from segmented in Limits.AtLeast(MinimumPipeSegments).Check(segments, nameof(segments))
            from accurate in Limits.AtLeast(1).Check(accuracy, nameof(accuracy))
            select (Radius: positive, Segments: segmented, Accuracy: accurate))
        from mesh in IO.lift(() => Missing.Unless(Mesh.CreateFromCurvePipe(curve, valid.Radius, valid.Segments, valid.Accuracy, cap, faceted, intervals.IsEmpty ? null : intervals), nameof(Mesh.CreateFromCurvePipe)))
        select mesh;

    public static IO<Mesh> CurveExtrude(Curve curve, Vector3d direction, MeshExtrudeMethod method) =>
        IO.lift(() => method.Switch(
            (Curve: curve, Direction: direction),
            free: static (state, _) => Missing.Unless(Mesh.CreateExtrusion(state.Curve, state.Direction), nameof(Mesh.CreateExtrusion)),
            fitted: static (state, of) => Missing.Unless(Mesh.CreateExtrusion(state.Curve, state.Direction, of.Meshing), nameof(Mesh.CreateExtrusion)),
            boxed: static (state, of) =>
                from bounded in Invalid.Unless(of.Bounds.IsValid, nameof(BoundingBox.IsValid))
                from mesh in Missing.Unless(Mesh.CreateFromCurveExtrusion(state.Curve, state.Direction, of.Meshing, of.Bounds), nameof(Mesh.CreateFromCurveExtrusion))
                select mesh));

    public static IO<Mesh> Isosurface(Func<Point3d, double> field, BoundingBox box, int resolution, int rootFindingMaxSteps) =>
        from valid in IO.lift(() =>
            from bounded in Invalid.Unless(box.IsValid, nameof(BoundingBox.IsValid))
            from counted in Seq((Value: resolution, Member: nameof(resolution)), (Value: rootFindingMaxSteps, Member: nameof(rootFindingMaxSteps))).Traverse(static row => Limits.AtLeast(1).Check(row.Value, row.Member)).As()
            select unit)
        from mesh in IO.lift(() => Missing.Unless(Mesh.CreateFromIsosurface(field, box, resolution, rootFindingMaxSteps), nameof(Mesh.CreateFromIsosurface)))
        select mesh;

    public static IO<Mesh> FromLines(Seq<Curve> lines, int maxFaceValence, double tolerance) =>
        from present in IO.lift(() => Invalid.Unless(!lines.IsEmpty, nameof(lines)))
        from mesh in IO.lift(() => Missing.Unless(Mesh.CreateFromLines([.. lines], maxFaceValence, tolerance), nameof(Mesh.CreateFromLines)))
        select mesh;

    public static IO<Mesh> Tessellate(Seq<Point3d> points, Seq<Seq<Point3d>> edges, Plane plane, bool allowNewVertices) =>
        from valid in IO.lift(() =>
            from present in Invalid.Unless(!points.IsEmpty, nameof(points))
            from planar in Invalid.Unless(plane.IsValid, nameof(Plane.IsValid))
            select unit)
        from mesh in IO.lift(() => Missing.Unless(Mesh.CreateFromTessellation(points, edges.Map(static edge => edge.AsEnumerable()), plane, allowNewVertices), nameof(Mesh.CreateFromTessellation)))
        select mesh;

    public static IO<ConvexHullResult> ConvexHull(Seq<Point3d> points, double tolerance, double angleTolerance) =>
        IO.lift(() =>
            from enough in Limits.AtLeast(MinimumHullPoints).Check(points.Count, nameof(points))
            let answer = (Mesh: Mesh.CreateConvexHull3D(points, out int[][] facets, tolerance, angleTolerance), Facets: facets)
            from mesh in Missing.Unless(answer.Mesh, nameof(Mesh.CreateConvexHull3D))
            select new ConvexHullResult(mesh, toSeq(answer.Facets).Map(static facet => toSeq(facet)).Strict()));

    public static IO<Mesh> Patch(Polyline outer, MeshPatchOptions options) =>
        from valid in IO.lift(() =>
            from closed in Invalid.Unless(outer.IsClosed, nameof(Polyline.IsClosed))
            from divided in Limits.AtLeast(1).Check(options.Divisions, nameof(options.Divisions))
            select unit)
        from mesh in IO.lift(() =>
            Missing.Unless(Mesh.CreatePatch(outer, options.AngleToleranceRadians, options.Pullback.ValueUnsafe(), options.InnerBoundaries, options.InnerBothSides, options.InnerPoints, options.Trimback, options.Divisions), nameof(Mesh.CreatePatch)))
        select mesh;

    // --- [REBUILT]
    public static IO<Mesh> Rebuild(Mesh mesh, bool preserveTextureCoordinates, bool preserveVertexColors) =>
        IO.lift(() => Missing.Unless(Mesh.RebuildMesh(mesh, preserveTextureCoordinates, preserveVertexColors), nameof(Mesh.RebuildMesh)));

    public static IO<Seq<Mesh>> FromIterativeCleanup(Seq<Mesh> meshes, double tolerance) =>
        from present in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)))
        from cleaned in GeometryResults.Acquire(() => Mesh.CreateFromIterativeCleanup(meshes, tolerance), nameof(Mesh.CreateFromIterativeCleanup))
        select cleaned;

    public static IO<Mesh> FromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        from present in IO.lift(() => Invalid.Unless(!faces.IsEmpty, nameof(faces)))
        from inside in IO.lift(() => Answers.InRange(faces, mesh.Faces.Count, nameof(Mesh.Faces)))
        let marked = toHashSet(faces)
        from filtered in IO.lift(() => Missing.Unless(Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(marked.Contains)), nameof(Mesh.CreateFromFilteredFaceList)))
        select filtered;

    public static IO<Mesh> Refine(Mesh mesh, RefineMethod method, int level, CreaseEdges nakedEdges, CancellationToken cancel) =>
        from leveled in IO.lift(() => Limits.AtLeast(1).Check(level, nameof(RefinementSettings.Level)))
        from refined in IO.lift(() => method.Switch(
            (Mesh: mesh, Settings: new RefinementSettings { Level = leveled, NakedEdgeMode = nakedEdges, ContinueRequest = cancel }),
            loop: static (state, of) => Missing.Unless(Mesh.CreateRefinedLoopMesh(state.Mesh, of.Formula, state.Settings), nameof(Mesh.CreateRefinedLoopMesh)),
            catmullClark: static (state, _) => Missing.Unless(Mesh.CreateRefinedCatmullClarkMesh(state.Mesh, state.Settings), nameof(Mesh.CreateRefinedCatmullClarkMesh))))
        select refined;
}

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RefineMethod {
    public sealed record Loop(LoopFormula Formula) : RefineMethod;

    public sealed record CatmullClark() : RefineMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshConstruction {
    // --- [LIMITS]
    private static readonly Limits<int> SphereCount = Limits.AtLeast(2);

    // --- [FROM_GEOMETRY]
    public static IO<Seq<Mesh>> FromGeometry(GeometryBase source, MeshingParameters parameters) =>
        source switch {
            Brep brep => GeometryResults.Acquire(() => Mesh.CreateFromBrep(brep, parameters), nameof(Mesh.CreateFromBrep), emptyFails: false),
            Extrusion extrusion => GeometryResults.Acquire(() => Mesh.CreateFromExtrusion(extrusion, parameters), nameof(Mesh.CreateFromExtrusion)).Map(static mesh => Seq(mesh)),
            Surface surface => GeometryResults.Acquire(() => Mesh.CreateFromSurface(surface, parameters), nameof(Mesh.CreateFromSurface)).Map(static mesh => Seq(mesh)),
            _ => IO.fail<Seq<Mesh>>(new WrongType(typeof(Brep), source.GetType())),
        };

    public static IO<Mesh> FromSubD(SubD subd, SubDDisplayParameters.Density density) =>
        GeometryResults.Acquire(() => Mesh.CreateFromSubD(subd, density), nameof(Mesh.CreateFromSubD));

    public static IO<Mesh> ControlNet(CageSource source) =>
        source.Switch(
            ofSubD: static of => of.TextureCoordinates
                ? GeometryResults.Acquire(() => Mesh.CreateFromSubDControlNetWithTextureCoordinates(of.SubD), nameof(Mesh.CreateFromSubDControlNetWithTextureCoordinates))
                : GeometryResults.Acquire(() => Mesh.CreateFromSubDControlNet(of.SubD), nameof(Mesh.CreateFromSubDControlNet)),
            ofSurface: static of => GeometryResults.Acquire(() => Mesh.CreateFromSurfaceControlNet(of.Surface), nameof(Mesh.CreateFromSurfaceControlNet)));

    public static IO<Mesh> FromBoundary(Curve boundary, MeshingParameters parameters, double tolerance) =>
        GeometryResults.Acquire(() => Mesh.CreateFromPlanarBoundary(boundary, parameters, tolerance), nameof(Mesh.CreateFromPlanarBoundary));

    public static IO<Mesh> Create(MeshSource source) =>
        source.Switch(
            ofPlane: static of => GeometryResults.Acquire(() => Mesh.CreateFromPlane(of.Plane, of.X, of.Y, of.XCount, of.YCount), nameof(Mesh.CreateFromPlane)),
            ofBox: static of => GeometryResults.Acquire(() => Mesh.CreateFromBox(of.Box, of.XCount, of.YCount, of.ZCount), nameof(Mesh.CreateFromBox)),
            ofBounds: static of => GeometryResults.Acquire(() => Mesh.CreateFromBox(of.Bounds, of.XCount, of.YCount, of.ZCount), nameof(Mesh.CreateFromBox)),
            ofCorners: static of => GeometryResults.Acquire(
                () => CountMismatch.Unless(SolidConstruction.BoxCorners, of.Corners.Count, nameof(Mesh.CreateFromBox)).Map(_ => Mesh.CreateFromBox(of.Corners, of.XCount, of.YCount, of.ZCount)),
                nameof(Mesh.CreateFromBox)),
            ofSphere: static of => GeometryResults.Acquire(
                () => (SphereCount.Check(of.XCount, nameof(of.XCount)), SphereCount.Check(of.YCount, nameof(of.YCount))).Apply((x, y) => Mesh.CreateFromSphere(of.Sphere, x, y)).As(),
                nameof(Mesh.CreateFromSphere)),
            icoSphere: static of => GeometryResults.Acquire(() => Mesh.CreateIcoSphere(of.Sphere, of.Subdivisions), nameof(Mesh.CreateIcoSphere)),
            quadSphere: static of => GeometryResults.Acquire(() => Mesh.CreateQuadSphere(of.Sphere, of.Subdivisions), nameof(Mesh.CreateQuadSphere)),
            ofCylinder: static of => GeometryResults.Acquire(
                () => Mesh.CreateFromCylinder(of.Cylinder, of.Vertical, of.Around, of.CapBottom, of.CapTop, of.Circumscribe, of.QuadCaps),
                nameof(Mesh.CreateFromCylinder)),
            ofCone: static of => GeometryResults.Acquire(() => Mesh.CreateFromCone(of.Cone, of.Vertical, of.Around, of.Solid, of.QuadCaps), nameof(Mesh.CreateFromCone)),
            ofTorus: static of => GeometryResults.Acquire(() => Mesh.CreateFromTorus(of.Torus, of.Vertical, of.Around), nameof(Mesh.CreateFromTorus)),
            closedPolyline: static of => GeometryResults.Acquire(
                () => Invalid.Unless(of.Polyline.IsClosed, nameof(MeshSource.ClosedPolyline.Polyline)).Map(_ => Mesh.CreateFromClosedPolyline(of.Polyline)),
                nameof(Mesh.CreateFromClosedPolyline)));

    // --- [GENERATED]
    public static IO<Mesh> QuadRemesh(QuadSource source, QuadRemeshParameters parameters, Seq<Curve> guides, Option<IProgress<int>> progress, CancellationToken cancel) =>
        from answer in source.Switch(
            (Parameters: parameters, Guides: guides, Progress: progress.ValueUnsafe(), Cancel: cancel),
            ofBrep: static (state, of) => IO.liftAsync(() => state.Guides.IsEmpty
                    ? Mesh.QuadRemeshBrepAsync(of.Brep, state.Parameters, state.Progress, state.Cancel)
                    : Mesh.QuadRemeshBrepAsync(of.Brep, state.Parameters, state.Guides, state.Progress, state.Cancel))
                .Map(static mesh => (Mesh: mesh, Member: nameof(Mesh.QuadRemeshBrepAsync))),
            ofMesh: static (state, of) => IO.liftAsync(() => state.Guides.IsEmpty
                    ? of.Mesh.QuadRemeshAsync(state.Parameters, state.Progress, state.Cancel)
                    : of.Mesh.QuadRemeshAsync(state.Parameters, state.Guides, state.Progress, state.Cancel))
                .Map(static mesh => (Mesh: mesh, Member: nameof(Mesh.QuadRemeshAsync))),
            ofMeshFaces: static (state, of) =>
                from inside in IO.lift(() => GeometryResults.InRange(of.FaceBlocks, of.Mesh.Faces.Count, nameof(Mesh.Faces)))
                from mesh in IO.liftAsync(() => of.Mesh.QuadRemeshAsync(of.FaceBlocks, state.Parameters, state.Guides, state.Progress, state.Cancel))
                select (Mesh: mesh, Member: nameof(Mesh.QuadRemeshAsync)))
        from mesh in GeometryResults.Acquire(() => Missing.Unless(answer.Mesh, answer.Member).BindFail(missing => cancel.IsCancellationRequested ? new Canceled() : missing), answer.Member)
        select mesh;

    public static IO<Mesh> ShrinkWrap(ShrinkWrapSource source, ShrinkWrapParameters parameters, CancellationToken cancel) =>
        source.Switch(
            (Parameters: parameters, Cancel: cancel),
            ofMeshes: static (state, of) => GeometryResults.Acquire(
                () => Invalid.Unless(!of.Meshes.IsEmpty, nameof(of.Meshes)).Map(_ => Mesh.ShrinkWrap(of.Meshes, state.Parameters, state.Cancel)),
                nameof(Mesh.ShrinkWrap)),
            ofCloud: static (state, of) => GeometryResults.Acquire(() => Mesh.ShrinkWrap(of.Cloud, state.Parameters, state.Cancel), nameof(Mesh.ShrinkWrap)),
            ofGeometry: static (state, of) => GeometryResults.Acquire(() => Mesh.ShrinkWrap(of.Geometry, state.Parameters, of.Meshing, state.Cancel), nameof(Mesh.ShrinkWrap)),
            ofMesh: static (state, of) => GeometryResults.Acquire(() => of.Mesh.ShrinkWrap(state.Parameters, state.Cancel), nameof(Mesh.ShrinkWrap)));

    public static IO<Mesh> CurvePipe(Curve curve, double radius, int segments, int accuracy, MeshPipeCapStyle cap, bool faceted, Seq<Interval> intervals) =>
        GeometryResults.Acquire(
            () => Limits.Above(0.0).Check(radius, nameof(radius)).Map(positive => Mesh.CreateFromCurvePipe(curve, positive, segments, accuracy, cap, faceted, intervals)),
            nameof(Mesh.CreateFromCurvePipe));

    public static IO<Mesh> CurveExtrude(Curve curve, Vector3d direction, MeshExtrudeMethod method) =>
        method.Switch(
            (Curve: curve, Direction: direction),
            free: static (state, _) => GeometryResults.Acquire(() => Mesh.CreateExtrusion(state.Curve, state.Direction), nameof(Mesh.CreateExtrusion)),
            fitted: static (state, of) => GeometryResults.Acquire(() => Mesh.CreateExtrusion(state.Curve, state.Direction, of.Meshing), nameof(Mesh.CreateExtrusion)),
            boxed: static (state, of) => GeometryResults.Acquire(
                () => Invalid.Unless(of.Bounds.IsValid, nameof(MeshExtrudeMethod.Boxed.Bounds)).Map(_ => Mesh.CreateFromCurveExtrusion(state.Curve, state.Direction, of.Meshing, of.Bounds)),
                nameof(Mesh.CreateFromCurveExtrusion)));

    public static IO<Mesh> Isosurface(Func<Point3d, double> field, BoundingBox box, int resolution, int rootFindingMaxSteps) =>
        GeometryResults.Acquire(
            () => (Invalid.Unless(box.IsValid, nameof(box)), Limits.AtLeast(1).Check(resolution, nameof(resolution)), Limits.AtLeast(1).Check(rootFindingMaxSteps, nameof(rootFindingMaxSteps)))
                .Apply((_, cells, steps) => Mesh.CreateFromIsosurface(field, box, cells, steps))
                .As(),
            nameof(Mesh.CreateFromIsosurface));

    public static IO<Mesh> FromLines(Seq<Curve> lines, int maxFaceValence, double tolerance) =>
        GeometryResults.Acquire(() => Mesh.CreateFromLines([.. lines], maxFaceValence, tolerance), nameof(Mesh.CreateFromLines))
            .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<Mesh>(new Invalid(nameof(lines))));

    public static IO<Mesh> Tessellate(Seq<Point3d> points, Seq<Seq<Point3d>> edges, Plane plane, bool allowNewVertices) =>
        GeometryResults.Acquire(
            () => (Invalid.Unless(!points.IsEmpty, nameof(points)), Invalid.Unless(plane.IsValid, nameof(plane)))
                .Apply((_, _) => Mesh.CreateFromTessellation(points, edges.Map(static edge => edge.AsEnumerable()), plane, allowNewVertices))
                .As(),
            nameof(Mesh.CreateFromTessellation));

    public static IO<ConvexHullResult> ConvexHull(Seq<Point3d> points, double tolerance, double angleTolerance) =>
        from answer in IO.lift(() => (Mesh: Mesh.CreateConvexHull3D(points, out int[][] facets, tolerance, angleTolerance), Facets: facets))
        from mesh in GeometryResults.Acquire(() => answer.Mesh, nameof(Mesh.CreateConvexHull3D))
        select new ConvexHullResult(mesh, toSeq(answer.Facets).Map(static facet => toSeq(facet)).Strict());

    public static IO<Mesh> Patch(Polyline outer, double angleToleranceRadians, Option<Surface> pullback, Seq<Curve> innerBoundaries, Seq<Curve> innerBothSides, Seq<Point3d> innerPoints, bool trimback, int divisions) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(outer.IsClosed, nameof(outer))
                .Map(_ => Mesh.CreatePatch(outer, angleToleranceRadians, pullback.ValueUnsafe(), innerBoundaries, innerBothSides, innerPoints, trimback, divisions)),
            nameof(Mesh.CreatePatch));

    // --- [REBUILT]
    public static IO<Mesh> Rebuild(Mesh mesh, bool preserveTextureCoordinates, bool preserveVertexColors) =>
        GeometryResults.Acquire(() => Mesh.RebuildMesh(mesh, preserveTextureCoordinates, preserveVertexColors), nameof(Mesh.RebuildMesh));

    public static IO<Seq<Mesh>> FromIterativeCleanup(Seq<Mesh> meshes, double tolerance) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)).Map(_ => Mesh.CreateFromIterativeCleanup(meshes, tolerance)),
            nameof(Mesh.CreateFromIterativeCleanup),
            emptyFails: false);

    public static IO<Mesh> FromFilteredFaceList(Mesh mesh, Seq<int> faces) =>
        GeometryResults.Acquire(
            () => (Invalid.Unless(!faces.IsEmpty, nameof(faces)), GeometryResults.InRange(faces, mesh.Faces.Count, nameof(Mesh.Faces)))
                .Apply((_, _) => toHashSet(faces))
                .As()
                .Map(marked => Mesh.CreateFromFilteredFaceList(mesh, toSeq(Range(0, mesh.Faces.Count)).Map(marked.Contains))),
            nameof(Mesh.CreateFromFilteredFaceList));

    public static IO<Mesh> Refine(Mesh mesh, RefineMethod method, int level, CreaseEdges nakedEdges, CancellationToken cancel) =>
        from settings in IO.lift(() => Limits.AtLeast(0).Check(level, nameof(RefinementSettings.Level)).Map(leveled => new RefinementSettings { Level = leveled, NakedEdgeMode = nakedEdges, ContinueRequest = cancel }))
        from refined in method.Switch(
            (Mesh: mesh, Settings: settings),
            loop: static (state, of) => GeometryResults.Acquire(() => Mesh.CreateRefinedLoopMesh(state.Mesh, of.Formula, state.Settings), nameof(Mesh.CreateRefinedLoopMesh)),
            catmullClark: static (state, _) => GeometryResults.Acquire(() => Mesh.CreateRefinedCatmullClarkMesh(state.Mesh, state.Settings), nameof(Mesh.CreateRefinedCatmullClarkMesh)))
        select refined;
}

using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidSource {
    public sealed record OfBox(Box Box) : SolidSource;

    public sealed record OfBounds(BoundingBox Bounds) : SolidSource;

    public sealed record OfCorners(Seq<Point3d> Corners) : SolidSource;

    public sealed record OfCylinder(Cylinder Cylinder, bool CapBottom, bool CapTop) : SolidSource;

    public sealed record OfCone(Cone Cone, bool CapBottom) : SolidSource;

    public sealed record OfTorus(Torus Torus) : SolidSource;

    public sealed record OfSphere(Sphere Sphere) : SolidSource;

    public sealed record QuadSphere(Sphere Sphere) : SolidSource;

    public sealed record Baseball(Point3d Center, double Radius, double Tolerance) : SolidSource;

    public sealed record Triangle(Point3d A, Point3d B, Point3d C, double Tolerance) : SolidSource;

    public sealed record Quad(Point3d A, Point3d B, Point3d C, Point3d D, double Tolerance) : SolidSource;

    public sealed record FromSurface(Surface Surface) : SolidSource;

    public sealed record FromRevolve(RevSurface Surface, bool CapStart, bool CapEnd) : SolidSource;

    public sealed record FromMesh(Mesh Mesh, bool TrimmedTriangles) : SolidSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionSource {
    public sealed record Planar(Curve Curve, double Height, bool Cap) : ExtrusionSource;

    public sealed record FramedProfile(Curve Curve, Plane Plane, double Height, bool Cap) : ExtrusionSource;

    public sealed record OfBox(Box Box, bool Cap) : ExtrusionSource;

    public sealed record OfCylinder(Cylinder Cylinder, bool CapBottom, bool CapTop) : ExtrusionSource;

    public sealed record OfPipe(Cylinder Cylinder, double OtherRadius, bool CapTop, bool CapBottom) : ExtrusionSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionCurveRead {
    public sealed record Wireframe() : ExtrusionCurveRead;

    public sealed record ProfileCurve(int Index, double Station) : ExtrusionCurveRead;

    public sealed record WallEdge(ComponentIndex Component) : ExtrusionCurveRead;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidConstruction {
    // --- [LIMITS]
    internal const int BoxCorners = 8;

    private static readonly Limits<int> EdgeCount = Limits.AtMost(4);

    // --- [BREPS]
    public static IO<Brep> Create(SolidSource source) =>
        source.Switch(
            ofBox: static box => GeometryResults.Acquire(() => Brep.CreateFromBox(box.Box), nameof(Brep.CreateFromBox)),
            ofBounds: static bounds => GeometryResults.Acquire(() => Brep.CreateFromBox(bounds.Bounds), nameof(Brep.CreateFromBox)),
            ofCorners: static corners => GeometryResults.Acquire(
                () => CountMismatch.Unless(BoxCorners, corners.Corners.Count, nameof(Brep.CreateFromBox)).Map(_ => Brep.CreateFromBox(corners.Corners)),
                nameof(Brep.CreateFromBox)),
            ofCylinder: static cylinder => GeometryResults.Acquire(() => Brep.CreateFromCylinder(cylinder.Cylinder, cylinder.CapBottom, cylinder.CapTop), nameof(Brep.CreateFromCylinder)),
            ofCone: static cone => GeometryResults.Acquire(() => Brep.CreateFromCone(cone.Cone, cone.CapBottom), nameof(Brep.CreateFromCone)),
            ofTorus: static torus => GeometryResults.Acquire(() => Brep.CreateFromTorus(torus.Torus), nameof(Brep.CreateFromTorus)),
            ofSphere: static sphere => GeometryResults.Acquire(() => Brep.CreateFromSphere(sphere.Sphere), nameof(Brep.CreateFromSphere)),
            quadSphere: static sphere => GeometryResults.Acquire(() => Brep.CreateQuadSphere(sphere.Sphere), nameof(Brep.CreateQuadSphere)),
            baseball: static ball => GeometryResults.Acquire(() => Brep.CreateBaseballSphere(ball.Center, ball.Radius, ball.Tolerance), nameof(Brep.CreateBaseballSphere)),
            triangle: static triangle => GeometryResults.Acquire(() => Brep.CreateFromCornerPoints(triangle.A, triangle.B, triangle.C, triangle.Tolerance), nameof(Brep.CreateFromCornerPoints)),
            quad: static quad => GeometryResults.Acquire(() => Brep.CreateFromCornerPoints(quad.A, quad.B, quad.C, quad.D, quad.Tolerance), nameof(Brep.CreateFromCornerPoints)),
            fromSurface: static surface => GeometryResults.Acquire(() => Brep.CreateFromSurface(surface.Surface), nameof(Brep.CreateFromSurface)),
            fromRevolve: static revolved => GeometryResults.Acquire(() => Brep.CreateFromRevSurface(revolved.Surface, revolved.CapStart, revolved.CapEnd), nameof(Brep.CreateFromRevSurface)),
            fromMesh: static meshed => GeometryResults.Acquire(() => Brep.CreateFromMesh(meshed.Mesh, meshed.TrimmedTriangles), nameof(Brep.CreateFromMesh)));

    public static IO<Seq<Brep>> TaperedExtrude(Curve curve, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType corner, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(
            () => Brep.CreateFromTaperedExtrude(curve, distance, direction, basePoint, draftAngleRadians, corner, tolerance, angleTolerance),
            nameof(Brep.CreateFromTaperedExtrude),
            emptyFails: false);

    public static IO<Seq<Brep>> TaperedExtrudeWithRef(Curve curve, Vector3d direction, double distance, double draftAngle, Plane plane, double tolerance) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(plane.IsValid, nameof(plane)).Map(_ => Brep.CreateFromTaperedExtrudeWithRef(curve, direction, distance, draftAngle, plane, tolerance)),
            nameof(Brep.CreateFromTaperedExtrudeWithRef),
            emptyFails: false);

    public static IO<Seq<Brep>> PlanarFill(Seq<Curve> loops, double tolerance) =>
        GeometryResults.Acquire(() => Brep.CreatePlanarBreps(loops, tolerance), nameof(Brep.CreatePlanarBreps), emptyFails: false);

    public static IO<Brep> EdgeSurface(Seq<Curve> edges) =>
        GeometryResults.Acquire(() => EdgeCount.Check(edges.Count, nameof(edges)).Map(_ => Brep.CreateEdgeSurface(edges)), nameof(Brep.CreateEdgeSurface));

    public static IO<Brep> TrimmedPlane(Plane plane, Seq<Curve> loops) =>
        GeometryResults.Acquire(() => Invalid.Unless(plane.IsValid, nameof(plane)).Map(_ => Brep.CreateTrimmedPlane(plane, loops)), nameof(Brep.CreateTrimmedPlane));

    // --- [EXTRUSIONS]
    public static IO<Extrusion> Create(ExtrusionSource source) =>
        source.Switch(
            planar: static planar => GeometryResults.Acquire(() => Extrusion.Create(planar.Curve, planar.Height, planar.Cap), nameof(Extrusion.Create)),
            framedProfile: static profile => GeometryResults.Acquire(() => Extrusion.Create(profile.Curve, profile.Plane, profile.Height, profile.Cap), nameof(Extrusion.Create)),
            ofBox: static box => GeometryResults.Acquire(() => Extrusion.CreateBoxExtrusion(box.Box, box.Cap), nameof(Extrusion.CreateBoxExtrusion)),
            ofCylinder: static cylinder => GeometryResults.Acquire(() => Extrusion.CreateCylinderExtrusion(cylinder.Cylinder, cylinder.CapBottom, cylinder.CapTop), nameof(Extrusion.CreateCylinderExtrusion)),
            ofPipe: static pipe => GeometryResults.Acquire(() => Extrusion.CreatePipeExtrusion(pipe.Cylinder, pipe.OtherRadius, pipe.CapTop, pipe.CapBottom), nameof(Extrusion.CreatePipeExtrusion)));

    public static IO<Extrusion> Reprofile(Extrusion source, Curve outer, Seq<Curve> inners, bool cap, Option<(Point3d A, Point3d B, Vector3d Up)> path) =>
        GeometryResults.EditCopy(source, copy =>
                from pathed in path.Traverse(frame => Refused.Unless(copy.SetPathAndUp(frame.A, frame.B, frame.Up), nameof(Extrusion.SetPathAndUp))).As()
                from profiled in Refused.Unless(copy.SetOuterProfile(outer, cap), nameof(Extrusion.SetOuterProfile))
                from holed in inners.TraverseM(inner => Refused.Unless(copy.AddInnerProfile(inner), nameof(Extrusion.AddInnerProfile))).As()
                select unit)
            .Map(static edited => edited.Copy);

    public static IO<Brep> ToBrep(Extrusion extrusion, bool splitKinkyFaces) =>
        GeometryResults.Acquire(() => extrusion.ToBrep(splitKinkyFaces), nameof(Extrusion.ToBrep));

    public static IO<Seq<Curve>> ExtrusionCurves(Extrusion extrusion, ExtrusionCurveRead read) =>
        read.Switch(
            extrusion,
            wireframe: static (source, _) => GeometryResults.Acquire(source.GetWireframe, nameof(Extrusion.GetWireframe), emptyFails: false),
            profileCurve: static (source, profile) => GeometryResults.Acquire(() => source.Profile3d(profile.Index, profile.Station), nameof(Extrusion.Profile3d)).Map(static curve => Seq(curve)),
            wallEdge: static (source, wall) => GeometryResults.Acquire(
                    () => Invalid.Unless(wall.Component.ComponentIndexType == ComponentIndexType.ExtrusionWallEdge, nameof(ExtrusionCurveRead.WallEdge.Component)).Map(_ => source.WallEdge(wall.Component)),
                    nameof(Extrusion.WallEdge))
                .Map(static curve => Seq(curve)));

    public static IO<Surface> WallSurface(Extrusion extrusion, ComponentIndex component) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(component.ComponentIndexType == ComponentIndexType.ExtrusionWallSurface, nameof(component)).Map(_ => extrusion.WallSurface(component)),
            nameof(Extrusion.WallSurface));

    public static IO<Mesh> RenderMesh(Extrusion extrusion, MeshType kind) =>
        GeometryResults.Acquire(() => Missing.Unless(extrusion.GetMesh(kind), nameof(Extrusion.GetMesh)).Map(static cached => cached.DuplicateMesh()), nameof(Mesh.DuplicateMesh));
}

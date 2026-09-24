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

    private static readonly Fin<Limits<int>> EdgeCount = Limits.AtLeast(2).AtMost(4, nameof(EdgeCount));

    // --- [BREPS]
    public static IO<Brep> Create(SolidSource source) =>
        IO.lift(() => source.Switch(
            ofBox: static box => Missing.Unless(Brep.CreateFromBox(box.Box), nameof(Brep.CreateFromBox)),
            ofBounds: static bounds => Missing.Unless(Brep.CreateFromBox(bounds.Bounds), nameof(Brep.CreateFromBox)),
            ofCorners: static corners =>
                from counted in CountMismatch.Unless(BoxCorners, corners.Corners.Count, nameof(Brep.CreateFromBox))
                from brep in Missing.Unless(Brep.CreateFromBox(corners.Corners), nameof(Brep.CreateFromBox))
                select brep,
            ofCylinder: static cylinder => Missing.Unless(Brep.CreateFromCylinder(cylinder.Cylinder, cylinder.CapBottom, cylinder.CapTop), nameof(Brep.CreateFromCylinder)),
            ofCone: static cone => Missing.Unless(Brep.CreateFromCone(cone.Cone, cone.CapBottom), nameof(Brep.CreateFromCone)),
            ofTorus: static torus => Missing.Unless(Brep.CreateFromTorus(torus.Torus), nameof(Brep.CreateFromTorus)),
            ofSphere: static sphere => Missing.Unless(Brep.CreateFromSphere(sphere.Sphere), nameof(Brep.CreateFromSphere)),
            quadSphere: static sphere => Missing.Unless(Brep.CreateQuadSphere(sphere.Sphere), nameof(Brep.CreateQuadSphere)),
            baseball: static ball =>
                from positive in Limits.Above(0.0).Check(ball.Radius, nameof(SolidSource.Baseball.Radius))
                from brep in Missing.Unless(Brep.CreateBaseballSphere(ball.Center, positive, ball.Tolerance), nameof(Brep.CreateBaseballSphere))
                select brep,
            triangle: static triangle => Missing.Unless(Brep.CreateFromCornerPoints(triangle.A, triangle.B, triangle.C, triangle.Tolerance), nameof(Brep.CreateFromCornerPoints)),
            quad: static quad => Missing.Unless(Brep.CreateFromCornerPoints(quad.A, quad.B, quad.C, quad.D, quad.Tolerance), nameof(Brep.CreateFromCornerPoints)),
            fromSurface: static surface => Missing.Unless(Brep.CreateFromSurface(surface.Surface), nameof(Brep.CreateFromSurface)),
            fromRevolve: static revolved => Missing.Unless(Brep.CreateFromRevSurface(revolved.Surface, revolved.CapStart, revolved.CapEnd), nameof(Brep.CreateFromRevSurface)),
            fromMesh: static meshed => Missing.Unless(Brep.CreateFromMesh(meshed.Mesh, meshed.TrimmedTriangles), nameof(Brep.CreateFromMesh))));

    public static IO<Seq<Brep>> TaperedExtrude(Curve curve, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType corner, double tolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => Brep.CreateFromTaperedExtrude(curve, distance, direction, basePoint, draftAngleRadians, corner, tolerance, angleTolerance), nameof(Brep.CreateFromTaperedExtrude));

    public static IO<Seq<Brep>> TaperedExtrudeWithRef(Curve curve, Vector3d direction, double distance, double draftAngle, Plane plane, double tolerance) =>
        from valid in IO.lift(() => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
        from breps in GeometryResults.Acquire(() => Brep.CreateFromTaperedExtrudeWithRef(curve, direction, distance, draftAngle, plane, tolerance), nameof(Brep.CreateFromTaperedExtrudeWithRef))
        select breps;

    public static IO<Seq<Brep>> PlanarFill(Seq<Curve> loops, double tolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!loops.IsEmpty, nameof(loops)))
        from breps in GeometryResults.Acquire(() => Brep.CreatePlanarBreps(loops, tolerance), nameof(Brep.CreatePlanarBreps))
        select breps;

    public static IO<Brep> EdgeSurface(Seq<Curve> edges) =>
        from counted in IO.lift(() => EdgeCount.Bind(limits => limits.Check(edges.Count, nameof(edges))))
        from brep in IO.lift(() => Missing.Unless(Brep.CreateEdgeSurface(edges), nameof(Brep.CreateEdgeSurface)))
        select brep;

    public static IO<Brep> TrimmedPlane(Plane plane, Seq<Curve> loops) =>
        from valid in IO.lift(() =>
            from planar in Invalid.Unless(plane.IsValid, nameof(Plane.IsValid))
            from filled in Invalid.Unless(!loops.IsEmpty, nameof(loops))
            select unit)
        from brep in IO.lift(() => Missing.Unless(Brep.CreateTrimmedPlane(plane, loops), nameof(Brep.CreateTrimmedPlane)))
        select brep;

    // --- [EXTRUSIONS]
    public static IO<Extrusion> Create(ExtrusionSource source) =>
        IO.lift(() => source.Switch(
            planar: static planar => Missing.Unless(Extrusion.Create(planar.Curve, planar.Height, planar.Cap), nameof(Extrusion.Create)),
            framedProfile: static profile => Missing.Unless(Extrusion.Create(profile.Curve, profile.Plane, profile.Height, profile.Cap), nameof(Extrusion.Create)),
            ofBox: static box => Missing.Unless(Extrusion.CreateBoxExtrusion(box.Box, box.Cap), nameof(Extrusion.CreateBoxExtrusion)),
            ofCylinder: static cylinder => Missing.Unless(Extrusion.CreateCylinderExtrusion(cylinder.Cylinder, cylinder.CapBottom, cylinder.CapTop), nameof(Extrusion.CreateCylinderExtrusion)),
            ofPipe: static pipe => Missing.Unless(Extrusion.CreatePipeExtrusion(pipe.Cylinder, pipe.OtherRadius, pipe.CapTop, pipe.CapBottom), nameof(Extrusion.CreatePipeExtrusion))));

    public static IO<Extrusion> Reprofile(Extrusion source, Curve outer, Seq<Curve> inners, bool cap, Option<(Point3d A, Point3d B, Vector3d Up)> path) =>
        GeometryOps.EditCopy(source, copy =>
                from pathed in path.Traverse(frame => Refused.Unless(copy.SetPathAndUp(frame.A, frame.B, frame.Up), nameof(Extrusion.SetPathAndUp))).As()
                from profiled in Refused.Unless(copy.SetOuterProfile(outer, cap), nameof(Extrusion.SetOuterProfile))
                from holed in inners.TraverseM(inner => Refused.Unless(copy.AddInnerProfile(inner), nameof(Extrusion.AddInnerProfile))).As()
                select unit)
            .Map(static edited => edited.Copy);

    public static IO<Brep> ToBrep(Extrusion extrusion, bool splitKinkyFaces) =>
        IO.lift(() => Missing.Unless(extrusion.ToBrep(splitKinkyFaces), nameof(Extrusion.ToBrep)));

    public static IO<Seq<Curve>> ExtrusionCurves(Extrusion extrusion, ExtrusionCurveRead read) =>
        read.Switch(
            extrusion,
            wireframe: static (source, _) => GeometryResults.Acquire(source.GetWireframe, nameof(Extrusion.GetWireframe)),
            profileCurve: static (source, profile) => IO.lift(() => Missing.Unless(source.Profile3d(profile.Index, profile.Station), nameof(Extrusion.Profile3d)).Map(static curve => Seq(curve))),
            wallEdge: static (source, wall) => IO.lift(() =>
                from typed in Invalid.Unless(wall.Component.ComponentIndexType == ComponentIndexType.ExtrusionWallEdge, nameof(ComponentIndex.ComponentIndexType))
                from curve in Missing.Unless(source.WallEdge(wall.Component), nameof(Extrusion.WallEdge))
                select Seq(curve)));

    public static IO<Surface> WallSurface(Extrusion extrusion, ComponentIndex component) =>
        from typed in IO.lift(() => Invalid.Unless(component.ComponentIndexType == ComponentIndexType.ExtrusionWallSurface, nameof(ComponentIndex.ComponentIndexType)))
        from wall in IO.lift(() => Missing.Unless(extrusion.WallSurface(component), nameof(Extrusion.WallSurface)))
        select wall;

    public static IO<Mesh> RenderMesh(Extrusion extrusion, MeshType kind) =>
        IO.lift(() => Missing.Unless(extrusion.GetMesh(kind), nameof(Extrusion.GetMesh)).Map(static cached => cached.DuplicateMesh()));
}

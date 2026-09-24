using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
public enum LiftDirection { Normal = 0, Tangent = 1 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetFrame {
    public sealed record InPlane(Plane Plane, CurveOffsetCornerStyle Corner) : OffsetFrame;

    public sealed record ByNormal(Point3d DirectionPoint, Vector3d Normal, bool Loose, CurveOffsetCornerStyle Corner, CurveOffsetEndStyle End) : OffsetFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOffsetTarget {
    public sealed record FaceDistance(BrepFace Face, double Distance) : SurfaceOffsetTarget;

    public sealed record FacePoint(BrepFace Face, Point2d Through) : SurfaceOffsetTarget;

    public sealed record FaceVarying(BrepFace Face, Seq<(double Parameter, double Distance)> Rows) : SurfaceOffsetTarget;

    public sealed record SurfaceDistance(Surface Surface, double Distance) : SurfaceOffsetTarget;

    public sealed record SurfacePoint(Surface Surface, Point2d Through) : SurfaceOffsetTarget;

    public sealed record SurfaceVarying(Surface Surface, Seq<(double Parameter, double Distance)> Rows) : SurfaceOffsetTarget;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RibbonOffsetResult {
    public sealed record Rails(Seq<Curve> RailCurves, Option<Seq<Curve>> CrossSectionCurves) : RibbonOffsetResult;

    public sealed record Surfaced(Curve Ribbon, Seq<Curve> RailCurves, Option<Seq<Curve>> CrossSectionCurves, Seq<Brep> BrepSurfaces) : RibbonOffsetResult;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PullTarget {
    public sealed record ToFace(BrepFace Face, bool Loose) : PullTarget;

    public sealed record ToMesh(Mesh Mesh, bool Loose) : PullTarget;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveOffsets {
    // --- [OFFSETS]
    public static IO<Seq<Curve>> Offset(Curve curve, OffsetFrame frame, double distance, double tolerance, double angleTolerance) =>
        frame.Switch(
            (Curve: curve, Distance: distance, Tolerance: tolerance, AngleTolerance: angleTolerance),
            inPlane: static (state, plane) =>
                from planar in IO.lift(() => Invalid.Unless(plane.Plane.IsValid, nameof(Plane.IsValid)))
                from offsets in GeometryResults.Acquire(() => state.Curve.Offset(plane.Plane, state.Distance, state.Tolerance, plane.Corner), nameof(Curve.Offset))
                select offsets,
            byNormal: static (state, normal) => GeometryResults.Acquire(
                () => state.Curve.Offset(normal.DirectionPoint, normal.Normal, state.Distance, state.Tolerance, state.AngleTolerance, normal.Loose, normal.Corner, normal.End),
                nameof(Curve.Offset)));

    public static IO<Seq<Curve>> OffsetOnSurface(Curve curve, SurfaceOffsetTarget target, double fittingTolerance) =>
        target.Switch(
            (Curve: curve, Tolerance: fittingTolerance),
            faceDistance: static (state, face) => GeometryResults.Acquire(() => state.Curve.OffsetOnSurface(face.Face, face.Distance, state.Tolerance), nameof(Curve.OffsetOnSurface)),
            facePoint: static (state, face) => GeometryResults.Acquire(() => state.Curve.OffsetOnSurface(face.Face, face.Through, state.Tolerance), nameof(Curve.OffsetOnSurface)),
            faceVarying: static (state, face) =>
                from filled in IO.lift(() => Invalid.Unless(!face.Rows.IsEmpty, nameof(SurfaceOffsetTarget.FaceVarying.Rows)))
                from offsets in GeometryResults.Acquire(
                    () => state.Curve.OffsetOnSurface(face.Face, [.. face.Rows.Map(static row => row.Parameter)], [.. face.Rows.Map(static row => row.Distance)], state.Tolerance),
                    nameof(Curve.OffsetOnSurface))
                select offsets,
            surfaceDistance: static (state, surface) => GeometryResults.Acquire(() => state.Curve.OffsetOnSurface(surface.Surface, surface.Distance, state.Tolerance), nameof(Curve.OffsetOnSurface)),
            surfacePoint: static (state, surface) => GeometryResults.Acquire(() => state.Curve.OffsetOnSurface(surface.Surface, surface.Through, state.Tolerance), nameof(Curve.OffsetOnSurface)),
            surfaceVarying: static (state, surface) =>
                from filled in IO.lift(() => Invalid.Unless(!surface.Rows.IsEmpty, nameof(SurfaceOffsetTarget.SurfaceVarying.Rows)))
                from offsets in GeometryResults.Acquire(
                    () => state.Curve.OffsetOnSurface(surface.Surface, [.. surface.Rows.Map(static row => row.Parameter)], [.. surface.Rows.Map(static row => row.Distance)], state.Tolerance),
                    nameof(Curve.OffsetOnSurface))
                select offsets);

    public static IO<Curve> OffsetLift(Curve curve, Surface surface, double height, LiftDirection direction) =>
        IO.lift(() => direction switch {
            LiftDirection.Normal => Missing.Unless(curve.OffsetNormalToSurface(surface, height), nameof(Curve.OffsetNormalToSurface)),
            LiftDirection.Tangent => Missing.Unless(curve.OffsetTangentToSurface(surface, height), nameof(Curve.OffsetTangentToSurface)),
        });

    public static IO<RibbonOffsetResult> Ribbon(Curve curve, RibbonOffsetParameters parameters) =>
        from answer in IO.lift(() => (Ribbon: curve.RibbonOffset(parameters, out Curve[]? rails, out Curve[]? sections, out Brep[] breps), Rails: rails, Sections: sections, Breps: breps))
        from result in IO.lift(() => parameters.RibbonSurfaceGenerationMethod == RibbonOffsetSurfaceMethod.None
            ? Missing.Unless(answer.Rails, nameof(Curve.RibbonOffset))
                .Map<RibbonOffsetResult>(rails => new RibbonOffsetResult.Rails(toSeq(rails), Optional(answer.Sections).Map(toSeq)))
            : Missing.Unless(answer.Ribbon, nameof(Curve.RibbonOffset))
                .Map<RibbonOffsetResult>(ribbon => new RibbonOffsetResult.Surfaced(ribbon, toSeq(answer.Rails), Optional(answer.Sections).Map(toSeq), toSeq(answer.Breps))))
        select result;

    // --- [ONTO_GEOMETRY]
    public static IO<Seq<Curve>> Pull(Curve curve, PullTarget target, double tolerance) =>
        target.Switch(
            (Curve: curve, Tolerance: tolerance),
            toFace: static (state, face) => GeometryResults.Acquire(() => Curve.PullToBrepFace(state.Curve, face.Face, state.Tolerance, face.Loose), nameof(Curve.PullToBrepFace)),
            toMesh: static (state, mesh) => IO.lift(() =>
                Missing.Unless(mesh.Loose ? state.Curve.PullToMesh(mesh.Mesh, state.Tolerance, loose: true) : state.Curve.PullToMesh(mesh.Mesh, state.Tolerance), nameof(Curve.PullToMesh))
                    .Map(static pulled => Seq(pulled))));

    public static IO<Seq<(Curve Curve, int CurveIndex, int BrepIndex)>> ProjectToBreps(Seq<Curve> curves, Seq<Brep> breps, Vector3d direction, double tolerance, bool loose) =>
        from valid in IO.lift(() =>
            from filled in Invalid.Unless(!curves.IsEmpty, nameof(curves))
            from targets in Invalid.Unless(!breps.IsEmpty, nameof(breps))
            select unit)
        from rows in GeometryResults.Acquire(
            () => (Curve.ProjectToBrep(curves, breps, direction, tolerance, loose, out int[] curveIndices, out int[] brepIndices), toSeq(curveIndices).Zip(toSeq(brepIndices)).ToArray()),
            nameof(Curve.ProjectToBrep))
        select rows.Map(static row => (Curve: row.Result, CurveIndex: row.Row.First, BrepIndex: row.Row.Second));

    public static IO<Seq<Curve>> ProjectToMeshes(Seq<Curve> curves, Seq<Mesh> meshes, Vector3d direction, double tolerance, bool loose) =>
        from valid in IO.lift(() =>
            from filled in Invalid.Unless(!curves.IsEmpty, nameof(curves))
            from targets in Invalid.Unless(!meshes.IsEmpty, nameof(meshes))
            select unit)
        from projected in GeometryResults.Acquire(() => Curve.ProjectToMesh(curves, meshes, direction, tolerance, loose), nameof(Curve.ProjectToMesh))
        select projected;

    public static IO<Seq<Curve>> ProjectToPlane(Seq<Curve> curves, Plane plane) =>
        from planar in IO.lift(() => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
        from answers in IO.lift(() => curves.Map(curve => Optional(Curve.ProjectToPlane(curve, plane))).Strict())
        from projected in GeometryResults.Complete(answers, Disposal.Release, nameof(Curve.ProjectToPlane))
        select projected;
}

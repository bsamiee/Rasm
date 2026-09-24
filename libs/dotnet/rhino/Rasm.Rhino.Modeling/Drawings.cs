using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionFrame {
    public sealed record FromViewport(RhinoViewport Viewport, BoundingBox Bounds) : ProjectionFrame;

    public sealed record FromPose(CameraPose Pose, BoundingBox Bounds) : ProjectionFrame;

    public sealed record Look(Plane Frame, BoundingBox Bounds) : ProjectionFrame;

    public sealed record Directed(Vector3d Direction, Vector3d Up, BoundingBox Bounds) : ProjectionFrame;
}

public sealed record DrawnSegment(
    Curve Curve,
    HiddenLineDrawingSegment.Visibility Visibility,
    bool IsSceneSilhouette,
    Option<object> Tag,
    SilhouetteType SilhouetteType,
    Option<int> ClippingPlane,
    Seq<HiddenLineDrawingSegment.SideFill> SideFills);

public sealed record DrawnPoint(Point3d Location, HiddenLineDrawingPoint.Visibility Visibility, Option<object> Tag);

public sealed record DrawingResult(Seq<DrawnSegment> Segments, Seq<DrawnPoint> Points);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SilhouetteFrame {
    public sealed record Eye(Point3d Location) : SilhouetteFrame;

    public sealed record Along(Vector3d Direction) : SilhouetteFrame;

    public sealed record Framed(ProjectionFrame Frame) : SilhouetteFrame;
}

public sealed record SilhouetteCurve(Curve Curve, SilhouetteType Kind, ComponentIndex Component);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Drawings {
    // --- [HIDDEN_LINE]
    public static IO<DrawingResult> Draw(HiddenLineDrawingParameters parameters, ProjectionFrame frame, bool multipleThreads, bool rejoin, Option<IProgress<double>> progress, CancellationToken cancel) =>
        Disposal.Using(Viewport(frame), viewport =>
            from viewed in IO.lift(() => parameters.SetViewport(viewport))
            from result in Disposal.Using(
                IO.lift(() => Missing.Unless(HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), cancel), nameof(HiddenLineDrawing.Compute)))
                    .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<HiddenLineDrawing>(new Invalid(nameof(HiddenLineDrawingParameters)))),
                drawing =>
                    from joined in when(rejoin, IO.lift(drawing.RejoinCompatibleVisible)).As()
                    let points = toSeq(drawing.Points)
                    from segments in IO.lift(toSeq(drawing.Segments).TraverseM(static segment =>
                            from parent in Missing.Unless(segment.ParentCurve, nameof(HiddenLineDrawingSegment.ParentCurve))
                            from tag in Tag(parent.SourceObject)
                            select (Segment: segment, Parent: parent, Tag: tag))
                        .As())
                    from tags in IO.lift(points.TraverseM(static point => Tag(point.SourceObject)).As())
                    select new DrawingResult(
                        segments.Map(static row => new DrawnSegment(
                            row.Segment.CurveGeometry.DuplicateCurve(),
                            row.Segment.SegmentVisibility,
                            row.Segment.IsSceneSilhouette,
                            row.Tag,
                            row.Parent.SilhouetteType,
                            row.Parent.SilhouetteType == SilhouetteType.SectionCut ? Some(row.Parent.ClippingPlaneIndex) : None,
                            toSeq(row.Segment.CurveSideFills))).Strict(),
                        points.Zip(tags, static (point, tag) => new DrawnPoint(point.Location, point.PointVisibility, tag)).Strict()))
            select result);

    private static Fin<Option<object>> Tag(HiddenLineDrawingObject? source) =>
        Missing.Unless(source, nameof(HiddenLineDrawingObjectCurve.SourceObject)).Map(static drawn => Optional(drawn.Tag));

    private static IO<ViewportInfo> Viewport(ProjectionFrame frame) =>
        frame.Switch(
            fromViewport: static of =>
                from bounded in IO.lift(() => Invalid.Unless(of.Bounds.IsValid, nameof(BoundingBox.IsValid)))
                from viewport in IO.lift(() => new ViewportInfo(of.Viewport))
                from clipped in GeometryOps.OnFailure(IO.lift(() => Refused.Unless(viewport.SetFrustumNearFar(of.Bounds), nameof(ViewportInfo.SetFrustumNearFar))), IO.lift(viewport.Dispose))
                select viewport,
            fromPose: static of => Cameras.ToViewportInfo(of.Pose, of.Bounds),
            look: static of => Parallel(of.Frame.Origin, -of.Frame.ZAxis, of.Frame.YAxis, of.Bounds),
            directed: static of => Parallel(of.Bounds.Center - (of.Direction / of.Direction.Length * of.Bounds.Diagonal.Length), of.Direction, of.Up, of.Bounds));

    private static IO<ViewportInfo> Parallel(Point3d location, Vector3d direction, Vector3d up, BoundingBox bounds) =>
        from bounded in IO.lift(() => Invalid.Unless(bounds.IsValid, nameof(BoundingBox.IsValid)))
        from viewport in IO.lift(static () => new ViewportInfo())
        from placed in GeometryOps.OnFailure(
            IO.lift(() =>
                from located in Refused.Unless(viewport.SetCameraLocation(location), nameof(ViewportInfo.SetCameraLocation))
                from aimed in Refused.Unless(viewport.SetCameraDirection(direction), nameof(ViewportInfo.SetCameraDirection))
                from upright in Refused.Unless(viewport.SetCameraUp(up), nameof(ViewportInfo.SetCameraUp))
                from projected in Refused.Unless(viewport.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection))
                from clipped in Refused.Unless(viewport.SetFrustumNearFar(bounds), nameof(ViewportInfo.SetFrustumNearFar))
                select viewport),
            IO.lift(viewport.Dispose))
        select placed;

    // --- [SILHOUETTES]
    public static IO<Seq<SilhouetteCurve>> Outline(GeometryBase subject, SilhouetteType kinds, SilhouetteFrame frame, Seq<Plane> clips, double tolerance, double angleTolerance, CancellationToken cancel) =>
        from kinded in IO.lift(() => Invalid.Unless(kinds != SilhouetteType.None, nameof(kinds)))
        from silhouettes in frame.Switch(
            (Subject: subject, Kinds: kinds, Clips: clips, Tolerance: tolerance, AngleTolerance: angleTolerance, Cancel: cancel),
            eye: static (state, of) => IO.lift(() => Silhouette.Compute(state.Subject, state.Kinds, of.Location, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel)),
            along: static (state, of) =>
                from aimed in IO.lift(() => Degenerate.Unless(of.Direction.IsValid && !of.Direction.IsTiny(), nameof(of.Direction)))
                from computed in IO.lift(() => Silhouette.Compute(state.Subject, state.Kinds, of.Direction, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel))
                select computed,
            framed: static (state, of) =>
                Disposal.Using(Viewport(of.Frame), viewport => IO.lift(() => Silhouette.Compute(state.Subject, state.Kinds, viewport, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel))))
        from outlined in IO.lift(() => Silhouettes(silhouettes, nameof(Silhouette.Compute)))
        select outlined;

    public static IO<Seq<SilhouetteCurve>> Draft(GeometryBase subject, double draftAngle, Vector3d pullDirection, double tolerance, double angleTolerance, CancellationToken cancel) =>
        from aimed in IO.lift(() => Degenerate.Unless(pullDirection.IsValid && !pullDirection.IsTiny(), nameof(pullDirection)))
        from silhouettes in IO.lift(() => Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerance, angleTolerance, cancel))
        from outlined in IO.lift(() => Silhouettes(silhouettes, nameof(Silhouette.ComputeDraftCurve)))
        select outlined;

    private static Fin<Seq<SilhouetteCurve>> Silhouettes(Silhouette[]? silhouettes, string member) =>
        from answered in Missing.Unless(silhouettes, member)
        let kept = toSeq(answered).Choose(static row => Optional(row.Curve).Map(curve => new SilhouetteCurve(curve, row.SilhouetteType, row.GeometryComponentIndex))).Strict()
        from present in Answers.NonEmpty(kept, member)
        select present;
}

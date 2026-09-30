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
        from viewed in DisposalOps.Using(Viewport(frame), viewport => IO.lift(() => parameters.SetViewport(viewport)))
        from result in DisposalOps.Using(
            IO.lift(() => Missing.Unless(HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), cancel), nameof(HiddenLineDrawing.Compute))
                    .BindFail(missing => cancel.IsCancellationRequested ? new Canceled() : missing))
                .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<HiddenLineDrawing>(new Invalid(nameof(HiddenLineDrawingParameters.AddGeometry)))),
            drawing =>
                from joined in when(rejoin, IO.lift(drawing.RejoinCompatibleVisible)).As()
                from drawn in IO.lift(() =>
                    (toSeq(drawing.Segments).Traverse(static segment =>
                                from parent in Missing.Unless(segment.ParentCurve, nameof(HiddenLineDrawingSegment.ParentCurve))
                                from tag in Tag(parent.SourceObject, nameof(HiddenLineDrawingObjectCurve.SourceObject))
                                select (Segment: segment, Parent: parent, Tag: tag))
                            .As(),
                        toSeq(drawing.Points).Traverse(static point => from tag in Tag(point.SourceObject, nameof(HiddenLineDrawingPoint.SourceObject)) select new DrawnPoint(point.Location, point.PointVisibility, tag))
                            .As())
                    .Apply(static (segments, points) => new DrawingResult(
                        segments.Map(static row => new DrawnSegment(
                                row.Segment.CurveGeometry.DuplicateCurve(),
                                row.Segment.SegmentVisibility,
                                row.Segment.IsSceneSilhouette,
                                row.Tag,
                                row.Parent.SilhouetteType,
                                Answers.Found(row.Parent.SilhouetteType == SilhouetteType.SectionCut, row.Parent.ClippingPlaneIndex),
                                toSeq(row.Segment.CurveSideFills)))
                            .Strict(),
                        points))
                    .As())
                select drawn)
        select result;

    private static Fin<Option<object>> Tag(HiddenLineDrawingObject? source, string member) =>
        Missing.Unless(source, member).Map(static drawn => Optional(drawn.Tag));

    private static IO<ViewportInfo> Viewport(ProjectionFrame frame) =>
        frame.Switch(
            fromViewport: static of =>
                from viewport in IO.lift(() => new ViewportInfo(of.Viewport))
                from clipped in DisposalOps.OnFailure(IO.lift(() => Refused.Unless(viewport.SetFrustumNearFar(of.Bounds), nameof(ViewportInfo.SetFrustumNearFar))), IO.lift(viewport.Dispose))
                select viewport,
            fromPose: static of => Cameras.ToViewportInfo(of.Pose, of.Bounds));

    // --- [SILHOUETTES]
    public static IO<Seq<SilhouetteCurve>> Outline(GeometryBase subject, SilhouetteType kinds, SilhouetteFrame frame, Seq<Plane> clips, double tolerance, double angleTolerance, CancellationToken cancel) =>
        frame.Switch(
                (Subject: subject, Kinds: kinds, Clips: clips, Tolerance: tolerance, AngleTolerance: angleTolerance, Cancel: cancel),
                eye: static (state, of) => IO.lift(() => Silhouette.Compute(state.Subject, state.Kinds, of.Location, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel)),
                along: static (state, of) => IO.lift(() =>
                    Degenerate.Unless(of.Direction, nameof(of.Direction)).Map(_ => Silhouette.Compute(state.Subject, state.Kinds, of.Direction, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel))),
                framed: static (state, of) =>
                    DisposalOps.Using(Viewport(of.Frame), viewport => IO.lift(() => Silhouette.Compute(state.Subject, state.Kinds, viewport, state.Tolerance, state.AngleTolerance, state.Clips, state.Cancel))))
            .Map(Silhouettes);

    public static IO<Seq<SilhouetteCurve>> Draft(GeometryBase subject, double draftAngle, Vector3d pullDirection, double tolerance, double angleTolerance, CancellationToken cancel) =>
        IO.lift(() => Degenerate.Unless(pullDirection, nameof(pullDirection)).Map(_ => Silhouettes(Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerance, angleTolerance, cancel))));

    private static Seq<SilhouetteCurve> Silhouettes(Silhouette[] silhouettes) =>
        toSeq(silhouettes).Choose(static row => Optional(row.Curve).Map(curve => new SilhouetteCurve(curve, row.SilhouetteType, row.GeometryComponentIndex))).Strict();
}

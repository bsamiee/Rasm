using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union<RhinoViewport, CameraPose>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ProjectionFrame;

public sealed record DrawnCurve(SilhouetteType SilhouetteType, Option<int> ClippingPlaneIndex, ComponentIndex SourceObjectComponentIndex, Option<object> Tag, Option<double> OriginalDomainStart);

public sealed record DrawnSegment(
    Curve Curve,
    HiddenLineDrawingSegment.Visibility SegmentVisibility,
    bool IsSceneSilhouette,
    Option<(HiddenLineDrawingSegment.SideFill Left, HiddenLineDrawingSegment.SideFill Right)> CurveSideFills,
    Option<DrawnCurve> ParentCurve);

public sealed record DrawnPoint(Point3d Location, HiddenLineDrawingPoint.Visibility PointVisibility, ComponentIndex SourceObjectComponentIndex, Option<object> Tag);

public sealed record LineDrawing(Seq<DrawnSegment> Segments, Seq<DrawnPoint> Points, Transform WorldToHiddenLine);

[Union<Point3d, Vector3d, ViewportInfo>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SilhouetteFrame {
    public static SilhouetteFrame Of(CameraPose pose) =>
        pose.Lens is CameraLens.Parallel ? new(-pose.Frame.ZAxis) : new(pose.Frame.Origin);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Drawings {
    // --- [PROJECTION]
    public static Seq<Plane> Clips(ClippingPlaneSurface surface) =>
        new Plane(surface.Plane.Origin, surface.Plane.YAxis, surface.Plane.XAxis) switch {
            var cut => surface.PlaneDepthEnabled
                ? Seq(cut, new Plane(cut.Origin - (surface.PlaneDepth * cut.Normal), cut.YAxis, cut.XAxis))
                : Seq(cut),
        };

    // --- [HIDDEN_LINE]
    public static IO<LineDrawing> Draw(
        Tolerances tolerances, ProjectionFrame frame, BoundingBox bounds, Func<HiddenLineDrawingParameters, Fin<Unit>> subjects,
        bool multipleThreads, bool rejoin, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        DisposalOps.Acquire(outputs =>
            from parameters in IO.lift(() => new HiddenLineDrawingParameters { AbsoluteTolerance = tolerances.Absolute })
            from configured in IO.lift(() => subjects(parameters))
            from projected in DisposalOps.Within<IDisposable, Unit>(temporary => Projected(frame, bounds, parameters, temporary), DisposalOps.Release)
            from drawn in DisposalOps.Within<IDisposable, LineDrawing>(
                temporary => Copies.Acquire(nameof(HiddenLineDrawing.Compute), token => Answer.One(HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), token)), temporary)
                    .Bind(drawing => Drawn(drawing, rejoin, outputs)),
                DisposalOps.Release)
            select drawn,
            retain);

    private static IO<Unit> Projected(ProjectionFrame frame, BoundingBox bounds, HiddenLineDrawingParameters parameters, Action<IDisposable> temporary) =>
        from camera in frame.Switch(
            temporary,
            rhinoViewport: static (retain, live) => DisposalOps.Acquire(IO.lift(() => new ViewportInfo(live)), retain).OnUiThread(),
            cameraPose: static (retain, pose) => DisposalOps.Acquire(IO.lift(static () => new ViewportInfo()), retain).Bind(made => IO.lift(() => Cameras.Pose(made, pose))))
        from bounded in IO.lift(() => Refused.Unless(camera.SetFrustumNearFar(bounds), nameof(ViewportInfo.SetFrustumNearFar)))
        from viewed in IO.lift(() => parameters.SetViewport(camera))
        select viewed;

    private static IO<LineDrawing> Drawn(HiddenLineDrawing drawing, bool rejoin, Action<IDisposable> retain) =>
        from rejoined in when(rejoin, IO.lift(drawing.RejoinCompatibleVisible)).As()
        let points = Conversions.Rows(drawing.Points).Map(ToDrawnPoint).Strict()
        let worldToHiddenLine = drawing.WorldToHiddenLine
        from segments in Conversions.Rows(drawing.Segments).TraverseM(segment => Segment(segment, retain)).As()
        select new LineDrawing(segments, points, worldToHiddenLine);

    private static IO<DrawnSegment> Segment(HiddenLineDrawingSegment segment, Action<IDisposable> retain) =>
        from metadata in IO.lift(() => (
            segment.SegmentVisibility,
            segment.IsSceneSilhouette,
            Fills: segment.CurveSideFills switch {
                [var left, var right] => Some((Left: left, Right: right)),
                _ => None,
            },
            Parent: Optional(segment.ParentCurve).Map(ToDrawnCurve)))
        from curve in Copies.Duplicate(segment.CurveGeometry, retain)
        select new DrawnSegment(curve, metadata.SegmentVisibility, metadata.IsSceneSilhouette, metadata.Fills, metadata.Parent);

    [MapProperty(nameof(HiddenLineDrawingObjectCurve.SourceObject), nameof(DrawnCurve.Tag), Use = nameof(Tag))]
    [MapPropertyFromSource(nameof(DrawnCurve.ClippingPlaneIndex), Use = nameof(SectionCutIndex))]
    private static partial DrawnCurve ToDrawnCurve(HiddenLineDrawingObjectCurve parent);

    [MapProperty(nameof(HiddenLineDrawingPoint.SourceObject), nameof(DrawnPoint.Tag), Use = nameof(Tag))]
    private static partial DrawnPoint ToDrawnPoint(HiddenLineDrawingPoint point);

    private static Option<int> SectionCutIndex(HiddenLineDrawingObjectCurve parent) =>
        Callbacks.Found(parent.SilhouetteType is SilhouetteType.SectionCut, parent.ClippingPlaneIndex);

    private static Option<object> Tag(HiddenLineDrawingObject? source) =>
        Optional(source).Bind(static found => Optional(found.Tag));

    // --- [SILHOUETTES]
    public static IO<Seq<Silhouette>> Silhouettes(Tolerances tolerances, GeometryBase subject, SilhouetteType silhouetteType, SilhouetteFrame frame, Seq<Plane> clips, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Silhouette.Compute), token => Answer.Rows(frame.Switch(
            (Subject: subject, Type: silhouetteType, Clips: clips, Tolerances: tolerances, Token: token),
            point3d: static (state, eye) => Silhouette.Compute(state.Subject, state.Type, eye, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
            vector3d: static (state, direction) => Silhouette.Compute(state.Subject, state.Type, direction, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
            viewportInfo: static (state, viewport) => Silhouette.Compute(state.Subject, state.Type, viewport, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token)),
            static row => Optional<IDisposable>(row.Curve)), retain);

    public static IO<Seq<Silhouette>> DraftCurves(Tolerances tolerances, GeometryBase subject, double draftAngle, Vector3d pullDirection, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Silhouette.ComputeDraftCurve), token => Answer.Rows(
            Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerances.Absolute, tolerances.Angle, token),
            static row => Optional<IDisposable>(row.Curve)), retain);
}

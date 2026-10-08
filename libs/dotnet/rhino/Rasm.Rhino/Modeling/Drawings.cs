using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ProjectionFrame {
    public sealed record FromViewport(RhinoViewport Viewport) : ProjectionFrame;

    public sealed record FromPose(CameraPose Pose) : ProjectionFrame;
}

public sealed record DrawnCurve(SilhouetteType SilhouetteType, Option<int> ClippingPlaneIndex, ComponentIndex SourceObjectComponentIndex, Option<object> Tag);

public sealed record DrawnSegment(
    Curve Curve,
    HiddenLineDrawingSegment.Visibility SegmentVisibility,
    bool IsSceneSilhouette,
    Option<(HiddenLineDrawingSegment.SideFill Left, HiddenLineDrawingSegment.SideFill Right)> CurveSideFills,
    Option<DrawnCurve> ParentCurve);

public sealed record DrawnPoint(Point3d Location, HiddenLineDrawingPoint.Visibility PointVisibility, ComponentIndex SourceObjectComponentIndex, Option<object> Tag);

public sealed record LineDrawing(Seq<DrawnSegment> Segments, Seq<DrawnPoint> Points, Transform WorldToHiddenLine);

[Union]
public abstract partial record SilhouetteFrame {
    public static SilhouetteFrame Of(CameraPose pose) =>
        pose.Lens.Switch<CameraPose, SilhouetteFrame>(
            pose,
            parallel: static (posed, _) => new Along(-posed.Frame.ZAxis),
            perspective: static (posed, _) => new Eye(posed.Frame.Origin),
            twoPoint: static (posed, _) => new Eye(posed.Frame.Origin));

    public sealed record Eye(Point3d Location) : SilhouetteFrame;

    public sealed record Along(Vector3d Direction) : SilhouetteFrame;
}

public sealed record SilhouetteCurve(Curve Curve, SilhouetteType SilhouetteType, ComponentIndex GeometryComponentIndex);

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

    private static IO<ViewportInfo> Projection(ProjectionFrame frame, BoundingBox bounds) =>
        frame.Switch(
            bounds,
            fromViewport: static (box, of) => IO.lift(() => new ViewportInfo(of.Viewport)).Post().Bind(viewport => DisposalOps.OnFailure(
                IO.lift(() => Refused.Unless(viewport.SetFrustumNearFar(box), viewport, nameof(ViewportInfo.SetFrustumNearFar))),
                IO.lift(viewport.Dispose))),
            fromPose: static (box, of) => Cameras.ToViewportInfo(of.Pose, box));

    // --- [HIDDEN_LINE]
    public static IO<LineDrawing> Draw(
        ProjectionFrame frame, BoundingBox bounds, Seq<Plane> clips, Tolerances tolerances, Func<HiddenLineDrawingParameters, Fin<Unit>> subjects,
        bool multipleThreads, bool rejoin, Option<IProgress<double>> progress) =>
        from parameters in IO.lift(() =>
            Callbacks.Unique(clips, identity, nameof(HiddenLineDrawingParameters.AddClippingPlane)).ToFin()
                .Bind(unique => Parameters(unique, tolerances, subjects)))
        from _ in use(Projection(frame, bounds)).Bind(viewport => IO.lift(() => parameters.SetViewport(viewport))).Bracket()
        from drawn in use(Computed(parameters, multipleThreads, progress)).Bind(drawing => Drawn(drawing, rejoin)).Bracket()
        select drawn;

    private static Fin<HiddenLineDrawingParameters> Parameters(Seq<Plane> clips, Tolerances tolerances, Func<HiddenLineDrawingParameters, Fin<Unit>> subjects) {
        HiddenLineDrawingParameters parameters = new() { AbsoluteTolerance = tolerances.Absolute };
        _ = clips.Iter(parameters.AddClippingPlane);
        return subjects(parameters).Map(_ => parameters);
    }

    private static IO<HiddenLineDrawing> Computed(HiddenLineDrawingParameters parameters, bool multipleThreads, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from drawing in IO.lift(() =>
            Callbacks.Thrown<ArgumentException, HiddenLineDrawing?>(
                    () => HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), token),
                    nameof(HiddenLineDrawing.Compute))
                .Bind(computed => Optional(computed).ToFin(token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(HiddenLineDrawing.Compute)))))
        select drawing;

    private static IO<LineDrawing> Drawn(HiddenLineDrawing drawing, bool rejoin) =>
        from rejoined in when(rejoin, IO.lift(drawing.RejoinCompatibleVisible)).As()
        from segments in DisposalOps.AcquireAll(
            Conversions.Rows(drawing.Segments).Map(Segment),
            static drawn => DisposalOps.Release(drawn.Map(static segment => segment.Curve)))
        select new LineDrawing(
            segments,
            Conversions.Rows(drawing.Points).Map(ToDrawnPoint).Strict(),
            drawing.WorldToHiddenLine);

    private static IO<DrawnSegment> Segment(HiddenLineDrawingSegment segment) =>
        Copies.Duplicate(segment.CurveGeometry).Map(curve => new DrawnSegment(
            curve,
            segment.SegmentVisibility,
            segment.IsSceneSilhouette,
            segment.CurveSideFills switch {
                [var left, var right] => Some((Left: left, Right: right)),
                _ => None,
            },
            Optional(segment.ParentCurve).Map(ToDrawnCurve)));

    [MapProperty(nameof(HiddenLineDrawingObjectCurve.SourceObject), nameof(DrawnCurve.Tag), Use = nameof(Tag))]
    [MapPropertyFromSource(nameof(DrawnCurve.ClippingPlaneIndex), Use = nameof(SectionCutIndex))]
    private static partial DrawnCurve ToDrawnCurve(HiddenLineDrawingObjectCurve parent);

    [MapProperty(nameof(HiddenLineDrawingPoint.SourceObject), nameof(DrawnPoint.Tag), Use = nameof(Tag))]
    private static partial DrawnPoint ToDrawnPoint(HiddenLineDrawingPoint point);

    private static Option<int> SectionCutIndex(HiddenLineDrawingObjectCurve parent) =>
        Callbacks.Found(parent.SilhouetteType == SilhouetteType.SectionCut, parent.ClippingPlaneIndex);

    private static Option<object> Tag(HiddenLineDrawingObject? source) =>
        Optional(source).Bind(static found => Optional(found.Tag));

    // --- [SILHOUETTES]
    public static IO<Seq<SilhouetteCurve>> Silhouettes(GeometryBase subject, SilhouetteType silhouetteType, SilhouetteFrame frame, Seq<Plane> clips, Tolerances tolerances) =>
        Acquired(
            token => frame.Switch(
                (Subject: subject, Type: silhouetteType, Clips: clips, Tolerances: tolerances, Token: token),
                eye: static (state, eye) => Silhouette.Compute(state.Subject, state.Type, eye.Location, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
                along: static (state, along) => Silhouette.Compute(state.Subject, state.Type, along.Direction, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token)),
            nameof(Silhouette.Compute));

    public static IO<Seq<SilhouetteCurve>> DraftCurves(GeometryBase subject, double draftAngle, Vector3d pullDirection, Tolerances tolerances) =>
        Acquired(
            token => Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerances.Absolute, tolerances.Angle, token),
            nameof(Silhouette.ComputeDraftCurve));

    private static IO<Seq<SilhouetteCurve>> Acquired(Func<CancellationToken, Silhouette[]> compute, string member) =>
        from token in cancelToken
        from pairs in Copies.Acquire<Curve, Silhouette>(
            () => compute(token) switch {
                var rows => (Array.ConvertAll(rows, static row => row.Curve), rows),
            },
            member)
        select pairs.Map(static pair => new SilhouetteCurve(pair.Result, pair.Row.SilhouetteType, pair.Row.GeometryComponentIndex)).Strict();
}

using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Viewport;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union<RhinoViewport, CameraPose>(MapMethods = SwitchMapMethodsGeneration.None)]
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

[Union<Point3d, Vector3d, ViewportInfo>(MapMethods = SwitchMapMethodsGeneration.None)]
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
        ProjectionFrame frame, BoundingBox bounds, Seq<Plane> clips, Tolerances tolerances, Func<HiddenLineDrawingParameters, Fin<Unit>> subjects,
        bool multipleThreads, bool rejoin, Option<IProgress<double>> progress) =>
        from unique in IO.lift(() => Callbacks.Unique(clips, identity, nameof(HiddenLineDrawingParameters.AddClippingPlane)).ToFin())
        let parameters = Parameters(tolerances.Absolute)
        from configured in IO.lift(() => {
            _ = unique.Iter(parameters.AddClippingPlane);
            return subjects(parameters);
        })
        from projected in (
            from viewport in use(frame.Switch(
                bounds,
                rhinoViewport: static (box, live) => Copies.Owned(
                    IO.lift(() => new ViewportInfo(live)).Post(),
                    camera => Refused.Unless(camera.SetFrustumNearFar(box), camera, nameof(ViewportInfo.SetFrustumNearFar))),
                cameraPose: static (box, pose) => Cameras.ToViewportInfo(pose, box)))
            from assigned in IO.lift(() => parameters.SetViewport(viewport))
            select assigned).Bracket()
        from drawn in (
            from drawing in use(Computed(parameters, multipleThreads, progress))
            from result in Drawn(drawing, rejoin)
            select result).Bracket()
        select drawn;

    [MapPropertyFromSource(nameof(HiddenLineDrawingParameters.AbsoluteTolerance))]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial HiddenLineDrawingParameters Parameters(double tolerance);

    private static IO<HiddenLineDrawing> Computed(HiddenLineDrawingParameters parameters, bool multipleThreads, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from computed in IO.lift(() =>
            Callbacks.Thrown<ArgumentException, HiddenLineDrawing?>(
                () => HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), token),
                nameof(HiddenLineDrawing.Compute)))
        from drawing in IO.lift(() => Optional(computed).ToFin(token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(HiddenLineDrawing.Compute))))
        select drawing;

    private static IO<LineDrawing> Drawn(HiddenLineDrawing drawing, bool rejoin) =>
        from rejoined in when(rejoin, IO.lift(drawing.RejoinCompatibleVisible)).As()
        let points = Conversions.Rows(drawing.Points).Map(ToDrawnPoint).Strict()
        let worldToHiddenLine = drawing.WorldToHiddenLine
        from segments in DisposalOps.AcquireAll(
            Conversions.Rows(drawing.Segments).Map(Segment),
            static drawn => DisposalOps.Release(drawn.Map(static segment => segment.Curve)))
        select new LineDrawing(segments, points, worldToHiddenLine);

    private static IO<DrawnSegment> Segment(HiddenLineDrawingSegment segment) =>
        from metadata in IO.lift(() => (
            segment.SegmentVisibility,
            segment.IsSceneSilhouette,
            Fills: segment.CurveSideFills switch {
                [var left, var right] => Some((Left: left, Right: right)),
                _ => None,
            },
            Parent: Optional(segment.ParentCurve).Map(ToDrawnCurve)))
        from curve in Copies.Duplicate(segment.CurveGeometry)
        select new DrawnSegment(curve, metadata.SegmentVisibility, metadata.IsSceneSilhouette, metadata.Fills, metadata.Parent);

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
    public static IO<Seq<Silhouette>> Silhouettes(GeometryBase subject, SilhouetteType silhouetteType, SilhouetteFrame frame, Seq<Plane> clips, Tolerances tolerances) =>
        Acquired(
            token => frame.Switch(
                (Subject: subject, Type: silhouetteType, Clips: clips, Tolerances: tolerances, Token: token),
                point3d: static (state, eye) => Silhouette.Compute(state.Subject, state.Type, eye, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
                vector3d: static (state, direction) => Silhouette.Compute(state.Subject, state.Type, direction, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
                viewportInfo: static (state, viewport) => Silhouette.Compute(state.Subject, state.Type, viewport, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token)),
            nameof(Silhouette.Compute));

    public static IO<Seq<Silhouette>> DraftCurves(GeometryBase subject, double draftAngle, Vector3d pullDirection, Tolerances tolerances) =>
        Acquired(
            token => Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerances.Absolute, tolerances.Angle, token),
            nameof(Silhouette.ComputeDraftCurve));

    private static IO<Seq<Silhouette>> Acquired(Func<CancellationToken, Silhouette[]> compute, string member) =>
        from token in cancelToken
        from pairs in Copies.Acquire(
            () => compute(token) switch {
                var rows => ((Curve?[]?)[.. rows.Select(static row => row.Curve)], rows),
            },
            member)
        select pairs.Map(static pair => pair.Row).Strict();
}

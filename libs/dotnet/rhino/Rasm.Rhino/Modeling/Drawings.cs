using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
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
        ProjectionFrame frame, BoundingBox bounds, Seq<Plane> clips, Tolerances tolerances, Func<HiddenLineDrawingParameters, Fin<Unit>> subjects,
        bool multipleThreads, bool rejoin, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        DisposalOps.Acquire(outputs =>
        from unique in IO.lift(() => Callbacks.Unique(clips, identity, nameof(HiddenLineDrawingParameters.AddClippingPlane)).ToFin())
        let parameters = Parameters(tolerances.Absolute)
        from configured in IO.lift(() => {
            _ = unique.Iter(parameters.AddClippingPlane);
            return subjects(parameters);
        })
        from projected in DisposalOps.Within<IDisposable, Unit>(
            temporary => frame.Switch(
                (Bounds: bounds, Retain: temporary),
                rhinoViewport: static (state, live) => DisposalOps.Acquire(
                        IO.lift(() => new ViewportInfo(live)), static camera => IO.lift(camera.Dispose), state.Retain)
                    .Post()
                    .Bind(camera => IO.lift(() => Refused.Unless(camera.SetFrustumNearFar(state.Bounds), camera, nameof(ViewportInfo.SetFrustumNearFar)))),
                cameraPose: static (state, pose) => Cameras.ToViewportInfo(pose, state.Bounds, state.Retain))
                .Bind(viewport => IO.lift(() => parameters.SetViewport(viewport))),
            DisposalOps.Release)
        from drawn in DisposalOps.Within<IDisposable, LineDrawing>(
            temporary => Computed(parameters, multipleThreads, progress, temporary).Bind(drawing => Drawn(drawing, rejoin, outputs)),
            DisposalOps.Release)
        select drawn,
        (_, lease) => retain(lease));

    [MapPropertyFromSource(nameof(HiddenLineDrawingParameters.AbsoluteTolerance))]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial HiddenLineDrawingParameters Parameters(double tolerance);

    private static IO<HiddenLineDrawing> Computed(HiddenLineDrawingParameters parameters, bool multipleThreads, Option<IProgress<double>> progress, Action<IDisposable> retain) =>
        from token in cancelToken
        from drawing in Copies.Owned(
            () => Callbacks.Thrown<ArgumentException, HiddenLineDrawing?>(
                () => HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), token),
                nameof(HiddenLineDrawing.Compute)),
            answer => answer.Bind(computed => Optional(computed).ToFin(token.IsCancellationRequested ? Errors.Cancelled : new Missing(nameof(HiddenLineDrawing.Compute)))),
            static answer => answer.Match(Succ: static drawing => DisposalOps.Release(Conversions.Rows(new[] { drawing })), Fail: static _ => IO.pure(unit)), retain)
        select drawing;

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
        Callbacks.Found(parent.SilhouetteType == SilhouetteType.SectionCut, parent.ClippingPlaneIndex);

    private static Option<object> Tag(HiddenLineDrawingObject? source) =>
        Optional(source).Bind(static found => Optional(found.Tag));

    // --- [SILHOUETTES]
    public static IO<Seq<Silhouette>> Silhouettes(GeometryBase subject, SilhouetteType silhouetteType, SilhouetteFrame frame, Seq<Plane> clips, Tolerances tolerances, Action<IDisposable> retain) =>
        Acquired(
            token => frame.Switch(
                (Subject: subject, Type: silhouetteType, Clips: clips, Tolerances: tolerances, Token: token),
                point3d: static (state, eye) => Silhouette.Compute(state.Subject, state.Type, eye, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
                vector3d: static (state, direction) => Silhouette.Compute(state.Subject, state.Type, direction, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
                viewportInfo: static (state, viewport) => Silhouette.Compute(state.Subject, state.Type, viewport, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token)),
            nameof(Silhouette.Compute), retain);

    public static IO<Seq<Silhouette>> DraftCurves(GeometryBase subject, double draftAngle, Vector3d pullDirection, Tolerances tolerances, Action<IDisposable> retain) =>
        Acquired(
            token => Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerances.Absolute, tolerances.Angle, token),
            nameof(Silhouette.ComputeDraftCurve), retain);

    private static IO<Seq<Silhouette>> Acquired(Func<CancellationToken, Silhouette[]> compute, string member, Action<IDisposable> retain) =>
        from token in cancelToken
        from pairs in Copies.Acquire(
            () => compute(token) switch {
                var rows => ((Curve?[]?)[.. rows.Select(static row => row.Curve)], rows),
            },
            member, retain)
        select pairs.Map(static pair => pair.Row).Strict();
}

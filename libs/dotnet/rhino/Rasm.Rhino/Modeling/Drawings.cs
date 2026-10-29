using Rasm.Rhino.Document.Units;
using Rhino.DocObjects;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DrawingSubject(GeometryBase Geometry, object Tag) {
    public Transform Xform { get; init; } = Transform.Identity;

    public bool OccludingSections { get; init; }

    public Seq<Plane> Clips { get; init; }
}

[Union<Point3d, Vector3d, ViewportInfo>(SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SilhouetteFrame;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Drawings {
    // --- [CLIPS]
    public static Seq<Plane> Clips(ClippingPlaneSurface surface) =>
        new Plane(surface.Plane.Origin, surface.Plane.YAxis, surface.Plane.XAxis) switch {
            var cut => surface.PlaneDepthEnabled
                ? Seq(cut, new Plane(cut.Origin - (surface.PlaneDepth * cut.Normal), cut.YAxis, cut.XAxis))
                : Seq(cut),
        };

    // --- [HIDDEN_LINE]
    public static IO<HiddenLineDrawing> Draw(
        Tolerances tolerances, ViewportInfo viewport, Seq<DrawingSubject> subjects, Action<HiddenLineDrawingParameters> configure, bool multipleThreads, Option<IProgress<double>> progress) =>
        from lowered in subjects.Map(static (subject, index) => (Subject: subject, Index: index)).TraverseM(static row => Lowered(row.Subject.Geometry, row.Index).Map(pieces => (row.Subject, Pieces: pieces))).As()
        from camera in Products.Of(() => new ViewportInfo(viewport))
        from bounded in Products.Accepted(() => camera.SetFrustumNearFar((from row in lowered from piece in row.Pieces select piece.GetBoundingBox(row.Subject.Xform)).Fold(BoundingBox.Empty, BoundingBox.Union)))
        from parameters in IO.lift(() => new HiddenLineDrawingParameters { AbsoluteTolerance = tolerances.Absolute })
        from configured in IO.lift(() => configure(parameters))
        from viewed in IO.lift(() => parameters.SetViewport(camera))
        from added in Products.Each((from row in lowered
                                     from piece in row.Pieces
                                     select Products.Accepted(() => parameters.AddGeometryAndPlanes(piece, row.Subject.Xform, row.Subject.Tag, row.Subject.OccludingSections, [.. row.Subject.Clips]))).Strict())
        from token in cancelToken
        from drawing in Products.Of(() => HiddenLineDrawing.Compute(parameters, multipleThreads, progress.ValueUnsafe(), token))
        select drawing;

    private static IO<Seq<GeometryBase>> Lowered(GeometryBase geometry, int index) =>
        geometry switch {
            SubD subd =>
                from options in use(static () => SubDToBrepOptions.Default)
                from brep in Products.Of(() => subd.ToBrep(options))
                select Seq<GeometryBase>(brep),
            TextDot dot => Products.Of<GeometryBase>(() => new Point(dot.Point)).Map(static point => Seq(point)),
            TextEntity text => Products.Many<GeometryBase>(text.Explode),
            Dimension dimension => Products.Many(dimension.Explode).Bind(static pieces => pieces.TraverseM(static piece => Lowered(piece, 0)).As()).Map(static pieces => pieces.Bind(static held => held)),
            AnnotationBase annotation => IO.fail<Seq<GeometryBase>>(new RhinoFault(annotation.ObjectType.ToString(), RhinoReason.Unsupported, index)),
            _ => IO.pure(Seq(geometry)),
        };

    // --- [SILHOUETTES]
    public static IO<Seq<Silhouette>> Silhouettes(Tolerances tolerances, GeometryBase subject, SilhouetteType silhouetteType, SilhouetteFrame frame, Seq<Plane> clips) =>
        from token in cancelToken
        from silhouettes in Traced(() => frame.Switch(
            (Subject: subject, Type: silhouetteType, Clips: clips, Tolerances: tolerances, Token: token),
            point3d: static (state, eye) => Silhouette.Compute(state.Subject, state.Type, eye, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
            vector3d: static (state, direction) => Silhouette.Compute(state.Subject, state.Type, direction, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token),
            viewportInfo: static (state, viewport) => Silhouette.Compute(state.Subject, state.Type, viewport, state.Tolerances.Absolute, state.Tolerances.Angle, state.Clips, state.Token)))
        select silhouettes;

    public static IO<Seq<Silhouette>> DraftCurves(Tolerances tolerances, GeometryBase subject, double draftAngle, Vector3d pullDirection) =>
        from token in cancelToken
        from silhouettes in Traced(() => Silhouette.ComputeDraftCurve(subject, draftAngle, pullDirection, tolerances.Absolute, tolerances.Angle, token))
        select silhouettes;

    private static IO<Seq<Silhouette>> Traced(Func<Silhouette[]?> compute) =>
        IO.lift(() => Optional(compute()).ToFin(new RhinoFault(nameof(Silhouette), RhinoReason.Missing)))
            .Bind(static rows => Products.Each(toSeq(rows).Map(static silhouette => Products.Maybe(() => silhouette.Curve).Map(_ => silhouette)).Strict()));
}

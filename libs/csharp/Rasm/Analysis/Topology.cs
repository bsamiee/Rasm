namespace Rasm.Analysis;

public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Interval) => Native<TGeometry, TOut, Curve, Interval>(key: DomainKey, project: static curve => One(key: DomainKey, value: curve.Domain).ToEff()),
        (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) => Native<TGeometry, TOut, Surface, Interval>(key: DomainKey, project: static surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1))).Apply((u, v) => u + v).As().ToEff()),
        _ => DomainKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) => Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<Polyline, Line>.Build(key: SegmentsKey, evaluator: static geometry => Many(key: SegmentsKey, values: geometry.GetSegments()).ToEff())),
        (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Native<TGeometry, TOut, Curve, Curve>(key: SegmentsKey, project: static curve => Many(key: SegmentsKey, values: curve.DuplicateSegments()).ToEff()),
        _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<Brep, Curve> Edges => Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static geometry => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()).ToEff());
    public static Query<TGeometry, TOut> EdgeMidpoints<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)]) => Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey,
                evaluator: static geometry => geometry switch {
                    Line line => One(key: EdgeMidpointsKey, value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => Many(key: EdgeMidpointsKey, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => Many(key: EdgeMidpointsKey, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Curve curve => CurveAtNormalized(geometry: curve, key: EdgeMidpointsKey, project: static (geometry, parameter) => geometry.PointAt(t: parameter)),
                    Brep brep => BrepLeaves(brep: brep, key: EdgeMidpointsKey, primitiveFault: static (key, label) => key.PrimitiveNoEdges(primitive: label), project: static (validated, context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context)),
                    Mesh mesh => from context in Analyze.Asks
                                 from validated in context.Validate(geometry: mesh, requirement: Requirement.Basic).ToEff()
                                 from result in Many(key: EdgeMidpointsKey, values: Enumerable.Range(start: 0, count: validated.TopologyEdges.Count).Select(index => validated.TopologyEdges.EdgeLine(topologyEdgeIndex: index).PointAt(t: 0.5))).ToEff()
                                 select result,
                    SubD subd => from context in Analyze.Asks
                                 from validated in context.Validate(geometry: subd, requirement: Requirement.Basic).ToEff()
                                 from result in EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context).ToEff()
                                 select result,
                    Box box => from context in Analyze.Asks
                               from result in Optional(box.ToBrep())
                                   .ToFin(EdgeMidpointsKey.InvalidResult())
                                   .Bind(brep => { using Brep disposable = brep; return EdgeCurveMidpoints(curves: disposable.DuplicateEdgeCurves(), context: context); })
                                   .ToEff()
                               select result,
                    _ => Fin.Fail<Seq<Point3d>>(EdgeMidpointsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                })),
        _ => EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Native<TGeometry, TOut, Brep, Curve>(key: NakedEdgesKey, project: static brep => Many(key: NakedEdgesKey, values: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)).ToEff()),
        (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) => Native<TGeometry, TOut, Mesh, Polyline>(key: NakedEdgesKey, project: static mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()).ToEff()),
        _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(key: OutlinesKey, state: plane, evaluator: static (sectionPlane, geometry) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: sectionPlane)).ToEff());
    internal static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target && Supports(geometry: geometry, native: native);
    internal static bool Supports(Type geometry, params Type[] native) => typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object) || native.Contains(value: geometry);
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) =>
        Query<Surface, Curve>.Build(key: IsoKey, requirement: Requirement.SurfaceEvaluation, state: (Iso: iso, Normalized: normalized), evaluator: static (state, geometry) => IsoCurveValues(surface: geometry, iso: state.Iso, normalized: state.Normalized, key: IsoKey).ToEff());
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull =>
        toSeq(PrimitiveDispatch.Items).Find(dispatch => dispatch.SourceType.IsAssignableFrom(c: typeof(TGeometry)) && dispatch.ResultType == typeof(TOut))
            .Match(
                Some: dispatch => dispatch.Build<TGeometry, TOut>(),
                None: () => PrimitiveKey.Unsupported<TGeometry, TOut>());
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(GeometryKind), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box), typeof(Sphere)]) => Cast<TGeometry, TOut>(key: KindKey, query: Query<TGeometry, GeometryKind>.Build(
                key: KindKey,
                evaluator: static geometry => geometry switch {
                    object value => from context in Analyze.Asks
                                    from result in One(key: KindKey, value: GeometryKinds.Kind(geometry: value, context: context)).ToEff()
                                    select result,
                })),
        _ => KindKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<TGeometry, TOut> SolidOrientation<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(BrepSolidOrientation) => Native<TGeometry, TOut, Brep, BrepSolidOrientation>(key: SolidOrientationKey, project: static brep => One(key: SolidOrientationKey, value: brep.SolidOrientation).ToEff()),
        (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) => Native<TGeometry, TOut, Mesh, int>(key: SolidOrientationKey, project: static mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()).ToEff()),
        _ => SolidOrientationKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase =>
        point.IsValid switch {
            false => Query<TGeometry, bool>.Reject(key: IsPointInsideKey, fault: IsPointInsideKey.InvalidInput()),
            true => typeof(TGeometry) switch {
                Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry) => Cast<TGeometry, bool>(key: IsPointInsideKey, query: Query<TGeometry, bool>.Build(
                        key: IsPointInsideKey, state: point, requirement: Requirement.SolidTopology, evaluator: static (target, geometry) => from context in Analyze.Asks
                                                                                                                                             from result in (geometry switch {
                                                                                                                                                 Brep brep => One(key: IsPointInsideKey, value: brep.IsPointInside(point: target, tolerance: context.Absolute.Value, strictlyIn: false)),
                                                                                                                                                 Mesh mesh => One(key: IsPointInsideKey, value: mesh.IsPointInside(point: target, tolerance: context.Absolute.Value, strictlyIn: false)),
                                                                                                                                                 _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                                                                                                                                             }).ToEff()
                                                                                                                                             select result)),
                _ => IsPointInsideKey.Unsupported<TGeometry, bool>(),
            },
        };
    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(Point3d), typeof(BoundingBox), typeof(Box)]) => Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                key: VerticesKey,
                requiresContext: true,
                evaluator: static geometry => geometry switch {
                    Point3d point => One(key: VerticesKey, value: point).ToEff(),
                    Point point => One(key: VerticesKey, value: point.Location).ToEff(),
                    Line line => Many(key: VerticesKey, values: new[] { line.From, line.To }).ToEff(),
                    Polyline polyline => Many(key: VerticesKey, values: polyline).ToEff(),
                    Curve curve => (curve.TryGetPolyline(polyline: out Polyline polyline) switch {
                        true => Many(key: VerticesKey, values: polyline),
                        false => Many(key: VerticesKey, values: new[] { curve.PointAtStart, curve.PointAtEnd }),
                    }).ToEff(),
                    Brep brep => BrepLeaves(
                        brep: brep, key: VerticesKey, primitiveFault: static (key, label) => key.PrimitiveNoVertices(primitive: label), project: static (validated, _) => Many(key: VerticesKey, values: validated.DuplicateVertices())),
                    Mesh mesh => Many(key: VerticesKey, values: mesh.Vertices.ToPoint3dArray()).ToEff(),
                    PointCloud cloud => Many(key: VerticesKey, values: cloud.GetPoints()).ToEff(),
                    SubD subd => Many(
                            key: VerticesKey, values: LanguageExt.List.unfold(
                                state: (SubDVertex?)subd.Vertices.First, unfolder: static current => current switch {
                                    SubDVertex vertex => Some((vertex.ControlNetPoint, (SubDVertex?)vertex.Next)),
                                    _ => None,
                                }))
                        .ToEff(),
                    BoundingBox box => Many(key: VerticesKey, values: box.GetCorners()).ToEff(),
                    Box box => Many(key: VerticesKey, values: box.GetCorners()).ToEff(),
                    _ => Fin.Fail<Seq<Point3d>>(VerticesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                })),
        _ => VerticesKey.Unsupported<TGeometry, TOut>(),
    };
    internal static Eff<Analyze.Runtime, Seq<TOut>> BrepLeaves<TOut>(Brep brep, Op key, Func<Op, string, Error> primitiveFault, Func<Brep, Context, Fin<Seq<TOut>>> project) =>
        from context in Analyze.Asks
        from validated in context.Validate(geometry: brep, requirement: Requirement.Basic).ToEff()
        from result in (GeometryKinds.KindOfBrep(brep: validated, context: context) switch {
            GeometryKind.BrepSphere => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Sphere")),
            GeometryKind.BrepCylinder => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cylinder")),
            GeometryKind.BrepCone => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cone")),
            GeometryKind.BrepTorus => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Torus")),
            _ => project(arg1: validated, arg2: context),
        }).ToEff()
        select result;
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Brep) => Native<TGeometry, TOut, Brep, Brep>(key: ComponentsKey, project: static brep => (brep.GetConnectedComponents() switch {
            Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
            _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
        }).ToEff()),
        (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) => Native<TGeometry, TOut, Mesh, Mesh>(key: ComponentsKey, project: static mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()).ToEff()),
        _ => ComponentsKey.Unsupported<TGeometry, TOut>(),
    };
    public static Query<Mesh, bool> IsManifold =>
        Query<Mesh, bool>.Build(key: IsManifoldKey, evaluator: static geometry => One(key: IsManifoldKey, value: geometry.IsManifold()).ToEff());
    public static Query<Mesh, bool> NakedPointStatus =>
        Query<Mesh, bool>.Build(key: NakedPointStatusKey, evaluator: static geometry => Many(key: NakedPointStatusKey, values: geometry.GetNakedEdgePointStatus()).ToEff());
    public static Query<Mesh, MeshCheckParameters> MeshCheck =>
        Query<Mesh, MeshCheckParameters>.Build(key: MeshCheckKey, evaluator: static geometry => MeshCheckParametersFor(geometry: geometry).ToEff());
    // Mesh.Check requires a TextLog using-local and a by-ref MeshCheckParameters; the imperative
    // shape is intrinsic to this Mesh.Check boundary adapter and cannot be expression-bodied.
    internal static Fin<Seq<MeshCheckParameters>> MeshCheckParametersFor(Mesh geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry.Check(
            textLog: textLog,
            parameters: ref parameters) switch {
                true or false => One(key: MeshCheckKey, value: parameters),
            };
    }
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) => Optional(value: count)
            .Bind(static metric => metric.Project)
            .Map(static project => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: project,
                evaluator: static (counter, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                         from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                                                         from result in One(key: MeshCheckCountKey, value: counter(arg: head)).ToEff()
                                                         select result))
            .IfNone(static () => Query<Mesh, int>.Reject(key: MeshCheckCountKey, fault: MeshCheckCountKey.InvalidInput()));
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) => Optional(value: metric)
            .Bind(static candidate => candidate.Project)
            .Map(static project => Query<Mesh, MeshFaceSample>.Build(
                key: MeshFaceMetricKey,
                state: project,
                requirement: Requirement.MeshCheck,
                evaluator: static (faceMetric, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => faceMetric(arg1: geometry, arg2: face) switch {
                        double value when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(new MeshFaceSample(Face: face, Value: value)),
                        _ => Fin.Fail<MeshFaceSample>(MeshFaceMetricKey.InvalidResult()),
                    })
                    .As()
                    .ToEff()))
            .IfNone(static () => Query<Mesh, MeshFaceSample>.Reject(key: MeshFaceMetricKey, fault: MeshFaceMetricKey.InvalidInput()));
    public static Query<Mesh, Polyline> SelfIntersections =>
        Query<Mesh, Polyline>.Build(
            key: SelfIntersectionsKey,
            requirement: Requirement.Basic,
            evaluator: static geometry => from runtime in Analyze.RuntimeAsks
                                          from result in SelfIntersectionsValue(geometry: geometry, runtime: runtime).ToEff()
                                          select result);
    // Mesh.GetSelfIntersections requires by-ref/out parameters and a TextLog using-local; the
    // CleanupFinally exemption permits the using-block at this GeometryBase boundary adapter.
    internal static Fin<Seq<Polyline>> SelfIntersectionsValue(Mesh geometry, Analyze.Runtime runtime) {
        using TextLog textLog = new();
        return geometry.GetSelfIntersections(
            tolerance: runtime.Context.MeshIntersectionTolerance,
            perforations: out Polyline[] perforations,
            overlapsPolylines: true,
            overlapsPolylinesResult: out Polyline[] overlaps,
            overlapsMesh: false,
            overlapsMeshResult: out Mesh _,
            textLog: textLog,
            cancel: runtime.Cancellation,
            progress: runtime.Progress) switch {
                true => (Many(key: SelfIntersectionsKey, values: perforations), Many(key: SelfIntersectionsKey, values: overlaps))
                    .Apply((left, right) => left + right)
                    .As(),
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(new OpFault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(SelfIntersectionsKey.InvalidResult()),
            };
    }
    internal static Fin<Seq<Point3d>> EdgeCurveMidpoints(IEnumerable<Curve>? curves, Context context) => Optional(curves).ToFin(EdgeMidpointsKey.InvalidResult()).Bind(source => toSeq(source)
            .TraverseM(curve => {
                using Curve disposable = curve;  // BOUNDARY ADAPTER -- DisposableCurve
                return CurveAtNormalizedValue(curve: disposable, context: context, key: EdgeMidpointsKey, project: static (c, parameter) => c.PointAt(t: parameter));
            }).As()).Bind(static points => Many(key: EdgeMidpointsKey, values: points));
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull {
        ArgumentNullException.ThrowIfNull(argument: aspect);
        return aspect.Apply<TGeometry, TOut>();
    }
    internal static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(Faces selector, Requirement requirement, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, TValue>.Build(
            key: FacesKey, state: (Selector: selector, Transfer: transfer, Project: project), requirement: requirement,
            evaluator: static (state, geometry) => ProjectFaces(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    internal static Eff<Analyze.Runtime, Seq<FaceProjection>> FaceProjections(object geometry, Faces selector) =>
        ProjectFaces(geometry: geometry, selector: selector, transfer: true, project: static (values, _) => Fin.Succ(values));
    internal static Eff<Analyze.Runtime, Seq<TValue>> ProjectFaces<TGeometry, TValue>(TGeometry geometry, Faces selector, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from context in Analyze.Asks
        from faces in DecomposeFaces(geometry: geometry).ToEff()
        from chosen in SelectFaces(faces: faces, selector: selector, runtime: context).ToEff()
        from result in ProjectOwned(all: faces, chosen: chosen, transfer: transfer, project: values => project(arg1: values, arg2: context), same: static (left, right) => ReferenceEquals(objA: left.Brep, objB: right.Brep), dispose: static face => face.Dispose()).ToEff()
        select result;
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull {
        ArgumentNullException.ThrowIfNull(argument: aspect);
        return aspect.Apply<TGeometry, TOut>();
    }
    internal static Eff<Analyze.Runtime, Seq<CurveProjection>> CurveProjections(object geometry, Curves aspect) =>
        ProjectCurves(geometry: geometry, selector: aspect, transfer: true, project: static values => Fin.Succ(values));
    internal static Query<TGeometry, TOut> CurveQuery<TGeometry, TOut, TValue>(Curves selector, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project, bool transfer = false) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: CurvesKey, query: Query<TGeometry, TValue>.Build(
            key: CurvesKey,
            state: (Selector: selector, Transfer: transfer, Project: project),
            evaluator: static (state, geometry) => ProjectCurves(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    internal static Eff<Analyze.Runtime, Seq<TValue>> ProjectCurves<TGeometry, TValue>(TGeometry geometry, Curves selector, bool transfer, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from runtime in Analyze.RuntimeAsks
        from curves in ExtractCurveProjections(geometry: geometry, aspect: selector is Rasm.Analysis.Curves.AtCase ? Rasm.Analysis.Curves.All : selector, runtime: runtime).ToEff()
        from chosen in SelectCurves(curves: curves, aspect: selector).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, transfer: transfer, project: project, same: static (left, right) => ReferenceEquals(objA: left.Curve, objB: right.Curve), dispose: static curve => curve.Dispose()).ToEff()
        select result;
    internal static Fin<Seq<CurveProjection>> ExtractCurveProjections<TGeometry>(TGeometry geometry, Curves aspect, Analyze.Runtime runtime) where TGeometry : notnull =>
        aspect switch {
            Rasm.Analysis.Curves.AllCase or Rasm.Analysis.Curves.BoundaryCase or Rasm.Analysis.Curves.SegmentsCase or Rasm.Analysis.Curves.NakedOuterCase or Rasm.Analysis.Curves.NakedInnerCase or Rasm.Analysis.Curves.InteriorCase or Rasm.Analysis.Curves.NonManifoldCase or Rasm.Analysis.Curves.SubCurvesCase => CurvesOf(geometry: geometry, selector: aspect),
            Rasm.Analysis.Curves.OuterLoopCase => LoopCurves(geometry: geometry, feature: CurveFeature.OuterLoop, loopType: BrepLoopType.Outer),
            Rasm.Analysis.Curves.InnerLoopCase => LoopCurves(geometry: geometry, feature: CurveFeature.InnerLoop, loopType: BrepLoopType.Inner),
            Rasm.Analysis.Curves.IsoUCase => IsoCurves(geometry: geometry, direction: 0, feature: CurveFeature.IsoU),
            Rasm.Analysis.Curves.IsoVCase => IsoCurves(geometry: geometry, direction: 1, feature: CurveFeature.IsoV),
            Rasm.Analysis.Curves.SilhouetteCase silhouette => SilhouetteCurves(geometry: geometry, direction: Optional(value: silhouette.Direction).IfNone(static () => Vector3d.ZAxis), runtime: runtime),
            Rasm.Analysis.Curves.DraftCase draft => DraftCurves(geometry: geometry, direction: Optional(value: draft.Direction).IfNone(static () => Vector3d.ZAxis), angle: Optional(value: draft.Angle).IfNone(static () => 0.0), runtime: runtime),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.InvalidInput()),
        };
    internal static Fin<Seq<CurveProjection>> CurvesOf<TGeometry>(TGeometry geometry, Curves selector) where TGeometry : notnull =>
        (selector, geometry) switch {
            (Curves kind, Curve curve) when kind.InputCurve => ProjectCurve(curve: curve, selector: kind, pieceSource: ComponentIndexType.PolycurveSegment, splitInput: true),
            (Curves kind, Line line) when line.IsValid && kind.InputCurve => PrimitiveCurve(primitive: line, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (Curves kind, Polyline polyline) when polyline.IsValid && kind.InputCurve => PrimitiveCurve(primitive: polyline, selector: kind, pieceSource: ComponentIndexType.PolycurveSegment, convert: static value => value.ToPolylineCurve()),
            (Curves kind, Circle circle) when circle.IsValid && kind.InputCurve => PrimitiveCurve(primitive: circle, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (Curves kind, Arc arc) when arc.IsValid && kind.InputCurve => PrimitiveCurve(primitive: arc, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (Curves kind, Brep brep) when kind.Edge is { } edge => BrepEdgeCurves(brep: brep, feature: edge.Feature, predicate: edge.Brep),
            (Rasm.Analysis.Curves.AllCase or Rasm.Analysis.Curves.BoundaryCase, BrepFace face) => Optional(face.DuplicateFace(duplicateMeshes: false))
                .ToFin(CurvesKey.InvalidResult())
                .Bind(duplicate => { using Brep disposable = duplicate; return BrepEdgeCurves(brep: disposable, feature: CurveFeature.Boundary, predicate: static _ => true); }),
            (Rasm.Analysis.Curves.AllCase or Rasm.Analysis.Curves.BoundaryCase, Surface surface) => SurfaceBoundaryCurves(surface: surface, feature: CurveFeature.Boundary),
            (Rasm.Analysis.Curves.AllCase or Rasm.Analysis.Curves.SegmentsCase, SubD subd) => subd.UpdateSurfaceMeshCache(lazyUpdate: true) switch {
                _ => IndexedCurves(curves: subd.DuplicateEdgeCurves(), feature: selector is Rasm.Analysis.Curves.SegmentsCase ? CurveFeature.Segment : CurveFeature.Edge, sourceType: ComponentIndexType.SubdEdge),
            },
            (Rasm.Analysis.Curves.NakedInnerCase, Mesh) => Fin.Succ(Seq<CurveProjection>()),
            (Curves kind, Mesh mesh) when kind.Edge is { } edge => MeshEdgeCurves(mesh: mesh, feature: edge.Feature, predicate: edge.Mesh),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    internal static Fin<Seq<CurveProjection>> PrimitiveCurve<TPrimitive>(TPrimitive primitive, Curves selector, ComponentIndexType pieceSource, Func<TPrimitive, Curve?> convert) => Optional(convert(arg: primitive)).ToFin(CurvesKey.InvalidResult()).Bind(curve => { using Curve disposable = curve; return ProjectCurve(curve: disposable, selector: selector, pieceSource: pieceSource, splitInput: false); });
    internal static Fin<Seq<CurveProjection>> IsoCurves<TGeometry>(TGeometry geometry, int direction, CurveFeature feature) where TGeometry : notnull =>
        geometry switch {
            Brep brep => toSeq(brep.Faces)
                .TraverseM(face => MidIsoCurve(surface: face, direction: direction, feature: feature, source: new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))
                .As()
                .Map(static nested => nested.Bind(static curves => curves)),
            Surface surface => MidIsoCurve(surface: surface, direction: direction, feature: feature, source: new ComponentIndex(type: ComponentIndexType.NoType, index: 0)),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    internal static Fin<Seq<CurveProjection>> ProjectCurve(Curve? curve, Curves selector, ComponentIndexType pieceSource, bool splitInput) => Optional(curve).ToFin(CurvesKey.InvalidResult()).Bind(value => (selector, splitInput) switch {
        (Curves kind, true) when kind.InputBoundary => CurvePieces(curve: value, feature: CurveFeature.Input, pieceSource: ComponentIndexType.NoType, fallbackSource: ComponentIndexType.NoType, project: static candidate => candidate.DuplicateSegments()),
        (Curves kind, _) when kind.InputBoundary => OneCurve(curve: value, feature: CurveFeature.Input, type: ComponentIndexType.NoType),
        (Rasm.Analysis.Curves.SegmentsCase, _) => CurvePieces(curve: value, feature: CurveFeature.Segment, pieceSource: pieceSource, fallbackSource: pieceSource, project: static candidate => candidate.DuplicateSegments()),
        (Rasm.Analysis.Curves.SubCurvesCase, _) => CurvePieces(curve: value, feature: CurveFeature.SubCurve, pieceSource: pieceSource, fallbackSource: ComponentIndexType.NoType, project: static candidate => candidate.GetSubCurves()),
        _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.InvalidInput()),
    });
    internal static Fin<Seq<CurveProjection>> CurvePieces(Curve curve, CurveFeature feature, ComponentIndexType pieceSource, ComponentIndexType fallbackSource, Func<Curve, Curve[]?> project) =>
        project(arg: curve) switch {
            Curve[] pieces when pieces.Length > 0 => IndexedCurves(curves: pieces, feature: feature, sourceType: pieceSource),
            _ => Optional(curve.DuplicateCurve())
                .ToFin(CurvesKey.InvalidResult())
                .Map(duplicate => Seq(new CurveProjection(curve: duplicate, feature: feature, type: fallbackSource, index: 0))),
        };
    internal static Fin<Seq<CurveProjection>> SurfaceBoundaryCurves(Surface surface, CurveFeature feature) =>
        Seq(IsoStatus.South, IsoStatus.East, IsoStatus.North, IsoStatus.West)
            .TraverseM(iso => Optional(surface.IsoCurve(iso: iso)).ToFin(CurvesKey.InvalidResult()))
            .As()
            .Map(curves => curves.Map((curve, index) => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.NoType, index: index)));
    internal static Fin<Seq<Curve>> IsoCurveValues(Surface surface, IsoStatus iso, double normalized, Op key) =>
        iso switch {
            IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North => Optional(surface.IsoCurve(iso: iso))
                    .ToFin(key.InvalidResult())
                    .Map(static curve => Seq(curve)),
            IsoStatus.X or IsoStatus.Y => iso switch {
                IsoStatus.X => 0,
                _ => 1,
            } switch {
                int direction => surface.Domain(direction: direction) switch {
                    Interval domain when domain.IsValid && normalized is >= 0.0 and <= 1.0 => surface switch {
                        BrepFace face => Many(key: key, values: face.TrimAwareIsoCurve(direction: direction, constantParameter: domain.ParameterAt(normalizedParameter: normalized))),
                        _ => Optional(surface.IsoCurve(iso, domain.ParameterAt(normalizedParameter: normalized)))
                            .ToFin(key.InvalidResult())
                            .Map(static curve => Seq(curve)),
                    },
                    _ => Fin.Fail<Seq<Curve>>(key.InvalidInput()),
                },
            },
            _ => Fin.Fail<Seq<Curve>>(key.InvalidInput()),
        };
    internal static Fin<Seq<CurveProjection>> MidIsoCurve(Surface surface, int direction, CurveFeature feature, ComponentIndex source) =>
        IsoCurveValues(
                surface: surface,
                iso: direction switch { 0 => IsoStatus.X, _ => IsoStatus.Y },
                normalized: 0.5,
                key: CurvesKey)
            .Map(curves => curves.Map(curve => new CurveProjection(curve: curve, feature: feature, source: source)));
    internal static Fin<Seq<CurveProjection>> OneCurve(Curve? curve, CurveFeature feature, ComponentIndexType type) => Optional(curve)
            .Bind(static value => Optional(value.DuplicateCurve()))
            .ToFin(CurvesKey.InvalidResult())
            .Map(duplicate => Seq(new CurveProjection(curve: duplicate, feature: feature, type: type, index: 0)));
    internal static Fin<Seq<CurveProjection>> IndexedCurves(IEnumerable<Curve>? curves, CurveFeature feature, ComponentIndexType sourceType) => Optional(curves).ToFin(CurvesKey.InvalidResult()).Map(values => toSeq(values.Select((curve, index) => Optional(curve)
                .Map(value => new CurveProjection(curve: value, feature: feature, type: sourceType, index: index))))
            .Bind(static projection => projection.ToSeq()));
    internal static Fin<Seq<CurveProjection>> BrepEdgeCurves(Brep brep, CurveFeature feature, Func<BrepEdge, bool> predicate) =>
        Fin.Succ(toSeq(brep.Edges)
            .Where(edge => predicate(arg: edge))
            .Bind(edge => Optional(edge.DuplicateCurve())
                .Map(curve => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.BrepEdge, index: edge.EdgeIndex))
                .ToSeq()));
    internal static Fin<Seq<CurveProjection>> MeshEdgeCurves(Mesh mesh, CurveFeature feature, Func<Mesh, int, bool> predicate) =>
        Fin.Succ(toSeq(Enumerable.Range(start: 0, count: mesh.TopologyEdges.Count))
            .Where(index => predicate(arg1: mesh, arg2: index))
            .Map(index => new CurveProjection(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: index).ToNurbsCurve(), feature: feature, type: ComponentIndexType.MeshTopologyEdge, index: index)));
    internal static Fin<Seq<CurveProjection>> LoopCurves<TGeometry>(TGeometry geometry, CurveFeature feature, BrepLoopType loopType) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(toSeq(brep.Loops)
                .Where(loop => loop.LoopType == loopType)
                .Bind(loop => Optional(loop.To3dCurve())
                    .Map(curve => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.BrepLoop, index: loop.LoopIndex))
                    .ToSeq())),
            BrepFace face => Optional(face.DuplicateFace(duplicateMeshes: false))
                .ToFin(CurvesKey.InvalidResult())
                .Bind(duplicate => { using Brep disposable = duplicate; return LoopCurves(geometry: disposable, feature: feature, loopType: loopType); }),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    internal static Fin<Seq<CurveProjection>> SilhouetteCurves<TGeometry>(TGeometry geometry, Vector3d direction, Analyze.Runtime runtime) where TGeometry : notnull =>
        SilhouetteProjections(geometry: geometry, state: (Direction: direction, Runtime: runtime), feature: CurveFeature.Silhouette, valid: static state => state.Direction.IsValid && !state.Direction.IsTiny(), project: static (native, state) => Silhouette.Compute(geometry: native, silhouetteType: SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, parallelCameraDirection: state.Direction, tolerance: state.Runtime.Context.Absolute.Value, angleToleranceRadians: state.Runtime.Context.Angle.Value, clippingPlanes: null!, cancelToken: state.Runtime.Cancellation));
    internal static Fin<Seq<CurveProjection>> DraftCurves<TGeometry>(TGeometry geometry, Vector3d direction, double angle, Analyze.Runtime runtime) where TGeometry : notnull =>
        SilhouetteProjections(geometry: geometry, state: (Direction: direction, Angle: angle, Runtime: runtime), feature: CurveFeature.Draft, valid: static state => state.Direction.IsValid && !state.Direction.IsTiny() && RhinoMath.IsValidDouble(x: state.Angle), project: static (native, state) => Silhouette.ComputeDraftCurve(geometry: native, draftAngle: state.Angle, pullDirection: state.Direction, tolerance: state.Runtime.Context.Absolute.Value, angleToleranceRadians: state.Runtime.Context.Angle.Value, cancelToken: state.Runtime.Cancellation));
    internal static Fin<Seq<CurveProjection>> SilhouetteProjections<TGeometry, TState>(TGeometry geometry, TState state, CurveFeature feature, Func<TState, bool> valid, Func<GeometryBase, TState, Silhouette[]?> project) where TGeometry : notnull =>
        (geometry, valid(arg: state)) switch {
            (GeometryBase native, true) => Optional(project(arg1: native, arg2: state))
                .ToFin(CurvesKey.InvalidResult())
                .Map(values => toSeq(values).Map(silhouette => new CurveProjection(curve: silhouette.Curve, feature: feature, source: silhouette.GeometryComponentIndex))),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
        };
    internal static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
        Seq<TProjection> all,
        Seq<TProjection> chosen,
        bool transfer,
        Func<Seq<TProjection>, Fin<Seq<TValue>>> project,
        Func<TProjection, TProjection, bool> same,
        Func<TProjection, Unit> dispose) {
        Fin<Seq<TValue>> result = project(arg: chosen);
        _ = all
            .Filter(value => (transfer && result.IsSucc, chosen.Exists(candidate => same(arg1: candidate, arg2: value))) switch {
                (true, true) => false,
                _ => true,
            })
            .Iter(value => dispose(arg: value));
        return result;
    }
    internal static Fin<Seq<CurveProjection>> SelectCurves(Seq<CurveProjection> curves, Curves aspect) =>
        (aspect, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<CurveProjection>()),
            (Rasm.Analysis.Curves.AtCase at, int count) => Fin.Succ(Seq(curves[RhinoMath.Clamp(at.Value ?? 0, 0, count - 1)])),
            _ => Fin.Succ(curves),
        };
    internal static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) =>
        FaceCentroid(face: face, runtime: runtime)
            .Bind(centroid => {
                BrepFace brepFace = face.Brep.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: double.MaxValue) switch {
                    true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                        (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() => Fin.Succ((frame.ZAxis * (face.Reversed ? -normal : normal)) switch {
                            >= 0.0 => frame,
                            _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                        }),
                        _ => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                };
            });
    internal static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) => Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    internal static Fin<Seq<Interval>> FaceDomains(FaceProjection face) =>
        (face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
            _ => Fin.Fail<Seq<Interval>>(FacesKey.InvalidResult()),
        };
    internal static Fin<Seq<FaceProjection>> DecomposeFaces<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(BrepFaceProjections(brep: brep)),
            BrepFace face => Fin.Succ(Seq(FaceProjection.From(face: face))),
            GeometryBase native when native is not Mesh && native.HasBrepForm => Optional(Brep.TryConvertBrep(geometry: native))
                .ToFin(FacesKey.InvalidResult())
                .Map(static brep => { using Brep disposable = brep; return BrepFaceProjections(brep: disposable); }),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Brep))),
        };
    internal static Seq<FaceProjection> BrepFaceProjections(Brep brep) => toSeq(brep.Faces.Select(static face => FaceProjection.From(face: face)));
    internal static Fin<Seq<FaceProjection>> SelectFaces(Seq<FaceProjection> faces, Faces selector, Context runtime) =>
        (selector, faces.Count) switch {
            (_, 0) => Fin.Succ(Seq<FaceProjection>()),
            (Rasm.Analysis.Faces.AllCase, _) => Fin.Succ(faces),
            (Rasm.Analysis.Faces.TopCase top, _) => RankByCentroidAxis(faces: faces, axis: top.Axis, descending: true, runtime: runtime),
            (Rasm.Analysis.Faces.BottomCase bottom, _) => RankByCentroidAxis(faces: faces, axis: bottom.Axis, descending: false, runtime: runtime),
            (Rasm.Analysis.Faces.AtCase at, int count) => Fin.Succ(Seq(faces[RhinoMath.Clamp(at.Value ?? 0, 0, count - 1)])),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.InvalidInput()),
        };
    internal static Fin<Seq<FaceProjection>> RankByCentroidAxis(Seq<FaceProjection> faces, Vector3d axis, bool descending, Context runtime) =>
        axis switch {
            { IsValid: true } when !axis.IsTiny() => faces
                .Traverse(face => FaceCentroid(face: face, runtime: runtime)
                    .Map(point => (face, Score: new Vector3d(x: point.X, y: point.Y, z: point.Z) * axis))).As()
                .Map(ranked => (ranked.IsEmpty, descending) switch {
                    (true, _) => Seq<FaceProjection>(),
                    (false, true) => ranked.Maxima(projection: static item => item.Score, tolerance: runtime.Absolute.Value * axis.Length).Map(static item => item.face),
                    (false, false) => ranked.Minima(projection: static item => item.Score, tolerance: runtime.Absolute.Value * axis.Length).Map(static item => item.face),
                }),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.InvalidInput()),
        };
}

// --- [FACES_ROLE] ------------------------------------------------------------------------
internal static class FacesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Faces selector) where TGeometry : notnull =>
        Query.Supports(typeof(TGeometry)) switch {
            false => Query.FacesKey.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Query.FaceQuery<TGeometry, TOut, Brep>(selector: selector, requirement: Requirement.None, transfer: true, project: static (chosen, _) => Query.Many(key: Query.FacesKey, values: chosen.Map(static face => face.Brep))),
                Type t when t == typeof(Plane) => Query.FaceQuery<TGeometry, TOut, Plane>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Point3d) => Query.FaceQuery<TGeometry, TOut, Point3d>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FaceCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Vector3d) => Query.FaceQuery<TGeometry, TOut, Vector3d>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                Type t when t == typeof(int) => Query.FaceQuery<TGeometry, TOut, int>(selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Query.Many(key: Query.FacesKey, values: chosen.Map(static face => face.FaceIndex))),
                Type t when t == typeof(ComponentIndex) => Query.FaceQuery<TGeometry, TOut, ComponentIndex>(selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Query.Many(key: Query.FacesKey, values: chosen.Map(static face => new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))),
                Type t when t == typeof(Interval) => Query.FaceQuery<TGeometry, TOut, Interval>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, _) => chosen.Traverse(Query.FaceDomains).Map(static nested => nested.Bind(static domain => domain)).As()),
                _ => Query.FacesKey.Unsupported<TGeometry, TOut>(),
            },
        };
}

// --- [CURVES_ROLE] -----------------------------------------------------------------------
internal static class CurvesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Curves selector) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when Query.Supports(geometry: geometry, output: output, target: typeof(Curve), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) => Query.CurveQuery<TGeometry, TOut, Curve>(selector: selector, transfer: true, project: static chosen => Query.Many(key: Query.CurvesKey, values: chosen.Map(static curve => curve.Curve))),
            (Type geometry, Type output) when Query.Supports(geometry: geometry, output: output, target: typeof(CurveFeature), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) => Query.CurveQuery<TGeometry, TOut, CurveFeature>(selector: selector, project: static chosen => Query.Many(key: Query.CurvesKey, values: chosen.Map(static curve => curve.Feature))),
            (Type geometry, Type output) when Query.Supports(geometry: geometry, output: output, target: typeof(ComponentIndex), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) => Query.CurveQuery<TGeometry, TOut, ComponentIndex>(selector: selector, project: static chosen => Query.Many(key: Query.CurvesKey, values: chosen.Map(static curve => curve.Source))),
            _ => Query.CurvesKey.Unsupported<TGeometry, TOut>(),
        };
}

// --- [PRIMITIVE_DISPATCH] ----------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PrimitiveDispatch {
    public static readonly PrimitiveDispatch Circle = new(key: 1, sourceType: typeof(Curve), resultType: typeof(Circle), extract: static (geometry, context) => geometry switch {
        Curve curve when curve.TryGetCircle(circle: out Circle value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Curve => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Circle))),
    });
    public static readonly PrimitiveDispatch Arc = new(key: 2, sourceType: typeof(Curve), resultType: typeof(Arc), extract: static (geometry, context) => geometry switch {
        Curve curve when curve.TryGetArc(arc: out Arc value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Curve => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Arc))),
    });
    public static readonly PrimitiveDispatch Ellipse = new(key: 3, sourceType: typeof(Curve), resultType: typeof(Ellipse), extract: static (geometry, context) => geometry switch {
        Curve curve when curve.TryGetEllipse(ellipse: out Ellipse value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Curve => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Ellipse))),
    });
    public static readonly PrimitiveDispatch Polyline = new(key: 4, sourceType: typeof(Curve), resultType: typeof(Polyline), extract: static (geometry, _) => geometry switch {
        Curve curve when curve.TryGetPolyline(polyline: out Polyline value) => Fin.Succ<object>(value),
        Curve => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Polyline))),
    });
    public static readonly PrimitiveDispatch Plane = new(key: 5, sourceType: typeof(Surface), resultType: typeof(Plane), extract: static (geometry, context) => geometry switch {
        Surface surface when surface.TryGetPlane(plane: out Plane value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Surface => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Plane))),
    });
    public static readonly PrimitiveDispatch Cylinder = new(key: 6, sourceType: typeof(Surface), resultType: typeof(Cylinder), extract: static (geometry, context) => geometry switch {
        Surface surface when surface.TryGetCylinder(cylinder: out Cylinder value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Surface => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Cylinder))),
    });
    public static readonly PrimitiveDispatch Sphere = new(key: 7, sourceType: typeof(Surface), resultType: typeof(Sphere), extract: static (geometry, context) => geometry switch {
        Surface surface when surface.TryGetSphere(sphere: out Sphere value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Surface => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Sphere))),
    });
    public static readonly PrimitiveDispatch Cone = new(key: 8, sourceType: typeof(Surface), resultType: typeof(Cone), extract: static (geometry, context) => geometry switch {
        Surface surface when surface.TryGetCone(cone: out Cone value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Surface => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Cone))),
    });
    public static readonly PrimitiveDispatch Torus = new(key: 9, sourceType: typeof(Surface), resultType: typeof(Torus), extract: static (geometry, context) => geometry switch {
        Surface surface when surface.TryGetTorus(torus: out Torus value, tolerance: context.Absolute.Value) => Fin.Succ<object>(value),
        Surface => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Torus))),
    });
    public static readonly PrimitiveDispatch Box = new(key: 10, sourceType: typeof(Brep), resultType: typeof(Box), extract: static (geometry, context) => geometry switch {
        Brep brep when brep.IsBox(tolerance: context.Absolute.Value)
                       && brep.GetBoundingBox(plane: Rhino.Geometry.Plane.WorldXY, worldBox: out Box value) is { IsValid: true } => Fin.Succ<object>(value),
        Brep => Fin.Fail<object>(Query.PrimitiveKey.InvalidResult()),
        _ => Fin.Fail<object>(Query.PrimitiveKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Box))),
    });
    public Type SourceType { get; }
    public Type ResultType { get; }
    internal Func<GeometryBase, Context, Fin<object>> Extract { get; }
    internal Query<TGeometry, TOut> Build<TGeometry, TOut>() where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: Query.PrimitiveKey,
            requirement: Requirement.Basic,
            requiresContext: true,
            state: this,
            evaluator: static (dispatch, geometry) => from context in Analyze.Asks
                                                      from native in (geometry switch {
                                                          GeometryBase value => Fin.Succ(value),
                                                          _ => Fin.Fail<GeometryBase>(Query.PrimitiveKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
                                                      }).ToEff()
                                                      from boxed in dispatch.Extract(arg1: native, arg2: context).ToEff()
                                                      from result in Query.PrimitiveKey.One(value: (TOut)boxed).ToEff()
                                                      select result);
}

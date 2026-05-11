namespace Rasm.Analysis;

public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Native<TGeometry, TOut, Curve, Interval>(key: DomainKey, project: static curve => One(key: DomainKey, value: curve.Domain).ToEff()),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Interval) =>
                Native<TGeometry, TOut, Surface, Interval>(key: DomainKey, project: static surface => (One(key: DomainKey, value: surface.Domain(direction: 0)), One(key: DomainKey, value: surface.Domain(direction: 1))).Apply((u, v) => u + v).As().ToEff()),
            _ => DomainKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) =>
                Cast<TGeometry, TOut>(key: SegmentsKey, query: Query<Polyline, Line>.Build(key: SegmentsKey, evaluator: static geometry => Many(key: SegmentsKey, values: geometry.GetSegments()).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Native<TGeometry, TOut, Curve, Curve>(key: SegmentsKey, project: static curve => Many(key: SegmentsKey, values: curve.DuplicateSegments()).ToEff()),
            _ => SegmentsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Brep, Curve> Edges => Query<Brep, Curve>.Build(key: EdgesKey, evaluator: static geometry => Many(key: EdgesKey, values: geometry.DuplicateEdgeCurves()).ToEff());
    public static Query<TGeometry, TOut> EdgeMidpoints<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)]) =>
                Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                    key: EdgeMidpointsKey,
                    evaluator: static geometry => geometry switch {
                        Line line => One(key: EdgeMidpointsKey, value: line.PointAt(t: 0.5)).ToEff(),
                        Polyline polyline => Many(key: EdgeMidpointsKey, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                        BoundingBox box => Many(key: EdgeMidpointsKey, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                        Curve curve => CurveAtNormalized(geometry: curve, key: EdgeMidpointsKey, project: static (geometry, parameter) => geometry.PointAt(t: parameter)),
                        Brep brep => BrepLeaves(brep: brep, key: EdgeMidpointsKey, primitiveFault: static (key, label) => key.PrimitiveNoEdges(primitive: label), project: static (validated, context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context)),
                        Mesh mesh =>
                            from ctx in Analyze.Asks
                            from validated in ctx.Validate(geometry: mesh, requirement: Requirement.Basic).ToEff()
                            from result in Many(key: EdgeMidpointsKey, values: Enumerable.Range(start: 0, count: validated.TopologyEdges.Count).Select(index => validated.TopologyEdges.EdgeLine(topologyEdgeIndex: index).PointAt(t: 0.5))).ToEff()
                            select result,
                        SubD subd =>
                            from ctx in Analyze.Asks
                            from validated in ctx.Validate(geometry: subd, requirement: Requirement.Basic).ToEff()
                            from result in EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: ctx).ToEff()
                            select result,
                        Box box =>
                            from ctx in Analyze.Asks
                            from result in Optional(box.ToBrep())
                                .ToFin(EdgeMidpointsKey.InvalidResult())
                                .Bind(brep => { using Brep disposable = brep; return EdgeCurveMidpoints(curves: disposable.DuplicateEdgeCurves(), context: ctx); })
                                .ToEff()
                            select result,
                        _ => Fin.Fail<Seq<Point3d>>(EdgeMidpointsKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Point3d))).ToEff(),
                    })),
            _ => EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) =>
                Native<TGeometry, TOut, Brep, Curve>(key: NakedEdgesKey, project: static brep => Many(key: NakedEdgesKey, values: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                Native<TGeometry, TOut, Mesh, Polyline>(key: NakedEdgesKey, project: static mesh => Many(key: NakedEdgesKey, values: mesh.GetNakedEdges()).ToEff()),
            _ => NakedEdgesKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<Mesh, Polyline> Outlines(Plane plane) =>
        Query<Mesh, Polyline>.Build(key: OutlinesKey, state: plane, evaluator: static (sectionPlane, geometry) => Many(key: OutlinesKey, values: geometry.GetOutlines(plane: sectionPlane)).ToEff());
    private static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target && Supports(geometry: geometry, native: native);
    private static bool Supports(Type geometry, params Type[] native) =>
        typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object) || native.Contains(value: geometry);
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) =>
        Query<Surface, Curve>.Build(key: IsoKey, requirement: Requirement.SurfaceEvaluation, state: (Iso: iso, Normalized: normalized), evaluator: static (state, geometry) => IsoCurveValues(surface: geometry, iso: state.Iso, normalized: state.Normalized, key: IsoKey).ToEff());
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TGeometry) switch {
            Type geometry when typeof(Curve).IsAssignableFrom(c: geometry) => typeof(TOut) switch {
                Type output when output == typeof(Circle) => PrimitiveMatch<TGeometry, TOut, Curve, Circle>(project: static (curve, context, out circle) => curve.TryGetCircle(circle: out circle, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Arc) => PrimitiveMatch<TGeometry, TOut, Curve, Arc>(project: static (curve, context, out arc) => curve.TryGetArc(arc: out arc, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Ellipse) => PrimitiveMatch<TGeometry, TOut, Curve, Ellipse>(project: static (curve, context, out ellipse) => curve.TryGetEllipse(ellipse: out ellipse, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Polyline) => PrimitiveMatch<TGeometry, TOut, Curve, Polyline>(project: static (curve, _, out polyline) => curve.TryGetPolyline(polyline: out polyline)),
                _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
            },
            Type geometry when typeof(Surface).IsAssignableFrom(c: geometry) => typeof(TOut) switch {
                Type output when output == typeof(Plane) => PrimitiveMatch<TGeometry, TOut, Surface, Plane>(project: static (surface, context, out plane) => surface.TryGetPlane(plane: out plane, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Cylinder) => PrimitiveMatch<TGeometry, TOut, Surface, Cylinder>(project: static (surface, context, out cylinder) => surface.TryGetCylinder(cylinder: out cylinder, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Sphere) => PrimitiveMatch<TGeometry, TOut, Surface, Sphere>(project: static (surface, context, out sphere) => surface.TryGetSphere(sphere: out sphere, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Cone) => PrimitiveMatch<TGeometry, TOut, Surface, Cone>(project: static (surface, context, out cone) => surface.TryGetCone(cone: out cone, tolerance: context.Absolute.Value)),
                Type output when output == typeof(Torus) => PrimitiveMatch<TGeometry, TOut, Surface, Torus>(project: static (surface, context, out torus) => surface.TryGetTorus(torus: out torus, tolerance: context.Absolute.Value)),
                _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
            },
            Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) && typeof(TOut) == typeof(Box) =>
                PrimitiveMatch<TGeometry, TOut, Brep, Box>(project: static (brep, context, out box) => (brep.IsBox(tolerance: context.Absolute.Value), brep.GetBoundingBox(plane: Plane.WorldXY, worldBox: out box)) switch {
                    (true, BoundingBox local) => local.IsValid,
                    _ => false,
                }),
            _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(GeometryKind), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box), typeof(Sphere)]) =>
                Cast<TGeometry, TOut>(key: KindKey, query: Query<TGeometry, GeometryKind>.Build(
                    key: KindKey,
                    evaluator: static geometry => geometry switch {
                        object value =>
                            from ctx in Analyze.Asks
                            from result in One(key: KindKey, value: GeometryKinds.Kind(geometry: value, context: ctx)).ToEff()
                            select result,
                    })),
            _ => KindKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> SolidOrientation<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(BrepSolidOrientation) =>
                Native<TGeometry, TOut, Brep, BrepSolidOrientation>(key: SolidOrientationKey, project: static brep => One(key: SolidOrientationKey, value: brep.SolidOrientation).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(int) =>
                Native<TGeometry, TOut, Mesh, int>(key: SolidOrientationKey, project: static mesh => One(key: SolidOrientationKey, value: mesh.SolidOrientation()).ToEff()),
            _ => SolidOrientationKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase =>
        point.IsValid switch {
            false => Query<TGeometry, bool>.Reject(key: IsPointInsideKey, fault: IsPointInsideKey.InvalidInput()),
            true => typeof(TGeometry) switch {
                Type geometry when typeof(Brep).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry) =>
                    Cast<TGeometry, bool>(key: IsPointInsideKey, query: Query<TGeometry, bool>.Build(
                        key: IsPointInsideKey,
                        state: point,
                        requirement: Requirement.SolidTopology,
                        evaluator: static (target, geometry) =>
                            from ctx in Analyze.Asks
                            from result in (geometry switch {
                                Brep brep => One(key: IsPointInsideKey, value: brep.IsPointInside(point: target, tolerance: ctx.Absolute.Value, strictlyIn: false)),
                                Mesh mesh => One(key: IsPointInsideKey, value: mesh.IsPointInside(point: target, tolerance: ctx.Absolute.Value, strictlyIn: false)),
                                _ => Fin.Fail<Seq<bool>>(IsPointInsideKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(bool))),
                            }).ToEff()
                            select result)),
                _ => IsPointInsideKey.Unsupported<TGeometry, bool>(),
            },
        };
    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(Point3d), typeof(BoundingBox), typeof(Box)]) =>
                Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
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
                            brep: brep,
                            key: VerticesKey,
                            primitiveFault: static (key, label) => key.PrimitiveNoVertices(primitive: label),
                            project: static (validated, _) => Many(key: VerticesKey, values: validated.DuplicateVertices())),
                        Mesh mesh => Many(key: VerticesKey, values: mesh.Vertices.ToPoint3dArray()).ToEff(),
                        PointCloud cloud => Many(key: VerticesKey, values: cloud.GetPoints()).ToEff(),
                        SubD subd => Many(
                                key: VerticesKey,
                                values: LanguageExt.List.unfold(
                                    state: (SubDVertex?)subd.Vertices.First,
                                    unfolder: static current => current switch {
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
    private static Eff<Analyze.Runtime, Seq<TOut>> BrepLeaves<TOut>(Brep brep, Op key, Func<Op, string, Error> primitiveFault, Func<Brep, Context, Fin<Seq<TOut>>> project) =>
        from ctx in Analyze.Asks
        from validated in ctx.Validate(geometry: brep, requirement: Requirement.Basic).ToEff()
        from result in (GeometryKinds.KindOfBrep(brep: validated, context: ctx) switch {
            GeometryKind.BrepSphere => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Sphere")),
            GeometryKind.BrepCylinder => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cylinder")),
            GeometryKind.BrepCone => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cone")),
            GeometryKind.BrepTorus => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Torus")),
            _ => project(arg1: validated, arg2: ctx),
        }).ToEff()
        select result;
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Brep) =>
                Native<TGeometry, TOut, Brep, Brep>(key: ComponentsKey, project: static brep => (brep.GetConnectedComponents() switch {
                    Brep[] components when components.Length > 0 => Many(key: ComponentsKey, values: components),
                    _ => Many(key: ComponentsKey, values: Brep.SplitDisjointPieces(brep: brep)),
                }).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Mesh) =>
                Native<TGeometry, TOut, Mesh, Mesh>(key: ComponentsKey, project: static mesh => Many(key: ComponentsKey, values: mesh.SplitDisjointPieces()).ToEff()),
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
    private static Fin<Seq<MeshCheckParameters>> MeshCheckParametersFor(Mesh geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry.Check(
            textLog: textLog,
            parameters: ref parameters) switch {
                true or false => One(key: MeshCheckKey, value: parameters),
            };
    }
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) =>
        Optional(value: count)
            .Bind(static metric => metric.Project)
            .Map(static project => Query<Mesh, int>.Build(
                key: MeshCheckCountKey,
                state: project,
                evaluator: static (counter, geometry) =>
                    from parameters in MeshCheck.Apply(geometry: geometry)
                    from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                    from result in One(key: MeshCheckCountKey, value: counter(arg: head)).ToEff()
                    select result))
            .IfNone(static () => Query<Mesh, int>.Reject(key: MeshCheckCountKey, fault: MeshCheckCountKey.InvalidInput()));
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) =>
        Optional(value: metric)
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
    private static Fin<Seq<Polyline>> SelfIntersectionsValue(Mesh geometry, Analyze.Runtime runtime) {
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
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(OpFault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(SelfIntersectionsKey.InvalidResult()),
            };
    }
    private static Fin<Seq<Point3d>> EdgeCurveMidpoints(IEnumerable<Curve>? curves, Context context) =>
        Optional(curves).ToFin(EdgeMidpointsKey.InvalidResult()).Bind(source => toSeq(source)
            .TraverseM(curve => {
                using Curve disposable = curve;  // BOUNDARY ADAPTER -- DisposableCurve
                return CurveAtNormalizedValue(curve: disposable, context: context, key: EdgeMidpointsKey, project: static (c, parameter) => c.PointAt(t: parameter));
            }).As()).Bind(static points => Many(key: EdgeMidpointsKey, values: points));
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull =>
        Aspect(
            aspect: aspect,
            key: FacesKey,
            dispatch: static selector => Supports(geometry: typeof(TGeometry)) switch {
                true => typeof(TOut) switch {
                    Type output when output == typeof(Brep) => FaceQuery<TGeometry, TOut, Brep>(selector: selector, requirement: Requirement.None, transfer: true, project: static (chosen, _) => Many(key: FacesKey, values: chosen.Map(static face => face.Brep))),
                    Type output when output == typeof(Plane) => FaceQuery<TGeometry, TOut, Plane>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => FrameAtCentroid(face: face, runtime: runtime)).As()),
                    Type output when output == typeof(Point3d) => FaceQuery<TGeometry, TOut, Point3d>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => FaceCentroid(face: face, runtime: runtime)).As()),
                    Type output when output == typeof(Vector3d) => FaceQuery<TGeometry, TOut, Vector3d>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                    Type output when output == typeof(int) => FaceQuery<TGeometry, TOut, int>(selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Many(key: FacesKey, values: chosen.Map(static face => face.FaceIndex))),
                    Type output when output == typeof(ComponentIndex) => FaceQuery<TGeometry, TOut, ComponentIndex>(selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Many(key: FacesKey, values: chosen.Map(static face => new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))),
                    Type output when output == typeof(Interval) => FaceQuery<TGeometry, TOut, Interval>(selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, _) => chosen.Traverse(FaceDomains).Map(static nested => nested.Bind(static domain => domain)).As()),
                    _ => null,
                },
                false => null,
            });
    private static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(Faces selector, Requirement requirement, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, TValue>.Build(
            key: FacesKey, state: (Selector: selector, Transfer: transfer, Project: project), requirement: requirement,
            evaluator: static (state, geometry) => ProjectFaces(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    internal static Eff<Analyze.Runtime, Seq<FaceProjection>> FaceProjections(Shape shape, Faces selector) =>
        ProjectFaces(geometry: shape.Inner, selector: selector, transfer: true, project: static (values, _) => Fin.Succ(values));
    private static Eff<Analyze.Runtime, Seq<TValue>> ProjectFaces<TGeometry, TValue>(TGeometry geometry, Faces selector, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from ctx in Analyze.Asks
        from faces in DecomposeFaces(geometry: geometry).ToEff()
        from chosen in SelectFaces(faces: faces, selector: selector, runtime: ctx).ToEff()
        from result in ProjectOwned(all: faces, chosen: chosen, transfer: transfer, project: values => project(arg1: values, arg2: ctx), same: static (left, right) => ReferenceEquals(objA: left.Brep, objB: right.Brep), dispose: static face => face.Dispose()).ToEff()
        select result;
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull =>
        Aspect(
            aspect: aspect,
            key: CurvesKey,
            dispatch: static selector => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(Curve), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) =>
                    CurveQuery<TGeometry, TOut, Curve>(selector: selector, transfer: true, project: static chosen => Many(key: CurvesKey, values: chosen.Map(static curve => curve.Curve))),
                (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(CurveFeature), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) =>
                    CurveQuery<TGeometry, TOut, CurveFeature>(selector: selector, project: static chosen => Many(key: CurvesKey, values: chosen.Map(static curve => curve.Feature))),
                (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(ComponentIndex), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) =>
                    CurveQuery<TGeometry, TOut, ComponentIndex>(selector: selector, project: static chosen => Many(key: CurvesKey, values: chosen.Map(static curve => curve.Source))),
                _ => null,
            });
    internal static Eff<Analyze.Runtime, Seq<CurveProjection>> CurveProjections(Shape shape, Curves aspect) =>
        ProjectCurves(geometry: shape.Inner, selector: aspect, transfer: true, project: static values => Fin.Succ(values));
    private static Query<TGeometry, TOut> CurveQuery<TGeometry, TOut, TValue>(Curves selector, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project, bool transfer = false) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: CurvesKey, query: Query<TGeometry, TValue>.Build(
            key: CurvesKey,
            state: (Selector: selector, Transfer: transfer, Project: project),
            evaluator: static (state, geometry) => ProjectCurves(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    private static Eff<Analyze.Runtime, Seq<TValue>> ProjectCurves<TGeometry, TValue>(TGeometry geometry, Curves selector, bool transfer, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from runtime in Analyze.RuntimeAsks
        from curves in ExtractCurveProjections(geometry: geometry, aspect: selector.Selector == CurveSelector.At ? Rasm.Analysis.Curves.All : selector, runtime: runtime).ToEff()
        from chosen in SelectCurves(curves: curves, aspect: selector).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, transfer: transfer, project: project, same: static (left, right) => ReferenceEquals(objA: left.Curve, objB: right.Curve), dispose: static curve => curve.Dispose()).ToEff()
        select result;
    private static Fin<Seq<CurveProjection>> ExtractCurveProjections<TGeometry>(TGeometry geometry, Curves aspect, Analyze.Runtime runtime) where TGeometry : notnull =>
        (aspect.Selector, geometry) switch {
            (CurveSelector selector, _) when EdgeCurveCase(selector: selector) is { } || selector == CurveSelector.SubCurves => CurvesOf(geometry: geometry, selector: selector),
            (CurveSelector selector, _) when selector == CurveSelector.OuterLoop => LoopCurves(geometry: geometry, feature: CurveFeature.OuterLoop, loopType: BrepLoopType.Outer),
            (CurveSelector selector, _) when selector == CurveSelector.InnerLoop => LoopCurves(geometry: geometry, feature: CurveFeature.InnerLoop, loopType: BrepLoopType.Inner),
            (CurveSelector selector, _) when selector == CurveSelector.IsoU => IsoCurves(geometry: geometry, direction: 0, feature: CurveFeature.IsoU),
            (CurveSelector selector, _) when selector == CurveSelector.IsoV => IsoCurves(geometry: geometry, direction: 1, feature: CurveFeature.IsoV),
            (CurveSelector selector, _) when selector == CurveSelector.Silhouette => SilhouetteCurves(geometry: geometry, direction: aspect.Direction.IfNone(static () => Vector3d.ZAxis), runtime: runtime),
            (CurveSelector selector, _) when selector == CurveSelector.Draft => DraftCurves(geometry: geometry, direction: aspect.Direction.IfNone(static () => Vector3d.ZAxis), angle: aspect.Angle.IfNone(static () => 0.0), runtime: runtime),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.InvalidInput()),
        };
    private static Fin<Seq<CurveProjection>> CurvesOf<TGeometry>(TGeometry geometry, CurveSelector selector) where TGeometry : notnull =>
        (selector, geometry) switch {
            (CurveSelector kind, Curve curve) when InputCurveCase(selector: kind) => ProjectCurve(curve: curve, selector: kind, pieceSource: ComponentIndexType.PolycurveSegment, splitInput: true),
            (CurveSelector kind, Line line) when line.IsValid && InputCurveCase(selector: kind) =>
                PrimitiveCurve(primitive: line, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (CurveSelector kind, Polyline polyline) when polyline.IsValid && InputCurveCase(selector: kind) =>
                PrimitiveCurve(primitive: polyline, selector: kind, pieceSource: ComponentIndexType.PolycurveSegment, convert: static value => value.ToPolylineCurve()),
            (CurveSelector kind, Circle circle) when circle.IsValid && InputCurveCase(selector: kind) =>
                PrimitiveCurve(primitive: circle, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (CurveSelector kind, Arc arc) when arc.IsValid && InputCurveCase(selector: kind) =>
                PrimitiveCurve(primitive: arc, selector: kind, pieceSource: ComponentIndexType.NoType, convert: static value => value.ToNurbsCurve()),
            (CurveSelector kind, Brep brep) when EdgeCurveCase(selector: kind) is { } edge => BrepEdgeCurves(brep: brep, feature: edge.Feature, predicate: edge.Brep),
            (CurveSelector kind, BrepFace face) when kind == CurveSelector.All || kind == CurveSelector.Boundary => Optional(face.DuplicateFace(duplicateMeshes: false))
                .ToFin(CurvesKey.InvalidResult())
                .Bind(duplicate => { using Brep disposable = duplicate; return BrepEdgeCurves(brep: disposable, feature: CurveFeature.Boundary, predicate: static _ => true); }),
            (CurveSelector kind, Surface surface) when kind == CurveSelector.All || kind == CurveSelector.Boundary => SurfaceBoundaryCurves(surface: surface, feature: CurveFeature.Boundary),
            (CurveSelector kind, SubD subd) when kind == CurveSelector.All || kind == CurveSelector.Segments =>
                subd.UpdateSurfaceMeshCache(lazyUpdate: true) switch {
                    _ => IndexedCurves(curves: subd.DuplicateEdgeCurves(), feature: kind == CurveSelector.Segments ? CurveFeature.Segment : CurveFeature.Edge, sourceType: ComponentIndexType.SubdEdge),
                },
            (CurveSelector kind, Mesh) when kind == CurveSelector.NakedInner => Fin.Succ(Seq<CurveProjection>()),
            (CurveSelector kind, Mesh mesh) when kind != CurveSelector.NakedInner && EdgeCurveCase(selector: kind) is { } edge => MeshEdgeCurves(mesh: mesh, feature: edge.Feature, predicate: edge.Mesh),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    private static Fin<Seq<CurveProjection>> PrimitiveCurve<TPrimitive>(TPrimitive primitive, CurveSelector selector, ComponentIndexType pieceSource, Func<TPrimitive, Curve?> convert) =>
        Optional(convert(arg: primitive)).ToFin(CurvesKey.InvalidResult()).Bind(curve => { using Curve disposable = curve; return ProjectCurve(curve: disposable, selector: selector, pieceSource: pieceSource, splitInput: false); });
    private static Fin<Seq<CurveProjection>> IsoCurves<TGeometry>(TGeometry geometry, int direction, CurveFeature feature) where TGeometry : notnull =>
        geometry switch {
            Brep brep => toSeq(brep.Faces)
                .TraverseM(face => MidIsoCurve(surface: face, direction: direction, feature: feature, source: new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))
                .As()
                .Map(static nested => nested.Bind(static curves => curves)),
            Surface surface => MidIsoCurve(surface: surface, direction: direction, feature: feature, source: new ComponentIndex(type: ComponentIndexType.NoType, index: 0)),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
        };
    private static Fin<Seq<CurveProjection>> ProjectCurve(Curve? curve, CurveSelector selector, ComponentIndexType pieceSource, bool splitInput) =>
        Optional(curve).ToFin(CurvesKey.InvalidResult()).Bind(value => (selector, splitInput) switch {
            (CurveSelector kind, true) when InputBoundaryCase(selector: kind) => CurvePieces(curve: value, feature: CurveFeature.Input, pieceSource: ComponentIndexType.NoType, fallbackSource: ComponentIndexType.NoType, project: static candidate => candidate.DuplicateSegments()),
            (CurveSelector kind, _) when InputBoundaryCase(selector: kind) => OneCurve(curve: value, feature: CurveFeature.Input, type: ComponentIndexType.NoType),
            (CurveSelector kind, _) when kind == CurveSelector.Segments => CurvePieces(curve: value, feature: CurveFeature.Segment, pieceSource: pieceSource, fallbackSource: pieceSource, project: static candidate => candidate.DuplicateSegments()),
            (CurveSelector kind, _) when kind == CurveSelector.SubCurves => CurvePieces(curve: value, feature: CurveFeature.SubCurve, pieceSource: pieceSource, fallbackSource: ComponentIndexType.NoType, project: static candidate => candidate.GetSubCurves()),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.InvalidInput()),
        });
    private static bool InputCurveCase(CurveSelector selector) => InputBoundaryCase(selector: selector) || selector == CurveSelector.Segments || selector == CurveSelector.SubCurves;
    private static bool InputBoundaryCase(CurveSelector selector) => selector == CurveSelector.All || selector == CurveSelector.Boundary;
    private static Fin<Seq<CurveProjection>> CurvePieces(Curve curve, CurveFeature feature, ComponentIndexType pieceSource, ComponentIndexType fallbackSource, Func<Curve, Curve[]?> project) =>
        project(arg: curve) switch {
            Curve[] pieces when pieces.Length > 0 => IndexedCurves(curves: pieces, feature: feature, sourceType: pieceSource),
            _ => Optional(curve.DuplicateCurve())
                .ToFin(CurvesKey.InvalidResult())
                .Map(duplicate => Seq(new CurveProjection(curve: duplicate, feature: feature, type: fallbackSource, index: 0))),
        };
    private static Fin<Seq<CurveProjection>> SurfaceBoundaryCurves(Surface surface, CurveFeature feature) =>
        Seq(IsoStatus.South, IsoStatus.East, IsoStatus.North, IsoStatus.West)
            .TraverseM(iso => Optional(surface.IsoCurve(iso: iso)).ToFin(CurvesKey.InvalidResult()))
            .As()
            .Map(curves => curves.Map((curve, index) => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.NoType, index: index)));
    private static Fin<Seq<Curve>> IsoCurveValues(Surface surface, IsoStatus iso, double normalized, Op key) =>
        iso switch {
            IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North =>
                Optional(surface.IsoCurve(iso: iso))
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
    private static Fin<Seq<CurveProjection>> MidIsoCurve(Surface surface, int direction, CurveFeature feature, ComponentIndex source) =>
        IsoCurveValues(
                surface: surface,
                iso: direction switch { 0 => IsoStatus.X, _ => IsoStatus.Y },
                normalized: 0.5,
                key: CurvesKey)
            .Map(curves => curves.Map(curve => new CurveProjection(curve: curve, feature: feature, source: source)));
    private static Fin<Seq<CurveProjection>> OneCurve(Curve? curve, CurveFeature feature, ComponentIndexType type) =>
        Optional(curve)
            .Bind(static value => Optional(value.DuplicateCurve()))
            .ToFin(CurvesKey.InvalidResult())
            .Map(duplicate => Seq(new CurveProjection(curve: duplicate, feature: feature, type: type, index: 0)));
    private static Fin<Seq<CurveProjection>> IndexedCurves(IEnumerable<Curve>? curves, CurveFeature feature, ComponentIndexType sourceType) =>
        Optional(curves).ToFin(CurvesKey.InvalidResult()).Map(values => toSeq(values.Select((curve, index) => Optional(curve)
                .Map(value => new CurveProjection(curve: value, feature: feature, type: sourceType, index: index))))
            .Bind(static projection => projection.ToSeq()));
    private static Fin<Seq<CurveProjection>> BrepEdgeCurves(Brep brep, CurveFeature feature, Func<BrepEdge, bool> predicate) =>
        Fin.Succ(toSeq(brep.Edges)
            .Where(edge => predicate(arg: edge))
            .Bind(edge => Optional(edge.DuplicateCurve())
                .Map(curve => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.BrepEdge, index: edge.EdgeIndex))
                .ToSeq()));
    private static Fin<Seq<CurveProjection>> MeshEdgeCurves(Mesh mesh, CurveFeature feature, Func<Mesh, int, bool> predicate) =>
        Fin.Succ(toSeq(Enumerable.Range(start: 0, count: mesh.TopologyEdges.Count))
            .Where(index => predicate(arg1: mesh, arg2: index))
            .Map(index => new CurveProjection(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: index).ToNurbsCurve(), feature: feature, type: ComponentIndexType.MeshTopologyEdge, index: index)));
    private static Fin<Seq<CurveProjection>> LoopCurves<TGeometry>(TGeometry geometry, CurveFeature feature, BrepLoopType loopType) where TGeometry : notnull =>
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
    private static Fin<Seq<CurveProjection>> SilhouetteCurves<TGeometry>(TGeometry geometry, Vector3d direction, Analyze.Runtime runtime) where TGeometry : notnull =>
        SilhouetteProjections(
            geometry: geometry,
            state: (Direction: direction, Runtime: runtime),
            feature: CurveFeature.Silhouette,
            valid: static state => state.Direction.IsValid && !state.Direction.IsTiny(),
            project: static (native, state) => Silhouette.Compute(
                geometry: native,
                silhouetteType: SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary,
                parallelCameraDirection: state.Direction,
                tolerance: state.Runtime.Context.Absolute.Value,
                angleToleranceRadians: state.Runtime.Context.Angle.Value,
                clippingPlanes: null!,
                cancelToken: state.Runtime.Cancellation));
    private static Fin<Seq<CurveProjection>> DraftCurves<TGeometry>(TGeometry geometry, Vector3d direction, double angle, Analyze.Runtime runtime) where TGeometry : notnull =>
        SilhouetteProjections(
            geometry: geometry,
            state: (Direction: direction, Angle: angle, Runtime: runtime),
            feature: CurveFeature.Draft,
            valid: static state => state.Direction.IsValid && !state.Direction.IsTiny() && RhinoMath.IsValidDouble(x: state.Angle),
            project: static (native, state) => Silhouette.ComputeDraftCurve(
                geometry: native,
                draftAngle: state.Angle,
                pullDirection: state.Direction,
                tolerance: state.Runtime.Context.Absolute.Value,
                angleToleranceRadians: state.Runtime.Context.Angle.Value,
                cancelToken: state.Runtime.Cancellation));
    private static Fin<Seq<CurveProjection>> SilhouetteProjections<TGeometry, TState>(TGeometry geometry, TState state, CurveFeature feature, Func<TState, bool> valid, Func<GeometryBase, TState, Silhouette[]?> project) where TGeometry : notnull =>
        (geometry, valid(arg: state)) switch {
            (GeometryBase native, true) => Optional(project(arg1: native, arg2: state))
                .ToFin(CurvesKey.InvalidResult())
                .Map(values => toSeq(values).Map(silhouette => new CurveProjection(curve: silhouette.Curve, feature: feature, source: silhouette.GeometryComponentIndex))),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
        };
    private static (CurveFeature Feature, Func<BrepEdge, bool> Brep, Func<Mesh, int, bool> Mesh)? EdgeCurveCase(CurveSelector selector) =>
        selector switch {
            CurveSelector kind when kind == CurveSelector.All => (CurveFeature.Edge, static _ => true, static (_, _) => true),
            CurveSelector kind when kind == CurveSelector.Segments => (CurveFeature.Segment, static _ => true, static (_, _) => true),
            CurveSelector kind when kind == CurveSelector.Boundary => (CurveFeature.Boundary, BrepNakedEdge(nakedOuter: true, nakedInner: true), static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 1),
            CurveSelector kind when kind == CurveSelector.NakedOuter => (CurveFeature.NakedOuter, BrepNakedEdge(nakedOuter: true, nakedInner: false), static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 1),
            CurveSelector kind when kind == CurveSelector.NakedInner => (CurveFeature.NakedInner, BrepNakedEdge(nakedOuter: false, nakedInner: true), static (_, _) => false),
            CurveSelector kind when kind == CurveSelector.Interior => (CurveFeature.Interior, static edge => edge.Valence == EdgeAdjacency.Interior, static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 2),
            CurveSelector kind when kind == CurveSelector.NonManifold => (CurveFeature.NonManifold, static edge => edge.Valence == EdgeAdjacency.NonManifold, static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
            _ => null,
        };
    private static Func<BrepEdge, bool> BrepNakedEdge(bool nakedOuter, bool nakedInner) =>
        edge => edge.Valence == EdgeAdjacency.Naked
            && toSeq(edge.TrimIndices()).Exists(trim => edge.Brep.Trims[trim].Loop.LoopType switch {
                BrepLoopType.Outer => nakedOuter,
                BrepLoopType.Inner => nakedInner,
                _ => false,
            });
    private static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
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
    private static Fin<Seq<CurveProjection>> SelectCurves(Seq<CurveProjection> curves, Curves aspect) =>
        (aspect.Selector, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<CurveProjection>()),
            (CurveSelector selector, int count) when selector == CurveSelector.At => Fin.Succ(Seq(curves[RhinoMath.Clamp(aspect.Index.IfNone(static () => 0), 0, count - 1)])),
            _ => Fin.Succ(curves),
        };
    internal static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) =>
        FaceCentroid(face: face, runtime: runtime)
            .Bind(centroid => {
                BrepFace brepFace = face.Brep.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: double.MaxValue) switch {
                    true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                        (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() =>
                            Fin.Succ((frame.ZAxis * (face.Reversed ? -normal : normal)) switch {
                                >= 0.0 => frame,
                                _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                            }),
                        _ => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(FacesKey.InvalidResult()),
                };
            });
    internal static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) =>
        Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    internal static Fin<Seq<Interval>> FaceDomains(FaceProjection face) =>
        (face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
            _ => Fin.Fail<Seq<Interval>>(FacesKey.InvalidResult()),
        };
    private static Fin<Seq<FaceProjection>> DecomposeFaces<TGeometry>(TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Brep brep => Fin.Succ(BrepFaceProjections(brep: brep)),
            BrepFace face => Fin.Succ(Seq(FaceProjection.From(face: face))),
            GeometryBase native when native is not Mesh && native.HasBrepForm => Optional(Brep.TryConvertBrep(geometry: native))
                .ToFin(FacesKey.InvalidResult())
                .Map(static brep => { using Brep disposable = brep; return BrepFaceProjections(brep: disposable); }),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Brep))),
        };
    private static Seq<FaceProjection> BrepFaceProjections(Brep brep) => toSeq(brep.Faces.Select(static face => FaceProjection.From(face: face)));
    private static Fin<Seq<FaceProjection>> SelectFaces(Seq<FaceProjection> faces, Faces selector, Context runtime) =>
        (selector.Selector, faces.Count) switch {
            (_, 0) => Fin.Succ(Seq<FaceProjection>()),
            (FaceSelector all, _) when all == FaceSelector.All => Fin.Succ(faces),
            (FaceSelector top, _) when top == FaceSelector.Top => RankByCentroidZ(faces: faces, descending: true, runtime: runtime),
            (FaceSelector bottom, _) when bottom == FaceSelector.Bottom => RankByCentroidZ(faces: faces, descending: false, runtime: runtime),
            (FaceSelector at, int count) when at == FaceSelector.At => Fin.Succ(Seq(faces[RhinoMath.Clamp(selector.Index.IfNone(static () => 0), 0, count - 1)])),
            _ => Fin.Fail<Seq<FaceProjection>>(FacesKey.InvalidInput()),
        };
    private static Fin<Seq<FaceProjection>> RankByCentroidZ(Seq<FaceProjection> faces, bool descending, Context runtime) =>
        faces
            .Traverse(face => FaceCentroid(face: face, runtime: runtime).Map(point => (face, point.Z))).As()
            .Map(ranked => (ranked.IsEmpty, descending) switch {
                (true, _) => Seq<FaceProjection>(),
                (false, true) => ranked.Maxima(projection: static item => item.Z, tolerance: runtime.Absolute.Value).Map(static item => item.face),
                (false, false) => ranked.Minima(projection: static item => item.Z, tolerance: runtime.Absolute.Value).Map(static item => item.face),
            });
}

using System.Collections.Frozen;

namespace Rasm.Analysis;

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct FaceProjection : ITopologyProjection {
    private FaceProjection(Brep brep, int faceIndex, bool reversed) { Brep = brep; FaceIndex = faceIndex; Reversed = reversed; }
    public Brep Brep { get; }
    public int FaceIndex { get; }
    public bool Reversed { get; }
    public ComponentIndex Source => new(type: ComponentIndexType.BrepFace, index: FaceIndex);
    public static FaceProjection From(BrepFace face) {
        ArgumentNullException.ThrowIfNull(argument: face);
        return new(brep: face.DuplicateFace(duplicateMeshes: false), faceIndex: face.FaceIndex, reversed: face.OrientationIsReversed);
    }
    public Unit Dispose() => fun(static (Brep brep) => { brep.Dispose(); return Unit.Default; })(Brep);
    public bool SameAs(ITopologyProjection other) => other is FaceProjection f && ReferenceEquals(objA: Brep, objB: f.Brep);
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceProjection(Mesh Mesh, int Face) : ITopologyProjection {
    public ComponentIndex Source => new(type: ComponentIndexType.MeshFace, index: Face);
    public Unit Dispose() => Unit.Default;
    public bool SameAs(ITopologyProjection other) => other is MeshFaceProjection m && ReferenceEquals(objA: Mesh, objB: m.Mesh) && Face == m.Face;
    public Vector3d Normal => MeshFaceMetrics.ComputeFaceNormal(mesh: Mesh, face: Face);
    public Seq<Point3d> Vertices => MeshFaceMetrics.FaceVertices(mesh: Mesh, face: Face);
    public Point3d Center => Vertices switch {
        Seq<Point3d> v when v.Count > 0 => (Point3d)(v.Fold(Vector3d.Zero, static (acc, p) => acc + (Vector3d)p) / v.Count),
        _ => Point3d.Unset,
    };
    public Mesh Isolated() {
        Mesh result = new();
        _ = Vertices.Iter(v => result.Vertices.Add(vertex: v));
        _ = Mesh.Faces[Face].IsQuad
            ? result.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2, vertex4: 3)
            : result.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2);
        result.RebuildNormals();
        return result;
    }
}
internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: +1);
    internal static Seq<TItem> Minima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: -1);
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, int direction) =>
        items.Fold(
            initialState: (Best: direction > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction),
            f: static (state, item) => state.Projection(arg: item) switch {
                double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
                double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
                _ => state,
            }).Hits.Rev();
}

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
                    Brep brep => BrepLeaves(brep: brep, key: EdgeMidpointsKey, primitiveFault: static (key, label) => key.PrimitiveRejected(primitive: label, reason: "no edges"), project: static (validated, context) => EdgeCurveMidpoints(curves: validated.DuplicateEdgeCurves(), context: context)),
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
        (typeof(TOut).AsKind(), Coercion.Supports(geometryType: typeof(TGeometry), targetType: typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => Query<TGeometry, TOut>.Build(
                key: PrimitiveKey,
                requirement: Requirement.Basic,
                requiresContext: true,
                state: someKind.IfNone(() => Rasm.Domain.Kind.Surface),
                evaluator: static (k, geometry) =>
                    from context in Analyze.Asks
                    from coerced in k.Coerce<TOut>(value: geometry, ctx: context, op: PrimitiveKey).ToEff()
                    from result in PrimitiveKey.One(value: coerced).ToEff()
                    select result),
            _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when Supports(geometry: geometry, output: output, target: typeof(GeometryKind), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box), typeof(Sphere)]) => Cast<TGeometry, TOut>(key: KindKey, query: Query<TGeometry, GeometryKind>.Build(
                key: KindKey,
                requiresContext: true,
                evaluator: static geometry => from context in Analyze.Asks
                                              from kind in geometry.Kind(ctx: context).ToEff()
                                              from result in One(key: KindKey, value: (kind.Primitive, kind.Topology) switch {
                                                  (Rasm.Domain.Primitive.BoundingBox, _) => GeometryKind.BoundingBox,
                                                  (Rasm.Domain.Primitive.Box, _) when kind.Type == typeof(Rhino.Geometry.Box) => GeometryKind.Box,
                                                  (Rasm.Domain.Primitive.Box, Rasm.Domain.Topology.Brep) => GeometryKind.BrepBox,
                                                  (Rasm.Domain.Primitive.Sphere, Rasm.Domain.Topology.Brep) => GeometryKind.BrepSphere,
                                                  (Rasm.Domain.Primitive.Cylinder, Rasm.Domain.Topology.Brep) => GeometryKind.BrepCylinder,
                                                  (Rasm.Domain.Primitive.Cone, Rasm.Domain.Topology.Brep) => GeometryKind.BrepCone,
                                                  (Rasm.Domain.Primitive.Torus, Rasm.Domain.Topology.Brep) => GeometryKind.BrepTorus,
                                                  (Rasm.Domain.Primitive.Plane, Rasm.Domain.Topology.Brep) => GeometryKind.BrepPlane,
                                                  (_, Rasm.Domain.Topology.Brep) => GeometryKind.BrepGeneral,
                                                  (Rasm.Domain.Primitive.Plane, Rasm.Domain.Topology.Surface) => GeometryKind.Plane,
                                                  (Rasm.Domain.Primitive.Sphere, Rasm.Domain.Topology.Surface) => GeometryKind.Sphere,
                                                  (Rasm.Domain.Primitive.Cylinder, Rasm.Domain.Topology.Surface) => GeometryKind.Cylinder,
                                                  (Rasm.Domain.Primitive.Cone, Rasm.Domain.Topology.Surface) => GeometryKind.Cone,
                                                  (Rasm.Domain.Primitive.Torus, Rasm.Domain.Topology.Surface) => GeometryKind.Torus,
                                                  (_, Rasm.Domain.Topology.Surface) => GeometryKind.Surface,
                                                  (Rasm.Domain.Primitive.Line, Rasm.Domain.Topology.Curve) => GeometryKind.Line,
                                                  (Rasm.Domain.Primitive.Polyline, Rasm.Domain.Topology.Curve) => GeometryKind.Polyline,
                                                  (_, Rasm.Domain.Topology.Curve) => GeometryKind.Curve,
                                                  (_, Rasm.Domain.Topology.Mesh) => GeometryKind.Mesh,
                                                  (_, Rasm.Domain.Topology.SubD) => GeometryKind.SubD,
                                                  _ => GeometryKind.Unknown,
                                              }).ToEff()
                                              select result)),
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
                        brep: brep, key: VerticesKey, primitiveFault: static (key, label) => key.PrimitiveRejected(primitive: label, reason: "no vertices"), project: static (validated, _) => Many(key: VerticesKey, values: validated.DuplicateVertices())),
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
    internal static Eff<Analyze.Env, Seq<TOut>> BrepLeaves<TOut>(Brep brep, Op key, Func<Op, string, Error> primitiveFault, Func<Brep, Context, Fin<Seq<TOut>>> project) =>
        from context in Analyze.Asks
        from validated in context.Validate(geometry: brep, requirement: Requirement.Basic).ToEff()
        from kind in validated.Kind(ctx: context).ToEff()
        from result in ((kind.Topology, kind.Primitive) switch {
            (Rasm.Domain.Topology.Brep, Rasm.Domain.Primitive.Sphere) => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Sphere")),
            (Rasm.Domain.Topology.Brep, Rasm.Domain.Primitive.Cylinder) => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cylinder")),
            (Rasm.Domain.Topology.Brep, Rasm.Domain.Primitive.Cone) => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Cone")),
            (Rasm.Domain.Topology.Brep, Rasm.Domain.Primitive.Torus) => Fin.Fail<Seq<TOut>>(primitiveFault(arg1: key, arg2: "Torus")),
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
    internal static Fin<Seq<MeshCheckParameters>> MeshCheckParametersFor(Mesh geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        _ = geometry.Check(textLog: textLog, parameters: ref parameters);
        return One(key: MeshCheckKey, value: parameters);
    }
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) => count switch {
        Rasm.Analysis.MeshCheckCount.None => Query<Mesh, int>.Reject(key: MeshCheckCountKey, fault: MeshCheckCountKey.InvalidInput()),
        _ => Query<Mesh, int>.Build(
            key: MeshCheckCountKey,
            state: count,
            evaluator: static (metric, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                    from head in parameters.Head.ToFin(MeshCheckCountKey.InvalidResult()).ToEff()
                                                    from result in One(key: MeshCheckCountKey, value: metric.Get(parameters: head)).ToEff()
                                                    select result),
    };
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) => metric switch {
        Rasm.Analysis.MeshFaceMetric.None => Query<Mesh, MeshFaceSample>.Reject(key: MeshFaceMetricKey, fault: MeshFaceMetricKey.InvalidInput()),
        _ => Query<Mesh, MeshFaceSample>.Build(
            key: MeshFaceMetricKey, state: metric, requirement: Requirement.MeshCheck,
            evaluator: static (faceMetric, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                .TraverseM(face => faceMetric.Sample(mesh: geometry, face: face)
                    .Filter(v => RhinoMath.IsValidDouble(x: v) && v >= 0.0)
                    .ToFin(MeshFaceMetricKey.InvalidResult())
                    .Map(v => new MeshFaceSample(Face: face, Value: v))).As().ToEff()),
    };
    public static Query<Mesh, bool> MeshValidityBundle =>
        Query<Mesh, bool>.Build(key: MeshValidityBundleKey, evaluator: static geometry => {
            bool manifold = geometry.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool boundary);
            return Many(key: MeshValidityBundleKey, values: new[] { geometry.IsValid, geometry.IsClosed, oriented, geometry.IsSolid, manifold, boundary }).ToEff();
        });
    public static Query<Mesh, int> MeshStatsBundle =>
        Query<Mesh, int>.Build(
            key: MeshStatsBundleKey,
            evaluator: static geometry => Many(key: MeshStatsBundleKey, values: new[] {
                geometry.Vertices.Count, geometry.Faces.Count, geometry.Faces.TriangleCount, geometry.Faces.QuadCount,
                geometry.TopologyEdges.Count, geometry.Vertices.Count - geometry.TopologyEdges.Count + geometry.Faces.Count,
            }).ToEff());
    public static Query<Mesh, int> MeshDefectsBundle =>
        Query<Mesh, int>.Build(
            key: MeshDefectsBundleKey,
            evaluator: static geometry => from parameters in MeshCheck.Apply(geometry: geometry)
                                          from head in parameters.Head.ToFin(MeshDefectsBundleKey.InvalidResult()).ToEff()
                                          from result in Many(key: MeshDefectsBundleKey, values: MeshCheckCounts.Defects.Select(m => m.Get(parameters: head))).ToEff()
                                          select result);
    public static Query<Mesh, MeshFaceProjection> MeshAtFace(int? index = null) =>
        Query<Mesh, MeshFaceProjection>.Build(
            key: MeshAtFaceKey, state: index,
            evaluator: static (selector, geometry) => geometry.Faces.Count switch {
                0 => Fin.Fail<Seq<MeshFaceProjection>>(MeshAtFaceKey.InvalidResult()).ToEff(),
                int count => One(key: MeshAtFaceKey, value: new MeshFaceProjection(Mesh: geometry, Face: RhinoMath.Clamp(selector ?? 0, 0, count - 1))).ToEff(),
            });
    public static Query<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull =>
        aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: MeshesKey, fault: MeshesKey.InvalidInput());
    public static Query<Mesh, Polyline> SelfIntersections =>
        Query<Mesh, Polyline>.Build(
            key: SelfIntersectionsKey,
            requirement: Requirement.Basic,
            evaluator: static geometry => from runtime in Analyze.EnvAsks
                                          from result in SelfIntersectionsValue(geometry: geometry, runtime: runtime).ToEff()
                                          select result);
    // Mesh.GetSelfIntersections requires by-ref/out parameters and a TextLog using-local; the
    // CleanupFinally exemption permits the using-block at this GeometryBase boundary adapter.
    internal static Fin<Seq<Polyline>> SelfIntersectionsValue(Mesh geometry, Analyze.Env runtime) {
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
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(SelfIntersectionsKey.InvalidResult()),
            };
    }
    internal static Fin<Seq<Point3d>> EdgeCurveMidpoints(IEnumerable<Curve>? curves, Context context) => Optional(curves).ToFin(EdgeMidpointsKey.InvalidResult()).Bind(source => toSeq(source)
            .TraverseM(curve => {
                using Curve disposable = curve;  // BOUNDARY ADAPTER -- DisposableCurve
                return CurveAtNormalizedValue(curve: disposable, context: context, key: EdgeMidpointsKey, project: static (c, parameter) => c.PointAt(t: parameter));
            }).As()).Bind(static points => Many(key: EdgeMidpointsKey, values: points));
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull =>
        aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: FacesKey, fault: FacesKey.InvalidInput());
    internal static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(Faces selector, Requirement requirement, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: FacesKey, query: Query<TGeometry, TValue>.Build(
            key: FacesKey, state: (Selector: selector, Transfer: transfer, Project: project), requirement: requirement,
            evaluator: static (state, geometry) => ProjectFaces(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    public static Eff<Analyze.Env, Seq<FaceProjection>> FaceProjections(object geometry, Faces selector) =>
        ProjectFaces(geometry: geometry, selector: selector, transfer: true, project: static (values, _) => Fin.Succ(values));
    internal static Eff<Analyze.Env, Seq<TValue>> ProjectFaces<TGeometry, TValue>(TGeometry geometry, Faces selector, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from context in Analyze.Asks
        from faces in DecomposeFaces(geometry: geometry).ToEff()
        from chosen in SelectFaces(faces: faces, selector: selector, runtime: context).ToEff()
        from result in ProjectOwned(all: faces, chosen: chosen, transfer: transfer, project: values => project(arg1: values, arg2: context)).ToEff()
        select result;
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull =>
        aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: CurvesKey, fault: CurvesKey.InvalidInput());
    public static Eff<Analyze.Env, Seq<CurveProjection>> CurveProjections(object geometry, Curves aspect) =>
        ProjectCurves(geometry: geometry, selector: aspect, transfer: true, project: static values => Fin.Succ(values));
    internal static Query<TGeometry, TOut> CurveQuery<TGeometry, TOut, TValue>(Curves selector, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project, bool transfer = false) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: CurvesKey, query: Query<TGeometry, TValue>.Build(
            key: CurvesKey,
            state: (Selector: selector, Transfer: transfer, Project: project),
            evaluator: static (state, geometry) => ProjectCurves(geometry: geometry, selector: state.Selector, transfer: state.Transfer, project: state.Project)));
    internal static Eff<Analyze.Env, Seq<TValue>> ProjectCurves<TGeometry, TValue>(TGeometry geometry, Curves selector, bool transfer, Func<Seq<CurveProjection>, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        from runtime in Analyze.EnvAsks
        from curves in ExtractCurveProjections(geometry: geometry, aspect: selector is Rasm.Analysis.Curves.AtCase ? Rasm.Analysis.Curves.All : selector, runtime: runtime).ToEff()
        from chosen in SelectCurves(curves: curves, aspect: selector).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, transfer: transfer, project: project).ToEff()
        select result;
    internal static Fin<Seq<CurveProjection>> ExtractCurveProjections<TGeometry>(TGeometry geometry, Curves aspect, Analyze.Env runtime) where TGeometry : notnull =>
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
    private static readonly FrozenDictionary<Type, (ComponentIndexType PieceSource, Func<object, Curve?> Convert, Func<object, bool> IsValid)> PrimitiveCurves =
        new Dictionary<Type, (ComponentIndexType, Func<object, Curve?>, Func<object, bool>)> {
            [typeof(Line)] = (ComponentIndexType.NoType, static o => ((Line)o).ToNurbsCurve(), static o => ((Line)o).IsValid),
            [typeof(Polyline)] = (ComponentIndexType.PolycurveSegment, static o => ((Polyline)o).ToPolylineCurve(), static o => ((Polyline)o).IsValid),
            [typeof(Circle)] = (ComponentIndexType.NoType, static o => ((Circle)o).ToNurbsCurve(), static o => ((Circle)o).IsValid),
            [typeof(Arc)] = (ComponentIndexType.NoType, static o => ((Arc)o).ToNurbsCurve(), static o => ((Arc)o).IsValid),
        }.ToFrozenDictionary();
    internal static Fin<Seq<CurveProjection>> CurvesOf<TGeometry>(TGeometry geometry, Curves selector) where TGeometry : notnull =>
        (selector, geometry) switch {
            (Curves kind, Curve curve) when kind.InputCurve => ProjectCurve(curve: curve, selector: kind, pieceSource: ComponentIndexType.PolycurveSegment, splitInput: true),
            (Curves kind, object primitive) when kind.InputCurve && PrimitiveCurves.TryGetValue(key: primitive.GetType(), value: out (ComponentIndexType PieceSource, Func<object, Curve?> Convert, Func<object, bool> IsValid) entry) && entry.IsValid(arg: primitive) =>
                Optional(entry.Convert(arg: primitive)).ToFin(CurvesKey.InvalidResult()).Bind(curve => { using Curve d = curve; return ProjectCurve(curve: d, selector: kind, pieceSource: entry.PieceSource, splitInput: false); }),
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
    internal static Fin<Seq<CurveProjection>> IsoCurves<TGeometry>(TGeometry geometry, int direction, CurveFeature feature) where TGeometry : notnull =>
        geometry switch {
            Brep brep => toSeq(brep.Faces)
                .TraverseM(face => IsoCurveValues(surface: face, iso: direction == 0 ? IsoStatus.X : IsoStatus.Y, normalized: 0.5, key: CurvesKey)
                    .Map(curves => curves.Map(curve => new CurveProjection(Curve: curve, Feature: feature, Source: new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))))
                .As()
                .Map(static nested => nested.Bind(static curves => curves)),
            Surface surface => IsoCurveValues(surface: surface, iso: direction == 0 ? IsoStatus.X : IsoStatus.Y, normalized: 0.5, key: CurvesKey)
                .Map(curves => curves.Map(curve => new CurveProjection(Curve: curve, Feature: feature, Source: new ComponentIndex(type: ComponentIndexType.NoType, index: 0)))),
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
            .TraverseM(iso => Optional(surface.IsoCurve(iso: iso)).ToFin(CurvesKey.InvalidResult())).As()
            .Map(curves => curves.Map((curve, index) => new CurveProjection(curve: curve, feature: feature, type: ComponentIndexType.NoType, index: index)));
    internal static Fin<Seq<Curve>> IsoCurveValues(Surface surface, IsoStatus iso, double normalized, Op key) =>
        (iso, normalized is >= 0.0 and <= 1.0) switch {
            (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) =>
                Optional(surface.IsoCurve(iso: iso)).ToFin(key.InvalidResult()).Map(static c => Seq(c)),
            (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(direction: iso == IsoStatus.X ? 0 : 1) is { IsValid: true } domain =>
                surface switch {
                    BrepFace face => Many(key: key, values: face.TrimAwareIsoCurve(direction: iso == IsoStatus.X ? 0 : 1, constantParameter: domain.ParameterAt(normalizedParameter: normalized))),
                    _ => Optional(surface.IsoCurve(iso, domain.ParameterAt(normalizedParameter: normalized))).ToFin(key.InvalidResult()).Map(static c => Seq(c)),
                },
            _ => Fin.Fail<Seq<Curve>>(key.InvalidInput()),
        };
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
    internal static Fin<Seq<CurveProjection>> SilhouetteCurves<TGeometry>(TGeometry geometry, Vector3d direction, Analyze.Env runtime) where TGeometry : notnull =>
        SilhouetteProjections(geometry: geometry, state: (Direction: direction, Runtime: runtime), feature: CurveFeature.Silhouette, valid: static state => state.Direction.IsValid && !state.Direction.IsTiny(), project: static (native, state) => Silhouette.Compute(geometry: native, silhouetteType: SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, parallelCameraDirection: state.Direction, tolerance: state.Runtime.Context.Absolute.Value, angleToleranceRadians: state.Runtime.Context.Angle.Value, clippingPlanes: [], cancelToken: state.Runtime.Cancellation));
    internal static Fin<Seq<CurveProjection>> DraftCurves<TGeometry>(TGeometry geometry, Vector3d direction, double angle, Analyze.Env runtime) where TGeometry : notnull =>
        SilhouetteProjections(geometry: geometry, state: (Direction: direction, Angle: angle, Runtime: runtime), feature: CurveFeature.Draft, valid: static state => state.Direction.IsValid && !state.Direction.IsTiny() && RhinoMath.IsValidDouble(x: state.Angle), project: static (native, state) => Silhouette.ComputeDraftCurve(geometry: native, draftAngle: state.Angle, pullDirection: state.Direction, tolerance: state.Runtime.Context.Absolute.Value, angleToleranceRadians: state.Runtime.Context.Angle.Value, cancelToken: state.Runtime.Cancellation));
    internal static Fin<Seq<CurveProjection>> SilhouetteProjections<TGeometry, TState>(TGeometry geometry, TState state, CurveFeature feature, Func<TState, bool> valid, Func<GeometryBase, TState, Silhouette[]?> project) where TGeometry : notnull =>
        (geometry, valid(arg: state)) switch {
            (GeometryBase native, true) => Optional(project(arg1: native, arg2: state))
                .ToFin(CurvesKey.InvalidResult())
                .Map(values => toSeq(values).Map(silhouette => new CurveProjection(Curve: silhouette.Curve, Feature: feature, Source: silhouette.GeometryComponentIndex))),
            _ => Fin.Fail<Seq<CurveProjection>>(CurvesKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
        };
    internal static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
        Seq<TProjection> all,
        Seq<TProjection> chosen,
        bool transfer,
        Func<Seq<TProjection>, Fin<Seq<TValue>>> project) where TProjection : ITopologyProjection {
        Fin<Seq<TValue>> result = project(arg: chosen);
        _ = all.Filter(value => !(transfer && result.IsSucc && chosen.Exists(c => c.SameAs(other: value)))).Iter(static v => v.Dispose());
        return result;
    }
    internal static Fin<Seq<CurveProjection>> SelectCurves(Seq<CurveProjection> curves, Curves aspect) =>
        (aspect, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<CurveProjection>()),
            (Rasm.Analysis.Curves.AtCase at, int count) => Fin.Succ(Seq(curves[RhinoMath.Clamp(at.Value ?? 0, 0, count - 1)])),
            _ => Fin.Succ(curves),
        };
    public static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) =>
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
    public static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) {
        ArgumentNullException.ThrowIfNull(argument: runtime);
        return Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(FacesKey.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    }
    public static Fin<Seq<Interval>> FaceDomains(FaceProjection face) =>
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

// --- [MESHES_ROLE] -----------------------------------------------------------------------
internal static class MeshesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Meshes selector) where TGeometry : notnull => selector switch {
        Rasm.Analysis.Meshes.ValidityBundleCase => Lift<TGeometry, TOut, bool>(key: Query.MeshValidityBundleKey, source: Query.MeshValidityBundle),
        Rasm.Analysis.Meshes.StatsBundleCase => Lift<TGeometry, TOut, int>(key: Query.MeshStatsBundleKey, source: Query.MeshStatsBundle),
        Rasm.Analysis.Meshes.DefectsBundleCase => Lift<TGeometry, TOut, int>(key: Query.MeshDefectsBundleKey, source: Query.MeshDefectsBundle),
        Rasm.Analysis.Meshes.FaceQualityCase fq => Lift<TGeometry, TOut, MeshFaceSample>(key: Query.MeshFaceMetricKey, source: Query.MeshFaceMetric(metric: fq.Metric)),
        Rasm.Analysis.Meshes.AtFaceCase at => Lift<TGeometry, TOut, MeshFaceProjection>(key: Query.MeshAtFaceKey, source: Query.MeshAtFace(index: at.Value)),
        _ => Query.MeshesKey.Unsupported<TGeometry, TOut>(),
    };
    private static Query<TGeometry, TOut> Lift<TGeometry, TOut, TValue>(Op key, Query<Mesh, TValue> source) where TGeometry : notnull =>
        Query.Native<TGeometry, TOut, Mesh, TValue, Query<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: mesh));
}

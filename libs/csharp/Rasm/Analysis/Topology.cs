namespace Rasm.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) => Extrema(items: items, projection: projection, tolerance: tolerance, direction: +1);
    internal static Seq<TItem> Minima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) => Extrema(items: items, projection: projection, tolerance: tolerance, direction: -1);
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, int direction) => items.Fold(
        initialState: (Best: direction > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction),
        f: static (state, item) => state.Projection(arg: item) switch {
            double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
            double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
            _ => state,
        }).Hits.Rev();
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static partial class Query {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) || typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Interval>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Analyze.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from domains in kind.Domains(value: geometry, op: op).ToEff()
                    from result in Many(key: op, values: domains).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Line) => Cast<TGeometry, TOut>(key: key, query: Query<Polyline, Line>.Build(key: key, state: key, evaluator: static (op, geometry) => Many(key: op, values: geometry.GetSegments()).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Native<TGeometry, TOut, Curve, Curve>(key: key, project: curve => Many(key: key, values: curve.DuplicateSegments()).ToEff()),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Query<Brep, Curve> Edges {
        get { Op key = Op.Of(); return Query<Brep, Curve>.Build(key: key, state: key, evaluator: static (op, geometry) => Many(key: op, values: geometry.DuplicateEdgeCurves()).ToEff()); }
    }
    public static Query<TGeometry, TOut> EdgeMidpoints<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Point3d) && Supports(geometry: typeof(TGeometry), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)])
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => One(key: op, value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => Many(key: op, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => Many(key: op, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Box box => Many(key: op, values: box.BoundingBox.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    _ => from context in Analyze.Asks
                         from kind in ((object)geometry).Kind(ctx: context).ToEff()
                         from curves in kind.Curves(value: geometry, selector: new CurveSelector(Feature: CurveFeature.Edge, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None), ctx: context, op: op).ToEff()
                         from result in Many(key: op, values: curves.Map(static projection => { using Curve c = projection.Curve; return c.PointAtNormalizedLength(length: 0.5); })).ToEff()
                         select result,
                }))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> NakedEdges<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Native<TGeometry, TOut, Brep, Curve>(key: key, project: brep => Many(key: key, values: brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true)).ToEff()),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) => Native<TGeometry, TOut, Mesh, Polyline>(key: key, project: mesh => Many(key: key, values: mesh.GetNakedEdges()).ToEff()),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Query<Mesh, Polyline> Outlines(Plane plane) {
        Op key = Op.Of();
        return Query<Mesh, Polyline>.Build(key: key, state: (Key: key, Plane: plane), evaluator: static (state, geometry) => Many(key: state.Key, values: geometry.GetOutlines(plane: state.Plane)).ToEff());
    }
    internal static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target && Supports(geometry: geometry, native: native);
    internal static bool Supports(Type geometry, params Type[] native) => typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object) || native.Contains(value: geometry);
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return Query<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Analyze.Asks
                from kind in ((object)geometry).Kind(ctx: context).ToEff()
                from curves in kind.IsoCurves(value: geometry, direction: state.Iso, normalized: state.Normalized, op: state.Key).ToEff()
                from result in Many(key: state.Key, values: curves).ToEff()
                select result);
    }
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut).AsKind(), Coercion.Supports(geometryType: typeof(TGeometry), targetType: typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => Query<TGeometry, TOut>.Build(
                key: key, requirement: Requirement.Basic, requiresContext: true,
                state: (Key: key, Kind: someKind.IfNone(() => Rasm.Domain.Kind.Surface)),
                evaluator: static (state, geometry) =>
                    from context in Analyze.Asks
                    from coerced in state.Kind.Coerce<TOut>(value: geometry, ctx: context, op: state.Key).ToEff()
                    from result in state.Key.One(value: coerced).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return Supports(geometry: typeof(TGeometry), output: typeof(TOut), target: typeof(Rasm.Domain.Kind), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box), typeof(Sphere)])
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Rasm.Domain.Kind>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Analyze.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from result in One(key: op, value: kind).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, Rasm.Domain.SolidOrientation> SolidOrientation<TGeometry>() where TGeometry : GeometryBase {
        Op key = Op.Of();
        return Query<TGeometry, Rasm.Domain.SolidOrientation>.Build(
            key: key, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Analyze.Asks
                from kind in ((object)geometry).Kind(ctx: context).ToEff()
                from orientation in kind.SolidOrientation(value: geometry, op: op).ToEff()
                from result in One(key: op, value: orientation).ToEff()
                select result);
    }
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase {
        Op key = Op.Of();
        return point.IsValid switch {
            false => Query<TGeometry, bool>.Reject(key: key, fault: key.InvalidInput()),
            true => Cast<TGeometry, bool>(key: key, query: Query<TGeometry, bool>.Build(
                key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, requiresContext: true,
                evaluator: static (state, geometry) =>
                    from context in Analyze.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from contained in kind.Contains(value: geometry, target: state.Target, ctx: context, op: state.Key).ToEff()
                    from result in One(key: state.Key, value: contained).ToEff()
                    select result)),
        };
    }
    public static Query<TGeometry, TOut> Vertices<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return Supports(geometry: typeof(TGeometry), output: typeof(TOut), target: typeof(Point3d), native: [typeof(Line), typeof(Polyline), typeof(Point3d), typeof(BoundingBox), typeof(Box)])
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Analyze.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from points in kind.Vertices(value: geometry, ctx: context, op: op).ToEff()
                    from result in Many(key: op, values: points).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Brep)) || (typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Mesh))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Analyze.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from components in kind.Components(value: geometry, ctx: context, op: op).ToEff()
                    from result in op.Results<GeometryBase, TOut>(values: components).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<Mesh, bool> IsManifold {
        get { Op key = Op.Of(); return Query<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => One(key: op, value: geometry.IsManifold()).ToEff()); }
    }
    public static Query<Mesh, bool> NakedPointStatus {
        get { Op key = Op.Of(); return Query<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => Many(key: op, values: geometry.GetNakedEdgePointStatus()).ToEff()); }
    }
    public static Query<Mesh, MeshCheckParameters> MeshCheck {
        get {
            Op key = Op.Of();
            return Query<Mesh, MeshCheckParameters>.Build(key: key, state: key, evaluator: static (op, geometry) => MeshCheckParametersFor(op: op, geometry: geometry).ToEff());
        }
    }
    private static Fin<Seq<MeshCheckParameters>> MeshCheckParametersFor(Op op, Mesh geometry) {
        // BOUNDARY ADAPTER — Rhino Check takes ref parameter and IDisposable TextLog.
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        _ = geometry.Check(textLog: textLog, parameters: ref parameters);
        return One(key: op, value: parameters);
    }
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) {
        Op key = Op.Of();
        return count switch {
            Rasm.Analysis.MeshCheckCount.None => Query<Mesh, int>.Reject(key: key, fault: key.InvalidInput()),
            _ => Query<Mesh, int>.Build(
                key: key, state: (Key: key, Count: count),
                evaluator: static (state, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                       from head in parameters.Head.ToFin(state.Key.InvalidResult()).ToEff()
                                                       from result in One(key: state.Key, value: state.Count.Get(parameters: head)).ToEff()
                                                       select result),
        };
    }
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) {
        Op key = Op.Of();
        return metric switch {
            Rasm.Analysis.MeshFaceMetric.None => Query<Mesh, MeshFaceSample>.Reject(key: key, fault: key.InvalidInput()),
            _ => Query<Mesh, MeshFaceSample>.Build(
                key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => state.Metric.Sample(mesh: geometry, face: face)
                        .Filter(v => RhinoMath.IsValidDouble(x: v) && v >= 0.0)
                        .ToFin(state.Key.InvalidResult())
                        .Map(v => new MeshFaceSample(Face: face, Value: v))).As().ToEff()),
        };
    }
    public static Query<Mesh, bool> MeshValidityBundle {
        get {
            Op key = Op.Of();
            return Query<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => {
                bool manifold = geometry.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool boundary);
                return Many(key: op, values: new[] { geometry.IsValid, geometry.IsClosed, oriented, geometry.IsSolid, manifold, boundary }).ToEff();
            });
        }
    }
    public static Query<Mesh, int> MeshStatsBundle {
        get {
            Op key = Op.Of();
            return Query<Mesh, int>.Build(
                key: key, state: key,
                evaluator: static (op, geometry) => Many(key: op, values: new[] {
                    geometry.Vertices.Count, geometry.Faces.Count, geometry.Faces.TriangleCount, geometry.Faces.QuadCount,
                    geometry.TopologyEdges.Count, geometry.Vertices.Count - geometry.TopologyEdges.Count + geometry.Faces.Count,
                }).ToEff());
        }
    }
    public static Query<Mesh, int> MeshDefectsBundle {
        get {
            Op key = Op.Of();
            return Query<Mesh, int>.Build(
                key: key, state: key,
                evaluator: static (op, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                    from head in parameters.Head.ToFin(op.InvalidResult()).ToEff()
                                                    from result in Many(key: op, values: MeshCheckCounts.Defects.Select(m => m.Get(parameters: head))).ToEff()
                                                    select result);
        }
    }
    public static Query<Mesh, MeshFaceProjection> MeshAtFace(int? index = null) {
        Op key = Op.Of();
        return Query<Mesh, MeshFaceProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => geometry.Faces.Count switch {
                0 => Fin.Fail<Seq<MeshFaceProjection>>(state.Key.InvalidResult()).ToEff(),
                int count => One(key: state.Key, value: new MeshFaceProjection(Mesh: geometry, Face: RhinoMath.Clamp(state.Selector ?? 0, 0, count - 1))).ToEff(),
            });
    }
    public static Query<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput());
    }
    public static Query<Mesh, Polyline> SelfIntersections {
        get {
            Op key = Op.Of();
            return Query<Mesh, Polyline>.Build(
                key: key, requirement: Requirement.Basic, state: key,
                evaluator: static (op, geometry) => from runtime in Analyze.EnvAsks
                                                    from result in SelfIntersectionsValue(op: op, geometry: geometry, runtime: runtime).ToEff()
                                                    select result);
        }
    }
    private static Fin<Seq<Polyline>> SelfIntersectionsValue(Op op, Mesh geometry, Env runtime) {
        // BOUNDARY ADAPTER — Rhino GetSelfIntersections takes IDisposable TextLog + multi-out.
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
                true => (Many(key: op, values: perforations), Many(key: op, values: overlaps))
                    .Apply((left, right) => left + right)
                    .As(),
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(op.InvalidResult()),
            };
    }
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput());
    }
    public static Eff<Env, Seq<FaceProjection>> FaceProjections(object geometry, Faces selector) {
        Op key = Op.Of(name: nameof(Faces));
        return from context in Analyze.Asks
               from faces in DecomposeFaces(key: key, ctx: context, geometry: geometry).ToEff()
               from chosen in SelectFaces(key: key, faces: faces, selector: selector, runtime: context).ToEff()
               select chosen;
    }
    internal static Fin<Seq<FaceProjection>> DecomposeFaces<TGeometry>(Op key, Context ctx, TGeometry geometry) where TGeometry : notnull =>
        ((object)geometry).Kind(ctx: ctx).Bind(kind => kind.Faces(value: geometry, ctx: ctx, op: key));
    internal static Fin<Seq<FaceProjection>> SelectFaces(Op key, Seq<FaceProjection> faces, Faces selector, Context runtime) => selector.Switch(
        state: (Key: key, Faces: faces, Runtime: runtime),
        allCase: static (s, _) => Fin.Succ(s.Faces),
        topCase: static (s, top) => RankFaces(state: s, axis: top.Axis, descending: true),
        bottomCase: static (s, bottom) => RankFaces(state: s, axis: bottom.Axis, descending: false),
        atCase: static (s, at) => s.Faces.Count switch { 0 => Fin.Succ(Seq<FaceProjection>()), int n => Fin.Succ(Seq(s.Faces[RhinoMath.Clamp(at.Value ?? 0, 0, n - 1)])) });
    private static Fin<Seq<FaceProjection>> RankFaces((Op Key, Seq<FaceProjection> Faces, Context Runtime) state, Vector3d axis, bool descending) =>
        (state.Faces.IsEmpty, axis.IsValid && !axis.IsTiny()) switch {
            (true, _) => Fin.Succ(Seq<FaceProjection>()),
            (false, false) => Fin.Fail<Seq<FaceProjection>>(state.Key.InvalidInput()),
            _ => state.Faces.Traverse(face => FaceCentroid(face: face, runtime: state.Runtime).Map(point => (face, Score: new Vector3d(x: point.X, y: point.Y, z: point.Z) * axis))).As()
                .Map(ranked => descending
                    ? ranked.Maxima(projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)
                    : ranked.Minima(projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)),
        };
    internal static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(Op key, Faces selector, Requirement requirement, bool transfer, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Selector: selector, Transfer: transfer, Project: project), requirement: requirement, requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Analyze.Asks
                from faces in DecomposeFaces(key: state.Key, ctx: context, geometry: geometry).ToEff()
                from chosen in SelectFaces(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
                from result in ProjectOwned(all: faces, chosen: chosen, transfer: state.Transfer, project: values => state.Project(arg1: values, arg2: context)).ToEff()
                select result));
    public static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) {
        Op key = Op.Of(name: nameof(Faces));
        return FaceCentroid(face: face, runtime: runtime)
            .Bind(centroid => {
                BrepFace brepFace = face.Brep.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: double.MaxValue) switch {
                    true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                        (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() => Fin.Succ((frame.ZAxis * (face.Reversed ? -normal : normal)) switch {
                            >= 0.0 => frame,
                            _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                        }),
                        _ => Fin.Fail<Plane>(key.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(key.InvalidResult()),
                };
            });
    }
    public static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) {
        ArgumentNullException.ThrowIfNull(argument: runtime);
        Op key = Op.Of(name: nameof(Faces));
        return Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(key.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    }
    public static Fin<Seq<Interval>> FaceDomains(FaceProjection face) {
        Op key = Op.Of(name: nameof(Faces));
        return (face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
            _ => Fin.Fail<Seq<Interval>>(key.InvalidResult()),
        };
    }
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return aspect?.Apply<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput());
    }
    public static Eff<Env, Seq<CurveProjection>> CurveProjections(object geometry, Curves aspect) {
        Op key = Op.Of(name: nameof(Curves));
        return from context in Analyze.Asks
               from kind in geometry.Kind(ctx: context).ToEff()
               from curves in kind.Curves(value: geometry, selector: aspect.ToSelector(topology: kind.Topology), ctx: context, op: key).ToEff()
               from chosen in SelectCurves(curves: curves, aspect: aspect).ToEff()
               select chosen;
    }
    internal static Fin<Seq<CurveProjection>> SelectCurves(Seq<CurveProjection> curves, Curves aspect) =>
        (aspect, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<CurveProjection>()),
            (Rasm.Analysis.Curves.AtCase at, int count) => Fin.Succ(Seq(curves[RhinoMath.Clamp(at.Value ?? 0, 0, count - 1)])),
            _ => Fin.Succ(curves),
        };
    internal static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
        Seq<TProjection> all, Seq<TProjection> chosen, bool transfer,
        Func<Seq<TProjection>, Fin<Seq<TValue>>> project) where TProjection : ITopologyProjection {
        Fin<Seq<TValue>> result = project(arg: chosen);
        _ = all.Filter(value => !(transfer && result.IsSucc && chosen.Exists(c => c.SameAs(other: value)))).Iter(static v => v.Dispose());
        return result;
    }
}

// --- [FACES_ROLE] ------------------------------------------------------------------------
internal static class FacesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Faces selector) where TGeometry : notnull {
        Op key = Op.Of(name: nameof(Faces));
        return Query.Supports(typeof(TGeometry)) switch {
            false => key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Query.FaceQuery<TGeometry, TOut, Brep>(key: key, selector: selector, requirement: Requirement.None, transfer: true, project: static (chosen, _) => Query.Many(key: Op.Of(name: nameof(Faces)), values: chosen.Map(static face => face.Brep))),
                Type t when t == typeof(Plane) => Query.FaceQuery<TGeometry, TOut, Plane>(key: key, selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Point3d) => Query.FaceQuery<TGeometry, TOut, Point3d>(key: key, selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FaceCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Vector3d) => Query.FaceQuery<TGeometry, TOut, Vector3d>(key: key, selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                Type t when t == typeof(int) => Query.FaceQuery<TGeometry, TOut, int>(key: key, selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Query.Many(key: Op.Of(name: nameof(Faces)), values: chosen.Map(static face => face.FaceIndex))),
                Type t when t == typeof(ComponentIndex) => Query.FaceQuery<TGeometry, TOut, ComponentIndex>(key: key, selector: selector, requirement: Requirement.None, transfer: false, project: static (chosen, _) => Query.Many(key: Op.Of(name: nameof(Faces)), values: chosen.Map(static face => new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))),
                Type t when t == typeof(Interval) => Query.FaceQuery<TGeometry, TOut, Interval>(key: key, selector: selector, requirement: Requirement.SurfaceEvaluation, transfer: false, project: static (chosen, _) => chosen.Traverse(Query.FaceDomains).Map(static nested => nested.Bind(static domain => domain)).As()),
                _ => key.Unsupported<TGeometry, TOut>(),
            },
        };
    }
}

// --- [CURVES_ROLE] -----------------------------------------------------------------------
internal static class CurvesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Curves aspect) where TGeometry : notnull {
        Op key = Op.Of(name: nameof(Curves));
        return Query.Supports(geometry: typeof(TGeometry), native: [typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc)]) switch {
            false => key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => CurveProject<TGeometry, TOut, Curve>(key: key, aspect: aspect, project: static projection => projection.Curve),
                Type t when t == typeof(CurveFeature) => CurveProject<TGeometry, TOut, CurveFeature>(key: key, aspect: aspect, project: static projection => projection.Feature),
                Type t when t == typeof(ComponentIndex) => CurveProject<TGeometry, TOut, ComponentIndex>(key: key, aspect: aspect, project: static projection => projection.Source),
                _ => key.Unsupported<TGeometry, TOut>(),
            },
        };
    }
    private static Query<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<CurveProjection, TValue> project) where TGeometry : notnull =>
        Query.Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Analyze.Asks
                from kind in ((object)geometry).Kind(ctx: context).ToEff()
                from curves in kind.Curves(value: geometry, selector: state.Aspect.ToSelector(topology: kind.Topology), ctx: context, op: state.Key).ToEff()
                from chosen in Query.SelectCurves(curves: curves, aspect: state.Aspect).ToEff()
                from result in Query.ProjectOwned(all: curves, chosen: chosen, transfer: typeof(TValue) == typeof(Curve), project: values => Query.Many(key: state.Key, values: values.Map(state.Project))).ToEff()
                select result));
}

// --- [MESHES_ROLE] -----------------------------------------------------------------------
internal static class MeshesRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Meshes selector) where TGeometry : notnull => selector.Switch(
        validityBundleCase: static _ => Lift<TGeometry, TOut, bool>(key: Op.Of(name: "MeshValidityBundle"), source: Query.MeshValidityBundle),
        statsBundleCase: static _ => Lift<TGeometry, TOut, int>(key: Op.Of(name: "MeshStatsBundle"), source: Query.MeshStatsBundle),
        defectsBundleCase: static _ => Lift<TGeometry, TOut, int>(key: Op.Of(name: "MeshDefectsBundle"), source: Query.MeshDefectsBundle),
        faceQualityCase: static fq => Lift<TGeometry, TOut, MeshFaceSample>(key: Op.Of(name: nameof(MeshFaceMetric)), source: Query.MeshFaceMetric(metric: fq.Metric)),
        atFaceCase: static at => Lift<TGeometry, TOut, MeshFaceProjection>(key: Op.Of(name: "MeshAtFace"), source: Query.MeshAtFace(index: at.Value)));
    private static Query<TGeometry, TOut> Lift<TGeometry, TOut, TValue>(Op key, Query<Mesh, TValue> source) where TGeometry : notnull =>
        Query.Native<TGeometry, TOut, Mesh, TValue, Query<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: mesh));
}

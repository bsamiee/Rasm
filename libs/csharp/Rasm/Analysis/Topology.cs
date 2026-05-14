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
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from domains in kind.Domains(value: geometry, op: op).ToEff()
                    from result in Many(key: op, values: domains).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        Query.Curves<TGeometry, TOut>(aspect: Rasm.Analysis.Curves.Segments);
    internal static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target && Supports(geometry: geometry, native: native);
    internal static bool Supports(Type geometry, params Type[] native) =>
        geometry == typeof(object) || geometry == typeof(GeometryBase) || native.Any(predicate: candidate => candidate.IsAssignableFrom(c: geometry));
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return Query<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from kind in ((object)geometry).Kind(ctx: context).ToEff()
                from curves in kind.IsoCurves(value: geometry, direction: state.Iso, normalized: state.Normalized, op: state.Key).ToEff()
                from result in Many(key: state.Key, values: curves).ToEff()
                select result);
    }
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut).AsKind(), Dispatch.SupportsCoercion(source: typeof(TGeometry), target: typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => Query<TGeometry, TOut>.Build(
                key: key, requirement: Requirement.Basic, requiresContext: true,
                state: (Key: key, Kind: someKind.IfNone(() => Rasm.Domain.Kind.Surface)),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
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
                    from context in Env.Asks
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
                from context in Env.Asks
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
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from contained in kind.Contains(value: geometry, target: state.Target, ctx: context, op: state.Key).ToEff()
                    from result in One(key: state.Key, value: contained).ToEff()
                    select result)),
        };
    }
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object)) && typeof(TOut) == typeof(Mesh))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from components in kind.Components(value: geometry, ctx: context, op: op).ToEff()
                    from result in components.TraverseM(component => component is TOut typed ? Fin.Succ(typed) : Fin.Fail<TOut>(op.Unsupported(geometryType: component.GetType(), outputType: typeof(TOut)))).As().ToEff()
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
        return geometry.Check(textLog: textLog, parameters: ref parameters) ? One(key: op, value: parameters) : Fin.Fail<Seq<MeshCheckParameters>>(op.InvalidResult());
    }
    public static Query<Mesh, int> MeshCheckCount(MeshCheckCount count) {
        Op key = Op.Of();
        return count == Rasm.Analysis.MeshCheckCount.None
            ? Query<Mesh, int>.Reject(key: key, fault: key.InvalidInput())
            : Query<Mesh, int>.Build(
                key: key, state: (Key: key, Count: count),
                evaluator: static (state, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                       from head in parameters.Head.ToFin(state.Key.InvalidResult()).ToEff()
                                                       from result in One(key: state.Key, value: state.Count.Get(parameters: head)).ToEff()
                                                       select result);
    }
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric metric) {
        Op key = Op.Of();
        return metric switch {
            Rasm.Analysis.MeshFaceMetric.None => Query<Mesh, MeshFaceSample>.Reject(key: key, fault: key.InvalidInput()),
            _ => Query<Mesh, MeshFaceSample>.Build(
                key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => state.Metric.Sample(mesh: geometry, face: face)
                        .Bind(v => RhinoMath.IsValidDouble(x: v) && v >= 0.0
                            ? Fin.Succ(new MeshFaceSample(Face: face, Value: v))
                            : Fin.Fail<MeshFaceSample>(state.Key.InvalidResult()))).As().ToEff()),
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
                    geometry.TopologyEdges.Count, geometry.TopologyVertices.Count - geometry.TopologyEdges.Count + geometry.Faces.Count,
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
                                                    from result in Many(key: op, values: Rasm.Analysis.MeshCheckCount.Defects.Select(m => m.Get(parameters: head))).ToEff()
                                                    select result);
        }
    }
    public static Query<Mesh, MeshFaceProjection> MeshAtFace(int? index = null) {
        Op key = Op.Of();
        return Query<Mesh, MeshFaceProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => geometry.Faces.Count switch {
                0 => Fin.Fail<Seq<MeshFaceProjection>>(state.Key.InvalidResult()).ToEff(),
                int count => MeshFaceProjection.Create(mesh: geometry, face: RhinoMath.Clamp(state.Selector ?? 0, 0, count - 1))
                    .Bind(projection => One(key: state.Key, value: projection))
                    .ToEff(),
            });
    }
    public static Query<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull => Aspect<Meshes, TGeometry, TOut>(aspect: aspect);
    internal static Query<TGeometry, TOut> MeshLift<TGeometry, TOut, TValue>(Op key, Query<Mesh, TValue> source) where TGeometry : notnull =>
        Native<TGeometry, TOut, Mesh, TValue, Query<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: mesh));
    internal static Fin<Seq<Polyline>> SelfIntersectionsValue(Op op, Mesh geometry, Env runtime) {
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
                true => (ManyOrEmpty(key: op, values: perforations), ManyOrEmpty(key: op, values: overlaps))
                    .Apply((left, right) => left + right)
                    .As(),
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(op.InvalidResult()),
            };
    }
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull => Aspect<Faces, TGeometry, TOut>(aspect: aspect);
    public static Eff<Env, Seq<FaceProjection>> FaceProjections(object geometry, Faces selector) =>
        FaceProjections(geometry: geometry, choose: _ => selector);
    public static Eff<Env, Seq<FaceProjection>> FaceProjections(object geometry, Func<int, Faces> choose) =>
        from context in Env.Asks
        from faces in DecomposeFaces(key: Rasm.Analysis.Faces.Key, ctx: context, geometry: geometry).ToEff()
        from chosen in SelectFaces(key: Rasm.Analysis.Faces.Key, faces: faces, selector: choose(arg: faces.Count), runtime: context).ToEff()
        from result in ProjectOwned(all: faces, chosen: chosen, transfer: true, project: static values => Fin.Succ(values)).ToEff()
        select result;
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
                from context in Env.Asks
                from faces in DecomposeFaces(key: state.Key, ctx: context, geometry: geometry).ToEff()
                from chosen in SelectFaces(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
                from result in ProjectOwned(all: faces, chosen: chosen, transfer: state.Transfer, project: values => state.Project(arg1: values, arg2: context)).ToEff()
                select result));
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
                        _ => Fin.Fail<Plane>(Rasm.Analysis.Faces.Key.InvalidResult()),
                    },
                    false => Fin.Fail<Plane>(Rasm.Analysis.Faces.Key.InvalidResult()),
                };
            });
    public static Fin<Point3d> FaceCentroid(FaceProjection face, Context runtime) {
        ArgumentNullException.ThrowIfNull(argument: runtime);
        return Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Relative.Value, absoluteTolerance: runtime.Absolute.Value))
            .ToFin(Rasm.Analysis.Faces.Key.InvalidResult())
            .Map(static mass => { using AreaMassProperties disposable = mass; return disposable.Centroid; });
    }
    public static Fin<Seq<Interval>> FaceDomains(FaceProjection face) {
        BrepFace brepFace = face.Brep.Faces[0];
        return (brepFace.Domain(direction: 0), brepFace.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
            _ => Fin.Fail<Seq<Interval>>(Rasm.Analysis.Faces.Key.InvalidResult()),
        };
    }
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull => Aspect<Curves, TGeometry, TOut>(aspect: aspect);
    internal static Query<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<CurveProjection, TValue> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in ((object)geometry).Kind(ctx: runtime.Context).ToEff()
                from curves in kind.Curves(value: geometry, selector: state.Aspect.ToSelector(topology: kind.Topology), ctx: runtime.Context, op: state.Key, cancel: runtime.Cancellation).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in ProjectOwned(all: curves, chosen: chosen, transfer: typeof(TValue) == typeof(Curve), project: values => Many(key: state.Key, values: values.Map(state.Project))).ToEff()
                select result));
    public static Eff<Env, Seq<CurveProjection>> CurveProjections(object geometry, Curves aspect) =>
        from runtime in Env.EnvAsks
        from kind in geometry.Kind(ctx: runtime.Context).ToEff()
        from curves in kind.Curves(value: geometry, selector: aspect.ToSelector(topology: kind.Topology), ctx: runtime.Context, op: Rasm.Analysis.Curves.Key, cancel: runtime.Cancellation).ToEff()
        from chosen in aspect.Select(curves: curves).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, transfer: true, project: static values => Fin.Succ(values)).ToEff()
        select result;
    public static Eff<Env, Seq<CurveProjection>> CurveProjections(object geometry, Func<int, Curves> choose) =>
        from runtime in Env.EnvAsks
        from kind in geometry.Kind(ctx: runtime.Context).ToEff()
        from curves in kind.Curves(value: geometry, selector: Rasm.Analysis.Curves.All.ToSelector(topology: kind.Topology), ctx: runtime.Context, op: Rasm.Analysis.Curves.Key, cancel: runtime.Cancellation).ToEff()
        from aspect in Fin.Succ(choose(arg: curves.Count)).ToEff()
        from chosen in aspect.Select(curves: curves).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, transfer: true, project: static values => Fin.Succ(values)).ToEff()
        select result;
    internal static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
        Seq<TProjection> all, Seq<TProjection> chosen, bool transfer,
        Func<Seq<TProjection>, Fin<Seq<TValue>>> project) where TProjection : ITopologyProjection {
        Fin<Seq<TValue>> result = project(arg: chosen);
        bool keep = transfer && result.IsSucc;
        _ = all.Filter(value => !keep || !chosen.Exists(c => c.SameAs(other: value))).Iter(static v => v.Dispose());
        return result;
    }
}

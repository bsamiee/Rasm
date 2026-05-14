namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Faces : IAspect {
    public sealed record AllCase : Faces; public sealed record TopCase(Vector3d Axis) : Faces; public sealed record BottomCase(Vector3d Axis) : Faces; public sealed record AtCase(int? Value) : Faces;
    public static Faces All => new AllCase();
    public static Faces Top(Vector3d? axis = null) => new TopCase(Axis: axis ?? Vector3d.ZAxis);
    public static Faces Bottom(Vector3d? axis = null) => new BottomCase(Axis: axis ?? Vector3d.ZAxis);
    public static Faces At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Faces));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull =>
        Dispatch.SupportsFaces(source: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Analyze.FaceQuery<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Transfer, project: static (chosen, _) => Analyze.Many(key: Key, values: chosen.Map(static face => face.Brep))),
                Type t when t == typeof(Plane) => Analyze.FaceQuery<TGeometry, TOut, Plane>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Analyze.FrameAtCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Point3d) => Analyze.FaceQuery<TGeometry, TOut, Point3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Analyze.FaceCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Vector3d) => Analyze.FaceQuery<TGeometry, TOut, Vector3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Analyze.FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                Type t when t == typeof(int) => Analyze.FaceQuery<TGeometry, TOut, int>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => Analyze.Many(key: Key, values: chosen.Map(static face => face.FaceIndex))),
                Type t when t == typeof(ComponentIndex) => Analyze.FaceQuery<TGeometry, TOut, ComponentIndex>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => Analyze.Many(key: Key, values: chosen.Map(static face => new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))),
                Type t when t == typeof(Interval) => Analyze.FaceQuery<TGeometry, TOut, Interval>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => chosen.Traverse(Analyze.FaceDomains).Map(static nested => nested.Bind(static domain => domain)).As()),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull => Aspect<Faces, TGeometry, TOut>(aspect: aspect);
    public static Eff<Env, Seq<FaceProjection>> FaceProjections(object geometry, Faces selector) =>
        FaceProjections(geometry: geometry, choose: _ => selector);
    public static Eff<Env, Seq<FaceProjection>> FaceProjections(object geometry, Func<int, Faces> choose) =>
        from context in Env.Asks
        from faces in DecomposeFaces(key: Rasm.Analysis.Faces.Key, context: context, geometry: geometry).ToEff()
        from chosen in SelectFaces(key: Rasm.Analysis.Faces.Key, faces: faces, selector: choose(arg: faces.Count), runtime: context).ToEff()
        from result in ProjectOwned(all: faces, chosen: chosen, ownership: ProjectionOwnership.Transfer, project: static values => Fin.Succ(values)).ToEff()
        select result;
    internal static Fin<Seq<FaceProjection>> DecomposeFaces<TGeometry>(Op key, Context context, TGeometry geometry) where TGeometry : notnull =>
        ((object)geometry).Kind(context: context).Bind(kind => kind.Faces(geometry: geometry, context: context, op: key));
    internal static Fin<Seq<FaceProjection>> SelectFaces(Op key, Seq<FaceProjection> faces, Faces selector, Context runtime) => selector.Switch(
        state: (Key: key, Faces: faces, Runtime: runtime),
        allCase: static (s, _) => Fin.Succ(s.Faces),
        topCase: static (s, top) => RankFaces(state: s, axis: top.Axis, descending: true),
        bottomCase: static (s, bottom) => RankFaces(state: s, axis: bottom.Axis, descending: false),
        atCase: static (s, at) => (s.Faces.Count, at.Value) switch {
            (0, _) => Fin.Succ(Seq<FaceProjection>()),
            (int n, int index) when index < 0 || index >= n => Fin.Fail<Seq<FaceProjection>>(s.Key.InvalidInput()),
            (_, int index) => Fin.Succ(Seq(s.Faces[index])),
            _ => Fin.Succ(Seq(s.Faces[0])),
        });
    private static Fin<Seq<FaceProjection>> RankFaces((Op Key, Seq<FaceProjection> Faces, Context Runtime) state, Vector3d axis, bool descending) =>
        (state.Faces.IsEmpty, axis.IsValid && !axis.IsTiny()) switch {
            (true, _) => Fin.Succ(Seq<FaceProjection>()),
            (false, false) => Fin.Fail<Seq<FaceProjection>>(state.Key.InvalidInput()),
            _ => state.Faces.Traverse(face => FaceCentroid(face: face, runtime: state.Runtime).Map(point => (face, Score: new Vector3d(x: point.X, y: point.Y, z: point.Z) * axis))).As()
                .Map(ranked => descending
                    ? ranked.Maxima(projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)
                    : ranked.Minima(projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)),
        };
    internal static Query<TGeometry, TOut> FaceQuery<TGeometry, TOut, TValue>(Op key, Faces selector, Requirement requirement, ProjectionOwnership ownership, Func<Seq<FaceProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Selector: selector, Ownership: ownership, Project: project), requirement: requirement, requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from faces in DecomposeFaces(key: state.Key, context: context, geometry: geometry).ToEff()
                from chosen in SelectFaces(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
                from result in ProjectOwned(all: faces, chosen: chosen, ownership: state.Ownership, project: values => state.Project(arg1: values, arg2: context)).ToEff()
                select result));
    public static Fin<Plane> FrameAtCentroid(FaceProjection face, Context runtime) =>
        FaceCentroid(face: face, runtime: runtime)
            .Bind(centroid => {
                BrepFace brepFace = face.Brep.Faces[0];
                return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: 0.0) switch {
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
        return Optional(AreaMassProperties.Compute(brep: face.Brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: runtime.Fractional, absoluteTolerance: runtime.Absolute.Value))
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
}

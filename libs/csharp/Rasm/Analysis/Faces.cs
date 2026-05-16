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
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
        GeometryKernel.CanDecomposeFaces(type: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Analyze.FaceOperation<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None, project: static (chosen, _) => Key.Accept(values: chosen.Choose(static face => face.As<Brep>()))),
                Type t when t == typeof(TopologyProjection) => Analyze.FaceOperation<TGeometry, TOut, TopologyProjection>(key: Key, selector: this, requirement: Requirement.None, project: static (chosen, _) => Key.Accept(values: chosen)),
                Type t when t == typeof(Plane) => Analyze.FaceOperation<TGeometry, TOut, Plane>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, project: static (chosen, runtime) => chosen.Traverse(face => face.FrameAtCentroid(context: runtime, key: Key)).As()),
                Type t when t == typeof(Point3d) => Analyze.FaceOperation<TGeometry, TOut, Point3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, project: static (chosen, runtime) => chosen.Traverse(face => face.Centroid(context: runtime, key: Key)).As()),
                Type t when t == typeof(Vector3d) => Analyze.FaceOperation<TGeometry, TOut, Vector3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, project: static (chosen, runtime) => chosen.Traverse(face => face.FrameAtCentroid(context: runtime, key: Key).Map(static frame => frame.ZAxis)).As()),
                Type t when t == typeof(int) => Analyze.FaceOperation<TGeometry, TOut, int>(key: Key, selector: this, requirement: Requirement.None, project: static (chosen, _) => Key.Accept(values: chosen.Map(static face => face.Source.Index))),
                Type t when t == typeof(ComponentIndex) => Analyze.FaceOperation<TGeometry, TOut, ComponentIndex>(key: Key, selector: this, requirement: Requirement.None, project: static (chosen, _) => Key.Accept(values: chosen.Map(static face => face.Source))),
                Type t when t == typeof(Interval) => Analyze.FaceOperation<TGeometry, TOut, Interval>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, project: static (chosen, _) => chosen.Traverse(face => face.Domains(key: Key)).Map(static nested => nested.Bind(static domain => domain)).As()),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Faces<TGeometry, TOut>(Faces aspect) where TGeometry : notnull => Aspect<Faces, TGeometry, TOut>(aspect: aspect);
    internal static Fin<Seq<TopologyProjection>> DecomposeFaces<TGeometry>(Op key, Context context, TGeometry geometry) where TGeometry : notnull =>
        Optional(geometry).ToFin(key.InvalidInput()).Bind(g => g switch {
            BrepFace face => Fin.Succ(Seq(TopologyProjection.Of(face))),
            GeometryBase native => native switch {
                Brep brep => Fin.Succ(toSeq(brep.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.Of(f)).ToArray())),
                { HasBrepForm: true } => GeometryKernel.BrepForm(source: native, op: key).Bind(lease => lease.Switch(
                    borrowed: static borrowed => Fin.Succ(toSeq(borrowed.Value.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.Of(f)).ToArray())),
                    owned: static owned => owned.Project(static brep => Fin.Succ(toSeq(brep.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.Of(f, copy: true)).ToArray()))))),
                _ => Fin.Fail<Seq<TopologyProjection>>(key.Unsupported(native.GetType(), typeof(Seq<TopologyProjection>))),
            },
            _ => Fin.Fail<Seq<TopologyProjection>>(key.Unsupported(g.GetType(), typeof(Seq<TopologyProjection>))),
        });
    internal static Fin<Seq<TopologyProjection>> SelectFaces(Op key, Seq<TopologyProjection> faces, Faces selector, Context runtime) => selector.Switch(
        state: (Key: key, Faces: faces, Runtime: runtime),
        allCase: static (s, _) => Fin.Succ(s.Faces),
        topCase: static (s, top) => RankFaces(state: s, axis: top.Axis, descending: true),
        bottomCase: static (s, bottom) => RankFaces(state: s, axis: bottom.Axis, descending: false),
        atCase: static (s, at) => (s.Faces.Count, at.Value) switch {
            (0, _) => Fin.Succ(Seq<TopologyProjection>()),
            (int n, int index) => Fin.Succ(Seq(s.Faces[Math.Clamp(value: index, min: 0, max: n - 1)])),
            _ => Fin.Succ(Seq(s.Faces[0])),
        });
    private static Fin<Seq<TopologyProjection>> RankFaces((Op Key, Seq<TopologyProjection> Faces, Context Runtime) state, Vector3d axis, bool descending) =>
        (state.Faces.IsEmpty, axis.IsValid && !axis.IsTiny()) switch {
            (true, _) => Fin.Succ(Seq<TopologyProjection>()),
            (false, false) => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
            _ => state.Faces.Traverse(face => face.Centroid(context: state.Runtime, key: state.Key).Map(point => (face, Score: new Vector3d(x: point.X, y: point.Y, z: point.Z) * axis))).As()
                .Map(ranked => descending
                    ? Stats.Maxima(items: ranked, projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)
                    : Stats.Minima(items: ranked, projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value * axis.Length).Map(static item => item.face)),
        };
    internal static Operation<TGeometry, TOut> FaceOperation<TGeometry, TOut, TValue>(Op key, Faces selector, Requirement requirement, Func<Seq<TopologyProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Selector: selector, Project: project), requirement: requirement, requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from faces in DecomposeFaces(key: state.Key, context: context, geometry: geometry).ToEff()
                from chosen in SelectFaces(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
                from result in TopologyProjection.Project(all: faces, chosen: chosen, project: values => state.Project(arg1: values, arg2: context)).ToEff()
                select result).As<TGeometry, TOut>(key: key);
}

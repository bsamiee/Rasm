using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
public enum SolidOrientation { Unknown = 0, Outward = 1, Inward = -1 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, Iso, Silhouette, SubCurve, Draft }
internal enum ProjectionOwnership { Dispose, Transfer }
internal enum CapTag { Bounds, Vertices, Centroid, Closest, Components, Domains, IsoCurves, ControlPoints, Curves, Intersect, Deviation, Length, Contains, SolidOrientation, Faces, Conformance, Coerce }
[BoundaryAdapter]
public sealed record TopologyProjection {
    private static readonly Op Key = Op.Of(name: nameof(TopologyProjection));
    public GeometryBase Value { get; }
    public CurveFeature Feature { get; }
    public ComponentIndex Source { get; }
    public bool Owns { get; }
    public bool Reversed { get; }
    private TopologyProjection(GeometryBase value, CurveFeature feature, ComponentIndex source, bool owns, bool reversed = false) { Value = value; Feature = feature; Source = source; Owns = owns; Reversed = reversed; }
    public int FaceIndex => Source.Index;
    public Option<T> As<T>() where T : class => Value is T match ? Some(match) : Option<T>.None;
    internal Fin<T> OnMeshFace<T>(Func<Mesh, int, Fin<T>> use) where T : notnull => (Value, Source) switch { (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index }) when index >= 0 && index < mesh.Faces.Count => use(mesh, index), _ => Fin.Fail<T>(Key.InvalidInput()) };
    public static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndex source) { ArgumentNullException.ThrowIfNull(curve); return new(value: curve, feature: feature, source: source, owns: true); }
    internal static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndexType type, int index) => FromCurve(curve, feature, new ComponentIndex(type, index));
    public static TopologyProjection FaceFrom(BrepFace face) { ArgumentNullException.ThrowIfNull(face); return new(value: face.DuplicateFace(duplicateMeshes: false), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), owns: true, reversed: face.OrientationIsReversed); }
    public static Fin<TopologyProjection> MeshFace(Mesh? mesh, int face) => Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => (native.Faces.Count, face) switch { ( <= 0, _) => Fin.Fail<TopologyProjection>(Key.InvalidResult()), (int count, int index) when index >= 0 && index < count => Fin.Succ<TopologyProjection>(new(value: native, feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.MeshFace, index), owns: false)), _ => Fin.Fail<TopologyProjection>(Key.InvalidInput()) });
    public Unit Dispose() => Optional((Owns, Value) switch { (true, IDisposable disposable) => disposable, _ => null }).Iter(static d => d.Dispose());
    public bool SameAs(TopologyProjection? other) => other switch { TopologyProjection p => ReferenceEquals(Value, p.Value) && Source.Equals(p.Source), _ => false };
    public bool Transfers(Type outputType) => Owns && ((Value is Curve && outputType == typeof(Curve)) || (Value is Brep && outputType == typeof(Brep)));
    public Fin<Seq<Point3d>> Vertices => OnMeshFace<Seq<Point3d>>(static (mesh, face) => mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch { true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)), true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)), false => Fin.Fail<Seq<Point3d>>(Key.InvalidResult()) });
    public Fin<Vector3d> Normal => OnMeshFace<Vector3d>(static (mesh, face) => (mesh.FaceNormals.Count == mesh.Faces.Count || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.UnitizeFaceNormals() && mesh.FaceNormals[face] is Vector3f n && ((Vector3d)n) is { IsValid: true } normal ? Fin.Succ(normal) : Fin.Fail<Vector3d>(Key.InvalidResult()));
    internal static Fin<Seq<TValue>> Project<TValue>(
        Seq<TopologyProjection> all,
        Seq<TopologyProjection> chosen,
        ProjectionOwnership ownership,
        Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
        Fin<Seq<TValue>> result = project(arg: chosen);
        bool keep = ownership == ProjectionOwnership.Transfer && result.IsSucc;
        _ = all.Filter(value => !keep || !chosen.Exists(c => c.SameAs(other: value))).Iter(static v => v.Dispose());
        return result;
    }
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveSelector(CurveFeature Feature, Option<Vector3d> Direction = default, Option<double> Angle = default, Option<int> Index = default, Option<double> Normalized = default, Option<IsoStatus> Iso = default);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Hit(int Id);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Couple(int A, int B);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);
[BoundaryAdapter]
public abstract partial record IntersectionHit {
    private IntersectionHit() { }
    internal sealed record PointCase(Point3d Point) : IntersectionHit;
    internal sealed record CurveCase(Curve Curve, IntersectionKind CurveKind) : IntersectionHit;
    internal sealed record OverlapCase(Point3d Start, Point3d End, Interval OverlapA, Interval OverlapB, Option<Curve> Curve) : IntersectionHit;
    public IntersectionKind Kind => this switch { PointCase => IntersectionKind.Point, CurveCase c => c.CurveKind, OverlapCase => IntersectionKind.Overlap, _ => IntersectionKind.Unknown };
    public Seq<Curve> Curves => this switch { CurveCase c => Seq(c.Curve), OverlapCase o => o.Curve.ToSeq(), _ => Seq<Curve>() };
    public Seq<Point3d> Points => this switch { PointCase p => Seq(p.Point), OverlapCase o => Seq(o.Start, o.End), _ => Seq<Point3d>() };
    public Seq<Interval> Intervals => this switch { OverlapCase o => Seq(o.OverlapA, o.OverlapB), _ => Seq<Interval>() };
    public static IntersectionHit At(Point3d point) => new PointCase(point);
    public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new CurveCase(curve, kind);
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB, Option<Curve> curve = default) => new OverlapCase(start, end, overlapA, overlapB, curve);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance);
[BoundaryAdapter] internal sealed record Cap(CapTag Tag, Type Left, Type? Right, object? Variant, int Priority, Delegate Run);
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult; public sealed record Lines(Seq<Line> Values) : IntersectionResult; public sealed record Circles(Seq<Circle> Values) : IntersectionResult; public sealed record Points(Seq<Point3d> Values) : IntersectionResult; public sealed record Intervals(Seq<Interval> Values) : IntersectionResult; public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult; public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
}
// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point = new(0, typeof(Point3d), Topology.Point), Line = new(1, typeof(Line), Topology.Curve), Polyline = new(2, typeof(Polyline), Topology.Curve), Circle = new(3, typeof(Circle), Topology.Curve), Arc = new(4, typeof(Arc), Topology.Curve), Ellipse = new(5, typeof(Ellipse), Topology.Curve);
    public static readonly Kind Curve = new(6, typeof(Curve), Topology.Curve), Surface = new(7, typeof(Surface), Topology.Surface), Plane = new(8, typeof(Plane), Topology.Surface), Sphere = new(9, typeof(Sphere), Topology.Surface), Cylinder = new(10, typeof(Cylinder), Topology.Surface), Cone = new(11, typeof(Cone), Topology.Surface), Torus = new(12, typeof(Torus), Topology.Surface);
    public static readonly Kind Brep = new(13, typeof(Brep), Topology.Brep), Box = new(14, typeof(Box), Topology.Brep), BoundingBox = new(15, typeof(BoundingBox), Topology.Brep), Mesh = new(16, typeof(Mesh), Topology.Mesh), SubD = new(17, typeof(SubD), Topology.SubD), PointCloud = new(18, typeof(PointCloud), Topology.PointCloud), Extrusion = new(19, typeof(Extrusion), Topology.Extrusion), Hatch = new(20, typeof(Hatch), Topology.Hatch);
    public Type Type { get; }
    public Topology Topology { get; }
}
[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MassKind {
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None, compute: static (_, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))), aggregate: static (_, _, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))));
    public static readonly MassKind Length = new(key: 1, label: nameof(Length), requirement: Requirement.CurveLength, compute: LengthOf, aggregate: LengthAggregate);
    public static readonly MassKind Area = new(key: 2, label: nameof(Area), requirement: Requirement.AreaMass, compute: AreaOf, aggregate: static (self, geom, ctx, fm, sm, pm, op) => SumAggregate<AreaMassProperties>(geom, ctx, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)));
    public static readonly MassKind Volume = new(key: 3, label: nameof(Volume), requirement: Requirement.VolumeMass, compute: VolumeOf, aggregate: static (self, geom, ctx, fm, sm, pm, op) => SumAggregate<VolumeMassProperties>(geom, ctx, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)));
    private readonly Func<object, Context, bool, bool, bool, Op, Fin<IDisposable>> compute;
    private readonly Func<MassKind, IEnumerable<GeometryBase>, Context, bool, bool, bool, Op, Fin<IDisposable>> aggregate;
    public string Label { get; }
    internal Requirement Requirement { get; }
    internal Fin<IDisposable> Compute(object geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => compute(geometry, context, firstMoments, secondMoments, productMoments, op);
    internal Fin<IDisposable> Aggregate(IEnumerable<GeometryBase> geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => aggregate(this, geometry, context, firstMoments, secondMoments, productMoments, op);
    public Eff<Env, IDisposable> Compute(object? geometry, Op op, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) => Optional(geometry).ToFin(op.InvalidInput()).ToEff().Bind(g => Env.Asks.Bind(context => Compute(g, context, firstMoments, secondMoments, productMoments, op).ToEff()));
    private static Fin<IDisposable> Done<TMass>(TMass? mass) where TMass : class, IDisposable => Optional(mass).ToFin(new Fault.ComputationFailed(typeof(TMass).Name)).Map(static p => (IDisposable)p);
    private static Fin<IDisposable> LengthOf(object geometry, Context _, bool fm, bool sm, bool pm, Op op) => geometry switch { Curve curve => Done(LengthMassProperties.Compute(curve, length: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(LengthMassProperties))) };
    private static Fin<IDisposable> AreaOf(object geometry, Context context, bool fm, bool sm, bool pm, Op op) => geometry switch { Curve curve => Done(AreaMassProperties.Compute(curve, context.Absolute.Value)), Mesh mesh => Done(AreaMassProperties.Compute(mesh, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), Brep brep => Done(AreaMassProperties.Compute(brep, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm, relativeTolerance: context.Relative.Value, absoluteTolerance: context.Absolute.Value)), Surface surface => Done(AreaMassProperties.Compute(surface, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(AreaMassProperties))) };
    private static Fin<IDisposable> VolumeOf(object geometry, Context context, bool fm, bool sm, bool pm, Op op) => geometry switch { Mesh mesh => Done(VolumeMassProperties.Compute(mesh, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), Brep brep => Done(VolumeMassProperties.Compute(brep, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm, relativeTolerance: context.Relative.Value, absoluteTolerance: context.Absolute.Value)), Surface surface => Done(VolumeMassProperties.Compute(surface, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(VolumeMassProperties))) };
    private static Fin<IDisposable> LengthAggregate(MassKind self, IEnumerable<GeometryBase> geometry, Context context, bool fm, bool sm, bool pm, Op op) => toSeq(geometry) switch { Seq<GeometryBase> items when items.ForAll(static i => i is Curve) => Done(LengthMassProperties.Compute(curves: items.AsIterable().Cast<Curve>(), length: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), Seq<GeometryBase> items => SumAggregate<LengthMassProperties>(items.AsIterable(), context, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)) };
    private static Fin<IDisposable> SumAggregate<TMass>(IEnumerable<GeometryBase> geometry, Context context, MassKind mass, bool fm, bool sm, bool pm, Op op, Func<TMass, IEnumerable<TMass>, bool> sum) where TMass : class, IDisposable =>
        toSeq(geometry).Fold(Fin.Succ(Seq<IDisposable>()), (state, item) => state.Bind(owned =>
            mass.compute(item, context, fm, sm, pm, op)
                .Match(Succ: r => Fin.Succ(r.Cons(owned)), Fail: e => (owned.Iter(static r => r.Dispose()), Fin.Fail<Seq<IDisposable>>(e)).Item2)))
            .Bind(owned => {
                TMass[] masses = [.. owned.AsIterable().Cast<TMass>()];
                Fin<IDisposable> result = masses.Length switch {
                    1 => Fin.Succ<IDisposable>(masses[0]),
                    > 1 when sum(masses[0], Enumerable.Skip(masses, 1)) => Fin.Succ<IDisposable>(masses[0]),
                    _ => Fin.Fail<IDisposable>(new Fault.ComputationFailed(typeof(TMass).Name)),
                };
                _ = toSeq(Enumerable.Skip(masses, result.IsSucc ? 1 : 0)).Iter(static r => r.Dispose());
                return result;
            });
}
// --- [CONSTANTS] --------------------------------------------------------------------------
// FrozenDictionary on Type keys: LanguageExt v5 Map/HashMap<Type,_> trips a Rhino reflection enumeration bug.
[BoundaryAdapter]
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = Kind.Items.ToFrozenDictionary(static k => k.Type);
    internal static Option<Kind> For(Type type) => type == typeof(Point) ? Some(Kind.Point) : Optional(ByType.GetValueOrDefault(type)) | (InheritsBase(type) is Type bt ? Optional(ByType.GetValueOrDefault(bt)) : Option<Kind>.None);
    internal static Type? InheritsBase(Type type) => type.BaseType is Type b ? (ByType.ContainsKey(b) ? b : InheritsBase(b)) : null;
}
// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class Dispatch {

    internal static Cap For<TS, TArgs, TOut>(CapTag tag, Func<TS, TArgs, Fin<TOut>> run, object? variant = null) where TS : notnull =>
        new(tag, typeof(TS), null, variant, 0, (Func<object, TArgs, Fin<TOut>>)((g, a) => run((TS)g, a)));
    internal static Cap ForPair<TL, TR, TArgs, TOut>(CapTag tag, Func<TL, TR, TArgs, Fin<TOut>> run) where TL : notnull where TR : notnull =>
        new(tag, typeof(TL), typeof(TR), null, 0, (Func<object, object, TArgs, Fin<TOut>>)((l, r, a) => run((TL)l, (TR)r, a)));
    internal static Cap Probe<TS, TT>(int priority, Kind inferred, Func<TS, Context, Option<TT>> probe) where TS : notnull where TT : notnull =>
        new(CapTag.Coerce, typeof(TS), typeof(TT), inferred, priority, (Func<object, Context, Option<object>>)((g, c) => probe((TS)g, c).Map(static v => (object)v)));
    internal static IEnumerable<Cap> SymProbe<TT>(int priority, Kind inferred, Func<Surface, Context, Option<TT>> probe) where TT : notnull => [Probe<Brep, TT>(priority, inferred, (g, c) => g is { IsSurface: true } b ? probe(b.Surfaces[0], c) : Option<TT>.None), Probe<Surface, TT>(priority, inferred, probe)];
    private static Seq<Type> Lineage(Type type) => Seq(type) + Optional(KindLookup.InheritsBase(type)).ToSeq() + (typeof(GeometryBase).IsAssignableFrom(type) ? Seq(typeof(GeometryBase)) : Seq<Type>());
    private static Option<Cap> Lookup(CapTag tag, Type left, Type? right = null, object? variant = null) => right is Type r ? Lineage(left).Bind(l => Lineage(r).Map(rr => (L: l, R: rr))).Choose(p => Optional(Index.GetValueOrDefault((tag, p.L, (Type?)p.R, variant)))).Head : Lineage(left).Choose(t => Optional(Index.GetValueOrDefault((tag, t, (Type?)null, variant)))).Head;
    private static bool Registered(CapTag tag, Type left, Type? right = null) => right is Type r ? Lineage(left).Bind(l => Lineage(r).Map(rr => (L: l, R: (Type?)rr))).Exists(p => SupportIndex.Contains((tag, p.L, p.R))) : Lineage(left).Exists(t => SupportIndex.Contains((tag, t, (Type?)null)));
    private static Fin<TOut> Invoke<TOut, TArgs>(Cap cap, object src, TArgs args, Op op) =>
        Verify.ValidityOf(source: src).Case switch {
            false => Fin.Fail<TOut>(op.InvalidInput()),
            _ => ((Func<object, TArgs, Fin<TOut>>)cap.Run)(src, args),
        };

    internal static Fin<TOut> Resolve<TOut, TArgs>(CapTag tag, object? source, TArgs args, Op op, object? variant = null) => Optional(source).ToFin(op.InvalidInput()).Bind(s => Lookup(tag, s.GetType(), variant: variant).ToFin(op.Unsupported(s.GetType(), typeof(TOut))).Bind(cap => Invoke<TOut, TArgs>(cap, s, args, op)));
    internal static Fin<TOut> Resolve<TOut, TArgs>(CapTag tag, object? left, object? right, TArgs args, Op op, bool unordered = false) =>
        from l in Optional(left).ToFin(op.InvalidInput())
        from r in Optional(right).ToFin(op.InvalidInput())
        from hit in (Lookup(tag, l.GetType(), r.GetType()).Map(c => (L: l, R: r, Cap: c))
            | (unordered ? Lookup(tag, r.GetType(), l.GetType()).Map(c => (L: r, R: l, Cap: c)) : Option<(object L, object R, Cap Cap)>.None)).ToFin(op.Unsupported(l.GetType(), r.GetType()))
        from result in ((Func<object, object, TArgs, Fin<TOut>>)hit.Cap.Run)(hit.L, hit.R, args)
        select result;

    internal static bool Supports(CapTag tag, Type source, Type? right = null, object? variant = null, bool unordered = false) => source == typeof(object) || (right is null ? (variant is null ? Registered(tag, source) : Lookup(tag, source, variant: variant).IsSome) : (variant is null ? Registered(tag, source, right) || (unordered && Registered(tag, right, source)) : Lookup(tag, source, right, variant).IsSome || (unordered && Lookup(tag, right, source, variant).IsSome)));
    internal static bool SupportsBounds(Type source, bool includeSphere) => Supports(CapTag.Bounds, source) && (includeSphere || source != typeof(Sphere)) && source != typeof(Plane);
    internal static bool SupportsKind(Type source) => source == typeof(object) || source == typeof(GeometryBase) || KindLookup.For(source).IsSome;
    internal static bool SupportsCoercion(Type source, Type target) => target.IsAssignableFrom(source) || Lookup(CapTag.Coerce, source, target).IsSome;

    public static Fin<Kind> Kind(this object geometry, Context context) => (CoerceProbes.Choose(p => p.Left.IsInstanceOfType(geometry) ? ((Func<object, Context, Option<object>>)p.Run)(geometry, context).Map(_ => (Kind)p.Variant!) : Option<Kind>.None).Head | KindLookup.For(geometry?.GetType() ?? typeof(object))).ToFin(Op.Of(name: nameof(Kind)).InvalidInput());
    public static Fin<BoundingBox> Bounds(this object geometry, Op op) => Resolve<BoundingBox, Op>(CapTag.Bounds, geometry, op, op);
    public static Fin<TTarget> Coerce<TTarget>(object? source, Context context, Op op) where TTarget : notnull => Optional(source).ToFin(op.InvalidInput()).Bind(s => s switch { TTarget t => op.RequireValid(t), _ => Lookup(CapTag.Coerce, s.GetType(), typeof(TTarget)).ToFin(op.Unsupported(s.GetType(), typeof(TTarget))).Bind(p => ((Func<object, Context, Option<object>>)p.Run)(s, context).ToFin(op.InvalidResult()).Map(static v => (TTarget)v)) });

    public static TR Borrowed<T, TR>(T owned, Func<T, TR> use) where T : IDisposable { ArgumentNullException.ThrowIfNull(use); using T d = owned; return use(d); }
    public static Fin<Seq<double>> Fractions(int count, Op op) => count switch { 1 => Fin.Succ(Seq(0.5)), > 1 => Fin.Succ(toSeq(Enumerable.Range(0, count).Select(i => i / (count - 1.0)))), _ => Fin.Fail<Seq<double>>(op.InvalidInput()) };

    // --- [REGISTRY] ---------------------------------------------------------------------------
    private static readonly Cap[] Capabilities = [.. BuildCapabilities()];
    private static readonly FrozenDictionary<(CapTag Tag, Type Left, Type? Right, object? Variant), Cap> Index = Capabilities.ToFrozenDictionary(static c => (c.Tag, c.Left, c.Right, c.Variant));
    private static readonly FrozenSet<(CapTag Tag, Type Left, Type? Right)> SupportIndex = Capabilities.Select(static c => (c.Tag, c.Left, c.Right)).ToFrozenSet();
    private static readonly Seq<Cap> CoerceProbes = toSeq(Capabilities.Where(static c => c.Tag == CapTag.Coerce).OrderByDescending(static c => c.Priority));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static Fin<Point3d> MassCentroid(object geometry, bool isSolid, Context context, Op op) =>
        (isSolid ? MassKind.Volume : MassKind.Area).Compute(geometry, context, true, false, false, op)
            .Bind(d => Borrowed(d, owned => owned switch { LengthMassProperties l => Fin.Succ(l.Centroid), AreaMassProperties a => Fin.Succ(a.Centroid), VolumeMassProperties v => Fin.Succ(v.Centroid), _ => Fin.Fail<Point3d>(op.InvalidResult()) }));
    private static Fin<Seq<GeometryBase>> BrepComponents(Brep brep, Op op) =>
        brep.GetConnectedComponents() switch {
            Brep[] cs when cs.Length > 0 => Fin.Succ(toSeq(cs.Cast<GeometryBase>())),
            _ => Brep.SplitDisjointPieces(brep) switch {
                Brep[] ps when ps.Length > 0 => Fin.Succ(toSeq(ps.Cast<GeometryBase>())),
                _ => op.RequireValid(brep).Map(static v => Seq((GeometryBase)v.DuplicateBrep())),
            },
        };
    private static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso)).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(iso == IsoStatus.X ? 0 : 1) is { IsValid: true } d =>
            surface is BrepFace face ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(iso == IsoStatus.X ? 0 : 1, d.ParameterAt(normalized))))
                : Optional(surface.IsoCurve(iso, d.ParameterAt(normalized))).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        _ => Fin.Fail<Seq<Curve>>(op.InvalidInput()),
    };
    private static bool OnFiniteLine(Line line, Point3d p, double tol) => p.IsValid && p.DistanceTo(line.ClosestPoint(p, true)) <= tol;
    private static Seq<Interval> SegmentInterval(Interval iv) =>
        (Math.Min(iv.T0, iv.T1), Math.Max(iv.T0, iv.T1)) switch {
            (double min, double max) when Math.Max(min, 0.0) <= Math.Min(max, 1.0) => Seq(new Interval(
                iv.T0 <= iv.T1 ? Math.Max(min, 0.0) : Math.Min(max, 1.0),
                iv.T0 <= iv.T1 ? Math.Min(max, 1.0) : Math.Max(min, 0.0))),
            _ => Seq<Interval>(),
        };
    private static Fin<IntersectionResult> EventHits(CurveIntersections? hits, Op op, Curve? source = null, Option<Line> finiteLine = default, double tolerance = 0.0) =>
        Optional(hits).ToFin(op.InvalidResult()).Map(native => (IntersectionResult)new IntersectionResult.Hits(toSeq(native.AsIterable().SelectMany(h => h switch {
            { IsPoint: true } when finiteLine.Map(l => OnFiniteLine(l, h.PointB, tolerance)).IfNone(true) => Seq(IntersectionHit.At(h.PointA)),
            { IsOverlap: true } => (finiteLine.Case switch {
                Line => SegmentInterval(h.OverlapB).Head.Map(cb => (A: new Interval(h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T0)), h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T1))), B: cb)),
                _ => Some((A: h.OverlapA, B: h.OverlapB)),
            }).Map(o => Optional(source).Map(c => IntersectionHit.Overlap(c.PointAt(o.A.T0), c.PointAt(o.A.T1), o.A, o.B, Optional(c.Trim(o.A))))
                .IfNone(IntersectionHit.Overlap(h.PointA, h.PointA2, o.A, o.B))).ToSeq(),
            _ => Seq<IntersectionHit>(),
        }))));
    private static Fin<IntersectionResult> SolvedHits(bool solved, Curve[]? curves, Point3d[]? points, IntersectionKind kind, (Context Ctx, Op Op, CancellationToken Cancel, IProgress<double>? Progress) args) =>
        (solved, args.Cancel.IsCancellationRequested) switch {
            (true, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(toSeq(curves ?? []).Map(c => IntersectionHit.Along(c, kind)) + toSeq(points ?? []).Map(static p => IntersectionHit.At(p)))),
            (false, true) => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            _ => Fin.Fail<IntersectionResult>(args.Op.InvalidResult()),
        };
    [BoundaryAdapter]
    private static Fin<CurveDeviation> CurveDeviationOf(Curve left, Curve right, Context context, Op op) =>
        Curve.GetDistancesBetweenCurves(curveA: left, curveB: right, tolerance: context.Absolute.Value, maxDistance: out double maxDist, maxDistanceParameterA: out double maxA, maxDistanceParameterB: out double maxB, minDistance: out double minDist, minDistanceParameterA: out double minA, minDistanceParameterB: out double minB) switch {
            true => (op.RequireValid(value: minDist), op.RequireValid(value: maxDist), op.RequireValid(value: left.PointAt(t: minA)), op.RequireValid(value: right.PointAt(t: minB)), op.RequireValid(value: left.PointAt(t: maxA)), op.RequireValid(value: right.PointAt(t: maxB)))
                .Apply((minD, maxD, mA, mB, xA, xB) => new CurveDeviation(MinimumDistance: minD, MinimumA: mA, MinimumB: mB, MaximumDistance: maxD, MaximumA: xA, MaximumB: xB, Tolerance: context.Absolute.Value, WithinTolerance: maxD <= context.Absolute.Value))
                .As()
                .Bind(deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                    (true, true) => Fin.Succ(deviation),
                    _ => Fin.Fail<CurveDeviation>(op.InvalidResult()),
                }),
            false => Fin.Fail<CurveDeviation>(op.InvalidResult()),
        };
    private static Seq<ResidualSample> Residuals<TP>(Seq<Point3d> points, TP primitive, Context context, Func<TP, Point3d, double> distance) where TP : notnull =>
        toSeq(points.AsIterable().Select((p, i) => distance(primitive, p) switch { double d => new ResidualSample(i, p, d, context.Absolute.Value, d <= context.Absolute.Value) }));
    private static Fin<Seq<ResidualSample>> SampleCurveAgainst<TP>(Curve curve, TP primitive, int count, Context context, Op op, Func<TP, Point3d, double> distance) where TP : notnull =>
        Fractions(count, op).Bind(fs => Optional(curve.NormalizedLengthParameters([.. fs.AsIterable()], context.Absolute.Value, context.Fractional)).ToFin(op.InvalidResult()).Map(ps => Residuals(toSeq(ps).Map(curve.PointAt), primitive, context, distance)));
    private static Fin<Seq<ResidualSample>> SampleSurfaceAgainst<TP>(Surface surface, TP primitive, int resolution, Context context, Op op, Func<TP, Point3d, double> distance) where TP : notnull =>
        (surface.Domain(0), surface.Domain(1)) switch {
            ( { IsValid: true } u, { IsValid: true } v) => Fractions(resolution, op).Map(fs => Residuals(
                fs.Map(f => u.ParameterAt(f)).Bind(pu => fs.Map(f => v.ParameterAt(f)).Map(pv => surface.PointAt(pu, pv))), primitive, context, distance)),
            _ => Fin.Fail<Seq<ResidualSample>>(op.InvalidInput()),
        };

    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> CurveInput = static (g, args) =>
        (g is Curve c ? c : g switch { Line l => (Curve?)new LineCurve(l), Polyline p => p.ToPolylineCurve(), Circle ci => new ArcCurve(ci), Arc a => new ArcCurve(a), _ => null }) switch {
            Curve native => ((args.Item1.Feature is CurveFeature.Segment or CurveFeature.SubCurve)
                ? Optional(args.Item1.Feature == CurveFeature.SubCurve ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                    Option<Curve[]> opt when opt.Case is Curve[] arr && arr.Length > 0 => Fin.Succ(toSeq(arr.Select((cc, i) => TopologyProjection.FromCurve(cc, args.Item1.Feature, ComponentIndexType.PolycurveSegment, i)))),
                    _ => Optional(native.DuplicateCurve()).ToFin(args.Item3.InvalidResult()).Map(d => Seq(TopologyProjection.FromCurve(d, args.Item1.Feature, ComponentIndexType.PolycurveSegment, 0))),
                }
                : Optional(native.DuplicateCurve()).ToFin(args.Item3.InvalidResult()).Map(d => Seq(TopologyProjection.FromCurve(d, args.Item1.Feature, ComponentIndexType.NoType, 0))))
                .Map(seq => (g is not Curve) ? Borrowed(native, (Curve _) => seq) : seq),
            _ => Fin.Fail<Seq<TopologyProjection>>(args.Item3.InvalidResult()),
        };
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> BrepEdges = static (g, args) =>
        Fin.Succ(toSeq(((Brep)g).Edges).Where(e => (args.Item1.Feature, e.Valence) switch { (CurveFeature.Edge, _) => true, (CurveFeature.Interior, EdgeAdjacency.Interior) => true, (CurveFeature.NonManifold, EdgeAdjacency.NonManifold) => true, (CurveFeature.NakedOuter, EdgeAdjacency.Naked) => toSeq(e.TrimIndices()).Exists(t => e.Brep.Trims[t].Loop.LoopType == BrepLoopType.Outer), (CurveFeature.NakedInner, EdgeAdjacency.Naked) => toSeq(e.TrimIndices()).Exists(t => e.Brep.Trims[t].Loop.LoopType == BrepLoopType.Inner), (CurveFeature.Boundary, EdgeAdjacency.Naked) => true, _ => false }).Bind(e => Optional(e.DuplicateCurve()).Map(c => TopologyProjection.FromCurve(c, args.Item1.Feature, ComponentIndexType.BrepEdge, e.EdgeIndex)).ToSeq()));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> MeshEdges = static (g, args) =>
        Fin.Succ(toSeq(Enumerable.Range(0, ((Mesh)g).TopologyEdges.Count)).Where(i => (args.Item1.Feature, ((Mesh)g).TopologyEdges.GetConnectedFaces(i).Length) switch { (CurveFeature.Edge, _) => true, (CurveFeature.Boundary, 1) => true, (CurveFeature.Interior, 2) => true, (CurveFeature.NonManifold, > 2) => true, _ => false }).Map(i => TopologyProjection.FromCurve(((Mesh)g).TopologyEdges.EdgeLine(i).ToNurbsCurve(), args.Item1.Feature, ComponentIndexType.MeshTopologyEdge, i)));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> BrepLoops = static (g, args) =>
        Fin.Succ(toSeq(((Brep)g).Loops).Where(l => (args.Item1.Feature, l.LoopType) switch { (CurveFeature.OuterLoop, BrepLoopType.Outer) => true, (CurveFeature.InnerLoop, BrepLoopType.Inner) => true, _ => false }).Bind(l => Optional(l.To3dCurve()).Map(c => TopologyProjection.FromCurve(c, args.Item1.Feature, ComponentIndexType.BrepLoop, l.LoopIndex)).ToSeq()));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> Iso = static (g, args) => g switch {
        Brep b => toSeq(b.Faces).TraverseM(f => IsoSeq(f, args.Item1.Iso.IfNone(static () => IsoStatus.X), args.Item1.Normalized.IfNone(static () => 0.5), args.Item3).Map(s => s.Map(c => TopologyProjection.FromCurve(c, args.Item1.Feature, new ComponentIndex(ComponentIndexType.BrepFace, f.FaceIndex))))).As().Map(static n => n.Bind(static s => s)),
        Surface s => IsoSeq(s, args.Item1.Iso.IfNone(static () => IsoStatus.X), args.Item1.Normalized.IfNone(static () => 0.5), args.Item3).Map(seq => seq.Map(c => TopologyProjection.FromCurve(c, args.Item1.Feature, new ComponentIndex(ComponentIndexType.NoType, 0)))),
        _ => Fin.Fail<Seq<TopologyProjection>>(args.Item3.Unsupported(g.GetType(), typeof(Curve))),
    };
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> SilhouetteH = static (g, args) => args.Item4.IsCancellationRequested
        ? Fin.Fail<Seq<TopologyProjection>>(new Fault.Cancelled())
        : g switch {
            GeometryBase native when args.Item1.Direction.IfNone(static () => Vector3d.ZAxis) is { IsValid: true } dir && !dir.IsTiny() =>
                (native switch {
                    Brep or BrepFace or Mesh or Extrusion => Fin.Succ((Geometry: native, Owned: Option<GeometryBase>.None)),
                    Surface surface => Optional(surface.ToBrep()).ToFin(args.Item3.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                    SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(args.Item3.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                    _ => Fin.Fail<(GeometryBase Geometry, Option<GeometryBase> Owned)>(args.Item3.Unsupported(native.GetType(), typeof(Curve))),
                }).Bind(shape => {
                    Fin<Seq<TopologyProjection>> result = Optional((args.Item1.Feature == CurveFeature.Draft ? Some(args.Item1.Angle.IfNone(static () => 0.0)) : None).Case switch {
                        double angle => Silhouette.ComputeDraftCurve(shape.Geometry, angle, dir, args.Item2.Absolute.Value, args.Item2.Angle.Value, args.Item4),
                        _ => Silhouette.Compute(shape.Geometry, SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, dir, args.Item2.Absolute.Value, args.Item2.Angle.Value, [], args.Item4),
                    }).ToFin(args.Item4.IsCancellationRequested ? (Error)new Fault.Cancelled() : args.Item3.InvalidResult())
                        .Map(arr => toSeq(arr).Map(sil => TopologyProjection.FromCurve(sil.Curve, args.Item1.Feature, sil.GeometryComponentIndex)));
                    _ = shape.Owned.Iter(static geom => geom.Dispose());
                    return result;
                }),
            _ => Fin.Fail<Seq<TopologyProjection>>(args.Item3.Unsupported(g.GetType(), typeof(Curve))),
        };

    private static IEnumerable<Cap> Curves<TG>(Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<TopologyProjection>>> h, params CurveFeature[] features) where TG : notnull =>
        features.Select(f => new Cap(CapTag.Curves, typeof(TG), null, f, 0, h));

    // --- [CAPABILITIES] -----------------------------------------------------------------------
    private static IEnumerable<Cap> BuildCapabilities() => [
        For(CapTag.Bounds, static (BoundingBox g, Op _) => Fin.Succ(g)), For(CapTag.Bounds, static (Box g, Op _) => Fin.Succ(g.BoundingBox)),
        For(CapTag.Bounds, static (Sphere g, Op _) => Fin.Succ(g.BoundingBox)), For(CapTag.Bounds, static (Line g, Op _) => Fin.Succ(g.BoundingBox)),
        For(CapTag.Bounds, static (Polyline g, Op _) => Fin.Succ(g.BoundingBox)), For(CapTag.Bounds, static (Circle g, Op _) => Fin.Succ(g.BoundingBox)),
        For(CapTag.Bounds, static (Arc g, Op _) => Fin.Succ(g.BoundingBox())), For(CapTag.Bounds, static (Point3d g, Op _) => Fin.Succ(new BoundingBox(g, g))),
        For(CapTag.Bounds, static (Plane _, Op op) => Fin.Fail<BoundingBox>(op.Unsupported(typeof(Plane), typeof(BoundingBox)))),
        For(CapTag.Bounds, static (Ellipse g, Op op) => Optional(g.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static c => Borrowed(c, static d => d.GetBoundingBox(true)))),
        For(CapTag.Bounds, static (Cylinder g, Op op) => Optional(g.ToBrep(true, true)).ToFin(op.InvalidResult()).Map(static b => Borrowed(b, static d => d.GetBoundingBox(true)))),
        For(CapTag.Bounds, static (Cone g, Op op) => Optional(g.ToBrep(true)).ToFin(op.InvalidResult()).Map(static b => Borrowed(b, static d => d.GetBoundingBox(true)))),
        For(CapTag.Bounds, static (Torus g, Op op) => Optional(g.ToBrep()).ToFin(op.InvalidResult()).Map(static b => Borrowed(b, static d => d.GetBoundingBox(true)))),
        For(CapTag.Bounds, static (GeometryBase g, Op op) => g.IsValid ? Fin.Succ(g.GetBoundingBox(true)) : Fin.Fail<BoundingBox>(op.InvalidInput())),
        For(CapTag.Vertices, static (Point3d g, (Context, Op) _) => Fin.Succ(Seq(g))), For(CapTag.Vertices, static (Point g, (Context, Op) _) => Fin.Succ(Seq(g.Location))),
        For(CapTag.Vertices, static (Line g, (Context, Op) _) => Fin.Succ(Seq(g.From, g.To))), For(CapTag.Vertices, static (Polyline g, (Context, Op) _) => Fin.Succ(toSeq(g))),
        For(CapTag.Vertices, static (BoundingBox g, (Context, Op) _) => Fin.Succ(toSeq(g.GetCorners()))), For(CapTag.Vertices, static (Box g, (Context, Op) _) => Fin.Succ(toSeq(g.GetCorners()))),
        For(CapTag.Vertices, static (Curve g, (Context, Op) _) => Fin.Succ(g.TryGetPolyline(out Polyline poly) ? toSeq(poly) : Seq(g.PointAtStart, g.PointAtEnd))),
        For(CapTag.Vertices, static (Brep g, (Context, Op) _) => Fin.Succ(toSeq(g.DuplicateVertices()))), For(CapTag.Vertices, static (Mesh g, (Context, Op) _) => Fin.Succ(toSeq(g.Vertices.ToPoint3dArray()))),
        For(CapTag.Vertices, static (PointCloud g, (Context, Op) _) => Fin.Succ(toSeq(g.GetPoints()))),
        For(CapTag.Vertices, static (SubD g, (Context, Op) _) => Fin.Succ(toSeq(LanguageExt.List.unfold((SubDVertex?)g.Vertices.First, static v => v switch { SubDVertex sv => Some((sv.ControlNetPoint, (SubDVertex?)sv.Next)), _ => None })))),
        For(CapTag.Vertices, static (Surface g, (Context, Op) args) => (g.Domain(0), g.Domain(1)) switch { (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(g.PointAt(u.T0, v.T0), g.PointAt(u.T1, v.T0), g.PointAt(u.T1, v.T1), g.PointAt(u.T0, v.T1))), _ => Fin.Fail<Seq<Point3d>>(args.Item2.InvalidResult()) }),
        For(CapTag.Centroid, static (Point3d g, (Context, Op) _) => Fin.Succ(g)), For(CapTag.Centroid, static (Point g, (Context, Op) _) => Fin.Succ(g.Location)),
        For(CapTag.Centroid, static (Line g, (Context, Op) _) => Fin.Succ(g.PointAt(0.5))), For(CapTag.Centroid, static (Polyline g, (Context, Op) _) => Fin.Succ(g.CenterPoint())),
        For(CapTag.Centroid, static (BoundingBox g, (Context, Op) _) => Fin.Succ(g.Center)), For(CapTag.Centroid, static (Box g, (Context, Op) _) => Fin.Succ(g.Center)),
        For(CapTag.Centroid, static (Brep g, (Context, Op) args) => MassCentroid(g, g.IsSolid, args.Item1, args.Item2)),
        For(CapTag.Centroid, static (Mesh g, (Context, Op) args) => MassCentroid(g, g.IsSolid, args.Item1, args.Item2)),
        For(CapTag.Centroid, static (Surface g, (Context, Op) args) => MassCentroid(g, g.IsSolid, args.Item1, args.Item2)),
        For(CapTag.Centroid, static (Curve g, (Context, Op) args) => (g.IsClosed, g.TryGetPlane(out Plane _, args.Item1.Absolute.Value)) switch {
            (false, _) => Optional(LengthMassProperties.Compute(g)).ToFin(args.Item2.InvalidResult()).Map(static m => Borrowed(m, static d => d.Centroid)),
            (true, true) => Optional(AreaMassProperties.Compute(g, args.Item1.Absolute.Value)).ToFin(args.Item2.InvalidResult()).Map(static m => Borrowed(m, static d => d.Centroid)),
            (true, false) => Fin.Fail<Point3d>(args.Item2.InvalidResult()),
        }),
        For(CapTag.Centroid, static (SubD g, (Context, Op) args) => Optional(g.ToBrep(SubDToBrepOptions.Default)).ToFin(args.Item2.InvalidResult()).Bind(b => Borrowed(b, (Brep d) => MassCentroid(d, d.IsSolid, args.Item1, args.Item2)))),
        For(CapTag.Closest, static (Line g, (Point3d t, Context, Op op) a) => a.t.IsValid ? Fin.Succ(new ClosestHit(g.ClosestPoint(a.t, true), Some(a.t.DistanceTo(g.ClosestPoint(a.t, true))), None, None, None)) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Closest, static (Polyline g, (Point3d t, Context, Op op) a) => a.t.IsValid ? Fin.Succ(new ClosestHit(g.ClosestPoint(a.t), Some(a.t.DistanceTo(g.ClosestPoint(a.t))), None, None, None)) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Closest, static (Curve g, (Point3d t, Context, Op op) a) => a.t.IsValid && g.ClosestPoint(a.t, out double p) ? Fin.Succ(new ClosestHit(g.PointAt(p), Some(a.t.DistanceTo(g.PointAt(p))), None, None, None)) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Closest, static (Surface g, (Point3d t, Context, Op op) a) => a.t.IsValid && g.ClosestPoint(a.t, out double u, out double v) ? Fin.Succ(new ClosestHit(g.PointAt(u, v), Some(a.t.DistanceTo(g.PointAt(u, v))), Some(g.NormalAt(u, v)), None, None)) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Closest, static (Brep g, (Point3d t, Context, Op op) a) => a.t.IsValid && g.ClosestPoint(a.t, out Point3d pt, out ComponentIndex ci, out double _, out double _, 0.0, out Vector3d n) ? Fin.Succ(new ClosestHit(pt, Some(a.t.DistanceTo(pt)), Some(n), Some(ci), None)) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Closest, static (Mesh g, (Point3d t, Context, Op op) a) => a.t.IsValid ? Optional(g.ClosestMeshPoint(a.t, 0.0)).ToFin(a.op.InvalidResult()).Map(mp => new ClosestHit(mp.Point, Some(a.t.DistanceTo(mp.Point)), Some(g.NormalAt(mp)), None, Some(mp))) : Fin.Fail<ClosestHit>(a.op.InvalidInput())),
        For(CapTag.Components, static (Mesh g, Op _) => Fin.Succ(toSeq(g.SplitDisjointPieces().Cast<GeometryBase>()))),
        For(CapTag.Components, static (GeometryBase g, Op op) => g switch { Brep b => BrepComponents(b, op), { HasBrepForm: true } => Optional(Brep.TryConvertBrep(g)).ToFin(op.InvalidResult()).Bind(c => ReferenceEquals(g, c) ? BrepComponents(c, op) : Borrowed(c, (Brep d) => BrepComponents(d, op))), _ => Fin.Fail<Seq<GeometryBase>>(op.Unsupported(g.GetType(), typeof(Seq<GeometryBase>))) }),
        For(CapTag.Domains, static (Curve g, Op _) => Fin.Succ(Seq(g.Domain))), For(CapTag.Domains, static (Surface g, Op _) => Fin.Succ(Seq(g.Domain(0), g.Domain(1)))),
        For(CapTag.IsoCurves, static (Surface g, (IsoStatus iso, double n, Op op) a) => IsoSeq(g, a.iso, a.n, a.op)),
        For(CapTag.IsoCurves, static (Brep g, (IsoStatus iso, double n, Op op) a) => toSeq(g.Faces).TraverseM(f => IsoSeq(f, a.iso, a.n, a.op).Map(static s => (IEnumerable<Curve>)s)).As().Map(static x => x.Bind(static cs => toSeq(cs)))),
        For(CapTag.ControlPoints, static (Curve g, Op op) => g is NurbsCurve nc
            ? Fin.Succ(toSeq(Enumerable.Range(0, nc.Points.Count).Select(i => nc.Points[i].Location)))
            : Optional(g.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static c => Borrowed(c, static d => toSeq(Enumerable.Range(0, d.Points.Count).Select(i => d.Points[i].Location).ToArray())))),
        For(CapTag.ControlPoints, static (Surface g, Op op) => g is NurbsSurface ns
            ? Fin.Succ(toSeq(Enumerable.Range(0, ns.Points.CountU).SelectMany(u => Enumerable.Range(0, ns.Points.CountV).Select(v => ns.Points.GetControlPoint(u, v).Location))))
            : Optional(g.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => Borrowed(s, static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray())))),
        For(CapTag.ControlPoints, static (Brep g, Op op) => toSeq(g.Faces).TraverseM(f => Optional(f.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => Borrowed(s, static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray())))).As().Map(static n => n.Bind(static p => p))),
        .. Curves<Curve>(CurveInput, CurveFeature.Input, CurveFeature.Boundary, CurveFeature.Segment, CurveFeature.SubCurve),
        .. Curves<Line>(CurveInput, CurveFeature.Input), .. Curves<Polyline>(CurveInput, CurveFeature.Input, CurveFeature.Segment),
        .. Curves<Circle>(CurveInput, CurveFeature.Input), .. Curves<Arc>(CurveInput, CurveFeature.Input),
        .. Curves<Brep>(BrepEdges, CurveFeature.Edge, CurveFeature.Boundary, CurveFeature.NakedOuter, CurveFeature.NakedInner, CurveFeature.Interior, CurveFeature.NonManifold),
        .. Curves<BrepFace>(static (g, args) => Optional(((BrepFace)g).DuplicateFace(false)).ToFin(args.Item3.InvalidResult())
            .Bind(fb => Borrowed(fb, owned => Optional(owned.DuplicateNakedEdgeCurves(true, true)).ToFin(args.Item3.InvalidResult())
                .Map(cs => toSeq(cs.Select(c => TopologyProjection.FromCurve(c, CurveFeature.Boundary, new ComponentIndex(ComponentIndexType.BrepFace, ((BrepFace)g).FaceIndex))).ToArray())))), CurveFeature.Boundary),
        .. Curves<Mesh>(MeshEdges, CurveFeature.Edge, CurveFeature.Boundary, CurveFeature.Interior, CurveFeature.NonManifold),
        .. Curves<Mesh>(static (g, args) => Optional(((Mesh)g).GetNakedEdges()).ToFin(args.Item3.InvalidResult())
            .Map(ps => toSeq(ps).Map((p, i) => TopologyProjection.FromCurve(p.ToPolylineCurve(), args.Item1.Feature, ComponentIndexType.NoType, i))), CurveFeature.NakedOuter),
        .. Curves<Brep>(BrepLoops, CurveFeature.OuterLoop, CurveFeature.InnerLoop),
        .. Curves<Brep>(Iso, CurveFeature.Iso), .. Curves<Surface>(Iso, CurveFeature.Iso),
        .. new[] { typeof(Brep), typeof(Mesh), typeof(Extrusion), typeof(Surface), typeof(SubD) }.SelectMany(t => new[] { CurveFeature.Silhouette, CurveFeature.Draft }.Select(f => new Cap(CapTag.Curves, t, null, f, 0, SilhouetteH))),
        .. Curves<Surface>(static (g, args) => Seq(IsoStatus.South, IsoStatus.East, IsoStatus.North, IsoStatus.West)
            .TraverseM(iso => Optional(((Surface)g).IsoCurve(iso)).ToFin(args.Item3.InvalidResult())).As()
            .Map(seq => seq.Map((c, i) => TopologyProjection.FromCurve(c, CurveFeature.Boundary, ComponentIndexType.NoType, i))), CurveFeature.Boundary),
        .. Curves<SubD>(static (g, args) => { _ = ((SubD)g).UpdateSurfaceMeshCache(true); return Fin.Succ(toSeq(((SubD)g).DuplicateEdgeCurves().Select((c, i) => TopologyProjection.FromCurve(c, args.Item1.Feature, ComponentIndexType.SubdEdge, i)))); }, CurveFeature.Edge, CurveFeature.Segment),
        ForPair(CapTag.Intersect, static (Line a, Plane b, (Context, Op, CancellationToken, IProgress<double>?) _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LinePlane(a, b, out double t) && t is >= 0.0 and <= 1.0 ? Seq(a.PointAt(t)) : Seq<Point3d>()))),
        ForPair(CapTag.Intersect, static (Plane a, Plane b, (Context, Op, CancellationToken, IProgress<double>?) _) => Fin.Succ((IntersectionResult)new IntersectionResult.Lines(Intersection.PlanePlane(a, b, out Line line) ? Seq(line) : Seq<Line>()))),
        ForPair(CapTag.Intersect, static (Line a, Circle b, (Context, Op, CancellationToken, IProgress<double>?) _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineCircle(a, b, out double t1, out Point3d p1, out double t2, out Point3d p2) switch {
            LineCircleIntersection.Single when t1 is >= 0.0 and <= 1.0 => Seq(p1),
            LineCircleIntersection.Multiple => Seq((T: t1, Point: p1), (T: t2, Point: p2)).Where(static p => p.T is >= 0.0 and <= 1.0).Map(static p => p.Point),
            _ => Seq<Point3d>(),
        }))),
        ForPair(CapTag.Intersect, static (Line a, Sphere b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineSphere(a, b, out Point3d p1, out Point3d p2) switch {
            LineSphereIntersection.Single when OnFiniteLine(a, p1, args.ctx.Absolute.Value) => Seq(p1),
            LineSphereIntersection.Multiple => Seq(p1, p2).Where(p => OnFiniteLine(a, p, args.ctx.Absolute.Value)),
            _ => Seq<Point3d>(),
        }))),
        ForPair(CapTag.Intersect, static (Line a, BoundingBox b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, args.ctx.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>()))),
        ForPair(CapTag.Intersect, static (Line a, Box b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, args.ctx.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>()))),
        ForPair(CapTag.Intersect, static (Curve a, Curve b, (Context ctx, Op op, CancellationToken cancel, IProgress<double>?) args) => args.cancel.IsCancellationRequested
            ? Fin.Fail<IntersectionResult>(new Fault.Cancelled())
            : Borrowed(Intersection.CurveCurve(a, b, args.ctx.Absolute.Value, args.ctx.Absolute.Value), hits => args.cancel.IsCancellationRequested ? Fin.Fail<IntersectionResult>(new Fault.Cancelled()) : EventHits(hits, args.op, a))),
        ForPair(CapTag.Intersect, static (Curve a, Plane b, (Context ctx, Op op, CancellationToken, IProgress<double>?) args) => { using CurveIntersections? h = Intersection.CurvePlane(a, b, args.ctx.Absolute.Value); return EventHits(h, args.op, a); }),
        ForPair(CapTag.Intersect, static (Curve a, Line b, (Context ctx, Op op, CancellationToken, IProgress<double>?) args) => { using CurveIntersections? h = Intersection.CurveLine(a, b, args.ctx.Absolute.Value, args.ctx.Absolute.Value); return EventHits(h, args.op, a, Some(b), args.ctx.Absolute.Value); }),
        ForPair(CapTag.Intersect, static (Curve a, Surface b, (Context ctx, Op op, CancellationToken, IProgress<double>?) args) => { using CurveIntersections? h = Intersection.CurveSurface(a, b, args.ctx.Absolute.Value, args.ctx.Absolute.Value); return EventHits(h, args.op, a); }),
        ForPair(CapTag.Intersect, static (Curve a, Brep b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.CurveBrep(a, b, args.ctx.Absolute.Value, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Overlap, args)),
        ForPair(CapTag.Intersect, static (Curve a, BrepFace b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.CurveBrepFace(a, b, args.ctx.Absolute.Value, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Overlap, args)),
        ForPair(CapTag.Intersect, static (Surface a, Surface b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.SurfaceSurface(a, b, args.ctx.Absolute.Value, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Curve, args)),
        ForPair(CapTag.Intersect, static (Brep a, Plane b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.BrepPlane(a, b, args.ctx.Absolute.Value, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Curve, args)),
        ForPair(CapTag.Intersect, static (Brep a, Surface b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.BrepSurface(a, b, args.ctx.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Curve, args)),
        ForPair(CapTag.Intersect, static (Brep a, Brep b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => SolvedHits(Intersection.BrepBrep(a, b, args.ctx.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), cs, ps, IntersectionKind.Curve, args)),
        ForPair(CapTag.Intersect, static (Mesh a, Line b, (Context, Op, CancellationToken, IProgress<double>?) _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(toSeq(Intersection.MeshLineSorted(a, b, out int[] _) ?? [])))),
        ForPair(CapTag.Intersect, static (Mesh a, Plane b, (Context ctx, Op, CancellationToken, IProgress<double>?) args) => { using MeshIntersectionCache cache = new(); Polyline[]? ps = Intersection.MeshPlane(a, cache, b, args.ctx.MeshIntersectionTolerance, true); return Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(ps).ToSeq().Bind(static h => h)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)))); }),
        ForPair(CapTag.Intersect, static (Mesh a, Mesh b, (Context ctx, Op op, CancellationToken cancel, IProgress<double>? prog) args) => {
            using TextLog log = new();
            return Intersection.MeshMesh([a, b], args.ctx.MeshIntersectionTolerance, out Polyline[] ints, true, out Polyline[] olap, false, out Mesh _, log, args.cancel, args.prog) switch {
                true => Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(ints).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)) + toSeq(Optional(olap).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Overlap)))),
                false when args.cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
                false => Fin.Fail<IntersectionResult>(args.op.InvalidResult()),
            };
        }),
        ForPair(CapTag.Deviation, static (Curve a, Curve b, (Context ctx, Op op) args) => CurveDeviationOf(left: a, right: b, context: args.ctx, op: args.op)),
        For(CapTag.Length, static (Line g, (Context, Op) _) => Fin.Succ(g.Length)), For(CapTag.Length, static (Polyline g, (Context, Op) _) => Fin.Succ(g.Length)),
        For(CapTag.Length, static (Curve g, (Context ctx, Op op) a) => g.GetLength(a.ctx.Fractional) switch { double l when RhinoMath.IsValidDouble(l) && l >= 0.0 => Fin.Succ(l), _ => Fin.Fail<double>(a.op.InvalidResult()) }),
        For(CapTag.Contains, static (Brep g, (Point3d t, Context ctx, Op op) a) => a.t.IsValid ? Fin.Succ(g.IsPointInside(a.t, a.ctx.Absolute.Value, false)) : Fin.Fail<bool>(a.op.InvalidInput())),
        For(CapTag.Contains, static (Mesh g, (Point3d t, Context ctx, Op op) a) => a.t.IsValid ? Fin.Succ(g.IsPointInside(a.t, a.ctx.Absolute.Value, false)) : Fin.Fail<bool>(a.op.InvalidInput())),
        For(CapTag.SolidOrientation, static (Brep g, Op _) => Fin.Succ((SolidOrientation)(int)g.SolidOrientation)), For(CapTag.SolidOrientation, static (Mesh g, Op _) => Fin.Succ((SolidOrientation)g.SolidOrientation())),
        For(CapTag.Faces, static (BrepFace g, (Context, Op) _) => Fin.Succ(Seq(TopologyProjection.FaceFrom(g)))),
        For(CapTag.Faces, static (GeometryBase g, (Context, Op op) a) => g switch { Brep brep => Fin.Succ(toSeq(brep.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.FaceFrom(f)).ToArray())), { HasBrepForm: true } => Optional(Brep.TryConvertBrep(g)).ToFin(a.op.InvalidResult()).Bind(b => ReferenceEquals(g, b) ? Fin.Succ(toSeq(b.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.FaceFrom(f)).ToArray())) : Borrowed(b, static (Brep d) => Fin.Succ(toSeq(d.Faces.Cast<BrepFace>().Select(static f => TopologyProjection.FaceFrom(f)).ToArray())))), _ => Fin.Fail<Seq<TopologyProjection>>(a.op.Unsupported(g.GetType(), typeof(Seq<TopologyProjection>))) }),
        ForPair(CapTag.Conformance, static (Curve g, Line p, (int n, Context ctx, Op op) a) => SampleCurveAgainst(g, p, a.n, a.ctx, a.op, static (l, pt) => pt.DistanceTo(l.ClosestPoint(pt, true)))),
        ForPair(CapTag.Conformance, static (Curve g, Circle p, (int n, Context ctx, Op op) a) => SampleCurveAgainst(g, p, a.n, a.ctx, a.op, static (ci, pt) => pt.DistanceTo(ci.ClosestPoint(pt)))),
        ForPair(CapTag.Conformance, static (Curve g, Arc p, (int n, Context ctx, Op op) a) => SampleCurveAgainst(g, p, a.n, a.ctx, a.op, static (ar, pt) => pt.DistanceTo(ar.ClosestPoint(pt)))),
        ForPair(CapTag.Conformance, static (Surface g, Plane p, (int n, Context ctx, Op op) a) => SampleSurfaceAgainst(g, p, a.n, a.ctx, a.op, static (pl, pt) => Math.Abs(pl.DistanceTo(pt)))),
        ForPair(CapTag.Conformance, static (Surface g, Sphere p, (int n, Context ctx, Op op) a) => SampleSurfaceAgainst(g, p, a.n, a.ctx, a.op, static (sp, pt) => pt.DistanceTo(sp.ClosestPoint(pt)))),
        Probe<Brep, Box>(100, global::Rasm.Domain.Kind.Box, static (g, c) => g.IsBox(c.Absolute.Value) && g.Faces[0].UnderlyingSurface().TryGetPlane(out Plane pl, c.Absolute.Value) && g.GetBoundingBox(pl, out Box bx) is { IsValid: true } ? Some(bx) : Option<Box>.None),
        Probe<Curve, Line>(95, global::Rasm.Domain.Kind.Line, static (g, c) => g.IsLinear(c.Absolute.Value) ? Some(new Line(g.PointAtStart, g.PointAtEnd)) : Option<Line>.None),
        Probe<Curve, Circle>(94, global::Rasm.Domain.Kind.Circle, static (g, c) => g.TryGetCircle(out Circle x, c.Absolute.Value) ? Some(x) : Option<Circle>.None),
        Probe<Curve, Arc>(93, global::Rasm.Domain.Kind.Arc, static (g, c) => g.TryGetArc(out Arc x, c.Absolute.Value) ? Some(x) : Option<Arc>.None),
        Probe<Curve, Ellipse>(92, global::Rasm.Domain.Kind.Ellipse, static (g, c) => g.TryGetEllipse(out Ellipse x, c.Absolute.Value) ? Some(x) : Option<Ellipse>.None),
        Probe<Curve, Polyline>(90, global::Rasm.Domain.Kind.Polyline, static (g, _) => g.TryGetPolyline(out Polyline x) ? Some(x) : Option<Polyline>.None),
        Probe<Brep, Plane>(70, global::Rasm.Domain.Kind.Surface, static (g, c) => g is { IsSurface: true } b && b.Surfaces[0].TryGetPlane(out Plane x, c.Absolute.Value) ? Some(x) : Option<Plane>.None),
        Probe<Surface, Plane>(60, global::Rasm.Domain.Kind.Plane, static (g, c) => g.TryGetPlane(out Plane x, c.Absolute.Value) ? Some(x) : Option<Plane>.None),
        .. SymProbe<Sphere>(50, global::Rasm.Domain.Kind.Sphere, static (s, c) => s.TryGetSphere(out Sphere x, c.Absolute.Value) ? Some(x) : Option<Sphere>.None),
        .. SymProbe<Cylinder>(49, global::Rasm.Domain.Kind.Cylinder, static (s, c) => s.TryGetCylinder(out Cylinder x, c.Absolute.Value) ? Some(x) : Option<Cylinder>.None),
        .. SymProbe<Cone>(48, global::Rasm.Domain.Kind.Cone, static (s, c) => s.TryGetCone(out Cone x, c.Absolute.Value) ? Some(x) : Option<Cone>.None),
        .. SymProbe<Torus>(47, global::Rasm.Domain.Kind.Torus, static (s, c) => s.TryGetTorus(out Torus x, c.Absolute.Value) ? Some(x) : Option<Torus>.None),
        Probe<Extrusion, Brep>(10, global::Rasm.Domain.Kind.Brep, static (g, _) => Optional(g.ToBrep())),
    ];
}

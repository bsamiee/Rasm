using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshFaceSample(int Face, double Value);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSample(MeshSampleKind Kind, int Value);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Meshes : IAspect {
    public sealed record ValidityCase : Meshes; public sealed record CountsCase : Meshes; public sealed record DefectsCase : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes; public sealed record AtFaceCase(int? Value) : Meshes;
    public static Meshes Validity => new ValidityCase(); public static Meshes Counts => new CountsCase(); public static Meshes Defects => new DefectsCase();
    public static Meshes FaceQuality(MeshMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshMetric.AspectRatio);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
    private static readonly Op ValidityKey = Op.Of(name: "MeshValidity");
    private static readonly Op CountsKey = Op.Of(name: "MeshCounts");
    private static readonly Op DefectsKey = Op.Of(name: "MeshDefects");
    private static readonly Op FaceMetricKey = Op.Of(name: nameof(MeshMetric));
    private static readonly Op AtFaceKey = Op.Of(name: "MeshAtFace");
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        validityCase: static _ => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: ValidityKey, source: Analyze.MeshValidity),
        countsCase: static _ => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: CountsKey, source: Analyze.MeshCounts),
        defectsCase: static _ => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: DefectsKey, source: Analyze.MeshDefects),
        faceQualityCase: static fq => Analyze.MeshLift<TGeometry, TOut, MeshFaceSample>(key: FaceMetricKey, source: Analyze.MeshMetric(metric: fq.Metric)),
        atFaceCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtFaceKey, source: Analyze.MeshAtFace(index: at.Value)));
}

[SmartEnum<int>]
public partial class MeshSampleKind {
    public static readonly MeshSampleKind None = new(key: 0, isValidity: false, isCount: false, isDefect: false, sample: static (_, _) => 0), Valid = new(key: 1, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => m.IsValid ? 1 : 0), Closed = new(key: 2, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => m.IsClosed ? 1 : 0);
    public static readonly MeshSampleKind Oriented = new(key: 3, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => (m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented) ? 1 : 0), Solid = new(key: 4, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => m.IsSolid ? 1 : 0);
    public static readonly MeshSampleKind Manifold = new(key: 5, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool _) ? 1 : 0), BoundaryFree = new(key: 6, isValidity: true, isCount: false, isDefect: false, sample: static (m, _) => (m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary) ? 1 : 0);
    public static readonly MeshSampleKind Vertices = new(key: 10, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.Vertices.Count), Faces = new(key: 11, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.Faces.Count), Triangles = new(key: 12, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.Faces.TriangleCount), Quads = new(key: 13, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.Faces.QuadCount);
    public static readonly MeshSampleKind Edges = new(key: 14, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.TopologyEdges.Count), Euler = new(key: 15, isValidity: false, isCount: true, isDefect: false, sample: static (m, _) => m.TopologyVertices.Count - m.TopologyEdges.Count + m.Faces.Count);
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, isValidity: false, isCount: false, isDefect: true, sample: static (_, p) => p.DegenerateFaceCount), DisjointMeshes = new(key: 21, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.DisjointMeshCount), DuplicateFaces = new(key: 22, isValidity: false, isCount: false, isDefect: true, sample: static (_, p) => p.DuplicateFaceCount);
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.ExtremelyShortEdgeCount), InvalidNgons = new(key: 24, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.InvalidNgonCount), NakedEdges = new(key: 25, isValidity: false, isCount: false, isDefect: true, sample: static (_, p) => p.NakedEdgeCount), NonManifoldEdges = new(key: 26, isValidity: false, isCount: false, isDefect: true, sample: static (_, p) => p.NonManifoldEdgeCount);
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.NonUnitVectorNormalCount), RandomFaceNormals = new(key: 28, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.RandomFaceNormalCount), SelfIntersectingPairs = new(key: 29, isValidity: false, isCount: false, isDefect: true, sample: static (_, p) => p.SelfIntersectingPairsCount), UnusedVertices = new(key: 30, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.UnusedVertexCount);
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.VertexFaceNormalsDifferCount), ZeroLengthNormals = new(key: 32, isValidity: false, isCount: false, isDefect: false, sample: static (_, p) => p.ZeroLengthNormalCount);
    public bool IsValidity { get; }
    public bool IsCount { get; }
    public bool IsDefect { get; }
    [UseDelegateFromConstructor] internal partial int Sample(Mesh mesh, MeshCheckParameters parameters);
    internal static Seq<MeshSampleKind> Validity => toSeq(Items).Filter(static kind => kind.IsValidity);
    internal static Seq<MeshSampleKind> Counts => toSeq(Items).Filter(static kind => kind.IsCount);
    internal static Seq<MeshSampleKind> Defects => toSeq(Items).Filter(static kind => kind.IsDefect);
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshMetric {
    private static readonly Op MetricKey = Op.Of(name: nameof(MeshMetric));
    public static readonly MeshMetric None = new(key: 0, sample: static _ => Fin.Fail<double>(MetricKey.InvalidInput())), AspectRatio = new(key: 1, sample: static projection => projection.OnMeshFace<double>(static (mesh, face) => Fin.Succ(mesh.Faces.GetFaceAspectRatio(index: face))));
    public static readonly MeshMetric Area = new(key: 2, sample: FaceArea), Perimeter = new(key: 3, sample: FacePerimeter), Skewness = new(key: 4, sample: FaceSkewness), DihedralAngle = new(key: 5, sample: FaceMaxDihedral);
    private readonly Func<TopologyProjection, Fin<double>> sample;
    internal Fin<MeshFaceSample> Sample(Mesh? mesh, int face) =>
        Fin.Succ((Mesh: mesh, Face: face, Sample: sample))
            .Bind(static state => TopologyProjection.MeshFace(mesh: state.Mesh, face: state.Face)
                .Map(projection => (state.Face, state.Sample, Projection: projection)))
            .Bind(static state => state.Sample(arg: state.Projection)
                .Map(value => (state.Face, Value: value)))
            .Bind(static state => state switch {
                (Face: >= 0, Value: double value) when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(new MeshFaceSample(Face: state.Face, Value: value)),
                _ => Fin.Fail<MeshFaceSample>(MetricKey.InvalidResult()),
            });
    private static Fin<double> FaceArea(TopologyProjection projection) =>
        projection.Vertices.Bind(static v => v.Count >= 3
            ? Fin.Succ(0.5 * Enumerable.Range(start: 1, count: v.Count - 2).Sum(i => Vector3d.CrossProduct(a: v[i] - v[0], b: v[i + 1] - v[0]).Length))
            : Fin.Fail<double>(MetricKey.InvalidResult()));
    private static Fin<double> FacePerimeter(TopologyProjection projection) =>
        projection.Vertices.Map(static v => v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d));
    private static Fin<double> FaceSkewness(TopologyProjection projection) =>
        projection.Vertices.Bind(static v => v.Count switch {
            < 3 => Fin.Fail<double>(MetricKey.InvalidResult()),
            int n => v.Map((vertex, i) => Vector3d.VectorAngle(a: v[(i + n - 1) % n] - vertex, b: v[(i + 1) % n] - vertex))
                .Fold(initialState: Fin.Succ((Max: 0.0, Ideal: (n - 2) * Math.PI / n)), f: static (state, angle) => state.Bind(s => RhinoMath.IsValidDouble(x: angle)
                    ? Fin.Succ((Math.Max(val1: s.Max, val2: Math.Max(val1: (angle - s.Ideal) / (Math.PI - s.Ideal), val2: (s.Ideal - angle) / s.Ideal)), s.Ideal))
                    : Fin.Fail<(double Max, double Ideal)>(MetricKey.InvalidResult())))
                .Map(static state => state.Max),
        });
    private static Fin<double> FaceMaxDihedral(TopologyProjection projection) =>
        projection.Normal.Bind(normal => normal.IsValid switch {
            false => Fin.Succ(0.0),
            true => projection.OnMeshFace<double>((mesh, face) => toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))
                .Fold(initialState: Fin.Succ((Max: 0.0, Mesh: mesh, Normal: normal)), f: static (state, other) => state.Bind(s => TopologyProjection.MeshFace(mesh: s.Mesh, face: other)
                    .Bind(static otherProjection => otherProjection.Normal)
                    .Map(neighbour => neighbour.IsValid switch {
                        true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal),
                        false => s,
                    })))
                .Map(static state => state.Max)),
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull => Aspect<Meshes, TGeometry, TOut>(aspect: aspect);
    public static global::Rasm.Analysis.Operation<Mesh, bool> IsManifold {
        get { Op key = Op.Of(); return global::Rasm.Analysis.Operation<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => op.Accept(value: geometry.IsManifold()).ToEff()); }
    }
    public static global::Rasm.Analysis.Operation<Mesh, bool> NakedPointStatus {
        get { Op key = Op.Of(); return global::Rasm.Analysis.Operation<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => op.Accept(values: geometry.GetNakedEdgePointStatus()).ToEff()); }
    }
    public static global::Rasm.Analysis.Operation<Mesh, MeshCheckParameters> MeshCheck {
        get {
            Op key = Op.Of();
            return global::Rasm.Analysis.Operation<Mesh, MeshCheckParameters>.Build(
                key: key, state: key,
                evaluator: static (op, geometry) => from parameters in Requirement.MeshReport(mesh: geometry, check: op.ToString()).ToEff()
                                                    from result in op.Accept(value: parameters).ToEff()
                                                    select result);
        }
    }
    public static global::Rasm.Analysis.Operation<Mesh, MeshFaceSample> MeshMetric(Rasm.Analysis.MeshMetric? metric) {
        Op key = Op.Of();
        return Optional(metric).Filter(static candidate => !candidate.Equals(Rasm.Analysis.MeshMetric.None)).Case switch {
            Rasm.Analysis.MeshMetric active => global::Rasm.Analysis.Operation<Mesh, MeshFaceSample>.Build(
                key: key, state: (Key: key, Metric: active), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => state.Metric.Sample(mesh: geometry, face: face)).As().ToEff()),
            _ => global::Rasm.Analysis.Operation<Mesh, MeshFaceSample>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    public static global::Rasm.Analysis.Operation<Mesh, MeshSample> MeshValidity {
        get {
            Op key = Op.Of();
            return global::Rasm.Analysis.Operation<Mesh, MeshSample>.Build(key: key, state: MeshSampleKind.Validity, evaluator: static (kinds, geometry) => Fin.Succ(kinds.Map(kind => new MeshSample(Kind: kind, Value: kind.Sample(mesh: geometry, parameters: default)))).ToEff());
        }
    }
    public static global::Rasm.Analysis.Operation<Mesh, MeshSample> MeshCounts {
        get {
            Op key = Op.Of();
            return global::Rasm.Analysis.Operation<Mesh, MeshSample>.Build(
                key: key, state: MeshSampleKind.Counts,
                evaluator: static (kinds, geometry) => Fin.Succ(kinds.Map(kind => new MeshSample(Kind: kind, Value: kind.Sample(mesh: geometry, parameters: default)))).ToEff());
        }
    }
    public static global::Rasm.Analysis.Operation<Mesh, MeshSample> MeshDefects {
        get {
            Op key = Op.Of();
            return global::Rasm.Analysis.Operation<Mesh, MeshSample>.Build(
                key: key, state: (Key: key, Kinds: MeshSampleKind.Defects),
                evaluator: static (state, geometry) => from parameters in MeshCheck.Apply(geometry: geometry)
                                                       from head in parameters.Head.ToFin(state.Key.InvalidResult()).ToEff()
                                                       select state.Kinds.Map(kind => new MeshSample(Kind: kind, Value: kind.Sample(mesh: geometry, parameters: head))));
        }
    }
    public static global::Rasm.Analysis.Operation<Mesh, TopologyProjection> MeshAtFace(int? index = null) {
        Op key = Op.Of();
        return global::Rasm.Analysis.Operation<Mesh, TopologyProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => geometry.Faces.Count switch {
                0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()).ToEff(),
                int count when state.Selector is int selected && (selected < 0 || selected >= count) => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()).ToEff(),
                _ => TopologyProjection.MeshFace(mesh: geometry, face: state.Selector ?? 0)
                    .Bind(projection => state.Key.Accept(value: projection))
                    .ToEff(),
            });
    }
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> MeshLift<TGeometry, TOut, TValue>(Op key, global::Rasm.Analysis.Operation<Mesh, TValue> source) where TGeometry : notnull =>
        Native<TGeometry, TOut, Mesh, TValue, global::Rasm.Analysis.Operation<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: mesh));
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
                true => (op.AcceptOptional(values: perforations), op.AcceptOptional(values: overlaps))
                    .Apply((left, right) => left + right)
                    .As(),
                false when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<Polyline>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Polyline>>(op.InvalidResult()),
            };
    }
}

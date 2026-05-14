namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)] public readonly record struct MeshFaceSample(int Face, double Value);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Meshes : IAspect {
    public sealed record ValidityBundleCase : Meshes; public sealed record StatsBundleCase : Meshes; public sealed record DefectsBundleCase : Meshes;
    public sealed record FaceQualityCase(MeshFaceMetric Metric) : Meshes; public sealed record AtFaceCase(int? Value) : Meshes;
    public static Meshes ValidityBundle => new ValidityBundleCase(); public static Meshes StatsBundle => new StatsBundleCase(); public static Meshes DefectsBundle => new DefectsBundleCase();
    public static Meshes FaceQuality(MeshFaceMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshFaceMetric.AspectRatio);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
    private static readonly Op ValidityBundleKey = Op.Of(name: "MeshValidityBundle");
    private static readonly Op StatsBundleKey = Op.Of(name: "MeshStatsBundle");
    private static readonly Op DefectsBundleKey = Op.Of(name: "MeshDefectsBundle");
    private static readonly Op FaceMetricKey = Op.Of(name: nameof(MeshFaceMetric));
    private static readonly Op AtFaceKey = Op.Of(name: "MeshAtFace");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        validityBundleCase: static _ => Analyze.MeshLift<TGeometry, TOut, bool>(key: ValidityBundleKey, source: Analyze.MeshValidityBundle),
        statsBundleCase: static _ => Analyze.MeshLift<TGeometry, TOut, int>(key: StatsBundleKey, source: Analyze.MeshStatsBundle),
        defectsBundleCase: static _ => Analyze.MeshLift<TGeometry, TOut, int>(key: DefectsBundleKey, source: Analyze.MeshDefectsBundle),
        faceQualityCase: static fq => Analyze.MeshLift<TGeometry, TOut, MeshFaceSample>(key: FaceMetricKey, source: Analyze.MeshFaceMetric(metric: fq.Metric)),
        atFaceCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtFaceKey, source: Analyze.MeshAtFace(index: at.Value)));
}

[SmartEnum<int>]
public partial class MeshCheckCount {
    public static readonly MeshCheckCount None = new(key: 0, get: static _ => 0);
    public static readonly MeshCheckCount DegenerateFaces = new(key: 1, get: static p => p.DegenerateFaceCount);
    public static readonly MeshCheckCount DisjointMeshes = new(key: 2, get: static p => p.DisjointMeshCount);
    public static readonly MeshCheckCount DuplicateFaces = new(key: 3, get: static p => p.DuplicateFaceCount);
    public static readonly MeshCheckCount ExtremelyShortEdges = new(key: 4, get: static p => p.ExtremelyShortEdgeCount);
    public static readonly MeshCheckCount InvalidNgons = new(key: 5, get: static p => p.InvalidNgonCount);
    public static readonly MeshCheckCount NakedEdges = new(key: 6, get: static p => p.NakedEdgeCount);
    public static readonly MeshCheckCount NonManifoldEdges = new(key: 7, get: static p => p.NonManifoldEdgeCount);
    public static readonly MeshCheckCount NonUnitVectorNormals = new(key: 8, get: static p => p.NonUnitVectorNormalCount);
    public static readonly MeshCheckCount RandomFaceNormals = new(key: 9, get: static p => p.RandomFaceNormalCount);
    public static readonly MeshCheckCount SelfIntersectingPairs = new(key: 10, get: static p => p.SelfIntersectingPairsCount);
    public static readonly MeshCheckCount UnusedVertices = new(key: 11, get: static p => p.UnusedVertexCount);
    public static readonly MeshCheckCount VertexFaceNormalsDiffer = new(key: 12, get: static p => p.VertexFaceNormalsDifferCount);
    public static readonly MeshCheckCount ZeroLengthNormals = new(key: 13, get: static p => p.ZeroLengthNormalCount);
    private readonly Func<MeshCheckParameters, int> get;
    public int Get(MeshCheckParameters parameters) => get(arg: parameters);
    internal static IEnumerable<MeshCheckCount> Defects => [DegenerateFaces, DuplicateFaces, NakedEdges, NonManifoldEdges, SelfIntersectingPairs];
}

[SmartEnum<int>]
public sealed partial class MeshFaceMetric {
    public static readonly MeshFaceMetric None = new(key: 0, sample: static _ => Fin.Fail<double>(Op.Of(name: nameof(MeshFaceMetric)).InvalidInput()));
    public static readonly MeshFaceMetric AspectRatio = new(key: 1, sample: static projection => Fin.Succ(projection.Mesh.Faces.GetFaceAspectRatio(index: projection.Face)));
    public static readonly MeshFaceMetric Area = new(key: 2, sample: FaceArea);
    public static readonly MeshFaceMetric Perimeter = new(key: 3, sample: FacePerimeter);
    public static readonly MeshFaceMetric Skewness = new(key: 4, sample: FaceSkewness);
    public static readonly MeshFaceMetric DihedralAngle = new(key: 5, sample: FaceMaxDihedral);
    private readonly Func<TopologyProjection, Fin<double>> sample;
    public Fin<double> Sample(Mesh mesh, int face) {
        ArgumentNullException.ThrowIfNull(argument: mesh);
        return TopologyProjection.MeshFace(mesh: mesh, face: face)
            .Bind(projection => sample(arg: projection));
    }
    private static Fin<double> FaceArea(TopologyProjection projection) =>
        projection.Vertices.Map(vertices => vertices switch {
            Seq<Point3d> v when v.Count == 4 => 0.5 * (Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length + Vector3d.CrossProduct(a: v[2] - v[0], b: v[3] - v[0]).Length),
            Seq<Point3d> v => 0.5 * Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length,
        });
    private static Fin<double> FacePerimeter(TopologyProjection projection) =>
        projection.Vertices.Map(static v => v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d));
    private static Fin<double> FaceSkewness(TopologyProjection projection) =>
        projection.Vertices.Map(static v => (Ideal: v.Count == 4 ? Math.PI / 2.0 : Math.PI / 3.0, Vertices: v))
            .Map(static state => state.Vertices.Map((vertex, i) => Vector3d.VectorAngle(a: state.Vertices[(i + state.Vertices.Count - 1) % state.Vertices.Count] - vertex, b: state.Vertices[(i + 1) % state.Vertices.Count] - vertex))
                .Fold(initialState: 0.0, f: (acc, angle) => Math.Max(val1: acc, val2: Math.Max(val1: (angle - state.Ideal) / (Math.PI - state.Ideal), val2: (state.Ideal - angle) / state.Ideal))));
    private static Fin<double> FaceMaxDihedral(TopologyProjection projection) =>
        projection.Normal.Bind(normal => normal.IsValid switch {
            false => Fin.Succ(0.0),
            true => toSeq(projection.Mesh.TopologyEdges.GetEdgesForFace(faceIndex: projection.Face))
                .Bind(edge => toSeq(projection.Mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge)).Filter(other => other != projection.Face))
                .Fold(initialState: Fin.Succ((Max: 0.0, projection.Mesh, Normal: normal)), f: static (state, other) => state.Bind(s => TopologyProjection.MeshFace(mesh: s.Mesh, face: other)
                    .Bind(static otherProjection => otherProjection.Normal)
                    .Map(neighbour => neighbour.IsValid switch {
                        true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal),
                        false => s,
                    })))
                .Map(static state => state.Max),
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull => Aspect<Meshes, TGeometry, TOut>(aspect: aspect);
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
    public static Query<Mesh, MeshFaceSample> MeshFaceMetric(MeshFaceMetric? metric) {
        Op key = Op.Of();
        return Optional(metric).Filter(static candidate => !candidate.Equals(Rasm.Analysis.MeshFaceMetric.None)).Case switch {
            MeshFaceMetric active => Query<Mesh, MeshFaceSample>.Build(
                key: key, state: (Key: key, Metric: active), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) => toSeq(Enumerable.Range(start: 0, count: geometry.Faces.Count))
                    .TraverseM(face => state.Metric.Sample(mesh: geometry, face: face)
                        .Bind(v => RhinoMath.IsValidDouble(x: v) && v >= 0.0
                            ? Fin.Succ(new MeshFaceSample(Face: face, Value: v))
                            : Fin.Fail<MeshFaceSample>(state.Key.InvalidResult()))).As().ToEff()),
            _ => Query<Mesh, MeshFaceSample>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    public static Query<Mesh, bool> MeshValidityBundle {
        get {
            Op key = Op.Of();
            return Query<Mesh, bool>.Build(key: key, state: key, evaluator: static (op, geometry) => {
                bool manifold = geometry.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool boundary);
                return Many(key: op, values: new[] { geometry.IsValid, geometry.IsClosed, oriented, geometry.IsSolid, manifold, !boundary }).ToEff();
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
    public static Query<Mesh, TopologyProjection> MeshAtFace(int? index = null) {
        Op key = Op.Of();
        return Query<Mesh, TopologyProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => geometry.Faces.Count switch {
                0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()).ToEff(),
                int count when state.Selector is int selected && (selected < 0 || selected >= count) => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()).ToEff(),
                _ => TopologyProjection.MeshFace(mesh: geometry, face: state.Selector ?? 0)
                    .Bind(projection => One(key: state.Key, value: projection))
                    .ToEff(),
            });
    }
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
}

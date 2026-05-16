using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshMetricSample(ComponentIndex Source, double Value);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSample(MeshSampleKind Kind, int Value);
public enum MeshSampleCategory { None, Validity, Count, Defect, Quality }

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Meshes : IAspect {
    public sealed record SamplesCase(MeshSampleCategory Category) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record AtFaceCase(int? Value) : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    public static Meshes Validity => new SamplesCase(Category: MeshSampleCategory.Validity);
    public static Meshes Counts => new SamplesCase(Category: MeshSampleCategory.Count);
    public static Meshes Defects => new SamplesCase(Category: MeshSampleCategory.Defect);
    public static Meshes Quality => new SamplesCase(Category: MeshSampleCategory.Quality);
    public static Meshes FaceQuality(MeshMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshMetric.AspectRatio);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
    public static Meshes NakedEdges => new NakedEdgesCase();
    public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
    private static readonly Op SamplesKey = Op.Of(name: "MeshSamples");
    private static readonly Op FaceMetricKey = Op.Of(name: nameof(MeshMetric));
    private static readonly Op AtFaceKey = Op.Of(name: "MeshAtFace");
    private static readonly Op NakedEdgesKey = Op.Of(name: "MeshNakedEdges");
    private static readonly Op OutlineKey = Op.Of(name: "MeshOutline");
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        samplesCase: static s => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: SamplesKey, source: Analyze.MeshSamples(category: s.Category)),
        faceQualityCase: static fq => Analyze.MeshLift<TGeometry, TOut, MeshMetricSample>(key: FaceMetricKey, source: Analyze.MeshMetric(metric: fq.Metric)),
        atFaceCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtFaceKey, source: Analyze.MeshAtFace(index: at.Value)),
        nakedEdgesCase: static _ => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: NakedEdgesKey, source: Analyze.MeshNakedEdges),
        outlineCase: static o => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: OutlineKey, source: Analyze.MeshOutline(plane: o.Plane)));
}

[SmartEnum<int>]
public sealed partial class MeshSampleKind {
    public static readonly MeshSampleKind None = new(key: 0, category: MeshSampleCategory.None, sample: static (_, _) => Fin.Succ(0)), Valid = new(key: 1, category: MeshSampleCategory.Validity, sample: static (m, _) => Fin.Succ(m.IsValid ? 1 : 0)), Closed = new(key: 2, category: MeshSampleCategory.Validity, sample: static (m, _) => Fin.Succ(m.IsClosed ? 1 : 0));
    public static readonly MeshSampleKind Oriented = new(key: 3, category: MeshSampleCategory.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented) ? 1 : 0)), Solid = new(key: 4, category: MeshSampleCategory.Validity, sample: static (m, _) => Fin.Succ(m.IsSolid ? 1 : 0));
    public static readonly MeshSampleKind Manifold = new(key: 5, category: MeshSampleCategory.Validity, sample: static (m, _) => Analyze.ManifoldOf(geometry: m, op: ManifoldKey).Map(static value => value ? 1 : 0)), BoundaryFree = new(key: 6, category: MeshSampleCategory.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary) ? 1 : 0));
    public static readonly MeshSampleKind Vertices = new(key: 10, category: MeshSampleCategory.Count, sample: static (m, _) => Fin.Succ(m.Vertices.Count)), Faces = new(key: 11, category: MeshSampleCategory.Count, sample: static (m, _) => Fin.Succ(m.Faces.Count)), Triangles = new(key: 12, category: MeshSampleCategory.Count, sample: static (m, _) => Fin.Succ(m.Faces.TriangleCount)), Quads = new(key: 13, category: MeshSampleCategory.Count, sample: static (m, _) => Fin.Succ(m.Faces.QuadCount));
    public static readonly MeshSampleKind Edges = new(key: 14, category: MeshSampleCategory.Count, sample: static (m, _) => Fin.Succ(m.TopologyEdges.Count)), Euler = new(key: 15, category: MeshSampleCategory.Count, sample: static (m, _) => Analyze.EulerOf(geometry: m, op: EulerKey));
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.DegenerateFaceCount)), DisjointMeshes = new(key: 21, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.DisjointMeshCount)), DuplicateFaces = new(key: 22, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.DuplicateFaceCount));
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.ExtremelyShortEdgeCount)), InvalidNgons = new(key: 24, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.InvalidNgonCount)), NakedEdges = new(key: 25, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.NakedEdgeCount)), NonManifoldEdges = new(key: 26, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.NonManifoldEdgeCount));
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.NonUnitVectorNormalCount)), RandomFaceNormals = new(key: 28, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.RandomFaceNormalCount)), SelfIntersectingPairs = new(key: 29, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.SelfIntersectingPairsCount)), UnusedVertices = new(key: 30, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.UnusedVertexCount));
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.VertexFaceNormalsDifferCount)), ZeroLengthNormals = new(key: 32, category: MeshSampleCategory.Defect, sample: static (_, p) => Fin.Succ(p.ZeroLengthNormalCount));
    public static readonly MeshSampleKind MaximumValence = new(key: 40, category: MeshSampleCategory.Quality, sample: static (m, _) => Fin.Succ(Valences(mesh: m).Fold(0, Math.Max))), MinimumValence = new(key: 41, category: MeshSampleCategory.Quality, sample: static (m, _) => Valences(mesh: m) switch { Seq<int> v when !v.IsEmpty => Fin.Succ(v.Fold(v.Head.IfNone(0), Math.Min)), _ => Fin.Succ(0) });
    public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, category: MeshSampleCategory.Quality, sample: static (m, _) => Analyze.BoundaryLoopsOf(geometry: m, op: BoundaryLoopsKey)), Genus = new(key: 43, category: MeshSampleCategory.Quality, sample: static (m, _) => Analyze.GenusOf(geometry: m, op: GenusKey));
    private static readonly Op ManifoldKey = Op.Of(name: "MeshManifold");
    private static readonly Op EulerKey = Op.Of(name: "MeshEuler");
    private static readonly Op BoundaryLoopsKey = Op.Of(name: "MeshBoundaryLoops");
    private static readonly Op GenusKey = Op.Of(name: "MeshGenus");
    private static Seq<int> Valences(Mesh mesh) =>
        toSeq(Enumerable.Range(0, mesh.TopologyVertices.Count).Select(mesh.TopologyVertices.ConnectedEdgesCount));
    internal MeshSampleCategory Category { get; }
    [UseDelegateFromConstructor] internal partial Fin<int> Sample(Mesh mesh, MeshCheckParameters parameters);
    internal static Seq<MeshSampleKind> Validity => toSeq(Items).Filter(static kind => kind.Category == MeshSampleCategory.Validity);
    internal static Seq<MeshSampleKind> Counts => toSeq(Items).Filter(static kind => kind.Category == MeshSampleCategory.Count);
    internal static Seq<MeshSampleKind> Defects => toSeq(Items).Filter(static kind => kind.Category == MeshSampleCategory.Defect);
    internal static Seq<MeshSampleKind> Quality => toSeq(Items).Filter(static kind => kind.Category == MeshSampleCategory.Quality);
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshMetric {
    private static readonly Op MetricKey = Op.Of(name: nameof(MeshMetric));
    public static readonly MeshMetric None = new(key: 0, sample: static _ => Fin.Fail<double>(MetricKey.InvalidInput())), AspectRatio = new(key: 1, sample: FaceAspectRatio);
    public static readonly MeshMetric Area = new(key: 2, sample: FaceArea), Perimeter = new(key: 3, sample: FacePerimeter), Skewness = new(key: 4, sample: FaceSkewness), DihedralAngle = new(key: 5, sample: FaceMaxDihedral);
    private readonly Func<TopologyProjection, Fin<double>> sample;
    internal Fin<MeshMetricSample> Sample(Mesh? mesh, MeshNgon polygon) =>
        Fin.Succ((Mesh: mesh, Polygon: polygon, Sample: sample))
            .Bind(static state => TopologyProjection.OfMesh(mesh: state.Mesh, polygon: state.Polygon)
                .Map(projection => (state.Sample, Projection: projection)))
            .Bind(static state => state.Sample(arg: state.Projection)
                .Map(value => (state.Projection.Source, Value: value)))
            .Bind(static state => state switch {
                (Source: { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 }, Value: double value) when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(new MeshMetricSample(Source: state.Source, Value: value)),
                _ => Fin.Fail<MeshMetricSample>(MetricKey.InvalidResult()),
            });
    private static Fin<double> FaceAspectRatio(TopologyProjection projection) =>
        ((projection.Value, projection.Source) switch {
            (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index }) when index >= 0 && index < mesh.Faces.Count => Fin.Succ(mesh.Faces.GetFaceAspectRatio(index: index)),
            _ => Fin.Fail<double>(MetricKey.InvalidInput()),
        })
            .BindFail(_ => projection.Vertices.Bind(static v => v.Count >= 3
            ? v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Filter(static length => length > RhinoMath.ZeroTolerance) switch {
                Seq<double> lengths when !lengths.IsEmpty => lengths.Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0), f: static (range, length) => (Math.Min(val1: range.Min, val2: length), Math.Max(val1: range.Max, val2: length))) switch {
                    (double min, double max) when RhinoMath.IsValidDouble(x: min) && min > RhinoMath.ZeroTolerance && RhinoMath.IsValidDouble(x: max) => Fin.Succ(max / min),
                    _ => Fin.Fail<double>(MetricKey.InvalidResult()),
                },
                _ => Fin.Fail<double>(MetricKey.InvalidResult()),
            }
            : Fin.Fail<double>(MetricKey.InvalidResult())));
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
            true => (projection.Value, projection.Source) switch {
                (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face }) when face >= 0 && face < mesh.Faces.Count =>
                    toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))
                        .Fold(initialState: Fin.Succ((Max: 0.0, Mesh: mesh, Normal: normal)), f: static (state, other) => state.Bind(s => TopologyProjection.OfMesh(mesh: s.Mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, other))
                            .Bind(static otherProjection => otherProjection.Normal)
                            .Map(neighbour => neighbour.IsValid switch {
                                true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal),
                                false => s,
                            })))
                        .Map(static state => state.Max),
                (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon }) when ngon >= 0 && ngon < mesh.Ngons.Count =>
                    Optional(mesh.Ngons[ngon].FaceIndexList()).ToFin(MetricKey.InvalidResult())
                        .Bind(faces => toSeq(faces).TraverseM(face => face <= int.MaxValue && (int)face < mesh.Faces.Count ? Fin.Succ((int)face) : Fin.Fail<int>(MetricKey.InvalidResult())).As())
                        .Bind(parts => parts.Bind(face => toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))).Filter(other => !parts.Exists(face => face == other)).Distinct()
                            .Fold(initialState: Fin.Succ((Max: 0.0, Mesh: mesh, Normal: normal)), f: static (state, other) => state.Bind(s => TopologyProjection.OfMesh(mesh: s.Mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, other))
                                .Bind(static otherProjection => otherProjection.Normal)
                                .Map(neighbour => neighbour.IsValid switch {
                                    true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal),
                                    false => s,
                                })))
                            .Map(static state => state.Max)),
                _ => Fin.Fail<double>(MetricKey.InvalidInput()),
            },
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Meshes<TGeometry, TOut>(Meshes aspect) where TGeometry : notnull => Aspect<Meshes, TGeometry, TOut>(aspect: aspect);
    internal static Operation<Mesh, MeshCheckParameters> MeshCheck {
        get {
            Op key = Op.Of();
            return Operation<Mesh, MeshCheckParameters>.Build(
                key: key, state: key,
                evaluator: static (op, geometry) => from parameters in Requirement.MeshReport(mesh: geometry, check: op.ToString()).ToEff()
                                                    from result in op.Accept(value: parameters).ToEff()
                                                    select result);
        }
    }
    internal static Operation<Mesh, MeshMetricSample> MeshMetric(MeshMetric? metric) {
        Op key = Op.Of();
        return Optional(metric).Filter(static candidate => !candidate.Equals(Analysis.MeshMetric.None)).Case switch {
            MeshMetric active => Operation<Mesh, MeshMetricSample>.Build(
                key: key, state: (Key: key, Metric: active), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) => VisiblePolygonsOf(mesh: geometry, key: state.Key)
                    .Bind(polygons => polygons.TraverseM(polygon => state.Metric.Sample(mesh: geometry, polygon: polygon)).As()).ToEff()),
            _ => Operation<Mesh, MeshMetricSample>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    internal static Operation<Mesh, MeshSample> MeshSamples(MeshSampleCategory category) {
        Op key = Op.Of(name: $"Mesh{category}");
        Seq<MeshSampleKind> kinds = category switch {
            MeshSampleCategory.Validity => MeshSampleKind.Validity,
            MeshSampleCategory.Count => MeshSampleKind.Counts,
            MeshSampleCategory.Defect => MeshSampleKind.Defects,
            MeshSampleCategory.Quality => MeshSampleKind.Quality,
            _ => Seq<MeshSampleKind>(),
        };
        return Operation<Mesh, MeshSample>.Build(
            key: key, state: (Key: key, Kinds: kinds, Inspect: category == MeshSampleCategory.Defect),
            evaluator: static (state, geometry) => state.Inspect switch {
                true => from parameters in MeshCheck.Apply(geometry: Seq(geometry))
                        from head in parameters.Head.ToFin(state.Key.InvalidResult()).ToEff()
                        from samples in state.Kinds.TraverseM(kind => kind.Sample(mesh: geometry, parameters: head).Map(value => new MeshSample(Kind: kind, Value: value))).As().ToEff()
                        select samples,
                false => state.Kinds.TraverseM(kind => kind.Sample(mesh: geometry, parameters: default).Map(value => new MeshSample(Kind: kind, Value: value))).As().ToEff(),
            });
    }
    internal static Operation<Mesh, Polyline> MeshNakedEdges {
        get {
            Op key = Op.Of(name: "MeshNakedEdges");
            return Operation<Mesh, Polyline>.Build(
                key: key, state: key,
                evaluator: static (op, mesh) => Optional(mesh.GetNakedEdges()).Map(seq => op.Accept(values: seq)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff());
        }
    }
    internal static Operation<Mesh, Polyline> MeshOutline(Plane plane) {
        Op key = Op.Of(name: "MeshOutline");
        return plane.IsValid switch {
            true => Operation<Mesh, Polyline>.Build(
                key: key, state: (Key: key, Plane: plane),
                evaluator: static (state, mesh) => state.Key.Accept(values: mesh.GetOutlines(plane: state.Plane)).ToEff()),
            false => Operation<Mesh, Polyline>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    internal static Operation<Mesh, TopologyProjection> MeshAtFace(int? index = null) {
        Op key = Op.Of();
        return Operation<Mesh, TopologyProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => VisiblePolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => polygons.Count switch {
                0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
                int count when state.Selector is int selected && (selected < 0 || selected >= count) => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
                _ => TopologyProjection.OfMesh(mesh: geometry, polygon: polygons[state.Selector ?? 0])
                    .Bind(projection => state.Key.Accept(value: projection))
            }).ToEff());
    }
    internal static Operation<TGeometry, TOut> MeshLift<TGeometry, TOut, TValue>(Op key, Operation<Mesh, TValue> source) where TGeometry : notnull =>
        Native<TGeometry, TOut, Mesh, TValue, Operation<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: Seq(mesh)));
    private static Fin<Seq<MeshNgon>> VisiblePolygonsOf(Mesh mesh, Op key) =>
        Optional(mesh.GetNgonAndFacesEnumerable()).ToFin(key.InvalidResult())
            .Map(static polygons => toSeq(polygons));
}

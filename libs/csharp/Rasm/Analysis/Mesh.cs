using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshMetricSample(ComponentIndex Source, double Value);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSample(MeshSampleKind Kind, int Value);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Meshes : IAspect {
    public sealed record SamplesCase(MeshSampleGroup Group) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record FaceQualitySummaryCase(MeshMetric Metric) : Meshes;
    public sealed record AtVisiblePolygonCase(int? Value) : Meshes;
    public sealed record VisiblePolygonCountCase : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    public static Meshes Validity => new SamplesCase(Group: MeshSampleGroup.Validity);
    public static Meshes Counts => new SamplesCase(Group: MeshSampleGroup.Count);
    public static Meshes Defects => new SamplesCase(Group: MeshSampleGroup.Defect);
    public static Meshes Quality => new SamplesCase(Group: MeshSampleGroup.Quality);
    public static Meshes FaceQuality(MeshMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshMetric.EdgeAspect);
    public static Meshes FaceQualitySummary(MeshMetric? metric = null) => new FaceQualitySummaryCase(Metric: metric ?? MeshMetric.EdgeAspect);
    public static Meshes AtVisiblePolygon(int? index = null) => new AtVisiblePolygonCase(Value: index);
    public static Meshes VisiblePolygonCount => new VisiblePolygonCountCase();
    public static Meshes NakedEdges => new NakedEdgesCase();
    public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
    private static readonly Op SamplesKey = Op.Of(name: "MeshSamples");
    private static readonly Op FaceQualityKey = Op.Of(name: "MeshFaceQuality");
    private static readonly Op FaceQualitySummaryKey = Op.Of(name: "MeshFaceQualitySummary");
    private static readonly Op AtVisiblePolygonKey = Op.Of(name: "MeshAtVisiblePolygon");
    private static readonly Op VisiblePolygonCountKey = Op.Of(name: "MeshVisiblePolygonCount");
    private static readonly Op NakedEdgesKey = Op.Of(name: "MeshNakedEdges");
    private static readonly Op OutlineKey = Op.Of(name: "MeshOutline");
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        samplesCase: static s => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: SamplesKey, source: Analyze.MeshSamples(group: s.Group)),
        faceQualityCase: static fq => Analyze.MeshLift<TGeometry, TOut, MeshMetricSample>(key: FaceQualityKey, source: Analyze.MeshMetric(metric: fq.Metric)),
        faceQualitySummaryCase: static fqs => Analyze.MeshLift<TGeometry, TOut, Stat>(key: FaceQualitySummaryKey, source: Analyze.MeshMetricSummary(metric: fqs.Metric)),
        atVisiblePolygonCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtVisiblePolygonKey, source: Analyze.MeshAtVisiblePolygon(index: at.Value)),
        visiblePolygonCountCase: static _ => Analyze.MeshLift<TGeometry, TOut, int>(key: VisiblePolygonCountKey, source: Analyze.MeshVisiblePolygonCount),
        nakedEdgesCase: static _ => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: NakedEdgesKey, source: Analyze.MeshNakedEdges),
        outlineCase: static o => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: OutlineKey, source: Analyze.MeshOutline(plane: o.Plane)));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshSampleGroup {
    public static readonly MeshSampleGroup None = new(key: 0, label: nameof(None), inspect: false), Validity = new(key: 1, label: nameof(Validity), inspect: false), Count = new(key: 2, label: nameof(Count), inspect: false), Defect = new(key: 3, label: nameof(Defect), inspect: true), Quality = new(key: 4, label: nameof(Quality), inspect: false);
    public string Label { get; }
    internal bool Inspect { get; }
    internal Seq<MeshSampleKind> Kinds => toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(this));
}

[SmartEnum<int>]
public sealed partial class MeshSampleKind {
    public static readonly MeshSampleKind None = new(key: 0, group: MeshSampleGroup.None, sample: static (_, _) => Fin.Succ(0)), Valid = new(key: 1, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsValid ? 1 : 0)), Closed = new(key: 2, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsClosed ? 1 : 0));
    public static readonly MeshSampleKind Oriented = new(key: 3, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented) ? 1 : 0)), Solid = new(key: 4, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsSolid ? 1 : 0));
    public static readonly MeshSampleKind Manifold = new(key: 5, group: MeshSampleGroup.Validity, sample: static (m, _) => Analyze.ManifoldOf(geometry: m, op: ManifoldKey).Map(static value => value ? 1 : 0)), BoundaryFree = new(key: 6, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary) ? 1 : 0));
    public static readonly MeshSampleKind Vertices = new(key: 10, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.VertexCount.IntegerOf(geometry: m, op: VertexCountKey)), Faces = new(key: 11, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.FaceCount.IntegerOf(geometry: m, op: FaceCountKey)), Triangles = new(key: 12, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.TriangleCount)), Quads = new(key: 13, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.QuadCount));
    public static readonly MeshSampleKind Edges = new(key: 14, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.EdgeCount.IntegerOf(geometry: m, op: EdgeCountKey)), Euler = new(key: 15, group: MeshSampleGroup.Count, sample: static (m, _) => Analyze.EulerOf(geometry: m, op: EulerKey)), VisiblePolygons = new(key: 16, group: MeshSampleGroup.Count, sample: static (m, _) => Analyze.VisiblePolygonCountOf(mesh: m, key: VisiblePolygonKey));
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DegenerateFaceCount)), DisjointMeshes = new(key: 21, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DisjointMeshCount)), DuplicateFaces = new(key: 22, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DuplicateFaceCount));
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ExtremelyShortEdgeCount)), InvalidNgons = new(key: 24, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.InvalidNgonCount)), NakedEdges = new(key: 25, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NakedEdgeCount)), NonManifoldEdges = new(key: 26, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonManifoldEdgeCount));
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonUnitVectorNormalCount)), RandomFaceNormals = new(key: 28, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.RandomFaceNormalCount)), SelfIntersectingPairs = new(key: 29, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.SelfIntersectingPairsCount)), UnusedVertices = new(key: 30, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.UnusedVertexCount));
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.VertexFaceNormalsDifferCount)), ZeroLengthNormals = new(key: 32, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ZeroLengthNormalCount));
    public static readonly MeshSampleKind MaximumValence = new(key: 40, group: MeshSampleGroup.Quality, sample: static (m, _) => Fin.Succ(Valences(mesh: m).Fold(0, Math.Max))), MinimumValence = new(key: 41, group: MeshSampleGroup.Quality, sample: static (m, _) => Valences(mesh: m) switch { Seq<int> v when !v.IsEmpty => Fin.Succ(v.Fold(v.Head.IfNone(0), Math.Min)), _ => Fin.Succ(0) });
    public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.BoundaryLoopsOf(geometry: m, op: BoundaryLoopsKey)), Genus = new(key: 43, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.GenusOf(geometry: m, op: GenusKey));
    public static readonly MeshSampleKind AverageValence = new(key: 44, group: MeshSampleGroup.Quality, sample: static (m, _) => Valences(mesh: m) switch { Seq<int> v when !v.IsEmpty => Fin.Succ((int)Math.Round(v.Fold(0, static (acc, n) => acc + n) / (double)v.Count)), _ => Fin.Succ(0) });
    private static readonly Op ManifoldKey = Op.Of(name: "MeshManifold");
    private static readonly Op VertexCountKey = Op.Of(name: "MeshVertexCount");
    private static readonly Op FaceCountKey = Op.Of(name: "MeshFaceCount");
    private static readonly Op EdgeCountKey = Op.Of(name: "MeshEdgeCount");
    private static readonly Op EulerKey = Op.Of(name: "MeshEuler");
    private static readonly Op VisiblePolygonKey = Op.Of(name: "MeshVisiblePolygons");
    private static readonly Op BoundaryLoopsKey = Op.Of(name: "MeshBoundaryLoops");
    private static readonly Op GenusKey = Op.Of(name: "MeshGenus");
    private static Seq<int> Valences(Mesh mesh) =>
        toSeq(Enumerable.Range(0, mesh.TopologyVertices.Count).Select(mesh.TopologyVertices.ConnectedEdgesCount));
    internal MeshSampleGroup Group { get; }
    [UseDelegateFromConstructor] internal partial Fin<int> Sample(Mesh mesh, MeshCheckParameters parameters);
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshMetric {
    public static readonly MeshMetric None = new(key: 0, sample: static (_, _, _, key) => Fin.Fail<double>(key.InvalidInput())), EdgeAspect = new(key: 1, sample: FaceEdgeAspect);
    public static readonly MeshMetric Area = new(key: 2, sample: FaceArea), Perimeter = new(key: 3, sample: FacePerimeter), Skewness = new(key: 4, sample: FaceSkewness), DihedralAngle = new(key: 5, sample: FaceMaxDihedral);
    private readonly Func<Mesh, ComponentIndex, Seq<Point3d>, Op, Fin<double>> sample;
    internal Fin<MeshMetricSample> Sample(Mesh? mesh, MeshNgon polygon, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput())
            .Bind(native => Analyze.VisiblePolygonSourceOf(mesh: native, polygon: polygon, key: key)
                .Bind(source => VerticesOf(mesh: native, source: source, key: key).Map(vertices => (Mesh: native, Source: source, Vertices: vertices))))
            .Bind(state => sample(arg1: state.Mesh, arg2: state.Source, arg3: state.Vertices, arg4: key)
                .Map(value => (state.Source, Value: value)))
            .Bind(state => state switch {
                (Source: { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 }, Value: double value) when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(new MeshMetricSample(Source: state.Source, Value: value)),
                _ => Fin.Fail<MeshMetricSample>(key.InvalidResult()),
            });
    private static Fin<Seq<Point3d>> VerticesOf(Mesh mesh, ComponentIndex source, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch { true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)), true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)), false => Fin.Fail<Seq<Point3d>>(key.InvalidResult()) },
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
            Optional(mesh.Ngons[ngon].BoundaryVertexIndexList()).ToFin(key.InvalidResult()).Bind(indices => toSeq(indices).TraverseM(i => i <= int.MaxValue && (int)i < mesh.Vertices.Count ? Fin.Succ((Point3d)mesh.Vertices[(int)i]) : Fin.Fail<Point3d>(key.InvalidResult())).As()),
        _ => Fin.Fail<Seq<Point3d>>(key.InvalidInput()),
    };
    private static Fin<Vector3d> NormalOf(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            (mesh.FaceNormals.Count > face || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count > face
                ? UnitNormal(candidate: new Vector3d(mesh.FaceNormals[face]), key: key)
                : Fin.Fail<Vector3d>(key.InvalidResult()),
        { ComponentIndexType: ComponentIndexType.MeshNgon } => vertices.Count switch {
            >= 3 => UnitNormal(candidate: vertices.Map((point, index) => Vector3d.CrossProduct(point - vertices[0], vertices[(index + 1) % vertices.Count] - vertices[0])).Fold(Vector3d.Zero, static (sum, v) => sum + v), key: key),
            _ => Fin.Fail<Vector3d>(key.InvalidResult()),
        },
        _ => Fin.Fail<Vector3d>(key.InvalidInput()),
    };
    private static Fin<Vector3d> NormalOf(Mesh mesh, ComponentIndex source, Op key) =>
        VerticesOf(mesh: mesh, source: source, key: key).Bind(vertices => NormalOf(mesh: mesh, source: source, vertices: vertices, key: key));
    private static Fin<Vector3d> UnitNormal(Vector3d candidate, Op key) =>
        candidate.IsValid && !candidate.IsTiny() ? Fin.Succ(candidate / candidate.Length) : Fin.Fail<Vector3d>(key.InvalidResult());
    private static Fin<double> FaceEdgeAspect(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) =>
        (source switch {
            { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index } when index >= 0 && index < mesh.Faces.Count => Fin.Succ(mesh.Faces.GetFaceAspectRatio(index: index)),
            _ => Fin.Fail<double>(key.InvalidInput()),
        })
            .BindFail(_ => vertices.Count >= 3
            ? vertices.Map((p, i) => p.DistanceTo(other: vertices[(i + 1) % vertices.Count])).Filter(static length => length > RhinoMath.ZeroTolerance) switch {
                Seq<double> lengths when !lengths.IsEmpty => lengths.Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0), f: static (range, length) => (Math.Min(val1: range.Min, val2: length), Math.Max(val1: range.Max, val2: length))) switch {
                    (double min, double max) when RhinoMath.IsValidDouble(x: min) && min > RhinoMath.ZeroTolerance && RhinoMath.IsValidDouble(x: max) => Fin.Succ(max / min),
                    _ => Fin.Fail<double>(key.InvalidResult()),
                },
                _ => Fin.Fail<double>(key.InvalidResult()),
            }
            : Fin.Fail<double>(key.InvalidResult()));
    private static Fin<double> FaceArea(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) =>
        source switch {
            { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                Optional(mesh.Ngons[ngon].FaceIndexList()).ToFin(key.InvalidResult())
                    .Bind(faces => toSeq(faces).TraverseM(face => face <= int.MaxValue ? FaceArea(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, (int)face), vertices: Seq<Point3d>(), key: key) : Fin.Fail<double>(key.InvalidResult())).As())
                    .Map(static areas => areas.Fold(initialState: 0.0, f: static (total, area) => total + area)),
            _ => (vertices.IsEmpty ? VerticesOf(mesh: mesh, source: source, key: key) : Fin.Succ(vertices))
                .Bind(points => points.Count >= 3
                    ? Fin.Succ(0.5 * Enumerable.Range(start: 1, count: points.Count - 2).Sum(i => Vector3d.CrossProduct(a: points[i] - points[0], b: points[i + 1] - points[0]).Length))
                    : Fin.Fail<double>(key.InvalidResult())),
        };
    private static Fin<double> FacePerimeter(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) =>
        vertices.Map((p, i) => p.DistanceTo(other: vertices[(i + 1) % vertices.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d);
    private static Fin<double> FaceSkewness(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) =>
        vertices.Count switch {
            < 3 => Fin.Fail<double>(key.InvalidResult()),
            int n => vertices.Map((vertex, i) => Vector3d.VectorAngle(a: vertices[(i + n - 1) % n] - vertex, b: vertices[(i + 1) % n] - vertex))
                .Fold(initialState: Fin.Succ((Max: 0.0, Ideal: (n - 2) * Math.PI / n)), f: (state, angle) => state.Bind(s => RhinoMath.IsValidDouble(x: angle)
                    ? Fin.Succ((Math.Max(val1: s.Max, val2: Math.Max(val1: (angle - s.Ideal) / (Math.PI - s.Ideal), val2: (s.Ideal - angle) / s.Ideal)), s.Ideal))
                    : Fin.Fail<(double Max, double Ideal)>(key.InvalidResult())))
                .Map(static state => state.Max),
        };
    private static Fin<double> FaceMaxDihedral(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Op key) =>
        NormalOf(mesh: mesh, source: source, vertices: vertices, key: key).Bind(normal => normal.IsValid switch {
            false => Fin.Succ(0.0),
            true => (source switch {
                { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
                    Fin.Succ(toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))),
                { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                    Optional(mesh.Ngons[ngon].FaceIndexList()).ToFin(key.InvalidResult())
                        .Bind(faces => toSeq(faces).TraverseM(face => face <= int.MaxValue && (int)face < mesh.Faces.Count ? Fin.Succ((int)face) : Fin.Fail<int>(key.InvalidResult())).As())
                        .Map(parts => parts.Bind(face => toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))).Filter(other => !parts.Exists(face => face == other)).Distinct()),
                _ => Fin.Fail<Seq<int>>(key.InvalidInput()),
            }).Bind(neighbours => neighbours
                .Fold(initialState: Fin.Succ((Max: 0.0, Mesh: mesh, Normal: normal, Key: key)), f: static (state, other) => state.Bind(s => NormalOf(mesh: s.Mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, other), key: s.Key)
                    .Map(neighbour => neighbour.IsValid switch {
                        true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal, s.Key),
                        false => s,
                    })))
                .Map(static state => state.Max)),
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
                evaluator: static (state, geometry) => MeshMetricSamples(mesh: geometry, metric: state.Metric, key: state.Key).ToEff()),
            _ => Operation<Mesh, MeshMetricSample>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    internal static Operation<Mesh, Stat> MeshMetricSummary(MeshMetric? metric) {
        Op key = Op.Of(name: "MeshMetricSummary");
        return Optional(metric).Filter(static candidate => !candidate.Equals(Analysis.MeshMetric.None)).Case switch {
            MeshMetric active => Operation<Mesh, Stat>.Build(
                key: key, state: (Key: key, Metric: active), requirement: Requirement.MeshCheck,
                evaluator: static (state, geometry) =>
                    from samples in MeshMetricSamples(mesh: geometry, metric: state.Metric, key: state.Key).ToEff()
                    from stat in Stat.Of(values: samples.Map(static s => s.Value), key: state.Key).ToEff()
                    from result in state.Key.Accept(value: stat).ToEff()
                    select result),
            _ => Operation<Mesh, Stat>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    internal static Operation<Mesh, MeshSample> MeshSamples(MeshSampleGroup group) {
        Op key = Op.Of(name: $"Mesh{group.Label}");
        return Operation<Mesh, MeshSample>.Build(
            key: key, state: (Key: key, group.Kinds, group.Inspect),
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
    internal static Operation<Mesh, TopologyProjection> MeshAtVisiblePolygon(int? index = null) {
        Op key = Op.Of();
        return Operation<Mesh, TopologyProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => VisiblePolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => polygons.Count switch {
                0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
                int count when state.Selector is int selected && (selected < 0 || selected >= count) => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
                _ => VisiblePolygonSourceOf(mesh: geometry, polygon: polygons[state.Selector ?? 0], key: state.Key)
                    .Bind(source => TopologyProjection.FromMesh(mesh: geometry, source: source))
                    .Bind(projection => state.Key.Accept(value: projection))
            }).ToEff());
    }
    internal static Operation<Mesh, int> MeshVisiblePolygonCount {
        get {
            Op key = Op.Of();
            return Operation<Mesh, int>.Build(
                key: key, state: key,
                evaluator: static (op, mesh) => VisiblePolygonCountOf(mesh: mesh, key: op).Bind(count => op.Accept(value: count)).ToEff());
        }
    }
    internal static Operation<TGeometry, TOut> MeshLift<TGeometry, TOut, TValue>(Op key, Operation<Mesh, TValue> source) where TGeometry : notnull =>
        Native<TGeometry, TOut, Mesh, TValue, Operation<Mesh, TValue>>(key: key, state: source, project: static (q, mesh) => q.Apply(geometry: Seq(mesh)));
    private static Fin<Seq<MeshMetricSample>> MeshMetricSamples(Mesh mesh, MeshMetric metric, Op key) =>
        VisiblePolygonsOf(mesh: mesh, key: key).Bind(polygons => polygons.TraverseM(polygon => metric.Sample(mesh: mesh, polygon: polygon, key: key)).As());
    internal static Fin<int> VisiblePolygonCountOf(Mesh mesh, Op key) =>
        Fin.Succ(mesh.GetNgonAndFacesCount());
    internal static Fin<ComponentIndex> VisiblePolygonSourceOf(Mesh mesh, MeshNgon polygon, Op key) =>
        Optional(polygon.BoundaryVertexIndexList()).Filter(static vertices => vertices.Length >= 3).ToFin(key.InvalidResult())
            .Bind(_ => Optional(polygon.FaceIndexList()).ToFin(key.InvalidResult()).Bind(faces => faces switch {
                uint[] values when values.Length == 1 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) < 0 => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshFace, (int)values[0])),
                uint[] values when values.Length > 0 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) is >= 0 and int ngon => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshNgon, ngon)),
                _ => Fin.Fail<ComponentIndex>(key.InvalidInput()),
            }));
    private static Fin<Seq<MeshNgon>> VisiblePolygonsOf(Mesh mesh, Op key) =>
        Optional(mesh.GetNgonAndFacesEnumerable()).ToFin(key.InvalidResult())
            .Map(static polygons => toSeq(polygons));
}

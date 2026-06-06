using Foundation.CSharp.Analyzers.Contracts;
using Rasm.Vectors;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record Meshes : IAspect {
    public sealed record SamplesCase(MeshSampleGroup Group) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record FaceShapeCase : Meshes;
    public sealed record AtVisiblePolygonCase(Option<int> Value) : Meshes;
    public sealed record VisiblePolygonCountCase : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    public static Meshes Validity => new SamplesCase(Group: MeshSampleGroup.Validity);
    public static Meshes Counts => new SamplesCase(Group: MeshSampleGroup.Count);
    public static Meshes Defects => new SamplesCase(Group: MeshSampleGroup.Defect);
    public static Meshes Quality => new SamplesCase(Group: MeshSampleGroup.Quality);
    public static Meshes FaceQuality(MeshMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshMetric.EdgeAspect);
    public static Meshes FaceShape => new FaceShapeCase();
    public static Meshes AtVisiblePolygon(int? index = null) => new AtVisiblePolygonCase(Value: Optional(index));
    public static Meshes VisiblePolygonCount => new VisiblePolygonCountCase();
    public static Meshes NakedEdges => new NakedEdgesCase();
    public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
    private static readonly Op SamplesKey = Op.Of(name: "MeshSamples");
    private static readonly Op FaceQualityKey = Op.Of(name: "MeshFaceQuality");
    private static readonly Op FaceShapeKey = Op.Of(name: "MeshFaceShape");
    private static readonly Op AtVisiblePolygonKey = Op.Of(name: "MeshAtVisiblePolygon");
    private static readonly Op VisiblePolygonCountKey = Op.Of(name: "MeshVisiblePolygonCount");
    private static readonly Op NakedEdgesKey = Op.Of(name: "MeshNakedEdges");
    private static readonly Op OutlineKey = Op.Of(name: "MeshOutline");
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        samplesCase: static s => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: SamplesKey, source: Analyze.MeshSamples(group: s.Group)),
        faceQualityCase: static fq => fq.Metric.Equals(MeshMetric.None)
            ? Analysis.Operation<TGeometry, TOut>.Reject(key: FaceQualityKey, fault: FaceQualityKey.InvalidInput())
            : typeof(TOut) switch {
                Type output when output == typeof(MeshMetricSample) => Analyze.MeshLift<TGeometry, TOut, MeshMetricSample>(key: FaceQualityKey, source: Analyze.MeshMetricSamplesOp(metric: fq.Metric, key: FaceQualityKey)),
                Type output when output == typeof(Stat) => Analyze.MeshLift<TGeometry, TOut, Stat>(key: FaceQualityKey, source: Analyze.MeshMetricStatOp(metric: fq.Metric, key: FaceQualityKey)),
                _ => FaceQualityKey.Unsupported<TGeometry, TOut>(),
            },
        faceShapeCase: static _ => typeof(TOut) == typeof(MeshFaceShape)
            ? Analyze.MeshLift<TGeometry, TOut, MeshFaceShape>(key: FaceShapeKey, source: Analyze.MeshFaceShapesOp(key: FaceShapeKey))
            : FaceShapeKey.Unsupported<TGeometry, TOut>(),
        atVisiblePolygonCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtVisiblePolygonKey, source: Analyze.MeshAtVisiblePolygon(index: at.Value)),
        visiblePolygonCountCase: static _ => Analyze.MeshLift<TGeometry, TOut, int>(key: VisiblePolygonCountKey, source: Analyze.MeshVisiblePolygonCount),
        nakedEdgesCase: static _ => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: NakedEdgesKey, source: Analyze.MeshNakedEdges),
        outlineCase: static o => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: OutlineKey, source: Analyze.MeshOutline(plane: o.Plane)));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshSampleGroup {
    public static readonly MeshSampleGroup None = new(key: 0, label: nameof(None), inspect: false);
    public static readonly MeshSampleGroup Validity = new(key: 1, label: nameof(Validity), inspect: false);
    public static readonly MeshSampleGroup Count = new(key: 2, label: nameof(Count), inspect: false);
    public static readonly MeshSampleGroup Defect = new(key: 3, label: nameof(Defect), inspect: true);
    public static readonly MeshSampleGroup Quality = new(key: 4, label: nameof(Quality), inspect: false);
    public string Label { get; }
    internal bool Inspect { get; }
    internal Seq<MeshSampleKind> Kinds => toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(this));
}

[SmartEnum<int>]
public sealed partial class MeshSampleKind {
    public static readonly MeshSampleKind None = new(key: 0, group: MeshSampleGroup.None, sample: static (_, _) => Fin.Succ(0));
    public static readonly MeshSampleKind Valid = new(key: 1, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsValid ? 1 : 0));
    public static readonly MeshSampleKind Closed = new(key: 2, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsClosed ? 1 : 0));
    public static readonly MeshSampleKind Oriented = new(key: 3, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented) ? 1 : 0));
    public static readonly MeshSampleKind Solid = new(key: 4, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsSolid ? 1 : 0));
    public static readonly MeshSampleKind Manifold = new(key: 5, group: MeshSampleGroup.Validity, sample: static (m, _) => Analyze.ManifoldOf(geometry: m, op: ManifoldKey).Map(static value => value ? 1 : 0));
    public static readonly MeshSampleKind BoundaryFree = new(key: 6, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary) ? 1 : 0));
    public static readonly MeshSampleKind Vertices = new(key: 10, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.VertexCount.IntegerOf(geometry: m, op: VertexCountKey));
    public static readonly MeshSampleKind Faces = new(key: 11, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.FaceCount.IntegerOf(geometry: m, op: FaceCountKey));
    public static readonly MeshSampleKind Triangles = new(key: 12, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.TriangleCount));
    public static readonly MeshSampleKind Quads = new(key: 13, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.QuadCount));
    public static readonly MeshSampleKind Edges = new(key: 14, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.EdgeCount.IntegerOf(geometry: m, op: EdgeCountKey));
    public static readonly MeshSampleKind Euler = new(key: 15, group: MeshSampleGroup.Count, sample: static (m, _) => Analyze.EulerOf(geometry: m, op: EulerKey));
    public static readonly MeshSampleKind VisiblePolygons = new(key: 16, group: MeshSampleGroup.Count, sample: static (m, _) => Analyze.VisiblePolygonCountOf(mesh: m, key: VisiblePolygonKey));
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DegenerateFaceCount));
    public static readonly MeshSampleKind DisjointMeshes = new(key: 21, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DisjointMeshCount));
    public static readonly MeshSampleKind DuplicateFaces = new(key: 22, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DuplicateFaceCount));
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ExtremelyShortEdgeCount));
    public static readonly MeshSampleKind InvalidNgons = new(key: 24, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.InvalidNgonCount));
    public static readonly MeshSampleKind NakedEdges = new(key: 25, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NakedEdgeCount));
    public static readonly MeshSampleKind NonManifoldEdges = new(key: 26, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonManifoldEdgeCount));
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonUnitVectorNormalCount));
    public static readonly MeshSampleKind RandomFaceNormals = new(key: 28, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.RandomFaceNormalCount));
    public static readonly MeshSampleKind SelfIntersectingPairs = new(key: 29, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.SelfIntersectingPairsCount));
    public static readonly MeshSampleKind UnusedVertices = new(key: 30, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.UnusedVertexCount));
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.VertexFaceNormalsDifferCount));
    public static readonly MeshSampleKind ZeroLengthNormals = new(key: 32, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ZeroLengthNormalCount));
    public static readonly MeshSampleKind MaximumValence = new(key: 40, group: MeshSampleGroup.Quality, sample: static (m, _) => Fin.Succ(Valences(mesh: m).Fold(0, Math.Max)));
    public static readonly MeshSampleKind MinimumValence = new(key: 41, group: MeshSampleGroup.Quality, sample: static (m, _) => Valences(mesh: m) switch { Seq<int> v when !v.IsEmpty => Fin.Succ(v.Fold(v.Head.IfNone(0), Math.Min)), _ => Fin.Succ(0) });
    public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.BoundaryLoopsOf(geometry: m, op: BoundaryLoopsKey));
    public static readonly MeshSampleKind Genus = new(key: 43, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.GenusOf(geometry: m, op: GenusKey));
    public static readonly MeshSampleKind AverageValence = new(key: 44, group: MeshSampleGroup.Quality, sample: static (m, _) => Valences(mesh: m) switch { Seq<int> v when !v.IsEmpty => Fin.Succ((int)Math.Round(v.Fold(0, static (acc, n) => acc + n) / (double)v.Count, MidpointRounding.ToEven)), _ => Fin.Succ(0) });
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
    public static readonly MeshMetric None = new(key: 0, measure: static (_, _, _, _, key) => Fin.Fail<double>(key.InvalidInput()));
    public static readonly MeshMetric EdgeAspect = new(key: 1, measure: FaceEdgeAspect);
    public static readonly MeshMetric Area = new(key: 2, measure: FaceArea);
    public static readonly MeshMetric Perimeter = new(key: 3, measure: static (mesh, source, vertices, context, key) => RingMetric<double>(metric: VectorCloudMetric.Perimeter, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    public static readonly MeshMetric Skewness = new(key: 4, measure: static (mesh, source, vertices, context, key) => RingMetric<double>(metric: VectorCloudMetric.Skewness, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    public static readonly MeshMetric DihedralAngle = new(key: 5, measure: FaceMaxDihedral);
    [UseDelegateFromConstructor] private partial Fin<double> Measure(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key);
    internal Fin<MeshMetricSample> Sample(Mesh? mesh, MeshNgon polygon, Context context, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput())
            .Bind(native => Analyze.VisiblePolygonSourceOf(mesh: native, polygon: polygon, key: key)
                .Bind(source => VerticesOf(mesh: native, source: source, key: key).Map(vertices => (Mesh: native, Source: source, Vertices: vertices))))
            .Bind(state => Measure(mesh: state.Mesh, source: state.Source, vertices: state.Vertices, context: context, key: key)
                .Map(value => (state.Source, Value: value)))
            .Bind(state => state switch {
                (Source: { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 }, Value: double value) when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(new MeshMetricSample(Source: state.Source, Value: value)),
                _ => Fin.Fail<MeshMetricSample>(key.InvalidResult()),
            });
    internal static Fin<MeshFaceShape> Shape(Mesh? mesh, MeshNgon polygon, Context context, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput())
            .Bind(native => Analyze.VisiblePolygonSourceOf(mesh: native, polygon: polygon, key: key)
                .Bind(source => VerticesOf(mesh: native, source: source, key: key).Map(vertices => (Mesh: native, Source: source, Vertices: vertices))))
            .Bind(state => RingMetric<VectorCloudShape>(metric: VectorCloudMetric.Shape, mesh: state.Mesh, source: state.Source, vertices: state.Vertices, context: context, key: key)
                .Map(shape => new MeshFaceShape(Source: state.Source, Shape: shape)));
    private static Fin<Seq<Point3d>> VerticesOf(Mesh mesh, ComponentIndex source, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch { true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)), true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)), false => Fin.Fail<Seq<Point3d>>(key.InvalidResult()) },
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
            Optional(mesh.Ngons.NgonBoundaryVertexList(ngon: mesh.Ngons[ngon], bAppendStartPoint: false)).ToFin(key.InvalidResult()).Map(static points => toSeq(points)),
        _ => Fin.Fail<Seq<Point3d>>(key.InvalidInput()),
    };
    private static Fin<Seq<int>> FaceIndicesOf(Mesh mesh, int ngon, Op key) =>
        Optional(mesh.Ngons[ngon].FaceIndexList()).ToFin(key.InvalidResult())
            .Bind(faces => toSeq(faces).TraverseM(face => face <= int.MaxValue && (int)face < mesh.Faces.Count ? Fin.Succ((int)face) : Fin.Fail<int>(key.InvalidResult())).As()
                .Bind(indices => indices.IsEmpty ? Fin.Fail<Seq<int>>(key.InvalidResult()) : Fin.Succ(indices)));
    private static Fin<TOut> RingMetric<TOut>(VectorCloudMetric metric, Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        (vertices.IsEmpty ? VerticesOf(mesh: mesh, source: source, key: key) : Fin.Succ(vertices))
            .Bind(points => VectorCloud.Ring(points: points, context: context, key: key))
            .Bind(cloud => VectorIntent.Cloud(cloud: cloud, metric: metric, key: key))
            .Bind(intent => intent.Project<TOut>(context: context, key: key));
    private static Fin<Vector3d> NormalOf(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            (mesh.FaceNormals.Count > face || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count > face
                ? VectorIntent.Direction(value: new Vector3d(mesh.FaceNormals[face])).Project<Vector3d>(context: context, key: key)
                : Fin.Fail<Vector3d>(key.InvalidResult()),
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
            FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                .Bind(faces => ((mesh.FaceNormals.Count >= mesh.Faces.Count || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count >= mesh.Faces.Count) switch {
                    true => faces.TraverseM(face => FaceArea(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, face), vertices: Seq<Point3d>(), context: context, key: key)
                        .Map(area => new Vector3d(mesh.FaceNormals[face]) * area)).As()
                        .Bind(weighted => VectorIntent.Direction(value: weighted.Fold(initialState: Vector3d.Zero, f: static (sum, normal) => sum + normal)).Project<Vector3d>(context: context, key: key)),
                    false => RingMetric<Vector3d>(metric: VectorCloudMetric.Normal, mesh: mesh, source: source, vertices: vertices, context: context, key: key),
                }),
        _ => Fin.Fail<Vector3d>(key.InvalidInput()),
    };
    private static Fin<double> FaceEdgeAspect(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        (source switch {
            { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index } when index >= 0 && index < mesh.Faces.Count => Fin.Succ(mesh.Faces.GetFaceAspectRatio(index: index)),
            _ => Fin.Fail<double>(key.InvalidInput()),
        })
            .BindFail(_ => RingMetric<double>(metric: VectorCloudMetric.EdgeAspect, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    private static Fin<double> FaceArea(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        source switch {
            { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                    .Bind(faces => faces.TraverseM(face => FaceArea(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, face), vertices: Seq<Point3d>(), context: context, key: key)).As())
                    .Map(static areas => areas.Fold(initialState: 0.0, f: static (total, area) => total + area)),
            _ => RingMetric<double>(metric: VectorCloudMetric.Area, mesh: mesh, source: source, vertices: vertices, context: context, key: key),
        };
    private static Fin<double> FaceMaxDihedral(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        NormalOf(mesh: mesh, source: source, vertices: vertices, context: context, key: key).Bind(normal =>
            (source switch {
                { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
                    Fin.Succ(toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))),
                { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                    FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                        .Map(parts => parts.Bind(face => toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))).Filter(other => !parts.Exists(face => face == other)).Distinct()),
                _ => Fin.Fail<Seq<int>>(key.InvalidInput()),
            }).Bind(neighbours => neighbours
                .Fold(initialState: Fin.Succ((Max: 0.0, Mesh: mesh, Normal: normal, Context: context, Key: key)), f: static (state, other) => state.Bind(s => NormalOf(mesh: s.Mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, other), vertices: Seq<Point3d>(), context: s.Context, key: s.Key)
                    .Bind(neighbour => VectorIntent.Angular(a: s.Normal, b: neighbour)
                            .Project<double>(context: s.Context, key: s.Key)
                        .Map(angle => (Math.Max(val1: s.Max, val2: angle), s.Mesh, s.Normal, s.Context, s.Key)))))
                .Map(static state => state.Max)));
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshMetricSample(ComponentIndex Source, double Value);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSample(MeshSampleKind Kind, int Value);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceShape(ComponentIndex Source, VectorCloudShape Shape);

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
    internal static Operation<Mesh, MeshMetricSample> MeshMetricSamplesOp(MeshMetric metric, Op key) =>
        Operation<Mesh, MeshMetricSample>.Build(
            key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (state, geometry) => from context in Env.Asks
                                                   from samples in MeshMetricSamples(mesh: geometry, metric: state.Metric, context: context, key: state.Key).ToEff()
                                                   select samples);
    internal static Operation<Mesh, Stat> MeshMetricStatOp(MeshMetric metric, Op key) =>
        Operation<Mesh, Stat>.Build(
            key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (state, geometry) => from context in Env.Asks
                                                   from stat in MeshMetricSamples(mesh: geometry, metric: state.Metric, context: context, key: state.Key)
                                                       .Bind(samples => Stat.Of(values: samples.Map(static sample => sample.Value), key: state.Key))
                                                       .Bind(stat => state.Key.Accept(value: stat)).ToEff()
                                                   select stat);
    internal static Operation<Mesh, MeshFaceShape> MeshFaceShapesOp(Op key) =>
        Operation<Mesh, MeshFaceShape>.Build(
            key: key, state: key, requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (op, geometry) => from context in Env.Asks
                                                from shapes in MeshFaceShapes(mesh: geometry, context: context, key: op).ToEff()
                                                select shapes);
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
    internal static Operation<Mesh, TopologyProjection> MeshAtVisiblePolygon(Option<int> index = default) {
        Op key = Op.Of();
        return Operation<Mesh, TopologyProjection>.Build(
            key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => VisiblePolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => (Source: polygons, Index: state.Selector.IfNone(0)) switch {
                (Seq<MeshNgon> source, _) when source.Count == 0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
                (Seq<MeshNgon> source, int selected) when selected < 0 || selected >= source.Count => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
                (Seq<MeshNgon> source, int selected) => VisiblePolygonSourceOf(mesh: geometry, polygon: source[selected], key: state.Key)
                    .Bind(component => TopologyProjection.FromMesh(mesh: geometry, source: component))
                    .Bind(projection => state.Key.Accept(value: projection)),
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
        Native<TGeometry, TOut, Mesh, TValue, Operation<Mesh, TValue>>(key: key, state: source, requirement: source.Requirement, requiresContext: source.RequiresContext, project: static (q, mesh) => q.Apply(geometry: Seq(mesh)));
    private static Fin<Seq<MeshMetricSample>> MeshMetricSamples(Mesh mesh, MeshMetric metric, Context context, Op key) =>
        VisiblePolygonsOf(mesh: mesh, key: key).Bind(polygons => polygons.TraverseM(polygon => metric.Sample(mesh: mesh, polygon: polygon, context: context, key: key)).As());
    private static Fin<Seq<MeshFaceShape>> MeshFaceShapes(Mesh mesh, Context context, Op key) =>
        VisiblePolygonsOf(mesh: mesh, key: key).Bind(polygons => polygons.TraverseM(polygon => MeshMetric.Shape(mesh: mesh, polygon: polygon, context: context, key: key)).As());
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

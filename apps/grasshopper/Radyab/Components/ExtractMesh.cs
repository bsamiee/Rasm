namespace Radyab.Components;

[IoId("C61FD8BC-5E22-400A-9C4F-F3C1C57F0765")]
[Nomen(
    name: "Extract Mesh",
    info: "Validity, topology stats, defect counts, naked-edge polylines, disjoint pieces, principal axes, per-face quality, and indexed face details for Rhino mesh values. Non-manifold edges are available as curves via Extract Curves with a mesh input.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractMesh : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly Port<int> FaceIndex = Port.Index(name: "Face Index", code: "FI", info: "Zero-based face selector for the Indexed Face output; missing Face Index defaults to face 0 and supplied values clamp to [0, faces-1].");
    private static readonly Port<MeshFaceMetric> Metric = Port.Optional<MeshFaceMetric>(name: "Quality Metric", code: "M", info: "Per-face metric for the Face Quality output. Missing Quality Metric defaults to AspectRatio.", kind: PortKind.Enum(initial: MeshFaceMetric.AspectRatio), fallback: Some(MeshFaceMetric.AspectRatio));
    private static readonly IOutputGroup Validity = Output.Query(input: Geometry, port: Port.List<bool>(name: "Validity", code: "V", info: "[IsValid, IsClosed, IsOriented, IsSolid, IsManifold, HasBoundary] per mesh; all six derive from one Mesh.IsManifold pass plus native validity properties."), aspect: Meshes.ValidityBundle);
    private static readonly IOutputGroup Stats = Output.Query(input: Geometry, port: Port.List<int>(name: "Stats", code: "ST", info: "[VertexCount, FaceCount, TriangleCount, QuadCount, EdgeCount, EulerCharacteristic] as a six-item list per mesh."), aspect: Meshes.StatsBundle);
    private static readonly IOutputGroup Defects = Output.Query(input: Geometry, port: Port.List<int>(name: "Defect Counts", code: "D", info: "Thirteen-item list of MeshCheckParameters counts in MeshCheckCount enum order (DegenerateFaces, DisjointMeshes, DuplicateFaces, ExtremelyShortEdges, InvalidNgons, NakedEdges, NonManifoldEdges, NonUnitVectorNormals, RandomFaceNormals, SelfIntersectingPairs, UnusedVertices, VertexFaceNormalsDiffer, ZeroLengthNormals)."), aspect: Meshes.DefectsBundle);
    private static readonly IOutputGroup NakedEdges = Output.Query(input: Geometry, port: Port.List<Polyline>(kind: PortKind.Polyline, name: "Naked Edges", code: "NE", info: "Connected boundary polylines via Mesh.GetNakedEdges(); empty when the mesh is closed."), aspect: Rasm.Analysis.Boundaries.Naked);
    private static readonly IOutputGroup Pieces = Output.Query(input: Geometry, port: Port.List<Mesh>(name: "Pieces", code: "P", info: "Disjoint mesh components via Mesh.SplitDisjointPieces(); a single-element list for a connected mesh."), aspect: static _ => Rasm.Analysis.Query.Components<object, Mesh>());
    private static readonly IOutputGroup PrincipalAxes = Output.Details<(double Moment, Vector3d Axis)>(
        input: Geometry,
        aspect: static _ => shape => Rasm.Analysis.Query.Measure<object, (double Moment, Vector3d Axis)>(aspect: new Measure.PrincipalAxes(Mass: MassKind.Volume)).Apply(geometry: shape.Inner),
        emptyUnsupported: false,
        aspectLabel: nameof(Measure),
        slots: [
            Output.Plain<(double Moment, Vector3d Axis), double>(port: Port.List<double>(name: "Principal Moments", code: "PM", info: "Three volume-mass principal moments of inertia (X, Y, Z). Empty when the mesh is not solid."), project: static value => value.Moment),
            Output.Plain<(double Moment, Vector3d Axis), Vector3d>(port: Port.List<Vector3d>(name: "Principal Axes", code: "PA", info: "Three world-coordinate principal axes aligned with the moments; surfaces intrinsic geometric orientation."), project: static value => value.Axis),
        ]);
    private static readonly IOutputGroup Quality = Output.Details<MeshFaceSample>(
        input: Geometry,
        aspect: runtime => shape => Rasm.Analysis.Query.Meshes<object, MeshFaceSample>(aspect: Meshes.FaceQuality(metric: runtime.ReadOrInvalid(port: Metric, invalid: MeshFaceMetric.AspectRatio).ToNullable())).Apply(geometry: shape.Inner),
        emptyUnsupported: true,
        aspectLabel: nameof(Meshes),
        slots: [
            Output.Plain<MeshFaceSample, int>(port: Port.List<int>(name: "Face Index", code: "FI", info: "Face index for each metric sample.", kind: PortKind.Integer, policy: PortPolicy.Index()), project: static value => value.Face),
            Output.Plain<MeshFaceSample, double>(port: Port.List<double>(name: "Face Quality", code: "Q", info: "Metric value for each face."), project: static value => value.Value),
        ]);
    private static readonly IOutputGroup IndexedFace = Output.Details<MeshFaceProjection>(
        input: Geometry,
        aspect: runtime => shape => Rasm.Analysis.Query.Meshes<object, MeshFaceProjection>(aspect: Meshes.AtFace(index: shape.Inner is Mesh mesh ? runtime.Index(port: FaceIndex, limit: mesh.Faces.Count).ToNullable() : runtime.ReadOrInvalid(port: FaceIndex, invalid: int.MinValue).ToNullable())).Apply(geometry: shape.Inner),
        emptyUnsupported: false,
        aspectLabel: nameof(Meshes),
        slots: [
            Output.One<MeshFaceProjection, Mesh>(port: Port.List<Mesh>(name: "Indexed Face", code: "IF", info: "Mesh containing only the selected face."), project: static (value, _) => value.Isolated()),
            Output.Many<MeshFaceProjection, Point3d>(port: Port.List<Point3d>(name: "Face Vertices", code: "FV", info: "Vertex positions of the selected face."), project: static value => value.Vertices),
            Output.One<MeshFaceProjection, Point3d>(port: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Centroid of the selected face."), project: static (value, _) => value.Center),
            Output.One<MeshFaceProjection, Vector3d>(port: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Unit normal of the selected face."), project: static (value, _) => value.Normal),
            Output.Plain<MeshFaceProjection, int>(port: Port.List<int>(name: "Source Face", code: "S", info: "Source mesh face index.", kind: PortKind.Integer, policy: PortPolicy.Index()), project: static value => value.Source.Index),
        ]);
    public ExtractMesh() : base(self: typeof(ExtractMesh), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry, FaceIndex, Metric),
        outputs: Seq<IOutputGroup>(Validity, Stats, Defects, NakedEdges, Pieces, PrincipalAxes, Quality, IndexedFace))) { }
}

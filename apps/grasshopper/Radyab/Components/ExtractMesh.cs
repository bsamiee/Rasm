namespace Radyab.Components;

[IoId("E37712C0-BA9B-4350-8B1E-ED90B5183B51")]
[Nomen(
    name: "Extract Mesh",
    info: "Validity, topology stats, defect counts, naked-edge polylines, disjoint pieces, principal axes, per-face quality, and indexed face details for Rhino mesh values. Non-manifold edges are available as curves via Extract Curves with a mesh input.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractMesh : Component<ExtractMesh> {
    [Input] private readonly Port<Shape> Geometry = Port.Shape();
    [Input] private readonly Port<int> FaceIndex = Port.Index(name: "Face Index", code: "FI", info: "Zero-based face selector for the Indexed Face output; missing Face Index defaults to face 0 and supplied values clamp to [0, faces-1].");
    [Input] private readonly Port<MeshFaceMetric> Metric = Port.Optional<MeshFaceMetric>(name: "Quality Metric", code: "M", info: "Per-face metric for the Face Quality output. Missing Quality Metric defaults to AspectRatio.", kind: PortKind.Enum(initial: MeshFaceMetric.AspectRatio));
    [Output] private IOutputGroup Validity => Output.Query(input: Geometry, port: Port.List<bool>(name: "Validity", code: "V", info: "[IsValid, IsClosed, IsOriented, IsSolid, IsManifold, HasBoundary] per mesh; all six derive from one Mesh.IsManifold pass plus native validity properties."), aspect: Meshes.ValidityBundle);
    [Output] private IOutputGroup Stats => Output.Query(input: Geometry, port: Port.List<int>(name: "Stats", code: "ST", info: "[VertexCount, FaceCount, TriangleCount, QuadCount, EdgeCount, EulerCharacteristic] as a six-item list per mesh."), aspect: Meshes.StatsBundle);
    [Output] private IOutputGroup Defects => Output.Query(input: Geometry, port: Port.List<int>(name: "Defect Counts", code: "D", info: "Thirteen-item list of MeshCheckParameters counts in MeshCheckCount enum order (DegenerateFaces, DisjointMeshes, DuplicateFaces, ExtremelyShortEdges, InvalidNgons, NakedEdges, NonManifoldEdges, NonUnitVectorNormals, RandomFaceNormals, SelfIntersectingPairs, UnusedVertices, VertexFaceNormalsDiffer, ZeroLengthNormals)."), aspect: Meshes.DefectsBundle);
    [Output] private IOutputGroup NakedEdges => Output.Query(input: Geometry, port: Port.List<Polyline>(name: "Naked Edges", code: "NE", info: "Connected boundary polylines via Mesh.GetNakedEdges(); empty when the mesh is closed."), operation: static (_, _) => Rasm.Analysis.Query.NakedEdges<object, Polyline>());
    [Output] private IOutputGroup Pieces => Output.Query(input: Geometry, port: Port.List<Mesh>(name: "Pieces", code: "P", info: "Disjoint mesh components via Mesh.SplitDisjointPieces(); a single-element list for a connected mesh."), operation: static (_, _) => Rasm.Analysis.Query.Components<object, Mesh>());
    [Output]
    private IOutputGroup PrincipalAxes => Output.Details<(double Moment, Vector3d Axis)>(
        input: Geometry,
        source: static (_, _) => shape => Rasm.Analysis.Query.Measure<object, (double Moment, Vector3d Axis)>(aspect: new Measure.PrincipalAxes(Mass: MassKind.Volume)).Apply(geometry: shape.Inner),
        slots: [
            Output.Plain<(double Moment, Vector3d Axis), double>(port: Port.List<double>(name: "Principal Moments", code: "PM", info: "Three volume-mass principal moments of inertia (X, Y, Z). Empty when the mesh is not solid."), project: static value => value.Moment),
            Output.Plain<(double Moment, Vector3d Axis), Vector3d>(port: Port.List<Vector3d>(name: "Principal Axes", code: "PA", info: "Three world-coordinate principal axes aligned with the moments; surfaces intrinsic geometric orientation."), project: static value => value.Axis),
        ]);
    [Output] private IOutputGroup Quality => Output.Query(input: Geometry, port: Port.List<MeshFaceSample>(name: "Face Quality", code: "Q", info: "Per-face metric samples selected by Quality Metric; each sample carries the face index and metric value."), operation: (access, runtime) => Rasm.Analysis.Query.Meshes<object, MeshFaceSample>(aspect: Meshes.FaceQuality(metric: runtime.Value(access: access, port: Metric).IfNone(MeshFaceMetric.AspectRatio))));
    [Output]
    private IOutputGroup IndexedFace => Output.Details<MeshFaceProjection>(
        input: Geometry,
        source: (access, runtime) => shape => Rasm.Analysis.Query.Meshes<object, MeshFaceProjection>(aspect: Meshes.AtFace(index: runtime.NullableIndex(access: access, port: FaceIndex))).Apply(geometry: shape.Inner),
        slots: [
            Output.Plain<MeshFaceProjection, Mesh>(port: Port.List<Mesh>(name: "Indexed Face", code: "IF", info: "Mesh containing only the selected face."), project: static value => value.Isolated()),
            Output.Many<MeshFaceProjection, Point3d>(port: Port.List<Point3d>(name: "Face Vertices", code: "FV", info: "Vertex positions of the selected face."), project: static value => Fin.Succ(value.Vertices)),
            Output.Plain<MeshFaceProjection, Point3d>(port: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Centroid of the selected face."), project: static value => value.Center),
            Output.Plain<MeshFaceProjection, Vector3d>(port: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Unit normal of the selected face."), project: static value => value.Normal),
            Output.Plain<MeshFaceProjection, ComponentIndex>(port: Port.List<ComponentIndex>(name: "Source", code: "S", info: "Component index identifying the selected face."), project: static value => value.Source),
        ]);
}

namespace Radyab.Components;

[IoId("C61FD8BC-5E22-400A-9C4F-F3C1C57F0765")]
[Nomen(
    name: "Extract Mesh",
    info: "Validity panel, topology stats, critical defect counts, naked-edge polylines, and disjoint pieces for Rhino mesh values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractMesh : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly IOutputGroup Validity = Output.Single(input: Geometry, port: Port.Tree<bool>(name: "Validity", code: "V", info: "[IsValid, IsClosed, IsOriented, IsSolid, IsManifold, IsBoundaryFree] per mesh; six flags derived from one Mesh.IsManifold pass plus native validity properties."), aspect: Meshes.ValidityBundle);
    private static readonly IOutputGroup Stats = Output.Single(input: Geometry, port: Port.Tree<int>(name: "Stats", code: "ST", info: "[VertexCount, FaceCount, TriangleCount, QuadCount, EdgeCount, EulerCharacteristic] per mesh."), aspect: Meshes.StatsBundle);
    private static readonly IOutputGroup Defects = Output.Single(input: Geometry, port: Port.Tree<int>(name: "Defects", code: "D", info: "[DegenerateFaces, DuplicateFaces, NakedEdges, NonManifoldEdges, SelfIntersectingPairs] critical defect counts per mesh."), aspect: Meshes.DefectsBundle);
    private static readonly IOutputGroup NakedEdges = Output.Single(input: Geometry, port: Port.Tree<Polyline>(kind: PortKind.Polyline, name: "Naked Edges", code: "NE", info: "Connected boundary polylines via Mesh.GetNakedEdges(); empty when the mesh is closed."), aspect: Rasm.Analysis.Boundaries.Naked);
    private static readonly IOutputGroup Pieces = Output.Single<Mesh>(
        input: Geometry,
        port: Port.Tree<Mesh>(name: "Pieces", code: "P", info: "Disjoint mesh components via Mesh.SplitDisjointPieces(); a single-element list for a connected mesh."),
        query: static _ => Fin.Succ(Rasm.Analysis.Query.Components<object, Mesh>()));
    public ExtractMesh() : base(self: typeof(ExtractMesh), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry),
        outputs: Seq<IOutputGroup>(Validity, Stats, Defects, NakedEdges, Pieces))) { }
}

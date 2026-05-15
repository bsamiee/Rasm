namespace Radyab.Components;

[IoId("C61FD8BC-5E22-400A-9C4F-F3C1C57F0765")]
[Nomen(
    name: "Extract Mesh",
    info: "Validity panel, topology stats, critical defect counts, naked-edge polylines, and disjoint pieces for Rhino mesh values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractMesh : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly OutputGroup Validity = Output.Single(input: Geometry, port: Port.Tree<MeshSample>(name: "Validity", code: "V", info: "Keyed validity samples per mesh."), aspect: Meshes.Validity);
    private static readonly OutputGroup Stats = Output.Single(input: Geometry, port: Port.Tree<MeshSample>(name: "Stats", code: "ST", info: "Keyed topology samples per mesh."), aspect: Meshes.Counts);
    private static readonly OutputGroup Defects = Output.Single(input: Geometry, port: Port.Tree<MeshSample>(name: "Defects", code: "D", info: "Keyed defect samples per mesh."), aspect: Meshes.Defects);
    private static readonly OutputGroup NakedEdges = Output.Single(input: Geometry, port: Port.Tree<Polyline>(kind: PortKind.Polyline, name: "Naked Edges", code: "NE", info: "Connected boundary polylines via Mesh.GetNakedEdges(); empty when the mesh is closed."), aspect: Rasm.Analysis.Boundaries.Naked);
    private static readonly OutputGroup Pieces = Output.Single<Mesh>(
        input: Geometry,
        port: Port.Tree<Mesh>(name: "Pieces", code: "P", info: "Disjoint mesh components via Mesh.SplitDisjointPieces(); a single-element list for a connected mesh."),
        operation: static _ => Fin.Succ(Rasm.Analysis.Analyze.Components<object, Mesh>()));
    public ExtractMesh() : base(self: typeof(ExtractMesh), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry),
        outputs: Seq<OutputGroup>(Validity, Stats, Defects, NakedEdges, Pieces))) { }
}

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class QuadSplit {
    public static readonly QuadSplit Default = new(0);
    public static readonly QuadSplit ShortDiagonal = new(1);
    public static readonly QuadSplit LongDiagonal = new(2);
    public static readonly QuadSplit MinimumArea = new(3);
    public static readonly QuadSplit MaximumArea = new(4);
    public static readonly QuadSplit MinimumNormalAngle = new(5);
    public static readonly QuadSplit MaximumNormalAngle = new(6);

    public Step<Mesh> Triangulate(Option<double> planarTolerance, Option<double> angleTolerance) =>
        Steps.Count<Mesh>(mesh => mesh.Faces.ConvertNonPlanarQuadsToTriangles(Conversions.Unset(planarTolerance), Conversions.Unset(angleTolerance), Key), static count => count >= 0);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FacePatch {
    public Seq<ComponentIndex> Corners => Switch(vertexEdge: static patch => Seq(patch.Vertex), twoEdges: static _ => Seq<ComponentIndex>(), threeVertices: static patch => Seq(patch.A, patch.B, patch.C));

    public Seq<int> Edges => Switch(vertexEdge: static patch => Seq(patch.Edge), twoEdges: static patch => Seq(patch.EdgeA, patch.EdgeB), threeVertices: static _ => Seq<int>());

    public sealed record VertexEdge(ComponentIndex Vertex, int Edge) : FacePatch;

    public sealed record TwoEdges(int EdgeA, int EdgeB) : FacePatch;

    public sealed record ThreeVertices(ComponentIndex A, ComponentIndex B, ComponentIndex C) : FacePatch;

    public Step<Mesh> Patch =>
        Steps.Derived<Mesh>(mesh =>
            from held in IO.lift(() =>
                from corners in Products.Each(Corners, static (vertex, at) => vertex.ComponentIndexType is ComponentIndexType.MeshVertex or ComponentIndexType.MeshTopologyVertex
                    ? Fin.Succ(vertex)
                    : new RhinoFault(vertex.ComponentIndexType, RhinoReason.Mismatched, at))
                from components in mesh.Within(corners + Edges.Map(static edge => new ComponentIndex(ComponentIndexType.MeshTopologyEdge, edge)))
                select components)
            from patched in Products.Accepted(() => mesh.PatchSingleFace(held))
            select mesh);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeshEdits {
    // --- [REDUCTION]
    public static Step<Mesh> Reduce(Func<ReduceMeshParameters> parameters, bool threaded) =>
        Steps.Derived<Mesh>(mesh =>
            from token in cancelToken
            from fresh in IO.lift(parameters)
            from cancellable in IO.lift(() => fresh.CancelToken = token)
            from reduced in Products.Accepted(() => mesh.Reduce(fresh, threaded))
                .MapFail(error => Conversions.Present(fresh.Error).Match(Some: reason => error + Error.New(reason), None: () => error))
            select mesh);

    // --- [COMPONENTS]
    public static Step<Mesh> UnweldVertices(Seq<int> topologyVertices, bool modifyNormals) =>
        Steps.At<Mesh>(ComponentIndexType.MeshTopologyVertex, topologyVertices, (mesh, vertices) =>
            mesh.UnweldEdge(toSeq(vertices.Bind(vertex => toSeq(mesh.TopologyVertices.ConnectedEdges(vertex))).Distinct()), modifyNormals));

    public static Step<Mesh> Align(double distance, bool average, Seq<int> vertices) =>
        Steps.At<Mesh>(ComponentIndexType.MeshVertex, vertices, (mesh, held) => mesh.Vertices.Align(distance, average, toSeq(Range(0, mesh.Vertices.Count)).Map(toHashSet(held).Contains)) >= 0);

    public static Step<Mesh> Subdivide(Seq<int> faces) =>
        Steps.At<Mesh>(ComponentIndexType.MeshFace, faces, static (mesh, held) => mesh.Subdivide(held));

    public static Step<Mesh> DeleteFaces(Seq<int> faces, bool compactVertices) =>
        Steps.At<Mesh>(ComponentIndexType.MeshFace, faces, (mesh, held) => mesh.Faces.DeleteFaces(held, compactVertices) == held.Count);
}

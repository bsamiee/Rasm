using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TopologySource {
    public sealed record BrepFaces(Brep Brep) : TopologySource;
    public sealed record MeshTopology(Mesh Mesh) : TopologySource;
    public sealed record MeshFaces(Mesh Mesh) : TopologySource;
    public sealed record SubDNet(SubD SubD) : TopologySource;
    public sealed record CurveNetwork(Seq<Curve> Curves) : TopologySource;

    public IO<(IUndirectedGraph<int, TaggedUndirectedEdge<int, int>> Graph, Func<TaggedUndirectedEdge<int, int>, double> Weight)> Graph(Tolerances tolerances) =>
        IO.lift(() => Switch(
            tolerances,
            brepFaces: static (_, of) => View(
                Enumerable.Range(0, of.Brep.Faces.Count),
                face => of.Brep.Faces[face].AdjacentEdges().Distinct().SelectMany(edge => Duals(edge, face, of.Brep.Edges[edge].AdjacentFaces())),
                edge => of.Brep.Edges[edge.Tag].GetLength()),
            meshTopology: static (_, of) => View(
                Enumerable.Range(0, of.Mesh.TopologyVertices.Count),
                vertex => of.Mesh.TopologyVertices.ConnectedEdges(vertex).Select(edge => of.Mesh.TopologyEdges.GetTopologyVertices(edge) switch {
                    var ends => Edge(ends.I, ends.J, edge),
                }),
                edge => of.Mesh.TopologyEdges.EdgeLine(edge.Tag).Length),
            meshFaces: static (_, of) => View(
                Enumerable.Range(0, of.Mesh.Faces.Count),
                face => of.Mesh.TopologyEdges.GetEdgesForFace(face).Distinct().SelectMany(edge => Duals(edge, face, of.Mesh.TopologyEdges.GetConnectedFaces(edge))),
                edge => of.Mesh.TopologyEdges.EdgeLine(edge.Tag).Length),
            subDNet: static (_, of) => View(
                of.SubD.Vertices.Select(static vertex => (int)vertex.Id),
                id => of.SubD.Vertices.Find((uint)id) switch {
                    var vertex => Enumerable.Range(0, vertex.EdgeCount).Select(at => vertex.EdgeAt(at)).Select(static edge =>
                        Edge((int)edge.RelativeVertexAt(0).Id, (int)edge.RelativeVertexAt(1).Id, (int)edge.Id)),
                },
                edge => of.SubD.Edges.Find((uint)edge.Tag).ControlNetLine.Length),
            curveNetwork: static (at, of) => Network(at, of.Curves)));

    private static (IUndirectedGraph<int, TaggedUndirectedEdge<int, int>> Graph, Func<TaggedUndirectedEdge<int, int>, double> Weight) Network(Tolerances tolerances, Seq<Curve> curves) {
        Seq<Point3d> ends = curves.Bind(static curve => Seq(curve.PointAtStart, curve.PointAtEnd)).Strict();
        IEnumerable<int[]> found = RTree.Point3dClosestPoints(ends, ends, tolerances.Absolute);
        using IDisposable? tree = found as IDisposable;
        UndirectedGraph<int, SUndirectedEdge<int>> near = new();
        _ = near.AddVertexRange(Enumerable.Range(0, ends.Count));
        _ = near.AddEdgeRange(found.SelectMany(static (hay, needle) => hay.Where(other => other > needle).Select(other => new SUndirectedEdge<int>(needle, other))));
        Dictionary<int, int> component = [];
        int count = near.ConnectedComponents(component);
        Seq<(int Start, int End)> joints = curves.Map((_, index) => (Start: component[2 * index], End: component[(2 * index) + 1])).Strict();
        ILookup<int, int> incident = joints.SelectMany(static (joint, curve) => Seq((Joint: joint.Start, Curve: curve), (Joint: joint.End, Curve: curve)))
            .ToLookup(static end => end.Joint, static end => end.Curve);
        return View(
            Enumerable.Range(0, count),
            vertex => incident[vertex].Distinct().Select(curve => Edge(joints[curve].Start, joints[curve].End, curve)),
            edge => curves[edge.Tag].GetLength());
    }

    private static IEnumerable<TaggedUndirectedEdge<int, int>> Duals(int edge, int face, int[] faces) =>
        faces.SelectMany((one, at) => faces.Skip(at + 1).Where(other => one == face || other == face).Select(other => Edge(one, other, edge)));

    private static TaggedUndirectedEdge<int, int> Edge(int a, int b, int tag) => new(Math.Min(a, b), Math.Max(a, b), tag);

    private static (IUndirectedGraph<int, TaggedUndirectedEdge<int, int>> Graph, Func<TaggedUndirectedEdge<int, int>, double> Weight) View(
        IEnumerable<int> vertices, Func<int, IEnumerable<TaggedUndirectedEdge<int, int>>> adjacent, Func<TaggedUndirectedEdge<int, int>, double> weight) =>
        (Graph: vertices.ToDelegateUndirectedGraph(adjacent), Weight: weight);
}

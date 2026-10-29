using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TopologyGraph(IUndirectedGraph<int, TaggedUndirectedEdge<int, int>> Graph, Func<TaggedUndirectedEdge<int, int>, double> Weight) {
    internal static TopologyGraph Of(IEnumerable<int> vertices, Func<int, IEnumerable<TaggedUndirectedEdge<int, int>>> adjacent, Func<TaggedUndirectedEdge<int, int>, double> weight) =>
        new(vertices.ToDelegateUndirectedGraph(adjacent), weight);

    internal static TaggedUndirectedEdge<int, int> Edge(int a, int b, int tag) => new(Math.Min(a, b), Math.Max(a, b), tag);

    internal static IEnumerable<TaggedUndirectedEdge<int, int>> Across(int edge, int face, int[] faces) =>
        faces.SelectMany((one, at) => faces.Skip(at + 1).Where(other => one == face || other == face).Select(other => Edge(one, other, edge)));
}

[SmartEnum]
public sealed partial class Adjacency {
    public static readonly Adjacency Primal = new(
        static brep => TopologyGraph.Of(
            Enumerable.Range(0, brep.Vertices.Count),
            vertex => brep.Vertices[vertex].EdgeIndices().Select(edge => TopologyGraph.Edge(brep.Edges[edge].StartVertex.VertexIndex, brep.Edges[edge].EndVertex.VertexIndex, edge)),
            edge => brep.Edges[edge.Tag].GetLength()),
        static mesh => TopologyGraph.Of(
            Enumerable.Range(0, mesh.TopologyVertices.Count),
            vertex => mesh.TopologyVertices.ConnectedEdges(vertex).Select(edge => mesh.TopologyEdges.GetTopologyVertices(edge) switch {
                var ends => TopologyGraph.Edge(ends.I, ends.J, edge),
            }),
            edge => mesh.TopologyEdges.EdgeLine(edge.Tag).Length),
        static subd => TopologyGraph.Of(
            subd.Vertices.Select(static vertex => (int)vertex.Id),
            id => subd.Vertices.Find((uint)id) switch {
                var vertex => Enumerable.Range(0, vertex.EdgeCount).Select(vertex.EdgeAt).Select(static edge =>
                    TopologyGraph.Edge((int)edge.RelativeVertexAt(0).Id, (int)edge.RelativeVertexAt(1).Id, (int)edge.Id)),
            },
            edge => subd.Edges.Find((uint)edge.Tag).ControlNetLine.Length),
        static network => TopologyGraph.Of(
            Enumerable.Range(0, network.Joints),
            vertex => network.Incident[vertex].Distinct().Select(curve => TopologyGraph.Edge(network.Ends[curve].Start, network.Ends[curve].End, curve)),
            edge => network.Curves[edge.Tag].GetLength()));

    public static readonly Adjacency Dual = new(
        static brep => TopologyGraph.Of(
            Enumerable.Range(0, brep.Faces.Count),
            face => brep.Faces[face].AdjacentEdges().Distinct().SelectMany(edge => TopologyGraph.Across(edge, face, brep.Edges[edge].AdjacentFaces())),
            edge => brep.Edges[edge.Tag].GetLength()),
        static mesh => TopologyGraph.Of(
            Enumerable.Range(0, mesh.Faces.Count),
            face => mesh.TopologyEdges.GetEdgesForFace(face).Distinct().SelectMany(edge => TopologyGraph.Across(edge, face, mesh.TopologyEdges.GetConnectedFaces(edge))),
            edge => mesh.TopologyEdges.EdgeLine(edge.Tag).Length),
        static subd => TopologyGraph.Of(
            subd.Faces.Select(static face => (int)face.Id),
            id => subd.Faces.Find((uint)id) switch {
                var face => Enumerable.Range(0, face.EdgeCount).Select(face.EdgeAt).Where(static edge => edge.FaceCount is 2).Select(static edge =>
                    TopologyGraph.Edge((int)edge.FaceAt(0).Id, (int)edge.FaceAt(1).Id, (int)edge.Id)),
            },
            edge => subd.Edges.Find((uint)edge.Tag).ControlNetLine.Length),
        static network => TopologyGraph.Of(
            Enumerable.Range(0, network.Curves.Count),
            curve => from joint in Seq(network.Ends[curve].Start, network.Ends[curve].End).Distinct()
                     from other in network.Incident[joint]
                     where other != curve
                     select TopologyGraph.Edge(curve, other, joint),
            edge => (network.Curves[edge.Source].GetLength() + network.Curves[edge.Target].GetLength()) / 2d));

    [UseDelegateFromConstructor]
    internal partial TopologyGraph OfBrep(Brep brep);

    [UseDelegateFromConstructor]
    internal partial TopologyGraph OfMesh(Mesh mesh);

    [UseDelegateFromConstructor]
    internal partial TopologyGraph OfSubD(SubD subd);

    [UseDelegateFromConstructor]
    internal partial TopologyGraph OfNetwork((Seq<Curve> Curves, Seq<(int Start, int End)> Ends, ILookup<int, int> Incident, int Joints) network);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TopologySource {
    public sealed record BrepTopology(Brep Brep) : TopologySource;

    public sealed record MeshTopology(Mesh Mesh) : TopologySource;

    public sealed record SubDTopology(SubD SubD) : TopologySource;

    public sealed record CurveNetwork(Seq<Curve> Curves) : TopologySource;

    public IO<TopologyGraph> Graph(Tolerances tolerances, Adjacency adjacency) =>
        IO.lift(() => Switch(
            (Tolerances: tolerances, Adjacency: adjacency),
            brepTopology: static (at, of) => at.Adjacency.OfBrep(of.Brep),
            meshTopology: static (at, of) => at.Adjacency.OfMesh(of.Mesh),
            subDTopology: static (at, of) => at.Adjacency.OfSubD(of.SubD),
            curveNetwork: static (at, of) => at.Adjacency.OfNetwork(Joined(at.Tolerances, of.Curves))));

    private static (Seq<Curve> Curves, Seq<(int Start, int End)> Ends, ILookup<int, int> Incident, int Joints) Joined(Tolerances tolerances, Seq<Curve> curves) {
        Seq<Point3d> ends = curves.Bind(static curve => Seq(curve.PointAtStart, curve.PointAtEnd)).Strict();
        IEnumerable<int[]> found = RTree.Point3dClosestPoints(ends, ends, tolerances.Absolute);
        using IDisposable? tree = found as IDisposable;
        UndirectedGraph<int, SUndirectedEdge<int>> near = new();
        _ = near.AddVertexRange(Enumerable.Range(0, ends.Count));
        _ = near.AddEdgeRange(found.SelectMany(static (hay, needle) => hay.Where(other => other > needle).Select(other => new SUndirectedEdge<int>(needle, other))));
        Dictionary<int, int> component = [];
        int count = near.ConnectedComponents(component);
        Seq<(int Start, int End)> joints = curves.Map((_, index) => (Start: component[2 * index], End: component[(2 * index) + 1])).Strict();
        return (curves, joints, joints.SelectMany(static (joint, curve) => Seq((Joint: joint.Start, Curve: curve), (Joint: joint.End, Curve: curve)))
            .ToLookup(static end => end.Joint, static end => end.Curve), count);
    }
}

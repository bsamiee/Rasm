using Grasshopper2.Doc;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Doc;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WireGraph {
    private WireGraph(ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> graph, HashMap<Guid, string> labels) =>
        (Graph, Labels) = (graph, labels);

    public ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Graph { get; }

    private HashMap<Guid, string> Labels { get; }

    private IMutableBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> Condensed =>
        Graph.CondensateStronglyConnected<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>();

    public static WireGraph Of(Connectivity connectivity) {
        Seq<(Guid Id, string Description)> nodes = toSeq(connectivity.AllNodes).Map(static node => (node.Id, node.Description)).Strict();
        return new(
            nodes.Select(static node => node.Id)
                .ToBidirectionalGraph(id => connectivity.FindImmediateOutputs(id).Select(target => new SEquatableEdge<Guid>(id, target.Id)), allowParallelEdges: false)
                .ToArrayBidirectionalGraph(),
            toHashMap(nodes));
    }

    public static Seq<WireEnds> WiresOf(Document document, SEquatableEdge<Guid> pair) =>
        toSeq(document.Objects.AllWires)
            .Filter(wire => Connectivity.GetConnectiveId(document, wire.Source) == pair.Source && Connectivity.GetConnectiveId(document, wire.Target) == pair.Target)
            .Strict();

    public Seq<Seq<Guid>> Cycles() =>
        toSeq(Condensed.Vertices)
            .Filter(static component => component.EdgeCount > 0)
            .Map(static component => toSeq(component.Vertices).Strict())
            .Strict();

    public HashMap<Guid, int> Ranks() {
        IMutableBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> condensed = Condensed;
        return toSeq(condensed.SourceFirstBidirectionalTopologicalSort())
            .Fold(HashMap<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, int>(), (ranks, component) =>
                ranks.Add(component, toSeq(condensed.InEdges(component)).Fold(0, (rank, edge) => int.Max(rank, ranks[edge.Source] + 1))))
            .AsIterable()
            .Fold(HashMap<Guid, int>(), static (ranks, entry) => toSeq(entry.Key.Vertices).Fold(ranks, (all, vertex) => all.Add(vertex, entry.Value)));
    }

    public Seq<SEquatableEdge<Guid>> Redundant() {
        IMutableBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> condensed = Condensed;
        return toSeq(condensed.Edges.Except(condensed.ComputeTransitiveReduction().Edges))
            .Bind(static edge => toSeq(edge.Edges))
            .Strict();
    }

    public Option<Seq<SEquatableEdge<Guid>>> Path(Guid upstream, Guid downstream) =>
        Some(upstream)
            .Filter(Graph.ContainsVertex)
            .Bind(root => Callbacks.Found(Graph.TreeBreadthFirstSearch(root)(downstream, out IEnumerable<SEquatableEdge<Guid>> path), path))
            .Map(static path => toSeq(path).Strict());

    public string Dot() =>
        Graph.ToGraphviz(algorithm => {
            algorithm.GraphFormat.RankDirection = GraphvizRankDirection.LR;
            algorithm.FormatVertex += (_, args) => args.VertexFormat.Label = Labels[args.Vertex];
        });
}

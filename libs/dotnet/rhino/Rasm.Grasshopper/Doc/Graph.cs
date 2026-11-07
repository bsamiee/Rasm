using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using GraphShape.Algorithms;
using GraphShape.Algorithms.Layout;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Extensions;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using Rasm.Grasshopper.UI;

namespace Rasm.Grasshopper.Doc;

// --- [MODELS] --------------------------------------------------------------------------

public sealed class WireGraph {

    // --- [CONSTRUCTION]

    private readonly IMutableBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> components;
    private readonly HashMap<Guid, string> labels;

    private WireGraph(ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> graph, HashMap<Guid, string> labels) {
        Graph = graph;
        this.labels = labels;
        components = graph.CondensateStronglyConnected<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>();
        Ranks = Ranked(components);

        static HashMap<Guid, int> Ranked<TVertex, TEdge>(IVertexAndEdgeListGraph<TVertex, TEdge> dag)
            where TVertex : IVertexSet<Guid> where TEdge : IEdge<TVertex> {
            LayeredTopologicalSortAlgorithm<TVertex, TEdge> layers = new(dag);
            layers.Compute();
            return toHashMap(layers.LayerIndices.SelectMany(static layer => layer.Key.Vertices, static (layer, vertex) => (vertex, layer.Value)));
        }
    }

    public ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Graph { get; }

    public HashMap<Guid, int> Ranks { get; }

    public static WireGraph Of(Connectivity connectivity) {
        Seq<(Guid Id, string Description)> nodes = toSeq(connectivity.AllNodes).Map(static node => (node.Id, node.Description)).Strict();
        return new(
            nodes.Select(static node => node.Id)
                .ToBidirectionalGraph(id => connectivity.FindImmediateOutputs(id).Select(target => new SEquatableEdge<Guid>(id, target.Id)), allowParallelEdges: false)
                .ToArrayBidirectionalGraph(),
            toHashMap(nodes));
    }

    public static Seq<WireEnds> WiresOf(Document document, SEquatableEdge<Guid> pair) =>
        (from target in Optional(document.Objects.Find(pair.Target)).ToSeq()
         from parameter in toSeq(target.EntireFamily).Choose(static member => Optional(member as IParameter))
             + (target is IPinCushion cushion ? toSeq(cushion.Pins.AboveAndBelow).Choose(static pin => Optional(pin as IParameter)) : Seq<IParameter>())
         from source in toSeq(parameter.Inputs.Forwards)
         where Connectivity.GetConnectiveId(document, source) == pair.Source
         select new WireEnds(source, parameter.InstanceId)).Strict();

    // --- [QUERIES]

    public Seq<Seq<Guid>> Cycles() =>
        toSeq(components.Vertices)
            .Filter(static component => component.EdgeCount > 0)
            .Map(static component => toSeq(component.Vertices).Strict())
            .Strict();

    public Seq<SEquatableEdge<Guid>> Redundant() =>
        toSeq(components.Edges.Except(components.ComputeTransitiveReduction().Edges))
            .Bind(static edge => toSeq(edge.Edges))
            .Strict();

    public Option<Seq<SEquatableEdge<Guid>>> Path(Guid upstream, Guid downstream) =>
        Some(upstream)
            .Filter(Graph.ContainsVertex)
            .Bind(root => Graph.TreeBreadthFirstSearch(root)(downstream, out IEnumerable<SEquatableEdge<Guid>> path) ? Some(toSeq(path).Strict()) : None);

    public string Dot() =>
        Graph.ToGraphviz(algorithm => {
            algorithm.GraphFormat.RankDirection = GraphvizRankDirection.LR;
            algorithm.FormatVertex += (_, args) => args.VertexFormat.Label = labels[args.Vertex];
        });

    // --- [LAYERS]

    public Seq<Seq<Guid>> Layers() {
        if (components.IsEdgesEmpty)
            return Graph.IsVerticesEmpty ? [] : Seq(toSeq(Graph.Vertices));
        HashMap<Guid, int> order = toHashMap(Graph.Vertices.Select(static (vertex, index) => (vertex, index)));
        return Ordered(components);

        Seq<Seq<Guid>> Ordered<TVertex, TEdge>(IVertexAndEdgeListGraph<TVertex, TEdge> graph)
            where TVertex : class, IVertexSet<Guid> where TEdge : IEdge<TVertex> {
            SugiyamaLayoutAlgorithm<TVertex, TEdge, IVertexAndEdgeListGraph<TVertex, TEdge>> layout = new(graph, new SugiyamaLayoutParameters { MinimizeEdgeLength = false }) { Rand = new Random(0) };
            layout.Compute();
            return toSeq(graph.Vertices
                .GroupBy(component => Ranks[component.Vertices.First()])
                .OrderBy(static layer => layer.Key)
                .Select(layer => toSeq(layer
                    .OrderBy(component => layout.VerticesPositions[component].X)
                    .ThenBy(component => component.Vertices.Min(vertex => order[vertex]))
                    .SelectMany(component => component.Vertices.OrderBy(vertex => order[vertex])))));
        }
    }
}

[SmartEnum]
public sealed partial class Relationship {

    // --- [ITEMS]

    public static readonly Relationship Inputs = new("Sources", static (_, connectivity, id) => toSeq(connectivity.FindImmediateInputs(id)).Map(static node => node.Id));
    public static readonly Relationship Outputs = new("Recipients", static (_, connectivity, id) => toSeq(connectivity.FindImmediateOutputs(id)).Map(static node => node.Id));
    public static readonly Relationship Neighbours = new("Neighbours", static (document, connectivity, id) => Inputs.Next(document, connectivity, id) + Outputs.Next(document, connectivity, id));
    public static readonly Relationship Content = new("Group Content", static (_, connectivity, id) =>
        connectivity.Find(id, out ConnectiveObject? node) && node is ConnectiveGroup group ? toSeq(group.Contents) : Seq<Guid>());
    public static readonly Relationship Groups = new("Containing Groups", static (document, _, id) => toSeq(document.Objects.ContainingGroups(id)).Map(static group => group.InstanceId));
    public static readonly Relationship Type = new("Objects Of Same Type", static (document, _, id) => Kin(document, id, static member => Some(member.GetType())));
    public static readonly Relationship Plugin = new("Objects Of Same Plug-in", static (document, _, id) => Kin(document, id, static member => Optional(PluginServer.FindPluginForObject(member)).Map(static owner => owner.Id)));

    public string Noun { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Guid> Next(Document document, Connectivity connectivity, Guid id);

    // --- [TRAVERSALS]

    public Seq<IDocumentObject> Reach(Document document, Seq<Guid> roots, int depth, Func<IDocumentObject, bool> through) {
        Connectivity connectivity = document.Objects.Connectivity;
        return Walked(roots, toHashSet(roots), depth, Seq<IDocumentObject>()).Run();

        Trampoline<Seq<IDocumentObject>> Walked(Seq<Guid> frontier, LanguageExt.HashSet<Guid> seen, int left, Seq<IDocumentObject> reached) =>
            left <= 0 || frontier.IsEmpty
                ? Trampoline.Pure(reached)
                : frontier.Bind(id => Next(document, connectivity, id)).Fold((Seen: seen, Found: Seq<IDocumentObject>()), (state, id) =>
                    state.Seen.Contains(id)
                        ? state
                        : Optional(document.Objects.Find(id)).Filter(through).Match(Some: member => (Seen: state.Seen.Add(id), Found: state.Found.Add(member)), None: () => state)) switch {
                            var step => Trampoline.More(() => Walked(step.Found.Map(static member => member.InstanceId), step.Seen, left - 1, reached + step.Found)),
                        };
    }

    private static Seq<Guid> Kin<TKey>(Document document, Guid id, Func<IDocumentObject, Option<TKey>> key) =>
        from held in Optional(document.Objects.Find(id)).ToSeq()
        from shared in key(held).ToSeq()
        from member in toSeq(document.Objects.Forwards)
        where key(member).Exists(owned => EqualityComparer<TKey>.Default.Equals(owned, shared))
        select member.InstanceId;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Selections {

    // --- [SELECTION]

    public static IO<int> Select(Document document, Relationship relation, int depth, SelectionMode mode) =>
        IO.lift(() => (
            Prior: toHashSet(toSeq(document.Objects.PrimaryAndSecondary).Filter(static member => member.Selected).Map(static member => member.InstanceId)),
            Reached: toHashSet(relation.Reach(document, Roots(document), depth, static _ => true).Map(static member => member.InstanceId))) switch {
                var (prior, reached) => Selected(document, mode switch {
                    SelectionMode.Promote => reached,
                    SelectionMode.Include => prior.Union(reached),
                    SelectionMode.Exclude => prior.Except(reached),
                    SelectionMode.Inverse => prior.SymmetricExcept(reached),
                }),
            });

    public static Seq<IDocumentObject> Find(Document document, string pattern) =>
        (pattern.AsSpan().ContainsAny('*', '?') ? pattern : $"*{pattern}*") switch {
            var wildcard => toSeq(document.Objects.Forwards)
                .Filter(member => Seq(member.Nomen.Name, member.UserName, member.DisplayName, member.GetType().Name)
                    .Exists(text => !string.IsNullOrEmpty(text) && text.WildcardMatch(wildcard, StringComparison.OrdinalIgnoreCase)))
                .Strict(),
        };

    public static IO<Unit> Search(Canvas canvas) =>
        from document in IO.lift(() => canvas.Document)
        from prior in IO.lift(() => toHashSet(toSeq(document.Objects.SelectedObjects).Map(static member => member.InstanceId)))
        from anchor in IO.lift(() => (Frame: canvas.VisibleFrame, Size: new SizeF(StandardLayout.LargeObjectWidth, StandardLayout.ParameterMinHeight)) switch {
            var (frame, size) => new RectangleF(new PointF(frame.MiddleX - (size.Width / 2f), frame.Top + size.Height), size),
        })
        from shown in IO.lift(() => canvas.ShowInlineEditor(
            anchor,
            "",
            text => Hooks.Run(IO.lift(() => Matched(document, text, prior)), Result<int>.Fail("", text)),
            () => Hooks.Run(IO.lift(() => Selected(document, prior)), 0)))
        select unit;

    private static Result<int> Matched(Document document, string text, LanguageExt.HashSet<Guid> prior) =>
        text.Length is 0
            ? Result<int>.Succeed(Selected(document, prior), text)
            : Find(document, text).Map(static member => member.InstanceId) switch {
                { IsEmpty: true } => Result<int>.Fail(new GrasshopperFault(text, GrasshopperReason.Missing).Message, text),
                var found => Result<int>.Succeed(Selected(document, toHashSet(found)), text),
            };

    private static Seq<Guid> Roots(Document document) =>
        toSeq(toHashSet(toSeq(document.Objects.SelectedObjects).Map(static member => Connectivity.GetConnectiveId(member))));

    private static int Selected(Document document, LanguageExt.HashSet<Guid> chosen) {
        Seq<IDocumentObject> changed = toSeq(document.Objects.PrimaryAndSecondary).Filter(member => member.Selected != chosen.Contains(member.InstanceId)).Strict();
        _ = changed.Iter(static member => member.Selection = member.Selected ? ObjectSelection.Unselected : ObjectSelection.Selected);
        return changed.Count;
    }

    // --- [COMMANDS]

    public static Seq<Command> Commands =>
        (from relation in toSeq(Relationship.Items)
         from mode in toSeq(Enum.GetValues<SelectionMode>())
         let face = mode switch {
             SelectionMode.Promote => (Verb: "Select", Info: "Replaces the selection with"),
             SelectionMode.Include => (Verb: "Add", Info: "Adds to the selection"),
             SelectionMode.Exclude => (Verb: "Remove", Info: "Removes from the selection"),
             SelectionMode.Inverse => (Verb: "Toggle", Info: "Toggles in the selection"),
         }
         select Chrome.Run(
             new Nomen($"{face.Verb} {relation.Noun}", $"{face.Info} the {relation.Noun} of the selected objects"),
             canvas => Select(canvas.Document, relation, 1, mode).Map(static _ => unit)))
            .Add(Chrome.Run(new Nomen("Find Objects", "Selects the objects whose name, nickname or type matches a wildcard pattern"), Search, Application.Instance.CommonModifier | Keys.F));
}

using System.Globalization;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MemberReference(Guid Container, Guid Member, Guid Definition);

public sealed record DefinitionGraph {
    private DefinitionGraph(ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> nesting, HashMap<Guid, Seq<Guid>> placements, HashMap<Guid, string> emptyLinked, Seq<MemberReference> dangling) =>
        (Nesting, Placements, EmptyLinked, Dangling, Components) = (nesting, placements, emptyLinked, dangling, nesting.Condensed);

    public ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Nesting { get; }

    public HashMap<Guid, Seq<Guid>> Placements { get; }

    public HashMap<Guid, string> EmptyLinked { get; }

    public Seq<MemberReference> Dangling { get; }

    public IMutableBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> Components { get; }

    // --- [ORDER]
    public Seq<Guid> Removable(HashSet<Guid> stale) =>
        toSeq(Components.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)).Fold(
            (Components: HashSet<AdjacencyGraph<Guid, SEquatableEdge<Guid>>>(), Vertices: Seq<Guid>()), (removable, component) =>
                component.Vertices.All(id => stale.Contains(id) && !Placements.ContainsKey(id)) && Components.OutEdges(component).All(edge => removable.Components.Contains(edge.Target))
                    ? (removable.Components.Add(component), removable.Vertices + toSeq(component.Vertices))
                    : removable).Vertices;

    // --- [SOURCES]
    public static IO<DefinitionGraph> Read(RhinoDoc doc) =>
        from definitions in IO.lift(() => Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true)))
        let known = toHashSet(definitions.Map(static definition => definition.Id))
        let members = (from definition in definitions
                       from member in Conversions.Rows(definition.GetObjects())
                       from placed in Optional(member.Geometry as InstanceReferenceGeometry).ToSeq()
                       select new MemberReference(definition.Id, member.Id, placed.ParentIdefId)).ToLookup(member => known.Contains(member.Definition))
        select Of(
            definitions.Map(static definition => definition.Id).Strict(),
            toSeq(members[true]),
            toHashMap(definitions.Map(static definition => (definition.Id, Placed: definition.References(ReferenceScope.TopLevel).Map(static instance => instance.Id).Strict()))
                .Filter(static row => !row.Placed.IsEmpty)),
            toHashMap(definitions.Filter(static definition => definition.IsLinkedType && definition.ObjectCount is 0)
                .Choose(static definition => Conversions.Present(definition.SourceArchive).Map(path => (definition.Id, path)))),
            toSeq(members[false]));

    public static DefinitionGraph Of(ArchiveGraph archive) =>
        (Instances: archive.Components.Edges.Where(static edge => edge.Kind == RelationKind.InstanceOf),
         Containers: archive.Components.Edges.Where(static edge => edge.Kind == RelationKind.MemberOf).ToLookup(static edge => edge.Source.Id, static edge => edge.Target.Id)) switch {
             var relations => Of(
                 toSeq(archive.Components.Vertices).Filter(static vertex => vertex.Type is ModelComponentType.InstanceDefinition).Map(static vertex => vertex.Id).Strict(),
                 toSeq(from instance in relations.Instances
                       from container in relations.Containers[instance.Source.Id]
                       select new MemberReference(container, instance.Source.Id, instance.Target.Id)),
                 toHashMap(relations.Instances.Where(instance => !relations.Containers.Contains(instance.Source.Id))
                     .GroupBy(static instance => instance.Target.Id, static instance => instance.Source.Id).Select(static placed => (placed.Key, toSeq(placed).Strict()))),
                 archive.Sources.Filter((id, _) => archive.Components.InEdges(new ComponentIdentity(id, ModelComponentType.InstanceDefinition)).All(static edge => edge.Kind != RelationKind.MemberOf)),
                 from link in archive.Dangling
                 where link.Kind == RelationKind.InstanceOf
                 from container in toSeq(relations.Containers[link.Source])
                 from definition in link.Reference.ToOption().ToSeq()
                 select new MemberReference(container, link.Source, definition)),
         };

    private static DefinitionGraph Of(Seq<Guid> definitions, Seq<MemberReference> nested, HashMap<Guid, Seq<Guid>> placements, HashMap<Guid, string> emptyLinked, Seq<MemberReference> dangling) =>
        nested.ToLookup(static member => member.Definition, static member => new SEquatableEdge<Guid>(member.Definition, member.Container)) switch {
            var outEdges => new(definitions.ToBidirectionalGraph(vertex => outEdges[vertex], allowParallelEdges: false).ToArrayBidirectionalGraph(), placements, emptyLinked, dangling.Strict()),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Graphs {
    // --- [CONDENSATION]
    extension<TVertex, TEdge>(IVertexAndEdgeListGraph<TVertex, TEdge> graph) where TEdge : IEdge<TVertex> {
        public IMutableBidirectionalGraph<AdjacencyGraph<TVertex, TEdge>, CondensedEdge<TVertex, TEdge, AdjacencyGraph<TVertex, TEdge>>> Condensed =>
            graph.CondensateStronglyConnected<TVertex, TEdge, AdjacencyGraph<TVertex, TEdge>>();
    }

    // --- [ORDER]
    extension<TVertex, TEdge>(IBidirectionalGraph<AdjacencyGraph<TVertex, TEdge>, CondensedEdge<TVertex, TEdge, AdjacencyGraph<TVertex, TEdge>>> components) where TEdge : IEdge<TVertex> {
        public Seq<Seq<TVertex>> Cycles =>
            toSeq(components.Vertices).Filter(static component => component.EdgeCount > 0).Map(static component => toSeq(component.Vertices).Strict()).Strict();

        public Fin<Seq<TVertex>> SourceFirst =>
            components.Cycles switch {
                { IsEmpty: true } => toSeq(components.SourceFirstBidirectionalTopologicalSort()).Bind(static component => toSeq(component.Vertices)).Strict(),
                var cycles => Error.Many(cycles.Map(static cycle => (Error)new RhinoFault(string.Join(", ", cycle.Map(static vertex => Convert.ToString(vertex, CultureInfo.InvariantCulture))), RhinoReason.Repeated))),
            };
    }

    // --- [REACH]
    public static IO<(ArrayBidirectionalGraph<TVertex, TEdge> Graph, HashMap<TVertex, Fin<TRead>> Readings)> Reached<TVertex, TRead, TEdge>(
        TVertex root, Func<TVertex, IO<TRead>> read, Func<TRead, Seq<TEdge>> edges) where TVertex : notnull where TEdge : IEdge<TVertex> =>
        from first in read(root)
        from walked in Walked(Across(root, edges(first)), Seq(root), HashMap((root, Fin.Succ(first))), read, edges)
        select (walked.Order.ToBidirectionalGraph(
                vertex => walked.Readings[vertex].ToSeq().Bind(edges).Filter(edge => System.Collections.Generic.EqualityComparer<TVertex>.Default.Equals(edge.Source, vertex)), allowParallelEdges: true)
            .ToArrayBidirectionalGraph(), walked.Readings);

    private static IO<(Seq<TVertex> Order, HashMap<TVertex, Fin<TRead>> Readings)> Walked<TVertex, TRead, TEdge>(
        Seq<TVertex> pending, Seq<TVertex> order, HashMap<TVertex, Fin<TRead>> readings, Func<TVertex, IO<TRead>> read, Func<TRead, Seq<TEdge>> edges)
        where TVertex : notnull where TEdge : IEdge<TVertex> =>
        pending.Match(
            Empty: () => IO.pure((Order: order, Readings: readings)),
            Tail: (next, rest) => readings.ContainsKey(next)
                ? Walked(rest, order, readings, read, edges)
                : read(next).Map(Fin.Succ).Catch(static error => error.IsExpected && !Session.Cancellation(error), static error => IO.pure(Fin.Fail<TRead>(error)))
                    .Bind(reading => Walked(rest + reading.ToSeq().Bind(held => Across(next, edges(held))), order.Add(next), readings.Add(next, reading), read, edges)));

    private static Seq<TVertex> Across<TVertex, TEdge>(TVertex vertex, Seq<TEdge> edges) where TVertex : notnull where TEdge : IEdge<TVertex> =>
        edges.Map(edge => System.Collections.Generic.EqualityComparer<TVertex>.Default.Equals(edge.Source, vertex) ? edge.Target : edge.Source);
}

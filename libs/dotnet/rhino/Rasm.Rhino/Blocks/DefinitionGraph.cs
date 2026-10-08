using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MemberReference(Guid Container, Guid Member, Guid Definition);

public sealed record DefinitionGraph(ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Nesting, HashMap<Guid, Seq<Guid>> Placements,
    HashMap<Guid, string> Opaque, Seq<MemberReference> Dangling) {
    // --- [ORDER]
    public Seq<Guid> BakeOrder => toSeq(Nesting.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward)).Strict();

    public Seq<Guid> Removable(LanguageExt.HashSet<Guid> stale) =>
        toSeq(Nesting.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)).Strict() switch {
            var order => order.Filter(order.Fold(LanguageExt.HashSet<Guid>.Empty, (removable, id) =>
                stale.Contains(id) && !Placements.ContainsKey(id) && toSeq(Nesting.OutEdges(id)).ForAll(edge => removable.Contains(edge.Target))
                    ? removable.Add(id)
                    : removable).Contains),
        };

    // --- [SOURCES]
    public static IO<DefinitionGraph> Read(RhinoDoc doc) =>
        from definitions in IO.lift(() => Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true)))
        let ids = definitions.Map(static definition => definition.Id).Strict()
        let known = toHashSet(ids)
        let members = (from definition in definitions
                       from member in Conversions.Rows(definition.GetObjects())
                       from placed in Optional(member.Geometry as InstanceReferenceGeometry).ToSeq()
                       select new MemberReference(definition.Id, member.Id, placed.ParentIdefId)).ToLookup(member => known.Contains(member.Definition))
        select Of(ids, toSeq(members[true]),
            toHashMap(definitions.Map(static definition => (definition.Id,
                Placed: Conversions.Rows(definition.GetReferences((int)ReferenceScope.TopLevel)).Map(static instance => instance.Id).Strict())).Filter(static row => !row.Placed.IsEmpty)),
            toHashMap(definitions.Filter(static definition => definition.IsLinkedType && definition.ObjectCount == 0)
                .Choose(static definition => Conversions.Present(definition.SourceArchive).Map(path => (definition.Id, path)))), toSeq(members[false]));

    public static DefinitionGraph Of(ArchiveGraph archive) =>
        (Instances: archive.Components.Edges.Where(static edge => edge.Kind == RelationKind.InstanceOf),
         Containers: archive.Components.Edges.Where(static edge => edge.Kind == RelationKind.MemberOf).ToLookup(static edge => edge.Source.Id, static edge => edge.Target.Id)) switch {
             var relations => Of(
                 toSeq(archive.Components.Vertices).Filter(static vertex => vertex.Type == ModelComponentType.InstanceDefinition).Map(static vertex => vertex.Id).Strict(),
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

    // --- [CONSTRUCTION]
    private static DefinitionGraph Of(Seq<Guid> definitions, Seq<MemberReference> nested, HashMap<Guid, Seq<Guid>> placements, HashMap<Guid, string> opaque, Seq<MemberReference> dangling) =>
        nested.ToLookup(static member => member.Definition, static member => new SEquatableEdge<Guid>(member.Definition, member.Container)) switch {
            var outEdges => new(definitions.ToBidirectionalGraph(vertex => outEdges[vertex], allowParallelEdges: false).ToArrayBidirectionalGraph(),
                placements, opaque, dangling.Strict()),
        };
}

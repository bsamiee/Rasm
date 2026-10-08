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

public sealed record DefinitionGraph(
    ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Nesting,
    HashMap<Guid, Seq<Guid>> Placements,
    HashMap<Guid, string> Opaque,
    Seq<MemberReference> Dangling) {
    public Seq<Guid> BakeOrder => toSeq(Nesting.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward)).Strict();

    public Seq<Guid> Removable(LanguageExt.HashSet<Guid> stale) =>
        toSeq(Nesting.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)).Strict() switch {
            var order => order.Filter(order.Fold(LanguageExt.HashSet<Guid>.Empty, (removable, id) =>
                stale.Contains(id) && !Placements.ContainsKey(id) && toSeq(Nesting.OutEdges(id)).ForAll(edge => removable.Contains(edge.Target))
                    ? removable.Add(id)
                    : removable).Contains),
        };

    public static IO<DefinitionGraph> Read(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true)).Map(static definition => (
                definition.Id,
                Members: Conversions.Rows(definition.GetObjects())
                    .Choose(member => Optional(member.Geometry as InstanceReferenceGeometry).Map(placed => new MemberReference(definition.Id, member.Id, placed.ParentIdefId)))
                    .Strict(),
                Placed: Conversions.Rows(definition.GetReferences((int)ReferenceScope.TopLevel)).Map(static instance => instance.Id).Strict(),
                Opaque: Some(definition).Filter(static held => held.IsLinkedType && held.ObjectCount == 0).Bind(static held => Conversions.Present(held.SourceArchive))))
            .Strict())
        .Map(static rows => toHashSet(rows.Map(static row => row.Id)) switch {
            var known => Of(
                rows.Map(static row => row.Id),
                rows.Bind(static row => row.Members).Filter(member => known.Contains(member.Definition)),
                toHashMap(rows.Filter(static row => !row.Placed.IsEmpty).Map(static row => (row.Id, row.Placed))),
                toHashMap(rows.Choose(static row => row.Opaque.Map(path => (row.Id, path)))),
                rows.Bind(static row => row.Members).Filter(member => !known.Contains(member.Definition))),
        });

    public static DefinitionGraph Of(ArchiveGraph archive) =>
        toSeq(archive.Components.Edges).Filter(static edge => edge.Kind == RelationKind.InstanceOf).Map(edge => (Edge: edge, Container: Container(archive, edge.Source))).Strict() switch {
            var instances => Of(
                toSeq(archive.Components.Vertices).Filter(static vertex => vertex.Type == ModelComponentType.InstanceDefinition).Map(static vertex => vertex.Id).Strict(),
                instances.Choose(static row => row.Container.Map(container => new MemberReference(container, row.Edge.Source.Id, row.Edge.Target.Id))),
                instances.Filter(static row => row.Container.IsNone).Fold(
                    HashMap<Guid, Seq<Guid>>(),
                    static (placed, row) => placed.AddOrUpdate(row.Edge.Target.Id, held => held.Add(row.Edge.Source.Id), Seq(row.Edge.Source.Id))),
                archive.Sources.Filter((id, _) => !toSeq(archive.Components.InEdges(new ComponentIdentity(id, ModelComponentType.InstanceDefinition)))
                    .Exists(static edge => edge.Kind == RelationKind.MemberOf)),
                archive.Dangling.Filter(static link => link.Kind == RelationKind.InstanceOf).Choose(link =>
                    from container in Container(archive, new ComponentIdentity(link.Source, ModelComponentType.ModelGeometry))
                    from definition in link.Reference.ToOption()
                    select new MemberReference(container, link.Source, definition))),
        };

    private static DefinitionGraph Of(Seq<Guid> definitions, Seq<MemberReference> nested, HashMap<Guid, Seq<Guid>> placements, HashMap<Guid, string> opaque, Seq<MemberReference> dangling) =>
        nested.ToLookup(static member => member.Definition, static member => new SEquatableEdge<Guid>(member.Definition, member.Container)) switch {
            var outEdges => new(
                definitions.ToBidirectionalGraph(vertex => outEdges[vertex], allowParallelEdges: false).ToArrayBidirectionalGraph(),
                placements,
                opaque,
                dangling),
        };

    private static Option<Guid> Container(ArchiveGraph archive, ComponentIdentity member) =>
        toSeq(archive.Components.OutEdges(member)).Find(static edge => edge.Kind == RelationKind.MemberOf).Map(static edge => edge.Target.Id);
}

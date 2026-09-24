using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DefinitionNode(Guid Id, Option<string> Name, InstanceDefinitionUpdateType UpdateType, UnitSystem UnitSystem, Option<string> SourceArchive, bool MembersUnread);

public sealed record DefinitionEdge(Guid Used, Guid Container);

public sealed record InstanceNode(Guid InstanceId, Guid DefinitionId);

public sealed record DefinitionGraph(Seq<DefinitionNode> Nodes, Seq<DefinitionEdge> Edges, Seq<InstanceNode> Instances) {
    public bool Resolved => !Nodes.Exists(static node => node.MembersUnread);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DefinitionGraphs {
    public static IO<DefinitionGraph> ReadGraph(RhinoDoc doc) =>
        from definitions in Definitions.Rows(doc, includeDeleted: false)
        from rows in definitions.TraverseM(static definition =>
            from node in IO.lift(() => DefinitionMapper.ToNode(definition))
            from containers in Definitions.ContainerIds(definition)
            from placements in Definitions.Placements(definition, ReferenceScope.TopLevel)
            select (
                Node: node,
                Edges: containers.Map(container => new DefinitionEdge(node.Id, container)).Strict(),
                Instances: placements.Map(placement => new InstanceNode(placement.Id, node.Id)).Strict()))
            .As()
        select new DefinitionGraph(
            rows.Map(static row => row.Node).Strict(),
            rows.Bind(static row => row.Edges).Strict(),
            rows.Bind(static row => row.Instances).Strict());

    public static IO<DefinitionGraph> ReadGraph(File3dm archive) =>
        from rows in IO.lift(() => toSeq(archive.AllInstanceDefinitions)
            .Map(static definition => (Definition: definition, SourceArchive: Answers.Present(definition.SourceArchive), Members: toSeq(definition.GetObjectIds())))
            .Map(static row => (
                Node: new DefinitionNode(
                    row.Definition.Id,
                    Answers.Present(row.Definition.Name),
                    row.Definition.UpdateType,
                    row.Definition.UnitSystem,
                    row.SourceArchive,
                    MembersUnread: row.SourceArchive.IsSome && row.Members.IsEmpty),
                row.Members))
            .Strict())
        from references in IO.lift(() => toSeq(archive.Objects)
            .Choose(static obj =>
                from reference in Optional(obj.Geometry as InstanceReferenceGeometry)
                from parent in Answers.Present(reference.ParentIdefId)
                select (Instance: obj.Id, Parent: parent))
            .Strict())
        let owners = toHashMap(from row in rows from member in row.Members select (member, row.Node.Id))
        select new DefinitionGraph(
            rows.Map(static row => row.Node).Strict(),
            references.Choose(reference => from owner in owners.Find(reference.Instance) select new DefinitionEdge(reference.Parent, owner)).Strict(),
            references.Filter(reference => !owners.ContainsKey(reference.Instance)).Map(static reference => new InstanceNode(reference.Instance, reference.Parent)).Strict());
}

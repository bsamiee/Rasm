using Rasm.Rhino.Document;
using Rhino;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DefinitionNode(Guid Id, Option<string> Name, bool Linked, Seq<Guid> Members);

public sealed record InstanceReference(Guid Id, Guid Definition, Option<Guid> Container);

public sealed record DefinitionGraph(Seq<DefinitionNode> Nodes, Seq<InstanceReference> References);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DefinitionGraphs {
    public static IO<DefinitionGraph> ReadGraph(RhinoDoc doc) =>
        Definitions.Rows(doc).Map(static definitions => new DefinitionGraph(
            definitions.Map(DefinitionMapper.ToNode).Strict(),
            ((from definition in definitions
              from member in Answers.Present(definition.GetObjects())
              from reference in Optional(member.Geometry as InstanceReferenceGeometry).ToSeq()
              select new InstanceReference(member.Id, reference.ParentIdefId, Some(definition.Id)))
             + (from definition in definitions
                from instance in Answers.Present(definition.GetReferences((int)ReferenceScope.TopLevel))
                select new InstanceReference(instance.Id, definition.Id, Option<Guid>.None)))
            .Strict()));

    public static IO<DefinitionGraph> ReadGraph(File3dm archive) =>
        from nodes in IO.lift(() => toSeq(archive.AllInstanceDefinitions).Map(DefinitionMapper.ToNode).Strict())
        let owners = toHashMap(from node in nodes from member in node.Members select (member, node.Id))
        from references in IO.lift(() =>
            (from held in toSeq(archive.Objects)
             from reference in Optional(held.Geometry as InstanceReferenceGeometry).ToSeq()
             select new InstanceReference(held.Id, reference.ParentIdefId, owners.Find(held.Id)))
            .Strict())
        select new DefinitionGraph(nodes, references);
}

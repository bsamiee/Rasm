using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using File3dmModel = Rhino.FileIO.File3dm;
using File3dmObject = Rhino.FileIO.File3dmObject;
using InstanceReferenceGeometry = Rhino.Geometry.InstanceReferenceGeometry;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BlockArchiveDefinition(
    DefinitionId Id,
    DefinitionName Name,
    Option<ArchivePath> Source,
    ImmutableArray<Guid> MemberIds,
    Option<string> Description,
    Option<ArchivePath> Url) {
    public bool IsLinked => Source.IsSome;

    public static Fin<BlockArchiveDefinition> From(InstanceDefinitionGeometry geometry, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(geometry).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => Project(active: active, op: op)));
    }

    private static Fin<BlockArchiveDefinition> Project(InstanceDefinitionGeometry active, Op op) =>
        from id in DefinitionId.From(value: active.Id, key: op)
        from name in DefinitionName.From(value: active.Name ?? string.Empty, key: op)
        let description = NonBlank(value: active.Description)
        let url = NonBlank(value: active.Url).Bind(value => ArchivePath.From(value: value, key: op).ToOption())
        let source = NonBlank(value: active.SourceArchive).Bind(value => ArchivePath.From(value: value, key: op).ToOption())
        let members = active.GetObjectIds() is Guid[] ids ? [.. ids] : ImmutableArray<Guid>.Empty
        select new BlockArchiveDefinition(
            Id: id,
            Name: name,
            Source: source,
            MemberIds: members,
            Description: description,
            Url: url);

    private static Option<string> NonBlank(string? value) =>
        Optional(value).Filter(static s => !string.IsNullOrWhiteSpace(value: s));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct BlockArchiveInstance(Guid ObjectId, DefinitionId ParentDefId, Transform Xform);

public sealed record BlockArchiveGraph(
    ImmutableArray<BlockArchiveDefinition> Definitions,
    ImmutableArray<BlockArchiveInstance> Instances,
    ImmutableArray<ArchivePath> LinkedArchives) {
    public static BlockArchiveGraph Empty { get; } = new(
        Definitions: [],
        Instances: [],
        LinkedArchives: []);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BlockArchive {
    public static Fin<BlockArchiveGraph> From(File3dmModel model, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(model).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => Build(active: active, op: op)));
    }

    private static Fin<BlockArchiveGraph> Build(File3dmModel active, Op op) =>
        toSeq(active.AllInstanceDefinitions)
            .Map(geometry => BlockArchiveDefinition.From(geometry: geometry, key: op))
            .TraverseM(identity)
            .As()
            .Map(defs => Compose(active: active, definitions: defs));

    private static BlockArchiveGraph Compose(File3dmModel active, Seq<BlockArchiveDefinition> definitions) {
        ImmutableArray<BlockArchiveDefinition> defArray = [.. definitions.ToArr()];
        FrozenDictionary<Guid, BlockArchiveDefinition> lookup = defArray.ToFrozenDictionary(static d => d.Id.Value);
        ImmutableArray<BlockArchiveInstance> instances = ProjectInstances(model: active, lookup: lookup);
        ImmutableArray<ArchivePath> linked = [.. toSeq(defArray)
            .Choose(static d => d.Source)
            .Distinct()];
        return new BlockArchiveGraph(Definitions: defArray, Instances: instances, LinkedArchives: linked);
    }

    public static Fin<DocumentReceipt> AddLinked(File3dmModel model, Seq<FileEndpoint> sources, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(model).ToFin(Fail: op.InvalidInput()).Bind(active => sources.IsEmpty switch {
            true => Fin.Succ(value: DocumentReceipt.Empty),
            false => sources
                .Map(endpoint => op.Catch(() => AddOne(model: active, endpoint: endpoint, op: op)))
                .TraverseM(identity)
                .As()
                .Map(static changes => DocumentReceipt.Empty with { ResourceChanged = changes }),
        });
    }

    public static Seq<FileResourceEntry> ToFileResourceEntries(BlockArchiveGraph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        return toSeq(graph.Definitions).Map(static definition => new FileResourceEntry(
            Kind: DocumentResourceKind.Block,
            Name: Some(definition.Name.Value),
            Path: definition.Source.Map(static path => path.Value),
            Id: Some(definition.Id.Value),
            Source: Some(definition.IsLinked ? "linked-block" : "block")));
    }

    public static Seq<FileResourceEdge> ToFileResourceEdges(BlockArchiveGraph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        return BuildEdges(active: graph);
    }

    private static Seq<FileResourceEdge> BuildEdges(BlockArchiveGraph active) {
        Seq<FileResourceEdge> linkedEdges = toSeq(active.Definitions)
            .Bind(static d => d.Source.Match(
                Some: path => Seq(new FileResourceEdge(
                    FromKind: DocumentResourceKind.Block,
                    FromId: Some(d.Id.Value),
                    ToKind: DocumentResourceKind.FileReference,
                    ToId: Option<Guid>.None,
                    Role: FileResourceRole.Linked,
                    Path: Some(path.Value))),
                None: static () => Seq<FileResourceEdge>()));
        Seq<FileResourceEdge> memberEdges = toSeq(active.Definitions)
            .Bind(static d => toSeq(d.MemberIds.AsEnumerable())
                .Map(memberId => new FileResourceEdge(
                    FromKind: DocumentResourceKind.Block,
                    FromId: Some(d.Id.Value),
                    ToKind: DocumentResourceKind.Object,
                    ToId: Some(memberId),
                    Role: FileResourceRole.Member)));
        Seq<FileResourceEdge> instanceEdges = toSeq(active.Instances.AsEnumerable())
            .Map(static instance => new FileResourceEdge(
                FromKind: DocumentResourceKind.Object,
                FromId: Some(instance.ObjectId),
                ToKind: DocumentResourceKind.Block,
                ToId: Some(instance.ParentDefId.Value),
                Role: FileResourceRole.Instance));
        return linkedEdges + memberEdges + instanceEdges;
    }

    public static int Count(BlockArchiveGraph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        return graph.Definitions.Length;
    }

    public static Seq<string> LinkedPaths(BlockArchiveGraph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        return toSeq(graph.LinkedArchives.AsEnumerable()).Map(static path => path.Value);
    }

    private static ImmutableArray<BlockArchiveInstance> ProjectInstances(
        File3dmModel model,
        FrozenDictionary<Guid, BlockArchiveDefinition> lookup) {
        ImmutableArray<BlockArchiveInstance>.Builder builder = ImmutableArray.CreateBuilder<BlockArchiveInstance>();
        // [BOUNDARY ADAPTER — File3dmObjectTable lacks LINQ-friendly Filter; native iter avoids Seq alloc.]
        foreach (File3dmObject fileObject in model.Objects) {
            if (fileObject.Geometry is not InstanceReferenceGeometry reference) continue;
            Guid parentId = reference.ParentIdefId;
            if (parentId == Guid.Empty || !lookup.TryGetValue(key: parentId, value: out BlockArchiveDefinition? parent)) continue;
            builder.Add(item: new BlockArchiveInstance(
                ObjectId: fileObject.Attributes.ObjectId,
                ParentDefId: parent.Id,
                Xform: reference.Xform));
        }
        return builder.ToImmutable();
    }

    private static Fin<DocumentResourceChange> AddOne(File3dmModel model, FileEndpoint endpoint, Op op) {
        string path = endpoint.Path;
        string name = IOPath.GetFileNameWithoutExtension(path: path);
        return model.AllInstanceDefinitions.AddLinked(filename: path, name: name, description: string.Empty) switch {
            >= 0 => Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: path)),
            _ => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
        };
    }
}

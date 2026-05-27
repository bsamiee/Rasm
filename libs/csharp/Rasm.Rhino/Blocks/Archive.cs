using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using IOPath = System.IO.Path;
using LxHashSet = LanguageExt.HashSet;

namespace Rasm.Rhino.Blocks;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Archive {
    // ---- [MODELS] (nested under Archive — same archival concern) -------------------------
    public sealed record Graph(
        ImmutableArray<Definition> Definitions,
        ImmutableArray<Instance> Instances,
        ImmutableArray<ArchiveLink> LinkedArchives) {
        public static Graph Empty { get; } = new(Definitions: [], Instances: [], LinkedArchives: []);
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Instance(Guid ObjectId, DefinitionId ParentDefId, Transform Xform);

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct LinkedArchiveEdge(ArchivePath FromPath, ArchiveLink Link, ArchivePath ToPath, int Depth);

    private const int LinkedArchiveClosureMaxDepth = 64;

    // ---- [BUILD] -------------------------------------------------------------------------
    public static Fin<Graph> From(File3dm model, Op? key = null, Option<string> archivePath = default) {
        Op op = key.OrDefault();
        Option<string> anchor = archivePath.Bind(static path => BlockPaths.ArchiveAnchor(archivePath: Some(path)));
        return Optional(model).ToFin(Fail: op.InvalidInput())
            .Bind(active => op.Catch(() => Build(active: active, anchorDirectory: anchor, op: op)));
    }

    private static Fin<Graph> Build(File3dm active, Option<string> anchorDirectory, Op op) =>
        from defs in toSeq(active.AllInstanceDefinitions)
            .Map(geo => Definition.From(definition: geo, anchorDirectory: anchorDirectory, key: op))
            .TraverseM(identity).As()
        select Compose(active: active, definitions: defs, anchorDirectory: anchorDirectory, key: op);

    /// Transitive linked-archive walk; each hop resolves `SourceArchive` against `dirname(containing file)`.
    public static Fin<Seq<LinkedArchiveEdge>> LinkedArchiveClosure(File3dm root, string rootPath, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(root).ToFin(Fail: op.InvalidInput())
            .Bind(model => op.Catch(() => Walk(model: model, path: rootPath, depth: 0, visited: LxHashSet.empty<string>(), key: op)));
    }

    private static Fin<Seq<LinkedArchiveEdge>> Walk(File3dm model, string path, int depth, LanguageExt.HashSet<string> visited, Op key) {
        string here = IOPath.GetFullPath(path: path);
        return depth > LinkedArchiveClosureMaxDepth || visited.Contains(key: here)
            ? Fin.Succ(Seq<LinkedArchiveEdge>())
            : Expand(model: model, path: path, here: here, depth: depth, seen: visited.Add(key: here), key: key);
    }

    private static Fin<Seq<LinkedArchiveEdge>> Expand(File3dm model, string path, string here, int depth, LanguageExt.HashSet<string> seen, Op key) =>
        from graph in From(model: model, archivePath: Some(path), key: key)
        from anchor in ArchivePath.From(value: here, key: key)
        from nested in toSeq(graph.LinkedArchives)
            .Filter(link => !seen.Contains(key: IOPath.GetFullPath(path: link.Full.Value)))
            .TraverseM(link => WalkEdge(anchor: anchor, link: link, depth: depth, visited: seen, key: key))
            .As()
        select nested.Fold(Seq<LinkedArchiveEdge>(), static (acc, edges) => acc + edges);

    /// Filter at the caller guarantees `link.Full` is unseen; this method only reads + recurses.
    private static Fin<Seq<LinkedArchiveEdge>> WalkEdge(ArchivePath anchor, ArchiveLink link, int depth, LanguageExt.HashSet<string> visited, Op key) {
        string toFull = IOPath.GetFullPath(path: link.Full.Value);
        return key.Catch(() => {
            using File3dm? child = File3dm.Read(path: toFull);
            return child is null
                ? Fin.Fail<Seq<LinkedArchiveEdge>>(error: key.InvalidInput())
                : Walk(model: child, path: toFull, depth: depth + 1, visited: visited, key: key)
                    .Map(nested => Seq(new LinkedArchiveEdge(FromPath: anchor, Link: link, ToPath: link.Full, Depth: depth)) + nested);
        });
    }

    public static Graph ComposeLive(InstanceDefinitionTable table, Seq<Definition> definitions) {
        HashMap<Guid, Definition> lookup = Lookup(definitions: definitions);
        ImmutableArray<Instance> instances = ProjectInstances(
            lookup: lookup,
            source: definitions.Bind(def =>
                Optional(table.Find(instanceId: def.Id.Value, ignoreDeletedInstanceDefinitions: true))
                    .Map(active => toSeq(active.GetObjects())
                        .Filter(static o => o?.Geometry is InstanceReferenceGeometry)
                        .Map(o => (ObjectId: o!.Id, Reference: (InstanceReferenceGeometry)o.Geometry!)))
                    .IfNone(Seq<(Guid, InstanceReferenceGeometry)>())));
        return Compose(
            definitions: definitions,
            instances: instances,
            linkedArchives: LinkedArchives(definitions: definitions));
    }

    private static ImmutableArray<ArchiveLink> LinkedArchives(Seq<Definition> definitions) =>
        [.. toSeq(definitions).Choose(static d => d.Source)
            .Fold(HashMap<string, ArchiveLink>(), static (acc, link) => acc.AddOrUpdate(key: link.Full.Value, value: link))
            .Values
            .ToSeq()];

    private static HashMap<Guid, Definition> Lookup(Seq<Definition> definitions) =>
        definitions.Fold(
            initialState: HashMap<Guid, Definition>(),
            f: (acc, def) => acc.AddOrUpdate(key: def.Id.Value, value: def));

    private static ImmutableArray<Instance> ProjectInstances(
        HashMap<Guid, Definition> lookup,
        Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)> source) =>
        [.. source.Choose(pair =>
            lookup.Find(key: pair.Reference.ParentIdefId).Map(parent => new Instance(
                ObjectId: pair.ObjectId,
                ParentDefId: parent.Id,
                Xform: pair.Reference.Xform)))];

    private static Graph Compose(Seq<Definition> definitions, ImmutableArray<Instance> instances, ImmutableArray<ArchiveLink> linkedArchives) {
        ImmutableArray<Definition> defArray = [.. definitions];
        return new Graph(Definitions: defArray, Instances: instances, LinkedArchives: linkedArchives);
    }

    public static Graph Subgraph(InstanceDefinitionTable table, Seq<Definition> definitions, Definition anchor) {
        ArgumentNullException.ThrowIfNull(argument: anchor);
        HashMap<Guid, Definition> lookup = Lookup(definitions: definitions);
        LanguageExt.HashSet<Guid> reachable = Reachable(defId: anchor.Id.Value, lookup: lookup, table: table);
        return ComposeLive(table: table, definitions: definitions.Filter(def => reachable.Contains(key: def.Id.Value)));
    }

    // ---- [AUDIT GRAPH] (Blocks.Graph — document dependency audit) ------------------------
    public static Fin<Blocks.Graph> Audit(InstanceDefinitionTable table, Op key) {
        ArgumentNullException.ThrowIfNull(argument: table);
        Option<string> anchor = BlockPaths.DocAnchor(document: table.Document);
        Seq<InstanceDefinition> definitions = toSeq(table.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null);
        (Seq<Blocks.Graph.Node> nodes, Seq<Blocks.Graph.Edge> edges) = definitions
            .Choose(d => ProjectAudit(definition: d, anchorDirectory: anchor, key: key))
            .Fold((Nodes: Seq<Blocks.Graph.Node>(), Edges: Seq<Blocks.Graph.Edge>()), static (acc, pair) =>
                (Nodes: acc.Nodes + pair.Node, Edges: acc.Edges + pair.Edges));
        return Fin.Succ(value: new Blocks.Graph(Nodes: [.. nodes], Edges: [.. edges]));
    }

    /// Single-pass edge projection: members + linked-archive + top/nested inserts share the same `From`.
    private static Option<(Blocks.Graph.Node Node, Seq<Blocks.Graph.Edge> Edges)> ProjectAudit(
        InstanceDefinition definition,
        Option<string> anchorDirectory,
        Op key) =>
        Definition.From(definition: definition, anchorDirectory: anchorDirectory, key: key).ToOption().Map(def => (
            Node: def.ToAuditNode(),
            Edges: toSeq(def.MemberIds).Map(id => new Blocks.Graph.Edge(From: def.Id, Kind: BlockEdgeKind.Member, To: new EdgeTarget.ObjectId(Id: id)))
                + def.Source.Map(link => Seq(new Blocks.Graph.Edge(From: def.Id, Kind: BlockEdgeKind.LinkedArchive, To: new EdgeTarget.ArchiveTarget(Path: link.Full)))).IfNone(Seq<Blocks.Graph.Edge>())
                + toSeq(definition.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native) ?? [])
                    .Filter(static inst => inst is not null)
                    .Map(inst => new Blocks.Graph.Edge(From: def.Id, Kind: BlockEdgeKind.InstanceInsert, To: new EdgeTarget.ObjectId(Id: inst.Id)))));

    // ---- [LIVE GRAPH] (Archive.Graph — bake/plan topology) --------------------------------
    public static Fin<Graph> LiveGraph(InstanceDefinitionTable table, Option<Definition> anchor, Op key) {
        ArgumentNullException.ThrowIfNull(argument: table);
        return toSeq(table.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Map(d => Definition.From(definition: d!, anchorDirectory: BlockPaths.DocAnchor(document: table.Document), key: key))
            .TraverseM(identity).As()
            .Map(definitions => anchor.Case switch {
                Definition a => Subgraph(table: table, definitions: definitions, anchor: a),
                _ => ComposeLive(table: table, definitions: definitions),
            });
    }

    /// Topologically ordered linked definitions intersecting candidates; BakePlan rank drives refresh order.
    public static Fin<Seq<InstanceDefinition>> LinkedRefreshOrder(InstanceDefinitionTable table, Seq<InstanceDefinition> candidates, Op key) {
        ArgumentNullException.ThrowIfNull(argument: table);
        return LiveGraph(table: table, anchor: Option<Definition>.None, key: key)
            .Map(graph => {
                Dictionary<Guid, int> rank = BakePlan(graph: graph)
                    .Select((definition, index) => (definition.Id.Value, index))
                    .ToDictionary(pair => pair.Value, pair => pair.index);
                return toSeq(candidates.OrderBy(d => rank.GetValueOrDefault(key: d.Id, defaultValue: int.MaxValue)));
            });
    }

    private static LanguageExt.HashSet<Guid> Reachable(Guid defId, HashMap<Guid, Definition> lookup, InstanceDefinitionTable table) {
        Seq<Guid> Nested(Guid id) =>
            Optional(table.Find(instanceId: id, ignoreDeletedInstanceDefinitions: true))
                .Map(def => toSeq(def.GetObjects())
                    .Filter(static o => o?.Geometry is InstanceReferenceGeometry)
                    .Map(o => ((InstanceReferenceGeometry)o!.Geometry!).ParentIdefId)
                    .Filter(parent => lookup.ContainsKey(key: parent)))
                .IfNone(Seq<Guid>());
        return ExpandReachable(frontier: Seq(defId), seen: LanguageExt.HashSet<Guid>.Empty.Add(defId), nested: Nested);
    }

    private static LanguageExt.HashSet<Guid> ExpandReachable(
        Seq<Guid> frontier,
        LanguageExt.HashSet<Guid> seen,
        Func<Guid, Seq<Guid>> nested) {
        Seq<Guid> next = frontier.Bind(nested).Filter(id => !seen.Contains(key: id));
        return next.IsEmpty
            ? seen
            : ExpandReachable(
                frontier: next,
                seen: next.Fold(seen, static (acc, id) => acc.Add(key: id)),
                nested: nested);
    }

    internal static Seq<ArchiveLink> LinkedSources(File3dm model, Option<string> anchorDirectory, Op key) =>
        toSeq(model.AllInstanceDefinitions)
            .Choose(def => (Definition.NonBlank(value: def.SourceArchive) | Definition.NonBlank(value: def.Url))
                .Bind(raw => ArchiveLink.Resolve(raw: raw, anchorDirectory: anchorDirectory, key: key).ToOption()));

    private static Graph Compose(File3dm active, Seq<Definition> definitions, Option<string> anchorDirectory, Op key) {
        HashMap<Guid, Definition> lookup = Lookup(definitions: definitions);
        ImmutableArray<Instance> instances = ProjectInstances(
            lookup: lookup,
            source: toSeq(active.Objects)
                .Choose(o => o.Geometry switch {
                    InstanceReferenceGeometry { ParentIdefId: var parentId } r when parentId != Guid.Empty =>
                        lookup.Find(key: parentId).Map(_ => (o.Attributes.ObjectId, r)),
                    _ => Option<(Guid, InstanceReferenceGeometry)>.None,
                }));
        return Compose(
            definitions: definitions,
            instances: instances,
            linkedArchives: LinkedArchives(definitions: definitions, model: active, anchorDirectory: anchorDirectory, key: key));
    }

    private static ImmutableArray<ArchiveLink> LinkedArchives(
        Seq<Definition> definitions,
        File3dm model,
        Option<string> anchorDirectory,
        Op key) =>
        [.. toSeq(definitions).Choose(static d => d.Source)
            .Concat(LinkedSources(model: model, anchorDirectory: anchorDirectory, key: key))
            .Fold(HashMap<string, ArchiveLink>(), static (acc, link) => acc.AddOrUpdate(key: link.Full.Value, value: link))
            .Values
            .ToSeq()];

    public static Fin<DocumentReceipt> AddLinked(File3dm model, Seq<FileEndpoint> sources, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(model).ToFin(Fail: op.InvalidInput()).Bind(active => sources.IsEmpty
            ? Fin.Succ(value: DocumentReceipt.Empty)
            : sources
                .Map(endpoint => op.Catch(() => active.AllInstanceDefinitions.AddLinked(
                    filename: endpoint.StoredLinkPath,
                    name: IOPath.GetFileNameWithoutExtension(path: endpoint.Path),
                    description: string.Empty) switch {
                        >= 0 => Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: endpoint.StoredLinkPath)),
                        _ => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
                    }))
                .TraverseM(identity).As()
                .Map(static changes => DocumentReceipt.Empty with { ResourceChanged = changes }));
    }

    // ---- [BAKE PLAN] (Speckle topological-sort pattern) ----------------------------------
    /// Topologically-sorted bake order; depth = longest (def → nested-def) chain.
    /// Cycle-safe via visiting set; nested-def lookup pre-built so the walk is O(N + E).
    public static Seq<Definition> BakePlan(Graph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        FrozenDictionary<Guid, DefinitionId> instanceByObject = graph.Instances.Length == 0
            ? FrozenDictionary<Guid, DefinitionId>.Empty
            : graph.Instances.ToFrozenDictionary(static i => i.ObjectId, static i => i.ParentDefId);
        HashMap<Guid, ImmutableArray<Guid>> nestedDefs = toSeq(graph.Definitions).Fold(
            initialState: HashMap<Guid, ImmutableArray<Guid>>(),
            f: (acc, def) => acc.AddOrUpdate(key: def.Id.Value, value: NestedDefsOf(def: def, instanceByObject: instanceByObject)));
        HashMap<Guid, int> resolved = toSeq(graph.Definitions).Fold(
            initialState: HashMap<Guid, int>(),
            f: (acc, def) => acc.ContainsKey(key: def.Id.Value)
                ? acc
                : acc.AddOrUpdate(key: def.Id.Value, value: ComputeDepth(defId: def.Id.Value, nestedDefs: nestedDefs, visiting: LxHashSet.empty<Guid>(), memo: acc)));
        return toSeq(graph.Definitions.OrderBy(d => resolved.Find(key: d.Id.Value).IfNone(noneValue: 0)));
    }

    private static ImmutableArray<Guid> NestedDefsOf(Definition def, FrozenDictionary<Guid, DefinitionId> instanceByObject) =>
        [.. toSeq(def.MemberIds).Choose(memberId =>
            instanceByObject.TryGetValue(key: memberId, value: out DefinitionId nested) ? Some(value: nested.Value) : Option<Guid>.None).Distinct()];

    /// Recursive walk; visiting set guards cycles. Pure — HashMap memo is by-value.
    private static int ComputeDepth(Guid defId, HashMap<Guid, ImmutableArray<Guid>> nestedDefs, LanguageExt.HashSet<Guid> visiting, HashMap<Guid, int> memo) =>
        (memo.Find(key: defId), LxHashSet.contains(set: visiting, value: defId)) switch {
            ( { IsSome: true, Case: int cached }, _) => cached,
            (_, true) => 0,        // cycle: zero depth, don't recurse
            _ => 1 + toSeq(nestedDefs.Find(key: defId).IfNone(noneValue: []))
                    .Map(nestedId => ComputeDepth(defId: nestedId, nestedDefs: nestedDefs, visiting: visiting.Add(key: defId), memo: memo))
                    .DefaultIfEmpty(defaultValue: 0)
                    .Max(),
        };

    // ---- [PROJECTIONS] -------------------------------------------------------------------
    public static Seq<FileResourceEntry> ToFileResourceEntries(Graph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        return toSeq(graph.Definitions).Map(static d => new FileResourceEntry(
            Kind: DocumentResourceKind.Block,
            Name: Some(value: d.Name.Value),
            Path: d.Source.Map(static link => link.Stored),
            Id: Some(value: d.Id.Value),
            Source: Some(value: d.IsLinked ? "linked-block" : "block")));
    }

    public static Seq<FileResourceEdge> ToFileResourceEdges(Graph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        Seq<FileResourceEdge> linked = toSeq(graph.Definitions).Bind(static d => d.Source.Match(
            Some: link => Seq(new FileResourceEdge(
                FromKind: DocumentResourceKind.Block, FromId: Some(value: d.Id.Value),
                ToKind: DocumentResourceKind.FileReference, ToId: Option<Guid>.None,
                Role: FileResourceRole.Linked, Path: Some(value: link.Stored))),
            None: static () => Seq<FileResourceEdge>()));
        Seq<FileResourceEdge> members = toSeq(graph.Definitions).Bind(static d => toSeq(d.MemberIds)
            .Map(memberId => new FileResourceEdge(
                FromKind: DocumentResourceKind.Block, FromId: Some(value: d.Id.Value),
                ToKind: DocumentResourceKind.Object, ToId: Some(value: memberId),
                Role: FileResourceRole.Member)));
        Seq<FileResourceEdge> instances = toSeq(graph.Instances).Map(static i => new FileResourceEdge(
            FromKind: DocumentResourceKind.Object, FromId: Some(value: i.ObjectId),
            ToKind: DocumentResourceKind.Block, ToId: Some(value: i.ParentDefId.Value),
            Role: FileResourceRole.Instance));
        return linked + members + instances;
    }

}

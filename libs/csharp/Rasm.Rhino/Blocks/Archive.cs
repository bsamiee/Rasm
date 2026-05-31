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
    internal const int LinkedArchiveClosureMaxDepth = 64;
    // ---- [BUILD] -------------------------------------------------------------------------
    public static Fin<Graph> From(File3dm model, Op? key = null, Option<string> archivePath = default) {
        Op op = key.OrDefault();
        Option<string> anchor = archivePath.Bind(static path => BlockPaths.ArchiveAnchor(archivePath: Some(path)));
        return Optional(model).ToFin(Fail: op.InvalidInput())
            .Bind(active => op.Catch(() => Build(active: active, anchorDirectory: anchor, op: op)));
    }
    private static Fin<Graph> Build(File3dm active, Option<string> anchorDirectory, Op op) =>
        from defs in Rows(() => active.AllInstanceDefinitions)
            .Map(geo => Definition.From(definition: geo, anchorDirectory: anchorDirectory, key: op))
            .TraverseM(identity).As()
        select Compose(active: active, definitions: defs, anchorDirectory: anchorDirectory, key: op);

    /// Transitive linked-archive walk; each hop resolves `SourceArchive` against `dirname(containing file)`.
    /// Fails when <see cref="ValidateArchiveClosure"/> reports broken links or cycles.
    /// Callers needing partial edge lists on broken archives must use <see cref="ValidateArchiveClosure"/> and read <see cref="ArchiveClosureReport.Edges"/> even when <see cref="ArchiveClosureReport.Valid"/> is false.
    public static Fin<Seq<LinkedArchiveEdge>> LinkedArchiveClosure(
        File3dm root,
        string rootPath,
        Op? key = null,
        ClosureValidationPolicy? policy = null) =>
        ValidateArchiveClosure(root: root, rootPath: rootPath, policy: policy, key: key)
            .Bind(report => report.Valid
                ? Fin.Succ(value: report.Edges)
                : Fin.Fail<Seq<LinkedArchiveEdge>>(error: key.OrDefault().InvalidResult(detail: nameof(LinkedArchiveClosure))));

    /// Read-only linked-archive closure audit: existence, readability, and cycle detection without document mutation.
    /// Always returns discovered <see cref="ArchiveClosureReport.Edges"/> before validity is evaluated; broken or cyclic archives still populate partial edge lists.
    public static Fin<ArchiveClosureReport> ValidateArchiveClosure(
        File3dm root,
        string rootPath,
        ClosureValidationPolicy? policy = null,
        Op? key = null) {
        Op op = key.OrDefault();
        ClosureValidationPolicy admitted = policy ?? ClosureValidationPolicy.Default;
        return from validPolicy in admitted.Admit(key: op)
               from model in Optional(root).ToFin(Fail: op.InvalidInput())
               from state in op.Catch(() => WalkClosure(
                   model: model,
                   path: rootPath,
                   depth: 0,
                   stack: Seq<string>(),
                   visited: LxHashSet.empty<string>(),
                   state: ClosureState.Empty,
                   policy: validPolicy,
                   key: op))
               select state.Report(policy: validPolicy);
    }
    private sealed record ClosureState(
        Seq<LinkedArchiveEdge> Edges,
        Seq<ArchivePath> Broken,
        Seq<Seq<ArchivePath>> Cycles) {
        public static ClosureState Empty { get; } = new(Edges: Seq<LinkedArchiveEdge>(), Broken: Seq<ArchivePath>(), Cycles: Seq<Seq<ArchivePath>>());
        public ArchiveClosureReport Report(ClosureValidationPolicy policy) =>
            new(
                Valid: Broken.IsEmpty && (!policy.DetectCycles || Cycles.IsEmpty),
                Edges: Edges,
                Broken: Broken,
                Cycles: Cycles);
    }
    private static Fin<ClosureState> WalkClosure(
        File3dm model,
        string path,
        int depth,
        Seq<string> stack,
        LanguageExt.HashSet<string> visited,
        ClosureState state,
        ClosureValidationPolicy policy,
        Op key) {
        string here = IOPath.GetFullPath(path: path);
        bool onStack = stack.Any(item => string.Equals(a: item, b: here, comparisonType: StringComparison.OrdinalIgnoreCase));
        return (depth > policy.MaxDepth, onStack, visited.Contains(key: here)) switch {
            (true, _, _) => Fin.Succ(state),
            (_, true, _) when policy.DetectCycles => Fin.Succ(state with {
                Cycles = state.Cycles + CyclePath(stack: stack, cycleAt: here, key: key),
            }),
            (_, true, _) => Fin.Succ(state),
            (_, _, true) => Fin.Succ(state),
            _ => ExpandClosure(
                model: model,
                path: path,
                here: here,
                depth: depth,
                stack: stack.Add(here),
                visited: visited.Add(key: here),
                state: state,
                policy: policy,
                key: key),
        };
    }
    private static Fin<ClosureState> ExpandClosure(
        File3dm model,
        string path,
        string here,
        int depth,
        Seq<string> stack,
        LanguageExt.HashSet<string> visited,
        ClosureState state,
        ClosureValidationPolicy policy,
        Op key) =>
        From(model: model, archivePath: Some(path), key: key).Match(
            Fail: _ => Fin.Succ(state with { Broken = state.Broken + BrokenPath(here: here, key: key) }),
            Succ: graph => ArchivePath.From(value: here, key: key).Match(
                Fail: _ => Fin.Succ(state),
                Succ: anchorPath => toSeq(graph.LinkedArchives)
                    .Fold(
                        initialState: Fin.Succ(state),
                        f: (stateFin, link) => stateFin.Bind(acc => WalkEdge(
                            anchor: anchorPath,
                            link: link,
                            depth: depth,
                            stack: stack,
                            visited: visited,
                            state: acc,
                            policy: policy,
                            key: key)))));
    private static Fin<ClosureState> WalkEdge(
        ArchivePath anchor,
        ArchiveLink link,
        int depth,
        Seq<string> stack,
        LanguageExt.HashSet<string> visited,
        ClosureState state,
        ClosureValidationPolicy policy,
        Op key) {
        string toFull = IOPath.GetFullPath(path: link.Full.Value);
        LinkedArchiveEdge edge = new(FromPath: anchor, Link: link, ToPath: link.Full, Depth: depth);
        ClosureState next = state with { Edges = state.Edges + edge };
        return File.Exists(path: toFull)
            ? ReadNestedArchive(toFull: toFull, next: next, depth: depth, stack: stack, visited: visited, policy: policy, key: key)
            : Fin.Succ(next with { Broken = next.Broken + BrokenPath(here: toFull, key: key) });
    }
    // BOUNDARY ADAPTER — File3dm.Read owns native archive lifetime.
    private static Fin<ClosureState> ReadNestedArchive(
        string toFull,
        ClosureState next,
        int depth,
        Seq<string> stack,
        LanguageExt.HashSet<string> visited,
        ClosureValidationPolicy policy,
        Op key) =>
        key.Catch(() => {
            using File3dm? child = File3dm.Read(path: toFull);
            return child switch {
                null => Fin.Succ(next with { Broken = next.Broken + BrokenPath(here: toFull, key: key) }),
                File3dm model => WalkClosure(
                    model: model,
                    path: toFull,
                    depth: depth + 1,
                    stack: stack,
                    visited: visited,
                    state: next,
                    policy: policy,
                    key: key),
            };
        });
    private static Seq<ArchivePath> BrokenPath(string here, Op key) =>
        ArchivePath.From(value: here, key: key).Match(Succ: path => Seq(path), Fail: _ => Seq<ArchivePath>());
    private static Seq<Seq<ArchivePath>> CyclePath(Seq<string> stack, string cycleAt, Op key) {
        Seq<ArchivePath> closed = toSeq(stack.SkipWhile(item => !string.Equals(a: item, b: cycleAt, comparisonType: StringComparison.OrdinalIgnoreCase)).Append(cycleAt))
            .Choose(item => ArchivePath.From(value: item, key: key).ToOption());
        return closed.IsEmpty ? Seq<Seq<ArchivePath>>() : Seq(closed);
    }
    private static Graph ComposeFrom(
        Seq<Definition> definitions,
        Func<HashMap<Guid, Definition>, Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>> source,
        ImmutableArray<ArchiveLink> linkedArchives) {
        HashMap<Guid, Definition> lookup = Lookup(definitions: definitions);
        return Compose(definitions: definitions, instances: ProjectInstances(lookup: lookup, source: source(arg: lookup)), linkedArchives: linkedArchives);
    }
    public static Graph ComposeLive(InstanceDefinitionTable table, Seq<Definition> definitions) =>
        ComposeFrom(
            definitions: definitions,
            source: _ => definitions.Bind(def =>
                Optional(table.Find(instanceId: def.Id.Value, ignoreDeletedInstanceDefinitions: true))
                    .Map(active => Operations.NestedReferences(def: active))
                    .IfNone(Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>())),
            linkedArchives: LinkedArchives(definitions: definitions));
    private static ImmutableArray<ArchiveLink> LinkedArchives(
        Seq<Definition> definitions,
        Option<File3dm> model = default,
        Option<string> anchorDirectory = default,
        Op? key = null) {
        Op op = key.OrDefault();
        Seq<ArchiveLink> fromModel = model.Case switch {
            File3dm active => LinkedSources(model: active, anchorDirectory: anchorDirectory, key: op),
            _ => Seq<ArchiveLink>(),
        };
        return [.. toSeq(definitions).Choose(static d => d.Source)
            .Concat(fromModel)
            .Fold(HashMap<string, ArchiveLink>(), static (acc, link) => acc.AddOrUpdate(key: link.Full.Value, value: link))
            .Values
            .ToSeq()];
    }
    private static HashMap<Guid, Definition> Lookup(Seq<Definition> definitions) =>
        definitions.Fold(
            initialState: HashMap<Guid, Definition>(),
            f: (acc, def) => acc.AddOrUpdate(key: def.Id.Value, value: def));
    // --- [WALK] -----------------------------------------------------------------------------
    public readonly record struct DefinitionWalkFrame(
        Transform Composed,
        ImmutableArray<DefinitionId> Path,
        int Depth,
        Option<Guid> InstanceId,
        Option<Guid> MemberId);
    internal readonly record struct DefinitionWalkNode(
        Option<ReachInsert> Reach,
        Option<FlatPiece> Flat);
    internal static Seq<DefinitionWalkNode> WalkDefinitions(
        InstanceDefinitionTable table,
        InstanceObject seed,
        Transform parent,
        ImmutableArray<DefinitionId> path,
        int depth,
        DepthPolicy policy,
        bool flatLeaves,
        Op key) =>
        (flatLeaves && depth >= policy.MaxDepth, seed.InstanceDefinition) switch {
            (true, _) or (_, null) => Seq<DefinitionWalkNode>(),
            (_, InstanceDefinition def) => ExpandDefinitions(
                table: table,
                def: def,
                frame: new DefinitionWalkFrame(
                    Composed: parent * seed.InstanceXform,
                    Path: path,
                    Depth: flatLeaves ? depth + 1 : depth,
                    InstanceId: Some(seed.Id),
                    MemberId: Option<Guid>.None),
                policy: policy,
                flatLeaves: flatLeaves,
                key: key),
        };
    private static Seq<DefinitionWalkNode> ExpandDefinitions(
        InstanceDefinitionTable table,
        InstanceDefinition def,
        DefinitionWalkFrame frame,
        DepthPolicy policy,
        bool flatLeaves,
        Op key) =>
        DefinitionId.From(value: def.Id, key: key).ToOption().Map(defId => {
            ImmutableArray<DefinitionId> nextPath = frame.Path.Add(item: defId);
            Seq<DefinitionWalkNode> prefix = flatLeaves
                ? Seq<DefinitionWalkNode>()
                : Seq(new DefinitionWalkNode(
                    Reach: Some(new ReachInsert(
                        InstanceId: frame.InstanceId.IfNone(noneValue: frame.MemberId.IfNone(noneValue: Guid.Empty)),
                        DefId: defId,
                        WorldXform: frame.Composed,
                        Depth: frame.Depth,
                        Path: nextPath)),
                    Flat: Option<FlatPiece>.None));
            bool stop = flatLeaves
                ? frame.Depth >= policy.MaxDepth
                : (policy.StopOnCycle && frame.Path.Contains(defId)) || frame.Depth >= policy.MaxDepth;
            return stop
                ? prefix
                : prefix + Operations.BindDefinitionMembers(
                    def: def,
                    composed: frame.Composed,
                    onInstance: (nested, parentXform) => WalkDefinitions(
                        table: table,
                        seed: nested,
                        parent: parentXform,
                        path: nextPath,
                        depth: frame.Depth + 1,
                        policy: policy,
                        flatLeaves: flatLeaves,
                        key: key),
                    onReference: (reference, memberId, parentXform) =>
                        Optional(table.Find(instanceId: reference.ParentIdefId, ignoreDeletedInstanceDefinitions: true))
                            .Map(nestedDef => ExpandDefinitions(
                                table: table,
                                def: nestedDef,
                                frame: new DefinitionWalkFrame(
                                    Composed: parentXform * reference.Xform,
                                    Path: nextPath,
                                    Depth: frame.Depth + 1,
                                    InstanceId: frame.InstanceId,
                                    MemberId: Some(memberId)),
                                policy: policy,
                                flatLeaves: flatLeaves,
                                key: key))
                            .IfNone(Seq<DefinitionWalkNode>()),
                    onLeaf: flatLeaves
                        ? (leaf, parentXform) => leaf.Geometry is GeometryBase g
                            ? Seq(new DefinitionWalkNode(
                                Reach: Option<ReachInsert>.None,
                                Flat: Some(new FlatPiece(Geometry: g, Composed: parentXform, Path: nextPath))))
                            : Seq<DefinitionWalkNode>()
                        : null);
        }).IfNone(Seq<DefinitionWalkNode>());
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
    // ---- [AUDIT GRAPH] (Blocks.AuditGraph — document dependency audit) -------------------
    public static Fin<AuditGraph> Audit(InstanceDefinitionTable table, Op key) {
        ArgumentNullException.ThrowIfNull(argument: table);
        Option<string> anchor = BlockPaths.DocAnchor(document: table.Document);
        Seq<InstanceDefinition> definitions = Definition.List(table: table);
        (Seq<AuditGraph.Node> nodes, Seq<AuditGraph.Edge> edges) = definitions
            .Choose(d => ProjectAudit(definition: d, anchorDirectory: anchor, key: key))
            .Fold((Nodes: Seq<AuditGraph.Node>(), Edges: Seq<AuditGraph.Edge>()), static (acc, pair) =>
                (Nodes: acc.Nodes + pair.Node, Edges: acc.Edges + pair.Edges));
        return Fin.Succ(value: new AuditGraph(Nodes: [.. nodes], Edges: [.. edges]));
    }

    /// Single-pass edge projection: members + linked-archive + top/nested inserts share the same `From`.
    private static Option<(AuditGraph.Node Node, Seq<AuditGraph.Edge> Edges)> ProjectAudit(
        InstanceDefinition definition,
        Option<string> anchorDirectory,
        Op key) =>
        Definition.From(definition: definition, anchorDirectory: anchorDirectory, key: key).ToOption().Map(def => (
            Node: def.ToAuditNode(),
            Edges: toSeq(def.MemberIds).Map(id => new AuditGraph.Edge(From: def.Id, Kind: BlockEdgeKind.Member, To: new EdgeTarget.ObjectId(Id: id)))
                + def.Source.Map(link => Seq(new AuditGraph.Edge(From: def.Id, Kind: BlockEdgeKind.LinkedArchive, To: new EdgeTarget.ArchiveTarget(Path: link.Full)))).IfNone(Seq<AuditGraph.Edge>())
                + toSeq(definition.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native) ?? [])
                    .Filter(static inst => inst is not null)
                    .Map(inst => new AuditGraph.Edge(From: def.Id, Kind: BlockEdgeKind.InstanceInsert, To: new EdgeTarget.ObjectId(Id: inst!.Id)))));
    // ---- [LIVE GRAPH] (Archive.Graph — bake/plan topology) --------------------------------
    public static Fin<Graph> LiveGraph(InstanceDefinitionTable table, Option<Definition> anchor, Op key) {
        ArgumentNullException.ThrowIfNull(argument: table);
        return Definition.List(table: table)
            .Map(d => Definition.From(definition: d, anchorDirectory: BlockPaths.DocAnchor(document: table.Document), key: key))
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
                .Map(def => Operations.NestedReferences(def: def)
                    .Map(pair => pair.Reference.ParentIdefId)
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
        Rows(() => model.AllInstanceDefinitions)
            .Choose(def => Definition.NonBlank(value: def.SourceArchive)
                .Bind(raw => ArchiveLink.Resolve(raw: raw, anchorDirectory: anchorDirectory, key: key).ToOption()));
    private static Graph Compose(File3dm active, Seq<Definition> definitions, Option<string> anchorDirectory, Op key) {
        Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)> topLevel = Rows(() => active.Objects)
            .Choose(o => Native(() => o.Geometry).Bind(geometry => geometry switch {
                InstanceReferenceGeometry { ParentIdefId: var parentId } r when parentId != Guid.Empty =>
                    Native(() => o.Attributes).Map(attributes => (attributes.ObjectId, Reference: r)),
                _ => Option<(Guid ObjectId, InstanceReferenceGeometry Reference)>.None,
            }));
        Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)> memberRefs = definitions.Bind((Definition snap) =>
            toSeq(snap.MemberIds).Bind((Guid memberId) =>
                Native(() => active.Objects)
                    .Bind(objects => Native(() => objects.FindId(id: memberId))).Case switch {
                        File3dmObject member when Native(() => member.Geometry).Case is InstanceReferenceGeometry reference =>
                            Seq((ObjectId: memberId, Reference: (InstanceReferenceGeometry)reference.Duplicate())),
                        _ => Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>(),
                    }));
        LanguageExt.HashSet<Guid> memberIds = definitions
            .Bind(static definition => toSeq(definition.MemberIds))
            .Fold(LanguageExt.HashSet<Guid>.Empty, static (ids, id) => ids.Add(key: id));
        return ComposeFrom(
            definitions: definitions,
            source: _ => topLevel.Filter(instance => !memberIds.Contains(key: instance.ObjectId)) + memberRefs,
            linkedArchives: LinkedArchives(definitions: definitions, model: Some(active), anchorDirectory: anchorDirectory, key: key));
    }

    private static Seq<T> Rows<T>(IEnumerable<T>? source) =>
        source is null ? Seq<T>() : toSeq(source).Filter(static item => item is not null);
    private static Seq<T> Rows<T>(Func<IEnumerable<T>?> read) =>
        Native(read: read).Map(source => Rows(source: source)).IfNone(Seq<T>());
    private static Option<T> Native<T>(Func<T> read) {
        try {
            return Optional(read()).Filter(static value => value is not null);
        } catch (NullReferenceException) {
            return Option<T>.None;
        }
    }
    // ---- [BAKE PLAN] (Speckle topological-sort pattern) ----------------------------------
    /// Topologically-sorted bake order; depth = longest (def -> nested-def) chain.
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

# [RASM_RHINO_BLOCK_GRAPH]

The definition-graph rail (`Rasm.Rhino.Blocks`). One `GraphSource` union admits the live `InstanceDefinitionTable` and the offline `File3dm` archive into one folded topology — definition nodes keyed by guid, nesting edges oriented dependency-first — and one `BlockGraph.Ask` entry answers every topology question: containers, placed references, nesting depth, dependency ordering, cycle grouping, usage tallies, and the linked-archive closure. Reachability, strongly connected components, and topological order COMPOSE `QuikGraph` over a transient `BidirectionalGraph<Guid, SEdge<Guid>>` folded per ask — the census-era hand-rolled reachability, cycle, and depth passes are dead, and the hard-coded closure depth ceiling dies with them: the closure walk is cycle-guarded by the visited set, never bounded by a magic constant. Unit facts cross as evidence — an archive's `UnitSystem` beside the host's — and any rescaling composes the kernel `Context`, never a second folder-local unit policy.

## [01]-[INDEX]

- [02]-[SOURCE_AND_TOPOLOGY]: `GraphSource`, `DefinitionNode`, and the one topology fold onto the QuikGraph container.
- [03]-[ASK_FAMILY]: `BlockGraphAsk`/`BlockGraphAnswer` — the topology question vocabulary over the folded graph.
- [04]-[ARCHIVE_CLOSURE]: `ClosureEdge`, `UnitFact`, `ClosureReport`, and the cycle-guarded linked-archive walk.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SOURCE_AND_TOPOLOGY]

- Owner: `GraphSource` `[Union]` — `Live` over an admitted session, `Loaded` over an archive already opened by the exchange rail, `Stored` over a path this rail reads through `ReadWithLog`; `DefinitionNode` — the per-definition node fact: key, index, name, update discriminant, archive units; the internal `Topology` product — nodes plus dependency-first edges.
- Law: edges orient dependency-first — an edge runs from the used definition to its container, so `SourceFirstTopologicalSort` yields the bake order directly (a nested definition lands before every definition that places it) and no consumer reverses an order by hand.
- Law: the graph is transient per ask, never a stored field — the fold runs against live state at each question, the QuikGraph container never escapes, and every result projects onto typed keys; invalidation is therefore structural, and a consumer wanting cheap re-asks keys its own memo on the operation receipts.
- Law: the live fold reads the host topology members — `GetList` for the roster, `GetContainers` for nesting — and the offline fold reads the verified `AllInstanceDefinitions` roster plus the top-level placement references off the object table's `InstanceReferenceGeometry` entries.
- Law: an offline node carries its archive's `UnitSystem` — the unit mismatch between an archive and the host document is a reported fact on the node, and geometry rescaling composes the kernel `Context`; `RhinoMath.UnitScale` never appears in this package.
- RESEARCH: the archive-side member-object roster (nested placement edges inside a `File3dmInstanceDefinitionTable` definition) is unverified — the offline fold carries definition nodes and top-level placement edges now, and the nested offline edge extraction lands as one fold extension after catalog verification.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphSource {
    private GraphSource() { }
    public sealed record Live(DocumentSession Session) : GraphSource;
    public sealed record Loaded(File3dm Archive, Option<string> Path = default) : GraphSource;
    public sealed record Stored(string Path) : GraphSource;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record DefinitionNode(
    Guid Key,
    int Index,
    string Name,
    Option<InstanceDefinitionUpdateType> UpdateType,
    Option<UnitSystem> ArchiveUnits);

internal sealed record Topology(Seq<DefinitionNode> Nodes, Seq<(Guid Used, Guid Container)> Edges, Seq<Guid> TopLevelPlacements) : IDetachedDocumentResult {
    internal BidirectionalGraph<Guid, SEdge<Guid>> Fold() {
        BidirectionalGraph<Guid, SEdge<Guid>> graph = new(allowParallelEdges: false);
        _ = graph.AddVertexRange(Nodes.Map(static node => node.Key).AsIterable());
        _ = Edges.Iter(edge => graph.AddEdge(new SEdge<Guid>(edge.Used, edge.Container)));
        return graph;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class TopologyFold {
    internal static Fin<Topology> Of(GraphSource source, Op key) =>
        Optional(source).ToFin(Fail: key.InvalidInput()).Bind(request => request.Switch(
            state: key,
            live: static (op, held) => held.Session.Demand(
                use: document => op.Catch(() => {
                    Seq<InstanceDefinition> roster = toSeq(document.InstanceDefinitions.GetList(ignoreDeleted: true))
                        .Choose(static definition => Optional(definition));
                    Seq<DefinitionNode> nodes = roster.Map(definition => new DefinitionNode(
                        Key: definition.Id, Index: definition.Index, Name: definition.Name,
                        UpdateType: Some(definition.UpdateType), ArchiveUnits: Option<UnitSystem>.None));
                    Seq<(Guid Used, Guid Container)> edges = roster.Bind(definition =>
                        toSeq(definition.GetContainers()).Map(container => (definition.Id, container.Id)));
                    Seq<Guid> placed = roster
                        .Filter(static definition => definition.UseCount() > 0)
                        .Map(static definition => definition.Id);
                    return Fin.Succ(value: new Topology(Nodes: nodes, Edges: edges.Distinct(), TopLevelPlacements: placed));
                }),
                key: op,
                needs: [SessionNeed.Read]),
            loaded: static (op, held) => Offline(archive: held.Archive, op: op),
            stored: static (op, held) =>
                from path in op.AcceptText(value: held.Path)
                from topology in op.Catch(() => {
                    File3dm archive = File3dm.ReadWithLog(path: path, errorLog: out string log);
                    return Optional(archive).ToFin(Fail: op.InvalidResult(detail: log)).Bind(model => {
                        Fin<Topology> folded = Offline(archive: model, op: op);
                        model.Dispose();
                        return folded;
                    });
                })
                select topology));

    private static Fin<Topology> Offline(File3dm archive, Op op) =>
        op.Catch(() => {
            UnitSystem units = archive.Settings.ModelUnitSystem;
            Seq<DefinitionNode> nodes = toSeq(archive.AllInstanceDefinitions).Map(definition => new DefinitionNode(
                Key: definition.Id, Index: -1, Name: definition.Name,
                UpdateType: Option<InstanceDefinitionUpdateType>.None, ArchiveUnits: Some(units)));
            Seq<Guid> placed = toSeq(archive.Objects)
                .Choose(static entry => Optional(entry.Geometry as InstanceReferenceGeometry))
                .Map(static reference => reference.ParentIdefId)
                .Filter(static parent => parent != Guid.Empty)
                .Distinct();
            return Fin.Succ(value: new Topology(Nodes: nodes, Edges: Seq<(Guid, Guid)>(), TopLevelPlacements: placed));
        });
}
```

## [03]-[ASK_FAMILY]

- Owner: `BlockGraphAsk` `[Union]` — the topology questions: `Containers` the definitions nesting a target, `References` the placed instances under a scope, `Nesting` the depth of one definition inside another, `Order` the dependency-first bake ordering, `Cycles` the strongly-connected groups, `Placed` the top-level placement census, `Tally` the split usage counts; `BlockGraphAnswer` `[Union]` — one typed result case per question.
- Entry: `BlockGraph.Ask(GraphSource, BlockGraphAsk) : Fin<BlockGraphAnswer>` — folds the topology once, dispatches the question, and projects every graph result back onto guid keys.
- Law: order and cycles are QuikGraph, never local walks — `Order` pre-gates with `IsDirectedAcyclicGraph` and answers `SourceFirstTopologicalSort`, so a residual cycle surfaces as a typed fault rather than a thrown `NonAcyclicGraphException`; `Cycles` labels components through the one vertex-generic `CycleGroups` SCC fold the archive closure shares, reporting every group larger than one.
- Law: per-definition questions read the host member that already answers them — `Containers`, `References`, `Nesting`, and `Tally` resolve through `GetContainers`, `GetReferences`, `UsesDefinition`, and `UseCount` on the live definition inside one `Demand` window, so the folded graph serves the whole-topology questions, no host answer is re-derived from edges, and no live definition handle escapes the window.
- Law: live-only questions refuse offline sources typed — `Containers`, `References`, `Nesting`, and `Tally` demand a `Live` source because the host members carry the answer; the whole-graph questions serve every source.
- Growth: a new topology question is one ask case with its answer case; the fold and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockGraphAsk {
    private BlockGraphAsk() { }
    public sealed record Containers(BlockRef Target) : BlockGraphAsk;
    public sealed record References(BlockRef Target, ReferenceScope Scope) : BlockGraphAsk;
    public sealed record Nesting(BlockRef Outer, BlockRef Inner) : BlockGraphAsk;
    public sealed record Order : BlockGraphAsk;
    public sealed record Cycles : BlockGraphAsk;
    public sealed record Placed : BlockGraphAsk;
    public sealed record Tally(BlockRef Target) : BlockGraphAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockGraphAnswer : IDetachedDocumentResult {
    private BlockGraphAnswer() { }
    public sealed record Definitions(Seq<Guid> Keys) : BlockGraphAnswer;
    public sealed record Placements(Seq<Guid> InstanceIds) : BlockGraphAnswer;
    public sealed record Depth(int Levels) : BlockGraphAnswer;
    public sealed record Ordered(Seq<Guid> BakeOrder) : BlockGraphAnswer;
    public sealed record Groups(Seq<Seq<Guid>> Cyclic) : BlockGraphAnswer;
    public sealed record Usage(BlockUsage Counts) : BlockGraphAnswer;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BlockGraph {
    public static Fin<BlockGraphAnswer> Ask(GraphSource source, BlockGraphAsk question) {
        Op op = Op.Of();
        return from active in Optional(question).ToFin(Fail: op.InvalidInput())
               from answer in active.Switch(
                   state: (Source: source, Op: op),
                   containers: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from definition in ask.Target.Resolve(document: document, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Definitions(
                           Keys: toSeq(definition.GetContainers()).Map(static container => container.Id))),
                   references: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from definition in ask.Target.Resolve(document: document, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Placements(
                           InstanceIds: toSeq(definition.GetReferences(wheretoLook: (int)ask.Scope)).Map(static instance => instance.Id))),
                   nesting: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from outer in ask.Outer.Resolve(document: document, key: ctx.Op)
                       from inner in ask.Inner.Resolve(document: document, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Depth(Levels: outer.UsesDefinition(otherIdefIndex: inner.Index))),
                   order: static (ctx, _) =>
                       from topology in TopologyFold.Of(source: ctx.Source, key: ctx.Op)
                       from ordered in ctx.Op.Catch(() => {
                           BidirectionalGraph<Guid, SEdge<Guid>> graph = topology.Fold();
                           return graph.IsDirectedAcyclicGraph()
                               ? Fin.Succ(value: toSeq(graph.SourceFirstBidirectionalTopologicalSort()))
                               : Fin.Fail<Seq<Guid>>(error: ctx.Op.InvalidResult(detail: nameof(BlockGraphAsk.Cycles)));
                       })
                       select (BlockGraphAnswer)new BlockGraphAnswer.Ordered(BakeOrder: ordered),
                   cycles: static (ctx, _) =>
                       from topology in TopologyFold.Of(source: ctx.Source, key: ctx.Op)
                       from groups in ctx.Op.Catch(() => Fin.Succ(value: CycleGroups.Of(graph: topology.Fold())))
                       select (BlockGraphAnswer)new BlockGraphAnswer.Groups(Cyclic: groups),
                   placed: static (ctx, _) =>
                       from topology in TopologyFold.Of(source: ctx.Source, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Definitions(Keys: topology.TopLevelPlacements),
                   tally: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from definition in ask.Target.Resolve(document: document, key: ctx.Op)
                       from usage in ctx.Op.Catch(() => {
                           int total = definition.UseCount(topLevelReferenceCount: out int topLevel, nestedReferenceCount: out int nested);
                           return Fin.Succ(value: new BlockUsage(Total: total, TopLevel: topLevel, Nested: nested));
                       })
                       select (BlockGraphAnswer)new BlockGraphAnswer.Usage(Counts: usage)))
               select answer;
    }

    private static Fin<BlockGraphAnswer> Live(GraphSource source, Op op, Func<RhinoDoc, Fin<BlockGraphAnswer>> read) =>
        source is GraphSource.Live live
            ? live.Session.Demand(use: read, key: op, needs: [SessionNeed.Read])
            : Fin.Fail<BlockGraphAnswer>(error: op.Unsupported(geometryType: typeof(GraphSource), outputType: typeof(BlockGraphAnswer)));
}

internal static class CycleGroups {
    internal static Seq<Seq<TVertex>> Of<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph, IEqualityComparer<TVertex>? comparer = null) where TVertex : notnull {
        Dictionary<TVertex, int> labels = comparer is null ? new() : new(comparer);
        _ = graph.StronglyConnectedComponents(components: labels);
        return toSeq(labels)
            .Map(static pair => (pair.Key, pair.Value))
            .GroupBy(static pair => pair.Value)
            .AsIterable()
            .Map(static group => toSeq(group).Map(static pair => pair.Key))
            .Filter(static members => members.Count > 1)
            .ToSeq();
    }
}
```

## [04]-[ARCHIVE_CLOSURE]

- Owner: `ClosureEdge` — one linked-archive dependency fact: the referencing archive, the raw stored link, the resolved target; `UnitFact` — one archive-units evidence row; `ClosureReport` — the whole walk product: edges, broken paths, cyclic path groups, unit facts, and the native read logs.
- Entry: `ArchiveClosure.Closure(string RootPath) : Fin<ClosureReport>` — walks the linked-archive graph breadth-first from the root, reading each archive once under a visited set and extracting each archive's linked sources from the verified `SourceArchive` member on its definition rows, so the census-era depth ceiling and the caller-supplied extraction column are both dead.
- Law: every archive reads exactly once — the visited set keys on the full resolved path, a re-encountered path is a shared dependency the walk skips silently, and a failing read records a broken edge with its native log rather than aborting the walk; the report is total over the reachable closure.
- Law: cycles are graph facts, never walk accidents — a diamond (two archives linking one sub-archive) is not a cycle, so the walk records edges only and the cycle groups derive post-walk from `StronglyConnectedComponents` over the edge set; a group larger than one is a genuine circular link chain.
- Law: a stored link resolves against its referencing archive's directory — relative source paths are anchored per edge, never against the process working directory, and the raw stored form survives on the edge beside the resolution so relativity evidence is never erased.
- Law: unit evidence accumulates per archive — each read records its `ModelUnitSystem` beside the path, and a consumer reconciling mixed-unit closures composes the kernel `Context` for any rescale decision.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ClosureEdge(string FromPath, string StoredLink, Option<string> ResolvedPath);

public readonly record struct UnitFact(string Path, UnitSystem Units);

public sealed record ClosureReport(
    Seq<ClosureEdge> Edges,
    Seq<string> Broken,
    Seq<Seq<string>> Cycles,
    Seq<UnitFact> Units,
    Seq<string> NativeLog) {
    public bool Sound => Broken.IsEmpty && Cycles.IsEmpty;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ArchiveClosure {
    public static Fin<ClosureReport> Closure(string rootPath) {
        Op op = Op.Of();
        return from root in op.AcceptText(value: rootPath)
               from walked in op.Catch(() => Fin.Succ(value: Walk(
                   pending: Seq(System.IO.Path.GetFullPath(path: root)),
                   visited: LanguageExt.HashSet<string>.Empty,
                   state: new ClosureReport(
                       Edges: Seq<ClosureEdge>(), Broken: Seq<string>(), Cycles: Seq<Seq<string>>(),
                       Units: Seq<UnitFact>(), NativeLog: Seq<string>()))))
               from report in op.Catch(() => Fin.Succ(value: walked with { Cycles = CyclesOf(edges: walked.Edges) }))
               select report;
    }

    private static ClosureReport Walk(Seq<string> pending, LanguageExt.HashSet<string> visited, ClosureReport state) =>
        pending.Head.Case switch {
            string path when visited.Contains(key: path) =>
                Walk(pending: pending.Tail, visited: visited, state: state),
            string path => Optional(File3dm.ReadWithLog(path: path, errorLog: out string log)).Case switch {
                File3dm archive => Step(path: path, archive: archive, log: log, rest: pending.Tail,
                    visited: visited.Add(key: path), state: state),
                _ => Walk(pending: pending.Tail, visited: visited.Add(key: path),
                    state: state with { Broken = state.Broken.Add(value: path), NativeLog = state.NativeLog.Add(value: log) }),
            },
            _ => state,
        };

    private static ClosureReport Step(
        string path, File3dm archive, string log, Seq<string> rest,
        LanguageExt.HashSet<string> visited, ClosureReport state) {
        string anchor = System.IO.Path.GetDirectoryName(path: path) ?? string.Empty;
        Seq<ClosureEdge> edges = toSeq(archive.AllInstanceDefinitions)
            .Choose(definition => Optional(definition.SourceArchive).Filter(static stored => stored.Length > 0))
            .Distinct()
            .Map(stored => System.IO.Path.GetFullPath(path: stored, basePath: anchor) switch {
                var resolved when System.IO.File.Exists(path: resolved) =>
                    new ClosureEdge(FromPath: path, StoredLink: stored, ResolvedPath: Some(resolved)),
                _ => new ClosureEdge(FromPath: path, StoredLink: stored, ResolvedPath: Option<string>.None),
            });
        UnitFact units = new(Path: path, Units: archive.Settings.ModelUnitSystem);
        archive.Dispose();
        return Walk(
            pending: rest + edges.Choose(static edge => edge.ResolvedPath).Filter(link => !visited.Contains(key: link)),
            visited: visited,
            state: state with {
                Edges = state.Edges + edges,
                Broken = state.Broken + edges.Filter(static edge => edge.ResolvedPath.IsNone).Map(static edge => edge.StoredLink),
                Units = state.Units.Add(value: units),
                NativeLog = log.Length > 0 ? state.NativeLog.Add(value: log) : state.NativeLog,
            });
    }

    private static Seq<Seq<string>> CyclesOf(Seq<ClosureEdge> edges) {
        BidirectionalGraph<string, SEdge<string>> graph = new(allowParallelEdges: false);
        _ = graph.AddVertexRange(edges.Map(static edge => edge.FromPath).AsIterable());
        _ = edges.Choose(static edge => edge.ResolvedPath.Map(resolved => (edge.FromPath, Resolved: resolved)))
            .Iter(edge => graph.AddVerticesAndEdge(new SEdge<string>(edge.FromPath, edge.Resolved)));
        return CycleGroups.Of(graph: graph, comparer: StringComparer.Ordinal);
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]           | [FORM]                                                | [ENTRY]                          |
| :-----: | :------------------ | :---------------- | :----------------------------------------------------- | :--------------------------------- |
|  [01]   | topology source     | `GraphSource`     | live session, loaded archive, stored path               | `TopologyFold.Of`                   |
|  [02]   | folded graph        | `Topology`        | transient `BidirectionalGraph<Guid, SEdge<Guid>>`       | per-ask fold, never stored          |
|  [03]   | topology questions  | `BlockGraphAsk`   | one union, typed `BlockGraphAnswer` per case            | `BlockGraph.Ask`                    |
|  [04]   | dependency ordering | `BlockGraph`      | DAG pre-gate + `SourceFirstBidirectionalTopologicalSort`| `BlockGraphAsk.Order`               |
|  [05]   | cycle grouping      | `BlockGraph`      | `StronglyConnectedComponents` groups over one fold      | `BlockGraphAsk.Cycles`              |
|  [06]   | archive closure     | `ArchiveClosure`  | visited-set walk, SCC cycle groups, unit evidence       | `Closure(rootPath)`                 |

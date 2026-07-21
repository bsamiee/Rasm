# [RASM_RHINO_BLOCK_GRAPH]

Definition topology (`Rasm.Rhino.Blocks`) owns one graph-source union, one evidence-bearing fold, and one `BlockGraph.Ask` family. Edges orient used definition to container, placement facts retain instance identity and multiplicity, archive folds reconstruct nested edges from definition member ids, and algorithms refuse an archive whose linked definitions make the edge set opaque.

## [01]-[INDEX]

| [INDEX] | [OWNER] | [CONTRACT] |
| :-----: | :------ | :--------- |
|  [01]   | `GraphSource` · `Topology` | live and archive evidence admission |
|  [02]   | `BlockGraph.Ask` | topology, path, component, and usage questions |
|  [03]   | `ClosureReport` | linked-archive closure evidence |

## [02]-[SOURCE_AND_TOPOLOGY]

`GraphSource` admits a live session, a borrowed archive supplied by a caller, or a stored archive opened and closed inside the fold. `Topology` carries nodes, dependency-first edges, top-level placement facts, and `GraphEvidence`; its fold admits nonempty unique node keys and proves both endpoints before graph mutation. `OpaqueLinks` blocks every answer whose truth depends on missing linked-definition members.

Live placement evidence comes from `GetReferences(0)` and retains every instance id. Archive placement evidence correlates each `File3dmObject.Id` with definition `GetObjectIds()` membership; an instance reference outside every definition roster is top-level, while one inside a roster yields a nesting edge.

`GraphFold` canonicalizes vertices through an admitted order before grouping, component ranking, and condensed-edge ordering; equivalent graphs therefore emit identical component and edge sequences.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphSource {
    private GraphSource() { }
    public sealed record Live(DocumentSession Session) : GraphSource;
    public sealed record Loaded(File3dm Archive, Option<string> Path = default) : GraphSource;
    public sealed record Stored(string Path) : GraphSource;
}

[SmartEnum<int>]
public sealed partial class GraphEvidence {
    public static readonly GraphEvidence Complete = new(key: 0, isComplete: true);
    public static readonly GraphEvidence OpaqueLinks = new(key: 1, isComplete: false);

    public bool IsComplete { get; }
}

[SmartEnum<int>]
public sealed partial class GraphBoundary {
    public static readonly GraphBoundary Roots = new(
        key: 0,
        select: static graph => toSeq(graph.Roots()));
    public static readonly GraphBoundary Leaves = new(
        key: 1,
        select: static graph => toSeq(graph.Sinks()));

    public Func<BidirectionalGraph<Guid, SEdge<Guid>>, Seq<Guid>> Select { get; }
}

[SmartEnum<int>]
public sealed partial class GraphGrouping {
    public static readonly GraphGrouping Cycles = new(key: 0, select: static graph => GraphFold.Cycles(graph: graph));
    public static readonly GraphGrouping Components = new(key: 1, select: static graph => GraphFold.Components(graph: graph));

    public Func<BidirectionalGraph<Guid, SEdge<Guid>>, Seq<Seq<Guid>>> Select { get; }
}

[SmartEnum<int>]
public sealed partial class GraphProjection {
    public static readonly GraphProjection Closure = new(key: 0, project: static (graph, op) =>
        op.Catch(() => Fin.Succ(value: graph.ComputeTransitiveClosure(
            edgeFactory: static (source, target) => new SEdge<Guid>(source: source, target: target)))));
    public static readonly GraphProjection Reduction = new(key: 1, project: static (graph, op) =>
        GraphFold.Reduced(graph: graph, op: op));

    public Func<BidirectionalGraph<Guid, SEdge<Guid>>, Op, Fin<BidirectionalGraph<Guid, SEdge<Guid>>>> Project { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record DefinitionNode(
    Guid Key,
    Option<int> Index,
    string Name,
    Option<InstanceDefinitionUpdateType> UpdateType,
    Option<UnitSystem> ArchiveUnits,
    bool Opaque);

public sealed record PlacementNode(Guid InstanceId, Guid DefinitionId);

internal sealed record Topology(
    Seq<DefinitionNode> Nodes,
    Seq<(Guid Used, Guid Container)> Edges,
    Seq<PlacementNode> Placements,
    GraphEvidence Evidence) : IDetachedDocumentResult {
    internal Fin<BidirectionalGraph<Guid, SEdge<Guid>>> Fold(Op key) {
        Seq<Guid> nodes = Nodes.Map(static node => node.Key);
        return from _present in guard(!nodes.IsEmpty, key.InvalidResult()).ToFin()
               from _ in nodes
                   .Traverse(node => guard(node != Guid.Empty, key.InvalidResult()).ToFin().ToValidation())
                   .As()
                   .ToFin()
               from __ in guard(nodes.Distinct().Count == nodes.Count, key.InvalidResult()).ToFin()
               from ___ in Edges
                   .Traverse(edge => guard(
                       nodes.Contains(value: edge.Used) && nodes.Contains(value: edge.Container),
                       key.InvalidResult()).ToFin().ToValidation())
                   .As()
                   .ToFin()
               from graph in key.Catch(() => {
                   BidirectionalGraph<Guid, SEdge<Guid>> admitted = new(allowParallelEdges: false);
                   _ = admitted.AddVertexRange(nodes.AsIterable());
                   Edges.Iter(edge => admitted.AddEdge(new SEdge<Guid>(source: edge.Used, target: edge.Container)));
                   return Fin.Succ(value: admitted);
               })
               select graph;
    }
}
```

## [03]-[ASK_FAMILY]

`BlockGraphAsk` closes direct host queries, whole-topology projections, structural algorithms, and archive closure under one entry. `BlockGraphAnswer` preserves result meaning: definition keys never masquerade as placement ids, paths never masquerade as order, and placement answers carry completeness evidence instead of projecting opaque links as top-level instances.

`GraphFold` is the one vertex-generic `QuikGraph` fold surface — cycles, weak components, DAG-guarded order and reduction, and condensation — consumed by every graph projection in the assembly; a sibling rail re-deriving one of its folds is the deleted form. `GraphGrouping.Cycles` includes multi-vertex components and one-vertex components containing a self-edge, and reduction refuses a cyclic graph with the cycle detail. `GraphBoundary`, `GraphGrouping`, and `GraphProjection` carry paired algorithm choice as behavior rows instead of sibling request cases. `Containers`, `References`, `Nesting`, and `Tally` retain the host members that answer them directly and reject non-live sources.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockGraphAsk {
    private BlockGraphAsk() { }
    public sealed record Definitions : BlockGraphAsk;
    public sealed record Containers(ResourceRef Target) : BlockGraphAsk;
    public sealed record References(ResourceRef Target, ReferenceScope Scope) : BlockGraphAsk;
    public sealed record Nesting(ResourceRef Outer, ResourceRef Inner) : BlockGraphAsk;
    public sealed record Boundary(GraphBoundary Policy) : BlockGraphAsk;
    public sealed record Path(ResourceRef From, ResourceRef To) : BlockGraphAsk;
    public sealed record Order : BlockGraphAsk;
    public sealed record Groups(GraphGrouping Policy) : BlockGraphAsk;
    public sealed record Projection(GraphProjection Policy) : BlockGraphAsk;
    public sealed record Condensation : BlockGraphAsk;
    public sealed record Placed : BlockGraphAsk;
    public sealed record Tally(ResourceRef Target) : BlockGraphAsk;
    public sealed record Archives(ClosureBudget Budget) : BlockGraphAsk;
}

[ComplexValueObject]
public sealed partial class ClosureBudget {
    public int MaxArchives { get; }
    public long MaxLinks { get; }
    public int MaxDepth { get; }
    public long MaxBytes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int maxArchives,
        ref long maxLinks,
        ref int maxDepth,
        ref long maxBytes) =>
        validationError = maxArchives > 0 && maxLinks > 0 && maxDepth > 0 && maxBytes > 0
            ? null
            : new ValidationError(message: "Archive closure limits are positive.");

    public static Fin<ClosureBudget> Of(
        int maxArchives,
        long maxLinks,
        int maxDepth,
        long maxBytes,
        Op? key = null) =>
        Admission.Admitted(
            fault: Validate(maxArchives, maxLinks, maxDepth, maxBytes, out ClosureBudget? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockGraphAnswer : IDetachedDocumentResult {
    private BlockGraphAnswer() { }
    public sealed record Nodes(Seq<DefinitionNode> Values) : BlockGraphAnswer;
    public sealed record Definitions(Seq<Guid> Keys) : BlockGraphAnswer;
    public sealed record Placements(GraphEvidence Evidence, Seq<PlacementNode> Values) : BlockGraphAnswer;
    public sealed record Depth(int Levels) : BlockGraphAnswer;
    public sealed record Path(Seq<Guid> Keys) : BlockGraphAnswer;
    public sealed record Ordered(Seq<Guid> BakeOrder) : BlockGraphAnswer;
    public sealed record Groups(Seq<Seq<Guid>> Values) : BlockGraphAnswer;
    public sealed record Graph(Seq<(Guid Used, Guid Container)> Edges) : BlockGraphAnswer;
    public sealed record Condensed(Seq<Seq<Guid>> Components, Seq<(int From, int To)> Edges) : BlockGraphAnswer;
    public sealed record Usage(BlockUsage Counts) : BlockGraphAnswer;
    public sealed record Archives(ClosureReport Report) : BlockGraphAnswer;
}

// --- [SERVICES] ----------------------------------------------------------------------------
public static partial class BlockGraph {
    public static Fin<BlockGraphAnswer> Ask(GraphSource source, BlockGraphAsk question) {
        Op op = Op.Of();
        return from active in Optional(question).ToFin(Fail: op.InvalidInput())
               from answer in active.Switch(
                   context: (Source: source, Op: op),
                   definitions: static (ctx, _) =>
                       from topology in Of(source: ctx.Source, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Nodes(Values: topology.Nodes),
                   containers: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from target in Optional(ask.Target).ToFin(Fail: ctx.Op.InvalidInput())
                       from definition in Definitions.Resolve(target: target, document: document, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Definitions(
                           Keys: toSeq(definition.GetContainers()).Map(static container => container.Id))),
                   references: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from target in Optional(ask.Target).ToFin(Fail: ctx.Op.InvalidInput())
                       from definition in Definitions.Resolve(target: target, document: document, key: ctx.Op)
                       from scope in Optional(ask.Scope).ToFin(Fail: ctx.Op.InvalidInput())
                       select (BlockGraphAnswer)new BlockGraphAnswer.Placements(
                           Evidence: GraphEvidence.Complete,
                           Values: toSeq(definition.GetReferences(wheretoLook: scope.HostValue))
                               .Map(instance => new PlacementNode(InstanceId: instance.Id, DefinitionId: definition.Id)))),
                   nesting: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from outerTarget in Optional(ask.Outer).ToFin(Fail: ctx.Op.InvalidInput())
                       from innerTarget in Optional(ask.Inner).ToFin(Fail: ctx.Op.InvalidInput())
                       from outer in Definitions.Resolve(target: outerTarget, document: document, key: ctx.Op)
                       from inner in Definitions.Resolve(target: innerTarget, document: document, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Depth(
                           Levels: outer.UsesDefinition(otherIdefIndex: inner.Index))),
                   boundary: static (ctx, ask) =>
                       from policy in Optional(ask.Policy).ToFin(Fail: ctx.Op.InvalidInput())
                       from answer in Keys(source: ctx.Source, op: ctx.Op, project: policy.Select)
                       select answer,
                   path: static (ctx, ask) =>
                       from topology in Complete(source: ctx.Source, op: ctx.Op)
                       from start in KeyOf(topology: topology, target: ask.From, op: ctx.Op)
                       from finish in KeyOf(topology: topology, target: ask.To, op: ctx.Op)
                       from graph in topology.Fold(key: ctx.Op)
                       from path in ctx.Op.Catch(() => {
                           TryFunc<Guid, IEnumerable<SEdge<Guid>>> search = graph.TreeBreadthFirstSearch(start);
                           return search(finish, out IEnumerable<SEdge<Guid>> edges)
                               ? Fin.Succ(value: Seq(start).Concat(toSeq(edges).Map(static edge => edge.Target)))
                               : Fin.Fail<Seq<Guid>>(error: ctx.Op.MissingContext());
                       })
                       select (BlockGraphAnswer)new BlockGraphAnswer.Path(Keys: path),
                   order: static (ctx, _) =>
                       from topology in Complete(source: ctx.Source, op: ctx.Op)
                       from graph in topology.Fold(key: ctx.Op)
                       from ordered in GraphFold.Ordered(graph: graph, op: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Ordered(BakeOrder: ordered),
                   groups: static (ctx, ask) =>
                       from topology in Complete(source: ctx.Source, op: ctx.Op)
                       from policy in Optional(ask.Policy).ToFin(Fail: ctx.Op.InvalidInput())
                       from graph in topology.Fold(key: ctx.Op)
                       from groups in ctx.Op.Catch(() => Fin.Succ(value: policy.Select(arg: graph)))
                       select (BlockGraphAnswer)new BlockGraphAnswer.Groups(Values: groups),
                   projection: static (ctx, ask) =>
                       from policy in Optional(ask.Policy).ToFin(Fail: ctx.Op.InvalidInput())
                       from answer in Project(source: ctx.Source, op: ctx.Op, project: policy.Project)
                       select answer,
                   condensation: static (ctx, _) =>
                       from topology in Complete(source: ctx.Source, op: ctx.Op)
                       from graph in topology.Fold(key: ctx.Op)
                       from condensed in ctx.Op.Catch(() => Fin.Succ(value: GraphFold.Condensed(graph: graph)))
                       select (BlockGraphAnswer)new BlockGraphAnswer.Condensed(
                           Components: condensed.Components,
                           Edges: condensed.Edges),
                   placed: static (ctx, _) =>
                       from topology in Of(source: ctx.Source, key: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Placements(
                           Evidence: topology.Evidence,
                           Values: topology.Evidence.IsComplete ? topology.Placements : Seq<PlacementNode>()),
                   tally: static (ctx, ask) => Live(source: ctx.Source, op: ctx.Op, read: document =>
                       from target in Optional(ask.Target).ToFin(Fail: ctx.Op.InvalidInput())
                       from definition in Definitions.Resolve(target: target, document: document, key: ctx.Op)
                       from usage in ctx.Op.Catch(() => {
                           int total = definition.UseCount(
                               topLevelReferenceCount: out int topLevel,
                               nestedReferenceCount: out int nested);
                           return BlockUsage.Of(total: total, topLevel: topLevel, nested: nested, key: ctx.Op);
                       })
                       select (BlockGraphAnswer)new BlockGraphAnswer.Usage(Counts: usage)),
                   archives: static (ctx, ask) =>
                       from root in RootPath(source: ctx.Source, op: ctx.Op)
                       from budget in Optional(ask.Budget).ToFin(Fail: ctx.Op.InvalidInput())
                       from report in ArchiveClosure(rootPath: root, budget: budget, op: ctx.Op)
                       select (BlockGraphAnswer)new BlockGraphAnswer.Archives(Report: report))
               select answer;
    }

    private static Fin<BlockGraphAnswer> Keys(
        GraphSource source,
        Op op,
        Func<BidirectionalGraph<Guid, SEdge<Guid>>, Seq<Guid>> project) =>
        from topology in Complete(source: source, op: op)
        from graph in topology.Fold(key: op)
        from values in op.Catch(() => Fin.Succ(value: project(arg: graph)))
        select (BlockGraphAnswer)new BlockGraphAnswer.Definitions(Keys: values);

    private static Fin<BlockGraphAnswer> Project(
        GraphSource source,
        Op op,
        Func<BidirectionalGraph<Guid, SEdge<Guid>>, Op, Fin<BidirectionalGraph<Guid, SEdge<Guid>>>> project) =>
        from topology in Complete(source: source, op: op)
        from folded in topology.Fold(key: op)
        from graph in project(folded, op)
        select (BlockGraphAnswer)new BlockGraphAnswer.Graph(
            Edges: toSeq(graph.Edges).Map(static edge => (edge.Source, edge.Target)));

    private static Fin<Topology> Complete(GraphSource source, Op op) =>
        Of(source: source, key: op).Bind(topology => topology.Evidence.IsComplete
            ? Fin.Succ(value: topology)
            : Fin.Fail<Topology>(error: op.InvalidResult(detail: nameof(GraphEvidence.OpaqueLinks))));

    private static Fin<Guid> KeyOf(Topology topology, ResourceRef target, Op op) =>
        Optional(target).ToFin(Fail: op.InvalidInput()).Bind(active => active.Switch(
            context: (Topology: topology, Op: op),
            byId: static (ctx, value) => ctx.Topology.Nodes
                .Find(node => node.Key == value.Value)
                .Map(static node => node.Key)
                .ToFin(Fail: ctx.Op.MissingContext()),
            byName: static (ctx, value) => {
                Seq<DefinitionNode> matches = ctx.Topology.Nodes
                    .Filter(node => string.Equals(node.Name, value.Value, StringComparison.OrdinalIgnoreCase))
                    .Strict();
                return matches.Count switch {
                    0 => Fin.Fail<Guid>(error: ctx.Op.MissingContext()),
                    1 => matches.Head.Map(static node => node.Key).ToFin(Fail: ctx.Op.MissingContext()),
                    _ => Fin.Fail<Guid>(error: ctx.Op.InvalidResult(detail: $"ambiguous definition name: {value.Value}")),
                };
            },
            byIndex: static (ctx, value) => ctx.Topology.Nodes
                .Find(node => node.Index.Exists(index => index == value.Value))
                .Map(static node => node.Key)
                .ToFin(Fail: ctx.Op.MissingContext())));

    private static Fin<Topology> Of(GraphSource source, Op key) =>
        Optional(source).ToFin(Fail: key.InvalidInput()).Bind(request => request.Switch(
            context: key,
            live: static (op, held) => Optional(held.Session).ToFin(Fail: op.InvalidInput()).Bind(session => session.Demand(
                use: document => LiveTopology(document: document, op: op),
                key: op,
                needs: [SessionNeed.Read])),
            loaded: static (op, held) => Optional(held.Archive).ToFin(Fail: op.InvalidInput())
                .Bind(archive => Offline(archive: archive, op: op)),
            stored: static (op, held) =>
                from path in op.AcceptText(value: held.Path)
                from topology in op.Catch(() => Optional(File3dm.ReadWithLog(path: path, errorLog: out string log))
                    .ToFin(Fail: op.InvalidResult(detail: log))
                    .Bind(archive => {
                        using (archive) {
                            return Offline(archive: archive, op: op);
                        }
                    }))
                select topology));

    private static Fin<Topology> LiveTopology(RhinoDoc document, Op op) =>
        op.Catch(() => {
            Seq<InstanceDefinition> roster = toSeq(document.InstanceDefinitions.GetList(ignoreDeleted: true))
                .Choose(static definition => Optional(definition));
            Seq<DefinitionNode> nodes = roster.Map(static definition => new DefinitionNode(
                Key: definition.Id,
                Index: Some(definition.Index),
                Name: definition.Name,
                UpdateType: Some(definition.UpdateType),
                ArchiveUnits: Option<UnitSystem>.None,
                Opaque: false));
            Seq<(Guid Used, Guid Container)> edges = roster.Bind(definition =>
                toSeq(definition.GetContainers()).Map(container => (definition.Id, container.Id))).Distinct();
            Seq<PlacementNode> placements = roster.Bind(definition => toSeq(definition
                .GetReferences(wheretoLook: ReferenceScope.Direct.HostValue))
                .Map(instance => new PlacementNode(InstanceId: instance.Id, DefinitionId: definition.Id)));
            return Fin.Succ(value: new Topology(
                Nodes: nodes,
                Edges: edges,
                Placements: placements,
                Evidence: GraphEvidence.Complete));
        });

    private static Fin<Topology> Offline(File3dm archive, Op op) =>
        op.Catch(() => {
            UnitSystem units = archive.Settings.ModelUnitSystem;
            Seq<InstanceDefinitionGeometry> roster = toSeq(archive.AllInstanceDefinitions);
            Seq<(Guid Definition, LanguageExt.HashSet<Guid> Members)> membership = roster.Map(definition => (
                definition.Id,
                toSeq(definition.GetObjectIds()).ToHashSet()));
            LanguageExt.HashSet<Guid> memberIds = membership
                .Bind(static row => row.Members)
                .ToHashSet();
            Seq<(Guid Object, Guid Definition)> references = toSeq(archive.Objects)
                .Choose(static entry => Optional(entry.Geometry as InstanceReferenceGeometry)
                    .Map(reference => (Object: entry.Id, Definition: reference.ParentIdefId)))
                .Filter(static row => row.Definition != Guid.Empty);
            Seq<(Guid Used, Guid Container)> edges = membership.Bind(row => references
                .Filter(reference => row.Members.Contains(key: reference.Object))
                .Map(reference => (reference.Definition, row.Definition)))
                .Distinct();
            Seq<PlacementNode> placements = references
                .Filter(reference => !memberIds.Contains(key: reference.Object))
                .Map(static reference => new PlacementNode(
                    InstanceId: reference.Object,
                    DefinitionId: reference.Definition));
            Seq<DefinitionNode> nodes = roster.Map(definition => {
                bool opaque = !string.IsNullOrWhiteSpace(definition.SourceArchive) && definition.GetObjectIds().Length == 0;
                return new DefinitionNode(
                    Key: definition.Id,
                    Index: Option<int>.None,
                    Name: definition.Name,
                    UpdateType: Option<InstanceDefinitionUpdateType>.None,
                    ArchiveUnits: Some(units),
                    Opaque: opaque);
            });
            return Fin.Succ(value: new Topology(
                Nodes: nodes,
                Edges: edges,
                Placements: placements,
                Evidence: nodes.Exists(static node => node.Opaque)
                    ? GraphEvidence.OpaqueLinks
                    : GraphEvidence.Complete));
        });

    private static Fin<BlockGraphAnswer> Live(
        GraphSource source,
        Op op,
        Func<RhinoDoc, Fin<BlockGraphAnswer>> read) =>
        source is GraphSource.Live live
            ? Optional(live.Session).ToFin(Fail: op.InvalidInput())
                .Bind(session => session.Demand(use: read, key: op, needs: [SessionNeed.Read]))
            : Fin.Fail<BlockGraphAnswer>(error: op.Unsupported(
                geometryType: typeof(GraphSource),
                outputType: typeof(BlockGraphAnswer)));

    private static Fin<string> RootPath(GraphSource source, Op op) => source switch {
        GraphSource.Stored stored => op.AcceptText(value: stored.Path),
        GraphSource.Loaded loaded => loaded.Path.ToFin(Fail: op.InvalidInput()).Bind(op.AcceptText),
        _ => Fin.Fail<string>(error: op.Unsupported(
            geometryType: typeof(GraphSource),
            outputType: typeof(ClosureReport))),
    };
}

internal static class GraphFold {
    internal static Seq<Seq<TVertex>> Cycles<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        IEqualityComparer<TVertex>? comparer = null,
        IComparer<TVertex>? order = null) where TVertex : notnull {
        Dictionary<TVertex, int> labels = comparer is null ? new() : new(comparer);
        _ = graph.StronglyConnectedComponents(components: labels);
        return Grouped(graph: graph, labels: labels, order: order).Filter(members =>
            members.Count > 1 || members.Head.Exists(vertex => graph.ContainsEdge(source: vertex, target: vertex)));
    }

    internal static Seq<Seq<TVertex>> Components<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        IComparer<TVertex>? order = null)
        where TVertex : notnull {
        Dictionary<TVertex, int> labels = new();
        _ = graph.WeaklyConnectedComponents(components: labels);
        return Grouped(graph: graph, labels: labels, order: order);
    }

    internal static Fin<Seq<TVertex>> Ordered<TVertex>(BidirectionalGraph<TVertex, SEdge<TVertex>> graph, Op op)
        where TVertex : notnull =>
        op.Catch(() => graph.IsDirectedAcyclicGraph()
            ? Fin.Succ(value: toSeq(graph.SourceFirstBidirectionalTopologicalSort()))
            : Fin.Fail<Seq<TVertex>>(error: op.InvalidResult(detail: nameof(Cycles))));

    internal static Fin<BidirectionalGraph<TVertex, SEdge<TVertex>>> Reduced<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        Op op) where TVertex : notnull =>
        op.Catch(() => graph.IsDirectedAcyclicGraph()
            ? Fin.Succ(value: graph.ComputeTransitiveReduction())
            : Fin.Fail<BidirectionalGraph<TVertex, SEdge<TVertex>>>(error: op.InvalidResult(detail: nameof(Cycles))));

    internal static (Seq<Seq<TVertex>> Components, Seq<(int From, int To)> Edges) Condensed<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        IComparer<TVertex>? order = null) where TVertex : notnull {
        Dictionary<TVertex, int> labels = new();
        _ = graph.StronglyConnectedComponents(components: labels);
        Seq<TVertex> vertices = OrderedVertices(graph: graph, order: order);
        Seq<int> components = vertices.Map(vertex => labels[vertex]).Distinct();
        Dictionary<int, int> ranks = components
            .Map(static (label, index) => (Label: label, Index: index))
            .ToDictionary(static pair => pair.Label, static pair => pair.Index);
        Seq<Seq<TVertex>> members = components.Map(component =>
            vertices.Filter(vertex => labels[vertex] == component));
        Seq<(int From, int To)> edges = toSeq(graph.Edges
            .Select(edge => (From: ranks[labels[edge.Source]], To: ranks[labels[edge.Target]]))
            .Where(static edge => edge.From != edge.To)
            .Distinct()
            .OrderBy(static edge => edge.From)
            .ThenBy(static edge => edge.To));
        return (Components: members, Edges: edges);
    }

    private static Seq<Seq<TVertex>> Grouped<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        Dictionary<TVertex, int> labels,
        IComparer<TVertex>? order) where TVertex : notnull {
        Seq<TVertex> vertices = OrderedVertices(graph: graph, order: order);
        return vertices
            .Map(vertex => labels[vertex])
            .Distinct()
            .Map(component => vertices.Filter(vertex => labels[vertex] == component));
    }

    private static Seq<TVertex> OrderedVertices<TVertex>(
        BidirectionalGraph<TVertex, SEdge<TVertex>> graph,
        IComparer<TVertex>? order) where TVertex : notnull =>
        toSeq(graph.Vertices.OrderBy(
            keySelector: static vertex => vertex,
            comparer: order ?? Comparer<TVertex>.Default));
}
```

## [04]-[ARCHIVE_CLOSURE]

`ClosureReport` is the bounded linked-archive walk: raw and resolved edges, source-aware broken links, self-inclusive cycle groups, unit facts, native read logs, resource usage, and typed completion evidence. `BlockGraphAsk.Archives` admits a closure budget plus only a stored path or a loaded archive carrying its path; each stored link resolves against its referencing archive directory.

Queue traversal interns symlink-resolved paths, pins the canonical root directory, and opens each dependency segment relative to that handle without following links. Handle length gates bytes before an exact owned snapshot reaches the native reader; link count and depth gate expansion. Each rejected native read preserves its log beside the broken-link detail without aborting independent traversal; SCC analysis distinguishes shared dependencies from circular links after the bounded walk settles.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record ClosureEdge(string FromPath, string StoredLink, Option<string> ResolvedPath);

public sealed record BrokenLink(
    string FromPath,
    string StoredLink,
    Option<string> ResolvedPath,
    string Detail);

public sealed record UnitFact(string Path, UnitSystem Units);

[SmartEnum<string>]
public sealed partial class ClosureLimit {
    public static readonly ClosureLimit Archives = new(key: nameof(Archives));
    public static readonly ClosureLimit Links = new(key: nameof(Links));
    public static readonly ClosureLimit Depth = new(key: nameof(Depth));
    public static readonly ClosureLimit Bytes = new(key: nameof(Bytes));
}

[Union]
public abstract partial record ClosureTerminal {
    private ClosureTerminal() { }
    public sealed record Complete : ClosureTerminal;
    public sealed record Exhausted(ClosureLimit Limit, long Observed, long Allowed, string Path) : ClosureTerminal;
}

public sealed record ClosureUsage(int Archives, long Links, int Depth, long Bytes);

public sealed record ClosureReport(
    Seq<ClosureEdge> Edges,
    Seq<BrokenLink> Broken,
    Seq<Seq<string>> Cycles,
    Seq<UnitFact> Units,
    Seq<string> NativeLog,
    ClosureUsage Usage,
    ClosureTerminal Terminal) : IDetachedDocumentResult {
    public bool Sound => Terminal is ClosureTerminal.Complete && Broken.IsEmpty && Cycles.IsEmpty;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class BlockGraph {
    private sealed record ArchiveTarget(string FromPath, string StoredLink, string Path, int Depth);

    [Union]
    private abstract partial record ArchiveScan {
        private ArchiveScan() { }
        internal sealed record Read(UnitSystem Units, Seq<string> Links, string NativeLog) : ArchiveScan;
        internal sealed record Rejected(Error Error, string NativeLog) : ArchiveScan;
    }

    private sealed class ArchiveRoot : IDisposable {
        private readonly Microsoft.Win32.SafeHandles.SafeFileHandle directory;
        private readonly string path;

        private ArchiveRoot(Microsoft.Win32.SafeHandles.SafeFileHandle directory, string path) =>
            (this.directory, this.path) = (directory, path);

        internal static Fin<ArchiveRoot> Open(string path, Op op) => op.Catch(() => {
            string full = System.IO.Path.GetFullPath(path: path);
            string prefix = System.IO.Path.GetPathRoot(path: full)
                ?? throw new InvalidOperationException(message: op.InvalidInput().Message);
            string[] segments = full[prefix.Length..].Split(
                separator: [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                options: StringSplitOptions.RemoveEmptyEntries);
            Microsoft.Win32.SafeHandles.SafeFileHandle opened = OpenDirectory(
                path: prefix,
                flags: OpenRead | OpenDirectoryOnly | OpenNoFollow | OpenCloseOnExec);
            Microsoft.Win32.SafeHandles.SafeFileHandle? owned = opened;
            try {
                if (opened.IsInvalid) {
                    int code = System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
                    return Fin.Fail<ArchiveRoot>(error: op.InvalidResult(detail: $"open-root:{code}"));
                }
                foreach (string segment in segments) {
                    Microsoft.Win32.SafeHandles.SafeFileHandle current = owned
                        ?? throw new InvalidOperationException(message: op.InvalidResult().Message);
                    Microsoft.Win32.SafeHandles.SafeFileHandle next = OpenRelative(
                        directory: current,
                        path: segment,
                        flags: OpenRead | OpenDirectoryOnly | OpenNoFollow | OpenCloseOnExec);
                    if (next.IsInvalid) {
                        int code = System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
                        next.Dispose();
                        return Fin.Fail<ArchiveRoot>(error: op.InvalidResult(detail: $"open-root-segment:{code}"));
                    }
                    current.Dispose();
                    owned = next;
                }
                Microsoft.Win32.SafeHandles.SafeFileHandle admitted = owned
                    ?? throw new InvalidOperationException(message: op.InvalidResult().Message);
                owned = null;
                return Fin.Succ(value: new ArchiveRoot(directory: admitted, path: full));
            }
            finally {
                owned?.Dispose();
            }
        });

        internal Fin<ArchiveInput> Open(string candidate, Op op) => op.Catch(() => {
            string relative = System.IO.Path.GetRelativePath(relativeTo: path, path: candidate);
            string[] segments = relative.Split(
                separator: [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                options: StringSplitOptions.RemoveEmptyEntries);
            if (System.IO.Path.IsPathRooted(path: relative)
                || segments.Length is 0
                || segments.Any(static segment => segment is "." or "..")) {
                return Fin.Fail<ArchiveInput>(error: op.InvalidContext());
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle? owned = null;
            try {
                for (int index = 0; index < segments.Length; index++) {
                    bool leaf = index == segments.Length - 1;
                    Microsoft.Win32.SafeHandles.SafeFileHandle next = OpenRelative(
                        directory: owned ?? directory,
                        path: segments[index],
                        flags: OpenRead | OpenNoFollow | OpenCloseOnExec | (leaf ? 0 : OpenDirectoryOnly));
                    if (next.IsInvalid) {
                        next.Dispose();
                        return Fin.Fail<ArchiveInput>(error: op.InvalidResult(
                            detail: $"open-segment:{System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}"));
                    }
                    owned?.Dispose();
                    owned = next;
                }

                if (owned is null) {
                    return Fin.Fail<ArchiveInput>(error: op.InvalidContext());
                }
                Microsoft.Win32.SafeHandles.SafeFileHandle admitted = owned;
                owned = null;
                long length = System.IO.RandomAccess.GetLength(handle: admitted);
                if (length < 0) {
                    admitted.Dispose();
                    return Fin.Fail<ArchiveInput>(error: op.InvalidResult());
                }
                return Fin.Succ(value: new ArchiveInput(handle: admitted, length: length));
            }
            finally {
                owned?.Dispose();
            }
        });

        public void Dispose() => directory.Dispose();
    }

    private sealed class ArchiveInput : IDisposable {
        internal ArchiveInput(Microsoft.Win32.SafeHandles.SafeFileHandle handle, long length) =>
            (Handle, Length) = (handle, length);

        internal Microsoft.Win32.SafeHandles.SafeFileHandle Handle { get; }
        internal long Length { get; }

        public void Dispose() => Handle.Dispose();
    }

    private const int OpenRead = 0x00000000;
    private const int OpenNoFollow = 0x00000100;
    private const int OpenDirectoryOnly = 0x00100000;
    private const int OpenCloseOnExec = 0x01000000;

    [System.Runtime.InteropServices.LibraryImport(
        "libc",
        EntryPoint = "open",
        SetLastError = true,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    private static partial Microsoft.Win32.SafeHandles.SafeFileHandle OpenDirectory(string path, int flags);

    [System.Runtime.InteropServices.LibraryImport(
        "libc",
        EntryPoint = "openat",
        SetLastError = true,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    private static partial Microsoft.Win32.SafeHandles.SafeFileHandle OpenRelative(
        Microsoft.Win32.SafeHandles.SafeFileHandle directory,
        string path,
        int flags);

    private static Fin<ClosureReport> ArchiveClosure(string rootPath, ClosureBudget budget, Op op) => op.Catch(() => {
            StringComparer pathComparer = OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
            StringComparison pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            Queue<ArchiveTarget> pending = new();
            System.Collections.Generic.HashSet<string> visited = new(pathComparer);
            System.Collections.Generic.Dictionary<string, string> identities = new(pathComparer);
            System.Collections.Generic.Dictionary<string, string> failed = new(pathComparer);
            Seq<ClosureEdge> edges = Seq<ClosureEdge>();
            Seq<BrokenLink> broken = Seq<BrokenLink>();
            Seq<UnitFact> units = Seq<UnitFact>();
            Seq<string> logs = Seq<string>();
            int archives = 0;
            long links = 0;
            int depth = 0;
            long bytes = 0;
            ClosureTerminal terminal = new ClosureTerminal.Complete();
            string Canonical(string path) {
                string full = System.IO.Path.TrimEndingDirectorySeparator(
                    path: System.IO.Path.GetFullPath(path: path));
                string prefix = System.IO.Path.GetPathRoot(path: full)
                    ?? throw new InvalidOperationException(message: op.InvalidInput().Message);
                string resolved = prefix;
                foreach (string segment in full[prefix.Length..].Split(
                    separator: [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                    options: StringSplitOptions.RemoveEmptyEntries)) {
                    string candidate = System.IO.Path.Combine(path1: resolved, path2: segment);
                    System.IO.FileSystemInfo identity = System.IO.Directory.Exists(path: candidate)
                        ? new System.IO.DirectoryInfo(path: candidate)
                        : new System.IO.FileInfo(fileName: candidate);
                    resolved = identity.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? candidate;
                }
                resolved = System.IO.Path.TrimEndingDirectorySeparator(
                    path: System.IO.Path.GetFullPath(path: resolved));
                if (identities.TryGetValue(key: resolved, value: out string? known)) {
                    return known;
                }
                identities.Add(key: resolved, value: resolved);
                return resolved;
            }

            bool Within(string root, string candidate) {
                string relative = System.IO.Path.GetRelativePath(relativeTo: root, path: candidate);
                return !System.IO.Path.IsPathRooted(path: relative)
                    && !string.Equals(a: relative, b: "..", comparisonType: pathComparison)
                    && !relative.StartsWith(value: $"..{System.IO.Path.DirectorySeparatorChar}", comparisonType: pathComparison)
                    && !relative.StartsWith(value: $"..{System.IO.Path.AltDirectorySeparatorChar}", comparisonType: pathComparison);
            }

            string root = Canonical(path: rootPath);
            string rootDirectory = System.IO.Path.GetDirectoryName(path: root)
                ?? throw new InvalidOperationException(message: op.InvalidInput().Message);
            return ArchiveRoot.Open(path: rootDirectory, op: op).Bind(rootHandle => {
                using (rootHandle) {
                    pending.Enqueue(item: new ArchiveTarget(FromPath: root, StoredLink: rootPath, Path: root, Depth: 0));

            while (terminal is ClosureTerminal.Complete && pending.TryDequeue(result: out ArchiveTarget? target)) {
                if (!visited.Add(item: target.Path)) {
                    if (failed.TryGetValue(key: target.Path, value: out string? prior)) {
                        broken = broken.Add(value: new BrokenLink(
                            FromPath: target.FromPath,
                            StoredLink: target.StoredLink,
                            ResolvedPath: Some(target.Path),
                            Detail: prior));
                    }
                    continue;
                }

                if (archives >= budget.MaxArchives) {
                    terminal = new ClosureTerminal.Exhausted(
                        Limit: ClosureLimit.Archives,
                        Observed: checked((long)archives + 1),
                        Allowed: budget.MaxArchives,
                        Path: target.Path);
                    break;
                }

                ArchiveInput? input = null;
                rootHandle.Open(candidate: target.Path, op: op).Match(
                    Succ: opened => { input = opened; return unit; },
                    Fail: error => {
                        failed[target.Path] = error.Message;
                        broken = broken.Add(value: new BrokenLink(
                            FromPath: target.FromPath,
                            StoredLink: target.StoredLink,
                            ResolvedPath: Some(target.Path),
                            Detail: error.Message));
                        return unit;
                    });
                if (input is null) {
                    continue;
                }
                using (input) {
                    long archiveBytes = input.Length;
                    if (archiveBytes > budget.MaxBytes - bytes) {
                        terminal = new ClosureTerminal.Exhausted(
                            Limit: ClosureLimit.Bytes,
                            Observed: archiveBytes > long.MaxValue - bytes ? long.MaxValue : bytes + archiveBytes,
                            Allowed: budget.MaxBytes,
                            Path: target.Path);
                        break;
                    }
                    archives = checked(archives + 1);
                    bytes = checked(bytes + archiveBytes);
                    depth = Math.Max(val1: depth, val2: target.Depth);

                    InspectArchive(input: input, op: op).Match(
                    Succ: scan => scan.Switch(
                        read: read => {
                            units = units.Add(value: new UnitFact(Path: target.Path, Units: read.Units));
                            logs = string.IsNullOrWhiteSpace(value: read.NativeLog)
                                ? logs
                                : logs.Add(value: read.NativeLog);
                            long observedLinks = read.Links.Count > long.MaxValue - links
                                ? long.MaxValue
                                : links + read.Links.Count;
                            if (observedLinks > budget.MaxLinks) {
                                terminal = new ClosureTerminal.Exhausted(
                                    Limit: ClosureLimit.Links,
                                    Observed: observedLinks,
                                    Allowed: budget.MaxLinks,
                                    Path: target.Path);
                                return unit;
                            }
                            links = observedLinks;
                            string anchor = System.IO.Path.GetDirectoryName(path: target.Path) ?? string.Empty;
                            foreach (string link in read.Links) {
                                if (terminal is not ClosureTerminal.Complete) {
                                    break;
                                }
                                op.Catch(() => Fin.Succ(value: Canonical(path:
                                    System.IO.Path.IsPathRooted(path: link)
                                        ? link
                                        : System.IO.Path.Combine(path1: anchor, path2: link)))).Match(
                                    Succ: resolved => {
                                        if (!Within(root: rootDirectory, candidate: resolved)) {
                                            edges = edges.Add(value: new ClosureEdge(
                                                FromPath: target.Path,
                                                StoredLink: link,
                                                ResolvedPath: Option<string>.None));
                                            broken = broken.Add(value: new BrokenLink(
                                                FromPath: target.Path,
                                                StoredLink: link,
                                                ResolvedPath: Some(resolved),
                                                Detail: op.InvalidContext().Message));
                                            return unit;
                                        }
                                        edges = edges.Add(value: new ClosureEdge(
                                            FromPath: target.Path,
                                            StoredLink: link,
                                            ResolvedPath: Some(resolved)));
                                        int nextDepth = checked(target.Depth + 1);
                                        if (nextDepth > budget.MaxDepth) {
                                            terminal = new ClosureTerminal.Exhausted(
                                                Limit: ClosureLimit.Depth,
                                                Observed: nextDepth,
                                                Allowed: budget.MaxDepth,
                                                Path: resolved);
                                            return unit;
                                        }
                                        pending.Enqueue(item: new ArchiveTarget(
                                            FromPath: target.Path,
                                            StoredLink: link,
                                            Path: resolved,
                                            Depth: nextDepth));
                                        return unit;
                                    },
                                    Fail: error => {
                                        edges = edges.Add(value: new ClosureEdge(
                                            FromPath: target.Path,
                                            StoredLink: link,
                                            ResolvedPath: Option<string>.None));
                                        broken = broken.Add(value: new BrokenLink(
                                            FromPath: target.Path,
                                            StoredLink: link,
                                            ResolvedPath: Option<string>.None,
                                            Detail: error.Message));
                                        return unit;
                                    });
                            }
                            return unit;
                        },
                        rejected: rejected => {
                            logs = string.IsNullOrWhiteSpace(value: rejected.NativeLog)
                                ? logs
                                : logs.Add(value: rejected.NativeLog);
                            failed[target.Path] = rejected.Error.Message;
                            broken = broken.Add(value: new BrokenLink(
                                FromPath: target.FromPath,
                                StoredLink: target.StoredLink,
                                ResolvedPath: Some(target.Path),
                                Detail: rejected.Error.Message));
                            return unit;
                        }),
                    Fail: error => {
                        failed[target.Path] = error.Message;
                        broken = broken.Add(value: new BrokenLink(
                            FromPath: target.FromPath,
                            StoredLink: target.StoredLink,
                            ResolvedPath: Some(target.Path),
                            Detail: error.Message));
                        return unit;
                        });
                }
            }

            BidirectionalGraph<string, SEdge<string>> graph = new(allowParallelEdges: false);
            edges.Iter(edge => edge.ResolvedPath.Iter(target => graph.AddVerticesAndEdge(
                edge: new SEdge<string>(source: edge.FromPath, target: target))));
                    return Fin.Succ(value: new ClosureReport(
                        Edges: edges,
                        Broken: broken,
                        Cycles: GraphFold.Cycles(graph: graph, comparer: pathComparer, order: pathComparer),
                        Units: units,
                        NativeLog: logs,
                        Usage: new ClosureUsage(Archives: archives, Links: links, Depth: depth, Bytes: bytes),
                        Terminal: terminal));
                }
            });
        });

    private static Fin<ArchiveScan> InspectArchive(ArchiveInput input, Op op) => op.Catch(() => {
        string snapshot = System.IO.Path.Combine(
            path1: System.IO.Path.GetTempPath(),
            path2: $"{Guid.NewGuid():N}.3dm");
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(minimumLength: 1);
        try {
            using (Microsoft.Win32.SafeHandles.SafeFileHandle output = System.IO.File.OpenHandle(
                path: snapshot,
                mode: System.IO.FileMode.CreateNew,
                access: System.IO.FileAccess.Write,
                share: System.IO.FileShare.None,
                options: System.IO.FileOptions.RandomAccess)) {
                long offset = 0;
                while (offset < input.Length) {
                    int count = checked((int)Math.Min(val1: buffer.Length, val2: input.Length - offset));
                    int read = System.IO.RandomAccess.Read(
                        handle: input.Handle,
                        buffer: buffer.AsSpan(start: 0, length: count),
                        fileOffset: offset);
                    if (read <= 0) {
                        return Fin.Succ<ArchiveScan>(value: new ArchiveScan.Rejected(
                            Error: op.InvalidResult(detail: "archive snapshot short read"),
                            NativeLog: string.Empty));
                    }
                    System.IO.RandomAccess.Write(
                        handle: output,
                        buffer: buffer.AsSpan(start: 0, length: read),
                        fileOffset: offset);
                    offset = checked(offset + read);
                }
                if (System.IO.RandomAccess.GetLength(handle: input.Handle) != input.Length) {
                    return Fin.Succ<ArchiveScan>(value: new ArchiveScan.Rejected(
                        Error: op.InvalidResult(detail: "archive length changed during snapshot"),
                        NativeLog: string.Empty));
                }
            }

            using File3dm? archive = File3dm.ReadWithLog(path: snapshot, errorLog: out string log);
            return archive is null
                ? Fin.Succ<ArchiveScan>(value: new ArchiveScan.Rejected(
                    Error: op.InvalidResult(detail: log),
                    NativeLog: log))
                : Fin.Succ<ArchiveScan>(value: new ArchiveScan.Read(
                    Units: archive.Settings.ModelUnitSystem,
                    Links: toSeq(archive.AllInstanceDefinitions)
                        .Choose(static definition => Optional(definition.SourceArchive)
                            .Filter(static source => !string.IsNullOrWhiteSpace(value: source))),
                    NativeLog: log));
        }
        finally {
            System.Buffers.ArrayPool<byte>.Shared.Return(array: buffer, clearArray: true);
            System.IO.File.Delete(path: snapshot);
        }
    });
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER] | [INGRESS] | [ALGORITHM] | [EGRESS] |
| :-----: | :------ | :-------- | :---------- | :------- |
|  [01]   | `BlockGraph` | `Ask` | `QuikGraph` · host reads · archive fold | `BlockGraphAnswer` |
|  [02]   | `GraphFold` | transient graph | SCC · components · order · reduction | graph evidence |
|  [03]   | `ClosureReport` | `Archives(ClosureBudget)` | bounded breadth-first archive reads | closure evidence |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

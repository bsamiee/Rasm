# [RASM_RHINO_BLOCK_GRAPH]

Definition topology (`Rasm.Rhino.Blocks`) owns one graph-source union, one evidence-bearing fold, and one `BlockGraph.Ask` family. Edges orient used definition to container, placement facts retain instance identity and multiplicity, archive folds reconstruct nested edges from definition member ids, and algorithms refuse an archive whose linked definitions make the edge set opaque.

## [01]-[INDEX]

- [02]-[SOURCE_AND_TOPOLOGY]: `GraphSource` admitting a live session, a borrowed archive, or a stored archive opened inside the fold, `Topology` carrying nodes, dependency-first edges, and placement evidence, and `GraphFold` canonicalizing vertices so equivalent graphs emit identical sequences.
- [03]-[ASK_FAMILY]: `BlockGraphAsk` closing host queries, whole-topology projections, and structural algorithms under one entry over the one vertex-generic `QuikGraph` fold, `BlockGraphAnswer` preserving result meaning against key, path, and completeness confusion.
- [04]-[ARCHIVE_CLOSURE]: `ClosureReport` the bounded linked-archive walk — raw and resolved edges, source-aware broken links, self-inclusive cycle groups, unit facts, native read logs, and typed completion evidence under a closure budget, `ArchivePlane` stating the POSIX-only traversal seam once and refusing every unservable platform at selection, and `ClosureWalk` carrying the whole traversal as one state-threaded value the budget-bounded fold advances.
- [05]-[SURFACE_LEDGER]: owner-to-ingress-to-algorithm-to-egress roster across `BlockGraph`, `GraphFold`, and `ClosureReport`.

## [02]-[SOURCE_AND_TOPOLOGY]

`GraphSource` admits a live session, a borrowed archive supplied by a caller, or a stored archive opened and closed inside the fold. `Topology` carries nodes, dependency-first edges, top-level placement facts, and `GraphEvidence`; its fold admits nonempty unique node keys and proves both endpoints before graph mutation. `OpaqueLinks` blocks every answer whose truth depends on missing linked-definition members.

Live placement evidence comes from `GetReferences(0)` and retains every instance id. Archive placement evidence correlates each `File3dmObject.Id` with definition `GetObjectIds()` membership; an instance reference outside every definition roster is top-level, while one inside a roster yields a nesting edge.

`GetReferences` is thread-affine, and `DocumentSession.Demand` is the rail that satisfies it: the demand resolves on the host command thread — a live session marshals through `RhinoApp.InvokeAndWait`, a headless one stays on the caller — so every live read on this page sits inside a demand window and no arm re-derives affinity. Composing a thread-affine host member outside a demand is the deleted form.

`GraphFold` canonicalizes vertices through an admitted order before grouping, component ranking, and condensed-edge ordering; equivalent graphs therefore emit identical component and edge sequences.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

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

`ArchivePlane` is the platform seam stated once. Relative-to-a-pinned-handle opening is a POSIX capability with no Win32 peer — `openat` has no `CreateFileW` equivalent, and the segment walk is exactly what defeats a symlink race — so the plane vocabulary carries one row per servable ABI with its own `open` flag columns and its own path-comparison policy, and `ArchivePlane.Current` REFUSES typed on any other platform instead of binding a `libc` entry point that platform does not export. Every downstream comparison reads the selected row's column, so no traversal step re-asks which platform it is on.

The walk is a state-threaded fold, not a loop: `ClosureWalk` carries the frontier, the seen set, every accumulator, and the terminal as ONE value, `ClosureWalk.Step` advances exactly one frontier entry, and the drive is a bounded fixpoint whose tick count derives from `ClosureBudget.MaxArchives` — a settled walk re-emits itself, so the fold is total. Deduplication happens at ENQUEUE, so every frontier entry is new and the archive budget is the exact step bound; a link resolving onto an already-failed archive receipts its broken re-reference there rather than re-entering the frontier.

The walk pins the canonical root directory and opens each dependency segment relative to that handle without following links. Handle length gates bytes before an exact owned snapshot reaches the native reader; link count and depth gate expansion. The snapshot copies through a pool lease sized from a declared chunk policy clamped to the archive's own extent, never a one-byte rent whose smallest bucket turns a real `.3dm` into millions of syscall pairs. Each rejected native read preserves its log beside the broken-link detail without aborting independent traversal; SCC analysis distinguishes shared dependencies from circular links after the bounded walk settles.

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

[SmartEnum<string>]
public sealed partial class ArchivePlane {
    // Flag columns are the platform's own `<fcntl.h>` ABI bits, declared per servable RID at this seam: they are
    // compile-time kernel ordinals no managed surface publishes, and they differ per plane, which is the whole
    // reason the plane owns them instead of a shared constant block.
    public static readonly ArchivePlane Darwin = new(
        key: "darwin",
        read: 0x00000000,
        noFollow: 0x00000100,
        directoryOnly: 0x00100000,
        closeOnExec: 0x01000000,
        comparer: StringComparer.Ordinal);
    public static readonly ArchivePlane Linux = new(
        key: "linux",
        read: 0x00000000,
        noFollow: 0x00020000,
        directoryOnly: 0x00010000,
        closeOnExec: 0x00080000,
        comparer: StringComparer.Ordinal);

    public int Read { get; }
    public int NoFollow { get; }
    public int DirectoryOnly { get; }
    public int CloseOnExec { get; }
    public StringComparer Comparer { get; }

    internal int Walk => Read | NoFollow | DirectoryOnly | CloseOnExec;

    internal int Leaf => Read | NoFollow | CloseOnExec;

    internal StringComparison Comparison => StringComparison.Ordinal;

    // A plane with no `openat` peer cannot serve a symlink-safe segment walk, so selection refuses rather
    // than binding a `libc` entry point the platform does not export.
    internal static Fin<ArchivePlane> Current(Op op) =>
        OperatingSystem.IsMacOS() ? Fin.Succ(value: Darwin)
        : OperatingSystem.IsLinux() ? Fin.Succ(value: Linux)
        : Fin.Fail<ArchivePlane>(error: op.Unsupported(
            geometryType: typeof(ArchiveRoot),
            outputType: typeof(ClosureReport)));
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
        private readonly ArchivePlane plane;
        private readonly string path;

        private ArchiveRoot(Microsoft.Win32.SafeHandles.SafeFileHandle directory, ArchivePlane plane, string path) =>
            (this.directory, this.plane, this.path) = (directory, plane, path);

        internal static Fin<ArchiveRoot> Open(string path, ArchivePlane plane, Op op) => op.Catch(() => {
            string full = System.IO.Path.GetFullPath(path: path);
            string prefix = System.IO.Path.GetPathRoot(path: full)
                ?? throw new InvalidOperationException(message: op.InvalidInput().Message);
            string[] segments = full[prefix.Length..].Split(
                separator: [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                options: StringSplitOptions.RemoveEmptyEntries);
            Microsoft.Win32.SafeHandles.SafeFileHandle opened = OpenDirectory(path: prefix, flags: plane.Walk);
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
                        flags: plane.Walk);
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
                return Fin.Succ(value: new ArchiveRoot(directory: admitted, plane: plane, path: full));
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
                        flags: leaf ? plane.Leaf : plane.Walk);
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

    // Exemption: the two `[LibraryImport]` declarations are the platform-forced POSIX seam `ArchivePlane` gates —
    // no managed surface offers a relative-to-handle open, and no arm reaches them before `Current` admits a plane.
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

    private const int SnapshotChunkBytes = 1 << 20;

    private sealed record ClosureScope(
        ArchiveRoot Root,
        string Anchor,
        ArchivePlane Plane,
        ClosureBudget Budget,
        Op Op);

    private sealed record ClosureWalk(
        Seq<ArchiveTarget> Pending,
        LanguageExt.HashSet<string> Seen,
        HashMap<string, string> Failed,
        Seq<ClosureEdge> Edges,
        Seq<BrokenLink> Broken,
        Seq<UnitFact> Units,
        Seq<string> Logs,
        ClosureUsage Usage,
        ClosureTerminal Terminal) {
        internal static ClosureWalk Of(ArchiveTarget root) => new(
            Pending: Seq(root),
            Seen: Seq(root.Path).ToHashSet(),
            Failed: HashMap<string, string>(),
            Edges: Seq<ClosureEdge>(),
            Broken: Seq<BrokenLink>(),
            Units: Seq<UnitFact>(),
            Logs: Seq<string>(),
            Usage: new ClosureUsage(Archives: 0, Links: 0, Depth: 0, Bytes: 0),
            Terminal: new ClosureTerminal.Complete());

        internal bool Running => Terminal is ClosureTerminal.Complete && !Pending.IsEmpty;

        internal ClosureWalk Edge(string from, string link, Option<string> resolved) =>
            this with { Edges = Edges.Add(value: new ClosureEdge(FromPath: from, StoredLink: link, ResolvedPath: resolved)) };

        internal ClosureWalk Broke(string from, string link, Option<string> resolved, string detail) =>
            this with {
                Broken = Broken.Add(value: new BrokenLink(
                    FromPath: from, StoredLink: link, ResolvedPath: resolved, Detail: detail)),
            };

        internal ClosureWalk Refused(ArchiveTarget target, string detail) =>
            (this with { Failed = Failed.AddOrUpdate(key: target.Path, value: detail) })
                .Broke(from: target.FromPath, link: target.StoredLink, resolved: Some(target.Path), detail: detail);

        internal ClosureWalk Logged(string log) =>
            string.IsNullOrWhiteSpace(value: log) ? this : this with { Logs = Logs.Add(value: log) };

        internal ClosureWalk Read(string path, UnitSystem units, string log) =>
            Logged(log: log) with { Units = Units.Add(value: new UnitFact(Path: path, Units: units)) };

        internal ClosureWalk Exhausted(ClosureLimit limit, long observed, long allowed, string path) =>
            this with {
                Terminal = new ClosureTerminal.Exhausted(
                    Limit: limit, Observed: observed, Allowed: allowed, Path: path),
            };
    }

    // The drive is a bounded fixpoint, not a loop: enqueue-side deduplication makes every frontier entry a fresh
    // archive, so `MaxArchives + 1` ticks strictly cover the walk (the extra tick is the one that exhausts), and a
    // settled walk re-emits itself, keeping the fold total.
    private static Fin<ClosureReport> ArchiveClosure(string rootPath, ClosureBudget budget, Op op) =>
        from plane in ArchivePlane.Current(op: op)
        from root in Canonical(path: rootPath, op: op)
        from anchor in Optional(System.IO.Path.GetDirectoryName(path: root)).ToFin(Fail: op.InvalidInput())
        from handle in ArchiveRoot.Open(path: anchor, plane: plane, op: op)
        from report in op.Catch(() => {
            using (handle) {
                ClosureScope scope = new(Root: handle, Anchor: anchor, Plane: plane, Budget: budget, Op: op);
                ClosureWalk settled = toSeq(Enumerable.Range(start: 0, count: checked(budget.MaxArchives + 1))).Fold(
                    ClosureWalk.Of(root: new ArchiveTarget(
                        FromPath: root, StoredLink: rootPath, Path: root, Depth: 0)),
                    (walk, _) => walk.Running ? Step(walk: walk, scope: scope) : walk);
                return Fin.Succ(value: Reported(walk: settled, plane: plane));
            }
        })
        select report;

    private static ClosureWalk Step(ClosureWalk walk, ClosureScope scope) =>
        walk.Pending.Head.Match(
            None: () => walk,
            Some: target => Advanced(walk: walk with { Pending = walk.Pending.Tail }, target: target, scope: scope));

    private static ClosureWalk Advanced(ClosureWalk walk, ArchiveTarget target, ClosureScope scope) =>
        walk.Usage.Archives >= scope.Budget.MaxArchives
            ? walk.Exhausted(
                limit: ClosureLimit.Archives,
                observed: checked((long)walk.Usage.Archives + 1),
                allowed: scope.Budget.MaxArchives,
                path: target.Path)
            : scope.Root.Open(candidate: target.Path, op: scope.Op).Match(
                Fail: error => walk.Refused(target: target, detail: error.Message),
                Succ: input => Measured(walk: walk, input: input, target: target, scope: scope));

    // Exemption: the input handle's `using` is the platform-forced disposal seam bracketing one archive read.
    private static ClosureWalk Measured(ClosureWalk walk, ArchiveInput input, ArchiveTarget target, ClosureScope scope) {
        using (input) {
            long extent = input.Length;
            return extent > scope.Budget.MaxBytes - walk.Usage.Bytes
                ? walk.Exhausted(
                    limit: ClosureLimit.Bytes,
                    observed: extent > long.MaxValue - walk.Usage.Bytes ? long.MaxValue : walk.Usage.Bytes + extent,
                    allowed: scope.Budget.MaxBytes,
                    path: target.Path)
                : Scanned(
                    walk: walk with {
                        Usage = walk.Usage with {
                            Archives = checked(walk.Usage.Archives + 1),
                            Bytes = checked(walk.Usage.Bytes + extent),
                            Depth = int.Max(walk.Usage.Depth, target.Depth),
                        },
                    },
                    input: input,
                    target: target,
                    scope: scope);
        }
    }

    private static ClosureWalk Scanned(ClosureWalk walk, ArchiveInput input, ArchiveTarget target, ClosureScope scope) =>
        InspectArchive(input: input, op: scope.Op).Match(
            Fail: error => walk.Refused(target: target, detail: error.Message),
            Succ: scan => scan.Switch(
                rejected: rejected => walk
                    .Logged(log: rejected.NativeLog)
                    .Refused(target: target, detail: rejected.Error.Message),
                read: read => Expanded(
                    walk: walk.Read(path: target.Path, units: read.Units, log: read.NativeLog),
                    links: read.Links,
                    target: target,
                    scope: scope)));

    private static ClosureWalk Expanded(ClosureWalk walk, Seq<string> links, ArchiveTarget target, ClosureScope scope) {
        long observed = links.Count > long.MaxValue - walk.Usage.Links
            ? long.MaxValue
            : walk.Usage.Links + links.Count;
        return observed > scope.Budget.MaxLinks
            ? walk.Exhausted(
                limit: ClosureLimit.Links,
                observed: observed,
                allowed: scope.Budget.MaxLinks,
                path: target.Path)
            : links.Fold(
                walk with { Usage = walk.Usage with { Links = observed } },
                (state, link) => state.Terminal is ClosureTerminal.Complete
                    ? Linked(walk: state, link: link, target: target, scope: scope)
                    : state);
    }

    private static ClosureWalk Linked(ClosureWalk walk, string link, ArchiveTarget target, ClosureScope scope) =>
        Canonical(
            path: System.IO.Path.IsPathRooted(path: link)
                ? link
                : System.IO.Path.Combine(
                    path1: System.IO.Path.GetDirectoryName(path: target.Path) ?? string.Empty,
                    path2: link),
            op: scope.Op).Match(
            Fail: error => walk
                .Edge(from: target.Path, link: link, resolved: Option<string>.None)
                .Broke(from: target.Path, link: link, resolved: Option<string>.None, detail: error.Message),
            Succ: resolved => Within(root: scope.Anchor, candidate: resolved, comparison: scope.Plane.Comparison)
                ? Frontier(
                    walk: walk.Edge(from: target.Path, link: link, resolved: Some(resolved)),
                    from: target.Path,
                    link: link,
                    resolved: resolved,
                    depth: checked(target.Depth + 1),
                    scope: scope)
                : walk
                    .Edge(from: target.Path, link: link, resolved: Option<string>.None)
                    .Broke(
                        from: target.Path,
                        link: link,
                        resolved: Some(resolved),
                        detail: scope.Op.InvalidContext().Message));

    // Deduplication is enqueue-side: a link onto an already-failed archive receipts its broken re-reference here,
    // a link onto a live shared dependency records its edge and stops, and only a fresh path joins the frontier.
    private static ClosureWalk Frontier(
        ClosureWalk walk,
        string from,
        string link,
        string resolved,
        int depth,
        ClosureScope scope) =>
        depth > scope.Budget.MaxDepth
            ? walk.Exhausted(
                limit: ClosureLimit.Depth,
                observed: depth,
                allowed: scope.Budget.MaxDepth,
                path: resolved)
            : walk.Failed.Find(key: resolved).Case switch {
                string prior => walk.Broke(from: from, link: link, resolved: Some(resolved), detail: prior),
                _ when walk.Seen.Contains(value: resolved) => walk,
                _ => walk with {
                    Seen = walk.Seen.Add(value: resolved),
                    Pending = walk.Pending.Add(value: new ArchiveTarget(
                        FromPath: from, StoredLink: link, Path: resolved, Depth: depth)),
                },
            };

    private static ClosureReport Reported(ClosureWalk walk, ArchivePlane plane) {
        BidirectionalGraph<string, SEdge<string>> graph = new(allowParallelEdges: false);
        walk.Edges.Iter(edge => edge.ResolvedPath.Iter(target => graph.AddVerticesAndEdge(
            edge: new SEdge<string>(source: edge.FromPath, target: target))));
        return new ClosureReport(
            Edges: walk.Edges,
            Broken: walk.Broken,
            Cycles: GraphFold.Cycles(graph: graph, comparer: plane.Comparer, order: plane.Comparer),
            Units: walk.Units,
            NativeLog: walk.Logs,
            Usage: walk.Usage,
            Terminal: walk.Terminal);
    }

    private static Fin<string> Canonical(string path, Op op) => op.Catch(() => {
        string full = System.IO.Path.TrimEndingDirectorySeparator(path: System.IO.Path.GetFullPath(path: path));
        string prefix = System.IO.Path.GetPathRoot(path: full)
            ?? throw new InvalidOperationException(message: op.InvalidInput().Message);
        string resolved = full[prefix.Length..]
            .Split(
                separator: [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                options: StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(prefix, static (held, segment) => Resolved(
                candidate: System.IO.Path.Combine(path1: held, path2: segment)));
        return Fin.Succ(value: System.IO.Path.TrimEndingDirectorySeparator(
            path: System.IO.Path.GetFullPath(path: resolved)));
    });

    private static string Resolved(string candidate) =>
        (System.IO.Directory.Exists(path: candidate)
            ? (System.IO.FileSystemInfo)new System.IO.DirectoryInfo(path: candidate)
            : new System.IO.FileInfo(fileName: candidate))
            .ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? candidate;

    private static bool Within(string root, string candidate, StringComparison comparison) {
        string relative = System.IO.Path.GetRelativePath(relativeTo: root, path: candidate);
        return !System.IO.Path.IsPathRooted(path: relative)
            && !string.Equals(a: relative, b: "..", comparisonType: comparison)
            && !relative.StartsWith(value: $"..{System.IO.Path.DirectorySeparatorChar}", comparisonType: comparison)
            && !relative.StartsWith(value: $"..{System.IO.Path.AltDirectorySeparatorChar}", comparisonType: comparison);
    }

    private static Fin<ArchiveScan> InspectArchive(ArchiveInput input, Op op) => op.Catch(() => {
        string snapshot = System.IO.Path.Combine(
            path1: System.IO.Path.GetTempPath(),
            path2: $"{Guid.NewGuid():N}.3dm");
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(
            minimumLength: checked((int)long.Clamp(value: input.Length, min: 1, max: SnapshotChunkBytes)));
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

| [INDEX] | [OWNER]         | [INGRESS]                 | [ALGORITHM]                             | [EGRESS]           |
| :-----: | :-------------- | :------------------------ | :-------------------------------------- | :----------------- |
|  [01]   | `BlockGraph`    | `Ask`                     | `QuikGraph` · host reads · archive fold | `BlockGraphAnswer` |
|  [02]   | `GraphFold`     | transient graph           | SCC · components · order · reduction    | graph evidence     |
|  [03]   | `ClosureReport` | `Archives(ClosureBudget)` | budget-bounded `ClosureWalk` fold       | closure evidence   |
|  [04]   | `ArchivePlane`  | `Current`                 | per-RID open flags · path comparison    | plane or refusal   |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

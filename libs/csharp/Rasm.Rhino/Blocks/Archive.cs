using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
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
        ImmutableArray<ArchivePath> LinkedArchives) {
        public static Graph Empty { get; } = new(Definitions: [], Instances: [], LinkedArchives: []);
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Instance(Guid ObjectId, DefinitionId ParentDefId, Transform Xform);

    // ---- [BUILD] -------------------------------------------------------------------------
    public static Fin<Graph> From(File3dm model, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(model).ToFin(Fail: op.InvalidInput())
            .Bind(active => op.Catch(() => Build(active: active, op: op)));
    }

    private static Fin<Graph> Build(File3dm active, Op op) =>
        toSeq(active.AllInstanceDefinitions)
            .Map(geo => Definition.From(archive: geo, key: op))
            .TraverseM(identity).As()
            .Map(defs => Compose(active: active, definitions: defs));

    private static Graph Compose(File3dm active, Seq<Definition> definitions) {
        ImmutableArray<Definition> defArray = [.. definitions];
        FrozenDictionary<Guid, Definition> lookup = defArray.ToFrozenDictionary(static d => d.Id.Value);
        ImmutableArray<Instance> instances = [.. toSeq(active.Objects).Choose(o => ProjectOne(o: o, lookup: lookup))];
        ImmutableArray<ArchivePath> linked = [.. toSeq(defArray).Choose(static d => d.Source).Distinct()];
        return new Graph(Definitions: defArray, Instances: instances, LinkedArchives: linked);
    }

    private static Option<Instance> ProjectOne(File3dmObject o, FrozenDictionary<Guid, Definition> lookup) =>
        (o.Geometry, lookup) switch {
            (InstanceReferenceGeometry r, _) when r.ParentIdefId != Guid.Empty
                                              && lookup.TryGetValue(key: r.ParentIdefId, value: out Definition? parent) =>
                Some(value: new Instance(ObjectId: o.Attributes.ObjectId, ParentDefId: parent.Id, Xform: r.Xform)),
            _ => Option<Instance>.None,
        };

    public static Fin<DocumentReceipt> AddLinked(File3dm model, Seq<FileEndpoint> sources, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(model).ToFin(Fail: op.InvalidInput()).Bind(active => sources.IsEmpty
            ? Fin.Succ(value: DocumentReceipt.Empty)
            : sources
                .Map(endpoint => op.Catch(() => AddOne(model: active, endpoint: endpoint, op: op)))
                .TraverseM(identity).As()
                .Map(static changes => DocumentReceipt.Empty with { ResourceChanged = changes }));
    }

    // ---- [BAKE PLAN] (Speckle topological-sort pattern) ----------------------------------
    /// Topologically-sorted bake order: definitions with no nested-instance dependencies first.
    /// Depth = length of longest chain of (def → nested-def) edges. Cycle-safe via visiting set.
    ///
    /// Nested-def discovery: each Instance.ObjectId may be a member of some def (D.MemberIds);
    /// that maps D → set-of-nested-def-ids. Pre-built once into a FrozenDictionary lookup so
    /// the recursive walk is O(N + E) over (defs + nested-edges) not O(N × instances).
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

    /// Recursive walk over the nested-def graph; visiting set guards cycles. Pure (HashMap memo
    /// is by-value, recursion does not mutate); the boundary `int` return collapses over depth.
    private static int ComputeDepth(Guid defId, HashMap<Guid, ImmutableArray<Guid>> nestedDefs, LanguageExt.HashSet<Guid> visiting, HashMap<Guid, int> memo) =>
        (memo.Find(key: defId), LxHashSet.contains(set: visiting, value: defId)) switch {
            ( { IsSome: true, Case: int cached }, _) => cached,
            (_, true) => 0,        // cycle: contribute zero depth, don't recurse
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
            Path: d.Source.Map(static p => p.Value),
            Id: Some(value: d.Id.Value),
            Source: Some(value: d.IsLinked ? "linked-block" : "block")));
    }

    public static Seq<FileResourceEdge> ToFileResourceEdges(Graph graph) {
        ArgumentNullException.ThrowIfNull(argument: graph);
        Seq<FileResourceEdge> linked = toSeq(graph.Definitions).Bind(static d => d.Source.Match(
            Some: p => Seq(new FileResourceEdge(
                FromKind: DocumentResourceKind.Block, FromId: Some(value: d.Id.Value),
                ToKind: DocumentResourceKind.FileReference, ToId: Option<Guid>.None,
                Role: FileResourceRole.Linked, Path: Some(value: p.Value))),
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

    private static Fin<DocumentResourceChange> AddOne(File3dm model, FileEndpoint endpoint, Op op) =>
        model.AllInstanceDefinitions.AddLinked(
            filename: endpoint.Path,
            name: IOPath.GetFileNameWithoutExtension(path: endpoint.Path),
            description: string.Empty) switch {
                >= 0 => Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: endpoint.Path)),
                _ => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
            };
}

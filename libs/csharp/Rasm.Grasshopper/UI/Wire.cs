using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<UiFault>]
public readonly partial struct WireObjectLimit {
    internal const int DefaultCount = 32;

    // DefaultCount is non-negative, so the smart factory always succeeds; materialize the validated value once.
    public static readonly WireObjectLimit Default = Create(value: DefaultCount);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref int value) =>
        validationError = value >= 0
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(WireObjectLimit)), message: string.Create(CultureInfo.InvariantCulture, $"must be non-negative (got {value})."));
}

[SkipUnionOps]
[Union]
public partial record WireSnapshot {
    private WireSnapshot() { }
    public sealed record ConnectedCase(Guid Source, Guid Target, bool SourceResolved, bool TargetResolved, bool Connected, bool Selected) : WireSnapshot;
    public sealed record AbsentCase : WireSnapshot;

    public static readonly WireSnapshot Absent = new AbsentCase();
}

[GenerateUnionOps]
[Union]
public partial record WireSelectionOp {
    private WireSelectionOp() { }
    public sealed partial record SelectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed partial record DeselectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed partial record DeselectAllCase : WireSelectionOp;

    public static readonly WireSelectionOp DeselectAll = new DeselectAllCase();
}

[SkipUnionOps]
[Union]
public partial record GraphKey {
    private GraphKey() { }
    public sealed record ParameterCase(Guid Id) : GraphKey;
    public sealed record OwnerCase(Guid Id) : GraphKey;
    public static GraphKey Parameter(Guid id) => new ParameterCase(Id: id);
    public static GraphKey Owner(Guid id) => new OwnerCase(Id: id);
    internal Guid SeedId => Switch(parameterCase: static p => p.Id, ownerCase: static o => o.Id);
}

[SmartEnum<int>]
public sealed partial class WireTraversal {
    private delegate IEnumerable<IDocumentObject> WalkFn(GhObjectList objects, GraphKey key);

    // Depth is case identity: All* items walk the transitive closure (FindAll*/SearchUpstream); Immediate*
    // items walk one hop (FindImmediate*). The parameter case has no host immediate search, so it always
    // walks the transitive frontier; the owner case honours the depth via its connectivity fetcher.
    public static readonly WireTraversal Upstream = new(key: 0, walkObjects: static (objects, key) => WalkAll(objects, key, up: true));
    public static readonly WireTraversal Downstream = new(key: 1, walkObjects: static (objects, key) => WalkAll(objects, key, up: false));
    public static readonly WireTraversal Bidirectional = new(key: 2, walkObjects: static (objects, key) => WalkAll(objects, key, up: true).Concat(WalkAll(objects, key, up: false)));
    public static readonly WireTraversal ImmediateUpstream = new(key: 3, walkObjects: static (objects, key) => WalkImmediate(objects, key, up: true));
    public static readonly WireTraversal ImmediateDownstream = new(key: 4, walkObjects: static (objects, key) => WalkImmediate(objects, key, up: false));
    public static readonly WireTraversal Neighbours = new(key: 5, walkObjects: static (objects, key) => WalkImmediate(objects, key, up: true).Concat(WalkImmediate(objects, key, up: false)));

    [UseDelegateFromConstructor]
    internal partial IEnumerable<IDocumentObject> WalkObjects(GhObjectList objects, GraphKey key);

    private static IEnumerable<IDocumentObject> WalkAll(GhObjectList objects, GraphKey key, bool up) =>
        Walk(
            objects: objects,
            key: key,
            up: up,
            immediate: false,
            ownerWalk: static (connectivity, id, upstream) => upstream ? connectivity.FindAllInputs(id) : connectivity.FindAllOutputs(id));

    private static IEnumerable<IDocumentObject> WalkImmediate(GhObjectList objects, GraphKey key, bool up) =>
        Walk(objects: objects, key: key, up: up, immediate: true, ownerWalk: ImmediateOwnerWalk);

    private static IEnumerable<IDocumentObject> Walk(GhObjectList objects, GraphKey key, bool up, bool immediate, Func<Connectivity, Guid, bool, IEnumerable<ConnectiveObject>> ownerWalk) =>
        key.Switch(
            parameterCase: p => Optional(objects.FindParameter(instanceId: p.Id))
                .Map(param => immediate
                    ? WalkImmediateParameter(objects: objects, parameter: param, up: up)
                    : up ? objects.SearchUpstream(parameter: param) : objects.SearchDownstream(parameter: param))
                .IfNone(noneValue: []),
            ownerCase: o => ownerWalk(objects.Connectivity, o.Id, up)
                .Select(co => objects.Find(instanceId: co.Id))
                .OfType<IDocumentObject>());

    private static IEnumerable<ConnectiveObject> ImmediateOwnerWalk(Connectivity connectivity, Guid id, bool upstream) =>
        upstream ? connectivity.FindImmediateInputs(id) : connectivity.FindImmediateOutputs(id);

    private static IEnumerable<IDocumentObject> WalkImmediateParameter(GhObjectList objects, IParameter parameter, bool up) =>
        (up ? parameter.Inputs.Forwards : parameter.Outputs.Forwards)
            .Select(id => objects.FindParameter(instanceId: id))
            .OfType<IDocumentObject>();
}

// Optional per-edit arguments: connection endpoint indices, native omit-set, and secondary endpoints for
// replacement/swap/bypass verbs. Default is the zero/empty carrier the index-free verbs ignore.
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireEditArgs(int SourceIndex = 0, int TargetIndex = 0, Seq<Guid> Omit = default, Guid Source = default, Guid Target = default);

[SmartEnum<int>]
public sealed partial class WireEdit {
    // The op is threaded in by the caller as Op.Of($"Wire.{edit}") so each item names itself from its case
    // identity — ToString() == the former literal, so provenance is unchanged and the 8 literals are gone.
    // ApplyEditRow gates RequiresSource/RequiresTarget before dispatch, so run bodies !-assert the resolved
    // sides they declared required — one widest delegate carries pair, endpoint, and connect verbs uniformly.
    // Disconnect-all verbs operate on one endpoint, so they never require a live source<->target pair.
    public bool RequiresConnectedPair { get; }
    public bool RequiresSource { get; }
    public bool RequiresTarget { get; }

    // Connect/ConnectAt CREATE a connection, so they require a valid source/target pair but NOT an
    // already-live one — RequiresConnectedPair is false (MutateConnectedWire's RequireConnected pre-guard
    // would otherwise reject the very pair being connected); native Connect owns compatibility and no-op status.
    public static readonly WireEdit Connect = Make(
        key: 0, source: true, target: true, connected: false,
        run: static (op, _, objects, source, target, actions, _) =>
            NativeConnect(op: op, objects: objects, source: source!, target: target!, actions: actions));

    // Disconnect/Delete operate on an existing wire — MutateConnectedWire's RequiresConnectedPair guard owns
    // the is-connected check, so no in-verb re-guard.
    public static readonly WireEdit Disconnect = Make(
        key: 1, source: true, target: true, connected: true,
        run: static (op, _, _, source, target, actions, _) =>
            Native(op: op, name: "Connections.Disconnect", run: () => Connections.Disconnect(source: source!, target: target!, undo: actions)));

    public static readonly WireEdit Delete = Make(
        key: 2, source: true, target: true, connected: true,
        run: static (op, methods, _, source, target, actions, _) =>
            NativeCount(op: op, name: "DocumentMethods.DeleteObjects", run: () => methods.DeleteObjects(objects: [], wires: [new WireEnds(source: source!.InstanceId, target: target!.InstanceId)], actions: actions)));

    public static readonly WireEdit DisconnectInputs = Make(
        key: 3, source: false, target: true, connected: false,
        run: static (op, _, _, _, target, actions, _) =>
            NativeCount(op: op, name: "Connections.DisconnectAllInputs", run: () => Connections.DisconnectAllInputs(target: target!, undo: actions)));

    public static readonly WireEdit DisconnectOutputs = Make(
        key: 4, source: true, target: false, connected: false,
        run: static (op, _, _, source, _, actions, _) =>
            NativeCount(op: op, name: "Connections.DisconnectAllOutputs", run: () => Connections.DisconnectAllOutputs(source: source!, undo: actions)));

    public static readonly WireEdit ConnectAt = Make(
        key: 5, source: true, target: true, connected: false,
        run: static (op, _, objects, source, target, actions, args) =>
            NativeConnect(op: op, objects: objects, source: source!, target: target!, actions: actions, indices: Some((args.SourceIndex, args.TargetIndex))));

    // Empty omissions would disconnect ALL inputs — a footgun masquerading as a selective verb; reject so the
    // caller routes through DisconnectInputs explicitly when total clearance is intended.
    public static readonly WireEdit DisconnectInputsExcept = Make(
        key: 6, source: false, target: true, connected: false,
        run: static (op, _, _, _, target, actions, args) =>
            args.Omit.IsEmpty
                ? Fin.Fail<int>(error: UiFault.InvalidInput(op: op, detail: "DisconnectInputsExcept requires at least one omitted parameter; use DisconnectInputs to clear all"))
                : NativeCount(op: op, name: "Connections.DisconnectAllInputsExcept", run: () => Connections.DisconnectAllInputsExcept(target: target!, omissions: [.. args.Omit], undo: actions)));

    public static readonly WireEdit DisconnectOutputsExcept = Make(
        key: 7, source: true, target: false, connected: false,
        run: static (op, _, _, source, _, actions, args) =>
            args.Omit.IsEmpty
                ? Fin.Fail<int>(error: UiFault.InvalidInput(op: op, detail: "DisconnectOutputsExcept requires at least one omitted parameter; use DisconnectOutputs to clear all"))
                : NativeCount(op: op, name: "Connections.DisconnectAllOutputsExcept", run: () => Connections.DisconnectAllOutputsExcept(source: source!, omissions: [.. args.Omit], undo: actions)));
    public static readonly WireEdit CopyInputs = Make(
        key: 8, source: true, target: true, connected: false,
        run: static (op, _, _, source, target, actions, _) =>
            Rewire(op: op, source: source!, target: target!, actions: actions, clearSource: false, inputs: true));
    public static readonly WireEdit MigrateInputs = Make(
        key: 9, source: true, target: true, connected: false,
        run: static (op, _, _, source, target, actions, _) =>
            Rewire(op: op, source: source!, target: target!, actions: actions, clearSource: true, inputs: true));
    public static readonly WireEdit CopyOutputs = Make(
        key: 10, source: true, target: true, connected: false,
        run: static (op, _, _, source, target, actions, _) =>
            Rewire(op: op, source: source!, target: target!, actions: actions, clearSource: false, inputs: false));
    public static readonly WireEdit MigrateOutputs = Make(
        key: 11, source: true, target: true, connected: false,
        run: static (op, _, _, source, target, actions, _) =>
            Rewire(op: op, source: source!, target: target!, actions: actions, clearSource: true, inputs: false));
    public static readonly WireEdit ReplaceSource = Make(
        key: 12, source: true, target: true, connected: true,
        run: static (op, _, objects, oldSource, target, actions, args) =>
            from newSource in NeedArg(op: op, objects: objects, id: args.Source, role: "replacement source")
            from changed in Native(op: op, name: "Connections.ReplaceSource", run: () => Connections.ReplaceSource(oldSource: oldSource!, newSource: newSource, target: target!, undo: actions))
            select changed);
    public static readonly WireEdit ReplaceTarget = Make(
        key: 13, source: true, target: true, connected: true,
        run: static (op, _, objects, source, oldTarget, actions, args) =>
            from newTarget in NeedArg(op: op, objects: objects, id: args.Target, role: "replacement target")
            from changed in Native(op: op, name: "Connections.ReplaceTarget", run: () => Connections.ReplaceTarget(source: source!, oldTarget: oldTarget!, newTarget: newTarget, undo: actions))
            select changed);
    public static readonly WireEdit SwapSources = Make(
        key: 14, source: true, target: true, connected: true,
        run: static (op, _, objects, sourceA, targetA, actions, args) =>
            from sourceB in NeedArg(op: op, objects: objects, id: args.Source, role: "source B")
            from targetB in NeedArg(op: op, objects: objects, id: args.Target, role: "target B")
            from changed in Native(op: op, name: "Connections.SwapSources", run: () => Connections.SwapSources(sourceA: sourceA!, sourceB: sourceB, targetA: targetA!, targetB: targetB, undo: actions))
            select changed);
    public static readonly WireEdit CutOutMiddleMan = Make(
        key: 15, source: true, target: true, connected: true,
        run: static (op, _, objects, left, middle, actions, args) =>
            from right in NeedArg(op: op, objects: objects, id: args.Target, role: "right")
            from changed in Native(op: op, name: "Connections.CutOutMiddleMan", run: () => Connections.CutOutMiddleMan(left: left!, middle: middle!, right: right, undo: actions))
            select changed);

    [UseDelegateFromConstructor]
    internal partial Fin<int> Apply(Op op, GhDocumentMethods methods, GhObjectList objects, IParameter? source, IParameter? target, ActionList actions, WireEditArgs args);

    // One factory: the required-side flags drive ApplyEditRow's pre-dispatch gate, so the run body receives
    // already-resolved sides (asserted via ! inside the body) — pair, endpoint, and connect verbs collapse here.
    private static WireEdit Make(int key, bool source, bool target, bool connected, Func<Op, GhDocumentMethods, GhObjectList, IParameter?, IParameter?, ActionList, WireEditArgs, Fin<int>> run) =>
        new(
            key: key,
            requiresConnectedPair: connected,
            requiresSource: source,
            requiresTarget: target,
            apply: run);

    private static Fin<IParameter> NeedArg(Op op, GhObjectList objects, Guid id, string role) =>
        id == Guid.Empty
            ? Fin.Fail<IParameter>(error: UiFault.InvalidInput(op: op, detail: $"{role} id is required"))
            : Optional(objects.FindParameter(instanceId: id))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"{role} parameter {id} not found"));

    // One native-mutation scaffold: op.Attempt(run, name).Bind(interpret). The native return type T varies
    // (bool/int), the interpreter projects it to a Fin<int> row count — Native interprets bool→1/reject,
    // NativeCount interprets int>=0→count/reject, NativeConnect short-circuits the idempotent case then routes
    // through here with an always-Succ interpret.
    private static Fin<int> NativeResult<T>(Op op, string name, Func<T> run, Func<T, Fin<int>> interpret) =>
        op.Attempt(body: run, what: name).Bind(interpret);

    private static Fin<int> Native(Op op, string name, Func<bool> run) =>
        NativeResult(op: op, name: name, run: run, interpret: changed =>
            guard(changed, (Error)UiFault.MutationRejected(op: op, detail: $"{name} returned false")).ToFin().Map(_ => 1));

    private static Fin<int> NativeCount(Op op, string name, Func<int> run) =>
        NativeResult(op: op, name: name, run: run, interpret: count =>
            guard(count >= 0, (Error)UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"{name} returned {count}"))).ToFin().Map(_ => count));

    private static Fin<int> NativeConnect(Op op, GhObjectList objects, IParameter source, IParameter target, ActionList actions, Option<(int Source, int Target)> indices = default) =>
        Wire.IsConnected(objects: objects, wire: new WireEnds(source: source.InstanceId, target: target.InstanceId))
            ? Fin.Succ(value: 0)
            : NativeResult(
                op: op,
                name: "Connections.Connect",
                run: () => indices.Match(
                    Some: i => Connections.Connect(source: source, target: target, indexAtSource: i.Source, indexAtTarget: i.Target, undo: actions),
                    None: () => Connections.Connect(source: source, target: target, undo: actions)),
                interpret: static changed => Fin.Succ(value: changed ? 1 : 0));

    private static Fin<int> Rewire(Op op, IParameter source, IParameter target, ActionList actions, bool clearSource, bool inputs) {
        RewireMode mode = RewireMode.Of(inputs: inputs, clearSource: clearSource);
        return NativeCount(op: op, name: mode.Name, run: () => mode.Run(source: source, target: target, actions: actions));
    }

    // The 2x2 (inputs, clearSource) matrix carried its native call-name and invocation as two parallel switches;
    // one SmartEnum item per quadrant fuses name + run delegate so Rewire resolves the mode and dispatches once.
    [SmartEnum<int>]
    private sealed partial class RewireMode {
        private delegate int RunFn(IParameter source, IParameter target, ActionList actions);

        public string Name { get; }

        public static readonly RewireMode MigrateInputs = new(key: 0, name: "Connections.MigrateAllInputs", run: static (source, target, actions) => Connections.MigrateAllInputs(oldTarget: source, newTarget: target, undo: actions));
        public static readonly RewireMode CopyInputs = new(key: 1, name: "Connections.CopyAllInputs", run: static (source, target, actions) => Connections.CopyAllInputs(oldTarget: source, newTarget: target, undo: actions));
        public static readonly RewireMode MigrateOutputs = new(key: 2, name: "Connections.MigrateAllOutputs", run: static (source, target, actions) => Connections.MigrateAllOutputs(oldSource: source, newSource: target, undo: actions));
        public static readonly RewireMode CopyOutputs = new(key: 3, name: "Connections.CopyAllOutputs", run: static (source, target, actions) => Connections.CopyAllOutputs(oldSource: source, newSource: target, undo: actions));

        [UseDelegateFromConstructor]
        public partial int Run(IParameter source, IParameter target, ActionList actions);

        public static RewireMode Of(bool inputs, bool clearSource) =>
            (inputs, clearSource) switch {
                (true, true) => MigrateInputs,
                (true, false) => CopyInputs,
                (false, true) => MigrateOutputs,
                _ => CopyOutputs,
            };
    }
}

[SmartEnum<int>]
public sealed partial class GraphMetric {
    private delegate Fin<WireResult> RunFn(GrasshopperUi.Scope scope, Seq<Guid> ids);

    // Native Connectivity.IsLinear unconditionally reads array[0] of the sorted node set (decompiled), so an
    // empty subgraph throws IndexOutOfRangeException. An empty subgraph is definitively non-linear, so the
    // library projects it to IsLinear:false here rather than surfacing the host throw to callers.
    public static readonly GraphMetric Linearity = new(
        key: 0,
        run: static (scope, ids) =>
            NonEmpty(ids: ids).Match(
                None: () => Fin.Succ(value: (WireResult)new WireResult.LinearityResult(Linearity: new WireLinearity(
                    IsLinear: false, Start: Option<Guid>.None, End: Option<Guid>.None))),
                Some: present =>
                    from objects in scope.NeedObjects()
                    from result in Op.Of(name: nameof(Linearity)).Attempt(
                        body: () => {
                            Connectivity connectivity = objects.Connectivity;
                            bool linear = connectivity.IsLinear(ids: [.. present], first: out ConnectiveObject? start, last: out ConnectiveObject? end);
                            return (WireResult)new WireResult.LinearityResult(Linearity: new WireLinearity(
                                IsLinear: linear,
                                Start: (start?.Id).NonEmpty(),
                                End: (end?.Id).NonEmpty()));
                        },
                        what: "Connectivity.IsLinear")
                    select result));
    public static readonly GraphMetric Topology = new(
        key: 1,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(Topology)).Attempt(
                body: () => (WireResult)new WireResult.TopologyResult(Topology: objects.Connectivity.SubsetTopology(ids: [.. ids])),
                what: "Connectivity.SubsetTopology")
            select result);
    public static readonly GraphMetric SortedTopology = new(
        key: 2,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(SortedTopology)).Attempt(
                body: () => {
                    Connectivity connectivity = objects.Connectivity;
                    ConnectiveObject[] nodes = [.. ids.Choose(id => connectivity.Find(id, out ConnectiveObject? co) ? Optional(co) : Option<ConnectiveObject>.None)];
                    ConnectiveObject[] sorted = connectivity.SortCausally(objects: nodes);
                    return (WireResult)new WireResult.SortedIdsResult(Ids: toSeq(sorted.Select(static co => co.Id)));
                },
                what: "Connectivity.SortCausally")
            select result);
    // Relay-collapsed topology: drop dangling + simple relays (keep junctions), then read the subset topology
    // of the collapsed graph — reuses the GraphTopology result of the plain Topology metric. The relay triple
    // is named item data (Dangling, Simple, Complex) so WithoutRelays' third flag is a reachable parameter,
    // not a dead inline literal; RelayCollapsed pins it to (dangling:true, simple:true, complex:false).
    private static readonly (bool Dangling, bool Simple, bool Complex) RelayCollapsedFlags = (Dangling: true, Simple: true, Complex: false);
    public static readonly GraphMetric RelayCollapsed = new(
        key: 3,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(RelayCollapsed)).Attempt(
                body: () => {
                    Connectivity connectivity = objects.Connectivity;
                    (bool dangling, bool simple, bool complex) = RelayCollapsedFlags;
                    return (WireResult)new WireResult.TopologyResult(Topology: connectivity
                        .WithoutRelays(dangling: dangling, simple: simple, complex: complex)
                        .SubsetTopology(ids: [.. ids]));
                },
                what: "Connectivity.WithoutRelays")
            select result);
    // All upstream->downstream paths between the first and last id; native FindConnections is unbounded, so the
    // local traversal caps yielded paths and rejects cyclic path extensions before enqueueing them.
    public static readonly GraphMetric Paths = new(
        key: 4,
        run: static (scope, ids) =>
            from endpoints in PathEndpoints(ids: ids)
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(Paths)).Attempt(
                body: () => (WireResult)new WireResult.PathsResult(Paths: Wire.BoundedPaths(
                    connectivity: objects.Connectivity,
                    source: endpoints.Source,
                    target: endpoints.Target,
                    limit: WireObjectLimit.DefaultCount)),
                what: "Connectivity.Find")
            select result);
    public static readonly GraphMetric Integrity = new(
        key: 5,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from document in scope.NeedDocument()
            select (WireResult)new WireResult.IntegrityResult(Integrity: Wire.IntegrityOf(objects: objects, document: document, seeds: ids)));

    public static WireQuery Query(Seq<Guid> ids, GraphMetric kind) => WireQuery.GraphMetric(ids: ids, kind: kind);

    [UseDelegateFromConstructor]
    internal partial Fin<WireResult> Run(GrasshopperUi.Scope scope, Seq<Guid> ids);

    private static Fin<(Guid Source, Guid Target)> PathEndpoints(Seq<Guid> ids) {
        Seq<Guid> ends = ids.Filter(static id => id != Guid.Empty).Distinct();
        return guard(ends.Count >= 2, (Error)UiFault.InvalidInput(op: Op.Of(name: nameof(Paths)), detail: "paths require two non-empty endpoint ids"))
            .ToFin()
            .Map(_ => (Source: ends.Head.IfNone(Guid.Empty), Target: ends.Last.IfNone(Guid.Empty)));
    }

    private static Option<Seq<Guid>> NonEmpty(Seq<Guid> ids) => ids.IsEmpty ? None : Some(ids);
}

[SmartEnum<int>]
public sealed partial class WireListKind {
    private delegate IEnumerable<WireEnds>? WireSource(GhObjectList objects);
    private delegate bool WireFilter(WireSnapshot.ConnectedCase wire);

    public static readonly WireListKind All = new(key: 0, source: static objects => Wire.AllWireEnds(objects: objects), include: static _ => true);
    public static readonly WireListKind Selected = new(key: 1, source: static objects => objects.SelectedWires, include: static _ => true);
    public static readonly WireListKind Dangling = new(key: 2, source: static objects => Wire.AllWireEnds(objects: objects), include: static wire => !wire.Connected);
    public static readonly WireListKind Connected = new(key: 3, source: static objects => Wire.AllWireEnds(objects: objects), include: static wire => wire.Connected);
    // Cyclic: the per-wire Include cannot decide cycle membership in isolation, so it admits all wires and
    // Listed post-filters the projected seq against CycleWires (whole-graph seed). Listed detects this kind by
    // singleton identity rather than a per-item flag, keeping the SmartEnum constructor uniform across items.
    public static readonly WireListKind Cyclic = new(key: 4, source: static objects => Wire.AllWireEnds(objects: objects), include: static _ => true);

    [UseDelegateFromConstructor]
    internal partial IEnumerable<WireEnds>? Source(GhObjectList objects);

    [UseDelegateFromConstructor]
    internal partial bool Include(WireSnapshot.ConnectedCase wire);
}

[GenerateUnionOps]
[Union]
public partial record WireQuery {
    private WireQuery() { }
    public sealed partial record ListCase(WireListKind Kind) : WireQuery;
    public sealed partial record PickCase(PointF Point, PickTolerance Tolerance) : WireQuery;
    public sealed partial record GraphCase(GraphKey Anchor, WireTraversal Direction, WireObjectLimit MaxObjects) : WireQuery;
    public sealed partial record GraphMetricCase(Seq<Guid> Ids, GraphMetric Kind) : WireQuery;
    public sealed partial record CanInsertCase(Guid ObjectId, PointF Location) : WireQuery;
    public sealed partial record RecentlyDrawnCase : WireQuery;
    public static readonly WireQuery All = new ListCase(Kind: WireListKind.All);
    public static readonly WireQuery Selected = new ListCase(Kind: WireListKind.Selected);
    public static readonly WireQuery Dangling = new ListCase(Kind: WireListKind.Dangling);
    public static WireQuery Pick(PointF point, PickTolerance? tolerance = null) => new PickCase(Point: point, Tolerance: tolerance ?? PickTolerance.Create(value: 0f));
    public static WireQuery Graph(GraphKey anchor, WireTraversal? direction = null, WireObjectLimit? maxObjects = null) =>
        new GraphCase(
            Anchor: anchor,
            Direction: direction ?? WireTraversal.Bidirectional,
            MaxObjects: maxObjects ?? WireObjectLimit.Create(value: WireObjectLimit.DefaultCount));
    public static WireQuery Graph(GraphKey anchor, WireTraversal? direction, int maxObjects) =>
        new GraphCase(
            Anchor: anchor,
            Direction: direction ?? WireTraversal.Bidirectional,
            MaxObjects: WireObjectLimit.Create(value: maxObjects));
    public static WireQuery GraphMetric(Seq<Guid> ids, GraphMetric kind) => new GraphMetricCase(Ids: ids, Kind: kind);
    public static WireQuery CanInsert(Guid objectId, PointF location) => new CanInsertCase(ObjectId: objectId, Location: location);
    public static WireQuery RecentlyDrawn() => new RecentlyDrawnCase();
}

// Bounded vocabulary of the host's concrete WireShape subclasses; the InstallShape(WireShapeKind) overload
// funnels through the same validated Type rail as InstallShape(Type), so callers name the shape without
// reaching for typeof at the call site.
[SmartEnum<string>]
public sealed partial class WireShapeKind {
    public Type ShapeType { get; }
    public static readonly WireShapeKind Default = new(key: "default", shapeType: typeof(WireShapeDefault));
}

[GenerateUnionOps]
[Union]
public partial record WireOp : IUiOp<WireResult> {
    private WireOp() { }
    public sealed partial record QueryCase(WireQuery Request) : WireOp;
    public sealed partial record SelectCase(WireSelectionOp Op) : WireOp;
    public sealed partial record SplitCase(WireSnapshot.ConnectedCase Wire, PointF Location) : WireOp;
    public sealed partial record EditCase(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args = default) : WireOp;
    public sealed partial record EditBatchCase(Seq<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> Edits) : WireOp;
    public sealed partial record RouteCase(Seq<Guid> Chain) : WireOp;
    public sealed partial record InstallShapeCase(Type ShapeType) : WireOp;
    public sealed partial record OverlayCase(WireOverlayStyle Style, MotionClock? Clock = null) : WireOp;
    public sealed partial record WirePaintObserveCase(MotionClock? Clock = null) : WireOp;
    public sealed partial record DiagnosticsCase(Seq<Guid> Seeds) : WireOp;
    public static WireOp Query(WireQuery query) => new QueryCase(Request: query);
    public static WireOp Select(WireSelectionOp op) => new SelectCase(Op: op);
    public static WireOp Split(WireSnapshot.ConnectedCase wire, PointF location) => new SplitCase(Wire: wire, Location: location);
    public static WireOp Edit(WireSnapshot.ConnectedCase wire, WireEdit edit, WireEditArgs args = default) => new EditCase(Wire: wire, Kind: edit, Args: args);
    // Batch: N wire edits thread one ActionList → one undo entry, reusing the WireEdit dispatch.
    public static WireOp EditBatch(params ReadOnlySpan<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> edits) =>
        new EditBatchCase(Edits: toSeq(edits.ToArray()));
    // Route: connect an ordered parameter chain by synthesizing adjacent-pair Connect edits onto the EditBatch rail.
    public static WireOp Route(Seq<Guid> chain) => new RouteCase(Chain: chain);
    public static WireOp InstallShape(Type shapeType) => new InstallShapeCase(ShapeType: shapeType);
    public static WireOp InstallShape(WireShapeKind kind) {
        ArgumentNullException.ThrowIfNull(argument: kind);
        return new InstallShapeCase(ShapeType: kind.ShapeType);
    }
    public static WireOp Overlay(WireOverlayStyle style, MotionClock? clock = null) => new OverlayCase(Style: style, Clock: clock);
    public static WireOp WirePaintObserve(MotionClock? clock = null) => new WirePaintObserveCase(Clock: clock);
    public static WireOp Diagnostics(params ReadOnlySpan<Guid> seeds) => new DiagnosticsCase(Seeds: toSeq(seeds.ToArray()));

    GrasshopperUiIntent<WireResult> IUiOp<WireResult>.Intent() => Switch(
        queryCase: static q => Wire.Query(query: q.Request),
        selectCase: static s => Wire.Selection(op: s.Op).Map(static delta => (WireResult)new WireResult.SelectionCase(Delta: delta)),
        splitCase: static s => Wire.Split(wire: s.Wire, location: s.Location).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editCase: static e => Wire.Edit(wire: e.Wire, edit: e.Kind, args: e.Args).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editBatchCase: static e => Wire.EditBatch(edits: e.Edits).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        routeCase: static r => Wire.Route(chain: r.Chain).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        installShapeCase: static i => Wire.InstallShape(shapeType: i.ShapeType).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        overlayCase: static o => Wire.Overlay(style: o.Style, clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        wirePaintObserveCase: static o => Wire.WirePaintObserve(clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        diagnosticsCase: static d => Wire.Diagnostics(seeds: d.Seeds).Map(static diagnostics => (WireResult)new WireResult.DiagnosticsResult(Diagnostics: diagnostics)));
}

[SkipUnionOps]
[Union]
public partial record WireResult {
    private WireResult() { }
    public sealed record WiresCase(Seq<WireSnapshot.ConnectedCase> Wires) : WireResult;
    public sealed record WireCase(WireSnapshot Wire) : WireResult;
    public sealed record GraphCase(WireGraph Graph) : WireResult;
    public sealed record MutationCase(Snapshot<DocumentMutationDelta> Delta) : WireResult;
    public sealed record SelectionCase(WireSelectionDelta Delta) : WireResult;
    public sealed record LinearityResult(WireLinearity Linearity) : WireResult;
    public sealed record TopologyResult(GraphTopology Topology) : WireResult;
    public sealed record SortedIdsResult(Seq<Guid> Ids) : WireResult;
    public sealed record PathsResult(Seq<Seq<Guid>> Paths) : WireResult;
    public sealed record IntegrityResult(GraphIntegrity Integrity) : WireResult;
    public sealed record DiagnosticsResult(WireDiagnostics Diagnostics) : WireResult;
    public sealed record InsertCase(WireInsertSnapshot Snapshot) : WireResult;
    public sealed record DrawnCase(WireDrawnSnapshot Snapshot) : WireResult;
    public sealed record SubscriptionCase(Subscription Subscription) : WireResult;
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireGraph(Seq<WireSnapshot.ConnectedCase> Wires, Seq<Guid> Visited);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireLinearity(bool IsLinear, Option<Guid> Start, Option<Guid> End);

[StructLayout(LayoutKind.Auto)]
public readonly record struct GraphIntegrity(Seq<WireSnapshot.ConnectedCase> Dangling, Seq<WireSnapshot.ConnectedCase> Cycles, Seq<Guid> Missing, Seq<Guid> External);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireInsertSnapshot(bool CanInsert, Option<Guid> SourceId, Option<Guid> TargetId);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnEntry(Guid SourceId, Guid TargetId, WireKind Kind, RectangleF Bounds);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDiagnostics(Seq<WireSnapshot.ConnectedCase> Wires, GraphIntegrity Integrity, Option<WireDrawnSnapshot> Drawn);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireOverlayStyle(PaintStyle Style, Option<Func<WireDrawnEntry, PaintStyle>> Select = default) {
    internal PaintStyle For(WireDrawnEntry entry) {
        PaintStyle fallback = Style;
        Option<Func<WireDrawnEntry, PaintStyle>> select = Select;
        return select.Match(Some: pick => pick(arg: entry), None: () => fallback);
    }

    // Per-WireKind override built from a lookup table: the Select picker resolves the entry's kind against the
    // map and falls back to the base style when the kind is unmapped — one table-driven selector, no per-kind
    // branches at the call site.
    public static WireOverlayStyle ByKind(PaintStyle fallback, params (WireKind Kind, PaintStyle Style)[] map) {
        HashMap<WireKind, PaintStyle> table = toHashMap(toSeq(map).Map(static pair => (pair.Kind, pair.Style)));
        return new WireOverlayStyle(
            Style: fallback,
            Select: Some<Func<WireDrawnEntry, PaintStyle>>(entry => table.Find(key: entry.Kind).IfNone(fallback)));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnStamp(
    Guid DocumentHash,
    int Modifications,
    PointF ProjectionCentre,
    float ProjectionZoom,
    RectangleF DrawInnerFrame);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnSnapshot(Seq<WireDrawnEntry> Entries, WireDrawnStamp Stamp, bool FreshFromWirePaint) {
    public int DocumentModifications => Stamp.Modifications;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireSelectionDelta(int Selected, int Deselected);

internal static partial class Wire {
    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) => query.Switch(
        listCase: static list => Listed(kind: list.Kind).Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
        pickCase: static p => Pick(point: p.Point, tolerance: p.Tolerance).Map(static wire => (WireResult)new WireResult.WireCase(Wire: wire)),
        graphCase: static g => Graph(anchor: g.Anchor, direction: g.Direction, maxObjects: g.MaxObjects).Map(static graph => (WireResult)new WireResult.GraphCase(Graph: graph)),
        graphMetricCase: static g => GhUi.Document(run: scope => g.Kind.Run(scope: scope, ids: g.Ids)),
        canInsertCase: static c => CanInsert(objectId: c.ObjectId, location: c.Location).Map(static snap => (WireResult)new WireResult.InsertCase(Snapshot: snap)),
        recentlyDrawnCase: static _ => RecentlyDrawn().Map(static snap => (WireResult)new WireResult.DrawnCase(Snapshot: snap)));

    // Null-guard first (AcceptAll needs a non-null value), then accumulate the three structural faults in
    // parallel — derives-from WireShape, concrete, and a public (PointF, PointF) ctor — so a caller sees every
    // shape violation at once instead of the first short-circuited one.
    internal static GrasshopperUiIntent<Subscription> InstallShape(Type shapeType) =>
        GhUi.Canvas(run: _ =>
            from valid in Optional(shapeType).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(InstallShape)), detail: "shape type is required"))
            from accepted in Op.Of(name: nameof(InstallShape)).AcceptAll(
                value: valid,
                checks:
                [
                    op => guard(typeof(WireShape).IsAssignableFrom(c: valid), (Error)UiFault.InvalidInput(op: op, detail: $"{valid.FullName} does not derive from {typeof(WireShape).FullName}")).ToFin(),
                    op => guard(valid is { IsAbstract: false }, (Error)UiFault.InvalidInput(op: op, detail: $"{valid.FullName} must be concrete")).ToFin(),
                    op => guard(valid.GetConstructor(types: [typeof(PointF), typeof(PointF)]) is not null, (Error)UiFault.InvalidInput(op: op, detail: $"{valid.FullName} must expose a public ({nameof(PointF)}, {nameof(PointF)}) constructor")).ToFin(),
                ])
            from sub in WireShapeInstall.Push(shapeType: accepted)
            select sub);

    internal static GrasshopperUiIntent<Subscription> Overlay(WireOverlayStyle style, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.AfterWires,
                paint: paintScope => WireRepositoryRail.AfterWirePaint(canvas: canvas, scope: paintScope, style: style),
                clock: clock).Run(scope: scope)
            select sub);

    internal static GrasshopperUiIntent<Subscription> WirePaintObserve(MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from pacer in Motion.PacerOption(canvas: canvas, clock: clock)
            from drift in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: _ => Fin.Succ(value: WireDrawnCache.InvalidateOnStampDrift(canvas: canvas)),
                clock: clock,
                adoptedPacer: pacer,
                ownsPacerLifecycle: false).Run(scope: scope)
            from capture in Paint.Hook(
                phase: CanvasPaintPhase.AfterWires,
                paint: _ => WireRepositoryRail.CaptureDrawn(canvas: canvas)
                    .Map(snapshot => WireDrawnCache.Record(canvas: canvas, snapshot: snapshot)),
                clock: clock,
                adoptedPacer: pacer,
                ownsPacerLifecycle: false).Run(scope: scope)
            let hooks = drift | capture
            from kicked in Op.Of(name: nameof(WirePaintObserve)).Attempt(
                body: () => {
                    _ = pacer.ResumeOr(canvas: canvas);
                    return unit;
                },
                what: "wire paint observe wake").MapFail(error => UiFault.ThreadMarshal(detail: error.Message))
            select Subscription.DisposeOnce(Subscription.PaintPacer(
                paintHook: hooks,
                pacerRelease: () => _ = pacer.ReleaseOnce())));

    internal static GrasshopperUiIntent<WireInsertSnapshot> CanInsert(Guid objectId, PointF location) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from objects in scope.NeedObjects()
            from _ in WireRepositoryRail.Require()
            from obj in Optional(objects.Find(instanceId: objectId)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanInsert)), detail: $"object {objectId} not found"))
            from valid in Op.Of(name: nameof(CanInsert)).AcceptPoint(value: location, detail: "non-finite location")
            from snapshot in WireRepositoryRail.CanInsert(canvas: canvas, obj: obj, location: valid)
            select snapshot);

    internal static GrasshopperUiIntent<WireDrawnSnapshot> RecentlyDrawn() =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from _ in WireRepositoryRail.Require()
            from snapshot in WireDrawnCache.Read(canvas: canvas)
            select snapshot);

    internal static GrasshopperUiIntent<WireDiagnostics> Diagnostics(Seq<Guid> seeds) =>
        GhUi.Canvas(run: scope =>
            from objects in scope.NeedObjects()
            from document in scope.NeedDocument()
            from canvas in scope.NeedCanvas()
            let wires = SafeWires(source: AllWireEnds(objects: objects), objects: objects, document: Some(document))
            let drawn = WireDrawnCache.Read(canvas: canvas).ToOption()
            select new WireDiagnostics(Wires: wires, Integrity: IntegrityOf(objects: objects, document: document, seeds: seeds), Drawn: drawn));

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Listed(WireListKind kind) =>
        GhUi.Document(run: scope =>
            from objs in scope.NeedObjects()
            from doc in scope.NeedDocument()
            let projected = SafeWires(source: kind.Source(objects: objs), objects: objs, document: Some(doc)).Filter(kind.Include)
            // Cyclic admits all wires via Include, then intersects the projection with the cycle-membership set
            // computed over the whole connected graph (empty seeds → every connected node is a candidate root).
            select kind.Equals(WireListKind.Cyclic)
                ? IntersectCyclic(projected: projected)
                : projected);

    private static Seq<WireSnapshot.ConnectedCase> IntersectCyclic(Seq<WireSnapshot.ConnectedCase> projected) {
        LanguageExt.HashSet<(Guid Source, Guid Target)> cyclic = toHashSet(
            CycleWires(seeds: Seq<Guid>(), wires: projected).Map(static wire => (wire.Source, wire.Target)));
        return projected.Filter(wire => cyclic.Find(key: (wire.Source, wire.Target)).IsSome);
    }

    internal static GrasshopperUiIntent<WireSnapshot> Pick(PointF point, PickTolerance tolerance) =>
        GhUi.Document(run: scope =>
            from pick in UiRail.PickAt(scope: scope, point: point, source: CoordinateSystem.Content, policy: CanvasPickPolicy.Wires, tolerance: tolerance)
            from objs in scope.NeedObjects()
            from doc in scope.NeedDocument()
            select pick.WireUnderPick.Match(
                Some: wireEnds => SnapshotIn(objects: objs, wire: wireEnds, index: WireIndexCache.ConnectedOf(objects: objs, document: doc)),
                None: () => WireSnapshot.Absent));

    internal static GrasshopperUiIntent<WireSelectionDelta> Selection(WireSelectionOp op) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from objects in scope.NeedObjects()
                from delta in op.Switch(
                    state: objects,
                    selectCase: static (objs, s) => ToggleWire(op: WireSelectionOp.SelectCase.SelfOp, objects: objs, wire: s.Wire, picked: true),
                    deselectCase: static (objs, d) => ToggleWire(op: WireSelectionOp.DeselectCase.SelfOp, objects: objs, wire: d.Wire, picked: false),
                    deselectAllCase: static (objs, _) => DeselectAllWires(op: WireSelectionOp.DeselectAllCase.SelfOp, objects: objs))
                select delta);

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Split(WireSnapshot.ConnectedCase wire, PointF location) =>
        MutateConnectedWire(
            op: Op.Of(name: nameof(Split)),
            wire: wire,
            run: (methods, objs, source, target, actions) =>
                from valid in Op.Of(name: nameof(Split)).AcceptPoint(value: location, detail: "non-finite location")
                from connected in RequireConnected(objects: objs, source: source, target: target, op: Op.Of(name: nameof(Split)))
                from split in Op.Of(name: nameof(Split)).Attempt(
                    body: () => {
                        bool changed = methods.SplitWire(source: source, target: target, name: string.Empty, location: valid, shout: out Shout? shout, listen: out Listen? listen, actions: actions);
                        // Of filters Guid.Empty, so the two created-id chains collapse to one Seq + the changed flag.
                        return DocumentMutationReceipt.Of(changed: changed ? 1 : 0, created: Seq(shout?.InstanceId ?? Guid.Empty, listen?.InstanceId ?? Guid.Empty));
                    },
                    what: "DocumentMethods.SplitWire")
                select split);

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Edit(WireSnapshot.ConnectedCase wire, WireEdit edit, WireEditArgs args) {
        Op op = Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{edit}"));
        return GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunDocumentMutation(
                scope: scope,
                op: op,
                mutate: (methods, objs, actions) => ApplyEditRow(methods: methods, objs: objs, actions: actions, wire: wire, edit: edit, args: args)));
    }

    // Batch edit: one shared ActionList flows through every WireEdit.Apply, so RunMutation commits a single
    // History.Do (N undo entries → 1) while reusing the same row projection as the single-edit rail.
    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> EditBatch(Seq<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> edits) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunDocumentMutation(
                scope: scope,
                op: WireOp.EditBatchCase.SelfOp,
                mutate: (methods, objs, actions) =>
                    edits.TraverseM(entry => ApplyEditRow(methods: methods, objs: objs, actions: actions, wire: entry.Wire, edit: entry.Kind, args: entry.Args))
                        .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, r) => sum + r)).As()));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Route(Seq<Guid> chain) =>
        chain.Count >= 2 && !chain.Exists(static id => id == Guid.Empty)
            ? EditBatch(edits: RouteEdits(chain: chain))
            : GhUi.Document(run: _ => Fin.Fail<Snapshot<DocumentMutationDelta>>(UiFault.InvalidInput(op: WireOp.RouteCase.SelfOp, detail: "wire route requires two or more non-empty parameter ids")));

    // Route: an ordered chain [a,b,c] yields the adjacent-pair Connect edits (a→b, b→c) via Zip(chain.Tail),
    // each carried as a minimal ConnectedCase (Connect's run only reads Source/Target) onto the EditBatch rail.
    private static Seq<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> RouteEdits(Seq<Guid> chain) =>
        chain.Zip(chain.Tail).Map(static pair =>
            (Wire: new WireSnapshot.ConnectedCase(
                Source: pair.First, Target: pair.Second,
                SourceResolved: false, TargetResolved: false, Connected: false, Selected: false),
             Kind: WireEdit.Connect,
             Args: default(WireEditArgs)));

    private static Fin<DocumentMutationReceipt> ApplyEditRow(GhDocumentMethods methods, GhObjectList objs, ActionList actions, WireSnapshot.ConnectedCase wire, WireEdit edit, WireEditArgs args) {
        Op op = Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{edit}"));
        return from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "wire edit is required"))
               let source = objs.FindParameter(instanceId: wire.Source)
               let target = objs.FindParameter(instanceId: wire.Target)
               from _sourceGate in validEdit.RequiresSource
                   ? Optional(source).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"source param {wire.Source} not found")).Map(static _ => unit)
                   : Fin.Succ(unit)
               from _targetGate in validEdit.RequiresTarget
                   ? Optional(target).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"target param {wire.Target} not found")).Map(static _ => unit)
                   : Fin.Succ(unit)
               from _ in validEdit.RequiresConnectedPair
                   ? RequireConnectedPair(objects: objs, source: source, target: target, op: op).Map(static _ => unit)
                   : Fin.Succ(unit)
               from receipt in ApplyResolvedEdit(op: op, methods: methods, objects: objs, source: source, target: target, actions: actions, edit: validEdit, args: args)
               select receipt;
    }

    private static Fin<DocumentMutationReceipt> ApplyResolvedEdit(Op op, GhDocumentMethods methods, GhObjectList objects, IParameter? source, IParameter? target, ActionList actions, WireEdit edit, WireEditArgs args) =>
        from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "wire edit is required"))
        from changed in validEdit.Apply(op: op, methods: methods, objects: objects, source: source, target: target, actions: actions, args: args)
        select DocumentMutationReceipt.Count(changed: changed);

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateConnectedWire(
        Op op,
        WireSnapshot.ConnectedCase wire,
        Func<GhDocumentMethods, GhObjectList, IParameter, IParameter, ActionList, Fin<DocumentMutationReceipt>> run,
        bool requireConnectedPair = true) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunDocumentMutation(
                scope: scope,
                op: op,
                mutate: (methods, objs, actions) =>
                    from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"source param {wire.Source} not found"))
                    from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"target param {wire.Target} not found"))
                    from _ in requireConnectedPair
                        ? RequireConnected(objects: objs, source: source, target: target, op: op).Map(static _ => unit)
                        : Fin.Succ(unit)
                    from result in run(arg1: methods, arg2: objs, arg3: source, arg4: target, arg5: actions)
                    select result));

    // GraphKey discriminates parameter-keyed (SearchUpstream/Downstream — both endpoints must match
    // visited frontier) vs owner-keyed (Connectivity.FindAll — either endpoint owner sweeps in).
    internal static GrasshopperUiIntent<WireGraph> Graph(GraphKey anchor, WireTraversal direction, WireObjectLimit maxObjects) =>
        GhUi.Document(run: scope =>
            from seed in anchor.SeedId.NonEmpty()
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Graph)), detail: "anchor id must be non-empty"))
            from objects in scope.NeedObjects()
            from doc in scope.NeedDocument()
            from result in Op.Of(name: nameof(Graph)).Attempt(
                body: () => GraphOf(objects: objects, document: doc, anchor: anchor, direction: direction, maxObjects: maxObjects),
                what: "graph walk")
            select result);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static GraphIntegrity IntegrityOf(GhObjectList objects, GhDocument document, Seq<Guid> seeds) {
        Seq<WireSnapshot.ConnectedCase> wires = SafeWires(source: AllWireEnds(objects: objects), objects: objects, document: Some(document));
        Seq<WireSnapshot.ConnectedCase> dangling = wires.Filter(static wire => !wire.Connected);
        LanguageExt.HashSet<Guid> known = toHashSet(wires.Bind(static wire => Seq(wire.Source, wire.Target)));
        Seq<Guid> missing = seeds.Filter(id =>
            id == Guid.Empty
            || (objects.Find(instanceId: id) is null && objects.FindParameter(instanceId: id) is null));
        Seq<WireSnapshot.ConnectedCase> cycles = CycleWires(seeds: seeds, wires: wires);
        LanguageExt.HashSet<Guid> missingSet = toHashSet(missing);
        Seq<Guid> external = seeds.Filter(id => id != Guid.Empty && known.Find(key: id).IsNone && missingSet.Find(key: id).IsNone);
        return new GraphIntegrity(Dangling: dangling, Cycles: cycles, Missing: missing, External: external);
    }

    // All bounded upstream->downstream paths. The native FindConnections is unbounded, so this is a pure
    // frontier-fold over Seq<Seq<Guid>>: each step expands the head partial path along node.Out, rejects
    // cyclic extensions by path membership, routes completed extensions (reaching end) into the accumulator,
    // and recurses on the remaining frontier until it empties or the limit is hit.
    internal static Seq<Seq<Guid>> BoundedPaths(Connectivity connectivity, Guid source, Guid target, int limit) =>
        limit <= 0 || !connectivity.Find(source, out ConnectiveObject? start) || !connectivity.Find(target, out ConnectiveObject? end)
            ? Seq<Seq<Guid>>()
            : Expand(connectivity: connectivity, end: end.Id, limit: limit, frontier: Seq(Seq(start.Id)), found: Seq<Seq<Guid>>());

    private static Seq<Seq<Guid>> Expand(Connectivity connectivity, Guid end, int limit, Seq<Seq<Guid>> frontier, Seq<Seq<Guid>> found) =>
        (frontier.Head, found.Count >= limit) switch {
            (_, true) => found,
            ( { IsNone: true }, _) => found,
            ( { IsSome: true, Case: Seq<Guid> path }, _) => Step(connectivity: connectivity, end: end, limit: limit, frontier: frontier.Tail, found: found, path: path),
            _ => found,
        };

    private static Seq<Seq<Guid>> Step(Connectivity connectivity, Guid end, int limit, Seq<Seq<Guid>> frontier, Seq<Seq<Guid>> found, Seq<Guid> path) {
        Guid current = path.Last.IfNone(Guid.Empty);
        Seq<Guid> outward = current != Guid.Empty && connectivity.Find(current, out ConnectiveObject? node)
            ? toSeq(node.Out).Filter(next => !path.Exists(id => id == next))
            : Seq<Guid>();
        Seq<Seq<Guid>> extended = outward.Map(next => path + next);
        Seq<Seq<Guid>> completed = extended.Filter(p => p.Last.IfNone(Guid.Empty) == end);
        Seq<Seq<Guid>> continuing = extended.Filter(p => p.Last.IfNone(Guid.Empty) != end);
        return Expand(
            connectivity: connectivity,
            end: end,
            limit: limit,
            frontier: frontier + continuing,
            found: toSeq((found + completed).Take(count: limit)));
    }

    internal static Seq<WireEnds> AllWireEnds(GhObjectList objects) =>
        toSeq(toSeq(objects.Forwards)
            .Bind(static obj => toSeq(obj.EntireFamily))
            .OfType<IParameter>()
            .Bind(static parameter =>
                toSeq(parameter.Outputs.Forwards).Map(target => new WireEnds(source: parameter.InstanceId, target: target))
                + toSeq(parameter.Inputs.Forwards).Map(source => new WireEnds(source: source, target: parameter.InstanceId)))
            .Distinct());

    private static Seq<WireSnapshot.ConnectedCase> CycleWires(Seq<Guid> seeds, Seq<WireSnapshot.ConnectedCase> wires) {
        Seq<WireSnapshot.ConnectedCase> connected = wires.Filter(static wire => wire.Connected);
        // Build the source->targets adjacency ONCE per call; HasCycle then reuses it across every candidate seed
        // instead of rebuilding the same HashMap per node (O(nodes·edges) → O(edges) build).
        HashMap<Guid, Seq<Guid>> graph = toHashMap(connected
            .GroupBy(static wire => wire.Source)
            .Select(group => (group.Key, toSeq(group.Select(static wire => wire.Target)))));
        Seq<Guid> starts = seeds.Filter(static id => id != Guid.Empty).Distinct();
        Seq<Guid> nodes = (starts.IsEmpty
            ? connected.Bind(static wire => Seq(wire.Source, wire.Target))
            : starts).Distinct();
        LanguageExt.HashSet<Guid> cyclic = toHashSet(nodes.Filter(id => HasCycle(seed: id, graph: graph)));
        return connected.Filter(wire => cyclic.Find(key: wire.Source).IsSome || cyclic.Find(key: wire.Target).IsSome);
    }

    private static bool HasCycle(Guid seed, HashMap<Guid, Seq<Guid>> graph) {
        bool Visit(Guid current, LanguageExt.HashSet<Guid> seen) =>
            graph.Find(current).IfNone(Seq<Guid>()).Exists(next =>
                next == seed || (seen.Find(key: next).IsNone && Visit(current: next, seen: seen.Add(next))));
        return Visit(current: seed, seen: toHashSet(Seq(seed)));
    }

    // Single shared (Source, Target) index per call → SnapshotIn is O(1) membership + 2 FindParameter
    // lookups instead of O(N) endpoint scans. WireIndexCache hoists the index O(W)→O(W per doc edit)
    // when document is supplied.
    private static Seq<WireSnapshot.ConnectedCase> SafeWires(IEnumerable<WireEnds>? source, GhObjectList objects, Option<GhDocument> document = default) {
        LanguageExt.HashSet<(Guid Source, Guid Target)> index = document switch {
            { IsSome: true, Case: GhDocument doc } => WireIndexCache.ConnectedOf(objects: objects, document: doc),
            _ => WireIndexCache.BuildConnected(objects: objects),
        };
        return Optional(source)
            .Map(wires => toSeq(wires).Map(wire => SnapshotIn(objects: objects, wire: wire, index: index)))
            .IfNone(Seq<WireSnapshot.ConnectedCase>());
    }

    internal static WireSnapshot.ConnectedCase SnapshotConnected(GhObjectList objects, WireEnds wire) =>
        SnapshotIn(objects: objects, wire: wire, index: WireIndexCache.BuildConnected(objects: objects));

    // Batch snapshot: build the document-stamped connected index ONCE, then Map SnapshotIn over the batch
    // (O(W+N) vs O(W·N) when per-wire SnapshotConnected rebuilds the index each call).
    internal static Seq<WireSnapshot.ConnectedCase> SnapshotConnectedBatch(GhObjectList objects, GhDocument document, IEnumerable<WireEnds> wires) {
        LanguageExt.HashSet<(Guid Source, Guid Target)> index = WireIndexCache.ConnectedOf(objects: objects, document: document);
        return toSeq(wires).Map(wire => SnapshotIn(objects: objects, wire: wire, index: index));
    }

    // O(degree) via IndexOf; avoids O(N) HashSet rebuild for single-wire guards. Bidirectional to match
    // GH2 Connections.IsConnected: Disconnect detaches both half-edges, so a one-sided test reports a
    // half-torn wire as still connected — breaks Require/Toggle/Count/WireSnapshot.Connected.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsConnected(GhObjectList objects, WireEnds wire) =>
        wire.Source != Guid.Empty
            && wire.Target != Guid.Empty
            && objects.FindParameter(instanceId: wire.Source) is IParameter source
            && objects.FindParameter(instanceId: wire.Target) is IParameter target
            && source.Outputs.IndexOf(parameter: wire.Target) >= 0
            && target.Inputs.IndexOf(parameter: wire.Source) >= 0;

    private static WireSnapshot.ConnectedCase SnapshotIn(GhObjectList objects, WireEnds wire, LanguageExt.HashSet<(Guid Source, Guid Target)> index) {
        IParameter? source = objects.FindParameter(instanceId: wire.Source);
        IParameter? target = objects.FindParameter(instanceId: wire.Target);
        bool connected = index.Find(key: (wire.Source, wire.Target)).IsSome && IsConnected(objects: objects, wire: wire);
        return new WireSnapshot.ConnectedCase(
            Source: wire.Source, Target: wire.Target,
            SourceResolved: source is not null, TargetResolved: target is not null,
            Connected: connected,
            Selected: connected && objects.IsWireSelected(wire: wire));
    }

    internal static Fin<Unit> RequireConnected(GhObjectList objects, IParameter source, IParameter target, Op op) =>
        guard(IsConnected(objects: objects, wire: new WireEnds(source: source.InstanceId, target: target.InstanceId)), (Error)UiFault.MutationRejected(op: op, detail: "wire is not currently connected")).ToFin();

    private static Fin<Unit> RequireConnectedPair(GhObjectList objects, IParameter? source, IParameter? target, Op op) =>
        source is IParameter src && target is IParameter dst
            ? RequireConnected(objects: objects, source: src, target: dst, op: op)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: "connected wire pair requires resolved source and target"));

    // Native WireEnds ctor throws ArgumentNullException on an empty source/target guid; guard both ends before
    // construction so a degenerate snapshot is a typed Fin.Fail rather than a host throw.
    private static Fin<WireEnds> WireEndsOf(Op op, Guid source, Guid target) =>
        guard(source != Guid.Empty && target != Guid.Empty, (Error)UiFault.InvalidInput(op: op, detail: "wire endpoints must both be non-empty"))
            .ToFin()
            .Map(_ => new WireEnds(source: source, target: target));

    private static Fin<WireSelectionDelta> ToggleWire(Op op, GhObjectList objects, WireSnapshot.ConnectedCase wire, bool picked) =>
        from ends in WireEndsOf(op: op, source: wire.Source, target: wire.Target)
        from _live in Optional(IsConnected(objects: objects, wire: ends))
            .Filter(static live => live)
            .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"))
        let before = objects.IsWireSelected(wire: ends)
        from delta in op.Attempt(body: () => picked ? objects.SelectWire(wire: ends) : objects.DeselectWire(wire: ends), what: "wire selection")
            .Map(_ => SelectionDelta(picked: picked, before: before, after: objects.IsWireSelected(wire: ends)))
        select delta;

    private static WireSelectionDelta SelectionDelta(bool picked, bool before, bool after) =>
        (picked, before, after) switch {
            (true, false, true) => new WireSelectionDelta(Selected: 1, Deselected: 0),
            (false, true, false) => new WireSelectionDelta(Selected: 0, Deselected: 1),
            _ => new WireSelectionDelta(Selected: 0, Deselected: 0),
        };

    private static Fin<WireSelectionDelta> DeselectAllWires(Op op, GhObjectList objects) {
        int before = ConnectedSelectedCount(objects: objects);
        return op.Attempt(body: objects.DeselectAllWires, what: "DeselectAllWires")
            .Map(_ => new WireSelectionDelta(Selected: 0, Deselected: Math.Max(val1: 0, val2: before - ConnectedSelectedCount(objects: objects))));
    }

    private static int ConnectedSelectedCount(GhObjectList objects) =>
        toSeq(objects.SelectedWires).Count(wire => IsConnected(objects: objects, wire: wire));

    // Object-count-bounded walk shared by Document.FindCriterion.Graph and GraphOf — GraphKey discriminates
    // parameter- vs owner-keyed traversal, MaxObjects caps the flat walk (Take(n), prefix-monotone — proven by
    // Wire.spec.cs). An empty seed fails; an unresolved (but non-empty) anchor walks to an empty result.
    internal static Fin<Seq<IDocumentObject>> GraphObjects(GhObjectList objects, GraphKey anchor, WireTraversal direction, WireObjectLimit maxObjects) =>
        anchor.SeedId.NonEmpty()
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(GraphObjects)), detail: "anchor id must be non-empty"))
            .Map(_ => WalkBounded(objects: objects, anchor: anchor, direction: direction, maxObjects: maxObjects));

    private static Seq<IDocumentObject> WalkBounded(GhObjectList objects, GraphKey anchor, WireTraversal direction, WireObjectLimit maxObjects) =>
        toSeq(direction.WalkObjects(objects: objects, key: anchor).Take(count: maxObjects.Value)).Distinct();

    private static WireGraph GraphOf(GhObjectList objects, GhDocument document, GraphKey anchor, WireTraversal direction, WireObjectLimit maxObjects) {
        Seq<IDocumentObject> walked = WalkBounded(objects: objects, anchor: anchor, direction: direction, maxObjects: maxObjects);
        Seq<WireSnapshot.ConnectedCase> all = SafeWires(source: AllWireEnds(objects: objects), objects: objects, document: Some(document));
        // GraphKey discriminates keying: parameter-keyed requires BOTH endpoints visited; owner-keyed requires EITHER.
        (Seq<Guid> visited, Func<WireSnapshot.ConnectedCase, bool> connects) = anchor.Switch(
            parameterCase: p => VisitedWith(seed: p.Id, ids: walked.Choose(static obj => Optional(obj as IParameter)).Map(static x => x.InstanceId), bothEndpoints: true),
            ownerCase: o => VisitedWith(seed: o.Id, ids: walked.Map(static obj => obj.InstanceId), bothEndpoints: false));
        return new WireGraph(Wires: all.Filter(connects), Visited: visited);
    }

    private static (Seq<Guid> Visited, Func<WireSnapshot.ConnectedCase, bool> Connects) VisitedWith(Guid seed, Seq<Guid> ids, bool bothEndpoints) {
        Seq<Guid> visited = (Seq(seed) + ids).Distinct();
        LanguageExt.HashSet<Guid> set = toHashSet(visited);
        return (visited, bothEndpoints
            ? w => set.Find(key: w.Source).IsSome && set.Find(key: w.Target).IsSome
            : w => set.Find(key: w.Source).IsSome || set.Find(key: w.Target).IsSome);
    }
}

// --- [BOUNDARY] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class WireRepositoryRail {
    private static readonly Op RailOp = Op.Of(name: nameof(WireRepositoryRail));

    internal readonly record struct WireRepositoryAccess(
        PropertyInfo CacheProperty,
        MethodInfo CanInsert,
        PropertyInfo RecentlyDrawn,
        PropertyInfo InnerFrame,
        FieldInfo Shape,
        FieldInfo Source,
        FieldInfo Target,
        FieldInfo Kind);

    internal static readonly Lazy<Fin<WireRepositoryAccess>> Repository = new(Bootstrap);

    internal static Fin<WireRepositoryAccess> Require() => Repository.Value;

    private static Fin<WireRepositoryAccess> Bootstrap() {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Op op = RailOp;
        // One reflective-lookup capsule: the member-name arg derives the failure detail, so every arm (and the
        // field TraverseM) shares a single ToFin construction instead of six hand-written literals.
        Fin<T> Reflect<T>(string memberName, Func<T?> lookup) where T : class =>
            Optional(lookup()).ToFin(Fail: UiFault.MutationRejected(op: op, detail: $"WireRepository member '{memberName}' not found"));
        Fin<FieldInfo> Field(Type dataType, string memberName, Type expected) =>
            Reflect(memberName, () => dataType.GetField(name: memberName, bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                .Bind(field => guard(expected.IsAssignableFrom(c: field.FieldType), (Error)UiFault.MutationRejected(op: op, detail: $"WireData.{memberName} type drifted to {field.FieldType.FullName}")).ToFin().Map(_ => field));
        return from cache in Reflect("WireDrawCache", () => typeof(GhCanvas).GetProperty(name: "WireDrawCache", bindingAttr: instance))
               let repoType = cache.PropertyType
               from wireData in Reflect("WireData", () => repoType.GetNestedType(name: "WireData", bindingAttr: BindingFlags.Public | BindingFlags.NonPublic))
               from canInsert in Reflect("CanInsertObject", () => repoType.GetMethod(
                       name: "CanInsertObject",
                       bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                       binder: null,
                       types: [typeof(IDocumentObject), typeof(PointF), typeof(IParameter).MakeByRefType(), typeof(IParameter).MakeByRefType()],
                       modifiers: null))
               from recent in Reflect("MostRecentlyDrawnWires", () => repoType.GetProperty(name: "MostRecentlyDrawnWires", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
               from innerFrame in Reflect("InnerFrame", () => repoType.GetProperty(name: "InnerFrame", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
               from shape in Field(dataType: wireData, memberName: "Shape", expected: typeof(WireShape))
               from source in Field(dataType: wireData, memberName: "Source", expected: typeof(IParameter))
               from target in Field(dataType: wireData, memberName: "Target", expected: typeof(IParameter))
               from kind in Field(dataType: wireData, memberName: "Kind", expected: typeof(WireKind))
               select new WireRepositoryAccess(
                   CacheProperty: cache,
                   CanInsert: canInsert,
                   RecentlyDrawn: recent,
                   InnerFrame: innerFrame,
                   Shape: shape,
                   Source: source,
                   Target: target,
                   Kind: kind);
    }
    internal static WireDrawnStamp StampOf(GhCanvas canvas, object repo, WireRepositoryAccess access) {
        GhDocument? document = canvas.Document;
        (PointF centre, float zoom) = document?.Projection ?? (PointF.Empty, 1f);
        RectangleF innerFrame = (RectangleF)access.InnerFrame.GetValue(obj: repo)!;
        return new WireDrawnStamp(
            DocumentHash: document?.Hash ?? Guid.Empty,
            Modifications: document?.Modifications ?? 0,
            ProjectionCentre: centre,
            ProjectionZoom: zoom,
            DrawInnerFrame: innerFrame);
    }

    // One repo-resolution capsule: every repository read threads through (access, repo) so the per-frame
    // CacheProperty.GetValue happens once rather than per-method on the AfterWires hot path.
    private static Fin<T> WithRepo<T>(GhCanvas canvas, Func<WireRepositoryAccess, object, Fin<T>> body) =>
        from access in Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: "wire repository is null"))
        from result in body(arg1: access, arg2: repo)
        select result;

    internal static Fin<WireInsertSnapshot> CanInsert(GhCanvas canvas, IDocumentObject obj, PointF location) =>
        WithRepo(canvas: canvas, body: (access, repo) => RailOp.Attempt(body: () => {
            object?[] args = [obj, location, null, null];
            bool canInsert = (bool)access.CanInsert.Invoke(obj: repo, parameters: args)!;
            IParameter? source = (IParameter?)args[2];
            IParameter? target = (IParameter?)args[3];
            return new WireInsertSnapshot(
                CanInsert: canInsert,
                SourceId: (source?.InstanceId).NonEmpty(),
                TargetId: (target?.InstanceId).NonEmpty());
        }, what: nameof(CanInsert)));

    internal static Fin<WireDrawnSnapshot> CaptureDrawn(GhCanvas canvas) =>
        WithRepo(canvas: canvas, body: (access, repo) => CaptureDrawnWith(canvas: canvas, access: access, repo: repo));

    private static Fin<WireDrawnSnapshot> CaptureDrawnWith(GhCanvas canvas, WireRepositoryAccess access, object repo) =>
        RailOp.Attempt(body: () => {
            IEnumerable<object> wires = ((System.Collections.IEnumerable)access.RecentlyDrawn.GetValue(obj: repo)!).Cast<object>();
            return new WireDrawnSnapshot(
                Entries: toSeq(wires).Map(wire => MapWireData(wire: wire, access: access)).ToSeq(),
                Stamp: StampOf(canvas: canvas, repo: repo, access: access),
                FreshFromWirePaint: true);
        }, what: nameof(CaptureDrawn));

    internal static Fin<Unit> AfterWirePaint(GhCanvas canvas, PaintScope scope, WireOverlayStyle style) =>
        WithRepo(canvas: canvas, body: (access, repo) =>
            from snapshot in CaptureDrawnWith(canvas: canvas, access: access, repo: repo)
            from _ in Fin.Succ(value: WireDrawnCache.Record(canvas: canvas, snapshot: snapshot))
            from __ in DrawOverlayWith(access: access, repo: repo, scope: scope, style: style)
            select unit);

    internal static Fin<Unit> DrawOverlay(GhCanvas canvas, PaintScope scope, WireOverlayStyle style) =>
        WithRepo(canvas: canvas, body: (access, repo) => DrawOverlayWith(access: access, repo: repo, scope: scope, style: style));

    private static Fin<Unit> DrawOverlayWith(WireRepositoryAccess access, object repo, PaintScope scope, WireOverlayStyle style) =>
        RailOp.Attempt(body: () => {
            Graphics graphics = scope.Graphics.Content;
            _ = toSeq(((System.Collections.IEnumerable)access.RecentlyDrawn.GetValue(obj: repo)!).Cast<object>()).Iter(wire =>
                Op.Side(() => {
                    WireShape shape = (WireShape)access.Shape.GetValue(wire)!;
                    using Pen pen = style.For(entry: MapWireData(wire: wire, access: access)).Pen();
                    shape.Draw(graphics: graphics, edge: pen);
                }));
            return unit;
        }, what: nameof(DrawOverlay));

    private static WireDrawnEntry MapWireData(object wire, WireRepositoryAccess access) {
        IParameter source = (IParameter)access.Source.GetValue(obj: wire)!;
        IParameter target = (IParameter)access.Target.GetValue(obj: wire)!;
        WireKind kind = (WireKind)access.Kind.GetValue(obj: wire)!;
        return new WireDrawnEntry(
            SourceId: source.InstanceId,
            TargetId: target.InstanceId,
            Kind: kind,
            Bounds: RectangleF.Union(rect1: source.Attributes.AggregateBounds, rect2: target.Attributes.AggregateBounds));
    }
}

file static class WireShapeInstall {
    private readonly record struct Entry(Guid Token, Type Shape);
    private static readonly Type DefaultShape = typeof(WireShapeDefault);
    private static readonly Atom<Seq<Entry>> Stack = Atom(value: Seq<Entry>());

    internal static Fin<Subscription> Push(Type shapeType) =>
        Op.Of(name: nameof(WireShapeInstall)).Attempt(body: () => {
            Guid token = Guid.NewGuid();
            WireShape.ShapeType = shapeType;
            _ = Stack.Swap(stack => stack + new Entry(Token: token, Shape: shapeType));
            return token;
        }, what: nameof(WireShapeInstall))
        .Bind(token => Subscription.Bind(attach: static () => { }, detach: () => Pop(token: token), marshalToUi: true, detachOnce: true));

    // Guard the throwing static setter (same as Push) so a UI-thread detach throw cannot silently corrupt the
    // static via the swallowing InvokeOnUiThread.
    private static void Pop(Guid token) {
        Seq<Entry> after = Stack.Swap(stack => stack.Filter(entry => entry.Token != token).ToSeq());
        Type restore = after.Last.Map(static entry => entry.Shape).IfNone(DefaultShape);
        _ = Op.Of(name: nameof(WireShapeInstall)).Attempt(
            body: () => { WireShape.ShapeType = restore; return unit; },
            what: nameof(Pop)).Ignore();
    }
}

// --- [CACHE] ------------------------------------------------------------------------------
// Shared MRU-bounded cache: Order tracks recency (tail = most recent), Map holds entries; Record promotes the
// key and evicts the oldest past capacity. WireDrawnCache and WireIndexCache parameterize key/value over it.
file sealed class MruCache<TKey, TVal>(int capacity = 8) where TKey : notnull {
    private readonly Atom<(Seq<TKey> Order, HashMap<TKey, TVal> Map)> cell =
        Atom(value: (Order: Seq<TKey>(), Map: HashMap<TKey, TVal>()));

    internal Option<TVal> Find(TKey key) => cell.Value.Map.Find(key: key);

    // Stamp-fresh read-through: return the cached value when present and fresh, otherwise build, record (which
    // promotes recency + evicts), and return the rebuilt value. WireIndexCache routes through here;
    // WireDrawnCache deliberately does not (its miss path is a typed Fin.Fail, never a silent rebuild).
    internal TVal GetOrAdd(TKey key, Func<TVal, bool> fresh, Func<TVal> build) =>
        Find(key: key).Filter(fresh).Match(
            Some: static hit => hit,
            None: () => Rebuild(key: key, build: build));

    private TVal Rebuild(TKey key, Func<TVal> build) {
        TVal rebuilt = build();
        _ = Record(key: key, value: rebuilt);
        return rebuilt;
    }

    internal Unit Record(TKey key, TVal value) {
        int cap = capacity;
        _ = cell.Swap(state => {
            Seq<TKey> merged = state.Order.Filter(k => !EqualityComparer<TKey>.Default.Equals(x: k, y: key)) + Seq(key);
            int skip = merged.Count > cap ? merged.Count - cap : 0;
            Seq<TKey> promoted = toSeq(merged.Skip(count: skip));
            LanguageExt.HashSet<TKey> keep = toHashSet(promoted);
            return (Order: promoted, Map: state.Map.AddOrUpdate(key: key, value: value).Filter((k, _) => keep.Find(key: k).IsSome));
        });
        return unit;
    }

    internal Unit Invalidate(TKey key) {
        _ = cell.Swap(state => (Order: state.Order.Filter(k => !EqualityComparer<TKey>.Default.Equals(x: k, y: key)), Map: state.Map.Remove(key: key)));
        return unit;
    }
}

// AfterWires populates; Read matches composite stamp (doc, modifications, projection, inner frame).
file static class WireDrawnCache {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CacheKey(int CanvasId, Guid DocumentHash);
    private readonly record struct Entry(WireDrawnStamp Stamp, WireDrawnSnapshot Snapshot);

    private static readonly MruCache<CacheKey, Entry> Cache = new();

    private static CacheKey KeyOf(GhCanvas canvas, WireDrawnStamp stamp) =>
        new(CanvasId: RuntimeHelpers.GetHashCode(canvas), DocumentHash: stamp.DocumentHash);

    internal static Unit Record(GhCanvas canvas, WireDrawnSnapshot snapshot) =>
        Cache.Record(key: KeyOf(canvas: canvas, stamp: snapshot.Stamp), value: new Entry(Stamp: snapshot.Stamp, Snapshot: snapshot));

    // Repository bootstrap failure is silent — drift invalidation no-ops when reflection rail is absent. The
    // whole chain runs in Option: Fin.ToOption collapses a Fail rail to None and any None short-circuits to
    // unit, so the paint hook never throws (a throw here would tear the AfterWires pipeline).
    internal static Unit InvalidateOnStampDrift(GhCanvas canvas) =>
        (from access in WireRepositoryRail.Repository.Value.ToOption()
         from repo in Optional(access.CacheProperty.GetValue(obj: canvas))
         let current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access)
         let key = KeyOf(canvas: canvas, stamp: current)
         from stale in Cache.Find(key: key).Filter(entry => entry.Stamp != current)
         select Cache.Invalidate(key: key))
        .IfNone(unit);

    internal static Fin<WireDrawnSnapshot> Read(GhCanvas canvas) =>
        from access in WireRepositoryRail.Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(
            op: Op.Of(name: nameof(Read)), detail: "wire repository is null"))
        let current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access)
        let key = KeyOf(canvas: canvas, stamp: current)
        from entry in Cache.Find(key: key)
            .Filter(entry => entry.Stamp == current)
            .ToFin(Fail: UiFault.InvalidInput(
                op: Op.Of(name: nameof(Read)),
                detail: "wire draw snapshot is missing or stale; subscribe WirePaintObserve or Overlay and schedule repaint before query"))
        select entry.Snapshot with { FreshFromWirePaint = false };
}

// Document-keyed (Source,Target) index. Refreshes on Document.Modifications change; MRU-bounded.
file static class WireIndexCache {
    private readonly record struct Entry(int Stamp, LanguageExt.HashSet<(Guid Source, Guid Target)> Connected);
    private static readonly MruCache<Guid, Entry> Cache = new();

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> ConnectedOf(GhObjectList objects, GhDocument document) {
        int stamp = document.Modifications;
        return Cache.GetOrAdd(
            key: document.Hash,
            fresh: entry => entry.Stamp == stamp,
            build: () => new Entry(Stamp: stamp, Connected: BuildConnected(objects: objects))).Connected;
    }

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> BuildConnected(GhObjectList objects) =>
        toHashSet(Wire.AllWireEnds(objects: objects).Map(static w => (w.Source, w.Target)));
}

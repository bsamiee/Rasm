using System.Buffers;
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
using GhDuration = Grasshopper2.UI.Animation.Duration;
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

    // Depth is case identity: parameters lack host immediate search, owners honour depth through connectivity fetchers.
    public static readonly WireTraversal Upstream = new(key: 0, walkObjects: static (objects, key) => Walk(objects, key, up: true, immediate: false));
    public static readonly WireTraversal Downstream = new(key: 1, walkObjects: static (objects, key) => Walk(objects, key, up: false, immediate: false));
    public static readonly WireTraversal Bidirectional = new(key: 2, walkObjects: static (objects, key) => Walk(objects, key, up: true, immediate: false).Concat(Walk(objects, key, up: false, immediate: false)));
    public static readonly WireTraversal ImmediateUpstream = new(key: 3, walkObjects: static (objects, key) => Walk(objects, key, up: true, immediate: true));
    public static readonly WireTraversal ImmediateDownstream = new(key: 4, walkObjects: static (objects, key) => Walk(objects, key, up: false, immediate: true));
    public static readonly WireTraversal Neighbours = new(key: 5, walkObjects: static (objects, key) => Walk(objects, key, up: true, immediate: true).Concat(Walk(objects, key, up: false, immediate: true)));

    [UseDelegateFromConstructor]
    internal partial IEnumerable<IDocumentObject> WalkObjects(GhObjectList objects, GraphKey key);

    private static IEnumerable<IDocumentObject> Walk(GhObjectList objects, GraphKey key, bool up, bool immediate) =>
        key.Switch(
            state: (objects, up, immediate),
            parameterCase: static (s, p) => Optional(s.objects.FindParameter(instanceId: p.Id))
                .Map(param => s.immediate
                    ? WalkImmediateParameter(objects: s.objects, parameter: param, up: s.up)
                    : s.up ? s.objects.SearchUpstream(parameter: param) : s.objects.SearchDownstream(parameter: param))
                .IfNone(noneValue: []),
            ownerCase: static (s, o) => OwnerWalk(connectivity: s.objects.Connectivity, id: o.Id, up: s.up, immediate: s.immediate)
                .Select(co => s.objects.Find(instanceId: co.Id))
                .OfType<IDocumentObject>());

    // The (up, immediate) quadrant selects the connectivity fetcher; mirrors RewireMode.Of's discard-last idiom.
    private static IEnumerable<ConnectiveObject> OwnerWalk(Connectivity connectivity, Guid id, bool up, bool immediate) =>
        (up, immediate) switch {
            (true, true) => connectivity.FindImmediateInputs(id),
            (false, true) => connectivity.FindImmediateOutputs(id),
            (true, false) => connectivity.FindAllInputs(id),
            _ => connectivity.FindAllOutputs(id),
        };

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
    // Caller-threaded Op.Of($"Wire.{edit}") preserves case provenance without literals.
    // ApplyEditRow resolves declared sides before dispatch; disconnect-all verbs require only one endpoint.
    public bool RequiresConnectedPair { get; }
    public bool RequiresSource { get; }
    public bool RequiresTarget { get; }

    // Connect verbs require a valid pair, not an already-live pair; native Connect owns compatibility/no-op status.
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

    // Empty omissions would disconnect all inputs; reject so total clearance must use DisconnectInputs.
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

    // One mutation scaffold: native bool/int results project through an interpreter into Fin<int> row counts.
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

    // Each (inputs, clearSource) quadrant fuses the native call name and its invocation; Of resolves the row and Run dispatches once.
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

    // Connectivity.IsLinear reads array[0] on empty subgraphs (decompiled); empty projects to non-linear instead of host throw.
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
    // Relay-collapsed topology drops dangling/simple relays, keeps junctions, then reuses the plain GraphTopology metric.
    // The relay triple keeps WithoutRelays' flags named instead of buried as literals.
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
                    limit: WireObjectLimit.Default.Value)),
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

// Three behavior columns: Source supplies the wire seed, Include filters per wire, and Project post-folds the filtered
// set over whole-graph context. Identity projection is the default; Cyclic intersects the projection with cycle membership.
[SmartEnum<int>]
public sealed partial class WireListKind {
    private delegate IEnumerable<WireEnds>? WireSource(GhObjectList objects);
    private delegate bool WireFilter(WireSnapshot.ConnectedCase wire);
    private delegate Seq<WireSnapshot.ConnectedCase> WireProjection(Seq<WireSnapshot.ConnectedCase> filtered);

    public static readonly WireListKind All = new(key: 0, source: static objects => Wire.AllWireEnds(objects: objects), include: static _ => true, project: static wires => wires);
    public static readonly WireListKind Selected = new(key: 1, source: static objects => objects.SelectedWires, include: static _ => true, project: static wires => wires);
    public static readonly WireListKind Dangling = new(key: 2, source: static objects => Wire.AllWireEnds(objects: objects), include: static wire => !wire.Connected, project: static wires => wires);
    public static readonly WireListKind Connected = new(key: 3, source: static objects => Wire.AllWireEnds(objects: objects), include: static wire => wire.Connected, project: static wires => wires);
    // Cycle membership needs whole-graph context; Include admits all wires and Project intersects with the cycle set.
    public static readonly WireListKind Cyclic = new(key: 4, source: static objects => Wire.AllWireEnds(objects: objects), include: static _ => true, project: Wire.IntersectCyclic);

    [UseDelegateFromConstructor]
    internal partial IEnumerable<WireEnds>? Source(GhObjectList objects);

    [UseDelegateFromConstructor]
    internal partial bool Include(WireSnapshot.ConnectedCase wire);

    [UseDelegateFromConstructor]
    internal partial Seq<WireSnapshot.ConnectedCase> Project(Seq<WireSnapshot.ConnectedCase> filtered);
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
            MaxObjects: maxObjects ?? WireObjectLimit.Default);
    public static WireQuery Graph(GraphKey anchor, WireTraversal? direction, int maxObjects) =>
        new GraphCase(
            Anchor: anchor,
            Direction: direction ?? WireTraversal.Bidirectional,
            MaxObjects: WireObjectLimit.Create(value: maxObjects));
    public static WireQuery GraphMetric(Seq<Guid> ids, GraphMetric kind) => new GraphMetricCase(Ids: ids, Kind: kind);
    public static WireQuery CanInsert(Guid objectId, PointF location) => new CanInsertCase(ObjectId: objectId, Location: location);
    public static WireQuery RecentlyDrawn() => new RecentlyDrawnCase();
}

// Obstacle disposition of a wire style: the only discriminated axis is AutoObstacles. None and Elbow are deliberately
// equal (no auto provider); Avoid auto-installs the live document-object-bounds obstacle provider. The bezier-vs-orthogonal
// distinction is carried by WireStyle.Direct (WireShapeDefault vs WireShapeElbow), not by this row.
[SmartEnum<int>]
public sealed partial class WireRouteKind {
    public bool AutoObstacles { get; }

    public static readonly WireRouteKind None = new(key: 0, autoObstacles: false);
    public static readonly WireRouteKind Elbow = new(key: 1, autoObstacles: false);
    public static readonly WireRouteKind Avoid = new(key: 2, autoObstacles: true);
}

// Elbow bend abscissa: Midpoint bends at the segment midpoint; Split bends at a source->target split fraction.
// Split at ratio 0.5 equals Midpoint.
[SmartEnum<int>]
public sealed partial class BendMode {
    private delegate float CalcFn(PointF source, PointF target, float splitRatio);

    public static readonly BendMode Midpoint = new(key: 0, calculate: static (source, target, _) => (source.X + target.X) * 0.5f);
    public static readonly BendMode Split = new(key: 1, calculate: static (source, target, splitRatio) => source.X + ((target.X - source.X) * splitRatio));

    [UseDelegateFromConstructor]
    internal partial float Calculate(PointF source, PointF target, float splitRatio);
}

// Each row carries its install policy: concrete-shape resolver, obstacle RouteKind, default BendMode. Public shapes
// ride Direct typeof; GH2-internal shapes (WireShapeLinear/WireShapeBiArc) resolve by reflected name. Rows must be the
// cases because the GH2 Activator path admits no ctor args, so only a typeof/reflected-name resolver reaches the shape.
[SmartEnum<string>]
public sealed partial class WireStyle {
    public Type? Direct { get; }
    public string SimpleName { get; }
    public WireRouteKind RouteKind { get; }
    public BendMode Bend { get; }

    public static readonly WireStyle Default = new(key: "default", direct: typeof(WireShapeDefault), simpleName: "", routeKind: WireRouteKind.None, bend: BendMode.Midpoint);
    public static readonly WireStyle Linear = new(key: "linear", direct: null, simpleName: "WireShapeLinear", routeKind: WireRouteKind.None, bend: BendMode.Midpoint);
    public static readonly WireStyle BiArc = new(key: "biarc", direct: null, simpleName: "WireShapeBiArc", routeKind: WireRouteKind.None, bend: BendMode.Midpoint);
    public static readonly WireStyle Elbow = new(key: "elbow", direct: typeof(WireShapeElbow), simpleName: "", routeKind: WireRouteKind.Elbow, bend: BendMode.Midpoint);
    public static readonly WireStyle Avoid = new(key: "avoid", direct: typeof(WireShapeElbow), simpleName: "", routeKind: WireRouteKind.Avoid, bend: BendMode.Split);
    public static readonly WireStyle Custom = new(key: "custom", direct: null, simpleName: "", routeKind: WireRouteKind.None, bend: BendMode.Midpoint);

    // Concrete installable type, else the concrete default fallback so install validation always accepts it.
    public Type ShapeType => Resolve().IfNone(typeof(WireShapeDefault));

    internal Option<Type> Resolve(Option<Type> @override = default) =>
        @override.Filter(IsInstallable)
            | Optional(Direct).Filter(IsInstallable)
            | ReflectByName().Filter(IsInstallable);

    private Option<Type> ReflectByName() =>
        Optional(string.IsNullOrEmpty(SimpleName) ? null : typeof(WireShape).Assembly.GetType(name: $"{typeof(WireShape).Namespace}.{SimpleName}", throwOnError: false));

    private static bool IsInstallable(Type type) =>
        typeof(WireShape).IsAssignableFrom(c: type) && type is { IsAbstract: false };
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
    public sealed partial record InstallShapeCase(WireStyle Style, float CornerRadius, float SplitRatio, WireRoutingProfile Profile, Option<Type> CustomShape = default, Option<Func<RectangleF, Seq<RectangleF>>> ObstaclesOverride = default) : WireOp;
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
    // One polymorphic install: the style row carries shape resolution, RouteKind (Avoid auto-provides obstacles over
    // live document bounds), and the default BendMode. cornerRadius/splitRatio/profile tune geometry; customShape
    // resolves the Custom row; obstaclesOverride replaces the row's auto provider with a caller-supplied obstacle set.
    public static WireOp InstallShape(
        WireStyle style,
        float cornerRadius = 8f,
        float splitRatio = 0.5f,
        WireRoutingProfile? profile = default,
        Option<Type> customShape = default,
        Option<Func<RectangleF, Seq<RectangleF>>> obstaclesOverride = default) =>
        new InstallShapeCase(
            Style: style ?? WireStyle.Default,
            CornerRadius: cornerRadius,
            SplitRatio: splitRatio,
            Profile: profile ?? WireRoutingProfile.Balanced,
            CustomShape: customShape,
            ObstaclesOverride: obstaclesOverride);
    public static WireOp Overlay(WireOverlayStyle style, MotionClock? clock = null) => new OverlayCase(Style: style, Clock: clock);
    public static WireOp WirePaintObserve(MotionClock? clock = null) => new WirePaintObserveCase(Clock: clock);
    public static WireOp Diagnostics(params ReadOnlySpan<Guid> seeds) => new DiagnosticsCase(Seeds: toSeq(seeds.ToArray()));

    // AnimateEndpoint is a distinct motion operation, not a WireOp union case: its SpringHandle<PointF> return escapes
    // the WireResult rail, so it stays a direct factory over Wire.AnimateEndpoint. config carries the spring physics;
    // when omitted, duration seeds SpringConfig.Response so duration-style callers need no physics knowledge.
    public static GrasshopperUiIntent<SpringHandle<PointF>> AnimateEndpoint(
        Guid endpointId, PointF target, GhDuration duration, Action<PointF> sink, Option<SpringConfig> config = default) =>
        Wire.AnimateEndpoint(endpointId: endpointId, target: target, duration: duration, config: config, sink: sink);

    GrasshopperUiIntent<WireResult> IUiOp<WireResult>.Intent() => Switch(
        queryCase: static q => Wire.Query(query: q.Request),
        selectCase: static s => Wire.Selection(op: s.Op).Map(static delta => (WireResult)new WireResult.SelectionCase(Delta: delta)),
        splitCase: static s => Wire.Split(wire: s.Wire, location: s.Location).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editCase: static e => Wire.Edit(wire: e.Wire, edit: e.Kind, args: e.Args).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editBatchCase: static e => Wire.EditBatch(edits: e.Edits).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        routeCase: static r => Wire.Route(chain: r.Chain).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        installShapeCase: static i => Wire.InstallShape(
            style: i.Style,
            cornerRadius: i.CornerRadius,
            splitRatio: i.SplitRatio,
            profile: i.Profile,
            customShape: i.CustomShape,
            obstaclesOverride: i.ObstaclesOverride).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
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
public readonly record struct WireDrawnEntry(
    WireEnds Pair,
    WireKind Kind,
    RectangleF Bounds,
    RectangleF SourceBounds,
    RectangleF TargetBounds,
    float Fade,
    Option<WireRouteTrace> Route) {
    public Guid SourceId => Pair.Source;
    public Guid TargetId => Pair.Target;
}

public enum WireRouteState { Native, Direct, Avoided, Fallback }

// Solver receipt: route disposition plus sparse-Hanan-grid evidence captured for free during Search
// (grid dimensions and the count of states dequeued). GridWidth/GridHeight are 0 on routes that never built a grid.
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireRouteTrace(WireRouteState State, int Segments, int Obstacles, RectangleF Bounds, int GridWidth = 0, int GridHeight = 0, int NodesEvaluated = 0);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDiagnostics(Seq<WireSnapshot.ConnectedCase> Wires, GraphIntegrity Integrity, Option<WireDrawnSnapshot> Drawn);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireOverlayStyle(PaintStyle Style, Option<Func<WireDrawnEntry, PaintStyle>> Select = default) {
    internal PaintStyle For(WireDrawnEntry entry) {
        PaintStyle fallback = Style;
        Option<Func<WireDrawnEntry, PaintStyle>> select = Select;
        return select.Match(Some: pick => pick(arg: entry), None: () => fallback);
    }

    // Per-WireKind overrides use one lookup selector with base-style fallback.
    public static WireOverlayStyle ByKind(PaintStyle fallback, params ReadOnlySpan<(WireKind Kind, PaintStyle Style)> map) {
        // BOUNDARY ADAPTER — ReadOnlySpan cannot be captured by the selector closure; freeze to a HashMap first.
        HashMap<WireKind, PaintStyle> table = toHashMap(toSeq(map.ToArray()).Map(static pair => (pair.Kind, pair.Style)));
        return new WireOverlayStyle(
            Style: fallback,
            Select: Some<Func<WireDrawnEntry, PaintStyle>>(entry => table.Find(key: entry.Kind).IfNone(fallback)));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnStamp(
    Guid DocumentId,
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

// --- [SERVICES] ---------------------------------------------------------------------------
// Orthogonal wire routing: the active BendMode (Midpoint/Split) defines the fallback bend abscissa; the route owner
// resolves an N-segment path once and every visual/pick/window operation folds over that same geometry. The single
// concrete subclass WireShapeElbow renders Midpoint and Split (any ratio) bends.
public abstract class WireShapeOrthogonal(PointF source, PointF target) : WireShape(source, target) {
    private readonly float radius = WireShapeParams.CornerRadius;
    private readonly WireRoutePolicy policy = WireRoutePolicy.Derive(cornerRadius: WireShapeParams.CornerRadius, splitRatio: WireShapeParams.SplitRatio, profile: WireShapeParams.Profile);
    private WireRoutePath? path;
    protected abstract float BendX { get; }

    internal WireRouteTrace Trace => Route.Trace;

    private WireRoutePath Route => path ??= WireRouteSolver.Resolve(source: Source, target: Target, fallbackBendX: BendX, policy: policy);

    public override RectangleF Bounds {
        get {
            ReadOnlySpan<PointF> points = Route.Points.Span;
            float left = points[0].X, right = points[0].X, top = points[0].Y, bottom = points[0].Y;
            for (int i = 1; i < points.Length; i++) {
                PointF p = points[i];
                left = MathF.Min(left, p.X);
                right = MathF.Max(right, p.X);
                top = MathF.Min(top, p.Y);
                bottom = MathF.Max(bottom, p.Y);
            }
            return RectangleF.Inflate(rectangle: new RectangleF(x: left, y: top, width: right - left, height: bottom - top), width: radius, height: radius);
        }
    }

    public override PointF Project(PointF point) {
        ReadOnlySpan<PointF> points = Route.Points.Span;
        PointF best = points[0];
        float bestDistance = float.PositiveInfinity;
        for (int i = 1; i < points.Length; i++) {
            (PointF candidate, float quadrance) = NearOn(points[i - 1], points[i], point);
            if (quadrance < bestDistance) {
                best = candidate;
                bestDistance = quadrance;
            }
        }
        return best;
    }

    public override float DistanceTo(PointF point) {
        ReadOnlySpan<PointF> points = Route.Points.Span;
        float best = float.PositiveInfinity;
        for (int i = 1; i < points.Length; i++) {
            best = MathF.Min(best, NearOn(points[i - 1], points[i], point).Quadrance);
        }
        return MathF.Sqrt(best);
    }

    public override bool Intersects(RectangleF box) {
        ReadOnlySpan<PointF> points = Route.Points.Span;
        for (int i = 0; i < points.Length; i++) {
            if (box.Contains(points[i]) || (i > 0 && Seg(box, points[i - 1], points[i]))) {
                return true;
            }
        }
        return false;
    }

    public override void Draw(Graphics graphics, Pen edge) {
        // BOUNDARY ADAPTER — GH2 supplies non-null paint args; the guard satisfies CA1062 on this public override.
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(edge);
        ReadOnlySpan<PointF> points = Route.Points.Span;
        using GraphicsPath strokePath = new();
        strokePath.StartFigure();
        _ = radius <= policy.Tolerance || points.Length <= 2 ? StraightPolyline(strokePath, points) : RoundedPolyline(strokePath, points, radius);
        graphics.DrawPath(edge, strokePath);
    }

    private static Unit StraightPolyline(GraphicsPath path, ReadOnlySpan<PointF> points) {
        path.MoveTo(points[0].X, points[0].Y);
        for (int i = 1; i < points.Length; i++) {
            path.LineTo(points[i].X, points[i].Y);
        }
        return unit;
    }

    private static Unit RoundedPolyline(GraphicsPath path, ReadOnlySpan<PointF> points, float requestedRadius) {
        // kappa = 4/3 * tan(pi/8): cubic-bezier control-arm fraction approximating a quarter circular arc.
        const float k = 0.5522847498f;
        path.MoveTo(points[0].X, points[0].Y);
        for (int i = 1; i < points.Length - 1; i++) {
            PointF previous = points[i - 1], corner = points[i], next = points[i + 1];
            float before = Distance(a: previous, b: corner);
            float after = Distance(a: corner, b: next);
            float r = MathF.Min(requestedRadius, MathF.Min(before, after) * 0.5f);
            if (r <= 0f || before <= 0f || after <= 0f) {
                path.LineTo(corner.X, corner.Y);
                continue;
            }
            PointF entry = MoveToward(from: corner, to: previous, distance: r);
            PointF exit = MoveToward(from: corner, to: next, distance: r);
            PointF cin = MoveToward(from: entry, to: corner, distance: k * r);
            PointF cout = MoveToward(from: exit, to: corner, distance: k * r);
            path.LineTo(entry.X, entry.Y);
            path.AddBezier(entry, cin, cout, exit);
        }
        PointF last = points[^1];
        path.LineTo(last.X, last.Y);
        return unit;
    }

    private static PointF MoveToward(PointF from, PointF to, float distance) {
        float length = Distance(a: from, b: to);
        return length <= 0f
            ? from
            : new PointF(x: from.X + ((to.X - from.X) * distance / length), y: from.Y + ((to.Y - from.Y) * distance / length));
    }

    private static float Distance(PointF a, PointF b) {
        float dx = b.X - a.X, dy = b.Y - a.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    // Nearest point on segment [a,b] to p plus its squared distance — one pass feeds both Project and DistanceTo.
    private static (PointF Point, float Quadrance) NearOn(PointF a, PointF b, PointF p) {
        float dx = b.X - a.X, dy = b.Y - a.Y, l = (dx * dx) + (dy * dy);
        float t = l < 1e-6f ? 0f : MathF.Max(0f, MathF.Min(1f, (((p.X - a.X) * dx) + ((p.Y - a.Y) * dy)) / l));
        PointF q = new(a.X + (t * dx), a.Y + (t * dy));
        return (Point: q, Quadrance: ((q.X - p.X) * (q.X - p.X)) + ((q.Y - p.Y) * (q.Y - p.Y)));
    }

    // Axis-aligned segment-vs-rect overlap: every elbow segment is horizontal or vertical, so the bbox test is exact.
    private static bool Seg(RectangleF r, PointF a, PointF b) =>
        MathF.Max(a.X, b.X) >= r.Left && MathF.Min(a.X, b.X) <= r.Right
        && MathF.Max(a.Y, b.Y) >= r.Top && MathF.Min(a.Y, b.Y) <= r.Bottom;
}

// Routing tuning policy: each WireRoutePolicy.Derive constant rides a row here, so clearance,
// bend penalty, and query margin are recoverable from the selected row instead of literals scattered in the kernel.
// ClearanceFloor (canvas px), RadiusRatio (clearance from corner radius), BendMultiplier (turn cost vs clearance),
// RadiusQueryRatio/ClearanceQueryRatio (obstacle-scan inflation), Tolerance (axis-merge + cost-compare epsilon).
[SmartEnum<int>]
public sealed partial class WireRoutingProfile {
    public float ClearanceFloor { get; }
    public float RadiusRatio { get; }
    public float BendMultiplier { get; }
    public float RadiusQueryRatio { get; }
    public float ClearanceQueryRatio { get; }
    public float Tolerance { get; }

    public static readonly WireRoutingProfile Conservative = new(key: 0, clearanceFloor: 8f, radiusRatio: 0.75f, bendMultiplier: 4f, radiusQueryRatio: 6f, clearanceQueryRatio: 12f, tolerance: 1e-4f);
    public static readonly WireRoutingProfile Balanced = new(key: 1, clearanceFloor: 4f, radiusRatio: 0.5f, bendMultiplier: 2.5f, radiusQueryRatio: 4f, clearanceQueryRatio: 8f, tolerance: 1e-4f);
    public static readonly WireRoutingProfile Aggressive = new(key: 2, clearanceFloor: 2f, radiusRatio: 0.35f, bendMultiplier: 1.5f, radiusQueryRatio: 3f, clearanceQueryRatio: 6f, tolerance: 1e-4f);
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct WireRoutePolicy(WireRoutingProfile Profile, float CornerRadius, float SplitRatio, float Clearance, float BendPenalty, float QueryMargin, float Tolerance) {
    // Grid-explosion guard, not tunable: the sparse-Hanan grid is capped so a pathological obstacle set cannot
    // blow up the A* state array (states reach MaxGridNodes*3). Every other constant lives on WireRoutingProfile.
    internal const int MaxGridNodes = 4096;

    internal static WireRoutePolicy Derive(float cornerRadius, float splitRatio, WireRoutingProfile profile) {
        float clearance = MathF.Max(profile.ClearanceFloor, cornerRadius * profile.RadiusRatio);
        return new WireRoutePolicy(
            Profile: profile,
            CornerRadius: cornerRadius,
            SplitRatio: splitRatio,
            Clearance: clearance,
            BendPenalty: clearance * profile.BendMultiplier,
            QueryMargin: MathF.Max(cornerRadius * profile.RadiusQueryRatio, clearance * profile.ClearanceQueryRatio),
            Tolerance: profile.Tolerance);
    }
}

internal enum WireRouteDirection { None, Horizontal, Vertical }

[StructLayout(LayoutKind.Auto)]
internal readonly record struct WireRoutePath(ReadOnlyMemory<PointF> Points, WireRouteTrace Trace);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct WireRouteKey(PointF Source, PointF Target, float BendX, float Radius, float Split, int ObstacleCount, int ObstacleHash);

internal static class WireRouteSolver {
    // Per-wire A* path cache keyed on endpoints, bend, radius, split, and obstacle fingerprint; the working set is
    // one entry per visible wire on a busy canvas, so 256 absorbs a full graph without churning across pan/zoom.
    private static readonly BoundedCache<WireRouteKey, WireRoutePath> Cache = new(capacity: 256);

    internal static WireRoutePath Resolve(PointF source, PointF target, float fallbackBendX, WireRoutePolicy policy) {
        PointF[] fallback = Fallback(source: source, target: target, bendX: fallbackBendX);
        RectangleF query = RectangleF.Inflate(rectangle: BoundsOf(fallback), width: policy.QueryMargin, height: policy.QueryMargin);
        RectangleF[] obstacles;
        try {
            obstacles = Normalize(source: source, target: target, region: query, policy: policy, raw: WireObstacles.Obstacles(region: query));
        } catch (Exception error) when (error is InvalidOperationException or NotSupportedException or ArgumentException) {
            return Path(points: fallback, state: WireRouteState.Direct, obstacles: 0, evidence: GridEvidence.Empty);
        }

        int obstacleHash = Hash(obstacles: obstacles);
        WireRouteKey key = new(Source: source, Target: target, BendX: fallbackBendX, Radius: policy.CornerRadius, Split: policy.SplitRatio, ObstacleCount: obstacles.Length, ObstacleHash: obstacleHash);
        return Cache.GetOrAdd(key: key, valueFactory: _ => Build(source: source, target: target, bendX: fallbackBendX, fallback: fallback, obstacles: obstacles, policy: policy));
    }

    private static WireRoutePath Build(PointF source, PointF target, float bendX, PointF[] fallback, RectangleF[] obstacles, WireRoutePolicy policy) =>
        obstacles.Length == 0
            ? Path(points: fallback, state: WireRouteState.Direct, obstacles: 0, evidence: GridEvidence.Empty)
            : Search(source: source, target: target, bendX: bendX, obstacles: obstacles, policy: policy) switch {
                { Path.IsSome: true } solved => Path(points: solved.Path.IfNone(fallback), state: WireRouteState.Avoided, obstacles: obstacles.Length, evidence: solved.Evidence),
                { Evidence: var evidence } => Path(points: fallback, state: WireRouteState.Fallback, obstacles: obstacles.Length, evidence: evidence),
            };

    private static WireRoutePath Path(PointF[] points, WireRouteState state, int obstacles, GridEvidence evidence) {
        PointF[] simplified = SimplifyPath(points: points);
        RectangleF bounds = BoundsOf(points: simplified);
        return new WireRoutePath(
            Points: simplified,
            Trace: new WireRouteTrace(
                State: state, Segments: Math.Max(0, simplified.Length - 1), Obstacles: obstacles, Bounds: bounds,
                GridWidth: evidence.Width, GridHeight: evidence.Height, NodesEvaluated: evidence.NodesEvaluated));
    }

    private static PointF[] Fallback(PointF source, PointF target, float bendX) =>
        SimplifyPath(points: [source, new PointF(bendX, source.Y), new PointF(bendX, target.Y), target]);

    private static RectangleF[] Normalize(PointF source, PointF target, RectangleF region, WireRoutePolicy policy, Seq<RectangleF> raw) =>
        [.. raw.Filter(rect => rect.Intersects(region))
            .Map(rect => RectangleF.Inflate(rectangle: rect, width: policy.Clearance, height: policy.Clearance))
            .Filter(rect => !rect.Contains(source) && !rect.Contains(target))
            .OrderBy(static rect => rect.Left)
            .ThenBy(static rect => rect.Top)
            .ThenBy(static rect => rect.Right)
            .ThenBy(static rect => rect.Bottom)];

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct GridEvidence(int Width, int Height, int NodesEvaluated) {
        public static readonly GridEvidence Empty = new(Width: 0, Height: 0, NodesEvaluated: 0);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct SearchResult(Option<PointF[]> Path, GridEvidence Evidence);

    // MEASURED-KERNEL EXEMPTION — A* over the sparse-Hanan grid uses statement loops and ArrayPool scratch: states
    // reach MaxGridNodes*3 = 12288, too large for stackalloc, and the priority-queue relaxation has no expression form.
    private static SearchResult Search(PointF source, PointF target, float bendX, RectangleF[] obstacles, WireRoutePolicy policy) {
        float[] xs = Axes(policy.Tolerance, source.X, target.X, bendX, obstacles, horizontal: true);
        float[] ys = Axes(policy.Tolerance, source.Y, target.Y, source.Y, obstacles, horizontal: false);
        int width = xs.Length, height = ys.Length, nodes = width * height;
        if (nodes is <= 0 or > WireRoutePolicy.MaxGridNodes) {
            return new SearchResult(Option<PointF[]>.None, GridEvidence.Empty);
        }

        int sourceNode = NodeIndex(xs: xs, ys: ys, point: source, tolerance: policy.Tolerance);
        int targetNode = NodeIndex(xs: xs, ys: ys, point: target, tolerance: policy.Tolerance);
        if (sourceNode < 0 || targetNode < 0 || PointInObstacle(point: source, obstacles: obstacles) || PointInObstacle(point: target, obstacles: obstacles)) {
            return new SearchResult(Option<PointF[]>.None, new GridEvidence(width, height, 0));
        }

        int states = nodes * 3;
        float[] cost = ArrayPool<float>.Shared.Rent(states);
        int[] previous = ArrayPool<int>.Shared.Rent(states);
        bool[] closed = ArrayPool<bool>.Shared.Rent(states);
        try {
            System.Array.Fill(cost, float.PositiveInfinity, 0, states);
            System.Array.Fill(previous, -1, 0, states);
            System.Array.Clear(closed, 0, states);
            PriorityQueue<int, float> queue = new();
            int start = State(node: sourceNode, direction: WireRouteDirection.None);
            cost[start] = 0f;
            queue.Enqueue(start, 0f);
            int found = -1, evaluated = 0;

            while (queue.TryDequeue(out int current, out _)) {
                if (closed[current]) {
                    continue;
                }
                closed[current] = true;
                evaluated++;
                int node = Node(state: current);
                if (node == targetNode) {
                    found = current;
                    break;
                }

                PointF here = PointOf(node: node, xs: xs, ys: ys, width: width);
                int x = node % width, y = node / width;
                WireRouteDirection prior = DirectionOf(state: current);
                // 4-cardinal neighbour expansion: each (dx, dy, direction) row is one grid step.
                for (int n = 0; n < 4; n++) {
                    (int dx, int dy, WireRouteDirection direction) = n switch {
                        0 => (-1, 0, WireRouteDirection.Horizontal),
                        1 => (1, 0, WireRouteDirection.Horizontal),
                        2 => (0, -1, WireRouteDirection.Vertical),
                        _ => (0, 1, WireRouteDirection.Vertical),
                    };
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) {
                        continue;
                    }
                    int next = (ny * width) + nx;
                    PointF there = PointOf(node: next, xs: xs, ys: ys, width: width);
                    if (PointInObstacle(point: there, obstacles: obstacles) || SegmentInObstacle(a: here, b: there, obstacles: obstacles)) {
                        continue;
                    }

                    float bend = prior != WireRouteDirection.None && prior != direction ? policy.BendPenalty : 0f;
                    float candidate = cost[current] + Manhattan(a: here, b: there) + bend;
                    int nextState = State(node: next, direction: direction);
                    if (candidate + policy.Tolerance >= cost[nextState]) {
                        continue;
                    }

                    cost[nextState] = candidate;
                    previous[nextState] = current;
                    queue.Enqueue(nextState, candidate + Manhattan(a: there, b: target));
                }
            }

            GridEvidence evidence = new(width, height, evaluated);
            return new SearchResult(
                found < 0 ? Option<PointF[]>.None : Some(Reconstruct(found: found, previous: previous, xs: xs, ys: ys, width: width)),
                evidence);
        } finally {
            ArrayPool<float>.Shared.Return(cost);
            ArrayPool<int>.Shared.Return(previous);
            ArrayPool<bool>.Shared.Return(closed);
        }
    }

    // Sparse-Hanan axis set: gather the source/target/bend candidates plus each obstacle's two edges on this axis,
    // sort, and merge within tolerance.
    private static float[] Axes(float tolerance, float first, float second, float third, RectangleF[] obstacles, bool horizontal) {
        int raw = 3 + (obstacles.Length * 2);
        float[] buffer = ArrayPool<float>.Shared.Rent(raw);
        try {
            int count = 0;
            for (int i = 0; i < obstacles.Length; i++) {
                RectangleF r = obstacles[i];
                buffer[count++] = horizontal ? r.Left : r.Top;
                buffer[count++] = horizontal ? r.Right : r.Bottom;
            }
            buffer[count++] = first;
            buffer[count++] = second;
            buffer[count++] = third;
            Span<float> span = buffer.AsSpan(0, count);
            span.Sort();
            float[] axes = new float[count];
            int kept = 0;
            for (int i = 0; i < count; i++) {
                float value = span[i];
                if (!float.IsFinite(value)) {
                    continue;
                }
                if (kept == 0 || MathF.Abs(axes[kept - 1] - value) > tolerance) {
                    axes[kept++] = value;
                }
            }
            return axes[..kept];
        } finally {
            ArrayPool<float>.Shared.Return(buffer);
        }
    }

    private static PointF[] Reconstruct(int found, int[] previous, float[] xs, float[] ys, int width) {
        int length = 0;
        for (int cursor = found; cursor >= 0; cursor = previous[cursor]) {
            length++;
        }
        PointF[] points = new PointF[length];
        int i = length - 1;
        for (int cursor = found; cursor >= 0; cursor = previous[cursor]) {
            points[i--] = PointOf(node: Node(state: cursor), xs: xs, ys: ys, width: width);
        }
        return points;
    }

    // Drop coincident points and merge collinear runs; the axis-aligned Collinear test is exact for orthogonal routes.
    internal static PointF[] SimplifyPath(PointF[] points) {
        PointF[] scratch = new PointF[points.Length];
        int kept = 0;
        foreach (PointF point in points) {
            if (kept > 0 && Same(a: scratch[kept - 1], b: point)) {
                continue;
            }
            if (kept >= 2 && Collinear(a: scratch[kept - 2], b: scratch[kept - 1], c: point)) {
                scratch[kept - 1] = point;
            } else {
                scratch[kept++] = point;
            }
        }
        return kept switch {
            0 => [PointF.Empty],
            1 => [scratch[0], scratch[0]],
            _ => scratch[..kept],
        };
    }

    private static RectangleF BoundsOf(PointF[] points) {
        float left = points[0].X, right = points[0].X, top = points[0].Y, bottom = points[0].Y;
        for (int i = 1; i < points.Length; i++) {
            left = MathF.Min(left, points[i].X);
            right = MathF.Max(right, points[i].X);
            top = MathF.Min(top, points[i].Y);
            bottom = MathF.Max(bottom, points[i].Y);
        }
        return new RectangleF(x: left, y: top, width: right - left, height: bottom - top);
    }

    // Strict-inequality interior test: a point exactly on an (already clearance-inflated) obstacle edge is admissible,
    // so a wire may run flush along a node's inflated boundary without the grid rejecting the boundary node.
    private static bool PointInObstacle(PointF point, RectangleF[] obstacles) {
        for (int i = 0; i < obstacles.Length; i++) {
            RectangleF r = obstacles[i];
            if (point.X > r.Left && point.X < r.Right && point.Y > r.Top && point.Y < r.Bottom) {
                return true;
            }
        }
        return false;
    }

    private static bool SegmentInObstacle(PointF a, PointF b, RectangleF[] obstacles) {
        for (int i = 0; i < obstacles.Length; i++) {
            RectangleF r = obstacles[i];
            bool hit = a.Y == b.Y
                ? a.Y > r.Top && a.Y < r.Bottom && MathF.Min(a.X, b.X) < r.Right && MathF.Max(a.X, b.X) > r.Left
                : a.X > r.Left && a.X < r.Right && MathF.Min(a.Y, b.Y) < r.Bottom && MathF.Max(a.Y, b.Y) > r.Top;
            if (hit) {
                return true;
            }
        }
        return false;
    }

    private static int NodeIndex(float[] xs, float[] ys, PointF point, float tolerance) {
        int x = IndexOf(values: xs, value: point.X, tolerance: tolerance);
        int y = IndexOf(values: ys, value: point.Y, tolerance: tolerance);
        return x < 0 || y < 0 ? -1 : (y * xs.Length) + x;
    }

    private static int IndexOf(float[] values, float value, float tolerance) {
        for (int i = 0; i < values.Length; i++) {
            if (MathF.Abs(values[i] - value) <= tolerance) {
                return i;
            }
        }
        return -1;
    }

    private static int State(int node, WireRouteDirection direction) => (node * 3) + (int)direction;
    private static int Node(int state) => state / 3;
    private static WireRouteDirection DirectionOf(int state) => (WireRouteDirection)(state % 3);
    private static PointF PointOf(int node, float[] xs, float[] ys, int width) => new(x: xs[node % width], y: ys[node / width]);
    private static float Manhattan(PointF a, PointF b) => MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);
    private static bool Same(PointF a, PointF b) => a.X == b.X && a.Y == b.Y;
    private static bool Collinear(PointF a, PointF b, PointF c) => (a.X == b.X && b.X == c.X) || (a.Y == b.Y && b.Y == c.Y);

    internal static int Hash(RectangleF[] obstacles) {
        HashCode hash = new();
        foreach (RectangleF rect in obstacles) {
            hash.Add(BitConverter.SingleToInt32Bits(rect.Left));
            hash.Add(BitConverter.SingleToInt32Bits(rect.Top));
            hash.Add(BitConverter.SingleToInt32Bits(rect.Right));
            hash.Add(BitConverter.SingleToInt32Bits(rect.Bottom));
        }
        return hash.ToHashCode();
    }
}

// Avoid-route multi-wire repulsion pass: crossing/overlap minimisation across the whole solved wire SET (interior
// vertices repel along their shared normal up to clearance, endpoints pinned), awaiting the future multi-wire layout consumer.
internal static class WireNudge {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct NudgeKey(int Paths, int Vertices, int Geometry, int Obstacles, int Profile);

    // One adjusted layout per solved-set fingerprint; the working set is the handful of obstacle-routed wire bundles
    // visible at once, so 64 absorbs repeated repaints of the same crowded region without recomputing the repulsion.
    private static readonly BoundedCache<NudgeKey, Seq<ReadOnlyMemory<PointF>>> Cache = new(capacity: 64);

    // Repulsion is bounded so a pathological set cannot blow up the O(paths^2 . segments) pairwise sweep.
    private const int MaxPaths = 64;
    private const int Relaxations = 3;

    internal static Seq<ReadOnlyMemory<PointF>> Nudge(Seq<ReadOnlyMemory<PointF>> paths, RectangleF[] obstacles, WireRoutingProfile profile) =>
        paths.Count is < 2 or > MaxPaths
            ? paths
            : Cache.GetOrAdd(
                key: KeyOf(paths: paths, obstacles: obstacles, profile: profile),
                valueFactory: _ => Relax(paths: paths, profile: profile));

    private static NudgeKey KeyOf(Seq<ReadOnlyMemory<PointF>> paths, RectangleF[] obstacles, WireRoutingProfile profile) {
        HashCode geometry = new();
        int vertices = 0;
        foreach (ReadOnlyMemory<PointF> path in paths) {
            ReadOnlySpan<PointF> span = path.Span;
            vertices += span.Length;
            for (int i = 0; i < span.Length; i++) {
                geometry.Add(BitConverter.SingleToInt32Bits(span[i].X));
                geometry.Add(BitConverter.SingleToInt32Bits(span[i].Y));
            }
        }
        return new NudgeKey(Paths: paths.Count, Vertices: vertices, Geometry: geometry.ToHashCode(), Obstacles: WireRouteSolver.Hash(obstacles: obstacles), Profile: profile.Key);
    }

    // MEASURED-KERNEL EXEMPTION — pairwise elastic repulsion uses statement loops and mutable per-vertex scratch:
    // each relaxation reads every other wire's segments to accumulate one displacement per interior vertex, a
    // stencil with no expression form; scratch is local to the call frame and the result re-enters the immutable Seq.
    private static Seq<ReadOnlyMemory<PointF>> Relax(Seq<ReadOnlyMemory<PointF>> paths, WireRoutingProfile profile) {
        float clearance = MathF.Max(profile.ClearanceFloor, 1f);
        PointF[][] work = [.. paths.Map(static path => path.ToArray())];
        for (int pass = 0; pass < Relaxations; pass++) {
            for (int a = 0; a < work.Length; a++) {
                PointF[] self = work[a];
                for (int v = 1; v < self.Length - 1; v++) {
                    (float pushX, float pushY) = Displacement(work: work, owner: a, vertex: self[v], clearance: clearance);
                    // Preserve orthogonality: a vertex between two H/V segments moves only along the axis its incident
                    // segments allow, so push the dominant component and leave the neighbour-shared coordinate fixed.
                    self[v] = MathF.Abs(pushX) >= MathF.Abs(pushY)
                        ? new PointF(x: self[v].X + pushX, y: self[v].Y)
                        : new PointF(x: self[v].X, y: self[v].Y + pushY);
                }
            }
        }
        return toSeq(work.Select(static path => (ReadOnlyMemory<PointF>)WireRouteSolver.SimplifyPath(points: path)));
    }

    // Accumulated repulsion on one vertex from every OTHER wire's vertices within clearance: a 1/r falloff scaled to
    // the residual gap, summed so a vertex crowded by several neighbours moves out of the whole bundle.
    private static (float X, float Y) Displacement(PointF[][] work, int owner, PointF vertex, float clearance) {
        float accX = 0f, accY = 0f;
        for (int b = 0; b < work.Length; b++) {
            if (b == owner) {
                continue;
            }
            PointF[] other = work[b];
            for (int j = 0; j < other.Length; j++) {
                float dx = vertex.X - other[j].X, dy = vertex.Y - other[j].Y;
                float distance = MathF.Sqrt((dx * dx) + (dy * dy));
                if (distance >= clearance) {
                    continue;
                }
                float gap = clearance - distance;
                (float nx, float ny) = distance > 1e-4f ? (dx / distance, dy / distance) : (0f, 1f);
                accX += nx * gap * 0.5f;
                accY += ny * gap * 0.5f;
            }
        }
        return (accX, accY);
    }
}

// The single concrete orthogonal shape: the install-time BendMode rides WireShapeParams (same thread-static channel as
// CornerRadius/SplitRatio), and BendX dispatches bend.Calculate(source, target, splitRatio): Midpoint bends at the
// segment midpoint, Split at the source->target split fraction. WireShape.Create's Activator admits no ctor args, so the
// per-frame config must arrive on the thread-static channel rather than as a subclass.
public sealed class WireShapeElbow(PointF source, PointF target) : WireShapeOrthogonal(source, target) {
    private readonly BendMode bend = WireShapeParams.Bend;
    private readonly float split = WireShapeParams.SplitRatio;
    protected override float BendX => bend.Calculate(source: Source, target: Target, splitRatio: split);
}

// Obstacle-provider injection point for orthogonal routing. WireRouteSolver implements sparse-Hanan-grid A* with a
// direction-change bend penalty over the rectangles this owner supplies; geometric (bezier) shapes never query it
// (zero cost). The provider is UI-thread-affine; Use installs it symmetrically so a disposed style cannot leak a
// stale closure onto the slot.
public static class WireObstacles {
    [ThreadStatic] internal static Func<RectangleF, Seq<RectangleF>>? Provider;

    public static Seq<RectangleF> Obstacles(RectangleF region) =>
        (Provider ?? (static _ => Seq<RectangleF>()))(region);

    // Live document object bounds as obstacles, excluding the routed pair's owners; bounds re-read each query.
    public static Func<RectangleF, Seq<RectangleF>> ObstaclesFrom(GhObjectList objects, IReadOnlySet<Guid> exclude) =>
        region => {
            LanguageExt.HashSet<Guid> omitted = OwnerExclusions(objects: objects, exclude: exclude);
            return toSeq(objects.Forwards)
            .Filter(o => omitted.Find(key: o.InstanceId).IsNone && o.Attributes.AggregateBounds.Intersects(region))
            .Map(static o => o.Attributes.AggregateBounds);
        };

    private static LanguageExt.HashSet<Guid> OwnerExclusions(GhObjectList objects, IReadOnlySet<Guid> exclude) {
        LanguageExt.HashSet<Guid> omitted = toHashSet(toSeq(exclude));
        foreach (IDocumentObject owner in objects.Forwards) {
            if (exclude.Contains(owner.InstanceId) || (owner is IParameter parameter && exclude.Contains(parameter.InstanceId))) {
                omitted = omitted.Add(owner.InstanceId);
                continue;
            }
            if (owner.EntireFamily.OfType<IParameter>().Any(param => exclude.Contains(param.InstanceId))) {
                omitted = omitted.Add(owner.InstanceId);
            }
        }
        return omitted;
    }

    // Use installs ONLY the obstacle provider (shape/geometry unchanged); WireOp.InstallShape installs shape + geometry + provider together in one selection.
    public static Fin<Subscription> Use(Func<RectangleF, Seq<RectangleF>> provider) {
        Func<RectangleF, Seq<RectangleF>>? prior = null;
        return Subscription.Bind(
            attach: () => { prior = Provider; Provider = provider; },
            detach: () => Provider = prior,
            marshalToUi: true,
            detachOnce: true);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class Wire {
    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) => query.Switch(
        listCase: static list => Listed(kind: list.Kind).Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
        pickCase: static p => Pick(point: p.Point, tolerance: p.Tolerance).Map(static wire => (WireResult)new WireResult.WireCase(Wire: wire)),
        graphCase: static g => Graph(anchor: g.Anchor, direction: g.Direction, maxObjects: g.MaxObjects).Map(static graph => (WireResult)new WireResult.GraphCase(Graph: graph)),
        graphMetricCase: static g => GhUi.Document(run: scope => g.Kind.Run(scope: scope, ids: g.Ids)),
        canInsertCase: static c => CanInsert(objectId: c.ObjectId, location: c.Location).Map(static snap => (WireResult)new WireResult.InsertCase(Snapshot: snap)),
        recentlyDrawnCase: static _ => RecentlyDrawn().Map(static snap => (WireResult)new WireResult.DrawnCase(Snapshot: snap)));

    // One install rail: the style row resolves the concrete shape (customShape overrides the Custom row), structural
    // validation accumulates derive/concrete/ctor/range faults, and the obstacle provider derives from the row's
    // RouteKind — Avoid auto-provides over live document object bounds unless obstaclesOverride supplies a custom set.
    internal static GrasshopperUiIntent<Subscription> InstallShape(
        WireStyle style,
        float cornerRadius,
        float splitRatio,
        WireRoutingProfile profile,
        Option<Type> customShape,
        Option<Func<RectangleF, Seq<RectangleF>>> obstaclesOverride) =>
        GhUi.Canvas(run: scope =>
            from valid in style.Resolve(@override: customShape)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(InstallShape)), detail: $"{style.Key} resolves to no concrete WireShape type"))
                // Resolve already applied IsInstallable (derive-from-WireShape + concrete) to every candidate, so AcceptAll
                // adds only the ctor and geometry-range faults the resolver cannot assert.
            from accepted in Op.Of(name: nameof(InstallShape)).AcceptAll(
                value: valid,
                checks:
                [
                    op => guard(valid.GetConstructor(types: [typeof(PointF), typeof(PointF)]) is not null, (Error)UiFault.InvalidInput(op: op, detail: $"{valid.FullName} must expose a public ({nameof(PointF)}, {nameof(PointF)}) constructor")).ToFin(),
                    op => guard(cornerRadius >= 0f, (Error)UiFault.InvalidInput(op: op, detail: "cornerRadius must be >= 0")).ToFin(),
                    op => guard(splitRatio is >= 0f and <= 1f, (Error)UiFault.InvalidInput(op: op, detail: "splitRatio must be in [0, 1]")).ToFin(),
                ])
            from obstacles in ResolveObstacles(scope: scope, style: style, obstaclesOverride: obstaclesOverride)
            from sub in WireShapeInstall.Push(shapeType: accepted, cornerRadius: cornerRadius, splitRatio: splitRatio, bend: style.Bend, profile: profile, obstacles: obstacles)
            select sub);

    // Obstacle provision derives from the row's RouteKind: an explicit override always wins; otherwise an
    // AutoObstacles row (Avoid) builds the live document-bounds provider (Normalize drops the endpoint owners), and a
    // non-auto row installs no provider (the elbow falls back to its geometric bend).
    private static Fin<Func<RectangleF, Seq<RectangleF>>?> ResolveObstacles(GrasshopperUi.Scope scope, WireStyle style, Option<Func<RectangleF, Seq<RectangleF>>> obstaclesOverride) =>
        obstaclesOverride.Match(
            Some: provider => Fin.Succ<Func<RectangleF, Seq<RectangleF>>?>(provider),
            None: () => style.RouteKind.AutoObstacles
                ? scope.NeedObjects().Map(objects => (Func<RectangleF, Seq<RectangleF>>?)WireObstacles.ObstaclesFrom(objects: objects, exclude: new System.Collections.Generic.HashSet<Guid>()))
                : Fin.Succ<Func<RectangleF, Seq<RectangleF>>?>(value: null));

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

    // Spring the resolved endpoint's live canvas position (AggregateBounds.Center, the same anchor Layout reads) toward
    // target via Motion.Spring<PointF>. config supplies the physics; otherwise duration's seconds seed SpringConfig.Response.
    internal static GrasshopperUiIntent<SpringHandle<PointF>> AnimateEndpoint(
        Guid endpointId, PointF target, GhDuration duration, Option<SpringConfig> config, Action<PointF> sink) =>
        GhUi.Canvas(run: scope =>
            from objects in scope.NeedObjects()
            from endpoint in Optional(objects.FindParameter(instanceId: endpointId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(AnimateEndpoint)), detail: $"endpoint param {endpointId} not found"))
            from validTarget in Op.Of(name: nameof(AnimateEndpoint)).AcceptPoint(value: target, detail: "non-finite endpoint target")
            from springConfig in config.Match(
                Some: static physics => Fin.Succ(value: physics),
                None: () => SpringConfig.Response(response: SpringResponseSeconds(duration: duration), dampingFraction: 0.9f))
            from handle in Motion.Spring(
                start: endpoint.Attributes.AggregateBounds.Center,
                target: validTarget,
                config: springConfig,
                vector: MotionVector.PointF,
                sink: sink,
                initialVelocity: Option<PointF>.None,
                timeSource: null).Run(scope: scope)
            select handle);

    // GhDuration carries no public seconds accessor; the host Animators projection is the one-hop conversion, floored at
    // the float rest epsilon so a zero/instant duration still yields a finite, positive spring response.
    private static float SpringResponseSeconds(GhDuration duration) =>
        MathF.Max((float)Grasshopper2.UI.Animation.Animators.DurationToTimeSpan(duration: duration).TotalSeconds, MotionVector.Float.RestEpsilon);

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
            // Project post-folds over whole-graph context: identity for most rows, cycle-membership intersection for
            // Cyclic (empty seeds → every connected node is a candidate root). Total over the vocabulary, no identity probe.
            select kind.Project(filtered: projected));

    internal static Seq<WireSnapshot.ConnectedCase> IntersectCyclic(Seq<WireSnapshot.ConnectedCase> projected) {
        LanguageExt.HashSet<(Guid Source, Guid Target)> cyclic = toHashSet(
            CycleWires(seeds: Seq<Guid>(), wires: projected).Map(static wire => (wire.Source, wire.Target)));
        return projected.Filter(wire => cyclic.Find(key: (wire.Source, wire.Target)).IsSome);
    }

    internal static GrasshopperUiIntent<WireSnapshot> Pick(PointF point, PickTolerance tolerance) =>
        GhUi.Document(run: scope =>
            from canvas in scope.NeedCanvas()
            from objs in scope.NeedObjects()
            from doc in scope.NeedDocument()
            from native in tolerance.Value > 0f
                ? WireRepositoryRail.WireAt(canvas: canvas, point: point, radius: tolerance.Value)
                : Fin.Succ(Option<WireEnds>.None)
            from picked in native.Match(
                Some: wire => Fin.Succ(Some(wire)),
                None: () => UiRail.PickAt(scope: scope, point: point, source: CoordinateSystem.Content, policy: CanvasPickPolicy.Wires, tolerance: tolerance).Map(pick => pick.WireUnderPick))
            select picked.Match(
                Some: wireEnds => SnapshotIn(objects: objs, wire: wireEnds, index: WireIndexCache.ConnectedOf(objects: objs, document: doc)),
                None: () => WireSnapshot.Absent));

    internal static GrasshopperUiIntent<WireSelectionDelta> Selection(WireSelectionOp op) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from objects in scope.NeedObjects()
                from delta in op.Switch(
                    state: objects,
                    selectCase: static (objs, s) => ToggleWire(op: Op.Of(name: nameof(WireSelectionOp.SelectCase)), objects: objs, wire: s.Wire, picked: true),
                    deselectCase: static (objs, d) => ToggleWire(op: Op.Of(name: nameof(WireSelectionOp.DeselectCase)), objects: objs, wire: d.Wire, picked: false),
                    deselectAllCase: static (objs, _) => DeselectAllWires(op: Op.Of(name: nameof(WireSelectionOp.DeselectAllCase)), objects: objs))
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
    // History.Do (N undo entries → 1) while reusing the same row projection as the single-edit rail. The repaint
    // folds each edit's two endpoint-object regions through RepaintRequest.Batch; the absorption lattice unions
    // ObjectCase only when ids match, so any batch touching two or more distinct owners absorbs to a full Canvas
    // invalidate (the lattice has no multi-object-region union).
    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> EditBatch(Seq<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> edits) =>
        GhUi.Document(
            repaint: RepaintRequest.Batch(requests: edits.Bind(static entry => Seq(RepaintRequest.Object(id: entry.Wire.Source), RepaintRequest.Object(id: entry.Wire.Target)))),
            run: scope => UiRail.RunDocumentMutation(
                scope: scope,
                op: Op.Of(name: nameof(WireOp.EditBatchCase)),
                mutate: (methods, objs, actions) =>
                    edits.TraverseM(entry => ApplyEditRow(methods: methods, objs: objs, actions: actions, wire: entry.Wire, edit: entry.Kind, args: entry.Args))
                        .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, r) => sum + r)).As()));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Route(Seq<Guid> chain) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunDocumentMutation(
                scope: scope,
                op: Op.Of(name: nameof(WireOp.RouteCase)),
                mutate: (methods, objs, actions) => ApplyRouteChain(
                    methods: methods,
                    objects: objs,
                    actions: actions,
                    chain: chain,
                    op: Op.Of(name: nameof(WireOp.RouteCase)))));

    internal static Fin<DocumentMutationReceipt> ApplyRouteChain(GhDocumentMethods methods, GhObjectList objects, ActionList actions, Seq<Guid> chain, Op op) =>
        chain.Count >= 2 && !chain.Exists(static id => id == Guid.Empty)
            ? RouteEdits(chain: chain)
                .TraverseM(entry => ApplyEditRow(methods: methods, objs: objects, actions: actions, wire: entry.Wire, edit: entry.Kind, args: entry.Args))
                .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, r) => sum + r)).As()
            : Fin.Fail<DocumentMutationReceipt>(error: UiFault.InvalidInput(op: op, detail: "wire route requires two or more non-empty parameter ids"));

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
               from changed in validEdit.Apply(op: op, methods: methods, objects: objs, source: source, target: target, actions: actions, args: args)
               select DocumentMutationReceipt.Count(changed: changed);
    }

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

    // Native FindConnections is unbounded; a bounded BFS rejects cyclic extensions and stops at limit.
    internal static Seq<Seq<Guid>> BoundedPaths(Connectivity connectivity, Guid source, Guid target, int limit) {
        // BOUNDARY ADAPTER — mutable Queue/foreach drive bounded BFS path-enumeration; state is local to this call frame.
        if (limit <= 0
            || !connectivity.Find(source, out ConnectiveObject? start)
            || !connectivity.Find(target, out ConnectiveObject? end)) {
            return Seq<Seq<Guid>>();
        }
        Guid endId = end.Id;
        Queue<Seq<Guid>> queue = new();
        queue.Enqueue(item: Seq(start.Id));
        Seq<Seq<Guid>> found = Seq<Seq<Guid>>();
        while (queue.Count > 0 && found.Count < limit) {
            Seq<Guid> path = queue.Dequeue();
            Guid current = path.Last.IfNone(Guid.Empty);
            if (current == Guid.Empty || !connectivity.Find(current, out ConnectiveObject? node)) {
                continue;
            }
            foreach (Guid next in node.Out) {
                if (path.Exists(id => id == next)) {
                    continue;
                }
                Seq<Guid> extended = path + next;
                if (next == endId) {
                    found += extended;
                    if (found.Count >= limit) {
                        break;
                    }
                } else {
                    queue.Enqueue(item: extended);
                }
            }
        }
        return found;
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
        return connected.Filter(wire => cyclic.Find(key: wire.Source).IsSome && cyclic.Find(key: wire.Target).IsSome);
    }

    private static bool HasCycle(Guid seed, HashMap<Guid, Seq<Guid>> graph) {
        bool Visit(Guid current, LanguageExt.HashSet<Guid> seen) =>
            graph.Find(current).IfNone(Seq<Guid>()).Exists(next =>
                next == seed || (seen.Find(key: next).IsNone && Visit(current: next, seen: seen.Add(next))));
        return Visit(current: seed, seen: toHashSet(Seq(seed)));
    }

    // Shared (Source,Target) index makes SnapshotIn O(1); WireIndexCache refreshes O(W) per document edit.
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

    // O(degree) guard stays bidirectional because Disconnect detaches both half-edges.
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

    // Object-count-bounded graph walk: empty seed fails, unresolved non-empty anchor yields empty.
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
            state: walked,
            parameterCase: static (w, p) => VisitedWith(seed: p.Id, ids: w.Choose(static obj => Optional(obj as IParameter)).Map(static x => x.InstanceId), bothEndpoints: true),
            ownerCase: static (w, o) => VisitedWith(seed: o.Id, ids: w.Map(static obj => obj.InstanceId), bothEndpoints: false));
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

// --- [BOUNDARIES] -------------------------------------------------------------------------
[BoundaryAdapter]
internal static class WireRepositoryRail {
    private static readonly Op RailOp = Op.Of(name: nameof(WireRepositoryRail));

    internal readonly record struct WireRepositoryAccess(
        PropertyInfo CacheProperty,
        MethodInfo CanInsert,
        MethodInfo WireAt,
        PropertyInfo RecentlyDrawn,
        PropertyInfo InnerFrame,
        PropertyInfo OuterFrame,
        FieldInfo Shape,
        FieldInfo Source,
        FieldInfo Target,
        FieldInfo Pair,
        FieldInfo Kind,
        FieldInfo Fade);

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
               from wireAt in Reflect("WireAt", () => repoType.GetMethod(
                       name: "WireAt",
                       bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                       binder: null,
                       types: [typeof(PointF), typeof(float), typeof(bool)],
                       modifiers: null))
               from recent in Reflect("MostRecentlyDrawnWires", () => repoType.GetProperty(name: "MostRecentlyDrawnWires", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
               from innerFrame in Reflect("InnerFrame", () => repoType.GetProperty(name: "InnerFrame", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
               from outerFrame in Reflect("OuterFrame", () => repoType.GetProperty(name: "OuterFrame", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
               from shape in Field(dataType: wireData, memberName: "Shape", expected: typeof(WireShape))
               from source in Field(dataType: wireData, memberName: "Source", expected: typeof(IParameter))
               from target in Field(dataType: wireData, memberName: "Target", expected: typeof(IParameter))
               from pair in Field(dataType: wireData, memberName: "Pair", expected: typeof(WireEnds))
               from kind in Field(dataType: wireData, memberName: "Kind", expected: typeof(WireKind))
               from fade in Field(dataType: wireData, memberName: "Fade", expected: typeof(float))
               select new WireRepositoryAccess(
                   CacheProperty: cache,
                   CanInsert: canInsert,
                   WireAt: wireAt,
                   RecentlyDrawn: recent,
                   InnerFrame: innerFrame,
                   OuterFrame: outerFrame,
                   Shape: shape,
                   Source: source,
                   Target: target,
                   Pair: pair,
                   Kind: kind,
                   Fade: fade);
    }
    internal static WireDrawnStamp StampOf(GhCanvas canvas, object repo, WireRepositoryAccess access) {
        GhDocument? document = canvas.Document;
        (PointF centre, float zoom) = document?.Projection ?? (PointF.Empty, 1f);
        RectangleF innerFrame = (RectangleF)access.InnerFrame.GetValue(obj: repo)!;
        return new WireDrawnStamp(
            DocumentId: UiDocumentIdentity.Of(document),
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

    internal static Fin<Option<WireEnds>> WireAt(GhCanvas canvas, PointF point, float radius) =>
        WithRepo(canvas: canvas, body: (access, repo) => RailOp.Attempt(body: () => {
            object? result = access.WireAt.Invoke(obj: repo, parameters: [point, radius, true]);
            return result is WireEnds wire && wire.Source != Guid.Empty && wire.Target != Guid.Empty
                ? Some(wire)
                : Option<WireEnds>.None;
        }, what: "WireRepository.WireAt"));

    internal static Fin<WireDrawnSnapshot> CaptureDrawn(GhCanvas canvas) =>
        WithRepo(canvas: canvas, body: (access, repo) =>
            from wires in ReadWires(access: access, repo: repo)
            from snapshot in CaptureDrawnWith(canvas: canvas, access: access, repo: repo, wires: wires)
            select snapshot);

    // Materialise MostRecentlyDrawnWires ONCE; AfterWirePaint threads the same Seq into capture + overlay.
    private static Fin<Seq<object>> ReadWires(WireRepositoryAccess access, object repo) =>
        RailOp.Attempt(
            body: () => toSeq(((System.Collections.IEnumerable)access.RecentlyDrawn.GetValue(obj: repo)!).Cast<object>()),
            what: "MostRecentlyDrawnWires");

    private static Fin<WireDrawnSnapshot> CaptureDrawnWith(GhCanvas canvas, WireRepositoryAccess access, object repo, Seq<object> wires) =>
        RailOp.Attempt(body: () => new WireDrawnSnapshot(
            Entries: wires.Map(wire => MapWireData(wire: wire, access: access)),
            Stamp: StampOf(canvas: canvas, repo: repo, access: access),
            FreshFromWirePaint: true), what: nameof(CaptureDrawn));

    internal static Fin<Unit> AfterWirePaint(GhCanvas canvas, PaintScope scope, WireOverlayStyle style) =>
        WithRepo(canvas: canvas, body: (access, repo) =>
            from wires in ReadWires(access: access, repo: repo)
            from snapshot in CaptureDrawnWith(canvas: canvas, access: access, repo: repo, wires: wires)
            from _ in Fin.Succ(value: WireDrawnCache.Record(canvas: canvas, snapshot: snapshot))
            from __ in DrawOverlayWith(access: access, scope: scope, style: style, wires: wires)
            select unit);

    internal static Fin<Unit> DrawOverlay(GhCanvas canvas, PaintScope scope, WireOverlayStyle style) =>
        WithRepo(canvas: canvas, body: (access, repo) =>
            from wires in ReadWires(access: access, repo: repo)
            from drawn in DrawOverlayWith(access: access, scope: scope, style: style, wires: wires)
            select drawn);

    private static Fin<Unit> DrawOverlayWith(WireRepositoryAccess access, PaintScope scope, WireOverlayStyle style, Seq<object> wires) =>
        RailOp.Attempt(body: () => {
            Graphics graphics = scope.Graphics.Content;
            _ = wires.Iter(wire =>
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
        WireShape shape = (WireShape)access.Shape.GetValue(obj: wire)!;
        WireEnds pair = (WireEnds)access.Pair.GetValue(obj: wire)!;
        WireKind kind = (WireKind)access.Kind.GetValue(obj: wire)!;
        float fade = (float)access.Fade.GetValue(obj: wire)!;
        RectangleF bounds = shape.Bounds;
        return new WireDrawnEntry(
            Pair: pair.Source != Guid.Empty && pair.Target != Guid.Empty
                ? pair
                : new WireEnds(source: source.InstanceId, target: target.InstanceId),
            Kind: kind,
            Bounds: bounds,
            SourceBounds: source.Attributes.AggregateBounds,
            TargetBounds: target.Attributes.AggregateBounds,
            Fade: fade,
            Route: shape is WireShapeOrthogonal routeShape ? Some(routeShape.Trace) : Option<WireRouteTrace>.None);
    }
}

internal static class WireShapeInstall {
    private readonly record struct Entry(Guid Token, Type Shape, float CornerRadius, float SplitRatio, BendMode Bend, WireRoutingProfile Profile, Func<RectangleF, Seq<RectangleF>>? Provider) {
        // Sole owner of the empty-stack base frame: the geometric default shape with the canonical Midpoint/Balanced pair.
        internal static readonly Entry Base = new(Token: Guid.Empty, Shape: typeof(WireShapeDefault), CornerRadius: 0f, SplitRatio: 0.5f, Bend: BendMode.Midpoint, Profile: WireRoutingProfile.Balanced, Provider: null);
    }

    private static readonly Atom<Seq<Entry>> Stack = Atom(value: Seq<Entry>());

    // Install runs inside a UI-thread-marshalled intent (GrasshopperUi.Use -> OnUiThread), so the resolved per-shape
    // config — geometry, BendMode, routing profile, AND the obstacle provider — lands on the same thread WireShape.Create
    // later reads during paint. WireStyle/BendMode are the install-time selection that Push decomposes onto the statics.
    internal static Fin<Subscription> Push(Type shapeType, float cornerRadius, float splitRatio, BendMode bend, WireRoutingProfile profile, Func<RectangleF, Seq<RectangleF>>? obstacles = null) =>
        Op.Of(name: nameof(WireShapeInstall)).Attempt(body: () => {
            Guid token = Guid.NewGuid();
            WireShapeParams.CornerRadius = cornerRadius;
            WireShapeParams.SplitRatio = splitRatio;
            WireShapeParams.Bend = bend;
            WireShapeParams.Profile = profile;
            WireShape.ShapeType = shapeType;
            WireObstacles.Provider = obstacles;
            _ = Stack.Swap(stack => stack + new Entry(Token: token, Shape: shapeType, CornerRadius: cornerRadius, SplitRatio: splitRatio, Bend: bend, Profile: profile, Provider: obstacles));
            return token;
        }, what: nameof(WireShapeInstall))
        .Bind(token => Subscription.Bind(attach: static () => { }, detach: () => Pop(token: token).ThrowIfFail(), marshalToUi: true, detachOnce: true));

    // Pop returns its Fin so DetachOnUiThread's Protect captures a restore failure and ObserveDetach folds it into the
    // observable HandlerFaultSink. The lone fallible foreign write (WireShape.ShapeType) precedes the Stack.Swap commit,
    // so a setter throw leaves the stack untouched and the entry still owns its slot for a later retry.
    private static Fin<Unit> Pop(Guid token) =>
        Op.Of(name: nameof(WireShapeInstall)).Attempt(body: () => {
            Entry prior = Stack.Value.Filter(entry => entry.Token != token).ToSeq().Last.IfNone(Entry.Base);
            WireShape.ShapeType = prior.Shape;
            _ = Stack.Swap(stack => stack.Filter(entry => entry.Token != token).ToSeq());
            WireShapeParams.CornerRadius = prior.CornerRadius;
            WireShapeParams.SplitRatio = prior.SplitRatio;
            WireShapeParams.Bend = prior.Bend;
            WireShapeParams.Profile = prior.Profile;
            WireObstacles.Provider = prior.Provider;
            return unit;
        }, what: nameof(Pop));
}

// UI-thread-affine per-shape config: Push writes these on the marshalled install thread; WireShapeOrthogonal captures
// them at construction (WireShape.Create's Activator path admits no extra ctor args). Bend/Profile default to the
// Midpoint/Balanced rows on a thread that never ran Push, matching the geometric default shape.
file static class WireShapeParams {
    [ThreadStatic] internal static float CornerRadius;
    [ThreadStatic] internal static float SplitRatio;

    [field: ThreadStatic]
    internal static BendMode Bend {
        get => field ?? BendMode.Midpoint;
        set;
    }

    [field: ThreadStatic]
    internal static WireRoutingProfile Profile {
        get => field ?? WireRoutingProfile.Balanced;
        set;
    }
}

// --- [TABLES] -----------------------------------------------------------------------------
// Shared MRU-bounded cache: Order tracks recency (tail = most recent), Map holds entries; Record promotes the
// key and evicts the oldest past capacity. WireDrawnCache and WireIndexCache parameterize key/value over it.
// AfterWires populates; Read matches composite stamp (doc, modifications, projection, inner frame).
internal static class WireDrawnCache {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CacheKey(int CanvasId, WireDrawnStamp Stamp);
    private readonly record struct Entry(WireDrawnSnapshot Snapshot);

    private static readonly BoundedCache<CacheKey, Entry> Cache = new(capacity: 8);

    private static CacheKey KeyOf(GhCanvas canvas, WireDrawnStamp stamp) =>
        new(CanvasId: RuntimeHelpers.GetHashCode(canvas), Stamp: stamp);

    internal static Unit Record(GhCanvas canvas, WireDrawnSnapshot snapshot) =>
        Cache.Record(key: KeyOf(canvas: canvas, stamp: snapshot.Stamp), value: new Entry(Snapshot: snapshot));

    // Reflection bootstrap failures no-op drift invalidation; the Option chain keeps AfterWires from throwing.
    internal static Unit InvalidateOnStampDrift(GhCanvas canvas) =>
        (from access in WireRepositoryRail.Repository.Value.ToOption()
         from repo in Optional(access.CacheProperty.GetValue(obj: canvas))
         let current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access)
         let key = KeyOf(canvas: canvas, stamp: current)
         from stale in Cache.Find(key: key)
         select Cache.Invalidate(key: key))
        .IfNone(unit);

    internal static Fin<WireDrawnSnapshot> Read(GhCanvas canvas) =>
        from access in WireRepositoryRail.Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(
            op: Op.Of(name: nameof(Read)), detail: "wire repository is null"))
        let current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access)
        let key = KeyOf(canvas: canvas, stamp: current)
        from entry in Cache.Find(key: key)
            .ToFin(Fail: UiFault.InvalidInput(
                op: Op.Of(name: nameof(Read)),
                detail: "wire draw snapshot is missing or stale; subscribe WirePaintObserve or Overlay and schedule repaint before query"))
        select entry.Snapshot with { FreshFromWirePaint = false };
}

// Document-keyed (Source,Target) index. Refreshes on Document.Modifications change; MRU-bounded.
file static class WireIndexCache {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CacheKey(Guid DocumentId, int Modifications);
    private static readonly BoundedCache<CacheKey, LanguageExt.HashSet<(Guid Source, Guid Target)>> Cache = new(capacity: 8);

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> ConnectedOf(GhObjectList objects, GhDocument document) {
        return Cache.GetOrAdd(
            key: new CacheKey(DocumentId: UiDocumentIdentity.Of(document), Modifications: document.Modifications),
            valueFactory: _ => BuildConnected(objects: objects));
    }

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> BuildConnected(GhObjectList objects) =>
        toHashSet(Wire.AllWireEnds(objects: objects).Map(static w => (w.Source, w.Target)));
}

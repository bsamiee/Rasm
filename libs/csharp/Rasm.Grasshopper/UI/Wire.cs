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

// Optional per-edit arguments: connection endpoint indices (ConnectAt) and the omit-set for the
// DisconnectAll*Except verbs. Default is the zero/empty carrier the index-free verbs ignore.
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireEditArgs(int SourceIndex = 0, int TargetIndex = 0, Seq<Guid> Omit = default);

[SmartEnum<int>]
public sealed partial class WireEdit {
    // The op is threaded in by the caller as Op.Of($"Wire.{edit}") so each item names itself from its case
    // identity — ToString() == the former literal, so provenance is unchanged and the 8 literals are gone.
    private delegate Fin<int> WireEditRun(Op op, GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions, WireEditArgs args);

    // Disconnect-all verbs operate on one endpoint, so they never require a live source<->target pair.
    public bool RequiresConnectedPair { get; }

    // Connect/ConnectAt CREATE a connection, so they require a valid source/target pair but NOT an
    // already-live one — RequiresConnectedPair is false (MutateConnectedWire's RequireConnected pre-guard
    // would otherwise reject the very pair being connected); native Connect owns compatibility and no-op status.
    public static readonly WireEdit Connect = new(
        key: 0,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, _) =>
            NativeConnect(op: op, run: () => Connections.Connect(source: source, target: target, undo: actions)));

    // Disconnect/Delete operate on an existing wire — MutateConnectedWire's RequiresConnectedPair guard owns
    // the is-connected check, so no in-verb re-guard.
    public static readonly WireEdit Disconnect = new(
        key: 1,
        requiresConnectedPair: true,
        apply: static (op, _, _, source, target, actions, _) =>
            Native(op: op, name: "Connections.Disconnect", run: () => Connections.Disconnect(source: source, target: target, undo: actions)));

    public static readonly WireEdit Delete = new(
        key: 2,
        requiresConnectedPair: true,
        apply: static (op, methods, _, source, target, actions, _) =>
            NativeCount(op: op, name: "DocumentMethods.DeleteObjects", run: () => methods.DeleteObjects(objects: [], wires: [new WireEnds(source: source.InstanceId, target: target.InstanceId)], actions: actions)));

    public static readonly WireEdit DisconnectInputs = new(
        key: 3,
        requiresConnectedPair: false,
        apply: static (op, _, _, _, target, actions, _) =>
            NativeCount(op: op, name: "Connections.DisconnectAllInputs", run: () => Connections.DisconnectAllInputs(target: target, undo: actions)));

    public static readonly WireEdit DisconnectOutputs = new(
        key: 4,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, _, actions, _) =>
            NativeCount(op: op, name: "Connections.DisconnectAllOutputs", run: () => Connections.DisconnectAllOutputs(source: source, undo: actions)));

    public static readonly WireEdit ConnectAt = new(
        key: 5,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, args) =>
            NativeConnect(op: op, run: () => Connections.Connect(source: source, target: target, indexAtSource: args.SourceIndex, indexAtTarget: args.TargetIndex, undo: actions)));

    public static readonly WireEdit DisconnectInputsExcept = new(
        key: 6,
        requiresConnectedPair: false,
        apply: static (op, _, _, _, target, actions, args) =>
            NativeCount(op: op, name: "Connections.DisconnectAllInputsExcept", run: () => Connections.DisconnectAllInputsExcept(target: target, omissions: [.. args.Omit], undo: actions)));

    public static readonly WireEdit DisconnectOutputsExcept = new(
        key: 7,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, _, actions, args) =>
            NativeCount(op: op, name: "Connections.DisconnectAllOutputsExcept", run: () => Connections.DisconnectAllOutputsExcept(source: source, omissions: [.. args.Omit], undo: actions)));
    public static readonly WireEdit CopyInputs = new(
        key: 8,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, _) =>
            RewireInputs(op: op, source: source, target: target, actions: actions, clearSource: false));
    public static readonly WireEdit MigrateInputs = new(
        key: 9,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, _) =>
            RewireInputs(op: op, source: source, target: target, actions: actions, clearSource: true));
    public static readonly WireEdit CopyOutputs = new(
        key: 10,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, _) =>
            RewireOutputs(op: op, source: source, target: target, actions: actions, clearSource: false));
    public static readonly WireEdit MigrateOutputs = new(
        key: 11,
        requiresConnectedPair: false,
        apply: static (op, _, _, source, target, actions, _) =>
            RewireOutputs(op: op, source: source, target: target, actions: actions, clearSource: true));

    [UseDelegateFromConstructor]
    internal partial Fin<int> Apply(Op op, GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions, WireEditArgs args);

    private static Fin<int> NativeConnect(Op op, Func<bool> run) =>
        op.Attempt(body: run, what: "Connections.Connect")
            .Map(static changed => changed ? 1 : 0);

    private static Fin<int> Native(Op op, string name, Func<bool> run) =>
        op.Attempt(body: run, what: name)
            .Bind(changed => changed switch {
                true => Fin.Succ(value: 1),
                false => Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: $"{name} returned false")),
            });

    private static Fin<int> NativeCount(Op op, string name, Func<int> run) =>
        op.Attempt(body: run, what: name)
            .Bind(count => count switch {
                >= 0 => Fin.Succ(value: count),
                _ => Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"{name} returned {count}"))),
            });

    private static Fin<int> RewireInputs(Op op, IParameter source, IParameter target, ActionList actions, bool clearSource) =>
        NativeCount(
            op: op,
            name: clearSource ? "Connections.MigrateAllInputs" : "Connections.CopyAllInputs",
            run: () => clearSource
                ? Connections.MigrateAllInputs(oldTarget: source, newTarget: target, undo: actions)
                : Connections.CopyAllInputs(oldTarget: source, newTarget: target, undo: actions));

    private static Fin<int> RewireOutputs(Op op, IParameter source, IParameter target, ActionList actions, bool clearSource) =>
        NativeCount(
            op: op,
            name: clearSource ? "Connections.MigrateAllOutputs" : "Connections.CopyAllOutputs",
            run: () => clearSource
                ? Connections.MigrateAllOutputs(oldSource: source, newSource: target, undo: actions)
                : Connections.CopyAllOutputs(oldSource: source, newSource: target, undo: actions));
}

[SmartEnum<int>]
public sealed partial class GraphMetric {
    private delegate Fin<WireResult> RunFn(GrasshopperUi.Scope scope, Seq<Guid> ids);

    // Native Connectivity.IsLinear throws on an empty id set; an empty subgraph is definitively non-linear,
    // so the library owns the degenerate case rather than surfacing the host throw to callers.
    public static readonly GraphMetric Linearity = new(
        key: 0,
        run: static (scope, ids) =>
            ids.IsEmpty
                ? Fin.Succ(value: (WireResult)new WireResult.LinearityResult(Linearity: new WireLinearity(
                    IsLinear: false, Start: Option<Guid>.None, End: Option<Guid>.None)))
                : (from objects in scope.NeedObjects()
                   from result in Op.Of(name: nameof(Linearity)).Attempt(
                       body: () => {
                           bool linear = objects.Connectivity.IsLinear(ids: [.. ids], first: out ConnectiveObject? start, last: out ConnectiveObject? end);
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
    // of the collapsed graph — reuses the GraphTopology result of the plain Topology metric.
    public static readonly GraphMetric RelayCollapsed = new(
        key: 3,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(RelayCollapsed)).Attempt(
                body: () => (WireResult)new WireResult.TopologyResult(Topology: objects.Connectivity.WithoutRelays(dangling: true, simple: true, complex: false).SubsetTopology(ids: [.. ids])),
                what: "Connectivity.WithoutRelays")
            select result);
    // All upstream->downstream paths between the first and last id; FindConnections is an unbounded BFS, so the
    // path count is capped at DefaultCount and each path projects ConnectiveObject -> Id.
    public static readonly GraphMetric Paths = new(
        key: 4,
        run: static (scope, ids) =>
            from endpoints in PathEndpoints(ids: ids)
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(Paths)).Attempt(
                body: () => {
                    Seq<Seq<Guid>> paths = toSeq(objects.Connectivity
                            .FindConnections(endpoints.Source, endpoints.Target)
                            .Take(count: WireObjectLimit.DefaultCount))
                            .Map(static path => toSeq(path.Select(static co => co.Id)));
                    return (WireResult)new WireResult.PathsResult(Paths: paths);
                },
                what: "Connectivity.FindConnections")
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
        return ends.Count >= 2
            ? Fin.Succ((Source: ends.Head.IfNone(Guid.Empty), Target: ends.Last.IfNone(Guid.Empty)))
            : Fin.Fail<(Guid Source, Guid Target)>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Paths)), detail: "paths require two non-empty endpoint ids"));
    }
}

[SmartEnum<int>]
public sealed partial class WireListKind {
    private delegate IEnumerable<WireEnds>? WireSource(GhObjectList objects);
    private delegate bool WireFilter(WireSnapshot.ConnectedCase wire);

    public static readonly WireListKind All = new(key: 0, source: static objects => objects.AllWires, include: static _ => true);
    public static readonly WireListKind Selected = new(key: 1, source: static objects => objects.SelectedWires, include: static _ => true);
    public static readonly WireListKind Dangling = new(key: 2, source: static objects => objects.AllWires, include: static wire => !wire.Connected);

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

[GenerateUnionOps]
[Union]
public partial record WireOp {
    private WireOp() { }
    public sealed partial record QueryCase(WireQuery Request) : WireOp;
    public sealed partial record SelectCase(WireSelectionOp Op) : WireOp;
    public sealed partial record SplitCase(WireSnapshot.ConnectedCase Wire, PointF Location) : WireOp;
    public sealed partial record EditCase(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args = default) : WireOp;
    public sealed partial record EditBatchCase(Seq<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> Edits) : WireOp;
    public sealed partial record InstallShapeCase(Type ShapeType) : WireOp;
    public sealed partial record OverlayCase(WireOverlayStyle Style, MotionClock? Clock = null) : WireOp;
    public sealed partial record WirePaintObserveCase(MotionClock? Clock = null) : WireOp;
    public static WireOp Query(WireQuery query) => new QueryCase(Request: query);
    public static WireOp Select(WireSelectionOp op) => new SelectCase(Op: op);
    public static WireOp Split(WireSnapshot.ConnectedCase wire, PointF location) => new SplitCase(Wire: wire, Location: location);
    public static WireOp Edit(WireSnapshot.ConnectedCase wire, WireEdit edit, WireEditArgs args = default) => new EditCase(Wire: wire, Kind: edit, Args: args);
    // Batch: N wire edits thread one ActionList → one undo entry, reusing the WireEdit dispatch.
    public static WireOp EditBatch(params ReadOnlySpan<(WireSnapshot.ConnectedCase Wire, WireEdit Kind, WireEditArgs Args)> edits) =>
        new EditBatchCase(Edits: toSeq(edits.ToArray()));
    public static WireOp InstallShape(Type shapeType) => new InstallShapeCase(ShapeType: shapeType);
    public static WireOp Overlay(WireOverlayStyle style, MotionClock? clock = null) => new OverlayCase(Style: style, Clock: clock);
    public static WireOp WirePaintObserve(MotionClock? clock = null) => new WirePaintObserveCase(Clock: clock);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        queryCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        installShapeCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.None),
        overlayCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled),
        wirePaintObserveCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled),
        selectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        splitCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        editCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        editBatchCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
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
public readonly record struct WireDrawnEntry(Guid SourceId, Guid TargetId, WireKind Kind);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireOverlayStyle(PaintStyle Style, Option<Func<WireDrawnEntry, PaintStyle>> Select = default) {
    internal PaintStyle For(WireDrawnEntry entry) {
        PaintStyle fallback = Style;
        Option<Func<WireDrawnEntry, PaintStyle>> select = Select;
        return select.Match(Some: pick => pick(arg: entry), None: () => fallback);
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
    internal static GrasshopperUiIntent<WireResult> Dispatch(WireOp op) => op.Switch(
        queryCase: static q => Query(query: q.Request),
        selectCase: static s => Selection(op: s.Op).Map(static delta => (WireResult)new WireResult.SelectionCase(Delta: delta)),
        splitCase: static s => Split(wire: s.Wire, location: s.Location).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editCase: static e => Edit(wire: e.Wire, edit: e.Kind, args: e.Args).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        editBatchCase: static e => EditBatch(edits: e.Edits).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        installShapeCase: static i => InstallShape(shapeType: i.ShapeType).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        overlayCase: static o => Overlay(style: o.Style, clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        wirePaintObserveCase: static o => WirePaintObserve(clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)));

    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) => query.Switch(
        listCase: static list => Listed(kind: list.Kind).Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
        pickCase: static p => Pick(point: p.Point, tolerance: p.Tolerance).Map(static wire => (WireResult)new WireResult.WireCase(Wire: wire)),
        graphCase: static g => Graph(anchor: g.Anchor, direction: g.Direction, maxObjects: g.MaxObjects).Map(static graph => (WireResult)new WireResult.GraphCase(Graph: graph)),
        graphMetricCase: static g => GhUi.Document(run: scope => g.Kind.Run(scope: scope, ids: g.Ids)),
        canInsertCase: static c => CanInsert(objectId: c.ObjectId, location: c.Location).Map(static snap => (WireResult)new WireResult.InsertCase(Snapshot: snap)),
        recentlyDrawnCase: static _ => RecentlyDrawn().Map(static snap => (WireResult)new WireResult.DrawnCase(Snapshot: snap)));

    internal static GrasshopperUiIntent<Subscription> InstallShape(Type shapeType) =>
        GhUi.Canvas(run: _ =>
            from valid in Optional(shapeType).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(InstallShape)), detail: "shape type is required"))
            from assignable in typeof(WireShape).IsAssignableFrom(c: valid)
                ? Fin.Succ(value: valid)
                : Fin.Fail<Type>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(InstallShape)), detail: $"{valid.FullName} does not derive from {typeof(WireShape).FullName}"))
            from sub in WireShapeInstall.Push(shapeType: assignable)
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
                    _ = Motion.PacerResume(pacer: pacer, canvas: canvas);
                    return unit;
                },
                what: "wire paint observe wake").MapFail(error => UiFault.ThreadMarshal(detail: error.Message))
            select Subscription.DisposeOnce(Subscription.PaintPacer(
                paintHook: hooks,
                pacerRelease: () => _ = Motion.PacerRelease(pacer: pacer))));

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

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Listed(WireListKind kind) =>
        GhUi.Document(run: scope =>
            from objs in scope.NeedObjects()
            from doc in scope.NeedDocument()
            select SafeWires(source: kind.Source(objects: objs), objects: objs, document: Some(doc)).Filter(kind.Include));

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

    private static Fin<DocumentMutationReceipt> ApplyEditRow(GhDocumentMethods methods, GhObjectList objs, ActionList actions, WireSnapshot.ConnectedCase wire, WireEdit edit, WireEditArgs args) {
        Op op = Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{edit}"));
        return from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "wire edit is required"))
               from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"source param {wire.Source} not found"))
               from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"target param {wire.Target} not found"))
               from _ in validEdit.RequiresConnectedPair
                   ? RequireConnected(objects: objs, source: source, target: target, op: op).Map(static _ => unit)
                   : Fin.Succ(unit)
               from receipt in ApplyResolvedEdit(op: op, methods: methods, objects: objs, source: source, target: target, actions: actions, edit: validEdit, args: args)
               select receipt;
    }

    private static Fin<DocumentMutationReceipt> ApplyResolvedEdit(Op op, GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions, WireEdit edit, WireEditArgs args) =>
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
        Seq<WireSnapshot.ConnectedCase> wires = SafeWires(source: objects.AllWires, objects: objects, document: Some(document));
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

    private static Seq<WireSnapshot.ConnectedCase> CycleWires(Seq<Guid> seeds, Seq<WireSnapshot.ConnectedCase> wires) {
        Seq<WireSnapshot.ConnectedCase> connected = wires.Filter(static wire => wire.Connected);
        Seq<Guid> starts = seeds.Filter(static id => id != Guid.Empty).Distinct();
        Seq<Guid> nodes = (starts.IsEmpty
            ? connected.Bind(static wire => Seq(wire.Source, wire.Target))
            : starts).Distinct();
        LanguageExt.HashSet<Guid> cyclic = toHashSet(nodes.Filter(id => HasCycle(seed: id, wires: connected)));
        return connected.Filter(wire => cyclic.Find(key: wire.Source).IsSome || cyclic.Find(key: wire.Target).IsSome);
    }

    private static bool HasCycle(Guid seed, Seq<WireSnapshot.ConnectedCase> wires) {
        HashMap<Guid, Seq<Guid>> graph = toHashMap(wires
            .GroupBy(static wire => wire.Source)
            .Select(group => (group.Key, toSeq(group.Select(static wire => wire.Target)))));
        bool Visit(Guid current, LanguageExt.HashSet<Guid> seen) =>
            graph.Find(current).IfNone(Seq<Guid>()).Exists(next =>
                next == seed || (seen.Find(key: next).IsNone && Visit(current: next, seen: seen.Add(next))));
        return Visit(current: seed, seen: toHashSet(Seq(seed)));
    }

    // Single shared (Source, Target) index per call → SnapshotIn is O(1) membership + 2 FindParameter
    // lookups instead of O(N) AllWires.Any. WireIndexCache hoists the index O(W)→O(W per doc edit)
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
        IsConnected(objects: objects, wire: new WireEnds(source: source.InstanceId, target: target.InstanceId))
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"));

    private static Fin<WireSelectionDelta> ToggleWire(Op op, GhObjectList objects, WireSnapshot.ConnectedCase wire, bool picked) {
        WireEnds ends = new(source: wire.Source, target: wire.Target);
        return Optional(IsConnected(objects: objects, wire: ends))
            .Filter(static live => live)
            .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"))
            .Map(_ => objects.IsWireSelected(wire: ends))
            .Bind(before =>
                op.Attempt(body: () => picked ? objects.SelectWire(wire: ends) : objects.DeselectWire(wire: ends), what: "wire selection")
                .Map(_ => SelectionDelta(picked: picked, before: before, after: objects.IsWireSelected(wire: ends))));
    }

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
        Seq<WireSnapshot.ConnectedCase> all = SafeWires(source: objects.AllWires, objects: objects, document: Some(document));
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
                .Bind(field => expected.IsAssignableFrom(c: field.FieldType)
                    ? Fin.Succ(value: field)
                    : Fin.Fail<FieldInfo>(error: UiFault.MutationRejected(op: op, detail: $"WireData.{memberName} type drifted to {field.FieldType.FullName}")));
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
        return new WireDrawnEntry(SourceId: source.InstanceId, TargetId: target.InstanceId, Kind: kind);
    }
}

file static class WireShapeInstall {
    private static readonly Type DefaultShape = typeof(WireShapeDefault);
    private static readonly Atom<Seq<Type>> Stack = Atom(value: Seq<Type>());

    internal static Fin<Subscription> Push(Type shapeType) =>
        Op.Of(name: nameof(WireShapeInstall)).Attempt(body: () => {
            Type prior = WireShape.ShapeType ?? DefaultShape;
            WireShape.ShapeType = shapeType;
            _ = Stack.Swap(stack => stack + prior);
            return unit;
        }, what: nameof(WireShapeInstall))
        .Bind(_ => Subscription.Bind(attach: static () => { }, detach: Pop, marshalToUi: true, detachOnce: true));

    // Guard the throwing static setter (same as Push) so a UI-thread detach throw cannot silently corrupt the
    // static via the swallowing InvokeOnUiThread.
    private static void Pop() {
        Seq<Type> remaining = Stack.Swap(static stack => stack.IsEmpty ? stack : stack.Init);
        _ = Op.Of(name: nameof(WireShapeInstall)).Attempt(
            body: () => { WireShape.ShapeType = remaining.IsEmpty ? DefaultShape : remaining.Last(); return unit; },
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

    // Repository bootstrap failure is silent — drift invalidation no-ops when reflection rail is absent.
    internal static Unit InvalidateOnStampDrift(GhCanvas canvas) =>
        WireRepositoryRail.Repository.Value.Match(
            Fail: _ => unit,
            Succ: access => Optional(access.CacheProperty.GetValue(obj: canvas)).Match(
                None: () => unit,
                Some: repo => {
                    WireDrawnStamp current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access);
                    CacheKey key = KeyOf(canvas: canvas, stamp: current);
                    _ = Cache.Find(key: key)
                        .Filter(entry => entry.Stamp != current)
                        .Iter(_ => Cache.Invalidate(key: key));
                    return unit;
                }));

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
        Guid hash = document.Hash;
        int stamp = document.Modifications;
        return Cache.Find(key: hash) is { IsSome: true, Case: Entry hit } && hit.Stamp == stamp
            ? hit.Connected
            : InsertAndReturn(hash: hash, stamp: stamp, index: BuildConnected(objects: objects));
    }

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> BuildConnected(GhObjectList objects) =>
        Optional(objects.AllWires)
            .Map(wires => toHashSet(toSeq(wires).Map(static w => (w.Source, w.Target))))
            .IfNone(toHashSet(Seq<(Guid Source, Guid Target)>()));

    private static LanguageExt.HashSet<(Guid Source, Guid Target)> InsertAndReturn(Guid hash, int stamp, LanguageExt.HashSet<(Guid Source, Guid Target)> index) {
        _ = Cache.Record(key: hash, value: new Entry(Stamp: stamp, Connected: index));
        return index;
    }
}

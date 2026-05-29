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
public readonly partial struct WireHopLimit {
    internal const int DefaultCount = 32;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref int value) =>
        validationError = value >= 0
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(WireHopLimit)), message: string.Create(CultureInfo.InvariantCulture, $"must be non-negative (got {value})."));
}

[SkipUnionOps]
[Union]
public partial record WireSnapshot {
    private WireSnapshot() { }
    public sealed record ConnectedCase(Guid Source, Guid Target, bool SourceResolved, bool TargetResolved, bool Connected, bool Selected) : WireSnapshot;
    public sealed record AbsentCase : WireSnapshot;

    public static readonly WireSnapshot Absent = new AbsentCase();
}

[SkipUnionOps]
[Union]
public partial record WireSelectionOp {
    private WireSelectionOp() { }
    public sealed record SelectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed record DeselectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed record DeselectAllCase : WireSelectionOp;

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

    public static readonly WireTraversal Upstream = new(key: 0, walkObjects: static (objects, key) => Walk(objects, key, up: true));
    public static readonly WireTraversal Downstream = new(key: 1, walkObjects: static (objects, key) => Walk(objects, key, up: false));
    public static readonly WireTraversal Bidirectional = new(key: 2, walkObjects: static (objects, key) => Walk(objects, key, up: true).Concat(Walk(objects, key, up: false)));

    [UseDelegateFromConstructor]
    internal partial IEnumerable<IDocumentObject> WalkObjects(GhObjectList objects, GraphKey key);

    private static IEnumerable<IDocumentObject> Walk(GhObjectList objects, GraphKey key, bool up) =>
        key.Switch(
            parameterCase: p => Optional(objects.FindParameter(instanceId: p.Id))
                .Map(param => up ? objects.SearchUpstream(parameter: param) : objects.SearchDownstream(parameter: param))
                .IfNone(noneValue: []),
            ownerCase: o => (up ? objects.Connectivity.FindAllInputs(o.Id) : objects.Connectivity.FindAllOutputs(o.Id))
                .Select(co => objects.Find(instanceId: co.Id))
                .OfType<IDocumentObject>());
}

[SmartEnum<int>]
public sealed partial class WireEdit {
    private delegate Fin<int> WireEditRun(GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions);

    // Disconnect-all verbs operate on one endpoint, so they never require a live source<->target pair.
    public bool RequiresConnectedPair { get; }

    public static readonly WireEdit Connect = new(
        key: 0,
        requiresConnectedPair: true,
        apply: static (_, _, source, target, actions) =>
            WireData.TryCreate(source: source, target: target, data: out WireData _)
                ? Native(op: Op.Of(name: "Wire.Connect"), name: "Connections.Connect", run: () => Connections.Connect(source: source, target: target, undo: actions))
                : Fin.Fail<int>(error: UiFault.MutationRejected(op: Op.Of(name: "Wire.Connect"), detail: "source and target are incompatible for a wire connection")));

    public static readonly WireEdit Disconnect = new(
        key: 1,
        requiresConnectedPair: true,
        apply: static (_, objects, source, target, actions) =>
            from connected in Wire.RequireConnected(objects: objects, source: source, target: target, op: Op.Of(name: "Wire.Disconnect"))
            from changed in Native(op: Op.Of(name: "Wire.Disconnect"), name: "Connections.Disconnect", run: () => Connections.Disconnect(source: source, target: target, undo: actions))
            select changed);

    public static readonly WireEdit Delete = new(
        key: 2,
        requiresConnectedPair: true,
        apply: static (methods, objects, source, target, actions) =>
            from connected in Wire.RequireConnected(objects: objects, source: source, target: target, op: Op.Of(name: "Wire.Delete"))
            from changed in NativeCount(op: Op.Of(name: "Wire.Delete"), name: "DocumentMethods.DeleteObjects", run: () => methods.DeleteObjects(objects: [], wires: [new WireEnds(source: source.InstanceId, target: target.InstanceId)], actions: actions))
            select changed);

    public static readonly WireEdit DisconnectInputs = new(
        key: 3,
        requiresConnectedPair: false,
        apply: static (_, _, _, target, actions) =>
            NativeCount(op: Op.Of(name: "Wire.DisconnectInputs"), name: "Connections.DisconnectAllInputs", run: () => Connections.DisconnectAllInputs(target: target, undo: actions)));

    public static readonly WireEdit DisconnectOutputs = new(
        key: 4,
        requiresConnectedPair: false,
        apply: static (_, _, source, _, actions) =>
            NativeCount(op: Op.Of(name: "Wire.DisconnectOutputs"), name: "Connections.DisconnectAllOutputs", run: () => Connections.DisconnectAllOutputs(source: source, undo: actions)));

    [UseDelegateFromConstructor]
    internal partial Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions);

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
}

[SmartEnum<int>]
public sealed partial class GraphMetric {
    private delegate Fin<WireResult> RunFn(GrasshopperUi.Scope scope, Seq<Guid> ids);

    public static readonly GraphMetric Linearity = new(
        key: 0,
        run: static (scope, ids) =>
            from objects in scope.NeedObjects()
            from result in Op.Of(name: nameof(Linearity)).Attempt(
                body: () => {
                    bool linear = objects.Connectivity.IsLinear(ids: [.. ids], first: out ConnectiveObject? start, last: out ConnectiveObject? end);
                    return (WireResult)new WireResult.LinearityResult(Linearity: new WireLinearity(
                        IsLinear: linear,
                        Start: Optional(start?.Id).Filter(static g => g != Guid.Empty),
                        End: Optional(end?.Id).Filter(static g => g != Guid.Empty)));
                },
                what: "Connectivity.IsLinear")
            select result);
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
                    ConnectiveObject[] nodes = [.. ids.Choose(id => objects.Connectivity.Find(id, out ConnectiveObject? co) ? Optional(co) : Option<ConnectiveObject>.None)];
                    ConnectiveObject[] sorted = objects.Connectivity.SortCausally(objects: nodes);
                    return (WireResult)new WireResult.SortedIdsResult(Ids: toSeq(sorted.Select(static co => co.Id)));
                },
                what: "Connectivity.SortCausally")
            select result);

    public static WireQuery Query(Seq<Guid> ids, GraphMetric kind) => WireQuery.GraphMetric(ids: ids, kind: kind);

    [UseDelegateFromConstructor]
    internal partial Fin<WireResult> Run(GrasshopperUi.Scope scope, Seq<Guid> ids);
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
    public sealed partial record PickCase(PointF Point) : WireQuery;
    public sealed partial record GraphCase(GraphKey Anchor, WireTraversal Direction, WireHopLimit MaxHops) : WireQuery;
    public sealed partial record GraphMetricCase(Seq<Guid> Ids, GraphMetric Kind) : WireQuery;
    public sealed partial record CanInsertCase(Guid ObjectId, PointF Location) : WireQuery;
    public sealed partial record RecentlyDrawnCase : WireQuery;
    public static readonly WireQuery All = new ListCase(Kind: WireListKind.All);
    public static readonly WireQuery Selected = new ListCase(Kind: WireListKind.Selected);
    public static readonly WireQuery Dangling = new ListCase(Kind: WireListKind.Dangling);
    public static WireQuery Pick(PointF point) => new PickCase(Point: point);
    public static WireQuery Graph(GraphKey anchor, WireTraversal? direction = null, WireHopLimit? maxHops = null) =>
        new GraphCase(
            Anchor: anchor,
            Direction: direction ?? WireTraversal.Bidirectional,
            MaxHops: maxHops ?? WireHopLimit.Create(value: WireHopLimit.DefaultCount));
    public static WireQuery Graph(GraphKey anchor, WireTraversal? direction, int maxHops) =>
        new GraphCase(
            Anchor: anchor,
            Direction: direction ?? WireTraversal.Bidirectional,
            MaxHops: WireHopLimit.Create(value: maxHops));
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
    public sealed partial record EditCase(WireSnapshot.ConnectedCase Wire, WireEdit Kind) : WireOp;
    public sealed partial record InstallShapeCase(Type ShapeType) : WireOp;
    public sealed partial record OverlayPenCase(Pen Pen, MotionClock? Clock = null) : WireOp;
    public sealed partial record WirePaintObserveCase(MotionClock? Clock = null) : WireOp;
    public static WireOp Query(WireQuery query) => new QueryCase(Request: query);
    public static WireOp Select(WireSelectionOp op) => new SelectCase(Op: op);
    public static WireOp Split(WireSnapshot.ConnectedCase wire, PointF location) => new SplitCase(Wire: wire, Location: location);
    public static WireOp Edit(WireSnapshot.ConnectedCase wire, WireEdit edit) => new EditCase(Wire: wire, Kind: edit);
    public static WireOp InstallShape(Type shapeType) => new InstallShapeCase(ShapeType: shapeType);
    public static WireOp OverlayPen(Pen pen, MotionClock? clock = null) => new OverlayPenCase(Pen: pen, Clock: clock);
    public static WireOp WirePaintObserve(MotionClock? clock = null) => new WirePaintObserveCase(Clock: clock);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        queryCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        installShapeCase: static _ => GrasshopperUiPolicy.Read,
        overlayPenCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled),
        wirePaintObserveCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled),
        selectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        splitCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        editCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
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
public readonly record struct WireInsertSnapshot(bool CanInsert, Option<Guid> SourceId, Option<Guid> TargetId);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnEntry(Guid SourceId, Guid TargetId, WireKind Kind);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireDrawnStamp(
    Guid DocumentHash,
    int Modifications,
    PointF ProjectionCentre,
    float ProjectionZoom,
    RectangleF DrawInnerFrame);

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
        editCase: static e => Edit(wire: e.Wire, edit: e.Kind).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
        installShapeCase: static i => InstallShape(shapeType: i.ShapeType).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        overlayPenCase: static o => OverlayPen(pen: o.Pen, clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)),
        wirePaintObserveCase: static o => WirePaintObserve(clock: o.Clock ?? MotionClock.MessageLoop).Map(static sub => (WireResult)new WireResult.SubscriptionCase(Subscription: sub)));

    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) => query.Switch(
        listCase: static list => Listed(kind: list.Kind).Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
        pickCase: static p => Pick(point: p.Point).Map(static wire => (WireResult)new WireResult.WireCase(Wire: wire)),
        graphCase: static g => Graph(anchor: g.Anchor, direction: g.Direction, maxHops: g.MaxHops).Map(static graph => (WireResult)new WireResult.GraphCase(Graph: graph)),
        graphMetricCase: static g => GhUi.Document(run: scope => g.Kind.Run(scope: scope, ids: g.Ids)),
        canInsertCase: static c => CanInsert(objectId: c.ObjectId, location: c.Location).Map(static snap => (WireResult)new WireResult.InsertCase(Snapshot: snap)),
        recentlyDrawnCase: static _ => RecentlyDrawn().Map(static snap => (WireResult)new WireResult.DrawnCase(Snapshot: snap)));

    internal static GrasshopperUiIntent<Subscription> InstallShape(Type shapeType) =>
        GhUi.Read(run: _ => WireShapeInstall.Push(shapeType: shapeType));

    internal static GrasshopperUiIntent<Subscription> OverlayPen(Pen pen, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.AfterWires,
                paint: paintScope => WireRepositoryRail.AfterWirePaint(canvas: canvas, scope: paintScope, pen: pen),
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

    internal static GrasshopperUiIntent<WireSnapshot> Pick(PointF point) =>
        GhUi.Document(run: scope =>
            from pick in UiRail.PickAt(scope: scope, point: point, source: CoordinateSystem.Content, policy: CanvasPickPolicy.Wires, tolerance: PickTolerance.Create(value: 0f))
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
                    selectCase: static (objs, s) => ToggleWire(op: Op.Of(name: "Wire.Select"), objects: objs, wire: s.Wire, picked: true),
                    deselectCase: static (objs, d) => ToggleWire(op: Op.Of(name: "Wire.Deselect"), objects: objs, wire: d.Wire, picked: false),
                    deselectAllCase: static (objs, _) => DeselectAllWires(op: Op.Of(name: "Wire.DeselectAll"), objects: objs))
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
                        Seq<Guid> created = Optional(shout?.InstanceId).Filter(static g => g != Guid.Empty).Map(static id => Seq(id)).IfNone(Seq<Guid>())
                            + Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty).Map(static id => Seq(id)).IfNone(Seq<Guid>());
                        return DocumentMutationReceipt.Of(changed: changed ? 1 : 0, created: created);
                    },
                    what: "DocumentMethods.SplitWire")
                select split);

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Edit(WireSnapshot.ConnectedCase wire, WireEdit edit) =>
        MutateConnectedWire(
            op: Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{edit}")),
            wire: wire,
            requireConnectedPair: edit.RequiresConnectedPair,
            run: (methods, objs, source, target, actions) =>
                from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: "wire edit is required"))
                from changed in validEdit.Apply(methods: methods, objects: objs, source: source, target: target, actions: actions)
                select DocumentMutationReceipt.Count(changed: changed));

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateConnectedWire(
        Op op,
        WireSnapshot.ConnectedCase wire,
        Func<GhDocumentMethods, GhObjectList, IParameter, IParameter, ActionList, Fin<DocumentMutationReceipt>> run,
        bool requireConnectedPair = true) =>
        GhUi.Document(
            repaint: RepaintRequest.Canvas,
            run: scope => UiRail.RunMutation(
                scope: scope,
                op: op,
                policy: DocumentMutationPolicy.Default,
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
    internal static GrasshopperUiIntent<WireGraph> Graph(GraphKey anchor, WireTraversal direction, WireHopLimit maxHops) =>
        GhUi.Document(run: scope =>
            from seed in Optional(anchor.SeedId).Filter(static g => g != Guid.Empty)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Graph)), detail: "anchor id must be non-empty"))
            from objects in scope.NeedObjects()
            from doc in scope.NeedDocument()
            from result in Op.Of(name: nameof(Graph)).Attempt(
                body: () => GraphOf(objects: objects, document: doc, anchor: anchor, direction: direction, maxHops: maxHops),
                what: "graph walk")
            select result);

    // --- [OPERATIONS] -------------------------------------------------------------------------
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
        bool connected = index.Find(key: (wire.Source, wire.Target)).IsSome && source is not null && target is not null;
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
                op.Attempt(body: () => {
                    _ = picked ? objects.SelectWire(wire: ends) : objects.DeselectWire(wire: ends);
                    return unit;
                }, what: "wire selection")
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

    internal static Fin<Seq<IDocumentObject>> TraverseObjects(GhObjectList objects, Guid startParameterId, WireTraversal direction) =>
        Optional(objects.FindParameter(instanceId: startParameterId))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TraverseObjects)), detail: $"parameter {startParameterId} not found"))
            .Map(_ => toSeq(direction.WalkObjects(objects: objects, key: GraphKey.Parameter(id: startParameterId))).Distinct());

    private static WireGraph GraphOf(GhObjectList objects, GhDocument document, GraphKey anchor, WireTraversal direction, WireHopLimit maxHops) {
        int hopCount = maxHops.Value;
        Seq<IDocumentObject> walked = toSeq(direction.WalkObjects(objects: objects, key: anchor).Take(count: hopCount)).Distinct();
        bool parameterKeyed = anchor is GraphKey.ParameterCase;
        Seq<Guid> visited = (parameterKeyed
            ? Seq(anchor.SeedId) + walked.Choose(static obj => Optional(obj as IParameter)).Map(static p => p.InstanceId)
            : Seq(anchor.SeedId) + walked.Map(static obj => obj.InstanceId)).Distinct();
        LanguageExt.HashSet<Guid> visitedSet = toHashSet(visited);
        Seq<WireSnapshot.ConnectedCase> all = SafeWires(source: objects.AllWires, objects: objects, document: Some(document));
        Seq<WireSnapshot.ConnectedCase> wires = parameterKeyed
            ? all.Filter(w => visitedSet.Find(key: w.Source).IsSome && visitedSet.Find(key: w.Target).IsSome)
            : all.Filter(w => visitedSet.Find(key: w.Source).IsSome || visitedSet.Find(key: w.Target).IsSome);
        return new WireGraph(Wires: wires, Visited: visited);
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
        return from cache in Optional(typeof(GhCanvas).GetProperty(name: "WireDrawCache", bindingAttr: instance))
                .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "Canvas.WireDrawCache property not found"))
               let repoType = cache.PropertyType
               from wireData in Optional(repoType.GetNestedType(name: "WireData", bindingAttr: BindingFlags.Public | BindingFlags.NonPublic))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.WireData not found"))
               from canInsert in Optional(repoType.GetMethod(
                       name: "CanInsertObject",
                       bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                       binder: null,
                       types: [typeof(IDocumentObject), typeof(PointF), typeof(IParameter).MakeByRefType(), typeof(IParameter).MakeByRefType()],
                       modifiers: null))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.CanInsertObject not found"))
               from recent in Optional(repoType.GetProperty(name: "MostRecentlyDrawnWires", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.MostRecentlyDrawnWires not found"))
               from innerFrame in Optional(repoType.GetProperty(name: "InnerFrame", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.InnerFrame not found"))
               from shape in Optional(wireData.GetField(name: "Shape", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.WireData.Shape not found"))
               from source in Optional(wireData.GetField(name: "Source", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.WireData.Source not found"))
               from target in Optional(wireData.GetField(name: "Target", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.WireData.Target not found"))
               from kind in Optional(wireData.GetField(name: "Kind", bindingAttr: BindingFlags.Instance | BindingFlags.Public))
                   .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "WireRepository.WireData.Kind not found"))
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

    internal static Fin<WireInsertSnapshot> CanInsert(GhCanvas canvas, IDocumentObject obj, PointF location) =>
        from access in Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: "wire repository is null"))
        from snapshot in RailOp.Attempt(body: () => {
            object?[] args = [obj, location, null, null];
            bool canInsert = (bool)access.CanInsert.Invoke(obj: repo, parameters: args)!;
            IParameter? source = (IParameter?)args[2];
            IParameter? target = (IParameter?)args[3];
            return new WireInsertSnapshot(
                CanInsert: canInsert,
                SourceId: Optional(source?.InstanceId).Filter(static g => g != Guid.Empty),
                TargetId: Optional(target?.InstanceId).Filter(static g => g != Guid.Empty));
        }, what: nameof(CanInsert))
        select snapshot;

    internal static Fin<WireDrawnSnapshot> CaptureDrawn(GhCanvas canvas) =>
        from access in Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: "wire repository is null"))
        from snapshot in RailOp.Attempt(body: () => {
            IEnumerable<object> wires = ((System.Collections.IEnumerable)access.RecentlyDrawn.GetValue(obj: repo)!).Cast<object>();
            return new WireDrawnSnapshot(
                Entries: toSeq(wires).Map(wire => MapWireData(wire: wire, access: access)).ToSeq(),
                Stamp: StampOf(canvas: canvas, repo: repo, access: access),
                FreshFromWirePaint: true);
        }, what: nameof(CaptureDrawn))
        select snapshot;

    internal static Fin<Unit> AfterWirePaint(GhCanvas canvas, PaintScope scope, Pen pen) =>
        from snapshot in CaptureDrawn(canvas: canvas)
        from _ in Fin.Succ(value: WireDrawnCache.Record(canvas: canvas, snapshot: snapshot))
        from __ in DrawOverlay(canvas: canvas, scope: scope, pen: pen)
        select unit;

    internal static Fin<Unit> DrawOverlay(GhCanvas canvas, PaintScope scope, Pen pen) =>
        from access in Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(op: RailOp, detail: "wire repository is null"))
        from _ in RailOp.Attempt(body: () => {
            Graphics graphics = scope.Graphics.Content;
            _ = toSeq(((System.Collections.IEnumerable)access.RecentlyDrawn.GetValue(obj: repo)!).Cast<object>()).Iter(wire =>
                Op.Side(() => {
                    WireShape shape = (WireShape)access.Shape.GetValue(wire)!;
                    shape.Draw(graphics: graphics, edge: pen);
                }));
            return unit;
        }, what: nameof(DrawOverlay))
        select unit;

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
            _ = Stack.Swap(stack => stack + prior);
            WireShape.ShapeType = shapeType;
            return unit;
        }, what: nameof(WireShapeInstall))
        .Bind(_ => Subscription.Bind(attach: static () => { }, detach: Pop, marshalToUi: true, detachOnce: true));

    private static void Pop() {
        Seq<Type> remaining = Stack.Swap(static stack => stack.IsEmpty ? stack : stack.Init);
        WireShape.ShapeType = remaining.IsEmpty ? DefaultShape : remaining.Last();
    }
}

// --- [CACHE] ------------------------------------------------------------------------------
// AfterWires populates; Read matches composite stamp (doc, modifications, projection, inner frame).
file static class WireDrawnCache {
    private const int MaxEntries = 8;

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CacheKey(int CanvasId, Guid DocumentHash);
    private readonly record struct Entry(WireDrawnStamp Stamp, WireDrawnSnapshot Snapshot);

    private static readonly Atom<(Seq<CacheKey> Order, HashMap<CacheKey, Entry> Map)> ByKey =
        Atom(value: (Order: Seq<CacheKey>(), Map: HashMap<CacheKey, Entry>()));

    private static CacheKey KeyOf(GhCanvas canvas, WireDrawnStamp stamp) =>
        new(CanvasId: RuntimeHelpers.GetHashCode(canvas), DocumentHash: stamp.DocumentHash);

    internal static Unit Record(GhCanvas canvas, WireDrawnSnapshot snapshot) {
        CacheKey key = KeyOf(canvas: canvas, stamp: snapshot.Stamp);
        _ = ByKey.Swap(state => {
            Seq<CacheKey> merged = state.Order.Filter(k => k != key) + Seq(key);
            int skip = merged.Count > MaxEntries ? merged.Count - MaxEntries : 0;
            Seq<CacheKey> promoted = toSeq(merged.Skip(count: skip));
            LanguageExt.HashSet<CacheKey> keep = toHashSet(promoted);
            HashMap<CacheKey, Entry> map = state.Map
                .AddOrUpdate(key: key, value: new Entry(Stamp: snapshot.Stamp, Snapshot: snapshot))
                .Filter((k, _) => keep.Find(key: k).IsSome);
            return (Order: promoted, Map: map);
        });
        return unit;
    }

    private static Unit Invalidate(GhCanvas canvas, CacheKey key) {
        CacheKey cacheKey = key;
        _ = ByKey.Swap(state => (
            Order: state.Order.Filter(k => k != cacheKey),
            Map: state.Map.Remove(cacheKey)));
        return unit;
    }

    // Repository bootstrap failure is silent — drift invalidation no-ops when reflection rail is absent.
    internal static Unit InvalidateOnStampDrift(GhCanvas canvas) =>
        WireRepositoryRail.Repository.Value.Match(
            Fail: _ => unit,
            Succ: access => Optional(access.CacheProperty.GetValue(obj: canvas)).Match(
                None: () => unit,
                Some: repo => {
                    WireDrawnStamp current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access);
                    CacheKey key = KeyOf(canvas: canvas, stamp: current);
                    _ = ByKey.Value.Map.Find(key)
                        .Filter(entry => entry.Stamp != current)
                        .Iter(_ => Invalidate(canvas: canvas, key: key));
                    return unit;
                }));

    internal static Fin<WireDrawnSnapshot> Read(GhCanvas canvas) =>
        from access in WireRepositoryRail.Repository.Value
        from repo in Optional(access.CacheProperty.GetValue(obj: canvas)).ToFin(Fail: UiFault.MutationRejected(
            op: Op.Of(name: nameof(Read)), detail: "wire repository is null"))
        let current = WireRepositoryRail.StampOf(canvas: canvas, repo: repo, access: access)
        let key = KeyOf(canvas: canvas, stamp: current)
        from entry in ByKey.Value.Map.Find(key)
            .Filter(entry => entry.Stamp == current)
            .ToFin(Fail: UiFault.InvalidInput(
                op: Op.Of(name: nameof(Read)),
                detail: "wire draw snapshot is missing or stale; subscribe WirePaintObserve or OverlayPen and schedule repaint before query"))
        select entry.Snapshot with { FreshFromWirePaint = false };
}

// Document-keyed (Source,Target) index. Refreshes on Document.Modifications change; MRU-bounded.
file static class WireIndexCache {
    private const int MaxDocuments = 8;

    private readonly record struct Entry(int Stamp, LanguageExt.HashSet<(Guid Source, Guid Target)> Connected);

    private static readonly Atom<(Seq<Guid> Order, HashMap<Guid, Entry> Entries)> Cell =
        Atom(value: (Order: Seq<Guid>(), Entries: HashMap<Guid, Entry>()));

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> ConnectedOf(GhObjectList objects, GhDocument document) {
        Guid hash = document.Hash;
        int stamp = document.Modifications;
        (_, HashMap<Guid, Entry> entries) = Cell.Value;
        return entries.Find(key: hash) is { IsSome: true, Case: Entry hit } && hit.Stamp == stamp
            ? hit.Connected
            : InsertAndReturn(hash: hash, stamp: stamp, index: BuildConnected(objects: objects));
    }

    internal static LanguageExt.HashSet<(Guid Source, Guid Target)> BuildConnected(GhObjectList objects) =>
        Optional(objects.AllWires)
            .Map(wires => toHashSet(toSeq(wires).Map(static w => (w.Source, w.Target))))
            .IfNone(toHashSet(Seq<(Guid Source, Guid Target)>()));

    private static LanguageExt.HashSet<(Guid Source, Guid Target)> InsertAndReturn(Guid hash, int stamp, LanguageExt.HashSet<(Guid Source, Guid Target)> index) {
        _ = Cell.Swap(f: state => Touch(state: state, hash: hash, stamp: stamp, index: index));
        return index;
    }

    private static (Seq<Guid> Order, HashMap<Guid, Entry> Entries) Touch(
        (Seq<Guid> Order, HashMap<Guid, Entry> Entries) state,
        Guid hash, int stamp, LanguageExt.HashSet<(Guid Source, Guid Target)> index) {
        Seq<Guid> merged = state.Order.Filter(h => h != hash) + Seq(hash);
        int skip = merged.Count > MaxDocuments ? merged.Count - MaxDocuments : 0;
        Seq<Guid> promoted = toSeq(merged.Skip(count: skip));
        LanguageExt.HashSet<Guid> keep = toHashSet(promoted);
        HashMap<Guid, Entry> entries = state.Entries
            .AddOrUpdate(key: hash, value: new Entry(Stamp: stamp, Connected: index))
            .Filter((k, _) => keep.Find(key: k).IsSome);
        return (Order: promoted, Entries: entries);
    }
}

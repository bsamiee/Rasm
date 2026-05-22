using System.Globalization;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record WireSnapshot {
    private WireSnapshot() { }
    public sealed record ConnectedCase(
        Guid Source, Guid Target,
        bool SourceResolved, bool TargetResolved,
        bool Connected, bool Selected) : WireSnapshot;
    public sealed record AbsentCase : WireSnapshot;

    public static readonly WireSnapshot Absent = new AbsentCase();
}

[Union]
public partial record WireSelectionOp {
    private WireSelectionOp() { }
    public sealed record SelectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed record DeselectCase(WireSnapshot.ConnectedCase Wire) : WireSelectionOp;
    public sealed record DeselectAllCase : WireSelectionOp;

    public static WireSelectionOp Select(WireSnapshot.ConnectedCase wire) => new SelectCase(Wire: wire);
    public static WireSelectionOp Deselect(WireSnapshot.ConnectedCase wire) => new DeselectCase(Wire: wire);
    public static readonly WireSelectionOp DeselectAll = new DeselectAllCase();
}

[SmartEnum<int>]
public sealed partial class WireTraversal {
    private delegate IEnumerable<IDocumentObject> SearchObjects(GhObjectList objects, IParameter parameter);

    public static readonly WireTraversal Upstream = new(
        key: 0,
        search: static (objects, parameter) => objects.SearchUpstream(parameter: parameter));

    public static readonly WireTraversal Downstream = new(
        key: 1,
        search: static (objects, parameter) => objects.SearchDownstream(parameter: parameter));

    public static readonly WireTraversal Bidirectional = new(
        key: 2,
        search: static (objects, parameter) => objects.SearchUpstream(parameter: parameter).Concat(objects.SearchDownstream(parameter: parameter)));

    [UseDelegateFromConstructor]
    internal partial IEnumerable<IDocumentObject> Search(GhObjectList objects, IParameter parameter);
}

[SmartEnum<int>]
public sealed partial class WireEdit {
    private delegate Fin<int> WireEditRun(GhDocumentMethods methods, IParameter source, IParameter target, ActionList actions);

    public static readonly WireEdit Connect = new(
        key: 0,
        apply: static (_, source, target, actions) =>
            WireData.TryCreate(source: source, target: target, data: out WireData _)
                ? Native(op: Op.Of(name: "Wire.Connect"), name: "Connections.Connect", run: () => Connections.Connect(source: source, target: target, undo: actions))
                : Fin.Fail<int>(error: UiFault.MutationRejected(op: Op.Of(name: "Wire.Connect"), detail: "source and target are incompatible for a wire connection")));

    public static readonly WireEdit Disconnect = new(
        key: 1,
        apply: static (_, source, target, actions) =>
            from connected in Wire.RequireConnected(source: source, target: target, op: Op.Of(name: "Wire.Disconnect"))
            from changed in Native(op: Op.Of(name: "Wire.Disconnect"), name: "Connections.Disconnect", run: () => Connections.Disconnect(source: source, target: target, undo: actions))
            select changed);

    public static readonly WireEdit Delete = new(
        key: 2,
        apply: static (methods, source, target, actions) =>
            from connected in Wire.RequireConnected(source: source, target: target, op: Op.Of(name: "Wire.Delete"))
            from changed in NativeCount(op: Op.Of(name: "Wire.Delete"), name: "DocumentMethods.DeleteObjects", run: () => methods.DeleteObjects(objects: [], wires: [new WireEnds(source: source.InstanceId, target: target.InstanceId)], actions: actions))
            select changed);

    public static readonly WireEdit DisconnectInputs = new(
        key: 3,
        apply: static (_, _, target, actions) =>
            NativeCount(op: Op.Of(name: "Wire.DisconnectInputs"), name: "Connections.DisconnectAllInputs", run: () => Connections.DisconnectAllInputs(target: target, undo: actions)));

    public static readonly WireEdit DisconnectOutputs = new(
        key: 4,
        apply: static (_, source, _, actions) =>
            NativeCount(op: Op.Of(name: "Wire.DisconnectOutputs"), name: "Connections.DisconnectAllOutputs", run: () => Connections.DisconnectAllOutputs(source: source, undo: actions)));

    [UseDelegateFromConstructor]
    internal partial Fin<int> Apply(GhDocumentMethods methods, IParameter source, IParameter target, ActionList actions);

    private static Fin<int> Native(Op op, string name, Func<bool> run) =>
        Try.lift(f: run)
            .Run()
            .MapFail(error => UiFault.MutationRejected(op: op, detail: $"{name} threw: {error.Message}"))
            .Bind(changed => changed switch {
                true => Fin.Succ(value: 1),
                false => Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: $"{name} returned false")),
            });

    private static Fin<int> NativeCount(Op op, string name, Func<int> run) =>
        Try.lift(f: run)
            .Run()
            .MapFail(error => UiFault.MutationRejected(op: op, detail: $"{name} threw: {error.Message}"))
            .Bind(count => count switch {
                >= 0 => Fin.Succ(value: count),
                _ => Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"{name} returned {count}"))),
            });
}

[Union]
public partial record WireQuery {
    private WireQuery() { }
    public sealed record AllCase : WireQuery;
    public sealed record SelectedCase : WireQuery;
    public sealed record DanglingCase : WireQuery;
    public sealed record PickCase(PointF Point) : WireQuery;
    public sealed record GraphCase(Guid StartParameterId, WireTraversal Direction, int MaxHops) : WireQuery;
    public static readonly WireQuery All = new AllCase();
    public static readonly WireQuery Selected = new SelectedCase();
    public static readonly WireQuery Dangling = new DanglingCase();
    public static WireQuery Pick(PointF point) => new PickCase(Point: point);
    public static WireQuery Graph(Guid startParameterId, WireTraversal? direction = null, int maxHops = 32) =>
        new GraphCase(StartParameterId: startParameterId, Direction: direction ?? WireTraversal.Bidirectional, MaxHops: maxHops);
}

[Union]
public partial record WireOp {
    private WireOp() { }
    public sealed record QueryCase(WireQuery Request) : WireOp;
    public sealed record SelectCase(WireSelectionOp Op) : WireOp;
    public sealed record SplitCase(WireSnapshot.ConnectedCase Wire, PointF Location) : WireOp;
    public sealed record EditCase(WireSnapshot.ConnectedCase Wire, WireEdit Kind) : WireOp;
    public static WireOp Query(WireQuery query) => new QueryCase(Request: query);
    public static WireOp Select(WireSelectionOp op) => new SelectCase(Op: op);
    public static WireOp Split(WireSnapshot.ConnectedCase wire, PointF location) => new SplitCase(Wire: wire, Location: location);
    public static WireOp Edit(WireSnapshot.ConnectedCase wire, WireEdit edit) => new EditCase(Wire: wire, Kind: edit);
}

[Union]
public partial record WireResult {
    private WireResult() { }
    public sealed record WiresCase(Seq<WireSnapshot.ConnectedCase> Wires) : WireResult;
    public sealed record WireCase(WireSnapshot Wire) : WireResult;
    public sealed record GraphCase(WireGraph Graph) : WireResult;
    public sealed record MutationCase(Snapshot<DocumentMutationDelta> Delta) : WireResult;
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WireGraph(
    Seq<WireSnapshot.ConnectedCase> Wires,
    Seq<Guid> Visited);

internal sealed record WireRequest(WireOp Op) : GhUiRequest<WireResult> {
    internal override GrasshopperUiPolicy Policy => Wire.PolicyOf(op: Op);
    internal override Fin<WireResult> Apply(GrasshopperUi.Scope scope) => Wire.Dispatch(op: Op).Run(scope: scope);
}

internal static partial class Wire {
    internal static GrasshopperUiPolicy PolicyOf(WireOp op) =>
        GrasshopperUiPolicy.Document(repaint: op switch {
            WireOp.SelectCase or WireOp.SplitCase or WireOp.EditCase => RepaintRequest.Canvas,
            _ => RepaintRequest.None,
        });

    internal static GrasshopperUiIntent<WireResult> Dispatch(WireOp op) =>
        op switch {
            WireOp.QueryCase q => Query(query: q.Request),
            WireOp.SelectCase s => Selection(op: s.Op).Map(delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
            WireOp.SplitCase s => Split(wire: s.Wire, location: s.Location).Map(delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
            WireOp.EditCase e => Edit(wire: e.Wire, edit: e.Kind).Map(delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
            _ => GhUi.Document(run: _ => Fin.Fail<WireResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Dispatch)), detail: "unknown wire op"))),
        };

    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) =>
        query switch {
            WireQuery.AllCase => All().Map(wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            WireQuery.SelectedCase => Selected().Map(wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            WireQuery.DanglingCase => Dangling().Map(wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            WireQuery.PickCase p => Pick(point: p.Point).Map(wire => (WireResult)new WireResult.WireCase(Wire: wire)),
            WireQuery.GraphCase g => Graph(startParameterId: g.StartParameterId, direction: g.Direction, maxHops: g.MaxHops).Map(graph => (WireResult)new WireResult.GraphCase(Graph: graph)),
            _ => GhUi.Document(run: _ => Fin.Fail<WireResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Query)), detail: "unknown wire query"))),
        };

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> All() =>
        GhUi.Document(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs)));

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Selected() =>
        GhUi.Document(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.SelectedWires, objects: objs)));

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Dangling() =>
        GhUi.Document(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs).Filter(static w => !w.Connected)));

    internal static GrasshopperUiIntent<WireSnapshot> Pick(PointF point) =>
        GhUi.Document(run: scope =>
            from pickResult in UiRail.CanvasDispatch(scope: scope, op: CanvasOp.Pick(point: point))
                .Bind(static result => result switch {
                    CanvasResult.PickResult pick => Fin.Succ(value: pick.Pick),
                    _ => Fin.Fail<CanvasPickSnapshot>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Pick)), detail: "canvas pick did not return a pick result")),
                })
            from objs in scope.NeedObjects()
            select pickResult.WireUnderPick.Match(
                Some: wireEnds => SnapshotConnected(objects: objs, wire: wireEnds),
                None: () => WireSnapshot.Absent));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Selection(WireSelectionOp op) =>
        op switch {
            WireSelectionOp.SelectCase s => MutateWire(op: Op.Of(name: "Wire.Select"), wire: s.Wire, run: static (objs, ends) => objs.SelectWire(wire: ends)),
            WireSelectionOp.DeselectCase d => MutateWire(op: Op.Of(name: "Wire.Deselect"), wire: d.Wire, run: static (objs, ends) => objs.DeselectWire(wire: ends)),
            WireSelectionOp.DeselectAllCase => MutateDeselectAll(op: Op.Of(name: "Wire.DeselectAll")),
            _ => GhUi.Document(run: _ => Fin.Fail<Snapshot<DocumentMutationDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "unknown WireSelectionOp"))),
        };

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Split(WireSnapshot.ConnectedCase wire, PointF location) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: nameof(Split)),
            undo: UndoStrategy.None,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from valid in Optional(location)
                    .Filter(static point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: "non-finite location"))
                from methods in scope.NeedMethods()
                from objs in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"source param {wire.Source} not found"))
                from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"target param {wire.Target} not found"))
                from connected in RequireConnected(source: source, target: target, op: Op.Of(name: nameof(Split)))
                let actions = ActionList.Empty
                let split = SplitWire(methods: methods, source: source, target: target, name: string.Empty, location: valid, actions: actions)
                let created = split.Shout.Map(static id => Seq(id)).IfNone(Seq<Guid>()) + split.Listen.Map(static id => Seq(id)).IfNone(Seq<Guid>())
                from committed in UiRail.CommitActions(document: doc, op: Op.Of(name: nameof(Split)), actions: actions)
                select new DocumentMutationDelta(
                    Changed: split.Changed ? 1 : 0,
                    After: UiRail.DocumentSnapshotOf(document: doc, objects: objs),
                    Created: created));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Edit(WireSnapshot.ConnectedCase wire, WireEdit edit) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{edit}")),
            undo: UndoStrategy.None,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: "wire edit is required"))
                from methods in scope.NeedMethods()
                from objs in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: $"source param {wire.Source} not found"))
                from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: $"target param {wire.Target} not found"))
                let actions = ActionList.Empty
                from changed in validEdit.Apply(methods: methods, source: source, target: target, actions: actions)
                from committed in UiRail.CommitActions(document: doc, op: Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Wire.{validEdit}")), actions: actions)
                select new DocumentMutationDelta(Changed: changed, After: UiRail.DocumentSnapshotOf(document: doc, objects: objs)));

    internal static GrasshopperUiIntent<WireGraph> Graph(Guid startParameterId, WireTraversal? direction = null, int maxHops = 32) =>
        GhUi.Document(run: scope =>
            from hops in Optional(maxHops)
                .Filter(static count => count >= 0)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Graph)), detail: "maxHops must be non-negative"))
            from objects in scope.NeedObjects()
            from graphObjects in TraverseObjects(objects: objects, startParameterId: startParameterId, direction: direction ?? WireTraversal.Bidirectional)
            select GraphOf(objects: objects, graphObjects: graphObjects, startParameterId: startParameterId, maxHops: hops));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static Seq<WireSnapshot.ConnectedCase> SafeWires(IEnumerable<WireEnds>? source, GhObjectList objects) =>
        Optional(source)
            .Map(wires => toSeq(wires).Map(wire => SnapshotConnected(objects: objects, wire: wire)))
            .IfNone(Seq<WireSnapshot.ConnectedCase>());

    internal static WireSnapshot.ConnectedCase SnapshotConnected(GhObjectList objects, WireEnds wire) {
        IParameter? source = objects.FindParameter(instanceId: wire.Source);
        IParameter? target = objects.FindParameter(instanceId: wire.Target);
        bool connected = source is not null && target is not null && WireData.TryCreate(source: source, target: target, data: out _);
        return new WireSnapshot.ConnectedCase(
            Source: wire.Source, Target: wire.Target,
            SourceResolved: source is not null, TargetResolved: target is not null,
            Connected: connected,
            Selected: connected && objects.IsWireSelected(wire: wire));
    }

    internal static bool IsConnected(GhObjectList objects, WireEnds wire) =>
        (objects.FindParameter(instanceId: wire.Source), objects.FindParameter(instanceId: wire.Target)) switch {
            (IParameter source, IParameter target) => WireData.TryCreate(source: source, target: target, data: out _),
            _ => false,
        };

    internal static Fin<Unit> RequireConnected(IParameter source, IParameter target, Op op) =>
        WireData.TryCreate(source: source, target: target, data: out _)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"));

    internal static Fin<Seq<IDocumentObject>> TraverseObjects(GhObjectList objects, Guid startParameterId, WireTraversal direction) =>
        Optional(objects.FindParameter(instanceId: startParameterId))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TraverseObjects)), detail: $"parameter {startParameterId} not found"))
            .Map(parameter => toSeq(direction.Search(objects: objects, parameter: parameter)).Distinct());

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateWire(
        Op op,
        WireSnapshot.ConnectedCase wire,
        Func<GhObjectList, WireEnds, bool> run) =>
            GrasshopperUi.Mutate(
                op: op,
                undo: UndoStrategy.GhBuiltIn,
                repaint: RepaintRequest.Canvas,
                mutate: scope =>
                    from objects in scope.NeedObjects()
                    from doc in scope.NeedDocument()
                    let ends = new WireEnds(source: wire.Source, target: wire.Target)
                    from live in Optional(IsConnected(objects: objects, wire: ends))
                        .Filter(static live => live)
                        .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"))
                    let before = objects.IsWireSelected(wire: ends)
                    from ran in Try.lift(f: () => run(arg1: objects, arg2: ends))
                        .Run()
                        .MapFail(error => UiFault.MutationRejected(op: op, detail: error.Message))
                    let after = objects.IsWireSelected(wire: ends)
                    select new DocumentMutationDelta(Changed: before == after ? 0 : 1, After: UiRail.DocumentSnapshotOf(document: doc, objects: objects)));

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateDeselectAll(Op op) =>
        GrasshopperUi.Mutate(
            op: op,
            undo: UndoStrategy.GhBuiltIn,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from objects in scope.NeedObjects()
                from doc in scope.NeedDocument()
                let before = objects.SelectedWireCount
                from ran in Try.lift(f: () => { objects.DeselectAllWires(); return unit; })
                    .Run()
                    .MapFail(error => UiFault.MutationRejected(op: op, detail: $"DeselectAllWires threw: {error.Message}"))
                let after = objects.SelectedWireCount
                select new DocumentMutationDelta(Changed: Math.Max(val1: 0, val2: before - after), After: UiRail.DocumentSnapshotOf(document: doc, objects: objects)));

    private static (bool Changed, Option<Guid> Shout, Option<Guid> Listen) SplitWire(
        GhDocumentMethods methods, IParameter source, IParameter target, string name, PointF location, ActionList actions) {
        bool changed = methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: actions);
        return (Changed: changed,
            Shout: Optional(shout?.InstanceId).Filter(static g => g != Guid.Empty),
            Listen: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty));
    }

    private static WireGraph GraphOf(GhObjectList objects, Seq<IDocumentObject> graphObjects, Guid startParameterId, int maxHops) {
        Seq<Guid> visited = (Seq(startParameterId) + toSeq(graphObjects.Choose(static obj => Optional(obj as IParameter)).Take(count: maxHops).Select(static parameter => parameter.InstanceId))).Distinct();
        Seq<WireSnapshot.ConnectedCase> wires = SafeWires(source: objects.AllWires, objects: objects)
            .Filter(wire => visited.Exists(id => id == wire.Source) && visited.Exists(id => id == wire.Target));
        return new WireGraph(Wires: wires, Visited: visited);
    }
}

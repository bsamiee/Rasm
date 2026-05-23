using System.Globalization;
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
    private delegate Fin<int> WireEditRun(GhDocumentMethods methods, GhObjectList objects, IParameter source, IParameter target, ActionList actions);

    public static readonly WireEdit Connect = new(
        key: 0,
        apply: static (_, _, source, target, actions) =>
            WireData.TryCreate(source: source, target: target, data: out WireData _)
                ? Native(op: Op.Of(name: "Wire.Connect"), name: "Connections.Connect", run: () => Connections.Connect(source: source, target: target, undo: actions))
                : Fin.Fail<int>(error: UiFault.MutationRejected(op: Op.Of(name: "Wire.Connect"), detail: "source and target are incompatible for a wire connection")));

    public static readonly WireEdit Disconnect = new(
        key: 1,
        apply: static (_, objects, source, target, actions) =>
            from connected in Wire.RequireConnected(objects: objects, source: source, target: target, op: Op.Of(name: "Wire.Disconnect"))
            from changed in Native(op: Op.Of(name: "Wire.Disconnect"), name: "Connections.Disconnect", run: () => Connections.Disconnect(source: source, target: target, undo: actions))
            select changed);

    public static readonly WireEdit Delete = new(
        key: 2,
        apply: static (methods, objects, source, target, actions) =>
            from connected in Wire.RequireConnected(objects: objects, source: source, target: target, op: Op.Of(name: "Wire.Delete"))
            from changed in NativeCount(op: Op.Of(name: "Wire.Delete"), name: "DocumentMethods.DeleteObjects", run: () => methods.DeleteObjects(objects: [], wires: [new WireEnds(source: source.InstanceId, target: target.InstanceId)], actions: actions))
            select changed);

    public static readonly WireEdit DisconnectInputs = new(
        key: 3,
        apply: static (_, _, _, target, actions) =>
            NativeCount(op: Op.Of(name: "Wire.DisconnectInputs"), name: "Connections.DisconnectAllInputs", run: () => Connections.DisconnectAllInputs(target: target, undo: actions)));

    public static readonly WireEdit DisconnectOutputs = new(
        key: 4,
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
        op.Switch(
            queryCase: static q => Query(query: q.Request),
            selectCase: static s => Selection(op: s.Op).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
            splitCase: static s => Split(wire: s.Wire, location: s.Location).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)),
            editCase: static e => Edit(wire: e.Wire, edit: e.Kind).Map(static delta => (WireResult)new WireResult.MutationCase(Delta: delta)));

    internal static GrasshopperUiIntent<WireResult> Query(WireQuery query) =>
        query.Switch(
            allCase: static _ => All().Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            selectedCase: static _ => Selected().Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            danglingCase: static _ => Dangling().Map(static wires => (WireResult)new WireResult.WiresCase(Wires: wires)),
            pickCase: static p => Pick(point: p.Point).Map(static wire => (WireResult)new WireResult.WireCase(Wire: wire)),
            graphCase: static g => Graph(startParameterId: g.StartParameterId, direction: g.Direction, maxHops: g.MaxHops).Map(static graph => (WireResult)new WireResult.GraphCase(Graph: graph)));

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
        op.Switch(
            selectCase: static s => MutateWire(op: Op.Of(name: "Wire.Select"), wire: s.Wire, run: static (objs, ends) => objs.SelectWire(wire: ends)),
            deselectCase: static d => MutateWire(op: Op.Of(name: "Wire.Deselect"), wire: d.Wire, run: static (objs, ends) => objs.DeselectWire(wire: ends)),
            deselectAllCase: static _ => MutateDeselectAll(op: Op.Of(name: "Wire.DeselectAll")));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Split(WireSnapshot.ConnectedCase wire, PointF location) =>
        GrasshopperUi.Mutate(
            op: Op.Of(name: nameof(Split)),
            undo: UndoStrategy.Auto,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from valid in Op.Of(name: nameof(Split)).AcceptPoint(value: location, detail: "non-finite location")
                from methods in scope.NeedMethods()
                from objs in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"source param {wire.Source} not found"))
                from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"target param {wire.Target} not found"))
                from connected in RequireConnected(objects: objs, source: source, target: target, op: Op.Of(name: nameof(Split)))
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
            undo: UndoStrategy.Auto,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from validEdit in Optional(edit).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: "wire edit is required"))
                from methods in scope.NeedMethods()
                from objs in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: $"source param {wire.Source} not found"))
                from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Edit)), detail: $"target param {wire.Target} not found"))
                let actions = ActionList.Empty
                from changed in validEdit.Apply(methods: methods, objects: objs, source: source, target: target, actions: actions)
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
        bool connected = IsConnected(objects: objects, wire: wire);
        return new WireSnapshot.ConnectedCase(
            Source: wire.Source, Target: wire.Target,
            SourceResolved: source is not null, TargetResolved: target is not null,
            Connected: connected,
            Selected: connected && objects.IsWireSelected(wire: wire));
    }

    internal static bool IsConnected(GhObjectList objects, WireEnds wire) =>
        Optional(objects.AllWires)
            .Map(wires => wires.Any(w => w.Source == wire.Source && w.Target == wire.Target))
            .IfNone(noneValue: false);

    internal static Fin<Unit> RequireConnected(GhObjectList objects, IParameter source, IParameter target, Op op) =>
        IsConnected(objects: objects, wire: new WireEnds(source: source.InstanceId, target: target.InstanceId))
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"));

    // ObjectList.SearchUpstream/SearchDownstream is GH2-native cycle-safe depth-first-without-duplicates
    // keyed by the source parameter (narrower than Connectivity's owner-keyed walk). Distinct() guards
    // the Bidirectional case where Up/Down sequences may overlap at shared subgraphs.
    internal static Fin<Seq<IDocumentObject>> TraverseObjects(GhObjectList objects, Guid startParameterId, WireTraversal direction) =>
        Optional(objects.FindParameter(instanceId: startParameterId))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TraverseObjects)), detail: $"parameter {startParameterId} not found"))
            .Map(parameter => toSeq(direction.Search(objects: objects, parameter: parameter)).Distinct());

    // Common selection-mutation scaffolding: NeedObjects/NeedDocument, optional precheck, native run via
    // op.Attempt, then a (before, after) → changed projection. Centralizes the rail so MutateWire and
    // MutateDeselectAll share one capsule. `precheck` returns Fin<Unit> for guard predicates; on Succ the
    // before-metric snapshots, the native body runs, and the changed-delta projection computes the result.
    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateMetric(
        Op op,
        Func<GhObjectList, Fin<Unit>> precheck,
        Func<GhObjectList, int> metric,
        Func<GhObjectList, Fin<Unit>> action,
        Func<int, int, int> delta) =>
            GrasshopperUi.Mutate(
                op: op,
                undo: UndoStrategy.Auto,
                repaint: RepaintRequest.Canvas,
                mutate: scope =>
                    from objects in scope.NeedObjects()
                    from doc in scope.NeedDocument()
                    from _gate in precheck(arg: objects)
                    let before = metric(arg: objects)
                    from _ran in action(arg: objects)
                    let after = metric(arg: objects)
                    select new DocumentMutationDelta(Changed: delta(arg1: before, arg2: after), After: UiRail.DocumentSnapshotOf(document: doc, objects: objects)));

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateWire(
        Op op,
        WireSnapshot.ConnectedCase wire,
        Func<GhObjectList, WireEnds, bool> run) {
        WireEnds ends = new(source: wire.Source, target: wire.Target);
        return MutateMetric(
            op: op,
            precheck: objects => Optional(IsConnected(objects: objects, wire: ends))
                .Filter(static live => live)
                .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "wire is not currently connected"))
                .Map(static _ => unit),
            metric: objects => objects.IsWireSelected(wire: ends) ? 1 : 0,
            action: objects => op.Attempt(body: () => run(arg1: objects, arg2: ends), what: "wire selection").Map(static _ => unit),
            delta: static (before, after) => before == after ? 0 : 1);
    }

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateDeselectAll(Op op) =>
        MutateMetric(
            op: op,
            precheck: static _ => Fin.Succ(value: unit),
            metric: static objects => objects.SelectedWireCount,
            action: objects => op.Attempt(body: () => objects.DeselectAllWires(), what: "DeselectAllWires"),
            delta: static (before, after) => Math.Max(val1: 0, val2: before - after));

    private static (bool Changed, Option<Guid> Shout, Option<Guid> Listen) SplitWire(
        GhDocumentMethods methods, IParameter source, IParameter target, string name, PointF location, ActionList actions) {
        bool changed = methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: actions);
        return (Changed: changed,
            Shout: Optional(shout?.InstanceId).Filter(static g => g != Guid.Empty),
            Listen: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty));
    }

    private static WireGraph GraphOf(GhObjectList objects, Seq<IDocumentObject> graphObjects, Guid startParameterId, int maxHops) {
        Seq<Guid> visited = (Seq(startParameterId) + toSeq(graphObjects.Choose(static obj => Optional(obj as IParameter)).Take(count: maxHops).Select(static parameter => parameter.InstanceId))).Distinct();
        // O(1) lookup avoids O(N*M) Seq.Exists when filtering wires against the visited frontier.
        LanguageExt.HashSet<Guid> visitedSet = toHashSet(visited);
        Seq<WireSnapshot.ConnectedCase> wires = SafeWires(source: objects.AllWires, objects: objects)
            .Filter(wire => visitedSet.Find(key: wire.Source).IsSome && visitedSet.Find(key: wire.Target).IsSome);
        return new WireGraph(Wires: wires, Visited: visited);
    }
}

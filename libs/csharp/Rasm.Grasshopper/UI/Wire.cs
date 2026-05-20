using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
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

public enum WireTraversal { Upstream, Downstream, Bidirectional }

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireGraph(
    Seq<WireSnapshot.ConnectedCase> Wires,
    Seq<Guid> Visited,
    bool CycleDetected,
    Option<Seq<Guid>> Cycle);

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Wire {
    public static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> All() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs)));

    public static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Selected() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.SelectedWires, objects: objs)));

    public static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Dangling() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs).Filter(static w => !w.Connected)));

    public static GrasshopperUiIntent<WireSnapshot> Pick(PointF point) =>
        IntentFactory.Document<WireSnapshot>(run: scope =>
            from pickResult in Canvas.Pick(point: point).Run(scope: scope)
            from objs in scope.NeedObjects()
            select pickResult.WireUnderPick.Match(
                Some: wireEnds => (WireSnapshot)SnapshotConnected(objects: objs, wire: wireEnds),
                None: () => WireSnapshot.Absent));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Selection(WireSelectionOp op) =>
        op switch {
            WireSelectionOp.SelectCase s => MutateWire(op: Op.Of(name: "Wire.Select"), wire: s.Wire, run: static (objs, ends) => objs.SelectWire(wire: ends)),
            WireSelectionOp.DeselectCase d => MutateWire(op: Op.Of(name: "Wire.Deselect"), wire: d.Wire, run: static (objs, ends) => objs.DeselectWire(wire: ends)),
            WireSelectionOp.DeselectAllCase => MutateDeselectAll(op: Op.Of(name: "Wire.DeselectAll")),
            _ => IntentFactory.Document<Snapshot<DocumentMutationDelta>>(run: _ => Fin.Fail<Snapshot<DocumentMutationDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "unknown WireSelectionOp"))),
        };

    public static GrasshopperUiIntent<Snapshot<WireSplitDelta>> Split(WireSnapshot.ConnectedCase wire, string name, PointF location) =>
        Optional((Name: name, Location: location))
            .Filter(static s => !string.IsNullOrWhiteSpace(s.Name) && float.IsFinite(s.Location.X) && float.IsFinite(s.Location.Y))
            .Match(
                Some: valid => GrasshopperUi.Mutate<WireSplitDelta>(
                    op: Op.Of(name: nameof(Split)),
                    undo: UndoStrategy.GhBuiltIn,
                    repaint: RepaintRequest.Canvas,
                    mutate: scope =>
                        from methods in scope.NeedMethods()
                        from objs in scope.NeedObjects()
                        from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"source param {wire.Source} not found"))
                        from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"target param {wire.Target} not found"))
                        let split = SplitWire(methods: methods, source: source, target: target, name: valid.Name, location: valid.Location)
                        select new WireSplitDelta(Changed: split.Changed, Wire: wire, Shout: split.Shout, Listen: split.Listen)),
                None: () => IntentFactory.Document<Snapshot<WireSplitDelta>>(run: _ => Fin.Fail<Snapshot<WireSplitDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: "empty name or non-finite location"))));

    public static GrasshopperUiIntent<WireGraph> Graph(Guid startParameterId, WireTraversal direction = WireTraversal.Bidirectional, int maxHops = 32) =>
        IntentFactory.Document<WireGraph>(run: scope =>
            scope.NeedObjects().Bind(objs => Optional(objs.FindParameter(instanceId: startParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Graph)), detail: $"parameter {startParameterId} not found"))
                .Map(start => TraverseGraph(objects: objs, start: start, direction: direction, maxHops: maxHops))));

    // --- [OPERATIONS] ----------------------------------------------------------------------
    // W-001 fix: explicit null guard before toSeq — protects against GH2 returning null AllWires/SelectedWires.
    private static Seq<WireSnapshot.ConnectedCase> SafeWires(IEnumerable<WireEnds>? source, GhObjectList objects) =>
        source is null
            ? Seq<WireSnapshot.ConnectedCase>()
            : toSeq(source).Map(wire => SnapshotConnected(objects: objects, wire: wire));

    internal static WireSnapshot.ConnectedCase SnapshotConnected(GhObjectList objects, WireEnds wire) {
        IParameter? source = objects.FindParameter(instanceId: wire.Source);
        IParameter? target = objects.FindParameter(instanceId: wire.Target);
        bool connected = source is not null && target is not null && target.Inputs.IndexOf(wire.Source) >= 0;
        return new WireSnapshot.ConnectedCase(
            Source: wire.Source, Target: wire.Target,
            SourceResolved: source is not null, TargetResolved: target is not null,
            Connected: connected,
            Selected: connected && objects.IsWireSelected(wire: wire));
    }

    internal static bool IsConnected(GhObjectList objects, WireEnds wire) =>
        objects.FindParameter(instanceId: wire.Source) is not null
        && objects.FindParameter(instanceId: wire.Target) is { } target
        && target.Inputs.IndexOf(wire.Source) >= 0;

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateWire(
        Op op,
        WireSnapshot.ConnectedCase wire,
        Func<GhObjectList, WireEnds, bool> run) =>
            GrasshopperUi.Mutate<DocumentMutationDelta>(
                op: op,
                undo: UndoStrategy.GhBuiltIn,
                repaint: RepaintRequest.Canvas,
                mutate: scope =>
                    from objects in scope.NeedObjects()
                    from doc in scope.NeedDocument()
                    let succeeded = run(arg1: objects, arg2: new WireEnds(source: wire.Source, target: wire.Target))
                    from changed in succeeded
                        ? Fin.Succ(value: 1)
                        : Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: $"objects.{op} returned false"))
                    select new DocumentMutationDelta(Changed: changed, After: Document.SnapshotOf(document: doc, objects: objects)));

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateDeselectAll(Op op) =>
        GrasshopperUi.Mutate<DocumentMutationDelta>(
            op: op,
            undo: UndoStrategy.GhBuiltIn,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from objects in scope.NeedObjects()
                from doc in scope.NeedDocument()
                let _ = Tap(unit, _ => objects.DeselectAllWires())
                select new DocumentMutationDelta(Changed: 1, After: Document.SnapshotOf(document: doc, objects: objects)));

    private static (bool Changed, Option<Guid> Shout, Option<Guid> Listen) SplitWire(
        GhDocumentMethods methods, IParameter source, IParameter target, string name, PointF location) {
        bool changed = methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: null);
        return (Changed: changed,
            Shout: Optional(shout?.InstanceId).Filter(static g => g != Guid.Empty),
            Listen: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty));
    }

    // BOUNDARY ADAPTER — graph BFS with mutable visited set; algorithmic clarity preferred over fold-with-set-state.
    [BoundaryAdapter]
    private static WireGraph TraverseGraph(GhObjectList objects, IParameter start, WireTraversal direction, int maxHops) {
        HashSet<Guid> visited = [];
        Queue<(IParameter Node, int Hops)> frontier = new();
        frontier.Enqueue(item: (Node: start, Hops: 0));
        Seq<WireSnapshot.ConnectedCase> edges = Seq<WireSnapshot.ConnectedCase>();
        Option<Seq<Guid>> cycle = Option<Seq<Guid>>.None;
        while (frontier.Count > 0) {
            (IParameter node, int hops) = frontier.Dequeue();
            if (!visited.Add(item: node.InstanceId)) {
                cycle = Some(toSeq(visited));
                continue;
            }
            if (hops >= maxHops) continue;
            IEnumerable<IDocumentObject> neighbours = direction switch {
                WireTraversal.Upstream => objects.SearchUpstream(parameter: node),
                WireTraversal.Downstream => objects.SearchDownstream(parameter: node),
                _ => objects.SearchUpstream(parameter: node).Concat(objects.SearchDownstream(parameter: node)),
            };
            foreach (IDocumentObject neighbour in neighbours) {
                if (neighbour is IParameter neighbourParam && !visited.Contains(item: neighbourParam.InstanceId)) {
                    frontier.Enqueue(item: (Node: neighbourParam, Hops: hops + 1));
                    Option<WireEnds> edge = toSeq(objects.AllWires)
                        .Find(w => (w.Source == node.InstanceId && w.Target == neighbour.InstanceId)
                                   || (w.Source == neighbour.InstanceId && w.Target == node.InstanceId));
                    edge.IfSome(e => edges = edges.Add(SnapshotConnected(objects: objects, wire: e)));
                }
            }
        }
        return new WireGraph(
            Wires: edges.Distinct().ToSeq(),
            Visited: toSeq(visited),
            CycleDetected: cycle.IsSome,
            Cycle: cycle);
    }

    private static T Tap<T>(T value, System.Action<T> action) { action(obj: value); return value; }
}

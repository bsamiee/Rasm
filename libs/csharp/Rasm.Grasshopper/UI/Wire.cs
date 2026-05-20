using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
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
    Seq<Guid> Visited);

public abstract record WireRequest<T> : GhUiRequest<T> {
    public sealed record All : WireRequest<Seq<WireSnapshot.ConnectedCase>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<Seq<WireSnapshot.ConnectedCase>> Apply(GrasshopperUi.Scope scope) => Wire.All().Run(scope: scope);
    }
    public sealed record Selected : WireRequest<Seq<WireSnapshot.ConnectedCase>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<Seq<WireSnapshot.ConnectedCase>> Apply(GrasshopperUi.Scope scope) => Wire.Selected().Run(scope: scope);
    }
    public sealed record Dangling : WireRequest<Seq<WireSnapshot.ConnectedCase>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<Seq<WireSnapshot.ConnectedCase>> Apply(GrasshopperUi.Scope scope) => Wire.Dangling().Run(scope: scope);
    }
    public sealed record Pick(PointF Point) : WireRequest<WireSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<WireSnapshot> Apply(GrasshopperUi.Scope scope) => Wire.Pick(point: Point).Run(scope: scope);
    }
    public sealed record Selection(WireSelectionOp Op) : WireRequest<Snapshot<DocumentMutationDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas);
        internal override Fin<Snapshot<DocumentMutationDelta>> Apply(GrasshopperUi.Scope scope) => Wire.Selection(op: Op).Run(scope: scope);
    }
    public sealed record Split(WireSnapshot.ConnectedCase Wire, string Name, PointF Location) : WireRequest<Snapshot<WireSplitDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas);
        internal override Fin<Snapshot<WireSplitDelta>> Apply(GrasshopperUi.Scope scope) => Rasm.Grasshopper.UI.Wire.Split(Wire, Name, Location).Run(scope: scope);
    }
    public sealed record Graph(Guid StartParameterId, WireTraversal Direction = WireTraversal.Bidirectional, int MaxHops = 32) : WireRequest<WireGraph> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<WireGraph> Apply(GrasshopperUi.Scope scope) => Wire.Graph(startParameterId: StartParameterId, direction: Direction, maxHops: MaxHops).Run(scope: scope);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class Wire {
    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> All() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs)));

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Selected() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.SelectedWires, objects: objs)));

    internal static GrasshopperUiIntent<Seq<WireSnapshot.ConnectedCase>> Dangling() =>
        IntentFactory.Document<Seq<WireSnapshot.ConnectedCase>>(run: scope =>
            scope.NeedObjects().Map(objs => SafeWires(source: objs.AllWires, objects: objs).Filter(static w => !w.Connected)));

    internal static GrasshopperUiIntent<WireSnapshot> Pick(PointF point) =>
        IntentFactory.Document<WireSnapshot>(run: scope =>
            from pickResult in UiRail.CanvasDispatch(scope: scope, op: CanvasOp.Pick(point: point))
                .Bind(static result => result switch {
                    CanvasResult.PickResult pick => Fin.Succ(value: pick.Pick),
                    _ => Fin.Fail<CanvasPickSnapshot>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Pick)), detail: "canvas pick did not return a pick result")),
                })
            from objs in scope.NeedObjects()
            select pickResult.WireUnderPick.Match(
                Some: wireEnds => (WireSnapshot)SnapshotConnected(objects: objs, wire: wireEnds),
                None: () => WireSnapshot.Absent));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Selection(WireSelectionOp op) =>
        op switch {
            WireSelectionOp.SelectCase s => MutateWire(op: Op.Of(name: "Wire.Select"), wire: s.Wire, run: static (objs, ends) => objs.SelectWire(wire: ends)),
            WireSelectionOp.DeselectCase d => MutateWire(op: Op.Of(name: "Wire.Deselect"), wire: d.Wire, run: static (objs, ends) => objs.DeselectWire(wire: ends)),
            WireSelectionOp.DeselectAllCase => MutateDeselectAll(op: Op.Of(name: "Wire.DeselectAll")),
            _ => IntentFactory.Document<Snapshot<DocumentMutationDelta>>(run: _ => Fin.Fail<Snapshot<DocumentMutationDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "unknown WireSelectionOp"))),
        };

    internal static GrasshopperUiIntent<Snapshot<WireSplitDelta>> Split(WireSnapshot.ConnectedCase wire, string name, PointF location) =>
        Optional((Name: name, Location: location))
            .Filter(static s => !string.IsNullOrWhiteSpace(s.Name) && float.IsFinite(s.Location.X) && float.IsFinite(s.Location.Y))
            .Match(
                Some: valid => GrasshopperUi.Mutate<WireSplitDelta>(
                    op: Op.Of(name: nameof(Split)),
                    undo: UndoStrategy.None,
                    repaint: RepaintRequest.Canvas,
                    mutate: scope =>
                        from methods in scope.NeedMethods()
                        from objs in scope.NeedObjects()
                        from doc in scope.NeedDocument()
                        from source in Optional(objs.FindParameter(instanceId: wire.Source)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"source param {wire.Source} not found"))
                        from target in Optional(objs.FindParameter(instanceId: wire.Target)).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: $"target param {wire.Target} not found"))
                        let actions = new ActionList([])
                        let split = SplitWire(methods: methods, source: source, target: target, name: valid.Name, location: valid.Location, actions: actions)
                        from _ in UiRail.CommitActions(document: doc, op: Op.Of(name: nameof(Split)), actions: actions)
                        select new WireSplitDelta(Changed: split.Changed, Wire: wire, Shout: split.Shout, Listen: split.Listen)),
                None: () => IntentFactory.Document<Snapshot<WireSplitDelta>>(run: _ => Fin.Fail<Snapshot<WireSplitDelta>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: "empty name or non-finite location"))));

    internal static GrasshopperUiIntent<WireGraph> Graph(Guid startParameterId, WireTraversal direction = WireTraversal.Bidirectional, int maxHops = 32) =>
        IntentFactory.Document<WireGraph>(run: scope =>
            scope.NeedObjects().Bind(objs => Optional(objs.FindParameter(instanceId: startParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Graph)), detail: $"parameter {startParameterId} not found"))
                .Map(start => TraverseGraph(objects: objs, start: start, direction: direction, maxHops: maxHops))));

    // --- [OPERATIONS] ----------------------------------------------------------------------
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
                    select new DocumentMutationDelta(Changed: changed, After: UiRail.DocumentSnapshotOf(document: doc, objects: objects)));

    private static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateDeselectAll(Op op) =>
        GrasshopperUi.Mutate<DocumentMutationDelta>(
            op: op,
            undo: UndoStrategy.GhBuiltIn,
            repaint: RepaintRequest.Canvas,
            mutate: scope =>
                from objects in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from cleared in Try.lift<Unit>(f: () => { objects.DeselectAllWires(); return unit; })
                    .Run()
                    .MapFail(_ => UiFault.MutationRejected(op: op, detail: "DeselectAllWires threw"))
                select new DocumentMutationDelta(Changed: 1, After: UiRail.DocumentSnapshotOf(document: doc, objects: objects)));

    private static (bool Changed, Option<Guid> Shout, Option<Guid> Listen) SplitWire(
        GhDocumentMethods methods, IParameter source, IParameter target, string name, PointF location, ActionList actions) {
        bool changed = methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: actions);
        return (Changed: changed,
            Shout: Optional(shout?.InstanceId).Filter(static g => g != Guid.Empty),
            Listen: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty));
    }

    private static WireGraph TraverseGraph(GhObjectList objects, IParameter start, WireTraversal direction, int maxHops) {
        Seq<Guid> visited = Seq(start.InstanceId) + toSeq((direction switch {
            WireTraversal.Upstream => objects.SearchUpstream(parameter: start),
            WireTraversal.Downstream => objects.SearchDownstream(parameter: start),
            _ => objects.SearchUpstream(parameter: start).Concat(objects.SearchDownstream(parameter: start)),
        }).OfType<IParameter>().Take(count: Math.Max(0, maxHops)).Select(static parameter => parameter.InstanceId)).Distinct().ToSeq();
        Seq<WireSnapshot.ConnectedCase> wires = SafeWires(source: objects.AllWires, objects: objects)
            .Filter(wire => visited.Exists(id => id == wire.Source) && visited.Exists(id => id == wire.Target));
        return new WireGraph(Wires: wires, Visited: visited);
    }
}

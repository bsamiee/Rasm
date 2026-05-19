using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;

namespace Rasm.Grasshopper.UI;

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireSnapshot(Guid Source, Guid Target, bool SourceResolved, bool TargetResolved, bool Selected);

[StructLayout(LayoutKind.Auto)]
public readonly record struct WireSplitSnapshot(bool Changed, WireSnapshot Wire, Option<Guid> Shout, Option<Guid> Listen);

// --- [INTENTS] --------------------------------------------------------------------------
public static class WireIntent {
    public static GrasshopperUiIntent<Seq<WireSnapshot>> All() =>
        new(
            run: scope => scope.Objects
                .ToFin(Fail: Op.Of(name: nameof(All)).InvalidInput())
                .Map(objects => toSeq(objects.AllWires).Choose(wire => ResolvedSnapshotOf(objects: objects, wire: wire))),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<Seq<WireSnapshot>> Selected() =>
        new(
            run: scope => scope.Objects
                .ToFin(Fail: Op.Of(name: nameof(Selected)).InvalidInput())
                .Map(objects => toSeq(objects.SelectedWires).Choose(wire => ResolvedSnapshotOf(objects: objects, wire: wire))),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<WireSnapshot> Pick(PointF point) =>
        new(
            run: scope =>
                from pick in CanvasIntent.Pick(point: point, policy: CanvasPickPolicy.All).Run(scope: scope)
                from wire in pick.WireUnderPick.ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidResult())
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
                from snapshot in ResolvedSnapshotOf(objects: objects, wire: new WireEnds(source: wire.Source, target: wire.Target)).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidResult())
                select snapshot,
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<Unit> SelectWire(WireSnapshot wire) =>
        Mutate(name: nameof(SelectWire), wire: wire, run: static (objects, found) => objects.SelectWire(wire: found));

    public static GrasshopperUiIntent<Unit> DeselectWire(WireSnapshot wire) =>
        Mutate(name: nameof(DeselectWire), wire: wire, run: static (objects, found) => objects.DeselectWire(wire: found));

    public static GrasshopperUiIntent<Unit> DeselectAll() =>
        new(
            run: scope => scope.Objects
                .ToFin(Fail: Op.Of(name: nameof(DeselectAll)).InvalidInput())
                .Map(objects => {
                    objects.DeselectAllWires();
                    return unit;
                }),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<WireSplitSnapshot> Split(WireSnapshot wire, string name, PointF location) =>
        new(
            run: scope => Optional((Name: name, Location: location))
                .Filter(static item => !string.IsNullOrWhiteSpace(item.Name) && float.IsFinite(item.Location.X) && float.IsFinite(item.Location.Y))
                .ToFin(Fail: Op.Of(name: nameof(Split)).InvalidInput())
                .Bind(validInput =>
                from valid in Rewire(scope: scope, wire: wire, name: nameof(Split))
                from methods in scope.Methods.ToFin(Fail: Op.Of(name: nameof(Split)).InvalidInput())
                select Split(methods: methods, wire: valid.Snapshot, source: valid.Source, target: valid.Target, name: validInput.Name, location: validInput.Location)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static GrasshopperUiIntent<Unit> Mutate(string name, WireSnapshot wire, Func<ObjectList, WireEnds, bool> run) =>
        new(
            run: scope =>
                from found in Rewire(scope: scope, wire: wire, name: name)
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from changed in Mutated(objects: objects, wire: found.Wire, run: run, name: name)
                select changed,
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static Fin<(IParameter Source, IParameter Target, WireEnds Wire, WireSnapshot Snapshot)> Rewire(GrasshopperUi.Scope scope, WireSnapshot wire, string name) =>
        from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
        from source in Optional(objects.FindParameter(instanceId: wire.Source)).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from target in Optional(objects.FindParameter(instanceId: wire.Target)).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from ends in Optional(new WireEnds(source: wire.Source, target: wire.Target))
            .Filter(found => target.Inputs.IndexOf(found.Source) >= 0)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
        from snapshot in ResolvedSnapshotOf(objects: objects, wire: ends).ToFin(Fail: Op.Of(name: name).InvalidInput())
        select (Source: source, Target: target, Wire: ends, Snapshot: snapshot);

    private static Option<WireSnapshot> ResolvedSnapshotOf(ObjectList objects, WireEnds wire) =>
        Optional((Source: objects.FindParameter(instanceId: wire.Source), Target: objects.FindParameter(instanceId: wire.Target)))
            .Filter(found => found.Source is not null && found.Target is not null && found.Target.Inputs.IndexOf(wire.Source) >= 0)
            .Map(_ => new WireSnapshot(
                Source: wire.Source,
                Target: wire.Target,
                SourceResolved: true,
                TargetResolved: true,
                Selected: objects.IsWireSelected(wire: wire)));

    private static Fin<Unit> Mutated(ObjectList objects, WireEnds wire, Func<ObjectList, WireEnds, bool> run, string name) =>
        run(arg1: objects, arg2: wire) switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: name).InvalidResult()),
        };

    private static WireSplitSnapshot Split(DocumentMethods methods, WireSnapshot wire, IParameter source, IParameter target, string name, PointF location) {
        bool changed = methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: null);
        return changed ? new(Changed: true, Wire: wire, Shout: Optional(shout).Map(static item => item.InstanceId), Listen: Optional(listen).Map(static item => item.InstanceId)) : new(Changed: false, Wire: wire, Shout: None, Listen: None);
    }
}

using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;

namespace Rasm.Grasshopper.UI;

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentSnapshot(
    Guid Hash,
    bool Modified,
    int Modifications,
    int ObjectCount,
    int PinCount,
    int ExpiredCount,
    int SelectedObjectCount,
    int SelectedWireCount,
    int SelectedDanglingWireCount,
    int WireCount,
    int DanglingWireCount,
    RectangleF AttributeBounds,
    RectangleF PivotBounds,
    PointF ProjectionCentre,
    float ProjectionZoom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentObjectSnapshot(
    Guid Id,
    string Name,
    string DisplayName,
    bool Selected,
    string Activity,
    string Display,
    string Phase,
    string State,
    RectangleF Bounds,
    PointF Pivot);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentMutationSnapshot(int Changed, DocumentSnapshot Document);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentGripSnapshot(Guid Parameter, bool InletWithinRange, bool OutletWithinRange);

public enum DocumentObjectScope {
    Primary,
    PrimaryAndSecondary,
    Selected,
}

public enum DocumentGripKind {
    Inlet,
    Outlet,
    InletOrOutlet,
}

// --- [INTENTS] --------------------------------------------------------------------------
public static class DocumentIntent {
    public static GrasshopperUiIntent<DocumentSnapshot> Snapshot() =>
        new(
            run: scope => scope.Document
                .ToFin(Fail: Op.Of(name: nameof(Snapshot)).InvalidInput())
                .Bind(document => scope.Objects
                    .ToFin(Fail: Op.Of(name: nameof(Snapshot)).InvalidInput())
                    .Map(objects => SnapshotOf(document: document, objects: objects))),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<Seq<DocumentObjectSnapshot>> Objects(DocumentObjectScope scope = DocumentObjectScope.Primary) =>
        new(
            run: ui => ui.Objects
                .ToFin(Fail: Op.Of(name: nameof(Objects)).InvalidInput())
                .Map(objects => scope switch {
                    DocumentObjectScope.Primary => toSeq(objects.Forwards.Select(SnapshotObject)),
                    DocumentObjectScope.PrimaryAndSecondary => toSeq(objects.PrimaryAndSecondary.Select(SnapshotObject)),
                    DocumentObjectScope.Selected => toSeq(objects.SelectedObjects.Select(SnapshotObject)),
                    _ => Seq<DocumentObjectSnapshot>(),
                }),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<Option<DocumentObjectSnapshot>> Object(Guid id) =>
        new(
            run: scope => Optional(id)
                .Filter(static value => value != Guid.Empty)
                .ToFin(Fail: Op.Of(name: nameof(Object)).InvalidInput())
                .Bind(valid => scope.Objects
                    .ToFin(Fail: Op.Of(name: nameof(Object)).InvalidInput())
                    .Map(objects => Optional(objects.Find(instanceId: valid)).Map(SnapshotObject))),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<Option<DocumentGripSnapshot>> Grip(PointF point, DocumentGripKind kind = DocumentGripKind.InletOrOutlet) =>
        new(
            run: scope => Optional(point)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: Op.Of(name: nameof(Grip)).InvalidInput())
                .Bind(valid => scope.Objects
                    .ToFin(Fail: Op.Of(name: nameof(Grip)).InvalidInput())
                    .Map(objects => GripOf(objects: objects, point: valid, kind: kind))),
            policy: GrasshopperUiPolicy.Document());

    public static GrasshopperUiIntent<DocumentMutationSnapshot> DeleteSelection() =>
        Mutate(name: nameof(DeleteSelection), run: static methods => methods.DeleteSelection(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> DeleteSelectionData() =>
        Mutate(name: nameof(DeleteSelectionData), run: static methods => methods.DeleteSelectionData(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> SelectAll() =>
        Mutate(name: nameof(SelectAll), run: static methods => methods.SelectAll());

    public static GrasshopperUiIntent<DocumentMutationSnapshot> DeselectAll() =>
        Mutate(name: nameof(DeselectAll), run: static methods => methods.DeselectAll());

    public static GrasshopperUiIntent<DocumentMutationSnapshot> InvertSelection() =>
        Mutate(name: nameof(InvertSelection), run: static methods => methods.InvertSelection());

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelected() =>
        Mutate(name: nameof(ShowSelected), run: static methods => methods.ShowSelected(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelected() =>
        Mutate(name: nameof(HideSelected), run: static methods => methods.HideSelected(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ToggleDisplaySelected() =>
        Mutate(name: nameof(ToggleDisplaySelected), run: static methods => methods.ToggleDisplaySelected(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> EnableSelected() =>
        Mutate(name: nameof(EnableSelected), run: static methods => methods.EnableSelected(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelectedInputs() =>
        Mutate(name: nameof(ShowSelectedInputs), run: static methods => methods.ShowSelectedInputs(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelectedOutputs() =>
        Mutate(name: nameof(ShowSelectedOutputs), run: static methods => methods.ShowSelectedOutputs(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelectedInputs() =>
        Mutate(name: nameof(HideSelectedInputs), run: static methods => methods.HideSelectedInputs(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelectedOutputs() =>
        Mutate(name: nameof(HideSelectedOutputs), run: static methods => methods.HideSelectedOutputs(actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> CopySelection(ClipboardKind clipboard = ClipboardKind.Global) =>
        Mutate(name: nameof(CopySelection), clipboard: clipboard, run: static (methods, valid) => methods.CopySelection(clipboard: valid));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> CutSelection(ClipboardKind clipboard = ClipboardKind.Global) =>
        Mutate(name: nameof(CutSelection), clipboard: clipboard, run: static (methods, valid) => methods.CutSelection(clipboard: valid, actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> Paste(ClipboardKind clipboard = ClipboardKind.Global, PasteBehaviour behaviour = PasteBehaviour.Centre | PasteBehaviour.DeselectOldObjects | PasteBehaviour.SelectNewObjects) =>
        Mutate(name: nameof(Paste), clipboard: clipboard, run: (methods, valid) => methods.PasteFromClipboard(clipboard: valid, behaviour: behaviour, actions: null));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> PasteGrasshopper1Xml() =>
        Mutate(name: nameof(PasteGrasshopper1Xml), run: static methods => methods.PasteGrasshopper1XmlFromClipboard(actions: null) ? 1 : 0);

    public static GrasshopperUiIntent<Option<Guid>> GroupSelection(string name) =>
        new(
            run: scope => scope.Methods
                .ToFin(Fail: Op.Of(name: nameof(GroupSelection)).InvalidInput())
                .Map(methods => Optional(methods.GroupSelection(name: name, colour: null, actions: null)).Map(static group => group.InstanceId)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<Option<Guid>> ChainSelection() =>
        new(
            run: scope => scope.Methods
                .ToFin(Fail: Op.Of(name: nameof(ChainSelection)).InvalidInput())
                .Map(methods => Optional(methods.ChainSelection(actions: null)).Map(static chain => chain.InstanceId)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<Option<Guid>> ClusterSelection() =>
        new(
            run: scope => scope.Methods
                .ToFin(Fail: Op.Of(name: nameof(ClusterSelection)).InvalidInput())
                .Map(methods => Optional(methods.ClusterSelection(actions: null)).Map(static cluster => cluster.InstanceId)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> Drop(Guid proxyId, PointF pivot) =>
        new(
            run: scope => Optional((ProxyId: proxyId, Pivot: pivot))
                .Filter(static item => item.ProxyId != Guid.Empty && float.IsFinite(item.Pivot.X) && float.IsFinite(item.Pivot.Y))
                .ToFin(Fail: Op.Of(name: nameof(Drop)).InvalidInput())
                .Bind(valid =>
                    from methods in scope.Methods.ToFin(Fail: Op.Of(name: nameof(Drop)).InvalidInput())
                    from document in scope.Document.ToFin(Fail: Op.Of(name: nameof(Drop)).InvalidInput())
                    from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(Drop)).InvalidInput())
                    select new DocumentMutationSnapshot(Changed: methods.DropObject(obj: valid.ProxyId, location: valid.Pivot, actions: null) ? 1 : 0, Document: SnapshotOf(document: document, objects: objects))),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> AddDependency(PointF pivot, Listen dependency) =>
        new(
            run: scope => Optional((Pivot: pivot, Dependency: dependency))
                .Filter(static item => float.IsFinite(item.Pivot.X) && float.IsFinite(item.Pivot.Y))
                .ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                .Bind(valid =>
                    from methods in scope.Methods.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    from document in scope.Document.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    select AddedDependency(methods: methods, location: valid.Pivot, dependency: valid.Dependency, document: document, objects: objects)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static GrasshopperUiIntent<DocumentMutationSnapshot> Mutate(string name, Func<DocumentMethods, int> run) =>
        new(
            run: scope =>
                from methods in scope.Methods.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from document in scope.Document.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from changed in Changed(name: name, count: run(arg: methods))
                select new DocumentMutationSnapshot(Changed: changed, Document: SnapshotOf(document: document, objects: objects)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static GrasshopperUiIntent<DocumentMutationSnapshot> Mutate(string name, ClipboardKind clipboard, Func<DocumentMethods, ClipboardKind, bool> run) =>
        new(
            run: scope => Optional(clipboard)
                .Filter(static value => value != ClipboardKind.Instance)
                .ToFin(Fail: Op.Of(name: name).InvalidInput())
                .Bind(valid =>
                    from methods in scope.Methods.ToFin(Fail: Op.Of(name: name).InvalidInput())
                    from document in scope.Document.ToFin(Fail: Op.Of(name: name).InvalidInput())
                    from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
                    select new DocumentMutationSnapshot(Changed: run(arg1: methods, arg2: valid) ? 1 : 0, Document: SnapshotOf(document: document, objects: objects))),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static Fin<int> Changed(string name, int count) =>
        count switch {
            >= 0 => Fin.Succ(value: count),
            _ => Fin.Fail<int>(error: Op.Of(name: name).InvalidResult()),
        };

    private static DocumentSnapshot SnapshotOf(Document document, ObjectList objects) {
        Seq<WireEnds> allWires = toSeq(objects.AllWires);
        Seq<WireEnds> selectedWires = toSeq(objects.SelectedWires);
        int wireCount = allWires.Count(IsResolvedWire(objects: objects));
        int selectedWireCount = selectedWires.Count(IsResolvedWire(objects: objects));
        return new(
            Hash: document.Hash,
            Modified: document.Modified,
            Modifications: document.Modifications,
            ObjectCount: objects.Count,
            PinCount: objects.PinCount,
            ExpiredCount: objects.ExpiredCount,
            SelectedObjectCount: objects.SelectedCount,
            SelectedWireCount: selectedWireCount,
            SelectedDanglingWireCount: selectedWires.Count - selectedWireCount,
            WireCount: wireCount,
            DanglingWireCount: allWires.Count - wireCount,
            AttributeBounds: objects.AttributeBounds,
            PivotBounds: objects.PivotBounds,
            ProjectionCentre: document.Projection.centre,
            ProjectionZoom: document.Projection.zoom);
    }

    private static Func<WireEnds, bool> IsResolvedWire(ObjectList objects) =>
        wire => objects.FindParameter(instanceId: wire.Source) is not null && objects.FindParameter(instanceId: wire.Target) is { } target && target.Inputs.IndexOf(wire.Source) >= 0;

    private static Option<DocumentGripSnapshot> GripOf(ObjectList objects, PointF point, DocumentGripKind kind) =>
        kind switch {
            DocumentGripKind.Inlet => Optional(objects.FindByInlet(point: point)).Map(static parameter => new DocumentGripSnapshot(Parameter: parameter.InstanceId, InletWithinRange: true, OutletWithinRange: false)),
            DocumentGripKind.Outlet => Optional(objects.FindByOutlet(point: point)).Map(static parameter => new DocumentGripSnapshot(Parameter: parameter.InstanceId, InletWithinRange: false, OutletWithinRange: true)),
            DocumentGripKind.InletOrOutlet => Optional(objects.FindByInletOrOutlet(point: point)).Filter(static found => found.parameter is not null).Map(static found => new DocumentGripSnapshot(Parameter: found.parameter.InstanceId, InletWithinRange: found.inletWithinRange, OutletWithinRange: found.outletWithinRange)),
            _ => None,
        };

    private static DocumentMutationSnapshot AddedDependency(DocumentMethods methods, PointF location, Listen dependency, Document document, ObjectList objects) {
        methods.AddDependency(location: location, listen: dependency, actions: null);
        return new(Changed: 1, Document: SnapshotOf(document: document, objects: objects));
    }

    private static DocumentObjectSnapshot SnapshotObject(IDocumentObject obj) {
        obj.Attributes.Layout(Grasshopper2.UI.Skinning.Shape.Default);
        return new(
            Id: obj.InstanceId,
            Name: obj.Nomen.Name,
            DisplayName: obj.DisplayName,
            Selected: obj.Selected,
            Activity: obj.Activity.ToString(),
            Display: obj.Display.ToString(),
            Phase: obj.Phase.ToString(),
            State: obj.State.ToString(),
            Bounds: obj.Attributes.Bounds,
            Pivot: obj.Attributes.Pivot);
    }
}

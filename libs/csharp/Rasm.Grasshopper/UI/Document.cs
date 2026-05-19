using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using GhColour = Grasshopper2.Types.Colour.Colour;

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
        Mutate(name: nameof(DeleteSelection), run: static methods => Fin.Succ(value: methods.DeleteSelection(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> DeleteSelectionData() =>
        Mutate(name: nameof(DeleteSelectionData), run: static methods => Fin.Succ(value: methods.DeleteSelectionData(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> SelectAll() =>
        Mutate(name: nameof(SelectAll), run: static methods => Fin.Succ(value: methods.SelectAll()));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> DeselectAll() =>
        Mutate(name: nameof(DeselectAll), run: static methods => Fin.Succ(value: methods.DeselectAll()));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> InvertSelection() =>
        Mutate(name: nameof(InvertSelection), run: static methods => Fin.Succ(value: methods.InvertSelection()));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelected() =>
        Mutate(name: nameof(ShowSelected), run: static methods => Fin.Succ(value: methods.ShowSelected(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelected() =>
        Mutate(name: nameof(HideSelected), run: static methods => Fin.Succ(value: methods.HideSelected(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ToggleDisplaySelected() =>
        Mutate(name: nameof(ToggleDisplaySelected), run: static methods => Fin.Succ(value: methods.ToggleDisplaySelected(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> EnableSelected() =>
        Mutate(name: nameof(EnableSelected), run: static methods => Fin.Succ(value: methods.EnableSelected(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> SetSelectedColourOverride(GhColour? colour = null) =>
        Mutate(name: nameof(SetSelectedColourOverride), run: methods => Fin.Succ(value: methods.SetColourOverrideSelected(colour: colour, actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelectedInputs() =>
        Mutate(name: nameof(ShowSelectedInputs), run: static methods => Fin.Succ(value: methods.ShowSelectedInputs(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> ShowSelectedOutputs() =>
        Mutate(name: nameof(ShowSelectedOutputs), run: static methods => Fin.Succ(value: methods.ShowSelectedOutputs(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelectedInputs() =>
        Mutate(name: nameof(HideSelectedInputs), run: static methods => Fin.Succ(value: methods.HideSelectedInputs(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> HideSelectedOutputs() =>
        Mutate(name: nameof(HideSelectedOutputs), run: static methods => Fin.Succ(value: methods.HideSelectedOutputs(actions: null)));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> CopySelection(ClipboardKind clipboard = ClipboardKind.Global) =>
        Mutate(name: nameof(CopySelection), run: methods => ValidClipboard(name: nameof(CopySelection), clipboard: clipboard).Map(valid => methods.CopySelection(clipboard: valid) ? 1 : 0));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> CutSelection(ClipboardKind clipboard = ClipboardKind.Global) =>
        Mutate(name: nameof(CutSelection), run: methods => ValidClipboard(name: nameof(CutSelection), clipboard: clipboard).Map(valid => methods.CutSelection(clipboard: valid, actions: null) ? 1 : 0));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> Paste(ClipboardKind clipboard = ClipboardKind.Global, PasteBehaviour behaviour = PasteBehaviour.Centre | PasteBehaviour.DeselectOldObjects | PasteBehaviour.SelectNewObjects) =>
        Mutate(name: nameof(Paste), run: methods => ValidClipboard(name: nameof(Paste), clipboard: clipboard).Map(valid => methods.PasteFromClipboard(clipboard: valid, behaviour: behaviour, actions: null) ? 1 : 0));

    public static GrasshopperUiIntent<DocumentMutationSnapshot> PasteGrasshopper1Xml() =>
        Mutate(name: nameof(PasteGrasshopper1Xml), run: static methods => Fin.Succ(value: methods.PasteGrasshopper1XmlFromClipboard(actions: null) ? 1 : 0));

    public static GrasshopperUiIntent<Option<Guid>> GroupSelection(string name) =>
        new(
            run: scope =>
                from validName in Optional(name)
                    .Filter(static value => !string.IsNullOrWhiteSpace(value))
                    .ToFin(Fail: Op.Of(name: nameof(GroupSelection)).InvalidInput())
                from methods in scope.Methods.ToFin(Fail: Op.Of(name: nameof(GroupSelection)).InvalidInput())
                select Optional(methods.GroupSelection(name: validName, colour: null, actions: null)).Map(static item => item.InstanceId),
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
                .Filter(static item => float.IsFinite(item.Pivot.X) && float.IsFinite(item.Pivot.Y) && item.Dependency is not null)
                .ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                .Bind(valid =>
                    from methods in scope.Methods.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    from document in scope.Document.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(AddDependency)).InvalidInput())
                    select (Methods: methods, Document: document, Objects: objects, Input: valid))
                .Map(state => {
                    state.Methods.AddDependency(location: state.Input.Pivot, listen: state.Input.Dependency, actions: null);
                    return new DocumentMutationSnapshot(Changed: 1, Document: SnapshotOf(document: state.Document, objects: state.Objects));
                }),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static GrasshopperUiIntent<DocumentMutationSnapshot> Mutate(string name, Func<DocumentMethods, Fin<int>> run) =>
        new(
            run: scope =>
                from validRun in Optional(run).ToFin(Fail: Op.Of(name: name).InvalidInput())
                from methods in scope.Methods.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from document in scope.Document.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: name).InvalidInput())
                from changed in validRun(arg: methods).Bind(count => count switch {
                    >= 0 => Fin.Succ(value: count),
                    _ => Fin.Fail<int>(error: Op.Of(name: name).InvalidResult()),
                })
                select new DocumentMutationSnapshot(Changed: changed, Document: SnapshotOf(document: document, objects: objects)),
            policy: GrasshopperUiPolicy.Document(repaint: true));

    private static Fin<ClipboardKind> ValidClipboard(string name, ClipboardKind clipboard) =>
        Optional(clipboard)
            .Filter(static value => value != ClipboardKind.Instance)
            .ToFin(Fail: Op.Of(name: name).InvalidInput());

    private static DocumentSnapshot SnapshotOf(Document document, ObjectList objects) {
        Seq<WireEnds> allWires = toSeq(objects.AllWires);
        Seq<WireEnds> selectedWires = toSeq(objects.SelectedWires);
        int wireCount = allWires.Count(wire => WireSnapshot.IsConnected(objects: objects, wire: wire));
        int selectedWireCount = selectedWires.Count(wire => WireSnapshot.IsConnected(objects: objects, wire: wire));
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

    private static Option<DocumentGripSnapshot> GripOf(ObjectList objects, PointF point, DocumentGripKind kind) =>
        kind switch {
            DocumentGripKind.Inlet => Optional(objects.FindByInlet(point: point)).Map(static parameter => new DocumentGripSnapshot(Parameter: parameter.InstanceId, InletWithinRange: true, OutletWithinRange: false)),
            DocumentGripKind.Outlet => Optional(objects.FindByOutlet(point: point)).Map(static parameter => new DocumentGripSnapshot(Parameter: parameter.InstanceId, InletWithinRange: false, OutletWithinRange: true)),
            DocumentGripKind.InletOrOutlet => Optional(objects.FindByInletOrOutlet(point: point)).Filter(static found => found.parameter is not null).Map(static found => new DocumentGripSnapshot(Parameter: found.parameter.InstanceId, InletWithinRange: found.inletWithinRange, OutletWithinRange: found.outletWithinRange)),
            _ => None,
        };

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

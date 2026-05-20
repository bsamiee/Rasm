using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Special;
using GhColour = Grasshopper2.Types.Colour.Colour;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GhOpenColorFamily = Eto.Drawing.OpenColor.Family;
using Op = Rasm.Domain.Op;
using SysHashSet = System.Collections.Generic.HashSet<System.Guid>;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
public enum VisibilityChange { Show, Hide, Toggle }

[StructLayout(LayoutKind.Auto)]
public readonly record struct IsolateOptions(bool IncludePins = true, bool IncludeInputs = true, bool IncludeOutputs = true);

[Union]
public partial record SelectionOp {
    private SelectionOp() { }
    public sealed record AllCase : SelectionOp;
    public sealed record NoneCase : SelectionOp;
    public sealed record InvertCase : SelectionOp;
    public sealed record DeleteCase(bool DataOnly = false) : SelectionOp;
    public sealed record VisibilityCase(VisibilityChange Change) : SelectionOp;
    public sealed record EnableCase : SelectionOp;

    public static readonly SelectionOp All = new AllCase();
    public static readonly SelectionOp None = new NoneCase();
    public static readonly SelectionOp Invert = new InvertCase();
    public static SelectionOp Delete(bool dataOnly = false) => new DeleteCase(DataOnly: dataOnly);
    public static readonly SelectionOp Show = new VisibilityCase(Change: VisibilityChange.Show);
    public static readonly SelectionOp Hide = new VisibilityCase(Change: VisibilityChange.Hide);
    public static readonly SelectionOp ToggleDisplay = new VisibilityCase(Change: VisibilityChange.Toggle);
    public static readonly SelectionOp Enable = new EnableCase();
}

[Union]
public partial record DisplayOp {
    private DisplayOp() { }
    public sealed record InputsCase(VisibilityChange Change) : DisplayOp;
    public sealed record OutputsCase(VisibilityChange Change) : DisplayOp;
    public static DisplayOp Inputs(VisibilityChange change) => new InputsCase(Change: change);
    public static DisplayOp Outputs(VisibilityChange change) => new OutputsCase(Change: change);
}

[Union]
public partial record ClipboardOp {
    private ClipboardOp() { }
    public sealed record CopyCase(ClipboardKind Kind) : ClipboardOp;
    public sealed record CutCase(ClipboardKind Kind) : ClipboardOp;
    public sealed record PasteCase(ClipboardKind Kind, PasteBehaviour Behaviour) : ClipboardOp;
    public sealed record PasteGh1XmlCase : ClipboardOp;

    public static ClipboardOp Copy(ClipboardKind kind = ClipboardKind.Global) => new CopyCase(Kind: kind);
    public static ClipboardOp Cut(ClipboardKind kind = ClipboardKind.Global) => new CutCase(Kind: kind);
    public static ClipboardOp Paste(ClipboardKind kind = ClipboardKind.Global, PasteBehaviour behaviour = PasteBehaviour.Centre | PasteBehaviour.DeselectOldObjects | PasteBehaviour.SelectNewObjects) =>
        new PasteCase(Kind: kind, Behaviour: behaviour);
    public static readonly ClipboardOp PasteGh1Xml = new PasteGh1XmlCase();
}

[Union]
public partial record ComposeOp {
    private ComposeOp() { }
    public sealed record GroupCase(Option<string> Name, Option<GhOpenColorFamily> Colour) : ComposeOp;
    public sealed record ChainCase : ComposeOp;
    public sealed record ClusterCase : ComposeOp;
    public static ComposeOp Group(string? name = null, GhOpenColorFamily? colour = null) =>
        new GroupCase(Name: Optional(name), Colour: Optional(colour));
    public static readonly ComposeOp Chain = new ChainCase();
    public static readonly ComposeOp Cluster = new ClusterCase();
}

[Union]
public partial record ObjectsScope {
    private ObjectsScope() { }
    public sealed record PrimaryCase : ObjectsScope;
    public sealed record PrimaryAndSecondaryCase : ObjectsScope;
    public sealed record SelectedCase : ObjectsScope;
    public static readonly ObjectsScope Primary = new PrimaryCase();
    public static readonly ObjectsScope PrimaryAndSecondary = new PrimaryAndSecondaryCase();
    public static readonly ObjectsScope Selected = new SelectedCase();

    internal IEnumerable<IDocumentObject> Project(GhObjectList objects) => this switch {
        PrimaryCase => objects.Forwards,
        PrimaryAndSecondaryCase => objects.PrimaryAndSecondary,
        SelectedCase => objects.SelectedObjects,
        _ => Enumerable.Empty<IDocumentObject>(),
    };
}

[Union]
public partial record GripKind {
    private GripKind() { }
    public sealed record InletCase : GripKind;
    public sealed record OutletCase : GripKind;
    public sealed record InletOrOutletCase : GripKind;
    public static readonly GripKind Inlet = new InletCase();
    public static readonly GripKind Outlet = new OutletCase();
    public static readonly GripKind InletOrOutlet = new InletOrOutletCase();
}

[Union]
public partial record FindCriterion {
    private FindCriterion() { }
    public sealed record NearPointCase(PointF Point, int MaxResults, float MaxDistance) : FindCriterion;
    public sealed record ByDrawOrderCase(bool Foreground, bool Background) : FindCriterion;
    public sealed record UpstreamCase(Guid ParameterId) : FindCriterion;
    public sealed record DownstreamCase(Guid ParameterId) : FindCriterion;
    public sealed record ByNameCase(string Substring, bool CaseInsensitive) : FindCriterion;
    public sealed record BySearchCase(string Query, int MaxResults) : FindCriterion;

    public static FindCriterion Near(PointF point, int maxResults = 16, float maxDistance = 32f) =>
        new NearPointCase(Point: point, MaxResults: maxResults, MaxDistance: maxDistance);
    public static FindCriterion DrawOrder(bool foreground = true, bool background = true) =>
        new ByDrawOrderCase(Foreground: foreground, Background: background);
    public static FindCriterion Upstream(Guid parameterId) => new UpstreamCase(ParameterId: parameterId);
    public static FindCriterion Downstream(Guid parameterId) => new DownstreamCase(ParameterId: parameterId);
    public static FindCriterion ByName(string substring, bool caseInsensitive = true) =>
        new ByNameCase(Substring: substring, CaseInsensitive: caseInsensitive);
    public static FindCriterion Search(string query, int maxResults = 32) =>
        new BySearchCase(Query: query, MaxResults: maxResults);
}

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentSnapshot(
    Guid Hash, bool Modified, int Modifications,
    int ObjectCount, int PinCount, int ExpiredCount,
    int SelectedObjectCount, int SelectedWireCount, int SelectedDanglingWireCount,
    int WireCount, int DanglingWireCount,
    RectangleF AttributeBounds, RectangleF PivotBounds,
    PointF ProjectionCentre, float ProjectionZoom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentObjectSnapshot(
    Guid Id, string Name, string DisplayName,
    bool Selected, string Activity, string Display, string Phase, string State,
    RectangleF Bounds, PointF Pivot);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentGripSnapshot(Guid Parameter, bool InletWithinRange, bool OutletWithinRange);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentComposeResult(Snapshot<DocumentMutationDelta> Delta, Option<Guid> CreatedId);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentDropResult(Snapshot<DocumentMutationDelta> Delta, bool Placed);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentDependencyResult(Snapshot<DocumentMutationDelta> Delta, Option<Guid> Listener, Option<Guid> ShoutId);

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Document {
    public static GrasshopperUiIntent<DocumentSnapshot> Snapshot() =>
        IntentFactory.Document<DocumentSnapshot>(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            select SnapshotOf(document: doc, objects: objs));

    public static GrasshopperUiIntent<Seq<DocumentObjectSnapshot>> Objects(ObjectsScope objectsScope) =>
        IntentFactory.Document<Seq<DocumentObjectSnapshot>>(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objectsScope.Project(objects: objs).Select(SnapshotObject))));

    public static GrasshopperUiIntent<Option<DocumentObjectSnapshot>> Object(Guid id) =>
        IntentFactory.Document<Option<DocumentObjectSnapshot>>(run: scope =>
            Optional(id).Filter(static g => g != Guid.Empty)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Object)), detail: "empty Guid"))
                .Bind(valid => scope.NeedObjects().Map(objs => Optional(objs.Find(instanceId: valid)).Map(SnapshotObject))));

    public static GrasshopperUiIntent<Option<DocumentGripSnapshot>> Grip(PointF point, GripKind? kind = null) =>
        IntentFactory.Document<Option<DocumentGripSnapshot>>(run: scope =>
            Optional(point).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Grip)), detail: "non-finite point"))
                .Bind(_ => scope.NeedObjects().Map(objs => GripOf(objects: objs, point: point, kind: kind ?? GripKind.InletOrOutlet))));

    public static GrasshopperUiIntent<Seq<DocumentObjectSnapshot>> Find(FindCriterion criterion) =>
        IntentFactory.Document<Seq<DocumentObjectSnapshot>>(run: scope =>
            scope.NeedObjects().Bind(objs => FindBy(objects: objs, criterion: criterion)));

    public static GrasshopperUiIntent<Seq<MetaName>> MetaNames() =>
        IntentFactory.Document<Seq<MetaName>>(run: scope =>
            scope.NeedObjects().Map(objs => toSeq(objs.MetaNames())));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Selection(SelectionOp op) =>
        GrasshopperUi.MutateDocument(
            op: Op.Of(name: $"Selection.{op.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty)}"),
            mutate: methods => RunSelection(methods: methods, op: op));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Display(DisplayOp op) =>
        GrasshopperUi.MutateDocument(
            op: Op.Of(name: $"Display.{op.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty)}"),
            mutate: methods => RunDisplay(methods: methods, op: op));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Clipboard(ClipboardOp op) =>
        GrasshopperUi.MutateDocument(
            op: Op.Of(name: $"Clipboard.{op.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty)}"),
            mutate: methods => RunClipboard(methods: methods, op: op));

    public static GrasshopperUiIntent<DocumentComposeResult> Compose(ComposeOp op) =>
        IntentFactory.Document<DocumentComposeResult>(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from methods in scope.NeedMethods()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                from created in RunCompose(methods: methods, op: op)
                select new DocumentComposeResult(
                    Delta: Snapshot.Of<DocumentMutationDelta>(
                        payload: new DocumentMutationDelta(Changed: created.IsSome ? 1 : 0, After: SnapshotOf(document: doc, objects: objs)),
                        ownerId: Some(doc.Hash)),
                    CreatedId: created));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Colour(GhColour? colour = null) =>
        GrasshopperUi.MutateDocument(
            op: Op.Of(name: nameof(Colour)),
            mutate: methods => Fin.Succ(value: methods.SetColourOverrideSelected(colour: colour, actions: null)));

    public static GrasshopperUiIntent<DocumentDropResult> Drop(Guid proxyId, PointF location) =>
        IntentFactory.Document<DocumentDropResult>(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                Optional((ProxyId: proxyId, Location: location))
                    .Filter(static s => s.ProxyId != Guid.Empty && float.IsFinite(s.Location.X) && float.IsFinite(s.Location.Y))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Drop)), detail: "empty proxy id or non-finite location"))
                    .Bind(valid =>
                        from methods in scope.NeedMethods()
                        from doc in scope.NeedDocument()
                        from objs in scope.NeedObjects()
                        let placed = methods.DropObject(obj: valid.ProxyId, location: valid.Location, actions: null)
                        select new DocumentDropResult(
                            Delta: Snapshot.Of<DocumentMutationDelta>(
                                payload: new DocumentMutationDelta(Changed: placed ? 1 : 0, After: SnapshotOf(document: doc, objects: objs)),
                                ownerId: Some(doc.Hash)),
                            Placed: placed)));

    public static GrasshopperUiIntent<DocumentDependencyResult> AddDependency(PointF location) =>
        IntentFactory.Document<DocumentDependencyResult>(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                Optional(location).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(AddDependency)), detail: "non-finite location"))
                    .Bind(valid =>
                        from methods in scope.NeedMethods()
                        from doc in scope.NeedDocument()
                        from objs in scope.NeedObjects()
                        let listen = methods.AddDependency(location: valid, actions: null)
                        select new DocumentDependencyResult(
                            Delta: Snapshot.Of<DocumentMutationDelta>(
                                payload: new DocumentMutationDelta(Changed: listen is not null ? 1 : 0, After: SnapshotOf(document: doc, objects: objs)),
                                ownerId: Some(doc.Hash)),
                            Listener: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty),
                            ShoutId: Optional(listen?.ShoutId).Filter(static g => g != Guid.Empty))));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Isolate(IsolateOptions options = default) =>
        IntentFactory.Document<Snapshot<DocumentMutationDelta>>(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from methods in scope.NeedMethods()
                from objs in scope.NeedObjects()
                from doc in scope.NeedDocument()
                from primary in Optional(objs.SelectedObjects.FirstOrDefault())
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Isolate)), detail: "no object selected to isolate"))
                from _ in RunIsolate(methods: methods, primary: primary, omit: new SysHashSet(collection: objs.SelectedObjects.Select(static o => o.InstanceId)), options: options)
                select Snapshot.Of<DocumentMutationDelta>(
                    payload: new DocumentMutationDelta(Changed: 1, After: SnapshotOf(document: doc, objects: objs)),
                    ownerId: Some(doc.Hash)));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> GrowSelection(bool upstream = true, bool downstream = true) =>
        GrasshopperUi.MutateDocument(
            op: Op.Of(name: nameof(GrowSelection)),
            mutate: methods => Fin.Succ(value: methods.GrowSelection(upstream: upstream, downstream: downstream)));

    public static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> Batch(params GrasshopperUiIntent<Snapshot<DocumentMutationDelta>>[] steps) =>
        IntentFactory.Document<Snapshot<DocumentMutationDelta>>(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                from outcome in RunBatch(scope: scope, steps: toSeq(steps), doc: doc, objs: objs)
                select outcome);

    // --- [OPERATIONS] ----------------------------------------------------------------------
    internal static DocumentSnapshot SnapshotOf(GhDocument document, GhObjectList objects) {
        Seq<WireEnds> allWires = toSeq(objects.AllWires);
        Seq<WireEnds> selectedWires = toSeq(objects.SelectedWires);
        int wireCount = allWires.Count(wire => Wire.IsConnected(objects: objects, wire: wire));
        int selectedWireCount = selectedWires.Count(wire => Wire.IsConnected(objects: objects, wire: wire));
        return new DocumentSnapshot(
            Hash: document.Hash, Modified: document.Modified, Modifications: document.Modifications,
            ObjectCount: objects.Count, PinCount: objects.PinCount, ExpiredCount: objects.ExpiredCount,
            SelectedObjectCount: objects.SelectedCount, SelectedWireCount: selectedWireCount,
            SelectedDanglingWireCount: selectedWires.Count - selectedWireCount,
            WireCount: wireCount, DanglingWireCount: allWires.Count - wireCount,
            AttributeBounds: objects.AttributeBounds, PivotBounds: objects.PivotBounds,
            ProjectionCentre: document.Projection.centre, ProjectionZoom: document.Projection.zoom);
    }

    // L-002/D-006 fix: SnapshotObject NO LONGER calls Attributes.Layout(). Bounds read directly from current attributes.
    private static DocumentObjectSnapshot SnapshotObject(IDocumentObject obj) =>
        new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
            Selected: obj.Selected, Activity: obj.Activity.ToString(),
            Display: obj.Display.ToString(), Phase: obj.Phase.ToString(), State: obj.State.ToString(),
            Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);

    private static Fin<int> RunSelection(GhDocumentMethods methods, SelectionOp op) => op switch {
        SelectionOp.AllCase => Fin.Succ(value: methods.SelectAll()),
        SelectionOp.NoneCase => Fin.Succ(value: methods.DeselectAll()),
        SelectionOp.InvertCase => Fin.Succ(value: methods.InvertSelection()),
        SelectionOp.DeleteCase { DataOnly: false } => Fin.Succ(value: methods.DeleteSelection(actions: null)),
        SelectionOp.DeleteCase { DataOnly: true } => Fin.Succ(value: methods.DeleteSelectionData(actions: null)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.Show } => Fin.Succ(value: methods.ShowSelected(actions: null)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.Hide } => Fin.Succ(value: methods.HideSelected(actions: null)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.Toggle } => Fin.Succ(value: methods.ToggleDisplaySelected(actions: null)),
        SelectionOp.EnableCase => Fin.Succ(value: methods.EnableSelected(actions: null)),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(RunSelection)), detail: "unknown selection op")),
    };

    private static Fin<int> RunDisplay(GhDocumentMethods methods, DisplayOp op) => op switch {
        DisplayOp.InputsCase { Change: VisibilityChange.Show } => Fin.Succ(value: methods.ShowSelectedInputs(actions: null)),
        DisplayOp.InputsCase { Change: VisibilityChange.Hide } => Fin.Succ(value: methods.HideSelectedInputs(actions: null)),
        DisplayOp.OutputsCase { Change: VisibilityChange.Show } => Fin.Succ(value: methods.ShowSelectedOutputs(actions: null)),
        DisplayOp.OutputsCase { Change: VisibilityChange.Hide } => Fin.Succ(value: methods.HideSelectedOutputs(actions: null)),
        DisplayOp.InputsCase { Change: VisibilityChange.Toggle } or DisplayOp.OutputsCase { Change: VisibilityChange.Toggle } =>
            Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(RunDisplay)), detail: "Toggle not supported on Inputs/Outputs visibility")),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(RunDisplay)), detail: "unknown display op")),
    };

    private static Fin<int> RunClipboard(GhDocumentMethods methods, ClipboardOp op) => op switch {
        ClipboardOp.CopyCase c => ValidClipboard(name: nameof(ClipboardOp.Copy), clipboard: c.Kind).Map(k => methods.CopySelection(clipboard: k) ? 1 : 0),
        ClipboardOp.CutCase c => ValidClipboard(name: nameof(ClipboardOp.Cut), clipboard: c.Kind).Map(k => methods.CutSelection(clipboard: k, actions: null) ? 1 : 0),
        ClipboardOp.PasteCase p => ValidClipboard(name: nameof(ClipboardOp.Paste), clipboard: p.Kind).Map(k => methods.PasteFromClipboard(clipboard: k, behaviour: p.Behaviour, actions: null) ? 1 : 0),
        ClipboardOp.PasteGh1XmlCase => Fin.Succ(value: methods.PasteGrasshopper1XmlFromClipboard(actions: null) ? 1 : 0),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(RunClipboard)), detail: "unknown clipboard op")),
    };

    private static Fin<Option<Guid>> RunCompose(GhDocumentMethods methods, ComposeOp op) => op switch {
        ComposeOp.GroupCase g =>
            Fin.Succ(value: Optional(methods.GroupSelection(
                name: g.Name.MatchUnsafe(Some: x => x, None: () => null!),
                colour: g.Colour.Match(Some: c => (GhOpenColorFamily?)c, None: () => null),
                actions: null))?.InstanceId is Guid id && id != Guid.Empty ? Some(id) : Option<Guid>.None),
        ComposeOp.ChainCase =>
            Fin.Succ(value: Optional(methods.ChainSelection(actions: null))?.InstanceId is Guid cid && cid != Guid.Empty ? Some(cid) : Option<Guid>.None),
        ComposeOp.ClusterCase =>
            Fin.Succ(value: Optional(methods.ClusterSelection(actions: null))?.InstanceId is Guid lid && lid != Guid.Empty ? Some(lid) : Option<Guid>.None),
        _ => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(RunCompose)), detail: "unknown compose op")),
    };

    private static Fin<Unit> RunIsolate(GhDocumentMethods methods, IDocumentObject primary, SysHashSet omit, IsolateOptions options) =>
        Try.lift<Unit>(f: () => {
            methods.IsolateObject(
                obj: primary,
                pins: options.IncludePins,
                inputs: options.IncludeInputs,
                outputs: options.IncludeOutputs,
                omit: omit,
                actions: null);
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Isolate)), detail: "IsolateObject threw"));

    private static Fin<Snapshot<DocumentMutationDelta>> RunBatch(
        GrasshopperUi.Scope scope, Seq<GrasshopperUiIntent<Snapshot<DocumentMutationDelta>>> steps,
        GhDocument doc, GhObjectList objs) {
        if (steps.IsEmpty) return Fin.Succ(value: Snapshot.Of<DocumentMutationDelta>(
            payload: new DocumentMutationDelta(Changed: 0, After: SnapshotOf(document: doc, objects: objs)),
            ownerId: Some(doc.Hash)));
        UndoGroup bag = new(verb: "Edit", noun: steps.Count == 1 ? "Mutation" : $"{steps.Count} Mutations");
        GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
        return steps.Fold(
            initialState: Fin.Succ<int>(0),
            f: (state, step) =>
                from running in state
                from result in step.Run(scope: scoped)
                select running + result.Payload.Changed)
            .Bind(total => bag.Commit(document: doc).Map(_ => Snapshot.Of<DocumentMutationDelta>(
                payload: new DocumentMutationDelta(Changed: total, After: SnapshotOf(document: doc, objects: objs)),
                ownerId: Some(doc.Hash))));
    }

    private static Option<DocumentGripSnapshot> GripOf(GhObjectList objects, PointF point, GripKind kind) => kind switch {
        GripKind.InletCase => Optional(objects.FindByInlet(point: point))
            .Map(static p => new DocumentGripSnapshot(Parameter: p.InstanceId, InletWithinRange: true, OutletWithinRange: false)),
        GripKind.OutletCase => Optional(objects.FindByOutlet(point: point))
            .Map(static p => new DocumentGripSnapshot(Parameter: p.InstanceId, InletWithinRange: false, OutletWithinRange: true)),
        GripKind.InletOrOutletCase => Optional(objects.FindByInletOrOutlet(point: point))
            .Filter(static found => found.parameter is not null)
            .Map(static found => new DocumentGripSnapshot(Parameter: found.parameter.InstanceId, InletWithinRange: found.inletWithinRange, OutletWithinRange: found.outletWithinRange)),
        _ => Option<DocumentGripSnapshot>.None,
    };

    private static Fin<Seq<DocumentObjectSnapshot>> FindBy(GhObjectList objects, FindCriterion criterion) => criterion switch {
        FindCriterion.NearPointCase n =>
            Optional(n.Point).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: "non-finite point"))
                .Map(_ => toSeq(objects.FindNear<IDocumentObject>(locus: n.Point, maxResults: n.MaxResults, maxDistance: n.MaxDistance)).Map(SnapshotObject)),
        FindCriterion.ByDrawOrderCase d =>
            Fin.Succ(value: toSeq(objects.ObjectsByDrawOrder(includeForeground: d.Foreground, includeBackground: d.Background)).Map(SnapshotObject)),
        FindCriterion.UpstreamCase u =>
            Optional(objects.FindParameter(instanceId: u.ParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: $"parameter {u.ParameterId} not found"))
                .Map(p => toSeq(objects.SearchUpstream(parameter: p)).Map(SnapshotObject)),
        FindCriterion.DownstreamCase d =>
            Optional(objects.FindParameter(instanceId: d.ParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: $"parameter {d.ParameterId} not found"))
                .Map(p => toSeq(objects.SearchDownstream(parameter: p)).Map(SnapshotObject)),
        FindCriterion.ByNameCase n =>
            Fin.Succ(value: toSeq(objects.Forwards)
                .Filter(o => n.CaseInsensitive
                    ? o.Nomen.Name.Contains(value: n.Substring, comparisonType: StringComparison.OrdinalIgnoreCase)
                    : o.Nomen.Name.Contains(value: n.Substring))
                .Map(SnapshotObject)),
        FindCriterion.BySearchCase s =>
            Fin.Succ(value: toSeq(objects.FindBySearch(search: s.Query, maxResults: s.MaxResults)).Map(SnapshotObject)),
        _ => Fin.Fail<Seq<DocumentObjectSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: "unknown criterion")),
    };

    private static Fin<ClipboardKind> ValidClipboard(string name, ClipboardKind clipboard) =>
        clipboard == ClipboardKind.Instance
            ? Fin.Fail<ClipboardKind>(error: UiFault.InvalidInput(op: Op.Of(name: name), detail: "ClipboardKind.Instance is not supported"))
            : Fin.Succ(value: clipboard);
}

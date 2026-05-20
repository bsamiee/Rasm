using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using GhColour = Grasshopper2.Types.Colour.Colour;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GhOpenColorFamily = Eto.Drawing.OpenColor.Family;
using Op = Rasm.Domain.Op;
using SysHashSet = System.Collections.Generic.HashSet<System.Guid>;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public partial record VisibilityChange {
    private VisibilityChange() { }
    public sealed record ShowCase : VisibilityChange;
    public sealed record HideCase : VisibilityChange;
    public sealed record ToggleCase : VisibilityChange;
    public static readonly VisibilityChange Show = new ShowCase();
    public static readonly VisibilityChange Hide = new HideCase();
    public static readonly VisibilityChange Toggle = new ToggleCase();
}

[Union]
public partial record SelectionOp {
    private SelectionOp() { }
    public sealed record AllCase : SelectionOp;
    public sealed record NoneCase : SelectionOp;
    public sealed record InvertCase : SelectionOp;
    public sealed record DeleteCase(bool DataOnly) : SelectionOp;
    public sealed record VisibilityCase(VisibilityChange Change) : SelectionOp;
    public sealed record EnableCase : SelectionOp;

    public static readonly SelectionOp All = new AllCase();
    public static readonly SelectionOp None = new NoneCase();
    public static readonly SelectionOp Invert = new InvertCase();
    public static SelectionOp Delete(bool dataOnly = false) => new DeleteCase(DataOnly: dataOnly);
    public static SelectionOp Visibility(VisibilityChange change) => new VisibilityCase(Change: change);
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

    public static FindCriterion Near(PointF point, int maxResults = 16, float maxDistance = 32f) => new NearPointCase(Point: point, MaxResults: maxResults, MaxDistance: maxDistance);
    public static FindCriterion DrawOrder(bool foreground = true, bool background = true) => new ByDrawOrderCase(Foreground: foreground, Background: background);
    public static FindCriterion Upstream(Guid parameterId) => new UpstreamCase(ParameterId: parameterId);
    public static FindCriterion Downstream(Guid parameterId) => new DownstreamCase(ParameterId: parameterId);
    public static FindCriterion ByName(string substring, bool caseInsensitive = true) => new ByNameCase(Substring: substring, CaseInsensitive: caseInsensitive);
    public static FindCriterion Search(string query, int maxResults = 32) => new BySearchCase(Query: query, MaxResults: maxResults);
}

[Union]
public partial record DocumentOp {
    private DocumentOp() { }
    public sealed record SnapshotCase : DocumentOp;
    public sealed record ObjectsCase(ObjectsScope Scope) : DocumentOp;
    public sealed record ObjectCase(Guid Id) : DocumentOp;
    public sealed record ParameterCase(Guid Id) : DocumentOp;
    public sealed record GripCase(PointF Point, GripKind Kind) : DocumentOp;
    public sealed record FindCase(FindCriterion Criterion) : DocumentOp;
    public sealed record MetaNamesCase : DocumentOp;
    public sealed record SelectionCase(SelectionOp Op) : DocumentOp;
    public sealed record DisplayCase(DisplayOp Op) : DocumentOp;
    public sealed record ClipboardCase(ClipboardOp Op) : DocumentOp;
    public sealed record ComposeCase(ComposeOp Op) : DocumentOp;
    public sealed record ColourCase(Option<GhColour> Override) : DocumentOp;
    public sealed record DropCase(Guid ProxyId, PointF Location) : DocumentOp;
    public sealed record AddDependencyCase(PointF Location) : DocumentOp;
    public sealed record IsolateCase(IsolateOptions Options) : DocumentOp;
    public sealed record GrowSelectionCase(bool Upstream, bool Downstream) : DocumentOp;
    public sealed record BatchCase(Seq<GrasshopperUiIntent<DocumentResult>> Steps) : DocumentOp;

    public static readonly DocumentOp Snapshot = new SnapshotCase();
    public static DocumentOp Objects(ObjectsScope scope) => new ObjectsCase(Scope: scope);
    public static DocumentOp Object(Guid id) => new ObjectCase(Id: id);
    public static DocumentOp Parameter(Guid id) => new ParameterCase(Id: id);
    public static DocumentOp Grip(PointF point, GripKind? kind = null) => new GripCase(Point: point, Kind: kind ?? GripKind.InletOrOutlet);
    public static DocumentOp Find(FindCriterion criterion) => new FindCase(Criterion: criterion);
    public static readonly DocumentOp MetaNames = new MetaNamesCase();
    public static DocumentOp Selection(SelectionOp op) => new SelectionCase(Op: op);
    public static DocumentOp Display(DisplayOp op) => new DisplayCase(Op: op);
    public static DocumentOp Clipboard(ClipboardOp op) => new ClipboardCase(Op: op);
    public static DocumentOp Compose(ComposeOp op) => new ComposeCase(Op: op);
    public static DocumentOp Colour(GhColour? colour = null) => new ColourCase(Override: Optional(colour));
    public static DocumentOp Drop(Guid proxyId, PointF location) => new DropCase(ProxyId: proxyId, Location: location);
    public static DocumentOp AddDependency(PointF location) => new AddDependencyCase(Location: location);
    public static DocumentOp Isolate(IsolateOptions options = default) => new IsolateCase(Options: options);
    public static DocumentOp GrowSelection(bool upstream = true, bool downstream = true) => new GrowSelectionCase(Upstream: upstream, Downstream: downstream);
    public static DocumentOp Batch(params GrasshopperUiIntent<DocumentResult>[] steps) => new BatchCase(Steps: toSeq(steps));
}

[Union]
public partial record DocumentResult {
    private DocumentResult() { }
    public sealed record SnapshotResult(DocumentSnapshot Snapshot) : DocumentResult;
    public sealed record ObjectListResult(Seq<DocumentObjectSnapshot> Snapshots) : DocumentResult;
    public sealed record ObjectResult(Option<DocumentObjectSnapshot> Snapshot) : DocumentResult;
    public sealed record ParameterResult(Option<ParameterSnapshot> Snapshot) : DocumentResult;
    public sealed record GripResult(Option<DocumentGripSnapshot> Grip) : DocumentResult;
    public sealed record FindResult(Seq<DocumentObjectSnapshot> Matches) : DocumentResult;
    public sealed record MetaNamesResult(Seq<MetaName> Names) : DocumentResult;
    public sealed record MutationResult(Snapshot<DocumentMutationDelta> Delta) : DocumentResult;
    public sealed record ComposeResult(Snapshot<DocumentMutationDelta> Delta, Option<Guid> CreatedId) : DocumentResult;
    public sealed record DropResult(Snapshot<DocumentMutationDelta> Delta, bool Placed) : DocumentResult;
    public sealed record DependencyResult(Snapshot<DocumentMutationDelta> Delta, Option<Guid> Listener, Option<Guid> ShoutId) : DocumentResult;
}

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct IsolateOptions(bool IncludePins = true, bool IncludeInputs = true, bool IncludeOutputs = true);

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
public readonly record struct ParameterSnapshot(
    Guid Id,
    string Name,
    string Kind,
    string Access,
    string AccessVariability,
    string Requirement,
    string TypeFlavour,
    int InputCount,
    int OutputCount,
    bool HasColourOverride);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentGripSnapshot(Guid Parameter, bool InletWithinRange, bool OutletWithinRange);

public abstract record DocumentRequest<T> : GhUiRequest<T> {
    public sealed record Use(DocumentOp Op) : DocumentRequest<DocumentResult> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: UiRail.DocumentRepaintFor(op: Op));
        internal override Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) => UiRail.DocumentDispatch(scope: scope, op: Op);
    }
    public sealed record Transaction(Seq<DocumentMutation> Mutations) : DocumentRequest<Snapshot<DocumentMutationDelta>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas);
        internal override Fin<Snapshot<DocumentMutationDelta>> Apply(GrasshopperUi.Scope scope) =>
            from methods in scope.NeedMethods()
            from document in scope.NeedDocument()
            from objects in scope.NeedObjects()
            let actions = new ActionList([])
            from changed in Mutations.TraverseM(m => m.Apply(methods: methods, objects: objects, actions: actions)).Map(static xs => xs.Fold(initialState: 0, f: static (sum, value) => sum + value)).As()
            from _ in UiRail.CommitActions(document: document, op: Op.Of(name: nameof(Transaction)), actions: actions)
            select Snapshot.Of(
                payload: new DocumentMutationDelta(Changed: changed, After: UiRail.DocumentSnapshotOf(document: document, objects: objects)),
                ownerId: Some(document.Hash));
    }
}

public abstract record DocumentMutation {
    public sealed record Targets(Seq<Guid> Ids, DocumentTargetOp Op) : DocumentMutation;
    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, ActionList actions) => this switch {
        Targets targets => targets.Op.Apply(methods: methods, objects: objects, ids: targets.Ids, actions: actions),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentMutation)), detail: "unknown document mutation")),
    };
}

public abstract record DocumentTargetOp {
    public sealed record ShowCase : DocumentTargetOp;
    public sealed record HideCase : DocumentTargetOp;
    public sealed record EnableCase : DocumentTargetOp;
    public sealed record DisableCase : DocumentTargetOp;
    public sealed record DeleteCase : DocumentTargetOp;
    public sealed record ColourCase(Option<GhColour> Override) : DocumentTargetOp;
    public static readonly DocumentTargetOp Show = new ShowCase();
    public static readonly DocumentTargetOp Hide = new HideCase();
    public static readonly DocumentTargetOp Enable = new EnableCase();
    public static readonly DocumentTargetOp Disable = new DisableCase();
    public static readonly DocumentTargetOp Delete = new DeleteCase();
    public static DocumentTargetOp Style(GhColour colour) => new ColourCase(Override: Some(colour));
    public static readonly DocumentTargetOp ClearStyle = new ColourCase(Override: Option<GhColour>.None);
    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, Seq<Guid> ids, ActionList actions) =>
        Fin.Succ(value: ids.Choose(id => Optional(objects.Find(instanceId: id))).ToArray()).Bind(targets => this switch {
            ShowCase => Fin.Succ(value: methods.ShowObjects(objects: targets, actions: actions)),
            HideCase => Fin.Succ(value: methods.HideObjects(objects: targets, actions: actions)),
            EnableCase => Fin.Succ(value: methods.EnableObjects(objects: targets, actions: actions)),
            DisableCase => Fin.Succ(value: methods.DisableObjects(objects: targets, actions: actions)),
            DeleteCase => Fin.Succ(value: methods.DeleteObjects(objects: targets, wires: [], actions: actions)),
            ColourCase colour => Fin.Succ(value: methods.SetColourOverrideObjects(
                objects: targets,
                colour: colour.Override.Map(static value => (GhColour?)value).IfNone((GhColour?)null),
                actions: actions)),
            _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentTargetOp)), detail: "unknown target op")),
        });
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class UiRail {
    internal static GrasshopperUiIntent<DocumentResult> Document(DocumentOp op) {
        ArgumentNullException.ThrowIfNull(argument: op);
        return GhUi.Apply(new DocumentRequest<DocumentResult>.Use(Op: op));
    }

    // --- [OPERATIONS] ----------------------------------------------------------------------
    internal static Fin<DocumentResult> DocumentDispatch(GrasshopperUi.Scope scope, DocumentOp op) => op switch {
        DocumentOp.SnapshotCase =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            select (DocumentResult)new DocumentResult.SnapshotResult(Snapshot: DocumentSnapshotOf(document: doc, objects: objs)),

        DocumentOp.ObjectsCase o =>
            scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectListResult(
                Snapshots: toSeq(ProjectObjects(scope: o.Scope, objects: objs).Select(DocumentObjectSnapshotOf)))),

        DocumentOp.ObjectCase obj =>
            Optional(obj.Id).Filter(static g => g != Guid.Empty)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.Object)), detail: "empty Guid"))
                .Bind(valid => scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectResult(
                    Snapshot: Optional(objs.Find(instanceId: valid)).Map(DocumentObjectSnapshotOf)))),

        DocumentOp.ParameterCase param =>
            Optional(param.Id).Filter(static g => g != Guid.Empty)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.Parameter)), detail: "empty Guid"))
                .Bind(valid => scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ParameterResult(
                    Snapshot: Optional(objs.FindParameter(instanceId: valid)).Map(ParameterSnapshotOf)))),

        DocumentOp.GripCase g =>
            Optional(g.Point).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.Grip)), detail: "non-finite point"))
                .Bind(_ => scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.GripResult(
                    Grip: GripSnapshotOf(objects: objs, point: g.Point, kind: g.Kind)))),

        DocumentOp.FindCase f =>
            scope.NeedObjects().Bind(objs => FindBy(objects: objs, criterion: f.Criterion)
                .Map(matches => (DocumentResult)new DocumentResult.FindResult(Matches: matches))),

        DocumentOp.MetaNamesCase =>
            scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.MetaNamesResult(Names: toSeq(objs.MetaNames()))),

        DocumentOp.SelectionCase s =>
            RunMutation(scope: scope, op: Op.Of(name: $"Selection.{NameOfSelection(case_: s.Op)}"),
                mutate: (methods, _, actions) => SelectionDispatch(methods: methods, op: s.Op, actions: actions))
                .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta)),

        DocumentOp.DisplayCase d =>
            RunMutation(scope: scope, op: Op.Of(name: $"Display.{NameOfDisplay(case_: d.Op)}"),
                mutate: (methods, _, actions) => DisplayDispatch(methods: methods, op: d.Op, actions: actions))
                .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta)),

        DocumentOp.ClipboardCase c =>
            RunMutation(scope: scope, op: Op.Of(name: $"Clipboard.{NameOfClipboard(case_: c.Op)}"),
                mutate: (methods, _, actions) => ClipboardDispatch(methods: methods, op: c.Op, actions: actions))
                .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta)),

        DocumentOp.ComposeCase c =>
            from methods in scope.NeedMethods()
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            let actions = new ActionList([])
            from created in ComposeDispatch(methods: methods, op: c.Op, actions: actions)
            from _ in CommitActions(document: doc, op: Op.Of(name: nameof(DocumentOp.Compose)), actions: actions)
            select (DocumentResult)new DocumentResult.ComposeResult(
                Delta: Snapshot.Of<DocumentMutationDelta>(
                    payload: new DocumentMutationDelta(Changed: created.IsSome ? 1 : 0, After: DocumentSnapshotOf(document: doc, objects: objs)),
                    ownerId: Some(doc.Hash)),
                CreatedId: created),

        DocumentOp.ColourCase c =>
            RunMutation(scope: scope, op: Op.Of(name: nameof(DocumentOp.Colour)),
                mutate: (methods, _, actions) => Fin.Succ(value: methods.SetColourOverrideSelected(
                    colour: c.Override.Map(static value => (GhColour?)value).IfNone((GhColour?)null),
                    actions: actions)))
                .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta)),

        DocumentOp.DropCase d =>
            Optional((d.ProxyId, d.Location))
                .Filter(static s => s.ProxyId != Guid.Empty && float.IsFinite(s.Location.X) && float.IsFinite(s.Location.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.Drop)), detail: "empty proxy id or non-finite location"))
                .Bind(valid =>
                    from methods in scope.NeedMethods()
                    from doc in scope.NeedDocument()
                    from objs in scope.NeedObjects()
                    let actions = new ActionList([])
                    let placed = methods.DropObject(obj: valid.ProxyId, location: valid.Location, actions: actions)
                    from _ in CommitActions(document: doc, op: Op.Of(name: nameof(DocumentOp.Drop)), actions: actions)
                    select (DocumentResult)new DocumentResult.DropResult(
                        Delta: Snapshot.Of<DocumentMutationDelta>(
                            payload: new DocumentMutationDelta(Changed: placed ? 1 : 0, After: DocumentSnapshotOf(document: doc, objects: objs)),
                            ownerId: Some(doc.Hash)),
                        Placed: placed)),

        DocumentOp.AddDependencyCase a =>
            Optional(a.Location).Filter(static p => float.IsFinite(p.X) && float.IsFinite(p.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.AddDependency)), detail: "non-finite location"))
                .Bind(valid =>
                    from methods in scope.NeedMethods()
                    from doc in scope.NeedDocument()
                    from objs in scope.NeedObjects()
                    let actions = new ActionList([])
                    let listen = methods.AddDependency(location: valid, actions: actions)
                    from _ in CommitActions(document: doc, op: Op.Of(name: nameof(DocumentOp.AddDependency)), actions: actions)
                    select (DocumentResult)new DocumentResult.DependencyResult(
                        Delta: Snapshot.Of<DocumentMutationDelta>(
                            payload: new DocumentMutationDelta(Changed: listen is not null ? 1 : 0, After: DocumentSnapshotOf(document: doc, objects: objs)),
                            ownerId: Some(doc.Hash)),
                        Listener: Optional(listen?.InstanceId).Filter(static g => g != Guid.Empty),
                        ShoutId: Optional(listen?.ShoutId).Filter(static g => g != Guid.Empty))),

        DocumentOp.IsolateCase i =>
            from methods in scope.NeedMethods()
            from objs in scope.NeedObjects()
            from doc in scope.NeedDocument()
            let actions = new ActionList([])
            from primary in Optional(objs.SelectedObjects.FirstOrDefault())
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.Isolate)), detail: "no object selected to isolate"))
            from _ in Try.lift<Unit>(f: () => {
                methods.IsolateObject(obj: primary,
                    pins: i.Options.IncludePins,
                    inputs: i.Options.IncludeInputs,
                    outputs: i.Options.IncludeOutputs,
                    omit: [.. objs.SelectedObjects.Select(static o => o.InstanceId)],
                    actions: actions);
                return unit;
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(DocumentOp.Isolate)), detail: "IsolateObject threw"))
            from committed in CommitActions(document: doc, op: Op.Of(name: nameof(DocumentOp.Isolate)), actions: actions)
            select (DocumentResult)new DocumentResult.MutationResult(
                Delta: Snapshot.Of<DocumentMutationDelta>(
                    payload: new DocumentMutationDelta(Changed: 1, After: DocumentSnapshotOf(document: doc, objects: objs)),
                    ownerId: Some(doc.Hash))),

        DocumentOp.GrowSelectionCase g =>
            RunMutation(scope: scope, op: Op.Of(name: nameof(DocumentOp.GrowSelection)),
                mutate: (methods, _, _) => Fin.Succ(value: methods.GrowSelection(upstream: g.Upstream, downstream: g.Downstream)))
                .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta)),

        DocumentOp.BatchCase b =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from outcome in RunBatch(scope: scope, steps: b.Steps, doc: doc, objs: objs)
            select (DocumentResult)new DocumentResult.MutationResult(Delta: outcome),

        _ => Fin.Fail<DocumentResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Document)), detail: "unknown DocumentOp")),
    };

    internal static RepaintRequest DocumentRepaintFor(DocumentOp op) => op switch {
        DocumentOp.SnapshotCase or DocumentOp.ObjectsCase or DocumentOp.ObjectCase or
        DocumentOp.ParameterCase or DocumentOp.GripCase or DocumentOp.FindCase or DocumentOp.MetaNamesCase => RepaintRequest.None,
        _ => RepaintRequest.Canvas,
    };

    private static Fin<int> SelectionDispatch(GhDocumentMethods methods, SelectionOp op, ActionList actions) => op switch {
        SelectionOp.AllCase => Fin.Succ(value: methods.SelectAll()),
        SelectionOp.NoneCase => Fin.Succ(value: methods.DeselectAll()),
        SelectionOp.InvertCase => Fin.Succ(value: methods.InvertSelection()),
        SelectionOp.DeleteCase { DataOnly: false } => Fin.Succ(value: methods.DeleteSelection(actions: actions)),
        SelectionOp.DeleteCase { DataOnly: true } => Fin.Succ(value: methods.DeleteSelectionData(actions: actions)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.ShowCase } => Fin.Succ(value: methods.ShowSelected(actions: actions)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.HideCase } => Fin.Succ(value: methods.HideSelected(actions: actions)),
        SelectionOp.VisibilityCase { Change: VisibilityChange.ToggleCase } => Fin.Succ(value: methods.ToggleDisplaySelected(actions: actions)),
        SelectionOp.EnableCase => Fin.Succ(value: methods.EnableSelected(actions: actions)),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(SelectionDispatch)), detail: "unknown selection op")),
    };

    private static Fin<int> DisplayDispatch(GhDocumentMethods methods, DisplayOp op, ActionList actions) => op switch {
        DisplayOp.InputsCase { Change: VisibilityChange.ShowCase } => Fin.Succ(value: methods.ShowSelectedInputs(actions: actions)),
        DisplayOp.InputsCase { Change: VisibilityChange.HideCase } => Fin.Succ(value: methods.HideSelectedInputs(actions: actions)),
        DisplayOp.OutputsCase { Change: VisibilityChange.ShowCase } => Fin.Succ(value: methods.ShowSelectedOutputs(actions: actions)),
        DisplayOp.OutputsCase { Change: VisibilityChange.HideCase } => Fin.Succ(value: methods.HideSelectedOutputs(actions: actions)),
        DisplayOp.InputsCase { Change: VisibilityChange.ToggleCase } or DisplayOp.OutputsCase { Change: VisibilityChange.ToggleCase } =>
            Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DisplayDispatch)), detail: "Toggle not supported on Inputs/Outputs visibility")),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DisplayDispatch)), detail: "unknown display op")),
    };

    private static Fin<int> ClipboardDispatch(GhDocumentMethods methods, ClipboardOp op, ActionList actions) => op switch {
        ClipboardOp.CopyCase c => ValidateClipboard(name: nameof(ClipboardOp.Copy), clipboard: c.Kind).Map(k => methods.CopySelection(clipboard: k) ? 1 : 0),
        ClipboardOp.CutCase c => ValidateClipboard(name: nameof(ClipboardOp.Cut), clipboard: c.Kind).Map(k => methods.CutSelection(clipboard: k, actions: actions) ? 1 : 0),
        ClipboardOp.PasteCase p => ValidateClipboard(name: nameof(ClipboardOp.Paste), clipboard: p.Kind).Map(k => methods.PasteFromClipboard(clipboard: k, behaviour: p.Behaviour, actions: actions) ? 1 : 0),
        ClipboardOp.PasteGh1XmlCase => Fin.Succ(value: methods.PasteGrasshopper1XmlFromClipboard(actions: actions) ? 1 : 0),
        _ => Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ClipboardDispatch)), detail: "unknown clipboard op")),
    };

    private static Fin<Option<Guid>> ComposeDispatch(GhDocumentMethods methods, ComposeOp op, ActionList actions) => op switch {
        ComposeOp.GroupCase g => Fin.Succ(value: NonEmptyIdOf(Optional(methods.GroupSelection(
            name: g.Name.IfNone(string.Empty),
            colour: g.Colour.Map(static c => (GhOpenColorFamily?)c).IfNone((GhOpenColorFamily?)null),
            actions: actions)).Map(static r => r.InstanceId))),
        ComposeOp.ChainCase => Fin.Succ(value: NonEmptyIdOf(Optional(methods.ChainSelection(actions: actions)).Map(static c => c.InstanceId))),
        ComposeOp.ClusterCase => Fin.Succ(value: NonEmptyIdOf(Optional(methods.ClusterSelection(actions: actions)).Map(static c => c.InstanceId))),
        _ => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: "unknown compose op")),
    };

    private static Option<Guid> NonEmptyIdOf(Option<Guid> id) => id.Filter(static g => g != Guid.Empty);

    private static Fin<ClipboardKind> ValidateClipboard(string name, ClipboardKind clipboard) =>
        clipboard == ClipboardKind.Instance
            ? Fin.Fail<ClipboardKind>(error: UiFault.InvalidInput(op: Op.Of(name: name), detail: "ClipboardKind.Instance is not supported"))
            : Fin.Succ(value: clipboard);

    private static IEnumerable<IDocumentObject> ProjectObjects(ObjectsScope scope, GhObjectList objects) => scope switch {
        ObjectsScope.PrimaryCase => objects.Forwards,
        ObjectsScope.PrimaryAndSecondaryCase => objects.PrimaryAndSecondary,
        ObjectsScope.SelectedCase => objects.SelectedObjects,
        _ => [],
    };

    private static Option<DocumentGripSnapshot> GripSnapshotOf(GhObjectList objects, PointF point, GripKind kind) => kind switch {
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
                .Map(_ => toSeq(objects.FindNear<IDocumentObject>(locus: n.Point, maxResults: n.MaxResults, maxDistance: n.MaxDistance)).Map(DocumentObjectSnapshotOf)),
        FindCriterion.ByDrawOrderCase d =>
            Fin.Succ(value: toSeq(objects.ObjectsByDrawOrder(includeForeground: d.Foreground, includeBackground: d.Background)).Map(DocumentObjectSnapshotOf)),
        FindCriterion.UpstreamCase u =>
            Optional(objects.FindParameter(instanceId: u.ParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: $"parameter {u.ParameterId} not found"))
                .Map(p => toSeq(objects.SearchUpstream(parameter: p)).Map(DocumentObjectSnapshotOf)),
        FindCriterion.DownstreamCase d =>
            Optional(objects.FindParameter(instanceId: d.ParameterId))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: $"parameter {d.ParameterId} not found"))
                .Map(p => toSeq(objects.SearchDownstream(parameter: p)).Map(DocumentObjectSnapshotOf)),
        FindCriterion.ByNameCase n =>
            Fin.Succ(value: toSeq(objects.Forwards)
                .Filter(o => o.Nomen.Name.Contains(value: n.Substring, comparisonType: n.CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                .Map(DocumentObjectSnapshotOf)),
        FindCriterion.BySearchCase s =>
            Fin.Succ(value: toSeq(objects.FindBySearch(search: s.Query, maxResults: s.MaxResults)).Map(DocumentObjectSnapshotOf)),
        _ => Fin.Fail<Seq<DocumentObjectSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: "unknown criterion")),
    };

    private static Fin<Snapshot<DocumentMutationDelta>> RunMutation(GrasshopperUi.Scope scope, Op op, Func<GhDocumentMethods, GhObjectList, ActionList, Fin<int>> mutate) =>
        from methods in scope.NeedMethods()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        let actions = new ActionList([])
        from changed in mutate(arg1: methods, arg2: objects, arg3: actions).Bind(count => count >= 0
            ? Fin.Succ(value: count)
            : Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: $"count={count}")))
        from _ in CommitActions(document: document, op: op, actions: actions)
        select Snapshot.Of<DocumentMutationDelta>(
            payload: new DocumentMutationDelta(Changed: changed, After: DocumentSnapshotOf(document: document, objects: objects)),
            ownerId: Some(document.Hash));

    internal static Fin<Unit> CommitActions(GhDocument document, Op op, ActionList actions) =>
        actions.Count <= 0
            ? Fin.Succ(value: unit)
            : Try.lift<Unit>(f: () => {
                document.Undo.Do(name: ("Edit", op.ToString()), actions: actions);
                return unit;
            }).Run().MapFail(_ => UiFault.MutationRejected(op: op, detail: "History.Do threw"));

    private static Fin<Snapshot<DocumentMutationDelta>> RunBatch(
        GrasshopperUi.Scope scope, Seq<GrasshopperUiIntent<DocumentResult>> steps,
        GhDocument doc, GhObjectList objs) =>
        steps.IsEmpty switch {
            true => Fin.Succ(value: Snapshot.Of<DocumentMutationDelta>(
                payload: new DocumentMutationDelta(Changed: 0, After: DocumentSnapshotOf(document: doc, objects: objs)),
                ownerId: Some(doc.Hash))),
            false => RunBatchSteps(scope: scope, steps: steps, doc: doc, objs: objs),
        };

    private static Fin<Snapshot<DocumentMutationDelta>> RunBatchSteps(
        GrasshopperUi.Scope scope, Seq<GrasshopperUiIntent<DocumentResult>> steps,
        GhDocument doc, GhObjectList objs) {
        UndoGroup bag = new(verb: "Edit", noun: steps.Count == 1 ? "Mutation" : $"{steps.Count} Mutations");
        GrasshopperUi.Scope scoped = scope with { UndoGroup = Some(bag) };
        return steps.Fold(
                initialState: Fin.Succ<int>(0),
                f: (state, step) =>
                    from running in state
                    from result in step.Run(scope: scoped)
                    let changed = ChangeCountOf(result: result)
                    select running + changed)
            .Bind(total => bag.Commit(document: doc).Map(_ => Snapshot.Of<DocumentMutationDelta>(
                payload: new DocumentMutationDelta(Changed: total, After: DocumentSnapshotOf(document: doc, objects: objs)),
                ownerId: Some(doc.Hash))));
    }

    private static int ChangeCountOf(DocumentResult result) => result switch {
        DocumentResult.MutationResult m => m.Delta.Payload.Changed,
        DocumentResult.ComposeResult c => c.Delta.Payload.Changed,
        DocumentResult.DropResult d => d.Delta.Payload.Changed,
        DocumentResult.DependencyResult dep => dep.Delta.Payload.Changed,
        _ => 0,
    };

    internal static DocumentSnapshot DocumentSnapshotOf(GhDocument document, GhObjectList objects) {
        Seq<WireEnds> allWires = Optional(objects.AllWires).Map(static wires => toSeq(wires)).IfNone(Seq<WireEnds>());
        Seq<WireEnds> selectedWires = Optional(objects.SelectedWires).Map(static wires => toSeq(wires)).IfNone(Seq<WireEnds>());
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

    private static DocumentObjectSnapshot DocumentObjectSnapshotOf(IDocumentObject obj) =>
        new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
            Selected: obj.Selected, Activity: obj.Activity.ToString(),
            Display: obj.Display.ToString(), Phase: obj.Phase.ToString(), State: obj.State.ToString(),
            Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);

    private static ParameterSnapshot ParameterSnapshotOf(IParameter parameter) =>
        new(
            Id: parameter.InstanceId,
            Name: parameter.Nomen.Name,
            Kind: parameter.Kind.ToString(),
            Access: parameter.Access.ToString(),
            AccessVariability: parameter.AccessVariability.ToString(),
            Requirement: parameter.Requirement.ToString(),
            TypeFlavour: parameter.TypeFlavour.ToString(),
            InputCount: parameter.Inputs.Count,
            OutputCount: parameter.Outputs.Count,
            HasColourOverride: !parameter.ColourOverride.Equals(obj: default(GhColour)));

    private static string NameOfSelection(SelectionOp case_) => case_.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty, comparisonType: StringComparison.Ordinal);
    private static string NameOfDisplay(DisplayOp case_) => case_.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty, comparisonType: StringComparison.Ordinal);
    private static string NameOfClipboard(ClipboardOp case_) => case_.GetType().Name.Replace(oldValue: "Case", newValue: string.Empty, comparisonType: StringComparison.Ordinal);
}

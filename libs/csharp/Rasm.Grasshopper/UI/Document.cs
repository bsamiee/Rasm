using System.Globalization;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Special;
using Grasshopper2.UI.Slider;
using Grasshopper2.Undo;
using GhColour = Grasshopper2.Types.Colour.Colour;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhEditor = Grasshopper2.UI.Editor;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GhOpenColorFamily = Eto.Drawing.OpenColor.Family;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record VisibilityChange {
    private VisibilityChange() { }
    public sealed record ShowCase : VisibilityChange;
    public sealed record HideCase : VisibilityChange;
    public static readonly VisibilityChange Show = new ShowCase();
    public static readonly VisibilityChange Hide = new HideCase();
}

[Union]
public partial record SelectionOp {
    private SelectionOp() { }
    public sealed record AllCase : SelectionOp;
    public sealed record NoneCase : SelectionOp;
    public sealed record InvertCase : SelectionOp;
    public sealed record GrowCase(bool Upstream, bool Downstream) : SelectionOp;
    public sealed record ShiftCase(bool Upstream) : SelectionOp;

    public static readonly SelectionOp All = new AllCase();
    public static readonly SelectionOp None = new NoneCase();
    public static readonly SelectionOp Invert = new InvertCase();
    public static SelectionOp Grow(bool upstream = true, bool downstream = true) => new GrowCase(Upstream: upstream, Downstream: downstream);
    public static SelectionOp Shift(bool upstream) => new ShiftCase(Upstream: upstream);
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

    public static FindCriterion Near(PointF point, int maxResults = 16, float maxDistance = 32f) => new NearPointCase(Point: point, MaxResults: maxResults, MaxDistance: maxDistance);
    public static FindCriterion DrawOrder(bool foreground = true, bool background = true) => new ByDrawOrderCase(Foreground: foreground, Background: background);
    public static FindCriterion Upstream(Guid parameterId) => new UpstreamCase(ParameterId: parameterId);
    public static FindCriterion Downstream(Guid parameterId) => new DownstreamCase(ParameterId: parameterId);
    public static FindCriterion ByName(string substring, bool caseInsensitive = true) => new ByNameCase(Substring: substring, CaseInsensitive: caseInsensitive);
}

[Union]
public partial record DocumentQuery {
    private DocumentQuery() { }
    public sealed record SnapshotCase : DocumentQuery;
    public sealed record ObjectsCase(ObjectsScope Scope) : DocumentQuery;
    public sealed record ObjectCase(Guid Id) : DocumentQuery;
    public sealed record ParameterCase(Guid Id) : DocumentQuery;
    public sealed record GripCase(PointF Point, GripKind Kind) : DocumentQuery;
    public sealed record FindCase(FindCriterion Criterion) : DocumentQuery;
    public sealed record MetaNamesCase : DocumentQuery;
    public sealed record UniverseCase : DocumentQuery;

    public static readonly DocumentQuery Snapshot = new SnapshotCase();
    public static DocumentQuery Objects(ObjectsScope scope) => new ObjectsCase(Scope: scope);
    public static DocumentQuery Object(Guid id) => new ObjectCase(Id: id);
    public static DocumentQuery Parameter(Guid id) => new ParameterCase(Id: id);
    public static DocumentQuery Grip(PointF point, GripKind? kind = null) => new GripCase(Point: point, Kind: kind ?? GripKind.InletOrOutlet);
    public static DocumentQuery Find(FindCriterion criterion) => new FindCase(Criterion: criterion);
    public static readonly DocumentQuery MetaNames = new MetaNamesCase();
    public static readonly DocumentQuery Universe = new UniverseCase();
}

[Union]
public partial record DocumentOp {
    private DocumentOp() { }
    public sealed record QueryCase(DocumentQuery Request) : DocumentOp;
    public sealed record MutateCase(Seq<DocumentMutation> Mutations, DocumentMutationPolicy Policy) : DocumentOp;
    public sealed record HistoryCase(DocumentHistoryOp Request) : DocumentOp;

    public static DocumentOp Query(DocumentQuery query) => new QueryCase(Request: query);
    public static DocumentOp Mutate(params DocumentMutation[] mutations) => new MutateCase(Mutations: toSeq(mutations), Policy: DocumentMutationPolicy.Default);
    public static DocumentOp Mutate(DocumentMutationPolicy policy, params DocumentMutation[] mutations) => new MutateCase(Mutations: toSeq(mutations), Policy: policy);
    public static DocumentOp History(DocumentHistoryOp history) => new HistoryCase(Request: history);
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
    public sealed record UniverseResult(DocumentUniverseSnapshot Snapshot) : DocumentResult;
    public sealed record MutationResult(Snapshot<DocumentMutationDelta> Delta) : DocumentResult;
    public sealed record HistoryResult(DocumentHistorySnapshot Snapshot) : DocumentResult;
}

[Union]
public partial record DocumentHistoryOp {
    private DocumentHistoryOp() { }
    public sealed record QueryCase : DocumentHistoryOp;
    public sealed record UndoCase : DocumentHistoryOp;
    public sealed record RedoCase : DocumentHistoryOp;
    public sealed record ClearCase : DocumentHistoryOp;
    public sealed record ShowHistoryCase : DocumentHistoryOp;
    public static readonly DocumentHistoryOp Query = new QueryCase();
    public static readonly DocumentHistoryOp Undo = new UndoCase();
    public static readonly DocumentHistoryOp Redo = new RedoCase();
    public static readonly DocumentHistoryOp Clear = new ClearCase();
    public static readonly DocumentHistoryOp ShowHistory = new ShowHistoryCase();
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentMutationPolicy(Option<RepaintRequest> Repaint = default) {
    public static DocumentMutationPolicy Default => new(Repaint: Some(RepaintRequest.Canvas));
    internal RepaintRequest RepaintOrDefault => Repaint.IfNone(RepaintRequest.Canvas);
}

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
public readonly record struct DocumentUniverseSnapshot(Seq<DocumentSnapshot> Documents, int Count);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentHistorySnapshot(bool IsEmpty, bool CanUndo, bool CanRedo, int UndoCount, int RedoCount);

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

[Union]
public partial record ObjectSpec {
    private ObjectSpec() { }
    public sealed record SliderCase(string Name, UiNumber Number, Option<string> Format, GripShape Shape, Color Colour) : ObjectSpec;
    public sealed record ToggleCase(string Name, bool State) : ObjectSpec;

    public static ObjectSpec Slider(string name, UiNumber number, string? format = null, GripShape shape = GripShape.Label, Color? colour = null) =>
        new SliderCase(Name: name, Number: number, Format: Optional(format), Shape: shape, Colour: colour ?? Colors.Crimson);
    public static ObjectSpec Toggle(string name, bool state) => new ToggleCase(Name: name, State: state);

    internal Fin<IDocumentObject> Build(Op op) =>
        Switch(
            state: op,
            sliderCase: static (key, s) =>
                from name in key.AcceptText(value: s.Name)
                from number in Optional(s.Number).ToFin(Fail: UiFault.InvalidInput(op: key, detail: "slider number is required"))
                select (IDocumentObject)new NumberSliderObject(userName: name, number: number) {
                    GripColour = s.Colour,
                    GripDisplay = s.Shape,
                    GripFormat = s.Format.IfNone("{0}"),
                },
            toggleCase: static (key, t) =>
                from name in key.AcceptText(value: t.Name)
                select (IDocumentObject)new ToggleObject(state: t.State) { UserName = name });
}

[Union]
public partial record DropCue {
    private DropCue() { }
    public sealed record NoneCase : DropCue;
    public sealed record SourceCase(Guid Id) : DropCue;
    public sealed record TargetCase(Guid Id) : DropCue;
    public sealed record BetweenCase(Guid SourceId, Guid TargetId) : DropCue;

    public static readonly DropCue None = new NoneCase();
    public static DropCue Source(Guid source) => new SourceCase(Id: source);
    public static DropCue Target(Guid target) => new TargetCase(Id: target);
    public static DropCue Between(Guid source, Guid target) => new BetweenCase(SourceId: source, TargetId: target);

    internal Fin<(Option<IParameter> Source, Option<IParameter> Target)> Resolve(GhObjectList objects, Op op) =>
        Switch(
            state: (Objects: objects, Op: op),
            noneCase: static (_, _) => Fin.Succ(value: (Source: Option<IParameter>.None, Target: Option<IParameter>.None)),
            sourceCase: static (state, cue) => ResolveCue(objects: state.Objects, id: cue.Id, label: nameof(Source), op: state.Op)
                .Map(static source => (Source: source, Target: Option<IParameter>.None)),
            targetCase: static (state, cue) => ResolveCue(objects: state.Objects, id: cue.Id, label: nameof(Target), op: state.Op)
                .Map(static target => (Source: Option<IParameter>.None, Target: target)),
            betweenCase: static (state, cue) =>
                from source in ResolveCue(objects: state.Objects, id: cue.SourceId, label: nameof(Source), op: state.Op)
                from target in ResolveCue(objects: state.Objects, id: cue.TargetId, label: nameof(Target), op: state.Op)
                select (Source: source, Target: target));

    private static Fin<Option<IParameter>> ResolveCue(GhObjectList objects, Guid id, string label, Op op) =>
        Op.Of(name: label).AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: $"{label} cue id is empty"))
            .Bind(valid => Optional(objects.FindParameter(instanceId: valid))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"{label} cue parameter {valid} not found"))
                .Map(static parameter => Some(parameter)));
}

internal sealed record DocumentRequest(DocumentOp Op) : GhUiRequest<DocumentResult> {
    internal override GrasshopperUiPolicy Policy => UiRail.DocumentPolicyFor(op: Op);
    internal override Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) => UiRail.DocumentDispatch(scope: scope, op: Op);
}

[Union]
public partial record DocumentTarget {
    private DocumentTarget() { }
    public sealed record SelectionCase : DocumentTarget;
    public sealed record ObjectsCase(Seq<Guid> Ids) : DocumentTarget;

    public static readonly DocumentTarget Selection = new SelectionCase();
    public static DocumentTarget Objects(params ReadOnlySpan<Guid> ids) => new ObjectsCase(Ids: toSeq(ids.ToArray()));

    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, DocumentTargetOp op, ActionList actions) =>
        Switch(
            state: (methods, objects, op, actions),
            selectionCase: static (s, _) => s.op.ApplySelected(methods: s.methods, actions: s.actions),
            objectsCase: static (s, target) => s.op.Apply(methods: s.methods, objects: s.objects, ids: target.Ids, actions: s.actions));
}

[Union]
public abstract partial record DocumentMutation {
    public sealed record TargetCase(DocumentTarget Subject, DocumentTargetOp Op) : DocumentMutation;
    public sealed record SelectionCase(SelectionOp Op) : DocumentMutation;
    public sealed record DisplayCase(DisplayOp Op) : DocumentMutation;
    public sealed record ClipboardCase(ClipboardOp Op) : DocumentMutation;
    public sealed record ComposeCase(DocumentTarget Subject, ComposeOp Op) : DocumentMutation;
    public sealed record DropCase(Guid ProxyId, PointF Location) : DocumentMutation;
    public sealed record PlaceCase(ObjectSpec Object, PointF Location, DropCue Cue) : DocumentMutation;
    public sealed record AddDependencyCase(PointF Location) : DocumentMutation;
    public sealed record IsolateCase(IsolateOptions Options) : DocumentMutation;

    public static DocumentMutation Target(DocumentTarget target, DocumentTargetOp op) => new TargetCase(Subject: target, Op: op);
    public static DocumentMutation Selection(SelectionOp op) => new SelectionCase(Op: op);
    public static DocumentMutation Display(DisplayOp op) => new DisplayCase(Op: op);
    public static DocumentMutation Clipboard(ClipboardOp op) => new ClipboardCase(Op: op);
    public static DocumentMutation Compose(ComposeOp op) => new ComposeCase(Subject: DocumentTarget.Selection, Op: op);
    public static DocumentMutation Compose(DocumentTarget target, ComposeOp op) => new ComposeCase(Subject: target, Op: op);
    public static DocumentMutation Drop(Guid proxyId, PointF location) => new DropCase(ProxyId: proxyId, Location: location);
    public static DocumentMutation Place(ObjectSpec obj, PointF location, DropCue? cue = null) => new PlaceCase(Object: obj, Location: location, Cue: cue ?? DropCue.None);
    public static DocumentMutation AddDependency(PointF location) => new AddDependencyCase(Location: location);
    public static DocumentMutation Isolate(IsolateOptions options = default) => new IsolateCase(Options: options);

    internal Fin<DocumentMutationReceipt> Apply(GhDocumentMethods methods, GhObjectList objects, ActionList actions) =>
        Switch(
            state: (methods, objects, actions),
            targetCase: static (s, target) => target.Subject.Apply(methods: s.methods, objects: s.objects, op: target.Op, actions: s.actions).Map(static changed => DocumentMutationReceipt.Count(changed: changed)),
            selectionCase: static (s, selection) => UiRail.SelectionDispatch(methods: s.methods, op: selection.Op).Map(static changed => DocumentMutationReceipt.Count(changed: changed)),
            displayCase: static (s, display) => UiRail.DisplayDispatch(methods: s.methods, op: display.Op, actions: s.actions).Map(static changed => DocumentMutationReceipt.Count(changed: changed)),
            clipboardCase: static (s, clipboard) => UiRail.ClipboardDispatch(methods: s.methods, op: clipboard.Op, actions: s.actions).Map(static changed => DocumentMutationReceipt.Count(changed: changed)),
            composeCase: static (s, compose) => UiRail.ComposeDispatch(methods: s.methods, objects: s.objects, target: compose.Subject, op: compose.Op, actions: s.actions)
                .Map(static created => created.Map(static id => DocumentMutationReceipt.CreatedObject(id: id)).IfNone(DocumentMutationReceipt.None)),
            dropCase: static (s, drop) =>
                from proxy in Op.Of(name: nameof(Drop)).AcceptValue(value: drop.ProxyId)
                    .MapFail(static _ => UiFault.InvalidInput(op: Op.Of(name: nameof(Drop)), detail: "empty proxy id"))
                from location in Op.Of(name: nameof(Drop)).AcceptPoint(value: drop.Location, detail: "non-finite location")
                select s.methods.DropObject(obj: proxy, location: location, actions: s.actions) switch {
                    true => DocumentMutationReceipt.Count(changed: 1),
                    false => DocumentMutationReceipt.None,
                },
            placeCase: static (s, place) =>
                from spec in Optional(place.Object).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Place)), detail: "object spec is required"))
                from location in Op.Of(name: nameof(Place)).AcceptPoint(value: place.Location, detail: "non-finite location")
                from obj in spec.Build(op: Op.Of(name: nameof(Place)))
                from cue in Optional(place.Cue).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Place)), detail: "drop cue is required"))
                from resolved in cue.Resolve(objects: s.objects, op: Op.Of(name: nameof(Place)))
                let native = (resolved.Source.IsSome || resolved.Target.IsSome) switch {
                    true => s.methods.DropObject(
                        obj: obj,
                        location: location,
                        sourceCue: resolved.Source.Map(static value => (IParameter?)value).IfNone((IParameter?)null),
                        targetCue: resolved.Target.Map(static value => (IParameter?)value).IfNone((IParameter?)null),
                        actions: s.actions),
                    false => s.methods.DropObject(obj: obj, location: location, actions: s.actions),
                }
                select native switch {
                    true => DocumentMutationReceipt.CreatedObject(id: obj.InstanceId),
                    false => DocumentMutationReceipt.None,
                },
            addDependencyCase: static (s, dependency) => Op.Of(name: nameof(AddDependency))
                .AcceptPoint(value: dependency.Location, detail: "non-finite location")
                .Map(valid => Optional(s.methods.AddDependency(location: valid, actions: s.actions))
                    .Map(static listen => DocumentMutationReceipt.CreatedObject(id: listen.InstanceId))
                    .IfNone(DocumentMutationReceipt.None)),
            isolateCase: static (s, isolate) =>
                from selected in Fin.Succ(value: toSeq(s.objects.SelectedObjects))
                from primary in selected.Head
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Isolate)), detail: "no object selected to isolate"))
                let before = s.actions.Count
                from _ in Try.lift(f: () => {
                    s.methods.IsolateObject(obj: primary,
                        pins: isolate.Options.IncludePins,
                        inputs: isolate.Options.IncludeInputs,
                        outputs: isolate.Options.IncludeOutputs,
                        omit: selected.ToInstanceIdSet(),
                        actions: s.actions);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Isolate)), detail: $"IsolateObject threw: {error.Message}"))
                select (s.actions.Count > before) switch {
                    true => DocumentMutationReceipt.Count(changed: 1),
                    false => DocumentMutationReceipt.None,
                });
}

[Union]
public abstract partial record DocumentTargetOp {
    public sealed record ShowCase : DocumentTargetOp;
    public sealed record HideCase : DocumentTargetOp;
    public sealed record ToggleVisibilityCase : DocumentTargetOp;
    public sealed record EnableCase : DocumentTargetOp;
    public sealed record DisableCase : DocumentTargetOp;
    public sealed record DeleteCase(bool DataOnly, Seq<WireEnds> Wires) : DocumentTargetOp;
    public sealed record ColourCase(Option<GhColour> Override) : DocumentTargetOp;
    public static readonly DocumentTargetOp Show = new ShowCase();
    public static readonly DocumentTargetOp Hide = new HideCase();
    public static readonly DocumentTargetOp ToggleVisibility = new ToggleVisibilityCase();
    public static readonly DocumentTargetOp Enable = new EnableCase();
    public static readonly DocumentTargetOp Disable = new DisableCase();
    public static DocumentTargetOp Delete(bool dataOnly = false, Seq<WireEnds> wires = default) => new DeleteCase(DataOnly: dataOnly, Wires: wires);
    public static DocumentTargetOp Style(GhColour colour) => new ColourCase(Override: Some(colour));
    public static readonly DocumentTargetOp ClearStyle = new ColourCase(Override: Option<GhColour>.None);
    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, Seq<Guid> ids, ActionList actions) =>
        ids.TraverseM(id => Optional(objects.Find(instanceId: id))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentTarget)), detail: $"object {id} not found")))
        .Map(static resolved => resolved.ToArray())
        .Bind(targets => Dispatch(methods: methods, actions: actions, targets: targets)).As();

    internal Fin<int> ApplySelected(GhDocumentMethods methods, ActionList actions) =>
        Dispatch(methods: methods, actions: actions, targets: null);

    // Unified target/selection dispatcher. Non-null `targets` routes through *Objects(targets, actions);
    // null `targets` routes through *Selected(actions). Centralizes seven native verbs in one site.
    // Null sentinel over Option<...> because CSP0705 forbids Option.Match mid-Switch-arm.
    private Fin<int> Dispatch(GhDocumentMethods methods, ActionList actions, IDocumentObject[]? targets) =>
        Switch(
            state: (methods, targets, actions),
            showCase: static (s, _) => Fin.Succ(value: s.targets is null
                ? s.methods.ShowSelected(actions: s.actions)
                : s.methods.ShowObjects(objects: s.targets, actions: s.actions)),
            hideCase: static (s, _) => Fin.Succ(value: s.targets is null
                ? s.methods.HideSelected(actions: s.actions)
                : s.methods.HideObjects(objects: s.targets, actions: s.actions)),
            toggleVisibilityCase: static (s, _) => Fin.Succ(value: s.targets is null
                ? s.methods.ToggleDisplaySelected(actions: s.actions)
                : s.methods.ToggleDisplayObjects(objects: s.targets, actions: s.actions)),
            enableCase: static (s, _) => Fin.Succ(value: s.targets is null
                ? s.methods.EnableSelected(actions: s.actions)
                : s.methods.EnableObjects(objects: s.targets, actions: s.actions)),
            disableCase: static (s, _) => Fin.Succ(value: s.targets is null
                ? s.methods.DisableSelected(actions: s.actions)
                : s.methods.DisableObjects(objects: s.targets, actions: s.actions)),
            deleteCase: static (s, delete) => Fin.Succ(value: s.targets is null
                ? delete.DataOnly
                    ? s.methods.DeleteSelectionData(actions: s.actions)
                    : delete.Wires.IsEmpty
                        ? s.methods.DeleteSelection(actions: s.actions)
                        : s.methods.DeleteObjects(objects: [], wires: [.. delete.Wires], actions: s.actions)
                : delete.DataOnly
                    ? s.methods.DeleteObjectData(objects: s.targets, actions: s.actions)
                    : s.methods.DeleteObjects(objects: s.targets, wires: [.. delete.Wires], actions: s.actions)),
            colourCase: static (s, colour) => Fin.Succ(value: s.targets is null
                ? s.methods.SetColourOverrideSelected(
                    colour: colour.Override.Map(static value => (GhColour?)value).IfNone((GhColour?)null),
                    actions: s.actions)
                : s.methods.SetColourOverrideObjects(
                    objects: s.targets,
                    colour: colour.Override.Map(static value => (GhColour?)value).IfNone((GhColour?)null),
                    actions: s.actions)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class UiRail {
    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static Fin<DocumentResult> DocumentDispatch(GrasshopperUi.Scope scope, DocumentOp op) =>
        op.Switch(
            state: scope,
            queryCase: static (s, q) => QueryDispatch(scope: s, query: q.Request),
            mutateCase: static (s, m) => MutateDispatch(scope: s, mutations: m.Mutations, policy: m.Policy),
            historyCase: static (s, h) => HistoryDispatch(scope: s, op: h.Request));

    internal static GrasshopperUiPolicy DocumentPolicyFor(DocumentOp op) =>
        op.Switch(
            queryCase: static q => q.Request is DocumentQuery.UniverseCase ? GrasshopperUiPolicy.Read : GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
            mutateCase: static mutate => GrasshopperUiPolicy.Document(repaint: mutate.Policy.RepaintOrDefault),
            historyCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None));

    private static Fin<DocumentResult> QueryDispatch(GrasshopperUi.Scope scope, DocumentQuery query) => query switch {
        DocumentQuery.SnapshotCase =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            select (DocumentResult)new DocumentResult.SnapshotResult(Snapshot: DocumentSnapshotOf(document: doc, objects: objs)),
        DocumentQuery.ObjectsCase o =>
            scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectListResult(
                Snapshots: toSeq((o.Scope switch {
                    ObjectsScope.PrimaryCase => objs.Forwards,
                    ObjectsScope.PrimaryAndSecondaryCase => objs.PrimaryAndSecondary,
                    ObjectsScope.SelectedCase => objs.SelectedObjects,
                    _ => [],
                }).Select(DocumentObjectSnapshotOf)))),
        DocumentQuery.ObjectCase obj =>
            Op.Of(name: nameof(DocumentQuery.Object)).AcceptValue(value: obj.Id)
                .MapFail(_ => UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentQuery.Object)), detail: "empty Guid"))
                .Bind(valid => scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectResult(
                    Snapshot: Optional(objs.Find(instanceId: valid)).Map(DocumentObjectSnapshotOf)))),
        DocumentQuery.ParameterCase param =>
            Op.Of(name: nameof(DocumentQuery.Parameter)).AcceptValue(value: param.Id)
                .MapFail(_ => UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentQuery.Parameter)), detail: "empty Guid"))
                .Bind(valid => scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ParameterResult(
                    Snapshot: Optional(objs.FindParameter(instanceId: valid)).Map(ParameterSnapshotOf)))),
        DocumentQuery.GripCase g =>
            from point in Op.Of(name: nameof(DocumentQuery.Grip)).AcceptPoint(value: g.Point, detail: "non-finite point")
            from objs in scope.NeedObjects()
            select (DocumentResult)new DocumentResult.GripResult(Grip: g.Kind switch {
                GripKind.InletCase => Optional(objs.FindByInlet(point: point))
                    .Map(static p => new DocumentGripSnapshot(Parameter: p.InstanceId, InletWithinRange: true, OutletWithinRange: false)),
                GripKind.OutletCase => Optional(objs.FindByOutlet(point: point))
                    .Map(static p => new DocumentGripSnapshot(Parameter: p.InstanceId, InletWithinRange: false, OutletWithinRange: true)),
                GripKind.InletOrOutletCase => Optional(objs.FindByInletOrOutlet(point: point))
                    .Filter(static found => found.parameter is not null)
                    .Map(static found => new DocumentGripSnapshot(Parameter: found.parameter.InstanceId, InletWithinRange: found.inletWithinRange, OutletWithinRange: found.outletWithinRange)),
                _ => Option<DocumentGripSnapshot>.None,
            }),
        DocumentQuery.FindCase f =>
            scope.NeedObjects().Bind(objs => FindBy(objects: objs, criterion: f.Criterion)
                .Map(matches => (DocumentResult)new DocumentResult.FindResult(Matches: matches))),
        DocumentQuery.MetaNamesCase =>
            scope.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.MetaNamesResult(Names: toSeq(objs.MetaNames()))),
        DocumentQuery.UniverseCase =>
            Universe().Map(snapshot => (DocumentResult)new DocumentResult.UniverseResult(Snapshot: snapshot)),
        _ => Fin.Fail<DocumentResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(QueryDispatch)), detail: "unknown DocumentQuery")),
    };

    private static Fin<DocumentResult> MutateDispatch(GrasshopperUi.Scope scope, Seq<DocumentMutation> mutations, DocumentMutationPolicy policy) =>
        RunMutation(scope: scope, op: Op.Of(name: nameof(DocumentOp.Mutate)),
            mutate: (methods, objects, actions) => mutations.TraverseM(m => m.Apply(methods: methods, objects: objects, actions: actions))
                .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, receipt) => sum + receipt)).As(),
            policy: policy)
            .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta));

    internal static Fin<int> SelectionDispatch(GhDocumentMethods methods, SelectionOp op) =>
        op.Switch(
            state: methods,
            allCase: static (m, _) => Fin.Succ(value: m.SelectAll()),
            noneCase: static (m, _) => Fin.Succ(value: m.DeselectAll()),
            invertCase: static (m, _) => Fin.Succ(value: m.InvertSelection()),
            growCase: static (m, g) => Fin.Succ(value: m.GrowSelection(upstream: g.Upstream, downstream: g.Downstream)),
            shiftCase: static (m, s) => Fin.Succ(value: m.ShiftSelection(upstream: s.Upstream)));

    internal static Fin<int> DisplayDispatch(GhDocumentMethods methods, DisplayOp op, ActionList actions) =>
        op.Switch(
            state: (methods, actions),
            inputsCase: static (s, inputs) => Fin.Succ(value: inputs.Change is VisibilityChange.ShowCase
                ? s.methods.ShowSelectedInputs(actions: s.actions)
                : s.methods.HideSelectedInputs(actions: s.actions)),
            outputsCase: static (s, outputs) => Fin.Succ(value: outputs.Change is VisibilityChange.ShowCase
                ? s.methods.ShowSelectedOutputs(actions: s.actions)
                : s.methods.HideSelectedOutputs(actions: s.actions)));

    internal static Fin<int> ClipboardDispatch(GhDocumentMethods methods, ClipboardOp op, ActionList actions) =>
        op.Switch(
            state: (methods, actions),
            copyCase: static (s, c) => ValidateClipboard(name: nameof(ClipboardOp.Copy), clipboard: c.Kind).Map(k => s.methods.CopySelection(clipboard: k) ? 1 : 0),
            cutCase: static (s, c) => ValidateClipboard(name: nameof(ClipboardOp.Cut), clipboard: c.Kind).Map(k => s.methods.CutSelection(clipboard: k, actions: s.actions) ? 1 : 0),
            pasteCase: static (s, p) => ValidateClipboard(name: nameof(ClipboardOp.Paste), clipboard: p.Kind).Map(k => s.methods.PasteFromClipboard(clipboard: k, behaviour: p.Behaviour, actions: s.actions) ? 1 : 0),
            pasteGh1XmlCase: static (s, _) => Fin.Succ(value: s.methods.PasteGrasshopper1XmlFromClipboard(actions: s.actions) ? 1 : 0));

    internal static Fin<Option<Guid>> ComposeDispatch(GhDocumentMethods methods, GhObjectList objects, DocumentTarget target, ComposeOp op, ActionList actions) =>
        target switch {
            DocumentTarget.SelectionCase => op switch {
                ComposeOp.GroupCase g => Fin.Succ(value: Optional(methods.GroupSelection(
                    name: g.Name.IfNone(string.Empty),
                    colour: g.Colour.Map(static c => (GhOpenColorFamily?)c).IfNone((GhOpenColorFamily?)null),
                    actions: actions)).Map(static r => r.InstanceId).Filter(static id => id != Guid.Empty)),
                ComposeOp.ChainCase =>
                    PreflightCompose(op: Op.Of(name: nameof(ComposeOp.Chain)), check: () => methods.CanCreateChain(objects.SelectedObjects, out string whyNot) ? (true, whyNot) : (false, whyNot))
                        .Map(_ => Optional(methods.ChainSelection(actions: actions)).Map(static c => c.InstanceId).Filter(static id => id != Guid.Empty)),
                ComposeOp.ClusterCase =>
                    PreflightCompose(op: Op.Of(name: nameof(ComposeOp.Cluster)), check: () => methods.CanCreateCluster(objects.SelectedObjects, out string whyNot) ? (true, whyNot) : (false, whyNot))
                        .Map(_ => Optional(methods.ClusterSelection(actions: actions)).Map(static c => c.InstanceId).Filter(static id => id != Guid.Empty)),
                _ => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: "unknown compose op")),
            },
            DocumentTarget.ObjectsCase selected => selected.Ids
                .TraverseM(id => Optional(objects.Find(instanceId: id))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentTarget.Objects)), detail: $"object {id} not found")))
                .As()
                .Map(static resolved => resolved.ToArray())
                .Bind(targets => op switch {
                    ComposeOp.GroupCase g => Fin.Succ(value: Optional(methods.GroupObjects(
                        objects: targets,
                        name: g.Name.IfNone(string.Empty),
                        colour: g.Colour.Map(static c => (GhOpenColorFamily?)c).IfNone((GhOpenColorFamily?)null),
                        actions: actions)).Map(static r => r.InstanceId).Filter(static id => id != Guid.Empty)),
                    ComposeOp.ChainCase =>
                        PreflightCompose(op: Op.Of(name: nameof(ComposeOp.Chain)), check: () => methods.CanCreateChain(targets, out string whyNot) ? (true, whyNot) : (false, whyNot))
                            .Map(_ => Optional(methods.ChainObjects(objects: targets, actions: actions)).Map(static c => c.InstanceId).Filter(static id => id != Guid.Empty)),
                    ComposeOp.ClusterCase =>
                        PreflightCompose(op: Op.Of(name: nameof(ComposeOp.Cluster)), check: () => methods.CanCreateCluster(targets, out string whyNot) ? (true, whyNot) : (false, whyNot))
                            .Map(_ => Optional(methods.ClusterObjects(objects: targets, actions: actions)).Map(static c => c.InstanceId).Filter(static id => id != Guid.Empty)),
                    _ => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: "unknown compose op")),
                }),
            _ => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: "unknown target")),
        };

    private static Fin<ClipboardKind> ValidateClipboard(string name, ClipboardKind clipboard) =>
        clipboard switch {
            ClipboardKind.Instance => Fin.Fail<ClipboardKind>(error: UiFault.InvalidInput(op: Op.Of(name: name), detail: "ClipboardKind.Instance is not supported")),
            _ => Fin.Succ(value: clipboard),
        };

    private static Fin<Unit> PreflightCompose(Op op, Func<(bool Ok, string WhyNot)> check) =>
        check() switch {
            (true, _) => Fin.Succ(value: unit),
            (false, string whyNot) => Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: string.IsNullOrWhiteSpace(value: whyNot) ? "pre-flight rejected" : whyNot)),
        };

    private static Fin<Seq<DocumentObjectSnapshot>> FindBy(GhObjectList objects, FindCriterion criterion) => criterion switch {
        FindCriterion.NearPointCase n =>
            from point in Op.Of(name: nameof(FindBy)).AcceptPoint(value: n.Point, detail: "non-finite point")
            from maxResults in Optional(n.MaxResults).Filter(static count => count > 0)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: "maxResults must be positive"))
            from maxDistance in Op.Of(name: nameof(FindBy)).AcceptFinite(value: n.MaxDistance, detail: "maxDistance must be finite and non-negative", nonNegative: true)
            select toSeq(objects.FindNear<IDocumentObject>(locus: point, maxResults: maxResults, maxDistance: maxDistance)).Map(DocumentObjectSnapshotOf),
        FindCriterion.ByDrawOrderCase d =>
            Fin.Succ(value: toSeq(objects.ObjectsByDrawOrder(includeForeground: d.Foreground, includeBackground: d.Background)).Map(DocumentObjectSnapshotOf)),
        FindCriterion.UpstreamCase u =>
            Wire.TraverseObjects(objects: objects, startParameterId: u.ParameterId, direction: WireTraversal.Upstream)
                .Map(matches => matches.Map(DocumentObjectSnapshotOf)),
        FindCriterion.DownstreamCase d =>
            Wire.TraverseObjects(objects: objects, startParameterId: d.ParameterId, direction: WireTraversal.Downstream)
                .Map(matches => matches.Map(DocumentObjectSnapshotOf)),
        FindCriterion.ByNameCase n =>
            Op.Of(name: nameof(FindCriterion.ByName)).AcceptText(value: n.Substring)
                .Map(text => toSeq(objects.Forwards)
                .Filter(o => o.Nomen.Name.Contains(value: text, comparisonType: n.CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                .Map(DocumentObjectSnapshotOf)),
        _ => Fin.Fail<Seq<DocumentObjectSnapshot>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(FindBy)), detail: "unknown criterion")),
    };

    private static Fin<DocumentUniverseSnapshot> Universe() =>
        Optional(GhEditor.Instance?.Documents)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(GhEditor.Documents)))
            .Map(documents => toSeq(documents.All.SelectMany(stack => stack.Documents))
                .Map(document => DocumentSnapshotOf(document: document, objects: document.Objects)))
            .Map(static snapshots => new DocumentUniverseSnapshot(Documents: snapshots, Count: snapshots.Count));

    private static Fin<DocumentResult> HistoryDispatch(GrasshopperUi.Scope scope, DocumentHistoryOp op) =>
        op.Switch(
            state: scope,
            queryCase: static (s, _) => s.NeedDocument().Map(document => (DocumentResult)new DocumentResult.HistoryResult(Snapshot: HistorySnapshotOf(document: document))),
            undoCase: static (s, _) => MutateHistory(scope: s, op: Op.Of(name: nameof(DocumentHistoryOp.Undo)), run: static document => document.Undo.Undo()),
            redoCase: static (s, _) => MutateHistory(scope: s, op: Op.Of(name: nameof(DocumentHistoryOp.Redo)), run: static document => document.Undo.Redo()),
            clearCase: static (s, _) => MutateHistory(scope: s, op: Op.Of(name: nameof(DocumentHistoryOp.Clear)), run: static document => document.Undo.Clear()),
            showHistoryCase: static (s, _) => DispatchShowHistory(scope: s));

    private static Fin<DocumentResult> DispatchShowHistory(GrasshopperUi.Scope scope) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        from shown in Try.lift<DocumentResult>(f: () => {
            _ = History.ShowHistory(canvas: canvas);
            return new DocumentResult.HistoryResult(Snapshot: HistorySnapshotOf(document: document));
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(DocumentHistoryOp.ShowHistory)), detail: error.Message))
        select shown;

    private static Fin<DocumentResult> MutateHistory(GrasshopperUi.Scope scope, Op op, Action<GhDocument> run) =>
        from document in scope.NeedDocument()
        from _ in Try.lift(f: () => { run(document); return unit; }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: error.Message))
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: HistorySnapshotOf(document: document));

    internal static DocumentHistorySnapshot HistorySnapshotOf(GhDocument document) =>
        new(
            IsEmpty: document.Undo.IsEmpty,
            CanUndo: document.Undo.FirstUndo is not null,
            CanRedo: document.Undo.FirstRedo is not null,
            UndoCount: document.Undo.CentralUndoSequence.Count(),
            RedoCount: document.Undo.CentralRedoSequence.Count());

    private static Fin<Snapshot<DocumentMutationDelta>> RunMutation(
        GrasshopperUi.Scope scope,
        Op op,
        Func<GhDocumentMethods, GhObjectList, ActionList, Fin<DocumentMutationReceipt>> mutate,
        DocumentMutationPolicy policy) =>
        from methods in scope.NeedMethods()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        let actions = ActionList.Empty
        from receipt in mutate(arg1: methods, arg2: objects, arg3: actions).Bind(result => result.Changed switch {
            >= 0 => Fin.Succ(value: result),
            _ => Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"count={result.Changed}"))),
        })
        from _ in CommitActions(document: document, op: op, actions: actions)
        select Snapshot.Of(
            payload: new DocumentMutationDelta(Changed: receipt.Changed, After: DocumentSnapshotOf(document: document, objects: objects), Created: receipt.Created),
            ownerId: Some(document.Hash));

    internal static Fin<Unit> CommitActions(GhDocument document, Op op, ActionList actions) =>
        actions.Count switch {
            <= 0 => Fin.Succ(value: unit),
            _ => Try.lift(f: () => {
                document.Undo.Do(name: VerbNounOf(op: op), actions: actions);
                return unit;
            }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: $"History.Do threw: {error.Message}")),
        };

    // Op names follow "Noun.Verb"; the History panel renders VerbNoun as "Verb Noun" (e.g. "Connect Wire"),
    // so we split rather than emit a flat "Edit Wire.Connect" label.
    internal static VerbNoun VerbNounOf(Op op) {
        string name = op.ToString();
        int dot = name.IndexOf(value: '.', comparisonType: StringComparison.Ordinal);
        return dot > 0 && dot < name.Length - 1
            ? (Verb: name[(dot + 1)..], Noun: name[..dot])
            : (Verb: "Edit", Noun: name);
    }

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

    internal static DocumentObjectSnapshot DocumentObjectSnapshotOf(IDocumentObject obj) =>
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
            HasColourOverride: parameter.ColourOverride is not null);

}

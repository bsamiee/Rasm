using System.Globalization;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Special;
using Grasshopper2.SpecialObjects;
using Grasshopper2.UI.Slider;
using Grasshopper2.Undo;
using GhColour = Grasshopper2.Types.Colour.Colour;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhEditor = Grasshopper2.UI.Editor;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using GhOpenColorFamily = Eto.Drawing.OpenColor.Family;
using GhSnippet = Grasshopper2.Framework.Snippet;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record ObjectScope {
    private ObjectScope() { }
    public sealed record SelectionCase : ObjectScope;
    public sealed record ObjectsCase(Seq<Guid> Ids) : ObjectScope;
    public sealed record PrimaryCase : ObjectScope;
    public sealed record PrimaryAndSecondaryCase : ObjectScope;

    public static readonly ObjectScope Selection = new SelectionCase();
    public static ObjectScope Objects(params ReadOnlySpan<Guid> ids) => new ObjectsCase(Ids: toSeq(ids.ToArray()));
    public static readonly ObjectScope Primary = new PrimaryCase();
    public static readonly ObjectScope PrimaryAndSecondary = new PrimaryAndSecondaryCase();

    internal IEnumerable<IDocumentObject> Resolve(GhObjectList objects) =>
        Switch(
            state: objects,
            selectionCase: static (objs, _) => objs.SelectedObjects,
            objectsCase: static (objs, o) => o.Ids.Choose(id => Optional(objs.Find(instanceId: id))).OfType<IDocumentObject>(),
            primaryCase: static (objs, _) => objs.Forwards,
            primaryAndSecondaryCase: static (objs, _) => objs.PrimaryAndSecondary);

    // Per-arm rejection is the single gate (feedback_per_arm_op_provenance): the primary arms own the reject,
    // so no standalone pre-guard shadows them into unreachability.
    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, DocumentTargetOp op, ActionList actions) =>
        Switch(
            state: (methods, objects, op, actions),
            selectionCase: static (s, _) => s.op.ApplySelected(methods: s.methods, actions: s.actions),
            objectsCase: static (s, target) => s.op.Apply(methods: s.methods, objects: s.objects, ids: target.Ids, actions: s.actions),
            primaryCase: static (_, _) => ScopeUse.TargetMutation.RejectPrimary<int>(op: Op.Of(name: nameof(ObjectScope))),
            primaryAndSecondaryCase: static (_, _) => ScopeUse.TargetMutation.RejectPrimary<int>(op: Op.Of(name: nameof(ObjectScope))));
}

[SmartEnum<int>]
internal sealed partial class ScopeUse {
    public static readonly ScopeUse TargetMutation = new(key: 0, rejectsPrimaryDetail: static () => "Primary scope is not valid for target mutation");
    public static readonly ScopeUse Compose = new(key: 1, rejectsPrimaryDetail: static () => "Primary scope is not valid for compose");
    public static readonly ScopeUse LayoutMeasure = new(key: 2, rejectsPrimaryDetail: static () => "Primary scope is not valid for layout measure");

    [UseDelegateFromConstructor]
    internal partial string RejectsPrimaryDetail();

    // Single rejection projection shared by every primary-scope guard arm (ObjectScope.Apply,
    // UiRail.ComposeDispatch, Layout.Measure, RequireMutationScope) so the failure construction lives once.
    internal Fin<T> RejectPrimary<T>(Op op) =>
        Fin.Fail<T>(error: UiFault.InvalidInput(op: op, detail: RejectsPrimaryDetail()));
}

[SmartEnum<int>]
internal sealed partial class DocumentDeleteMode {
    public static readonly DocumentDeleteMode SelectionDataOnly = new(key: 0, run: static (m, _, _, a) => m.DeleteSelectionData(actions: a));
    public static readonly DocumentDeleteMode SelectionFull = new(key: 1, run: static (m, _, _, a) => m.DeleteSelection(actions: a));
    public static readonly DocumentDeleteMode SelectionWires = new(key: 2, run: static (m, _, w, a) => m.DeleteObjects(objects: [], wires: [.. w], actions: a));
    public static readonly DocumentDeleteMode ObjectsDataOnly = new(key: 3, run: static (m, t, _, a) => m.DeleteObjectData(objects: t!, actions: a));
    public static readonly DocumentDeleteMode ObjectsFull = new(key: 4, run: static (m, t, w, a) => m.DeleteObjects(objects: t!, wires: [.. w], actions: a));

    [UseDelegateFromConstructor]
    internal partial int Run(GhDocumentMethods methods, IDocumentObject[]? targets, Seq<WireEnds> wires, ActionList actions);

    internal static DocumentDeleteMode Resolve(IDocumentObject[]? targets, bool dataOnly, Seq<WireEnds> wires) =>
        (targets, dataOnly, wires.IsEmpty) switch {
            (null, true, _) => SelectionDataOnly,
            (null, false, true) => SelectionFull,
            (null, false, false) => SelectionWires,
            (_, true, _) => ObjectsDataOnly,
            (_, false, _) => ObjectsFull,
        };
}

[SkipUnionOps]
[Union]
public partial record VisibilityChange {
    private VisibilityChange() { }
    public sealed record ShowCase : VisibilityChange;
    public sealed record HideCase : VisibilityChange;
    public static readonly VisibilityChange Show = new ShowCase();
    public static readonly VisibilityChange Hide = new HideCase();

    internal bool IsShow => Switch(showCase: static _ => true, hideCase: static _ => false);
}

[SkipUnionOps]
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

[SkipUnionOps]
[Union]
public partial record ClipboardOp {
    private ClipboardOp() { }
    public sealed record VerbCase(ClipboardVerb Verb, ClipboardKind Kind, PasteBehaviour Behaviour) : ClipboardOp;

    public static ClipboardOp Copy(ClipboardKind kind = ClipboardKind.Global) => new VerbCase(Verb: ClipboardVerb.Copy, Kind: kind, Behaviour: default);
    public static ClipboardOp Cut(ClipboardKind kind = ClipboardKind.Global) => new VerbCase(Verb: ClipboardVerb.Cut, Kind: kind, Behaviour: default);
    public static ClipboardOp Paste(ClipboardKind kind = ClipboardKind.Global, PasteBehaviour behaviour = PasteBehaviour.Centre | PasteBehaviour.DeselectOldObjects | PasteBehaviour.SelectNewObjects) =>
        new VerbCase(Verb: ClipboardVerb.Paste, Kind: kind, Behaviour: behaviour);
    public static readonly ClipboardOp PasteGh1Xml = new VerbCase(Verb: ClipboardVerb.PasteGh1Xml, Kind: default, Behaviour: default);
}

[SkipUnionOps]
[Union]
public partial record ComposeOp {
    private ComposeOp() { }
    public sealed record GroupCase(Option<string> Name, Option<GhOpenColorFamily> Colour) : ComposeOp;
    public sealed record LinkCase(DocumentLinkKind Kind) : ComposeOp;
    public static ComposeOp Group(string? name = null, GhOpenColorFamily? colour = null) =>
        new GroupCase(Name: Optional(name), Colour: Optional(colour));
    public static readonly ComposeOp Chain = new LinkCase(Kind: DocumentLinkKind.Chain);
    public static readonly ComposeOp Cluster = new LinkCase(Kind: DocumentLinkKind.Cluster);

    // Apply for both Selection (targets=null → *Selection method) and Objects (targets=IDocumentObject[] →
    // *Objects method). The preflight check on Chain/Cluster uses targets when present, falling back to
    // GhObjectList.SelectedObjects when selection-scope.
    internal Fin<Option<Guid>> Apply(GhDocumentMethods methods, GhObjectList objects, IDocumentObject[]? targets, ActionList actions) =>
        Switch(state: (methods, objects, targets, actions),
            groupCase: static (s, g) => {
                string name = g.Name.IfNone(string.Empty);
                GhOpenColorFamily? colour = g.Colour.OrNullable();
                return Fin.Succ(value: Optional(s.targets is null
                    ? s.methods.GroupSelection(name: name, colour: colour, actions: s.actions)
                    : s.methods.GroupObjects(objects: s.targets, name: name, colour: colour, actions: s.actions))
                    .Map(static r => r.InstanceId).Bind(static id => id.NonEmpty()));
            },
            linkCase: static (s, link) => Link(state: s, kind: link.Kind));

    private static Fin<Option<Guid>> Link(
        (GhDocumentMethods methods, GhObjectList objects, IDocumentObject[]? targets, ActionList actions) state,
        DocumentLinkKind kind) =>
        UiRail.PreflightCompose(
            op: kind.Op,
            check: () => kind.CanCreate(methods: state.methods, targets: state.targets ?? state.objects.SelectedObjects, whyNot: out string whyNot) ? (true, whyNot) : (false, whyNot))
            .Map(_ => Optional(kind.Create(methods: state.methods, targets: state.targets, actions: state.actions)).Map(static c => c.InstanceId).Bind(static id => id.NonEmpty()));
}

[SmartEnum<int>]
public sealed partial class DocumentLinkKind {
    private delegate bool CanCreateFn(GhDocumentMethods methods, IEnumerable<IDocumentObject> targets, out string whyNot);
    private delegate IDocumentObject? CreateFn(GhDocumentMethods methods, IDocumentObject[]? targets, ActionList actions);

    public Op Op { get; }

    public static readonly DocumentLinkKind Chain = new(
        key: 0,
        op: Op.Of(name: nameof(Chain)),
        canCreate: static (GhDocumentMethods methods, IEnumerable<IDocumentObject> targets, out string whyNot) => methods.CanCreateChain(objects: targets, whyNot: out whyNot),
        create: static (methods, targets, actions) => targets is null ? methods.ChainSelection(actions: actions) : methods.ChainObjects(objects: targets, actions: actions));
    public static readonly DocumentLinkKind Cluster = new(
        key: 1,
        op: Op.Of(name: nameof(Cluster)),
        canCreate: static (GhDocumentMethods methods, IEnumerable<IDocumentObject> targets, out string whyNot) => methods.CanCreateCluster(objects: targets, whyNot: out whyNot),
        create: static (methods, targets, actions) => targets is null ? methods.ClusterSelection(actions: actions) : methods.ClusterObjects(objects: targets, actions: actions));

    [UseDelegateFromConstructor]
    internal partial bool CanCreate(GhDocumentMethods methods, IEnumerable<IDocumentObject> targets, out string whyNot);

    [UseDelegateFromConstructor]
    internal partial IDocumentObject? Create(GhDocumentMethods methods, IDocumentObject[]? targets, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class GripKind {
    private delegate Option<DocumentGripSnapshot> GripResolver(GhObjectList objs, PointF point);

    public static readonly GripKind Inlet = new(0, static (o, p) => Optional(o.FindByInlet(point: p))
        .Map(static x => new DocumentGripSnapshot(Parameter: x.InstanceId, InletWithinRange: true, OutletWithinRange: false)));
    public static readonly GripKind Outlet = new(1, static (o, p) => Optional(o.FindByOutlet(point: p))
        .Map(static x => new DocumentGripSnapshot(Parameter: x.InstanceId, InletWithinRange: false, OutletWithinRange: true)));
    public static readonly GripKind InletOrOutlet = new(2, static (o, p) => Optional(o.FindByInletOrOutlet(point: p))
        .Filter(static f => f.parameter is not null)
        .Map(static f => new DocumentGripSnapshot(Parameter: f.parameter.InstanceId, InletWithinRange: f.inletWithinRange, OutletWithinRange: f.outletWithinRange)));

    [UseDelegateFromConstructor]
    internal partial Option<DocumentGripSnapshot> Resolve(GhObjectList objs, PointF point);
}

[SkipUnionOps]
[Union]
public partial record FindCriterion {
    private FindCriterion() { }
    public sealed record NearPointCase(PointF Point, int MaxResults, float MaxDistance) : FindCriterion;
    public sealed record ByDrawOrderCase(bool Foreground, bool Background) : FindCriterion;
    public sealed record GraphCase(GraphKey Anchor, WireTraversal Direction, WireObjectLimit MaxObjects) : FindCriterion;
    public sealed record ByNameCase(string Substring, bool CaseInsensitive, ObjectScope Scope) : FindCriterion;

    public static FindCriterion Near(PointF point, int maxResults = 16, float maxDistance = UiTolerance.PickRadius) => new NearPointCase(Point: point, MaxResults: maxResults, MaxDistance: maxDistance);
    public static FindCriterion DrawOrder(bool foreground = true, bool background = true) => new ByDrawOrderCase(Foreground: foreground, Background: background);
    public static FindCriterion Upstream(Guid parameterId, WireObjectLimit? maxObjects = null) => Upstream(anchor: GraphKey.Parameter(id: parameterId), maxObjects: maxObjects);
    public static FindCriterion Downstream(Guid parameterId, WireObjectLimit? maxObjects = null) => Downstream(anchor: GraphKey.Parameter(id: parameterId), maxObjects: maxObjects);
    public static FindCriterion Upstream(GraphKey anchor, WireObjectLimit? maxObjects = null) => new GraphCase(Anchor: anchor, Direction: WireTraversal.Upstream, MaxObjects: maxObjects ?? WireObjectLimit.Create(value: WireObjectLimit.DefaultCount));
    public static FindCriterion Downstream(GraphKey anchor, WireObjectLimit? maxObjects = null) => new GraphCase(Anchor: anchor, Direction: WireTraversal.Downstream, MaxObjects: maxObjects ?? WireObjectLimit.Create(value: WireObjectLimit.DefaultCount));
    public static FindCriterion ByName(string substring, bool caseInsensitive = true, ObjectScope? scope = null) => new ByNameCase(Substring: substring, CaseInsensitive: caseInsensitive, Scope: scope ?? ObjectScope.Primary);
    // Host ObjectList.FindBySearch is a verified empty-array stub; Search is a name-substring alias over ByNameCase
    // that defaults to PrimaryAndSecondary scope (the "search everything" the host stub never delivered).
    public static FindCriterion Search(string query, bool caseInsensitive = true) => new ByNameCase(Substring: query, CaseInsensitive: caseInsensitive, Scope: ObjectScope.PrimaryAndSecondary);
}

[SkipUnionOps]
[Union]
public partial record DocumentQuery {
    private DocumentQuery() { }
    public sealed record SnapshotCase : DocumentQuery;
    public sealed record ObjectsCase(ObjectScope Scope) : DocumentQuery;
    public sealed record ObjectCase(Guid Id) : DocumentQuery;
    public sealed record ParameterCase(Guid Id) : DocumentQuery;
    public sealed record GripCase(PointF Point, GripKind Kind) : DocumentQuery;
    public sealed record FindCase(FindCriterion Criterion) : DocumentQuery;
    public sealed record MetaNamesCase : DocumentQuery;
    public sealed record MetaValuesCase : DocumentQuery;
    public sealed record UniverseCase : DocumentQuery;

    public static readonly DocumentQuery Snapshot = new SnapshotCase();
    public static DocumentQuery Objects(ObjectScope scope) => new ObjectsCase(Scope: scope);
    public static DocumentQuery Object(Guid id) => new ObjectCase(Id: id);
    public static DocumentQuery Parameter(Guid id) => new ParameterCase(Id: id);
    public static DocumentQuery Grip(PointF point, GripKind? kind = null) => new GripCase(Point: point, Kind: kind ?? GripKind.InletOrOutlet);
    public static DocumentQuery Find(FindCriterion criterion) => new FindCase(Criterion: criterion);
    public static readonly DocumentQuery MetaNames = new MetaNamesCase();
    public static readonly DocumentQuery MetaValues = new MetaValuesCase();
    public static readonly DocumentQuery Universe = new UniverseCase();
}

[GenerateUnionOps]
[Union]
public partial record DocumentOp {
    private DocumentOp() { }
    public sealed partial record QueryCase(DocumentQuery Request) : DocumentOp;
    public sealed partial record MutateCase(Seq<DocumentMutation> Mutations, DocumentMutationPolicy Policy) : DocumentOp;
    public sealed partial record HistoryCase(DocumentHistory Request) : DocumentOp;
    public sealed partial record InspectCase(DocumentInspect Kind) : DocumentOp;
    public sealed partial record SolutionCase(SolutionControl Control, SolutionMode Mode) : DocumentOp;
    // Node-targeted rewind: the zero-arg DocumentHistory SmartEnum cannot carry a runtime ordinal, so a
    // parameterized case resolves the central-sequence node at Ordinal and routes to History.Undo/Redo(Node).
    public sealed partial record HistoryTargetCase(int Ordinal, bool Redo) : DocumentOp;

    public static DocumentOp Query(DocumentQuery query) => new QueryCase(Request: query);
    public static DocumentOp Mutate(params ReadOnlySpan<DocumentMutation> mutations) =>
        new MutateCase(Mutations: toSeq(mutations.ToArray()), Policy: DocumentMutationPolicy.Default);
    public static DocumentOp Mutate(DocumentMutationPolicy policy, params ReadOnlySpan<DocumentMutation> mutations) =>
        new MutateCase(Mutations: toSeq(mutations.ToArray()), Policy: policy);
    public static DocumentOp History(DocumentHistory history) => new HistoryCase(Request: history);
    public static DocumentOp Inspect(DocumentInspect kind) => new InspectCase(Kind: kind);
    public static readonly DocumentOp DependencyGraph = new InspectCase(Kind: DocumentInspect.DependencyGraph);
    public static DocumentOp Solution(SolutionControl control, SolutionMode mode = SolutionMode.Regular) => new SolutionCase(Control: control, Mode: mode);
    public static DocumentOp HistoryTarget(int ordinal, bool redo = false) => new HistoryTargetCase(Ordinal: ordinal, Redo: redo);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        queryCase: static q => q.Request switch {
            DocumentQuery.UniverseCase => GrasshopperUiPolicy.Read,
            _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        },
        mutateCase: static m => GrasshopperUiPolicy.Document(repaint: m.Policy.RepaintOrDefault),
        historyCase: static h => h.Request.Key switch {
            var key when key == DocumentHistory.ShowHistory.Key => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.None),
            _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        },
        inspectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        solutionCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        historyTargetCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
}

// Item-owned native invocation closes the if/throw dispatch hole — every item carries the
// methods-mutation closure so adding a new inspect kind is one factory line, not a switch arm.
[SmartEnum<int>]
public sealed partial class DocumentInspect {
    private delegate Unit InspectInvoke(GhDocumentMethods methods);

    public static readonly DocumentInspect DependencyGraph = new(key: 0, invoke: static methods => { methods.ShowDependencyGraph(); return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Invoke(GhDocumentMethods methods);
}

// Solution-server lifecycle verbs. Start/Stop are bridge-safe; the idle-loop-driven solver means a
// blocking StartWait deadlocks on the bridge UI thread (reference_gh2_headless_solution_limits), so the
// wait-to-settle variant stays a LIVE-host-only follow-up rather than a bridge-driven verb here.
[SmartEnum<int>]
public sealed partial class SolutionControl {
    private delegate Fin<Unit> RunFn(SolutionServer server, SolutionMode mode);

    public static readonly SolutionControl Start = new(key: 0, run: static (server, mode) =>
        Op.Of(name: nameof(Start)).Attempt(body: () => { _ = server.Start(mode: mode); return unit; }, what: "SolutionServer.Start"));
    public static readonly SolutionControl Stop = new(key: 1, run: static (server, _) =>
        Op.Of(name: nameof(Stop)).Attempt(body: () => { server.Stop(); return unit; }, what: "SolutionServer.Stop"));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Run(SolutionServer server, SolutionMode mode);
}

[SmartEnum<int>]
public sealed partial class DocumentHistory {
    private delegate Fin<DocumentResult> RunFn(GrasshopperUi.Scope scope);

    public static readonly DocumentHistory Query = new(key: 0, run: static scope =>
        scope.NeedDocument().Map(document => (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document))));
    public static readonly DocumentHistory Undo = new(key: 1, run: static scope =>
        Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Undo)), run: static document => document.Undo.Undo()));
    public static readonly DocumentHistory Redo = new(key: 2, run: static scope =>
        Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Redo)), run: static document => document.Undo.Redo()));
    public static readonly DocumentHistory Clear = new(key: 3, run: static scope =>
        Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Clear)), run: static document => document.Undo.Clear()));
    public static readonly DocumentHistory ShowHistory = new(key: 4, run: static scope => Document.ShowHistory(scope: scope));

    [UseDelegateFromConstructor]
    internal partial Fin<DocumentResult> Run(GrasshopperUi.Scope scope);
}

[SkipUnionOps]
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
    public sealed record MetaValuesResult(Map<MetaName, Seq<string>> Values) : DocumentResult;
    public sealed record UniverseResult(DocumentUniverseSnapshot Snapshot) : DocumentResult;
    public sealed record MutationResult(Snapshot<DocumentMutationDelta> Delta) : DocumentResult;
    public sealed record HistoryResult(DocumentHistorySnapshot Snapshot) : DocumentResult;
    public sealed record InspectResult(DocumentInspect Kind) : DocumentResult;
    public sealed record SolutionResult(DocumentSnapshot Snapshot) : DocumentResult;
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
public readonly record struct DocumentSnapshot(Guid Hash, bool Modified, int Modifications, int ObjectCount, int PinCount, int ExpiredCount, int SelectedObjectCount, int SelectedWireCount, int SelectedDanglingWireCount, int WireCount, int DanglingWireCount, RectangleF AttributeBounds, RectangleF PivotBounds, PointF ProjectionCentre, float ProjectionZoom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentUniverseSnapshot(Seq<DocumentSnapshot> Documents, int Count);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentHistoryNode(string Name, int Count);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentHistorySnapshot(bool IsEmpty, bool CanUndo, bool CanRedo, Seq<DocumentHistoryNode> UndoNodes = default, Seq<DocumentHistoryNode> RedoNodes = default) {
    public int UndoCount => UndoNodes.Count;
    public int RedoCount => RedoNodes.Count;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentObjectSnapshot(Guid Id, string Name, string DisplayName, bool Selected, string Activity, string Display, string Phase, string State, RectangleF Bounds, PointF Pivot);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ParameterSnapshot(Guid Id, string Name, string Kind, string Access, string AccessVariability, string Requirement, string TypeFlavour, int InputCount, int OutputCount, bool HasColourOverride);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentGripSnapshot(Guid Parameter, bool InletWithinRange, bool OutletWithinRange);

[SkipUnionOps]
[Union]
public partial record ObjectSpec {
    private ObjectSpec() { }
    public sealed record SliderCase(string Name, UiNumber Number, Option<string> Format, GripShape Shape, Color Colour) : ObjectSpec;
    public sealed record ToggleCase(string Name, bool State) : ObjectSpec;
    // ScribbleObject (Grasshopper2.SpecialObjects) is a DocumentObject — placed through the same DropObject
    // path as Slider/Toggle. Panel/ValueList are deliberately excluded: their host types (DataPanelObject /
    // ValueListObject) derive from Parameter (dataflow insertion differs) and ValueListItem is internal, so
    // neither can be populated or placed cleanly through the IDocumentObject drop surface.
    public sealed record ScribbleCase(string Text, GhOpenColorFamily Colour, int Angle) : ObjectSpec;

    public static ObjectSpec Slider(string name, UiNumber number, string? format = null, GripShape shape = GripShape.Label, Color? colour = null) =>
        new SliderCase(Name: name, Number: number, Format: Optional(format), Shape: shape, Colour: colour ?? Colors.Crimson);
    public static ObjectSpec Toggle(string name, bool state) => new ToggleCase(Name: name, State: state);
    public static ObjectSpec Scribble(string text, GhOpenColorFamily colour = GhOpenColorFamily.Gray, int angle = 0) =>
        new ScribbleCase(Text: text, Colour: colour, Angle: angle);

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
                select (IDocumentObject)new ToggleObject(state: t.State) { UserName = name },
            scribbleCase: static (key, s) =>
                from text in key.AcceptText(value: s.Text)
                select (IDocumentObject)new ScribbleObject(text: text) { TextColour = s.Colour, TextAngle = s.Angle });
}

[SkipUnionOps]
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

[SkipUnionOps]
[Union]
public abstract partial record DocumentMutation {
    public sealed record TargetCase(ObjectScope Subject, DocumentTargetOp Op) : DocumentMutation;
    public sealed record ComposeCase(ObjectScope Subject, ComposeOp Op) : DocumentMutation;
    public sealed record DropCase(Guid ProxyId, PointF Location, Option<string> Init = default) : DocumentMutation;
    public sealed record DropSnippetCase(string File, PointF Location) : DocumentMutation;
    public sealed record PlaceCase(ObjectSpec Object, PointF Location, DropCue Cue) : DocumentMutation;
    public sealed record AddDependencyCase(PointF Location) : DocumentMutation;
    public sealed record IsolateCase(IsolateOptions Options) : DocumentMutation;
    public sealed record MergeCase : DocumentMutation {
        internal MergeCase(GhObjectList source, bool reinstateInputs, bool reinstateOutputs) =>
            (Source, ReinstateInputs, ReinstateOutputs) = (source, reinstateInputs, reinstateOutputs);

        internal GhObjectList Source { get; }
        internal bool ReinstateInputs { get; }
        internal bool ReinstateOutputs { get; }
    }

    public static DocumentMutation Target(ObjectScope subject, DocumentTargetOp op) => new TargetCase(Subject: subject, Op: op);
    public static DocumentMutation Selection(SelectionOp op) => Target(subject: ObjectScope.Selection, op: new DocumentTargetOp.SelectionCase(Op: op));
    public static DocumentMutation Display(DisplayPort port, VisibilityChange change) => Target(subject: ObjectScope.Selection, op: new DocumentTargetOp.DisplayCase(Port: port, Change: change));
    public static DocumentMutation Clipboard(ClipboardOp op) => Target(subject: ObjectScope.Selection, op: new DocumentTargetOp.ClipboardCase(Op: op));
    public static DocumentMutation Compose(ComposeOp op) => new ComposeCase(Subject: ObjectScope.Selection, Op: op);
    public static DocumentMutation Compose(ObjectScope subject, ComposeOp op) => new ComposeCase(Subject: subject, Op: op);
    public static DocumentMutation Drop(Guid proxyId, PointF location, string? init = null) => new DropCase(ProxyId: proxyId, Location: location, Init: Optional(init));
    public static DocumentMutation DropSnippet(string file, PointF location) => new DropSnippetCase(File: file, Location: location);
    public static DocumentMutation Place(ObjectSpec obj, PointF location, DropCue? cue = null) => new PlaceCase(Object: obj, Location: location, Cue: cue ?? DropCue.None);
    public static DocumentMutation AddDependency(PointF location) => new AddDependencyCase(Location: location);
    public static DocumentMutation Isolate(IsolateOptions options = default) => new IsolateCase(Options: options);
    // Clipboard/import other half: absorb a foreign object list into this document; ObjectList.Absorb repairs pins internally.
    internal static DocumentMutation Merge(GhObjectList source, bool reinstateInputs = true, bool reinstateOutputs = true) => new MergeCase(source: source, reinstateInputs: reinstateInputs, reinstateOutputs: reinstateOutputs);

    internal Fin<DocumentMutationReceipt> Apply(GhDocumentMethods methods, GhObjectList objects, ActionList actions) =>
        Switch(
            state: (methods, objects, actions),
            targetCase: static (s, target) => target.Subject.Apply(methods: s.methods, objects: s.objects, op: target.Op, actions: s.actions).Map(static changed => DocumentMutationReceipt.Count(changed: changed)),
            composeCase: static (s, compose) => UiRail.ComposeDispatch(methods: s.methods, objects: s.objects, subject: compose.Subject, op: compose.Op, actions: s.actions)
                .Map(static created => created.Map(static id => DocumentMutationReceipt.CreatedObject(id: id)).IfNone(DocumentMutationReceipt.None)),
            dropCase: static (s, drop) => {
                Op op = Op.Of(name: nameof(Drop));
                return from proxy in op.AcceptValue(value: drop.ProxyId)
                        .MapFail(static _ => UiFault.InvalidInput(op: Op.Of(name: nameof(Drop)), detail: "empty proxy id"))
                       from location in op.AcceptPoint(value: drop.Location, detail: "non-finite location")
                       let changed = drop.Init is { IsSome: true, Case: string init }
                           ? s.methods.DropObject(obj: (proxy, init), location: location, actions: s.actions)
                           : s.methods.DropObject(obj: proxy, location: location, actions: s.actions)
                       from receipt in changed
                           ? Fin.Succ(value: DocumentMutationReceipt.Count(changed: 1))
                           : Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: "DropObject rejected the proxy (unknown or incompatible)"))
                       select receipt;
            },
            dropSnippetCase: static (s, snippet) => {
                Op op = Op.Of(name: nameof(DropSnippet));
                return from file in op.AcceptText(value: snippet.File)
                       from location in op.AcceptPoint(value: snippet.Location, detail: "non-finite location")
                       from changed in Try.lift(f: () => s.methods.DropSnippet(snippet: new GhSnippet(file: file), location: location, actions: s.actions)).Run()
                           .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(DropSnippet)), detail: $"DropSnippet threw: {error.Message}"))
                       select DocumentMutationReceipt.From(changed: changed);
            },
            placeCase: static (s, place) => {
                Op op = Op.Of(name: nameof(Place));
                return from spec in Optional(place.Object).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "object spec is required"))
                       from location in op.AcceptPoint(value: place.Location, detail: "non-finite location")
                       from obj in spec.Build(op: op)
                       from resolved in place.Cue.Resolve(objects: s.objects, op: op)
                       let native = (resolved.Source.IsSome || resolved.Target.IsSome) switch {
                           true => s.methods.DropObject(
                               obj: obj,
                               location: location,
                               sourceCue: resolved.Source.OrNull(),
                               targetCue: resolved.Target.OrNull(),
                               actions: s.actions),
                           false => s.methods.DropObject(obj: obj, location: location, actions: s.actions),
                       }
                       from receipt in native
                           ? Fin.Succ(value: DocumentMutationReceipt.CreatedObject(id: obj.InstanceId))
                           : Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: "DropObject rejected the placed object (init parse or cue mismatch)"))
                       select receipt;
            },
            addDependencyCase: static (s, dependency) => Op.Of(name: nameof(AddDependency))
                .AcceptPoint(value: dependency.Location, detail: "non-finite location")
                .Map(valid => Optional(s.methods.AddDependency(location: valid, actions: s.actions))
                    .Map(static listen => listen.InstanceId)
                    .Bind(static id => id.NonEmpty())
                    .Map(static id => DocumentMutationReceipt.CreatedObject(id: id))
                    .IfNone(DocumentMutationReceipt.None)),
            isolateCase: static (s, isolate) => (
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
                select DocumentMutationReceipt.From(changed: s.actions.Count > before)),
            mergeCase: static (s, merge) => Op.Of(name: nameof(Merge)).Attempt(
                body: () => {
                    // ObjectList.Absorb already runs RepairPins internally (verified by decompile), so no second pass.
                    int changed = s.objects.Absorb(other: merge.Source, reinstateInputs: merge.ReinstateInputs, reinstateOutputs: merge.ReinstateOutputs, actions: s.actions);
                    return DocumentMutationReceipt.Count(changed: changed);
                },
                what: "ObjectList.Absorb"));
}

[SmartEnum<int>]
public sealed partial class DocumentTargetVerb {
    private delegate int RunSelectedFn(GhDocumentMethods methods, ActionList actions);
    private delegate int RunObjectsFn(GhDocumentMethods methods, IDocumentObject[] targets, ActionList actions);

    public static readonly DocumentTargetVerb Show = new(
        key: 0,
        runSelected: static (m, a) => m.ShowSelected(actions: a),
        runObjects: static (m, t, a) => m.ShowObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb Hide = new(
        key: 1,
        runSelected: static (m, a) => m.HideSelected(actions: a),
        runObjects: static (m, t, a) => m.HideObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb ToggleVisibility = new(
        key: 2,
        runSelected: static (m, a) => m.ToggleDisplaySelected(actions: a),
        runObjects: static (m, t, a) => m.ToggleDisplayObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb Enable = new(
        key: 3,
        runSelected: static (m, a) => m.EnableSelected(actions: a),
        runObjects: static (m, t, a) => m.EnableObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb Disable = new(
        key: 4,
        runSelected: static (m, a) => m.DisableSelected(actions: a),
        runObjects: static (m, t, a) => m.DisableObjects(objects: t, actions: a));

    [UseDelegateFromConstructor]
    private partial int RunSelected(GhDocumentMethods methods, ActionList actions);

    [UseDelegateFromConstructor]
    private partial int RunObjects(GhDocumentMethods methods, IDocumentObject[] targets, ActionList actions);

    internal int Run(GhDocumentMethods methods, IDocumentObject[]? targets, ActionList actions) =>
        targets is null ? RunSelected(methods: methods, actions: actions) : RunObjects(methods: methods, targets: targets, actions: actions);
}

[SkipUnionOps]
[Union]
public abstract partial record DocumentTargetOp {
    public sealed record VerbCase(DocumentTargetVerb Verb) : DocumentTargetOp;
    public sealed record SelectionCase(SelectionOp Op) : DocumentTargetOp;
    public sealed record DisplayCase(DisplayPort Port, VisibilityChange Change) : DocumentTargetOp;
    public sealed record ClipboardCase(ClipboardOp Op) : DocumentTargetOp;
    public sealed record DeleteCase(bool DataOnly, Seq<WireEnds> Wires) : DocumentTargetOp;
    public sealed record ColourCase(Option<GhColour> Override) : DocumentTargetOp;

    public static readonly DocumentTargetOp Show = new VerbCase(Verb: DocumentTargetVerb.Show);
    public static readonly DocumentTargetOp Hide = new VerbCase(Verb: DocumentTargetVerb.Hide);
    public static readonly DocumentTargetOp ToggleVisibility = new VerbCase(Verb: DocumentTargetVerb.ToggleVisibility);
    public static readonly DocumentTargetOp Enable = new VerbCase(Verb: DocumentTargetVerb.Enable);
    public static readonly DocumentTargetOp Disable = new VerbCase(Verb: DocumentTargetVerb.Disable);
    public static DocumentTargetOp Delete(bool dataOnly = false, Seq<WireEnds> wires = default) => new DeleteCase(DataOnly: dataOnly, Wires: wires);
    public static DocumentTargetOp Style(GhColour colour) => new ColourCase(Override: Some(colour));
    public static readonly DocumentTargetOp ClearStyle = new ColourCase(Override: Option<GhColour>.None);

    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, Seq<Guid> ids, ActionList actions) =>
        ids.TraverseM(id => Optional(objects.Find(instanceId: id))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectScope)), detail: $"object {id} not found")))
        .Map(static resolved => resolved.ToArray())
        .Bind(targets => Dispatch(methods: methods, actions: actions, targets: targets)).As();

    internal Fin<int> ApplySelected(GhDocumentMethods methods, ActionList actions) =>
        Dispatch(methods: methods, actions: actions, targets: null);

    // Non-null `targets` → *Objects(targets, actions); null → *Selected(actions). Null sentinel
    // over Option<T> because CSP0705 forbids Option.Match inside the Switch arms below.
    private Fin<int> Dispatch(GhDocumentMethods methods, ActionList actions, IDocumentObject[]? targets) =>
        Switch(
            state: (methods, targets, actions),
            verbCase: static (s, v) => Fin.Succ(value: v.Verb.Run(methods: s.methods, targets: s.targets, actions: s.actions)),
            selectionCase: static (s, selection) => UiRail.SelectionDispatch(methods: s.methods, op: selection.Op),
            displayCase: static (s, display) => Fin.Succ(value: display.Port.Run(methods: s.methods, show: display.Change.IsShow, actions: s.actions)),
            clipboardCase: static (s, clipboard) => UiRail.ClipboardDispatch(methods: s.methods, op: clipboard.Op, actions: s.actions),
            deleteCase: static (s, delete) => Fin.Succ(value: UiRail.RunDelete(
                methods: s.methods, targets: s.targets, dataOnly: delete.DataOnly, wires: delete.Wires, actions: s.actions)),
            colourCase: static (s, colour) => {
                GhColour? value = colour.Override.OrNull();
                return Fin.Succ(value: s.targets is null
                    ? s.methods.SetColourOverrideSelected(colour: value, actions: s.actions)
                    : s.methods.SetColourOverrideObjects(objects: s.targets, colour: value, actions: s.actions));
            });
}

[SmartEnum<int>]
public sealed partial class ClipboardVerb {
    private delegate Fin<int> RunFn(GhDocumentMethods methods, ClipboardKind kind, PasteBehaviour behaviour, ActionList actions);

    public static readonly ClipboardVerb Copy = new(
        key: 0,
        run: static (methods, kind, _, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Copy), clipboard: kind).Map(k => methods.CopySelection(clipboard: k) ? 1 : 0));
    public static readonly ClipboardVerb Cut = new(
        key: 1,
        run: static (methods, kind, _, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Cut), clipboard: kind).Map(k => methods.CutSelection(clipboard: k, actions: actions) ? 1 : 0));
    public static readonly ClipboardVerb Paste = new(
        key: 2,
        run: static (methods, kind, behaviour, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Paste), clipboard: kind).Map(k => methods.PasteFromClipboard(clipboard: k, behaviour: behaviour, actions: actions) ? 1 : 0));
    public static readonly ClipboardVerb PasteGh1Xml = new(
        key: 3,
        run: static (methods, _, _, actions) => Fin.Succ(value: methods.PasteGrasshopper1XmlFromClipboard(actions: actions) ? 1 : 0));

    [UseDelegateFromConstructor]
    internal partial Fin<int> Run(GhDocumentMethods methods, ClipboardKind kind, PasteBehaviour behaviour, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class DisplayPort {
    private delegate int RunFn(GhDocumentMethods methods, bool show, ActionList actions);

    public static readonly DisplayPort Inputs = new(
        key: 0,
        run: static (methods, show, actions) => show ? methods.ShowSelectedInputs(actions: actions) : methods.HideSelectedInputs(actions: actions));
    public static readonly DisplayPort Outputs = new(
        key: 1,
        run: static (methods, show, actions) => show ? methods.ShowSelectedOutputs(actions: actions) : methods.HideSelectedOutputs(actions: actions));

    [UseDelegateFromConstructor]
    internal partial int Run(GhDocumentMethods methods, bool show, ActionList actions);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Native-boundary projections: OrNull/OrNullable collapse the `.Map(cast).IfNone(null)` ladders that
// bridge Option<T> back to the host's nullable parameter surface; NonEmpty is the single Guid-sentinel
// → Option rule shared by every host-id read. Inv invariant-stringifies snapshot fields once.
internal static class OptionNativeExtensions {
    internal static T? OrNull<T>(this Option<T> option) where T : class =>
        option is { IsSome: true, Case: T value } ? value : null;
    internal static T? OrNullable<T>(this Option<T> option) where T : struct =>
        option is { IsSome: true, Case: T value } ? value : null;
    internal static Option<Guid> NonEmpty(this Guid id) => id == Guid.Empty ? None : Some(id);
    internal static Option<Guid> NonEmpty(this Guid? id) => id is Guid g && g != Guid.Empty ? Some(g) : None;
}

internal static class SnapshotFormatExtensions {
    internal static string Inv<T>(this T value) => string.Create(CultureInfo.InvariantCulture, $"{value}");
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Document {
    internal static Fin<DocumentResult> Dispatch(GrasshopperUi.Scope scope, DocumentOp op) =>
        op.Switch(
            state: scope,
            queryCase: static (s, q) => Query(scope: s, query: q.Request),
            mutateCase: static (s, m) => Mutate(scope: s, mutations: m.Mutations, policy: m.Policy),
            historyCase: static (s, h) => h.Request.Run(scope: s),
            inspectCase: static (s, i) => DispatchInspect(scope: s, kind: i.Kind),
            solutionCase: static (s, sol) =>
                from document in s.NeedDocument()
                from objects in s.NeedObjects()
                from _ in sol.Control.Run(server: document.Solution, mode: sol.Mode)
                select (DocumentResult)new DocumentResult.SolutionResult(Snapshot: UiRail.DocumentSnapshotOf(document: document, objects: objects)),
            historyTargetCase: static (s, t) =>
                from document in s.NeedDocument()
                from ordinal in t.Ordinal >= 0
                    ? Fin.Succ(value: t.Ordinal)
                    : Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.HistoryTarget)), detail: "history ordinal must be non-negative"))
                from node in toSeq(t.Redo ? document.Undo.CentralRedoSequence : document.Undo.CentralUndoSequence)
                    .Skip(ordinal).Head
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentOp.HistoryTarget)), detail: string.Create(CultureInfo.InvariantCulture, $"no history node at ordinal {t.Ordinal}")))
                from _ in Op.Of(name: nameof(DocumentOp.HistoryTarget)).Attempt(
                    body: () => {
                        _ = t.Redo ? Op.Side(() => document.Undo.Redo(node)) : Op.Side(() => document.Undo.Undo(node));
                        return unit;
                    },
                    what: "History node target")
                select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document)));

    private static Fin<DocumentResult> DispatchInspect(GrasshopperUi.Scope scope, DocumentInspect kind) =>
        from methods in scope.NeedMethods()
        from _ in DocumentOp.InspectCase.SelfOp.Attempt(body: () => kind.Invoke(methods), what: string.Create(CultureInfo.InvariantCulture, $"DocumentInspect[{kind.Key}]"))
        select (DocumentResult)new DocumentResult.InspectResult(Kind: kind);

    private static Fin<DocumentResult> Query(GrasshopperUi.Scope scope, DocumentQuery query) =>
        query.Switch(
            state: scope,
            snapshotCase: static (s, _) =>
                from doc in s.NeedDocument()
                from objs in s.NeedObjects()
                select (DocumentResult)new DocumentResult.SnapshotResult(Snapshot: UiRail.DocumentSnapshotOf(document: doc, objects: objs)),
            objectsCase: static (s, o) =>
                s.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectListResult(
                    Snapshots: toSeq(o.Scope.Resolve(objects: objs).Select(UiRail.DocumentObjectSnapshotOf)))),
            objectCase: static (s, obj) =>
                Op.Of(name: nameof(DocumentQuery.Object)).AcceptValue(value: obj.Id)
                    .MapFail(_ => UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentQuery.Object)), detail: "empty Guid"))
                    .Bind(valid => s.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ObjectResult(
                        Snapshot: Optional(objs.Find(instanceId: valid)).Map(UiRail.DocumentObjectSnapshotOf)))),
            parameterCase: static (s, param) =>
                Op.Of(name: nameof(DocumentQuery.Parameter)).AcceptValue(value: param.Id)
                    .MapFail(_ => UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentQuery.Parameter)), detail: "empty Guid"))
                    .Bind(valid => s.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.ParameterResult(
                        Snapshot: Optional(objs.FindParameter(instanceId: valid)).Map(ParameterSnapshotOf)))),
            gripCase: static (s, g) =>
                from point in Op.Of(name: nameof(DocumentQuery.Grip)).AcceptPoint(value: g.Point, detail: "non-finite point")
                from objs in s.NeedObjects()
                select (DocumentResult)new DocumentResult.GripResult(Grip: g.Kind.Resolve(objs: objs, point: point)),
            findCase: static (s, f) =>
                s.NeedObjects().Bind(objs => Find(objects: objs, criterion: f.Criterion)
                    .Map(matches => (DocumentResult)new DocumentResult.FindResult(Matches: matches))),
            metaNamesCase: static (s, _) =>
                s.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.MetaNamesResult(Names: toSeq(objs.MetaNames()))),
            metaValuesCase: static (s, _) =>
                s.NeedObjects().Map(objs => (DocumentResult)new DocumentResult.MetaValuesResult(
                    Values: toMap(objs.MetaNamesAndValues().Select(static kv => (kv.Key, toSeq(kv.Value)))))),
            universeCase: static (_, _) =>
                Universe().Map(snapshot => (DocumentResult)new DocumentResult.UniverseResult(Snapshot: snapshot)));

    private static Fin<DocumentResult> Mutate(GrasshopperUi.Scope scope, Seq<DocumentMutation> mutations, DocumentMutationPolicy policy) =>
        UiRail.RunMutation(scope: scope, op: DocumentOp.MutateCase.SelfOp,
            mutate: (methods, objects, actions) => mutations.TraverseM(m => m.Apply(methods: methods, objects: objects, actions: actions))
                .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, receipt) => sum + receipt)).As(),
            policy: policy)
            .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta));

    internal static Fin<DocumentResult> MutateHistory(GrasshopperUi.Scope scope, Op op, Action<GhDocument> run) =>
        from document in scope.NeedDocument()
        from _ in Try.lift(f: () => { run(document); return unit; }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: error.Message))
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document));

    internal static Fin<DocumentResult> ShowHistory(GrasshopperUi.Scope scope) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        from _ in Op.Of(name: nameof(DocumentHistory.ShowHistory)).Attempt(
            body: () => { _ = History.ShowHistory(canvas: canvas); return unit; },
            what: "History.ShowHistory")
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document));

    private static Fin<Seq<DocumentObjectSnapshot>> Find(GhObjectList objects, FindCriterion criterion) =>
        criterion.Switch(
            state: objects,
            nearPointCase: static (objs, n) =>
                from point in Op.Of(name: nameof(Find)).AcceptPoint(value: n.Point, detail: "non-finite point")
                from maxResults in Optional(n.MaxResults).Filter(static count => count > 0)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Find)), detail: "maxResults must be positive"))
                from maxDistance in Op.Of(name: nameof(Find)).AcceptFinite(value: n.MaxDistance, detail: "maxDistance must be finite and non-negative", nonNegative: true)
                select toSeq(objs.FindNear<IDocumentObject>(locus: point, maxResults: maxResults, maxDistance: maxDistance)).Map(UiRail.DocumentObjectSnapshotOf),
            byDrawOrderCase: static (objs, d) =>
                Fin.Succ(value: toSeq(objs.ObjectsByDrawOrder(includeForeground: d.Foreground, includeBackground: d.Background)).Map(UiRail.DocumentObjectSnapshotOf)),
            graphCase: static (objs, graph) => Wire.GraphObjects(objects: objs, anchor: graph.Anchor, direction: graph.Direction, maxObjects: graph.MaxObjects)
                .Map(matches => matches.Map(UiRail.DocumentObjectSnapshotOf)),
            byNameCase: static (objs, n) =>
                Op.Of(name: nameof(FindCriterion.ByName)).AcceptText(value: n.Substring)
                    .Map(text => toSeq(n.Scope.Resolve(objects: objs))
                        .Filter(o => o.Nomen.Name.Contains(value: text, comparisonType: n.CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        .Map(UiRail.DocumentObjectSnapshotOf)));

    private static Fin<DocumentUniverseSnapshot> Universe() =>
        Optional(GhEditor.Instance?.Documents)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(GhEditor.Documents)))
            .Map(documents => toSeq(documents.All.SelectMany(stack => stack.Documents))
                .Map(document => UiRail.DocumentSnapshotOf(document: document, objects: document.Objects)))
            .Map(static snapshots => new DocumentUniverseSnapshot(Documents: snapshots, Count: snapshots.Count));

    private static ParameterSnapshot ParameterSnapshotOf(IParameter parameter) =>
        new(
            Id: parameter.InstanceId,
            Name: parameter.Nomen.Name,
            Kind: parameter.Kind.Inv(),
            Access: parameter.Access.Inv(),
            AccessVariability: parameter.AccessVariability.Inv(),
            Requirement: parameter.Requirement.Inv(),
            TypeFlavour: parameter.TypeFlavour.Inv(),
            InputCount: parameter.Inputs.Count,
            OutputCount: parameter.Outputs.Count,
            HasColourOverride: parameter.ColourOverride is not null);
}

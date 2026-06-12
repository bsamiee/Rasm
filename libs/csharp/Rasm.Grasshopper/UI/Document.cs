using System.Globalization;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Special;
using Grasshopper2.SpecialObjects;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Slider;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
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

    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, DocumentTargetOp op, ActionList actions) =>
        Switch(
            state: (methods, objects, op, actions),
            selectionCase: static (s, _) => s.op.ApplySelected(methods: s.methods, objects: s.objects, actions: s.actions),
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

    internal Fin<T> RejectPrimary<T>(Op op) =>
        Fin.Fail<T>(error: UiFault.InvalidInput(op: op, detail: RejectsPrimaryDetail()));
}

[SmartEnum<int>]
internal sealed partial class DocumentDeleteMode {
    public static readonly DocumentDeleteMode SelectionDataOnly = new(key: 0, run: static (m, _, _, a) => m.DeleteSelectionData(actions: a));
    public static readonly DocumentDeleteMode SelectionFull = new(key: 1, run: static (m, _, _, a) => m.DeleteSelection(actions: a));
    public static readonly DocumentDeleteMode ObjectsDataOnly = new(key: 2, run: static (m, t, _, a) => m.DeleteObjectData(objects: t!, actions: a));
    public static readonly DocumentDeleteMode ObjectsFull = new(key: 3, run: static (m, t, w, a) => m.DeleteObjects(objects: t!, wires: [.. w], actions: a));

    [UseDelegateFromConstructor]
    internal partial int Run(GhDocumentMethods methods, IDocumentObject[]? targets, Seq<WireEnds> wires, ActionList actions);

    // Key bit-packing: bit1 = targets present (0 selection, 2 objects), bit0 = full (0 data-only, 1 full).
    internal static DocumentDeleteMode Resolve(IDocumentObject[]? targets, bool dataOnly) =>
        Get((targets is null ? 0 : 2) | (dataOnly ? 0 : 1));
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
    // One graph-walk discriminant: MaxHops=None is the unbounded transitive closure, Some(n) bounds the walk to n hops;
    // Replace re-seeds the selection onto the reached frontier (Shift) instead of growing it (Grow/GrowN).
    public sealed record GraphCase(Option<int> MaxHops, bool Upstream, bool Downstream, bool Replace) : SelectionOp;

    public static readonly SelectionOp All = new AllCase();
    public static readonly SelectionOp None = new NoneCase();
    public static readonly SelectionOp Invert = new InvertCase();
    public static SelectionOp Grow(bool upstream = true, bool downstream = true) => new GraphCase(MaxHops: Option<int>.None, Upstream: upstream, Downstream: downstream, Replace: false);
    public static SelectionOp GrowN(int hops, bool upstream = true, bool downstream = true) => new GraphCase(MaxHops: Some(hops), Upstream: upstream, Downstream: downstream, Replace: false);
    public static SelectionOp Shift(bool upstream) => new GraphCase(MaxHops: Some(1), Upstream: upstream, Downstream: !upstream, Replace: true);
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

[SmartEnum<int>]
public sealed partial class ClipboardVerb {
    private delegate Fin<int> RunFn(GhDocumentMethods methods, ClipboardKind kind, PasteBehaviour behaviour, ActionList actions);

    public static readonly ClipboardVerb Copy = new(
        key: 0,
        run: static (methods, kind, _, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Copy), clipboard: kind)
                .Bind(k => Op.Of(name: nameof(ClipboardOp.Copy)).Attempt(body: () => methods.CopySelection(clipboard: k) ? 1 : 0, what: "DocumentMethods.CopySelection")));
    public static readonly ClipboardVerb Cut = new(
        key: 1,
        run: static (methods, kind, _, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Cut), clipboard: kind)
                .Bind(k => Op.Of(name: nameof(ClipboardOp.Cut)).Attempt(body: () => methods.CutSelection(clipboard: k, actions: actions) ? 1 : 0, what: "DocumentMethods.CutSelection")));
    public static readonly ClipboardVerb Paste = new(
        key: 2,
        run: static (methods, kind, behaviour, actions) =>
            UiRail.ValidateClipboard(name: nameof(ClipboardOp.Paste), clipboard: kind)
                .Bind(k => Op.Of(name: nameof(ClipboardOp.Paste)).Attempt(body: () => methods.PasteFromClipboard(clipboard: k, behaviour: behaviour, actions: actions) ? 1 : 0, what: "DocumentMethods.PasteFromClipboard")));
    public static readonly ClipboardVerb PasteGh1Xml = new(
        key: 3,
        run: static (methods, _, _, actions) =>
            Op.Of(name: nameof(ClipboardOp.PasteGh1Xml)).Attempt(body: () => methods.PasteGrasshopper1XmlFromClipboard(actions: actions) ? 1 : 0, what: "DocumentMethods.PasteGrasshopper1XmlFromClipboard"));

    [UseDelegateFromConstructor]
    internal partial Fin<int> Run(GhDocumentMethods methods, ClipboardKind kind, PasteBehaviour behaviour, ActionList actions);
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

    internal Fin<Option<Guid>> Apply(GhDocumentMethods methods, GhObjectList objects, IDocumentObject[]? targets, ActionList actions) =>
        Switch(state: (methods, objects, targets, actions),
            groupCase: static (s, g) => {
                string name = g.Name.IfNone(string.Empty);
                GhOpenColorFamily? colour = g.Colour.OrNullable();
                return Op.Of(name: nameof(Group)).Attempt(
                    body: () => Optional(s.targets is null
                        ? s.methods.GroupSelection(name: name, colour: colour, actions: s.actions)
                        : s.methods.GroupObjects(objects: s.targets, name: name, colour: colour, actions: s.actions)),
                    what: "DocumentMethods.Group")
                    .Map(static group => group.Map(static r => r.InstanceId).Bind(static id => id.NonEmpty()));
            },
            linkCase: static (s, link) => Link(state: s, kind: link.Kind));

    private static Fin<Option<Guid>> Link(
        (GhDocumentMethods methods, GhObjectList objects, IDocumentObject[]? targets, ActionList actions) state,
        DocumentLinkKind kind) =>
        UiRail.PreflightCompose(
            op: kind.Op,
            check: () => kind.CanCreate(methods: state.methods, targets: state.targets ?? state.objects.SelectedObjects, whyNot: out string whyNot) ? (true, whyNot) : (false, whyNot))
            .Bind(_ => kind.Op.Attempt(
                body: () => Optional(kind.Create(methods: state.methods, targets: state.targets, actions: state.actions))
                    .Map(static c => c.InstanceId)
                    .Bind(static id => id.NonEmpty()),
                what: "DocumentMethods.Compose"));
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
    public sealed record ByActivityCase(bool Enabled) : FindCriterion;
    public sealed record ByGroupCase(Guid GroupId) : FindCriterion;

    // 32f mirrors Layout.PickRadius.Default; a compile-time default cannot reference the static-readonly value object.
    public static FindCriterion Near(PointF point, int maxResults = 16, float maxDistance = 32f) => new NearPointCase(Point: point, MaxResults: maxResults, MaxDistance: maxDistance);
    public static FindCriterion DrawOrder(bool foreground = true, bool background = true) => new ByDrawOrderCase(Foreground: foreground, Background: background);
    public static FindCriterion Upstream(Guid parameterId, WireObjectLimit? maxObjects = null) => Upstream(anchor: GraphKey.Parameter(id: parameterId), maxObjects: maxObjects);
    public static FindCriterion Upstream(GraphKey anchor, WireObjectLimit? maxObjects = null) => new GraphCase(Anchor: anchor, Direction: WireTraversal.Upstream, MaxObjects: maxObjects ?? WireObjectLimit.Default);
    public static FindCriterion Downstream(Guid parameterId, WireObjectLimit? maxObjects = null) => Downstream(anchor: GraphKey.Parameter(id: parameterId), maxObjects: maxObjects);
    public static FindCriterion Downstream(GraphKey anchor, WireObjectLimit? maxObjects = null) => new GraphCase(Anchor: anchor, Direction: WireTraversal.Downstream, MaxObjects: maxObjects ?? WireObjectLimit.Default);
    public static FindCriterion ByName(string substring, bool caseInsensitive = true, ObjectScope? scope = null) => new ByNameCase(Substring: substring, CaseInsensitive: caseInsensitive, Scope: scope ?? ObjectScope.Primary);
    // Host FindBySearch is an empty-array stub, so Search aliases name matching over all primary/secondary objects.
    public static FindCriterion Search(string query, bool caseInsensitive = true) => new ByNameCase(Substring: query, CaseInsensitive: caseInsensitive, Scope: ObjectScope.PrimaryAndSecondary);
    public static FindCriterion ByActivity(bool enabled) => new ByActivityCase(Enabled: enabled);
    public static FindCriterion ByGroup(Guid groupId) => new ByGroupCase(GroupId: groupId);
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
    public sealed record DisplayStateCase : DocumentQuery;
    public sealed record NotesCase : DocumentQuery;
    public sealed record CustomValueCase(string Key, string DefaultValue) : DocumentQuery;

    public static readonly DocumentQuery Snapshot = new SnapshotCase();
    public static DocumentQuery Objects(ObjectScope scope) => new ObjectsCase(Scope: scope);
    public static DocumentQuery Object(Guid id) => new ObjectCase(Id: id);
    public static DocumentQuery Parameter(Guid id) => new ParameterCase(Id: id);
    public static DocumentQuery Grip(PointF point, GripKind? kind = null) => new GripCase(Point: point, Kind: kind ?? GripKind.InletOrOutlet);
    public static DocumentQuery Find(FindCriterion criterion) => new FindCase(Criterion: criterion);
    public static readonly DocumentQuery MetaNames = new MetaNamesCase();
    public static readonly DocumentQuery MetaValues = new MetaValuesCase();
    public static readonly DocumentQuery Universe = new UniverseCase();
    public static readonly DocumentQuery DisplayState = new DisplayStateCase();
    public static readonly DocumentQuery Notes = new NotesCase();
    public static DocumentQuery CustomValue(string key, string defaultValue = "") => new CustomValueCase(Key: key, DefaultValue: defaultValue);

    // Universe spans every open document, so it opens the editor and runs Canvas-scoped; all others are Document-scoped reads.
    internal GrasshopperUiPolicy QueryPolicy => this is UniverseCase
        ? GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.None)
        : GrasshopperUiPolicy.Document(repaint: RepaintRequest.None);
}

[SkipUnionOps]
[Union]
public partial record GroupOp {
    private GroupOp() { }
    public sealed record CreateCase(ObjectScope Scope, Option<string> Label, Option<GhOpenColorFamily> Colour) : GroupOp;
    public sealed record FindCase(Guid Member) : GroupOp;
    public sealed record ModifyCase(Guid Group, Option<string> Label, Option<GhOpenColorFamily> Colour, Seq<Guid> Add, Seq<Guid> Remove) : GroupOp;
    public sealed record DissolveCase(Guid Group) : GroupOp;
    public sealed record StatusCase(Guid Group) : GroupOp;
    public sealed record FindAllCase : GroupOp;

    public static GroupOp Create(ObjectScope? scope = null, string? label = null, GhOpenColorFamily? colour = null) =>
        new CreateCase(Scope: scope ?? ObjectScope.Selection, Label: Optional(label), Colour: Optional(colour));
    public static GroupOp Find(Guid member) => new FindCase(Member: member);
    public static GroupOp Modify(Guid group, string? label = null, GhOpenColorFamily? colour = null, Seq<Guid> add = default, Seq<Guid> remove = default) =>
        new ModifyCase(Group: group, Label: Optional(label), Colour: Optional(colour), Add: add, Remove: remove);
    public static GroupOp Dissolve(Guid group) => new DissolveCase(Group: group);
    public static GroupOp Status(Guid group) => new StatusCase(Group: group);
    public static readonly GroupOp FindAll = new FindAllCase();

    internal GrasshopperUiPolicy Policy => Switch(
        createCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        findCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        modifyCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        dissolveCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        statusCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        findAllCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None));

    internal Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) =>
        Switch(
            state: scope,
            createCase: static (s, create) =>
                from objects in s.NeedObjects()
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Create)),
                    mutate: (m, objs, actions) => Op.Of(name: nameof(Create)).Attempt(
                        body: () => {
                            // BOUNDARY ADAPTER -- GroupObjects/GroupSelection insert one GroupObject; null targets route selection.
                            string label = create.Label.IfNone(string.Empty);
                            IDocumentObject[]? targets = (create.Scope is ObjectScope.SelectionCase) ? null : [.. create.Scope.Resolve(objects: objs)];
                            GroupObject grp = targets is null
                                ? m.GroupSelection(name: label, colour: create.Colour.OrNullable(), actions: actions)
                                : m.GroupObjects(objects: targets, name: label, colour: create.Colour.OrNullable(), actions: actions);
                            return DocumentMutationReceipt.CreatedObject(id: grp.InstanceId);
                        },
                        what: "DocumentMethods.Group"))
                let grp = snapshot.Payload.Created.Head.Bind(id => Optional(objects.FindGroup(instanceId: id)))
                select (DocumentResult)new DocumentResult.GroupResult(Snapshot: GroupSnapshotOf(target: grp, changed: snapshot.Payload.Changed)),
            findCase: static (s, find) =>
                from objects in s.NeedObjects()
                from member in Op.Of(name: nameof(Find)).AcceptValue(value: find.Member)
                    .MapFail(static _ => UiFault.InvalidInput(op: Op.Of(name: nameof(Find)), detail: "empty member id"))
                select (DocumentResult)new DocumentResult.GroupListResult(
                    Snapshots: toSeq(objects.ContainingGroups(objectId: member)).Map(grp => GroupSnapshotOf(target: Some(grp), changed: 0))),
            modifyCase: static (s, modify) =>
                from objects in s.NeedObjects()
                from grp in ResolveGroup(objects: objects, id: modify.Group, op: Op.Of(name: nameof(Modify)))
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Modify)),
                    mutate: (_, objs, actions) => Op.Of(name: nameof(Modify)).Attempt(
                        body: () => ModifyGroup(objects: objs, target: grp, label: modify.Label, colour: modify.Colour, add: modify.Add, remove: modify.Remove, actions: actions),
                        what: "GroupObject.Modify"))
                select (DocumentResult)new DocumentResult.GroupResult(Snapshot: GroupSnapshotOf(target: Some(grp), changed: snapshot.Payload.Changed)),
            dissolveCase: static (s, dissolve) =>
                from objects in s.NeedObjects()
                from grp in ResolveGroup(objects: objects, id: dissolve.Group, op: Op.Of(name: nameof(Dissolve)))
                let members = toSeq(grp.ContentIds)
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Dissolve)),
                    mutate: (m, _, actions) => Op.Of(name: nameof(Dissolve)).Attempt(
                        body: () => DocumentMutationReceipt.Count(changed: m.DeleteObjects(objects: [grp], wires: [], actions: actions)),
                        what: "DocumentMethods.DeleteObjects"))
                select (DocumentResult)new DocumentResult.GroupResult(
                    Snapshot: new DocumentGroupSnapshot(Group: None, Label: None, Colour: None, Members: members, Changed: snapshot.Payload.Changed, Exists: false)),
            statusCase: static (s, status) =>
                from objects in s.NeedObjects()
                let grp = Optional(objects.FindGroup(instanceId: status.Group))
                select (DocumentResult)new DocumentResult.GroupResult(Snapshot: GroupSnapshotOf(target: grp, changed: 0)),
            findAllCase: static (s, _) =>
                from objects in s.NeedObjects()
                select (DocumentResult)new DocumentResult.GroupListResult(Snapshots: toSeq(objects.ForwardsOfT<GroupObject>()).Map(grp => GroupSnapshotOf(target: Some(grp), changed: 0))));

    private static Fin<GroupObject> ResolveGroup(GhObjectList objects, Guid id, Op op) =>
        op.AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: "group id is empty"))
            .Bind(valid => Optional(objects.FindGroup(instanceId: valid))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"group {valid} not found")));

    private static DocumentMutationReceipt ModifyGroup(GhObjectList objects, GroupObject target, Option<string> label, Option<GhOpenColorFamily> colour, Seq<Guid> add, Seq<Guid> remove, ActionList actions) {
        // BOUNDARY ADAPTER -- label/colour have undo actions; membership uses Add/RemoveContent without a host action.
        _ = label.Iter(text => { actions.Add(new RenameAction(target)); target.UserName = text; });
        _ = colour.Iter(family => { actions.Add(new PropertyAction(target, nameof(GroupObject.GroupColour))); target.GroupColour = family; });
        int added = add.Filter(static id => id != Guid.Empty).Fold(0, (count, id) => count + (target.AddContent(id) ? 1 : 0));
        int removed = remove.Filter(static id => id != Guid.Empty).Fold(0, (count, id) => count + (target.RemoveContent(id) ? 1 : 0));
        target.Expire();
        return DocumentMutationReceipt.Count(changed: 1 + added + removed);
    }

    private static DocumentGroupSnapshot GroupSnapshotOf(Option<GroupObject> target, int changed) =>
        target is { IsSome: true, Case: GroupObject g }
            ? new DocumentGroupSnapshot(
                Group: g.InstanceId.NonEmpty(),
                Label: Optional(g.UserName).Filter(static n => !string.IsNullOrEmpty(value: n)),
                Colour: Some(g.GroupColour),
                Members: toSeq(g.ContentIds),
                Changed: changed,
                Exists: true)
            : new DocumentGroupSnapshot(Group: None, Label: None, Colour: None, Members: Seq<Guid>(), Changed: changed, Exists: false);
}

[SkipUnionOps]
[Union]
public partial record WirelessOp {
    private WirelessOp() { }
    public sealed record SplitCase(Guid Source, Guid Target, string Name, PointF Location) : WirelessOp;
    public sealed record FindCase(Guid Shout) : WirelessOp;
    public sealed record ConnectCase(Guid Shout, Seq<Guid> Listens) : WirelessOp;
    public sealed record EnumerateCase : WirelessOp;
    public sealed record RenameCase(Guid Shout, string Label) : WirelessOp;

    public static WirelessOp Split(Guid source, Guid target, string name, PointF location) => new SplitCase(Source: source, Target: target, Name: name, Location: location);
    public static WirelessOp Find(Guid shout) => new FindCase(Shout: shout);
    public static WirelessOp Connect(Guid shout, Seq<Guid> listens) => new ConnectCase(Shout: shout, Listens: listens);
    public static readonly WirelessOp Enumerate = new EnumerateCase();
    public static WirelessOp Rename(Guid shout, string label) => new RenameCase(Shout: shout, Label: label);

    internal GrasshopperUiPolicy Policy => Switch(
        splitCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        findCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        connectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        enumerateCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        renameCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));

    internal Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) =>
        Switch(
            state: scope,
            splitCase: static (s, split) =>
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Split)),
                    mutate: (m, objs, actions) => {
                        Op op = Op.Of(name: nameof(Split));
                        return from location in op.AcceptPoint(value: split.Location, detail: "non-finite location")
                               from source in Optional(objs.FindParameter(instanceId: split.Source)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"source parameter {split.Source} not found"))
                               from target in Optional(objs.FindParameter(instanceId: split.Target)).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"target parameter {split.Target} not found"))
                               from receipt in op.Attempt(
                                   body: () => {
                                       // BOUNDARY ADAPTER -- SplitWire reports Shout/Listen through out-params; empty name is a valid unlabelled split.
                                       bool changed = m.SplitWire(source: source, target: target, name: split.Name ?? string.Empty, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: actions);
                                       return DocumentMutationReceipt.Of(changed: changed ? 1 : 0, created: Seq(shout?.InstanceId ?? Guid.Empty, listen?.InstanceId ?? Guid.Empty));
                                   },
                                   what: "DocumentMethods.SplitWire")
                               select receipt;
                    })
                let created = snapshot.Payload.Created
                select (DocumentResult)new DocumentResult.WirelessResult(Snapshot: new DocumentWirelessSnapshot(
                    Shout: created.Head, Listens: toSeq(created.Skip(count: 1)), Changed: snapshot.Payload.Changed)),
            findCase: static (s, find) =>
                from objects in s.NeedObjects()
                from shout in ResolveWireless<Shout>(objects: objects, id: find.Shout, op: Op.Of(name: nameof(Find)), label: "shout")
                let listens = ListensOf(shout: shout)
                select (DocumentResult)new DocumentResult.WirelessResult(Snapshot: new DocumentWirelessSnapshot(
                    Shout: shout.InstanceId.NonEmpty(), Listens: listens, Changed: 0)),
            connectCase: static (s, connect) =>
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Connect)),
                    mutate: (_, objs, actions) => {
                        Op op = Op.Of(name: nameof(Connect));
                        return from shout in ResolveWireless<Shout>(objects: objs, id: connect.Shout, op: op, label: "shout")
                               from listens in connect.Listens.TraverseM(id => ResolveWireless<Listen>(objects: objs, id: id, op: op, label: "listen")).As()
                               from _nonEmpty in listens.IsEmpty
                                   ? Fin.Fail<Seq<Listen>>(error: UiFault.InvalidInput(op: op, detail: "connect requires at least one listen id"))
                                   : Fin.Succ(value: listens)
                               from changed in op.Attempt(
                                   body: () => {
                                       // BOUNDARY ADAPTER -- Listen uses UserName as ShoutId; RenameAction snapshots each bind.
                                       string shoutKey = shout.InstanceId.ToString();
                                       return listens.Fold(0, (count, listen) => { actions.Add(new RenameAction(listen)); listen.UserName = shoutKey; listen.Expire(); return count + 1; });
                                   },
                                   what: "Shout/Listen pair")
                               select DocumentMutationReceipt.Count(changed: changed);
                    })
                select (DocumentResult)new DocumentResult.WirelessResult(Snapshot: new DocumentWirelessSnapshot(
                    Shout: connect.Shout.NonEmpty(), Listens: connect.Listens.Filter(static id => id != Guid.Empty), Changed: snapshot.Payload.Changed)),
            enumerateCase: static (s, _) =>
                from objects in s.NeedObjects()
                let shouts = toSeq(objects.ForwardsOfT<Shout>())
                select (DocumentResult)new DocumentResult.WirelessListResult(Snapshots: shouts.Map(shout => new DocumentWirelessSnapshot(
                    Shout: shout.InstanceId.NonEmpty(), Listens: ListensOf(shout: shout), Changed: 0))),
            renameCase: static (s, rename) =>
                from objects in s.NeedObjects()
                from shout in ResolveWireless<Shout>(objects: objects, id: rename.Shout, op: Op.Of(name: nameof(Rename)), label: "shout")
                let listens = ListensOf(shout: shout)
                from snapshot in UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: nameof(Rename)),
                    mutate: (_, _, actions) => Op.Of(name: nameof(Rename)).Attempt(
                        body: () => {
                            // BOUNDARY ADAPTER -- Listen binding key is shout.InstanceId.ToString(), not UserName, so rename is cosmetic; routing is unaffected.
                            actions.Add(new RenameAction(shout));
                            shout.UserName = rename.Label;
                            shout.Expire();
                            return DocumentMutationReceipt.Count(changed: 1);
                        },
                        what: "Shout rename"))
                select (DocumentResult)new DocumentResult.WirelessResult(Snapshot: new DocumentWirelessSnapshot(
                    Shout: rename.Shout.NonEmpty(), Listens: listens, Changed: snapshot.Payload.Changed)));

    private static Fin<T> ResolveWireless<T>(GhObjectList objects, Guid id, Op op, string label) where T : class, IDocumentObject =>
        op.AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: $"{label} id is empty"))
            .Bind(valid => Optional(objects.Find(instanceId: valid) as T)
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"{label} {valid} not found")));

    private static Seq<Guid> ListensOf(Shout shout) =>
        Optional(shout.Document).Map(document => {
            // BOUNDARY ADAPTER -- FindReferences splits Listen bindings from expression/script references.
            document.Globals.FindReferences(shout: shout, listenObjects: out IDocumentObject[] listenObjects, otherObjects: out _);
            return toSeq(listenObjects).Map(static listen => listen.InstanceId).Filter(static id => id != Guid.Empty);
        }).IfNone(Seq<Guid>());
}

// Document.File is FileUtility; HasPath means Path is non-null, but Path may still be empty.
[SmartEnum<int>]
public sealed partial class DocumentFileOp {
    public static readonly DocumentFileOp QueryPath = new(key: 0, run: static document =>
        Op.Of(name: nameof(QueryPath)).Attempt(
            body: () => (DocumentResult)new DocumentResult.FileResult(
                Snapshot: new DocumentFileSnapshot(HasPath: document.File.HasPath, Path: document.File.Path ?? string.Empty)),
            what: "Document.File"));

    [UseDelegateFromConstructor]
    internal partial Fin<DocumentResult> Run(GhDocument document);
}

[SmartEnum<int>]
public sealed partial class DocumentInspect {
    private delegate Unit InspectInvoke(GhDocumentMethods methods);

    public static readonly DocumentInspect DependencyGraph = new(key: 0, invoke: static methods => { methods.ShowDependencyGraph(); return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Invoke(GhDocumentMethods methods);
}

// StartWait is documented as non-UI-thread only; bridge execution stays on async Start/Stop/Close.
[SmartEnum<int>]
public sealed partial class SolutionControl {
    private delegate Fin<Unit> RunFn(SolutionServer server, GhDocument document, SolutionMode mode);

    public static readonly SolutionControl Start = new(key: 0, run: static (server, document, mode) =>
        Op.Of(name: nameof(Start)).Attempt(body: () => {
            // BOUNDARY ADAPTER -- surface fire-and-forget solver faults through HandlerFaultSink.
            Task pending = server.Start(mode: mode);
            _ = pending.ContinueWith(
                static t => Op.Side(() => GrasshopperUi.Handler(valid: () =>
                    Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(Start)), detail: t.Exception!.GetBaseException().Message)))),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return unit;
        }, what: "SolutionServer.Start"));
    public static readonly SolutionControl Stop = new(key: 1, run: static (server, _, _) =>
        Op.Of(name: nameof(Stop)).Attempt(body: () => { server.Stop(); return unit; }, what: "SolutionServer.Stop"));
    public static readonly SolutionControl Close = new(key: 2, run: static (_, document, _) =>
        Op.Of(name: nameof(Close)).Attempt(body: () => { document.Close(); return unit; }, what: "Document.Close"));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Run(SolutionServer server, GhDocument document, SolutionMode mode);
}

[GenerateUnionOps]
[Union(SwitchMapStateParameterName = "scope")]
public partial record DocumentHistory {
    private DocumentHistory() { }
    public sealed partial record QueryCase : DocumentHistory;
    public sealed partial record UndoCase : DocumentHistory;
    public sealed partial record RedoCase : DocumentHistory;
    public sealed partial record ClearCase : DocumentHistory;
    public sealed partial record ShowHistoryCase : DocumentHistory;
    public sealed partial record TargetCase(int Ordinal, bool IsRedo) : DocumentHistory;

    public static readonly DocumentHistory Query = new QueryCase();
    public static readonly DocumentHistory Undo = new UndoCase();
    public static readonly DocumentHistory Redo = new RedoCase();
    public static readonly DocumentHistory Clear = new ClearCase();
    public static readonly DocumentHistory ShowHistory = new ShowHistoryCase();
    public static DocumentHistory Target(int ordinal, bool redo = false) => new TargetCase(Ordinal: ordinal, IsRedo: redo);

    internal GrasshopperUiIntent<DocumentResult> Plan() =>
        Switch(
            queryCase: static _ => GhUi.Document(
                run: static scope => scope.NeedDocument().Map(document => (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document))),
                repaint: RepaintRequest.None),
            undoCase: static _ => GhUi.Document(
                run: static scope => Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Undo)), run: static document => document.Undo.Undo()),
                repaint: RepaintRequest.Canvas),
            redoCase: static _ => GhUi.Document(
                run: static scope => Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Redo)), run: static document => document.Undo.Redo()),
                repaint: RepaintRequest.Canvas),
            clearCase: static _ => GhUi.Document(
                run: static scope => Document.MutateHistory(scope: scope, op: Op.Of(name: nameof(Clear)), run: static document => document.Undo.Clear()),
                repaint: RepaintRequest.Canvas),
            showHistoryCase: static _ => GhUi.Canvas(run: static scope => Document.ShowHistory(scope: scope), repaint: RepaintRequest.None),
            targetCase: static t => GhUi.Document(
                run: scope => Document.MutateHistoryTarget(scope: scope, ordinal: t.Ordinal, redo: t.IsRedo),
                repaint: RepaintRequest.Canvas));
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
    public sealed record MarkResult(DocumentMarkSnapshot Snapshot) : DocumentResult;
    public sealed record ReviewResult(DocumentReviewSnapshot Snapshot, Subscription Subscription) : DocumentResult;
    public sealed record DisplayStateResult(DocumentDisplaySnapshot Snapshot) : DocumentResult;
    public sealed record NotesResult(string Value) : DocumentResult;
    public sealed record CustomValueResult(string Key, string Value) : DocumentResult;
    public sealed record GroupResult(DocumentGroupSnapshot Snapshot) : DocumentResult;
    public sealed record WirelessResult(DocumentWirelessSnapshot Snapshot) : DocumentResult;
    public sealed record GroupListResult(Seq<DocumentGroupSnapshot> Snapshots) : DocumentResult;
    public sealed record WirelessListResult(Seq<DocumentWirelessSnapshot> Snapshots) : DocumentResult;
    public sealed record FileResult(DocumentFileSnapshot Snapshot) : DocumentResult;
}

[SkipUnionOps]
[Union]
public partial record DocumentMarkFocus {
    private DocumentMarkFocus() { }
    public sealed record CurrentViewCase : DocumentMarkFocus;
    public sealed record SelectionCase : DocumentMarkFocus;
    public sealed record ObjectsCase(Seq<Guid> Ids) : DocumentMarkFocus;
    public sealed record FrameCase(RectangleF Bounds) : DocumentMarkFocus;

    public static readonly DocumentMarkFocus CurrentView = new CurrentViewCase();
    public static readonly DocumentMarkFocus Selection = new SelectionCase();
    public static DocumentMarkFocus Objects(params ReadOnlySpan<Guid> ids) => new ObjectsCase(Ids: toSeq(ids.ToArray()));
    public static DocumentMarkFocus Frame(RectangleF frame) => new FrameCase(Bounds: frame);

    internal Fin<NamedView> Build(GrasshopperUi.Scope scope, string category, string name) =>
        Switch(
            state: (Scope: scope, Category: category, Name: name, Op: Op.Of(name: nameof(DocumentMarkFocus))),
            currentViewCase: static (s, _) =>
                from canvas in s.Scope.NeedCanvas()
                from frame in s.Op.AcceptRect(value: canvas.VisibleFrame, detail: "invalid visible frame", requirePositive: true)
                select new NamedView(category: s.Category, name: s.Name, frame: frame),
            selectionCase: static (s, _) =>
                from objects in s.Scope.NeedObjects()
                let ids = toSeq(objects.SelectedObjects.Select(static o => o.InstanceId))
                from validIds in ids.IsEmpty
                    ? Fin.Fail<Seq<Guid>>(error: UiFault.InvalidInput(op: s.Op, detail: "selection mark requires selected objects"))
                    : Fin.Succ(value: ids)
                select new NamedView(category: s.Category, name: s.Name, objects: validIds),
            objectsCase: static (s, o) =>
                from objects in s.Scope.NeedObjects()
                from resolved in o.Ids.TraverseM(id => UiRail.ResolveObject(objects: objects, id: id, op: s.Op)).As()
                from nonEmpty in resolved.IsEmpty
                    ? Fin.Fail<Seq<IDocumentObject>>(error: UiFault.InvalidInput(op: s.Op, detail: "mark requires at least one object"))
                    : Fin.Succ(value: resolved)
                select new NamedView(category: s.Category, name: s.Name, objects: nonEmpty.Map(static obj => obj.InstanceId)),
            frameCase: static (s, f) =>
                from frame in s.Op.AcceptRect(value: f.Bounds, detail: "invalid mark frame", requirePositive: true)
                select new NamedView(category: s.Category, name: s.Name, frame: frame));
}

[GenerateUnionOps]
[Union(SwitchMapStateParameterName = "scope")]
public partial record DocumentMark {
    private DocumentMark() { }
    public sealed partial record ListCase(Option<string> Category) : DocumentMark;
    public sealed partial record SaveCase(string Name, string Category, DocumentMarkFocus Focus) : DocumentMark;
    public sealed partial record RemoveCase(string Name) : DocumentMark;
    public sealed partial record RecallCase(string Name, CanvasViewPolicy View) : DocumentMark;
    public sealed partial record RenameCase(string OldName, string NewName) : DocumentMark;

    public static DocumentMark List(string? category = null) => new ListCase(Category: Optional(category));
    public static DocumentMark Save(string name, DocumentMarkFocus focus, string category = "") => new SaveCase(Name: name, Category: category, Focus: focus);
    public static DocumentMark Remove(string name) => new RemoveCase(Name: name);
    public static DocumentMark Recall(string name, CanvasViewPolicy policy = default) => new RecallCase(Name: name, View: policy);
    public static DocumentMark Rename(string oldName, string newName) => new RenameCase(OldName: oldName, NewName: newName);

    internal GrasshopperUiIntent<DocumentResult> Plan() =>
        GhUi.Document(run: Apply, repaint: Switch(
            listCase: static _ => RepaintRequest.None,
            saveCase: static _ => RepaintRequest.None,
            removeCase: static _ => RepaintRequest.None,
            recallCase: static _ => RepaintRequest.Canvas,
            renameCase: static _ => RepaintRequest.None));

    internal Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) =>
        Switch(
            scope: scope,
            listCase: static (s, list) =>
                from document in s.NeedDocument()
                select (DocumentResult)new DocumentResult.MarkResult(Snapshot: MarkSnapshot(document: document, category: list.Category)),
            saveCase: static (s, save) =>
                from document in s.NeedDocument()
                from name in Op.Of(name: nameof(Save)).AcceptText(value: save.Name)
                from focus in Optional(save.Focus).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Save)), detail: "mark focus is required"))
                from view in focus.Build(scope: s, category: save.Category, name: name)
                    // NamedViews.Add overwrites same-name views; existence is the only useful receipt.
                let overwriting = toSeq(document.NamedViews.Views).Exists(v => string.Equals(a: v.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase))
                from _ in Op.Of(name: nameof(Save)).Attempt(body: () => { _ = document.NamedViews.Add(view: view); document.Modify(); return unit; }, what: "NamedViews.Add")
                select (DocumentResult)new DocumentResult.MarkResult(Snapshot: MarkSnapshot(document: document, category: Optional(save.Category), name: Some(name), changed: !overwriting)),
            removeCase: static (s, remove) =>
                from document in s.NeedDocument()
                from name in Op.Of(name: nameof(Remove)).AcceptText(value: remove.Name)
                from changed in Op.Of(name: nameof(Remove)).Attempt(body: () => document.NamedViews.Remove(name: name), what: "NamedViews.Remove")
                from _ in changed
                    ? Op.Of(name: nameof(Remove)).Attempt(body: () => { document.Modify(); return unit; }, what: "Document.Modify")
                    : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(Remove)), detail: $"mark '{name}' not found"))
                select (DocumentResult)new DocumentResult.MarkResult(Snapshot: MarkSnapshot(document: document, category: default, name: Some(name), changed: changed)),
            recallCase: static (s, recall) =>
                from document in s.NeedDocument()
                from canvas in s.NeedCanvas()
                from objects in s.NeedObjects()
                from methods in s.NeedMethods()
                from name in Op.Of(name: nameof(Recall)).AcceptText(value: recall.Name)
                from view in FindNamedView(document: document, name: name, op: Op.Of(name: nameof(Recall)))
                from frame in Op.Of(name: nameof(Recall)).Attempt(body: () => {
                    bool resolved = view.ResolveRectangle(document, out RectangleF rectangle);
                    return resolved ? rectangle : RectangleF.Empty;
                }, what: "NamedView.ResolveRectangle")
                from baseFrame in Op.Of(name: nameof(Recall)).AcceptRect(value: frame, detail: "invalid mark frame", requirePositive: true)
                let policy = UiRail.ResolveView(raw: recall.View)
                let validFrame = RectangleF.Inflate(rectangle: baseFrame, width: policy.Padding, height: policy.Padding)
                from _ in Op.Of(name: nameof(Recall)).Attempt(body: () => {
                    canvas.Navigate(frame: validFrame, zoomLimits: (policy.MinimumZoom, policy.MaximumZoom), duration: policy.Duration);
                    // BOUNDARY ADAPTER -- selection is unconditional on recall: clear then re-select the mark's objects.
                    _ = methods.DeselectAll();
                    _ = Optional(view.Objects).Map(static ids => toSeq(ids)).IfNone(Seq<Guid>())
                        .Choose(id => Optional(objects.Find(instanceId: id)))
                        .Iter(static obj => obj.Selection = ObjectSelection.Selected);
                    return unit;
                }, what: "DocumentMark.Recall")
                select (DocumentResult)new DocumentResult.MarkResult(Snapshot: MarkSnapshot(document: document, category: default, name: Some(name), frame: Some(validFrame))),
            renameCase: static (s, rename) =>
                from document in s.NeedDocument()
                from oldName in Op.Of(name: nameof(Rename)).AcceptText(value: rename.OldName)
                from newName in Op.Of(name: nameof(Rename)).AcceptText(value: rename.NewName)
                from view in FindNamedView(document: document, name: oldName, op: Op.Of(name: nameof(Rename)))
                from _distinct in string.Equals(a: oldName, b: newName, comparisonType: StringComparison.OrdinalIgnoreCase)
                    ? Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(Rename)), detail: $"mark '{oldName}' already has that name"))
                    : Fin.Succ(value: unit)
                from _free in toSeq(document.NamedViews.OrderedViews).Exists(v => string.Equals(a: v.Name, b: newName, comparisonType: StringComparison.OrdinalIgnoreCase))
                    ? Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(Rename)), detail: $"a mark named '{newName}' already exists"))
                    : Fin.Succ(value: unit)
                from changed in Op.Of(name: nameof(Rename)).Attempt(body: () => {
                    // BOUNDARY ADAPTER -- NamedView is immutable; rename clones into a fresh view then atomically swaps.
                    NamedView renamed = view.Frame.HasValue
                        ? new NamedView(category: view.Category, name: newName, frame: view.Frame.Value)
                        : new NamedView(category: view.Category, name: newName, objects: view.Objects);
                    bool removed = document.NamedViews.Remove(name: oldName);
                    bool added = removed && document.NamedViews.Add(view: renamed);
                    _ = added ? Op.Side(() => document.Modify()) : unit;
                    return added;
                }, what: "NamedViews.Rename")
                from _ in changed
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(Rename)), detail: $"rename of mark '{oldName}' rejected"))
                select (DocumentResult)new DocumentResult.MarkResult(Snapshot: MarkSnapshot(document: document, category: default, name: Some(newName), changed: changed)));

    private static DocumentMarkSnapshot MarkSnapshot(GhDocument document, Option<string> category, Option<string> name = default, Option<RectangleF> frame = default, bool changed = false) {
        Seq<NamedView> views = toSeq(document.NamedViews.OrderedViews);
        Seq<string> names = category is { IsSome: true, Case: string c }
            ? views.Filter(view => string.Equals(a: view.Category, b: c, comparisonType: StringComparison.OrdinalIgnoreCase)).Map(static view => view.Name)
            : views.Map(static view => view.Name);
        return new DocumentMarkSnapshot(Names: names, Name: name, Frame: frame, Changed: changed);
    }

    private static Fin<NamedView> FindNamedView(GhDocument document, string name, Op op) =>
        toSeq(document.NamedViews.OrderedViews)
            .Find(view => string.Equals(a: view.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"mark '{name}' not found"));
}

[SkipUnionOps]
[Union]
public partial record DocumentReview {
    private DocumentReview() { }
    public sealed record CompareFilesCase(string Left, string Right, bool IncludeText = true) : DocumentReview;

    public static DocumentReview CompareFiles(string left, string right, bool includeText = true) =>
        new CompareFilesCase(Left: left, Right: right, IncludeText: includeText);

    internal GrasshopperUiPolicy Policy => Switch(compareFilesCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.None));

    internal Fin<DocumentResult> Apply(GrasshopperUi.Scope scope) =>
        Switch(state: scope, compareFilesCase: static (_, compare) => {
            Op op = Op.Of(name: nameof(CompareFiles));
            Form? form = null;
            return from left in op.AcceptText(value: compare.Left)
                   from right in op.AcceptText(value: compare.Right)
                   from subscription in Subscription.Bind(
                       attach: () => form = DiffCanvas.ShowFileCompareForm(fileA: left, fileB: right, includeTextualComparison: compare.IncludeText)
                           ?? throw new InvalidOperationException(message: "DiffCanvas did not create a comparison form."),
                       detach: () => form?.Close(),
                       marshalToUi: true,
                       detachOnce: true)
                   select (DocumentResult)new DocumentResult.ReviewResult(
                       Snapshot: new DocumentReviewSnapshot(Left: left, Right: right, IncludeText: compare.IncludeText, Shown: true),
                       Subscription: subscription);
        });
}

[SkipUnionOps]
[Union]
public partial record ObjectSpec {
    private ObjectSpec() { }
    public sealed record SliderCase(string Name, UiNumber Number, Option<string> Format, GripShape Shape, Color Colour) : ObjectSpec;
    public sealed record ToggleCase(string Name, bool State) : ObjectSpec;
    // Scribble is a DocumentObject; DataPanel/ValueList are Parameters and need dataflow-specific placement.
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
                select (IDocumentObject)new NumberSliderObject(userName: name, number: s.Number) {
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
        op.AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: $"{label} cue id is empty"))
            .Bind(valid => Optional(objects.FindParameter(instanceId: valid))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"{label} cue parameter {valid} not found"))
                .Map(static parameter => Some(parameter)));
}

[SmartEnum<int>]
public sealed partial class DocumentTargetVerb {
    private delegate int RunSelectedFn(GhDocumentMethods methods, GhObjectList objects, ActionList actions);
    private delegate int RunObjectsFn(GhDocumentMethods methods, GhObjectList objects, IDocumentObject[] targets, ActionList actions);

    public static readonly DocumentTargetVerb Show = new(
        key: 0,
        runSelected: static (m, _, a) => m.ShowSelected(actions: a),
        runObjects: static (m, _, t, a) => m.ShowObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb Hide = new(
        key: 1,
        runSelected: static (m, _, a) => m.HideSelected(actions: a),
        runObjects: static (m, _, t, a) => m.HideObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb ToggleVisibility = new(
        key: 2,
        runSelected: static (m, _, a) => m.ToggleDisplaySelected(actions: a),
        runObjects: static (m, _, t, a) => m.ToggleDisplayObjects(objects: t, actions: a));
    public static readonly DocumentTargetVerb Enable = new(
        key: 3,
        runSelected: static (_, objects, actions) => SetActivity(targets: objects.SelectedObjects, activity: ObjectActivity.Enabled, actions: actions),
        runObjects: static (_, _, targets, actions) => SetActivity(targets: targets, activity: ObjectActivity.Enabled, actions: actions));
    public static readonly DocumentTargetVerb Disable = new(
        key: 4,
        runSelected: static (_, objects, actions) => SetActivity(targets: objects.SelectedObjects, activity: ObjectActivity.Disabled, actions: actions),
        runObjects: static (_, _, targets, actions) => SetActivity(targets: targets, activity: ObjectActivity.Disabled, actions: actions));
    public static readonly DocumentTargetVerb ToggleActivity = new(
        key: 5,
        runSelected: static (_, objects, actions) => ToggleActivityOf(targets: objects.SelectedObjects, actions: actions),
        runObjects: static (_, _, targets, actions) => ToggleActivityOf(targets: targets, actions: actions));

    [UseDelegateFromConstructor]
    private partial int RunSelected(GhDocumentMethods methods, GhObjectList objects, ActionList actions);

    [UseDelegateFromConstructor]
    private partial int RunObjects(GhDocumentMethods methods, GhObjectList objects, IDocumentObject[] targets, ActionList actions);

    internal int Run(GhDocumentMethods methods, GhObjectList objects, IDocumentObject[]? targets, ActionList actions) =>
        targets is null ? RunSelected(methods: methods, objects: objects, actions: actions) : RunObjects(methods: methods, objects: objects, targets: targets, actions: actions);

    private static int SetActivity(IEnumerable<IDocumentObject> targets, ObjectActivity activity, ActionList actions) =>
        toSeq(targets).Fold(
            initialState: 0,
            f: (count, obj) => obj.Activity == activity
                ? count
                : ApplyActivity(count: count, obj: obj, activity: activity, actions: actions));

    // Toggle flips each object to the opposite activity, so it always applies (never the no-op SetActivity skip).
    private static int ToggleActivityOf(IEnumerable<IDocumentObject> targets, ActionList actions) =>
        toSeq(targets).Fold(
            initialState: 0,
            f: (count, obj) => ApplyActivity(count: count, obj: obj, activity: obj.Activity == ObjectActivity.Enabled ? ObjectActivity.Disabled : ObjectActivity.Enabled, actions: actions));

    private static int ApplyActivity(int count, IDocumentObject obj, ObjectActivity activity, ActionList actions) {
        // BOUNDARY ADAPTER -- GH2 mutation: record the undoable action then flip + expire the object in place.
        actions.Add(new ObjectActivityAction(obj));
        obj.Activity = activity;
        obj.Expire();
        return count + 1;
    }
}

[SkipUnionOps]
[Union]
public abstract partial record DocumentTargetOp {
    public sealed record VerbCase(DocumentTargetVerb Verb) : DocumentTargetOp;
    public sealed record SelectionCase(SelectionOp Op) : DocumentTargetOp;
    public sealed record DisplayCase(DisplayPort Port, VisibilityChange Change) : DocumentTargetOp;
    public sealed record DeleteCase(bool DataOnly, Seq<WireEnds> Wires) : DocumentTargetOp;
    public sealed record ColourCase(Option<GhColour> Override) : DocumentTargetOp;
    public sealed record NudgeCase(int Dx, int Dy) : DocumentTargetOp;
    public sealed record RenameCase(string Name) : DocumentTargetOp;

    public static readonly DocumentTargetOp Show = new VerbCase(Verb: DocumentTargetVerb.Show);
    public static readonly DocumentTargetOp Hide = new VerbCase(Verb: DocumentTargetVerb.Hide);
    public static readonly DocumentTargetOp ToggleVisibility = new VerbCase(Verb: DocumentTargetVerb.ToggleVisibility);
    public static readonly DocumentTargetOp Enable = new VerbCase(Verb: DocumentTargetVerb.Enable);
    public static readonly DocumentTargetOp Disable = new VerbCase(Verb: DocumentTargetVerb.Disable);
    public static readonly DocumentTargetOp ToggleActivity = new VerbCase(Verb: DocumentTargetVerb.ToggleActivity);
    public static DocumentTargetOp Delete(bool dataOnly = false, Seq<WireEnds> wires = default) => new DeleteCase(DataOnly: dataOnly, Wires: wires);
    public static DocumentTargetOp Style(GhColour colour) => new ColourCase(Override: Some(colour));
    public static readonly DocumentTargetOp ClearStyle = new ColourCase(Override: Option<GhColour>.None);
    public static DocumentTargetOp Nudge(int dx, int dy) => new NudgeCase(Dx: dx, Dy: dy);
    public static readonly DocumentTargetOp NudgeLeft = new NudgeCase(Dx: -1, Dy: 0);
    public static readonly DocumentTargetOp NudgeRight = new NudgeCase(Dx: 1, Dy: 0);
    public static readonly DocumentTargetOp NudgeUp = new NudgeCase(Dx: 0, Dy: -1);
    public static readonly DocumentTargetOp NudgeDown = new NudgeCase(Dx: 0, Dy: 1);
    public static DocumentTargetOp Rename(string name) => new RenameCase(Name: name);

    internal Fin<int> Apply(GhDocumentMethods methods, GhObjectList objects, Seq<Guid> ids, ActionList actions) =>
        ids.TraverseM(id => Optional(objects.Find(instanceId: id))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ObjectScope)), detail: $"object {id} not found")))
        .Map(static resolved => resolved.ToArray())
        .Bind(targets => Dispatch(methods: methods, objects: objects, actions: actions, targets: targets)).As();

    internal Fin<int> ApplySelected(GhDocumentMethods methods, GhObjectList objects, ActionList actions) =>
        Dispatch(methods: methods, objects: objects, actions: actions, targets: null);

    // Null sentinel chooses *Selected(actions); non-null chooses *Objects(targets, actions).
    private Fin<int> Dispatch(GhDocumentMethods methods, GhObjectList objects, ActionList actions, IDocumentObject[]? targets) =>
        Switch(
            state: (methods, objects, targets, actions),
            verbCase: static (s, v) => Op.Of(name: nameof(DocumentTargetVerb)).Attempt(
                body: () => v.Verb.Run(methods: s.methods, objects: s.objects, targets: s.targets, actions: s.actions),
                what: "DocumentTargetVerb"),
            selectionCase: static (s, selection) => UiRail.SelectionDispatch(methods: s.methods, objects: s.objects, op: selection.Op),
            displayCase: static (s, display) => Op.Of(name: nameof(DisplayPort)).Attempt(
                body: () => display.Port.Run(methods: s.methods, show: display.Change.IsShow, actions: s.actions),
                what: "DisplayPort"),
            deleteCase: static (s, delete) => Op.Of(name: nameof(DeleteCase)).Attempt(
                body: () => UiRail.RunDelete(methods: s.methods, targets: s.targets, dataOnly: delete.DataOnly, wires: delete.Wires, actions: s.actions),
                what: "DocumentMethods.Delete"),
            colourCase: static (s, colour) => {
                GhColour? value = colour.Override.OrNull();
                return Op.Of(name: nameof(ColourCase)).Attempt(
                    body: () => s.targets is null
                        ? s.methods.SetColourOverrideSelected(colour: value, actions: s.actions)
                        : s.methods.SetColourOverrideObjects(objects: s.targets, colour: value, actions: s.actions),
                    what: "DocumentMethods.SetColourOverride");
            },
            nudgeCase: static (s, nudge) => MoveTargets(objects: s.objects, targets: s.targets, dx: nudge.Dx, dy: nudge.Dy, actions: s.actions),
            renameCase: static (s, rename) => RenameTargets(objects: s.objects, targets: s.targets, name: rename.Name, actions: s.actions));

    private static Fin<int> RenameTargets(GhObjectList objects, IDocumentObject[]? targets, string name, ActionList actions) =>
        Op.Of(name: nameof(RenameCase)).Attempt(body: () => {
            Seq<IDocumentObject> selected = targets is null ? toSeq(objects.SelectedObjects) : toSeq(targets);
            _ = selected.Iter(obj => {
                actions.Add(new RenameAction(obj));
                obj.UserName = name;
                obj.Expire();
            });
            return selected.Count;
        }, what: "target rename");

    private static Fin<int> MoveTargets(GhObjectList objects, IDocumentObject[]? targets, int dx, int dy, ActionList actions) {
        Op op = Op.Of(name: nameof(NudgeCase));
        return (dx, dy) switch {
            (0, 0) => Fin.Succ(value: 0),
            _ => op.Attempt(body: () => {
                Seq<IDocumentObject> selected = targets is null ? toSeq(objects.SelectedObjects) : toSeq(targets);
                _ = selected.Iter(obj => {
                    actions.Add(new PivotAction(obj: obj));
                    obj.Attributes.Move(dx: dx, dy: dy);
                    obj.Expire();
                });
                return selected.Count;
            }, what: "target attribute nudge"),
        };
    }
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
    public sealed record MigrateCase(Seq<Guid> Ids, PointF Location) : DocumentMutation;
    public sealed record SplitWireCase(Guid Source, Guid TargetId, string Name, PointF Location) : DocumentMutation;
    public sealed record WireRouteCase(Seq<Guid> Chain) : DocumentMutation;
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
    public static DocumentMutation Compose(ComposeOp op) => new ComposeCase(Subject: ObjectScope.Selection, Op: op);
    public static DocumentMutation Compose(ObjectScope subject, ComposeOp op) => new ComposeCase(Subject: subject, Op: op);
    public static DocumentMutation Drop(Guid proxyId, PointF location, string? init = null) => new DropCase(ProxyId: proxyId, Location: location, Init: Optional(init));
    public static DocumentMutation DropSnippet(string file, PointF location) => new DropSnippetCase(File: file, Location: location);
    public static DocumentMutation Place(ObjectSpec obj, PointF location, DropCue? cue = null) => new PlaceCase(Object: obj, Location: location, Cue: cue ?? DropCue.None);
    public static DocumentMutation AddDependency(PointF location) => new AddDependencyCase(Location: location);
    public static DocumentMutation Isolate(IsolateOptions options = default) => new IsolateCase(Options: options);
    public static DocumentMutation Migrate(Seq<Guid> ids, PointF location) => new MigrateCase(Ids: ids, Location: location);
    public static DocumentMutation SplitWire(Guid source, Guid target, string name, PointF location) => new SplitWireCase(Source: source, TargetId: target, Name: name, Location: location);
    public static DocumentMutation WireRoute(Seq<Guid> chain) => new WireRouteCase(Chain: chain);
    // ObjectList.Absorb imports foreign objects and repairs pins internally.
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
                       from changed in op.Attempt(
                           body: () => drop.Init is { IsSome: true, Case: string init }
                               ? s.methods.DropObject(obj: (proxy, init), location: location, actions: s.actions)
                               : s.methods.DropObject(obj: proxy, location: location, actions: s.actions),
                           what: "DocumentMethods.DropObject")
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
                       from receipt in changed
                           ? Fin.Succ(value: DocumentMutationReceipt.Count(changed: 1))
                           : Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: "DropSnippet rejected the snippet"))
                       select receipt;
            },
            placeCase: static (s, place) => {
                Op op = Op.Of(name: nameof(Place));
                return from spec in Optional(place.Object).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "object spec is required"))
                       from location in op.AcceptPoint(value: place.Location, detail: "non-finite location")
                       from obj in spec.Build(op: op)
                       from resolved in place.Cue.Resolve(objects: s.objects, op: op)
                       from native in op.Attempt(
                           body: () => (resolved.Source.IsSome || resolved.Target.IsSome) switch {
                               true => s.methods.DropObject(
                                   obj: obj,
                                   location: location,
                                   sourceCue: resolved.Source.OrNull(),
                                   targetCue: resolved.Target.OrNull(),
                                   actions: s.actions),
                               false => s.methods.DropObject(obj: obj, location: location, actions: s.actions),
                           },
                           what: "DocumentMethods.DropObject")
                       from receipt in native
                           ? Fin.Succ(value: DocumentMutationReceipt.CreatedObject(id: obj.InstanceId))
                           : Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: "DropObject rejected the placed object (init parse or cue mismatch)"))
                       select receipt;
            },
            addDependencyCase: static (s, dependency) => Op.Of(name: nameof(AddDependency))
                .AcceptPoint(value: dependency.Location, detail: "non-finite location")
                .Bind(valid => Op.Of(name: nameof(AddDependency)).Attempt(
                    body: () => Optional(s.methods.AddDependency(location: valid, actions: s.actions))
                    .Map(static listen => listen.InstanceId)
                    .Bind(static id => id.NonEmpty())
                    .Map(static id => DocumentMutationReceipt.CreatedObject(id: id))
                    .IfNone(DocumentMutationReceipt.None),
                    what: "DocumentMethods.AddDependency")),
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
            migrateCase: static (s, migrate) => {
                Op op = Op.Of(name: nameof(Migrate));
                return from location in op.AcceptPoint(value: migrate.Location, detail: "non-finite location")
                       from objects in migrate.Ids.TraverseM(id => Optional(s.objects.Find(instanceId: id))
                           .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"object {id} not found"))).As()
                       from _objects in objects.IsEmpty
                           ? Fin.Fail<Seq<IDocumentObject>>(error: UiFault.InvalidInput(op: op, detail: "no objects to migrate"))
                           : Fin.Succ(value: objects)
                       from remap in op.Attempt(
                           body: () => s.methods.MigrateObjects(objects: objects, location: location, actions: s.actions),
                           what: "DocumentMethods.MigrateObjects")
                       from receipt in remap.Count > 0
                           ? Fin.Succ(value: DocumentMutationReceipt.Of(changed: remap.Count, created: toSeq(remap.Values)))
                           : Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: "MigrateObjects returned no remap"))
                       select receipt;
            },
            splitWireCase: static (s, split) => {
                Op op = Op.Of(name: nameof(SplitWire));
                return from location in op.AcceptPoint(value: split.Location, detail: "non-finite location")
                       let name = split.Name ?? string.Empty
                       from source in Optional(s.objects.FindParameter(instanceId: split.Source))
                           .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"source parameter {split.Source} not found"))
                       from target in Optional(s.objects.FindParameter(instanceId: split.TargetId))
                           .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"target parameter {split.TargetId} not found"))
                       from receipt in op.Attempt(
                           body: () => {
                               // BOUNDARY ADAPTER -- SplitWire reports the inserted Shout/Listen pair through out-params.
                               bool changed = s.methods.SplitWire(source: source, target: target, name: name, location: location, shout: out Shout? shout, listen: out Listen? listen, actions: s.actions);
                               return DocumentMutationReceipt.Of(changed: changed ? 1 : 0, created: Seq(shout?.InstanceId ?? Guid.Empty, listen?.InstanceId ?? Guid.Empty));
                           },
                           what: "DocumentMethods.SplitWire")
                       select receipt;
            },
            wireRouteCase: static (s, route) => {
                Op op = Op.Of(name: nameof(WireRoute));
                return Wire.ApplyRouteChain(methods: s.methods, objects: s.objects, actions: s.actions, chain: route.Chain, op: op);
            },
            mergeCase: static (s, merge) => Op.Of(name: nameof(Merge)).Attempt(
                body: () => {
                    // Absorb already runs RepairPins (decompile-verified); no second pass.
                    int changed = s.objects.Absorb(other: merge.Source, reinstateInputs: merge.ReinstateInputs, reinstateOutputs: merge.ReinstateOutputs, actions: s.actions);
                    return DocumentMutationReceipt.Count(changed: changed);
                },
                what: "ObjectList.Absorb"));
}

[GenerateUnionOps]
[Union]
public partial record DocumentOp : IUiOp<DocumentResult> {
    private DocumentOp() { }
    public sealed partial record QueryCase(DocumentQuery Request) : DocumentOp;
    public sealed partial record MutateCase(Seq<DocumentMutation> Mutations, DocumentMutationPolicy Policy) : DocumentOp;
    public sealed partial record ClipboardCase(ClipboardOp Op) : DocumentOp;
    public sealed partial record HistoryCase(DocumentHistory Request) : DocumentOp;
    public sealed partial record InspectCase(DocumentInspect Kind) : DocumentOp;
    public sealed partial record SolutionCase(SolutionControl Control, SolutionMode Mode) : DocumentOp;
    public sealed partial record MarkCase(DocumentMark Request) : DocumentOp;
    public sealed partial record ReviewCase(DocumentReview Request) : DocumentOp;
    public sealed partial record GroupCase(GroupOp Op) : DocumentOp;
    public sealed partial record WirelessCase(WirelessOp Op) : DocumentOp;
    public sealed partial record CustomValueCase(string Key, Option<string> Value) : DocumentOp;
    public sealed partial record FileCase(DocumentFileOp Op) : DocumentOp;

    public static DocumentOp Query(DocumentQuery query) => new QueryCase(Request: query);
    public static DocumentOp Mutate(params ReadOnlySpan<DocumentMutation> mutations) =>
        new MutateCase(Mutations: toSeq(mutations.ToArray()), Policy: DocumentMutationPolicy.Default);
    public static DocumentOp Mutate(DocumentMutationPolicy policy, params ReadOnlySpan<DocumentMutation> mutations) =>
        new MutateCase(Mutations: toSeq(mutations.ToArray()), Policy: policy);
    public static DocumentOp Clipboard(ClipboardOp op) => new ClipboardCase(Op: op);
    public static DocumentOp History(DocumentHistory history) => new HistoryCase(Request: history);
    public static DocumentOp Inspect(DocumentInspect kind) => new InspectCase(Kind: kind);
    public static readonly DocumentOp DependencyGraph = new InspectCase(Kind: DocumentInspect.DependencyGraph);
    public static DocumentOp Solution(SolutionControl control, SolutionMode mode = SolutionMode.Regular) => new SolutionCase(Control: control, Mode: mode);
    public static DocumentOp Mark(DocumentMark mark) => new MarkCase(Request: mark);
    public static DocumentOp Review(DocumentReview review) => new ReviewCase(Request: review);
    public static DocumentOp Group(GroupOp op) => new GroupCase(Op: op);
    public static DocumentOp Wireless(WirelessOp op) => new WirelessCase(Op: op);
    public static DocumentOp CustomValue(string key, string value) => new CustomValueCase(Key: key, Value: Some(value));
    public static DocumentOp ClearCustomValue(string key) => new CustomValueCase(Key: key, Value: None);
    public static DocumentOp File(DocumentFileOp op) => new FileCase(Op: op);

    GrasshopperUiIntent<DocumentResult> IUiOp<DocumentResult>.Intent() => Document.Plan(op: this);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentMarkSnapshot(Seq<string> Names, Option<string> Name = default, Option<RectangleF> Frame = default, bool Changed = false);

public readonly record struct DocumentReviewSnapshot(string Left, string Right, bool IncludeText, bool Shown);

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

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentDisplaySnapshot(bool Enabled, bool DrawWires, bool DrawMeshes, TimeSpan Throttling, string RuleSetName);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentFileSnapshot(bool HasPath, string Path);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentGroupSnapshot(Option<Guid> Group, Option<string> Label, Option<GhOpenColorFamily> Colour, Seq<Guid> Members, int Changed, bool Exists);

[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentWirelessSnapshot(Option<Guid> Shout, Seq<Guid> Listens, int Changed);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class OptionNativeExtensions {
    internal static T? OrNull<T>(this Option<T> option) where T : class =>
        option is { IsSome: true, Case: T value } ? value : null;
    internal static T? OrNullable<T>(this Option<T> option) where T : struct =>
        option is { IsSome: true, Case: T value } ? value : null;
    internal static Option<Guid> NonEmpty(this Guid id) => id == Guid.Empty ? None : Some(id);
    internal static Option<Guid> NonEmpty(this Guid? id) => id is Guid g && g != Guid.Empty ? Some(g) : None;
}

internal static class SnapshotFormatExtensions {
    internal static string Inv<T>(this T value) => $"{value}";
}

internal static partial class Document {
    internal static GrasshopperUiIntent<DocumentResult> Plan(DocumentOp op) =>
        op.Switch(
            queryCase: static q => new GrasshopperUiIntent<DocumentResult>(
                policy: q.Request.QueryPolicy,
                run: scope => Query(scope: scope, query: q.Request)),
            mutateCase: static m => GhUi.Document(
                run: scope => Mutate(scope: scope, mutations: m.Mutations, policy: m.Policy),
                repaint: m.Policy.RepaintOrDefault),
            clipboardCase: static c => new GrasshopperUiIntent<DocumentResult>(
                policy: c.Op.Switch(
                    verbCase: static v => (v.Verb == ClipboardVerb.Copy) switch {
                        true => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
                        false => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
                    }),
                run: scope => RunClipboard(scope: scope, op: c.Op)
                    .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta))),
            historyCase: static h => h.Request.Plan(),
            inspectCase: static i => GhUi.Document(run: scope => DispatchInspect(scope: scope, kind: i.Kind), repaint: RepaintRequest.None),
            solutionCase: static sol => GhUi.Document(
                run: scope =>
                    from document in scope.NeedDocument()
                    from objects in scope.NeedObjects()
                    from _ in sol.Control.Run(server: document.Solution, document: document, mode: sol.Mode)
                    select (DocumentResult)new DocumentResult.SolutionResult(Snapshot: UiRail.DocumentSnapshotOf(document: document, objects: objects)),
                repaint: RepaintRequest.None),
            markCase: static m => m.Request.Plan(),
            reviewCase: static r => new GrasshopperUiIntent<DocumentResult>(
                policy: r.Request.Policy,
                run: scope => r.Request.Apply(scope: scope)),
            groupCase: static g => new GrasshopperUiIntent<DocumentResult>(
                policy: g.Op.Policy,
                run: scope => g.Op.Apply(scope: scope)),
            wirelessCase: static w => new GrasshopperUiIntent<DocumentResult>(
                policy: w.Op.Policy,
                run: scope => w.Op.Apply(scope: scope)),
            customValueCase: static c => GhUi.Document(
                run: scope => SetCustomValue(scope: scope, key: c.Key, value: c.Value),
                repaint: RepaintRequest.None),
            fileCase: static f => GhUi.Document(
                run: scope => scope.NeedDocument().Bind(document => f.Op.Run(document: document)),
                repaint: RepaintRequest.None));

    private static Fin<DocumentResult> DispatchInspect(GrasshopperUi.Scope scope, DocumentInspect kind) =>
        from methods in scope.NeedMethods()
        from _ in Op.Of(name: nameof(DocumentOp.Inspect)).Attempt(body: () => kind.Invoke(methods), what: string.Create(CultureInfo.InvariantCulture, $"DocumentInspect[{kind.Key}]"))
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
                Universe().Map(snapshot => (DocumentResult)new DocumentResult.UniverseResult(Snapshot: snapshot)),
            displayStateCase: static (s, _) =>
                s.NeedDocument().Map(doc => (DocumentResult)new DocumentResult.DisplayStateResult(Snapshot: new DocumentDisplaySnapshot(
                    Enabled: doc.Display.Enabled,
                    DrawWires: doc.Display.DrawWires,
                    DrawMeshes: doc.Display.DrawMeshes,
                    Throttling: doc.Display.Throttling,
                    RuleSetName: doc.Display.CurrentRuleSetName))),
            notesCase: static (s, _) =>
                s.NeedDocument().Map(doc => (DocumentResult)new DocumentResult.NotesResult(Value: doc.Notes)),
            customValueCase: static (s, custom) =>
                from key in Op.Of(name: nameof(DocumentQuery.CustomValue)).AcceptText(value: custom.Key)
                from doc in s.NeedDocument()
                select (DocumentResult)new DocumentResult.CustomValueResult(Key: key, Value: doc.CustomValues.Get(key: key, @default: custom.DefaultValue)));

    private static Fin<DocumentResult> SetCustomValue(GrasshopperUi.Scope scope, string key, Option<string> value) =>
        from validKey in Op.Of(name: nameof(DocumentOp.CustomValue)).AcceptText(value: key)
        from doc in scope.NeedDocument()
        from _ in Op.Of(name: nameof(DocumentOp.CustomValue)).Attempt(
            body: () => {
                // BOUNDARY ADAPTER -- KeyedValues.Set(string,string) selects the text overload.
                _ = value is { IsSome: true, Case: string text }
                    ? Op.Side(() => doc.CustomValues.Set(key: validKey, value: text))
                    : Op.Side(() => doc.CustomValues.Delete(key: validKey));
                doc.Modify();
                return unit;
            },
            what: "Document.CustomValues")
        select (DocumentResult)new DocumentResult.CustomValueResult(Key: validKey, Value: doc.CustomValues.Get(key: validKey, @default: string.Empty));

    internal static Fin<DocumentResult> MutateHistory(GrasshopperUi.Scope scope, Op op, Action<GhDocument> run) =>
        from document in scope.NeedDocument()
        let before = document.Modifications
        from _ in Try.lift(f: () => { run(document); return unit; }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: error.Message))
        from changed in document.Modifications == before
            ? Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "nothing to undo/redo/clear"))
            : Fin.Succ(value: unit)
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document));

    internal static Fin<DocumentResult> ShowHistory(GrasshopperUi.Scope scope) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        from _ in Op.Of(name: nameof(DocumentHistory.ShowHistory)).Attempt(
            body: () => { _ = History.ShowHistory(canvas: canvas); return unit; },
            what: "History.ShowHistory")
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document));

    internal static Fin<DocumentResult> MutateHistoryTarget(GrasshopperUi.Scope scope, int ordinal, bool redo) =>
        from document in scope.NeedDocument()
        from validOrdinal in ordinal >= 0
            ? Fin.Succ(value: ordinal)
            : Fin.Fail<int>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentHistory.Target)), detail: "history ordinal must be non-negative"))
        from node in toSeq(redo ? document.Undo.CentralRedoSequence : document.Undo.CentralUndoSequence)
            .Skip(validOrdinal).Head
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentHistory.Target)), detail: string.Create(CultureInfo.InvariantCulture, $"no history node at ordinal {ordinal}")))
        from _gate in (node.IsRoot, node.Record.State == (redo ? Grasshopper2.Undo.State.Redo : Grasshopper2.Undo.State.Undo)) switch {
            (true, _) => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentHistory.Target)), detail: "root history node cannot be replayed")),
            (_, false) => Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(DocumentHistory.Target)), detail: redo ? "history node is not in a redoable state" : "history node is not in an undoable state")),
            _ => Fin.Succ(value: unit),
        }
        from _ in Op.Of(name: nameof(DocumentHistory.Target)).Attempt(
            body: () => {
                // BOUNDARY ADAPTER -- direction-dispatched native replay; one Attempt wraps the chosen action.
                (redo ? (Action<Node>)document.Undo.Redo : document.Undo.Undo)(node);
                return unit;
            },
            what: "History node target")
        select (DocumentResult)new DocumentResult.HistoryResult(Snapshot: UiRail.HistorySnapshotOf(document: document));

    private static Fin<DocumentResult> Mutate(GrasshopperUi.Scope scope, Seq<DocumentMutation> mutations, DocumentMutationPolicy policy) =>
        UiRail.RunDocumentMutation(scope: scope, op: Op.Of(name: nameof(DocumentOp.Mutate)),
            mutate: (methods, objects, actions) => mutations.TraverseM(m => m.Apply(methods: methods, objects: objects, actions: actions))
                .Map(static receipts => receipts.Fold(initialState: DocumentMutationReceipt.None, f: static (sum, receipt) => sum + receipt)).As()
            )
            .Map(static delta => (DocumentResult)new DocumentResult.MutationResult(Delta: delta));

    private static Fin<Snapshot<DocumentMutationDelta>> RunClipboard(GrasshopperUi.Scope scope, ClipboardOp op) =>
        op.Switch(
            state: scope,
            verbCase: static (s, clipboard) =>
                clipboard.Verb == ClipboardVerb.Copy ? CopyClipboard(scope: s, clipboard: clipboard)
                : clipboard.Verb == ClipboardVerb.PasteGh1Xml ? UiRail.RunDocumentMutation(scope: s, op: Op.Of(name: nameof(ClipboardOp.PasteGh1Xml)), mutate: PasteGh1Clipboard)
                : UiRail.RunDocumentMutation(
                    scope: s,
                    op: Op.Of(name: string.Create(CultureInfo.InvariantCulture, $"Clipboard.{clipboard.Verb}")),
                    mutate: (methods, _, actions) => clipboard.Verb.Run(methods: methods, kind: clipboard.Kind, behaviour: clipboard.Behaviour, actions: actions)
                        .Map(DocumentMutationReceipt.Count)));

    private static Fin<DocumentMutationReceipt> PasteGh1Clipboard(GhDocumentMethods methods, GhObjectList objects, ActionList actions) =>
        Op.Of(name: nameof(ClipboardOp.PasteGh1Xml)).Attempt(body: () => {
            System.Collections.Generic.HashSet<Guid> before = [.. objects.Forwards.Select(static obj => obj.InstanceId)];
            int beforeActions = actions.Count;
            bool pasted = methods.PasteGrasshopper1XmlFromClipboard(actions: actions);
            Seq<IDocumentObject> created = toSeq(objects.Forwards)
                .Filter(obj => obj.InstanceId != Guid.Empty && !before.Contains(obj.InstanceId));
            _ = actions.Count == beforeActions ? created.Iter(obj => actions.Add(new CreateObjectAction(obj: obj))) : unit;
            return DocumentMutationReceipt.Of(
                changed: Math.Max(val1: created.Count, val2: pasted ? 1 : 0),
                created: created.Map(static obj => obj.InstanceId));
        }, what: "DocumentMethods.PasteGrasshopper1XmlFromClipboard");

    private static Fin<Snapshot<DocumentMutationDelta>> CopyClipboard(GrasshopperUi.Scope scope, ClipboardOp.VerbCase clipboard) =>
        from methods in scope.NeedMethods()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        from kind in UiRail.ValidateClipboard(name: nameof(ClipboardOp.Copy), clipboard: clipboard.Kind)
        from copied in Op.Of(name: nameof(ClipboardOp.Copy)).Attempt(body: () => methods.CopySelection(clipboard: kind), what: "DocumentMethods.CopySelection")
        from _ in copied
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(ClipboardOp.Copy)), detail: "copy returned false"))
        let receipt = DocumentMutationReceipt.Count(changed: 0)
        select new Snapshot<DocumentMutationDelta>(
            Payload: new DocumentMutationDelta(
                Changed: receipt.Changed,
                After: UiRail.DocumentSnapshotOf(document: document, objects: objects),
                Created: receipt.Created),
            OwnerId: Some(UiDocumentIdentity.Of(document: document)));

    private static Fin<Seq<DocumentObjectSnapshot>> Find(GhObjectList objects, FindCriterion criterion) =>
        criterion.Switch(
            state: objects,
            // FindNear ranks by bounding-box edge distance (0 inside the box), nearest first, capped at maxResults/maxDistance.
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
                        .Map(UiRail.DocumentObjectSnapshotOf)),
            byActivityCase: static (objs, a) =>
                Fin.Succ(value: toSeq(objs.Forwards)
                    .Filter(o => o.Activity == (a.Enabled ? ObjectActivity.Enabled : ObjectActivity.Disabled))
                    .Map(UiRail.DocumentObjectSnapshotOf)),
            byGroupCase: static (objs, g) =>
                Fin.Succ(value: Optional(objs.FindGroup(instanceId: g.GroupId))
                    .Map(grp => toSeq(grp.ContentIds).Choose(id => Optional(objs.Find(instanceId: id))).Map(UiRail.DocumentObjectSnapshotOf))
                    .IfNone(Seq<DocumentObjectSnapshot>())));

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

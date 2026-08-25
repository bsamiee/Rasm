# [RASM_GRASSHOPPER_DOCUMENT_GRAPH]

`GraphScope` is the graph query-and-wire operator of the GH2 document boundary — ONE scope owner with two gates: `Ask` returns every read intent over the host's own `ObjectList` and `Connectivity` surfaces, and `Mutate` returns every wire and membership mutation over `Grasshopper2.Parameters.Connections`, `ObjectList`, and the `SplitWire` wireless split, with each undo-bearing mutation sealed as one act on `Document/document.md`'s shared gate spine.

Traversal is host-absorbed: reachability walks, cycle detection, bounded path enumeration, and causal ordering are the host's `Connectivity.FindImmediate*`/`FindAll*`/`FindConnections`/`SubsetTopology`/`IsLinear`/`SortCausally`/`WithoutRelays`, and spatial resolution is `ObjectList.SearchUpstream`/`SearchDownstream`/`FindNear`/`FindByInlet`/`FindByOutlet`/`FindByInletOrOutlet`, each reached through typed rows, never re-implemented beside them. Read intent is a `GraphProbe` union returning one `GraphAnswer` union; mutation intent is a `GraphMutation` union whose sealed arms pair the host verb with its `ActionList` record; direction, reach span, roster facet, wire-end role, and bulk-transfer kind are `[SmartEnum<int>]` row families so a new modality is a row, never a sibling method. Membership on the multi-axis knobs rides `CapabilitySet` vocabularies — relay elision on its own `RelayAxis` rows, the window survey on the KERNEL's `PickAxis` rows directly — so no positional bool triple survives at any host call.

## [01]-[INDEX]

- [02]-[QUERY]: `FlowSide` + `GraphReach` + `GraphRoster` + `NearKind` + `GripSearch` + `RelayAxis` + `GripHit` + `LinearVerdict` + `GraphProbe` + `GraphAnswer` — the read-intent union, the direction/reach/roster/search row families, and the one `Ask` gate.
- [03]-[MUTATION]: `WireEndRole` + `WireFreight` + `GraphMutation` — the wire/membership mutation union and the one `Mutate` gate onto the folder's `DocumentGate.Run` spine.

## [02]-[QUERY]

- Owner: `GraphProbe` `[Union]` — the one read-intent vocabulary. `ObjectCase(Guid)`/`ParameterCase(Guid)` resolve identity through `ObjectList.Find`/`FindParameter` into `Option`-carrying answers; `FlowCase(IParameter, FlowSide)` runs the transitive object sweep through `SearchUpstream`/`SearchDownstream` selected by the direction row; `ReachCase(ConnectiveObject, GraphReach)` runs the four `Connectivity` neighbourhood reads through one 4-row family (`ImmediateInputs`/`ImmediateOutputs`/`AllInputs`/`AllOutputs`); `EdgeCase(ConnectiveObject, ConnectiveObject)` enumerates every causal path between the pair through `FindConnections`; `TopologyCase(Seq<IDocumentObject>)` measures the subset's topology class through `SubsetTopology`; `LinearCase(Seq<ConnectiveObject>)` answers chain detection through `IsLinear` as a `LinearVerdict` — the union carries the head/tail witnesses ONLY on the chain case, so a tangled verdict with endpoints is unrepresentable; `CausalCase(Seq<ConnectiveObject>)` orders through `SortCausally`; `RelayFreeCase(CapabilitySet<RelayAxis>)` projects the relay-elided view through `WithoutRelays`, each admitted axis naming the relay arity it removes and the EMPTY set being the un-elided `ObjectList.Connectivity` snapshot itself; `NearCase(PointF, int, float, NearKind)` runs the host's relevance-sorted spatial search through `FindNear<T>`, the kind row closing the type parameter; `GripCase(PointF, GripSearch)` resolves the closest inlet, outlet, or exposed grip through the three `FindBy*` finders into one `GripHit` carrier; `RosterCase(GraphRoster)` sweeps the membership projections — `Forwards`, `Backwards`, `ActiveObjects`, `ExpiredObjects`, `Groups`, `AllWires`, `SelectedWires`, the document-level pin roster, the supported-pin id set, and the `AttributeBounds`/`PivotBounds` pair of graph bounding envelopes — through one 11-row family.
- Owner: `GripHit` — ONE grip-probe carrier: its resolved pin beside its in-range sides as `CapabilitySet<PinSide>` (`Components/ports.md`'s side vocabulary), so the occlusion-split finders and the exposure finder settle ONE shape — side-specific finders mint singleton sets, and the exposure finder folds the host's two range booleans into membership. `LinearVerdict` `[Union]` — `ChainCase(Head, Tail)` carries the endpoint witnesses, `TangledCase` carries nothing. `GraphAnswer` `[Union]` closes the result space: `ObjectCase`/`ParameterCase` (`Option` payloads), `ObjectsCase`/`NodesCase`/`GroupsCase`/`WiresCase`/`GlobalPinsCase`/`IdentitiesCase` (`Seq` payloads), `BoundsCase` (a `RectangleF` envelope), `WebCase` (the `WithoutRelays` view), `TopologyCase` (a `GraphTopology` verdict), `PathsCase` (route sequences), `LinearCase(LinearVerdict)`, `GripCase(Option<GripHit>)`.
- Entry: `GraphScope.Ask(GraphProbe probe, Option<HostDocument> graph = default, Op? key = null)` → `Fin<GraphAnswer>` — the one read gate; identity lookup, neighbourhood reach, spatial search, topology, and roster sweeps are cases, never sibling methods; reads return direct answers, so the gate rides `DocumentGate.Resolve`'s marshal without a gauge.
- Law: traversal is host-absorbed — a reachability walk, cycle probe, path enumeration, causal sort, or nearest-object scan written locally beside `Connectivity` and `ObjectList` is the re-derivation defect this page kills; the kernel contributes no graph algorithm here because the host owns its own graph, and kernel graph owners serve host-neutral geometry, not the live document.
- Law: the spatial and grip finders live on `ObjectList`, never on `Connectivity` — `Connectivity` is the immutable connection snapshot and carries no coordinate, so a pick, hover, or drop probe reaches the object list directly and `ObjectList.Connectivity` is where the two meet.
- Law: the grip family splits on OCCLUSION, not on side alone — `FindByInlet`/`FindByOutlet` answer the closest grip even where another object covers it (the drag-target read), while `FindByInletOrOutlet` refuses an occluded grip and reports which side fell within range (the hover read); a `null` from any of the three is absence, projected at the finder into the one `Option<GripHit>` carrier.
- Law: `FindNear<T>` filters by type INSIDE its bounded search, so the type parameter is load-bearing — filtering a `FindNear<IDocumentObject>` result afterwards returns fewer rows than the caller asked for whenever a nearer foreign object consumed a slot; the `NearKind` rows therefore close the parameter at the row, never at the consumer.
- Law: answers are values — an `Option`-carrying case states absence without a null, a `Seq`-carrying case is a detached immutable projection, and a `WebCase` hands the host's own `Connectivity` view for further row-driven reads; no answer carries a mutable host collection.
- Boundary: window selection mutates selection state and rides `[03]`'s mutation union; whole-graph selection sweeps are `Document/document.md`'s `SelectionSweep`; wire picking, routing, and drawing are `Canvas/wires.md`'s visual owner, which composes `GripCase` for its own hit reads rather than re-scanning attributes.
- Packages: Grasshopper2 (`ObjectList`, `Connectivity`, `ConnectiveObject`, `GroupObject`, `GraphTopology`, `WireEnds`, `IParameter`, `IPin`, `IDocumentObject`), Eto (`PointF`, `RectangleF`), `Components/ports.md` (`PinSide`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new host reach is one `GraphReach` row; a new membership projection is one `GraphRoster` row; a new searched type is one `NearKind` row; a new elision axis is one `RelayAxis` row; a new read intent is one `GraphProbe` case whose arm breaks the gate's total `Switch` loudly.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Grasshopper.Components;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FlowSide {
    public static readonly FlowSide Upstream = new(
        key: 0,
        search: static (objects, pin) => toSeq(objects.SearchUpstream(pin)),
        prune: static (pin, kept, actions) => Connections.DisconnectAllInputsExcept(pin, kept, actions));
    public static readonly FlowSide Downstream = new(
        key: 1,
        search: static (objects, pin) => toSeq(objects.SearchDownstream(pin)),
        prune: static (pin, kept, actions) => Connections.DisconnectAllOutputsExcept(pin, kept, actions));
    [UseDelegateFromConstructor] internal partial Seq<IDocumentObject> Search(ObjectList objects, IParameter pin);
    [UseDelegateFromConstructor] internal partial int Prune(IParameter pin, HashSet<Guid> kept, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class NearKind {
    public static readonly NearKind Objects = new(key: 0, find: static (objects, locus, cap, span) =>
        new GraphAnswer.ObjectsCase(Members: toSeq(objects.FindNear<IDocumentObject>(locus, cap, span))));
    public static readonly NearKind Groups = new(key: 1, find: static (objects, locus, cap, span) =>
        new GraphAnswer.GroupsCase(Members: toSeq(objects.FindNear<GroupObject>(locus, cap, span))));
    [UseDelegateFromConstructor] internal partial GraphAnswer Find(ObjectList objects, PointF locus, int cap, float span);
}

[SmartEnum<int>]
public sealed partial class GripSearch {
    public static readonly GripSearch Inlet = new(key: 0, probe: static (objects, at) =>
        new GraphAnswer.GripCase(Hit: Optional(objects.FindByInlet(at))
            .Map(pin => new GripHit(Pin: pin, Sides: CapabilitySet<PinSide>.Of(PinSide.Inlet)))));
    public static readonly GripSearch Outlet = new(key: 1, probe: static (objects, at) =>
        new GraphAnswer.GripCase(Hit: Optional(objects.FindByOutlet(at))
            .Map(pin => new GripHit(Pin: pin, Sides: CapabilitySet<PinSide>.Of(PinSide.Outlet)))));
    public static readonly GripSearch Exposed = new(key: 2, probe: static (objects, at) =>
        Exposure(hit: objects.FindByInletOrOutlet(at)));
    [UseDelegateFromConstructor] internal partial GraphAnswer Probe(ObjectList objects, PointF at);
    private static GraphAnswer Exposure((IParameter parameter, bool inletWithinRange, bool outletWithinRange) hit) =>
        new GraphAnswer.GripCase(Hit: Optional(hit.parameter).Map(pin => new GripHit(
            Pin: pin,
            Sides: CapabilitySet<PinSide>.Of(
                [.. Seq(hit.inletWithinRange ? Some(PinSide.Inlet) : None,
                        hit.outletWithinRange ? Some(PinSide.Outlet) : None).Somes()]))));
}

[SmartEnum<int>]
public sealed partial class GraphReach {
    public static readonly GraphReach ImmediateInputs = new(key: 0, find: static (web, node) => toSeq(web.FindImmediateInputs(node)));
    public static readonly GraphReach ImmediateOutputs = new(key: 1, find: static (web, node) => toSeq(web.FindImmediateOutputs(node)));
    public static readonly GraphReach AllInputs = new(key: 2, find: static (web, node) => toSeq(web.FindAllInputs(node)));
    public static readonly GraphReach AllOutputs = new(key: 3, find: static (web, node) => toSeq(web.FindAllOutputs(node)));
    [UseDelegateFromConstructor] internal partial Seq<ConnectiveObject> Find(Connectivity web, ConnectiveObject node);
}

[SmartEnum<int>]
public sealed partial class GraphRoster {
    public static readonly GraphRoster Forwards = new(key: 0, project: static objects => new GraphAnswer.ObjectsCase(Members: toSeq(objects.Forwards)));
    public static readonly GraphRoster Backwards = new(key: 1, project: static objects => new GraphAnswer.ObjectsCase(Members: toSeq(objects.Backwards)));
    public static readonly GraphRoster ActiveObjects = new(key: 2, project: static objects => new GraphAnswer.ObjectsCase(Members: toSeq(objects.ActiveObjects)));
    public static readonly GraphRoster ExpiredObjects = new(key: 3, project: static objects => new GraphAnswer.ObjectsCase(Members: toSeq(objects.ExpiredObjects)));
    public static readonly GraphRoster Groups = new(key: 4, project: static objects => new GraphAnswer.GroupsCase(Members: toSeq(objects.Groups)));
    public static readonly GraphRoster AllWires = new(key: 5, project: static objects => new GraphAnswer.WiresCase(Members: toSeq(objects.AllWires)));
    public static readonly GraphRoster SelectedWires = new(key: 6, project: static objects => new GraphAnswer.WiresCase(Members: toSeq(objects.SelectedWires)));
    public static readonly GraphRoster GlobalPins = new(key: 7, project: static objects => new GraphAnswer.GlobalPinsCase(Members: toSeq(objects.Pins)));
    public static readonly GraphRoster SupportedPins = new(key: 8, project: static objects => new GraphAnswer.IdentitiesCase(Ids: toSeq(objects.SupportedPins)));
    public static readonly GraphRoster AttributeBounds = new(key: 9, project: static objects => new GraphAnswer.BoundsCase(Envelope: objects.AttributeBounds));
    public static readonly GraphRoster PivotBounds = new(key: 10, project: static objects => new GraphAnswer.BoundsCase(Envelope: objects.PivotBounds));
    [UseDelegateFromConstructor] internal partial GraphAnswer Project(ObjectList objects);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RelayAxis : ICapability<RelayAxis> {
    public static readonly RelayAxis Dangling = new(key: "dangling");
    public static readonly RelayAxis Simple = new(key: "simple");
    public static readonly RelayAxis Complex = new(key: "complex");
    public static CapabilityLaw<RelayAxis> Law => CapabilityLaw<RelayAxis>.Open;
}

[Union]
[GenerateUnionOps]
public abstract partial record GraphProbe {
    private GraphProbe() { }
    public sealed record ObjectCase(Guid Id) : GraphProbe;
    public sealed record ParameterCase(Guid Id) : GraphProbe;
    public sealed record FlowCase(IParameter Pin, FlowSide Side) : GraphProbe;
    public sealed record ReachCase(ConnectiveObject Node, GraphReach Reach) : GraphProbe;
    public sealed record EdgeCase(ConnectiveObject From, ConnectiveObject To) : GraphProbe;
    public sealed record TopologyCase(Seq<IDocumentObject> Subset) : GraphProbe;
    public sealed record LinearCase(Seq<ConnectiveObject> Nodes) : GraphProbe;
    public sealed record CausalCase(Seq<ConnectiveObject> Nodes) : GraphProbe;
    public sealed record RelayFreeCase(CapabilitySet<RelayAxis> Elide) : GraphProbe;
    public sealed record NearCase(PointF Locus, int MaxResults, float MaxDistance, NearKind Kind) : GraphProbe;
    public sealed record GripCase(PointF At, GripSearch Search) : GraphProbe;
    public sealed record RosterCase(GraphRoster Roster) : GraphProbe;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GripHit(IParameter Pin, CapabilitySet<PinSide> Sides);

[Union]
public abstract partial record LinearVerdict {
    private LinearVerdict() { }
    public sealed record ChainCase(ConnectiveObject Head, ConnectiveObject Tail) : LinearVerdict;
    public sealed record TangledCase : LinearVerdict;
}

[Union]
public abstract partial record GraphAnswer {
    private GraphAnswer() { }
    public sealed record ObjectCase(Option<IDocumentObject> Subject) : GraphAnswer;
    public sealed record ParameterCase(Option<IParameter> Pin) : GraphAnswer;
    public sealed record ObjectsCase(Seq<IDocumentObject> Members) : GraphAnswer;
    public sealed record NodesCase(Seq<ConnectiveObject> Members) : GraphAnswer;
    public sealed record GroupsCase(Seq<GroupObject> Members) : GraphAnswer;
    public sealed record WiresCase(Seq<WireEnds> Members) : GraphAnswer;
    public sealed record GlobalPinsCase(Seq<IPin> Members) : GraphAnswer;
    public sealed record IdentitiesCase(Seq<Guid> Ids) : GraphAnswer;
    public sealed record BoundsCase(RectangleF Envelope) : GraphAnswer;
    public sealed record WebCase(Connectivity Web) : GraphAnswer;
    public sealed record TopologyCase(GraphTopology Class) : GraphAnswer;
    public sealed record PathsCase(Seq<Seq<ConnectiveObject>> Routes) : GraphAnswer;
    public sealed record LinearCase(LinearVerdict Verdict) : GraphAnswer;
    public sealed record GripCase(Option<GripHit> Hit) : GraphAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static partial class GraphScope {
    public static Fin<GraphAnswer> Ask(GraphProbe probe, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(probe).ToFin(active.InvalidInput())
            .Bind(valid => DocumentGate.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Objects: document.Objects),
                objectCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.ObjectCase(Subject: Optional(frame.Objects.Find(c.Id))))),
                parameterCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.ParameterCase(Pin: Optional(frame.Objects.FindParameter(c.Id))))),
                flowCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.ObjectsCase(Members: c.Side.Search(objects: frame.Objects, pin: c.Pin)))),
                reachCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.NodesCase(Members: c.Reach.Find(web: frame.Objects.Connectivity, node: c.Node)))),
                edgeCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.PathsCase(
                        Routes: toSeq(frame.Objects.Connectivity.FindConnections(c.From, c.To)).Map(toSeq)))),
                topologyCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.TopologyCase(
                        Class: frame.Objects.Connectivity.SubsetTopology(c.Subset)))),
                linearCase: static (frame, c) => frame.Key.Catch(body: () => {
                    bool linear = frame.Objects.Connectivity.IsLinear(c.Nodes, out ConnectiveObject head, out ConnectiveObject tail);
                    return Fin.Succ<GraphAnswer>(new GraphAnswer.LinearCase(Verdict: linear
                        ? new LinearVerdict.ChainCase(Head: head, Tail: tail)
                        : new LinearVerdict.TangledCase()));
                }),
                causalCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.NodesCase(Members: toSeq(frame.Objects.Connectivity.SortCausally(c.Nodes.ToArray()))))),
                relayFreeCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.WebCase(Web: frame.Objects.Connectivity.WithoutRelays(
                        dangling: c.Elide.Admits(RelayAxis.Dangling),
                        simple: c.Elide.Admits(RelayAxis.Simple),
                        complex: c.Elide.Admits(RelayAxis.Complex))))),
                nearCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ(c.Kind.Find(objects: frame.Objects, locus: c.Locus, cap: c.MaxResults, span: c.MaxDistance))),
                gripCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ(c.Search.Probe(objects: frame.Objects, at: c.At))),
                rosterCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ(c.Roster.Project(objects: frame.Objects))))));
    }
}
```

## [03]-[MUTATION]

- Owner: `GraphMutation` `[Union]` `[GenerateUnionOps]` — the one wire-and-membership mutation vocabulary. `LinkCase(IParameter, IParameter)`/`UnlinkCase(IParameter, IParameter)` add and remove one wire through `Connections.Connect`/`Disconnect`; `PruneCase(IParameter, FlowSide, Seq<Guid>)` clears one side but a kept set through the direction row's `DisconnectAll*Except` column — an empty kept set is the full-side clear, so the bare disconnect-all verbs are the empty shape of one case; `RewireCase(WireEndRole, IParameter, IParameter, IParameter)` re-points a wire endpoint through `ReplaceSource`/`ReplaceTarget` selected by the end-role row; `SwapCase(IParameter, IParameter, IParameter, IParameter)` exchanges the sources feeding two targets through `SwapSources`; `BypassCase(IParameter, IParameter, IParameter)` cuts an intermediate through `CutOutMiddleMan`; `TransferCase(WireFreight, IParameter, IParameter)` hauls a whole wire set through `CopyAllInputs`/`MigrateAllOutputs` as freight rows; `SplitCase(IParameter, IParameter, string, PointF)` splits a wire into its wireless `Shout`/`Listen` pair through `DocumentMethods.SplitWire`, returning `WirelessPair`; `RemapCase(Option<HashMap<Guid, Guid>>)` discriminates on payload shape — `None` remints every id through `ChangeAllIds` and answers the host's own correspondence, `Some` applies the explicit map through `ApplyIdMap`; `PinCase(IPin)`/`RepairCase(PinRepair)` own document-pin membership and repair, the repair arm surfacing the host's per-pin report as `PinRepairRow` rows; `ExpireCase` expires the whole membership through `ExpireAll`; `WindowCase(WindowSelection, SelectionMode, CapabilitySet<PickAxis>)` applies rectangle selection through `WindowSelect` over the KERNEL pick vocabulary — the same `Foreground`/`Background`/`Wires` axes the canvas pick gate reads, so no folder-local survey triple exists.
- Entry: `GraphScope.Mutate(VerbNoun label, GraphMutation op, Option<HostDocument> graph = default, Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Op? key = null)` → `Fin<GateOutcome>` — the one mutation gate on the shared `DocumentGate.Run` spine.
- Law: the `document.mutate` veto fires HERE exactly as at `Document/document.md`'s `Transact` — the two undo-sealed gates share the fire-site row, a `Fail` verdict refuses with nothing mutated, and an absent rail dispatches ungoverned.
- Law: mutation and undo are one act — every `ActionList`-bearing arm mints one list, runs its host verb, and seals through `Document/history.md`'s `HistoryLedger.Seal` under the caller's `VerbNoun`; `WindowCase`, `RemapCase`, `PinCase`, `RepairCase`, and `ExpireCase` are the host's own unsealed membership verbs.
- Law: wire mutation writes through `Grasshopper2.Parameters.Connections`, and reads never mutate: a probe from `[02]` inside a mutation arm is composition, never a second traversal implementation.
- Law: every arm reports what the host answered — the wireless pair, the id correspondence `ChangeAllIds` returns, the admitted flag `AddGlobalPin` returns, and the `(method, pin, cushion)` rows `RepairPins` returns each land as the arm's `GateOutcome`.
- Law: `WindowSelect`'s `SelectionResult` stays inside the arm — it is the host's mutable pick accumulator, not a value, so the arm returns `SettledCase` rather than leaking a live accumulator through the answer.
- Law: cross-document adoption has no seam here — `ObjectList.Transfer` is the host's own private internal move, so a foreign object enters the live graph through the clipboard round-trip (`Document/document.md`'s `PasteCase`) or `MigrateObjects`, and a case naming an uncallable member is the deleted form.
- Law: the two replace verbs disagree on parameter order — `ReplaceSource(oldSource, newSource, target)` against `ReplaceTarget(source, oldTarget, newTarget)` — so the `WireEndRole` rows bind by NAME and the anchor/retired/replacement vocabulary is the one order every call site spells.
- Boundary: `Shout`/`Listen` as canvas objects — their attributes, painting, and interaction — are `Canvas/*` and `Components/objects.md` territory; this page owns only their minting at the split seam. Relay elision on reads is `[02]`'s `RelayFreeCase`; `GateOutcome`, `WirelessPair`, and `PinRepairRow` are `Document/document.md`'s.
- Packages: Grasshopper2 (`Connections`, `ObjectList.ChangeAllIds`/`ApplyIdMap`/`AddGlobalPin`/`RepairPins`/`ExpireAll`/`WindowSelect`, `DocumentMethods.SplitWire`, `Shout`, `Listen`, `PinRepair`, `WindowSelection`, `SelectionMode`), Eto (`PointF`), `Rasm.Interaction` (`PickAxis`), `Shell/hooks.md` (`GrasshopperPoint`, `HookSignal`, `HookScope`), `Document/document.md` (`DocumentGate`, `GateOutcome`, `WirelessPair`, `PinRepairRow`), `Document/history.md` (`HistoryLedger.Seal`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new wire verb is one `GraphMutation` case; a new bulk-transfer kind is one `WireFreight` row; a new endpoint role is one `WireEndRole` row; a new survey axis is one kernel `PickAxis` row — the gate never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WireEndRole {
    public static readonly WireEndRole SourceEnd = new(key: 0, replace: static (anchor, retired, replacement, actions) =>
        Connections.ReplaceSource(oldSource: retired, newSource: replacement, target: anchor, undo: actions));
    public static readonly WireEndRole TargetEnd = new(key: 1, replace: static (anchor, retired, replacement, actions) =>
        Connections.ReplaceTarget(source: anchor, oldTarget: retired, newTarget: replacement, undo: actions));
    [UseDelegateFromConstructor] internal partial bool Replace(IParameter anchor, IParameter retired, IParameter replacement, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class WireFreight {
    public static readonly WireFreight CopyInputs = new(key: 0, haul: static (from, to, actions) => Connections.CopyAllInputs(from, to, actions));
    public static readonly WireFreight MigrateOutputs = new(key: 1, haul: static (from, to, actions) => Connections.MigrateAllOutputs(from, to, actions));
    [UseDelegateFromConstructor] internal partial int Haul(IParameter from, IParameter to, ActionList actions);
}

[Union]
[GenerateUnionOps]
public abstract partial record GraphMutation {
    private GraphMutation() { }
    public sealed record LinkCase(IParameter Source, IParameter Target) : GraphMutation;
    public sealed record UnlinkCase(IParameter Source, IParameter Target) : GraphMutation;
    public sealed record PruneCase(IParameter Pin, FlowSide Side, Seq<Guid> Kept) : GraphMutation;
    public sealed record RewireCase(WireEndRole Role, IParameter Anchor, IParameter Retired, IParameter Replacement) : GraphMutation;
    public sealed record SwapCase(IParameter SourceA, IParameter SourceB, IParameter TargetA, IParameter TargetB) : GraphMutation;
    public sealed record BypassCase(IParameter Source, IParameter Middle, IParameter Target) : GraphMutation;
    public sealed record TransferCase(WireFreight Freight, IParameter From, IParameter To) : GraphMutation;
    public sealed record SplitCase(IParameter Source, IParameter Target, string Name, PointF At) : GraphMutation;
    public sealed record RemapCase(Option<HashMap<Guid, Guid>> Map) : GraphMutation;
    public sealed record PinCase(IPin Pin) : GraphMutation;
    public sealed record RepairCase(PinRepair Repair) : GraphMutation;
    public sealed record ExpireCase : GraphMutation;
    public sealed record WindowCase(WindowSelection Frame, SelectionMode Mode, CapabilitySet<PickAxis> Survey) : GraphMutation;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class GraphScope {
    public static Fin<GateOutcome> Mutate(
        VerbNoun label,
        GraphMutation op,
        Option<HostDocument> graph = default,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default,
        Op? key = null) {
        Op active = key.OrDefault();
        return Optional(op).ToFin(active.InvalidInput())
            .Bind(valid => DocumentGate.Run(
                graph: graph, key: active,
                body: document => Vetoed(rail: rail, op: valid.SelfOp, document: document, key: active)
                    .Bind(_ => valid.Switch(
                state: (Key: active, Graph: document, Label: label),
                linkCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.ChangedCase(Changed: Connections.Connect(c.Source, c.Target, actions))),
                unlinkCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.ChangedCase(Changed: Connections.Disconnect(c.Source, c.Target, actions))),
                pruneCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.CountCase(Touched: c.Side.Prune(pin: c.Pin, kept: [.. c.Kept], actions: actions))),
                rewireCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.ChangedCase(Changed: c.Role.Replace(anchor: c.Anchor, retired: c.Retired, replacement: c.Replacement, actions: actions))),
                swapCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.ChangedCase(Changed: Connections.SwapSources(
                        sourceA: c.SourceA, sourceB: c.SourceB, targetA: c.TargetA, targetB: c.TargetB, undo: actions))),
                bypassCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.ChangedCase(Changed: Connections.CutOutMiddleMan(c.Source, c.Middle, c.Target, actions))),
                transferCase: static (frame, c) => Sealed(frame, actions =>
                    new GateOutcome.CountCase(Touched: c.Freight.Haul(from: c.From, to: c.To, actions: actions))),
                splitCase: static (frame, c) => frame.Key.Catch(body: () => {
                    ActionList actions = new();
                    bool split = frame.Graph.Methods.SplitWire(c.Source, c.Target, c.Name, c.At, out Shout shout, out Listen listen, actions);
                    return guard(split, (Error)frame.Key.InvalidResult()).ToFin()
                        .Bind(_ => HistoryLedger.Seal(ledger: frame.Graph.Undo, actions: actions, label: frame.Label, key: frame.Key))
                        .Map(_ => (GateOutcome)new GateOutcome.WirelessCase(
                            Pair: new WirelessPair(Shout: shout.InstanceId, Listen: listen.InstanceId)));
                }),
                remapCase: static (frame, c) => Free(frame.Key, () => c.Map.Match(
                    Some: map => (Op.Side(action: () => frame.Graph.Objects.ApplyIdMap(
                                      map.AsIterable().ToDictionary(static row => row.Key, static row => row.Value))),
                                  (GateOutcome)new GateOutcome.SettledCase()).Item2,
                    None: () => new GateOutcome.RemapCase(Correspondence: toHashMap(
                        frame.Graph.Objects.ChangeAllIds().Select(static row => (row.Key, row.Value)))))),
                pinCase: static (frame, c) => Free(frame.Key,
                    () => new GateOutcome.ChangedCase(Changed: frame.Graph.Objects.AddGlobalPin(c.Pin))),
                repairCase: static (frame, c) => Free(frame.Key,
                    () => new GateOutcome.RepairCase(Rows: toSeq(frame.Graph.Objects.RepairPins(c.Repair))
                        .Map(static row => new PinRepairRow(Method: row.method, Pin: row.pin, Cushion: row.cushion)))),
                expireCase: static (frame, c) => Free(frame.Key,
                    () => (Op.Side(action: () => frame.Graph.Objects.ExpireAll()), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                windowCase: static (frame, c) => Free(frame.Key,
                    () => (Op.Side(action: () => frame.Graph.Objects.WindowSelect(
                        c.Frame,
                        c.Mode,
                        c.Survey.Admits(PickAxis.Foreground),
                        c.Survey.Admits(PickAxis.Background),
                        c.Survey.Admits(PickAxis.Wires))), (GateOutcome)new GateOutcome.SettledCase()).Item2))));
    }

    private static Fin<Unit> Vetoed(
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail, Op op, HostDocument document, Op key) =>
        rail.Match(
            Some: live => live.Fire(
                    at: GrasshopperPoint.DocumentMutate,
                    fact: new HookSignal.IntentCase(Operation: op, DocumentId: Some(document.Identity)),
                    key: key)
                .Map(static _ => unit),
            None: () => Fin.Succ(unit));

    private static Fin<GateOutcome> Free(Op key, Func<GateOutcome> settle) =>
        key.Catch(body: () => Fin.Succ(settle()));

    private static Fin<GateOutcome> Sealed(
        (Op Key, HostDocument Graph, VerbNoun Label) frame, Func<ActionList, GateOutcome> act) =>
        frame.Key.Catch(body: () => {
            ActionList actions = new();
            GateOutcome outcome = act(arg: actions);
            return HistoryLedger.Seal(ledger: frame.Graph.Undo, actions: actions, label: frame.Label, key: frame.Key)
                .Map(_ => outcome);
        });
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                       | [RAIL]                         | [CASES] |
| :-----: | :------------------ | :---------------------------- | :----------------------------- | :-----: |
|  [01]   | flow direction      | `FlowSide`                    | `Search`/`Prune` (internal)    |    2    |
|  [02]   | neighbourhood reach | `GraphReach`                  | `Find → Seq<ConnectiveObject>` |    4    |
|  [03]   | membership roster   | `GraphRoster`                 | `Project → GraphAnswer`        |   11    |
|  [04]   | spatial search      | `NearKind` + `GripSearch`     | `Find`/`Probe → GraphAnswer`   |  2 + 3  |
|  [05]   | relay elision       | `RelayAxis`                   | `CapabilitySet` membership     |    3    |
|  [06]   | read intent         | `GraphProbe` + `GraphAnswer`  | `Ask → Fin<GraphAnswer>`       | 12 + 14 |
|  [07]   | wire end role       | `WireEndRole` + `WireFreight` | `Replace`/`Haul` (internal)    |  2 + 2  |
|  [08]   | mutation intent     | `GraphMutation`               | `Mutate → Fin<GateOutcome>`    |   13    |

`DocumentGate.Run`/`Resolve`, `GateOutcome`, `WirelessPair`, `PinRepairRow`, `HistoryLedger.Seal`, kernel `PickAxis`, `Components/ports.md`'s `PinSide`, `Op`, and `ValidityClaim` are composed upstream owners; local graph algorithms and folder-local survey/elision flag enums have no home here.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

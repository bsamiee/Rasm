# [RASM_GRASSHOPPER_DOCUMENT_GRAPH]

`GraphScope` is the graph query-and-wire operator of the GH2 document boundary — ONE scope owner with two gates: `Ask` settles every read intent over the host's own `ObjectList` and `Connectivity` surfaces, and `Mutate` settles every wire and membership mutation over `Grasshopper2.Parameters.Connections`, `ObjectList`, and the `SplitWire` wireless split, each mutation sealed into the undo ledger as one act.

Traversal is host-absorbed: reachability walks, cycle detection, bounded path enumeration, and causal ordering are the host's `Connectivity.FindImmediate*`/`FindAll*`/`FindConnections`/`SubsetTopology`/`IsLinear`/`SortCausally`/`WithoutRelays` and `ObjectList.SearchUpstream`/`SearchDownstream`, reached through typed rows, never re-implemented beside them. Read intent is a `GraphProbe` union settled into one `GraphAnswer` union; mutation intent is a `GraphMutation` union whose sealed arms pair the host verb with its `ActionList` record; direction, reach span, roster facet, wire-end role, and bulk-transfer kind are `[SmartEnum<int>]` row families so a new modality is a row, never a sibling method.

## [01]-[INDEX]

- [02]-[QUERY]: `FlowSide` + `GraphReach` + `GraphRoster` + `GraphProbe` + `GraphAnswer` — the read-intent union, the direction/reach/roster row families, and the one `Ask` gate.
- [03]-[MUTATION]: `WireEndRole` + `WireFreight` + `GraphMutation` + `WirelessPair` + `MutationReceipt` — the wire/membership mutation union, the sealed settlement law, and the one `Mutate` gate.

## [02]-[QUERY]

- Owner: `GraphProbe` `[Union]` — the one read-intent vocabulary. `ObjectCase(Guid)`/`ParameterCase(Guid)` resolve identity through `ObjectList.Find`/`FindParameter` into `Option`-carrying answers; `FlowCase(IParameter, FlowSide)` runs the transitive parameter sweep through `SearchUpstream`/`SearchDownstream` selected by the direction row; `ReachCase(ConnectiveObject, GraphReach)` runs the four `Connectivity` neighbourhood reads through one 4-row family (`ImmediateInputs`/`ImmediateOutputs`/`AllInputs`/`AllOutputs`); `EdgeCase(ConnectiveObject, ConnectiveObject)` looks up the connecting wires through `FindConnections`; `TopologyCase(Seq<IDocumentObject>)` projects the subgraph view through `SubsetTopology`; `LinearCase(Seq<ConnectiveObject>)` answers chain detection through `IsLinear` with its head/tail witnesses as `Option` values — a non-linear verdict carries no endpoints; `CausalCase(Seq<ConnectiveObject>)` orders through `SortCausally`; `RelayFreeCase(bool, bool, bool)` projects the relay-elided view through `WithoutRelays`; `RosterCase(GraphRoster)` sweeps the membership projections — `Forwards`, `Backwards`, `ActiveObjects`, `ExpiredObjects`, `Groups`, `AllWires`, `SelectedWires`, the global-pin roster, and the `AttributeBounds`/`PivotBounds` graph-envelope pair — through one 10-row family. `GraphAnswer` `[Union]` is the closed result: `ObjectCase`/`ParameterCase` (`Option` payloads), `ObjectsCase`/`NodesCase`/`PinsCase`/`GroupsCase`/`WiresCase`/`GlobalPinsCase` (`Seq` payloads), `BoundsCase` (a `RectangleF` envelope), `WebCase` (a `Connectivity` view), `LinearCase` (verdict with `Option` endpoint witnesses).
- Entry: `GraphScope.Ask(GraphProbe probe, Option<HostDocument> graph = default, Op? key = null)` → `Fin<GraphAnswer>` — the one read gate; identity lookup, neighbourhood reach, topology, and roster sweeps are cases, never sibling methods.
- Law: traversal is host-absorbed — a reachability walk, cycle probe, path enumeration, or causal sort written locally beside `Connectivity` is the re-derivation defect this page kills; the kernel contributes no graph algorithm here because the host owns its own graph, and kernel graph owners serve host-neutral geometry, not the live document.
- Law: answers are values — an `Option`-carrying case states absence without a null, a `Seq`-carrying case is a detached immutable projection, and a `WebCase` hands the host's own `Connectivity` view for further row-driven reads; no answer carries a mutable host collection.
- RESEARCH: `ObjectList.FindNear`/`FindByInlet`/`FindByOutlet` are catalog-unverified — the near/inlet/outlet probe cases land when the decompile confirms them; the `ObjectList.Pins` member spelling the global-pin roster row assumes, the element carriers of the roster properties, the `SortCausally`/`WithoutRelays`/`SubsetTopology`/`FindConnections` return spellings, the `WithoutRelays` flag semantics, and the `RectangleF` carrier the bounds rows assume re-verify at the same pass.
- Boundary: window selection mutates selection state and rides `[03]`'s mutation union; whole-graph selection sweeps are `Document/document.md`'s `SelectionSweep`; wire picking, routing, and drawing are `Canvas/wires.md`'s visual owner.
- Packages: Grasshopper2 (`ObjectList`, `Connectivity`, `ConnectiveObject`, `GroupObject`, `WireEnds`, `IParameter`, `IPin`), Eto (`RectangleF`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new host reach is one `GraphReach` row; a new membership projection is one `GraphRoster` row; a new read intent is one `GraphProbe` case whose arm breaks the gate's total `Switch` loudly.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Undo;
using Rasm.Csp;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
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
    [UseDelegateFromConstructor] internal partial Seq<IParameter> Search(ObjectList objects, IParameter pin);
    [UseDelegateFromConstructor] internal partial void Prune(IParameter pin, HashSet<Guid> kept, ActionList actions);
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
    public static readonly GraphRoster AttributeBounds = new(key: 8, project: static objects => new GraphAnswer.BoundsCase(Envelope: objects.AttributeBounds));
    public static readonly GraphRoster PivotBounds = new(key: 9, project: static objects => new GraphAnswer.BoundsCase(Envelope: objects.PivotBounds));
    [UseDelegateFromConstructor] internal partial GraphAnswer Project(ObjectList objects);
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
    public sealed record RelayFreeCase(bool KeepPins, bool KeepGroups, bool KeepDormant) : GraphProbe;
    public sealed record RosterCase(GraphRoster Roster) : GraphProbe;
}

[Union]
public abstract partial record GraphAnswer {
    private GraphAnswer() { }
    public sealed record ObjectCase(Option<IDocumentObject> Subject) : GraphAnswer;
    public sealed record ParameterCase(Option<IParameter> Pin) : GraphAnswer;
    public sealed record ObjectsCase(Seq<IDocumentObject> Members) : GraphAnswer;
    public sealed record NodesCase(Seq<ConnectiveObject> Members) : GraphAnswer;
    public sealed record PinsCase(Seq<IParameter> Members) : GraphAnswer;
    public sealed record GroupsCase(Seq<GroupObject> Members) : GraphAnswer;
    public sealed record WiresCase(Seq<WireEnds> Members) : GraphAnswer;
    public sealed record GlobalPinsCase(Seq<IPin> Members) : GraphAnswer;
    public sealed record BoundsCase(RectangleF Envelope) : GraphAnswer;
    public sealed record WebCase(Connectivity Web) : GraphAnswer;
    public sealed record LinearCase(bool Linear, Option<ConnectiveObject> Head, Option<ConnectiveObject> Tail) : GraphAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static partial class GraphScope {
    public static Fin<GraphAnswer> Ask(GraphProbe probe, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(probe).ToFin(active.InvalidInput())
            .Bind(valid => DocumentScope.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Objects: document.Objects),
                objectCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.ObjectCase(Subject: Optional(frame.Objects.Find(c.Id))))),
                parameterCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.ParameterCase(Pin: Optional(frame.Objects.FindParameter(c.Id))))),
                flowCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.PinsCase(Members: c.Side.Search(objects: frame.Objects, pin: c.Pin)))),
                reachCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.NodesCase(Members: c.Reach.Find(web: frame.Objects.Connectivity, node: c.Node)))),
                edgeCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.WiresCase(Members: toSeq(frame.Objects.Connectivity.FindConnections(c.From, c.To))))),
                topologyCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.WebCase(Web: frame.Objects.Connectivity.SubsetTopology(c.Subset)))),
                linearCase: static (frame, c) => frame.Key.Catch(body: () => {
                    bool linear = frame.Objects.Connectivity.IsLinear(c.Nodes, out ConnectiveObject head, out ConnectiveObject tail);
                    return Fin.Succ<GraphAnswer>(new GraphAnswer.LinearCase(Linear: linear, Head: Optional(head), Tail: Optional(tail)));
                }),
                causalCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.NodesCase(Members: toSeq(frame.Objects.Connectivity.SortCausally(c.Nodes.ToArray()))))),
                relayFreeCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ<GraphAnswer>(new GraphAnswer.WebCase(Web: frame.Objects.Connectivity.WithoutRelays(c.KeepPins, c.KeepGroups, c.KeepDormant)))),
                rosterCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ(c.Roster.Project(objects: frame.Objects))))));
    }
}
```

## [03]-[MUTATION]

- Owner: `GraphMutation` `[Union]` `[GenerateUnionOps]` — the one wire-and-membership mutation vocabulary. `LinkCase(IParameter, IParameter)`/`UnlinkCase(IParameter, IParameter)` add and remove one wire through `Connections.Connect`/`Disconnect`; `PruneCase(IParameter, FlowSide, Seq<Guid>)` clears one side but a kept set through the direction row's `DisconnectAll*Except` column — an empty kept set is the full-side clear, so the bare disconnect-all verbs are the empty shape of one case; `RewireCase(WireEndRole, IParameter, IParameter, IParameter)` re-points a wire endpoint through `ReplaceSource`/`ReplaceTarget` selected by the end-role row; `BypassCase(IParameter, IParameter, IParameter)` cuts an intermediate through `CutOutMiddleMan`; `TransferCase(WireFreight, IParameter, IParameter)` hauls a whole wire set through `CopyAllInputs`/`MigrateAllOutputs` as freight rows; `SplitCase(IParameter, IParameter, string, PointF)` splits a wire into its wireless `Shout`/`Listen` pair through `DocumentMethods.SplitWire`, the pair's identities riding the receipt; `AdoptCase(IDocumentObject)` pulls a foreign object through `ObjectList.Transfer`; `RemapCase(Option<HashMap<Guid, Guid>>)` discriminates on payload shape — `None` remints every id through `ChangeAllIds`, `Some` applies the explicit map through `ApplyIdMap`; `PinCase(IPin)`/`RepairCase(PinRepair)` own global-pin membership and repair; `ExpireCase` expires the whole membership through `ExpireAll`; `WindowCase(WindowSelection, SelectionMode, bool, bool, bool)` applies rectangle selection through `WindowSelect`.
- Entry: `GraphScope.Mutate(VerbNoun label, GraphMutation op, Option<HostDocument> graph = default, Op? key = null)` → `Fin<MutationReceipt>` — the one mutation gate; wire arity, direction, and bulk kind are case payloads and rows, never sibling methods.
- Law: mutation and undo are one act — every `ActionList`-bearing arm mints one list, runs its host verb, and seals through `Document/history.md`'s `HistoryLedger.Seal` under the caller's `VerbNoun`; `WindowCase`, `AdoptCase`, `RemapCase`, `PinCase`, `RepairCase`, and `ExpireCase` are the host's own unsealed membership verbs and stamp `Sealed: false`.
- Law: wire mutation writes through `Grasshopper2.Parameters.Connections`, and reads never mutate: a probe from `[02]` inside a mutation arm is composition, never a second traversal implementation.
- Law: the wireless split is evidence-bearing — `SplitCase` surfaces the minted `Shout`/`Listen` instance ids as a `WirelessPair` on the receipt, so a consumer wires the listen side without re-searching the graph.
- RESEARCH: the positional semantics of `ReplaceSource`/`ReplaceTarget`'s three-parameter shape and of `WindowSelect`'s three trailing flags re-verify at decompile; `Connections.SwapSources` and the bare `DisconnectAllInputs`/`DisconnectAllOutputs` forms are catalog-unverified — the empty-`PruneCase` shape covers the latter pair, and a swap case lands if the decompile confirms the member.
- Boundary: `Shout`/`Listen` as canvas objects — their attributes, painting, and interaction — are `Canvas/*` and `Components/objects.md` territory; this page owns only their minting at the split seam. Relay elision on reads is `[02]`'s `RelayFreeCase`.
- Packages: Grasshopper2 (`Connections`, `ObjectList.Transfer`/`ChangeAllIds`/`ApplyIdMap`/`AddGlobalPin`/`RepairPins`/`ExpireAll`/`WindowSelect`, `DocumentMethods.SplitWire`, `Shout`, `Listen`, `PinRepair`, `WindowSelection`, `SelectionMode`), Eto (`PointF`), LanguageExt.Core, `Rasm.Domain`, `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`), `Document/history.md` (`HistoryLedger.Seal`).
- Growth: a new wire verb is one `GraphMutation` case; a new bulk-transfer kind is one `WireFreight` row; a new endpoint role is one `WireEndRole` row — the gate never widens.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using Rasm.Csp;
using Rasm.Parametric;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WireEndRole {
    public static readonly WireEndRole SourceEnd = new(key: 0, replace: static (anchor, retired, replacement, actions) =>
        Connections.ReplaceSource(anchor, retired, replacement, actions));
    public static readonly WireEndRole TargetEnd = new(key: 1, replace: static (anchor, retired, replacement, actions) =>
        Connections.ReplaceTarget(anchor, retired, replacement, actions));
    [UseDelegateFromConstructor] internal partial void Replace(IParameter anchor, IParameter retired, IParameter replacement, ActionList actions);
}

[SmartEnum<int>]
public sealed partial class WireFreight {
    public static readonly WireFreight CopyInputs = new(key: 0, haul: static (from, to, actions) => Connections.CopyAllInputs(from, to, actions));
    public static readonly WireFreight MigrateOutputs = new(key: 1, haul: static (from, to, actions) => Connections.MigrateAllOutputs(from, to, actions));
    [UseDelegateFromConstructor] internal partial void Haul(IParameter from, IParameter to, ActionList actions);
}

[Union]
[GenerateUnionOps]
public abstract partial record GraphMutation {
    private GraphMutation() { }
    public sealed record LinkCase(IParameter Source, IParameter Target) : GraphMutation;
    public sealed record UnlinkCase(IParameter Source, IParameter Target) : GraphMutation;
    public sealed record PruneCase(IParameter Pin, FlowSide Side, Seq<Guid> Kept) : GraphMutation;
    public sealed record RewireCase(WireEndRole Role, IParameter Anchor, IParameter Retired, IParameter Replacement) : GraphMutation;
    public sealed record BypassCase(IParameter Source, IParameter Middle, IParameter Target) : GraphMutation;
    public sealed record TransferCase(WireFreight Freight, IParameter From, IParameter To) : GraphMutation;
    public sealed record SplitCase(IParameter Source, IParameter Target, string Name, PointF At) : GraphMutation;
    public sealed record AdoptCase(IDocumentObject Subject) : GraphMutation;
    public sealed record RemapCase(Option<HashMap<Guid, Guid>> Map) : GraphMutation;
    public sealed record PinCase(IPin Pin) : GraphMutation;
    public sealed record RepairCase(PinRepair Repair) : GraphMutation;
    public sealed record ExpireCase : GraphMutation;
    public sealed record WindowCase(WindowSelection Frame, SelectionMode Mode, bool CrossWindow, bool KeepExisting, bool IncludeWires) : GraphMutation;
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct WirelessPair(Guid Shout, Guid Listen) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Shout != Guid.Empty),
        ValidityClaim.Of(holds: Listen != Guid.Empty));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MutationReceipt(
    Op Operation, string Verb, bool Sealed, Option<WirelessPair> Wireless,
    MonotonicStamp Entered, MonotonicStamp Settled, TimeSpan Latency) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Verb)),
        ValidityClaim.Evidence(evidence: Entered),
        ValidityClaim.Evidence(evidence: Settled),
        ValidityClaim.Nonnegative(value: Latency.TotalSeconds));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class GraphScope {
    public static Fin<MutationReceipt> Mutate(VerbNoun label, GraphMutation op, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return from valid in Optional(op).ToFin(active.InvalidInput())
               from timeline in MonotonicTimeline.Of(provider: TimeProvider.System, key: active)
               from entered in timeline.Capture(key: active)
               from outcome in DocumentScope.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Graph: document, Label: label),
                linkCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.LinkCase), actions => Connections.Connect(c.Source, c.Target, actions)),
                unlinkCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.UnlinkCase), actions => Connections.Disconnect(c.Source, c.Target, actions)),
                pruneCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.PruneCase), actions =>
                    c.Side.Prune(pin: c.Pin, kept: [.. c.Kept], actions: actions)),
                rewireCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.RewireCase), actions =>
                    c.Role.Replace(anchor: c.Anchor, retired: c.Retired, replacement: c.Replacement, actions: actions)),
                bypassCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.BypassCase), actions =>
                    Connections.CutOutMiddleMan(c.Source, c.Middle, c.Target, actions)),
                transferCase: static (frame, c) => Sealed(frame, nameof(GraphMutation.TransferCase), actions =>
                    c.Freight.Haul(from: c.From, to: c.To, actions: actions)),
                splitCase: static (frame, c) => frame.Key.Catch(body: () => {
                    ActionList actions = new();
                    frame.Graph.Methods.SplitWire(c.Source, c.Target, c.Name, c.At, out Shout shout, out Listen listen, actions);
                    return HistoryLedger.Seal(ledger: frame.Graph.Undo, actions: actions, label: frame.Label, key: frame.Key)
                        .Map(_ => (Verb: nameof(GraphMutation.SplitCase), Sealed: true,
                                   Wireless: Some(new WirelessPair(Shout: shout.InstanceId, Listen: listen.InstanceId))));
                }),
                adoptCase: static (frame, c) => Free(frame.Key, nameof(GraphMutation.AdoptCase), () => frame.Graph.Objects.Transfer(c.Subject)),
                remapCase: static (frame, c) => Free(frame.Key, nameof(GraphMutation.RemapCase), () => c.Map.Match(
                    Some: map => frame.Graph.Objects.ApplyIdMap(map.ToDictionary()),
                    None: () => frame.Graph.Objects.ChangeAllIds())),
                pinCase: static (frame, c) => Free(frame.Key, nameof(GraphMutation.PinCase), () => frame.Graph.Objects.AddGlobalPin(c.Pin)),
                repairCase: static (frame, c) => Free(frame.Key, nameof(GraphMutation.RepairCase), () => frame.Graph.Objects.RepairPins(c.Repair)),
                expireCase: static (frame, _) => Free(frame.Key, nameof(GraphMutation.ExpireCase), () => frame.Graph.Objects.ExpireAll()),
                windowCase: static (frame, c) => Free(frame.Key, nameof(GraphMutation.WindowCase), () =>
                    frame.Graph.Objects.WindowSelect(c.Frame, c.Mode, c.CrossWindow, c.KeepExisting, c.IncludeWires))))
               from settled in timeline.Capture(key: active)
               from latency in timeline.Elapsed(start: entered, end: settled, key: active)
               select new MutationReceipt(
                   Operation: active, Verb: outcome.Verb, Sealed: outcome.Sealed, Wireless: outcome.Wireless,
                   Entered: entered, Settled: settled, Latency: latency);
    }

    private static Fin<(string Verb, bool Sealed, Option<WirelessPair> Wireless)> Free(Op key, string verb, Action act) =>
        key.Catch(body: () => Fin.Succ((Op.Side(action: act), (Verb: verb, Sealed: false, Wireless: Option<WirelessPair>.None)).Item2));

    private static Fin<(string Verb, bool Sealed, Option<WirelessPair> Wireless)> Sealed(
        (Op Key, HostDocument Graph, VerbNoun Label) frame, string verb, Action<ActionList> act) =>
        frame.Key.Catch(body: () => {
            ActionList actions = new();
            act(obj: actions);
            return HistoryLedger.Seal(ledger: frame.Graph.Undo, actions: actions, label: frame.Label, key: frame.Key)
                .Map(_ => (Verb: verb, Sealed: true, Wireless: Option<WirelessPair>.None));
        });
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                             | [RAIL]                          | [CASES] |
| :-----: | :------------------ | :---------------------------------- | :------------------------------ | :-----: |
|  [01]   | flow direction      | `FlowSide`                          | `Search`/`Prune` (internal)     |    2    |
|  [02]   | neighbourhood reach | `GraphReach`                        | `Find → Seq<ConnectiveObject>`  |    4    |
|  [03]   | membership roster   | `GraphRoster`                       | `Project → GraphAnswer`         |   10    |
|  [04]   | read intent         | `GraphProbe` + `GraphAnswer`        | `Ask → Fin<GraphAnswer>`        | 10 + 11 |
|  [05]   | wire end role       | `WireEndRole` + `WireFreight`       | `Replace`/`Haul` (internal)     |  2 + 2  |
|  [06]   | mutation intent     | `GraphMutation` + `MutationReceipt` | `Mutate → Fin<MutationReceipt>` |   13    |

- [01]-[FLOW_DIRECTION]: `[SmartEnum<int>]` search + prune columns.
- [02]-[NEIGHBOURHOOD_REACH]: `[SmartEnum<int>]` `Connectivity` rows.
- [03]-[MEMBERSHIP_ROSTER]: `[SmartEnum<int>]` projection rows.
- [04]-[READ_INTENT]: `[Union]` request → closed `[Union]` result.
- [05]-[WIRE_END_ROLE]: `[SmartEnum<int>]` `Connections` delegate rows.
- [06]-[MUTATION_INTENT]: `[GenerateUnionOps]` `[Union]` + sealed evidence receipt.

`DocumentScope.Resolve`, `HistoryLedger.Seal`, `Op`, `Fault`, and `ValidityClaim` are composed upstream owners; local graph algorithms have no home here — host `Connectivity`/`ObjectList`/`Connections` absorption carries every capability as the cases and rows above.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

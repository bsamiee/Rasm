# [SYSTEMS_CONNECTIVITY]

The host-neutral MEP distribution-system connectivity layer: one `DistributionSystem` record carrying a closed `DistributionSystemKind` `[SmartEnum<string>]` partition over `IfcDistributionSystemEnum` (`airconditioning`/`electrical`/`domesticwater`/`drainage`/`fireprotection`/`ventilation`/…), its member distribution-element set, and the served spatial-structure set — and the `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) projecting `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts` as the two connection-edge modalities the flow network is built from. The connectivity layer is the network GRAPH, distinct from the `Model/elements#ELEMENT_MODEL` `IfcClass` MEP rows (`FlowSegment`/`FlowFitting`/`FlowTerminal`/`FlowController`/`DistributionPort` on the `IfcDomain.HvacFire`/`Electrical` partition) which model the element FAMILY: an air-handling system threading a hundred ducts, a domestic-water riser feeding every fixture, and an electrical distribution board powering every circuit each carry their full member set and their typed port-to-port adjacency on one record, never a per-discipline system type. The layer is HOST-NEUTRAL — it joins distribution elements and ports by stable `GlobalId` and never carries a RhinoCommon binding, the same seam violation the `Model/zones#ZONE_GRAPH` overlay and the `Model/structural#ANALYSIS_MODEL` graph also reject — and is a VIEW of the federated `Model/elements#ELEMENT_MODEL` `BimModel`: each `DistributionPort` binds its owning physical `BimElement` by `OwnerGlobalId` reference and each connection edge names its two ports by `GlobalId`, so the network is the flow-graph algebra the single-membership `Model/zones#ZONE_GRAPH` `DistributionSystem` zone row (a logical assignment of members into a group) structurally cannot express — the zone row owns WHICH elements belong to a system, the connectivity graph owns HOW they connect. The `SystemTrace` graph fold over the connection edges — every element reachable downstream of a junction port — is the typed network evidence the `Model/zones#ZONE_GRAPH` MEP-system grouping and the `Model/structural#ANALYSIS_MODEL` thermal/flow selection read by reference, and the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Electrical)`/`ByDomain(IfcDomain.Plumbing)` arms select the distribution-domain element set the projection traces over, never a second selection surface; a connectivity rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[CONNECTIVITY]: `DistributionSystem` record, the `DistributionSystemKind` `[SmartEnum<string>]` over `IfcDistributionSystemEnum`, the `DistributionPort` record carrying its `FlowDirection`/`PortKind`, the `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`), and the `DistributionSystemProjection.Project`/`ProjectAll` fold from the GeometryGym `IfcDistributionSystem`/`IfcDistributionPort`/`IfcRelConnectsPorts` surface to the typed network.
- [02]-[SYSTEM_TRACE]: `SystemNetwork` the port-adjacency graph the `PortConnection` edges fold into, the `SystemTrace` reachability fold over every element downstream of a junction port, and the `(GeometryKey, TopologyKey)` content-key identity the trace re-reads only on a changed network.

## [02]-[CONNECTIVITY]

- Owner: `DistributionSystem` the single host-neutral MEP distribution-system record carrying its `DistributionSystemKind` discriminant, its member distribution-element `GlobalId` set, its served spatial-structure `GlobalId` set, and the typed `PortConnection` edge set the flow network is built from; `DistributionSystemKind` the closed `[SmartEnum<string>]` keyed over the `IfcDistributionSystemEnum` member set (the airconditioning/electrical/water/drainage/fireprotection/ventilation distribution disciplines) with a `Domain` column resolving each kind onto the `Model/elements#ELEMENT_MODEL` `IfcDomain` partition; `DistributionPort` the typed connection-port record carrying its `FlowDirection` (`Source`/`Sink`/`SourceAndSink`/`NotDefined` over `IfcFlowDirectionEnum`), its `PortKind` (`Cable`/`Duct`/`Pipe`/… over the `IfcDistributionPort.PredefinedType`), and the `OwnerGlobalId` binding to the physical distribution element the port belongs to; `PortConnection` the closed `[Union]` discriminating the two IFC connection modalities — `ConnectsPortToElement` (the `IfcRelConnectsPortToElement` port-membership edge binding a port to its owning element) and `ConnectsPorts` (the `IfcRelConnectsPorts` port-to-port flow edge carrying its optional realizing element); `DistributionSystemProjection` the static fold over the GeometryGym `IfcDistributionSystem` surface.
- Cases: `DistributionSystemKind` rows `AirConditioning`/`Ventilation`/`ChilledWater`/`CompressedAir`/`FireProtection`/`Electrical`/`Lighting`/`Telephone`/`DataCommunication`/`DomesticColdWater`/`DomesticHotWater`/`Drainage`/`Sewage` (13 disciplines, each frozen with its `IfcDomain` over the seven-case partition — the HVAC/fire disciplines on `HvacFire`, the power/telecom disciplines on `Electrical`, the water/drainage disciplines on `Plumbing`) plus the `UserDefined`/`NotDefined` fallback rows the `Of` resolver lowers an unmapped `IfcDistributionSystemEnum` member onto (15 total); `PortConnection` arms `ConnectsPortToElement` (the `IfcRelConnectsPortToElement` port-membership edge — `PortGlobalId`, `ElementGlobalId`) · `ConnectsPorts` (the `IfcRelConnectsPorts` port-to-port flow edge — `RelatingPortGlobalId`, `RelatedPortGlobalId`, and the optional `RealizingElementGlobalId` the joint/fitting that realizes the connection) (2); `DistributionPort` carries `GlobalId`, `Name`, `FlowDirection`, `PortKind`, and `OwnerGlobalId` — a duct port is a `Source`/`Sink` `DistributionPort` on a `FlowSegment` owner, a tee fitting carries three ports, and an unconnected port is a port with no incident `ConnectsPorts` edge.
- Entry: `DistributionSystemProjection.Project(IfcDistributionSystem system, BimModel federated)` folds one GeometryGym distribution-system container into one `DistributionSystem` — materializing the system's `IsGroupedBy` `IfcRelAssignsToGroup.RelatedObjects` distribution-element member set once (the system is an `IfcSystem` grouping its flow elements), discriminating the system's `PredefinedType` onto the `DistributionSystemKind`, folding each member element's `HasPorts` `IfcRelConnectsPortToElement` set onto `DistributionPort` rows and `ConnectsPortToElement` edges, folding each port's single `ConnectedFrom`/`ConnectedTo` `IfcRelConnectsPorts` references onto the `ConnectsPorts` flow edges (deduped on the unordered port pair so a connection materialized from both incident ports rides one edge), and carrying the served spatial-structure `GlobalId` set (the forward `IfcRelServicesBuildings` inverse is absent on this GeometryGym surface — the relationship is reachable only from the spatial side `IfcSpatialElement.ServicedBySystems`, so the column is presently empty and joined from the spatial owner at a later pass) — and `DistributionSystemProjection.ProjectAll(DatabaseIfc db, BimModel federated)` lifts every `IfcDistributionSystem` the database carries onto the `Seq<DistributionSystem>` the trace folds; `Fin<T>` aborts on a system grouping a member the federated model never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) or a connection edge naming a port absent from the materialized port set (`BimFault.DanglingReference`), each lowered with `.ToError()`, and a system whose `PredefinedType` carries no mapped `IfcDistributionSystemEnum` member resolves to the `UserDefined`/`NotDefined` kind row rather than faulting.
- Auto: `Project` reads the `IfcDistributionSystem` runtime graph and folds it into the typed system — the `MembersOf` projection materializes the container's `IsGroupedBy` grouped `IfcDistributionElement` set once and the `PortsOf` fold reads each member's `HasPorts` `IfcRelConnectsPortToElement` relationship onto a `DistributionPort` (reading `FlowDirection` onto the typed direction and `PredefinedType` onto the `PortKind`) and the matching `ConnectsPortToElement` membership edge, all over the one materialized member set so the ports and the port-to-port edges read the single traversal rather than three; the `EdgesOf` fold reads each materialized port's single `ConnectedFrom`/`ConnectedTo` `IfcRelConnectsPorts` reference onto `ConnectsPorts` flow edges keyed by the unordered `(RelatingPort.GlobalId, RelatedPort.GlobalId)` pair so a connection reachable from either incident port is one edge, carrying the `RealizingElement.GlobalId` joint when the relationship names one; the `ServesOf` fold yields an empty served-structure set because the forward `IfcRelServicesBuildings` inverse is absent on the GeometryGym distribution-system surface (the serving relationship is exposed only as `IfcSpatialElement.ServicedBySystems`), so the `ServedStructureGlobalIds` column is joined from the spatial owner at a later pass rather than re-mining a non-existent member; the `DistributionSystem.BindFederated` fold confirms every member and every edge-named port resolves against the `Model/elements#ELEMENT_MODEL` `BimModel` element/port index so a dangling member or a connection naming an absent port lowers `BimFault.DanglingReference`, and the `DistributionSystem.Identity` fold derives the `(GeometryKey, TopologyKey)` `UInt128` pair the trace re-reads the network by — `GeometryKey` over the ordered member-element `GlobalId` set through `XxHash128.HashToUInt128` (the member-set fingerprint the federated geometry joins by reference) and `TopologyKey` over the sorted connection-edge unordered port pairs so the trace re-walks only a changed membership or a changed adjacency.
- Receipt: the `Seq<DistributionSystem>` is the connectivity evidence the `[3]-[SYSTEM_TRACE]` `SystemTrace` fold walks, the `Model/zones#ZONE_GRAPH` MEP `DistributionSystem` zone row reads the member set by reference, and the `Model/structural#ANALYSIS_MODEL` flow/thermal selection reads the typed `Seq<DistributionSystem>` by reference; the air-handling system, the water riser, and the electrical board each carry their member set and their typed port-to-port adjacency on one record, the flow network the `IfcClass` MEP element rows model the family of but never the connectivity of.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core
- Growth: a new distribution discipline is one `DistributionSystemKind` row reading the next `IfcDistributionSystemEnum` member with its `IfcDomain` column; a new connection modality is one `PortConnection` union arm reading the next port-relationship entity; a new port flow direction is one `FlowDirection` row; a new served-structure relationship rides the existing `ServesOf` fold on one row; never a per-discipline system record, never a second connectivity store, and never a per-relationship connection class.
- Boundary: `DistributionSystem` is ONE record discriminated by the `DistributionSystemKind` row data and the `PortConnection` union — an `HvacSystem`/`ElectricalSystem`/`PlumbingSystem` class family or sibling per-discipline factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL` and the no-per-grouping-type law at `Model/zones#ZONE_GRAPH`; the two connection relationships are the closed `PortConnection` union, never a per-relationship class and never an `Option<PortConnection>` escape; the GeometryGym `IfcDistributionSystem`/`IfcDistributionElement`/`IfcDistributionPort`/`IfcRelConnectsPortToElement`/`IfcRelConnectsPorts`/`IfcRelServicesBuildings` surface (`.api/api-geometrygym-ifc` MEP distribution-element family rows 1-10, grouping-zone-distribution-system family rows 4/8/9/10) is consumed as settled vocabulary through the `IsGroupedBy`/`HasPorts`/`ConnectedFrom`/`ConnectedTo` traversal and a hand-rolled distribution-system reader is the deleted form; the distribution-domain element selection is the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Electrical)`/`ByDomain(IfcDomain.Plumbing)` predicate and a parallel system-element selection arm is the no-second-selection-surface reject; the connectivity graph is the orthogonal companion to the `Model/zones#ZONE_GRAPH` `DistributionSystem` zone row — the zone row owns the logical member assignment (which elements belong) and the connectivity graph owns the typed flow adjacency (how they connect), the two coexisting and never collapsed; the `(GeometryKey, TopologyKey)` identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and a second identity scheme is the named drift defect; a connectivity rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class FlowDirection {
    public static readonly FlowDirection Source        = new("SOURCE");
    public static readonly FlowDirection Sink          = new("SINK");
    public static readonly FlowDirection SourceAndSink = new("SOURCEANDSINK");
    public static readonly FlowDirection NotDefined    = new("NOTDEFINED");

    public static FlowDirection Of(IfcFlowDirectionEnum direction) =>
        TryGet(direction.ToString()).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class DistributionSystemKind {
    public static readonly DistributionSystemKind AirConditioning   = new("AIRCONDITIONING",   IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Ventilation       = new("VENTILATION",       IfcDomain.HvacFire);
    public static readonly DistributionSystemKind ChilledWater      = new("CHILLEDWATER",      IfcDomain.HvacFire);
    public static readonly DistributionSystemKind CompressedAir     = new("COMPRESSEDAIR",     IfcDomain.HvacFire);
    public static readonly DistributionSystemKind FireProtection    = new("FIREPROTECTION",    IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Electrical        = new("ELECTRICAL",        IfcDomain.Electrical);
    public static readonly DistributionSystemKind Lighting          = new("LIGHTING",          IfcDomain.Electrical);
    public static readonly DistributionSystemKind Telephone         = new("TELEPHONE",         IfcDomain.Electrical);
    public static readonly DistributionSystemKind DataCommunication = new("DATACOMMUNICATION", IfcDomain.Electrical);
    public static readonly DistributionSystemKind DomesticColdWater = new("DOMESTICCOLDWATER", IfcDomain.Plumbing);
    public static readonly DistributionSystemKind DomesticHotWater  = new("DOMESTICHOTWATER",  IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Drainage          = new("DRAINAGE",          IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Sewage            = new("SEWAGE",            IfcDomain.Plumbing);
    public static readonly DistributionSystemKind UserDefined       = new("USERDEFINED",       IfcDomain.HvacFire);
    public static readonly DistributionSystemKind NotDefined        = new("NOTDEFINED",        IfcDomain.HvacFire);

    public IfcDomain Domain { get; }

    public static DistributionSystemKind Of(IfcDistributionSystemEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

public sealed record DistributionPort(
    string GlobalId,
    string Name,
    FlowDirection FlowDirection,
    PredefinedType PortKind,
    string OwnerGlobalId);

[Union]
public partial record PortConnection {
    partial record ConnectsPortToElement(string PortGlobalId, string ElementGlobalId);
    partial record ConnectsPorts(string RelatingPortGlobalId, string RelatedPortGlobalId, Option<string> RealizingElementGlobalId);

    public (string A, string B) Endpoints => Switch(
        connectsPortToElement: static e => (e.PortGlobalId, e.ElementGlobalId),
        connectsPorts:         static e => Pair(e.RelatingPortGlobalId, e.RelatedPortGlobalId));

    static (string A, string B) Pair(string a, string b) =>
        string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);
}

public sealed record DistributionSystem(
    string GlobalId,
    string Name,
    DistributionSystemKind Kind,
    Seq<string> MemberGlobalIds,
    Seq<DistributionPort> Ports,
    Seq<PortConnection> Connections,
    Seq<string> ServedStructureGlobalIds) {
    public (UInt128 GeometryKey, UInt128 TopologyKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join(",", MemberGlobalIds.Order()))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join(";",
            Connections.Map(static c => c.Endpoints).Map(static e => $"{e.A}-{e.B}").Order()))));

    public Fin<DistributionSystem> BindFederated(BimModel federated) {
        var elements = toHashSet(federated.Elements.Map(static e => e.GlobalId));
        var ports = toHashSet(Ports.Map(static p => p.GlobalId));
        return MemberGlobalIds.Find(id => !elements.Contains(id)).Match(
            Some: id => FinFail<DistributionSystem>(new BimFault.DanglingReference($"distribution-member-unmapped:{GlobalId}:{id}").ToError()),
            None: () => Connections
                .Filter(static c => c is PortConnection.ConnectsPorts)
                .Map(static c => c.Endpoints)
                .Find(pair => !ports.Contains(pair.A) || !ports.Contains(pair.B))
                .Match(
                    Some: pair => FinFail<DistributionSystem>(new BimFault.DanglingReference($"connection-port-unmapped:{GlobalId}:{pair.A}/{pair.B}").ToError()),
                    None: () => FinSucc(this)));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class DistributionSystemProjection {
    public static Fin<DistributionSystem> Project(IfcDistributionSystem system, BimModel federated) {
        var members = MembersOf(system);
        var ports = PortsOf(members);
        return new DistributionSystem(
            system.GlobalId,
            system.Name ?? "",
            DistributionSystemKind.Of(system.PredefinedType),
            members.Map(static m => m.GlobalId),
            ports,
            EdgesOf(members),
            ServesOf(system))
            .BindFederated(federated);
    }

    public static Fin<Seq<DistributionSystem>> ProjectAll(DatabaseIfc db, BimModel federated) =>
        db.Project.Extract<IfcDistributionSystem>()
            .AsIterable()
            .ToSeq()
            .TraverseM(system => Project(system, federated))
            .As();

    static Seq<IfcDistributionElement> MembersOf(IfcDistributionSystem system) =>
        system.IsGroupedBy
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcDistributionElement>()
            .ToSeq();

    static Seq<DistributionPort> PortsOf(Seq<IfcDistributionElement> members) =>
        members
            .AsIterable()
            .SelectMany(static element => element.HasPorts
                .AsIterable()
                .Select(static rel => rel.RelatingPort)
                .OfType<IfcDistributionPort>()
                .Select(port => new DistributionPort(
                    port.GlobalId,
                    port.Name ?? "",
                    FlowDirection.Of(port.FlowDirection),
                    PredefinedType.Create(port.PredefinedType.ToString()),
                    element.GlobalId)))
            .DistinctBy(static p => p.GlobalId)
            .ToSeq();

    static Seq<PortConnection> EdgesOf(Seq<IfcDistributionElement> members) {
        var membership = members
            .AsIterable()
            .SelectMany(static element => element.HasPorts
                .AsIterable()
                .OfType<IfcRelConnectsPortToElement>()
                .Map(rel => (PortConnection)new PortConnection.ConnectsPortToElement(
                    rel.RelatingPort?.GlobalId ?? "",
                    element.GlobalId)))
            .Where(static edge => Bounded(edge.Endpoints));
        var flow = members
            .AsIterable()
            .SelectMany(static element => element.HasPorts.AsIterable().Select(static rel => rel.RelatingPort))
            .OfType<IfcDistributionPort>()
            .SelectMany(static port => new[] { port.ConnectedFrom, port.ConnectedTo })
            .Where(static rel => rel is not null)
            .Map(static rel => (PortConnection)new PortConnection.ConnectsPorts(
                rel.RelatingPort?.GlobalId ?? "",
                rel.RelatedPort?.GlobalId ?? "",
                Optional(rel.RealizingElement?.GlobalId).Filter(static id => id.Length > 0)))
            .Where(static edge => Bounded(edge.Endpoints))
            .DistinctBy(static edge => edge.Endpoints);
        return membership.Concat(flow).ToSeq();

        static bool Bounded((string A, string B) endpoints) =>
            endpoints.A.Length > 0 && endpoints.B.Length > 0;
    }

    static Seq<string> ServesOf(IfcDistributionSystem system) =>
        Seq<string>();
}
```

## [03]-[SYSTEM_TRACE]

- Owner: `SystemNetwork` the undirected port-adjacency graph the `PortConnection.ConnectsPorts` flow edges and the `ConnectsPortToElement` membership edges fold into — a `Map<string, Seq<string>>` adjacency from each port to the ports it connects to plus the `Map<string, string>` port-to-owner index the membership edges build — and `SystemTrace` the reachability fold over that adjacency: the set of every distribution element reachable downstream of a starting junction port through the connection graph, a breadth-first closure over the port adjacency that crosses each fitting's ports to the next segment, the network walk the per-discipline imperative traversal is the deleted form of.
- Entry: `SystemNetwork.Of(DistributionSystem system)` folds one typed system into the adjacency-and-owner graph once, and `SystemTrace.Downstream(DistributionSystem system, string fromPortGlobalId)` folds the reachable-element closure from a junction port — materializing the `SystemNetwork`, walking the port adjacency from the seed port as an immutable visited-set fold (a port's owner element joins the trace, the port's incident ports enqueue, each visited once), and returning the `SystemTrace` carrying the reached element `GlobalId` set in encounter order and the reached-port set; `SystemTrace.DownstreamOf(DistributionSystem system, string elementGlobalId)` seeds the trace from every port the named element owns so a downstream query keys on an element rather than a port; the trace is total over the closed graph and never faults — an isolated port traces to itself, a port absent from the network traces to the empty set — so the reachability fold carries no `Fin<T>` rail, the rejection already lowered at `Project`.
- Auto: `SystemNetwork.Of` folds the system's `Connections` into the adjacency map — each `ConnectsPorts` edge adds the unordered pair to both endpoints' adjacency sets (the flow graph is undirected at the topology layer, the `FlowDirection` carried on the `DistributionPort` orienting it only when a directed trace is asked for) and each `ConnectsPortToElement` membership edge populates the port-to-owner index — and `SystemTrace.Downstream` runs the closure as a tail-recursive fold threading the `(frontier, visited, ports, elements)` accumulator: the frontier seeds with the start port, each step pops a port, joins its owner element to the ordered element accumulator if newly seen, and pushes the port's not-yet-visited adjacency neighbours onto the frontier, terminating when the frontier empties; the trace keys on the owning `DistributionSystem.Identity` `(GeometryKey, TopologyKey)` the `Downstream` fold reads its system by, so a trace is memoizable against the network content key and re-walks only on a changed adjacency, never re-deriving the network per query.
- Receipt: the `SystemTrace` reached-element `Seq<string>` is the downstream-network evidence the `Model/zones#ZONE_GRAPH` MEP `DistributionSystem` grouping reads to resolve a system's effective member closure and the `Model/query#ELEMENT_SET` consumers intersect against a domain set — a "every air terminal fed from this air-handling unit" / "every fixture downstream of this shutoff valve" query is one `Downstream` fold over the port graph, the connectivity the single-membership zone row cannot express; the `SystemNetwork` adjacency is the typed graph the `Model/structural#ANALYSIS_MODEL` flow/thermal idealization reads by reference, never re-projected per consumer.
- Packages: LanguageExt.Core
- Growth: a new trace direction (upstream, both) is one orientation column read off the `FlowDirection` the port already carries; a new reachability predicate (stop at a controller, stop at a discipline boundary) is one closure-guard argument on the existing fold; a new graph query rides the same `SystemNetwork` adjacency; never a per-direction trace record, never a second adjacency store, and never a per-discipline traversal.
- Boundary: the `SystemTrace` is ONE reachability fold over the closed `SystemNetwork` adjacency — a `TraceHvac`/`TraceElectrical`/`TracePlumbing` operation family is the deleted form per the no-operation-family law, the discipline already carried by the `DistributionSystem.Kind` the trace folds within; the closure is an immutable visited-set fold over the port adjacency, never a mutable-accumulator imperative walk with a `HashSet<string>` mutated in place outside the fold; the adjacency is built once by `SystemNetwork.Of` from the typed `PortConnection` edges, never re-walked from the GeometryGym graph (the network is a VIEW of the settled `[2]-[CONNECTIVITY]` projection); the trace carries no `Fin<T>` rail because the rejection lowered at `Project` (a dangling member or absent port faulted there) so the closed graph is total; the trace reads the `DistributionSystem.Identity` content key for memoization and a second identity scheme is the named drift defect; the `SystemTrace` reached set is consumed by the `zoning`/`query`/`analysis` peers by reference and re-deriving the network in any consumer is the named cross-page drift.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record SystemNetwork(
    Map<string, Seq<string>> Adjacency,
    Map<string, string> PortOwner) {
    public static SystemNetwork Of(DistributionSystem system) {
        var owner = system.Connections
            .Filter(static c => c is PortConnection.ConnectsPortToElement)
            .Fold(Map<string, string>(), static (map, c) =>
                c is PortConnection.ConnectsPortToElement(var port, var element) ? map.AddOrUpdate(port, element) : map);
        var adjacency = system.Connections
            .Filter(static c => c is PortConnection.ConnectsPorts)
            .Fold(Map<string, Seq<string>>(), static (map, c) => {
                var (a, b) = c.Endpoints;
                return map
                    .AddOrUpdate(a, existing => existing.Add(b), () => Seq(b))
                    .AddOrUpdate(b, existing => existing.Add(a), () => Seq(a));
            });
        return new SystemNetwork(adjacency, owner);
    }

    public Seq<string> Neighbours(string portGlobalId) =>
        Adjacency.Find(portGlobalId).IfNone(Seq<string>());

    public Option<string> OwnerOf(string portGlobalId) =>
        PortOwner.Find(portGlobalId);
}

public sealed record SystemTrace(string SeedPortGlobalId, Seq<string> ReachedElements, Seq<string> ReachedPorts) {
    public static SystemTrace Downstream(DistributionSystem system, string fromPortGlobalId) =>
        Walk(SystemNetwork.Of(system), Seq(fromPortGlobalId), fromPortGlobalId);

    public static SystemTrace DownstreamOf(DistributionSystem system, string elementGlobalId) {
        var network = SystemNetwork.Of(system);
        var seeds = system.Ports.Filter(p => p.OwnerGlobalId == elementGlobalId).Map(static p => p.GlobalId);
        return seeds.Fold(
            new SystemTrace(elementGlobalId, Seq<string>(), Seq<string>()),
            (trace, seed) => Merge(trace, Walk(network, Seq(seed), elementGlobalId)));
    }

    static SystemTrace Walk(SystemNetwork network, Seq<string> frontier, string seed) {
        var (reachedPorts, reachedElements) = Closure(network, frontier, toHashSet<string>(), Seq<string>(), Seq<string>());
        return new SystemTrace(seed, reachedElements, reachedPorts);
    }

    static (Seq<string> Ports, Seq<string> Elements) Closure(
        SystemNetwork network, Seq<string> frontier,
        HashSet<string> visited, Seq<string> ports, Seq<string> elements) =>
        frontier.HeadOrNone().Match(
            None: () => (ports, elements),
            Some: port => visited.Contains(port)
                ? Closure(network, frontier.Tail, visited, ports, elements)
                : Closure(
                    network,
                    frontier.Tail.Concat(network.Neighbours(port).Filter(n => !visited.Contains(n))),
                    visited.Add(port),
                    ports.Add(port),
                    network.OwnerOf(port).Match(
                        Some: element => elements.Contains(element) ? elements : elements.Add(element),
                        None: () => elements)));

    static SystemTrace Merge(SystemTrace into, SystemTrace from) =>
        into with {
            ReachedElements = into.ReachedElements.Concat(from.ReachedElements.Filter(e => !into.ReachedElements.Contains(e))),
            ReachedPorts = into.ReachedPorts.Concat(from.ReachedPorts.Filter(p => !into.ReachedPorts.Contains(p))),
        };
}
```

## [04]-[RESEARCH]

- [DISTRIBUTION_SYSTEM_TRAVERSAL]: the `IfcDistributionSystem` container traversal is verified against the live GeometryGym decompile — `IfcDistributionSystem : IfcSystem : IfcGroup` carries `PredefinedType` (`IfcDistributionSystemEnum`) and inherits `IsGroupedBy` (`IfcRelAssignsToGroup`), so the `MembersOf` fold materializes the grouped `IfcDistributionElement` member set (`IfcFlowSegment`/`IfcFlowFitting`/`IfcFlowTerminal`/`IfcFlowController`) through the same `IsGroupedBy` grouping path the `Model/zones#ZONE_GRAPH` overlay flattens, distinct from per-element spatial containment; the served spatial-structure set has NO forward inverse on the distribution system in GeometryGym 25.7.30 (`IfcRelServicesBuildings` is reachable only as `IfcSpatialElement.ServicedBySystems` from the spatial side), so the `ServesOf` fold yields empty and the served column is joined from the spatial owner at a later pass; the `IfcDistributionSystem.PredefinedType`/`IsGroupedBy` and `IfcDistributionElement.HasPorts` member spellings confirm against the live surface before the projection fold is final.
- [PORT_CONNECTION_GRAPH]: the `IfcDistributionPort`/`IfcRelConnectsPortToElement`/`IfcRelConnectsPorts` connection-edge member spellings the `PortsOf`/`EdgesOf` folds read onto the `PortConnection` union — the `IfcDistributionElement.HasPorts` `IfcRelConnectsPortToElement` port-membership set (`RelatingPort` the port, the element the owner), the port's `IfcDistributionPort.FlowDirection` `IfcFlowDirectionEnum` and `PredefinedType`, the port's single `ContainedIn` (`IfcRelConnectsPortToElement`), `ConnectedFrom`, and `ConnectedTo` (each a single `IfcRelConnectsPorts`, NOT a set — the fold wraps the two non-null references into a two-element array) flow-edge references (`RelatingPort`/`RelatedPort` the two ends, the optional `RealizingElement` the joint that realizes the connection) — verified against the live GeometryGym decompile: `IfcDistributionElement.HasPorts` is a `SET<IfcRelConnectsPortToElement>`, `IfcDistributionPort : IfcPort` carries `FlowDirection` (`IfcFlowDirectionEnum`) and `PredefinedType` (`IfcDistributionPortTypeEnum`), `IfcRelConnectsPortToElement.RelatingPort`/`RelatedElement`, and `IfcRelConnectsPorts.RelatingPort`/`RelatedPort`/`RealizingElement` are the real member spellings, and the unordered-pair dedupe on the `(RelatingPort.GlobalId, RelatedPort.GlobalId)` key — a connection materialized from both incident ports — is the verified single-edge invariant before the `EdgesOf` fold is final.
- [NETWORK_CONTENT_KEY]: the `DistributionSystem.Identity` `(GeometryKey, TopologyKey)` `UInt128` pair the `SystemTrace` re-reads the network by grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom, so a trace re-walks the network only on a changed `GeometryKey` (the ordered member-element geometry-handle keys) or `TopologyKey` (the sorted connection-edge unordered port pairs) rather than re-folding the adjacency; the trace identity is the BIM-side network key and re-minting a second identity scheme for the connectivity memoization is the named cross-folder drift defect — the connectivity owner produces the typed network and its content-key identity, the trace folds reachability over it, never a re-projection of the GeometryGym graph per query.

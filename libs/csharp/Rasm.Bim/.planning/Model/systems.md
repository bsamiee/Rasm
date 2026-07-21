# [SYSTEMS_CONNECTIVITY]

The host-neutral MEP distribution-system connectivity layer is a VIEW over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second GeometryGym lowering: the `Projection/semantic#SEMANTIC_PROJECTOR` projector is the SOLE IFC owner and has already lowered every `IfcDistributionSystem`, `IfcDistributionPort`, `IfcRelConnectsPorts`, `IfcRelConnectsPortToElement`, `IfcRelNests` (the IFC4 port-containment form), `IfcRelAssignsToGroup`, and `IfcRelServicesBuildings` onto NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` edges — a `Connect{Port}` port-to-port FLOW edge, a `Generic("IfcRelConnectsPortToElement")`/`Compose{Nest}` port-OWNERSHIP pair (the IFC2x3 and IFC4 containment forms), an `Assign{Group}` membership edge, and a `Generic("IfcRelServicesBuildings")` passthrough — so this layer reads the settled graph and folds it into a typed flow-network view, a `QuikGraph` reachability trace, and a `SwiftCollections.Lean` interference clash, exactly as the `Model/query#ELEMENT_SET` query surface reads the same graph. The retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold and the `PortConnection` `[Union]` mirroring `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts` are GONE — the typed-`IfcRel*`-case shape is the deleted form the seam's neutral edge algebra collapsed [NEUTRAL_EDGE_RULING], the connectivity now reading the neutral edges by wire-name and endpoint classification (a flow edge joins two `IfcDistributionPort` objects; an ownership edge joins a port to its distribution element through either containment form).

The connectivity layer is the network GRAPH, orthogonal to the `Model/zones#ZONE_GRAPH` overlay that owns the LOGICAL membership (which elements belong to a system, the `Assign{Group}` edge) — the connectivity owns the typed flow ADJACENCY (how they connect, the `Connect{Port}` edge), the two coexisting and never collapsed. An air-handling system threading a hundred ducts, a domestic-water riser feeding every fixture, and an electrical distribution board powering every circuit each surface their member set, their typed port adjacency, and their served spatial structures from one `DistributionSystem` view, never a per-discipline system type. Identity is the seam `Rasm.Element/Graph/element#NODE_MODEL` `NodeId` (a rooted Guid-v7), never an IFC `GlobalId` — the compressed GlobalId is the node's `ExternalId` projection attribute the trace and interference carry only for the IFC-keyed downstream consumers. The layer is HOST-NEUTRAL: it joins nodes by `NodeId`, references geometry by the seam `RepresentationContentHash` content key, routes every solid-proximity test to the injected kernel `GeometryProximity` seam, and never carries a RhinoCommon binding or an in-process tessellation — the same seam law the `Model/zones#ZONE_GRAPH` overlay and the `Model/structural#STRUCTURAL_PROJECTION` graph hold. The reachability fold and the system view are TOTAL over the already-validated graph (the dangling-endpoint rejection lowered at `Projection/semantic#SEMANTIC_PROJECTOR` `Project` and `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Apply`); only the interference clash carries a `Fin<T>` rail, lowering `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE when a member's geometry content key resolves to no kernel geometry.

## [01]-[INDEX]

- [01]-[CONNECTIVITY]: the `DistributionSystem` derived view (member `NodeId` set, nested sub-circuit set, typed `DistributionPort` set, port `FlowEdge` set, served-structure set, `(MembershipKey, TopologyKey)` identity), the `DistributionSystemKind` `[SmartEnum<string>]` over `IfcDistributionSystemEnum` with its `IfcDomain` discipline and `FlowMedium` carrier columns, the `FlowDirection` `[SmartEnum<string>]` over `IfcFlowDirectionEnum`, and the `DistributionNetwork` fold reading the seam `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views.
- [02]-[SYSTEM_TRACE]: the `SystemTrace` reachability fold over the seam's port-and-element flow graph — one transient `QuikGraph` `AdjacencyGraph<NodeId, SEdge<NodeId>>` built from the `Connect{Port}` edges, the `TraceMode` orientation policy (reach/downstream/upstream) reading the port `FlowDirection`, the `BreadthFirstSearchAlgorithm` event fold computing the reachable-element closure (the shared `[GRAPH_ALGORITHM]` owner replacing the hand-rolled visited-set walk), the `Demand` downstream accumulation reducing reached-terminal design values through the query `SumOf` composition, and the `Runs` index-run ranking ordering every reached terminal by its best-route resistance from the seed over the same oriented adjacency.
- [03]-[INTERFERENCE]: the `Interference` evidence, `GeometryProximity` kernel seam, stateful `BroadPhase` policy, and `InterferenceCheck.Interferences`/`Refit` folds over refittable `SwiftBVH<NodeId>` or `SwiftSpatialHash<NodeId>` indexes.

## [02]-[CONNECTIVITY]

- Owner: `DistributionSystem` the single host-neutral derived VIEW of one MEP distribution system read from the seam `ElementGraph` — the system group `NodeId`, the `ExternalId` (the IFC `GlobalId` projection attribute), the `DistributionSystemKind` discriminant resolved off the group node's `PredefinedType`, the member `NodeId` set, the `Circuits` sub-circuit subset (the members themselves classified as system groups — an `IfcDistributionCircuit` under its parent board), the typed `DistributionPort` set, the port-to-port `FlowEdge` set the flow network is built from, and the served spatial-structure `NodeId` set, with a derived `(MembershipKey, TopologyKey)` content-key identity the trace re-reads the network by; `DistributionSystemKind` the closed `[SmartEnum<string>]` keyed over the `IfcDistributionSystemEnum` member set with a `Domain` column resolving each kind onto the `Model/elements#IFC_CLASS` `IfcDomain` partition; `FlowDirection` the `[SmartEnum<string>]` over `IfcFlowDirectionEnum` carrying the `Emits`/`Receives` orientation columns the directed trace reads; `DistributionPort` the derived port view (`NodeId`, name, `FlowDirection`, the `PredefinedType` port kind, and the owning distribution element `NodeId`); `DistributionNetwork` the static fold reading the seam graph's `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views.
- Cases: `DistributionSystemKind` rows span the FULL `IfcDistributionSystemEnum` distribution vocabulary partitioned across the `IfcDomain` set — air/thermal-fluid/combustion-fuel/fire-and-life-safety (`AirConditioning`/`Ventilation`/`ChilledWater`/`CompressedAir`/`Heating`/`Refrigeration`/`Fuel`/`Gas`/`Oil`/`FireProtection`/`Safety`/…) on `HvacFire`, piped water/drainage/waste/process (`DomesticColdWater`/`DomesticHotWater`/`WaterSupply`/`Drainage`/`Sewage`/`RainWater`/`StormWater`/`WasteWater`/`Chemical`/…) on `Plumbing`, power/lighting/telecom/data/signal/rail-traction (`Electrical`/`Lighting`/`Telephone`/the DISTINCT `Data` and `Communication` rows/`Security`/`CatenarySystem`/…) on `Electrical`, and `Conveying` on `Architecture` — every IFC4 discipline, the seven IFC4X3 rail-electrification/telephony additions (`CatenarySystem`/`OverheadContactLine`/`ReturnCircuit`/`FixedTransmissionNetwork`/`OperationalTelephony`/`MobileNetwork`/`MonitoringSystem`), and the IFC4X4 `Safety` draft each frozen with its `IfcDomain` discipline AND its `FlowMedium` carrier (the physical medium the token implies — the demand/sizing partition beyond the discipline), plus the `UserDefined`/`NotDefined` fallback rows the `Of` resolver lowers an unmapped token onto — carried on the generic `Architecture` domain so an unclassified system never pollutes a `ByDomain(IfcDomain.HvacFire)` discipline selection — the closed buildingSMART roster, never a 13-of-50 slice and never a fused `DataCommunication` phantom (the enum carries `DATA` and `COMMUNICATION` separately, no `DATACOMMUNICATION` token); `FlowDirection` rows `Source` (emits) · `Sink` (receives) · `SourceAndSink` (both) · `NotDefined` (both — an undirected port conducts either way) (the full `IfcFlowDirectionEnum`, 4); a `DistributionPort` is a `Source`/`Sink` port on a `FlowSegment` owner, a tee fitting carries three ports, and a `FlowEdge` joins two ports across elements (the `IfcRelConnectsPorts` flow connection) carrying its optional realizing fitting `NodeId`.
- Entry: `DistributionNetwork.View(ElementGraph graph, Option<NodeId> scope)` folds either all distribution-system group nodes or one selected group into `Seq<DistributionSystem>`; the input value owns modality, and callers state `None` when they request the whole model. The read is total because graph admission already rejected dangling endpoints. An unmapped `PredefinedType` resolves to `DistributionSystemKind.NotDefined`.
- Auto: `View` reads the group `Node.Object` set classified as a distribution system, and `Of` folds one — `MembersOf` reads the group's incident `Assign{Group}` edges with the system PINNED as the `Definition` endpoint (the projector's INVERTED `Assign`: `Subject` = member, `Definition` = group — the same directional pin the zones read holds, so a nested system's own membership in a parent group never folds the parent into its member set), the `Circuits` subset filtering the members whose own node is system-classified (an `IfcDistributionCircuit` rides as a member AND as its own `DistributionSystem` row); `PortsOf` reads each member's incident port-OWNERSHIP edges over BOTH containment forms — the `Generic` edge carrying the `IfcRelKind.ConnectsPortToElement.Key` wire-name (the IFC2x3 projection, its port on the relating side) and the `Compose{Nest}` edge whose part is a port (the IFC4 `IfcRelNests` port-containment projection) — onto deduped `DistributionPort` rows carrying the port node's name, its `PredefinedType` kind, and its `FlowDirection` read off the port's effective `FlowDirection` property the projector lowers, so an IFC4 model whose ports nest under their elements loses no port; `FlowEdgesOf` reads the `Connect{Port}` FLOW edges whose BOTH endpoints are in the system's port set (the `IfcRelConnectsPorts` projection, deduped on the unordered port pair so a connection materialized from either incident port rides one edge, carrying the optional realizing fitting from the `Connect.Realizing`); and `ServedOf` reads the group's incident `Generic` edges whose wire-name is `IfcRelKind.ServicesBuildings.Key` (the served spatial structures riding the neutral passthrough [NEUTRAL_EDGE_RULING]); the `Identity` fold derives the `(MembershipKey, TopologyKey)` `UInt128` pair through the kernel seed-zero `Rasm.Domain.ContentHash.Of` — `MembershipKey` over the ordered member `NodeId` set and `TopologyKey` over the sorted flow-edge unordered port pairs — so a consumer re-walks only a changed membership or a changed adjacency, the single seed-zero hasher the seam `NodeId`/`ContentAddress` also compose, never a second hasher [H7].
- Receipt: the `Seq<DistributionSystem>` is the connectivity evidence the `[02]-[SYSTEM_TRACE]` `SystemTrace` fold walks and the `[03]-[INTERFERENCE]` clash pairs the distribution members from; the `Model/zones#ZONE_GRAPH` MEP grouping reads the member set by reference; the air-handling system, the water riser, and the electrical board each carry their member set, their nested sub-circuits, their typed port adjacency, and their served structures on one record.
- Packages: Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new distribution discipline is one `DistributionSystemKind` row reading the next `IfcDistributionSystemEnum` token with its `IfcDomain` AND `FlowMedium` columns (the medium the demand/sizing consumers partition on — air, water, gas, liquid, electricity, signal, solid); a new flow direction is one `FlowDirection` row with its `Emits`/`Receives` orientation; a new port-containment or membership relationship rides the existing seam edge kinds the fold already reads; never a per-discipline system record, never a second connectivity store, and never a per-relationship connection class.
- Boundary: `DistributionSystem` is ONE derived view discriminated by the `DistributionSystemKind` row data — an `HvacSystem`/`ElectricalSystem`/`PlumbingSystem` class family or sibling per-discipline factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the retired `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) is GONE — it mirrored the typed `IfcRel*` cases the seam's neutral `Connect` algebra collapsed [NEUTRAL_EDGE_RULING], and re-introducing a typed connection union is the named drift, the connectivity reading the neutral edges as the `IfcRelKind` roster actually lands them — `Connect{Port}` the port-to-port FLOW edge, `Generic("IfcRelConnectsPortToElement")` + `Compose{Nest}` the two port-OWNERSHIP containment forms — and an ownership read against an edge kind the roster never emits (the prior `Connect{Port}`-ownership probe, dead against the `EdgeAxis.Generic` row) or one that drops every IFC4-nested port is the deleted illusory read; the retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold is GONE and a `GeometryGym.Ifc` import crossing this owner is the named seam violation — `Projection/semantic#SEMANTIC_PROJECTOR` is the sole IFC lowering and this owner reads the resulting seam graph; identity is the seam `NodeId` and a `GlobalId`-keyed view is the deleted form (the `GlobalId` is the node `ExternalId` the IFC-keyed consumers read); the served-structure set rides the `Generic("IfcRelServicesBuildings")` passthrough; the distribution-domain element selection is the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Electrical)`/`ByDomain(IfcDomain.Plumbing)` predicate and a parallel system-element selection arm is the no-second-selection-surface reject; the connectivity graph is the orthogonal companion to the `Model/zones#ZONE_GRAPH` logical membership — the zone overlay owns which elements belong, the connectivity owns how they connect, the two never collapsed; the `(MembershipKey, TopologyKey)` identity is the `Rasm.Domain.ContentHash.Of` seed-zero key and a second identity scheme is the named drift defect [H7].

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using LanguageExt;
using Rasm.Bim.Projection;   // PortsOf/ServedOf compose the Projection/relations#RELATION_ALGEBRA IfcRelKind wire-name rows
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The port flow orientation over IfcFlowDirectionEnum: Emits/Receives drive the directed trace
// (a Source emits flow outward, a Sink receives it, NotDefined conducts both ways so an unoriented
// port never severs reachability). Resolved from the port node's effective FlowDirection property.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class FlowDirection {
    public static readonly FlowDirection Source        = new("SOURCE",        emits: true,  receives: false);
    public static readonly FlowDirection Sink          = new("SINK",          emits: false, receives: true);
    public static readonly FlowDirection SourceAndSink = new("SOURCEANDSINK", emits: true,  receives: true);
    public static readonly FlowDirection NotDefined    = new("NOTDEFINED",    emits: true,  receives: true);

    public bool Emits { get; }
    public bool Receives { get; }

    // Trim only — the KeyMemberEqualityComparer is the single case arbiter; a local ToUpperInvariant re-derives it.
    public static FlowDirection Of(string? token) => TryGet(token?.Trim(), out FlowDirection? direction) ? direction : NotDefined;
}

// The physical carrier a system kind implies — the MEDIUM axis a sizing/demand consumer partitions on beyond the
// discipline (a duct main and a chilled-water main are both HvacFire; only the medium separates the airflow
// accumulation from the hydronic one): Air ducted/vented air, Water piped hydronic/domestic/drainage, Gas fuel
// gas/refrigerant/compressed-service, Liquid fuel oil/chemical/process, Electricity power and traction current,
// Signal data/communication/control, Solid waste conveyance, None the carrier-free residue (safety, conveying
// plant, the unclassified fallbacks).
[SmartEnum<string>]
public sealed partial class FlowMedium {
    public static readonly FlowMedium Air         = new("air");
    public static readonly FlowMedium Water       = new("water");
    public static readonly FlowMedium Gas         = new("gas");
    public static readonly FlowMedium Liquid      = new("liquid");
    public static readonly FlowMedium Electricity = new("electricity");
    public static readonly FlowMedium Signal      = new("signal");
    public static readonly FlowMedium Solid       = new("solid");
    public static readonly FlowMedium None        = new("none");
}

// The FULL IfcDistributionSystemEnum distribution vocabulary (decompile-verified against GeometryGymIFC_Core
// 25.7.30), each row carrying its IfcDomain partition the consumer reads off `system.Kind.Domain` to select a
// discipline's systems AND its FlowMedium carrier the demand/sizing consumers partition on. The kind resolves off
// the system group node's PredefinedType token (the seam-carried value-object); an unmapped/unknown token lowers
// NotDefined. Domain grouping: air/thermal-fluid/combustion-fuel/fire-and-life-safety on HvacFire, piped
// water/drainage/waste/process on Plumbing, power/lighting/telecom/data/signal/rail-traction on Electrical,
// conveying + the UserDefined/NotDefined fallbacks on the generic Architecture (an unclassified system never
// pollutes a discipline's ByDomain selection) — the closed buildingSMART roster.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DistributionSystemKind {
    public static readonly DistributionSystemKind AirConditioning          = new("AIRCONDITIONING",             IfcDomain.HvacFire,   FlowMedium.Air);
    public static readonly DistributionSystemKind Ventilation              = new("VENTILATION",                 IfcDomain.HvacFire,   FlowMedium.Air);
    public static readonly DistributionSystemKind Vent                     = new("VENT",                        IfcDomain.HvacFire,   FlowMedium.Air);
    public static readonly DistributionSystemKind Exhaust                  = new("EXHAUST",                     IfcDomain.HvacFire,   FlowMedium.Air);
    public static readonly DistributionSystemKind ChilledWater             = new("CHILLEDWATER",                IfcDomain.HvacFire,   FlowMedium.Water);
    public static readonly DistributionSystemKind CondenserWater           = new("CONDENSERWATER",              IfcDomain.HvacFire,   FlowMedium.Water);
    public static readonly DistributionSystemKind CompressedAir            = new("COMPRESSEDAIR",               IfcDomain.HvacFire,   FlowMedium.Gas);
    public static readonly DistributionSystemKind Heating                  = new("HEATING",                     IfcDomain.HvacFire,   FlowMedium.Water);
    public static readonly DistributionSystemKind Refrigeration            = new("REFRIGERATION",               IfcDomain.HvacFire,   FlowMedium.Gas);
    public static readonly DistributionSystemKind FireProtection           = new("FIREPROTECTION",              IfcDomain.HvacFire,   FlowMedium.Water);
    public static readonly DistributionSystemKind Vacuum                   = new("VACUUM",                      IfcDomain.HvacFire,   FlowMedium.Air);
    public static readonly DistributionSystemKind Fuel                     = new("FUEL",                        IfcDomain.HvacFire,   FlowMedium.Liquid);
    public static readonly DistributionSystemKind Gas                      = new("GAS",                         IfcDomain.HvacFire,   FlowMedium.Gas);
    public static readonly DistributionSystemKind Oil                      = new("OIL",                         IfcDomain.HvacFire,   FlowMedium.Liquid);
    public static readonly DistributionSystemKind Safety                   = new("SAFETY",                      IfcDomain.HvacFire,   FlowMedium.None);
    public static readonly DistributionSystemKind DomesticColdWater        = new("DOMESTICCOLDWATER",           IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind DomesticHotWater         = new("DOMESTICHOTWATER",            IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind WaterSupply              = new("WATERSUPPLY",                 IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind Drainage                 = new("DRAINAGE",                    IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind Sewage                   = new("SEWAGE",                      IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind RainWater                = new("RAINWATER",                   IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind StormWater               = new("STORMWATER",                  IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind WasteWater               = new("WASTEWATER",                  IfcDomain.Plumbing,   FlowMedium.Water);
    public static readonly DistributionSystemKind MunicipalSolidWaste      = new("MUNICIPALSOLIDWASTE",         IfcDomain.Plumbing,   FlowMedium.Solid);
    public static readonly DistributionSystemKind Disposal                 = new("DISPOSAL",                    IfcDomain.Plumbing,   FlowMedium.Solid);
    public static readonly DistributionSystemKind Chemical                 = new("CHEMICAL",                    IfcDomain.Plumbing,   FlowMedium.Liquid);
    public static readonly DistributionSystemKind Hazardous                = new("HAZARDOUS",                   IfcDomain.Plumbing,   FlowMedium.Liquid);
    public static readonly DistributionSystemKind Electrical               = new("ELECTRICAL",                  IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind Lighting                 = new("LIGHTING",                    IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind PowerGeneration          = new("POWERGENERATION",             IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind Earthing                 = new("EARTHING",                    IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind LightningProtection      = new("LIGHTNINGPROTECTION",         IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind Telephone                = new("TELEPHONE",                   IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Data                     = new("DATA",                        IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Communication            = new("COMMUNICATION",               IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind AudioVisual              = new("AUDIOVISUAL",                 IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind ElectroAcoustic          = new("ELECTROACOUSTIC",             IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Television               = new("TV",                          IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Signal                   = new("SIGNAL",                      IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Control                  = new("CONTROL",                     IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Security                 = new("SECURITY",                    IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind Operational              = new("OPERATIONAL",                 IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind OperationalTelephony     = new("OPERATIONALTELEPHONYSYSTEM",  IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind MobileNetwork            = new("MOBILENETWORK",               IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind MonitoringSystem         = new("MONITORINGSYSTEM",            IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind FixedTransmissionNetwork = new("FIXEDTRANSMISSIONNETWORK",    IfcDomain.Electrical, FlowMedium.Signal);
    public static readonly DistributionSystemKind CatenarySystem           = new("CATENARY_SYSTEM",             IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind OverheadContactLine      = new("OVERHEAD_CONTACTLINE_SYSTEM", IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind ReturnCircuit            = new("RETURN_CIRCUIT",              IfcDomain.Electrical, FlowMedium.Electricity);
    public static readonly DistributionSystemKind Conveying                = new("CONVEYING",                   IfcDomain.Architecture, FlowMedium.None);
    public static readonly DistributionSystemKind UserDefined              = new("USERDEFINED",                 IfcDomain.Architecture, FlowMedium.None);
    public static readonly DistributionSystemKind NotDefined               = new("NOTDEFINED",                  IfcDomain.Architecture, FlowMedium.None);

    public IfcDomain Domain { get; }
    public FlowMedium Medium { get; }

    public static DistributionSystemKind Of(string? token) => TryGet(token?.Trim(), out DistributionSystemKind? kind) ? kind : NotDefined;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The derived port view — the seam IfcDistributionPort Object node read with its owner element and flow
// direction folded in. Kind is the port node's PredefinedType (CABLE/CABLECARRIER/DUCT/PIPE/WIRELESS).
public sealed record DistributionPort(NodeId Id, string Name, FlowDirection Flow, PredefinedType Kind, NodeId Owner);

// One port-to-port flow connection (the IfcRelConnectsPorts projection) carrying the optional realizing fitting.
public readonly record struct FlowEdge(NodeId From, NodeId To, Option<NodeId> Realizing);

public sealed record DistributionSystem(
    NodeId Id,
    Option<string> ExternalId,
    string Name,
    DistributionSystemKind Kind,
    Seq<NodeId> Members,
    Seq<NodeId> Circuits,
    Seq<DistributionPort> Ports,
    Seq<FlowEdge> Flow,
    Seq<NodeId> Served) {
    // The content-key identity the trace re-reads the network by — MembershipKey over the ordered member ids,
    // TopologyKey over the sorted unordered flow-edge port pairs, both through the kernel seed-zero ContentHash
    // [H7]. Ordinal ordering throughout: the culture-sensitive Comparer<string>.Default would fork the SAME
    // network's key across machines, breaking the memoization contract a content key exists to hold.
    public (UInt128 MembershipKey, UInt128 TopologyKey) Identity => (
        ContentHash.Of(Encoding.UTF8.GetBytes(string.Join(",", Members.Map(static m => m.Value).Order(StringComparer.Ordinal)))),
        ContentHash.Of(Encoding.UTF8.GetBytes(string.Join(";", Flow
            .Map(static f => string.CompareOrdinal(f.From.Value, f.To.Value) <= 0 ? $"{f.From.Value}-{f.To.Value}" : $"{f.To.Value}-{f.From.Value}")
            .Order(StringComparer.Ordinal)))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class DistributionNetwork {
    // The system-group classification codes the seam Object node carries (the projector stamps the IFC entity
    // type as the generic Classification("ifc", code)); a group node outside this set is the zones overlay's.
    private static readonly FrozenSet<string> SystemClasses =
        new[] { "IfcDistributionSystem", "IfcDistributionCircuit", "IfcSystem", "IfcBuiltSystem" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private const string PortClass = "IfcDistributionPort";
    private static readonly PropertyName FlowKey = PropertyName.Create("FlowDirection");

    // ONE reader over ONE scope value: an absent scope folds every distribution-system group in the model, a
    // present NodeId folds that one group (empty when the id resolves no system-classified Object) — the modality
    // is the Option value itself, never a per-arity View overload pair and never a ViewAll/ViewBy name family.
    // Total over the consistent graph.
    public static Seq<DistributionSystem> View(ElementGraph graph, Option<NodeId> scope = default) =>
        scope.Match(
            None: () => graph.ObjectNodes.Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o)),
            Some: id => graph.Find<Node.Object>(id).Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o)).ToSeq());

    private static DistributionSystem Of(ElementGraph graph, Node.Object system) {
        Seq<NodeId> members = MembersOf(graph, system.Id);
        // Nested sub-circuits: the members that are themselves system groups (IfcDistributionCircuit under its
        // parent board) — each also folds as its own DistributionSystem row through View.
        Seq<NodeId> circuits = members.Filter(m => graph.Find<Node.Object>(m).Map(static o => SystemClasses.Contains(o.Classification.Code)).IfNone(false));
        Seq<DistributionPort> ports = members.Bind(m => PortsOf(graph, m));
        Seq<FlowEdge> flow = FlowEdgesOf(graph, toHashSet(ports.Map(static p => p.Id)));
        return new DistributionSystem(
            system.Id, system.ExternalId, system.Name,
            DistributionSystemKind.Of(system.PredefinedType.Token),
            members, circuits, ports, flow, ServedOf(graph, system.Id));
    }

    // The grouped members: the system's incident Assign{Group} edges with the system PINNED as the Definition
    // endpoint (the projector's INVERTED Assign — Subject = member, Definition = group, the same directional pin
    // the zones MembersOf holds), so a nested system's OWN membership in a parent group (the system as Subject)
    // never folds its parent into its member set — the either-endpoint read that did was the deleted form.
    private static Seq<NodeId> MembersOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Definition == system
                ? Some(a.Subject) : None);

    // A member's ports over BOTH ownership forms the projector lands — the Generic("IfcRelConnectsPortToElement")
    // passthrough (the IFC2x3 containment; its IfcRelKind row rides EdgeAxis.Generic with the PORT relating) and
    // the Compose{Nest} edge whose part is a port (IfcRelNests, the IFC4 port containment) — deduped on the port
    // id for a belt-and-braces file carrying both. Connect{Port} is the port-to-port FLOW edge, never ownership.
    private static Seq<DistributionPort> PortsOf(ElementGraph graph, NodeId member) =>
        toSeq(graph.EdgesAt(member))
            .Choose(e => e switch {
                Relationship.Generic g when string.Equals(g.WireName, IfcRelKind.ConnectsPortToElement.Key, StringComparison.Ordinal) && g.Related == member
                    => PortNode(graph, g.Relating),
                Relationship.Compose n when n.SubKind == ComposeKind.Nest && n.Whole == member => PortNode(graph, n.Part),
                _ => Option<Node.Object>.None,
            })
            .Map(port => new DistributionPort(port.Id, port.Name, PortFlow(graph, port.Id), port.PredefinedType, member))
            .DistinctBy(static p => p.Id)
            .ToSeq();

    private static Option<Node.Object> PortNode(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Filter(static o => string.Equals(o.Classification.Code, PortClass, StringComparison.OrdinalIgnoreCase));

    // The flow edges: Connect{Port} edges whose BOTH endpoints are ports of this system (the IfcRelConnectsPorts
    // projection), read through each port's incidence (EdgesAt, O(port-degree)) NOT a whole-graph Edges rescan (the
    // seam incidence-index law View/MembersOf/PortsOf/ServedOf hold), then deduped on the unordered port pair so a
    // connection incident to both its ports (visited twice) rides one edge.
    private static Seq<FlowEdge> FlowEdgesOf(ElementGraph graph, LanguageExt.HashSet<NodeId> ports) =>
        ports.ToSeq()
            .Bind(port => toSeq(graph.EdgesAt(port)))
            .Choose(e => e is Relationship.Connect c && c.SubKind == ConnectKind.Port && ports.Contains(c.From) && ports.Contains(c.To)
                ? Some(new FlowEdge(c.From, c.To, c.Realizing)) : None)
            .DistinctBy(static f => string.CompareOrdinal(f.From.Value, f.To.Value) <= 0 ? (f.From.Value, f.To.Value) : (f.To.Value, f.From.Value))
            .ToSeq();

    // The served spatial structures via the Generic passthrough — IfcRelServicesBuildings has no neutral case, so
    // it rides Generic carrying its wire-name, the relating side the system.
    private static Seq<NodeId> ServedOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Generic g && string.Equals(g.WireName, IfcRelKind.ServicesBuildings.Key, StringComparison.Ordinal) && g.Relating == system
                ? Some(g.Related) : None);

    // The port flow direction off the port node's neutral "FlowDirection" property — the direct IFC attribute
    // IfcDistributionPort.FlowDirection has NO seam Node.Object home (the seam carries no flow column), so the directed
    // trace depends on the Projection/semantic#SEMANTIC_PROJECTOR surfacing that attribute as a "FlowDirection" entry on
    // a port PropertySet bag at ingest; absent that surfacing every port reads NotDefined (conducts both ways), so a
    // model without the lowered direction traces undirected rather than faulting — the directed orientation degrades to
    // reachability, never an error, and a host-neutral systems view never reads the GeometryGym attribute itself.
    private static FlowDirection PortFlow(ElementGraph graph, NodeId port) =>
        toSeq(graph.EdgesAt(port))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition && a.Subject == port
                ? graph.Find<Node.PropertySet>(a.Definition) : Option<Node.PropertySet>.None)
            .Choose(static ps => ps.Bag.Find(FlowKey))
            .Head
            .Match(Some: static v => FlowDirection.Of(v.Render()), None: static () => FlowDirection.NotDefined);
}
```

## [03]-[SYSTEM_TRACE]

- Owner: `SystemTrace` the reachability fold over one `DistributionSystem` view's port-and-element flow graph — the set of every distribution element reachable from a seed port or element through the connection network, folded by the shared `QuikGraph` `[GRAPH_ALGORITHM]` owner the `Planning/schedule#CRITICAL_PATH` topological order and the `Review/versioning#VERSION_GRAPH` common-ancestor walk also compose, never a hand-rolled visited-set walk; `TraceMode` the orientation policy (`Reach` the undirected both-directions closure, `Downstream`/`Upstream` the `FlowDirection`-oriented directed closure). The flow network is a graph over BOTH ports AND elements so the closure crosses each fitting (a tee's inlet port → the tee element → the tee's outlet ports → the next segment), the bipartite-style traversal the port-only adjacency the retired walk built never crossed.
- Entry: `SystemTrace.From(DistributionSystem system, NodeId seed, TraceMode mode)` folds one explicit orientation over a transient `AdjacencyGraph<NodeId, SEdge<NodeId>>`, accumulates the closure through `BreadthFirstSearchAlgorithm.DiscoverVertex`, and partitions reached elements from reached ports. `SystemTrace.Demand(ElementGraph graph, ValueSource source, Op key)` reduces reached effective values through `ElementSet.SumOf`. `SystemTrace.Runs(ElementGraph graph, Option<ValueSource> resistance, Op key)` ranks every reached TERMINAL (a reached vertex with no outgoing oriented leg) by its best-route cost from the seed over the SAME retained oriented adjacency — the `ShortestPathsDijkstra` route fold, each element weighing its effective resistance value (segment length, fitting loss — a present non-measure faults the same `aggregate-non-measure` law `SumOf` rails) with the hop-count identity when no source is named — descending, so the head IS the index run the duct-sizing, riser-diagram, and feeder-schedule reads start from; in a tree-shaped run the best route is the only route, so the ranking is exact, and a ring main ranks each terminal by its least-resistance route. An isolated system member yields itself; a seed outside the system yields an empty trace, so construction remains total without fabricating membership.
- Auto: `From` folds the system's `Ports` into the graph as ownership edges (each port ↔ its owner element, both directions — a port belongs to its element regardless of flow) and the system's `Flow` edges oriented by `TraceMode` over BOTH endpoint `FlowDirection`s (`Reach` adds both directions; a `Downstream` leg exists only where the emitting side `Emits` AND the receiving side `Receives`, so a `Source`→`Sink` edge carries one leg, a `NotDefined` pair conducts both ways, a `NotDefined`-against-`Source` edge still flows OUT of the source only, and two facing pure sources honestly sever the directed closure; `Upstream` is the mirror), the optional realizing fitting linked as an intermediate on each ORIENTED leg (emitting port → realizer → receiving port, so `Reach` keeps its bidirectional participation while a directed trace never crosses a connection backwards through its fitting); the reached closure accumulates through the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold — O(reachable), the seed discovered first, the same accumulation form the `Model/spatial#SPATIAL_STRUCTURE` `Reachable` and the emitter `DomainAtlas` claim hold, never an all-vertex `TryFunc` path-probe sweep re-recovering a path per vertex — and the fold partitions the reached vertices into the non-port reached elements and the reached ports by the port-set membership — the directed `Downstream` trace from an air-handling unit reaching every air terminal it feeds, the `Reach` trace from a shutoff valve reaching every fixture on its branch, both one fold over the QuikGraph adjacency; the trace reads the `DistributionSystem` view (one hop — the view already carries the ports with their `FlowDirection` and the deduped flow edges), never re-reading the seam graph or re-resolving the port flow per query, and a consumer memoizes the trace against the owning system's `(MembershipKey, TopologyKey)` `Identity` so a re-trace re-folds only on a changed membership or adjacency.
- Receipt: the `SystemTrace` reached-element `Seq<NodeId>` is the downstream-network evidence the `Model/zones#ZONE_GRAPH` MEP grouping reads to resolve a system's effective member closure and the `Model/query#ELEMENT_SET` consumers intersect against a domain set — a "every air terminal fed from this air-handling unit" / "every fixture downstream of this shutoff valve" query is one `From` fold over the flow graph, the connectivity the single-membership zone overlay cannot express; `Demand` turns the same closure into the quantified network evidence the discipline actually sizes from — the accumulated terminal airflow behind a duct main, the fixture units on a riser branch, the connected load behind a feeder, each one railed `SumOf` reduction partitioned by the owning `Kind.Medium` — declared-property aggregation the `Rasm.AppUi` schedules and a `Rasm.Compute` sizing check consume without re-deriving connectivity; the ranked `Seq<SystemRun>` is the index-run evidence the same consumers read — each row the terminal, its route `NodeId` chain, and its accumulated resistance, the head row the hydraulically-critical run — declared-property ranking, never a pressure solve (the solve stays `Rasm.Compute`'s); the reached set is consumed by the `zoning`/`query`/`analysis` peers by reference, never re-derived per consumer.
- Packages: QuikGraph, Rasm.Element, Rasm (the kernel `Op` the `Demand` reduction threads), LanguageExt.Core
- Growth: a new trace orientation is one `TraceMode` row carrying its `Symmetric`/`Reverse` data the `Orient` fold reads off the same `FlowDirection`; a new reachability guard (stop at a controller, stop at a discipline boundary) is one filter on the edge fold; a new graph query (shortest flow path, connected components) rides the SAME `QuikGraph` `AlgorithmExtensions` facade over the same adjacency; never a per-direction trace record, never a second adjacency store, and never a per-discipline traversal.
- Boundary: the `SystemTrace` is ONE reachability fold over the shared `QuikGraph` `AdjacencyGraph` — the retired hand-rolled `SystemNetwork`/`Closure` visited-set tail-recursion is the deleted form, the `[GRAPH_ALGORITHM]` owner the whole stack folds a transient graph into rather than re-implementing a walk (the api-quikgraph `BreadthFirstSearchAlgorithm` event-fold law), and a `Map<>`/`HashSet<>` adjacency with a mutated visited set is the named drift; a `TraceHvac`/`TraceElectrical`/`TracePlumbing` operation family is the deleted form per the no-operation-family law, the discipline already carried by the system's `Kind` the trace folds within; the trace carries no `Fin<T>` rail because the closed graph is total (the dangling-endpoint rejection lowered at `Project`); the trace reads the `DistributionSystem` view ONE HOP and a re-read of the seam graph or a re-resolution of the port `FlowDirection` per query is the named cross-page drift; the directed orientation reads the port `FlowDirection` the view carries and an `AdjacencyGraph` with no orientation policy is the no-modality reject; a consumer memoizes against the system `Identity` and a second identity scheme is the named drift defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The trace orientation policy: Reach is the undirected both-directions closure (the flow component);
// Downstream/Upstream orient each flow edge by BOTH endpoint FlowDirections through the Symmetric/Reverse
// data columns Orient reads — an unoriented (NotDefined) pair stays bidirectional, so an unoriented network
// degrades to reachability rather than severing the trace. The data columns drive Orient with no
// runtime-silent arm; a new orientation is one row carrying its Symmetric/Reverse data.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TraceMode {
    public static readonly TraceMode Reach      = new("reach",      symmetric: true,  reverse: false);
    public static readonly TraceMode Downstream = new("downstream", symmetric: false, reverse: false);
    public static readonly TraceMode Upstream   = new("upstream",   symmetric: false, reverse: true);

    public bool Symmetric { get; }
    public bool Reverse { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// The terminal-run row the index-run ranking yields: the reached terminal, its best route from the seed, and the
// accumulated resistance — the head of the descending ranking IS the index run.
public sealed record SystemRun(NodeId Terminal, Seq<NodeId> Route, double Cost);

public sealed record SystemTrace(NodeId Seed, TraceMode Mode, Seq<NodeId> ReachedElements, Seq<NodeId> ReachedPorts) {
    // The oriented adjacency retained from From — the Runs route fold reads the SAME network the closure walked,
    // never a re-derived orientation; reference-identity state, excluded from the record's value semantics.
    internal AdjacencyGraph<NodeId, SEdge<NodeId>> Network { get; init; } = new(allowParallelEdges: true);

    public static SystemTrace From(DistributionSystem system, NodeId seed, TraceMode mode) {
        LanguageExt.HashSet<NodeId> ports = toHashSet(system.Ports.Map(static p => p.Id));
        Map<NodeId, FlowDirection> flowByPort = system.Ports.Fold(Map<NodeId, FlowDirection>(), static (map, port) => map.AddOrUpdate(port.Id, port.Flow));
        AdjacencyGraph<NodeId, SEdge<NodeId>> graph = new(allowParallelEdges: true);
        graph.AddVertexRange(system.Members);
        graph.AddVertexRange(ports);
        // Ownership: every port is part of its element both ways (the view's Owner column, containment-form-agnostic).
        foreach (DistributionPort port in system.Ports) { Link(graph, port.Id, port.Owner); Link(graph, port.Owner, port.Id); }
        // Flow: oriented by mode + BOTH endpoint FlowDirections; the realizing fitting rides each ORIENTED leg as an
        // intermediate (emitting port -> realizer -> receiving port), so Reach keeps its bidirectional participation
        // while a directed trace can never escape backwards through a fitting onto the source side.
        foreach (FlowEdge edge in system.Flow) {
            (FlowDirection fromFlow, FlowDirection toFlow) = (flowByPort.Find(edge.From).IfNone(FlowDirection.NotDefined), flowByPort.Find(edge.To).IfNone(FlowDirection.NotDefined));
            foreach ((NodeId from, NodeId to) in Orient(edge.From, edge.To, fromFlow, toFlow, mode)) {
                Link(graph, from, to);
                edge.Realizing.Iter(realizing => { Link(graph, from, realizing); Link(graph, realizing, to); });
            }
        }
        if (!graph.ContainsVertex(seed)) { return new SystemTrace(seed, mode, Seq<NodeId>(), Seq<NodeId>()) { Network = graph }; }
        // The reached closure through the BreadthFirstSearchAlgorithm DiscoverVertex event fold — O(reachable),
        // the seed discovered first; the all-vertex TryFunc path-probe sweep re-recovering a path per vertex is
        // the deleted form the spatial view names.
        BreadthFirstSearchAlgorithm<NodeId, SEdge<NodeId>> search = new(graph);
        Seq<NodeId> reached = Seq<NodeId>();
        search.DiscoverVertex += v => reached = reached.Add(v);
        search.Compute(seed);
        return new SystemTrace(seed, mode, reached.Filter(v => !ports.Contains(v)), reached.Filter(ports.Contains)) { Network = graph };
    }

    // The INDEX-RUN ranking: every reached terminal — a reached vertex with no outgoing oriented leg — ranked by its
    // best-route cost from the seed over the SAME retained adjacency through the QuikGraph ShortestPathsDijkstra
    // fold, descending, so the head IS the index run. The per-vertex weight is the element's effective resistance
    // value for the caller-chosen source (segment length, fitting loss) with the hop-count identity when None is
    // named or the element carries no value; a PRESENT non-measure faults the same aggregate-non-measure law SumOf
    // rails, never a silently-skipped weight. Declared-property ranking, never a pressure solve — the solve stays
    // Rasm.Compute's; in a tree the best route is the only route, a ring main ranks by least-resistance route.
    public Fin<Seq<SystemRun>> Runs(ElementGraph graph, Option<ValueSource> resistance, Op key) =>
        ReachedElements
            .TraverseM(id => resistance
                .Bind(source => graph.Find<Node.Object>(id).Map(o => (Object: o, Source: source)))
                .Match(
                    None: () => FinSucc((Id: id, Weight: 1.0)),
                    Some: row => ElementSet.ValuesOf(graph, row.Object, row.Source)
                        .TraverseM(value => value is PropertyValue.Measure measure
                            ? FinSucc(measure.Value.Si)
                            : FinFail<double>(ElementFault.ValueRejected(key, $"<aggregate-non-measure:{value.GetType().Name}>")))
                        .As()
                        .Map(weights => (Id: id, Weight: weights.IsEmpty ? 1.0 : weights.Sum()))))
            .As()
            .Map(rows => {
                Map<NodeId, double> weights = rows.Fold(Map<NodeId, double>(), static (map, row) => map.AddOrUpdate(row.Id, row.Weight));
                TryFunc<NodeId, IEnumerable<SEdge<NodeId>>> routes =
                    Network.ShortestPathsDijkstra(edge => weights.Find(edge.Target).IfNone(0.0), Seed);
                return ReachedElements
                    .Filter(Network.IsOutEdgesEmpty)
                    .Choose(terminal => routes(terminal, out var path) && toSeq(path) is var legs
                        ? Some(new SystemRun(terminal, legs.Map(static e => e.Target), legs.Map(e => weights.Find(e.Target).IfNone(0.0)).Sum()))
                        : Option<SystemRun>.None)
                    .OrderByDescending(static run => run.Cost)
                    .ToSeq();
            });

    // The downstream-DEMAND accumulation: the reached elements' effective design values for one source — terminal
    // airflow, fixture units, connected load — reduced through the ONE Model/query#ELEMENT_SET SumOf composition
    // (the seam same-type MeasureValue.Sum under the dimension law), so a duct main's accumulated terminal demand
    // is one directed trace plus one railed sum, partitioned by the owning Kind.Medium carrier; None when no
    // reached element carries the source. Declared-property AGGREGATION, never a solve — sizing stays Rasm.Compute's.
    public Fin<Option<MeasureValue>> Demand(ElementGraph graph, ValueSource source, Op key) =>
        ElementSet.SumOf(graph, ReachedElements, source, key);

    // The flow-edge orientation reads the mode's Symmetric/Reverse data over BOTH endpoint directions: Reach is
    // symmetric (both legs); a directed leg exists only where the emitting side Emits AND the receiving side
    // Receives, Reverse mirroring it — so a NotDefined port (Emits && Receives) degrades to bidirectional against
    // another NotDefined, a NotDefined-against-Source edge still carries flow OUT of the source only, and two
    // facing pure sources (a modeling contradiction) honestly sever the directed trace while Reach still spans
    // them. No mode switch and no runtime-silent arm: the booleans drive the orientation directly.
    private static Seq<(NodeId From, NodeId To)> Orient(NodeId from, NodeId to, FlowDirection fromFlow, FlowDirection toFlow, TraceMode mode) =>
        mode.Symmetric
            ? Seq((from, to), (to, from))
            : (fromFlow.Emits && toFlow.Receives ? Seq(mode.Reverse ? (to, from) : (from, to)) : Seq<(NodeId, NodeId)>())
            + (toFlow.Emits && fromFlow.Receives ? Seq(mode.Reverse ? (from, to) : (to, from)) : Seq<(NodeId, NodeId)>());

    private static void Link(AdjacencyGraph<NodeId, SEdge<NodeId>> graph, NodeId from, NodeId to) =>
        graph.AddVerticesAndEdge(new SEdge<NodeId>(from, to));
}
```

## [04]-[INTERFERENCE]

- Owner: `Interference` the host-neutral clash-evidence record carrying the clashing `(NodeId, NodeId)` pair, the `ClashKind` (`Hard` overlapping solids, `Clearance` insufficient maintenance/insulation gap), the measured deficit (the penetration depth for a hard clash, the clearance shortfall for a clearance clash, both kernel-SI scalars), the two member disciplines (`IfcDomain` pair), and the priority rank a cross-discipline clash carries above an intra-discipline one; `ClashKind` the closed `[SmartEnum<string>]` clash partition; `InterferenceQuery` the proximity request keyed by the two members' `RepresentationContentHash` body geometry content keys plus the clearance threshold, the host-neutral systems owner producing the request and reading the scalar deficit back, the kernel `Rasm` geometry owner resolving the content-keyed geometry and evaluating the solid intersection; `GeometryProximity` the injected kernel PORT — a `readonly record struct` of two decode legs (`Bounds` the content-keyed `(Vector3 Min, Vector3 Max)` AABB, `Test` the precise `Fin<ProximityResult>` signed gap), the SAME app-wired resolver shape the seam `Graph/element#NODE_MODEL` `GeometrySource` holds, never an interface floor for a two-arrow contract; `InterferenceCheck` the fold pairing the distribution-run geometry against itself (cross-system) and against the static obstruction set — every other body-carrying occurrence, the Architecture-domain built elements included.
- Cases: `ClashKind` rows `Hard` (overlapping solids, deficit = penetration depth) · `Clearance` (gap below the maintenance/insulation envelope, deficit = threshold − gap) (2); an `Interference` carries the ordered member `NodeId` pair, the `ClashKind`, the SI deficit, and the discipline `IfcDomain` pair, a cross-discipline clash (`FirstDomain != SecondDomain`) ranking above an intra-discipline one through the `DisciplineWeight` ordering offset.
- Entry: `InterferenceCheck.Interferences(ElementGraph graph, GeometryProximity proximity, Op key)` resolves every occurrence bound through the injected `Fin` rail, expands each broad-phase `BoundVolume` by the wider of `GeometryProximity.MinimumClearance` and the member property envelope, and admits run-to-run or run-to-static candidates before the precise signed-gap test. Missing geometry aborts on the owning `BimFault`; hard and clearance evidence ranks by cross-discipline priority and deficit.
- Auto: `Interferences` traverses `GeometryProximity.Bounds`, refits or inserts clearance-expanded boxes in the selected `BroadPhase` index, deduplicates unordered `NodeId` overlaps, and traverses `GeometryProximity.Test`. `BroadPhase.Bvh` queries `SwiftBVH<NodeId>` and `BroadPhase.SpatialHash` queries `SwiftSpatialHash<NodeId>.QueryNeighborhood`; `Refit` applies moved bounds through `UpdateEntryBounds` and removals through `Remove` on the same carried index.
- Receipt: the ranked `Seq<Interference>` is the MEP coordination evidence the `Review/coordination#COORDINATION` `ClashProposal` fold consumes (the clash `NodeId` pair, kind, and deficit anchoring a proposed resolution and a BCF topic, the coordination owner resolving each member's `ExternalId` IFC `GlobalId` off the graph for the viewpoint) and the `csharp:Rasm.AppUi/Charts` clash report renders — a duct-vs-beam hard clash, a pipe-clearance violation, and a tray-vs-structure graze each carry their measured deficit and discipline pair on one host-neutral row.
- Packages: Rasm.Element, Rasm, SwiftCollections.Lean, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new clash kind is one `ClashKind` row; a new run discipline is one `RunDomains` row; a new clearance source is one `PropertyName` in `ClearanceKeys`; a new broad-phase modality is one `BroadPhase` arm binding its `Insert`/`UpdateEntryBounds`/`Remove`/`Query` surface into `Indexed` and `Refit`.
- Boundary: `GeometryProximity` resolves bounds and signed distance by content key, so RhinoCommon, GeometryGym, and tessellation remain outside this owner. `BroadPhase` carries the exact mutable `SwiftBVH<NodeId>` or `SwiftSpatialHash<NodeId>` instance plus its key registry; `Interferences` and `Refit` share that carrier, and a local throwaway index, O(N²) scan, or refit of a different instance is the deleted form. RUN/STATIC admission derives from `IfcDomain`, clearance derives from baked properties, `Interference` retains seam `NodeId` identity, and coordination consumes the ranked evidence without repeating proximity.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using SwiftCollections;
using SwiftCollections.Query;
using Thinktecture;
using Vector3 = System.Numerics.Vector3;   // the broad-phase float AABB corner; the seam Rasm.Element.Graph.Vector3 would collide unaliased
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard      = new("HARD");
    public static readonly ClashKind Clearance = new("CLEARANCE");
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The kernel proximity port resolves bounds and signed distance on one `Fin` rail. `MinimumClearance` is the
// composition policy that expands the broad-phase envelope before the precise test.
public readonly record struct InterferenceQuery(UInt128 First, UInt128 Second, double ClearanceThreshold);

public readonly record struct ProximityResult(double Gap, double ClosestApproach);

[Union]
public abstract partial record BroadPhase {
    private BroadPhase() { }

    public sealed record Bvh : BroadPhase {
        internal SwiftBVH<NodeId> Index { get; } = new(2);
        internal System.Collections.Generic.HashSet<NodeId> Keys { get; } = new();
    }

    public sealed record SpatialHash(float CellSize, int NeighborhoodPadding) : BroadPhase {
        internal SwiftSpatialHash<NodeId> Index { get; } = new(2, CellSize, new SwiftSpatialHashOptions(NeighborhoodPadding));
        internal System.Collections.Generic.HashSet<NodeId> Keys { get; } = new();
    }
}

public readonly record struct GeometryProximity(
    Func<UInt128, Fin<(Vector3 Min, Vector3 Max)>> Bounds,
    Func<InterferenceQuery, Fin<ProximityResult>> Test,
    double MinimumClearance,
    BroadPhase BroadPhase);

// --- [MODELS] -----------------------------------------------------------------------------
// One broad-phase admission row per body-carrying occurrence — the discipline, the body content key, the
// run/static side, and the member clearance envelope, resolved ONCE before any pairing.
internal readonly record struct ClashCandidate(NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance);

public sealed record Interference(
    NodeId First,
    NodeId Second,
    ClashKind Kind,
    double Deficit,
    IfcDomain FirstDomain,
    IfcDomain SecondDomain) {
    private const double DisciplineWeight = 1_000_000d;

    public bool CrossDiscipline => FirstDomain != SecondDomain;
    public double Rank => CrossDiscipline ? Deficit + DisciplineWeight : Deficit;

    public UInt128 Identity => ContentHash.Of(Encoding.UTF8.GetBytes(
        string.CompareOrdinal(First.Value, Second.Value) <= 0 ? $"{First.Value}|{Second.Value}" : $"{Second.Value}|{First.Value}"));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class InterferenceCheck {
    // The RUN/STATIC pair-admission policy: the MEP run domains select the moving side of every pair; every OTHER
    // body-carrying occurrence (the Architecture-domain beams/columns/walls, the Structural footings/piles, the
    // Infrastructure/Geotechnical set) is the static obstruction side. A pair admits when at least one member is a
    // run — MEP-vs-MEP crosses systems, MEP-vs-static catches a duct-vs-beam, static-vs-static (wall against wall)
    // never enters. A four-domain roster omitting Architecture made the duct-vs-beam clash unreachable: IfcClass
    // roots IfcBuiltElement on Architecture, Structural only via the reinforcing/footing/pile override rows.
    private static readonly FrozenSet<IfcDomain> RunDomains =
        new[] { IfcDomain.HvacFire, IfcDomain.Electrical, IfcDomain.Plumbing }.ToFrozenSet();

    // The clearance-envelope property policy: the wider of a member's insulation/maintenance properties (a new
    // clearance source is one row), read off the baked element's effective property bags — never a hardcoded table.
    private static readonly Seq<PropertyName> ClearanceKeys = Seq(
        PropertyName.Create("InsulationThickness"), PropertyName.Create("Clearance"), PropertyName.Create("MaintenanceClearance"));

    public static Fin<Seq<Interference>> Interferences(ElementGraph graph, GeometryProximity proximity, Op key) {
        // Every body-carrying OCCURRENCE enters (a Type's shared representation is un-placed geometry, never a clash
        // body); the Run flag tags the MEP side the pair admission requires, and the clearance envelope resolves
        // ONCE per member here (one Bake + bag walk) rather than once per candidate pair.
        Seq<ClashCandidate> members = graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Occurrence)
            .Choose(o => IfcClass.TryGet(o.Classification.Code)
                .Bind(c => o.Representations.Body.Map(body =>
                    new ClashCandidate(o.Id, c.Domain, body, RunDomains.Contains(c.Domain), ClearanceOf(graph, o.Id, key)))))
            .ToSeq();
        return members
            .TraverseM(member => proximity.Bounds(member.Body)
                .Map(bounds => (Member: member, Bounds: Envelope(bounds, Math.Max(proximity.MinimumClearance, member.Clearance)))))
            .As()
            .Bind(indexed => Candidates(indexed, proximity.BroadPhase)
                .TraverseM(pair => Clash(pair.A, pair.B, proximity)).As())
            .Map(static rows => rows.Somes().OrderByDescending(static clash => clash.Rank).ToSeq());
    }

    private static Seq<(ClashCandidate A, ClashCandidate B)> Candidates(
        Seq<(ClashCandidate Member, BoundVolume Bounds)> members, BroadPhase broadPhase) =>
        broadPhase.Switch(
            state: members,
            bvh: static (indexed, policy) => Indexed(
                indexed, policy.Keys, policy.Index.Insert,
                (id, bounds) => { policy.Index.UpdateEntryBounds(id, bounds); return true; }, policy.Index.Query),
            spatialHash: static (indexed, policy) => Indexed(
                indexed, policy.Keys, policy.Index.Insert, policy.Index.UpdateEntryBounds, policy.Index.QueryNeighborhood));

    private static Seq<(ClashCandidate A, ClashCandidate B)> Indexed(
        Seq<(ClashCandidate Member, BoundVolume Bounds)> members,
        System.Collections.Generic.HashSet<NodeId> keys,
        Func<NodeId, BoundVolume, bool> insert,
        Func<NodeId, BoundVolume, bool> update,
        Action<BoundVolume, ICollection<NodeId>> query) {
        Map<NodeId, (ClashCandidate Member, BoundVolume Bounds)> current =
            toMap(members.Map(static member => (member.Member.Id, member)));
        foreach ((ClashCandidate Member, BoundVolume Bounds) row in members) {
            if (keys.Contains(row.Member.Id)) { update(row.Member.Id, row.Bounds); }
            else if (insert(row.Member.Id, row.Bounds)) { keys.Add(row.Member.Id); }
        }
        System.Collections.Generic.HashSet<NodeId> seen = new();
        SwiftList<NodeId> overlaps = new();
        Seq<(ClashCandidate A, ClashCandidate B)> pairs = Seq<(ClashCandidate, ClashCandidate)>();
        foreach ((ClashCandidate Member, BoundVolume Bounds) row in members) {
            overlaps.Clear();
            seen.Clear();
            query(row.Bounds, overlaps);
            foreach (NodeId otherId in overlaps) {
                Option<(ClashCandidate Member, BoundVolume Bounds)> other = current.Find(otherId);
                if (string.CompareOrdinal(row.Member.Id.Value, otherId.Value) >= 0 || !seen.Add(otherId)) { continue; }
                pairs = other.Match(
                    Some: candidate => (row.Member.Run || candidate.Member.Run) && row.Bounds.Intersects(candidate.Bounds)
                        ? pairs.Add((row.Member, candidate.Member))
                        : pairs,
                    None: () => pairs);
            }
        }
        return pairs;
    }

    public static Unit Refit(BroadPhase broadPhase, Seq<(NodeId Id, Option<BoundVolume> Bounds)> changes) =>
        broadPhase.Switch(
            state: changes,
            bvh: static (rows, policy) => Refit(
                rows, policy.Keys, policy.Index.Insert,
                (id, bounds) => { policy.Index.UpdateEntryBounds(id, bounds); return true; }, policy.Index.Remove),
            spatialHash: static (rows, policy) => Refit(
                rows, policy.Keys, policy.Index.Insert, policy.Index.UpdateEntryBounds, policy.Index.Remove));

    private static Unit Refit(
        Seq<(NodeId Id, Option<BoundVolume> Bounds)> changes,
        System.Collections.Generic.HashSet<NodeId> keys,
        Func<NodeId, BoundVolume, bool> insert,
        Func<NodeId, BoundVolume, bool> update,
        Func<NodeId, bool> remove) =>
        changes.Iter(change => change.Bounds.Match(
            Some: bounds => { if (keys.Contains(change.Id)) { update(change.Id, bounds); } else if (insert(change.Id, bounds)) { keys.Add(change.Id); } },
            None: () => { if (remove(change.Id)) { keys.Remove(change.Id); } }));

    private static Fin<Option<Interference>> Clash(ClashCandidate a, ClashCandidate b, GeometryProximity proximity) {
        double threshold = Math.Max(proximity.MinimumClearance, Math.Max(a.Clearance, b.Clearance));
        return proximity.Test(new InterferenceQuery(a.Body, b.Body, threshold)).Map(result => Classify(a, b, result, threshold));
    }

    private static Option<Interference> Classify(ClashCandidate a, ClashCandidate b, ProximityResult result, double threshold) =>
        result.Gap < 0d
            ? Some(new Interference(a.Id, b.Id, ClashKind.Hard, -result.Gap, a.Domain, b.Domain))
        : result.Gap < threshold
            ? Some(new Interference(a.Id, b.Id, ClashKind.Clearance, threshold - result.Gap, a.Domain, b.Domain))
            : None;

    private static BoundVolume Envelope((Vector3 Min, Vector3 Max) bounds, double clearance) {
        Vector3 padding = new((float)Math.Max(0d, clearance));
        return new BoundVolume(bounds.Min - padding, bounds.Max + padding);
    }

    // The member clearance reads the wider of the insulation/maintenance envelopes off the BAKED element's
    // effective property bags through the ClearanceKeys policy (the seam owns the type->occurrence merge), an
    // absent property reading 0d so `GeometryProximity.MinimumClearance` floors it.
    private static double ClearanceOf(ElementGraph graph, NodeId member, Op key) =>
        graph.Bake(member, key).ToOption().Match(
            Some: element => ClearanceKeys
                .Choose(name => element.Properties.Choose(bag => bag.Find(name)).Head)
                .Choose(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None)
                .Fold(0d, static (max, si) => Math.Max(max, si)),
            None: static () => 0d);
}
```

## [05]-[RESEARCH]

- [SEAM_VIEW_NOT_PROJECTION]: `DistributionNetwork` reads `ElementGraph` and its neutral `Connect`/`Compose`/`Assign`/`Generic` edges. `SystemClasses` carries only catalog-confirmed `IfcDistributionSystem`, `IfcDistributionCircuit`, `IfcSystem`, and `IfcBuiltSystem`; an unverified foreign alias never enters the closed classifier.
- [PORT_CONNECTION_COLLAPSE]: the retired `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) mirrored two typed `IfcRel*` cases — exactly the seventeen-typed-`IfcRel*`-case design the seam's neutral edge algebra rejected (`ELEMENT-REBUILD-PLAN.md` §4-RT C5: the seam carries `Compose`/`Assign`/`Associate`/`Connect`/`Void` + `Generic`, the IFC relationship roster living in the Bim projector) — so the connectivity reads the neutral edges as the `IfcRelKind` roster lands them — the FLOW edge the `Relationship.Connect` (`From`/`To`/`SubKind`/`Realizing`, `ConnectKind.Port`) both of whose endpoints classify `IfcDistributionPort`, the OWNERSHIP edge one of the two containment forms — never a typed connection union; port OWNERSHIP is schema-dual — IFC2x3 binds a port to its element through `IfcRelConnectsPortToElement`, which the roster lands as a `Generic` edge carrying its wire-name with the PORT on the relating side (`RelatingPort`/`RelatedElement`), while IFC4 nests ports under their element through `IfcRelNests`, landing as `Compose{Nest}` (`IfcPort.ContainedIn` retained for back-compatibility) — so `PortsOf` reads BOTH edge kinds and dedupes on the port id: an ownership probe against `Connect{Port}` (an edge kind the roster never emits for containment) is a dead read yielding an EMPTY port set for every schema, and a `Generic`-only read silently loses every IFC4-nested port; the realizing fitting rides the `Connect.Realizing` `Option<NodeId>` (the `IfcRelConnectsPorts.RealizingElement` projection), and the unordered-pair dedupe on the `(From, To)` `NodeId` pair is the verified single-edge invariant.
- [QUIKGRAPH_TRACE]: the `SystemTrace` reachability fold is the shared `QuikGraph` `[GRAPH_ALGORITHM]` owner the `libs/csharp/.api/api-quikgraph` catalog mandates for every closure in the stack — so the retired tail-recursive `Closure` visited-set walk over a hand `Map<>`/`HashSet<>` adjacency is the named deleted form; the fold builds a transient `AdjacencyGraph<NodeId, SEdge<NodeId>>` (`AddVerticesAndEdge`, the value `SEdge<NodeId>` allocating nothing, the dense `NodeId`-keyed network) over BOTH ports and elements (the ownership edges crossing each fitting so the closure traverses a tee's inlet→element→outlets), accumulates the reached set through the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold (O(reachable), the seed discovered first — the catalog's named accumulation form, replacing the all-vertex `TryFunc` path-probe sweep whose per-vertex path recovery the spatial owner names deleted), and projects the reached vertex set back onto the reached-element/reached-port `NodeId` sets; the `TraceMode` orientation reads BOTH endpoint `FlowDirection`s (a directed leg exists only where the emitting side `Emits` AND the receiving side `Receives`) so the directed downstream/upstream closure walks the real flow path while a `NotDefined` pair stays bidirectional and a contradictory source-facing-source edge severs honestly, the same shared owner the `Planning/schedule#CRITICAL_PATH` `SourceFirstTopologicalSort` and the `Review/versioning#VERSION_GRAPH` `OfflineLeastCommonAncestor` compose, never N bespoke walks.
- [INTERFERENCE_PROXIMITY]: `.api/api-swiftcollections` confirms `SwiftBVH<TKey,TVolume>` and `SwiftSpatialHash<TKey,TVolume>` expose `Insert`, `UpdateEntryBounds`, `Remove`, and bounds queries, with `SwiftSpatialHash.QueryNeighborhood` adding the padded-cell query. `BroadPhase` carries the chosen `NodeId` index and key registry, `Interferences` refits current bounds before querying the same instance, and `Refit` applies `Option<BoundVolume>` changes as update/insert or removal; clearance expansion precedes the broad phase and only admitted RUN/STATIC overlaps reach `GeometryProximity.Test`.
- [SERVED_STRUCTURES_AND_FLOW_DIRECTION]: the served spatial-structure set rides the `Generic("IfcRelServicesBuildings")` neutral passthrough [NEUTRAL_EDGE_RULING] — `EdgeProjection.Generics` fans the relationship out (`IfcSystem.ServicesBuildings` → `RelatedBuildings`, decompile-verified on the `IfcSystem` base) onto `Generic` edges carrying the `IfcRelKind.ServicesBuildings.Key` wire-name, so the served set reads off the system node's incident `Generic` edges. The port `FlowDirection` the directed trace orients on is read off the port `Node.Object`'s neutral `"FlowDirection"` property — the direct IFC attribute `IfcDistributionPort.FlowDirection` (decompile-verified `IfcFlowDirectionEnum`, alongside `PredefinedType`/`SystemType`) carries no seam `Node.Object` column, so the directed trace DEPENDS on the `Projection/semantic#SEMANTIC_PROJECTOR` surfacing that attribute as a `"FlowDirection"` entry on a port `PropertySet` bag at ingest (a synthesized property, distinct from the `Bags` fold that lowers only real `IfcPropertySet`/`IfcElementQuantity`); absent that surfacing every port reads `NotDefined` (the port conducts both ways), so the directed trace degrades to undirected reachability rather than faulting — the orientation a correct refinement over the always-total `Reach` closure, never an illusory directed walk, the host-neutral systems view never reading the GeometryGym attribute itself.

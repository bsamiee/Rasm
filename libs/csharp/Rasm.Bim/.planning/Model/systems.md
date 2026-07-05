# [SYSTEMS_CONNECTIVITY]

The host-neutral MEP distribution-system connectivity layer is a VIEW over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second GeometryGym lowering: the `Projection/semantic#SEMANTIC_PROJECTOR` projector is the SOLE IFC owner and has already lowered every `IfcDistributionSystem`, `IfcDistributionPort`, `IfcRelConnectsPorts`, `IfcRelConnectsPortToElement`, `IfcRelNests` (the IFC4 port-containment form), `IfcRelAssignsToGroup`, and `IfcRelServicesBuildings` onto NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` edges — a `Connect{Port}` port-to-port FLOW edge, a `Generic("IfcRelConnectsPortToElement")`/`Compose{Nest}` port-OWNERSHIP pair (the IFC2x3 and IFC4 containment forms), an `Assign{Group}` membership edge, and a `Generic("IfcRelServicesBuildings")` passthrough — so this layer reads the settled graph and folds it into a typed flow-network view, a `QuikGraph` reachability trace, and a `SwiftCollections.Lean` interference clash, exactly as the `Model/query#ELEMENT_SET` query surface reads the same graph. The retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold and the `PortConnection` `[Union]` mirroring `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts` are GONE — the typed-`IfcRel*`-case shape is the deleted form the seam's neutral edge algebra collapsed [C5], the connectivity now reading the neutral edges by wire-name and endpoint classification (a flow edge joins two `IfcDistributionPort` objects; an ownership edge joins a port to its distribution element through either containment form).

The connectivity layer is the network GRAPH, orthogonal to the `Model/zones#ZONE_GRAPH` overlay that owns the LOGICAL membership (which elements belong to a system, the `Assign{Group}` edge) — the connectivity owns the typed flow ADJACENCY (how they connect, the `Connect{Port}` edge), the two coexisting and never collapsed. An air-handling system threading a hundred ducts, a domestic-water riser feeding every fixture, and an electrical distribution board powering every circuit each surface their member set, their typed port adjacency, and their served spatial structures from one `DistributionSystem` view, never a per-discipline system type. Identity is the seam `Rasm.Element/Graph/element#NODE_MODEL` `NodeId` (a rooted Guid-v7), never an IFC `GlobalId` — the compressed GlobalId is the node's `ExternalId` projection attribute the trace and interference carry only for the IFC-keyed downstream consumers. The layer is HOST-NEUTRAL: it joins nodes by `NodeId`, references geometry by the seam `RepresentationContentHash` content key, routes every solid-proximity test to the injected kernel `GeometryProximity` seam, and never carries a RhinoCommon binding or an in-process tessellation — the same seam law the `Model/zones#ZONE_GRAPH` overlay and the `Model/structural#STRUCTURAL_PROJECTION` graph hold. The reachability fold and the system view are TOTAL over the already-validated graph (the dangling-endpoint rejection lowered at `Projection/semantic#SEMANTIC_PROJECTOR` `Project` and `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Apply`); only the interference clash carries a `Fin<T>` rail, lowering `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE when a member's geometry content key resolves to no kernel geometry.

## [01]-[INDEX]

- [01]-[CONNECTIVITY]: the `DistributionSystem` derived view (member `NodeId` set, nested sub-circuit set, typed `DistributionPort` set, port `FlowEdge` set, served-structure set, `(MembershipKey, TopologyKey)` identity), the `DistributionSystemKind` `[SmartEnum<string>]` over `IfcDistributionSystemEnum` with its `IfcDomain` column, the `FlowDirection` `[SmartEnum<string>]` over `IfcFlowDirectionEnum`, and the `DistributionNetwork` fold reading the seam `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views.
- [02]-[SYSTEM_TRACE]: the `SystemTrace` reachability fold over the seam's port-and-element flow graph — one transient `QuikGraph` `AdjacencyGraph<NodeId, SEdge<NodeId>>` built from the `Connect{Port}` edges, the `TraceMode` orientation policy (reach/downstream/upstream) reading the port `FlowDirection`, and `TreeBreadthFirstSearch` computing the reachable-element closure, the shared `[GRAPH_ALGORITHM]` owner replacing the hand-rolled visited-set walk.
- [03]-[INTERFERENCE]: the `Interference` record (clashing `NodeId` pair, `ClashKind` hard/clearance, measured deficit, the two discipline `IfcDomain`s, cross-discipline rank), the `GeometryProximity` kernel seam, the `SwiftCollections.Lean` `SwiftBVH` broad-phase, and the `InterferenceCheck.Interferences` fold ranking clashes between distribution runs and against the static obstruction set (every other body-carrying occurrence — beams, walls, structure), feeding `Review/coordination#COORDINATION` `ClashProposal`.

## [02]-[CONNECTIVITY]

- Owner: `DistributionSystem` the single host-neutral derived VIEW of one MEP distribution system read from the seam `ElementGraph` — the system group `NodeId`, the `ExternalId` (the IFC `GlobalId` projection attribute), the `DistributionSystemKind` discriminant resolved off the group node's `PredefinedType`, the member `NodeId` set, the `Circuits` sub-circuit subset (the members themselves classified as system groups — an `IfcDistributionCircuit` under its parent board), the typed `DistributionPort` set, the port-to-port `FlowEdge` set the flow network is built from, and the served spatial-structure `NodeId` set, with a derived `(MembershipKey, TopologyKey)` content-key identity the trace re-reads the network by; `DistributionSystemKind` the closed `[SmartEnum<string>]` keyed over the `IfcDistributionSystemEnum` member set with a `Domain` column resolving each kind onto the `Model/elements#IFC_CLASS` `IfcDomain` partition; `FlowDirection` the `[SmartEnum<string>]` over `IfcFlowDirectionEnum` carrying the `Emits`/`Receives` orientation columns the directed trace reads; `DistributionPort` the derived port view (`NodeId`, name, `FlowDirection`, the `PredefinedType` port kind, and the owning distribution element `NodeId`); `DistributionNetwork` the static fold reading the seam graph's `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views.
- Cases: `DistributionSystemKind` rows span the FULL `IfcDistributionSystemEnum` distribution vocabulary partitioned across the `IfcDomain` set — air/thermal-fluid/combustion-fuel/fire-and-life-safety (`AirConditioning`/`Ventilation`/`ChilledWater`/`CompressedAir`/`Heating`/`Refrigeration`/`Fuel`/`Gas`/`Oil`/`FireProtection`/`Safety`/…) on `HvacFire`, piped water/drainage/waste/process (`DomesticColdWater`/`DomesticHotWater`/`WaterSupply`/`Drainage`/`Sewage`/`RainWater`/`StormWater`/`WasteWater`/`Chemical`/…) on `Plumbing`, power/lighting/telecom/data/signal/rail-traction (`Electrical`/`Lighting`/`Telephone`/the DISTINCT `Data` and `Communication` rows/`Security`/`CatenarySystem`/…) on `Electrical`, and `Conveying` on `Architecture` — every IFC4 discipline, the seven IFC4X3 rail-electrification/telephony additions (`CatenarySystem`/`OverheadContactLine`/`ReturnCircuit`/`FixedTransmissionNetwork`/`OperationalTelephony`/`MobileNetwork`/`MonitoringSystem`), and the IFC4X4 `Safety` draft each frozen with its `IfcDomain`, plus the `UserDefined`/`NotDefined` fallback rows the `Of` resolver lowers an unmapped token onto — carried on the generic `Architecture` domain so an unclassified system never pollutes a `ByDomain(IfcDomain.HvacFire)` discipline selection — the closed buildingSMART roster, never a 13-of-50 slice and never a fused `DataCommunication` phantom (the enum carries `DATA` and `COMMUNICATION` separately, no `DATACOMMUNICATION` token); `FlowDirection` rows `Source` (emits) · `Sink` (receives) · `SourceAndSink` (both) · `NotDefined` (both — an undirected port conducts either way) (the full `IfcFlowDirectionEnum`, 4); a `DistributionPort` is a `Source`/`Sink` port on a `FlowSegment` owner, a tee fitting carries three ports, and a `FlowEdge` joins two ports across elements (the `IfcRelConnectsPorts` flow connection) carrying its optional realizing fitting `NodeId`.
- Entry: `DistributionNetwork.View(ElementGraph graph)` folds every distribution-system group node into the `Seq<DistributionSystem>` the trace and interference read, and `DistributionNetwork.View(ElementGraph graph, NodeId system)` folds one — ONE polymorphic reader discriminating on the arity of the input (the whole-model set versus one group node), never a `ViewAll`/`ViewBy` name family; both are TOTAL and carry no `Fin<T>` rail because the seam graph is already consistent (a dangling member or an absent port lowered at `Projection/semantic#SEMANTIC_PROJECTOR` `Project` and `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Apply`, never re-validated here). A group node whose classification is not a distribution-system class is skipped (it is the `Model/zones#ZONE_GRAPH` overlay's logical zone, not a flow network), and a system whose `PredefinedType` carries no mapped `IfcDistributionSystemEnum` token resolves to the `UserDefined`/`NotDefined` kind rather than dropping the system.
- Auto: `View` reads the group `Node.Object` set classified as a distribution system, and `Of` folds one — `MembersOf` reads the group's incident `Assign{Group}` edges (the OTHER endpoint the member, the seam projection of `IfcRelAssignsToGroup`), the `Circuits` subset filtering the members whose own node is system-classified (an `IfcDistributionCircuit` rides as a member AND as its own `DistributionSystem` row); `PortsOf` reads each member's incident port-OWNERSHIP edges over BOTH containment forms — the `Generic` edge carrying the `IfcRelKind.ConnectsPortToElement.Key` wire-name (the IFC2x3 projection, its port on the relating side) and the `Compose{Nest}` edge whose part is a port (the IFC4 `IfcRelNests` port-containment projection) — onto deduped `DistributionPort` rows carrying the port node's name, its `PredefinedType` kind, and its `FlowDirection` read off the port's effective `FlowDirection` property the projector lowers, so an IFC4 model whose ports nest under their elements loses no port; `FlowEdgesOf` reads the `Connect{Port}` FLOW edges whose BOTH endpoints are in the system's port set (the `IfcRelConnectsPorts` projection, deduped on the unordered port pair so a connection materialized from either incident port rides one edge, carrying the optional realizing fitting from the `Connect.Realizing`); and `ServedOf` reads the group's incident `Generic` edges whose wire-name is `IfcRelKind.ServicesBuildings.Key` (the served spatial structures riding the neutral passthrough [C5]); the `Identity` fold derives the `(MembershipKey, TopologyKey)` `UInt128` pair through the kernel seed-zero `Rasm.Domain.ContentHash.Of` — `MembershipKey` over the ordered member `NodeId` set and `TopologyKey` over the sorted flow-edge unordered port pairs — so a consumer re-walks only a changed membership or a changed adjacency, the single seed-zero hasher the seam `NodeId`/`ContentAddress` also compose, never a second hasher [H7].
- Receipt: the `Seq<DistributionSystem>` is the connectivity evidence the `[02]-[SYSTEM_TRACE]` `SystemTrace` fold walks and the `[03]-[INTERFERENCE]` clash pairs the distribution members from; the `Model/zones#ZONE_GRAPH` MEP grouping reads the member set by reference; the air-handling system, the water riser, and the electrical board each carry their member set, their nested sub-circuits, their typed port adjacency, and their served structures on one record.
- Packages: Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new distribution discipline is one `DistributionSystemKind` row reading the next `IfcDistributionSystemEnum` token with its `IfcDomain` column; a new flow direction is one `FlowDirection` row with its `Emits`/`Receives` orientation; a new port-containment or membership relationship rides the existing seam edge kinds the fold already reads; never a per-discipline system record, never a second connectivity store, and never a per-relationship connection class.
- Boundary: `DistributionSystem` is ONE derived view discriminated by the `DistributionSystemKind` row data — an `HvacSystem`/`ElectricalSystem`/`PlumbingSystem` class family or sibling per-discipline factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the retired `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) is GONE — it mirrored the typed `IfcRel*` cases the seam's neutral `Connect` algebra collapsed [C5], and re-introducing a typed connection union is the named drift, the connectivity reading the neutral edges as the `IfcRelKind` roster actually lands them — `Connect{Port}` the port-to-port FLOW edge, `Generic("IfcRelConnectsPortToElement")` + `Compose{Nest}` the two port-OWNERSHIP containment forms — and an ownership read against an edge kind the roster never emits (the prior `Connect{Port}`-ownership probe, dead against the `EdgeAxis.Generic` row) or one that drops every IFC4-nested port is the deleted illusory read; the retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold is GONE and a `GeometryGym.Ifc` import crossing this owner is the named seam violation — `Projection/semantic#SEMANTIC_PROJECTOR` is the sole IFC lowering and this owner reads the resulting seam graph; identity is the seam `NodeId` and a `GlobalId`-keyed view is the deleted form (the `GlobalId` is the node `ExternalId` the IFC-keyed consumers read); the served-structure set rides the `Generic("IfcRelServicesBuildings")` passthrough; the distribution-domain element selection is the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Electrical)`/`ByDomain(IfcDomain.Plumbing)` predicate and a parallel system-element selection arm is the no-second-selection-surface reject; the connectivity graph is the orthogonal companion to the `Model/zones#ZONE_GRAPH` logical membership — the zone overlay owns which elements belong, the connectivity owns how they connect, the two never collapsed; the `(MembershipKey, TopologyKey)` identity is the `Rasm.Domain.ContentHash.Of` seed-zero key and a second identity scheme is the named drift defect [H7].

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Text;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

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
    public static FlowDirection Of(string token) => TryGet(token.Trim(), out var direction) ? direction : NotDefined;
}

// The FULL IfcDistributionSystemEnum distribution vocabulary (decompile-verified against GeometryGymIFC_Core
// 25.7.30), each row carrying its IfcDomain partition the consumer reads off `system.Kind.Domain` to select a
// discipline's systems. The kind resolves off the system group node's PredefinedType token (the seam-carried
// value-object); an unmapped/unknown token lowers NotDefined. Domain grouping: air/thermal-fluid/combustion-fuel/
// fire-and-life-safety on HvacFire, piped water/drainage/waste/process on Plumbing, power/lighting/telecom/data/
// signal/rail-traction on Electrical, conveying + the UserDefined/NotDefined fallbacks on the generic Architecture
// (an unclassified system never pollutes a discipline's ByDomain selection) — the closed buildingSMART roster.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DistributionSystemKind {
    public static readonly DistributionSystemKind AirConditioning          = new("AIRCONDITIONING",             IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Ventilation              = new("VENTILATION",                 IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Vent                     = new("VENT",                        IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Exhaust                  = new("EXHAUST",                     IfcDomain.HvacFire);
    public static readonly DistributionSystemKind ChilledWater             = new("CHILLEDWATER",                IfcDomain.HvacFire);
    public static readonly DistributionSystemKind CondenserWater           = new("CONDENSERWATER",              IfcDomain.HvacFire);
    public static readonly DistributionSystemKind CompressedAir            = new("COMPRESSEDAIR",               IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Heating                  = new("HEATING",                     IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Refrigeration            = new("REFRIGERATION",               IfcDomain.HvacFire);
    public static readonly DistributionSystemKind FireProtection           = new("FIREPROTECTION",              IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Vacuum                   = new("VACUUM",                      IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Fuel                     = new("FUEL",                        IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Gas                      = new("GAS",                         IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Oil                      = new("OIL",                         IfcDomain.HvacFire);
    public static readonly DistributionSystemKind Safety                   = new("SAFETY",                      IfcDomain.HvacFire);
    public static readonly DistributionSystemKind DomesticColdWater        = new("DOMESTICCOLDWATER",           IfcDomain.Plumbing);
    public static readonly DistributionSystemKind DomesticHotWater         = new("DOMESTICHOTWATER",            IfcDomain.Plumbing);
    public static readonly DistributionSystemKind WaterSupply              = new("WATERSUPPLY",                 IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Drainage                 = new("DRAINAGE",                    IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Sewage                   = new("SEWAGE",                      IfcDomain.Plumbing);
    public static readonly DistributionSystemKind RainWater                = new("RAINWATER",                   IfcDomain.Plumbing);
    public static readonly DistributionSystemKind StormWater               = new("STORMWATER",                  IfcDomain.Plumbing);
    public static readonly DistributionSystemKind WasteWater               = new("WASTEWATER",                  IfcDomain.Plumbing);
    public static readonly DistributionSystemKind MunicipalSolidWaste      = new("MUNICIPALSOLIDWASTE",         IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Disposal                 = new("DISPOSAL",                    IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Chemical                 = new("CHEMICAL",                    IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Hazardous                = new("HAZARDOUS",                   IfcDomain.Plumbing);
    public static readonly DistributionSystemKind Electrical               = new("ELECTRICAL",                  IfcDomain.Electrical);
    public static readonly DistributionSystemKind Lighting                 = new("LIGHTING",                    IfcDomain.Electrical);
    public static readonly DistributionSystemKind PowerGeneration          = new("POWERGENERATION",             IfcDomain.Electrical);
    public static readonly DistributionSystemKind Earthing                 = new("EARTHING",                    IfcDomain.Electrical);
    public static readonly DistributionSystemKind LightningProtection      = new("LIGHTNINGPROTECTION",         IfcDomain.Electrical);
    public static readonly DistributionSystemKind Telephone                = new("TELEPHONE",                   IfcDomain.Electrical);
    public static readonly DistributionSystemKind Data                     = new("DATA",                        IfcDomain.Electrical);
    public static readonly DistributionSystemKind Communication            = new("COMMUNICATION",               IfcDomain.Electrical);
    public static readonly DistributionSystemKind AudioVisual              = new("AUDIOVISUAL",                 IfcDomain.Electrical);
    public static readonly DistributionSystemKind ElectroAcoustic          = new("ELECTROACOUSTIC",             IfcDomain.Electrical);
    public static readonly DistributionSystemKind Television               = new("TV",                          IfcDomain.Electrical);
    public static readonly DistributionSystemKind Signal                   = new("SIGNAL",                      IfcDomain.Electrical);
    public static readonly DistributionSystemKind Control                  = new("CONTROL",                     IfcDomain.Electrical);
    public static readonly DistributionSystemKind Security                 = new("SECURITY",                    IfcDomain.Electrical);
    public static readonly DistributionSystemKind Operational              = new("OPERATIONAL",                 IfcDomain.Electrical);
    public static readonly DistributionSystemKind OperationalTelephony     = new("OPERATIONALTELEPHONYSYSTEM",  IfcDomain.Electrical);
    public static readonly DistributionSystemKind MobileNetwork            = new("MOBILENETWORK",               IfcDomain.Electrical);
    public static readonly DistributionSystemKind MonitoringSystem         = new("MONITORINGSYSTEM",            IfcDomain.Electrical);
    public static readonly DistributionSystemKind FixedTransmissionNetwork = new("FIXEDTRANSMISSIONNETWORK",    IfcDomain.Electrical);
    public static readonly DistributionSystemKind CatenarySystem           = new("CATENARY_SYSTEM",             IfcDomain.Electrical);
    public static readonly DistributionSystemKind OverheadContactLine      = new("OVERHEAD_CONTACTLINE_SYSTEM", IfcDomain.Electrical);
    public static readonly DistributionSystemKind ReturnCircuit            = new("RETURN_CIRCUIT",              IfcDomain.Electrical);
    public static readonly DistributionSystemKind Conveying                = new("CONVEYING",                   IfcDomain.Architecture);
    public static readonly DistributionSystemKind UserDefined              = new("USERDEFINED",                 IfcDomain.Architecture);
    public static readonly DistributionSystemKind NotDefined               = new("NOTDEFINED",                  IfcDomain.Architecture);

    public IfcDomain Domain { get; }

    public static DistributionSystemKind Of(string token) => TryGet(token.Trim(), out var kind) ? kind : NotDefined;   // the comparer owns case
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
    // IfcBuiltSystem is the SHIPPED GG 25.7.30 entity (decompile-verified: IfcBuiltSystem : IfcSystem carrying
    // IfcBuiltSystemTypeEnum; IfcBuildingSystem is ABSENT from the assembly) — the IfcBuildingSystem token stays
    // ONLY as a foreign-payload alias for hand-authored classifications, since a GG-ingested node never mints it.
    // IfcDistributionCircuit is the sub-circuit subtype.
    static readonly FrozenSet<string> SystemClasses =
        new[] { "IfcDistributionSystem", "IfcDistributionCircuit", "IfcSystem", "IfcBuiltSystem", "IfcBuildingSystem" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    const string PortClass = "IfcDistributionPort";
    static readonly PropertyName FlowKey = PropertyName.Create("FlowDirection");

    // ONE polymorphic reader: the whole-model system set, or one system by its group node — the arity
    // discriminates on the input shape, never a ViewAll/ViewBy name family. Total over the consistent graph.
    public static Seq<DistributionSystem> View(ElementGraph graph) =>
        graph.ObjectNodes.Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o));

    public static Option<DistributionSystem> View(ElementGraph graph, NodeId system) =>
        graph.Find<Node.Object>(system).Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o));

    static DistributionSystem Of(ElementGraph graph, Node.Object system) {
        var members = MembersOf(graph, system.Id);
        // Nested sub-circuits: the members that are themselves system groups (IfcDistributionCircuit under its
        // parent board) — each also folds as its own DistributionSystem row through View.
        var circuits = members.Filter(m => graph.Find<Node.Object>(m).Map(static o => SystemClasses.Contains(o.Classification.Code)).IfNone(false));
        var ports = members.Bind(m => PortsOf(graph, m));
        var flow = FlowEdgesOf(graph, toHashSet(ports.Map(static p => p.Id)));
        return new DistributionSystem(
            system.Id, system.ExternalId, system.Name,
            DistributionSystemKind.Of(system.PredefinedType.Token),
            members, circuits, ports, flow, ServedOf(graph, system.Id));
    }

    // The grouped members: the system's incident Assign{Group} edges (the incidence index already scopes to this
    // node, so the OTHER endpoint is the member; the self-loop filter guards a degenerate self-assignment).
    static Seq<NodeId> MembersOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group
                ? Some(a.Relating == system ? a.Related : a.Relating) : None)
            .Filter(id => id != system);

    // A member's ports over BOTH ownership forms the projector lands — the Generic("IfcRelConnectsPortToElement")
    // passthrough (the IFC2x3 containment; its IfcRelKind row rides EdgeAxis.Generic with the PORT relating) and
    // the Compose{Nest} edge whose part is a port (IfcRelNests, the IFC4 port containment) — deduped on the port
    // id for a belt-and-braces file carrying both. Connect{Port} is the port-to-port FLOW edge, never ownership.
    static Seq<DistributionPort> PortsOf(ElementGraph graph, NodeId member) =>
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

    static Option<Node.Object> PortNode(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Filter(static o => string.Equals(o.Classification.Code, PortClass, StringComparison.OrdinalIgnoreCase));

    // The flow edges: Connect{Port} edges whose BOTH endpoints are ports of this system (the IfcRelConnectsPorts
    // projection), read through each port's incidence (EdgesAt, O(port-degree)) NOT a whole-graph Edges rescan (the
    // seam incidence-index law View/MembersOf/PortsOf/ServedOf hold), then deduped on the unordered port pair so a
    // connection incident to both its ports (visited twice) rides one edge.
    static Seq<FlowEdge> FlowEdgesOf(ElementGraph graph, LanguageExt.HashSet<NodeId> ports) =>
        ports.ToSeq()
            .Bind(port => toSeq(graph.EdgesAt(port)))
            .Choose(e => e is Relationship.Connect c && c.SubKind == ConnectKind.Port && ports.Contains(c.From) && ports.Contains(c.To)
                ? Some(new FlowEdge(c.From, c.To, c.Realizing)) : None)
            .DistinctBy(static f => string.CompareOrdinal(f.From.Value, f.To.Value) <= 0 ? (f.From.Value, f.To.Value) : (f.To.Value, f.From.Value))
            .ToSeq();

    // The served spatial structures via the Generic passthrough — IfcRelServicesBuildings has no neutral case, so
    // it rides Generic carrying its wire-name, the relating side the system.
    static Seq<NodeId> ServedOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Generic g && string.Equals(g.WireName, IfcRelKind.ServicesBuildings.Key, StringComparison.Ordinal) && g.Relating == system
                ? Some(g.Related) : None);

    // The port flow direction off the port node's neutral "FlowDirection" property — the direct IFC attribute
    // IfcDistributionPort.FlowDirection has NO seam Node.Object home (the seam carries no flow column), so the directed
    // trace depends on the Projection/semantic#SEMANTIC_PROJECTOR surfacing that attribute as a "FlowDirection" entry on
    // a port PropertySet bag at ingest; absent that surfacing every port reads NotDefined (conducts both ways), so a
    // model without the lowered direction traces undirected rather than faulting — the directed orientation degrades to
    // reachability, never an error, and a host-neutral systems view never reads the GeometryGym attribute itself.
    static FlowDirection PortFlow(ElementGraph graph, NodeId port) =>
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
- Entry: `SystemTrace.From(DistributionSystem system, NodeId seed, Option<TraceMode> mode = default)` folds the reachable closure — the absent mode `IfNone`-defaults to the undirected `Reach` closure, the explicit value the directed `Downstream`/`Upstream` orientation (ONE signature; a mode-defaulting sibling overload is a knob spelled as arity) — building the transient `AdjacencyGraph<NodeId, SEdge<NodeId>>` from the system's ownership and flow edges ONCE, running `TreeBreadthFirstSearch(seed)` from the seed, and partitioning the reached vertices into the reached-element `NodeId` set and the reached-port `NodeId` set; the seed is a port (the trace from a junction) OR an element (the trace from any of its ports, reached through the element's ownership edges), the input node kind the discriminant, never a `FromPort`/`FromElement` name family; the trace is TOTAL over the consistent graph and never faults — an isolated seed traces to itself, a seed absent from the network traces to itself alone — so the reachability fold carries no `Fin<T>` rail, the rejection already lowered at `Projection/semantic#SEMANTIC_PROJECTOR` `Project`.
- Auto: `From` folds the system's `Ports` into the graph as ownership edges (each port ↔ its owner element, both directions — a port belongs to its element regardless of flow) and the system's `Flow` edges oriented by `TraceMode` over BOTH endpoint `FlowDirection`s (`Reach` adds both directions; a `Downstream` leg exists only where the emitting side `Emits` AND the receiving side `Receives`, so a `Source`→`Sink` edge carries one leg, a `NotDefined` pair conducts both ways, a `NotDefined`-against-`Source` edge still flows OUT of the source only, and two facing pure sources honestly sever the directed closure; `Upstream` is the mirror), the optional realizing fitting linked bidirectionally so a connection's joint joins the reached set; `TreeBreadthFirstSearch` returns the `TryFunc<NodeId, IEnumerable<SEdge<NodeId>>>` whose reachable domain IS the downstream closure (the seed plus every vertex with a recovered path), and the fold partitions the reached vertices into the non-port reached elements and the reached ports by the port-set membership — the directed `Downstream` trace from an air-handling unit reaching every air terminal it feeds, the `Reach` trace from a shutoff valve reaching every fixture on its branch, both one fold over the QuikGraph adjacency; the trace reads the `DistributionSystem` view (one hop — the view already carries the ports with their `FlowDirection` and the deduped flow edges), never re-reading the seam graph or re-resolving the port flow per query, and a consumer memoizes the trace against the owning system's `(MembershipKey, TopologyKey)` `Identity` so a re-trace re-folds only on a changed membership or adjacency.
- Receipt: the `SystemTrace` reached-element `Seq<NodeId>` is the downstream-network evidence the `Model/zones#ZONE_GRAPH` MEP grouping reads to resolve a system's effective member closure and the `Model/query#ELEMENT_SET` consumers intersect against a domain set — a "every air terminal fed from this air-handling unit" / "every fixture downstream of this shutoff valve" query is one `From` fold over the flow graph, the connectivity the single-membership zone overlay cannot express; the reached set is consumed by the `zoning`/`query`/`analysis` peers by reference, never re-derived per consumer.
- Packages: QuikGraph, Rasm.Element, LanguageExt.Core
- Growth: a new trace orientation is one `TraceMode` row carrying its `Symmetric`/`Reverse` data the `Orient` fold reads off the same `FlowDirection`; a new reachability guard (stop at a controller, stop at a discipline boundary) is one filter on the edge fold; a new graph query (shortest flow path, connected components) rides the SAME `QuikGraph` `AlgorithmExtensions` facade over the same adjacency; never a per-direction trace record, never a second adjacency store, and never a per-discipline traversal.
- Boundary: the `SystemTrace` is ONE reachability fold over the shared `QuikGraph` `AdjacencyGraph` — the retired hand-rolled `SystemNetwork`/`Closure` visited-set tail-recursion is the deleted form, the `[GRAPH_ALGORITHM]` owner the whole stack folds a transient graph into rather than re-implementing a walk (the api-quikgraph `Model/systems#SYSTEM_TRACE` `TreeBreadthFirstSearch` law), and a `Map<>`/`HashSet<>` adjacency with a mutated visited set is the named drift; a `TraceHvac`/`TraceElectrical`/`TracePlumbing` operation family is the deleted form per the no-operation-family law, the discipline already carried by the system's `Kind` the trace folds within; the trace carries no `Fin<T>` rail because the closed graph is total (the dangling-endpoint rejection lowered at `Project`); the trace reads the `DistributionSystem` view ONE HOP and a re-read of the seam graph or a re-resolution of the port `FlowDirection` per query is the named cross-page drift; the directed orientation reads the port `FlowDirection` the view carries and an `AdjacencyGraph` with no orientation policy is the no-modality reject; a consumer memoizes against the system `Identity` and a second identity scheme is the named drift defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

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
public sealed record SystemTrace(NodeId Seed, TraceMode Mode, Seq<NodeId> ReachedElements, Seq<NodeId> ReachedPorts) {
    // ONE reachability fold over the system's flow graph — the optional mode IfNone-defaults to the undirected
    // Reach closure (Option, never a mode-defaulting sibling overload), Downstream/Upstream reading both endpoint
    // FlowDirections. The foreach graph fill is the QuikGraph mutable-builder platform seam, confined here.
    public static SystemTrace From(DistributionSystem system, NodeId seed, Option<TraceMode> mode = default) {
        var traceMode = mode.IfNone(TraceMode.Reach);
        var ports = toHashSet(system.Ports.Map(static p => p.Id));
        var flowByPort = system.Ports.Fold(Map<NodeId, FlowDirection>(), static (m, p) => m.AddOrUpdate(p.Id, p.Flow));
        var graph = new AdjacencyGraph<NodeId, SEdge<NodeId>>(allowParallelEdges: true);
        // Ownership: every port is part of its element both ways (the view's Owner column, containment-form-agnostic).
        foreach (var port in system.Ports) { Link(graph, port.Id, port.Owner); Link(graph, port.Owner, port.Id); }
        // Flow: oriented by mode + BOTH endpoint FlowDirections; the realizing fitting joins both ports.
        foreach (var edge in system.Flow) {
            var (fromFlow, toFlow) = (flowByPort.Find(edge.From).IfNone(FlowDirection.NotDefined), flowByPort.Find(edge.To).IfNone(FlowDirection.NotDefined));
            foreach (var (a, b) in Orient(edge.From, edge.To, fromFlow, toFlow, traceMode)) { Link(graph, a, b); }
            edge.Realizing.Iter(realizing => { Link(graph, edge.From, realizing); Link(graph, realizing, edge.From); Link(graph, realizing, edge.To); Link(graph, edge.To, realizing); });
        }
        if (!graph.ContainsVertex(seed)) { graph.AddVertex(seed); }
        var paths = graph.TreeBreadthFirstSearch(seed);
        // TryGetPath returns false for the root (no predecessor edge), so the seed is admitted explicitly.
        var reached = toSeq(graph.Vertices).Filter(v => v == seed || paths(v, out _));
        return new SystemTrace(seed, traceMode, reached.Filter(v => !ports.Contains(v)), reached.Filter(ports.Contains));
    }

    // The flow-edge orientation reads the mode's Symmetric/Reverse data over BOTH endpoint directions: Reach is
    // symmetric (both legs); a directed leg exists only where the emitting side Emits AND the receiving side
    // Receives, Reverse mirroring it — so a NotDefined port (Emits && Receives) degrades to bidirectional against
    // another NotDefined, a NotDefined-against-Source edge still carries flow OUT of the source only, and two
    // facing pure sources (a modeling contradiction) honestly sever the directed trace while Reach still spans
    // them. No mode switch and no runtime-silent arm: the booleans drive the orientation directly.
    static Seq<(NodeId From, NodeId To)> Orient(NodeId from, NodeId to, FlowDirection fromFlow, FlowDirection toFlow, TraceMode mode) =>
        mode.Symmetric
            ? Seq((from, to), (to, from))
            : (fromFlow.Emits && toFlow.Receives ? Seq(mode.Reverse ? (to, from) : (from, to)) : Seq<(NodeId, NodeId)>())
            + (toFlow.Emits && fromFlow.Receives ? Seq(mode.Reverse ? (from, to) : (to, from)) : Seq<(NodeId, NodeId)>());

    static void Link(AdjacencyGraph<NodeId, SEdge<NodeId>> graph, NodeId from, NodeId to) =>
        graph.AddVerticesAndEdge(new SEdge<NodeId>(from, to));
}
```

## [04]-[INTERFERENCE]

- Owner: `Interference` the host-neutral clash-evidence record carrying the clashing `(NodeId, NodeId)` pair, the `ClashKind` (`Hard` overlapping solids, `Clearance` insufficient maintenance/insulation gap), the measured deficit (the penetration depth for a hard clash, the clearance shortfall for a clearance clash, both kernel-SI scalars), the two member disciplines (`IfcDomain` pair), and the priority rank a cross-discipline clash carries above an intra-discipline one; `ClashKind` the closed `[SmartEnum<string>]` clash partition; `InterferenceQuery` the proximity request keyed by the two members' `RepresentationContentHash` body geometry content keys plus the clearance threshold, the host-neutral systems owner producing the request and reading the scalar deficit back, the kernel `Rasm` geometry owner resolving the content-keyed geometry and evaluating the solid intersection; `GeometryProximity` the injected kernel PORT — a `readonly record struct` of two decode legs (`Bounds` the content-keyed `(Vector3 Min, Vector3 Max)` AABB, `Test` the precise `Fin<ProximityResult>` signed gap), the SAME app-wired resolver shape the seam `Graph/element#NODE_MODEL` `GeometrySource` holds, never an interface floor for a two-arrow contract; `InterferenceCheck` the fold pairing the distribution-run geometry against itself (cross-system) and against the static obstruction set — every other body-carrying occurrence, the Architecture-domain built elements included.
- Cases: `ClashKind` rows `Hard` (overlapping solids, deficit = penetration depth) · `Clearance` (gap below the maintenance/insulation envelope, deficit = threshold − gap) (2); an `Interference` carries the ordered member `NodeId` pair, the `ClashKind`, the SI deficit, and the discipline `IfcDomain` pair, a cross-discipline clash (`FirstDomain != SecondDomain`) ranking above an intra-discipline one through the `DisciplineWeight` ordering offset.
- Entry: `InterferenceCheck.Interferences(ElementGraph graph, GeometryProximity proximity, double clearanceThreshold, Op key)` folds the cross-system clash set — selecting every body-carrying OCCURRENCE off the seam graph (a `RepresentationContentHash.Body` key, the discipline resolved through `Model/elements#IFC_CLASS` the `IfcClass` row's `Domain`, the `HvacFire`/`Electrical`/`Plumbing` rows the RUN side and every other domain the STATIC obstruction side — the Architecture-domain beams/columns/walls included), admitting a pair only when at least one member is a run (MEP-vs-MEP crosses systems, MEP-vs-static catches the duct-vs-beam, static-vs-static never enters), pairing each admitted member's body geometry against every bounds-overlapping candidate through the `SwiftCollections.Lean` broad-phase, routing each candidate pair to the injected `proximity.Test(InterferenceQuery)` kernel test, and emitting an `Interference` row for each pair whose solids overlap (`Hard`) or whose gap falls below the pair clearance envelope (`Clearance`); `Fin<T>` propagates the kernel `GeometryProximity` rejection BARE (a member whose body content key resolves to no realized kernel geometry lowers `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss`, the `Expected`-derived case lifting with no `.ToError()` hop), and the fold ranks the result by deficit descending and cross-discipline-first so the worst structural penetration sorts above a minor intra-discipline graze, the ranked `Seq<Interference>` feeding the `Review/coordination#COORDINATION` `ClashProposal` substrate. The pair clearance threshold is the wider of the two members' `Semantics/properties#PROPERTY_TEMPLATES` insulation/maintenance envelopes read off the BAKED element's effective property bags through the `ClearanceKeys` policy table, floored by the flat `clearanceThreshold` when a member carries no clearance property so the envelope never collapses to zero (an insulated chilled-water pipe carries a larger maintenance envelope than a bare cable tray).
- Auto: `Interferences` builds the candidate member-pair set through the `SwiftCollections.Lean` `SwiftBVH<int>` broad-phase — each member's kernel-SI AABB (`proximity.Bounds(bodyKey)`, the content-keyed geometry, no host type) inserts once by SAH-cost placement, then one `SwiftBVH.Query(volume, results)` per member fills a `SwiftList<int>` candidate sink and the unordered index pair dedups through a `SwiftSparseSet.Add` so only run-admitted bounds-overlapping pairs reach the precise `proximity.Test`, retiring the O(N²) all-pairs enumeration; the broad-phase is host-neutral (the `BoundVolume` a `System.Numerics.Vector3` AABB) and the precise solid-distance test the kernel's concern; each surviving pair routes to `proximity.Test(new InterferenceQuery(a.Body, b.Body, Math.Max(clearanceThreshold, Math.Max(a.Clearance, b.Clearance))))` — the per-member envelope resolved ONCE in the member fold (one memoized `Bake` + bag walk), never re-derived per pair — returning a `ProximityResult` (the signed gap — negative for penetration — and the closest approach), the fold reading `result.Gap < 0` as a `Hard` clash with deficit `−result.Gap` and `0 ≤ result.Gap < threshold` as a `Clearance` clash with deficit `threshold − result.Gap`; the discipline pair reads each member's `IfcDomain` (a structural member carries `IfcDomain.Structural`), the rank folding `crossDiscipline ? deficit + DisciplineWeight : deficit` so the ranking is one ordering key, and the `Interference.Identity` content key over the ordered `NodeId` pair through `Rasm.Domain.ContentHash.Of` memoizes the clash so a `Review/diff#MODEL_DIFF` re-check re-tests only a moved member's pairs (the `SwiftBVH.UpdateEntryBounds` incremental refit rebuilding only the changed AABBs).
- Receipt: the ranked `Seq<Interference>` is the MEP coordination evidence the `Review/coordination#COORDINATION` `ClashProposal` fold consumes (the clash `NodeId` pair, kind, and deficit anchoring a proposed resolution and a BCF topic, the coordination owner resolving each member's `ExternalId` IFC `GlobalId` off the graph for the viewpoint) and the `csharp:Rasm.AppUi/Charts` clash report renders — a duct-vs-beam hard clash, a pipe-clearance violation, and a tray-vs-structure graze each carry their measured deficit and discipline pair on one host-neutral row.
- Packages: Rasm.Element, Rasm, SwiftCollections.Lean, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new clash kind (a soft clash, a code-clearance violation) is one `ClashKind` row reading the same proximity result; a new run discipline is one `RunDomains` row (the static side derives — everything else); a new clearance source (a code-mandated envelope, a thermal-expansion gap) is one `PropertyName` in the `ClearanceKeys` policy table; a new ranking dimension is one ordering key on the same fold; a broad-phase structure swap (octree, spatial hash) is the `SwiftOctree`/`SwiftSpatialHash` behind the shared `Insert`/`Query` contract; never a per-discipline clash record, never a second proximity surface, and never a re-tessellation here.
- Boundary: the interference test is geometric proximity binding the kernel `Rasm` geometry by reference — the systems owner keeps the `Interference` row as host-neutral scalar evidence (the `NodeId` pair plus the measured deficit) and the solid-intersection/signed-distance test routes to the injected `GeometryProximity` kernel owner through the `InterferenceQuery` keyed by the seam `RepresentationContentHash` body content key, the same host-neutrality law the `[01]-[CONNECTIVITY]` view holds — a RhinoCommon `Brep.CreateBooleanIntersection`, a `Mesh` overlap test, or a `GeometryGym.Ifc` import crossing this owner is the named seam violation; the candidate broad-phase is the `SwiftCollections.Lean` `SwiftBVH<int>` over the content-keyed AABB and an O(N²) all-pairs scan or a hand-rolled BVH is the deleted form (the package owns the SAH-cost 3D index; the `NetTopologySuite` `STRtree` owns the 2D planar `Semantics/geospatial#GEOSPATIAL_SEAM` index — neither reimplements the other's dimension), and the precise test is the kernel's concern, never re-tessellating in this lane; the member selection is the RUN/STATIC partition policy (`RunDomains` the MEP side, every other body-carrying occurrence the static side, a Type's un-placed representation excluded) and a hardcoded clash-domain roster omitting the Architecture-domain built elements is the deleted form (it made the duct-vs-beam clash unreachable — `IfcClass` roots `IfcBuiltElement` on `Architecture`, `Structural` holding only the reinforcing/footing/pile override rows); the clash partition is the closed `ClashKind` `[SmartEnum]` and a `HardClash`/`ClearanceClash` class family is the deleted form; the clearance threshold reads the BAKED element's `Semantics/properties#PROPERTY_TEMPLATES` effective property through the `ClearanceKeys` policy table and a hardcoded per-discipline gap table or an injected `Func<string,double>` is the deleted form; the `Interference` row carries the seam `NodeId` identity (the coordination/BCF consumer resolves the `ExternalId` for the viewpoint) and a `GlobalId`-keyed clash row is the deleted form; the `Interference` row is the `Review/coordination#COORDINATION` `ClashProposal` substrate so coordination consumes this evidence rather than re-deriving proximity — re-running the proximity test in the coordination owner is the named cross-page drift defect; the `(NodeId, NodeId)` content key is derived through `Rasm.Domain.ContentHash.Of` [H7] and a second hasher is the named drift; an interference rejection lowers `BimFault.CapabilityMiss` BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Text;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using SwiftCollections;
using SwiftCollections.Query;
using Thinktecture;
using Vector3 = System.Numerics.Vector3;   // the broad-phase float AABB corner; the seam Rasm.Element.Vector3 would collide unaliased
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard      = new("HARD");
    public static readonly ClashKind Clearance = new("CLEARANCE");
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The kernel proximity PORT (the seam GeometrySource idiom — an app-wired record of decode legs, one per return
// kind, never an interface floor for a two-arrow contract): the host-neutral systems owner produces the request
// keyed by the seam RepresentationContentHash body content key, the kernel Rasm geometry owner resolves the
// content-keyed geometry and evaluates the solid distance/intersection — never a host geometry type crossing
// this signature. Bounds returns the System.Numerics AABB corners the SwiftBVH BoundVolume takes directly (the
// float narrowing pads the broad phase only; Test stays double-precise).
public readonly record struct InterferenceQuery(UInt128 First, UInt128 Second, double ClearanceThreshold);

public readonly record struct ProximityResult(double Gap, double ClosestApproach);

public readonly record struct GeometryProximity(
    Func<UInt128, (Vector3 Min, Vector3 Max)> Bounds,
    Func<InterferenceQuery, Fin<ProximityResult>> Test);

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Interference(
    NodeId First,
    NodeId Second,
    ClashKind Kind,
    double Deficit,
    IfcDomain FirstDomain,
    IfcDomain SecondDomain) {
    const double DisciplineWeight = 1_000_000d;

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
    static readonly FrozenSet<IfcDomain> RunDomains =
        new[] { IfcDomain.HvacFire, IfcDomain.Electrical, IfcDomain.Plumbing }.ToFrozenSet();

    // The clearance-envelope property policy: the wider of a member's insulation/maintenance properties (a new
    // clearance source is one row), read off the baked element's effective property bags — never a hardcoded table.
    static readonly Seq<PropertyName> ClearanceKeys = Seq(
        PropertyName.Create("InsulationThickness"), PropertyName.Create("Clearance"), PropertyName.Create("MaintenanceClearance"));

    public static Fin<Seq<Interference>> Interferences(ElementGraph graph, GeometryProximity proximity, double clearanceThreshold, Op key) {
        // Every body-carrying OCCURRENCE enters (a Type's shared representation is un-placed geometry, never a clash
        // body); the Run flag tags the MEP side the pair admission requires, and the clearance envelope resolves
        // ONCE per member here (one memoized Bake + bag walk) rather than once per candidate pair.
        var members = graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Occurrence)
            .Choose(o => IfcClass.TryGet(o.Classification.Code)
                .Bind(c => o.Representations.Body.Map(body =>
                    (o.Id, c.Domain, Body: body, Run: RunDomains.Contains(c.Domain), Clearance: ClearanceOf(graph, o.Id, key)))))
            .ToSeq();
        return Candidates(members, proximity)
            .TraverseM(pair => Clash(members[pair.A], members[pair.B], proximity, clearanceThreshold)).As()
            .Map(static rows => rows.Somes().OrderByDescending(static c => c.Rank).ToSeq());
    }

    // The SwiftBVH broad-phase replaces the O(N^2) all-pairs test: each member's content-keyed AABB inserts once,
    // each Query returns only the bounds-overlapping candidate set, and the RUN guard drops a static-vs-static pair
    // before the precise test. The unordered pair stays single through the j > i
    // guard (an unordered {i,j} is produced exactly once, at the outer index i), and the per-i SwiftSparseSet keyed on
    // the candidate index j (cleared per query) dedups a duplicate Query hit — the sparse backing stays O(N), never the
    // O(N^2) a global i*N+j key forces (re-introducing the all-pairs memory the broad-phase exists to kill, and
    // overflowing int past ~46k members). The structure is a CoordinationRule tuning knob (SwiftBVH/SwiftOctree/
    // SwiftSpatialHash share Insert/Query); SwiftBVH SAH-cost insertion is the default. The loop fill is the
    // package's imperative sink contract (Insert / Query into ICollection<int>) — the named platform seam.
    static Seq<(int A, int B)> Candidates(Seq<(NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance)> members, GeometryProximity proximity) {
        var volumes = members.Map(m => proximity.Bounds(m.Body)).Map(static b => new BoundVolume(b.Min, b.Max)).ToArray();
        var bvh = new SwiftBVH<int>(Math.Max(2, volumes.Length));
        for (int i = 0; i < volumes.Length; i++) { bvh.Insert(i, volumes[i]); }
        var seen = new SwiftSparseSet();
        var overlaps = new SwiftList<int>();
        var pairs = Seq<(int A, int B)>();
        for (int i = 0; i < volumes.Length; i++) {
            overlaps.Clear();
            seen.Clear();
            bvh.Query(volumes[i], overlaps);
            foreach (int j in overlaps) {
                if (j <= i || !(members[i].Run || members[j].Run) || !volumes[i].Intersects(volumes[j])) { continue; }
                if (seen.Add(j)) { pairs = pairs.Add((i, j)); }
            }
        }
        return pairs;
    }

    static Fin<Option<Interference>> Clash(
        (NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance) a, (NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance) b,
        GeometryProximity proximity, double clearanceThreshold) {
        double threshold = Math.Max(clearanceThreshold, Math.Max(a.Clearance, b.Clearance));
        return proximity.Test(new InterferenceQuery(a.Body, b.Body, threshold)).Map(result => Classify(a, b, result, threshold));
    }

    static Option<Interference> Classify(
        (NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance) a, (NodeId Id, IfcDomain Domain, UInt128 Body, bool Run, double Clearance) b,
        ProximityResult result, double threshold) =>
        result.Gap < 0d
            ? Some(new Interference(a.Id, b.Id, ClashKind.Hard, -result.Gap, a.Domain, b.Domain))
        : result.Gap < threshold
            ? Some(new Interference(a.Id, b.Id, ClashKind.Clearance, threshold - result.Gap, a.Domain, b.Domain))
            : None;

    // The pair clearance reads the wider of the two members' insulation/maintenance envelopes off the BAKED element's
    // effective property bags through the ClearanceKeys policy (the seam owns the type->occurrence merge), an absent
    // property reading 0d so the flat clearanceThreshold floors it — never a hardcoded per-discipline gap table.
    static double ClearanceOf(ElementGraph graph, NodeId member, Op key) =>
        graph.Bake(member, key).ToOption().Match(
            Some: element => ClearanceKeys
                .Choose(name => element.Properties.Choose(bag => bag.Find(name)).Head)
                .Choose(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None)
                .Fold(0d, static (max, si) => Math.Max(max, si)),
            None: static () => 0d);
}
```

## [05]-[RESEARCH]

- [SEAM_VIEW_NOT_PROJECTION]: the connectivity layer reads the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` rather than re-projecting GeometryGym — `Projection/semantic#SEMANTIC_PROJECTOR` is the SOLE IFC owner (`ELEMENT-REBUILD-PLAN.md` §4A: GeometryGym stays sole in `Rasm.Bim`; the projector lowers `DatabaseIfc` to a seam `GraphDelta`), and its `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.All` already folds `IfcRelConnectsPorts` (`IfcRelKind.ConnectsPorts`, `EdgeAxis.Connect` sub-kind `port`), `IfcRelConnectsPortToElement` (`IfcRelKind.ConnectsPortToElement`, `EdgeAxis.Generic` — the port↔element shape distinct from the port↔port `Connect`), `IfcRelNests` (`EdgeAxis.Compose` sub-kind `nest`, the IFC4 port containment), `IfcRelAssignsToGroup` (`EdgeAxis.Assign`), and `IfcRelServicesBuildings` (the `Generic` passthrough) onto neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` edges; the connectivity reads them exactly as `Model/query#ELEMENT_SET` reads the same graph (`ElementGraph.ObjectNodes`/`EdgesAt`/`Find`, `Relationship.Connect`/`Compose`/`Assign`/`Generic`, `Node.Object.Classification`/`PredefinedType`/`ExternalId`, `Relationship.Relating`/`Related`), so the retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold and the `BimModel`/`BimElement` binding it required are GONE (`ELEMENT-REBUILD-PLAN.md` §2 element collapse, §6 `Rasm.Bim` ripple). The FULL `IfcDistributionSystemEnum` member set the `DistributionSystemKind` `[SmartEnum]` keys over — the 42 IFC4 disciplines (`AIRCONDITIONING`…`WATERSUPPLY`, the DISTINCT `DATA` and `COMMUNICATION` rows, never a fused `DATACOMMUNICATION`), the seven IFC4X3 rail-electrification/telephony additions (`CATENARY_SYSTEM`/`OVERHEAD_CONTACTLINE_SYSTEM`/`RETURN_CIRCUIT`/`FIXEDTRANSMISSIONNETWORK`/`OPERATIONALTELEPHONYSYSTEM`/`MOBILENETWORK`/`MONITORINGSYSTEM`), and the IFC4X4 `SAFETY` draft, plus the `USERDEFINED`/`NOTDEFINED` fallback — and `IfcFlowDirectionEnum` (`SOURCE`/`SINK`/`SOURCEANDSINK`/`NOTDEFINED`) the `FlowDirection` keys over, both rosters decompile-verified against GeometryGymIFC_Core 25.7.30 (`.api/api-geometrygym-ifc`); the system-group entity types `IfcDistributionSystem`/`IfcDistributionCircuit`/`IfcSystem` plus the `IfcBuiltSystem`/`IfcBuildingSystem` IFC4.3 rename PAIR (the catalog documents `IfcBuildingSystem` with `IfcBuildingSystemTypeEnum` at 25.7.30; GG retains deprecated entities, so BOTH codes are carried — an absent name never matches and the emitter census pins the shipped set) and the `IfcDistributionPort.FlowDirection`/`PredefinedType` (`IfcDistributionPortTypeEnum` `CABLE`/`CABLECARRIER`/`DUCT`/`PIPE`/`WIRELESS`) port vocabulary ground against the same catalog (the distribution-element/port/system rows), the kinds resolved off the seam node's `PredefinedType` token rather than the GeometryGym enum so this owner carries no `GeometryGym.Ifc` import.
- [PORT_CONNECTION_COLLAPSE]: the retired `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) mirrored two typed `IfcRel*` cases — exactly the seventeen-typed-`IfcRel*`-case design the seam's neutral edge algebra rejected (`ELEMENT-REBUILD-PLAN.md` §4-RT C5: the seam carries `Compose`/`Assign`/`Associate`/`Connect`/`Void` + `Generic`, the IFC relationship roster living in the Bim projector) — so the connectivity reads the neutral edges as the `IfcRelKind` roster lands them — the FLOW edge the `Relationship.Connect` (`From`/`To`/`SubKind`/`Realizing`, `ConnectKind.Port`) both of whose endpoints classify `IfcDistributionPort`, the OWNERSHIP edge one of the two containment forms — never a typed connection union; port OWNERSHIP is schema-dual — IFC2x3 binds a port to its element through `IfcRelConnectsPortToElement`, which the roster lands as a `Generic` edge carrying its wire-name with the PORT on the relating side (`RelatingPort`/`RelatedElement`), while IFC4 nests ports under their element through `IfcRelNests`, landing as `Compose{Nest}` (`IfcPort.ContainedIn` retained for back-compatibility) — so `PortsOf` reads BOTH edge kinds and dedupes on the port id: an ownership probe against `Connect{Port}` (an edge kind the roster never emits for containment) is a dead read yielding an EMPTY port set for every schema, and a `Generic`-only read silently loses every IFC4-nested port; the realizing fitting rides the `Connect.Realizing` `Option<NodeId>` (the `IfcRelConnectsPorts.RealizingElement` projection), and the unordered-pair dedupe on the `(From, To)` `NodeId` pair is the verified single-edge invariant.
- [QUIKGRAPH_TRACE]: the `SystemTrace` reachability fold is the shared `QuikGraph` `[GRAPH_ALGORITHM]` owner the `libs/csharp/.api/api-quikgraph` `Model/systems#SYSTEM_TRACE` law mandates — "the `PortConnection` edges fold into an `AdjacencyGraph`/`UndirectedGraph` over port keys; `graph.TreeBreadthFirstSearch(seedPort)` returns the `TryFunc` whose reachable domain IS the downstream closure, replacing the hand-rolled `SystemNetwork.Walk` visited-set fold" — so the retired tail-recursive `Closure` visited-set walk is the named deleted form; the fold builds a transient `AdjacencyGraph<NodeId, SEdge<NodeId>>` (`AddVerticesAndEdge`, the value `SEdge<NodeId>` allocating nothing, the dense `NodeId`-keyed network) over BOTH ports and elements (the ownership edges crossing each fitting so the closure traverses a tee's inlet→element→outlets), runs `AlgorithmExtensions.TreeBreadthFirstSearch`, and projects the reached vertex set back onto the reached-element/reached-port `NodeId` sets (the `VertexPredecessorRecorderObserver.TryGetPath` returning `false` for the root, so the seed is admitted explicitly); the `TraceMode` orientation reads BOTH endpoint `FlowDirection`s (a directed leg exists only where the emitting side `Emits` AND the receiving side `Receives`) so the directed downstream/upstream closure walks the real flow path while a `NotDefined` pair stays bidirectional and a contradictory source-facing-source edge severs honestly, the same shared owner the `Planning/schedule#CRITICAL_PATH` `SourceFirstTopologicalSort` and the `Review/versioning#VERSION_GRAPH` `OfflineLeastCommonAncestor` compose, never N bespoke walks.
- [INTERFERENCE_PROXIMITY]: the `Interference` clash fold binds the kernel `Rasm` geometry by reference through the injected `GeometryProximity` seam — the host-neutral systems owner produces the `InterferenceQuery` keyed by the seam `Rasm.Element/Graph/element#NODE_MODEL` `RepresentationContentHash.Body` `UInt128` body content key (no host geometry type, no GeometryGym surface) and reads the `ProximityResult` signed gap back, the kernel owner resolving the content-keyed geometry and evaluating the solid distance so a RhinoCommon `Brep`/`Mesh` overlap test never crosses this lane; the candidate pair set is generated by the `SwiftCollections.Lean` `SwiftBVH<int>` broad-phase (`.api/api-swiftcollections`: `new SwiftBVH<int>(capacity)`, `Insert(key, BoundVolume)`, `Query(BoundVolume, ICollection<int>)`, `BoundVolume(Vector3 min, Vector3 max)`/`Intersects`, `SwiftSparseSet.Add` pair dedupe, `SwiftList<int>` result sink) over the `proximity.Bounds(bodyKey)` AABB so only bounds-overlapping pairs reach the precise `proximity.Test` — broad (SwiftBVH AABB) then narrow (kernel solid distance), the `SwiftOctree`/`SwiftSpatialHash` interchangeable behind the shared `Insert`/`Query` contract and `SwiftBVH.UpdateEntryBounds` the incremental refit a `Review/diff#MODEL_DIFF` `moved` arm drives; the member set is every body-carrying occurrence off the seam graph partitioned by `Model/elements#IFC_CLASS` the `IfcClass` row's `Domain` — RUN `{HvacFire, Electrical, Plumbing}` versus STATIC (every other domain: the Architecture-domain built elements land STATIC because `IfcClass` roots `IfcBuiltElement` on `Architecture`), a pair admitted when at least one side is a run — and the clearance reads the BAKED element's `Semantics/properties#PROPERTY_TEMPLATES` effective property bags through the `ClearanceKeys` policy (the seam `Bake` owning the type→occurrence merge), the ranked `Seq<Interference>` the `Review/coordination#COORDINATION` `ClashProposal` substrate (the coordination owner resolving each `NodeId`'s `ExternalId` for the BCF viewpoint), so the coordination owner consumes this evidence rather than re-deriving proximity.
- [SERVED_STRUCTURES_AND_FLOW_DIRECTION]: the served spatial-structure set rides the `Generic("IfcRelServicesBuildings")` neutral passthrough [C5] — `EdgeProjection.Generics` fans the relationship out (`IfcSystem.ServicesBuildings` → `RelatedBuildings`, decompile-verified on the `IfcSystem` base) onto `Generic` edges carrying the `IfcRelKind.ServicesBuildings.Key` wire-name, so the served set reads off the system node's incident `Generic` edges. The port `FlowDirection` the directed trace orients on is read off the port `Node.Object`'s neutral `"FlowDirection"` property — the direct IFC attribute `IfcDistributionPort.FlowDirection` (decompile-verified `IfcFlowDirectionEnum`, alongside `PredefinedType`/`SystemType`) carries no seam `Node.Object` column, so the directed trace DEPENDS on the `Projection/semantic#SEMANTIC_PROJECTOR` surfacing that attribute as a `"FlowDirection"` entry on a port `PropertySet` bag at ingest (a synthesized property, distinct from the `Bags` fold that lowers only real `IfcPropertySet`/`IfcElementQuantity`); absent that surfacing every port reads `NotDefined` (the port conducts both ways), so the directed trace degrades to undirected reachability rather than faulting — the orientation a correct refinement over the always-total `Reach` closure, never an illusory directed walk, the host-neutral systems view never reading the GeometryGym attribute itself.


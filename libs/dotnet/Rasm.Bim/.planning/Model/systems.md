# [SYSTEMS_CONNECTIVITY]

The host-neutral MEP distribution-system connectivity layer is a VIEW over the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second GeometryGym lowering: the `Projection/semantic#SEMANTIC_PROJECTOR` projector is the SOLE IFC owner and has already lowered every `IfcDistributionSystem`, `IfcDistributionPort`, `IfcRelConnectsPorts`, `IfcRelConnectsPortToElement`, `IfcRelNests` (the IFC4 port-containment form), `IfcRelAssignsToGroup`, and `IfcRelServicesBuildings` onto NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` edges — a `Connect{Port}` port-to-port FLOW edge, a `Generic("IfcRelConnectsPortToElement")`/`Compose{Nest}` port-OWNERSHIP pair (the IFC2x3 and IFC4 containment forms), an `Assign{Group}` membership edge, and a `Generic("IfcRelServicesBuildings")` passthrough — so this layer reads the settled graph and folds it into a typed flow-network view, a `QuikGraph` reachability trace, and a `SwiftCollections.Lean` interference clash, exactly as the `Model/query#ELEMENT_SET` query surface reads the same graph. The retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold and the `PortConnection` `[Union]` mirroring `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts` are GONE — the typed-`IfcRel*`-case shape is the deleted form the contract's neutral edge algebra collapsed [NEUTRAL_EDGE_RULING], the connectivity now reading the neutral edges by wire-name and endpoint classification (a flow edge joins two `IfcDistributionPort` objects; an ownership edge joins a port to its distribution element through either containment form).

The connectivity layer is the network GRAPH, orthogonal to the `Model/zones#ZONE_GRAPH` overlay that owns the LOGICAL membership (which elements belong to a system, the `Assign{Group}` edge) — the connectivity owns the typed flow ADJACENCY (how they connect, the `Connect{Port}` edge), the two coexisting and never collapsed. An air-handling system threading a hundred ducts, a domestic-water riser feeding every fixture, and an electrical distribution board powering every circuit each surface their member set, their typed port adjacency, and their served spatial structures from one `DistributionSystem` view, never a per-discipline system type. Identity is the shared `Rasm.Element/Graph/element#NODE_MODEL` `NodeId` (a rooted Guid-v7), never an IFC `GlobalId` — the compressed GlobalId is the node's `ExternalId` projection attribute the trace and interference carry only for the IFC-keyed downstream consumers. The layer is HOST-NEUTRAL: it joins nodes by `NodeId`, references geometry by the shared `RepresentationContentHash` content key, routes every solid-proximity test to the injected kernel `GeometryProximity` port, and never carries a RhinoCommon binding or an in-process tessellation — the same contract law the `Model/zones#ZONE_GRAPH` overlay and the `Model/structural#STRUCTURAL_PROJECTION` graph hold. The reachability fold and the system view are TOTAL over the already-validated graph (the dangling-endpoint rejection lowered at `Projection/semantic#SEMANTIC_PROJECTOR` `Project` and `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Apply`); only the interference clash carries a `Fin<T>` result, lowering `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Capability` BARE when a member's geometry content key resolves to no kernel geometry.

## [01]-[INDEX]

- [02]-[CONNECTIVITY]: the `DistributionSystem` derived view (member `NodeId` set, nested sub-circuit set, typed `DistributionPort` set, port `FlowEdge` set, served-structure set, `(MembershipKey, TopologyKey)` identity), the `DistributionSystemKind` `[SmartEnum<string>]` over `IfcDistributionSystemEnum` with its `IfcDomain` discipline and `FlowMedium` carrier columns, the `FlowDirection` `[SmartEnum<string>]` over `IfcFlowDirectionEnum`, the `DistributionNetwork` fold reading the shared `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views, and `DistributionNetwork.BrickProjection` the Brick Schema operations overlay lowering the settled view onto `BrickSystem`/`BrickSpace` nodes with `PartOf`/`PointOf`/`Fedby`/`LocationOf` edges through the injected `BrickBinding` class election, minting the `BrickGraph` the app-platform live-binding seat persists.
- [03]-[SYSTEM_TRACE]: the `SystemTrace` reachability fold over the contract's port-and-element flow graph — one transient `QuikGraph` `AdjacencyGraph<NodeId, SEdge<NodeId>>` built from the `Connect{Port}` edges, the `TraceMode` orientation policy (reach/downstream/upstream) reading the port `FlowDirection`, the `BreadthFirstSearchAlgorithm` event fold computing the depth-carrying reachable-element closure — `TraceHop` rows pairing every reached node with its ELEMENT-hop distance from the seed, the bare reach a derived projection (the shared `QuikGraph` graph-algorithm owner replacing the hand-rolled visited-set walk), the `Demand` downstream accumulation reducing reached-terminal design values through the query `SumOf` composition, and the `Runs` index-run ranking ordering every reached terminal by its best-route resistance from the seed over the same oriented adjacency.
- [04]-[INTERFERENCE]: `Interference` carries the clash evidence, `GeometryProximity` the injected kernel port, `ClashIndex` the retained two-structure broad phase (`SwiftBVH` hard overlap, `SwiftSpatialHash` clearance ring, `SwiftBucket` handle registry), and `InterferenceCheck.Build`/`Candidates`/`Neighborhood`/`Interferences`/`Refit` the folds over it.

## [02]-[CONNECTIVITY]

- Owner: `DistributionSystem` the single host-neutral derived VIEW of one MEP distribution system read from the shared `ElementGraph` — the system group `NodeId`, the `ExternalId` (the IFC `GlobalId` projection attribute), the `DistributionSystemKind` discriminant resolved off the group node's `PredefinedType`, the member `NodeId` set, the `Circuits` sub-circuit subset (the members themselves classified as system groups — an `IfcDistributionCircuit` under its parent board), the typed `DistributionPort` set, the port-to-port `FlowEdge` set the flow network is built from, and the served spatial-structure `NodeId` set, with a derived `(MembershipKey, TopologyKey)` content-key identity the trace re-reads the network by; `DistributionSystemKind` the closed `[SmartEnum<string>]` keyed over the `IfcDistributionSystemEnum` member set with a `Domain` column resolving each kind onto the `Model/elements#IFC_CLASS` `IfcDomain` partition; `FlowDirection` the `[SmartEnum<string>]` over `IfcFlowDirectionEnum` carrying the kernel `CapabilitySet<PortCapability>` conduction column the directed trace reads under its own `CapabilityLaw` corner set; `DistributionPort` the derived port view (`NodeId`, name, `FlowDirection`, the `PredefinedType` port kind, and the owning distribution element `NodeId`); `DistributionNetwork` the static fold reading the element graph's `Assign{Group}`/`Connect{Port}`/`Compose{Nest}`/`Generic` edges into the typed views AND lowering them onto the Brick Schema operations graph; `BrickBinding` the injected Brick class-election port (the composition mints each equipment and point node, this fold wires the topology around it); `BrickGraph` the minted `BrickSchemaManager` beside the measured `Unbound` residue.
- Cases: `DistributionSystemKind` rows span the FULL `IfcDistributionSystemEnum` distribution vocabulary partitioned across the `IfcDomain` set — air/thermal-fluid/combustion-fuel/fire-and-life-safety (`AirConditioning`/`Ventilation`/`ChilledWater`/`CompressedAir`/`Heating`/`Refrigeration`/`Fuel`/`Gas`/`Oil`/`FireProtection`/…) on `HvacFire`, piped water/drainage/waste/process (`DomesticColdWater`/`DomesticHotWater`/`WaterSupply`/`Drainage`/`Sewage`/`RainWater`/`StormWater`/`WasteWater`/`Chemical`/…) on `Plumbing`, power/lighting/telecom/data/signal/rail-traction (`Electrical`/`Lighting`/`Telephone`/the DISTINCT `Data` and `Communication` rows/`Security`/`CatenarySystem`/…) on `Electrical`, and `Conveying` on `Architecture` — every IFC4 discipline and the seven IFC4X3 rail-electrification/telephony additions (`CatenarySystem`/`OverheadContactLine`/`ReturnCircuit`/`FixedTransmissionNetwork`/`OperationalTelephony`/`MobileNetwork`/`MonitoringSystem`) each frozen with its `IfcDomain` discipline AND its `FlowMedium` carrier (the physical medium the token implies — the demand/sizing partition beyond the discipline), with the `UserDefined`/`NotDefined` fallback rows the `Of` resolver lowers an unmapped token onto — carried on the generic `Architecture` domain so an unclassified system never pollutes a `ByDomain(IfcDomain.HvacFire)` discipline selection — the closed buildingSMART roster, never a partial slice and never a fused `DataCommunication` phantom (the enum carries `DATA` and `COMMUNICATION` separately, no `DATACOMMUNICATION` token); `FlowDirection` rows `Source` (emits) · `Sink` (receives) · `SourceAndSink` (both) · `NotDefined` (both — an undirected port conducts either way), the full `IfcFlowDirectionEnum`; a `DistributionPort` is a `Source`/`Sink` port on a `FlowSegment` owner, a tee fitting carries three ports, and a `FlowEdge` joins two ports across elements (the `IfcRelConnectsPorts` flow connection) carrying its optional realizing fitting `NodeId`.
- Entry: `DistributionNetwork.View(ElementGraph graph, Option<NodeId> scope)` folds either all distribution-system group nodes or one selected group into `Seq<DistributionSystem>`; the input value owns modality, and callers state `None` when they request the whole model. The read is total because graph admission already rejected dangling endpoints. An unmapped `PredefinedType` resolves to `DistributionSystemKind.NotDefined`. `DistributionNetwork.BrickProjection(ElementGraph graph, Seq<DistributionSystem> systems, BrickBinding binding)` lowers those settled views onto one `BrickGraph` — a `BrickSystem` per system and per nested circuit, the binding's elected equipment node per member joined `PartOf` its collection, its ports joined `PointOf` their equipment, each flow edge joined `Fedby` from fed to feeder under the `SystemTrace.Orient` downstream law, and each served spatial structure a `BrickSpace` the collection joins `LocationOf`; the fold is total and reports the members the binding elected no class for on `BrickGraph.Unbound`.
- Auto: `View` reads the group `Node.Object` set classified as a distribution system, and `Of` folds one — `MembersOf` reads the group's incident `Assign{Group}` edges with the system PINNED as the `Definition` endpoint (the projector's INVERTED `Assign`: `Subject` = member, `Definition` = group — the same directional pin the zones read holds, so a nested system's own membership in a parent group never folds the parent into its member set), the `Circuits` subset filtering the members whose own node is system-classified (an `IfcDistributionCircuit` rides as a member AND as its own `DistributionSystem` row); `PortsOf` reads each member's incident port-OWNERSHIP edges over BOTH containment forms — the `Generic` edge carrying the `IfcRelKind.ConnectsPortToElement.Key` wire-name (the IFC2x3 projection, its port on the relating side) and the `Compose{Nest}` edge whose part is a port (the IFC4 `IfcRelNests` port-containment projection) — onto deduped `DistributionPort` rows carrying the port node's name, its `PredefinedType` kind, and its `FlowDirection` read off the port's effective `FlowDirection` property the projector lowers, so an IFC4 model whose ports nest under their elements loses no port; `FlowEdgesOf` reads the `Connect{Port}` FLOW edges whose BOTH endpoints are in the system's port set (the `IfcRelConnectsPorts` projection, deduped on the unordered port pair so a connection materialized from either incident port rides one edge, carrying the optional realizing fitting from the `Connect.Realizing`); and `ServedOf` reads the group's incident `Generic` edges whose wire-name is `IfcRelKind.ServicesBuildings.Key` (the served spatial structures riding the neutral passthrough [NEUTRAL_EDGE_RULING], `EdgeProjection.Generics` fanning `IfcSystem.ServicesBuildings` → `RelatedBuildings` onto one `Generic` edge per served structure with the system relating); the two ownership forms are schema-dual and orientation-distinct — IFC2x3 `IfcRelConnectsPortToElement` lands as `Generic` with the PORT relating (`RelatingPort`/`RelatedElement`), IFC4 `IfcRelNests` as `Compose{Nest}` with the port the part (`IfcPort.ContainedIn` retained for back-compatibility) — so `PortsOf` reads BOTH and dedupes on the port id, while the realizing fitting on a flow edge is the `IfcRelConnectsPorts.RealizingElement` projection riding `Connect.Realizing` and the unordered `(From, To)` pair dedupe is the single-edge invariant; the port `FlowDirection` reads off the port node's synthesized `"FlowDirection"` bag entry because the IFC `IfcDistributionPort.FlowDirection` attribute (alongside `PredefinedType`/`SystemType`) has no shared `Node.Object` column, so a model whose projector did not surface it traces undirected rather than faulting; the `Identity` fold derives the `(MembershipKey, TopologyKey)` `UInt128` pair through the kernel seed-zero `Rasm.Domain.ContentHash.Of` — `MembershipKey` over the ordered member `NodeId` set and `TopologyKey` over the sorted flow-edge unordered port pairs — so a consumer re-walks only a changed membership or a changed adjacency, the single seed-zero hasher the shared `NodeId`/`ContentAddress` also compose, never a second hasher.
- Output: the `Seq<DistributionSystem>` is the connectivity evidence the `[03]-[SYSTEM_TRACE]` `SystemTrace` fold walks and the `[04]-[INTERFERENCE]` clash pairs the distribution members from; the `Model/zones#ZONE_GRAPH` MEP grouping reads the member set by reference; the air-handling system, the water riser, and the electrical board each carry their member set, their nested sub-circuits, their typed port adjacency, and their served structures on one record; the `BrickGraph` is the operations-phase evidence the app-platform live-binding seat persists as JSON-LD through `BrickSchemaManager.SaveSchema` and streams live point values onto, so a BMS reads the SAME network the coordination lane clashes against rather than a hand-kept twin, and `Unbound` is the honest completeness measure of the composition's class election.
- Packages: Rasm.Element, Rasm, BrickSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new distribution discipline is one `DistributionSystemKind` row reading the next `IfcDistributionSystemEnum` token with its `IfcDomain` AND `FlowMedium` columns (the medium the demand/sizing consumers partition on — air, water, gas, liquid, electricity, signal, solid); a new flow direction is one `FlowDirection` row with its `PortCapability` conduction set, refused at construction unless it lands on a declared corner; a new port-containment or membership relationship rides the existing boundary edge kinds the fold already reads; a new Brick equipment or point class is one arm on the composition's `BrickBinding` election with zero edit here, and a new Brick relation is one `AddRelationship<T>` beside the view column that already resolves its endpoints; never a per-discipline system record, never a second connectivity store, never a per-relationship connection class, and never a Brick class roster carried as rows on this page.
- Boundary: `DistributionSystem` is ONE derived view discriminated by the `DistributionSystemKind` row data — an `HvacSystem`/`ElectricalSystem`/`PlumbingSystem` class family or sibling per-discipline factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the retired `PortConnection` `[Union]` (`ConnectsPortToElement`/`ConnectsPorts`) is GONE — it mirrored the typed `IfcRel*` cases the contract's neutral `Connect` algebra collapsed [NEUTRAL_EDGE_RULING], and re-introducing a typed connection union is the named drift, the connectivity reading the neutral edges as the `IfcRelKind` roster lands them — `Connect{Port}` the port-to-port FLOW edge, `Generic("IfcRelConnectsPortToElement")` + `Compose{Nest}` the two port-OWNERSHIP containment forms — and an ownership read against an edge kind the roster never emits (the prior `Connect{Port}`-ownership probe, dead against the `EdgeAxis.Generic` row) or one that drops every IFC4-nested port is the deleted illusory read; the retired `DistributionSystemProjection.Project(IfcDistributionSystem, BimModel)` GeometryGym fold is GONE and a `GeometryGym.Ifc` import crossing this owner is the named contract violation — `Projection/semantic#SEMANTIC_PROJECTOR` is the sole IFC lowering and this owner reads the resulting element graph and its neutral `Connect`/`Compose`/`Assign`/`Generic` edges alone; `SystemClasses` COMPOSES the `Model/zones#ZONE_GRAPH` `BimZoneKind` distribution rows — the grouping vocabulary has ONE owner, so a re-spelled entity-name literal here is the deleted fork and an unverified foreign alias never enters the classifier; the port `FlowDirection` bag key composes the Element-declared `PortRows.FlowDirection` static the ingest `SourceBag` stamps (the `BoundaryRows` custody — Element declares, Bim stamps and reads), and a call-site mint at either end is the deleted form; identity is the shared `NodeId` and a `GlobalId`-keyed view is the deleted form (the `GlobalId` is the node `ExternalId` the IFC-keyed consumers read); the served-structure set rides the `Generic("IfcRelServicesBuildings")` passthrough; the distribution-domain element selection is the `Model/query#ELEMENT_SET` `ByDomain(IfcDomain.HvacFire)`/`ByDomain(IfcDomain.Electrical)`/`ByDomain(IfcDomain.Plumbing)` predicate and a parallel system-element selection arm is the no-second-selection-surface reject; the connectivity graph is the orthogonal companion to the `Model/zones#ZONE_GRAPH` logical membership — the zone overlay owns which elements belong, the connectivity owns how they connect, the two never collapsed; the `(MembershipKey, TopologyKey)` identity is the `Rasm.Domain.ContentHash.Of` seed-zero key and a second identity scheme is the named drift defect; the Brick overlay READS the settled `DistributionSystem` view and re-derives nothing — a second element-graph traversal, a second flow-orientation rule beside `SystemTrace.Orient`, or a Brick node minted for a member the binding elected no class for (a generic placeholder standing in for a real class) is the deleted form, the unelected member landing on `BrickGraph.Unbound` as measured residue; the Brick entity id IS the shared `NodeId` string so a Brick node joins back to its `Object` node with no side map, the IFC `GlobalId` riding `AddOrUpdateProperty` for the handover readers; the `BrickSchema.Net` surface is consumed verbatim as settled vocabulary (`.api/api-brickschema-net`) — `new BrickSchemaManager()`, the ONE polymorphic `AddEntity<T>(string? id, string? name)` mint constrained `where T : BrickEntity, new()` (never an `AddEquipment*`/`AddCollection*` named-factory pick), `GetEntity(string id, bool byReference)` taken `byReference: true` so a relationship lands on the live node rather than a read-only clone, `BrickEntity.AddRelationship<T>(string parentId)`, `AddOrUpdateProperty<T>`, and `SaveSchema` — over the `Fedby`/`PointOf`/`PartOf`/`LocationOf` relationship classes and the `Classes.Collection.System`/`Classes.Locations.Space` taxonomy nodes, `System` and `Space` binding through file aliases because each bare name captures the BCL root namespace or a sibling noun inside every member of this file; the Brick equipment/point CLASS election and the live-point `BACnetReference`/`ModbusDevice` binding are the composition's through `BrickBinding` (the `Rasm.AppHost` `Wire/livewire` transport axis owns the external source), because `AddEntity<T>` takes its class as a type argument over a generated taxonomy family — a Brick class roster carried as rows here, or a `BrickSchema.Net` type crossing any other signature on this page, is the named contract violation; conformance analytics (`BrickBehavior`, `Aggregation.AggregateByInterval`) attach at that same seat over live point streams, never in this fold, which carries no clock and no series.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using BrickSchema.Net;
using BrickSchema.Net.Relationships;
using LanguageExt;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using BrickSpace = BrickSchema.Net.Classes.Locations.Space;
using BrickSystem = BrickSchema.Net.Classes.Collection.System;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PortCapability : ICapability<PortCapability> {
    public static readonly PortCapability Emit    = new("emit",    rank: 0);
    public static readonly PortCapability Receive = new("receive", rank: 1);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class FlowDirection {
    public static readonly FlowDirection Source        = new("SOURCE",        CapabilitySet<PortCapability>.Of(PortCapability.Emit));
    public static readonly FlowDirection Sink          = new("SINK",          CapabilitySet<PortCapability>.Of(PortCapability.Receive));
    public static readonly FlowDirection SourceAndSink = new("SOURCEANDSINK", CapabilitySet<PortCapability>.All);
    public static readonly FlowDirection NotDefined    = new("NOTDEFINED",    CapabilitySet<PortCapability>.All);

    private static readonly CapabilityLaw<PortCapability> Conduction = new(Seq(
        CapabilitySet<PortCapability>.Of(PortCapability.Emit),
        CapabilitySet<PortCapability>.Of(PortCapability.Receive),
        CapabilitySet<PortCapability>.All));

    public CapabilitySet<PortCapability> Conducts { get; }

    static partial void ValidateConstructorArguments(ref string key, ref CapabilitySet<PortCapability> conducts) {
        if (Conduction.Admit(conducts).IsFail) { throw new ArgumentException($"<flow-direction-corner:{key}>", nameof(conducts)); }
    }

    public static FlowDirection Of(string? token) => TryGet(token?.Trim(), out FlowDirection? direction) ? direction : NotDefined;
}

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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DistributionPort(NodeId Id, string Name, FlowDirection Flow, PredefinedType Kind, NodeId Owner);

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
    public (UInt128 MembershipKey, UInt128 TopologyKey) Identity { get; } = (
        ContentHash.Of(Members, static (rows, canon) => canon.Sorted(
            rows, static id => id.ToValue(), StringComparer.Ordinal, static (id, w) => w.String(id.ToValue()))),
        ContentHash.Of(
            Flow.Map(static f => string.CompareOrdinal(f.From.ToValue(), f.To.ToValue()) <= 0
                ? (Low: f.From.ToValue(), High: f.To.ToValue())
                : (Low: f.To.ToValue(), High: f.From.ToValue())),
            static (rows, canon) => canon.Sorted(
                rows, static pair => $"{pair.Low}{pair.High}", StringComparer.Ordinal,
                static (pair, w) => w.String(pair.Low).String(pair.High))));
}

public sealed record BrickGraph(BrickSchemaManager Manager, Seq<NodeId> Unbound);

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct BrickBinding(
    Func<BrickSchemaManager, Node.Object, Option<BrickEntity>> Equipment,
    Func<BrickSchemaManager, DistributionPort, Option<BrickEntity>> Point);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DistributionNetwork {
    private static readonly FrozenSet<string> SystemClasses = new[] {
        BimZoneKind.DistributionSystem.Key, BimZoneKind.DistributionCircuit.Key,
        BimZoneKind.System.Key, BimZoneKind.BuiltSystem.Key,
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private const string PortClass = "IfcDistributionPort";

    public static Seq<DistributionSystem> View(ElementGraph graph, Option<NodeId> scope = default) =>
        scope.Match(
            None: () => graph.ObjectNodes.Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o)),
            Some: id => graph.Find<Node.Object>(id).Filter(static o => SystemClasses.Contains(o.Classification.Code)).Map(o => Of(graph, o)).ToSeq());

    private static DistributionSystem Of(ElementGraph graph, Node.Object system) {
        Seq<NodeId> members = MembersOf(graph, system.Id);
        Seq<NodeId> circuits = members.Filter(m => graph.Find<Node.Object>(m).Map(static o => SystemClasses.Contains(o.Classification.Code)).IfNone(false));
        Seq<DistributionPort> ports = members.Bind(m => PortsOf(graph, m));
        Seq<FlowEdge> flow = FlowEdgesOf(graph, toHashSet(ports.Map(static p => p.Id)));
        return new DistributionSystem(
            system.Id, system.ExternalId, system.Name,
            DistributionSystemKind.Of(system.PredefinedType.ToValue()),
            members, circuits, ports, flow, ServedOf(graph, system.Id));
    }

    private static Seq<NodeId> MembersOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.Group && a.Definition == system
                ? Some(a.Subject) : None);

    private static Seq<DistributionPort> PortsOf(ElementGraph graph, NodeId member) =>
        toSeq(graph.EdgesAt(member)
            .Select(e => e switch {
                Relationship.Generic g when string.Equals(g.WireName, IfcRelKind.ConnectsPortToElement.Key, StringComparison.Ordinal) && g.Related == member
                    => PortNode(graph, g.Relating),
                Relationship.Compose n when n.SubKind == ComposeKind.Nest && n.Whole == member => PortNode(graph, n.Part),
                _ => Option<Node.Object>.None,
            })
            .Somes()
            .Select(port => new DistributionPort(port.Id, port.Name, PortFlow(graph, port.Id), port.PredefinedType, member))
            .DistinctBy(static p => p.Id));

    private static Option<Node.Object> PortNode(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Filter(static o => string.Equals(o.Classification.Code, PortClass, StringComparison.OrdinalIgnoreCase));

    private static Seq<FlowEdge> FlowEdgesOf(ElementGraph graph, LanguageExt.HashSet<NodeId> ports) =>
        toSeq(ports.ToSeq()
            .Bind(port => toSeq(graph.EdgesAt(port)))
            .Choose(e => e is Relationship.Connect c && c.SubKind == ConnectKind.Port && ports.Contains(c.From) && ports.Contains(c.To)
                ? Some(new FlowEdge(c.From, c.To, c.Realizing)) : None)
            .DistinctBy(static f => string.CompareOrdinal(f.From.ToValue(), f.To.ToValue()) <= 0 ? (f.From.ToValue(), f.To.ToValue()) : (f.To.ToValue(), f.From.ToValue())));

    private static Seq<NodeId> ServedOf(ElementGraph graph, NodeId system) =>
        toSeq(graph.EdgesAt(system))
            .Choose(e => e is Relationship.Generic g && string.Equals(g.WireName, IfcRelKind.ServicesBuildings.Key, StringComparison.Ordinal) && g.Relating == system
                ? Some(g.Related) : None);

    public static BrickGraph BrickProjection(ElementGraph graph, Seq<DistributionSystem> systems, BrickBinding binding) {
        BrickSchemaManager brick = new();
        BrickLedger ledger = systems.Fold(BrickLedger.Empty, (held, system) => Lower(graph, brick, binding, held, system));
        return new BrickGraph(brick, ledger.Unbound.Distinct());
    }

    private readonly record struct BrickLedger(Map<NodeId, string> Minted, Seq<NodeId> Unbound) {
        public static readonly BrickLedger Empty = new(Map<NodeId, string>(), Seq<NodeId>());

        public BrickLedger Mint(NodeId node, string entity) => this with { Minted = Minted.AddOrUpdate(node, entity) };
        public BrickLedger Unbind(NodeId node) => this with { Unbound = Unbound.Add(node) };
    }

    private static BrickLedger Lower(
        ElementGraph graph, BrickSchemaManager brick, BrickBinding binding, BrickLedger held, DistributionSystem system) {
        BrickSystem collection = brick.AddEntity<BrickSystem>(system.Id.ToValue(), system.Name);
        system.ExternalId.Iter(external => collection.AddOrUpdateProperty(nameof(Node.Object.ExternalId), external));
        collection.AddOrUpdateProperty(nameof(DistributionSystem.Kind), system.Kind.Key);
        collection.AddOrUpdateProperty(nameof(DistributionSystemKind.Medium), system.Kind.Medium.Key);
        BrickLedger withMembers = system.Members.Fold(held.Mint(system.Id, collection.Id), (state, member) =>
            state.Minted.Find(member).Match(
                Some: entity => Related<PartOf>(brick, entity, collection.Id, state),
                None: () => (system.Circuits.Contains(member)
                        ? graph.Find<Node.Object>(member).Map(o => (BrickEntity)brick.AddEntity<BrickSystem>(o.Id.ToValue(), o.Name))
                        : graph.Find<Node.Object>(member).Bind(o => binding.Equipment(brick, o)))
                    .Match(
                        Some: entity => Related<PartOf>(brick, entity.Id, collection.Id, state.Mint(member, entity.Id)),
                        None: () => state.Unbind(member))));
        BrickLedger withPorts = system.Ports.Fold(withMembers, (state, port) =>
            binding.Point(brick, port).Match(
                Some: point => state.Minted.Find(port.Owner)
                    .Match(
                        Some: owner => Related<PointOf>(brick, point.Id, owner, state),
                        None: () => state)
                    .Mint(port.Id, point.Id),
                None: () => state));
        Map<NodeId, FlowDirection> flowByPort = system.Ports.Fold(Map<NodeId, FlowDirection>(), static (map, port) => map.AddOrUpdate(port.Id, port.Flow));
        Map<NodeId, NodeId> ownerByPort = system.Ports.Fold(Map<NodeId, NodeId>(), static (map, port) => map.AddOrUpdate(port.Id, port.Owner));
        system.Flow
            .Bind(edge => SystemTrace.Orient(edge.From, edge.To,
                flowByPort.Find(edge.From).IfNone(FlowDirection.NotDefined),
                flowByPort.Find(edge.To).IfNone(FlowDirection.NotDefined),
                TraceMode.Downstream))
            .Iter(leg =>
                (from ownerFrom in ownerByPort.Find(leg.From)
                 from ownerTo in ownerByPort.Find(leg.To)
                 from feeder in withPorts.Minted.Find(ownerFrom)
                 from fed in withPorts.Minted.Find(ownerTo)
                 select (Feeder: feeder, Fed: fed))
                    .Iter(pair => Related<Fedby>(brick, pair.Fed, pair.Feeder, withPorts)));
        return system.Served.Fold(withPorts, (state, served) => graph.Find<Node.Object>(served).Match(
            Some: o => Related<LocationOf>(brick, collection.Id, brick.AddEntity<BrickSpace>(o.Id.ToValue(), o.Name).Id, state),
            None: () => state));
    }

    private static BrickLedger Related<T>(BrickSchemaManager brick, string child, string parent, BrickLedger held)
        where T : BrickRelationship, new() {
        ignore(brick.GetEntity(child, byReference: true)?.AddRelationship<T>(parent));
        return held;
    }

    private static FlowDirection PortFlow(ElementGraph graph, NodeId port) =>
        (FlowOf(graph, port) | TypeOf(graph, port).Bind(type => FlowOf(graph, type)))
            .Match(Some: static token => FlowDirection.Of(token), None: static () => FlowDirection.NotDefined);

    private static Option<string> FlowOf(ElementGraph graph, NodeId node) =>
        toSeq(graph.EdgesAt(node))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition && a.Subject == node
                ? graph.Find<Node.PropertySet>(a.Definition) : Option<Node.PropertySet>.None)
            .Choose(static ps => ps.Bag.Find(PortRows.FlowDirection))
            .Head
            .Map(static value => value.Render());

    private static Option<NodeId> TypeOf(ElementGraph graph, NodeId port) =>
        toSeq(graph.EdgesAt(port))
            .Choose(e => e is Relationship.Assign a && a.SubKind == AssignKind.TypeDefinition && a.Subject == port
                ? Some(a.Definition) : Option<NodeId>.None)
            .Head;
}
```

## [03]-[SYSTEM_TRACE]

- Owner: `SystemTrace` the reachability fold over one `DistributionSystem` view's port-and-element flow graph — the set of every distribution element reachable from a seed port or element through the connection network, folded by the shared `QuikGraph` graph-algorithm owner the `Planning/schedule#CRITICAL_PATH` topological order and the `Review/versioning#VERSION_GRAPH` common-ancestor walk also compose, never a hand-rolled visited-set walk; `TraceMode` the orientation policy (`Reach` the undirected both-directions closure, `Downstream`/`Upstream` the `FlowDirection`-oriented directed closure). The flow network is a graph over BOTH ports AND elements so the closure crosses each fitting (a tee's inlet port → the tee element → the tee's outlet ports → the next segment), the bipartite-style traversal the port-only adjacency the retired walk built never crossed.
- Entry: `SystemTrace.From(DistributionSystem system, NodeId seed, TraceMode mode)` folds one explicit orientation over a transient `AdjacencyGraph<NodeId, SEdge<NodeId>>`, accumulates the closure through `BreadthFirstSearchAlgorithm` under two scoped observers, and partitions reached elements from reached ports as `TraceHop` rows carrying each node's ELEMENT-hop distance from the seed; `ReachedElements`/`ReachedPorts` are the DERIVED bare-reach projections, so a membership question still reads one hop while a ripple-banding consumer reads the distance column. `SystemTrace.Demand(ElementGraph graph, ValueSource source, Op key)` reduces reached effective values through `ElementQuery.SumOf`. `SystemTrace.Runs(ElementGraph graph, Option<ValueSource> resistance, Op key)` ranks every reached TERMINAL (a reached vertex with no outgoing oriented leg) by its best-route cost from the seed over the SAME retained oriented adjacency — the `ShortestPathsDijkstra` route fold, each element weighing its effective resistance value (segment length, fitting loss — a present non-measure faults the same `aggregate-non-measure` law `SumOf` returns, and a negative accumulated weight faults beside it because the route fold's optimality is a non-negativity precondition, never a silently wrong best route) with the hop-count identity when no source is named — descending, so the head IS the index run the duct-sizing, riser-diagram, and feeder-schedule reads start from; in a tree-shaped run the best route is the only route, so the ranking is exact, and a ring main ranks each terminal by its least-resistance route. An isolated system member yields itself; a seed outside the system yields an empty trace, so construction remains total without fabricating membership.
- Auto: `From` folds the system's `Ports` into the graph as ownership edges (each port ↔ its owner element, both directions — a port belongs to its element regardless of flow) and the system's `Flow` edges oriented by `TraceMode` over BOTH endpoint `FlowDirection`s (`Reach` adds both directions; a `Downstream` leg exists only where the emitting side holds `PortCapability.Emit` AND the receiving side holds `Receive`, so a `Source`→`Sink` edge carries one leg, a `NotDefined` pair conducts both ways, a `NotDefined`-against-`Source` edge still flows OUT of the source only, and two facing pure sources honestly sever the directed closure; `Upstream` is the mirror), the optional realizing fitting linked as an intermediate on each ORIENTED leg (emitting port → realizer → receiving port, so `Reach` keeps its bidirectional participation while a directed trace never crosses a connection backwards through its fitting); the reached closure accumulates through the `BreadthFirstSearchAlgorithm` under TWO SCOPED observers on the ONE walk — a `VertexTimeStamperObserver` whose discovery time orders the closure and a `VertexDistanceRecorderObserver` weighing each tree edge by whether its TARGET is an element, so the recorder accumulates the ELEMENT-hop distance rather than a vertex count the alternating port legs and the optional realizing fitting make unreadable (a directly connected neighbour measures one, a neighbour behind a fitting two, the fitting itself one) — O(reachable), the seed discovered first and seated at depth zero because the recorder records off TREE EDGES alone and no tree edge reaches a root, the same scoped-observer form the `Model/spatial#SPATIAL_STRUCTURE` `Reachable` holds, never an all-vertex `TryFunc` path-probe sweep and never a raw event `+=` over a mutable accumulator — and the fold partitions the reached vertices into the non-port reached elements and the reached ports by the port-set membership — the directed `Downstream` trace from an air-handling unit reaching every air terminal it feeds, the `Reach` trace from a shutoff valve reaching every fixture on its branch, both one fold over the QuikGraph adjacency; the trace reads the `DistributionSystem` view (one hop — the view already carries the ports with their `FlowDirection` and the deduped flow edges), never re-reading the element graph or re-resolving the port flow per query, and a consumer memoizes the trace against the owning system's `(MembershipKey, TopologyKey)` `Identity` so a re-trace re-folds only on a changed membership or adjacency.
- Output: the `SystemTrace` reached-element `Seq<TraceHop>` is the depth-carrying downstream-network evidence the `Review/coordination#COORDINATION` `ImpactReport` flow leg bands each rippled element by, its `ReachedElements` projection the membership read the `Model/zones#ZONE_GRAPH` MEP grouping resolves a system's effective member closure from and the `Model/query#ELEMENT_SET` consumers intersect against a domain set — a "every air terminal fed from this air-handling unit" / "every fixture downstream of this shutoff valve" query is one `From` fold over the flow graph, the connectivity the single-membership zone overlay cannot express; `Demand` turns the same closure into the quantified network evidence the discipline sizes from — the accumulated terminal airflow behind a duct main, the fixture units on a riser branch, the connected load behind a feeder, each one result-returning `SumOf` reduction partitioned by the owning `Kind.Medium` — declared-property aggregation the `Rasm.AppUi` schedules and a `Rasm.Compute` sizing check consume without re-deriving connectivity; the ranked `Seq<SystemRun>` is the index-run evidence the same consumers read — each row the terminal, its route `NodeId` chain, and its accumulated resistance, the head row the hydraulically-critical run — declared-property ranking, never a pressure solve (the solve stays `Rasm.Compute`'s); the reached set is consumed by the `zoning`/`query`/`analysis` peers by reference, never re-derived per consumer.
- Packages: QuikGraph, Rasm.Element, Rasm (the kernel `Op` the `Demand` reduction threads), LanguageExt.Core
- Growth: a new trace orientation is one `TraceMode` row carrying its `EdgeCapability` leg set the `Orient` fold reads beside the same `FlowDirection` conduction; a new reachability guard (stop at a controller, stop at a discipline boundary) is one filter on the edge fold; a new graph query (shortest flow path, connected components) rides the SAME `QuikGraph` `AlgorithmExtensions` facade over the same adjacency; never a per-direction trace record, never a second adjacency store, and never a per-discipline traversal.
- Boundary: the `SystemTrace` is ONE reachability fold over the shared `QuikGraph` `AdjacencyGraph` — the retired hand-rolled `SystemNetwork`/`Closure` visited-set tail-recursion is the deleted form, the `QuikGraph` graph-algorithm owner the whole stack folds a transient graph into rather than re-implementing a walk (the api-quikgraph `BreadthFirstSearchAlgorithm` event-fold law, `AddVerticesAndEdge` over the value `SEdge<NodeId>` that allocates nothing on the dense `NodeId`-keyed network), the SAME owner the `Planning/schedule#CRITICAL_PATH` topological order and the `Review/versioning#VERSION_GRAPH` common-ancestor walk compose so the stack carries one graph-algorithm owner rather than three bespoke walks, and a `Map<>`/`HashSet<>` adjacency with a mutated visited set or an all-vertex `TryFunc` path-probe sweep recovering a path per vertex is the named drift; a `TraceHvac`/`TraceElectrical`/`TracePlumbing` operation family is the deleted form per the no-operation-family law, the discipline already carried by the system's `Kind` the trace folds within; the trace carries no `Fin<T>` result because the closed graph is total (the dangling-endpoint rejection lowered at `Project`); the trace reads the `DistributionSystem` view ONE HOP and a re-read of the element graph or a re-resolution of the port `FlowDirection` per query is the named cross-page drift; the directed orientation reads the port `FlowDirection` the view carries and an `AdjacencyGraph` with no orientation policy is the no-modality reject; a consumer memoizes against the system `Identity` and a second identity scheme is the named drift defect. DEPTH GAIN/LOSS: every reached row publishes its element-hop distance so a consumer bands ripple by real depth instead of flattening a whole closure to one hop, and what the column does NOT promise is a minimum-element-count route — the distance counts the elements crossed on the BFS shortest-VERTEX-hop tree path, so a ring main whose vertex-longer leg touches fewer elements ranks by the tree route while weighted route optimality stays `Runs`' Dijkstra; a consumer re-tracing per level to recover depth, and a hop-1 flattening standing beside the column, are the deleted forms.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class EdgeCapability : ICapability<EdgeCapability> {
    public static readonly EdgeCapability Forward    = new("forward",    rank: 0);
    public static readonly EdgeCapability Reverse    = new("reverse",    rank: 1);
    public static readonly EdgeCapability Unoriented = new("unoriented", rank: 2);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TraceMode {
    public static readonly TraceMode Reach      = new("reach",      CapabilitySet<EdgeCapability>.Of(EdgeCapability.Unoriented));
    public static readonly TraceMode Downstream = new("downstream", CapabilitySet<EdgeCapability>.Of(EdgeCapability.Forward));
    public static readonly TraceMode Upstream   = new("upstream",   CapabilitySet<EdgeCapability>.Of(EdgeCapability.Reverse));

    private static readonly CapabilityLaw<EdgeCapability> Orientation = new(Seq(
        CapabilitySet<EdgeCapability>.Of(EdgeCapability.Unoriented),
        CapabilitySet<EdgeCapability>.Of(EdgeCapability.Forward),
        CapabilitySet<EdgeCapability>.Of(EdgeCapability.Reverse)));

    public CapabilitySet<EdgeCapability> Legs { get; }

    static partial void ValidateConstructorArguments(ref string key, ref CapabilitySet<EdgeCapability> legs) {
        if (Orientation.Admit(legs).IsFail) { throw new ArgumentException($"<trace-mode-corner:{key}>", nameof(legs)); }
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TraceHop(NodeId Node, int Hops);

public sealed record SystemRun(NodeId Terminal, Seq<NodeId> Route, double Cost);

public sealed record SystemTrace(NodeId Seed, TraceMode Mode, Seq<TraceHop> ElementHops, Seq<TraceHop> PortHops) {
    internal AdjacencyGraph<NodeId, SEdge<NodeId>> Network { get; init; } = new(allowParallelEdges: true);

    internal LanguageExt.HashSet<NodeId> Transit { get; init; } = LanguageExt.HashSet<NodeId>();

    public Seq<NodeId> ReachedElements => ElementHops.Map(static hop => hop.Node);

    public Seq<NodeId> ReachedPorts => PortHops.Map(static hop => hop.Node);

    public static SystemTrace From(DistributionSystem system, NodeId seed, TraceMode mode) {
        LanguageExt.HashSet<NodeId> ports = toHashSet(system.Ports.Map(static p => p.Id));
        Map<NodeId, FlowDirection> flowByPort = system.Ports.Fold(Map<NodeId, FlowDirection>(), static (map, port) => map.AddOrUpdate(port.Id, port.Flow));
        Map<NodeId, NodeId> ownerByPort = system.Ports.Fold(Map<NodeId, NodeId>(), static (map, port) => map.AddOrUpdate(port.Id, port.Owner));
        LanguageExt.HashSet<NodeId> transit = LanguageExt.HashSet<NodeId>();
        AdjacencyGraph<NodeId, SEdge<NodeId>> graph = new(allowParallelEdges: true);
        graph.AddVertexRange(system.Members);
        graph.AddVertexRange(ports);
        foreach (DistributionPort port in system.Ports) { Link(graph, port.Id, port.Owner); Link(graph, port.Owner, port.Id); }
        foreach (FlowEdge edge in system.Flow) {
            (FlowDirection fromFlow, FlowDirection toFlow) = (flowByPort.Find(edge.From).IfNone(FlowDirection.NotDefined), flowByPort.Find(edge.To).IfNone(FlowDirection.NotDefined));
            foreach ((NodeId from, NodeId to) in Orient(edge.From, edge.To, fromFlow, toFlow, mode)) {
                Link(graph, from, to);
                        ownerByPort.Find(from).Iter(owner => transit = transit.Add(owner));
                edge.Realizing.Iter(realizing => { Link(graph, from, realizing); Link(graph, realizing, to); });
            }
        }
        if (!graph.ContainsVertex(seed)) { return new SystemTrace(seed, mode, Seq<TraceHop>(), Seq<TraceHop>()) { Network = graph, Transit = transit }; }
        BreadthFirstSearchAlgorithm<NodeId, SEdge<NodeId>> search = new(graph);
        VertexTimeStamperObserver<NodeId> discovered = new();
        VertexDistanceRecorderObserver<NodeId, SEdge<NodeId>> depths = new(edge => ports.Contains(edge.Target) ? 0.0 : 1.0);
        using (discovered.Attach(search))
        using (depths.Attach(search)) { search.Compute(seed); }
        Map<NodeId, int> depth = toSeq(depths.Distances).Fold(
            Map<NodeId, int>().AddOrUpdate(seed, 0),
            static (map, row) => map.AddOrUpdate(row.Key, (int)row.Value));
        Seq<TraceHop> reached = toSeq(discovered.DiscoverTimes.OrderBy(static row => row.Value))
            .Map(row => new TraceHop(row.Key, depth.Find(row.Key).IfNone(0)));
        return new SystemTrace(seed, mode, reached.Filter(hop => !ports.Contains(hop.Node)), reached.Filter(hop => ports.Contains(hop.Node))) { Network = graph, Transit = transit };
    }

    public Fin<Seq<SystemRun>> Runs(ElementGraph graph, Option<ValueSource> resistance, Op key) =>
        ReachedElements
            .TraverseM(id => resistance
                .Bind(source => graph.Find<Node.Object>(id).Map(o => (Object: o, Source: source)))
                .Match(
                    None: () => Fin.Succ((Id: id, Weight: 1.0)),
                    Some: row => ElementQuery.ValuesOf(graph, row.Object, row.Source)
                        .TraverseM(value => value is PropertyValue.Measure measure
                            ? Fin.Succ(measure.Value.Si)
                            : Fin.Fail<double>(ElementFault.ValueRejected(key, $"<aggregate-non-measure:{value.GetType().Name}>")))
                        .As()
                        .Map(weights => weights.IsEmpty ? 1.0 : weights.Fold(0.0, static (total, si) => total + si))
                        .Bind(weight => weight < 0d
                            ? Fin.Fail<(NodeId Id, double Weight)>(ElementFault.ValueRejected(key, $"<route-weight-negative:{id.ToValue()}:{weight:R}>"))
                            : Fin.Succ((Id: id, Weight: weight)))))
            .As()
            .Map(rows => {
                Map<NodeId, double> weights = rows.Fold(Map<NodeId, double>(), static (map, row) => map.AddOrUpdate(row.Id, row.Weight));
                TryFunc<NodeId, IEnumerable<SEdge<NodeId>>> routes =
                    Network.ShortestPathsDijkstra(edge => weights.Find(edge.Target).IfNone(0.0), Seed);
                return toSeq(ReachedElements
                    .Filter(id => !Transit.Contains(id))
                    .Choose(terminal => routes(terminal, out var path) && toSeq(path) is var legs
                        ? Some(new SystemRun(terminal, legs.Map(static e => e.Target), legs.Sum(e => weights.Find(e.Target).IfNone(0.0))))
                        : Option<SystemRun>.None)
                    .OrderByDescending(static run => run.Cost));
            });

    public Fin<Option<MeasureValue>> Demand(ElementGraph graph, ValueSource source, Op key) =>
        ElementQuery.SumOf(graph, ReachedElements, source, key);

    internal static Seq<(NodeId From, NodeId To)> Orient(NodeId from, NodeId to, FlowDirection fromFlow, FlowDirection toFlow, TraceMode mode) =>
        mode.Legs.Admits(EdgeCapability.Unoriented)
            ? Seq((from, to), (to, from))
            : Leg(from, to, fromFlow, toFlow, mode) + Leg(to, from, toFlow, fromFlow, mode);

    private static Seq<(NodeId From, NodeId To)> Leg(
        NodeId emitting, NodeId receiving, FlowDirection emits, FlowDirection receives, TraceMode mode) =>
        emits.Conducts.Admits(PortCapability.Emit) && receives.Conducts.Admits(PortCapability.Receive)
            ? Seq(mode.Legs.Admits(EdgeCapability.Reverse) ? (receiving, emitting) : (emitting, receiving))
            : Seq<(NodeId, NodeId)>();

    private static void Link(AdjacencyGraph<NodeId, SEdge<NodeId>> graph, NodeId from, NodeId to) =>
        graph.AddVerticesAndEdge(new SEdge<NodeId>(from, to));
}
```

## [04]-[INTERFERENCE]

- Owner: `Interference` the host-neutral clash-evidence record carrying the clashing `(NodeId, NodeId)` pair, the `ClashKind` (`Hard` overlapping solids, `Clearance` insufficient maintenance/insulation gap), the measured deficit (the penetration depth for a hard clash, the clearance shortfall for a clearance clash, both kernel-SI scalars), the two member disciplines (`IfcDomain` pair), and the priority rank a cross-discipline clash carries above an intra-discipline one; `ClashKind` the closed `[SmartEnum<string>]` clash partition; `InterferenceQuery` the proximity request keyed by the two members' `RepresentationContentHash` body geometry content keys with the clearance threshold, the host-neutral systems owner producing the request and reading the scalar deficit back, the kernel `Rasm` geometry owner resolving the content-keyed geometry and evaluating the solid intersection; `GeometryProximity` the injected kernel PORT — a `readonly record struct` of two decode legs (`Bounds` the content-keyed `BoundVolume` AABB, `Test` the precise `Fin<ProximityResult>` signed gap) beside the composition's minimum clearance, the SAME app-wired resolver shape the shared `Graph/element#NODE_MODEL` `GeometrySource` holds, never an interface floor for a two-arrow contract; `ClashIndex` the retained `SwiftCollections.Lean` broad phase — the `SwiftBVH` hard-overlap tree and the `SwiftSpatialHash` clearance ring over ONE `SwiftBucket` handle space; `InterferenceCheck` the fold pairing the distribution-run geometry against itself (cross-system) and against the static obstruction set — every other body-carrying occurrence, the Architecture-domain built elements included.
- Cases: `ClashKind` rows `Hard` (overlapping solids, deficit = penetration depth) · `Clearance` (gap below the maintenance/insulation clearance, deficit = threshold − gap); an `Interference` carries the ordered member `NodeId` pair, the `ClashKind`, the SI deficit, and the discipline `IfcDomain` pair, a cross-discipline clash (`FirstDomain != SecondDomain`) ranking above an intra-discipline one through the `DisciplineWeight` ordering offset.
- Entry: `InterferenceCheck.Build(ElementGraph graph, GeometryProximity proximity, Op key)` resolves every occurrence bound through the injected `Fin` result and seats the hard-overlap tree, the clearance ring, and the handle registry; `Candidates(ClashIndex)` reads the admitted run-to-run and run-to-static pairs off the tree; `Neighborhood(ClashIndex index, BoundVolume volume)` answers the clearance-ring modality the `Review/coordination#COORDINATION` clearance read consumes; `Interferences(ElementGraph graph, GeometryProximity proximity, Op key)` composes the build and the precise signed-gap test; `Refit(ClashIndex index, Seq<(int Handle, BoundVolume Bounds)> moved, Op key)` re-seats the entries a `ModelDiff` moved. Missing geometry aborts on the owning `BimFault`; hard and clearance evidence ranks by cross-discipline priority and deficit.
- Auto: `Build` resolves each occurrence's clearance value ONCE (one `Bake` with one bag walk, never per pair), traverses `GeometryProximity.Bounds`, and seats the member's TIGHT volume under the handle `SwiftBucket.Add` returns — the registry OWNS the key space both structures index on, so no `NodeId` map sits beside them to desynchronize on a partial refit. The two structures answer the two modalities the `ClashKind` partition already names: `SwiftBVH.Query` the hard overlap on the geometry itself, `SwiftSpatialHash.QueryNeighborhood` the clearance ring on the padded cell neighborhood, so the clearance floor no longer widens every hard-clash candidate set. The ring's cell size DERIVES from the widest declared clearance, which is what makes the default one-ring padding cover every clearance question. Both `Query` surfaces sink into a `SwiftHashSet` — the package's own `ICollection` result contract — and the `other > self` handle gate IS the unordered-pair dedupe, so no `seen` set and no synthesized pair key exists. Only pairs this page's run/static policy admits reach `GeometryProximity.Test`. `Refit` gates every handle through `SwiftBucket.TryGetValue` first, and past that gate the hash's BOOLEAN refit return is the pair's verdict — the BVH leg returns void and reports nothing, so a hash refusal is the one observable a divergence produces.
- Output: the ranked `Seq<Interference>` is the MEP coordination evidence the `Review/coordination#COORDINATION` `ClashProposal` fold consumes (the clash `NodeId` pair, kind, and deficit anchoring a proposed resolution and a BCF topic, the coordination owner resolving each member's `ExternalId` IFC `GlobalId` off the graph for the viewpoint) and the `Rasm.AppUi/Charts` clash report renders — a duct-vs-beam hard clash, a pipe-clearance violation, and a tray-vs-structure graze each carry their measured deficit and discipline pair on one host-neutral row.
- Packages: Rasm.Element, Rasm, SwiftCollections.Lean, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new clash kind is one `ClashKind` row; a new run discipline is one `RunDomains` row; a new clearance source is one `PropertyCategory.Neutral.Row` in `ClearanceKeys`; a new broad-phase structure binds the SAME `IBoundVolume<TVolume>` contract `SwiftOctree` and `SwiftSpatialHash` already implement, so it is a `ClashIndex` type argument and never a second fold here.
- Boundary: `GeometryProximity` resolves bounds and signed distance by content key, so RhinoCommon, GeometryGym, and tessellation remain outside this owner — the port's `BoundVolume` is host-free over `System.Numerics`, which is what keeps a host AABB type off a host-neutral signature. `BoundVolume` is the ONE bound type end to end: the port returns it, both structures index it, and the refit re-seats it, so a second AABB record, a bare corner tuple, and a host bounding-box crossing the port are each the deleted form that put three incompatible types on one data path. `SwiftCollections.Lean` owns the broad phase — the `SwiftBVH` tight-volume tree answering HARD overlap and the `SwiftSpatialHash` padded ring answering CLEARANCE through `QueryNeighborhood`, the two modalities `ClashKind` already partitions, so one pre-expanded bounding envelope answering both (which polluted every hard-clash candidate set with members that only came near) is the deleted form — and this page owns the run/static pair-admission policy and the precise test alone; both structures are RETAINED on `ClashIndex` over the ONE `SwiftBucket` handle space so a `ModelDiff` moved arm refits its own entries, and a throwaway index rebuilt per edit, a parallel `NodeId` registry beside the registry's own handles, a hand-rolled `seen`-set dedupe, or an O(N²) scan is the deleted form. RUN/STATIC admission derives from `IfcDomain`, clearance derives from baked properties read through owner-minted shared rows, `Interference` retains shared `NodeId` identity, and coordination consumes the ranked evidence without repeating proximity.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using SwiftCollections;
using SwiftCollections.Query;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard      = new("HARD");
    public static readonly ClashKind Clearance = new("CLEARANCE");
}

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct InterferenceQuery(UInt128 First, UInt128 Second, double ClearanceThreshold);

public readonly record struct ProximityResult(double Gap, double ClosestApproach);

public readonly record struct GeometryProximity(
    Func<UInt128, Fin<BoundVolume>> Bounds,
    Func<InterferenceQuery, Fin<ProximityResult>> Test,
    double MinimumClearance);

[SmartEnum<string>]
public sealed partial class ClashRole : ICapability<ClashRole> {
    public static readonly ClashRole Run      = new("run",      rank: 0);
    public static readonly ClashRole Obstruct = new("obstruct", rank: 1);

    public int Rank { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ClashCandidate(
    NodeId Id, IfcDomain Domain, UInt128 Body, CapabilitySet<ClashRole> Roles, double Clearance);

public sealed record ClashIndex(
    SwiftBVH<int> Tree,
    SwiftSpatialHash<int> Ring,
    SwiftBucket<(ClashCandidate Member, BoundVolume Bounds)> Registry);

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
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class InterferenceCheck {
    private static readonly FrozenSet<IfcDomain> RunDomains =
        new[] { IfcDomain.HvacFire, IfcDomain.Electrical, IfcDomain.Plumbing }.ToFrozenSet();

    private static readonly Seq<PropertyName> ClearanceKeys = Seq(
        PropertyCategory.Neutral.Row("InsulationThickness"),
        PropertyCategory.Neutral.Row("Clearance"),
        PropertyCategory.Neutral.Row("MaintenanceClearance"));

    public static Fin<Seq<Interference>> Interferences(ElementGraph graph, GeometryProximity proximity, Op key) =>
        Build(graph, proximity, key)
            .Bind(index => Candidates(index).TraverseM(pair => Clash(pair.A, pair.B, proximity)).As())
            .Map(static rows => toSeq(rows.Somes().OrderByDescending(static clash => clash.Rank)));

    public static Seq<ClashCandidate> Neighborhood(ClashIndex index, BoundVolume volume) {
        SwiftHashSet<int> hits = new();
        index.Ring.QueryNeighborhood(volume, hits);
        return toSeq(hits).Choose(handle => index.Registry.TryGetValue(handle, out var entry) ? Some(entry.Member) : None);
    }

    public static Fin<ClashIndex> Build(ElementGraph graph, GeometryProximity proximity, Op key) =>
        graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Occurrence)
            .TraverseM(o => IfcClass.TryGet(o.Classification.Code)
                .Bind(c => o.Representations.Body.Map(body => (Class: c, Body: body)))
                .TraverseM(row => ClearanceOf(graph, o.Id, key).Map(clearance =>
                    new ClashCandidate(o.Id, row.Class.Domain, row.Body, RolesOf(row.Class.Domain), clearance)))
                .As())
            .As()
            .Bind(members => members.Somes()
                .TraverseM(member => proximity.Bounds(member.Body).Map(bounds => (Member: member, Bounds: bounds)))
                .As())
            .Map(entries => Seat(toSeq(entries), proximity.MinimumClearance));

    public static Seq<(ClashCandidate A, ClashCandidate B)> Candidates(ClashIndex index) =>
        toSeq(Enumerable.Range(0, index.Registry.PeakCount))
            .Filter(index.Registry.IsAllocated)
            .Bind(self => Overlaps(index.Tree, index.Registry[self].Bounds)
                .Filter(other => other > self && index.Registry.IsAllocated(other))
                .Map(other => (A: index.Registry[self].Member, B: index.Registry[other].Member)))
            .Filter(static pair => pair.A.Roles.Admits(ClashRole.Run) || pair.B.Roles.Admits(ClashRole.Run));

    private static CapabilitySet<ClashRole> RolesOf(IfcDomain domain) =>
        RunDomains.Contains(domain)
            ? CapabilitySet<ClashRole>.Of(ClashRole.Run, ClashRole.Obstruct)
            : CapabilitySet<ClashRole>.Of(ClashRole.Obstruct);

    private static ClashIndex Seat(Seq<(ClashCandidate Member, BoundVolume Bounds)> entries, double floor) {
        SwiftBVH<int> tree = new(entries.Count);
        float cell = (float)entries.Fold(floor, static (widest, entry) => Math.Max(widest, entry.Member.Clearance));
        SwiftSpatialHash<int> ring = new(entries.Count, cell, SwiftSpatialHashOptions.Default);
        SwiftBucket<(ClashCandidate Member, BoundVolume Bounds)> registry = new(entries.Count);
        entries.Iter(entry => {
            int handle = registry.Add(entry);
            ignore(tree.Insert(handle, entry.Bounds));
            ignore(ring.Insert(handle, entry.Bounds));
        });
        return new ClashIndex(tree, ring, registry);
    }

    private static Seq<int> Overlaps(SwiftBVH<int> tree, BoundVolume bounds) {
        SwiftHashSet<int> hits = new();
        tree.Query(bounds, hits);
        return toSeq(hits);
    }

    public static Fin<ClashIndex> Refit(ClashIndex index, Seq<(int Handle, BoundVolume Bounds)> moved, Op key) =>
        moved
            .TraverseM(row => index.Registry.TryGetValue(row.Handle, out var entry)
                ? Fin.Succ((row.Handle, row.Bounds, entry.Member))
                : Fin.Fail<(int Handle, BoundVolume Bounds, ClashCandidate Member)>(
                    new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "clash-refit", "unindexed", row.Handle.ToString(CultureInfo.InvariantCulture) }))))
            .Bind(admitted => admitted.TraverseM(row => {
                index.Tree.UpdateEntryBounds(row.Handle, row.Bounds);
                index.Registry[row.Handle] = (row.Member, row.Bounds);
                return index.Ring.UpdateEntryBounds(row.Handle, row.Bounds)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Model, BimReason.Rejected, string.Join(':', new object?[] { "clash-refit", "rejected", row.Handle.ToString(CultureInfo.InvariantCulture) })));
            }).As())
            .Map(_ => index);

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

    private static Fin<double> ClearanceOf(ElementGraph graph, NodeId member, Op key) =>
        graph.Bake(member, key).Map(element => ClearanceKeys
                .Choose(name => element.Properties.Choose(bag => bag.Find(name)).Head)
                .Choose(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : Option<double>.None)
                .Fold(0d, static (max, si) => Math.Max(max, si)));
}
```

## [05]-[RESEARCH]

(none)

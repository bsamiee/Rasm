# [BIM_CONNECTION]

`ConnectionProjection` is the realizing-element connection-detail reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: it lowers the WHOLE GeometryGym realizing-element surface — the bolted/welded `IfcMechanicalFastener` and the bonded/welded `IfcFastener`, the fabricated `IfcDiscreteAccessory` framing connector, the cast-in `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`, and the support `IfcBearing` — onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the ONE neutral realizing-detail schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` AUTHORS — this reader READS the same schema, never a hand-synced parallel bag), bound to the realizing element's `Graph/element#NODE_MODEL` `Object` node through one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, so an authored realizing element and a re-imported IFC one content-key to one `Node.PropertySet` and a `csharp:Rasm.Fabrication` detailer reads the bolt diameter, the stud-shear connector, the framing-connector accessory token, the reinforcing cover, the post-tensioning tendon, and the bearing type off the ONE `Graph/element#ELEMENT_GRAPH` `Bake`-derived element it already holds — never a second store. The physical joint TOPOLOGY — which two members meet through which realizing element — is the `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing node on the `Connect.Realizing` `Option<NodeId>` field (realizing-ness the field, never a `ConnectKind` row — the medium closes at `Path`/`Port`) the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` already authors from `IfcRelConnectsWithRealizingElements` (reading the realizing head into the seam `Connect.Realizing` option); this page owns ONLY the realizing element's fabrication DETAIL the general `Object` fold does not read — the native reinforcing scalars and the fastener nominal diameter the internal GeometryGym scalar hides.

This RETIRES the migration source's parallel `ConnectionDetail` record + `ConnectionRealization` `[Union]` + `BoltPattern`/`WeldSchedule`/`BearingSurface` `[ComplexValueObject]` + `ConnectionKind`/`Clearance` family keyed by `BimModel`/`GlobalId` AND its hand-rolled `(GeometryKey, DetailKey)` second content-key — the very "second stored record off the element" the rebuild forbids, mirroring `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection` retiring the `MemberConnection`/`SupportRestraint` typed store — AND the `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary, the deleted form mirroring `Semantics/composition#MATERIAL_COMPOSITION` retiring the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers. The joint modality is the realizing `Object.PredefinedType` token plus the bag's `JointType` enumerated, the bolt/weld/bearing/cast detail is typed `Properties/property#PROPERTY_VALUE` `PropertyValue` entries, and the IFC egress is the `Projection/egress#IFC_EGRESS` `Emit` generic round-trip — the realizing `IfcMechanicalFastener`/`IfcReinforcingBar` re-authored as an `Object` node, the detail bag as an `IfcPropertySet` through `ReauthorProperties`, the joint as `IfcRelConnectsWithRealizingElements` through `ReauthorRelationships` — never a connection-specific writer. The reader is HOST-NEUTRAL (it binds the realizing geometry by the `Graph/element#NODE_MODEL` `RepresentationContentHash` content key, never re-tessellating the fastener) and TOTAL (a realizing element with no readable detail yields no bag, an unrooted realizing endpoint is the `EdgeProjection` `Connect`-edge `edge-endpoint-miss`, never this reader's fault), a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection`, `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection`, and `Semantics/georeference#GEO_PROJECTION` `GeoReferenceProjector` — so a connection enrichment never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [01]-[CONNECTION_DETAIL]: `ConnectionProjection` the GeometryGym realizing-element-detail reader — `Detail` the ONE polymorphic attribute-bag reader discriminating the EIGHT-family realizing-element surface (`IfcMechanicalFastener` bolt/weld, `IfcFastener` glue/mortar/weld, `IfcDiscreteAccessory` framing connector, `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor` cast, `IfcBearing` support) onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the IDENTICAL neutral schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` authors, never a hand-synced parallel bag), the `Joint`/`Token`/`Measured`/`Rows` row-fold each arm composes (the schema `JointType` modality row, a token, an SI `Measured` over its `Dimension`), `DiameterOf` the fastener/tendon nominal-diameter recovery through the associated `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section (the public channel for the GeometryGym-internal `mNominalDiameter` scalar), `Bag` the realizing-vs-not gate composing the shared `Mint` content-keyed seam `PropertySet` node, and `All` the fold over the model's `Extract<IfcElement>` stream (the `Detail` switch the sole realizing discriminator) producing the `(bag node, Assign.PropertyDefinition edge)` pairs the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` composes onto the `ElementGraph`.

## [02]-[CONNECTION_DETAIL]

- Owner: `ConnectionProjection` the static realizing-element-detail reader `SemanticProjector` composes, lowering the WHOLE GeometryGym realizing-element surface onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node — never a stored record and never a hand-synced bag, the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors so an authored realizing element and a re-imported one are one content-keyed `Node.PropertySet`. It owns the polymorphic `Detail` attribute-bag reader (one entry discriminating the EIGHT-family realizing surface — mechanical/non-mechanical fastener, fabricated discrete accessory, reinforcing bar/mesh/tendon/anchor, support bearing — onto its bolt/weld/bonded/accessory/cast/bearing bag through the `Joint`/`Token`/`Measured`/`Rows` row-fold over `DetailSchema.Realization.Bag()`), the `DiameterOf` cross-section diameter recovery, the `FastenerOf` co-realizing attaching-fastener token recovery, the `Bag` realizing-gate composing the shared `Mint` of the seam `Node.PropertySet`, and the `All` model fold over `Extract<IfcElement>`; the typed structures the migration source minted (`ConnectionDetail`, the `ConnectionRealization` `[Union]`, `BoltPattern`/`WeldSchedule`/`BearingSurface`, `ConnectionKind`, `Clearance`) are all GONE — the realizing element is the seam `Object` node the general `Objects` fold mints, the joint modality its `PredefinedType` token plus the bag `JointType` row, the joint topology the neutral `Connect` edge carrying the realizing node on its `Connect.Realizing` field, and the fabrication scalars the typed `PropertyValue` bag entries.
- Entry: `ConnectionProjection.Detail(IfcElement realizing)` is the ONE polymorphic, TOTAL attribute-bag reader discriminating on the realizing-element shape — an `IfcMechanicalFastener` onto the bolt/weld bag (the `JointType`/`FastenerType` tokens plus the `NominalDiameter` recovered from the cross-section profile), an `IfcFastener` onto the bonded/weld bag (the `JointType`/`FastenerType` tokens), an `IfcDiscreteAccessory` onto the framing-connector bag (the `JointType`/`AccessoryType` tokens plus the `FastenerType` of the co-realizing attaching `IfcMechanicalFastener` — the two-token shape the `Rasm.Materials` `Connection/hanger#HANGER_FAMILY` hanger projection authors), an `IfcReinforcingBar` onto the cast bag (the `BarType`/`BarSurface` tokens plus the native `NominalDiameter`/`CrossSectionArea`/`BarLength` scalars), an `IfcReinforcingMesh` onto the mesh bag (the `MeshType` token plus the native mesh length/width, the longitudinal/transverse bar diameters, spacings, and cross-section areas), an `IfcTendon` onto the cast bag (the `TendonType` token plus the profile-recovered `NominalDiameter`), an `IfcTendonAnchor`/`IfcBearing` onto its type-token bag (`AnchorType`/`BearingType` plus the normalized `JointType`), and any other (or null) onto the empty bag — no `Fin` rail, because a connection enrichment never fails the whole import over one unreadable detail (the entity class is the general fold's `BimFault.UnmappedClass`, the dangling realizing endpoint its `Connect`-edge `BimFault.DanglingReference`), and no `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family — one polymorphic `Detail` discriminating by input value, the rich arms (`FastenerDetail`/`AccessoryDetail`/`BarDetail`/`MeshDetail`/`TendonDetail`) reading the hidden scalars onto the schema bag, the type-only arms inline; `ConnectionProjection.Bag(IfcElement realizing, double tolerance)` wraps a non-empty `Detail` `PropertyBag` (the schema's `Values` non-empty) in the content-keyed seam `Node.PropertySet` (`Option<T>.None` for an empty detail so a detail-free realizing element produces no bag); `ConnectionProjection.All(IfcProject project, Map<string, NodeId> rooted, double tolerance)` folds every `IfcElement` the project carries — the `Detail` switch the SOLE discriminator of the eight realizing families (`IfcMechanicalFastener`/`IfcFastener`/`IfcDiscreteAccessory`/`IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcBearing`), a non-realizing element folding to the empty `Detail` and so to no bag — into the `Seq<(Node Bag, Relationship Edge)>` the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` concats onto its node and edge sets (exactly as it concats `Materials` and `EdgeProjection.All`), so a new realizing family is one `Detail` arm and never a parallel extract list to drift.
- Auto: each arm folds its rows onto the seam `DetailSchema.Realization.Bag()` through the `Joint`/`Token`/`Measured`/`Rows` row-fold — one `Joint` schema modality row, the type tokens, and the SI `Measured` rows — so the arm is a flat declarative row list, never a repeated `MeasureValue.OfSi` construction and never a hand-spelled set-name or precedence (both ride the schema). `Detail` reads the realizing element's NATIVE fabrication scalars the general `Object` fold leaves on the geometry — the reinforcing bar/mesh expose their `NominalDiameter`/`CrossSectionArea`/`BarLength`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea` as public doubles wrapped directly into a `MeasureValue.OfSi(Dimension.LengthDim/AreaDim, ...)` (the IFC scalars are SI-base, never re-coerced — the SAME dimension-only overload the author uses, so an authored and an imported row content-key identically), and the `JointType` enumerated derives from the realizing family (an `IfcMechanicalFastener` `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` or an `IfcFastener` `WELD` is `Welded`, every other discrete mechanical fastener `Bolted`, an `IfcFastener` `GLUE`/`MORTAR` `Bonded`, an `IfcBearing` `Bearing`, a reinforcing bar/mesh/tendon/anchor `Cast`) through `DetailSchema.Realization.Joint(kind)`, the schema's closed `JointTypes` allowed set the egress facet validates against; the mechanical-fastener AND tendon `NominalDiameter` is the special case — `mNominalDiameter`/`mNominalLength` are GeometryGym-internal on both with NO public getter, so `DiameterOf` recovers the diameter through the inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` → `ForProfileSet.MaterialProfiles` → `Profile` → `IfcCircleProfileDef.Radius` × 2), the documented public round-trip channel, yielding `Option<double>.None` (read `IfNone(NaN)` and dropped at the `Filter`, never a fabricated 0) when no circle profile binds; every non-finite scalar (an unset GeometryGym `NaN` default) is dropped at the `Detail` egress `Filter` so a partially-specified realizing element never emits a misleading measure; `Bag` mints the seam `Node.PropertySet` whose id is `NodeId.Content` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical realizing details dedup to one node, and `All` resolves each realizing element's rooted `NodeId` through the `rooted` map and binds the bag through an `Assign.PropertyDefinition` edge, skipping an unrooted realizing element (the `Connect`-edge `edge-endpoint-miss` is `EdgeProjection`'s, not this fold's).
- Receipt: the connection-detail bag lands on the ONE seam `ElementGraph` as a `PropertySet` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold merges into `element.Properties` through the realizing element's `Assign.PropertyDefinition` edge, so the `csharp:Rasm.Fabrication` detailer reads `element.Properties.Find(b => b.SetName == DetailSchema.Realization.SetName).Bind(b => b.Find(DetailSchema.NominalDiameter))` for the bolt diameter / weld stud / reinforcing cover off the baked realizing element (the NEUTRAL `SetName`, never an IFC literal — the `Rasm_ConnectionRealization` Pset name is applied only at the `Projection/egress#IFC_EGRESS` mapping), and the joint topology off the `Connect` edge whose `Connect.Realizing` field carries the realizing node the `EdgeProjection` authors — a steel bolted moment connection's fasteners, a stud-shear-connector deck weld, and a cast-in reinforcing lap each carrying their physical detail on the one graph the consumer already holds, never a parallel connection store and never a second member-selection surface; the `Projection/egress#IFC_EGRESS` `Emit` re-authors the bag (`IfcPropertySet` through `ReauthorProperties`) and the joint (`IfcRelConnectsWithRealizingElements` through `ReauthorRelationships`) generically, so the connection round-trips with the rest of the graph.
- Packages: GeometryGymIFC_Core (the realizing-element surface consumed as settled vocabulary), Rasm.Element (the seam `DetailSchema` (the neutral realizing-detail schema this reader composes) + the `Node`/`NodeId`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`Relationship`/`AssignKind` payloads, the schema carrying the `SetName`/`OccurrenceWins`/`JointTypes` so this reader hand-spells none), LanguageExt.Core (`Option`/`Seq`/`Map`).
- Growth: a new realizing-element family is one arm on the `Detail` switch (a `Rows` of its `Joint`/`Token`/`Measured` rows inline, or a dedicated reader where a hidden scalar needs `DiameterOf`); a new fabrication scalar is one `Measured` row on its arm carrying its `MeasureValue` over the composed `Dimension` (a row in the canonical `DetailSchema` vocabulary composes the schema static, an ingest-only scalar a reader-side `[READER_ROWS]` `PropertyName`); a new joint modality is one token on the seam `DetailSchema.Realization.JointTypes` allowed set plus its `JointType` derivation, never a reader-local allowed set; never a per-joint-type connection record, never a `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family, never a second connection store, never a hand-synced parallel detail bag, never a `(GeometryKey, DetailKey)` parallel content key, and never a re-tessellation of the realizing element.
- Boundary: the connection detail is the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node, COMPOSED through `DetailSchema.Realization.Bag()`/`.Joint(kind)` (the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors) — a hand-synced parallel bag re-spelling the set name, the `OccurrenceWins` precedence, or the `JointTypes` allowed set is the deleted form (the reader READS the seam-declared schema, never a copy), and a typed `ConnectionDetail`/`ConnectionRealization`/`BoltPattern`/`WeldSchedule`/`BearingSurface`/`ConnectionKind`/`Clearance` second-store record family is the deleted form (mirroring `StructuralProjection` retiring `MemberConnection`/`SupportRestraint`) — the realizing element is the seam `Object` node, its detail the schema bag the `Bake` fold reads flat; the in-graph bag carries the NEUTRAL `SetName` and the IFC `Rasm_ConnectionRealization` Pset name is applied ONLY at the `Projection/egress#IFC_EGRESS` `Emit` mapping, so a `Rasm_ConnectionRealization` literal as the in-graph set name is the deleted form; the canonical realizing-detail rows compose the `DetailSchema` `PropertyName` statics while an INGEST-ONLY scalar the author never mints (a mesh sheet's geometry, a tendon/anchor/bearing token, a bar's surface + overall length) is a reader-side `[READER_ROWS]` open `PropertyName` — an authored bag and a richer imported bag are faithfully DIFFERENT content-keyed nodes, never a forced byte-match, and a reader-side row never widens the seam `DetailSchema`; the `BimModel`/`BimElement` join (`federated.Elements`, the `(MemberGlobalId, MemberGlobalId)` pair, the `BindFederated` dangling-reference rail) is GONE with the retired element records, the joint endpoints being the `Connect` edge's `NodeId` pair the `EdgeProjection` resolves and the analytical member↔connection topology the `Model/structural#STRUCTURAL_PROJECTION` `IfcRelConnectsStructuralMember` `Generic` edge, both meeting on the SHARED graph nodes, never a `GlobalId`-pair selection surface; the detail-bag attachment is ONE polymorphic `Detail` discriminating by input value and a `RealizationOf`/`BoltOf`/`WeldOf`/`LapOf`/`TendonOf`/`BearingOf` sibling-method family is the deleted form; the reader is TOTAL and routing a connection detail onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`); the connection detail stays host-neutral scalar data and a RhinoCommon `Brep`/`Mesh` realizing-element field or an in-process fastener tessellation is the named seam violation, the realizing geometry binding by the `RepresentationContentHash` content key; the GeometryGym realizing surface (`IfcMechanicalFastener.PredefinedType` `IfcMechanicalFastenerTypeEnum` and `IfcFastener.PredefinedType` `IfcFastenerTypeEnum`, the `IfcDiscreteAccessory.PredefinedType` `IfcDiscreteAccessoryTypeEnum` plus the `IfcElement.IsConnectionRealization` `SET<IfcRelConnectsWithRealizingElements>` back-pointer to the co-realizing attaching `IfcMechanicalFastener`, the public `IfcReinforcingBar.NominalDiameter` (`IfcReinforcingBarType.NominalDiameter` type-fallback)/`CrossSectionArea`/`BarLength`/`PredefinedType`/`BarSurface`, the public `IfcReinforcingMesh.PredefinedType`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea`, the `IfcTendon.PredefinedType` `IfcTendonTypeEnum` / `IfcTendonAnchor.PredefinedType` `IfcTendonAnchorTypeEnum` / `IfcBearing.PredefinedType` `IfcBearingTypeEnum`, the `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialProfileSetUsage` → `IfcCircleProfileDef.Radius` chain) is consumed as settled vocabulary (`.api/api-geometrygym-ifc`) and a hand-rolled realizing reader is the deleted form; the mechanical-fastener and tendon nominal diameter rides the associated circle-profile radius (the public channel for the internal `mNominalDiameter`) and a fabricated `0` diameter on an unprofiled element is the deleted form (the entry reads `NaN` and is dropped at the egress `Filter`); the realizing element's CLASSIFICATION and MATERIAL ride the general `Object`/`Associate` folds, not this bag — a steel grade or embodied-carbon column on the connection bag is the named seam violation (those grow on the seam `MaterialPropertySet` the `Semantics/composition` egress authors); the egress is the `Projection/egress#IFC_EGRESS` `Emit` generic `ReauthorProperties`/`ReauthorRelationships` and a `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary is the deleted form (those Materials wires are retired, a connection element authored from the Materials/Fabrication side projecting onto the seam graph as an `Object` node + `Connect` edge the `Emit` re-authors).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
// The realizing-element connection-detail reader Projection/semantic#SEMANTIC_PROJECTOR composes: it lowers the WHOLE
// GeometryGym realizing-element family (mechanical/non-mechanical fastener, reinforcing bar/mesh/tendon/anchor, bearing)
// onto the seam DetailSchema.Realization conforming PropertyBag (Properties/property#DETAIL_SCHEMA — the IDENTICAL neutral
// schema the Rasm.Materials ComponentProjector AUTHORS, so this reader READS it and never hand-synces a parallel
// ConnectionDetail store or a re-spelled bag), bound to the realizing Object node. The reader is TOTAL — an unreadable
// detail yields an empty/None bag, never a fault; the element identity, entity class, and dangling-endpoint rails are the
// general projector's Fin<GraphDelta> concern, so this reader never re-cases BimFault. The joint TOPOLOGY is the
// Connect edge whose Connect.Realizing Option<NodeId> field carries the realizing node EdgeProjection authors (the medium
// closes at ConnectKind.Path/Port); this page owns only the detail. The IFC Rasm_ConnectionRealization
// Pset name + the IFC predefined enums stay Bim-only — Emit maps the neutral DetailSchema.Realization.SetName onto the IFC
// Pset name at egress, the seam bag carrying the neutral name.
public static class ConnectionProjection {
    // The reader composes the seam DetailSchema.Realization (Properties/property#DETAIL_SCHEMA) — the ONE neutral
    // realizing-detail schema the Rasm.Materials Component projection AUTHORS — so an imported realizing element and an
    // authored one are one content-keyed Node.PropertySet on the one graph, never a hand-synced parallel bag. The NEUTRAL
    // SetName (DetailSchema.Realization.SetName), the OccurrenceWins precedence, the closed JointType allowed-set
    // (DetailSchema.Realization.JointTypes), and the canonical realizing-detail PropertyName vocabulary all ride the
    // schema; this reader hand-spells NONE of them. The IFC Pset NAME (the buildingSMART-reserved "Rasm_ConnectionRealization"
    // custom set) is Bim-only — Emit maps DetailSchema.Realization.SetName onto it at egress, so the literal lives ONLY at
    // the Projection/egress#IFC_EGRESS mapping, never as the in-graph set name a seam node carries.

    // The fold the SemanticProjector composes: every IfcElement the project carries -> (its content-keyed connection-detail
    // bag node, the Assign.PropertyDefinition edge binding the bag to the realizing Object node). The Detail switch is the
    // SOLE realizing-family discriminator — a non-realizing element folds to the empty Detail (the boundary _ arm), Bag
    // returns None, and Choose drops it — so a new realizing family is ONE Detail arm, never a parallel per-type Extract list
    // to drift out of sync. The one Extract<IfcElement> walk reads every element DIRECTLY (not via the realizing relation) so
    // an element carries its fabrication detail whether or not it sits in an IfcRelConnectsWithRealizingElements (the joint
    // topology riding the separate Connect edge), discovers each element once (the realizing families are IfcElement leaves,
    // so no sibling double-count), and the general Objects fold has already minted each as the Object node this bag binds
    // against. An unrooted element is skipped (its Connect-edge endpoint is EdgeProjection's edge-endpoint-miss) — TOTAL,
    // the projector concats the result onto its nodes/edges exactly as it concats Materials/EdgeProjection.All.
    public static Seq<(Node Bag, Relationship Edge)> All(IfcProject project, Map<string, NodeId> rooted, double tolerance) =>
        project.Extract<IfcElement>().AsIterable().Choose(realizing => rooted.Find(realizing.GlobalId).Bind(node =>
            Bag(realizing, tolerance).Map(bag =>
                ((Node)bag, (Relationship)new Relationship.Assign(node, bag.Id, AssignKind.PropertyDefinition))))).ToSeq();

    // The content-keyed seam PropertySet node, or None for an EMPTY Detail — the realizing-vs-not gate, since a
    // non-realizing element hits the Detail boundary _ arm (the empty schema bag) while every realizing family yields at
    // least its Joint row, so only a real connection detail lands a bag and a non-realizing element is dropped at the All
    // Choose. Detail already returns the conforming DetailSchema.Realization bag (the neutral SetName + OccurrenceWins
    // pinned by the schema), so emptiness reads its Values.
    static Option<Node.PropertySet> Bag(IfcElement realizing, double tolerance) =>
        Detail(realizing) is { Values.IsEmpty: false } detail
            ? Some(Mint(detail, tolerance))
            : Option<Node.PropertySet>.None;

    // The content-keyed seam PropertySet mint Semantics/composition and Semantics/appearance share: construct the node with
    // a discarded placeholder id, then re-key from the seam Node.ToCanonicalBytes (id excluded) so two structurally-identical
    // connection details dedup to one node — never a second (GeometryKey, DetailKey) hasher. A class-root [Union] Node case
    // has NO compiler-generated `with`, so the content id re-stamps through the seam Graph/element#NODE_MODEL Node.Relabel
    // (a `draft with { Id }` a class case cannot honour is the deleted form, the SAME re-stamp the Rasm.Materials Mint takes).
    static Node.PropertySet Mint(PropertyBag bag, double tolerance) {
        var draft = new Node.PropertySet(NodeId.Content(default), bag);
        return (Node.PropertySet)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
    }

    // ONE polymorphic realizing-detail reader discriminating on the element shape — never a BoltOf/WeldOf/CastOf/TendonOf/
    // BearingOf sibling family, and the SOLE realizing-family gate (the _ arm returns the empty DetailSchema.Realization bag
    // for a non-realizing element so All can fold the whole Extract<IfcElement> stream through it, no parallel family list).
    // Every arm folds onto the seam DetailSchema.Realization.Bag() (the author's IDENTICAL schema, never a hand-synced bag),
    // so an authored bolt and an imported one content-key identically. The egress Filter drops every non-finite Measure so
    // an unset OPTIONAL IFC scalar (a NaN-default reinforcing scalar, an unprofiled fastener's absent diameter) never emits a
    // NaN or misleading-0 measure; an arm therefore lists every candidate measure row unconditionally rather than branching
    // per presence, and the Filter re-projects through the bag record `with` so the schema-stamped SetName/precedence survive.
    public static PropertyBag Detail(IfcElement realizing) {
        PropertyBag detail = realizing switch {
            IfcMechanicalFastener fastener => FastenerDetail(fastener),
            IfcFastener fastener           => Rows(Joint(JointOf(fastener.PredefinedType)), Token(DetailSchema.FastenerType, fastener.PredefinedType.ToString())),
            IfcDiscreteAccessory accessory => AccessoryDetail(accessory),
            IfcReinforcingBar bar          => BarDetail(bar),
            IfcReinforcingMesh mesh        => MeshDetail(mesh),
            IfcTendon tendon               => TendonDetail(tendon),
            IfcTendonAnchor anchor         => Rows(Joint("Cast"), Token(AnchorType, anchor.PredefinedType.ToString())),
            IfcBearing bearing             => Rows(Joint("Bearing"), Token(BearingType, bearing.PredefinedType.ToString())),
            _                              => DetailSchema.Realization.Bag(),
        };
        return detail with { Values = detail.Values.Filter(static v => v is not PropertyValue.Measure m || double.IsFinite(m.Value.Si)) };
    }

    // --- [ROWS] -------------------------------------------------------------------------------
    // The bag-row constructors so each realizing arm is a flat declarative row list rather than repeating the
    // Enumerated/Text/Measure/OfSi construction: the joint modality through the schema's JointType row, a text token, and an
    // SI measure over its Dimension. Rows folds the candidate rows into the seam DetailSchema.Realization.Bag() through
    // ValueBag.With (last-write-wins). Joint composes DetailSchema.JointType + DetailSchema.Realization.Joint(kind) (the
    // PropertyValue.Enumerated over the schema's CLOSED allowed-set the egress facet validates against — never a local
    // Enumerated re-spelling the allowed set). Measured carries the DIMENSION-only QuantityType (the dimension-only OfSi the
    // author uses) so an imported and an authored NominalDiameter content-key identically. The material grade rides the seam
    // Material subgraph (Semantics/composition), never a SteelGrade column on this connection bag.
    static (PropertyName, PropertyValue) Joint(string kind) => (DetailSchema.JointType, DetailSchema.Realization.Joint(kind));
    static (PropertyName, PropertyValue) Token(PropertyName name, string value) => (name, new PropertyValue.Text(value));
    static (PropertyName, PropertyValue) Measured(PropertyName name, Dimension dim, double si) => (name, new PropertyValue.Measure(MeasureValue.OfSi(dim, si)));

    static PropertyBag Rows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        rows.ToSeq().Fold(DetailSchema.Realization.Bag(), static (bag, r) => bag.With(r.Name, r.Value));

    // --- [FASTENER] ---------------------------------------------------------------------------
    // A bolted/welded mechanical fastener: the JointType partition + the FastenerType token + the NominalDiameter. The
    // GeometryGym-internal mNominalDiameter has no public getter, so DiameterOf recovers it through the cross-section
    // profile radius; an absent diameter reads NaN (IfNone) and drops at the egress Filter rather than a fabricated 0.
    static PropertyBag FastenerDetail(IfcMechanicalFastener fastener) => Rows(
        Joint(JointOf(fastener.PredefinedType)),
        Token(DetailSchema.FastenerType, fastener.PredefinedType.ToString()),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, DiameterOf(fastener).IfNone(double.NaN)));

    static string JointOf(IfcMechanicalFastenerTypeEnum type) =>
        type is IfcMechanicalFastenerTypeEnum.STUDSHEARCONNECTOR or IfcMechanicalFastenerTypeEnum.SHEARCONNECTOR ? "Welded" : "Bolted";

    // The non-mechanical fastener (IfcFastener, sibling of IfcMechanicalFastener): a WELD realizes a Welded joint, a
    // GLUE/MORTAR a Bonded joint. It exposes only PredefinedType publicly (no diameter), so the inline arm carries the
    // normalized joint modality + the FastenerType token, the FastenerType token kept (mirroring BarType) for the
    // detailer's uniform bag read distinct from the Object node's classification-side PredefinedType.
    static string JointOf(IfcFastenerTypeEnum type) => type is IfcFastenerTypeEnum.WELD ? "Welded" : "Bonded";

    // The fastener/tendon nominal diameter rides its cross-section profile radius: mNominalDiameter/mNominalLength are
    // internal on the occurrence AND IfcMechanicalFastenerType (and the tendon's diameter likewise internal), no public
    // getter, so the diameter is recovered through the inherited HasAssociations IfcRelAssociatesMaterial.RelatingMaterial
    // (IfcMaterialProfileSetUsage -> ForProfileSet.MaterialProfiles -> Profile -> IfcCircleProfileDef.Radius x 2), the
    // same chain the profile-hosted IfcMechanicalFastener(IfcProduct, IfcMaterialProfileSetUsage, IfcAxis2Placement3D,
    // double) authoring ctor binds. The finiteness guard rides the Choose so the head is the first circle with a FINITE
    // radius (a degenerate NaN-radius profile never masks a later valid one); None when no circle profile binds a diameter.
    static Option<double> DiameterOf(IfcElement element) =>
        element.HasAssociations.AsIterable()
            .Choose(static rel => rel is IfcRelAssociatesMaterial { RelatingMaterial: IfcMaterialProfileSetUsage { ForProfileSet: { } profileSet } } ? Some(profileSet) : None)
            .SelectMany(static set => set.MaterialProfiles.AsIterable())
            .Choose(static profile => profile.Profile is IfcCircleProfileDef { Radius: var radius } && double.IsFinite(radius) ? Some(radius * 2d) : None)
            .Head;

    // --- [ACCESSORY] --------------------------------------------------------------------------
    // A fabricated framing connector (IfcDiscreteAccessory, the IfcElementComponent sibling of IfcMechanicalFastener):
    // a steel saddle/bracket/anchorplate that physically IS the connector body, FASTENED BY a separate IfcMechanicalFastener.
    // This is the IMPORT counterpart of the Rasm.Materials Connection/hanger#HANGER_FAMILY projection — the Materials
    // ConnectionProjector denormalizes BOTH tokens onto the one hanger ConnectionItem and authors a two-token bag
    // (AccessoryType = the connector's own IfcDiscreteAccessoryTypeEnum SHOE/BRACKET/ANCHORPLATE, FastenerType = the
    // SEPARATE attaching IfcMechanicalFastenerTypeEnum NAILPLATE/NAIL/SCREW/BOLT), so the Bim reader matches that SHAPE by
    // reading the AccessoryType off the connector's own PredefinedType AND recovering the FastenerType from the co-realizing
    // IfcMechanicalFastener sibling reached through the IfcElement.IsConnectionRealization back-pointer (the
    // IfcRelConnectsWithRealizingElements set both the accessory and its attaching fastener join) — so an imported framing
    // connector content-keys to the SAME bag an authored one does. JointType is Bolted (a fabricated connector is mechanically
    // attached, mirroring the Materials hanger Joint("Bolted")); the carried-member scalars ride the general Pset fold, not
    // this realizing bag. FastenerOf yields None (the FastenerType row dropped) when the accessory has no co-realizing
    // mechanical fastener — never a fabricated token.
    static PropertyBag AccessoryDetail(IfcDiscreteAccessory accessory) =>
        FastenerOf(accessory).Match(
            Some: fastenerType => Rows(
                Joint("Bolted"),
                Token(DetailSchema.AccessoryType, accessory.PredefinedType.ToString()),
                Token(DetailSchema.FastenerType, fastenerType)),
            None: () => Rows(
                Joint("Bolted"),
                Token(DetailSchema.AccessoryType, accessory.PredefinedType.ToString())));

    // The attaching-fastener token recovery: the connector and its mechanical fastener co-occupy an
    // IfcRelConnectsWithRealizingElements (the connector's IsConnectionRealization back-pointer), so the sibling
    // IfcMechanicalFastener's PredefinedType is the attaching FastenerType the Materials hanger bag denormalized — one bounded
    // hop to the co-realizing sibling, reading only its PredefinedType (the joint TOPOLOGY itself rides the EdgeProjection
    // Connect edge whose Connect.Realizing field carries the realizing node, never re-derived here). Head of the first co-realizing mechanical fastener.
    static Option<string> FastenerOf(IfcDiscreteAccessory accessory) =>
        accessory.IsConnectionRealization.AsIterable()
            .SelectMany(static rel => rel.RealizingElements.AsIterable())
            .Choose(static realizing => realizing is IfcMechanicalFastener fastener ? Some(fastener.PredefinedType.ToString()) : None)
            .Head;

    // --- [REINFORCING] ------------------------------------------------------------------------
    // A cast-in reinforcing bar: the public NominalDiameter (IfcReinforcingBarType.NominalDiameter type-fallback get) /
    // CrossSectionArea / BarLength scalars over their SI dimensions + the BarType (STUD is the cast-in bar, NOT the welded
    // connector) and BarSurface tokens; the NaN defaults drop at the Detail egress Filter.
    static PropertyBag BarDetail(IfcReinforcingBar bar) => Rows(
        Joint("Cast"),
        Token(DetailSchema.BarType, bar.PredefinedType.ToString()),
        Token(BarSurface, bar.BarSurface.ToString()),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, bar.NominalDiameter),
        Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, bar.CrossSectionArea),
        Measured(BarLength, Dimension.LengthDim, bar.BarLength));

    // A cast-in reinforcing mesh: the full public sheet geometry — overall length/width, the longitudinal/transverse bar
    // diameters, spacings, and cross-section areas — + the MeshType token, the welded-mesh fabrication detail a single-bar
    // reader drops; every unset NaN component drops at the Detail egress Filter.
    static PropertyBag MeshDetail(IfcReinforcingMesh mesh) => Rows(
        Joint("Cast"),
        Token(MeshType, mesh.PredefinedType.ToString()),
        Measured(MeshLength, Dimension.LengthDim, mesh.MeshLength),
        Measured(MeshWidth, Dimension.LengthDim, mesh.MeshWidth),
        Measured(LongitudinalBarNominalDiameter, Dimension.LengthDim, mesh.LongitudinalBarNominalDiameter),
        Measured(TransverseBarNominalDiameter, Dimension.LengthDim, mesh.TransverseBarNominalDiameter),
        Measured(LongitudinalBarSpacing, Dimension.LengthDim, mesh.LongitudinalBarSpacing),
        Measured(TransverseBarSpacing, Dimension.LengthDim, mesh.TransverseBarSpacing),
        Measured(LongitudinalBarCrossSectionArea, Dimension.AreaDim, mesh.LongitudinalBarCrossSectionArea),
        Measured(TransverseBarCrossSectionArea, Dimension.AreaDim, mesh.TransverseBarCrossSectionArea));

    // A post-tensioning tendon (IfcReinforcingElement, the cast-in prestressing peer of the bar/mesh): its native
    // NominalDiameter/CrossSectionArea/TensionForce are GeometryGym-internal with no public getter, so the bag carries the
    // TendonType token + the diameter recovered through the profile channel (NaN-filtered when unprofiled), the material
    // grade riding the seam Material subgraph (Semantics/composition), not this connection bag.
    static PropertyBag TendonDetail(IfcTendon tendon) => Rows(
        Joint("Cast"),
        Token(TendonType, tendon.PredefinedType.ToString()),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, DiameterOf(tendon).IfNone(double.NaN)));

    // --- [READER_ROWS] ------------------------------------------------------------------------
    // The realizing-detail row names the IMPORT reader recovers BEYOND the canonical DetailSchema vocabulary (the author
    // writes the canonical schema statics — the twelve discrete-part diameter/throat/lap rows this connection reader composes;
    // panel/deck/membrane product rows are owned by DetailSchema.Product and the general property fold. PropertyName stays an OPEN key per
    // Properties/property#DETAIL_SCHEMA, so an
    // ingest-only scalar the Materials author never mints — a mesh sheet's geometry, a tendon/anchor/bearing type token, a
    // bar's surface + overall length — is one static the reader writes and a downstream Fabrication consumer reads by name).
    // An authored bag carrying only the schema rows and an imported bag carrying these extra rows are faithfully DIFFERENT
    // content-keyed nodes, never a forced byte-match; these never widen the seam DetailSchema (a Materials-authored row is a
    // schema static, a Bim-ingest-only row is here), and the material grade still rides the seam Material subgraph, never here.
    static readonly PropertyName AnchorType = PropertyName.Create("AnchorType");
    static readonly PropertyName BearingType = PropertyName.Create("BearingType");
    static readonly PropertyName TendonType = PropertyName.Create("TendonType");
    static readonly PropertyName BarSurface = PropertyName.Create("BarSurface");
    static readonly PropertyName BarLength = PropertyName.Create("BarLength");
    static readonly PropertyName MeshType = PropertyName.Create("MeshType");
    static readonly PropertyName MeshLength = PropertyName.Create("MeshLength");
    static readonly PropertyName MeshWidth = PropertyName.Create("MeshWidth");
    static readonly PropertyName LongitudinalBarNominalDiameter = PropertyName.Create("LongitudinalBarNominalDiameter");
    static readonly PropertyName TransverseBarNominalDiameter = PropertyName.Create("TransverseBarNominalDiameter");
    static readonly PropertyName LongitudinalBarSpacing = PropertyName.Create("LongitudinalBarSpacing");
    static readonly PropertyName TransverseBarSpacing = PropertyName.Create("TransverseBarSpacing");
    static readonly PropertyName LongitudinalBarCrossSectionArea = PropertyName.Create("LongitudinalBarCrossSectionArea");
    static readonly PropertyName TransverseBarCrossSectionArea = PropertyName.Create("TransverseBarCrossSectionArea");
}
```

## [03]-[RESEARCH]

- [REALIZING_ELEMENT_SURFACE]: the realizing-element surface `ConnectionProjection.Detail` reads grounds against the live GeometryGym 25.7.30 decompile (`.api/api-geometrygym-ifc` rows `IfcRelConnectsWithRealizingElements`/`IfcMechanicalFastener`/`IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcMaterialProfileSetUsage`/`IfcCircleProfileDef`; `assay api` `--key GeometryGymIFC_Core`) — `IfcMechanicalFastener : IfcElementComponent` exposes ONLY `PredefinedType` (`IfcMechanicalFastenerTypeEnum`: `ANCHORBOLT`/`BOLT`/`DOWEL`/`NAIL`/`NAILPLATE`/`RIVET`/`SCREW`/`SHEARCONNECTOR`/`STAPLE`/`STUDSHEARCONNECTOR`/`COUPLER`) as a public scalar — its `mNominalDiameter`/`mNominalLength` are `internal` fields with NO public getter on the occurrence OR `IfcMechanicalFastenerType`, so the diameter rides the associated `IfcMaterialProfileSetUsage` (`ForProfileSet` → `MaterialProfiles` `LIST<IfcMaterialProfile>` → `Profile` → `IfcCircleProfileDef.Radius`) reached through the inherited `IfcObjectDefinition.HasAssociations` (`SET<IfcRelAssociates>`) `IfcRelAssociatesMaterial.RelatingMaterial`, the public channel the profile-hosted authoring ctor `IfcMechanicalFastener(IfcProduct, IfcMaterialProfileSetUsage, IfcAxis2Placement3D, double)` binds; `IfcReinforcingBar : IfcReinforcingElement` carries public `NominalDiameter` (with `IfcReinforcingBarType.NominalDiameter` fallback)/`CrossSectionArea`/`BarLength` (`double`), `PredefinedType` (`IfcReinforcingBarTypeEnum`), and `BarSurface` (`IfcReinforcingBarSurfaceEnum`); `IfcReinforcingMesh : IfcReinforcingElement` carries public `PredefinedType` (`IfcReinforcingMeshTypeEnum`) and `MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea` (all `double`, `NaN`-defaulted, dropped finite at the `Filter`); the family CLOSES over `IfcFastener : IfcElementComponent` (the non-mechanical sibling of `IfcMechanicalFastener`, public `PredefinedType` `IfcFastenerTypeEnum` `GLUE`/`MORTAR`/`WELD`), `IfcDiscreteAccessory : IfcElementComponent` (the fabricated framing-connector sibling, public `PredefinedType` `IfcDiscreteAccessoryTypeEnum` `SHOE`/`BRACKET`/`ANCHORPLATE`; the attaching `IfcMechanicalFastener` rides the public `IfcElement.IsConnectionRealization` `SET<IfcRelConnectsWithRealizingElements>` → `RealizingElements` back-pointer, the `Rasm.Materials` hanger projection's two-token bag the import counterpart matches), `IfcTendon : IfcReinforcingElement` (public `PredefinedType` `IfcTendonTypeEnum` `STRAND`/`WIRE`/`BAR`/`COATED`; its `mNominalDiameter`/`mCrossSectionArea`/`mTensionForce`/`mPreStress` are `internal` with NO public getter, so the diameter rides the SAME `IfcCircleProfileDef.Radius` profile channel as the mechanical fastener), `IfcTendonAnchor : IfcReinforcingElement` (public `PredefinedType` `IfcTendonAnchorTypeEnum`), and `IfcBearing : IfcBuiltElement` (public `PredefinedType` `IfcBearingTypeEnum` `CYLINDRICAL`/`SPHERICAL`/`ELASTOMERIC`/`POT`/`GUIDE`/`ROCKER`/`ROLLER`/`DISK`) — all `IfcElement` → `IfcProduct`, and `IfcMechanicalFastener`/`IfcFastener`/`IfcDiscreteAccessory` and `IfcReinforcingBar`/`Mesh`/`Tendon`/`TendonAnchor` are SIBLINGS leaves, so the one `Extract<IfcElement>` walk discovers each once with no double-count and the `Detail` switch is the sole realizing discriminator (a non-realizing element folds to the empty bag) — so the `Detail` switch reads the real realizing-element scalar members the general `Object` fold does not across the WHOLE realizing family, the connection detail staying host-neutral scalar data and never re-tessellating the element; the `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` mechanical-fastener partition and the `IfcFastener` `WELD` partition discriminate the `Welded` joint, every other discrete mechanical fastener `Bolted`, the `IfcFastener` `GLUE`/`MORTAR` `Bonded`, the fabricated `IfcDiscreteAccessory` framing connector `Bolted`, the `IfcBearing` `Bearing`, and the reinforcing bar/mesh/tendon/anchor `Cast`.
- [CONNECTION_AS_PROPERTY_BAG]: the connection detail is a seam `PropertySet` bag bound through an `Assign.PropertyDefinition` edge, not a stored record — the consumer-facing `Element` is the `Graph/element#ELEMENT_GRAPH` `Bake` fold, never a second stored record, mirroring the `Model/structural#STRUCTURAL_PROJECTION` precedent (the deep structural reader lowers the GeometryGym surface onto neutral `Map<PropertyName, PropertyValue>` payloads a `Generic` edge or a `PropertySet` node carries, retiring the typed `MemberConnection`/`SupportRestraint` store) — so `Detail` reads directly into the seam `Properties/property#PROPERTY_VALUE` typed bag and `Bag` mints the content-keyed `Graph/element#NODE_MODEL` `Node.PropertySet` (`NodeId.Content` over `Node.ToCanonicalBytes`, the kernel seed-zero `XxHash128` the seam owns [H7], never a second `(GeometryKey, DetailKey)` hasher), the `Graph/element#ELEMENT_GRAPH` `Bake` fold's `Assign.PropertyDefinition` arm merging the bag into `element.Properties`; the typed `BoltPattern`/`WeldSchedule` reconstruction relocates to the `csharp:Rasm.Fabrication` consumer (as the typed analysis model relocated to `Rasm.Compute`), the seam carrying only the neutral typed bag, so the migration source's rich `ConnectionRealization` `[Union]` whose `BoltOf` filled an empty `Grid`/`0` `EdgeDistance` (decorative capability the reader never populated) is the deleted form — the bag carries the genuinely-readable scalars, the custom grid/edge-distance Psets riding the general `Projection/semantic#SEMANTIC_PROJECTOR` `Bags` fold.
- [JOINT_TOPOLOGY_JOIN]: the joint topology is the `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing node on the `Connect.Realizing` `Option<NodeId>` field (realizing-ness the field, not a `ConnectKind` row — the medium closes at `Path`/`Port`) the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors from `IfcRelConnectsWithRealizingElements` (reading the relating/related members onto the `From`/`To` endpoints and the realizing head into the `Connect.Realizing` `Option<NodeId>`), so the `(MemberGlobalId, MemberGlobalId)` pair the migration source bound onto a `BimModel`-keyed `ConnectionDetail` is the deleted form — the joint endpoints are the `Connect` edge's resolved `NodeId` pair, the realizing element its `Object` node, and the realizing detail this page's bag; the analytical member↔connection topology the `Model/structural#STRUCTURAL_PROJECTION` `IfcRelConnectsStructuralMember` `Generic` edge carries (the 6-DOF restraint) and the physical realizing `Connect` edge MEET on the SHARED `ElementGraph` nodes (the same rooted `NodeId`s), so a `csharp:Rasm.Compute`/`csharp:Rasm.Fabrication` consumer reads both the analytical idealization and the physical realizing detail off the ONE graph it holds, never a `GlobalId`-pair member-selection surface and never a second store joining them.
- [EGRESS_AND_WIRE_RETIREMENT]: the connection egress is the `Projection/egress#IFC_EGRESS` `Emit` generic round-trip, not a connection-specific writer — `Emit.Author` re-authors the realizing `Object` node onto `IfcMechanicalFastener`/`IfcReinforcingBar` (resolving the `IfcClass` from the generic `Classification` code and re-stamping the `PredefinedType` through the `AdmitPredefined` gate [C6]), `ReauthorProperties` re-authors the detail bag onto an `IfcPropertySet` + `IfcRelDefinesByProperties`, and `ReauthorRelationships` re-authors the realizing `Connect` edge (its `Connect.Realizing` field set) onto `IfcRelConnectsWithRealizingElements` with its realizing intermediary (the special third-endpoint handling) — so the `ConnectionItemWire` carrier + `ConnectionWire.Author` the migration source kept (the Bim-side serializer of the `Rasm.Materials` `ConnectionItem` axis) is the deleted form, mirroring `Semantics/composition#MATERIAL_COMPOSITION` retiring the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers when `Rasm.Materials` became a projector; a connection element authored from the Materials/Fabrication side projects onto the seam graph as an `Object` node + `Connect` edge (a future Fabrication `IElementProjection`, one registration row) the `Emit` re-authors, never a second wire crossing the boundary, the material binding of a realizing element (its steel grade, embodied carbon, classification, appearance) riding the seam `Material` subgraph the `ComponentProjector` lowers and the `Associate` edge joins, NOT this connection bag.

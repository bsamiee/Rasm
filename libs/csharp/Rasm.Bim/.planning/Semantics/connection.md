# [BIM_CONNECTION]

`ConnectionProjection` is the realizing-element connection-detail reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: it lowers the WHOLE GeometryGym realizing-element surface — the bolted/welded `IfcMechanicalFastener` and the bonded/welded `IfcFastener`, the fabricated `IfcDiscreteAccessory` framing connector, the cast-in `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`, the support `IfcBearing`, and the isolation `IfcVibrationIsolator` — onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the ONE neutral realizing-detail schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` AUTHORS — this reader READS the same schema, never a hand-synced parallel bag), bound to the realizing element's `Graph/element#NODE_MODEL` `Object` node through one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, so an authored realizing element and a re-imported IFC one content-key to one `Node.PropertySet` and a `csharp:Rasm.Fabrication` detailer reads the bolt diameter, the stud-shear connector, the framing-connector accessory token, the reinforcing cover, the post-tensioning tendon, and the bearing type off the ONE `Graph/element#ELEMENT_GRAPH` `Bake`-derived element it already holds — never a second store. The physical joint TOPOLOGY — which two members meet through which realizing element — is the `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing node on the `Connect.Realizing` `Option<NodeId>` field (realizing-ness the field, never a `ConnectKind` row — the medium closes at `Path`/`Port`) the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` already authors from `IfcRelConnectsWithRealizingElements` (fanning one edge per `RealizingElements` member into the seam `Connect.Realizing` option, so a multi-realizer joint keeps every realizer); this page owns ONLY the realizing element's fabrication DETAIL the general `Object` fold does not read — the native reinforcing scalars, the declared `SteelGrade` designation, and the fastener nominal diameter the internal GeometryGym scalar hides.

This RETIRES the migration source's parallel `ConnectionDetail` record + `ConnectionRealization` `[Union]` + `BoltPattern`/`WeldSchedule`/`BearingSurface` `[ComplexValueObject]` + `ConnectionKind`/`Clearance` family keyed by `BimModel`/`GlobalId` AND its hand-rolled `(GeometryKey, DetailKey)` second content-key — the very "second stored record off the element" the rebuild forbids, mirroring `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection` retiring the `MemberConnection`/`SupportRestraint` typed store — AND the `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary, the deleted form mirroring `Semantics/composition#MATERIAL_COMPOSITION` retiring the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers. The joint modality is the realizing `Object.PredefinedType` token plus the bag's `JointType` enumerated, the bolt/weld/bearing/cast detail is typed `Properties/property#PROPERTY_VALUE` `PropertyValue` entries, and the IFC egress is the `Projection/egress#IFC_EGRESS` `Emit` generic round-trip — the realizing `IfcMechanicalFastener`/`IfcReinforcingBar` re-authored as an `Object` node, the detail bag as an `IfcPropertySet` through `ReauthorProperties`, the joint as `IfcRelConnectsWithRealizingElements` through `ReauthorRelationships` — never a connection-specific writer. The reader is HOST-NEUTRAL (it binds the realizing geometry by the `Graph/element#NODE_MODEL` `RepresentationContentHash` content key, never re-tessellating the fastener) and detail-TOTAL (a realizing element with no readable detail yields no bag — the `Fin` rail carries only the seam `MeasureValue.OfSi` admission and the `All` root-resolution, so a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` rather than stranding a source entity; the joint edge's own endpoint rail stays the `EdgeProjection` `edge-endpoint-miss`), a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection`, `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection`, and `Semantics/georeference#GEO_PROJECTION` `GeoReferenceProjector` — so a connection enrichment never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [02]-[CONNECTION_DETAIL]: `ConnectionProjection` the GeometryGym realizing-element-detail reader — `Detail` the ONE polymorphic attribute-bag reader discriminating the NINE-family realizing-element surface (`IfcMechanicalFastener` bolt/weld, `IfcFastener` glue/mortar/weld, `IfcDiscreteAccessory` framing connector, `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor` cast, `IfcBearing` support, `IfcVibrationIsolator` isolation) onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the IDENTICAL neutral schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` authors, never a hand-synced parallel bag), the `Joint`/`Token`/`Measured`/`Rows` row-fold each arm composes (the schema `JointType` modality row, a token, an SI `Measured` over its `Dimension`), `DiameterOf` the fastener/tendon nominal-diameter recovery through the associated `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section (the public channel for the GeometryGym-internal `mNominalDiameter` scalar), `FastenerOf` the co-realizing attaching-fastener token-set recovery off the `IfcElement.IsConnectionRealization` back-pointer (ALL tokens, distinct + ordinal-sorted), `Bag` the realizing-vs-not gate composing the shared `Mint` content-keyed seam `PropertySet` node, and `All` the fold over the model's `Extract<IfcElement>` stream (the `Detail` switch the sole realizing discriminator) producing the `(bag node, Assign.PropertyDefinition edge)` pairs the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` composes onto the `ElementGraph`.

## [02]-[CONNECTION_DETAIL]

- Owner: `ConnectionProjection` the static realizing-element-detail reader `SemanticProjector` composes, lowering the WHOLE GeometryGym realizing-element surface onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node — never a stored record and never a hand-synced bag, the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors so an authored realizing element and a re-imported one are one content-keyed `Node.PropertySet`. It owns the polymorphic `Detail` attribute-bag reader (one entry discriminating the NINE-family realizing surface — mechanical/non-mechanical fastener, fabricated discrete accessory, reinforcing bar/mesh/tendon/anchor, support bearing, isolation vibration isolator — onto its bolt/weld/bonded/accessory/cast/bearing bag through the `Joint`/`Token`/`Measured`/`Rows` row-fold over `DetailSchema.Realization.Bag()`), the `DiameterOf` cross-section diameter recovery, the `FastenerOf` co-realizing attaching-fastener token-SET recovery (ALL co-realizing `IfcMechanicalFastener` tokens fold — distinct, ordinal-sorted — never a `.Head` slice), the `Bag` realizing-gate composing the shared `Mint` of the seam `Node.PropertySet`, and the `All` model fold over `Extract<IfcElement>`; the typed structures the migration source minted (`ConnectionDetail`, the `ConnectionRealization` `[Union]`, `BoltPattern`/`WeldSchedule`/`BearingSurface`, `ConnectionKind`, `Clearance`) are all GONE — the realizing element is the seam `Object` node the general `Objects` fold mints, the joint modality its `PredefinedType` token plus the bag `JointType` row, the joint topology the neutral `Connect` edge carrying the realizing node on its `Connect.Realizing` field, and the fabrication scalars the typed `PropertyValue` bag entries.
- Entry: `ConnectionProjection.Detail(IfcElement realizing, UnitScale scale, Op key)` is the ONE polymorphic attribute-bag reader discriminating on the realizing-element shape, `Fin<PropertyBag>` carrying the seam `MeasureValue.OfSi` admission (`scale` the per-projection native→SI coercion every measured row multiplies through) and the schema's own `DetailSchema.Realization.Joint(selected, key)` allowed-set admission, so an out-of-set joint token rejects at the seam declarer rather than passing this call site unchecked — an `IfcMechanicalFastener` onto the bolt/weld bag (the `JointType`/`FastenerType` tokens plus the `NominalDiameter` recovered from the cross-section profile), an `IfcFastener` onto the bonded/weld bag (the `JointType`/`FastenerType` tokens), an `IfcDiscreteAccessory` onto the framing-connector bag (the `JointType`/`AccessoryType` tokens plus the `FastenerType` token set of ALL co-realizing attaching `IfcMechanicalFastener`s — a single fastener lands the two-token `Text` shape the `Rasm.Materials` `Component/connector#CONNECTOR_FAMILY` `ConnectorDetail` seed bag authors (`ComponentDetail.Token(DetailSchema.AccessoryType, ...)` + `Token(DetailSchema.FastenerType, install.Fastener.IfcFastenerType)`, projected by the `ComponentProjector`), content-keying byte-identically; a multi-fastener attachment (nailplate + screw, bolt + stud) lands the typed `PropertyValue.List` of distinct ordinal-sorted tokens the seam value family carries natively), an `IfcReinforcingBar` onto the cast bag (the `BarType`/`BarSurface`/`SteelGrade` tokens plus the native `NominalDiameter`/`CrossSectionArea`/`BarLength` scalars), an `IfcReinforcingMesh` onto the mesh bag (the `MeshType`/`SteelGrade` tokens plus the native mesh length/width, the longitudinal/transverse bar diameters, spacings, and cross-section areas), an `IfcTendon` onto the cast bag (the `TendonType`/`SteelGrade` tokens plus the profile-recovered `NominalDiameter`), an `IfcTendonAnchor`/`IfcBearing`/`IfcVibrationIsolator` onto its type-token bag (`AnchorType` + `SteelGrade`/`BearingType`/`IsolatorType` plus the normalized `JointType`), and any other (or null) onto the empty bag — detail ABSENCE never faults (an unreadable detail yields the empty/`None` bag; the entity class is the general fold's `BimFault.UnmappedClass`), the rail carrying only the seam value admission, and no `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family — one polymorphic `Detail` discriminating by input value, the rich arms (`FastenerDetail`/`AccessoryDetail`/`BarDetail`/`MeshDetail`/`TendonDetail`) reading the hidden scalars onto the schema bag, the type-only arms inline; `ConnectionProjection.Bag(IfcElement realizing, double tolerance, UnitScale scale, Op key)` wraps a non-empty `Detail` `PropertyBag` (the schema's `Values` non-empty) in the content-keyed seam `Node.PropertySet` (`Option<T>.None` for an empty detail so a detail-free realizing element produces no bag); `ConnectionProjection.All(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, Op key)` folds every `IfcElement` the project carries — the `Detail` switch the SOLE discriminator of the nine realizing families (`IfcMechanicalFastener`/`IfcFastener`/`IfcDiscreteAccessory`/`IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcBearing`/`IfcVibrationIsolator`), a non-realizing element folding to the empty `Detail` and so to no bag — into the `Seq<(Node Bag, Relationship Edge)>` the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` concats onto its node and edge sets (exactly as it concats `Materials` and `EdgeProjection.All`), so a new realizing family is one `Detail` arm and never a parallel extract list to drift.
- Auto: each arm folds its rows onto the seam `DetailSchema.Realization.Bag()` through the `Joint`/`Token`/`Measured`/`Rows` row-fold — one `Joint` schema modality row, the type tokens, and the SI `Measured` rows — so the arm is a flat declarative row list, never a repeated `MeasureValue.OfSi` construction and never a hand-spelled set-name or precedence (both ride the schema). `Detail` reads the realizing element's NATIVE fabrication scalars the general `Object` fold leaves on the geometry — the reinforcing bar/mesh expose their `NominalDiameter`/`CrossSectionArea`/`BarLength`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea` as public doubles wrapped directly into a `MeasureValue.OfSi(Dimension.LengthDim/AreaDim, ...)` (the IFC scalars are SI-base, never re-coerced — the SAME dimension-only overload the author uses, so an authored and an imported row content-key identically), and the `JointType` enumerated derives from the realizing family (an `IfcMechanicalFastener` `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` or an `IfcFastener` `WELD` is `Welded`, every other discrete mechanical fastener `Bolted`, an `IfcFastener` `GLUE`/`MORTAR` `Bonded`, an `IfcBearing` and the isolation-bearing `IfcVibrationIsolator` `Bearing`, a reinforcing bar/mesh/tendon/anchor `Cast`) through `DetailSchema.Realization.Joint(kind)`, the schema's closed `JointTypes` allowed set the egress facet validates against; the reinforcing root's public `IfcReinforcingElement.SteelGrade` lands as the ingest-only `SteelGrade` token (the deprecated-but-live EXPRESS designation, the only grade carrier when a legacy export binds no material); the mechanical-fastener AND tendon `NominalDiameter` is the special case — `mNominalDiameter`/`mNominalLength` are GeometryGym-internal on both with NO public getter, so `DiameterOf` recovers the diameter through the inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` → `ForProfileSet.MaterialProfiles` → `Profile` → `IfcCircleProfileDef.Radius` × 2), the documented public round-trip channel, yielding `Option<double>.None` (read `IfNone(NaN)` and dropped at the `Filter`, never a fabricated 0) when no circle profile binds; every non-finite scalar (an unset GeometryGym `NaN` default) and every blank token (an undeclared `SteelGrade` `""`) is dropped at the `Detail` egress `Filter` so a partially-specified realizing element never emits a misleading measure or an empty text row; `Bag` mints the seam `Node.PropertySet` whose id is `NodeId.Content` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical realizing details dedup to one node, and `All` resolves each realizing element's rooted `NodeId` through the `rooted` map and binds the bag through an `Assign.PropertyDefinition` edge — a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` (skipping it would strand a source entity while claiming the realization fold was total; the joint edge's own endpoint rail stays `EdgeProjection`'s `edge-endpoint-miss`).
- Receipt: the connection-detail bag lands on the ONE seam `ElementGraph` as a `PropertySet` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold merges into `element.Properties` through the realizing element's `Assign.PropertyDefinition` edge, so the `csharp:Rasm.Fabrication` detailer reads `element.Properties.Find(b => b.SetName == DetailSchema.Realization.SetName).Bind(b => b.Find(DetailSchema.NominalDiameter))` for the bolt diameter / weld stud / reinforcing cover off the baked realizing element (the NEUTRAL `SetName`, never an IFC literal — the `Rasm_ConnectionRealization` Pset name is applied only at the `Projection/egress#IFC_EGRESS` mapping), and the joint topology off the `Connect` edge whose `Connect.Realizing` field carries the realizing node the `EdgeProjection` authors — a steel bolted moment connection's fasteners, a stud-shear-connector deck weld, and a cast-in reinforcing lap each carrying their physical detail on the one graph the consumer already holds, never a parallel connection store and never a second member-selection surface; the `Projection/egress#IFC_EGRESS` `Emit` re-authors the bag (`IfcPropertySet` through `ReauthorProperties`) and the joint (`IfcRelConnectsWithRealizingElements` through `ReauthorRelationships`) generically, so the connection round-trips with the rest of the graph.
- Packages: GeometryGymIFC_Core (the realizing-element surface consumed as settled vocabulary), Rasm.Element (the seam `DetailSchema` (the neutral realizing-detail schema this reader composes) + the `Node`/`NodeId`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`Relationship`/`AssignKind` payloads, the schema carrying the `SetName`/`OccurrenceWins`/`JointTypes` so this reader hand-spells none), LanguageExt.Core (`Option`/`Seq`/`Map`).
- Growth: a new realizing-element family is one arm on the `Detail` switch (a `Rows` of its `Joint`/`Token`/`Measured` rows inline, or a dedicated reader where a hidden scalar needs `DiameterOf`); a new fabrication scalar is one `Measured` row on its arm carrying its `MeasureValue` over the composed `Dimension` (a row in the canonical `DetailSchema` vocabulary composes the schema static, an ingest-only scalar one `PropertyCategory.Seam.Row` mint in the `[READER_ROWS]` block); a new joint modality is one token on the seam `DetailSchema.Realization.JointTypes` allowed set plus its `JointType` derivation, never a reader-local allowed set; never a per-joint-type connection record, never a `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family, never a second connection store, never a hand-synced parallel detail bag, never a `(GeometryKey, DetailKey)` parallel content key, and never a re-tessellation of the realizing element.
- Boundary: the connection detail is the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node, COMPOSED through `DetailSchema.Realization.Bag()`/`.Joint(kind)` (the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors) — a hand-synced parallel bag re-spelling the set name, the `OccurrenceWins` precedence, or the `JointTypes` allowed set is the deleted form (the reader READS the seam-declared schema, never a copy), and a typed `ConnectionDetail`/`ConnectionRealization`/`BoltPattern`/`WeldSchedule`/`BearingSurface`/`ConnectionKind`/`Clearance` second-store record family is the deleted form (mirroring `StructuralProjection` retiring `MemberConnection`/`SupportRestraint`) — the realizing element is the seam `Object` node, its detail the schema bag the `Bake` fold reads flat; the in-graph bag carries the NEUTRAL `SetName` and the IFC `Rasm_ConnectionRealization` Pset name is applied ONLY at the `Projection/egress#IFC_EGRESS` `Emit` mapping, so a `Rasm_ConnectionRealization` literal as the in-graph set name is the deleted form; the canonical realizing-detail rows compose the `DetailSchema` `PropertyName` statics while an INGEST-ONLY scalar the author never mints (a mesh sheet's geometry, a tendon/anchor/bearing token, a bar's surface + overall length) is a reader-side `[READER_ROWS]` row minted through the owner-blessed `PropertyCategory.Seam.Row` category — a call-site `PropertyName.Create` in this reader is the deleted form and a row a second package begins keying on is promoted to a `DetailSchema` static at the seam owner — an authored bag and a richer imported bag are faithfully DIFFERENT content-keyed nodes, never a forced byte-match, and a reader-side row never widens the seam `DetailSchema`; the `BimModel`/`BimElement` join (`federated.Elements`, the `(MemberGlobalId, MemberGlobalId)` pair, the `BindFederated` dangling-reference rail) is GONE with the retired element records, the joint endpoints being the `Connect` edge's `NodeId` pair the `EdgeProjection` resolves and the analytical member↔connection topology the `Model/structural#STRUCTURAL_PROJECTION` `IfcRelConnectsStructuralMember` `Generic` edge, both meeting on the SHARED graph nodes, never a `GlobalId`-pair selection surface; the detail-bag attachment is ONE polymorphic `Detail` discriminating by input value and a `RealizationOf`/`BoltOf`/`WeldOf`/`LapOf`/`TendonOf`/`BearingOf` sibling-method family is the deleted form; detail ABSENCE never faults — an unreadable detail is the empty/`None` bag, and routing a missing scalar or token onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the entity-class rail is the general fold's `Fin<GraphDelta>`) — while the fold's own rail is real: a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` and a malformed seam measure faults through `MeasureValue.OfSi`; the connection detail stays host-neutral scalar data and a RhinoCommon `Brep`/`Mesh` realizing-element field or an in-process fastener tessellation is the named seam violation, the realizing geometry binding by the `RepresentationContentHash` content key; the GeometryGym realizing surface (`IfcMechanicalFastener.PredefinedType` `IfcMechanicalFastenerTypeEnum` and `IfcFastener.PredefinedType` `IfcFastenerTypeEnum`, the `IfcDiscreteAccessory.PredefinedType` `IfcDiscreteAccessoryTypeEnum` plus the `IfcElement.IsConnectionRealization` `SET<IfcRelConnectsWithRealizingElements>` back-pointer to the co-realizing attaching `IfcMechanicalFastener` SET — ALL tokens fold distinct and ordinal-sorted so an IFC file's set order never forks the content key, a single token the authored `Text` shape and a multi-token set the typed `PropertyValue.List`, never a `.Head` slice dropping a nailplate+screw second fastener and never a joined literal, the public `IfcReinforcingBar.NominalDiameter` (`IfcReinforcingBarType.NominalDiameter` type-fallback)/`CrossSectionArea`/`BarLength`/`PredefinedType`/`BarSurface`, the public `IfcReinforcingMesh.PredefinedType`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea`, the `IfcTendon.PredefinedType` `IfcTendonTypeEnum` / `IfcTendonAnchor.PredefinedType` `IfcTendonAnchorTypeEnum` / `IfcBearing.PredefinedType` `IfcBearingTypeEnum` / `IfcVibrationIsolator.PredefinedType` `IfcVibrationIsolatorTypeEnum`, the public `IfcReinforcingElement.SteelGrade` designation, the `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` or bare `IfcMaterialProfileSet`) → `IfcCircleProfileDef.Radius` chain) is consumed as settled vocabulary (`.api/api-geometrygym-ifc`) and a hand-rolled realizing reader is the deleted form; the mechanical-fastener and tendon nominal diameter rides the associated circle-profile radius (the public channel for the internal `mNominalDiameter`) and a fabricated `0` diameter on an unprofiled element is the deleted form (the entry reads `NaN` and is dropped at the egress `Filter`); the realizing element's CLASSIFICATION and MATERIAL ride the general `Object`/`Associate` folds, not this bag — an ASSOCIATED material's grade or embodied-carbon column on the connection bag is the named seam violation (those grow on the seam `MaterialPropertySet` the `Semantics/composition` egress authors), while the reinforcing root's OWN declared `SteelGrade` EXPRESS attribute is an ingest-only reader row (public on `IfcReinforcingElement`, the only carrier when a legacy export binds no material; blank drops at the `Filter`, and the Materials author never mints it); the realizing family CLOSES at the nine leaves the roster names and the attached-to-one-element `IfcVibrationDamper` is deliberately NOT one (a realizing element seats BETWEEN two elements), the nine being `IfcElement` sibling leaves so the one `Extract<IfcElement>` walk discovers each exactly once; the typed `BoltPattern`/`WeldSchedule` reconstruction relocates to the `csharp:Rasm.Fabrication` consumer exactly as the typed analysis model relocated to `Rasm.Compute`, the seam carrying the neutral typed bag alone; the egress is the `Projection/egress#IFC_EGRESS` `Emit` generic `ReauthorProperties`/`ReauthorRelationships` and a `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary is the deleted form (those Materials wires are retired, a connection element authored from the Materials/Fabrication side projecting onto the seam graph as an `Object` node + `Connect` edge the `Emit` re-authors).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Semantics;

// --- [OPERATIONS] ------------------------------------------------------------------------- The
// realizing-element connection-detail reader Projection/semantic#SEMANTIC_PROJECTOR composes: it lowers the WHOLE
// GeometryGym realizing-element family (mechanical/non-mechanical fastener, discrete accessory, reinforcing
// bar/mesh/tendon/anchor, bearing, vibration isolator) onto the seam DetailSchema.Realization conforming PropertyBag (Properties/property#DETAIL_SCHEMA — the IDENTICAL neutral
// schema the Rasm.Materials ComponentProjector AUTHORS, so this reader READS it and never hand-synces a parallel
// ConnectionDetail store or a re-spelled bag), bound to the realizing Object node. The reader is TOTAL — an unreadable
// detail yields an empty/None bag, never a fault; the element identity, entity class, and dangling-endpoint rails are the
// general projector's Fin<GraphDelta> concern, so this reader never re-cases BimFault. The joint TOPOLOGY is the
// Connect edge whose Connect.Realizing Option<NodeId> field carries the realizing node EdgeProjection authors (the medium
// closes at ConnectKind.Path/Port); this page owns only the detail. FastenerType is a row of BOTH
// DetailSchema.Realization (this reader's realizing-element fastening) and DetailSchema.Product (the Materials panel
// author's board fastening), so a consumer resolves it by the bag's SetName FIRST — a bare FastenerType lookup across
// bags reads a gypsum board's screw as a realizing bolt. MembraneSeam is a Product row the panel author emits from its
// membrane arm ALONE, so a reader keying it for any non-membrane panel resolves absence and reads that panel's own
// fastening off FastenerType instead; the two rows carry distinct facts and are never one read. The IFC
// Rasm_ConnectionRealization Pset name + the IFC predefined enums stay Bim-only — Emit maps the neutral DetailSchema.Realization.SetName onto the IFC
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
    // returns None, and Traverse drops it — so a new realizing family is ONE Detail arm, never a parallel per-type Extract list
    // to drift out of sync. The one Extract<IfcElement> walk reads every element DIRECTLY (not via the realizing relation) so
    // an element carries its fabrication detail whether or not it sits in an IfcRelConnectsWithRealizingElements (the joint
    // topology riding the separate Connect edge), discovers each element once (the realizing families are IfcElement leaves,
    // so no sibling double-count), and the general Objects fold has already minted each as the Object node this bag binds
    // against. A detail-bearing element without a rooted object faults; skipping it would strand a source entity while
    // claiming the realization bag was total. Non-realizing elements remain the successful None case.
    public static Fin<Seq<(Node Bag, Relationship Edge)>> All(
        IfcProject project,
        Map<string, NodeId> rooted,
        double tolerance,
        UnitScale scale,
        Op key) =>
        project.Extract<IfcElement>().AsIterable().ToSeq()
            .TraverseM(realizing => Bag(realizing, tolerance, scale, key).Bind(result => result.Match(
                None: static () => FinSucc(Option<(Node Bag, Relationship Edge)>.None),
                Some: bag => rooted.Find(realizing.GlobalId)
                    .ToFin(new BimFault.DanglingReference(key, $"connection-detail-root-miss:{realizing.GlobalId}"))
                    .Map(node => Some(((Node)bag, (Relationship)new Relationship.Assign(
                        node,
                        bag.Id,
                        AssignKind.PropertyDefinition)))))))
            .As()
            .Map(static details => details.Choose(identity).ToSeq());

    // The content-keyed seam PropertySet node, or None for an EMPTY Detail — the realizing-vs-not gate, since a
    // non-realizing element hits the Detail boundary _ arm (the empty schema bag) while every realizing family yields at
    // least its Joint row, so only a real connection detail lands a bag and a non-realizing element is dropped at the All
    // Choose. Detail already returns the conforming DetailSchema.Realization bag (the neutral SetName + OccurrenceWins
    // pinned by the schema), so emptiness reads its Values.
    static Fin<Option<Node.PropertySet>> Bag(IfcElement realizing, double tolerance, UnitScale scale, Op key) =>
        Detail(realizing, scale, key).Map(detail => detail.Values.IsEmpty
            ? Option<Node.PropertySet>.None
            : Some(Mint(detail, tolerance)));

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
    // so an authored bolt and an imported one content-key identically. The egress Filter drops every non-finite Measure AND
    // every blank Text token so an unset OPTIONAL IFC scalar (a NaN-default reinforcing scalar, an unprofiled fastener's
    // absent diameter, an undeclared SteelGrade "") never emits a NaN, a misleading-0 measure, or an empty token; an arm
    // therefore lists every candidate row unconditionally rather than branching per presence, and the Filter re-projects
    // through the bag record `with` so the schema-stamped SetName/precedence survive.
    public static Fin<PropertyBag> Detail(IfcElement realizing, UnitScale scale, Op key) =>
        realizing switch {
            IfcMechanicalFastener fastener => FastenerDetail(fastener, scale, key),
            IfcFastener fastener           => Rows(Joint(JointOf(fastener.PredefinedType), key), Token(DetailSchema.FastenerType, fastener.PredefinedType.ToString())),
            IfcDiscreteAccessory accessory => AccessoryDetail(accessory, key),
            IfcReinforcingBar bar          => BarDetail(bar, scale, key),
            IfcReinforcingMesh mesh        => MeshDetail(mesh, scale, key),
            IfcTendon tendon               => TendonDetail(tendon, scale, key),
            IfcTendonAnchor anchor         => Rows(Joint("Cast", key), Token(AnchorType, anchor.PredefinedType.ToString()), Token(SteelGrade, anchor.SteelGrade)),
            IfcBearing bearing             => Rows(Joint("Bearing", key), Token(BearingType, bearing.PredefinedType.ToString())),
            IfcVibrationIsolator isolator  => Rows(Joint("Bearing", key), Token(IsolatorType, isolator.PredefinedType.ToString())),
            _                              => FinSucc(DetailSchema.Realization.Bag()),
        };

    // --- [ROWS] ------------------------------------------------------------------------------- The
    // bag-row constructors so each realizing arm is a flat declarative row list rather than repeating the
    // Enumerated/Text/Measure/OfSi construction: the joint modality through the schema's JointType row, a text token, and an
    // SI measure over its Dimension. Rows folds the candidate rows into the seam DetailSchema.Realization.Bag() through
    // ValueBag.With (last-write-wins). Joint composes DetailSchema.JointType + DetailSchema.Realization.Joint(kind) (the
    // PropertyValue.Enumerated over the schema's CLOSED allowed-set the egress facet validates against — never a local
    // Enumerated re-spelling the allowed set). Measured carries the DIMENSION-only QuantityType (the dimension-only OfSi the
    // author uses) so an imported and an authored NominalDiameter content-key identically. An ASSOCIATED material's grade
    // rides the seam Material subgraph (Semantics/composition); only the reinforcing root's OWN declared SteelGrade EXPRESS
    // attribute lands here, as an ingest-only reader token.
    // Joint rails through the schema's OWN admission: DetailSchema.Realization.Joint(selected, key) returns
    // Fin<PropertyValue> because an out-of-set token rejects at PropertyValue.Of, so the closed JointType allowed-set
    // is enforced by the seam declarer rather than trusted at this call site.
    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Joint(string kind, Op key) =>
        DetailSchema.Realization.Joint(kind, key).Map(static value => Some((DetailSchema.JointType, value)));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Token(PropertyName name, string value) =>
        FinSucc(string.IsNullOrWhiteSpace(value)
            ? Option<(PropertyName, PropertyValue)>.None
            : Some((name, (PropertyValue)new PropertyValue.Text(value))));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Measured(PropertyName name, Dimension dim, double native, UnitScale scale) =>
        double.IsFinite(native)
            ? MeasureValue.OfSi(dim, native * scale.Factor(dim))
                .Map(value => Some((name, (PropertyValue)new PropertyValue.Measure(value))))
            : FinSucc(Option<(PropertyName, PropertyValue)>.None);

    static Fin<PropertyBag> Rows(params Fin<Option<(PropertyName Name, PropertyValue Value)>>[] rows) =>
        rows.ToSeq().TraverseM(identity).As()
            .Map(static values => values.Choose(identity)
                .Fold(DetailSchema.Realization.Bag(), static (bag, row) => bag.With(row.Name, row.Value)));

    // --- [FASTENER] --------------------------------------------------------------------------- A
    // bolted/welded mechanical fastener: the JointType partition + the FastenerType token + the NominalDiameter. The
    // GeometryGym-internal mNominalDiameter has no public getter, so DiameterOf recovers it through the cross-section
    // profile radius; an absent diameter reads NaN (IfNone) and drops at the egress Filter rather than a fabricated 0.
    static Fin<PropertyBag> FastenerDetail(IfcMechanicalFastener fastener, UnitScale scale, Op key) => Rows(
        Joint(JointOf(fastener.PredefinedType), key),
        Token(DetailSchema.FastenerType, fastener.PredefinedType.ToString()),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, DiameterOf(fastener).IfNone(double.NaN), scale));

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
    // (the IfcMaterialProfileSetUsage's ForProfileSet OR the bare IfcMaterialProfileSet a type-driven export associates
    // without the occurrence usage — both IfcMaterialSelect arms -> MaterialProfiles -> Profile -> IfcCircleProfileDef.
    // Radius x 2), the same chain the profile-hosted IfcMechanicalFastener(IfcProduct, IfcMaterialProfileSetUsage,
    // IfcAxis2Placement3D, double) authoring ctor binds. The finiteness guard rides the Choose so the head is the first
    // circle with a FINITE radius (a degenerate NaN-radius profile never masks a later valid one); None when no circle
    // profile binds a diameter.
    static Option<double> DiameterOf(IfcElement element) =>
        element.HasAssociations.AsIterable()
            .Choose(static rel => rel switch {
                IfcRelAssociatesMaterial { RelatingMaterial: IfcMaterialProfileSetUsage { ForProfileSet: { } set } } => Some(set),
                IfcRelAssociatesMaterial { RelatingMaterial: IfcMaterialProfileSet set } => Some(set),
                _ => None,
            })
            .SelectMany(static set => set.MaterialProfiles.AsIterable())
            .Choose(static profile => profile.Profile is IfcCircleProfileDef { Radius: var radius } && double.IsFinite(radius) ? Some(radius * 2d) : None)
            .Head;

    // --- [ACCESSORY] -------------------------------------------------------------------------- A
    // fabricated framing connector (IfcDiscreteAccessory, the IfcElementComponent sibling of IfcMechanicalFastener):
    // a steel saddle/bracket/anchorplate that physically IS the connector body, FASTENED BY separate IfcMechanicalFasteners.
    // This is the IMPORT counterpart of the Rasm.Materials Component/connector#CONNECTOR_FAMILY seed — the ConnectorDetail
    // seed-time bag denormalizes BOTH tokens onto the one connector Component (ComponentProjector-projected): AccessoryType =
    // the connector's own IfcDiscreteAccessoryTypeEnum SHOE/BRACKET/ANCHORPLATE, FastenerType = the SEPARATE attaching token
    // the row's ConnectorInstall FastenerSpec.IfcFastenerType carries (NAIL/SCREW/BOLT) — so the Bim reader matches that SHAPE by
    // reading the AccessoryType off the connector's own PredefinedType AND recovering the FastenerType set from the
    // co-realizing IfcMechanicalFastener siblings reached through the IfcElement.IsConnectionRealization back-pointer (the
    // IfcRelConnectsWithRealizingElements sets both the accessory and its attaching fasteners join). ONE co-realizing
    // fastener kind lands the SAME Text token the authored bag mints (byte-identical content key); a MULTI-kind attachment
    // (nailplate + screw) lands the ONE FastenerType row as the typed PropertyValue.List — the ingest-only richer bag that
    // is faithfully a DIFFERENT content-keyed node, never a .Head slice dropping the second fastener and never a joined
    // literal. JointType is Bolted (a fabricated connector is mechanically attached, mirroring the Materials connector-seed
    // Joint("Bolted")); an accessory with NO co-realizing mechanical fastener drops the row — never a fabricated token.
    static Fin<PropertyBag> AccessoryDetail(IfcDiscreteAccessory accessory, Op key) {
        Seq<string> fasteners = FastenerOf(accessory);
        return Rows(Joint("Bolted", key), Token(DetailSchema.AccessoryType, accessory.PredefinedType.ToString())).Map(detail => fasteners switch {
            { IsEmpty: true } => detail,
            { Count: 1 }      => detail.With(DetailSchema.FastenerType, new PropertyValue.Text(fasteners[0])),
            _                 => detail.With(DetailSchema.FastenerType, new PropertyValue.List(fasteners.Map(static t => (PropertyValue)new PropertyValue.Text(t)))),
        });
    }

    // The attaching-fastener token-set recovery: the connector and its mechanical fasteners co-occupy
    // IfcRelConnectsWithRealizingElements sets (the connector's IsConnectionRealization back-pointer), so the sibling
    // IfcMechanicalFastener PredefinedTypes are the attaching FastenerType vocabulary the Materials connector-seed bag denormalized
    // — one bounded hop reading only PredefinedType (the joint TOPOLOGY rides the EdgeProjection Connect edge whose
    // Connect.Realizing field carries the realizing node, never re-derived here). ALL co-realizing fastener tokens fold —
    // distinct then ORDINAL-sorted so duplicates collapse and an IFC file's SET order never forks the content key.
    static Seq<string> FastenerOf(IfcDiscreteAccessory accessory) =>
        toSeq(accessory.IsConnectionRealization.AsIterable()
            .SelectMany(static rel => rel.RealizingElements.AsIterable())
            .Choose(static realizing => realizing is IfcMechanicalFastener fastener ? Some(fastener.PredefinedType.ToString()) : None)
            .Distinct().OrderBy(static token => token, StringComparer.Ordinal));

    // --- [REINFORCING] ------------------------------------------------------------------------ A
    // cast-in reinforcing bar: the public NominalDiameter (IfcReinforcingBarType.NominalDiameter type-fallback get) /
    // CrossSectionArea / BarLength scalars over their SI dimensions + the BarType (STUD is the cast-in bar, NOT the welded
    // connector) and BarSurface tokens + the reinforcing root's declared SteelGrade designation (public on
    // IfcReinforcingElement — the deprecated-but-live EXPRESS attribute a legacy/Revit export carries where no material
    // association exists; blank drops at the Filter); the NaN defaults drop at the Detail egress Filter.
    static Fin<PropertyBag> BarDetail(IfcReinforcingBar bar, UnitScale scale, Op key) => Rows(
        Joint("Cast", key),
        Token(DetailSchema.BarType, bar.PredefinedType.ToString()),
        Token(BarSurface, bar.BarSurface.ToString()),
        Token(SteelGrade, bar.SteelGrade),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, bar.NominalDiameter, scale),
        Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, bar.CrossSectionArea, scale),
        Measured(BarLength, Dimension.LengthDim, bar.BarLength, scale));

    // A cast-in reinforcing mesh: the full public sheet geometry — overall length/width, the longitudinal/transverse bar
    // diameters, spacings, and cross-section areas — + the MeshType and SteelGrade tokens, the welded-mesh fabrication
    // detail a single-bar reader drops; every unset NaN component drops at the Detail egress Filter.
    static Fin<PropertyBag> MeshDetail(IfcReinforcingMesh mesh, UnitScale scale, Op key) => Rows(
        Joint("Cast", key),
        Token(MeshType, mesh.PredefinedType.ToString()),
        Token(SteelGrade, mesh.SteelGrade),
        Measured(MeshLength, Dimension.LengthDim, mesh.MeshLength, scale),
        Measured(MeshWidth, Dimension.LengthDim, mesh.MeshWidth, scale),
        Measured(LongitudinalBarNominalDiameter, Dimension.LengthDim, mesh.LongitudinalBarNominalDiameter, scale),
        Measured(TransverseBarNominalDiameter, Dimension.LengthDim, mesh.TransverseBarNominalDiameter, scale),
        Measured(LongitudinalBarSpacing, Dimension.LengthDim, mesh.LongitudinalBarSpacing, scale),
        Measured(TransverseBarSpacing, Dimension.LengthDim, mesh.TransverseBarSpacing, scale),
        Measured(LongitudinalBarCrossSectionArea, Dimension.AreaDim, mesh.LongitudinalBarCrossSectionArea, scale),
        Measured(TransverseBarCrossSectionArea, Dimension.AreaDim, mesh.TransverseBarCrossSectionArea, scale));

    // A post-tensioning tendon (IfcReinforcingElement, the cast-in prestressing peer of the bar/mesh): its native
    // NominalDiameter/CrossSectionArea/TensionForce are GeometryGym-internal with no public getter, so the bag carries the
    // TendonType + SteelGrade tokens + the diameter recovered through the profile channel (NaN-filtered when unprofiled);
    // the material-lane grade of an ASSOCIATED material still rides the seam Material subgraph (Semantics/composition).
    static Fin<PropertyBag> TendonDetail(IfcTendon tendon, UnitScale scale, Op key) => Rows(
        Joint("Cast", key),
        Token(TendonType, tendon.PredefinedType.ToString()),
        Token(SteelGrade, tendon.SteelGrade),
        Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, DiameterOf(tendon).IfNone(double.NaN), scale));

    // --- [READER_ROWS] ------------------------------------------------------------------------ The
    // realizing-detail row names the IMPORT reader recovers BEYOND the canonical DetailSchema vocabulary (the author
    // writes the twelve canonical discrete-part diameter/throat/lap schema statics; this reader composes the
    // import-recoverable subset — JointType/FastenerType/AccessoryType/BarType/NominalDiameter/CrossSectionArea — the
    // authored-only throat/lap/carried-member rows having no public GG read channel; panel/deck/membrane product rows
    // are DetailSchema.Product's and the general property fold's). Every row below mints through
    // PropertyCategory.Seam.Row — the owner-blessed EMPTY-prefix producer category, so custody routes through the seam
    // declarer while the round-tripped IFC property name stays bare — and a call-site PropertyName.Create here is the
    // deleted form that forks the bag's key space between non-referencing packages. PropertyName itself stays an OPEN
    // key per Properties/property#DETAIL_SCHEMA, so an ingest-only scalar the Materials author never mints — a mesh
    // sheet's geometry, a tendon/anchor/bearing/isolator type token, a bar's surface + overall length, the reinforcing
    // root's declared SteelGrade designation (the deprecated-but-live IfcReinforcingElement EXPRESS attribute, public
    // get/set — the ONLY grade carrier when a legacy export binds no material) — lands here, and the moment a second
    // package keys on one it is PROMOTED to a DetailSchema static at the Rasm.Element owner rather than staying here.
    // An authored bag carrying only the schema rows and an imported bag carrying these extra rows are faithfully DIFFERENT
    // content-keyed nodes, never a forced byte-match; these never widen the seam DetailSchema (a Materials-authored row is a
    // schema static, a Bim-ingest-only row is here), and the grade/carbon data of an ASSOCIATED material still rides the
    // seam Material subgraph, never here.
    static readonly PropertyName AnchorType = PropertyCategory.Seam.Row("AnchorType");
    static readonly PropertyName BearingType = PropertyCategory.Seam.Row("BearingType");
    static readonly PropertyName IsolatorType = PropertyCategory.Seam.Row("IsolatorType");
    static readonly PropertyName TendonType = PropertyCategory.Seam.Row("TendonType");
    static readonly PropertyName BarSurface = PropertyCategory.Seam.Row("BarSurface");
    static readonly PropertyName SteelGrade = PropertyCategory.Seam.Row("SteelGrade");
    static readonly PropertyName BarLength = PropertyCategory.Seam.Row("BarLength");
    static readonly PropertyName MeshType = PropertyCategory.Seam.Row("MeshType");
    static readonly PropertyName MeshLength = PropertyCategory.Seam.Row("MeshLength");
    static readonly PropertyName MeshWidth = PropertyCategory.Seam.Row("MeshWidth");
    static readonly PropertyName LongitudinalBarNominalDiameter = PropertyCategory.Seam.Row("LongitudinalBarNominalDiameter");
    static readonly PropertyName TransverseBarNominalDiameter = PropertyCategory.Seam.Row("TransverseBarNominalDiameter");
    static readonly PropertyName LongitudinalBarSpacing = PropertyCategory.Seam.Row("LongitudinalBarSpacing");
    static readonly PropertyName TransverseBarSpacing = PropertyCategory.Seam.Row("TransverseBarSpacing");
    static readonly PropertyName LongitudinalBarCrossSectionArea = PropertyCategory.Seam.Row("LongitudinalBarCrossSectionArea");
    static readonly PropertyName TransverseBarCrossSectionArea = PropertyCategory.Seam.Row("TransverseBarCrossSectionArea");
}
```

## [03]-[RESEARCH]

(none)

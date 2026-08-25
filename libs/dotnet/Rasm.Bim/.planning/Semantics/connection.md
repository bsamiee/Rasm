# [BIM_CONNECTION]

`ConnectionProjection` is the realizing-element connection-detail reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: it lowers the WHOLE GeometryGym realizing-element surface — the bolted/welded `IfcMechanicalFastener` and the bonded/welded `IfcFastener`, the fabricated `IfcDiscreteAccessory` framing connector, the cast-in `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcTendonConduit`, the support `IfcBearing`, the isolation `IfcVibrationIsolator`, and the realizing `IfcElementAssembly` composite — onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the ONE neutral realizing-detail schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` AUTHORS — this reader READS the same schema, never a hand-synced parallel bag), bound to the realizing element's `Graph/element#NODE_MODEL` `Object` node through one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, so an authored realizing element and a re-imported IFC one content-key to one `Node.PropertySet` and a downstream detailing consumer reads the bolt diameter, the stud-shear connector, the framing-connector accessory token, the reinforcing cover, the post-tensioning tendon, and the bearing type off the ONE `Graph/element#ELEMENT_GRAPH` `Bake`-derived element it already holds — never a second store. Physical joint TOPOLOGY — which two members meet through which realizing element — rides the `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing node on the `Connect.Realizing` `Option<NodeId>` field (realizing-ness the field, never a `ConnectKind` row — the medium closes at `Element`/`Path`/`Port`) the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors from `IfcRelConnectsWithRealizingElements` (fanning one edge per `RealizingElements` member into the seam `Connect.Realizing` option, so a multi-realizer joint keeps every realizer); this page owns ONLY the realizing element's fabrication DETAIL the general `Object` fold does not read — the native reinforcing scalars, the declared `SteelGrade` designation, and the fastener nominal diameter the internal GeometryGym scalar hides on BOTH the occurrence and its type.

This RETIRES the parallel `ConnectionDetail` record + `ConnectionRealization` `[Union]` + `BoltPattern`/`WeldSchedule`/`BearingSurface` `[ComplexValueObject]` + `ConnectionKind`/`Clearance` family keyed by `BimModel`/`GlobalId` AND its hand-rolled `(GeometryKey, DetailKey)` second content-key — the "second stored record off the element" the rebuild forbids, mirroring `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection` retiring the `MemberConnection`/`SupportRestraint` typed store — AND the `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary, the deleted form mirroring `Semantics/composition#MATERIAL_COMPOSITION` retiring the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers. Joint modality rides the realizing `Object.PredefinedType` token and the bag's `JointType` enumerated, the bolt/weld/bearing/cast detail is typed `Properties/property#PROPERTY_VALUE` `PropertyValue` entries, and the IFC egress is the `Projection/egress#IFC_EGRESS` `Emit` generic round-trip — the realizing `IfcMechanicalFastener`/`IfcReinforcingBar` re-authored as an `Object` node, the detail bag as an `IfcPropertySet` through `ReauthorProperties`, the joint as `IfcRelConnectsWithRealizingElements` through `ReauthorRelationships` — never a connection-specific writer. `ConnectionProjection` stays HOST-NEUTRAL (it binds the realizing geometry by the `Graph/element#NODE_MODEL` `RepresentationContentHash` content key, never re-tessellating the fastener) and detail-TOTAL (a realizing element with no readable detail yields no bag — the `Fin` rail carries only the seam `MeasureValue.OfSi` admission, the schema's own `JointType` allowed-set admission, and the `All` root-resolution, so a detail-bearing element without a rooted `Object` faults `BimFault.Refused` with `BimReason.DanglingReference` `connection-detail-root-miss` rather than stranding a source entity; the joint edge's own endpoint rail stays the `EdgeProjection` `edge-endpoint-miss`), a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection`, `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection`, and `Semantics/georeference#GEO_PROJECTION` `GeoReferenceProjector` — so a connection enrichment never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [02]-[CONNECTION_DETAIL]: `ConnectionProjection` the GeometryGym realizing-element-detail reader — `BagOf` the ONE polymorphic attribute-bag reader over the `Realizing` `FrozenDictionary<Type, RealizingRow>` row table (`IfcMechanicalFastener` bolt/weld, `IfcFastener` glue/mortar/weld, `IfcDiscreteAccessory` framing connector, `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcTendonConduit` cast, `IfcBearing` support, `IfcVibrationIsolator` isolation, `IfcElementAssembly` the realizing composite whose modality derives from its parts) onto the seam `DetailSchema.Realization` conforming `PropertyBag`, answering `None` for a non-realizing element so the realizing gate and the bag construction are one read, the `JointModality` token roster with its `Ranked` precedence order, the `RealizingRow` three-column shape and its `Row<T>`/`Token`/`Tokens`/`Measure` column mints, the `AssemblyJoint` composite derivation, the accumulating `Joint`/`Lift`/`Measured`/`Rows` row-fold every row composes, the `MeshBars` axis×column cross-product generator, `DiameterOf` the fastener/tendon nominal-diameter recovery through the associated `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section, `FastenerOf` the co-realizing attaching-fastener token-set recovery off the `IfcElement.IsConnectionRealization` back-pointer, `Mint` the content-keyed seam `PropertySet` node, and `All` the accumulating fold over `Extract<IfcElement>` producing the `(bag node, Assign.PropertyDefinition edge)` pairs `Project` composes onto the `ElementGraph`.

## [02]-[CONNECTION_DETAIL]

- Owner: `ConnectionProjection` the static realizing-element-detail reader `SemanticProjector` composes, lowering the WHOLE GeometryGym realizing-element surface onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node — never a stored record and never a hand-synced bag, the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors so an authored realizing element and a re-imported one are one content-keyed `Node.PropertySet`. It owns the polymorphic `BagOf` attribute-bag reader (one entry resolving the realizing surface through the `Realizing` type-keyed row table — mechanical/non-mechanical fastener, fabricated discrete accessory, reinforcing bar/mesh/tendon/anchor/conduit, support bearing, isolation vibration isolator, realizing element assembly — onto its bolt/weld/bonded/accessory/cast/bearing bag through the `Joint`/`Lift`/`Measured`/`Rows` row-fold over `DetailSchema.Realization.Bag()`, and onto `None` for an unclaimed type or an assembly whose parts declare no modality), the `JointModality` roster naming the five seam-declared joint tokens ONCE, the `MeshBars` cross-product generator, the `DiameterOf` cross-section diameter recovery, the `FastenerOf` co-realizing attaching-fastener token-SET recovery (ALL co-realizing `IfcMechanicalFastener` tokens fold — distinct, ordinal-sorted — never a `.Head` slice), the `Mint` of the seam `Node.PropertySet`, and the `All` model fold over `Extract<IfcElement>`; the typed structures a parallel store would mint (`ConnectionDetail`, the `ConnectionRealization` `[Union]`, `BoltPattern`/`WeldSchedule`/`BearingSurface`, `ConnectionKind`, `Clearance`) are all GONE — the realizing element is the seam `Object` node the general `Objects` fold mints, the joint modality its `PredefinedType` token and the bag `JointType` row, the joint topology the neutral `Connect` edge carrying the realizing node on its `Connect.Realizing` field, and the fabrication scalars the typed `PropertyValue` bag entries.
- Entry: `ConnectionProjection.BagOf(IfcElement realizing, UnitScheme scale, Op key)` is the ONE polymorphic attribute-bag reader discriminating on the realizing-element shape, `Fin<Option<PropertyBag>>` carrying the seam `MeasureValue.OfSi` admission (`scale` the per-projection native→SI coercion every measured row crosses) and the schema's own `DetailSchema.Realization.Joint(selected, key)` allowed-set admission, so an out-of-set joint token rejects at the seam declarer rather than passing this call site unchecked — an `IfcMechanicalFastener` onto the bolt/weld bag (the `JointType`/`FastenerType` tokens and the `NominalDiameter` recovered from the cross-section profile), an `IfcFastener` onto the bonded/weld bag (the `JointType`/`FastenerType` tokens), an `IfcDiscreteAccessory` onto the framing-connector bag (the `JointType`/`AccessoryType` tokens and the `FastenerType` token set of ALL co-realizing attaching `IfcMechanicalFastener`s — a single fastener lands the two-token `Text` shape the `Rasm.Materials` `Component/connector#CONNECTOR_FAMILY` `ConnectorDetail` seed bag authors, content-keying byte-identically; a multi-fastener attachment lands the typed `PropertyValue.List` of distinct ordinal-sorted tokens the seam value family carries natively), an `IfcReinforcingBar` onto the cast bag (the `BarType`/`BarSurface`/`SteelGrade` tokens and the native `NominalDiameter`/`CrossSectionArea`/`BarLength` scalars), an `IfcReinforcingMesh` onto the mesh bag (the `MeshType`/`SteelGrade` tokens, the native mesh length/width, and the `MeshBars` axis×column rows), an `IfcTendon` onto the cast bag (the `TendonType`/`SteelGrade` tokens and the profile-recovered `NominalDiameter`), an `IfcTendonAnchor`/`IfcTendonConduit`/`IfcBearing`/`IfcVibrationIsolator` onto its type-token bag, an `IfcElementAssembly` onto the composite bag whose modality derives from its realizing parts, and any other (or null) onto `None` — the realizing-vs-not gate IS the reader's own answer, so no bag is constructed for the element the fold then discards; detail ABSENCE never faults (the entity class is the general fold's `BimFault.Refused` with `BimReason.Unmapped`), and there is no `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family and no per-family `FastenerDetail`/`AccessoryDetail`/`BarDetail`/`MeshDetail`/`TendonDetail` method — one polymorphic reader over one row table whose columns carry every family's joint derivation, token readers, and measured projectors; `ConnectionProjection.All(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScheme scale, Op key)` folds every `IfcElement` the project carries — the `Realizing` table lookup the SOLE discriminator of the realizing families, a non-realizing element answering `None` and so yielding no bag — into the `Seq<(Node Bag, Relationship Edge)>` the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` concats onto its node and edge sets (exactly as it concats `Materials` and `EdgeProjection.All`), so a new realizing family is one `Realizing` row and never a parallel extract list to drift.
- Auto: each row folds onto the seam `DetailSchema.Realization.Bag()` through the `Joint`/`Token`/`Measured`/`Rows` row-fold — one `Joint` schema modality row, the type tokens, and the SI `Measured` rows — so a family is a flat declarative row list, never a repeated `MeasureValue.OfSi` construction and never a hand-spelled set-name, precedence, or joint literal (the set name and precedence ride the schema, the five joint tokens the `JointModality` roster). Rows of one bag are INDEPENDENT, so `Rows` inverts them through `Validation<Error, T>`/`Landed` on the `Validation<Error, _>` algebra and a malformed realizing element reports EVERY offending column rather than the first — `Joint` and `Measured` carry the only two admissions on the lane and `Token` is TOTAL, riding the pure lift so the row shape stays uniform; `All` takes the SAME crossing per element, so one unrooted realizing element never hides the next. `BagOf` reads the realizing element's NATIVE fabrication scalars the general `Object` fold leaves on the geometry — the reinforcing bar/mesh expose their `NominalDiameter`/`CrossSectionArea`/`BarLength`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea` as public doubles in the model's DECLARED units (GeometryGym pre-coerces nothing), so every one crosses the ONE per-projection `UnitScheme.Coerce` entry over its own `Dimension` before admitting through the DIMENSION-only `MeasureValue.OfSi(Dimension.LengthDim/AreaDim, ...)` overload — the same dimension-only admit the Materials author takes — the seam bag law's two-peer carve, because that author seeds its rows off a catalogue and holds no foreign measure type to name, so a name this importing side alone spells forks the content key the pair exists to share — and an authored and an imported row therefore content-key identically; the `JointType` enumerated derives from the realizing family (an `IfcMechanicalFastener` `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` or an `IfcFastener` `WELD` is `Welded`, every other discrete mechanical fastener `Bolted`, an `IfcFastener` `GLUE`/`MORTAR` `Bonded`, an `IfcBearing` and the isolation-bearing `IfcVibrationIsolator` `Bearing`, a reinforcing bar/mesh/tendon/anchor/conduit `Cast`, a realizing assembly the `Ranked` modality of its parts) through `DetailSchema.Realization.Joint(kind)`, the schema's closed `JointTypes` allowed set the egress facet validates against; the reinforcing root's public `IfcReinforcingElement.SteelGrade` lands as the ingest-only `SteelGrade` token (the superseded-but-live EXPRESS designation, the only grade carrier when an older export binds no material); the mechanical-fastener AND tendon `NominalDiameter` is the special case — `mNominalDiameter`/`mNominalLength` are GeometryGym-internal on both with NO public getter, so `DiameterOf` recovers the diameter through the inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` → `ForProfileSet.MaterialProfiles` → `Profile` → `IfcCircleProfileDef.Radius` × 2), the documented public round-trip channel, yielding `Option<double>.None` (read `IfNone(NaN)` and dropped at the `Filter`, never a fabricated 0) when no circle profile binds; every non-finite scalar (an unset GeometryGym `NaN` default) and every blank token (an undeclared `SteelGrade` `""`) is dropped at the row egress so a partially-specified realizing element never emits a misleading measure or an empty text row; `Mint` builds the seam `Node.PropertySet` whose id is `NodeId.Of(new NodeSeed.Content(node, tolerance))` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical realizing details dedup to one node, and `All` resolves each realizing element's rooted `NodeId` through the `rooted` map and binds the bag through an `Assign.PropertyDefinition` edge — a detail-bearing element without a rooted `Object` faults `BimFault.Refused` with `BimReason.DanglingReference` `connection-detail-root-miss` (skipping it strands a source entity while claiming the realization fold was total; the joint edge's own endpoint rail stays `EdgeProjection`'s `edge-endpoint-miss`).
- Output: the connection-detail bag lands on the ONE seam `ElementGraph` as a `PropertySet` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold merges into `element.Properties` through the realizing element's `Assign.PropertyDefinition` edge, so a downstream detailing consumer reads `element.Properties.Find(b => b.SetName == DetailSchema.Realization.SetName).Bind(b => b.Find(DetailSchema.NominalDiameter))` for the bolt diameter / weld stud / reinforcing cover off the baked realizing element (the NEUTRAL `SetName`, never an IFC literal — the `Rasm_ConnectionRealization` Pset name is applied only at the `Projection/egress#IFC_EGRESS` mapping), and the joint topology off the `Connect` edge whose `Connect.Realizing` field carries the realizing node the `EdgeProjection` authors — a steel bolted moment connection's fasteners, a stud-shear-connector deck weld, and a cast-in reinforcing lap each carrying their physical detail on the one graph the consumer already holds, never a parallel connection store and never a second member-selection surface; the `Projection/egress#IFC_EGRESS` `Emit` re-authors the bag (`IfcPropertySet` through `ReauthorProperties`) and the joint (`IfcRelConnectsWithRealizingElements` through `ReauthorRelationships`) generically, so the connection round-trips with the rest of the graph.
- Packages: GeometryGymIFC_Core (the realizing-element surface consumed as settled vocabulary), Rasm.Element (the seam `DetailSchema` + the `Node`/`NodeId`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`Relationship`/`AssignKind` payloads, the schema carrying the `SetName`/`OccurrenceWins`/`JointTypes` so this reader hand-spells none), Rasm (the `Op` key and the `UnitScheme` coercion entry this reader threads), LanguageExt.Core (`Option`/`Seq`/`Map`/`Validation`).
- Growth: a new realizing-element family is one `Realizing` row (its joint derivation, its token columns, and its measured columns, a hidden scalar composing `DiameterOf` inside its own projector); a new fabrication scalar is one `Measure` column on its row carrying its `MeasureValue` over the composed `Dimension` (a row in the canonical `DetailSchema` vocabulary composes the schema static, an ingest-only scalar one `PropertyCategory.Seam.Row` mint in the `[READER_ROWS]` block); a new mesh column is one `MeshBars` column row and a new bar axis one axis row, the cross-product minting both the `PropertyName`s and the readers; a new joint modality is one token on the seam `DetailSchema.Realization.JointTypes` allowed set and one `JointModality` row seated at its rank in `Ranked` and its `JointType` derivation, never a reader-local allowed set and never a bare literal at a derivation site; never a per-joint-type connection record, never a `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family, never a second connection store, never a hand-synced parallel detail bag, never a `(GeometryKey, DetailKey)` parallel content key, and never a re-tessellation of the realizing element.
- Boundary: the connection detail is the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node, COMPOSED through `DetailSchema.Realization.Bag()`/`.Joint(kind)` (the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors) — a hand-synced parallel bag re-spelling the set name, the `OccurrenceWins` precedence, or the `JointTypes` allowed set is the deleted form (the reader READS the seam-declared schema, never a copy), and a typed `ConnectionDetail`/`ConnectionRealization`/`BoltPattern`/`WeldSchedule`/`BearingSurface`/`ConnectionKind`/`Clearance` second-store record family is the deleted form (mirroring `StructuralProjection` retiring `MemberConnection`/`SupportRestraint`) — the realizing element is the seam `Object` node, its detail the schema bag the `Bake` fold reads flat; the in-graph bag carries the NEUTRAL `SetName` and the IFC `Rasm_ConnectionRealization` Pset name is applied ONLY at the `Projection/egress#IFC_EGRESS` `Emit` mapping, so a `Rasm_ConnectionRealization` literal as the in-graph set name is the deleted form; the five joint modality tokens are the `JointModality` roster naming the seam allowed-set members ONCE and a bare `"Cast"`/`"Bolted"`/`"Welded"`/`"Bonded"`/`"Bearing"` literal at a derivation site is the deleted form that forks the vocabulary the seam declares; the canonical realizing-detail rows compose the `DetailSchema` `PropertyName` statics while an INGEST-ONLY scalar the author never mints (a mesh sheet's geometry, a tendon/bearing/isolator token, a bar's surface + overall length) is a reader-side `[READER_ROWS]` row minted through the owner-blessed `PropertyCategory.Seam.Row` category — the anchor token already promoted to the seam `DetailSchema.AnchorType` static this reader composes — a call-site `PropertyName.Create` in this reader is the deleted form and a row a second package begins keying on is promoted to a `DetailSchema` static at the seam owner — an authored bag and a richer imported bag are faithfully DIFFERENT content-keyed nodes, never a forced byte-match, and a reader-side row never widens the seam `DetailSchema`; a row family that is a CROSS-PRODUCT of an axis and a column set (the mesh's two bar axes × three measured columns) is the `MeshBars` generator and six hand-spelled statics beside six hand-spelled `Measured` lines is the deleted form; the `BimModel`/`BimElement` join (`federated.Elements`, the `(MemberGlobalId, MemberGlobalId)` pair, the `BindFederated` dangling-reference rail) is GONE with the retired element records, the joint endpoints being the `Connect` edge's `NodeId` pair the `EdgeProjection` resolves and the analytical member↔connection topology the `Model/structural#STRUCTURAL_PROJECTION` `IfcRelConnectsStructuralMember` `Generic` edge, both meeting on the SHARED graph nodes, never a `GlobalId`-pair selection surface; the detail-bag attachment is ONE polymorphic `BagOf` over the `Realizing` row table keyed on the EXACT runtime type (the realizing families are `IfcElement` leaves with none a supertype of another, so exact keying fails SAFE where an `is` ladder silently details a future subtype against its parent's columns) and a `RealizationOf`/`BoltOf`/`WeldOf`/`LapOf`/`TendonOf`/`BearingOf` sibling-method family is the deleted form; the realizing-vs-not gate is `BagOf`'s OWN `Option` answer and a bag minted for a non-realizing element then discarded on an emptiness test is the deleted form (the schema bag is constructed once, for an element that carries a detail); detail ABSENCE never faults — an unreadable detail is `None`, and routing a missing scalar or token onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the entity-class rail is the general fold's `Fin<GraphDelta>`) — while the fold's own rail is real and ACCUMULATING: the independent rows of one bag and the independent elements of one model both cross `Validation<Error, T>` and collapse once through `Error.Many`, so a first-defect `TraverseM` that hides every offending column after the first is the deleted form; every native scalar crosses the ONE `UnitScheme.Coerce` entry the projection threads and a raw double admitted as already-SI is the mm-vs-metre import trap `Semantics/composition#MATERIAL_COMPOSITION` names; the connection detail stays host-neutral scalar data and a RhinoCommon `Brep`/`Mesh` realizing-element field or an in-process fastener tessellation is the named seam violation, the realizing geometry binding by the `RepresentationContentHash` content key; the GeometryGym realizing surface (`IfcMechanicalFastener.PredefinedType` `IfcMechanicalFastenerTypeEnum` and `IfcFastener.PredefinedType` `IfcFastenerTypeEnum`, the `IfcDiscreteAccessory.PredefinedType` `IfcDiscreteAccessoryTypeEnum` and the `IfcElement.IsConnectionRealization` `SET<IfcRelConnectsWithRealizingElements>` back-pointer to the co-realizing attaching `IfcMechanicalFastener` SET — ALL tokens fold distinct and ordinal-sorted so an IFC file's set order never forks the content key, a single token the authored `Text` shape and a multi-token set the typed `PropertyValue.List`, never a `.Head` slice dropping a nailplate+screw second fastener and never a joined literal, the public `IfcReinforcingBar.NominalDiameter` (`IfcReinforcingBarType.NominalDiameter` type-fallback)/`CrossSectionArea`/`BarLength`/`PredefinedType`/`BarSurface`, the public `IfcReinforcingMesh.PredefinedType`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea`, the `IfcTendon.PredefinedType` `IfcTendonTypeEnum` / `IfcTendonAnchor.PredefinedType` `IfcTendonAnchorTypeEnum` / `IfcBearing.PredefinedType` `IfcBearingTypeEnum` / `IfcVibrationIsolator.PredefinedType` `IfcVibrationIsolatorTypeEnum`, the public `IfcReinforcingElement.SteelGrade` designation, the `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` or bare `IfcMaterialProfileSet`) → `IfcCircleProfileDef.Radius` chain) is consumed as settled vocabulary (`.api/api-geometrygym-ifc`) and a hand-rolled realizing reader is the deleted form; the mechanical-fastener and tendon nominal diameter rides the associated circle-profile radius (the public channel for the internal `mNominalDiameter`) and a fabricated `0` diameter on an unprofiled element is the deleted form (the entry reads `NaN` and is dropped at the row egress); the realizing element's CLASSIFICATION and MATERIAL ride the general `Object`/`Associate` folds, not this bag — an ASSOCIATED material's grade or embodied-carbon column on the connection bag is the named seam violation (those grow on the seam `MaterialPropertySet` the `Semantics/composition` egress authors), while the reinforcing root's OWN declared `SteelGrade` EXPRESS attribute is an ingest-only reader row (public on `IfcReinforcingElement`, the only carrier when an older export binds no material; blank drops at the row egress, and the Materials author never mints it); the realizing family CLOSES at the leaves the `Realizing` table names and the attached-to-one-element `IfcVibrationDamper` is deliberately NOT one (a realizing element seats BETWEEN two elements, and the `.api/api-geometrygym-ifc` catalog publishes no damper surface to read), each being an `IfcElement` leaf so the one `Extract<IfcElement>` walk discovers it exactly once; DETAIL coverage is keyed on the ELEMENT and not on the relation, so a realizing element carries its detail whichever `IfcRelConnects` family realizes it — the relation family's own coverage is `Projection/relations#RELATION_ALGEBRA`'s, where `Connect.Realizing` is authored from `IfcRelConnectsWithRealizingElements` alone and the `IfcRelConnectsPorts.RealizingElement` singular carrier is an OPEN obligation at that owner, unread today; the typed `BoltPattern`/`WeldSchedule` reconstruction relocates to the `Rasm.Fabrication` consumer exactly as the typed analysis model relocated to `Rasm.Compute`, the seam carrying the neutral typed bag alone; the egress is the `Projection/egress#IFC_EGRESS` `Emit` generic `ReauthorProperties`/`ReauthorRelationships` and a `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary is the deleted form (those Materials wires are retired, a connection element authored from the Materials/Fabrication side projecting onto the seam graph as an `Object` node + `Connect` edge the `Emit` re-authors).

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim;
using Rasm.Bim.Model;
using Rasm.Bim.Projection;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Semantics;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ConnectionProjection {
    // --- [JOINT_MODALITY]
    static readonly string Bolted = nameof(Bolted);
    static readonly string Welded = nameof(Welded);
    static readonly string Bonded = nameof(Bonded);
    static readonly string Bearing = nameof(Bearing);
    static readonly string Cast = nameof(Cast);

    static readonly Seq<string> Ranked = Seq(Welded, Cast, Bolted, Bonded, Bearing);

    public static Fin<Seq<(Node Bag, Relationship Edge)>> All(
        IfcProject project,
        Map<string, NodeId> rooted,
        double tolerance,
        UnitScheme scale,
        Op key) =>
        (project.Extract<IfcElement>().AsIterable().ToSeq()
            .Traverse(realizing => (BagOf(realizing, scale, key).Bind(detail => detail.Match(
                None: static () => Fin.Succ(Option<(Node Bag, Relationship Edge)>.None),
                Some: bag => rooted.Find(realizing.GlobalId)
                    .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.DanglingReference, string.Join(':', new object?[] { "connection-detail-root-miss", realizing.GlobalId })))
                    .Map(node => {
                        Node.PropertySet minted = Mint(bag, tolerance);
                        return Some(((Node)minted, (Relationship)new Relationship.Assign(node, minted.Id, AssignKind.PropertyDefinition)));
                    })))).ToValidation())
            .As()
            .Map(static details => details.Choose(identity).ToSeq())).ToFin();

    static Node.PropertySet Mint(PropertyBag bag, double tolerance) {
        var draft = new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), bag);
        return (Node.PropertySet)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tolerance)));
    }

    // --- [REALIZING_TABLE]
    static readonly Lazy<FrozenDictionary<Type, RealizingRow>> Realizing = new(static () =>
        new Dictionary<Type, RealizingRow> {
            [typeof(IfcMechanicalFastener)] = Row<IfcMechanicalFastener>(
                static f => Some(f.PredefinedType is IfcMechanicalFastenerTypeEnum.STUDSHEARCONNECTOR or IfcMechanicalFastenerTypeEnum.SHEARCONNECTOR ? Welded : Bolted),
                Seq(Token<IfcMechanicalFastener>(DetailSchema.FastenerType, static f => f.PredefinedType.ToString())),
                Seq(Measure<IfcMechanicalFastener>(DetailSchema.NominalDiameter, Dimension.LengthDim, static f => DiameterOf(f).IfNone(double.NaN)))),
            [typeof(IfcFastener)] = Row<IfcFastener>(
                static f => Some(f.PredefinedType is IfcFastenerTypeEnum.WELD ? Welded : Bonded),
                Seq(Token<IfcFastener>(DetailSchema.FastenerType, static f => f.PredefinedType.ToString())),
                Empty<IfcFastener>()),
            [typeof(IfcDiscreteAccessory)] = Row<IfcDiscreteAccessory>(
                static _ => Some(Bolted),
                Seq(Token<IfcDiscreteAccessory>(DetailSchema.AccessoryType, static a => a.PredefinedType.ToString()),
                    Tokens<IfcDiscreteAccessory>(DetailSchema.FastenerType, FastenerOf)),
                Empty<IfcDiscreteAccessory>()),
            [typeof(IfcReinforcingBar)] = Row<IfcReinforcingBar>(
                static _ => Some(Cast),
                Seq(Token<IfcReinforcingBar>(DetailSchema.BarType, static b => b.PredefinedType.ToString()),
                    Token<IfcReinforcingBar>(BarSurface, static b => b.BarSurface.ToString()),
                    Token<IfcReinforcingBar>(SteelGrade, static b => b.SteelGrade)),
                Seq(Measure<IfcReinforcingBar>(DetailSchema.NominalDiameter, Dimension.LengthDim, static b => b.NominalDiameter),
                    Measure<IfcReinforcingBar>(DetailSchema.CrossSectionArea, Dimension.AreaDim, static b => b.CrossSectionArea),
                    Measure<IfcReinforcingBar>(BarLength, Dimension.LengthDim, static b => b.BarLength))),
            [typeof(IfcReinforcingMesh)] = Row<IfcReinforcingMesh>(
                static _ => Some(Cast),
                Seq(Token<IfcReinforcingMesh>(MeshType, static m => m.PredefinedType.ToString()),
                    Token<IfcReinforcingMesh>(SteelGrade, static m => m.SteelGrade)),
                Seq(Measure<IfcReinforcingMesh>(MeshLength, Dimension.LengthDim, static m => m.MeshLength),
                    Measure<IfcReinforcingMesh>(MeshWidth, Dimension.LengthDim, static m => m.MeshWidth)) + MeshBars),
            [typeof(IfcTendon)] = Row<IfcTendon>(
                static _ => Some(Cast),
                Seq(Token<IfcTendon>(TendonType, static t => t.PredefinedType.ToString()),
                    Token<IfcTendon>(SteelGrade, static t => t.SteelGrade)),
                Seq(Measure<IfcTendon>(DetailSchema.NominalDiameter, Dimension.LengthDim, static t => DiameterOf(t).IfNone(double.NaN)))),
            [typeof(IfcTendonAnchor)] = Row<IfcTendonAnchor>(
                static _ => Some(Cast),
                Seq(Token<IfcTendonAnchor>(DetailSchema.AnchorType, static a => a.PredefinedType.ToString()),
                    Token<IfcTendonAnchor>(SteelGrade, static a => a.SteelGrade)),
                Empty<IfcTendonAnchor>()),
            [typeof(IfcTendonConduit)] = Row<IfcTendonConduit>(
                static _ => Some(Cast),
                Seq(Token<IfcTendonConduit>(ConduitType, static c => c.PredefinedType.ToString()),
                    Token<IfcTendonConduit>(SteelGrade, static c => c.SteelGrade)),
                Empty<IfcTendonConduit>()),
            [typeof(IfcBearing)] = Row<IfcBearing>(
                static _ => Some(Bearing),
                Seq(Token<IfcBearing>(BearingType, static b => b.PredefinedType.ToString())),
                Empty<IfcBearing>()),
            [typeof(IfcVibrationIsolator)] = Row<IfcVibrationIsolator>(
                static _ => Some(Bearing),
                Seq(Token<IfcVibrationIsolator>(IsolatorType, static i => i.PredefinedType.ToString())),
                Empty<IfcVibrationIsolator>()),
            [typeof(IfcElementAssembly)] = Row<IfcElementAssembly>(
                AssemblyJoint,
                Seq(Token<IfcElementAssembly>(AssemblyType, static a => a.PredefinedType.ToString()),
                    Token<IfcElementAssembly>(AssemblyPlace, static a => a.AssemblyPlace.ToString())),
                Empty<IfcElementAssembly>()),
        }.ToFrozenDictionary());

    sealed record RealizingRow(
        Func<IfcElement, Option<string>> Joint,
        Seq<(PropertyName Name, Func<IfcElement, Option<PropertyValue>> Read)> Values,
        Seq<(PropertyName Name, Dimension Dimension, Func<IfcElement, double> Read)> Measures);

    static RealizingRow Row<T>(
        Func<T, Option<string>> joint,
        Seq<(PropertyName Name, Func<T, Option<PropertyValue>> Read)> values,
        Seq<(PropertyName Name, Dimension Dimension, Func<T, double> Read)> measures)
        where T : IfcElement =>
        new(e => joint((T)e),
            values.Map(static v => (v.Name, (Func<IfcElement, Option<PropertyValue>>)(e => v.Read((T)e)))),
            measures.Map(static m => (m.Name, m.Dimension, (Func<IfcElement, double>)(e => m.Read((T)e)))));

    static Seq<(PropertyName Name, Dimension Dimension, Func<T, double> Read)> Empty<T>() where T : IfcElement =>
        Seq<(PropertyName, Dimension, Func<T, double>)>();

    static (PropertyName Name, Func<T, Option<PropertyValue>> Read) Token<T>(PropertyName name, Func<T, string> read) =>
        (name, value => read(value) is { } token && !string.IsNullOrWhiteSpace(token)
            ? Some((PropertyValue)new PropertyValue.Text(token))
            : Option<PropertyValue>.None);

    static (PropertyName Name, Func<T, Option<PropertyValue>> Read) Tokens<T>(PropertyName name, Func<T, Seq<string>> read) =>
        (name, value => read(value) switch {
            { IsEmpty: true } => Option<PropertyValue>.None,
            { Count: 1 } one => Some((PropertyValue)new PropertyValue.Text(one[0])),
            var many => Some((PropertyValue)new PropertyValue.List(many.Map(static t => (PropertyValue)new PropertyValue.Text(t)))),
        });

    static (PropertyName Name, Dimension Dimension, Func<T, double> Read) Measure<T>(PropertyName name, Dimension dimension, Func<T, double> read) =>
        (name, dimension, read);

    static Option<string> AssemblyJoint(IfcElementAssembly assembly) {
        Seq<string> parts = toSeq(assembly.IsDecomposedBy.AsIterable())
            .Bind(static rel => toSeq(rel.RelatedObjects.AsIterable()))
            .Choose(static part => part is IfcElement and not IfcElementAssembly
                && Realizing.Value.TryGetValue(part.GetType(), out RealizingRow? row) && row is { } claimed
                    ? claimed.Joint((IfcElement)part)
                    : Option<string>.None);
        return Ranked.Filter(parts.Contains).Head;
    }

    public static Fin<Option<PropertyBag>> BagOf(IfcElement realizing, UnitScheme scale, Op key) =>
        Optional(Realizing.Value.GetValueOrDefault(realizing.GetType()))
            .Bind(row => row.Joint(realizing).Map(kind => (Row: row, Kind: kind)))
            .Match(
                None: static () => Fin.Succ(Option<PropertyBag>.None),
                Some: hit => Rows(Seq(Joint(hit.Kind, key))
                    + hit.Row.Values.Map(column => Lift(column.Name, column.Read(realizing)))
                    + hit.Row.Measures.Map(column => Measured(column.Name, column.Dimension, column.Read(realizing), scale, key))));

    // --- [ROWS]
    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Joint(string kind, Op key) =>
        DetailSchema.Realization.Joint(kind, key).Map(static value => Some((DetailSchema.JointType, value)));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Lift(PropertyName name, Option<PropertyValue> value) =>
        Fin.Succ(value.Map(read => (name, read)));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Measured(PropertyName name, Dimension dim, double native, UnitScheme scale, Op key) =>
        double.IsFinite(native)
            ? MeasureValue.OfSi(dim, scale.Coerce(native, QuantityType.OfDimension(dim), dim), key)
                .Map(value => Some((name, (PropertyValue)new PropertyValue.Measure(value))))
            : Fin.Succ(Option<(PropertyName, PropertyValue)>.None);

    static Fin<Option<PropertyBag>> Rows(Seq<Fin<Option<(PropertyName Name, PropertyValue Value)>>> rows) =>
        (rows.Traverse(static row => row.ToValidation()).As()
            .Map(static values => Some(values.Choose(identity)
                .Fold(DetailSchema.Realization.Bag(), static (bag, row) => bag.With(row.Name, row.Value))))).ToFin();

    // --- [PROFILE_DIAMETER]
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

    // --- [ACCESSORY]
    static Seq<string> FastenerOf(IfcDiscreteAccessory accessory) =>
        toSeq(accessory.IsConnectionRealization.AsIterable()
            .SelectMany(static rel => rel.RealizingElements.AsIterable())
            .Choose(static realizing => realizing is IfcMechanicalFastener fastener ? Some(fastener.PredefinedType.ToString()) : None)
            .Distinct().OrderBy(static token => token, StringComparer.Ordinal));

    // --- [MESH_AXES]
    static readonly Seq<(PropertyName Name, Dimension Dimension, Func<IfcReinforcingMesh, double> Read)> MeshBars =
        Family(
            ("Longitudinal", static m => (m.LongitudinalBarNominalDiameter, m.LongitudinalBarSpacing, m.LongitudinalBarCrossSectionArea)),
            ("Transverse", static m => (m.TransverseBarNominalDiameter, m.TransverseBarSpacing, m.TransverseBarCrossSectionArea)));

    static Seq<(PropertyName Name, Dimension Dimension, Func<IfcReinforcingMesh, double> Read)> Family(
        params (string Axis, Func<IfcReinforcingMesh, (double Diameter, double Spacing, double Area)> Read)[] axes) =>
        toSeq(axes).Bind(axis => Seq<(PropertyName, Dimension, Func<IfcReinforcingMesh, double>)>(
            (PropertyCategory.Seam.Row($"{axis.Axis}BarNominalDiameter"), Dimension.LengthDim, m => axis.Read(m).Diameter),
            (PropertyCategory.Seam.Row($"{axis.Axis}BarSpacing"), Dimension.LengthDim, m => axis.Read(m).Spacing),
            (PropertyCategory.Seam.Row($"{axis.Axis}BarCrossSectionArea"), Dimension.AreaDim, m => axis.Read(m).Area)));

    // --- [READER_ROWS]
    static readonly PropertyName BearingType = PropertyCategory.Seam.Row("BearingType");
    static readonly PropertyName IsolatorType = PropertyCategory.Seam.Row("IsolatorType");
    static readonly PropertyName TendonType = PropertyCategory.Seam.Row("TendonType");
    static readonly PropertyName BarSurface = PropertyCategory.Seam.Row("BarSurface");
    static readonly PropertyName SteelGrade = PropertyCategory.Seam.Row("SteelGrade");
    static readonly PropertyName BarLength = PropertyCategory.Seam.Row("BarLength");
    static readonly PropertyName MeshType = PropertyCategory.Seam.Row("MeshType");
    static readonly PropertyName MeshLength = PropertyCategory.Seam.Row("MeshLength");
    static readonly PropertyName MeshWidth = PropertyCategory.Seam.Row("MeshWidth");
    static readonly PropertyName ConduitType = PropertyCategory.Seam.Row("ConduitType");
    static readonly PropertyName AssemblyType = PropertyCategory.Seam.Row("AssemblyType");
    static readonly PropertyName AssemblyPlace = PropertyCategory.Seam.Row("AssemblyPlace");
}
```

## [03]-[RESEARCH]

(none)

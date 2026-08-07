# [BIM_CONNECTION]

`ConnectionProjection` is the realizing-element connection-detail reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: it lowers the WHOLE GeometryGym realizing-element surface — the bolted/welded `IfcMechanicalFastener` and the bonded/welded `IfcFastener`, the fabricated `IfcDiscreteAccessory` framing connector, the cast-in `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`, the support `IfcBearing`, and the isolation `IfcVibrationIsolator` — onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the ONE neutral realizing-detail schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` AUTHORS — this reader READS the same schema, never a hand-synced parallel bag), bound to the realizing element's `Graph/element#NODE_MODEL` `Object` node through one `Relations/relation#EDGE_ALGEBRA` `Assign.PropertyDefinition` edge, so an authored realizing element and a re-imported IFC one content-key to one `Node.PropertySet` and a downstream detailing consumer reads the bolt diameter, the stud-shear connector, the framing-connector accessory token, the reinforcing cover, the post-tensioning tendon, and the bearing type off the ONE `Graph/element#ELEMENT_GRAPH` `Bake`-derived element it already holds — never a second store. The physical joint TOPOLOGY — which two members meet through which realizing element — is the `Relations/relation#EDGE_ALGEBRA` `Connect` edge carrying its realizing node on the `Connect.Realizing` `Option<NodeId>` field (realizing-ness the field, never a `ConnectKind` row — the medium closes at `Path`/`Port`) the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` already authors from `IfcRelConnectsWithRealizingElements` (fanning one edge per `RealizingElements` member into the seam `Connect.Realizing` option, so a multi-realizer joint keeps every realizer); this page owns ONLY the realizing element's fabrication DETAIL the general `Object` fold does not read — the native reinforcing scalars, the declared `SteelGrade` designation, and the fastener nominal diameter the internal GeometryGym scalar hides on BOTH the occurrence and its type.

This RETIRES the migration source's parallel `ConnectionDetail` record + `ConnectionRealization` `[Union]` + `BoltPattern`/`WeldSchedule`/`BearingSurface` `[ComplexValueObject]` + `ConnectionKind`/`Clearance` family keyed by `BimModel`/`GlobalId` AND its hand-rolled `(GeometryKey, DetailKey)` second content-key — the "second stored record off the element" the rebuild forbids, mirroring `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection` retiring the `MemberConnection`/`SupportRestraint` typed store — AND the `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary, the deleted form mirroring `Semantics/composition#MATERIAL_COMPOSITION` retiring the `MaterialAssignmentWire`/`MaterialPropertyWire` carriers. The joint modality is the realizing `Object.PredefinedType` token plus the bag's `JointType` enumerated, the bolt/weld/bearing/cast detail is typed `Properties/property#PROPERTY_VALUE` `PropertyValue` entries, and the IFC egress is the `Projection/egress#IFC_EGRESS` `Emit` generic round-trip — the realizing `IfcMechanicalFastener`/`IfcReinforcingBar` re-authored as an `Object` node, the detail bag as an `IfcPropertySet` through `ReauthorProperties`, the joint as `IfcRelConnectsWithRealizingElements` through `ReauthorRelationships` — never a connection-specific writer. The reader is HOST-NEUTRAL (it binds the realizing geometry by the `Graph/element#NODE_MODEL` `RepresentationContentHash` content key, never re-tessellating the fastener) and detail-TOTAL (a realizing element with no readable detail yields no bag — the `Fin` rail carries only the seam `MeasureValue.OfSi` admission, the schema's own `JointType` allowed-set admission, and the `All` root-resolution, so a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` rather than stranding a source entity; the joint edge's own endpoint rail stays the `EdgeProjection` `edge-endpoint-miss`), a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection`, `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection`, and `Semantics/georeference#GEO_PROJECTION` `GeoReferenceProjector` — so a connection enrichment never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [02]-[CONNECTION_DETAIL]: `ConnectionProjection` the GeometryGym realizing-element-detail reader — `Detail` the ONE polymorphic attribute-bag reader over the `Realizing` `FrozenDictionary<Type, RealizingRow>` row table (`IfcMechanicalFastener` bolt/weld, `IfcFastener` glue/mortar/weld, `IfcDiscreteAccessory` framing connector, `IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcTendonConduit` cast, `IfcBearing` support, `IfcVibrationIsolator` isolation, `IfcElementAssembly` the realizing composite whose modality derives from its parts) onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` (the IDENTICAL neutral schema the `Rasm.Materials` `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` authors, never a hand-synced parallel bag) and answering `None` for a non-realizing element so the realizing gate and the bag construction are one read, the `JointModality` token roster with its `Ranked` precedence order, the `RealizingRow` three-column shape and its `Row<T>`/`Token`/`Tokens`/`Measure` column mints, the `AssemblyJoint` composite derivation, and the `Joint`/`Lift`/`Measured`/`Rows` row-fold every row composes, the `MeshBars` axis×column cross-product generator minting the mesh's paired `PropertyName`s and readers, `DiameterOf` the fastener/tendon nominal-diameter recovery through the associated `IfcMaterialProfileSetUsage` `IfcCircleProfileDef.Radius` cross-section (the public channel for the GeometryGym-internal `mNominalDiameter` scalar), `FastenerOf` the co-realizing attaching-fastener token-set recovery off the `IfcElement.IsConnectionRealization` back-pointer (ALL tokens, distinct + ordinal-sorted), `Mint` the content-keyed seam `PropertySet` node, and `All` the fold over the model's `Extract<IfcElement>` stream producing the `(bag node, Assign.PropertyDefinition edge)` pairs the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` composes onto the `ElementGraph`.

## [02]-[CONNECTION_DETAIL]

- Owner: `ConnectionProjection` the static realizing-element-detail reader `SemanticProjector` composes, lowering the WHOLE GeometryGym realizing-element surface onto the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node — never a stored record and never a hand-synced bag, the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors so an authored realizing element and a re-imported one are one content-keyed `Node.PropertySet`. It owns the polymorphic `Detail` attribute-bag reader (one entry resolving the realizing surface through the `Realizing` type-keyed row table — mechanical/non-mechanical fastener, fabricated discrete accessory, reinforcing bar/mesh/tendon/anchor/conduit, support bearing, isolation vibration isolator, realizing element assembly — onto its bolt/weld/bonded/accessory/cast/bearing bag through the `Joint`/`Lift`/`Measured`/`Rows` row-fold over `DetailSchema.Realization.Bag()`, and onto `None` for an unclaimed type or an assembly whose parts declare no modality), the `JointModality` roster naming the five seam-declared joint tokens ONCE, the `MeshBars` cross-product generator, the `DiameterOf` cross-section diameter recovery, the `FastenerOf` co-realizing attaching-fastener token-SET recovery (ALL co-realizing `IfcMechanicalFastener` tokens fold — distinct, ordinal-sorted — never a `.Head` slice), the `Mint` of the seam `Node.PropertySet`, and the `All` model fold over `Extract<IfcElement>`; the typed structures the migration source minted (`ConnectionDetail`, the `ConnectionRealization` `[Union]`, `BoltPattern`/`WeldSchedule`/`BearingSurface`, `ConnectionKind`, `Clearance`) are all GONE — the realizing element is the seam `Object` node the general `Objects` fold mints, the joint modality its `PredefinedType` token plus the bag `JointType` row, the joint topology the neutral `Connect` edge carrying the realizing node on its `Connect.Realizing` field, and the fabrication scalars the typed `PropertyValue` bag entries.
- Entry: `ConnectionProjection.Detail(IfcElement realizing, UnitScale scale, Op key)` is the ONE polymorphic attribute-bag reader discriminating on the realizing-element shape, `Fin<Option<PropertyBag>>` carrying the seam `MeasureValue.OfSi` admission (`scale` the per-projection native→SI coercion every measured row crosses) and the schema's own `DetailSchema.Realization.Joint(selected, key)` allowed-set admission, so an out-of-set joint token rejects at the seam declarer rather than passing this call site unchecked — an `IfcMechanicalFastener` onto the bolt/weld bag (the `JointType`/`FastenerType` tokens plus the `NominalDiameter` recovered from the cross-section profile), an `IfcFastener` onto the bonded/weld bag (the `JointType`/`FastenerType` tokens), an `IfcDiscreteAccessory` onto the framing-connector bag (the `JointType`/`AccessoryType` tokens plus the `FastenerType` token set of ALL co-realizing attaching `IfcMechanicalFastener`s — a single fastener lands the two-token `Text` shape the `Rasm.Materials` `Component/connector#CONNECTOR_FAMILY` `ConnectorDetail` seed bag authors (`ComponentDetail.Token(DetailSchema.AccessoryType, ...)` + `Token(DetailSchema.FastenerType, install.Fastener.IfcFastenerType)`, projected by the `ComponentProjector`), content-keying byte-identically; a multi-fastener attachment (nailplate + screw, bolt + stud) lands the typed `PropertyValue.List` of distinct ordinal-sorted tokens the seam value family carries natively), an `IfcReinforcingBar` onto the cast bag (the `BarType`/`BarSurface`/`SteelGrade` tokens plus the native `NominalDiameter`/`CrossSectionArea`/`BarLength` scalars), an `IfcReinforcingMesh` onto the mesh bag (the `MeshType`/`SteelGrade` tokens plus the native mesh length/width and the `MeshBars` axis×column rows — the longitudinal/transverse bar diameters, spacings, and cross-section areas), an `IfcTendon` onto the cast bag (the `TendonType`/`SteelGrade` tokens plus the profile-recovered `NominalDiameter`), an `IfcTendonAnchor`/`IfcBearing`/`IfcVibrationIsolator` onto its type-token bag (`AnchorType` + `SteelGrade`/`BearingType`/`IsolatorType` plus the normalized `JointType`), and any other (or null) onto `None` — the realizing-vs-not gate IS the reader's own answer, so no bag is constructed for the element the fold then discards; detail ABSENCE never faults (the entity class is the general fold's `BimFault.UnmappedClass`), and there is no `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family and no per-family `FastenerDetail`/`AccessoryDetail`/`BarDetail`/`MeshDetail`/`TendonDetail` method — one polymorphic `Detail` over one row table whose columns carry every family's joint derivation, token readers, and measured projectors; `ConnectionProjection.All(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, Op key)` folds every `IfcElement` the project carries — the `Realizing` table lookup the SOLE discriminator of the realizing families, a non-realizing element answering `None` and so yielding no bag — into the `Seq<(Node Bag, Relationship Edge)>` the `Projection/semantic#SEMANTIC_PROJECTOR` `Project` concats onto its node and edge sets (exactly as it concats `Materials` and `EdgeProjection.All`), so a new realizing family is one `Realizing` row and never a parallel extract list to drift.
- Auto: each arm folds its rows onto the seam `DetailSchema.Realization.Bag()` through the `Joint`/`Token`/`Measured`/`Rows` row-fold — one `Joint` schema modality row, the type tokens, and the SI `Measured` rows — so the arm is a flat declarative row list, never a repeated `MeasureValue.OfSi` construction and never a hand-spelled set-name, precedence, or joint literal (the set name and precedence ride the schema, the five joint tokens the `JointModality` roster). The rows of one arm are INDEPENDENT, so `Rows` inverts them applicatively through `Traverse` — `Joint` and `Measured` carry the only two admissions on the lane and `Token` is TOTAL, riding the pure lift so the row shape stays uniform. `Detail` reads the realizing element's NATIVE fabrication scalars the general `Object` fold leaves on the geometry — the reinforcing bar/mesh expose their `NominalDiameter`/`CrossSectionArea`/`BarLength`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea` as public doubles in the model's DECLARED units (GeometryGym pre-coerces nothing), so every one crosses the ONE per-projection `UnitScale.Coerce` entry over its `MeasureRow` before admitting through the DIMENSION-only `MeasureValue.OfSi(Dimension.LengthDim/AreaDim, ...)` overload — the same dimension-only admit the Materials author takes — the seam bag law's two-peer carve, because that author seeds its rows off a catalogue and holds no foreign measure type to name, so a name this importing side alone spells forks the content key the pair exists to share — and an authored and an imported row therefore content-key identically; the `JointType` enumerated derives from the realizing family (an `IfcMechanicalFastener` `STUDSHEARCONNECTOR`/`SHEARCONNECTOR` or an `IfcFastener` `WELD` is `Welded`, every other discrete mechanical fastener `Bolted`, an `IfcFastener` `GLUE`/`MORTAR` `Bonded`, an `IfcBearing` and the isolation-bearing `IfcVibrationIsolator` `Bearing`, a reinforcing bar/mesh/tendon/anchor `Cast`) through `DetailSchema.Realization.Joint(kind)`, the schema's closed `JointTypes` allowed set the egress facet validates against; the reinforcing root's public `IfcReinforcingElement.SteelGrade` lands as the ingest-only `SteelGrade` token (the superseded-but-live EXPRESS designation, the only grade carrier when an older export binds no material); the mechanical-fastener AND tendon `NominalDiameter` is the special case — `mNominalDiameter`/`mNominalLength` are GeometryGym-internal on both with NO public getter, so `DiameterOf` recovers the diameter through the inherited `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` → `ForProfileSet.MaterialProfiles` → `Profile` → `IfcCircleProfileDef.Radius` × 2), the documented public round-trip channel, yielding `Option<double>.None` (read `IfNone(NaN)` and dropped at the `Filter`, never a fabricated 0) when no circle profile binds; every non-finite scalar (an unset GeometryGym `NaN` default) and every blank token (an undeclared `SteelGrade` `""`) is dropped at the `Detail` egress `Filter` so a partially-specified realizing element never emits a misleading measure or an empty text row; `Mint` builds the seam `Node.PropertySet` whose id is `NodeId.Content` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical realizing details dedup to one node, and `All` resolves each realizing element's rooted `NodeId` through the `rooted` map and binds the bag through an `Assign.PropertyDefinition` edge — a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` (skipping it strands a source entity while claiming the realization fold was total; the joint edge's own endpoint rail stays `EdgeProjection`'s `edge-endpoint-miss`).
- Receipt: the connection-detail bag lands on the ONE seam `ElementGraph` as a `PropertySet` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold merges into `element.Properties` through the realizing element's `Assign.PropertyDefinition` edge, so a downstream detailing consumer reads `element.Properties.Find(b => b.SetName == DetailSchema.Realization.SetName).Bind(b => b.Find(DetailSchema.NominalDiameter))` for the bolt diameter / weld stud / reinforcing cover off the baked realizing element (the NEUTRAL `SetName`, never an IFC literal — the `Rasm_ConnectionRealization` Pset name is applied only at the `Projection/egress#IFC_EGRESS` mapping), and the joint topology off the `Connect` edge whose `Connect.Realizing` field carries the realizing node the `EdgeProjection` authors — a steel bolted moment connection's fasteners, a stud-shear-connector deck weld, and a cast-in reinforcing lap each carrying their physical detail on the one graph the consumer already holds, never a parallel connection store and never a second member-selection surface; the `Projection/egress#IFC_EGRESS` `Emit` re-authors the bag (`IfcPropertySet` through `ReauthorProperties`) and the joint (`IfcRelConnectsWithRealizingElements` through `ReauthorRelationships`) generically, so the connection round-trips with the rest of the graph.
- Packages: GeometryGymIFC_Core (the realizing-element surface consumed as settled vocabulary), Rasm.Element (the seam `DetailSchema` (the neutral realizing-detail schema this reader composes) + the `Node`/`NodeId`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`Relationship`/`AssignKind` payloads, the schema carrying the `SetName`/`OccurrenceWins`/`JointTypes` so this reader hand-spells none), Rasm (the `Op` key and the `UnitScale`/`MeasureRow` coercion pair this reader threads), LanguageExt.Core (`Option`/`Seq`/`Map`).
- Growth: a new realizing-element family is one `Realizing` row (its joint derivation, its token columns, and its measured columns, a hidden scalar composing `DiameterOf` inside its own projector); a new fabrication scalar is one `Measure` column on its row carrying its `MeasureValue` over the composed `Dimension` (a row in the canonical `DetailSchema` vocabulary composes the schema static, an ingest-only scalar one `PropertyCategory.Seam.Row` mint in the `[READER_ROWS]` block); a new mesh column is one `MeshBars` column row and a new bar axis one axis row, the cross-product minting both the `PropertyName`s and the readers; a new joint modality is one token on the seam `DetailSchema.Realization.JointTypes` allowed set plus one `JointModality` row seated at its rank in `Ranked` and its `JointType` derivation, never a reader-local allowed set and never a bare literal at a derivation site; never a per-joint-type connection record, never a `BoltOf`/`WeldOf`/`CastOf`/`TendonOf`/`BearingOf` sibling family, never a second connection store, never a hand-synced parallel detail bag, never a `(GeometryKey, DetailKey)` parallel content key, and never a re-tessellation of the realizing element.
- Boundary: the connection detail is the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` conforming `PropertyBag` on the realizing `Object` node, COMPOSED through `DetailSchema.Realization.Bag()`/`.Joint(kind)` (the IDENTICAL schema the `Rasm.Materials` `ComponentProjector` authors) — a hand-synced parallel bag re-spelling the set name, the `OccurrenceWins` precedence, or the `JointTypes` allowed set is the deleted form (the reader READS the seam-declared schema, never a copy), and a typed `ConnectionDetail`/`ConnectionRealization`/`BoltPattern`/`WeldSchedule`/`BearingSurface`/`ConnectionKind`/`Clearance` second-store record family is the deleted form (mirroring `StructuralProjection` retiring `MemberConnection`/`SupportRestraint`) — the realizing element is the seam `Object` node, its detail the schema bag the `Bake` fold reads flat; the in-graph bag carries the NEUTRAL `SetName` and the IFC `Rasm_ConnectionRealization` Pset name is applied ONLY at the `Projection/egress#IFC_EGRESS` `Emit` mapping, so a `Rasm_ConnectionRealization` literal as the in-graph set name is the deleted form; the five joint modality tokens are the `JointModality` roster naming the seam allowed-set members ONCE and a bare `"Cast"`/`"Bolted"`/`"Welded"`/`"Bonded"`/`"Bearing"` literal at a derivation site is the deleted form that forks the vocabulary the seam declares; the canonical realizing-detail rows compose the `DetailSchema` `PropertyName` statics while an INGEST-ONLY scalar the author never mints (a mesh sheet's geometry, a tendon/anchor/bearing token, a bar's surface + overall length) is a reader-side `[READER_ROWS]` row minted through the owner-blessed `PropertyCategory.Seam.Row` category — a call-site `PropertyName.Create` in this reader is the deleted form and a row a second package begins keying on is promoted to a `DetailSchema` static at the seam owner — an authored bag and a richer imported bag are faithfully DIFFERENT content-keyed nodes, never a forced byte-match, and a reader-side row never widens the seam `DetailSchema`; a row family that is a CROSS-PRODUCT of an axis and a column set (the mesh's two bar axes × three measured columns) is the `MeshBars` generator and six hand-spelled statics beside six hand-spelled `Measured` lines is the deleted form; the `BimModel`/`BimElement` join (`federated.Elements`, the `(MemberGlobalId, MemberGlobalId)` pair, the `BindFederated` dangling-reference rail) is GONE with the retired element records, the joint endpoints being the `Connect` edge's `NodeId` pair the `EdgeProjection` resolves and the analytical member↔connection topology the `Model/structural#STRUCTURAL_PROJECTION` `IfcRelConnectsStructuralMember` `Generic` edge, both meeting on the SHARED graph nodes, never a `GlobalId`-pair selection surface; the detail-bag attachment is ONE polymorphic `Detail` over the `Realizing` row table keyed on the EXACT runtime type (the realizing families are `IfcElement` leaves with none a supertype of another, so exact keying fails SAFE where an `is` ladder would silently detail a future subtype against its parent's columns) and a `RealizationOf`/`BoltOf`/`WeldOf`/`LapOf`/`TendonOf`/`BearingOf` sibling-method family is the deleted form; the realizing-vs-not gate is `Detail`'s OWN `Option` answer and a bag minted for a non-realizing element then discarded on an emptiness test is the deleted form (the schema bag is constructed once, for an element that carries a detail); detail ABSENCE never faults — an unreadable detail is `None`, and routing a missing scalar or token onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the entity-class rail is the general fold's `Fin<GraphDelta>`) — while the fold's own rail is real: a detail-bearing element without a rooted `Object` faults `BimFault.DanglingReference` `connection-detail-root-miss` and a malformed seam measure faults through `MeasureValue.OfSi`; every native scalar crosses the ONE `UnitScale.Coerce` entry the projection threads and a raw double admitted as already-SI is the mm-vs-metre import trap `Semantics/composition#MATERIAL_COMPOSITION` names; the connection detail stays host-neutral scalar data and a RhinoCommon `Brep`/`Mesh` realizing-element field or an in-process fastener tessellation is the named seam violation, the realizing geometry binding by the `RepresentationContentHash` content key; the GeometryGym realizing surface (`IfcMechanicalFastener.PredefinedType` `IfcMechanicalFastenerTypeEnum` and `IfcFastener.PredefinedType` `IfcFastenerTypeEnum`, the `IfcDiscreteAccessory.PredefinedType` `IfcDiscreteAccessoryTypeEnum` plus the `IfcElement.IsConnectionRealization` `SET<IfcRelConnectsWithRealizingElements>` back-pointer to the co-realizing attaching `IfcMechanicalFastener` SET — ALL tokens fold distinct and ordinal-sorted so an IFC file's set order never forks the content key, a single token the authored `Text` shape and a multi-token set the typed `PropertyValue.List`, never a `.Head` slice dropping a nailplate+screw second fastener and never a joined literal, the public `IfcReinforcingBar.NominalDiameter` (`IfcReinforcingBarType.NominalDiameter` type-fallback)/`CrossSectionArea`/`BarLength`/`PredefinedType`/`BarSurface`, the public `IfcReinforcingMesh.PredefinedType`/`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`/`LongitudinalBarCrossSectionArea`/`TransverseBarCrossSectionArea`, the `IfcTendon.PredefinedType` `IfcTendonTypeEnum` / `IfcTendonAnchor.PredefinedType` `IfcTendonAnchorTypeEnum` / `IfcBearing.PredefinedType` `IfcBearingTypeEnum` / `IfcVibrationIsolator.PredefinedType` `IfcVibrationIsolatorTypeEnum`, the public `IfcReinforcingElement.SteelGrade` designation, the `HasAssociations` `IfcRelAssociatesMaterial.RelatingMaterial` (`IfcMaterialProfileSetUsage` or bare `IfcMaterialProfileSet`) → `IfcCircleProfileDef.Radius` chain) is consumed as settled vocabulary (`.api/api-geometrygym-ifc`) and a hand-rolled realizing reader is the deleted form; the mechanical-fastener and tendon nominal diameter rides the associated circle-profile radius (the public channel for the internal `mNominalDiameter`) and a fabricated `0` diameter on an unprofiled element is the deleted form (the entry reads `NaN` and is dropped at the egress `Filter`); the realizing element's CLASSIFICATION and MATERIAL ride the general `Object`/`Associate` folds, not this bag — an ASSOCIATED material's grade or embodied-carbon column on the connection bag is the named seam violation (those grow on the seam `MaterialPropertySet` the `Semantics/composition` egress authors), while the reinforcing root's OWN declared `SteelGrade` EXPRESS attribute is an ingest-only reader row (public on `IfcReinforcingElement`, the only carrier when an older export binds no material; blank drops at the `Filter`, and the Materials author never mints it); the realizing family CLOSES at the leaves the `Realizing` table names and the attached-to-one-element `IfcVibrationDamper` is deliberately NOT one (a realizing element seats BETWEEN two elements), each being an `IfcElement` leaf so the one `Extract<IfcElement>` walk discovers it exactly once; the typed `BoltPattern`/`WeldSchedule` reconstruction relocates to the `Rasm.Fabrication` consumer exactly as the typed analysis model relocated to `Rasm.Compute`, the seam carrying the neutral typed bag alone; the egress is the `Projection/egress#IFC_EGRESS` `Emit` generic `ReauthorProperties`/`ReauthorRelationships` and a `ConnectionItemWire`/`ConnectionWire` second wire crossing the `Rasm.Materials` boundary is the deleted form (those Materials wires are retired, a connection element authored from the Materials/Fabrication side projecting onto the seam graph as an `Object` node + `Connect` edge the `Emit` re-authors).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;              // the realizing-family row table keyed on the runtime entity type
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim;
using Rasm.Bim.Projection;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Semantics;

// --- [OPERATIONS] -------------------------------------------------------------------------
// The realizing-element connection-detail reader Projection/semantic#SEMANTIC_PROJECTOR composes: it lowers the WHOLE
// GeometryGym realizing-element family (mechanical/non-mechanical fastener, discrete accessory, reinforcing
// bar/mesh/tendon/anchor, bearing, vibration isolator) onto the seam DetailSchema.Realization conforming PropertyBag
// (Properties/property#DETAIL_SCHEMA — the IDENTICAL neutral schema the Rasm.Materials ComponentProjector AUTHORS, so
// this reader READS it and never hand-synces a parallel ConnectionDetail store or a re-spelled bag), bound to the
// realizing Object node. The reader is TOTAL — an unreadable detail answers None, never a fault; the element identity,
// entity class, and dangling-endpoint rails are the general projector's Fin<GraphDelta> concern, so this reader never
// re-cases BimFault. The joint TOPOLOGY is the Connect edge whose Connect.Realizing Option<NodeId> field carries the
// realizing node EdgeProjection authors (the medium closes at ConnectKind.Path/Port); this page owns only the detail.
// FastenerType is a row of BOTH DetailSchema.Realization (this reader's realizing-element fastening) and
// DetailSchema.Product (the Materials panel author's board fastening), so a consumer resolves it by the bag's SetName
// FIRST — a bare FastenerType lookup across bags reads a gypsum board's screw as a realizing bolt. MembraneSeam is a
// Product row the panel author emits from its membrane arm ALONE, so a reader keying it for any non-membrane panel
// resolves absence and reads that panel's own fastening off FastenerType instead; the two rows carry distinct facts and
// are never one read. The IFC Rasm_ConnectionRealization Pset name + the IFC predefined enums stay Bim-only — Emit maps
// the neutral DetailSchema.Realization.SetName onto the IFC Pset name at egress, the seam bag carrying the neutral name.
public static class ConnectionProjection {
    // The reader composes the seam DetailSchema.Realization (Properties/property#DETAIL_SCHEMA) — the ONE neutral
    // realizing-detail schema the Rasm.Materials Component projection AUTHORS — so an imported realizing element and an
    // authored one are one content-keyed Node.PropertySet on the one graph, never a hand-synced parallel bag. The NEUTRAL
    // SetName (DetailSchema.Realization.SetName), the OccurrenceWins precedence, the closed JointType allowed-set
    // (DetailSchema.Realization.JointTypes), and the canonical realizing-detail PropertyName vocabulary all ride the
    // schema; this reader hand-spells NONE of them. The IFC Pset NAME (the buildingSMART-reserved "Rasm_ConnectionRealization"
    // custom set) is Bim-only — Emit maps DetailSchema.Realization.SetName onto it at egress, so the literal lives ONLY at
    // the Projection/egress#IFC_EGRESS mapping, never as the in-graph set name a seam node carries.

    // --- [JOINT_MODALITY]
    // The five joint modalities this reader derives, each a MEMBER of the seam DetailSchema.Realization.JointTypes
    // closed allowed set — the seam declares the set and Joint(...) admits through it, so a token drifting off the
    // roster rejects at PropertyValue.Of rather than at a call site. Named ONCE here through nameof so no derivation
    // site spells a literal and a modality rename is one row; a reader-local ALLOWED SET (a page [SmartEnum]
    // duplicating the seam's closed family) is the deleted fork this roster deliberately is not.
    // Ranked carries the roster in PRECEDENCE order, which is the second fact the modalities have always had and the
    // enumerated form could not express: a realizing COMPOSITE takes the modality of the parts that realize it, and a
    // truss whose members are both welded and bolted is a welded assembly. The rank is that of the joint whose failure
    // governs, so a fabricated connection outranks a fastened one and a mechanical one outranks a bonded one.
    static readonly string Bolted = nameof(Bolted);
    static readonly string Welded = nameof(Welded);
    static readonly string Bonded = nameof(Bonded);
    static readonly string Bearing = nameof(Bearing);
    static readonly string Cast = nameof(Cast);

    static readonly Seq<string> Ranked = Seq(Welded, Cast, Bolted, Bonded, Bearing);

    // The fold the SemanticProjector composes: every IfcElement the project carries -> (its content-keyed connection-detail
    // bag node, the Assign.PropertyDefinition edge binding the bag to the realizing Object node). The Detail switch is the
    // SOLE realizing-family discriminator — a non-realizing element answers None and Choose drops it — so a new realizing
    // family is ONE Detail arm, never a parallel per-type Extract list to drift out of sync. The one Extract<IfcElement>
    // walk reads every element DIRECTLY (not via the realizing relation) so an element carries its fabrication detail
    // whether or not it sits in an IfcRelConnectsWithRealizingElements (the joint topology riding the separate Connect
    // edge), discovers each element once (the realizing families are IfcElement leaves, so no sibling double-count), and
    // the general Objects fold has already minted each as the Object node this bag binds against. A detail-bearing element
    // without a rooted object faults; skipping it would strand a source entity while claiming the realization bag was total.
    public static Fin<Seq<(Node Bag, Relationship Edge)>> All(
        IfcProject project,
        Map<string, NodeId> rooted,
        double tolerance,
        UnitScale scale,
        Op key) =>
        project.Extract<IfcElement>().AsIterable().ToSeq()
            .TraverseM(realizing => Detail(realizing, scale, key).Bind(detail => detail.Match(
                None: static () => Fin.Succ(Option<(Node Bag, Relationship Edge)>.None),
                Some: bag => rooted.Find(realizing.GlobalId)
                    .ToFin(new BimFault.DanglingReference(key, $"connection-detail-root-miss:{realizing.GlobalId}"))
                    .Map(node => {
                        Node.PropertySet minted = Mint(bag, tolerance);
                        return Some(((Node)minted, (Relationship)new Relationship.Assign(node, minted.Id, AssignKind.PropertyDefinition)));
                    }))))
            .As()
            .Map(static details => details.Choose(identity).ToSeq());

    // The content-keyed seam PropertySet mint Semantics/composition and Semantics/appearance share: construct the node with
    // a discarded placeholder id, then re-key from the seam Node.ToCanonicalBytes (id excluded) so two structurally-identical
    // connection details dedup to one node — never a second (GeometryKey, DetailKey) hasher. A class-root [Union] Node case
    // has NO compiler-generated `with`, so the content id re-stamps through the seam Graph/element#NODE_MODEL Node.Relabel
    // (a `draft with { Id }` a class case cannot honour is the deleted form, the SAME re-stamp the Rasm.Materials Mint takes).
    static Node.PropertySet Mint(PropertyBag bag, double tolerance) {
        var draft = new Node.PropertySet(NodeId.Content(default), bag);
        return (Node.PropertySet)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
    }

    // --- [REALIZING_TABLE]
    // The realizing family is a ROW TABLE keyed on the runtime entity type, not a switch: each row carries the three
    // columns a realizing detail HAS — the joint-modality derivation, the token readers, and the measured-row
    // projectors — so a new family is one row's data and the reader body never grows an arm. The retired switch spread
    // those same three facts across ten arms and five sibling methods, which is why the assembly and conduit families
    // could not be added without a sixth method and why the mesh generator had to be spliced into an arm by hand.
    // Type identity is EXACT — GetType(), never a pattern match. The ten realizing families are IfcElement LEAVES in
    // IFC4.3 with none a supertype of another, so exact keying is total over them today AND fails SAFE tomorrow: a
    // future subtype under one of these lands unclaimed and details nothing, where an `is` ladder would silently
    // detail it against its parent's columns and its own scalars would go unread with no diagnostic.
    // The lookup IS the realizing gate — an unclaimed type answers None with no bag constructed, so All folds the
    // whole Extract<IfcElement> stream through it with no parallel family list. A row whose Joint derivation answers
    // None (a composite realizing nothing this reader can detail) likewise answers None, so Some still means exactly
    // "this element realizes and this bag describes it".
    static readonly Lazy<FrozenDictionary<Type, RealizingRow>> Realizing = new(static () =>
        new Dictionary<Type, RealizingRow> {
            // A stud/shear connector welds; every other discrete mechanical fastener bolts. The nominal diameter rides
            // the cross-section profile radius because mNominalDiameter has no public getter on the OCCURRENCE and
            // none on IfcMechanicalFastenerType either — the type edge publishes PredefinedType alone, so there is no
            // type-level diameter fallback to compose and claiming one would be a phantom.
            [typeof(IfcMechanicalFastener)] = Row<IfcMechanicalFastener>(
                static f => Some(f.PredefinedType is IfcMechanicalFastenerTypeEnum.STUDSHEARCONNECTOR or IfcMechanicalFastenerTypeEnum.SHEARCONNECTOR ? Welded : Bolted),
                Seq(Token<IfcMechanicalFastener>(DetailSchema.FastenerType, static f => f.PredefinedType.ToString())),
                Seq(Measure<IfcMechanicalFastener>(DetailSchema.NominalDiameter, Dimension.LengthDim, static f => DiameterOf(f).IfNone(double.NaN)))),
            // The non-mechanical fastener (IfcFastener, sibling of IfcMechanicalFastener): a WELD realizes a Welded
            // joint, a GLUE/MORTAR a Bonded one. It publishes PredefinedType alone, so the FastenerType token is the
            // whole row — kept (mirroring BarType) for the detailer's uniform bag read, distinct from the Object
            // node's classification-side PredefinedType.
            [typeof(IfcFastener)] = Row<IfcFastener>(
                static f => Some(f.PredefinedType is IfcFastenerTypeEnum.WELD ? Welded : Bonded),
                Seq(Token<IfcFastener>(DetailSchema.FastenerType, static f => f.PredefinedType.ToString())),
                Seq<(PropertyName, Dimension, Func<IfcFastener, double>)>()),
            // A fabricated framing connector: its own SHOE/BRACKET/ANCHORPLATE token plus the token SET of ALL
            // co-realizing attaching fasteners, which is why its second column is a multi-token reader rather than a
            // Token — the authored Materials seed bag denormalizes both onto one connector Component.
            [typeof(IfcDiscreteAccessory)] = Row<IfcDiscreteAccessory>(
                static _ => Some(Bolted),
                Seq(Token<IfcDiscreteAccessory>(DetailSchema.AccessoryType, static a => a.PredefinedType.ToString()),
                    Tokens<IfcDiscreteAccessory>(DetailSchema.FastenerType, FastenerOf)),
                Seq<(PropertyName, Dimension, Func<IfcDiscreteAccessory, double>)>()),
            // A cast-in reinforcing bar: the public NominalDiameter (IfcReinforcingBarType.NominalDiameter type-
            // fallback get) / CrossSectionArea / BarLength scalars over their SI dimensions, the BarType (STUD is the
            // cast-in bar, NOT the welded connector) and BarSurface tokens, and the reinforcing root's own declared
            // SteelGrade. The NaN defaults drop at the Detail egress Filter.
            [typeof(IfcReinforcingBar)] = Row<IfcReinforcingBar>(
                static _ => Some(Cast),
                Seq(Token<IfcReinforcingBar>(DetailSchema.BarType, static b => b.PredefinedType.ToString()),
                    Token<IfcReinforcingBar>(BarSurface, static b => b.BarSurface.ToString()),
                    Token<IfcReinforcingBar>(SteelGrade, static b => b.SteelGrade)),
                Seq(Measure<IfcReinforcingBar>(DetailSchema.NominalDiameter, Dimension.LengthDim, static b => b.NominalDiameter),
                    Measure<IfcReinforcingBar>(DetailSchema.CrossSectionArea, Dimension.AreaDim, static b => b.CrossSectionArea),
                    Measure<IfcReinforcingBar>(BarLength, Dimension.LengthDim, static b => b.BarLength))),
            // A cast-in welded mesh: the sheet's own overall length/width plus the MeshBars axis x column
            // cross-product, which composes straight into the measured column because the generator already produces
            // exactly this shape — the splice an arm formerly needed is gone.
            [typeof(IfcReinforcingMesh)] = Row<IfcReinforcingMesh>(
                static _ => Some(Cast),
                Seq(Token<IfcReinforcingMesh>(MeshType, static m => m.PredefinedType.ToString()),
                    Token<IfcReinforcingMesh>(SteelGrade, static m => m.SteelGrade)),
                Seq(Measure<IfcReinforcingMesh>(MeshLength, Dimension.LengthDim, static m => m.MeshLength),
                    Measure<IfcReinforcingMesh>(MeshWidth, Dimension.LengthDim, static m => m.MeshWidth)) + MeshBars),
            // A post-tensioning tendon: its native NominalDiameter/CrossSectionArea/TensionForce are internal with no
            // public getter, so the diameter recovers through the same profile channel the mechanical fastener takes.
            [typeof(IfcTendon)] = Row<IfcTendon>(
                static _ => Some(Cast),
                Seq(Token<IfcTendon>(TendonType, static t => t.PredefinedType.ToString()),
                    Token<IfcTendon>(SteelGrade, static t => t.SteelGrade)),
                Seq(Measure<IfcTendon>(DetailSchema.NominalDiameter, Dimension.LengthDim, static t => DiameterOf(t).IfNone(double.NaN)))),
            [typeof(IfcTendonAnchor)] = Row<IfcTendonAnchor>(
                static _ => Some(Cast),
                Seq(Token<IfcTendonAnchor>(AnchorType, static a => a.PredefinedType.ToString()),
                    Token<IfcTendonAnchor>(SteelGrade, static a => a.SteelGrade)),
                Seq<(PropertyName, Dimension, Func<IfcTendonAnchor, double>)>()),
            // The post-tensioning DUCT (IfcTendonConduit, the IfcReinforcingElement sibling of the tendon it sheathes):
            // a cast-in realizing leaf on the same footing as the anchor, publishing its PredefinedType and the
            // reinforcing root's SteelGrade.
            [typeof(IfcTendonConduit)] = Row<IfcTendonConduit>(
                static _ => Some(Cast),
                Seq(Token<IfcTendonConduit>(ConduitType, static c => c.PredefinedType.ToString()),
                    Token<IfcTendonConduit>(SteelGrade, static c => c.SteelGrade)),
                Seq<(PropertyName, Dimension, Func<IfcTendonConduit, double>)>()),
            [typeof(IfcBearing)] = Row<IfcBearing>(
                static _ => Some(Bearing),
                Seq(Token<IfcBearing>(BearingType, static b => b.PredefinedType.ToString())),
                Seq<(PropertyName, Dimension, Func<IfcBearing, double>)>()),
            [typeof(IfcVibrationIsolator)] = Row<IfcVibrationIsolator>(
                static _ => Some(Bearing),
                Seq(Token<IfcVibrationIsolator>(IsolatorType, static i => i.PredefinedType.ToString())),
                Seq<(PropertyName, Dimension, Func<IfcVibrationIsolator, double>)>()),
            // The realizing COMPOSITE: an IfcElementAssembly is an IfcElement and so may sit in an
            // IfcRelConnectsWithRealizingElements — a bolted splice assembly, a bearing shoe assembly, a prefabricated
            // moment connection. Its detail is the fabrication fact the parts do not carry (the FACTORY/SITE assembly
            // place and the assembly type), and its joint modality is DERIVED from the parts that realize it rather
            // than declared, which is the one row whose Joint can answer None: an assembly aggregating no realizing
            // part realizes nothing this reader can describe.
            [typeof(IfcElementAssembly)] = Row<IfcElementAssembly>(
                AssemblyJoint,
                Seq(Token<IfcElementAssembly>(AssemblyType, static a => a.PredefinedType.ToString()),
                    Token<IfcElementAssembly>(AssemblyPlace, static a => a.AssemblyPlace.ToString())),
                Seq<(PropertyName, Dimension, Func<IfcElementAssembly, double>)>()),
        }.ToFrozenDictionary());

    // One realizing family's three columns, typed to IfcElement at the table and closed over its own case by Row<T> —
    // so a row body never casts and the table stays one flat dictionary over the whole family.
    sealed record RealizingRow(
        Func<IfcElement, Option<string>> Joint,
        Seq<(PropertyName Name, Func<IfcElement, Option<PropertyValue>> Read)> Values,
        Seq<(PropertyName Name, Dimension Dimension, Func<IfcElement, double> Read)> Measures);

    // The per-case row mint: the ONE place the element cast lives, arm-guaranteed because the table key IS the type.
    static RealizingRow Row<T>(
        Func<T, Option<string>> joint,
        Seq<(PropertyName Name, Func<T, Option<PropertyValue>> Read)> values,
        Seq<(PropertyName Name, Dimension Dimension, Func<T, double> Read)> measures)
        where T : IfcElement =>
        new(e => joint((T)e),
            values.Map(static v => (v.Name, (Func<IfcElement, Option<PropertyValue>>)(e => v.Read((T)e)))),
            measures.Map(static m => (m.Name, m.Dimension, (Func<IfcElement, double>)(e => m.Read((T)e)))));

    // A single text token (blank answers None and its row does not emit) and the multi-token variant the accessory's
    // co-realizing fastener SET takes: ONE token lands the same Text shape the authored Materials seed bag mints
    // (byte-identical content key), several land the typed PropertyValue.List the seam value family carries natively —
    // the ingest-only richer bag that is faithfully a DIFFERENT content-keyed node, never a .Head slice dropping a
    // nailplate+screw second fastener and never a joined literal.
    static (PropertyName Name, Func<T, Option<PropertyValue>> Read) Token<T>(PropertyName name, Func<T, string> read) =>
        (name, value => read(value) is { Length: > 0 } token && !string.IsNullOrWhiteSpace(token)
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

    // A realizing COMPOSITE takes the modality of the parts that realize it, ranked so the governing joint wins: a
    // truss with welded gussets and bolted splices is a welded assembly. Only NON-assembly parts contribute, which
    // both bounds the walk at one level and makes a malformed cyclic aggregate terminate by construction. An assembly
    // whose parts declare no modality answers None and details nothing rather than asserting a joint it cannot see.
    static Option<string> AssemblyJoint(IfcElementAssembly assembly) {
        Seq<string> parts = toSeq(assembly.IsDecomposedBy.AsIterable())
            .Bind(static rel => toSeq(rel.RelatedObjects.AsIterable()))
            .Choose(static part => part is IfcElement and not IfcElementAssembly
                && Realizing.Value.TryGetValue(part.GetType(), out RealizingRow? row) && row is { } claimed
                    ? claimed.Joint((IfcElement)part)
                    : Option<string>.None);
        return Ranked.Filter(parts.Contains).Head;
    }

    // ONE polymorphic realizing-detail reader over that table — never a BoltOf/WeldOf/CastOf/TendonOf/BearingOf
    // sibling family. Every row folds onto the seam DetailSchema.Realization.Bag() (the author's IDENTICAL schema,
    // never a hand-synced bag), so an authored bolt and an imported one content-key identically. The egress Filter
    // drops every non-finite Measure AND every blank Text token so an unset OPTIONAL IFC scalar (a NaN-default
    // reinforcing scalar, an unprofiled fastener's absent diameter, an undeclared SteelGrade "") never emits a NaN, a
    // misleading-0 measure, or an empty token; a row therefore lists every candidate column unconditionally rather
    // than branching per presence, and the Filter re-projects through the bag record `with` so the schema-stamped
    // SetName/precedence survive.
    public static Fin<Option<PropertyBag>> Detail(IfcElement realizing, UnitScale scale, Op key) =>
        Optional(Realizing.Value.GetValueOrDefault(realizing.GetType()))
            .Bind(row => row.Joint(realizing).Map(kind => (Row: row, Kind: kind)))
            .Match(
                None: static () => Fin.Succ(Option<PropertyBag>.None),
                Some: hit => Rows(Seq(Joint(hit.Kind, key))
                    + hit.Row.Values.Map(column => Lift(column.Name, column.Read(realizing)))
                    + hit.Row.Measures.Map(column => Measured(column.Name, column.Dimension, column.Read(realizing), scale))));

    // --- [ROWS]
    // The bag-row constructors so each realizing arm is a flat declarative row list rather than repeating the
    // Enumerated/Text/Measure/OfSi construction: the joint modality through the schema's JointType row, a text token, and an
    // SI measure over its Dimension. Rows inverts the candidate rows APPLICATIVELY (Traverse — the rows of one bag are
    // independent, so nothing licenses the monadic sequence) and folds them into the seam DetailSchema.Realization.Bag()
    // through ValueBag.With (last-write-wins). Joint composes DetailSchema.JointType + DetailSchema.Realization.Joint(kind)
    // (the PropertyValue.Enumerated over the schema's CLOSED allowed-set the egress facet validates against — never a local
    // Enumerated re-spelling the allowed set) and rails through the schema's OWN admission, because an out-of-set token
    // rejects at PropertyValue.Of rather than being trusted at this call site. Measured carries the DIMENSION-only
    // QuantityType — the seam bag law's two-peer carve, the catalogue-seeding Materials author holding no measure type
    // to name — so an imported and an authored NominalDiameter content-key identically, and it coerces its NATIVE
    // magnitude through the ONE per-projection UnitScale entry first. Token is TOTAL —
    // a blank value answers None — so it rides the pure lift and the two admissions above are the whole rail. An
    // ASSOCIATED material's grade rides the seam Material subgraph (Semantics/composition); only the reinforcing root's OWN
    // declared SteelGrade EXPRESS attribute lands here, as an ingest-only reader token.
    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Joint(string kind, Op key) =>
        DetailSchema.Realization.Joint(kind, key).Map(static value => Some((DetailSchema.JointType, value)));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Lift(PropertyName name, Option<PropertyValue> value) =>
        Fin.Succ(value.Map(read => (name, read)));

    static Fin<Option<(PropertyName Name, PropertyValue Value)>> Measured(PropertyName name, Dimension dim, double native, UnitScale scale) =>
        double.IsFinite(native)
            ? MeasureValue.OfSi(dim, scale.Coerce(native, MeasureRow.Of(dim), null))
                .Map(value => Some((name, (PropertyValue)new PropertyValue.Measure(value))))
            : Fin.Succ(Option<(PropertyName, PropertyValue)>.None);

    static Fin<Option<PropertyBag>> Rows(Seq<Fin<Option<(PropertyName Name, PropertyValue Value)>>> rows) =>
        rows.Traverse(identity).As()
            .Map(static values => Some(values.Choose(identity)
                .Fold(DetailSchema.Realization.Bag(), static (bag, row) => bag.With(row.Name, row.Value))));

    // --- [PROFILE_DIAMETER]
    // The fastener/tendon nominal diameter rides its cross-section profile radius: mNominalDiameter/mNominalLength are
    // internal on the occurrence AND on IfcMechanicalFastenerType (and the tendon's diameter likewise internal), with no
    // public getter on either, so there is no type-edge fallback and the diameter is recovered through the inherited
    // HasAssociations IfcRelAssociatesMaterial.RelatingMaterial (the IfcMaterialProfileSetUsage's ForProfileSet OR the
    // bare IfcMaterialProfileSet a type-driven export associates without the occurrence usage — both IfcMaterialSelect
    // arms -> MaterialProfiles -> Profile -> IfcCircleProfileDef.Radius x 2), the same chain the profile-hosted
    // IfcMechanicalFastener(IfcProduct, IfcMaterialProfileSetUsage, IfcAxis2Placement3D, double) authoring ctor binds.
    // The finiteness guard rides the Choose so the head is the first circle with a FINITE radius (a degenerate
    // NaN-radius profile never masks a later valid one); None when no circle profile binds a diameter.
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
    // A fabricated framing connector (IfcDiscreteAccessory, the IfcElementComponent sibling of IfcMechanicalFastener):
    // a steel saddle/bracket/anchorplate that physically IS the connector body, FASTENED BY separate
    // IfcMechanicalFasteners. This is the IMPORT counterpart of the Rasm.Materials
    // Component/connector#CONNECTOR_FAMILY seed — the ConnectorDetail seed-time bag denormalizes BOTH tokens onto the
    // one connector Component (ComponentProjector-projected): AccessoryType = the connector's own
    // IfcDiscreteAccessoryTypeEnum SHOE/BRACKET/ANCHORPLATE, FastenerType = the SEPARATE attaching token the row's
    // ConnectorInstall FastenerSpec.IfcFastenerType carries (NAIL/SCREW/BOLT) — so the Bim reader matches that SHAPE
    // through its two token columns, the second recovering the fastener SET from the co-realizing
    // IfcMechanicalFastener siblings reached through the IfcElement.IsConnectionRealization back-pointer (the
    // IfcRelConnectsWithRealizingElements sets both the accessory and its attaching fasteners join). One bounded hop
    // reading only PredefinedType — the joint TOPOLOGY rides the EdgeProjection Connect edge whose Connect.Realizing
    // field carries the realizing node, never re-derived here. ALL co-realizing fastener tokens fold, distinct then
    // ORDINAL-sorted so duplicates collapse and an IFC file's SET order never forks the content key; an accessory with
    // NO co-realizing mechanical fastener answers the empty Seq and its row does not emit — never a fabricated token.
    static Seq<string> FastenerOf(IfcDiscreteAccessory accessory) =>
        toSeq(accessory.IsConnectionRealization.AsIterable()
            .SelectMany(static rel => rel.RealizingElements.AsIterable())
            .Choose(static realizing => realizing is IfcMechanicalFastener fastener ? Some(fastener.PredefinedType.ToString()) : None)
            .Distinct().OrderBy(static token => token, StringComparer.Ordinal));

    // --- [MESH_AXES]
    // The mesh's bar rows are a CROSS-PRODUCT — {longitudinal, transverse} x {nominal diameter, spacing, cross-section
    // area} — not six independent facts, so the axis fold mints BOTH the PropertyName and the paired GG reader: a third
    // measured column is one tuple in Family's body and a third bar axis one Bars row, where the enumerated form cost six
    // more statics and six more Measured lines that could drift apart. The axis reader answers the triple in column order,
    // so the fold zips one row per column and the name is derived from the axis prefix, never re-spelled.
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
    // The realizing-detail row names the IMPORT reader recovers BEYOND the canonical DetailSchema vocabulary (the author
    // writes the canonical discrete-part diameter/throat/lap schema statics; this reader composes the
    // import-recoverable subset — JointType/FastenerType/AccessoryType/BarType/NominalDiameter/CrossSectionArea — the
    // authored-only throat/lap/carried-member rows having no public GG read channel; panel/deck/membrane product rows
    // are DetailSchema.Product's and the general property fold's). Every row below mints through
    // PropertyCategory.Seam.Row — the owner-blessed EMPTY-prefix producer category, so custody routes through the seam
    // declarer while the round-tripped IFC property name stays bare — and a call-site PropertyName.Create here is the
    // deleted form that forks the bag's key space between non-referencing packages. PropertyName itself stays an OPEN
    // key per Properties/property#DETAIL_SCHEMA, so an ingest-only scalar the Materials author never mints — a mesh
    // sheet's geometry, a tendon/anchor/bearing/isolator type token, a bar's surface + overall length, the reinforcing
    // root's declared SteelGrade designation (the superseded-but-live IfcReinforcingElement EXPRESS attribute, public
    // get/set — the ONLY grade carrier when an older export binds no material) — lands here, and the moment a second
    // package keys on one it is PROMOTED to a DetailSchema static at the Rasm.Element owner rather than staying here.
    // An authored bag carrying only the schema rows and an imported bag carrying these extra rows are faithfully DIFFERENT
    // content-keyed nodes, never a forced byte-match; these never widen the seam DetailSchema (a Materials-authored row is a
    // schema static, a Bim-ingest-only row is here), and the grade/carbon data of an ASSOCIATED material still rides the
    // seam Material subgraph, never here. The mesh bar-axis rows mint through the SAME category inside Family.
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
    static readonly PropertyName ConduitType = PropertyCategory.Seam.Row("ConduitType");
    static readonly PropertyName AssemblyType = PropertyCategory.Seam.Row("AssemblyType");
    static readonly PropertyName AssemblyPlace = PropertyCategory.Seam.Row("AssemblyPlace");
}
```

## [03]-[RESEARCH]

(none)
